using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class SubscriptionSplitter
    {
        readonly SubscriptionAggregator _psubscriptions;
        readonly SubscriptionAggregator _subscriptions;

        internal Int32 KeyCount { get { return _psubscriptions.KeyCount + _subscriptions.KeyCount; } }
        internal Int32 ChannelCount { get { return _psubscriptions.ChannelCount + _subscriptions.ChannelCount; } }

        internal SubscriptionSplitter()
        {
            _psubscriptions = new SubscriptionAggregator();
            _subscriptions = new SubscriptionAggregator();
        }

        private Boolean IsSubscriptionMessage(String notificationHeader)
        {
            return "MESSAGE".Equals(notificationHeader, StringComparison.Ordinal) ||
                   "PMESSAGE".Equals(notificationHeader, StringComparison.Ordinal);
        }

        private RESPCommand BuildResponse(String header, IEnumerable<String> keys)
        {
            if (keys.Count() == 0)
                this.ToString();

            var response = new RESPCommand(new RESPCommandLiteral(header), true);
            foreach (var key in keys)
            {
                response.Add(new RESPCommandLiteral(key));
            }
            return response;
        }

        internal SubscriptionResponsesTracker Aggregate(IRedisChannel channel, IEnumerable<RESPCommand> commands)
        {
            Contract.Assert(commands.Any(), "Trying to track an empty list of commands.");

            var data = new SubscriptionResponsesTracker();

            foreach (var command in commands)
            {
                var operation = command.Header.ToUpperInvariant();
                var keys = command.Skip(1).Cast<RESPCommandPart>().Select(x => x.Value).ToList();

                if (IsSubscriptionMessage(operation))
                    throw new InvalidOperationException("Redis Messages are not subscription control operations.");

                GenerateTracker(channel, data, operation, keys);
            }

            return data;
        }

        private void GenerateTracker(IRedisChannel channel, SubscriptionResponsesTracker data, String operation, IList<string> keys)
        {
            switch (operation)
            {
                case "SUBSCRIBE":
                    Include(operation, _subscriptions.Subscribe(channel, keys), data.AddSubscription, data.AddCommand);
                    break;

                case "PSUBSCRIBE":
                    Include(operation, _psubscriptions.Subscribe(channel, keys), data.AddSubscription, data.AddCommand);
                    break;

                case "UNSUBSCRIBE":
                    Include(operation, _subscriptions.Unsubscribe(channel, keys), data.AddUnsubscription, data.AddCommand);
                    break;

                case "PUNSUBSCRIBE":
                    Include(operation, _psubscriptions.Unsubscribe(channel, keys), data.AddUnsubscription, data.AddCommand);
                    break;

                default: throw new InvalidOperationException("Unrecognized subscription control operation: " + operation);
            }
        }

        private void Include(String operation, IList<String> result, Action<String> add, Action<RESPCommand> addCommand)
        {
            if (result.Any())
            {
                foreach (var item in result)
                    add(item);

                addCommand(BuildResponse(operation, result));
            }
        }

        internal IEnumerable<IRedisChannel> GetSubscribedTo(RESPArray message)
        {
            Contract.Assert(message.Any(), "Empty RESPArray");

            var operation = message.ElementAt<RESPBulkString>(0).Value.ToUpperInvariant();

            if (!IsSubscriptionMessage(operation))
                return Enumerable.Empty<IRedisChannel>();

            var aggregator = operation.Equals("MESSAGE", StringComparison.Ordinal) ? _subscriptions : _psubscriptions;
            return aggregator.GetChannels(message.ElementAt<RESPBulkString>(1).Value);
        }

        internal SubscriptionResponsesTracker RemoveChannel(IRedisChannel channel)
        {
            SubscriptionResponsesTracker tracker = new SubscriptionResponsesTracker();

            var keys = _subscriptions.GetSubscriptions(channel).ToList();
            if(keys.Any())
                GenerateTracker(channel, tracker, "UNSUBSCRIBE", keys);

            keys = _psubscriptions.GetSubscriptions(channel).ToList();
            if(keys.Any())
                GenerateTracker(channel, tracker, "PUNSUBSCRIBE", keys);

            return tracker;
        }

        internal IEnumerable<RESPCommand> GetAllSubscribeCommands()
        {
            var responses = new List<RESPCommand>();

            var keys = _subscriptions.GetSubscriptions();
            if (keys.Any())
                responses.Add(BuildResponse("SUBSCRIBE", keys));

            keys = _psubscriptions.GetSubscriptions();
            if (keys.Any())
                responses.Add(BuildResponse("PSUBSCRIBE", keys));

            return responses;
        }

        internal IEnumerable<IRedisChannel> GetChannels()
        {
            var channels = new HashSet<IRedisChannel>(_subscriptions.GetChannels());
            foreach (var channel in _psubscriptions.GetChannels())
                channels.Add(channel);

            return channels;
        }

        internal Boolean IsMessage(RESPObject response, out RESPArray message)
        {
            message = null;

            if (response.Header != RESPHeaders.Array)
                return false;

            message = response.Cast<RESPArray>();

            return IsSubscriptionMessage(message.ElementAt<RESPBulkString>(0).Value.ToUpperInvariant());
        }
    }
}

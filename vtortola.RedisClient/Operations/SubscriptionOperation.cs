using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class SubscriptionOperation : ISubscriptionOperation
    {
        readonly SubscriptionSplitter _subscriptions;
        readonly IRedisChannel _channel;
        readonly RESPCommand[] _commands;
        readonly RESPObject[] _responses;

        SubscriptionResponsesTracker _tracker;

        public Boolean IsCompleted { get { return _tracker != null && _tracker.IsCompleted; } }

        public SubscriptionOperation(IRedisChannel channel, RESPCommand[] commands, RESPObject[] responses, SubscriptionSplitter subscriptions)
        {
            Contract.Assert(commands.Length == responses.Length, "The number of commands is different than the responses placeholder.");

            _subscriptions = subscriptions;
            _channel = channel;
            _commands = commands;
            _responses = responses;

            for (int i = 0; i < _commands.Length; i++)
            {
                if (_commands[i].IsSubscription)
                    _responses[i] = RESPSimpleString.OK;
            }
        }

        public IEnumerable<RESPCommand> Execute()
        {
            _tracker = _subscriptions.Aggregate(_channel, _commands.Where((c, i) => _commands[i].IsSubscription));
            return _tracker.GetCommands();
        }

        public void HandleResponse(RESPObject response)
        {
            Contract.Assert(_tracker != null, "Subscription tracker is not set when calling RemoveChannelOperation.HandleResponse");
            Contract.Assert(response != null && response is RESPArray, "SubscriptionOperation cannot handle response: " + (response==null?"Null":response.GetType().Name));

            _tracker.Acknowledge((RESPArray)response);
        }
    }
}

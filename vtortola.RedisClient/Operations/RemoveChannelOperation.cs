using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class RemoveChannelOperation : ISubscriptionOperation
    {
        readonly SubscriptionSplitter _subscriptions;
        readonly IRedisChannel _channel;

        SubscriptionResponsesTracker _tracker;

        public Boolean IsCompleted { get; private set; }

        public RemoveChannelOperation(IRedisChannel channel, SubscriptionSplitter subscriptions)
        {
            _subscriptions = subscriptions;
            _channel = channel;
        }

        public IEnumerable<RESPCommand> Execute()
        {
            _tracker = _subscriptions.RemoveChannel(_channel);
            if (_tracker == null)
                yield break;

            foreach (var cmd in _tracker.GetCommands())
                yield return cmd;
        }

        public void HandleResponse(RESPObject response)
        {
            Contract.Assert(_tracker != null, "Subscription tracker is not set when calling RemoveChannelOperation.HandleResponse");

            var array = response.Cast<RESPArray>();
            _tracker.Acknowledge(array);
            IsCompleted = _tracker.IsCompleted;
        }
    }
}

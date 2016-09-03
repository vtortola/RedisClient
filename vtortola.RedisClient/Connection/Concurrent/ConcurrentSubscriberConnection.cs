using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal sealed class ConcurrentSubscriberConnection : ConcurrentConnection, ISubscriptionConnection
    {
        static readonly RedisNotification _connectionMessage = new RedisNotification("connected", "connected", "connected");

        readonly SubscriptionSplitter _subscriptions;
        readonly RedisClientOptions _options;

        ExecutionToken _current;

        public SubscriptionSplitter Subscriptions { get { return _subscriptions; } } 
        
        public override Int32 CurrentLoad { get { return _subscriptions.KeyCount * LoadFactor; } }

        internal ConcurrentSubscriberConnection(IPEndPoint[] endpoints, RedisClientOptions options)
            :base(endpoints, options)
        {
            _subscriptions = new SubscriptionSplitter();
            _options = options;

            Initializers.Add(new SubscriptionsInitialization(_subscriptions));
        }

        protected override void OnConnection()
        {
            base.OnConnection();
            _current = null;
            foreach (var channel in Subscriptions.GetChannels().Cast<RedisChannel>())
                channel.PushMessage(_connectionMessage);
        }

        protected override IEnumerable<RESPCommand> ExecuteOperation(ExecutionToken token, CancellationToken cancel)
        {
            return token.SubscriptionOperation.Execute();
        }

        protected internal override ExecutionToken GeneratePingToken()
        {
            return new NoWaitExecutionToken(null, new PingSubscriberOperation());
        }

        protected override void ProcessResponse(RESPObject response, ConcurrentQueue<ExecutionToken> pending, CancellationToken cancel)
        {
            if (response == null)
            {
                if (_current != null)
                {
                    _current.SetCancelled();
                    _current = null;
                }

                return;
            }

            RESPArray message;
            if (Subscriptions.IsMessage(response, out message))
            {
                PushMessage(message, cancel);
            }
            else
            {
                if (_current == null)
                {
                    if (!pending.TryDequeue(out _current))
                        return;
                }

                Contract.Assert(_current.SubscriptionOperation != null, "A token without subscription reached the subscriber connection response handler.");

                if (!_current.SubscriptionOperation.IsCompleted)
                    TokenHandling.ProcessToken(_current, (t, c) => _current.SubscriptionOperation.HandleResponse(response), cancel);

                if (_current.SubscriptionOperation.IsCompleted)
                {
                    _current.SetCompleted();
                    _current = null;
                }
            }
        }

        private void PushMessage(RESPArray message, CancellationToken cancel)
        {
            var subscribers = Subscriptions.GetSubscribedTo(message);
            var notification = RedisNotification.ParseArray(message);

            foreach (var channel in subscribers.Cast<RedisChannel>())
                Task.Run(() => channel.PushMessage(notification));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal sealed class RedisSubscriberConnection : RedisConnection, ISubscriptionConnection
    {
        static readonly RedisNotification _connectionMessage = new RedisNotification("connected", "connected", "connected");
                
        readonly SubscriptionSplitter _subscriptions;

        ExecutionToken _current;
        
        public SubscriptionSplitter Subscriptions { get { return _subscriptions; } }

        internal RedisSubscriberConnection(IPEndPoint[] endpoints, RedisClientOptions options)
            :base(endpoints, options)
        {
            _subscriptions = new SubscriptionSplitter();
            Initializers.Add(new SubscriptionsInitialization(_subscriptions));
        }

        protected override void OnConnection()
        {
            foreach (var channel in Subscriptions.GetChannels().Cast<RedisChannel>())
                channel.PushMessage(_connectionMessage);
        }

        protected override void OnDisconnection()
        {
            if (_current != null)
            {
                _current.SetCancelled();
                _current = null;
            }
        }

        protected internal override ExecutionToken GeneratePingToken()
        {
            return new NoWaitExecutionToken(null, new PingSubscriberOperation());
        }

        protected override void ProcessResponse(RESPObject response)
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
                PushMessage(message);
            }
            else
            {
                if (_current == null && !_pending.TryDequeue(out _current))
                    throw new InvalidOperationException("Received command response but no token available.");

                try
                {
                    HandleResponseWithToken(response);
                }
                catch (OperationCanceledException)
                {
                    _current.SetCancelled();
                    throw;
                }
                catch (Exception ex)
                {
                    _current.SetFaulted(ex);
                    throw;
                }
            }
        }

        private void HandleResponseWithToken(RESPObject response)
        {
            Contract.Assert(_current.SubscriptionOperation != null, "A token without subscription reached the subscriber connection response handler.");

            if (!_current.SubscriptionOperation.IsCompleted)
                _current.SubscriptionOperation.HandleResponse(response);

            if (_current.SubscriptionOperation.IsCompleted)
            {
                _current.SetCompleted();
                _current = null;
            }
        }

        protected override IEnumerable<RESPCommand> ExecuteOperation(ExecutionToken token)
        {
            return token.SubscriptionOperation.Execute();
        }

        private void PushMessage(RESPArray message)
        {
            var subscribers = Subscriptions.GetSubscribedTo(message);
            var notification = RedisNotification.ParseArray(message);

            foreach (var channel in subscribers.Cast<RedisChannel>())
                Task.Run(() => channel.PushMessage(notification));
        }
    }
}

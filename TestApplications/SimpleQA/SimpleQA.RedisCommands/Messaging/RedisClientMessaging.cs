using SimpleQA.Messaging;
using SimpleQA.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class RedisClientMessaging : IMessaging
    {
        readonly IRedisChannel _channel;
        readonly BufferBlock<RedisNotification> _notifications;

        SimpleQAPrincipal _user;

        public void Init(SimpleQAPrincipal user)
        {
            _user = user;
            CheckInit();
        }

        private void CheckInit()
        {
            if (_user == null)
                throw new InvalidOperationException("'RedisClientMessaging' needs to be initialized with a valid user.");
        }

        public RedisClientMessaging(IRedisChannel channel)
        {
            _channel = channel;
            _notifications = new BufferBlock<RedisNotification>();
            _channel.NotificationHandler = n => _notifications.Post(n);
        }

        public async Task<PushMessage> ReceiveMessage(CancellationToken cancel)
        {
            CheckInit();
            var notification = await _notifications.ReceiveAsync(cancel).ConfigureAwait(false);
            var topic = GetApplicationTopic(notification.PublishedKey);
            return new PushMessage() { Topic = topic, Change = notification.Content };
        }

        public Task SendMessageAsync(PushSubscriptionRequest request, CancellationToken cancel)
        {
            CheckInit();
            var topic = GetRedisTopic(request.Topic);
            _channel.Dispatch("subscribe @topic", new { topic });
           return Task.FromResult<Object>(null);
        }

        private String GetRedisTopic(String topic)
        {
            if(topic.StartsWith("user-"))
                return topic + "-" + _user.SimpleQAIdentity.Id;

            return topic;
        }

        private String GetApplicationTopic(String topic)
        {
            if (topic.StartsWith("user-"))
            {
                var removeLength = _user.SimpleQAIdentity.Id.Length + 1;
                return topic.Substring(0, topic.Length - removeLength);
            }

            return topic;
        }

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}

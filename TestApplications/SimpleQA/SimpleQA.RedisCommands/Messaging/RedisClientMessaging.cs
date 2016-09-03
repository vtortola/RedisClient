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

        public RedisClientMessaging(IRedisChannel channel)
        {
            _channel = channel;
            _notifications = new BufferBlock<RedisNotification>();
            _channel.NotificationHandler = n => _notifications.Post(n);
        }

        public async Task<PushMessage> ReceiveMessage(CancellationToken cancel)
        {
            var notification = await _notifications.ReceiveAsync(cancel).ConfigureAwait(false);
            return new PushMessage() { Topic = notification.PublishedKey, Change = notification.Content };
        }

        public Task SendMessageAsync(PushSubscriptionRequest request, CancellationToken cancel)
        {
           _channel.Dispatch("subscribe @key", new { key = request.Topic });
           return Task.FromResult<Object>(null);
        }

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}

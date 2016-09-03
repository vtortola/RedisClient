using System;

namespace vtortola.Redis
{
    /// <summary>
    /// A push notification from Redis.
    /// </summary>
    public sealed class RedisNotification
    {
        /// <summary>
        /// Gets the header. Its value is MESSAGE or PMESSAGE.
        /// </summary>
        public String Header { get; private set; }

        /// <summary>
        /// Gets the subscribed key. If using PSUBSCRIBE, it will return the pattern that generated the message.
        /// </summary>
        public String SubscribedKey { get; private set; }

        /// <summary>
        /// The value that has caused this notification to be triggered. I case of MESSAGE, it will be the same than SubscribedKey.
        /// </summary>
        public String PublishedKey { get; private set; }

        /// <summary>
        /// Gets the message content.
        /// </summary>
        public String Content { get; private set; }
        
        internal RedisNotification(String header)
        {
            ParameterGuard.CannotBeNullOrEmpty(header, "header");

            this.Header = header;
        }

        internal RedisNotification(String header, String subscribedKey, String publishedKey, String content)
            : this(header)
        {
            ParameterGuard.CannotBeNullOrEmpty(subscribedKey, "subscribedKey");
            ParameterGuard.CannotBeNullOrEmpty(publishedKey, "publishedKey");
            ParameterGuard.CannotBeNullOrEmpty(content, "content");

            this.SubscribedKey = subscribedKey;
            this.PublishedKey = publishedKey;
            this.Content = content;
        }

        internal RedisNotification(String header, String key, String content)
            :this(header,key, key, content)
        {
        }

        internal static RedisNotification ParseArray(RESPArray array)
        {
            var header = array.ElementAt<RESPBulkString>(0).Value.ToUpperInvariant();
            if (header.Equals("PMESSAGE", StringComparison.Ordinal))
                return new RedisNotification(header, array.ElementAt<RESPBulkString>(1).Value, array.ElementAt<RESPBulkString>(2).Value, array.ElementAt<RESPBulkString>(3).Value);
            else if (header.Equals("MESSAGE", StringComparison.Ordinal))
                return new RedisNotification(header, array.ElementAt<RESPBulkString>(1).Value, array.ElementAt<RESPBulkString>(2).Value);
            else
                return new RedisNotification(header);
        }
    }
}

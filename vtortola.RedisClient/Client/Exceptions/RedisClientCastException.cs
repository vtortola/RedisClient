using System;

namespace vtortola.Redis
{
    /// <summary>
    /// Failed casting a Redis result to another type.
    /// </summary>
    /// <seealso cref="vtortola.Redis.RedisClientException" />
    public sealed class RedisClientCastException : RedisClientException
    {
        internal RedisClientCastException(String message)
            : base(message)
        {

        }

        internal RedisClientCastException(String message, Exception inner)
            : base(message, inner)
        {

        }
    }
}

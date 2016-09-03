using System;

namespace vtortola.Redis
{
    /// <summary>
    /// Failed during binding the result of a Redis command to a object or container.
    /// </summary>
    /// <seealso cref="vtortola.Redis.RedisClientException" />
    public sealed class RedisClientBindingException : RedisClientException
    {
        internal RedisClientBindingException(String message)
            : base(message)
        {

        }

        internal RedisClientBindingException(String message, Exception inner)
            : base(message, inner)
        {

        }
    }
}

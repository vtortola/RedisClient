using System;

namespace vtortola.Redis
{
    /// <summary>
    /// Result assertion failed.
    /// </summary>
    /// <seealso cref="vtortola.Redis.RedisClientException" />
    public sealed class RedisClientAssertException : RedisClientException
    {
        internal RedisClientAssertException(String message)
            : base(message)
        {

        }

        internal RedisClientAssertException(String message, Exception inner)
            : base(message, inner)
        {

        }
    }
}

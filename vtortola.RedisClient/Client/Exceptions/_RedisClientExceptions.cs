using System;

namespace vtortola.Redis
{
    /// <summary>
    /// Base exception class for <see cref="RedisClient"/>
    /// </summary>
    /// <seealso cref="System.Exception" />
    public abstract class RedisClientException : Exception
    {
        internal RedisClientException(String message)
            : base(message)
        {

        }

        internal RedisClientException(String message, Exception inner)
            : base(message, inner)
        {

        }
    }
}

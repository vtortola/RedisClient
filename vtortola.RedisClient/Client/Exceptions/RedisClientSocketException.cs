using System;

namespace vtortola.Redis
{
    /// <summary>
    /// Socket related error in <see cref="RedisClient"/>.
    /// </summary>
    /// <seealso cref="vtortola.Redis.RedisClientException" />
    public sealed class RedisClientSocketException : RedisClientException
    {
        internal RedisClientSocketException(String message)
            : base(message)
        {

        }

        internal RedisClientSocketException(String message, Exception inner)
            : base(message, inner)
        {

        }
    }
}

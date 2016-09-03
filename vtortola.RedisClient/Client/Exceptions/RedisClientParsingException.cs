using System;

namespace vtortola.Redis
{
    /// <summary>
    /// <see cref="RedisClient"/> failed to parse the give command.
    /// </summary>
    /// <seealso cref="vtortola.Redis.RedisClientException" />
    public sealed class RedisClientParsingException : RedisClientException
    {
        internal RedisClientParsingException(String message)
            : base(message)
        {

        }

        internal RedisClientParsingException(String message, Exception inner)
            : base(message, inner)
        {

        }
    }
}

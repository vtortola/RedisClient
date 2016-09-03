using System;

namespace vtortola.Redis
{
    /// <summary>
    /// <see cref="RedisClient"/> failed to parse the given procedure.
    /// </summary>
    /// <seealso cref="vtortola.Redis.RedisClientException" />
    public sealed class RedisClientProcedureParsingException : RedisClientException
    {
        internal RedisClientProcedureParsingException(String message)
            : base(message)
        {

        }

        internal RedisClientProcedureParsingException(String message, Exception inner)
            : base(message, inner)
        {

        }
    }
}

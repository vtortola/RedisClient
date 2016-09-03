using System;

namespace vtortola.Redis
{
    /// <summary>
    /// The <see cref="RedisClientOptions"/> used in <see cref="RedisClient"/> contains a non valid configuration.
    /// </summary>
    /// <seealso cref="vtortola.Redis.RedisClientException" />
    public sealed class RedisClientConfigurationException : RedisClientException
    {
        internal RedisClientConfigurationException(String message)
            : base(message)
        {

        }

        internal RedisClientConfigurationException(String message, Exception inner)
            : base(message, inner)
        {

        }
    }
}

using System;

namespace vtortola.Redis
{
    /// <summary>
    /// Aggregate of <see cref="RedisClientCommandException"/>
    /// </summary>
    /// <seealso cref="vtortola.Redis.RedisClientException" />
    public sealed class RedisClientMultipleCommandException : RedisClientException
    {
        /// <summary>
        /// Gets the exceptions.
        /// </summary>
        public RedisClientCommandException[] InnerExceptions { get; private set; }
        internal RedisClientMultipleCommandException(params RedisClientCommandException[] commandExceptions)
            : base("Multiple Redis command failed, please check 'InnerExceptions' property.", new AggregateException(commandExceptions))
        {
            ParameterGuard.CannotBeNullOrEmpty(commandExceptions, "commandExceptions");
            this.InnerExceptions = commandExceptions;
        }

        internal RedisClientMultipleCommandException(RESPError error, params RedisClientCommandException[] commandExceptions)
            : base(String.Format("{0}:'{1}'", error.Prefix, error.Message), new AggregateException(commandExceptions))
        {
            ParameterGuard.CannotBeNullOrEmpty(commandExceptions, "commandExceptions");
            this.InnerExceptions = commandExceptions;
        }
    }

}

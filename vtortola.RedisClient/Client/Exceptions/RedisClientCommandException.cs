using System;

namespace vtortola.Redis
{
    /// <summary>
    /// Error returned by Redis during the execution of the command.
    /// </summary>
    /// <seealso cref="vtortola.Redis.RedisClientException" />
    public sealed class RedisClientCommandException : RedisClientException
    {
        /// <summary>
        /// Line number of the failed command (Starts by 1).
        /// </summary>
        public Int32 LineNumber { get; private set; }

        /// <summary>
        /// Redis error prefix.
        /// </summary>
        public String Prefix { get; private set; }

        /// <summary>
        /// Redis error message.
        /// </summary>
        public String OriginalMessage { get; private set; }

        internal RedisClientCommandException(RESPError error, Int32 lineNumber)
            : base(String.Format("{0}:'{1}' in line #{2}", error.Prefix, error.Message, lineNumber))
        {
            ParameterGuard.CannotBeZeroOrNegative(lineNumber, "lineNumber");

            LineNumber = lineNumber;
            Prefix = error.Prefix;
            OriginalMessage = error.Message;
        }

        internal RedisClientCommandException(RESPError error)
            : this(error, 1)
        {

        }
    }
}

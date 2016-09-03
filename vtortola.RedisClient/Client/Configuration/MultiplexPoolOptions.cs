using System;

namespace vtortola.Redis
{
    /// <summary>
    /// Multiplexed pool options.
    /// </summary>
    public sealed class MultiplexPoolOptions
    {
        /// <summary>
        /// Number of command connections.
        /// </summary>
        public Int32 CommandConnection { get; set; }

        /// <summary>
        /// Number of subscription connections.
        /// </summary>
        public Int32 SubscriptionOptions { get; set; }
    }
}

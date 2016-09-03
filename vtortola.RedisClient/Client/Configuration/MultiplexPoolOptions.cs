using System;

namespace vtortola.Redis
{
    /// <summary>
    /// Multiplexed pool options.
    /// </summary>
    public sealed class MultiplexPoolOptions
    {
        /// <summary>
        /// Number of commander connections.
        /// </summary>
        public Int32 CommandConnections { get; set; }

        /// <summary>
        /// Number of subscriber connections.
        /// </summary>
        public Int32 SubscriptionOptions { get; set; }
    }
}

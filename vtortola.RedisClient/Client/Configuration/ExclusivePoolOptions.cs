using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    /// <summary>
    /// Exclusive connection pool configuration.
    /// </summary>
    public sealed class ExclusivePoolOptions
    {
        /// <summary>
        /// Minimum number of command connections.
        /// </summary>
        public Int32 Minimum { get; set; }

        /// <summary>
        /// Maximum number of command connections.
        /// </summary>
        public Int32 Maximum { get; set; }
    }
}

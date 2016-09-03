using System;
using System.Collections.Generic;

namespace vtortola.Redis
{
    /// <summary>
    /// Result of a multiple command statement.
    /// </summary>
    public interface IRedisResults : IEnumerable<IRedisResultInspector>
    {
        /// <summary>
        /// Gets the <see cref="IRedisResultInspector"/> at the specified index.
        /// </summary>
        IRedisResultInspector this[Int32 index] { get; }

        /// <summary>
        /// Gets the <see cref="IRedisResultInspector"/> count.
        /// </summary>
        Int32 Count { get; }

        /// <summary>
        /// Throws an exception if any of the <see cref="IRedisResultInspector"/> contains one.
        /// </summary>
        void ThrowErrorIfAny();
    }
}

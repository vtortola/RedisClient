using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;

namespace vtortola.Redis
{
    internal sealed class CachingExecutionPlanner : IExecutionPlanner
    {
        readonly ConcurrentDictionary<String, ExecutionPlan> _cache;
        readonly IExecutionPlanner _inner;

        internal CachingExecutionPlanner(IExecutionPlanner inner)
        {
            _inner = inner;
            _cache = new ConcurrentDictionary<String, ExecutionPlan>();
        }

        public ExecutionPlan Build(String command)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(command), "Calling to build command with an empty string.");

            return _cache.GetOrAdd(command, cmd => _inner.Build(cmd));
        }
    }
}

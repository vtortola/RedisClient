using System;
using System.Net;
using System.Threading;

namespace vtortola.Redis
{
    internal abstract class Connection : ConnectionBase
    {
        readonly Object _locker;

        internal Connection(IPEndPoint[] endpoints, RedisClientOptions options)
            :base(endpoints, options)
        {
            _locker = new Object();
        }

        protected override void ExecuteToken(ExecutionToken token, CancellationToken cancel)
        {
            // Ping can be executed at the same time that a user command.
            // Howerver, chances of this happening are very small. 'lock' expense is 
            // small if there is no contention.
            // ConcurrentConnection overrides this method in another way using producer/consumer            
            lock (_locker)
                base.ExecuteToken(token, cancel);
        }
    }
}

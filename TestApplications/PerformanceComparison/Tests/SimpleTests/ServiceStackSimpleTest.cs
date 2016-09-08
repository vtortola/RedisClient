using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceComparison.Tests.SimpleTests
{
    public class ServiceStackSimpleTest : ITest
    {
        IRedisClientsManager _pool;
        public Task Init(IPEndPoint endpoint, System.Threading.CancellationToken cancel)
        {
            _pool = new PooledRedisClientManager(100, 60, endpoint.ToString());
            using (var client = _pool.GetCacheClient())
            {
                client.FlushAll();
                client.Increment("whatever", 1);
            }
            return Task.FromResult((Object)null);
        }

        public Task RunClient(Int32 id, System.Threading.CancellationToken cancel)
        {
            var key = id.ToString();
            using (var client = _pool.GetCacheClient())
                client.Increment(key, 1); 

            return Task.FromResult<Object>(null);
        }

        public void ClearData()
        {
            using (var client = _pool.GetClient())
                client.FlushAll();
            _pool.Dispose();
        }
    }
}

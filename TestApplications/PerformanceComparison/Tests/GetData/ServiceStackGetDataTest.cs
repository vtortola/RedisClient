using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerformanceComparison.Tests.GetData
{
    public class ServiceStackGetDataTest : ITest
    {
        IRedisClientsManager _pool;
        public Task Init(IPEndPoint endpoint, System.Threading.CancellationToken cancel)
        {
            _pool = new PooledRedisClientManager(100, 60, endpoint.ToString());
            using (var c = _pool.GetClient())
            {
                c.FlushAll();
                c.AddRangeToSet("testkeySSE", Enumerable.Range(0, 10).Select(i => i.ToString("0000000000")).ToList());
            }
            return Task.FromResult((Object)null);
        }

        public Task RunClient(Int32 id, CancellationToken cancel)
        {
            using (var client = _pool.GetClient())
            {
                var hash = client.GetAllItemsFromSet("testkeySSE");
                return Task.FromResult<Object>(null);
            }
        }

        public void ClearData()
        {
            using(var client = _pool.GetClient())
                client.FlushAll();
        }
    }
}

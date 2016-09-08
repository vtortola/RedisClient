using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceComparison.Tests.SimpleTests
{
    public class ServiceStackSimpleTest:SimpleTest
    {
        IRedisClientsManager _pool;
        public override Task Init(IPEndPoint endpoint, System.Threading.CancellationToken cancel)
        {
            _pool = new PooledRedisClientManager(50, 10, endpoint.ToString());
            using (var client = _pool.GetClient())
            {
                client.FlushAll();
                client.IncrementValue("whatever");
            }
            return Task.FromResult((Object)null);
        }

        public override Task<Int64> RunClient(Int32 id, System.Threading.CancellationToken cancel)
        {
            var key = this.GetType().Name + "_" + id;
            using (var client = _pool.GetClient())
            {
                for (int i = 0; i < Iterations; i++)
                {
                    client.IncrementValue(key);
                }

                return Task.FromResult(client.Get<Int64>(key));
            }
        }

        public override void ClearData()
        {
            using (var client = _pool.GetClient())
                client.FlushAll();
            _pool.Dispose();
        }
    }
}

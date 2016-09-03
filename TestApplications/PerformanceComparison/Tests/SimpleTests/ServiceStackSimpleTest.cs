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
                var values = new[] { new KeyValuePair<String, String>("Member1", "Value1"), new KeyValuePair<String, String>("Member2", "Value2") };

                for (int i = 0; i < Iterations; i++)
                {
                    client.IncrementValue(key + "1");
                    client.Set(key + "2", key);
                    client.SetRangeInHash(key + "3", values);
                }

                return Task.FromResult(client.Get<Int64>(key + "1"));
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

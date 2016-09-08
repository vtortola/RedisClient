using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceComparison.Tests.SimpleTests
{
    public class ServiceStackTransactionTest : TransactionTest
    {
        IRedisClientsManager _pool;
        public override Task Init(IPEndPoint endpoint, System.Threading.CancellationToken cancel)
        {
            _pool = new PooledRedisClientManager(100, 60, endpoint.ToString());
            using (var c = _pool.GetClient())
                c.Increment("whatever", 1);
            return Task.FromResult((Object)null);
        }

        public override Task<Int64> RunClient(Int32 id, System.Threading.CancellationToken cancel)
        {
            var key = this.GetType().Name + "_" + id;
            using (var client = _pool.GetClient())
            {
                var values = new[] { new KeyValuePair<String, String>("Member1", "Value1"), new KeyValuePair<String, String>("Member2", "Value2") };

                for (var i = 0; i < Iterations; i++)
                {
                    using (var trans = client.CreateTransaction())
                    {
                        trans.QueueCommand(r => r.IncrementValue(key + "1"));
                        trans.QueueCommand(r => r.Set(key + "2", key));
                        trans.QueueCommand(r => r.SetRangeInHash(key + "3", values));
                        trans.Commit();
                    }
                }

                return Task.FromResult(client.Get<Int64>(key + "1"));
            }
        }

        public override void ClearData()
        {
            using(var client = _pool.GetClient())
                client.FlushAll();
        }
    }
}

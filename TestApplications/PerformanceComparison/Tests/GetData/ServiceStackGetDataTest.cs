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
            var list = Enumerable.Range(0, 10).Select(i => i.ToString("0000000000")).ToList();
            using (var c = _pool.GetClient())
            {
                c.FlushAll();
                c.AddRangeToSet("testkeySSE", list);
                c.AddRangeToSet("testkeySSE2", list);
                c.Set("testkeySSE3", "hi this is a test");
            }
            return Task.FromResult((Object)null);
        }

        public Task RunClient(Int32 id, CancellationToken cancel)
        {
            using (var client = _pool.GetClient())
            using (var pipeline = client.CreatePipeline())
            {
                pipeline.QueueCommand(r => r.GetAllItemsFromSet("testkeySSE"));
                pipeline.QueueCommand(r => r.GetAllItemsFromSet("testkeySSE2"));
                pipeline.QueueCommand(r => r.Get<String>("testkeySSE3"));
                pipeline.Flush();
            }
            return Task.FromResult<Object>(null);
        }

        public void ClearData()
        {
            using(var client = _pool.GetClient())
                client.FlushAll();
        }
    }
}

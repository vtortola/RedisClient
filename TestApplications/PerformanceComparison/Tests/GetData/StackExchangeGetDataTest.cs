using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceComparison.Tests.GetData
{
    public class StackExchangeGetDataTest : ITest
    {
        StackExchange.Redis.ConnectionMultiplexer _connection;

        public async Task Init(IPEndPoint endpoint, System.Threading.CancellationToken cancel)
        {
            _connection = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(endpoint.ToString()).ConfigureAwait(false);
            var db = _connection.GetDatabase();
            var data = Enumerable.Range(0, 10).Select(i => (RedisValue)i.ToString("0000000000")).ToArray();
            db.SetAdd("testkeySER", data);
            db.SetAdd("testkeySER2", data);
            db.StringSet("testkeySER3", "hi this is a test");
        }

        public async Task RunClient(Int32 id, System.Threading.CancellationToken cancel)
        {
            var db = _connection.GetDatabase();
            var batch = db.CreateBatch();
            var t1 = batch.SetMembersAsync("testkeySER");
            var t2 = batch.SetMembersAsync("testkeySER2");
            var t3 = batch.StringGetAsync("testkeySER3");
            batch.Execute();
            await Task.WhenAll(t1, t2, t3).ConfigureAwait(false);
        }

        public void ClearData()
        {
            //var ep = _connection.GetEndPoints();
            //await _connection.GetServer( "localhost:6379, allowAdmin=true").FlushDatabaseAsync().ConfigureAwait(false);
            _connection.Dispose();
        }
    }
}

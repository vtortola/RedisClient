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
            db.SetAdd("testkeySER", Enumerable.Range(0, 10).Select(i => (RedisValue)i.ToString("0000000000")).ToArray());
        }

        public async Task RunClient(Int32 id, System.Threading.CancellationToken cancel)
        {
            var db = _connection.GetDatabase();
            var values = await db.SetMembersAsync("testkeySER").ConfigureAwait(false);
        }

        public void ClearData()
        {
            //var ep = _connection.GetEndPoints();
            //await _connection.GetServer( "localhost:6379, allowAdmin=true").FlushDatabaseAsync().ConfigureAwait(false);
            _connection.Dispose();
        }
    }
}

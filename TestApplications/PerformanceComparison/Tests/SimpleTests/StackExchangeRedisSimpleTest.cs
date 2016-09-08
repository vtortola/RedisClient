using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceComparison.Tests.SimpleTests
{
    public class StackExchangeRedisSimpleTest : SimpleTest
    {
        StackExchange.Redis.ConnectionMultiplexer _connection;

        public override async Task Init(IPEndPoint endpoint, System.Threading.CancellationToken cancel)
        {
            _connection = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(endpoint.ToString()).ConfigureAwait(false);
            await _connection.GetDatabase().StringIncrementAsync("whatever").ConfigureAwait(false);
        }

        public override async Task<Int64> RunClient(Int32 id, System.Threading.CancellationToken cancel)
        {
            var key = this.GetType().Name + "_" + id;
            var db = _connection.GetDatabase();
            for (int i = 0; i < Iterations; i++)
            {
                await db.StringIncrementAsync(key).ConfigureAwait(false);
            }

            return Int64.Parse(await db.StringGetAsync(key).ConfigureAwait(false));
        }

        public override void ClearData()
        {
            //var ep = _connection.GetEndPoints();
            //await _connection.GetServer( "localhost:6379, allowAdmin=true").FlushDatabaseAsync().ConfigureAwait(false);
            _connection.Dispose();
        }
    }
}

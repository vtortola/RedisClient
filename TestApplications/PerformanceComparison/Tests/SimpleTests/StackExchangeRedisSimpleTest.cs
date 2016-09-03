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
            var values = new[] { new HashEntry("Member1", "Value1"), new HashEntry("Member2", "Value2") };
            for (int i = 0; i < Iterations; i++)
            {
                await db.StringIncrementAsync(key + "1").ConfigureAwait(false);
                await db.StringSetAsync(key + "2", key).ConfigureAwait(false);
                await db.HashSetAsync(key + "3", values).ConfigureAwait(false);
            }

            return Int64.Parse(await db.StringGetAsync(key + "1").ConfigureAwait(false));
        }

        public override void ClearData()
        {
            //var ep = _connection.GetEndPoints();
            //await _connection.GetServer( "localhost:6379, allowAdmin=true").FlushDatabaseAsync().ConfigureAwait(false);
            _connection.Dispose();
        }
    }
}

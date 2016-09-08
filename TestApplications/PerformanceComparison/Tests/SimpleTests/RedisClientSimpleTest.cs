using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace PerformanceComparison.Tests.SimpleTests
{
    public class RedisClientSimpleTest : ITest
    {
        RedisClient _client;

        public async Task Init(IPEndPoint endpoint, CancellationToken cancel)
        {
            var options = new RedisClientOptions();
            options.MultiplexPool.CommandConnections = 1;
            options.MultiplexPool.SubscriptionOptions = 1;

            _client = new RedisClient(endpoint, options);

            await _client.ConnectAsync(cancel).ConfigureAwait(false);
            using (var channel = _client.CreateChannel())
            {
                channel.Dispatch("flushdb");
                await channel.ExecuteAsync("incr @key", new { key = "whatever" }).ConfigureAwait(false);
            }
        }

        public async Task RunClient(Int32 id, CancellationToken cancel)
        {
            var key = id.ToString();
            using (var channel = _client.CreateChannel())
            {
                var r = await channel.ExecuteAsync(@"incr @key", new { key }).ConfigureAwait(false);
                r.ThrowErrorIfAny();
            }
        }

        public void ClearData()
        {
            using (var channel = _client.CreateChannel())
                channel.Execute("flushdb");
            _client.Dispose();
        }
    }
}

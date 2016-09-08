using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace PerformanceComparison.Tests.GetData
{
    public class RedisClientGetDataTest : ITest
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
                await channel.ExecuteAsync("sadd @key @data", new 
                { 
                    key = "testkeyRC",
                    data = Enumerable.Range(0, 10).Select(i=>i.ToString("0000000000"))
                }).ConfigureAwait(false);
            }
        }

        public async Task RunClient(Int32 id, CancellationToken cancel)
        {
            using(var channel = _client.CreateChannel())
            {
                var result = await channel
                                    .ExecuteAsync(@"smembers @key",
                                        new { key = "testkeyRC" }).ConfigureAwait(false);
                var x = result[0].GetStringArray();
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

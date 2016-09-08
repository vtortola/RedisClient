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
                var data = Enumerable.Range(0, 10).Select(i=>i.ToString("0000000000"));
                await channel.ExecuteAsync(@"
                              sadd @key @data
                              sadd @key2 @data
                              set @key3 'hi this is a test'"
                    , new 
                    { 
                        key = "testkeyRC",
                        key2 = "testkeyRC2",
                        key3 = "testkeyRC3",
                        data
                    }).ConfigureAwait(false);
            }
        }

        public async Task RunClient(Int32 id, CancellationToken cancel)
        {
            using(var channel = _client.CreateChannel())
            {
                await channel
                        .ExecuteAsync(@"
                                smembers @key
                                smembers @key2
                                get @key3",
                                new 
                                {
                                    key = "testkeyRC",
                                    key2 = "testkeyRC2",
                                    key3 = "testkeyRC3", 
                                }).ConfigureAwait(false);

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

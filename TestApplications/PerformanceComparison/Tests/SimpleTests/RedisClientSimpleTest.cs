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
    public class RedisClientSimpleTest : SimpleTest
    {
        RedisClient _client;

        public override async Task Init(IPEndPoint endpoint, CancellationToken cancel)
        {
            _client = new RedisClient(endpoint);

            await _client.ConnectAsync(cancel).ConfigureAwait(false);
            using (var channel = _client.CreateChannel())
            {
                channel.Dispatch("flushdb");
                await channel.ExecuteAsync("incr @key", new { key = "whatever" }).ConfigureAwait(false);
            }
        }

        public override async Task<Int64> RunClient(Int32 id, CancellationToken cancel)
        {
            var key = this.GetType().Name + "_" + id;
            using (var channel = _client.CreateChannel())
            {
                var data = new[] { "Member1", "Value1", "Member2", "Value2" };
                for (int i = 0; i < Iterations; i++)
                {
                    var r = await channel.ExecuteAsync(@"
                                          incr @key1
                                          set @key2 @key1
                                          hmset @key3 @data",
                                          new { key1 = key + "1", key2 = key + "2", key3 = key + "3", data }).ConfigureAwait(false);

                    r.ThrowErrorIfAny();
                }

                var final = await channel.ExecuteAsync("get @key", new { key = key + "1" }).ConfigureAwait(false);
                return Int64.Parse(final[0].GetString());
            }
        }

        public override void ClearData()
        {
            using (var channel = _client.CreateChannel())
                channel.Execute("flushdb");
            _client.Dispose();
        }
    }
}

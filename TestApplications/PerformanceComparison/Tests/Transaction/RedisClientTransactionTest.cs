﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace PerformanceComparison.Tests.SimpleTests
{
    public class RedisClientTransactionTest : ITest
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
            using(var channel = _client.CreateChannel())
            {
                var data = new[] { "Member1", "Value1", "Member2", "Value2", "Member3", "Value3", "Member4", "Value4" };

                await channel.ExecuteAsync(@"
                              multi
                              incr @key1
                              set @key2 @key1
                              hmset @key3 @data
                              exec",
                              new { key1 = key + "1", key2 = key + "2", key3 = key + "3", data }).ConfigureAwait(false);
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

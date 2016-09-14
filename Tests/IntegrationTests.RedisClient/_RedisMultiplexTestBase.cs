using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using vtortola.Redis;

namespace IntegrationTests.RedisClientTests
{
    [TestClass]
    public abstract class RedisMultiplexTestBase
    {
        protected RedisClient Client { get; private set; }
        CancellationTokenSource _cancel;

        [TestInitialize]
        public void Init()
        {
            var options = GetOptions();
            Client = new RedisClient(RedisInstance.Endpoint, options);
            _cancel = new CancellationTokenSource();
            Client.ConnectAsync(_cancel.Token).Wait();

            using(var channel = Client.CreateChannel())
            {
                channel.Dispatch("FLUSHALL");
            }
        }

        protected virtual RedisClientOptions GetOptions()
        {
            var options = new RedisClientOptions() 
            { 
               UseExecutionPlanCaching = true, 
                PreventPingIfActive = false,
                Logger = new TraceRedisClientLogger()
            };

            options.MultiplexPool.CommandConnections = 5;
            options.MultiplexPool.SubscriptionOptions = 5;
 
            if (!Debugger.IsAttached)
                options.PingTimeout = TimeSpan.FromMilliseconds(500);

            return options;
        }
       
        [TestCleanup]
        public void TearDown()
        {
            _cancel.Cancel();
            Client.Dispose();
        }
    }
}

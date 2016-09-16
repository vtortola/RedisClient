using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
            Client = new RedisClient(new IPEndPoint(IPAddress.Loopback, 6379)/* RedisInstance.Endpoint*/, options);
            _cancel = new CancellationTokenSource();
            Task.Run(()=> Client.ConnectAsync(_cancel.Token));
            using (var channel = Client.CreateChannel())
            {
                channel.Execute("FLUSHALL");
            }
            Log("Test Initialized.");
        }

        protected void Log(String format, params Object[] args)
        {
            Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff ") + String.Format(format, args));
        }

        protected virtual RedisClientOptions GetOptions()
        {
            var options = new RedisClientOptions() 
            { 
               UseExecutionPlanCaching = true, 
                PreventPingIfActive = false,
                Logger = new TraceRedisClientLogger()
            };

            if (!Debugger.IsAttached)
                options.PingTimeout = TimeSpan.FromMilliseconds(500);

            return options;
        }
       
        [TestCleanup]
        public void TearDown()
        {
            Log("Test Tear down.");
            _cancel.Cancel();
            Client.Dispose();
        }
    }
}

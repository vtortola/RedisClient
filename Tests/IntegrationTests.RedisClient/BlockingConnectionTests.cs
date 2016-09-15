using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IntegrationTests.RedisClientTests;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace IntegrationTests
{
    [TestClass]
    public class BlockingConnectionTests : RedisMultiplexTestBase
    {
        protected override RedisClientOptions GetOptions()
        {
            var options = base.GetOptions();
            options.PingTimeout = Timeout.InfiniteTimeSpan;
            options.ExclusivePool.Minimum = 1;
            options.ExclusivePool.Maximum = 1;
            return options;
        }

        [TestMethod]
        public async Task CanExecuteBlockingOperation()
        {
            Log("Test");
            using (var channel = Client.CreateChannel())
            {
                Log("Channel acquired");
                var cancellation = new CancellationTokenSource();
                var result = await channel.ExecuteAsync(@"LPUSH key test 
                                                          BRPOP key 10")
                                          .ConfigureAwait(false);
                Log("Operation completed");
                result = result[1].AsResults();
                Assert.AreEqual("test", result[1].GetString());

            }
        }
    }
}

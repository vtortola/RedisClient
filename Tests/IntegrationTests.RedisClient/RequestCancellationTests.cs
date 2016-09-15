using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using vtortola.Redis;

namespace IntegrationTests.RedisClientTests
{
    [TestClass]
    public class RequestCancellationTests : RedisMultiplexTestBase
    {
        protected override vtortola.Redis.RedisClientOptions GetOptions()
        {
            var options = base.GetOptions();
            options.PingTimeout = Timeout.InfiniteTimeSpan;
            options.ExclusivePool.Minimum = 1;
            options.ExclusivePool.Maximum = 1;
            return options;
        }

        [TestMethod]
        public async Task CanExecuteSimpleCommand()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync(@"
                                        incrby examplekey 1
                                        decrby lele 2");

                Assert.IsNotNull(result);
                Assert.AreEqual(1L, result[0].GetInteger());
                Assert.AreEqual(-2L, result[1].GetInteger());
            }
        }

        [TestMethod]
        public async Task CanDoMixedSubscribeAsync()
        {
            var msgList = new List<RedisNotification>();
            using (var channel = Client.CreateChannel())
            {
                channel.NotificationHandler = msg => msgList.Add(msg);

                var cmd = @"
                        set aa 1
                        subscribe whatever
                        get aa";

                var results = await channel.ExecuteAsync(cmd).ConfigureAwait(false);

                Assert.AreEqual("OK", results[0].GetString());
                Assert.AreEqual("OK", results[1].GetString());
                Assert.AreEqual("1", results[2].GetString());

                results = channel.Execute("publish whatever whenever");
                Assert.AreEqual(1L, results[0].GetInteger());

                var counter = 0;
                while (msgList.Count < 1 && counter < 10)
                {
                    await Task.Delay(100).ConfigureAwait(false);
                    counter++;
                }
                Assert.AreEqual(1, msgList.Count);
            }
        }


        [TestMethod]
        public void CanCancelEarly()
        {
            using (var channel = Client.CreateChannel())
            {
                var cancellation = new CancellationTokenSource();
                var task = channel.ExecuteAsync("BRPOP unexistent 10", cancellation.Token);
                cancellation.Cancel();
                Thread.Sleep(100);
                Assert.AreEqual(TaskStatus.Canceled, task.Status);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task ThrowsTaskCancelledException()
        {
            var cancellation = new CancellationTokenSource();
            using (var channel = Client.CreateChannel())
            {
                var task = channel.ExecuteAsync("BRPOP unexistent 10", cancellation.Token);
                cancellation.Cancel();
                await task;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task CanCancelLate()
        {
            var cancellation = new CancellationTokenSource();
            using (var channel = Client.CreateChannel())
            {
                var task = channel.ExecuteAsync("BRPOP unexistent 2", cancellation.Token);
                Thread.Sleep(1000);
                cancellation.Cancel();
                await task;
            }
        }

    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationTests.RedisClientTests
{
    [TestClass]
    public class WithPubSub : RedisMultiplexTestBase
    {
        [TestMethod]
        public void CanDoSimpleSubscribe()
        {
            using (var channel = Client.CreateChannel())
            {
                var results = channel.Execute("subscribe whatever");
                Assert.AreEqual("OK", results[0].GetString());
            }
        }

        [TestMethod]
        public void CanDoMultipleSubscribe()
        {
            using (var channel = Client.CreateChannel())
            {
                var results = channel.Execute("subscribe whatever whenever");
                Assert.AreEqual("OK", results[0].GetString());
            }
        }

        [TestMethod]
        public void CanDoMixedSubscribe()
        {
            using (var channel = Client.CreateChannel())
            {
                var cmd = @"
                        set aa 1
                        subscribe whatever
                        get aa";

                var results = channel.Execute(cmd);

                Assert.AreEqual("OK", results[0].GetString());
                Assert.AreEqual("OK", results[1].GetString());
                Assert.AreEqual("1", results[2].GetString());
            }
        }

        [TestMethod]
        public async Task CanDoMixedSubscribeAsync()
        {
            using (var channel = Client.CreateChannel())
            {
                var msgList = new List<RedisNotification>();
                channel.NotificationHandler = msg => msgList.Add(msg);

                var cmd = @"
                        set aa 1
                        subscribe whateverrr
                        get aa";

                var results = await channel.ExecuteAsync(cmd);

                Assert.AreEqual("OK", results[0].GetString());
                Assert.AreEqual("OK", results[1].GetString());
                Assert.AreEqual("1", results[2].GetString());

                results = channel.Execute("publish whateverrr whenever");
                Assert.AreEqual(1L, results[0].GetInteger());

                var counter = 0;
                while (msgList.Count < 1 && counter < 5)
                {
                    Thread.Sleep(100);
                    counter++;
                }
                Assert.AreEqual(1, msgList.Count);
            }
        }

        [TestMethod]
        public void CanReceiveMessage()
        {
            var msgList = new List<RedisNotification>();
            using (var channel = Client.CreateChannel())
            {
                channel.NotificationHandler = msg => msgList.Add(msg);
                
                var results = channel.Execute("subscribe whatever");
                results = channel.Execute("publish whatever whenever");
                var counter = 0;
                while (msgList.Count < 1 && counter < 10)
                {
                    Thread.Sleep(100);
                    counter++;
                }
                Assert.AreEqual(1, msgList.Count);
            }
        }

        [TestMethod]
        public void CanReceiveUtf8Message()
        {
            var msgList = new List<RedisNotification>();
            using (var channel = Client.CreateChannel())
            {
                channel.NotificationHandler = msg => msgList.Add(msg);

                var results = channel.Execute("subscribe Düsseldorf");
                results = channel.Execute("publish Düsseldorf Düsseldorf");
                var counter = 0;
                while (msgList.Count < 1 && counter < 10)
                {
                    Thread.Sleep(100);
                    counter++;
                }
                Assert.AreEqual(1, msgList.Count);
                var pushed = msgList[0];
                Assert.AreEqual("Düsseldorf", pushed.Content);
                Assert.AreEqual("Düsseldorf", pushed.PublishedKey);
                Assert.AreEqual("Düsseldorf", pushed.SubscribedKey);
            }
        }

        [TestMethod]
        public void CanReceivePMessage()
        {
            var messages = 0;
            using (var channel = Client.CreateChannel())
            {
                channel.NotificationHandler = msg => Interlocked.Increment(ref messages);

                var results = channel.Execute("psubscribe ?hateve?");
                results = channel.Execute("publish whatever whenever");
                var counter = 0;
                while (messages < 1 && counter < 100)
                {
                    Thread.Sleep(100);
                    counter++;
                }

                Assert.AreEqual(1, Thread.VolatileRead(ref messages));
            }
        }
    }
}

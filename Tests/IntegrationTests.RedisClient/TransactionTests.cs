using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.Collections.Generic;

namespace IntegrationTests.RedisClientTests
{
    [TestClass]
    public class TransactionTests : RedisMultiplexTestBase
    {
        [TestMethod]
        public void CanDoSimpleTransaction()
        {
            using (var channel = Client.CreateChannel())
            {
                var cmd = @"
                     multi
                     incr foo
                     incr bar
                     exec";

                var result = channel.Execute(cmd);

                Assert.AreEqual("OK", result[0].GetString());
                Assert.AreEqual(1, result[1].GetInteger());
                Assert.AreEqual(1, result[2].GetInteger());
                Assert.AreEqual("OK", result[3].GetString());
            }
        }

        [TestMethod]
        public void CanDoMultipleTransactions()
        {
            using (var channel = Client.CreateChannel())
            {
                var cmd = @"
                     multi
                     incr AA
                     incr BB
                     exec
                     multi
                     incrby CC 2
                     incrby DD 3
                     exec";

                var result = channel.Execute(cmd);

                Assert.AreEqual("OK", result[0].GetString());
                Assert.AreEqual(1, result[1].GetInteger());
                Assert.AreEqual(1, result[2].GetInteger());
                Assert.AreEqual("OK", result[3].GetString());
                Assert.AreEqual("OK", result[4].GetString());
                Assert.AreEqual(2, result[5].GetInteger());
                Assert.AreEqual(3, result[6].GetInteger());
                Assert.AreEqual("OK", result[7].GetString());
            }
        }

        [TestMethod]
        public void CanDiscardTransactions()
        {
            using (var channel = Client.CreateChannel())
            {
                var cmd = @"
                     multi
                     incr AA
                     incr BB
                     exec
                     multi
                     incrby CC 2
                     incrby DD 3
                     discard";

                var result = channel.Execute(cmd);

                Assert.AreEqual("OK", result[0].GetString());
                Assert.AreEqual(1, result[1].GetInteger());
                Assert.AreEqual(1, result[2].GetInteger());
                Assert.AreEqual("OK", result[3].GetString());
                Assert.AreEqual("OK", result[4].GetString());
                Assert.AreEqual("DISCARDED", result[5].GetString());
                Assert.AreEqual("DISCARDED", result[6].GetString());
                Assert.AreEqual("OK", result[7].GetString());
            }
        }

        [TestMethod]
        public void EarlyFailTransactionOnSyntaxError()
        {
            using (var channel = Client.CreateChannel())
            {
                var cmd = @"
                     multi
                     incr foo abc
                     incr bar
                     exec";

                try
                {
                    var result = channel.Execute(cmd);
                    Assert.Fail();
                }
                catch(RedisClientMultipleCommandException ex)
                {
                    Assert.AreEqual(1, ex.InnerExceptions.Length);
                    Assert.AreEqual(2, ex.InnerExceptions[0].LineNumber);
                }
                catch
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public void LateFailTransactionOnSyntaxError()
        {
            using (var channel = Client.CreateChannel())
            {
                var cmd = @"
                     multi
                     incrby foo abc
                     incr bar
                     exec";

                var result = channel.Execute(cmd);

                Assert.AreEqual("OK", result[0].GetString());
                Assert.IsInstanceOfType(result[1].GetException(), typeof(RedisClientCommandException));
                Assert.AreEqual(1, result[2].GetInteger());
                Assert.AreEqual("OK", result[3].GetString());
            }
        }
    }
}

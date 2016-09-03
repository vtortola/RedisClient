using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Collections.Generic;
using vtortola.Redis;
using System.Linq;

namespace IntegrationTests.RedisClientTests
{
    [TestClass]
    public class ZSetOperationTests : RedisMultiplexTestBase
    {
        [TestMethod]
        public async Task CanUseKeyValueCollection()
        {
            var values = new List<KeyValuePair<Int32, String>>();
            for (int i = 0; i < 10; i++)
            {
                values.Add(new KeyValuePair<Int32, String>(i, "member" + i));
            }

            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync(@"ZADD examplekey @values", new { values = Parameter.Sequence(values)});

                Assert.IsNotNull(result);
                Assert.AreEqual(10L, result[0].GetInteger());
                Assert.AreEqual("5", channel.Execute("ZSCORE examplekey member5")[0].GetString());
            }
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using vtortola.Redis;
using System.Threading.Tasks;
using System.Linq;

namespace IntegrationTests.RedisClientTests
{
    [TestClass]
    public class HashOperationsTests : RedisMultiplexTestBase
    {
        [TestMethod]
        public void FailsToConvertToArray()
        {
            using (var channel = Client.CreateChannel())
            {
                var command = @"hmset @key @collection
                                hget @key Name
                                hget @key Surname
                                hmget @key Name Surname non-existent
                                hgetall @key";

                var result = channel.Execute(command, new { key = "mykey", collection = Parameter.SequenceProperties(new { Name="Valeriano",  Surname="Tortola" }) });

                Assert.AreEqual("OK", result[0].GetString());
                Assert.AreEqual("Valeriano", result[1].GetString());
                Assert.AreEqual("Tortola", result[2].GetString());
                var array = result[3].GetStringArray();
                Assert.AreEqual(3, array.Length);
                Assert.AreEqual("Valeriano", array[0]);
                Assert.AreEqual("Tortola", array[1]);
                Assert.IsNull(array[2]);
                var dictionary = result[4].AsDictionaryCollation<String,String>();
                Assert.IsNotNull(dictionary);
                Assert.AreEqual("Valeriano", dictionary["Name"]);
                Assert.AreEqual("Tortola", dictionary["Surname"]);
            }
        }

        [TestMethod]
        public async Task CanHandleNilResponse()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync(@"hget examplekey field");

                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetString());
            }
        }

        class Test
        {
            public String Member { get; set; }
        }

        [TestMethod]
        public async Task CanSaveEmptyString()
        {
            using (var channel = Client.CreateChannel())
            {
                var test = new Test() { Member = String.Empty };

                var result = await channel.ExecuteAsync(@"
                                           hmset key @data
                                           hget key Member", new { data = Parameter.SequenceProperties(test) });
                result.ThrowErrorIfAny();

                Assert.AreEqual(String.Empty, result[1].GetString());
            }
        }

        [TestMethod]
        public async Task CanSaveAndRetrieveEmptyString()
        {
            using (var channel = Client.CreateChannel())
            {
                var test = new Test() { Member = String.Empty };

                var result = await channel.ExecuteAsync(@"
                                           hmset key @data
                                           hgetall key", new { data = Parameter.SequenceProperties(test) });
                result.ThrowErrorIfAny();

                var rtest = result[1].AsObjectCollation<Test>();

                Assert.AreEqual(test.Member, rtest.Member);
            }
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using UnitTests.Common;

namespace IntegrationTests.RedisClientTests
{
    [TestClass]
    public class ListOperationsTest : RedisMultiplexTestBase
    {
        [TestMethod]
        public void CanRetrieveElementsFromAList()
        {
            using (var channel = Client.CreateChannel())
            {
                var command = @"
                            rpush mylist 4
                            rpush mylist 5
                            rpush mylist 6
                            lpush mylist 3
                            lpush mylist 2
                            lpush mylist 1
                            llen mylist
                            lindex mylist 5
                            lrange mylist 1 -2
                            lpop mylist
                            rpop mylist
                            llen mylist
                        ";

                var result = channel.Execute(command);

                Assert.AreEqual(1L, result[0].GetInteger());
                Assert.AreEqual(2L, result[1].GetInteger());
                Assert.AreEqual(3L, result[2].GetInteger());
                Assert.AreEqual(4L, result[3].GetInteger());
                Assert.AreEqual(5L, result[4].GetInteger());
                Assert.AreEqual(6L, result[5].GetInteger());
                Assert.AreEqual(6L, result[6].GetInteger());
                Assert.AreEqual("6", result[7].GetString());

                var array = result[8].AsResults();
                Assert.AreEqual("2", array[0].GetString());
                Assert.AreEqual("3", array[1].GetString());
                Assert.AreEqual("4", array[2].GetString());
                Assert.AreEqual("5", array[3].GetString());

                var array3 = result[8].GetStringArray();
                Assert.AreEqual("2", array3[0]);
                Assert.AreEqual("3", array3[1]);
                Assert.AreEqual("4", array3[2]);
                Assert.AreEqual("5", array3[3]);

                Assert.AreEqual("1", result[9].GetString());
                Assert.AreEqual("6", result[10].GetString());
                Assert.AreEqual(4L, result[11].GetInteger());
            }
        }

        [TestMethod]
        public void CanHandleNil()
        {
            using (var channel = Client.CreateChannel())
            {
                var command = "rpop non-existent";

                var result = channel.Execute(command);
                Assert.IsNull(result[0].GetString());
            }
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientCastException), MessagePattern="^Object 'RESPInteger' cannot be casted to 'RESPArray'$")]
        public void FailsToConvertToArray()
        {
            using (var channel = Client.CreateChannel())
            {
                var command = @"
                            rpush mylist 4
                            rpush mylist 5
                            rpush mylist 6
                            lpush mylist 3
                            lpush mylist 2
                            lpush mylist 1
                            lrange mylist 1 -2
                            mget no1 no2 no3
                        ";

                var result = channel.Execute(command);
                result[0].GetStringArray();
            }
        }
    }
}

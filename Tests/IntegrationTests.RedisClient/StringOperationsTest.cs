using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using vtortola.Redis;
using System.Threading.Tasks;
using UnitTests.Common;

namespace IntegrationTests.RedisClientTests
{
    [TestClass]
    public class StringOperationsTest : RedisMultiplexTestBase
    {
        [TestMethod]
        public async Task CanExecuteVerySimpleCommand()
        {
           using (var channel = Client.CreateChannel())
           { 
                var result = await channel.ExecuteAsync(@"incr examplekey");

                Assert.IsNotNull(result);
                Assert.AreEqual(1L, result[0].GetInteger());
            }
        }

        [TestMethod]
        public async Task CanExecuteSimpleCommand()
        {
            using(var channel = Client.CreateChannel())
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
        public async Task CanExecuteComplexBreakLinesCommand()
        {
            using (var channel = Client.CreateChannel())
            {
                var parameter = new
                {
                    value = "dsfsadfsdaf  \n\n\n"
                };

                var result = await channel.ExecuteAsync(@"set examplekey @value", parameter);

                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetException());
                Assert.AreEqual("OK", result[0].GetString());

                result = await channel.ExecuteAsync(@"get examplekey");
                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetException());
                Assert.AreEqual("dsfsadfsdaf  \n\n\n", result[0].GetString());
            }
        }

        [TestMethod]
        public async Task CanExecuteComplexBreakLinesHashCommand()
        {
            using (var channel = Client.CreateChannel())
            {
                var parameter = new
                {
                    value = "dsfsadfsdaf  \r\n\r\n\r\n"
                };

                var result = await channel.ExecuteAsync(@"hmset examplekey field1 @value field2 @value", parameter);

                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetException());
                Assert.AreEqual("OK", result[0].GetString());

                result = await channel.ExecuteAsync(@"hmget examplekey field1 field2");
                Assert.IsNotNull(result);
                Assert.IsNull(result[0].GetException());
                Assert.AreEqual("dsfsadfsdaf  \r\n\r\n\r\n", result[0].GetStringArray()[0]);
            }
        }

        [TestMethod]
        public async Task CanDetectErrors()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync(@"
                                        incrby examplekey whatever
                                        decrby lele 2");

                Assert.IsNotNull(result);
                try
                {
                    result[0].GetInteger();
                    Assert.Fail();
                }
                catch (RedisClientCommandException) { }
                catch (Exception) { Assert.Fail(); }


                Assert.IsInstanceOfType(result[0].GetException(), typeof(RedisClientCommandException));
                Assert.AreEqual(-2L, result[1].GetInteger());
            }
        }

        [TestMethod]
        public async Task CanExecuteSimpleCommandWithArrayResult()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync(@"
                                        incrby examplekey 1
                                        decrby lele 2
                                        mget examplekey lele");

                Assert.IsNotNull(result);
                Assert.AreEqual(1L, result[0].GetInteger());
                Assert.AreEqual(-2L, result[1].GetInteger());
                Assert.AreEqual("1", result[2].AsResults()[0].GetString());
                Assert.AreEqual("-2", result[2].AsResults()[1].GetString());
            }
        }

        [TestMethod]
        public async Task CanExecuteSimpleCommandWithArrayParameterArrayResult()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync(@"
                                        incrby examplekey 1
                                        decrby lele 2
                                        mget @list", new { list = new[] { "examplekey", "lele" } });

                Assert.IsNotNull(result);
                Assert.AreEqual(1L, result[0].GetInteger());
                Assert.AreEqual(-2L, result[1].GetInteger());
                Assert.AreEqual("1", result[2].AsResults()[0].GetString());
                Assert.AreEqual("-2", result[2].AsResults()[1].GetString());
            }
        }

        [TestMethod]
        public async Task CanExecuteSimpleCommandWithArrayConvertResult()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync(@"
                                        incrby examplekey 1
                                        decrby lele 2
                                        mget examplekey lele");

                Assert.IsNotNull(result);
                Assert.AreEqual(1L, result[0].GetInteger());
                Assert.AreEqual(-2L, result[1].GetInteger());
                Assert.AreEqual("1", result[2].AsResults()[0].GetString());
                Assert.AreEqual("-2", result[2].AsResults()[1].GetString());
            }
        }

        [TestMethod]
        public async Task FailsIfCannotConvertType()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync(@"
                                        set examplekey test
                                        get examplekey");

                Assert.IsNotNull(result);
                Assert.AreEqual("OK", result[0].GetString());
                Assert.AreEqual("test", result[1].GetString());

                try
                {
                    result[1].GetInteger();
                    Assert.Fail();
                }
                catch (RedisClientException)
                { }

                try
                {
                    result[0].AsResults();
                    Assert.Fail();
                }
                catch (RedisClientException)
                { }
            }
        }

        [TestMethod]
        public async Task CanSaveEmptyString()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync(@"
                                           set key @data
                                           get key", new { data = String.Empty });
                result.ThrowErrorIfAny();

                Assert.AreEqual(String.Empty, result[1].GetString());
            }
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientBindingException), MessagePattern="^Null values are not accepted as parameters.$")]
        public async Task CanHandleNilStringParameter()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync(@"set lnullholder @data", new { data = (String)null });
                result.ThrowErrorIfAny();
            }
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientBindingException), MessagePattern="^The type 'Nullable`1' is not supported as parameter member.")]
        public async Task CanHandleNilParameter()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = await channel.ExecuteAsync(@"set lnullholder @data", new { data = (Int32?)null });
                result.ThrowErrorIfAny();
            }
        }
    }
}

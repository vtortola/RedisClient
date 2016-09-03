using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnitTests.Common;
using System.Threading;

namespace IntegrationTests.RedisClientTests
{
    [TestClass]
    public class ProcedureTests : RedisMultiplexTestBase
    {
        String uid = Guid.NewGuid().ToString();

        protected override RedisClientOptions GetOptions()
        {
            var options = base.GetOptions();
            var procedures = options.Procedures;
            procedures.Load(GetGoodScripts());

            procedures.Load(new StringReader(@"proc declaringError()
                                                return { 1, 2, 3
                                               endproc"));

            procedures.Load(new StringReader(@"proc executionError()
                                                return redis.call('incr', 'akey', 1) 
                                               endproc"));

            return options;
        }
                
        TextReader GetGoodScripts()
        {
            var builder = new StringBuilder();
            builder.AppendLine(@"proc test()
                                    return'" + uid + @"'
                                 endproc");

            builder.AppendLine(@"proc testWithParameters(arg)
                                    return arg .. '" + uid + @"'
                                 endproc");

            builder.AppendLine(@"proc testWithParametersAndKeys($akey, anarg)
                                    return akey .. anarg .. '" + uid + @"'
                                 endproc");

            builder.AppendLine(@"proc redisCall1($akey, anarg)                                   
                                    local r = string.reverse(anarg)
                                    redis.call('set', akey, r)
                                    return redis.call('get', akey) 
                                endproc");

            builder.AppendLine(@"proc register_user($users, name)
                                    local id = redis.call('incr', users)
                                    local storeId = users .. ':' .. id
                                    redis.call('set', storeId , name)
                                    return storeId
                                 endproc
                                  ");

            builder.AppendLine(@"proc arrayTableTest()
                                    return { 1, 2, 3} 
                                 endproc");

            builder.AppendLine(@"proc sumArraysAndRest(arg1[], arg2[])
                                    local function sum(t)
                                        local sum = 0
                                        for i=1, table.getn(t), 1 
                                        do 
                                           sum = sum + t[i]
                                        end
                                        return sum
                                    end
                                    local a1 = sum(arg1)
                                    local a2 = sum(arg2)
                                    return a2 - a1
                                 endproc");

            builder.AppendLine(@"proc sumKeyArraysAndRest(arg1[], $arg2[], $arg3)
                                    local function sum(t)
                                        local sum = 0
                                        for i=1, table.getn(t), 1 
                                        do 
                                           sum = sum + t[i]
                                        end
                                        return sum
                                    end
                                    local a1 = sum(arg1)
                                    local a2 = sum(arg2)
                                    return a2 - a1 + arg3
                                 endproc");

            return new StringReader(builder.ToString());
        }

        [TestMethod]
        public void CanRunSimpleScript()
        {
            using( var channel = Client.CreateChannel())
            {
                var result = channel.Execute("test");

                Assert.AreEqual(uid, result[0].GetString());
            }
        }

        [TestMethod]
        public void CanRunSimpleScriptWithParameters()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = channel.Execute("testWithParameters @testarg", new { testarg = 3 });

                Assert.AreEqual("3" + uid, result[0].GetString());
            }
        }

        [TestMethod]
        public void CanRunSimpleScriptWithParametersAndKeys()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = channel.Execute("testWithParametersAndKeys  @testakey @testanarg", new { testakey = "a", testanarg = "b" });

                Assert.AreEqual("ab" + uid, result[0].GetString());
            }
        }

        [TestMethod]
        public void CanRunRedisCalls()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = channel.Execute("redisCall1 @testakey @testanarg", new { testakey = "a", testanarg = uid });

                Assert.AreEqual(new String(uid.Reverse().ToArray()), result[0].GetString());
            }
        }

        [TestMethod]
        public void CanCombineCalls()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = channel.Execute(@"
                                    redisCall1 @akey @anarg
                                    get @akey", 
                                    new { akey = "a", anarg = uid });

                Assert.AreEqual(new String(uid.Reverse().ToArray()), result[0].GetString());
                Assert.AreEqual(new String(uid.Reverse().ToArray()), result[0].GetString());
            }
        }

        [TestMethod]
        public void CanPassArrayParameters()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = channel.Execute("sumArraysAndRest @a1 @a2",
                                             new { a1 = new [] { 1, 2, 3 }, a2 = new []{ 4, 5, 6 } });

                Assert.AreEqual(9L, result[0].GetInteger());

                result = channel.Execute("sumArraysAndRest @a1 @a2",
                                             new { a1 = new [] { 1, 2, 3, 4 }, a2 = new [] { 5, 6 } });

                Assert.AreEqual(1L, result[0].GetInteger());
            }
        }

        [TestMethod]
        public void CanPassKeyParametersInAnyPlace()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = channel.Execute("sumKeyArraysAndRest @a1 @a2 @a3",
                                             new { a1 = new[] { 1, 2, 3 }, a2 = new[] { 4, 5, 6 }, a3 = 2 });

                Assert.AreEqual(11L, result[0].GetInteger());

                result = channel.Execute("sumKeyArraysAndRest @a1 @a2 @a3",
                                             new { a1 = new[] { 1, 2, 3, 4 }, a2 = new[] { 5, 6 }, a3 = 2 });

                Assert.AreEqual(3L, result[0].GetInteger());
            }
        }

        [TestMethod]
        public void SimpleUserRegistration()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = channel.Execute("register_user '{users}' @name", new { name = "whatever" });

                Assert.AreEqual("{users}:1", result[0].GetString());
            }
        }

        [TestMethod]
        public void CanHandleLuaTables()
        {
            using (var channel = Client.CreateChannel())
            {
                var result = channel.Execute("arrayTableTest");

                var array = result[0].GetIntegerArray();
                Assert.IsNotNull(array);
                Assert.AreEqual(3, array.Length);
                Assert.AreEqual(1, array[0]);
                Assert.AreEqual(2, array[1]);
                Assert.AreEqual(3, array[2]);
            }
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientParsingException), typeof(RedisClientCommandException), MessagePattern="syntax error")]
        public void CanDetectDeclaringError()
        {
            Thread.Sleep(1000);
            using (var channel = Client.CreateChannel())
            {
                channel.Execute("declaringError")[0].GetString();
            }
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientCommandException), MessagePattern="executionError")]
        public void CanDetectExecutionError()
        {
            using (var channel = Client.CreateChannel())
            {
                channel.Execute("executionError")[0].GetString();
            }
        }
    }
}

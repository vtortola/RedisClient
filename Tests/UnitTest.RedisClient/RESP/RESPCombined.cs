using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;

namespace UnitTest.RedisClient.RESP
{
    [TestClass]
    public class RESPCombined
    {
        [TestMethod]
        public void CombinedResponses()
        {
            var str = ":45\r\n+OK\r\n$0\r\n\r\n$5\r\nhello\r\n$-1\r\n*0\r\n*-1\r\n+OK\r\n";

            for (int i = 1; i < str.Length + 10; i++)
            {
                var source = new DummySocketReader(str, i);

                Assert.AreEqual(45, RESPObject.Read<RESPInteger>(source).Value);
                Assert.AreEqual("OK", RESPObject.Read<RESPSimpleString>(source).Value);
                Assert.AreEqual(String.Empty, RESPObject.Read<RESPBulkString>(source).Value);
                Assert.AreEqual("hello", RESPObject.Read<RESPBulkString>(source).Value);
                Assert.AreEqual(null, RESPObject.Read<RESPBulkString>(source).Value);
                Assert.AreEqual(0, RESPObject.Read<RESPArray>(source).Count);
                Assert.AreEqual(0, RESPObject.Read<RESPArray>(source).Count);
                Assert.AreEqual("OK", RESPObject.Read<RESPSimpleString>(source).Value);
            }
        }
    }
}

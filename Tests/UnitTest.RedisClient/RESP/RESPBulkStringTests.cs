using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.IO;

namespace UnitTest.RedisClient.Protocol
{
    [TestClass]
    public class RESPBulkStringTests
    {
        [TestMethod]
        public void ShouldParseBulkString()
        {
            var source = new DummySocketReader("11\r\nHello world\r\nblahblahblah");
            RESPBulkString str = RESPBulkString.Load(source);
            Assert.AreEqual("Hello world", str.Value);
        }
        
        [TestMethod]
        public void ShouldParseBulkStringWithLineBreak()
        {
            var source = new DummySocketReader("12\r\nHello\r\nworld\r\nblahblahblah");
            RESPBulkString str = RESPBulkString.Load(source);
            Assert.AreEqual("Hello\r\nworld", str.Value);
        }

        [TestMethod]
        public void ShouldParseBulkStringWithLineBreakAtTheEnd()
        {
            var source = new DummySocketReader("14\r\nHello\r\nworld\r\n\r\n");
            RESPBulkString str = RESPBulkString.Load(source); 
            Assert.AreEqual("Hello\r\nworld\r\n", str.Value);
        }

        [TestMethod]
        public void ShouldParseEmptyBulkString()
        {
            var source = new DummySocketReader("0\r\n\r\n");
            RESPBulkString str = RESPBulkString.Load(source);

            Assert.AreEqual(String.Empty, str.Value);
        }

        [TestMethod]
        public void ShouldParseNullBulkString()
        {
            var source = new DummySocketReader("-1\r\n");
            RESPBulkString str = RESPBulkString.Load(source);
            Assert.AreEqual(null, str.Value);
        }
    }
}

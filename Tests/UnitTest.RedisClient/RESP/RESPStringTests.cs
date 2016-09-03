using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.IO;

namespace UnitTest.RedisClient.Protocol
{
    [TestClass]
    public class RESPStringTests
    {
        [TestMethod]
        public void ShouldParseString()
        {
            var source = new DummySocketReader("Hello world\r\nblahblahblah");
            RESPSimpleString str = RESPSimpleString.Load(source);
            Assert.AreEqual("Hello world", str.Value);
        }
    }
}

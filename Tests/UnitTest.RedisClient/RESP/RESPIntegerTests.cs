using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.IO;

namespace UnitTest.RedisClient.Protocol
{
    [TestClass]
    public class RESPIntegerTests
    {
        [TestMethod]
        public void ShouldParsePositiveInteger()
        {
            var source = new DummySocketReader("5\r\nblahblahblah");
            RESPInteger integer = RESPInteger.Load(source);
            Assert.AreEqual(5L, integer.Value);
        }

        [TestMethod]
        public void ShouldParseNegativeInteger()
        {
            var source = new DummySocketReader("-5\r\nblahblahblah");
            RESPInteger integer = RESPInteger.Load(source);
            Assert.AreEqual(-5L, integer.Value);
        }
    }
}

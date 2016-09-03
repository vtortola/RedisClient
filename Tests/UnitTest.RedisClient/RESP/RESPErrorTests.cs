using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.IO;

namespace UnitTest.RedisClient.Protocol
{
    [TestClass]
    public class RESPErrorTests
    {
        [TestMethod]
        public void ShouldParseError()
        {
            var source = new DummySocketReader("ERR Message\r\nblahblahblah");
            RESPError err = RESPError.Load(source);
            Assert.AreEqual("ERR", err.Prefix);
            Assert.AreEqual("Message", err.Message);
        }

        [TestMethod]
        public void ShouldParseErrorWithOnlyPrefix()
        {
            var source = new DummySocketReader("ERR\r\nblahblahblah");
            RESPError err = RESPError.Load(source);
            Assert.AreEqual("ERR", err.Prefix);
            Assert.IsNull(err.Message);
        }

    }
}

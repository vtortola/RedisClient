using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.IO;

namespace UnitTest.RedisClient.RESP
{
    [TestClass]
    public class RESPCommandTests
    {
        [TestMethod]
        public void CanCreateCommand()
        {
            var command = new RESPCommand(new RESPCommandLiteral("Hello"), false);
            command.Add(new RESPCommandLiteral("World"));
            command.Add(new RESPCommandLiteral("1"));

            var target = new DummySocketWriter(new MemoryStream());
            command.WriteTo(target);

            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("*3\r\n$5\r\nHello\r\n$5\r\nWorld\r\n$1\r\n1\r\n", target.ToString());
        }

    }
}

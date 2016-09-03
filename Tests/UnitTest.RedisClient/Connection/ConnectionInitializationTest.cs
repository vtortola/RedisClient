using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.IO;
using UnitTests.Common;

namespace UnitTest.RedisClient.Connection
{
    [TestClass]
    public class ConnectionInitializationTest
    {
        private SocketReader GetReader(Stream ms, Int32 bufferLength)
        {
            return new SocketReader(ms, bufferLength);
        }

        [TestMethod]
        public void IgnoresNoInitialization()
        {
            var initializer = new ConnectionInitializer(new RedisClientOptions());

            var writtingStream = new MemoryStream();
            var reader = new DummySocketReader(null);
            var writer = new DummySocketWriter(writtingStream);

            initializer.Initialize(reader, writer);
        }

        [TestMethod]
        public void InitializesWithCommands()
        {
            var options = new RedisClientOptions();
            options.InitializationCommands.Add(new PreInitializationCommand("auth vtortola"));
            var initializer = new ConnectionInitializer(options);

            var writtingStream = new MemoryStream();
            var reader = new DummySocketReader("+OK\r\n");
            var writer = new DummySocketWriter(writtingStream);

            initializer.Initialize(reader, writer);

            writtingStream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual("*2\r\n$4\r\nAUTH\r\n$8\r\nvtortola\r\n", new StreamReader(writtingStream).ReadToEnd());
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientCommandException), MessagePattern="ERR")]
        public void DetectInitializationErrors()
        {
            var options = new RedisClientOptions();
            options.InitializationCommands.Add(new PreInitializationCommand("auth vtortola"));
            var initializer = new ConnectionInitializer(options);

            var writtingStream = new MemoryStream();
            var reader = new DummySocketReader("-ERR Whatever\r\n");
            var writer = new DummySocketWriter(writtingStream);

            initializer.Initialize(reader, writer);
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientSocketException), MessagePattern = "Initialization command did not return any response.")]
        public void DetectMissingResponseErrors()
        {
            var options = new RedisClientOptions();
            options.InitializationCommands.Add(new PreInitializationCommand("auth vtortola"));
            var initializer = new ConnectionInitializer(options);

            var writtingStream = new MemoryStream();
            var reader = new DummySocketReader(null);
            var writer = new DummySocketWriter(writtingStream);

            initializer.Initialize(reader, writer);
        }
    }
}

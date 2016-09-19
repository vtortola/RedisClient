using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.IO;
using UnitTests.Common;
using System.Collections.Generic;

namespace UnitTest.RedisClient.Connection
{
    [TestClass]
    public class ProcedureInitializerTests
    {
        private SocketReader GetReader(Stream ms, Int32 bufferLength)
        {
            return new SocketReader(ms, bufferLength);
        }

        [TestMethod]
        public void IgnoresNoInitialization()
        {
            var procedures = new ProcedureCollection();
            var initializer = new ProcedureInitializer(procedures, NoLogger.Instance);
            var writtingStream = new MemoryStream();
            var reader = new DummySocketReader(null);
            var writer = new DummySocketWriter(writtingStream);

            initializer.Initialize(reader, writer);
            writer.Flush();
            writtingStream.Seek(0, SeekOrigin.Begin);
            
            Assert.AreEqual(0, writtingStream.Position);
        }

        [TestMethod]
        public void InitializesProcedures()
        {
            var bydigest = new Dictionary<String, ProcedureDefinition>
            {
                { "digest1", new ProcedureDefinition(){ Body ="digestBody1"} },
                { "digest2", new ProcedureDefinition(){ Body ="digestBody2"} }
            };
            var byalias = new Dictionary<String, ProcedureDefinition>();

            var procedures = new ProcedureCollection(byalias, bydigest);
            var initializer = new ProcedureInitializer(procedures, NoLogger.Instance);
            var writtingStream = new MemoryStream();
            var reader = new DummySocketReader("*2\r\n:0\r\n:0\r\n$7\r\ndigest1\r\n$7\r\ndigest2\r\n");
            var writer = new DummySocketWriter(writtingStream);

            initializer.Initialize(reader, writer);
            writer.Flush();
            writtingStream.Seek(0, SeekOrigin.Begin);

            Assert.AreEqual("*4\r\n$6\r\nSCRIPT\r\n$6\r\nexists\r\n$7\r\ndigest1\r\n$7\r\ndigest2\r\n*3\r\n$6\r\nSCRIPT\r\n$4\r\nload\r\n$11\r\ndigestBody1\r\n*3\r\n$6\r\nSCRIPT\r\n$4\r\nload\r\n$11\r\ndigestBody2\r\n", new StreamReader(writtingStream).ReadToEnd());
        }

        [TestMethod]
        public void IgnoresExistingProcedures()
        {
            var bydigest = new Dictionary<String, ProcedureDefinition>
            {
                { "digest1", new ProcedureDefinition(){ Body ="digestBody1"} },
                { "digest2", new ProcedureDefinition(){ Body ="digestBody2"} }
            };
            var byalias = new Dictionary<String, ProcedureDefinition>();

            var procedures = new ProcedureCollection(byalias, bydigest);
            var initializer = new ProcedureInitializer(procedures, NoLogger.Instance);
            var writtingStream = new MemoryStream();
            var reader = new DummySocketReader("*2\r\n:1\r\n:1\r\n");
            var writer = new DummySocketWriter(writtingStream);

            initializer.Initialize(reader, writer);
            writer.Flush();
            writtingStream.Seek(0, SeekOrigin.Begin);

            Assert.AreEqual("*4\r\n$6\r\nSCRIPT\r\n$6\r\nexists\r\n$7\r\ndigest1\r\n$7\r\ndigest2\r\n", new StreamReader(writtingStream).ReadToEnd());
        }
    }
}

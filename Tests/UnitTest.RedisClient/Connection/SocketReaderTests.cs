using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using vtortola.Redis;

namespace UnitTest.RedisClient.Connection
{
    [TestClass]
    public class SocketReaderTests
    {
        private SocketReader GetReader(Stream ms, Int32 bufferLength)
        {
            return new SocketReader(ms, bufferLength);
        }

        [TestMethod]
        public void CanReadInt32()
        {
            using (var ms = new MemoryStream())
            using (var reader = GetReader(ms, 20))
            using (var writer = new StreamWriter(ms))
            {
                writer.WriteLine("222");
                writer.WriteLine("333");
                writer.WriteLine(Int32.MaxValue.ToString());
                writer.WriteLine(Int32.MinValue.ToString());
                writer.WriteLine("444");

                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                Assert.AreEqual(222, reader.ReadInt32());
                Assert.AreEqual(333, reader.ReadInt32());
                Assert.AreEqual(Int32.MaxValue, reader.ReadInt32());
                Assert.AreEqual(Int32.MinValue, reader.ReadInt32());
                Assert.AreEqual(444, reader.ReadInt32());
            }
        }

        [TestMethod]
        public void CanReadInt64()
        {
            using (var ms = new MemoryStream())
            using (var reader = GetReader(ms, 21))
            using (var writer = new StreamWriter(ms))
            {
                writer.WriteLine("222");
                writer.WriteLine("333");
                writer.WriteLine(Int64.MaxValue.ToString());
                writer.WriteLine(Int64.MinValue.ToString());
                writer.WriteLine("444");

                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                Assert.AreEqual(222L, reader.ReadInt64());
                Assert.AreEqual(333L, reader.ReadInt64());
                Assert.AreEqual(Int64.MaxValue, reader.ReadInt64());
                Assert.AreEqual(Int64.MinValue, reader.ReadInt64());
                Assert.AreEqual(444L, reader.ReadInt64());
            }
        }

        [TestMethod]
        public void CanReadString()
        {
            var largeString1 = String.Empty.PadLeft(200, 'x');
            var largeString2 = "말\r말말\n말말\r\n말말";
            var largeString3 = String.Empty.PadLeft(20, '말').PadLeft(40, 'ʔ').PadLeft(60, '本');
            largeString3 = largeString3.Insert(10, "\r\n");
            largeString3 = largeString3.Insert(30, "\r\n");
            largeString3 = largeString3.Insert(50, "\r\n");

            for (int x = 1; x <= 25; x++)
            {
                for (int i = 0; i < 25; i++)
                {
                    var padder = String.Empty;
                    for (int y = 1; y <= x; y++)
                        padder += 'A';

                    using (var ms = new MemoryStream())
                    using (var reader = GetReader(ms, 20))
                    using (var writer = new StreamWriter(ms))
                    {
                        for (int j = 0; j < i; j++)
                            writer.WriteLine(padder);

                        writer.WriteLine(largeString1);
                        writer.WriteLine(largeString2);
                        writer.WriteLine(largeString3);

                        writer.Flush();
                        ms.Seek(0, SeekOrigin.Begin);

                        for (int j = 0; j < i; j++)
                            Assert.AreEqual(padder, reader.ReadString(CountUtf8Bytes(padder)));

                        Assert.AreEqual(largeString1, reader.ReadString(CountUtf8Bytes(largeString1)));
                        Assert.AreEqual(largeString2, reader.ReadString(CountUtf8Bytes(largeString2)));
                        Assert.AreEqual(largeString3, reader.ReadString(CountUtf8Bytes(largeString3)));
                    }
                }
            }
        }

        [TestMethod]
        public void CanReadSimpleString()
        {
            var largeString1 = String.Empty.PadLeft(21, 'x');
            var largeString2 = "말말말말말말말";
            var largeString3 = String.Empty.PadLeft(20, '말').PadLeft(40, 'ʔ').PadLeft(60, '本');

            for (int x = 1; x <= 25; x++)
            {
                for (int i = 0; i < 25; i++)
                {
                    var padder = String.Empty;
                    for (int y = 1; y <= x; y++)
                        padder += 'A';

                    using (var ms = new MemoryStream())
                    using (var reader = GetReader(ms, 20))
                    using (var writer = new StreamWriter(ms))
                    {
                        for (int j = 0; j < i; j++)
                            writer.WriteLine(padder);

                        writer.WriteLine(largeString1);
                        writer.WriteLine(largeString2);
                        writer.WriteLine(largeString3);

                        writer.Flush();
                        ms.Seek(0, SeekOrigin.Begin);

                        for (int j = 0; j < i; j++)
                            Assert.AreEqual(padder, reader.ReadString());

                        Assert.AreEqual(largeString1, reader.ReadString());
                        Assert.AreEqual(largeString2, reader.ReadString());
                        Assert.AreEqual(largeString3, reader.ReadString());
                    }
                }
            }
        }

        static Int32 CountUtf8Bytes(String value)
        {
            var count = value.Length;
            for (int i = 0; i < value.Length; i++)
            {
                var c = Char.ConvertToUtf32(value, i);
                if (c < 127)
                    continue;
                else if (c >= 65536)
                    count += 3;
                else if (c >= 2048)
                    count += 2;
                else if (c >= 128)
                    count += 1;
            }
            return count;
        }

        [TestMethod]
        public void CanReadByteAsChar()
        {
            using (var ms = new MemoryStream())
            using (var reader = GetReader(ms, 20))
            using (var writer = new StreamWriter(ms))
            {
                writer.WriteLine("+$*:-55");

                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                Assert.AreEqual('+', reader.ReadRESPHeader());
                Assert.AreEqual('$', reader.ReadRESPHeader());
                Assert.AreEqual('*', reader.ReadRESPHeader());
                Assert.AreEqual(':', reader.ReadRESPHeader());
                Assert.AreEqual('-', reader.ReadRESPHeader());
                Assert.AreEqual(55, reader.ReadInt32());
            }
        }

        [TestMethod]
        public void CanReadMix()
        {
            using (var ms = new MemoryStream())
            using (var reader = GetReader(ms, 20))
            using (var writer = new StreamWriter(ms))
            {
                writer.Write("*4\r\n$2\r\nOK\r\n$2\r\nOK\r\n$3\r\nNOK\r\n$2\r\nOK\r\n:-777\r\n$3\r\nNOK\r\n");

                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                Assert.AreEqual('*', reader.ReadRESPHeader());
                Assert.AreEqual(4, reader.ReadInt32());
                Assert.AreEqual('$', reader.ReadRESPHeader());
                Assert.AreEqual(2, reader.ReadInt32());
                Assert.AreEqual("OK", reader.ReadString(2));
                Assert.AreEqual('$', reader.ReadRESPHeader());
                Assert.AreEqual(2, reader.ReadInt32());
                Assert.AreEqual("OK", reader.ReadString(2));
                Assert.AreEqual('$', reader.ReadRESPHeader());
                Assert.AreEqual(3, reader.ReadInt32());
                Assert.AreEqual("NOK", reader.ReadString(3));
                Assert.AreEqual('$', reader.ReadRESPHeader());
                Assert.AreEqual(2, reader.ReadInt32());
                Assert.AreEqual("OK", reader.ReadString(2));
                Assert.AreEqual(':', reader.ReadRESPHeader());
                Assert.AreEqual(-777L, reader.ReadInt64());
                Assert.AreEqual('$', reader.ReadRESPHeader());
                Assert.AreEqual(3, reader.ReadInt32());
                Assert.AreEqual("NOK", reader.ReadString(3));
            }
        }
    }
}

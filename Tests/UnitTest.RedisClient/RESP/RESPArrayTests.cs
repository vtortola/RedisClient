using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.IO;

namespace UnitTest.RedisClient.Protocol
{
    [TestClass]
    public class RESPArrayTests
    {

        [TestMethod]
        public void CanParseArray()
        {
            var source = new DummySocketReader("3\r\n+Hello\r\n$10\r\nBig\r\nWorld\r\n:1\r\n");
            RESPArray array = RESPArray.Load(source);

            Assert.AreEqual(3, array.Count);
            var item1 = array.ElementAt<RESPSimpleString>(0);
            Assert.AreEqual("Hello", item1.Value);
            var item2 = array.ElementAt<RESPBulkString>(1);
            Assert.AreEqual("Big\r\nWorld", item2.Value);
            var item3 = array.ElementAt<RESPInteger>(2);
            Assert.AreEqual(1L, item3.Value);
            Assert.IsFalse(array.IsNullArray);
        }

        [TestMethod]
        public void CanParseArrayWithBreaks()
        {
            var source = new DummySocketReader("3\r\n+Hello\r\n$12\r\nBig\r\nWorld\r\n\r\n:1\r\n");
            RESPArray array = RESPArray.Load(source);

            Assert.AreEqual(3, array.Count);
            var item1 = array.ElementAt<RESPSimpleString>(0);
            Assert.AreEqual("Hello", item1.Value);
            var item2 = array.ElementAt<RESPBulkString>(1);
            Assert.AreEqual("Big\r\nWorld\r\n", item2.Value);
            var item3 = array.ElementAt<RESPInteger>(2);
            Assert.AreEqual(1L, item3.Value);
            Assert.IsFalse(array.IsNullArray);
        }

        [TestMethod]
        public void CanParseNullArray()
        {
            var source = new DummySocketReader("-1\r\n");
            RESPArray array = RESPArray.Load(source);

            Assert.AreEqual(0, array.Count);
            Assert.IsTrue(array.IsNullArray);
        }

        [TestMethod]
        public void CanParseEmptyArray()
        {
            var source = new DummySocketReader("0\r\n");
            RESPArray array = RESPArray.Load(source);

            Assert.AreEqual(0, array.Count);
            Assert.IsFalse(array.IsNullArray);
        }

        [TestMethod]
        public void CanCreateArrayOfArrays()
        {
            RESPArray nested1 = new RESPArray(new RESPSimpleString("This is level 1"), 
                                              new RESPBulkString("Nested\r\nArray\r\n1"),
                                              new RESPInteger(1));

            RESPArray nested2 = new RESPArray(new RESPSimpleString("This is level 2"),
                                              new RESPBulkString("Nested\r\nArray\r\n2"),
                                              new RESPInteger(2));

            RESPArray array = new RESPArray(nested1, nested2);
        }

        [TestMethod]
        public void CanParseArrayOfArrays()
        {
            var source = new DummySocketReader("2\r\n*3\r\n+This is level 1\r\n$16\r\nNested\r\nArray\r\n1\r\n:1\r\n*3\r\n+This is level 2\r\n$16\r\nNested\r\nArray\r\n2\r\n:2\r\n");
            RESPArray array = RESPArray.Load(source);

            Assert.AreEqual(2, array.Count);
            var nested1 = array.ElementAt<RESPArray>(0);
            Assert.AreEqual(3, nested1.Count);
            var nested2 = array.ElementAt<RESPArray>(1);
            Assert.AreEqual(3, nested2.Count);
        }

        [TestMethod]
        public void CanParseArrayWithNullElement()
        {
            var source = new DummySocketReader("3\r\n$3\r\nfoo\r\n$-1\r\n$3\r\nbar\r\n");
            RESPArray array = RESPArray.Load(source);

            Assert.AreEqual(3, array.Count);
            var item1 = array.ElementAt<RESPBulkString>(0);
            Assert.AreEqual("foo", item1.Value);
            var item2 = array.ElementAt<RESPBulkString>(1);
            Assert.AreEqual(null, item2.Value);
            var item3 = array.ElementAt<RESPBulkString>(2);
            Assert.AreEqual("bar", item3.Value);
            Assert.IsFalse(array.IsNullArray);
        }
    }
}

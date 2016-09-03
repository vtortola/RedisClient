using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.Linq;

namespace UnitTest.RedisClient
{
    [TestClass]
    public class ParserTests
    {

        [TestMethod]
        public void SimpleParsing()
        {
            var result = TextCommandWordParser.Parse("This is an example").ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.AreEqual("is", result[1].Value);
            Assert.AreEqual("an", result[2].Value);
            Assert.AreEqual("example", result[3].Value);
        }

        [TestMethod]
        public void EscapeAt()
        {
            var result = TextCommandWordParser.Parse("This is \\@an example").ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.AreEqual("is", result[1].Value);
            Assert.AreEqual("@an", result[2].Value);
            Assert.AreEqual("example", result[3].Value);
        }

        [TestMethod]
        public void UseAtInNonParameter()
        {
            var result = TextCommandWordParser.Parse("This is a@n example").ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.AreEqual("is", result[1].Value);
            Assert.AreEqual("a@n", result[2].Value);
            Assert.AreEqual("example", result[3].Value);
        }

        [TestMethod]
        public void UseAtInEndOfWord()
        {
            var result = TextCommandWordParser.Parse("This is an@ example").ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.AreEqual("is", result[1].Value);
            Assert.AreEqual("an@", result[2].Value);
            Assert.AreEqual("example", result[3].Value);
        }

        [TestMethod]
        public void IgnoresStandaloneAt()
        {
            var result = TextCommandWordParser.Parse("This is @ example").ToArray();

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.AreEqual("is", result[1].Value);
            Assert.AreEqual("example", result[2].Value);
        }

        [TestMethod]
        public void IgnoresStandaloneDoubleAt()
        {
            var result = TextCommandWordParser.Parse("This is @@ example").ToArray();

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.AreEqual("is", result[1].Value);
            Assert.AreEqual("example", result[2].Value);
        }

        [TestMethod]
        public void IgnoresStandaloneTripleAt()
        {
            var result = TextCommandWordParser.Parse("This is @@@ example").ToArray();

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.AreEqual("is", result[1].Value);
            Assert.AreEqual("example", result[2].Value);
        }

        [TestMethod]
        public void SimpleParsingWithEscapedChars()
        {
            var result = TextCommandWordParser.Parse("This\t\tis\ran\r\nexample").ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.IsFalse(result[0].IsEndOfLine);
            Assert.AreEqual("is", result[1].Value);
            Assert.IsTrue(result[1].IsEndOfLine);
            Assert.AreEqual("an", result[2].Value);
            Assert.IsTrue(result[2].IsEndOfLine);
            Assert.AreEqual("example", result[3].Value);
            Assert.IsTrue(result[3].IsEndOfLine);
        }

        [TestMethod]
        public void ParameterParsing()
        {
            var result = TextCommandWordParser.Parse("This @is an @example").ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.IsFalse(result[0].IsParameter);
            Assert.AreEqual("is", result[1].Value);
            Assert.IsTrue(result[1].IsParameter);
            Assert.AreEqual("an", result[2].Value);
            Assert.IsFalse(result[2].IsParameter);
            Assert.AreEqual("example", result[3].Value);
            Assert.IsTrue(result[3].IsParameter);
        }
        [TestMethod]
        public void ContinueSkippingSpaces()
        {
            var result = TextCommandWordParser.Parse("This @is\t\rtesting.").ToArray();

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.IsFalse(result[0].IsParameter);
            Assert.IsFalse(result[0].IsEndOfLine);

            Assert.AreEqual("is", result[1].Value);
            Assert.IsTrue(result[1].IsParameter);
            Assert.IsTrue(result[1].IsEndOfLine);

            Assert.AreEqual("testing.", result[2].Value);
            Assert.IsFalse(result[2].IsParameter);
            Assert.IsTrue(result[2].IsEndOfLine);
        }


        [TestMethod]
        public void ParametersParsingAndSpecialChars()
        {
            var result = TextCommandWordParser.Parse("This @is\t\ran\t@@example\r\n").ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.IsFalse(result[0].IsParameter);
            Assert.IsFalse(result[0].IsEndOfLine);

            Assert.AreEqual("is", result[1].Value);
            Assert.IsTrue(result[1].IsParameter);
            Assert.IsTrue(result[1].IsEndOfLine);

            Assert.AreEqual("an", result[2].Value);
            Assert.IsFalse(result[2].IsParameter);
            Assert.IsFalse(result[2].IsEndOfLine);

            Assert.AreEqual("example", result[3].Value);
            Assert.IsTrue(result[3].IsParameter);
            Assert.IsTrue(result[3].IsEndOfLine);
        }

        [TestMethod]
        public void UnderstandQuoteContext()
        {
            var result = TextCommandWordParser.Parse("This 'is \n@' 'an@' @example").ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.AreEqual("is \n@", result[1].Value);
            Assert.AreEqual("an@", result[2].Value);
            Assert.AreEqual("example", result[3].Value);
        }

        [TestMethod]
        public void UnderstandDoubleQuoteContext()
        {
            var result = TextCommandWordParser.Parse("This \"is \n@\" \"an@\" @example").ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.AreEqual("is \n@", result[1].Value);
            Assert.AreEqual("an@", result[2].Value);
            Assert.AreEqual("example", result[3].Value);
        }

        [TestMethod]
        public void UnderstandDoubleQuoteInsideSingleQuoteContext()
        {
            var result = TextCommandWordParser.Parse("This 'is \" \n@' 'a\"n@' @example").ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.AreEqual("is \" \n@", result[1].Value);
            Assert.AreEqual("a\"n@", result[2].Value);
            Assert.AreEqual("example", result[3].Value);
        }

        [TestMethod]
        public void UnderstandSingleQuoteInsideDoubleQuoteContext()
        {
            var result = TextCommandWordParser.Parse("This \"is ' \n@\" \"an'@\" @example").ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.AreEqual("is ' \n@", result[1].Value);
            Assert.AreEqual("an'@", result[2].Value);
            Assert.AreEqual("example", result[3].Value);
        }

        [TestMethod]
        public void UnderstandEscapedSingleQuoteInsideSingleQuoteContext()
        {
            var result = TextCommandWordParser.Parse(@"This 'is \' \n@' 'a\'n@' @example").ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("This", result[0].Value);
            Assert.AreEqual("is ' \n@", result[1].Value);
            Assert.AreEqual("a'n@", result[2].Value);
            Assert.AreEqual("example", result[3].Value);
        }
    }
}

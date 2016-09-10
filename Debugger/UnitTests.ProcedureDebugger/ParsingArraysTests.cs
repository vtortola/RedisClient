using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.RedisClient.ProcedureDebugger;
using UnitTests.Common;

namespace UnitTests.ProcedureDebugger
{
    [TestClass]
    public class ParsingArraysTests
    {
        private static void AssertEqualArrays(String[] expected, String[] result)
        {
            if (expected == null)
                Assert.Fail("Expected is null");
            if (result == null)
                Assert.Fail("Result is null");
            if (expected.Length != result.Length)
                Assert.Fail("Expected length is " + expected.Length + " but Result length is " + result.Length);

            for (int i = 0; i < expected.Length; i++)
            {
                if(expected[i] != result[i])
                    Assert.Fail("Expected["+i+"] is " + (expected[i]??"<null>") + " but Result["+i+"] is " + (result[i]??"<null>"));
            }

        }

        [TestMethod]
        public void CanParseSimpleArray()
        {
            var array = SessionModel.ParseArray("[1, 2, 3, 4]", 0);
            AssertEqualArrays(new[] { "1", "2", "3", "4" }, array);
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(SyntaxException), MessagePattern = @"^Array parameter ended unexpectedly: \[1, 2, 3, 4$")]
        public void FailOnOpenArray()
        {
            var array = SessionModel.ParseArray("[1, 2, 3, 4", 0);
        }

        [TestMethod]
        public void CanParseSafeStringArray()
        {
            var array = SessionModel.ParseArray("[\"1\", \"2\", \"3\", \"4\"]", 0);
            AssertEqualArrays(new[] { "1", "2", "3", "4" }, array);
        }

        [TestMethod]
        public void CanParseStringArray()
        {
            var array = SessionModel.ParseArray("[\"1 \", \"2,2\", \"3,,,4\", \"4\"]", 0);
            AssertEqualArrays(new[] { "1 ", "2,2", "3,,,4", "4" }, array);
        }
        [TestMethod]
        public void CanParseSingleQuotesStringArray()
        {
            var array = SessionModel.ParseArray("['1 ', '2,2', '3,,,\"', '\"4']", 0);
            AssertEqualArrays(new[] { "1 ", "2,2", "3,,,\"", "\"4" }, array);
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(SyntaxException), MessagePattern = "^Array parameter ended unexpectedly: \\[\"1 \", \"2,2\", \"3,,,4\", \"4\\]$")]
        public void FailOnOpenArray2()
        {
            var array = SessionModel.ParseArray("[\"1 \", \"2,2\", \"3,,,4\", \"4]", 0);
        }
    }
}

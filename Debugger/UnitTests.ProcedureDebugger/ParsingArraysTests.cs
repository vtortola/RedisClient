using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.RedisClient.ProcedureDebugger;

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

        // Add safe string test cases
    }
}

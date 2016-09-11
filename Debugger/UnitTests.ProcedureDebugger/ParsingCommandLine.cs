using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.RedisClient.ProcedureDebugger;
using UnitTests.Common;

namespace UnitTests.ProcedureDebugger
{
    [TestClass]
    public class ParsingCommandLine
    {
        // add parsing examples
        // with redis-cli additional commands
        // fail when KEYS or ARGV are passed

        [TestMethod]
        public void CanParseCommandLine()
        {
            var session = InputModel.Parse("--file", "mfile.rcpc", "--procedure", "procedurename", "--@p1","p1 value", "--@p2", "p2 value");

            Assert.AreEqual("mfile.rcpc", session.FileName);
            Assert.AreEqual("procedurename", session.Procedure);
            Assert.AreEqual(2, session.Parameters.Count);
            Assert.AreEqual(1, session.Parameters["p1"].Length);
            Assert.AreEqual("p1 value", session.Parameters["p1"][0]);
            Assert.AreEqual(1, session.Parameters["p2"].Length);
            Assert.AreEqual("p2 value", session.Parameters["p2"][0]);
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(SyntaxException), MessagePattern = "^--eval command is forbidden, use --file and --procedure$")]
        public void FailIfEvalIsPresent()
        {
            InputModel.Parse("--file", "mfile.rcpc", "--procedure", "procedurename","--eval","whatever", "--@p1", "p1 value", "--@p2", "p2 value");
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(SyntaxException), MessagePattern = "^--file is mandatory$")]
        public void FailIfMissingFile()
        {
            InputModel.Parse("--procedure", "procedurename", "--@p1", "p1 value", "--@p2", "p2 value");
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(SyntaxException), MessagePattern = "^--procedure is mandatory$")]
        public void FailIfMissingProcedure()
        {
            InputModel.Parse("--file", "mfile.rcpc", "--@p1", "p1 value", "--@p2", "p2 value");
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(SyntaxException), MessagePattern = "^Unexpected command part: whatever$")]
        public void FailIfUnkownElementsArePresent()
        {
            InputModel.Parse("--file", "mfile.rcpc", "--procedure", "procedurename", "whatever", "value");
        }
    }
}

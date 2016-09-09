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
            var session = SessionModel.Parse("--file", "mfile.rcpc", "--procedure", "procedurename", "--@p1","p1 value", "--@p2", "p2 value");

            Assert.AreEqual("mfile.rcpc", session.FileName);
            Assert.AreEqual("procedurename", session.Procedure);
            Assert.AreEqual(2, session.Parameters.Count);
            Assert.AreEqual(1, session.Parameters["p1"].Length);
            Assert.AreEqual("p1 value", session.Parameters["p1"][0]);
            Assert.AreEqual(1, session.Parameters["p2"].Length);
            Assert.AreEqual("p2 value", session.Parameters["p2"][0]);
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(InvalidOperationException), MessagePattern="^--eval command is forbidden, use --file and --procedure$")]
        public void FailIfEvalIsPresent()
        {
            SessionModel.Parse("--file", "mfile.rcpc", "--procedure", "procedurename","--eval","whatever", "--@p1", "p1 value", "--@p2", "p2 value");
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(InvalidOperationException), MessagePattern = "^--file is mandatory$")]
        public void FailIfMissingFile()
        {
            SessionModel.Parse("--procedure", "procedurename", "--@p1", "p1 value", "--@p2", "p2 value");
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(InvalidOperationException), MessagePattern = "^--procedure is mandatory$")]
        public void FailIfMissingProcedure()
        {
            SessionModel.Parse("--file", "mfile.rcpc", "--@p1", "p1 value", "--@p2", "p2 value");
        }
    }
}

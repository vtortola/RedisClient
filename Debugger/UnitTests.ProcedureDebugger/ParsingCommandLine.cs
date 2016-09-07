using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.RedisClient.ProcedureDebugger;

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
            SessionModel.Parse(new String[] { });
        }
    }
}

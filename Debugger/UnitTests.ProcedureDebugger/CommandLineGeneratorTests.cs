using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.RedisClient.ProcedureDebugger;
using System.IO;
using System.Text.RegularExpressions;
using UnitTests.Common;

namespace UnitTests.ProcedureDebugger
{
    [TestClass]
    public class CommandLineGeneratorTests
    {
        static void AssertMatch(String pattern, String actual)
        {
            if(!Regex.IsMatch(actual, pattern))
            {
                throw new AssertFailedException(String.Format("Input has to match '{0}' put '{1}", pattern, actual));
            }
        }

        [TestMethod]
        [DeploymentItem("Files\\procedureExample.rcproc")]
        public void GeneratesCommandLine()
        {
            using(var line = CommandLineGenerator.Generate("--file", "procedureExample.rcproc", "--procedure", "Test1", "--@a", "[1,2]", "--@b", "[3,4]", "--@key", "mykey"))
            {
                AssertMatch("^ --ldb  --eval \\\"[_\\:\\\\A-Za-z0-9]+\\.tmp\\\" \\\"mykey\\\" , \\\"2\\\" \\\"1\\\" \\\"2\\\" \\\"2\\\" \\\"3\\\" \\\"4\\\" $", line.CliArguments);
                Assert.IsTrue(line.CliArguments.Contains(line.TemporaryFile));
            }
        }

        [TestMethod]
        [DeploymentItem("Files\\procedureExample.rcproc")]
        public void GeneratesCommandLine2()
        {
            using (var line = CommandLineGenerator.Generate("--file", "procedureExample.rcproc", "--procedure", "Test2", "--@key1", "2", "--@somekeys", "[3,4]", "--@arg", "x", "--@someargv", "['hi this is', 'just, a test']"))
            {
                AssertMatch("^ --ldb  --eval \\\"[_\\:\\\\A-Za-z0-9]+\\.tmp\\\" \\\"2\\\" \\\"3\\\" \\\"4\\\" , \\\"2\\\" \\\"x\\\" \\\"2\\\" \\\"hi this is\\\" \\\"just, a test\\\" $", line.CliArguments);
                Assert.IsTrue(line.CliArguments.Contains(line.TemporaryFile));
            }
        }

        [TestMethod]
        [DeploymentItem("Files\\procedureExample.rcproc")]
        public void GeneratesCommandLineForParameterlessProcedures()
        {
            using (var line = CommandLineGenerator.Generate("--file", "procedureExample.rcproc", "--procedure", "Test4"))
            {
                AssertMatch("^ --ldb  --eval \\\"[_\\:\\\\A-Za-z0-9]+\\.tmp\\\" , $", line.CliArguments);
                Assert.IsTrue(line.CliArguments.Contains(line.TemporaryFile));
            }
        }
        [TestMethod]
        [DeploymentItem("Files\\procedureExample.rcproc")]
        public void CreatesAndDestroysTempFile()
        {
            var line = CommandLineGenerator.Generate("--file", "procedureExample.rcproc", "--procedure", "Test1", "--@a", "[1,2]", "--@b", "[3,4]", "--@key", "mykey");
            Assert.IsTrue(File.Exists(line.TemporaryFile));
            line.Dispose();
            Assert.IsFalse(File.Exists(line.TemporaryFile));
        }

        [TestMethod]
        [DeploymentItem("Files\\procedureExample.rcproc")]
        [ExpectedExceptionPattern(typeof(SyntaxException), MessagePattern="^Cannot find the specified procedure 'Test3' in 'procedureExample.rcproc'$")]
        public void FailOnNonExistentProcedure()
        {
            var line = CommandLineGenerator.Generate("--file", "procedureExample.rcproc", "--procedure", "Test3", "--@a", "[1,2]", "--@b", "[3,4]", "--@key", "mykey");
        }

        [TestMethod]
        [DeploymentItem("Files\\procedureExample.rcproc")]
        [ExpectedExceptionPattern(typeof(SyntaxException), MessagePattern = "^Command line does not include values for parameter: a$")]
        public void FailOnMissingParameters()
        {
            var line = CommandLineGenerator.Generate("--file", "procedureExample.rcproc", "--procedure", "Test1", "--@b", "[3,4]", "--@key", "mykey");
        }

        [TestMethod]
        [DeploymentItem("Files\\wrongFile.rcproc")]
        [ExpectedExceptionPattern(typeof(SyntaxException), MessagePattern = "^The file \\\"wrongFile\\.rcproc\\\" is not a valid vtortola.RedisClient procedures files or there is a syntax problem in it.$")]
        public void FailOnWrongFile()
        {
            var line = CommandLineGenerator.Generate("--file", "wrongFile.rcproc", "--procedure", "Test1", "--@b", "[3,4]", "--@key", "mykey");
        }
    }
}

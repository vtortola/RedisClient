using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.IO;
using System.Linq;
using UnitTests.Common;

namespace UnitTest.RedisClient.Procedures
{
    [TestClass]
    public class ProcedureParserTests
    {
        [TestMethod]
        public void ParameterLessProcedure()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore()
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();

            Assert.AreEqual("AddStore", procedure.Name);
            Assert.IsNotNull(procedure.Parameters);
            Assert.AreEqual(0, procedure.Parameters.Length);
            Assert.IsNotNull(procedure.Body);
        }

        [TestMethod]
        public void ParameterLessProcedure_WithSpaces()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore(   )
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();

            Assert.AreEqual("AddStore", procedure.Name);
            Assert.IsNotNull(procedure.Parameters);
            Assert.AreEqual(0, procedure.Parameters.Length);
            Assert.IsNotNull(procedure.Body);
        }

        [TestMethod]
        public void BasicTest()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc  AddStore($key, a, b)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();

            Assert.AreEqual("AddStore", procedure.Name);
            Assert.IsNotNull(procedure.Parameters);
            Assert.AreEqual(3, procedure.Parameters.Length);
            Assert.AreEqual("key", procedure.Parameters[0].Name);
            Assert.IsTrue(procedure.Parameters[0].IsKey);
            Assert.IsFalse(procedure.Parameters[0].IsArray);
            Assert.AreEqual("a", procedure.Parameters[1].Name);
            Assert.IsFalse(procedure.Parameters[1].IsKey);
            Assert.IsFalse(procedure.Parameters[1].IsArray);
            Assert.AreEqual("b", procedure.Parameters[2].Name);
            Assert.IsFalse(procedure.Parameters[2].IsKey);
            Assert.IsFalse(procedure.Parameters[2].IsArray);
            Assert.IsNotNull(procedure.Body);
        }

        [TestMethod]
        public void CanPutKeyInAnyPlace()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc  AddStore(a, $key, b)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();

            Assert.AreEqual("AddStore", procedure.Name);
            Assert.IsNotNull(procedure.Parameters);
            Assert.AreEqual(3, procedure.Parameters.Length);
            Assert.AreEqual("key", procedure.Parameters[1].Name);
            Assert.IsTrue(procedure.Parameters[1].IsKey);
            Assert.IsFalse(procedure.Parameters[1].IsArray);
            Assert.AreEqual("a", procedure.Parameters[0].Name);
            Assert.IsFalse(procedure.Parameters[0].IsKey);
            Assert.IsFalse(procedure.Parameters[0].IsArray);
            Assert.AreEqual("b", procedure.Parameters[2].Name);
            Assert.IsFalse(procedure.Parameters[2].IsKey);
            Assert.IsFalse(procedure.Parameters[2].IsArray);
            Assert.IsNotNull(procedure.Body);
        }

        [TestMethod]
        public void BasicTestWithArrays()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                proc  AddStore ($key[],  a,  b[] )
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();

            Assert.AreEqual("AddStore", procedure.Name);
            Assert.IsNotNull(procedure.Parameters);
            Assert.AreEqual(3, procedure.Parameters.Length);
            Assert.AreEqual("key", procedure.Parameters[0].Name);
            Assert.IsTrue(procedure.Parameters[0].IsKey);
            Assert.IsTrue(procedure.Parameters[0].IsArray);
            Assert.AreEqual("a", procedure.Parameters[1].Name);
            Assert.IsFalse(procedure.Parameters[1].IsKey);
            Assert.IsFalse(procedure.Parameters[1].IsArray);
            Assert.AreEqual("b", procedure.Parameters[2].Name);
            Assert.IsFalse(procedure.Parameters[2].IsKey);
            Assert.IsTrue(procedure.Parameters[2].IsArray);
            Assert.IsNotNull(procedure.Body);
        }

        [TestMethod]
        public void MultipleTest()
        {
            var procedures = ProcedureParser.Parse(new StringReader(@"
                   proc  AddStore($key, a, b)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
                proc  AddStore2 ($key[],  a,  b[] )
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).ToArray();

            var procedure = procedures[0];
            Assert.AreEqual("AddStore", procedure.Name);
            Assert.IsNotNull(procedure.Parameters);
            Assert.AreEqual(3, procedure.Parameters.Length);
            Assert.AreEqual("key", procedure.Parameters[0].Name);
            Assert.IsTrue(procedure.Parameters[0].IsKey);
            Assert.IsFalse(procedure.Parameters[0].IsArray);
            Assert.AreEqual("a", procedure.Parameters[1].Name);
            Assert.IsFalse(procedure.Parameters[1].IsKey);
            Assert.IsFalse(procedure.Parameters[1].IsArray);
            Assert.AreEqual("b", procedure.Parameters[2].Name);
            Assert.IsFalse(procedure.Parameters[2].IsKey);
            Assert.IsFalse(procedure.Parameters[2].IsArray);
            Assert.IsNotNull(procedure.Body);

            procedure = procedures[1];
            Assert.AreEqual("AddStore2", procedure.Name);
            Assert.IsNotNull(procedure.Parameters);
            Assert.AreEqual(3, procedure.Parameters.Length);
            Assert.AreEqual("key", procedure.Parameters[0].Name);
            Assert.IsTrue(procedure.Parameters[0].IsKey);
            Assert.IsTrue(procedure.Parameters[0].IsArray);
            Assert.AreEqual("a", procedure.Parameters[1].Name);
            Assert.IsFalse(procedure.Parameters[1].IsKey);
            Assert.IsFalse(procedure.Parameters[1].IsArray);
            Assert.AreEqual("b", procedure.Parameters[2].Name);
            Assert.IsFalse(procedure.Parameters[2].IsKey);
            Assert.IsTrue(procedure.Parameters[2].IsArray);
            Assert.IsNotNull(procedure.Body);
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Empty parameter name is not allowed.")]
        public void FailsOnMissingParameters_1()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore($key, a, )
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Empty parameter name is not allowed.")]
        public void FailsOnMissingParameters_2()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore(, a, b)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Empty parameter name is not allowed.")]
        public void FailsOnMissingParameters_3()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore(, )
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Procedure name must start by a letter.")]
        public void FailsOnWrongProcNaming_1()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc 2AddStore($key, a, b)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Procedure name must start by a letter.")]
        public void FailsOnWrongProcNaming_2()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc _AddStore($key, a, b)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }
        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Procedure name cannot contain spaces.")]
        public void FailsOnWrongProcNaming_3()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddS tore($key, a, b)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }
        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Procedure name cannot contain spaces.")]
        public void FailsOnBadParameterDefinition_1()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore $key, a, b)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }
        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Cannot parse procedure parameters.")]
        public void FailsOnBadParameterDefinition_2()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore ($key, a, b
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }
        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Duplicated parameter names.")]
        public void FailsOnBadParameterDefinition_DuplicateParameters_1()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore ($key, a, a)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }
        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Duplicated parameter names.")]
        public void FailsOnBadParameterDefinition_DuplicateParameters_2()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore ($key, key, a)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }
        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Parameter names must start by a letter.")]
        public void FailsOnWrongKeyParameterNaming_1()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore($1key, a, b)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Invalid character in parameter name ' '")]
        public void FailsOnWrongKeyParameterNaming_2()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore($k ey, a, b)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Unclosed array indicator in parameter.")]
        public void FailsOnWrongKeyParameterNaming_3()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore($key, a, b[)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Invalid character in parameter name ']'")]
        public void FailsOnWrongKeyParameterNaming_4()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore($key, a, b])
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Procedure end found without beginning.")]
        public void FailsOnMissingProc_1()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   poc  AddStore($key, a, b)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Procedure end found without beginning.")]
        public void FailsOnMissingProc_2()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   procAddStore($key, a, b)
                    local result = a + b
                    redis.call('SET', key, result)
                endproc
            ")).Single();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Procedure end missing.")]
        public void FailsOnMissingEndProc_1()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore($key, a, b)
                    local result = a + b
                    redis.call('SET', key, result)
            ")).Single();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientProcedureParsingException), MessagePattern = "Procedure end missing.")]
        public void FailsOnMissingEndProc_2()
        {
            var procedure = ProcedureParser.Parse(new StringReader(@"
                   proc AddStore($key, a, b)
                    local result = a + b
                    redis.call('SET', key, result) endproc
            ")).Single();
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.Linq;
using UnitTests.Common;

namespace UnitTest.RedisClient
{
    [TestClass]
    public class RedisCommandParserTests
    {
        ExecutionPlanner _parser;

        [TestInitialize]
        public void Init()
        {
            _parser = new ExecutionPlanner(new ProcedureCollection());
        }

        [TestMethod]
        public void CanParseSingleCommand()
        {
            var plan = _parser.Build("INCRBY key 1");
            var commands = plan.Bind<Object>(null);
            
            Assert.IsNotNull(plan);
            Assert.AreEqual(1, commands.Length);
            var command = commands[0];
            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("INCRBY", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("key", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("1", command.Cast<RESPCommandLiteral>(2).Value);
        }

        [TestMethod]
        public void CanIgnoreEmptyLines()
        {
            var plan = _parser.Build(@"incrby key 1
                                      ");
            var commands = plan.Bind<Object>(null);

            Assert.IsNotNull(plan);
            Assert.AreEqual(1, commands.Length);
            var command = commands[0];
            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("INCRBY", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("key", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("1", command.Cast<RESPCommandLiteral>(2).Value);
        }

        [TestMethod]
        public void CanParseSingleCommandWithParameters()
        {
            var plan = _parser.Build("INCRBY key @amount");
            var commands = plan.Bind(new { amount = 5 });

            Assert.IsNotNull(plan);
            Assert.AreEqual(1, commands.Length);
            var command = commands[0];
            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("INCRBY", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("key", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("5", command.Cast<RESPCommandValue>(2).Value);
        }

        [TestMethod]
        public void CanParseMultipleCommands()
        {
            var plan = _parser.Build(@"INCRBY keyA 1
                                       DECRBY keyB 33");
            var commands = plan.Bind<Object>(null);

            Assert.IsNotNull(plan);
            Assert.AreEqual(2, commands.Length);
            var command = commands[0];
            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("INCRBY", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("keyA", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("1", command.Cast<RESPCommandLiteral>(2).Value);
            command = commands[1];
            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("DECRBY", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("keyB", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("33", command.Cast<RESPCommandLiteral>(2).Value);
        }

        [TestMethod]
        public void CanParseMultipleCommandsWithBreakN()
        {
            var plan = _parser.Build(@"INCRBY keyA 1\nDECRBY keyB 33");
            var commands = plan.Bind<Object>(null);

            Assert.IsNotNull(plan);
            Assert.AreEqual(2, commands.Length);
            var command = commands[0];
            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("INCRBY", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("keyA", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("1", command.Cast<RESPCommandLiteral>(2).Value);
            command = commands[1];
            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("DECRBY", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("keyB", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("33", command.Cast<RESPCommandLiteral>(2).Value);
        }

        [TestMethod]
        public void CanParseMultipleCommandsWithBreakR()
        {
            var plan = _parser.Build(@"INCRBY keyA 1\rDECRBY keyB 33");
            var commands = plan.Bind<Object>(null);

            Assert.IsNotNull(plan);
            Assert.AreEqual(2, commands.Length);
            var command = commands[0];
            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("INCRBY", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("keyA", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("1", command.Cast<RESPCommandLiteral>(2).Value);
            command = commands[1];
            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("DECRBY", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("keyB", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("33", command.Cast<RESPCommandLiteral>(2).Value);
        }

        [TestMethod]
        public void CanParseMultipleCommandsWithParameters()
        {
            var plan = _parser.Build(@"INCRBY keyA @amount1
                                       DECRBY keyB @amount2");
            var commands = plan.Bind(new { amount1 = 5555, amount2 = 6666 });

            Assert.IsNotNull(plan);
            Assert.AreEqual(2, commands.Length);
            var command = commands[0];
            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("INCRBY", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("keyA", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("5555", command.Cast<RESPCommandValue>(2).Value);
            command = commands[1];
            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("DECRBY", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("keyB", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("6666", command.Cast<RESPCommandValue>(2).Value);
        }

        [TestMethod]
        public void CanParseMultipleCommandsWithCollectionParameters()
        {
            var plan = _parser.Build(@"INCRBY keyA @amount1
                                       DECRBY keyB @amount2
                                       MGET @keys");
            var commands = plan.Bind(new { amount1 = 5555, amount2 = 6666, keys = new[] { "keyA", "keyB" } });

            Assert.IsNotNull(plan);
            Assert.AreEqual(3, commands.Length);
            var command = commands[2];
            Assert.AreEqual(3, command.Count);
            Assert.AreEqual("MGET", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("keyA", command.Cast<RESPCommandValue>(1).Value);
            Assert.AreEqual("keyB", command.Cast<RESPCommandValue>(2).Value);
        }

        [TestMethod]
        public void CanParseSingleCommandWithManyParameters()
        {
            var plan = _parser.Build("MGET key @key1 @key2 @key3 @key4");
            var commands = plan.Bind(new { key1 = 1, key2 = 2, key3 = 3, key4 = 4 });

            Assert.IsNotNull(plan);
            Assert.AreEqual(1, commands.Length);
            var command = commands[0];
            Assert.AreEqual(6, command.Count);
            Assert.AreEqual("MGET", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("key", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("1", command.Cast<RESPCommandValue>(2).Value);
            Assert.AreEqual("2", command.Cast<RESPCommandValue>(3).Value);
            Assert.AreEqual("3", command.Cast<RESPCommandValue>(4).Value);
            Assert.AreEqual("4", command.Cast<RESPCommandValue>(5).Value);
        }

        [TestMethod]
        public void CanParseSingleCommandWithRepeatedParameters()
        {
            var plan = _parser.Build("MGET key @key1 @key1 @key1 @key2");
            var commands = plan.Bind(new { key1 = 1, key2 = 2 });

            Assert.IsNotNull(plan);
            Assert.AreEqual(1, commands.Length);
            var command = commands[0];
            Assert.AreEqual(6, command.Count);
            Assert.AreEqual("MGET", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("key", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("1", command.Cast<RESPCommandValue>(2).Value);
            Assert.AreEqual("1", command.Cast<RESPCommandValue>(3).Value);
            Assert.AreEqual("1", command.Cast<RESPCommandValue>(4).Value);
            Assert.AreEqual("2", command.Cast<RESPCommandValue>(5).Value);
        }

        [TestMethod]
        public void CanParseSingleCommandWithComplexParameters()
        {
            var plan = _parser.Build("HMSET key @values");
            var commands = plan.Bind(new { values = Parameter.SequenceProperties(new { Name="Valeriano", Surname="Tortola"}) });

            Assert.IsNotNull(plan);
            Assert.AreEqual(1, commands.Length);
            var command = commands[0];
            Assert.AreEqual(6, command.Count);
            Assert.AreEqual("HMSET", command.Cast<RESPCommandLiteral>(0).Value);
            Assert.AreEqual("key", command.Cast<RESPCommandLiteral>(1).Value);
            Assert.AreEqual("Name", command.Cast<RESPCommandValue>(2).Value);
            Assert.AreEqual("Valeriano", command.Cast<RESPCommandValue>(3).Value);
            Assert.AreEqual("Surname", command.Cast<RESPCommandValue>(4).Value);
            Assert.AreEqual("Tortola", command.Cast<RESPCommandValue>(5).Value);
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientBindingException), MessagePattern="^The parameter key 'amount' does not exists in the given parameter data.$")]
        public void FailsOnMissingParameters()
        {
            var plan = _parser.Build("INCRBY key @amount");
            var commands = plan.Bind<Object>(new Object());
        }
    }
}

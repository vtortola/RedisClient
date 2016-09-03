using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UnitTest.RedisClient.Api
{
    [TestClass]
    public class RedisChannelTests
    {
        ProcedureCollection _procedures;
        Mock<ICommandConnection> _multiplex;
        Mock<IConnectionProvider<ICommandConnection>> _pool;
        Mock<IConnectionProvider<ISubscriptionConnection>> _subscriber;
        RedisChannel _channel;

        [TestInitialize]
        public void Init()
        {
            _procedures = ProcedureCollection.Empty;
            _multiplex = new Mock<ICommandConnection>();
            _multiplex.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult<Object>(null));
            _pool = new Mock<IConnectionProvider<ICommandConnection>>();
            _pool.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult<Object>(null));
            _subscriber = new Mock<IConnectionProvider<ISubscriptionConnection>>();
            _subscriber.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult<Object>(null));
            _channel = new RedisChannel(new ExecutionPlanner(_procedures), _procedures, _multiplex.Object, _subscriber.Object, _pool.Object);
        }

        private Mock<ICommandConnection> MultiplexSetup()
        {
            _multiplex.Setup(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>())).Verifiable();
            _multiplex.Setup(x => x.Dispose()).Throws(new Exception("Do not dispose multiplexed connections."));
            _pool.Setup(x => x.Provide()).Throws(new Exception("Exclusive pool should not be used."));
            return _multiplex;
        }

        private Mock<ICommandConnection> ExclusivePoolSetup()
        {
            var connection = new Mock<ICommandConnection>();
            connection.Setup(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()))
                      .Callback((ExecutionToken t, CancellationToken c) => t.SetCompleted())
                      .Verifiable();
            connection.Setup(x => x.Dispose()).Verifiable();
            _pool.Setup(x => x.Provide()).Returns(connection.Object);
            _multiplex.Setup(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            return connection;
        }

        [TestMethod]
        public void RunWholeTransactionMultiplexed()
        {
            var connection = MultiplexSetup();

            _channel.ExecuteAsync(@"
                        MULTI
                        INCR X
                        EXEC");

            connection.Verify();
        }

        [TestMethod]
        public void RunWholeDiscardedTransactionMultiplexed()
        {
            var connection = MultiplexSetup();

            _channel.ExecuteAsync(@"
                        MULTI
                        INCR X
                        DISCARD");

            connection.Verify();
        }

        [TestMethod]
        public void RunWholeWatchedTransactionMultiplexed()
        {
            var connection = MultiplexSetup();

            _channel.ExecuteAsync(@"
                        WATCH whatever
                        MULTI
                        INCR X
                        EXEC");

            connection.Verify();
        }

        [TestMethod]
        public void RunWholeDiscardedWathedTransactionMultiplexed()
        {
            var connection = MultiplexSetup();

            _channel.ExecuteAsync(@"
                        WATCH whatever
                        MULTI
                        INCR X
                        DISCARD");

            connection.Verify();
        }

        [TestMethod]
        public void RunWholeUnWatchedTransactionMultiplexed()
        {
            var connection = MultiplexSetup();

            _channel.ExecuteAsync(@"
                        WATCH whatever
                        INCR X
                        UNWATCH");

            connection.Verify();
        }

        [TestMethod]
        public void ReusesStandaloneConnectionOnMulti()
        {
            var connection = ExclusivePoolSetup();

            _channel.Execute(@"
                        MULTI
                        INCR X");

            _channel.Execute(@"
                        INCR X");

            _channel.Execute(@"
                        INCR X
                        EXEC");

            connection.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            connection.Verify(x => x.Dispose(), Times.Once);
        }

        [TestMethod]
        public void ReusesStandaloneConnectionOnOpenWatch()
        {
            var connection = ExclusivePoolSetup();

            _channel.Execute(@"
                        WATCH
                        INCR X");

            _channel.Execute(@"
                        INCR X");

            _channel.Execute(@"
                        INCR X
                        UNWATCH");

            connection.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            connection.Verify(x => x.Dispose(), Times.Once);
        }

        [TestMethod]
        public void StandaloneConnectionForBlocking()
        {
            var connection = ExclusivePoolSetup();

            _channel.Execute(@"BRPOP X");

            connection.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            connection.Verify(x => x.Dispose(), Times.Once);
        }

        [TestMethod]
        public void SwitchesBetweenConnections()
        {
            var standalone = new Mock<ICommandConnection>();
            standalone.Setup(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()))
                      .Callback((ExecutionToken t, CancellationToken c) => t.SetCompleted())
                      .Verifiable();
            standalone.Setup(x => x.Dispose()).Verifiable();
            _pool.Setup(x => x.Provide()).Returns(standalone.Object);

            _multiplex.Setup(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()))
                      .Callback((ExecutionToken t, CancellationToken c) => t.SetCompleted())
                      .Verifiable();
            _multiplex.Setup(x => x.Dispose()).Throws(new Exception("Do not dispose multiplexed connections."));
            
            _channel.Execute(@"
                        INCR X");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(0));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

            _channel.Execute(@"
                        WATCH X
                        GET X");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

            _channel.ExecuteAsync(@"
                        MULTI
                        INCR X
                        EXEC");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            standalone.Verify(x => x.Dispose(), Times.Once);

            _channel.Execute(@"
                        INCR X");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [TestMethod]
        public void SwitchesBetweenConnectionsEvenWithUnwatch()
        {
            var standalone = new Mock<ICommandConnection>();
            standalone.Setup(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()))
                      .Callback((ExecutionToken t, CancellationToken c) => t.SetCompleted())
                      .Verifiable();
            standalone.Setup(x => x.Dispose()).Verifiable();
            _pool.Setup(x => x.Provide()).Returns(standalone.Object);

            _multiplex.Setup(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()))
                      .Callback((ExecutionToken t, CancellationToken c) => t.SetCompleted())
                      .Verifiable();
            _multiplex.Setup(x => x.Dispose()).Throws(new Exception("Do not dispose multiplexed connections."));

            _channel.Execute(@"
                        INCR X");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(0));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

            _channel.Execute(@"
                        WATCH X
                        GET X");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

            _channel.ExecuteAsync(@"
                        MULTI
                        INCR X");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

            _channel.Execute(@"
                        UNWATCH");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));


            _channel.Execute(@"
                        INCR X
                        EXEC");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            standalone.Verify(x => x.Dispose(), Times.Once);

            _channel.Execute(@"
                        INCR X");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [TestMethod]
        public void SwitchesBetweenConnectionsWithBlocking()
        {
            var standalone = new Mock<ICommandConnection>();
            standalone.Setup(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()))
                      .Callback((ExecutionToken t, CancellationToken c) => t.SetCompleted())
                      .Verifiable();
            standalone.Setup(x => x.Dispose()).Verifiable();
            _pool.Setup(x => x.Provide()).Returns(standalone.Object);

            _multiplex.Setup(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()))
                      .Callback((ExecutionToken t, CancellationToken c) => t.SetCompleted())
                      .Verifiable();
            _multiplex.Setup(x => x.Dispose()).Throws(new Exception("Do not dispose multiplexed connections."));

            _channel.Execute(@"INCR X");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(0));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

            _channel.Execute(@"BLPOP X");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            standalone.Verify(x => x.Dispose(), Times.Once);

            _channel.Execute(@"INCR X");

            standalone.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            _multiplex.Verify(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [TestMethod]
        public void FixUnfinishedTransaction()
        {
            var connection = ExclusivePoolSetup();

            var tokens = new List<ExecutionToken>();
            connection.Setup(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()))
                      .Callback((ExecutionToken token, CancellationToken cancel) =>
                      {
                          tokens.Add(token);
                      });

            _channel.ExecuteAsync(@"
                        MULTI
                        INCR X");

            _channel.Dispose();

            Assert.AreEqual(2, tokens.Count);
            Assert.IsInstanceOfType(tokens[1].CommandOperation, typeof(DiscardTransactionOperation));
        }

        [TestMethod]
        public void FixUnfinishedWatch()
        {
            var connection = ExclusivePoolSetup();

            var tokens = new List<ExecutionToken>();
            connection.Setup(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()))
                      .Callback((ExecutionToken token, CancellationToken cancel) =>
                      {
                          tokens.Add(token);
                      });

            _channel.ExecuteAsync(@"
                        WATCH
                        INCR X");

            _channel.Dispose();

            Assert.AreEqual(2, tokens.Count);
            Assert.IsInstanceOfType(tokens[1].CommandOperation, typeof(UnwatchOperation));
        }

        [TestMethod]
        public void FixUnfinishedWatchAndTransaction()
        {
            var connection = ExclusivePoolSetup();

            var tokens = new List<ExecutionToken>();
            connection.Setup(x => x.Execute(It.IsAny<ExecutionToken>(), It.IsAny<CancellationToken>()))
                      .Callback((ExecutionToken token, CancellationToken cancel) =>
                      {
                          tokens.Add(token);
                      });

            _channel.ExecuteAsync(@"
                        WATCH");

            _channel.ExecuteAsync(@"
                        MULTI
                        INCR X");

            _channel.Dispose();

            Assert.AreEqual(3, tokens.Count);
            Assert.IsInstanceOfType(tokens[2].CommandOperation, typeof(DiscardTransactionOperation));
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class ConnectionPool : IConnectionProvider<ICommandConnection>
    {
        readonly BlockingCollection<ICommandConnection> _queue;
        readonly Func<ICommandConnection> _factory;
        readonly ICommandConnection[] _connections;
        readonly CancellationTokenSource _cancel;
        readonly IRedisClientLog _logger;

        Int32 _current;
        Boolean _disposed;

        internal ConnectionPool(Int32 minimum, Int32 maximum, Func<ICommandConnection> factory, IRedisClientLog logger)
        {
            Contract.Assert(minimum >= 0, "Minimum must be bigger or equal than 0");
            Contract.Assert(minimum <= maximum, "Minimum cannot be bigger than Maximum");

            _factory = factory;
            _queue = new BlockingCollection<ICommandConnection>(maximum);
            _connections = new ICommandConnection[maximum];
            _current = minimum;
            _logger = logger;
            _cancel = new CancellationTokenSource();

            for (int i = 0; i < minimum; i++)
            {
                var connection = factory();
                _connections[i] = connection;
                _queue.Add(connection);
            }
        }

        public async Task ConnectAsync(CancellationToken cancel)
        {
            var connections = _connections.Where((c, i) => i < _current).ToArray();
            await Task.WhenAll(connections.Select(c => c.ConnectAsync(cancel))).ConfigureAwait(false);
        }

        public ICommandConnection Provide()
        {
            ICommandConnection connection = null;
            if (!_queue.TryTake(out connection))
            {
                if (_current < _queue.BoundedCapacity) // first checks to avoid pointless increments
                {
                    var current = Interlocked.Increment(ref _current);

                    if (current < _queue.BoundedCapacity)
                    {
                        connection = _factory();
                        _connections[current] = connection;
                        _logger.Info("Connection Pool failed to produce a queued connection, so a new one is created. Current count {0}", current);

                        // todo: synchronous .Connect() method?
                        connection.ConnectAsync(_cancel.Token).Wait();
                    }
                }

                // spin another transient connection rather than block?

                if(connection == null) // it could not create a new connection because pool reached maximum
                {
                    _logger.Info("Connection Pool failed to produce a queued connection and maximum {0} has been reached. Waiting for connection...", _queue.BoundedCapacity);
                    // if cannot add more connections, block until 
                    // an available one is in the queue.
                    connection = _queue.Take(_cancel.Token);
                }
            }

            return new PooledConnection(connection, () => Return(connection));
        }

        private void Return(ICommandConnection connection)
        {
            if(!_disposed)
                _queue.Add(connection, _cancel.Token);
        }

        public void Dispose()
        {
            _disposed = true;
            _cancel.Cancel();
            _cancel.Dispose();
            _queue.Dispose();
            for (int i = 0; i < _connections.Length; i++)
            {
                DisposeHelper.SafeDispose(_connections[i]);
            }
        }
    }
}

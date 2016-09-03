using System;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal sealed class PooledConnection : ICommandConnection
    {
        readonly ICommandConnection _inner;
        readonly Action _returnConnection;

        Int32 _disposed;

        internal PooledConnection(ICommandConnection inner, Action returnConnection)
        {
            _inner = inner;
            _returnConnection = returnConnection;
        }

        public Task ConnectAsync(CancellationToken cancel)
        {
            return _inner.ConnectAsync(cancel);
        }

        public void Execute(ExecutionToken token, CancellationToken cancel)
        {
            _inner.Execute(token, cancel);
        }

        public void Dispose()
        {
            // when disposing, returns the connection to the pool once

            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
                _returnConnection();
            else
                throw new ObjectDisposedException("This pooled connection has been already disposed.");
        }
    }
}

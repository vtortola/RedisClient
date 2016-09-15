using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal sealed class ConnectionSelector<TConnection> : IConnectionProvider<TConnection>, IReadOnlyList<TConnection>
        where TConnection : IConnection, ILoadMeasurable
    {
        readonly TConnection[] _connections;
        readonly ILoadBasedSelector _selector;
        readonly RedisClientOptions _options;

        internal ConnectionSelector(Int32 count, Func<TConnection> factory, RedisClientOptions options)
        {
            _connections = Enumerable
                                .Range(0, count)
                                .Select(i => factory())
                                .ToArray();

            _selector = options.LoadBasedSelector;
            _options = options;
        }

        public TConnection this[int index]
        {
            get { return _connections[index]; }
        }

        public Int32 Count
        {
            get { return _connections.Length; }
        }

        public TConnection Provide()
        {
            return _selector.Select(_connections);
        }

        public IEnumerator<TConnection> GetEnumerator()
        {
            return (IEnumerator<TConnection>)_connections.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _connections.GetEnumerator();
        }

        public async Task ConnectAsync(CancellationToken cancel)
        {
            _options.Logger.Info("Initiating pool with {0} connections ...", _connections.Length);
            var task = await Task.WhenAny(_connections.Select(c => c.ConnectAsync(cancel))).ConfigureAwait(false);
            await task.ConfigureAwait(false);
        }

        public void Dispose()
        {
            for (int i = 0; i < _connections.Length; i++)
            {
                _connections[i].Dispose();
            }
        }
    }
}

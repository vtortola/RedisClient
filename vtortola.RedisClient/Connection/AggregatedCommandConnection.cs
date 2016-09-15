using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal sealed class AggregatedCommandConnection<TConnection> : ICommandConnection
        where TConnection : ICommandConnection, ILoadMeasurable
    {
        readonly TConnection[] _commanders;
        readonly ILoadBasedSelector _selector;
        readonly RedisClientOptions _options;

        public ProcedureCollection Procedures { get; private set; }

        internal AggregatedCommandConnection(Int32 count, Func<TConnection> factory, RedisClientOptions options, ProcedureCollection procedures)
        {
            Contract.Assert(options != null, "Redis optiosn cannot be null.");
            Contract.Assert(procedures != null, "Procedures cannot be null.");

            Procedures = procedures;

            _commanders = Enumerable
                            .Range(0, count)
                            .Select(i => factory())
                            .ToArray();

            _selector = options.LoadBasedSelector;
            _options = options;
        }

        public void Execute(ExecutionToken token, CancellationToken cancel)
        {
            _selector.Select(_commanders).Execute(token, cancel);
        }

        public async Task ConnectAsync(CancellationToken cancel)
        {
            _options.Logger.Info("Initiating {0} commander connections ...", _commanders.Length);
            var task = await Task.WhenAny(_commanders.Select(c => c.ConnectAsync(cancel))).ConfigureAwait(false);
            await task.ConfigureAwait(false);
        }

        public void Dispose()
        {
            for (int i = 0; i < _commanders.Length; i++)
            {
                DisposeHelper.SafeDispose(_commanders[i]);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class ExecutionPlan : IReadOnlyList<CommandBinder>
    {
        readonly IList<CommandBinder> _binders;

        internal String[] Headers { get; private set; }
        internal Boolean IsTransaction { get; private set; }
        internal Boolean HasScripts { get; private set; }
        internal ExecutionContextOperation ContextOperation { get; private set; }

        internal ExecutionPlan(IList<CommandBinder> binders)
        {
            Contract.Assert(binders.Any(), "Execution plan needs a list of binders, one per command line.");

            _binders = binders;
            Headers = binders.Select(l => l.Header.Value).ToArray();
            IsTransaction = Headers.Contains("MULTI", StringComparer.Ordinal);
            HasScripts = Headers.Contains("EVALSHA", StringComparer.Ordinal);
            ContextOperation = ExecutionContextOperation.Parse(Headers);
        }

        internal RESPCommand[] Bind<T>(T parameters = null) where T:class
        {
            var result = new RESPCommand[_binders.Count];

            for (var i = 0; i < _binders.Count; i++)
                result[i] = _binders[i].Bind(parameters);

            return result;
        }

        public CommandBinder this[Int32 index] { get { return _binders[index]; } }

        public Int32 Count { get { return _binders.Count; } }

        public IEnumerator<CommandBinder> GetEnumerator()
        {
            return _binders.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _binders.GetEnumerator();
        }
    }
}

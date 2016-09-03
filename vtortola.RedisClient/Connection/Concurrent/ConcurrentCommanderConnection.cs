using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading;

namespace vtortola.Redis
{
    internal sealed class ConcurrentCommanderConnection : ConcurrentConnection, ICommandConnection
    {       
        readonly RedisClientOptions _options;

        ExecutionToken _current;

        public override Int32 CurrentLoad { get { return base.PendingCount * LoadFactor; } }

        internal ConcurrentCommanderConnection(IPEndPoint[] endpoints, RedisClientOptions options, ProcedureCollection procedures)
            :base(endpoints, options)
        {
            _options = options;

            Initializers.Add(new ScriptInitialization(procedures, options.Logger));
        }

        protected internal override ExecutionToken GeneratePingToken()
        {
            return new NoWaitExecutionToken(new PingCommanderOperation(), null);
        }

        protected override IEnumerable<RESPCommand> ExecuteOperation(ExecutionToken token, CancellationToken cancel)
        {
            return token.CommandOperation.Execute();
        }

        protected override void ProcessResponse(RESPObject response, ConcurrentQueue<ExecutionToken> pending, CancellationToken cancel)
        {
            if (response == null)
            {
                if (_current != null)
                {
                    _current.SetCancelled();
                    _current = null;
                }

                return;
            }

            if (_current == null && !pending.TryDequeue(out _current))
                throw new InvalidOperationException("Received response but no token available.");

            _current.CommandOperation.HandleResponse(response);
            if (_current.CommandOperation.IsCompleted)
            {
                if (!_current.IsCancelled)
                {
                    cancel.ThrowIfCancellationRequested();
                    _current.SetCompleted();
                }
                _current = null;
            }
        }
    }
}

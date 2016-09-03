using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace vtortola.Redis
{
    internal sealed class CommanderConnection : Connection, ICommandConnection
    {
        ExecutionToken _current;

        public CommanderConnection(IPEndPoint[] endpoints, RedisClientOptions options, ProcedureCollection procedures)
            :base(endpoints, options)
        {
            Initializers.Add(new ScriptInitialization(procedures, options.Logger));
        }

        protected override IEnumerable<RESPCommand> ExecuteOperation(ExecutionToken token, CancellationToken cancel)
        {
            return token.CommandOperation.Execute();
        }

        protected override void OnConnection()
        {
            _current = null;
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

        protected internal override ExecutionToken GeneratePingToken()
        {
            return new NoWaitExecutionToken(new PingCommanderOperation(), null);
        }
    }
}

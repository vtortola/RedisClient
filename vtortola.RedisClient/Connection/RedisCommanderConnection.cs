using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal sealed class RedisCommanderConnection : RedisConnection, ICommandConnection
    {
        ExecutionToken _current;

        internal RedisCommanderConnection(IPEndPoint[] endpoints, RedisClientOptions options, ProcedureCollection procedures)
            :base(endpoints, options)
        {
            Initializers.Add(new ScriptInitialization(procedures, options.Logger));
        }

        protected override void OnDisconnection()
        {
            if (_current != null)
            {
                _current.SetCancelled();
                _current = null;
            }
        }

        protected internal override ExecutionToken GeneratePingToken()
        {
            return new NoWaitExecutionToken(new PingCommanderOperation(), null);
        }

        protected override void ProcessResponse(RESPObject response)
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

            if (_current == null && !_pending.TryDequeue(out _current))
                throw new InvalidOperationException("Received command response but no token available.");

            try
            {
                HandleResponseWithToken(response);
            }
            catch(OperationCanceledException)
            {
                _current.SetCancelled();
                throw;
            }
            catch (Exception ex)
            {
                _current.SetFaulted(ex);
                throw;
            }
        }

        private void HandleResponseWithToken(RESPObject response)
        {
            _current.CommandOperation.HandleResponse(response);
            if (_current.CommandOperation.IsCompleted)
            {
                if (!_current.IsCancelled)
                {
                    _connectionCancellation.Token.ThrowIfCancellationRequested();
                    _current.SetCompleted();
                }
                _current = null;
            }
        }

        protected override IEnumerable<RESPCommand> ExecuteOperation(ExecutionToken token)
        {
            return token.CommandOperation.Execute();
        }
    }
}

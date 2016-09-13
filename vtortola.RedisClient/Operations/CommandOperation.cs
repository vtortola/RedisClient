using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal class CommandOperation : ICommandOperation
    {
        readonly RESPCommand[] _commands;
        readonly RESPObject[] _responses;
        readonly ProcedureCollection _procedures;

        Int32 _nextResponse = -1;

        public Boolean IsCompleted { get { return _nextResponse >= _responses.Length; } }

        internal CommandOperation(RESPCommand[] commands, RESPObject[] responses, ProcedureCollection procedures)
        {
            Contract.Assert(commands.Any(), "Creating operation with empty command list.");
            Contract.Assert(responses.Any(), "Creating operation with empty responses lsit.");
            Contract.Assert(procedures != null, "Creating operation with empty procedure list.");
            Contract.Assert(commands.Length == responses.Length, "Commands and responses placeholder have different lenghts.");

            _commands = commands;
            _responses = responses;
            _procedures = procedures;

            PointToNextResponse();
        }

        private void PointToNextResponse()
        {
            _nextResponse++;
            for (; _nextResponse < _commands.Length; _nextResponse++)
            {
                var command = _commands[_nextResponse];
                if (!command.IsSubscription)
                    break;
            }
        }

        public IEnumerable<RESPCommand> Execute()
        {
            return _commands.Where(c => !c.IsSubscription);
        }

        public void HandleResponse(RESPObject response)
        {
            _responses[_nextResponse] = response;
            PointToNextResponse();
        }
    }
}

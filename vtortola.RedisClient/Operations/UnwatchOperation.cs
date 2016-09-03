using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal sealed class UnwatchOperation : ICommandOperation
    {
        static readonly RESPCommand[] _command = new [] { new RESPCommand(new RESPCommandLiteral("UNWATCH"), false) };
        public Boolean IsCompleted { get; private set; }

        public IEnumerable<RESPCommand> Execute()
        {
            return _command;
        }

        public void HandleResponse(RESPObject response)
        {
            Contract.Assert(response.ToString() == "OK", "UnwatchOperation handler trying to handle a non OK response. ");

            IsCompleted = true;
        }
    }
}

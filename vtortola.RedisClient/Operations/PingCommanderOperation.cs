using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal sealed class PingCommanderOperation : ICommandOperation
    {
        static readonly RESPCommand[] _ping = new [] { new RESPCommand(new RESPCommandLiteral("PING"), false) };
        public Boolean IsCompleted { get; private set; }

        public IEnumerable<RESPCommand> Execute()
        {
            return _ping;
        }

        public void HandleResponse(RESPObject response)
        {
            Contract.Assert(response.ToString() == "PONG", "PingCommanderOperation handler trying to work out a non-ping response. ");

            IsCompleted = true;
        }
    }
}

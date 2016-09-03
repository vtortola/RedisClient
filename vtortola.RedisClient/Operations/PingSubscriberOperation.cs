using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal sealed class PingSubscriberOperation : ISubscriptionOperation
    {
        static readonly RESPCommand[] _ping = new [] { new RESPCommand(new RESPCommandLiteral("PING"), true) };

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

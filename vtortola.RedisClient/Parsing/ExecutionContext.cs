using System;
using System.Threading;

namespace vtortola.Redis
{
    internal sealed class ExecutionContext
    {
        internal Boolean IsTransactionOngoing { get; private set; }
        internal Boolean IsWatchOnGoing { get; set; }
        internal Boolean MustKeepConnection { get { return IsTransactionOngoing || IsWatchOnGoing; } }

        internal void Apply(ExecutionContextOperation next)
        {
            IsTransactionOngoing = (IsTransactionOngoing && !next.ClosesTransaction) || next.OpensTransaction;
            IsWatchOnGoing = (IsWatchOnGoing && !next.ClosesWatch) || next.OpensWatch;
        }

    }
}

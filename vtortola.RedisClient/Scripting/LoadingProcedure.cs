using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class LoadingProcedure
    {
        internal ProcedureDefinition Procedure { get; private set; }
        internal RESPCommand Command { get; private set; }

        internal LoadingProcedure(ProcedureDefinition procedure, RESPCommand command)
        {
            Procedure = procedure;
            Command = command;
        }
    }
}

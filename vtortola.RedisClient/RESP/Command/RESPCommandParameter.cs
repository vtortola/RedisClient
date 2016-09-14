using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace vtortola.Redis
{
    [DebuggerDisplay("<{Value}>")]
    internal sealed class RESPCommandParameter : RESPCommandPart
    {
        internal RESPCommandParameter(String parameterName)
            :base(parameterName, true)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(parameterName), "Parameter name cannot be empty.");
        }

        // TODO: SOLID.LSP broken?
        // this is just a marker to be replaced
        internal override void WriteTo(SocketWriter writer)
        {
            throw new NotImplementedException("Parameters are replaced during parameter binding, and this method should be never called.");
        }
    }
}

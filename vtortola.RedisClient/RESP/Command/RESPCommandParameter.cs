using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace vtortola.Redis
{
    [DebuggerDisplay("<{Value}>")]
    internal sealed class RESPCommandParameter : RESPCommandPart
    {
        String _value;
        internal override String Value { get { return _value; } }

        internal RESPCommandParameter(String parameterName)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(parameterName), "Parameter name cannot be empty.");

            _value = parameterName;
        }

        // TODO: SOLID.LSP broken?
        internal override void WriteTo(SocketWriter writer)
        {
            throw new NotImplementedException("Parameters are replaced during parameter binding, and this method should be never called.");
        }
    }
}

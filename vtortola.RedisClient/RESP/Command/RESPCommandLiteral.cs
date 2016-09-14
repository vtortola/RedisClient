using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace vtortola.Redis
{
    [DebuggerDisplay("{Value}")]
    internal sealed class RESPCommandLiteral : RESPCommandPart
    {
        readonly String _resp;

        internal RESPCommandLiteral(String value)
            :base(value)
        {
            if (value == null)
            {
                _resp = _empty;
                return;
            }

            _resp = "$" + CountBytes(Value).ToString() + "\r\n" + value + "\r\n";
        }

        internal override void WriteTo(SocketWriter writer)
        {
            writer.Write(_resp);
        }
    }

}

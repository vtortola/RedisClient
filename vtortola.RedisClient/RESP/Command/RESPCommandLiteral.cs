using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace vtortola.Redis
{
    [DebuggerDisplay("{Value}")]
    internal sealed class RESPCommandLiteral : RESPCommandPart
    {
        readonly String _resp;
        readonly String _value;
        internal override String Value { get { return _value; } }


        internal RESPCommandLiteral(String value)
        {
            _value = value;

            if (value == null)
            {
                _resp = _empty;
                return;
            }

            _resp = "$" + CountBytes(Value).ToString() + "\r\n" + _value;
        }

        internal override void WriteTo(SocketWriter writer)
        {
            writer.WriteLine(_resp);
        }
    }

}

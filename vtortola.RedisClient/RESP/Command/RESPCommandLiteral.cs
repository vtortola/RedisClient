using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace vtortola.Redis
{
    [DebuggerDisplay("{Value}")]
    internal sealed class RESPCommandLiteral : RESPCommandPart
    {
        readonly Char[] _value;
        String _svalue;
        internal override String Value { get { return _svalue; } }


        internal RESPCommandLiteral(String value)
        {
            _svalue = value;

            if (value == null)
            {
                _value = _empty;
                return;
            }

            var scount = CountBytes(Value).ToString();
            _value = new Char[1 + scount.Length + 2 + value.Length + 2];

            var index = 0;
            _value[index++] = RESPHeaders.BulkString;

            for (int i = 0; i < scount.Length; i++)
                _value[index++] = scount[i];

            _value[index++] = CRChar;
            _value[index++] = LFChar;

            for (int i = 0; i < value.Length; i++)
                _value[index++] = value[i];

            _value[index++] = CRChar;
            _value[index++] = LFChar;

            Contract.Assert(_value.Length == index, "Literal construction failed.");
        }

        internal override void WriteTo(SocketWriter writer)
        {
            writer.Write(_value);
        }
    }

}

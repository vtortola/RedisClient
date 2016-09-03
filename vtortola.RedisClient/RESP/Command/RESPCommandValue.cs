using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    [DebuggerDisplay("{Value}")]
    internal sealed class RESPCommandValue : RESPCommandPart
    {
        static readonly Char[] _tail = new[] { CRChar, LFChar };
                
        readonly Char[] _header;

        String _value;
        internal override String Value { get { return _value; } }
        
        public RESPCommandValue(String value)
        {
            _value = value;

            if (value != null)
            {
                var scount = CountBytes(value).ToString();
                _header = new Char[1 + scount.Length + 2];
                var index = 0;
                _header[index++] = RESPHeaders.BulkString;
                for (var i = 0; i < scount.Length; i++)
                    _header[index++] = scount[i];
                _header[index++] = CRChar;
                _header[index++] = LFChar;

                Contract.Assert(_header.Length == index, "Value header construction failed.");
            }
        }

        internal override void WriteTo(SocketWriter writer)
        {
            if (Value == null)
            {
                writer.Write(_empty);
            }
            else
            {
                writer.Write(_header);
                writer.Write(Value);
                writer.Write(_tail);
            }
        }
    }
}

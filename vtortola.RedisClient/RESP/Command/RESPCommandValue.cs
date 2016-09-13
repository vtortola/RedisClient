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
        String _value;
        String _scount;
        internal override String Value { get { return _value; } }
        
        public RESPCommandValue(String value)
        {
            _value = value;
            if(_value != null)
                _scount = CountBytes(Value).ToString();
        }

        internal override void WriteTo(SocketWriter writer)
        {
            if (Value == null)
            {
                writer.Write(_empty);
            }
            else
            {
                writer.Write(RESPHeaders.BulkString);
                writer.WriteLine(_scount);
                writer.WriteLine(Value);
            }
        }
    }
}

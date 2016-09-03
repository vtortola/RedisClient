using System;
using System.Diagnostics.Contracts;

namespace vtortola.Redis
{
    internal sealed class RESPInteger : RESPObject
    {
        internal Int64 Value { get; private set; }
        internal override Char Header { get { return RESPHeaders.Integer; } }

        internal RESPInteger(Int64 value)
        {
            Value = value;
        }

        internal static RESPInteger Load(SocketReader reader)
        {
            return new RESPInteger(reader.ReadInt64());
        }

        public override String ToString()
        {
            return Value.ToString();
        }
    }
}

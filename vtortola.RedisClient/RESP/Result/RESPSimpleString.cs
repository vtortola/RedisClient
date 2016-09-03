using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class RESPSimpleString : RESPString
    {
        internal static readonly RESPSimpleString OK = new RESPSimpleString("OK");
        internal static readonly RESPSimpleString DISCARDED = new RESPSimpleString("DISCARDED");
        
        internal override Char Header { get { return RESPHeaders.SimpleString; } }

        internal RESPSimpleString(String value)
            :base(value)
        {
        }

        internal static RESPSimpleString Load(SocketReader reader)
        {
            return new RESPSimpleString(reader.ReadString());
        }
    }
}

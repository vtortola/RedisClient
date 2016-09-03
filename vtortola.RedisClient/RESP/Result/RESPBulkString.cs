using System;
using System.Diagnostics.Contracts;

namespace vtortola.Redis
{
    internal sealed class RESPBulkString : RESPString
    {
        internal static readonly RESPBulkString Null = new RESPBulkString(null);

        internal override Char Header { get { return RESPHeaders.BulkString; } }

        internal RESPBulkString(String value)
            :base(value)
        {
        }

        internal static RESPBulkString Load(SocketReader reader)
        {
            Int32 byteLength = reader.ReadInt32();
            if (byteLength < 0)
                return Null;
            else
                return new RESPBulkString(reader.ReadString(byteLength));
        }
    }
}

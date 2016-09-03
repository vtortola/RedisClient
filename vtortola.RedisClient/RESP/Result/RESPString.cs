using System;
using System.Diagnostics.Contracts;

namespace vtortola.Redis
{
    internal abstract class RESPString : RESPObject
    {
        internal String Value { get; private set; }

        internal RESPString(String value)
        {
            Value = value;
        }

        internal static Boolean Same(RESPString item, String comparand)
        {
            Contract.Assert(item != null, "Item is null");

            return String.Equals(item.Value, comparand, StringComparison.OrdinalIgnoreCase);
        }

        internal static Boolean Same(RESPObject item, String comparand)
        {
            Contract.Assert(item != null, "Item is null");
            Contract.Assert(item is RESPString, "Item is not RESPString");

            return Same((RESPString)item, comparand);
        }

        internal static Boolean IsString(Char header)
        {
            return header == RESPHeaders.SimpleString || header == RESPHeaders.BulkString;
        }

        public override String ToString()
        {
            return Value;
        }
    }
}

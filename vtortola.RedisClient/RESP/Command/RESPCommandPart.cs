using System;
using System.Diagnostics.Contracts;

namespace vtortola.Redis
{
    internal abstract class RESPCommandPart
    {
        protected const Char CRChar = '\r';
        protected const Char LFChar = '\n';

        internal abstract String Value { get;}

        protected static readonly String _empty = "$-1\r\n";

        internal abstract void WriteTo(SocketWriter writer);

        protected static Int32 CountBytes(String value)
        {
            Contract.Assert(value != null, "Value cannot be null.");

            var count = value.Length;
            for (int i = 0; i < value.Length; i++)
            {
                var c = Char.ConvertToUtf32(value, i);
                if (c < 127)
                    continue;
                else if (c >= 65536)
                    count += 3;
                else if (c >= 2048)
                    count += 2;
                else if (c >= 128)
                    count += 1;
            }
            return count;
        }
    }
}

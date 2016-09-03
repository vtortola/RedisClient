using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    internal static class RESPHeaders
    {
        internal const Char SimpleString = '+';
        internal const Char BulkString = '$';
        internal const Char Array = '*';
        internal const Char Error = '-';
        internal const Char Integer = ':';
    }

    internal abstract class RESPObject
    {
        internal static readonly IFormatProvider FormatInfo = new System.Globalization.NumberFormatInfo()
        {
            NumberDecimalSeparator = ".",
            NumberGroupSeparator = String.Empty,
            NumberNegativePattern = 1,
            NegativeSign = "-"
        };

        internal abstract Char Header { get; }

        internal static RESPObject Read(SocketReader reader)
        {
            var first = reader.ReadRESPHeader();
            return ProcessResponse(first, reader);
        }

        internal static async Task<RESPObject> ReadAsync(SocketReader reader, CancellationToken cancel)
        {
            var first = await reader.ReadRESPHeaderAsync(cancel).ConfigureAwait(false);
            return ProcessResponse(first, reader);
        }

        static RESPObject ProcessResponse(Char? header, SocketReader reader)
        {
            if (!header.HasValue)
                return null;

            switch (header)
            {
                case RESPHeaders.SimpleString: return RESPSimpleString.Load(reader);
                case RESPHeaders.BulkString: return RESPBulkString.Load(reader);
                case RESPHeaders.Array: return RESPArray.Load(reader);
                case RESPHeaders.Error: return RESPError.Load(reader);
                case RESPHeaders.Integer: return RESPInteger.Load(reader);

                default: throw new RESPException("Unrecognized RESP header (byte): " + (byte)header.Value);
            }
        }

        internal static TRESP Read<TRESP>(SocketReader reader)
            where TRESP: RESPObject
        {
            return (TRESP)Read(reader);
        }
    }
}

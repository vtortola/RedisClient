using System;
namespace vtortola.Redis
{
    internal static class Utf8ByteHelper
    {
        internal static Int64 ParseInt64(Byte[] array, Int32 count)
        {
            var result = 0L;
            var exp = 0;
            var negative = false;

            for (var i = count - 1; i >= 0; i--)
            {
                var c = array[i];
               
                if (c == 45)
                {
                    if (i != 0)
                        throw new RESPException("Negative symbol can only exists at the beginning of the string.");

                    negative = true;
                }
                else
                {
                    var v = GetUtf8CharNumber(c);
                    result += Convert.ToInt64(v * Math.Pow(10, exp++));
                }
            }

            return negative ? 0 - result : result;
        }

        private static Int64 GetUtf8CharNumber(Byte b)
        {
            switch (b)
            {
                case 48: return 0;
                case 49: return 1;
                case 50: return 2;
                case 51: return 3;
                case 52: return 4;
                case 53: return 5;
                case 54: return 6;
                case 55: return 7;
                case 56: return 8;
                case 57: return 9;
                default: throw new RESPException("Byte " + b + " cannot be translated to an UTF8 number");
            }
        }

        internal static Char ParseRESPHeader(Byte b)
        {
            switch (b)
            {
                case 36: return RESPHeaders.BulkString;
                case 42: return RESPHeaders.Array;
                case 43: return RESPHeaders.SimpleString;
                case 45: return RESPHeaders.Error;
                case 58: return RESPHeaders.Integer;
                default: throw new RESPException("Byte " + b + " cannot be translated to a RESPHeaders char.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vtortola.Redis
{
    /*
     * StreamReader.ReadLine consideres /r/n, /r and /n line delimiters,
     * which makes difficult to work with bulk strings that contains line breaks.
     * SocketReader only considers /r/n as line delimiter.
     * 
     * It assumes UTF8 when not using the Decoder.
     * 
     * It is optimized for the RESP protocol, it does not intend
     * to be a general purpose reader.
     * 
     */
    internal class SocketReader : IDisposable
    {
        const Byte CRByte = 13;
        const Byte LFByte = 10;

        readonly Stream _stream;
        readonly Byte[] _buffer;
        readonly Byte[] _integerBuffer;
        readonly Char[] _charBuffer;
        readonly Decoder _decoder;

        Int32 _left, _right;

        internal SocketReader(Stream stream, Int32 bufferSize)
        {
            Contract.Assert(bufferSize >= 1, "Buffer size must be 1 or bigger.");

            _decoder = Encoding.UTF8.GetDecoder();
            _stream = stream;
            _buffer = new Byte[bufferSize];
            _charBuffer = new Char[bufferSize];
            _integerBuffer = new Byte[21];
        }

        private Boolean CanRead()
        {
            if (_right - _left > 0)
                return true;

            _left = 0;
            _right = _stream.Read(_buffer, 0, _buffer.Length);

            return _right > 0;
        }

        private async Task<Boolean> CanReadAsync(CancellationToken cancel)
        {
            if (_right - _left > 0)
                return true;

            _left = 0;
            _right = await _stream.ReadAsync(_buffer, 0, _buffer.Length, cancel).ConfigureAwait(false);

            return _right > 0;
        }

        internal Char? ReadRESPHeader()
        {
            if (CanRead())
                return Utf8ByteHelper.ParseRESPHeader(_buffer[_left++]);

            return null;
        }

        internal async Task<Char?> ReadRESPHeaderAsync(CancellationToken cancel)
        {
            if (await CanReadAsync(cancel).ConfigureAwait(false))
                return Utf8ByteHelper.ParseRESPHeader(_buffer[_left++]);

            return null;
        }

        internal Int32 ReadInt32()
        {
            return (Int32)ReadInt64();
        }

        internal Int64 ReadInt64()
        {
            var integerIndex = 0;
            var crfound = false;

            while (CanRead())
            {
                for (; _left < _right; _left++)
                {
                    var b = _buffer[_left];

                    if(b == LFByte && crfound)
                    {
                        _left++; // moving to next line
                        return Utf8ByteHelper.ParseInt64(_integerBuffer, integerIndex-1);
                    }

                    crfound = b == CRByte;

                    _integerBuffer[integerIndex++] = b;
                }
            }

            throw new RedisClientSocketException("Cannot read an integer, not available data.");
        }

        internal String ReadString()
        {
            var builder = new StringBuilder(8); // most sort responses will be OK, QUEUED, etc..
            var crfound = false;

            var bytesUsed = 0;
            var charUsed = 0;
            var completed = false;

            while (CanRead())
            {
                var start = _left;
                for (; _left < _right; _left++)
                {
                    var c = _buffer[_left];

                    if(c == LFByte && crfound )
                    {
                        _left++;
                        _decoder.Convert(_buffer, start, _left - start, _charBuffer, 0, _charBuffer.Length, true, out bytesUsed, out charUsed, out completed);
                        builder.Append(_charBuffer, 0, charUsed);
                        
                        return builder.ToString(0, builder.Length - 2);
                    }

                    crfound = c == CRByte;
                }

                _decoder.Convert(_buffer, start, _right - start, _charBuffer, 0, _charBuffer.Length, false, out bytesUsed, out charUsed, out completed);
                builder.Append(_charBuffer, 0, charUsed);
            }

            throw new RedisClientSocketException("Cannot read a string, not available data.");
        }

        internal String ReadString(Int32 byteCount)
        {
            byteCount += 2;
            // byteCount + 2 since I want to read the final CRLF
            var builder = byteCount > 2 ? new StringBuilder() : new StringBuilder(byteCount);

            var bytesUsed = 0;
            var charUsed = 0;
            var completed = false;

            while (CanRead())
            {
                var available = _right - _left;

                if (byteCount <= available)
                    _decoder.Convert(_buffer, _left, byteCount, _charBuffer, 0, _charBuffer.Length, true, out bytesUsed, out charUsed, out completed);
                else
                    _decoder.Convert(_buffer, _left, available, _charBuffer, 0, _charBuffer.Length, false, out bytesUsed, out charUsed, out completed);
                
                builder.Append(_charBuffer, 0, charUsed);

                _left += bytesUsed;
                byteCount -= bytesUsed;

                if (byteCount <= 0)
                    return builder.ToString(0, builder.Length - 2);
            }

            throw new RedisClientSocketException("Cannot read a string with byte count, not available data.");
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}

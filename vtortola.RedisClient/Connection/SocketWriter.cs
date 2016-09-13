using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace vtortola.Redis
{
    internal class SocketWriter:IDisposable
    {
        readonly StreamWriter _writer;

        internal SocketWriter(Stream stream, Int32 bufferSize)
        {
            Contract.Assert(bufferSize >= 1, "Buffer size must be 1 or bigger.");

            _writer = new StreamWriter(stream, new UTF8Encoding(false), bufferSize);
        }
        internal void Write(Char value)
        {
            _writer.Write(value);
        }
        internal void Write(String value)
        {
            _writer.Write(value);
        }
        internal void WriteLine(String value)
        {
            _writer.WriteLine(value);
        }
        internal void Flush()
        {
            _writer.Flush();
        }
        public void Dispose()
        {
            _writer.Dispose();
        }
    }   
}

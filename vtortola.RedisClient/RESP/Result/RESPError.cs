using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class RESPError : RESPObject
    {
        internal static readonly RESPError EXECWATCHFAILED = new RESPError("EXECWATCHFAILED", "EXEC command returned null");

        internal String Prefix { get; private set; }
        internal String Message { get; private set; }
        internal override Char Header { get { return RESPHeaders.Error; } }

        internal RESPError(String prefix)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(prefix), "Calling RESPError iwth empty prefix.");

            Prefix = prefix;
        }

        internal RESPError(String prefix, String message)
            :this(prefix)
        {
            Message = message;
        }

        internal static RESPError Load(SocketReader reader)
        {
            var line = reader.ReadString();
            var space = line.IndexOf(' ');
            if (space != -1)
                return new RESPError(line.Substring(0, space), line.Substring(space + 1));
            else
                return new RESPError(line);
        }

        internal void SetMessage(String message)
        {
            Message = message;
        }

        public override String ToString()
        {
            return Header.ToString() + ' ' + Prefix + ' ' + Message;
        }
    }
}

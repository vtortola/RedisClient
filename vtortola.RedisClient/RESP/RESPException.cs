using System;

namespace vtortola.Redis
{
    internal sealed class RESPException : Exception
    {
        internal RESPException(String message)
            : base(message)
        {

        }
    }
}

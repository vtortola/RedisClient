using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vtortola.RedisClient.ProcedureDebugger
{
    internal sealed class SyntaxException : Exception
    {
        internal SyntaxException(String message)
            : base(message)
        {

        }

        internal SyntaxException(String message, Exception inner)
            : base (message, inner)
        {

        }
    }
}

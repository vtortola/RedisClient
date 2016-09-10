using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vtortola.RedisClient.ProcedureDebugger
{
    public sealed class SyntaxException : Exception
    {
        public SyntaxException(String message)
            : base(message)
        {

        }

        public SyntaxException(String message, Exception inner)
            : base (message, inner)
        {

        }
    }
}

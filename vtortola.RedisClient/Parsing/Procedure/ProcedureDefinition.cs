using System;
using System.Diagnostics;

namespace vtortola.Redis
{
    [DebuggerDisplay("{Name}, valid: {Error==null}")]
    internal class ProcedureDefinition
    {
        internal String Name { get; set; }
        internal String Body { get; set; }
        internal String Digest { get; set; }
        internal RedisClientException Error { get; set; }
        internal ProcedureParameter[] Parameters { get; set; }

    }
}

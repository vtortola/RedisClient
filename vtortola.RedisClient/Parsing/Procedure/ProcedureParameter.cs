using System;

namespace vtortola.Redis
{
    internal class ProcedureParameter
    {
        internal String Name { get; set; }
        internal Boolean IsKey { get; set; }
        internal Boolean IsArray { get; set; }
        internal static readonly ProcedureParameter[] EmptyArray = new ProcedureParameter[0];
    }
}

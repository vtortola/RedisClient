using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class ProcedureCollection
    {
        internal static readonly ProcedureCollection Empty = new ProcedureCollection();

        readonly IDictionary<String, ProcedureDefinition> _proceduresByName;
        readonly IDictionary<String, ProcedureDefinition> _proceduresByDigest;
        readonly IReadOnlyList<String> _digests;

        public IReadOnlyList<String> Digests { get { return _digests; } } 
        
        internal ProcedureCollection()
        {
            _proceduresByDigest = new Dictionary<String, ProcedureDefinition>();
            _proceduresByName = new Dictionary<String, ProcedureDefinition>(StringComparer.Ordinal);
            _digests = new List<String>().AsReadOnly();
        }

        internal ProcedureCollection(IDictionary<String, ProcedureDefinition> byName, IDictionary<String, ProcedureDefinition> byDigest)
        {
            _proceduresByName = byName;
            _proceduresByDigest = byDigest;
            _digests = _proceduresByDigest.Keys.ToList().AsReadOnly();
        }

        internal bool TryGetByDigest(String digest, out ProcedureDefinition procedure)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(digest), "Using an empty digest for finding a procedure.");

            return _proceduresByDigest.TryGetValue(digest, out procedure);
        }

        internal Boolean TryGetByAlias(String alias, out ProcedureDefinition procedure)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(alias), "Using an empty alias for finding a procedure.");

            return _proceduresByName.TryGetValue(alias, out procedure);
        }

        internal void SetFaulted(String scriptDigest, RedisClientException error)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(scriptDigest), "Using an empty digest for finding a procedure.");
            Contract.Assert(error != null, "Setting a null error on a procedure.");

            ProcedureDefinition procedure;
            if (!_proceduresByDigest.TryGetValue(scriptDigest, out procedure))
                throw new RedisClientParsingException("Script with digest '" + scriptDigest + "' does not exist.");

            procedure.Error = error;
        }
    }
}

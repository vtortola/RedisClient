using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace vtortola.Redis
{
    internal sealed class ProcedureCollection
    {
        readonly IDictionary<String, ProcedureDefinition> _proceduresByName;
        readonly IDictionary<String, ProcedureDefinition> _proceduresByDigest;

        internal static readonly ProcedureCollection Empty = new ProcedureCollection();

        internal ProcedureCollection()
        {
            _proceduresByDigest = new Dictionary<String, ProcedureDefinition>();
            _proceduresByName = new Dictionary<String, ProcedureDefinition>(StringComparer.Ordinal);
        }

        internal ProcedureCollection(IDictionary<String, ProcedureDefinition> byName, IDictionary<String, ProcedureDefinition> byDigest)
        {
            _proceduresByName = byName;
            _proceduresByDigest = byDigest;
        }

        internal IEnumerable<LoadingProcedure> GenerateScriptCheckings()
        {
            foreach (var procedure in _proceduresByName.Values)
            {
                var array = new RESPCommand(new RESPCommandLiteral("SCRIPT"), false);
                array.Add(new RESPCommandLiteral("exists"));
                array.Add(new RESPCommandLiteral(procedure.Digest));
                yield return new LoadingProcedure(procedure, array);
            }
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

        internal RESPCommand GenerateLoadCommand(String scriptDigest)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(scriptDigest), "Using an empty digest for finding a procedure.");

            ProcedureDefinition procedure;
            if (!_proceduresByDigest.TryGetValue(scriptDigest, out procedure))
                throw new RedisClientParsingException("Script with digest '" + scriptDigest + "' does not exist.");

            var array = new RESPCommand(new RESPCommandLiteral("SCRIPT"), false);
            array.Add(new RESPCommandLiteral("load"));
            array.Add(new RESPCommandLiteral(procedure.Body));
            return array;
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

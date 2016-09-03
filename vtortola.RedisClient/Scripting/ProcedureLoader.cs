using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace vtortola.Redis
{
    /// <summary>
    /// Loads procedures into <see cref="RedisClient"/>
    /// </summary>
    public sealed class ProcedureLoader
    {
        static readonly Char[] _split = new[] { ' ' };

        readonly Dictionary<String, ProcedureDefinition> _proceduresByName;
        readonly Dictionary<String, ProcedureDefinition> _proceduresByDigest;

        readonly ReaderWriterLockSlim _locker;

        internal ProcedureLoader()
        {
            _proceduresByName = new Dictionary<String, ProcedureDefinition>(StringComparer.Ordinal);
            _proceduresByDigest = new Dictionary<String, ProcedureDefinition>();
            _locker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">A text reader that provides procedures.</param>
        public void Load(TextReader reader)
        {
            var procedures = ProcedureParser.Parse(reader).ToArray();

            try
            {
                _locker.EnterWriteLock();
                Save(procedures);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Loads procedures from embedded resources in the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="extension">The extension that identifies a Redis procedure.</param>
        public void LoadFromAssembly(Assembly assembly, String extension = ".rcproc")
        {
            foreach (var filename in assembly.GetManifestResourceNames())
            {
                if (!filename.EndsWith(extension))
                    continue;

                using (var stream = assembly.GetManifestResourceStream(filename))
                using (var reader = new StreamReader(stream))
                {
                    Load(reader);
                }
            }
        }

        private void Save(ProcedureDefinition[] procedures)
        {
            Contract.Assert(procedures.Any(), "Trying to save an empty list of procedures.");

            foreach (var procedure in procedures)
            {
                if (!_proceduresByName.ContainsKey(procedure.Name))
                {
                    if (!_proceduresByDigest.ContainsKey(procedure.Digest))
                    {
                        _proceduresByName.Add(procedure.Name, procedure);
                        _proceduresByDigest.Add(procedure.Digest, procedure);
                    }
                    else
                        throw new RedisClientProcedureParsingException("Script with that content already exists.");
                }
                else
                    throw new RedisClientProcedureParsingException("Script '" + procedure.Name + "' already exists.");
            }
        }

        internal ProcedureCollection ToCollection()
        {
            Dictionary<String, ProcedureDefinition> byname = null;
            Dictionary<String, ProcedureDefinition> bydigest = null;
            try
            {
                _locker.EnterReadLock();
                byname = new Dictionary<String, ProcedureDefinition>(_proceduresByName);
                bydigest = new Dictionary<String, ProcedureDefinition>(_proceduresByDigest);
            }
            finally
            {
                _locker.ExitReadLock();
            }
            return new ProcedureCollection(byname, bydigest);
            
        }
    }
}

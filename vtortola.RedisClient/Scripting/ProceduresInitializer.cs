using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace vtortola.Redis
{
    internal sealed class ProcedureInitializer : IConnectionInitializer
    {
        readonly ProcedureCollection _procedures;
        readonly IRedisClientLog _logger;

        public ProcedureInitializer(ProcedureCollection procedures, IRedisClientLog logger)
        {
            _procedures = procedures;
            _logger = logger;
        }

        public void Initialize(SocketReader reader, SocketWriter writer)
        {
            if (!ShouldLoadScripts(writer))
                return;

            var loadQueue = LoadUnexistentScripts(reader, writer);
            ValidateScriptLoadingResults(reader, loadQueue);
        }

        private void ValidateScriptLoadingResults(SocketReader reader, Queue<String> loadQueue)
        {
            // for loaded scripts, read responses and ensure
            // returned SHA1 fits the locally calculated one
            while (loadQueue.Any())
            {
                var digest = loadQueue.Dequeue();
                var result = RESPObject.Read(reader);

                if(result == null)
                    throw new RedisClientParsingException("Cannot read responses.");

                try
                {
                    var resultDigest = result.Cast<RESPString>().Value;
                    if (digest != resultDigest)
                        throw new RedisClientParsingException("Script digest differs.");

                    _logger.Info("Script with digest {0} loaded.", digest);
                }
                catch(RedisClientException rcex)
                {
                    _logger.Error(rcex, "Script with digest {0} failed to load.", digest);
                    _procedures.SetFaulted(digest, rcex);
                }
                catch (Exception cmdEx)
                {
                    _logger.Error(cmdEx, "Script with digest {0} failed to load.", digest);
                    _procedures.SetFaulted(digest, new RedisClientParsingException("Error validating script digest", cmdEx));
                }
            }
        }

        private Queue<String> LoadUnexistentScripts(SocketReader reader, SocketWriter writer)
        {
            var loadQueue = new Queue<String>();

            // read results
            var result = RESPObject.Read(reader);
            var array = result.Cast<RESPArray>();
            // if a script does not exists, send 'script load'
            for (int i = 0; i < array.Count; i++)
			{
			    var found = array[i].Cast<RESPInteger>().Value != 0;
                if(!found)
                {
                    var digest = _procedures.Digests[i];
                    _logger.Info("Script with digest {0} not found, loading...", digest);
                    var load = GenerateLoadCommand(digest);
                    load.WriteTo(writer);
                    loadQueue.Enqueue(digest);
                }
			}

            writer.Flush();
            return loadQueue;
        }

        private RESPCommand GenerateLoadCommand(String digest)
        {
            ProcedureDefinition procedure;
            if (!_procedures.TryGetByDigest(digest, out procedure))
                throw new RedisClientParsingException("Script with digest '" + digest + "' does not exist.");

            var array = new RESPCommand(new RESPCommandLiteral("SCRIPT"), false);
            array.Add(new RESPCommandLiteral("load"));
            array.Add(new RESPCommandLiteral(procedure.Body));
            return array;
        }

        private Boolean ShouldLoadScripts(SocketWriter writer)
        {
            if (_procedures.Digests.Any())
            {
                var array = new RESPCommand(new RESPCommandLiteral("SCRIPT"), false);
                array.Add(new RESPCommandLiteral("exists"));
                foreach (var digest in _procedures.Digests)
                {
                    _logger.Info("Checking existence of script digest {0}...", digest);
                    array.Add(new RESPCommandLiteral(digest));
                }

                array.WriteTo(writer);
                writer.Flush();
                return true;
            }
            return false;
        }

    }
}

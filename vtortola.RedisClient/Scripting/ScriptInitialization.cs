using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class ScriptInitialization : IConnectionInitializer
    {
        readonly ProcedureCollection _procedures;
        readonly IRedisClientLog _logger;

        public ScriptInitialization(ProcedureCollection procedures, IRedisClientLog logger)
        {
            _procedures = procedures;
            _logger = logger;
        }

        public void Initialize(SocketReader reader, SocketWriter writer)
        {
            var checkQueue = CheckScriptExistence(writer);
            var loadQueue = LoadUnexistentScripts(reader, writer, checkQueue);
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

        private Queue<String> LoadUnexistentScripts(SocketReader reader, SocketWriter writer, Queue<String> checkQueue)
        {
            var loadQueue = new Queue<String>();

            // read results
            while (checkQueue.Any())
            {
                var digest = checkQueue.Dequeue();
                var result = RESPObject.Read(reader);
                // if a script does not exists, send 'script load'
                if (result.Cast<RESPArray>().ElementAt<RESPInteger>(0).Value == 0)
                {
                    _logger.Info("Script with digest {0} not found, loading...", digest);
                    var load = _procedures.GenerateLoadCommand(digest);
                    load.WriteTo(writer);
                    loadQueue.Enqueue(digest);
                }
            }
            writer.Flush();
            return loadQueue;
        }

        private Queue<String> CheckScriptExistence(SocketWriter writer)
        {
            // get existing scripts and send 'script exists' command
            var checkQueue = new Queue<String>();
            foreach (var script in _procedures.GenerateScriptCheckings())
            {
                _logger.Info("Checking existence of {0} ({1})...", script.Procedure.Name, script.Procedure.Digest);
                script.Command.WriteTo(writer);
                checkQueue.Enqueue(script.Procedure.Digest);
            }
            writer.Flush();
            return checkQueue;
        }

    }
}

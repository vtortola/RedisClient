using System;
using System.Collections.Generic;

namespace vtortola.Redis
{
    internal sealed class ConnectionInitializer : IConnectionInitializer
    {
        readonly RESPCommand[] _commands;
        readonly IRedisClientLog _logger;

        public ConnectionInitializer(RedisClientOptions options)
        {
            if (options == null)
                return;

            _logger = options.Logger ?? NoLogger.Instance;
            _commands = GenerateCommands(options).ToArray();
        }

        static List<RESPCommand> GenerateCommands(RedisClientOptions options)
        {
            var list = new List<RESPCommand>();
            var planner = new ExecutionPlanner();
            foreach (var cmd in options.InitializationCommands)
            {
                var plan = planner.Build(cmd.Command);
                var respCommand = plan.Bind(cmd.Parameters);
                list.AddRange(respCommand);
            }
            return list;
        }

        public void Initialize(SocketReader reader, SocketWriter writer)
        {
            if (_commands == null || _commands.Length == 0)
                return;

            SendCommands(writer);

            writer.Flush();

            ReadResults(reader);
        }

        private void ReadResults(SocketReader reader)
        {
            foreach (var command in _commands)
            {
                var response = RESPObject.Read(reader);
                _logger.Info(" <- Response for initialization command '{0}' is type '{1}'.", command.Header, response != null ? response.Header.ToString() : "<null>.");
                if (response == null)
                    throw new RedisClientSocketException("Initialization command did not return any response.");
                if (response.Header == RESPHeaders.Error)
                    throw new RedisClientCommandException(response.Cast<RESPError>());
            }
        }

        private void SendCommands(SocketWriter writer)
        {
            foreach (var command in _commands)
            {
                _logger.Info(" -> Sending initialization command '{0}'.", command.Header);
                command.WriteTo(writer);
            }
        }
    }
}

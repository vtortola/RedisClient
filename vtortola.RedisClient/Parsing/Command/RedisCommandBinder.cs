using System;
using System.Collections.Generic;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class RedisCommandBinder : CommandBinder
    {
        static HashSet<String> _subscriptionCommands =
                new HashSet<String>(new[] { "SUBSCRIBE", "UNSUBSCRIBE", "PSUBSCRIBE", "PUNSUBSCRIBE" }, StringComparer.Ordinal);

        public RedisCommandBinder(String header)
            : base(new RESPCommandLiteral(header), _subscriptionCommands.Contains(header))
        {
        }

        internal override RESPCommand Bind<T>(T parameters)
        {
            var command = base.CreateUnboundCommand();
            foreach (var part in Parts)
            {
                if (part.IsParameter)
                    command.AddRange(GetParameterByName(part.Value, parameters));
                else
                    command.Add(part);
            }
            return command;
        }
    }

}

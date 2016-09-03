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

        internal override RESPCommand Bind<T>(ParameterReader<T> values)
        {
            var command = base.CreateUnboundCommand();
            foreach (var part in Parts)
            {
                var parameter = part as RESPCommandParameter;
                if (parameter != null)
                    command.AddRange(GetParameterByName(parameter.Value, values));
                else
                    command.Add(part);
            }
            return command;
        }
    }

}

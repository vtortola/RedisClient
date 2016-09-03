using System;
using System.Collections.Generic;
using System.Linq;

namespace vtortola.Redis
{
    internal abstract class CommandBinder
    {
        protected List<RESPCommandPart> Parts { get; private set; }
        internal RESPCommandLiteral Header { get; private set; }
        internal Boolean IsSubscription { get; private set; }

        internal CommandBinder(RESPCommandLiteral header, Boolean isSubscription)
        {
            Header = header;
            IsSubscription = isSubscription;
            Parts = new List<RESPCommandPart>();
        }

        internal virtual void Add(RESPCommandPart part)
        {
            Parts.Add(part);
        }

        internal abstract RESPCommand Bind<T>(ParameterReader<T> values);

        protected RESPCommand CreateUnboundCommand()
        {
            return new RESPCommand(Header, IsSubscription, Parts.Count);
        }

        protected static IEnumerable<RESPCommandPart> GetParameterByName<T>(String parameterName, ParameterReader<T> parameters)
        {
            Exception error = null;
            try
            {
                if (parameters != null)
                    return parameters.GetValues(parameterName).Select(n => new RESPCommandValue(n));
            }
            catch (RedisClientBindingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                error = ex;
            }

            throw new RedisClientBindingException("No parameter has been provided, but the command is requiring the parameter: " + parameterName, error);
        }
    }
}

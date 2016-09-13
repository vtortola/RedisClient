using System;
using System.Collections.Generic;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class ProcedureCommandBinder : CommandBinder
    {
        static readonly RESPCommandLiteral _evalsha = new RESPCommandLiteral("EVALSHA");
        static readonly RESPCommandLiteral _literalArray = new RESPCommandLiteral("1");

        readonly ProcedureDefinition _procedure;

        public ProcedureCommandBinder(ProcedureDefinition procedure)
            : base(_evalsha, false)
        {
            _procedure = procedure;
            base.Add(new RESPCommandLiteral(procedure.Digest));
        }

        internal override RESPCommand Bind<T>(T parameters)
        {
            if (_procedure.Error != null)
                throw _procedure.Error;

            var command = base.CreateUnboundCommand();

            var keys = new List<RESPCommandPart>();
            var argv = new List<RESPCommandPart>();

            if (Parts.Count - 1 != _procedure.Parameters.Length)
                throw new RedisClientParsingException(String.Format("The procedure '{0}' expected {1} parameters, but only {2} have been provided.", _procedure.Name, _procedure.Parameters.Length, Parts.Count -1 ));

            command.Add(Parts[0]);

            ProcessKeys<T>(parameters, keys, argv);

            ProcessArgs<T>(parameters, argv);

            command.Add(new RESPCommandLiteral(keys.Count.ToString()));

            if (keys.Any())
                command.AddRange(keys);
            if (argv.Any())
                command.AddRange(argv);

            return command;
        }

        private void ProcessArgs<T>(T parameters, List<RESPCommandPart> argv)
        {
            for (int i = 0; i < _procedure.Parameters.Length; i++)
            {
                var parameter = _procedure.Parameters[i];
                if (parameter.IsKey)
                    continue;
                var part = Parts[i + 1];
                AppendParameter(argv, argv, parameters, parameter, part);
            }
        }

        private void ProcessKeys<T>(T parameters, List<RESPCommandPart> keys, List<RESPCommandPart> argv)
        {
            for (int i = 0; i < _procedure.Parameters.Length; i++)
            {
                var parameter = _procedure.Parameters[i];
                if (!parameter.IsKey)
                    continue;
                var part = Parts[i + 1];
                AppendParameter(keys, argv, parameters, parameter, part);
            }
        }

        static void AppendParameter<T>(List<RESPCommandPart> list, List<RESPCommandPart> argv, T parameters, ProcedureParameter parameter, RESPCommandPart commandPart)
        {
            var parameterPart = commandPart as RESPCommandParameter;
            if (parameterPart != null)
            {
                var paramValues = GetParameterByName<T>(parameterPart.Value, parameters).ToList();
                MapInputParameterToProcParameter(parameter, paramValues, argv);
                list.AddRange(paramValues);
            }
            else
            {
                if(parameter.IsArray)
                    argv.Add(_literalArray);

                list.Add(commandPart);
            }
        }

        static void MapInputParameterToProcParameter(ProcedureParameter parameter, List<RESPCommandPart> values, IList<RESPCommandPart> argv)
        {
            if (values.Count == 0)
            {
                throw new RedisClientParsingException("There is no value defined for the parameter '" + parameter.Name + "'");
            }
            else if (!parameter.IsArray && values.Count > 1)
            {
                throw new RedisClientParsingException("Parameter '" + parameter.Name + "' is not marked as array, however multiple values have been found for it.");
            }
            else if (parameter.IsArray)
            {
                argv.Add(new RESPCommandLiteral(values.Count.ToString()));
            }
        }
    }
}

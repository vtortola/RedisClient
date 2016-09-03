using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vtortola.Redis
{
    internal sealed class ExecutionPlanner : IExecutionPlanner
    {
        readonly ProcedureCollection _procedures;

        internal ExecutionPlanner()
        {
        }

        internal ExecutionPlanner(ProcedureCollection procedures)
        {
            _procedures = procedures;
        }

        public ExecutionPlan Build(String command)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(command), "Calling to build execution plan on an empty string.");

            var commands = new List<CommandBinder>();
            CommandBinder current = null;

            foreach (var part in TextCommandWordParser.Parse(command))
            {
                if (current == null)
                {
                    if(part.IsParameter)
                        throw new RedisClientParsingException("Parameter cannot be first in the statement.");

                    ProcedureDefinition procedure;
                    if (_procedures != null && _procedures.TryGetByAlias(part.Value, out procedure))
                    {
                        if (procedure.Error != null)
                            throw new RedisClientParsingException("Procedure '" + part.Value + "' throwed an exception due a syntax error in its declaration.", procedure.Error);

                        current = new ProcedureCommandBinder(procedure);
                    }
                    else
                    {
                        current = new RedisCommandBinder(part.Value.ToUpperInvariant());
                    }

                    commands.Add(current);

                    if (part.IsEndOfLine)
                        current = null;

                    continue;
                }
                
                if (part.IsParameter)
                    current.Add(new RESPCommandParameter(part.Value));
                else
                    current.Add(new RESPCommandLiteral(part.Value));

                if (part.IsEndOfLine)
                    current = null;
            }

            return new ExecutionPlan(commands);
        }
    }
}

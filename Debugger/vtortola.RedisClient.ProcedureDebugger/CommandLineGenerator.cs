using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtortola.Redis;

namespace vtortola.RedisClient.ProcedureDebugger
{
    internal static class CommandLineGenerator
    {
        internal static String Generate(params String[] args)
        {
            // Parse input
            var session = SessionModel.Parse(args);

            // Parse procedures
            ProcedureDefinition[] procedures;
            using (var sr = new StreamReader(session.FileName))
                procedures = ProcedureParser.Parse(sr).ToArray();

            var procedure = procedures.SingleOrDefault(p => p.Name.Equals(session.Procedure, StringComparison.InvariantCultureIgnoreCase));
            if (procedure == null)
                throw new InvalidOperationException(String.Format("Cannot find the specified procedure '{0}' in '{1}'", session.Procedure, session.FileName));

            // generate temporary LUA file with the procedure content
            var tempFile = Path.GetTempFileName();
            using (var sw = new StreamWriter(tempFile))
                sw.WriteLine(procedure.Body);

            var keyValues = new List<String>();
            var argValues = new List<String>();

            foreach (var keyParameter in procedure.Parameters.Where(p => p.IsKey))
            {
                // if parameter is an array, then the length is added to the ARGV
                String[] values;
                if (!session.Parameters.TryGetValue(keyParameter.Name, out values) || values.Length == 0)
                    throw new InvalidOperationException("Command line does not include values for parameter: " + keyParameter.Name);

                if (values.Length > 1)
                    argValues.Add(values.Length.ToString());
                keyValues.AddRange(values);
            }

            foreach (var argParameter in procedure.Parameters.Where(p => !p.IsKey))
            {
                // if parameter is an array, then the length is added to the ARGV
                String[] values;
                if (!session.Parameters.TryGetValue(argParameter.Name, out values) || values.Length == 0)
                    throw new InvalidOperationException("Command line does not include values for parameter: " + argParameter.Name);

                if (values.Length > 1)
                    argValues.Add(values.Length.ToString());
                argValues.AddRange(values);
            }

            var builder = new StringBuilder(" --ldb ");
            builder.Append(session.ExtraCommands);
            builder.Append(' ');
            builder.AppendFormat("--eval \"{0}\" ", tempFile);
            foreach (var key in keyValues)
            {
                builder.Append(key);
                builder.Append(" ");
            }

            builder.Append(", ");
            foreach (var arg in argValues)
            {
                builder.Append(arg);
                builder.Append(" ");
            }
            
            return builder.ToString();
        }
    }
}

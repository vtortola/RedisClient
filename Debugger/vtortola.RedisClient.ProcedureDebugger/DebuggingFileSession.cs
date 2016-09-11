using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using vtortola.Redis;

namespace vtortola.RedisClient.ProcedureDebugger
{
    internal sealed class DebuggingFileSession : IDisposable
    {
        readonly FileInfo _file;
        public String CliArguments { get; private set; }
        public String TemporaryFile { get { return _file.FullName; } }

        internal DebuggingFileSession(InputModel input, ProcedureDefinition procedure)
        {
            _file = new FileInfo(Path.GetTempFileName());
            using (var sw = new StreamWriter(_file.Open(FileMode.Create, FileAccess.Write, FileShare.None)))
                sw.Write(procedure.Body);

            var keyValues = new List<String>();
            var argValues = new List<String>();

            AddParameters(input, procedure, keyValues, argValues, p => p.IsKey );
            AddParameters(input, procedure, argValues, argValues, p => !p.IsKey);

            CliArguments = BuildArgumentsString(input, keyValues, argValues);
        }

        private String BuildArgumentsString(InputModel input, List<string> keyValues, List<string> argValues)
        {
            var builder = new StringBuilder(" --ldb ");
            builder.Append(input.CliCommands);
            builder.Append(' ');
            builder.AppendFormat("--eval \"{0}\" ", _file.FullName);
            foreach (var key in keyValues)
            {
                builder.Append("\"");
                builder.Append(key);
                builder.Append("\" ");
            }

            builder.Append(", ");
            foreach (var arg in argValues)
            {
                builder.Append("\"");
                builder.Append(arg);
                builder.Append("\" ");
            }

            return builder.ToString();
        }

        private static void AddParameters(InputModel input, ProcedureDefinition procedure, List<String> target, List<String> argValues, Predicate<ProcedureParameter> predicate)
        {
            foreach (var keyParameter in procedure.Parameters.Where(p=>predicate(p)))
            {
                // if parameter is an array, then the length is added to the ARGV
                String[] values;
                if (!input.Parameters.TryGetValue(keyParameter.Name, out values) || values.Length == 0)
                    throw new SyntaxException("Command line does not include values for parameter: " + keyParameter.Name);

                if (values.Length > 1)
                    argValues.Add(values.Length.ToString());

                target.AddRange(values);
            }
        }

        public void Dispose()
        {
            _file.Delete();
        }
    }
}

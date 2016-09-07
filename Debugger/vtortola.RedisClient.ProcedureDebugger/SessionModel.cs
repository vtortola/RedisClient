using System;
using System.Collections.Generic;
using System.Text;

namespace vtortola.RedisClient.ProcedureDebugger
{
    public sealed class Parameter
    {
        public String Name { get; private set; }
        public String[] Values { get; private set; }
        public Parameter(String name, String[] values)
        {
            Name = name;
            Values = values;
        }
    }

    public sealed class SessionModel
    {
        public String FileName { get; private set; }
        public String Procedure { get; private set; }
        public String ExtraCommands { get; private set; }
        public Dictionary<String, String[]> Parameters { get; private set; }

        private SessionModel()
        {
            Parameters = new Dictionary<String, String[]>();
        }

        public static SessionModel Parse(String[] args)
        {
            var session = new SessionModel();
            var extraCommands = new StringBuilder();
            var lastWasExtra = false;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if(arg == "--file")
                {
                    session.FileName = args[++i];
                    lastWasExtra = false;
                }
                else if (arg == "--procedure")
                {
                    session.Procedure = args[++i];
                    lastWasExtra = false;
                }
                else if (arg == "--eval")
                {
                    throw new InvalidOperationException("--eval command is forbidden, use --file and --procedure");
                }
                else if (arg.StartsWith("--@"))
                {
                    arg = arg.Substring(3, arg.Length - 3);
                    session.Parameters.Add(arg, ParseValues(args[++i]));
                    lastWasExtra = false;
                }
                else
                {
                    if(arg.StartsWith("-"))
                    {
                        lastWasExtra = true;
                        extraCommands.Append(arg);
                        extraCommands.Append(" ");
                    }
                    else if(lastWasExtra)
                    {
                        extraCommands.Append(arg);
                        lastWasExtra = false;
                    }
                    else
                    {
                        throw new InvalidOperationException("invalid part: " + arg);
                    }
                }
            }
            session.ExtraCommands = extraCommands.ToString();
            return session;
        }

        static String[] ParseValues(String valuesString)
        {
            var index = 0;
            while (valuesString[index] == ' ')
                index++;

            if(valuesString[index] == '[')
            {
                return ParseArray(valuesString, index);
            }
            else
            {
                return new String[] { valuesString };
            }
        }

        internal static String[] ParseArray(String valuesString, Int32 index)
        {
            var list = new List<String>();
            String current = String.Empty;
            for (; index < valuesString.Length; index++)
            {
                var c = valuesString[index];
                if (c == ']' || c == ',')
                {
                    list.Add(current);
                    current = String.Empty;
                }
                else if(c== '[' || c == ' ')
                {
                    continue;
                }
                else
                {
                    current += c;
                }

            }
            return list.ToArray();
        }
    }
}

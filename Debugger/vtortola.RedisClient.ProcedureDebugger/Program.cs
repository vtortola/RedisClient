using System;
using System.Diagnostics;

namespace vtortola.RedisClient.ProcedureDebugger
{
    class Program
    {
        static void Main(String[] args)
        {
            var s = CommandLineGenerator.Generate(args);

            Process cmd = new Process();
            cmd.StartInfo.FileName = "redis-cli";
            cmd.StartInfo.Arguments = s;
            cmd.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            cmd.Start();
        }
    }
}

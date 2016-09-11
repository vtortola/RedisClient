using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace vtortola.RedisClient.ProcedureDebugger
{
    class Program
    {
        static void Main(String[] args)
        {
            if (AskForHelp(args))
            {
                ShowHelp();
            }
            else if (AskForVersion(args))
            {
                ShowVersion();
            }
            else
            {
                try
                {
                    LaunchDebugger(args);
                }
                catch(SyntaxException sex)
                {
                    ShowError("Syntax Error", sex);
                }
                catch (Exception ex)
                {
                    ShowError("Unhandled Exception", ex);
                }
            }
        }

        private static void ShowError(String title, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(title);
            Console.WriteLine(ex.Message);
            Console.ResetColor();
            Console.WriteLine();
            ShowHelp();
        }

        static void LaunchDebugger(String[] args)
        {
            using (var session = CommandLineGenerator.Generate(args))
            {
                Process cmd = new Process();
                cmd.StartInfo.FileName = Path.Combine(ConfigurationManager.AppSettings["RedisCliExeLocation"], "redis-cli.exe");
                cmd.StartInfo.Arguments = session.CliArguments;
                cmd.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
                cmd.Start();
                cmd.WaitForExit();
                Console.WriteLine("Dispose");
            }
        }

        static Boolean AskForHelp(String[] args)
        {
            return args == null || args.Length ==0 || (args.Length == 1 && args[0] == "--help");
        }

        static void ShowHelp()
        {
            ShowVersion();
            Console.WriteLine("Usage procedure-debugger [OPTIONS]");
            Console.WriteLine("\t-h <hostname>       Server hostname (default: 127.0.0.1).");
            Console.WriteLine("\t-p <port>           Server port (default: 6379).");
            Console.WriteLine("\t-s <socket>         Server socket (overrides hostname and port).");
            Console.WriteLine("\t-a <password>       Password to use when connecting to the server.");
            Console.WriteLine("\t--sync-mode         Uses the synchronous Lua debugger, in");
            Console.WriteLine("\t                    this mode the server is blocked and script changes are");
            Console.WriteLine("\t                    are not rolled back from the server memory.");
            Console.WriteLine("\t--help              Output this help and exit.");
            Console.WriteLine("\t--version           Output version and exit.");
            Console.WriteLine();                     
            Console.WriteLine("\t--file <filename>   Name of the file with the procedure/s.");
            Console.WriteLine("\t--procedure <proc>  Name of the procedure to execute.");
            Console.WriteLine("\t--@<name> <value>   Name and value for a parameter.");
            Console.WriteLine();         

        }

        static void ShowVersion()
        {
            Console.WriteLine("procedure-debugger " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        private static bool AskForVersion(string[] args)
        {
            return args.Length == 1 && args[0] == "--version";
        }

        


    }
}

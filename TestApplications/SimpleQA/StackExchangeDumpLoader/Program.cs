using SimpleInjector.Extensions.ExecutionContextScoping;
using SimpleQA;
using System;
using System.IO;
using System.Net;
using System.Xml.Linq;
using vtortola.Redis;

namespace StackExchangeDumpLoader
{
    // Dumps can be found at https://archive.org/details/stackexchange

    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo directory;
            IPEndPoint endpoint;
            try
            {
                directory = new DirectoryInfo(args[0]);
                if (!directory.Exists)
                    throw new DirectoryNotFoundException(args[0]);

                endpoint = new IPEndPoint(IPAddress.Parse(args[1]), Int32.Parse(args[2]));
            }
            catch(ArgumentOutOfRangeException)
            {
                Console.WriteLine("Usage: StackExchangeDumpLoader <Directory> <IPAddress> <Port>");
                Console.ReadKey(true);
                return;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Usage: StackExchangeDumpLoader <Directory>  <IPAddress> <Port>");
                Console.ReadKey(true);
                return;
            }

            var dicontainer = new SimpleInjector.Container();
            dicontainer.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();

            dicontainer.Register<PostsXMLProcessor>();
            dicontainer.Register<UsersXMLProcessor>();
            dicontainer.Register<VotesXMLProcessor>();
            dicontainer.Register<ICommandExecuterMediator>( () => new CommandExecuterMediator(dicontainer) );
            RedisCommandsConfiguration.Configure(dicontainer, endpoint, new ConsoleLogger());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nLOAD STACKEXCHANGE DUMP INTO SIMPLEQA\n");
            Console.ResetColor();
            Console.WriteLine("Reading files from " + directory);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nWARNING: This tool was not designed for uploading big dumps. Do not use it with dumps > 50Mb.\n");
            Console.ResetColor();
            Console.WriteLine("Press any key to start...");
            Console.ReadKey(true);

            using (dicontainer.BeginExecutionContextScope())
            {
                var users = dicontainer.GetInstance<UsersXMLProcessor>().Process(XDocument.Load(Path.Combine(directory.FullName, "Users.xml")));
                var posts = dicontainer.GetInstance<PostsXMLProcessor>().Process(XDocument.Load(Path.Combine(directory.FullName, "Posts.xml")), users);
                dicontainer.GetInstance<VotesXMLProcessor>().Process(XDocument.Load(Path.Combine(directory.FullName, "Votes.xml")), users, posts);
            }

            Console.WriteLine("END");
            Console.ReadKey(true);
        }

        public sealed class ConsoleLogger : IRedisClientLog
        {
            public void Info(string format, params object[] args)
            {
                Console.WriteLine(format, args);
            }

            public void Error(string format, params object[] args)
            {
                Console.WriteLine("Exception: " + format, args);
            }

            public void Error(Exception error, string format, params object[] args)
            {
                Console.WriteLine("Exception: " + String.Format(format, args) + "\n" + error.ToString());
            }

            public void Debug(String format, params Object[] args)
            {
            }
        }
    }
}

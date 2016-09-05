using SimpleInjector.Extensions.ExecutionContextScoping;
using SimpleQA;
using System;
using System.IO;
using System.Xml.Linq;
using vtortola.Redis;

namespace StackExchangeDumpLoader
{
    // Dumps can be found at https://archive.org/details/stackexchange

    class Program
    {
        static void Main(string[] args)
        {
            //var directory = @"C:\\Users\\valeriano_tortola\\Downloads\\arduino.stackexchange.com";
            //var directory = @"C:\\Users\\valeriano_tortola\\Downloads\\crafts.stackexchange.com";
            var directory = @"C:\\Users\\valeriano_tortola\\Downloads\\cooking.stackexchange.com";
            //var directory = @"C:\\Users\\valeriano_tortola\\Downloads\\askubuntu.com";
            var dicontainer = new SimpleInjector.Container();
            dicontainer.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();

            dicontainer.Register<PostsXMLProcessor>();
            dicontainer.Register<UsersXMLProcessor>();
            dicontainer.Register<VotesXMLProcessor>();
            dicontainer.Register<ICommandExecuterMediator>( () => new CommandExecuterMediator(dicontainer) );
            RedisCommandsConfiguration.Configure(dicontainer, new ConsoleLogger());

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
                var users = dicontainer.GetInstance<UsersXMLProcessor>().Process(XDocument.Load(Path.Combine(directory, "Users.xml")));
                var posts = dicontainer.GetInstance<PostsXMLProcessor>().Process(XDocument.Load(Path.Combine(directory, "Posts.xml")), users);
                dicontainer.GetInstance<VotesXMLProcessor>().Process(XDocument.Load(Path.Combine(directory, "Votes.xml")), users, posts);
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
        }
    }
}

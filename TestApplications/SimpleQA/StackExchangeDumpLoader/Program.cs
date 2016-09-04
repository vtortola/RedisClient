using SimpleInjector.Extensions.ExecutionContextScoping;
using SimpleQA;
using System;
using System.IO;
using System.Xml.Linq;

namespace StackExchangeDumpLoader
{
    // Dumps can be found at https://archive.org/details/stackexchange

    class Program
    {
        static void Main(string[] args)
        {
            var directory = @"C:\\Users\\valeriano_tortola\\Downloads\\crafts.stackexchange.com";
            
            var dicontainer = new SimpleInjector.Container();
            dicontainer.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();

            dicontainer.Register<PostsXMLProcessor>();
            dicontainer.Register<ICommandExecuterMediator>( () => new CommandExecuterMediator(dicontainer) );
            RedisCommandsConfiguration.Configure(dicontainer, null);

            using (dicontainer.BeginExecutionContextScope())
            {
                var postsXMLProcessor = dicontainer.GetInstance<PostsXMLProcessor>();

                postsXMLProcessor.Process(XDocument.Load(Path.Combine(directory, "Posts.xml")));
            }

            Console.WriteLine("END");
            Console.ReadKey(true);
        }
    }
}

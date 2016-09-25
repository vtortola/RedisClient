using NLog;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;
using SimpleQA.Markdown;
using System.Net;
using System.Reflection;
using System.Web.Mvc;

namespace SimpleQA.WebApp
{
    public static class DependencyInjectionConfig
    {
        public static void Configure()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

            container.Register<ILogger>(() => LogManager.GetCurrentClassLogger(), Lifestyle.Singleton);
            container.Register<IMarkdown, SimpleQA.WebApp.Infrastructure.Markdown>(Lifestyle.Singleton);
            container.Register<ICommandExecuterMediator, CommandExecuterMediator>(Lifestyle.Singleton);
            container.Register<IModelBuilderMediator, ModelBuilderMediator>(Lifestyle.Singleton);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());

            container.RegisterMvcIntegratedFilterProvider();

            RedisCommandsConfiguration.Configure(container, new IPEndPoint(IPAddress.Loopback, 6379), true);

            container.Verify();

            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }
    }
}
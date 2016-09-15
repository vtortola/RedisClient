using NLog;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;
using SimpleQA.Markdown;
using System.Reflection;
using System.Web.Mvc;
using vtortola.Redis;

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

            RedisCommandsConfiguration.Configure(container, new RedisLogger());

            container.Verify();

            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }

        class RedisLogger : IRedisClientLog
        {
            readonly ILogger _log;
            public RedisLogger()
            {
                _log = LogManager.GetLogger("vtortola.RedisClient");
            }

            public void Info(string format, params object[] args)
            {
                _log.Info(format, args);
            }

            public void Error(string format, params object[] args)
            {
                _log.Error(format, args);
            }

            public void Error(System.Exception error, string format, params object[] args)
            {
                _log.Error(error, format, args);
            }

            public void Debug(string format, params object[] args)
            {
                
            }
        }

    }
}
using NLog;
using SimpleQA.WebApp.Models.ModelBinders;
using StackExchange.Profiling;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SimpleQA.WebApp
{
    public class Global : HttpApplication
    {
        public Global()
        {
            this.BeginRequest += Global_BeginRequest;
            this.EndRequest += Global_EndRequest;
        }

        void Global_EndRequest(object sender, EventArgs e)
        {
            MiniProfiler.Stop();
        }

        void Global_BeginRequest(object sender, EventArgs e)
        {
            MiniProfiler.Start();
        }

        void Application_Start(object sender, EventArgs e)
        {
            DependencyInjectionConfig.Configure();
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            BundlingConfiguration.RegisterBundles(BundleTable.Bundles);
            MiniProfilerHandler.RegisterRoutes();
            ModelBinderProviders.BinderProviders.Add(new QuestionModelBinderProvider());
            LogManager.GetCurrentClassLogger().Info("Application started");

            // no wep api right now
            // GlobalConfiguration.Configure(WebApiConfig.Register);
        }

    }
}
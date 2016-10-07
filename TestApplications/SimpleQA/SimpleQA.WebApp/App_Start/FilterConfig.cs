using NLog;
using SimpleQA.WebApp.Filter;
using System.Web.Mvc;

namespace SimpleQA.WebApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new SetIdentityAttribute());
            filters.Add(new SimpleQAErrorHandler(LogManager.GetCurrentClassLogger()));
        }
    }
}
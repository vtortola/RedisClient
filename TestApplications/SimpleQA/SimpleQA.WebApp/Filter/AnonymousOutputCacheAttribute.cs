using SimpleQA.Commands;
using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace SimpleQA.WebApp.Filter
{
    public sealed class AnonymousOutputCacheAttribute : OutputCacheAttribute
    {
        public AnonymousOutputCacheAttribute()
        {
            // Bug in OutputCache profiles? Even if Location is set to
            // Server in the config, I can see Cache-Control: public in the response...
            // because the Location property is initialized to Any ignoring the Web.config.

            Location = OutputCacheLocation.Server;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Location = filterContext.HttpContext.User.Identity.IsAuthenticated ?  OutputCacheLocation.None : OutputCacheLocation.Server;

            filterContext
                .HttpContext
                .Response
                .Cache
                .AddValidationCallback(SetAnonymousCaching, null);

            base.OnActionExecuting(filterContext);
        }

        static void SetAnonymousCaching(HttpContext context, Object data, ref HttpValidationStatus status)
        {
            if(context.User.Identity.IsAuthenticated)
            {
                status = HttpValidationStatus.IgnoreThisRequest;
            }
            else
            {
                status = HttpValidationStatus.Valid;
            }
        }
    }
}
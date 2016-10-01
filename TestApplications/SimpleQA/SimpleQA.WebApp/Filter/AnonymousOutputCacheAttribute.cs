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
            // Caching checks precede the authentication check.
            // So unless you check here, once page is cached for anonymous, users
            // still see the cached version after login

            // TODO : Should I merge this with the authentication attribute?

            var cookie = context.Request.Cookies[Constant.CookieKey];
            if (cookie != null && !String.IsNullOrWhiteSpace(cookie.Value))
            {
                var dispatcher = DependencyResolver.Current.GetService<ICommandExecuterMediator>();

                var command = new ValidateSessionCommand(cookie.Value);
                try
                {
                    var result = dispatcher.ExecuteAsync<ValidateSessionCommand, ValidateSessionCommandResult>(command, context.User, CancellationToken.None).Result;
                    if (result.IsValid)
                    {
                        status = HttpValidationStatus.IgnoreThisRequest;
                        return;
                    }
                }
                catch (Exception)
                {
                    status = HttpValidationStatus.Invalid;
                    return;
                }
            }

            status = HttpValidationStatus.Valid;
        }
    }
}

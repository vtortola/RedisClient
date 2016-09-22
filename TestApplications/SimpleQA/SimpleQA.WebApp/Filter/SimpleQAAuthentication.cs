using SimpleQA.Commands;
using System;
using System.Threading;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace SimpleQA.WebApp.Filter
{
    public class SimpleQAAuthentication : ActionFilterAttribute,  IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            var cookie = filterContext.RequestContext.HttpContext.Request.Cookies[Constant.CookieKey];
            if (cookie != null && !String.IsNullOrWhiteSpace(cookie.Value))
            {
                var dispatcher = DependencyResolver.Current.GetService<ICommandExecuterMediator>();

                var command = new ValidateSessionCommand(cookie.Value);
                try
                {
                    var result = dispatcher.ExecuteAsync<ValidateSessionCommand, ValidateSessionCommandResult>(command, filterContext.HttpContext.User, CancellationToken.None).Result;
                    if (result.IsValid)
                        filterContext.RequestContext.HttpContext.User = new SimpleQAPrincipal(result.Id, result.UserName, cookie.Value, result.InboxCount);
                }
                catch(Exception ex)
                {
                    filterContext.RequestContext.HttpContext.User = SimpleQAPrincipal.Anonymous;
                    throw new SimpleQAException("Error performing session validation.", ex);
                }
            }
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {

        }
    }
}
using NLog;
using SimpleQA.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Security;

namespace SimpleQA.WebApp.Filter
{
    public class SetIdentityAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var cookie = filterContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (cookie != null && !String.IsNullOrWhiteSpace(cookie.Value))
                {
                    var sessionId = FormsAuthentication.Decrypt(cookie.Value).UserData;

                    var dispatcher = DependencyResolver.Current.GetService<ICommandExecuterMediator>();

                    var command = new ValidateSessionCommand(sessionId);
                    try
                    {
                        var result = dispatcher.ExecuteAsync<ValidateSessionCommand, ValidateSessionCommandResult>(command, filterContext.HttpContext.User, CancellationToken.None).Result;
                        if (result.IsValid)
                        
                        return;
                    }
                    catch (Exception ex)
                    {
                        DependencyResolver
                            .Current
                            .GetService<ILogger>()
                            .Error(ex, "Error performing session validation.");
                    }
                }
            }

            filterContext.HttpContext.User = SimpleQAPrincipal.Anonymous;
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {

        }
    }
}

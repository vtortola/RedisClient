using SimpleQA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Filter
{
    public class ValidateSessionAttribute : ActionFilterAttribute
    {
        

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;
            if(request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                if (filterContext.HttpContext.User.Identity.IsAuthenticated && request.Form.AllKeys.Any(x => x == Constant.CookieKey))
                {
                    var cookie = filterContext.RequestContext.HttpContext.Request.Cookies[Constant.CookieKey];
                    if (cookie != null && !String.IsNullOrWhiteSpace(cookie.Value) && cookie.Value == request.Form[Constant.CookieKey])
                    {
                        return;
                    }
                }

                throw new SimpleQAException("Your session has expired or your security token is not valid.");    
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
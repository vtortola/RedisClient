using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Filter
{
    public class AuthorizeWriteAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "Your session has ended.");
                }
                else
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "~/Views/Error/SessionEnded.cshtml",
                        ViewData = new ViewDataDictionary(),
                        TempData = filterContext.Controller.TempData
                    };
                }
            }
        }
    }
}
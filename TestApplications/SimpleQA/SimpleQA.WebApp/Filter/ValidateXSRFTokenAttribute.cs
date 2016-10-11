using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Filter
{
    // ValidateAntiForgeryToken does not work properly with AJAX or multiple forms in the same page
	public class ValidateXSRFTokenAttribute : ActionFilterAttribute
	{
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var form = filterContext.HttpContext.Request.Form[Constant.XSRFVarName];
            var cookie = filterContext.HttpContext.Request.Cookies[Constant.XSRFCookie];

            if (form != cookie.Value)
            {

                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid XSRF token. Please refresh and try again.");
                }
                else
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "~/Views/Error/InvalidToken.cshtml",
                        ViewData = new ViewDataDictionary(),
                        TempData = filterContext.Controller.TempData
                    };
                }
            }
        }
	}
}
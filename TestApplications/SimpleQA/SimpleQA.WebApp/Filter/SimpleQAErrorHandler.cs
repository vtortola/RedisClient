using NLog;
using SimpleQA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace SimpleQA.WebApp.Filter
{
    public sealed class SimpleQAErrorHandler : ActionFilterAttribute, IExceptionFilter
    {
        readonly ILogger _logger;
        public SimpleQAErrorHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception != null && !filterContext.ExceptionHandled)
            {
                var simpleQaException = filterContext.Exception as SimpleQAException;
                if (simpleQaException == null)
                {
                    simpleQaException = new SimpleQAException("Unexpected error", filterContext.Exception);
                }

                _logger.Error(simpleQaException, simpleQaException.Message);

                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    var message = simpleQaException.Message;
                    if (simpleQaException.InnerException != null)
                        message += ("(" + simpleQaException.InnerException.Message + ")");
                    filterContext.Result = new HttpStatusCodeResult(500, message);
                }
                else if (simpleQaException is SimpleQAAuthenticationException)
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "~/Views/Error/AuthenticationFailed.cshtml",
                        ViewData = new ViewDataDictionary<SimpleQAException>(simpleQaException),
                        TempData = filterContext.Controller.TempData
                    };
                }
                else
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "~/Views/Error/Display.cshtml",
                        ViewData = new ViewDataDictionary<SimpleQAException>(simpleQaException),
                        TempData = filterContext.Controller.TempData
                    };
                }
                filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                filterContext.ExceptionHandled = true;
            }
        }
    }
}
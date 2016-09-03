using SimpleQA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Filter
{
    public class EnforceValidModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.Controller.ViewData.ModelState.IsValid)
            {
                FixTagsError(filterContext.Controller.ViewData.ModelState);
                throw new SimpleQABadRequestException(filterContext.Controller.ViewData.ModelState);
            }
        }

        private void FixTagsError(ModelStateDictionary modelState)
        {
            if (modelState.Any(e => e.Key == "Tags"))
            {
                var error = modelState["Tags"];
                modelState.Remove("Tags");
                modelState.AddModelError(String.Empty, error.Errors.First().ErrorMessage);
            }
        }

    }
}
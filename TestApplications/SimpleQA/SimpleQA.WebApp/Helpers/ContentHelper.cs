using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Helpers
{
    public static class ContentHelper
    {
        public static MvcHtmlString BreakContent(this HtmlHelper helper, String content)
        {
            return MvcHtmlString.Create(content.Replace("\n", "<br/>"));
        }
    }
}
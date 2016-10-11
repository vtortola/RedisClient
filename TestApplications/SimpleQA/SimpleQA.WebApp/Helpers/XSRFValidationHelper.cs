using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Helpers
{
    public static class XSRFValidationHelper
    {
        public static MvcHtmlString SessionTokenInput(this HtmlHelper helper, IPrincipal user)
        {
            var token = GetOrCreateToken(helper.ViewContext.HttpContext);
            var builder = new TagBuilder("input");
            builder.Attributes.Add("type", "hidden");
            builder.Attributes.Add("name", Constant.XSRFVarName);
            var prin = user as SimpleQAPrincipal;
            if (prin != null)
            {
                builder.Attributes.Add("value", token);
            }
            return MvcHtmlString.Create(builder.ToString());
        }

        private static String GetOrCreateToken(HttpContextBase context)
        {
            if (context.Response.Cookies.AllKeys.Contains(Constant.XSRFCookie))
            {
                return context.Response.Cookies[Constant.XSRFCookie].Value;
            }
            else
            {
                var token = Guid.NewGuid().ToString().Replace("-", String.Empty);
                context.Response.Cookies.Add(new HttpCookie(Constant.XSRFCookie, token));
                return token;
            }
        }
        
        public static MvcHtmlString GetSessionToken(this HttpContextBase context)
        {
            var token = context.User.Identity.IsAuthenticated ? 
                GetOrCreateToken(context) :
                String.Empty;
            return MvcHtmlString.Create(token);
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Helpers
{
    public static class SessionHelper
    {
        public static MvcHtmlString SessionTokenInput(this HtmlHelper helper, IPrincipal user)
        {
            var builder = new TagBuilder("input");
            builder.Attributes.Add("type", "hidden");
            builder.Attributes.Add("name", "sessionId");
            var prin = user as SimpleQAPrincipal;
            if (prin != null)
            {
                builder.Attributes.Add("value", prin.GetSimpleQAIdentity().Session);
            }
            return MvcHtmlString.Create(builder.ToString());
        }


        public static MvcHtmlString GetSessionToken(this IPrincipal user)
        {
            var session = String.Empty;
            var prin = user as SimpleQAPrincipal;
            if (prin != null)
            {
                session = prin.GetSimpleQAIdentity().Session;
            }
            return MvcHtmlString.Create(session);
        }
    }

}
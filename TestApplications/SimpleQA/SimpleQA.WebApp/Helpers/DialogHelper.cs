using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Helpers
{
    public static class DialogHelper
    {
        public static MvcHtmlString BootstrapDialog(this HtmlHelper helper, String id)
        {
            var loading = new TagBuilder("div");
            loading.Attributes.Add("class", "glyphicon glyphicon-refresh loading");
            
            var content = new TagBuilder("div");
            content.Attributes.Add("class", "modal-content");
            content.InnerHtml = loading.ToString();
 
            var dialog = new TagBuilder("div");
            dialog.Attributes.Add("class", "modal-dialog");
            dialog.InnerHtml = content.ToString();

            var container = new TagBuilder("div");
            container.Attributes.Add("id", id);
            container.Attributes.Add("class", "modal fade");
            container.Attributes.Add("tabindex", "-1");
            container.Attributes.Add("role", "dialog");
            container.InnerHtml = dialog.ToString();

            return MvcHtmlString.Create(container.ToString());
            
        }
    }
}
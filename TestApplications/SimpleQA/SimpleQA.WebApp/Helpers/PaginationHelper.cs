using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Helpers
{
    public static class PaginationHelper
    {
        public static MvcHtmlString Pagination<T>(this HtmlHelper<T> helper, Int32 pagesToShow, Func<Int32, String> urlFactory)
            where T:IPaginable
        {
            var buttonList = new StringBuilder();
            var model = helper.ViewData.Model;
            var first = 1;
            var last = Math.Min(pagesToShow, model.TotalPages);

            if (model.Page > 1)
            {
                var half = pagesToShow / 2; // integer division
                first = Math.Max(1, model.Page - half);
                var skipped = 0;
                if (first < half)
                    skipped = half - first;
                last = Math.Min(model.TotalPages, model.Page + half + skipped);
            }

            for (int i = first; i <= last; i++)
            {
                buttonList.AppendLine(CreateButton(urlFactory, i, i == model.Page ? "active" : null).ToString());
            }

            var builder = new TagBuilder("ul");
            builder.AddCssClass("pagination");
            builder.InnerHtml = buttonList.ToString();
            return MvcHtmlString.Create(builder.ToString());
        }

        static TagBuilder CreateButton(Func<Int32, String> urlFactory, Int32 page, String className = null)
        {
            var anchor = new TagBuilder("a");
            anchor.Attributes.Add("href", urlFactory(page));
            anchor.SetInnerText(page.ToString());
            var button = new TagBuilder("li");
            if (!String.IsNullOrWhiteSpace(className))
                button.AddCssClass(className);
            button.InnerHtml = anchor.ToString();
            return button;    
        }
    }
}
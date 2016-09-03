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
            if(model.Page > 1)
            {
                for (int i = model.Page - 1; i > 0; i--)
                {
                    buttonList.AppendLine(CreateButton(urlFactory, i).ToString());
                    pagesToShow--;
                }
            }
            buttonList.AppendLine(CreateButton(urlFactory, model.Page, "active").ToString());
            if (model.Page < model.TotalPages)
            {
                for (int i = model.Page + 1; i < Math.Min(model.Page+pagesToShow, model.TotalPages + 1) ; i++)
                {
                    buttonList.AppendLine(CreateButton(urlFactory, i).ToString());
                }
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
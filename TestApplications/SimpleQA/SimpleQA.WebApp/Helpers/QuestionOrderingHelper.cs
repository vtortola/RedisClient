using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Helpers
{
    public static class QuestionOrderingHelper
    {
        public static MvcHtmlString QuestionSorter<T>(this HtmlHelper<T> helper, String title, Func<QuestionSorting, String> urlFactory)
            where T : IOrderable
        {
            // todo : use t4 template for this
            var model = helper.ViewData.Model;

            var titleTag = new TagBuilder("h2");
            titleTag.SetInnerText(title);
            titleTag.AddCssClass("home-header-title");
            var titleLi = new TagBuilder("li");
            titleLi.InnerHtml = titleTag.ToString();

            var intersting = CreateListItem(model, QuestionSorting.ByScore, "Interesting", urlFactory);
            var newest = CreateListItem(model, QuestionSorting.ByDate, "Newest", urlFactory);

            var ul = new TagBuilder("ul");
            ul.AddCssClass("nav nav-tabs navbar-right home-header-nav");
            ul.InnerHtml = titleLi.ToString() + intersting.ToString() + newest.ToString();

            return MvcHtmlString.Create(ul.ToString());
        }

        static TagBuilder CreateListItem<T>(T model, QuestionSorting sorting, String title, Func<QuestionSorting, String> urlFactory) 
            where T : IOrderable
        {
            var anchor = new TagBuilder("a");
            anchor.Attributes.Add("href", urlFactory(sorting));
            anchor.SetInnerText(title);

            var listItem = new TagBuilder("li");
            if (model.Sorting == sorting)
                listItem.AddCssClass("active");
            listItem.InnerHtml = anchor.ToString();

            return listItem;
        }
    }
}
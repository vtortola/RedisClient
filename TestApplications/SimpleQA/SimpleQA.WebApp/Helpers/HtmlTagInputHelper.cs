using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace SimpleQA.WebApp.Helpers
{
    public static class HtmlTagInputHelper
    {
        public static MvcHtmlString TagsFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, String[]>> property, String tagSuggestionUrl, Object htmlAttributes)
        {
            var member = ((MemberExpression)property.Body).Member;
            var name = member.Name;
            var attrs = member.GetCustomAttributesData();
            var values = property.Compile().Invoke(helper.ViewData.Model);
            var dic = new RouteValueDictionary(htmlAttributes);
            dic.Add("data-tags-url", tagSuggestionUrl);
            dic.Add("data-val", "true");
            dic.Add("data-val-required", "At least one tag is required.");
            var textbox = helper.TextBox(name, String.Join(",", values), dic);
            return textbox;
        }
    }
}
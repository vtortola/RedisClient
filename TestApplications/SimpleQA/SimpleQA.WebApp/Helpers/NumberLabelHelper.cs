using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Helpers
{
    public static class NumberLabelHelper
    {
        public static MvcHtmlString AsLabel(this Int32 number)
        {
            var label = String.Empty;
            if (number < 1000)
                label = number.ToString();
            else if (number < 1100)
                label = "1K";
            else
                label = (number / 1000D).ToString(".0") + "K";

            return MvcHtmlString.Create(label);
        }
    }
}
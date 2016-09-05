using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Helpers
{
    public static class RelativeDateTimeHelper
    {
        public static MvcHtmlString RelativeDate(this DateTime datetime)
        {
            var now = DateTime.Now;
            var diff = now - datetime;
            var relative = String.Empty;

            if (diff.TotalMinutes < 60)
            {
                relative = Truncate(diff.TotalMinutes).ToString() + " minutes ago.";
            }
            else if(diff.TotalHours < 24)
            {
                relative = Truncate(diff.TotalHours).ToString() + " hours ago.";
            }
            else if(diff.TotalDays < 31)
            {
                relative = Truncate(diff.TotalDays).ToString() + " days ago.";
            }
            else if (diff.TotalDays < 365)
            {
                // todo: asuming 31 days months. Which is wrong.
                relative = Truncate(diff.TotalDays / 31).ToString() + " months ago.";
            }
            else
            {
                relative = String.Format("{0} years ago.", now.Year - datetime.Year);
            }

            return MvcHtmlString.Create(relative);
        }

        static Int32 Truncate(Double value)
        {
            return (Int32)value;
        }
    }
}
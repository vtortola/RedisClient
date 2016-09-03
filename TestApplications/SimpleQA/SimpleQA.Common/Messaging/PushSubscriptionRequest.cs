using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQA.Messaging
{
    public sealed class PushSubscriptionRequest
    {
        public String Topic { get; set; }
    }

    public sealed class PushMessage
    {
        public String Topic { get; set; }
        public String Change { get; set; }
    }
}
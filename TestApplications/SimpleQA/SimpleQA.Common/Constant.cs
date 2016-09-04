using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQA
{
    // This should be in a configuration storage
    public sealed class Constant
    {
        public const String CookieKey = "sessionId";

        public const Int64 CloseVotesRequired = 10;

        public static readonly Int64 VoteScore = TimeSpan.FromDays(1).Ticks;

        public const Int32 ItemsPerPage = 10;
    }
}
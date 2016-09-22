using System;
using System.Security.Principal;

namespace SimpleQA
{
    public sealed class SimpleQAIdentity : IIdentity
    {
        public String AuthenticationType { get; private set; }
        public Boolean IsAuthenticated { get; private set; }
        public String Name { get; private set; }
        public String Session { get; private set; }
        public Int32 InboxCount { get; set; }

        public String Id { get; private set; }

        public SimpleQAIdentity(String id, String name, String session, Int32 inboxCount)
        {
            Name = name;
            Id = id;
            Session = session;
            InboxCount = inboxCount;
            IsAuthenticated = true;
        }

        public SimpleQAIdentity()
        {
            Id = Name = "Anonymous";
        }
    }
}

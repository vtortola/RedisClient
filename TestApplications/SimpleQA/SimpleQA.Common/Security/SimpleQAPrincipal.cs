using System;
using System.Security.Principal;

namespace SimpleQA
{
    public class SimpleQAPrincipal : IPrincipal
    {
        class SimpleQAIdentity : IIdentity
        {
            public String AuthenticationType { get; private set; }
            public Boolean IsAuthenticated { get; private set; }
            public String Name { get; private set; }

            public SimpleQAIdentity(String name)
            {
                Name = name;
                IsAuthenticated = true;
            }

            public SimpleQAIdentity()
            {
                Name = "Anonymous";
            }
        }

        public IIdentity Identity { get; private set; }
        public String Session { get; private set; }
        public Int32 InboxCount { get; set; }

        public static SimpleQAPrincipal Anonymous = new SimpleQAPrincipal();

        private SimpleQAPrincipal()
        {
            Identity = new SimpleQAIdentity();
        }

        public SimpleQAPrincipal(String name, String session, Int32 inboxCount)
        {
            Identity = new SimpleQAIdentity(name);
            Session = session;
            InboxCount = inboxCount;
        }

        public Boolean IsInRole(string role)
        {
            return false;
        }
    }

    public static class SimpleQAPrincipalExtensions
    {
        public static SimpleQAPrincipal AsSimpleQAPrincipal(this IPrincipal principal)
        {
            return principal as SimpleQAPrincipal;
        }
    }
}
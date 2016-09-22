using System;
using System.Security.Principal;

namespace SimpleQA
{
    public class SimpleQAPrincipal : IPrincipal
    {
        public IIdentity Identity { get { return SimpleQAIdentity; } }
        public SimpleQAIdentity SimpleQAIdentity { get; private set; }
        public static SimpleQAPrincipal Anonymous = new SimpleQAPrincipal();

        private SimpleQAPrincipal()
        {
            SimpleQAIdentity = new SimpleQAIdentity();
        }

        public SimpleQAPrincipal(String id, String name, String session, Int32 inboxCount)
        {
            SimpleQAIdentity = new SimpleQAIdentity(id, name, session, inboxCount);
        }

        public Boolean IsInRole(string role)
        {
            return false;
        }
    }

    public static class SimpleQAPrincipalExtensions
    {
        public static SimpleQAIdentity GetSimpleQAIdentity(this IPrincipal principal)
        {
            var identity = principal.Identity as SimpleQAIdentity;
            return identity ?? SimpleQAPrincipal.Anonymous.SimpleQAIdentity;
        }
    }
}
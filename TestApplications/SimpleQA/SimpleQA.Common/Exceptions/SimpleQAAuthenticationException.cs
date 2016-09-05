using System;

namespace SimpleQA
{
    public class SimpleQAAuthenticationException : SimpleQAException
    {
        public SimpleQAAuthenticationException(String message)
            : base(message)
        {
        }

        public SimpleQAAuthenticationException(String message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

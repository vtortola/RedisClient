using System;

namespace SimpleQA
{
    public class SimpleQANotOwnerException : SimpleQAException
    {
        public SimpleQANotOwnerException(String message)
            : base(message)
        {

        }
    }
}
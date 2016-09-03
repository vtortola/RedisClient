using System;

namespace SimpleQA
{
    public class SimpleQAException : Exception
    {
        public String Id { get; private set; }

        public SimpleQAException(String message)
            : base(message)
        {
            Id = Guid.NewGuid().ToString();
        }

        public SimpleQAException(String message, Exception inner)
            : base(message, inner)
        {
            Id = Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return "ErrorId:" + Id.ToString() + ", " + base.ToString();
        }
    }
}
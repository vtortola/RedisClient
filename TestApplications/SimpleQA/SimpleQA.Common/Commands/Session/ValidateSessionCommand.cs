using System;

namespace SimpleQA.Commands
{
    public class ValidateSessionCommand : ICommand<ValidateSessionCommandResult>
    {
        public String SessionId { get; private set; }

        public ValidateSessionCommand(String sessionId)
        {
            SessionId = sessionId;
        }
    }

    public class ValidateSessionCommandResult
    {
        public static readonly ValidateSessionCommandResult NonValid = new ValidateSessionCommandResult();

        public String User { get; private set; }
        public Boolean IsValid { get; private set; }
        public Int32 InboxCount { get; private set; }
        public ValidateSessionCommandResult(String user, Int32 inboxCount)
        {
            User = user;
            IsValid = true;
            InboxCount = inboxCount;
        }
        private ValidateSessionCommandResult()
        {
    
        }
    }
}
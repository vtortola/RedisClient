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

        public String Id { get; private set; }
        public String UserName { get; private set; }
        public Boolean IsValid { get; private set; }
        public Int32 InboxCount { get; private set; }
        public ValidateSessionCommandResult(String id, String userName, Int32 inboxCount)
        {
            Id = id;
            UserName = userName;
            IsValid = true;
            InboxCount = inboxCount;
        }

        private ValidateSessionCommandResult(){}
    }
}
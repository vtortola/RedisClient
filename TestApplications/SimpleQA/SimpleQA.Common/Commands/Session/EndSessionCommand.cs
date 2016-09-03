using System;

namespace SimpleQA.Commands
{
    public class EndSessionCommand : ICommand<EndSessionCommandResult>
    {
        public String SessionId { get; private set; }
        public EndSessionCommand(String sessionId)
        {
            SessionId = sessionId;
        }
    }

    public class EndSessionCommandResult
    {
        public static readonly EndSessionCommandResult Instance = new EndSessionCommandResult();

        private EndSessionCommandResult()
        {

        }
    }
}
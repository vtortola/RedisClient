using System;

namespace SimpleQA.Commands
{
    public class AuthenticateCommand : ICommand<AuthenticateCommandResult>
    {
        public String Username { get; private set; }
        public String Password { get; private set; }

        public AuthenticateCommand(String username, String password)
        {
            Username = username;
            Password = password;
        }
    }

    public class AuthenticateCommandResult
    {
        public String SessionId { get; private set; }
        public AuthenticateCommandResult(String sessionId)
        {
            SessionId = sessionId;
        }
    }
}
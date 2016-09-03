using SimpleQA.Commands;
using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class AuthenticateCommandExecuter : ICommandExecuter<AuthenticateCommand, AuthenticateCommandResult>
    {
        readonly IRedisChannel _channel;
        public AuthenticateCommandExecuter(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<AuthenticateCommandResult> ExecuteAsync(AuthenticateCommand command, IPrincipal user, CancellationToken cancel)
        {
            var userData = new
            {
                name = command.Username
            };

            var session = Keys.GenerateUserSession();
            var userKey = Keys.UserKey(command.Username);

            var sessionDuration = TimeSpan.FromMinutes(5).TotalSeconds;

            var result = await _channel.ExecuteAsync(@"
                                        HSETNX @userKey @userData
                                        SETEX @session @sessionDuration @Username",
                                        new { userKey, userData = Parameter.SequenceProperties(userData), session, command.Username, sessionDuration })
                                        .ConfigureAwait(false);

            return new AuthenticateCommandResult(session);
        }
    }
}

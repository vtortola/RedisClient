using SimpleQA.Commands;
using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class ValidateSessionCommandExecuter : ICommandExecuter<ValidateSessionCommand, ValidateSessionCommandResult>
    {
        readonly IRedisChannel _channel;
        public ValidateSessionCommandExecuter(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<ValidateSessionCommandResult> ExecuteAsync(ValidateSessionCommand command, IPrincipal user, CancellationToken cancel)
        {
            var result = await _channel.ExecuteAsync("GET @SessionId", new { command.SessionId }).ConfigureAwait(false);
            var userName = result[0].GetString();

            var sessionDuration = TimeSpan.FromMinutes(5).TotalSeconds;

            if (!String.IsNullOrWhiteSpace(userName))
            {
                result = await _channel.ExecuteAsync(@"
                                    EXPIRE @SessionId @sessionDuration
                                    SCARD @inbox", new { command.SessionId, sessionDuration, inbox = Keys.UserInboxKey(userName)});
                return new ValidateSessionCommandResult(userName, (Int32)result[1].GetInteger());
            }
            else
            {
                return ValidateSessionCommandResult.NonValid;
            }
        }
    }
}

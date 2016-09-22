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
            var sessionDuration = TimeSpan.FromMinutes(5).TotalSeconds;

            var result = await _channel.ExecuteAsync(
                                        "ValidateSession {user} @SessionId @sessionDuration",
                                         new { command.SessionId, sessionDuration })
                                        .ConfigureAwait(false);

            var error = result[0].GetException();
            if (error != null)
            {
                switch (error.Prefix)
                {
                    case "NOTVALID":
                        return ValidateSessionCommandResult.NonValid;
                    default: throw error;
                }
            }
            result = result[0].AsResults();

            return new ValidateSessionCommandResult(result[0].GetString(), result[1].GetString(), (Int32)result[2].GetInteger());
        }
    }
}

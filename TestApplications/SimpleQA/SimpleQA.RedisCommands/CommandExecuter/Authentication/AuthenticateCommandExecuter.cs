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

        // Dummy authentication
        public async Task<AuthenticateCommandResult> ExecuteAsync(AuthenticateCommand command, IPrincipal user, CancellationToken cancel)
        {
            // Preventing user from login in with a user from a data dump
            // because markdown code is missing and cannto be edited
            if(user.Identity.Name != "dumpprocessor")
            {
                var ismember = await _channel.ExecuteAsync("SISMEMBER {user}:builtin @user", new { user = command.Username }).ConfigureAwait(false);
                if (ismember[0].GetInteger() == 1)
                    throw new SimpleQAAuthenticationException("It is a built-in user.");
            }

            var session = GenerateLOLRandomIdentifier();
            var newid = GenerateLOLRandomIdentifier();
            var sessionDuration = TimeSpan.FromMinutes(5).TotalSeconds;
            
            
            var result = await _channel.ExecuteAsync(
                                        "Authenticate {user} @username @newid @session @sessionduration",
                                        new 
                                        {
                                            username = command.Username,
                                            newid,
                                            session,
                                            sessionDuration
                                        })
                                        .ConfigureAwait(false);
            result.ThrowErrorIfAny();
            var userId = result[0].GetString();

            // This should be done during registration
            // but since this demo uses dummy authenticaion
            // is done here.
            await _channel.ExecuteAsync(
                           "HSET {question}:uidmapping @userId @username",
                           new { userId, username = command.Username })
                           .ConfigureAwait(false);

            return new AuthenticateCommandResult(session);
        }

        static String GenerateLOLRandomIdentifier()
        {
            return Guid.NewGuid().ToString().Replace("-", String.Empty);
        }
    }
}

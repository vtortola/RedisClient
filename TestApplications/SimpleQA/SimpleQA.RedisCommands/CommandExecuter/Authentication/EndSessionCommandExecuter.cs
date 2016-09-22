using SimpleQA.Commands;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class EndSessionCommandExecuter : ICommandExecuter<EndSessionCommand, EndSessionCommandResult>
    {
        readonly IRedisChannel _channel;
        public EndSessionCommandExecuter(IRedisChannel channel)
        {
            _channel = channel;
        }

        public Task<EndSessionCommandResult> ExecuteAsync(EndSessionCommand command, IPrincipal user, CancellationToken cancel)
        {
            _channel.Dispatch("EndSession {user} @SessionId", new { command.SessionId });
            return Task.FromResult(EndSessionCommandResult.Instance);
        }
    }
}

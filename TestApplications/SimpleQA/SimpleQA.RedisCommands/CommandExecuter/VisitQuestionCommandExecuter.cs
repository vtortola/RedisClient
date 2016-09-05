using SimpleQA.Commands;
using System.Security.Principal;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public class VisitQuestionCommandExecuter : ICommandExecuter<VisitQuestionCommand, VisitQuestionCommandResult>
    {
        readonly IRedisChannel _channel;

        public VisitQuestionCommandExecuter(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<VisitQuestionCommandResult> ExecuteAsync(VisitQuestionCommand command, IPrincipal user, System.Threading.CancellationToken cancel)
        {
            var key = new
            {
                key = Keys.QuestionKey(command.QuestionId),
                views = command.Views
            };

            var result = await _channel.ExecuteAsync("EXISTS @key", key).ConfigureAwait(false);
            if (result[0].GetInteger() == 1)
            {
                await _channel.ExecuteAsync("HINCRBY @key ViewCount @views", key).ConfigureAwait(false);
            }

            return VisitQuestionCommandResult.Nothing;
        }
    }
}

using SimpleQA.Commands;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class AnswerDeleteCommandExecuter : ICommandExecuter<AnswerDeleteCommand, AnswerDeleteCommandResult>
    {
        readonly IRedisChannel _channel;
        public AnswerDeleteCommandExecuter(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<AnswerDeleteCommandResult> ExecuteAsync(AnswerDeleteCommand command, IPrincipal user, CancellationToken cancel)
        {
            var parameters = new
            {
                key = Keys.AnswerKey(command.QuestionId, command.AnswerId),
                colkey = Keys.AnswerCollectionKey(command.QuestionId),
                qKey = Keys.QuestionKey(command.QuestionId)
            };

            var result = await _channel.ExecuteAsync(@"
                                            HGET @key User
                                            HGET @qKey Slug",
                                            parameters).ConfigureAwait(false);

            var answerUser = result[0].GetString();

            if (answerUser != user.Identity.Name)
                throw new SimpleQANotOwnerException("You cannot delete an answer you did not create.");

            var slug = result[1].GetString();

            result = await _channel.ExecuteAsync(@"
                                    DEL @key
                                    SREM  @colkey @key
                                    HDECRBY @qKey Answers 1",
                                    parameters).ConfigureAwait(false);

            return new AnswerDeleteCommandResult(command.QuestionId, slug);
        }
    }
}

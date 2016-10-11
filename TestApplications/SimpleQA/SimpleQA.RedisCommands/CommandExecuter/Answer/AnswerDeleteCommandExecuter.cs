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

        public async Task<AnswerDeleteCommandResult> ExecuteAsync(AnswerDeleteCommand command, SimpleQAIdentity user, CancellationToken cancel)
        {
            var result = await _channel.ExecuteAsync(
                                        "DeleteAnswer {question} @answerId @userId",
                                        new
                                        {
                                            answerId = command.AnswerId,
                                            userId = user.Id
                                        })
                                        .ConfigureAwait(false);
            CheckException(result);
            var slug = result[0].GetString();

            return new AnswerDeleteCommandResult(command.QuestionId, slug);
        }

        static void CheckException(IRedisResults result)
        {
            var error = result[0].GetException();
            if (error != null)
            {
                switch (error.Prefix)
                {
                    case "NOTOWNER":
                        throw new SimpleQAException("You are not the author of the answer you try to delete.");
                    default:
                        throw error;
                }
            }
        }
    }
}

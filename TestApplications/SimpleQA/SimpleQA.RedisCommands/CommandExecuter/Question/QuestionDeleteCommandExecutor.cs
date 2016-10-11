using SimpleQA.Commands;
using SimpleQA.Models;
using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class QuestionDeleteCommandExecutor : ICommandExecuter<QuestionDeleteCommand, QuestionDeleteCommandResult>
    {
        readonly IRedisChannel _channel;

        public QuestionDeleteCommandExecutor(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<QuestionDeleteCommandResult> ExecuteAsync(QuestionDeleteCommand command, SimpleQAIdentity user, CancellationToken cancel)
        {
            var result = 
                await _channel.ExecuteAsync(
                               "QuestionDelete {question} @id @userId", 
                               new 
                               { 
                                   id = command.Id,
                                   userId = user.Id
                               })
                               .ConfigureAwait(false);

            CheckException(result);

            result = result[0].AsResults();

            var slug = result[0].GetString();
            var tags = result[1].GetStringArray();

            result = await _channel.ExecuteAsync(@"
                                        UnindexQuestion {questions} @id
                                        UnindexQuestionTags {tag} @id @score @tags",
                                        new
                                        {
                                            id = command.Id,
                                            tags,
                                            score = Constant.VoteScore
                                        }).ConfigureAwait(false);

            return new QuestionDeleteCommandResult(command.Id, slug);
        }

        static void CheckException(IRedisResults result)
        {
            var error = result[0].GetException();
            if (error != null)
            {
                switch (error.Prefix)
                {
                    case "NOTOWNER":
                        throw new SimpleQANotOwnerException("You cannot delete a question that is not yours.");

                    case "CANNOTCLOSE":
                        throw new SimpleQANotOwnerException("Tne question is not open anymore.");

                    default: throw error;
                }
            }
        }
    }
}

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

        public async Task<QuestionDeleteCommandResult> ExecuteAsync(QuestionDeleteCommand command, IPrincipal user, CancellationToken cancel)
        {
            var questionKey = Keys.QuestionKey(command.Id);
            var result = await _channel.ExecuteAsync("HMGET @questionKey User Status Slug", new { questionKey }).ConfigureAwait(false);

            var results = result[0].GetStringArray();

            var questionUser = results[0];
            if (questionUser != user.Identity.Name)
                throw new SimpleQANotOwnerException("You cannot delete a question that is not yours.");

            var status = (QuestionStatus)Enum.Parse(typeof(QuestionStatus), results[1]);
            if (status != QuestionStatus.Open)
                throw new SimpleQAException("You cannot delete a question that is Closed or already Deleted.");

            var slug = results[2];

            var questionByTime = Keys.QuestionsByDate();
            var questionByScore = Keys.QuestionsByScore();

            result = await _channel.ExecuteAsync(@"
                                        MULTI
                                        ZREM @questionByTime @questionKey
                                        ZREM @questionByScore @questionKey
                                        HSET @questionKey Status @deleted
                                        EXEC",
                                        new
                                        {
                                            questionKey,
                                            deleted = QuestionStatus.Deleted,
                                            questionByTime,
                                            questionByScore
                                        }).ConfigureAwait(false);

            return new QuestionDeleteCommandResult(command.Id, slug);
        }
    }
}

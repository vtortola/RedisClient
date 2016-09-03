using SimpleQA.Commands;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class QuestionCloseCommandExecuter : ICommandExecuter<QuestionCloseCommand, QuestionCloseCommandResult>
    {
        readonly IRedisChannel _channel;

        public QuestionCloseCommandExecuter(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<QuestionCloseCommandResult> ExecuteAsync(QuestionCloseCommand command, IPrincipal user, CancellationToken cancel)
        {
            var questionKey = Keys.QuestionKey(command.Id);
            var username = user.Identity.Name;

            var result = await _channel.ExecuteAsync("HMGET @questionKey User CloseStatus Slug", new { questionKey }).ConfigureAwait(false);

            var data = result[0].GetStringArray();
            var questionUser = data[0];
            if (questionUser == user.Identity.Name)
                throw new SimpleQANotOwnerException("You cannot close a question that is yours.");

            var slug = data[2];

            if (data[1] != "Closed" && data[1] != "Deleted")
            {
                var closeSet = Keys.QuestionCloseVotesCollectionKey(command.Id);
                result = await _channel.ExecuteAsync(@"
                                            HINCRBY @questionKey CloseVotes 1
                                            SADD @closeSet @username",
                                            new { questionKey, closeSet, username }).ConfigureAwait(false);

                if (result[0].GetInteger() >= Constant.CloseVotesRequired)
                    result = await _channel.ExecuteAsync(@"HSET @key Status Closed",
                                                            new { questionKey }).ConfigureAwait(false);
            }

            return new QuestionCloseCommandResult(command.Id, slug);
        }
    }
}

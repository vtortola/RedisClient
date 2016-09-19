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
            var closeSet = Keys.QuestionCloseVotesCollectionKey(command.Id);
            var votesToClose = Constant.CloseVotesRequired;

            var result = await _channel.ExecuteAsync(@"
                                        QuestionClose @questionKey @closeSet @username @votesToClose
                                        HGET @questionKey Slug", 
                                        new { questionKey, closeSet, username, votesToClose }).ConfigureAwait(false);

            var error = result[0].GetException();
            if (error != null)
            {
                switch (error.Prefix)
                {
                    case "OWNER":
                        throw new SimpleQANotOwnerException("You cannot close a question that is yours.");

                    case "CANNOTCLOSE":
                        throw new SimpleQANotOwnerException("Tne question is not open anymore.");

                    case "ALREADYVOTED":
                        throw new SimpleQANotOwnerException("User already voted to close this question.");

                    default: throw error;
                }
            }

            return new QuestionCloseCommandResult(command.Id, result[1].GetString());
        }
    }
}

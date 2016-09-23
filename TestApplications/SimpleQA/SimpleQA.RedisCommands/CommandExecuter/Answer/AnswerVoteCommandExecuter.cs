using SimpleQA.Commands;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class AnswerVoteCommandExecuter : ICommandExecuter<AnswerVoteCommand, AnswerVoteCommandResult>
    {
        readonly IRedisChannel _channel;
        public AnswerVoteCommandExecuter(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<AnswerVoteCommandResult> ExecuteAsync(AnswerVoteCommand command, IPrincipal user, CancellationToken cancel)
        {
            var result = await _channel.ExecuteAsync(
                                        "VoteAswer {question} @answerId @userId @vote",
                                        new 
                                        { 
                                            answerId = command.AnswerId,
                                            userId = user.GetSimpleQAIdentity().Id,
                                            vote = command.Upvote ? "1" : "-1"
                                        })
                                        .ConfigureAwait(false);

            CheckExceptions(result);
            var votes = result[0].AsIntegerArray();

            return new AnswerVoteCommandResult(votes[0] - votes[1]);
        }

        static void CheckExceptions(IRedisResults result)
        {
            var error = result[0].GetException();
            if (error != null)
            {
                switch (error.Prefix)
                {
                    case "ALREADYVOTED": throw new SimpleQAException("User already voted");
                    default: throw error;
                }
            }
        }
    }
}

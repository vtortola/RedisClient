using SimpleQA.Commands;
using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;
using System.Linq;

namespace SimpleQA.RedisCommands
{
    public sealed class QuestionVoteCommandExecuter : ICommandExecuter<QuestionVoteCommand, QuestionVoteCommandResult>
    {
        readonly IRedisChannel _channel;

        public QuestionVoteCommandExecuter(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<QuestionVoteCommandResult> ExecuteAsync(QuestionVoteCommand command, IPrincipal user, CancellationToken cancel)
        {
            var result = 
                await _channel.ExecuteAsync(
                                        "VoteQuestion {question} @id @userId @score @upvote",
                                        new
                                        {
                                            id = command.QuestionId,
                                            userId = user.GetSimpleQAIdentity().Id,
                                            score = command.Upvote ? Constant.VoteScore : 0 - Constant.VoteScore,
                                            upvote = command.Upvote ? 1 : 0
                                        })
                                        .ConfigureAwait(false);

            CheckExceptions(result);

            result = result[0].AsResults();
            var votes = result[0].AsIntegerArray();
            var tags = result[1].AsStringArray();

            result = await _channel.ExecuteAsync(@"
                                    VoteQuestionGlobally {questions} @id @score
                                    VoteTags {tag} @id @tags @score",
                                    new
                                    {
                                        id = command.QuestionId,
                                        score = command.Upvote ? Constant.VoteScore : 0 - Constant.VoteScore,
                                        tags
                                    })
                                    .ConfigureAwait(false);

            result.ThrowErrorIfAny();
            return new QuestionVoteCommandResult(votes[0] - votes[1]);
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

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
            // pear each tag it has to vote the question 

            var questionKey = Keys.QuestionKey(command.QuestionId);
            var voteTrackerKey = Keys.UserVotesKey(user.Identity.Name);
            var result = await _channel.ExecuteAsync("VoteQuestion @questionKey @field @questionsByScore @score @voteTrackerKey @value @qtags @tagsByScore",
                                    new
                                    {
                                        questionKey,
                                        field = command.Upvote ? "UpVotes" : "DownVotes",
                                        value = command.Upvote ? "1" : "-1",
                                        voteTrackerKey,
                                        questionsByScore = Keys.QuestionsByScore(),
                                        score = command.Upvote ? Constant.VoteScore : 0 - Constant.VoteScore,
                                        tagsByScore = Keys.TagsByScore(),
                                        qtags = Keys.QuestionTagsKey(command.QuestionId)
                                    }).ConfigureAwait(false);

            var procResult = result[0];
            if (procResult.RedisType == RedisType.Error)
            {
                var error = procResult.GetException();
                if (error.OriginalMessage.Contains("ALREADYVOTED"))
                    throw new SimpleQAException("User already voted");
                throw error;
            }
            else if (procResult.RedisType == RedisType.Array)
            {
                var votes = procResult.AsIntegerArray();
                return new QuestionVoteCommandResult(votes[0] - votes[1]);
            }
            else
                throw new Exception("Unexpected result: " + procResult.AsString());
        }
    }
}

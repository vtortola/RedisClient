using SimpleQA.Commands;
using System;
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
            var voteTrackerKey = Keys.AnswerVoteKey(command.QuestionId, command.AnswerId); ;
            var username = user.Identity.Name;

            var result = await _channel.ExecuteAsync(@"
                                        ZSCORE @voteTrackerKey @username",
                                        new { voteTrackerKey, username }).ConfigureAwait(false);

            if (result[0].GetString() != null)
                throw new Exception("User already voted");

            var answerKey = Keys.AnswerKey(command.QuestionId, command.AnswerId);

            result = await _channel.ExecuteAsync(@"
                                    HINCRBY @answerKey @field 1
                                    HINCRBY @answerKey Score @value
                                    ZADD @voteTrackerKey @value @username
                                    HMGET @answerKey UpVotes DownVotes",
                                    new
                                    {
                                        answerKey,
                                        field = command.Upvote ? "UpVotes" : "DownVotes",
                                        value = command.Upvote ? "1" : "-1",
                                        voteTrackerKey,
                                        username
                                    }).ConfigureAwait(false);

            var votes = result[3].AsIntegerArray();
            return new AnswerVoteCommandResult(votes[0] - votes[1]);
        }
    }
}

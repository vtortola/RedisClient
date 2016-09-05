using SimpleQA.Commands;
using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class AnswerCreateCommandExecuter : ICommandExecuter<AnswerCreateCommand, AnswerCreateCommandResult>
    {
        readonly IRedisChannel _channel;
        public AnswerCreateCommandExecuter(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<AnswerCreateCommandResult> ExecuteAsync(AnswerCreateCommand command, IPrincipal user, CancellationToken cancel)
        {
            var store = Keys.QuestionAnswerIdStore(command.QuestionId);
            var questionKey = Keys.QuestionKey(command.QuestionId);

            var result = await _channel.ExecuteAsync(@"
                                            INCR @store
                                            HMGET @questionKey Slug User
                                            HINCRBY @questionKey AnswerCount 1",
                                            new { store, questionKey }).ConfigureAwait(false);

            var id = result[0].GetInteger().ToString();
            var questionData = result[1].GetStringArray();
            var slug = questionData[0];
            var inbox = Keys.UserInboxKey(questionData[1]);

            var data = GetAnswerData(command, user, id);

            var answerKey = Keys.AnswerKey(command.QuestionId, id);
            var answerCollectionKey = Keys.AnswerCollectionKey(command.QuestionId);

            result = await _channel.ExecuteAsync(@"
                                    HMSET @answerKey @data
                                    SADD  @answerCollectionKey @answerKey
                                    HINCRBY @questionKey Answers 1
                                    PUBLISH @inbox @answerKey
                                    SADD @inbox @questionKey",
                                    new { answerKey, data, answerCollectionKey, questionKey, inbox }).ConfigureAwait(false);
            result.ThrowErrorIfAny();
            return new AnswerCreateCommandResult(command.QuestionId, slug, id);
        }

        private static System.Collections.Generic.IEnumerable<string> GetAnswerData(AnswerCreateCommand command, IPrincipal user, string id)
        {
            var data = Parameter.SequenceProperties(new
            {
                Id = id,
                User = user.Identity.Name,
                Content = command.Content,
                HtmlContent = command.HtmlContent,
                CreatedOn = command.CreationDate,
                Score = 0,
                UpVotes = 0,
                DownVotes = 0
            });
            return data;
        }
    }
}

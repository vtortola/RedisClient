using SimpleQA.Commands;
using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class AnswerEditCommandExecuter : ICommandExecuter<AnswerEditCommand, AnswerEditCommandResult>
    {
        readonly IRedisChannel _channel;
        public AnswerEditCommandExecuter(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<AnswerEditCommandResult> ExecuteAsync(AnswerEditCommand command, IPrincipal user, CancellationToken cancel)
        {
            var parameters = new
            {
                key = Keys.AnswerKey(command.QuestionId, command.AnswerId),
                qKey = Keys.QuestionKey(command.QuestionId),
                data = Parameter.SequenceProperties(new
                {
                    ModifiedOn = DateTime.Now,
                    Content = command.Content,
                    HtmlContent = command.HtmlContent
                })
            };

            var result = await _channel.ExecuteAsync(@"
                                            HGET @qKey Slug
                                            HGET @key User",
                                            parameters).ConfigureAwait(false);

            var answerUser = result[1].GetString();

            if (answerUser != user.Identity.Name)
                throw new SimpleQANotOwnerException("You cannot delete an answer you did not create.");

            var slug = result[0].GetString();

            result = await _channel.ExecuteAsync("HMSET @key @data", parameters).ConfigureAwait(false);

            return new AnswerEditCommandResult(command.QuestionId, slug, command.AnswerId);
        }
    }
}

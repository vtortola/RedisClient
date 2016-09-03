using SimpleQA.Commands;
using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class QuestionEditCommandExecuter : ICommandExecuter<QuestionEditCommand, QuestionEditCommandResult>
    {
        readonly IRedisChannel _channel;

        public QuestionEditCommandExecuter(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<QuestionEditCommandResult> ExecuteAsync(QuestionEditCommand command, IPrincipal user, CancellationToken cancel)
        {
            var data = Parameter.SequenceProperties(new
            {
                Title = command.Title,
                Content = command.Content,
                HtmlContent = command.HtmlContent,
                ModifiedOn = DateTime.Now,
            });

            var questionKey = Keys.QuestionKey(command.Id);
            var tagKey = Keys.QuestionTagsKey(command.Id);
            var updates = Keys.QuestionNotification(command.Id);

            var result = await _channel.ExecuteAsync(@"
                                        HMSET @questionKey @data
                                        DEL @tagKey
                                        HGET @questionKey Slug
                                        PUBLISH @updates EDIT",
                                        new { questionKey, data, tagKey, updates }).ConfigureAwait(false);

            var slug = result[2].GetString();
            if (command.Tags != null && command.Tags.Length > 0)
            {
                foreach (var tag in command.Tags)
                {
                    result = await _channel.ExecuteAsync(@"
                                            SADD tags @tag
                                            SADD @tagKey @tag",
                                            new { tag, tagKey }).ConfigureAwait(false);
                }
            }
            return new QuestionEditCommandResult(command.Id, slug);
        }
    }
}

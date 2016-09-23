using SimpleQA.Commands;
using System;
using System.Collections.Generic;
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
            var result = await _channel.ExecuteAsync(
                                        "AnswerEdit {question} @answerId @userId @data",
                                         new
                                         {
                                             answerId = command.AnswerId,
                                             userId = user.GetSimpleQAIdentity().Id,
                                             data = GetData(command)
                                         })
                                         .ConfigureAwait(false);
            CheckException(result);
            var slug = result[0].GetString();
            return new AnswerEditCommandResult(command.QuestionId, slug, command.AnswerId);
        }

        static IEnumerable<String> GetData(AnswerEditCommand command)
        {
            return Parameter.SequenceProperties(new
            {
                ModifiedOn = DateTime.Now,
                Content = command.Content,
                HtmlContent = command.HtmlContent
            });
        }

        static void CheckException(IRedisResults result)
        {
            var error = result[0].GetException();
            if (error != null)
            {
                switch (error.Prefix)
                {
                    case "NOTOWNER":
                        throw new SimpleQAException("You are not the author of the answer you try to edit.");
                    default:
                        throw error;
                }
            }
        }
    }
}

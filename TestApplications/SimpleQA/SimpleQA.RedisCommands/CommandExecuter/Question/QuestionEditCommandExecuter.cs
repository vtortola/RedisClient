using SimpleQA.Commands;
using System;
using System.Collections.Generic;
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

        public async Task<QuestionEditCommandResult> ExecuteAsync(QuestionEditCommand command, SimpleQAIdentity user, CancellationToken cancel)
        {
            var data = GetPatchData(command);

            var result = await _channel.ExecuteAsync(@"
                                        UpdateQuestion {question} @id @userId @data @tags @topic",
                                        new 
                                        { 
                                            id = command.Id,
                                            userId = user.Id,
                                            data, 
                                            tags = command.Tags,
                                            topic = "question-" + command.Id
                                        })
                                        .ConfigureAwait(false);

            CheckException(result);

            result = result[0].AsResults();
            var slug = result[0].GetString();
            var addedTags = result[1].GetStringArray();
            var removedTags = result[2].GetStringArray();



            return new QuestionEditCommandResult(command.Id, slug);
        }

        static IEnumerable<String> GetPatchData(QuestionEditCommand command)
        {
            return Parameter.SequenceProperties(new
            {
                Title = command.Title,
                Content = command.Content,
                HtmlContent = command.HtmlContent,
                ModifiedOn = DateTime.Now,
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
                        throw new SimpleQANotOwnerException("You cannot close a question that is yours.");

                    case "CANNOTCLOSE":
                        throw new SimpleQANotOwnerException("Tne question is not open anymore.");

                    default: throw error;
                }
            }
        }
    }
}

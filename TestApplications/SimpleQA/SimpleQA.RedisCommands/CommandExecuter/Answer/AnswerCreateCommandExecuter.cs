using SimpleQA.Commands;
using System;
using System.Collections.Generic;
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

        public async Task<AnswerCreateCommandResult> ExecuteAsync(AnswerCreateCommand command, SimpleQAIdentity user, CancellationToken cancel)
        {
            var result = await _channel.ExecuteAsync(
                                        "CreateAnswer {question} @qid @data",
                                        new 
                                        { 
                                            qid= command.QuestionId,
                                            data = GetAnswerData(command, command.QuestionId, user)
                                        }).ConfigureAwait(false);

            result = result[0].AsResults();
            var aid = result[0].GetString();
            var quesionData = result[1].GetStringArray();
            var slug = quesionData[0];
            var ownerId = quesionData[1];
            if (ownerId != user.Id)
            {
                _channel.Dispatch("NotifyQuestionUpdate {user} @qid @uid",
                                  new
                                  {
                                      qid = command.QuestionId,
                                      uid = user.Id
                                  });
            }
            return new AnswerCreateCommandResult(command.QuestionId, slug, aid);
        }

        static IEnumerable<String> GetAnswerData(AnswerCreateCommand command, String questionId, SimpleQAIdentity user)
        {
            var data = Parameter.SequenceProperties(new
            {
                QuestionId = questionId,
                UserId = user.Id,
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

using SimpleQA.Models;
using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class AnswerRequestBuilder : IModelBuilder<AnswerRequest, AnswerViewModel>
    {
        readonly IRedisChannel _channel;

        public AnswerRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<AnswerViewModel> BuildAsync(AnswerRequest request, IPrincipal user, CancellationToken cancel)
        {
            var result = await _channel.ExecuteAsync(
                                        "GetAnswer {question} @answerId @userId",
                                         new
                                         {
                                             answerId = request.AnswerId,
                                             userId = user.GetSimpleQAIdentity().Id
                                         })
                                         .ConfigureAwait(false);

            result = result[0].AsResults();
            var questionStatus = result[3].AsEnum<QuestionStatus>();

            var answer = new AnswerViewModel();
            result[0].AsObjectCollation(answer);
            answer.User = result[1].GetString();
            answer.QuestionId = request.QuestionId;
            answer.Editable = questionStatus == QuestionStatus.Open;
            answer.Votable = questionStatus == QuestionStatus.Open;
            answer.AuthoredByUser = answer.User == user.Identity.Name;
            answer.UpVoted = GetVote(result[2].GetString());
            return answer;
        }

        static Boolean? GetVote(String value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return null;
            if (value == "0")
                return null;
            if (value == "1")
                return true;
            if (value == "-1")
                return false;
            throw new InvalidOperationException("Unexpected vote value: " + value);
        }
    }
}

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
            var result = await _channel.ExecuteAsync(@"
                                        HGETALL @answerId
                                        ZSCORE @votesKey @user
                                        HGET @questionId Status",
                                         new
                                         {
                                             questionId = Keys.QuestionKey(request.QuestionId),
                                             answerId = Keys.AnswerKey(request.QuestionId, request.AnswerId),
                                             votesKey = Keys.AnswerVoteKey(request.QuestionId, request.AnswerId),
                                             user = user.Identity.IsAuthenticated ? user.Identity.Name : "__anon__"

                                         }).ConfigureAwait(false);

            var questionStatus = result[2].GetString();

            var answer = new AnswerViewModel();
            result[0].AsObjectCollation(answer);
            answer.QuestionId = request.QuestionId;
            answer.Editable = questionStatus == "Open";
            answer.Votable = questionStatus == "Open";
            answer.AuthoredByUser = answer.User == user.Identity.Name;
            answer.UpVoted = result[1].GetString() == null ? (Boolean?)null : (Int32.Parse(result[1].GetString()) > 0 ? true : false);
            return answer;
        }
    }
}

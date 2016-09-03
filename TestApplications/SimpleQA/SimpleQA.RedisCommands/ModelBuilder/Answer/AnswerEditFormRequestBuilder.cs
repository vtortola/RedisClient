using SimpleQA.Models;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class AnswerEditFormRequestBuilder : IModelBuilder<AnswerEditFormRequest, AnswerEditFormViewModel>
    {
        readonly IRedisChannel _channel;

        public AnswerEditFormRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<AnswerEditFormViewModel> BuildAsync(AnswerEditFormRequest request, IPrincipal user, CancellationToken cancel)
        {
            var answerKey = Keys.AnswerKey(request.QuestionId, request.AnswerId);

            var result = await _channel.ExecuteAsync(@"
                                        HMGET @answerKey User Content",
                                        new { answerKey })
                                        .ConfigureAwait(false);

            var array = result[0].GetStringArray();
            if (array[0] != user.Identity.Name)
                throw new SimpleQAException("You cannot edit a question that you did not create");

            return new AnswerEditFormViewModel()
            {
                AnswerId = request.AnswerId,
                QuestionId = request.QuestionId,
                Content = array[1]
            };
        }
    }
}

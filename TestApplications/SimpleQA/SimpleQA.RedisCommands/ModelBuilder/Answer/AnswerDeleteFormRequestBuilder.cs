using SimpleQA.Models;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class AnswerDeleteFormRequestBuilder : IModelBuilder<AnswerDeleteFormRequest, AnswerDeleteFormViewModel>
    {
        readonly IRedisChannel _channel;

        public AnswerDeleteFormRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public Task<AnswerDeleteFormViewModel> BuildAsync(AnswerDeleteFormRequest request, SimpleQAIdentity user, CancellationToken cancel)
        {
            return Task.FromResult(new AnswerDeleteFormViewModel() { QuestionId = request.QuestionId, AnswerId = request.AnswerId });
        }
    }
}

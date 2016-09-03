using SimpleQA.Models;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class QuestionDeleteFormRequestBuilder : IModelBuilder<QuestionDeleteFormRequest, QuestionDeleteFormViewModel>
    {
        readonly IRedisChannel _channel;
        public QuestionDeleteFormRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }
        public Task<QuestionDeleteFormViewModel> BuildAsync(QuestionDeleteFormRequest request, IPrincipal user, CancellationToken cancel)
        {
            return Task.FromResult(new QuestionDeleteFormViewModel() { Id = request.Id });
        }
    }
}

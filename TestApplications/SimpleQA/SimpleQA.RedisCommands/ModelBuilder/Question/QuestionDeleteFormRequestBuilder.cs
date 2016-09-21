using SimpleQA.Models;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class QuestionDeleteFormRequestBuilder : IModelBuilder<QuestionDeleteFormRequest, QuestionDeleteFormViewModel>
    {
        public Task<QuestionDeleteFormViewModel> BuildAsync(QuestionDeleteFormRequest request, IPrincipal user, CancellationToken cancel)
        {
            return Task.FromResult(new QuestionDeleteFormViewModel() { Id = request.Id });
        }
    }
}

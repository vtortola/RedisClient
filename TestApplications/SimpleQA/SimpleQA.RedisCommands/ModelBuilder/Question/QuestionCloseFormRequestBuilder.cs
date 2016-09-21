using SimpleQA.Models;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class QuestionCloseFormRequestBuilder : IModelBuilder<QuestionCloseFormRequest, QuestionCloseFormViewModel>
    {
        readonly IRedisChannel _channel;
        public QuestionCloseFormRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }
        public async Task<QuestionCloseFormViewModel> BuildAsync(QuestionCloseFormRequest request, IPrincipal user, CancellationToken cancel)
        {
            var result = await _channel.ExecuteAsync(
                                        "QuestionCloseForm {question} @id",
                                        new
                                        {
                                            id = request.Id
                                        })
                                        .ConfigureAwait(false);

            result.ThrowErrorIfAny();
            return new QuestionCloseFormViewModel() { Id = request.Id, Votes = result[0].AsInteger() };
        }
    }
}

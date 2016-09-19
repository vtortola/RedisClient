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
            var result = await _channel.ExecuteAsync(@"
                                        HGET @key CloseVotes
                                        SISMEMBER @closeVotes @user",
                                        new
                                        {
                                            key = Keys.QuestionKey(request.Id),
                                            closeVotes = Keys.QuestionCloseVotesCollectionKey(request.Id),
                                            user = user.Identity.IsAuthenticated ? user.Identity.Name : "__anon__"
                                        }).ConfigureAwait(false);

            if (result[1].GetInteger() == 1)
                throw new SimpleQAException("You already voted to close this item.");

            var votes = result[0].GetString() == null ? 0 : result[0].AsInteger();

            return new QuestionCloseFormViewModel() { Id = request.Id, Votes = votes };
        }
    }
}

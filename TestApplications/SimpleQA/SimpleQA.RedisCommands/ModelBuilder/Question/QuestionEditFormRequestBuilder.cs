using SimpleQA.Models;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class QuestionEditFormRequestBuilder : IModelBuilder<QuestionEditFormRequest, QuestionEditFormViewModel>
    {
        readonly IRedisChannel _channel;
        public QuestionEditFormRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<QuestionEditFormViewModel> BuildAsync(QuestionEditFormRequest request, IPrincipal user, CancellationToken cancel)
        {
            var result = await _channel.ExecuteAsync(@"
                                        HMGET @key Content Title User
                                        SMEMBERS @tagKey", new
                                                         {
                                                             key = Keys.QuestionKey(request.Id),
                                                             tagKey = Keys.QuestionTagsKey(request.Id),
                                                         }).ConfigureAwait(false);

            var dic = result[0].GetStringArray();

            if (dic[2] != user.Identity.Name)
                throw new SimpleQAException("You are not the author of the question you try to edit.");

            var model = new QuestionEditFormViewModel();
            model.Content = dic[0];
            model.Title = dic[1];
            model.Tags = result[1].GetStringArray();
            model.Id = request.Id;
            return model;
        }
    }
}

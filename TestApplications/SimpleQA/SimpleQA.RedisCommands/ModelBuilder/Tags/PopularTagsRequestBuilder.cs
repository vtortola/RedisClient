using SimpleQA.Models;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class PopularTagsRequestBuilder : IModelBuilder<PopularTagsRequest, PopularTagsViewModel>
    {
        readonly IRedisChannel _channel;
        public PopularTagsRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<PopularTagsViewModel> BuildAsync(PopularTagsRequest request, IPrincipal user, CancellationToken cancel)
        {
            var result = await _channel.ExecuteAsync("GetPopularTags {tag} @count",
                                        new { count = 20  })
                                        .ConfigureAwait(false);
            result.ThrowErrorIfAny();
            return new PopularTagsViewModel()
            {
                Tags = result[0].GetStringArray()
            };
        }
    }
}

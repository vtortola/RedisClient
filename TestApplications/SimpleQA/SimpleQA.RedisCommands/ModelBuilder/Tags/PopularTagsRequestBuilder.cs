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
            var result = await _channel.ExecuteAsync(@"ZREVRANGE @tagquestions @start @end",
                                        new
                                        {
                                            tagquestions = Keys.TagCounting(),
                                            start = 0,
                                            end = 20
                                        })
                                        .ConfigureAwait(false);

            return new PopularTagsViewModel()
            {
                Tags = result[0].GetStringArray()
            };
        }
    }
}

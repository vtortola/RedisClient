using SimpleQA.Models;
using System;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    // http://oldblog.antirez.com/post/autocomplete-with-redis.html
    public sealed class TagSuggestionRequestBuilder : IModelBuilder<TagSuggestionRequest, TagSuggestionsModel>
    {
        readonly IRedisChannel _channel;
        public TagSuggestionRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<TagSuggestionsModel> BuildAsync(TagSuggestionRequest request, IPrincipal user, CancellationToken cancel)
        {
            var store = Keys.AutoCompleteTags();
            var result = await _channel.ExecuteAsync(
                                        "SuggestTags {tag} @prefix @max", 
                                        new 
                                        { 
                                            prefix = request.Query,
                                            max = 50
                                        }).ConfigureAwait(false);

            result.ThrowErrorIfAny();
            return new TagSuggestionsModel(result[0].GetStringArray());
        }
    }
}

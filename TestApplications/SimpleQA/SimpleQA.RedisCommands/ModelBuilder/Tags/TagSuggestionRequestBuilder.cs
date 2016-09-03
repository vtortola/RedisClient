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
            var result = await _channel.ExecuteAsync("ZRANK @store @prefix", new { store, prefix = request.Query }).ConfigureAwait(false);

            var start = result[0].AsInteger();

            result = await _channel.ExecuteAsync("ZRANGE @store @start @end", new { store, start, end = start + 50 }).ConfigureAwait(false);

            var tags = result[0]
                         .GetStringArray()
                         .Where(t => t.EndsWith("*", StringComparison.Ordinal) && t.StartsWith(request.Query, StringComparison.OrdinalIgnoreCase))
                         .Select(t=>t.Substring(0, t.Length -1))
                         .ToArray();

            return new TagSuggestionsModel(tags);
        }
    }
}

using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class HomeByTagRequestBuilder : IModelBuilder<HomeByTagRequest, HomeByTagViewModel>
    {
        readonly IRedisChannel _channel;

        public HomeByTagRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<HomeByTagViewModel> BuildAsync(HomeByTagRequest request, IPrincipal user, CancellationToken cancel)
        {
            var model = new HomeByTagViewModel();

            var ordering = request.Sorting.HasValue ? request.Sorting.Value : default(QuestionSorting);
            var start = (request.Page - 1) * Constant.ItemsPerPage;
            var end = start + Constant.ItemsPerPage - 1;
            var tagquestions = ordering == QuestionSorting.ByDate ? Keys.TagKeyByDate(request.Tag) : Keys.TagKeyByScore(request.Tag);

            var result = await _channel.ExecuteAsync(@"
                                        ZREVRANGE @tagquestions @start @end
                                        ZSCORE @tagCounter @tag",
                                        new 
                                        { 
                                            tagquestions,
                                            start, 
                                            end, 
                                            tagCounter = Keys.TagCounting(),
                                            tag = request.Tag
                                        })
                                        .ConfigureAwait(false);

            model.Page = request.Page;
            model.TotalPages = (Int32)Math.Ceiling(result[1].AsInteger() / (Constant.ItemsPerPage * 1.0));

            var list = new List<QuestionExcerptViewModel>(end - start);
            foreach (var questionKey in result[0].GetStringArray())
            {
                result = await _channel.ExecuteAsync(@"
                                        HGETALL @questionKey
                                        SMEMBERS @qtagKey
                                        ",
                                         new
                                         {
                                             questionKey,
                                             qtagKey = questionKey + ":tags"

                                         }).ConfigureAwait(false);
                var question = result[0].AsObjectCollation<QuestionExcerptViewModel>();
                question.Tags = result[1].GetStringArray();
                list.Add(question);
            }

            model.Questions = list;
            model.Sorting = ordering;
            model.Tag = request.Tag;
            return model;
        }
    }
}

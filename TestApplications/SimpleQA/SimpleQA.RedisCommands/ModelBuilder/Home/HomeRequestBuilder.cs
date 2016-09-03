using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class HomeRequestBuilder : IModelBuilder<HomeRequest, HomeViewModel>
    {
        readonly IRedisChannel _channel;

        public HomeRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<HomeViewModel> BuildAsync(HomeRequest request, IPrincipal user, CancellationToken cancel)
        {
            var model = new HomeViewModel();

            var ordering = request.Sorting.HasValue ? request.Sorting.Value : default(QuestionSorting);
            var start = (request.Page - 1) * Constant.ItemsPerPage;
            var end = start + Constant.ItemsPerPage - 1;
            var questions = ordering == QuestionSorting.ByDate ? Keys.QuestionsByDate() : Keys.QuestionsByScore();

            var result = await _channel.ExecuteAsync(@"
                                        ZREVRANGE @questions @start @end
                                        GET @questionCounter",
                                        new { questions, start, end, questionCounter = Keys.QuestionCounter() })
                                        .ConfigureAwait(false);

            model.Page = request.Page;
            model.TotalPages = (Int32)Math.Ceiling(result[1].AsInt64() / (Constant.ItemsPerPage * 1.0));

            var list = new List<QuestionHeaderViewModel>(end - start);
            foreach (var questionKey in result[0].GetStringArray())
            {
                result = await _channel.ExecuteAsync(@"
                                        HGETALL @questionKey
                                        SMEMBERS @qtagKey
                                        ", 
                                         new 
                                         { 
                                             questionKey,
                                             qtagKey = questionKey  + ":tags"

                                         }).ConfigureAwait(false);
                var question = result[0].AsObjectCollation<QuestionHeaderViewModel>();
                question.Tags = result[1].GetStringArray();
                list.Add(question);
            }

            model.Questions = list;
            model.Sorting = ordering;
            return model;
        }
    }
}

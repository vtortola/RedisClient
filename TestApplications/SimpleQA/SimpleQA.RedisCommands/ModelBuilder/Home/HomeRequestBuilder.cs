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

        public async Task<HomeViewModel> BuildAsync(HomeRequest request, SimpleQAIdentity user, CancellationToken cancel)
        {
            var model = new HomeViewModel();

            var sorting = request.Sorting.HasValue ? request.Sorting.Value : QuestionSorting.ByScore;

            var result = 
                await _channel.ExecuteAsync(
                               "PaginateHome {questions} @page @items @orderBy",
                               new 
                               { 
                                   page = request.Page - 1,
                                   items = Constant.ItemsPerPage,
                                   orderBy = sorting.ToString() 
                               })
                               .ConfigureAwait(false);

            result.ThrowErrorIfAny();
            result = result[0].AsResults();

            model.Page = request.Page;
            model.TotalPages = (Int32)Math.Ceiling(result[0].AsInteger() / (Constant.ItemsPerPage * 1.0));
            model.Sorting = sorting;

            var ids = result.Skip(1).Select(r => r.GetString()).ToArray();
            if (ids.Any())
                model.Questions = await GetQuestions(ids).ConfigureAwait(false);
            else
                model.Questions = new List<QuestionHeaderViewModel>();
            
            return model;
        }

        private async Task<List<QuestionHeaderViewModel>> GetQuestions(IEnumerable<String> ids)
        {
            var result =
                await _channel.ExecuteAsync(
                               "GetQuestions {question} @ids",
                               new { ids })
                               .ConfigureAwait(false);

            result.ThrowErrorIfAny();

            return result[0]
                        .AsResults()
                        .Select(r => r.AsResults())
                        .Select(q =>
                        {
                            var question = q[0].AsObjectCollation<QuestionHeaderViewModel>();
                            question.User = q[1].GetString();
                            question.Tags = q[2].GetStringArray();
                            return question;
                        })
                        .ToList();
        }
    }
}

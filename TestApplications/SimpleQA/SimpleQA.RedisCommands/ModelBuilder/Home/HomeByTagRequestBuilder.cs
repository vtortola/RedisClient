using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;
using System.Linq;

namespace SimpleQA.RedisCommands
{
    public sealed class HomeByTagRequestBuilder : IModelBuilder<HomeByTagRequest, HomeByTagViewModel>
    {
        readonly IRedisChannel _channel;

        public HomeByTagRequestBuilder(IRedisChannel channel)
        {
            _channel = channel;
        }

        public async Task<HomeByTagViewModel> BuildAsync(HomeByTagRequest request, SimpleQAIdentity user, CancellationToken cancel)
        {
            var model = new HomeByTagViewModel();

            var sorting = request.Sorting.HasValue ? request.Sorting.Value : QuestionSorting.ByScore;

            var result =
                await _channel.ExecuteAsync(
                               "PaginateTag {tag} @tag @page @items @orderBy",
                               new
                               {
                                   tag = request.Tag,
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
            model.Tag = request.Tag;

            var ids = result.Skip(1).Select(r => r.GetString()).ToArray();
            if (ids.Any())
                model.Questions = await GetQuestions(ids).ConfigureAwait(false);
            else
                model.Questions = new List<QuestionExcerptViewModel>();

            return model;
        }

        private async Task<List<QuestionExcerptViewModel>> GetQuestions(IEnumerable<String> ids)
        {
            var result =
                await _channel.ExecuteAsync( // Reuse proc from HomeProcedures.rcproc
                               "GetQuestions {question} @ids",
                               new { ids })
                               .ConfigureAwait(false);

            result.ThrowErrorIfAny();

            return result[0]
                        .AsResults()
                        .Select(r => r.AsResults())
                        .Select(q =>
                        {
                            var question = q[0].AsObjectCollation<QuestionExcerptViewModel>();
                            question.User = q[1].GetString();
                            question.Tags = q[2].GetStringArray();
                            return question;
                        })
                        .ToList();
        }
    }
}

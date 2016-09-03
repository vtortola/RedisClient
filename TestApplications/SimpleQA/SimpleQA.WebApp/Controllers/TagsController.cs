using SimpleQA.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Controllers
{
    public class TagsController : Controller
    {
        readonly IModelBuilderMediator _mediator;

        public TagsController(IModelBuilderMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<JsonResult> Suggest(TagSuggestionRequest input, CancellationToken cancel)
        {
            var suggestions = await _mediator.BuildAsync<TagSuggestionRequest, TagSuggestionsModel>(input, User, cancel);

            return Json(suggestions.Suggestions);
        }

        [ChildActionOnly]
        public PartialViewResult Popular(CancellationToken cancel)
        {
            // ChildActionOnly does not support asynchronous operations...
            var model = _mediator.BuildAsync<PopularTagsRequest, PopularTagsViewModel>(new PopularTagsRequest(), User, cancel).Result;
            return PartialView(model);
        }
    }
}
using SimpleQA.Models;
using SimpleQA.WebApp.Filter;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Controllers
{
    [Authorize]
    public class AnswerReadController : Controller
    {
        readonly IModelBuilderMediator _mediator;

        public AnswerReadController(IModelBuilderMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<PartialViewResult> Get(AnswerRequest request, CancellationToken cancel)
        {
            return PartialView(await _mediator.BuildAsync<AnswerRequest, AnswerViewModel>(request, User.GetAppIdentity(), cancel));
        }

        public async Task<PartialViewResult> Edit(AnswerEditFormRequest request, CancellationToken cancel)
        {
            return PartialView(await _mediator.BuildAsync<AnswerEditFormRequest, AnswerEditFormViewModel>(request, User.GetAppIdentity(), cancel));
        }

        public async Task<PartialViewResult> Delete(AnswerDeleteFormRequest request, CancellationToken cancel)
        {
            return PartialView(await _mediator.BuildAsync<AnswerDeleteFormRequest, AnswerDeleteFormViewModel>(request, User.GetAppIdentity(), cancel));
        }
    }
}
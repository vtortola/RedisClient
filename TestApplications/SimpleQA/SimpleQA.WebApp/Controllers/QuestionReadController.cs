using SimpleQA.Models;
using SimpleQA.WebApp.Filter;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Controllers
{
    public class QuestionReadController : Controller
    {
        readonly IModelBuilderMediator _mediator;

        public QuestionReadController(IModelBuilderMediator builder)
        {
            _mediator = builder;
        }

        [AnonymousOutputCache(CacheProfile = "QuestionCaching")]
        public async Task<ActionResult> Get(QuestionRequest request, CancellationToken cancel)
        {
            var model = await _mediator.BuildAsync<QuestionRequest, QuestionViewModel>(request, User.GetAppIdentity(), cancel);
            return View(model);
        }

        [Authorize]
        public ActionResult Ask(QuestionAskFormRequest request, CancellationToken cancel)
        {
            var tags = String.IsNullOrWhiteSpace(request.Tag) ? new String[0] : new[] { request.Tag };

            return View(new QuestionAddFormViewModel() { Tags = tags });
        }

        [Authorize]
        public async Task<ActionResult> Edit(QuestionEditFormRequest request, CancellationToken cancel)
        {
            var model = await _mediator.BuildAsync<QuestionEditFormRequest, QuestionEditFormViewModel>(request, User.GetAppIdentity(), cancel);
            return View(model);
        }

        [Authorize]
        public async Task<PartialViewResult> Close(QuestionCloseFormRequest request, CancellationToken cancel)
        {
            var model = await _mediator.BuildAsync<QuestionCloseFormRequest, QuestionCloseFormViewModel>(request, User.GetAppIdentity(), cancel);
            return PartialView(model);
        }

        [Authorize]
        public async Task<PartialViewResult> Delete(QuestionDeleteFormRequest request, CancellationToken cancel)
        {
            var model = await _mediator.BuildAsync<QuestionDeleteFormRequest, QuestionDeleteFormViewModel>(request, User.GetAppIdentity(), cancel);
            return PartialView(model);
        }
    }
}
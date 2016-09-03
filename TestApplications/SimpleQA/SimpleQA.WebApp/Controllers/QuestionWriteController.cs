using SimpleQA.Commands;
using SimpleQA.WebApp.Filter;
using SimpleQA.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using SimpleQA.Markdown;
using System.Threading;

namespace SimpleQA.WebApp.Controllers
{
    [ValidateSession]
    [SimpleQAAuthorize]
    [EnforceValidModel]
    public class QuestionWriteController : Controller
    {
        readonly ICommandExecuterMediator _mediator;
        readonly IMarkdown _markdown;

        public QuestionWriteController(ICommandExecuterMediator mediator, IMarkdown markdown)
        {
            _mediator = mediator;
            _markdown = markdown;
        }

        [HttpPost]
        public async Task<ActionResult> Ask(QuestionCreateRequest request, CancellationToken cancel)
        {
            var command = new QuestionCreateCommand(request.Title, request.Content, _markdown.TransformIntoHTML(request.Content), request.Tags);
            var result = await _mediator.ExecuteAsync<QuestionCreateCommand, QuestionCreateCommandResult>(command, User, cancel);
            return RedirectToRoute("QuestionRead", new { id = result.Id, slug = result.Slug, action = "get" });
        }

        [HttpPost]
        public async Task<ActionResult> Edit(QuestionEditRequest request, CancellationToken cancel)
        {
            var command = new QuestionEditCommand(request.Id, request.Title, request.Content, _markdown.TransformIntoHTML(request.Content), request.Tags);
            var result = await _mediator.ExecuteAsync<QuestionEditCommand, QuestionEditCommandResult>(command, User, cancel);
            return RedirectToRoute("QuestionRead", new { id = result.Id, slug = result.Slug, action = "get" });
        }
        
        [HttpPost]
        public async Task<ActionResult> Vote(QuestionVoteRequest request, CancellationToken cancel)
        {
            var command = new QuestionVoteCommand(request.Id, request.Upvote);
            var result = await _mediator.ExecuteAsync<QuestionVoteCommand, QuestionVoteCommandResult>(command, User, cancel);
            return Json(new { Votes = result.Votes });
        }

        [HttpPost]
        public async Task<ActionResult> Close(QuestionActionRequest request, CancellationToken cancel)
        {
            var command = new QuestionCloseCommand(request.Id);
            var result = await _mediator.ExecuteAsync<QuestionCloseCommand, QuestionCloseCommandResult>(command, User, cancel);
            return RedirectToRoute("QuestionRead", new { id = result.Id, slug = result.Slug, action = "get" });
        }

        [HttpPost]
        public async Task<ActionResult> Delete(QuestionActionRequest request, CancellationToken cancel)
        {
            var command = new QuestionDeleteCommand(request.Id);
            var result = await _mediator.ExecuteAsync<QuestionDeleteCommand, QuestionDeleteCommandResult>(command, User, cancel);
            return RedirectToRoute("QuestionRead", new { id = result.Id, slug = result.Slug, action = "get" });
        }
    }
}
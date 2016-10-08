using SimpleQA.Commands;
using SimpleQA.Markdown;
using SimpleQA.Models;
using SimpleQA.WebApp.Filter;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Controllers
{
    [AuthorizeWrite]
    [EnforceValidModel]
    public class AnswerWriteController : Controller
    {
        readonly ICommandExecuterMediator _mediator;
        readonly IMarkdown _markdown;

        public AnswerWriteController(ICommandExecuterMediator mediator, IMarkdown markdown)
        {
            _mediator = mediator;
            _markdown = markdown;
        }

        public async Task<ActionResult> Vote(AnswerVoteRequest request, CancellationToken cancel)
        {
            var command = new AnswerVoteCommand(request.QuestionId, request.AnswerId, request.Upvote);
            var result = await _mediator.ExecuteAsync<AnswerVoteCommand, AnswerVoteCommandResult>(command, User, cancel);
            return Json(new { Votes = result.Votes  });
        }
                
        public async Task<ActionResult> Add(AnswerCreateRequest request, CancellationToken cancel)
        {
            var command = new AnswerCreateCommand(request.QuestionId, DateTime.Now, request.Content, _markdown.TransformIntoHTML(request.Content));
            var result = await _mediator.ExecuteAsync<AnswerCreateCommand, AnswerCreateCommandResult>(command, User, cancel);
            return Redirect(Url.RouteUrl("QuestionRead", new { id = result.QuestionId, slug = result.Slug, action = "get" }) + "#ans_" + result.AnswerId);
        }

        public async Task<ActionResult> Edit(AnswerEditRequest request, CancellationToken cancel)
        {
            var command = new AnswerEditCommand(request.QuestionId, request.AnswerId, request.Content, _markdown.TransformIntoHTML(request.Content));
            var result = await _mediator.ExecuteAsync<AnswerEditCommand, AnswerEditCommandResult>(command, User, cancel);
            return Redirect(Url.RouteUrl("AnswerRead", new { questionId = result.QuestionId, answerId=result.AnswerId, action = "get" }));
        }

        public async Task<ActionResult> Delete(AnswerActionRequest request, CancellationToken cancel)
        {
            var command = new AnswerDeleteCommand(request.QuestionId, request.AnswerId);
            var result = await _mediator.ExecuteAsync<AnswerDeleteCommand, AnswerDeleteCommandResult>(command, User, cancel);
            return Redirect(Url.RouteUrl("QuestionRead", new { id = result.QuestionId, slug = result.Slug, action = "get" }));
        }
    }
}
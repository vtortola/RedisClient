using SimpleQA.Commands;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Controllers
{
    public class QuestionVisitCounterController : Controller
    {
        readonly ICommandExecuterMediator _mediator;

        public QuestionVisitCounterController(ICommandExecuterMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Visit(String questionId, CancellationToken cancel)
        {
            await _mediator.ExecuteAsync<VisitQuestionCommand, VisitQuestionCommandResult>(new VisitQuestionCommand(questionId), User.GetAppIdentity(), cancel).ConfigureAwait(false);
            return new HttpStatusCodeResult(HttpStatusCode.NoContent);
        }
    }
}
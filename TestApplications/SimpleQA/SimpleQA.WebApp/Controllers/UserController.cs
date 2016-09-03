using SimpleQA.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Controllers
{
    public class UserController : Controller
    {
        readonly IModelBuilderMediator _mediator;

        public UserController(IModelBuilderMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult> Index(UserModelRequest request, CancellationToken cancel)
        {
            var model = await _mediator.BuildAsync<UserModelRequest, UserModel>(request, User, cancel);
            return View(model);
        }

        [HttpPost]
        public async Task<PartialViewResult> Inbox(CancellationToken cancel)
        {
            var model = await _mediator.BuildAsync<UserInboxRequest, UserInboxModel>(UserInboxRequest.Empty, User, cancel);
            return PartialView(model);
        }
	}
}
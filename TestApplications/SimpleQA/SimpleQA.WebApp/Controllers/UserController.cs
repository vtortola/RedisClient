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

        public async Task<ActionResult> Index(UserModelRequest request, CancellationToken cancel)
        {
            var model = await _mediator.BuildAsync<UserModelRequest, UserModel>(request, User.GetAppIdentity(), cancel);
            return View(model);
        }

        public async Task<PartialViewResult> Inbox(CancellationToken cancel)
        {
            var model = await _mediator.BuildAsync<UserInboxRequest, UserInboxModel>(UserInboxRequest.Empty, User.GetAppIdentity(), cancel);
            return PartialView(model);
        }
	}
}
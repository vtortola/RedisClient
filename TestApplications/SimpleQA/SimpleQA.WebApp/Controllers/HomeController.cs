using SimpleQA.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Controllers
{
    public class HomeController : Controller
    {
        readonly IModelBuilderMediator _mediator;

        public HomeController(IModelBuilderMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Get(HomeRequest request, CancellationToken cancel)
        {
            return View(await _mediator.BuildAsync<HomeRequest, HomeViewModel>(request, User, cancel));
        }

        public async Task<ActionResult> ByTag(HomeByTagRequest request, CancellationToken cancel)
        {
            return View(await _mediator.BuildAsync<HomeByTagRequest, HomeByTagViewModel>(request, User, cancel));
        }
	}
}
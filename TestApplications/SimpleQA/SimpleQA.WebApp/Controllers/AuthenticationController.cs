using SimpleQA.Commands;
using SimpleQA.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Controllers
{
    public class AuthenticationController : Controller
    {
        readonly ICommandExecuterMediator _mediator;

        public AuthenticationController(ICommandExecuterMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginModel model, CancellationToken cancel)
        {
            if (ModelState.IsValid)
            {
                var command = new AuthenticateCommand(model.Username, model.Password);
                var result = await _mediator.ExecuteAsync<AuthenticateCommand, AuthenticateCommandResult>(command, User, cancel);
                Response.Cookies.Add(new HttpCookie(Constant.CookieKey, result.SessionId));
                if (!String.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
            }

            return RedirectToRoute("Default");
        }

        [HttpPost]
        public async Task<ActionResult> Logout(String returnUrl, CancellationToken cancel)
        {
            if (User.Identity.IsAuthenticated)
            {
                var cookie = Request.Cookies[Constant.CookieKey];
                if (cookie != null && !String.IsNullOrWhiteSpace(cookie.Value))
                {
                    var command = new EndSessionCommand(cookie.Value);
                    await _mediator.ExecuteAsync<EndSessionCommand, EndSessionCommandResult>(command, User, cancel);
                }

                Response.Cookies.Add(new HttpCookie(Constant.CookieKey, String.Empty));
            }

            if (!String.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToRoute("Default");
            }
        }
    }
}
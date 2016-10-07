using SimpleQA.Commands;
using SimpleQA.Models;
using SimpleQA.WebApp.Filter;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

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

                var ticket = new FormsAuthenticationTicket(1, model.Username, DateTime.Now, DateTime.Now.Add(FormsAuthentication.Timeout), true, result.SessionId);
                var cookie = FormsAuthentication.Encrypt(ticket);
                Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, cookie));

                if (!String.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
            }

            return RedirectToRoute("Default");
        }

        [Authorize]
        public async Task<ActionResult> Logout(String returnUrl, CancellationToken cancel)
        {
            var cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie != null && !String.IsNullOrWhiteSpace(cookie.Value))
            {
                var sessionId = FormsAuthentication.Decrypt(cookie.Value).UserData;
                var command = new EndSessionCommand(cookie.Value);

                await _mediator.ExecuteAsync<EndSessionCommand, EndSessionCommandResult>(command, User, cancel);

                Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, String.Empty));
                if(Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
            }

            return RedirectToRoute("Default");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQA.WebApp.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult SessionEnded()
        {
            return View();
        }
    }
}
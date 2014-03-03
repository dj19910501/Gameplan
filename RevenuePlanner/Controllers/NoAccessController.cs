using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HVCTA.Controllers
{
    public class NoAccessController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult JavaScriptNotEnabled()
        {
            return View();
        }
    }
}

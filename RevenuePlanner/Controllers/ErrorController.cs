using System.Web.Mvc;
/*
 *  Author: Manoj Limbachiya
 *  Created Date: 10/22/2013
 *  Screen: Error
 *  Purpose: Redirect to error page in case of runtime error and dispaly error details
  */
namespace RevenuePlanner.Controllers
{
    public class ErrorController : Controller
    {
        /// <summary>
        /// Error page
        /// </summary>
        /// <returns></returns>
        public ActionResult Error()
        {
            return View();
        }

    }
}

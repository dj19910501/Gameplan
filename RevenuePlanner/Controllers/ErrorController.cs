using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
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
            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// Error page
        /// </summary>
        /// <returns></returns>
        public ActionResult ElmahError()
        {
            return View("~/Views/Shared/ElmahError.cshtml");
        }

        /// <summary>
        /// Added by: Maninder Singh Wadhva on 11/18/2014 to address ticket #942 Exception handeling in Gameplan.
        /// Action for Page not found
        /// </summary>
        /// <returns>Returns page not found view.</returns>
        public ActionResult PageNotFound()
        {
            try
            {
                string applicationReleaseVersion = RevenuePlanner.Helpers.Common.GetCurrentApplicationReleaseVersion();
                ViewBag.ApplicationReleaseVersion = applicationReleaseVersion;
            }
            catch (System.Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }

            return View("PageNotFound");
        }
        //Added by Rahul Shah on 25/09/2015 for PL#900
        /// <summary>
        /// Function to handle exeption to ELMAH.
        /// Added By: Maninder Singh Wadhva on 12/10/2014 to address ticket #900 Exception handling in client side scripting
        /// </summary>
        /// <param name="message">Error message.</param>
        public void LogJavaScriptError(string message)
        {
            if (message != null)
            {
                Elmah.ErrorSignal
                    .FromCurrentContext()
                    .Raise(new RevenuePlanner.Helpers.JavaScriptException(message));
            }
        }
    }
}

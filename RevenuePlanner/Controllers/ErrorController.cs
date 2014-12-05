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

    }
}

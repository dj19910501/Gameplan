using RevenuePlanner.Controllers;
using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Net;
using System.Web.SessionState;

namespace RevenuePlanner
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Registring Route, Filters and Bundles
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
        }
        /// <summary>
        /// When application throw any error, it will set the Context variable to display the error details
        /// Modified by: Maninder Singh Wadhva on 11/18/2014 to address ticket #942 Exception handeling in Gameplan.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs object.</param>
        protected void Application_Error(object sender, EventArgs e)
        {
            //// By default Internal server error.
            HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError;

            //// Evaluating current error status code.
            var ex = Server.GetLastError();
            if (ex != null && ex is HttpException)
            {
                var exception = ex as HttpException;
                httpStatusCode = (HttpStatusCode)exception.GetHttpCode();
            }

            //// Skip if current http status code is NotFound.
            if (!httpStatusCode.Equals(HttpStatusCode.NotFound))
            {
            var httpContext = ((MvcApplication)sender).Context;
            var currentController = " ";
            var currentAction = " ";
            var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

            if (currentRouteData != null)
            {
                if (currentRouteData.Values["controller"] != null && !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
                {
                    currentController = currentRouteData.Values["controller"].ToString();
                }

                if (currentRouteData.Values["action"] != null && !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
                {
                    currentAction = currentRouteData.Values["action"].ToString();
                }
            }

            var controller = new ErrorController();
            var routeData = new RouteData();
            var action = "Error";

            httpContext.ClearError();
            httpContext.Response.Clear();
                httpContext.Response.StatusCode = (int)httpStatusCode;
            httpContext.Response.TrySkipIisCustomErrors = true;

            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = action;

            controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
            }
        }

        /// <summary>
        /// This is required for API method to sahre sessions with normal controllers - zz
        /// </summary>
        protected void Application_PostAuthorizeRequest()
        {
            HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
        }

    }

}
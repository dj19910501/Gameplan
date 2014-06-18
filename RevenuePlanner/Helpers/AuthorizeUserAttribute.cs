/// Added By: Maninder Singh Wadhva.
/// Date: 06/18/2014
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RevenuePlanner.Helpers
{
    public class AuthorizeUserAttribute : AuthorizeAttribute
    {
        // Summary:
        //     Custom data member to hold permission for action method.
        private Enums.ApplicationActivity Permissions;

        // Summary:
        //     Initializes a new instance of the AuthorizeUserAttribute class.
        public AuthorizeUserAttribute(Enums.ApplicationActivity permissions)
        {
            this.Permissions = permissions;
        }

        // Summary:
        //     When overridden, provides an entry point for custom authorization checks.
        //
        // Parameters:
        //   httpContext:
        //     The HTTP context, which encapsulates all HTTP-specific information about
        //     an individual HTTP request.
        //
        // Returns:
        //     true if the user is authorized; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     The httpContext parameter is null.
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }


            isAuthorized = IsAuthorized(Permissions);
            if (isAuthorized)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //
        // Summary:
        //     Processes HTTP requests that fail authorization.
        //
        // Parameters:
        //   filterContext:
        //     Encapsulates the information for using System.Web.Mvc.AuthorizeAttribute.
        //     The filterContext object contains the controller, HTTP context, request context,
        //     action result, and route data.
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                controller = "NoAccess",
                                action = "Index"
                            })
                        );
        }

        //
        // Summary:
        //     Checks whether user is authorized or not..
        //
        // Parameters:
        //   permissions:
        //     The permission object contains the permission for request.
        public static bool IsAuthorized(Enums.ApplicationActivity permissions)
        {
            return ((int)(Sessions.UserActivityPermission & permissions)) > 0;
        }

    }
}
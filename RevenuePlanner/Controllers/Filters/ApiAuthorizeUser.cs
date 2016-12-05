using System.Web.Http;
using System.Web.Http.Controllers;
using RevenuePlanner.Helpers;

namespace RevenuePlanner.Controllers.Filters
{
    public class ApiAuthorizeUser : AuthorizeAttribute
    {
        private Enums.ApplicationActivity _permissions;
        public ApiAuthorizeUser(Enums.ApplicationActivity permissions)
        {
            _permissions = permissions;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            return (Sessions.UserActivityPermission & _permissions) > 0;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace RevenuePlanner.Controllers.Filters
{
    /// <summary>
    /// Exception filter for the API Controller, handles exceptions thrown during when entering through an API Controller
    /// </summary>
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        /// <summary>
        /// We simply log the exception to Elmah, this is similar to what the CommonController does for non-api pages
        /// </summary>
        /// <param name="actionExecutedContext">Exected Context for the exception</param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnException(actionExecutedContext);
            Elmah.ErrorSignal.FromCurrentContext().Raise(actionExecutedContext.Exception);

        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;

namespace RevenuePlanner.Test.MockHelpers
{
    public static class MockHelpers
    {
        /// <summary>
        /// Fake HTTP Context class 
        /// </summary>
        /// <returns></returns>
        public static HttpContext FakeHttpContext()
        {
            var httpRequest = new HttpRequest("", "http://localhost:51115/", "");
            var stringWriter = new StringWriter();
            var httpResponce = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponce);

            var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                                                    new HttpStaticObjectsCollection(), 10, true,
                                                    HttpCookieMode.AutoDetect,
                                                    SessionStateMode.InProc, false);

            SessionStateUtility.AddHttpSessionStateToContext(httpContext, sessionContainer);

            return httpContext;
        }

    }
    public static class FakeUrlHelper
    {
        public static UrlHelper UrlHelper()
        {
            UrlHelper urlHelper = new UrlHelper(new RequestContext(new System.Web.HttpContextWrapper(HttpContext.Current), new RouteData()), new RouteCollection());
            return urlHelper;
        }
    }
}
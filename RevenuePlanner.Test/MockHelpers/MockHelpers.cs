﻿using Moq;
using System;
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
        public static HttpContextBase FakeHttpContext()
        {
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            var session = new Mock<HttpSessionStateBase>();
            var server = new Mock<HttpServerUtilityBase>();

            request.SetupGet(x => x.Headers).Returns(
    new System.Net.WebHeaderCollection {
        {"X-Requested-With", "XMLHttpRequest"}
    });

            context.SetupGet(x => x.Request).Returns(request.Object);
            request.Setup(r => r.AppRelativeCurrentExecutionFilePath).Returns("/");
            request.Setup(r => r.ApplicationPath).Returns("/");
            response.Setup(s => s.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(s => s);
            request.SetupGet(x => x.Url).Returns(new Uri("http://localhost:51115/", UriKind.Absolute));

            context.Setup(ctx => ctx.Request).Returns(request.Object);
            context.Setup(ctx => ctx.Response).Returns(response.Object);
            context.Setup(ctx => ctx.Session).Returns(session.Object);
            context.Setup(ctx => ctx.Server).Returns(server.Object);

            return context.Object;
        }
    }


}
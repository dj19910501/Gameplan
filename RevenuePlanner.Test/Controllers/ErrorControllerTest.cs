using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class ErrorControllerTest
    {
        #region Error
        /// <summary>
        /// Error View.
        /// <author>Rahul Shah</author>
        /// <createddate>11July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Error()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Error View.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ErrorController objErrorController = new ErrorController();
            objErrorController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objErrorController);
            objErrorController.Url = MockHelpers.FakeUrlHelper.UrlHelper();           
            var result = objErrorController.Error() as ViewResult;
            if (result != null)
            {
                Assert.IsNotNull(result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }
        #endregion

        #region Elmah Error
        /// <summary>
        /// Elmah Error View.
        /// <author>Rahul Shah</author>
        /// <createddate>11July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Elmah_Error()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Elmah Error View.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ErrorController objErrorController = new ErrorController();
            objErrorController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objErrorController);
            objErrorController.Url = MockHelpers.FakeUrlHelper.UrlHelper();           
            var result = objErrorController.ElmahError() as ViewResult;
            if (result != null)
            {
                Assert.IsNotNull(result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }
        #endregion

        #region Page Not Found
        /// <summary>
        /// Page Not Found.
        /// <author>Rahul Shah</author>
        /// <createddate>11July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Page_Not_Found()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Page Not Found.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ErrorController objErrorController = new ErrorController();
            objErrorController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objErrorController);
            objErrorController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            var result = objErrorController.PageNotFound() as ViewResult;
            if (result != null)
            {
                Assert.IsNotNull(result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }
        #endregion

       
    }
}

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
    public class CommonControllerTest
    {
        #region Load Support Partial
        /// <summary>
        /// Load Support Partial.
        /// <author>Rahul Shah</author>
        /// <createddate>12July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Load_Support_Partial()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Load Support Partial.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            CommonController objCommonController = new CommonController();
            objCommonController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objCommonController);
            objCommonController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = objCommonController.LoadSupportPartial() as PartialViewResult;
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

        #region Load No Model Partial
        /// <summary>
        /// Load No Model Partial.
        /// <author>Rahul Shah</author>
        /// <createddate>12July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Load_No_Model_Partial()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Load No Model Partial.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            CommonController objCommonController = new CommonController();
            objCommonController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objCommonController);
            objCommonController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = objCommonController.LoadNoModelPartial() as PartialViewResult;
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

        #region Load Session Warning
        /// <summary>
        /// Load Session Warning.
        /// <author>Rahul Shah</author>
        /// <createddate>12July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Load_Session_Warning()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Load Session Warning.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            CommonController objCommonController = new CommonController();
            objCommonController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objCommonController);
            objCommonController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = objCommonController.LoadSessionWarning() as PartialViewResult;
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Services;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class CurrencyControllerTest
    {
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
        }
        /// <summary>
        /// Add By Nishant Sheth
        /// Set Plan Currency Symbol and exchange rate in sessions varibles without UserId ClientId.
        /// Date : 16-Aug-2016
        /// </summary>
        [TestMethod]
        public void GetPlanCurrencyDetail_Without_UserId_ClientId()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To set plan currency exchange rate and currency symbol.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            CurrencyController objCurrencyController = new CurrencyController();
            objCurrencyController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objCurrencyController);
            objCurrencyController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = objCurrencyController.GetPlanCurrencyDetail() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Set Plan Currency Symbol and exchange rate in sessions varibles with UserId ClientId.
        /// Date : 16-Aug-2016
        /// </summary>
        [TestMethod]
        public void GetPlanCurrencyDetail_With_UserId_ClientId()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To set plan currency exchange rate and currency symbol.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ICurrency objCurrency = new Currency();
            var UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            var ClientId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ClientId;
            objCurrency.SetUserCurrencyCache(ClientId, UserId);
            CurrencyController objCurrencyController = new CurrencyController();
            objCurrencyController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objCurrencyController);
            objCurrencyController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = objCurrencyController.GetPlanCurrencyDetail() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        #region currency methodsB
        /// <summary>
        /// To Get Currency and Client Currency
        /// <author>Kausha Somaiya</author>
        /// <createddate>26-8-2016</createddate>
        /// </summary>
        [TestMethod]
        public void GetCurrency()
        {

            MRPEntities db = new MRPEntities();
            var routes = new RouteCollection();
            Console.WriteLine("To get currency and client currency.\n");
           
            HttpContext.Current = DataHelper.SetUserAndPermission();
            CurrencyController objCurrencyController = new CurrencyController();
            objCurrencyController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objCurrencyController);
            objCurrencyController.Url = MockHelpers.FakeUrlHelper.UrlHelper();


            var result = objCurrencyController.Index() as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.IsNotNull(result.Model);
            Assert.AreEqual("Index", result.ViewName);

        }
        /// <summary>
        /// To Get exchange rate data.
        /// <author>Kausha Somaiya</author>
        /// <createddate>26-8-2016</createddate>
        /// </summary>
        [TestMethod]
        public void GetExchangeRate()
        {

            MRPEntities db = new MRPEntities();
            var routes = new RouteCollection();
            Console.WriteLine("To get currency exchange rates.\n");

            HttpContext.Current = DataHelper.SetUserAndPermission();
            CurrencyController objCurrencyController = new CurrencyController();
            objCurrencyController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objCurrencyController);
            objCurrencyController.Url = MockHelpers.FakeUrlHelper.UrlHelper();


            var result = objCurrencyController.ExchangeRate() as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            //Assert.IsNotNull(result.Model);
            Assert.AreEqual("ExchangeRate", result.ViewName);

        }
        /// <summary>
        /// To Get plan grid data.
        /// <author>Kausha Somaiya</author>
        /// <createddate>26-8-2016</createddate>
        /// </summary>
        [TestMethod]
        public void GetPlanGridData()
        {

            MRPEntities db = new MRPEntities();
            var routes = new RouteCollection();
            Console.WriteLine("To get plan currency gridview data.\n");


            HttpContext.Current = DataHelper.SetUserAndPermission();
            CurrencyController objCurrencyController = new CurrencyController();
            objCurrencyController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objCurrencyController);
            objCurrencyController.Url = MockHelpers.FakeUrlHelper.UrlHelper();


            var result = objCurrencyController.PlanGrid("2016") as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.IsNotNull(result.Model);
            Assert.AreEqual("_PlanCurrencyGrid", result.ViewName);

        }
        /// <summary>
        /// To Get report grid data.
        /// <author>Kausha Somaiya</author>
        /// <createddate>26-8-2016</createddate>
        /// </summary>
        [TestMethod]
        public void GetReportGridData()
        {

            MRPEntities db = new MRPEntities();
            var routes = new RouteCollection();
            Console.WriteLine("To get report currency gridview data.\n");

            HttpContext.Current = DataHelper.SetUserAndPermission();
            CurrencyController objCurrencyController = new CurrencyController();
            objCurrencyController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objCurrencyController);
            objCurrencyController.Url = MockHelpers.FakeUrlHelper.UrlHelper();


            var result = objCurrencyController.ReportGrid("2016") as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.IsNotNull(result.Model);
            Assert.AreEqual("_ReportCurrencyGrid", result.ViewName);

        }
        /// <summary>
        /// To save client curruncy detail.
        /// <author>Kausha Somaiya</author>
        /// <createddate>26-8-2016</createddate>
        /// </summary>
        [TestMethod]
        public void SaveClientCurrency()
        {

            var routes = new RouteCollection();
            Console.WriteLine("To save client currency detail.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            CurrencyController objCurrencyController = new CurrencyController();
            objCurrencyController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objCurrencyController);
            objCurrencyController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            List<string> clientCurrncies = new List<string>();
            var result = objCurrencyController.SaveClientCurrency(clientCurrncies) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
     
        #endregion
    }
}

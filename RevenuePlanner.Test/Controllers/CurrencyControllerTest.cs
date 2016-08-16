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
    }
}

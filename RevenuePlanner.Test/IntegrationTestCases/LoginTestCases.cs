using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using System.Configuration;
using Excel = Microsoft.Office.Interop.Excel;
using RevenuePlanner.Test.MockHelpers;
using RevenuePlanner.Test.IntegrationHelpers;
using System.Data;
//using System.Data;
//using Moq;
namespace RevenuePlanner.Test.IntegrationHelpers
{

    [TestClass]
    public class LoginTestCases
    {
        public TestContext TestContext { get; set; }
        Microsoft.Office.Interop.Excel.Application oExcel = new Microsoft.Office.Interop.Excel.Application();
        LoginController objLoginController;
        CommonFunctions cm = new CommonFunctions();
        MRPEntities db = new MRPEntities();
        int PlanId; string filepath;

        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = MockHelpers.MockHelpers.FakeHttpContext();
            objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            PlanId = DataHelper.GetPlanId();
        }

        [TestMethod()]
        [DataSource("Login")]
        public void Login()
        {
            try
            {
                DataTable dt = this.TestContext.DataRow.Table;

                string UserEmail = TestContext.DataRow["USER"].ToString();
                string Password = TestContext.DataRow["Password"].ToString();

                Console.WriteLine("To Create Login View.\n");
                HttpContext.Current = IntegrationDataHelper.SetUserAndPermission(true, UserEmail, Password);
                Sessions.User.CID = DataHelper.GetClientId(PlanId);
                string returnURL = string.Empty;

                LoginModel login = new LoginModel();
                login.Password = Password;
                login.UserEmail = UserEmail;

                var result = objLoginController.Index(login, returnURL) as RedirectToRouteResult;

                Assert.AreEqual("Index", result.RouteValues["Action"]);
                Console.WriteLine("LoginController - Index With Parameters \n The assert value of Action : " + result.RouteValues["Action"]);

                Assert.AreEqual("Home", result.RouteValues["Controller"]);
                Console.WriteLine("LoginController - Index With Parameters \n The assert value of Controller : " + result.RouteValues["Controller"]);

                Assert.AreEqual("Home", result.RouteValues["ActiveMenu"]);
                Console.WriteLine("LoginController - Index With Parameters \n The assert value of Active menu : " + result.RouteValues["ActiveMenu"]);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}

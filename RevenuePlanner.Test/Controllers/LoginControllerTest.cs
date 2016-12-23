﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using System.Configuration;


namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class LoginControllerTest
    {
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
        }

        #region Create Login View
        /// <summary>
        /// To Create Login View.
        /// <author>Rahul Shah</author>
        /// <createddate>11July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Login_View()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Create Login View.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            var result = objLoginController.Index() as ViewResult;
            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["ApplicationReleaseVersion"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);

        }
        [TestMethod]
        public void Login_View_With_Error_Message()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Create Login View.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            MockHelpers.MockHelpers.TestTempDataHttpContext tempDataHttpContext = new MockHelpers.MockHelpers.TestTempDataHttpContext();
            objLoginController.TempData["ErrorMessage"] = "Unit";
            var result = objLoginController.Index() as ViewResult;
            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["ApplicationReleaseVersion"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);

        }
        #endregion

        #region Post Login
        /// <summary>
        /// To Post Login.
        /// <author>Rahul Shah</author>
        /// <createddate>11July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Index_Login()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Post Login.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            //int PlanId = DataHelper.GetPlanId();
            //Sessions.User.CID = DataHelper.GetClientId(PlanId);
            LoginModel form = new LoginModel();
            form.UserEmail = ConfigurationSettings.AppSettings["Username"].ToString() + "Wrong";
            form.Password = ConfigurationSettings.AppSettings["Password"].ToString();
            string returnURL = "https://172.30.17.111/gameplan/Home?activeMenu=Home";
            var result = objLoginController.Index(form, returnURL) as ViewResult;
            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["ApplicationReleaseVersion"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value resultvalue:  " + resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);

        }
        [TestMethod]
        public void Index_Login_With_True_Credintials()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Post Login.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objLoginController.Url = new UrlHelper(
            new RequestContext(
            objLoginController.HttpContext, new RouteData()
            ),
            routes
            );
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            LoginModel form = new LoginModel();
            form.UserEmail = ConfigurationSettings.AppSettings["Username"].ToString();
            form.Password = ConfigurationSettings.AppSettings["Password"].ToString();
            string returnURL = string.Empty;// ConfigurationSettings.AppSettings["ReturnUrl"].ToString();//"https://172.30.17.111/gameplan/Home?activeMenu=Home";
            var result = objLoginController.Index(form, returnURL) as RedirectToRouteResult;
            Assert.AreEqual("Index", result.RouteValues["Action"]);
            Assert.AreEqual("Home", result.RouteValues["Controller"]);
        }
        [TestMethod]
        public void Index_Login_With_Wrong_Password()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Post Login.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            int PlanId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            LoginModel form = new LoginModel();
            form.UserEmail = ConfigurationSettings.AppSettings["Username"].ToString();
            form.Password = ConfigurationSettings.AppSettings["Password"].ToString() + "Wrong";
            string returnURL = string.Empty;//"https://172.30.17.111/gameplan/Home?activeMenu=Home";
            var result = objLoginController.Index(form, returnURL) as ViewResult;
            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["ApplicationReleaseVersion"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value resultvalue:  " + resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
        }
        #endregion

        #region Load Support Partial On Login
        /// <summary>
        /// To Load Support Partial On Login .
        /// <author>Rahul Shah</author>
        /// <createddate>12July2016</createddate>
        /// </summary>
        [TestMethod]
        public void LoadSupportPartialOnLogin()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Load Support Partial On Login .\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            Sessions.User.ID = DataHelper.GetUserId(PlanId);
            var result = objLoginController.LoadSupportPartialOnLogin() as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);

        }
        #endregion

        #region Forgot Password View
        /// <summary>
        /// To Forgot Password View.
        /// <author>Rahul Shah</author>
        /// <createddate>12July2016</createddate>
        /// </summary>
        [TestMethod]
        public void ForgotPassword_View()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Forgot Password View.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = objLoginController.ForgotPassword() as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);

        }
        #endregion

        #region Reset Password View
        /// <summary>
        /// To Reset Password View.
        /// <author>Rahul Shah</author>
        /// <createddate>12July2016</createddate>
        /// </summary>
        [TestMethod]
        public void ResetPassword_View()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Reset Password View.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var lstUser = objBDSServiceClient.GetTeamMemberListEx(Sessions.User.CID, Sessions.ApplicationId, Sessions.User.ID, true);
            if (lstUser != null && lstUser.Count > 0)
            {
                string Email = lstUser.FirstOrDefault().Email;
                var objUser = objBDSServiceClient.GetUserDetails(Email);
                BDSService.PasswordResetRequest objPasswordResetRequest = new BDSService.PasswordResetRequest();
                objPasswordResetRequest.PasswordResetRequestId = Guid.NewGuid();
                objPasswordResetRequest.UserId = objUser.UserId;
                objPasswordResetRequest.AttemptCount = 0;
                objPasswordResetRequest.CreatedDate = DateTime.Now;
                string passwordResetRequestId = objBDSServiceClient.CreatePasswordResetRequest(objPasswordResetRequest);
                var result = objLoginController.ResetPassword(passwordResetRequestId, false) as ViewResult;
                var serializedData = new RouteValueDictionary(result.Model);
                var resultvalue = serializedData["RequestId"];
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value resultvalue:  " + Convert.ToString(resultvalue));
                Assert.AreEqual(passwordResetRequestId, Convert.ToString(resultvalue));
            }
        }
        #endregion

        #region Reset Password Post
        /// <summary>
        /// To Reset Password Post.
        /// <author>Rahul Shah</author>
        /// <createddate>12July2016</createddate>
        /// </summary>
        [TestMethod]
        public void ResetPassword_Post()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Reset Password Post.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            //Get Users Password Request
            BDSService.PasswordResetRequest objPasswordResetRequest = new BDSService.PasswordResetRequest();
            objPasswordResetRequest.PasswordResetRequestId = Guid.NewGuid();
            objPasswordResetRequest.UserId = Sessions.User.UserId;
            objPasswordResetRequest.AttemptCount = 0;
            objPasswordResetRequest.CreatedDate = DateTime.Now;

            string passwordResetRequestId = objBDSServiceClient.CreatePasswordResetRequest(objPasswordResetRequest);
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);

            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objLoginController.ControllerContext.HttpContext.Session.Add("RequestId", Guid.Parse(passwordResetRequestId));
            HttpContext.Current.Session["RequestId"] = passwordResetRequestId;
            ResetPasswordModel form = new ResetPasswordModel();
            form.RequestId = new Guid(passwordResetRequestId);
            //Set Users Password Request in to session which will be used in ResetPassword
            Sessions.RequestId = form.RequestId;
            form.NewPassword = Convert.ToString(ConfigurationManager.AppSettings["Password"]);
            form.ConfirmNewPassword = Convert.ToString(ConfigurationManager.AppSettings["Password"]);
            var result = objLoginController.ResetPassword(form) as ViewResult;
            var serializedData = new RouteValueDictionary(result.Model);
            var resultvalue = serializedData["RequestId"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + resultvalue.ToString());
            Assert.AreEqual(passwordResetRequestId, Convert.ToString(resultvalue));

        }
        #endregion

        #region Check Current Password
        /// <summary>
        /// To Check Current Password.
        /// <author>Rahul Shah</author>
        /// <createddate>12July2016</createddate>
        /// </summary>
        [TestMethod]
        public void CheckCurrentPassword()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Check Current Password.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            string currentPassword = Convert.ToString(ConfigurationManager.AppSettings["Password"]);
            //Get Users Password Request
            BDSService.PasswordResetRequest objPasswordResetRequest = new BDSService.PasswordResetRequest();
            objPasswordResetRequest.PasswordResetRequestId = Guid.NewGuid();
            objPasswordResetRequest.UserId = Sessions.User.UserId;
            objPasswordResetRequest.AttemptCount = 0;
            objPasswordResetRequest.CreatedDate = DateTime.Now;
            string passwordResetRequestId = objBDSServiceClient.CreatePasswordResetRequest(objPasswordResetRequest);
            //Set Users Password Request in to session which will be used in CheckCurrentPassword
            Sessions.RequestId = Guid.Parse(passwordResetRequestId);
            var result = objLoginController.CheckCurrentPassword(currentPassword, Guid.Parse(passwordResetRequestId)) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }

        [TestMethod]
        public void ForgotPassword_POST()
        {

            RouteCollection routes = new RouteCollection();
            Console.WriteLine("To POST Forgot Password View.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            ForgotPasswordModel form = new ForgotPasswordModel();
            form.UserEmail = ConfigurationSettings.AppSettings["Username"].ToString();
            var result = objLoginController.ForgotPassword(form) as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value ViewName:  " + result);
            Assert.AreEqual(((RevenuePlanner.Models.ForgotPasswordModel)(((System.Web.Mvc.ViewResultBase)(result)).Model)).UserEmail, form.UserEmail);

        }

        #endregion

        #region return Site maintainance
        /// <summary>
        /// To returnt Site maintainance.
        /// <author>Rahul Shah</author>
        /// <createddate>12July2016</createddate>
        /// </summary>
        [TestMethod]
        public void MaintenanceSite()
        {
            var routes = new RouteCollection();
            Console.WriteLine("returnt Site maintainance.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = objLoginController.MaintenanceSite() as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
        }
        #endregion
    }
}

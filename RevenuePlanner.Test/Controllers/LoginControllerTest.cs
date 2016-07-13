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


namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class LoginControllerTest
    {
        //#region DBService Unavailable
        ///// <summary>
        ///// To DBService Unavailable.
        ///// <author>Rahul Shah</author>
        ///// <createddate>11July2016</createddate>
        ///// </summary>
        //[TestMethod]
        //public void DBService_Unavailable()
        //{
        //    var routes = new RouteCollection();
        //    Console.WriteLine("To check DBService Unavailable.\n");
        //    MRPEntities db = new MRPEntities();
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    LoginController objLoginController = new LoginController();
        //    objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
        //    objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

        //    int PlanId = DataHelper.GetPlanId();
        //    Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
        //    Sessions.PlanId = PlanId;

        //    var result = objLoginController.DBServiceUnavailable() as ViewResult;

        //    if (result != null)
        //    {
        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
        //    }
        //    else
        //    {
        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
        //    }

        //}
        //#endregion

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
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);

            var result = objLoginController.Index() as ViewResult;

            if (result != null)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

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
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            LoginModel form = new LoginModel();
            form.UserEmail = "test@Hive9.com";
            form.Password = "Test@1234";
            string returnURL = "https://172.30.17.111/gameplan/Home?activeMenu=Home";
            var result = objLoginController.Index(form, returnURL) as ViewResult;

            if (result != null)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        //#region Log off
        ///// <summary>
        ///// To log off.
        ///// <author>Rahul Shah</author>
        ///// <createddate>12July2016</createddate>
        ///// </summary>
        //[TestMethod]
        //public void LogOff()
        //{
        //    var routes = new RouteCollection();
        //    Console.WriteLine("To Log off.\n");
        //    MRPEntities db = new MRPEntities();
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    LoginController objLoginController = new LoginController();
        //    objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
        //    objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
        //    int PlanId = DataHelper.GetPlanId();
        //    Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
        //    var result = objLoginController.LogOff() as ActionResult;

        //    if (result != null)
        //    {
        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result);
        //    }
        //    else
        //    {
        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
        //    }

        //}
        //#endregion

        //#region Support request Issue field 
        ///// <summary>
        ///// To Support request Issue field .
        ///// <author>Rahul Shah</author>
        ///// <createddate>12July2016</createddate>
        ///// </summary>
        //[TestMethod]
        //public void Contact_Support()
        //{
        //    var routes = new RouteCollection();
        //    Console.WriteLine("To Support request Issue field .\n");
        //    MRPEntities db = new MRPEntities();
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
        //    LoginController objLoginController = new LoginController();
        //    objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
        //    objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
        //    int PlanId = DataHelper.GetPlanId();
        //    Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
        //    Sessions.User.UserId = DataHelper.GetUserId(PlanId);
        //    var lstUser = objBDSServiceClient.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, true);
        //    if (lstUser != null && lstUser.Count > 0)
        //    {
        //        string EmailId = lstUser.FirstOrDefault().Email;
        //        string CompanyName = "Hive9";
        //        string Issue = "Data not support";
        //        var result = objLoginController.ContactSupport(EmailId, CompanyName, Issue) as JsonResult;

        //        if (result.Data != null)
        //        {
        //            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
        //        }
        //        else
        //        {
        //            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + "Data not found.");
        //    }

        //}
        //#endregion

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
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);
            var result = objLoginController.LoadSupportPartialOnLogin() as PartialViewResult;

            if (result != null)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }



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

            if (result != null)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }



        }
        #endregion

        //#region Forgot Password Submit
        ///// <summary>
        ///// To Forgot Password Submit.
        ///// <author>Rahul Shah</author>
        ///// <createddate>12July2016</createddate>
        ///// </summary>
        //[TestMethod]
        //public void ForgotPassword_Submit()
        //{
        //    var routes = new RouteCollection();
        //    Console.WriteLine("To Forgot Password Submit.\n");
        //    MRPEntities db = new MRPEntities();
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    LoginController objLoginController = new LoginController();
        //    objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
        //    objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
        //    ForgotPasswordModel form = new ForgotPasswordModel();
        //    form.UserEmail = "rahul.shah@indusa.com";
        //    form.IsSuccess = true;

        //    var result = objLoginController.ForgotPassword(form) as ActionResult;

        //    if (result != null)
        //    {
        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result);
        //    }
        //    else
        //    {
        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
        //    }



        //}
        //#endregion

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
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);
            var lstUser = objBDSServiceClient.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, true);

            if (lstUser != null && lstUser.Count > 0)
            {
                string Email = lstUser.FirstOrDefault().Email;
                var objUser = objBDSServiceClient.GetUserDetails(Email);
                BDSService.PasswordResetRequest objPasswordResetRequest = new BDSService.PasswordResetRequest();
                objPasswordResetRequest.PasswordResetRequestId = Guid.NewGuid();
                objPasswordResetRequest.UserId = objUser.UserId;
                objPasswordResetRequest.AttemptCount = 0;
                objPasswordResetRequest.CreatedDate = DateTime.Now;

                string PasswordResetRequestId = objBDSServiceClient.CreatePasswordResetRequest(objPasswordResetRequest);

                var result = objLoginController.ResetPassword(PasswordResetRequestId, false) as ViewResult;

                if (result != null)
                {
                    Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
                }
                else
                {
                    Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
                }
            }
            else {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + "Data not found.");
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
            LoginController objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);

            ResetPasswordModel form = new ResetPasswordModel();
            form.UserId = Sessions.User.UserId;
            form.NewPassword = "test1234";
            form.ConfirmNewPassword = "test1234";

            var result = objLoginController.ResetPassword(form) as ViewResult;

            if (result != null)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }


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
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);
            string currentPassword = "Test1234";
           
            var result = objLoginController.CheckCurrentPassword(currentPassword, Sessions.User.UserId.ToString()) as JsonResult;

            if (result.Data != null)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }


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

            if (result != null)
            {
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
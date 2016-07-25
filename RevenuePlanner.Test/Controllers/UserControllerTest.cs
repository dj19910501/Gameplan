using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.BDSService;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
namespace RevenuePlanner.Test.Controllers
{

    [TestClass]
    public class UserControllerTest
    {
        #region Team Member Listing.
        /// <summary>
        /// Team Member Listing.
        /// <author>Rahul Shah</author>
        /// <createddate>07July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Index_User()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Team Member Listing.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);
            var result = objUserController.Index() as ViewResult;

            if (result != null)
            {
                Assert.IsNotNull(result.Model);
                var serializedData = new RouteValueDictionary(result.Model);
                var resultvalue = serializedData["Count"];
                Assert.IsNotNull(resultvalue.ToString());
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + resultvalue.ToString());
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Change Password Form.
        /// <summary>
        /// To Change Password Form.
        /// <author>Rahul Shah</author>
        /// <createddate>07July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Change_Password_Form()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Change Password Form.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
         
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
           
            var result = objUserController.ChangePassword() as ViewResult;

            if (result != null)
            {
                Assert.IsNotNull(result.ViewData);               
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Change Password Save.
        /// <summary>
        /// To Change Password Save.
        /// <author>Rahul Shah</author>
        /// <createddate>07July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Change_Password_Save()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Change Password Save.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
           
            int PlanId = DataHelper.GetDeletedPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);           
           
            UserChangePassword form = new UserChangePassword();
            form.UserId = 0;
            form.CurrentPassword = "Test@123";
            form.NewPassword = "Test@1234";
            form.ConfirmNewPassword = "Test@1234";

            var result = objUserController.ChangePassword(form) as ViewResult;

            if (result != null)
            {
                Assert.IsNotNull(result.Model);
                var serializedData = new RouteValueDictionary(result.Model);
                var resultvalue = serializedData["UserId"];
                var resultvalue1 = serializedData["CurrentPassword"];
                Assert.IsNotNull(resultvalue.ToString());
                Assert.IsNotNull(resultvalue1.ToString());
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + "UserID: " + resultvalue.ToString());
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + "Password: " + resultvalue1.ToString());
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Check Current Password.
        /// <summary>
        /// To Check Current Password.
        /// <author>Rahul Shah</author>
        /// <createddate>08July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Check_Current_Password()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Save Budget Allocation.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();          

            int PlanId = DataHelper.GetDeletedPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);
            string currentPassword = "Test@12345";
          
            var result = objUserController.CheckCurrentPassword(currentPassword) as JsonResult;

            if (result != null)
            {
                Assert.IsNotNull(result.Data);               
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Create User Form.
        /// <summary>
        /// To Create User Form.
        /// <author>Rahul Shah</author>
        /// <createddate>08July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Create_User_Form()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Create User Form.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;

            var result = objUserController.Create() as ViewResult;

            if (result != null)
            {
                Assert.IsNotNull(result.ViewName);
                var serializedData = new RouteValueDictionary(result.ViewData);
                var resultvalue = serializedData["CurrClient"];               
                Assert.IsNotNull(resultvalue.ToString());                
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + "UserID: " + resultvalue.ToString());
           
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Save New User.
        /// <summary>
        /// To Save New User.
        /// <author>Rahul Shah</author>
        /// <createddate>08July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Save_User()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Save New User.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            int PlanId = DataHelper.GetDeletedPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);

            UserModel form = new UserModel();
            form.DisplayName = "Test@Hive9";
            form.FirstName = "Test";
            form.LastName = "Hive9";
            form.Email = "Test@Hive9.com";
            form.Password = "hive9@123";
            form.ConfirmPassword = "hive9@123";
            form.RoleTitle = "Admin";
            form.ClientId = Sessions.User.ClientId;
            form.Client = Sessions.User.Client.ToString();
            form.ManagerName = "Hive9";
            form.JobTitle = "Admin";

            var result = objUserController.Create(form,null) as ViewResult;

            if (result != null)
            {
                Assert.IsNotNull(result.ViewName);
                var serializedData = new RouteValueDictionary(result.ViewData);
                var resultvalue = serializedData["CurrClient"];
                Assert.IsNotNull(resultvalue.ToString());
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + "UserID: " + resultvalue.ToString());
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Check Email Exist or Not.
        /// <summary>
        /// To Check Email Exist or Not with new emailId.
        /// <author>Rahul Shah</author>
        /// <createddate>08July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Email_Exist_with_New_EmailId()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Check Email Exist or Not with new emailId.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            int PlanId = DataHelper.GetDeletedPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);

            string Email = "UnitTest@Hive9.com";
            var result = objUserController.IsEmailExist(Email) as JsonResult;

            if (result != null)
            {
                Assert.IsNotNull(result.Data);
                var serializedData = new RouteValueDictionary(result.Data);              
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }

        /// <summary>
        /// To Check Email Exist or Not with existing emailId.
        /// <author>Rahul Shah</author>
        /// <createddate>08July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Email_Exist_with_Old_EmailId()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Check Email Exist or Not with existing emailId.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            var objUser = objBDSServiceClient.GetTeamMemberDetails(Sessions.User.UserId, Sessions.ApplicationId);            
            if (objUser != null)
            {
                string Email = objUser.Email;
                var result = objUserController.IsEmailExist(Email) as JsonResult;

                if (result != null)
                {
                    Assert.IsNotNull(result.Data);                   
                    Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
                }
                else
                {
                    Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
                }
            }
            else {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + "No User found for Client" + Sessions.User.Client.ToString());
            }

        }
        #endregion

        #region Edit User Form.
        /// <summary>
        /// To Edit User Form.
        /// <author>Rahul Shah</author>
        /// <createddate>07July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Edit_User_Form()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Edit User Form.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);

            var result = objUserController.Edit() as ViewResult;

            if (result != null)
            {
                Assert.IsNotNull(result.ViewName);
                var serializedData = new RouteValueDictionary(result.ViewData);
                var resultvalue = serializedData["IsUserAdminAuthorized"];
                Assert.IsNotNull(resultvalue.ToString());
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + "UserID: " + resultvalue.ToString());
               
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Edit User
        /// <summary>
        /// To Edit User.
        /// <author>Rahul Shah</author>
        /// <createddate>07July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Edit_User()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Edit User.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objUserController.Url = new UrlHelper(
  new RequestContext(
      objUserController.HttpContext, new RouteData()
  ),
  routes
);
            //objUserController.ControllerContext.HttpContext.Request.UrlReferrer.AbsolutePath = "";
            
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);
            UserModel form = new UserModel();
            form.DisplayName = "Test@Hive9";
            form.FirstName = "Test";
            form.LastName = "Hive9";
            form.Email = "Test@Hive9.com";
            form.Password = "hive9@123";
            form.ConfirmPassword = "hive9@123";
            form.RoleTitle = "Admin";
            form.ClientId = Sessions.User.ClientId;
            form.Client = Sessions.User.Client.ToString();
            form.ManagerName = "Hive9";
            form.JobTitle = "Admin";

            FormCollection formCollection = new FormCollection();
            formCollection.Add("UserId","UserId");
            var result = objUserController.Edit(form,null, formCollection) as ActionResult;

            
            if (result != null)
            {
                Assert.IsNotNull(result);                
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Load Notification
        /// <summary>
        /// To Load Notification.
        /// <author>Rahul Shah</author>
        /// <createddate>07July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Load_Notification()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Load Notification.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objUserController.Url = new UrlHelper(
  new RequestContext(
      objUserController.HttpContext, new RouteData()
  ),
  routes
);

            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);           
            
            var result = objUserController.Notifications() as ViewResult;

            if (result != null)
            {
                Assert.IsNotNull(result.Model);
                List<UserNotification> objModelList = (List<UserNotification>)result.Model;
                UserNotification objModel = objModelList.FirstOrDefault();
                var serializedData = new RouteValueDictionary(objModel);
                var resultvalue = serializedData["NotificationTitle"];
                Assert.IsNotNull(resultvalue.ToString());
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + resultvalue.ToString());                
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Save Notification
        /// <summary>
        /// To Save Notification.
        /// <author>Rahul Shah</author>
        /// <createddate>07July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Save_Notification()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Save Notification.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objUserController.Url = new UrlHelper(
  new RequestContext(
      objUserController.HttpContext, new RouteData()
  ),
  routes
);

            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);
            int NotificationId = db.Notifications.Select(no => no.NotificationId).FirstOrDefault();
            string strNotification = NotificationId.ToString();
            try
            {
                //here we are calling void  method so it doesn't return any data.
                objUserController.SaveNotifications(strNotification);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }                 

        }
        #endregion

        #region Get Managers List
        /// <summary>
        /// To Get Managers List withosut passing ClientId and UserId.
        /// <author>Rahul Shah</author>
        /// <createddate>08July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_ManagersList_Without_Passing_Parameters()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Managers List withosut passing ClientId and UserId.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objUserController.Url = new UrlHelper(
  new RequestContext(
      objUserController.HttpContext, new RouteData()
  ),
  routes
);

            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);

            var result = objUserController.GetManagers() as JsonResult;
            if (result != null)
            {
                Assert.IsNotNull(result.Data);                
                var serializedData = new RouteValueDictionary(result.Data);
                var resultvalue = serializedData["Count"];
                Assert.IsNotNull(resultvalue.ToString());
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + resultvalue.ToString());
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }

        /// <summary>
        /// To Get Managers List withosut passing ClientId.
        /// <author>Rahul Shah</author>
        /// <createddate>08July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_ManagersList_Without_Passing_ClientId()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Managers List withosut passing ClientId.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objUserController.Url = new UrlHelper(
  new RequestContext(
      objUserController.HttpContext, new RouteData()
  ),
  routes
);

            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);
            string UserId = Sessions.User.UserId.ToString();
            var result = objUserController.GetManagers(null,UserId) as JsonResult;
            if (result != null)
            {
                Assert.IsNotNull(result.Data);
                var serializedData = new RouteValueDictionary(result.Data);
                var resultvalue = serializedData["Count"];
                Assert.IsNotNull(resultvalue.ToString());
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + resultvalue.ToString());
              
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }

        /// <summary>
        /// To Get Managers List with passing All Parameter.
        /// <author>Rahul Shah</author>
        /// <createddate>08July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_ManagersList_With_Passing_All_Parameter()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Managers List with passing All Parameter.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objUserController.Url = new UrlHelper(
  new RequestContext(
      objUserController.HttpContext, new RouteData()
  ),
  routes
);

            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);
            string UserId = Sessions.User.UserId.ToString();
            string ClientId = Sessions.User.ClientId.ToString();
            var result = objUserController.GetManagers(ClientId, UserId) as JsonResult;
            if (result != null)
            {
                Assert.IsNotNull(result.Data);
                var serializedData = new RouteValueDictionary(result.Data);
                var resultvalue = serializedData["Count"];
                Assert.IsNotNull(resultvalue.ToString());
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + resultvalue.ToString());
                
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Assign Other Application User
        /// <summary>
        /// To Assign Other Application User.
        /// <author>Rahul Shah</author>
        /// <createddate>07July2016</createddate>
        /// </summary>
        [TestMethod]
        public void Assign_User()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Assign Other Application User.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            UserController objUserController = new UserController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();           

            int PlanId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            Sessions.PlanId = PlanId;
            Sessions.User.UserId = DataHelper.GetUserId(PlanId);
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            var objUser = objBDSServiceClient.GetTeamMemberDetails(Sessions.User.UserId, Sessions.ApplicationId);
            if (objUser != null)
            {
                string userId = objUser.UserId.ToString();
                string RoleId = objUser.RoleId.ToString();
                var result = objUserController.AssignUser(userId, RoleId) as JsonResult;
                if (result != null)
                {
                    Assert.IsNotNull(result.Data);                    
                    Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
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

    }


}

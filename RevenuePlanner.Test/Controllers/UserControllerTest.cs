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
        BDSServiceClient objBDSServiceClient = new BDSServiceClient();
        UserController objUserController = new UserController();
        MRPEntities db = new MRPEntities();
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

        }
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
           
         
            var result = objUserController.Index() as ViewResult;
            Assert.IsNotNull(result.Model);
            var serializedData = new RouteValueDictionary(result.Model);
            var resultvalue = serializedData["Count"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue: " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
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
          
         
            var result = objUserController.ChangePassword() as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result :" + result.ViewName);
            Assert.IsNotNull(result.ViewData);
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
          
            UserChangePassword form = new UserChangePassword();
            form.UserId = 0;
            form.CurrentPassword = "Test@123";
            form.NewPassword = "Test@1234";
            form.ConfirmNewPassword = "Test@1234";


            var result = objUserController.ChangePassword(form) as ActionResult;
            Assert.IsNotNull(result);
            var serializedData = new RouteValueDictionary(result);
            var resultvalue = new RouteValueDictionary(serializedData.Keys);            
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());                       
            Assert.IsTrue(resultvalue.Count > 0);
            
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
          
            string currentPassword = "Test@12345";
          
            var result = objUserController.CheckCurrentPassword(currentPassword) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
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
          
            var result = objUserController.Create() as ViewResult;
            Assert.IsNotNull(result.ViewName);
            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["CurrClient"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + "UserID: " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
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
          
            UserModel form = new UserModel();
            form.DisplayName = "Test@Hive9";
            form.FirstName = "Test";
            form.LastName = "Hive9";
            form.Email = "Test@Hive9.com";
            form.Password = "hive9@123";
            form.ConfirmPassword = "hive9@123";
            form.RoleTitle = "Admin";
            form.ClientId = Sessions.User.CID;
            form.Client = Sessions.User.Client.ToString();
            form.ManagerName = "Hive9";
            form.JobTitle = "Admin";

            var result = objUserController.Create(form,null) as ViewResult;
            Assert.IsNotNull(result.ViewName);
            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["CurrClient"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + "UserID: " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
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
         
            string Email = "UnitTest@Hive9.com";
            var result = objUserController.IsEmailExist(Email) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
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

            var objUser = objBDSServiceClient.GetTeamMemberDetailsEx(Sessions.User.ID, Sessions.ApplicationId);            
            if (objUser != null)
            {
                string Email = objUser.Email;
                var result = objUserController.IsEmailExist(Email) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
             
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
           
            var result = objUserController.Edit() as ViewResult;
            Assert.IsNotNull(result.ViewName);
            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["IsUserAdminAuthorized"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + "UserID: " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
         

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
           
            UserModel form = new UserModel();
            form.DisplayName = "Test@Hive9";
            form.FirstName = "Test";
            form.LastName = "Hive9";
            form.Email = "Test@Hive9.com";
            form.Password = "hive9@123";
            form.ConfirmPassword = "hive9@123";
            form.RoleTitle = "Admin";
            form.ClientId = Sessions.User.CID;
            form.Client = Sessions.User.Client.ToString();
            form.ManagerName = "Hive9";
            form.JobTitle = "Admin";

            FormCollection formCollection = new FormCollection();
            formCollection.Add("UserId","UserId");
            var result = objUserController.Edit(form,null, formCollection) as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
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
          
            
            var result = objUserController.Notifications() as ViewResult;
            Assert.IsNotNull(result.Model);
            List<UserNotification> objModelList = (List<UserNotification>)result.Model;
            UserNotification objModel = objModelList.FirstOrDefault();
            var serializedData = new RouteValueDictionary(objModel);
            var resultvalue = serializedData["NotificationTitle"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());

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
          
            var result = objUserController.GetManagers() as JsonResult;
            Assert.IsNotNull(result.Data);
            var serializedData = new RouteValueDictionary(result.Data);
            var resultvalue = serializedData["Count"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue :  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());

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
            
            Guid UserId = Sessions.User.UserId;
            var result = objUserController.GetManagers(0,UserId) as JsonResult;
            Assert.IsNotNull(result.Data);
            var serializedData = new RouteValueDictionary(result.Data);
            var resultvalue = serializedData["Count"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
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
           
            Guid UserId = Sessions.User.UserId;
            int ClientId = Sessions.User.CID;
            var result = objUserController.GetManagers(ClientId, UserId) as JsonResult;
            Assert.IsNotNull(result.Data);
            var serializedData = new RouteValueDictionary(result.Data);
            var resultvalue = serializedData["Count"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());

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
           
            var objUser = objBDSServiceClient.GetTeamMemberDetailsEx(Sessions.User.ID, Sessions.ApplicationId);
            if (objUser != null)
            {
                Guid userId = objUser.UserId;
                string RoleId = objUser.RoleId.ToString();
                var result = objUserController.AssignUser(userId, RoleId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);                    
            }
           
        }
        #endregion                

        #region Alerts and alert Rule
        #region method to get rule list
        /// <summary>
        /// To Get the list of Alert rule
        /// <author>Devanshi gandhi</author>
        /// <createddate>12-8-2016</createddate>
        /// </summary>
        [TestMethod]
        public void GetAlertRuleList()
        {
       
            var result = objUserController.GetAlertRuleList() as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.IsNotNull(result.Model);
            Assert.AreEqual("_AlertRuleListing", result.ViewName);

        }
        #endregion
        #region method to Search entity
        /// <summary>
        /// To Get the list of entity as per search text
        /// <author>Devanshi gandhi</author>
        /// <createddate>12-8-2016</createddate>
        /// </summary>
        [TestMethod]
        public void SearchListEntity()
        {

            string SearchTerm = "plan test";
            var result = objUserController.ListEntity(SearchTerm) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region method to delete alert rule
        /// <summary>
        /// To Delete the Alert rule
        /// <author>Devanshi gandhi</author>
        /// <createddate>17-8-2016</createddate>
        /// </summary>
        [TestMethod]
        public void DeleteAlertrule()
        {

           
            AlertRuleDetail objRule = new AlertRuleDetail();
            int ruleId = 0;
            int entityid = (DataHelper.GetPlanId());
            objRule.RuleId = 0;
            objRule.EntityID = Convert.ToString(entityid);
            objRule.RuleSummary = "test alert rule";
            objRule.EntityType = "Plan";
            objRule.Indicator = "MQL";
            objRule.IndicatorComparision = "LT";
            objRule.IndicatorGoal = "25";
            objRule.CompletionGoal = "25";
            objRule.Frequency = "Daily";
            var result = objUserController.SaveAlertRule(objRule, 0) as JsonResult;
            var rules = db.Alert_Rules.Where(a => a.RuleSummary == "test alert rule" && a.EntityId == entityid).Select(a => a).FirstOrDefault();
            if (rules != null)
                ruleId = rules.RuleId;
            result = objUserController.DeleteAlertRule(ruleId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion
        #region method to enable/disable alert rule
        /// <summary>
        /// To enable/disable the Alert rule
        /// <author>Devanshi gandhi</author>
        /// <createddate>17-8-2016</createddate>
        /// </summary>
        [TestMethod]
        public void EnableDisableAlertrule()
        {
            int ruleId = DataHelper.GetAlertruleId(Sessions.User.ID);
            bool turnOff = true;
            var result = objUserController.DisableAlertRule(ruleId, turnOff) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion
        #region method to get rule list
        /// <summary>
        /// To Get the list of Top 5 alert
        /// <author>Devanshi gandhi</author>
        /// <createddate>17-8-2016</createddate>
        /// </summary>
        [TestMethod]
        public void GetAlertSummary()
        {

           

            var result = objUserController.GetAlertNotificationSummary() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
         
        }
        #endregion
        #region method to save alert rule
        #region method to delete alert rule
        /// <summary>
        /// To Save the Alert rule
        /// <author>Devanshi gandhi</author>
        /// <createddate>17-8-2016</createddate>
        /// </summary>
        [TestMethod]
        public void SaveAlertrule()
        {

            MRPEntities db = new MRPEntities();
         
            AlertRuleDetail objRule = new AlertRuleDetail();
            objRule.RuleId = 0;
            objRule.EntityID = Convert.ToString(DataHelper.GetPlanId());
            objRule.RuleSummary = "<h4>  Responses are less than 75% of Goal</h4><br/><span>Start at 50% completion</span><span>Repeat Daily</span>";
            objRule.EntityType = "Plan";
            objRule.Indicator = "MQL";
            objRule.IndicatorComparision = "LT";
            objRule.IndicatorGoal = "25";
            objRule.CompletionGoal = "25";
            objRule.Frequency = "Daily";
            var result = objUserController.SaveAlertRule(objRule, 0) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion
        #endregion
        #endregion
    }


}

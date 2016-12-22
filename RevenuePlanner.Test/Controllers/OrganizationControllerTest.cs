using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.BDSService;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class OrganizationControllerTest
    {
        OrganizationController objOrganizationController = new OrganizationController();
        BDSServiceClient objBDSServiceClient = new BDSServiceClient();
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
            HttpContext.Current = DataHelper.SetUserAndPermission();
        }
        #region PL#1139 Custom Fields: Changes to custom restriction - db design/development

        #region ViewEditPermission in View Mode with no userId
        /// <summary>
        /// To check ViewEditPermission in View Mode with no userId
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>16Jan2015</createddate>
        [TestMethod]
        public void ViewEditPermission_View_Mode_With_No_User_Id()
        {
            Console.WriteLine("To check ViewEditPermission in View Mode with no userId.\n");
            //// Set session value

            //// Call view edit permission method
            var result = objOrganizationController.ViewEditPermission(new Guid(), Enums.UserPermissionMode.View.ToString()) as RedirectToRouteResult;

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value ViewName:  " + result.RouteValues["Action"]);
            Assert.AreEqual("OrganizationHierarchy", result.RouteValues["Action"]);
            
        }
        #endregion

        #region ViewEditPermission in View Mode with valid user id
        /// <summary>
        /// To check ViewEditPermission in View Mode with valid user id
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>16Jan2015</createddate>
        [TestMethod]
        public void ViewEditPermission_View_Mode_With_Valid_User_Id()
        {
            Console.WriteLine("To check ViewEditPermission in View Mode with valid user id.\n");
            //// Set session value

            //// Call view edit permission method
            var result = objOrganizationController.ViewEditPermission(Sessions.User.UserId, Enums.UserPermissionMode.View.ToString()) as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value ViewName:  " + result.ViewName);
            Assert.AreEqual("ViewEditPermission", result.ViewName);
           
        }
        #endregion

        #region ViewEditPermission in Edit Mode with no userId
        /// <summary>
        /// To check ViewEditPermission in Edit Mode with no userId
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>16Jan2015</createddate>
        [TestMethod]
        public void ViewEditPermission_Edit_Mode_With_No_User_Id()
        {
            Console.WriteLine("To check ViewEditPermission in Edit Mode with no userId.\n");
          

            //// Call view edit permission method
            var result = objOrganizationController.ViewEditPermission(new Guid(), Enums.UserPermissionMode.Edit.ToString()) as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNull(result);
        
        }
        #endregion

        #region ViewEditPermission in Edit Mode with valid user id
        /// <summary>
        /// To check ViewEditPermission in Edit Mode with valid user id
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>16Jan2015</createddate>
        [TestMethod]
        public void ViewEditPermission_Edit_Mode_With_Valid_User_Id()
        {
            Console.WriteLine("To check ViewEditPermission in Edit Mode with valid user id.\n");
           
            var result = objOrganizationController.ViewEditPermission(Sessions.User.UserId, Enums.UserPermissionMode.Edit.ToString()) as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
           
        }
        #endregion

        #region Save user permission and custom restriction with Empty Parameters
        /// <summary>
        /// To check Save user permission and custom restriction with Empty Parameters
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>16Jan2015</createddate>
        [TestMethod]
        public void Save_User_Permission_And_Custom_Restricion_With_Empty_Parameters()
        {
            Console.WriteLine("To check Save user permission and custom restriction with Empty Parameters.\n");
          
            var result = objOrganizationController.SaveUserPermission(string.Empty, new Guid()) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.GetValue:  " + result.GetValue("status"));
            Assert.IsNotNull(result.GetValue("status"));
           
        }
        #endregion

        #region Save user permission and custom restriction with View/Edit rights
        /// <summary>
        /// To check Save user permission and custom restriction with View/Edit rights
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>16Jan2015</createddate>
        [TestMethod]
        public void Save_User_Permission_And_Custom_Restricion_With_ViewEdit_Rights()
        {
            Console.WriteLine("To check Save user permission and custom restriction with View/Edit rights.\n");
           
            string customRestrictions = DataHelper.GetCustomRestrictionInViewEditForm(Sessions.User.ID);
            var result = objOrganizationController.SaveUserPermission(customRestrictions, Sessions.User.UserId) as JsonResult;

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.GetValue:  " + result.GetValue("status"));
            Assert.IsNotNull(result.GetValue("status"));
           
        }
        #endregion

        #endregion
    }
}

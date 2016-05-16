using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call view edit permission method
            OrganizationController objOrganizationController = new OrganizationController();
            var result = objOrganizationController.ViewEditPermission(Guid.Empty.ToString(), Enums.UserPermissionMode.View.ToString()) as ViewResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.AreEqual("ViewEditPermission", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
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
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call view edit permission method
            OrganizationController objOrganizationController = new OrganizationController();
            var result = objOrganizationController.ViewEditPermission(Sessions.User.UserId.ToString(), Enums.UserPermissionMode.View.ToString()) as ViewResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.AreEqual("ViewEditPermission", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call view edit permission method
            OrganizationController objOrganizationController = new OrganizationController();
            var result = objOrganizationController.ViewEditPermission(Guid.Empty.ToString(), Enums.UserPermissionMode.Edit.ToString()) as ViewResult;

            if (result == null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call view edit permission method
            OrganizationController objOrganizationController = new OrganizationController();
            var result = objOrganizationController.ViewEditPermission(Sessions.User.UserId.ToString(), Enums.UserPermissionMode.Edit.ToString()) as ActionResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call save user permission and custom restriction method
            OrganizationController objOrganizationController = new OrganizationController();
            var result = objOrganizationController.SaveUserPermission(string.Empty, Guid.Empty.ToString()) as JsonResult;

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Data);

                //// Json result data should contain status property
                Assert.IsNotNull(result.GetValue("status"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("status"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call save user permission and custom restriction method
            OrganizationController objOrganizationController = new OrganizationController();
            string customRestrictions = DataHelper.GetCustomRestrictionInViewEditForm(Sessions.User.UserId);
            var result = objOrganizationController.SaveUserPermission(customRestrictions, Sessions.User.UserId.ToString()) as JsonResult;

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Data);

                //// Json result data should contain status property
                Assert.IsNotNull(result.GetValue("status"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("status"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }
        #endregion

        #endregion
    }
}

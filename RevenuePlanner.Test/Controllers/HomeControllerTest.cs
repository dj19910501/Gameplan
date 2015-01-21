using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
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
    public class HomeControllerTest
    {
        #region PL#1134 Multi select: Home/Plan page changes for custom fields

        #region Home page with no parameters
        /// <summary>
        /// To check to retrieve Home view with no parameters
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>21Jan2015</createddate>
        [TestMethod]
        public void Get_Home_View_With_No_Parameters()
        {
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            HomeController objHomeController = new HomeController();
            var result = objHomeController.Index() as ViewResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                if (!(result.ViewName.Equals("Index") || result.ViewName.Equals("PlanSelector")))
                {
                    Assert.Fail();
                }
                else if (result.ViewName.Equals("Index"))
                {
                    Assert.IsNotNull(result.Model);
                    HomePlanModel objModel = (HomePlanModel)result.Model;
                    Assert.AreNotEqual(0, objModel.PlanId);
                }

                Assert.IsNotNull(result.ViewBag.ActiveMenu);
            }
        }
        #endregion

        #region Home page with plan id  
        /// <summary>
        /// To check to retrieve Home view with plan id
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>21Jan2015</createddate>
        [TestMethod]
        public void Get_Home_View_For_Home_Screen_With_PlanId()
        {
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            HomeController objHomeController = new HomeController();
            int planId = DataHelper.GetPlanId();
            var result = objHomeController.Index(Enums.ActiveMenu.Home, planId) as ViewResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                if (!(result.ViewName.Equals("Index") || result.ViewName.Equals("PlanSelector")))
                {
                    Assert.Fail();
                }
                else if (result.ViewName.Equals("Index"))
                {
                    Assert.IsNotNull(result.Model);
                    HomePlanModel objModel = (HomePlanModel)result.Model;
                    Assert.AreNotEqual(0, objModel.PlanId);
                }

                Assert.IsNotNull(result.ViewBag.ActiveMenu);
            }
        }
        #endregion

        #region Plan page with plan id
        /// <summary>
        /// To check to retrieve Plan view with plan id
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>21Jan2015</createddate>
        [TestMethod]
        public void Get_Home_View_For_Plan_Screen_With_PlanId()
        {
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            HomeController objHomeController = new HomeController();
            int planId = DataHelper.GetPlanId();
            var result = objHomeController.Index(Enums.ActiveMenu.Plan, planId) as ViewResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                if (!(result.ViewName.Equals("Index") || result.ViewName.Equals("PlanSelector")))
                {
                    Assert.Fail();
                }
                else if (result.ViewName.Equals("Index"))
                {
                    Assert.IsNotNull(result.Model);
                    HomePlanModel objModel = (HomePlanModel)result.Model;
                    Assert.AreNotEqual(0, objModel.PlanId);
                }

                Assert.IsNotNull(result.ViewBag.ActiveMenu);
            }
        }
        #endregion

        #region Load calendar for home screen view by tactic
        /// <summary>
        /// To check to retrieve calendar data for Home Screen viewby tactic
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>21Jan2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_ViewBy_Tactic()
        {
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            HomeController objHomeController = new HomeController();
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(Sessions.User.UserId);
            var result = objHomeController.GetViewControlDetail(ViewBy, CommaSeparatedPlanId, DateTime.Now.Year.ToString(), "", CommaSeparatedCustomFields, "", Enums.ActiveMenu.Home.ToString(), false) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result.Data);
                Assert.IsNotNull(result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for home screen view by custom field
        /// <summary>
        /// To check to retrieve calendar data for Home Screen viewby custom field
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>21Jan2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_ViewBy_CustomField()
        {
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            HomeController objHomeController = new HomeController();
            string ViewBy = PlanGanttTypes.Custom.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(Sessions.User.UserId);
            var result = objHomeController.GetViewControlDetail(ViewBy, CommaSeparatedPlanId, DateTime.Now.Year.ToString(), "", CommaSeparatedCustomFields, "", Enums.ActiveMenu.Home.ToString(), false) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result.Data);
                Assert.IsNotNull(result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for plan screen view by tactic
        /// <summary>
        /// To check to retrieve calendar data for Plan Screen viewby tactic
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>21Jan2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_ViewBy_Tactic()
        {
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            HomeController objHomeController = new HomeController();
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(Sessions.User.UserId);
            var result = objHomeController.GetViewControlDetail(ViewBy, CommaSeparatedPlanId, DateTime.Now.Year.ToString(), "", CommaSeparatedCustomFields, "", Enums.ActiveMenu.Plan.ToString(), false) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result.Data);
                Assert.IsNotNull(result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for plan screen view by custom field
        /// <summary>
        /// To check to retrieve calendar data for Plan Screen viewby custom field
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>21Jan2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_ViewBy_CustomField()
        {
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            HomeController objHomeController = new HomeController();
            string ViewBy = PlanGanttTypes.Custom.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(Sessions.User.UserId);
            var result = objHomeController.GetViewControlDetail(ViewBy, CommaSeparatedPlanId, DateTime.Now.Year.ToString(), "", CommaSeparatedCustomFields, "", Enums.ActiveMenu.Plan.ToString(), false) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result.Data);
                Assert.IsNotNull(result.GetValue("taskData"));
            }
        }
        #endregion

        #endregion
    }
}

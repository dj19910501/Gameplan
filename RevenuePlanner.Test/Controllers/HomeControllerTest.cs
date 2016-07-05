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
using System.Web.Routing;

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
            Console.WriteLine("To check to retrieve Home view with no parameters.\n");
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
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewBag.ActiveMenu);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.ViewBag.ActiveMenu);
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
            Console.WriteLine("To check to retrieve Home view with plan id.\n");
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
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewBag.ActiveMenu);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.ViewBag.ActiveMenu);
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
            Console.WriteLine("To check to retrieve Plan view with plan id.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            HomeController objHomeController = new HomeController();
            int planId = DataHelper.GetPlanId();
            var SetOFLastViews = db.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            Common.PlanUserSavedViews = SetOFLastViews;
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
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewBag.ActiveMenu);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.ViewBag.ActiveMenu);
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
            Console.WriteLine("To check to retrieve calendar data for Home Screen viewby tactic.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();

            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Home.ToString(), false, "", "") as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
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
            Console.WriteLine("To check to retrieve calendar data for Home Screen viewby custom field.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            string ViewBy = PlanGanttTypes.Custom.ToString();
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Home.ToString(), false, "", "") as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for home screen view by Status
        /// <summary>
        /// To check to retrieve calendar data for Home Screen viewby Status
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_ViewBy_Status()
        {
            Console.WriteLine("To check to retrieve calendar data for Home Screen viewby Status.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            string ViewBy = PlanGanttTypes.Status.ToString();
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Home.ToString(), false, "", "");
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for home screen status view by tactic
        /// <summary>
        /// To check to retrieve calendar data for Home Screen  status viewby tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_status_ViewBy_Tactic()
        {
            Console.WriteLine("To check to retrieve calendar data for Home Screen  status viewby tactic.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);

            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Home.ToString(), false, "", Status) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for home screen status view by custom field
        /// <summary>
        /// To check to retrieve calendar data for Home Screen  status viewby custom field
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_status_ViewBy_Custom()
        {
            Console.WriteLine("To check to retrieve calendar data for Home Screen  status viewby custom field.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);

            //// Call index method
            string ViewBy = PlanGanttTypes.Custom.ToString();
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Home.ToString(), false, "", Status) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for home screen TacticType view by tactic
        /// <summary>
        /// To check to retrieve calendar data for Home Screen  TacticType viewby tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_TacticType_ViewBy_Tactic()
        {
            Console.WriteLine("To check to retrieve calendar data for Home Screen  TacticType viewby tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();


            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Home.ToString(), false, tactictypeids, "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }

        #endregion

        #region Load calendar for home screen TacticType view by Custom field
        /// <summary>
        /// To check to retrieve calendar data for Home Screen  TacticType viewby Custom field
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_TacticType_ViewBy_Custom()
        {
            Console.WriteLine("To check to retrieve calendar data for Home Screen  TacticType viewby Custom field.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();


            //// Call index method
            string ViewBy = PlanGanttTypes.Custom.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Home.ToString(), false, tactictypeids, "") as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for home screen Owner view by tactic
        /// <summary>
        /// To check to retrieve calendar data for Home Screen  Owner viewby tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_Owner_ViewBy_Tactic()
        {
            Console.WriteLine("To check to retrieve calendar data for Home Screen  Owner viewby tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();


            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);

            var result = LoadFunction(ViewBy, Ownerids, Enums.ActiveMenu.Home.ToString(), false, "", "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }

        #endregion

        #region Load calendar for home screen Owner view by Custom field
        /// <summary>
        /// To check to retrieve calendar data for Home Screen  Owner viewby Custom field
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_Owner_ViewBy_Custom()
        {
            Console.WriteLine("To check to retrieve calendar data for Home Screen  Owner viewby Custom field.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();


            //// Call index method
            string ViewBy = PlanGanttTypes.Custom.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);

            var result = LoadFunction(ViewBy, Ownerids, Enums.ActiveMenu.Home.ToString(), false, "", "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }

        #endregion

        #region Load calendar for home screen TacticType and Status view by Custom field
        /// <summary>
        /// To check to retrieve calendar data for Home Screen  TacticType  and Status  viewby Custom field
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_TacticType_Status_ViewBy_Custom()
        {
            Console.WriteLine("To check to retrieve calendar data for Home Screen  TacticType  and Status  viewby Custom field.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);


            //// Call index method
            string ViewBy = PlanGanttTypes.Custom.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Home.ToString(), false, tactictypeids, Status) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for home screen TacticType and status view by tactic
        /// <summary>
        /// To check to retrieve calendar data for Home Screen  TacticType and status viewby tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_TacticType_Status_ViewBy_Tactic()
        {
            Console.WriteLine("To check to retrieve calendar data for Home Screen  TacticType and status viewby tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);

            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Home.ToString(), false, tactictypeids, Status) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for home screen TacticType ,Status and Owner view by tactic
        /// <summary>
        /// To check to retrieve calendar data for Home Screen  TacticType ,Status and Owner  viewby tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_TacticType_Status__Owner_ViewBy_Tactic()
        {
            Console.WriteLine("To check to retrieve calendar data for Home Screen  TacticType ,Status and Owner  viewby tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);


            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);


            var result = LoadFunction(ViewBy, Ownerids, Enums.ActiveMenu.Home.ToString(), false, tactictypeids, Status) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for home screen TacticType ,Status and Owner view by Custom field
        /// <summary>
        /// To check to retrieve calendar data for Home Screen  TacticType ,Status and Owner  viewby Custom field
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Home_Screen_TacticType_Status__Owner_ViewBy_Custom()
        {
            Console.WriteLine("To check to retrieve calendar data for Home Screen  TacticType ,Status and Owner  viewby Custom field.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);


            //// Call index method
            string ViewBy = PlanGanttTypes.Custom.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);


            var result = LoadFunction(ViewBy, Ownerids, Enums.ActiveMenu.Home.ToString(), false, tactictypeids, Status) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
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
            Console.WriteLine("To check to retrieve calendar data for Plan Screen viewby tactic.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Plan.ToString(), false, "", "") as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
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
            Console.WriteLine("To check to retrieve calendar data for Plan Screen viewby custom field.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            string ViewBy = PlanGanttTypes.Custom.ToString();
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Plan.ToString(), false, "", "") as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for Plan screen view by Status
        /// <summary>
        /// To check to retrieve calendar data for plan Screen viewby Status
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_ViewBy_Status()
        {
            Console.WriteLine("To check to retrieve calendar data for plan Screen viewby Status.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            string ViewBy = PlanGanttTypes.Status.ToString();
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Plan.ToString(), false, "", "") as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for plan screen status view by tactic
        /// <summary>
        /// To check to retrieve calendar data for plan Screen  status viewby tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_status_ViewBy_Tactic()
        {
            Console.WriteLine("To check to retrieve calendar data for plan Screen  status viewby tactic.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);

            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Plan.ToString(), false, "", Status) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for plan screen status view by custom field
        /// <summary>
        /// To check to retrieve calendar data for plan Screen  status viewby custom field
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_status_ViewBy_Custom()
        {
            Console.WriteLine("To check to retrieve calendar data for plan Screen  status viewby custom field.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);

            //// Call index method
            string ViewBy = PlanGanttTypes.Custom.ToString();
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Plan.ToString(), false, "", Status) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for plan screen TacticType view by tactic
        /// <summary>
        /// To check to retrieve calendar data for plan Screen  TacticType viewby tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_TacticType_ViewBy_Tactic()
        {
            Console.WriteLine("To check to retrieve calendar data for plan Screen  TacticType viewby tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();


            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Plan.ToString(), false, tactictypeids, "") as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for plan screen TacticType view by Custom field
        /// <summary>
        /// To check to retrieve calendar data for plan Screen  TacticType viewby Custom field
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_TacticType_ViewBy_Custom()
        {
            Console.WriteLine("To check to retrieve calendar data for plan Screen  TacticType viewby Custom field.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();


            //// Call index method
            string ViewBy = PlanGanttTypes.Custom.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Plan.ToString(), false, tactictypeids, "") as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for plan screen Owner view by tactic
        /// <summary>
        /// To check to retrieve calendar data for plan Screen  Owner viewby tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_Owner_ViewBy_Tactic()
        {
            Console.WriteLine("To check to retrieve calendar data for plan Screen  Owner viewby tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();


            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);

            var result = LoadFunction(ViewBy, Ownerids, Enums.ActiveMenu.Plan.ToString(), false, "", "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }

        #endregion

        #region Load calendar for plan screen Owner view by Custom field
        /// <summary>
        /// To check to retrieve calendar data for plan Screen  Owner viewby Custom field
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_Owner_ViewBy_Custom()
        {
            Console.WriteLine("To check to retrieve calendar data for plan Screen  Owner viewby Custom field.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();


            //// Call index method
            string ViewBy = PlanGanttTypes.Custom.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);

            var result = LoadFunction(ViewBy, Ownerids, Enums.ActiveMenu.Plan.ToString(), false, "", "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }

        #endregion

        #region Load calendar for plan screen TacticType and Status view by Custom field
        /// <summary>
        /// To check to retrieve calendar data for plan Screen  TacticType  and Status  viewby Custom field
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_TacticType_Status_ViewBy_Custom()
        {
            Console.WriteLine("To check to retrieve calendar data for plan Screen  TacticType  and Status  viewby Custom field.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);


            //// Call index method
            string ViewBy = PlanGanttTypes.Custom.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Plan.ToString(), false, tactictypeids, Status) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for plan screen TacticType and status view by tactic
        /// <summary>
        /// To check to retrieve calendar data for plan Screen  TacticType and status viewby tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_TacticType_Status_ViewBy_Tactic()
        {
            Console.WriteLine("To check to retrieve calendar data for plan Screen  TacticType and status viewby tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);

            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Plan.ToString(), false, tactictypeids, Status) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for plan screen TacticType ,Status and Owner view by tactic
        /// <summary>
        /// To check to retrieve calendar data for plan Screen  TacticType ,Status and Owner  viewby tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_TacticType_Status__Owner_ViewBy_Tactic()
        {
            Console.WriteLine("To check to retrieve calendar data for plan Screen  TacticType ,Status and Owner  viewby tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);


            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);


            var result = LoadFunction(ViewBy, Ownerids, Enums.ActiveMenu.Plan.ToString(), false, tactictypeids, Status) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Load calendar for plan screen TacticType ,Status and Owner view by Custom field
        /// <summary>
        /// To check to retrieve calendar data for plan Screen  TacticType ,Status and Owner  viewby Custom field
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>26June2015</createddate>
        [TestMethod]
        public void Load_Calendar_For_Plan_Screen_TacticType_Status__Owner_ViewBy_Custom()
        {
            Console.WriteLine("To check to retrieve calendar data for plan Screen  TacticType ,Status and Owner  viewby Custom field.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);


            //// Call index method
            string ViewBy = PlanGanttTypes.Custom.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);


            var result = LoadFunction(ViewBy, Ownerids, Enums.ActiveMenu.Plan.ToString(), false, tactictypeids, Status) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("taskData"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.GetValue("taskData"));
            }
        }
        #endregion

        #region Common Function

        public JsonResult LoadFunction(string ViewBy, string OwnerIds, string Activemenu, bool getViewByList, string Tactictypeids, string Statusids)
        {
            Console.WriteLine("Common function.\n");
            HomeController objHomeController = new HomeController();
            string CommaSeparatedPlanId = "";
            string Year = DataHelper.GetYear();
            if (Activemenu == Enums.ActiveMenu.Home.ToString())
            {
                CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            }
            else
            {
                CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();
            }
            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(Sessions.User.UserId);

            var result = objHomeController.GetViewControlDetail(ViewBy, CommaSeparatedPlanId, Year, CommaSeparatedCustomFields, OwnerIds, Activemenu, getViewByList, Tactictypeids, Statusids, true) as Task<JsonResult>;
            return new JsonResult();
        }

        #endregion
        #endregion

        #region PL#1144 Multi select: Add actuals page changes for custom fields

        #region Add Actual view
        /// <summary>
        /// To check to retrieve Add actual view
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>23Jan2015</createddate>
        [TestMethod]
        public void Get_Add_Actual_View()
        {
            Console.WriteLine("To check to retrieve Add actual view.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call AddActual method
            PlanController objHomeController = new PlanController();
            Sessions.PlanId = DataHelper.GetPlanId();
            var SetOFLastViews = db.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            Common.PlanUserSavedViews = SetOFLastViews;
            var result = objHomeController.AddActual(Convert.ToInt32(Sessions.PlanId)) as ViewResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.AreEqual("AddActual", result.ViewName);

                Assert.IsNotNull(result.Model);
                HomePlanModel objModel = (HomePlanModel)result.Model;
                Assert.IsNotNull(objModel);

                Assert.IsNotNull(result.ViewBag.IsPlanEditable);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewBag.IsPlanEditable);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.ViewBag.IsPlanEditable);
            }
        }
        #endregion

        #region Add actual tactics with no filter paramater for OpenTactic tab
        /// <summary>
        /// To check to retrieve add actual tactics with no filter paramters for open tactic tab
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>23Jan2015</createddate>
        [TestMethod]
        public void Get_Add_Actual_Tactic_With_No_Filter_Parameter_For_OpenTactic_Tab()
        {
            Console.WriteLine("To check to retrieve add actual tactics with no filter paramters for open tactic tab.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call AddActual method
            HomeController objHomeController = new HomeController();
            Sessions.PlanId = DataHelper.GetPlanId();
            int Status = 0; // Open tactic
            var result = objHomeController.GetActualTactic(Status, string.Empty, string.Empty, string.Empty, Convert.ToInt32(Sessions.PlanId)) as JsonResult;

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Data);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }
        }
        #endregion

        #region Add actual tactics with no filter paramater for AllTactic tab
        /// <summary>
        /// To check to retrieve add actual tactics with no filter paramters for all tactic tab
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>23Jan2015</createddate>
        [TestMethod]
        public void Get_Add_Actual_Tactic_With_No_Filter_Parameter_For_AllTactic_Tab()
        {
            Console.WriteLine("To check to retrieve add actual tactics with no filter paramters for all tactic tab.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call AddActual method
            HomeController objHomeController = new HomeController();
            Sessions.PlanId = DataHelper.GetPlanId();
            int Status = 1; // All tactic
            var result = objHomeController.GetActualTactic(Status, string.Empty, string.Empty, string.Empty, Convert.ToInt32(Sessions.PlanId)) as JsonResult;

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Data);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }
        }
        #endregion

        #region Add actual tactics with filter paramaters for OpenTactic tab
        /// <summary>
        /// To check to retrieve add actual tactics with filter paramters for open tactic tab
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>23Jan2015</createddate>
        [TestMethod]
        public void Get_Add_Actual_Tactic_With_Filter_Parameters_For_OpenTactic_Tab()
        {
            Console.WriteLine("To check to retrieve add actual tactics with filter paramters for open tactic tab.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call AddActual method
            HomeController objHomeController = new HomeController();
            Sessions.PlanId = DataHelper.GetPlanId();
            int Status = 0; // Open tactic
            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(Sessions.User.UserId);
            var result = objHomeController.GetActualTactic(Status, string.Empty, CommaSeparatedCustomFields, string.Empty, Convert.ToInt32(Sessions.PlanId)) as JsonResult;

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Data);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }
        }
        #endregion

        #region Add actual tactics with filter paramaters for AllTactic tab
        /// <summary>
        /// To check to retrieve add actual tactics with filter paramters for all tactic tab
        /// </summary>
        /// <auther>Sohel Pathan</auther>
        /// <createddate>23Jan2015</createddate>
        [TestMethod]
        public void Get_Add_Actual_Tactic_With_Filter_Parameters_For_AllTactic_Tab()
        {
            Console.WriteLine("To check to retrieve add actual tactics with filter paramters for all tactic tab.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call AddActual method
            HomeController objHomeController = new HomeController();
            Sessions.PlanId = DataHelper.GetPlanId();
            int Status = 1; // All tactic
            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(Sessions.User.UserId);
            var result = objHomeController.GetActualTactic(Status, string.Empty, CommaSeparatedCustomFields, string.Empty, Convert.ToInt32(Sessions.PlanId)) as JsonResult;

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Data);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }
        }
        #endregion

        #endregion

        #region HeaderSection

        #region Get Header Data
        [TestMethod]
        public void Get_HeaderData_With_PlanId()
        {
            Console.WriteLine("To check to retrieve Header Data.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            PlanController objPlanController = new PlanController();
            int planId = DataHelper.GetPlanId();
            var result = objPlanController.GetPlanByPlanID(planId) as Task<JsonResult>;

            if (result != null)
            {
                // Json result data should not be null
                Assert.IsNotNull(result.Status);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Status);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Status);
            }
        }

        [TestMethod]
        public void Get_HeaderData_With_MultiplePlanIds()
        {
            Console.WriteLine("To check to retrieve Header Data with multiple planids.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            PlanController objPlanController = new PlanController();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            string Year = DataHelper.GetYear();
            int planId = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(planId);
            Common.PlanUserSavedViews = db.Plan_UserSavedViews.Where(t => t.Userid == Sessions.User.UserId).ToList();
            var result = objPlanController.GetPlanByMultiplePlanIDs(CommaSeparatedPlanId, Enums.ActiveMenu.Home.ToString(), Year) as JsonResult;

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Data);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }
        #endregion

        #region Get ActivityDistribution Data
        [TestMethod]
        public void Get_ActivityDistributionData()
        {
            Console.WriteLine("To check to retrieve Header Data with Activity distribution data.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            HomeController objHomeController = new HomeController();
            int planId = DataHelper.GetPlanId();
            string Year = DataHelper.GetYear();
            var result = objHomeController.GetNumberOfActivityPerMonth(planId.ToString(), Year, false) as Task<JsonResult>;

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Status);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Status);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Status);
            }
        }

        [TestMethod]
        public void Get_ActivityDistributionData_WithMultiplePlans()
        {
            Console.WriteLine("To check to retrieve Header Data with Activity distribution data with multiple planids.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            HomeController objHomeController = new HomeController();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            string Year = DataHelper.GetYear();
            var result = objHomeController.GetNumberOfActivityPerMonth(CommaSeparatedPlanId, Year, true);

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Status);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Status);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Status);
            }
        }
        #endregion

        #endregion

        #region --Saving and rendering Last accessed data of Views---

        /// <summary>
        /// To Save last set data
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>25Nov2015</createddate>
        [TestMethod]
        public void SaveLastSetOfViews()
        {
            Console.WriteLine("To Save last set data.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();

            string ViewBy = PlanGanttTypes.Tactic.ToString();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);

            var UserID = Sessions.User.UserId;

            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(UserID);

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);

            var result = objHomeController.SaveLastSetofViews(CommaSeparatedPlanId, CommaSeparatedCustomFields, Ownerids, tactictypeids, Status, "", "", "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.GetValue("taskData"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }
        }

        /// <summary>
        /// To Save last set data with empty planid
        /// </summary>
        /// <auther>Maitri Gandhi</auther>
        /// <createddate>08Jan2015</createddate>
        [TestMethod]
        public void SaveLastSetOfViews_With_EmptyValues()
        {
            Console.WriteLine("To Save last set data with empty planid.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            Common.PlanUserSavedViews = db.Plan_UserSavedViews.Where(t => t.Userid == Sessions.User.UserId).ToList();
            var result = objHomeController.SaveLastSetofViews("", "", "", "", "", "", "", "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName                
                Assert.AreEqual(true, result.GetValue("isSuccess"));
                Assert.AreEqual("", result.GetValue("ViewName"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }
        }

        /// <summary>
        /// To Save last set data with null CustomFields and tactictypeid
        /// </summary>
        /// <auther>Maitri Gandhi</auther>
        /// <createddate>08Jan2015</createddate>
        [TestMethod]
        public void SaveLastSetOfViews_With_Null_CustomFields_Tactictypeid()
        {
            Console.WriteLine("To Save last set data with null CustomFields and tactictypeid.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();

            var result = objHomeController.SaveLastSetofViews(null, null, null, null, null, null, null, null) as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.AreEqual(null, result.GetValue("ViewName"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("ViewName"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }

        /// <summary>
        /// To Render last set of view
        /// </summary>
        /// <auther>Maitri Gandhi</auther>
        /// <createddate>08Jan2016</createddate>
        [TestMethod]
        public void Render_LastSetofViews()
        {
            Console.WriteLine("To Render last set of view.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();

            var result = objHomeController.LastSetOfViews() as JsonResult;
            //// ViewResult shoud not be null and should match with viewName

            if (result != null)
            {
                Assert.IsNotNull(result);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }

        /// <summary>
        /// To Render last set of view
        /// </summary>
        /// <auther>Maitri Gandhi</auther>
        /// <createddate>09Jan2016</createddate>
        [TestMethod]
        public void SaveDefaultPreset()
        {
            Console.WriteLine("To Render last set of view.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();

            var SavedPresetNames = db.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).Select(view => view).ToList();
            List<Preset> PresetList = (from item in SavedPresetNames
                                       where item.ViewName != null
                                       select new Preset
                                       {
                                           Id = Convert.ToString(item.Id),
                                           Name = item.ViewName,
                                           IsDefaultPreset = item.IsDefaultPreset
                                       }).ToList();
            string presetName = string.Empty;
            if (PresetList.Count() == 0)
                presetName = "TestPreset";
            else
                presetName = PresetList.FirstOrDefault().Name.ToString();
            var result = objHomeController.SaveDefaultPreset(presetName) as JsonResult;
            if (result != null)
            {
                Assert.AreEqual(true, result.GetValue("isSuccess"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("isSuccess"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }

        /// <summary>
        /// To Set Filter Preset Name
        /// </summary>
        /// <auther>Maitri Gandhi</auther>
        /// <createddate>09Jan2016</createddate>
        [TestMethod]
        public void SetFilterPresetName()
        {
            Console.WriteLine("To Set Filter Preset Name.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();

            var SavedPresetNames = db.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).Select(view => view).ToList();
            List<Preset> PresetList = (from item in SavedPresetNames
                                       where item.ViewName != null
                                       select new Preset
                                       {
                                           Id = Convert.ToString(item.Id),
                                           Name = item.ViewName,
                                           IsDefaultPreset = item.IsDefaultPreset
                                       }).ToList();
            string presetName = string.Empty;
            if (PresetList.Count() == 0)
                presetName = "TestPreset";
            else
                presetName = PresetList.FirstOrDefault().Name.ToString();
            var result = objHomeController.SetFilterPresetName(presetName) as JsonResult;
            if (result != null)
            {
                Assert.AreEqual(true, result.GetValue("isSuccess"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("isSuccess"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }


        #region --Delete Preset Data---
        /// <summary>
        /// To delete Preset data
        /// </summary>
        /// <auther>Maitri Gandhi</auther>
        /// <createddate>09Jan2016</createddate>
        [TestMethod]
        public void DeletePreset()
        {
            Console.WriteLine("To delete Preset data.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            Common.PlanUserSavedViews = db.Plan_UserSavedViews.Where(t => t.Userid == Sessions.User.UserId).ToList();
            var result = objHomeController.DeletePreset("Test") as JsonResult;
            if (result != null)
            {
                Assert.AreEqual(true, result.GetValue("isSuccess"));
                Assert.AreEqual("Preset Test deleted successfully", result.GetValue("msg")); //Modified by Maitri Gandhi on 28/4/2016 for #2136
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("msg"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }

        /// <summary>
        /// To delete Preset data with empty PrestName
        /// </summary>
        /// <auther>Maitri Gandhi</auther>
        /// <createddate>09Jan2016</createddate>
        [TestMethod]
        public void DeletePreset_empty_PrestName()
        {
            Console.WriteLine("delete Preset data with empty PrestName.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            var result = objHomeController.DeletePreset("") as JsonResult;
            if (result != null)
            {
                Assert.AreEqual(false, result.GetValue("isSuccess"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("isSuccess"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }

        /// <summary>
        /// To delete Preset data with null PrestName
        /// </summary>
        /// <auther>Maitri Gandhi</auther>
        /// <createddate>09Jan2016</createddate>
        [TestMethod]
        public void DeletePreset_null_PrestName()
        {
            Console.WriteLine("To delete Preset data with null PrestName.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            var result = objHomeController.DeletePreset(null) as JsonResult;
            if (result != null)
            {
                Assert.AreEqual(false, result.GetValue("isSuccess"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("isSuccess"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }
        #endregion

        #endregion

        #region --Get Header Data for HoneyComb Pdf---
        /// <summary>
        /// To Get Header Data for HoneyComb Pdf
        /// </summary>
        /// <auther>Maitri Gandhi</auther>
        /// <createddate>09Jan2016</createddate>
        [TestMethod]
        public void GetHeaderDataforHoneycombPDF()
        {
            Console.WriteLine("To Get Header Data for HoneyComb Pdf.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();

            string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            var result = objHomeController.GetHeaderDataforHoneycombPDF(tactictypeids, "2016");
            if (result != null)
            {
                Assert.AreEqual(0, result.GetValue("TotalCount"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("TotalCount"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }

        /// <summary>
        /// To Get Header Data for HoneyComb Pdf with empty Tactic Id
        /// </summary>
        /// <auther>Maitri Gandhi</auther>
        /// <createddate>09Jan2016</createddate>
        [TestMethod]
        public void GetHeaderDataforHoneycombPDF_With_Empty_TacticId()
        {
            Console.WriteLine("To Get Header Data for HoneyComb Pdf with empty Tactic Id.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();

            var result = objHomeController.GetHeaderDataforHoneycombPDF("", "2016");
            if (result != null)
            {
                Assert.AreEqual(0, result.GetValue("TotalCount"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("TotalCount"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }

        /// <summary>
        /// To Get Header Data for HoneyComb Pdf with null Tactic Id
        /// </summary>
        /// <auther>Maitri Gandhi</auther>
        /// <createddate>09Jan2016</createddate>
        [TestMethod]
        public void GetHeaderDataforHoneycombPDF_With_Null_TacticId()
        {
            Console.WriteLine("To Get Header Data for HoneyComb Pdf with null Tactic Id.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();

            var result = objHomeController.GetHeaderDataforHoneycombPDF(null, "2016");
            if (result != null)
            {
                Assert.AreEqual(0, result.GetValue("TotalCount"));
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("TotalCount"));
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }

        #endregion

        #region Get TacticType List For Filter
        /// <summary>
        /// To check to Get TacticType List For Filter
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_TacticType_List_For_Filter()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get TacticType List For Filter.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);
            objHomeController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objHomeController.Url = new UrlHelper(
    new RequestContext(
        objHomeController.HttpContext, new RouteData()
    ),
    routes
);
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
          
            var result = objHomeController.GetTacticTypeListForFilter(PlanId.ToString()) as Task<JsonResult>;

            if (result.Result != null)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Result.Data);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Set Session For Plan
        /// <summary>
        /// To check to Set Session For Plan
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Set_Session_For_Plan()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Set Session For Plan.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);
            objHomeController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objHomeController.Url = new UrlHelper(
    new RequestContext(
        objHomeController.HttpContext, new RouteData()
    ),
    routes
);
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;

            var result = objHomeController.SetSessionPlan(PlanId.ToString()) as JsonResult;

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

        #region Bind Upcoming Activites Values
        /// <summary>
        /// To check to Bind Upcoming Activites Values
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Bind_Upcoming_Activites_Values()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Bind Upcoming Activites Values.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);
            objHomeController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objHomeController.Url = new UrlHelper(
    new RequestContext(
        objHomeController.HttpContext, new RouteData()
    ),
    routes
);
            var TaskData  = DataHelper.GetPlan(Sessions.User.ClientId);
            int PlanId = TaskData.PlanId;
            Sessions.PlanId = PlanId;
            string Year = TaskData.Year;
            var result = objHomeController.BindUpcomingActivitesValues(PlanId.ToString(),Year) as JsonResult;

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

        #region Get Custom Attributes for Plan
        /// <summary>
        /// To check to Get Custom Attributes for Plan
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Custom_Attributes()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Custom Attributes for Plan.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);


            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
           
            var result = objHomeController.GetCustomAttributes() as JsonResult;

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

        #region Get Plans
        /// <summary>
        /// To check to Get Plans for Home Screen.
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Plans_Home()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Plans for Home Screen.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);


            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            string ActiveMenu = Enums.ActiveMenu.Home.ToString();
            var result = objHomeController.GetPlans(ActiveMenu) as JsonResult;

            if (result.Data != null)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }

        /// <summary>
        /// To check to Get Plans for Plan Screen.
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Plans_Plan()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Plans for Plan Screen.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);


            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            string ActiveMenu = Enums.ActiveMenu.Plan.ToString();
            var result = objHomeController.GetPlans(ActiveMenu) as JsonResult;

            if (result.Data != null)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }

        /// <summary>
        /// To check to Get Plans for Report Screen.
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Plans_Report()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Plans for Report Screen.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);


            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            string ActiveMenu = Enums.ActiveMenu.Report.ToString();
            var result = objHomeController.GetPlans(ActiveMenu) as JsonResult;

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

        #region Check User Id
        /// <summary>
        /// To check User Id
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Check_UserId()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Check User Id.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);


            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            string UserId = Sessions.User.UserId.ToString();
            var result = objHomeController.CheckUserId(UserId) as JsonResult;

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

        #region Get Owner List For Filter
        /// <summary>
        /// To Get Owner List For Filter
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Owner_List_For_Filter()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Owner List For Filter.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);


            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            string UserId = Sessions.User.UserId.ToString();
            string ActiveMenu = Enums.ActiveMenu.Home.ToString();
            string viewBy = Enums.EntityType.Tactic.ToString();
            var result = objHomeController.GetOwnerListForFilter(PlanId.ToString(),viewBy,ActiveMenu) as Task<JsonResult>;

            if (result != null)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Load Change Log
        /// <summary>
        /// To Get Load Change Log
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Load_Change_Log()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Load Change Log.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);


            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            
            var result = objHomeController.LoadChangeLog(PlanId) as PartialViewResult;

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

        #region Get Number Of Activity Per Month
        /// <summary>
        /// To Get Number Of Activity Per Month.
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Number_Of_Activity_Per_Month()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Number Of Activity Per Month.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);

            var TaskData = DataHelper.GetPlan(Sessions.User.ClientId);
            int PlanId = TaskData.PlanId;
            Sessions.PlanId = PlanId;
            string strParam = TaskData.Year.ToString();
            bool isMultiyear = false;

            var result = objHomeController.GetNumberOfActivityPerMonthPer(PlanId.ToString(),strParam,isMultiyear) as Task<JsonResult>;

            if (result != null)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Get Default URL
        /// <summary>
        /// To Get Default URL.
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_default_URL()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Default URL.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);
            objHomeController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            
            var result = objHomeController.Homezero() as ViewResult;

            if (result != null)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewData.Values);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Get Current Plan Permission Detail
        /// <summary>
        /// To Get Current Plan Permission Detail.
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Current_Plan_Permission_Detail()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Current Plan Permission Detail.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);
            objHomeController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;

            var result = objHomeController.GetCurrentPlanPermissionDetail(PlanId) as JsonResult;

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

        #region Get Plan Data for Home Screen
        /// <summary>
        /// To Get Plan Data for Home Screen.
        /// <author>Rahul Shah</author>
        /// <createddate>04Jul2016</createddate>
        /// </summary>
        [TestMethod]
        public void Home_Plan()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Plan Data for Home Screen.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            HomeController objHomeController = new HomeController();
            objHomeController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objHomeController);
            objHomeController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            var TaskData = DataHelper.GetPlan(Sessions.User.ClientId);
            int PlanId = TaskData.PlanId;
            Sessions.PlanId = PlanId;
            string Year = TaskData.Year;

            var result = objHomeController.HomePlan(PlanId.ToString(),Year) as PartialViewResult;

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

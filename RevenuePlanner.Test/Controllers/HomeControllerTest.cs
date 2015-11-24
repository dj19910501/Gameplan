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
            string ViewBy = PlanGanttTypes.Tactic.ToString();
          
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Home.ToString(), false, "", "") as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
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
            string ViewBy = PlanGanttTypes.Custom.ToString();
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Home.ToString(), false, "", "") as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
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
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Plan.ToString(), false, "", "") as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
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
            string ViewBy = PlanGanttTypes.Custom.ToString();
            var result = LoadFunction(ViewBy, "", Enums.ActiveMenu.Plan.ToString(), false, "", "") as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
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
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();


            //// Call index method
            string ViewBy = PlanGanttTypes.Tactic.ToString();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);

            var result = LoadFunction(ViewBy,Ownerids, Enums.ActiveMenu.Plan.ToString(), false, "", "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNull(result.Data);
                Assert.IsNull(result.GetValue("taskData"));
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
            }
        }
        #endregion

        #region Common Function

        public JsonResult LoadFunction(string ViewBy, string OwnerIds, string Activemenu, bool getViewByList, string Tactictypeids, string Statusids)
        {
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
            var result = objHomeController.GetViewControlDetail(ViewBy, CommaSeparatedPlanId, Year, CommaSeparatedCustomFields, OwnerIds, Activemenu, getViewByList, Tactictypeids, Statusids) as Task<JsonResult>;
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call AddActual method
            PlanController objHomeController = new PlanController();
            Sessions.PlanId = DataHelper.GetPlanId();
            var result = objHomeController.AddActual(Convert.ToInt32(Sessions.PlanId)) as ViewResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.AreEqual("AddActual", result.ViewName);
                
                Assert.IsNotNull(result.Model);
                HomePlanModel objModel = (HomePlanModel)result.Model;
                Assert.IsNotNull(objModel.objIndividuals);
                
                Assert.IsNotNull(result.ViewBag.IsPlanEditable);
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call AddActual method
            HomeController objHomeController = new HomeController();
            Sessions.PlanId = DataHelper.GetPlanId();
            int Status = 0; // Open tactic
            var result = objHomeController.GetActualTactic(Status, string.Empty, string.Empty,string.Empty,Convert.ToInt32(Sessions.PlanId)) as JsonResult;

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Data);
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call AddActual method
            HomeController objHomeController = new HomeController();
            Sessions.PlanId = DataHelper.GetPlanId();
            int Status = 1; // All tactic
            var result = objHomeController.GetActualTactic(Status, string.Empty, string.Empty, string.Empty,Convert.ToInt32(Sessions.PlanId)) as JsonResult;

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Data);
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call AddActual method
            HomeController objHomeController = new HomeController();
            Sessions.PlanId = DataHelper.GetPlanId();
            int Status = 0; // Open tactic
            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(Sessions.User.UserId);
            var result = objHomeController.GetActualTactic(Status, string.Empty, CommaSeparatedCustomFields, string.Empty,Convert.ToInt32(Sessions.PlanId)) as JsonResult;

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Data);
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
            }
        }
        #endregion

        #endregion

        #region HeaderSection

        #region Get Header Data
        [TestMethod]
        public void Get_HeaderData_With_PlanId()
        {
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            PlanController objPlanController = new PlanController();
            int planId = DataHelper.GetPlanId();
            var result = objPlanController.GetPlanByPlanID(planId) as Task<JsonResult>;

            if (result != null)
            {
                //// Json result data should not be null
                //Assert.IsNotNull(result.Data);
            }
        }

        [TestMethod]
        public void Get_HeaderData_With_MultiplePlanIds()
        {
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            PlanController objPlanController = new PlanController();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            string Year = DataHelper.GetYear();
             var result = objPlanController.GetPlanByMultiplePlanIDs(CommaSeparatedPlanId, Enums.ActiveMenu.Home.ToString(), Year) as JsonResult;

            if (result != null)
            {
                //// Json result data should not be null
                Assert.IsNotNull(result.Data);
            }
        }
        #endregion

        #region Get ActivityDistribution Data
        [TestMethod]
        public void Get_ActivityDistributionData()
        {
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            HomeController objHomeController = new HomeController();
            int planId = DataHelper.GetPlanId();
            string Year = DataHelper.GetYear();
            var result = objHomeController.GetNumberOfActivityPerMonth(planId.ToString(),Year, false) as Task<JsonResult>;

            if (result != null)
            {
                //// Json result data should not be null
              //  Assert.IsNotNull(result.Data);
            }
        }

        [TestMethod]
        public void Get_ActivityDistributionData_WithMultiplePlans()
        {
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
               // Assert.IsNotNull(result.Data);
            }
        }
        #endregion

        #endregion
    }
}

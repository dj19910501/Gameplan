using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using RevenuePlanner.Models;
using System.Web.Routing;

namespace RevenuePlanner.Test.Controllers
{

    [TestClass]
    public class ReportControllerTest
    {
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
        }
        #region "Overview section of Report"
        MRPEntities db = new MRPEntities();
        #region "GetOverviewData function TestCases"
        #region GetOverviewData section with TimeFrame Empty
        string PlanYear = string.Empty;
        public ReportControllerTest()
        {
            this.PlanYear = DataHelper.GetPlanYear();
        }
        /// <summary>
        /// To check GetOverviewData function with no timeframeOption
        /// This test case return "Object Reference not set to an instance of an Object" error due to Common.objCached.RevenueSparklineChartHeader becomes null.
        /// Otherwise this test case successfully executed.
        /// </summary>
        /// <auther>Viral Kadiya</auther>
        /// <createddate>19Jun2015</createddate>
        [TestMethod]
        public void GetOverviewData_TimeFrame_Empty()
        {
            Console.WriteLine("To check GetOverviewData function with no timeframeOption.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            List<int> lst = new List<int>();
            lst.Add(9775);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            #region "Old Code: Load Cache Data"
            ////Common.objCached.RevenueSparklineChartHeader ="Top {0} by";

            ////string xmlMsgFilePath = HttpContext.Current.Request.ApplicationPath == null ? string.Empty : HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath.Replace("/", "\\") + "\\" + System.Configuration.ConfigurationSettings.AppSettings.Get("XMLCommonMsgFilePath"));
            //string xmlMsgFilePath = Convert.ToString(ConfigurationManager.AppSettings["XMLCommonMsgFilePath"]);
            //Helper.CommonTest objCommon = new Helper.CommonTest();
            //objCommon.loadMsg(xmlMsgFilePath);
            //System.Web.HttpContext.Current.Cache["CommonMsg"] = objCommon;
            //string strXMLPath = "E:/Project/Gameplan/GIT-June-2/Gameplan/RevenuePlanner.Test/bin/Debug/commonmessages.xml";
            //CacheDependency dependency = new CacheDependency(strXMLPath);
            //Message objMessage = new Message();
            //System.Web.HttpContext.Current.Cache.Insert("CommonMsg", objCommon, dependency); 
            #endregion

            //// Call GetOverviewData() function
            ReportController ReportController = new ReportController();

            var result = ReportController.GetOverviewData(string.Empty, Enums.ViewByAllocated.Quarterly.ToString()) as Task<ActionResult>;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Status);
            Assert.AreNotEqual("_Overview", result);
    

        }
        #endregion

        #region GetOverviewData section with Invalid TimeFrame
        /// <summary>
        /// To check GetOverviewData function with InValide value of timeframeOption
        /// This test case return "Object Reference not set to an instance of an Object" error due to Common.objCached.RevenueSparklineChartHeader becomes null.
        /// Otherwise this test case successfully executed.
        /// </summary>
        /// <auther>Viral Kadiya</auther>
        /// <createddate>19Jun2015</createddate>
        [TestMethod]
        public void GetOverviewData_TimeFrame_InValid()
        {
            Console.WriteLine("To check GetOverviewData function with InValide value of timeframeOption.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            List<int> lst = new List<int>();
            lst.Add(9775);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            //Common.objCached.RevenueSparklineChartHeader = "Top {0} by";

            string _InvalidTimeFrame = "InValid";

            //// Call GetOverviewData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetOverviewData(_InvalidTimeFrame, Enums.ViewByAllocated.Quarterly.ToString()) as Task<ActionResult>;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Status);
            Assert.AreNotEqual("_Overview", result);
        }
        #endregion

        #region GetOverviewData section with IsQuarterly Empty
        /// <summary>
        /// To check GetOverviewData function with no timeframeOption
        /// This test case return "Object Reference not set to an instance of an Object" error due to Common.objCached.RevenueSparklineChartHeader becomes null.
        /// Otherwise this test case successfully executed.
        /// </summary>
        /// <auther>Viral Kadiya</auther>
        /// <createddate>19Jun2015</createddate>
        [TestMethod]
        public void GetOverviewData_IsQuarterly_Empty()
        {
            Console.WriteLine("To check GetOverviewData function with no timeframeOption.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            List<int> lst = new List<int>();
            lst.Add(9775);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ///Common.objCached.RevenueSparklineChartHeader = "Top {0} by";
            //// Call GetOverviewData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetOverviewData(PlanYear, string.Empty) as Task<ActionResult>;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Status);
            Assert.AreNotEqual("_Overview", result);
        }
        #endregion

        #region GetOverviewData section with IsQuarterly InValid
        /// <summary>
        /// To check GetOverviewData function with no timeframeOption
        /// This test case return "Object Reference not set to an instance of an Object" error due to Common.objCached.RevenueSparklineChartHeader becomes null.
        /// Otherwise this test case successfully executed.
        /// </summary>
        /// <auther>Viral Kadiya</auther>
        /// <createddate>19Jun2015</createddate>
        [TestMethod]
        public void GetOverviewData_IsQuarterly_InValid()
        {
            Console.WriteLine("To check GetOverviewData function with no timeframeOption.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            List<int> lst = new List<int>();
            lst.Add(9775);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string _InValidIsQuarterly = "InValidQuarterly";
            ///Common.objCached.RevenueSparklineChartHeader = "Top {0} by";
            //// Call GetOverviewData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetOverviewData(PlanYear, _InValidIsQuarterly) as Task<ActionResult>;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Status);
            Assert.AreNotEqual("_Overview", result);
        }
        #endregion
        #endregion

        #endregion

        #region "Revenue section of Report"
        #region "GetRevenueData function TestCases"

        #region GetRevenueData section with No Parameter
        /// <summary>
        /// GetRevenueData section with TimeFrame Empty
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13Aug2015</createddate>
        [TestMethod]
        public void GetRevenueData_No_Parameter()
        {
            Console.WriteLine("GetRevenueData section with TimeFrame Empty.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            var SetOFLastViews = db.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            Common.PlanUserSavedViews = SetOFLastViews;
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();

            var result = ReportController.GetRevenueData(PlanYear) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_Revenue", result.ViewName);
        }
        #endregion

        #region GetRevenueData section with TimeFrame Empty
        /// <summary>
        /// GetRevenueData section with TimeFrame Empty
        /// </summary>
        /// <auther>Viral Kadiya</auther>
        /// <createddate>19Jun2015</createddate>
        [TestMethod]
        public void GetRevenueData_TimeFrame_Empty()
        {
            Console.WriteLine("GetRevenueData section with TimeFrame Empty.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            var SetOFLastViews = db.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            Common.PlanUserSavedViews = SetOFLastViews;

            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetRevenueData(PlanYear, Enums.ViewByAllocated.Quarterly.ToString()) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_Revenue", result.ViewName);
        }
        #endregion

        //Commented by Rahul Shah on 21/11/2015. Because it is negative test cases
        //#region GetRevenueData section with Invalid TimeFrame
        ///// <summary>
        ///// GetRevenueData section with Invalid TimeFrame
        ///// </summary>
        ///// <auther>Viral Kadiya</auther>
        ///// <createddate>19Jun2015</createddate>
        //[TestMethod]
        //public void GetRevenueData_TimeFrame_InValid()
        //{
        //    //// Set session value
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    List<int> lst = new List<int>();
        //    lst.Add(9775);
        //    HttpContext.Current.Session["ReportPlanIds"] = lst;
        //    string _InvalidTimeFrame = "InValid";

        //    //// Call GetRevenueData() function
        //    ReportController ReportController = new ReportController();
        //    var result = ReportController.GetRevenueData(_InvalidTimeFrame, Enums.ViewByAllocated.Quarterly.ToString()) as PartialViewResult;
        //    //// PartialViewResult shoud not be null and should match with Partial viewName
        //    Assert.AreEqual("_Revenue", result.ViewName);
        //}
        //#endregion

        #region GetRevenueData section with IsQuarterly Empty
        /// <summary>
        ///  GetRevenueData section with IsQuarterly Empty
        /// </summary>
        /// <auther>Viral Kadiya</auther>
        /// <createddate>19Jun2015</createddate>
        [TestMethod]
        public void GetRevenueData_IsQuarterly_Empty()
        {
            Console.WriteLine("GetRevenueData section with IsQuarterly Empty.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Set session value
            var SetOFLastViews = db.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            Common.PlanUserSavedViews = SetOFLastViews;
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ///Common.objCached.RevenueSparklineChartHeader = "Top {0} by";
            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetRevenueData(PlanYear, string.Empty) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_Revenue", result.ViewName);
        }
        #endregion

        #region GetRevenueData section with IsQuarterly InValid
        /// <summary>
        /// GetRevenueData section with IsQuarterly InValid
        /// </summary>
        /// <auther>Viral Kadiya</auther>
        /// <createddate>19Jun2015</createddate>
        [TestMethod]
        public void GetRevenueData_IsQuarterly_InValid()
        {
            Console.WriteLine("GetRevenueData section with IsQuarterly InValid.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string _InValidIsQuarterly = "InValidQuarterly";
            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetRevenueData(PlanYear, _InValidIsQuarterly) as PartialViewResult;
            Assert.AreEqual("_Revenue", result.ViewName);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
        }
        #endregion

        #region "GetRevenueToPlanByFilter section with No Parameter"
        /// <summary>
        /// GetRevenueToPlanByFilter No Parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13Aug2015</createddate>
        [TestMethod]
        public void GetRevenueToPlanByFilter_No_Parameter()
        {
            Console.WriteLine("GetRevenueToPlanByFilter No Parameter.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            var SetOFLastViews = db.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            Common.PlanUserSavedViews = SetOFLastViews;
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();
            ReportController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            ReportController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), ReportController);
            var result = ReportController.GetRevenueToPlanByFilter("Campaign", "", "", PlanYear);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_RevenueToPlan", result.ViewName);
        }
        #endregion

        #region "GetRevenueToPlanByFilter section with Empty Parent Type Label"
        /// <summary>
        /// GetRevenueToPlanByFilter No Parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13Aug2015</createddate>
        [TestMethod]
        public void GetRevenueToPlanByFilter_Parentltype_Empty()
        {
            Console.WriteLine("GetRevenueToPlanByFilter No Parameter.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrParent = string.Empty;
            string Isquater = Enums.ViewByAllocated.Quarterly.ToString();
            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0 
            var result = ReportController.GetRevenueToPlanByFilter(StrParent, "Campaign", "0", PlanYear, Isquater, false, "", false, "CampaignDrp", "", 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_RevenueToPlan", result.ViewName);
        }
        #endregion

        #region "GetRevenueToPlanByFilter section with Invalid Parent Type Label"
        /// <summary>
        /// Retrieve data to render view by Invalid ParentLabel 
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13Aug2015</createddate>
        [TestMethod]
        public void GetRevenueToPlanByFilter_Invalid_Parentltype()
        {
            Console.WriteLine("Retrieve data to render view by Invalid ParentLabel.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrParent = "Invalid";
            string Isquater = Enums.ViewByAllocated.Quarterly.ToString();
            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0 
            var result = ReportController.GetRevenueToPlanByFilter(StrParent, "Campaign", "0", PlanYear, Isquater, false, "", false, "CampaignDrp", "", 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_RevenueToPlan", result.ViewName);

        }
        #endregion

        #region "GetRevenueToPlanByFilter section with Campaign as ParentType"
        /// <summary>
        /// Retrieve data to render view by ParentLabel as Campaign 
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13Aug2015</createddate>
        [TestMethod]
        public void GetRevenueToPlanByFilter_By_Campaign()
        {
            Console.WriteLine("Retrieve data to render view by ParentLabel as Campaign.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string Isquater = Enums.ViewByAllocated.Quarterly.ToString();
            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0 
            var result = ReportController.GetRevenueToPlanByFilter(StrParentLabel, "Campaign", "0", PlanYear, Isquater, false, "", false, "CampaignDrp", "", 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_RevenueToPlan", result.ViewName);
        }
        #endregion

        #region "GetRevenueToPlanByFilter section with Campaign as Tactic Custom"
        /// <summary>
        /// Retrieve data to render view by ParentLabel as Tactic Custom 
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13Aug2015</createddate>
        [TestMethod]
        public void GetRevenueToPlanByFilter_By_TacticCustom()
        {
            Console.WriteLine("Retrieve data to render view by ParentLabel as Tactic Custom.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrParentLabel = Enums.PlanEntity.Tactic.ToString();
            string Isquater = Enums.ViewByAllocated.Quarterly.ToString();
            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0 
            var result = ReportController.GetRevenueToPlanByFilter(StrParentLabel, "Campaign", "0", PlanYear, Isquater, false, "", false, "CampaignDrp", "", 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_RevenueToPlan", result.ViewName);
        }
        #endregion

        #region "GetRevenueToPlanByFilter section with Campaign as Program Custom"
        /// <summary>
        /// Retrieve data to render view by ParentLabel as Program Custom 
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13Aug2015</createddate>
        [TestMethod]
        public void GetRevenueToPlanByFilter_By_ProgramCustom()
        {
            Console.WriteLine("Retrieve data to render view by ParentLabel as Program Custom.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrParentLabel = Enums.PlanEntity.Program.ToString();
            string Isquater = Enums.ViewByAllocated.Quarterly.ToString();
            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0 
            var result = ReportController.GetRevenueToPlanByFilter(StrParentLabel, "Campaign", "0", PlanYear, Isquater, false, "", false, "CampaignDrp", "", 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_RevenueToPlan", result.ViewName);
        }
        #endregion

        #region "GetRevenueToPlanByFilter section with Campaign Custom"
        /// <summary>
        /// Retrieve data to render view by ParentLabel as Campaign Custom 
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13Aug2015</createddate>
        [TestMethod]
        public void GetRevenueToPlanByFilter_By_CampaignCustom()
        {
            Console.WriteLine("Retrieve data to render view by ParentLabel as Campaign Custom.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            string published = Convert.ToString(Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value).ToLower();
            //int planId = DataHelper.GetPlanId();
            int planId = 126;
            List<int> lst = new List<int>();
            lst.Add(planId);
            Sessions.User.ClientId = DataHelper.GetClientId(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string Isquater = Enums.ViewByAllocated.Quarterly.ToString();

            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            ReportController.TempData["ReportData"] = tacticStageList;
            //// Call GetRevenueToPlanByFilter() function


            var result = ReportController.GetRevenueToPlanByFilter(StrParentLabel, "Campaign", "0", PlanYear, Isquater, false, "", false, "CampaignDrp", "", 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_RevenueToPlan", result.ViewName);
        }
        #endregion

        #region "GetRevenueToPlanByFilter section with ChildCampaignCustom"
        /// <summary>
        /// Retrieve data to render view by Child as Campaign Custom 
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13Aug2015</createddate>
        [TestMethod]
        public void GetRevenueToPlanByFilter_By_Child_CampaignCustom()
        {
            Console.WriteLine("Retrieve data to render view by Child as Campaign Custom.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            //int planId = DataHelper.GetPlanId();
            int planId = 126;
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string Isquater = Enums.ViewByAllocated.Quarterly.ToString();

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            ReportController.TempData["ReportData"] = tacticStageList;
            //// Call GetRevenueToPlanByFilter() function
            var result = ReportController.GetRevenueToPlanByFilter(StrParentLabel, StrChildLabel, "0", PlanYear, Isquater, false, "", false, "CampaignDrp", "", 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_RevenueToPlan", result.ViewName);
         
        }
        #endregion

        #region "GetRevenueToPlanByFilter section with Empty Quater"
        /// <summary>
        /// Retrieve data to render view by Empty Quater 
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13Aug2015</createddate>
        [TestMethod]
        public void GetRevenueToPlanByFilter_By_Empty_Quater()
        {
            Console.WriteLine("Retrieve data to render view by Empty Quater.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            //int planId = DataHelper.GetPlanId();
            int planId = 126;
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;


            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            ReportController.TempData["ReportData"] = tacticStageList;
            //// Call GetRevenueToPlanByFilter() function
            var result = ReportController.GetRevenueToPlanByFilter(StrParentLabel, StrChildLabel, "0", PlanYear, string.Empty, false, "", false, "CampaignDrp", "", 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_RevenueToPlan", result.ViewName);
        }
        #endregion

        #region "GetRevenueToPlanByFilter section with Invalid Quater"
        /// <summary>
        /// Retrieve data to render view by Invalid Quater 
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13Aug2015</createddate>
        [TestMethod]
        public void GetRevenueToPlanByFilter_By_Invalid_Quater()
        {
            Console.WriteLine("Retrieve data to render view by Invalid Quater.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            //int planId = DataHelper.GetPlanId();
            int planId = 126;
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string invalidQuater = "Invalid";

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            ReportController.TempData["ReportData"] = tacticStageList;
            //// Call GetRevenueToPlanByFilter() function
            var result = ReportController.GetRevenueToPlanByFilter(StrParentLabel, StrChildLabel, "0", PlanYear, invalidQuater, false, "", false, "CampaignDrp", "", 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_RevenueToPlan", result.ViewName);
         
        }
        #endregion

        #region "GetRevenueToPlanByFilter section with Empty BackHeadTitle,DrpChange,marsterCustomField,masterCustomFieldOptionId"
        /// <summary>
        /// Retrieve data to render view by  Empty BackHeadTitle,DrpChange,marsterCustomField,masterCustomFieldOptionId
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13Aug2015</createddate>
        [TestMethod]
        public void GetRevenueToPlanByFilter_By_Empty_BackHeadTitle_DrpChange_marsterCustomField_masterCustomFieldOptionId()
        {
            Console.WriteLine("Retrieve data to render view by  Empty BackHeadTitle,DrpChange,marsterCustomField,masterCustomFieldOptionId.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            //int planId = DataHelper.GetPlanId();
            int planId = 126;
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string IsQuater = Enums.ViewByAllocated.Quarterly.ToString();

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            ReportController.TempData["ReportData"] = tacticStageList;
            //// Call GetRevenueToPlanByFilter() function
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0 
            var result = ReportController.GetRevenueToPlanByFilter(StrParentLabel, StrChildLabel, "0", PlanYear, IsQuater, false, string.Empty, false, string.Empty, string.Empty, 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_RevenueToPlan", result.ViewName);

        }
        #endregion

        #region "SearchSortPaginataionRevenue with No Parameter"
        /// <summary>
        /// Retrieve data to render to check data with empty parameter.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>14aug2015</createddate>
        ///SearchSortPaginataionRevenue_No_Parameter
        [TestMethod]
        public void SearchSortPaginataionRevenue_No_Parameter()
        {
            Console.WriteLine("Retrieve data to render to check data with empty parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController reportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();

            // Fetch the respectives Campaign Ids and Program Ids from the tactic list

            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //ReportController.TempData["ReportData"] = tacticStageList;
            List<ActualTrendModel> ActualTacticTrendList = new List<ActualTrendModel>();
            List<string> ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add("MQL");
            List<string> includemonth = new List<string>();
            includemonth.Add(PlanYear);

            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<ProjectedTrendModel> ProjectedTrendList = new List<ProjectedTrendModel>();
            ProjectedTrendList = reportController.CalculateProjectedTrend(tacticStageList, includemonth, "MQL");
            ActualTacticStageList = reportController.GetActualListInTacticInterval(tacticStageList, PlanYear, ActualStageCodeList, true);

            ActualTacticTrendList = reportController.GetActualTrendModelForRevenueOverview(tacticStageList, ActualTacticStageList);

            CardSectionListModel = reportController.GetCardSectionDefaultData(tacticStageList, ActualTacticTrendList, ProjectedTrendList, _cmpgnMappingList, PlanYear, true, "Campaign", false, "", 0);
            reportController.TempData["RevenueCardList"] = CardSectionListModel;
            ////// Call SearchSortPaginataionRevenue() function
            ////string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0 
            var result = reportController.SearchSortPaginataionRevenue() as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_ReportCardSection", result.ViewName);
        }
        #endregion

        #region "SearchSortPaginataionRevenue with short by Revenue"
        /// <summary>
        /// Retrieve data to render view to check data with Short by Revenue.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>14aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionRevenue_ShortBy_Revenue()
        {
            Console.WriteLine("Retrieve data to render view to check data with Short by Revenue.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController reportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();

            // Fetch the respectives Campaign Ids and Program Ids from the tactic list

            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //ReportController.TempData["ReportData"] = tacticStageList;
            List<ActualTrendModel> ActualTacticTrendList = new List<ActualTrendModel>();
            List<string> ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add("MQL");
            List<string> includemonth = new List<string>();
            includemonth.Add(PlanYear);

            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<ProjectedTrendModel> ProjectedTrendList = new List<ProjectedTrendModel>();
            ProjectedTrendList = reportController.CalculateProjectedTrend(tacticStageList, includemonth, "MQL");
            ActualTacticStageList = reportController.GetActualListInTacticInterval(tacticStageList, PlanYear, ActualStageCodeList, true);

            ActualTacticTrendList = reportController.GetActualTrendModelForRevenueOverview(tacticStageList, ActualTacticStageList);

            CardSectionListModel = reportController.GetCardSectionDefaultData(tacticStageList, ActualTacticTrendList, ProjectedTrendList, _cmpgnMappingList, PlanYear, true, "Campaign", false, "", 0);
            reportController.TempData["RevenueCardList"] = CardSectionListModel;
            string shortBy = Enums.SortByRevenue.Revenue.ToString();
            ////// Call SearchSortPaginataionRevenue() function
            ////string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0 
            var result = reportController.SearchSortPaginataionRevenue(0, 0, "", shortBy, "", "", "") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_ReportCardSection", result.ViewName);
        }
        #endregion

        #region "SearchSortPaginataionRevenue with short by Cost"
        /// <summary>
        /// Retrieve data to render view to check data with Short by Cost.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>14aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionRevenue_ShortBy_Cost()
        {
            Console.WriteLine("Retrieve data to render view to check data with Short by Cost.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController reportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            List<ActualTrendModel> ActualTacticTrendList = new List<ActualTrendModel>();
            List<string> ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add("MQL");
            List<string> includemonth = new List<string>();
            includemonth.Add(PlanYear);

            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<ProjectedTrendModel> ProjectedTrendList = new List<ProjectedTrendModel>();
            ProjectedTrendList = reportController.CalculateProjectedTrend(tacticStageList, includemonth, "MQL");
            ActualTacticStageList = reportController.GetActualListInTacticInterval(tacticStageList, PlanYear, ActualStageCodeList, true);

            ActualTacticTrendList = reportController.GetActualTrendModelForRevenueOverview(tacticStageList, ActualTacticStageList);

            CardSectionListModel = reportController.GetCardSectionDefaultData(tacticStageList, ActualTacticTrendList, ProjectedTrendList, _cmpgnMappingList, PlanYear, true, "Campaign", false, "", 0);
            reportController.TempData["RevenueCardList"] = CardSectionListModel;
            string shortBy = Enums.SortByRevenue.Cost.ToString();
            ////// Call SearchSortPaginataionRevenue() function
            var result = reportController.SearchSortPaginataionRevenue(0, 0, "", shortBy, "", "", "") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("_ReportCardSection", result.ViewName);
        }
        #endregion

        #region "SearchSortPaginataionRevenue with short by ROI"
        /// <summary>
        /// Retrieve data to render view to check data with Short by ROI.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>14aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionRevenue_ShortBy_ROI()
        {
            Console.WriteLine("Retrieve data to render view to check data with Short by ROI.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController reportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            List<ActualTrendModel> ActualTacticTrendList = new List<ActualTrendModel>();
            List<string> ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add("MQL");
            List<string> includemonth = new List<string>();
            includemonth.Add(PlanYear);

            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<ProjectedTrendModel> ProjectedTrendList = new List<ProjectedTrendModel>();
            ProjectedTrendList = reportController.CalculateProjectedTrend(tacticStageList, includemonth, "MQL");
            ActualTacticStageList = reportController.GetActualListInTacticInterval(tacticStageList, PlanYear, ActualStageCodeList, true);

            ActualTacticTrendList = reportController.GetActualTrendModelForRevenueOverview(tacticStageList, ActualTacticStageList);

            CardSectionListModel = reportController.GetCardSectionDefaultData(tacticStageList, ActualTacticTrendList, ProjectedTrendList, _cmpgnMappingList, PlanYear, true, "Campaign", false, "", 0);
            reportController.TempData["RevenueCardList"] = CardSectionListModel;
            string shortBy = Enums.SortByRevenue.ROI.ToString();
            ////// Call SearchSortPaginataionRevenue() function
            var result = reportController.SearchSortPaginataionRevenue(0, 0, "", shortBy, "", "", "") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_ReportCardSection", result.ViewName);
        }
        #endregion

        #region "SearchSortPaginataionRevenue with short by Invalid Parameter"
        /// <summary>
        /// Retrieve data to render view to check data with Short by Invalid Parameter.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>14aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionRevenue_ShortBy_Invalid_Param()
        {
            Console.WriteLine("Retrieve data to render view to check data with Short by Invalid Parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController reportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            List<ActualTrendModel> ActualTacticTrendList = new List<ActualTrendModel>();
            List<string> ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add("MQL");
            List<string> includemonth = new List<string>();
            includemonth.Add(PlanYear);

            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<ProjectedTrendModel> ProjectedTrendList = new List<ProjectedTrendModel>();
            ProjectedTrendList = reportController.CalculateProjectedTrend(tacticStageList, includemonth, "MQL");
            ActualTacticStageList = reportController.GetActualListInTacticInterval(tacticStageList, PlanYear, ActualStageCodeList, true);

            ActualTacticTrendList = reportController.GetActualTrendModelForRevenueOverview(tacticStageList, ActualTacticStageList);

            CardSectionListModel = reportController.GetCardSectionDefaultData(tacticStageList, ActualTacticTrendList, ProjectedTrendList, _cmpgnMappingList, PlanYear, true, "Campaign", false, "", 0);
            reportController.TempData["RevenueCardList"] = CardSectionListModel;
            string shortBy = "Invalid";
            ////// Call SearchSortPaginataionRevenue() function
            var result = reportController.SearchSortPaginataionRevenue(0, 0, "", shortBy, "", "", "") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_ReportCardSection", result.ViewName);
            
        }
        #endregion

        #region "SearchSortPaginataionRevenue with  Empty Search Parameter"
        /// <summary>
        /// Retrieve data to render view to check data with Empty Search Parameter.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>14aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionRevenue_Empty_Search_Param()
        {
            Console.WriteLine("Retrieve data to render view to check data with Empty Search Parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;


            ReportController reportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            List<ActualTrendModel> ActualTacticTrendList = new List<ActualTrendModel>();
            List<string> ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add("MQL");
            List<string> includemonth = new List<string>();
            includemonth.Add(PlanYear);

            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<ProjectedTrendModel> ProjectedTrendList = new List<ProjectedTrendModel>();
            ProjectedTrendList = reportController.CalculateProjectedTrend(tacticStageList, includemonth, "MQL");
            ActualTacticStageList = reportController.GetActualListInTacticInterval(tacticStageList, PlanYear, ActualStageCodeList, true);

            ActualTacticTrendList = reportController.GetActualTrendModelForRevenueOverview(tacticStageList, ActualTacticStageList);

            CardSectionListModel = reportController.GetCardSectionDefaultData(tacticStageList, ActualTacticTrendList, ProjectedTrendList, _cmpgnMappingList, PlanYear, true, "Campaign", false, "", 0);
            reportController.TempData["RevenueCardList"] = CardSectionListModel;
            string shortBy = Enums.SortByRevenue.Revenue.ToString();
            ////// Call SearchSortPaginataionRevenue() function
            var result = reportController.SearchSortPaginataionRevenue(0, 0, string.Empty, shortBy, "", "", "") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_ReportCardSection", result.ViewName);
        }
        #endregion

        #region "GetChildLabelDataViewByModel with empty parameter"
        /// <summary>
        /// Get data check with Empty Parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>14aug2015</createddate>
        ///
        [TestMethod]
        public void GetChildLabelDataViewByModel_empty()
        {
            Console.WriteLine("Get data check with Empty Parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ReportController reportcontroller = new ReportController();
            reportcontroller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            var result = reportcontroller.GetChildLabelDataViewByModel("", "");
      
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Count);
            Assert.IsNotNull(result);
            
        }
        #endregion




        #endregion
        #endregion

        #region "Waterfall Section"

        #region "GetWaterFallData with No Parameter"
        /// <summary>
        /// GetWaterFallData section with No Parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>12aug2015</createddate>
        /// 
        [TestMethod]
        public void GetWaterFallData_No_Parameter()
        {
            Console.WriteLine("GetWaterFallData section with No Parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            var SetOFLastViews = db.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            Common.PlanUserSavedViews = SetOFLastViews;
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            //int planId=DataHelper.GetPlanId();
            //// Call GetWaterFallData() function
            ReportController ReportController = new ReportController();

            var result = ReportController.GetWaterFallData(PlanYear) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_ReportConversion", result.ViewName);
          
        }
        #endregion

        #region "GetWaterFallData section with timeframe empty"
        /// <summary>
        /// GetWaterFallData section with timeframe empty
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>11aug2015</createddate>
        /// 
        [TestMethod]
        public void GetWaterFallData_Timeframe_Empty()
        {
            Console.WriteLine("GetWaterFallData section with timeframe empty.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            var SetOFLastViews = db.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            Common.PlanUserSavedViews = SetOFLastViews;
            List<int> PlanIds = new List<int>();
            PlanIds.Add(17314);
            Sessions.ReportPlanIds = PlanIds;
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            //int planId=DataHelper.GetPlanId();
            //// Call GetWaterFallData() function
            ReportController ReportController = new ReportController();

            var result = ReportController.GetWaterFallData(PlanYear, Enums.ViewByAllocated.Quarterly.ToString()) as PartialViewResult;
       
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_ReportConversion", result.ViewName);
            
        }
        #endregion
        //Commented by Rahul Shah on 21/11/2015. Because it is negative test cases
        //#region"GetWaterFallData section with Invalid timeframe"

        ///// <summary>
        ///// GetWaterFallData section with Invalid timeframe
        ///// </summary>
        ///// <auther>Dashrath Prajapati</auther>
        ///// <createddate>11aug2015</createddate>
        ///// 
        //[TestMethod]
        //public void GetWaterFallData_Timeframe_InvalidTimeframe()
        //{
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int planId = DataHelper.GetPlanId();
        //    List<int> lst = new List<int>();
        //    lst.Add(planId);
        //    HttpContext.Current.Session["ReportPlanIds"] = lst;
        //    //// Call GetWaterFallData() function
        //    ReportController ReportController = new ReportController();
        //    string invalidTimeframe = "Invalid";
        //    var result = ReportController.GetWaterFallData(invalidTimeframe, Enums.ViewByAllocated.Quarterly.ToString()) as PartialViewResult;
        //    //// PartialViewResult shoud not be null and should match with Partial viewName
        //    Assert.AreEqual("_ReportConversion", result.ViewName);
        //}
        //#endregion

        #region "GetWaterFallData section with Empty Quaterly"
        /// <summary>
        /// GetWaterFallData section with Empty Quaterly
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>11aug2015</createddate>
        /// 
        [TestMethod]
        public void GetWaterFallData_Empty_Quaterly()
        {
            Console.WriteLine("GetWaterFallData section with Empty Quaterly.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();
            var result = ReportController.GetWaterFallData(PlanYear, string.Empty) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_ReportConversion", result.ViewName);
        }
        #endregion

        #region "GetWaterFallData section with Invalid Quaterly"
        /// <summary>
        /// GetWaterFallData section with Invalid Quaterly
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>11aug2015</createddate>
        /// 
        [TestMethod]
        public void GetWaterFallData_InvalidQuaterly()
        {
            Console.WriteLine("GetWaterFallData section with Invalid Quaterly.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string invalidQuater = "_InValidIsQuarterly";
            ReportController ReportController = new ReportController();
            var result = ReportController.GetWaterFallData(PlanYear, invalidQuater) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_ReportConversion", result.ViewName);
        }
        #endregion

        #region "GetWaterFallData section with both empty Quaterly and timeframe"
        /// <summary>
        /// GetWaterFallData section with both empty Quaterly and timeframe
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>11aug2015</createddate>
        /// 
        [TestMethod]
        public void GetWaterFallData_Empty_Quater_andTimeframe()
        {
            Console.WriteLine("GetWaterFallData section with both empty Quaterly and timeframe.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            var SetOFLastViews = db.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            Common.PlanUserSavedViews = SetOFLastViews;
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();
            var result = ReportController.GetWaterFallData(PlanYear, string.Empty) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_ReportConversion", result.ViewName);
        }
        #endregion

        #region "GetTopConversionToPlanByCustomFilter section with both empty parent label"
        /// <summary>
        /// GetTopConversionToPlanByCustomFilter section with both empty parent label
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>11aug2015</createddate>
        /// 
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_EmptyParentlbl()
        {
            Console.WriteLine("GetTopConversionToPlanByCustomFilter section with both empty parent label.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();

            var result = ReportController.GetTopConversionToPlanByCustomFilter(string.Empty, "Campaign", "", PlanYear, Enums.ViewByAllocated.Quarterly.ToString(), Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region "GetTopConversionToPlanByCustomFilter section with both invalid parent label"
        /// <summary>
        /// GetTopConversionToPlanByCustomFilter section with both invalid parent label
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>11aug2015</createddate>
        /// 
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_InvalidParentlbl()
        {
            Console.WriteLine("GetTopConversionToPlanByCustomFilter section with both invalid parent label.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();
            string _InvalidParentbl = "_Invalid";
            var result = ReportController.GetTopConversionToPlanByCustomFilter(_InvalidParentbl, "Campaign", "", PlanYear, Enums.ViewByAllocated.Quarterly.ToString(), Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region "GetTopConversionToPlanByCustomFilter section with No Parameter"

        /// <summary>
        /// GetTopConversionToPlanByCustomFilter section with No Parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>12aug2015</createddate>
        ///
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_with_NoParameter()
        {
            Console.WriteLine("GetTopConversionToPlanByCustomFilter section with No Parameter.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();
            var result = ReportController.GetTopConversionToPlanByCustomFilter() as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region "GetTopConversionToPlanByCustomFilter with Campaign as ParentType"
        /// <summary>
        /// Retrieve data to render view by ParentLabel as Campaign
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>12aug2015</createddate>
        ///
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_ByCampaign()
        {
            Console.WriteLine("Retrieve data to render view by ParentLabel as Campaign.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();

            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, "", "", PlanYear, "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region "GetTopConversionToPlanByCustomFilter with Tactic Custom"
        /// <summary>
        /// Retrieve data to render view by ParentLabel as Tactic Custom
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>12aug2015</createddate>
        ///
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_ByTacticCustom()
        {
            Console.WriteLine("Retrieve data to render view by ParentLabel as Tactic Custom.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Tactic.ToString();
            ReportController ReportController = new ReportController();

            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, "", "", PlanYear, "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region "GetTopConversionToPlanByCustomFilter with Program Custom"
        /// <summary>
        /// Retrieve data to render view by ParentLabel as Program Custom
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>12aug2015</createddate>
        ///
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_ByProgramCustom()
        {
            Console.WriteLine("Retrieve data to render view by ParentLabel as Program Custom.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Program.ToString();
            ReportController ReportController = new ReportController();

            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, "", "", PlanYear, "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region "GetTopConversionToPlanByCustomFilter with CampaignCustom"
        /// <summary>
        /// Retrieve data to render view by ParentLabel as Program Campaign Custom
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>12aug2015</createddate>
        ///
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_ByCampaignCustom()
        {
            Console.WriteLine("Retrieve data to render view by ParentLabel as Program Campaign Custom.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();

            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, "", "", PlanYear, "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region "GetTopConversionToPlanByCustomFilter with ChildCampaignCustom"
        /// <summary>
        /// Retrieve data to render view by Child Type as Campaign Custom
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>12aug2015</createddate>
        ///
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_By_ChildCampaignCustom()
        {
            Console.WriteLine("Retrieve data to render view by Child Type as Campaign Custom.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();

            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            //List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            //List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();

            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //TempData["ReportData"] = tacticStageList;
            ReportController.TempData["ReportData"] = tacticStageList;
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, StrChildLabel, "", PlanYear, "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region "GetTopConversionToPlanByCustomFilter with empty parent and child"
        /// <summary>
        /// Retrieve data to render view by Parent Label and Child Type are empty
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>12aug2015</createddate>
        ///
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_By_Parent_Child_Empty()
        {
            Console.WriteLine("Retrieve data to render view by Parent Label and Child Type are empty.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //TempData["ReportData"] = tacticStageList;
            ReportController.TempData["ReportData"] = tacticStageList;
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(string.Empty, string.Empty, "", PlanYear, "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region "GetTopConversionToPlanByCustomFilter with child empty"
        /// <summary>
        /// Retrieve data to render view by childId with empty
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>12aug2015</createddate>
        ///
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_ChildId_Empty()
        {
            Console.WriteLine("Retrieve data to render view by childId with empty.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //TempData["ReportData"] = tacticStageList;
            ReportController.TempData["ReportData"] = tacticStageList;
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, StrChildLabel, "", PlanYear, "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region"GetTopConversionToPlanByCustomFilter with timeframe and quater empty"
        /// <summary>
        /// Retrieve data to render view by Timeframe and Quater with empty
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>12aug2015</createddate>
        ///
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_Timeframe_Qauter_Empty()
        {
            Console.WriteLine("Retrieve data to render view by Timeframe and Quater with empty.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //TempData["ReportData"] = tacticStageList;
            ReportController.TempData["ReportData"] = tacticStageList;
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, StrChildLabel, "0", string.Empty, string.Empty, Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region "GetTopConversionToPlanByCustomFilter with Invalid Quater and Code"
        /// <summary>
        /// Retrieve data to render view by Invalid Quater and Code
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>12aug2015</createddate>
        ///
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_InvalidQuater_Code()
        {

            Console.WriteLine("Retrieve data to render view by Invalid Quater and Code.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            string InvalidQuater = "InvalidQuater";
            string InvalidCode = "XYZ";
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //TempData["ReportData"] = tacticStageList;
            ReportController.TempData["ReportData"] = tacticStageList;
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, StrChildLabel, "0", PlanYear, InvalidQuater, InvalidCode, false, null, false, null, null, 0) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region "GetTopConversionToPlanByCustomFilter with Empty Code,BackHeadTitle,DrpChange,marsterCustomField,masterCustomFieldOptionId"
        /// <summary>
        /// Retrieve data to render view by Empty Code,BackHeadTitle,DrpChange,marsterCustomField,masterCustomFieldOptionId
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>12aug2015</createddate>
        ///
        [TestMethod]
        public void GetTopConversionToPlanByCustomFilter_Empty_Code_BackHeadTitle_DrpChange_marsterCustomField_masterCustomFieldOptionId()
        {

            Console.WriteLine("Retrieve data to render view by Empty Code,BackHeadTitle,DrpChange,marsterCustomField,masterCustomFieldOptionId.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string StrChildLabel = Enums.PlanEntity.Campaign.ToString();
            string InvalidQuater = "InvalidQuater";
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //TempData["ReportData"] = tacticStageList;
            ReportController.TempData["ReportData"] = tacticStageList;
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, StrChildLabel, "0", PlanYear, InvalidQuater, string.Empty, false, null, false, null, null, -1) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionToPlan", result.ViewName);
        }
        #endregion

        #region "SearchSortPaginataionConverstion with empty parameter"
        /// <summary>
        /// Retrieve data to render to check data with empty parameter.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionConverstion_Empty_Parameter()
        {
            Console.WriteLine("Retrieve data to render to check data with empty parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrCampaign = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //TempData["ReportData"] = tacticStageList;
            ReportController.TempData["ReportData"] = tacticStageList;
            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
            CardSectionModel objCardSectionModel = new CardSectionModel();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            CardSectionListModel = ReportController.GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, PlanYear, false, StrCampaign, false, "", 0);
            ReportController.TempData["ConverstionCardList"] = CardSectionListModel;
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.SearchSortPaginataionConverstion() as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionCardSection", result.ViewName);
        }
        #endregion

        #region "SearchSortPaginataionConverstion with short by INQ"
        /// <summary>
        /// Retrieve data to render view to check data with Short by INQ.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionConverstion_Shortby_INQ()
        {
            Console.WriteLine("Retrieve data to render view to check data with Short by INQ.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrCampaign = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //TempData["ReportData"] = tacticStageList;
            ReportController.TempData["ReportData"] = tacticStageList;
            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
            CardSectionModel objCardSectionModel = new CardSectionModel();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            CardSectionListModel = ReportController.GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, PlanYear, false, StrCampaign, false, "", 0);
            ReportController.TempData["ConverstionCardList"] = CardSectionListModel;
            string shortBy = Enums.SortByWaterFall.INQ.ToString();

            var result = ReportController.SearchSortPaginataionConverstion(0, 0, "", shortBy, "", "", "") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionCardSection", result.ViewName);
        }
        #endregion

        #region "SearchSortPaginataionConverstion with short by MQL"
        /// <summary>
        /// Retrieve data to render view to check data with Short by MQL.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionConverstion_Shortby_MQL()
        {
            Console.WriteLine("Retrieve data to render view to check data with Short by MQL.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrCampaign = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //TempData["ReportData"] = tacticStageList;
            ReportController.TempData["ReportData"] = tacticStageList;
            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
            CardSectionModel objCardSectionModel = new CardSectionModel();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            CardSectionListModel = ReportController.GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, PlanYear, false, StrCampaign, false, "", 0);
            ReportController.TempData["ConverstionCardList"] = CardSectionListModel;
            string shortBy = Enums.SortByWaterFall.MQL.ToString();

            var result = ReportController.SearchSortPaginataionConverstion(0, 0, "", shortBy, "", "", "") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionCardSection", result.ViewName);
        }
        #endregion

        #region "SearchSortPaginataionConverstion with short by CW"
        /// <summary>
        /// Retrieve data to render view to check data with Short by CW.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionConverstion_Shortby_CW()
        {
            Console.WriteLine("Retrieve data to render view to check data with Short by CW.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrCampaign = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //TempData["ReportData"] = tacticStageList;
            ReportController.TempData["ReportData"] = tacticStageList;
            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
            CardSectionModel objCardSectionModel = new CardSectionModel();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            CardSectionListModel = ReportController.GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, PlanYear, false, StrCampaign, false, "", 0);
            ReportController.TempData["ConverstionCardList"] = CardSectionListModel;
            string shortBy = Enums.SortByWaterFall.CW.ToString();

            var result = ReportController.SearchSortPaginataionConverstion(0, 0, "", shortBy, "", "", "") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionCardSection", result.ViewName);
        }
        #endregion

        #region "SearchSortPaginataionConverstion with Invalid Shortby"
        /// <summary>
        /// Retrieve data to render view to check data with Invalid Shortby.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionConverstion_Invalid_Shortby()
        {
            Console.WriteLine("Retrieve data to render view to check data with Invalid Shortby.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrCampaign = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //TempData["ReportData"] = tacticStageList;
            ReportController.TempData["ReportData"] = tacticStageList;
            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
            CardSectionModel objCardSectionModel = new CardSectionModel();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            CardSectionListModel = ReportController.GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, PlanYear, false, StrCampaign, false, "", 0);
            ReportController.TempData["ConverstionCardList"] = CardSectionListModel;
            string shortBy = "_Invalid";

            var result = ReportController.SearchSortPaginataionConverstion(0, 0, "", shortBy, "", "", "") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionCardSection", result.ViewName);
         }
        #endregion

        #region "SearchSortPaginataionConverstion with Invalid short by and empty search content"
        /// <summary>
        /// Retrieve data to render view to check data with empty SerachContent and invalid shortby.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionConverstion_Invalid_Shortby_empty_SearchContent()
        {
            Console.WriteLine("Retrieve data to render view to check data with empty SerachContent and invalid shortby.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrCampaign = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            //TempData["ReportData"] = tacticStageList;
            ReportController.TempData["ReportData"] = tacticStageList;
            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
            CardSectionModel objCardSectionModel = new CardSectionModel();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            CardSectionListModel = ReportController.GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, PlanYear, false, StrCampaign, false, "", 0);
            ReportController.TempData["ConverstionCardList"] = CardSectionListModel;
            string shortBy = "_Invalid";

            var result = ReportController.SearchSortPaginataionConverstion(0, 0, string.Empty, shortBy, "", "", "") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionCardSection", result.ViewName);
        }
        #endregion

        #region "LoadReportCardSectionPartial with empty parameter"
        /// <summary>
        /// Retrieve data to render view to check with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void LoadReportCardSectionPartial_Empty()
        {
            Console.WriteLine("Retrieve data to render view to check with empty parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ReportController ReportController = new ReportController();
            var result = ReportController.LoadReportCardSectionPartial() as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ReportCardSection", result.ViewName);
        }
        #endregion

        #region "LoadConverstionCardSectionPartial with empty parameter"
        /// <summary>
        /// Retrieve data to render view to check with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void LoadConverstionCardSectionPartial_Empty()
        {
            Console.WriteLine("Retrieve data to render view to check with empty parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ReportController ReportController = new ReportController();
            var result = ReportController.LoadConverstionCardSectionPartial() as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_ConversionCardSection", result.ViewName);
        }
        #endregion

        #endregion

        #region Custom Reports

        #region "GetCustomReport with null DashboardId"
        /// <summary>
        /// GetCustomReport with Null DashboardId
        /// Created By Nishant Sheth
        /// Created Date : 14-Jun-2016
        /// </summary>
        [TestMethod]
        public void GetCustomReport_Null_DashboardId()
        {
            Console.WriteLine("GetCustomReport section with Null DashboardId.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ReportController ReportController = new ReportController();
            ReportController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = ReportController.GetCustomReport(null) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_DynamicReport", result.ViewName);
  
        }
        #endregion

        #region "GetCustomReport with Empty DashboardId"
        /// <summary>
        /// GetCustomReport with Empty DashboardId
        /// Created By Nishant Sheth
        /// Created Date : 14-Jun-2016
        /// </summary>
        [TestMethod]
        public void GetCustomReport_Empty_DashboardId()
        {
            Console.WriteLine("GetCustomReport section with Empty DashboardId.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ReportController ReportController = new ReportController();
            ReportController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = ReportController.GetCustomReport(string.Empty) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : : " + result.ViewName);
            Assert.AreEqual("_DynamicReport", result.ViewName);

            
        }
        #endregion

        #region "GetCustomReport with DashboardId"
        /// <summary>
        /// GetCustomReport with DashboardId
        /// Created By Nishant Sheth
        /// Created Date : 14-Jun-2016
        /// </summary>
        [TestMethod]
        public void GetCustomReport_With_DashboardId()
        {
            Console.WriteLine("GetCustomReport section with DashboardId.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ReportController ReportController = new ReportController();
            ReportController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var DashboardId = DataHelper.GetDashboardId();
            var result = ReportController.GetCustomReport(DashboardId) as PartialViewResult;
            Assert.AreEqual("_DynamicReport", result.ViewName);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
        }
        #endregion

        #endregion

        #region GetReportBudgetDataQuarter
        [TestMethod]
        public void GetReportBudgetData()
        {
            Console.WriteLine("To get report on revenue tab.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            List<int> lstPlanIds = new List<int>();
            var tactic = db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted == false).FirstOrDefault();
            int planId = tactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
            lstPlanIds.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lstPlanIds;

            ReportController ReportController = new ReportController();
            var result = ReportController.GetReportBudgetData("2016", "quarters", "Plan","") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_Budget", result.ViewName);
            var result1 = ReportController.GetReportBudgetData("thisquarter", "quarters", "Plan", "") as PartialViewResult;
            Assert.AreEqual("_Budget", result1.ViewName);
           
        }

        #endregion
    }
}

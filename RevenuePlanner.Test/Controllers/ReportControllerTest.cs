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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null) {

                Assert.AreNotEqual("_Overview", result);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.Status);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
            
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreNotEqual("_Overview", result);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.Status);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            List<int> lst = new List<int>();
            lst.Add(9775);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ///Common.objCached.RevenueSparklineChartHeader = "Top {0} by";
            //// Call GetOverviewData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetOverviewData(PlanYear, string.Empty) as Task<ActionResult>;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreNotEqual("_Overview", result);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.Status);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreNotEqual("_Overview", result);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.Status);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName            
            if (result != null)
            {

                Assert.AreEqual("_Revenue", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_Revenue", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_Revenue", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_Revenue", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_RevenueToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_RevenueToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_RevenueToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_RevenueToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_RevenueToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_RevenueToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            string Isquater = Enums.ViewByAllocated.Quarterly.ToString();

            ReportController ReportController = new ReportController();
            List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            ReportController.TempData["ReportData"] = tacticStageList;
            //// Call GetRevenueToPlanByFilter() function


            var result = ReportController.GetRevenueToPlanByFilter(StrParentLabel, "Campaign", "0", PlanYear, Isquater, false, "", false, "CampaignDrp", "", 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_RevenueToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }


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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_RevenueToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_RevenueToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_RevenueToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_RevenueToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ReportCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName            
            if (result != null)
            {

                Assert.AreEqual("_ReportCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ReportCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ReportCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ReportCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ReportCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
        }
        #endregion

        //#region "LoadRevenueContribution with Empty Parameter"
        ///// <summary>
        ///// Get data check with Empty Parameter.
        ///// </summary>
        ///// <auther>Dashrath Prajapati</auther>
        ///// <createddate>14aug2015</createddate>
        /////LoadRevenueContribution
        //[TestMethod]
        //public void LoadRevenueContribution_Empty_Param()
        //{
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int planId = DataHelper.GetPlanId();
        //    List<int> lst = new List<int>();
        //    lst.Add(14936);
        //    HttpContext.Current.Session["ReportPlanIds"] = lst;
        //    ReportController reportController = new ReportController();
        //   var result= reportController.LoadRevenueContribution("","");
        //   Assert.IsNotNull(result.Data);

        //}
        //#endregion

        //#region "LoadRevenueContribution with ParentLabel as TacticCustom"
        ///// <summary>
        ///// Get data check with Parameter ParentLabel as TacticCustom .
        ///// </summary>
        ///// <auther>Dashrath Prajapati</auther>
        ///// <createddate>14aug2015</createddate>
        /////LoadRevenueContribution
        //[TestMethod]
        //public void LoadRevenueContribution_ParentType_TacticCustom()
        //{
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int planId = DataHelper.GetPlanId();
        //    List<int> lst = new List<int>();
        //    lst.Add(14936);
        //    HttpContext.Current.Session["ReportPlanIds"] = lst;
        //    ReportController reportController = new ReportController();
        //    List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
        //    // Fetch the respectives Campaign Ids and Program Ids from the tactic list

        //    //// Calculate Value for ecah tactic
        //    List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
        //    //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
        //    //TempData["ReportData"] = tacticStageList;
        //    reportController.TempData["ReportData"] = tacticStageList;
        //    string Parentlbl = Common.TacticCustomTitle.ToString();
        //    var result = reportController.LoadRevenueContribution(Parentlbl,PlanYear);
        //    Assert.IsNotNull(result.Data);

        //}
        //#endregion

        //#region "LoadRevenueContribution with ParentLabel as CampaignCustom"
        ///// <summary>
        ///// Get data check with Parameter ParentLabel as CampaignCustom .
        ///// </summary>
        ///// <auther>Dashrath Prajapati</auther>
        ///// <createddate>14aug2015</createddate>
        /////LoadRevenueContribution
        //[TestMethod]
        //public void LoadRevenueContribution_ParentType_CampaignCustom()
        //{
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int planId = DataHelper.GetPlanId();
        //    List<int> lst = new List<int>();
        //    lst.Add(14936);
        //    HttpContext.Current.Session["ReportPlanIds"] = lst;
        //    ReportController reportController = new ReportController();
        //    List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
        //    // Fetch the respectives Campaign Ids and Program Ids from the tactic list

        //    //// Calculate Value for ecah tactic
        //    List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
        //    //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
        //    //TempData["ReportData"] = tacticStageList;
        //    reportController.TempData["ReportData"] = tacticStageList;
        //    string Parentlbl = Common.CampaignCustomTitle.ToString();
        //    var result = reportController.LoadRevenueContribution(Parentlbl, PlanYear);
        //    Assert.IsNotNull(result.Data);

        //}
        //#endregion

        //#region "LoadRevenueContribution with ParentLabel as ProgramCustom"
        ///// <summary>
        ///// Get data check with Parameter ParentLabel as ProgramCustom .
        ///// </summary>
        ///// <auther>Dashrath Prajapati</auther>
        ///// <createddate>14aug2015</createddate>
        /////LoadRevenueContribution
        //[TestMethod]
        //public void LoadRevenueContribution_ParentType_ProgramCustom()
        //{
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int planId = DataHelper.GetPlanId();
        //    List<int> lst = new List<int>();
        //    lst.Add(14936);
        //    HttpContext.Current.Session["ReportPlanIds"] = lst;
        //    ReportController reportController = new ReportController();
        //    List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
        //    // Fetch the respectives Campaign Ids and Program Ids from the tactic list

        //    //// Calculate Value for ecah tactic
        //    List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
        //    //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
        //    //TempData["ReportData"] = tacticStageList;
        //    reportController.TempData["ReportData"] = tacticStageList;
        //    string Parentlbl = Common.ProgramCustomTitle.ToString();
        //    var result = reportController.LoadRevenueContribution(Parentlbl, PlanYear);
        //    Assert.IsNotNull(result.Data);

        //}
        //#endregion

        //#region "GetRevenueToPlan with Empty"
        ///// <summary>
        ///// Get data check with Empty parameter.
        ///// </summary>
        ///// <auther>Dashrath Prajapati</auther>
        ///// <createddate>14aug2015</createddate>
        /////GetRevenueToPlan
        //[TestMethod]
        //public void GetRevenueToPlan_empty()
        //{
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int planId = DataHelper.GetPlanId();
        //    List<int> lst = new List<int>();
        //    lst.Add(14936);
        //    HttpContext.Current.Session["ReportPlanIds"] = lst;
        //    ReportController reportController = new ReportController();
        //    List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
        //    // Fetch the respectives Campaign Ids and Program Ids from the tactic list

        //    //// Calculate Value for ecah tactic
        //    List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
        //    //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
        //    //TempData["ReportData"] = tacticStageList;
        //    reportController.TempData["ReportData"] = tacticStageList;
        //    string Parentlbl = Common.CampaignCustomTitle.ToString();
        //   var result= reportController.GetRevenueToPlan(Parentlbl,"0",PlanYear,"");
        //   Assert.IsNotNull(result.Data);

        //}
        //#endregion

        //#region "GetRevenueToPlan with Parent Label as Campaign Custom"
        ///// <summary>
        ///// Get data check with Parameter ParentLabel as Camapaign Custom.
        ///// </summary>
        ///// <auther>Dashrath Prajapati</auther>
        ///// <createddate>14aug2015</createddate>
        /////GetRevenueToPlan
        //[TestMethod]
        //public void GetRevenueToPlan_Parent_CamapaignCustom()
        //{
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int planId = DataHelper.GetPlanId();
        //    List<int> lst = new List<int>();
        //    lst.Add(14936);
        //    HttpContext.Current.Session["ReportPlanIds"] = lst;
        //    ReportController reportController = new ReportController();
        //    List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
        //    // Fetch the respectives Campaign Ids and Program Ids from the tactic list

        //    //// Calculate Value for ecah tactic
        //    List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
        //    //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
        //    //TempData["ReportData"] = tacticStageList;
        //    reportController.TempData["ReportData"] = tacticStageList;
        //    string Parentlbl = Common.CampaignCustomTitle.ToString();
        //    var result = reportController.GetRevenueToPlan(Parentlbl, "0", PlanYear, "");
        //    Assert.IsNotNull(result.Data);

        //}
        //#endregion

        //#region "GetRevenueToPlan with ProgramCustomTitle as Parenet Label"
        ///// <summary>
        ///// Get data check with Parameter ParentLabel as ProgramCustomTitle Custom.
        ///// </summary>
        ///// <auther>Dashrath Prajapati</auther>
        ///// <createddate>14aug2015</createddate>
        /////GetRevenueToPlan
        //[TestMethod]
        //public void GetRevenueToPlan_Parent_ProgramCustom()
        //{
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int planId = DataHelper.GetPlanId();
        //    List<int> lst = new List<int>();
        //    lst.Add(14936);
        //    HttpContext.Current.Session["ReportPlanIds"] = lst;
        //    ReportController reportController = new ReportController();
        //    List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
        //    // Fetch the respectives Campaign Ids and Program Ids from the tactic list

        //    //// Calculate Value for ecah tactic
        //    List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
        //    //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
        //    //TempData["ReportData"] = tacticStageList;
        //    reportController.TempData["ReportData"] = tacticStageList;
        //    string Parentlbl = Common.ProgramCustomTitle.ToString();
        //    var result = reportController.GetRevenueToPlan(Parentlbl, "0", PlanYear, "");
        //    Assert.IsNotNull(result.Data);

        //}
        //#endregion

        //#region "GetRevenueToPlan with TacticCustomTitle as Parenet Label"
        ///// <summary>
        ///// Get data check with Parameter ParentLabel as TacticCustomTitle.
        ///// </summary>
        ///// <auther>Dashrath Prajapati</auther>
        ///// <createddate>14aug2015</createddate>
        /////GetRevenueToPlan
        //[TestMethod]
        //public void GetRevenueToPlan_Parent_TacticCustom()
        //{
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int planId = DataHelper.GetPlanId();
        //    List<int> lst = new List<int>();
        //    lst.Add(14936);
        //    HttpContext.Current.Session["ReportPlanIds"] = lst;
        //    ReportController reportController = new ReportController();
        //    List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
        //    // Fetch the respectives Campaign Ids and Program Ids from the tactic list

        //    //// Calculate Value for ecah tactic
        //    List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
        //    //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
        //    //TempData["ReportData"] = tacticStageList;
        //    reportController.TempData["ReportData"] = tacticStageList;
        //    string Parentlbl = Common.TacticCustomTitle.ToString();
        //    var result = reportController.GetRevenueToPlan(Parentlbl, "0", PlanYear, "");
        //    Assert.IsNotNull(result.Data);

        //}
        //#endregion

        //#region "GetRevenueSummaryDataRevenueReport with empty parameter"
        ///// <summary>
        ///// Get data check with Empty Parameter
        ///// </summary>
        ///// <auther>Dashrath Prajapati</auther>
        ///// <createddate>14aug2015</createddate>
        /////
        //[TestMethod]
        //public void GetRevenueSummaryDataRevenueReport_Empty_Param()
        //{
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int planId = DataHelper.GetPlanId();
        //    List<int> lst = new List<int>();
        //    lst.Add(14936);
        //    HttpContext.Current.Session["ReportPlanIds"] = lst;
        //    ReportController reportController = new ReportController();
        //    List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
        //    // Fetch the respectives Campaign Ids and Program Ids from the tactic list

        //    //// Calculate Value for ecah tactic
        //    List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
        //    //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
        //    //TempData["ReportData"] = tacticStageList;
        //    reportController.TempData["ReportData"] = tacticStageList;

        //    var result = reportController.GetRevenueSummaryDataRevenueReport("","","");
        //    Assert.IsNotNull(result.Data);
        //}
        //#endregion

        //#region "GetRevenueSummaryDataRevenueReport with CampaignCustom Title"
        ///// <summary>
        ///// Get data check with Parent label as Campaign Custom Title
        ///// </summary>
        ///// <auther>Dashrath Prajapati</auther>
        ///// <createddate>14aug2015</createddate>
        /////
        //[TestMethod]
        //public void GetRevenueSummaryDataRevenueReport_Parent_CampaignCustom()
        //{
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int planId = DataHelper.GetPlanId();
        //    List<int> lst = new List<int>();
        //    lst.Add(14936);
        //    HttpContext.Current.Session["ReportPlanIds"] = lst;
        //    ReportController reportController = new ReportController();
        //    List<Plan_Campaign_Program_Tactic> tacticlist = DataHelper.GetTacticForReporting();
        //    // Fetch the respectives Campaign Ids and Program Ids from the tactic list

        //    //// Calculate Value for ecah tactic
        //    List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
        //    //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
        //    //TempData["ReportData"] = tacticStageList;
        //    reportController.TempData["ReportData"] = tacticStageList;
        //    string Parentlbl = Common.CampaignCustomTitle.ToString();
        //    var result = reportController.GetRevenueSummaryDataRevenueReport(Parentlbl,"0","");
        //    Assert.IsNotNull(result.Data);
        //}
        //#endregion

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
            ReportController reportcontroller = new ReportController();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            var result = reportcontroller.GetChildLabelDataViewByModel("", "");            
            if (result != null)
            {

                Assert.IsNotNull(result);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.Count);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName            
            if (result != null)
            {

                Assert.AreEqual("_ReportConversion", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ReportConversion", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();
            var result = ReportController.GetWaterFallData(PlanYear, string.Empty) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ReportConversion", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string invalidQuater = "_InValidIsQuarterly";
            ReportController ReportController = new ReportController();
            var result = ReportController.GetWaterFallData(PlanYear, invalidQuater) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ReportConversion", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            var SetOFLastViews = db.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            Common.PlanUserSavedViews = SetOFLastViews;
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();
            var result = ReportController.GetWaterFallData(PlanYear, string.Empty) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ReportConversion", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();

            var result = ReportController.GetTopConversionToPlanByCustomFilter(string.Empty, "Campaign", "", PlanYear, Enums.ViewByAllocated.Quarterly.ToString(), Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName            
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }

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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();
            string _InvalidParentbl = "_Invalid";
            var result = ReportController.GetTopConversionToPlanByCustomFilter(_InvalidParentbl, "Campaign", "", PlanYear, Enums.ViewByAllocated.Quarterly.ToString(), Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }

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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();
            var result = ReportController.GetTopConversionToPlanByCustomFilter() as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();

            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, "", "", PlanYear, "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }

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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Tactic.ToString();
            ReportController ReportController = new ReportController();

            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, "", "", PlanYear, "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }

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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Program.ToString();
            ReportController ReportController = new ReportController();

            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, "", "", PlanYear, "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();

            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, "", "", PlanYear, "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionToPlan", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName            
            if (result != null)
            {

                Assert.AreEqual("_ConversionCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ReportController ReportController = new ReportController();
            var result = ReportController.LoadReportCardSectionPartial() as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName            
            if (result != null)
            {

                Assert.AreEqual("_ReportCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ReportController ReportController = new ReportController();
            var result = ReportController.LoadConverstionCardSectionPartial() as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            if (result != null)
            {

                Assert.AreEqual("_ConversionCardSection", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + result.ViewName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Fail – And result value is " + result);
            }
        }
        #endregion

        #endregion
    }
}

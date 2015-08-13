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

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class ReportControllerTest
    {
        #region "Overview section of Report"

        #region "GetOverviewData function TestCases"
        #region GetOverviewData section with TimeFrame Empty
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
            
            var result = ReportController.GetOverviewData(string.Empty, Enums.ViewByAllocated.Quarterly.ToString()) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_Overview", result.ViewName);
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
            var result = ReportController.GetOverviewData(_InvalidTimeFrame, Enums.ViewByAllocated.Quarterly.ToString()) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_Overview", result.ViewName);
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
            var result = ReportController.GetOverviewData("2015", string.Empty) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_Overview", result.ViewName);
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
            var result = ReportController.GetOverviewData("2015", _InValidIsQuarterly) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_Overview", result.ViewName);
        }
        #endregion
        #endregion

        #endregion

        #region "Revenue section of Report"
        #region "GetRevenueData function TestCases"
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
            List<int> lst = new List<int>();
            lst.Add(9775);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();

            var result = ReportController.GetRevenueData(string.Empty, Enums.ViewByAllocated.Quarterly.ToString()) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_Revenue", result.ViewName);
        }
        #endregion

        #region GetRevenueData section with Invalid TimeFrame
        /// <summary>
        /// GetRevenueData section with Invalid TimeFrame
        /// </summary>
        /// <auther>Viral Kadiya</auther>
        /// <createddate>19Jun2015</createddate>
        [TestMethod]
        public void GetRevenueData_TimeFrame_InValid()
        {
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            List<int> lst = new List<int>();
            lst.Add(9775);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string _InvalidTimeFrame = "InValid";

            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetRevenueData(_InvalidTimeFrame, Enums.ViewByAllocated.Quarterly.ToString()) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_Revenue", result.ViewName);
        }
        #endregion

        #region GetRevenueData section with IsQuarterly Empty
        /// <summary>
        ///  GetRevenueData section with IsQuarterly Empty
        /// </summary>
        /// <auther>Viral Kadiya</auther>
        /// <createddate>19Jun2015</createddate>
        [TestMethod]
        public void GetRevenueData_IsQuarterly_Empty()
        {
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            List<int> lst = new List<int>();
            lst.Add(9775);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ///Common.objCached.RevenueSparklineChartHeader = "Top {0} by";
            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetRevenueData("2015", string.Empty) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            List<int> lst = new List<int>();
            lst.Add(9775);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string _InValidIsQuarterly = "InValidQuarterly";
            //// Call GetRevenueData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetRevenueData("2015", _InValidIsQuarterly) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_Revenue", result.ViewName);
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
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            //int planId=DataHelper.GetPlanId();
            //// Call GetWaterFallData() function
            ReportController ReportController = new ReportController();

            var result = ReportController.GetWaterFallData() as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            //int planId=DataHelper.GetPlanId();
            //// Call GetWaterFallData() function
            ReportController ReportController = new ReportController();

            var result = ReportController.GetWaterFallData(string.Empty, Enums.ViewByAllocated.Quarterly.ToString()) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_ReportConversion", result.ViewName);
        }
        #endregion

        #region"GetWaterFallData section with Invalid timeframe"

        /// <summary>
        /// GetWaterFallData section with Invalid timeframe
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>11aug2015</createddate>
        /// 
        [TestMethod]
        public void GetWaterFallData_Timeframe_InvalidTimeframe()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            //// Call GetWaterFallData() function
            ReportController ReportController = new ReportController();
            string invalidTimeframe = "Invalid";
            var result = ReportController.GetWaterFallData(invalidTimeframe, Enums.ViewByAllocated.Quarterly.ToString()) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_ReportConversion", result.ViewName);
        }
        #endregion

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
            var result = ReportController.GetWaterFallData("2015", string.Empty) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            string invalidQuater = "_InValidIsQuarterly";
            ReportController ReportController = new ReportController();
            var result = ReportController.GetWaterFallData("2015", invalidQuater) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();
            var result = ReportController.GetWaterFallData(string.Empty, string.Empty) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();

            var result = ReportController.GetTopConversionToPlanByCustomFilter(string.Empty, "Campaign", "", "2015", Enums.ViewByAllocated.Quarterly.ToString(), Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(planId);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();
            string _InvalidParentbl = "_Invalid";
            var result = ReportController.GetTopConversionToPlanByCustomFilter(_InvalidParentbl, "Campaign", "", "2015", Enums.ViewByAllocated.Quarterly.ToString(), Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;
            ReportController ReportController = new ReportController();
            var result = ReportController.GetTopConversionToPlanByCustomFilter() as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();

            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, "", "", "2015", "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Tactic.ToString();
            ReportController ReportController = new ReportController();

            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, "", "", "2015", "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Program.ToString();
            ReportController ReportController = new ReportController();

            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, "", "", "2015", "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int planId = DataHelper.GetPlanId();
            List<int> lst = new List<int>();
            lst.Add(14936);
            HttpContext.Current.Session["ReportPlanIds"] = lst;

            string StrParentLabel = Enums.PlanEntity.Campaign.ToString();
            ReportController ReportController = new ReportController();

            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, "", "", "2015", "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, StrChildLabel, "", "2015", "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            var result = ReportController.GetTopConversionToPlanByCustomFilter(string.Empty, string.Empty, "", "2015", "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, StrChildLabel, "", "2015", "", Enums.InspectStage.MQL.ToString(), false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, StrChildLabel, "0", "2015", InvalidQuater, InvalidCode, false, null, false, null, null, 0) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            var result = ReportController.GetTopConversionToPlanByCustomFilter(StrParentLabel, StrChildLabel, "0", "2015", InvalidQuater, string.Empty, false, null, false, null, null, -1) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            CardSectionListModel = ReportController.GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, "2015", false, StrCampaign, false, "", 0);
            ReportController.TempData["ConverstionCardList"] = CardSectionListModel;
            //string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0
            var result = ReportController.SearchSortPaginataionConverstion() as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_ConversionCardSection", result.ViewName);
        }
        #endregion

        #region "SearchSortPaginataionConverstion with short by Revenue"
        /// <summary>
        /// Retrieve data to render view to check data with Short by Revenue.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionConverstion_Shortby_Revenue()
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
            CardSectionListModel = ReportController.GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, "2015", false, StrCampaign, false, "", 0);
            ReportController.TempData["ConverstionCardList"] = CardSectionListModel;
            string shortBy = Enums.SortByRevenue.Revenue.ToString();

            var result = ReportController.SearchSortPaginataionConverstion(0, 0, "", shortBy, "", "", "") as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_ConversionCardSection", result.ViewName);
        }
        #endregion

        #region "SearchSortPaginataionConverstion with short by Cost"
        /// <summary>
        /// Retrieve data to render view to check data with Short by Cost.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionConverstion_Shortby_Cost()
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
            CardSectionListModel = ReportController.GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, "2015", false, StrCampaign, false, "", 0);
            ReportController.TempData["ConverstionCardList"] = CardSectionListModel;
            string shortBy = Enums.SortByRevenue.Cost.ToString();

            var result = ReportController.SearchSortPaginataionConverstion(0, 0, "", shortBy, "", "", "") as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_ConversionCardSection", result.ViewName);
        }
        #endregion

        #region "SearchSortPaginataionConverstion with short by ROI"
        /// <summary>
        /// Retrieve data to render view to check data with Short by ROI.
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>13aug2015</createddate>
        ///SearchSortPaginataionConverstion
        [TestMethod]
        public void SearchSortPaginataionConverstion_Shortby_ROI()
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
            CardSectionListModel = ReportController.GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, "2015", false, StrCampaign, false, "", 0);
            ReportController.TempData["ConverstionCardList"] = CardSectionListModel;
            string shortBy = Enums.SortByRevenue.ROI.ToString();

            var result = ReportController.SearchSortPaginataionConverstion(0, 0, "", shortBy, "", "", "") as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            CardSectionListModel = ReportController.GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, "2015", false, StrCampaign, false, "", 0);
            ReportController.TempData["ConverstionCardList"] = CardSectionListModel;
            string shortBy = "_Invalid";

            var result = ReportController.SearchSortPaginataionConverstion(0, 0, "", shortBy, "", "", "") as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            CardSectionListModel = ReportController.GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, "2015", false, StrCampaign, false, "", 0);
            ReportController.TempData["ConverstionCardList"] = CardSectionListModel;
            string shortBy = "_Invalid";

            var result = ReportController.SearchSortPaginataionConverstion(0, 0, string.Empty, shortBy, "", "", "") as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ReportController ReportController = new ReportController();
            var result = ReportController.LoadReportCardSectionPartial() as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ReportController ReportController = new ReportController();
            var result = ReportController.LoadConverstionCardSectionPartial() as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_ConversionCardSection", result.ViewName);
        }
        #endregion

        #endregion
    }
}

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
        /// To check GetRevenueData function with no timeframeOption
        /// This test case return "Object Reference not set to an instance of an Object" error due to Common.objCached.RevenueSparklineChartHeader becomes null.
        /// Otherwise this test case successfully executed.
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

            var result = ReportController.GetRevenueData(string.Empty, Enums.ViewByAllocated.Quarterly.ToString()) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_Revenue", result.ViewName);
        }
        #endregion

        #region GetRevenueData section with Invalid TimeFrame
        /// <summary>
        /// To check GetOverviewData function with InValide value of timeframeOption
        /// This test case return "Object Reference not set to an instance of an Object" error due to Common.objCached.RevenueSparklineChartHeader becomes null.
        /// Otherwise this test case successfully executed.
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

            //Common.objCached.RevenueSparklineChartHeader = "Top {0} by";

            string _InvalidTimeFrame = "InValid";

            //// Call GetOverviewData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetRevenueData(_InvalidTimeFrame, Enums.ViewByAllocated.Quarterly.ToString()) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_Revenue", result.ViewName);
        }
        #endregion

        #region GetRevenueData section with IsQuarterly Empty
        /// <summary>
        /// To check GetOverviewData function with no timeframeOption
        /// This test case return "Object Reference not set to an instance of an Object" error due to Common.objCached.RevenueSparklineChartHeader becomes null.
        /// Otherwise this test case successfully executed.
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
            //// Call GetOverviewData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetRevenueData("2015", string.Empty) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_Revenue", result.ViewName);
        }
        #endregion

        #region GetRevenueData section with IsQuarterly InValid
        /// <summary>
        /// To check GetOverviewData function with no timeframeOption
        /// This test case return "Object Reference not set to an instance of an Object" error due to Common.objCached.RevenueSparklineChartHeader becomes null.
        /// Otherwise this test case successfully executed.
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
            ///Common.objCached.RevenueSparklineChartHeader = "Top {0} by";
            //// Call GetOverviewData() function
            ReportController ReportController = new ReportController();
            var result = ReportController.GetRevenueData("2015", _InValidIsQuarterly) as PartialViewResult;
            //// PartialViewResult shoud not be null and should match with Partial viewName
            Assert.AreEqual("_Revenue", result.ViewName);
        }
        #endregion
        #endregion
        #endregion
    }
}

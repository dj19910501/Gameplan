using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Routing;
using RevenuePlanner.Models;
using System.Web;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using System.Web.Mvc;
using RevenuePlanner.Test.MockHelpers;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Data;
using System.Xml;
using System.Data.OleDb;
using System.IO;
using System.Reflection;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class MeasureDashboardControllerTest
    {
        MeasureDashboardController objMeasureDashboardController = new MeasureDashboardController();
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            objMeasureDashboardController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(),

objMeasureDashboardController);
            objMeasureDashboardController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
        }
        /// <summary>
        /// Following is testcase to get report table.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Get_ReportTable()
        {
            string connectionString = "ReportExcelConn"; ;
            try
            {
                //Get Data in to Data Table from Excel
                OleDbConnection ExcelConnection; DataSet ds; OleDbDataAdapter Command;
                var ExcelPath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.FullName + "\\" +

System.Configuration.ConfigurationManager.AppSettings.Get(connectionString);
                var path = connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + ExcelPath + ";Extended Properties=\"Excel 8.0;HDR=YES;\"";
                ExcelConnection = new OleDbConnection(path);
                ExcelConnection.Open();
                Command = new OleDbDataAdapter("select * from " + "[Report$]", ExcelConnection);
                Command.TableMappings.Add("Table", "TestTable");
                ds = new DataSet();
                Command.Fill(ds);
                ExcelConnection.Close();
                if (ds.Tables[0] != null)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        //Check column is exist in databale or not, and get that fields value.
                        int reportTableId = 0;
                        if (ds.Tables[0].Columns.Contains("ReportTableId"))
                            reportTableId = Convert.ToInt32(dr["ReportTableId"]);
                        int dashBoardId = 0;
                        if (ds.Tables[0].Columns.Contains("DashBoardId"))
                            dashBoardId = Convert.ToInt32(dr["DashBoardId"]);
                        int dashBoardContentID = 0;
                        if (ds.Tables[0].Columns.Contains("DashBoardContentID"))
                            dashBoardContentID = Convert.ToInt32(dr["DashBoardContentID"]);
                        int dashboardPageId = 0;
                        if (ds.Tables[0].Columns.Contains("DashboardPageId"))
                            dashboardPageId = Convert.ToInt32(dr["DashboardPageId"]);

                        string dbconnectionString = string.Empty;
                        if (ds.Tables[0].Columns.Contains("ConnectionString"))
                            dbconnectionString = Convert.ToString(dr["ConnectionString"]);

                        string startDate = string.Empty;
                        if (ds.Tables[0].Columns.Contains("StartDate"))
                            startDate = Convert.ToString(dr["StartDate"]);

                        string endDate = string.Empty;
                        if (ds.Tables[0].Columns.Contains("EndDate"))
                            endDate = Convert.ToString(dr["EndDate"]);

                        string viewBy = string.Empty;
                        if (ds.Tables[0].Columns.Contains("ViewBy"))
                            viewBy = Convert.ToString(dr["ViewBy"]);
                        Console.WriteLine("Get  report table data.\n");
                        MeasureDashboardController objDashboard = new MeasureDashboardController();
                        objMeasureDashboardController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(),

objMeasureDashboardController);
                        //Set Connection string in UserApplicationId of session
                        List<RevenuePlanner.BDSService.User.ApplicationDetail> lstApplicationDetail = new List<RevenuePlanner.BDSService.User.ApplicationDetail>();
                        RevenuePlanner.BDSService.User.ApplicationDetail objApplicationDetail = new RevenuePlanner.BDSService.User.ApplicationDetail();
                        objApplicationDetail.ApplicationTitle = Enums.ApplicationCode.RPC.ToString();
                        objApplicationDetail.ConnectionString = dbconnectionString;
                        lstApplicationDetail.Add(objApplicationDetail);
                        Sessions.User.UserApplicationId = lstApplicationDetail;
                        var result = await objDashboard.GetReportTable(reportTableId, Enums.ApplicationCode.RPC.ToString(), "", null, false, viewBy, startDate, endDate,

dashBoardId, 0, dashBoardContentID) as string;
                        Assert.IsNotNull(result);
                        Assert.IsTrue(result.Contains(string.Format("{0}{1}", "ReportTable", dashBoardContentID)));
                        Console.WriteLine("MeasureDashboardController - LoadReportTablePartial" + " \n The Assert Value is :  " + result);
                    }
                }

            }
            catch (Exception e)
            {
                Assert.Fail();
                Console.WriteLine("MeasureDashboardController - LoadReportTablePartial" + " \n The Assert Value is :  " + e.Message);
            }

        }


        /// <summary>
        /// To Get Drill Down Data
        /// <author>Nandish Shah</author>
        [TestMethod]
        public void LoadDrillDownData_With_Empty_Parameter()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Drill Down Data.\n");
            MRPEntities db = new MRPEntities();
            objMeasureDashboardController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objMeasureDashboardController.Url = new UrlHelper(
            new RequestContext(
            objMeasureDashboardController.HttpContext, new RouteData()
            ),
            routes
            );
            var result = objMeasureDashboardController.LoadDrillDownData("", 0, "", "", "", "", "", "", "", "", "", "", 0, false, "", "") as Task<PartialViewResult>;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
        }

        /// <summary>
        /// To Get Drill Down Data
        /// <author>Nandish Shah</author>
        [TestMethod]
        public void LoadDrillDownData_With_Parameter()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Drill Down Data.\n");
            MRPEntities db = new MRPEntities();
            objMeasureDashboardController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objMeasureDashboardController.Url = new UrlHelper(
            new RequestContext(
            objMeasureDashboardController.HttpContext, new RouteData()
            ),
            routes
            );
            var result = objMeasureDashboardController.LoadDrillDownData("Test", 1, "Inquiries Generated", "Q3-2014", "12,261.00", "12261", "Enterprise", "1", "1", "", "0", "0", 0, false, "desc", "") as Task<PartialViewResult>;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
        }

        /// <summary>
        /// To Load Drill Down Table
        /// <author>Nandish Shah</author>
        [TestMethod]
        public void GetDrillDownReportTable_With_Empty_Parameter()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Drill Down Data.\n");
            MRPEntities db = new MRPEntities();
            objMeasureDashboardController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objMeasureDashboardController.Url = new UrlHelper(
            new RequestContext(
            objMeasureDashboardController.HttpContext, new RouteData()
            ),
            routes
            );
            var result = objMeasureDashboardController.GetDrillDownReportTable(null, null, "", 0, "", "", "", "", "", 0, 0, "", "", 0, "0", "") as Task<ActionResult>;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
        }

        /// <summary>
        /// To Load Drill Down Table
        /// <author>Nandish Shah</author>
        [TestMethod]
        public void GetDrillDownReportTable_With_Parameter()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Drill Down Data.\n");
            MRPEntities db = new MRPEntities();
            objMeasureDashboardController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objMeasureDashboardController.Url = new UrlHelper(
            new RequestContext(
            objMeasureDashboardController.HttpContext, new RouteData()
            ),
            routes
            );
            string[] SelectedOthersDimension = new string[] { "59", "46" };
            string[] SelectedDimensionValue = new string[] { "59:612", "46:9" };
            var result = objMeasureDashboardController.GetDrillDownReportTable(SelectedOthersDimension, SelectedDimensionValue, "Test", 19, "Q3-2014", "Enterprise", "Campaign", "", "", 0, 10, "0", "", 13, "0", "0") as Task<ActionResult>;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
        }
        /// <summary>
        /// Following is testcase to get chart.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Get_Chart()
        {

            try
            {
                //Get Data in to Data Table from Excel
                DataSet ds = ReturnDataSetFromExcel("ReportExcelConn", "Report");

                if (ds.Tables[0] != null)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        //Check column is exist in databale or not, and get that fields value.
                        int reportGraphId = 0;
                        if (ds.Tables[0].Columns.Contains("ReportGraphId"))
                            reportGraphId = Convert.ToInt32(dr["ReportGraphId"]);
                        int dashBoardId = 0;
                        if (ds.Tables[0].Columns.Contains("DashBoardId"))
                            dashBoardId = Convert.ToInt32(dr["DashBoardId"]);
                        int dashBoardContentID = 0;
                        if (ds.Tables[0].Columns.Contains("DashBoardContentID"))
                            dashBoardContentID = Convert.ToInt32(dr["DashBoardContentID"]);
                        int dashboardPageId = 0;
                        if (ds.Tables[0].Columns.Contains("DashboardPageId"))
                            dashboardPageId = Convert.ToInt32(dr["DashboardPageId"]);

                        string dbconnectionString = string.Empty;
                        if (ds.Tables[0].Columns.Contains("ConnectionString"))
                            dbconnectionString = Convert.ToString(dr["ConnectionString"]);

                        string startDate = string.Empty;
                        if (ds.Tables[0].Columns.Contains("StartDate"))
                            startDate = Convert.ToString(dr["StartDate"]);

                        string endDate = string.Empty;
                        if (ds.Tables[0].Columns.Contains("EndDate"))
                            endDate = Convert.ToString(dr["EndDate"]);

                        string viewBy = string.Empty;
                        if (ds.Tables[0].Columns.Contains("ViewBy"))
                            viewBy = Convert.ToString(dr["ViewBy"]);
                        string chartType = string.Empty;
                        if (ds.Tables[0].Columns.Contains("chartType"))
                            chartType = Convert.ToString(dr["chartType"]);
                        Console.WriteLine("Get  report table data.\n");
                        MeasureDashboardController objDashboard = new MeasureDashboardController();
                        objMeasureDashboardController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(),

objMeasureDashboardController);
                        //Set Connection string in UserApplicationId of session
                        List<RevenuePlanner.BDSService.User.ApplicationDetail> lstApplicationDetail = new List<RevenuePlanner.BDSService.User.ApplicationDetail>();
                        RevenuePlanner.BDSService.User.ApplicationDetail objApplicationDetail = new RevenuePlanner.BDSService.User.ApplicationDetail();
                        objApplicationDetail.ApplicationTitle = Enums.ApplicationCode.RPC.ToString();
                        objApplicationDetail.ConnectionString = dbconnectionString;
                        lstApplicationDetail.Add(objApplicationDetail);
                        Sessions.User.UserApplicationId = lstApplicationDetail;
                        JsonResult result = await objDashboard.GetChart(reportGraphId, Enums.ApplicationCode.RPC.ToString(), "", null, false, viewBy, startDate, endDate, true, false) as JsonResult;                       
                        Assert.IsTrue(Convert.ToString(result.Data).Contains("type: '"+ chartType + "'"));                        
                        Console.WriteLine("MeasureDashboardController - GetChart" + " \n The Assert Value is :  " + result);
                    }
                }

            }
            catch (Exception e)
            {
                Assert.Fail();
                Console.WriteLine("MeasureDashboardController - GetChart" + " \n The Assert Value is :  " + e.Message);
            }

        }
        /// <summary>
        /// Following method returns Dataser from excel.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        private DataSet ReturnDataSetFromExcel(string connectionString, string sheetName)
        {
            DataSet ds = new DataSet();
            try
            {
               
                OleDbConnection ExcelConnection; OleDbDataAdapter Command;
                string ExcelPath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.FullName + "\\" +
    System.Configuration.ConfigurationManager.AppSettings.Get(connectionString);
                string path = connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + ExcelPath + ";Extended Properties=\"Excel 8.0;HDR=YES;\"";
                ExcelConnection = new OleDbConnection(path);
                ExcelConnection.Open();
                Command = new OleDbDataAdapter("select * from " + "[" + sheetName + "$]", ExcelConnection);
                Command.TableMappings.Add("Table", "TestTable");
                Command.Fill(ds);
                ExcelConnection.Close();
                return ds;
            }
            catch(Exception)
            {
                return ds;
            }
        }


    }
}



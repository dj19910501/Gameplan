
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
using static RevenuePlanner.BDSService.User;
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
        //  public TestContext TestContext { get; set; }
        MeasureDashboardController objMeasureDashboiardController = new MeasureDashboardController();
        [TestInitialize]

        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            objMeasureDashboiardController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(),

objMeasureDashboiardController);
            objMeasureDashboiardController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
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
                        objMeasureDashboiardController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(),

objMeasureDashboiardController);
                        //Set Connection string in UserApplicationId of session
                        List<ApplicationDetail> lstApplicationDetail = new List<ApplicationDetail>();
                        ApplicationDetail objApplicationDetail = new ApplicationDetail();
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
    }
}



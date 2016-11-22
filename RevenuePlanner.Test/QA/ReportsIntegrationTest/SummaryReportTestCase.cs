using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.IntegrationHelpers;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RevenuePlanner.Test.QA.ReportsIntegrationTest
{
    [TestClass]
    public class SummaryReportTestCase
    {
        #region Variable Declaration

        Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();
        PlanCommonFunctions ObjPlanCommonFunctions = new PlanCommonFunctions();

        static string currentYear = DateTime.Now.Year.ToString();

        static decimal INQGoal = 0; static decimal INQTotalProjected = 0; static decimal INQPercentage = 0;
        static decimal MQLGoal = 0; static decimal MQLTotalProjected = 0; static decimal MQLPercentage = 0;
        static decimal CWGoal = 0; static decimal CWTotalProjected = 0; static decimal CWPercentage = 0;

        static List<double> ActualList; static List<double> ProjectedList; static List<double> GoalList;

        ReportController objReportController; ReportModel objReportModel; ReportOverviewModel objReportOverviewModel; RevenueDataTable objReportDataTable;
        RevenueToPlanModel objRevenueToPlanModel; ConversionToPlanModel objConversionToPlanModel; CardSectionModel objCardSection;
        ConversionDataTable objConversionDataTable; ConversionSubDataTableModel SubDataTableModel; Projected_Goal objProjected_Goal;

        #endregion

        [TestMethod]
        [Priority(1)]
        public async Task INQSummary()
        {
            try
            {
                //Call common function for login
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Assert.AreEqual("Index", IsLogin.RouteValues["Action"]);
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value of Action : " + IsLogin.RouteValues["Action"]);
                    ObjPlanCommonFunctions.SetSessionData();
                    SetValuesForReport(currentYear, "Monthly", "ProjectedStageValue");
                    decimal sumOfActual = 0; decimal sumOfProjected = 0;

                    Sessions.PlanExchangeRate = 1.0;
                    var result = await objReportController.GetOverviewData("2016", "Quarterly") as PartialViewResult;
                    objReportOverviewModel = new ReportOverviewModel();
                    objReportOverviewModel = (ReportOverviewModel)result.Model;
                    if (objReportOverviewModel != null)
                    {
                        objProjected_Goal = new Projected_Goal();
                        objProjected_Goal = objReportOverviewModel.conversionOverviewModel.Projected_LineChartList[0].projected_goal;

                        #region Actual Projected
                        foreach (double actual in ActualList)
                        {
                            sumOfActual = sumOfActual + Convert.ToDecimal(actual);
                        }
                        foreach (double projected in ProjectedList)
                        {
                            sumOfProjected = sumOfProjected + Convert.ToDecimal(projected);
                        }
                        INQTotalProjected = sumOfActual + sumOfProjected;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Actual_Projected), 2), Math.Round(INQTotalProjected, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected INQ " + objProjected_Goal.Actual_Projected + ".");
                        #endregion

                        #region Actual Goal
                        DataTable dtModel = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Model$]").Tables[0];
                        DataRow drModel = dtModel.Rows[0];

                        INQGoal = Convert.ToDecimal(drModel["TACTIC_PROJECTED_STAGE"].ToString());
                     
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Goal), 2), Math.Round(INQGoal, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected INQ goal " + objProjected_Goal.Goal + ".");

                        #endregion

                        #region Actual Percentage

                        INQPercentage = ((INQTotalProjected - INQGoal) / INQGoal) * 100;

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Percentage), 2), Math.Round(INQPercentage, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected MQL percentage " + objProjected_Goal.Goal + ".");

                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [TestMethod]
        [Priority(2)]
        public async Task MQLSummary()
        {
            try
            {
                //Call common function for login
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Assert.AreEqual("Index", IsLogin.RouteValues["Action"]);
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value of Action : " + IsLogin.RouteValues["Action"]);
                    ObjPlanCommonFunctions.SetSessionData();
                    SetValuesForReport(currentYear, "Monthly", "MQL");
                    decimal sumOfActual = 0; decimal sumOfProjected = 0;

                    Sessions.PlanExchangeRate = 1.0;
                    var result = await objReportController.GetOverviewData("2016", "Quarterly") as PartialViewResult;
                    objReportOverviewModel = new ReportOverviewModel();
                    objReportOverviewModel = (ReportOverviewModel)result.Model;
                    if (objReportOverviewModel != null)
                    {
                        objProjected_Goal = new Projected_Goal();
                        objProjected_Goal = objReportOverviewModel.conversionOverviewModel.Projected_LineChartList[1].projected_goal;

                        #region Actual Projected
                        foreach (double actual in ActualList)
                        {
                            sumOfActual = sumOfActual + Convert.ToDecimal(actual);
                        }
                        foreach (double projected in ProjectedList)
                        {
                            sumOfProjected = sumOfProjected + Convert.ToDecimal(projected);
                        }
                        MQLTotalProjected = sumOfActual + sumOfProjected;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Actual_Projected), 2), Math.Round(MQLTotalProjected, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected MQL " + objProjected_Goal.Actual_Projected + ".");
                        #endregion

                        #region Actual Goal
                        DataTable dtModel = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Model$]").Tables[0];
                        DataRow drModel = dtModel.Rows[0];

                        INQGoal = Convert.ToDecimal(drModel["TACTIC_PROJECTED_STAGE"].ToString());
                        MQLGoal = ObjPlanCommonFunctions.CalculationForTactic(drModel, INQGoal, "TQL");

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Goal), 2), Math.Round(MQLGoal, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected MQL goal " + objProjected_Goal.Goal + ".");

                        #endregion

                        #region Actual Percentage

                        MQLPercentage = ((MQLTotalProjected - MQLGoal) / MQLGoal) * 100;

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Percentage), 2), Math.Round(MQLPercentage, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected MQL percentage " + objProjected_Goal.Goal + ".");

                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [TestMethod]
        [Priority(3)]
        public async Task CWSummary()
        {
            try
            {
                //Call common function for login
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Assert.AreEqual("Index", IsLogin.RouteValues["Action"]);
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value of Action : " + IsLogin.RouteValues["Action"]);
                    ObjPlanCommonFunctions.SetSessionData();
                    SetValuesForReport(currentYear, "Monthly", "CW");
                    decimal sumOfActual = 0; decimal sumOfProjected = 0;

                    Sessions.PlanExchangeRate = 1.0;
                    var result = await objReportController.GetOverviewData("2016", "Quarterly") as PartialViewResult;
                    objReportOverviewModel = new ReportOverviewModel();
                    objReportOverviewModel = (ReportOverviewModel)result.Model;
                    if (objReportOverviewModel != null)
                    {
                        objProjected_Goal = new Projected_Goal();
                        objProjected_Goal = objReportOverviewModel.conversionOverviewModel.Projected_LineChartList[2].projected_goal;

                        #region Actual Projected
                        foreach (double actual in ActualList)
                        {
                            sumOfActual = sumOfActual + Convert.ToDecimal(actual);
                        }
                        foreach (double projected in ProjectedList)
                        {
                            sumOfProjected = sumOfProjected + Convert.ToDecimal(projected);
                        }
                        CWTotalProjected = sumOfActual + sumOfProjected;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Actual_Projected), 2), Math.Round(CWTotalProjected, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected CW is " + objProjected_Goal.Actual_Projected + ".");
                        #endregion

                        #region Actual Goal
                        DataTable dtModel = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Model$]").Tables[0];
                        DataRow drModel = dtModel.Rows[0];

                        INQGoal = Convert.ToDecimal(drModel["TACTIC_PROJECTED_STAGE"].ToString());
                        MQLGoal = ObjPlanCommonFunctions.CalculationForTactic(drModel, INQGoal, "TQL");
                        CWGoal = ObjPlanCommonFunctions.CalculationForTactic(drModel, MQLGoal, "CW");

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Goal), 2), Math.Round(CWGoal, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected CW goal is " + objProjected_Goal.Goal + ".");

                        #endregion

                        #region Actual Percentage

                        CWPercentage = ((CWTotalProjected - CWGoal) / CWGoal) * 100;

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Percentage), 2), Math.Round(CWPercentage, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected CW percentage is " + objProjected_Goal.Goal + ".");

                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SetValuesForReport(string Year, string isQuarterly, string Stage)
        {
            objReportController = new ReportController();
            objReportModel = new ReportModel();
            objConversionToPlanModel = new ConversionToPlanModel();
            SubDataTableModel = new ConversionSubDataTableModel();

            objProjected_Goal = new Projected_Goal();
            objCardSection = new CardSectionModel();
            ObjPlanCommonFunctions.SetSessionData();

            var result1 = objReportController.GetTopConversionToPlanByCustomFilter("Campaign", "", "", currentYear, isQuarterly, Stage) as PartialViewResult;

            objConversionToPlanModel = (ConversionToPlanModel)(result1.ViewData.Model);
            objConversionDataTable = objConversionToPlanModel.ConversionToPlanDataTableModel;
            SubDataTableModel = objConversionDataTable.SubDataModel;

            objProjected_Goal = objConversionToPlanModel.RevenueHeaderModel;

            if (isQuarterly == "Monthly")
            {
                ActualList = objConversionDataTable.ActualList;
                ProjectedList = objConversionDataTable.ProjectedList;
                GoalList = objConversionDataTable.GoalList;
            }
        }



    }
}

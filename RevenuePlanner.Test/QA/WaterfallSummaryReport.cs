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

namespace RevenuePlanner.Test.QA
{
    [TestClass]
    public class WaterfallSummaryReport
    {
        #region Variable Declaration

        Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();
        PlanCommonFunctions ObjPlanCommonFunctions = new PlanCommonFunctions();

        static string currentYear = DateTime.Now.Year.ToString();

        static decimal INQGoal = 0; static decimal INQTotalProjected = 0; static decimal INQPercentage = 0;
        static decimal TQLGoal = 0; static decimal TQLTotalProjected = 0; static decimal TQLPercentage = 0;
        static decimal CWGoal = 0; static decimal CWTotalProjected = 0; static decimal CWPercentage = 0;
       
        static List<double> ActualList; static List<double> ProjectedList; static List<double> GoalList;


        ReportController objReportController; ReportModel objReportModel; ReportOverviewModel objReportOverviewModel; RevenueDataTable objReportDataTable;
        RevenueToPlanModel objRevenueToPlanModel; ConversionToPlanModel objConversionToPlanModel; CardSectionModel objCardSection;
        ConversionDataTable objConversionDataTable; ConversionSubDataTableModel SubDataTableModel; Projected_Goal objProjected_Goal; Conversion_Benchmark_Model objConversion_Benchmark_Model;
        DataTable dt;
        #endregion


        [TestMethod]
        [Priority(1)]
        public async Task INQSummaryTestCase()
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
                    var result = await objReportController.GetOverviewData(currentYear, "Quarterly") as PartialViewResult;
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
                        Console.WriteLine("ReportController -> GetOverviewData \n Report - Waterfall Summary Report \n The assert value of projected INQ in volume section " + objProjected_Goal.Actual_Projected + ".");
                        #endregion

                        #region Actual Goal
                        DataTable dtModel = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Model$]").Tables[0];
                        DataRow drModel = dtModel.Rows[0];

                        INQGoal = Convert.ToDecimal(drModel["TACTIC_PROJECTED_STAGE"].ToString());

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Goal), 2), Math.Round(INQGoal, 2));
                        Console.WriteLine("ReportController -> GetOverviewData \n Report - Waterfall Summary Report \n The assert value of projected INQ goal in volume section " + objProjected_Goal.Goal + ".");

                        #endregion

                        #region Actual Percentage

                        INQPercentage = ((INQTotalProjected - INQGoal) / INQGoal) * 100;

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Percentage), 2), Math.Round(INQPercentage, 2));
                        Console.WriteLine("ReportController -> GetOverviewData \n Report - Waterfall Summary Report \n The assert value of projected INQ percentage in volume section " + objProjected_Goal.Percentage + ".");

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
        public async Task TQLSummaryTestCase()
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
                    var result = await objReportController.GetOverviewData(currentYear, "Quarterly") as PartialViewResult;
                    objReportOverviewModel = new ReportOverviewModel();
                    objReportOverviewModel = (ReportOverviewModel)result.Model;
                    if (objReportOverviewModel != null)
                    {
                        objProjected_Goal = new Projected_Goal();
                        objProjected_Goal = objReportOverviewModel.conversionOverviewModel.Projected_LineChartList[1].projected_goal;
                        objConversion_Benchmark_Model = new Conversion_Benchmark_Model();
                        objConversion_Benchmark_Model = objReportOverviewModel.conversionOverviewModel.Projected_LineChartList[1].Stage_Benchmark;

                        #region Actual Projected
                        foreach (double actual in ActualList)
                        {
                            sumOfActual = sumOfActual + Convert.ToDecimal(actual);
                        }
                        foreach (double projected in ProjectedList)
                        {
                            sumOfProjected = sumOfProjected + Convert.ToDecimal(projected);
                        }
                        TQLTotalProjected = sumOfActual + sumOfProjected;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Actual_Projected), 2), Math.Round(TQLTotalProjected, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected TQL " + objProjected_Goal.Actual_Projected + ".");
                        #endregion

                        #region Actual Goal
                        DataTable dtModel = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Model$]").Tables[0];
                        DataRow drModel = dtModel.Rows[0];

                        INQGoal = Convert.ToDecimal(drModel["TACTIC_PROJECTED_STAGE"].ToString());
                        TQLGoal = ObjPlanCommonFunctions.CalculationForTactic(drModel, INQGoal, "TQL");

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Goal), 2), Math.Round(TQLGoal, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected TQL goal " + objProjected_Goal.Goal + ".");

                        #endregion

                        #region Actual Percentage

                        TQLPercentage = ((TQLTotalProjected - TQLGoal) / TQLGoal) * 100;

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Percentage), 2), Math.Round(TQLPercentage, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected TQL percentage " + objProjected_Goal.Percentage + ".");

                        #endregion

                        #region TQL Stage Volume

                        decimal TQLConversionRate = CalculateConversionValues("TQL");
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objConversion_Benchmark_Model.stageVolume), 2), Math.Round(TQLConversionRate, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected TQL percentage " + objConversion_Benchmark_Model.stageVolume + ".");

                        #endregion

                        #region TQL Stage Goal

                        decimal TQLConversionGoal = (TQLGoal / INQGoal) * 100;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objConversion_Benchmark_Model.Benchmark), 2), Math.Round(TQLConversionGoal, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected TQL goal percentage " + objConversion_Benchmark_Model.Benchmark + ".");

                        #endregion

                        #region TQL Stage Percentage Difference

                        decimal TQLPercentageDiff = ((TQLConversionRate - TQLConversionGoal) / TQLConversionGoal) * 100;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objConversion_Benchmark_Model.PercentageDifference), 2), Math.Round(TQLPercentageDiff, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected TQL difference percentage " + objConversion_Benchmark_Model.PercentageDifference + ".");

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
        public async Task CWSummaryTestCase()
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
                    var result = await objReportController.GetOverviewData(currentYear, "Quarterly") as PartialViewResult;
                    objReportOverviewModel = new ReportOverviewModel();
                    objReportOverviewModel = (ReportOverviewModel)result.Model;
                    if (objReportOverviewModel != null)
                    {
                        objProjected_Goal = new Projected_Goal();
                        objProjected_Goal = objReportOverviewModel.conversionOverviewModel.Projected_LineChartList[2].projected_goal;

                        objConversion_Benchmark_Model = new Conversion_Benchmark_Model();
                        objConversion_Benchmark_Model = objReportOverviewModel.conversionOverviewModel.Projected_LineChartList[2].Stage_Benchmark;

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
                        TQLGoal = ObjPlanCommonFunctions.CalculationForTactic(drModel, INQGoal, "TQL");
                        CWGoal = ObjPlanCommonFunctions.CalculationForTactic(drModel, TQLGoal, "CW");

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Goal), 2), Math.Round(CWGoal, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected CW goal is " + objProjected_Goal.Goal + ".");

                        #endregion

                        #region Actual Percentage

                        CWPercentage = ((CWTotalProjected - CWGoal) / CWGoal) * 100;

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objProjected_Goal.Percentage), 2), Math.Round(CWPercentage, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected CW percentage is " + objProjected_Goal.Percentage + ".");

                        #endregion

                        #region CW Stage Volume

                        decimal CWConversionRate = CalculateConversionValues("CW");
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objConversion_Benchmark_Model.stageVolume), 2), Math.Round(CWConversionRate, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected CW percentage " + objConversion_Benchmark_Model.stageVolume + ".");

                        #endregion

                        #region CW Stage Goal

                        decimal CWConversionGoal = (CWGoal / TQLGoal) * 100;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objConversion_Benchmark_Model.Benchmark), 2), Math.Round(CWConversionGoal, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected CW goal percentage" + objConversion_Benchmark_Model.Benchmark + ".");

                        #endregion

                        #region CW Stage Percentage Difference

                        decimal CWPercentageDiff = ((CWConversionRate - CWConversionGoal) / CWConversionGoal) * 100;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objConversion_Benchmark_Model.PercentageDifference), 2), Math.Round(CWPercentageDiff, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected CW difference percentage " + objConversion_Benchmark_Model.PercentageDifference + ".");

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

        public decimal CalculateConversionValues(string calculateFor)
        {
            decimal sumOfActualOOCW = 0; decimal sumOfTQL = 0;
            dt = new DataTable();
            dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
            DataTable dtTactic = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Tactic$]").Tables[0];
            DataRow drTactic = dtTactic.Rows[0];
            DateTime TacticEndDate = Convert.ToDateTime(drTactic["TacticEndDate"].ToString());
            for (int i = 1; i <= dt.Columns.Count - 1; i++)
            {
                if (i <= TacticEndDate.Month)
                {
                    sumOfTQL = sumOfTQL + Convert.ToDecimal(dt.Rows[1][i].ToString());
                }
            }
            if (calculateFor == "TQL")
            {
                for (int i = 1; i <= dt.Columns.Count - 1; i++)
                {
                    if (i <= TacticEndDate.Month)
                    {
                        sumOfActualOOCW = sumOfActualOOCW + Convert.ToDecimal(dt.Rows[0][i].ToString());
                    }
                }
                return ((sumOfTQL * 100) / sumOfActualOOCW);
            }
            else
            {
                for (int i = 1; i <= dt.Columns.Count - 1; i++)
                {
                    if (i <= TacticEndDate.Month)
                    {
                        sumOfActualOOCW = sumOfActualOOCW + Convert.ToDecimal(dt.Rows[2][i].ToString());
                    }
                }
                return ((sumOfActualOOCW * 100) / sumOfTQL);
            }


        }


    }
}

﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class SummaryReport
    {
        #region Variable Declaration

        Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();
        PlanCommonFunctions ObjPlanCommonFunctions = new PlanCommonFunctions();

        static string currentYear = DateTime.Now.Year.ToString(); int currentMonth = DateTime.Now.Month; static decimal TacticRevenueAmount = 0;

        static decimal INQGoal = 0; static decimal INQTotalProjected = 0; static decimal INQPercentage = 0;
        static decimal TQLGoal = 0; static decimal TQLTotalProjected = 0; static decimal TQLPercentage = 0;
        static decimal CWGoal = 0; static decimal CWTotalProjected = 0; static decimal CWPercentage = 0;
        static decimal TotalProjected = 0; decimal SumOfActual = 0; decimal SumOfProjected = 0;
        static List<double> ActualList; static List<double> ProjectedList; static List<double> GoalList;
        static decimal SumOfCost = 0;

        ReportController objReportController; ReportModel objReportModel; ReportOverviewModel objReportOverviewModel;
        CardSectionModel objCardSection; ConversionToPlanModel objConversionToPlanModel;
        ConversionDataTable objConversionDataTable; ConversionSubDataTableModel SubDataTableModel; Projected_Goal objProjected_Goal; Conversion_Benchmark_Model objConversion_Benchmark_Model;
        DataTable dt;
        #endregion

        #region Waterfall Report

        [TestMethod]
        public async Task INQSummaryReport()
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
        public async Task TQLSummaryReport()
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
        public async Task CWSummaryReport()
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
                        Assert.AreEqual(objConversion_Benchmark_Model.stageVolume, Math.Floor(CWConversionRate).ToString());
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected CW percentage " + objConversion_Benchmark_Model.stageVolume + ".");

                        #endregion

                        #region CW Stage Goal

                        decimal CWConversionGoal = (CWGoal / TQLGoal) * 100;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(objConversion_Benchmark_Model.Benchmark), 2), Math.Round(CWConversionGoal, 2));
                        Console.WriteLine("ReportController - GetOverviewData \n The assert value of projected CW goal percentage" + objConversion_Benchmark_Model.Benchmark + ".");

                        #endregion

                        #region CW Stage Percentage Difference

                        decimal CWPercentageDiff = ((Math.Floor(CWConversionRate) - CWConversionGoal) / CWConversionGoal) * 100;
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
            DateTime NewEndDate = Convert.ToDateTime(drTactic["TQLEndDate"].ToString());
            for (int i = 1; i <= dt.Columns.Count - 1; i++)
            {
                if (i <= NewEndDate.Month)
                {
                    sumOfTQL = sumOfTQL + Convert.ToDecimal(dt.Rows[1][i].ToString());
                }
            }
            if (calculateFor == "TQL")
            {
                for (int i = 1; i <= dt.Columns.Count - 1; i++)
                {
                    if (i <= NewEndDate.Month)
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
                    if (i <= NewEndDate.Month)
                    {
                        sumOfActualOOCW = sumOfActualOOCW + Convert.ToDecimal(dt.Rows[2][i].ToString());
                    }
                }
                return ((sumOfActualOOCW * 100) / sumOfTQL);
            }


        }

        #endregion

        #region Revenue Report

        [TestMethod()]
        public async Task RevenueSummaryReport()
        {
            try
            {
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Assert.AreEqual("Index", IsLogin.RouteValues["Action"]);
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value of Action : " + IsLogin.RouteValues["Action"]);
                    ObjPlanCommonFunctions.SetSessionData();
                    objReportController = new ReportController();
                    objReportModel = new ReportModel();

                    Sessions.PlanExchangeRate = 1.0;

                    var result1 = objReportController.GetRevenueData(currentYear, "Quarterly") as PartialViewResult;
                    objReportModel = (ReportModel)(result1.ViewData.Model);
                    RevenueToPlanModel objRevenueToPlanModel = objReportModel.RevenueToPlanModel;
                    RevenueDataTable objReportDataTable = objRevenueToPlanModel.RevenueToPlanDataModel;
                    List<double> QuaterlyActualList = objReportDataTable.ActualList;
                    List<double> QuaterlyProjectedList = objReportDataTable.ProjectedList;
                    List<double> QuaterlyGoalList = objReportDataTable.GoalList;
                    
                    var result = await objReportController.GetOverviewData(currentYear, "Quarterly") as PartialViewResult;
                    objReportOverviewModel = new ReportOverviewModel();
                    objReportOverviewModel = (ReportOverviewModel)result.Model;

                    List<sparkLineCharts> SparkLineChartsList = new List<sparkLineCharts>();
                    lineChartData objlineChartData = new lineChartData();
                    SparkLineChartsList = objReportOverviewModel.revenueOverviewModel.SparkLineChartsData;
                    objlineChartData = objReportOverviewModel.revenueOverviewModel.linechartdata;
                  
                    if (objReportOverviewModel != null)
                    { 
                        #region Verify Data
                        objProjected_Goal = new Projected_Goal();
                        objProjected_Goal = objReportOverviewModel.revenueOverviewModel.projected_goal;

                        DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
                        TotalProjected = SetValuesForSummeryRevenueReport(dt);
                        Console.WriteLine("Testing revenueOverviewModel Method");
                        Assert.AreEqual(Math.Round(TotalProjected, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.Actual_Projected), 2));
                        Console.WriteLine("\n The assert value of actual projected is " + Math.Round(Convert.ToDecimal(objProjected_Goal.Actual_Projected), 2).ToString() + ". (The expected result is " + Math.Round(TotalProjected,2).ToString() + ".)");
            
                        Assert.AreEqual(Math.Round(TacticRevenueAmount, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.Goal), 2));
                        Console.WriteLine("\n The assert value of actual goal is  " + Math.Round(Convert.ToDecimal(objProjected_Goal.Goal), 2).ToString() + ". (The expected result is " + Math.Round(TacticRevenueAmount,2) + ".)");

                        decimal Percentage = ((TotalProjected - TacticRevenueAmount) / TacticRevenueAmount) * 100;

                        Assert.AreEqual(Math.Round(Percentage, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.Percentage), 2));
                        Console.WriteLine("\n The assert value of actual percentage  is " + Math.Round(Convert.ToDecimal(objProjected_Goal.Percentage), 2) + ". (The expected result is " + Math.Round(Percentage, 2) + ".)");
                       
                  
                        if (SparkLineChartsList != null)
                        {
                            decimal value = 0;
                            for (int i = 0; i < SparkLineChartsList.Count; i++)
                            {
                                var sparkLineData = SparkLineChartsList[i];
                                string attributeName1 = sparkLineData.sparklinechartdata[0].Name.ToLower();
                                string percentage = sparkLineData.sparklinechartdata[0].RevenueTypeValue;
                                string Totalpercentage = sparkLineData.sparklinechartdata[1].RevenueTypeValue.ToLower();
                                string attributeName2 = sparkLineData.sparklinechartdata[1].RevenueTypeValue.ToLower();
                                string Header = sparkLineData.ChartHeader;
                                if (sparkLineData.ChartHeader.ToLower() == "top revenue by")
                                {
                                    value = SumOfActual;

                                }
                                else if (sparkLineData.ChartHeader.ToLower() == "top cost by")
                                {

                                    if (dt.Rows[4] != null)
                                    {
                                        for (int j = 1; j <= dt.Columns.Count - 1; j++)
                                        {
                                            if (j <= currentMonth)
                                            {
                                                SumOfCost = SumOfCost + Convert.ToDecimal(dt.Rows[4][j].ToString());
                                            }
                                        }
                                    }
                                    value = SumOfCost;

                                }
                                else if (sparkLineData.ChartHeader.ToLower() == "top performance by")
                                {
                                    value = ((SumOfActual - TacticRevenueAmount) / TacticRevenueAmount) * 100;

                                }
                                else if (sparkLineData.ChartHeader.ToLower() == "top roi by")
                                {
                                    value = ((SumOfActual - SumOfCost) / SumOfCost) * 100;

                                }
                                if (percentage.Contains('%'))
                                {
                                    percentage = percentage.Replace('%', '0');
                                    Totalpercentage = Totalpercentage.Replace('%', '0');
                                }
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 1), Math.Round(Convert.ToDecimal(value), 1));
                                Console.WriteLine("\n The assert value of " + attributeName1 + " in " + Header + " is " + Math.Round(Convert.ToDecimal(percentage), 2) + ".");
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(Totalpercentage), 1), Math.Round(Convert.ToDecimal(value), 1));
                                Console.WriteLine("\n The assert value of " + attributeName2 + " in " + Header + " is " + Math.Round(Convert.ToDecimal(Totalpercentage), 2) + ".");
                            }
                        }
                        #endregion
                        
                        #region Verify Graph
                        if (objlineChartData != null)
                        {
                            List<double?> ActualList = objlineChartData.series[0].data;
                            List<double?> GoalList = objlineChartData.series[1].data;
                            if(ActualList != null && ActualList.Count > 0)
                            {
                                decimal sumOfAcual = 0;
                                for (int i = 0; i <= ActualList.Count - 1; i++)
                                {
                                    sumOfAcual = sumOfAcual + Convert.ToDecimal(QuaterlyActualList[i]) + Convert.ToDecimal(QuaterlyProjectedList[i]);
                                    Assert.AreEqual(Math.Round(Convert.ToDecimal(ActualList[i]), 2), Math.Round(sumOfAcual, 2));
                                    Console.WriteLine("ReportController - revenueOverviewModel \n The assert value of actual/projected cost in Quarter " + (i + 1).ToString() + " is " + Math.Round(Convert.ToDecimal(ActualList[i]), 2) + ".");
                                    // Console.WriteLine("Testing revenueOverviewModel method \n The assert value of actual/projected cost in Quarter " + (i + 1).ToString() + " is " + Math.Round(Convert.ToDecimal(ActualList[i]), 2) + ".");
                                }
                            }
                            if (GoalList != null && GoalList.Count > 0)
                            {
                                decimal sumOfGoal = 0;
                                for (int i = 0; i <= GoalList.Count - 1; i++)
                                {
                                    sumOfGoal = sumOfGoal + Convert.ToDecimal(QuaterlyGoalList[i]);
                                    Assert.AreEqual(Math.Round(Convert.ToDecimal(GoalList[i]), 2), Math.Round(sumOfGoal, 2));
                                    Console.WriteLine("ReportController - revenueOverviewModel \n The assert value of goal in  Quarter " + (i + 1).ToString() + " is " + Math.Round(Convert.ToDecimal(GoalList[i]), 2) + ".");
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public decimal SetValuesForSummeryRevenueReport(DataTable dt)
        {
            DataTable dtTactic = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Tactic$]").Tables[0];
            DataRow drTactic = dtTactic.Rows[0];

            DateTime NewStartDate = Convert.ToDateTime(drTactic["RevenueStartDate"].ToString());
            DateTime NewEndDate = Convert.ToDateTime(drTactic["RevenueEndDate"].ToString());
            int MonthDiff = Convert.ToInt32(drTactic["RevenueMonthDiff"].ToString());

            DataTable dtModel = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Model$]").Tables[0];
            DataRow drModel = dtModel.Rows[0];

            decimal TacticINQ = Convert.ToDecimal(drModel["TACTIC_PROJECTED_STAGE"].ToString());

            TacticRevenueAmount = ObjPlanCommonFunctions.CalculationForTactic(drModel, TacticINQ, "Revenue");
            decimal GoalAmount = TacticRevenueAmount / MonthDiff;


            //calculate Sum of actual
            if (dt.Rows[3] != null)
            {
                for (int i = 1; i <= dt.Columns.Count - 1; i++)
                {
                    if (i <= currentMonth)
                    {
                        SumOfActual = SumOfActual + Convert.ToDecimal(dt.Rows[3][i].ToString());
                    }
                }
            }
            //calculate Sum of projected
            if (MonthDiff >= 0)
            {
                if (DateTime.Now.Date >= NewStartDate && DateTime.Now.Date <= NewEndDate)
                {
                    for (int i = 1; i <= dt.Columns.Count - 1; i++)
                    {
                        int currentMonthNo = DateTime.Now.Month - NewStartDate.Month + 1;
                        if (DateTime.Now.Month == i)
                        {
                            decimal proCal = (GoalAmount / MonthDiff) * currentMonthNo;
                            SumOfProjected = SumOfProjected + Convert.ToDecimal(proCal);
                        }
                        else
                        {
                            if (DateTime.Now.Month < i)
                            {
                                int current = i - DateTime.Now.Month + currentMonthNo;
                                decimal cal = (GoalAmount / MonthDiff) * current;
                                var final = SumOfActual / DateTime.Now.Month;
                                var profinal = cal + final;
                                SumOfProjected = SumOfProjected + profinal;
                            }
                        }
                    }
                }
            }
            return SumOfProjected + SumOfActual;
        }

        #endregion

        #region Finance Report

        [TestMethod()]
        public async Task FinanceSummaryReport()
        {
            try
            {
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Assert.AreEqual("Index", IsLogin.RouteValues["Action"]);
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value of Action : " + IsLogin.RouteValues["Action"]);
                    ObjPlanCommonFunctions.SetSessionData();
                    objReportController = new ReportController();
                    objReportModel = new ReportModel();

                    Sessions.PlanExchangeRate = 1.0;

                    var result = await objReportController.GetOverviewData(currentYear, "Quarterly") as PartialViewResult;
                    objReportOverviewModel = new ReportOverviewModel();
                    objReportOverviewModel = (ReportOverviewModel)result.Model;
                    FinancialOverviewModel objFinancialOverviewModel = new FinancialOverviewModel();
                    objFinancialOverviewModel = objReportOverviewModel.financialOverviewModel;

                    DataTable Financedt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[QuarterlyFinance$]").Tables[0];
                    DataTable Plandt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Plan$]").Tables[0];

                    if (Financedt != null && objFinancialOverviewModel != null)
                    {
                        List<double> actualList = objFinancialOverviewModel.MainActualCostList;
                        List<double> plannedList = objFinancialOverviewModel.MainPlannedCostList;
                        List<double> budgetList = objFinancialOverviewModel.MainBudgetCostList;
                        double totalbudget = objFinancialOverviewModel.TotalBudgetAllocated;
                        double unallocatedTotalbudget = objFinancialOverviewModel.TotalBudgetUnAllocated;
                        double plannedCost = objFinancialOverviewModel.PlannedCostvsBudget;
                        double actualCost = objFinancialOverviewModel.ActualCostvsBudet;

                        #region Actual data
                        for (int i = 0; i <= actualList.Count - 1; i++)
                        {
                            if (i == 0)
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(actualList[i]), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Q1 - Actual"].ToString()), 2));
                            }
                            else if (i == 1)
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(actualList[i]), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Q2 - Actual"].ToString()), 2));
                            }
                            else if (i == 2)
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(actualList[i]), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Q3 - Actual"].ToString()), 2));
                            }
                            else
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(actualList[i]), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Q4 - Actual"].ToString()), 2));
                            }
                            Console.WriteLine("ReportController - revenueOverviewModel \n The assert value of actual cost Quarter " + (i + 1).ToString() + " is " + Math.Round(Convert.ToDecimal(actualList[i]), 2).ToString() + ".");
                        }
                        #endregion

                        #region Planned data
                        for (int i = 0; i <= plannedList.Count - 1; i++)
                        {
                            if (i == 0)
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(plannedList[i]), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Q1 - Planned"].ToString()), 2));
                            }
                            else if (i == 1)
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(plannedList[i]), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Q2 - Planned"].ToString()), 2));
                            }
                            else if (i == 2)
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(plannedList[i]), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Q3 - Planned"].ToString()), 2));
                            }
                            else
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(plannedList[i]), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Q4 - Planned"].ToString()), 2));
                            }
                            Console.WriteLine("ReportController - revenueOverviewModel \n The assert value of planned cost in Quarter " + (i + 1).ToString() + " is " + Math.Round(Convert.ToDecimal(plannedList[i]), 2).ToString() + ".");
                        }
                        #endregion

                        #region Budget data
                        for (int i = 0; i <= budgetList.Count - 1; i++)
                        {
                            if (i == 0)
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(budgetList[i]), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Q1 - Budget"].ToString()), 2));
                            }
                            else if (i == 1)
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(budgetList[i]), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Q2 - Budget"].ToString()), 2));
                            }
                            else if (i == 2)
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(budgetList[i]), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Q3 - Budget"].ToString()), 2));
                            }
                            else
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(budgetList[i]), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Q4 - Budget"].ToString()), 2));
                            }
                            Console.WriteLine("ReportController - revenueOverviewModel \n The assert value of budget in Quarter " + (i + 1).ToString() + " is " + Math.Round(Convert.ToDecimal(budgetList[i]), 2).ToString() + ".");
                        }
                        #endregion

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(totalbudget), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Total Budget"].ToString()), 2));
                        Console.WriteLine("ReportController - revenueOverviewModel \n The assert value of total budget is " + Math.Round(Convert.ToDecimal(totalbudget), 2).ToString() + ".");

                        decimal unallocatedBudget = Convert.ToDecimal(Plandt.Rows[0]["BudgetValue"].ToString()) - Convert.ToDecimal(totalbudget);

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(unallocatedTotalbudget), 2), Math.Round(unallocatedBudget, 2));
                        Console.WriteLine("ReportController - revenueOverviewModel \n The assert value of unallocated cost is " + Math.Round(Convert.ToDecimal(unallocatedTotalbudget), 2).ToString() + ".");

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(plannedCost), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Planned Cost"].ToString()), 2));
                        Console.WriteLine("ReportController - revenueOverviewModel \n The assert value of planned cost vs. budget is " + Math.Round(Convert.ToDecimal(plannedCost), 2).ToString() + " / " + totalbudget + ".");

                        Assert.AreEqual(Math.Round(Convert.ToDecimal(actualCost), 2), Math.Round(Convert.ToDecimal(Financedt.Rows[0]["Total Actual"].ToString()), 2));
                        Console.WriteLine("ReportController - revenueOverviewModel \n The assert value of actual cost vs. budget is " + Math.Round(Convert.ToDecimal(actualCost), 2).ToString() + " / " + totalbudget + ".");



                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion
    }
}

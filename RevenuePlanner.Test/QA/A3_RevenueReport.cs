using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.QA_Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace RevenuePlanner.Test.QA
{

    [TestClass]
    public class A3_RevenueReport
    {
        #region Variable Declaration
        Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();
        PlanCommonFunctions ObjPlanCommonFunctions = new PlanCommonFunctions();

        DateTime NewStartDate; DateTime NewEndDate; DateTime TacticEndDate; DateTime TacticStartDate; int MonthDiff = 0; int currentMonth = DateTime.Now.Month; string currentyear = DateTime.Now.Year.ToString();
        static decimal TacticRevenueAmount = 0; decimal TacticINQ = 0; decimal TacticProjectedCost = 0;
        decimal TacticTQL = 0; decimal GoalAmount = 0; decimal actualProjected = 0; static decimal TotalProjected = 0;

        static List<double> ActualList; static List<double> ProjectedList; static List<double> GoalList; static List<string> PerformanceList;
        static List<string> ActualCostList; static List<string> ROIList; static List<string> TotalRevenueList;
        List<double> QuaterlyActualList; List<double> QuaterlyProjectedList; List<double> QuaterlyGoalList; List<string> QuaterlyPerformanceList; List<string> QuaterlyActualCostList;
        List<string> QuaterlyROIList; List<string> QuaterlyTotalRevenueList;

        ReportController objReportController; ReportModel objReportModel; RevenueDataTable objReportDataTable; CardSectionModel objCardSection;
        RevenueToPlanModel objRevenueToPlanModel; RevenueSubDataTableModel subModelList; Projected_Goal objProjected_Goal; lineChartData objlineChartData;
        CardSectionListModel objCardSectionListModel;

        static string[] MonthList = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        static string[] QuarterList = { "quarter 1", "quarter 2", "quarter 3", "quarter 4" };
        static int[] num = { 2, 5, 8, 11 };
        #endregion

        [TestMethod()]
        //[Priority(2)]
        public void A1_MonthlyRevenueReport()
        {
            try
            {
                //Call common function for login
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Assert.AreEqual("Index", IsLogin.RouteValues["Action"]);
                    Console.WriteLine(" Testing LoginController - Index method");
                    Console.WriteLine(" The assert value of action is " + IsLogin.RouteValues["Action"] + ". (The expected value is Index.)");
                    Console.WriteLine("\n ----------------------------------------------------------------------");
                    Console.WriteLine("\n Testing  ReportController - GetRevenueData Method");
                    SetValuesForReport(currentyear, "Monthly");
                    DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        Console.WriteLine("\n -------------- Summary - Table Number Validation --------------");
                        VerifyReport_Actual(dt, ActualList);
                        VerifyReport_ActualCost(dt, ActualCostList);
                        VerifyReport_Goal(GoalList);
                        VerifyReport_ProjectedRevenue(ProjectedList, GoalList, ActualList);
                        VerifyReport_Performance(ActualList, PerformanceList);
                        VerifyReport_ROI(ActualList, ActualCostList, ROIList);
                        VerifyReport_TotalRevenue(ActualList, TotalRevenueList);
                        VerifyHeaderValue(objProjected_Goal, ActualList, GoalList, ProjectedList);                       
                        VerifyMonthlyGraphValue(objlineChartData, dt, ProjectedList);
                        VerifyCardSectionValue(objCardSection, ActualCostList);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SetValuesForReport(string Year, string isQuarterly)
        {
            objReportController = new ReportController();
            objReportModel = new ReportModel();
            objReportDataTable = new RevenueDataTable();
            subModelList = new RevenueSubDataTableModel();
            objRevenueToPlanModel = new RevenueToPlanModel();
            objProjected_Goal = new Projected_Goal();
            objCardSection = new CardSectionModel();
            objlineChartData = new lineChartData();
            objCardSectionListModel = new CardSectionListModel();
            ObjPlanCommonFunctions.SetSessionData();

            var result1 = objReportController.GetRevenueData(Year, isQuarterly) as PartialViewResult;
            objReportModel = (ReportModel)(result1.ViewData.Model);
            objRevenueToPlanModel = objReportModel.RevenueToPlanModel;
            objReportDataTable = objRevenueToPlanModel.RevenueToPlanDataModel;
            subModelList = objReportDataTable.SubDataModel;
            objlineChartData = objRevenueToPlanModel.LineChartModel;
            objProjected_Goal = objReportModel.RevenueHeaderModel;
            objCardSection = objReportModel.CardSectionModel;
            objCardSectionListModel = objCardSection.CardSectionListModel[0];

            if (isQuarterly == "Monthly")
            {
                ActualList = objReportDataTable.ActualList;
                ProjectedList = objReportDataTable.ProjectedList;
                GoalList = objReportDataTable.GoalList;
                PerformanceList = subModelList.PerformanceList;
                ActualCostList = subModelList.CostList;
                ROIList = subModelList.ROIList;
                TotalRevenueList = subModelList.RevenueList;
            }
            else
            {
                QuaterlyActualList = objReportDataTable.ActualList;
                QuaterlyProjectedList = objReportDataTable.ProjectedList;
                QuaterlyGoalList = objReportDataTable.GoalList;
                QuaterlyPerformanceList = subModelList.PerformanceList;
                QuaterlyActualCostList = subModelList.CostList;
                QuaterlyROIList = subModelList.ROIList;
                QuaterlyTotalRevenueList = subModelList.RevenueList;
            }
        }

        #region Monthly Report Calculation Methods

        public void VerifyReport_Actual(DataTable dt, List<double> ActualList)
        {
            if (dt.Rows[3] != null)
            {
                for (int i = 1; i <= dt.Columns.Count - 1; i++)
                {
                    if (i > currentMonth)
                    {
                        dt.Rows[3][i] = 0;
                    }
                    Assert.AreEqual(dt.Rows[3].ItemArray[i].ToString(), ActualList[i - 1].ToString());
                    Console.WriteLine("\n The assert value of actual in " + MonthList[i - 1] + " is " + Math.Round(Convert.ToDecimal(ActualList[i - 1]), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(dt.Rows[3].ItemArray[i].ToString()), 2).ToString() + ".)");
                }
            }
        }
        public void VerifyReport_ActualCost(DataTable dt, List<string> ActualCostList)
        {
            if (dt.Rows[4] != null)
            {
                for (int i = 1; i <= dt.Columns.Count - 1; i++)
                {
                    if (i > currentMonth)
                    {
                        dt.Rows[4][i] = 0;
                    }
                    Assert.AreEqual(dt.Rows[4].ItemArray[i].ToString(), ActualCostList[i - 1].ToString());
                    Console.WriteLine("\n The assert value of actual cost in " + MonthList[i - 1] + " is " + Math.Round(Convert.ToDecimal(ActualCostList[i - 1]), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(dt.Rows[4].ItemArray[i].ToString()), 2).ToString() + ".)");
                }
            }
        }
        public void VerifyReport_Goal(List<double> GoalList)
        {
            DataTable dtTactic = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Tactic$]").Tables[0];
            DataRow drTactic = dtTactic.Rows[0];

            NewStartDate = Convert.ToDateTime(drTactic["RevenueStartDate"].ToString());
            NewEndDate = Convert.ToDateTime(drTactic["RevenueEndDate"].ToString());
            TacticStartDate = Convert.ToDateTime(drTactic["TacticStartDate"].ToString());
            TacticEndDate = Convert.ToDateTime(drTactic["TacticEndDate"].ToString());
            MonthDiff = Convert.ToInt32(drTactic["RevenueMonthDiff"].ToString());

            DataTable dtModel = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Model$]").Tables[0];
            DataRow drModel = dtModel.Rows[0];

            TacticProjectedCost = Convert.ToDecimal(drModel["TACTIC_PROJECTED_COST"].ToString());
            TacticINQ = Convert.ToDecimal(drModel["TACTIC_PROJECTED_STAGE"].ToString());
            TacticTQL = ObjPlanCommonFunctions.CalculationForTactic(drModel, TacticINQ, "TQL");
            TacticRevenueAmount = ObjPlanCommonFunctions.CalculationForTactic(drModel, TacticINQ, "Revenue");

            GoalAmount = TacticRevenueAmount / MonthDiff;

            for (int i = 1; i < GoalList.Count; i++)
            {
                if (i >= NewStartDate.Month && i <= NewEndDate.Month)
                {
                    Assert.AreEqual(Math.Round(Convert.ToDouble(GoalAmount)), Convert.ToDouble(GoalList[i - 1].ToString()));
                    Console.WriteLine("\n The assert value of goal in " + MonthList[i - 1] + " is " + Math.Round(GoalList[i - 1]).ToString() + ". (The expected value is " + Math.Round(Convert.ToDouble(GoalAmount)).ToString() + ".)");
                }
                else
                {
                    Assert.AreEqual("0", GoalList[i - 1].ToString());
                    Console.WriteLine("\n The assert value of goal in " + MonthList[i - 1] + " is " + GoalList[i - 1].ToString() + ". (The expected value is 0.)");
                }
            }

        }
        public void VerifyReport_ProjectedRevenue(List<double> ProjectedList, List<double> GoalList, List<double> ActualList)
        {
            if (MonthDiff >= 0)
            {
                if (DateTime.Now.Date >= NewStartDate && DateTime.Now.Date <= NewEndDate)
                {

                    for (int i = 1; i <= GoalList.Count(); i++)
                    {
                        int currentMonthNo = currentMonth - NewStartDate.Month + 1;
                        if (currentMonth == i)
                        {
                            double proCal = (GoalList[i - 1] / MonthDiff) * currentMonthNo;
                            // Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), Convert.ToDouble(proCal));
                            Assert.AreEqual(proCal, ProjectedList[i - 1]);
                            Console.WriteLine("\n The assert value of projected in " + MonthList[i - 1] + " is " + ProjectedList[i - 1].ToString() + ". (The expected value is " + proCal.ToString() + ".)");
                        }
                        else if (currentMonth < i)
                        {
                            double proCal = 0;
                            int current = i - currentMonth + currentMonthNo;
                            double cal = (GoalList[i - 1] / MonthDiff) * current;
                            for (int j = 0; j < ActualList.Count(); j++)
                            {
                                proCal = proCal + ActualList[j];
                            }
                            var final = proCal / currentMonth;
                            var profinal = cal + final;
                            Assert.AreEqual(Math.Round(Convert.ToDouble(profinal), 2), Math.Round(Convert.ToDouble(ProjectedList[i - 1].ToString()), 2));
                            Console.WriteLine("\n The assert value of projected in " + MonthList[i - 1] + " is " + ProjectedList[i - 1].ToString() + ". (The expected value is " + profinal.ToString() + ".)");
                        }
                        else
                        {
                            Assert.AreEqual(0, Convert.ToDouble(ProjectedList[i - 1].ToString()));
                            Console.WriteLine("\n The assert value of projected in " + MonthList[i - 1] + " is " + ProjectedList[i - 1].ToString() + ". (The expected value is 0.)");
                        }
                    }
                }
            }
        }
        public void VerifyReport_Performance(List<double> ActualList, List<string> PerformanceList)
        {
            for (int i = 1; i <= ActualList.Count(); i++)
            {
                if (NewStartDate.Month <= i && NewEndDate.Month >= i)
                {
                    decimal calculatePer = ((Convert.ToDecimal(ActualList[i - 1].ToString()) - GoalAmount) / GoalAmount) * 100;
                    Assert.AreEqual(Math.Round(calculatePer, 2), Math.Round(Convert.ToDecimal(PerformanceList[i - 1].ToString()), 2));
                    Console.WriteLine("\n The assert value of performance in " + MonthList[i - 1] + " is " + Math.Round(Convert.ToDecimal(PerformanceList[i - 1]),2).ToString() + ". (The expected value is " + Math.Round(calculatePer, 2).ToString() + ".)");
                }
                else
                {
                    Assert.AreEqual(0, Convert.ToDouble(PerformanceList[i - 1].ToString()));
                    Console.WriteLine("\n The assert value of performance in " + MonthList[i - 1] + " is " + Math.Round(Convert.ToDecimal(PerformanceList[i - 1]),2).ToString() + ". (The expected value is 0.)");
                }
            }
        }
        public void VerifyReport_ROI(List<double> ActualList, List<string> ActualCostList, List<string> ROIList)
        {
            for (int i = 1; i <= ActualList.Count(); i++)
            {
                if (i <= currentMonth)
                {
                    decimal calculatePer = ((Convert.ToDecimal(ActualList[i - 1].ToString()) - Convert.ToDecimal(ActualCostList[i - 1].ToString())) / (Convert.ToDecimal(ActualCostList[i - 1].ToString()))) * 100;
                    Assert.AreEqual(Math.Round(calculatePer, 2), Math.Round(Convert.ToDecimal(ROIList[i - 1].ToString()), 2));
                    Console.WriteLine("\n The assert value of ROI in " + MonthList[i - 1] + " is " + Math.Round(Convert.ToDecimal(ROIList[i - 1]),2).ToString() + ". (The expected value is " + Math.Round(calculatePer, 2).ToString() + ".)");
                }
                else
                {
                    Assert.AreEqual("0", ROIList[i - 1].ToString());
                    Console.WriteLine("\n The assert value of ROI in " + MonthList[i - 1] + " is " + Math.Round(Convert.ToDecimal(ROIList[i - 1]),2).ToString() + ". (The expected value is 0.)");
                }
            }
        }
        public void VerifyReport_TotalRevenue(List<double> ActualList, List<string> TotalRevenueList)
        {
            decimal Total = 0;
            for (int i = 1; i <= ActualList.Count(); i++)
            {
                if (i <= currentMonth)
                {
                    Total = Total + Convert.ToDecimal(ActualList[i - 1].ToString());
                    Assert.AreEqual(Math.Round(Total, 2).ToString(), TotalRevenueList[i - 1].ToString());
                    Console.WriteLine("\n The assert value of total revenue in " + MonthList[i - 1] + " is " + TotalRevenueList[i - 1].ToString() + ". (The expected value is " + Math.Round(Total, 2).ToString() + ".)");
                }
                else
                {
                    Assert.AreEqual("0", TotalRevenueList[i - 1].ToString());
                    Console.WriteLine("\n The assert value of total revenue in " + MonthList[i - 1] + " is " + TotalRevenueList[i - 1].ToString() + ". (The expected value is 0.)");
                }
            }
        }

        #endregion

        #region Header calculation

        public void VerifyHeaderValue(Projected_Goal objProjected_Goal, List<double> ActualList, List<double> GoalList, List<double> ProjectedList)
        {
            Console.WriteLine("\n -------------- Header Number Validation --------------");
            #region Actual
            // Get actual projected value
            decimal GoalUpToCurrentMonth = 0; decimal actualPerecentage = 0; decimal SumOfProjected = 0; decimal ProjectedPercentage = 0;
            foreach (decimal actual in ActualList)
            {
                actualProjected = actualProjected + actual;
            }
            Assert.AreEqual(Math.Round(actualProjected, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.Actual_Projected), 2));
            Console.WriteLine("\n The assert value of actual projected in header is " + Math.Round(Convert.ToDecimal(objProjected_Goal.Actual_Projected), 2).ToString() + ". (The expected value is " + Math.Round(actualProjected, 2).ToString() + ".)");

            // Get actual perecentage value
            for (int i = 1; i <= GoalList.Count(); i++)
            {
                if (currentMonth >= i)
                    GoalUpToCurrentMonth = GoalUpToCurrentMonth + Convert.ToDecimal(GoalList[i - 1].ToString());
            }
            actualPerecentage = ((actualProjected - GoalUpToCurrentMonth) / GoalUpToCurrentMonth) * 100;
            Assert.AreEqual(Math.Round(actualPerecentage, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.ActualPercentage), 2));
            Console.WriteLine("\n The assert value of actual perecentage in header is " + Math.Round(Convert.ToDecimal(objProjected_Goal.ActualPercentage), 2).ToString() + ". (The expected value is " + Math.Round(actualPerecentage, 2).ToString() + ".)");

            #endregion

            #region Projected
            // Get projected value
            for (int i = 0; i < ProjectedList.Count(); i++)
            {
                SumOfProjected = SumOfProjected + Convert.ToDecimal(ProjectedList[i].ToString());
            }
            TotalProjected = SumOfProjected + actualProjected;
            Assert.AreEqual(Math.Round(TotalProjected, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.Projected), 2));
            Console.WriteLine("\n The assert value of projected in header is " + Math.Round(Convert.ToDecimal(objProjected_Goal.Projected), 2).ToString() + ". (The expected value is " + Math.Round(TotalProjected, 2).ToString() + ".)");

            //Calculation for projected percentage
            ProjectedPercentage = ((TotalProjected - TacticRevenueAmount) / TacticRevenueAmount) * 100;
            Assert.AreEqual(Math.Round(ProjectedPercentage, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.ProjectedPercentage), 2));
            Console.WriteLine("\n The assert value of projected perecentage in header is " + Math.Round(Convert.ToDecimal(objProjected_Goal.ProjectedPercentage), 2).ToString() + ". (The expected value is " + Math.Round(ProjectedPercentage, 2).ToString() + ".)");

            #endregion
        }

        #endregion

        #region Card section calculation

        public void VerifyCardSectionValue(CardSectionModel objCardSection, List<string> ActualCostList)
        {
            var Card = objCardSection.CardSectionListModel[0];
            Console.WriteLine("\n -------------- Summary - Card Number Validation --------------");

            #region Actual
            Assert.AreEqual(Math.Round(actualProjected, 2), Math.Round(Convert.ToDecimal(Card.RevenueCardValues.Actual_Projected), 2));
            Console.WriteLine("\n The assert value of actual projected is " + Math.Round(Convert.ToDecimal(Card.RevenueCardValues.Actual_Projected), 2).ToString() + ". (The expected value is " + Math.Round(actualProjected, 2).ToString() + ".)");

            Assert.AreEqual(Math.Round(TacticRevenueAmount, 2), Math.Round(Convert.ToDecimal(Card.RevenueCardValues.Goal), 2));
            Console.WriteLine("\n The assert value of actual goal is " + Math.Round(Convert.ToDecimal(Card.RevenueCardValues.Goal), 2).ToString() + ". (The expected value is " + Math.Round(TacticRevenueAmount, 2).ToString() + ".)");

            var actualPercentage = ((actualProjected - TacticRevenueAmount) / TacticRevenueAmount) * 100;
            Assert.AreEqual(Math.Round(actualPercentage, 2), Math.Round(Convert.ToDecimal(Card.RevenueCardValues.Percentage), 2));
            Console.WriteLine("\n The assert value of actual perecentage is " + Math.Round(Convert.ToDecimal(Card.RevenueCardValues.Percentage), 2).ToString() + ". (The expected value is " + Math.Round(actualPercentage, 2).ToString() + ".)");

            #endregion

            #region Cost
            decimal tacticCost = 0;
            foreach (var actualCost in ActualCostList)
            {
                tacticCost = tacticCost + Convert.ToDecimal(actualCost);
            }
            Assert.AreEqual(Math.Round(tacticCost, 2), Math.Round(Convert.ToDecimal(Card.CostCardValues.Actual_Projected), 2));
            Console.WriteLine("\n The assert value of tactic cost is " + Math.Round(Convert.ToDecimal(Card.CostCardValues.Actual_Projected), 2).ToString() + ". (The expected value is " + Math.Round(tacticCost, 2).ToString() + ".)");

            Assert.AreEqual(Math.Round(TacticProjectedCost, 2), Math.Round(Convert.ToDecimal(Card.CostCardValues.Goal), 2));
            Console.WriteLine("\n The assert value of tactic projected is " + Math.Round(Convert.ToDecimal(Card.CostCardValues.Goal), 2).ToString() + ". (The expected value is " + Math.Round(TacticProjectedCost, 2).ToString() + ".)");

            decimal costPercentage = ((tacticCost - TacticProjectedCost) / TacticProjectedCost) * 100;
            Assert.AreEqual(Math.Round(costPercentage, 2), Math.Round(Convert.ToDecimal(Card.CostCardValues.Percentage), 2));
            Console.WriteLine("\n The assert value of tactic perecentage is " + Math.Round(Convert.ToDecimal(Card.CostCardValues.Percentage), 2).ToString() + ". (The expected value is " + Math.Round(costPercentage, 2).ToString() + ".)");
            #endregion

            #region ROI
            var ROIActual_Projected = ((actualProjected - tacticCost) / tacticCost) * 100;
            Assert.AreEqual(Math.Round(ROIActual_Projected, 2), Math.Round(Convert.ToDecimal(Card.ROICardValues.Actual_Projected), 2));
            Console.WriteLine("\n The assert value of ROI projected is " + Math.Round(Convert.ToDecimal(Card.ROICardValues.Actual_Projected), 2).ToString() + ". (The expected value is " + Math.Round(ROIActual_Projected, 2).ToString() + ".)");

            var ROIActual_Goal = ((TacticRevenueAmount - TacticProjectedCost) / TacticProjectedCost) * 100;
            Assert.AreEqual(Math.Round(ROIActual_Goal, 2), Math.Round(Convert.ToDecimal(Card.ROICardValues.Goal), 2));
            Console.WriteLine("\n The assert value of ROI goal is " + Math.Round(Convert.ToDecimal(Card.ROICardValues.Goal), 2).ToString() + ". (The expected value is " + Math.Round(ROIActual_Goal, 2).ToString() + ".)");

            var ROIActual_Percentage = ((ROIActual_Projected - ROIActual_Goal) / ROIActual_Goal) * 100;
            Assert.AreEqual(Math.Round(ROIActual_Percentage, 2), Math.Round(Convert.ToDecimal(Card.ROICardValues.Percentage), 2));
            Console.WriteLine("\n The assert value of ROI perecentage is " + Math.Round(Convert.ToDecimal(Card.ROICardValues.Percentage), 2).ToString() + ". (The expected value is " + Math.Round(ROIActual_Percentage, 2).ToString() + ".)");
            #endregion

        }

        #endregion

        #region Monthly Graph

        public void VerifyMonthlyGraphValue(lineChartData objlineChartData, DataTable dt, List<double> ProjectedList)
        {
            if (objlineChartData.categories != null && objlineChartData.categories.Count > 0 && objlineChartData.series != null && objlineChartData.series.Count > 0)
            {
                List<string> categories = objlineChartData.categories;
                series ActualSeries = objlineChartData.series[0];
                series GoalSeries = objlineChartData.series[1];
                series ProjectedSeries = objlineChartData.series[2];
                decimal SumOfActualSeries = 0;

                Console.WriteLine("\n -------------- Summary - Graph Number Validation --------------");

                if (dt.Rows[3] != null)
                {
                    for (int i = 1; i <= dt.Columns.Count - 1; i++)
                    {
                        SumOfActualSeries = SumOfActualSeries + Convert.ToDecimal(dt.Rows[3].ItemArray[i].ToString()) + Convert.ToDecimal(ProjectedList[i - 1]);
                        Assert.AreEqual(Math.Round(SumOfActualSeries, 2),Math.Round(Convert.ToDecimal(ActualSeries.data[i + 1].ToString()), 2));
                        Console.WriteLine("\n The assert value of actual revenue in " + MonthList[i - 1] + " is " + Math.Round(Convert.ToDecimal(ActualSeries.data[i + 1]),2).ToString() + ". (The expected value is " + Math.Round(SumOfActualSeries, 2).ToString() + ".)");
                    }
                }
                if (GoalAmount != 0)
                {
                    decimal goal = 0;
                    for (int i = 2; i <= GoalSeries.data.Count - 1; i++)
                    {
                        if (i - 1 >= NewStartDate.Month && i - 1 <= NewEndDate.Month)
                        {
                            goal = goal + GoalAmount;
                            Assert.AreEqual(Math.Round(goal, 2), Math.Round(Convert.ToDecimal(GoalSeries.data[i].ToString()), 2));
                            Console.WriteLine("\n The assert value of goal in " + MonthList[i - 2] + " is " + Math.Round(Convert.ToDecimal(GoalSeries.data[i].ToString()), 2).ToString() + ". (The expected value is " + Math.Round(goal, 2).ToString() + ".)");
                        }
                        else
                        {
                            Assert.AreEqual("0", GoalSeries.data[i].ToString());
                            Console.WriteLine("\n The assert value of goal in " + MonthList[i - 2] + " is " + Math.Round(Convert.ToDecimal(GoalSeries.data[i].ToString()), 2).ToString() + ". (The expected value is 0.)");
                        }
                    }
                }
                for (int i = 2; i <= ProjectedSeries.data.Count - 1; i++)
                {
                    decimal projectedData = 0; decimal projected = 0;
                    if (ProjectedSeries.data[i].ToString() != null)
                    {
                        projectedData = Convert.ToDecimal(ProjectedSeries.data[i]);
                        if (currentMonth > i - 1)
                            projected = Convert.ToDecimal(ProjectedList[i - 2]);
                        else
                            projected = Convert.ToDecimal(ProjectedList[i - 2]) + Convert.ToDecimal(TotalRevenueList[i - 2].ToString());
                    }
                    Assert.AreEqual(Math.Round(projected, 2), Math.Round(projectedData, 2) );
                    Console.WriteLine("\n The assert value of projected revenue in " + MonthList[i - 2] + " is " + Math.Round(projectedData,2).ToString() + ". (The expected value is "+ Math.Round(projected, 2).ToString()+".)");
                }
            }
        }

        #endregion

        [TestMethod()]
        //[Priority(1)]
        public void A2_QuarterlyRevenueReport()
        {
            try
            {
                //Call common function for login
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Assert.AreEqual("Index", IsLogin.RouteValues["Action"]);
                    Console.WriteLine(" Testing LoginController - Index method");
                    Console.WriteLine(" The assert value of action is " + IsLogin.RouteValues["Action"] + ". (The expected value is Index.)");
                    Console.WriteLine("\n ----------------------------------------------------------------------");
                    Console.WriteLine("\n Testing  ReportController - GetRevenueData Method");
                    Console.WriteLine("\n -------------- Summary - Table Number Validation --------------");
                    SetValuesForReport(currentyear, "Quarterly");
                    if (objRevenueToPlanModel != null)
                    {
                        VerifyQuarterly_Actual(ActualList, QuaterlyActualList);
                        VerifyQuarterly_ActualCost(ActualCostList, QuaterlyActualCostList);
                        VerifyQuarterly_Goal(GoalList, QuaterlyGoalList);
                        VerifyQuarterly_ProjectedRevenue(ProjectedList, QuaterlyProjectedList);
                        VerifyQuarterly_Performance(QuaterlyActualList, QuaterlyGoalList, QuaterlyPerformanceList);
                        VerifyQuarterly_ROI(QuaterlyActualList, QuaterlyActualCostList, QuaterlyROIList);
                        VerifyQuarterly_TotalRevenue(TotalRevenueList, QuaterlyTotalRevenueList);
                        VerifyQuarterlyGraphValue(objlineChartData, objCardSectionListModel);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #region Quarterly Report Calculation

        public void VerifyQuarterly_Actual(List<double> ActualList, List<double> QuaterlyActualList)
        {
            decimal QuaActual = 0;
            int j = 0;
            for (int i = 0; i < ActualList.Count(); i++)
            {
                QuaActual = QuaActual + Convert.ToDecimal(ActualList[i].ToString());
                if (num.Contains(i))
                {
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyActualList[j].ToString()), 2), Math.Round(QuaActual, 2));
                    Console.WriteLine("\n The assert value of actual in " + QuarterList[j] + " is " + Math.Round(QuaterlyActualList[j], 2).ToString() + ". (The expected value is " + Math.Round(QuaActual, 2).ToString() + ".)");
                    QuaActual = 0;
                    j++;
                }
            }
        }
        public void VerifyQuarterly_ActualCost(List<string> ActualCostList, List<string> QuaterlyActualCostList)
        {
            decimal QuaActualCost = 0;
            int j = 0;
            for (int i = 0; i < ActualCostList.Count(); i++)
            {
                QuaActualCost = QuaActualCost + Convert.ToDecimal(ActualCostList[i].ToString());
                if (num.Contains(i))
                {
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyActualCostList[j].ToString()), 2), Math.Round(QuaActualCost, 2));
                    Console.WriteLine("\n The assert value of actual cost in " + QuarterList[j] + " is " + QuaterlyActualCostList[j].ToString() + ". (The expected value is " + Math.Round(QuaActualCost, 2).ToString() + ".)");
                    QuaActualCost = 0;
                    j++;
                }
            }
        }
        public void VerifyQuarterly_Goal(List<double> GoalList, List<double> QuaterlyGoalList)
        {
            decimal QuaGoal = 0; int j = 0;
            for (int i = 0; i < GoalList.Count(); i++)
            {
                QuaGoal = QuaGoal + Convert.ToDecimal(GoalList[i].ToString());
                if (num.Contains(i))
                {
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyGoalList[j].ToString()), 2), Math.Round(QuaGoal, 2));
                    Console.WriteLine("\n The assert value of goal in " + QuarterList[j] + " is " + Math.Round(QuaterlyGoalList[j], 2).ToString() + ". (The expected value is " + Math.Round(QuaGoal, 2).ToString() + ".)");
                    QuaGoal = 0;
                    j++;
                }
            }
        }
        public void VerifyQuarterly_ProjectedRevenue(List<double> ProjectedList, List<double> QuaterlyProjectedList)
        {
            decimal QuaProjected = 0; int j = 0;
            for (int i = 0; i < ProjectedList.Count(); i++)
            {
                QuaProjected = QuaProjected + Convert.ToDecimal(ProjectedList[i].ToString());
                if (num.Contains(i))
                {
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyProjectedList[j].ToString()), 2), Math.Round(QuaProjected, 2));
                    Console.WriteLine("\n The assert value of projected revenue in " + QuarterList[j] + " is " + Math.Round(QuaterlyProjectedList[j], 2).ToString() + ". (The expected value is " + Math.Round(QuaProjected, 2).ToString() + ".)");
                    QuaProjected = 0;
                    j++;
                }
            }
        }
        public void VerifyQuarterly_Performance(List<double> QuaterlyActualList, List<double> QuaterlyGoalList, List<string> QuaterlyPerformanceList)
        {
            decimal QuaPerformance = 0; //int j = 0;
            for (int i = 0; i < QuaterlyGoalList.Count(); i++)
            {
                if (Convert.ToInt32(QuaterlyGoalList[i]) != 0)
                {
                    QuaPerformance = Convert.ToDecimal((QuaterlyActualList[i] - QuaterlyGoalList[i]) / QuaterlyGoalList[i]) * 100;
                }
                Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyPerformanceList[i].ToString()), 2), Math.Round(QuaPerformance, 2));
                Console.WriteLine("\n The assert value of performance in " + QuarterList[i] + " is " + Math.Round(Convert.ToDecimal(QuaterlyPerformanceList[i]), 2).ToString() + ". (The expected value is " + Math.Round(QuaPerformance, 2).ToString() + ".)");

            }
        }
        public void VerifyQuarterly_ROI(List<double> QuaterlyActualList, List<string> QuaterlyActualCostList, List<string> QuaterlyROIList)
        {
            decimal QuaROI = 0;
            for (int i = 0; i < QuaterlyActualCostList.Count(); i++)
            {
                if (Convert.ToInt32(QuaterlyActualCostList[i]) != 0)
                {
                    QuaROI = ((Convert.ToDecimal(QuaterlyActualList[i]) - Convert.ToDecimal(QuaterlyActualCostList[i])) / Convert.ToDecimal(QuaterlyActualCostList[i])) * 100;
                }
                Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyROIList[i].ToString()), 2), Math.Round(QuaROI, 2));
                Console.WriteLine("\n The assert value of ROI in " + QuarterList[i] + " is " + Math.Round(Convert.ToDecimal(QuaterlyROIList[i]), 2).ToString() + ". (The expected value is " + Math.Round(QuaROI, 2).ToString() + ".)");
            }
        }
        public void VerifyQuarterly_TotalRevenue(List<string> TotalRevenueList, List<string> QuaterlyTotalRevenueList)
        {
            int j = 0;
            for (int i = 0; i < TotalRevenueList.Count(); i++)
            {
                if (num.Contains(i))
                {
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyTotalRevenueList[j].ToString()), 2), Math.Round(Convert.ToDecimal(TotalRevenueList[i]), 2));
                    Console.WriteLine("\n The assert value of total revenue in " + QuarterList[j] + " is " + Math.Round(Convert.ToDecimal(QuaterlyTotalRevenueList[j]), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(TotalRevenueList[i]), 2).ToString() + ".)");
                    j++;
                }
            }
        }

        #endregion

        #region Quarterly Graph

        public void VerifyQuarterlyGraphValue(lineChartData objlineChartData, CardSectionListModel objCardSectionListModel)
        {
            if (objlineChartData.categories != null && objlineChartData.categories.Count > 0 && objlineChartData.series != null && objlineChartData.series.Count > 0)
            {
                List<string> categories = objlineChartData.categories;
                series ActualSeries = objlineChartData.series[0];
                series GoalSeries = objlineChartData.series[1];
                series ProjectedSeries = objlineChartData.series[2];
                decimal SumOfActualSeries = 0; decimal SumOfGoalSeries = 0; decimal SumOfProjectedSeries = 0; decimal SumOfActualCostSeries = 0;

                lineChartData tacticLineChartData = objCardSectionListModel.LineChartData;
                series tacticRevenueSeries = tacticLineChartData.series[0];
                series tacticCostSeries = tacticLineChartData.series[1];

                Console.WriteLine("\n -------------- Summary - Graph Number Validation --------------");

                #region Graph for Plan
                for (int i = 1; i <= ActualSeries.data.Count - 1; i++)
                {
                    if (QuaterlyTotalRevenueList != null && QuaterlyTotalRevenueList.Count > 0 && QuaterlyProjectedList != null && QuaterlyProjectedList.Count > 0)
                    {
                        SumOfActualSeries = Convert.ToDecimal(QuaterlyTotalRevenueList[i - 1]) + Convert.ToDecimal(QuaterlyProjectedList[i - 1]);
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(ActualSeries.data[i].ToString()), 2), Math.Round(SumOfActualSeries, 2));
                        Console.WriteLine("\n The assert value of actual revenue in " + QuarterList[i - 1] + " is " + ActualSeries.data[i].ToString() + ". (The expected value is " + Math.Round(SumOfActualSeries, 2).ToString() + ".)");
                    }
                }
                for (int i = 1; i <= GoalSeries.data.Count - 1; i++)
                {
                    if (QuaterlyGoalList != null && QuaterlyGoalList.Count > 0)
                    {
                        SumOfGoalSeries = SumOfGoalSeries + Convert.ToDecimal(QuaterlyGoalList[i - 1]);
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(GoalSeries.data[i].ToString()), 2), Math.Round(SumOfGoalSeries, 2));
                        Console.WriteLine("\n The assert value of goal in " + QuarterList[i - 1] + " is " + GoalSeries.data[i].ToString() + ". (The expected value is " + Math.Round(SumOfGoalSeries, 2).ToString() + ".)");
                    }
                }
                for (int i = 1; i <= ProjectedSeries.data.Count - 1; i++)
                {
                    decimal projectedSeries = 0;
                    if (QuaterlyProjectedList != null && QuaterlyProjectedList.Count > 0 && QuaterlyProjectedList[i - 1] != 0)
                    {
                        SumOfProjectedSeries = Convert.ToDecimal(QuaterlyTotalRevenueList[i - 1]) + Convert.ToDecimal(QuaterlyProjectedList[i - 1]);
                    }
                    if (ProjectedSeries.data[i].ToString() == "")
                        projectedSeries = 0;
                    else
                        projectedSeries = Convert.ToDecimal(ProjectedSeries.data[i].ToString());

                    Assert.AreEqual(Math.Round(Convert.ToDecimal(projectedSeries), 2), Math.Round(SumOfProjectedSeries, 2));
                    Console.WriteLine("\n The assert value of projected revenue in " + QuarterList[i - 1] + " is " + ProjectedSeries.data[i].ToString() + ". (The expected value is " + Math.Round(SumOfProjectedSeries, 2).ToString() + ".)");
                }
                #endregion

                #region Graph for Tactic

                for (int i = 0; i <= QuaterlyTotalRevenueList.Count - 1; i++)
                {
                    if (QuaterlyTotalRevenueList != null && QuaterlyTotalRevenueList.Count > 0)
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(tacticRevenueSeries.data[i].ToString()), 2), Math.Round(Convert.ToDecimal(QuaterlyTotalRevenueList[i]), 2));
                        Console.WriteLine("\n The assert value of tactic actual revenue in " + QuarterList[i] + " is " + tacticRevenueSeries.data[i].ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(QuaterlyTotalRevenueList[i]), 2).ToString() + ".)");
                    }
                }

                for (int i = 0; i <= QuaterlyActualCostList.Count - 1; i++)
                {
                    if (QuaterlyActualCostList != null && QuaterlyActualCostList.Count > 0)
                    {
                        SumOfActualCostSeries = SumOfActualCostSeries + Convert.ToDecimal(QuaterlyActualCostList[i]);
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(tacticCostSeries.data[i].ToString()), 2), Math.Round(SumOfActualCostSeries, 2));
                        Console.WriteLine("\n The assert value of tactic actual cost in " + QuarterList[i] + " is " + tacticCostSeries.data[i].ToString() + ". (The expected value is " + Math.Round(SumOfActualCostSeries, 2).ToString() + ".)");
                    }
                }
                #endregion
            }
        }

        #endregion
    }
}


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
    public class MQLWaterfallReport
    {
        #region Variable Declaration

        Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();
        PlanCommonFunctions ObjPlanCommonFunctions = new PlanCommonFunctions();

        DateTime NewStartDate; DateTime NewEndDate; DateTime TacticEndDate; DateTime TacticStartDate; int MonthDiff = 0; int currentMonth = DateTime.Now.Month;
        // decimal TacticRevenueAmount = 0;
        decimal TacticINQ = 0; decimal TacticProjectedCost = 0; static string currentYear = DateTime.Now.Year.ToString();
        decimal PlanBudget = 0; decimal TacticTQL = 0; decimal GoalAmount = 0; decimal actualProjected = 0; decimal TacticCW = 0;

        List<int> PlanIds; List<int> ReportOwnerIds; List<int> ReportTacticTypeIds;

        static List<double> ActualList; static List<double> ProjectedList; static List<double> GoalList; static List<string> PerformanceList;
        static List<string> TotalActualList;
        List<double> QuaterlyActualList; List<double> QuaterlyProjectedList; List<double> QuaterlyGoalList; List<string> QuaterlyPerformanceList;
        List<string> QuaterlyTotalActualList;

        ReportController objReportController; ReportModel objReportModel; ConversionToPlanModel objConversionToPlanModel; CardSectionModel objCardSection;
        ConversionDataTable objConversionDataTable; ConversionSubDataTableModel SubDataTableModel; Projected_Goal objProjected_Goal;

        static decimal CardINQActual = 0; static decimal CardINQGoal = 0; static decimal CardINQPercentage = 0;
        static decimal CardMQLActual = 0; static decimal CardMQLGoal = 0; static decimal CardMQLPercentage = 0;
        static decimal CardCWActual = 0; static decimal CardCWGoal = 0; static decimal CardCWPercentage = 0;

        #endregion

        #region  MQL Report Calculation

        [TestMethod()]
        public void MQLWaterfallReportTestCase()
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
                    MonthlyMQLWaterfallReportTest();
                    QuaterlyMQLWaterfallReportTest();
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void MonthlyMQLWaterfallReportTest()
        {

            SetValuesForReport(currentYear, "Monthly", "MQL");
            DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                VerifyWaterfall_MQLActual(dt, ActualList, "Monthly");
                VerifyWaterfall_MQLGoal(GoalList, "Monthly");
                VerifyWaterfall_MQLPerformance(ActualList, PerformanceList, "Monthly");
                VerifyWaterfall_MQLProjected(ProjectedList, GoalList, ActualList, "Monthly");
                VerifyWaterfall_MQLTotal(ActualList, TotalActualList, "Monthly");
                VerifyHeaderValue(objProjected_Goal, ActualList, GoalList, ProjectedList);
                CheckValueForCard("MQL");
                VerifyWaterfall_MQLCardSection(ActualList);
            }

        }

        public void QuaterlyMQLWaterfallReportTest()
        {
            SetValuesForReport(currentYear, "Quaterly", "MQL");
            DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                VerifyWaterfall_MQLActual(dt, ActualList, "Quaterly", QuaterlyActualList);
                VerifyWaterfall_MQLGoal(GoalList, "Quaterly", QuaterlyGoalList);
                VerifyWaterfall_MQLPerformance(ActualList, PerformanceList, "Quaterly", QuaterlyPerformanceList);
                VerifyWaterfall_MQLProjected(ProjectedList, GoalList, ActualList, "Quaterly", QuaterlyProjectedList);
                VerifyWaterfall_MQLTotal(ActualList, TotalActualList, "Quaterly", QuaterlyTotalActualList);
            }

        }

        #region  MQL Calculation

        public void VerifyWaterfall_MQLActual(DataTable dt, List<double> ActualList, string IsQuaterly, List<double> QuaterlyActualList = null)
        {
            if (IsQuaterly == "Monthly")
            {
                if (dt.Rows[1] != null)
                {
                    for (int i = 1; i <= dt.Columns.Count - 1; i++)
                    {
                        if (i > currentMonth)
                        {
                            dt.Rows[1][i] = 0;
                        }
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(ActualList[i - 1]), 2), (Math.Round(Convert.ToDecimal(dt.Rows[1].ItemArray[i].ToString()), 2)));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of actual MQL is " + ActualList[i - 1].ToString() + ".");
                    }
                }
                else
                {
                    decimal QuaActual = 0;
                    for (int i = 0; i < ActualList.Count(); i++)
                    {
                        QuaActual = QuaActual + Convert.ToDecimal(ActualList[i].ToString());
                        if (i % 4 < 0)
                        {
                            Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyActualList[i % 4].ToString()), 2), Math.Round(QuaActual, 2));
                            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly TQL Waterfall Report \n The assert value of actual TQL is " + QuaterlyActualList[i % 4].ToString() + ".");
                            QuaActual = 0;
                        }
                    }
                }
            }
        }

        public void VerifyWaterfall_MQLGoal(List<double> GoalList, string IsQuaterly, List<double> QuaterlyGoalList = null)
        {
            if (IsQuaterly == "Monthly")
            {
                DataTable dtTactic = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Tactic$]").Tables[0];
                DataRow drTactic = dtTactic.Rows[0];

                NewStartDate = Convert.ToDateTime(drTactic["RevenueStartDate"].ToString());
                NewEndDate = Convert.ToDateTime(drTactic["RevenueEndDate"].ToString());
                TacticStartDate = Convert.ToDateTime(drTactic["TacticStartDate"].ToString());
                TacticEndDate = Convert.ToDateTime(drTactic["TacticEndDate"].ToString());
                var tacticMonthDiff = Convert.ToInt32(drTactic["TQLMonthDiff"].ToString());

                DataTable dtModel = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Model$]").Tables[0];
                DataRow drModel = dtModel.Rows[0];

                TacticProjectedCost = Convert.ToDecimal(drModel["TACTIC_PROJECTED_COST"].ToString());
                TacticINQ = Convert.ToDecimal(drModel["TACTIC_PROJECTED_STAGE"].ToString());
                TacticTQL = ObjPlanCommonFunctions.CalculationForTactic(drModel, TacticINQ, "TQL");

                GoalAmount = TacticTQL / tacticMonthDiff;

                for (int i = 1; i <= GoalList.Count; i++)
                {
                    if (i >= NewStartDate.Month && i + 1 <= NewEndDate.Month)
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(GoalList[i - 1].ToString()), 2), Math.Round(Convert.ToDecimal(GoalAmount), 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of TQL goal is" + GoalList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                decimal QuaGoal = 0;
                for (int i = 0; i < GoalList.Count(); i++)
                {
                    QuaGoal = QuaGoal + Convert.ToDecimal(GoalList[i].ToString());
                    if (i % 4 < 0)
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyGoalList[i % 4].ToString()), 2), Math.Round(QuaGoal, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly TQL Waterfall Report  \n The assert value of TQL goal is" + QuaterlyGoalList[i % 4].ToString() + ".");
                        QuaGoal = 0;
                    }
                }
            }
        }

        public void VerifyWaterfall_MQLPerformance(List<double> ActualList, List<string> PerformanceList, string IsQuaterly, List<string> QuaterlyPerformanceList = null)
        {
            if (IsQuaterly == "Monthly")
            {
                DataTable dtTactic = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Tactic$]").Tables[0];
                DataRow drTactic = dtTactic.Rows[0];

                DateTime TQLStartDate = Convert.ToDateTime(drTactic["TQLStartDate"].ToString());
                DateTime TQLEndDate = Convert.ToDateTime(drTactic["TQLEndDate"].ToString());
                for (int i = 1; i <= ActualList.Count(); i++)
                {
                    if (TQLStartDate.Month <= i && TQLEndDate.Month >= i)
                    {
                        decimal calculatePer = ((Convert.ToDecimal(ActualList[i - 1].ToString()) - GoalAmount) / GoalAmount) * 100;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(PerformanceList[i - 1].ToString()), 2), Math.Round(calculatePer, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of TQL performance is " + PerformanceList[i - 1].ToString() + ".");
                    }
                    else
                    {
                        Assert.AreEqual(Convert.ToDouble(PerformanceList[i - 1].ToString()), 0);
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of TQL performance is " + PerformanceList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                decimal QuaPerformance = 0;
                for (int i = 0; i < PerformanceList.Count(); i++)
                {
                    QuaPerformance = QuaPerformance + Convert.ToDecimal(PerformanceList[i].ToString());
                    if (i % 4 < 0)
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyPerformanceList[i % 4].ToString()), 2), Math.Round(QuaPerformance, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly TQL Waterfall Report \n The assert value of TQL performance is " + QuaterlyPerformanceList[i % 4].ToString() + ".");
                        QuaPerformance = 0;
                    }
                }
            }
        }

        public void VerifyWaterfall_MQLProjected(List<double> ProjectedList, List<double> GoalList, List<double> ActualList, string IsQuaterly, List<double> QuaterlyPerformanceList = null)
        {
            if (IsQuaterly == "Monthly")
            {
                if (MonthDiff >= 0)
                {
                    if (DateTime.Now.Date >= TacticStartDate && DateTime.Now.Date <= TacticEndDate)
                    {

                        for (int i = 1; i <= GoalList.Count(); i++)
                        {
                            int currentMonthNo = DateTime.Now.Month - TacticStartDate.Month + 1;
                            if (DateTime.Now.Month == i)
                            {
                                double proCal = (GoalList[i] / MonthDiff) * currentMonthNo;
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), Convert.ToDouble(proCal));
                                Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of projected TQL is " + ProjectedList[i - 1].ToString() + ".");
                            }
                            else if (DateTime.Now.Month < i)
                            {
                                double proCal = 0;
                                int current = i - DateTime.Now.Month + currentMonthNo;
                                double cal = (GoalList[i - 1] / MonthDiff) * current;
                                for (int j = 0; j < ActualList.Count(); j++)
                                {
                                    proCal = proCal + ActualList[j];
                                }
                                var final = proCal / DateTime.Now.Month;
                                var profinal = cal + final;
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), Convert.ToDouble(profinal));
                                Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of projected TQL is " + ProjectedList[i - 1].ToString() + ".");
                            }
                            else
                            {
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), 0);
                                Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of projected TQL is " + ProjectedList[i - 1].ToString() + ".");
                            }
                        }
                    }
                }
            }
            else
            {
                decimal QuaProjected = 0;
                for (int i = 0; i < ProjectedList.Count(); i++)
                {
                    QuaProjected = QuaProjected + Convert.ToDecimal(ProjectedList[i].ToString());
                    if (i % 4 < 0)
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyProjectedList[i % 4].ToString()), 2), Math.Round(QuaProjected, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly TQL Waterfall Report \n The assert value of projected TQL is " + QuaterlyProjectedList[i % 4].ToString() + ".");
                        QuaProjected = 0;
                    }
                }
            }
        }

        public void VerifyWaterfall_MQLTotal(List<double> ActualList, List<string> TotalActualList, string IsQuaterly, List<string> QuaterlyTotalActualList = null)
        {
            decimal Total = 0;
            if (IsQuaterly == "Monthly")
            {
                for (int i = 1; i <= ActualList.Count(); i++)
                {

                    if (i <= currentMonth)
                    {
                        Total = Total + Convert.ToDecimal(ActualList[i - 1].ToString());
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(TotalActualList[i - 1].ToString()), 2), Math.Round(Total, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of total TQL is " + TotalActualList[i - 1].ToString() + ".");
                    }
                    else
                    {
                        Assert.AreEqual(TotalActualList[i - 1].ToString(), "0");
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of total TQL is " + TotalActualList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                decimal QuaTotal = 0;
                for (int i = 0; i < QuaterlyTotalActualList.Count(); i++)
                {
                    QuaTotal = QuaTotal + Convert.ToDecimal(TotalActualList[i].ToString());
                    if (i % 4 < 0)
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyTotalActualList[i % 4].ToString()), 2), Math.Round(QuaTotal, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly TQL Waterfall Report \n The assert value of total TQL is " + QuaterlyTotalActualList[i % 4].ToString() + ".");
                        QuaTotal = 0;
                    }
                }
            }

        }

        public void VerifyWaterfall_MQLCardSection(List<double> ActualList)
        {
            decimal cardActual = 0;
            foreach (var actual in ActualList)
            {
                cardActual = cardActual + Convert.ToDecimal(actual);
            }
            Assert.AreEqual(Math.Round(cardActual, 2), Math.Round(CardMQLActual, 2));
            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter  \n Report - TQL Waterfall Report Card Section \n The assert value of TQL actual " + Math.Round(CardMQLActual, 2).ToString() + ".");

            Assert.AreEqual(Math.Round(TacticTQL, 2), Math.Round(CardMQLGoal, 2));
            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter  \n Report - TQL Waterfall Report Card Section \n The assert value of TQL goal " + Math.Round(CardMQLGoal, 2).ToString() + ".");

            decimal cardPercentage = ((cardActual - TacticTQL) / TacticTQL) * 100;
            Assert.AreEqual(Math.Round(cardPercentage, 2), Math.Round(CardMQLPercentage, 2));
            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter  \n Report - TQL Waterfall Report Card Section \n The assert value of TQL percentage " + Math.Round(CardMQLPercentage, 2).ToString() + ".");
        }

        #endregion

        #region Header calculation

        public void VerifyHeaderValue(Projected_Goal objProjected_Goal, List<double> ActualList, List<double> GoalList, List<double> ProjectedList)
        {
            #region Actual
            // Get actual projected value
            decimal GoalUpToCurrentMonth = 0; decimal actualPerecentage = 0; decimal SumOfProjected = 0; decimal TotalProjected = 0; decimal ProjectedPercentage = 0;
            foreach (decimal actual in ActualList)
            {
                actualProjected = actualProjected + actual;
            }
            Assert.AreEqual(Math.Round(actualProjected, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.Actual_Projected), 2));
            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - TQL Waterfall Report Header Section \n The assert value of actual TQL in header is " + objProjected_Goal.Actual_Projected + ".");

            // Get actual perecentage value
            for (int i = 1; i <= GoalList.Count(); i++)
            {
                if (currentMonth >= i)
                    GoalUpToCurrentMonth = GoalUpToCurrentMonth + Convert.ToDecimal(GoalList[i - 1].ToString());
            }
            actualPerecentage = ((actualProjected - GoalUpToCurrentMonth) / GoalUpToCurrentMonth) * 100;
            Assert.AreEqual(Math.Round(actualPerecentage, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.ActualPercentage), 2));
            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter  \n Report - TQL Waterfall Report Header Section \n The assert value of actual TQL perecentage in header is " + objProjected_Goal.Actual_Projected + ".");

            #endregion

            #region Projected
            // Get projected value
            for (int i = 0; i < ProjectedList.Count(); i++)
            {
                SumOfProjected = SumOfProjected + Convert.ToDecimal(ProjectedList[i].ToString());
            }
            TotalProjected = SumOfProjected + actualProjected;
            Assert.AreEqual(Math.Round(TotalProjected, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.Projected), 2));
            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - TQL Waterfall Report Header Section \n The assert value of projected TQL in header is " + Math.Round(TotalProjected, 2).ToString() + ".");

            //Calculation for projected percentage
            ProjectedPercentage = ((TotalProjected - TacticTQL) / TacticTQL) * 100;
            Assert.AreEqual(Math.Round(ProjectedPercentage, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.ProjectedPercentage), 2));
            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - TQL Waterfall Report Header Section \n The assert value of projected TQL perecentage in header is " + Math.Round(ProjectedPercentage, 2).ToString() + ".");

            #endregion
        }

        #endregion

        #endregion

        #region Common Function

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

            SetValuesForCardReport();


            if (isQuarterly == "Monthly")
            {
                ActualList = objConversionDataTable.ActualList;
                ProjectedList = objConversionDataTable.ProjectedList;
                GoalList = objConversionDataTable.GoalList;
                PerformanceList = SubDataTableModel.PerformanceList;
                TotalActualList = SubDataTableModel.RevenueList;
            }
            else
            {
                QuaterlyActualList = objConversionDataTable.ActualList;
                QuaterlyProjectedList = objConversionDataTable.ProjectedList;
                QuaterlyGoalList = objConversionDataTable.GoalList;
                QuaterlyPerformanceList = SubDataTableModel.PerformanceList;
                QuaterlyTotalActualList = SubDataTableModel.RevenueList;
            }
        }

        public void SetValuesForCardReport()
        {
            objReportModel = new ReportModel();

            var result1 = objReportController.GetWaterFallData(currentYear, "false") as PartialViewResult;
            objReportModel = (ReportModel)(result1.ViewData.Model);

            objCardSection = objReportModel.CardSectionModel;
        }

        public void CheckValueForCard(string Stage)
        {
            objReportModel = new ReportModel();
            var result1 = objReportController.GetWaterFallData(currentYear, "false") as PartialViewResult;
            objReportModel = (ReportModel)(result1.ViewData.Model);
            objCardSection = objReportModel.CardSectionModel;
            var Card = objCardSection.CardSectionListModel[0];
            if (objCardSection != null)
            {

                CardMQLActual = Convert.ToDecimal(Card.TQLCardValues.Actual_Projected);
                CardMQLPercentage = Convert.ToDecimal(Card.TQLCardValues.Percentage);
                CardMQLGoal = Convert.ToDecimal(Card.TQLCardValues.Goal);


            }
        }

        #endregion
    }
}

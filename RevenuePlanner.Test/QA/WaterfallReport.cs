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
    public class WaterfallReport
    {
        #region Variable Declaration

        Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();
        PlanCommonFunctions ObjPlanCommonFunctions = new PlanCommonFunctions();

        DateTime NewStartDate; DateTime NewEndDate; DateTime TacticEndDate; DateTime TacticStartDate; int MonthDiff = 0; int currentMonth = DateTime.Now.Month;
        // decimal TacticRevenueAmount = 0;
        decimal TacticINQ = 0; decimal TacticProjectedCost = 0; static string currentYear = DateTime.Now.Year.ToString();
        decimal TacticTQL = 0; decimal GoalAmount = 0; decimal TacticCW = 0; decimal actualProjected = 0;

        static List<double> ActualList; static List<double> ProjectedList; static List<double> GoalList; static List<string> PerformanceList;
        static List<string> TotalActualList;
        List<double> QuaterlyActualList; List<double> QuaterlyProjectedList; List<double> QuaterlyGoalList; List<string> QuaterlyPerformanceList;
        List<string> QuaterlyTotalActualList;

        ReportController objReportController; ReportModel objReportModel; ConversionToPlanModel objConversionToPlanModel; CardSectionModel objCardSection;
        ConversionDataTable objConversionDataTable; ConversionSubDataTableModel SubDataTableModel; Projected_Goal objProjected_Goal;

        static decimal CardINQActual = 0; static decimal CardINQGoal = 0; static decimal CardINQPercentage = 0;
        static decimal CardMQLActual = 0; static decimal CardMQLGoal = 0; static decimal CardMQLPercentage = 0;
        static decimal CardCWActual = 0; static decimal CardCWGoal = 0; static decimal CardCWPercentage = 0;

        static decimal TotalProjected = 0;

        static string[] MonthList = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        static string[] QuarterList = { "Q1", "Q2", "Q3", "Q4" };
        static int[] num = { 2, 5, 8, 11 };
        #endregion

        #region CW Report Calculation

        [TestMethod()]
        public void CWWaterfallReport()
        {
            try
            {
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Assert.AreEqual("Index", IsLogin.RouteValues["Action"]);
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value of Action : " + IsLogin.RouteValues["Action"]);
                    ObjPlanCommonFunctions.SetSessionData();
                    MonthlyCWWaterfallReportTest();
                    QuaterlyCWWaterfallReportTest();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void MonthlyCWWaterfallReportTest()
        {
            Console.WriteLine("-------------- Monthly Report -------------- \n \n");
            SetValuesForCWReport(currentYear, "Monthly", "CW");
            DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                VerifyWaterfall_CWActual(dt, ActualList, "Monthly");
                VerifyWaterfall_CWGoal(GoalList, "Monthly");
                VerifyWaterfall_CWPerformance(ActualList, PerformanceList, "Monthly");
                VerifyWaterfall_CWProjected(ProjectedList, GoalList, ActualList, "Monthly");
                VerifyWaterfall_CWTotal(ActualList, TotalActualList, "Monthly");
                CheckValueForCWCard("CW");
                VerifyWaterfall_CWCardSection(ActualList);
            }
        }

        public void QuaterlyCWWaterfallReportTest()
        {
            Console.WriteLine("\n \n-------------- Quarterly Report -------------- \n \n");
            SetValuesForCWReport(currentYear, "Quarterly", "CW");
            DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                VerifyWaterfall_CWActual(dt, ActualList, "Quarterly", QuaterlyActualList);
                VerifyWaterfall_CWGoal(GoalList, "Quarterly", QuaterlyGoalList);
                VerifyWaterfall_CWPerformance(QuaterlyActualList, PerformanceList, "Quarterly", QuaterlyPerformanceList, QuaterlyGoalList);
                VerifyWaterfall_CWProjected(ProjectedList, GoalList, ActualList, "Quarterly", QuaterlyProjectedList);
                VerifyWaterfall_CWTotal(ActualList, TotalActualList, "Quarterly", QuaterlyTotalActualList);
            }
        }

        #region  CW Calculation

        public void VerifyWaterfall_CWActual(DataTable dt, List<double> ActualList, string IsQuaterly, List<double> QuaterlyActualList = null)
        {
            if (IsQuaterly == "Monthly")
            {
                if (dt.Rows[2] != null)
                {
                    for (int i = 1; i <= dt.Columns.Count - 1; i++)
                    {
                        if (i > currentMonth)
                        {
                            dt.Rows[2][i] = 0;
                        }
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(ActualList[i - 1]), 2), (Math.Round(Convert.ToDecimal(dt.Rows[2].ItemArray[i].ToString()))));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly CW Waterfall Report \n The assert value of actual CW in " + MonthList[i - 1] + " is " + ActualList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                decimal QuaActual = 0; int j = 0;
                for (int i = 0; i < ActualList.Count(); i++)
                {
                    QuaActual = QuaActual + Convert.ToDecimal(ActualList[i].ToString());
                    if (num.Contains(i))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyActualList[j].ToString()), 2), Math.Round(QuaActual, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly CW Waterfall Report \n The assert value of actual CW in " + QuarterList[j] + " is " + QuaterlyActualList[j].ToString() + ".");
                        QuaActual = 0; j++;
                    }
                }
            }
        }

        public void VerifyWaterfall_CWGoal(List<double> GoalList, string IsQuaterly, List<double> QuaterlyGoalList = null)
        {
            if (IsQuaterly == "Monthly")
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
                TacticCW = ObjPlanCommonFunctions.CalculationForTactic(drModel, TacticTQL, "CW");

                GoalAmount = TacticCW / MonthDiff;

                for (int i = 1; i <= GoalList.Count; i++)
                {
                    if (i >= NewStartDate.Month && i <= NewEndDate.Month)
                    {
                        Assert.AreEqual(Convert.ToDouble(GoalList[i - 1].ToString()), Math.Round(Convert.ToDouble(GoalAmount)), 2);
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly CW Waterfall Report \n The assert value of CW goal in " + MonthList[i - 1] + " is " + GoalList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                decimal QuaGoal = 0; int j = 0;
                for (int i = 0; i < GoalList.Count(); i++)
                {
                    QuaGoal = QuaGoal + Convert.ToDecimal(GoalList[i].ToString());
                    if (num.Contains(i))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyGoalList[j].ToString()), 2), Math.Round(QuaGoal, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly CW Waterfall Report \n The assert value of CW goal in " + QuarterList[j] + " is " + QuaterlyGoalList[j].ToString() + ".");
                        QuaGoal = 0; j++;
                    }
                }
            }
        }

        public void VerifyWaterfall_CWPerformance(List<double> ActualList, List<string> PerformanceList, string IsQuaterly, List<string> QuaterlyPerformanceList = null, List<double> GoalList = null)
        {
            if (IsQuaterly == "Monthly")
            {
                DataTable dtTactic = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Tactic$]").Tables[0];
                DataRow drTactic = dtTactic.Rows[0];

                DateTime TQLStartDate = Convert.ToDateTime(drTactic["TQLStartDate"].ToString());
                DateTime TQLEndDate = Convert.ToDateTime(drTactic["TQLEndDate"].ToString());
                for (int i = 1; i <= ActualList.Count(); i++)
                {
                    if (NewStartDate.Month <= i && NewEndDate.Month >= i)
                    {
                        decimal calculatePer = ((Convert.ToDecimal(ActualList[i - 1].ToString()) - GoalAmount) / GoalAmount) * 100;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(PerformanceList[i - 1].ToString()), 2), Math.Round(calculatePer, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly CW Waterfall Report \n The assert value of CW performance in " + MonthList[i - 1] + " is " + PerformanceList[i - 1].ToString() + ".");
                    }
                    else
                    {
                        Assert.AreEqual(Convert.ToDouble(PerformanceList[i - 1].ToString()), 0);
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly CW Waterfall Report \n The assert value of CW performance in " + MonthList[i - 1] + " is " + PerformanceList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                decimal QuaPerformance = 0;
                for (int i = 0; i < ActualList.Count(); i++)
                {
                    if (Convert.ToDecimal(GoalList[i]) != 0)
                        QuaPerformance = Convert.ToDecimal((ActualList[i] - GoalList[i]) / GoalList[i]) * 100;

                    Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyPerformanceList[i].ToString()), 2), Math.Round(QuaPerformance, 2));
                    Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly CW Waterfall Report \n The assert value of CW performance in " + QuarterList[i] + " is " + QuaterlyPerformanceList[i].ToString() + ".");
                }
            }
        }

        public void VerifyWaterfall_CWProjected(List<double> ProjectedList, List<double> GoalList, List<double> ActualList, string IsQuaterly, List<double> QuaterlyPerformanceList = null)
        {
            if (IsQuaterly == "Monthly")
            {
                if (MonthDiff >= 0)
                {
                    if (DateTime.Now.Date >= NewStartDate && DateTime.Now.Date <= NewEndDate)
                    {

                        for (int i = 1; i <= GoalList.Count(); i++)
                        {
                            int currentMonthNo = DateTime.Now.Month - NewStartDate.Month + 1;
                            if (DateTime.Now.Month == i)
                            {
                                double proCal = (GoalList[i - 1] / MonthDiff) * currentMonthNo;
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), Convert.ToDouble(proCal));
                                Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly CW Waterfall Report \n The assert value of projected CW in " + MonthList[i - 1] + " is " + ProjectedList[i - 1].ToString() + ".");
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
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), Math.Round(Convert.ToDouble(ProjectedList[i - 1].ToString()), 2), Convert.ToDouble(profinal));
                                Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly CW Waterfall Report \n The assert value of projected CW in " + MonthList[i - 1] + " is " + ProjectedList[i - 1].ToString() + ".");
                            }
                            else
                            {
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), 0);
                                Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly CW Waterfall Report \n The assert value of projected CW in " + MonthList[i - 1] + " is " + ProjectedList[i - 1].ToString() + ".");
                            }
                        }
                    }
                }
            }
            else
            {
                decimal QuaProjected = 0; int j = 0;
                for (int i = 0; i < ProjectedList.Count(); i++)
                {
                    QuaProjected = QuaProjected + Convert.ToDecimal(ProjectedList[i].ToString());
                    if (num.Contains(i))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyProjectedList[j].ToString()), 2), Math.Round(QuaProjected, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly CW Waterfall Report \n The assert value of projected in " + QuarterList[j] + " is " + QuaterlyProjectedList[j].ToString() + " .");
                        QuaProjected = 0; j++;
                    }
                }
            }
        }

        public void VerifyWaterfall_CWTotal(List<double> ActualList, List<string> TotalActualList, string IsQuaterly, List<string> QuaterlyTotalActualList = null)
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly CW Waterfall Report \n The assert value of total CW in " + MonthList[i - 1] + " is " + TotalActualList[i - 1].ToString() + ".");
                    }
                    else
                    {
                        Assert.AreEqual(TotalActualList[i - 1].ToString(), "0");
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly CW Waterfall Report \n The assert value of total CW in " + MonthList[i - 1] + " is " + TotalActualList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                int j = 2; int k = 0;
                for (int i = 0; i < TotalActualList.Count(); i++)
                {
                    if (i == j)
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyTotalActualList[k].ToString()), 2), Math.Round(Convert.ToDecimal(TotalActualList[i].ToString()), 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly CW Waterfall Report \n The assert value of total CW in " + QuarterList[k] + " is " + QuaterlyTotalActualList[k].ToString() + ".");
                        j = j + 3; k++;
                    }
                }
            }
        }

        public void VerifyWaterfall_CWCardSection(List<double> ActualList)
        {
            decimal cardActual = 0;
            foreach (var actual in ActualList)
            {
                cardActual = cardActual + Convert.ToDecimal(actual);
            }
            Assert.AreEqual(Math.Round(cardActual, 2), Math.Round(CardCWActual, 2));
            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - CW Waterfall Report Card Section \n The assert value of CW actual is " + Math.Round(CardCWActual, 2).ToString() + " .");

            Assert.AreEqual(Math.Round(TacticCW, 1), Math.Round(CardCWGoal, 1));
            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - CW Waterfall Report Card Section \n The assert value of CW actual goal is " + Math.Round(CardCWGoal, 2).ToString() + " .");

            decimal cardPercentage = ((cardActual - Math.Round(TacticCW, 1)) / Math.Round(TacticCW, 1)) * 100;
            Assert.AreEqual(Math.Round(cardPercentage, 2), Math.Round(CardCWPercentage, 2));
            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - CW Waterfall Report Card Section  \n The assert value of CW actual percentage is " + Math.Round(CardINQActual, 2).ToString() + " .");


        }

        #endregion

        #region Common Function

        public void SetValuesForCWReport(string Year, string isQuarterly, string Stage)
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

            SetValuesForCWCardReport();


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

        public void SetValuesForCWCardReport()
        {
            objReportModel = new ReportModel();

            var result1 = objReportController.GetWaterFallData(currentYear, "false") as PartialViewResult;
            objReportModel = (ReportModel)(result1.ViewData.Model);

            objCardSection = objReportModel.CardSectionModel;
        }

        public void CheckValueForCWCard(string Stage)
        {
            objReportModel = new ReportModel();
            var result1 = objReportController.GetWaterFallData(currentYear, "false") as PartialViewResult;
            objReportModel = (ReportModel)(result1.ViewData.Model);
            objCardSection = objReportModel.CardSectionModel;
            var Card = objCardSection.CardSectionListModel[0];
            if (objCardSection != null)
            {
                if (Stage == "ProjectedStageValue")
                {
                    CardINQActual = Convert.ToDecimal(Card.INQCardValues.Actual_Projected);
                    CardINQPercentage = Convert.ToDecimal(Card.INQCardValues.Percentage);
                    CardINQGoal = Convert.ToDecimal(Card.INQCardValues.Goal);
                }
                else if (Stage == "MQL")
                {
                    CardMQLActual = Convert.ToDecimal(Card.TQLCardValues.Actual_Projected);
                    CardMQLPercentage = Convert.ToDecimal(Card.TQLCardValues.Percentage);
                    CardMQLGoal = Convert.ToDecimal(Card.TQLCardValues.Goal);
                }
                else
                {
                    CardCWActual = Convert.ToDecimal(Card.CWCardValues.Actual_Projected);
                    CardCWPercentage = Convert.ToDecimal(Card.CWCardValues.Percentage);
                    CardCWGoal = Convert.ToDecimal(Card.CWCardValues.Goal);
                }
            }
        }

        #endregion

        #endregion

        #region  INQ Report Calculation

        [TestMethod()]
        public void INQWaterfallReport()
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
                    MonthlyINQWaterfallReportTest();
                    QuaterlyINQWaterfallReportTest();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void MonthlyINQWaterfallReportTest()
        {
            Console.WriteLine("-------------- Monthly Report -------------- \n \n");
            SetValuesForINQReport(currentYear, "Monthly", "ProjectedStageValue");
            DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                VerifyWaterfall_INQActual(dt, ActualList, "Monthly");
                VerifyWaterfall_INQGoal(GoalList, "Monthly");
                VerifyWaterfall_INQPerformance(ActualList, PerformanceList, "Monthly");
                VerifyWaterfall_INQProjected(ProjectedList, GoalList, ActualList, "Monthly");
                VerifyWaterfall_INQTotal(ActualList, TotalActualList, "Monthly");
                CheckValueForINQCard("ProjectedStageValue");
                VerifyWaterfall_INQCardSection(ActualList);
            }
        }

        public void QuaterlyINQWaterfallReportTest()
        {
            try
            {
                //Call common function for login
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Assert.AreEqual("Index", IsLogin.RouteValues["Action"]);
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value of Action : " + IsLogin.RouteValues["Action"]);
                    SetValuesForINQReport(currentYear.ToString(), "Quarterly", "ProjectedStageValue");
                    DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
                    Console.WriteLine("\n \n-------------- Quarterly Report -------------- \n \n");
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        VerifyWaterfall_INQActual(dt, ActualList, "Quarterly", QuaterlyActualList);
                        VerifyWaterfall_INQGoal(GoalList, "Quarterly", QuaterlyGoalList);
                        VerifyWaterfall_INQPerformance(QuaterlyActualList, PerformanceList, "Quarterly", QuaterlyPerformanceList, QuaterlyGoalList);
                        VerifyWaterfall_INQProjected(ProjectedList, GoalList, ActualList, "Quarterly", QuaterlyProjectedList);
                        VerifyWaterfall_INQTotal(ActualList, TotalActualList, "Quarterly", QuaterlyTotalActualList);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #region  INQ Calculation

        public void VerifyWaterfall_INQActual(DataTable dt, List<double> ActualList, string IsQuaterly, List<double> QuaterlyActualList = null)
        {
            if (IsQuaterly == "Monthly")
            {
                if (dt.Rows[0] != null)
                {
                    for (int i = 1; i <= dt.Columns.Count - 1; i++)
                    {
                        if (i > currentMonth)
                        {
                            dt.Rows[0][i] = 0;
                        }
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(ActualList[i - 1]), 2), Math.Round(Convert.ToDecimal(dt.Rows[0].ItemArray[i].ToString()), 2));
                        Console.WriteLine("ReportController - GetRevenueData \n Report - Monthly Inquiry Waterfall Report \n The assert value of actual inquiry in " + MonthList[i - 1] + " is" + ActualList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                decimal QuaActual = 0; int j = 0;
                for (int i = 0; i < ActualList.Count(); i++)
                {
                    QuaActual = QuaActual + Convert.ToDecimal(ActualList[i].ToString());
                    if (num.Contains(i))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyActualList[j].ToString()), 2), Math.Round(QuaActual, 2));
                        Console.WriteLine("ReportController - GetRevenueData \n Report - Quaterly Inquiry Waterfall Report \n The assert value of actual inquiry in " + QuarterList[j] + " is" + QuaterlyActualList[j].ToString().ToString() + ".");
                        QuaActual = 0; j++;
                    }
                }
            }
        }

        public void VerifyWaterfall_INQGoal(List<double> GoalList, string IsQuaterly, List<double> QuaterlyGoalList = null)
        {
            if (IsQuaterly == "Monthly")
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

                GoalAmount = TacticINQ / MonthDiff;

                for (int i = 1; i <= GoalList.Count; i++)
                {
                    if (i >= TacticStartDate.Month && i <= TacticEndDate.Month)
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(GoalList[i - 1].ToString()), 2), Math.Round(Convert.ToDouble(GoalAmount)), 2);
                        Console.WriteLine("ReportController - GetRevenueData  \n Report - Monthly Inquiry Waterfall Report \n The assert value of inquiry goal in " + MonthList[i - 1] + " is " + GoalList[i - 1].ToString() + ".");
                    }
                    else
                    {
                        Assert.AreEqual(GoalList[i - 1].ToString(), "0");
                        Console.WriteLine("ReportController - GetRevenueData \n Report - Monthly Inquiry Waterfall Report \n The assert value of inquiry goal in " + MonthList[i - 1] + " is " + GoalList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                decimal QuaGoal = 0; int j = 0;
                for (int i = 0; i < GoalList.Count(); i++)
                {
                    QuaGoal = QuaGoal + Convert.ToDecimal(GoalList[i].ToString());
                    if (num.Contains(i))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyGoalList[j].ToString()), 2), Math.Round(QuaGoal, 2));
                        Console.WriteLine("ReportController - GetRevenueData \n Report - Quaterly Inquiry Waterfall Report \n The assert value of inquiry goal in " + QuarterList[j] + " is " + QuaterlyGoalList[j].ToString() + ".");
                        QuaGoal = 0; j++;
                    }
                }
            }
        }

        public void VerifyWaterfall_INQPerformance(List<double> ActualList, List<string> PerformanceList, string IsQuaterly, List<string> QuaterlyPerformanceList = null, List<double> GoalList = null)
        {
            if (IsQuaterly == "Monthly")
            {
                for (int i = 1; i <= ActualList.Count(); i++)
                {
                    if (TacticStartDate.Month <= i && TacticEndDate.Month >= i)
                    {
                        decimal calculatePer = ((Convert.ToDecimal(ActualList[i - 1].ToString()) - GoalAmount) / GoalAmount) * 100;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(PerformanceList[i - 1].ToString()), 2), Math.Round(calculatePer, 2));
                        Console.WriteLine("ReportController - GetRevenueData \n Report - Monthly Inquiry Waterfall Report \n The assert value of inquiry performance in " + MonthList[i - 1] + " is " + PerformanceList[i - 1].ToString() + ".");
                    }
                    else
                    {
                        Assert.AreEqual(PerformanceList[i - 1].ToString(), "0");
                        Console.WriteLine("ReportController - GetRevenueData \n Report - Monthly Inquiry Waterfall Report \n The assert value of inquiry performance in " + MonthList[i - 1] + " is " + PerformanceList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                decimal QuaPerformance = 0;
                for (int i = 0; i < ActualList.Count(); i++)
                {
                    if (Convert.ToDecimal(GoalList[i]) != 0)
                        QuaPerformance = Convert.ToDecimal((ActualList[i] - GoalList[i]) / GoalList[i]) * 100;

                    Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyPerformanceList[i].ToString()), 2), Math.Round(QuaPerformance, 2));
                    Console.WriteLine("ReportController - GetRevenueData \n Report - Quaterly Inquiry Waterfall Report \n The assert value of inquiry performance in " + QuarterList[i] + " is " + QuaterlyPerformanceList[i].ToString() + ".");
                }
            }
        }

        public void VerifyWaterfall_INQProjected(List<double> ProjectedList, List<double> GoalList, List<double> ActualList, string IsQuaterly, List<double> QuaterlyPerformanceList = null)
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
                                double TotalProjected = (GoalList[i - 1] / MonthDiff) * currentMonthNo;
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(ProjectedList[i - 1].ToString()), 2), Math.Round(Convert.ToDecimal(TotalProjected), 2));
                                Console.WriteLine("ReportController - GetRevenueData \n Report - Monthly Inquiry Waterfall Report  \n The assert value of projected inquiry in " + MonthList[i - 1] + " is " + ProjectedList[i - 1].ToString() + ".");
                            }
                            else if (DateTime.Now.Month < i)
                            {
                                decimal proCal = 0;
                                int current = i - DateTime.Now.Month + currentMonthNo;
                                double cal = (GoalList[i - 1] / MonthDiff) * current;
                                for (int j = 0; j < ActualList.Count(); j++)
                                {
                                    proCal = proCal + Convert.ToDecimal(ActualList[j].ToString());
                                }
                                decimal final = proCal / DateTime.Now.Month;
                                TotalProjected = Convert.ToDecimal(cal) + final;
                                Assert.AreEqual(Math.Round(Convert.ToDouble(ProjectedList[i - 1].ToString()), 2), Math.Round(TotalProjected, 2));
                                Console.WriteLine("ReportController - GetRevenueData \n Report - Monthly Inquiry Waterfall Report \n The assert value of projected inquiry in " + MonthList[i - 1] + " is " + ProjectedList[i - 1].ToString() + ".");
                            }
                            else
                            {
                                Assert.AreEqual(ProjectedList[i - 1].ToString(), "0");
                                Console.WriteLine("ReportController - GetRevenueData \n Report - Monthly Inquiry Waterfall Report \n The assert value of projected inquiry in " + MonthList[i - 1] + " is " + ProjectedList[i - 1].ToString() + ".");
                            }
                        }
                    }
                }
            }
            else
            {
                decimal QuaProjected = 0; int j = 0;
                for (int i = 0; i < ProjectedList.Count(); i++)
                {
                    QuaProjected = QuaProjected + Convert.ToDecimal(ProjectedList[i].ToString());
                    if (num.Contains(i))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyProjectedList[j].ToString()), 2), Math.Round(QuaProjected, 2));
                        Console.WriteLine("ReportController - GetRevenueData \n Report - Quaterly Inquiry Waterfall Report  \n The assert value of projected inquiry in " + QuarterList[j] + " is " + QuaterlyProjectedList[j].ToString() + ".");
                        QuaProjected = 0; j++;
                    }
                }
            }
        }

        public void VerifyWaterfall_INQTotal(List<double> ActualList, List<string> TotalActualList, string IsQuaterly, List<string> QuaterlyTotalActualList = null)
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
                        Console.WriteLine("ReportController - GetRevenueData \n Report - Monthly Inquiry Waterfall Report  \n The assert value of inquiry total in " + MonthList[i - 1] + " is " + TotalActualList[i - 1].ToString() + ".");
                    }
                    else
                    {
                        Assert.AreEqual(TotalActualList[i - 1].ToString(), "0");
                        Console.WriteLine("ReportController - GetRevenueData \n Report - Monthly Inquiry Waterfall Report  \n The assert value of inquiry total in " + MonthList[i - 1] + " is " + TotalActualList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                int j = 2; int k = 0;
                for (int i = 0; i < TotalActualList.Count(); i++)
                {
                    if (i == j)
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyTotalActualList[k].ToString()), 2), Math.Round(Convert.ToDecimal(TotalActualList[i].ToString()), 2));
                        Console.WriteLine("ReportController - GetRevenueData \n Report - Quaterly Inquiry Waterfall Report  \n The assert value of inquiry total in " + QuarterList[k] + " is " + QuaterlyTotalActualList[k].ToString() + ".");
                        j = j + 3; k++;
                    }
                }

            }

        }

        public void VerifyWaterfall_INQCardSection(List<double> ActualList)
        {
            decimal cardActual = 0;
            foreach (var actual in ActualList)
            {
                cardActual = cardActual + Convert.ToDecimal(actual);
            }
            Assert.AreEqual(Math.Round(cardActual, 2), Math.Round(CardINQActual, 2));
            Console.WriteLine("ReportController - GetRevenueData \n Report - Inquiry Waterfall Report Card Section \n The assert value of actual inquiry is " + Math.Round(CardINQActual, 2).ToString() + ".");

            Assert.AreEqual(Math.Round(TacticINQ, 2), Math.Round(CardINQGoal, 2));
            Console.WriteLine("ReportController - GetRevenueData \n Report - Inquiry Waterfall Report Card Section \n The assert value of actual goal inquiry is " + Math.Round(CardINQActual, 2).ToString() + ".");

            decimal cardPercentage = ((cardActual - TacticINQ) / TacticINQ) * 100;
            Assert.AreEqual(Math.Round(cardPercentage, 2), Math.Round(CardINQPercentage, 2));
            Console.WriteLine("ReportController - GetRevenueData \n Report - Inquiry Waterfall Report Card Section \n The assert value of actual percentage inquiry is " + Math.Round(CardINQActual, 2).ToString() + ".");
        }

        #endregion

        #region Common Function

        public void SetValuesForINQReport(string Year, string isQuarterly, string Stage)
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

            SetValuesForINQCardReport();


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

        public void SetValuesForINQCardReport()
        {
            objReportModel = new ReportModel();

            var result1 = objReportController.GetWaterFallData(currentYear, "false") as PartialViewResult;
            objReportModel = (ReportModel)(result1.ViewData.Model);
            objCardSection = objReportModel.CardSectionModel;
        }

        public void CheckValueForINQCard(string Stage)
        {
            objReportModel = new ReportModel();
            var result1 = objReportController.GetWaterFallData(currentYear, "false") as PartialViewResult;
            objReportModel = (ReportModel)(result1.ViewData.Model);
            objCardSection = objReportModel.CardSectionModel;
            var Card = objCardSection.CardSectionListModel[0];
            if (objCardSection != null)
            {
                CardINQActual = Convert.ToDecimal(Card.INQCardValues.Actual_Projected);
                CardINQPercentage = Convert.ToDecimal(Card.INQCardValues.Percentage);
                CardINQGoal = Convert.ToDecimal(Card.INQCardValues.Goal);
            }
        }

        #endregion

        #endregion

        #region  MQL Report Calculation

        [TestMethod()]
        public void MQLWaterfallReport()
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
            Console.WriteLine("-------------- Monthly Report -------------- \n \n");
            SetValuesForMQLReport(currentYear, "Monthly", "MQL");
            DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                VerifyWaterfall_MQLActual(dt, ActualList, "Monthly");
                VerifyWaterfall_MQLGoal(GoalList, "Monthly");
                VerifyWaterfall_MQLPerformance(ActualList, PerformanceList, "Monthly");
                VerifyWaterfall_MQLProjected(ProjectedList, GoalList, ActualList, "Monthly");
                VerifyWaterfall_MQLTotal(ActualList, TotalActualList, "Monthly");
                VerifyHeaderValue(objProjected_Goal, ActualList, GoalList, ProjectedList);
                CheckValueForMQLCard("MQL");
                VerifyWaterfall_MQLCardSection(ActualList);
            }

        }

        public void QuaterlyMQLWaterfallReportTest()
        {
            Console.WriteLine("\n \n-------------- Quarterly Report -------------- \n \n");
            SetValuesForMQLReport(currentYear, "Quarterly", "MQL");
            DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                VerifyWaterfall_MQLActual(dt, ActualList, "Quarterly", QuaterlyActualList);
                VerifyWaterfall_MQLGoal(GoalList, "Quarterly", QuaterlyGoalList);
                VerifyWaterfall_MQLPerformance(QuaterlyActualList, PerformanceList, "Quarterly", QuaterlyPerformanceList, QuaterlyGoalList);
                VerifyWaterfall_MQLProjected(ProjectedList, QuaterlyGoalList, QuaterlyActualList, "Quarterly", QuaterlyProjectedList);
                VerifyWaterfall_MQLTotal(ActualList, TotalActualList, "Quarterly", QuaterlyTotalActualList);
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of actual TQL in " + MonthList[i - 1] + " is " + ActualList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                decimal QuaActual = 0; int j = 0;
                for (int i = 0; i < ActualList.Count(); i++)
                {
                    QuaActual = QuaActual + Convert.ToDecimal(ActualList[i].ToString());
                    if (num.Contains(i))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyActualList[j].ToString()), 2), Math.Round(QuaActual, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly TQL Waterfall Report \n The assert value of actual TQL in " + QuarterList[j] + " is " + QuaterlyActualList[j].ToString() + ".");
                        QuaActual = 0; j++;
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of TQL goal in " + MonthList[i - 1] + " is " + GoalList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                decimal QuaGoal = 0; int j = 0;
                for (int i = 0; i < GoalList.Count(); i++)
                {
                    QuaGoal = QuaGoal + Convert.ToDecimal(GoalList[i].ToString());
                    if (num.Contains(i))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyGoalList[j].ToString()), 2), Math.Round(QuaGoal, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly TQL Waterfall Report  \n The assert value of TQL goal in " + QuarterList[j] + " is " + QuaterlyGoalList[j].ToString() + ".");
                        QuaGoal = 0; j++;
                    }
                }
            }
        }

        public void VerifyWaterfall_MQLPerformance(List<double> ActualList, List<string> PerformanceList, string IsQuaterly, List<string> QuaterlyPerformanceList = null, List<double> GoalList = null)
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of TQL performance in " + MonthList[i - 1] + " is " + PerformanceList[i - 1].ToString() + ".");
                    }
                    else
                    {
                        Assert.AreEqual(Convert.ToDouble(PerformanceList[i - 1].ToString()), 0);
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of TQL performance in " + MonthList[i - 1] + " is " + PerformanceList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                decimal QuaPerformance = 0;
                for (int i = 0; i < ActualList.Count(); i++)
                {
                    if (Convert.ToDecimal(GoalList[i]) != 0)
                        QuaPerformance = Convert.ToDecimal((ActualList[i] - GoalList[i]) / GoalList[i]) * 100;

                    Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyPerformanceList[i].ToString()), 2), Math.Round(QuaPerformance, 2));
                    Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly TQL Waterfall Report \n The assert value of TQL performance in " + QuarterList[i] + " is " + QuaterlyPerformanceList[i].ToString() + ".");
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
                                double proCal = (GoalList[i - 1] / MonthDiff) * currentMonthNo;
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), Convert.ToDouble(proCal));
                                Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of projected TQL in " + MonthList[i - 1] + " is " + ProjectedList[i - 1].ToString() + ".");
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
                                Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of projected TQL in " + MonthList[i - 1] + " is " + ProjectedList[i - 1].ToString() + ".");
                            }
                            else
                            {
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), 0);
                                Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of projected TQL in " + MonthList[i - 1] + " is " + ProjectedList[i - 1].ToString() + ".");
                            }
                        }
                    }
                }
            }
            else
            {
                decimal QuaProjected = 0; int j = 0;
                for (int i = 0; i < ProjectedList.Count(); i++)
                {
                    QuaProjected = QuaProjected + Convert.ToDecimal(ProjectedList[i].ToString());
                    if (num.Contains(i))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyProjectedList[j].ToString()), 2), Math.Round(QuaProjected, 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly TQL Waterfall Report \n The assert value of projected TQL in " + QuarterList[j] + " is " + QuaterlyProjectedList[j].ToString() + ".");
                        QuaProjected = 0; j++;
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of total TQL in " + MonthList[i - 1] + " is " + TotalActualList[i - 1].ToString() + ".");
                    }
                    else
                    {
                        Assert.AreEqual(TotalActualList[i - 1].ToString(), "0");
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Monthly TQL Waterfall Report \n The assert value of total TQL in " + MonthList[i - 1] + " is " + TotalActualList[i - 1].ToString() + ".");
                    }
                }
            }
            else
            {
                int j = 2; int k = 0;
                for (int i = 0; i < TotalActualList.Count(); i++)
                {
                    if (i == j)
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(QuaterlyTotalActualList[k].ToString()), 2), Math.Round(Convert.ToDecimal(TotalActualList[i].ToString()), 2));
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n Report - Quaterly TQL Waterfall Report \n The assert value of total TQL in " + QuarterList[k] + " is " + QuaterlyTotalActualList[k].ToString() + ".");
                        j = j + 3; k++;
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

        #region Common Function

        public void SetValuesForMQLReport(string Year, string isQuarterly, string Stage)
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

            SetValuesForMQLCardReport();


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

        public void SetValuesForMQLCardReport()
        {
            objReportModel = new ReportModel();

            var result1 = objReportController.GetWaterFallData(currentYear, "false") as PartialViewResult;
            objReportModel = (ReportModel)(result1.ViewData.Model);

            objCardSection = objReportModel.CardSectionModel;
        }

        public void CheckValueForMQLCard(string Stage)
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

        #endregion
    }
}

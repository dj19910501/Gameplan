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
using System.Web.Mvc;

namespace RevenuePlanner.Test.QA.ReportsIntegrationTest
{
    [TestClass]
    public class WaterfallReportTestCase
    {
        #region Variable Declaration

        Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();
        PlanCommonFunctions ObjPlanCommonFunctions = new PlanCommonFunctions();

        DateTime NewStartDate; DateTime NewEndDate; DateTime TacticEndDate; DateTime TacticStartDate; int MonthDiff = 0; int currentMonth = DateTime.Now.Month;
        decimal TacticRevenueAmount = 0; decimal TacticINQ = 0; decimal TacticProjectedCost = 0;
        decimal PlanBudget = 0; decimal TacticTQL = 0; decimal GoalAmount = 0; decimal actualProjected = 0;

        List<int> PlanIds; List<int> ReportOwnerIds; List<int> ReportTacticTypeIds;

        static List<double> ActualList; static List<double> ProjectedList; static List<double> GoalList; static List<string> PerformanceList;
        static List<string> TotalActualList;
        List<double> QuaterlyActualList; List<double> QuaterlyProjectedList; List<double> QuaterlyGoalList; List<string> QuaterlyPerformanceList;
        List<string> QuaterlyTotalActualList;

        ReportController objReportController; ReportModel objReportModel; ConversionToPlanModel objConversionToPlanModel; CardSectionModel objCardSection;
        ConversionDataTable objConversionDataTable; ConversionSubDataTableModel SubDataTableModel; Projected_Goal objProjected_Goal;

        #endregion

        [TestMethod()]
        [Priority(1)]
        public void MonthlyINQWaterfallReportTest()
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
                    SetValuesForReport("2016", "Monthly", "ProjectedStageValue");
                    DataTable dt = ObjCommonFunctions.GetExcelData("ExcelConn", "[Actual Data$]").Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        VerifyWaterfall_INQActual(dt, ActualList, "Monthly");
                        VerifyWaterfall_INQGoal(GoalList, "Monthly");
                        VerifyWaterfall_INQPerformance(ActualList, PerformanceList, "Monthly");
                        VerifyWaterfall_INQProjected(ProjectedList, GoalList, ActualList, "Monthly");
                        VerifyWaterfall_INQTotal(ActualList, TotalActualList, "Monthly");
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [TestMethod()]
        [Priority(2)]
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
                    SetValuesForReport("2016", "Quaterly", "ProjectedStageValue");
                    DataTable dt = ObjCommonFunctions.GetExcelData("ExcelConn", "[Actual Data$]").Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        VerifyWaterfall_INQActual(dt, ActualList, "Quaterly", QuaterlyActualList);
                        VerifyWaterfall_INQGoal(GoalList, "Quaterly", QuaterlyGoalList);
                        VerifyWaterfall_INQPerformance(ActualList, PerformanceList, "Quaterly", QuaterlyPerformanceList);
                        VerifyWaterfall_INQProjected(ProjectedList, GoalList, ActualList, "Quaterly", QuaterlyProjectedList);
                        VerifyWaterfall_INQTotal(ActualList, TotalActualList, "Quaterly", QuaterlyTotalActualList);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [TestMethod()]
        [Priority(1)]
        public void MonthlyMQLWaterfallReportTest()
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
                    SetValuesForReport("2016", "Monthly", "MQL");
                    DataTable dt = ObjCommonFunctions.GetExcelData("ExcelConn", "[Actual Data$]").Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        VerifyWaterfall_MQLActual(dt, ActualList, "Monthly");
                        //VerifyWaterfall_MQLGoal(GoalList, "Monthly");
                        //VerifyWaterfall_MQLPerformance(ActualList, PerformanceList, "Monthly");
                        //VerifyWaterfall_MQLProjected(ProjectedList, GoalList, ActualList, "Monthly");
                        //VerifyWaterfall_MQLTotal(ActualList, TotalActualList, "Monthly");
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [TestMethod()]
        [Priority(2)]
        public void QuaterlyMQLWaterfallReportTest()
        {
            try
            {
                //Call common function for login
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Assert.AreEqual("Index", IsLogin.RouteValues["Action"]);
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value of Action : " + IsLogin.RouteValues["Action"]);
                    SetValuesForReport("2016", "Quaterly", "MQL");
                    DataTable dt = ObjCommonFunctions.GetExcelData("ExcelConn", "[Actual Data$]").Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        VerifyWaterfall_INQActual(dt, ActualList, "Quaterly", QuaterlyActualList);
                        VerifyWaterfall_INQGoal(GoalList, "Quaterly", QuaterlyGoalList);
                        VerifyWaterfall_INQPerformance(ActualList, PerformanceList, "Quaterly", QuaterlyPerformanceList);
                        VerifyWaterfall_INQProjected(ProjectedList, GoalList, ActualList, "Quaterly", QuaterlyProjectedList);
                        VerifyWaterfall_INQTotal(ActualList, TotalActualList, "Quaterly", QuaterlyTotalActualList);
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

            var result1 = objReportController.GetTopConversionToPlanByCustomFilter("Campaign", "", "", "2016", isQuarterly, Stage) as PartialViewResult;
            objConversionToPlanModel = (ConversionToPlanModel)(result1.ViewData.Model);
            objConversionDataTable = objConversionToPlanModel.ConversionToPlanDataTableModel;
            SubDataTableModel = objConversionDataTable.SubDataModel;

            objProjected_Goal = objReportModel.RevenueHeaderModel;
            objCardSection = objReportModel.CardSectionModel;
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

        #region  Monthly Report Calculation

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
                        Assert.AreEqual(ActualList[i - 1], dt.Rows[0].ItemArray[i].ToString());
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual " + ActualList[i - 1].ToString() + " is not matching.");
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
                            Console.WriteLine("ReportController - GetRevenueData \n The assert value of quater actual " + QuaterlyActualList[i % 4].ToString() + " is not matching.");
                            QuaActual = 0;
                        }
                    }
                }
            }
        }

        public void VerifyWaterfall_INQGoal(List<double> GoalList, string IsQuaterly, List<double> QuaterlyGoalList = null)
        {
            if (IsQuaterly == "Monthly")
            {
                DataTable dtTactic = ObjCommonFunctions.GetExcelData("ExcelConn", "[Tactic$]").Tables[0];
                DataRow drTactic = dtTactic.Rows[0];

                NewStartDate = Convert.ToDateTime(drTactic["RevenueStartDate"].ToString());
                NewEndDate = Convert.ToDateTime(drTactic["RevenueEndDate"].ToString());
                TacticStartDate = Convert.ToDateTime(drTactic["TacticStartDate"].ToString());
                TacticEndDate = Convert.ToDateTime(drTactic["TacticEndDate"].ToString());
                MonthDiff = Convert.ToInt32(drTactic["RevenueMonthDiff"].ToString());

                DataTable dtModel = ObjCommonFunctions.GetExcelData("ExcelConn", "[Model$]").Tables[0];
                DataRow drModel = dtModel.Rows[0];

                TacticProjectedCost = Convert.ToDecimal(drModel["TACTIC_PROJECTED_COST"].ToString());
                TacticINQ = Convert.ToDecimal(drModel["TACTIC_PROJECTED_STAGE"].ToString());
                TacticTQL = ObjPlanCommonFunctions.CalculationForTactic(drModel, TacticINQ, "TQL");

                GoalAmount = TacticINQ / MonthDiff;

                for (int i = 1; i <= GoalList.Count; i++)
                {
                    if (i >= NewStartDate.Month && i + 1 <= NewEndDate.Month)
                    {
                        Assert.AreEqual(Convert.ToDouble(GoalList[i - 1].ToString()), Math.Round(Convert.ToDouble(GoalAmount)));
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of goal " + GoalList[i - 1].ToString() + " is not matching.");
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
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(GoalList[i % 4].ToString()), 2), Math.Round(QuaGoal, 2));
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of quater actual cost " + GoalList[i % 4].ToString() + " is not matching.");
                        QuaGoal = 0;
                    }
                }
            }
        }

        public void VerifyWaterfall_INQPerformance(List<double> ActualList, List<string> PerformanceList, string IsQuaterly, List<string> QuaterlyPerformanceList = null)
        {
            if (IsQuaterly == "Monthly")
            {
                for (int i = 1; i <= ActualList.Count(); i++)
                {
                    if (TacticStartDate.Month <= i && TacticEndDate.Month >= i)
                    {
                        decimal calculatePer = ((Convert.ToDecimal(ActualList[i - 1].ToString()) - GoalAmount) / GoalAmount) * 100;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(PerformanceList[i - 1].ToString()), 2), Math.Round(calculatePer, 2));
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of performance  " + PerformanceList[i - 1].ToString() + " is not matching.");
                    }
                    else
                    {
                        Assert.AreEqual(Convert.ToDouble(PerformanceList[i - 1].ToString()), 0);
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of performance  " + PerformanceList[i - 1].ToString() + " is not matching.");
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
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of quater performance " + QuaterlyPerformanceList[i % 4].ToString() + " is not matching.");
                        QuaPerformance = 0;
                    }
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
                                double proCal = (GoalList[i] / MonthDiff) * currentMonthNo;
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), Convert.ToDouble(proCal));
                                Console.WriteLine("ReportController - GetRevenueData \n The assert value of projected  " + ProjectedList[i - 1].ToString() + " is not matching.");
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
                                Console.WriteLine("ReportController - GetRevenueData \n The assert value of projected  " + ProjectedList[i - 1].ToString() + " is not matching.");
                            }
                            else
                            {
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), 0);
                                Console.WriteLine("ReportController - GetRevenueData \n The assert value of projected  " + ProjectedList[i - 1].ToString() + " is not matching.");
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
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of quater projected revenue " + QuaterlyProjectedList[i % 4].ToString() + " is not matching.");
                        QuaProjected = 0;
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
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of ROI  " + TotalActualList[i - 1].ToString() + " is not matching.");
                    }
                    else
                    {
                        Assert.AreEqual(TotalActualList[i - 1].ToString(), "0");
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of ROI  " + TotalActualList[i - 1].ToString() + " is not matching.");
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
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of quater ROI " + QuaterlyTotalActualList[i % 4].ToString() + " is not matching.");
                        QuaTotal = 0;
                    }
                }
            }

        }

        #endregion

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
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(ActualList[i - 1]), 2), (Math.Round(Convert.ToDecimal(dt.Rows[1].ItemArray[i].ToString()))));
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual " + ActualList[i - 1].ToString() + " is not matching.");
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
                            Console.WriteLine("ReportController - GetRevenueData \n The assert value of quater actual " + QuaterlyActualList[i % 4].ToString() + " is not matching.");
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
                DataTable dtTactic = ObjCommonFunctions.GetExcelData("ExcelConn", "[Tactic$]").Tables[0];
                DataRow drTactic = dtTactic.Rows[0];

                NewStartDate = Convert.ToDateTime(drTactic["RevenueStartDate"].ToString());
                NewEndDate = Convert.ToDateTime(drTactic["RevenueEndDate"].ToString());
                TacticStartDate = Convert.ToDateTime(drTactic["TacticStartDate"].ToString());
                TacticEndDate = Convert.ToDateTime(drTactic["TacticEndDate"].ToString());
                MonthDiff = Convert.ToInt32(drTactic["RevenueMonthDiff"].ToString());

                DataTable dtModel = ObjCommonFunctions.GetExcelData("ExcelConn", "[Model$]").Tables[0];
                DataRow drModel = dtModel.Rows[0];

                TacticProjectedCost = Convert.ToDecimal(drModel["TACTIC_PROJECTED_COST"].ToString());
                TacticINQ = Convert.ToDecimal(drModel["TACTIC_PROJECTED_STAGE"].ToString());
                TacticTQL = ObjPlanCommonFunctions.CalculationForTactic(drModel, TacticINQ, "TQL");

                GoalAmount = TacticINQ / MonthDiff;

                for (int i = 1; i <= GoalList.Count; i++)
                {
                    if (i >= NewStartDate.Month && i + 1 <= NewEndDate.Month)
                    {
                        Assert.AreEqual(Convert.ToDouble(QuaterlyGoalList[i - 1].ToString()), Math.Round(Convert.ToDouble(GoalAmount)));
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of goal " + GoalList[i - 1].ToString() + " is not matching.");
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
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of quater actual cost " + QuaterlyGoalList[i % 4].ToString() + " is not matching.");
                        QuaGoal = 0;
                    }
                }
            }
        }

        public void VerifyWaterfall_MQLPerformance(List<double> ActualList, List<string> PerformanceList, string IsQuaterly, List<string> QuaterlyPerformanceList = null)
        {
            if (IsQuaterly == "Monthly")
            {
                DataTable dtTactic = ObjCommonFunctions.GetExcelData("ExcelConn", "[Tactic$]").Tables[0];
                DataRow drTactic = dtTactic.Rows[0];

                DateTime TQLStartDate = Convert.ToDateTime(drTactic["TQLStartDate"].ToString());
                DateTime TQLEndDate = Convert.ToDateTime(drTactic["TQLEndDate"].ToString());
                for (int i = 1; i <= ActualList.Count(); i++)
                {
                    if (TQLStartDate.Month <= i && TQLEndDate.Month >= i)
                    {
                        decimal calculatePer = ((Convert.ToDecimal(ActualList[i - 1].ToString()) - GoalAmount) / GoalAmount) * 100;
                        Assert.AreEqual(Math.Round(Convert.ToDecimal(PerformanceList[i - 1].ToString()), 2), Math.Round(calculatePer, 2));
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of performance  " + PerformanceList[i - 1].ToString() + " is not matching.");
                    }
                    else
                    {
                        Assert.AreEqual(Convert.ToDouble(PerformanceList[i - 1].ToString()), 0);
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of performance  " + PerformanceList[i - 1].ToString() + " is not matching.");
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
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of quater performance " + QuaterlyPerformanceList[i % 4].ToString() + " is not matching.");
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
                                Console.WriteLine("ReportController - GetRevenueData \n The assert value of projected  " + ProjectedList[i - 1].ToString() + " is not matching.");
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
                                Console.WriteLine("ReportController - GetRevenueData \n The assert value of projected  " + ProjectedList[i - 1].ToString() + " is not matching.");
                            }
                            else
                            {
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), 0);
                                Console.WriteLine("ReportController - GetRevenueData \n The assert value of projected  " + ProjectedList[i - 1].ToString() + " is not matching.");
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
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of quater projected revenue " + QuaterlyProjectedList[i % 4].ToString() + " is not matching.");
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
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of ROI  " + TotalActualList[i - 1].ToString() + " is not matching.");
                    }
                    else
                    {
                        Assert.AreEqual(TotalActualList[i - 1].ToString(), "0");
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of ROI  " + TotalActualList[i - 1].ToString() + " is not matching.");
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
                        Console.WriteLine("ReportController - GetRevenueData \n The assert value of quater ROI " + QuaterlyTotalActualList[i % 4].ToString() + " is not matching.");
                        QuaTotal = 0;
                    }
                }
            }

        }

        #endregion
        
        #endregion

    }
}

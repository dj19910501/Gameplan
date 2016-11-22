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
    public class CWWaterfallReport
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

        #region CW Report Calculation

        [TestMethod()]
        public void CWWaterfallReportTest()
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
            SetValuesForReport(currentYear, "Monthly", "CW");
            DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                VerifyWaterfall_CWActual(dt, ActualList, "Monthly");
                VerifyWaterfall_CWGoal(GoalList, "Monthly");
                VerifyWaterfall_CWPerformance(ActualList, PerformanceList, "Monthly");
                VerifyWaterfall_CWProjected(ProjectedList, GoalList, ActualList, "Monthly");
                VerifyWaterfall_CWTotal(ActualList, TotalActualList, "Monthly");
                CheckValueForCard("CW");
                VerifyWaterfall_CWCardSection(ActualList);
            }
        }

        public void QuaterlyCWWaterfallReportTest()
        {
            SetValuesForReport(currentYear, "Quaterly", "CW");
            DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Actual Data$]").Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                VerifyWaterfall_CWActual(dt, ActualList, "Quaterly", QuaterlyActualList);
                VerifyWaterfall_CWGoal(GoalList, "Quaterly", QuaterlyGoalList);
                VerifyWaterfall_CWPerformance(ActualList, PerformanceList, "Quaterly", QuaterlyPerformanceList);
                VerifyWaterfall_CWProjected(ProjectedList, GoalList, ActualList, "Quaterly", QuaterlyProjectedList);
                VerifyWaterfall_CWTotal(ActualList, TotalActualList, "Quaterly", QuaterlyTotalActualList);
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of actual CW is " + ActualList[i - 1].ToString() + ".");
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
                            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of quaterly actual CW is " + QuaterlyActualList[i % 4].ToString() + ".");
                            QuaActual = 0;
                        }
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of CW goal is " + GoalList[i - 1].ToString() + ".");
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of quaterly CW goal is " + QuaterlyGoalList[i % 4].ToString() + ".");
                        QuaGoal = 0;
                    }
                }
            }
        }

        public void VerifyWaterfall_CWPerformance(List<double> ActualList, List<string> PerformanceList, string IsQuaterly, List<string> QuaterlyPerformanceList = null)
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of CW performance is " + PerformanceList[i - 1].ToString() + ".");
                    }
                    else
                    {
                        Assert.AreEqual(Convert.ToDouble(PerformanceList[i - 1].ToString()), 0);
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of CW performance is " + PerformanceList[i - 1].ToString() + ".");
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of quaterly CW performance is " + QuaterlyPerformanceList[i % 4].ToString() + ".");
                        QuaPerformance = 0;
                    }
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
                                double proCal = (GoalList[i] / MonthDiff) * currentMonthNo;
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), Convert.ToDouble(proCal));
                                Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of projected CW is " + ProjectedList[i - 1].ToString() + ".");
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
                                Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of projected CW is " + ProjectedList[i - 1].ToString() + ".");
                            }
                            else
                            {
                                Assert.AreEqual(Convert.ToDouble(ProjectedList[i - 1].ToString()), 0);
                                Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of projected CW is " + ProjectedList[i - 1].ToString() + ".");
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of quaterly projected  " + QuaterlyProjectedList[i % 4].ToString() + " .");
                        QuaProjected = 0;
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of total CW  " + TotalActualList[i - 1].ToString() + ".");
                    }
                    else
                    {
                        Assert.AreEqual(TotalActualList[i - 1].ToString(), "0");
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of total CW is " + TotalActualList[i - 1].ToString() + ".");
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
                        Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of quaterly total CW is " + QuaterlyTotalActualList[i % 4].ToString() + ".");
                        QuaTotal = 0;
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
            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of CW actual  " + Math.Round(CardCWActual, 2).ToString() + " .");

            Assert.AreEqual(Math.Round(TacticCW, 2), Math.Round(CardCWGoal, 2));
            Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of CW actual goal " + Math.Round(CardCWGoal, 2).ToString() + " .");

            //decimal cardPercentage = ((CardINQActual - CardMQLActual) / CardMQLActual) * 100;
            //Assert.AreEqual(Math.Round(cardPercentage, 2), Math.Round(CardCWPercentage, 2));
            //Console.WriteLine("ReportController - GetTopConversionToPlanByCustomFilter \n The assert value of CW actual percentage " + Math.Round(CardINQActual, 2).ToString() + " .");


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

    }
}

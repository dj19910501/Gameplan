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
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RevenuePlanner.Test.QA.ReportsIntegrationTest
{

    [TestClass]

    public class RevenueReportTestCase
    {
        #region Variable Declaration
        Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();
        PlanCommonFunctions ObjPlanCommonFunctions = new PlanCommonFunctions();
        DateTime NewStartDate; DateTime NewEndDate; DateTime TacticEndDate; DateTime TacticStartDate; int MonthDiff = 0;
        int currentMonth = DateTime.Now.Month;
        decimal TacticRevenueAmount = 0; decimal TacticINQ = 0; decimal TacticProjectedCost = 0;
        decimal PlanBudget = 0; decimal TacticTQL = 0;
        decimal GoalAmount = 0; decimal actualProjected = 0;

        ReportController objReportController = new ReportController();
        ReportModel objReportModel = new ReportModel();
        RevenueDataTable objReportDataTable = new RevenueDataTable();
        RevenueToPlanModel objRevenueToPlanModel = new RevenueToPlanModel();
        RevenueSubDataTableModel subModelList = new RevenueSubDataTableModel();
        Projected_Goal objProjected_Goal = new Projected_Goal();
        CardSectionModel objCardSection = new CardSectionModel();

        #endregion

        [TestMethod()]
        //[Priority(1)]
        public void MonthlyRevenueReportTest()
        {
            try
            {
                //Call common function for login
                var result = ObjCommonFunctions.CheckLogin();

                if (result != null)
                {
                    Assert.AreEqual("Index", result.RouteValues["Action"]);
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value of Action : " + result.RouteValues["Action"]);

                    List<int> PlanIds = new List<int>();
                    List<int> ReportOwnerIds = new List<int>();
                    List<int> ReportTacticTypeIds = new List<int>();

                    int PlanId = Convert.ToInt32(ConfigurationManager.AppSettings["PlanId"]);
                    PlanIds.Add(PlanId);
                    Sessions.ReportPlanIds = PlanIds;

                    int OwnerId = DataHelper.GetPlanOwnerId(PlanId);
                    ReportOwnerIds.Add(OwnerId);
                    Sessions.ReportOwnerIds = ReportOwnerIds;

                    int ModelId = Convert.ToInt32(ConfigurationManager.AppSettings["ModelId"]);
                    ReportTacticTypeIds = QA_DataHelper.GetTacticTypeIds(ModelId);
                    Sessions.ReportTacticTypeIds = ReportTacticTypeIds;

                    var result1 = objReportController.GetRevenueData("2016", "Monthly") as PartialViewResult;

                    objReportModel = (ReportModel)(result1.ViewData.Model);
                    objRevenueToPlanModel = objReportModel.RevenueToPlanModel;
                    objReportDataTable = objRevenueToPlanModel.RevenueToPlanDataModel;
                    subModelList = objReportDataTable.SubDataModel;

                    objProjected_Goal = objReportModel.RevenueHeaderModel;
                    objCardSection = objReportModel.CardSectionModel;

                    List<double> ActualList = objReportDataTable.ActualList;
                    List<double> ProjectedList = objReportDataTable.ProjectedList;
                    List<double> GoalList = objReportDataTable.GoalList;
                    List<string> PerformaceList = subModelList.PerformanceList;
                    List<string> ActualCostList = subModelList.CostList;
                    List<string> ROIList = subModelList.ROIList;
                    List<string> TotalRevenueList = subModelList.RevenueList;

                    DataTable dt = ObjCommonFunctions.GetExcelData("ExcelConn", "[Actual Data$]").Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        VerifyReport_Actual(dt, ActualList);
                        VerifyReport_ActualCost(dt, ActualCostList);
                        VerifyReport_Goal(GoalList);
                        VerifyReport_ProjectedRevenue(ProjectedList, GoalList, ActualList);
                        VerifyReport_Performnace(ActualList, PerformaceList);
                        VerifyReport_ROI(ActualList, ActualCostList, ROIList);
                        VerifyReport_TotalRevenue(ActualList, TotalRevenueList);
                        VerifyHeaderValue(objProjected_Goal, ActualList, GoalList, ProjectedList);
                        VerifyCardSectionValue(objCardSection, ActualCostList);
                    }

                    #region Comment
                    //var count = 0;
                    //foreach (DataRow dr in dt.Rows)
                    //{
                    //    for (int i = 1; i <= dt.Columns.Count; i++)
                    //    {
                    //        if (count == 0)
                    //        {
                    //            Assert.AreEqual(ActualList[i - 1].ToString(), dr.ItemArray[i].ToString());
                    //            Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual  " + ActualList[i - 1].ToString() + "is not matching.");

                    //        }
                    //        else if (count == 1)
                    //        {
                    //            Assert.AreEqual(ProjectedList[i - 1].ToString(), dr.ItemArray[i].ToString());
                    //            Console.WriteLine("ReportController - GetRevenueData \n The assert value of projected  " + ProjectedList[i - 1].ToString() + "is not matching.");
                    //        }
                    //        else if (count == 2)
                    //        {
                    //            Assert.AreEqual(GoalList[i - 1].ToString(), dr.ItemArray[i].ToString());
                    //            Console.WriteLine("ReportController - GetRevenueData \n The assert value of goal  " + GoalList[i - 1].ToString() + "is not matching.");
                    //        }
                    //        else if (count == 3)
                    //        {
                    //            Assert.AreEqual(PerformaceList[i - 1].ToString(), dr.ItemArray[i].ToString());
                    //            Console.WriteLine("ReportController - GetRevenueData \n The assert value of performnace  " + PerformaceList[i - 1].ToString() + "is not matching.");
                    //        }
                    //        else if (count == 5)
                    //        {
                    //            Assert.AreEqual(ActualCostList[i - 1].ToString(), dr.ItemArray[i].ToString());
                    //            Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual cost " + ActualCostList[i - 1].ToString() + "is not matching.");
                    //        }
                    //        else if (count == 6)
                    //        {
                    //            Assert.AreEqual(ROIList[i - 1].ToString(), dr.ItemArray[i].ToString());
                    //            Console.WriteLine("ReportController - GetRevenueData \n The assert value of ROI " + ROIList[i - 1].ToString() + "is not matching.");
                    //        }
                    //        else if (count == 7)
                    //        {
                    //            Assert.AreEqual(TotalRevenueList[i - 1].ToString(), dr.ItemArray[i].ToString());
                    //            Console.WriteLine("ReportController - GetRevenueData \n The assert value of total revenue  " + TotalRevenueList[i - 1].ToString() + "is not matching.");
                    //        }
                    //    }
                    //}
                    #endregion
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #region Report Calculation Methods
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
                    Assert.AreEqual(ActualList[i - 1].ToString(), dt.Rows[3].ItemArray[i].ToString());
                    Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual " + ActualList[i - 1].ToString() + " is not matching.");
                }
            }
        }

        public void VerifyReport_ActualCost(DataTable dt, List<string> ActualCostList)
        {
            if (dt.Rows[4] != null)
            {
                for (int i = 1; i <= dt.Columns.Count - 1; i++)
                {
                    var currentMonth = DateTime.Now.Month;
                    if (i > currentMonth)
                    {
                        dt.Rows[4][i] = 0;
                    }
                    Assert.AreEqual(ActualCostList[i - 1].ToString(), dt.Rows[4].ItemArray[i].ToString());
                    Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual cost " + ActualCostList[i - 1].ToString() + " is not matching.");
                }
            }
        }

        public void VerifyReport_Goal(List<double> GoalList)
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
            TacticRevenueAmount = ObjPlanCommonFunctions.CalculationForTactic(drModel, TacticINQ, "Revenue");

            GoalAmount = TacticRevenueAmount / MonthDiff;

            for (int i = 0; i < GoalList.Count - 1; i++)
            {
                if (i + 1 >= NewStartDate.Month && i + 1 <= NewEndDate.Month)
                {
                    Assert.AreEqual(Convert.ToDouble(GoalList[i + 1].ToString()), Math.Round(Convert.ToDouble(GoalAmount)));
                    Console.WriteLine("ReportController - GetRevenueData \n The assert value of goal " + GoalList[i + 1].ToString() + " is not matching.");
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
                        int currentMonthNo = DateTime.Now.Month - NewStartDate.Month + 1;
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

        public void VerifyReport_Performnace(List<double> ActualList, List<string> PerformaceList)
        {
            for (int i = 1; i <= ActualList.Count(); i++)
            {
                if (NewStartDate.Month <= i && NewEndDate.Month >= i)
                {
                    decimal calculatePer = ((Convert.ToDecimal(ActualList[i - 1].ToString()) - GoalAmount) / GoalAmount) * 100;
                    Assert.AreEqual(PerformaceList[i - 1].ToString(), Math.Round(calculatePer).ToString());
                    Console.WriteLine("ReportController - GetRevenueData \n The assert value of performance  " + PerformaceList[i - 1].ToString() + " is not matching.");
                }
                else
                {
                    Assert.AreEqual(Convert.ToDouble(PerformaceList[i - 1].ToString()), 0);
                    Console.WriteLine("ReportController - GetRevenueData \n The assert value of performance  " + PerformaceList[i - 1].ToString() + " is not matching.");
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
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(ROIList[i - 1].ToString()), 2), Math.Round(calculatePer, 2));
                    Console.WriteLine("ReportController - GetRevenueData \n The assert value of ROI  " + ROIList[i - 1].ToString() + " is not matching.");
                }
                else
                {
                    Assert.AreEqual(ROIList[i - 1].ToString(), "0");
                    Console.WriteLine("ReportController - GetRevenueData \n The assert value of ROI  " + ROIList[i - 1].ToString() + " is not matching.");
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
                    Assert.AreEqual(TotalRevenueList[i - 1].ToString(), Math.Round(Total, 2).ToString());
                    Console.WriteLine("ReportController - GetRevenueData \n The assert value of ROI  " + TotalRevenueList[i - 1].ToString() + " is not matching.");
                }
                else
                {
                    Assert.AreEqual(TotalRevenueList[i - 1].ToString(), "0");
                    Console.WriteLine("ReportController - GetRevenueData \n The assert value of ROI  " + TotalRevenueList[i - 1].ToString() + " is not matching.");
                }
            }
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
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual projected  " + Math.Round(actualProjected, 2).ToString() + " is not matching.");

            // Get actual perecentage value
            for (int i = 1; i <= GoalList.Count(); i++)
            {
                if (currentMonth >= i)
                    GoalUpToCurrentMonth = GoalUpToCurrentMonth + Convert.ToDecimal(GoalList[i - 1].ToString());
            }
            actualPerecentage = ((actualProjected - GoalUpToCurrentMonth) / GoalUpToCurrentMonth) * 100;
            Assert.AreEqual(Math.Round(actualPerecentage, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.ActualPercentage), 2));
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual perecentage  " + Math.Round(actualPerecentage, 2).ToString() + " is not matching.");

            #endregion

            #region Projected
            // Get projected value
            for (int i = 0; i < ProjectedList.Count(); i++)
            {
                SumOfProjected = SumOfProjected + Convert.ToDecimal(ProjectedList[i].ToString());
            }
            TotalProjected = SumOfProjected + actualProjected;
            Assert.AreEqual(Math.Round(TotalProjected, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.Projected), 2));
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual perecentage  " + Math.Round(TotalProjected, 2).ToString() + " is not matching.");

            //Calculation for projected percentage
            ProjectedPercentage = ((TotalProjected - TacticRevenueAmount) / TacticRevenueAmount) * 100;
            Assert.AreEqual(Math.Round(ProjectedPercentage, 2), Math.Round(Convert.ToDecimal(objProjected_Goal.ProjectedPercentage), 2));
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual perecentage  " + Math.Round(ProjectedPercentage, 2).ToString() + " is not matching.");

            #endregion
        }

        #endregion

        #region Card section calculation
        public void VerifyCardSectionValue(CardSectionModel objCardSection, List<string> ActualCostList)
        {
            var Card = objCardSection.CardSectionListModel[0];

            #region Actual
            Assert.AreEqual(Math.Round(actualProjected, 2), Math.Round(Convert.ToDecimal(Card.RevenueCardValues.Actual_Projected), 2));
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual projected  " + Math.Round(Card.RevenueCardValues.Actual_Projected, 2).ToString() + " is not matching.");

            Assert.AreEqual(Math.Round(TacticRevenueAmount, 2), Math.Round(Convert.ToDecimal(Card.RevenueCardValues.Goal), 2));
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual goal  " + Math.Round(Card.RevenueCardValues.Goal, 2).ToString() + " is not matching.");

            var actualPercentage = ((actualProjected - TacticRevenueAmount) / TacticRevenueAmount) * 100;
            Assert.AreEqual(Math.Round(actualPercentage, 2), Math.Round(Convert.ToDecimal(Card.RevenueCardValues.Percentage), 2));
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of actual perecentage  " + Math.Round(Card.RevenueCardValues.Percentage, 2).ToString() + " is not matching.");

            #endregion

            #region Cost
            decimal tacticCost = 0;
            foreach (var actualCost in ActualCostList)
            {
                tacticCost = tacticCost + Convert.ToDecimal(actualCost);
            }
            Assert.AreEqual(Math.Round(tacticCost, 2), Math.Round(Convert.ToDecimal(Card.CostCardValues.Actual_Projected), 2));
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of tactic cost " + Math.Round(Card.CostCardValues.Actual_Projected, 2).ToString() + " is not matching.");

            Assert.AreEqual(Math.Round(TacticProjectedCost, 2), Math.Round(Convert.ToDecimal(Card.CostCardValues.Goal), 2));
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of tactic projected " + Math.Round(Card.CostCardValues.Goal, 2).ToString() + " is not matching.");

            decimal costPercentage = ((tacticCost - TacticProjectedCost) / TacticProjectedCost) * 100;
            Assert.AreEqual(Math.Round(costPercentage, 2), Math.Round(Convert.ToDecimal(Card.CostCardValues.Percentage), 2));
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of tactic perecentage " + Math.Round(Card.CostCardValues.Percentage, 2).ToString() + " is not matching.");
            #endregion

            #region ROI
            var ROIActual_Projected = ((actualProjected - tacticCost) / tacticCost) * 100;
            Assert.AreEqual(Math.Round(ROIActual_Projected, 2), Math.Round(Convert.ToDecimal(Card.ROICardValues.Actual_Projected), 2));
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of ROI projected  " + Math.Round(Card.ROICardValues.Actual_Projected, 2).ToString() + " is not matching.");

            var ROIActual_Goal = ((TacticRevenueAmount - TacticProjectedCost) / TacticProjectedCost) * 100;
            Assert.AreEqual(Math.Round(ROIActual_Goal, 2), Math.Round(Convert.ToDecimal(Card.ROICardValues.Goal), 2));
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of ROI goal  " + Math.Round(Card.ROICardValues.Goal, 2).ToString() + " is not matching.");

            var ROIActual_Percentage = ((ROIActual_Projected - ROIActual_Goal) / ROIActual_Goal) * 100;
            Assert.AreEqual(Math.Round(ROIActual_Percentage, 2), Math.Round(Convert.ToDecimal(Card.ROICardValues.Percentage), 2));
            Console.WriteLine("ReportController - GetRevenueData \n The assert value of ROI perecentage  " + Math.Round(Card.RevenueCardValues.Percentage, 2).ToString() + " is not matching.");
            #endregion

        }

        #endregion
    
    }
}


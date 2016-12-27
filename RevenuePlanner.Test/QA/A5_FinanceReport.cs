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
    public class A5_FinanceReport
    {
        #region Variable Declaration

        Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();
        PlanCommonFunctions ObjPlanCommonFunctions = new PlanCommonFunctions();

        static string currentYear = DateTime.Now.Year.ToString();

        #endregion

        #region  Monthly Finance Report

        [TestMethod]
        public void A1_MonthlyFinanceReport()
        {
            try
            {
                //Call common function for login
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Console.WriteLine(" Testing LoginController - Index method");
                    Console.WriteLine(" The assert value of action is " + IsLogin.RouteValues["Action"] + ". (The expected value is Index.)");
                    Console.WriteLine("\n ----------------------------------------------------------------------");
                    Console.WriteLine("\n Testing  ReportController - GetReportBudgetData Method");
                    ReportController objReportController = new ReportController();
                    ReportModel objReportModel = new ReportModel();
                    BudgetDHTMLXGridDataModel objBudgetDHTMLXGridDataModel = new BudgetDHTMLXGridDataModel();
                    BudgetDHTMLXGridModel objBudgetDHTMLXGridModel = new BudgetDHTMLXGridModel();
                    ObjPlanCommonFunctions.SetSessionData();
                    DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[MonthlyFinance$]").Tables[0];
                    var result = objReportController.GetReportBudgetData(currentYear, "months", "Plan", "") as PartialViewResult;

                    string MainTotalAllocated = result.ViewBag.MainTotalAllocated.ToString();
                    string MainTotalActual = result.ViewBag.MainTotalActual.ToString();
                    BudgetMonth PercAllocated = result.ViewBag.PercAllocated;

                    objBudgetDHTMLXGridModel = (BudgetDHTMLXGridModel)result.Model;
                    List<Budgetdataobj> PlanData = objBudgetDHTMLXGridModel.Grid.rows[1].data;
                    List<string> subHeader = objBudgetDHTMLXGridModel.AttachHeader;
                    string mainHeaderString = objBudgetDHTMLXGridModel.SetHeader;
                    string[] mainHeader = mainHeaderString.Split(',').ToArray();

                    VerifyHeaderValue(dt, MainTotalAllocated, MainTotalActual);
                    Console.WriteLine("\n -------------- Summary - Table Number Validation --------------");
                    VerifyMonthlyData(dt, PlanData, mainHeader, subHeader, PercAllocated);

                    if (objBudgetDHTMLXGridModel.Grid.rows[1].rows != null && objBudgetDHTMLXGridModel.Grid.rows[1].rows.Count > 0)
                    {
                        var Campaign = objBudgetDHTMLXGridModel.Grid.rows[1].rows[0];
                        List<Budgetdataobj> CampaignData = Campaign.data;
                        VerifyMonthlyData(dt, CampaignData, mainHeader, subHeader);
                        if (Campaign.rows != null && Campaign.rows.Count > 0)
                        {
                            var program = Campaign.rows[0];
                            List<Budgetdataobj> programData = program.data;
                            VerifyMonthlyData(dt, programData, mainHeader, subHeader);
                            if (program.rows != null && program.rows.Count > 0)
                            {
                                var tactic = program.rows[0];
                                List<Budgetdataobj> tacticData = tactic.data;
                                VerifyMonthlyData(dt, tacticData, mainHeader, subHeader);
                                if (tactic.rows != null && tactic.rows.Count > 0)
                                {
                                    var lineItem = tactic.rows[0];
                                    List<Budgetdataobj> lineItemData = lineItem.data;
                                    VerifyMonthlyData(dt, lineItemData, mainHeader, subHeader);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public void VerifyMonthlyData(DataTable dt, List<Budgetdataobj> data, string[] mainHeader, List<string> subHeader, BudgetMonth PercAllocated = null)
        {
            DataRow dr = dt.NewRow();
            if (data.Count > 0 && data != null)
            {
                switch (data[0].value)
                {
                    case "Plan":
                        dr = dt.Rows[0];
                        break;
                    case "Campaign":
                        dr = dt.Rows[1];
                        break;
                    case "Program":
                        dr = dt.Rows[2];
                        break;
                    case "Tactic":
                        dr = dt.Rows[3];
                        break;
                    case "LineItem":
                        dr = dt.Rows[4];
                        break;
                }
                string mHeader = "";
                // int j = 0;
                for (int i = 1; i <= data.Count() - 1; i++)
                {
                    string htmlString = data[i].value;
                    string budgetValue = GetBudgetData(htmlString);

                    if (dr != null)
                    {
                        if (i == 1)
                        {
                            Assert.AreEqual(dr[i - 1].ToString().Trim(), budgetValue.Trim());
                            Console.WriteLine("\n The assert value of " + mainHeader[i - 1] + " is " + dr[i - 1].ToString() + ".");
                        }
                        else
                        {
                            if (budgetValue.Contains("---"))
                            {
                                budgetValue = "0";
                            }

                            if (i <= 40)
                            {
                                if (!mainHeader[i].Contains('#'))
                                {
                                    mHeader = mainHeader[i];
                                }
                            }

                            Assert.AreEqual(Math.Round(Convert.ToDecimal(dr[i - 1].ToString()), 2), Math.Round(Convert.ToDecimal(budgetValue), 2));
                            Console.WriteLine("\n The assert value of " + subHeader[i] + " of " + dr[0].ToString() + " in " + mHeader + " is " + budgetValue + ". (The expected value is " + Math.Round(Convert.ToDecimal(dr[i - 1].ToString()), 2).ToString() + ".)");
                            if (PercAllocated != null && subHeader[i].ToLower() == "budget")
                            {
                                decimal percentage = 0;
                                if (Math.Round(Convert.ToDecimal(dr[i - 1].ToString())) == 0)
                                {
                                    percentage = 101;
                                }
                                else
                                {
                                    percentage = (Convert.ToDecimal(dr[i - 3].ToString()) / Convert.ToDecimal(dr[i - 1].ToString())) * 100;
                                }
                                VerifyMonthlyPercentage(percentage, PercAllocated, mHeader);
                            }
                        }
                    }
                }
            }
        }

        public void VerifyHeaderValue(DataTable dt, string MainTotalAllocated, string MainTotalActual)
        {
            Console.WriteLine("\n -------------- Header Number Validation --------------");
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                Assert.AreEqual(dr["Total Budget"].ToString(), MainTotalAllocated);
                Console.WriteLine("\n The assert value of total allocated amount is " + MainTotalAllocated + ". (The expected value is " + dr["Total Budget"].ToString() + ".)");
                Assert.AreEqual(dr["Total Actual"].ToString(), MainTotalActual);
                Console.WriteLine("\n The assert value of total spent amount is " + MainTotalActual + ". (The expected value is " + dr["Total Actual"].ToString() + ".)");
            }

        }

        public string GetBudgetData(string htmlString)
        {
            string budgetValue = "";
            if (htmlString.Contains('<') && htmlString.Contains('>'))
            {
                int firstDivEndIndex = htmlString.IndexOf('>');
                int spanStartIndex = htmlString.IndexOf('<', firstDivEndIndex);
                int length = spanStartIndex - firstDivEndIndex - 1;
                budgetValue = htmlString.Substring(firstDivEndIndex + 1, length);
            }
            else
            {
                budgetValue = htmlString;
            }
            return budgetValue;
        }

        public void VerifyMonthlyPercentage(decimal percentage, BudgetMonth budgetMonth, string month)
        {
            decimal data = 0;
            switch (month)
            {
                case "JAN":
                    data = Convert.ToDecimal(budgetMonth.BudgetY1);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + month + " is " + Math.Round(Convert.ToDecimal(data), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "FEB":
                    data = Convert.ToDecimal(budgetMonth.BudgetY2);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + month + " is " + Math.Round(Convert.ToDecimal(data), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "MAR":
                    data = Convert.ToDecimal(budgetMonth.BudgetY3);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + month + " is " + Math.Round(Convert.ToDecimal(data), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "APR":
                    data = Convert.ToDecimal(budgetMonth.BudgetY4);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + month + " is " + Math.Round(Convert.ToDecimal(data), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "MAY":
                    data = Convert.ToDecimal(budgetMonth.BudgetY5);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + month + " is " + Math.Round(Convert.ToDecimal(data), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "JUN":
                    data = Convert.ToDecimal(budgetMonth.BudgetY6);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + month + " is " + Math.Round(Convert.ToDecimal(data), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "JUL":
                    data = Convert.ToDecimal(budgetMonth.BudgetY7);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + month + " is " + Math.Round(Convert.ToDecimal(data), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "AUG":
                    data = Convert.ToDecimal(budgetMonth.BudgetY8);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + month + " is " + Math.Round(Convert.ToDecimal(data), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "SEP":
                    data = Convert.ToDecimal(budgetMonth.BudgetY9);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + month + " is " + Math.Round(Convert.ToDecimal(data), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "OCT":
                    data = Convert.ToDecimal(budgetMonth.BudgetY10);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + month + " is " + Math.Round(Convert.ToDecimal(data), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "NOV":
                    data = Convert.ToDecimal(budgetMonth.BudgetY11);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + month + " is " + Math.Round(Convert.ToDecimal(data), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "DEC":
                    data = Convert.ToDecimal(budgetMonth.BudgetY12);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + month + " is " + Math.Round(Convert.ToDecimal(data), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
            }


        }

        #endregion

        #region  Quarterly Finance Report

        [TestMethod]
        public void A2_QuarterlyFinanceReport()
        {
            try
            {
                //Call common function for login
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Console.WriteLine(" Testing LoginController - Index method");
                    Console.WriteLine(" The assert value of action is " + IsLogin.RouteValues["Action"] + ". (The expected value is Index.)");
                    Console.WriteLine("\n ----------------------------------------------------------------------");
                    Console.WriteLine("\n Testing  ReportController - GetReportBudgetData Method");
                    ReportController objReportController = new ReportController();
                    ReportModel objReportModel = new ReportModel();
                    BudgetDHTMLXGridDataModel objBudgetDHTMLXGridDataModel = new BudgetDHTMLXGridDataModel();
                    BudgetDHTMLXGridModel objBudgetDHTMLXGridModel = new BudgetDHTMLXGridModel();
                    ObjPlanCommonFunctions.SetSessionData();
                    DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[QuarterlyFinance$]").Tables[0];
                    var result = objReportController.GetReportBudgetData(currentYear, "quarters", "Plan", "") as PartialViewResult;

                    string MainTotalAllocated = result.ViewBag.MainTotalAllocated.ToString();
                    string MainTotalActual = result.ViewBag.MainTotalActual.ToString();
                    BudgetMonth PercAllocated = result.ViewBag.PercAllocated;

                    objBudgetDHTMLXGridModel = (BudgetDHTMLXGridModel)result.Model;
                    List<Budgetdataobj> PlanData = objBudgetDHTMLXGridModel.Grid.rows[1].data;
                    List<string> subHeader = objBudgetDHTMLXGridModel.AttachHeader;
                    string mainHeaderString = objBudgetDHTMLXGridModel.SetHeader;
                    string[] mainHeader = mainHeaderString.Split(',').ToArray();

                    VerifyHeaderValue(dt, MainTotalAllocated, MainTotalActual);
                    Console.WriteLine("\n -------------- Summary - Table Number Validation --------------");
                    VerifyQuaterlyData(dt, PlanData, mainHeader, subHeader, PercAllocated);
                    if (objBudgetDHTMLXGridModel.Grid.rows[1].rows != null && objBudgetDHTMLXGridModel.Grid.rows[1].rows.Count > 0)
                    {
                        var Campaign = objBudgetDHTMLXGridModel.Grid.rows[1].rows[0];
                        List<Budgetdataobj> CampaignData = Campaign.data;
                        VerifyQuaterlyData(dt, CampaignData, mainHeader, subHeader);
                        if (Campaign.rows != null && Campaign.rows.Count > 0)
                        {
                            var program = Campaign.rows[0];
                            List<Budgetdataobj> programData = program.data;
                            VerifyQuaterlyData(dt, programData, mainHeader, subHeader);
                            if (program.rows != null && program.rows.Count > 0)
                            {
                                var tactic = program.rows[0];
                                List<Budgetdataobj> tacticData = tactic.data;
                                VerifyQuaterlyData(dt, tacticData, mainHeader, subHeader);
                                if (tactic.rows != null && tactic.rows.Count > 0)
                                {
                                    var lineItem = tactic.rows[0];
                                    List<Budgetdataobj> lineItemData = lineItem.data;
                                    VerifyQuaterlyData(dt, lineItemData, mainHeader, subHeader);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public void VerifyQuaterlyData(DataTable dt, List<Budgetdataobj> data, string[] mainHeader, List<string> subHeader, BudgetMonth PercAllocated = null)
        {
            DataRow dr = dt.NewRow();
            if (data.Count > 0 && data != null)
            {
                switch (data[0].value)
                {
                    case "Plan":
                        dr = dt.Rows[0];
                        break;
                    case "Campaign":
                        dr = dt.Rows[1];
                        break;
                    case "Program":
                        dr = dt.Rows[2];
                        break;
                    case "Tactic":
                        dr = dt.Rows[3];
                        break;
                    case "LineItem":
                        dr = dt.Rows[4];
                        break;
                }
                string mHeader = "";
                int j = 0;
                for (int i = 1; i <= data.Count() - 1; i++)
                {
                    string htmlString = data[i].value;
                    string budgetValue = GetBudgetData(htmlString);


                    if (dr != null)
                    {

                        if (i == 1)
                        {
                            Assert.AreEqual(dr[i - 1].ToString().Trim(), budgetValue.Trim());
                            Console.WriteLine("\n The assert value of " + mainHeader[i - 1] + " is " + budgetValue.Trim() + ". (The expected value is " + dr[i - 1].ToString().Trim() + ".)");
                        }
                        else
                        {
                            if (budgetValue.Contains("---"))
                            {
                                budgetValue = "0";
                            }
                            if (!mainHeader[i].Contains('#'))
                            {
                                mHeader = mainHeader[i];
                            }
                            Assert.AreEqual(Math.Round(Convert.ToDecimal(dr[i - 1].ToString()), 2), Math.Round(Convert.ToDecimal(budgetValue), 2));
                            Console.WriteLine("\n The assert value of " + subHeader[i] + " of " + dr[0].ToString() + " in " + mHeader + " is " + budgetValue + ". (The expected value is " + Math.Round(Convert.ToDecimal(dr[i - 1].ToString()), 2).ToString() + ".)");
                            if (PercAllocated != null && subHeader[i].ToLower() == "budget")
                            {
                                decimal percentage = 0;
                                if (Math.Round(Convert.ToDecimal(dr[i - 1].ToString())) == 0)
                                {
                                    percentage = 101;
                                }
                                else
                                {
                                    percentage = (Convert.ToDecimal(dr[i - 3].ToString()) / Convert.ToDecimal(dr[i - 1].ToString())) * 100;
                                }
                                VerifyQuarterlyPercentage(percentage, PercAllocated, mHeader);
                            }
                        }
                    }
                }
            }
        }

        public void VerifyQuarterlyPercentage(decimal percentage, BudgetMonth budgetquarter, string quarter)
        {
            decimal data = 0;
            switch (quarter)
            {
                case "Q1":
                    data = Convert.ToDecimal(budgetquarter.BudgetY1);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + quarter + " is " + data.ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "Q2":
                    data = Convert.ToDecimal(budgetquarter.BudgetY4);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + quarter + " is " + data.ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "Q3":
                    data = Convert.ToDecimal(budgetquarter.BudgetY7);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + quarter + " is " + data.ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "Q4":
                    data = Convert.ToDecimal(budgetquarter.BudgetY10);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + quarter + " is " + data.ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;

            }


        }

        #endregion

        #region  Monthly Finance Report With Attribute

        [TestMethod]
        public void A3_MonthlyFinanceReportWithAudience()
        {
            try
            {
                //Call common function for login
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Console.WriteLine(" Testing LoginController - Index method");
                    Console.WriteLine(" The assert value of action is " + IsLogin.RouteValues["Action"] + ". (The expected value is Index.)");
                    Console.WriteLine("\n ----------------------------------------------------------------------");
                    Console.WriteLine("\n Testing  ReportController - GetReportBudgetData Method");

                    ReportController objReportController = new ReportController();
                    ReportModel objReportModel = new ReportModel();
                    BudgetDHTMLXGridDataModel objBudgetDHTMLXGridDataModel = new BudgetDHTMLXGridDataModel();
                    BudgetDHTMLXGridModel objBudgetDHTMLXGridModel = new BudgetDHTMLXGridModel();
                    ObjPlanCommonFunctions.SetSessionData();

                    DataTable dtTactic = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Tactic$]").Tables[0];
                    string viewByName = dtTactic.Rows[0]["ViewByName"].ToString();
                    string viewById = dtTactic.Rows[0]["ViewById"].ToString();

                    DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[MonthlyFinance$]").Tables[0];
                    var result = objReportController.GetReportBudgetData(currentYear, "months", viewById, "") as PartialViewResult;

                    string MainTotalAllocated = result.ViewBag.MainTotalAllocated.ToString();
                    string MainTotalActual = result.ViewBag.MainTotalActual.ToString();
                    BudgetMonth PercAllocated = result.ViewBag.PercAllocated;

                    objBudgetDHTMLXGridModel = (BudgetDHTMLXGridModel)result.Model;
                    List<Budgetdataobj> AttributePlanData = objBudgetDHTMLXGridModel.Grid.rows[1].data;
                    List<string> subHeader = objBudgetDHTMLXGridModel.AttachHeader;

                    string mainHeaderString = objBudgetDHTMLXGridModel.SetHeader;
                    string[] mainHeader = mainHeaderString.Split(',').ToArray();

                    VerifyHeaderValueWithAttribute(dt, MainTotalAllocated, MainTotalActual);
                    Console.WriteLine("\n -------------- Summary - Table Number Validation --------------");
                    VerifyMonthlyDataWithAttribute(dt, AttributePlanData, mainHeader, subHeader, PercAllocated, viewByName);

                    if (objBudgetDHTMLXGridModel.Grid.rows[1].rows != null && objBudgetDHTMLXGridModel.Grid.rows[1].rows.Count > 0)
                    {

                        var Campaign = objBudgetDHTMLXGridModel.Grid.rows[1].rows[0];
                        List<Budgetdataobj> CampaignData = Campaign.data;
                        VerifyMonthlyDataWithAttribute(dt, CampaignData, mainHeader, subHeader);
                        if (Campaign.rows != null && Campaign.rows.Count > 0)
                        {
                            var program = Campaign.rows[0];
                            List<Budgetdataobj> programData = program.data;
                            VerifyMonthlyDataWithAttribute(dt, programData, mainHeader, subHeader);
                            if (program.rows != null && program.rows.Count > 0)
                            {
                                var tactic = program.rows[0];
                                List<Budgetdataobj> tacticData = tactic.data;
                                VerifyMonthlyDataWithAttribute(dt, tacticData, mainHeader, subHeader);
                                if (tactic.rows != null && tactic.rows.Count > 0)
                                {
                                    var lineItem = tactic.rows[0];
                                    List<Budgetdataobj> lineItemData = lineItem.data;
                                    VerifyMonthlyDataWithAttribute(dt, lineItemData, mainHeader, subHeader);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public void VerifyMonthlyDataWithAttribute(DataTable dt, List<Budgetdataobj> data, string[] mainHeader, List<string> subHeader, BudgetMonth PercAllocated = null, string viewByName = null)
        {
            DataRow dr = dt.NewRow();
            if (data.Count > 0 && data != null)
            {
                switch (data[0].value)
                {
                    case "Plan":
                        dr = dt.Rows[0];
                        break;
                    case "Campaign":
                        dr = dt.Rows[1];
                        break;
                    case "Program":
                        dr = dt.Rows[2];
                        break;
                    case "Tactic":
                        dr = dt.Rows[3];
                        break;
                    case "LineItem":
                        dr = dt.Rows[4];
                        break;
                }
                string mHeader = "";
                int j = 4;
                for (int i = 1; i <= data.Count() - 1; i++)
                {
                    string htmlString = data[i].value;
                    string budgetValue = GetBudgetData(htmlString);

                    if (dr != null)
                    {
                        if (i == 1)
                        {
                            if (data[0].value == "Plan")
                            {
                                Assert.AreEqual(viewByName, budgetValue.Trim());
                                Console.WriteLine("\n The assert value of " + mainHeader[i - 1] + " is " + budgetValue.Trim() + ". The expected value is " + viewByName.Trim() + ".)");
                            }
                            else
                            {
                                Assert.AreEqual(dr[i - 1].ToString().Trim(), budgetValue.Trim());
                                Console.WriteLine("\n The assert value of " + mainHeader[i - 1] + " is " + budgetValue.Trim() + ". The expected value is " + dr[i - 1].ToString().Trim() + ".)");

                            }
                        }
                        else
                        {
                            if (budgetValue.Contains("---"))
                            {
                                budgetValue = "0";
                            }

                            if (i <= 40)
                            {
                                if (!mainHeader[i].Contains('#'))
                                {
                                    mHeader = mainHeader[i];
                                }
                            }
                            if (j == i || i == data.Count() - 2 || i == data.Count() - 1)
                            {
                                decimal budgetVal = 0;
                                Assert.AreEqual(Math.Round(budgetVal, 2), Math.Round(Convert.ToDecimal(budgetValue), 2));
                                j = j + 3;
                            }
                            else
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(dr[i - 1].ToString()), 2), Math.Round(Convert.ToDecimal(budgetValue), 2));
                            }
                            Console.WriteLine("\n The assert value of " + subHeader[i] + " of " + dr[0].ToString() + " in " + mHeader + " is " + Math.Round(Convert.ToDecimal(budgetValue), 2).ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(dr[i - 1].ToString()), 2).ToString() + ".)");
                            if (PercAllocated != null && subHeader[i].ToLower() == "budget")
                            {
                                decimal percentage = 0;
                                if (Math.Round(Convert.ToDecimal(dr[i - 2].ToString())) == 0)
                                {
                                    percentage = 101;
                                }
                                else
                                {
                                    percentage = (Convert.ToDecimal(dr[i - 3].ToString()) / Convert.ToDecimal(dr[i - 2].ToString())) * 100;
                                }
                                VerifyMonthlyPercentage(percentage, PercAllocated, mHeader);
                            }
                        }
                    }
                }
            }
        }

        public void VerifyHeaderValueWithAttribute(DataTable dt, string MainTotalAllocated, string MainTotalActual)
        {
            Console.WriteLine("\n -------------- Header Number Validation --------------");
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];

                Assert.AreEqual(dr["Planned Cost"].ToString(), MainTotalAllocated);
                Console.WriteLine("\n The assert value of total allocated amount is " + MainTotalAllocated + ". (The expected value is " + dr["Planned Cost"].ToString() + ".)");
                Assert.AreEqual(dr["Total Actual"].ToString(), MainTotalActual);
                Console.WriteLine("\n The assert value of total spent amount is " + MainTotalActual + ". (The expected value is " + dr["Total Actual"].ToString() + ".)");
            }

        }

        #endregion

        #region  Quarterly Finance Report With Attribute

        [TestMethod]
        public void A4_QuarterlyFinanceReportWithAudience()
        {
            try
            {
                //Call common function for login
                var IsLogin = ObjCommonFunctions.CheckLogin();
                if (IsLogin != null)
                {
                    Console.WriteLine(" Testing LoginController - Index method");
                    Console.WriteLine(" The assert value of action is " + IsLogin.RouteValues["Action"] + ". (The expected value is Index.)");
                    Console.WriteLine("\n ----------------------------------------------------------------------");
                    Console.WriteLine("\n Testing  ReportController - GetReportBudgetData Method");

                    ReportController objReportController = new ReportController();
                    ReportModel objReportModel = new ReportModel();
                    BudgetDHTMLXGridDataModel objBudgetDHTMLXGridDataModel = new BudgetDHTMLXGridDataModel();
                    BudgetDHTMLXGridModel objBudgetDHTMLXGridModel = new BudgetDHTMLXGridModel();
                    ObjPlanCommonFunctions.SetSessionData();

                    DataTable dtTactic = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[Tactic$]").Tables[0];
                    string viewByName = dtTactic.Rows[0]["ViewByName"].ToString();
                    string viewById = dtTactic.Rows[0]["ViewById"].ToString();

                    DataTable dt = ObjCommonFunctions.GetExcelData("GamePlanExcelConn", "[QuarterlyFinance$]").Tables[0];
                    var result = objReportController.GetReportBudgetData(currentYear, "quarters", viewById, "") as PartialViewResult;

                    string MainTotalAllocated = result.ViewBag.MainTotalAllocated.ToString();
                    string MainTotalActual = result.ViewBag.MainTotalActual.ToString();
                    BudgetMonth PercAllocated = result.ViewBag.PercAllocated;

                    objBudgetDHTMLXGridModel = (BudgetDHTMLXGridModel)result.Model;
                    List<Budgetdataobj> AttributePlanData = objBudgetDHTMLXGridModel.Grid.rows[1].data;
                    List<string> subHeader = objBudgetDHTMLXGridModel.AttachHeader;

                    string mainHeaderString = objBudgetDHTMLXGridModel.SetHeader;
                    string[] mainHeader = mainHeaderString.Split(',').ToArray();

                    VerifyHeaderValueWithAttribute(dt, MainTotalAllocated, MainTotalActual);
                    Console.WriteLine("\n -------------- Summary - Table Number Validation --------------");
                    VerifyQuaterlyDataWithAttribute(dt, AttributePlanData, mainHeader, subHeader, PercAllocated, viewByName);

                    if (objBudgetDHTMLXGridModel.Grid.rows[1].rows != null && objBudgetDHTMLXGridModel.Grid.rows[1].rows.Count > 0)
                    {

                        var Campaign = objBudgetDHTMLXGridModel.Grid.rows[1].rows[0];
                        List<Budgetdataobj> CampaignData = Campaign.data;
                        VerifyQuaterlyDataWithAttribute(dt, CampaignData, mainHeader, subHeader);
                        if (Campaign.rows != null && Campaign.rows.Count > 0)
                        {
                            var program = Campaign.rows[0];
                            List<Budgetdataobj> programData = program.data;
                            VerifyQuaterlyDataWithAttribute(dt, programData, mainHeader, subHeader);
                            if (program.rows != null && program.rows.Count > 0)
                            {
                                var tactic = program.rows[0];
                                List<Budgetdataobj> tacticData = tactic.data;
                                VerifyQuaterlyDataWithAttribute(dt, tacticData, mainHeader, subHeader);
                                if (tactic.rows != null && tactic.rows.Count > 0)
                                {
                                    var lineItem = tactic.rows[0];
                                    List<Budgetdataobj> lineItemData = lineItem.data;
                                    VerifyQuaterlyDataWithAttribute(dt, lineItemData, mainHeader, subHeader);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public void VerifyQuaterlyDataWithAttribute(DataTable dt, List<Budgetdataobj> data, string[] mainHeader, List<string> subHeader, BudgetMonth PercAllocated = null, string viewByName = null)
        {
            DataRow dr = dt.NewRow();
            if (data.Count > 0 && data != null)
            {
                switch (data[0].value)
                {
                    case "Plan":
                        dr = dt.Rows[0];
                        break;
                    case "Campaign":
                        dr = dt.Rows[1];
                        break;
                    case "Program":
                        dr = dt.Rows[2];
                        break;
                    case "Tactic":
                        dr = dt.Rows[3];
                        break;
                    case "LineItem":
                        dr = dt.Rows[4];
                        break;
                }
                string mHeader = "";
                int j = 4;
                for (int i = 1; i <= data.Count() - 1; i++)
                {
                    string htmlString = data[i].value;
                    string budgetValue = GetBudgetData(htmlString);

                    if (dr != null)
                    {
                        if (i == 1)
                        {
                            if (data[0].value == "Plan")
                            {
                                Assert.AreEqual(viewByName, budgetValue.Trim());
                                Console.WriteLine("\n The assert value of " + mainHeader[i - 1] + " is " + budgetValue.Trim() + ". (The expected value is " + viewByName.Trim() + ".)");
                            }
                            else
                            {
                                Assert.AreEqual(dr[i - 1].ToString().Trim(), budgetValue.Trim());
                                Console.WriteLine("\n The assert value of " + mainHeader[i - 1] + " is " + budgetValue.Trim() + ". (The expected value is " + dr[i - 1].ToString() + ".)");
                            }
                        }
                        else
                        {
                            if (budgetValue.Contains("---"))
                            {
                                budgetValue = "0";
                            }
                            if (!mainHeader[i].Contains('#'))
                            {
                                mHeader = mainHeader[i];
                            }
                            if (j == i || i == data.Count() - 2 || i == data.Count() - 1)
                            {
                                decimal budgetVal = 0;
                                Assert.AreEqual(Math.Round(budgetVal, 2), Math.Round(Convert.ToDecimal(budgetValue), 2));
                                j = j + 3;
                            }
                            else
                            {
                                Assert.AreEqual(Math.Round(Convert.ToDecimal(dr[i - 1].ToString()), 2), Math.Round(Convert.ToDecimal(budgetValue), 2));
                            }
                            Console.WriteLine("\n The assert value of " + subHeader[i] + " of " + dr[0].ToString() + " in " + mHeader + " is " + budgetValue + ". (The expected value is " + Math.Round(Convert.ToDecimal(dr[i - 1].ToString()), 2).ToString() + ".)");
                            if (PercAllocated != null && subHeader[i].ToLower() == "budget" && i < data.Count() - 1)
                            {
                                decimal percentage = 0;
                                if (Math.Round(Convert.ToDecimal(dr[i - 2].ToString())) == 0)
                                {
                                    percentage = 101;
                                }
                                else
                                {
                                    percentage = (Convert.ToDecimal(dr[i - 3].ToString()) / Convert.ToDecimal(dr[i - 2].ToString())) * 100;
                                }
                                VerifyQuarterlyPercentageWithAttribute(percentage, PercAllocated, mHeader);
                            }
                        }
                    }
                }
            }
        }

        public void VerifyQuarterlyPercentageWithAttribute(decimal percentage, BudgetMonth budgetquarter, string quarter)
        {
            decimal data = 0;
            switch (quarter)
            {
                case "Q1":
                    data = Convert.ToDecimal(budgetquarter.BudgetY1);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + quarter + " is " + data.ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "Q2":
                    data = Convert.ToDecimal(budgetquarter.BudgetY4);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + quarter + " is " + data.ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "Q3":
                    data = Convert.ToDecimal(budgetquarter.BudgetY7);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + quarter + " is " + data.ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;
                case "Q4":
                    data = Convert.ToDecimal(budgetquarter.BudgetY10);
                    Assert.AreEqual(Math.Round(Convert.ToDecimal(percentage), 2), Math.Round(Convert.ToDecimal(data), 2));
                    Console.WriteLine("\n The assert value of percetage in " + quarter + " is " + data.ToString() + ". (The expected value is " + Math.Round(Convert.ToDecimal(percentage), 2).ToString() + ".)");
                    break;

            }


        }

        #endregion
    }
}

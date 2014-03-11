using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System.Transactions;
using Elmah;
using EvoPdf.HtmlToPdf;
using System.Web;
using System.IO;

namespace RevenuePlanner.Controllers
{
    public class ReportController : CommonController
    {
        #region Variables

        private MRPEntities db = new MRPEntities();
        private const string ColumnMonth = "Month";
        private const string ColumnValue = "Value";
        private const string PeriodPrefix = "Y";
        private const string ColumnId = "Id";
        private string PublishedPlan = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString();
        private string currentYear = DateTime.Now.Year.ToString();
        private int currentMonth = DateTime.Now.Month;
        private const double dividemillion = 1000;

        #endregion

        #region Report Summaries

        /// <summary>
        /// This action will return the Report index page
        /// </summary>
        /// <param name="activeMenu"></param>
        /// <returns></returns>
        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Report)
        {
            if (Sessions.RolePermission != null)
            {
                Common.Permission permission = Common.GetPermission(ActionItem.Report);
                switch (permission)
                {
                    case Common.Permission.FullAccess:
                        break;
                    case Common.Permission.NoAccess:
                        return RedirectToAction("Homezero", "Home");
                    case Common.Permission.NotAnEntity:
                        break;
                    case Common.Permission.ViewOnly:
                        ViewBag.IsViewOnly = "true";
                        break;
                }
            }

            ViewBag.ActiveMenu = activeMenu;

            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            string currentYear = DateTime.Now.Year.ToString();

            //List of Business Units
            List<SelectListItem> lstBusinessUnits = Common.GetBussinessUnitIds(Sessions.User.ClientId).Select(p => new SelectListItem() { Text = p.Text, Value = p.Value.ToString(), Selected = false }).ToList();



            //List of Plans
            List<SelectListItem> lstPlans = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Status.Equals(planPublishedStatus) && pl.Year.Equals(currentYear)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();

            if (Sessions.BusinessUnitId != Guid.Empty)
            {
                lstBusinessUnits.Where(lbu => lbu.Value == Convert.ToString(Sessions.BusinessUnitId)).ToList().ForEach(lbu => lbu.Selected = true);
                lstPlans = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Model.BusinessUnitId == Sessions.BusinessUnitId && pl.Status.Equals(planPublishedStatus) && pl.Year.Equals(currentYear)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();
            }


            if (Sessions.ReportPlanId > 0)
            {
                lstPlans.Where(lp => lp.Value == Convert.ToString(Sessions.ReportPlanId)).ToList().ForEach(lp => lp.Selected = true);
            }

            FilterDropdownValues objFilterData = new FilterDropdownValues();
            objFilterData.lstBusinessUnit = lstBusinessUnits;
            objFilterData.lstAllPlans = lstPlans;

            return View(objFilterData);
        }

        /// <summary>
        /// This action will return the Partial View for Summary data
        /// </summary>
        /// <param name="BusinessUnitId"></param>
        /// <param name="PlanId"></param>
        /// <returns></returns>
        public ActionResult GetSummaryData(string BusinessUnitId = "", string PlanId = "")
        {
            SummaryReportModel objSummaryReportModel = new SummaryReportModel();
            ViewBag.IsPlanExistToShowReport = false;


            //// Getting current year's all published plan for all business unit of clientid of director.
            var plans = Common.GetPlan();
            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            string currentYear = DateTime.Now.Year.ToString();
            plans = plans.Where(p => p.Status.Equals(planPublishedStatus) && p.Year.Equals(currentYear)).Select(p => p).ToList();

            //Filter to filter out the plan based on the Selected businessunit and PlanId
            if (!string.IsNullOrEmpty(PlanId) && PlanId != "0")
            {
                int int_PlanId = Convert.ToInt32(PlanId);
                plans = plans.Where(p => p.PlanId.Equals(int_PlanId)).ToList();
                Sessions.ReportPlanId = int_PlanId;
            }
            else if (PlanId == "0") // This means all plans are selected
            {
                Sessions.ReportPlanId = 0;
            }

            if (!string.IsNullOrEmpty(BusinessUnitId) && BusinessUnitId != "0" && BusinessUnitId != Convert.ToString(Guid.Empty))
            {
                Guid BusinessUnitGuid = new Guid(BusinessUnitId);
                plans = plans.Where(pl => pl.Model.BusinessUnitId.Equals(BusinessUnitGuid)).ToList();
                Sessions.BusinessUnitId = BusinessUnitGuid;
            }
            else if (BusinessUnitId == "0" && BusinessUnitId == Convert.ToString(Guid.Empty))
            {
                Sessions.BusinessUnitId = Guid.Empty;
            }

            //double mqlPercentage = 0;
            //int mqlActual = 0;

            double overAllMQLProjected = 0;
            double overAllMQLActual = 0;
            double overAllRevenueActual = 0;
            double overAllRevenueProjected = 0;
            double overAllInqActual = 0;
            double overAllInqProjected = 0;
            double overAllCWActual = 0;
            double overAllCWProjected = 0;

            if (plans != null && plans.Count() != 0)
            {
                ViewBag.IsPlanExistToShowReport = true;

                {
                    //// Getting current year's published plan for business unit of director.
                    Plan sessionActivePlan = plans.Where(p => p.Model.BusinessUnitId.Equals(Sessions.User.BusinessUnitId)).Select(p => p).FirstOrDefault();
                    if (sessionActivePlan != null)
                    {
                        Sessions.PlanId = sessionActivePlan.PlanId;
                    }
                }

                if (!string.IsNullOrEmpty(PlanId) && Convert.ToString(PlanId) != "0")
                {
                    Sessions.PlanId = Convert.ToInt32(PlanId);
                }


                foreach (var plan in plans)
                {
                    Plan activePlan = plan;
                    double currentMQLProjected = 0;
                    double currentMQLActual = 0;
                    double currentRevenueActual = 0;
                    double currentRevenueProjected = 0;

                    #region "Realized to revenue plan"
                    //// MQL
                    //// Added By: Maninder Singh Wadhva for Bug 295:Waterfall Conversion Summary Graph misleading
                    currentMQLActual = GetMQLActual(activePlan.PlanId);
                    currentMQLProjected = GetMQLProjected(activePlan);

                    //// Revenue Actual.
                    currentRevenueActual = GetRevenueActual(activePlan.PlanId);

                    //// Revenue Projected.
                    //// Modified By: Maninder Singh Wadhva to address TFS Bug 296:Close and realize numbers in Revenue Summary are incorrectly calculated.
                    currentRevenueProjected = GetRevenueProjected(activePlan);

                    #endregion

                    #region "For Waterfall Conversion Summary"
                    #region INQ
                    List<long> inq = GetINQ(activePlan.PlanId);
                    double currentInqActual = inq[0];
                    double currentInqProjected = inq[1];
                    #endregion

                    #region CW
                    double currentCWActual = GetCWActual(activePlan.PlanId);
                    double currentCWProjected = GetCWProjected(activePlan);
                    #endregion
                    #endregion

                    #region "Add to overall"
                    overAllMQLProjected += double.IsNaN(currentMQLProjected) ? 0 : currentMQLProjected;
                    overAllMQLActual += double.IsNaN(currentMQLActual) ? 0 : currentMQLActual;
                    overAllRevenueActual += double.IsNaN(currentRevenueActual) ? 0 : currentRevenueActual;
                    overAllRevenueProjected += double.IsNaN(currentRevenueProjected) ? 0 : currentRevenueProjected;
                    overAllInqActual += double.IsNaN(currentInqActual) ? 0 : currentInqActual;
                    overAllInqProjected += double.IsNaN(currentInqProjected) ? 0 : currentInqProjected;
                    overAllCWActual += double.IsNaN(currentCWActual) ? 0 : currentCWActual;
                    overAllCWProjected += double.IsNaN(currentCWProjected) ? 0 : currentCWProjected;
                    #endregion
                }
            }

            overAllRevenueProjected = Math.Round(overAllRevenueProjected);
            overAllRevenueActual = Math.Round(overAllRevenueActual);

            //// MQL
            objSummaryReportModel.MQLs = overAllMQLActual;
            double overAllMQLPercentage = GetPercentageDifference(overAllMQLActual, overAllMQLProjected);
            objSummaryReportModel.MQLsPercentage = overAllMQLPercentage.ToString("#0.##", CultureInfo.InvariantCulture);

            //// Actual Revenue
            objSummaryReportModel.Revenue = FormatNumber(overAllRevenueActual);
            objSummaryReportModel.RevenuePercentage = GetPercentageDifference(overAllRevenueActual, overAllRevenueProjected).ToString("#0.##", CultureInfo.InvariantCulture);

            //// Modified By: Maninder Singh Wadhva to address TFS Bug 296:Close and realize numbers in Revenue Summary are incorrectly calculated.
            string abovePlan = "above plan";
            string belowPlan = "below plan";
            string at_par = "equal to";

            //For Revenue Summary
            objSummaryReportModel.PlanStatus = (overAllRevenueActual < overAllRevenueProjected) ? belowPlan : (overAllRevenueActual > overAllRevenueProjected) ? abovePlan : at_par;

            //// Projected Revenue
            objSummaryReportModel.ProjectedRevenue = FormatNumber(overAllRevenueProjected);

            #region INQ
            double inqPercentageDifference = GetPercentageDifference(overAllInqActual, overAllInqProjected);
            if (inqPercentageDifference < 0)
            {
                objSummaryReportModel.ISQsStatus = string.Format("{0}% {1}", Math.Abs(inqPercentageDifference).ToString("#0.##", CultureInfo.InvariantCulture), belowPlan);
            }
            else
            {
                objSummaryReportModel.ISQsStatus = string.Format("{0}% {1}", inqPercentageDifference.ToString("#0.##", CultureInfo.InvariantCulture), abovePlan);
            }
            #endregion

            #region MQL
            if (overAllMQLPercentage < 0)
            {
                objSummaryReportModel.MQLsStatus = string.Format("{0}% {1}", Math.Abs(overAllMQLPercentage).ToString("#0.##", CultureInfo.InvariantCulture), belowPlan);
            }
            else
            {
                objSummaryReportModel.MQLsStatus = string.Format("{0}% {1}", overAllMQLPercentage.ToString("#0.##", CultureInfo.InvariantCulture), abovePlan);
            }
            #endregion

            #region CW
            double cwPercentageDifference = GetPercentageDifference(overAllCWActual, overAllCWProjected);
            if (cwPercentageDifference < 0)
            {
                objSummaryReportModel.OverallConversionPlanStatus = belowPlan;
                objSummaryReportModel.CWsStatus = string.Format("{0}% {1}", Math.Abs(cwPercentageDifference).ToString("#0.##", CultureInfo.InvariantCulture), belowPlan);
            }
            else
            {
                objSummaryReportModel.OverallConversionPlanStatus = abovePlan;
                objSummaryReportModel.CWsStatus = string.Format("{0}% {1}", cwPercentageDifference.ToString("#0.##", CultureInfo.InvariantCulture), abovePlan);
            }

            #endregion

            #region Chart
            List<WaterfallConversionSummaryChart> chart = new List<WaterfallConversionSummaryChart>();

            WaterfallConversionSummaryChart chartStage = new WaterfallConversionSummaryChart();
            #region INQ
            //// Added By: Maninder Singh Wadhva for Bug 295:Waterfall Conversion Summary Graph misleading
            chartStage.Actual = overAllInqActual.ToString();
            chartStage.Projected = overAllInqProjected.ToString();
            string INQStageLabel = Common.GetLabel(Common.StageModeINQ);
            if (string.IsNullOrEmpty(INQStageLabel))
            {
                chartStage.Stage = "INQ";
            }
            else
            {
                chartStage.Stage = INQStageLabel;
            }

            chart.Add(chartStage);
            #endregion

            #region MQL
            //// Added By: Maninder Singh Wadhva for Bug 295:Waterfall Conversion Summary Graph misleading
            chartStage = new WaterfallConversionSummaryChart();
            chartStage.Actual = overAllMQLActual.ToString();
            chartStage.Projected = overAllMQLProjected.ToString();
            string MQLStageLabel = Common.GetLabel(Common.StageModeMQL);
            if (string.IsNullOrEmpty(MQLStageLabel))
            {
                chartStage.Stage = "MQL";
            }
            else
            {
                chartStage.Stage = MQLStageLabel;
            }
            chart.Add(chartStage);
            #endregion

            #region CW
            //// Bug 295:Waterfall Conversion Summary Graph misleading
            chartStage = new WaterfallConversionSummaryChart();
            chartStage.Actual = overAllCWActual.ToString();
            chartStage.Projected = overAllCWProjected.ToString();
            string CWStageLabel = Common.GetLabel(Common.StageModeCW);
            if (string.IsNullOrEmpty(CWStageLabel))
            {
                chartStage.Stage = "CW";
            }
            else
            {
                chartStage.Stage = CWStageLabel;
            }
            chart.Add(chartStage);
            #endregion

            objSummaryReportModel.chartData = chart;
            #endregion

            // return Partial view
            return PartialView("_Summary", objSummaryReportModel);
        }

        /// <summary>
        /// This method returns the list of Plan for given BusinessUnit Id (This will return only Published plan for Current year)
        /// </summary>
        /// <param name="BusinessUnitId"></param>
        /// <returns></returns>
        public JsonResult GetPlansListFromBusinessUnitId(string BusinessUnitId)
        {
            List<SelectListItem> lstPlan = new List<SelectListItem>();
            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            string currentYear = DateTime.Now.Year.ToString();
            if (!string.IsNullOrEmpty(BusinessUnitId))
            {
                if (BusinessUnitId == "0") // All Business Units is selected
                {
                    lstPlan = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Status.Equals(planPublishedStatus) && pl.Year.Equals(currentYear)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();
                    Sessions.BusinessUnitId = Guid.Empty;
                }
                else
                {
                    Guid BUId = new Guid(BusinessUnitId);
                    lstPlan = Common.GetPlan().Where(pl => pl.Model.BusinessUnitId == BUId && pl.Status.Equals(planPublishedStatus) && pl.Year.Equals(currentYear)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();
                    Sessions.BusinessUnitId = BUId;
                }
            }
            return Json(new { lstPlan }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Function to get Actual and Projected INQ.
        /// </summary>
        /// <returns>Returns actual and projected INQ.</returns>
        private List<long> GetINQ(int planId)
        {
            string tacticApproved = Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString();
            string tacticComplete = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
            string tacticInprogress = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();

            //// Getting approved/inprogress/completed tactic
            var tactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(planId) &&
                                                                    (t.Status.Equals(tacticApproved) || t.Status.Equals(tacticComplete) || t.Status.Equals(tacticInprogress)) &&
                                                                    t.IsDeleted.Equals(false));
            long inqActual = 0, inqProjected = 0;
            if (tactic.Count() != 0)
            {
                //// Actual INQ entered for all approved/inprogress/completed tactic
                inqActual = Convert.ToInt64(tactic.Sum(t => t.INQsActual));

                //// Projected INQ for all approved/inprogress/completed tactic
                inqProjected = Convert.ToInt64(tactic.Sum(t => t.INQs));
            }

            List<long> inq = new List<long>();
            inq.Add(inqActual);
            inq.Add(inqProjected);

            return inq;
        }

        /// <summary>
        /// Function to get revenue percentage.
        /// </summary>
        /// <param name="revenueActual">Revenue Actual.</param>
        /// <param name="revenueProjected">Revenue Projected.</param>
        /// <returns>Returns percentage revenue.</returns>
        private double GetPercentageDifference(double actual, double projected)
        {
            double percentage = 0;
            if (projected != 0)
            {
                percentage = (actual / projected) * 100;
            }

            percentage = (percentage - 100);
            return percentage;
        }

        /// <summary>
        /// Function to get actual revenue.
        /// </summary>
        /// <returns>Actual Revenue.</returns>
        private double GetRevenueActual(int planId)
        {
            string tacticApproved = Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString();
            string tacticComplete = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
            string tacticInprogress = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();

            //// Getting approved/inprogress/completed tactic
            var tactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(planId) &&
                                                                    (t.Status.Equals(tacticApproved) || t.Status.Equals(tacticComplete) || t.Status.Equals(tacticInprogress)) &&
                                                                    t.IsDeleted.Equals(false));

            double revenueActual = 0;
            if (tactic.Count() != 0)
            {
                //// Actual revenue entered for all approved/inprogress/completed tactic
                revenueActual = Convert.ToDouble(tactic.Sum(t => t.RevenuesActual));
            }

            return revenueActual;
        }

        /// <summary>
        /// Function to get projected revenue.
        /// Modified By: Maninder Singh Wadhva to address TFS Bug 296:Close and realize numbers in Revenue Summary are incorrectly calculated.
        /// </summary>
        /// <param name="plan">Current Plan.</param>
        /// <returns>Returns projected revenue for current plan.</returns>
        private double GetRevenueProjected(Plan plan)
        {
            List<string> tacticStatus = Common.GetStatusListAfterApproved();

            //// Getting non-deleted and approved/in-progress/completed tactic id of current plan.
            List<int> tacticsId = db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted.Equals(false) &&
                                                                tacticStatus.Contains(t.Status) &&
                                                                t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(t => t.PlanTacticId).ToList<int>();

            double projectedRevenue = 0;
            if (tacticsId.Count() > 0)
            {
                //// Getting projected revenue for each tactic.
                List<ProjectedRevenueClass> tacticsProjectedRevenue = ProjectedRevenueCalculate(tacticsId);
                if (tacticsProjectedRevenue.Count() > 0)
                {
                    //// Aggregating projected revenue of tactic to get same for plan.
                    projectedRevenue = tacticsProjectedRevenue.Sum(tactics => tactics.ProjectedRevenue);
                }
            }

            return projectedRevenue;
        }

        /// <summary>
        /// Function to get projected MQL.
        /// Added By: Maninder Singh Wadhva for Bug 295:Waterfall Conversion Summary Graph misleading
        /// </summary>
        /// <param name="plan">Plan for which MQL is to be calculated.</param>
        /// <returns>Returns Projected MQL.</returns>
        private double GetMQLProjected(Plan plan)
        {
            List<string> tacticStatus = Common.GetStatusListAfterApproved();

            //// Getting non-deleted and approved/in-progress/completed tactic id of current plan.
            var tactics = db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted.Equals(false) &&
                                                                tacticStatus.Contains(t.Status) &&
                                                                t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(t => t);

            double projectedMQL = 0;
            if (tactics.Count() > 0)
            {
                //// Getting projected MQL for plan.
                projectedMQL = tactics.Sum(tactic => tactic.MQLs);
            }

            return projectedMQL;
        }

        /// <summary>
        /// Function to get actual MQL.
        /// </summary>
        /// <returns>Returns actual MQL.</returns>
        private double GetMQLActual(int planId)
        {
            string tacticApproved = Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString();
            string tacticComplete = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
            string tacticInprogress = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();

            //// Getting approved/inprogress/completed tactic
            var tactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(planId) &&
                                                                    (t.Status.Equals(tacticApproved) || t.Status.Equals(tacticComplete) || t.Status.Equals(tacticInprogress)) &&
                                                                    t.IsDeleted.Equals(false));
            double mqlActual = 0;
            if (tactic.Count() != 0)
            {
                //// Actual MQL entered for all approved/inprogress/completed tactic
                mqlActual = Convert.ToDouble(tactic.Sum(t => t.MQLsActual));
            }

            return mqlActual;
        }

        /// <summary>
        /// Function to get actual closed won.
        /// </summary>
        /// <returns>Returns CW actuals.</returns>
        private double GetCWActual(int planId)
        {
            string tacticApproved = Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString();
            string tacticComplete = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
            string tacticInprogress = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();

            //// Getting approved/inprogress/completed tactic
            var tactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(planId) &&
                                                                    (t.Status.Equals(tacticApproved) || t.Status.Equals(tacticComplete) || t.Status.Equals(tacticInprogress)) &&
                                                                    t.IsDeleted.Equals(false));
            double cwActual = 0;
            if (tactic.Count() != 0)
            {
                //// Actual INQ entered for all approved/inprogress/completed tactic
                cwActual = Convert.ToDouble(tactic.Sum(t => t.CWsActual));
            }

            return cwActual;
        }

        /// <summary>
        /// Function to get projected closed won.
        /// Modified By: Maninder Singh Wadhva to address TFS Bug#280 :Error Message Showing when editing a tactic - Preventing MQLs from updating
        /// Modified By: Maninder Singh Wadhva on 27-Feb-2014 to address TFS Bug#296.
        /// </summary>
        /// <param name="plan">Plan for which closed won is to be calculated.</param>
        /// <returns>Returns closed won for current plan.</returns>
        private double GetCWProjected(Plan plan)
        {
            List<string> tacticStatus = Common.GetStatusListAfterApproved();

            //// Getting non-deleted and approved/in-progress/completed tactic id of current plan.
            List<int> tacticsId = db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted.Equals(false) &&
                                                                tacticStatus.Contains(t.Status) &&
                                                                t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(t => t.PlanTacticId).ToList<int>();

            double projectedCW = 0;
            if (tacticsId.Count() > 0)
            {
                //// Getting projected CW for each tactic.
                List<ProjectedRevenueClass> tacticsProjectedCW = ProjectedRevenueCalculate(tacticsId, true);
                if (tacticsProjectedCW.Count() > 0)
                {
                    //// Aggregating projected CW of tactic to get same for plan.
                    projectedCW = tacticsProjectedCW.Sum(tactics => tactics.ProjectedRevenue);
                }
            }

            return projectedCW;
        }

        /// <summary>
        /// Function to get formated number.
        /// </summary>
        /// <param name="revenue">Number</param>
        /// <returns>Returns formated number.</returns>
        private string FormatNumber(double number)
        {
            //double value = 1234567890;
            if (number < 9999)
            {
                // Displays 1,234,567,890   
                return number.ToString();
            }
            else if (number <= 999999)
            {
                //// Displays 1,234,568K
                return number.ToString("#,##0,.##K", CultureInfo.InvariantCulture);
            }
            else if (number <= 999999999)
            {
                // Displays 1,235M
                return number.ToString("#,##0,,.##M", CultureInfo.InvariantCulture);
            }
            else
            {
                //// Displays 1B
                return number.ToString("#,##0,,,.##B", CultureInfo.InvariantCulture);
            }
        }

        #endregion

        #region Conversion Summary

        /// <summary>
        /// Returns view of Conversion report
        /// </summary>
        /// <returns></returns>
        public ActionResult Conversion(string id = "thisyear")
        {
            if (Sessions.RolePermission != null)
            {
                Common.Permission permission = Common.GetPermission(ActionItem.Report);
                switch (permission)
                {
                    case Common.Permission.FullAccess:
                        break;
                    case Common.Permission.NoAccess:
                        return RedirectToAction("Index", "NoAccess");
                    case Common.Permission.NotAnEntity:
                        break;
                    case Common.Permission.ViewOnly:
                        ViewBag.IsViewOnly = "true";
                        break;
                }
            }

            if (Sessions.ReportPlanId == 0)
            {
                ViewBag.PlanTitle = "All Plans";
            }
            else
            {
                var plan = db.Plans.Single(p => p.PlanId.Equals(Sessions.ReportPlanId));
                ViewBag.PlanTitle = plan.Title;
            }

            //// Get plan to show in report.
            //var plans = GetPlanForReport();
            //if (plans != null && plans.Count() != 0)
            //{
            //    //// Getting current year's published plan for business unit of director/system admin/client admint for add actuals.
            //    Plan sessionActivePlan = plans.Where(p => p.Model.BusinessUnitId.Equals(Sessions.User.BusinessUnitId)).Select(p => p).FirstOrDefault();
            //    if (sessionActivePlan != null)
            //    {
            //        Sessions.PlanId = sessionActivePlan.PlanId;
            //    }
            //}

            ViewBag.MonthTitle = GetDisplayMonthList(id);
            ViewBag.SelectOption = id;

            return View();
        }

        #region MQL Conversion Plan Report

        /// <summary>
        /// This method returns the data for MQL Conversion plan summary report
        /// </summary>
        /// <param name="ParentTab"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult GetMQLConversionPlanData(string ParentTab = "", string Id = "", string selectOption = "thisyear")
        {
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            List<string> includeYearList = GetYearList(selectOption);
            List<string> includeMonth = GetMonthList(selectOption);
            List<int> rrTacticList = new List<int>();
            List<int> tacticIds = GetTacticForReport(includeYearList);
            if (ParentTab == Common.BusinessUnit)
            {
                Guid buid = new Guid(Id);
                rrTacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.BusinessUnitId == buid).Select(t => t.PlanTacticId).ToList();
            }
            else if (ParentTab == Common.Audience)
            {
                int auid = Convert.ToInt32(Id);
                rrTacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.AudienceId == auid).Select(t => t.PlanTacticId).ToList();
            }
            else if (ParentTab == Common.Geography)
            {
                Guid geographyId = new Guid(Id);
                rrTacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.GeographyId == geographyId).Select(t => t.PlanTacticId).ToList();
            }
            else if (ParentTab == Common.Vertical)
            {
                int verticalid = Convert.ToInt32(Id);
                rrTacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.VerticalId == verticalid).Select(t => t.PlanTacticId).ToList();
            }

            if (rrTacticList.Count() > 0)
            {
                var rdata = new[] { new { 
                INQGoal = GetConversionProjectedINQData(rrTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                monthList = includeMonth,
                INQActual = (db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => rrTacticList.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.INQ.ToString()].ToString()))).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period).Select(pcptj => new
                {
                    PKey = pcptj.Key,
                    PSum = pcptj.Sum(pt => pt.Actualvalue)
                }),
                MQLGoal = GetConversionProjectedMQLData(rrTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                MQLActual = (db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => rrTacticList.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()))).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period).Select(pcptj => new
                {
                    PKey = pcptj.Key,
                    PSum = pcptj.Sum(pt => pt.Actualvalue)
                })
            }  };

                return Json(rdata, JsonRequestBehavior.AllowGet);
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Projected INQ Data With Month Wise.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetConversionProjectedINQData(List<int> tlist)
        {
            List<TacticDataTable> tacticdata = (from tactic in db.Plan_Campaign_Program_Tactic.ToList()
                                                where tlist.Contains(tactic.PlanTacticId)
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.PlanTacticId,
                                                    Value = tactic.INQs,
                                                    StartMonth = tactic.StartDate.Month,
                                                    EndMonth = tactic.EndDate.Month,
                                                    StartYear = tactic.StartDate.Year,
                                                    EndYear = tactic.EndDate.Year
                                                }).ToList();

            return GetDatatable(tacticdata);
        }

        /// <summary>
        /// Get Projected MQL Data With Month Wise.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetConversionProjectedMQLData(List<int> tlist)
        {
            List<TacticDataTable> tacticdata = (from tactic in db.Plan_Campaign_Program_Tactic.ToList()
                                                where tlist.Contains(tactic.PlanTacticId)
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.PlanTacticId,
                                                    Value = tactic.MQLs,
                                                    StartMonth = tactic.StartDate.Month,
                                                    EndMonth = tactic.EndDate.Month,
                                                    StartYear = tactic.StartDate.Year,
                                                    EndYear = tactic.EndDate.Year
                                                }).ToList();

            return GetDatatable(tacticdata);
        }

        /// <summary>
        /// Get Child Tab.
        /// </summary>
        /// <param name="ParentTab"></param>
        /// <returns></returns>
        public JsonResult GetChildTab(string ParentTab)
        {
            // Businessunit id =  B478B6E8-5C9D-4549-BCA2-B810FD22508E

            var returnDataGuid = (db.BusinessUnits.ToList().Where(bu => bu.ClientId.Equals(Sessions.User.ClientId) && bu.IsDeleted.Equals(false)).Select(bu => bu).ToList()).Select(b => new
            {
                id = b.BusinessUnitId,
                title = b.Title
            }).Select(b => b).Distinct().OrderBy(b => b.id);

            var returnDataInt = (db.Audiences.ToList().Where(au => au.ClientId.Equals(Sessions.User.ClientId) && au.IsDeleted.Equals(false)).Select(au => au).ToList()).Select(a => new
            {
                id = a.AudienceId,
                title = a.Title
            }).Select(a => a).Distinct().OrderBy(a => a.id);

            if (ParentTab == Common.BusinessUnit)
            {
                returnDataGuid = (db.BusinessUnits.ToList().Where(bu => bu.ClientId.Equals(Sessions.User.ClientId) && bu.IsDeleted.Equals(false)).Select(bu => bu).ToList()).Select(b => new
                {
                    id = b.BusinessUnitId,
                    title = b.Title
                }).Select(b => b).Distinct().OrderBy(b => b.id);

                return Json(returnDataGuid, JsonRequestBehavior.AllowGet);
            }
            else if (ParentTab == Common.Audience)
            {
                returnDataInt = (db.Audiences.ToList().Where(au => au.ClientId.Equals(Sessions.User.ClientId) && au.IsDeleted.Equals(false)).Select(au => au).ToList()).Select(a => new
                {
                    id = a.AudienceId,
                    title = a.Title
                }).Select(a => a).Distinct().OrderBy(a => a.id);

                return Json(returnDataInt, JsonRequestBehavior.AllowGet);
            }

            else if (ParentTab == Common.Geography)
            {
                returnDataGuid = (db.Geographies.ToList().Where(ge => ge.ClientId.Equals(Sessions.User.ClientId) && ge.IsDeleted.Equals(false)).Select(ge => ge).ToList()).Select(g => new
                {
                    id = g.GeographyId,
                    title = g.Title
                }).Select(g => g).Distinct().OrderBy(g => g.id);

                return Json(returnDataGuid, JsonRequestBehavior.AllowGet);
            }
            else if (ParentTab == Common.Vertical)
            {
                returnDataInt = (db.Verticals.ToList().Where(ve => ve.ClientId.Equals(Sessions.User.ClientId) && ve.IsDeleted.Equals(false)).Select(ve => ve).ToList()).Select(v => new
                {
                    id = v.VerticalId,
                    title = v.Title
                }).Select(v => v).Distinct().OrderBy(v => v.id);

                return Json(returnDataInt, JsonRequestBehavior.AllowGet);
            }

            return Json(returnDataGuid, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region MQL Performance Report

        // <summary>
        // This method will return the list of all tactics for current client for current year
        // </summary>
        // <param name="BusinessUnitId">This is optional argument, if we don't pass any BusinessUnit, It'll return tacticId for all BusinessUnit</param>
        // <returns></returns>
        //private List<int> GetTacticsForBusinessUnit(Guid BusinessUnitId = new Guid())
        //{
        //    List<Guid> lst_BusinessUnitId = new List<Guid>();
        //    List<int> lst_ModelId = new List<int>();
        //    List<int> lst_PlanId = new List<int>();
        //    List<int> lst_Plan_CampaignId = new List<int>();
        //    List<int> lst_Plan_Campaign_ProgramId = new List<int>();
        //    List<int> lst_Plan_Campaing_Program_TacticId = new List<int>();

        //    if (BusinessUnitId != Guid.Empty)
        //    {
        //        lst_ModelId = db.Models.Where(m => m.BusinessUnitId == BusinessUnitId && m.IsDeleted == false).Select(m => m.ModelId).ToList();
        //    }
        //    else
        //    {
        //        lst_BusinessUnitId = db.BusinessUnits.Where(bu => bu.ClientId == Sessions.User.ClientId && bu.IsDeleted == false).Select(bu => bu.BusinessUnitId).ToList();
        //        if (lst_BusinessUnitId.Count > 0)
        //        {
        //            lst_ModelId = db.Models.Where(m => lst_BusinessUnitId.Contains(m.BusinessUnitId) && m.IsDeleted == false).Select(m => m.ModelId).ToList();
        //        }
        //    }

        //    var Current_Year = DateTime.Now.Year.ToString();

        //    if (lst_ModelId.Count > 0)
        //    {

        //        string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
        //        string currentYear = DateTime.Now.Year.ToString();
        //        List<int> planIds = db.Plans.Where(p => lst_ModelId.Contains(p.ModelId) && p.Status.Equals(planPublishedStatus) && p.Year.Equals(currentYear) && p.IsDeleted == false).Select(p => p.PlanId).ToList();

        //        // Tactic status approved/in-progress/complete.
        //        List<string> status = Enums.TacticStatusValues.Where(tsv => tsv.Key.Equals(Enums.TacticStatus.Approved.ToString())
        //                                                             || tsv.Key.Equals(Enums.TacticStatus.InProgress.ToString()) || tsv.Key.Equals(Enums.TacticStatus.Complete.ToString())).Select(tsv => tsv.Value).ToList<string>();

        //        // Variable for revenue stage title .
        //        string stageTitleRevenue = Enums.InspectStageValues.Single(isv => isv.Key.Equals(Enums.InspectStage.Revenue.ToString())).Value;

        //        // Getting period wise actual revenue of non-deleted and approved/in-progress/complete tactic of plan present in planids.
        //        lst_Plan_Campaing_Program_TacticId = db.Plan_Campaign_Program_Tactic.Where(pcpt => planIds.Contains(pcpt.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
        //                                                                                        status.Contains(pcpt.Status) &&
        //                                                                                        pcpt.IsDeleted.Equals(false)).Select(pcpt => pcpt.PlanTacticId).ToList();
        //    }
        //    return lst_Plan_Campaing_Program_TacticId;
        //}

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to get data for source performance report.
        /// </summary>
        /// <param name="filter">Filter to get data for plan/trend or actual.</param>
        /// <returns>Return json data for source performance report.</returns>
        public JsonResult GetMQLPerformance(string filter, string selectOption = "thisyear")
        {
            if (filter.Equals(Common.Plan))
            {
                return GetMQLPerformanceProjected(selectOption);
            }
            else if (filter.Equals(Common.Trend))
            {
                return GetMQLPerformanceTrend(selectOption);
            }
            else
            {
                return GetMQLPerformanceActual(selectOption);
            }
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to get source perfromance trend.
        /// </summary>
        /// <returns>Returns json result of source perfromance trend.</returns>
        private JsonResult GetMQLPerformanceTrend(string selectOption)
        {
            string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            List<string> includeYearList = GetYearList(selectOption, true);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            List<string> months = GetUpToCurrentMonth();

            int lastMonth = GetLastMonthForTrend(selectOption);
            var planCampaignTacticActualAll = db.Plan_Campaign_Program_Tactic_Actual.Where(tactic => tacticIds.Contains(tactic.PlanTacticId));

            var tacticTrenBusinessUnit = planCampaignTacticActualAll.Where(ta => months.Contains(ta.Period) &&
                                                                (ta.StageTitle == mql))
                                                   .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.BusinessUnitId)
                                                   .Select(ta => new
                                                   {
                                                       BusinessUnitId = ta.Key,
                                                       Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth) / dividemillion
                                                   });

            var tacticTrendGeography = planCampaignTacticActualAll.Where(ta => months.Contains(ta.Period) &&
                                                     (ta.StageTitle == mql))
                                        .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.GeographyId)
                                        .Select(ta => new
                                        {
                                            GeographyId = ta.Key,
                                            Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth) / dividemillion
                                        });

            var tacticTrendVertical = planCampaignTacticActualAll.Where(ta => months.Contains(ta.Period) &&
                                         (ta.StageTitle == mql))
                            .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.VerticalId)
                            .Select(ta => new
                            {
                                VerticalId = ta.Key,
                                Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth) / dividemillion
                            });

            var businessUnits = db.BusinessUnits.ToList().Where(b => b.ClientId.Equals(Sessions.User.ClientId))
                                           .Select(b => new
                                           {
                                               Title = b.Title,
                                               ColorCode = string.Format("#{0}", b.ColorCode),
                                               Value = tacticTrenBusinessUnit.Any(bu => bu.BusinessUnitId.Equals(b.BusinessUnitId)) ? tacticTrenBusinessUnit.Where(bu => bu.BusinessUnitId.Equals(b.BusinessUnitId)).First().Trend : 0

                                           });
            var vertical = db.Verticals.ToList().Where(v => v.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = tacticTrendVertical.Any(ve => ve.VerticalId.Equals(v.VerticalId)) ? tacticTrendVertical.Where(ve => ve.VerticalId.Equals(v.VerticalId)).First().Trend : 0
                                                });

            var geography = db.Geographies.ToList().Where(g => g.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = tacticTrendGeography.Any(ge => ge.GeographyId.Equals(g.GeographyId)) ? tacticTrendGeography.Where(ge => ge.GeographyId.Equals(g.GeographyId)).First().Trend : 0
                                                });
            return Json(new
            {
                ChartBusinessUnit = businessUnits,
                ChartVertical = vertical,
                ChartGeography = geography
            }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to get source perfromance actual.
        /// </summary>
        /// <returns>Returns json result of source perfromance actual.</returns>
        private JsonResult GetMQLPerformanceActual(string selectOption)
        {
            List<string> includeYearList = GetYearList(selectOption, true);
            List<string> includeMonth = GetMonthList(selectOption, true);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            var businessUnits = db.BusinessUnits.ToList().Where(b => b.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(b => new
                                                {
                                                    Title = b.Title,
                                                    ColorCode = string.Format("#{0}", b.ColorCode),
                                                    Value = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(mql)).Any(ta => ta.Plan_Campaign_Program_Tactic.BusinessUnitId.Equals(b.BusinessUnitId)) ? db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.Plan_Campaign_Program_Tactic.BusinessUnitId == b.BusinessUnitId && ta.StageTitle.Equals(mql)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue) / dividemillion : 0
                                                });
            var vertical = db.Verticals.ToList().Where(v => v.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(mql)).Any(ta => ta.Plan_Campaign_Program_Tactic.VerticalId.Equals(v.VerticalId)) ? db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.Plan_Campaign_Program_Tactic.VerticalId == v.VerticalId && ta.StageTitle.Equals(mql)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue) / dividemillion : 0
                                                });

            var geography = db.Geographies.ToList().Where(g => g.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(mql)).Any(ta => ta.Plan_Campaign_Program_Tactic.GeographyId.Equals(g.GeographyId)) ? db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.Plan_Campaign_Program_Tactic.GeographyId == g.GeographyId && ta.StageTitle.Equals(mql)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue) / dividemillion : 0
                                                });
            return Json(new
            {
                ChartBusinessUnit = businessUnits,
                ChartVertical = vertical,
                ChartGeography = geography
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to get source perfromance projected.
        /// </summary>
        /// <returns>Returns json result of source perfromance projected.</returns>
        private JsonResult GetMQLPerformanceProjected(string selectOption)
        {
            List<string> includeYearList = GetYearList(selectOption, true);
            List<string> includeMonth = GetMonthList(selectOption, true);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            //// Applying filters i.e. bussiness unit, audience, vertical or geography.

            var businessUnits = db.BusinessUnits.ToList().Where(b => b.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(b => new
                                                {
                                                    Title = b.Title,
                                                    ColorCode = string.Format("#{0}", b.ColorCode),
                                                    Value = db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId)).Any(t => t.BusinessUnitId.Equals(b.BusinessUnitId)) ? GetProjectedMQLData(db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId) && t.BusinessUnitId.Equals(b.BusinessUnitId)).Select(t => t.PlanTacticId).ToList()).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)) / dividemillion : 0
                                                });



            var vertical = db.Verticals.ToList().Where(v => v.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId)).Any(t => t.VerticalId.Equals(v.VerticalId)) ? GetProjectedMQLData(db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId) && t.VerticalId.Equals(v.VerticalId)).Select(t => t.PlanTacticId).ToList()).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)) / dividemillion : 0
                                                });

            var geography = db.Geographies.ToList().Where(g => g.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId)).Any(t => t.GeographyId.Equals(g.GeographyId)) ? GetProjectedMQLData(db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId) && t.GeographyId.Equals(g.GeographyId)).Select(t => t.PlanTacticId).ToList()).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)) / dividemillion : 0
                                                });
            return Json(new
            {
                ChartBusinessUnit = businessUnits,
                ChartVertical = vertical,
                ChartGeography = geography
            }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Conversion Summary Report

        /// <summary>
        /// This will return the data for Conversion Summary report
        /// </summary>
        /// <param name="ParentConversionSummaryTab"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult GetConversionSummary(string ParentConversionSummaryTab = "", string Id = "", string selectOption = "thisyear")
        {
            List<ConversionSummary> data = new List<ConversionSummary>();
            List<int> lst_Model_FunnelIds = new List<int>();
            int FunnelId = Convert.ToInt32(Id);
            List<string> includeYearList = GetYearList(selectOption);
            List<int> tacticIds = GetTacticForReport(includeYearList);

            if (ParentConversionSummaryTab.Equals(Common.BusinessUnit))
            {
                List<Guid> lst_BU = new List<Guid>();
                List<int> lst_BU_tactic = new List<int>();
                lst_BU = db.BusinessUnits.Where(bu => bu.ClientId.Equals(Sessions.User.ClientId) && bu.IsDeleted == false).Select(bu => bu.BusinessUnitId).ToList(); //gets the list of businessUnits for current client
                int i = 0;
                if (lst_BU.Count > 0)
                {
                    foreach (var individual_BU in lst_BU)
                    {
                        var obj_BusinessUnit = db.BusinessUnits.Where(bu => bu.BusinessUnitId == individual_BU).FirstOrDefault();
                        i++;
                        lst_BU_tactic = new List<int>();
                        Guid businessUnitId = new Guid(Convert.ToString(individual_BU));
                        lst_BU_tactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.BusinessUnitId == businessUnitId).Select(pcpt => pcpt.PlanTacticId).ToList(); //Gets the list of tacticId for current Business Unit
                        if (lst_BU_tactic.Count > 0)
                        {
                            var lst_modelId = db.Plan_Campaign_Program_Tactic.Where(pcpt => lst_BU_tactic.Contains(pcpt.PlanTacticId)).Select(pcpt => pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId).Distinct().ToList();
                            if (FunnelId > 0)
                                lst_Model_FunnelIds = db.Model_Funnel.Where(mf => lst_modelId.Contains(mf.ModelId) && mf.FunnelId == FunnelId).Select(mf => mf.ModelFunnelId).ToList();
                            else
                                lst_Model_FunnelIds = db.Model_Funnel.Where(mf => lst_modelId.Contains(mf.ModelId)).Select(mf => mf.ModelFunnelId).ToList();
                            ConversionSummary c1 = new ConversionSummary();
                            c1 = GetConversionSummaryRow(i, obj_BusinessUnit.Title, lst_BU_tactic, lst_Model_FunnelIds, selectOption);
                            data.Add(c1);
                        }
                    }
                }
            }
            else if (ParentConversionSummaryTab.Equals(Common.Audience))
            {
                List<int> lst_AU = new List<int>();
                List<int> lst_AU_tactic = new List<int>();
                lst_AU = db.Audiences.Where(au => au.ClientId == Sessions.User.ClientId && au.IsDeleted == false).Select(au => au.AudienceId).ToList();

                int i = 0;
                if (lst_AU.Count > 0)
                {
                    foreach (var individial_AU in lst_AU)
                    {
                        var obj_Audience = db.Audiences.Where(au => au.AudienceId == individial_AU).FirstOrDefault();
                        i++;
                        lst_AU_tactic = new List<int>();
                        lst_AU_tactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.AudienceId.Equals(individial_AU)).Select(pcpt => pcpt.PlanTacticId).ToList();
                        if (lst_AU_tactic.Count > 0)
                        {
                            var lst_modelId = db.Plan_Campaign_Program_Tactic.Where(pcpt => lst_AU_tactic.Contains(pcpt.PlanTacticId)).Select(pcpt => pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId).Distinct().ToList();
                            if (FunnelId > 0)
                                lst_Model_FunnelIds = db.Model_Funnel.Where(mf => lst_modelId.Contains(mf.ModelId) && mf.FunnelId == FunnelId).Select(mf => mf.ModelFunnelId).ToList();
                            else
                                lst_Model_FunnelIds = db.Model_Funnel.Where(mf => lst_modelId.Contains(mf.ModelId)).Select(mf => mf.ModelFunnelId).ToList();
                            ConversionSummary c1 = new ConversionSummary();
                            c1 = GetConversionSummaryRow(i, obj_Audience.Title, lst_AU_tactic, lst_Model_FunnelIds, selectOption);
                            data.Add(c1);
                        }
                    }
                }
            }
            else if (ParentConversionSummaryTab.Equals(Common.Geography))
            {
                List<Guid> lst_GE = new List<Guid>();
                List<int> lst_GE_tactic = new List<int>();
                lst_GE = db.Geographies.Where(ge => ge.ClientId == Sessions.User.ClientId && ge.IsDeleted == false).Select(ge => ge.GeographyId).ToList();
                int i = 0;
                if (lst_GE.Count > 0)
                {
                    foreach (var individial_GE in lst_GE)
                    {
                        var obj_Geography = db.Geographies.Where(ge => ge.GeographyId == individial_GE).FirstOrDefault();
                        i++;
                        lst_GE_tactic = new List<int>();
                        lst_GE_tactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.GeographyId.Equals(individial_GE)).Select(pcpt => pcpt.PlanTacticId).ToList();
                        if (lst_GE_tactic.Count > 0)
                        {
                            var lst_modelId = db.Plan_Campaign_Program_Tactic.Where(pcpt => lst_GE_tactic.Contains(pcpt.PlanTacticId)).Select(pcpt => pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId).Distinct().ToList();
                            if (FunnelId > 0)
                                lst_Model_FunnelIds = db.Model_Funnel.Where(mf => lst_modelId.Contains(mf.ModelId) && mf.FunnelId == FunnelId).Select(mf => mf.ModelFunnelId).ToList();
                            else
                                lst_Model_FunnelIds = db.Model_Funnel.Where(mf => lst_modelId.Contains(mf.ModelId)).Select(mf => mf.ModelFunnelId).ToList();
                            ConversionSummary c1 = new ConversionSummary();
                            c1 = GetConversionSummaryRow(i, obj_Geography.Title, lst_GE_tactic, lst_Model_FunnelIds, selectOption);
                            data.Add(c1);
                        }
                    }
                }
            }
            else if (ParentConversionSummaryTab.Equals(Common.Vertical))
            {
                List<int> lst_VE = new List<int>();
                List<int> lst_VE_tactic = new List<int>();
                lst_VE = db.Verticals.Where(ve => ve.ClientId == Sessions.User.ClientId && ve.IsDeleted == false).Select(ve => ve.VerticalId).ToList();
                int i = 0;
                if (lst_VE.Count > 0)
                {
                    foreach (var individial_VE in lst_VE)
                    {
                        var obj_Vertical = db.Verticals.Where(ve => ve.VerticalId == individial_VE).FirstOrDefault();
                        i++;
                        lst_VE_tactic = new List<int>();
                        lst_VE_tactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.VerticalId.Equals(individial_VE)).Select(pcpt => pcpt.PlanTacticId).ToList();
                        if (lst_VE_tactic.Count > 0)
                        {
                            var lst_modelId = db.Plan_Campaign_Program_Tactic.Where(pcpt => lst_VE_tactic.Contains(pcpt.PlanTacticId)).Select(pcpt => pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId).Distinct().ToList();
                            if (FunnelId > 0)
                                lst_Model_FunnelIds = db.Model_Funnel.Where(mf => lst_modelId.Contains(mf.ModelId) && mf.FunnelId == FunnelId).Select(mf => mf.ModelFunnelId).ToList();
                            else
                                lst_Model_FunnelIds = db.Model_Funnel.Where(mf => lst_modelId.Contains(mf.ModelId)).Select(mf => mf.ModelFunnelId).ToList();
                            ConversionSummary c1 = new ConversionSummary();
                            c1 = GetConversionSummaryRow(i, obj_Vertical.Title, lst_VE_tactic, lst_Model_FunnelIds, selectOption);
                            data.Add(c1);
                        }
                    }
                }
            }

            // Code to calculate Blended average and Total

            if (data.Count > 0)
            {
                var TotalRows = data.Count;

                double Sum_NewInq = 0;
                double Avg_NewInq = 0;

                double Sum_Actual_InqMql = 0;
                double Sum_Projected_InqMql = 0;
                double Avg_Actual_InqMql = 0;
                double Avg_Projected_InqMql = 0;

                double Sum_Mql = 0;
                double Avg_Mql = 0;

                double Sum_Cw = 0;
                double Avg_Cw = 0;

                double Sum_Actual_Ads = 0;
                double Sum_Projected_Ads = 0;
                double Avg_Actual_Ads = 0;
                double Avg_Projected_Ads = 0;

                double Sum_Actual_Rev = 0;
                double Sum_Projected_Rev = 0;
                double Avg_Actual_Rev = 0;
                double Avg_Projected_Rev = 0;

                foreach (ConversionSummary c1 in data)
                {
                    Sum_NewInq += c1.newInq;
                    Sum_Actual_InqMql += c1.inqMql[0].val;
                    Sum_Projected_InqMql += c1.inqMql[1].val;
                    Sum_Mql += c1.mql;
                    Sum_Cw += c1.cw;
                    Sum_Actual_Ads += c1.ads[0].val;
                    Sum_Projected_Ads += c1.ads[1].val;
                    Sum_Actual_Rev += c1.revenue[0].val;
                    Sum_Projected_Rev += c1.revenue[1].val;
                }

                Avg_NewInq = Sum_NewInq / TotalRows;
                Avg_Actual_InqMql = Sum_Actual_InqMql / TotalRows;
                Avg_Projected_InqMql = Sum_Projected_InqMql / TotalRows;
                Avg_Mql = Sum_Mql / TotalRows;
                Avg_Cw = Sum_Cw / TotalRows;
                Avg_Actual_Ads = Sum_Actual_Ads / TotalRows;
                Avg_Projected_Ads = Sum_Projected_Ads / TotalRows;
                Avg_Actual_Rev = Sum_Actual_Rev / TotalRows;
                Avg_Projected_Rev = Sum_Projected_Rev / TotalRows;

                ConversionSummary BlendedAvg = new ConversionSummary();
                BlendedAvg = CustomSummaryRow(TotalRows + 1, "Blended Average", Avg_NewInq, Avg_Actual_InqMql, Avg_Projected_InqMql, Avg_Mql, Avg_Cw, Avg_Actual_Ads, Avg_Projected_Ads, Avg_Actual_Rev, Avg_Projected_Rev, 2);
                data.Add(BlendedAvg);

                ConversionSummary Total = new ConversionSummary();
                Total = CustomSummaryRow(TotalRows + 2, "Total", Sum_NewInq, Sum_Actual_InqMql, Sum_Projected_InqMql, Sum_Mql, Sum_Cw, Sum_Actual_Ads, Sum_Projected_Ads, Sum_Actual_Rev, Sum_Projected_Rev, 2);
                data.Add(Total);
            }
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This method generates the Conversion Summary JSON result row
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="TacticIds"></param>
        /// <param name="ModelFunnelIds"></param>
        /// <param name="type"></param>
        /// <param name="ObjectType"></param>
        /// <param name="ObjectId"></param>
        /// <returns></returns>
        private ConversionSummary GetConversionSummaryRow(int id, string title, List<int> TacticIds, List<int> ModelFunnelIds, string selectOption)
        {
            ConversionSummary cm = new ConversionSummary();
            cm.id = id;
            cm.title = title;

            double Projected_INQ = 0;
            double Actual_INQ = 0;

            double Projected_MQL = 0;
            double Actual_MQL = 0;

            double Actual_CW = 0;

            double Projected_ADS = 0;
            double Actual_ADS = 0;

            double Projected_INQ_MQL = 0;
            double Actual_INQ_MQL = 0;

            double Projected_Revenue = 0;
            double Actual_Revenue = 0;

            List<string> includeMonth = GetMonthList(selectOption, true);

            List<ProjectedRevenueClass> lstRev = ProjectedRevenueCalculate(TacticIds);
            List<TacticDataTable> tacticdata = (from t in db.Plan_Campaign_Program_Tactic.ToList()
                                                join l in lstRev on t.PlanTacticId equals l.PlanTacticId
                                                select new TacticDataTable
                                                {
                                                    TacticId = t.PlanTacticId,
                                                    StartMonth = t.StartDate.Month,
                                                    EndMonth = t.EndDate.Month,
                                                    Value = l.ProjectedRevenue,
                                                    StartYear = t.StartDate.Year,
                                                    EndYear = t.EndDate.Year
                                                }).ToList();

            Projected_Revenue = GetDatatable(tacticdata).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
            {
                PKey = g.Key,
                PSum = g.Sum(r => r.Field<double>(ColumnValue))
            }).Sum(p => p.PSum);

            foreach (var tactic in TacticIds)
            {
                var obj_tactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == tactic && pcpt.IsDeleted == false).FirstOrDefault();

                // get projected values
                Projected_INQ += obj_tactic.INQs;
                Projected_MQL += obj_tactic.MQLs;

                // get Actual values
                var lst_tactic_actuals = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpta => pcpta.PlanTacticId == tactic && includeMonth.Contains(pcpta.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pcpta.Period)).ToList();
                if (lst_tactic_actuals.Count > 0)
                {
                    foreach (var obj_tactic_actual in lst_tactic_actuals)
                    {
                        if (obj_tactic_actual.StageTitle == "INQ")
                        {
                            Actual_INQ += obj_tactic_actual.Actualvalue;
                        }
                        else if (obj_tactic_actual.StageTitle == "MQL")
                        {
                            Actual_MQL += obj_tactic_actual.Actualvalue;
                        }
                        else if (obj_tactic_actual.StageTitle == "CW")
                        {
                            Actual_CW += obj_tactic_actual.Actualvalue;
                        }
                        else if (obj_tactic_actual.StageTitle == "Revenue")
                        {
                            Actual_Revenue += obj_tactic_actual.Actualvalue;
                        }

                    }
                }
            }

            double ads = 0;
            var obj_ADS = db.Model_Funnel.Where(mf => ModelFunnelIds.Contains(mf.ModelFunnelId)).ToList();
            if (obj_ADS.Count > 0)
            {
                ads = obj_ADS.Sum(objads => objads.AverageDealSize);
            }

            Actual_ADS = ads;
            Projected_ADS = ads;

            /********* INQ data ***********/
            cm.newInq = Math.Round(Actual_INQ, 2);

            /********* INQ->MQL data **********/
            Actual_INQ_MQL = (Actual_MQL / (Actual_INQ == 0 ? 1 : Actual_INQ)) * 100; // to avoid can't divide by zero error
            Projected_INQ_MQL = (Projected_MQL / (Projected_INQ == 0 ? 1 : Projected_INQ)) * 100; // to avoid can't divide by zero error

            InqMql Actual_I_M = new InqMql();
            InqMql Projected_I_M = new InqMql();
            Actual_I_M.val = Math.Round(Actual_INQ_MQL, 2);
            Projected_I_M.val = Math.Round(Projected_INQ_MQL, 2);

            cm.inqMql = new List<InqMql>();

            cm.inqMql.Add(Actual_I_M);
            cm.inqMql.Add(Projected_I_M);

            /********* MQL, CW data *********/
            cm.mql = Math.Round(Actual_MQL, 2);
            cm.cw = Math.Round(Actual_CW, 2);

            /********** ADS data **********/
            Ad obj_Actual_ads = new Ad();
            Ad obj_Projected_ads = new Ad();
            obj_Actual_ads.val = Math.Round(Actual_ADS, 2);
            obj_Projected_ads.val = Math.Round(Projected_ADS, 2);

            cm.ads = new List<Ad>();

            cm.ads.Add(obj_Actual_ads);
            cm.ads.Add(obj_Projected_ads);

            /*********** REVENUE data *************/
            Revenue Actual_Rev = new Revenue();
            Revenue Projected_Rev = new Revenue();
            Actual_Rev.val = Math.Round(Actual_Revenue, 2);
            Projected_Rev.val = Math.Round(Projected_Revenue, 2);

            cm.revenue = new List<Revenue>();

            cm.revenue.Add(Actual_Rev);
            cm.revenue.Add(Projected_Rev);


            cm.type = 1;

            return cm;
        }

        /// <summary>
        /// This method will return MQL conversion summary JSON result for Total / Blended row
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="newInq"></param>
        /// <param name="actual_inqmql"></param>
        /// <param name="projected_inqmql"></param>
        /// <param name="mql"></param>
        /// <param name="cw"></param>
        /// <param name="actual_ads"></param>
        /// <param name="projected_ads"></param>
        /// <param name="actual_rev"></param>
        /// <param name="projected_rev"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private ConversionSummary CustomSummaryRow(int id, string title, double newInq, double actual_inqmql, double projected_inqmql, double mql, double cw, double actual_ads, double projected_ads, double actual_rev, double projected_rev, int type)
        {
            ConversionSummary cs = new ConversionSummary();
            cs.id = id;
            cs.title = title;
            cs.newInq = Math.Round(newInq, 2);

            InqMql InqMql_Actual = new InqMql();
            InqMql InqMql_Projected = new InqMql();

            InqMql_Actual.val = Math.Round(actual_inqmql);
            InqMql_Projected.val = Math.Round(projected_inqmql);

            cs.inqMql = new List<InqMql>();
            cs.inqMql.Add(InqMql_Actual);
            cs.inqMql.Add(InqMql_Projected);

            cs.mql = Math.Round(mql, 2);
            cs.cw = Math.Round(cw, 2);

            Ad Ads_Actual = new Ad();
            Ad Ads_Projected = new Ad();

            Ads_Actual.val = Math.Round(actual_ads, 2);
            Ads_Projected.val = Math.Round(projected_ads, 2);

            cs.ads = new List<Ad>();
            cs.ads.Add(Ads_Actual);
            cs.ads.Add(Ads_Projected);

            Revenue Revenue_Actual = new Revenue();
            Revenue Revenue_Projected = new Revenue();

            Revenue_Actual.val = Math.Round(actual_rev, 2);
            Revenue_Projected.val = Math.Round(projected_rev, 2);

            cs.revenue = new List<Revenue>();
            cs.revenue.Add(Revenue_Actual);
            cs.revenue.Add(Revenue_Projected);

            cs.type = type;

            return cs;
        }

        /// <summary>
        /// Returns the list of child tab for selected Master tab
        /// </summary>
        /// <returns></returns>
        public JsonResult GetChildTabForConversionSummary()
        {
            var returnData = db.Funnels.Where(fu => fu.IsDeleted == false).ToList().Select(f => new
            {
                id = f.FunnelId,
                title = f.Title
            }).Select(b => b).Distinct().OrderBy(b => b.id);
            return Json(returnData, JsonRequestBehavior.AllowGet);
        }

        #region Classes for Conversion Summary
        /// <summary>
        /// Classes for returning JSON data for Conversion Summary report
        /// </summary>
        public class InqMql
        {
            public double val { get; set; }
        }

        public class Ad
        {
            public double val { get; set; }
        }

        public class Revenue
        {
            public double val { get; set; }
        }

        public class ConversionSummary
        {
            public int id { get; set; }
            public string title { get; set; }
            public double newInq { get; set; }
            public List<InqMql> inqMql { get; set; }
            public double mql { get; set; }
            public double cw { get; set; }
            public List<Ad> ads { get; set; }
            public List<Revenue> revenue { get; set; }
            public int type { get; set; }
        }

        #endregion

        #endregion

        #endregion

        #region Revenue

        /// <summary>
        /// Revenue View.
        /// </summary>
        /// <returns></returns>
        [ActionName("Revenue")]
        public ActionResult LoadRevenue(string id = "thisyear")
        {
            if (Sessions.RolePermission != null)
            {
                Common.Permission permission = Common.GetPermission(ActionItem.Report);
                switch (permission)
                {
                    case Common.Permission.FullAccess:
                        break;
                    case Common.Permission.NoAccess:
                        return RedirectToAction("Index", "NoAccess");
                    case Common.Permission.NotAnEntity:
                        break;
                    case Common.Permission.ViewOnly:
                        ViewBag.IsViewOnly = "true";
                        break;
                }
            }

            ViewBag.MonthTitle = GetDisplayMonthList(id);
            ViewBag.SelectOption = id;

            ViewBag.BusinessUnit = db.BusinessUnits.Where(b => b.ClientId == Sessions.User.ClientId).OrderBy(b => b.Title);

            if (Sessions.ReportPlanId == 0)
            {
                ViewBag.PlanTitle = "All Plans";
            }
            else
            {
                var plan = db.Plans.Single(p => p.PlanId.Equals(Sessions.ReportPlanId));
                ViewBag.PlanTitle = plan.Title;
            }

            return View();
        }

        /// <summary>
        /// Get Child Tab Title As per Selection.
        /// </summary>
        /// <param name="ParentLabel">ParentLabel.</param>
        /// <returns>jsonResult</returns>
        public JsonResult GetChildLabelData(string ParentLabel, string selectOption = "thisyear")
        {
            if (ParentLabel == Common.RevenueBusinessUnit)
            {
                var returnData = (db.BusinessUnits.ToList().Where(bu => bu.ClientId.Equals(Sessions.User.ClientId) && bu.IsDeleted.Equals(false)).Select(bu => bu).ToList()).Select(b => new
                {
                    id = b.BusinessUnitId,
                    title = b.Title
                }).Select(b => b).Distinct().OrderBy(b => b.id);

                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel == Common.RevenueAudience)
            {
                var returnData = (db.Audiences.ToList().Where(au => au.ClientId.Equals(Sessions.User.ClientId) && au.IsDeleted.Equals(false)).Select(au => au).ToList()).Select(a => new
                {
                    id = a.AudienceId,
                    title = a.Title
                }).Select(a => a).Distinct().OrderBy(a => a.id);

                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel == Common.RevenueGeography)
            {
                var returnData = (db.Geographies.ToList().Where(ge => ge.ClientId.Equals(Sessions.User.ClientId) && ge.IsDeleted.Equals(false)).Select(ge => ge).ToList()).Select(g => new
                {
                    id = g.GeographyId,
                    title = g.Title
                }).Select(g => g).Distinct().OrderBy(g => g.id);

                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel == Common.RevenueVertical)
            {
                var returnData = (db.Verticals.ToList().Where(v => v.ClientId.Equals(Sessions.User.ClientId) && v.IsDeleted.Equals(false)).Select(v => v).ToList()).Select(v => new
                {
                    id = v.VerticalId,
                    title = v.Title
                }).Select(v => v).Distinct().OrderBy(v => v.id);

                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel == Common.RevenuePlans)
            {
                List<string> includeYearList = GetYearList(selectOption);
                var returnData = (db.Plans.ToList().Where(p => p.Model.BusinessUnit.ClientId.Equals(Sessions.User.ClientId) && p.IsDeleted.Equals(false) && p.Status == PublishedPlan && includeYearList.Contains(p.Year)).Select(p => p).ToList()).Select(p => new
                {
                    id = p.PlanId,
                    title = p.Title
                }).Select(b => b).Distinct().OrderBy(b => b.id);

                return Json(returnData, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        #region "Revenue Summary"

        /// <summary>
        /// Declare Class.
        /// Declare for Campaign and ite tactic id list.
        /// </summary>
        public class CampaignData
        {
            public int PlanCampaignId { get; set; }
            public string Title { get; set; }
            public List<int> planTacticList { get; set; }
        }

        /// <summary>
        /// Get Data For Revenue Summary Report.
        /// </summary>
        /// <param name="ParentLabel"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetRevenueSummaryData(string ParentLabel, string id, string selectOption)
        {
            List<string> includeYearList = GetYearList(selectOption);
            List<string> includeMonth = GetMonthList(selectOption);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            var campaignListobj = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId)).ToList().GroupBy(pc => new { PCid = pc.Plan_Campaign_Program.PlanCampaignId, title = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pc =>
                        new CampaignData
                        {
                            PlanCampaignId = pc.Key.PCid,
                            Title = pc.Key.title,
                            planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                        }).ToList();

            if (ParentLabel == Common.RevenueBusinessUnit)
            {
                Guid businessUnitid = new Guid(id);
                campaignListobj = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.BusinessUnitId == businessUnitid).ToList().GroupBy(pc => new { PCid = pc.Plan_Campaign_Program.PlanCampaignId, title = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pc =>
                                 new CampaignData
                                 {
                                     PlanCampaignId = pc.Key.PCid,
                                     Title = pc.Key.title,
                                     planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                                 }).ToList();
            }
            else if (ParentLabel == Common.RevenueGeography)
            {
                Guid geographyid = new Guid(id);
                campaignListobj = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.GeographyId == geographyid).ToList().GroupBy(pc => new { PCid = pc.Plan_Campaign_Program.PlanCampaignId, title = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pc =>
                             new CampaignData
                             {
                                 PlanCampaignId = pc.Key.PCid,
                                 Title = pc.Key.title,
                                 planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                             }).ToList();
            }
            else if (ParentLabel == Common.RevenuePlans)
            {
                int planId = Convert.ToInt32(id);
                campaignListobj = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).ToList().GroupBy(pc => new { PCid = pc.Plan_Campaign_Program.PlanCampaignId, title = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pc =>
                            new CampaignData
                            {
                                PlanCampaignId = pc.Key.PCid,
                                Title = pc.Key.title,
                                planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                            }).ToList();
            }
            var campaignList = campaignListobj.Select(p => new
            {
                id = p.PlanCampaignId,
                title = p.Title,
                monthList = includeMonth,
                trevenueProjected = GetProjectedRevenueData(p.planTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                tproject = GetProjectedMQLData(p.planTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                tRevenueActual = (db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => p.planTacticList.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString()))).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period).Select(pcptj => new
                {
                    key = pcptj.Key,
                    ActualValue = pcptj.Sum(pt => pt.Actualvalue)
                }),
                tacticActual = (db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => p.planTacticList.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()))).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period).Select(pcptj => new
                {
                    key = pcptj.Key,
                    ActualValue = pcptj.Sum(pt => pt.Actualvalue)
                }).Select(pcptj => pcptj),
            }).Select(p => p).Distinct().OrderBy(p => p.id);

            return Json(campaignList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Projected Mql Data With Month Wise.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetProjectedMQLData(List<int> planTacticList)
        {

            List<TacticDataTable> tacticdata = (from td in db.Plan_Campaign_Program_Tactic
                                                where planTacticList.Contains(td.PlanTacticId)
                                                select new TacticDataTable { TacticId = td.PlanTacticId, Value = td.MQLs, StartMonth = td.StartDate.Month, EndMonth = td.EndDate.Month, StartYear = td.StartDate.Year, EndYear = td.EndDate.Year }).ToList();

            return GetDatatable(tacticdata);
        }

        /// <summary>
        /// Get Projected Revenue Data & Calculation.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetProjectedRevenueData(List<int> planTacticList)
        {
            List<ProjectedRevenueClass> prlist = ProjectedRevenueCalculate(planTacticList).ToList();
            List<TacticDataTable> tacticdata = (from t in db.Plan_Campaign_Program_Tactic.ToList()
                                                join l in prlist on t.PlanTacticId equals l.PlanTacticId
                                                select new TacticDataTable
                                                {
                                                    TacticId = t.PlanTacticId,
                                                    StartMonth = t.StartDate.Month,
                                                    EndMonth = t.EndDate.Month,
                                                    Value = l.ProjectedRevenue,
                                                    StartYear = t.StartDate.Year,
                                                    EndYear = t.EndDate.Year
                                                }).ToList();

            return GetDatatable(tacticdata);
        }

        #endregion

        #region Revenue Realization

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Campaign List.
        /// </summary>
        /// <param name="id"> Id</param>
        /// <returns>Returns Json Result of Campaign List.</returns>
        public JsonResult LoadCampaignDropDown(Guid id, string selectOption = "thisyear")
        {
            List<string> includeYearList = GetYearList(selectOption);
            var campaign = db.Plan_Campaign.Where(pc => pc.Plan.Model.BusinessUnitId.Equals(id) && pc.Plan.IsDeleted == false && pc.Plan.Status == PublishedPlan && includeYearList.Contains(pc.Plan.Year) && pc.IsDeleted == false).OrderBy(pcp => pcp.Title);

            if (Sessions.ReportPlanId > 0)
            {
                campaign = db.Plan_Campaign.Where(pc => pc.Plan.Model.BusinessUnitId.Equals(id) && pc.PlanId == Sessions.ReportPlanId && pc.Plan.IsDeleted == false && pc.Plan.Status == PublishedPlan && includeYearList.Contains(pc.Plan.Year) && pc.IsDeleted == false).OrderBy(pcp => pcp.Title);
            }

            if (campaign == null)
                return Json(null);
            var campaignList = (from c in campaign
                                select new
                                {
                                    c.PlanCampaignId,
                                    c.Title
                                }
                               ).ToList();
            return Json(campaignList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Program List.
        /// </summary>
        /// <param name="id"> Id</param>
        /// <returns>Returns Json Result of Program List.</returns>
        public JsonResult LoadProgramDropDown(string id, string type = "", string selectOption = "thisyear")
        {
            List<string> includeYearList = GetYearList(selectOption);
            if (type == Common.RevenueBusinessUnit)
            {
                Guid businessunitid = new Guid(id);
                var program = db.Plan_Campaign_Program.Where(pc => pc.Plan_Campaign.Plan.Model.BusinessUnitId == businessunitid && pc.Plan_Campaign.Plan.IsDeleted == false && pc.IsDeleted == false && pc.Plan_Campaign.Plan.Status == PublishedPlan && includeYearList.Contains(pc.Plan_Campaign.Plan.Year)).OrderBy(pcp => pcp.Title);
                if (program == null)
                    return Json(null);
                var programList = (from c in program
                                   select new
                                   {
                                       c.PlanProgramId,
                                       c.Title
                                   }
                                   ).ToList();
                return Json(programList, JsonRequestBehavior.AllowGet);
            }
            int campaignid = Convert.ToInt32(id);
            var programout = db.Plan_Campaign_Program.Where(pc => pc.PlanCampaignId == campaignid && pc.IsDeleted == false).OrderBy(pcp => pcp.Title);
            if (programout == null)
                return Json(null);
            var programoutList = (from c in programout
                                  select new
                                  {
                                      c.PlanProgramId,
                                      c.Title
                                  }
                               ).ToList();
            return Json(programoutList, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Tactic List.
        /// </summary>
        /// <param name="id"> Id</param>
        /// <returns>Returns Json Result of Tactic List.</returns>
        public JsonResult LoadTacticDropDown(string id, string type = "", string selectOption = "thisyear")
        {
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            List<string> includeYearList = GetYearList(selectOption);
            if (type == Common.RevenueBusinessUnit)
            {
                Guid businessunitid = new Guid(id);
                var tactic = db.Plan_Campaign_Program_Tactic.Where(pc => pc.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId == businessunitid && tacticStatus.Contains(pc.Status) && pc.IsDeleted == false && pc.Plan_Campaign_Program.Plan_Campaign.Plan.IsDeleted == false && pc.Plan_Campaign_Program.Plan_Campaign.Plan.Status == PublishedPlan && includeYearList.Contains(pc.Plan_Campaign_Program.Plan_Campaign.Plan.Year)).OrderBy(pcp => pcp.Title);
                if (tactic == null)
                    return Json(null);
                var tacticList = (from c in tactic
                                  select new
                                  {
                                      c.PlanTacticId,
                                      c.Title
                                  }
                                   ).ToList();
                return Json(tacticList, JsonRequestBehavior.AllowGet);
            }
            else if (type == Common.RevenueCampaign)
            {
                int campaignid = Convert.ToInt32(id);
                var tactic = db.Plan_Campaign_Program_Tactic.Where(pc => pc.Plan_Campaign_Program.PlanCampaignId == campaignid && tacticStatus.Contains(pc.Status) && pc.IsDeleted == false).OrderBy(pcp => pcp.Title);
                if (tactic == null)
                    return Json(null);
                var tacticList = (from c in tactic
                                  select new
                                  {
                                      c.PlanTacticId,
                                      c.Title
                                  }
                                   ).ToList();
                return Json(tacticList, JsonRequestBehavior.AllowGet);
            }

            int programid = Convert.ToInt32(id);
            var tacticout = db.Plan_Campaign_Program_Tactic.Where(pc => pc.PlanProgramId == programid && tacticStatus.Contains(pc.Status) && pc.IsDeleted == false).OrderBy(pcp => pcp.Title);
            if (tacticout == null)
                return Json(null);
            var tacticoutList = (from c in tacticout
                                 select new
                                 {
                                     c.PlanTacticId,
                                     c.Title
                                 }
                               ).ToList();
            return Json(tacticoutList, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Load Revenue Realization Grid.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public JsonResult LoadRevenueRealization(string id, string businessUnitId, string type = "", string selectOption = "thisyear")
        {
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            List<string> includeYearList = GetYearList(selectOption);
            List<string> includeMonth = GetMonthList(selectOption);
            List<int> rrTacticList = new List<int>();
            List<int> tacticIds = GetTacticForReport(includeYearList);
            Guid BusinessUnitid = new Guid(businessUnitId);
            if (type == Common.RevenueBusinessUnit)
            {
                Guid buid = new Guid(id);
                rrTacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.BusinessUnitId == buid).Select(t => t.PlanTacticId).ToList();
            }
            else if (type == Common.RevenueCampaign)
            {
                int campaignid = Convert.ToInt32(id);
                rrTacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.Plan_Campaign_Program.PlanCampaignId == campaignid).Select(t => t.PlanTacticId).ToList();
            }
            else if (type == Common.RevenueProgram)
            {
                int programid = Convert.ToInt32(id);
                rrTacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.PlanProgramId == programid).Select(t => t.PlanTacticId).ToList();
            }
            else if (type == Common.RevenueTactic)
            {
                int tacticid = Convert.ToInt32(id);
                rrTacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.PlanTacticId == tacticid).Select(t => t.PlanTacticId).ToList();
            }

            if (rrTacticList.Count() > 0)
            {
                var rdata = new[] { new { 
                INQGoal = GetProjectedINQData(rrTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                monthList = includeMonth,
                INQActual = (db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => rrTacticList.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.INQ.ToString()].ToString()))).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period).Select(pcptj => new
                {
                    PKey = pcptj.Key,
                    PSum = pcptj.Sum(pt => pt.Actualvalue)
                }),
                MQLGoal = GetProjectedMQLDataRealization(rrTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                MQLActual = (db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => rrTacticList.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()))).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period).Select(pcptj => new
                {
                    PKey = pcptj.Key,
                    PSum = pcptj.Sum(pt => pt.Actualvalue)
                }),
                 RevenueGoal = (db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => rrTacticList.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString()))).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period).Select(pcptj => new
                {
                    PKey = pcptj.Key,
                    PSum = pcptj.Sum(pt => pt.Actualvalue)
                })
                //RevenueGoal = GetPRevenueData(rrTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                //{
                //    PKey = g.Key,
                //    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                //})
            }  };

                return Json(rdata, JsonRequestBehavior.AllowGet);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Projected INQ Data With Month Wise.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetProjectedINQData(List<int> tlist)
        {
            string stageINQ = Enums.Stage.INQ.ToString();
            int levelINQ = db.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageINQ)).Level.Value;
            string stageTypeSV = Enums.StageType.SV.ToString();
            List<TacticModelRelation> tacticModelList = GetTacticModelRelation(tlist);
            List<TacticDataTable> tacticdata = (from tactic in db.Plan_Campaign_Program_Tactic.ToList()
                                                join t in tacticModelList on tactic.PlanTacticId equals t.PlanTacticId
                                                join modelFunnelStage in db.Model_Funnel_Stage on t.ModelId equals modelFunnelStage.Model_Funnel.ModelId
                                                join stage in db.Stages on modelFunnelStage.StageId equals stage.StageId
                                                where tlist.Contains(tactic.PlanTacticId) &&
                                                                modelFunnelStage.StageType.Equals(stageTypeSV) &&
                                                                stage.ClientId.Equals(Sessions.User.ClientId) &&
                                                                stage.Level < levelINQ
                                                select new
                                                {
                                                    PlanTacticId = tactic.PlanTacticId,
                                                    INQs = tactic.INQs,
                                                    value = modelFunnelStage.Value,
                                                    startDate = tactic.StartDate,
                                                    endDate = tactic.EndDate,
                                                    year = tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year,
                                                    m = modelFunnelStage.Model_Funnel.ModelId
                                                }).GroupBy(rl => new { id = rl.PlanTacticId, INQ = rl.INQs, sDate = rl.startDate, eDate = rl.endDate, year = rl.year }).ToList().Select(r => new
                                                {
                                                    id = r.Key.id,
                                                    INQ = r.Key.INQ,
                                                    sDate = r.Key.sDate,
                                                    eDate = r.Key.eDate,
                                                    year = r.Key.year,
                                                    value = r.Sum(t => t.value)
                                                }).Select(r => new TacticDataTable { TacticId = r.id, Value = r.INQ, StartMonth = r.sDate.AddDays(r.value).Month, EndMonth = r.eDate.AddDays(r.value).Month, StartYear = r.sDate.AddDays(r.value).Year, EndYear = r.eDate.AddDays(r.value).Year }).ToList();

            return GetDatatable(tacticdata);
        }

        /// <summary>
        /// Get Projected MQL Data With Month Wise.
        /// Modified By: Maninder Singh Wadhva to address TFS Bug#280 :Error Message Showing when editing a tactic - Preventing MQLs from updating
        /// Modified By: Maninder Singh Wadhva 1-March-2014 to address TFS Bug#322 : Changes made to INQ, MQL and Projected Revenue Calculation.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetProjectedMQLDataRealization(List<int> tlist)
        {
            string stageINQ = Enums.Stage.INQ.ToString();
            int levelINQ = db.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageINQ)).Level.Value;
            string stageMQL = Enums.Stage.MQL.ToString();
            int levelMQL = db.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageMQL)).Level.Value;
            string stageTypeSV = Enums.StageType.SV.ToString();
            List<TacticModelRelation> tacticModelList = GetTacticModelRelation(tlist);
            List<TacticDataTable> tacticdata = (from tactic in db.Plan_Campaign_Program_Tactic.ToList()
                                                join t in tacticModelList on tactic.PlanTacticId equals t.PlanTacticId
                                                join modelFunnelStage in db.Model_Funnel_Stage on t.ModelId equals modelFunnelStage.Model_Funnel.ModelId
                                                join stage in db.Stages on modelFunnelStage.StageId equals stage.StageId
                                                where tlist.Contains(tactic.PlanTacticId) &&
                                                                modelFunnelStage.StageType.Equals(stageTypeSV) &&
                                                                stage.ClientId.Equals(Sessions.User.ClientId) &&
                                                                stage.Level >= levelINQ && stage.Level < levelMQL
                                                select new
                                                {
                                                    PlanTacticId = tactic.PlanTacticId,
                                                    MQLs = tactic.MQLs,
                                                    value = modelFunnelStage.Value,
                                                    startDate = tactic.StartDate,
                                                    endDate = tactic.EndDate,
                                                    year = tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year,
                                                    m = modelFunnelStage.Model_Funnel.ModelId
                                                }).GroupBy(rl => new { id = rl.PlanTacticId, MQL = rl.MQLs, sDate = rl.startDate, eDate = rl.endDate, year = rl.year }).ToList().Select(r => new
                                                {
                                                    id = r.Key.id,
                                                    MQL = r.Key.MQL,
                                                    sDate = r.Key.sDate,
                                                    eDate = r.Key.eDate,
                                                    year = r.Key.year,
                                                    value = r.Sum(t => t.value)
                                                }).Select(r => new TacticDataTable { TacticId = r.id, Value = r.MQL, StartMonth = r.sDate.AddDays(r.value).Month, EndMonth = r.eDate.AddDays(r.value).Month, StartYear = r.sDate.AddDays(r.value).Year, EndYear = r.eDate.AddDays(r.value).Year }).ToList();

            return GetDatatable(tacticdata);
        }

        //// Modified By: Maninder Singh Wadhva 1-March-2014 to address TFS Bug#322 : Changes made to INQ, MQL and Projected Revenue Calculation.
        ///// <summary>
        ///// Get Projected Revenue Data & Calculation.
        ///// Modified By: Maninder Singh Wadhva to address TFS Bug#280 :Error Message Showing when editing a tactic - Preventing MQLs from updating
        ///// </summary>
        ///// <param name="cl"></param>
        ///// <returns></returns>
        //public DataTable GetPRevenueData(List<int> tlist)
        //{
        //    string stageMQL = Enums.Stage.MQL.ToString();
        //    int levelMQL = db.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageMQL)).Level.Value;
        //    string stageTypeSV = Enums.StageType.SV.ToString();
        //    List<TacticModelRelation> tacticModelList = GetTacticModelRelation(tlist);
        //    var revenuelist = from tactic in db.Plan_Campaign_Program_Tactic.ToList()
        //                      join t in tacticModelList on tactic.PlanTacticId equals t.PlanTacticId
        //                      join modelFunnelStage in db.Model_Funnel_Stage on t.ModelId equals modelFunnelStage.Model_Funnel.ModelId
        //                      join stage in db.Stages on modelFunnelStage.StageId equals stage.StageId
        //                      where tlist.Contains(tactic.PlanTacticId) &&
        //                          modelFunnelStage.StageType.Equals(stageTypeSV) &&
        //                                      stage.ClientId.Equals(Sessions.User.ClientId) &&
        //                                      stage.Level >= levelMQL
        //                      select new
        //                      {
        //                          PlanTacticId = tactic.PlanTacticId,
        //                          Value = (double)modelFunnelStage.Value,
        //                      };

        //    var cprSV = revenuelist.GroupBy(rl => new { id = rl.PlanTacticId }).ToList().Select(r => new
        //    {
        //        id = r.Key.id,
        //        value = r.Sum(t => t.Value)
        //    }).Select(r => new { id = r.id, value = r.value });

        //    List<ProjectedRevenueClass> prlist = ProjectedRevenueCalculate(tlist).ToList();
        //    List<TacticDataTable> tacticdata = (from t in db.Plan_Campaign_Program_Tactic.ToList()
        //                                        join l in prlist on t.PlanTacticId equals l.PlanTacticId
        //                                        join c in cprSV on t.PlanTacticId equals c.id
        //                                        select new TacticDataTable
        //                                        {
        //                                            TacticId = t.PlanTacticId,
        //                                            StartMonth = t.StartDate.AddDays(c.value).Month,
        //                                            EndMonth = t.EndDate.AddDays(c.value).Month,
        //                                            Value = l.ProjectedRevenue,
        //                                            StartYear = t.StartDate.AddDays(c.value).Year,
        //                                            EndYear = t.EndDate.AddDays(c.value).Year
        //                                        }).ToList();

        //    return GetDatatable(tacticdata);
        //}

        #endregion

        #region Revenue Contribution

        /// <summary>
        /// Declare Class.
        /// </summary>
        public class RevenueContrinutionData
        {
            public string Title { get; set; }
            public List<int> planTacticList { get; set; }
        }

        /// <summary>
        /// Load Revenue Contribution.
        /// </summary>
        /// <param name="parentlabel"></param>
        /// <param name="businessUnitId"></param>
        /// <param name="isBusinessUnit"></param>
        /// <returns></returns>
        public JsonResult LoadRevenueContribution(string parentlabel, string businessUnitId, bool isBusinessUnit, string selectOption)
        {
            List<string> includeYearList = GetYearList(selectOption, true);
            List<string> includeMonth = GetMonthList(selectOption, true);
            int lastMonth = GetLastMonthForTrend(selectOption);
            Guid buid = new Guid();
            if (isBusinessUnit)
            {
                if (!string.IsNullOrEmpty(businessUnitId))
                {
                    buid = new Guid(businessUnitId);
                }
                else if (Sessions.BusinessUnitId != Guid.Empty)
                {
                    buid = Sessions.BusinessUnitId;
                }

            }
            List<int> tacticIds = GetTacticForReport(includeYearList);
            var campaignListobj = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) &&
                ((parentlabel == Common.RevenueBusinessUnit && pcpt.BusinessUnit.ClientId == Sessions.User.ClientId) ||
                (parentlabel == Common.RevenueAudience && pcpt.Audience.ClientId == Sessions.User.ClientId) ||
                (parentlabel == Common.RevenueGeography && pcpt.Geography.ClientId == Sessions.User.ClientId) ||
                (parentlabel == Common.RevenueVertical && pcpt.Vertical.ClientId == Sessions.User.ClientId) ||
                (parentlabel == Common.RevenueCampaign))
                ).ToList();
            if (isBusinessUnit)
            {
                campaignListobj = campaignListobj.Where(c => c.BusinessUnitId == buid).ToList();
            }
            var campaignList = campaignListobj.GroupBy(pc => new { title = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pc =>
                     new RevenueContrinutionData
                     {
                         Title = pc.Key.title,
                         planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                     }).ToList();

            if (parentlabel == Common.RevenueVertical)
            {
                campaignList = campaignListobj.GroupBy(pc => new { title = pc.Vertical.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                         }).ToList();
            }
            else if (parentlabel == Common.RevenueGeography)
            {
                campaignList = campaignListobj.GroupBy(pc => new { title = pc.Geography.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                         }).ToList();
            }
            else if (parentlabel == Common.RevenueAudience)
            {
                campaignList = campaignListobj.GroupBy(pc => new { title = pc.Audience.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                         }).ToList();
            }
            else if (parentlabel == Common.RevenueBusinessUnit)
            {
                campaignList = campaignListobj.GroupBy(pc => new { title = pc.BusinessUnit.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                         }).ToList();
            }

            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            var campaignListFinal = campaignList.Select(p => new
            {
                Title = p.Title,
                PlanRevenue = GetProjectedRevenueData(p.planTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                ActualRevenue = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => p.planTacticList.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue),
                TrendRevenue = GetTrendRevenueDataContribution(p.planTacticList, lastMonth),
                PlanCost = GetProjectedCostData(p.planTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                ActualCost = GetProjectedCostData(p.planTacticList, false).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                TrendCost = GetTrendCostDataContribution(p.planTacticList, lastMonth),
                RunRate = GetTrendRevenueDataContribution(p.planTacticList, lastMonth),
                PipelineCoverage = GetPipelineCoverage(p.planTacticList, lastMonth),
                RevSpend = GetRevenueVSSpendContribution(p.planTacticList),
                RevenueTotal = GetActualRevenueTotal(p.planTacticList),
                CostTotal = GetActualCostTotal(p.planTacticList),
            }).Select(p => p).Distinct().OrderBy(p => p.Title);

            return Json(campaignListFinal, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Projected Cost Data With Month Wise.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetProjectedCostData(List<int> planTacticList, bool isprojected = true)
        {

            List<TacticDataTable> tacticdata = (from td in db.Plan_Campaign_Program_Tactic
                                                where planTacticList.Contains(td.PlanTacticId)
                                                select new TacticDataTable { Value = isprojected ? td.Cost : td.CostActual.HasValue ? (double)td.CostActual : 0, StartMonth = td.StartDate.Month, EndMonth = td.EndDate.Month, StartYear = td.StartDate.Year, EndYear = td.EndDate.Year }).ToList();
            return GetDatatable(tacticdata);
        }

        /// <summary>
        /// Get Projected Revenue Data & Calculation.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public double GetProjectedRevenueDataContribution(List<int> planTacticList)
        {

            List<ProjectedRevenueClass> prlist = ProjectedRevenueCalculate(planTacticList).ToList();

            double projectedRevenue = 0;
            projectedRevenue = prlist.Sum(cp => cp.ProjectedRevenue);
            return projectedRevenue;
        }

        /// <summary>
        /// Get Trend.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public double GetTrendRevenueDataContribution(List<int> planTacticList, int lastMonth)
        {
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            List<string> monthList = GetUpToCurrentMonth();

            var actualList = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => planTacticList.Contains(ta.PlanTacticId) && monthList.Contains(ta.Period) && ta.StageTitle == revenue).ToList().GroupBy(t => t.PlanTacticId).Select(pt => new
            {
                id = pt.Key,
                trendValue = (pt.Sum(a => a.Actualvalue) / currentMonth) * lastMonth
            });

            double trendRevenue = 0;
            trendRevenue = actualList.Sum(a => a.trendValue);
            return trendRevenue;
        }

        /// <summary>
        /// Get Trend.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public double GetTrendCostDataContribution(List<int> planTacticList, int lastMonth)
        {
            List<TacticDataTable> tacticdata = (from td in db.Plan_Campaign_Program_Tactic
                                                where planTacticList.Contains(td.PlanTacticId)
                                                select new TacticDataTable { TacticId = td.PlanTacticId, Value = td.CostActual.HasValue ? (double)td.CostActual : 0, StartMonth = td.StartDate.Month, EndMonth = td.EndDate.Month, StartYear = td.StartDate.Year, EndYear = td.EndDate.Year }).ToList();

            List<string> monthList = GetUpToCurrentMonth();

            var costTrenList = GetDatatable(tacticdata).AsEnumerable().AsQueryable().Where(c => monthList.Contains(c.Field<string>(ColumnMonth))).GroupBy(r => r.Field<int>(ColumnId)).Select(g => new
            {
                id = g.Key,
                trendValue = (g.Sum(r => r.Field<double>(ColumnValue)) / currentMonth) * lastMonth
            });

            double trendCost = 0;
            trendCost = costTrenList.Sum(a => a.trendValue);
            return trendCost;
        }

        /// <summary>
        /// Get Trend.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public double GetRevenueVSSpendContribution(List<int> planTacticList)
        {
            List<TacticDataTable> tacticdata = (from td in db.Plan_Campaign_Program_Tactic
                                                where planTacticList.Contains(td.PlanTacticId)
                                                select new TacticDataTable { TacticId = td.PlanTacticId, Value = td.CostActual.HasValue ? (double)td.CostActual : 0, StartMonth = td.StartDate.Month, EndMonth = td.EndDate.Month, StartYear = td.StartDate.Year, EndYear = td.EndDate.Year }).ToList();

            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            List<string> monthList = GetUpToCurrentMonth();

            double costTotal = GetDatatable(tacticdata).AsEnumerable().AsQueryable().Where(c => monthList.Contains(c.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));

            double revenueTotal = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => planTacticList.Contains(ta.PlanTacticId) && monthList.Contains(ta.Period) && ta.StageTitle == revenue).ToList().Sum(a => a.Actualvalue);

            double RevenueSpend = 0;
            if (costTotal != 0)
            {
                RevenueSpend = ((revenueTotal - costTotal) / costTotal);
            }
            return RevenueSpend;
        }

        /// <summary>
        /// Get Revenue Total.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public double GetActualRevenueTotal(List<int> planTacticList)
        {
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            List<string> monthList = GetUpToCurrentMonth();

            double revenueTotal = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => planTacticList.Contains(ta.PlanTacticId) && monthList.Contains(ta.Period) && ta.StageTitle == revenue).ToList().Sum(a => a.Actualvalue);

            return revenueTotal;
        }

        /// <summary>
        /// Get Pipeline Coverage.
        /// Modified By: Maninder Singh Wadhva to address TFS Bug#280 :Error Message Showing when editing a tactic - Preventing MQLs from updating
        /// Modified By: Maninder Singh Wadhva 1-March-2014 to address TFS Bug#322 : Changes made to INQ, MQL and Projected Revenue Calculation.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public double GetPipelineCoverage(List<int> planTacticList, int lastMonth)
        {
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            string inq = Enums.InspectStageValues[Enums.InspectStage.INQ.ToString()].ToString();
            string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            List<string> monthList = GetUpToCurrentMonth();
            List<TacticModelRelation> tacticModelList = GetTacticModelRelation(planTacticList);
            double revenueTotal = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => planTacticList.Contains(ta.PlanTacticId) && monthList.Contains(ta.Period) && ta.StageTitle == revenue).ToList().Sum(a => a.Actualvalue);

            string stageINQ = Enums.Stage.INQ.ToString();
            int levelINQ = db.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageINQ)).Level.Value;
            string stageTypeCR = Enums.StageType.CR.ToString();

            var inqlist = (from tactic in db.Plan_Campaign_Program_Tactic.ToList()
                           join t in tacticModelList on tactic.PlanTacticId equals t.PlanTacticId
                           join modelFunnelStage in db.Model_Funnel_Stage on t.ModelId equals modelFunnelStage.Model_Funnel.ModelId
                           join stage in db.Stages on modelFunnelStage.StageId equals stage.StageId
                           where planTacticList.Contains(tactic.PlanTacticId) &&
                                           modelFunnelStage.StageType.Equals(stageTypeCR) &&
                                           stage.ClientId.Equals(Sessions.User.ClientId) &&
                                           stage.Level < levelINQ && modelFunnelStage.Value != 0
                           select new
                           {
                               PlanTacticId = tactic.PlanTacticId,
                               INQs = tactic.Plan_Campaign_Program_Tactic_Actual.Where(pta => monthList.Contains(pta.Period) && pta.StageTitle == inq).Sum(pta => pta.Actualvalue),
                               ADS = modelFunnelStage.Model_Funnel.AverageDealSize,
                               ModelFunnelId = modelFunnelStage.ModelFunnelId,
                               value = modelFunnelStage.Value,
                           }).Where(ta => ta.INQs != null).GroupBy(rl => new { id = rl.PlanTacticId, INQ = rl.INQs, ModelFunnelId = rl.ModelFunnelId, ADS = rl.ADS }).ToList().Select(r => new
                           {
                               id = r.Key.id,
                               INQ = r.Key.INQ,
                               ADS = r.Key.ADS,
                               value = (r.Aggregate(1.0, (s1, s2) => s1 * (s2.value / 100)))
                           }).Select(r => new { id = r.id, value = r.value * r.INQ * r.ADS });

            double inqTotal = inqlist.Sum(s => s.value);

            string stageMQL = Enums.Stage.MQL.ToString();
            int levelMQL = db.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageMQL)).Level.Value;

            var mqllist = (from tactic in db.Plan_Campaign_Program_Tactic.ToList()
                           join t in tacticModelList on tactic.PlanTacticId equals t.PlanTacticId
                           join modelFunnelStage in db.Model_Funnel_Stage on t.ModelId equals modelFunnelStage.Model_Funnel.ModelId
                           join stage in db.Stages on modelFunnelStage.StageId equals stage.StageId
                           where planTacticList.Contains(tactic.PlanTacticId) &&
                                           modelFunnelStage.StageType.Equals(stageTypeCR) &&
                                           stage.ClientId.Equals(Sessions.User.ClientId) &&
                                           stage.Level >= levelINQ && stage.Level < levelMQL && modelFunnelStage.Value != 0
                           select new
                           {
                               PlanTacticId = tactic.PlanTacticId,
                               MQLs = tactic.Plan_Campaign_Program_Tactic_Actual.Where(pta => monthList.Contains(pta.Period) && pta.StageTitle == mql).Sum(pta => pta.Actualvalue),
                               ADS = modelFunnelStage.Model_Funnel.AverageDealSize,
                               ModelFunnelId = modelFunnelStage.ModelFunnelId,
                               value = modelFunnelStage.Value,
                           }).Where(ta => ta.MQLs != null).GroupBy(rl => new { id = rl.PlanTacticId, MQL = rl.MQLs, ModelFunnelId = rl.ModelFunnelId, ADS = rl.ADS }).ToList().Select(r => new
                           {
                               id = r.Key.id,
                               MQL = r.Key.MQL,
                               ADS = r.Key.ADS,
                               value = (r.Aggregate(1.0, (s1, s2) => s1 * (s2.value / 100)))
                           }).Select(r => new { id = r.id, MQL = r.MQL * r.value * r.ADS });

            double mqlTotal = mqllist.Sum(s => s.MQL);

            List<ProjectedRevenueClass> prlist = ProjectedRevenueCalculate(planTacticList).ToList();
            List<TacticDataTable> tacticdata = (from t in db.Plan_Campaign_Program_Tactic.ToList()
                                                join l in prlist on t.PlanTacticId equals l.PlanTacticId
                                                select new TacticDataTable
                                                {
                                                    TacticId = t.PlanTacticId,
                                                    StartMonth = t.StartDate.Month,
                                                    EndMonth = t.EndDate.Month,
                                                    Value = l.ProjectedRevenue,
                                                    StartYear = t.StartDate.Year,
                                                    EndYear = t.EndDate.Year
                                                }).ToList();


            List<string> monthLastList = new List<string>();
            for (int i = currentMonth + 1; i <= lastMonth; i++)
            {
                monthLastList.Add(PeriodPrefix + i);
            }

            double projectedRevenueTotal = 0;
            projectedRevenueTotal = GetDatatable(tacticdata).AsEnumerable().AsQueryable().Where(c => monthLastList.Contains(c.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));

            return revenueTotal + inqTotal + mqlTotal + projectedRevenueTotal;
        }

        /// <summary>
        /// Get Cost Total.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public double GetActualCostTotal(List<int> planTacticList)
        {
            List<TacticDataTable> tacticdata = (from td in db.Plan_Campaign_Program_Tactic
                                                where planTacticList.Contains(td.PlanTacticId)
                                                select new TacticDataTable { TacticId = td.PlanTacticId, Value = td.CostActual.HasValue ? (double)td.CostActual : 0, StartMonth = td.StartDate.Month, EndMonth = td.EndDate.Month, StartYear = td.StartDate.Year, EndYear = td.EndDate.Year }).ToList();

            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            List<string> monthList = GetUpToCurrentMonth();

            double costTotal = GetDatatable(tacticdata).AsEnumerable().AsQueryable().Where(c => monthList.Contains(c.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));
            return costTotal;
        }

        #endregion

        #region "Revenue to Plan"

        /// <summary>
        /// Function to get data for revenue to plan report.
        /// </summary>
        /// <param name="ParentLabel">Filter name.</param>
        /// <param name="id">GUID for filtering data.</param>
        /// <returns>Returns json data for revenue to plan report.</returns>
        public JsonResult GetRevenueToPlan(string ParentLabel, string id, string selectOption = "thisyear", string originalOption = "thisyear")
        {
            List<string> includeYearList = GetYearList(selectOption);
            List<string> includeMonth = GetMonthList(selectOption);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            if (tacticIds.Count > 0)
            {
                DataTable dtActualRevenue = GetActualRevenue(tacticIds, ParentLabel, id);
                DataTable dtProjectedRevenue = GetProjectedRevenue(tacticIds, ParentLabel, id);
                DataTable dtDifference = GetDifference(dtProjectedRevenue, dtActualRevenue);
                DataTable dtRevenueTrend = GetRevenueTrend(dtActualRevenue, originalOption);
                DataTable dtContribution = GetContribution(dtActualRevenue, dtRevenueTrend);
                DataTable dtTotalRevenue = GetTotalRevenue(dtActualRevenue);
                DataTable dtChartData = GetChartData(dtActualRevenue, dtProjectedRevenue, dtContribution);

                return Json(new
                {
                    projectedRevenue = dtProjectedRevenue.AsEnumerable().Select(pr => new { Month = pr.Field<int>(ColumnMonth), Value = pr.Field<double?>(ColumnValue) }),
                    actualRevenue = dtActualRevenue.AsEnumerable().Select(ar => new { Month = ar.Field<int>(ColumnMonth), Value = ar.Field<double?>(ColumnValue) }),
                    difference = dtDifference.AsEnumerable().Select(d => new { Month = d.Field<int>(ColumnMonth), Value = d.Field<double?>(ColumnValue) }),
                    contribution = dtContribution.AsEnumerable().Select(c => new { Month = c.Field<int>(ColumnMonth), Value = c.Field<double?>(ColumnValue) }),
                    revenueTrend = dtRevenueTrend.AsEnumerable().Select(rt => new { Month = rt.Field<int>(ColumnMonth), Value = rt.Field<double?>(ColumnValue) }),
                    totalRevenue = dtTotalRevenue.AsEnumerable().Select(tr => new { Month = tr.Field<int>(ColumnMonth), Value = tr.Field<double?>(ColumnValue) }),
                    chartData = dtChartData.AsEnumerable().Select(cd => new
                    {
                        Month = GetAbbreviatedMonth(cd.Field<int>(ColumnMonth)),
                        Actual = cd.Field<double?>("Actual"),
                        Projected = cd.Field<double?>("Projected"),
                        Contribution = cd.Field<double?>("Contribution"),
                        ColorActual = "#d4d4d4",
                        ColorProjected = "#1a638a",
                        ColorContibution = "#559659"
                    }),
                }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Function to get abberiviation like Jan, Feb, Mar,....,Dec.
        /// </summary>
        /// <param name="month">Numeric Month.</param>
        /// <returns>Returns abberiviated month.</returns>
        private string GetAbbreviatedMonth(int month)
        {
            DateTimeFormatInfo dtfi = new DateTimeFormatInfo();
            return dtfi.GetAbbreviatedMonthName(month);
        }

        /// <summary>
        /// Function to get data for chart.
        /// </summary>
        /// <param name="dtActualRevenue">Datatable containing actual revenue.</param>
        /// <param name="dtProjectedRevenue">Datatable containing projected revenue.</param>
        /// <param name="dtContribution">Datatable containing contribution.</param>
        /// <returns>Returns datatable containing projected/actual/contribution for each month.</returns>
        private DataTable GetChartData(DataTable dtActualRevenue, DataTable dtProjectedRevenue, DataTable dtContribution)
        {
            DataTable dtChartData = new DataTable();
            dtChartData.Columns.Add(ColumnMonth, typeof(int));
            dtChartData.Columns.Add("Actual", typeof(double));
            dtChartData.Columns.Add("Projected", typeof(double));
            dtChartData.Columns.Add("Contribution", typeof(double));

            for (int month = 1; month <= 12; month++)
            {
                DataRow drActualRevenue = dtActualRevenue.Select(string.Format("{0}={1}", ColumnMonth, month)).FirstOrDefault();
                DataRow drProjectedRevenue = dtProjectedRevenue.Select(string.Format("{0}={1}", ColumnMonth, month)).FirstOrDefault();
                DataRow drContribution = dtContribution.Select(string.Format("{0}={1}", ColumnMonth, month)).FirstOrDefault();
                double? actualRevenue = 0;
                if (!string.IsNullOrWhiteSpace(Convert.ToString(drActualRevenue[ColumnValue])))
                {
                    actualRevenue = (double)drActualRevenue[ColumnValue];
                }

                double? projectedRevenue = 0;
                if (!string.IsNullOrWhiteSpace(Convert.ToString(drProjectedRevenue[ColumnValue])))
                {
                    projectedRevenue = (double)drProjectedRevenue[ColumnValue];
                }

                double? contribution = 0;
                if (!string.IsNullOrWhiteSpace(Convert.ToString(drContribution[ColumnValue])))
                {
                    contribution = (double)drContribution["Contribution"];
                }

                //// Modified By: Maninder Singh Wadhva Bug 298:Revenue to plan graph is incorrect.
                dtChartData.Rows.Add(month, Math.Round(Convert.ToDouble(actualRevenue), 2), Math.Round(Convert.ToDouble(projectedRevenue), 2), Math.Round(Convert.ToDouble(contribution), 2));
            }

            return dtChartData;
        }

        /// <summary>
        /// Function to get total revenue.
        /// </summary>
        /// <param name="dtActualRevenue">Datatable containing actual revenue.</param>
        /// <returns>Returns datatable containing total revenue for each month.</returns>
        private DataTable GetTotalRevenue(DataTable dtActualRevenue)
        {
            DataTable dtTotalRevenue = GetDataTableMonthValue();
            foreach (DataRow drTotalRevenue in dtTotalRevenue.Rows)
            {
                //// Getting actual revenue of month from datatable.
                var drActualRevenue = dtActualRevenue.Select(string.Format("{0}<={1}", ColumnMonth, drTotalRevenue[ColumnMonth]));

                //// Setting contribution value.
                drTotalRevenue[ColumnValue] = drActualRevenue.Sum(ar => ar.Field<double?>(ColumnValue));
            }

            return dtTotalRevenue;
        }

        /// <summary>
        /// Function to get contribution.
        /// </summary>
        /// <param name="dtActualRevenue">Datatable containing actual revenue.</param>
        /// <param name="dtRevenueTrend">Datatable containing revenue trend.</param>
        /// <returns>Returns datatable containing contribution for each month.</returns>
        private DataTable GetContribution(DataTable dtActualRevenue, DataTable dtRevenueTrend)
        {
            DataTable dtContribution = GetDataTableMonthValue();
            dtContribution.Columns.Add("Contribution", typeof(double));

            foreach (DataRow drContribution in dtContribution.Rows)
            {
                //// Getting actual revenue of month from datatable.
                var drActualRevenue = dtActualRevenue.Select(string.Format("{0}={1}", ColumnMonth, drContribution[ColumnMonth])).First();

                //// Getting revenue trend of month from datatable.
                var drRevenueTrend = dtRevenueTrend.Select(string.Format("{0}={1}", ColumnMonth, drContribution[ColumnMonth])).First();

                double? revenueTrend = 0;
                if (drRevenueTrend.Field<double?>(ColumnValue).HasValue)
                {
                    revenueTrend = drRevenueTrend.Field<double>(ColumnValue);
                }

                double? actualRevenue = 0;
                if (drActualRevenue.Field<double?>(ColumnValue).HasValue)
                {
                    actualRevenue = drActualRevenue.Field<double>(ColumnValue);
                }

                //// Calculating contribution.
                double? contributionPercentage = 0;
                double? contribution = 0;
                if (actualRevenue != 0 && revenueTrend != 0)
                {
                    contribution = (actualRevenue / revenueTrend);
                    contributionPercentage = contribution * 100;
                }

                //// Setting contribution value.
                drContribution[ColumnValue] = contributionPercentage;
                drContribution["Contribution"] = contribution;
            }

            return dtContribution;
        }

        /// <summary>
        /// Function to get revenue trend
        /// </summary>
        /// <param name="dtActualRevenue">Datatable containing actual revenue.</param>
        /// <returns>Returns datatable containing revenue trend for each month.</returns>
        private DataTable GetRevenueTrend(DataTable dtActualRevenue, string selectOption)
        {
            DataTable dtRevenueTrend = GetDataTableMonthValue();
            foreach (DataRow drRevenueTrend in dtRevenueTrend.Rows)
            {
                int month = Convert.ToInt32(drRevenueTrend[ColumnMonth]);

                //// Getting appropriate month from datatable.
                var drActualRevenue = dtActualRevenue.Select(string.Format("{0}<={1}", ColumnMonth, month));

                //// Calculating revenue trend.
                double? sumActualRevenue = drActualRevenue.Sum(ar => ar.Field<double?>(ColumnValue));
                double? revenueTrend = 0;
                int EndMonth = 12;
                if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
                {
                    int currentQuarter = ((month - 1) / 3) + 1;
                    if (currentQuarter == 1)
                    {
                        EndMonth = 3;
                    }
                    else if (currentQuarter == 2)
                    {
                        EndMonth = 6;
                    }
                    else if (currentQuarter == 3)
                    {
                        EndMonth = 9;
                    }
                    else
                    {
                        EndMonth = 12;
                    }
                }

                if (sumActualRevenue.HasValue)
                {
                    revenueTrend = (sumActualRevenue / month) * EndMonth;
                }

                //// Setting revenue trend value.
                drRevenueTrend[ColumnValue] = revenueTrend;
            }

            return dtRevenueTrend;
        }

        /// <summary>
        /// Function to get percentage difference between actual and projected revenue.
        /// </summary>
        /// <param name="dtProjectedRevenue">Datatable containing projected revenue.</param>
        /// <param name="dtActualRevenue">Datatable containing actual revenue.</param>
        /// <returns>Returns datatable containing percentage difference for each month.</returns>
        private DataTable GetDifference(DataTable dtProjectedRevenue, DataTable dtActualRevenue)
        {
            //// Calcualting percentage difference.
            var differences = dtActualRevenue.AsEnumerable().Join(dtProjectedRevenue.AsEnumerable(),
                                                                actual => actual.Field<int>(ColumnMonth), projected => projected.Field<int>(ColumnMonth),
                                                                (actual, projected) => new
                                                                {
                                                                    Month = actual.Field<int>(ColumnMonth),
                                                                    Value = GetPercentageDifference(actual.Field<double?>(ColumnValue).HasValue ? actual.Field<double>(ColumnValue) : 0,
                                                                                                    projected.Field<double?>(ColumnValue).HasValue ? projected.Field<double>(ColumnValue) : 0),
                                                                });

            DataTable dtDifference = GetDataTableMonthValue();
            foreach (var difference in differences)
            {
                //// Getting appropriate month from datatable.
                DataRow drDifference = dtDifference.Select(string.Format("{0}={1}", ColumnMonth, difference.Month)).FirstOrDefault();

                //// Setting value for period.
                drDifference[ColumnValue] = difference.Value;
            }

            return dtDifference;
        }

        /// <summary>
        /// Function to get projected revenue.
        /// </summary>
        /// <param name="planIds">Plan ids.</param>
        /// <param name="ParentLabel">Filter Name.</param>
        /// <param name="id">Filter Id.</param>
        /// <returns>Return datatable containing projected revenue for each month.</returns>
        private DataTable GetProjectedRevenue(List<int> tacticIds, string ParentLabel, string id)
        {
            //// Applying filters i.e. bussiness unit, audience, vertical or geography.
            List<int> tacticIdsList = tacticIds;

            if (ParentLabel == Common.RevenueVertical)
            {
                int verticalId = Convert.ToInt32(id);
                tacticIdsList = db.Plan_Campaign_Program_Tactic.Where(pcpta => tacticIds.Contains(pcpta.PlanTacticId) && pcpta.VerticalId == verticalId).Select(t => t.PlanTacticId).ToList<int>();

            }
            else if (ParentLabel == Common.RevenueGeography)
            {
                Guid geographyId = new Guid(id);
                tacticIdsList = db.Plan_Campaign_Program_Tactic.Where(pcpta => tacticIds.Contains(pcpta.PlanTacticId) && pcpta.GeographyId == geographyId).Select(t => t.PlanTacticId).ToList<int>();
            }
            else if (ParentLabel == Common.RevenueAudience)
            {
                int audienceId = Convert.ToInt32(id);
                tacticIdsList = db.Plan_Campaign_Program_Tactic.Where(pcpta => tacticIds.Contains(pcpta.PlanTacticId) && pcpta.AudienceId == audienceId).Select(t => t.PlanTacticId).ToList<int>();
            }
            else if (ParentLabel == Common.RevenueBusinessUnit)
            {
                Guid businessUnitId = new Guid(id);
                tacticIdsList = db.Plan_Campaign_Program_Tactic.Where(pcpta => tacticIds.Contains(pcpta.PlanTacticId) && pcpta.BusinessUnitId == businessUnitId).Select(t => t.PlanTacticId).ToList<int>();
            }

            List<ProjectedRevenueClass> tacticProjectedRevenue = ProjectedRevenueCalculate(tacticIdsList);

            var calculateProjectedRevenue = from t in db.Plan_Campaign_Program_Tactic.ToList()
                                            join l in tacticProjectedRevenue on t.PlanTacticId equals l.PlanTacticId
                                            select new
                                            {
                                                id = t.PlanTacticId,
                                                StartMonth = t.StartDate.Month,
                                                EndMonth = t.EndDate.Month,
                                                ProjectedRevenue = l.ProjectedRevenue
                                            };
            ////


            //// Distributing revenue as per duration.
            DataTable dtProjectedRevenueAll = GetDataTableMonthValue();
            foreach (var revenue in calculateProjectedRevenue)
            {
                if (revenue.StartMonth == revenue.EndMonth)
                {
                    dtProjectedRevenueAll.Rows.Add(revenue.StartMonth, revenue.ProjectedRevenue);
                }
                else
                {
                    int totalMonth = (revenue.EndMonth - revenue.StartMonth) + 1;
                    double totalRevenue = (double)revenue.ProjectedRevenue / (double)totalMonth;
                    for (var month = revenue.StartMonth; month <= revenue.EndMonth; month++)
                    {
                        dtProjectedRevenueAll.Rows.Add(month, Math.Round(totalRevenue, 2));
                    }
                }
            }


            DataTable dtProjectedRevenue = GetDataTableMonthValue();
            foreach (DataRow drProjectedRevenue in dtProjectedRevenue.Rows)
            {
                //// Aggregating projected revenue month wise.
                DataRow[] drProjectedRevenueAll = dtProjectedRevenueAll.Select(string.Format("{0}={1}", ColumnMonth, drProjectedRevenue[ColumnMonth]));
                double? value = null;
                foreach (var dr in drProjectedRevenueAll)
                {
                    if (dr.Field<double?>(ColumnValue).HasValue)
                    {
                        if (value.HasValue)
                        {
                            value += (double)dr[ColumnValue];
                        }
                        else
                        {
                            value = (double)dr[ColumnValue];
                        }
                    }
                }

                if (value.HasValue)
                {
                    //// Setting value for period.
                    drProjectedRevenue[ColumnValue] = value;
                }
            }

            return dtProjectedRevenue;
        }

        /// <summary>
        /// Function to get actual revenue.
        /// </summary>
        /// <param name="planIds">Plan ids.</param>
        /// <param name="ParentLabel">Filter Name.</param>
        /// <param name="id">Filter Id.</param>
        /// <returns>Return datatable containing actual revenue for each month.</returns>
        private DataTable GetActualRevenue(List<int> tacticIds, string ParentLabel, string id)
        {
            //// Variable for revenue stage title .
            string stageTitleRevenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            var planCampaignTacticActualAll = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpta => tacticIds.Contains(pcpta.Plan_Campaign_Program_Tactic.PlanTacticId) && pcpta.StageTitle == stageTitleRevenue)
                                                                                    .ToList()
                                                                                    .GroupBy(pcpta => pcpta.Period)
                                                                                    .Select(group => new { Period = group.Key, ActualValue = group.Sum(pcpta => pcpta.Actualvalue) })
                                                                                    .OrderBy(pcpta => pcpta.Period);


            if (ParentLabel == Common.RevenueVertical)
            {
                int verticalId = Convert.ToInt32(id);
                planCampaignTacticActualAll = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpta => tacticIds.Contains(pcpta.Plan_Campaign_Program_Tactic.PlanTacticId) && pcpta.StageTitle == stageTitleRevenue)
                                                                                    .ToList()
                                                                                    .Where(pcpta => pcpta.Plan_Campaign_Program_Tactic.VerticalId == verticalId)
                                                                                    .GroupBy(pcpta => pcpta.Period)
                                                                                    .Select(group => new { Period = group.Key, ActualValue = group.Sum(pcpta => pcpta.Actualvalue) })
                                                                                    .OrderBy(pcpta => pcpta.Period);
            }
            else if (ParentLabel == Common.RevenueGeography)
            {
                Guid geographyId = new Guid(id);
                planCampaignTacticActualAll = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpta => tacticIds.Contains(pcpta.Plan_Campaign_Program_Tactic.PlanTacticId) && pcpta.StageTitle == stageTitleRevenue)
                                                                                     .ToList()
                                                                                     .Where(pcpta => pcpta.Plan_Campaign_Program_Tactic.GeographyId == geographyId)
                                                                                     .GroupBy(pcpta => pcpta.Period)
                                                                                     .Select(group => new { Period = group.Key, ActualValue = group.Sum(pcpta => pcpta.Actualvalue) })
                                                                                     .OrderBy(pcpta => pcpta.Period);
            }
            else if (ParentLabel == Common.RevenueAudience)
            {
                int audienceId = Convert.ToInt32(id);
                planCampaignTacticActualAll = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpta => tacticIds.Contains(pcpta.Plan_Campaign_Program_Tactic.PlanTacticId) && pcpta.StageTitle == stageTitleRevenue)
                                                                                     .ToList()
                                                                                     .Where(pcpta => pcpta.Plan_Campaign_Program_Tactic.AudienceId == audienceId)
                                                                                     .GroupBy(pcpta => pcpta.Period)
                                                                                     .Select(group => new { Period = group.Key, ActualValue = group.Sum(pcpta => pcpta.Actualvalue) })
                                                                                     .OrderBy(pcpta => pcpta.Period);
            }
            else if (ParentLabel == Common.RevenueBusinessUnit)
            {
                Guid businessUnitId = new Guid(id);
                planCampaignTacticActualAll = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpta => tacticIds.Contains(pcpta.Plan_Campaign_Program_Tactic.PlanTacticId) && pcpta.StageTitle == stageTitleRevenue)
                                                                                    .ToList()
                                                                                    .Where(pcpta => pcpta.Plan_Campaign_Program_Tactic.BusinessUnitId == businessUnitId)
                                                                                    .GroupBy(pcpta => pcpta.Period)
                                                                                    .Select(group => new { Period = group.Key, ActualValue = group.Sum(pcpta => pcpta.Actualvalue) })
                                                                                    .OrderBy(pcpta => pcpta.Period);
            }

            //// Getting period wise actual revenue of non-deleted and approved/in-progress/complete tactic of plan present in planids.


            DataTable dtActualRevenue = GetDataTableMonthValue();

            //// Populating values in data table.
            foreach (var planCampaignTacticActualMonth in planCampaignTacticActualAll)
            {
                //// Getting appropriate period from datatable.
                DataRow drAactualRevenue = dtActualRevenue.Select(string.Format("{0}={1}", ColumnMonth, Convert.ToInt32(planCampaignTacticActualMonth.Period.Replace("Y", "")))).FirstOrDefault();

                //// Setting value for period.
                drAactualRevenue[ColumnValue] = planCampaignTacticActualMonth.ActualValue;
            }

            return dtActualRevenue;
        }

        /// <summary>
        /// Function to get data table with month and value column.
        /// </summary>
        /// <returns>Return dataable.</returns>
        private DataTable GetDataTableMonthValue()
        {
            DataTable dtMonthValue = new DataTable();
            dtMonthValue.Columns.Add(ColumnMonth, typeof(int));
            dtMonthValue.Columns.Add(ColumnValue, typeof(double));

            //// Adding each month.
            for (int month = 1; month <= 12; month++)
            {
                dtMonthValue.Rows.Add(month, null);
            }

            return dtMonthValue;
        }

        /// <summary>
        /// Function to get plan for report.
        /// </summary>
        /// <returns>Returns list of plan.</returns>
        private List<Plan> GetPlanForReport()
        {
            //// Getting current year's all published plan for all business unit of clientid of director.
            var plans = Common.GetPlan();
            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            string currentYear = DateTime.Now.Year.ToString();
            return plans.Where(p => p.Status.Equals(planPublishedStatus) && p.Year.Equals(currentYear)).Select(p => p).ToList();
        }

        #endregion

        #region "Source Performance"
        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to get data for source performance report.
        /// </summary>
        /// <param name="filter">Filter to get data for plan/trend or actual.</param>
        /// <returns>Return json data for source performance report.</returns>
        public JsonResult GetSourcePerformance(string filter, string selectOption = "thisyear")
        {
            if (filter.Equals(Common.SourcePerformancePlan))
            {
                return GetSourcePerformanceProjected(selectOption);
            }
            else if (filter.Equals(Common.SourcePerformanceTrend))
            {
                return GetSourcePerformanceTrend(selectOption);
            }
            else
            {
                return GetSourcePerformanceActual(selectOption);
            }
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to get source perfromance trend.
        /// </summary>
        /// <returns>Returns json result of source perfromance trend.</returns>
        private JsonResult GetSourcePerformanceTrend(string selectOption)
        {
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            List<string> includeYearList = GetYearList(selectOption, true);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            List<string> months = GetUpToCurrentMonth();

            int lastMonth = GetLastMonthForTrend(selectOption);
            var planCampaignTacticActualAll = db.Plan_Campaign_Program_Tactic_Actual.Where(tactic => tacticIds.Contains(tactic.PlanTacticId));

            var tacticTrenBusinessUnit = planCampaignTacticActualAll.Where(ta => months.Contains(ta.Period) &&
                                                                (ta.StageTitle == revenue))
                                                   .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.BusinessUnitId)
                                                   .Select(ta => new
                                                   {
                                                       BusinessUnitId = ta.Key,
                                                       Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth) / dividemillion
                                                   });

            var tacticTrendGeography = planCampaignTacticActualAll.Where(ta => months.Contains(ta.Period) &&
                                                     (ta.StageTitle == revenue))
                                        .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.GeographyId)
                                        .Select(ta => new
                                        {
                                            GeographyId = ta.Key,
                                            Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth) / dividemillion
                                        });

            var tacticTrendVertical = planCampaignTacticActualAll.Where(ta => months.Contains(ta.Period) &&
                                         (ta.StageTitle == revenue))
                            .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.VerticalId)
                            .Select(ta => new
                            {
                                VerticalId = ta.Key,
                                Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth) / dividemillion
                            });

            var businessUnits = db.BusinessUnits.ToList().Where(b => b.ClientId.Equals(Sessions.User.ClientId))
                                           .Select(b => new
                                           {
                                               Title = b.Title,
                                               ColorCode = string.Format("#{0}", b.ColorCode),
                                               Value = tacticTrenBusinessUnit.Any(bu => bu.BusinessUnitId.Equals(b.BusinessUnitId)) ? tacticTrenBusinessUnit.Where(bu => bu.BusinessUnitId.Equals(b.BusinessUnitId)).First().Trend : 0

                                           });
            var vertical = db.Verticals.ToList().Where(v => v.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = tacticTrendVertical.Any(ve => ve.VerticalId.Equals(v.VerticalId)) ? tacticTrendVertical.Where(ve => ve.VerticalId.Equals(v.VerticalId)).First().Trend : 0
                                                });

            var geography = db.Geographies.ToList().Where(g => g.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = tacticTrendGeography.Any(ge => ge.GeographyId.Equals(g.GeographyId)) ? tacticTrendGeography.Where(ge => ge.GeographyId.Equals(g.GeographyId)).First().Trend : 0
                                                });
            return Json(new
            {
                ChartBusinessUnit = businessUnits,
                ChartVertical = vertical,
                ChartGeography = geography
            }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to get source perfromance actual.
        /// </summary>
        /// <returns>Returns json result of source perfromance actual.</returns>
        private JsonResult GetSourcePerformanceActual(string selectOption)
        {
            List<string> includeYearList = GetYearList(selectOption, true);
            List<string> includeMonth = GetMonthList(selectOption, true);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            var businessUnits = db.BusinessUnits.ToList().Where(b => b.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(b => new
                                                {
                                                    Title = b.Title,
                                                    ColorCode = string.Format("#{0}", b.ColorCode),
                                                    Value = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue)).Any(ta => ta.Plan_Campaign_Program_Tactic.BusinessUnitId.Equals(b.BusinessUnitId)) ? db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.Plan_Campaign_Program_Tactic.BusinessUnitId == b.BusinessUnitId && ta.StageTitle.Equals(revenue)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue) / dividemillion : 0
                                                });
            var vertical = db.Verticals.ToList().Where(v => v.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue)).Any(ta => ta.Plan_Campaign_Program_Tactic.VerticalId.Equals(v.VerticalId)) ? db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.Plan_Campaign_Program_Tactic.VerticalId == v.VerticalId && ta.StageTitle.Equals(revenue)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue) / dividemillion : 0
                                                });

            var geography = db.Geographies.ToList().Where(g => g.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue)).Any(ta => ta.Plan_Campaign_Program_Tactic.GeographyId.Equals(g.GeographyId)) ? db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.Plan_Campaign_Program_Tactic.GeographyId == g.GeographyId && ta.StageTitle.Equals(revenue)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue) / dividemillion : 0
                                                });
            return Json(new
            {
                ChartBusinessUnit = businessUnits,
                ChartVertical = vertical,
                ChartGeography = geography
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to get source perfromance projected.
        /// </summary>
        /// <returns>Returns json result of source perfromance projected.</returns>
        private JsonResult GetSourcePerformanceProjected(string selectOption)
        {
            List<string> includeYearList = GetYearList(selectOption, true);
            List<string> includeMonth = GetMonthList(selectOption, true);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            //// Applying filters i.e. bussiness unit, audience, vertical or geography.

            var businessUnits = db.BusinessUnits.ToList().Where(b => b.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(b => new
                                                {
                                                    Title = b.Title,
                                                    ColorCode = string.Format("#{0}", b.ColorCode),
                                                    Value = db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId)).Any(t => t.BusinessUnitId.Equals(b.BusinessUnitId)) ? GetProjectedRevenueData(db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId) && t.BusinessUnitId.Equals(b.BusinessUnitId)).Select(t => t.PlanTacticId).ToList()).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)) / dividemillion : 0
                                                });



            var vertical = db.Verticals.ToList().Where(v => v.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId)).Any(t => t.VerticalId.Equals(v.VerticalId)) ? GetProjectedRevenueData(db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId) && t.VerticalId.Equals(v.VerticalId)).Select(t => t.PlanTacticId).ToList()).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)) / dividemillion : 0
                                                });

            var geography = db.Geographies.ToList().Where(g => g.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId)).Any(t => t.GeographyId.Equals(g.GeographyId)) ? GetProjectedRevenueData(db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId) && t.GeographyId.Equals(g.GeographyId)).Select(t => t.PlanTacticId).ToList()).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)) / dividemillion : 0
                                                });
            return Json(new
            {
                ChartBusinessUnit = businessUnits,
                ChartVertical = vertical,
                ChartGeography = geography
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Report General

        /// <summary>
        /// Get Year base on select option.
        /// </summary>
        /// <param name="selectOption">select Option.</param>
        /// <param name="isQuarterOnly">isQuarter Only.</param>
        /// <returns>List of Year.</returns>
        public List<string> GetYearList(string selectOption, bool isQuarterOnly = false)
        {
            List<string> includeYearList = new List<string>();
            if (selectOption == Enums.UpcomingActivities.thisyear.ToString())
            {
                includeYearList.Add(DateTime.Now.Year.ToString());
            }
            else if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
            {
                includeYearList.Add(DateTime.Now.Year.ToString());
                int currentQuarter = ((DateTime.Now.Month - 1) / 3) + 1;
                if (currentQuarter == 1)
                {
                    if (!isQuarterOnly)
                    {
                        includeYearList.Add(DateTime.Now.AddYears(-1).Year.ToString());
                    }
                }
            }
            else if (selectOption == Enums.UpcomingActivities.previousyear.ToString())
            {
                includeYearList.Add(DateTime.Now.AddYears(-1).Year.ToString());
            }
            return includeYearList;
        }

        /// <summary>
        /// Get Month Based on Select Option.
        /// </summary>
        /// <param name="selectOption">select Option.</param>
        /// <param name="isQuarterOnly">isQuarter Only.</param>
        /// <returns>List of Month.</returns>
        public List<string> GetMonthList(string selectOption, bool isQuarterOnly = false)
        {
            List<string> includeMonth = new List<string>();
            int startMonth = 1, EndMonth = 12;
            if (selectOption == Enums.UpcomingActivities.thisyear.ToString())
            {
                for (int i = 1; i <= 12; i++)
                {
                    includeMonth.Add(DateTime.Now.Year.ToString() + PeriodPrefix + i);
                }
            }
            else if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
            {
                int currentQuarter = ((DateTime.Now.Month - 1) / 3) + 1;

                if (currentQuarter == 1)
                {
                    startMonth = 1;
                    EndMonth = 3;
                    if (!isQuarterOnly)
                    {
                        for (int i = 10; i <= 12; i++)
                        {
                            includeMonth.Add(DateTime.Now.AddYears(-1).Year.ToString() + PeriodPrefix + i);
                        }
                    }
                }
                else if (currentQuarter == 2)
                {
                    startMonth = 1;
                    EndMonth = 6;
                }
                else if (currentQuarter == 3)
                {
                    startMonth = 4;
                    EndMonth = 9;
                }
                else
                {
                    startMonth = 7;
                    EndMonth = 12;
                }

                for (int i = startMonth; i <= EndMonth; i++)
                {
                    includeMonth.Add(DateTime.Now.Year.ToString() + PeriodPrefix + i);
                }
            }
            else if (selectOption == Enums.UpcomingActivities.previousyear.ToString())
            {
                for (int i = 1; i <= 12; i++)
                {
                    includeMonth.Add(DateTime.Now.AddYears(-1).Year.ToString() + PeriodPrefix + i);
                }
            }
            return includeMonth;
        }

        /// <summary>
        /// Get Last month of current quarter.
        /// </summary>
        /// <param name="selectOption">select Option</param>
        /// <returns>return last month.</returns>
        public int GetLastMonthForTrend(string selectOption)
        {
            int EndMonth = 12;
            if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
            {
                int currentQuarter = ((DateTime.Now.Month - 1) / 3) + 1;
                if (currentQuarter == 1)
                {
                    EndMonth = 3;
                }
                else if (currentQuarter == 2)
                {
                    EndMonth = 6;
                }
                else if (currentQuarter == 3)
                {
                    EndMonth = 9;
                }
                else
                {
                    EndMonth = 12;
                }
            }
            return EndMonth;
        }

        /// <summary>
        /// Function to get tactic for report.
        /// Added By Bhavesh.
        /// </summary>
        /// <returns>Returns list of Tactic Id.</returns>
        private List<int> GetTacticForReport(List<string> includeYearList)
        {
            //// Getting current year's all published plan for all business unit of clientid of director.
            var plans = Common.GetPlan().Select(p => p.PlanId).ToList();
            if (Sessions.ReportPlanId != 0)
            {
                plans = Common.GetPlan().Where(gp => gp.PlanId == Sessions.ReportPlanId).Select(p => p.PlanId).ToList();
            }
            else if (Sessions.BusinessUnitId != Guid.Empty)
            {
                plans = Common.GetPlan().Where(gp => gp.Model.BusinessUnitId == Sessions.BusinessUnitId).Select(p => p.PlanId).ToList();
            }
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            return db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted == false && tacticStatus.Contains(t.Status) && plans.Contains(t.Plan_Campaign_Program.Plan_Campaign.PlanId) && t.Plan_Campaign_Program.Plan_Campaign.Plan.Status == PublishedPlan && includeYearList.Contains(t.Plan_Campaign_Program.Plan_Campaign.Plan.Year)).Select(t => t.PlanTacticId).ToList();
        }

        /// <summary>
        /// Get Month list for Display.
        /// </summary>
        /// <param name="selectOption">select Option.</param>
        /// <returns>list of Month.</returns>
        private List<string> GetDisplayMonthList(string selectOption)
        {
            List<string> lmtitle = new List<string>();
            if (selectOption == Enums.UpcomingActivities.thisyear.ToString())
            {
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jan.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Feb.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Mar.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.April.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.May.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jun.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.July.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Aug.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Sep.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Oct.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Nov.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Dec.ToString()].ToString());
            }
            else if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
            {
                int currentQuarter = ((DateTime.Now.Month - 1) / 3) + 1;

                if (currentQuarter == 1)
                {
                    string currentYearstr = DateTime.Now.ToString("yy");
                    string previousYearstr = DateTime.Now.AddYears(-1).ToString("yy");
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Oct.ToString()].ToString() + "-" + previousYearstr);
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Nov.ToString()].ToString() + "-" + previousYearstr);
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Dec.ToString()].ToString() + "-" + previousYearstr);
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jan.ToString()].ToString() + "-" + currentYearstr);
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Feb.ToString()].ToString() + "-" + currentYearstr);
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Mar.ToString()].ToString() + "-" + currentYearstr);
                }
                else if (currentQuarter == 2)
                {
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jan.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Feb.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Mar.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.April.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.May.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jun.ToString()].ToString());
                }
                else if (currentQuarter == 3)
                {
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.April.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.May.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jun.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.July.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Aug.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Sep.ToString()].ToString());
                }
                else
                {
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.July.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Aug.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Sep.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Oct.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Nov.ToString()].ToString());
                    lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Dec.ToString()].ToString());
                }
            }
            else if (selectOption == Enums.UpcomingActivities.previousyear.ToString())
            {
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jan.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Feb.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Mar.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.April.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.May.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Jun.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.July.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Aug.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Sep.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Oct.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Nov.ToString()].ToString());
                lmtitle.Add(Enums.ReportMonthDisplayValues[Enums.ReportMonthDisplay.Dec.ToString()].ToString());
            }

            return lmtitle;
        }

        /// <summary>
        /// Get Upto Current Month List.
        /// </summary>
        /// <returns>list.</returns>
        private List<string> GetUpToCurrentMonth()
        {
            List<string> monthList = new List<string>();
            for (int i = 1; i <= currentMonth; i++)
            {
                monthList.Add(PeriodPrefix + i);
            }

            return monthList;
        }

        /// <summary>
        /// Class for Tactic Datatable to divide value month wise.
        /// </summary>
        public class TacticDataTable
        {
            public int TacticId { get; set; }
            public int StartMonth { get; set; }
            public int EndMonth { get; set; }
            public double Value { get; set; }
            public int StartYear { get; set; }
            public int EndYear { get; set; }
        }

        /// <summary>
        /// Get Datatable divide value month wise.
        /// </summary>
        /// <param name="tacticdata">tacticdata.</param>
        /// <returns></returns>
        private DataTable GetDatatable(List<TacticDataTable> tacticdata)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(ColumnId, typeof(int));
            dt.Columns.Add(ColumnMonth, typeof(string));
            dt.Columns.Add(ColumnValue, typeof(double));
            foreach (var t in tacticdata)
            {
                if (t.StartYear == t.EndYear)
                {
                    if (t.StartMonth == t.EndMonth)
                    {
                        dt.Rows.Add(t.TacticId, t.StartYear + PeriodPrefix + t.StartMonth, t.Value);
                    }
                    else
                    {
                        int totalMonth = (t.EndMonth - t.StartMonth) + 1;
                        double totalValue = (double)t.Value / (double)totalMonth;
                        for (var i = t.StartMonth; i <= t.EndMonth; i++)
                        {
                            dt.Rows.Add(t.TacticId, t.StartYear.ToString() + PeriodPrefix + i, Math.Round(totalValue, 2));
                        }
                    }
                }
                else
                {
                    int totalMonth = (12 - t.StartMonth) + t.EndMonth + 1;
                    double totalValue = (double)t.Value / (double)totalMonth;
                    for (var i = t.StartMonth; i <= 12; i++)
                    {
                        dt.Rows.Add(t.TacticId, t.StartYear.ToString() + PeriodPrefix + i, Math.Round(totalValue, 2));
                    }
                    for (var i = 1; i <= t.EndMonth + 1; i++)
                    {
                        dt.Rows.Add(t.TacticId, t.EndYear.ToString() + PeriodPrefix + i, Math.Round(totalValue, 2));
                    }
                }
            }
            return dt;
        }

        public static int GetModelId(DateTime StartDate, int ModelId)
        {
            MRPEntities mdbt = new MRPEntities();
            DateTime? effectiveDate = mdbt.Models.Where(m => m.ModelId == ModelId).Select(m => m.EffectiveDate).SingleOrDefault();
            if (effectiveDate != null)
            {
                if (StartDate >= effectiveDate)
                {
                    return ModelId;
                }
                else
                {
                    int? ParentModelId = mdbt.Models.Where(m => m.ModelId == ModelId).Select(m => m.ParentModelId).SingleOrDefault();
                    if (ParentModelId != null)
                    {
                        return GetModelId(StartDate, (int)ParentModelId);
                    }
                    else
                    {
                        return ModelId;
                    }
                }
            }
            else
            {
                return ModelId;
            }
        }

        /// <summary>
        /// Calculate Projected Revenue of Tactic List.
        /// Modified By: Maninder Singh Wadhva to address TFS Bug#280 :Error Message Showing when editing a tactic - Preventing MQLs from updating
        /// Modified By: Maninder Singh Wadhva 1-March-2014 to address TFS Bug#322 : Changes made to INQ, MQL and Projected Revenue Calculation.
        /// </summary>
        /// <param name="tlist"></param>
        /// <returns></returns>
        public static List<ProjectedRevenueClass> ProjectedRevenueCalculate(List<int> tlist, bool isCW = false)
        {
            MRPEntities mdb = new MRPEntities();
            List<string> status = Common.GetStatusListAfterApproved();
            string stageMQL = Enums.Stage.MQL.ToString();
            // Get Level for MQL stage to get Value for CW.
            int levelMQL = mdb.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageMQL)).Level.Value;
            string stageTypeCR = Enums.StageType.CR.ToString();
            List<TacticModelRelation> tacticModelList = GetTacticModelRelation(tlist);

            var revenuelist = (from tactic in mdb.Plan_Campaign_Program_Tactic.ToList()
                               join t in tacticModelList on tactic.PlanTacticId equals t.PlanTacticId
                               join modelFunnelStage in mdb.Model_Funnel_Stage on t.ModelId equals modelFunnelStage.Model_Funnel.ModelId
                               join stage in mdb.Stages on modelFunnelStage.StageId equals stage.StageId
                               where modelFunnelStage.StageType.Equals(stageTypeCR) &&
                                               stage.ClientId.Equals(Sessions.User.ClientId) &&
                                               stage.Level >= levelMQL &&
                                               modelFunnelStage.Value != 0 && modelFunnelStage.Model_Funnel.AverageDealSize != 0
                               select new
                               {
                                   PlanTacticId = tactic.PlanTacticId,
                                   ModelFunnelId = modelFunnelStage.ModelFunnelId,
                                   MQLs = tactic.MQLs,
                                   ADS = modelFunnelStage.Model_Funnel.AverageDealSize,
                                   Value = (double)modelFunnelStage.Value
                               }).ToList().GroupBy(rl => new { PlanTacticId = rl.PlanTacticId, MQL = rl.MQLs, ModelFunnelId = rl.ModelFunnelId, ADS = rl.ADS }).ToList().Select(r => new
                               {
                                   PlanTacticId = r.Key.PlanTacticId,
                                   mql = r.Key.MQL,
                                   ADS = r.Key.ADS,
                                   value = (r.Aggregate(1.0, (s1, s2) => s1 * (s2.Value / 100)))
                               }).Select(lr => new { PlanTacticId = lr.PlanTacticId, ProjectedRevenue = isCW ? lr.mql * lr.value : lr.mql * lr.value * lr.ADS }).ToList();

            var projectedrevenuelist = revenuelist.GroupBy(rl => new { id = rl.PlanTacticId }).Select(r => new
            {
                id = r.Key.id,
                pr = r.Sum(a => a.ProjectedRevenue)
            }).ToList().Select(p => new { id = p.id, prevenue = p.pr }).ToList();

            List<ProjectedRevenueClass> tacticList = projectedrevenuelist.Select(al => new ProjectedRevenueClass { PlanTacticId = al.id, ProjectedRevenue = al.prevenue }).ToList();

            return tacticList;
        }

        /// <summary>
        /// Class of TacticId & Model Id Relation.
        /// </summary>
        public class TacticModelRelation
        {
            public int PlanTacticId { get; set; }
            public int ModelId { get; set; }
        }

        public static List<TacticModelRelation> GetTacticModelRelation(List<int> tlist)
        {
            MRPEntities modeldb = new MRPEntities();
            var tacticModel = (from tactic in modeldb.Plan_Campaign_Program_Tactic
                               join m in modeldb.Models on tactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId equals m.ModelId
                               where tlist.Contains(tactic.PlanTacticId)
                               select new
                               {
                                   PlanTacticId = tactic.PlanTacticId,
                                   StartDate = tactic.StartDate,
                                   ModelId = m.ModelId
                               }).ToList();

            List<TacticModelRelation> tacticModellist = (from t in tacticModel
                                                         select new TacticModelRelation
                                                         {
                                                             PlanTacticId = t.PlanTacticId,
                                                             ModelId = GetModelId(t.StartDate, t.ModelId)
                                                         }).ToList();

            return tacticModellist;
        }

        #endregion

        #endregion

        #region Report Header

        /// <summary>
        /// Load Report Header.
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult ReportHeader(string id = "thisyear")
        {
            List<SelectListItem> UpcomingActivityList = GetUpcomingActivityForReport().Select(p => new SelectListItem() { Text = p.Text, Value = p.Value.ToString() }).ToList();
            ViewBag.UpcomingActivity = UpcomingActivityList;
            ViewBag.SelectOption = id;
            return PartialView();
        }

        /// <summary>
        /// Get Upcoming Activity for Report Header.
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetUpcomingActivityForReport()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thisyear.ToString()].ToString(), Value = Enums.UpcomingActivities.thisyear.ToString(), Selected = false });
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thisquarter.ToString()].ToString(), Value = Enums.UpcomingActivities.thisquarter.ToString(), Selected = false });
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.previousyear.ToString()].ToString(), Value = Enums.UpcomingActivities.previousyear.ToString(), Selected = false });
            return items;
        }

        /// <summary>
        /// Load Conversion Report Header.
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult ConversionReportHeader(string id = "thisyear")
        {
            List<SelectListItem> UpcomingActivityList = GetUpcomingActivityForReport().Select(p => new SelectListItem() { Text = p.Text, Value = p.Value.ToString() }).ToList();
            ViewBag.UpcomingActivity = UpcomingActivityList;
            ViewBag.SelectOption = id;
            return PartialView();
        }

        /// <summary>
        /// Get Report Header Data.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public JsonResult GetReportRevenueHeader(string option)
        {
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            List<string> includeYearList = GetYearList(option, true);
            List<string> includeMonth = GetMonthList(option, true);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            int lastMonth = GetLastMonthForTrend(option);
            List<string> monthList = GetUpToCurrentMonth();
            double projectedRevenue = 0;
            double actualRevenue = 0;
            double trendRevenue = 0;
            double trendMQL = 0;
            double projectedMQL = 0;
            double actualMQL = 0;
            if (tacticIds.Count > 0)
            {
                projectedRevenue = GetProjectedRevenueData(tacticIds).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));
                actualRevenue = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue);
                trendRevenue = GetTrendRevenueDataContribution(tacticIds, lastMonth);
                trendMQL = ((db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && monthList.Contains(pcpt.Period) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString())).Sum(pt => pt.Actualvalue)) / currentMonth) * lastMonth;
                projectedMQL = GetProjectedMQLData(tacticIds).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));
                actualMQL = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(mql)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue);
            }
            return Json(new { ProjectedRevenueValue = projectedRevenue, ActualRevenueValue = actualRevenue, TrendRevenue = trendRevenue, TrendMQL = Math.Round(trendMQL), ProjectedMQLValue = Math.Round(projectedMQL), ActualMQLValue = Math.Round(actualMQL) });
        }

        #endregion

        #region "Share Report"
        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to show share report popup.
        /// </summary>
        /// <param name="reportType">Type of report.</param>
        /// <returns>Returns partial view of result.</returns>
        public PartialViewResult ShowShareReport(string reportType)
        {
            ViewBag.ReportType = reportType;
            BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();
            var individuals = bdsUserRepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, Sessions.IsSystemAdmin);
            if (individuals.Count != 0)
            {
                ViewBag.EmailIds = individuals.Select(member => member.Email).ToList<string>();
            }

            return PartialView("ShareReport");
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to generate report and send to e-mail id.
        /// </summary>
        /// <param name="reportType">Type of report.</param>
        /// <param name="toEmailIds">Email id to whom pdf report should be sent.</param>
        /// <param name="optionalMessage">Optional message.</param>
        /// <param name="htmlOfCurrentView">Html of current view.</param>
        /// <returns>Returns json result which indicates report is generated and sent sucessfully.</returns>
        public JsonResult ShareReport(string reportType, string toEmailIds, string optionalMessage, string htmlOfCurrentView)
        {
            int result = 0;
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        if (ModelState.IsValid)
                        {
                            htmlOfCurrentView = HttpUtility.UrlDecode(htmlOfCurrentView, System.Text.Encoding.Default);

                            //// Modified By Maninder Singh Wadhva so that mail is sent to multiple user.
                            MemoryStream pdfStream = GeneratePDFReport(htmlOfCurrentView, reportType);

                            string notificationShareReport = Enums.Custom_Notification.ShareReport.ToString();
                            Notification notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(notificationShareReport));
                            string emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage);

                            foreach (string toEmail in toEmailIds.Split(','))
                            {
                                Report_Share reportShare = new Report_Share();
                                reportShare.ReportType = reportType;
                                reportShare.EmailId = toEmail;
                                reportShare.EmailBody = emailBody;
                                reportShare.CreatedDate = DateTime.Now;
                                reportShare.CreatedBy = Sessions.User.UserId;
                                db.Entry(reportShare).State = EntityState.Added;
                                db.Report_Share.Add(reportShare);
                                result = db.SaveChanges();
                                if (result == 1)
                                {
                                    //// Modified By Maninder Singh Wadhva so that mail is sent to multiple user.
                                    Common.sendMail(toEmail, Common.FromMail, emailBody, notification.Subject, new MemoryStream(pdfStream.ToArray()), string.Format("{0}.pdf", reportType));
                                }
                            }

                            scope.Complete();
                            return Json(true, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to generate pdf report.
        /// Modified By: Maninder Singh Wadhva.
        /// To send mail to multiple user.
        /// </summary>
        /// <param name="htmlOfCurrentView">Html of current view.</param>
        /// <param name="reportType">Type of report.</param>
        /// <returns>Returns stream of PDF report.</returns>
        private MemoryStream GeneratePDFReport(string htmlOfCurrentView, string reportType)
        {
            htmlOfCurrentView = AddCSSAndJS(htmlOfCurrentView, reportType);
            PdfConverter pdfConverter = new PdfConverter();
            pdfConverter.LicenseKey = System.Configuration.ConfigurationManager.AppSettings["EvoHTMLKey"];

            pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.Custom;
            pdfConverter.PdfDocumentOptions.CustomPdfPageSize = new System.Drawing.SizeF(1024, 1000);
            pdfConverter.PdfDocumentOptions.EmbedFonts = true;


            pdfConverter.PdfDocumentOptions.PdfCompressionLevel = PdfCompressionLevel.Normal;
            pdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;
            byte[] pdf = pdfConverter.GetPdfBytesFromHtmlString(htmlOfCurrentView);

            return new System.IO.MemoryStream(pdf);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to add css and javascript.
        /// </summary>
        /// <param name="htmlOfCurrentView">Html of current view.</param>
        /// <param name="reportType">Report Type.</param>
        /// <returns>Return html string with CSS and Javascript.</returns>
        private string AddCSSAndJS(string htmlOfCurrentView, string reportType)
        {
            var htmlStart = "<html>";
            var headStart = "<head>";
            var cssBootstrap = string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/bootstrap.css"));
            var cssBootstrapResponsive = string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/bootstrap-responsive.css"));
            var cssStyle = string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/style.css"));
            var cssDatepicker = string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/datepicker.css"));
            var cssStyle_extended = string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/style_extended.css"));
            var jsDhtmlxgantt = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/DHTMLX/dhtmlxgantt.js"));
            var cssDhtmlxgantt = string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/DHTMLX/dhtmlxgantt.css"));

            var jsjqueryminjs = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.min.js"));
            var jsbootstrapminjs = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/bootstrap.min.js"));
            var jsjqueryslimscrolljs = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.slimscroll.js"));
            var jsjqueryslidepaneljs = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.slidepanel.js"));
            var jsscriptsjs = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/scripts.js"));
            var jsscripts_extendedjs = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/scripts_extended.js"));
            var jsjqueryformminjs = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.form.js"));
            var jsbootstrapdatepickerjs = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/bootstrap-datepicker.js"));
            var jsjqueryprice_format18js = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.price_format.1.8.js"));
            var jsjqueryprice_format18minjs = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.price_format.1.8.js"));
            var jsslimScrollHorizontaljs = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/slimScrollHorizontal.js"));
            var jsjquerytipsyjs = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.tipsy.js"));
            var csstipsy = string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/tipsy.css"));
            var jsDhtmlxchart = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/dhtmlxchart.js"));
            var cssDhtmlchart = string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/dhtmlxchart.css"));

            var jsReportSummary = string.Empty;
            var jsReportRevenue = string.Empty;
            var jsReportConversion = string.Empty;
            if (reportType.Equals(Enums.ReportType.Summary.ToString()))
            {
                jsReportSummary = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/ReportSummary.js"));
            }
            else if (reportType.Equals(Enums.ReportType.Revenue.ToString()))
            {
                jsReportRevenue = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/ReportRevenue.js"));
            }
            else
            {
                jsReportConversion = string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/ReportConversion.js"));
            }

            var headEnd = "</head>";
            var bodyStart = "<body>";
            var bodyEnd = "</body>";
            var htmlEnd = "</html>";
            return htmlStart + headStart + cssBootstrap + cssBootstrapResponsive + cssStyle + cssDatepicker + cssStyle_extended + jsDhtmlxgantt + cssDhtmlxgantt + jsjqueryminjs +
                jsbootstrapminjs + jsjqueryslimscrolljs + jsjqueryslidepaneljs + jsscriptsjs + jsscripts_extendedjs + jsjqueryformminjs + jsbootstrapdatepickerjs + jsjqueryprice_format18js + jsjqueryprice_format18minjs
               + jsslimScrollHorizontaljs + jsjquerytipsyjs + csstipsy + jsDhtmlxchart + cssDhtmlchart + headEnd + bodyStart + htmlOfCurrentView + bodyEnd + htmlEnd + jsReportSummary + jsReportRevenue + jsReportConversion;
        }
        #endregion

    }
}

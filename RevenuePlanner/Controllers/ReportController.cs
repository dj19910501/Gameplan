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
        //private const double dividemillion = 1000;

        #endregion

        #region Report Summaries

        /// <summary>
        /// This action will return the Report index page
        /// </summary>
        /// <param name="activeMenu"></param>
        /// <returns></returns>
        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Report, bool isFromReport = false)
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
            List<SelectListItem> lstPlans = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();

            if (Sessions.BusinessUnitId != Guid.Empty)
            {
                lstBusinessUnits.Where(lbu => lbu.Value == Convert.ToString(Sessions.BusinessUnitId)).ToList().ForEach(lbu => lbu.Selected = true);
                lstPlans = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Model.BusinessUnitId == Sessions.BusinessUnitId && pl.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();
            }
            /* added by Nirav shah for TFS Point : 218*/
            if (Sessions.PlanId != 0 && !isFromReport)
            {
                if (Common.IsPlanPublished(Sessions.PlanId))
                {
                    Sessions.ReportPlanId = Sessions.PlanId;
                }
                else
                {
                    Sessions.ReportPlanId = Sessions.PublishedPlanId;
                }
            }
            if (Sessions.ReportPlanId > 0)
            {
                lstPlans.Where(lp => lp.Value == Convert.ToString(Sessions.ReportPlanId)).ToList().ForEach(lp => lp.Selected = true);
            }


            if (Sessions.ReportPlanId == 0)
            {
                ViewBag.PlanTitle = "All Plans";
                //Sessions.PlanId = 0;/* added by Nirav shah for TFS Point : 218*/
                Sessions.PublishedPlanId = 0;
            }
            else
            {
                var plan = db.Plans.Single(p => p.PlanId.Equals(Sessions.ReportPlanId));
                ViewBag.PlanTitle = plan.Title;
            }


            FilterDropdownValues objFilterData = new FilterDropdownValues();
            objFilterData.lstBusinessUnit = lstBusinessUnits;
            objFilterData.lstAllPlans = lstPlans;

            return View(objFilterData);
        }

        /// <summary>
        /// Set Report Parameter : Update by Bhavesh
        /// </summary>
        /// <param name="BusinessUnitId"></param>
        /// <param name="PlanId"></param>
        public void SetReportParameter(string BusinessUnitId = "", string PlanId = "")
        {
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
                Sessions.PlanId = int_PlanId;
            }
            else if (PlanId == "0" || PlanId == "") // This means all plans are selected
            {
                Sessions.ReportPlanId = 0;
                Sessions.PlanId = 0;/* added by Nirav shah for TFS Point : 218*/
                Sessions.PublishedPlanId = 0;
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
                Sessions.PlanId = int_PlanId;/* added by Nirav shah for TFS Point : 218*/
            }
            else if (PlanId == "0" || string.IsNullOrEmpty(PlanId)) // This means all plans are selected
            {
                Sessions.ReportPlanId = 0;
                Sessions.PlanId = 0;/* added by Nirav shah for TFS Point : 218*/
                Sessions.PublishedPlanId = 0;
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
                /* commented by Nirav shah for TFS Point : 218*/
                //{
                //    //// Getting current year's published plan for business unit of director.
                //    Plan sessionActivePlan = plans.Where(p => p.Model.BusinessUnitId.Equals(Sessions.User.BusinessUnitId)).Select(p => p).FirstOrDefault();
                //    if (sessionActivePlan != null)
                //    {
                //        Sessions.PlanId = sessionActivePlan.PlanId;
                //    }
                //}

                //if (!string.IsNullOrEmpty(PlanId) && Convert.ToString(PlanId) != "0")
                //{
                //    Sessions.PlanId = Convert.ToInt32(PlanId);
                //}
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
            /*added by Nirav Shah for PL 339: Revenue Summary - red % value should be compared to year to date on  15 APR 2014 */
            string thisYear = Enums.UpcomingActivities.thisyear.ToString();
            List<string> includeYearList = GetYearList(thisYear);
            List<string> includeMonth = GetUpToCurrentMonthWithYear(thisYear);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            overAllMQLActual = 0;
            overAllRevenueActual = 0;
            overAllMQLProjected = 0;
            overAllRevenueProjected = 0;

            var MQLActuallist = (db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()))).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).ToList();
            if (MQLActuallist.Count > 0)
            {
                overAllMQLActual = MQLActuallist.Sum(t => t.Actualvalue);
            }

            var MQLProjectedlist = GetProjectedMQLData(tacticIds).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).ToList();
            if (MQLProjectedlist.Count > 0)
            {
                overAllMQLProjected = MQLProjectedlist.Sum(mr => mr.Field<double>(ColumnValue));
            }

            var RevenueActualllist = (db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString()))).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).ToList();
            if (RevenueActualllist.Count > 0)
            {
                overAllRevenueActual = RevenueActualllist.Sum(t => t.Actualvalue);
            }

            var RevenueProjectedlist = GetProjectedRevenueData(tacticIds).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).ToList();
            if (RevenueProjectedlist.Count > 0)
            {
                overAllRevenueProjected = RevenueProjectedlist.Sum(mr => mr.Field<double>(ColumnValue));
            }

            overAllRevenueProjected = Math.Round(overAllRevenueProjected);
            overAllRevenueActual = Math.Round(overAllRevenueActual);

            // MQL
            objSummaryReportModel.MQLs = overAllMQLActual;
            double overAllMQLPercentage = GetPercentageDifference(overAllMQLActual, overAllMQLProjected);
            objSummaryReportModel.MQLsPercentage = overAllMQLPercentage.ToString("#0.##", CultureInfo.InvariantCulture);

            // Actual Revenue
            objSummaryReportModel.Revenue = Convert.ToString(overAllRevenueActual);
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
            var INQActualList = (db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.INQ.ToString()].ToString()))).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).ToList();
            if (INQActualList.Count > 0)
            {
                overAllInqActual = INQActualList.Sum(t => t.Actualvalue);
            }

            var InqProjectedList = GetConversionProjectedINQData(tacticIds).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).ToList();
            if (InqProjectedList.Count > 0)
            {
                overAllInqProjected = InqProjectedList.Sum(mr => mr.Field<double>(ColumnValue));
            }

            double inqPercentageDifference = WaterfallGetPercentageDifference(overAllMQLActual, overAllMQLProjected, overAllInqActual, overAllInqProjected);
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
            var CWActualList = (db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString()))).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).ToList();
            if (CWActualList.Count > 0)
            {
                overAllCWActual = CWActualList.Sum(t => t.Actualvalue);
            }

            var CwProjectedList = GetProjectedRevenueData(tacticIds, true).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).ToList();
            if (CwProjectedList.Count > 0)
            {
                overAllCWProjected = CwProjectedList.Sum(mr => mr.Field<double>(ColumnValue));
            }

            overAllMQLPercentage = WaterfallGetPercentageDifference(overAllCWActual, overAllCWProjected, overAllMQLActual, overAllMQLProjected);
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
            double cwPercentageDifference = WaterfallGetPercentageDifference(overAllCWActual, overAllCWProjected, overAllMQLActual, overAllMQLProjected);
            //GetPercentageDifference(overAllCWActual, overAllCWProjected);
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
                List<ProjectedRevenueClass> tacticsProjectedRevenue = Common.ProjectedRevenueCalculate(tacticsId);
                if (tacticsProjectedRevenue.Count() > 0)
                {
                    //// Aggregating projected revenue of tactic to get same for plan.
                    projectedRevenue = tacticsProjectedRevenue.Sum(tactics => tactics.ProjectedRevenue);
                }
            }

            return projectedRevenue;
        }

        /// <summary>
        /// Function to get projected revenue.
        /// Added By: Nirav Shah for PL 342:Revenue Summary - Waterfall Summary - How are conversions calculatedon 16 apr 2014.
        /// </summary>
        /// <param name="mql">Overall MQL.</param>
        /// <param name="mqlProjected">Overall MQ projectedL.</param>
        /// <param name="inq">Overall INQ.</param>
        /// <param name="inqProjected">Overall INQ projected.</param>
        /// <returns>Returns projected revenue for current plan.</returns>
        private double WaterfallGetPercentageDifference(double mql, double mqlProjected, double inq, double inqProjected)
        {
            double percentage = 0;
            if (inqProjected != 0 && inq != 0 && mql != 0 && mqlProjected != 0)
            {
                percentage = ((mql / inq) / (mqlProjected / inqProjected)) * 100;
            }
            return percentage;
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
                                                                t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(t => t.PlanTacticId).ToList();

            double projectedMQL = 0;
            if (tactics.Count() > 0)
            {
                //// Getting projected MQL for plan.
                projectedMQL = Common.GetMQLTacticList(tactics).Sum(tm => tm.MQL);
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
                List<ProjectedRevenueClass> tacticsProjectedCW = Common.ProjectedRevenueCalculate(tacticsId, true);
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
                return number.ToString("#,##0,.##k", CultureInfo.InvariantCulture);
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

            /* To resolve Bug 312: Report plan selector needs to be moved */

            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            string currentYear = DateTime.Now.Year.ToString();

            //List of Business Units
            List<SelectListItem> lstBusinessUnits = Common.GetBussinessUnitIds(Sessions.User.ClientId).Select(p => new SelectListItem() { Text = p.Text, Value = p.Value.ToString(), Selected = false }).ToList();

            //List of Plans
            List<SelectListItem> lstPlans = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();

            if (Sessions.BusinessUnitId != Guid.Empty)
            {
                lstBusinessUnits.Where(lbu => lbu.Value == Convert.ToString(Sessions.BusinessUnitId)).ToList().ForEach(lbu => lbu.Selected = true);
                lstPlans = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Model.BusinessUnitId == Sessions.BusinessUnitId && pl.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();
            }


            if (Sessions.ReportPlanId > 0)
            {
                lstPlans.Where(lp => lp.Value == Convert.ToString(Sessions.ReportPlanId)).ToList().ForEach(lp => lp.Selected = true);
            }

            SummaryReportModel objSummaryReportModel = new SummaryReportModel();
            objSummaryReportModel.lstBusinessUnit = lstBusinessUnits;
            objSummaryReportModel.lstAllPlans = lstPlans;

            /* To resolve Bug 312: Report plan selector needs to be moved */

            ViewBag.MonthTitle = GetDisplayMonthList(id);
            ViewBag.SelectOption = id;

            return View(objSummaryReportModel);
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
                string inspectStageINQ = Enums.InspectStageValues[Enums.InspectStage.INQ.ToString()].ToString();
                string inspectStageMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
                List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = db.Plan_Campaign_Program_Tactic_Actual
                                                                               .Where(pcpt => rrTacticList.Contains(pcpt.PlanTacticId) &&
                                                                                             includeMonth.Contains(pcpt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pcpt.Period))
                                                                               .ToList();

                var rdata = new[] { new { 
                INQGoal = GetConversionProjectedINQData(rrTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                monthList = includeMonth,
                INQActual =planTacticActual.Where(pcpt => pcpt.StageTitle.Equals(inspectStageINQ))
                                             .GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period)
                                             .Select(pcptj => new
                                              {
                                                PKey = pcptj.Key,
                                                PSum = pcptj.Sum(pt => pt.Actualvalue)
                                              }),
                MQLGoal = GetConversionProjectedMQLData(rrTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                MQLActual = planTacticActual.Where(pcpt => pcpt.StageTitle.Equals(inspectStageMQL))
                            .GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period)
                            .Select(pcptj => new
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
        /// Modified By: Maninde Singh Wadhva for #426 	Conversion Reporting Page is slow to render.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetConversionProjectedINQData(List<int> tlist)
        {
            List<TacticDataTable> tacticdata = (from tactic in db.Plan_Campaign_Program_Tactic.Where(t => tlist.Contains(t.PlanTacticId)).ToList()
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
        /// Modified By: Maninde Singh Wadhva for #426 	Conversion Reporting Page is slow to render.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetConversionProjectedMQLData(List<int> tlist)
        {
            List<Plan_Tactic_MQL> MQLTacticList = Common.GetMQLTacticList(tlist);
            List<TacticDataTable> tacticdata = (from tactic in db.Plan_Campaign_Program_Tactic.Where(t => tlist.Contains(t.PlanTacticId)).ToList()
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.PlanTacticId,
                                                    Value = MQLTacticList.Where(tm => tm.PlanTacticId == tactic.PlanTacticId).Select(tm => tm.MQL).SingleOrDefault(),
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

            var returnDataGuid = new object();

            if (ParentTab == Common.BusinessUnit)
            {
                returnDataGuid = (db.BusinessUnits.Where(bu => bu.ClientId.Equals(Sessions.User.ClientId) && bu.IsDeleted.Equals(false))).Select(b => new
                {
                    id = b.BusinessUnitId,
                    title = b.Title
                }).Distinct().OrderBy(b => b.id);
            }
            else if (ParentTab == Common.Audience)
            {
                returnDataGuid = (db.Audiences.ToList().Where(au => au.ClientId.Equals(Sessions.User.ClientId) && au.IsDeleted.Equals(false))).Select(a => new
                {
                    id = a.AudienceId,
                    title = a.Title
                }).Distinct().OrderBy(a => a.id);
            }
            else if (ParentTab == Common.Geography)
            {
                returnDataGuid = (db.Geographies.ToList().Where(ge => ge.ClientId.Equals(Sessions.User.ClientId) && ge.IsDeleted.Equals(false))).Select(g => new
                {
                    id = g.GeographyId,
                    title = g.Title
                }).Distinct().OrderBy(g => g.id);
            }
            else if (ParentTab == Common.Vertical)
            {
                returnDataGuid = (db.Verticals.ToList().Where(ve => ve.ClientId.Equals(Sessions.User.ClientId) && ve.IsDeleted.Equals(false))).Select(v => new
                {
                    id = v.VerticalId,
                    title = v.Title
                }).Distinct().OrderBy(v => v.id);
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
                                                       Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth)
                                                   });

            var tacticTrendGeography = planCampaignTacticActualAll.Where(ta => months.Contains(ta.Period) &&
                                                     (ta.StageTitle == mql))
                                        .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.GeographyId)
                                        .Select(ta => new
                                        {
                                            GeographyId = ta.Key,
                                            Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth)
                                        });

            var tacticTrendVertical = planCampaignTacticActualAll.Where(ta => months.Contains(ta.Period) &&
                                         (ta.StageTitle == mql))
                            .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.VerticalId)
                            .Select(ta => new
                            {
                                VerticalId = ta.Key,
                                Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth)
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
                                                    Value = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(mql)).Any(ta => ta.Plan_Campaign_Program_Tactic.BusinessUnitId.Equals(b.BusinessUnitId)) ? db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.Plan_Campaign_Program_Tactic.BusinessUnitId == b.BusinessUnitId && ta.StageTitle.Equals(mql)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue) : 0
                                                });
            var vertical = db.Verticals.ToList().Where(v => v.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(mql)).Any(ta => ta.Plan_Campaign_Program_Tactic.VerticalId.Equals(v.VerticalId)) ? db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.Plan_Campaign_Program_Tactic.VerticalId == v.VerticalId && ta.StageTitle.Equals(mql)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue) : 0
                                                });

            var geography = db.Geographies.ToList().Where(g => g.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(mql)).Any(ta => ta.Plan_Campaign_Program_Tactic.GeographyId.Equals(g.GeographyId)) ? db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.Plan_Campaign_Program_Tactic.GeographyId == g.GeographyId && ta.StageTitle.Equals(mql)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue) : 0
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
                                                    Value = db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId)).Any(t => t.BusinessUnitId.Equals(b.BusinessUnitId)) ? GetProjectedMQLData(db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId) && t.BusinessUnitId.Equals(b.BusinessUnitId)).Select(t => t.PlanTacticId).ToList()).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)) : 0
                                                });



            var vertical = db.Verticals.ToList().Where(v => v.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId)).Any(t => t.VerticalId.Equals(v.VerticalId)) ? GetProjectedMQLData(db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId) && t.VerticalId.Equals(v.VerticalId)).Select(t => t.PlanTacticId).ToList()).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)) : 0
                                                });

            var geography = db.Geographies.ToList().Where(g => g.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId)).Any(t => t.GeographyId.Equals(g.GeographyId)) ? GetProjectedMQLData(db.Plan_Campaign_Program_Tactic.Where(t => tacticIds.Contains(t.PlanTacticId) && t.GeographyId.Equals(g.GeographyId)).Select(t => t.PlanTacticId).ToList()).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)) : 0
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
        /// Modified By: Maninde Singh Wadhva for #426 	Conversion Reporting Page is slow to render.
        /// </summary>
        /// <param name="ParentConversionSummaryTab"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult GetConversionSummary(string ParentConversionSummaryTab = "", string selectOption = "thisyear")
        {
            List<string> includeYearList = GetYearList(selectOption);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            List<Plan_Campaign_Program_Tactic> tacticListobj = db.Plan_Campaign_Program_Tactic.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) &&
                ((ParentConversionSummaryTab == Common.BusinessUnit && pcpt.BusinessUnit.ClientId == Sessions.User.ClientId) ||
                (ParentConversionSummaryTab == Common.Audience && pcpt.Audience.ClientId == Sessions.User.ClientId) ||
                (ParentConversionSummaryTab == Common.Geography && pcpt.Geography.ClientId == Sessions.User.ClientId) ||
                (ParentConversionSummaryTab == Common.Vertical && pcpt.Vertical.ClientId == Sessions.User.ClientId))
                ).ToList();

            var DataTitleList = tacticListobj.GroupBy(pc => new { title = pc.BusinessUnit.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                         }).ToList();

            if (ParentConversionSummaryTab == Common.RevenueVertical)
            {
                DataTitleList = tacticListobj.GroupBy(pc => new { title = pc.Vertical.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                         }).ToList();
            }
            else if (ParentConversionSummaryTab == Common.RevenueGeography)
            {
                DataTitleList = tacticListobj.GroupBy(pc => new { title = pc.Geography.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                         }).ToList();
            }
            else if (ParentConversionSummaryTab == Common.RevenueAudience)
            {
                DataTitleList = tacticListobj.GroupBy(pc => new { title = pc.Audience.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.PlanTacticId).ToList()
                         }).ToList();
            }
            List<string> includeMonth = GetMonthList(selectOption, true);
            string stageTitleINQ = Enums.InspectStage.INQ.ToString();
            string stageTitleMQL = Enums.InspectStage.MQL.ToString();
            string stageTitleCW = Enums.InspectStage.CW.ToString();
            string stageTitleRevenue = Enums.InspectStage.Revenue.ToString();
            string marketing = Enums.Funnel.Marketing.ToString();

            List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = db.Plan_Campaign_Program_Tactic_Actual
                                                                           .Where(pcpta => tacticIds.Contains(pcpta.PlanTacticId) &&
                                                                                           includeMonth.Contains(pcpta.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pcpta.Period))
                                                                           .ToList();

            var DataListFinal = DataTitleList.Select(p => new
            {
                Title = p.Title,
                INQ = GetActualValueForConversionSummary(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleINQ),
                MQL = GetActualValueForConversionSummary(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleMQL),
                ActualCW = GetActualValueForConversionSummary(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleCW),
                ActualRevenue = GetActualValueForConversionSummary(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleRevenue),
                ActualADS = CalculateActualADS(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleCW, stageTitleRevenue),
                ProjectedCW = GetProjectedRevenueData(p.planTacticList, true).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                ProjectedRevenue = GetProjectedRevenueData(p.planTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                ProjectedADS = db.Model_Funnel.Where(mf => mf.Funnel.Title == marketing && (db.Plan_Campaign_Program_Tactic.Where(t => p.planTacticList.Contains(t.PlanTacticId)).Select(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId).Distinct()).Contains(mf.ModelId)).Sum(mf => mf.AverageDealSize)
            }).Distinct().OrderBy(p => p.Title);

            return Json(new { data = DataListFinal }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Actual value based on stagetitle
        /// Added by Bhavesh
        /// Modified By: Maninde Singh Wadhva for #426 	Conversion Reporting Page is slow to render.
        /// </summary>
        /// <param name="planTacticActual">Plan tactic actual</param>
        /// <param name="stagetitle">Stage title.</param>
        /// <returns>Returns actual value for conversion summary.</returns>
        private double GetActualValueForConversionSummary(List<Plan_Campaign_Program_Tactic_Actual> planTacticActual, string stagetitle)
        {
            double actualValue = planTacticActual.Where(pcpta => pcpta.StageTitle == stagetitle)
                                                 .Sum(pcpta => pcpta.Actualvalue);
            return actualValue;
        }

        /// <summary>
        /// Calculate ADS based on actual value i.e. Revenue / CW
        /// Added By Bhavesh 
        /// Modified By: Maninde Singh Wadhva for #426 	Conversion Reporting Page is slow to render.
        /// </summary>
        /// <param name="planTacticActual">Plan tactic actual.</param>
        /// <param name="stagetitleCW">Stage title CW.</param>
        /// <param name="stagetitleRevenue">Stage title revenue.</param>
        /// <returns>Returns calculated ADS.</returns>
        private double CalculateActualADS(List<Plan_Campaign_Program_Tactic_Actual> planTacticActual, string stagetitleCW, string stagetitleRevenue)
        {
            double ads = 0;
            double actualRevenue = GetActualValueForConversionSummary(planTacticActual, stagetitleRevenue);
            double actualCW = GetActualValueForConversionSummary(planTacticActual, stagetitleCW);
            if (actualCW > 0)
            {
                ads = actualRevenue / actualCW;
            }
            return ads;
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

            /* To resolve Bug 312: Report plan selector needs to be moved */
            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            string currentYear = DateTime.Now.Year.ToString();

            //List of Business Units
            List<SelectListItem> lstBusinessUnits = Common.GetBussinessUnitIds(Sessions.User.ClientId).Select(p => new SelectListItem() { Text = p.Text, Value = p.Value.ToString(), Selected = false }).ToList();

            //List of Plans
            List<SelectListItem> lstPlans = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();

            if (Sessions.BusinessUnitId != Guid.Empty)
            {
                lstBusinessUnits.Where(lbu => lbu.Value == Convert.ToString(Sessions.BusinessUnitId)).ToList().ForEach(lbu => lbu.Selected = true);
                lstPlans = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Model.BusinessUnitId == Sessions.BusinessUnitId && pl.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();
            }


            if (Sessions.ReportPlanId > 0)
            {
                lstPlans.Where(lp => lp.Value == Convert.ToString(Sessions.ReportPlanId)).ToList().ForEach(lp => lp.Selected = true);
            }

            FilterDropdownValues objFilterData = new FilterDropdownValues();
            objFilterData.lstBusinessUnit = lstBusinessUnits;
            objFilterData.lstAllPlans = lstPlans;
            /* To resolve Bug 312: Report plan selector needs to be moved */

            return View(objFilterData);
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
            List<Plan_Tactic_MQL> MQLTacticList = Common.GetMQLTacticList(planTacticList);
            List<TacticDataTable> tacticdata = ((from td in db.Plan_Campaign_Program_Tactic
                                                 where planTacticList.Contains(td.PlanTacticId)
                                                 select td).ToList().Select(td => new TacticDataTable { TacticId = td.PlanTacticId, Value = MQLTacticList.Where(tm => tm.PlanTacticId == td.PlanTacticId).Select(tm => tm.MQL).SingleOrDefault(), StartMonth = td.StartDate.Month, EndMonth = td.EndDate.Month, StartYear = td.StartDate.Year, EndYear = td.EndDate.Year })).ToList();

            return GetDatatable(tacticdata);
        }

        /// <summary>
        /// Get Projected Revenue Data & Calculation.
        /// Modified By: Maninde Singh Wadhva for #426 	Conversion Reporting Page is slow to render.
        /// </summary>
        /// <param name="planTacticList">Plan tactic list.</param>
        /// <param name="isCW">Plan tactic list.</param>
        /// <returns></returns>
        public DataTable GetProjectedRevenueData(List<int> planTacticList, bool isCW = false)
        {
            List<ProjectedRevenueClass> prlist = Common.ProjectedRevenueCalculate(planTacticList, isCW);
            List<int> tacticIds = prlist.Select(tactic => tactic.PlanTacticId).ToList();
            List<TacticDataTable> tacticdata = (from t in db.Plan_Campaign_Program_Tactic.Where(tactic => tacticIds.Contains(tactic.PlanTacticId)).ToList()
                                                join p in prlist on t.PlanTacticId equals p.PlanTacticId
                                                select new TacticDataTable
                                                {
                                                    TacticId = t.PlanTacticId,
                                                    Value = p.ProjectedRevenue,
                                                    StartMonth = t.StartDate.Month,
                                                    EndMonth = t.EndDate.Month,
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
            List<TacticModelRelation> tacticModelList = Common.GetTacticModelRelation(tlist);
            List<ModelVelocityRelation> mlist = Common.GetModelVelocity(tacticModelList.Select(t => t.ModelId).Distinct().ToList(), Enums.Stage.INQ.ToString());
            List<TacticDataTable> tacticdata = (from tactic in db.Plan_Campaign_Program_Tactic.ToList()
                                                join t in tacticModelList on tactic.PlanTacticId equals t.PlanTacticId
                                                join ml in mlist on t.ModelId equals ml.ModelId
                                                where tlist.Contains(tactic.PlanTacticId)
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.PlanTacticId,
                                                    Value = tactic.INQs,
                                                    StartMonth = tactic.StartDate.AddDays(ml.Velocity).Month,
                                                    EndMonth = tactic.EndDate.AddDays(ml.Velocity).Month,
                                                    StartYear = tactic.StartDate.AddDays(ml.Velocity).Year,
                                                    EndYear = tactic.EndDate.AddDays(ml.Velocity).Year
                                                }).ToList();

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
            List<TacticModelRelation> tacticModelList = Common.GetTacticModelRelation(tlist);
            List<Plan_Tactic_MQL> MQLTacticList = Common.GetMQLTacticList(tlist);
            List<ModelVelocityRelation> mlist = Common.GetModelVelocity(tacticModelList.Select(t => t.ModelId).Distinct().ToList(), Enums.Stage.MQL.ToString());
            List<TacticDataTable> tacticdata = (from tactic in db.Plan_Campaign_Program_Tactic.ToList()
                                                join t in tacticModelList on tactic.PlanTacticId equals t.PlanTacticId
                                                join ml in mlist on t.ModelId equals ml.ModelId
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.PlanTacticId,
                                                    Value = MQLTacticList.Where(tm => tm.PlanTacticId == tactic.PlanTacticId).Select(tm => tm.MQL).SingleOrDefault(),
                                                    StartMonth = tactic.StartDate.AddDays(ml.Velocity).Month,
                                                    EndMonth = tactic.EndDate.AddDays(ml.Velocity).Month,
                                                    StartYear = tactic.StartDate.AddDays(ml.Velocity).Year,
                                                    EndYear = tactic.EndDate.AddDays(ml.Velocity).Year
                                                }).ToList();

            return GetDatatable(tacticdata);
        }

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
            List<string> monthList = GetUpToCurrentMonth();
            var campaignListFinal = campaignList.Select(p => new
            {
                Title = p.Title,
                PlanRevenue = GetProjectedRevenueData(p.planTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                ActualRevenue = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => p.planTacticList.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue),
                TrendRevenue = 0,//GetTrendRevenueDataContribution(p.planTacticList, lastMonth),
                PlanCost = GetProjectedCostData(p.planTacticList).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                ActualCost = GetProjectedCostData(p.planTacticList, false).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                TrendCost = 0,//GetTrendCostDataContribution(p.planTacticList, lastMonth),
                RunRate = GetTrendRevenueDataContribution(p.planTacticList, lastMonth, monthList),
                PipelineCoverage = 0,//GetPipelineCoverage(p.planTacticList, lastMonth),
                RevSpend = GetRevenueVSSpendContribution(p.planTacticList, selectOption, monthList),
                RevenueTotal = GetActualRevenueTotal(p.planTacticList, monthList),
                CostTotal = GetActualCostTotal(p.planTacticList, selectOption),
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
        /// Get Trend.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public double GetTrendRevenueDataContribution(List<int> planTacticList, int lastMonth, List<string> monthList)
        {
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
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
        public double GetRevenueVSSpendContribution(List<int> planTacticList, string selectOption, List<string> monthList)
        {
            List<TacticDataTable> tacticdata = (from td in db.Plan_Campaign_Program_Tactic
                                                where planTacticList.Contains(td.PlanTacticId)
                                                select new TacticDataTable { TacticId = td.PlanTacticId, Value = td.CostActual.HasValue ? (double)td.CostActual : 0, StartMonth = td.StartDate.Month, EndMonth = td.EndDate.Month, StartYear = td.StartDate.Year, EndYear = td.EndDate.Year }).ToList();

            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();

            List<string> monthWithYearList = GetUpToCurrentMonthWithYear(selectOption, true);

            double costTotal = GetDatatable(tacticdata).AsEnumerable().AsQueryable().Where(c => monthWithYearList.Contains(c.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));

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
        public double GetActualRevenueTotal(List<int> planTacticList, List<string> monthList)
        {
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
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
            // When require to use this function than change code as per latest.
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            string inq = Enums.InspectStageValues[Enums.InspectStage.INQ.ToString()].ToString();
            string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            List<string> monthList = GetUpToCurrentMonth();
            List<TacticModelRelation> tacticModelList = Common.GetTacticModelRelation(planTacticList);
            double revenueTotal = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => planTacticList.Contains(ta.PlanTacticId) && monthList.Contains(ta.Period) && ta.StageTitle == revenue).ToList().Sum(a => a.Actualvalue);

            List<ModelConvertionRateRelation> mlist = Common.GetModelConversionRate(tacticModelList.Select(t => t.ModelId).Distinct().ToList(), Enums.Stage.INQ.ToString());
            var inqlist = (from tactic in db.Plan_Campaign_Program_Tactic.ToList()
                           join t in tacticModelList on tactic.PlanTacticId equals t.PlanTacticId
                           join ml in mlist on t.ModelId equals ml.ModelId
                           select new
                           {
                               PlanTacticId = tactic.PlanTacticId,
                               INQs = tactic.Plan_Campaign_Program_Tactic_Actual.Where(pta => monthList.Contains(pta.Period) && pta.StageTitle == inq).Sum(pta => pta.Actualvalue) * ml.ConversionRate * ml.AverageDealSize
                           }).ToList();
            double inqTotal = inqlist.Sum(s => s.INQs);

            List<ModelConvertionRateRelation> mlistMQL = Common.GetModelConversionRate(tacticModelList.Select(t => t.ModelId).Distinct().ToList(), Enums.Stage.MQL.ToString());
            var mqllist = (from tactic in db.Plan_Campaign_Program_Tactic.ToList()
                           join t in tacticModelList on tactic.PlanTacticId equals t.PlanTacticId
                           join ml in mlistMQL on t.ModelId equals ml.ModelId
                           select new
                           {
                               PlanTacticId = tactic.PlanTacticId,
                               MQLs = tactic.Plan_Campaign_Program_Tactic_Actual.Where(pta => monthList.Contains(pta.Period) && pta.StageTitle == mql).Sum(pta => pta.Actualvalue) * ml.ConversionRate * ml.AverageDealSize,
                           }).ToList();
            double mqlTotal = mqllist.Sum(s => s.MQLs);

            List<ProjectedRevenueClass> prlist = Common.ProjectedRevenueCalculate(planTacticList).ToList();
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
        public double GetActualCostTotal(List<int> planTacticList, string selectOption)
        {
            List<TacticDataTable> tacticdata = (from td in db.Plan_Campaign_Program_Tactic
                                                where planTacticList.Contains(td.PlanTacticId)
                                                select new TacticDataTable { TacticId = td.PlanTacticId, Value = td.CostActual.HasValue ? (double)td.CostActual : 0, StartMonth = td.StartDate.Month, EndMonth = td.EndDate.Month, StartYear = td.StartDate.Year, EndYear = td.EndDate.Year }).ToList();

            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            List<string> monthWithYearList = GetUpToCurrentMonthWithYear(selectOption, true);

            double costTotal = GetDatatable(tacticdata).AsEnumerable().AsQueryable().Where(c => monthWithYearList.Contains(c.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));
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
            List<string> includeMonth = GetMonthList(selectOption, true);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            if (tacticIds.Count > 0)
            {
                DataTable dtActualRevenue = GetActualRevenue(tacticIds, ParentLabel, id);
                DataTable dtProjectedRevenue = GetProjectedRevenue(tacticIds, ParentLabel, id, includeMonth, selectOption);
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
        private DataTable GetProjectedRevenue(List<int> tacticIds, string ParentLabel, string id, List<string> monthList, string selectOption)
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

            //// Added By Bhavesh Dobariya
            //// PL Ticket #345
            List<ProjectedRevenueClass> tacticProjectedRevenue = Common.ProjectedRevenueCalculate(tacticIdsList);
            List<TacticModelRelation> tacticModelList = Common.GetTacticModelRelation(tacticIdsList);
            List<ModelVelocityRelation> mlist = Common.GetModelVelocity(tacticModelList.Select(t => t.ModelId).Distinct().ToList(), Enums.Stage.CW.ToString());
            List<TacticDataTable> tacticdata = (from t in db.Plan_Campaign_Program_Tactic.ToList()
                                                join l in tacticModelList on t.PlanTacticId equals l.PlanTacticId
                                                join p in tacticProjectedRevenue on t.PlanTacticId equals p.PlanTacticId
                                                join ml in mlist on l.ModelId equals ml.ModelId
                                                select new TacticDataTable
                                                {
                                                    TacticId = t.PlanTacticId,
                                                    Value = p.ProjectedRevenue,
                                                    StartMonth = t.StartDate.AddDays(ml.Velocity).Month,
                                                    EndMonth = t.EndDate.AddDays(ml.Velocity).Month,
                                                    StartYear = t.StartDate.AddDays(ml.Velocity).Year,
                                                    EndYear = t.EndDate.AddDays(ml.Velocity).Year
                                                }).ToList();
            var trevenueProjected = GetDatatable(tacticdata).AsEnumerable().AsQueryable().Where(mr => monthList.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                 {
                     PKey = g.Key,
                     PSum = g.Sum(r => r.Field<double>(ColumnValue))
                 });

            string selectedYear = DateTime.Now.Year.ToString();
            if (selectOption == Enums.UpcomingActivities.previousyear.ToString())
            {
                selectedYear = DateTime.Now.AddYears(-1).Year.ToString();
            }

            DataTable dtProjectedRevenue = GetDataTableMonthValue();
            foreach (DataRow drProjectedRevenue in dtProjectedRevenue.Rows)
            {
                drProjectedRevenue[ColumnValue] = trevenueProjected.Where(tp => tp.PKey == selectedYear + PeriodPrefix + drProjectedRevenue[ColumnMonth]).Count() > 0 ? trevenueProjected.Where(tp => tp.PKey == selectedYear + PeriodPrefix + drProjectedRevenue[ColumnMonth]).Sum(tp => tp.PSum) : 0;
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
        public JsonResult GetSourcePerformance(string selectOption = "thisyear")
        {
            return GetSourcePerformanceActual(selectOption);
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
            List<string> includeMonthUpCurrent = GetUpToCurrentMonthWithYear(selectOption, true);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            var businessUnits = db.BusinessUnits.ToList().Where(b => b.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(b => new
                                                {
                                                    Title = b.Title,
                                                    ColorCode = string.Format("#{0}", b.ColorCode),
                                                    Value = GetActualVSPlannedRevenue(db.Plan_Campaign_Program_Tactic.Where(t => t.BusinessUnitId.Equals(b.BusinessUnitId) && tacticIds.Contains(t.PlanTacticId)).Select(t => t.PlanTacticId).ToList(), includeMonthUpCurrent)
                                                    //Commentt by bhavesh PL Ticket #349 Value = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue)).Any(ta => ta.Plan_Campaign_Program_Tactic.BusinessUnitId.Equals(b.BusinessUnitId)) ? db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.Plan_Campaign_Program_Tactic.BusinessUnitId == b.BusinessUnitId && ta.StageTitle.Equals(revenue)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue) : 0
                                                });
            var vertical = db.Verticals.ToList().Where(v => v.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = GetActualVSPlannedRevenue(db.Plan_Campaign_Program_Tactic.Where(t => t.VerticalId.Equals(v.VerticalId) && tacticIds.Contains(t.PlanTacticId)).Select(t => t.PlanTacticId).ToList(), includeMonthUpCurrent)
                                                    //Commentt by bhavesh PL Ticket #349 Value = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue)).Any(ta => ta.Plan_Campaign_Program_Tactic.VerticalId.Equals(v.VerticalId)) ? db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.Plan_Campaign_Program_Tactic.VerticalId == v.VerticalId && ta.StageTitle.Equals(revenue)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue) : 0
                                                });

            var geography = db.Geographies.ToList().Where(g => g.ClientId.Equals(Sessions.User.ClientId))
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = GetActualVSPlannedRevenue(db.Plan_Campaign_Program_Tactic.Where(t => t.GeographyId.Equals(g.GeographyId) && tacticIds.Contains(t.PlanTacticId)).Select(t => t.PlanTacticId).ToList(), includeMonthUpCurrent)
                                                    //Commentt by bhavesh PL Ticket #349 Value = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue)).Any(ta => ta.Plan_Campaign_Program_Tactic.GeographyId.Equals(g.GeographyId)) ? db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) && ta.Plan_Campaign_Program_Tactic.GeographyId == g.GeographyId && ta.StageTitle.Equals(revenue)).Select(pcpt => pcpt).ToList().Where(mr => includeMonth.Contains(mr.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + mr.Period)).Sum(ta => ta.Actualvalue) : 0
                                                });
            return Json(new
            {
                ChartBusinessUnit = businessUnits,
                ChartVertical = vertical,
                ChartGeography = geography
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Actual vs Planned revenue difference %.
        /// Added By Bhavesh : PL Ticket  #349 - 16/4/2014
        /// </summary>
        /// <param name="tacticIds"></param>
        /// <param name="includeMonthUpCurrent"></param>
        /// <returns></returns>
        private double GetActualVSPlannedRevenue(List<int> tacticIds, List<string> includeMonthUpCurrent)
        {
            double actualRevenueValue = 0;
            double percentageValue = 0;
            var ActualRevenue = db.Plan_Campaign_Program_Tactic_Actual.ToList().Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString()) && includeMonthUpCurrent.Contains(pcpt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pcpt.Period)).ToList();
            if (ActualRevenue.Count() > 0)
            {
                actualRevenueValue = ActualRevenue.Sum(a => a.Actualvalue);
            }
            double projectedRevenueValue = GetProjectedRevenueData(tacticIds).AsEnumerable().AsQueryable().Where(mr => includeMonthUpCurrent.Contains(mr.Field<string>(ColumnMonth))).Sum(mr => mr.Field<double>(ColumnValue));
            if (projectedRevenueValue != 0 && actualRevenueValue >= projectedRevenueValue)
            {
                percentageValue = Math.Round(((actualRevenueValue - projectedRevenueValue) / projectedRevenueValue) * 100, 2);
            }
            return percentageValue;
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
                    startMonth = !isQuarterOnly ? 1 : 4;
                    EndMonth = 6;
                }
                else if (currentQuarter == 3)
                {
                    startMonth = !isQuarterOnly ? 4 : 7;
                    EndMonth = 9;
                }
                else
                {
                    startMonth = !isQuarterOnly ? 7 : 10;
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
        /// Get Upto Current Month List With year.
        /// </summary>
        /// <returns>list.</returns>
        private List<string> GetUpToCurrentMonthWithYear(string selectOption, bool isQuarterOnly = false)
        {
            List<string> includeMonth = new List<string>();
            int startMonth = 1, EndMonth = currentMonth;
            if (selectOption == Enums.UpcomingActivities.thisyear.ToString())
            {
                for (int i = startMonth; i <= EndMonth; i++)
                {
                    includeMonth.Add(DateTime.Now.Year.ToString() + PeriodPrefix + i);
                }
            }
            else if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
            {
                int currentQuarter = ((DateTime.Now.Month - 1) / 3) + 1;

                if (currentQuarter == 1)
                {
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
                    startMonth = !isQuarterOnly ? 1 : 4;
                }
                else if (currentQuarter == 3)
                {
                    startMonth = !isQuarterOnly ? 4 : 7;
                }
                else
                {
                    startMonth = !isQuarterOnly ? 7 : 10;
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
            List<Plan> plans = Common.GetPlan().Where(p => p.Status.Equals(PublishedPlan) &&
                                                           includeYearList.Contains(p.Year)).ToList();
            if (Sessions.ReportPlanId != 0)
            {
                plans = plans.Where(gp => gp.PlanId == Sessions.ReportPlanId).ToList();
            }
            else if (Sessions.BusinessUnitId != Guid.Empty)
            {
                plans = plans.Where(gp => gp.Model.BusinessUnitId == Sessions.BusinessUnitId).ToList();
            }

            List<int> planIds = plans.Select(p => p.PlanId).ToList();
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            return db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted == false &&
                                                              tacticStatus.Contains(t.Status) &&
                                                              planIds.Contains(t.Plan_Campaign_Program.Plan_Campaign.PlanId))
                                                   .Select(t => t.PlanTacticId).ToList();
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
                            dt.Rows.Add(t.TacticId, t.StartYear.ToString() + PeriodPrefix + i, totalValue);
                        }
                    }
                }
                else
                {
                    int totalMonth = (12 - t.StartMonth) + t.EndMonth + 1;
                    double totalValue = (double)t.Value / (double)totalMonth;
                    for (var i = t.StartMonth; i <= 12; i++)
                    {
                        dt.Rows.Add(t.TacticId, t.StartYear.ToString() + PeriodPrefix + i, totalValue);
                    }
                    for (var i = 1; i <= t.EndMonth + 1; i++)
                    {
                        dt.Rows.Add(t.TacticId, t.EndYear.ToString() + PeriodPrefix + i, totalValue);
                    }
                }
            }
            return dt;
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
            List<string> includeYearList = GetYearList(option, true);
            List<string> includeMonth = GetMonthList(option, true);
            List<int> tacticIds = GetTacticForReport(includeYearList);
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            double projectedRevenue = 0;
            double actualRevenue = 0;
            double projectedMQL = 0;
            double actualMQL = 0;
            if (tacticIds.Count > 0)
            {
                List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId) &&
                                                                                                                                includeMonth.Contains(ta.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + ta.Period))
                                                                                                                    .ToList();
                projectedRevenue = GetProjectedRevenueData(tacticIds).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));
                actualRevenue = planTacticActual.Where(ta => ta.StageTitle.Equals(revenue))
                                                .Sum(ta => ta.Actualvalue);
                projectedMQL = GetProjectedMQLData(tacticIds).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));
                actualMQL = planTacticActual.Where(ta => ta.StageTitle.Equals(mql))
                                            .Sum(ta => ta.Actualvalue);
            }

            return Json(new { ProjectedRevenueValue = projectedRevenue, ActualRevenueValue = actualRevenue, ProjectedMQLValue = Math.Round(projectedMQL), ActualMQLValue = Math.Round(actualMQL) });
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

            ////Added by :- Sohel Pathan on 27 March 2014 For Ticket #358
            if (Sessions.User != null)
            {
                individuals.Add(Sessions.User);
                individuals = individuals.OrderBy(a => a.FirstName).ToList();
            }
            ////

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
                            //// Added by Sohel on 2nd April for PL#398 to decode the optionalMessage text
                            optionalMessage = HttpUtility.UrlDecode(optionalMessage, System.Text.Encoding.Default);
                            ////
                            string emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage);

                            foreach (string toEmail in toEmailIds.Split(','))
                            {
                                Report_Share reportShare = new Report_Share();
                                reportShare.ReportType = reportType;
                                reportShare.EmailId = toEmail;
                                //// Modified by Sohel on 3rd April for PL#398 to encode the email body while inserting into DB.
                                reportShare.EmailBody = HttpUtility.HtmlEncode(emailBody);
                                ////
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

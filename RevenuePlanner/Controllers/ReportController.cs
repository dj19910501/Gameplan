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

        #region Report

        /// <summary>
        /// Report Index : This action will return the Report index page
        /// </summary>
        /// <param name="activeMenu"></param>
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.ReportView)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #519 to implement user permission Logic
        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Report)
        {
            // Added by Sohel Pathan on 27/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.TacticActualsAddEdit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);

            //if (Sessions.RolePermission != null)
            //{
            //    Common.Permission permission = Common.GetPermission(ActionItem.Report);
            //    switch (permission)
            //    {
            //        case Common.Permission.FullAccess:
            //            break;
            //        case Common.Permission.NoAccess:
            //            return RedirectToAction("Homezero", "Home");
            //        case Common.Permission.NotAnEntity:
            //            break;
            //        case Common.Permission.ViewOnly:
            //            ViewBag.IsViewOnly = "true";
            //            break;
            //    }
            //}

            ViewBag.ActiveMenu = activeMenu;

            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            string currentYear = DateTime.Now.Year.ToString();

            //List of Business Units
            List<SelectListItem> lstBusinessUnits = Common.GetBussinessUnitIds(Sessions.User.ClientId).Select(p => new SelectListItem() { Text = p.Text, Value = p.Value.ToString(), Selected = false }).ToList();

            //List of Plans
            List<SelectListItem> lstPlans = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();

            if (Sessions.PlanId != 0)
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
                Sessions.BusinessUnitId = db.Plans.Where(p => p.PlanId == Sessions.ReportPlanId).Select(p => p.Model.BusinessUnitId).SingleOrDefault();
                lstPlans = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Model.BusinessUnitId == Sessions.BusinessUnitId && pl.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();
                lstPlans.Where(lp => lp.Value == Convert.ToString(Sessions.ReportPlanId)).ToList().ForEach(lp => lp.Selected = true);
            }
            if (Sessions.BusinessUnitId != Guid.Empty)
            {
                lstBusinessUnits.Where(lbu => lbu.Value == Convert.ToString(Sessions.BusinessUnitId)).ToList().ForEach(lbu => lbu.Selected = true);
                if (Sessions.ReportPlanId <= 0)
                {
                    lstPlans = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Model.BusinessUnitId == Sessions.BusinessUnitId && pl.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();
                }
            }

            if (Sessions.ReportPlanId == 0)
            {
                ViewBag.PlanTitle = "All Plans";
                Sessions.PublishedPlanId = 0;
            }
            else
            {
                ViewBag.PlanTitle = lstPlans.Single(p => p.Value == Convert.ToString(Sessions.ReportPlanId)).Text;
            }

            FilterDropdownValues objFilterData = new FilterDropdownValues();
            objFilterData.lstBusinessUnit = lstBusinessUnits;
            objFilterData.lstAllPlans = lstPlans;

            return View("Index", objFilterData);
        }


        #endregion

        #region Summary Report

        /// <summary>
        /// This action will return the Partial View for Summary data
        /// </summary>
        /// <param name="BusinessUnitId"></param>
        /// <param name="PlanId"></param>
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.ReportView)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #519 to implement user permission Logic
        public ActionResult GetSummaryData(string BusinessUnitId = "", string PlanId = "")
        {
            SummaryReportModel objSummaryReportModel = new SummaryReportModel();
            SetReportParameter(BusinessUnitId, PlanId);
            //// Getting current year's all published plan for all business unit of clientid of director.
            var plans = Common.GetPlan();
            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            plans = plans.Where(p => p.Status.Equals(planPublishedStatus)).Select(p => p).ToList();
            //Filter to filter out the plan based on the Selected businessunit and PlanId
            if (Sessions.ReportPlanId != 0)
            {
                plans = plans.Where(p => p.PlanId.Equals(Sessions.ReportPlanId)).ToList();
            }

            if (Sessions.BusinessUnitId != Guid.Empty)
            {
                plans = plans.Where(pl => pl.Model.BusinessUnitId.Equals(Sessions.BusinessUnitId)).ToList();
            }

            List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting("", true);
            List<TacticStageValue> Tacticdata = Common.GetTacticStageRelation(tacticlist);
            TempData["ReportData"] = Tacticdata;

            Tacticdata = Tacticdata.Where(t => Convert.ToInt32(t.TacticYear) <= DateTime.Now.Year).ToList();
            List<string> yearList = Tacticdata.Select(t => t.TacticYear).Distinct().ToList();
            List<string> includeMonth = GetMonthWithYearUptoCurrentMonth(yearList);

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
            }

            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));

            overAllMQLActual = 0;
            overAllRevenueActual = 0;
            overAllMQLProjected = 0;
            overAllRevenueProjected = 0;
            var MQLActuallist = ActualTacticList.Where(pcpt => pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()) && includeMonth.Contains(Tacticdata.Single(t => t.TacticObj.PlanTacticId == pcpt.PlanTacticId).TacticYear + pcpt.Period)).ToList();
            //pt => Tacticdata.Single(t => t.TacticObj.PlanTacticId == pt.PlanTacticId).TacticYear + pt.Period
            if (MQLActuallist.Count > 0)
            {
                overAllMQLActual = MQLActuallist.Sum(t => t.Actualvalue);
            }

            var MQLProjectedlist = GetProjectedMQLValueDataTableForReport(Tacticdata).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).ToList();
            if (MQLProjectedlist.Count > 0)
            {
                overAllMQLProjected = MQLProjectedlist.Sum(mr => mr.Field<double>(ColumnValue));
            }

            var RevenueActualllist = ActualTacticList.Where(pcpt => pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString()) && includeMonth.Contains(Tacticdata.Single(t => t.TacticObj.PlanTacticId == pcpt.PlanTacticId).TacticYear + pcpt.Period)).ToList();
            if (RevenueActualllist.Count > 0)
            {
                overAllRevenueActual = RevenueActualllist.Sum(t => t.Actualvalue);
            }

            var RevenueProjectedlist = GetProjectedRevenueValueDataTableForReport(Tacticdata).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).ToList();
            if (RevenueProjectedlist.Count > 0)
            {
                overAllRevenueProjected = RevenueProjectedlist.Sum(mr => mr.Field<double>(ColumnValue));
            }

            overAllRevenueProjected = Math.Round(overAllRevenueProjected);
            overAllRevenueActual = Math.Round(overAllRevenueActual);

            // MQL
            objSummaryReportModel.MQLs = overAllMQLActual;
            objSummaryReportModel.MQLsPercentage = GetPercentageDifference(overAllMQLActual, overAllMQLProjected);

            // Actual Revenue
            objSummaryReportModel.Revenue = Convert.ToString(overAllRevenueActual);
            objSummaryReportModel.RevenuePercentage = GetPercentageDifference(overAllRevenueActual, overAllRevenueProjected);

            //// Modified By: Maninder Singh Wadhva to address TFS Bug 296:Close and realize numbers in Revenue Summary are incorrectly calculated.
            string abovePlan = "above plan";
            string belowPlan = "below plan";
            string at_par = "equal to";

            //For Revenue Summary
            objSummaryReportModel.PlanStatus = (overAllRevenueActual < overAllRevenueProjected) ? belowPlan : (overAllRevenueActual > overAllRevenueProjected) ? abovePlan : at_par;

            //// Projected Revenue
            objSummaryReportModel.ProjectedRevenue = overAllRevenueProjected;

            #region INQ
            string inq = Enums.Stage.INQ.ToString();
            int INQStageId = db.Stages.Single(s => s.ClientId == Sessions.User.ClientId && s.Code == inq).StageId;

            var INQActualList = GetActualValueForINQ(ActualTacticList, INQStageId).Where(pcta => includeMonth.Contains(Tacticdata.Single(t => t.TacticObj.PlanTacticId == pcta.PlanTacticId).TacticYear + pcta.Period)).ToList();
            if (INQActualList.Count > 0)
            {
                overAllInqActual = INQActualList.Sum(t => t.Actualvalue);
            }

            var InqProjectedList = GetConversionProjectedINQData(Tacticdata).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).ToList();
            if (InqProjectedList.Count > 0)
            {
                overAllInqProjected = InqProjectedList.Sum(mr => mr.Field<double>(ColumnValue));
            }

            double inqPercentageDifference = WaterfallGetPercentageDifference(overAllMQLActual, overAllMQLProjected, overAllInqActual, overAllInqProjected);
            objSummaryReportModel.INQPerValue = inqPercentageDifference;
            if (inqPercentageDifference < 0)
            {
                objSummaryReportModel.ISQsStatus = belowPlan;
                //.ToString("#0.##", CultureInfo.InvariantCulture) //  Comment by bhavesh to include format
            }
            else
            {
                objSummaryReportModel.ISQsStatus = abovePlan;
            }
            #endregion

            #region MQL
            var CWActualList = ActualTacticList.Where(pcpt => pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString()) && includeMonth.Contains(Tacticdata.Single(t => t.TacticObj.PlanTacticId == pcpt.PlanTacticId).TacticYear + pcpt.Period)).ToList();
            if (CWActualList.Count > 0)
            {
                overAllCWActual = CWActualList.Sum(t => t.Actualvalue);
            }

            var CwProjectedList = GetProjectedCWValueDataTableForReport(Tacticdata).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).ToList();
            if (CwProjectedList.Count > 0)
            {
                overAllCWProjected = CwProjectedList.Sum(mr => mr.Field<double>(ColumnValue));
            }

            objSummaryReportModel.MQLPerValue = WaterfallGetPercentageDifference(overAllCWActual, overAllCWProjected, overAllMQLActual, overAllMQLProjected);
            if (objSummaryReportModel.MQLPerValue < 0)
            {
                objSummaryReportModel.MQLsStatus = belowPlan;
            }
            else
            {
                objSummaryReportModel.MQLsStatus = abovePlan;
            }
            #endregion

            //#region CW
            //double cwPercentageDifference = WaterfallGetPercentageDifference(overAllCWActual, overAllCWProjected, overAllMQLActual, overAllMQLProjected);
            ////GetPercentageDifference(overAllCWActual, overAllCWProjected);
            //if (cwPercentageDifference < 0)
            //{
            //    objSummaryReportModel.OverallConversionPlanStatus = belowPlan;
            //    objSummaryReportModel.CWsStatus = string.Format("{0}% {1}", Math.Abs(cwPercentageDifference).ToString("#0.##", CultureInfo.InvariantCulture), belowPlan);
            //}
            //else
            //{
            //    objSummaryReportModel.OverallConversionPlanStatus = abovePlan;
            //    objSummaryReportModel.CWsStatus = string.Format("{0}% {1}", cwPercentageDifference.ToString("#0.##", CultureInfo.InvariantCulture), abovePlan);
            //}

            //#endregion

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

        #endregion

        #region Report General

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


        /// <summary>
        /// Set Report Parameter : Update by Bhavesh
        /// </summary>
        /// <param name="BusinessUnitId"></param>
        /// <param name="PlanId"></param>
        public void SetReportParameter(string BusinessUnitId = "", string PlanId = "")
        {
            ViewBag.IsPlanExistToShowReport = false;

            //Filter to filter out the plan based on the Selected businessunit and PlanId
            if (!string.IsNullOrEmpty(PlanId) && PlanId != "0")
            {
                int int_PlanId = Convert.ToInt32(PlanId);
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
                Sessions.BusinessUnitId = BusinessUnitGuid;
            }
            else if (BusinessUnitId == "0" && BusinessUnitId == Convert.ToString(Guid.Empty))
            {
                Sessions.BusinessUnitId = Guid.Empty;
            }
        }

        /// <summary>
        /// This method returns the list of Plan for given BusinessUnit Id (This will return only Published plan for Current year)
        /// </summary>
        /// <param name="BusinessUnitId"></param>
        /// <returns></returns>
        public JsonResult GetPlansListFromBusinessUnitId(string BusinessUnitId, bool isBusinessUnit = false)
        {
            List<SelectListItem> lstPlan = new List<SelectListItem>();
            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            string currentYear = DateTime.Now.Year.ToString();
            if (!string.IsNullOrEmpty(BusinessUnitId))
            {
                if (BusinessUnitId == "0") // All Business Units is selected
                {
                    lstPlan = Common.GetPlan().Where(pl => pl.Model.BusinessUnit.ClientId == Sessions.User.ClientId && pl.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();
                    Sessions.BusinessUnitId = Guid.Empty;
                }
                else
                {
                    Guid BUId = new Guid(BusinessUnitId);
                    lstPlan = Common.GetPlan().Where(pl => pl.Model.BusinessUnitId == BUId && pl.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();
                    Sessions.BusinessUnitId = BUId;
                }
                if (!isBusinessUnit)
                {
                if (Sessions.ReportPlanId != 0)
                {
                    lstPlan.Where(lp => Convert.ToString(lp.Value) == Convert.ToString(Sessions.ReportPlanId)).ToList().ForEach(lp => lp.Selected = true);
                    }
                }
            }
            return Json(new { lstPlan }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Function to get tactic for report.
        /// Added By Bhavesh.
        /// </summary>
        /// <returns>Returns list of Tactic Id.</returns>
        private List<Plan_Campaign_Program_Tactic> GetTacticForReporting(string selectOption = "", bool isForSummary = false)
        {
            //// Getting current year's all published plan for all business unit of clientid of director.
            List<Plan> plans = Common.GetPlan().Where(p => p.Status.Equals(PublishedPlan)).ToList();
            if (Sessions.ReportPlanId != 0)
            {
                plans = plans.Where(gp => gp.PlanId == Sessions.ReportPlanId).ToList();
            }
            else if (Sessions.BusinessUnitId != Guid.Empty)
            {
                plans = plans.Where(gp => gp.Model.BusinessUnitId == Sessions.BusinessUnitId).ToList();
            }
            if (!isForSummary)
            {
                string year = selectOption;
                if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
                {
                    year = DateTime.Now.Year.ToString();
                }
                plans = plans.Where(p => p.Year == year).ToList();
            }

            List<int> planIds = plans.Select(p => p.PlanId).ToList();
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            return db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted == false &&
                                                              tacticStatus.Contains(t.Status) &&
                                                              planIds.Contains(t.Plan_Campaign_Program.Plan_Campaign.PlanId)).ToList();
        }

        /// <summary>
        /// Get Upto Current Month List With Up to current year.
        /// </summary>
        /// <returns>list.</returns>
        private List<string> GetMonthWithYearUptoCurrentMonth(List<string> yearList)
        {
            List<string> includeMonth = new List<string>();
            int startMonth = 1, EndMonth = currentMonth;
            foreach (string year in yearList)
            {
                if (year == DateTime.Now.Year.ToString())
                {
                    for (int i = startMonth; i <= EndMonth; i++)
                    {
                        includeMonth.Add(year + PeriodPrefix + i);
                    }
                }
                else
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        includeMonth.Add(year + PeriodPrefix + i);
                    }
                }
            }

            return includeMonth;
        }

        /// <summary>
        /// Get Projected Revenue Data & Calculation.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetProjectedRevenueValueDataTableForReport(List<TacticStageValue> planTacticList)
        {
            List<TacticDataTable> tacticdata = (from t in planTacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = t.TacticObj.PlanTacticId,
                                                    Value = t.RevenueValue,
                                                    StartMonth = t.TacticObj.StartDate.Month,
                                                    EndMonth = t.TacticObj.EndDate.Month,
                                                    StartYear = t.TacticObj.StartDate.Year,
                                                    EndYear = t.TacticObj.EndDate.Year
                                                }).ToList();

            return GetDatatable(tacticdata);
        }

        /// <summary>
        /// Get Projected MQL Data & Calculation.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetProjectedMQLValueDataTableForReport(List<TacticStageValue> planTacticList)
        {
            List<TacticDataTable> tacticdata = (from t in planTacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = t.TacticObj.PlanTacticId,
                                                    Value = t.MQLValue,
                                                    StartMonth = t.TacticObj.StartDate.Month,
                                                    EndMonth = t.TacticObj.EndDate.Month,
                                                    StartYear = t.TacticObj.StartDate.Year,
                                                    EndYear = t.TacticObj.EndDate.Year
                                                }).ToList();

            return GetDatatable(tacticdata);
        }

        /// <summary>
        /// Get Projected CW Data & Calculation.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetProjectedCWValueDataTableForReport(List<TacticStageValue> planTacticList)
        {
            List<TacticDataTable> tacticdata = (from t in planTacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = t.TacticObj.PlanTacticId,
                                                    Value = t.CWValue,
                                                    StartMonth = t.TacticObj.StartDate.Month,
                                                    EndMonth = t.TacticObj.EndDate.Month,
                                                    StartYear = t.TacticObj.StartDate.Year,
                                                    EndYear = t.TacticObj.EndDate.Year
                                                }).ToList();

            return GetDatatable(tacticdata);
        }

        /// <summary>
        /// Get Projected INQ Data With Month Wise.
        /// Modified By: Maninde Singh Wadhva for #426 	Conversion Reporting Page is slow to render.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetConversionProjectedINQData(List<TacticStageValue> TacticList)
        {
            List<TacticDataTable> tacticdata = (from tactic in TacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.TacticObj.PlanTacticId,
                                                    Value = tactic.INQValue,
                                                    StartMonth = tactic.TacticObj.StartDate.Month,
                                                    EndMonth = tactic.TacticObj.EndDate.Month,
                                                    StartYear = tactic.TacticObj.StartDate.Year,
                                                    EndYear = tactic.TacticObj.EndDate.Year
                                                }).ToList();

            return GetDatatable(tacticdata);
        }

        /// <summary>
        /// Get Projected INQ Data With Month Wise.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetProjectedINQDataWithVelocity(List<TacticStageValue> tlist)
        {
            List<TacticDataTable> tacticdata = (from tactic in tlist
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.TacticObj.PlanTacticId,
                                                    Value = tactic.INQValue,
                                                    StartMonth = tactic.TacticObj.StartDate.AddDays(tactic.INQVelocity).Month,
                                                    EndMonth = tactic.TacticObj.EndDate.AddDays(tactic.INQVelocity).Month,
                                                    StartYear = tactic.TacticObj.StartDate.AddDays(tactic.INQVelocity).Year,
                                                    EndYear = tactic.TacticObj.EndDate.AddDays(tactic.INQVelocity).Year
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
        public DataTable GetProjectedMQLDataWithVelocity(List<TacticStageValue> tlist)
        {
            List<TacticDataTable> tacticdata = (from tactic in tlist
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.TacticObj.PlanTacticId,
                                                    Value = tactic.MQLValue,
                                                    StartMonth = tactic.TacticObj.StartDate.AddDays(tactic.MQLVelocity).Month,
                                                    EndMonth = tactic.TacticObj.EndDate.AddDays(tactic.MQLVelocity).Month,
                                                    StartYear = tactic.TacticObj.StartDate.AddDays(tactic.MQLVelocity).Year,
                                                    EndYear = tactic.TacticObj.EndDate.AddDays(tactic.MQLVelocity).Year
                                                }).ToList();

            return GetDatatable(tacticdata);
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
                percentage = ((actual - projected) / projected) * 100;    //// Modified by :- Sohel on 09/05/2014 for PL #474 to corrrect the calcualtion formula
            }
            else if (actual != 0)
            {
                percentage = 100;
            }

            return percentage;
        }

        /// <summary>
        /// Get Year base on select option.
        /// </summary>
        /// <param name="selectOption">select Option.</param>
        /// <param name="isQuarterOnly">isQuarter Only.</param>
        /// <returns>List of Year.</returns>
        public List<string> GetYearListForReport(string selectOption, bool isQuarterOnly = false)
        {
            List<string> includeYearList = new List<string>();
            if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
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
            else
            {
                includeYearList.Add(selectOption);
            }
            return includeYearList;
        }

        /// <summary>
        /// Get Month Based on Select Option.
        /// </summary>
        /// <param name="selectOption">select Option.</param>
        /// <param name="isQuarterOnly">isQuarter Only.</param>
        /// <returns>List of Month.</returns>
        public List<string> GetMonthListForReport(string selectOption, bool isQuarterOnly = false)
        {
            List<string> includeMonth = new List<string>();
            int startMonth = 1, EndMonth = 12;
            if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
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
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    includeMonth.Add(selectOption + PeriodPrefix + i);
                }
            }
            return includeMonth;
        }

        /// <summary>
        /// Get Month list for Display.
        /// </summary>
        /// <param name="selectOption">select Option.</param>
        /// <returns>list of Month.</returns>
        private List<string> GetDisplayMonthListForReport(string selectOption)
        {
            List<string> lmtitle = new List<string>();
            if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
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
            else
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
        /// Get Upto Current Month List With year.
        /// </summary>
        /// <returns>list.</returns>
        private List<string> GetUpToCurrentMonthWithYearForReport(string selectOption, bool isQuarterOnly = false)
        {
            List<string> includeMonth = new List<string>();
            int startMonth = 1, EndMonth = currentMonth;
            if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
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
            else if (Convert.ToInt32(selectOption) == DateTime.Now.Year)
            {
                for (int i = startMonth; i <= EndMonth; i++)
                {
                    includeMonth.Add(selectOption + PeriodPrefix + i);
                }
            }
            else if (Convert.ToInt32(selectOption) < DateTime.Now.Year)
            {
                for (int i = 1; i <= 12; i++)
                {
                    includeMonth.Add(selectOption + PeriodPrefix + i);
                }
            }

            return includeMonth;
        }

        /// <summary>
        /// Get Upcoming Activity for Report Header.
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetUpcomingActivityForReport(bool isBusinessUnit = false)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            var plans = Common.GetPlan();
            if (Sessions.BusinessUnitId != Guid.Empty)
            {
                plans = plans.Where(p => p.Model.BusinessUnitId == Sessions.BusinessUnitId).ToList();
            }
            var yearlistPrevious = plans.Where(p => p.Year != DateTime.Now.Year.ToString() && p.Year != DateTime.Now.AddYears(-1).Year.ToString() && Convert.ToInt32(p.Year) < DateTime.Now.AddYears(-1).Year && p.Status.Equals(planPublishedStatus)).Select(p => p.Year).Distinct().OrderBy(p => p).ToList();
            var yearlistAfter = plans.Where(p => p.Year != DateTime.Now.Year.ToString() && p.Year != DateTime.Now.AddYears(-1).Year.ToString() && Convert.ToInt32(p.Year) > DateTime.Now.Year && p.Status.Equals(planPublishedStatus)).Select(p => p.Year).Distinct().OrderBy(p => p).ToList();

            yearlistPrevious.ForEach(p => items.Add(new SelectListItem { Text = p, Value = p, Selected = false }));

            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.lastyear.ToString()].ToString(), Value = DateTime.Now.AddYears(-1).Year.ToString(), Selected = false });
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thisyear.ToString()].ToString(), Value = DateTime.Now.Year.ToString(), Selected = false });
            items.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thisquarter.ToString()].ToString(), Value = Enums.UpcomingActivities.thisquarter.ToString(), Selected = false });

            yearlistAfter.ForEach(p => items.Add(new SelectListItem { Text = p, Value = p, Selected = false }));
            //if (!isBusinessUnit && Sessions.ReportPlanId != 0)
            if (Sessions.BusinessUnitId != Guid.Empty && Sessions.ReportPlanId != 0)
            {
                string year = plans.Single(p => p.PlanId == Sessions.ReportPlanId).Year;
                items.Where(lp => Convert.ToString(lp.Value) == year).ToList().ForEach(lp => lp.Selected = true);
            }
            else
            {
                items.Where(lp => Convert.ToString(lp.Value) == DateTime.Now.Year.ToString()).ToList().ForEach(lp => lp.Selected = true);
            }


            return items;
        }

        /// <summary>
        /// Load TimeFrame values
        /// </summary>
        /// <param name="BusinessUnitId"></param>
        /// <param name="isBusinessUnit"></param>
        /// <returns></returns>
        public JsonResult GetTimeFrameValues(string BusinessUnitId = "", bool isBusinessUnit = false)
        {
            if (!string.IsNullOrEmpty(BusinessUnitId) && BusinessUnitId != "0" && BusinessUnitId != Convert.ToString(Guid.Empty))
            {
                Guid BusinessUnitGuid = new Guid(BusinessUnitId);
                if (Sessions.BusinessUnitId != BusinessUnitGuid)
                {
                Sessions.BusinessUnitId = BusinessUnitGuid;
                    Sessions.ReportPlanId = 0;
                }
            }
            else if (BusinessUnitId == "0" || BusinessUnitId == Convert.ToString(Guid.Empty))
            {
                Sessions.BusinessUnitId = Guid.Empty;
                if (isBusinessUnit)
                {
                    Sessions.ReportPlanId = 0;
                }
            }

            var upcomingList = GetUpcomingActivityForReport(isBusinessUnit).Select(p => new
            {
                Text = p.Text,
                Value = p.Value.ToString(),
                Selected = p.Selected
            }).ToList();

            return Json(upcomingList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This method returns the list of Plan for given BusinessUnit Id (This will return only Published plan for Current year)
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPlansListFromTimeFrame(string timeFrameOption)
        {
            string filterYear = timeFrameOption;
            if (timeFrameOption == Enums.UpcomingActivities.thisquarter.ToString())
            {
                filterYear = DateTime.Now.Year.ToString();
            }
            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            var plans = Common.GetPlan().Where(p => p.Year == filterYear && p.Status.Equals(planPublishedStatus)).ToList();
            if (Sessions.BusinessUnitId != Guid.Empty)
            {
                plans = plans.Where(p => p.Model.BusinessUnitId == Sessions.BusinessUnitId).ToList();
            }

            List<SelectListItem> lstPlan = new List<SelectListItem>();
            lstPlan = plans.Select(p => new SelectListItem() { Text = p.Title, Value = Convert.ToString(p.PlanId), Selected = false }).ToList();

            if (Sessions.ReportPlanId != 0)
            {
                lstPlan.Where(lp => Convert.ToString(lp.Value) == Convert.ToString(Sessions.ReportPlanId)).ToList().ForEach(lp => lp.Selected = true);
            }   

            return Json(new { lstPlan }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Child Tab Title As per Selection.
        /// </summary>
        /// <param name="ParentLabel">ParentLabel.</param>
        /// <returns>jsonResult</returns>
        public JsonResult GetChildLabelData(string ParentLabel, string selectOption = "")
        {
            if (ParentLabel == Common.RevenueBusinessUnit)
            {
                var returnData = (db.BusinessUnits.Where(bu => bu.ClientId.Equals(Sessions.User.ClientId) && bu.IsDeleted.Equals(false)).Select(b => new
                {
                    id = b.BusinessUnitId,
                    title = b.Title
                }).Select(b => b).Distinct().OrderBy(b => b.title)).ToList();

                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel == Common.RevenueAudience)
            {
                var returnData = (db.Audiences.Where(au => au.ClientId == Sessions.User.ClientId && au.IsDeleted == false).Select(a => new
                {
                    id = a.AudienceId,
                    title = a.Title
                }).Select(a => a).Distinct().OrderBy(a => a.title)).ToList();

                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel == Common.RevenueGeography)
            {
                var returnData = (db.Geographies.Where(ge => ge.ClientId.Equals(Sessions.User.ClientId) && ge.IsDeleted.Equals(false)).Select(g => new
                {
                    id = g.GeographyId,
                    title = g.Title
                }).Select(g => g).Distinct().OrderBy(g => g.title)).ToList();

                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel == Common.RevenueVertical)
            {
                var returnData = (db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted == false).Select(v => new
                {
                    id = v.VerticalId,
                    title = v.Title
                }).Select(v => v).Distinct().OrderBy(v => v.title)).ToList();

                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel == Common.RevenuePlans)
            {
                string year = selectOption;
                var plans = db.Plans.Where(p => p.Model.BusinessUnit.ClientId.Equals(Sessions.User.ClientId) && p.IsDeleted.Equals(false) && p.Status == PublishedPlan).ToList();
                if (Sessions.BusinessUnitId != Guid.Empty)
                {
                    plans = plans.Where(p => p.Model.BusinessUnitId == Sessions.BusinessUnitId).ToList();
                }
                if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
                {
                    year = DateTime.Now.Year.ToString();
                }

                var returnData = plans.Where(p => p.Year == year).Select(p => new
                {
                    id = p.PlanId,
                    title = p.Title
                }).Select(b => b).Distinct().OrderBy(b => b.title).ToList();

                return Json(returnData, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Report Header Data.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public JsonResult GetReportHeader(string option, bool isRevenue = true)
        {
            List<string> includeYearList = GetYearListForReport(option, true);
            List<string> includeMonth = GetMonthListForReport(option, true);

            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            List<int> TacticIds = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            double projectedRevenue = 0;
            double actualRevenue = 0;
            double projectedMQL = 0;
            double actualMQL = 0;
            if (Tacticdata.Count > 0)
            {
                List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => TacticIds.Contains(ta.PlanTacticId) &&
                                                                                                                                includeMonth.Contains(ta.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + ta.Period))
                                                                                                                    .ToList();
                if (isRevenue)
                {
                    projectedRevenue = GetProjectedRevenueValueDataTableForReport(Tacticdata).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));
                    actualRevenue = planTacticActual.Where(ta => ta.StageTitle.Equals(revenue))
                                                    .Sum(ta => ta.Actualvalue);
                }
                else
                {
                    projectedMQL = GetProjectedMQLValueDataTableForReport(Tacticdata).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));
                    actualMQL = planTacticActual.Where(ta => ta.StageTitle.Equals(mql))
                                                .Sum(ta => ta.Actualvalue);
                }
            }

            return Json(new { ProjectedRevenueValue = projectedRevenue, ActualRevenueValue = actualRevenue, ProjectedMQLValue = Math.Round(projectedMQL), ActualMQLValue = Math.Round(actualMQL) });
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

        #endregion

        #region Conversion Summary

        /// <summary>
        /// Returns view of Conversion report
        /// </summary>
        /// <param name="BusinessUnitId"></param>
        /// <param name="PlanId"></param>
        /// <param name="timeFrameOption"></param>
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.ReportView)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #519 to implement user permission Logic
        public ActionResult GetConversionData(string BusinessUnitId = "", string PlanId = "", string timeFrameOption = "thisquarter")
        {
            //if (Sessions.RolePermission != null)
            //{
            //    Common.Permission permission = Common.GetPermission(ActionItem.Report);
            //    switch (permission)
            //    {
            //        case Common.Permission.FullAccess:
            //            break;
            //        case Common.Permission.NoAccess:
            //            return RedirectToAction("Index", "NoAccess");
            //        case Common.Permission.NotAnEntity:
            //            break;
            //        case Common.Permission.ViewOnly:
            //            ViewBag.IsViewOnly = "true";
            //            break;
            //    }
            //}

            SetReportParameter(BusinessUnitId, PlanId);
            ViewBag.MonthTitle = GetDisplayMonthListForReport(timeFrameOption);
            ViewBag.SelectOption = timeFrameOption;
            List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting(timeFrameOption);
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist);
            TempData["ReportData"] = tacticStageList;

            return PartialView("Conversion");
        }

        #region MQL Conversion Plan Report

        /// <summary>
        /// This method returns the data for MQL Conversion plan summary report
        /// </summary>
        /// <param name="ParentTab"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult GetMQLConversionPlanData(string ParentTab = "", string Id = "", string selectOption = "")
        {
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            List<string> includeYearList = GetYearListForReport(selectOption);
            List<string> includeMonth = GetMonthListForReport(selectOption);

            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            if (ParentTab == Common.BusinessUnit)
            {
                Guid buid = new Guid(Id);
                Tacticdata = Tacticdata.Where(pcpt => pcpt.TacticObj.BusinessUnitId == buid).ToList();
            }
            else if (ParentTab == Common.Audience)
            {
                int auid = Convert.ToInt32(Id);
                Tacticdata = Tacticdata.Where(pcpt => pcpt.TacticObj.AudienceId == auid).ToList();
            }
            else if (ParentTab == Common.Geography)
            {
                Guid geographyId = new Guid(Id);
                Tacticdata = Tacticdata.Where(pcpt => pcpt.TacticObj.GeographyId == geographyId).ToList();
            }
            else if (ParentTab == Common.Vertical)
            {
                int verticalid = Convert.ToInt32(Id);
                Tacticdata = Tacticdata.Where(pcpt => pcpt.TacticObj.VerticalId == verticalid).ToList();
            }
            string inq = Enums.Stage.INQ.ToString();
            int INQStageId = db.Stages.Single(s => s.ClientId == Sessions.User.ClientId && s.Code == inq).StageId;
            if (Tacticdata.Count() > 0)
            {
                string inspectStageINQ = Enums.InspectStageValues[Enums.InspectStage.INQ.ToString()].ToString();
                string inspectStageMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();

                List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = new List<Plan_Campaign_Program_Tactic_Actual>();
                Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => planTacticActual.Add(a)));
                planTacticActual = planTacticActual.Where(mr => includeMonth.Contains((Tacticdata.Single(t => t.TacticObj.PlanTacticId == mr.PlanTacticId).TacticYear) + mr.Period)).ToList();

                var rdata = new[] { new { 
                INQGoal = GetConversionProjectedINQData(Tacticdata).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                monthList = includeMonth,
                INQActual = GetActualValueForINQ(planTacticActual,INQStageId).GroupBy(pt => Tacticdata.Single(t => t.TacticObj.PlanTacticId == pt.PlanTacticId).TacticYear + pt.Period)
                                             .Select(pcptj => new
                                              {
                                                PKey = pcptj.Key,
                                                PSum = pcptj.Sum(pt => pt.Actualvalue)
                                              }),
                MQLGoal = GetProjectedMQLValueDataTableForReport(Tacticdata).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                MQLActual = planTacticActual.Where(pcpt => pcpt.StageTitle.Equals(inspectStageMQL))
                            .GroupBy(pt => Tacticdata.Single(t => t.TacticObj.PlanTacticId == pt.PlanTacticId).TacticYear + pt.Period)
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
        public JsonResult GetMQLPerformance(string filter, string selectOption = "")
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
        /// Modified By: Maninde Singh Wadhva for #426 	Conversion Reporting Page is slow to render.
        /// </summary>
        /// <returns>Returns json result of source perfromance trend.</returns>
        private JsonResult GetMQLPerformanceTrend(string selectOption)
        {
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            List<string> includeYearList = GetYearListForReport(selectOption, true);
            List<string> months = GetUpToCurrentMonth();

            int lastMonth = GetLastMonthForTrend(selectOption);
            List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => planTacticActual.Add(a)));

            //var planCampaignTacticActualAll = db.Plan_Campaign_Program_Tactic_Actual.Where(tactic => TacticIds.Contains(tactic.PlanTacticId));

            var tacticTrenBusinessUnit = planTacticActual.Where(ta => months.Contains(ta.Period) &&
                                                                (ta.StageTitle == mql))
                                                   .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.BusinessUnitId)
                                                   .Select(ta => new
                                                   {
                                                       BusinessUnitId = ta.Key,
                                                       Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth)
                                                   });

            var tacticTrendGeography = planTacticActual.Where(ta => months.Contains(ta.Period) &&
                                                     (ta.StageTitle == mql))
                                        .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.GeographyId)
                                        .Select(ta => new
                                        {
                                            GeographyId = ta.Key,
                                            Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth)
                                        });

            var tacticTrendVertical = planTacticActual.Where(ta => months.Contains(ta.Period) &&
                                         (ta.StageTitle == mql))
                            .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.VerticalId)
                            .Select(ta => new
                            {
                                VerticalId = ta.Key,
                                Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth)
                            });
            //Start : Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
            var businessUnits = db.BusinessUnits.Where(b => b.ClientId == Sessions.User.ClientId && b.IsDeleted==false).ToList()
                                           .Select(b => new
                                           {
                                               Title = b.Title,
                                               ColorCode = string.Format("#{0}", b.ColorCode),
                                               Value = tacticTrenBusinessUnit.Any(bu => bu.BusinessUnitId.Equals(b.BusinessUnitId)) ? tacticTrenBusinessUnit.Where(bu => bu.BusinessUnitId.Equals(b.BusinessUnitId)).First().Trend : 0

                                           });
            var vertical = db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted==false).ToList()
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = tacticTrendVertical.Any(ve => ve.VerticalId.Equals(v.VerticalId)) ? tacticTrendVertical.Where(ve => ve.VerticalId.Equals(v.VerticalId)).First().Trend : 0
                                                });

            var geography = db.Geographies.Where(g => g.ClientId == Sessions.User.ClientId && g.IsDeleted==false).ToList()
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = tacticTrendGeography.Any(ge => ge.GeographyId.Equals(g.GeographyId)) ? tacticTrendGeography.Where(ge => ge.GeographyId.Equals(g.GeographyId)).First().Trend : 0
                                                });
            //End : Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag 
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
        /// Modified By: Maninde Singh Wadhva for #426 	Conversion Reporting Page is slow to render.
        /// </summary>
        /// <returns>Returns json result of source perfromance actual.</returns>
        private JsonResult GetMQLPerformanceActual(string selectOption)
        {
            List<string> includeYearList = GetYearListForReport(selectOption, true);
            List<string> includeMonth = GetMonthListForReport(selectOption, true);
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            List<Plan_Campaign_Program_Tactic_Actual> planTacticActuals = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => planTacticActuals.Add(a)));

            //List<Plan_Campaign_Program_Tactic_Actual> planTacticActuals = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => tacticIds.Contains(ta.PlanTacticId)).ToList();
            //Start : Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
            var businessUnits = db.BusinessUnits.Where(b => b.ClientId == Sessions.User.ClientId && b.IsDeleted==false).ToList()
                                                .Select(b => new
                                                {
                                                    Title = b.Title,
                                                    ColorCode = string.Format("#{0}", b.ColorCode),
                                                    Value = planTacticActuals.Any(ta => ta.StageTitle.Equals(mql) &&
                                                                                        ta.Plan_Campaign_Program_Tactic.BusinessUnitId.Equals(b.BusinessUnitId)) ?
                                                            planTacticActuals.Where(ta => ta.Plan_Campaign_Program_Tactic.BusinessUnitId == b.BusinessUnitId &&
                                                                                          ta.StageTitle.Equals(mql) &&
                                                                                          includeMonth.Contains(ta.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + ta.Period))
                                                                             .Sum(ta => ta.Actualvalue) :
                                                                             0
                                                });
            var vertical = db.Verticals.ToList().Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted==false).ToList()
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = planTacticActuals.Any(ta => ta.StageTitle.Equals(mql) &&
                                                                                        ta.Plan_Campaign_Program_Tactic.VerticalId.Equals(v.VerticalId)) ?
                                                            planTacticActuals.Where(ta => ta.Plan_Campaign_Program_Tactic.VerticalId == v.VerticalId &&
                                                                                          ta.StageTitle.Equals(mql) &&
                                                                                          includeMonth.Contains(ta.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + ta.Period))
                                                                             .Sum(ta => ta.Actualvalue) :
                                                                             0
                                                });

            var geography = db.Geographies.ToList().Where(g => g.ClientId == Sessions.User.ClientId && g.IsDeleted==false).ToList()
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = planTacticActuals.Any(ta => ta.StageTitle.Equals(mql) &&
                                                                                        ta.Plan_Campaign_Program_Tactic.GeographyId.Equals(g.GeographyId)) ?
                                                            planTacticActuals.Where(ta => ta.Plan_Campaign_Program_Tactic.GeographyId == g.GeographyId &&
                                                                                          ta.StageTitle.Equals(mql) &&
                                                                                          includeMonth.Contains(ta.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + ta.Period))
                                                                             .Sum(ta => ta.Actualvalue) :
                                                                             0
                                                });
            //End : Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
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
        /// Modified By: Maninde Singh Wadhva for #426 	Conversion Reporting Page is slow to render.
        /// </summary>
        /// <returns>Returns json result of source perfromance projected.</returns>
        private JsonResult GetMQLPerformanceProjected(string selectOption)
        {
            List<string> includeYearList = GetYearListForReport(selectOption, true);
            List<string> includeMonth = GetMonthListForReport(selectOption, true);
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];
            //// Applying filters i.e. bussiness unit, audience, vertical or geography.
            //Start : Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
            var businessUnits = db.BusinessUnits.Where(b => b.ClientId == Sessions.User.ClientId && b.IsDeleted==false).ToList()
                                                .Select(b => new
                                                {
                                                    Title = b.Title,
                                                    ColorCode = string.Format("#{0}", b.ColorCode),
                                                    Value = Tacticdata.Any(t => t.TacticObj.BusinessUnitId.Equals(b.BusinessUnitId)) ?
                                                            GetProjectedMQLValueDataTableForReport(Tacticdata.Where(t => t.TacticObj.BusinessUnitId.Equals(b.BusinessUnitId))
                                                                                        .ToList())
                                                                                        .AsEnumerable()
                                                                                        .AsQueryable()
                                                                                        .Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth)))
                                                                                        .Sum(r => r.Field<double>(ColumnValue)) :
                                                                                        0
                                                });



            var vertical = db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted==false).ToList()
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = Tacticdata.Any(t => t.TacticObj.VerticalId.Equals(v.VerticalId)) ?
                                                            GetProjectedMQLValueDataTableForReport(Tacticdata.Where(t => t.TacticObj.VerticalId.Equals(v.VerticalId))
                                                                                       .ToList())
                                                                                       .AsEnumerable()
                                                                                       .AsQueryable()
                                                                                       .Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth)))
                                                                                       .Sum(r => r.Field<double>(ColumnValue)) :
                                                                                       0
                                                });

            var geography = db.Geographies.Where(g => g.ClientId == Sessions.User.ClientId && g.IsDeleted==false).ToList()
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = Tacticdata.Any(t => t.TacticObj.GeographyId.Equals(g.GeographyId)) ?
                                                    GetProjectedMQLValueDataTableForReport(Tacticdata.Where(t => t.TacticObj.GeographyId.Equals(g.GeographyId))
                                                                               .ToList())
                                                                               .AsEnumerable()
                                                                               .AsQueryable()
                                                                               .Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth)))
                                                                               .Sum(r => r.Field<double>(ColumnValue)) :
                                                                               0
                                                });
            //End : Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
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
        public JsonResult GetConversionSummary(string ParentConversionSummaryTab = "", string selectOption = "")
        {
            List<string> includeYearList = GetYearListForReport(selectOption);
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            Tacticdata = Tacticdata.Where(pcpt =>
                ((ParentConversionSummaryTab == Common.BusinessUnit && pcpt.TacticObj.BusinessUnit.ClientId == Sessions.User.ClientId) ||
                (ParentConversionSummaryTab == Common.Audience && pcpt.TacticObj.Audience.ClientId == Sessions.User.ClientId) ||
                (ParentConversionSummaryTab == Common.Geography && pcpt.TacticObj.Geography.ClientId == Sessions.User.ClientId) ||
                (ParentConversionSummaryTab == Common.Vertical && pcpt.TacticObj.Vertical.ClientId == Sessions.User.ClientId))
                ).ToList();

            var DataTitleList = Tacticdata.GroupBy(pc => new { title = pc.TacticObj.BusinessUnit.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.TacticObj.PlanTacticId).ToList()
                         }).ToList();

            if (ParentConversionSummaryTab == Common.RevenueVertical)
            {
                DataTitleList = Tacticdata.GroupBy(pc => new { title = pc.TacticObj.Vertical.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.TacticObj.PlanTacticId).ToList()
                         }).ToList();
            }
            else if (ParentConversionSummaryTab == Common.RevenueGeography)
            {
                DataTitleList = Tacticdata.GroupBy(pc => new { title = pc.TacticObj.Geography.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.TacticObj.PlanTacticId).ToList()
                         }).ToList();
            }
            else if (ParentConversionSummaryTab == Common.RevenueAudience)
            {
                DataTitleList = Tacticdata.GroupBy(pc => new { title = pc.TacticObj.Audience.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.TacticObj.PlanTacticId).ToList()
                         }).ToList();
            }
            List<string> includeMonth = GetMonthListForReport(selectOption, true);
            string stageTitleINQ = Enums.InspectStage.INQ.ToString();
            string stageTitleMQL = Enums.InspectStage.MQL.ToString();
            string stageTitleCW = Enums.InspectStage.CW.ToString();
            string stageTitleRevenue = Enums.InspectStage.Revenue.ToString();
            string marketing = Enums.Funnel.Marketing.ToString();

            List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => planTacticActual.Add(a)));
            planTacticActual = planTacticActual.Where(mr => includeMonth.Contains((Tacticdata.Single(t => t.TacticObj.PlanTacticId == mr.PlanTacticId).TacticYear) + mr.Period)).ToList();
            string inq = Enums.Stage.INQ.ToString();
            int INQStageId = db.Stages.Single(s => s.ClientId == Sessions.User.ClientId && s.Code == inq).StageId;
            var DataListFinal = DataTitleList.Select(p => new
            {
                Title = p.Title,
                INQ = GetActualValueForINQ(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), INQStageId).Sum(a => a.Actualvalue),
                MQL = GetActualValueForConversionSummary(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleMQL),
                ActualCW = GetActualValueForConversionSummary(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleCW),
                ActualRevenue = GetActualValueForConversionSummary(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleRevenue),
                ActualADS = CalculateActualADS(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleCW, stageTitleRevenue),
                ProjectedCW = GetProjectedCWValueDataTableForReport(Tacticdata.Where(t => p.planTacticList.Contains(t.TacticObj.PlanTacticId)).ToList()).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                ProjectedRevenue = GetProjectedRevenueValueDataTableForReport(Tacticdata.Where(t => p.planTacticList.Contains(t.TacticObj.PlanTacticId)).ToList()).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
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


        /// <summary>
        /// Get Actual value based on stagetitle
        /// Added by Bhavesh
        /// Modified By: Maninde Singh Wadhva for #426 	Conversion Reporting Page is slow to render.
        /// </summary>
        /// <param name="planTacticActual">Plan tactic actual</param>
        /// <param name="stagetitle">Stage title.</param>
        /// <returns>Returns actual value for conversion summary.</returns>
        private List<Plan_Campaign_Program_Tactic_Actual> GetActualValueForINQ(List<Plan_Campaign_Program_Tactic_Actual> planTacticActual, int stageId)
        {
            string projectedStageValue = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
            planTacticActual = planTacticActual.Where(pta => pta.Plan_Campaign_Program_Tactic.StageId == stageId && pta.StageTitle == projectedStageValue).ToList();
            return planTacticActual;
        }

        #endregion

        #endregion

        #region Revenue

        /// <summary>
        /// Return Revenue Partial View
        /// </summary>
        /// <param name="BusinessUnitId"></param>
        /// <param name="PlanId"></param>
        /// <param name="timeFrameOption"></param>
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.ReportView)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #519 to implement user permission Logic
        public ActionResult GetRevenueData(string BusinessUnitId = "", string PlanId = "", string timeFrameOption = "thisquarter")
        {
            //if (Sessions.RolePermission != null)
            //{
            //    Common.Permission permission = Common.GetPermission(ActionItem.Report);
            //    switch (permission)
            //    {
            //        case Common.Permission.FullAccess:
            //            break;
            //        case Common.Permission.NoAccess:
            //            return RedirectToAction("Index", "NoAccess");
            //        case Common.Permission.NotAnEntity:
            //            break;
            //        case Common.Permission.ViewOnly:
            //            ViewBag.IsViewOnly = "true";
            //            break;
            //    }
            //}

            SetReportParameter(BusinessUnitId, PlanId);
            ViewBag.MonthTitle = GetDisplayMonthListForReport(timeFrameOption);
            ViewBag.SelectOption = timeFrameOption;

            ViewBag.BusinessUnit = db.BusinessUnits.Where(b => b.ClientId == Sessions.User.ClientId && b.IsDeleted == false).OrderBy(b => b.Title);//Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
            List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting(timeFrameOption);
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist);
            TempData["ReportData"] = tacticStageList;

            return PartialView("Revenue");
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
        public JsonResult GetRevenueSummaryDataRevenueReport(string ParentLabel, string id, string selectOption)
        {
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            List<string> includeYearList = GetYearListForReport(selectOption);
            List<string> includeMonth = GetMonthListForReport(selectOption);

            Tacticdata = Tacticdata.Where(pcpt => ((ParentLabel == Common.RevenueBusinessUnit && pcpt.TacticObj.BusinessUnitId == new Guid(id)) ||
                (ParentLabel == Common.RevenueGeography && pcpt.TacticObj.GeographyId == new Guid(id)) ||
                (ParentLabel == Common.RevenuePlans && pcpt.TacticObj.Plan_Campaign_Program.Plan_Campaign.PlanId == Convert.ToInt32(id)))).ToList();
            List<int> tacticIdList = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();

            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));
            ActualTacticList = ActualTacticList.Where(mr => includeMonth.Contains((Tacticdata.Single(t => t.TacticObj.PlanTacticId == mr.PlanTacticId).TacticYear) + mr.Period)).ToList();

            var campaignListobj = Tacticdata.GroupBy(pc => new { PCid = pc.TacticObj.Plan_Campaign_Program.PlanCampaignId, title = pc.TacticObj.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pc =>
                        new CampaignData
                        {
                            PlanCampaignId = pc.Key.PCid,
                            Title = pc.Key.title,
                            planTacticList = pc.Select(p => p.TacticObj.PlanTacticId).ToList()
                        }).ToList();

            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            DataTable ProjectedRevenueDatatable = GetProjectedRevenueValueDataTableForReport(Tacticdata);
            DataTable MQLDatatable = GetProjectedMQLValueDataTableForReport(Tacticdata);
            var campaignList = campaignListobj.Select(p => new
            {
                id = p.PlanCampaignId,
                title = p.Title,
                monthList = includeMonth,
                trevenueProjected = ProjectedRevenueDatatable.AsEnumerable().AsQueryable().Where(mr => p.planTacticList.Contains(mr.Field<int>(ColumnId)) && includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                tproject = MQLDatatable.AsEnumerable().AsQueryable().Where(mr => p.planTacticList.Contains(mr.Field<int>(ColumnId)) && includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                tRevenueActual = ActualTacticList.Where(pcpt => p.planTacticList.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(revenue)).GroupBy(pt => Tacticdata.Single(t => t.TacticObj.PlanTacticId == pt.PlanTacticId).TacticYear + pt.Period).Select(pcptj => new
                {
                    key = pcptj.Key,
                    ActualValue = pcptj.Sum(pt => pt.Actualvalue)
                }),
                tacticActual = ActualTacticList.Where(pcpt => p.planTacticList.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(mql)).GroupBy(pt => Tacticdata.Single(t => t.TacticObj.PlanTacticId == pt.PlanTacticId).TacticYear + pt.Period).Select(pcptj => new
                {
                    key = pcptj.Key,
                    ActualValue = pcptj.Sum(pt => pt.Actualvalue)
                }).Select(pcptj => pcptj),
            }).Select(p => p).Distinct().OrderBy(p => p.id);

            return Json(campaignList, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Revenue Realization

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Campaign List.
        /// </summary>
        /// <param name="id"> Id</param>
        /// <returns>Returns Json Result of Campaign List.</returns>
        public JsonResult LoadCampaignDropDown(Guid id, string selectOption = "")
        {
            List<string> includeYearList = GetYearListForReport(selectOption);
            if (Sessions.ReportPlanId > 0)
            {
                var campaignList = db.Plan_Campaign.Where(pc => pc.Plan.Model.BusinessUnitId.Equals(id) && pc.Plan.IsDeleted == false && pc.Plan.Status == PublishedPlan && includeYearList.Contains(pc.Plan.Year) && pc.IsDeleted == false && pc.PlanId == Sessions.ReportPlanId)
                    .Select(pcp => new { PlanCampaignId = pcp.PlanCampaignId, Title = pcp.Title })
                    .OrderBy(pcp => pcp.Title);
                if (campaignList == null)
                    return Json(new { });
                return Json(campaignList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var campaignList = db.Plan_Campaign.Where(pc => pc.Plan.Model.BusinessUnitId.Equals(id) && pc.Plan.IsDeleted == false && pc.Plan.Status == PublishedPlan && includeYearList.Contains(pc.Plan.Year) && pc.IsDeleted == false)
                    .Select(pcp => new { PlanCampaignId = pcp.PlanCampaignId, Title = pcp.Title })
                    .OrderBy(pcp => pcp.Title);
                if (campaignList == null)
                    return Json(new { });
                return Json(campaignList, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Program List.
        /// </summary>
        /// <param name="id"> Id</param>
        /// <returns>Returns Json Result of Program List.</returns>
        public JsonResult LoadProgramDropDown(string id, string type = "", string selectOption = "")
        {
            List<string> includeYearList = GetYearListForReport(selectOption);
            if (type == Common.RevenueBusinessUnit)
            {
                Guid businessunitid = new Guid(id);
                var programList = db.Plan_Campaign_Program.Where(pc => pc.Plan_Campaign.Plan.Model.BusinessUnitId == businessunitid && pc.Plan_Campaign.Plan.IsDeleted == false && pc.IsDeleted == false && pc.Plan_Campaign.Plan.Status == PublishedPlan && includeYearList.Contains(pc.Plan_Campaign.Plan.Year))
                    .Select(c => new { PlanProgramId = c.PlanProgramId, Title = c.Title })
                    .OrderBy(pcp => pcp.Title);
                if (programList == null)
                    return Json(new { });

                return Json(programList, JsonRequestBehavior.AllowGet);
            }
            int campaignid = Convert.ToInt32(id);
            var programoutList = db.Plan_Campaign_Program.Where(pc => pc.PlanCampaignId == campaignid && pc.IsDeleted == false)
                 .Select(c => new { PlanProgramId = c.PlanProgramId, Title = c.Title })
                 .OrderBy(pcp => pcp.Title);
            if (programoutList == null)
                return Json(new { });
            return Json(programoutList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Tactic List.
        /// </summary>
        /// <param name="id"> Id</param>
        /// <returns>Returns Json Result of Tactic List.</returns>
        public JsonResult LoadTacticDropDown(string id, string type = "", string selectOption = "")
        {
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            List<string> includeYearList = GetYearListForReport(selectOption);
            if (type == Common.RevenueBusinessUnit)
            {
                Guid businessunitid = new Guid(id);
                var tacticList = db.Plan_Campaign_Program_Tactic.Where(pc => pc.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId == businessunitid && tacticStatus.Contains(pc.Status) && pc.IsDeleted == false && pc.Plan_Campaign_Program.Plan_Campaign.Plan.IsDeleted == false && pc.Plan_Campaign_Program.Plan_Campaign.Plan.Status == PublishedPlan && includeYearList.Contains(pc.Plan_Campaign_Program.Plan_Campaign.Plan.Year))
                    .Select(t => new { PlanTacticId = t.PlanTacticId, Title = t.Title })
                    .OrderBy(pcp => pcp.Title);
                if (tacticList == null)
                    return Json(new { });
                return Json(tacticList, JsonRequestBehavior.AllowGet);
            }
            else if (type == Common.RevenueCampaign)
            {
                int campaignid = Convert.ToInt32(id);
                var tacticList = db.Plan_Campaign_Program_Tactic.Where(pc => pc.Plan_Campaign_Program.PlanCampaignId == campaignid && tacticStatus.Contains(pc.Status) && pc.IsDeleted == false)
                    .Select(t => new { PlanTacticId = t.PlanTacticId, Title = t.Title })
                    .OrderBy(pcp => pcp.Title);
                if (tacticList == null)
                    return Json(new { });
                return Json(tacticList, JsonRequestBehavior.AllowGet);
            }

            int programid = Convert.ToInt32(id);
            var tacticoutList = db.Plan_Campaign_Program_Tactic.Where(pc => pc.PlanProgramId == programid && tacticStatus.Contains(pc.Status) && pc.IsDeleted == false)
                .Select(t => new { PlanTacticId = t.PlanTacticId, Title = t.Title })
                .OrderBy(pcp => pcp.Title);
            if (tacticoutList == null)
                return Json(new { });
            return Json(tacticoutList, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Load Revenue Realization Grid.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public JsonResult LoadRevenueRealization(string id, string businessUnitId, string type = "", string selectOption = "")
        {
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            List<string> includeYearList = GetYearListForReport(selectOption);
            List<string> includeMonth = GetMonthListForReport(selectOption);

            Guid BusinessUnitid = new Guid(businessUnitId);
            if (type == Common.RevenueBusinessUnit)
            {
                Guid buid = new Guid(id);
                Tacticdata = Tacticdata.Where(pcpt => pcpt.TacticObj.BusinessUnitId == buid).Select(t => t).ToList();
            }
            else if (type == Common.RevenueCampaign)
            {
                int campaignid = Convert.ToInt32(id);
                Tacticdata = Tacticdata.Where(pcpt => pcpt.TacticObj.Plan_Campaign_Program.PlanCampaignId == campaignid).Select(t => t).ToList();
            }
            else if (type == Common.RevenueProgram)
            {
                int programid = Convert.ToInt32(id);
                Tacticdata = Tacticdata.Where(pcpt => pcpt.TacticObj.PlanProgramId == programid).Select(t => t).ToList();
            }
            else if (type == Common.RevenueTactic)
            {
                int tacticid = Convert.ToInt32(id);
                Tacticdata = Tacticdata.Where(pcpt => pcpt.TacticObj.PlanTacticId == tacticid).Select(t => t).ToList();
            }

            if (Tacticdata.Count() > 0)
            {
                string inq = Enums.Stage.INQ.ToString();
                int INQStaged = db.Stages.Single(s => s.ClientId == Sessions.User.ClientId && s.Code == inq).StageId;
                // List<int> TacticIdList = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
                //List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpt => TacticIdList.Contains(pcpt.PlanTacticId) && includeMonth.Contains(pcpt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pcpt.Period)).ToList();
                List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));
                ActualTacticList = ActualTacticList.Where(mr => includeMonth.Contains((Tacticdata.Single(t => t.TacticObj.PlanTacticId == mr.PlanTacticId).TacticYear) + mr.Period)).ToList();


                var rdata = new[] { new { 
                INQGoal = GetProjectedINQDataWithVelocity(Tacticdata).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                monthList = includeMonth,
                INQActual = ActualTacticList.Where(pcpt => pcpt.Plan_Campaign_Program_Tactic.StageId == INQStaged && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString())).GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period).Select(pcptj => new
                {
                    PKey = pcptj.Key,
                    PSum = pcptj.Sum(pt => pt.Actualvalue)
                }),
                MQLGoal = GetProjectedMQLDataWithVelocity(Tacticdata).AsEnumerable().AsQueryable().Where(mr => includeMonth.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Field<double>(ColumnValue))
                }),
                MQLActual = ActualTacticList.Where(pcpt => pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString())).GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period).Select(pcptj => new
                {
                    PKey = pcptj.Key,
                    PSum = pcptj.Sum(pt => pt.Actualvalue)
                }),
                 RevenueGoal = ActualTacticList.Where(pcpt => pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString())).GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period).Select(pcptj => new
                {
                    PKey = pcptj.Key,
                    PSum = pcptj.Sum(pt => pt.Actualvalue)
                })
            }  };

                return Json(rdata, JsonRequestBehavior.AllowGet);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
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
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            List<string> includeYearList = GetYearListForReport(selectOption, true);

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

            Tacticdata = Tacticdata.Where(pcpt =>
                ((parentlabel == Common.RevenueBusinessUnit && pcpt.TacticObj.BusinessUnit.ClientId == Sessions.User.ClientId) ||
                (parentlabel == Common.RevenueAudience && pcpt.TacticObj.Audience.ClientId == Sessions.User.ClientId) ||
                (parentlabel == Common.RevenueGeography && pcpt.TacticObj.Geography.ClientId == Sessions.User.ClientId) ||
                (parentlabel == Common.RevenueVertical && pcpt.TacticObj.Vertical.ClientId == Sessions.User.ClientId) ||
                (parentlabel == Common.RevenueCampaign))
                ).ToList();
            if (isBusinessUnit)
            {
                Tacticdata = Tacticdata.Where(c => c.TacticObj.BusinessUnitId == buid).ToList();
            }
            var campaignList = Tacticdata.GroupBy(pc => new { title = pc.TacticObj.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pc =>
                     new RevenueContrinutionData
                     {
                         Title = pc.Key.title,
                         planTacticList = pc.Select(p => p.TacticObj.PlanTacticId).ToList()
                     }).ToList();

            if (parentlabel == Common.RevenueVertical)
            {
                campaignList = Tacticdata.GroupBy(pc => new { title = pc.TacticObj.Vertical.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.TacticObj.PlanTacticId).ToList()
                         }).ToList();
            }
            else if (parentlabel == Common.RevenueGeography)
            {
                campaignList = Tacticdata.GroupBy(pc => new { title = pc.TacticObj.Geography.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.TacticObj.PlanTacticId).ToList()
                         }).ToList();
            }
            else if (parentlabel == Common.RevenueAudience)
            {
                campaignList = Tacticdata.GroupBy(pc => new { title = pc.TacticObj.Audience.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.TacticObj.PlanTacticId).ToList()
                         }).ToList();
            }
            else if (parentlabel == Common.RevenueBusinessUnit)
            {
                campaignList = Tacticdata.GroupBy(pc => new { title = pc.TacticObj.BusinessUnit.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.TacticObj.PlanTacticId).ToList()
                         }).ToList();
            }

            if (campaignList.Count() > 0)
            {
                string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
                int lastMonth = GetLastMonthForTrend(selectOption);
                List<string> includeMonth = GetMonthListForReport(selectOption, true);
                List<string> monthList = GetUpToCurrentMonth();
                DataTable ProjectedRevenueDatatable = GetProjectedRevenueValueDataTableForReport(Tacticdata);
                DataTable ProjectedCostDatatable = GetProjectedCostData(Tacticdata, true);
                DataTable ActualCostDatatable = GetProjectedCostData(Tacticdata, false);

                List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));

                List<Plan_Tactic_MQL> ActualRevenueTrendList = GetTrendRevenueDataContribution(ActualTacticList, lastMonth, monthList, revenue);
                List<string> monthWithYearList = GetUpToCurrentMonthWithYearForReport(selectOption, true);

                var campaignListFinal = campaignList.Select(p => new
                {
                    Title = p.Title,
                    PlanRevenue = ProjectedRevenueDatatable.AsEnumerable().AsQueryable().Where(mr => p.planTacticList.Contains(mr.Field<int>(ColumnId)) && includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                    ActualRevenue = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => p.planTacticList.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue) && includeMonth.Contains(ta.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + ta.Period)).ToList().Sum(ta => ta.Actualvalue),
                    TrendRevenue = 0,//GetTrendRevenueDataContribution(p.planTacticList, lastMonth),
                    PlanCost = ProjectedCostDatatable.AsEnumerable().AsQueryable().Where(mr => p.planTacticList.Contains(mr.Field<int>(ColumnId)) && includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                    ActualCost = ActualCostDatatable.AsEnumerable().AsQueryable().Where(mr => p.planTacticList.Contains(mr.Field<int>(ColumnId)) && includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                    TrendCost = 0,//GetTrendCostDataContribution(p.planTacticList, lastMonth),
                    RunRate = ActualRevenueTrendList.Where(ar => p.planTacticList.Contains(ar.PlanTacticId)).Sum(ar => ar.MQL),//GetTrendRevenueDataContribution(p.planTacticList, lastMonth, monthList),
                    PipelineCoverage = 0,//GetPipelineCoverage(p.planTacticList, lastMonth),
                    RevSpend = GetRevenueVSSpendContribution(Tacticdata, ActualTacticList, p.planTacticList, monthWithYearList, monthList, revenue),
                    RevenueTotal = GetActualRevenueTotal(ActualTacticList, p.planTacticList, monthList, revenue),
                    CostTotal = ActualCostDatatable.AsEnumerable().AsQueryable().Where(mr => p.planTacticList.Contains(mr.Field<int>(ColumnId)) && monthWithYearList.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue))//GetActualCostTotal(p.planTacticList, selectOption),
                }).Select(p => p).Distinct().OrderBy(p => p.Title);

                return Json(campaignListFinal, JsonRequestBehavior.AllowGet);
            }
            return Json(new { });
        }

        /// <summary>
        /// Get Projected Cost Data With Month Wise.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetProjectedCostData(List<TacticStageValue> tacticData, bool isprojected = true)
        {
            List<TacticDataTable> tacticdata = tacticData.Select(td => new TacticDataTable { TacticId = td.TacticObj.PlanTacticId, Value = isprojected ? td.TacticObj.Cost : (td.TacticObj.CostActual.HasValue ? (double)td.TacticObj.CostActual : 0), StartMonth = td.TacticObj.StartDate.Month, EndMonth = td.TacticObj.EndDate.Month, StartYear = td.TacticObj.StartDate.Year, EndYear = td.TacticObj.EndDate.Year }).ToList();
            return GetDatatable(tacticdata);
        }

        /// <summary>
        /// Get Trend.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public List<Plan_Tactic_MQL> GetTrendRevenueDataContribution(List<Plan_Campaign_Program_Tactic_Actual> planTacticList, int lastMonth, List<string> monthList, string revenue)
        {
            return planTacticList.Where(ta => monthList.Contains(ta.Period) && ta.StageTitle == revenue).GroupBy(t => t.PlanTacticId).Select(pt => new Plan_Tactic_MQL
            {
                PlanTacticId = pt.Key,
                MQL = (pt.Sum(a => a.Actualvalue) / currentMonth) * lastMonth
            }).ToList();
        }

        /// <summary>
        /// Get Trend.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public double GetRevenueVSSpendContribution(List<TacticStageValue> TacticList, List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList, List<int> planTacticList, List<string> monthWithYearList, List<string> monthList, string revenue)
        {
            List<TacticDataTable> tacticdata = (from td in TacticList
                                                where planTacticList.Contains(td.TacticObj.PlanTacticId)
                                                select new TacticDataTable { TacticId = td.TacticObj.PlanTacticId, Value = td.TacticObj.CostActual.HasValue ? (double)td.TacticObj.CostActual : 0, StartMonth = td.TacticObj.StartDate.Month, EndMonth = td.TacticObj.EndDate.Month, StartYear = td.TacticObj.StartDate.Year, EndYear = td.TacticObj.EndDate.Year }).ToList();

            double costTotal = GetDatatable(tacticdata).AsEnumerable().AsQueryable().Where(c => monthWithYearList.Contains(c.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));

            double revenueTotal = ActualTacticList.Where(ta => planTacticList.Contains(ta.PlanTacticId) && monthList.Contains(ta.Period) && ta.StageTitle == revenue).ToList().Sum(a => a.Actualvalue);

            double RevenueSpend = 0;
            if (costTotal != 0)
            {
                RevenueSpend = ((revenueTotal - costTotal) / costTotal);
            }
            else if (revenueTotal != 0)
            {
                RevenueSpend = 1;
            }
            return RevenueSpend;
        }

        /// <summary>
        /// Get Revenue Total.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public double GetActualRevenueTotal(List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList, List<int> planTacticList, List<string> monthList, string revenue)
        {
            return ActualTacticList.Where(ta => planTacticList.Contains(ta.PlanTacticId) && monthList.Contains(ta.Period) && ta.StageTitle == revenue).ToList().Sum(a => a.Actualvalue);
        }

        #endregion

        #region "Revenue to Plan"

        /// <summary>
        /// Function to get data for revenue to plan report.
        /// </summary>
        /// <param name="ParentLabel">Filter name.</param>
        /// <param name="id">GUID for filtering data.</param>
        /// <returns>Returns json data for revenue to plan report.</returns>
        public JsonResult GetRevenueToPlan(string ParentLabel, string id, string selectOption = "", string originalOption = "")
        {
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            List<string> includeYearList = GetYearListForReport(selectOption);
            List<string> includeMonth = GetMonthListForReport(selectOption, true);
            Tacticdata = Tacticdata.Where(pcpt => ((ParentLabel == Common.RevenueBusinessUnit && pcpt.TacticObj.BusinessUnitId == new Guid(id)) ||
                                                                (ParentLabel == Common.RevenueAudience && pcpt.TacticObj.AudienceId == Convert.ToInt32(id)) ||
                                                                (ParentLabel == Common.RevenueGeography && pcpt.TacticObj.GeographyId == new Guid(id)) ||
                                                                (ParentLabel == Common.RevenueVertical && pcpt.TacticObj.VerticalId == Convert.ToInt32(id) ||
                                                                (ParentLabel == Common.RevenueOrganization)
                                                                ))).ToList();
            // List<int> TacticIdList = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
            string stageTitleRevenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            //List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpta => TacticIdList.Contains(pcpta.Plan_Campaign_Program_Tactic.PlanTacticId) && pcpta.StageTitle == stageTitleRevenue).ToList();

            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));
            ActualTacticList = ActualTacticList.Where(pcpta => pcpta.StageTitle == stageTitleRevenue).ToList();


            if (Tacticdata.Count > 0)
            {
                DataTable dtActualRevenue = GetActualRevenue(ActualTacticList);
                DataTable dtProjectedRevenue = GetProjectedRevenue(Tacticdata, includeMonth, selectOption);
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
        private DataTable GetProjectedRevenue(List<TacticStageValue> TacticList, List<string> monthList, string selectOption)
        {
            List<TacticDataTable> tacticdata = (from t in TacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = t.TacticObj.PlanTacticId,
                                                    Value = t.RevenueValue,
                                                    StartMonth = t.TacticObj.StartDate.AddDays(t.CWVelocity).Month,
                                                    EndMonth = t.TacticObj.EndDate.AddDays(t.CWVelocity).Month,
                                                    StartYear = t.TacticObj.StartDate.AddDays(t.CWVelocity).Year,
                                                    EndYear = t.TacticObj.EndDate.AddDays(t.CWVelocity).Year
                                                }).ToList();
            var trevenueProjected = GetDatatable(tacticdata).AsEnumerable().AsQueryable().Where(mr => monthList.Contains(mr.Field<string>(ColumnMonth))).GroupBy(r => r.Field<string>(ColumnMonth)).Select(g => new
            {
                PKey = g.Key,
                PSum = g.Sum(r => r.Field<double>(ColumnValue))
            });


            DataTable dtProjectedRevenue = GetDataTableMonthValue();
            foreach (DataRow drProjectedRevenue in dtProjectedRevenue.Rows)
            {
                drProjectedRevenue[ColumnValue] = trevenueProjected.Where(tp => tp.PKey == selectOption + PeriodPrefix + drProjectedRevenue[ColumnMonth]).Count() > 0 ? trevenueProjected.Where(tp => tp.PKey == selectOption + PeriodPrefix + drProjectedRevenue[ColumnMonth]).Sum(tp => tp.PSum) : 0;
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
        private DataTable GetActualRevenue(List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList)
        {
            //// Getting period wise actual revenue of non-deleted and approved/in-progress/complete tactic of plan present in planids.
            var planCampaignTacticActualAll = ActualTacticList.GroupBy(pcpta => pcpta.Period).Select(group => new { Period = group.Key, ActualValue = group.Sum(pcpta => pcpta.Actualvalue) }).OrderBy(pcpta => pcpta.Period);

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

        #endregion

        #region "Source Performance"
        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to get data for source performance report.
        /// </summary>
        /// <param name="filter">Filter to get data for plan/trend or actual.</param>
        /// <returns>Return json data for source performance report.</returns>
        public JsonResult GetSourcePerformance(string selectOption = "")
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
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            List<string> includeYearList = GetYearListForReport(selectOption, true);
            List<string> includeMonth = GetMonthListForReport(selectOption, true);
            List<string> includeMonthUpCurrent = GetUpToCurrentMonthWithYearForReport(selectOption, true);
            // List<int> TacticIds = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
            DataTable ProjectedRevenueDataTable = GetProjectedRevenueValueDataTableForReport(Tacticdata);
            string Revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            //  List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpt => TacticIds.Contains(pcpt.PlanTacticId) && pcpt.StageTitle == Revenue && includeMonthUpCurrent.Contains(pcpt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pcpt.Period)).ToList();

            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));
            //Start : Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
            ActualTacticList = ActualTacticList.Where(mr => mr.StageTitle == Revenue && includeMonth.Contains((Tacticdata.Single(t => t.TacticObj.PlanTacticId == mr.PlanTacticId).TacticYear) + mr.Period)).ToList();


            var businessUnits = db.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId) && b.IsDeleted == false).ToList()
                                                .Select(b => new
                                                {
                                                    Title = b.Title,
                                                    ColorCode = string.Format("#{0}", b.ColorCode),
                                                    Value = GetActualVSPlannedRevenue(ActualTacticList, ProjectedRevenueDataTable, Tacticdata.Where(t => t.TacticObj.BusinessUnitId.Equals(b.BusinessUnitId)).Select(t => t.TacticObj.PlanTacticId).ToList(), includeMonthUpCurrent)
                                                }).OrderBy(b => b.Title);
            var vertical = db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted==false).ToList()
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = GetActualVSPlannedRevenue(ActualTacticList, ProjectedRevenueDataTable, Tacticdata.Where(t => t.TacticObj.VerticalId.Equals(v.VerticalId)).Select(t => t.TacticObj.PlanTacticId).ToList(), includeMonthUpCurrent)
                                                }).OrderBy(v => v.Title);

            var geography = db.Geographies.Where(g => g.ClientId.Equals(Sessions.User.ClientId) && g.IsDeleted==false).ToList()
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = GetActualVSPlannedRevenue(ActualTacticList, ProjectedRevenueDataTable, Tacticdata.Where(t => t.TacticObj.GeographyId.Equals(g.GeographyId)).Select(t => t.TacticObj.PlanTacticId).ToList(), includeMonthUpCurrent)
                                                }).OrderBy(g => g.Title);
            //End : Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
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
        private double GetActualVSPlannedRevenue(List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList, DataTable ProjectedRevenueDataTable, List<int> tacticIds, List<string> includeMonthUpCurrent)
        {
            double actualRevenueValue = 0;
            double percentageValue = 0;
            if (tacticIds.Count() > 0)
            {
                var ActualRevenue = ActualTacticList.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId)).ToList();
                if (ActualRevenue.Count() > 0)
                {
                    actualRevenueValue = ActualRevenue.Sum(a => a.Actualvalue);
                }
                double projectedRevenueValue = ProjectedRevenueDataTable.AsEnumerable().AsQueryable().Where(mr => tacticIds.Contains(mr.Field<int>(ColumnId)) && includeMonthUpCurrent.Contains(mr.Field<string>(ColumnMonth))).Sum(mr => mr.Field<double>(ColumnValue));
                if (projectedRevenueValue != 0 && actualRevenueValue >= projectedRevenueValue)
                {
                    percentageValue = Math.Round(((actualRevenueValue - projectedRevenueValue) / projectedRevenueValue) * 100, 2);
                }

            }
            return percentageValue;
        }

        #endregion

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
            var individuals = bdsUserRepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, true);//Sessions.IsSystemAdmin);

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
       
            string html = "<html>";
            html +=  "<head>";
            html +=  string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/bootstrap.css"));
            html +=  string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/bootstrap-responsive.css"));
            html +=  string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/style.css"));
            html += string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/datepicker.css"));
            html += string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/style_extended.css"));
            html += string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/tipsy.css"));
            html += string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/DHTMLX/dhtmlxgantt.css"));
            
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/DHTMLX/dhtmlxgantt.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.min.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery-migrate-1.2.1.min.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/bootstrap.min.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.slimscroll.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.slidepanel.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/scripts.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/scripts_extended.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/jquery.form.min.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/bootstrap-datepicker.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.price_format.1.8.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.price_format.1.8.min.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/slimScrollHorizontal.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.tipsy.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/jquery.selectbox-0.2.js"));

            html += string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/font-awesome.min.css"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/modernizr-2.5.3.js"));

            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/dhtmlxchart.js"));
            html += string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/dhtmlxchart.css"));
            
            html += "</head>";
            html += "<body style='background: none repeat scroll 0 0 #FFFFFF; font-size: 14px;'>";
            //style='background: none repeat scroll 0 0 #FFFFFF; font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; font-size: 14px;'
            html += htmlOfCurrentView;
            html += "</body>";
            html += "</html>";

            if (reportType.Equals(Enums.ReportType.Summary.ToString()))
            {
                html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/ReportSummary.js"));
            }
            else if (reportType.Equals(Enums.ReportType.Revenue.ToString()))
            {
                html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/ReportRevenue.js"));
            }
            else
            {
                html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/ReportConversion.js"));
            }

            

            return html;
        }

        #endregion

    }
}

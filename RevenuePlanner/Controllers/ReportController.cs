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
using System.Data.Objects.SqlClient;

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

            ////Start Added by Mitesh Vaishnav for PL ticket #846
            Guid reportBusinessUnitId = Guid.Empty;
            if (Sessions.PlanId > 0)
            {
                Sessions.ReportPlanIds = new List<int>();
                Sessions.ReportBusinessUnitIds = new List<Guid>();
                Sessions.ReportPlanIds.Add(Sessions.PlanId);
                reportBusinessUnitId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.Model.BusinessUnitId).SingleOrDefault();
                Sessions.ReportBusinessUnitIds.Add(reportBusinessUnitId);
            }

            List<ViewByModel> lstViewByTab = new List<ViewByModel>();
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Plan.ToString(), Value = ReportTabType.Plan.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Vertical.ToString(), Value = ReportTabType.Vertical.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Geography.ToString(), Value = ReportTabType.Geography.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.BusinessUnit.ToString(), Value = ReportTabType.BusinessUnit.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = Common.CustomLabelFor(Enums.CustomLabelCode.Audience), Value = ReportTabType.Audience.ToString() });
            lstViewByTab = lstViewByTab.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewByTab = lstViewByTab;

            List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
            lstViewByAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarterly", Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstViewByAllocated = lstViewByAllocated.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewByAllocated = lstViewByAllocated;

            List<SelectListItem> lstGeography = new List<SelectListItem>();
            lstGeography = db.Geographies.Where(g => g.ClientId == Sessions.User.ClientId && g.IsDeleted == false).ToList().Select(g => new SelectListItem { Text = g.Title, Value = g.GeographyId.ToString(), Selected = true }).ToList();
            ViewBag.ViewGeography = lstGeography.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();

            List<SelectListItem> lstBusinessUnit = new List<SelectListItem>();
            lstBusinessUnit = db.BusinessUnits.Where(b => b.ClientId == Sessions.User.ClientId && b.IsDeleted == false).ToList().Select(b => new SelectListItem { Text = b.Title, Value = b.BusinessUnitId.ToString(), Selected = (b.BusinessUnitId == reportBusinessUnitId ? true : false) }).ToList();
            ViewBag.ViewBusinessUnit = lstBusinessUnit.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();

            List<SelectListItem> lstVertical = new List<SelectListItem>();
            lstVertical = db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted == false).ToList().Select(v => new SelectListItem { Text = v.Title, Value = v.VerticalId.ToString(), Selected = true }).ToList();
            ViewBag.ViewVertical = lstVertical.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();

            List<SelectListItem> lstAudience = new List<SelectListItem>();
            lstAudience = db.Audiences.Where(a => a.ClientId == Sessions.User.ClientId && a.IsDeleted == false).ToList().Select(a => new SelectListItem { Text = a.Title, Value = a.AudienceId.ToString(), Selected = true }).ToList();
            ViewBag.ViewAudience = lstAudience.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();

            string published = Enums.PlanStatus.Published.ToString();
            List<SelectListItem> lstYear = new List<SelectListItem>();
            var lstPlan = db.Plans.Where(p => p.IsDeleted == false && p.Status == published && p.Model.BusinessUnit.ClientId == Sessions.User.ClientId).ToList();
            var yearlist = lstPlan.OrderBy(p => p.Year).Select(p => p.Year).Distinct().ToList();
            yearlist.ForEach(year => lstYear.Add(new SelectListItem { Text = "FY " + year, Value = year }));
            SelectListItem thisQuarter = new SelectListItem { Text = "this quarter", Value = "thisquarter" };
            lstYear.Add(thisQuarter);

            string defaultallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString();
            string Noneallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString();

            List<SelectListItem> lstPlanList = new List<SelectListItem>();
            string planyear = DateTime.Now.Year.ToString();

            lstPlanList = lstPlan.Where(p => p.Year == planyear).Select(p => new SelectListItem { Text = p.Title + " - " + (p.AllocatedBy == defaultallocatedby ? Noneallocatedby : p.AllocatedBy), Value = p.PlanId.ToString() + "_" + p.AllocatedBy, Selected = (p.PlanId == Sessions.PlanId ? true : false) }).ToList();
            ViewBag.ViewPlan = lstPlanList.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewYear = lstYear.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            ViewBag.SelectedYear = planyear;
            //End Added by Mitesh Vaishnav for PL ticket #846

            return View("Index");
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
        public ActionResult GetSummaryData()
        {
            SummaryReportModel objSummaryReportModel = new SummaryReportModel();
            //// Getting current year's all published plan for all business unit of clientid of director.
            var plans = Common.GetPlan();
            string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            plans = plans.Where(p => p.Status.Equals(planPublishedStatus)).Select(p => p).ToList();
            //Filter to filter out the plan based on the Selected businessunit and PlanId
            if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count > 0)
            {
                plans = plans.Where(p => Sessions.ReportPlanIds.Contains(p.PlanId)).ToList();
            }
            else
            {
                plans.Clear();
            }

            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count > 0)
            {
                plans = plans.Where(pl => Sessions.ReportBusinessUnitIds.Contains(pl.Model.BusinessUnitId)).ToList();
            }
            else
            {
                plans.Clear();
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
        /// Function to get tactic for report.
        /// Added By Bhavesh.
        /// </summary>
        /// <returns>Returns list of Tactic Id.</returns>
        private List<Plan_Campaign_Program_Tactic> GetTacticForReporting(string selectOption = "", bool isForSummary = false)
        {
            //// Getting current year's all published plan for all business unit of clientid of director.
            List<Plan> plans = Common.GetPlan().Where(p => p.Status.Equals(PublishedPlan)).ToList();
            if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count > 0)
            {
                plans = plans.Where(gp => Sessions.ReportPlanIds.Contains(gp.PlanId)).ToList();
            }
            else
            {
                plans.Clear();
            }
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count > 0)
            {
                plans = plans.Where(gp => Sessions.ReportBusinessUnitIds.Contains(gp.Model.BusinessUnitId)).ToList();
            }
            else
            {
                plans.Clear();
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
        /// Get Child Tab Title As per Selection.
        /// </summary>
        /// <param name="ParentLabel">ParentLabel.</param>
        /// <returns>jsonResult</returns>
        public JsonResult GetChildLabelData(string ParentLabel, string selectOption = "")
        {
            if (ParentLabel == Common.RevenueBusinessUnit)
            {
                var returnData = (db.BusinessUnits.Where(bu => bu.ClientId.Equals(Sessions.User.ClientId) && bu.IsDeleted.Equals(false) && (Sessions.ReportBusinessUnitIds.Contains(bu.BusinessUnitId))).Select(b => new
                {
                    id = b.BusinessUnitId,
                    title = b.Title
                }).Select(b => b).Distinct().OrderBy(b => b.title)).ToList();
                returnData = returnData.Where(s => !string.IsNullOrEmpty(s.title)).OrderBy(s => s.title, new AlphaNumericComparer()).ToList();
                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel == Common.RevenueAudience)
            {
                var returnData = (db.Audiences.Where(au => au.ClientId == Sessions.User.ClientId && au.IsDeleted == false).Select(a => new
                {
                    id = a.AudienceId,
                    title = a.Title
                }).Select(a => a).Distinct().OrderBy(a => a.title)).ToList();
                returnData = returnData.Where(s => !string.IsNullOrEmpty(s.title)).OrderBy(s => s.title, new AlphaNumericComparer()).ToList();
                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel == Common.RevenueGeography)
            {
                var returnData = (db.Geographies.Where(ge => ge.ClientId.Equals(Sessions.User.ClientId) && ge.IsDeleted.Equals(false)).Select(g => new
                {
                    id = g.GeographyId,
                    title = g.Title
                }).Select(g => g).Distinct().OrderBy(g => g.title)).ToList();
                returnData = returnData.Where(s => !string.IsNullOrEmpty(s.title)).OrderBy(s => s.title, new AlphaNumericComparer()).ToList();
                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel == Common.RevenueVertical)
            {
                var returnData = (db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted == false).Select(v => new
                {
                    id = v.VerticalId,
                    title = v.Title
                }).Select(v => v).Distinct().OrderBy(v => v.title)).ToList();
                returnData = returnData.Where(s => !string.IsNullOrEmpty(s.title)).OrderBy(s => s.title, new AlphaNumericComparer()).ToList();
                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel == Common.RevenuePlans)
            {
                string year = selectOption;
                var plans = db.Plans.Where(p => p.Model.BusinessUnit.ClientId.Equals(Sessions.User.ClientId) && p.IsDeleted.Equals(false) && p.Status == PublishedPlan && Sessions.ReportPlanIds.Contains(p.PlanId)).ToList();
                if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
                {
                    year = DateTime.Now.Year.ToString();
                }

                var returnData = plans.Where(p => p.Year == year).Select(p => new
                {
                    id = p.PlanId,
                    title = p.Title
                }).Select(b => b).Distinct().OrderBy(b => b.title).ToList();
                returnData = returnData.Where(s => !string.IsNullOrEmpty(s.title)).OrderBy(s => s.title, new AlphaNumericComparer()).ToList();
                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
            else if (ParentLabel.Contains(Common.CustomTitle))
            {

                int customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.CustomTitle, ""));
                string customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();

                if (customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                {
                    var optionlist = db.CustomFieldOptions.Where(co => co.CustomFieldId == customfieldId).ToList();
                    var returnData = optionlist.Select(p => new
                    {
                        id = p.CustomFieldOptionId,
                        title = p.Value
                    }).Select(b => b).Distinct().OrderBy(b => b.title).ToList();
                    returnData = returnData.Where(s => !string.IsNullOrEmpty(s.title)).OrderBy(s => s.title, new AlphaNumericComparer()).ToList();
                    return Json(returnData, JsonRequestBehavior.AllowGet);
                }
                else if (customFieldType == Enums.CustomFieldType.TextBox.ToString())
                {
                    List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting(selectOption);
                    List<int> entityids = tacticlist.Select(t => t.PlanTacticId).ToList();
                    var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();
                    var returnData = cusomfieldEntity.Select(p => new
                    {
                        id = p.Value,
                        title = p.Value
                    }).Select(b => b).Distinct().OrderBy(b => b.title).ToList();
                    returnData = returnData.Where(s => !string.IsNullOrEmpty(s.title)).OrderBy(s => s.title, new AlphaNumericComparer()).ToList();
                    return Json(returnData, JsonRequestBehavior.AllowGet);
                }
                var returnDatamain = new List<string>();
                return Json(returnDatamain, JsonRequestBehavior.AllowGet);

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
        public ActionResult GetConversionData(string timeFrameOption = "thisquarter")
        {

            ViewBag.MonthTitle = GetDisplayMonthListForReport(timeFrameOption);
            ViewBag.SelectOption = timeFrameOption;
            List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting(timeFrameOption);
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist);
            TempData["ReportData"] = tacticStageList;

            var lstCustomFieldsTactics = Common.GetTacticsCustomFields(tacticlist.Select(a => a.PlanTacticId).ToList());

            List<ViewByModel> lstParentConversionSummery = new List<ViewByModel>();
            lstParentConversionSummery.Add(new ViewByModel { Text = Common.CustomLabelFor(Enums.CustomLabelCode.Audience), Value = Common.RevenueAudience });
            lstParentConversionSummery.Add(new ViewByModel { Text = Common.RevenueGeography, Value = Common.RevenueGeography });
            lstParentConversionSummery.Add(new ViewByModel { Text = Common.RevenueVertical, Value = Common.RevenueVertical });
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count != 1)
            {
                lstParentConversionSummery.Add(new ViewByModel { Text = Common.RevenueBusinessUnit, Value = Common.RevenueBusinessUnit });
            }
            lstParentConversionSummery = lstParentConversionSummery.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            lstCustomFieldsTactics = lstCustomFieldsTactics.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            lstParentConversionSummery = lstParentConversionSummery.Concat(lstCustomFieldsTactics).ToList();
            ViewBag.parentConvertionSummery = lstParentConversionSummery;

            List<ViewByModel> lstParentConversionPerformance = new List<ViewByModel>();
            lstParentConversionPerformance.Add(new ViewByModel { Text = Common.Plan, Value = Common.Plan });
            lstParentConversionPerformance.Add(new ViewByModel { Text = Common.Trend, Value = Common.Trend });
            lstParentConversionPerformance.Add(new ViewByModel { Text = Common.Actuals, Value = Common.Actuals });
            lstParentConversionPerformance = lstParentConversionPerformance.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            ViewBag.parentConvertionPerformance = lstParentConversionPerformance;

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
            else if (ParentTab.Contains(Common.CustomTitle))
            {
                List<int> entityids = new List<int>();
                int customfieldId = Convert.ToInt32(ParentTab.Replace(Common.CustomTitle, ""));
                string customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                entityids = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && c.Value == Id).Select(c => c.EntityId).ToList();
                Tacticdata = Tacticdata.Where(pcpt => entityids.Contains(pcpt.TacticObj.PlanTacticId)).ToList();
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
            var businessUnits = db.BusinessUnits.Where(b => b.ClientId == Sessions.User.ClientId && b.IsDeleted == false).ToList()
                                           .Select(b => new
                                           {
                                               Title = b.Title,
                                               ColorCode = string.Format("#{0}", b.ColorCode),
                                               Value = tacticTrenBusinessUnit.Any(bu => bu.BusinessUnitId.Equals(b.BusinessUnitId)) ? tacticTrenBusinessUnit.Where(bu => bu.BusinessUnitId.Equals(b.BusinessUnitId)).First().Trend : 0

                                           });
            var vertical = db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted == false).ToList()
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = tacticTrendVertical.Any(ve => ve.VerticalId.Equals(v.VerticalId)) ? tacticTrendVertical.Where(ve => ve.VerticalId.Equals(v.VerticalId)).First().Trend : 0
                                                });

            var geography = db.Geographies.Where(g => g.ClientId == Sessions.User.ClientId && g.IsDeleted == false).ToList()
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
            var businessUnits = db.BusinessUnits.Where(b => b.ClientId == Sessions.User.ClientId && b.IsDeleted == false).ToList()
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
            var vertical = db.Verticals.ToList().Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted == false).ToList()
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

            var geography = db.Geographies.ToList().Where(g => g.ClientId == Sessions.User.ClientId && g.IsDeleted == false).ToList()
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
            var businessUnits = db.BusinessUnits.Where(b => b.ClientId == Sessions.User.ClientId && b.IsDeleted == false).ToList()
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



            var vertical = db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted == false).ToList()
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

            var geography = db.Geographies.Where(g => g.ClientId == Sessions.User.ClientId && g.IsDeleted == false).ToList()
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

            //Custom
            if (ParentConversionSummaryTab.Contains(Common.CustomTitle))
            {
                int customfieldId = Convert.ToInt32(ParentConversionSummaryTab.Replace(Common.CustomTitle, ""));
                List<int> entityids = db.CustomField_Entity.Where(e => e.CustomFieldId == customfieldId).Select(e => e.EntityId).ToList();
                Tacticdata = Tacticdata.Where(t => entityids.Contains(t.TacticObj.PlanTacticId)).ToList();
            }
            else
            {
                Tacticdata = Tacticdata.Where(pcpt =>
                    ((ParentConversionSummaryTab == Common.BusinessUnit && pcpt.TacticObj.BusinessUnit.ClientId == Sessions.User.ClientId) ||
                    (ParentConversionSummaryTab == Common.Audience && pcpt.TacticObj.Audience.ClientId == Sessions.User.ClientId) ||
                    (ParentConversionSummaryTab == Common.Geography && pcpt.TacticObj.Geography.ClientId == Sessions.User.ClientId) ||
                    (ParentConversionSummaryTab == Common.Vertical && pcpt.TacticObj.Vertical.ClientId == Sessions.User.ClientId))
                    ).ToList();
            }
            var DataTitleList = new List<RevenueContrinutionData>();

            if (ParentConversionSummaryTab == Common.RevenueBusinessUnit)
            {
                DataTitleList = Tacticdata.GroupBy(pc => new { title = pc.TacticObj.BusinessUnit.Title }).Select(pc =>
                         new RevenueContrinutionData
                         {
                             Title = pc.Key.title,
                             planTacticList = pc.Select(p => p.TacticObj.PlanTacticId).ToList()
                         }).ToList();
            }
            else if (ParentConversionSummaryTab == Common.RevenueVertical)
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
            else
            {
                int customfieldId = Convert.ToInt32(ParentConversionSummaryTab.Replace(Common.CustomTitle, ""));
                List<int> entityids = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
                string customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();
                if (customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                {
                    var optionlist = cusomfieldEntity.Select(c => Convert.ToInt32(c.Value)).ToList();
                    DataTitleList = (from cfo in db.CustomFieldOptions
                                     where cfo.CustomFieldId == customfieldId && optionlist.Contains(cfo.CustomFieldOptionId)
                                     select cfo).ToList().GroupBy(pc => new { id = pc.CustomFieldOptionId, title = pc.Value }).Select(pc =>
                                  new RevenueContrinutionData
                                  {
                                      Title = pc.Key.title,
                                      planTacticList = cusomfieldEntity.Where(c => c.Value == pc.Key.id.ToString()).Select(c => c.EntityId).ToList()
                                  }).ToList();

                }
                else if (customFieldType == Enums.CustomFieldType.TextBox.ToString())
                {
                    DataTitleList = cusomfieldEntity.GroupBy(pc => new { title = pc.Value }).Select(pc =>
                                new RevenueContrinutionData
                                {
                                    Title = pc.Key.title,
                                    planTacticList = pc.Select(c => c.EntityId).ToList()
                                }).ToList();
                }
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
        public ActionResult GetRevenueData(string timeFrameOption = "thisquarter")
        {
            ViewBag.MonthTitle = GetDisplayMonthListForReport(timeFrameOption);
            ViewBag.SelectOption = timeFrameOption;
            var lstBusinessunits = db.BusinessUnits.Where(b => b.ClientId == Sessions.User.ClientId && b.IsDeleted == false && Sessions.ReportBusinessUnitIds.Contains(b.BusinessUnitId)).OrderBy(b => b.Title).ToList();//Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
            if (lstBusinessunits.Count == 0)
            {
                BusinessUnit objBusinessUnit = new BusinessUnit();
                objBusinessUnit.BusinessUnitId = Guid.Empty;
                objBusinessUnit.Title = "None";
                lstBusinessunits.Add(objBusinessUnit);

            }
            lstBusinessunits = lstBusinessunits.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
            ViewBag.BusinessUnit = lstBusinessunits;
            List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting(timeFrameOption);
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist);

            var lstCustomFieldsTactics = Common.GetTacticsCustomFields(tacticlist.Select(a => a.PlanTacticId).ToList());

            List<ViewByModel> lstParentRevenueSummery = new List<ViewByModel>();
            lstParentRevenueSummery.Add(new ViewByModel { Text = Common.RevenueGeography, Value = Common.RevenueGeography });
            lstParentRevenueSummery.Add(new ViewByModel { Text = Common.RevenueVertical, Value = Common.RevenueVertical });
            lstParentRevenueSummery.Add(new ViewByModel { Text = Common.CustomLabelFor(Enums.CustomLabelCode.Audience), Value = Common.RevenueAudience });
            if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count != 1)
            {
                lstParentRevenueSummery.Add(new ViewByModel { Text = Common.RevenuePlans, Value = Common.RevenuePlans });
            }
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count != 1)
            {
                lstParentRevenueSummery.Add(new ViewByModel { Text = Common.RevenueBusinessUnit, Value = Common.RevenueBusinessUnit });
            }
            lstParentRevenueSummery = lstParentRevenueSummery.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            lstCustomFieldsTactics = lstCustomFieldsTactics.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            lstParentRevenueSummery = lstParentRevenueSummery.Concat(lstCustomFieldsTactics).ToList();
            ViewBag.parentRevenueSummery = lstParentRevenueSummery;

            List<ViewByModel> lstParentRevenueToPlan = new List<ViewByModel>();
            lstParentRevenueToPlan.Add(new ViewByModel { Text = Common.RevenueOrganization, Value = Common.RevenueOrganization });
            lstParentRevenueToPlan.Add(new ViewByModel { Text = Common.RevenueVertical, Value = Common.RevenueVertical });
            lstParentRevenueToPlan.Add(new ViewByModel { Text = Common.CustomLabelFor(Enums.CustomLabelCode.Audience), Value = Common.RevenueAudience });
            lstParentRevenueToPlan.Add(new ViewByModel { Text = Common.RevenueGeography, Value = Common.RevenueGeography });
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count != 1)
            {
                lstParentRevenueToPlan.Add(new ViewByModel { Text = Common.RevenueBusinessUnit, Value = Common.RevenueBusinessUnit });
            }
            lstParentRevenueToPlan = lstParentRevenueToPlan.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            lstCustomFieldsTactics = lstCustomFieldsTactics.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            lstParentRevenueToPlan = lstParentRevenueToPlan.Concat(lstCustomFieldsTactics).ToList();
            ViewBag.parentRevenueToPlan = lstParentRevenueToPlan;

            List<ViewByModel> lstParentRevenueContribution = new List<ViewByModel>();
            lstParentRevenueContribution.Add(new ViewByModel { Text = Common.CustomLabelFor(Enums.CustomLabelCode.Audience), Value = Common.RevenueAudience });
            lstParentRevenueContribution.Add(new ViewByModel { Text = Common.RevenueGeography, Value = Common.RevenueGeography });
            lstParentRevenueContribution.Add(new ViewByModel { Text = Common.RevenueVertical, Value = Common.RevenueVertical });
            lstParentRevenueContribution.Add(new ViewByModel { Text = Common.RevenueCampaign, Value = Common.RevenueCampaign });
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count != 1)
            {
                lstParentRevenueContribution.Add(new ViewByModel { Text = Common.RevenueBusinessUnit, Value = Common.RevenueBusinessUnit });
            }
            lstParentRevenueContribution = lstParentRevenueContribution.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            lstCustomFieldsTactics = lstCustomFieldsTactics.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            lstParentRevenueContribution = lstParentRevenueContribution.Concat(lstCustomFieldsTactics).ToList();
            ViewBag.parentRevenueContribution = lstParentRevenueContribution;


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
            //Custom
            List<int> entityids = new List<int>();
            if (ParentLabel.Contains(Common.CustomTitle))
            {
                int customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.CustomTitle, ""));
                string customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                entityids = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && c.Value == id).Select(c => c.EntityId).ToList();
            }
            Tacticdata = Tacticdata.Where(pcpt => ((ParentLabel == Common.RevenueBusinessUnit && pcpt.TacticObj.BusinessUnitId == new Guid(id)) ||
                (ParentLabel == Common.RevenueGeography && pcpt.TacticObj.GeographyId == new Guid(id)) ||
                (ParentLabel == Common.RevenueAudience && pcpt.TacticObj.AudienceId == Convert.ToInt32(id)) ||
                (ParentLabel == Common.RevenueVertical && pcpt.TacticObj.VerticalId == Convert.ToInt32(id)) ||
                (ParentLabel == Common.RevenuePlans && pcpt.TacticObj.Plan_Campaign_Program.Plan_Campaign.PlanId == Convert.ToInt32(id)) ||
                (ParentLabel.Contains(Common.CustomTitle) && entityids.Contains(pcpt.TacticObj.PlanTacticId))
                )).ToList();
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
            List<Plan_Campaign_Program_Tactic> TacticList = GetTacticForReporting(selectOption);
            List<int> campaignIds = TacticList.Where(t => t.BusinessUnitId == id).Select(t => t.Plan_Campaign_Program.PlanCampaignId).Distinct().ToList<int>();
            var campaignList = db.Plan_Campaign.Where(pc => campaignIds.Contains(pc.PlanCampaignId))
                    .Select(pcp => new { PlanCampaignId = pcp.PlanCampaignId, Title = pcp.Title })
                    .OrderBy(pcp => pcp.Title).ToList();
            if (campaignList == null)
                return Json(new { });
            campaignList = campaignList.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
            return Json(campaignList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Program List.
        /// </summary>
        /// <param name="id"> Id</param>
        /// <returns>Returns Json Result of Program List.</returns>
        public JsonResult LoadProgramDropDown(string id, string type = "", string selectOption = "")
        {
            List<Plan_Campaign_Program_Tactic> TacticList = GetTacticForReporting(selectOption);
            List<int> programIds = new List<int>();

            if (type == Common.RevenueBusinessUnit)
            {
                Guid businessunitid = new Guid(id);
                programIds = TacticList.Where(t => t.BusinessUnitId == businessunitid).Select(t => t.PlanProgramId).Distinct().ToList<int>();
            }
            else
            {
                int campaignid = Convert.ToInt32(id);
                programIds = TacticList.Where(t => t.Plan_Campaign_Program.PlanCampaignId == campaignid).Select(t => t.PlanProgramId).Distinct().ToList<int>();
            }
            var programList = db.Plan_Campaign_Program.Where(pc => programIds.Contains(pc.PlanProgramId))
                    .Select(c => new { PlanProgramId = c.PlanProgramId, Title = c.Title })
                    .OrderBy(pcp => pcp.Title).ToList();
            if (programList == null)
                return Json(new { });
            programList = programList.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
            return Json(programList, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Tactic List.
        /// </summary>
        /// <param name="id"> Id</param>
        /// <returns>Returns Json Result of Tactic List.</returns>
        public JsonResult LoadTacticDropDown(string id, string type = "", string selectOption = "")
        {
            List<Plan_Campaign_Program_Tactic> TacticList = GetTacticForReporting(selectOption);
            if (type == Common.RevenueBusinessUnit)
            {
                Guid businessunitid = new Guid(id);
                var tacticListinner = TacticList.Where(t => t.BusinessUnitId == businessunitid)
                    .Select(t => new { PlanTacticId = t.PlanTacticId, Title = t.Title })
                    .OrderBy(pcp => pcp.Title).ToList();
                if (tacticListinner == null)
                    return Json(new { });
                tacticListinner = tacticListinner.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
                return Json(tacticListinner, JsonRequestBehavior.AllowGet);
            }
            else if (type == Common.RevenueCampaign)
            {
                int campaignid = Convert.ToInt32(id);
                var tacticListinner = TacticList.Where(t => t.Plan_Campaign_Program.PlanCampaignId == campaignid)
                    .Select(t => new { PlanTacticId = t.PlanTacticId, Title = t.Title })
                    .OrderBy(pcp => pcp.Title).ToList();
                if (tacticListinner == null)
                    return Json(new { });
                tacticListinner = tacticListinner.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
                return Json(tacticListinner, JsonRequestBehavior.AllowGet);
            }
            else
            {
                int programid = Convert.ToInt32(id);
                var tacticListinner = TacticList.Where(t => t.PlanProgramId == programid)
                .Select(t => new { PlanTacticId = t.PlanTacticId, Title = t.Title })
                .OrderBy(pcp => pcp.Title).ToList();
                if (tacticListinner == null)
                    return Json(new { });
                tacticListinner = tacticListinner.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
                return Json(tacticListinner, JsonRequestBehavior.AllowGet);
            }
            return Json(new { });
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
            }
            //Custom
            if (parentlabel.Contains(Common.CustomTitle))
            {
                int customfieldId = Convert.ToInt32(parentlabel.Replace(Common.CustomTitle, ""));
                List<int> entityids = db.CustomField_Entity.Where(e => e.CustomFieldId == customfieldId).Select(e => e.EntityId).ToList();
                Tacticdata = Tacticdata.Where(t => entityids.Contains(t.TacticObj.PlanTacticId)).ToList();
            }
            else
            {
                Tacticdata = Tacticdata.Where(pcpt =>
                    ((parentlabel == Common.RevenueBusinessUnit && pcpt.TacticObj.BusinessUnit.ClientId == Sessions.User.ClientId) ||
                    (parentlabel == Common.RevenueAudience && pcpt.TacticObj.Audience.ClientId == Sessions.User.ClientId) ||
                    (parentlabel == Common.RevenueGeography && pcpt.TacticObj.Geography.ClientId == Sessions.User.ClientId) ||
                    (parentlabel == Common.RevenueVertical && pcpt.TacticObj.Vertical.ClientId == Sessions.User.ClientId) ||
                    (parentlabel == Common.RevenueCampaign))
                    ).ToList();
            }
            if (isBusinessUnit)
            {
                Tacticdata = Tacticdata.Where(c => c.TacticObj.BusinessUnitId == buid).ToList();
            }
            var campaignList = new List<RevenueContrinutionData>();

            if (parentlabel == Common.RevenueCampaign)
            {
                campaignList = Tacticdata.GroupBy(pc => new { title = pc.TacticObj.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pc =>
                     new RevenueContrinutionData
                     {
                         Title = pc.Key.title,
                         planTacticList = pc.Select(p => p.TacticObj.PlanTacticId).ToList()
                     }).ToList();
            }
            else if (parentlabel == Common.RevenueVertical)
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
            else if (parentlabel.Contains(Common.CustomTitle))
            {
                int customfieldId = Convert.ToInt32(parentlabel.Replace(Common.CustomTitle, ""));
                List<int> entityids = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
                string customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();
                if (customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                {
                    var optionlist = cusomfieldEntity.Select(c => Convert.ToInt32(c.Value)).ToList();
                    campaignList = (from cfo in db.CustomFieldOptions
                                    where cfo.CustomFieldId == customfieldId && optionlist.Contains(cfo.CustomFieldOptionId)
                                    select cfo).ToList().GroupBy(pc => new { id = pc.CustomFieldOptionId, title = pc.Value }).Select(pc =>
                                  new RevenueContrinutionData
                                  {
                                      Title = pc.Key.title,
                                      planTacticList = cusomfieldEntity.Where(c => c.Value == pc.Key.id.ToString()).Select(c => c.EntityId).ToList()
                                  }).ToList();

                }
                else if (customFieldType == Enums.CustomFieldType.TextBox.ToString())
                {
                    campaignList = cusomfieldEntity.GroupBy(pc => new { title = pc.Value }).Select(pc =>
                                new RevenueContrinutionData
                                {
                                    Title = pc.Key.title,
                                    planTacticList = pc.Select(c => c.EntityId).ToList()
                                }).ToList();
                }
            }

            if (campaignList.Count() > 0)
            {
                string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();

                //Added By : Kalpesh Sharma #734 Actual cost - Verify that report section is up to date with actual cost changes
                string cost = Enums.InspectStageValues[Enums.InspectStage.Cost.ToString()].ToString();

                int lastMonth = GetLastMonthForTrend(selectOption);
                List<string> includeMonth = GetMonthListForReport(selectOption, true);
                List<string> monthList = GetUpToCurrentMonth();
                DataTable ProjectedRevenueDatatable = GetProjectedRevenueValueDataTableForReport(Tacticdata);
                DataTable ProjectedCostDatatable = GetProjectedCostData(Tacticdata);

                List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));

                List<Plan_Tactic_Values> ActualRevenueTrendList = GetTrendRevenueDataContribution(ActualTacticList, lastMonth, monthList, revenue);
                List<string> monthWithYearList = GetUpToCurrentMonthWithYearForReport(selectOption, true);

                List<int> ObjTactic = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
                List<Plan_Campaign_Program_Tactic_LineItem> LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => ObjTactic.Contains(l.PlanTacticId) && l.IsDeleted == false).ToList();
                List<int> ObjTacticLineItemList = LineItemList.Select(t => t.PlanLineItemId).ToList();
                List<Plan_Campaign_Program_Tactic_LineItem_Actual> LineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(l => ObjTacticLineItemList.Contains(l.PlanLineItemId)).ToList();
                DataTable ActualCostDatatable = GetActualCostData(Tacticdata, LineItemList, LineItemActualList);

                var campaignListFinal = campaignList.Select(p => new
                {
                    Title = p.Title,
                    PlanRevenue = ProjectedRevenueDatatable.AsEnumerable().AsQueryable().Where(mr => p.planTacticList.Contains(mr.Field<int>(ColumnId)) && includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                    ActualRevenue = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => p.planTacticList.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue) && includeMonth.Contains(ta.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + ta.Period)).ToList().Sum(ta => ta.Actualvalue),
                    TrendRevenue = 0,//GetTrendRevenueDataContribution(p.planTacticList, lastMonth),
                    PlanCost = ProjectedCostDatatable.AsEnumerable().AsQueryable().Where(mr => p.planTacticList.Contains(mr.Field<int>(ColumnId)) && includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                    //Added By : Kalpesh Sharma #734 Actual cost - Verify that report section is up to date with actual cost changes
                    ActualCost = ActualCostDatatable.AsEnumerable().AsQueryable().Where(mr => p.planTacticList.Contains(mr.Field<int>(ColumnId)) && includeMonth.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue)),
                    TrendCost = 0,//GetTrendCostDataContribution(p.planTacticList, lastMonth),
                    RunRate = ActualRevenueTrendList.Where(ar => p.planTacticList.Contains(ar.PlanTacticId)).Sum(ar => ar.MQL),//GetTrendRevenueDataContribution(p.planTacticList, lastMonth, monthList),
                    PipelineCoverage = 0,//GetPipelineCoverage(p.planTacticList, lastMonth),
                    RevSpend = GetRevenueVSSpendContribution(ActualCostDatatable, ActualTacticList, p.planTacticList, monthWithYearList, monthList, revenue),
                    RevenueTotal = GetActualRevenueTotal(ActualTacticList, p.planTacticList, monthList, revenue),
                    CostTotal = ActualCostDatatable.AsEnumerable().AsQueryable().Where(mr => p.planTacticList.Contains(mr.Field<int>(ColumnId)) && monthWithYearList.Contains(mr.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue))
                }).Select(p => p).Distinct().OrderBy(p => p.Title);

                return Json(campaignListFinal, JsonRequestBehavior.AllowGet);
            }
            return Json(new { });
        }


        private DataTable GetActualCostData(List<TacticStageValue> Tacticdata, List<Plan_Campaign_Program_Tactic_LineItem> LineItemList, List<Plan_Campaign_Program_Tactic_LineItem_Actual> LineItemActualList)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(ColumnId, typeof(int));
            dt.Columns.Add(ColumnMonth, typeof(string));
            dt.Columns.Add(ColumnValue, typeof(double));
            foreach (var t in Tacticdata)
            {
                int id = t.TacticObj.PlanTacticId;
                var InnerLineItemList = LineItemList.Where(l => l.PlanTacticId == id).ToList();
                if (InnerLineItemList.Count() > 0)
                {
                    var innerLineItemActualList = LineItemActualList.Where(la => InnerLineItemList.Select(line => line.PlanLineItemId).Contains(la.PlanLineItemId)).ToList();
                    innerLineItemActualList.ForEach(innerline => dt.Rows.Add(id, t.TacticYear + innerline.Period, innerline.Value));
                }
                else
                {
                    var innerTacticActualList = t.ActualTacticList.Where(a => a.StageTitle == Enums.InspectStage.Cost.ToString()).ToList();
                    innerTacticActualList.ForEach(innerTactic => dt.Rows.Add(id, t.TacticYear + innerTactic.Period, innerTactic.Actualvalue));
                }
            }

            return dt;
        }

        /// <summary>
        /// Get Projected Cost Data With Month Wise.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public DataTable GetProjectedCostData(List<TacticStageValue> tacticDataList)
        {
            List<TacticDataTable> tacticdata = new List<TacticDataTable>();
            tacticdata = tacticDataList.Select(
                td => new TacticDataTable
                {
                    TacticId = td.TacticObj.PlanTacticId,
                    Value = td.TacticObj.Cost,
                    StartMonth = td.TacticObj.StartDate.Month,
                    EndMonth = td.TacticObj.EndDate.Month,
                    StartYear = td.TacticObj.StartDate.Year,
                    EndYear = td.TacticObj.EndDate.Year
                }).ToList();

            return GetDatatable(tacticdata);
        }

        /// <summary>
        /// Get Trend.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public List<Plan_Tactic_Values> GetTrendRevenueDataContribution(List<Plan_Campaign_Program_Tactic_Actual> planTacticList, int lastMonth, List<string> monthList, string revenue)
        {
            return planTacticList.Where(ta => monthList.Contains(ta.Period) && ta.StageTitle == revenue).GroupBy(t => t.PlanTacticId).Select(pt => new Plan_Tactic_Values
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
        public double GetRevenueVSSpendContribution(DataTable ActualDT, List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList, List<int> planTacticList, List<string> monthWithYearList, List<string> monthList, string revenue)
        {
            double costTotal = ActualDT.AsEnumerable().AsQueryable().Where(c => planTacticList.Contains(c.Field<int>(ColumnId)) && monthWithYearList.Contains(c.Field<string>(ColumnMonth))).Sum(r => r.Field<double>(ColumnValue));

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
            //Custom
            List<int> entityids = new List<int>();
            if (ParentLabel.Contains(Common.CustomTitle))
            {
                int customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.CustomTitle, ""));
                string customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                entityids = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && c.Value == id).Select(c => c.EntityId).ToList();
            }
            Tacticdata = Tacticdata.Where(pcpt => ((ParentLabel == Common.RevenueBusinessUnit && pcpt.TacticObj.BusinessUnitId == new Guid(id)) ||
                                                                (ParentLabel == Common.RevenueAudience && pcpt.TacticObj.AudienceId == Convert.ToInt32(id)) ||
                                                                (ParentLabel == Common.RevenueGeography && pcpt.TacticObj.GeographyId == new Guid(id)) ||
                                                                (ParentLabel == Common.RevenueVertical && pcpt.TacticObj.VerticalId == Convert.ToInt32(id) ||
                                                                (ParentLabel == Common.RevenueOrganization) ||
                                                                (ParentLabel.Contains(Common.CustomTitle) && entityids.Contains(pcpt.TacticObj.PlanTacticId))
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
            ActualTacticList = ActualTacticList.Where(mr => mr.StageTitle == Revenue && includeMonthUpCurrent.Contains((Tacticdata.Single(t => t.TacticObj.PlanTacticId == mr.PlanTacticId).TacticYear) + mr.Period)).ToList();


            var businessUnits = db.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId) && b.IsDeleted == false).ToList()
                                                .Select(b => new
                                                {
                                                    Title = b.Title,
                                                    ColorCode = string.Format("#{0}", b.ColorCode),
                                                    Value = GetActualVSPlannedRevenue(ActualTacticList, ProjectedRevenueDataTable, Tacticdata.Where(t => t.TacticObj.BusinessUnitId.Equals(b.BusinessUnitId)).Select(t => t.TacticObj.PlanTacticId).ToList(), includeMonthUpCurrent)
                                                }).OrderBy(b => b.Title);
            var vertical = db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted == false).ToList()
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = GetActualVSPlannedRevenue(ActualTacticList, ProjectedRevenueDataTable, Tacticdata.Where(t => t.TacticObj.VerticalId.Equals(v.VerticalId)).Select(t => t.TacticObj.PlanTacticId).ToList(), includeMonthUpCurrent)
                                                }).OrderBy(v => v.Title);

            var geography = db.Geographies.Where(g => g.ClientId.Equals(Sessions.User.ClientId) && g.IsDeleted == false).ToList()
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
                ////Start - Modified by Mitesh Vaishnav for PL ticket #611 Source Performance Graphs dont show anything
                if (projectedRevenueValue != 0)
                {
                    percentageValue = Math.Round((actualRevenueValue / projectedRevenueValue) * 100, 2);
                }
                ////End - Modified by Mitesh Vaishnav for PL ticket #611 Source Performance Graphs dont show anything
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

            /*Modified By Maninder Singh Wadhva on  10/17/2014 for ticket #865 	Custom fields & Report filter - Review changes on reports PDF*/
            int marginTopBottom = 50;
            pdfConverter.PdfDocumentOptions.TopMargin = marginTopBottom;
            pdfConverter.PdfDocumentOptions.BottomMargin = marginTopBottom;

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
            html += "<head>";
            html += string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/bootstrap.css"));
            html += string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/bootstrap-responsive.css"));
            html += string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/style.css"));
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

            /*Modified By Maninder Singh Wadhva on  10/17/2014 for ticket #865 	Custom fields & Report filter - Review changes on reports PDF*/
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.multiselect.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.multiselect.filter.js"));

            html += string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/font-awesome.min.css"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/modernizr-2.5.3.js"));

            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/dhtmlxchart.js"));
            html += string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/dhtmlxchart.css"));

            /*Modified By Maninder Singh Wadhva on  10/17/2014 for ticket #865 	Custom fields & Report filter - Review changes on reports PDF*/
            html += string.Format("<link rel='stylesheet' href='{0}' type='text/css' />", Server.MapPath("~/Content/css/jquery.multiselect.css"));

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

        #region Budget

        #region Constant variable declaration those needs to move to common
        public const string Jan = "Y1";
        public const string Feb = "Y2";
        public const string Mar = "Y3";
        public const string Apr = "Y4";
        public const string May = "Y5";
        public const string Jun = "Y6";
        public const string Jul = "Y7";
        public const string Aug = "Y8";
        public const string Sep = "Y9";
        public const string Oct = "Y10";
        public const string Nov = "Y11";
        public const string Dec = "Y12";
        #endregion

        /// <summary>
        /// View Budget 
        /// </summary>
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.ReportView)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #519 to implement user permission Logic
        public ActionResult GetBudget()
        {
            string planIds = string.Join(",", Sessions.ReportPlanIds.Select(n => n.ToString()).ToArray());
            List<int> TacticId = Common.GetTacticByPlanIDs(planIds);
            List<ViewByModel> lstViewByTab = new List<ViewByModel>();
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Plan.ToString(), Value = ReportTabType.Plan.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Vertical.ToString(), Value = ReportTabType.Vertical.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Geography.ToString(), Value = ReportTabType.Geography.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.BusinessUnit.ToString(), Value = ReportTabType.BusinessUnit.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = Common.CustomLabelFor(Enums.CustomLabelCode.Audience), Value = ReportTabType.Audience.ToString() });

            var lstCustomFields = Common.GetTacticsCustomFields(TacticId);
            lstViewByTab = lstViewByTab.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            lstCustomFields = lstCustomFields.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            lstViewByTab = lstViewByTab.Concat(lstCustomFields).ToList();
            ViewBag.ViewByTab = lstViewByTab;

            List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
            lstViewByAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarterly", Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstViewByAllocated = lstViewByAllocated.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewByAllocated = lstViewByAllocated;

            List<SelectListItem> lstGeography = new List<SelectListItem>();
            lstGeography = db.Geographies.Where(g => g.ClientId == Sessions.User.ClientId && g.IsDeleted == false).ToList().Select(g => new SelectListItem { Text = g.Title, Value = g.GeographyId.ToString(), Selected = true }).ToList();
            ViewBag.ViewGeography = lstGeography.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();

            List<SelectListItem> lstBusinessUnit = new List<SelectListItem>();
            lstBusinessUnit = db.BusinessUnits.Where(b => b.ClientId == Sessions.User.ClientId && b.IsDeleted == false).ToList().Select(b => new SelectListItem { Text = b.Title, Value = b.BusinessUnitId.ToString(), Selected = true }).ToList();
            ViewBag.ViewBusinessUnit = lstBusinessUnit.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();

            List<SelectListItem> lstVertical = new List<SelectListItem>();
            lstVertical = db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted == false).ToList().Select(v => new SelectListItem { Text = v.Title, Value = v.VerticalId.ToString(), Selected = true }).ToList();
            ViewBag.ViewVertical = lstVertical.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();

            List<SelectListItem> lstAudience = new List<SelectListItem>();
            lstAudience = db.Audiences.Where(a => a.ClientId == Sessions.User.ClientId && a.IsDeleted == false).ToList().Select(a => new SelectListItem { Text = a.Title, Value = a.AudienceId.ToString(), Selected = true }).ToList();
            ViewBag.ViewAudience = lstAudience.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();

            string published = Enums.PlanStatus.Published.ToString();
            List<SelectListItem> lstYear = new List<SelectListItem>();
            var lstPlan = db.Plans.Where(p => p.IsDeleted == false && p.Status == published && p.Model.BusinessUnit.ClientId == Sessions.User.ClientId).ToList();
            var yearlist = lstPlan.OrderBy(p => p.Year).Select(p => p.Year).Distinct().ToList();
            yearlist.ForEach(year => lstYear.Add(new SelectListItem { Text = "FY " + year, Value = year }));


            string defaultallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString();
            string Noneallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString();

            List<SelectListItem> lstPlanList = new List<SelectListItem>();
            string planyear = DateTime.Now.Year.ToString();

            lstPlanList = lstPlan.Where(p => p.Year == planyear).Select(p => new SelectListItem { Text = p.Title + " - " + (p.AllocatedBy == defaultallocatedby ? Noneallocatedby : p.AllocatedBy), Value = p.PlanId.ToString() + "_" + p.AllocatedBy, Selected = (Sessions.ReportPlanIds.Contains(p.PlanId) ? true : false) }).ToList();
            ViewBag.ViewPlan = lstPlanList.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewYear = lstYear.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            ViewBag.SelectedYear = planyear;

            return PartialView("Budget");
        }

        /// <summary>
        /// Load Plan values
        /// </summary>
        /// <param name="BusinessUnitId"></param>
        /// <param name="isBusinessUnit"></param>
        /// <returns></returns>
        public JsonResult GetBudgetPlanBasedOnYear(string Year)
        {
            string published = Enums.PlanStatus.Published.ToString();
            string defaultallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString();
            string Noneallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString();
            var planList = db.Plans.Where(p => p.Year == Year && p.IsDeleted == false && p.Status == published && p.Model.BusinessUnit.ClientId == Sessions.User.ClientId).ToList().Select(p => new
            {
                Text = p.Title + " - " + (p.AllocatedBy == defaultallocatedby ? Noneallocatedby : p.AllocatedBy),
                Value = p.PlanId.ToString() + "_" + p.AllocatedBy
            }).ToList();
			planList = planList.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            return Json(planList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Data For Budget Report
        /// </summary>
        /// <param name="PlanIds"></param>
        /// <param name="GeographyIds"></param>
        /// <param name="BusinessUnitIds"></param>
        /// <param name="VerticalIds"></param>
        /// <param name="AudienceIds"></param>
        /// <param name="Year"></param>
        /// <param name="AllocatedBy"></param>
        /// <param name="Tab"></param>
        /// <param name="SortingId"></param>
        /// <returns></returns>
        public ActionResult GetReportBudgetData(string GeographyIds, string VerticalIds, string AudienceIds, string Year, string AllocatedBy, string Tab, string SortingId)
        {
            if (Year == "thisquarter")
            {
                Year = DateTime.Now.Year.ToString();
            }
            List<int> PlanIdList = new List<int>();
            if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count > 0)
            {
                PlanIdList = Sessions.ReportPlanIds;
            }

            List<int> AudienceIdList = new List<int>();
            if (AudienceIds.ToString() != string.Empty)
            {
                AudienceIdList = AudienceIds.Split(',').Select(int.Parse).ToList<int>();
            }

            List<int> VerticalIdList = new List<int>();
            if (VerticalIds.ToString() != string.Empty)
            {
                VerticalIdList = VerticalIds.Split(',').Select(int.Parse).ToList<int>();
            }

            List<Guid> BusinessUnitIdList = new List<Guid>();
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count > 0)
            {
                BusinessUnitIdList = Sessions.ReportBusinessUnitIds;
            }


            List<Guid> GeographyIdList = new List<Guid>();
            if (GeographyIds.ToString() != string.Empty)
            {
                GeographyIdList = GeographyIds.Split(',').Select(Guid.Parse).ToList<Guid>();
            }

            //int PlanId = 3533;
            // Apply filter on tactic
            var tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic =>
                            PlanIdList.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)
                            && BusinessUnitIdList.Contains(tactic.BusinessUnitId)
                            && GeographyIdList.Contains(tactic.GeographyId)
                            && VerticalIdList.Contains(tactic.VerticalId)
                            && AudienceIdList.Contains(tactic.AudienceId)
                            && tactic.IsDeleted.Equals(false)
                            ).ToList();

            List<int> TacticIds = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
            var LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => TacticIds.Contains(lineitem.PlanTacticId) && lineitem.IsDeleted.Equals(false)).ToList();
            List<int> ProgramIds = tacticList.Select(tactic => tactic.PlanProgramId).ToList();
            var ProgramList = db.Plan_Campaign_Program.Where(program => ProgramIds.Contains(program.PlanProgramId) && program.IsDeleted.Equals(false)).ToList();
            List<int> CampaignIds = ProgramList.Select(tactic => tactic.PlanCampaignId).ToList();
            var CampaignList = db.Plan_Campaign.Where(campaign => CampaignIds.Contains(campaign.PlanCampaignId) && campaign.IsDeleted.Equals(false)).ToList();
            List<int> PlanIdsInner = CampaignList.Select(campaign => campaign.PlanId).ToList();
            List<int> AudienceIdsInner = tacticList.Select(tactic => tactic.AudienceId).Distinct().ToList();
            List<Guid> BusinessUnitIdsInner = tacticList.Select(tactic => tactic.BusinessUnitId).Distinct().ToList();
            List<int> VerticalIdsInner = tacticList.Select(tactic => tactic.VerticalId).Distinct().ToList();
            List<Guid> GeographyIdsInner = tacticList.Select(tactic => tactic.GeographyId).Distinct().ToList();

            List<string> AfterApprovedStatus = Common.GetStatusListAfterApproved();

            List<BudgetedValue> EmptyBudgetList = new List<BudgetedValue>();
            EmptyBudgetList = GetEmptyList();

            List<BudgetModelReport> model = new List<BudgetModelReport>();
            BudgetModelReport obj = new BudgetModelReport();
            obj.Id = "0";
            obj.ActivityId = "main_0";
            obj.ActivityName = "";
            obj.ActivityType = ActivityType.ActivityMain;
            obj.ParentActivityId = "0";
            obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Planned.ToString());
            obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
            obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Allocated.ToString());
            model.Add(obj);
            string parentPlanId = "0", parentCampaignId = "0", parentProgramId = "0", parentTacticId = "0";
            string parentMainId = "main_0";
            if (Tab == ReportTabType.Plan.ToString())
            {

                var planobj = db.Plans.Where(plan => PlanIdsInner.Contains(plan.PlanId)).ToList();
                if (planobj != null)
                {
                    foreach (var p in planobj)
                    {
                        obj = new BudgetModelReport();
                        //List<BudgetedValue> lst = (from bv in p.Plan_Budget
                        //                           select new BudgetedValue
                        //                           {
                        //                               Period = bv.Period,
                        //                               Value = bv.Value
                        //                           }).ToList();
                        obj.Id = p.PlanId.ToString();
                        obj.ActivityId = "plan_" + p.PlanId.ToString();
                        obj.ActivityName = p.Title;
                        obj.ActivityType = ActivityType.ActivityPlan;
                        obj.ParentActivityId = parentMainId;
                        obj.TabActivityId = p.PlanId.ToString();
                        obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Planned.ToString());
                        obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                        obj = GetMonthWiseDataReport(obj, p.Plan_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Allocated.ToString());
                        model.Add(obj);
                        parentPlanId = "plan_" + p.PlanId.ToString();

                        var campaignObj = CampaignList.Where(c => c.PlanId == p.PlanId).ToList();
                        foreach (var c in campaignObj)
                        {
                            obj = new BudgetModelReport();
                            obj.Id = c.PlanCampaignId.ToString();
                            obj.ActivityId = "c_" + c.PlanCampaignId.ToString();
                            obj.ActivityName = c.Title;
                            obj.ActivityType = ActivityType.ActivityCampaign;
                            obj.ParentActivityId = parentPlanId;
                            obj.TabActivityId = p.PlanId.ToString();
                            obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Planned.ToString());
                            obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                            obj = GetMonthWiseDataReport(obj, c.Plan_Campaign_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Allocated.ToString());
                            model.Add(obj);
                            parentCampaignId = "c_" + c.PlanCampaignId.ToString();
                            var ProgramObj = ProgramList.Where(pr => pr.PlanCampaignId == c.PlanCampaignId).ToList();
                            foreach (var pr in ProgramObj)
                            {
                                obj = new BudgetModelReport();
                                obj.Id = pr.PlanProgramId.ToString();
                                obj.ActivityId = "cp_" + pr.PlanProgramId.ToString();
                                obj.ActivityName = pr.Title;
                                obj.ActivityType = ActivityType.ActivityProgram;
                                obj.ParentActivityId = parentCampaignId;
                                obj.TabActivityId = p.PlanId.ToString();
                                obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Planned.ToString());
                                obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                                obj = GetMonthWiseDataReport(obj, pr.Plan_Campaign_Program_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Allocated.ToString());
                                model.Add(obj);
                                parentProgramId = "cp_" + pr.PlanProgramId.ToString();

                                var TacticObj = tacticList.Where(t => t.PlanProgramId == pr.PlanProgramId).ToList();
                                foreach (var t in TacticObj)
                                {
                                    obj = new BudgetModelReport();
                                    obj.Id = t.PlanTacticId.ToString();
                                    obj.ActivityId = "cpt_" + t.PlanTacticId.ToString();
                                    obj.ActivityName = t.Title;
                                    obj.ActivityType = ActivityType.ActivityTactic;
                                    obj.ParentActivityId = parentProgramId;
                                    obj.TabActivityId = p.PlanId.ToString();
                                    obj = GetMonthWiseDataReport(obj, t.Plan_Campaign_Program_Tactic_Cost.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Planned.ToString());
                                    obj = AfterApprovedStatus.Contains(t.Status) ? GetMonthWiseDataReport(obj, t.Plan_Campaign_Program_Tactic_Actual.Where(b => b.StageTitle == "Cost").Select(b => new BudgetedValue { Period = b.Period, Value = b.Actualvalue }).ToList(), ReportColumnType.Actual.ToString()) : GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                                    obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Allocated.ToString());
                                    model.Add(obj);
                                    parentTacticId = "cpt_" + t.PlanTacticId.ToString();

                                    var LineItemObj = LineItemList.Where(l => l.PlanTacticId == t.PlanTacticId).ToList();
                                    foreach (var l in LineItemObj)
                                    {
                                        obj = new BudgetModelReport();
                                        obj.Id = l.PlanLineItemId.ToString();
                                        obj.ActivityId = "cptl_" + l.PlanLineItemId.ToString();
                                        obj.ActivityName = l.Title;
                                        obj.ActivityType = ActivityType.ActivityLineItem;
                                        obj.ParentActivityId = parentTacticId;
                                        obj.TabActivityId = p.PlanId.ToString();
                                        obj = GetMonthWiseDataReport(obj, l.Plan_Campaign_Program_Tactic_LineItem_Cost.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Planned.ToString());
                                        obj = AfterApprovedStatus.Contains(t.Status) ? GetMonthWiseDataReport(obj, l.Plan_Campaign_Program_Tactic_LineItem_Actual.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Actual.ToString()) : GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                                        obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Allocated.ToString());
                                        model.Add(obj);
                                    }

                                }
                            }


                        }
                    }
                }
            }
            else
            {
                int customfieldId = 0;
                List<BudgetReportTab> planobj = new List<BudgetReportTab>();
                if (Tab == ReportTabType.Audience.ToString())
                {
                    planobj = db.Audiences.Where(a => AudienceIdsInner.Contains(a.AudienceId)).ToList().Select(a => new BudgetReportTab { Id = a.AudienceId.ToString(), Title = a.Title }).ToList();
                }
                if (Tab == ReportTabType.BusinessUnit.ToString())
                {
                    planobj = db.BusinessUnits.Where(a => BusinessUnitIdsInner.Contains(a.BusinessUnitId)).ToList().Select(a => new BudgetReportTab { Id = a.BusinessUnitId.ToString(), Title = a.Title }).ToList();
                }
                else if (Tab == ReportTabType.Geography.ToString())
                {
                    planobj = db.Geographies.Where(a => GeographyIdsInner.Contains(a.GeographyId)).ToList().Select(a => new BudgetReportTab { Id = a.GeographyId.ToString(), Title = a.Title }).ToList();
                }
                else if (Tab == ReportTabType.Vertical.ToString())
                {
                    planobj = db.Verticals.Where(a => VerticalIdsInner.Contains(a.VerticalId)).ToList().Select(a => new BudgetReportTab { Id = a.VerticalId.ToString(), Title = a.Title }).ToList();
                }
                else if (Tab.Contains(Common.CustomTitle))
                {
                    customfieldId = Convert.ToInt32(Tab.Replace(Common.CustomTitle, ""));
                    string customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();

                    if (customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                    {
                        var optionlist = db.CustomFieldOptions.Where(co => co.CustomFieldId == customfieldId).ToList();
                        planobj = optionlist.Select(p => new BudgetReportTab
                        {
                            Id = p.CustomFieldOptionId.ToString(),
                            Title = p.Value
                        }).Select(b => b).Distinct().OrderBy(b => b.Title).ToList();
                    }
                    else if (customFieldType == Enums.CustomFieldType.TextBox.ToString())
                    {
                        List<int> entityids = tacticList.Select(t => t.PlanTacticId).ToList();
                        var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();
                        planobj = cusomfieldEntity.Select(p => new BudgetReportTab
                        {
                            Id = p.Value,
                            Title = p.Value
                        }).Select(b => b).Distinct().OrderBy(b => b.Title).ToList();
                    }

                }


                if (planobj != null)
                {
                    foreach (var p in planobj)
                    {
                        obj = new BudgetModelReport();
                        obj.Id = p.Id.ToString();
                        obj.ActivityId = "plan_" + p.Id.ToString();
                        obj.ActivityName = p.Title;
                        obj.ActivityType = ActivityType.ActivityPlan;
                        obj.ParentActivityId = parentMainId;
                        obj.TabActivityId = p.Id.ToString();
                        obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Planned.ToString());
                        obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                        obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Allocated.ToString());
                        model.Add(obj);
                        parentPlanId = "plan_" + p.Id.ToString();

                        var TacticListInner = tacticList;

                        if (Tab == ReportTabType.Audience.ToString())
                        {
                            TacticListInner = tacticList.Where(tactic => tactic.AudienceId.ToString() == p.Id).ToList();
                        }
                        if (Tab == ReportTabType.BusinessUnit.ToString())
                        {
                            TacticListInner = tacticList.Where(tactic => tactic.BusinessUnitId.ToString() == p.Id).ToList();
                        }
                        else if (Tab == ReportTabType.Geography.ToString())
                        {
                            TacticListInner = tacticList.Where(tactic => tactic.GeographyId.ToString() == p.Id).ToList();
                        }
                        else if (Tab == ReportTabType.Vertical.ToString())
                        {
                            TacticListInner = tacticList.Where(tactic => tactic.VerticalId.ToString() == p.Id).ToList();
                        }
                        else if (Tab.Contains(Common.CustomTitle))
                        {
                            var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && c.Value == p.Id).ToList();
                            List<int> entityids = cusomfieldEntity.Select(e => e.EntityId).ToList();
                            TacticListInner = tacticList.Where(tactic => entityids.Contains(tactic.PlanTacticId)).ToList();
                        }
                        var ProgramListInner = ProgramList.Where(program => TacticListInner.Select(t => t.PlanProgramId).Contains(program.PlanProgramId)).ToList();
                        var campaignObj = CampaignList.Where(campaign => ProgramListInner.Select(program => program.PlanCampaignId).Contains(campaign.PlanCampaignId)).ToList();

                        foreach (var c in campaignObj)
                        {
                            obj = new BudgetModelReport();
                            obj.Id = c.PlanCampaignId.ToString();
                            obj.ActivityId = "c_" + p.Id + c.PlanCampaignId.ToString();
                            obj.ActivityName = c.Title;
                            obj.ActivityType = ActivityType.ActivityCampaign;
                            obj.ParentActivityId = parentPlanId;
                            obj.TabActivityId = p.Id.ToString();
                            obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Planned.ToString());
                            obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                            obj = GetMonthWiseDataReport(obj, c.Plan_Campaign_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Allocated.ToString());
                            model.Add(obj);
                            parentCampaignId = "c_" + p.Id + c.PlanCampaignId.ToString();
                            var ProgramObj = ProgramListInner.Where(pr => pr.PlanCampaignId == c.PlanCampaignId).ToList();
                            foreach (var pr in ProgramObj)
                            {
                                obj = new BudgetModelReport();
                                obj.Id = pr.PlanProgramId.ToString();
                                obj.ActivityId = "cp_" + p.Id + pr.PlanProgramId.ToString();
                                obj.ActivityName = pr.Title;
                                obj.ActivityType = ActivityType.ActivityProgram;
                                obj.ParentActivityId = parentCampaignId;
                                obj.TabActivityId = p.Id.ToString();
                                obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Planned.ToString());
                                obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                                obj = GetMonthWiseDataReport(obj, pr.Plan_Campaign_Program_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Allocated.ToString());
                                model.Add(obj);
                                parentProgramId = "cp_" + p.Id + pr.PlanProgramId.ToString();

                                var TacticObj = TacticListInner.Where(t => t.PlanProgramId == pr.PlanProgramId).ToList();
                                foreach (var t in TacticObj)
                                {
                                    obj = new BudgetModelReport();
                                    obj.Id = t.PlanTacticId.ToString();
                                    obj.ActivityId = "cpt_" + p.Id + t.PlanTacticId.ToString();
                                    obj.ActivityName = t.Title;
                                    obj.ActivityType = ActivityType.ActivityTactic;
                                    obj.ParentActivityId = parentProgramId;
                                    obj.TabActivityId = p.Id.ToString();
                                    obj = GetMonthWiseDataReport(obj, t.Plan_Campaign_Program_Tactic_Cost.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Planned.ToString());
                                    obj = AfterApprovedStatus.Contains(t.Status) ? GetMonthWiseDataReport(obj, t.Plan_Campaign_Program_Tactic_Actual.Where(b => b.StageTitle == "Cost").Select(b => new BudgetedValue { Period = b.Period, Value = b.Actualvalue }).ToList(), ReportColumnType.Actual.ToString()) : GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                                    obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Allocated.ToString());
                                    model.Add(obj);
                                    parentTacticId = "cpt_" + p.Id + t.PlanTacticId.ToString();

                                    var LineItemObj = LineItemList.Where(l => l.PlanTacticId == t.PlanTacticId).ToList();
                                    foreach (var l in LineItemObj)
                                    {
                                        obj = new BudgetModelReport();
                                        obj.Id = l.PlanLineItemId.ToString();
                                        obj.ActivityId = "cptl_" + l.PlanLineItemId.ToString();
                                        obj.ActivityName = l.Title;
                                        obj.ActivityType = ActivityType.ActivityLineItem;
                                        obj.ParentActivityId = parentTacticId;
                                        obj.TabActivityId = p.Id.ToString();
                                        obj = GetMonthWiseDataReport(obj, l.Plan_Campaign_Program_Tactic_LineItem_Cost.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Planned.ToString());
                                        obj = AfterApprovedStatus.Contains(t.Status) ? GetMonthWiseDataReport(obj, l.Plan_Campaign_Program_Tactic_LineItem_Actual.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Actual.ToString()) : GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                                        obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Allocated.ToString());
                                        model.Add(obj);
                                    }

                                }
                            }


                        }
                    }
                }
            }



            //Threre is no need to manage lines for actuals
            //if (Tab == ReportTabType.Plan.ToString())
            //{
            model = ManageLineItems(model);
            // }

            //Set actual for quarters
            if (AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString())  // Modified by Sohel Pathan on 08/09/2014 for PL ticket #642.
            {
                foreach (BudgetModelReport bm in model)
                {
                    if (bm.ActivityType == ActivityType.ActivityLineItem || bm.ActivityType == ActivityType.ActivityTactic)
                    {
                        bm.MonthActual.Jan = bm.MonthActual.Jan + bm.MonthActual.Feb + bm.MonthActual.Mar;
                        bm.MonthActual.Apr = bm.MonthActual.Apr + bm.MonthActual.May + bm.MonthActual.Jun;
                        bm.MonthActual.Jul = bm.MonthActual.Jul + bm.MonthActual.Aug + bm.MonthActual.Sep;
                        bm.MonthActual.Oct = bm.MonthActual.Oct + bm.MonthActual.Nov + bm.MonthActual.Dec;
                        bm.MonthActual.Feb = 0;
                        bm.MonthActual.Mar = 0;
                        bm.MonthActual.May = 0;
                        bm.MonthActual.Jun = 0;
                        bm.MonthActual.Aug = 0;
                        bm.MonthActual.Sep = 0;
                        bm.MonthActual.Nov = 0;
                        bm.MonthActual.Dec = 0;

                        bm.MonthPlanned.Jan = bm.MonthPlanned.Jan + bm.MonthPlanned.Feb + bm.MonthPlanned.Mar;
                        bm.MonthPlanned.Apr = bm.MonthPlanned.Apr + bm.MonthPlanned.May + bm.MonthPlanned.Jun;
                        bm.MonthPlanned.Jul = bm.MonthPlanned.Jul + bm.MonthPlanned.Aug + bm.MonthPlanned.Sep;
                        bm.MonthPlanned.Oct = bm.MonthPlanned.Oct + bm.MonthPlanned.Nov + bm.MonthPlanned.Dec;
                        bm.MonthPlanned.Feb = 0;
                        bm.MonthPlanned.Mar = 0;
                        bm.MonthPlanned.May = 0;
                        bm.MonthPlanned.Jun = 0;
                        bm.MonthPlanned.Aug = 0;
                        bm.MonthPlanned.Sep = 0;
                        bm.MonthPlanned.Nov = 0;
                        bm.MonthPlanned.Dec = 0;
                    }
                    else
                    {
                        bm.MonthAllocated.Jan = bm.MonthAllocated.Jan + bm.MonthAllocated.Feb + bm.MonthAllocated.Mar;
                        bm.MonthAllocated.Apr = bm.MonthAllocated.Apr + bm.MonthAllocated.May + bm.MonthAllocated.Jun;
                        bm.MonthAllocated.Jul = bm.MonthAllocated.Jul + bm.MonthAllocated.Aug + bm.MonthAllocated.Sep;
                        bm.MonthAllocated.Oct = bm.MonthAllocated.Oct + bm.MonthAllocated.Nov + bm.MonthAllocated.Dec;
                        bm.MonthAllocated.Feb = 0;
                        bm.MonthAllocated.Mar = 0;
                        bm.MonthAllocated.May = 0;
                        bm.MonthAllocated.Jun = 0;
                        bm.MonthAllocated.Aug = 0;
                        bm.MonthAllocated.Sep = 0;
                        bm.MonthAllocated.Nov = 0;
                        bm.MonthAllocated.Dec = 0;
                    }
                }
            }


            model = CalculateBottomUpReport(model, ActivityType.ActivityTactic, ActivityType.ActivityLineItem);
            model = CalculateBottomUpReport(model, ActivityType.ActivityProgram, ActivityType.ActivityTactic);
            model = CalculateBottomUpReport(model, ActivityType.ActivityCampaign, ActivityType.ActivityProgram);
            model = CalculateBottomUpReport(model, ActivityType.ActivityPlan, ActivityType.ActivityCampaign);
            model = CalculateBottomUpReport(model, ActivityType.ActivityMain, ActivityType.ActivityPlan);

            ViewBag.AllocatedBy = AllocatedBy;
            ViewBag.Tab = Tab;

            double MainTotalActual = 0;
            double MainTotalAllocated = 0;
            BudgetMonth ActualTotal = new BudgetMonth();
            BudgetMonth AllocatedTotal = new BudgetMonth();
            BudgetMonth PercAllocated = new BudgetMonth();
            ActualTotal = model.Where(m => m.ActivityType == ActivityType.ActivityMain).Select(m => m.MonthActual).SingleOrDefault();
            MainTotalActual = ActualTotal.Jan + ActualTotal.Feb + ActualTotal.Mar + ActualTotal.Apr + ActualTotal.May + ActualTotal.Jun + ActualTotal.Jul + ActualTotal.Aug + ActualTotal.Sep + ActualTotal.Oct + ActualTotal.Nov + ActualTotal.Dec;
            if (Tab == ReportTabType.Plan.ToString())
            {
                AllocatedTotal = model.Where(m => m.ActivityType == ActivityType.ActivityMain).Select(m => m.ChildMonthAllocated).SingleOrDefault();
                MainTotalAllocated = AllocatedTotal.Jan + AllocatedTotal.Feb + AllocatedTotal.Mar + AllocatedTotal.Apr + AllocatedTotal.May + AllocatedTotal.Jun + AllocatedTotal.Jul + AllocatedTotal.Aug + AllocatedTotal.Sep + AllocatedTotal.Oct + AllocatedTotal.Nov + AllocatedTotal.Dec;
            }
            else
            {
                AllocatedTotal = model.Where(m => m.ActivityType == ActivityType.ActivityMain).Select(m => m.MonthPlanned).SingleOrDefault();
                MainTotalAllocated = AllocatedTotal.Jan + AllocatedTotal.Feb + AllocatedTotal.Mar + AllocatedTotal.Apr + AllocatedTotal.May + AllocatedTotal.Jun + AllocatedTotal.Jul + AllocatedTotal.Aug + AllocatedTotal.Sep + AllocatedTotal.Oct + AllocatedTotal.Nov + AllocatedTotal.Dec;
            }

            PercAllocated.Jan = (AllocatedTotal.Jan == 0 && ActualTotal.Jan == 0) ? 0 : (AllocatedTotal.Jan == 0 && ActualTotal.Jan > 0) ? 101 : ActualTotal.Jan / AllocatedTotal.Jan * 100;
            PercAllocated.Feb = (AllocatedTotal.Feb == 0 && ActualTotal.Feb == 0) ? 0 : (AllocatedTotal.Feb == 0 && ActualTotal.Feb > 0) ? 101 : ActualTotal.Feb / AllocatedTotal.Feb * 100;
            PercAllocated.Mar = (AllocatedTotal.Mar == 0 && ActualTotal.Mar == 0) ? 0 : (AllocatedTotal.Mar == 0 && ActualTotal.Mar > 0) ? 101 : ActualTotal.Mar / AllocatedTotal.Mar * 100;
            PercAllocated.Apr = (AllocatedTotal.Apr == 0 && ActualTotal.Apr == 0) ? 0 : (AllocatedTotal.Apr == 0 && ActualTotal.Apr > 0) ? 101 : ActualTotal.Apr / AllocatedTotal.Apr * 100;
            PercAllocated.May = (AllocatedTotal.May == 0 && ActualTotal.May == 0) ? 0 : (AllocatedTotal.May == 0 && ActualTotal.May > 0) ? 101 : ActualTotal.May / AllocatedTotal.May * 100;
            PercAllocated.Jun = (AllocatedTotal.Jun == 0 && ActualTotal.Jun == 0) ? 0 : (AllocatedTotal.Jun == 0 && ActualTotal.Jun > 0) ? 101 : ActualTotal.Jun / AllocatedTotal.Jun * 100;
            PercAllocated.Jul = (AllocatedTotal.Jul == 0 && ActualTotal.Jul == 0) ? 0 : (AllocatedTotal.Jul == 0 && ActualTotal.Jul > 0) ? 101 : ActualTotal.Jul / AllocatedTotal.Jul * 100;
            PercAllocated.Aug = (AllocatedTotal.Aug == 0 && ActualTotal.Aug == 0) ? 0 : (AllocatedTotal.Aug == 0 && ActualTotal.Aug > 0) ? 101 : ActualTotal.Aug / AllocatedTotal.Aug * 100;
            PercAllocated.Sep = (AllocatedTotal.Sep == 0 && ActualTotal.Sep == 0) ? 0 : (AllocatedTotal.Sep == 0 && ActualTotal.Sep > 0) ? 101 : ActualTotal.Sep / AllocatedTotal.Sep * 100;
            PercAllocated.Oct = (AllocatedTotal.Oct == 0 && ActualTotal.Oct == 0) ? 0 : (AllocatedTotal.Oct == 0 && ActualTotal.Oct > 0) ? 101 : ActualTotal.Oct / AllocatedTotal.Oct * 100;
            PercAllocated.Nov = (AllocatedTotal.Nov == 0 && ActualTotal.Nov == 0) ? 0 : (AllocatedTotal.Nov == 0 && ActualTotal.Nov > 0) ? 101 : ActualTotal.Nov / AllocatedTotal.Nov * 100;
            PercAllocated.Dec = (AllocatedTotal.Dec == 0 && ActualTotal.Dec == 0) ? 0 : (AllocatedTotal.Dec == 0 && ActualTotal.Dec > 0) ? 101 : ActualTotal.Dec / AllocatedTotal.Dec * 100;

            ViewBag.PercAllocated = PercAllocated;
            ViewBag.MainTotalActual = MainTotalActual;
            ViewBag.MainTotalAllocated = MainTotalAllocated;

            DateTime CurrentDate = DateTime.Now;
            int currentQuarter = ((CurrentDate.Month - 1) / 3) + 1;
            string TodayDate = CurrentDate.ToString("MM/dd/yyyy");
            ViewBag.CurrentQuarter = "Q" + currentQuarter;
            ViewBag.TodayDate = TodayDate;
            ViewBag.DisplayYear = Year;
            ViewBag.SortingId = SortingId;
            if (SortingId != null && SortingId != string.Empty)
            {
                List<BudgetModelReport> SortingModel = new List<BudgetModelReport>();
                SortingModel = model.Where(m => m.ActivityType == ActivityType.ActivityPlan).ToList();
                SortingModel.ForEach(s => model.Remove(s));

                string[] splitsorting = SortingId.Split('_');
                string SortingColumn = splitsorting[0];
                string SortingUpDown = splitsorting[1];
                string SortingColumnMonth = splitsorting[2];
                if (SortingColumn == "Actual")
                {
                    if (SortingUpDown == "Up")
                    {
                        switch (SortingColumnMonth)
                        {
                            case "1":
                                SortingModel = SortingModel.OrderBy(s => s.MonthActual.Jan).ToList();
                                break;
                            case "2":
                                SortingModel = SortingModel.OrderBy(s => s.MonthActual.Feb).ToList();
                                break;
                            case "3":
                                SortingModel = SortingModel.OrderBy(s => s.MonthActual.Mar).ToList();
                                break;
                            case "4":
                                SortingModel = SortingModel.OrderBy(s => s.MonthActual.Apr).ToList();
                                break;
                            case "5":
                                SortingModel = SortingModel.OrderBy(s => s.MonthActual.May).ToList();
                                break;
                            case "6":
                                SortingModel = SortingModel.OrderBy(s => s.MonthActual.Jun).ToList();
                                break;
                            case "7":
                                SortingModel = SortingModel.OrderBy(s => s.MonthActual.Jul).ToList();
                                break;
                            case "8":
                                SortingModel = SortingModel.OrderBy(s => s.MonthActual.Aug).ToList();
                                break;
                            case "9":
                                SortingModel = SortingModel.OrderBy(s => s.MonthActual.Sep).ToList();
                                break;
                            case "10":
                                SortingModel = SortingModel.OrderBy(s => s.MonthActual.Oct).ToList();
                                break;
                            case "11":
                                SortingModel = SortingModel.OrderBy(s => s.MonthActual.Nov).ToList();
                                break;
                            case "12":
                                SortingModel = SortingModel.OrderBy(s => s.MonthActual.Dec).ToList();
                                break;
                            case "Total":
                                SortingModel = SortingModel.OrderBy(s => (
                                      s.MonthActual.Jan + s.MonthActual.Feb + s.MonthActual.Mar + s.MonthActual.Apr
                                    + s.MonthActual.May + s.MonthActual.Jun + s.MonthActual.Jul + s.MonthActual.Aug
                                    + s.MonthActual.Sep + s.MonthActual.Oct + s.MonthActual.Nov + s.MonthActual.Dec)).ToList();
                                break;
                        }
                    }
                    else
                    {
                        switch (SortingColumnMonth)
                        {
                            case "1":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthActual.Jan).ToList();
                                break;
                            case "2":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthActual.Feb).ToList();
                                break;
                            case "3":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthActual.Mar).ToList();
                                break;
                            case "4":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthActual.Apr).ToList();
                                break;
                            case "5":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthActual.May).ToList();
                                break;
                            case "6":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthActual.Jun).ToList();
                                break;
                            case "7":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthActual.Jul).ToList();
                                break;
                            case "8":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthActual.Aug).ToList();
                                break;
                            case "9":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthActual.Sep).ToList();
                                break;
                            case "10":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthActual.Oct).ToList();
                                break;
                            case "11":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthActual.Nov).ToList();
                                break;
                            case "12":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthActual.Dec).ToList();
                                break;
                            case "Total":
                                SortingModel = SortingModel.OrderByDescending(s => (
                                      s.MonthActual.Jan + s.MonthActual.Feb + s.MonthActual.Mar + s.MonthActual.Apr
                                    + s.MonthActual.May + s.MonthActual.Jun + s.MonthActual.Jul + s.MonthActual.Aug
                                    + s.MonthActual.Sep + s.MonthActual.Oct + s.MonthActual.Nov + s.MonthActual.Dec)).ToList();
                                break;
                        }
                    }
                }
                else if (SortingColumn == "Planned")
                {
                    if (SortingUpDown == "Up")
                    {
                        switch (SortingColumnMonth)
                        {
                            case "1":
                                SortingModel = SortingModel.OrderBy(s => s.MonthPlanned.Jan).ToList();
                                break;
                            case "2":
                                SortingModel = SortingModel.OrderBy(s => s.MonthPlanned.Feb).ToList();
                                break;
                            case "3":
                                SortingModel = SortingModel.OrderBy(s => s.MonthPlanned.Mar).ToList();
                                break;
                            case "4":
                                SortingModel = SortingModel.OrderBy(s => s.MonthPlanned.Apr).ToList();
                                break;
                            case "5":
                                SortingModel = SortingModel.OrderBy(s => s.MonthPlanned.May).ToList();
                                break;
                            case "6":
                                SortingModel = SortingModel.OrderBy(s => s.MonthPlanned.Jun).ToList();
                                break;
                            case "7":
                                SortingModel = SortingModel.OrderBy(s => s.MonthPlanned.Jul).ToList();
                                break;
                            case "8":
                                SortingModel = SortingModel.OrderBy(s => s.MonthPlanned.Aug).ToList();
                                break;
                            case "9":
                                SortingModel = SortingModel.OrderBy(s => s.MonthPlanned.Sep).ToList();
                                break;
                            case "10":
                                SortingModel = SortingModel.OrderBy(s => s.MonthPlanned.Oct).ToList();
                                break;
                            case "11":
                                SortingModel = SortingModel.OrderBy(s => s.MonthPlanned.Nov).ToList();
                                break;
                            case "12":
                                SortingModel = SortingModel.OrderBy(s => s.MonthPlanned.Dec).ToList();
                                break;
                            case "Total":
                                SortingModel = SortingModel.OrderBy(s => (
                                      s.MonthPlanned.Jan + s.MonthPlanned.Feb + s.MonthPlanned.Mar + s.MonthPlanned.Apr
                                    + s.MonthPlanned.May + s.MonthPlanned.Jun + s.MonthPlanned.Jul + s.MonthPlanned.Aug
                                    + s.MonthPlanned.Sep + s.MonthPlanned.Oct + s.MonthPlanned.Nov + s.MonthPlanned.Dec)).ToList();
                                break;
                        }
                    }
                    else
                    {
                        switch (SortingColumnMonth)
                        {
                            case "1":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthPlanned.Jan).ToList();
                                break;
                            case "2":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthPlanned.Feb).ToList();
                                break;
                            case "3":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthPlanned.Mar).ToList();
                                break;
                            case "4":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthPlanned.Apr).ToList();
                                break;
                            case "5":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthPlanned.May).ToList();
                                break;
                            case "6":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthPlanned.Jun).ToList();
                                break;
                            case "7":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthPlanned.Jul).ToList();
                                break;
                            case "8":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthPlanned.Aug).ToList();
                                break;
                            case "9":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthPlanned.Sep).ToList();
                                break;
                            case "10":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthPlanned.Oct).ToList();
                                break;
                            case "11":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthPlanned.Nov).ToList();
                                break;
                            case "12":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthPlanned.Dec).ToList();
                                break;
                            case "Total":
                                SortingModel = SortingModel.OrderByDescending(s => (
                                      s.MonthPlanned.Jan + s.MonthPlanned.Feb + s.MonthPlanned.Mar + s.MonthPlanned.Apr
                                    + s.MonthPlanned.May + s.MonthPlanned.Jun + s.MonthPlanned.Jul + s.MonthPlanned.Aug
                                    + s.MonthPlanned.Sep + s.MonthPlanned.Oct + s.MonthPlanned.Nov + s.MonthPlanned.Dec)).ToList();
                                break;
                        }
                    }
                }
                else if (SortingColumn == "Allocated")
                {
                    if (SortingUpDown == "Up")
                    {
                        switch (SortingColumnMonth)
                        {
                            case "1":
                                SortingModel = SortingModel.OrderBy(s => s.MonthAllocated.Jan).ToList();
                                break;
                            case "2":
                                SortingModel = SortingModel.OrderBy(s => s.MonthAllocated.Feb).ToList();
                                break;
                            case "3":
                                SortingModel = SortingModel.OrderBy(s => s.MonthAllocated.Mar).ToList();
                                break;
                            case "4":
                                SortingModel = SortingModel.OrderBy(s => s.MonthAllocated.Apr).ToList();
                                break;
                            case "5":
                                SortingModel = SortingModel.OrderBy(s => s.MonthAllocated.May).ToList();
                                break;
                            case "6":
                                SortingModel = SortingModel.OrderBy(s => s.MonthAllocated.Jun).ToList();
                                break;
                            case "7":
                                SortingModel = SortingModel.OrderBy(s => s.MonthAllocated.Jul).ToList();
                                break;
                            case "8":
                                SortingModel = SortingModel.OrderBy(s => s.MonthAllocated.Aug).ToList();
                                break;
                            case "9":
                                SortingModel = SortingModel.OrderBy(s => s.MonthAllocated.Sep).ToList();
                                break;
                            case "10":
                                SortingModel = SortingModel.OrderBy(s => s.MonthAllocated.Oct).ToList();
                                break;
                            case "11":
                                SortingModel = SortingModel.OrderBy(s => s.MonthAllocated.Nov).ToList();
                                break;
                            case "12":
                                SortingModel = SortingModel.OrderBy(s => s.MonthAllocated.Dec).ToList();
                                break;
                            case "Total":
                                SortingModel = SortingModel.OrderBy(s => (
                                      s.MonthAllocated.Jan + s.MonthAllocated.Feb + s.MonthAllocated.Mar + s.MonthAllocated.Apr
                                    + s.MonthAllocated.May + s.MonthAllocated.Jun + s.MonthAllocated.Jul + s.MonthAllocated.Aug
                                    + s.MonthAllocated.Sep + s.MonthAllocated.Oct + s.MonthAllocated.Nov + s.MonthAllocated.Dec)).ToList();
                                break;
                        }
                    }
                    else
                    {
                        switch (SortingColumnMonth)
                        {
                            case "1":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthAllocated.Jan).ToList();
                                break;
                            case "2":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthAllocated.Feb).ToList();
                                break;
                            case "3":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthAllocated.Mar).ToList();
                                break;
                            case "4":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthAllocated.Apr).ToList();
                                break;
                            case "5":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthAllocated.May).ToList();
                                break;
                            case "6":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthAllocated.Jun).ToList();
                                break;
                            case "7":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthAllocated.Jul).ToList();
                                break;
                            case "8":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthAllocated.Aug).ToList();
                                break;
                            case "9":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthAllocated.Sep).ToList();
                                break;
                            case "10":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthAllocated.Oct).ToList();
                                break;
                            case "11":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthAllocated.Nov).ToList();
                                break;
                            case "12":
                                SortingModel = SortingModel.OrderByDescending(s => s.MonthAllocated.Dec).ToList();
                                break;
                            case "Total":
                                SortingModel = SortingModel.OrderByDescending(s => (
                                      s.MonthAllocated.Jan + s.MonthAllocated.Feb + s.MonthAllocated.Mar + s.MonthAllocated.Apr
                                    + s.MonthAllocated.May + s.MonthAllocated.Jun + s.MonthAllocated.Jul + s.MonthAllocated.Aug
                                    + s.MonthAllocated.Sep + s.MonthAllocated.Oct + s.MonthAllocated.Nov + s.MonthAllocated.Dec)).ToList();
                                break;
                        }
                    }
                }
                SortingModel.ForEach(s => model.Add(s));

            }


            return PartialView("_Budget", model);
        }

        /// <summary>
        /// Get Empty Month List
        /// </summary>
        /// <returns></returns>
        private List<BudgetedValue> GetEmptyList()
        {
            List<BudgetedValue> emptyList = new List<BudgetedValue>();
            for (int i = 1; i < 13; i++)
            {
                BudgetedValue month = new BudgetedValue();
                month.Period = "Y" + i;
                month.Value = 0;
                emptyList.Add(month);
            }
            return emptyList;
        }

        /// <summary>
        /// set monthly data for the object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="lst"></param>
        /// <returns></returns>
        private BudgetModelReport GetMonthWiseDataReport(BudgetModelReport obj, List<BudgetedValue> lst, string columnType)
        {
            BudgetMonth month = new BudgetMonth();
            month.Jan = lst.Where(v => v.Period.ToUpper() == Jan).Select(v => v.Value).SingleOrDefault();
            month.Feb = lst.Where(v => v.Period.ToUpper() == Feb).Select(v => v.Value).SingleOrDefault();
            month.Mar = lst.Where(v => v.Period.ToUpper() == Mar).Select(v => v.Value).SingleOrDefault();
            month.Apr = lst.Where(v => v.Period.ToUpper() == Apr).Select(v => v.Value).SingleOrDefault();
            month.May = lst.Where(v => v.Period.ToUpper() == May).Select(v => v.Value).SingleOrDefault();
            month.Jun = lst.Where(v => v.Period.ToUpper() == Jun).Select(v => v.Value).SingleOrDefault();
            month.Jul = lst.Where(v => v.Period.ToUpper() == Jul).Select(v => v.Value).SingleOrDefault();
            month.Aug = lst.Where(v => v.Period.ToUpper() == Aug).Select(v => v.Value).SingleOrDefault();
            month.Sep = lst.Where(v => v.Period.ToUpper() == Sep).Select(v => v.Value).SingleOrDefault();
            month.Oct = lst.Where(v => v.Period.ToUpper() == Oct).Select(v => v.Value).SingleOrDefault();
            month.Nov = lst.Where(v => v.Period.ToUpper() == Nov).Select(v => v.Value).SingleOrDefault();
            month.Dec = lst.Where(v => v.Period.ToUpper() == Dec).Select(v => v.Value).SingleOrDefault();

            if (columnType.ToString() == ReportColumnType.Planned.ToString())
            {
                obj.MonthPlanned = month;
                obj.ParentMonthPlanned = month;
                obj.Planned = lst.Sum(v => v.Value);
            }
            else if (columnType.ToString() == ReportColumnType.Actual.ToString())
            {
                obj.MonthActual = month;
                obj.ParentMonthActual = month;
                obj.Actual = lst.Sum(v => v.Value);
            }
            else
            {
                obj.MonthAllocated = month;
                obj.ChildMonthAllocated = month;
                obj.Allocated = lst.Sum(v => v.Value);
            }

            return obj;
        }

        /// <summary>
        /// Calculate the bottom up planeed cost
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ParentActivityType"></param>
        /// <param name="ChildActivityType"></param>
        /// <returns></returns>
        public List<BudgetModelReport> CalculateBottomUpReport(List<BudgetModelReport> model, string ParentActivityType, string ChildActivityType)
        {
            if (ParentActivityType == ActivityType.ActivityTactic)
            {
                foreach (BudgetModelReport l in model.Where(l => l.ActivityType == ParentActivityType))
                {
                    List<BudgetModelReport> LineCheck = model.Where(lines => lines.ParentActivityId == l.ActivityId && lines.ActivityType == ActivityType.ActivityLineItem).ToList();
                    if (LineCheck.Count() > 0)
                    {
                        BudgetMonth parent = new BudgetMonth();
                        parent.Jan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Jan) ?? 0;
                        parent.Feb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Feb) ?? 0;
                        parent.Mar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Mar) ?? 0;
                        parent.Apr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Apr) ?? 0;
                        parent.May = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.May) ?? 0;
                        parent.Jun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Jun) ?? 0;
                        parent.Jul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Jul) ?? 0;
                        parent.Aug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Aug) ?? 0;
                        parent.Sep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Sep) ?? 0;
                        parent.Oct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Oct) ?? 0;
                        parent.Nov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Nov) ?? 0;
                        parent.Dec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Dec) ?? 0;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonthPlanned = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthPlanned;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthPlanned = parent;

                        BudgetMonth parentActual = new BudgetMonth();
                        parentActual.Jan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Jan) ?? 0;
                        parentActual.Feb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Feb) ?? 0;
                        parentActual.Mar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Mar) ?? 0;
                        parentActual.Apr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Apr) ?? 0;
                        parentActual.May = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.May) ?? 0;
                        parentActual.Jun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Jun) ?? 0;
                        parentActual.Jul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Jul) ?? 0;
                        parentActual.Aug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Aug) ?? 0;
                        parentActual.Sep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Sep) ?? 0;
                        parentActual.Oct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Oct) ?? 0;
                        parentActual.Nov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Nov) ?? 0;
                        parentActual.Dec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Dec) ?? 0;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonthActual = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthActual;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthActual = parentActual;
                    }
                    else
                    {
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonthActual = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthPlanned;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonthActual = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthActual;
                    }
                }
            }
            else
            {
                foreach (BudgetModelReport l in model.Where(l => l.ActivityType == ParentActivityType))
                {
                    BudgetMonth parent = new BudgetMonth();
                    parent.Jan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Jan) ?? 0;
                    parent.Feb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Feb) ?? 0;
                    parent.Mar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Mar) ?? 0;
                    parent.Apr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Apr) ?? 0;
                    parent.May = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.May) ?? 0;
                    parent.Jun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Jun) ?? 0;
                    parent.Jul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Jul) ?? 0;
                    parent.Aug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Aug) ?? 0;
                    parent.Sep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Sep) ?? 0;
                    parent.Oct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Oct) ?? 0;
                    parent.Nov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Nov) ?? 0;
                    parent.Dec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthPlanned.Dec) ?? 0;
                    model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonthPlanned = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthPlanned;
                    model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthPlanned = parent;
                    //l.ParentMonth = parent;

                    BudgetMonth parentActual = new BudgetMonth();
                    parentActual.Jan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Jan) ?? 0;
                    parentActual.Feb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Feb) ?? 0;
                    parentActual.Mar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Mar) ?? 0;
                    parentActual.Apr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Apr) ?? 0;
                    parentActual.May = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.May) ?? 0;
                    parentActual.Jun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Jun) ?? 0;
                    parentActual.Jul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Jul) ?? 0;
                    parentActual.Aug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Aug) ?? 0;
                    parentActual.Sep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Sep) ?? 0;
                    parentActual.Oct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Oct) ?? 0;
                    parentActual.Nov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Nov) ?? 0;
                    parentActual.Dec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthActual.Dec) ?? 0;
                    model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonthActual = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthActual;
                    model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthActual = parentActual;

                    BudgetMonth parentAllocated = new BudgetMonth();
                    parentAllocated.Jan = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Jan) ?? 0;
                    parentAllocated.Feb = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Feb) ?? 0;
                    parentAllocated.Mar = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Mar) ?? 0;
                    parentAllocated.Apr = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Apr) ?? 0;
                    parentAllocated.May = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.May) ?? 0;
                    parentAllocated.Jun = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Jun) ?? 0;
                    parentAllocated.Jul = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Jul) ?? 0;
                    parentAllocated.Aug = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Aug) ?? 0;
                    parentAllocated.Sep = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Sep) ?? 0;
                    parentAllocated.Oct = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Oct) ?? 0;
                    parentAllocated.Nov = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Nov) ?? 0;
                    parentAllocated.Dec = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Dec) ?? 0;
                    model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ChildMonthAllocated = parentAllocated;
                }
            }
            return model;
        }

        /// <summary>
        /// Manage lines items if cost is allocated to other
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<BudgetModelReport> ManageLineItems(List<BudgetModelReport> model)
        {
            foreach (BudgetModelReport l in model.Where(l => l.ActivityType == ActivityType.ActivityTactic))
            {
                BudgetMonth lineDiffPlanned = new BudgetMonth();
                List<BudgetModelReport> lines = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId).ToList();
                BudgetModelReport otherLine = lines.Where(ol => ol.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault();
                lines = lines.Where(ol => ol.ActivityName != Common.DefaultLineItemTitle).ToList();
                if (otherLine != null)
                {
                    if (lines.Count > 0)
                    {
                        lineDiffPlanned.Jan = l.MonthPlanned.Jan - lines.Sum(lmon => (double?)lmon.MonthPlanned.Jan) ?? 0;
                        lineDiffPlanned.Feb = l.MonthPlanned.Feb - lines.Sum(lmon => (double?)lmon.MonthPlanned.Feb) ?? 0;
                        lineDiffPlanned.Mar = l.MonthPlanned.Mar - lines.Sum(lmon => (double?)lmon.MonthPlanned.Mar) ?? 0;
                        lineDiffPlanned.Apr = l.MonthPlanned.Apr - lines.Sum(lmon => (double?)lmon.MonthPlanned.Apr) ?? 0;
                        lineDiffPlanned.May = l.MonthPlanned.May - lines.Sum(lmon => (double?)lmon.MonthPlanned.May) ?? 0;
                        lineDiffPlanned.Jun = l.MonthPlanned.Jun - lines.Sum(lmon => (double?)lmon.MonthPlanned.Jun) ?? 0;
                        lineDiffPlanned.Jul = l.MonthPlanned.Jul - lines.Sum(lmon => (double?)lmon.MonthPlanned.Jul) ?? 0;
                        lineDiffPlanned.Aug = l.MonthPlanned.Aug - lines.Sum(lmon => (double?)lmon.MonthPlanned.Aug) ?? 0;
                        lineDiffPlanned.Sep = l.MonthPlanned.Sep - lines.Sum(lmon => (double?)lmon.MonthPlanned.Sep) ?? 0;
                        lineDiffPlanned.Oct = l.MonthPlanned.Oct - lines.Sum(lmon => (double?)lmon.MonthPlanned.Oct) ?? 0;
                        lineDiffPlanned.Nov = l.MonthPlanned.Nov - lines.Sum(lmon => (double?)lmon.MonthPlanned.Nov) ?? 0;
                        lineDiffPlanned.Dec = l.MonthPlanned.Dec - lines.Sum(lmon => (double?)lmon.MonthPlanned.Dec) ?? 0;

                        lineDiffPlanned.Jan = lineDiffPlanned.Jan < 0 ? 0 : lineDiffPlanned.Jan;
                        lineDiffPlanned.Feb = lineDiffPlanned.Feb < 0 ? 0 : lineDiffPlanned.Feb;
                        lineDiffPlanned.Mar = lineDiffPlanned.Mar < 0 ? 0 : lineDiffPlanned.Mar;
                        lineDiffPlanned.Apr = lineDiffPlanned.Apr < 0 ? 0 : lineDiffPlanned.Apr;
                        lineDiffPlanned.May = lineDiffPlanned.May < 0 ? 0 : lineDiffPlanned.May;
                        lineDiffPlanned.Jun = lineDiffPlanned.Jun < 0 ? 0 : lineDiffPlanned.Jun;
                        lineDiffPlanned.Jul = lineDiffPlanned.Jul < 0 ? 0 : lineDiffPlanned.Jul;
                        lineDiffPlanned.Aug = lineDiffPlanned.Aug < 0 ? 0 : lineDiffPlanned.Aug;
                        lineDiffPlanned.Sep = lineDiffPlanned.Sep < 0 ? 0 : lineDiffPlanned.Sep;
                        lineDiffPlanned.Oct = lineDiffPlanned.Oct < 0 ? 0 : lineDiffPlanned.Oct;
                        lineDiffPlanned.Nov = lineDiffPlanned.Nov < 0 ? 0 : lineDiffPlanned.Nov;
                        lineDiffPlanned.Dec = lineDiffPlanned.Dec < 0 ? 0 : lineDiffPlanned.Dec;

                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().MonthPlanned = lineDiffPlanned;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().ParentMonthPlanned = lineDiffPlanned;

                        double planned = l.Planned - lines.Sum(l1 => l1.Planned);
                        planned = planned < 0 ? 0 : planned;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().Planned = planned;
                    }
                    else
                    {
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().MonthPlanned = l.MonthPlanned;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().ParentMonthPlanned = l.MonthPlanned;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().Planned = l.Planned < 0 ? 0 : l.Planned;
                    }
                }
            }
            //foreach (BudgetModel l in model.Where(l => l.ActivityType == ActivityLineItem && l.ActivityName == "Other"))
            //{
            //    l.Allocated = l.Month.Jan + l.Month.Feb + l.Month.Mar + l.Month.Apr + l.Month.May + l.Month.Jun + l.Month.Jul + l.Month.Aug + l.Month.Sep + l.Month.Oct + l.Month.Nov + l.Month.Dec;
            //}
            return model;
        }

        #endregion

        /// <summary>
        /// Added by Mitesh Vaishnav for PL ticket #846
        /// set session for multiple selected plans and business units
        /// </summary>
        /// <param name="businessUnitIds">Comma separated string which contains business unit's Ids</param>
        /// <param name="planIds">Comma separated string which contains plan's Ids</param>
        /// <returns>If success than return status 1 else 0</returns>
        public JsonResult SetReportData(string businessUnitIds, string planIds)
        {
            try
            {
                List<Guid> lstBusinessUnitIds = new List<Guid>();
                List<int> lstPlanIds = new List<int>();
                if (businessUnitIds != string.Empty)
                {
                    string[] arrBusinessUnitIds = businessUnitIds.Split(',');
                    foreach (string bu in arrBusinessUnitIds)
                    {
                        Guid BusinessUnitId;
                        if (Guid.TryParse(bu, out BusinessUnitId))
                        {
                            lstBusinessUnitIds.Add(BusinessUnitId);
                        }
                    }
                    if (lstBusinessUnitIds.Count > 0)
                    {
                        Sessions.ReportBusinessUnitIds = lstBusinessUnitIds;
                    }

                }
                else
                {
                    Sessions.ReportBusinessUnitIds = lstBusinessUnitIds;
                }
                if (planIds != string.Empty)
                {
                    string[] arrPlanIds = planIds.Split(',');
                    foreach (string pId in arrPlanIds)
                    {
                        int planId;
                        if (int.TryParse(pId, out planId))
                        {
                            lstPlanIds.Add(planId);
                        }
                    }
                    if (lstPlanIds.Count > 0)
                    {
                        Sessions.ReportPlanIds = lstPlanIds;
                    }
                }
                else
                {
                    Sessions.ReportPlanIds = lstPlanIds;
                }
                return Json(new { status = true });
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                return Json(new { status = false });
            }
        }
    }
}

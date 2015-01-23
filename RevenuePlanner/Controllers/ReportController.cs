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

        #endregion

        #region Report Index

        /// <summary>
        /// Report Index : This action will return the Report index page
        /// </summary>
        /// <param name="activeMenu"></param>
        /// <returns>Return Index View</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ReportView)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #519 to implement user permission Logic
        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Report)
        {
            // Added by Sohel Pathan on 27/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.TacticActualsAddEdit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);
            ViewBag.ActiveMenu = activeMenu;

            ////Start Added by Mitesh Vaishnav for PL ticket #846
            Guid reportBusinessUnitId = Guid.Empty;
            Sessions.ReportPlanIds = new List<int>();
            Sessions.ReportBusinessUnitIds = new List<Guid>();

            //// Set Report BusinessUnitId in Session.
            if (Sessions.PlanId > 0)
            {
                Sessions.ReportPlanIds.Add(Sessions.PlanId);
                //reportBusinessUnitId = db.Plans.Where(plan => plan.PlanId == Sessions.PlanId).Select(plan => plan.Model.BusinessUnitId).FirstOrDefault();
                //Sessions.ReportBusinessUnitIds.Add(reportBusinessUnitId);
            }

            //// List of Tab - Parent
            List<ViewByModel> lstViewByTab = new List<ViewByModel>();
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Plan.ToString(), Value = ReportTabType.Plan.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Vertical.ToString(), Value = ReportTabType.Vertical.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Geography.ToString(), Value = ReportTabType.Geography.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.BusinessUnit.ToString(), Value = ReportTabType.BusinessUnit.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = Common.CustomLabelFor(Enums.CustomLabelCode.Audience), Value = ReportTabType.Audience.ToString() });
            lstViewByTab = lstViewByTab.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewByTab = lstViewByTab;

            //// List of Allocated Value
            List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
            lstViewByAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarterly", Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstViewByAllocated = lstViewByAllocated.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewByAllocated = lstViewByAllocated;

            //// Geography Value
            List<SelectListItem> lstGeography = new List<SelectListItem>();
            lstGeography = db.Geographies.Where(geography => geography.ClientId == Sessions.User.ClientId && geography.IsDeleted == false).ToList().Select(geography => new SelectListItem { Text = geography.Title, Value = geography.GeographyId.ToString(), Selected = true }).ToList();
            ViewBag.ViewGeography = lstGeography.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();

            //// Businessunit Value
            List<SelectListItem> lstBusinessUnit = new List<SelectListItem>();
            lstBusinessUnit = db.BusinessUnits.Where(businessunit => businessunit.ClientId == Sessions.User.ClientId && businessunit.IsDeleted == false).ToList().Select(businessunit => new SelectListItem { Text = businessunit.Title, Value = businessunit.BusinessUnitId.ToString(), Selected = (businessunit.BusinessUnitId == reportBusinessUnitId ? true : false) }).ToList();
            ViewBag.ViewBusinessUnit = lstBusinessUnit.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();

            //// Vertical value
            List<SelectListItem> lstVertical = new List<SelectListItem>();
            lstVertical = db.Verticals.Where(vertical => vertical.ClientId == Sessions.User.ClientId && vertical.IsDeleted == false).ToList().Select(vertical => new SelectListItem { Text = vertical.Title, Value = vertical.VerticalId.ToString(), Selected = true }).ToList();
            ViewBag.ViewVertical = lstVertical.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();

            //// Audience Value
            List<SelectListItem> lstAudience = new List<SelectListItem>();
            lstAudience = db.Audiences.Where(audience => audience.ClientId == Sessions.User.ClientId && audience.IsDeleted == false).ToList().Select(audience => new SelectListItem { Text = audience.Title, Value = audience.AudienceId.ToString(), Selected = true }).ToList();
            ViewBag.ViewAudience = lstAudience.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();

            //// Get Plan List
            List<SelectListItem> lstYear = new List<SelectListItem>();
            var lstPlan = db.Plans.Where(plan => plan.IsDeleted == false && plan.Status == PublishedPlan && plan.Model.ClientId == Sessions.User.ClientId && plan.Model.IsDeleted == false && plan.IsActive == true).ToList();
            if (lstPlan.Count == 0)
            {
                TempData["ErrorMessage"] = Common.objCached.NoPublishPlanAvailableOnReport;
                return RedirectToAction("PlanSelector", "Plan");
            }
            var yearlist = lstPlan.OrderBy(plan => plan.Year).Select(plan => plan.Year).Distinct().ToList();
            yearlist.ForEach(year => lstYear.Add(new SelectListItem { Text = "FY " + year, Value = year, Selected = year == currentYear ? true : false }));
            SelectListItem thisQuarter = new SelectListItem { Text = "this quarter", Value = "thisquarter" };
            lstYear.Add(thisQuarter);

            string defaultallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString();
            string Noneallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString();

            List<SelectListItem> lstPlanList = new List<SelectListItem>();

            lstPlanList = lstPlan.Where(plan => plan.Year == currentYear).Select(plan => new SelectListItem { Text = plan.Title + " - " + (plan.AllocatedBy == defaultallocatedby ? Noneallocatedby : plan.AllocatedBy), Value = plan.PlanId.ToString() + "_" + plan.AllocatedBy, Selected = (plan.PlanId == Sessions.PlanId ? true : false) }).ToList();
            ViewBag.ViewPlan = lstPlanList.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewYear = lstYear.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            ViewBag.SelectedYear = currentYear;
            //End Added by Mitesh Vaishnav for PL ticket #846

            return View("Index");
        }

        #endregion

        #region Summary Report

        /// <summary>
        /// This action will return the Partial View for Summary data
        /// </summary>
        /// <returns>Return Partial View _Summary</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ReportView)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #519 to implement user permission Logic
        public ActionResult GetSummaryData()
        {
            SummaryReportModel objSummaryReportModel = new SummaryReportModel();
            //// Getting current year's all published plan for all business unit of clientid of director.

            //// get tactic list
            List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> Tacticdata = Common.GetTacticStageRelation(tacticlist);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            TempData["ReportData"] = Tacticdata;

            //// Data shows up to current year
            Tacticdata = Tacticdata.Where(tactic => Convert.ToInt32(tactic.TacticYear) <= DateTime.Now.Year).ToList();
            List<string> yearList = Tacticdata.Select(tactic => tactic.TacticYear).Distinct().ToList();

            //// Get list of month up to current month based on year list
            List<string> includeMonth = GetMonthWithYearUptoCurrentMonth(yearList);

            //// Declare variables
            double overAllMQLProjected = 0;
            double overAllMQLActual = 0;
            double overAllRevenueActual = 0;
            double overAllRevenueProjected = 0;
            double overAllInqActual = 0;
            double overAllInqProjected = 0;
            double overAllCWActual = 0;
            double overAllCWProjected = 0;

            //// Check BusinessUnitids selected or not
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count > 0)
            {
                //// check planids selected or not
                if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count > 0)
                {
                    //// set viewbag to display plan or msg
                    ViewBag.IsPlanExistToShowReport = true;
                }
            }

            //// Get Tactic List for actual value
            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(tactic => tactic.ActualTacticList.ForEach(actualtactic => ActualTacticList.Add(actualtactic)));

            //// get MQL Actual value
            string mqlstage = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            var MQLActuallist = ActualTacticList.Where(actualtactic => actualtactic.StageTitle.Equals(mqlstage) && includeMonth.Contains(Tacticdata.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == actualtactic.PlanTacticId).TacticYear + actualtactic.Period)).ToList();
            if (MQLActuallist.Count > 0)
            {
                overAllMQLActual = MQLActuallist.Sum(actualtactic => actualtactic.Actualvalue);
            }

            //// get MQL projected value
            var MQLProjectedlist = GetProjectedMQLValueDataTableForReport(Tacticdata).Where(tactictable => includeMonth.Contains(tactictable.Month)).ToList();
            if (MQLProjectedlist.Count > 0)
            {
                overAllMQLProjected = MQLProjectedlist.Sum(tactictable => tactictable.Value);
            }

            //// get Revenue Actual value
            string revenuestage = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            var RevenueActualllist = ActualTacticList.Where(actualtactic => actualtactic.StageTitle.Equals(revenuestage) && includeMonth.Contains(Tacticdata.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == actualtactic.PlanTacticId).TacticYear + actualtactic.Period)).ToList();
            if (RevenueActualllist.Count > 0)
            {
                overAllRevenueActual = RevenueActualllist.Sum(tactic => tactic.Actualvalue);
            }

            //// get Revenue Projeted Value
            var RevenueProjectedlist = GetProjectedRevenueValueDataTableForReport(Tacticdata).Where(tactictable => includeMonth.Contains(tactictable.Month)).ToList();
            if (RevenueProjectedlist.Count > 0)
            {
                overAllRevenueProjected = RevenueProjectedlist.Sum(tactictable => tactictable.Value);
            }

            //// Round Revenue value
            overAllRevenueProjected = Math.Round(overAllRevenueProjected);
            overAllRevenueActual = Math.Round(overAllRevenueActual);

            //// Assign it to objSummaryReportModel - MQL
            objSummaryReportModel.MQLs = overAllMQLActual;
            objSummaryReportModel.MQLsPercentage = GetPercentageDifference(overAllMQLActual, overAllMQLProjected);

            //// Assign it to objSummaryReportModel - Revenue
            objSummaryReportModel.Revenue = Convert.ToString(overAllRevenueActual);
            objSummaryReportModel.RevenuePercentage = GetPercentageDifference(overAllRevenueActual, overAllRevenueProjected);

            //// Declare Text for display in view
            string abovePlan = "above plan";
            string belowPlan = "below plan";
            string at_par = "equal to";

            //// For Revenue Summary
            objSummaryReportModel.PlanStatus = (overAllRevenueActual < overAllRevenueProjected) ? belowPlan : (overAllRevenueActual > overAllRevenueProjected) ? abovePlan : at_par;

            //// Assign it to objSummaryReportModel - Projected Revenue
            objSummaryReportModel.ProjectedRevenue = overAllRevenueProjected;

            #region INQ
            string inq = Enums.Stage.INQ.ToString();
            int INQStageId = db.Stages.FirstOrDefault(stage => stage.ClientId == Sessions.User.ClientId && stage.Code == inq && stage.IsDeleted == false).StageId;

            //// Get INQ Actual Value
            var INQActualList = GetActualValueForINQ(ActualTacticList, INQStageId).Where(tacticactual => includeMonth.Contains(Tacticdata.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == tacticactual.PlanTacticId).TacticYear + tacticactual.Period)).ToList();
            if (INQActualList.Count > 0)
            {
                overAllInqActual = INQActualList.Sum(tacticactual => tacticactual.Actualvalue);
            }

            //// Get INQ Projected Value
            var InqProjectedList = GetConversionProjectedINQData(Tacticdata).Where(tactictable => includeMonth.Contains(tactictable.Month)).ToList();
            if (InqProjectedList.Count > 0)
            {
                overAllInqProjected = InqProjectedList.Sum(tactictable => tactictable.Value);
            }

            //// Get Percentage difference for inq
            double inqPercentageDifference = WaterfallGetPercentageDifference(overAllMQLActual, overAllMQLProjected, overAllInqActual, overAllInqProjected);
            objSummaryReportModel.INQPerValue = inqPercentageDifference;
            if (inqPercentageDifference < 0)
            {
                objSummaryReportModel.ISQsStatus = belowPlan;
            }
            else
            {
                objSummaryReportModel.ISQsStatus = abovePlan;
            }

            #endregion

            #region MQL

            //// Get CW actual Value
            string cwSatge = Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString();
            var CWActualList = ActualTacticList.Where(tacticactual => tacticactual.StageTitle.Equals(cwSatge) && includeMonth.Contains(Tacticdata.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == tacticactual.PlanTacticId).TacticYear + tacticactual.Period)).ToList();
            if (CWActualList.Count > 0)
            {
                overAllCWActual = CWActualList.Sum(tacticactual => tacticactual.Actualvalue);
            }

            //// Get CW Projected Value
            var CwProjectedList = GetProjectedCWValueDataTableForReport(Tacticdata).Where(tactictable => includeMonth.Contains(tactictable.Month)).ToList();
            if (CwProjectedList.Count > 0)
            {
                overAllCWProjected = CwProjectedList.Sum(tactictable => tactictable.Value);
            }

            //// Assign it to objSummaryReportModel - MQl Percentage
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
        /// Function to get Percentage difference.
        /// </summary>
        /// <param name="firstActualValue">Overall First Actual value like MQL Actual.</param>
        /// <param name="firstProjectedValue">Overall First Projected Value like MQl Projected.</param>
        /// <param name="secondActualValue">Overall Second Actual Value like INQ Actual.</param>
        /// <param name="secondProjectedValue">Overall Second Projected Value like INQ projected.</param>
        /// <returns>Returns Percentage.</returns>
        private double WaterfallGetPercentageDifference(double firstActualValue, double firstProjectedValue, double secondActualValue, double secondProjectedValue)
        {
            double percentage = 0;
            if (secondProjectedValue != 0 && secondActualValue != 0 && firstActualValue != 0 && firstProjectedValue != 0)
            {
                percentage = ((firstActualValue / secondActualValue) / (firstProjectedValue / secondProjectedValue)) * 100;
            }
            return percentage;
        }

        #endregion

        #region Report General

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

                //// Create BusinessUnitIds list from comma separated string of businessUnitIds and assign to Session variable.
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

                //// Create PlanIds list from comma separated string of planIds and assign to Session variable.
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
                        if (lstPlanIds.Count == 1)
                        {
                            Sessions.PlanId = lstPlanIds.FirstOrDefault();
                        }
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
        /// Class for Tactic Datatable to divide value month wise.
        /// </summary>
        public class TacticMonthValue
        {
            public int Id { get; set; }
            public string Month { get; set; }
            public double Value { get; set; }
        }

        /// <summary>
        /// Get List of object divide value month wise based on start & end date.
        /// </summary>
        /// <param name="tacticdata">tacticdata.</param>
        /// <returns></returns>
        private List<TacticMonthValue> GetMonthWiseValueList(List<TacticDataTable> tacticdata)
        {
            List<TacticMonthValue> listTacticMonthValue = new List<TacticMonthValue>();
            foreach (var tactic in tacticdata)
            {
                if (tactic.StartYear == tactic.EndYear)
                {
                    if (tactic.StartMonth == tactic.EndMonth)
                    {
                        listTacticMonthValue.Add(new TacticMonthValue { Id = tactic.TacticId, Month = tactic.StartYear + PeriodPrefix + tactic.StartMonth, Value = tactic.Value });
                    }
                    else
                    {
                        int totalMonth = (tactic.EndMonth - tactic.StartMonth) + 1;
                        double totalValue = (double)tactic.Value / (double)totalMonth;
                        for (var i = tactic.StartMonth; i <= tactic.EndMonth; i++)
                        {
                            listTacticMonthValue.Add(new TacticMonthValue { Id = tactic.TacticId, Month = tactic.StartYear.ToString() + PeriodPrefix + i, Value = totalValue });
                        }
                    }
                }
                else
                {
                    int totalMonth = (12 - tactic.StartMonth) + tactic.EndMonth + 1;
                    double totalValue = (double)tactic.Value / (double)totalMonth;
                    for (var i = tactic.StartMonth; i <= 12; i++)
                    {
                        listTacticMonthValue.Add(new TacticMonthValue { Id = tactic.TacticId, Month = tactic.StartYear.ToString() + PeriodPrefix + i, Value = totalValue });
                    }
                    for (var i = 1; i <= tactic.EndMonth + 1; i++)
                    {
                        listTacticMonthValue.Add(new TacticMonthValue { Id = tactic.TacticId, Month = tactic.EndYear.ToString() + PeriodPrefix + i, Value = totalValue });
                    }
                }
            }
            return listTacticMonthValue;
        }

        /// <summary>
        /// Function to get tactic for report.
        /// Tactic based on ReportPlanids
        /// Added By Bhavesh.
        /// </summary>
        /// <returns>Returns list of Tactic Id.</returns>
        private List<Plan_Campaign_Program_Tactic> GetTacticForReporting()
        {
            //// Getting current year's all published plan for all business unit of clientid of director.
            List<Plan_Campaign_Program_Tactic> tacticList = new List<Plan_Campaign_Program_Tactic>();
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count > 0)
            {
                List<int> planIds = new List<int>();
                if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count > 0)
                {
                    planIds = Sessions.ReportPlanIds;
                }
                //// Get Tactic list.
                List<string> tacticStatus = Common.GetStatusListAfterApproved();
                tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted == false &&
                                                                  tacticStatus.Contains(tactic.Status) &&
                                                                  planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
                                                                  Sessions.ReportBusinessUnitIds.Contains(tactic.BusinessUnitId)).ToList();
            }
            return tacticList;
        }

        /// <summary>
        /// Get Upto Current Month List With Up to current year.
        /// </summary>
        /// <param name="yearList"></param>
        /// <returns>list.</returns>
        private List<string> GetMonthWithYearUptoCurrentMonth(List<string> yearList)
        {
            List<string> includeMonth = new List<string>();
            int startMonth = 1, EndMonth = currentMonth;
            foreach (string year in yearList)
            {
                //// if year is current year.
                if (year == currentYear)
                {
                    //// Add values to list till current month.
                    for (int i = startMonth; i <= EndMonth; i++)
                    {
                        includeMonth.Add(year + PeriodPrefix + i);
                    }
                }
                else
                {
                    //// Add all months values to list.
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
        public List<TacticMonthValue> GetProjectedRevenueValueDataTableForReport(List<TacticStageValue> planTacticList)
        {
            //// Get tactic Revenue data from planTacticlist.
            List<TacticDataTable> tacticdata = (from tactic in planTacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.TacticObj.PlanTacticId,
                                                    Value = tactic.RevenueValue,
                                                    StartMonth = tactic.TacticObj.StartDate.Month,
                                                    EndMonth = tactic.TacticObj.EndDate.Month,
                                                    StartYear = tactic.TacticObj.StartDate.Year,
                                                    EndYear = tactic.TacticObj.EndDate.Year
                                                }).ToList();

            return GetMonthWiseValueList(tacticdata);
        }

        /// <summary>
        /// Get Projected MQL Data & Calculation.
        /// </summary>
        /// <param name="planTacticList"></param>
        /// <returns></returns>
        public List<TacticMonthValue> GetProjectedMQLValueDataTableForReport(List<TacticStageValue> planTacticList)
        {
            //// Get tactic MQL data from planTacticlist.
            List<TacticDataTable> tacticdata = (from tactic in planTacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.TacticObj.PlanTacticId,
                                                    Value = tactic.MQLValue,
                                                    StartMonth = tactic.TacticObj.StartDate.Month,
                                                    EndMonth = tactic.TacticObj.EndDate.Month,
                                                    StartYear = tactic.TacticObj.StartDate.Year,
                                                    EndYear = tactic.TacticObj.EndDate.Year
                                                }).ToList();

            return GetMonthWiseValueList(tacticdata);
        }

        /// <summary>
        /// Get Projected CW Data & Calculation.
        /// </summary>
        /// <param name="planTacticList"></param>
        /// <returns></returns>
        public List<TacticMonthValue> GetProjectedCWValueDataTableForReport(List<TacticStageValue> planTacticList)
        {
            //// Get tactic CW data from planTacticlist.
            List<TacticDataTable> tacticdata = (from tactic in planTacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.TacticObj.PlanTacticId,
                                                    Value = tactic.CWValue,
                                                    StartMonth = tactic.TacticObj.StartDate.Month,
                                                    EndMonth = tactic.TacticObj.EndDate.Month,
                                                    StartYear = tactic.TacticObj.StartDate.Year,
                                                    EndYear = tactic.TacticObj.EndDate.Year
                                                }).ToList();

            return GetMonthWiseValueList(tacticdata);
        }

        /// <summary>
        /// Get Projected INQ Data With Month Wise.
        /// Modified By: Maninde Singh Wadhva for #426 	Conversion Reporting Page is slow to render.
        /// </summary>
        /// <param name="TacticList"></param>
        /// <returns></returns>
        public List<TacticMonthValue> GetConversionProjectedINQData(List<TacticStageValue> planTacticList)
        {
            //// Get tactic INQ data from planTacticlist.
            List<TacticDataTable> tacticdata = (from tactic in planTacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.TacticObj.PlanTacticId,
                                                    Value = tactic.INQValue,
                                                    StartMonth = tactic.TacticObj.StartDate.Month,
                                                    EndMonth = tactic.TacticObj.EndDate.Month,
                                                    StartYear = tactic.TacticObj.StartDate.Year,
                                                    EndYear = tactic.TacticObj.EndDate.Year
                                                }).ToList();

            return GetMonthWiseValueList(tacticdata);
        }

        /// <summary>
        /// Get Projected INQ Data With Month Wise.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public List<TacticMonthValue> GetProjectedINQDataWithVelocity(List<TacticStageValue> planTacticList)
        {
            //// Get tactic INQ with Velocity data from planTacticlist.
            List<TacticDataTable> tacticdata = (from tactic in planTacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.TacticObj.PlanTacticId,
                                                    Value = tactic.INQValue,
                                                    StartMonth = tactic.TacticObj.StartDate.AddDays(tactic.INQVelocity).Month,
                                                    EndMonth = tactic.TacticObj.EndDate.AddDays(tactic.INQVelocity).Month,
                                                    StartYear = tactic.TacticObj.StartDate.AddDays(tactic.INQVelocity).Year,
                                                    EndYear = tactic.TacticObj.EndDate.AddDays(tactic.INQVelocity).Year
                                                }).ToList();

            return GetMonthWiseValueList(tacticdata);
        }

        /// <summary>
        /// Get Projected MQL Data With Month Wise.
        /// Modified By: Maninder Singh Wadhva to address TFS Bug#280 :Error Message Showing when editing a tactic - Preventing MQLs from updating
        /// Modified By: Maninder Singh Wadhva 1-March-2014 to address TFS Bug#322 : Changes made to INQ, MQL and Projected Revenue Calculation.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public List<TacticMonthValue> GetProjectedMQLDataWithVelocity(List<TacticStageValue> planTacticList)
        {
            //// Get tactic MQL with Velocity data from planTacticlist.
            List<TacticDataTable> tacticdata = (from tactic in planTacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.TacticObj.PlanTacticId,
                                                    Value = tactic.MQLValue,
                                                    StartMonth = tactic.TacticObj.StartDate.AddDays(tactic.MQLVelocity).Month,
                                                    EndMonth = tactic.TacticObj.EndDate.AddDays(tactic.MQLVelocity).Month,
                                                    StartYear = tactic.TacticObj.StartDate.AddDays(tactic.MQLVelocity).Year,
                                                    EndYear = tactic.TacticObj.EndDate.AddDays(tactic.MQLVelocity).Year
                                                }).ToList();

            return GetMonthWiseValueList(tacticdata);
        }

        /// <summary>
        /// Function to get percentage.
        /// </summary>
        /// <param name="actual">Actual Value.</param>
        /// <param name="projected">Projected Value.</param>
        /// <returns>Returns percentage.</returns>
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
                includeYearList.Add(currentYear);
                int currentQuarter = ((currentMonth - 1) / 3) + 1;
                if (currentQuarter == 1 && !isQuarterOnly)
                {
                    includeYearList.Add(DateTime.Now.AddYears(-1).Year.ToString());
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
                int currentQuarter = ((currentMonth - 1) / 3) + 1;

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
                    includeMonth.Add(currentYear + PeriodPrefix + i);
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
                int currentQuarter = ((currentMonth - 1) / 3) + 1;
                
                //// Monthly Title list based on Quarter.
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
        /// <param name="selectOption">selection option</param>
        /// <param name="isQuarterOnly">Is Quarter only</param>
        /// </summary>
        /// <returns>list.</returns>
        private List<string> GetUpToCurrentMonthWithYearForReport(string selectOption, bool isQuarterOnly = false)
        {
            List<string> includeMonth = new List<string>();
            int startMonth = 1, EndMonth = currentMonth;
            if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
            {
                int currentQuarter = ((currentMonth - 1) / 3) + 1;

                if (currentQuarter == 1 && !isQuarterOnly)
                {
                    for (int i = 10; i <= 12; i++)
                    {
                        includeMonth.Add(DateTime.Now.AddYears(-1).Year.ToString() + PeriodPrefix + i);
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
                    includeMonth.Add(currentYear + PeriodPrefix + i);
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
            return Json(GetChildLabelDataViewByModel(ParentLabel, selectOption), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Child List in View By Model
        /// </summary>
        /// <param name="ParentLabel"></param>
        /// <param name="selectOption"></param>
        /// <returns></returns>
        public List<ViewByModel> GetChildLabelDataViewByModel(string ParentLabel, string selectOption = "")
        {
            List<ViewByModel> returnData = new List<ViewByModel>();
            if (ParentLabel == Common.RevenueBusinessUnit)
            {
                returnData = (db.BusinessUnits.Where(businessunit => businessunit.ClientId.Equals(Sessions.User.ClientId) && businessunit.IsDeleted.Equals(false) && (Sessions.ReportBusinessUnitIds.Contains(businessunit.BusinessUnitId))).ToList().Select(businessunit => new ViewByModel
                {
                    Value = businessunit.BusinessUnitId.ToString(),
                    Text = businessunit.Title
                }).Select(businessunit => businessunit).Distinct().OrderBy(businessunit => businessunit.Text)).ToList();
                returnData = returnData.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            }
            else if (ParentLabel == Common.RevenueAudience)
            {
                returnData = (db.Audiences.Where(audience => audience.ClientId == Sessions.User.ClientId && audience.IsDeleted == false).ToList().Select(audience => new ViewByModel
                {
                    Value = audience.AudienceId.ToString(),
                    Text = audience.Title
                }).Select(audience => audience).Distinct().OrderBy(audience => audience.Text)).ToList();
                returnData = returnData.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            }
            else if (ParentLabel == Common.RevenueGeography)
            {
                returnData = (db.Geographies.Where(geography => geography.ClientId.Equals(Sessions.User.ClientId) && geography.IsDeleted.Equals(false)).ToList().Select(geography => new ViewByModel
                {
                    Value = geography.GeographyId.ToString(),
                    Text = geography.Title
                }).Select(geography => geography).Distinct().OrderBy(geography => geography.Text)).ToList();
                returnData = returnData.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            }
            else if (ParentLabel == Common.RevenueVertical)
            {
                returnData = (db.Verticals.Where(vertical => vertical.ClientId == Sessions.User.ClientId && vertical.IsDeleted == false).ToList().Select(vertical => new ViewByModel
                {
                    Value = vertical.VerticalId.ToString(),
                    Text = vertical.Title
                }).Select(vertical => vertical).Distinct().OrderBy(vertical => vertical.Text)).ToList();
                returnData = returnData.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            }
            else if (ParentLabel == Common.RevenuePlans)
            {
                string year = selectOption;
                var plans = db.Plans.Where(plan => plan.Model.ClientId.Equals(Sessions.User.ClientId) && plan.IsDeleted.Equals(false) && plan.Status == PublishedPlan && Sessions.ReportPlanIds.Contains(plan.PlanId)).ToList();
                if (selectOption == Enums.UpcomingActivities.thisquarter.ToString())
                {
                    year = currentYear;
                }

                returnData = plans.Where(plan => plan.Year == year).Select(plan => new ViewByModel
                {
                    Value = plan.PlanId.ToString(),
                    Text = plan.Title
                }).Select(plan => plan).Distinct().OrderBy(plan => plan.Text).ToList();
                returnData = returnData.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            }
            else if (ParentLabel.Contains(Common.TacticCustomTitle) || ParentLabel.Contains(Common.CampaignCustomTitle) || ParentLabel.Contains(Common.ProgramCustomTitle))
            {
                int customfieldId = 0;
                bool IsCampaign = false;
                bool IsProgram = false;
                bool IsTactic = false;

                //// From which custom field called
                if (ParentLabel.Contains(Common.TacticCustomTitle))
                {
                    customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.TacticCustomTitle, ""));
                    IsTactic = true;
                }
                else if (ParentLabel.Contains(Common.CampaignCustomTitle))
                {
                    customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.CampaignCustomTitle, ""));
                    IsCampaign = true;
                }
                else if (ParentLabel.Contains(Common.ProgramCustomTitle))
                {
                    customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.ProgramCustomTitle, ""));
                    IsProgram = true;
                }

                if (customfieldId > 0)
                {
                    //// Get Custom field type
                    string customFieldType = db.CustomFields.Where(customfield => customfield.CustomFieldId == customfieldId).Select(customfield => customfield.CustomFieldType.Name).FirstOrDefault();
                    //// check its dropdown or textbox
                    if (customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                    {
                        //// get option list for dropdown
                        var optionlist = db.CustomFieldOptions.Where(customfieldoption => customfieldoption.CustomFieldId == customfieldId).ToList();
                        returnData = optionlist.Select(option => new ViewByModel
                        {
                            Value = option.CustomFieldOptionId.ToString(),
                            Text = option.Value
                        }).Select(option => option).Distinct().OrderBy(option => option.Text).ToList();
                        returnData = returnData.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
                    }
                    else if (customFieldType == Enums.CustomFieldType.TextBox.ToString())
                    {
                        List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting();
                        List<int> entityids = new List<int>();
                        //// get entity id based on custom field entity
                        if (IsCampaign)
                        {
                            entityids = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
                        }
                        else if (IsProgram)
                        {
                            entityids = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();
                        }
                        else if (IsTactic)
                        {
                            entityids = tacticlist.Select(tactic => tactic.PlanTacticId).ToList();
                        }

                        var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();
                        returnData = cusomfieldEntity.Select(customfield => new ViewByModel
                        {
                            Value = customfield.Value,
                            Text = customfield.Value
                        }).Select(customfield => customfield).Distinct().OrderBy(customfield => customfield.Text).ToList();
                        //// apply sorting
                        returnData = returnData.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
                    }
                }
            }

            return returnData;
        }

        /// <summary>
        /// Get Report Header Data for revenue & conversion.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="isRevenue"></param>
        /// <returns></returns>
        public JsonResult GetReportHeader(string option, bool isRevenue = true)
        {
            //// get month list
            List<string> includeMonth = GetMonthListForReport(option, true);

            //// get tactic data from tempdata variable
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            double projectedRevenue = 0;
            double actualRevenue = 0;
            double projectedMQL = 0;
            double actualMQL = 0;
            if (Tacticdata.Count > 0)
            {
                string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
                string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();

                //// get actual list of tactic from tactic data
                List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = new List<Plan_Campaign_Program_Tactic_Actual>();
                Tacticdata.ForEach(tactic => tactic.ActualTacticList.Where(tacticactual => includeMonth.Contains(tactic.TacticYear + tacticactual.Period) && (tacticactual.StageTitle == mql || tacticactual.StageTitle == revenue)).ToList().ForEach(actual => planTacticActual.Add(actual)));

                //// chech it from revenue or conversion
                if (isRevenue)
                {
                    projectedRevenue = GetProjectedRevenueValueDataTableForReport(Tacticdata).Where(mr => includeMonth.Contains(mr.Month)).Sum(r => r.Value);
                    actualRevenue = planTacticActual.Where(ta => ta.StageTitle.Equals(revenue))
                                                    .Sum(ta => ta.Actualvalue);
                }
                else
                {
                    projectedMQL = GetProjectedMQLValueDataTableForReport(Tacticdata).Where(mr => includeMonth.Contains(mr.Month)).Sum(r => r.Value);
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
                int currentQuarter = ((currentMonth - 1) / 3) + 1;
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
        /// <param name="timeFrameOption"></param>
        /// <returns>return conversion partial view</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ReportView)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #519 to implement user permission Logic
        public ActionResult GetConversionData(string timeFrameOption = "thisquarter")
        {
            //// Get list of month display in view
            ViewBag.MonthTitle = GetDisplayMonthListForReport(timeFrameOption);

            //// get tactic list
            List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting();

            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();

            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            TempData["ReportData"] = tacticStageList;

            // Fetch the Custom field data based upon id's .
            // Modified by : #960 Kalpesh Sharma : Combine the method by passing one extra parameter.

            //// conversion summary view by dropdown
            List<ViewByModel> lstParentConversionSummery = new List<ViewByModel>();
            lstParentConversionSummery.Add(new ViewByModel { Text = Common.CustomLabelFor(Enums.CustomLabelCode.Audience), Value = Common.RevenueAudience });
            lstParentConversionSummery.Add(new ViewByModel { Text = Common.RevenueGeography, Value = Common.RevenueGeography });
            lstParentConversionSummery.Add(new ViewByModel { Text = Common.RevenueVertical, Value = Common.RevenueVertical });
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count != 1)
            {
                lstParentConversionSummery.Add(new ViewByModel { Text = Common.RevenueBusinessUnit, Value = Common.RevenueBusinessUnit });
            }
            lstParentConversionSummery = lstParentConversionSummery.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            // Modified by : #960 Kalpesh Sharma : Filter changes for Revenue report - Revenue Report
            //Concat the Campaign and Program custom fields data with exsiting one. 
            var lstCustomFields = Common.GetCustomFields(tacticlist.Select(tactic => tactic.PlanTacticId).ToList(), programlist, campaignlist);
            lstParentConversionSummery = lstParentConversionSummery.Concat(lstCustomFields).ToList();
            ViewBag.parentConvertionSummery = lstParentConversionSummery;

            //// conversion performance view by dropdown
            List<ViewByModel> lstParentConversionPerformance = new List<ViewByModel>();
            lstParentConversionPerformance.Add(new ViewByModel { Text = Common.Plan, Value = Common.Plan });
            lstParentConversionPerformance.Add(new ViewByModel { Text = Common.Trend, Value = Common.Trend });
            lstParentConversionPerformance.Add(new ViewByModel { Text = Common.Actuals, Value = Common.Actuals });
            lstParentConversionPerformance = lstParentConversionPerformance.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            ViewBag.parentConvertionPerformance = lstParentConversionPerformance;
            ViewBag.ChildTabListAudience = GetChildLabelDataViewByModel(Common.RevenueAudience);

            return PartialView("Conversion");
        }

        #region MQL Conversion Plan Report

        /// <summary>
        /// This method returns the data for MQL Conversion plan summary report
        /// </summary>
        /// <param name="ParentTab"></param>
        /// <param name="Id"></param>
        /// <param name="selectOption"></param>
        /// <returns></returns>
        public JsonResult GetMQLConversionPlanData(string ParentTab = "", string Id = "", string selectOption = "")
        {
            //// get list of include based on option
            List<string> includeMonth = GetMonthListForReport(selectOption);

            //// get data from tempdata variable
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            //// load tacticdata based on ParentTab.
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
            else if (ParentTab.Contains(Common.TacticCustomTitle))
            {
                int customfieldId = Convert.ToInt32(ParentTab.Replace(Common.TacticCustomTitle, ""));
                List<int> entityids = db.CustomField_Entity.Where(customfield => customfield.CustomFieldId == customfieldId && customfield.Value == Id).Select(customfield => customfield.EntityId).ToList();
                Tacticdata = Tacticdata.Where(pcpt => entityids.Contains(pcpt.TacticObj.PlanTacticId)).ToList();
            }
            else if (ParentTab.Contains(Common.CampaignCustomTitle))
            {
                int customfieldId = Convert.ToInt32(ParentTab.Replace(Common.CampaignCustomTitle, ""));
                List<int> entityids = db.CustomField_Entity.Where(customfield => customfield.CustomFieldId == customfieldId && customfield.Value == Id).Select(customfield => customfield.EntityId).ToList();
                Tacticdata = Tacticdata.Where(pcpt => entityids.Contains(pcpt.TacticObj.Plan_Campaign_Program.PlanCampaignId)).ToList();
            }
            else if (ParentTab.Contains(Common.ProgramCustomTitle))
            {
                int customfieldId = Convert.ToInt32(ParentTab.Replace(Common.ProgramCustomTitle, ""));
                List<int> entityids = db.CustomField_Entity.Where(customfield => customfield.CustomFieldId == customfieldId && customfield.Value == Id).Select(customfield => customfield.EntityId).ToList();
                Tacticdata = Tacticdata.Where(pcpt => entityids.Contains(pcpt.TacticObj.PlanProgramId)).ToList();
            }
            //// Calculate MQL & INQ data.
            string inq = Enums.Stage.INQ.ToString();
            int INQStageId = db.Stages.FirstOrDefault(stage => stage.ClientId == Sessions.User.ClientId && stage.Code == inq && stage.IsDeleted == false).StageId;
            if (Tacticdata.Count() > 0)
            {
                string inspectStageINQ = Enums.InspectStageValues[Enums.InspectStage.INQ.ToString()].ToString();
                string inspectStageMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();

                List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = new List<Plan_Campaign_Program_Tactic_Actual>();
                Tacticdata.ForEach(tacticactual => tacticactual.ActualTacticList.ForEach(actual => planTacticActual.Add(actual)));
                planTacticActual = planTacticActual.Where(tacticactual => includeMonth.Contains((Tacticdata.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == tacticactual.PlanTacticId).TacticYear) + tacticactual.Period)).ToList();

                var rdata = new[] { new { 
                INQGoal = GetConversionProjectedINQData(Tacticdata).Where(tactictable => includeMonth.Contains(tactictable.Month)).GroupBy(tactictable => tactictable.Month).Select(tactictable => new
                {
                    PKey = tactictable.Key,
                    PSum = tactictable.Sum(tactic => tactic.Value)
                }),
                monthList = includeMonth,
                INQActual = GetActualValueForINQ(planTacticActual,INQStageId).GroupBy(tactictable => Tacticdata.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == tactictable.PlanTacticId).TacticYear + tactictable.Period)
                                             .Select(tactictable => new
                                              {
                                                PKey = tactictable.Key,
                                                PSum = tactictable.Sum(tactic => tactic.Actualvalue)
                                              }),
                MQLGoal = GetProjectedMQLValueDataTableForReport(Tacticdata).Where(tactictable => includeMonth.Contains(tactictable.Month)).GroupBy(tactictable => tactictable.Month).Select(tactictable => new
                {
                    PKey = tactictable.Key,
                    PSum = tactictable.Sum(tactic => tactic.Value)
                }),
                MQLActual = planTacticActual.Where(pcpt => pcpt.StageTitle.Equals(inspectStageMQL))
                            .GroupBy(pt => Tacticdata.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == pt.PlanTacticId).TacticYear + pt.Period)
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
            var returnDataGuid = new object();

            //// Get child tab data based on ParentTab.
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
                returnDataGuid = (db.Audiences.Where(au => au.ClientId.Equals(Sessions.User.ClientId) && au.IsDeleted.Equals(false))).ToList().Select(a => new
                {
                    id = a.AudienceId,
                    title = a.Title
                }).Distinct().OrderBy(a => a.id);
            }
            else if (ParentTab == Common.Geography)
            {
                returnDataGuid = (db.Geographies.Where(ge => ge.ClientId.Equals(Sessions.User.ClientId) && ge.IsDeleted.Equals(false))).ToList().Select(g => new
                {
                    id = g.GeographyId,
                    title = g.Title
                }).Distinct().OrderBy(g => g.id);
            }
            else if (ParentTab == Common.Vertical)
            {
                returnDataGuid = (db.Verticals.Where(ve => ve.ClientId.Equals(Sessions.User.ClientId) && ve.IsDeleted.Equals(false))).ToList().Select(v => new
                {
                    id = v.VerticalId,
                    title = v.Title
                }).Distinct().OrderBy(v => v.id);
            }

            return Json(returnDataGuid, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region MQL Performance Report

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
        /// <param name="selectOption">Selection Option</param>
        /// </summary>
        /// <returns>Returns json result of source perfromance trend.</returns>
        private JsonResult GetMQLPerformanceTrend(string selectOption)
        {
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            List<string> months = GetUpToCurrentMonth();

            int lastMonth = GetLastMonthForTrend(selectOption);
            List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.Where(actual => months.Contains(actual.Period) && actual.StageTitle == mql).ToList().ForEach(a => planTacticActual.Add(a)));

            //// Get BusinessUnit data.
            var tacticTrenBusinessUnit = planTacticActual
                                                   .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.BusinessUnitId)
                                                   .Select(ta => new
                                                   {
                                                       BusinessUnitId = ta.Key,
                                                       Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth)
                                                   });
            //// Get Geography data.
            var tacticTrendGeography = planTacticActual
                                        .GroupBy(ta => ta.Plan_Campaign_Program_Tactic.GeographyId)
                                        .Select(ta => new
                                        {
                                            GeographyId = ta.Key,
                                            Trend = ((ta.Sum(actual => actual.Actualvalue) / currentMonth) * lastMonth)
                                        });

            //// Get Vertical data.
            var tacticTrendVertical = planTacticActual
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

                                           }).OrderByDescending(b => b.Value).ThenBy(b => b.Title).Take(5);
            var vertical = db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted == false).ToList()
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = tacticTrendVertical.Any(ve => ve.VerticalId.Equals(v.VerticalId)) ? tacticTrendVertical.Where(ve => ve.VerticalId.Equals(v.VerticalId)).First().Trend : 0
                                                }).OrderByDescending(v => v.Value).ThenBy(v => v.Title).Take(5);

            var geography = db.Geographies.Where(g => g.ClientId == Sessions.User.ClientId && g.IsDeleted == false).ToList()
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = tacticTrendGeography.Any(ge => ge.GeographyId.Equals(g.GeographyId)) ? tacticTrendGeography.Where(ge => ge.GeographyId.Equals(g.GeographyId)).First().Trend : 0
                                                }).OrderByDescending(g => g.Value).ThenBy(g => g.Title).Take(5);
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
            Tacticdata.ForEach(tactic => tactic.ActualTacticList.ForEach(_tac => planTacticActuals.Add(_tac)));

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
                                                }).OrderByDescending(ta => ta.Value).ThenBy(b => b.Title).Take(5); ;
            var vertical = db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted == false).ToList()
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
                                                }).OrderByDescending(ta => ta.Value).ThenBy(v => v.Title).Take(5);

            var geography = db.Geographies.Where(g => g.ClientId == Sessions.User.ClientId && g.IsDeleted == false).ToList()
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
                                                }).OrderByDescending(ta => ta.Value).ThenBy(g => g.Title).Take(5); ;
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
        /// <param name="selectOption">Selection Option</param>
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
                                                                                        .Where(mr => includeMonth.Contains(mr.Month))
                                                                                        .Sum(r => r.Value) :
                                                                                        0
                                                }).OrderByDescending(ta => ta.Value).ThenBy(ta => ta.Title).Take(5);



            var vertical = db.Verticals.Where(v => v.ClientId == Sessions.User.ClientId && v.IsDeleted == false).ToList()
                                                .Select(v => new
                                                {
                                                    Title = v.Title,
                                                    ColorCode = string.Format("#{0}", v.ColorCode),
                                                    Value = Tacticdata.Any(t => t.TacticObj.VerticalId.Equals(v.VerticalId)) ?
                                                            GetProjectedMQLValueDataTableForReport(Tacticdata.Where(t => t.TacticObj.VerticalId.Equals(v.VerticalId))
                                                                                       .ToList())
                                                                                       .Where(mr => includeMonth.Contains(mr.Month))
                                                                                       .Sum(r => r.Value) :
                                                                                       0
                                                }).OrderByDescending(ta => ta.Value).ThenBy(ta => ta.Title).Take(5);

            var geography = db.Geographies.Where(g => g.ClientId == Sessions.User.ClientId && g.IsDeleted == false).ToList()
                                                .Select(g => new
                                                {
                                                    Title = g.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = Tacticdata.Any(t => t.TacticObj.GeographyId.Equals(g.GeographyId)) ?
                                                    GetProjectedMQLValueDataTableForReport(Tacticdata.Where(t => t.TacticObj.GeographyId.Equals(g.GeographyId))
                                                                               .ToList())
                                                                               .Where(mr => includeMonth.Contains(mr.Month))
                                                                               .Sum(r => r.Value) :
                                                                               0
                                                }).OrderByDescending(ta => ta.Value).ThenBy(ta => ta.Title).Take(5);
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
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];
            bool IsCampaignCustomField = false;
            bool IsProgramCustomField = false;
            bool IsTacticCustomField = false;

            //Custom
            if (ParentConversionSummaryTab.Contains(Common.TacticCustomTitle))
            {
                int customfieldId = Convert.ToInt32(ParentConversionSummaryTab.Replace(Common.TacticCustomTitle, ""));
                List<int> entityids = db.CustomField_Entity.Where(e => e.CustomFieldId == customfieldId).Select(e => e.EntityId).ToList();
                Tacticdata = Tacticdata.Where(t => entityids.Contains(t.TacticObj.PlanTacticId)).ToList();
                IsTacticCustomField = true;
            }
            else if (ParentConversionSummaryTab.Contains(Common.CampaignCustomTitle))
            {
                int customfieldId = Convert.ToInt32(ParentConversionSummaryTab.Replace(Common.CampaignCustomTitle, ""));
                List<int> entityids = db.CustomField_Entity.Where(e => e.CustomFieldId == customfieldId).Select(e => e.EntityId).ToList();
                Tacticdata = Tacticdata.Where(t => entityids.Contains(t.TacticObj.Plan_Campaign_Program.PlanCampaignId)).ToList();
                IsCampaignCustomField = true;
            }
            else if (ParentConversionSummaryTab.Contains(Common.ProgramCustomTitle))
            {
                int customfieldId = Convert.ToInt32(ParentConversionSummaryTab.Replace(Common.ProgramCustomTitle, ""));
                List<int> entityids = db.CustomField_Entity.Where(e => e.CustomFieldId == customfieldId).Select(e => e.EntityId).ToList();
                Tacticdata = Tacticdata.Where(t => entityids.Contains(t.TacticObj.PlanProgramId)).ToList();
                IsProgramCustomField = true;
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
                int customfieldId = 0;
                List<int> entityids = new List<int>();
                if (IsTacticCustomField)
                {
                    customfieldId = Convert.ToInt32(ParentConversionSummaryTab.Replace(Common.TacticCustomTitle, ""));
                    entityids = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
                }
                else if (IsCampaignCustomField)
                {
                    customfieldId = Convert.ToInt32(ParentConversionSummaryTab.Replace(Common.CampaignCustomTitle, ""));
                    entityids = Tacticdata.Select(t => t.TacticObj.Plan_Campaign_Program.PlanCampaignId).ToList();
                }
                else
                {
                    customfieldId = Convert.ToInt32(ParentConversionSummaryTab.Replace(Common.ProgramCustomTitle, ""));
                    entityids = Tacticdata.Select(t => t.TacticObj.PlanProgramId).ToList();
                }

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
                                      // Modified By : Kalpesh Sharma Filter changes for Revenue report - Revenue Report
                                      // Fetch the filtered list based upon custom fields type
                                      planTacticList = Tacticdata.Where(t => cusomfieldEntity.Where(c => c.Value == pc.Key.id.ToString()).Select(c => c.EntityId).ToList().Contains(IsCampaignCustomField ? t.TacticObj.Plan_Campaign_Program.PlanCampaignId :
                                          (IsProgramCustomField ? t.TacticObj.PlanProgramId : t.TacticObj.PlanTacticId))).Select(t => t.TacticObj.PlanTacticId).ToList()
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
            string stageTitleMQL = Enums.InspectStage.MQL.ToString();
            string stageTitleCW = Enums.InspectStage.CW.ToString();
            string stageTitleRevenue = Enums.InspectStage.Revenue.ToString();
            string marketing = Enums.Funnel.Marketing.ToString();

            List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => planTacticActual.Add(a)));
            planTacticActual = planTacticActual.Where(mr => includeMonth.Contains((Tacticdata.FirstOrDefault(_tac => _tac.TacticObj.PlanTacticId == mr.PlanTacticId).TacticYear) + mr.Period)).ToList();
            string inq = Enums.Stage.INQ.ToString();
            int INQStageId = db.Stages.FirstOrDefault(stage => stage.ClientId == Sessions.User.ClientId && stage.Code == inq && stage.IsDeleted == false).StageId;

            var DataListFinal = DataTitleList.Select(p => new
            {
                Title = p.Title,
                INQ = GetActualValueForINQ(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), INQStageId).Sum(a => a.Actualvalue),
                MQL = GetActualValueForConversionSummary(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleMQL),
                ActualCW = GetActualValueForConversionSummary(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleCW),
                ActualRevenue = GetActualValueForConversionSummary(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleRevenue),
                ActualADS = CalculateActualADS(planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), stageTitleCW, stageTitleRevenue),
                ProjectedCW = GetProjectedCWValueDataTableForReport(Tacticdata.Where(t => p.planTacticList.Contains(t.TacticObj.PlanTacticId)).ToList()).Where(mr => includeMonth.Contains(mr.Month)).Sum(r => r.Value),
                ProjectedRevenue = GetProjectedRevenueValueDataTableForReport(Tacticdata.Where(t => p.planTacticList.Contains(t.TacticObj.PlanTacticId)).ToList()).Where(mr => includeMonth.Contains(mr.Month)).Sum(r => r.Value),
                ProjectedADS = p.planTacticList.Any() ? db.Model_Funnel.Where(mf => mf.Funnel.Title == marketing && (db.Plan_Campaign_Program_Tactic.Where(t => p.planTacticList.Contains(t.PlanTacticId)).Select(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId).Distinct()).Contains(mf.ModelId)).Sum(mf => mf.AverageDealSize) : 0
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
            var returnData = db.Funnels.Where(fu => fu.IsDeleted == false).ToList().Select(funl => new
            {
                id = funl.FunnelId,
                title = funl.Title
            }).Select(funl => funl).Distinct().OrderBy(funl => funl.id);
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
            var lstBusinessunits = db.BusinessUnits.Where(bu => bu.ClientId == Sessions.User.ClientId && bu.IsDeleted == false && Sessions.ReportBusinessUnitIds.Contains(bu.BusinessUnitId)).OrderBy(bu => bu.Title).ToList();//Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
            if (lstBusinessunits.Count == 0)
            {
                BusinessUnit objBusinessUnit = new BusinessUnit();
                objBusinessUnit.BusinessUnitId = Guid.Empty;
                objBusinessUnit.Title = "None";
                lstBusinessunits.Add(objBusinessUnit);

            }
            lstBusinessunits = lstBusinessunits.Where(bu => !string.IsNullOrEmpty(bu.Title)).OrderBy(bu => bu.Title, new AlphaNumericComparer()).ToList();
            ViewBag.BusinessUnit = lstBusinessunits;
            List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting();

            // Modified by : #960 Kalpesh Sharma : Filter changes for Revenue report - Revenue Report
            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(t => t.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(t => t.PlanProgramId).ToList();

            //// Get First Businessunit id
            Guid businessunitid = new Guid();
            if (lstBusinessunits.Count > 0)
            {
                businessunitid = lstBusinessunits.Select(businessunit => businessunit.BusinessUnitId).FirstOrDefault();
            }

            //// Get Campaign list for dropdown
            List<int> campaignIds = tacticlist.Where(t => t.BusinessUnitId == businessunitid).Select(t => t.Plan_Campaign_Program.PlanCampaignId).Distinct().ToList<int>();
            var campaignList = db.Plan_Campaign.Where(pc => campaignIds.Contains(pc.PlanCampaignId))
                    .Select(pcp => new { PlanCampaignId = pcp.PlanCampaignId, Title = pcp.Title })
                    .OrderBy(pcp => pcp.Title).ToList();
            campaignList = campaignList.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
            var lstCampaignList = campaignList;
            lstCampaignList.Insert(0, new { PlanCampaignId = 0, Title = "All Campaigns" });
            
            //// Get Program list for dropdown
            var programList = db.Plan_Campaign_Program.Where(pc => campaignIds.Contains(pc.PlanCampaignId))
                   .Select(c => new { PlanProgramId = c.PlanProgramId, Title = c.Title })
                   .OrderBy(pcp => pcp.Title).ToList();
            programList = programList.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
            var lstProgramList = programList;
            lstProgramList.Insert(0, new { PlanProgramId = 0, Title = "All Programs" });
            
            //// Get tactic list for dropdown
            var tacticListinner = tacticlist.Where(t => t.BusinessUnitId == businessunitid)
                .Select(t => new { PlanTacticId = t.PlanTacticId, Title = t.Title })
                .OrderBy(pcp => pcp.Title).ToList();
            tacticListinner = tacticListinner.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
            var lstTacticList = tacticListinner;
            lstTacticList.Insert(0, new { PlanTacticId = 0, Title = "All Tactics" });

            //// Set in viewbag
            ViewBag.CampaignDropdownList = lstCampaignList;
            ViewBag.ProgramDropdownList = lstProgramList;
            ViewBag.TacticDropdownList = lstTacticList;

            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist);
            string customLableAudience = Common.CustomLabelFor(Enums.CustomLabelCode.Audience);

            //// Set Parent Revenue Summary data to list.
            List<ViewByModel> lstParentRevenueSummery = new List<ViewByModel>();
            lstParentRevenueSummery.Add(new ViewByModel { Text = Common.RevenueGeography, Value = Common.RevenueGeography });
            lstParentRevenueSummery.Add(new ViewByModel { Text = Common.RevenueVertical, Value = Common.RevenueVertical });
            lstParentRevenueSummery.Add(new ViewByModel { Text = customLableAudience, Value = Common.RevenueAudience });
            if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count != 1)
            {
                lstParentRevenueSummery.Add(new ViewByModel { Text = Common.RevenuePlans, Value = Common.RevenuePlans });
            }
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count != 1)
            {
                lstParentRevenueSummery.Add(new ViewByModel { Text = Common.RevenueBusinessUnit, Value = Common.RevenueBusinessUnit });
            }
            lstParentRevenueSummery = lstParentRevenueSummery.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();

            // Modified by : #960 Kalpesh Sharma : Filter changes for Revenue report - Revenue Report
            //Concat the Campaign and Program custom fields data with exsiting one. 
            var lstCustomFields = Common.GetCustomFields(tacticlist.Select(tactic => tactic.PlanTacticId).ToList(), programlist, campaignlist);
            lstParentRevenueSummery = lstParentRevenueSummery.Concat(lstCustomFields).ToList();
            ViewBag.parentRevenueSummery = lstParentRevenueSummery;

            //// Set Parent Revenue Plan data to list.
            List<ViewByModel> lstParentRevenueToPlan = new List<ViewByModel>();
            lstParentRevenueToPlan.Add(new ViewByModel { Text = Common.RevenueVertical, Value = Common.RevenueVertical });
            lstParentRevenueToPlan.Add(new ViewByModel { Text = customLableAudience, Value = Common.RevenueAudience });
            lstParentRevenueToPlan.Add(new ViewByModel { Text = Common.RevenueGeography, Value = Common.RevenueGeography });
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count != 1)
            {
                lstParentRevenueToPlan.Add(new ViewByModel { Text = Common.RevenueBusinessUnit, Value = Common.RevenueBusinessUnit });
            }
            lstParentRevenueToPlan = lstParentRevenueToPlan.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            lstParentRevenueToPlan = lstParentRevenueToPlan.Concat(lstCustomFields).ToList();
            ViewBag.parentRevenueToPlan = lstParentRevenueToPlan;

            //// Set Parent Revenue Contribution data to list.
            List<ViewByModel> lstParentRevenueContribution = new List<ViewByModel>();
            lstParentRevenueContribution.Add(new ViewByModel { Text = customLableAudience, Value = Common.RevenueAudience });
            lstParentRevenueContribution.Add(new ViewByModel { Text = Common.RevenueGeography, Value = Common.RevenueGeography });
            lstParentRevenueContribution.Add(new ViewByModel { Text = Common.RevenueVertical, Value = Common.RevenueVertical });
            lstParentRevenueContribution.Add(new ViewByModel { Text = Common.RevenueCampaign, Value = Common.RevenueCampaign });
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count != 1)
            {
                lstParentRevenueContribution.Add(new ViewByModel { Text = Common.RevenueBusinessUnit, Value = Common.RevenueBusinessUnit });
            }
            lstParentRevenueContribution = lstParentRevenueContribution.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            //Concat the Campaign and Program custom fields data with exsiting one. 
            lstParentRevenueContribution = lstParentRevenueContribution.Concat(lstCustomFields).ToList();
            ViewBag.parentRevenueContribution = lstParentRevenueContribution;
            ViewBag.ChildTabListAudience = GetChildLabelDataViewByModel(Common.RevenueAudience);

            //// Set child BusinessUnit data to list.
            List<ViewByModel> listChildBusinessunit = GetChildLabelDataViewByModel(Common.RevenueBusinessUnit);
            listChildBusinessunit.Insert(0, new ViewByModel { Value = "0", Text = "All Business Units" });
            ViewBag.ChildTabListBusinessUnit = listChildBusinessunit;
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
            List<int> entityids = new List<int>();
            //Check the custom field typa and replace the id with eliminate extra word 
            // Modified by : #960 Kalpesh Sharma : Filter changes for Revenue report - Revenue Report 
            if (ParentLabel.Contains(Common.TacticCustomTitle) || ParentLabel.Contains(Common.CampaignCustomTitle) || ParentLabel.Contains(Common.ProgramCustomTitle))
            {
                int customfieldId = 0;
                if (ParentLabel.Contains(Common.TacticCustomTitle))
                {
                    customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.TacticCustomTitle, ""));
                }
                else if (ParentLabel.Contains(Common.CampaignCustomTitle))
                {
                    customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.CampaignCustomTitle, ""));
                }
                else if (ParentLabel.Contains(Common.ProgramCustomTitle))
                {
                    customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.ProgramCustomTitle, ""));
                }

                string customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                entityids = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && c.Value == id).Select(c => c.EntityId).ToList();
            }

            // Modified by : #960 Kalpesh Sharma : Filter changes for Revenue report - Revenue Report 
            // merge campaing and program entity condition with exisiting one.   
            Tacticdata = Tacticdata.Where(pcpt => ((ParentLabel == Common.RevenueBusinessUnit && pcpt.TacticObj.BusinessUnitId == new Guid(id)) ||
                (ParentLabel == Common.RevenueGeography && pcpt.TacticObj.GeographyId == new Guid(id)) ||
                (ParentLabel == Common.RevenueAudience && pcpt.TacticObj.AudienceId == Convert.ToInt32(id)) ||
                (ParentLabel == Common.RevenueVertical && pcpt.TacticObj.VerticalId == Convert.ToInt32(id)) ||
                (ParentLabel == Common.RevenuePlans && pcpt.TacticObj.Plan_Campaign_Program.Plan_Campaign.PlanId == Convert.ToInt32(id)) ||
                (ParentLabel.Contains(Common.TacticCustomTitle) && entityids.Contains(pcpt.TacticObj.PlanTacticId)) ||
                (ParentLabel.Contains(Common.CampaignCustomTitle) && entityids.Contains(pcpt.TacticObj.Plan_Campaign_Program.PlanCampaignId)) ||
                (ParentLabel.Contains(Common.ProgramCustomTitle) && entityids.Contains(pcpt.TacticObj.PlanProgramId))
                )).ToList();
            List<int> tacticIdList = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();

            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));
            ActualTacticList = ActualTacticList.Where(mr => includeMonth.Contains((Tacticdata.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == mr.PlanTacticId).TacticYear) + mr.Period)).ToList();

            var campaignListobj = Tacticdata.GroupBy(pc => new { PCid = pc.TacticObj.Plan_Campaign_Program.PlanCampaignId, title = pc.TacticObj.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pc =>
                        new CampaignData
                        {
                            PlanCampaignId = pc.Key.PCid,
                            Title = pc.Key.title,
                            planTacticList = pc.Select(tactic => tactic.TacticObj.PlanTacticId).ToList()
                        }).ToList();

            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            string mql = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            List<TacticMonthValue> ProjectedRevenueDatatable = GetProjectedRevenueValueDataTableForReport(Tacticdata);
            List<TacticMonthValue> MQLDatatable = GetProjectedMQLValueDataTableForReport(Tacticdata);
            var campaignList = campaignListobj.Select(campaign => new
            {
                id = campaign.PlanCampaignId,
                title = campaign.Title,
                monthList = includeMonth,
                trevenueProjected = ProjectedRevenueDatatable.Where(mr => campaign.planTacticList.Contains(mr.Id) && includeMonth.Contains(mr.Month)).GroupBy(r => r.Month).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Value)
                }),
                tproject = MQLDatatable.Where(mr => campaign.planTacticList.Contains(mr.Id) && includeMonth.Contains(mr.Month)).GroupBy(r => r.Month).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Value)
                }),
                tRevenueActual = ActualTacticList.Where(pcpt => campaign.planTacticList.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(revenue)).GroupBy(pt => Tacticdata.FirstOrDefault(_tactic => _tactic.TacticObj.PlanTacticId == pt.PlanTacticId).TacticYear + pt.Period).Select(pcptj => new
                {
                    key = pcptj.Key,
                    ActualValue = pcptj.Sum(pt => pt.Actualvalue)
                }),
                tacticActual = ActualTacticList.Where(pcpt => campaign.planTacticList.Contains(pcpt.PlanTacticId) && pcpt.StageTitle.Equals(mql)).GroupBy(pt => Tacticdata.FirstOrDefault(_tactic => _tactic.TacticObj.PlanTacticId == pt.PlanTacticId).TacticYear + pt.Period).Select(pcptj => new
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
        /// <param name="id">BusinessUnit Id</param>
        /// <returns>Returns Json Result of Campaign List.</returns>
        public JsonResult LoadCampaignDropDown(Guid id, string selectOption = "")
        {
            List<Plan_Campaign_Program_Tactic> TacticList = GetTacticForReporting();
            List<int> campaignIds = TacticList.Where(tactic => tactic.BusinessUnitId == id).Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).Distinct().ToList<int>();
            //// Set Campaign list data
            var campaignList = db.Plan_Campaign.Where(pc => campaignIds.Contains(pc.PlanCampaignId))
                    .Select(pcp => new { PlanCampaignId = pcp.PlanCampaignId, Title = pcp.Title })
                    .OrderBy(pcp => pcp.Title).ToList();
            if (campaignList == null)
                return Json(new { });
            campaignList = campaignList.Where(campaign => !string.IsNullOrEmpty(campaign.Title)).OrderBy(campaign => campaign.Title, new AlphaNumericComparer()).ToList();
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
            List<Plan_Campaign_Program_Tactic> TacticList = GetTacticForReporting();
            List<int> programIds = new List<int>();

            if (type == Common.RevenueBusinessUnit)
            {
                Guid businessunitid = new Guid(id);
                programIds = TacticList.Where(tactic => tactic.BusinessUnitId == businessunitid).Select(tactic => tactic.PlanProgramId).Distinct().ToList<int>();
            }
            else
            {
                int campaignid = Convert.ToInt32(id);
                programIds = TacticList.Where(tactic => tactic.Plan_Campaign_Program.PlanCampaignId == campaignid).Select(tactic => tactic.PlanProgramId).Distinct().ToList<int>();
            }
            var programList = db.Plan_Campaign_Program.Where(pc => programIds.Contains(pc.PlanProgramId))
                    .Select(program => new { PlanProgramId = program.PlanProgramId, Title = program.Title })
                    .OrderBy(pcp => pcp.Title).ToList();
            if (programList == null)
                return Json(new { });
            programList = programList.Where(program => !string.IsNullOrEmpty(program.Title)).OrderBy(program => program.Title, new AlphaNumericComparer()).ToList();
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
            List<Plan_Campaign_Program_Tactic> TacticList = GetTacticForReporting();

            //// return tactic data bya Type.
            if (type == Common.RevenueBusinessUnit)
            {
                Guid businessunitid = new Guid(id);
                var tacticListinner = TacticList.Where(tactic => tactic.BusinessUnitId == businessunitid)
                    .Select(tactic => new { PlanTacticId = tactic.PlanTacticId, Title = tactic.Title })
                    .OrderBy(pcp => pcp.Title).ToList();
                if (tacticListinner == null)
                    return Json(new { });
                tacticListinner = tacticListinner.Where(tactic => !string.IsNullOrEmpty(tactic.Title)).OrderBy(tactic => tactic.Title, new AlphaNumericComparer()).ToList();
                return Json(tacticListinner, JsonRequestBehavior.AllowGet);
            }
            else if (type == Common.RevenueCampaign)
            {
                int campaignid = Convert.ToInt32(id);
                var tacticListinner = TacticList.Where(tactic => tactic.Plan_Campaign_Program.PlanCampaignId == campaignid)
                    .Select(tactic => new { PlanTacticId = tactic.PlanTacticId, Title = tactic.Title })
                    .OrderBy(pcp => pcp.Title).ToList();
                if (tacticListinner == null)
                    return Json(new { });
                tacticListinner = tacticListinner.Where(tactic => !string.IsNullOrEmpty(tactic.Title)).OrderBy(tactic => tactic.Title, new AlphaNumericComparer()).ToList();
                return Json(tacticListinner, JsonRequestBehavior.AllowGet);
            }
            else
            {
                int programid = Convert.ToInt32(id);
                var tacticListinner = TacticList.Where(tactic => tactic.PlanProgramId == programid)
                .Select(tactic => new { PlanTacticId = tactic.PlanTacticId, Title = tactic.Title })
                .OrderBy(pcp => pcp.Title).ToList();
                if (tacticListinner == null)
                    return Json(new { });
                tacticListinner = tacticListinner.Where(tactic => !string.IsNullOrEmpty(tactic.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
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

            List<string> includeMonth = GetMonthListForReport(selectOption);

            Guid BusinessUnitid = new Guid(businessUnitId);

            //// Set Tacticdata based on Type.
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
                int INQStaged = db.Stages.FirstOrDefault(stage => stage.ClientId == Sessions.User.ClientId && stage.Code == inq).StageId;
                List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                Tacticdata.ForEach(tactic => tactic.ActualTacticList.ForEach(_tac => ActualTacticList.Add(_tac)));
                ActualTacticList = ActualTacticList.Where(mr => includeMonth.Contains((Tacticdata.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == mr.PlanTacticId).TacticYear) + mr.Period)).ToList();

                //// Set INQ & MQL values.
                var rdata = new[] { new { 
                INQGoal = GetProjectedINQDataWithVelocity(Tacticdata).Where(mr => includeMonth.Contains(mr.Month)).GroupBy(r => r.Month).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Value)
                }),
                monthList = includeMonth,
                INQActual = ActualTacticList.Where(pcpt => pcpt.Plan_Campaign_Program_Tactic.StageId == INQStaged && pcpt.StageTitle.Equals(Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString())).GroupBy(pt => pt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pt.Period).Select(pcptj => new
                {
                    PKey = pcptj.Key,
                    PSum = pcptj.Sum(pt => pt.Actualvalue)
                }),
                MQLGoal = GetProjectedMQLDataWithVelocity(Tacticdata).Where(mr => includeMonth.Contains(mr.Month)).GroupBy(r => r.Month).Select(g => new
                {
                    PKey = g.Key,
                    PSum = g.Sum(r => r.Value)
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

            Guid buid = new Guid();
            bool IsCampaignCustomField = false;
            bool IsProgramCustomField = false;
            bool IsTacticCustomField = false;

            if (isBusinessUnit && !string.IsNullOrEmpty(businessUnitId))
            {
                buid = new Guid(businessUnitId);
            }
            //Modified By : Kalpesh Sharma #960 Filter changes for Revenue report - Revenue Report
            // Check the custom field type and remove extra string for get Custom Field Id
            if (parentlabel.Contains(Common.TacticCustomTitle))
            {
                int customfieldId = Convert.ToInt32(parentlabel.Replace(Common.TacticCustomTitle, ""));
                List<int> entityids = db.CustomField_Entity.Where(custField => custField.CustomFieldId == customfieldId).Select(custField => custField.EntityId).ToList();
                Tacticdata = Tacticdata.Where(t => entityids.Contains(t.TacticObj.PlanTacticId)).ToList();
                IsTacticCustomField = true;
            }
            else if (parentlabel.Contains(Common.CampaignCustomTitle))
            {
                int customfieldId = Convert.ToInt32(parentlabel.Replace(Common.CampaignCustomTitle, ""));
                List<int> entityids = db.CustomField_Entity.Where(custField => custField.CustomFieldId == customfieldId).Select(custField => custField.EntityId).ToList();
                Tacticdata = Tacticdata.Where(tactic => entityids.Contains(tactic.TacticObj.Plan_Campaign_Program.PlanCampaignId)).ToList();
                IsCampaignCustomField = true;
            }
            else if (parentlabel.Contains(Common.ProgramCustomTitle))
            {
                int customfieldId = Convert.ToInt32(parentlabel.Replace(Common.ProgramCustomTitle, ""));
                List<int> entityids = db.CustomField_Entity.Where(custField => custField.CustomFieldId == customfieldId).Select(custField => custField.EntityId).ToList();
                Tacticdata = Tacticdata.Where(tactic => entityids.Contains(tactic.TacticObj.PlanProgramId)).ToList();
                IsProgramCustomField = true;
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
                Tacticdata = Tacticdata.Where(tactic => tactic.TacticObj.BusinessUnitId == buid).ToList();
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

            //Check the custom field type and filter tactic based on Custom field Id
            else if (parentlabel.Contains(Common.TacticCustomTitle) || parentlabel.Contains(Common.CampaignCustomTitle) || parentlabel.Contains(Common.ProgramCustomTitle))
            {
                int customfieldId = 0;
                List<int> entityids = new List<int>();
                if (IsTacticCustomField)
                {
                    customfieldId = Convert.ToInt32(parentlabel.Replace(Common.TacticCustomTitle, ""));
                    entityids = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
                }
                else if (IsCampaignCustomField)
                {
                    customfieldId = Convert.ToInt32(parentlabel.Replace(Common.CampaignCustomTitle, ""));
                    entityids = Tacticdata.Select(t => t.TacticObj.Plan_Campaign_Program.PlanCampaignId).ToList();
                }
                else
                {
                    customfieldId = Convert.ToInt32(parentlabel.Replace(Common.ProgramCustomTitle, ""));
                    entityids = Tacticdata.Select(t => t.TacticObj.PlanProgramId).ToList();
                }

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
                                      // Modified By : Kalpesh Sharma Filter changes for Revenue report - Revenue Report
                                      // Fetch the filtered list based upon custom fields type
                                      planTacticList = Tacticdata.Where(t => cusomfieldEntity.Where(c => c.Value == pc.Key.id.ToString()).Select(c => c.EntityId).ToList().Contains(IsCampaignCustomField ? t.TacticObj.Plan_Campaign_Program.PlanCampaignId :
                                          (IsProgramCustomField ? t.TacticObj.PlanProgramId : t.TacticObj.PlanTacticId))).Select(t => t.TacticObj.PlanTacticId).ToList()
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
                List<TacticMonthValue> ProjectedRevenueDatatable = GetProjectedRevenueValueDataTableForReport(Tacticdata);
                List<TacticMonthValue> ProjectedCostDatatable = GetProjectedCostData(Tacticdata);

                List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));

                List<Plan_Tactic_Values> ActualRevenueTrendList = GetTrendRevenueDataContribution(ActualTacticList, lastMonth, monthList, revenue);
                List<string> monthWithYearList = GetUpToCurrentMonthWithYearForReport(selectOption, true);

                List<int> ObjTactic = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
                List<Plan_Campaign_Program_Tactic_LineItem> LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => ObjTactic.Contains(l.PlanTacticId) && l.IsDeleted == false).ToList();
                List<int> ObjTacticLineItemList = LineItemList.Select(t => t.PlanLineItemId).ToList();
                List<Plan_Campaign_Program_Tactic_LineItem_Actual> LineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(l => ObjTacticLineItemList.Contains(l.PlanLineItemId)).ToList();
                List<TacticMonthValue> ActualCostDatatable = GetActualCostData(Tacticdata, LineItemList, LineItemActualList);

                var campaignListFinal = campaignList.Select(p => new
                {
                    Title = p.Title,
                    PlanRevenue = ProjectedRevenueDatatable.Where(mr => p.planTacticList.Contains(mr.Id) && includeMonth.Contains(mr.Month)).Sum(r => r.Value),
                    ActualRevenue = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => p.planTacticList.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue) && includeMonth.Contains(ta.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + ta.Period)).ToList().Sum(ta => ta.Actualvalue),
                    TrendRevenue = 0,//GetTrendRevenueDataContribution(p.planTacticList, lastMonth),
                    PlanCost = ProjectedCostDatatable.Where(mr => p.planTacticList.Contains(mr.Id) && includeMonth.Contains(mr.Month)).Sum(r => r.Value),
                    //Added By : Kalpesh Sharma #734 Actual cost - Verify that report section is up to date with actual cost changes
                    ActualCost = ActualCostDatatable.Where(mr => p.planTacticList.Contains(mr.Id) && includeMonth.Contains(mr.Month)).Sum(r => r.Value),
                    TrendCost = 0,//GetTrendCostDataContribution(p.planTacticList, lastMonth),
                    RunRate = ActualRevenueTrendList.Where(ar => p.planTacticList.Contains(ar.PlanTacticId)).Sum(ar => ar.MQL),//GetTrendRevenueDataContribution(p.planTacticList, lastMonth, monthList),
                    PipelineCoverage = 0,//GetPipelineCoverage(p.planTacticList, lastMonth),
                    RevSpend = GetRevenueVSSpendContribution(ActualCostDatatable, ActualTacticList, p.planTacticList, monthWithYearList, monthList, revenue),
                    RevenueTotal = GetActualRevenueTotal(ActualTacticList, p.planTacticList, monthList, revenue),
                    CostTotal = ActualCostDatatable.Where(mr => p.planTacticList.Contains(mr.Id) && monthWithYearList.Contains(mr.Month)).Sum(r => r.Value)
                }).Select(p => p).Distinct().OrderBy(p => p.Title);

                return Json(campaignListFinal, JsonRequestBehavior.AllowGet);
            }
            return Json(new { });
        }


        private List<TacticMonthValue> GetActualCostData(List<TacticStageValue> Tacticdata, List<Plan_Campaign_Program_Tactic_LineItem> LineItemList, List<Plan_Campaign_Program_Tactic_LineItem_Actual> LineItemActualList)
        {
            List<TacticMonthValue> listmonthwise = new List<TacticMonthValue>();
            foreach (var tactic in Tacticdata)
            {
                int id = tactic.TacticObj.PlanTacticId;
                var InnerLineItemList = LineItemList.Where(l => l.PlanTacticId == id).ToList();
                if (InnerLineItemList.Count() > 0)
                {
                    var innerLineItemActualList = LineItemActualList.Where(la => InnerLineItemList.Select(line => line.PlanLineItemId).Contains(la.PlanLineItemId)).ToList();
                    innerLineItemActualList.ForEach(innerline => listmonthwise.Add(new TacticMonthValue { Id = id, Month = tactic.TacticYear + innerline.Period, Value = innerline.Value }));
                }
                else
                {
                    var innerTacticActualList = tactic.ActualTacticList.Where(actualTac => actualTac.StageTitle == Enums.InspectStage.Cost.ToString()).ToList();
                    innerTacticActualList.ForEach(innerTactic => listmonthwise.Add(new TacticMonthValue { Id = id, Month = tactic.TacticYear + innerTactic.Period, Value = innerTactic.Actualvalue }));
                }
            }

            return listmonthwise;
        }

        /// <summary>
        /// Get Projected Cost Data With Month Wise.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public List<TacticMonthValue> GetProjectedCostData(List<TacticStageValue> tacticDataList)
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

            return GetMonthWiseValueList(tacticdata);
        }

        /// <summary>
        /// Get Trend.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public List<Plan_Tactic_Values> GetTrendRevenueDataContribution(List<Plan_Campaign_Program_Tactic_Actual> planTacticList, int lastMonth, List<string> monthList, string revenue)
        {
            return planTacticList.Where(tactic => monthList.Contains(tactic.Period) && tactic.StageTitle == revenue).GroupBy(_tac => _tac.PlanTacticId).Select(pt => new Plan_Tactic_Values
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
        public double GetRevenueVSSpendContribution(List<TacticMonthValue> ActualDT, List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList, List<int> planTacticList, List<string> monthWithYearList, List<string> monthList, string revenue)
        {
            double costTotal = ActualDT.Where(actual => planTacticList.Contains(actual.Id) && monthWithYearList.Contains(actual.Month)).Sum(actual => actual.Value);
            double revenueTotal = ActualTacticList.Where(actualTac => planTacticList.Contains(actualTac.PlanTacticId) && monthList.Contains(actualTac.Period) && actualTac.StageTitle == revenue).ToList().Sum(actual => actual.Actualvalue);
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
            bool isCustomField = false;
            int customfieldId = 0;

            //Custom
            List<int> entityids = new List<int>();
            if (ParentLabel.Contains(Common.TacticCustomTitle))
            {
                customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.TacticCustomTitle, ""));
                isCustomField = true;
            }
            else if (ParentLabel.Contains(Common.CampaignCustomTitle))
            {
                customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.CampaignCustomTitle, ""));
                isCustomField = true;
            }
            else if (ParentLabel.Contains(Common.ProgramCustomTitle))
            {
                customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.ProgramCustomTitle, ""));
                isCustomField = true;
            }

            if (isCustomField)
            {
                string customFieldType = db.CustomFields.Where(custField => custField.CustomFieldId == customfieldId).Select(custField => custField.CustomFieldType.Name).FirstOrDefault();
                entityids = db.CustomField_Entity.Where(custField => custField.CustomFieldId == customfieldId && custField.Value == id).Select(custField => custField.EntityId).ToList();
            }

            Tacticdata = Tacticdata.Where(pcpt => ((ParentLabel == Common.RevenueBusinessUnit && pcpt.TacticObj.BusinessUnitId == new Guid(id)) ||
                                                                (ParentLabel == Common.RevenueAudience && pcpt.TacticObj.AudienceId == Convert.ToInt32(id)) ||
                                                                (ParentLabel == Common.RevenueGeography && pcpt.TacticObj.GeographyId == new Guid(id)) ||
                                                                (ParentLabel == Common.RevenueVertical && pcpt.TacticObj.VerticalId == Convert.ToInt32(id) ||
                                                                (ParentLabel == Common.RevenueOrganization) ||
                                                                (ParentLabel.Contains(Common.TacticCustomTitle) && entityids.Contains(pcpt.TacticObj.PlanTacticId))
                                                                || (ParentLabel.Contains(Common.CampaignCustomTitle) && entityids.Contains(pcpt.TacticObj.Plan_Campaign_Program.PlanCampaignId))
                                                                || (ParentLabel.Contains(Common.ProgramCustomTitle) && entityids.Contains(pcpt.TacticObj.PlanProgramId))
                                                                ))).ToList();

            if (Tacticdata.Count > 0)
            {
                string stageTitleRevenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
                List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                Tacticdata.ForEach(t => t.ActualTacticList.Where(pcpta => pcpta.StageTitle == stageTitleRevenue).ToList().ForEach(a => ActualTacticList.Add(a)));

                List<string> includeMonth = GetMonthListForReport(selectOption, true);
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

            //// Set month wise chart data.
            for (int month = 1; month <= 12; month++)
            {
                DataRow drActualRevenue = dtActualRevenue.Select(string.Format("{0}={1}", ColumnMonth, month)).FirstOrDefault();
                DataRow drProjectedRevenue = dtProjectedRevenue.Select(string.Format("{0}={1}", ColumnMonth, month)).FirstOrDefault();
                DataRow drContribution = dtContribution.Select(string.Format("{0}={1}", ColumnMonth, month)).FirstOrDefault();

                //// Set Actual Revenue value.
                double? actualRevenue = 0;
                if (!string.IsNullOrWhiteSpace(Convert.ToString(drActualRevenue[ColumnValue])))
                {
                    actualRevenue = (double)drActualRevenue[ColumnValue];
                }

                //// Set Projected Revenue value.
                double? projectedRevenue = 0;
                if (!string.IsNullOrWhiteSpace(Convert.ToString(drProjectedRevenue[ColumnValue])))
                {
                    projectedRevenue = (double)drProjectedRevenue[ColumnValue];
                }

                //// Set Contribution value.
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
            List<TacticDataTable> tacticdata = (from tactic in TacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.TacticObj.PlanTacticId,
                                                    Value = tactic.RevenueValue,
                                                    StartMonth = tactic.TacticObj.StartDate.AddDays(tactic.CWVelocity).Month,
                                                    EndMonth = tactic.TacticObj.EndDate.AddDays(tactic.CWVelocity).Month,
                                                    StartYear = tactic.TacticObj.StartDate.AddDays(tactic.CWVelocity).Year,
                                                    EndYear = tactic.TacticObj.EndDate.AddDays(tactic.CWVelocity).Year
                                                }).ToList();
            var trevenueProjected = GetMonthWiseValueList(tacticdata).Where(mr => monthList.Contains(mr.Month)).GroupBy(r => r.Month).Select(g => new
            {
                PKey = g.Key,
                PSum = g.Sum(r => r.Value)
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
                DataRow drAactualRevenue = dtActualRevenue.Select(string.Format("{0}={1}", ColumnMonth, Convert.ToInt32(planCampaignTacticActualMonth.Period.Replace(PeriodPrefix, "")))).FirstOrDefault();

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
            List<TacticMonthValue> ProjectedRevenueDataTable = GetProjectedRevenueValueDataTableForReport(Tacticdata);
            string Revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            //  List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpt => TacticIds.Contains(pcpt.PlanTacticId) && pcpt.StageTitle == Revenue && includeMonthUpCurrent.Contains(pcpt.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + pcpt.Period)).ToList();

            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));
            //Start : Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
            ActualTacticList = ActualTacticList.Where(mr => mr.StageTitle == Revenue && includeMonthUpCurrent.Contains((Tacticdata.FirstOrDefault(t => t.TacticObj.PlanTacticId == mr.PlanTacticId).TacticYear) + mr.Period)).ToList();

            //// Get Top 5 Business Unit data based on ClientId.
            var businessUnits = db.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId) && b.IsDeleted == false).ToList()
                                                .Select(b => new
                                                {
                                                    Title = b.Title,
                                                    ColorCode = string.Format("#{0}", b.ColorCode),
                                                    Value = GetActualVSPlannedRevenue(ActualTacticList, ProjectedRevenueDataTable, Tacticdata.Where(t => t.TacticObj.BusinessUnitId.Equals(b.BusinessUnitId)).Select(t => t.TacticObj.PlanTacticId).ToList(), includeMonthUpCurrent)
                                                }).OrderByDescending(b => b.Value).ThenBy(b => b.Title).Take(5);

            //// Get Top 5 Vertical data based on ClientId.
            var vertical = db.Verticals.Where(vert => vert.ClientId == Sessions.User.ClientId && vert.IsDeleted == false).ToList()
                                                .Select(vert => new
                                                {
                                                    Title = vert.Title,
                                                    ColorCode = string.Format("#{0}", vert.ColorCode),
                                                    Value = GetActualVSPlannedRevenue(ActualTacticList, ProjectedRevenueDataTable, Tacticdata.Where(tactic => tactic.TacticObj.VerticalId.Equals(vert.VerticalId)).Select(tactic => tactic.TacticObj.PlanTacticId).ToList(), includeMonthUpCurrent)
                                                }).OrderByDescending(vert => vert.Value).ThenBy(vert => vert.Title).Take(5);

            //// Get Top 5 Geography data based on ClientId.
            var geography = db.Geographies.Where(geo => geo.ClientId.Equals(Sessions.User.ClientId) && geo.IsDeleted == false).ToList()
                                                .Select(geo => new
                                                {
                                                    Title = geo.Title,
                                                    ColorCode = "#1627a0",
                                                    Value = GetActualVSPlannedRevenue(ActualTacticList, ProjectedRevenueDataTable, Tacticdata.Where(tactic => tactic.TacticObj.GeographyId.Equals(geo.GeographyId)).Select(tactic => tactic.TacticObj.PlanTacticId).ToList(), includeMonthUpCurrent)
                                                }).OrderByDescending(geo => geo.Value).ThenBy(geo => geo.Title).Take(5);
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
        private double GetActualVSPlannedRevenue(List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList, List<TacticMonthValue> ProjectedRevenueDataTable, List<int> tacticIds, List<string> includeMonthUpCurrent)
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
                double projectedRevenueValue = ProjectedRevenueDataTable.Where(mr => tacticIds.Contains(mr.Id) && includeMonthUpCurrent.Contains(mr.Month)).Sum(mr => mr.Value);
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
            try
            {
                ViewBag.ReportType = reportType;
                BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();

                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                ViewBag.IsServiceUnavailable = false;
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
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    ViewBag.IsServiceUnavailable = true;
                }
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
                            Notification notification = (Notification)db.Notifications.FirstOrDefault(notfctn => notfctn.NotificationInternalUseOnly.Equals(notificationShareReport));
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
            //// Start - Added Sohel Pathan on 30/12/2014 for and Internal Review Point
            if (reportType.Equals(Enums.ReportType.Summary.ToString()))
            {
                htmlOfCurrentView = htmlOfCurrentView.Replace("class=\"dollarFormat\"", "");
            }
            //// End - Added Sohel Pathan on 30/12/2014 for and Internal Review Point
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
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.price_format.1.8_v2.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.price_format.1.8.min.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/slimScrollHorizontal.js"));
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/jquery.selectbox-0.2.js"));

            /*Modified By Maninder Singh Wadhva on  10/17/2014 for ticket #865 	Custom fields & Report filter - Review changes on reports PDF*/
            html += string.Format("<script src='{0}'></script>", Server.MapPath("~/Scripts/js/jquery.multiselect_v1.js"));
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
            string planIds = string.Join(",", Sessions.ReportPlanIds.Select(plan => plan.ToString()).ToArray());
            List<int> TacticId = Common.GetTacticByPlanIDs(planIds);

            //// Set Viewby Dropdownlist.
            List<ViewByModel> lstViewByTab = new List<ViewByModel>();
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Plan.ToString(), Value = ReportTabType.Plan.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Vertical.ToString(), Value = ReportTabType.Vertical.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Geography.ToString(), Value = ReportTabType.Geography.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.BusinessUnit.ToString(), Value = ReportTabType.BusinessUnit.ToString() });
            lstViewByTab.Add(new ViewByModel { Text = Common.CustomLabelFor(Enums.CustomLabelCode.Audience), Value = ReportTabType.Audience.ToString() });

            ////Start - Modified by Mitesh Vaishnav for PL ticket #831
            var campaignProgramList = db.Plan_Campaign_Program_Tactic.Where(tactic => TacticId.Contains(tactic.PlanTacticId)).ToList();
            List<int> campaignlist = campaignProgramList.Select(campaign => campaign.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = campaignProgramList.Select(program => program.PlanProgramId).ToList();

            lstViewByTab = lstViewByTab.Where(modal => !string.IsNullOrEmpty(modal.Text)).OrderBy(modal => modal.Text, new AlphaNumericComparer()).ToList();
            var lstCustomFields = Common.GetCustomFields(TacticId, programlist, campaignlist);
            lstViewByTab = lstViewByTab.Concat(lstCustomFields).ToList();
            ////End - Modified by Mitesh Vaishnav for PL ticket #831
            ViewBag.ViewByTab = lstViewByTab;

            //// Set View By Allocated values.
            List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
            lstViewByAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarterly", Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstViewByAllocated = lstViewByAllocated.Where(modal => !string.IsNullOrEmpty(modal.Text)).OrderBy(modal => modal.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewByAllocated = lstViewByAllocated;

            //// Set Geography list.
            List<SelectListItem> lstGeography = new List<SelectListItem>();
            lstGeography = db.Geographies.Where(geo => geo.ClientId == Sessions.User.ClientId && geo.IsDeleted == false).ToList().Select(geo => new SelectListItem { Text = geo.Title, Value = geo.GeographyId.ToString(), Selected = true }).ToList();
            ViewBag.ViewGeography = lstGeography.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();

            //// Set BusinessUnit list.
            List<SelectListItem> lstBusinessUnit = new List<SelectListItem>();
            lstBusinessUnit = db.BusinessUnits.Where(bu => bu.ClientId == Sessions.User.ClientId && bu.IsDeleted == false).ToList().Select(bu => new SelectListItem { Text = bu.Title, Value = bu.BusinessUnitId.ToString(), Selected = true }).ToList();
            ViewBag.ViewBusinessUnit = lstBusinessUnit.Where(bu => !string.IsNullOrEmpty(bu.Text)).OrderBy(bu => bu.Text, new AlphaNumericComparer()).ToList();

            //// Set Vertical list.
            List<SelectListItem> lstVertical = new List<SelectListItem>();
            lstVertical = db.Verticals.Where(vert => vert.ClientId == Sessions.User.ClientId && vert.IsDeleted == false).ToList().Select(vert => new SelectListItem { Text = vert.Title, Value = vert.VerticalId.ToString(), Selected = true }).ToList();
            ViewBag.ViewVertical = lstVertical.Where(vert => !string.IsNullOrEmpty(vert.Text)).OrderBy(vert => vert.Text, new AlphaNumericComparer()).ToList();

            //// Set Audience list.
            List<SelectListItem> lstAudience = new List<SelectListItem>();
            lstAudience = db.Audiences.Where(aud => aud.ClientId == Sessions.User.ClientId && aud.IsDeleted == false).ToList().Select(aud => new SelectListItem { Text = aud.Title, Value = aud.AudienceId.ToString(), Selected = true }).ToList();
            ViewBag.ViewAudience = lstAudience.Where(aud => !string.IsNullOrEmpty(aud.Text)).OrderBy(aud => aud.Text, new AlphaNumericComparer()).ToList();

            //// Set Year list.
            List<SelectListItem> lstYear = new List<SelectListItem>();
            var lstPlan = db.Plans.Where(plan => plan.IsDeleted == false && plan.Status == PublishedPlan && plan.Model.ClientId == Sessions.User.ClientId).ToList();
            var yearlist = lstPlan.OrderBy(plan => plan.Year).Select(plan => plan.Year).Distinct().ToList();
            yearlist.ForEach(year => lstYear.Add(new SelectListItem { Text = "FY " + year, Value = year }));


            string defaultallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString();
            string Noneallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString();

            List<SelectListItem> lstPlanList = new List<SelectListItem>();

            lstPlanList = lstPlan.Where(plan => plan.Year == currentYear).Select(plan => new SelectListItem { Text = plan.Title + " - " + (plan.AllocatedBy == defaultallocatedby ? Noneallocatedby : plan.AllocatedBy), Value = plan.PlanId.ToString() + "_" + plan.AllocatedBy, Selected = (Sessions.ReportPlanIds.Contains(plan.PlanId) ? true : false) }).ToList();
            ViewBag.ViewPlan = lstPlanList.Where(plan => !string.IsNullOrEmpty(plan.Text)).OrderBy(plan => plan.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewYear = lstYear.Where(plan => !string.IsNullOrEmpty(plan.Text)).OrderBy(plan => plan.Text, new AlphaNumericComparer()).ToList();
            ViewBag.SelectedYear = currentYear;

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
            string defaultallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString();
            string Noneallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString();
            
            //// Set Plan list.
            var planList = db.Plans.Where(plan => plan.Year == Year && plan.IsDeleted == false && plan.Status == PublishedPlan && plan.Model.IsDeleted == false && plan.Model.ClientId == Sessions.User.ClientId).ToList().Select(plan => new
            {
                Text = plan.Title + " - " + (plan.AllocatedBy == defaultallocatedby ? Noneallocatedby : plan.AllocatedBy),
                Value = plan.PlanId.ToString() + "_" + plan.AllocatedBy
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
                Year = currentYear;
            }
            List<int> PlanIdList = new List<int>();
            if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count > 0)
            {
                PlanIdList = Sessions.ReportPlanIds;
            }

            //// Split AudienceIds comma separated string to list.
            List<int> AudienceIdList = new List<int>();
            if (AudienceIds.ToString() != string.Empty)
            {
                AudienceIdList = AudienceIds.Split(',').Select(int.Parse).ToList<int>();
            }

            //// Split VerticalIds comma separated string to list.
            List<int> VerticalIdList = new List<int>();
            if (VerticalIds.ToString() != string.Empty)
            {
                VerticalIdList = VerticalIds.Split(',').Select(int.Parse).ToList<int>();
            }

            //// Split BusinessUnitIds comma separated string to list.
            List<Guid> BusinessUnitIdList = new List<Guid>();
            if (Sessions.ReportBusinessUnitIds != null && Sessions.ReportBusinessUnitIds.Count > 0)
            {
                BusinessUnitIdList = Sessions.ReportBusinessUnitIds;
            }

            //// Split GeographyIds comma separated string to list.
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

            //// load Filter lists.
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
                    var planbudgetlist = db.Plan_Budget.Where(pb => PlanIdsInner.Contains(pb.PlanId)).ToList();
                    var campaignbudgetlist = db.Plan_Campaign_Budget.Where(pcb => CampaignIds.Contains(pcb.PlanCampaignBudgetId)).ToList();
                    var programbudgetlist = db.Plan_Campaign_Program_Budget.Where(pcpb => ProgramIds.Contains(pcpb.PlanProgramId)).ToList();
                    var tacticcostlist = db.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => TacticIds.Contains(pcptc.PlanTacticId)).ToList();
                    foreach (var p in planobj)
                    {
                        //// Add Plan data to BudgetModelReport.
                        obj = new BudgetModelReport();
                        obj.Id = p.PlanId.ToString();
                        obj.ActivityId = "plan_" + p.PlanId.ToString();
                        obj.ActivityName = p.Title;
                        obj.ActivityType = ActivityType.ActivityPlan;
                        obj.ParentActivityId = parentMainId;
                        obj.TabActivityId = p.PlanId.ToString();
                        obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Planned.ToString());
                        obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                        obj = GetMonthWiseDataReport(obj, planbudgetlist.Where(planbudget => planbudget.PlanId == p.PlanId).Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Allocated.ToString());
                        model.Add(obj);
                        parentPlanId = "plan_" + p.PlanId.ToString();

                        var campaignObj = CampaignList.Where(campaign => campaign.PlanId == p.PlanId).ToList();
                        foreach (var c in campaignObj)
                        {
                            //// Add Campagin data to BudgetModelReport.
                            obj = new BudgetModelReport();
                            obj.Id = c.PlanCampaignId.ToString();
                            obj.ActivityId = "c_" + c.PlanCampaignId.ToString();
                            obj.ActivityName = c.Title;
                            obj.ActivityType = ActivityType.ActivityCampaign;
                            obj.ParentActivityId = parentPlanId;
                            obj.TabActivityId = p.PlanId.ToString();
                            obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Planned.ToString());
                            obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                            obj = GetMonthWiseDataReport(obj, campaignbudgetlist.Where(pcb => pcb.PlanCampaignId == c.PlanCampaignId).Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Allocated.ToString());
                            model.Add(obj);
                            parentCampaignId = "c_" + c.PlanCampaignId.ToString();
                            var ProgramObj = ProgramList.Where(program => program.PlanCampaignId == c.PlanCampaignId).ToList();
                            foreach (var pr in ProgramObj)
                            {
                                //// Add Program data to BudgetModelReport.
                                obj = new BudgetModelReport();
                                obj.Id = pr.PlanProgramId.ToString();
                                obj.ActivityId = "cp_" + pr.PlanProgramId.ToString();
                                obj.ActivityName = pr.Title;
                                obj.ActivityType = ActivityType.ActivityProgram;
                                obj.ParentActivityId = parentCampaignId;
                                obj.TabActivityId = p.PlanId.ToString();
                                obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Planned.ToString());
                                obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                                obj = GetMonthWiseDataReport(obj, programbudgetlist.Where(pcpb => pcpb.PlanProgramId == pr.PlanProgramId).Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Allocated.ToString());
                                model.Add(obj);
                                parentProgramId = "cp_" + pr.PlanProgramId.ToString();

                                var TacticObj = tacticList.Where(tactic => tactic.PlanProgramId == pr.PlanProgramId).ToList();
                                foreach (var t in TacticObj)
                                {
                                    //// Add Tactic data to BudgetModelReport.
                                    obj = new BudgetModelReport();
                                    obj.Id = t.PlanTacticId.ToString();
                                    obj.ActivityId = "cpt_" + t.PlanTacticId.ToString();
                                    obj.ActivityName = t.Title;
                                    obj.ActivityType = ActivityType.ActivityTactic;
                                    obj.ParentActivityId = parentProgramId;
                                    obj.TabActivityId = p.PlanId.ToString();
                                    obj = GetMonthWiseDataReport(obj, tacticcostlist.Where(pcptc => pcptc.PlanTacticId == t.PlanTacticId).Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Planned.ToString());
                                    obj = AfterApprovedStatus.Contains(t.Status) ? GetMonthWiseDataReport(obj, t.Plan_Campaign_Program_Tactic_Actual.Where(b => b.StageTitle == "Cost").Select(b => new BudgetedValue { Period = b.Period, Value = b.Actualvalue }).ToList(), ReportColumnType.Actual.ToString()) : GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Actual.ToString());
                                    obj = GetMonthWiseDataReport(obj, EmptyBudgetList, ReportColumnType.Allocated.ToString());
                                    model.Add(obj);
                                    parentTacticId = "cpt_" + t.PlanTacticId.ToString();

                                    var LineItemObj = LineItemList.Where(line => line.PlanTacticId == t.PlanTacticId).ToList();
                                    foreach (var l in LineItemObj)
                                    {
                                        //// Add LineItem data to BudgetModelReport.
                                        obj = new BudgetModelReport();
                                        obj.Id = l.PlanLineItemId.ToString();
                                        obj.ActivityId = "cptl_" + l.PlanLineItemId.ToString();
                                        obj.ActivityName = l.Title;
                                        obj.LineItemTypeId = l.LineItemTypeId;
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
                else if (Tab.Contains(Common.TacticCustomTitle))
                {
                    customfieldId = Convert.ToInt32(Tab.Replace(Common.TacticCustomTitle, ""));
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
                        planobj = cusomfieldEntity.GroupBy(c => c.Value).Select(p => new BudgetReportTab
                        {
                            Id = p.Key,
                            Title = p.Key
                        }).Select(b => b).OrderBy(b => b.Title).ToList();
                    }

                }
                ////Start - Added by Mitesh Vaishnav for PL ticket #831
                else if (Tab.Contains(Common.ProgramCustomTitle))
                {
                    customfieldId = Convert.ToInt32(Tab.Replace(Common.ProgramCustomTitle, ""));
                    string customFieldType = db.CustomFields.Where(custm => custm.CustomFieldId == customfieldId).Select(custm => custm.CustomFieldType.Name).FirstOrDefault();

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
                        List<int> entityids = tacticList.Select(t => t.PlanProgramId).ToList();
                        var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();
                        planobj = cusomfieldEntity.GroupBy(c => c.Value).Select(p => new BudgetReportTab
                        {
                            Id = p.Key,
                            Title = p.Key
                        }).Select(b => b).OrderBy(b => b.Title).ToList();
                    }
                }
                else if (Tab.Contains(Common.CampaignCustomTitle))
                {
                    customfieldId = Convert.ToInt32(Tab.Replace(Common.CampaignCustomTitle, ""));
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
                        List<int> entityids = tacticList.Select(t => t.Plan_Campaign_Program.PlanCampaignId).ToList();
                        var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();
                        planobj = cusomfieldEntity.GroupBy(c => c.Value).Select(p => new BudgetReportTab
                        {
                            Id = p.Key,
                            Title = p.Key
                        }).Select(b => b).OrderBy(b => b.Title).ToList();
                    }
                }
                ////End - Added by Mitesh Vaishnav for PL ticket #831

                if (planobj != null)
                {
                    foreach (var p in planobj)
                    {
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
                        else if (Tab.Contains(Common.TacticCustomTitle))
                        {
                            var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && c.Value == p.Id).ToList();
                            List<int> entityids = cusomfieldEntity.Select(e => e.EntityId).ToList();
                            TacticListInner = tacticList.Where(tactic => entityids.Contains(tactic.PlanTacticId)).ToList();
                            p.Id = p.Id.Replace(' ', '_').Replace('#', '_').Replace('-', '_');
                        }
                        ////Start - Added by Mitesh Vaishnav for PL ticket #831
                        else if (Tab.Contains(Common.ProgramCustomTitle))
                        {
                            var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && c.Value == p.Id).ToList();
                            List<int> entityids = cusomfieldEntity.Select(e => e.EntityId).ToList();
                            TacticListInner = tacticList.Where(tactic => entityids.Contains(tactic.PlanProgramId)).ToList();
                            p.Id = p.Id.Replace(' ', '_').Replace('#', '_').Replace('-', '_');
                        }
                        else if (Tab.Contains(Common.CampaignCustomTitle))
                        {
                            var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && c.Value == p.Id).ToList();
                            List<int> entityids = cusomfieldEntity.Select(e => e.EntityId).ToList();
                            TacticListInner = tacticList.Where(tactic => entityids.Contains(tactic.Plan_Campaign_Program.PlanCampaignId)).ToList();
                            p.Id = p.Id.Replace(' ', '_').Replace('#', '_').Replace('-', '_');
                        }
                        ////End - Added by Mitesh Vaishnav for PL ticket #831

                        //// Add Plan data to BudgetModelReport.
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

                        var ProgramListInner = ProgramList.Where(program => TacticListInner.Select(t => t.PlanProgramId).Contains(program.PlanProgramId)).ToList();
                        var campaignObj = CampaignList.Where(campaign => ProgramListInner.Select(program => program.PlanCampaignId).Contains(campaign.PlanCampaignId)).ToList();

                        foreach (var c in campaignObj)
                        {
                            //// Add Campaign data to BudgetModelReport.
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
                                //// Add Program data to BudgetModelReport.
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
                                    //// Add Tactic data to BudgetModelReport.
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
                                        //// Add LineItem data to BudgetModelReport.
                                        obj = new BudgetModelReport();
                                        obj.Id = l.PlanLineItemId.ToString();
                                        obj.ActivityId = "cptl_" + l.PlanLineItemId.ToString();
                                        obj.ActivityName = l.Title;
                                        obj.LineItemTypeId = l.LineItemTypeId;
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

            model = ManageLineItems(model);


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
                month.Period = PeriodPrefix + i;
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
            //// Get monthly budget values.
            BudgetMonth month = new BudgetMonth();
            month.Jan = lst.Where(budgt => budgt.Period.ToUpper() == Jan).Select(budgt => budgt.Value).FirstOrDefault();
            month.Feb = lst.Where(budgt => budgt.Period.ToUpper() == Feb).Select(budgt => budgt.Value).FirstOrDefault();
            month.Mar = lst.Where(budgt => budgt.Period.ToUpper() == Mar).Select(budgt => budgt.Value).FirstOrDefault();
            month.Apr = lst.Where(budgt => budgt.Period.ToUpper() == Apr).Select(budgt => budgt.Value).FirstOrDefault();
            month.May = lst.Where(budgt => budgt.Period.ToUpper() == May).Select(budgt => budgt.Value).FirstOrDefault();
            month.Jun = lst.Where(budgt => budgt.Period.ToUpper() == Jun).Select(budgt => budgt.Value).FirstOrDefault();
            month.Jul = lst.Where(budgt => budgt.Period.ToUpper() == Jul).Select(budgt => budgt.Value).FirstOrDefault();
            month.Aug = lst.Where(budgt => budgt.Period.ToUpper() == Aug).Select(budgt => budgt.Value).FirstOrDefault();
            month.Sep = lst.Where(budgt => budgt.Period.ToUpper() == Sep).Select(budgt => budgt.Value).FirstOrDefault();
            month.Oct = lst.Where(budgt => budgt.Period.ToUpper() == Oct).Select(budgt => budgt.Value).FirstOrDefault();
            month.Nov = lst.Where(budgt => budgt.Period.ToUpper() == Nov).Select(budgt => budgt.Value).FirstOrDefault();
            month.Dec = lst.Where(budgt => budgt.Period.ToUpper() == Dec).Select(budgt => budgt.Value).FirstOrDefault();

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
                        //// Set parent line values.
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

                        //// Set parent Actual line values.
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
                    //// Set Parent line values.
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

                    //// Set parent Actual line values.
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

                    //// Set parent monthly allocated line values.
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
            foreach (BudgetModelReport _budgModel in model.Where(mdl => mdl.ActivityType == ActivityType.ActivityTactic))
            {
                BudgetMonth lineDiffPlanned = new BudgetMonth();
                List<BudgetModelReport> lines = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == _budgModel.ActivityId).ToList();
                BudgetModelReport otherLine = lines.Where(ol => ol.ActivityName == Common.DefaultLineItemTitle && ol.LineItemTypeId == null).SingleOrDefault();
                lines = lines.Where(ol => ol.ActivityName != Common.DefaultLineItemTitle).ToList();
                if (otherLine != null)
                {
                    //// Set monthly line Items values.
                    if (lines.Count > 0)
                    {
                        lineDiffPlanned.Jan = _budgModel.MonthPlanned.Jan - lines.Sum(lmon => (double?)lmon.MonthPlanned.Jan) ?? 0;
                        lineDiffPlanned.Feb = _budgModel.MonthPlanned.Feb - lines.Sum(lmon => (double?)lmon.MonthPlanned.Feb) ?? 0;
                        lineDiffPlanned.Mar = _budgModel.MonthPlanned.Mar - lines.Sum(lmon => (double?)lmon.MonthPlanned.Mar) ?? 0;
                        lineDiffPlanned.Apr = _budgModel.MonthPlanned.Apr - lines.Sum(lmon => (double?)lmon.MonthPlanned.Apr) ?? 0;
                        lineDiffPlanned.May = _budgModel.MonthPlanned.May - lines.Sum(lmon => (double?)lmon.MonthPlanned.May) ?? 0;
                        lineDiffPlanned.Jun = _budgModel.MonthPlanned.Jun - lines.Sum(lmon => (double?)lmon.MonthPlanned.Jun) ?? 0;
                        lineDiffPlanned.Jul = _budgModel.MonthPlanned.Jul - lines.Sum(lmon => (double?)lmon.MonthPlanned.Jul) ?? 0;
                        lineDiffPlanned.Aug = _budgModel.MonthPlanned.Aug - lines.Sum(lmon => (double?)lmon.MonthPlanned.Aug) ?? 0;
                        lineDiffPlanned.Sep = _budgModel.MonthPlanned.Sep - lines.Sum(lmon => (double?)lmon.MonthPlanned.Sep) ?? 0;
                        lineDiffPlanned.Oct = _budgModel.MonthPlanned.Oct - lines.Sum(lmon => (double?)lmon.MonthPlanned.Oct) ?? 0;
                        lineDiffPlanned.Nov = _budgModel.MonthPlanned.Nov - lines.Sum(lmon => (double?)lmon.MonthPlanned.Nov) ?? 0;
                        lineDiffPlanned.Dec = _budgModel.MonthPlanned.Dec - lines.Sum(lmon => (double?)lmon.MonthPlanned.Dec) ?? 0;

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

                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == _budgModel.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().MonthPlanned = lineDiffPlanned;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == _budgModel.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().ParentMonthPlanned = lineDiffPlanned;

                        double planned = _budgModel.Planned - lines.Sum(l1 => l1.Planned);
                        planned = planned < 0 ? 0 : planned;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == _budgModel.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().Planned = planned;
                    }
                    else
                    {
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == _budgModel.ActivityId && line.ActivityName == Common.DefaultLineItemTitle && line.LineItemTypeId == null).SingleOrDefault().MonthPlanned = _budgModel.MonthPlanned;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == _budgModel.ActivityId && line.ActivityName == Common.DefaultLineItemTitle && line.LineItemTypeId == null).SingleOrDefault().ParentMonthPlanned = _budgModel.MonthPlanned;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == _budgModel.ActivityId && line.ActivityName == Common.DefaultLineItemTitle && line.LineItemTypeId == null).SingleOrDefault().Planned = _budgModel.Planned < 0 ? 0 : _budgModel.Planned;
                    }
                }
            }
            return model;
        }

        #endregion

    }
}

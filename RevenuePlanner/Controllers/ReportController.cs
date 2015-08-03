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
using System.Web.Script.Serialization;
using System.Data.Objects.SqlClient;
using RevenuePlanner.BDSService; 
using RevenuePlanner.Helpers;
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
        bool isPublishedPlanExist = false;
        private string abovePlan = "Above Plan";
        private string belowPlan = "Below Plan";
        private string strPercentage = "%";
        private string strCurrency = "$";
        private const string strPlannedCost = "Planned Cost";
        private const string strActualCost = "Actual Cost";
        private const string strBudget = "Budget";
        private const double _constQuartPlotBandPadding = 0.4;
        private const double _lastQuarterValue = 4;
        private const double _PlotBandToValue = 5;
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
            Sessions.ReportPlanIds = new List<int>();

            //// Set Report PlanId in Session.
            if (Sessions.PlanId > 0)
            {
                Sessions.ReportPlanIds.Add(Sessions.PlanId);
            }
 				else
                {
                    // Get Plan Id if Session Plan id not exist : Added By Bhavesh : Report Code ereview
                    string published = Convert.ToString(Enums.PlanStatus.Published);
                    string year = Convert.ToString(DateTime.Now.Year);
                    int planid = db.Plans.Where(plan => plan.Status == published && !plan.IsDeleted && plan.Year == year).FirstOrDefault().PlanId;
                    Sessions.ReportPlanIds.Add(planid);
                }
            //// Modified by Arpita Soni for Ticket #1148 on 01/23/2015
            //// List of Tab - Parent
            List<ViewByModel> lstViewByTab = new List<ViewByModel>();
            lstViewByTab.Add(new ViewByModel { Text = ReportTabTypeText.Plan.ToString(), Value = ReportTabType.Plan.ToString() });
            lstViewByTab = lstViewByTab.Where(sort => !string.IsNullOrEmpty(sort.Text)).ToList();
            ViewBag.ViewByTab = lstViewByTab;

            //// List of Allocated Value
            List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
            lstViewByAllocated.Add(new ViewByModel { Text = Enums.ViewByAllocated.Monthly.ToString(), Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = Enums.ViewByAllocated.Quarterly.ToString(), Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstViewByAllocated = lstViewByAllocated.Where(sort => !string.IsNullOrEmpty(sort.Text)).ToList();
            ViewBag.ViewByAllocated = lstViewByAllocated;

            //// Start - Added by Arpita Soni for Ticket #1148 on 01/23/2015
            //// Custom Field Value
            List<CustomField> lstCustomFields = new List<CustomField>();
            string tactic = Enums.EntityType.Tactic.ToString();

                // Get Custom Field Type Id
                string customFieldType = Convert.ToString(Enums.CustomFieldType.DropDownList);
                int customFieldTypeId = db.CustomFieldTypes.Where(type => type.Name == customFieldType).FirstOrDefault().CustomFieldTypeId;

            lstCustomFields = db.CustomFields.Where(customfield => customfield.ClientId == Sessions.User.ClientId &&
                customfield.EntityType == tactic &&
                customfield.IsRequired == true &&
                customfield.CustomFieldTypeId == customFieldTypeId &&
                customfield.IsDisplayForFilter == true &&
                customfield.IsDeleted == false).ToList();

            lstCustomFields = lstCustomFields.Where(sort => !string.IsNullOrEmpty(sort.Name)).OrderBy(sort => sort.Name, new AlphaNumericComparer()).ToList();
            List<int> ids = lstCustomFields.Select(c => c.CustomFieldId).ToList();
            List<CustomFieldOption> tblCustomFieldOption = db.CustomFieldOptions.Where(_option => ids.Contains(_option.CustomFieldId) && _option.IsDeleted == false).ToList();
            //// Filter Custom Fields having no options
            var lstCustomFieldIds = tblCustomFieldOption.Select(customfieldid => customfieldid.CustomFieldId).Distinct();
            lstCustomFields = lstCustomFields.Where(c => lstCustomFieldIds.Contains(c.CustomFieldId)).ToList();

            ViewBag.ViewCustomFields = lstCustomFields;
            ViewBag.ViewCustomFieldOptions = tblCustomFieldOption.Where(_option => lstCustomFieldIds.Contains(_option.CustomFieldId)).ToList();
            //// End - Added by Arpita Soni for Ticket #1148 on 01/23/2015			

            //// Get Plan List
            List<SelectListItem> lstYear = new List<SelectListItem>();
            List<Plan> tblPlan = db.Plans.Where(plan => plan.IsDeleted == false).ToList();
            var lstPlan = tblPlan.Where(plan => plan.Status == PublishedPlan && plan.Model.ClientId == Sessions.User.ClientId && plan.Model.IsDeleted == false && plan.IsActive == true).ToList();
            if (lstPlan.Count == 0)
            {
                TempData["ErrorMessage"] = Common.objCached.NoPublishPlanAvailableOnReport;
                return RedirectToAction("PlanSelector", "Plan");
            }
            List<SelectListItem> lstPlanList = new List<SelectListItem>();
            string defaultallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString();
            string Noneallocatedby = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString();

            // Start - Added by Arpita Soni for Ticket #1148 on 02/02/2015
            // to make default selected plan from session planId
            var selectedYear = tblPlan.Where(plan => plan.PlanId == Sessions.PlanId).Select(plan => plan.Year).FirstOrDefault();
            lstPlanList = lstPlan.Where(plan => plan.Year == selectedYear).Select(plan => new SelectListItem { Text = plan.Title + " - " + (plan.AllocatedBy == defaultallocatedby ? Noneallocatedby : plan.AllocatedBy), Value = plan.PlanId.ToString() + "_" + plan.AllocatedBy, Selected = (plan.PlanId == Sessions.PlanId ? true : false) }).ToList();
            ViewBag.SelectedYear = selectedYear;
            // End - Added by Arpita Soni for Ticket #1148 on 02/02/2015

            var yearlist = lstPlan.OrderBy(plan => plan.Year).Select(plan => plan.Year).Distinct().ToList();
            SelectListItem objYear = new SelectListItem();
            foreach (string year in yearlist)
            {
                objYear = new SelectListItem();

                objYear.Text = "FY " + year;

                objYear.Value = year;
                objYear.Selected = year == selectedYear ? true : false;
                lstYear.Add(objYear);
            }
           // SelectListItem thisQuarter = new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thisquarter.ToString()].ToString(), Value = Enums.UpcomingActivities.thisquarter.ToString() };
           // lstYear.Add(thisQuarter);


            ViewBag.ViewPlan = lstPlanList.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewYear = lstYear.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
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

            //// get tactic list
            List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting();
            //// Calculate Value for ecah tactic
            List<TacticStageValue> Tacticdata = Common.GetTacticStageRelation(tacticlist, IsReport: true);
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
            string cwSatge = Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString();
            string mqlstage = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();

            // Start - Added by Arpita Soni for Ticket #1148 on 01/30/2015
            // To avoid summary display when no published plan selected (It displays no data found message.)
            foreach (var planId in Sessions.ReportPlanIds)
            {
                if (Common.IsPlanPublished(planId))
                {
                    isPublishedPlanExist = true;
                    break;
                }
            }
            // End - Added by Arpita Soni for Ticket #1148 on 01/30/2015

            //// check planids selected or not
            if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count > 0 && isPublishedPlanExist)
            {
                //// set viewbag to display plan or msg
                ViewBag.IsPlanExistToShowReport = true;
            }

            //// Get Tactic List for actual value
            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(tactic => tactic.ActualTacticList.ForEach(actualtactic => ActualTacticList.Add(actualtactic)));

            //// get MQL Actual value

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
            int INQStageId = db.Stages.FirstOrDefault(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false && stage.Code == inq && stage.IsDeleted == false).StageId;

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
                chartStage.Stage = Enums.InspectStageValues[Enums.InspectStage.INQ.ToString()].ToString();
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
                chartStage.Stage = mqlstage;
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
                chartStage.Stage = cwSatge;
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
        /// set session for multiple selected plans and custom fields
        /// </summary>
        /// <param name="planIds">Comma separated string which contains plan's Ids</param>
        /// <returns>If success than return status 1 else 0</returns>
        public JsonResult SetReportData(string planIds, string customIds, string OwnerIDs, string TactictypeIDs)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            CustomFieldFilter[] arrCustomFieldFilter = js.Deserialize<CustomFieldFilter[]>(customIds);
            try
            {
                //// Modified by Arpita Soni for Ticket #1148 on 01/23/2015
                List<int> lstPlanIds = new List<int>();
                List<string> lstOwnerIds = new List<string>();
                List<int> lstTactictypeIds = new List<int>();
                if (arrCustomFieldFilter.Count() > 0)
                {
                    Sessions.ReportCustomFieldIds = arrCustomFieldFilter;
                }
                else
                {
                    Sessions.ReportCustomFieldIds = null;
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

                //Added By Komal Rawal
                if (OwnerIDs != string.Empty)
                {
                    string[] arrOwnerIds = OwnerIDs.Split(',');
                    lstOwnerIds = arrOwnerIds.ToList();

                    if (lstOwnerIds.Count > 0)
                    {
                        Sessions.ReportOwnerIds = lstOwnerIds;

                    }

                }
                else
                {
                    Sessions.ReportOwnerIds = lstOwnerIds;
                }

                //Tactictype list
                if (TactictypeIDs != string.Empty)
                {
                    string[] arrTactictypeIds = TactictypeIDs.Split(',');
                    foreach (string TId in arrTactictypeIds)
                    {
                        int TacticId;
                        if (int.TryParse(TId, out TacticId))
                        {
                            lstTactictypeIds.Add(TacticId);
                        }
                    }
                    if (lstTactictypeIds.Count > 0)
                    {
                        Sessions.ReportTacticTypeIds = lstTactictypeIds;

                    }
                }
                else
                {
                    Sessions.ReportTacticTypeIds = lstTactictypeIds;
                }
                //End
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
            public int StartMonth { get; set; }
            public int EndMonth { get; set; }
            public int StartYear { get; set; }
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
                        listTacticMonthValue.Add(new TacticMonthValue { Id = tactic.TacticId, Month = tactic.StartYear + PeriodPrefix + tactic.StartMonth, Value = tactic.Value, StartMonth = tactic.StartMonth, EndMonth = tactic.EndMonth, StartYear = tactic.StartYear });
                    }
                    else
                    {
                        int totalMonth = (tactic.EndMonth - tactic.StartMonth) + 1;
                        double totalValue = (double)tactic.Value / (double)totalMonth;
                        for (var i = tactic.StartMonth; i <= tactic.EndMonth; i++)
                        {
                            listTacticMonthValue.Add(new TacticMonthValue { Id = tactic.TacticId, Month = tactic.StartYear.ToString() + PeriodPrefix + i, Value = totalValue, StartMonth = tactic.StartMonth, EndMonth = tactic.EndMonth, StartYear = tactic.StartYear });
                        }
                    }
                }
                else
                {
                    int totalMonth = (12 - tactic.StartMonth) + tactic.EndMonth + 1;
                    double totalValue = (double)tactic.Value / (double)totalMonth;
                    for (var i = tactic.StartMonth; i <= 12; i++)
                    {
                        listTacticMonthValue.Add(new TacticMonthValue { Id = tactic.TacticId, Month = tactic.StartYear.ToString() + PeriodPrefix + i, Value = totalValue, StartMonth = tactic.StartMonth, EndMonth = tactic.EndMonth, StartYear = tactic.StartYear });
                    }
                    for (var i = 1; i <= tactic.EndMonth + 1; i++)
                    {
                        listTacticMonthValue.Add(new TacticMonthValue { Id = tactic.TacticId, Month = tactic.EndYear.ToString() + PeriodPrefix + i, Value = totalValue, StartMonth = tactic.StartMonth, EndMonth = tactic.EndMonth, StartYear = tactic.StartYear });
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
        private List<Plan_Campaign_Program_Tactic> GetTacticForReporting(bool isBugdet = false)
        {
            //// Getting current year's all published plan for all custom fields of clientid of director.
            List<Plan_Campaign_Program_Tactic> tacticList = new List<Plan_Campaign_Program_Tactic>();
            List<int> planIds = new List<int>();
            List<Guid> ownerIds = new List<Guid>();
            List<int> TactictypeIds = new List<int>();
            if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count > 0)
            {
                planIds = Sessions.ReportPlanIds;
            }

            //// Get Tactic list.


            if (isBugdet)
            {
                tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted == false &&
                                                              planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)
                                                              ).ToList();
            }
            else
            {
                List<string> tacticStatus = Common.GetStatusListAfterApproved();
                tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted == false &&
                                                                  tacticStatus.Contains(tactic.Status) &&
                                                                  planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)
                                                                  ).ToList();
            }

            //Added by Komal Rawal
            if (Sessions.ReportOwnerIds != null && Sessions.ReportOwnerIds.Count > 0)
            {
                ownerIds = Sessions.ReportOwnerIds.Select(owner => new Guid(owner)).ToList();
                tacticList = tacticList.Where(tactic => ownerIds.Contains(tactic.CreatedBy)
                                                              ).ToList();
            }


            if (Sessions.ReportTacticTypeIds != null && Sessions.ReportTacticTypeIds.Count > 0)
            {
                TactictypeIds = Sessions.ReportTacticTypeIds;
                tacticList = tacticList.Where(tactic => TactictypeIds.Contains(tactic.TacticTypeId)
                                                              ).ToList();

            }
            //End

            if (Sessions.ReportCustomFieldIds != null && Sessions.ReportCustomFieldIds.Count() > 0)
            {
                List<int> tacticids = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
                List<CustomFieldFilter> lstCustomFieldFilter = Sessions.ReportCustomFieldIds.ToList();
                tacticids = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, tacticids);
                tacticList = tacticList.Where(tactic => tacticids.Contains(tactic.PlanTacticId)).ToList();
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

        #region "Get Projected DataTable Methods for StageCode Revenue,MQL,CW and INQ"
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
        /// Get Projected Stage Data & Monthly Calculation.
        /// </summary>
        /// <param name="planTacticList"></param>
        /// <returns></returns>
        public List<TacticMonthValue> GetProjectedDatabyStageCode(int CustomFieldId, string CustomFieldOptionId, string CustomFieldType, Enums.InspectStage stagecode, List<TacticStageValue> planTacticList, bool IsTacticCustomField)
        {
            //// Get tactic projected Stage data from planTacticlist.
            List<TacticDataTable> tacticdata = GetTacticDataTablebyStageCode(CustomFieldId, CustomFieldOptionId, CustomFieldType, stagecode, planTacticList, IsTacticCustomField);
            return GetMonthWiseValueList(tacticdata);
        }
        #endregion

        /// <summary>
        /// Get Projected Revenue Data With Month Wise.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public List<TacticMonthValue> GetProjectedRevenueDataWithVelocity(List<TacticStageValue> planTacticList)
        {
            //// Get tactic INQ with Velocity data from planTacticlist.
            List<TacticDataTable> tacticdata = (from tactic in planTacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.TacticObj.PlanTacticId,
                                                    Value = tactic.RevenueValue,
                                                    StartMonth = tactic.TacticObj.StartDate.AddDays(tactic.CWVelocity).Month,
                                                    EndMonth = tactic.TacticObj.EndDate.AddDays(tactic.CWVelocity).Month,
                                                    StartYear = tactic.TacticObj.StartDate.AddDays(tactic.CWVelocity).Year,
                                                    EndYear = tactic.TacticObj.EndDate.AddDays(tactic.CWVelocity).Year
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
        /// Get Projected Revenue Data With Month Wise.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public List<TacticMonthValue> GetProjectedCWDataWithVelocity(List<TacticStageValue> planTacticList)
        {
            //// Get tactic INQ with Velocity data from planTacticlist.
            List<TacticDataTable> tacticdata = (from tactic in planTacticList
                                                select new TacticDataTable
                                                {
                                                    TacticId = tactic.TacticObj.PlanTacticId,
                                                    Value = tactic.CWValue,
                                                    StartMonth = tactic.TacticObj.StartDate.AddDays(tactic.CWVelocity).Month,
                                                    EndMonth = tactic.TacticObj.EndDate.AddDays(tactic.CWVelocity).Month,
                                                    StartYear = tactic.TacticObj.StartDate.AddDays(tactic.CWVelocity).Year,
                                                    EndYear = tactic.TacticObj.EndDate.AddDays(tactic.CWVelocity).Year
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
        public JsonResult GetChildLabelData(string ParentLabel, string selectOption = "", bool IsAllInclude = false)
        {
            List<ViewByModel> _ChildDataList = new List<ViewByModel>();
            if (IsAllInclude)
                _ChildDataList.Add(new ViewByModel { Text = "All", Value = "0" });
            _ChildDataList = _ChildDataList.Concat(GetChildLabelDataViewByModel(ParentLabel, selectOption)).ToList();
            return Json(_ChildDataList, JsonRequestBehavior.AllowGet);
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

            if (ParentLabel == Common.RevenuePlans)
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
                        var optionlist = db.CustomFieldOptions.Where(customfieldoption => customfieldoption.CustomFieldId == customfieldId && customfieldoption.IsDeleted == false).ToList();
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

            return Json(new { ProjectedRevenueValue = projectedRevenue, ActualRevenueValue = actualRevenue, ProjectedMQLValue = Math.Round((double)projectedMQL, 0, MidpointRounding.AwayFromZero), ActualMQLValue = Math.Round(actualMQL) });
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
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            TempData["ReportData"] = tacticStageList;

            // Fetch the Custom field data based upon id's .
            // Modified by : #960 Kalpesh Sharma : Combine the method by passing one extra parameter.

            //// conversion summary view by dropdown
            List<ViewByModel> lstParentConversionSummery = new List<ViewByModel>();
            //Concat the Campaign and Program custom fields data with exsiting one. 
            var lstCustomFields = Common.GetCustomFields(tacticlist.Select(tactic => tactic.PlanTacticId).ToList(), programlist, campaignlist);
            lstParentConversionSummery = lstParentConversionSummery.Concat(lstCustomFields).ToList();
            ViewBag.parentConvertionSummery = lstParentConversionSummery;
            // Get child tab list
            if (lstParentConversionSummery.Count > 0)
                ViewBag.ChildTabListConvertionSummary = GetChildLabelDataViewByModel(lstParentConversionSummery.First().Value, timeFrameOption);
            else
                ViewBag.ChildTabListConvertionSummary = "";
            //// conversion performance view by dropdown
            List<ViewByModel> lstParentConversionPerformance = new List<ViewByModel>();
            lstParentConversionPerformance.Add(new ViewByModel { Text = Common.Plan, Value = Common.Plan });
            lstParentConversionPerformance.Add(new ViewByModel { Text = Common.Trend, Value = Common.Trend });
            lstParentConversionPerformance.Add(new ViewByModel { Text = Common.Actuals, Value = Common.Actuals });
            lstParentConversionPerformance = lstParentConversionPerformance.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            ViewBag.parentConvertionPerformance = lstParentConversionPerformance;

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
            bool IsTacticCustomField = false;
            //// get data from tempdata variable
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            //// load tacticdata based on ParentTab.
            int customfieldId = 0;
            if (ParentTab.Contains(Common.TacticCustomTitle))
            {
                IsTacticCustomField = true;
                customfieldId = Convert.ToInt32(ParentTab.Replace(Common.TacticCustomTitle, ""));
            }
            else if (ParentTab.Contains(Common.CampaignCustomTitle))
            {
                customfieldId = Convert.ToInt32(ParentTab.Replace(Common.CampaignCustomTitle, ""));
            }
            else if (ParentTab.Contains(Common.ProgramCustomTitle))
            {
                customfieldId = Convert.ToInt32(ParentTab.Replace(Common.ProgramCustomTitle, ""));
            }
            //// Calculate MQL & INQ data.
            string inq = Enums.Stage.INQ.ToString();
            string mql = Enums.Stage.MQL.ToString();
            string CustomfieldType = string.Empty;
            int INQStageId = db.Stages.FirstOrDefault(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false && stage.Code == inq && stage.IsDeleted == false).StageId;
            string strINQStage = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
            if (Tacticdata.Count() > 0)
            {
                CustomfieldType = db.CustomFields.Where(customfield => customfield.CustomFieldId.Equals(customfieldId)).Select(customfield => customfield.CustomFieldType.Name).FirstOrDefault();

                List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = new List<Plan_Campaign_Program_Tactic_Actual>();
                Tacticdata.ForEach(tacticactual => tacticactual.ActualTacticList.ForEach(actual => planTacticActual.Add(actual)));
                planTacticActual = planTacticActual.Where(tacticactual => includeMonth.Contains((Tacticdata.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == tacticactual.PlanTacticId).TacticYear) + tacticactual.Period)).ToList();

                List<TacticMonthValue> ProjectedMQLDataTable = GetProjectedDatabyStageCode(customfieldId, Id, CustomfieldType, Enums.InspectStage.MQL, Tacticdata, IsTacticCustomField);
                List<TacticMonthValue> ProjectedINQDataTable = GetProjectedDatabyStageCode(customfieldId, Id, CustomfieldType, Enums.InspectStage.INQ, Tacticdata, IsTacticCustomField);
                List<ActualDataTable> ActualINQDataTable = GetActualTacticDataTablebyStageCode(customfieldId, Id, CustomfieldType, Enums.InspectStage.INQ, planTacticActual.Where(act => act.StageTitle.Equals(strINQStage)).ToList(), Tacticdata, IsTacticCustomField);
                List<ActualDataTable> ActualMQLDataTable = GetActualTacticDataTablebyStageCode(customfieldId, Id, CustomfieldType, Enums.InspectStage.MQL, planTacticActual.Where(act => act.StageTitle.Equals(mql)).ToList(), Tacticdata, IsTacticCustomField);
                var rdata = new[] { new { 
                INQGoal = ProjectedINQDataTable.Where(tactictable => includeMonth.Contains(tactictable.Month)).GroupBy(tactictable => tactictable.Month).Select(tactictable => new
                {
                    PKey = tactictable.Key,
                    PSum = tactictable.Sum(tactic => tactic.Value)
                }),
                monthList = includeMonth,
                INQActual = ActualINQDataTable.GroupBy(tactictable => Tacticdata.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == tactictable.PlanTacticId).TacticYear + tactictable.Period)
                                             .Select(tactictable => new
                                              {
                                                PKey = tactictable.Key,
                                                PSum = tactictable.Sum(tactic => tactic.ActualValue)
                                              }),
                MQLGoal = ProjectedMQLDataTable.Where(tactictable => includeMonth.Contains(tactictable.Month)).GroupBy(tactictable => tactictable.Month).Select(tactictable => new
                {
                    PKey = tactictable.Key,
                    PSum = tactictable.Sum(tactic => tactic.Value)
                }),
                MQLActual = ActualMQLDataTable
                            .GroupBy(pt => Tacticdata.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == pt.PlanTacticId).TacticYear + pt.Period)
                            .Select(pcptj => new
                            {
                                PKey = pcptj.Key,
                                PSum = pcptj.Sum(pt => pt.ActualValue)
                            })
            }  };

                return Json(rdata, JsonRequestBehavior.AllowGet);
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);
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

            //// Start - Added by Arpita Soni for Ticket #1148 on 01/27/2015
            string tactic = Enums.EntityType.Tactic.ToString();
            string dropdownType = Enums.CustomFieldType.DropDownList.ToString();
            int dropdwnCustomFieldTypeId = db.CustomFieldTypes.Where(custType => custType.Name.Equals(dropdownType)).Select(custType => custType.CustomFieldTypeId).FirstOrDefault();

            // Get first 3 custom fields
            var customFields = db.CustomFields.Where(c => c.ClientId.Equals(Sessions.User.ClientId)
                && c.IsDeleted == false && c.IsRequired == true && c.IsDefault == true && c.EntityType == tactic && c.CustomFieldTypeId == dropdwnCustomFieldTypeId
                ).Select(c => new
                {
                    CustomFieldId = c.CustomFieldId,
                    CustomFieldName = c.Name
                }).Take(3).ToList();

            customFields = customFields.Where(sort => !string.IsNullOrEmpty(sort.CustomFieldName)).OrderBy(sort => sort.CustomFieldName, new AlphaNumericComparer()).ToList();
            List<ListSourcePerformanceData> lstListSourcePerformance = new List<ListSourcePerformanceData>();
            List<string> lstCustomFieldNames = new List<string>();
            List<CustomFieldOption> tblCustomFieldOption = db.CustomFieldOptions.ToList().Where(co => customFields.Select(custm => custm.CustomFieldId).Contains(co.CustomFieldId) && co.IsDeleted == false).ToList();

            // Applying custom field filters
            foreach (var customfield in customFields)
            {
                List<SourcePerformanceData> lstSourcePerformance = new List<SourcePerformanceData>();

                lstSourcePerformance = tblCustomFieldOption.Where(s => s.CustomFieldId == customfield.CustomFieldId).ToList().Select(s => new SourcePerformanceData
                {
                    Title = s.Value,
                    ColorCode = string.Format("#{0}", s.ColorCode),
                    Value = GetTrendActualTacticData(lastMonth, planTacticActual, customfield.CustomFieldId, s.CustomFieldOptionId.ToString(), dropdownType, Tacticdata, true) //tacticTrendCustomField.Any(cf => Convert.ToInt32(cf.CustomFieldOptionId) == s.CustomFieldOptionId) ? tacticTrendCustomField.Where(cf => Convert.ToInt32(cf.CustomFieldOptionId) == s.CustomFieldOptionId).FirstOrDefault().Trend : 0
                }).OrderByDescending(s => s.Value).ThenBy(s => s.Title).Take(5).ToList();

                lstCustomFieldNames.Add(customfield.CustomFieldName);
                lstListSourcePerformance.Add(new ListSourcePerformanceData
                {
                    lstSourcePerformanceData = lstSourcePerformance,
                    CustomFieldName = customfield.CustomFieldName
                });
            }
            //// End - Added by Arpita Soni for Ticket #1148 on 01/27/2015

            // Modified by Arpita Soni for Ticket #1148 on 01/27/2015 
            return Json(new
            {
                ChartCustomField1 = lstListSourcePerformance.Count > 0 ? lstListSourcePerformance.ElementAt(0).lstSourcePerformanceData : null,
                ChartCustomField2 = lstListSourcePerformance.Count > 1 ? lstListSourcePerformance.ElementAt(1).lstSourcePerformanceData : null,
                ChartCustomField3 = lstListSourcePerformance.Count > 2 ? lstListSourcePerformance.ElementAt(2).lstSourcePerformanceData : null,
                CustomField1 = lstCustomFieldNames.Count > 0 ? lstCustomFieldNames.ElementAt(0) : null,
                CustomField2 = lstCustomFieldNames.Count > 1 ? lstCustomFieldNames.ElementAt(1) : null,
                CustomField3 = lstCustomFieldNames.Count > 2 ? lstCustomFieldNames.ElementAt(2) : null

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

            //// Start - Added by Arpita Soni for Ticket #1148 on 01/27/2015
            string entityType = Enums.EntityType.Tactic.ToString();
            string dropdownType = Enums.CustomFieldType.DropDownList.ToString();
            int dropdwnCustomFieldTypeId = db.CustomFieldTypes.Where(custType => custType.Name.Equals(dropdownType)).Select(custType => custType.CustomFieldTypeId).FirstOrDefault();

            // Get first 3 custom fields
            var customFields = db.CustomFields.Where(c => c.ClientId.Equals(Sessions.User.ClientId)
                && c.IsDeleted == false && c.IsRequired == true && c.IsDefault == true && c.EntityType == entityType && c.CustomFieldTypeId == dropdwnCustomFieldTypeId
                ).Select(c => new
                {
                    CustomFieldId = c.CustomFieldId,
                    CustomFieldName = c.Name
                }).Take(3).ToList();

            customFields = customFields.Where(sort => !string.IsNullOrEmpty(sort.CustomFieldName)).OrderBy(sort => sort.CustomFieldName, new AlphaNumericComparer()).ToList();
            List<CustomFieldOption> tblCustomFieldOption = db.CustomFieldOptions.ToList().Where(co => customFields.Select(custm => custm.CustomFieldId).Contains(co.CustomFieldId) && co.IsDeleted == false).ToList();
            List<CustomField_Entity> tblCustomfieldEntity = db.CustomField_Entity.ToList().Where(ent => customFields.Select(custm => custm.CustomFieldId).Contains(ent.CustomFieldId)).ToList();
            List<ListSourcePerformanceData> lstListSourcePerformance = new List<ListSourcePerformanceData>();
            List<string> lstCustomFieldNames = new List<string>();
            // Applying custom field filters
            foreach (var customfield in customFields)
            {
                List<SourcePerformanceData> lstSourcePerformance = new List<SourcePerformanceData>();

                List<string> lstOptionIds = new List<string>();
                var customFieldOptionsIds = db.CustomFieldOptions.Where(c => c.CustomFieldId == customfield.CustomFieldId && c.IsDeleted == false).Select(c => c.CustomFieldOptionId);
                foreach (var customOptionId in customFieldOptionsIds)
                {
                    lstOptionIds.Add(customOptionId.ToString());
                }

                var lstCustomFieldEntity = tblCustomfieldEntity.Where(cf => lstOptionIds.Contains(cf.Value)).Select(e => new
                {
                    EntityId = e.EntityId,
                    Value = e.Value
                }).ToList();

                var lstTacticActuals = from entity in lstCustomFieldEntity
                                       join tacticactual in planTacticActuals
                                       on entity.EntityId equals tacticactual.PlanTacticId
                                       where tacticactual.StageTitle == mql
                                       select new { entity.Value, entity.EntityId, tacticactual.Actualvalue, timeFrameOptions = tacticactual.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + tacticactual.Period, tacticactual };

                lstSourcePerformance = tblCustomFieldOption.Where(cf => cf.CustomFieldId == customfield.CustomFieldId).ToList()
                                                .Select(cf => new SourcePerformanceData
                                                {
                                                    Title = cf.Value,
                                                    ColorCode = string.Format("#{0}", cf.ColorCode),
                                                    Value = lstTacticActuals.Any(ta => ta.Value == Convert.ToString(cf.CustomFieldOptionId)) ? GetActualTacticDataTablebyStageCode(customfield.CustomFieldId, cf.CustomFieldOptionId.ToString(), dropdownType, Enums.InspectStage.MQL, lstTacticActuals.Where(t => t.Value == Convert.ToString(cf.CustomFieldOptionId) &&
                                                        includeMonth.Contains(t.timeFrameOptions)).Select(actTactic => actTactic.tacticactual).ToList(), Tacticdata, true).Sum(t => t.ActualValue) : 0
                                                }).OrderByDescending(cf => cf.Value).ThenBy(cf => cf.Title).Take(5).ToList();

                lstCustomFieldNames.Add(customfield.CustomFieldName);

                lstListSourcePerformance.Add(new ListSourcePerformanceData
                {
                    lstSourcePerformanceData = lstSourcePerformance,
                    CustomFieldName = customfield.CustomFieldName
                });
            }
            //// End - Added by Arpita Soni for Ticket #1148 on 01/27/2015

            // Modified by Arpita Soni for Ticket #1148 on 01/27/2015
            return Json(new
            {
                ChartCustomField1 = lstListSourcePerformance.Count > 0 ? lstListSourcePerformance.ElementAt(0).lstSourcePerformanceData : null,
                ChartCustomField2 = lstListSourcePerformance.Count > 1 ? lstListSourcePerformance.ElementAt(1).lstSourcePerformanceData : null,
                ChartCustomField3 = lstListSourcePerformance.Count > 2 ? lstListSourcePerformance.ElementAt(2).lstSourcePerformanceData : null,
                CustomField1 = lstCustomFieldNames.Count > 0 ? lstCustomFieldNames.ElementAt(0) : null,
                CustomField2 = lstCustomFieldNames.Count > 1 ? lstCustomFieldNames.ElementAt(1) : null,
                CustomField3 = lstCustomFieldNames.Count > 2 ? lstCustomFieldNames.ElementAt(2) : null
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

            //// Start - Added by Arpita Soni for Ticket #1148 on 01/27/2015
            string entityType = Enums.EntityType.Tactic.ToString();
            string dropdownType = Enums.CustomFieldType.DropDownList.ToString();
            int dropdwnCustomFieldTypeId = db.CustomFieldTypes.Where(custType => custType.Name.Equals(dropdownType)).Select(custType => custType.CustomFieldTypeId).FirstOrDefault();

            // Get first 3 custom fields
            var customFields = db.CustomFields.Where(c => c.ClientId.Equals(Sessions.User.ClientId)
                && c.IsDeleted == false && c.IsRequired == true && c.IsDefault == true && c.EntityType == entityType && c.CustomFieldTypeId == dropdwnCustomFieldTypeId
                ).Select(c => new
                {
                    CustomFieldId = c.CustomFieldId,
                    CustomFieldName = c.Name
                }).Take(3).ToList();

            customFields = customFields.Where(sort => !string.IsNullOrEmpty(sort.CustomFieldName)).OrderBy(sort => sort.CustomFieldName, new AlphaNumericComparer()).ToList();
            List<CustomFieldOption> tblCustomFieldOption = db.CustomFieldOptions.ToList().Where(co => customFields.Select(custm => custm.CustomFieldId).Contains(co.CustomFieldId) && co.IsDeleted == false).ToList();
            List<CustomField_Entity> tblCustomfieldEntity = db.CustomField_Entity.ToList().Where(ent => customFields.Select(custm => custm.CustomFieldId).Contains(ent.CustomFieldId)).ToList();
            List<ListSourcePerformanceData> lstListSourcePerformance = new List<ListSourcePerformanceData>();
            List<string> lstCustomFieldNames = new List<string>();

            // Applying custom field filters
            foreach (var customfield in customFields)
            {
                List<SourcePerformanceData> lstSourcePerformance = new List<SourcePerformanceData>();

                List<string> lstOptionIds = new List<string>();
                var customFieldOptionsIds = tblCustomFieldOption.Where(c => c.CustomFieldId == customfield.CustomFieldId).Select(c => c.CustomFieldOptionId);
                foreach (var customfieldOptionId in customFieldOptionsIds)
                {
                    lstOptionIds.Add(customfieldOptionId.ToString());
                }

                var lstCustomFieldEntity = tblCustomfieldEntity.Where(cf => lstOptionIds.Contains(cf.Value)).Select(e => new
                {
                    EntityId = e.EntityId,
                    Value = e.Value
                }).ToList();

                var lstTacticActuals = from entity in lstCustomFieldEntity
                                       join tacticactual in Tacticdata
                                       on entity.EntityId equals tacticactual.TacticObj.PlanTacticId
                                       select new { entity.Value, entity.EntityId, tacticactual };


                lstSourcePerformance = tblCustomFieldOption.Where(b => b.CustomFieldId == customfield.CustomFieldId).ToList()
                                                .Select(b => new SourcePerformanceData
                                                {
                                                    Title = b.Value,
                                                    ColorCode = string.Format("#{0}", b.ColorCode),
                                                    Value = lstTacticActuals.Any(t => t.Value == Convert.ToString(b.CustomFieldOptionId)) ?
                                                    GetProjectedDatabyStageCode(customfield.CustomFieldId, b.CustomFieldOptionId.ToString(), dropdownType, Enums.InspectStage.MQL, lstTacticActuals.Where(lta => lta.Value == Convert.ToString(b.CustomFieldOptionId)).Select(ts => ts.tacticactual)
                                                                                        .ToList(), true)
                                                                                        .Where(mr => includeMonth.Contains(mr.Month))
                                                                                        .Sum(r => r.Value) : 0
                                                }).OrderByDescending(ta => ta.Value).ThenBy(ta => ta.Title).Take(5).ToList();

                lstCustomFieldNames.Add(customfield.CustomFieldName);

                lstListSourcePerformance.Add(new ListSourcePerformanceData
                {
                    lstSourcePerformanceData = lstSourcePerformance,
                    CustomFieldName = customfield.CustomFieldName
                });
            }
            //// End - Added by Arpita Soni for Ticket #1148 on 01/27/2015

            // Modified by Arpita Soni for Ticket #1148 on 01/27/2015
            return Json(new
            {
                ChartCustomField1 = lstListSourcePerformance.Count > 0 ? lstListSourcePerformance.ElementAt(0).lstSourcePerformanceData : null,
                ChartCustomField2 = lstListSourcePerformance.Count > 1 ? lstListSourcePerformance.ElementAt(1).lstSourcePerformanceData : null,
                ChartCustomField3 = lstListSourcePerformance.Count > 2 ? lstListSourcePerformance.ElementAt(2).lstSourcePerformanceData : null,
                CustomField1 = lstCustomFieldNames.Count > 0 ? lstCustomFieldNames.ElementAt(0) : null,
                CustomField2 = lstCustomFieldNames.Count > 1 ? lstCustomFieldNames.ElementAt(1) : null,
                CustomField3 = lstCustomFieldNames.Count > 2 ? lstCustomFieldNames.ElementAt(2) : null
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
            int customfieldId = 0;
            string customFieldType = string.Empty;
            //Custom
            if (ParentConversionSummaryTab.Contains(Common.TacticCustomTitle))
            {
                customfieldId = Convert.ToInt32(ParentConversionSummaryTab.Replace(Common.TacticCustomTitle, ""));
                IsTacticCustomField = true;
            }
            else if (ParentConversionSummaryTab.Contains(Common.CampaignCustomTitle))
            {
                customfieldId = Convert.ToInt32(ParentConversionSummaryTab.Replace(Common.CampaignCustomTitle, ""));
                IsCampaignCustomField = true;
            }
            else if (ParentConversionSummaryTab.Contains(Common.ProgramCustomTitle))
            {
                customfieldId = Convert.ToInt32(ParentConversionSummaryTab.Replace(Common.ProgramCustomTitle, ""));
                IsProgramCustomField = true;
            }
            else
            {
                Tacticdata = Tacticdata.ToList();
            }
            var DataTitleList = new List<RevenueContrinutionData>();

            if (ParentConversionSummaryTab.Contains(Common.CampaignCustomTitle) || ParentConversionSummaryTab.Contains(Common.ProgramCustomTitle) || ParentConversionSummaryTab.Contains(Common.TacticCustomTitle))
            {

                List<int> entityids = new List<int>();
                if (IsTacticCustomField)
                {
                    entityids = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
                }
                else if (IsCampaignCustomField)
                {
                    entityids = Tacticdata.Select(t => t.TacticObj.Plan_Campaign_Program.PlanCampaignId).ToList();
                }
                else
                {
                    entityids = Tacticdata.Select(t => t.TacticObj.PlanProgramId).ToList();
                }

                customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();
                if (customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                {
                    var optionlist = cusomfieldEntity.Select(c => Convert.ToInt32(c.Value)).ToList();
                    DataTitleList = (from cfo in db.CustomFieldOptions
                                     where cfo.CustomFieldId == customfieldId && optionlist.Contains(cfo.CustomFieldOptionId) && cfo.IsDeleted == false
                                     select cfo).ToList().GroupBy(pc => new { id = pc.CustomFieldOptionId, title = pc.Value }).Select(pc =>
                                  new RevenueContrinutionData
                                  {
                                      Title = pc.Key.title,
                                      CustomFieldOptionid = pc.Key.id,
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
                                    CustomFieldOptionid = 0,
                                    planTacticList = pc.Select(c => c.EntityId).ToList()
                                }).ToList();
                }
            }

            List<string> includeMonth = GetMonthListForReport(selectOption, true);
            string stageTitleMQL = Enums.InspectStage.MQL.ToString();
            string stageTitleCW = Enums.InspectStage.CW.ToString();
            string stageTitleRevenue = Enums.InspectStage.Revenue.ToString();

            List<Plan_Campaign_Program_Tactic_Actual> planTacticActual = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => planTacticActual.Add(a)));
            planTacticActual = planTacticActual.Where(mr => includeMonth.Contains((Tacticdata.FirstOrDefault(_tac => _tac.TacticObj.PlanTacticId == mr.PlanTacticId).TacticYear) + mr.Period)).ToList();
            string inq = Enums.Stage.INQ.ToString();
            int INQStageId = db.Stages.FirstOrDefault(stage => stage.ClientId == Sessions.User.ClientId && stage.Code == inq && stage.IsDeleted == false).StageId;

            var DataListFinal = DataTitleList.Select(p => new
            {
                Title = p.Title,
                INQ = GetActualValuebyWeightageForINQ(customfieldId, p.CustomFieldOptionid.ToString(), customFieldType, planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), INQStageId, Tacticdata, IsTacticCustomField),
                MQL = GetActualValuebyWeightageForConversionSummary(customfieldId, p.CustomFieldOptionid.ToString(), customFieldType, planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), Enums.InspectStage.MQL, Tacticdata, IsTacticCustomField),
                ActualCW = GetActualValuebyWeightageForConversionSummary(customfieldId, p.CustomFieldOptionid.ToString(), customFieldType, planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), Enums.InspectStage.CW, Tacticdata, IsTacticCustomField),
                ActualRevenue = GetActualValuebyWeightageForConversionSummary(customfieldId, p.CustomFieldOptionid.ToString(), customFieldType, planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), Enums.InspectStage.Revenue, Tacticdata, IsTacticCustomField),
                ActualADS = CalculateActualADS(customfieldId, p.CustomFieldOptionid.ToString(), customFieldType, planTacticActual.Where(t => p.planTacticList.Contains(t.PlanTacticId)).ToList(), Tacticdata, IsTacticCustomField),
                ProjectedCW = GetPlanValue(customfieldId, p.CustomFieldOptionid.ToString(), Tacticdata.Where(t => p.planTacticList.Contains(t.TacticObj.PlanTacticId)).ToList(), customFieldType, includeMonth, Enums.InspectStage.CW, IsTacticCustomField),
                ProjectedRevenue = GetPlanValue(customfieldId, p.CustomFieldOptionid.ToString(), Tacticdata.Where(t => p.planTacticList.Contains(t.TacticObj.PlanTacticId)).ToList(), customFieldType, includeMonth, Enums.InspectStage.Revenue, IsTacticCustomField),
                ProjectedADS = p.planTacticList.Any() ? db.Models.Where(m => (db.Plan_Campaign_Program_Tactic.Where(t => p.planTacticList.Contains(t.PlanTacticId)).Select(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId).Distinct()).Contains(m.ModelId)).Sum(mf => mf.AverageDealSize) : 0
            }).Distinct().OrderBy(p => p.Title, new AlphaNumericComparer());

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
        /// Get Actual value based on stagetitle & weightage
        /// Added by Viral Kadiya
        /// </summary>
        /// <param name="planTacticActual">Plan tactic actual</param>
        /// <param name="stagetitle">Stage title.</param>
        /// <returns>Returns actual value for conversion summary.</returns>
        private double GetActualValuebyWeightageForConversionSummary(int customfieldId, string CustomFieldOptionId, string customfieldType, List<Plan_Campaign_Program_Tactic_Actual> planTacticActual, Enums.InspectStage stagecode, List<TacticStageValue> TacticData, bool IsTacticCustomField)
        {
            string projectedStageValue = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
            List<Plan_Campaign_Program_Tactic_Actual> lstActualsTactic = new List<Plan_Campaign_Program_Tactic_Actual>();

            lstActualsTactic = planTacticActual.Where(pcpta => pcpta.StageTitle == stagecode.ToString()).ToList();
            List<ActualDataTable> lstActualData = new List<ActualDataTable>();
            lstActualData = GetActualTacticDataTablebyStageCode(customfieldId, CustomFieldOptionId, customfieldType, stagecode, lstActualsTactic, TacticData, IsTacticCustomField);
            double actualValue = lstActualData.Sum(pcpta => pcpta.ActualValue);
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
        private double CalculateActualADS(int customfieldId, string CustomFieldOptionId, string customfieldType, List<Plan_Campaign_Program_Tactic_Actual> planTacticActual, List<TacticStageValue> TacticData, bool IsTacticCustomField)
        {
            double ads = 0;
            List<ActualDataTable> lstRevenueData = new List<ActualDataTable>();
            List<ActualDataTable> lstCWData = new List<ActualDataTable>();
            string strRevenueStageCode = Enums.InspectStage.Revenue.ToString();
            string strCWStageCode = Enums.InspectStage.CW.ToString();
            lstRevenueData = GetActualTacticDataTablebyStageCode(customfieldId, CustomFieldOptionId, customfieldType, Enums.InspectStage.Revenue, planTacticActual.Where(act => act.StageTitle.Equals(strRevenueStageCode)).ToList(), TacticData, IsTacticCustomField);
            lstCWData = GetActualTacticDataTablebyStageCode(customfieldId, CustomFieldOptionId, customfieldType, Enums.InspectStage.CW, planTacticActual.Where(act => act.StageTitle.Equals(strCWStageCode)).ToList(), TacticData, IsTacticCustomField);
            double actualRevenue = lstRevenueData.Sum(reve => reve.ActualValue);
            double actualCW = lstCWData.Sum(reve => reve.ActualValue);
            if (actualCW > 0)
            {
                ads = actualRevenue / actualCW;
            }
            return ads;
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

        /// <summary>
        /// Get Actual value based on stagetitle
        /// Added by Viral Kadiya
        /// </summary>
        /// <param name="planTacticActual">Plan tactic actual</param>
        /// <param name="stagetitle">Stage title.</param>
        /// <returns>Returns actual value for conversion summary.</returns>
        private double GetActualValuebyWeightageForINQ(int customfieldId, string CustomFieldOptionId, string customfieldType, List<Plan_Campaign_Program_Tactic_Actual> planTacticActual, int stageId, List<TacticStageValue> TacticData, bool IsTacticCustomField)
        {
            string projectedStageValue = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
            double INQValue = 0;
            List<Plan_Campaign_Program_Tactic_Actual> lstActualsTactic = new List<Plan_Campaign_Program_Tactic_Actual>();
            List<ActualDataTable> lstActualDataTable = new List<ActualDataTable>();
            lstActualsTactic = planTacticActual.Where(pta => pta.Plan_Campaign_Program_Tactic.StageId == stageId && pta.StageTitle == projectedStageValue).ToList();
            lstActualDataTable = GetActualTacticDataTablebyStageCode(customfieldId, CustomFieldOptionId, customfieldType, Enums.InspectStage.INQ, lstActualsTactic, TacticData, IsTacticCustomField);

            INQValue = lstActualDataTable.Where(pta => pta.StageTitle == projectedStageValue).ToList().Sum(act => act.ActualValue);
            return INQValue;
        }
        #endregion

        #endregion

        #region Revenue

        #region "Old Code GetRevenueData function"
        ///// <summary>
        ///// Return Revenue Partial View
        ///// </summary>
        ///// <param name="PlanId"></param>
        ///// <param name="timeFrameOption"></param>
        ///// <returns></returns>
        //[AuthorizeUser(Enums.ApplicationActivity.ReportView)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #519 to implement user permission Logic
        //public ActionResult GetRevenueData(string timeFrameOption = "thisquarter")
        //{
        //    ViewBag.MonthTitle = GetDisplayMonthListForReport(timeFrameOption);
        //    ViewBag.SelectOption = timeFrameOption;

        //    List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting();

        //    // Modified by : #960 Kalpesh Sharma : Filter changes for Revenue report - Revenue Report
        //    // Fetch the respectives Campaign Ids and Program Ids from the tactic list
        //    List<int> campaignlist = tacticlist.Select(t => t.Plan_Campaign_Program.PlanCampaignId).ToList();
        //    List<int> programlist = tacticlist.Select(t => t.PlanProgramId).ToList();


        //    //// Get Campaign list for dropdown
        //    List<int> campaignIds = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).Select(t => t.Plan_Campaign_Program.PlanCampaignId).Distinct().ToList<int>();
        //    var campaignList = db.Plan_Campaign.Where(pc => campaignIds.Contains(pc.PlanCampaignId))
        //            .Select(pcp => new { PlanCampaignId = pcp.PlanCampaignId, Title = pcp.Title })
        //            .OrderBy(pcp => pcp.Title).ToList();
        //    campaignList = campaignList.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
        //    var lstCampaignList = campaignList;
        //    lstCampaignList.Insert(0, new { PlanCampaignId = 0, Title = "All Campaigns" });

        //    //// Get Program list for dropdown
        //    var programList = db.Plan_Campaign_Program.Where(pc => campaignIds.Contains(pc.PlanCampaignId))
        //           .Select(c => new { PlanProgramId = c.PlanProgramId, Title = c.Title })
        //           .OrderBy(pcp => pcp.Title).ToList();
        //    programList = programList.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
        //    var lstProgramList = programList;
        //    lstProgramList.Insert(0, new { PlanProgramId = 0, Title = "All Programs" });

        //    //// Get tactic list for dropdown
        //    var tacticListinner = tacticlist.Select(t => new { PlanTacticId = t.PlanTacticId, Title = t.Title })
        //        .OrderBy(pcp => pcp.Title).ToList();
        //    tacticListinner = tacticListinner.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
        //    var lstTacticList = tacticListinner;
        //    lstTacticList.Insert(0, new { PlanTacticId = 0, Title = "All Tactics" });

        //    //// Set in viewbag
        //    ViewBag.CampaignDropdownList = lstCampaignList;
        //    ViewBag.ProgramDropdownList = lstProgramList;
        //    ViewBag.TacticDropdownList = lstTacticList;

        //    List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);

        //    //// Set Parent Revenue Summary data to list.
        //    List<ViewByModel> lstParentRevenueSummery = new List<ViewByModel>();
        //    if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count > 0 && isPublishedPlanExist)
        //    {
        //        lstParentRevenueSummery.Add(new ViewByModel { Text = Common.RevenuePlans, Value = Common.RevenuePlans });
        //    }

        //    lstParentRevenueSummery = lstParentRevenueSummery.Where(s => !string.IsNullOrEmpty(s.Text)).ToList();



        //    // Modified by : #960 Kalpesh Sharma : Filter changes for Revenue report - Revenue Report
        //    //Concat the Campaign and Program custom fields data with exsiting one. 
        //    var lstCustomFields = Common.GetCustomFields(tacticlist.Select(tactic => tactic.PlanTacticId).ToList(), programlist, campaignlist);
        //    lstParentRevenueSummery = lstParentRevenueSummery.Concat(lstCustomFields).ToList();
        //    ViewBag.parentRevenueSummery = lstParentRevenueSummery;
        //    // Get child tab list
        //    if (lstParentRevenueSummery.Count > 0)
        //        ViewBag.ChildTabListRevenueSummary = GetChildLabelDataViewByModel(lstParentRevenueSummery.First().Value, timeFrameOption);
        //    else
        //        ViewBag.ChildTabListRevenueSummary = "";
        //    //// Set Parent Revenue Plan data to list.
        //    List<ViewByModel> lstParentRevenueToPlan = new List<ViewByModel>();
        //    lstParentRevenueToPlan = lstParentRevenueToPlan.Concat(lstCustomFields).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
        //    ViewBag.parentRevenueToPlan = lstParentRevenueToPlan;
        //    // Get child tab list
        //    if (lstParentRevenueToPlan.Count > 0)
        //        ViewBag.ChildTabListRevenueToPlan = GetChildLabelDataViewByModel(lstParentRevenueToPlan.First().Value, timeFrameOption);
        //    else
        //        ViewBag.ChildTabListRevenueToPlan = "";
        //    //// Set Parent Revenue Contribution data to list.
        //    List<ViewByModel> lstParentRevenueContribution = new List<ViewByModel>();

        //    lstParentRevenueContribution.Add(new ViewByModel { Text = Common.RevenueCampaign, Value = Common.RevenueCampaign });

        //    lstParentRevenueContribution = lstParentRevenueContribution.Where(s => !string.IsNullOrEmpty(s.Text)).ToList();
        //    //Concat the Campaign and Program custom fields data with exsiting one. 
        //    lstParentRevenueContribution = lstParentRevenueContribution.Concat(lstCustomFields).ToList();
        //    ViewBag.parentRevenueContribution = lstParentRevenueContribution;

        //    TempData["ReportData"] = tacticStageList;

        //    return PartialView("Revenue");
        //} 
        #endregion

        /// <summary>
        ///  Get Header value for reveneue #1397
        ///  Created By Nishant Sheth
        /// </summary>
        public ReportModel GetRevenueHeaderValue(BasicModel objBasicModel, string timeFrameOption)
        {
            double _actualval, _actualtotal = 0, _projectedval, _projectedtotal = 0, _goalval, _goaltotal = 0, _goalYTD = 0;
            double _ActualPercentage, _ProjectedPercentage;
            string currentyear = DateTime.Now.Year.ToString();
            int currentEndMonth = 12;
            ReportModel objReportModel = new ReportModel();
            Projected_Goal objProjectedGoal = new Projected_Goal();
            objProjectedGoal.ActualPercentageIsnegative = true;
            objProjectedGoal.ProjectedPercentageIsnegative = true;
            List<string> categories = new List<string>();
            categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
            int categorieslength = 4;
            categorieslength = categories.Count;   // Set categories list count.
            List<ProjectedTrendModel> ProjectedTrendModelList = new List<ProjectedTrendModel>();
            if (objBasicModel.IsQuarterly)
            {

                //List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
                //List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
                //List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
                //List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };

                //List<string> _curntQuarterList = new List<string>();

                //for (int i = 1; i <= categorieslength; i++)
                //{
                //    #region "Get Quarter list based on loop value"
                //    if (i == 1)
                //    {
                //        _curntQuarterList = Q1.Where(q1 => Convert.ToInt32(q1.Replace("Y", "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                //    }
                //    else if (i == 2)
                //    {
                //        _curntQuarterList = Q2.Where(q2 => Convert.ToInt32(q2.Replace("Y", "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                //    }
                //    else if (i == 3)
                //    {
                //        _curntQuarterList = Q3.Where(q3 => Convert.ToInt32(q3.Replace("Y", "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                //    }
                //    else if (i == 4)
                //    {
                //        _curntQuarterList = Q4.Where(q4 => Convert.ToInt32(q4.Replace("Y", "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                //    }
                //    #endregion

                //}

                _actualtotal = objBasicModel.ActualList.Sum(actual => actual);
                _projectedtotal = objBasicModel.ProjectedList.Sum(projected => projected);
                _goaltotal = objBasicModel.GoalList.Sum(goal => goal);
                _goalYTD = objBasicModel.GoalYTD.Sum(goalYTD => goalYTD);
            }
            else
            {

                if (timeFrameOption.ToLower() == currentyear.ToLower())
                {
                    currentEndMonth = Convert.ToInt32(DateTime.Now.Month);
                }
                #region Calculate GoalYTD
                for (int i = 0; i < 12; i++)
                {
                    _goalval = objBasicModel.GoalList.ToList()[i];
                    if (currentEndMonth > i)
                    {
                        if (_goalval != 0.0)
                        {
                            _goalYTD = _goalYTD + _goalval;
                        }
                    }
                    else
                    {
                        _goalYTD += 0;
                    }
                    //_monthTrendList.Add(_actualtotal);
                }
                #endregion

                #region Calculate GoalTotoal/Goal Year
                _goaltotal = objBasicModel.GoalList.Sum(goal => goal);
                #endregion

                #region Calculate Actual Value
                _actualtotal = objBasicModel.ActualList.Sum(actual => actual);
                //for (int i = 0; i < 12; i++)
                //{
                //    _actualval = objBasicModel.ActualList.ToList()[i];
                //    if (currentEndMonth > i)
                //    {
                //        if (_actualval != 0.0)
                //        {
                //            _actualtotal = _actualtotal + _actualval;
                //        }
                //    }
                //    else
                //    {
                //        _actualtotal += 0;
                //    }
                //    //_monthTrendList.Add(_actualtotal);
                //}
                #endregion

                #region Calculate Projected Value
                _projectedtotal = objBasicModel.ProjectedList.Sum(projected => projected);
                #endregion
            }
            _ActualPercentage = _goalYTD != 0 ? (((_actualtotal - _goalYTD) / _goalYTD) * 100) : 0;
            if (_ActualPercentage > 0)
            {
                objProjectedGoal.ActualPercentageIsnegative = false;
            }
            _ProjectedPercentage = _goaltotal != 0 ? (((_projectedtotal - _goaltotal) / _goaltotal) * 100) : 0;
            if (_ProjectedPercentage > 0)
            {
                objProjectedGoal.ProjectedPercentageIsnegative = false;
            }
            objProjectedGoal.GoalYTD = Convert.ToString(_goalYTD);
            objProjectedGoal.GoalYear = Convert.ToString(_goaltotal);
            objProjectedGoal.Actual_Projected = Convert.ToString(_actualtotal);
            objProjectedGoal.Projected = Convert.ToString(_projectedtotal);
            objProjectedGoal.ActualPercentage = Convert.ToString(_ActualPercentage);
            objProjectedGoal.ProjectedPercentage = Convert.ToString(_ProjectedPercentage);
            objReportModel.RevenueHeaderModel = objProjectedGoal;

            return objReportModel;
        }

        [AuthorizeUser(Enums.ApplicationActivity.ReportView)]
        public ActionResult GetRevenueData(string option = "thisquarter", string isQuarterly = "Quarterly")
        {
			#region "Declare Main Variable for Model"

                ReportModel objReportModel = new ReportModel();

                #endregion
                //Update logic by Bhavesh : Report Review code
                // Start - Added by Arpita Soni for Ticket #1148 on 01/30/2015
                // To avoid summary display when no published plan selected (It displays no data found message.)

                string PublishedPlan = Convert.ToString(Enums.PlanStatus.Published).ToLower();
                var plan = db.Plans.Where(p => Sessions.ReportPlanIds.Contains(p.PlanId) && p.Status.ToLower() == PublishedPlan).FirstOrDefault();
                // End - Added by Arpita Soni for Ticket #1148 on 01/30/2015

                //// check planids selected or not
                if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count > 0 && plan != null)
                {
            #region "Declare Local Variables"
            List<Plan_Campaign_Program_Tactic> tacticlist = new List<Plan_Campaign_Program_Tactic>();
            List<int> campaignlist = new List<int>();
            List<int> programlist = new List<int>();
            List<TacticStageValue> Tacticdata = new List<TacticStageValue>();


            bool IsTillCurrentMonth = true, IsQuarterly = true;
            string revStageCode = Convert.ToString(Enums.InspectStageValues[Convert.ToString(Enums.InspectStage.Revenue)]);
            List<string> ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add(revStageCode);
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<ActualTrendModel> ActualTacticTrendList = new List<ActualTrendModel>();
            List<ProjectedTrendModel> ProjectedTrendList = new List<ProjectedTrendModel>();
            List<TacticwiseOverviewModel> OverviewModelList = new List<TacticwiseOverviewModel>();
            Projected_Goal objProjectedGoal = new Projected_Goal();
            lineChartData objLineChartData = new lineChartData();
            #endregion

            // Add BY Nishant Sheth
            ViewBag.ParentLabel = Common.RevenueCampaign;
            ViewBag.childlabelType = Common.RevenueCampaign;
            ViewBag.childId = 0;
            ViewBag.option = option;
            #region ViewBag for Back Button
            //TempData["BackParentLabel"] = Common.RevenueCampaign;
            //TempData["BackChildLabel"] = "";
            //TempData["BackId"] = "";
            //TempData["IsBack"] = "";
            //TempData["HeadHireachy"] = "Marketing Revenue";
            //TempData["HeadTitle"] = "Marketing Revenue";
            //TempData["BackWithCustom"] = Common.RevenueCampaign;

            //TempData["HeadHireachy"] = "";
            //TempData["BackParentHireachy"] = "";
            //TempData["BackChildHireachy"] = "";
            //TempData["BackHeadTitle"] = "";
            //TempData["BackIdHireachy"] = "";
            //TempData["BackDummyId"] = "";
            //ViewBag.HeadTitleHearichy = "";
            #endregion
            // End By Nishant Sheth
           

                if (!string.IsNullOrEmpty(isQuarterly) && isQuarterly.Equals(Enums.ViewByAllocated.Monthly.ToString()))
                    IsQuarterly = false;

                //// Set View By Allocated values.
                List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
                lstViewByAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.months.ToString() });
                lstViewByAllocated.Add(new ViewByModel { Text = "Quarterly", Value = Enums.PlanAllocatedBy.quarters.ToString() });
                lstViewByAllocated = lstViewByAllocated.Where(modal => !string.IsNullOrEmpty(modal.Text)).ToList();
                ViewBag.ViewByAllocated = lstViewByAllocated;

                if (IsQuarterly)
                    ViewBag.SelectedTimeFrame = Enums.PlanAllocatedBy.quarters.ToString();
                else
                    ViewBag.SelectedTimeFrame = Enums.PlanAllocatedBy.months.ToString();

               
                    tacticlist = GetTacticForReporting();
                    // Fetch the respectives Campaign Ids and Program Ids from the tactic list
                    campaignlist = tacticlist.Select(t => t.Plan_Campaign_Program.PlanCampaignId).ToList();
                    programlist = tacticlist.Select(t => t.PlanProgramId).ToList();
                    Tacticdata = Common.GetTacticStageRelation(tacticlist, IsReport: true);
                    TempData["ReportData"] = Tacticdata;

                    //// get month list
                    List<string> includeMonth = GetMonthListForReport(option, true);

                    #region "Get CustomField List"

                    #region "Set Parent DDL data to ViewBag"
                    //// Set Parent Revenue Summary data to list.
                    List<ViewByModel> lstParentRevenueSummery = new List<ViewByModel>();
                    lstParentRevenueSummery.Add(new ViewByModel { Text = Common.RevenueCampaign, Value = Common.RevenueCampaign });
                    lstParentRevenueSummery = lstParentRevenueSummery.Where(s => !string.IsNullOrEmpty(s.Text)).ToList();
                    //Concat the Campaign and Program custom fields data with exsiting one. 
                    var lstCustomFields = Common.GetCustomFields(tacticlist.Select(tactic => tactic.PlanTacticId).ToList(), programlist, campaignlist);
                    lstParentRevenueSummery = lstParentRevenueSummery.Concat(lstCustomFields).ToList();
                    ViewBag.parentRevenueSummery = lstParentRevenueSummery;
                    #endregion

                    #region "Set Child DDL data to ViewBag"
                    // Get child tab list
                    List<ViewByModel> lstChildRevenueToPlan = new List<ViewByModel>();
                    lstChildRevenueToPlan.Add(new ViewByModel { Text = "All", Value = "0" });
                    List<ViewByModel> childCustomFieldOptionList = new List<ViewByModel>();
                    if (lstParentRevenueSummery.Count > 0)
                        childCustomFieldOptionList = GetChildLabelDataViewByModel(lstParentRevenueSummery.First().Value, option);

                    ViewBag.ChildTabListRevenueToPlan = lstChildRevenueToPlan.Concat(childCustomFieldOptionList).ToList();
                    #endregion

                    #region "Set Campaign,Program,Tactic list to ViewBag"

                    //// Get Campaign list for dropdown
                    List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
                    List<int> campaignIds = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).Select(t => t.Plan_Campaign_Program.PlanCampaignId).Distinct().ToList<int>();
                    var campaignList = db.Plan_Campaign.Where(pc => campaignIds.Contains(pc.PlanCampaignId))
                            .Select(pcp => new { PlanCampaignId = pcp.PlanCampaignId, Title = pcp.Title })
                            .OrderBy(pcp => pcp.Title).ToList();
                    campaignList = campaignList.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
                    var lstCampaignList = campaignList;
                    lstCampaignList.Insert(0, new { PlanCampaignId = 0, Title = "All Campaigns" });

                    List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
                    _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
                    //// Get Program list for dropdown
                    var programList = db.Plan_Campaign_Program.Where(pc => campaignIds.Contains(pc.PlanCampaignId))
                           .Select(c => new { PlanProgramId = c.PlanProgramId, Title = c.Title })
                           .OrderBy(pcp => pcp.Title).ToList();
                    programList = programList.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
                    var lstProgramList = programList;
                    lstProgramList.Insert(0, new { PlanProgramId = 0, Title = "All Programs" });

                    //// Get tactic list for dropdown
                    var tacticListinner = tacticlist.Select(t => new { PlanTacticId = t.PlanTacticId, Title = t.Title })
                        .OrderBy(pcp => pcp.Title).ToList();
                    tacticListinner = tacticListinner.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
                    var lstTacticList = tacticListinner;
                    lstTacticList.Insert(0, new { PlanTacticId = 0, Title = "All Tactics" });

                    //// Set DDL List in ViewBag.
                    ViewBag.CampaignDropdownList = lstCampaignList;
                    ViewBag.ProgramDropdownList = lstProgramList;
                    ViewBag.TacticDropdownList = lstTacticList;
                    #endregion

                    #endregion

                    #region "Revenue Model Values"

                    ActualTacticStageList = GetActualListInTacticInterval(Tacticdata, option, ActualStageCodeList, IsTillCurrentMonth);
                    ActualTacticTrendList = GetActualTrendModelForRevenueOverview(Tacticdata, ActualTacticStageList);

                    #region "Revenue : Get Tacticwise Actual_Projected Vs Goal Model data "
                    ProjectedTrendList = CalculateProjectedTrend(Tacticdata, includeMonth, revStageCode);
                    OverviewModelList = GetTacticwiseActualProjectedRevenueList(ActualTacticTrendList, ProjectedTrendList);
                    #endregion

                    #region "Get Basic Model"
                    BasicModel objBasicModel = GetValuesListByTimeFrame(ActualTacticTrendList, ProjectedTrendList, option, IsQuarterly);
                    #endregion

                    #region "Set Linechart & Revenue Overview data to model"
                    objLineChartData = GetCombinationLineChartData(objBasicModel);
                    //objProjectedGoal = GetRevenueOverviewData(OverviewModelList, option);
                    //objReportModel.RevenueLineChartModel = objLineChartData != null ? objLineChartData : new lineChartData();
                    // Add By NIshant Sheth For change the logic of header value as per #1397
                    objReportModel.RevenueHeaderModel = GetRevenueHeaderValue(objBasicModel, option).RevenueHeaderModel;

                    //objReportModel.RevenueHeaderModel = objProjectedGoal != null ? objProjectedGoal : new Projected_Goal();
                    #endregion

                    #endregion

                    #region "Revenue To Plan"
                    RevenueToPlanModel objRevenueToPlanModel = new RevenueToPlanModel();
                    #region "Calculate Barchart data by TimeFrame"
                    BarChartModel objBarChartModel = new BarChartModel();
                    List<BarChartSeries> lstSeries = new List<BarChartSeries>();
                    List<string> _Categories = new List<string>();
                    _Categories = objBasicModel.Categories;
                    double catLength = _Categories != null ? _Categories.Count : 0;
                    List<double> serData1 = new List<double>();
                    List<double> serData2 = new List<double>();
                    List<double> serData3 = new List<double>();
                    double _Actual = 0, _Projected = 0, _Goal = 0, Actual_Projected = 0, _plotBandFromValue = 0, _plotBandToValue = 0;
                    bool _IsQuarterly = objBasicModel.IsQuarterly;
                    int _compareValue = 0;
                    serData1.Add(0); // Insert blank data at 1st index of list to Add padding to Graph.
                    serData2.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.
                    serData3.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.

                    if (!_IsQuarterly)
                    {
                        serData1.Add(0); // Insert blank data at 1st index of list to Add padding to Graph.
                        serData2.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.
                        serData3.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.
                    }
                    _compareValue = IsQuarterly ? GetCurrentQuarterNumber() : currentMonth;

                    #region "Set PlotBand From & To values"
                    _plotBandFromValue = IsQuarterly && (_compareValue != _lastQuarterValue) ? (_compareValue + _constQuartPlotBandPadding) : 0;
                    objBarChartModel.plotBandFromValue = _plotBandFromValue;
                    objBarChartModel.plotBandToValue = _plotBandFromValue > 0 ? (_PlotBandToValue) : 0;
                    #endregion

                    #region "Calcuate ActualList up to Current Month"
                    //List<double> _dtActualList = new List<double>();
                    //List<Plan_Campaign_Program_Tactic_Actual> _fltrActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                    //_fltrActualTacticList = ActualTacticStageList.Where(actualstg => actualstg.StageCode.Equals(revStageCode)).Select(actualstg => actualstg.ActualTacticList).FirstOrDefault();

                    //if (IsQuarterly)
                    //{
                    //    //curntPeriod = PeriodPrefix + i;
                    //    List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
                    //    List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
                    //    List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
                    //    List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };
                    //    //double ActualQ1 = 0, ActualQ2 = 0, ActualQ3 = 0, ActualQ4 = 0, ProjectedQ1 = 0, ProjectedQ2 = 0, ProjectedQ3 = 0, ProjectedQ4 = 0, GoalQ1 = 0, GoalQ2 = 0, GoalQ3 = 0, GoalQ4 = 0, Actual_ProjectedQ1 = 0, Actual_ProjectedQ2 = 0, Actual_ProjectedQ3 = 0, Actual_ProjectedQ4 = 0;


                    //    List<string> _curntQuarterList = new List<string>();

                    //    for (int i = 1; i <= catLength; i++)
                    //    {
                    //        #region "Get Quarter list based on loop value"
                    //        if (i == 1)
                    //            _curntQuarterList = Q1;
                    //        else if (i == 2)
                    //            _curntQuarterList = Q2;
                    //        else if (i == 3)
                    //            _curntQuarterList = Q3;
                    //        else if (i == 4)
                    //            _curntQuarterList = Q4;
                    //        #endregion

                    //        _Actual = _fltrActualTacticList.Where(actual => _curntQuarterList.Contains(actual.Period)).Sum(actual => actual.Actualvalue);
                    //        _dtActualList.Add(_Actual);
                    //    }
                    //}
                    //else
                    //{
                    //    string curntPeriod = string.Empty;

                    //    for (int i = 1; i <= catLength; i++)
                    //    {
                    //        curntPeriod = PeriodPrefix + i;
                    //        _Actual = _fltrActualTacticList.Where(actual => actual.Period.Equals(curntPeriod)).Sum(actual => actual.Actualvalue);
                    //        _dtActualList.Add(_Actual);
                    //    }
                    //}

                    #endregion

                    for (int i = 0; i < catLength; i++)
                    {
                        _Actual = objBasicModel.ActualList[i] != null ? objBasicModel.ActualList[i] : 0;
                        _Projected = objBasicModel.ProjectedList[i] != null ? objBasicModel.ProjectedList[i] : 0;
                        _Goal = objBasicModel.GoalList[i] != null ? objBasicModel.GoalList[i] : 0;
                        Actual_Projected = _Actual + _Projected;
                        //serData1.Add(Actual_Projected);
                        serData2.Add(_Goal);
                        if ((i + 1) <= (_compareValue))
                        {
                            serData1.Add(Actual_Projected);
                            serData3.Add(0);
                        }
                        else
                        {
                            serData1.Add(0);
                            serData3.Add(Actual_Projected);
                        }
                    }
                    List<string> _barChartCategories = new List<string>();
                    if (!IsQuarterly)
                    {
                        _barChartCategories.Add(string.Empty);
                        _barChartCategories.Add(string.Empty);
                        _barChartCategories.AddRange(_Categories);
                    }
                    else
                    {
                        _barChartCategories.Add(string.Empty);
                        _barChartCategories.AddRange(_Categories);
                    }

                    BarChartSeries _chartSeries1 = new BarChartSeries();
                    _chartSeries1.name = "Actual";
                    _chartSeries1.data = serData1;
                    _chartSeries1.type = "column";
                    lstSeries.Add(_chartSeries1);

                    BarChartSeries _chartSeries2 = new BarChartSeries();
                    _chartSeries2.name = "Goal";
                    _chartSeries2.data = serData2;
                    _chartSeries2.type = "column";
                    lstSeries.Add(_chartSeries2);

                    BarChartSeries _chartSeries3 = new BarChartSeries();
                    _chartSeries3.name = "Projected";
                    _chartSeries3.data = serData3;
                    _chartSeries3.type = "column";
                    lstSeries.Add(_chartSeries3);

                    objBarChartModel.series = lstSeries;
                    objBarChartModel.categories = _barChartCategories;

                    objRevenueToPlanModel.RevenueToPlanBarChartModel = objBarChartModel;
                    objRevenueToPlanModel.LineChartModel = objLineChartData;
                    #endregion

                    #region "Calculate DataTable"
                    RevenueDataTable objRevenueDataTable = new RevenueDataTable();
                    RevenueSubDataTableModel objSubDataModel = new RevenueSubDataTableModel();
                    objRevenueDataTable.Categories = _Categories;
                    objRevenueDataTable.ActualList = objBasicModel.ActualList;
                    objRevenueDataTable.ProjectedList = objBasicModel.ProjectedList;
                    objRevenueDataTable.GoalList = objBasicModel.GoalList;
                    objSubDataModel = GetRevenueToPlanDataByCampaign(Tacticdata, option, objBasicModel.IsQuarterly, objBasicModel);
                    objRevenueDataTable.SubDataModel = objSubDataModel;
                    objRevenueDataTable.IsQuarterly = objBasicModel.IsQuarterly;
                    objRevenueDataTable.timeframeOption = objBasicModel.timeframeOption;
                    objRevenueToPlanModel.RevenueToPlanDataModel = objRevenueDataTable;
                    #endregion
                    #endregion
                    objReportModel.RevenueToPlanModel = objRevenueToPlanModel;

                    #region "CardSection Model"
                    CardSectionModel objCardSectionModel = new CardSectionModel();
                    List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
                    CardSectionListModel = GetCardSectionDefaultData(Tacticdata, ActualTacticTrendList, ProjectedTrendList, _cmpgnMappingList, option, IsQuarterly, Common.RevenueCampaign.ToString(), false, "", 0);

                    objCardSectionModel.CardSectionListModel = CardSectionListModel;
                    //objReportModel.RevenueHeaderModel = objCardSectionModel.RevenueHeaderModel;
                    #endregion

                    //objReportModel.CardSectionModel = objCardSectionModel;
                    TempData["RevenueCardList"] = null;
                    TempData["RevenueCardList"] = CardSectionListModel;// For Pagination Sorting and searching
                    //RevenueCardList.RevenueCardListModel = CardSectionListModel;// For Pagination Sorting and searching
                    objReportModel.CardSectionModel = RevenueCardSectionModelWithFilter(0, 5, "", Enums.SortByRevenue.Revenue.ToString());
                    objReportModel.CardSectionModel.TotalRecords = CardSectionListModel.Count();
                }
                else
                {
                    objReportModel.RevenueLineChartModel = new lineChartData();
                    objReportModel.RevenueHeaderModel = new Projected_Goal();
                    //objReportModel.RevenueToPlanBarChartModel = new BarChartModel();
                    //objReportModel.RevenueToPlanDataModel = new RevenueDataTable();
                    objReportModel.RevenueToPlanModel = new RevenueToPlanModel();
                    objReportModel.CardSectionModel = new CardSectionModel();
                }
           
            return PartialView("_Revenue", objReportModel);
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
            int customfieldId = 0;
            string customFieldType = string.Empty;
            bool IsTacticCustomField = false;
            //Check the custom field typa and replace the id with eliminate extra word 
            // Modified by : #960 Kalpesh Sharma : Filter changes for Revenue report - Revenue Report 
            if (ParentLabel.Contains(Common.TacticCustomTitle) || ParentLabel.Contains(Common.CampaignCustomTitle) || ParentLabel.Contains(Common.ProgramCustomTitle))
            {
                if (ParentLabel.Contains(Common.TacticCustomTitle))
                {
                    IsTacticCustomField = true;
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

                customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
            }

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
            List<TacticMonthValue> ProjectedRevenueDatatable = GetProjectedDatabyStageCode(customfieldId, id, customFieldType, Enums.InspectStage.Revenue, Tacticdata, IsTacticCustomField);
            List<TacticMonthValue> MQLDatatable = GetProjectedDatabyStageCode(customfieldId, id, customFieldType, Enums.InspectStage.MQL, Tacticdata, IsTacticCustomField);
            List<ActualDataTable> ActualRevenueDataTable = GetActualTacticDataTablebyStageCode(customfieldId, id, customFieldType, Enums.InspectStage.Revenue, ActualTacticList.Where(act => act.StageTitle.Equals(revenue)).ToList(), Tacticdata, IsTacticCustomField);
            List<ActualDataTable> ActualMQLDataTable = GetActualTacticDataTablebyStageCode(customfieldId, id, customFieldType, Enums.InspectStage.MQL, ActualTacticList.Where(act => act.StageTitle.Equals(mql)).ToList(), Tacticdata, IsTacticCustomField);
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
                tRevenueActual = ActualRevenueDataTable.Where(pcpt => campaign.planTacticList.Contains(pcpt.PlanTacticId)).GroupBy(pt => Tacticdata.FirstOrDefault(_tactic => _tactic.TacticObj.PlanTacticId == pt.PlanTacticId).TacticYear + pt.Period).Select(pcptj => new
                {
                    key = pcptj.Key,
                    ActualValue = pcptj.Sum(pt => pt.ActualValue)
                }),
                tacticActual = ActualMQLDataTable.Where(pcpt => campaign.planTacticList.Contains(pcpt.PlanTacticId)).GroupBy(pt => Tacticdata.FirstOrDefault(_tactic => _tactic.TacticObj.PlanTacticId == pt.PlanTacticId).TacticYear + pt.Period).Select(pcptj => new
                {
                    key = pcptj.Key,
                    ActualValue = pcptj.Sum(pt => pt.ActualValue)
                }).Select(pcptj => pcptj),
            }).Select(p => p).Distinct().OrderBy(p => p.id).OrderBy(p => p.title, new AlphaNumericComparer());

            return Json(campaignList, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Revenue Realization

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Program List.
        /// </summary>
        /// <param name="id"> Id</param>
        /// <returns>Returns Json Result of Program List.</returns>
        public JsonResult LoadProgramDropDown(string id, string selectOption = "")
        {
            // Modified by Arpita Soni  for Ticket #1148 on 01/28/2014
            List<Plan_Campaign_Program_Tactic> TacticList = GetTacticForReporting();
            List<int> programIds = new List<int>();
            if (id != null && id != "")
            {
                int campaignid = Convert.ToInt32(id);
                programIds = TacticList.Where(tactic => tactic.Plan_Campaign_Program.PlanCampaignId == campaignid).Select(tactic => tactic.PlanProgramId).Distinct().ToList<int>();
            }
            else
            {
                programIds = TacticList.Select(tactic => tactic.PlanProgramId).Distinct().ToList<int>();
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

            // Modified by Arpita Soni  for Ticket #1148 on 01/28/2014
            if (id != null && id != "")
            {
                if (type == Common.RevenueCampaign)
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
            }
            else
            {
                var tacticListinner = TacticList.Select(tactic => new { PlanTacticId = tactic.PlanTacticId, Title = tactic.Title })
                    .OrderBy(pcp => pcp.Title).ToList();
                if (tacticListinner == null)
                    return Json(new { });
                tacticListinner = tacticListinner.Where(tactic => !string.IsNullOrEmpty(tactic.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
                return Json(tacticListinner, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Load Revenue Realization Grid.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public JsonResult LoadRevenueRealization(string id, string type = "", string selectOption = "")
        {
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            List<string> includeMonth = GetMonthListForReport(selectOption);

            if (type == Common.RevenueCampaign)
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
            else
            {
                Tacticdata = Tacticdata.ToList();
            }

            if (Tacticdata.Count() > 0)
            {
                string inq = Enums.Stage.INQ.ToString();
                int INQStaged = db.Stages.FirstOrDefault(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false && stage.Code == inq).StageId;
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
            public int CustomFieldOptionid { get; set; }
            // Add TacticMappingItem By Nishant Sheth For Card Sectinon : 13-Jul-2015
            public List<TacticMappingItem> TacticMappingItem { get; set; }
        }

        /// <summary>
        /// Load Revenue Contribution.
        /// </summary>
        /// <param name="parentlabel"></param>
        /// <returns></returns>
        public JsonResult LoadRevenueContribution(string parentlabel, string selectOption)
        {
            List<TacticStageValue> Tacticdata = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];

            bool IsCampaignCustomField = false;
            bool IsProgramCustomField = false;
            bool IsTacticCustomField = false;
            int customfieldId = 0;
            string customFieldType = string.Empty;
            //Modified By : Kalpesh Sharma #960 Filter changes for Revenue report - Revenue Report
            // Check the custom field type and remove extra string for get Custom Field Id
            if (parentlabel.Contains(Common.TacticCustomTitle))
            {
                customfieldId = Convert.ToInt32(parentlabel.Replace(Common.TacticCustomTitle, ""));
                IsTacticCustomField = true;
            }
            else if (parentlabel.Contains(Common.CampaignCustomTitle))
            {
                customfieldId = Convert.ToInt32(parentlabel.Replace(Common.CampaignCustomTitle, ""));
                IsCampaignCustomField = true;
            }
            else if (parentlabel.Contains(Common.ProgramCustomTitle))
            {
                customfieldId = Convert.ToInt32(parentlabel.Replace(Common.ProgramCustomTitle, ""));
                IsProgramCustomField = true;
            }
            else
            {
                Tacticdata = Tacticdata.ToList();
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

            //Check the custom field type and filter tactic based on Custom field Id
            else if (parentlabel.Contains(Common.TacticCustomTitle) || parentlabel.Contains(Common.CampaignCustomTitle) || parentlabel.Contains(Common.ProgramCustomTitle))
            {

                List<int> entityids = new List<int>();
                if (IsTacticCustomField)
                {
                    entityids = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
                }
                else if (IsCampaignCustomField)
                {
                    entityids = Tacticdata.Select(t => t.TacticObj.Plan_Campaign_Program.PlanCampaignId).ToList();
                }
                else
                {
                    entityids = Tacticdata.Select(t => t.TacticObj.PlanProgramId).ToList();
                }

                customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();
                if (customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                {
                    var optionlist = cusomfieldEntity.Select(c => Convert.ToInt32(c.Value)).ToList();
                    campaignList = (from cfo in db.CustomFieldOptions
                                    where cfo.CustomFieldId == customfieldId && optionlist.Contains(cfo.CustomFieldOptionId) && cfo.IsDeleted == false
                                    select cfo).ToList().GroupBy(pc => new { id = pc.CustomFieldOptionId, title = pc.Value }).Select(pc =>
                                  new RevenueContrinutionData
                                  {
                                      Title = pc.Key.title,
                                      CustomFieldOptionid = pc.Key.id,
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

                List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));

                List<string> monthWithYearList = GetUpToCurrentMonthWithYearForReport(selectOption, true);

                List<int> ObjTactic = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
                List<Plan_Campaign_Program_Tactic_LineItem> LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => ObjTactic.Contains(l.PlanTacticId) && l.IsDeleted == false).ToList();
                List<int> ObjTacticLineItemList = LineItemList.Select(t => t.PlanLineItemId).ToList();
                List<Plan_Campaign_Program_Tactic_LineItem_Actual> LineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(l => ObjTacticLineItemList.Contains(l.PlanLineItemId)).ToList();

                var campaignListFinal = campaignList.Select(p => new
                {
                    Title = p.Title,
                    PlanRevenue = GetPlanValue(customfieldId, p.CustomFieldOptionid.ToString(), Tacticdata.Where(t => p.planTacticList.Contains(t.TacticObj.PlanTacticId)).ToList(), customFieldType, includeMonth, Enums.InspectStage.Revenue, IsTacticCustomField),
                    ActualRevenue = GetActualRevenueContribution(customfieldId, p.CustomFieldOptionid.ToString(), Tacticdata, p.planTacticList, customFieldType, includeMonth, IsTacticCustomField),
                    TrendRevenue = 0,
                    PlanCost = GetPlanValue(customfieldId, p.CustomFieldOptionid.ToString(), Tacticdata.Where(t => p.planTacticList.Contains(t.TacticObj.PlanTacticId)).ToList(), customFieldType, includeMonth, Enums.InspectStage.Cost, IsTacticCustomField),
                    //Added By : Kalpesh Sharma #734 Actual cost - Verify that report section is up to date with actual cost changes
                    ActualCost = GetActualCostDataByWeightage(customfieldId, p.CustomFieldOptionid.ToString(), customFieldType, Tacticdata, LineItemList, LineItemActualList, IsTacticCustomField).Where(mr => p.planTacticList.Contains(mr.Id) && includeMonth.Contains(mr.Month)).Sum(r => r.Value),
                    TrendCost = 0,
                    RunRate = GetTrendRevenueDataContribution(customfieldId, p.CustomFieldOptionid.ToString(), customFieldType, ActualTacticList, lastMonth, monthList, Tacticdata, IsTacticCustomField).Where(ar => p.planTacticList.Contains(ar.PlanTacticId)).Sum(ar => ar.MQL),
                    PipelineCoverage = 0,
                    RevSpend = GetRevenueVSSpendContribution(customfieldId, p.CustomFieldOptionid.ToString(), customFieldType, ActualTacticList, p.planTacticList, monthWithYearList, monthList, revenue, Tacticdata, LineItemList, LineItemActualList, IsTacticCustomField),
                    RevenueTotal = GetActualRevenueTotal(customfieldId, p.CustomFieldOptionid.ToString(), customFieldType, ActualTacticList, p.planTacticList, monthList, revenue, Tacticdata, IsTacticCustomField),
                    CostTotal = GetActualCostDataByWeightage(customfieldId, p.CustomFieldOptionid.ToString(), customFieldType, Tacticdata, LineItemList, LineItemActualList, IsTacticCustomField).Where(mr => p.planTacticList.Contains(mr.Id) && monthWithYearList.Contains(mr.Month)).Sum(r => r.Value)
                }).Select(p => p).Distinct().OrderBy(p => p.Title);

                return Json(campaignListFinal, JsonRequestBehavior.AllowGet);
            }
            return Json(new { });
        }


        private List<TacticMonthValue> GetActualCostDataByWeightage(int CustomFieldId, string CustomFieldOptionId, string CustomFieldType, List<TacticStageValue> Tacticdata, List<Plan_Campaign_Program_Tactic_LineItem> LineItemList, List<Plan_Campaign_Program_Tactic_LineItem_Actual> LineItemActualList, bool IsTacticCustomField)
        {
            List<TacticMonthValue> listmonthwise = new List<TacticMonthValue>();
            List<ActualDataTable> ActualData = new List<ActualDataTable>();
            foreach (var tactic in Tacticdata)
            {
                int id = tactic.TacticObj.PlanTacticId;
                var InnerLineItemList = LineItemList.Where(l => l.PlanTacticId == id).ToList();
                if (InnerLineItemList.Count() > 0)
                {
                    List<Plan_Campaign_Program_Tactic_LineItem_Actual> innerLineItemActualList = LineItemActualList.Where(la => InnerLineItemList.Select(line => line.PlanLineItemId).Contains(la.PlanLineItemId)).ToList();
                    ActualData = GetCostLineItemActualListbyWeightage(CustomFieldId, CustomFieldOptionId, CustomFieldType, id, innerLineItemActualList, tactic, IsTacticCustomField);
                    ActualData.ForEach(innerline => listmonthwise.Add(new TacticMonthValue { Id = id, Month = tactic.TacticYear + innerline.Period, Value = innerline.ActualValue }));
                }
                else
                {
                    List<Plan_Campaign_Program_Tactic_Actual> innerTacticActualList = tactic.ActualTacticList.Where(actualTac => actualTac.StageTitle == Enums.InspectStage.Cost.ToString()).ToList();
                    ActualData = GetActualTacticDataTablebyStageCode(CustomFieldId, CustomFieldOptionId, CustomFieldType, Enums.InspectStage.Cost, innerTacticActualList, Tacticdata, IsTacticCustomField);
                    ActualData.ForEach(actual => listmonthwise.Add(new TacticMonthValue { Id = id, Month = tactic.TacticYear + actual.Period, Value = actual.ActualValue }));
                }
            }
            return listmonthwise;
        }

        /// <summary>
        /// Get Actual Cost Data With Month Wise Without Weightage.
        /// Added By: Viral Kadiya
        /// </summary>
        /// <param name="Tacticdata"></param>
        /// <param name="LineItemActualList"></param>
        /// <param name="LineItemList"></param>
        /// <returns> Return Tactic Projected MonthWise Cost Data </returns>
        private List<TacticMonthValue> GetActualCostData(List<TacticStageValue> Tacticdata, List<Plan_Campaign_Program_Tactic_LineItem> LineItemList, List<Plan_Campaign_Program_Tactic_LineItem_Actual> LineItemActualList)
        {
            List<TacticMonthValue> listmonthwise = new List<TacticMonthValue>();
            List<ActualDataTable> ActualData = new List<ActualDataTable>();
            List<Plan_Campaign_Program_Tactic_LineItem_Actual> innerLineItemActualList = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
            foreach (var tactic in Tacticdata)
            {
                int id = tactic.TacticObj.PlanTacticId;
                var InnerLineItemList = LineItemList.Where(l => l.PlanTacticId == id).ToList();
                if (InnerLineItemList.Count() > 0)
                {
                    innerLineItemActualList = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                    innerLineItemActualList = LineItemActualList.Where(la => InnerLineItemList.Select(line => line.PlanLineItemId).Contains(la.PlanLineItemId)).ToList();
                    innerLineItemActualList.ForEach(innerline => listmonthwise.Add(new TacticMonthValue { Id = id, Month = tactic.TacticYear + innerline.Period, Value = innerline.Value }));
                }
                else
                {
                    List<Plan_Campaign_Program_Tactic_Actual> innerTacticActualList = tactic.ActualTacticList.Where(actualTac => actualTac.StageTitle == Enums.InspectStage.Cost.ToString()).ToList();
                    innerTacticActualList.ForEach(actual => listmonthwise.Add(new TacticMonthValue { Id = id, Month = tactic.TacticYear + actual.Period, Value = actual.Actualvalue }));
                }
            }
            return listmonthwise;
        }

        /// <summary>
        /// Get Projected Cost Data With Month Wise.
        /// Added By: Viral Kadiya
        /// </summary>
        /// <param name="CustomFieldId"></param>
        /// <param name="CustomFieldOptionId"></param>
        /// <param name="CustomFieldType"></param>
        /// <param name="Tacticdata"></param>
        /// <returns> Return Tactic Projected MonthWise Cost Data </returns>
        private List<TacticMonthValue> GetProjectedCostData(int CustomFieldId, string CustomFieldOptionId, string CustomFieldType, List<TacticStageValue> Tacticdata, bool IsTacticCustomField, List<Plan_Campaign_Program_Tactic_LineItem> lstTacticLineItem, List<Plan_Campaign_Program_Tactic_LineItem_Cost> tblLineItemCost, List<Plan_Campaign_Program_Tactic_Cost> tblTacticCostList)
        {
            #region "Declare Local variables"
            List<TacticMonthValue> listmonthwise = new List<TacticMonthValue>();
        //    List<Plan_Campaign_Program_Tactic_LineItem> lstTacticLineItem = new List<Plan_Campaign_Program_Tactic_LineItem>();
       //     List<Plan_Campaign_Program_Tactic_LineItem_Cost> tblLineItemCost = new List<Plan_Campaign_Program_Tactic_LineItem_Cost>();
       //     List<Plan_Campaign_Program_Tactic_Cost> tblTacticCostList = new List<Plan_Campaign_Program_Tactic_Cost>();
            List<Plan_Campaign_Program_Tactic_Cost> TacticCostList = new List<Plan_Campaign_Program_Tactic_Cost>();
            List<int> lstTacticIds = new List<int>();
            List<Enums.InspectStage> CostStageCode = new List<Enums.InspectStage>();
            CostStageCode.Add(Enums.InspectStage.Cost);
            #endregion

        //    lstTacticIds = Tacticdata.Select(tac => tac.TacticObj.PlanTacticId).ToList();
        //    lstTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(line => lstTacticIds.Contains(line.PlanTacticId) && line.IsDeleted == false).ToList();
        //    tblTacticCostList = db.Plan_Campaign_Program_Tactic_Cost.Where(line => lstTacticIds.Contains(line.PlanTacticId)).ToList();
        //    tblLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(line => lstTacticLineItem.Select(ln => ln.PlanLineItemId).Contains(line.PlanLineItemId)).ToList();

            //// Get TacticMonth value for each Tactic.
            foreach (TacticStageValue tactic in Tacticdata)
            {
                int PlanTacticId = tactic.TacticObj.PlanTacticId;
                var InnerLineItemList = lstTacticLineItem.Where(l => l.PlanTacticId == PlanTacticId).Select(l => l.PlanLineItemId).ToList();
                TacticCostList = tblTacticCostList.Where(tacCost => tacCost.PlanTacticId == PlanTacticId).ToList();
                string Period = string.Empty;
                double lineTotalValue = 0, TacticTotalValue = 0;
                int? weightage = 0;

                //// Get Tactic weightage if CustomFieldType is Dropdownlist o/w take 100 in Textbox.
                if (CustomFieldId != 0 && !string.IsNullOrEmpty(CustomFieldType) && CustomFieldType.Equals(Enums.CustomFieldType.DropDownList.ToString()) && IsTacticCustomField)
                {
                    TacticCustomFieldStageWeightage objTacticStageWeightage = new TacticCustomFieldStageWeightage();
                    objTacticStageWeightage = tactic.TacticStageWeightages.Where(_stage => _stage.CustomFieldId.Equals(CustomFieldId) && _stage.Value.Equals(CustomFieldOptionId)).FirstOrDefault();

                    if (objTacticStageWeightage != null)
                        weightage = objTacticStageWeightage.CostWeightage;
                    weightage = weightage != null ? weightage.Value : 0;
                }
                else
                    weightage = 100;

                //// if LineItem exist for this Tactic then check sum of LineItemCost with TacticCost.
                if (InnerLineItemList.Count() > 0)
                {
                    //// Get sum of LineItemCost based on LineItemID.
                    lineTotalValue = tblLineItemCost.Where(lineCost => InnerLineItemList.Contains(lineCost.PlanLineItemId)).Select(lineCost => lineCost.Value).Sum(r => r);
                    //// Get sum of TacticCost based on PlanTacticId.
                    TacticTotalValue = TacticCostList.Select(lineCost => lineCost.Value).Sum(r => r);

                    //// if sum of LineItemCost greater than TacticCost then retrieve TacticMonth value from LineItemCost o/w TacticCost.
                    if (lineTotalValue > TacticTotalValue)
                        tblLineItemCost.ForEach(lineCost => listmonthwise.Add(new TacticMonthValue { Id = PlanTacticId, Month = tactic.TacticYear + lineCost.Period, Value = (lineCost.Value * weightage.Value) / 100 }));
                    else
                        TacticCostList.ForEach(tacCost => listmonthwise.Add(new TacticMonthValue { Id = PlanTacticId, Month = tactic.TacticYear + tacCost.Period, Value = (tacCost.Value * weightage.Value) / 100 }));
                }
                else
                {
                    //// LineItem does not exist then retrieve TacticMonth value from TacticCost table.
                    TacticCostList.ForEach(tacCost => listmonthwise.Add(new TacticMonthValue { Id = PlanTacticId, Month = tactic.TacticYear + tacCost.Period, Value = (tacCost.Value * weightage.Value) / 100 }));
                }
            }
            return listmonthwise;
        }

        /// <summary>
        /// Get Trend.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public List<Plan_Tactic_Values> GetTrendRevenueDataContribution(int CustomFieldId, string CustomFieldOptionId, string CustomFieldType, List<Plan_Campaign_Program_Tactic_Actual> planActualTacticList, int lastMonth, List<string> monthList, List<TacticStageValue> TacticData, bool IsTacticCustomField)
        {
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            ActualTacticList = planActualTacticList.Where(tactic => monthList.Contains(tactic.Period) && tactic.StageTitle == revenue).ToList();
            List<ActualDataTable> lstActualDataTable = new List<ActualDataTable>();
            lstActualDataTable = GetActualTacticDataTablebyStageCode(CustomFieldId, CustomFieldOptionId, CustomFieldType, Enums.InspectStage.Revenue, ActualTacticList, TacticData, IsTacticCustomField);

            return lstActualDataTable.GroupBy(_tac => _tac.PlanTacticId).Select(pt => new Plan_Tactic_Values
            {
                PlanTacticId = pt.Key,
                MQL = (pt.Sum(a => a.ActualValue) / currentMonth) * lastMonth
            }).ToList();
        }

        /// <summary>
        /// Get Trend.
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public double GetRevenueVSSpendContribution(int CustomFieldId, string CustomFieldOptionId, string customFieldType, List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList, List<int> planTacticList, List<string> monthWithYearList, List<string> monthList, string revenue, List<TacticStageValue> TacticData, List<Plan_Campaign_Program_Tactic_LineItem> LineItemList, List<Plan_Campaign_Program_Tactic_LineItem_Actual> LineItemActualList, bool IsTacticCustomField)
        {
            List<TacticMonthValue> lstMonthData = new List<TacticMonthValue>();
            List<Plan_Campaign_Program_Tactic_Actual> lstActualsTacticData = new List<Plan_Campaign_Program_Tactic_Actual>();
            lstMonthData = GetActualCostDataByWeightage(CustomFieldId, CustomFieldOptionId, customFieldType, TacticData, LineItemList, LineItemActualList, IsTacticCustomField);
            lstActualsTacticData = ActualTacticList.Where(actualTac => planTacticList.Contains(actualTac.PlanTacticId) && monthList.Contains(actualTac.Period) && actualTac.StageTitle == revenue).ToList();
            List<ActualDataTable> lstActualDataTable = new List<ActualDataTable>();
            lstActualDataTable = GetActualTacticDataTablebyStageCode(CustomFieldId, CustomFieldOptionId, customFieldType, Enums.InspectStage.Revenue, lstActualsTacticData, TacticData, IsTacticCustomField);

            double costTotal = lstMonthData.Where(actual => planTacticList.Contains(actual.Id) && monthWithYearList.Contains(actual.Month)).Sum(actual => actual.Value);
            double revenueTotal = lstActualDataTable.Sum(actual => actual.ActualValue);
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
        public double GetActualRevenueTotal(int CustomFieldId, string CustomFieldOptionId, string customFieldType, List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList, List<int> planTacticList, List<string> monthList, string revenue, List<TacticStageValue> TacticData, bool IsTacticCustomField)
        {
            List<Plan_Campaign_Program_Tactic_Actual> lstActualsTacticData = new List<Plan_Campaign_Program_Tactic_Actual>();
            lstActualsTacticData = ActualTacticList.Where(ta => planTacticList.Contains(ta.PlanTacticId) && monthList.Contains(ta.Period) && ta.StageTitle == revenue).ToList();
            List<ActualDataTable> lstActualDataTable = new List<ActualDataTable>();
            lstActualDataTable = GetActualTacticDataTablebyStageCode(CustomFieldId, CustomFieldOptionId, customFieldType, Enums.InspectStage.Revenue, ActualTacticList, TacticData, IsTacticCustomField);
            return lstActualDataTable.Where(ta => planTacticList.Contains(ta.PlanTacticId) && monthList.Contains(ta.Period) && ta.StageTitle == revenue).ToList().Sum(a => a.ActualValue);
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
            int customfieldId = 0;
            bool IsTacticCustomField = false;

            //Custom
            List<int> entityids = new List<int>();
            if (ParentLabel.Contains(Common.TacticCustomTitle))
            {
                IsTacticCustomField = true;
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

            if (Tacticdata.Count > 0)
            {
                string CustomFieldType = string.Empty;
                string stageTitleRevenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
                List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                Tacticdata.ForEach(t => t.ActualTacticList.Where(pcpta => pcpta.StageTitle == stageTitleRevenue).ToList().ForEach(a => ActualTacticList.Add(a)));
                CustomFieldType = db.CustomFields.Where(custm => custm.CustomFieldId.Equals(customfieldId)).Select(custm => custm.CustomFieldType.Name).FirstOrDefault();

                List<string> includeMonth = GetMonthListForReport(selectOption, true);
                DataTable dtActualRevenue = GetActualRevenue(customfieldId, id, CustomFieldType, Enums.InspectStage.Revenue, ActualTacticList, Tacticdata, IsTacticCustomField);
                DataTable dtProjectedRevenue = GetProjectedRevenue(customfieldId, id, CustomFieldType, Enums.InspectStage.Revenue, Tacticdata, includeMonth, selectOption, IsTacticCustomField);
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
        private DataTable GetProjectedRevenue(int CustomFieldId, string CustomFieldOptionId, string CustomFieldType, Enums.InspectStage stagecode, List<TacticStageValue> TacticList, List<string> monthList, string selectOption, bool IsTacticCustomField)
        {
            bool IsVelocity = true;
            List<TacticDataTable> tacticdata = GetTacticDataTablebyStageCode(CustomFieldId, CustomFieldOptionId, CustomFieldType, stagecode, TacticList, IsTacticCustomField, IsVelocity);
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
        private DataTable GetActualRevenue(int CustomFieldId, string CustomFieldOptionId, string customfieldType, Enums.InspectStage stagecode, List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList, List<TacticStageValue> TacticData, bool IsTacticCustomField)
        {
            List<ActualDataTable> ActualRevenueDataTable = GetActualTacticDataTablebyStageCode(CustomFieldId, CustomFieldOptionId, customfieldType, Enums.InspectStage.Revenue, ActualTacticList, TacticData, IsTacticCustomField);
            //// Getting period wise actual revenue of non-deleted and approved/in-progress/complete tactic of plan present in planids.
            var planCampaignTacticActualAll = ActualRevenueDataTable.GroupBy(pcpta => pcpta.Period).Select(group => new { Period = group.Key, ActualValue = group.Sum(pcpta => pcpta.ActualValue) }).OrderBy(pcpta => pcpta.Period);

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

            string tactic = Enums.EntityType.Tactic.ToString();
            string dropdownType = Enums.CustomFieldType.DropDownList.ToString();
            string Revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();

            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            Tacticdata.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));
            //Start : Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 71.Add condition for isDeleted flag  
            ActualTacticList = ActualTacticList.Where(mr => mr.StageTitle == Revenue && includeMonthUpCurrent.Contains((Tacticdata.FirstOrDefault(t => t.TacticObj.PlanTacticId == mr.PlanTacticId).TacticYear) + mr.Period)).ToList();


            int dropdwnCustomFieldTypeId = db.CustomFieldTypes.Where(custType => custType.Name.Equals(dropdownType)).Select(custType => custType.CustomFieldTypeId).FirstOrDefault();

            // Get first 3 custom fields
            var customFields = db.CustomFields.Where(c => c.ClientId.Equals(Sessions.User.ClientId)
                && c.IsDeleted == false && c.IsRequired == true && c.IsDefault == true && c.EntityType == tactic && c.CustomFieldTypeId == dropdwnCustomFieldTypeId
                ).Select(c => new
                {
                    CustomFieldId = c.CustomFieldId,
                    CustomFieldName = c.Name
                }).Take(3).ToList();
            List<CustomFieldOption> tblCustomFieldOption = db.CustomFieldOptions.ToList().Where(co => customFields.Select(custm => custm.CustomFieldId).Contains(co.CustomFieldId) && co.IsDeleted == false).ToList();

            //// Start - Added by Arpita Soni for Ticket #1148 on 01/23/2015



            customFields = customFields.Where(sort => !string.IsNullOrEmpty(sort.CustomFieldName)).OrderBy(sort => sort.CustomFieldName, new AlphaNumericComparer()).ToList();
            List<ListSourcePerformanceData> lstListSourcePerformance = new List<ListSourcePerformanceData>();
            List<string> lstCustomFieldNames = new List<string>();
            string dropdwnCustomFieldType = Enums.CustomFieldType.DropDownList.ToString();
            // Applying custom field filters 
            foreach (var customfield in customFields)
            {
                List<SourcePerformanceData> lstSourcePerformance = new List<SourcePerformanceData>();

                lstSourcePerformance = tblCustomFieldOption.Where(s => s.CustomFieldId == customfield.CustomFieldId).ToList().Select(s => new SourcePerformanceData
                {
                    Title = s.Value,
                    ColorCode = string.Format("#{0}", s.ColorCode),
                    Value = GetActualVSPlannedRevenue(ActualTacticList, customfield.CustomFieldId, s.CustomFieldOptionId.ToString(), dropdwnCustomFieldType, Tacticdata, Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList(), includeMonthUpCurrent, true)
                }).OrderByDescending(s => s.Value).ThenBy(s => s.Title).Take(5).ToList();

                lstCustomFieldNames.Add(customfield.CustomFieldName);
                lstListSourcePerformance.Add(new ListSourcePerformanceData
                {
                    lstSourcePerformanceData = lstSourcePerformance,
                    CustomFieldName = customfield.CustomFieldName

                });
            }
            //// End - Added by Arpita Soni for Ticket #1148 on 01/23/2015
            // Modified by Arpita Soni for Ticket #1148 on 01/27/2015
            return Json(new
            {
                ChartCustomField1 = lstListSourcePerformance.Count > 0 ? lstListSourcePerformance.ElementAt(0).lstSourcePerformanceData : null,
                ChartCustomField2 = lstListSourcePerformance.Count > 1 ? lstListSourcePerformance.ElementAt(1).lstSourcePerformanceData : null,
                ChartCustomField3 = lstListSourcePerformance.Count > 2 ? lstListSourcePerformance.ElementAt(2).lstSourcePerformanceData : null,
                CustomField1 = lstCustomFieldNames.Count > 0 ? lstCustomFieldNames.ElementAt(0) : null,
                CustomField2 = lstCustomFieldNames.Count > 1 ? lstCustomFieldNames.ElementAt(1) : null,
                CustomField3 = lstCustomFieldNames.Count > 2 ? lstCustomFieldNames.ElementAt(2) : null
            }, JsonRequestBehavior.AllowGet);
        }

        public class ListSourcePerformanceData
        {
            public List<SourcePerformanceData> lstSourcePerformanceData { get; set; }
            public string CustomFieldName { get; set; }
        }

        public class SourcePerformanceData
        {
            public string Title { get; set; }
            public string ColorCode { get; set; }
            public double Value { get; set; }
        }

        public class RevenueContributionModel
        {
            public string Title { get; set; }
            public double PlanRevenue { get; set; }
            public double ActualRevenue { get; set; }
            public double TrendRevenue { get; set; }
            public double PlanCost { get; set; }
            public double ActualCost { get; set; }
            public double TrendCost { get; set; }
            public double RunRate { get; set; }
            public double PipelineCoverage { get; set; }
            public double RevSpend { get; set; }
            public double CostTotal { get; set; }
            public double RevenueTotal { get; set; }
        }

        /// <summary>
        /// Get Actual vs Planned revenue difference %.
        /// Added By Bhavesh : PL Ticket  #349 - 16/4/2014
        /// </summary>
        /// <param name="tacticIds"></param>
        /// <param name="includeMonthUpCurrent"></param>
        /// <returns></returns>
        private double GetActualVSPlannedRevenue(List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList, int CustomFieldId, string CustomFieldOptionId, string customfieldType, List<TacticStageValue> TacticData, List<int> tacticIds, List<string> includeMonthUpCurrent, bool IsTacticCustomField)
        {
            double actualRevenueValue = 0, percentageValue = 0;
            if (tacticIds.Count() == 0)
                return percentageValue;
            List<TacticMonthValue> ProjectedRevenueDataTable = GetProjectedDatabyStageCode(CustomFieldId, CustomFieldOptionId, customfieldType, Enums.InspectStage.Revenue, TacticData, IsTacticCustomField);
            List<Plan_Campaign_Program_Tactic_Actual> lstActualTacticlist = ActualTacticList.Where(pcpt => tacticIds.Contains(pcpt.PlanTacticId)).ToList();
            List<ActualDataTable> ActualData = new List<ActualDataTable>();
            ActualData = GetActualTacticDataTablebyStageCode(CustomFieldId, CustomFieldOptionId, customfieldType, Enums.InspectStage.Revenue, lstActualTacticlist, TacticData, IsTacticCustomField);
            if (ActualData.Count() > 0)
            {
                actualRevenueValue = ActualData.Sum(a => a.ActualValue);
            }
            double projectedRevenueValue = ProjectedRevenueDataTable.Where(mr => tacticIds.Contains(mr.Id) && includeMonthUpCurrent.Contains(mr.Month)).Sum(mr => mr.Value);
            ////Start - Modified by Mitesh Vaishnav for PL ticket #611 Source Performance Graphs dont show anything
            if (projectedRevenueValue != 0)
            {
                percentageValue = Math.Round((actualRevenueValue / projectedRevenueValue) * 100, 2);
            }
            ////End - Modified by Mitesh Vaishnav for PL ticket #611 Source Performance Graphs dont show anything
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
                var individuals = bdsUserRepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, true);

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
                htmlOfCurrentView = htmlOfCurrentView.Replace("class=\"percentageFormat\"", "");
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

            ////Start - Modified by Mitesh Vaishnav for PL ticket #831
            var campaignProgramList = db.Plan_Campaign_Program_Tactic.Where(tactic => TacticId.Contains(tactic.PlanTacticId)).ToList();
            List<int> campaignlist = campaignProgramList.Select(campaign => campaign.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = campaignProgramList.Select(program => program.PlanProgramId).ToList();

            lstViewByTab = lstViewByTab.Where(modal => !string.IsNullOrEmpty(modal.Text)).ToList();
            var lstCustomFields = Common.GetCustomFields(TacticId, programlist, campaignlist);
            lstViewByTab = lstViewByTab.Concat(lstCustomFields).ToList();
            ////End - Modified by Mitesh Vaishnav for PL ticket #831
            ViewBag.ViewByTab = lstViewByTab;

            //// Set View By Allocated values.
            List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
            lstViewByAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarterly", Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstViewByAllocated = lstViewByAllocated.Where(modal => !string.IsNullOrEmpty(modal.Text)).ToList();
            ViewBag.ViewByAllocated = lstViewByAllocated;

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
        public ActionResult GetReportBudgetData(string Year, string AllocatedBy, string Tab, string SortingId)
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
            string EntTacticType = Enums.EntityType.Tactic.ToString();

            List<Plan_Campaign_Program_Tactic> tacticList = new List<Plan_Campaign_Program_Tactic>();
            tacticList = GetTacticForReporting(true);

            //// load Filter lists.
            List<int> TacticIds = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
            var LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => TacticIds.Contains(lineitem.PlanTacticId) && lineitem.IsDeleted.Equals(false)).ToList();
            List<int> ProgramIds = tacticList.Select(tactic => tactic.PlanProgramId).ToList();
            var ProgramList = db.Plan_Campaign_Program.Where(program => ProgramIds.Contains(program.PlanProgramId) && program.IsDeleted.Equals(false)).ToList();
            List<int> CampaignIds = ProgramList.Select(tactic => tactic.PlanCampaignId).ToList();
            var CampaignList = db.Plan_Campaign.Where(campaign => CampaignIds.Contains(campaign.PlanCampaignId) && campaign.IsDeleted.Equals(false)).ToList();
            List<int> PlanIdsInner = CampaignList.Select(campaign => campaign.PlanId).ToList();

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
            bool IsCustomFieldViewBy = false;
            if (Tab == ReportTabType.Plan.ToString())
            {

                #region "Plan Model"
                var planobj = db.Plans.Where(plan => PlanIdsInner.Contains(plan.PlanId)).ToList();
                if (planobj != null)
                {
                    var planbudgetlist = db.Plan_Budget.Where(pb => PlanIdsInner.Contains(pb.PlanId)).ToList();
                    var campaignbudgetlist = db.Plan_Campaign_Budget.Where(pcb => CampaignIds.Contains(pcb.PlanCampaignId)).ToList();
                    var programbudgetlist = db.Plan_Campaign_Program_Budget.Where(pcpb => ProgramIds.Contains(pcpb.PlanProgramId)).ToList();
                    var tacticcostlist = db.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => TacticIds.Contains(pcptc.PlanTacticId)).ToList();
                    List<Plan_Campaign> campaignObj;
                    List<Plan_Campaign_Program> ProgramObj;
                    List<Plan_Campaign_Program_Tactic> TacticObj;
                    List<Plan_Campaign_Program_Tactic_LineItem> LineItemObj;
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
                        campaignObj = new List<Plan_Campaign>();
                        campaignObj = CampaignList.Where(campaign => campaign.PlanId == p.PlanId).OrderBy(campaign => campaign.Title).ToList();
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
                            ProgramObj = new List<Plan_Campaign_Program>();
                            ProgramObj = ProgramList.Where(program => program.PlanCampaignId == c.PlanCampaignId).OrderBy(program => program.Title).ToList();
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
                                TacticObj = new List<Plan_Campaign_Program_Tactic>();
                                TacticObj = tacticList.Where(tactic => tactic.PlanProgramId == pr.PlanProgramId).OrderBy(tactic => tactic.Title).ToList();
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
                                    obj = GetMonthWiseDataReport(obj, t.Plan_Campaign_Program_Tactic_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Allocated.ToString());
                                    model.Add(obj);
                                    parentTacticId = "cpt_" + t.PlanTacticId.ToString();

                                    LineItemObj = new List<Plan_Campaign_Program_Tactic_LineItem>();
                                    LineItemObj = LineItemList.Where(line => line.PlanTacticId == t.PlanTacticId).OrderBy(l => l.Title).ToList();
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
                #endregion
            }
            else
            {
                int customfieldId = 0;
                List<BudgetReportTab> planobj = new List<BudgetReportTab>();
                IsCustomFieldViewBy = true;
                string customFieldType = string.Empty;
                if (Tab.Contains(Common.TacticCustomTitle))
                    customfieldId = Convert.ToInt32(Tab.Replace(Common.TacticCustomTitle, ""));
                else if (Tab.Contains(Common.ProgramCustomTitle))
                    customfieldId = Convert.ToInt32(Tab.Replace(Common.ProgramCustomTitle, ""));
                else if (Tab.Contains(Common.CampaignCustomTitle))
                    customfieldId = Convert.ToInt32(Tab.Replace(Common.CampaignCustomTitle, ""));

                customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                planobj = GetCustomFieldOptionMappinglist(Tab, customfieldId, customFieldType, tacticList);

                if (planobj != null)
                {
                    List<Plan_Campaign_Program_Tactic> TacticListInner, TacticObj;
                    List<Plan_Campaign_Program> ProgramListInner, ProgramObj;
                    List<Plan_Campaign> campaignObj;
                    List<CustomField_Entity> cusomfieldEntity;
                    List<Plan_Campaign_Program_Tactic_LineItem> LineItemObj;
                    foreach (var p in planobj)
                    {
                        TacticListInner = new List<Plan_Campaign_Program_Tactic>();
                        TacticListInner = tacticList;

                        if (Tab.Contains(Common.TacticCustomTitle))
                        {
                            cusomfieldEntity = new List<CustomField_Entity>();
                            cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && c.Value == p.Id).ToList();
                            List<int> entityids = cusomfieldEntity.Select(e => e.EntityId).ToList();
                            TacticListInner = tacticList.Where(tactic => entityids.Contains(tactic.PlanTacticId)).ToList();
                            p.Id = p.Id.Replace(' ', '_').Replace('#', '_').Replace('-', '_');
                        }
                        ////Start - Added by Mitesh Vaishnav for PL ticket #831
                        else if (Tab.Contains(Common.ProgramCustomTitle))
                        {
                            cusomfieldEntity = new List<CustomField_Entity>();
                            cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && c.Value == p.Id).ToList();
                            List<int> entityids = cusomfieldEntity.Select(e => e.EntityId).ToList();
                            TacticListInner = tacticList.Where(tactic => entityids.Contains(tactic.PlanProgramId)).ToList();
                            p.Id = p.Id.Replace(' ', '_').Replace('#', '_').Replace('-', '_');
                        }
                        else if (Tab.Contains(Common.CampaignCustomTitle))
                        {
                            cusomfieldEntity = new List<CustomField_Entity>();
                            cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && c.Value == p.Id).ToList();
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

                        ProgramListInner = new List<Plan_Campaign_Program>();
                        campaignObj = new List<Plan_Campaign>();
                        ProgramListInner = ProgramList.Where(program => TacticListInner.Select(t => t.PlanProgramId).Contains(program.PlanProgramId)).ToList();
                        campaignObj = CampaignList.Where(campaign => ProgramListInner.Select(program => program.PlanCampaignId).Contains(campaign.PlanCampaignId)).OrderBy(campaign => campaign.Title).ToList();

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
                            ProgramObj = new List<Plan_Campaign_Program>();
                            ProgramObj = ProgramListInner.Where(pr => pr.PlanCampaignId == c.PlanCampaignId).OrderBy(pr => pr.Title).ToList();
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
                                TacticObj = new List<Plan_Campaign_Program_Tactic>();
                                TacticObj = TacticListInner.Where(t => t.PlanProgramId == pr.PlanProgramId).OrderBy(t => t.Title).ToList();
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
                                    obj = GetMonthWiseDataReport(obj, t.Plan_Campaign_Program_Tactic_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(), ReportColumnType.Allocated.ToString());
                                    obj.CustomFieldType = customFieldType;
                                    model.Add(obj);
                                    parentTacticId = "cpt_" + p.Id + t.PlanTacticId.ToString();

                                    LineItemObj = new List<Plan_Campaign_Program_Tactic_LineItem>();
                                    LineItemObj = LineItemList.Where(l => l.PlanTacticId == t.PlanTacticId).OrderBy(l => l.Title).ToList();
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

            model = SetTacticWeightage(model, IsCustomFieldViewBy);
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

                        if (bm.ActivityType == ActivityType.ActivityTactic)
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


            model = CalculateBottomUpReport(model, ActivityType.ActivityTactic, ActivityType.ActivityLineItem, IsCustomFieldViewBy);
            model = CalculateBottomUpReport(model, ActivityType.ActivityProgram, ActivityType.ActivityTactic, IsCustomFieldViewBy);
            model = CalculateBottomUpReport(model, ActivityType.ActivityCampaign, ActivityType.ActivityProgram, IsCustomFieldViewBy);
            model = CalculateBottomUpReport(model, ActivityType.ActivityPlan, ActivityType.ActivityCampaign, IsCustomFieldViewBy);
            model = CalculateBottomUpReport(model, ActivityType.ActivityMain, ActivityType.ActivityPlan, IsCustomFieldViewBy);

            //// Set LineItem monthly budget cost by it's parent tactic weightage.
            model = SetLineItemCostByWeightage(model, IsCustomFieldViewBy);

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
            month.Jan = lst.Where(budgt => budgt.Period.ToUpper() == Common.Jan).Select(budgt => budgt.Value).FirstOrDefault();
            month.Feb = lst.Where(budgt => budgt.Period.ToUpper() == Common.Feb).Select(budgt => budgt.Value).FirstOrDefault();
            month.Mar = lst.Where(budgt => budgt.Period.ToUpper() == Common.Mar).Select(budgt => budgt.Value).FirstOrDefault();
            month.Apr = lst.Where(budgt => budgt.Period.ToUpper() == Common.Apr).Select(budgt => budgt.Value).FirstOrDefault();
            month.May = lst.Where(budgt => budgt.Period.ToUpper() == Common.May).Select(budgt => budgt.Value).FirstOrDefault();
            month.Jun = lst.Where(budgt => budgt.Period.ToUpper() == Common.Jun).Select(budgt => budgt.Value).FirstOrDefault();
            month.Jul = lst.Where(budgt => budgt.Period.ToUpper() == Common.Jul).Select(budgt => budgt.Value).FirstOrDefault();
            month.Aug = lst.Where(budgt => budgt.Period.ToUpper() == Common.Aug).Select(budgt => budgt.Value).FirstOrDefault();
            month.Sep = lst.Where(budgt => budgt.Period.ToUpper() == Common.Sep).Select(budgt => budgt.Value).FirstOrDefault();
            month.Oct = lst.Where(budgt => budgt.Period.ToUpper() == Common.Oct).Select(budgt => budgt.Value).FirstOrDefault();
            month.Nov = lst.Where(budgt => budgt.Period.ToUpper() == Common.Nov).Select(budgt => budgt.Value).FirstOrDefault();
            month.Dec = lst.Where(budgt => budgt.Period.ToUpper() == Common.Dec).Select(budgt => budgt.Value).FirstOrDefault();

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
        public List<BudgetModelReport> CalculateBottomUpReport(List<BudgetModelReport> model, string ParentActivityType, string ChildActivityType, bool IsCustomFieldViewBy)
        {
            if (ParentActivityType == ActivityType.ActivityTactic)
            {
                int weightage = 100;
                List<BudgetModelReport> LineCheck;
                BudgetMonth parent, parentActual, parentAllocated;
                foreach (BudgetModelReport l in model.Where(l => l.ActivityType == ParentActivityType))
                {
                    if (IsCustomFieldViewBy)
                        weightage = l.Weightage;
                    LineCheck = new List<BudgetModelReport>();
                    LineCheck = model.Where(lines => lines.ParentActivityId == l.ActivityId && lines.ActivityType == ActivityType.ActivityLineItem).ToList();
                    if (LineCheck.Count() > 0)
                    {
                        //// Set parent line values.
                        parent = new BudgetMonth();
                        parent.Jan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthPlanned.Jan * weightage) / 100) ?? 0;
                        parent.Feb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthPlanned.Feb * weightage) / 100) ?? 0;
                        parent.Mar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthPlanned.Mar * weightage) / 100) ?? 0;
                        parent.Apr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthPlanned.Apr * weightage) / 100) ?? 0;
                        parent.May = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthPlanned.May * weightage) / 100) ?? 0;
                        parent.Jun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthPlanned.Jun * weightage) / 100) ?? 0;
                        parent.Jul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthPlanned.Jul * weightage) / 100) ?? 0;
                        parent.Aug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthPlanned.Aug * weightage) / 100) ?? 0;
                        parent.Sep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthPlanned.Sep * weightage) / 100) ?? 0;
                        parent.Oct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthPlanned.Oct * weightage) / 100) ?? 0;
                        parent.Nov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthPlanned.Nov * weightage) / 100) ?? 0;
                        parent.Dec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthPlanned.Dec * weightage) / 100) ?? 0;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonthPlanned = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthPlanned;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthPlanned = parent;

                        //// Set parent Actual line values.
                        parentActual = new BudgetMonth();
                        parentActual.Jan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthActual.Jan * weightage) / 100) ?? 0;
                        parentActual.Feb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthActual.Feb * weightage) / 100) ?? 0;
                        parentActual.Mar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthActual.Mar * weightage) / 100) ?? 0;
                        parentActual.Apr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthActual.Apr * weightage) / 100) ?? 0;
                        parentActual.May = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthActual.May * weightage) / 100) ?? 0;
                        parentActual.Jun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthActual.Jun * weightage) / 100) ?? 0;
                        parentActual.Jul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthActual.Jul * weightage) / 100) ?? 0;
                        parentActual.Aug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthActual.Aug * weightage) / 100) ?? 0;
                        parentActual.Sep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthActual.Sep * weightage) / 100) ?? 0;
                        parentActual.Oct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthActual.Oct * weightage) / 100) ?? 0;
                        parentActual.Nov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthActual.Nov * weightage) / 100) ?? 0;
                        parentActual.Dec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthActual.Dec * weightage) / 100) ?? 0;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonthActual = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthActual;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthActual = parentActual;
                    }
                    else
                    {
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonthActual = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthPlanned;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonthActual = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthActual;
                    }
                    ////// Set parent monthly allocated line values.
                    //parentAllocated = new BudgetMonth();
                    //parentAllocated.Jan = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Jan) ?? 0;
                    //parentAllocated.Feb = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Feb) ?? 0;
                    //parentAllocated.Mar = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Mar) ?? 0;
                    //parentAllocated.Apr = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Apr) ?? 0;
                    //parentAllocated.May = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.May) ?? 0;
                    //parentAllocated.Jun = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Jun) ?? 0;
                    //parentAllocated.Jul = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Jul) ?? 0;
                    //parentAllocated.Aug = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Aug) ?? 0;
                    //parentAllocated.Sep = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Sep) ?? 0;
                    //parentAllocated.Oct = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Oct) ?? 0;
                    //parentAllocated.Nov = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Nov) ?? 0;
                    //parentAllocated.Dec = model.Where(line => line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthAllocated.Dec) ?? 0;
                    //model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ChildMonthAllocated = parentAllocated;
                }
            }
            else
            {
                BudgetMonth parent, parentActual, parentAllocated;
                foreach (BudgetModelReport l in model.Where(l => l.ActivityType == ParentActivityType))
                {
                    //// Set Parent line values.
                    parent = new BudgetMonth();
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
                    parentActual = new BudgetMonth();
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
                    parentAllocated = new BudgetMonth();
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

        /// <summary>
        /// Set Tactic weightage to model record.
        /// </summary>
        /// <param name="lstModel">BudgetModelReport List</param>
        /// <param name="IsCustomFieldViewBy">ViewBy is Customfield or not</param>
        /// <returns>Return BudgetModelReport list.</returns>
        private List<BudgetModelReport> SetTacticWeightage(List<BudgetModelReport> lstModel, bool IsCustomFieldViewBy)
        {
            List<CustomField_Entity> lstCustomFieldEntities = new List<CustomField_Entity>();
            lstCustomFieldEntities = db.CustomField_Entity.ToList();
            int PlanTacticId = 0;
            string CustomFieldOptionID = string.Empty;
            foreach (BudgetModelReport obj in lstModel)
            {
                if (obj.ActivityType.Equals(ActivityType.ActivityTactic))
                {
                    //// if ViewBy is CustomFieldType(like Campaign/Program/Tactic CustomFields) then set Weightage from respective tables O/W take default 100% for Textbox type.
                    //// if CustomFieldType is Dropdownlist then retrieve weightage from CustomFieldEntity or StageWeight table O/W take default 100% for Textbox type.
                    if (IsCustomFieldViewBy && obj.CustomFieldType.Equals(Enums.CustomFieldType.DropDownList.ToString()))
                    {
                        PlanTacticId = !string.IsNullOrEmpty(obj.Id) ? Convert.ToInt32(obj.Id.ToString()) : 0; // Get PlanTacticId from Tactic ActivityId.
                        CustomFieldOptionID = obj.TabActivityId.ToString(); // Get CustomfieldOptionId from Tactic ActivityId.
                        int weightage = 0;
                        if (lstCustomFieldEntities != null && lstCustomFieldEntities.Count > 0)
                        {
                            //// Get CustomFieldEntity based on EntityId and CustomFieldOptionId from CustomFieldEntities.
                            var _custment = lstCustomFieldEntities.Where(_ent => _ent.EntityId.Equals(PlanTacticId) && _ent.Value.Equals(CustomFieldOptionID)).FirstOrDefault();
                            if (_custment == null)
                                weightage = 0;
                            else if (_custment.CostWeightage != null && Convert.ToInt32(_custment.CostWeightage.Value) > 0) // Get CostWeightage from table CustomFieldEntity.
                                weightage = Convert.ToInt32(_custment.CostWeightage.Value);

                        }
                        obj.Weightage = weightage;
                    }
                    else
                    {
                        obj.Weightage = 100;
                    }
                }
            }
            return lstModel;
        }

        /// <summary>
        /// Calculate the LineItem cost value based on it's parent Tactic weightage.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<BudgetModelReport> SetLineItemCostByWeightage(List<BudgetModelReport> model, bool IsCustomFieldViewBy)
        {
            int weightage = 100;
            foreach (BudgetModelReport l in model.Where(_mdl => _mdl.ActivityType == ActivityType.ActivityTactic))
            {
                BudgetMonth parent = new BudgetMonth();
                List<BudgetModelReport> lstLineItems = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId).ToList();

                //// check if ViewBy is Campaign selected then set weightage value to 100;
                if (IsCustomFieldViewBy)
                    weightage = l.Weightage;

                foreach (BudgetModelReport line in lstLineItems)
                {
                    BudgetMonth linePlannedBudget = new BudgetMonth();
                    linePlannedBudget.Jan = (double)(line.MonthPlanned.Jan * weightage) / 100;
                    linePlannedBudget.Feb = (double)(line.MonthPlanned.Feb * weightage) / 100;
                    linePlannedBudget.Mar = (double)(line.MonthPlanned.Mar * weightage) / 100;
                    linePlannedBudget.Apr = (double)(line.MonthPlanned.Apr * weightage) / 100;
                    linePlannedBudget.May = (double)(line.MonthPlanned.May * weightage) / 100;
                    linePlannedBudget.Jun = (double)(line.MonthPlanned.Jun * weightage) / 100;
                    linePlannedBudget.Jul = (double)(line.MonthPlanned.Jul * weightage) / 100;
                    linePlannedBudget.Aug = (double)(line.MonthPlanned.Aug * weightage) / 100;
                    linePlannedBudget.Sep = (double)(line.MonthPlanned.Sep * weightage) / 100;
                    linePlannedBudget.Oct = (double)(line.MonthPlanned.Oct * weightage) / 100;
                    linePlannedBudget.Nov = (double)(line.MonthPlanned.Nov * weightage) / 100;
                    linePlannedBudget.Dec = (double)(line.MonthPlanned.Dec * weightage) / 100;

                    line.MonthPlanned = linePlannedBudget;

                    BudgetMonth lineActualBudget = new BudgetMonth();
                    lineActualBudget.Jan = (double)(line.MonthActual.Jan * weightage) / 100;
                    lineActualBudget.Feb = (double)(line.MonthActual.Feb * weightage) / 100;
                    lineActualBudget.Mar = (double)(line.MonthActual.Mar * weightage) / 100;
                    lineActualBudget.Apr = (double)(line.MonthActual.Apr * weightage) / 100;
                    lineActualBudget.May = (double)(line.MonthActual.May * weightage) / 100;
                    lineActualBudget.Jun = (double)(line.MonthActual.Jun * weightage) / 100;
                    lineActualBudget.Jul = (double)(line.MonthActual.Jul * weightage) / 100;
                    lineActualBudget.Aug = (double)(line.MonthActual.Aug * weightage) / 100;
                    lineActualBudget.Sep = (double)(line.MonthActual.Sep * weightage) / 100;
                    lineActualBudget.Oct = (double)(line.MonthActual.Oct * weightage) / 100;
                    lineActualBudget.Nov = (double)(line.MonthActual.Nov * weightage) / 100;
                    lineActualBudget.Dec = (double)(line.MonthActual.Dec * weightage) / 100;
                    line.MonthActual = lineActualBudget;
                }
            }
            return model;
        }
        #endregion

        #region "Common Methods"
        /// <summary>
        /// Get list of CustomFieldOption mapping.
        /// </summary>
        /// <param name="Tab"></param>
        /// <param name="tacticList"> Reference of Tactic list</param>
        /// <param name="lstArrCustomFieldFilter">list of filtered CustomFields</param>
        /// <returns></returns>
        public List<BudgetReportTab> GetCustomFieldOptionMappinglist(string Tab, int customfieldId, string customFieldType, List<Plan_Campaign_Program_Tactic> tacticList)
        {
            List<BudgetReportTab> planobj = new List<BudgetReportTab>();
            List<string> lstFilteredOptions = new List<string>();
            try
            {
                //// Get Filter selected Option values based on ViewBy customfield value.
                List<int> entityids = new List<int>();
                if (Tab.Contains(Common.TacticCustomTitle))
                    entityids = tacticList.Select(t => t.PlanTacticId).ToList();
                else if (Tab.Contains(Common.ProgramCustomTitle))
                    entityids = tacticList.Select(t => t.PlanProgramId).ToList();
                else if (Tab.Contains(Common.CampaignCustomTitle))
                    entityids = tacticList.Select(t => t.Plan_Campaign_Program.PlanCampaignId).ToList();
                var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();
                if (customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                {
                    var customfieldoptionlist = db.CustomFieldOptions.Where(option => option.CustomFieldId == customfieldId && option.IsDeleted == false).ToList();
                    if (Tab.Contains(Common.TacticCustomTitle) && Sessions.ReportCustomFieldIds != null && Sessions.ReportCustomFieldIds.Count() > 0)
                    {
                        List<CustomFieldFilter> lstCustomFieldFilter = Sessions.ReportCustomFieldIds.ToList();
                        var optionIds = lstCustomFieldFilter.Where(x => x.CustomFieldId == customfieldId).Select(x => x.OptionId).First() != "" ?
                        lstCustomFieldFilter.Where(x => x.CustomFieldId == customfieldId).Select(x => x.OptionId).ToList() :
                        cusomfieldEntity.Where(x => x.CustomFieldId == customfieldId).Select(x => x.Value).Distinct().ToList();
                        customfieldoptionlist = customfieldoptionlist.Where(option => optionIds.Contains(option.CustomFieldOptionId.ToString())).ToList();
                    }
                    //// Retrieve CustomFieldOptions based on CustomField & Filtered CustomFieldOptionValues.
                    planobj = customfieldoptionlist.Select(p => new BudgetReportTab
                    {
                        Id = p.CustomFieldOptionId.ToString(),
                        Title = p.Value
                    }).Select(b => b).Distinct().OrderBy(b => b.Title).ToList();
                }
                else if (customFieldType == Enums.CustomFieldType.TextBox.ToString())
                {
                    planobj = cusomfieldEntity.GroupBy(c => c.Value).Select(p => new BudgetReportTab
                    {
                        Id = p.Key,
                        Title = p.Key
                    }).Select(b => b).OrderBy(b => b.Title).ToList();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return planobj;
        }

        /// <summary>
        /// Get list of Filtered Actual Tactics.
        /// </summary>
        /// <returns></returns>
        public List<ActualDataTable> GetCostLineItemActualListbyWeightage(int CustomFieldId, string CustomFieldOptionId, string customfieldType, int PlanTacticId, List<Plan_Campaign_Program_Tactic_LineItem_Actual> lstLineItemActuallist, TacticStageValue TacticData, bool IsTacticCustomField)
        {
            #region "Declare local variables"
            int? weightage = 0;
            TacticStageValue objTacticStageValue = new TacticStageValue();
            TacticCustomFieldStageWeightage objTacticStageWeightage = new TacticCustomFieldStageWeightage();
            string stagecode = string.Empty;
            List<Plan_Campaign_Program_Tactic_LineItem_Actual> LineItemActuallist = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
            LineItemActuallist = lstLineItemActuallist;
            ActualDataTable objActualTacticdt = new ActualDataTable();
            List<ActualDataTable> Actualtacticdata = new List<ActualDataTable>();
            double StageValue = 0;
            #endregion
            try
            {
                stagecode = Enums.InspectStage.Cost.ToString();
                foreach (Plan_Campaign_Program_Tactic_LineItem_Actual objActual in LineItemActuallist)
                {
                    weightage = 0;
                    objActualTacticdt = new ActualDataTable();
                    if (!string.IsNullOrEmpty(customfieldType) && customfieldType == Enums.CustomFieldType.DropDownList.ToString() && IsTacticCustomField)
                    {
                        objTacticStageWeightage = TacticData.TacticStageWeightages.Where(_stage => _stage.CustomFieldId.Equals(CustomFieldId) && _stage.Value.Equals(CustomFieldOptionId)).FirstOrDefault();

                        if (objTacticStageWeightage != null)
                        {
                            weightage = objTacticStageWeightage.CostWeightage;
                        }
                    }
                    else
                        weightage = 100;
                    StageValue = (objActual.Value * (weightage.HasValue ? weightage.Value : 0)) / 100;
                    objActualTacticdt.PlanTacticId = objActual.Plan_Campaign_Program_Tactic_LineItem.PlanTacticId;
                    objActualTacticdt.Period = objActual.Period;
                    objActualTacticdt.ActualValue = StageValue;
                    objActualTacticdt.StageTitle = string.Empty;
                    Actualtacticdata.Add(objActualTacticdt);

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Actualtacticdata;
        }

        /// <summary>
        /// Get respective Planned value by StageTitle.
        /// </summary>
        /// <returns></returns>
        public double GetPlanValue(int CustomFieldId, string CustomFieldOptionId, List<TacticStageValue> lstTacticData, string CustomFieldType, List<string> includeMonth, Enums.InspectStage stageCode, bool IsTacticCustomField)
        {
            double PlanValue = 0;
            List<TacticMonthValue> ProjectedDatatable = new List<TacticMonthValue>();
            try
            {
                if (stageCode.Equals(Enums.InspectStage.CW))
                    ProjectedDatatable = GetProjectedDatabyStageCode(CustomFieldId, CustomFieldOptionId, CustomFieldType, stageCode, lstTacticData, IsTacticCustomField);
                else if (stageCode.Equals(Enums.InspectStage.Revenue))
                    ProjectedDatatable = GetProjectedDatabyStageCode(CustomFieldId, CustomFieldOptionId, CustomFieldType, stageCode, lstTacticData, IsTacticCustomField);
                else if (stageCode.Equals(Enums.InspectStage.Cost))
                    ProjectedDatatable = GetProjectedCostData(CustomFieldId, CustomFieldOptionId, CustomFieldType, lstTacticData, IsTacticCustomField,null,null,null);

                PlanValue = ProjectedDatatable.Where(mr => includeMonth.Contains(mr.Month)).Sum(r => r.Value);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return PlanValue;
        }

        /// <summary>
        /// Get ActualRevenue value by weightage.
        /// </summary>
        /// <returns></returns>
        public double GetActualRevenueContribution(int CustomFieldId, string CustomFieldOptionId, List<TacticStageValue> TacticData, List<int> PlanTacticIds, string CustomFieldType, List<string> includeMonth, bool IsTacticCustomField)
        {
            double ActualRevenueValue = 0;
            List<Plan_Campaign_Program_Tactic_Actual> lstActuals = new List<Plan_Campaign_Program_Tactic_Actual>();
            string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            List<ActualDataTable> ActualDataTable = new List<ActualDataTable>();
            try
            {
                lstActuals = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => PlanTacticIds.Contains(ta.PlanTacticId) && ta.StageTitle.Equals(revenue) && includeMonth.Contains(ta.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year + ta.Period)).ToList();
                ActualDataTable = GetActualTacticDataTablebyStageCode(CustomFieldId, CustomFieldOptionId, CustomFieldType, Enums.InspectStage.Revenue, lstActuals, TacticData, IsTacticCustomField);
                ActualRevenueValue = ActualDataTable.Sum(ta => ta.ActualValue);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ActualRevenueValue;
        }

        /// <summary>
        /// Get list of Actual Tactic data for MQL Performance.
        /// </summary>
        /// <param name="Tab"></param>
        /// <param name="tacticList"> Reference of Tactic list</param>
        /// <param name="lstArrCustomFieldFilter">list of filtered CustomFields</param>
        /// <returns></returns>
        public double GetTrendActualTacticData(int lastMonth, List<Plan_Campaign_Program_Tactic_Actual> lstActual, int CustomFieldID, string CustomFieldOptionID, string customFieldType, List<TacticStageValue> TacticData, bool IsTacticCustomField)
        {
            List<ActualDataTable> lstActualsbyWeight = new List<ActualDataTable>();
            double trendValue = 0;
            try
            {
                lstActualsbyWeight = GetActualTacticDataTablebyStageCode(CustomFieldID, CustomFieldOptionID, customFieldType, Enums.InspectStage.MQL, lstActual, TacticData, IsTacticCustomField);
                var lstCustomFieldEntity = db.CustomField_Entity.Where(cf => cf.Value.Equals(CustomFieldOptionID)).Select(e => new
                {
                    EntityId = e.EntityId,
                    Value = e.Value
                }).ToList();

                var lstTacticActuals = (from entity in lstCustomFieldEntity
                                        join tacticactual in lstActualsbyWeight
                                        on entity.EntityId equals tacticactual.PlanTacticId
                                        select new { entity.Value, entity.EntityId, tacticactual.ActualValue });

                var tacticTrendCustomField = lstTacticActuals.GroupBy(ta => ta.Value).Select
                    (ta => new
                    {
                        CustomFieldOptionId = ta.Key,
                        Trend = ((ta.Sum(actual => actual.ActualValue) / currentMonth) * lastMonth)
                    }).ToList();
                if (tacticTrendCustomField != null && tacticTrendCustomField.Count > 0)
                    trendValue = tacticTrendCustomField.FirstOrDefault().Trend;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return trendValue;
        }

        /// <summary>
        /// Get Tactic Data by Stage Weightage.
        /// </summary>
        /// <param name="planTacticList"></param>
        /// <returns>Return list of TacticDataTable</returns>
        public List<TacticDataTable> GetTacticDataTablebyStageCode(int CustomFieldId, string CustomFieldOptionId, string CustomFieldType, Enums.InspectStage stagecode, List<TacticStageValue> planTacticList, bool IsTacticCustomField, bool IsVelocity = false)
        {
            double StageValue = 0;
            int? weightage = 0;
            TacticCustomFieldStageWeightage objTacticStageWeightage = new TacticCustomFieldStageWeightage();
            List<TacticDataTable> tacticdata = new List<TacticDataTable>();
            TacticDataTable objTacticdt = new TacticDataTable();
            if (stagecode.Equals(Enums.InspectStage.INQ))
            {
                foreach (TacticStageValue objTactic in planTacticList)
                {
                    weightage = 0;
                    objTacticdt = new TacticDataTable();
                    if (!string.IsNullOrEmpty(CustomFieldType) && CustomFieldType == Enums.CustomFieldType.DropDownList.ToString() && IsTacticCustomField)
                    {
                        objTacticStageWeightage = new TacticCustomFieldStageWeightage();
                        objTacticStageWeightage = objTactic.TacticStageWeightages.Where(_stage => _stage.CustomFieldId.Equals(CustomFieldId) && _stage.Value.Equals(CustomFieldOptionId)).FirstOrDefault();
                        if (objTacticStageWeightage != null)
                        {
                            weightage = objTacticStageWeightage.CVRWeightage;
                        }
                    }
                    else
                        weightage = 100;
                    StageValue = (objTactic.INQValue * (weightage.HasValue ? weightage.Value : 0)) / 100;
                    objTacticdt.TacticId = objTactic.TacticObj.PlanTacticId;
                    objTacticdt.Value = StageValue;
                    objTacticdt.StartMonth = IsVelocity ? objTactic.TacticObj.StartDate.AddDays(objTactic.INQVelocity).Month : objTactic.TacticObj.StartDate.Month;
                    objTacticdt.EndMonth = IsVelocity ? objTactic.TacticObj.EndDate.AddDays(objTactic.INQVelocity).Month : objTactic.TacticObj.EndDate.Month;
                    objTacticdt.StartYear = IsVelocity ? objTactic.TacticObj.StartDate.AddDays(objTactic.INQVelocity).Year : objTactic.TacticObj.StartDate.Year;
                    objTacticdt.EndYear = IsVelocity ? objTactic.TacticObj.EndDate.AddDays(objTactic.INQVelocity).Year : objTactic.TacticObj.EndDate.Year;

                    tacticdata.Add(objTacticdt);
                }
            }
            if (stagecode.Equals(Enums.InspectStage.MQL))
            {
                foreach (TacticStageValue objTactic in planTacticList)
                {
                    weightage = 0;
                    objTacticdt = new TacticDataTable();
                    if (!string.IsNullOrEmpty(CustomFieldType) && CustomFieldType == Enums.CustomFieldType.DropDownList.ToString() && IsTacticCustomField)
                    {
                        objTacticStageWeightage = new TacticCustomFieldStageWeightage();
                        objTacticStageWeightage = objTactic.TacticStageWeightages.Where(_stage => _stage.CustomFieldId.Equals(CustomFieldId) && _stage.Value.Equals(CustomFieldOptionId)).FirstOrDefault();
                        if (objTacticStageWeightage != null)
                        {
                            weightage = objTacticStageWeightage.CVRWeightage;
                        }
                    }
                    else
                        weightage = 100;
                    StageValue = (objTactic.MQLValue * (weightage.HasValue ? weightage.Value : 0)) / 100;
                    objTacticdt.TacticId = objTactic.TacticObj.PlanTacticId;
                    objTacticdt.Value = StageValue;
                    objTacticdt.StartMonth = IsVelocity ? objTactic.TacticObj.StartDate.AddDays(objTactic.MQLVelocity).Month : objTactic.TacticObj.StartDate.Month;
                    objTacticdt.EndMonth = IsVelocity ? objTactic.TacticObj.EndDate.AddDays(objTactic.MQLVelocity).Month : objTactic.TacticObj.EndDate.Month;
                    objTacticdt.StartYear = IsVelocity ? objTactic.TacticObj.StartDate.AddDays(objTactic.MQLVelocity).Year : objTactic.TacticObj.StartDate.Year;
                    objTacticdt.EndYear = IsVelocity ? objTactic.TacticObj.EndDate.AddDays(objTactic.MQLVelocity).Year : objTactic.TacticObj.EndDate.Year;
                    tacticdata.Add(objTacticdt);
                }
            }
            if (stagecode.Equals(Enums.InspectStage.CW))
            {
                foreach (TacticStageValue objTactic in planTacticList)
                {
                    weightage = 0;
                    objTacticdt = new TacticDataTable();
                    if (!string.IsNullOrEmpty(CustomFieldType) && CustomFieldType == Enums.CustomFieldType.DropDownList.ToString() && IsTacticCustomField)
                    {
                        objTacticStageWeightage = new TacticCustomFieldStageWeightage();
                        objTacticStageWeightage = objTactic.TacticStageWeightages.Where(_stage => _stage.CustomFieldId.Equals(CustomFieldId) && _stage.Value.Equals(CustomFieldOptionId)).FirstOrDefault();
                        if (objTacticStageWeightage != null)
                        {
                            weightage = objTacticStageWeightage.CVRWeightage;
                        }
                    }
                    else
                        weightage = 100;
                    StageValue = (objTactic.CWValue * (weightage.HasValue ? weightage.Value : 0)) / 100;
                    objTacticdt.TacticId = objTactic.TacticObj.PlanTacticId;
                    objTacticdt.Value = StageValue;
                    objTacticdt.StartMonth = IsVelocity ? objTactic.TacticObj.StartDate.AddDays(objTactic.CWVelocity).Month : objTactic.TacticObj.StartDate.Month;
                    objTacticdt.EndMonth = IsVelocity ? objTactic.TacticObj.EndDate.AddDays(objTactic.CWVelocity).Month : objTactic.TacticObj.EndDate.Month;
                    objTacticdt.StartYear = IsVelocity ? objTactic.TacticObj.StartDate.AddDays(objTactic.CWVelocity).Year : objTactic.TacticObj.StartDate.Year;
                    objTacticdt.EndYear = IsVelocity ? objTactic.TacticObj.EndDate.AddDays(objTactic.CWVelocity).Year : objTactic.TacticObj.EndDate.Year;
                    tacticdata.Add(objTacticdt);
                }
            }
            if (stagecode.Equals(Enums.InspectStage.Revenue))
            {
                foreach (TacticStageValue objTactic in planTacticList)
                {
                    weightage = 0;
                    objTacticdt = new TacticDataTable();
                    if (!string.IsNullOrEmpty(CustomFieldType) && CustomFieldType == Enums.CustomFieldType.DropDownList.ToString() && IsTacticCustomField)
                    {
                        objTacticStageWeightage = new TacticCustomFieldStageWeightage();
                        objTacticStageWeightage = objTactic.TacticStageWeightages.Where(_stage => _stage.CustomFieldId.Equals(CustomFieldId) && _stage.Value.Equals(CustomFieldOptionId)).FirstOrDefault();
                        if (objTacticStageWeightage != null)
                        {
                            weightage = objTacticStageWeightage.CVRWeightage;
                        }
                    }
                    else
                        weightage = 100;
                    StageValue = (objTactic.RevenueValue * (weightage.HasValue ? weightage.Value : 0)) / 100;
                    objTacticdt.TacticId = objTactic.TacticObj.PlanTacticId;
                    objTacticdt.Value = StageValue;
                    objTacticdt.StartMonth = IsVelocity ? objTactic.TacticObj.StartDate.AddDays(objTactic.CWVelocity).Month : objTactic.TacticObj.StartDate.Month;
                    objTacticdt.EndMonth = IsVelocity ? objTactic.TacticObj.EndDate.AddDays(objTactic.CWVelocity).Month : objTactic.TacticObj.EndDate.Month;
                    objTacticdt.StartYear = IsVelocity ? objTactic.TacticObj.StartDate.AddDays(objTactic.CWVelocity).Year : objTactic.TacticObj.StartDate.Year;
                    objTacticdt.EndYear = IsVelocity ? objTactic.TacticObj.EndDate.AddDays(objTactic.CWVelocity).Year : objTactic.TacticObj.EndDate.Year;
                    tacticdata.Add(objTacticdt);
                }
            }
            if (stagecode.Equals(Enums.InspectStage.Cost))
            {
                foreach (TacticStageValue objTactic in planTacticList)
                {
                    weightage = 0;
                    objTacticdt = new TacticDataTable();
                    if (!string.IsNullOrEmpty(CustomFieldType) && CustomFieldType == Enums.CustomFieldType.DropDownList.ToString() && IsTacticCustomField)
                    {
                        objTacticStageWeightage = new TacticCustomFieldStageWeightage();
                        objTacticStageWeightage = objTactic.TacticStageWeightages.Where(_stage => _stage.CustomFieldId.Equals(CustomFieldId) && _stage.Value.Equals(CustomFieldOptionId)).FirstOrDefault();
                        if (objTacticStageWeightage != null)
                        {
                            weightage = objTacticStageWeightage.CostWeightage;
                        }
                    }
                    else
                        weightage = 100;
                    StageValue = (objTactic.TacticObj.Cost * (weightage.HasValue ? weightage.Value : 0)) / 100;
                    objTacticdt.TacticId = objTactic.TacticObj.PlanTacticId;
                    objTacticdt.Value = StageValue;
                    objTacticdt.StartMonth = objTactic.TacticObj.StartDate.Month;
                    objTacticdt.EndMonth = objTactic.TacticObj.EndDate.Month;
                    objTacticdt.StartYear = objTactic.TacticObj.StartDate.Year;
                    objTacticdt.EndYear = objTactic.TacticObj.EndDate.Year;
                    tacticdata.Add(objTacticdt);
                }
            }
            return tacticdata;
        }

        /// <summary>
        /// Get Actuals of Tactic Data by Stage Weightage.
        /// </summary>
        /// <param name="planTacticList"></param>
        /// <returns>Return list of ActualDataTable</returns>
        public List<ActualDataTable> GetActualTacticDataTablebyStageCode(int CustomFieldId, string CustomFieldOptionId, string CustomFieldType, Enums.InspectStage stagecode, List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList, List<TacticStageValue> TacticData, bool IsTacticCustomField = false)
        {
            double StageValue = 0;
            int? weightage = 0;
            TacticCustomFieldStageWeightage objTacticStageWeightage = new TacticCustomFieldStageWeightage();
            List<ActualDataTable> Actualtacticdata = new List<ActualDataTable>();
            ActualDataTable objActualTacticdt = new ActualDataTable();
            string strStage = string.Empty;
            TacticStageValue objTacticStageValue = new TacticStageValue();
            if (stagecode.Equals(Enums.InspectStage.INQ))
            {
                strStage = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
                foreach (Plan_Campaign_Program_Tactic_Actual objActual in ActualTacticList.Where(actual => actual.StageTitle.Equals(strStage)))
                {
                    weightage = 0;
                    objActualTacticdt = new ActualDataTable();
                    if (!string.IsNullOrEmpty(CustomFieldType) && CustomFieldType == Enums.CustomFieldType.DropDownList.ToString() && IsTacticCustomField)
                    {
                        objTacticStageValue = TacticData.Where(tac => tac.TacticObj.PlanTacticId.Equals(objActual.PlanTacticId)).FirstOrDefault();
                        objTacticStageWeightage = objTacticStageValue.TacticStageWeightages.Where(_stage => _stage.CustomFieldId.Equals(CustomFieldId) && _stage.Value.Equals(CustomFieldOptionId)).FirstOrDefault();

                        if (objTacticStageWeightage != null)
                        {
                            weightage = objTacticStageWeightage.CVRWeightage;
                        }
                    }
                    else
                        weightage = 100;
                    StageValue = (objActual.Actualvalue * (weightage.HasValue ? weightage.Value : 0)) / 100;
                    objActualTacticdt.PlanTacticId = objActual.PlanTacticId;
                    objActualTacticdt.Period = objActual.Period;
                    objActualTacticdt.ActualValue = StageValue;
                    objActualTacticdt.StageTitle = objActual.StageTitle;
                    Actualtacticdata.Add(objActualTacticdt);
                }
            }
            if (stagecode.Equals(Enums.InspectStage.CW))
            {
                strStage = Enums.InspectStage.CW.ToString();
                foreach (Plan_Campaign_Program_Tactic_Actual objActual in ActualTacticList.Where(actual => actual.StageTitle.Equals(strStage)))
                {
                    weightage = 0;
                    objActualTacticdt = new ActualDataTable();
                    if (!string.IsNullOrEmpty(CustomFieldType) && CustomFieldType == Enums.CustomFieldType.DropDownList.ToString() && IsTacticCustomField)
                    {
                        objTacticStageValue = TacticData.Where(tac => tac.TacticObj.PlanTacticId.Equals(objActual.PlanTacticId)).FirstOrDefault();
                        objTacticStageWeightage = objTacticStageValue.TacticStageWeightages.Where(_stage => _stage.CustomFieldId.Equals(CustomFieldId) && _stage.Value.Equals(CustomFieldOptionId)).FirstOrDefault();

                        if (objTacticStageWeightage != null)
                        {
                            weightage = objTacticStageWeightage.CVRWeightage;
                        }
                    }
                    else
                        weightage = 100;
                    StageValue = (objActual.Actualvalue * (weightage.HasValue ? weightage.Value : 0)) / 100;
                    objActualTacticdt.PlanTacticId = objActual.PlanTacticId;
                    objActualTacticdt.Period = objActual.Period;
                    objActualTacticdt.ActualValue = StageValue;
                    objActualTacticdt.StageTitle = objActual.StageTitle;
                    Actualtacticdata.Add(objActualTacticdt);
                }
            }
            if (stagecode.Equals(Enums.InspectStage.MQL))
            {
                strStage = Enums.InspectStage.MQL.ToString();
                foreach (Plan_Campaign_Program_Tactic_Actual objActual in ActualTacticList.Where(actual => actual.StageTitle.Equals(strStage)))
                {
                    weightage = 0;
                    objActualTacticdt = new ActualDataTable();
                    if (!string.IsNullOrEmpty(CustomFieldType) && CustomFieldType == Enums.CustomFieldType.DropDownList.ToString() && IsTacticCustomField)
                    {
                        objTacticStageValue = TacticData.Where(tac => tac.TacticObj.PlanTacticId.Equals(objActual.PlanTacticId)).FirstOrDefault();
                        objTacticStageWeightage = objTacticStageValue.TacticStageWeightages.Where(_stage => _stage.CustomFieldId.Equals(CustomFieldId) && _stage.Value.Equals(CustomFieldOptionId)).FirstOrDefault();

                        if (objTacticStageWeightage != null)
                        {
                            weightage = objTacticStageWeightage.CVRWeightage;
                        }
                    }
                    else
                        weightage = 100;
                    StageValue = (objActual.Actualvalue * (weightage.HasValue ? weightage.Value : 0)) / 100;
                    objActualTacticdt.PlanTacticId = objActual.PlanTacticId;
                    objActualTacticdt.Period = objActual.Period;
                    objActualTacticdt.ActualValue = StageValue;
                    objActualTacticdt.StageTitle = objActual.StageTitle;
                    Actualtacticdata.Add(objActualTacticdt);
                }
            }
            if (stagecode.Equals(Enums.InspectStage.Revenue))
            {
                strStage = Enums.InspectStage.Revenue.ToString();
                foreach (Plan_Campaign_Program_Tactic_Actual objActual in ActualTacticList.Where(actual => actual.StageTitle.Equals(strStage)))
                {
                    weightage = 0;
                    objActualTacticdt = new ActualDataTable();
                    if (!string.IsNullOrEmpty(CustomFieldType) && CustomFieldType == Enums.CustomFieldType.DropDownList.ToString() && IsTacticCustomField)
                    {
                        objTacticStageValue = TacticData.Where(tac => tac.TacticObj.PlanTacticId.Equals(objActual.PlanTacticId)).FirstOrDefault();
                        objTacticStageWeightage = objTacticStageValue.TacticStageWeightages.Where(_stage => _stage.CustomFieldId.Equals(CustomFieldId) && _stage.Value.Equals(CustomFieldOptionId)).FirstOrDefault();

                        if (objTacticStageWeightage != null)
                        {
                            weightage = objTacticStageWeightage.CVRWeightage;
                        }
                    }
                    else
                        weightage = 100;
                    StageValue = (objActual.Actualvalue * (weightage.HasValue ? weightage.Value : 0)) / 100;
                    objActualTacticdt.PlanTacticId = objActual.PlanTacticId;
                    objActualTacticdt.Period = objActual.Period;
                    objActualTacticdt.ActualValue = StageValue;
                    objActualTacticdt.StageTitle = objActual.StageTitle;
                    Actualtacticdata.Add(objActualTacticdt);
                }
            }
            if (stagecode.Equals(Enums.InspectStage.Cost))
            {
                strStage = Enums.InspectStage.Cost.ToString();
                foreach (Plan_Campaign_Program_Tactic_Actual objActual in ActualTacticList.Where(actual => actual.StageTitle.Equals(strStage)))
                {
                    weightage = 0;
                    objActualTacticdt = new ActualDataTable();
                    if (!string.IsNullOrEmpty(CustomFieldType) && CustomFieldType == Enums.CustomFieldType.DropDownList.ToString() && IsTacticCustomField)
                    {
                        objTacticStageValue = TacticData.Where(tac => tac.TacticObj.PlanTacticId.Equals(objActual.PlanTacticId)).FirstOrDefault();
                        objTacticStageWeightage = objTacticStageValue.TacticStageWeightages.Where(_stage => _stage.CustomFieldId.Equals(CustomFieldId) && _stage.Value.Equals(CustomFieldOptionId)).FirstOrDefault();

                        if (objTacticStageWeightage != null)
                        {
                            weightage = objTacticStageWeightage.CostWeightage;
                        }
                    }
                    else
                        weightage = 100;
                    StageValue = (objActual.Actualvalue * (weightage.HasValue ? weightage.Value : 0)) / 100;
                    objActualTacticdt.PlanTacticId = objActual.PlanTacticId;
                    objActualTacticdt.Period = objActual.Period;
                    objActualTacticdt.ActualValue = StageValue;
                    objActualTacticdt.StageTitle = objActual.StageTitle;
                    Actualtacticdata.Add(objActualTacticdt);
                }
            }

            return Actualtacticdata;
        }

        /// <summary>
        /// Get Actuals of Tactic Data.
        /// </summary>
        /// <param name="ActualTacticList"></param>
        /// <returns>Return list of ActualDataTable</returns>
        public List<ActualDataTable> GetActualTacticDataTable(List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList)
        {
            List<ActualDataTable> Actualtacticdata = new List<ActualDataTable>();
            ActualDataTable objActualTacticdt = new ActualDataTable();
            foreach (Plan_Campaign_Program_Tactic_Actual objActual in ActualTacticList)
            {
                objActualTacticdt = new ActualDataTable();
                objActualTacticdt.PlanTacticId = objActual.PlanTacticId;
                objActualTacticdt.Period = objActual.Period;
                objActualTacticdt.ActualValue = objActual.Actualvalue;
                objActualTacticdt.StageTitle = objActual.StageTitle;
                Actualtacticdata.Add(objActualTacticdt);
            }
            return Actualtacticdata;
        }
        #endregion

        #region Get Owners by planID Method
        //Added By komal Rawal
        public JsonResult GetOwnerListForFilter(string PlanId, string leftPaneOption)
        {
            try
            {
                var lstOwners = GetIndividualsByPlanId(PlanId, leftPaneOption);
                var lstAllowedOwners = lstOwners.Select(owner => new
                {
                    OwnerId = owner.UserId,
                    Title = owner.FirstName + " " + owner.LastName,
                }).Distinct().ToList().OrderBy(owner => owner.Title).ToList();

                lstAllowedOwners = lstAllowedOwners.Where(owner => !string.IsNullOrEmpty(owner.Title)).OrderBy(owner => owner.Title, new AlphaNumericComparer()).ToList();

                return Json(new { isSuccess = true, AllowedOwner = lstAllowedOwners }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
        }

        private List<User> GetIndividualsByPlanId(string PlanId, string leftPaneOption)
        {
            List<int> PlanIds = string.IsNullOrWhiteSpace(PlanId) ? new List<int>() : PlanId.Split(',').Select(plan => int.Parse(plan)).ToList();


            List<string> status = Common.GetStatusListAfterApproved();


            //// Select Tactics of selected plans
            List<Plan_Campaign_Program_Tactic> tacticList = new List<Plan_Campaign_Program_Tactic>();

            //// Get Tactic list.


            if (leftPaneOption == RevenuePlanner.Helpers.Enums.ReportType.Budget.ToString())
            {

                tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => PlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactic => tactic).Distinct().ToList();
            }
            else
            {

                tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => status.Contains(tactic.Status) && PlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactic => tactic).Distinct().ToList();
            }
            if (tacticList.Count > 0)
            {
                List<int> planTacticIds = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
                var lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);

                //// Custom Restrictions applied
                tacticList = tacticList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId)).ToList();
            }

            if (Sessions.ReportCustomFieldIds != null && Sessions.ReportCustomFieldIds.Count() > 0)
            {
                List<int> tacticids = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
                List<CustomFieldFilter> lstCustomFieldFilter = Sessions.ReportCustomFieldIds.ToList();
                tacticids = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, tacticids);
                tacticList = tacticList.Where(tactic => tacticids.Contains(tactic.PlanTacticId)).ToList();
            }

            BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();
            string strContatedIndividualList = string.Join(",", tacticList.Select(tactic => tactic.CreatedBy.ToString()));
            var individuals = bdsUserRepository.GetMultipleTeamMemberName(strContatedIndividualList);

            return individuals;

        }
        //End
        #endregion

        #region Tactic type list
        public JsonResult GetTacticTypeListForFilter(string PlanId, string leftPaneOption)
        {

            try
            {
                List<int> lstPlanIds = new List<int>();
                if (PlanId != string.Empty)
                {
                    string[] arrPlanIds = PlanId.Split(',');
                    foreach (string pId in arrPlanIds)
                    {
                        int planId;
                        if (int.TryParse(pId, out planId))
                        {
                            lstPlanIds.Add(planId);
                        }
                    }
                }
                if (leftPaneOption == RevenuePlanner.Helpers.Enums.ReportType.Budget.ToString())
                {
                    var objTacticType = (from tactic in db.Plan_Campaign_Program_Tactic
                                         where lstPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && tactic.IsDeleted == false
                                         select new { tactic.TacticType.Title, tactic.TacticTypeId }).Distinct().ToList();

                    return Json(new { isSuccess = true, TacticTypelist = objTacticType }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<string> status = Common.GetStatusListAfterApproved();
                    var objTacticType = (from tactic in db.Plan_Campaign_Program_Tactic
                                         where status.Contains(tactic.Status) && lstPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && tactic.IsDeleted == false
                                         select new { tactic.TacticType.Title, tactic.TacticTypeId }).Distinct().ToList();

                    return Json(new { isSuccess = true, TacticTypelist = objTacticType }, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region "New Report Methods"

        #region "Overview"
        /// <summary>
        /// This action will return the View for Summary data
        /// </summary>
        /// <returns>Return Partial View _Summary</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ReportView)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #519 to implement user permission Logic
        public ActionResult GetOverviewData(string timeframeOption, string isQuarterly = "Quarterly")
        {
            #region "Declare Local Variables"
            ReportOverviewModel objReportOverviewModel = new ReportOverviewModel();
            RevenueOverviewModel objRevenueOverviewModel = new RevenueOverviewModel();
            ConversionOverviewModel objConversionOverviewModel = new ConversionOverviewModel();
            List<Plan_Campaign_Program_Tactic_Actual> ActualList = new List<Plan_Campaign_Program_Tactic_Actual>();
            List<ProjectedTrendModel> ProjectedTrendList = new List<ProjectedTrendModel>();
            List<TacticwiseOverviewModel> OverviewModelList = new List<TacticwiseOverviewModel>();
            lineChartData objLineChartData = new lineChartData();
            Projected_Goal objProjectedGoal = new Projected_Goal();
            List<conversion_Projected_Goal_LineChart> Projected_Goal_LineChartList = new List<conversion_Projected_Goal_LineChart>();
            conversion_Projected_Goal_LineChart objProjected_Goal_LineChart = new conversion_Projected_Goal_LineChart();
            List<Stage_Benchmark> StageBenchmarkList = new List<Stage_Benchmark>();
            string strMQLStageCode = Enums.InspectStage.MQL.ToString();
            string strCWStageCode = Enums.InspectStage.CW.ToString();
            double _Benchmark = 0, _inqActual = 0, _mqlActual = 0, _cwActual = 0, stageVolumePercntg = 0;
            bool IsQuarterly = true;
            string revStageCode = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            string inqStageCode = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
            string mqlStageCode = Enums.InspectStageValues[strMQLStageCode].ToString();
            string cwStageCode = Enums.InspectStageValues[strCWStageCode].ToString();
            List<string> ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add(revStageCode);
            ActualStageCodeList.Add(inqStageCode);
            ActualStageCodeList.Add(mqlStageCode);
            ActualStageCodeList.Add(cwStageCode);
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<ActualTrendModel> ActualTacticTrendList = new List<ActualTrendModel>();
            bool IsTillCurrentMonth = true;
            #endregion
            try
            {
                if (!string.IsNullOrEmpty(isQuarterly) && isQuarterly.Equals(Enums.ViewByAllocated.Monthly.ToString()))
                    IsQuarterly = false;
                //// get tactic list
                List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting();
                //// Calculate Value for ecah tactic
                List<TacticStageValue> Tacticdata = Common.GetTacticStageRelation(tacticlist, IsReport: true);
                //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
                TempData["ReportData"] = "";// Add BY Nishant SHeth
                TempData["ReportData"] = Tacticdata;

                // Start - Added by Arpita Soni for Ticket #1148 on 01/30/2015
                // To avoid summary display when no published plan selected (It displays no data found message.)
                foreach (var planId in Sessions.ReportPlanIds)
                {
                    if (Common.IsPlanPublished(planId))
                    {
                        isPublishedPlanExist = true;
                        break;
                    }
                }
                // End - Added by Arpita Soni for Ticket #1148 on 01/30/2015

                #region "Get Customfieldlist"
                List<ViewByModel> CustomFieldList = new List<ViewByModel>();
                List<int> campaignlist = tacticlist.Select(t => t.Plan_Campaign_Program.PlanCampaignId).ToList();
                List<int> programlist = tacticlist.Select(t => t.PlanProgramId).ToList();
                CustomFieldList = CustomFieldList.Where(s => !string.IsNullOrEmpty(s.Text)).ToList();
                var lstCustomFields = Common.GetCustomFields(tacticlist.Select(tactic => tactic.PlanTacticId).ToList(), programlist, campaignlist);
                CustomFieldList = CustomFieldList.Concat(lstCustomFields).ToList();
                SelectList ListCustomField = new SelectList(CustomFieldList, "Value", "Text");
                //ListCustomField = new Se
                SelectListItem _selectedCustomField = new SelectListItem();
                string selectedCustomFieldValue = string.Empty;
                if (CustomFieldList != null && CustomFieldList.Count > 0)
                    _selectedCustomField = ListCustomField.FirstOrDefault();
                selectedCustomFieldValue = _selectedCustomField.Value != null ? _selectedCustomField.Value : string.Empty;
                ViewBag.SummaryCustomFieldList = ListCustomField;

                #endregion

                //// check planids selected or not
                if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count > 0 && isPublishedPlanExist)
                {
                    //// set viewbag to display plan or msg
                    ViewBag.IsPlanExistToShowReport = true;

                    List<string> includeMonth = GetMonthListForReport(timeframeOption);
                    ActualTacticStageList = GetActualListInTacticInterval(Tacticdata, timeframeOption, ActualStageCodeList, IsTillCurrentMonth);
                    List<ActualTrendModel> ActualTacticTrendModelList = GetActualTrendModelForRevenueOverview(Tacticdata, ActualTacticStageList);
                    #region "Revenue related Code"

                    #region "Revenue : Get Tacticwise Actual_Projected Vs Goal Model data "

                    //ActualList = ActualTacticStageList.Where(actual => actual.StageCode.Equals(revStageCode)).Select(actual => actual.ActualTacticList).FirstOrDefault();
                    ActualTacticTrendList = ActualTacticTrendModelList.Where(actual => actual.StageCode.Equals(revStageCode)).ToList();
                    ProjectedTrendList = CalculateProjectedTrend(Tacticdata, includeMonth, revStageCode);
                    OverviewModelList = GetTacticwiseActualProjectedRevenueList(ActualTacticTrendList, ProjectedTrendList);
                    #endregion

                    #region "Get Basic Model"
                    BasicModel objBasicModel = GetValuesListByTimeFrame(ActualTacticTrendList, ProjectedTrendList, timeframeOption, IsQuarterly);
                    #endregion

                    #region "Set Linechart & Revenue Overview data to model"
                    //objLineChartData = GetLineChartData(ActualTacticTrendList, ProjectedTrendList, timeframeOption, IsQuarterly);
                    objLineChartData = GetLineChartData(objBasicModel);
                    objProjectedGoal = GetRevenueOverviewData(OverviewModelList, timeframeOption);
                    objRevenueOverviewModel.linechartdata = objLineChartData != null ? objLineChartData : new lineChartData();
                    objRevenueOverviewModel.projected_goal = objProjectedGoal != null ? objProjectedGoal : new Projected_Goal();
                    #endregion

                    #region "Get SparklineChart Data"
                    List<Enums.TOPRevenueType> RevenueTypeList = Enum.GetValues(typeof(Enums.TOPRevenueType)).Cast<Enums.TOPRevenueType>().ToList();
                    List<sparkLineCharts> SparkLineChartsData = GetRevenueSparkLineChartData(selectedCustomFieldValue, timeframeOption, RevenueTypeList, true);
                    objRevenueOverviewModel.SparkLineChartsData = SparkLineChartsData;
                    #endregion

                    #endregion

                    #region "Conversion related Code"

                    #region "Calculate ProjVsGoal for INQ"
                    #region "Conversion : Get Tacticwise Actual_Projected Vs Goal Model data "

                    ActualList = new List<Plan_Campaign_Program_Tactic_Actual>();
                    ProjectedTrendList = new List<ProjectedTrendModel>();
                    OverviewModelList = new List<TacticwiseOverviewModel>();
                    ActualTacticTrendList = new List<ActualTrendModel>();
                    ActualTacticTrendList = ActualTacticTrendModelList.Where(actual => actual.StageCode.Equals(inqStageCode)).ToList();
                    //ActualList = ActualTacticStageList.Where(actual => actual.StageCode.Equals(inqStageCode)).Select(actual => actual.ActualTacticList).FirstOrDefault();
                    ProjectedTrendList = CalculateProjectedTrend(Tacticdata, includeMonth, inqStageCode);
                    OverviewModelList = GetTacticwiseActualProjectedRevenueList(ActualTacticTrendList, ProjectedTrendList);
                    #endregion

                    #region "Conversion : Set Linechart & Revenue Overview data to model"
                    string INQStageLabel = Common.GetLabel(Common.StageModeINQ);
                    objProjectedGoal = new Projected_Goal();
                    objProjectedGoal = GetRevenueOverviewData(OverviewModelList, timeframeOption);
                    objProjectedGoal.Name = !string.IsNullOrEmpty(INQStageLabel) ? INQStageLabel : inqStageCode;
                    objProjected_Goal_LineChart.linechartdata = new lineChartData();
                    objProjected_Goal_LineChart.projected_goal = objProjectedGoal != null ? objProjectedGoal : new Projected_Goal();
                    objProjected_Goal_LineChart.StageCode = inqStageCode;
                    Projected_Goal_LineChartList.Add(objProjected_Goal_LineChart);
                    objConversionOverviewModel.Projected_LineChartList = Projected_Goal_LineChartList != null ? Projected_Goal_LineChartList : (new List<conversion_Projected_Goal_LineChart>());
                    if (ActualTacticTrendList != null)
                        _inqActual = ActualTacticTrendList.Sum(actual => actual.TrendValue);
                    #endregion
                    #endregion

                    #region "Calculate ProjVsGoal, Linechart & Benchmarck for MQL"
                    #region "Conversion : Get Tacticwise Actual_Projected Vs Goal Model data "

                    ActualList = new List<Plan_Campaign_Program_Tactic_Actual>();
                    ProjectedTrendList = new List<ProjectedTrendModel>();
                    OverviewModelList = new List<TacticwiseOverviewModel>();
                    ActualTacticTrendList = new List<ActualTrendModel>();
                    ActualTacticTrendList = ActualTacticTrendModelList.Where(actual => actual.StageCode.Equals(mqlStageCode)).ToList();
                    //ActualList = ActualTacticStageList.Where(actual => actual.StageCode.Equals(mqlStageCode)).Select(actual => actual.ActualTacticList).FirstOrDefault();
                    ProjectedTrendList = CalculateProjectedTrend(Tacticdata, includeMonth, mqlStageCode);
                    OverviewModelList = GetTacticwiseActualProjectedRevenueList(ActualTacticTrendList, ProjectedTrendList);
                    #endregion

                    #region "Conversion: GetStgewise Benchmark"
                    double Actual_Benchmark_Percentage = 0;
                    List<string> StageCodeList = new List<string>();
                    StageCodeList.Add(strMQLStageCode);
                    StageCodeList.Add(strCWStageCode);
                    StageBenchmarkList = GetStagewiseBenchmark(Tacticdata, StageCodeList);
                    #endregion

                    #region "Get Basic Model"
                    objBasicModel = new BasicModel();
                    objBasicModel = GetValuesListByTimeFrame(ActualTacticTrendList, ProjectedTrendList, timeframeOption, IsQuarterly);
                    #endregion

                    #region "Conversion : Set Linechart & Revenue Overview data to model"
                    string MQLStageLabel = Common.GetLabel(Common.StageModeMQL);
                    objLineChartData = new lineChartData();
                    //objLineChartData = GetLineChartData(ActualTacticTrendList, ProjectedTrendList, timeframeOption, IsQuarterly);
                    objLineChartData = GetLineChartData(objBasicModel);
                    objProjectedGoal = new Projected_Goal();
                    objProjectedGoal = GetRevenueOverviewData(OverviewModelList, timeframeOption);
                    objProjectedGoal.Name = !string.IsNullOrEmpty(MQLStageLabel) ? MQLStageLabel : mqlStageCode;
                    objProjected_Goal_LineChart = new conversion_Projected_Goal_LineChart();
                    objProjected_Goal_LineChart.linechartdata = objLineChartData != null ? objLineChartData : new lineChartData();
                    objProjected_Goal_LineChart.projected_goal = objProjectedGoal != null ? objProjectedGoal : new Projected_Goal();
                    objProjected_Goal_LineChart.StageCode = mqlStageCode;


                    #region "MQL: Set Benchmark Model data"
                    _Benchmark = stageVolumePercntg = 0;
                    if (ActualTacticTrendList != null)
                    {
                        _mqlActual = ActualTacticTrendList.Sum(actual => actual.TrendValue);
                        stageVolumePercntg = _inqActual > 0 ? (_mqlActual / _inqActual) : 0;
                    }

                    _Benchmark = StageBenchmarkList.Where(stage => stage.StageCode.Equals(strMQLStageCode)).Select(stage => stage.Benchmark).FirstOrDefault();
                    Conversion_Benchmark_Model mqlStageBenchmarkmodel = new Conversion_Benchmark_Model();
                    mqlStageBenchmarkmodel.stagename = MQLStageLabel.ToString();
                    mqlStageBenchmarkmodel.stageVolume = stageVolumePercntg.ToString();
                    mqlStageBenchmarkmodel.Benchmark = _Benchmark.ToString();
                    Actual_Benchmark_Percentage = _Benchmark > 0 ? (stageVolumePercntg / _Benchmark) : 0;
                    Actual_Benchmark_Percentage = Actual_Benchmark_Percentage - 100;
                    mqlStageBenchmarkmodel.IsNegativePercentage = Actual_Benchmark_Percentage < 0 ? true : false;
                    mqlStageBenchmarkmodel.PercentageDifference = Actual_Benchmark_Percentage.ToString();
                    #endregion
                    objProjected_Goal_LineChart.Stage_Benchmark = mqlStageBenchmarkmodel;

                    Projected_Goal_LineChartList.Add(objProjected_Goal_LineChart);
                    objConversionOverviewModel.Projected_LineChartList = Projected_Goal_LineChartList != null ? Projected_Goal_LineChartList : (new List<conversion_Projected_Goal_LineChart>());
                    #endregion
                    #endregion

                    #region "Calculate ProjVsGoal, LineChart & Benchmark for CW"
                    #region "Conversion : Get Tacticwise Actual_Projected Vs Goal Model data "

                    ActualList = new List<Plan_Campaign_Program_Tactic_Actual>();
                    ProjectedTrendList = new List<ProjectedTrendModel>();
                    OverviewModelList = new List<TacticwiseOverviewModel>();
                    ActualTacticTrendList = new List<ActualTrendModel>();
                    ActualTacticTrendList = ActualTacticTrendModelList.Where(actual => actual.StageCode.Equals(cwStageCode)).ToList();
                    //ActualList = ActualTacticStageList.Where(actual => actual.StageCode.Equals(cwStageCode)).Select(actual => actual.ActualTacticList).FirstOrDefault();
                    ProjectedTrendList = CalculateProjectedTrend(Tacticdata, includeMonth, cwStageCode);
                    OverviewModelList = GetTacticwiseActualProjectedRevenueList(ActualTacticTrendList, ProjectedTrendList);
                    #endregion


                    #region "Get Basic Model"
                    objBasicModel = new BasicModel();
                    objBasicModel = GetValuesListByTimeFrame(ActualTacticTrendList, ProjectedTrendList, timeframeOption, IsQuarterly);
                    #endregion

                    #region "Conversion : Set Linechart & Revenue Overview data to model"
                    string CWStageLabel = Common.GetLabel(Common.StageModeCW);
                    objLineChartData = new lineChartData();
                    //objLineChartData = GetLineChartData(ActualTacticTrendList, ProjectedTrendList, timeframeOption, IsQuarterly);
                    objLineChartData = GetLineChartData(objBasicModel);
                    objProjectedGoal = new Projected_Goal();
                    objProjectedGoal = GetRevenueOverviewData(OverviewModelList, timeframeOption);
                    objProjectedGoal.Name = !string.IsNullOrEmpty(CWStageLabel) ? CWStageLabel : cwStageCode;
                    objProjected_Goal_LineChart = new conversion_Projected_Goal_LineChart();
                    objProjected_Goal_LineChart.linechartdata = objLineChartData != null ? objLineChartData : new lineChartData();
                    objProjected_Goal_LineChart.projected_goal = objProjectedGoal != null ? objProjectedGoal : new Projected_Goal();
                    objProjected_Goal_LineChart.StageCode = cwStageCode;

                    #region "CW: Set Benchmark Model data"
                    stageVolumePercntg = 0;
                    if (ActualTacticTrendList != null)
                    {
                        _cwActual = ActualTacticTrendList.Sum(actual => actual.TrendValue);
                        stageVolumePercntg = _mqlActual > 0 ? (_cwActual / _mqlActual) : 0;
                    }
                    _Benchmark = 0;
                    _Benchmark = StageBenchmarkList.Where(stage => stage.StageCode.Equals(strCWStageCode)).Select(stage => stage.Benchmark).FirstOrDefault();
                    Conversion_Benchmark_Model cwStageBenchmarkmodel = new Conversion_Benchmark_Model();
                    cwStageBenchmarkmodel.stagename = CWStageLabel;
                    cwStageBenchmarkmodel.stageVolume = stageVolumePercntg.ToString();
                    cwStageBenchmarkmodel.Benchmark = _Benchmark.ToString();
                    Actual_Benchmark_Percentage = _Benchmark > 0 ? (stageVolumePercntg / _Benchmark) : 0;
                    Actual_Benchmark_Percentage = Actual_Benchmark_Percentage - 100;
                    cwStageBenchmarkmodel.IsNegativePercentage = Actual_Benchmark_Percentage < 0 ? true : false;
                    cwStageBenchmarkmodel.PercentageDifference = Actual_Benchmark_Percentage.ToString();
                    #endregion
                    objProjected_Goal_LineChart.Stage_Benchmark = cwStageBenchmarkmodel;

                    Projected_Goal_LineChartList.Add(objProjected_Goal_LineChart);
                    objConversionOverviewModel.Projected_LineChartList = Projected_Goal_LineChartList != null ? Projected_Goal_LineChartList : (new List<conversion_Projected_Goal_LineChart>());
                    #endregion
                    #endregion

                    #endregion

                    #region "Financial related Code"

                    #region "Declare local variables"
                    FinancialOverviewModel objFinanceModel = new FinancialOverviewModel();
                    double _PlanBudget = 0, _TacticTotalBudget = 0;
                    List<int> lstPlanIds = new List<int>();
                    List<int> _TacticIds = new List<int>();
                    List<Plan_Campaign_Program_Tactic> _tacList = new List<Plan_Campaign_Program_Tactic>();
                    List<Plan_Campaign_Program_Tactic_Cost> _tacCostList = new List<Plan_Campaign_Program_Tactic_Cost>();
                    List<Plan_Campaign_Program_Tactic_Actual> _tacActualList = new List<Plan_Campaign_Program_Tactic_Actual>();
                    List<Plan_Campaign_Program_Tactic_Budget> _tacBudgetList = new List<Plan_Campaign_Program_Tactic_Budget>();
                    string RevenueStageType = Enums.InspectStage.Revenue.ToString();
                    List<string> categories = new List<string>();
                    #endregion

                    #region "Budget data"

                    #region "Calculate Total Allocated/UnAllocated Budget for All Selected Plans"

                    lstPlanIds = (List<int>)Sessions.ReportPlanIds;

                    _tacList = GetTacticForReporting(true);
                    _TacticIds = _tacList.Select(tac => tac.PlanTacticId).ToList();

                    _tacBudgetList = db.Plan_Campaign_Program_Tactic_Budget.Where(tac => _TacticIds.Contains(tac.PlanTacticId)).ToList();
                    _TacticTotalBudget = _tacBudgetList != null && _tacBudgetList.Count > 0 ? _tacBudgetList.Sum(budget => budget.Value) : 0;

                    _PlanBudget = db.Plans.Where(plan => lstPlanIds.Contains(plan.PlanId)).Sum(plan => plan.Budget);
                    //List<Plan_Budget> lstPlanBudget = new List<Plan_Budget>();
                    //lstPlanBudget = db.Plan_Budget.Where(budget => lstPlanIds.Contains(budget.PlanId)).ToList();
                    //_TotalAllocatedBudget = lstPlanBudget != null && lstPlanBudget.Count > 0 ? lstPlanBudget.Sum(budget => budget.Value) : 0;

                    objFinanceModel.TotalBudgetAllocated = _TacticTotalBudget;
                    objFinanceModel.TotalBudgetUnAllocated = _PlanBudget - _TacticTotalBudget;

                    #endregion

                    #endregion

                    #region "PlannedCost vs Budget Calculation"

                    _tacCostList = db.Plan_Campaign_Program_Tactic_Cost.Where(tacCost => _TacticIds.Contains(tacCost.PlanTacticId)).ToList();
                    objFinanceModel.PlannedCostvsBudget = _tacCostList.Sum(tacCost => tacCost.Value);

                    List<TacticActualCostModel> TacticActualCostList = new List<TacticActualCostModel>();
                    TacticActualCostList = Common.CalculateActualCostTacticslist(_TacticIds);
                    double _ActualCostvsBudget = 0;
                    if (TacticActualCostList != null)
                        TacticActualCostList.ForEach(tac => _ActualCostvsBudget += tac.ActualList.Sum(actual => actual.Value));
                    objFinanceModel.ActualCostvsBudet = _ActualCostvsBudget;

                    #region "Calculate Barchart data for PlanCost & ActualCost"

                    List<BarChartSeries> lstSeries = new List<BarChartSeries>();
                    BarChartSeries objSeries = new BarChartSeries();
                    List<double> serData = new List<double>();

                    #region "Old PlannedCost vs Budget , ActualCost vs Budget  Barchart Code"
                    #region "Planned_Cost vs Budget Barchart Data"
                    //BarChartModel objPlannedCostModel = new BarChartModel();
                    //objSeries.name = "Planned Cost";
                    //serData.Add(objFinanceModel.PlannedCostvsBudget);
                    //objSeries.data = serData;
                    //lstSeries.Add(objSeries);

                    //objSeries = new BarChartSeries();
                    //serData = new List<double>();
                    //objSeries.name = "Budget";
                    //serData.Add(_TacticTotalBudget);
                    //objSeries.data = serData;
                    //lstSeries.Add(objSeries);

                    //objPlannedCostModel.series = lstSeries;
                    //objPlannedCostModel.categories = new List<string>();

                    #endregion

                    #region "Actual_Cost vs Budget Barchart Data"
                    //BarChartModel objActualCostModel = new BarChartModel();
                    //objSeries = new BarChartSeries();
                    //lstSeries = new List<BarChartSeries>();
                    //serData = new List<double>();

                    //objSeries.name = "Actual Cost";
                    //serData.Add(objFinanceModel.ActualCostvsBudet);
                    //objSeries.data = serData;
                    //lstSeries.Add(objSeries);

                    //objSeries = new BarChartSeries();
                    //serData = new List<double>();
                    //objSeries.name = "Budget";
                    //serData.Add(_TacticTotalBudget);
                    //objSeries.data = serData;
                    //lstSeries.Add(objSeries);

                    //objActualCostModel.series = lstSeries;
                    //objActualCostModel.categories = new List<string>();

                    #endregion
                    #endregion


                    #region "Get Categories based on selected Filter value like {'Monthly','Quarterly'}"
                    if (IsQuarterly)
                    {
                        categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
                    }
                    else
                    {
                        categories = GetDisplayMonthListForReport(timeframeOption); // Get Categories list for Yearly Filter value like {Jan,Feb..}.
                    }
                    #endregion

                    #region "Calculate Barchart data by TimeFrame"
                    BarChartModel objMainModel = new BarChartModel();
                    lstSeries = new List<BarChartSeries>();
                    List<double> btmPlannedCostList = new List<double>();
                    List<double> btmActualCostList = new List<double>();
                    List<double> btmBudgetCostList = new List<double>();
                    double _PlannedCostValue = 0, _ActualCostValue = 0, _BudgetCostValue = 0;

                    if (IsQuarterly)
                    {
                        List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
                        List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
                        List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
                        List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };

                        List<double> serPlannedData = new List<double>();
                        BarChartSeries plannedSeries = new BarChartSeries();
                        plannedSeries.name = "Planned Cost";

                        List<double> serBudgetData = new List<double>();
                        BarChartSeries budgetSeries = new BarChartSeries();
                        budgetSeries.name = "Budget";

                        List<double> scatterData = new List<double>();
                        BarChartSeries objSeriesScatter = new BarChartSeries();
                        objSeriesScatter.name = "Actual Cost";
                        objSeriesScatter.type = "scatter";


                        List<double> PlannedCostList = new List<double>();
                        List<double> ActualCostList = new List<double>();
                        List<double> BudgetCostList = new List<double>();


                        #region "Quarter 1 Calculation"

                        PlannedCostList = _tacCostList.Where(plancost => Q1.Contains(plancost.Period)).Select(plancost => plancost.Value).ToList();
                        TacticActualCostList.ForEach(tactic => _ActualCostValue = tactic.ActualList.Where(actual => Q1.Contains(actual.Period)).Sum(actual => actual.Value));
                        BudgetCostList = _tacBudgetList.Where(budgtcost => Q1.Contains(budgtcost.Period)).Select(budgtcost => budgtcost.Value).ToList();

                        _PlannedCostValue = PlannedCostList.Sum(val => val);
                        serPlannedData.Add(_PlannedCostValue);

                        _BudgetCostValue = BudgetCostList.Sum(val => val);
                        serBudgetData.Add(_BudgetCostValue);

                        scatterData.Add(_ActualCostValue);

                        btmPlannedCostList.Add(_PlannedCostValue);
                        btmActualCostList.Add(_ActualCostValue);
                        btmBudgetCostList.Add(_BudgetCostValue);
                        #endregion

                        #region "Quarter 2 Calculation"
                        PlannedCostList = new List<double>();
                        BudgetCostList = new List<double>();
                        _PlannedCostValue = _ActualCostValue = _BudgetCostValue = 0;

                        PlannedCostList = _tacCostList.Where(plancost => Q2.Contains(plancost.Period)).Select(plancost => plancost.Value).ToList();
                        TacticActualCostList.ForEach(tactic => _ActualCostValue += tactic.ActualList.Where(actual => Q2.Contains(actual.Period)).Sum(actual => actual.Value));
                        BudgetCostList = _tacBudgetList.Where(budgtcost => Q2.Contains(budgtcost.Period)).Select(budgtcost => budgtcost.Value).ToList();

                        _PlannedCostValue = PlannedCostList.Sum(val => val);
                        serPlannedData.Add(_PlannedCostValue);

                        _BudgetCostValue = BudgetCostList.Sum(val => val);
                        serBudgetData.Add(_BudgetCostValue);

                        scatterData.Add(_ActualCostValue);

                        btmPlannedCostList.Add(_PlannedCostValue);
                        btmActualCostList.Add(_ActualCostValue);
                        btmBudgetCostList.Add(_BudgetCostValue);
                        #endregion

                        #region "Quarter 3 Calculation"
                        PlannedCostList = new List<double>();
                        BudgetCostList = new List<double>();
                        _PlannedCostValue = _ActualCostValue = _BudgetCostValue = 0;

                        PlannedCostList = _tacCostList.Where(plancost => Q3.Contains(plancost.Period)).Select(plancost => plancost.Value).ToList();
                        TacticActualCostList.ForEach(tactic => _ActualCostValue += tactic.ActualList.Where(actual => Q3.Contains(actual.Period)).Sum(actual => actual.Value));
                        BudgetCostList = _tacBudgetList.Where(budgtcost => Q3.Contains(budgtcost.Period)).Select(budgtcost => budgtcost.Value).ToList();

                        _PlannedCostValue = PlannedCostList.Sum(val => val);
                        serPlannedData.Add(_PlannedCostValue);

                        _BudgetCostValue = BudgetCostList.Sum(val => val);
                        serBudgetData.Add(_BudgetCostValue);

                        scatterData.Add(_ActualCostValue);

                        btmPlannedCostList.Add(_PlannedCostValue);
                        btmActualCostList.Add(_ActualCostValue);
                        btmBudgetCostList.Add(_BudgetCostValue);
                        #endregion

                        #region "Quarter 4 Calculation"
                        PlannedCostList = new List<double>();
                        BudgetCostList = new List<double>();
                        _PlannedCostValue = _ActualCostValue = _BudgetCostValue = 0;

                        PlannedCostList = _tacCostList.Where(plancost => Q4.Contains(plancost.Period)).Select(plancost => plancost.Value).ToList();
                        TacticActualCostList.ForEach(tactic => _ActualCostValue += tactic.ActualList.Where(actual => Q4.Contains(actual.Period)).Sum(actual => actual.Value));
                        BudgetCostList = _tacBudgetList.Where(budgtcost => Q4.Contains(budgtcost.Period)).Select(budgtcost => budgtcost.Value).ToList();

                        _PlannedCostValue = PlannedCostList.Sum(val => val);
                        serPlannedData.Add(_PlannedCostValue);

                        _BudgetCostValue = BudgetCostList.Sum(val => val);
                        serBudgetData.Add(_BudgetCostValue);

                        scatterData.Add(_ActualCostValue);

                        btmPlannedCostList.Add(_PlannedCostValue);
                        btmActualCostList.Add(_ActualCostValue);
                        btmBudgetCostList.Add(_BudgetCostValue);
                        #endregion

                        plannedSeries.data = serPlannedData;
                        budgetSeries.data = serBudgetData;
                        objSeriesScatter.data = scatterData;

                        lstSeries.Add(budgetSeries);
                        lstSeries.Add(plannedSeries);
                        lstSeries.Add(objSeriesScatter);
                    }
                    else
                    {
                        string curntPeriod = string.Empty;
                        List<double> serPlannedData = new List<double>();
                        BarChartSeries plannedSeries = new BarChartSeries();
                        plannedSeries.name = "Planned Cost";

                        List<double> serBudgetData = new List<double>();
                        BarChartSeries budgetSeries = new BarChartSeries();
                        budgetSeries.name = "Budget";

                        List<double> scatterData = new List<double>();
                        BarChartSeries objSeriesScatter = new BarChartSeries();
                        objSeriesScatter.name = "Actual Cost";
                        objSeriesScatter.type = "scatter";

                        for (int i = 1; i <= categories.Count; i++)
                        {
                            curntPeriod = PeriodPrefix + i;

                            _PlannedCostValue = _tacCostList.Where(plancost => plancost.Period.Equals(curntPeriod)).Select(plancost => plancost.Value).FirstOrDefault();
                            TacticActualCostList.ForEach(tactic => _ActualCostValue += tactic.ActualList.Where(actual => actual.Period.Equals(curntPeriod)).Sum(actual => actual.Value));
                            //ActualCostList = _tacActualList.Where(actualcost => actualcost.Period.Equals(curntPeriod)).Select(actualcost => actualcost.Actualvalue).FirstOrDefault();
                            _BudgetCostValue = _tacBudgetList.Where(budgtcost => budgtcost.Period.Equals(curntPeriod)).Select(budgtcost => budgtcost.Value).FirstOrDefault();

                            //serPlannedData = new List<double>();
                            serPlannedData.Add(_PlannedCostValue);

                            //serBudgetData = new List<double>();
                            serBudgetData.Add(_BudgetCostValue);

                            //scatterData = new List<double>();
                            scatterData.Add(_ActualCostValue);


                            btmPlannedCostList.Add(_PlannedCostValue);
                            btmActualCostList.Add(_ActualCostValue);
                            btmBudgetCostList.Add(_BudgetCostValue);

                        }

                        plannedSeries.data = serPlannedData;
                        budgetSeries.data = serBudgetData;
                        objSeriesScatter.data = scatterData;

                        lstSeries.Add(budgetSeries);
                        lstSeries.Add(plannedSeries);
                        lstSeries.Add(objSeriesScatter);
                    }
                    objMainModel.series = lstSeries;
                    objMainModel.categories = categories;
                    #endregion

                    //objFinanceModel.PlannedCostBarChartModel = objPlannedCostModel;
                    //objFinanceModel.ActualCostBarChartModel = objActualCostModel;
                    objFinanceModel.MainBarChartModel = objMainModel;
                    objFinanceModel.MainPlannedCostList = btmPlannedCostList;
                    objFinanceModel.MainActualCostList = btmActualCostList;
                    objFinanceModel.MainBudgetCostList = btmBudgetCostList;
                    objFinanceModel.CategoriesCount = categories.Count;
                    #endregion

                    #endregion

                    #endregion

                    #region "Set Revenue & Coversion model data to Master Model(i.e ReportOverviewModel)"
                    objReportOverviewModel.revenueOverviewModel = objRevenueOverviewModel;
                    objReportOverviewModel.conversionOverviewModel = objConversionOverviewModel;
                    objReportOverviewModel.financialOverviewModel = objFinanceModel;
                    #endregion
                }
                else
                {
                    objReportOverviewModel.revenueOverviewModel = new RevenueOverviewModel();
                    objReportOverviewModel.conversionOverviewModel = new ConversionOverviewModel();
                    objReportOverviewModel.financialOverviewModel = new FinancialOverviewModel();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return PartialView("_Overview", objReportOverviewModel);
        }

        #region "Revenue report related functions"
        /// <summary>
        /// This function will return the Master data of RevenueOverviewModel.
        /// This function make calculation for each properties like Goal,Percentage, Actual_Projected.
        /// </summary>
        /// <param name="RevenueOverviewModelList"> List of TacticwiseOverviewModel</param>
        /// <param name="timeframeOption">Selected year value from left filter</param>
        /// <returns>Return Model of Revenue_Projected_Goal</returns>
        public Projected_Goal GetRevenueOverviewData(List<TacticwiseOverviewModel> RevenueOverviewModelList, string timeframeOption)
        {
            #region "Declare local variables"
            Projected_Goal objProjGoal = new Projected_Goal();
            double Actual_Projected = 0, Goal = 0, Percentage = 0, ProjvsGoal = 0;
            objProjGoal.IsnegativePercentage = true;
            #endregion
            try
            {
                #region "Calculate Actual_Projected, Goal, Percentage values & set to Model"
                Actual_Projected = RevenueOverviewModelList.Sum(tacMdl => tacMdl.Actual_ProjectedValue); // Make Summed of all Actual_ProjectedValue.
                Goal = RevenueOverviewModelList.Sum(tacMdl => tacMdl.Goal); // Calculate Goal value
                ProjvsGoal = Goal != 0 ? ((Actual_Projected - Goal) / Goal) : 0;
                Percentage = ProjvsGoal * 100; // Calculate Percentage based on Actual_Projected & Goal value.
                if (Percentage > 0)
                    objProjGoal.IsnegativePercentage = false;
                //if(Percentage.Equals())

                objProjGoal.year = "FY " + timeframeOption; // set selected year to Model.
                objProjGoal.Actual_Projected = Actual_Projected.ToString(); // set Actual_Projected to Model.
                objProjGoal.Goal = Goal.ToString(); // set Goal to Model.
                objProjGoal.Percentage = Percentage.ToString(); // set Percentage to Model. 
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objProjGoal;
        }

        /// <summary>
        /// This function will return list of ActualTactic
        /// </summary>
        /// <param name="Tacticdata"> List of Tactic</param>
        /// <param name="timeframeOption">Selected Year from left YearFilter dropdown</param>
        /// <param name="includeMonth"> list of include month to filter TacticData & ActualTactic list</param>
        /// <returns>Return List of ActualTacticList</returns>
        public List<ActualTacticListByStage> GetActualListUpToCurrentMonthByStageCode(List<TacticStageValue> TacticData, string timeframeOption, List<string> StageCodeList, bool IsTillCurrentMonth)
        {
            #region "Declare local Variables"
            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            List<string> includeActualMonth = new List<string>();
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            ActualTacticListByStage objActualTacticListByStage = new ActualTacticListByStage();
            #endregion
            try
            {
                List<string> yearlist = new List<string>();
                yearlist.Add(timeframeOption);
                if (IsTillCurrentMonth)
                {
                    //// Get list of month up to current month based on year list
                    includeActualMonth = GetMonthWithYearUptoCurrentMonth(yearlist);
                }
                else
                {
                    //// Get month list of entire year.
                    includeActualMonth = GetMonthListForReport(timeframeOption);
                }
                TacticData.ForEach(t => t.ActualTacticList.ForEach(a => ActualTacticList.Add(a)));

                foreach (string stagecode in StageCodeList)
                {
                    objActualTacticListByStage = new ActualTacticListByStage();
                    objActualTacticListByStage.StageCode = stagecode;
                    //// Filter ActualTacticlist by RevenueStageCode & IncludeMonth list till current Month ex.{Jan,Feb,March}.
                    objActualTacticListByStage.ActualTacticList = ActualTacticList.Where(actual => actual.StageTitle.Equals(stagecode) && includeActualMonth.Contains((TacticData.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == actual.PlanTacticId).TacticYear) + actual.Period)).ToList();
                    ActualTacticStageList.Add(objActualTacticListByStage);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ActualTacticStageList;
        }

        /// <summary>
        /// This function will return list of ProjectedRevenueTren Model.
        /// This function calculate Monthwise Trend.
        /// </summary>
        /// <param name="TacticList"> List of Tactic</param>
        /// <returns>Return List of Sparklinechart data</returns>
        public List<ActualTrendModel> GetActualTrendModel(List<TacticStageValue> TacticData, List<int> TacticIdList, List<ActualDataTable> curntActualTacticList)
        {
            #region "Declare local Variables"
            List<ActualTrendModel> ActualTrendModelList = new List<ActualTrendModel>();
            ActualTrendModel objActualTrendModel = new ActualTrendModel();
            List<ActualDataTable> fltrActualTacticList = new List<ActualDataTable>();
            List<ActualDataTable> ActualTacticListbyTactic = new List<ActualDataTable>();
            double TotalActualUpToCurrentMonth = 0;
            int involveMonthTillCurrentMonth = 0;
            int _currentYear = Convert.ToInt32(currentYear);
            #endregion

            try
            {
                List<Plan_Campaign_Program_Tactic> TacticList = TacticData.Where(tac => TacticIdList.Contains(tac.TacticObj.PlanTacticId)).Select(tac => tac.TacticObj).ToList();
                int _TacEndMonth = 0, _planTacticId = 0;
                foreach (var tactic in TacticList)
                {
                    _TacEndMonth = tactic.EndDate.Month;
                    _planTacticId = tactic.PlanTacticId;

                    //if (_TacEndMonth > currentMonth)
                    {
                        if (curntActualTacticList != null && curntActualTacticList.Count > 0)
                        {
                            //// Filter CurrentMonthActualTacticList by current PlanTacticId.
                            ActualTacticListbyTactic = curntActualTacticList.Where(actual => actual.PlanTacticId.Equals(_planTacticId)).ToList();

                            //// Get ActualValue sum.
                            TotalActualUpToCurrentMonth = ActualTacticListbyTactic.Sum(actual => actual.ActualValue);

                            //// Get No. of involved month till current month.
                            involveMonthTillCurrentMonth = ActualTacticListbyTactic.Where(actual => actual.ActualValue > 0).Count();
                        }
                        for (int _trendMonth = 1; _trendMonth <= _TacEndMonth; _trendMonth++)
                        {
                            objActualTrendModel = new ActualTrendModel();
                            objActualTrendModel.PlanTacticId = tactic.PlanTacticId;
                            objActualTrendModel.Month = PeriodPrefix + _trendMonth.ToString(); // Set Month like 'Y1','Y2','Y3'..

                            //// Calculate Trend calculation for month that is greater than current ruuning month.
                            if (_trendMonth > currentMonth && involveMonthTillCurrentMonth > 0 && _currentYear >= tactic.StartDate.Year)
                            {
                                objActualTrendModel.TrendValue = (TotalActualUpToCurrentMonth / involveMonthTillCurrentMonth) * _trendMonth;
                            }
                            else
                                objActualTrendModel.TrendValue = 0;
                            ActualTrendModelList.Add(objActualTrendModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ActualTrendModelList;
        }

        /// <summary>
        /// This function will return list of ProjectedRevenueTren Model.
        /// This function calculate Monthwise Trend.
        /// </summary>
        /// <param name="TacticList"> List of Tactic</param>
        /// <returns>Return List of Sparklinechart data</returns>
        public List<ActualTrendModel> GetActualTrendModelForRevenueOverview(List<TacticStageValue> TacticData, List<ActualTacticListByStage> ActualTacticStageList)
        {
            #region "Declare local Variables"
            List<ActualTrendModel> ActualTrendModelList = new List<ActualTrendModel>();
            ActualTrendModel objActualTrendModel = new ActualTrendModel();
            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticListbyTactic = new List<Plan_Campaign_Program_Tactic_Actual>();
            double TotalActualUpToCurrentMonth = 0;
            int involveMonthTillCurrentMonth = 0;
            List<int> TacticIds = new List<int>();
            List<string> ActualMonthList = new List<string>();
            string _Period = string.Empty, CurrentPeriod = PeriodPrefix + currentMonth;
            List<string> StageCodeList = new List<string>();
            StageCodeList = ActualTacticStageList.Distinct().Select(actual => actual.StageCode).ToList();
            List<Plan_Campaign_Program_Tactic> TacticList = new List<Plan_Campaign_Program_Tactic>();
            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            int _currentYear = Convert.ToInt32(currentYear);
            #endregion

            try
            {
                foreach (string stagecode in StageCodeList)
                {

                    TacticList = new List<Plan_Campaign_Program_Tactic>();
                    TacticIds = new List<int>();
                    ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                    ActualTacticList = ActualTacticStageList.Where(actual => actual.StageCode.Equals(stagecode)).Select(actual => actual.ActualTacticList).FirstOrDefault();
                    TacticIds = ActualTacticList.Select(actual => actual.PlanTacticId).ToList();
                    TacticList = TacticData.Where(tac => TacticIds.Contains(tac.TacticObj.PlanTacticId)).Select(tac => tac.TacticObj).ToList();
                    int _TacEndMonth = 0, _planTacticId = 0, _TacStartMonth = 0;

                    foreach (var tactic in TacticList)
                    {
                        if (_currentYear <= tactic.StartDate.Year)
                        {
                            _TacEndMonth = tactic.EndDate.Month;
                        }
                        else
                        {
                            _TacEndMonth = 12;
                        }
                        _planTacticId = tactic.PlanTacticId;

                        #region "Calculate Actual Trend"
                        if (ActualTacticList != null && ActualTacticList.Count > 0)
                        {
                            ActualTacticListbyTactic = new List<Plan_Campaign_Program_Tactic_Actual>();
                            //// Filter CurrentMonthActualTacticList by current PlanTacticId.
                            ActualTacticListbyTactic = ActualTacticList.Where(actual => actual.PlanTacticId.Equals(_planTacticId)).ToList();

                            //// Get ActualValue sum.
                            TotalActualUpToCurrentMonth = ActualTacticListbyTactic.Sum(actual => actual.Actualvalue);

                            //// Get No. of involved month till current month.
                            involveMonthTillCurrentMonth = ActualTacticListbyTactic.Where(actual => actual.Actualvalue > 0).Count();
                        }
                        for (int _trendMonth = 1; _trendMonth <= _TacEndMonth; _trendMonth++)
                        {
                            objActualTrendModel = new ActualTrendModel();
                            objActualTrendModel.PlanTacticId = tactic.PlanTacticId;
                            objActualTrendModel.Month = PeriodPrefix + _trendMonth.ToString(); // Set Month like 'Y1','Y2','Y3'..
                            objActualTrendModel.StageCode = stagecode;

                            //// Calculate Trend calculation for month that is greater than current ruuning month.
                            if (involveMonthTillCurrentMonth > 0 && (_currentYear < tactic.StartDate.Year || (_trendMonth > currentMonth && _currentYear == tactic.StartDate.Year)))
                            {
                                objActualTrendModel.TrendValue = (TotalActualUpToCurrentMonth / involveMonthTillCurrentMonth);
                            }
                            else if (_currentYear > tactic.StartDate.Year || (_currentYear == tactic.StartDate.Year && _trendMonth <= currentMonth)) // Set Same ActualValue as Trend value till current month from Tactic StartMonth.
                            {
                                CurrentPeriod = PeriodPrefix + _trendMonth.ToString();
                                objActualTrendModel.TrendValue = ActualTacticListbyTactic.Where(actual => actual.Period.Equals(CurrentPeriod)).Select(actual => actual.Actualvalue).FirstOrDefault();
                            }
                            else
                                objActualTrendModel.TrendValue = 0;
                            ActualTrendModelList.Add(objActualTrendModel);
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ActualTrendModelList;
        }

        /// <summary>
        /// This function will return list of ProjectedRevenueTren Model.
        /// This function calculate Monthwise Trend.
        /// </summary>
        /// <param name="TacticList"> List of Tactic</param>
        /// <returns>Return List of Sparklinechart data</returns>
        public List<ActualTrendModel> GetActualTrendModelForRevenue(List<TacticStageValue> TacticData, List<ActualDataTable> ActualTacticDataList, string strStageCode)
        {
            #region "Declare local Variables"
            List<ActualTrendModel> ActualTrendModelList = new List<ActualTrendModel>();
            ActualTrendModel objActualTrendModel = new ActualTrendModel();
            List<ActualDataTable> ActualTacticListbyTactic = new List<ActualDataTable>();
            double TotalActualUpToCurrentMonth = 0;
            int involveMonthTillCurrentMonth = 0;
            List<int> TacticIds = new List<int>();
            List<string> ActualMonthList = new List<string>();
            string _Period = string.Empty, CurrentPeriod = PeriodPrefix + currentMonth;
            List<Plan_Campaign_Program_Tactic> TacticList = new List<Plan_Campaign_Program_Tactic>();
            int _currentYear = Convert.ToInt32(currentYear);
            #endregion

            try
            {
                TacticList = new List<Plan_Campaign_Program_Tactic>();
                TacticIds = new List<int>();
                TacticIds = ActualTacticDataList.Select(actual => actual.PlanTacticId).Distinct().ToList();
                TacticList = TacticData.Where(tac => TacticIds.Contains(tac.TacticObj.PlanTacticId)).Select(tac => tac.TacticObj).ToList();
                int _TacEndMonth = 0, _planTacticId = 0, _TacStartMonth = 0;

                foreach (var tactic in TacticList)
                {
                    if (_currentYear <= tactic.StartDate.Year)
                    {
                        _TacEndMonth = tactic.EndDate.Month;
                    }
                    else
                    {
                        _TacEndMonth = 12;
                    }
                    _planTacticId = tactic.PlanTacticId;
                    _TacStartMonth = tactic.StartDate.Month;

                    #region "Calculate Actual Trend"

                    if (ActualTacticDataList != null && ActualTacticDataList.Count > 0)
                    {
                        ActualTacticListbyTactic = new List<ActualDataTable>();
                        //// Filter CurrentMonthActualTacticList by current PlanTacticId.
                        ActualTacticListbyTactic = ActualTacticDataList.Where(actual => actual.PlanTacticId.Equals(_planTacticId)).ToList();

                        //// Get ActualValue sum.
                        TotalActualUpToCurrentMonth = ActualTacticListbyTactic.Sum(actual => actual.ActualValue);

                        //// Get No. of involved month till current month.
                        involveMonthTillCurrentMonth = ActualTacticListbyTactic.Where(actual => actual.ActualValue > 0).Count();
                    }
                    for (int _trendMonth = 1; _trendMonth <= _TacEndMonth; _trendMonth++)
                    {
                        objActualTrendModel = new ActualTrendModel();
                        objActualTrendModel.PlanTacticId = tactic.PlanTacticId;
                        objActualTrendModel.Month = PeriodPrefix + _trendMonth.ToString(); // Set Month like 'Y1','Y2','Y3'..
                        objActualTrendModel.StageCode = strStageCode;

                        //// Calculate Trend calculation for month that is greater than current ruuning month.
                        if (involveMonthTillCurrentMonth > 0 && (_currentYear < tactic.StartDate.Year || (_trendMonth > currentMonth && _currentYear == tactic.StartDate.Year)))
                        {
                            objActualTrendModel.TrendValue = (TotalActualUpToCurrentMonth / involveMonthTillCurrentMonth);
                        }
                        else if (_currentYear > tactic.StartDate.Year || (_currentYear == tactic.StartDate.Year && _trendMonth <= currentMonth)) // Set Same ActualValue as Trend value till current month from Tactic StartMonth.
                        {
                            CurrentPeriod = PeriodPrefix + _trendMonth.ToString();
                            objActualTrendModel.TrendValue = ActualTacticListbyTactic.Where(actual => actual.Period.Equals(CurrentPeriod)).Select(actual => actual.ActualValue).FirstOrDefault();
                        }
                        else
                            objActualTrendModel.TrendValue = 0;
                        ActualTrendModelList.Add(objActualTrendModel);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ActualTrendModelList;
        }

        /// <summary>
        /// This function will return list of ProjectedRevenueTren Model.
        /// This function calculate Monthwise Trend.
        /// </summary>
        /// <param name="TacticList"> List of Tactic</param>
        /// <returns>Return List of Sparklinechart data</returns>
        public List<ActualTrendModel> GetActualCostTrendModel(List<TacticStageValue> TacticData, List<int> TacticIdList, List<TacticMonthValue> curntCostList)
        {
            #region "Declare local Variables"
            List<ActualTrendModel> CostTrendModelList = new List<ActualTrendModel>();
            ActualTrendModel objActualTrendModel = new ActualTrendModel();
            List<TacticMonthValue> CostListbyTactic = new List<TacticMonthValue>();
            double TotalCostUpToCurrentMonth = 0;
            int involveMonthTillCurrentMonth = 0;
            int _currentYear = Convert.ToInt32(currentYear);
            #endregion

            try
            {
                List<Plan_Campaign_Program_Tactic> TacticList = TacticData.Where(tac => TacticIdList.Contains(tac.TacticObj.PlanTacticId)).Select(tac => tac.TacticObj).ToList();
                int _TacEndMonth = 0, _planTacticId = 0;
                foreach (var tactic in TacticList)
                {
                    _TacEndMonth = tactic.EndDate.Month;
                    _planTacticId = tactic.PlanTacticId;

                    //if (_TacEndMonth > currentMonth)
                    {
                        if (curntCostList != null && curntCostList.Count > 0)
                        {
                            //// Filter CurrentMonthActualTacticList by current PlanTacticId.
                            CostListbyTactic = curntCostList.Where(tac => tac.Id.Equals(_planTacticId)).ToList();

                            //// Get ActualValue sum.
                            TotalCostUpToCurrentMonth = CostListbyTactic.Sum(_cost => _cost.Value);

                            //// Get No. of involved month till current month.
                            involveMonthTillCurrentMonth = CostListbyTactic.Where(actual => actual.Value > 0).Count();
                        }
                        for (int _trendMonth = 1; _trendMonth <= _TacEndMonth; _trendMonth++)
                        {
                            objActualTrendModel = new ActualTrendModel();
                            objActualTrendModel.PlanTacticId = tactic.PlanTacticId;
                            objActualTrendModel.Month = PeriodPrefix + _trendMonth.ToString(); // Set Month like 'Y1','Y2','Y3'..

                            //// Calculate Trend calculation for month that is greater than current ruuning month.
                            if (_trendMonth > currentMonth && involveMonthTillCurrentMonth > 0 && _currentYear >= tactic.StartDate.Year)
                            {
                                objActualTrendModel.TrendValue = (TotalCostUpToCurrentMonth / involveMonthTillCurrentMonth) * _trendMonth;
                            }
                            else
                                objActualTrendModel.TrendValue = 0;
                            CostTrendModelList.Add(objActualTrendModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return CostTrendModelList;
        }

        /// <summary>
        /// This function will return the list of Months involved in each Tactic.
        /// </summary>
        /// <param name="Tacticdata"> List of Tactic</param>
        /// <returns>Return List of TacticMonthInterval data</returns>
        public List<TacticMonthInterval> GetTacticMonthInterval(List<TacticStageValue> TacticData)
        {
            try
            {
                #region "Decalre Local Variables"
                List<TacticMonthInterval> listTacticMonthValue = new List<TacticMonthInterval>();
                Plan_Campaign_Program_Tactic objTactic = null;
                int _startyear = 0, _endyear = 0, _startmonth = 0, _endmonth = 0;
                #endregion

                foreach (TacticStageValue tactic in TacticData)
                {
                    objTactic = new Plan_Campaign_Program_Tactic();
                    objTactic = tactic.TacticObj;

                    #region "Get StartMonth,EndMonth, StartYear,EndYear Of Current Tactic"
                    _startyear = objTactic.StartDate.Year;
                    _endyear = objTactic.EndDate.Year;
                    _startmonth = objTactic.StartDate.Month;
                    _endmonth = objTactic.EndDate.Month;
                    #endregion

                    if (_startyear == _endyear) // If Tactic's StartYear is equal to EndYear
                    {
                        if (_startmonth == _endmonth) // If Tactic's StartMonth is equal to EndMonth
                        {
                            // Add Single record to List of TacticMonthValue.
                            listTacticMonthValue.Add(new TacticMonthInterval { PlanTacticId = objTactic.PlanTacticId, Month = _startyear + PeriodPrefix + _startmonth });
                        }
                        else
                        {
                            // Add record into TacticMonthInterval list from StartMonth to EndMonth.
                            for (var i = _startmonth; i <= _endmonth; i++)
                            {
                                listTacticMonthValue.Add(new TacticMonthInterval { PlanTacticId = objTactic.PlanTacticId, Month = _startyear + PeriodPrefix + i });
                            }
                        }
                    }
                    else
                    {
                        for (var i = _startmonth; i <= 12; i++)
                        {
                            listTacticMonthValue.Add(new TacticMonthInterval { PlanTacticId = objTactic.PlanTacticId, Month = _startyear + PeriodPrefix + i });
                        }
                        for (var i = 1; i <= _endmonth + 1; i++)
                        {
                            listTacticMonthValue.Add(new TacticMonthInterval { PlanTacticId = objTactic.PlanTacticId, Month = _endyear + PeriodPrefix + i });
                        }
                    }
                }
                return listTacticMonthValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This function will return the Model of SparkLinechart to GetOverviewData function.
        /// </summary>
        /// <param name="Tacticdata"> List of Tactic</param>
        /// <param name="ActualTacticList"> List of Actuals Tactic</param>
        /// <returns>Return Sparkline chart Model</returns>
        public List<sparkLineCharts> GetRevenueSparkLineChartData(string strCustomField, string timeFrameOption, List<Enums.TOPRevenueType> RevenueTypeList, bool IsFirst = false)
        {
            #region "Declare Local Variables"
            List<string> categories = new List<string>();
            List<sparkLineCharts> SparkLineCharts = new List<sparkLineCharts>();
            List<sparklineData> lstSparklineData = new List<sparklineData>();
            sparklineData _sparklinedata = new sparklineData();
            sparkLineCharts objsparkLineCharts = new sparkLineCharts();
            int _noOfSparklineChart = RevenueTypeList.Count();
            string _IdPrefix = "table-sparkline_";
            string _CustomDDLPrefix = "custmDDL_";
            Enums.TOPRevenueType CurrentRevenueType = new Enums.TOPRevenueType();
            string selectedCustomField = strCustomField;
            List<string> IncludeCurrentMonth = new List<string>();
            #endregion

            try
            {
                List<TacticStageValue> TacticData = (List<TacticStageValue>)TempData["ReportData"];
                TempData["ReportData"] = TempData["ReportData"];

                List<string> includeMonth = GetMonthListForReport(timeFrameOption);

                //// Get SparkLine chart data for each RevenueType
                Enums.TOPRevenueType revType = new Enums.TOPRevenueType();

                #region "Common code for all StageCode"

                #region "Declare local variables"
                bool IsTacticCustomField = false, IsProgramCustomField = false, IsCampaignCustomField = false;
                string customFieldType = string.Empty;
                int customfieldId = 0;
                List<RevenueContrinutionData> CustomFieldOptionList = new List<RevenueContrinutionData>();
                #endregion

                #region "Get CustomField Id & set IsTacticCustomField,IsCampaignCustomField,IsProgramCustomField by selecte CustomField label"
                if (strCustomField.Contains(Common.TacticCustomTitle))
                {
                    customfieldId = Convert.ToInt32(strCustomField.Replace(Common.TacticCustomTitle, ""));
                    IsTacticCustomField = true;
                }
                else if (strCustomField.Contains(Common.CampaignCustomTitle))
                {
                    customfieldId = Convert.ToInt32(strCustomField.Replace(Common.CampaignCustomTitle, ""));
                    IsCampaignCustomField = true;
                }
                else if (strCustomField.Contains(Common.ProgramCustomTitle))
                {
                    customfieldId = Convert.ToInt32(strCustomField.Replace(Common.ProgramCustomTitle, ""));
                    IsProgramCustomField = true;
                }
                #endregion

                if (strCustomField.Contains(Common.TacticCustomTitle) || strCustomField.Contains(Common.CampaignCustomTitle) || strCustomField.Contains(Common.ProgramCustomTitle))
                {

                    #region "Entity list based on CustomFieldId"
                    List<int> entityids = new List<int>();
                    if (IsTacticCustomField)
                    {
                        entityids = TacticData.Select(t => t.TacticObj.PlanTacticId).ToList();
                    }
                    else if (IsCampaignCustomField)
                    {
                        entityids = TacticData.Select(t => t.TacticObj.Plan_Campaign_Program.PlanCampaignId).ToList();
                    }
                    else
                    {
                        entityids = TacticData.Select(t => t.TacticObj.PlanProgramId).ToList();
                    }
                    #endregion

                    customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                    var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();

                    #region " Get CustomField Option list based on CustomFieldType"
                    if (customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                    {
                        var optionlist = cusomfieldEntity.Select(c => Convert.ToInt32(c.Value)).ToList();
                        CustomFieldOptionList = (from cfo in db.CustomFieldOptions
                                                 where cfo.CustomFieldId == customfieldId && optionlist.Contains(cfo.CustomFieldOptionId) && cfo.IsDeleted == false
                                                 select cfo).ToList().GroupBy(pc => new { id = pc.CustomFieldOptionId, title = pc.Value }).Select(pc =>
                                      new RevenueContrinutionData
                                      {
                                          Title = pc.Key.title,
                                          CustomFieldOptionid = pc.Key.id,
                                          planTacticList = TacticData.Where(t => cusomfieldEntity.Where(c => c.Value == pc.Key.id.ToString()).Select(c => c.EntityId).ToList().Contains(IsCampaignCustomField ? t.TacticObj.Plan_Campaign_Program.PlanCampaignId :
                                              (IsProgramCustomField ? t.TacticObj.PlanProgramId : t.TacticObj.PlanTacticId))).Select(t => t.TacticObj.PlanTacticId).ToList()
                                      }).ToList();

                    }
                    else if (customFieldType == Enums.CustomFieldType.TextBox.ToString())
                    {
                        CustomFieldOptionList = cusomfieldEntity.GroupBy(pc => new { title = pc.Value }).Select(pc =>
                                    new RevenueContrinutionData
                                    {
                                        Title = pc.Key.title,
                                        planTacticList = pc.Select(c => c.EntityId).ToList()
                                    }).ToList();
                    }
                    #endregion

                    #region "Get Year list"
                    List<string> yearlist = new List<string>();
                    yearlist.Add(timeFrameOption);
                    IncludeCurrentMonth = GetMonthWithYearUptoCurrentMonth(yearlist);
                    #endregion
                }
                #endregion

                for (int row = 1; row <= _noOfSparklineChart; row++)
                {
                    objsparkLineCharts = new sparkLineCharts();
                    revType = new Enums.TOPRevenueType();
                    revType = RevenueTypeList[row - 1];
                    lstSparklineData = new List<sparklineData>();
                    lstSparklineData = GetActualRevenueTrendData(customfieldId, customFieldType, IsTacticCustomField, TacticData, revType, timeFrameOption, CustomFieldOptionList, IncludeCurrentMonth);

                    objsparkLineCharts.sparklinechartdata = lstSparklineData;

                    if (row % 2 == 0)
                    {
                        objsparkLineCharts.IsOddSequence = false;
                    }
                    else
                        objsparkLineCharts.IsOddSequence = true;

                    #region "Set Revenue Type Sequence "
                    CurrentRevenueType = new Enums.TOPRevenueType();
                    CurrentRevenueType = RevenueTypeList[row - 1];
                    if (CurrentRevenueType.Equals(Enums.TOPRevenueType.Revenue)) // TOP Revenue By
                    {
                        objsparkLineCharts.RevenueTypeColumns = Common.TOPRevenueColumnList;
                    }
                    else if (CurrentRevenueType.Equals(Enums.TOPRevenueType.Performance)) // TOP Performance By
                    {
                        objsparkLineCharts.RevenueTypeColumns = Common.TOPPerformanceColumnList;
                    }
                    else if (CurrentRevenueType.Equals(Enums.TOPRevenueType.Cost)) // TOP Cost By
                    {
                        objsparkLineCharts.RevenueTypeColumns = Common.TOPCostColumnList;
                    }
                    else if (CurrentRevenueType.Equals(Enums.TOPRevenueType.ROI)) // TOP ROI By
                    {
                        objsparkLineCharts.RevenueTypeColumns = Common.TOPROIColumnList;
                    }
                    #endregion

                    #region "Set SparkLineChart Master data to Model"
                    objsparkLineCharts.sparklinechartId = _IdPrefix + CurrentRevenueType.ToString();
                    objsparkLineCharts.CustomfieldDDLId = _CustomDDLPrefix + CurrentRevenueType.ToString();
                    objsparkLineCharts.ChartHeader = Common.objCached.RevenueSparklineChartHeader.Replace("{0}", CurrentRevenueType.ToString());
                    objsparkLineCharts.TOPRevenueType = CurrentRevenueType;
                    #endregion

                    SparkLineCharts.Add(objsparkLineCharts);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return PartialView("_SparkLineChart", SparkLineCharts);
            return SparkLineCharts;
        }

        /// <summary>
        /// This function will return the data of Spark Line chart based on RevenueType
        /// </summary>
        /// <param name="strCustomField"> Selected CustomField value from Dropdownlist</param>
        /// <param name="Tacticdata"> List of Tactic</param>
        /// <param name="ActualTacticList"> List of Actuals Tactic</param>
        /// <param name="RevenueType"> This is Enum RevenueType like Revenue, Performance, Cost, ROI for that this function return data of Sparklinechart</param>
        /// <returns>Return List of Sparklinechart data</returns>
        public List<sparklineData> GetActualRevenueTrendData(int customfieldId, string customFieldType, bool IsTacticCustomField, List<TacticStageValue> Tacticdata, Enums.TOPRevenueType RevenueType, string timeFrameOption, List<RevenueContrinutionData> CustomFieldOptionList, List<string> IncludeCurrentMonth)
        {
            #region "Declare local variables"
            //bool IsTacticCustomField = false, IsProgramCustomField = false, IsCampaignCustomField = false;
            //string customFieldType = string.Empty;
            //int customfieldId = 0;
            //List<RevenueContrinutionData> CustomFieldOptionList = new List<RevenueContrinutionData>();
            List<sparkLineCharts> ListSparkLineChartsData = new List<sparkLineCharts>();
            sparklineData _sparklinedata = new sparklineData();
            List<sparklineData> lstSparklineData = new List<sparklineData>();
            List<sparklineData> resultSparklineData = new List<sparklineData>();
            bool IsQuarterly = true;
            //strCustomField = !string.IsNullOrEmpty(strCustomField) ? strCustomField : string.Empty;
            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            string revStageCode = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            List<string> revStageCodeList = new List<string> { revStageCode };
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<ActualTacticListByStage> ActualStageList = new List<ActualTacticListByStage>();
            #region "Quarterly Trend Varaibles"
            List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };
            string strActual, strProjected, strTrendValue;
            double ActualQ1 = 0, ActualQ2 = 0, ActualQ3 = 0, ActualQ4 = 0, TrendQ1 = 0, TrendQ2 = 0, TrendQ3 = 0, TrendQ4 = 0, TotalRevenueTypeCol = 0, TotalTrendQ1 = 0, TotalTrendQ2 = 0, TotalTrendQ3 = 0, TotalTrendQ4 = 0;
            #endregion

            #endregion

            try
            {
                #region " Old Code "
                //#region "Get CustomField Id & set IsTacticCustomField,IsCampaignCustomField,IsProgramCustomField by selecte CustomField label"
                //if (strCustomField.Contains(Common.TacticCustomTitle))
                //{
                //    customfieldId = Convert.ToInt32(strCustomField.Replace(Common.TacticCustomTitle, ""));
                //    IsTacticCustomField = true;
                //}
                //else if (strCustomField.Contains(Common.CampaignCustomTitle))
                //{
                //    customfieldId = Convert.ToInt32(strCustomField.Replace(Common.CampaignCustomTitle, ""));
                //    IsCampaignCustomField = true;
                //}
                //else if (strCustomField.Contains(Common.ProgramCustomTitle))
                //{
                //    customfieldId = Convert.ToInt32(strCustomField.Replace(Common.ProgramCustomTitle, ""));
                //    IsProgramCustomField = true;
                //}
                //#endregion 
                #endregion

                //if (strCustomField.Contains(Common.TacticCustomTitle) || strCustomField.Contains(Common.CampaignCustomTitle) || strCustomField.Contains(Common.ProgramCustomTitle))
                //{
                #region "Old Code"
                //#region "Entity list based on CustomFieldId"
                //List<int> entityids = new List<int>();
                //if (IsTacticCustomField)
                //{
                //    entityids = Tacticdata.Select(t => t.TacticObj.PlanTacticId).ToList();
                //}
                //else if (IsCampaignCustomField)
                //{
                //    entityids = Tacticdata.Select(t => t.TacticObj.Plan_Campaign_Program.PlanCampaignId).ToList();
                //}
                //else
                //{
                //    entityids = Tacticdata.Select(t => t.TacticObj.PlanProgramId).ToList();
                //}
                //#endregion

                //customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                //var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();

                //#region " Get CustomField Option list based on CustomFieldType"
                //if (customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                //{
                //    var optionlist = cusomfieldEntity.Select(c => Convert.ToInt32(c.Value)).ToList();
                //    CustomFieldOptionList = (from cfo in db.CustomFieldOptions
                //                             where cfo.CustomFieldId == customfieldId && optionlist.Contains(cfo.CustomFieldOptionId)
                //                             select cfo).ToList().GroupBy(pc => new { id = pc.CustomFieldOptionId, title = pc.Value }).Select(pc =>
                //                  new RevenueContrinutionData
                //                  {
                //                      Title = pc.Key.title,
                //                      CustomFieldOptionid = pc.Key.id,
                //                      // Modified By : Kalpesh Sharma Filter changes for Revenue report - Revenue Report
                //                      // Fetch the filtered list based upon custom fields type
                //                      planTacticList = Tacticdata.Where(t => cusomfieldEntity.Where(c => c.Value == pc.Key.id.ToString()).Select(c => c.EntityId).ToList().Contains(IsCampaignCustomField ? t.TacticObj.Plan_Campaign_Program.PlanCampaignId :
                //                          (IsProgramCustomField ? t.TacticObj.PlanProgramId : t.TacticObj.PlanTacticId))).Select(t => t.TacticObj.PlanTacticId).ToList()
                //                  }).ToList();

                //}
                //else if (customFieldType == Enums.CustomFieldType.TextBox.ToString())
                //{
                //    CustomFieldOptionList = cusomfieldEntity.GroupBy(pc => new { title = pc.Value }).Select(pc =>
                //                new RevenueContrinutionData
                //                {
                //                    Title = pc.Key.title,
                //                    planTacticList = pc.Select(c => c.EntityId).ToList()
                //                }).ToList();
                //}
                //#endregion

                //List<string> yearlist = new List<string>();
                //yearlist.Add(timeFrameOption);
                //List<string> IncludeCurrentMonth = GetMonthWithYearUptoCurrentMonth(yearlist); 
                #endregion

                if (RevenueType.Equals(Enums.TOPRevenueType.Revenue))
                {
                    #region "Code for TOPRevenue"
                    #region "Declare Local Variables"
                    double TotalActualValueCurrentMonth = 0;
                    List<Plan_Campaign_Program_Tactic_Actual> lstActuals = new List<Plan_Campaign_Program_Tactic_Actual>();
                    string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
                    List<ActualDataTable> ActualDataTable = new List<ActualDataTable>();
                    List<ActualDataTable> CurrentMonthActualTacticList = new List<ActualDataTable>();
                    #endregion

                    #region "Evaluate Customfield Option wise Sparkline chart data"
                    ActualTacticStageList = GetActualListUpToCurrentMonthByStageCode(Tacticdata, timeFrameOption, revStageCodeList, false);
                    if (ActualTacticStageList != null)
                    {
                        ActualTacticList = ActualTacticStageList.Where(act => act.StageCode.Equals(revStageCode)).Select(act => act.ActualTacticList).FirstOrDefault();
                    }
                    foreach (RevenueContrinutionData _obj in CustomFieldOptionList)
                    {

                        #region "Get Actuals List"
                        lstActuals = ActualTacticList.Where(ta => _obj.planTacticList.Contains(ta.PlanTacticId)).ToList();
                        //// Get Actuals Tactic list by weightage for Revenue.
                        ActualDataTable = GetActualTacticDataTablebyStageCode(customfieldId, _obj.CustomFieldOptionid.ToString(), customFieldType, Enums.InspectStage.Revenue, lstActuals, Tacticdata, IsTacticCustomField);

                        //// Get ActualList upto CurrentMonth.
                        CurrentMonthActualTacticList = ActualDataTable.Where(actual => IncludeCurrentMonth.Contains(Tacticdata.Where(tac => tac.TacticObj.PlanTacticId.Equals(actual.PlanTacticId)).FirstOrDefault().TacticYear + actual.Period)).ToList();
                        TotalActualValueCurrentMonth = CurrentMonthActualTacticList.Sum(ta => ta.ActualValue); // Get Total of Actual Revenue value. 
                        TotalRevenueTypeCol = TotalRevenueTypeCol + TotalActualValueCurrentMonth;

                        //List<ActualTrendModel> ActualTrendModelList = GetActualTrendModel(Tacticdata, _obj.planTacticList, CurrentMonthActualTacticList);
                        #endregion

                        #region "Set Sparkline chart Data"
                        _sparklinedata = new sparklineData();
                        _sparklinedata.Name = _obj.Title;
                        _sparklinedata.RevenueTypeValue = TotalActualValueCurrentMonth.ToString();
                        _sparklinedata.IsPositive = true;
                        _sparklinedata.Value = TotalActualValueCurrentMonth;
                        _sparklinedata.Tooltip_Prefix = strCurrency.ToString();
                        _sparklinedata.Tooltip_Suffix = string.Empty;
                        _sparklinedata.IsTotal = false;
                        #endregion

                        #region "Calcualte Actual & Projected value Quarterly"
                        if (IsQuarterly)
                        {
                            strActual = strProjected = strTrendValue = string.Empty;
                            ActualQ1 = ActualQ2 = ActualQ3 = ActualQ4 = TrendQ1 = TrendQ2 = TrendQ3 = TrendQ4 = 0;

                            ActualQ1 = CurrentMonthActualTacticList.Where(actual => Q1.Contains(actual.Period)).Sum(actual => actual.ActualValue);
                            ActualQ2 = ActualQ1 + (CurrentMonthActualTacticList.Where(actual => Q2.Contains(actual.Period)).Sum(actual => actual.ActualValue));
                            ActualQ3 = ActualQ2 + (CurrentMonthActualTacticList.Where(actual => Q3.Contains(actual.Period)).Sum(actual => actual.ActualValue));
                            ActualQ4 = ActualQ3 + (CurrentMonthActualTacticList.Where(actual => Q4.Contains(actual.Period)).Sum(actual => actual.ActualValue));

                            #region "Old Code"
                            //TrendQ1 = ActualTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q1.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //TrendQ2 = ActualTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q2.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //TrendQ3 = ActualTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q3.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //TrendQ4 = ActualTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q4.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //TotalTrendQ1 = TotalTrendQ1 + (ActualQ1 + TrendQ1);
                            //TotalTrendQ2 = TotalTrendQ2 + (ActualQ2 + TrendQ2);
                            //TotalTrendQ3 = TotalTrendQ3 + (ActualQ3 + TrendQ3);
                            //TotalTrendQ4 = TotalTrendQ4 + (ActualQ4 + TrendQ4); 
                            #endregion

                            TotalTrendQ1 = TotalTrendQ1 + (ActualQ1);
                            TotalTrendQ2 = TotalTrendQ2 + (ActualQ2);
                            TotalTrendQ3 = TotalTrendQ3 + (ActualQ3);
                            TotalTrendQ4 = TotalTrendQ4 + (ActualQ4);
                            //strTrendValue = string.Join(", ", new List<string> { (ActualQ1 + TrendQ1).ToString(), (ActualQ2 + TrendQ2).ToString(), (ActualQ3 + TrendQ3).ToString(), (ActualQ4 + TrendQ4).ToString() });
                            strTrendValue = string.Join(", ", new List<string> { (ActualQ1).ToString(), (ActualQ2).ToString(), (ActualQ3).ToString(), (ActualQ4).ToString() });
                            _sparklinedata.Trend = strTrendValue;
                        }
                        #endregion

                        lstSparklineData.Add(_sparklinedata);
                    }
                    resultSparklineData = lstSparklineData.OrderByDescending(data => data.Value).Take(5).ToList();
                    #region "Add TOTAL Sparkline record to list"
                    _sparklinedata = new sparklineData();
                    _sparklinedata.Name = "Total";
                    _sparklinedata.RevenueTypeValue = TotalRevenueTypeCol.ToString();
                    _sparklinedata.IsPositive = true;
                    _sparklinedata.Trend = string.Join(", ", new List<string> { TotalTrendQ1.ToString(), TotalTrendQ2.ToString(), TotalTrendQ3.ToString(), TotalTrendQ4.ToString() });
                    _sparklinedata.IsTotal = true;
                    _sparklinedata.Tooltip_Prefix = strCurrency.ToString();
                    _sparklinedata.Tooltip_Suffix = string.Empty;
                    resultSparklineData.Add(_sparklinedata);
                    #endregion

                    #endregion
                    #endregion
                }
                else if (RevenueType.Equals(Enums.TOPRevenueType.Performance))
                {
                    #region "Declare Local Variables"
                    double Proj_Goal = 0, Actual_Projected = 0, Goal = 0;
                    //List<Plan_Campaign_Program_Tactic_Actual> lstActuals = new List<Plan_Campaign_Program_Tactic_Actual>();
                    //string revenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
                    List<ActualDataTable> ActualDataTable = new List<ActualDataTable>();
                    List<TacticDataTable> TacticDataTable = new List<TacticDataTable>();
                    List<ProjectedTrendModel> ProjectedRevenueTrendList = new List<ProjectedTrendModel>();
                    List<ProjectedTacticModel> TacticList = new List<ProjectedTacticModel>();
                    List<Plan_Campaign_Program_Tactic_Actual> CurrentActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                    List<TacticStageValue> fltrTacticData = new List<TacticStageValue>();
                    List<Plan_Campaign_Program_Tactic_Actual> TrendActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                    List<Plan_Campaign_Program_Tactic_Actual> fltrTrendActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                    List<TacticMonthValue> TacticListMonth = new List<TacticMonthValue>();
                    string strRevenueTypeColumn = string.Empty;
                    double Act_ProjQ1 = 0, Act_ProjQ2 = 0, Act_ProjQ3 = 0, Act_ProjQ4 = 0, GoalQ1 = 0, GoalQ2 = 0, GoalQ3 = 0, GoalQ4 = 0, Proj_GoalQ1 = 0, Proj_GoalQ2 = 0, Proj_GoalQ3 = 0, Proj_GoalQ4 = 0;
                    #endregion

                    #region "Evaluate Customfield Option wise Sparkline chart data"
                    bool IsTillCurrentMonth = true;
                    ActualStageList = GetActualListInTacticInterval(Tacticdata, timeFrameOption, revStageCodeList, IsTillCurrentMonth);
                    if (ActualStageList != null)
                    {
                        ActualTacticList = ActualStageList.Where(act => act.StageCode.Equals(revStageCode)).Select(act => act.ActualTacticList).FirstOrDefault();
                    }

                    ActualTacticStageList = GetActualListUpToCurrentMonthByStageCode(Tacticdata, timeFrameOption, revStageCodeList, true);
                    if (ActualTacticStageList != null)
                    {
                        TrendActualTacticList = ActualTacticStageList.Where(act => act.StageCode.Equals(revStageCode)).Select(act => act.ActualTacticList).FirstOrDefault();
                    }
                    foreach (RevenueContrinutionData _obj in CustomFieldOptionList)
                    {

                        #region "Calculate Proj.Vs Goal Value"
                        #region "ReInitialize Variables"
                        ActualDataTable = new List<ActualDataTable>();
                        TacticDataTable = new List<TacticDataTable>();
                        ProjectedRevenueTrendList = new List<ProjectedTrendModel>();
                        TacticList = new List<ProjectedTacticModel>();
                        CurrentActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                        fltrTacticData = new List<TacticStageValue>();
                        strRevenueTypeColumn = string.Empty;
                        fltrTrendActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                        TacticListMonth = new List<TacticMonthValue>();
                        #endregion

                        fltrTacticData = Tacticdata.Where(tac => _obj.planTacticList.Contains(tac.TacticObj.PlanTacticId)).ToList();

                        #region "Get Actuals List"
                        CurrentActualTacticList = ActualTacticList.Where(ta => _obj.planTacticList.Contains(ta.PlanTacticId)).ToList();

                        ////// Get Actuals Tactic list by weightage for Revenue.
                        ActualDataTable = GetActualTacticDataTablebyStageCode(customfieldId, _obj.CustomFieldOptionid.ToString(), customFieldType, Enums.InspectStage.Revenue, CurrentActualTacticList, fltrTacticData, IsTacticCustomField);

                        #endregion

                        #region "Get Tactic data by Weightage for Projected by StageCode(Revenue)"
                        TacticDataTable = GetTacticDataTablebyStageCode(customfieldId, _obj.CustomFieldOptionid.ToString(), customFieldType, Enums.InspectStage.Revenue, fltrTacticData, IsTacticCustomField, true);
                        TacticListMonth = GetMonthWiseValueList(TacticDataTable);
                        TacticList = TacticListMonth.Select(tac => new ProjectedTacticModel
                        {
                            TacticId = tac.Id,
                            StartMonth = tac.StartMonth,
                            EndMonth = tac.EndMonth,
                            Value = tac.Value,
                            Year = tac.StartYear
                        }).Distinct().ToList();
                        ProjectedRevenueTrendList = GetProjectedTrendModel(TacticList);
                        ProjectedRevenueTrendList = (from _prjTac in ProjectedRevenueTrendList
                                                     group _prjTac by new
                                                     {
                                                         _prjTac.PlanTacticId,
                                                         _prjTac.Month,
                                                         _prjTac.Value,
                                                         _prjTac.TrendValue
                                                     } into tac
                                                     select new ProjectedTrendModel
                                                     {
                                                         PlanTacticId = tac.Key.PlanTacticId,
                                                         Month = tac.Key.Month,
                                                         Value = tac.Key.Value,
                                                         TrendValue = tac.Key.TrendValue
                                                     }).Distinct().ToList();
                        #endregion

                        #region "Calculate Proj.Vs Goal"
                        Actual_Projected = ActualDataTable.Sum(actual => actual.ActualValue);// +ProjectedRevenueTrendList.Sum(proj => proj.TrendValue);
                        Goal = ProjectedRevenueTrendList.Sum(proj => proj.Value);
                        //Proj_Goal = Actual_Projected > 0 ? ((Actual_Projected - Goal) / Actual_Projected) : 0; // Commneted By Nishant Sheth :#1424
                        Proj_Goal = Goal > 0 ? (((Actual_Projected - Goal) / Goal) * 100) : 0;// Change By Nishant Sheth :#1424 // 
                        TotalRevenueTypeCol = TotalRevenueTypeCol + Proj_Goal;
                        #endregion

                        #endregion

                        #region "Calculate Trend"
                        //fltrTrendActualTacticList = TrendActualTacticList.Where(ta => _obj.planTacticList.Contains(ta.PlanTacticId)).ToList();
                        //ActualDataTable = GetActualTacticDataTablebyStageCode(customfieldId, _obj.CustomFieldOptionid.ToString(), customFieldType, Enums.InspectStage.Revenue, fltrTrendActualTacticList, fltrTacticData, IsTacticCustomField);
                        #endregion

                        #region "Set Sparkline chart Data"
                        _sparklinedata = new sparklineData();
                        _sparklinedata.Name = _obj.Title;
                        strRevenueTypeColumn = Proj_Goal > 0 ? ("+" + Math.Round(Proj_Goal, 1).ToString() + "%") : (Proj_Goal.Equals(0) ? "0%" : Math.Round(Proj_Goal, 1).ToString() + "%");
                        _sparklinedata.RevenueTypeValue = strRevenueTypeColumn;
                        _sparklinedata.IsPositive = Proj_Goal >= 0 ? true : false;
                        _sparklinedata.IsPercentage = true;
                        _sparklinedata.Is_Pos_Neg_Status = true;
                        _sparklinedata.IsTotal = false;
                        _sparklinedata.Value = Proj_Goal;
                        _sparklinedata.Tooltip_Prefix = string.Empty;
                        _sparklinedata.Tooltip_Suffix = strPercentage.ToString();
                        #endregion

                        #region "Calcualte Actual & Projected value Quarterly"
                        if (IsQuarterly)
                        {
                            strActual = strProjected = strTrendValue = string.Empty;
                            ActualQ1 = ActualQ2 = ActualQ3 = ActualQ4 = TrendQ1 = TrendQ2 = TrendQ3 = TrendQ4 = 0;

                            ActualQ1 = ActualDataTable.Where(actual => Q1.Contains(actual.Period)).Sum(actual => actual.ActualValue);
                            ActualQ2 = ActualDataTable.Where(actual => Q1.Contains(actual.Period) || Q2.Contains(actual.Period)).Sum(actual => actual.ActualValue);
                            ActualQ3 = ActualDataTable.Where(actual => Q1.Contains(actual.Period) || Q2.Contains(actual.Period) || Q3.Contains(actual.Period)).Sum(actual => actual.ActualValue);
                            ActualQ4 = ActualDataTable.Where(actual => Q1.Contains(actual.Period) || Q2.Contains(actual.Period) || Q3.Contains(actual.Period) || Q4.Contains(actual.Period)).Sum(actual => actual.ActualValue);

                            TrendQ1 = ProjectedRevenueTrendList.Where(_projTrend => Q1.Contains(_projTrend.Month)).Sum(_projTrend => _projTrend.TrendValue);
                            TrendQ2 = ProjectedRevenueTrendList.Where(_projTrend => Q1.Contains(_projTrend.Month) || Q2.Contains(_projTrend.Month)).Sum(_projTrend => _projTrend.TrendValue);
                            TrendQ3 = ProjectedRevenueTrendList.Where(_projTrend => Q1.Contains(_projTrend.Month) || Q2.Contains(_projTrend.Month) || Q3.Contains(_projTrend.Month)).Sum(_projTrend => _projTrend.TrendValue);
                            TrendQ4 = ProjectedRevenueTrendList.Where(_projTrend => Q1.Contains(_projTrend.Month) || Q2.Contains(_projTrend.Month) || Q3.Contains(_projTrend.Month) || Q4.Contains(_projTrend.Month)).Sum(_projTrend => _projTrend.TrendValue);

                            #region "Newly Added Code"
                            Act_ProjQ1 = ActualQ1;// +TrendQ1;
                            Act_ProjQ2 = ActualQ2;// +TrendQ2;
                            Act_ProjQ3 = ActualQ3;// +TrendQ3;
                            Act_ProjQ4 = ActualQ4;// +TrendQ4;

                            GoalQ1 = ProjectedRevenueTrendList.Where(_proj => Q1.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                            GoalQ2 = ProjectedRevenueTrendList.Where(_proj => Q1.Contains(_proj.Month) || Q2.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                            GoalQ3 = ProjectedRevenueTrendList.Where(_proj => Q1.Contains(_proj.Month) || Q2.Contains(_proj.Month) || Q3.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                            GoalQ4 = ProjectedRevenueTrendList.Where(_proj => Q1.Contains(_proj.Month) || Q2.Contains(_proj.Month) || Q3.Contains(_proj.Month) || Q4.Contains(_proj.Month)).Sum(_proj => _proj.Value);

                            ///Comment By Nishant Sheth For #1424

                            //Proj_GoalQ1 = Act_ProjQ1 > 0 ? ((Act_ProjQ1 - GoalQ1) / Act_ProjQ1) : 0;
                            //Proj_GoalQ2 = Act_ProjQ2 > 0 ? ((Act_ProjQ2 - GoalQ2) / Act_ProjQ2) : 0;
                            //Proj_GoalQ3 = Act_ProjQ3 > 0 ? ((Act_ProjQ3 - GoalQ3) / Act_ProjQ3) : 0;
                            //Proj_GoalQ4 = Act_ProjQ4 > 0 ? ((Act_ProjQ4 - GoalQ4) / Act_ProjQ4) : 0;

                            /// End By Nishant Sheth
                            Proj_GoalQ1 = GoalQ1 > 0 ? (((Act_ProjQ1 - GoalQ1) / GoalQ1) * 100) : 0;// Change By Nishant #1424
                            Proj_GoalQ2 = GoalQ2 > 0 ? (((Act_ProjQ2 - GoalQ2) / GoalQ2) * 100) : 0;// Change By Nishant #1424
                            Proj_GoalQ3 = GoalQ3 > 0 ? (((Act_ProjQ3 - GoalQ3) / GoalQ3) * 100) : 0;// Change By Nishant #1424
                            Proj_GoalQ4 = GoalQ4 > 0 ? (((Act_ProjQ4 - GoalQ4) / GoalQ4) * 100) : 0;// Change By Nishant #1424



                            #endregion

                            //TotalTrendQ1 = TotalTrendQ1 + (ActualQ1 + TrendQ1);
                            //TotalTrendQ2 = TotalTrendQ2 + (ActualQ2 + TrendQ2);
                            //TotalTrendQ3 = TotalTrendQ3 + (ActualQ3 + TrendQ3);
                            //TotalTrendQ4 = TotalTrendQ4 + (ActualQ4 + TrendQ4);
                            //strTrendValue = string.Join(", ", new List<string> { (ActualQ1 + TrendQ1).ToString(), (ActualQ2 + TrendQ2).ToString(), (ActualQ3 + TrendQ3).ToString(), (ActualQ4 + TrendQ4).ToString() });
                            strTrendValue = string.Join(", ", new List<string> { (Proj_GoalQ1).ToString(), (Proj_GoalQ2).ToString(), (Proj_GoalQ3).ToString(), (Proj_GoalQ4).ToString() });
                            _sparklinedata.Trend = strTrendValue;
                        }
                        #endregion

                        lstSparklineData.Add(_sparklinedata);
                    }

                    #region "Calculate Total for Proj.Vs Goal & Trend"
                    List<ProjectedTacticModel> lstTotalTacticModel = new List<ProjectedTacticModel>();
                    TacticListMonth = new List<TacticMonthValue>();
                    TacticListMonth = GetProjectedRevenueDataWithVelocity(Tacticdata);
                    lstTotalTacticModel = TacticListMonth.Select(tac => new ProjectedTacticModel
                    {
                        TacticId = tac.Id,
                        StartMonth = tac.StartMonth,
                        EndMonth = tac.EndMonth,
                        Value = tac.Value,
                        Year = tac.StartYear
                    }).Distinct().ToList();

                    List<ProjectedTrendModel> lstTotalProjectedTrendModel = GetProjectedTrendModel(lstTotalTacticModel);
                    lstTotalProjectedTrendModel = (from _prjTac in lstTotalProjectedTrendModel
                                                   group _prjTac by new
                                                   {
                                                       _prjTac.PlanTacticId,
                                                       _prjTac.Month,
                                                       _prjTac.Value,
                                                       _prjTac.TrendValue
                                                   } into tac
                                                   select new ProjectedTrendModel
                                                   {
                                                       PlanTacticId = tac.Key.PlanTacticId,
                                                       Month = tac.Key.Month,
                                                       Value = tac.Key.Value,
                                                       TrendValue = tac.Key.TrendValue
                                                   }).Distinct().ToList();

                    double TotalActual_Projected = ActualTacticList.Sum(actual => actual.Actualvalue);// +lstTotalProjectedTrendModel.Sum(proj => proj.TrendValue);
                    double TotalGoal = lstTotalProjectedTrendModel.Sum(proj => proj.Value);
                    //double TotalProj_Goal = TotalActual_Projected > 0 ? ((TotalActual_Projected - TotalGoal) / TotalActual_Projected) : 0; // Comment By Nishant Sheth for #1424
                    double TotalProj_Goal = TotalGoal > 0 ? (((TotalActual_Projected - TotalGoal) / TotalGoal) * 100) : 0;// Change By Nishant #1424
                    double _totalTrendQ1 = 0, _totalTrendQ2 = 0, _totalTrendQ3 = 0, _totalTrendQ4 = 0, _totalActualQ1 = 0, _totalActualQ2 = 0, _totalActualQ3 = 0, _totalActualQ4 = 0;
                    Act_ProjQ1 = Act_ProjQ2 = Act_ProjQ3 = Act_ProjQ4 = GoalQ1 = GoalQ2 = GoalQ3 = GoalQ4 = Proj_GoalQ1 = Proj_GoalQ2 = Proj_GoalQ3 = Proj_GoalQ4 = 0;
                    #region "Calculate Trend Quarterly"
                    _totalActualQ1 = ActualTacticList.Where(actual => Q1.Contains(actual.Period)).Sum(actual => actual.Actualvalue);
                    _totalActualQ2 = ActualTacticList.Where(actual => Q1.Contains(actual.Period) || Q2.Contains(actual.Period)).Sum(actual => actual.Actualvalue);
                    _totalActualQ3 = ActualTacticList.Where(actual => Q1.Contains(actual.Period) || Q2.Contains(actual.Period) || Q3.Contains(actual.Period)).Sum(actual => actual.Actualvalue);
                    _totalActualQ4 = ActualTacticList.Where(actual => Q1.Contains(actual.Period) || Q2.Contains(actual.Period) || Q3.Contains(actual.Period) || Q4.Contains(actual.Period)).Sum(actual => actual.Actualvalue);

                    _totalTrendQ1 = lstTotalProjectedTrendModel.Where(_projTrend => Q1.Contains(_projTrend.Month)).Sum(_projTrend => _projTrend.TrendValue);
                    _totalTrendQ2 = lstTotalProjectedTrendModel.Where(_projTrend => Q1.Contains(_projTrend.Month) || Q2.Contains(_projTrend.Month)).Sum(_projTrend => _projTrend.TrendValue);
                    _totalTrendQ3 = lstTotalProjectedTrendModel.Where(_projTrend => Q1.Contains(_projTrend.Month) || Q2.Contains(_projTrend.Month) || Q3.Contains(_projTrend.Month)).Sum(_projTrend => _projTrend.TrendValue);
                    _totalTrendQ4 = lstTotalProjectedTrendModel.Where(_projTrend => Q1.Contains(_projTrend.Month) || Q2.Contains(_projTrend.Month) || Q3.Contains(_projTrend.Month) || Q4.Contains(_projTrend.Month)).Sum(_projTrend => _projTrend.TrendValue);

                    #region "Old Code"
                    //TotalTrendQ1 = (_totalActualQ1 + _totalTrendQ1);
                    //TotalTrendQ2 = (_totalActualQ2 + _totalTrendQ2);
                    //TotalTrendQ3 = (_totalActualQ3 + _totalTrendQ3);
                    //TotalTrendQ4 = (_totalActualQ4 + _totalTrendQ4); 
                    #endregion

                    #region "Newly added Code"
                    Act_ProjQ1 = _totalActualQ1;// +_totalTrendQ1;
                    Act_ProjQ2 = _totalActualQ2;// +_totalTrendQ2;
                    Act_ProjQ3 = _totalActualQ3;// +_totalTrendQ3;
                    Act_ProjQ4 = _totalActualQ4;// +_totalTrendQ4;

                    GoalQ1 = lstTotalProjectedTrendModel.Where(_proj => Q1.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ2 = lstTotalProjectedTrendModel.Where(_proj => Q1.Contains(_proj.Month) || Q2.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ3 = lstTotalProjectedTrendModel.Where(_proj => Q1.Contains(_proj.Month) || Q2.Contains(_proj.Month) || Q3.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ4 = lstTotalProjectedTrendModel.Where(_proj => Q1.Contains(_proj.Month) || Q2.Contains(_proj.Month) || Q3.Contains(_proj.Month) || Q4.Contains(_proj.Month)).Sum(_proj => _proj.Value);

                    /// Commented By Nishant Sheth for #1424

                    //TotalTrendQ1 = Act_ProjQ1 > 0 ? ((Act_ProjQ1 - GoalQ1) / Act_ProjQ1) : 0;
                    //TotalTrendQ2 = Act_ProjQ2 > 0 ? ((Act_ProjQ2 - GoalQ2) / Act_ProjQ2) : 0;
                    //TotalTrendQ3 = Act_ProjQ3 > 0 ? ((Act_ProjQ3 - GoalQ3) / Act_ProjQ3) : 0;
                    //TotalTrendQ4 = Act_ProjQ4 > 0 ? ((Act_ProjQ4 - GoalQ4) / Act_ProjQ4) : 0;

                    /// End By Nishant Sheth

                    TotalTrendQ1 = GoalQ1 > 0 ? (((Act_ProjQ1 - GoalQ1) / GoalQ1) * 100) : 0;// Change By Nishant #1424
                    TotalTrendQ2 = GoalQ2 > 0 ? (((Act_ProjQ2 - GoalQ2) / GoalQ2) * 100) : 0;// Change By Nishant #1424
                    TotalTrendQ3 = GoalQ3 > 0 ? (((Act_ProjQ3 - GoalQ3) / GoalQ3) * 100) : 0;// Change By Nishant #1424
                    TotalTrendQ4 = GoalQ4 > 0 ? (((Act_ProjQ4 - GoalQ4) / GoalQ4) * 100) : 0;// Change By Nishant #1424



                    #endregion

                    #endregion

                    #endregion

                    resultSparklineData = lstSparklineData.OrderByDescending(data => data.Value).Take(5).ToList();

                    #region "Add TOTAL Sparkline record to list"
                    _sparklinedata = new sparklineData();
                    _sparklinedata.Name = "Total";
                    strRevenueTypeColumn = string.Empty;
                    strRevenueTypeColumn = TotalProj_Goal > 0 ? ("+" + Math.Round(TotalProj_Goal, 1).ToString() + "%") : (TotalProj_Goal.Equals(0) ? "0%" : Math.Round(TotalProj_Goal, 1).ToString() + "%");
                    _sparklinedata.RevenueTypeValue = strRevenueTypeColumn;
                    _sparklinedata.IsPositive = TotalProj_Goal >= 0 ? true : false;
                    _sparklinedata.Is_Pos_Neg_Status = true;
                    _sparklinedata.Trend = string.Join(", ", new List<string> { TotalTrendQ1.ToString(), TotalTrendQ2.ToString(), TotalTrendQ3.ToString(), TotalTrendQ4.ToString() });
                    _sparklinedata.IsTotal = true;
                    _sparklinedata.Tooltip_Prefix = string.Empty;
                    _sparklinedata.Tooltip_Suffix = strPercentage.ToString();
                    resultSparklineData.Add(_sparklinedata);
                    #endregion

                    #endregion
                }
                else if (RevenueType.Equals(Enums.TOPRevenueType.Cost))
                {
                    #region "Code for TOPCost"

                    #region "Declare Local Variables"
                    double TotalActualCostCurrentMonth = 0;
                    string costStageCode = Enums.InspectStageValues[Enums.InspectStage.Cost.ToString()].ToString();
                    List<TacticMonthValue> CurrentMonthCostList = new List<TacticMonthValue>();
                    List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItemList = new List<Plan_Campaign_Program_Tactic_LineItem>();
                    List<Plan_Campaign_Program_Tactic_LineItem_Actual> tblLineItemActualList = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                    List<int> TacticIds = new List<int>();
                    List<int> LineItemIds = new List<int>();
                    List<TacticMonthValue> TacticCostData = new List<TacticMonthValue>();
                    List<TacticStageValue> fltrTacticData = new List<TacticStageValue>();
                    #endregion

                    #region "Evaluate Customfield Option wise Sparkline chart data"

                    TacticIds = Tacticdata.Select(tac => tac.TacticObj.PlanTacticId).ToList();
                    tblTacticLineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(line => TacticIds.Contains(line.PlanTacticId) && line.IsDeleted.Equals(false)).ToList();
                    LineItemIds = tblTacticLineItemList.Select(line => line.PlanLineItemId).ToList();
                    tblLineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineActual => LineItemIds.Contains(lineActual.PlanLineItemId)).ToList();
                    foreach (RevenueContrinutionData _obj in CustomFieldOptionList)
                    {
                        TacticCostData = new List<TacticMonthValue>();
                        fltrTacticData = new List<TacticStageValue>();
                        CurrentMonthCostList = new List<TacticMonthValue>();

                        #region "Get ActualCost Data"
                        fltrTacticData = Tacticdata.Where(tac => _obj.planTacticList.Contains(tac.TacticObj.PlanTacticId)).ToList();
                        #region "Get Cost by LineItem"
                        TacticCostData = GetActualCostDataByWeightage(customfieldId, _obj.CustomFieldOptionid.ToString(), customFieldType, fltrTacticData, tblTacticLineItemList, tblLineItemActualList, IsTacticCustomField);
                        CurrentMonthCostList = TacticCostData.Where(actual => IncludeCurrentMonth.Contains(actual.Month)).ToList();
                        TotalActualCostCurrentMonth = CurrentMonthCostList.Sum(tac => tac.Value);
                        TotalRevenueTypeCol = TotalRevenueTypeCol + TotalActualCostCurrentMonth;
                        #endregion

                        #region "Get Actuals Trend Model List"
                        //List<ActualTrendModel> ActualCostTrendModelList = GetActualCostTrendModel(Tacticdata, _obj.planTacticList, CurrentMonthCostList);
                        #endregion
                        #endregion

                        #region "Set Sparkline chart Data"
                        _sparklinedata = new sparklineData();
                        _sparklinedata.Name = _obj.Title;
                        _sparklinedata.RevenueTypeValue = TotalActualCostCurrentMonth.ToString();
                        _sparklinedata.IsPositive = true;
                        _sparklinedata.IsTotal = false;
                        _sparklinedata.Value = TotalActualCostCurrentMonth;
                        _sparklinedata.Tooltip_Prefix = strCurrency.ToString();
                        _sparklinedata.Tooltip_Suffix = string.Empty;
                        #endregion

                        #region "Calcualte Actual & Projected value Quarterly"
                        if (IsQuarterly)
                        {
                            strActual = strProjected = strTrendValue = string.Empty;
                            ActualQ1 = ActualQ2 = ActualQ3 = ActualQ4 = TrendQ1 = TrendQ2 = TrendQ3 = TrendQ4 = 0;

                            ActualQ1 = CurrentMonthCostList.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                            // return record from list which contains Q1 or Q2 months : Summed Up (Q1 + Q2) Actuals Value
                            ActualQ2 = ActualQ1 + CurrentMonthCostList.Where(actual => Q2.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                            // return record from list which contains Q1,Q2 or Q3 months : Summed Up (Q1 + Q2 + Q3) Actuals Value
                            ActualQ3 = ActualQ2 + CurrentMonthCostList.Where(actual => Q3.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                            // return record from list which contains Q1,Q2, Q3 or Q4 months : Summed Up (Q1 + Q2 + Q3 + Q4) Actuals Value
                            ActualQ4 = ActualQ3 + CurrentMonthCostList.Where(actual => Q4.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);

                            #region "Old Code"
                            //TrendQ1 = ActualCostTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q1.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //TrendQ2 = ActualCostTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q2.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //TrendQ3 = ActualCostTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q3.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //TrendQ4 = ActualCostTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q4.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //TotalTrendQ1 = TotalTrendQ1 + (ActualQ1 + TrendQ1);
                            //TotalTrendQ2 = TotalTrendQ2 + (ActualQ2 + TrendQ2);
                            //TotalTrendQ3 = TotalTrendQ3 + (ActualQ3 + TrendQ3);
                            //TotalTrendQ4 = TotalTrendQ4 + (ActualQ4 + TrendQ4); 
                            #endregion

                            TotalTrendQ1 = TotalTrendQ1 + (ActualQ1);
                            TotalTrendQ2 = TotalTrendQ2 + (ActualQ2);
                            TotalTrendQ3 = TotalTrendQ3 + (ActualQ3);
                            TotalTrendQ4 = TotalTrendQ4 + (ActualQ4);

                            //strTrendValue = string.Join(", ", new List<string> { (ActualQ1 + TrendQ1).ToString(), (ActualQ2 + TrendQ2).ToString(), (ActualQ3 + TrendQ3).ToString(), (ActualQ4 + TrendQ4).ToString() });
                            strTrendValue = string.Join(", ", new List<string> { (ActualQ1).ToString(), (ActualQ2).ToString(), (ActualQ3).ToString(), (ActualQ4).ToString() });
                            _sparklinedata.Trend = strTrendValue;
                        }
                        #endregion

                        lstSparklineData.Add(_sparklinedata);
                    }
                    resultSparklineData = lstSparklineData.OrderByDescending(data => data.Value).Take(5).ToList();
                    #region "Add TOTAL Sparkline record to list"
                    _sparklinedata = new sparklineData();
                    _sparklinedata.Name = "Total";
                    _sparklinedata.RevenueTypeValue = TotalRevenueTypeCol.ToString();
                    _sparklinedata.IsPositive = true;
                    _sparklinedata.Trend = string.Join(", ", new List<string> { TotalTrendQ1.ToString(), TotalTrendQ2.ToString(), TotalTrendQ3.ToString(), TotalTrendQ4.ToString() });
                    _sparklinedata.IsTotal = true;
                    _sparklinedata.Tooltip_Prefix = strCurrency.ToString();
                    _sparklinedata.Tooltip_Suffix = string.Empty;
                    resultSparklineData.Add(_sparklinedata);
                    #endregion

                    #endregion
                    #endregion
                }
                else if (RevenueType.Equals(Enums.TOPRevenueType.ROI))
                {
                    #region "Declare Local Variables"
                    double TotalRevenueValueCurrentMonth = 0, TotalCostValueCurrentMonth = 0, TotalROIValueCurrentMonth = 0;
                    List<Plan_Campaign_Program_Tactic_Actual> revFltrActuals = new List<Plan_Campaign_Program_Tactic_Actual>();
                    string revenueStageCode = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
                    List<ActualDataTable> ActualDataTable = new List<ActualDataTable>();
                    List<ActualDataTable> revCurrentMonthList = new List<ActualDataTable>();
                    List<Plan_Campaign_Program_Tactic_Actual> revActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                    List<TacticStageValue> fltrTacticData = new List<TacticStageValue>();
                    List<TacticMonthValue> CurrentMonthCostList = new List<TacticMonthValue>();
                    List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItemList = new List<Plan_Campaign_Program_Tactic_LineItem>();
                    List<Plan_Campaign_Program_Tactic_LineItem_Actual> tblLineItemActualList = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                    List<int> LineItemIds = new List<int>();
                    List<int> TacticIds = new List<int>();
                    List<TacticMonthValue> TacticCostData = new List<TacticMonthValue>();
                    List<ActualTrendModel> ActualCostTrendModelList = new List<ActualTrendModel>();
                    List<ActualTrendModel> ActualTrendModelList = new List<ActualTrendModel>();
                    double revActualQ1 = 0, revActualQ2 = 0, revActualQ3 = 0, revActualQ4 = 0, costActualQ1 = 0, costActualQ2 = 0, costActualQ3 = 0, costActualQ4 = 0, revTrendQ1 = 0, revTrendQ2 = 0, revTrendQ3 = 0, revTrendQ4 = 0, costTrendQ1 = 0, costTrendQ2 = 0, costTrendQ3 = 0, costTrendQ4 = 0;
                    string strRevenueTypeColumn = string.Empty;
                    #endregion

                    #region "Evaluate Customfield Option wise Sparkline chart data"

                    ActualTacticStageList = GetActualListUpToCurrentMonthByStageCode(Tacticdata, timeFrameOption, revStageCodeList, false);
                    if (ActualTacticStageList != null)
                    {
                        revActualTacticList = ActualTacticStageList.Where(act => act.StageCode.Equals(revStageCode)).Select(act => act.ActualTacticList).FirstOrDefault();
                    }

                    #region "LineItems list for Cost Calculation"
                    TacticIds = Tacticdata.Select(tac => tac.TacticObj.PlanTacticId).ToList();
                    tblTacticLineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(line => TacticIds.Contains(line.PlanTacticId) && line.IsDeleted.Equals(false)).ToList();
                    LineItemIds = tblTacticLineItemList.Select(line => line.PlanLineItemId).ToList();
                    tblLineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineActual => LineItemIds.Contains(lineActual.PlanLineItemId)).ToList();
                    #endregion
                    foreach (RevenueContrinutionData _obj in CustomFieldOptionList)
                    {
                        ActualCostTrendModelList = new List<ActualTrendModel>();
                        ActualTrendModelList = new List<ActualTrendModel>();

                        #region "Get Revenue Actuals List"
                        revFltrActuals = revActualTacticList.Where(ta => _obj.planTacticList.Contains(ta.PlanTacticId)).ToList();
                        //// Get Actuals Tactic list by weightage for Revenue.
                        ActualDataTable = GetActualTacticDataTablebyStageCode(customfieldId, _obj.CustomFieldOptionid.ToString(), customFieldType, Enums.InspectStage.Revenue, revFltrActuals, Tacticdata, IsTacticCustomField);

                        //// Get ActualList upto CurrentMonth.
                        revCurrentMonthList = ActualDataTable.Where(actual => IncludeCurrentMonth.Contains(Tacticdata.Where(tac => tac.TacticObj.PlanTacticId.Equals(actual.PlanTacticId)).FirstOrDefault().TacticYear + actual.Period)).ToList();
                        TotalRevenueValueCurrentMonth = revCurrentMonthList.Sum(ta => ta.ActualValue); // Get Total of Actual Revenue value. 
                        //TotalRevenueTypeCol = TotalRevenueTypeCol + TotalRevenueValueCurrentMonth;

                        //ActualTrendModelList = GetActualTrendModel(Tacticdata, _obj.planTacticList, revCurrentMonthList);
                        #endregion

                        #region " Get Cost Actuals List "

                        fltrTacticData = Tacticdata.Where(tac => _obj.planTacticList.Contains(tac.TacticObj.PlanTacticId)).ToList();

                        #region "Get Cost by LineItem"
                        TacticCostData = GetActualCostDataByWeightage(customfieldId, _obj.CustomFieldOptionid.ToString(), customFieldType, fltrTacticData, tblTacticLineItemList, tblLineItemActualList, IsTacticCustomField);
                        CurrentMonthCostList = TacticCostData.Where(actual => IncludeCurrentMonth.Contains(actual.Month)).ToList();
                        TotalCostValueCurrentMonth = CurrentMonthCostList.Sum(tac => tac.Value);
                        //TotalRevenueTypeCol = TotalRevenueTypeCol + TotalCostValueCurrentMonth;
                        #endregion

                        #region "Get Actuals Cost Trend Model List"
                        //ActualCostTrendModelList = GetActualCostTrendModel(Tacticdata, _obj.planTacticList, CurrentMonthCostList);
                        #endregion

                        #endregion

                        #region "Calculate ROI"
                        if (TotalCostValueCurrentMonth != 0)
                        {
                            //TotalROIValueCurrentMonth = (TotalRevenueValueCurrentMonth - TotalCostValueCurrentMonth) / TotalCostValueCurrentMonth; // Commented BY Nishant Sheth : #1423
                            TotalROIValueCurrentMonth = (((TotalRevenueValueCurrentMonth - TotalCostValueCurrentMonth) / TotalCostValueCurrentMonth) * 100); // Add By Nishant Sheth : #1423
                        }
                        else
                        {
                            TotalROIValueCurrentMonth = 0;
                        }

                        TotalRevenueTypeCol = TotalRevenueTypeCol + TotalROIValueCurrentMonth;
                        #endregion

                        #region "Set Sparkline chart Data"
                        _sparklinedata = new sparklineData();
                        _sparklinedata.Name = _obj.Title;
                        //strRevenueTypeColumn = Math.Round(TotalROIValueCurrentMonth, 1).ToString(); // Commented By Nishant Sheth : #1423
                        strRevenueTypeColumn = TotalROIValueCurrentMonth > 0 ? ("+" + Math.Round(TotalROIValueCurrentMonth, 1).ToString() + "%") : (TotalROIValueCurrentMonth.Equals(0) ? "0%" : Math.Round(TotalROIValueCurrentMonth, 1).ToString() + "%");// Change By Nishant Sheth #1423 (Display in %)
                        _sparklinedata.RevenueTypeValue = strRevenueTypeColumn;
                        _sparklinedata.IsPercentage = true; // Add BY Nishant Sheth : #1423
                        _sparklinedata.IsPositive = TotalROIValueCurrentMonth >= 0 ? true : false;
                        _sparklinedata.Is_Pos_Neg_Status = true;
                        _sparklinedata.IsTotal = false;
                        _sparklinedata.Value = TotalROIValueCurrentMonth;
                        //_sparklinedata.Tooltip_Prefix = strCurrency.ToString();
                        _sparklinedata.Tooltip_Prefix = _sparklinedata.Tooltip_Suffix = string.Empty;
                        _sparklinedata.Tooltip_Suffix = strPercentage.ToString();// Add BY Nishant Sheth : #1423
                        #endregion

                        #region "Calcualte Actual & Projected value Quarterly"
                        if (IsQuarterly)
                        {
                            strActual = strProjected = strTrendValue = string.Empty;
                            revActualQ1 = revActualQ2 = revActualQ3 = revActualQ4 = costActualQ1 = costActualQ2 = costActualQ3 = costActualQ4 = revTrendQ1 = revTrendQ2 = revTrendQ3 = revTrendQ4 = costTrendQ1 = costTrendQ2 = costTrendQ3 = costTrendQ4 = 0;
                            ActualQ1 = ActualQ2 = ActualQ3 = ActualQ4 = TrendQ1 = TrendQ2 = TrendQ3 = TrendQ4 = 0;

                            //// Get Actual Revenue value upto currentmonth by Quarterly.
                            revActualQ1 = revCurrentMonthList.Where(actual => Q1.Contains(actual.Period)).Sum(actual => actual.ActualValue);
                            revActualQ2 = revCurrentMonthList.Where(actual => Q1.Contains(actual.Period) || Q2.Contains(actual.Period)).Sum(actual => actual.ActualValue);
                            revActualQ3 = revCurrentMonthList.Where(actual => Q1.Contains(actual.Period) || Q2.Contains(actual.Period) || Q3.Contains(actual.Period)).Sum(actual => actual.ActualValue);
                            revActualQ4 = revCurrentMonthList.Where(actual => Q1.Contains(actual.Period) || Q2.Contains(actual.Period) || Q3.Contains(actual.Period) || Q4.Contains(actual.Period)).Sum(actual => actual.ActualValue);

                            //// Get Actual Cost value upto currentmonth by Quarterly.
                            costActualQ1 = CurrentMonthCostList.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                            costActualQ2 = CurrentMonthCostList.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty) || Q2.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                            costActualQ3 = CurrentMonthCostList.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty) || Q2.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty) || Q3.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                            costActualQ4 = CurrentMonthCostList.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty) || Q2.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty) || Q3.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty) || Q4.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);

                            #region "Old Code"
                            ////// Calculate Trend for Actual: Revenue.
                            //revTrendQ1 = ActualTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q1.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //revTrendQ2 = ActualTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q2.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //revTrendQ3 = ActualTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q3.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //revTrendQ4 = ActualTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q4.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);

                            ////// Calculate Trend for Actual: Cost.
                            //costTrendQ1 = ActualCostTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q1.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //costTrendQ2 = ActualCostTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q2.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //costTrendQ3 = ActualCostTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q3.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                            //costTrendQ4 = ActualCostTrendModelList.Where(_actTrend => _obj.planTacticList.Contains(_actTrend.PlanTacticId) && Q4.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);

                            ////// Calculate ROI Trend
                            //TrendQ1 = (costActualQ1 + costTrendQ1) != 0 ? (((revActualQ1 + revTrendQ1) - (costActualQ1 + costTrendQ1)) / (costActualQ1 + costTrendQ1)) : 0;
                            //TrendQ2 = (costActualQ2 + costTrendQ2) != 0 ? (((revActualQ2 + revTrendQ2) - (costActualQ2 + costTrendQ2)) / (costActualQ2 + costTrendQ2)) : 0;
                            //TrendQ3 = (costActualQ3 + costTrendQ3) != 0 ? (((revActualQ3 + revTrendQ3) - (costActualQ3 + costTrendQ3)) / (costActualQ3 + costTrendQ3)) : 0;
                            //TrendQ4 = (costActualQ4 + costTrendQ4) != 0 ? (((revActualQ4 + revTrendQ4) - (costActualQ4 + costTrendQ4)) / (costActualQ4 + costTrendQ4)) : 0; 
                            #endregion

                            /// Commented By Nishant Sheth for #1423

                            //TrendQ1 = (costActualQ1) != 0 ? (((revActualQ1) - (costActualQ1)) / (costActualQ1)) : 0;
                            //TrendQ2 = (costActualQ2) != 0 ? (((revActualQ2) - (costActualQ2)) / (costActualQ2)) : 0;
                            //TrendQ3 = (costActualQ3) != 0 ? (((revActualQ3) - (costActualQ3)) / (costActualQ3)) : 0;
                            //TrendQ4 = (costActualQ4) != 0 ? (((revActualQ4) - (costActualQ4)) / (costActualQ4)) : 0;

                            /// End By Nishant Sheth 









                            TrendQ1 = (costActualQ1) != 0 ? ((((revActualQ1) - (costActualQ1)) / (costActualQ1)) * 100) : 0; // Change By Nishant #1423
                            TrendQ2 = (costActualQ2) != 0 ? ((((revActualQ2) - (costActualQ2)) / (costActualQ2)) * 100) : 0; // Change By Nishant #1423
                            TrendQ3 = (costActualQ3) != 0 ? ((((revActualQ3) - (costActualQ3)) / (costActualQ3)) * 100) : 0; // Change By Nishant #1423
                            TrendQ4 = (costActualQ4) != 0 ? ((((revActualQ4) - (costActualQ4)) / (costActualQ4)) * 100) : 0; // Change By Nishant #1423
                            strTrendValue = string.Join(", ", new List<string> { (TrendQ1).ToString(), (TrendQ2).ToString(), (TrendQ3).ToString(), (TrendQ4).ToString() });
                            _sparklinedata.Trend = strTrendValue;
                        }
                        #endregion

                        lstSparklineData.Add(_sparklinedata);
                    }

                    #region "Calculate Total for Proj.Vs Goal & Trend"
                    #region "Calculate Revenue Actuals List"
                    ActualCostTrendModelList = new List<ActualTrendModel>();
                    ActualTrendModelList = new List<ActualTrendModel>();
                    ActualDataTable = new List<Models.ActualDataTable>();
                    revFltrActuals = new List<Plan_Campaign_Program_Tactic_Actual>();
                    revFltrActuals = revActualTacticList.Where(actual => IncludeCurrentMonth.Contains(Tacticdata.Where(tac => tac.TacticObj.PlanTacticId.Equals(actual.PlanTacticId)).FirstOrDefault().TacticYear + actual.Period)).ToList();
                    ActualDataTable = GetActualTacticDataTable(revFltrActuals);
                    TotalRevenueValueCurrentMonth = ActualDataTable.Sum(actual => actual.ActualValue);
                    TacticIds = new List<int>();
                    TacticIds = Tacticdata.Select(tac => tac.TacticObj.PlanTacticId).ToList();
                    //ActualTrendModelList = GetActualTrendModel(Tacticdata, TacticIds, ActualDataTable);
                    #endregion

                    #region " Get Cost Actuals List "
                    TacticCostData = new List<TacticMonthValue>();
                    TacticCostData = GetActualCostData(Tacticdata, tblTacticLineItemList, tblLineItemActualList);
                    TacticCostData = TacticCostData.Where(actual => IncludeCurrentMonth.Contains(actual.Month)).ToList();
                    TotalCostValueCurrentMonth = TacticCostData.Sum(tac => tac.Value);
                    #endregion

                    #region "Get Actuals Cost Trend Model List"
                    //ActualCostTrendModelList = GetActualCostTrendModel(Tacticdata, TacticIds, TacticCostData);
                    #endregion

                    #region "Calculate ROI"
                    if (TotalCostValueCurrentMonth != 0)
                    {
                        //TotalRevenueTypeCol = (TotalRevenueValueCurrentMonth - TotalCostValueCurrentMonth) / TotalCostValueCurrentMonth; // Commented BY Nishant Sheth : #1423
                        TotalRevenueTypeCol = (((TotalRevenueValueCurrentMonth - TotalCostValueCurrentMonth) / TotalCostValueCurrentMonth) * 100); // Add By Nishant Sheth : #1423
                    }
                    else
                    {
                        TotalRevenueTypeCol = 0;
                    }
                    #endregion

                    #region "Calcualte Actual & Projected value Quarterly"
                    if (IsQuarterly)
                    {
                        strActual = strProjected = strTrendValue = string.Empty;
                        revActualQ1 = revActualQ2 = revActualQ3 = revActualQ4 = costActualQ1 = costActualQ2 = costActualQ3 = costActualQ4 = revTrendQ1 = revTrendQ2 = revTrendQ3 = revTrendQ4 = costTrendQ1 = costTrendQ2 = costTrendQ3 = costTrendQ4 = 0;
                        ActualQ1 = ActualQ2 = ActualQ3 = ActualQ4 = TrendQ1 = TrendQ2 = TrendQ3 = TrendQ4 = 0;

                        //// Get Actual Revenue value upto currentmonth by Quarterly.
                        revActualQ1 = ActualDataTable.Where(actual => Q1.Contains(actual.Period)).Sum(actual => actual.ActualValue);
                        revActualQ2 = ActualDataTable.Where(actual => Q1.Contains(actual.Period) || Q2.Contains(actual.Period)).Sum(actual => actual.ActualValue);
                        revActualQ3 = ActualDataTable.Where(actual => Q1.Contains(actual.Period) || Q2.Contains(actual.Period) || Q3.Contains(actual.Period)).Sum(actual => actual.ActualValue);
                        revActualQ4 = ActualDataTable.Where(actual => Q1.Contains(actual.Period) || Q2.Contains(actual.Period) || Q3.Contains(actual.Period) || Q4.Contains(actual.Period)).Sum(actual => actual.ActualValue);

                        //// Get Actual Cost value upto currentmonth by Quarterly.
                        costActualQ1 = TacticCostData.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                        costActualQ2 = TacticCostData.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty) || Q2.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                        costActualQ3 = TacticCostData.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty) || Q2.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty) || Q3.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                        costActualQ4 = TacticCostData.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty) || Q2.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty) || Q3.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty) || Q4.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);

                        #region " Old Code "
                        ////// Calculate Trend for Actual: Revenue.
                        //revTrendQ1 = ActualTrendModelList.Where(_actTrend => Q1.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                        //revTrendQ2 = ActualTrendModelList.Where(_actTrend => Q2.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                        //revTrendQ3 = ActualTrendModelList.Where(_actTrend => Q3.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                        //revTrendQ4 = ActualTrendModelList.Where(_actTrend => Q4.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);

                        ////// Calculate Trend for Actual: Cost.
                        //costTrendQ1 = ActualCostTrendModelList.Where(_actTrend => Q1.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                        //costTrendQ2 = ActualCostTrendModelList.Where(_actTrend => Q2.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                        //costTrendQ3 = ActualCostTrendModelList.Where(_actTrend => Q3.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);
                        //costTrendQ4 = ActualCostTrendModelList.Where(_actTrend => Q4.Contains(_actTrend.Month)).Sum(_actTrend => _actTrend.TrendValue);

                        ////// Calculate ROI Trend
                        //TrendQ1 = (costActualQ1 + costTrendQ1) != 0 ? (((revActualQ1 + revTrendQ1) - (costActualQ1 + costTrendQ1)) / (costActualQ1 + costTrendQ1)) : 0;
                        //TrendQ2 = (costActualQ2 + costTrendQ2) != 0 ? (((revActualQ2 + revTrendQ2) - (costActualQ2 + costTrendQ2)) / (costActualQ2 + costTrendQ2)) : 0;
                        //TrendQ3 = (costActualQ3 + costTrendQ3) != 0 ? (((revActualQ3 + revTrendQ3) - (costActualQ3 + costTrendQ3)) / (costActualQ3 + costTrendQ3)) : 0;
                        //TrendQ4 = (costActualQ4 + costTrendQ4) != 0 ? (((revActualQ4 + revTrendQ4) - (costActualQ4 + costTrendQ4)) / (costActualQ4 + costTrendQ4)) : 0; 
                        #endregion

                        /// Commented By Nishant Sheth for #1423

                        //TotalTrendQ1 = (costActualQ1) != 0 ? (((revActualQ1) - (costActualQ1)) / (costActualQ1)) : 0;
                        //TotalTrendQ2 = (costActualQ2) != 0 ? (((revActualQ2) - (costActualQ2)) / (costActualQ2)) : 0;
                        //TotalTrendQ3 = (costActualQ3) != 0 ? (((revActualQ3) - (costActualQ3)) / (costActualQ3)) : 0;
                        //TotalTrendQ4 = (costActualQ4) != 0 ? (((revActualQ4) - (costActualQ4)) / (costActualQ4)) : 0;

                        /// End By Nishant Sheth
                        TotalTrendQ1 = (costActualQ1) != 0 ? ((((revActualQ1) - (costActualQ1)) / (costActualQ1)) * 100) : 0; // Change By Nishant #1423
                        TotalTrendQ2 = (costActualQ2) != 0 ? ((((revActualQ2) - (costActualQ2)) / (costActualQ2)) * 100) : 0; // Change By Nishant #1423
                        TotalTrendQ3 = (costActualQ3) != 0 ? ((((revActualQ3) - (costActualQ3)) / (costActualQ3)) * 100) : 0; // Change By Nishant #1423
                        TotalTrendQ4 = (costActualQ4) != 0 ? ((((revActualQ4) - (costActualQ4)) / (costActualQ4)) * 100) : 0; // Change By Nishant #1423

                        //TotalTrendQ1 = TrendQ1;//(ActualQ1 + TrendQ1);
                        //TotalTrendQ2 = TrendQ2;//(ActualQ2 + TrendQ2);
                        //TotalTrendQ3 = TrendQ3;//(ActualQ3 + TrendQ3);
                        //TotalTrendQ4 = TrendQ4;//(ActualQ4 + TrendQ4);

                    }
                    #endregion

                    #endregion

                    resultSparklineData = lstSparklineData.OrderByDescending(data => data.Value).Take(5).ToList();

                    #region "Add TOTAL Sparkline record to list"
                    _sparklinedata = new sparklineData();
                    _sparklinedata.Name = "Total";
                    strRevenueTypeColumn = string.Empty;
                    //strRevenueTypeColumn = Math.Round(TotalRevenueTypeCol, 1).ToString(); // Commented BY Nishant Sheth : #1423
                    strRevenueTypeColumn = TotalRevenueTypeCol > 0 ? ("+" + Math.Round(TotalRevenueTypeCol, 1).ToString() + "%") : (TotalRevenueTypeCol.Equals(0) ? "0%" : Math.Round(TotalRevenueTypeCol, 1).ToString() + "%"); // Change By Nishant Sheth #1423 (Display in %)
                    _sparklinedata.RevenueTypeValue = strRevenueTypeColumn;
                    _sparklinedata.IsPercentage = true; // Add BY Nishant Sheth : #1423
                    _sparklinedata.IsPositive = TotalRevenueTypeCol >= 0 ? true : false;
                    _sparklinedata.Is_Pos_Neg_Status = true;
                    _sparklinedata.Trend = string.Join(", ", new List<string> { TotalTrendQ1.ToString(), TotalTrendQ2.ToString(), TotalTrendQ3.ToString(), TotalTrendQ4.ToString() });
                    _sparklinedata.IsTotal = true;
                    _sparklinedata.Tooltip_Prefix = _sparklinedata.Tooltip_Suffix = string.Empty;
                    _sparklinedata.Tooltip_Suffix = strPercentage.ToString();// Add BY Nishant Sheth : #1423
                    resultSparklineData.Add(_sparklinedata);
                    #endregion

                    #endregion

                }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resultSparklineData;
        }

        /// <summary>
        /// This function load respective sparklinechart partialview based on selected Customfield.
        /// </summary>
        /// <param name="strCustomField"> Selected CustomField value from Dropdownlist</param>
        /// <param name="timeFrameOption">Selected timeframeOption from left filters</param>
        /// <param name="strRevenueType"> This is Enum RevenueType like Revenue, Performance, Cost, ROI for that this function return data of Sparklinechart</param>
        /// <returns>Return Partialview of Sparklinechart</returns>
        public PartialViewResult LoadSparkLineChartPartial(string strCutomfield, string timeFrameOption, string strRevenueType)
        {
            List<sparkLineCharts> SparkLinechartsModel = new List<sparkLineCharts>();
            Enums.TOPRevenueType result = new Enums.TOPRevenueType();
            sparkLineCharts objSparkLineChart = new sparkLineCharts();
            try
            {
                List<Enums.TOPRevenueType> RevenueTypeList = new List<Enums.TOPRevenueType>();
                result = (Enums.TOPRevenueType)Enum.Parse(typeof(Enums.TOPRevenueType), strRevenueType);
                RevenueTypeList.Add(result);
                SparkLinechartsModel = GetRevenueSparkLineChartData(strCutomfield, timeFrameOption, RevenueTypeList);
                if (SparkLinechartsModel != null && SparkLinechartsModel.Count > 0)
                    objSparkLineChart = SparkLinechartsModel.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return PartialView("_SparkLineChartTable", objSparkLineChart);
        }
        #endregion

        #region "Conversion report related functions"

        public List<Stage_Benchmark> GetStagewiseBenchmark(List<TacticStageValue> Tacticdata, List<string> StageCodeList)
        {
            List<Plan_Campaign_Program_Tactic> TacticList = new List<Plan_Campaign_Program_Tactic>();
            List<TacticPlanRelation> tacticPlanList = new List<TacticPlanRelation>();
            List<TacticModelRelation> tacticModelList = new List<TacticModelRelation>();
            List<int> ModelList = new List<int>();
            List<int> _StageWiselist = new List<int>();
            List<Model_Stage> _fltrModel_Stage = new List<Model_Stage>();
            List<Stage_Benchmark> StageBenchmarkList = new List<Stage_Benchmark>();
            double _benchmark = 0, _mdlBenchmark = 0;
            int _curntTacticCount = 0, _TotalTacticCount = 0;
            try
            {
                if (Tacticdata != null && Tacticdata.Count > 0)
                {
                    string CR = Enums.StageType.CR.ToString();
                    string stageINQ = Enums.Stage.INQ.ToString();
                    List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted.Equals(false)).Select(stage => stage).ToList();

                    int levelINQ = stageList.Where(s => s.Code.Equals(stageINQ)).Select(s => s.Level.Value).FirstOrDefault();
                    string stageMQL = Enums.Stage.MQL.ToString();
                    int levelMQL = stageList.Where(s => s.Code.Equals(stageMQL)).Select(s => s.Level.Value).FirstOrDefault();
                    string stageCW = Enums.Stage.CW.ToString();
                    int levelCW = stageList.Where(s => s.Code.Equals(stageCW)).Select(s => s.Level.Value).FirstOrDefault();
                    Stage_Benchmark StageBenchmark = null;


                    TacticList = Tacticdata.Select(tac => tac.TacticObj).ToList();
                    tacticPlanList = Common.GetTacticPlanRelationList(TacticList);
                    tacticModelList = Common.GetTacticModelRelationList(TacticList, tacticPlanList);
                    ModelList = tacticModelList.Select(tacModel => tacModel.ModelId).Distinct().ToList();
                    //_StageWiselist = stageList.Where(s => s.Level >= levelINQ && s.Level < levelMQL).Select(s => s.StageId).ToList();
                    _StageWiselist = stageList.Where(s => s.Level >= levelMQL && s.Level < levelCW).Select(s => s.StageId).ToList();
                    List<Model_Stage> tblModelStage = db.Model_Stage.Where(_mdlStg => _mdlStg.StageType == CR && ModelList.Contains(_mdlStg.ModelId)).Select(stage => stage).ToList();

                    foreach (string _stagecode in StageCodeList)
                    {
                        StageBenchmark = new Stage_Benchmark();
                        _TotalTacticCount = _curntTacticCount = 0;
                        _mdlBenchmark = _benchmark = 0;
                        StageBenchmark.StageCode = _stagecode;
                        foreach (int _mdlId in ModelList)
                        {

                            _fltrModel_Stage = new List<Model_Stage>();
                            if (_stagecode.Equals(stageMQL))
                            {
                                _StageWiselist = stageList.Where(s => s.Level >= levelINQ && s.Level < levelMQL).Select(s => s.StageId).ToList();
                            }
                            else if (_stagecode.Equals(stageCW))
                            {
                                _StageWiselist = stageList.Where(s => s.Level >= levelMQL && s.Level < levelCW).Select(s => s.StageId).ToList();
                            }
                            _fltrModel_Stage = tblModelStage.Where(_mdlStg => _StageWiselist.Contains(_mdlStg.StageId)).ToList();
                            _mdlBenchmark = _fltrModel_Stage.Where(_mdlStg => _mdlStg.ModelId.Equals(_mdlId)).Select(_mdlStg => _mdlStg.Value).Aggregate(1.0, (a, b) => a * (b / 100));
                            _mdlBenchmark = 100 * _mdlBenchmark;
                            _curntTacticCount = tacticModelList.Where(mdl => mdl.ModelId.Equals(_mdlId)).Count();
                            _TotalTacticCount = _TotalTacticCount + _curntTacticCount;
                            _benchmark = _benchmark + (_mdlBenchmark * _curntTacticCount);
                        }
                        _benchmark = _TotalTacticCount > 0 ? (_benchmark / _TotalTacticCount) : 0;
                        StageBenchmark.Benchmark = _benchmark;
                        StageBenchmarkList.Add(StageBenchmark);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return StageBenchmarkList;
        }

        #endregion

        #region "Common Methods"

        /// <summary>
        /// This function will return list of ActualTactic
        /// </summary>
        /// <param name="Tacticdata"> List of Tactic</param>
        /// <param name="timeframeOption">Selected Year from left YearFilter dropdown</param>
        /// <param name="includeMonth"> list of include month to filter TacticData & ActualTactic list</param>
        /// <returns>Return List of ActualTacticList</returns>
        public List<ActualTacticListByStage> GetActualListInTacticInterval(List<TacticStageValue> TacticData, string timeframeOption, List<string> StageCodeList, bool IsTillCurrentMonth)
        {
            #region "Declare local Variables"
            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            List<TacticMonthInterval> lstTacticMonths = new List<TacticMonthInterval>();
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            #endregion
            try
            {
                lstTacticMonths = GetTacticMonthInterval(TacticData);
                //bool IsTillCurrentMonth = false;
                ActualTacticStageList = GetActualListUpToCurrentMonthByStageCode(TacticData, timeframeOption, StageCodeList, IsTillCurrentMonth);

                //// Filter ActualTacticList by Total month included in Tactic.
                foreach (ActualTacticListByStage objActualStage in ActualTacticStageList)
                {
                    if (objActualStage.ActualTacticList != null)
                    {
                        //objActualStage.ActualTacticList = objActualStage.ActualTacticList.Where(actual => lstTacticMonths.Where(tac => tac.PlanTacticId.Equals(actual.PlanTacticId)).Select(tac => tac.Month).Contains((TacticData.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == actual.PlanTacticId).TacticYear) + actual.Period)).ToList();
                        //objActualStage.ActualTacticList = objActualStage.ActualTacticList.Where(actual => lstTacticMonths.Where(tac => tac.PlanTacticId.Equals(actual.PlanTacticId)).Select(tac => tac.Month).Contains((TacticData.FirstOrDefault(tactic => tactic.TacticObj.PlanTacticId == actual.PlanTacticId).TacticYear) + actual.Period)).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ActualTacticStageList;
        }

        /// <summary>
        /// This function will return the data of Spark Line chart based on RevenueType
        /// </summary>
        /// <param name="Tacticdata"> List of Tactic</param>
        /// <param name="includeMonth"> List of Inclueded Months to filter Tactic & ActualTactic list</param>
        /// <returns>Return List of ProjectedTrendModel</returns>
        public List<ProjectedTrendModel> CalculateProjectedTrend(List<TacticStageValue> TacticData, List<string> includeMonth, string StageCode)
        {
            List<ProjectedTrendModel> ProjectedTrendModelList = new List<ProjectedTrendModel>();
            List<TacticMonthValue> ProjectedTacticList = new List<TacticMonthValue>();
            List<ProjectedTacticModel> lstTactic = new List<ProjectedTacticModel>();
            try
            {
                if (StageCode.Equals(Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString()))
                {
                    // Get TacticDataTable list of Projected Revenue.
                    ProjectedTacticList = GetProjectedRevenueDataWithVelocity(TacticData).Where(mr => includeMonth.Contains(mr.Month)).ToList();
                }
                else if (StageCode.Equals(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()))
                {
                    // Get TacticDataTable list of Projected MQL.
                    ProjectedTacticList = GetProjectedMQLDataWithVelocity(TacticData).Where(mr => includeMonth.Contains(mr.Month)).ToList();
                }
                else if (StageCode.Equals(Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString()))
                {
                    // Get TacticDataTable list of Projected CW.
                    ProjectedTacticList = GetProjectedCWDataWithVelocity(TacticData).Where(mr => includeMonth.Contains(mr.Month)).ToList();
                }
                else if (StageCode.Equals(Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString()))
                {
                    // Get TacticDataTable list of Projected INQ.
                    ProjectedTacticList = GetProjectedINQDataWithVelocity(TacticData).Where(mr => includeMonth.Contains(mr.Month)).ToList();
                }

                // Create ProjectedTacticModel from ProjectedRevenueTacticList to get ProjecteRevenueTrend model list.
                lstTactic = (from _prjTac in ProjectedTacticList
                             group _prjTac by new
                             {
                                 _prjTac.Id,
                                 _prjTac.StartMonth,
                                 _prjTac.EndMonth,
                                 _prjTac.Value,
                                 _prjTac.StartYear,
                             } into tac
                             select new ProjectedTacticModel
                             {
                                 TacticId = tac.Key.Id,
                                 StartMonth = tac.Key.StartMonth,
                                 EndMonth = tac.Key.EndMonth,
                                 Value = tac.Key.Value,
                                 Year = tac.Key.StartYear
                             }).Distinct().ToList();

                // Get Projected Revenue Trend List.
                ProjectedTrendModelList = GetProjectedTrendModel(lstTactic);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ProjectedTrendModelList;
        }

        /// <summary>
        /// This function will return Tactic wise the Model of RevenueOverview
        /// </summary>
        /// <param name="ActualTacticList"> List of Actuals Tactic</param>
        /// <param name="ProjectedRevenueTrendModelList"> Trend Model list of Projected Revenue</param>
        /// <returns>Return Model of RevenueOverview</returns>
        public List<TacticwiseOverviewModel> GetTacticwiseActualProjectedRevenueList(List<ActualTrendModel> ActualTrendList, List<ProjectedTrendModel> ProjectedTrendModelList)
        {
            List<TacticwiseOverviewModel> OverviewModelList = new List<TacticwiseOverviewModel>();
            double ActualTotal = 0, ProjectedTrendTotal = 0, Actual_Projected = 0, Goal = 0;
            try
            {
                TacticwiseOverviewModel objOverviewModel = new TacticwiseOverviewModel();
                List<int> TacticIdList = new List<int>();
                TacticIdList = ProjectedTrendModelList.Select(_projTactic => _projTactic.PlanTacticId).Distinct().ToList();
                TacticIdList.AddRange(ActualTrendList.Select(actual => actual.PlanTacticId).Distinct().ToList());
                TacticIdList = TacticIdList.Distinct().ToList();
                foreach (int _tacticId in TacticIdList)
                {
                    objOverviewModel = new TacticwiseOverviewModel();
                    objOverviewModel.PlanTacticId = _tacticId;
                    ActualTotal = ActualTrendList.Where(actual => actual.PlanTacticId.Equals(_tacticId)).Sum(actual => actual.TrendValue);
                    ProjectedTrendTotal = ProjectedTrendModelList.Where(_projTactic => _projTactic.PlanTacticId.Equals(_tacticId)).Sum(_projTactic => _projTactic.TrendValue);
                    Actual_Projected = ActualTotal + ProjectedTrendTotal;
                    Goal = ProjectedTrendModelList.Where(_projTactic => _projTactic.PlanTacticId.Equals(_tacticId)).Sum(_projTactic => _projTactic.Value);
                    objOverviewModel.Actual_ProjectedValue = Actual_Projected;
                    objOverviewModel.Goal = Goal;
                    OverviewModelList.Add(objOverviewModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return OverviewModelList;
        }

        #region "Old Commented Code GetLineChartData"
        ///// <summary>
        ///// This action will return the data of Revenue Line chart
        ///// </summary>
        ///// <param name="ActualTacticList"> List of Actulals Tactic</param>
        ///// <param name="ProjectedTrendModelList"> Trend Model list of Projected</param>
        ///// <param name="timeframeOption">Selected Year from left Filter</param>
        ///// <returns>Return LineChart Model</returns>
        //public lineChartData GetLineChartData(List<ActualTrendModel> ActualTrendList, List<ProjectedTrendModel> ProjectedTrendModelList, string timeframeOption, bool IsQuarterly)
        //{
        //    #region "Declare Local Varialbles"
        //    List<string> categories = new List<string>();
        //    List<series> lstseries = new List<series>();
        //    lineChartData LineChartData = new lineChartData();
        //    bool IsDisplay = false;
        //    List<double> serData1 = new List<double>();
        //    List<double> serData2 = new List<double>();
        //    //double monthlyActualTotal = 0, monthlyProjectedTotal = 0, monthlyGoalTotal = 0, TodayValue=0;
        //    double TodayValue = 0;
        //    string curntPeriod = string.Empty, currentYear = DateTime.Now.Year.ToString();
        //    //int catLength = 12;
        //    #endregion

        //    try
        //    {
        //        #region "Get Today Plot Value"
        //        if (currentYear == timeframeOption)
        //        {
        //            IsDisplay = true;
        //            TodayValue = GetTodayPlotValue(timeframeOption, IsQuarterly);
        //        }
        //        #endregion

        //        #region "Get Categories based on selected Filter value like {'Monthly','Quarterly'}"
        //        if (IsQuarterly)
        //        {
        //            categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
        //        }
        //        else
        //        {
        //            categories = GetDisplayMonthListForReport(timeframeOption); // Get Categories list for Yearly Filter value like {Jan,Feb..}.
        //        }
        //        #endregion

        //        #region "Old Code Get Categories & Series data"
        //        //#region "Get Categories based on selected Filter value like {'Monthly','Quarterly'}"
        //        //if (IsQuarterly)
        //        //{
        //        //    categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
        //        //}
        //        //else
        //        //{
        //        //    categories = GetDisplayMonthListForReport(timeframeOption); // Get Categories list for Yearly Filter value like {Jan,Feb..}.
        //        //}
        //        //#endregion

        //        //#region "Monthly/Quarterly Calculate Actual, Projected & Goal Total"
        //        //if (IsQuarterly)
        //        //{
        //        //    //curntPeriod = PeriodPrefix + i;
        //        //    List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
        //        //    List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
        //        //    List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
        //        //    List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };
        //        //    double ActualQ1 = 0, ActualQ2 = 0, ActualQ3 = 0, ActualQ4 = 0, ProjectedQ1 = 0, ProjectedQ2 = 0, ProjectedQ3 = 0, ProjectedQ4 = 0, GoalQ1 = 0, GoalQ2 = 0, GoalQ3 = 0, GoalQ4 = 0, Actual_ProjectedQ1 = 0, Actual_ProjectedQ2 = 0, Actual_ProjectedQ3 = 0, Actual_ProjectedQ4 = 0;

        //        //    ActualQ1 = ActualTrendList.Where(actual => Q1.Contains(actual.Month)).Sum(actual => actual.TrendValue);
        //        //    ProjectedQ1 = ProjectedTrendModelList.Where(_projected => Q1.Contains(_projected.Month)).Sum(_projected => _projected.TrendValue);
        //        //    GoalQ1 = ProjectedTrendModelList.Where(_projected => Q1.Contains(_projected.Month)).Sum(_projected => _projected.Value);
        //        //    Actual_ProjectedQ1 = ActualQ1 + ProjectedQ1;
        //        //    serData1.Add(Actual_ProjectedQ1);
        //        //    serData2.Add(GoalQ1);

        //        //    ActualQ2 = ActualTrendList.Where(actual => Q2.Contains(actual.Month)).Sum(actual => actual.TrendValue);
        //        //    ProjectedQ2 = ProjectedTrendModelList.Where(_projected => Q2.Contains(_projected.Month)).Sum(_projected => _projected.TrendValue);
        //        //    GoalQ2 = ProjectedTrendModelList.Where(_projected => Q2.Contains(_projected.Month)).Sum(_projected => _projected.Value);

        //        //    Actual_ProjectedQ2 = Actual_ProjectedQ1 + (ActualQ2 + ProjectedQ2);
        //        //    serData2.Add(GoalQ1 + GoalQ2);
        //        //    serData1.Add(Actual_ProjectedQ2);
        //        //    //serData2.Add(GoalQ1 + GoalQ2);

        //        //    ActualQ3 = ActualTrendList.Where(actual => Q3.Contains(actual.Month)).Sum(actual => actual.TrendValue);
        //        //    ProjectedQ3 = ProjectedTrendModelList.Where(_projected => Q3.Contains(_projected.Month)).Sum(_projected => _projected.TrendValue);
        //        //    GoalQ3 = ProjectedTrendModelList.Where(_projected => Q3.Contains(_projected.Month)).Sum(_projected => _projected.Value);

        //        //    Actual_ProjectedQ3 = Actual_ProjectedQ2 + (ActualQ3 + ProjectedQ3);
        //        //    serData2.Add(GoalQ1 + GoalQ2 + GoalQ3);
        //        //    serData1.Add(Actual_ProjectedQ3);
        //        //    //serData2.Add(GoalQ1 + GoalQ2 + GoalQ3);

        //        //    ActualQ4 = ActualTrendList.Where(actual => Q4.Contains(actual.Month)).Sum(actual => actual.TrendValue);
        //        //    ProjectedQ4 = ProjectedTrendModelList.Where(_projected => Q4.Contains(_projected.Month)).Sum(_projected => _projected.TrendValue);
        //        //    GoalQ4 = ProjectedTrendModelList.Where(_projected => Q4.Contains(_projected.Month)).Sum(_projected => _projected.Value);

        //        //    Actual_ProjectedQ4 = Actual_ProjectedQ3 + (ActualQ4 + ProjectedQ4);
        //        //    serData2.Add(GoalQ1 + GoalQ2 + GoalQ3 + GoalQ4);
        //        //    serData1.Add(Actual_ProjectedQ4);
        //        //    //serData2.Add(GoalQ1 + GoalQ2 + GoalQ3 + GoalQ4);
        //        //}
        //        //else
        //        //{
        //        //    double Actual_Projected = 0, Goal = 0;
        //        //    for (int i = 1; i <= catLength; i++)
        //        //    {
        //        //        curntPeriod = PeriodPrefix + i;
        //        //        monthlyActualTotal = ActualTrendList.Where(actual => actual.Month.Equals(curntPeriod)).Sum(actual => actual.TrendValue);
        //        //        monthlyProjectedTotal = ProjectedTrendModelList.Where(_projected => _projected.Month.Equals(curntPeriod)).Sum(_projected => _projected.TrendValue);
        //        //        monthlyGoalTotal = ProjectedTrendModelList.Where(_projected => _projected.Month.Equals(curntPeriod)).Sum(_projected => _projected.Value);

        //        //        Actual_Projected = Actual_Projected + (monthlyActualTotal + monthlyProjectedTotal);
        //        //        Goal = Goal + monthlyGoalTotal;

        //        //        serData1.Add(Actual_Projected);
        //        //        serData2.Add(Goal);
        //        //    }
        //        //}
        //        //#endregion 
        //        #endregion

        //        //lstseries = GetSeriesListByTimeFrame(ActualTrendList, ProjectedTrendModelList, timeframeOption, IsQuarterly);

        //        #region "Set Series, Categories & Marker data to Model"

        //        foreach (series _ser in lstseries)
        //        {
        //            marker objMarker1 = new marker();
        //            objMarker1.symbol = "square";
        //            _ser.marker = objMarker1;
        //        }

        //        LineChartData.categories = categories;
        //        LineChartData.series = lstseries;

        //        // Set IsDisplay & TodayValue to Plot line on Linechart graph.
        //        LineChartData.isDisplay = IsDisplay.ToString();
        //        LineChartData.todayValue = TodayValue.ToString();
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return LineChartData;
        //    //return Json(RevenueLineChartData, JsonRequestBehavior.AllowGet);
        //} 
        #endregion

        /// <summary>
        /// This action will return the data of Revenue Line chart
        /// </summary>
        /// <param name="ActualTacticList"> List of Actulals Tactic</param>
        /// <param name="ProjectedTrendModelList"> Trend Model list of Projected</param>
        /// <param name="timeframeOption">Selected Year from left Filter</param>
        /// <returns>Return LineChart Model</returns>
        public lineChartData GetLineChartData(BasicModel objBasicModel)
        {
            #region "Declare Local Varialbles"
            List<string> categories = new List<string>();
            List<series> lstseries = new List<series>();
            lineChartData LineChartData = new lineChartData();
            bool IsDisplay = false, IsQuarterly = objBasicModel.IsQuarterly;
            List<double?> serData1 = new List<double?>();
            List<double?> serData2 = new List<double?>();
            //double monthlyActualTotal = 0, monthlyProjectedTotal = 0, monthlyGoalTotal = 0, TodayValue=0;
            double TodayValue = 0, catLength = 0;
            string curntPeriod = string.Empty, currentYear = DateTime.Now.Year.ToString(), timeframeOption = objBasicModel.timeframeOption;
            //int catLength = 12;
            #endregion

            try
            {

                #region "Get Today Plot Value"
                if (currentYear == timeframeOption)
                {
                    IsDisplay = true;
                    TodayValue = GetTodayPlotValue(timeframeOption, IsQuarterly);
                }
                #endregion

                #region "Get Series list"

                if (objBasicModel == null)
                    return LineChartData;
                catLength = objBasicModel.Categories.Count;   // Set categories list count.

                #region "Monthly/Quarterly Calculate Actual, Projected & Goal Total"

                double _Actual = 0, _Projected = 0, _Goal = 0, Actual_Projected = 0, _prevActual_Projected = 0, _prevGoal = 0;

                for (int i = 0; i < catLength; i++)
                {
                    _Actual = objBasicModel.ActualList[i] != null ? objBasicModel.ActualList[i] : 0;
                    _Projected = objBasicModel.ProjectedList[i] != null ? objBasicModel.ProjectedList[i] : 0;
                    _Goal = objBasicModel.GoalList[i] != null ? objBasicModel.GoalList[i] : 0;
                    Actual_Projected = _prevActual_Projected = _prevActual_Projected + (_Actual + _Projected);
                    _Goal = _prevGoal = _prevGoal + _Goal;
                    serData1.Add(Actual_Projected);
                    serData2.Add(_Goal);
                }

                series objSeries1 = new series();
                objSeries1.name = "Actual/Projected";
                objSeries1.data = serData1;
                marker objMarker1 = new marker();
                objMarker1.symbol = "square";
                objSeries1.marker = objMarker1;

                series objSeries2 = new series();
                objSeries2.name = "Goal";
                objSeries2.data = serData2;
                marker objMarker2 = new marker();
                objMarker2.symbol = "square";
                objSeries2.marker = objMarker2;

                lstseries.Add(objSeries1);
                lstseries.Add(objSeries2);
                #endregion

                #endregion

                #region "Set Series, Categories & Marker data to Model"

                //foreach (series _ser in lstseries)
                //{
                //    marker objMarker1 = new marker();
                //    objMarker1.symbol = "square";
                //    _ser.marker = objMarker1;
                //}
                categories = objBasicModel.Categories != null ? objBasicModel.Categories : new List<string>();
                LineChartData.categories = categories;
                LineChartData.series = lstseries;

                // Set IsDisplay & TodayValue to Plot line on Linechart graph.
                LineChartData.isDisplay = IsDisplay.ToString();
                LineChartData.todayValue = TodayValue.ToString();
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return LineChartData;
            //return Json(RevenueLineChartData, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This action will return the data of Revenue Line chart
        /// </summary>
        /// <param name="BasicModel"> Basic Model</param>
        /// <returns>Return LineChart Model</returns>
        public lineChartData GetCombinationLineChartData(BasicModel objBasicModel)
        {
            #region "Declare Local Varialbles"
            List<string> categories = new List<string>();
            List<series> lstseries = new List<series>();
            lineChartData LineChartData = new lineChartData();
            bool IsDisplay = false, IsQuarterly = objBasicModel.IsQuarterly;
            List<double?> serData1 = new List<double?>();
            List<double?> serData2 = new List<double?>();
            List<double?> serData3 = new List<double?>();
            double TodayValue = 0, catLength = 0, _PointLabelWidth = 20;
            string curntPeriod = string.Empty, currentYear = DateTime.Now.Year.ToString(), timeframeOption = objBasicModel.timeframeOption;
            //int catLength = 12;
            #endregion

            try
            {

                #region "Get Today Plot Value"
                if (currentYear == timeframeOption)
                {
                    IsDisplay = true;
                    TodayValue = GetTodayPlotValue(timeframeOption, IsQuarterly, IsPadding: true);
                }
                #endregion

                #region "Get Series list"

                if (objBasicModel == null)
                    return LineChartData;
                catLength = objBasicModel.Categories.Count;   // Set categories list count.

                #region "Monthly/Quarterly Calculate Actual, Projected & Goal Total"

                double _Actual = 0, _Projected = 0, _Goal = 0, Actual_Projected = 0, _prevActual_Projected = 0, _prevGoal = 0;

                serData1.Add(null); // Insert blank data at 1st index of list to Add padding to Graph.
                serData2.Add(null);// Insert blank data at 1st index of list to Add padding to Graph.
                serData3.Add(null);// Insert blank data at 1st index of list to Add padding to Graph.

                if (!IsQuarterly)
                {
                    serData1.Add(null); // Insert blank data at 1st index of list to Add padding to Graph.
                    serData2.Add(null);// Insert blank data at 1st index of list to Add padding to Graph.
                    serData3.Add(null);// Insert blank data at 1st index of list to Add padding to Graph.
                    _PointLabelWidth = 14;
                }
                double _comparevalue = 0;
                double paddingval = IsQuarterly ? 1 : 2;
                _comparevalue = TodayValue - paddingval;
                for (int i = 0; i < catLength; i++)
                {
                    _Actual = objBasicModel.ActualList[i] != null ? objBasicModel.ActualList[i] : 0;
                    _Projected = objBasicModel.ProjectedList[i] != null ? objBasicModel.ProjectedList[i] : 0;
                    _Goal = objBasicModel.GoalList[i] != null ? objBasicModel.GoalList[i] : 0;
                    Actual_Projected = _prevActual_Projected = _prevActual_Projected + (_Actual + _Projected);
                    _Goal = _prevGoal = _prevGoal + _Goal;
                    //serData1.Add(Actual_Projected);
                    serData2.Add(_Goal);
                    if ((i) <= (_comparevalue))
                    {
                        serData1.Add(Actual_Projected);
                        if (i == _comparevalue)
                            serData3.Add(Actual_Projected);
                        else
                            serData3.Add(null);


                    }
                    else
                    {
                        serData1.Add(null);
                        serData3.Add(Actual_Projected);
                    }
                }

                series objSeries1 = new series();
                objSeries1.name = "Actual";
                objSeries1.data = serData1;
                marker objMarker1 = new marker();
                objMarker1.symbol = "circle";
                objSeries1.marker = objMarker1;
                objSeries1.showInLegend = false;
                objSeries1.shadow = false;

                series objSeries2 = new series();
                objSeries2.name = "Goal";
                objSeries2.data = serData2;
                marker objMarker2 = new marker();
                objMarker2.symbol = "circle";
                objSeries2.marker = objMarker2;
                objSeries2.showInLegend = false;
                objSeries2.shadow = false;

                series objSeries3 = new series();
                objSeries3.name = "Projected";
                objSeries3.data = serData3;
                marker objMarker3 = new marker();
                objMarker3.symbol = "circle";
                objSeries3.marker = objMarker3;
                objSeries3.showInLegend = false;
                objSeries3.shadow = true;

                lstseries.Add(objSeries1);
                lstseries.Add(objSeries2);
                lstseries.Add(objSeries3);
                #endregion

                #endregion

                #region "Set Series, Categories & Marker data to Model"

                //foreach (series _ser in lstseries)
                //{
                //    marker objMarker1 = new marker();
                //    objMarker1.symbol = "square";
                //    _ser.marker = objMarker1;
                //}
                categories = objBasicModel.Categories != null ? objBasicModel.Categories : new List<string>();
                LineChartData.categories = categories;
                LineChartData.series = lstseries;

                // Set IsDisplay & TodayValue to Plot line on Linechart graph.
                LineChartData.isDisplay = IsDisplay.ToString();
                LineChartData.todayValue = TodayValue.ToString();
                LineChartData.pointLabelWidth = _PointLabelWidth;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return LineChartData;
            //return Json(RevenueLineChartData, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This function will return list of ProjectedRevenueTren Model.
        /// This function calculate Monthwise Trend.
        /// </summary>
        /// <param name="TacticList"> List of Tactic</param>
        /// <returns>Return List of Sparklinechart data</returns>
        public List<ProjectedTrendModel> GetProjectedTrendModel(List<ProjectedTacticModel> TacticList)
        {
            #region "Declare local Variables"
            List<ProjectedTrendModel> ProjectedTrendModelList = new List<ProjectedTrendModel>();
            int TotalTacticMonths = 0, _InvolvedTacticMonths = 0;
            //double TotalRevenue = 0;
            ProjectedTrendModel objProjectedTrendModel = new ProjectedTrendModel();
            int _currentYear = Convert.ToInt32(currentYear);
            #endregion

            try
            {
                foreach (var tactic in TacticList)
                {
                    for (int _trendMonth = 1; _trendMonth <= tactic.EndMonth; _trendMonth++)
                    {
                        objProjectedTrendModel = new ProjectedTrendModel();
                        objProjectedTrendModel.PlanTacticId = tactic.TacticId;
                        objProjectedTrendModel.Value = _trendMonth >= tactic.StartMonth ? tactic.Value : 0; // if trendmonth earlier than StartMonth then set Value to 0.
                        objProjectedTrendModel.Month = PeriodPrefix + _trendMonth.ToString(); // Set Month like 'Y1','Y2','Y3'..

                        //// Calculate Trend calculation for month that is greater than current ruuning month.
                        if (_trendMonth > tactic.StartMonth && ((_currentYear < tactic.Year) || (tactic.EndMonth > currentMonth && _trendMonth >= currentMonth && _currentYear == tactic.Year)))
                        {
                            TotalTacticMonths = (tactic.EndMonth - tactic.StartMonth) + 1; // Get Total Months of Tactic.
                            _InvolvedTacticMonths = (_trendMonth - tactic.StartMonth) + 1; // Get Involved Tactic month for current Trend Month calculation.
                            objProjectedTrendModel.TrendValue = (tactic.Value / TotalTacticMonths) * _InvolvedTacticMonths; // Calculate TrendValue.
                        }
                        else
                            objProjectedTrendModel.TrendValue = 0;
                        ProjectedTrendModelList.Add(objProjectedTrendModel);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ProjectedTrendModelList;
        }

        public double GetTodayPlotValue(string timeframeOption, bool IsQuarterly, bool IsPadding = false)
        {
            double resultTodayValue = 0;
            try
            {
                #region "Get Today Date"
                DateTime currentDate = DateTime.Now;
                string year = currentDate.Year.ToString();
                if (year == timeframeOption)
                {
                    //LineChartData.isDisplay = "1";
                    int currentmonth = currentDate.Month;
                    int currentdatedays = currentDate.Day;
                    double _paddingval = 0;
                    if (!IsQuarterly)
                    {
                        int currentmonthdays = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                        _paddingval = IsPadding ? 2 : 0;
                        //resultTodayValue = _paddingval + (currentmonth - 1) + (Convert.ToDouble(currentdatedays) / Convert.ToDouble(currentmonthdays));
                        resultTodayValue = _paddingval + (currentmonth - 1);
                    }
                    // LineChartData.TodayValue = todayindex.ToString();
                    else
                    {
                        int currentQuarter = ((currentMonth - 1) / 3);
                        int totaldays = 0;
                        int currentdays = 0;
                        _paddingval = IsPadding ? 1 : 0;
                        if (currentQuarter == 0)
                        {
                            for (int i = 1; i <= 3; i++)
                            {
                                totaldays += DateTime.DaysInMonth(currentDate.Year, i);
                            }
                            for (int i = 1; i < currentmonth; i++)
                            {
                                currentdays += DateTime.DaysInMonth(currentDate.Year, i);
                            }
                            currentdays += currentdatedays;
                        }
                        else if (currentQuarter == 1)
                        {
                            for (int i = 4; i <= 6; i++)
                            {
                                totaldays += DateTime.DaysInMonth(currentDate.Year, i);
                            }
                            for (int i = 4; i < currentmonth; i++)
                            {
                                currentdays += DateTime.DaysInMonth(currentDate.Year, i);
                            }
                            currentdays += currentdatedays;
                        }
                        else if (currentQuarter == 2)
                        {
                            for (int i = 7; i <= 9; i++)
                            {
                                totaldays += DateTime.DaysInMonth(currentDate.Year, i);
                            }
                            for (int i = 7; i < currentmonth; i++)
                            {
                                currentdays += DateTime.DaysInMonth(currentDate.Year, i);
                            }
                            currentdays += currentdatedays;
                        }
                        else if (currentQuarter == 3)
                        {
                            for (int i = 10; i <= 12; i++)
                            {
                                totaldays += DateTime.DaysInMonth(currentDate.Year, i);
                            }
                            for (int i = 10; i < currentmonth; i++)
                            {
                                currentdays += DateTime.DaysInMonth(currentDate.Year, i);
                            }
                            currentdays += currentdatedays;
                        }

                        //resultTodayValue = _paddingval + (currentQuarter) + (Convert.ToDouble(currentdays) / Convert.ToDouble(totaldays));
                        resultTodayValue = _paddingval + (currentQuarter);
                    }

                }
                //else
                //{
                //    LineChartData.isDisplay = "0";
                //    LineChartData.TodayValue = "0";
                //}
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resultTodayValue;
        }

        public BasicModel GetValuesListByTimeFrame(List<ActualTrendModel> ActualTrendList, List<ProjectedTrendModel> ProjectedTrendModelList, string timeframeOption, bool IsQuarterly)
        {
            #region "Declare Local Variables"
            int categorieslength = 4;
            BasicModel objBasicModel = new BasicModel();
            List<double> _actuallist = new List<double>();
            List<double> _projectedlist = new List<double>();
            List<double> _goallist = new List<double>();
            List<double> _goalYTDList = new List<double>();
            double _Actual = 0, _Projected = 0, _Goal = 0, _prevActual = 0, _prevProjected = 0, _prevGoal = 0, _GoalYTD = 0;
            List<string> categories = new List<string>();
            // Add By Nishant Sheth
            string currentyear = DateTime.Now.Year.ToString();
            int currentEndMonth = 12;
            #endregion
            try
            {

                #region "Get Categories based on selected Filter value like {'Monthly','Quarterly'}"
                if (IsQuarterly)
                {
                    categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
                }
                else
                {
                    categories = GetDisplayMonthListForReport(timeframeOption); // Get Categories list for Yearly Filter value like {Jan,Feb..}.
                }
                #endregion

                categorieslength = categories.Count;   // Set categories list count.
                if (currentYear == timeframeOption)
                {
                    currentEndMonth = DateTime.Now.Month;
                }
                #region "Monthly/Quarterly Calculate Actual, Projected & Goal Total"
                if (IsQuarterly)
                {
                    List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
                    List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
                    List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
                    List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };

                    List<string> _curntQuarterListActual = new List<string>();
                    List<string> _curntQuarterListProjected = new List<string>();// Add By Nishant Sheth
                    List<string> _curntQuarterListGoal = new List<string>();// Add By Nishant Sheth
                    for (int i = 1; i <= categorieslength; i++)
                    {
                        #region "Get Quarter list based on loop value"
                        if (i == 1)
                        {
                            _curntQuarterListActual = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                            _curntQuarterListProjected = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) > Convert.ToInt32(currentEndMonth)).ToList();
                            _curntQuarterListGoal = Q1;
                        }
                        else if (i == 2)
                        {
                            _curntQuarterListActual = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                            _curntQuarterListProjected = Q2.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) > Convert.ToInt32(currentEndMonth)).ToList();
                            _curntQuarterListGoal = Q2;
                        }
                        else if (i == 3)
                        {
                            _curntQuarterListActual = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                            _curntQuarterListProjected = Q3.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) > Convert.ToInt32(currentEndMonth)).ToList();
                            _curntQuarterListGoal = Q3;
                        }
                        else if (i == 4)
                        {
                            _curntQuarterListActual = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                            _curntQuarterListProjected = Q4.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) > Convert.ToInt32(currentEndMonth)).ToList();
                            _curntQuarterListGoal = Q4;
                        }
                        #endregion

                        _Actual = ActualTrendList.Where(actual => _curntQuarterListActual.Contains(actual.Month)).Sum(actual => actual.TrendValue);
                        _actuallist.Add(_Actual);


                        _Projected = ProjectedTrendModelList.Where(_projected => _curntQuarterListProjected.Contains(_projected.Month)).Sum(_projected => _projected.TrendValue);
                        _projectedlist.Add(_Projected);

                        _Goal = ProjectedTrendModelList.Where(_projected => _curntQuarterListGoal.Contains(_projected.Month)).Sum(_projected => _projected.Value);
                        _goallist.Add(_Goal);

                        // Addd For Rveneue header value Goal Yeat to Date
                        _GoalYTD = ProjectedTrendModelList.Where(_projected => _curntQuarterListActual.Contains(_projected.Month)).Sum(_projected => _projected.Value);
                        _goalYTDList.Add(_GoalYTD);
                    }
                }
                else
                {
                    string curntPeriod = string.Empty;

                    for (int i = 1; i <= categorieslength; i++)
                    {
                        curntPeriod = PeriodPrefix + i;
                        //_Actual = ActualTrendList.Where(actual => actual.Month.Equals(curntPeriod)).Sum(actual => actual.TrendValue);
                        _Actual = ActualTrendList.Where(actual => Convert.ToInt32(curntPeriod.Replace(PeriodPrefix, "")) <= currentEndMonth ? actual.Month.Equals(curntPeriod) : actual.Month.Equals("")).Sum(actual => actual.TrendValue);
                        _Projected = ProjectedTrendModelList.Where(_projected => Convert.ToInt32(curntPeriod.Replace(PeriodPrefix, "")) > currentEndMonth ? _projected.Month.Equals(curntPeriod) : _projected.Month.Equals("")).Sum(_projected => _projected.TrendValue);
                        _Goal = ProjectedTrendModelList.Where(_projected => _projected.Month.Equals(curntPeriod)).Sum(_projected => _projected.Value);

                        _actuallist.Add(_Actual);
                        _projectedlist.Add(_Projected);
                        _goallist.Add(_Goal);
                    }

                    //_Actual = ActualTrendList.Where(actual => Convert.ToDateTime(DateTime.Now.Date.ToString("dd") + "-" + actual.Month + "-" + DateTime.Now.Year).Month <= currentEndMonth).Sum(actual => actual.TrendValue);
                    //_actuallist.Add(_Actual);
                }

                #endregion
                objBasicModel.Categories = categories;
                objBasicModel.ActualList = _actuallist;
                objBasicModel.ProjectedList = _projectedlist;
                objBasicModel.GoalList = _goallist;
                objBasicModel.IsQuarterly = IsQuarterly;
                objBasicModel.timeframeOption = timeframeOption;
                objBasicModel.GoalYTD = _goalYTDList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objBasicModel;
        }

        public int GetCurrentQuarterNumber()
        {
            try
            {
                return ((currentMonth - 1) / 3 + 1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #endregion

        #region "Revenue"
        public PartialViewResult GetRevenueToPlanByFilter(string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0 )
        {
            #region "Declare Local Variables"
            List<TacticStageValue> TacticData = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<ActualTrendModel> ActualTacticTrendList = new List<ActualTrendModel>();
            string revStageCode = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            List<string> ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add(revStageCode);
            bool IsTillCurrentMonth = true;
            List<string> includeMonth = GetMonthListForReport(option);
            List<ProjectedTrendModel> ProjectedTrendList = new List<ProjectedTrendModel>();

            lineChartData objLineChartData = new lineChartData();


            int customfieldId = 0;
            bool IsCampaignCustomField = false, IsProgramCustomField = false, IsTacticCustomField = false;
            List<TacticStageValue> _tacticdata = new List<TacticStageValue>();
            List<int> PlanTacticIdsList = new List<int>();
            RevenueToPlanModel objRevenueToPlanModel = new RevenueToPlanModel();
            int _customfieldOptionId = 0;
            List<RevenueContrinutionData> _TacticOptionList = new List<RevenueContrinutionData>();
            string customFieldType = string.Empty;

            /// Declarion For Card Section 
            /// Nishant Sheth
            /// 
            ViewBag.ParentLabel = ParentLabel;
            ViewBag.childId = childId;
            ViewBag.option = option;

            //#region TempData for Back Button

            //if (DrpChange != "CampaignDrp")
            //{
            //    #region TempData for Back Button
            //    //TempData["BackParentLabel"] = Common.RevenueCampaign;
            //    //TempData["BackChildLabel"] = "";
            //    //TempData["BackId"] = "";
            //    //TempData["IsBack"] = "";
            //    //TempData["HeadHireachy"] = "Marketing Revenue";
            //    //TempData["HeadTitle"] = "Marketing Revenue";
            //    //TempData["BackWithCustom"] = Common.RevenueCampaign;

            //    //TempData["HeadHireachy"] = string.Empty;
            //    //TempData["BackParentHireachy"] = string.Empty;
            //    //TempData["BackChildHireachy"] = string.Empty;
            //    //TempData["BackHeadTitle"] = string.Empty;
            //    //TempData["BackIdHireachy"] = string.Empty;
            //    //TempData["BackDummyId"] = string.Empty;

            //    #endregion

            //}

            ////TempData["BackWithCustom"] = ParentLabel;
            ////TempData["IsDispalyCustomFieldChildDDL"] = false;// For Campaign Child DDL Display Or Not
            ////string HeadHireachy = TempData["HeadHireachy"] as string;
            ////string BackParentHireachy = TempData["BackParentHireachy"] as string;
            ////string BackChildHireachy = TempData["BackChildHireachy"] as string;
            ////string BackTitleHireachy = TempData["BackHeadTitle"] as string;
            ////string BackIdHireachy = TempData["BackIdHireachy"] as string;
            ////string BackDummyId = TempData["BackDummyId"] as string;
            //string[] BackParentArray = { };
            //string[] BackChildArray = { };
            //string[] BackIdArray = { };
            //string[] BackDummyArray = { };

            //if (DrpChange != "CampaignDrp" || isDetails)
            //{
            //    HeadHireachy = HeadHireachy + "," + HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(BackHeadTitle));
            //    BackParentHireachy = BackParentHireachy + "," + ParentLabel;
            //    BackChildHireachy = BackChildHireachy + "," + childlabelType;
            //}

            //string TempDummyid = ((!string.IsNullOrEmpty(ParentLabel) ? ParentLabel.Substring(0, 3) : "Parent") + "-" + (!string.IsNullOrEmpty(childlabelType) ? childlabelType.Substring(0, 3) : "Child") + childId);

            //BackDummyId = BackDummyId + "," + TempDummyid;

            //BackDummyArray = BackDummyId.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            //List<string> dummyids = BackDummyArray.ToList<string>();

            //var readdummy = from n in dummyids where n == TempDummyid select new { n };
            //var x = dummyids.GroupBy(s => s).Select(s => new { id = s.Key, count = s.Key.Count() }).ToList().Where(s => s.count == 1 && s.id == TempDummyid).Select(s => s.id).ToList();


            //if ((!IsBackClick && isDetails) || DrpChange == "true")
            //{
            //    BackIdHireachy = BackIdHireachy + "," + childId;
            //}

            //BackDummyArray = dummyids.Distinct().ToArray();

            //TempData["BackDummyId"] = BackDummyArray;

            //TempData["HeadHireachy"] = HeadHireachy;
            //TempData["BackParentHireachy"] = BackParentHireachy;
            //TempData["BackChildHireachy"] = BackChildHireachy;
            //TempData["BackIdHireachy"] = BackIdHireachy;

            //string[] HeadHireachyArray = HeadHireachy.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            //BackParentArray = BackParentHireachy.Split(',').Distinct().ToArray().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            //BackChildArray = BackChildHireachy.Split(',').Distinct().ToArray().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            ////if (isDetails)
            //BackIdArray = BackIdHireachy.Split(',').ToArray().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            //if (BackParentArray.Count() > 1)
            //{
            //    if (BackIdArray.Count() == 4)
            //    {
            //        TempData["BackParentLabel"] = Convert.ToString(BackParentArray[BackParentArray.Count() - 1]);
            //    }
            //    else
            //    {
            //        if (BackParentArray[0].Contains(Common.CampaignCustomTitle))
            //        {
            //            TempData["BackParentLabel"] = Convert.ToString(BackParentArray[BackParentArray.Count() - 2]);
            //            if (BackIdArray.Count() == 3 && childlabelType == Common.RevenueProgram)
            //            {
            //                TempData["BackParentLabel"] = Convert.ToString(BackParentArray[BackParentArray.Count() - 1]);
            //            }

            //        }
            //        else
            //        {
            //            TempData["BackParentLabel"] = Convert.ToString(BackParentArray[BackParentArray.Count() - 2]);
            //        }
            //    }
            //}
            //else
            //{
            //    TempData["BackParentLabel"] = (BackParentArray.Count() > 0) ? Convert.ToString(BackParentArray[0]) : string.Empty;
            //}

            //if (BackChildArray.Count() > 1)
            //{
            //    if (marsterCustomField == Common.RevenueCampaign)
            //    {
            //        TempData["BackChildLabel"] = Convert.ToString(BackChildArray[BackChildArray.Count() - 2]);
            //    }
            //    else
            //    {
            //        TempData["BackChildLabel"] = Convert.ToString(BackChildArray[BackChildArray.Count() - 2]);
            //    }
            //}
            //else
            //{
            //    if (ParentLabel == Common.RevenueCampaign && !string.IsNullOrEmpty(marsterCustomField))
            //    {
            //        TempData["BackChildLabel"] = childlabelType;
            //    }
            //    else
            //    {
            //        TempData["BackChildLabel"] = string.Empty;
            //    }
            //}

            //if (BackIdArray.Count() > 1)
            //{
            //    TempData["BackId"] = Convert.ToString(BackIdArray[BackIdArray.Count() - 2]);

            //    if (BackIdArray.Count() == 2 && BackParentArray[0].Contains(Common.ProgramCustomTitle) && ParentLabel == Common.RevenueCampaign && childlabelType == Common.RevenueProgram)
            //    {
            //        TempData["IsDispalyCustomFieldChildDDL"] = true;
            //    }

            //}
            //else
            //{
            //    TempData["BackId"] = "0";
            //}

            //if (HeadHireachyArray.Count() == 0)
            //{
            //    TempData["HeadTitle"] = "Marketing Revenue";
            //}
            //else
            //{
            //    TempData["HeadTitle"] = Convert.ToString(HeadHireachyArray[HeadHireachyArray.Count() - 1]);
            //}


            //if (IsBackClick)
            //{
            //    if (marsterCustomField.Contains(Common.CampaignCustomTitle))
            //    {
            //        if (BackIdArray.Count() == 3 && childlabelType == Common.RevenueCampaign)
            //        {
            //            TempData["IsDispalyCustomFieldChildDDL"] = true;
            //        }
            //    }
            //    marsterCustomField = string.Empty;
            //    BackParentArray = BackParentHireachy.Split(',').Distinct().ToArray().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            //    BackChildArray = BackChildHireachy.Split(',').Distinct().ToArray().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            //    BackIdArray = BackIdHireachy.Split(',').ToArray().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            //    if (BackDummyArray.Count() > 0)
            //    {
            //        List<string> y = BackDummyArray.ToList<string>();
            //        y.RemoveAt(BackDummyArray.Count() - 1);
            //        BackDummyArray = y.ToArray();
            //        TempData["BackDummyId"] = string.Join(",", BackDummyArray);
            //    }

            //    if (BackParentArray.Count() > 1)
            //    {
            //        List<string> y = BackParentArray.ToList<string>();
            //        y.RemoveAt(BackParentArray.Count() - 1);
            //        BackParentArray = y.ToArray();
            //        TempData["BackParentHireachy"] = string.Join(",", BackParentArray);
            //    }
            //    if (BackChildArray.Count() > 0)
            //    {
            //        List<string> y = BackChildArray.ToList<string>();
            //        y.RemoveAt(BackChildArray.Count() - 1);
            //        BackChildArray = y.ToArray();
            //        TempData["BackChildHireachy"] = string.Join(",", BackChildArray);
            //    }
            //    if (BackIdArray.Count() > 0)
            //    {
            //        List<string> y = BackIdArray.ToList<string>();
            //        //if (BackChildArray[0] != Common.RevenueCampaign && BackChildArray.Count() != 1)
            //        {
            //            y.RemoveAt(BackIdArray.Count() - 1);
            //        }
            //        BackIdArray = y.ToArray();
            //        TempData["BackIdHireachy"] = string.Join(",", BackIdArray);
            //    }

            //    if (BackParentArray.Count() > 1)
            //    {
            //        TempData["BackParentLabel"] = Convert.ToString(BackParentArray[BackParentArray.Count() - 1]);
            //    }
            //    else
            //    {
            //        TempData["BackParentLabel"] = Convert.ToString(BackParentArray[0]);
            //    }

            //    if (BackChildArray.Count() > 1)
            //    {
            //        TempData["BackChildLabel"] = Convert.ToString(BackChildArray[BackChildArray.Count() - 1]);
            //    }
            //    else
            //    {
            //        TempData["BackChildLabel"] = string.Empty;
            //    }

            //    if (BackIdArray.Count() > 1)
            //    {
            //        TempData["BackId"] = Convert.ToString(BackIdArray[BackIdArray.Count() - 2]);
            //    }
            //    else
            //    {
            //        TempData["BackId"] = "0";
            //        if (ParentLabel.Contains(Common.TacticCustomTitle) || ParentLabel.Contains(Common.CampaignCustomTitle) || ParentLabel.Contains(Common.ProgramCustomTitle))
            //        {
            //            TempData["IsDispalyCustomFieldChildDDL"] = true;
            //        }
            //    }

            //    if (HeadHireachyArray.Count() > 0)
            //    {
            //        List<string> y = HeadHireachyArray.ToList<string>();
            //        y.RemoveAt(HeadHireachyArray.Count() - 1);
            //        HeadHireachyArray = y.ToArray();
            //        TempData["HeadHireachy"] = string.Join(",", HeadHireachyArray);
            //        if (HeadHireachyArray.Count() == 0)
            //        {
            //            TempData["HeadTitle"] = "Marketing Revenue";
            //        }
            //        else
            //        {
            //            TempData["HeadTitle"] = Convert.ToString(HeadHireachyArray[HeadHireachyArray.Count() - 1]);
            //        }
            //    }
            //}

            //#endregion

            List<Plan_Campaign_Program_Tactic> tacticlist = new List<Plan_Campaign_Program_Tactic>();


            List<TacticStageValue> Tacticdata = new List<TacticStageValue>();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            List<Plan_Campaign_Program_Tactic> _lstTactic = new List<Plan_Campaign_Program_Tactic>();
            CardSectionModel objCardSectionModel = new CardSectionModel();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            /// End Declartion
            #endregion

            int customFieldIdCardSection = 0;
            bool isTacticCustomFieldCardSection = false;
            string customFieldTypeCardSection = string.Empty;
            if (masterCustomFieldOptionId > 0)
            {
                if (marsterCustomField.Contains(Common.CampaignCustomTitle))
                {
                    int mastercustomfieldIdInner = Convert.ToInt32(marsterCustomField.Replace(Common.CampaignCustomTitle, ""));
                    List<int> campaignIds = new List<int>();
                    campaignIds = TacticData.Select(p => p.TacticObj.Plan_Campaign_Program.PlanCampaignId).Distinct().ToList();
                    string customfiledvalue = Convert.ToString(masterCustomFieldOptionId);
                    campaignIds = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && campaignIds.Contains(c.EntityId) && c.Value == customfiledvalue).Select(c => c.EntityId).ToList();
                    TacticData = TacticData.Where(t => campaignIds.Contains(t.TacticObj.Plan_Campaign_Program.PlanCampaignId)).ToList();
                }
                else if (marsterCustomField.Contains(Common.ProgramCustomTitle))
                {
                    int mastercustomfieldIdInner = Convert.ToInt32(marsterCustomField.Replace(Common.ProgramCustomTitle, ""));
                    List<int> programIds = new List<int>();
                    programIds = TacticData.Select(p => p.TacticObj.PlanProgramId).Distinct().ToList();
                    string customfiledvalue = Convert.ToString(masterCustomFieldOptionId);
                    programIds = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && programIds.Contains(c.EntityId) && c.Value == customfiledvalue).Select(c => c.EntityId).ToList();
                    TacticData = TacticData.Where(t => programIds.Contains(t.TacticObj.PlanProgramId)).ToList();
                }
                else if (marsterCustomField.Contains(Common.TacticCustomTitle))
                {
                    int mastercustomfieldIdInner = Convert.ToInt32(marsterCustomField.Replace(Common.TacticCustomTitle, ""));
                    customFieldIdCardSection = mastercustomfieldIdInner;
                    isTacticCustomFieldCardSection = true;
                    customFieldTypeCardSection = db.CustomFields.Where(c => c.CustomFieldId == mastercustomfieldIdInner).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                    List<int> tacticIds = new List<int>();
                    tacticIds = TacticData.Select(p => p.TacticObj.PlanTacticId).Distinct().ToList();
                    string customfiledvalue = Convert.ToString(masterCustomFieldOptionId);

                    tacticIds = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && tacticIds.Contains(c.EntityId) && c.Value == customfiledvalue).Select(c => c.EntityId).ToList();
                    TacticData = TacticData.Where(t => tacticIds.Contains(t.TacticObj.PlanTacticId)).ToList();
                }
            }


            try
            {
                //PlanTacticIdsList
                if (ParentLabel.Equals(Common.RevenueCampaign))
                {
                    if (childlabelType == Common.RevenueCampaign)
                    {
                        int campaignid = Convert.ToInt32(childId);
                        _tacticdata = TacticData.Where(pcpt => pcpt.TacticObj.Plan_Campaign_Program.PlanCampaignId == campaignid).Select(t => t).ToList();
                    }
                    else if (childlabelType == Common.RevenueProgram)
                    {
                        int programid = Convert.ToInt32(childId);
                        _tacticdata = TacticData.Where(pcpt => pcpt.TacticObj.PlanProgramId == programid).Select(t => t).ToList();
                    }
                    else if (childlabelType == Common.RevenueTactic)
                    {
                        int tacticid = Convert.ToInt32(childId);
                        _tacticdata = TacticData.Where(pcpt => pcpt.TacticObj.PlanTacticId == tacticid).Select(t => t).ToList();
                    }
                    else
                    {
                        _tacticdata = TacticData.ToList();
                    }

                    ActualTacticStageList = GetActualListInTacticInterval(_tacticdata, option, ActualStageCodeList, IsTillCurrentMonth);
                    ActualTacticTrendList = GetActualTrendModelForRevenueOverview(_tacticdata, ActualTacticStageList);

                    #region "Revenue : Get Tacticwise Actual_Projected Vs Goal Model data "
                    ProjectedTrendList = CalculateProjectedTrend(_tacticdata, includeMonth, revStageCode);
                    #endregion

                    #region Mapping Items for Card Section
                    tacticlist = GetTacticForReporting();
                    // Fetch the respectives Campaign Ids and Program Ids from the tactic list


                    Tacticdata = Common.GetTacticStageRelation(tacticlist, IsReport: true);

                    if (childlabelType.Contains(Common.RevenueTactic))
                    {
                        ViewBag.childlabelType = Common.RevenueTactic;

                        _lstTactic = tacticlist.ToList();

                        if (!string.IsNullOrEmpty(childId) ? Convert.ToInt32(childId) > 0 : false)
                        {
                            _lstTactic = _lstTactic.Where(t => t.PlanTacticId == (Convert.ToInt32(childId) > 0 ? Convert.ToInt32(childId) : t.PlanTacticId))
                                .ToList();
                        }

                        _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _parentId = pc.PlanTacticId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Title })
                            .Select(pct => new TacticMappingItem { ParentId = pct.Key._parentId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
                    }
                    else if (childlabelType.Contains(Common.RevenueProgram))
                    {
                        ViewBag.childlabelType = Common.RevenueTactic;

                        _lstTactic = tacticlist.ToList();
                        if (!string.IsNullOrEmpty(childId) ? Convert.ToInt32(childId) > 0 : false)
                        {
                            _lstTactic = _lstTactic.Where(t => t.PlanProgramId == (Convert.ToInt32(childId) > 0 ? Convert.ToInt32(childId) : t.PlanProgramId))
                                .ToList();
                        }
                        _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _parentId = pc.PlanTacticId, _childId = pc.PlanTacticId, _parentTitle = pc.Title })
                            .Select(pct => new TacticMappingItem { ParentId = pct.Key._parentId, ChildId = pct.Key._childId, ParentTitle = pct.Key._parentTitle }).ToList();
                    }
                    else if (childlabelType.Contains(Common.RevenueCampaign))
                    {
                        ViewBag.childlabelType = Common.RevenueProgram;

                        _lstTactic = tacticlist.ToList();

                        if (!string.IsNullOrEmpty(childId) ? Convert.ToInt32(childId) > 0 : false)
                        {
                            _lstTactic = _lstTactic.Where(t => t.Plan_Campaign_Program.PlanCampaignId == (Convert.ToInt32(childId) > 0 ? Convert.ToInt32(childId) : t.Plan_Campaign_Program.PlanCampaignId))
                                .ToList();
                        }

                        _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _parentId = pc.PlanProgramId, _childId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Title })
                            .Select(pct => new TacticMappingItem { ParentId = pct.Key._parentId, ChildId = pct.Key._childId, ParentTitle = pct.Key._parentTitle }).ToList();
                    }
                    else
                    {
                        ViewBag.childlabelType = Common.RevenueCampaign;

                        _lstTactic = tacticlist.ToList();
                        _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
                    }

                    #endregion
                }
                else if (ParentLabel.Contains(Common.TacticCustomTitle) || ParentLabel.Contains(Common.CampaignCustomTitle) || ParentLabel.Contains(Common.ProgramCustomTitle))
                {

                    _customfieldOptionId = !string.IsNullOrEmpty(childId) ? int.Parse(childId) : 0;
                    if (ParentLabel.Contains(Common.TacticCustomTitle))
                    {
                        customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.TacticCustomTitle, ""));
                        IsTacticCustomField = true;
                        if (Convert.ToInt32(childId) > 0)
                        {
                            ViewBag.ParentLabel = Common.RevenueCampaign;// Add By Nishant Sheth
                            ViewBag.childlabelType = Common.RevenueTactic;
                        }
                    }
                    else if (ParentLabel.Contains(Common.CampaignCustomTitle))
                    {
                        customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.CampaignCustomTitle, ""));
                        IsCampaignCustomField = true;
                        if (Convert.ToInt32(childId) > 0)
                        {
                            ViewBag.ParentLabel = Common.RevenueCampaign;// Add By Nishant Sheth
                            ViewBag.childlabelType = Common.RevenueCampaign;
                        }
                    }
                    else if (ParentLabel.Contains(Common.ProgramCustomTitle))
                    {
                        customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.ProgramCustomTitle, ""));
                        IsProgramCustomField = true;
                        if (Convert.ToInt32(childId) > 0)
                        {
                            ViewBag.ParentLabel = Common.RevenueCampaign;// Add By Nishant Sheth
                            ViewBag.childlabelType = Common.RevenueProgram;
                        }
                    }
                    else
                    {
                        TacticData = TacticData.ToList();
                    }

                    #region "New Code"
                    List<int> entityids = new List<int>();
                    if (IsTacticCustomField)
                    {
                        entityids = TacticData.Select(t => t.TacticObj.PlanTacticId).ToList();
                    }
                    else if (IsCampaignCustomField)
                    {
                        entityids = TacticData.Select(t => t.TacticObj.Plan_Campaign_Program.PlanCampaignId).ToList();
                    }
                    else
                    {
                        entityids = TacticData.Select(t => t.TacticObj.PlanProgramId).ToList();
                    }

                    customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                    var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();
                    if (customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                    {
                        var optionlist = cusomfieldEntity.Select(c => Convert.ToInt32(c.Value)).ToList();
                        _TacticOptionList = (from cfo in db.CustomFieldOptions
                                             where cfo.CustomFieldId == customfieldId && optionlist.Contains(cfo.CustomFieldOptionId) && cfo.IsDeleted == false
                                             select cfo).ToList().GroupBy(pc => new { id = pc.CustomFieldOptionId, title = pc.Value }).Select(pc =>
                                      new RevenueContrinutionData
                                      {
                                          Title = pc.Key.title,
                                          CustomFieldOptionid = pc.Key.id,
                                          // Fetch the filtered list based upon custom fields type
                                          planTacticList = TacticData.Where(t => cusomfieldEntity.Where(c => c.Value == pc.Key.id.ToString()).Select(c => c.EntityId).ToList().Contains(IsCampaignCustomField ? t.TacticObj.Plan_Campaign_Program.PlanCampaignId :
                                              (IsProgramCustomField ? t.TacticObj.PlanProgramId : t.TacticObj.PlanTacticId))).Select(t => t.TacticObj.PlanTacticId).ToList()
                                      }).ToList();
                    }
                    else if (customFieldType == Enums.CustomFieldType.TextBox.ToString())
                    {
                        _TacticOptionList = cusomfieldEntity.GroupBy(pc => new { title = pc.Value }).Select(pc =>
                                    new RevenueContrinutionData
                                    {
                                        Title = pc.Key.title,
                                        planTacticList = pc.Select(c => c.EntityId).ToList()
                                    }).ToList();
                    }
                    #endregion
                    if (_customfieldOptionId > 0)
                    {
                        PlanTacticIdsList = _TacticOptionList.Where(rev => rev.CustomFieldOptionid.Equals(_customfieldOptionId)).Select(rev => rev.planTacticList).FirstOrDefault();
                    }
                    else
                    {
                        _TacticOptionList.ForEach(rev => PlanTacticIdsList.AddRange(rev.planTacticList));
                    }
                    PlanTacticIdsList = PlanTacticIdsList != null ? PlanTacticIdsList.Distinct().ToList() : new List<int>();
                    #region "filter TacticData based on Customfield"

                    _tacticdata = TacticData.Where(t => PlanTacticIdsList.Contains(t.TacticObj.PlanTacticId)).ToList();

                    #endregion

                    #region Add CampaginList For CardSection Base on CustomFieldOption
                    // Add BY Nishant Sheth
                    if (ParentLabel.Contains(Common.TacticCustomTitle) || ParentLabel.Contains(Common.CampaignCustomTitle) || ParentLabel.Contains(Common.ProgramCustomTitle))
                    {


                        if (_customfieldOptionId > 0)
                        {
                            tacticlist = _tacticdata.Select(t => t.TacticObj).ToList();
                            if (ParentLabel.Contains(Common.TacticCustomTitle))
                            {
                                _cmpgnMappingList = tacticlist.GroupBy(pc => new { _parentId = pc.PlanTacticId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Title })
                                 .Select(pct => new TacticMappingItem { ParentId = pct.Key._parentId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();

                            }
                            else if (ParentLabel.Contains(Common.ProgramCustomTitle))
                            {

                                _cmpgnMappingList = tacticlist.GroupBy(pc => new { _parentId = pc.PlanProgramId, _childId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Title })
                                    .Select(pct => new TacticMappingItem { ParentId = pct.Key._parentId, ChildId = pct.Key._childId, ParentTitle = pct.Key._parentTitle }).ToList();

                            }
                            else if (ParentLabel.Contains(Common.CampaignCustomTitle))
                            {

                                _cmpgnMappingList = tacticlist.GroupBy(pc => new { _parentId = pc.Plan_Campaign_Program.PlanCampaignId, _childId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title })
                                    .Select(pct => new TacticMappingItem { ParentId = pct.Key._parentId, ChildId = pct.Key._childId, ParentTitle = pct.Key._parentTitle }).ToList();

                            }
                        }
                        else
                        {
                            foreach (var data in _TacticOptionList)
                            {
                                if (data.planTacticList.Count > 0)
                                {
                                    data.planTacticList.ForEach(innerdata => _cmpgnMappingList.Add(new TacticMappingItem { ParentTitle = data.Title, ParentId = data.CustomFieldOptionid, ChildId = innerdata }));
                                }
                                else
                                {
                                    _cmpgnMappingList.Add(new TacticMappingItem { ParentTitle = data.Title, ParentId = data.CustomFieldOptionid, ChildId = 0 });
                                }
                            }
                        }

                    }
                    #endregion

                    #region "Get ActualTrend Model list"
                    List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                    ActualTacticStageList = GetActualListInTacticInterval(_tacticdata, option, ActualStageCodeList, IsTillCurrentMonth);
                    ActualTacticList = ActualTacticStageList.Where(actual => actual.StageCode.Equals(revStageCode)).Select(actual => actual.ActualTacticList).FirstOrDefault();
                    if (_customfieldOptionId > 0)
                    {
                        List<ActualDataTable> ActualRevenueDataTable = GetActualTacticDataTablebyStageCode(customfieldId, _customfieldOptionId.ToString(), customFieldType, Enums.InspectStage.Revenue, ActualTacticList, _tacticdata, IsTacticCustomField);
                        ActualTacticTrendList = GetActualTrendModelForRevenue(_tacticdata, ActualRevenueDataTable, revStageCode);
                    }
                    else
                    {
                        ActualTacticTrendList = GetActualTrendModelForRevenueOverview(_tacticdata, ActualTacticStageList);
                    }
                    #endregion

                    if (_customfieldOptionId > 0)
                    {
                        #region "Get Tactic data by Weightage for Projected by StageCode(Revenue)"
                        List<TacticDataTable> _TacticDataTable = new List<TacticDataTable>();
                        List<TacticMonthValue> _TacticListMonth = new List<TacticMonthValue>();
                        List<ProjectedTacticModel> _TacticList = new List<ProjectedTacticModel>();
                        _TacticDataTable = GetTacticDataTablebyStageCode(customfieldId, _customfieldOptionId.ToString(), customFieldType, Enums.InspectStage.Revenue, _tacticdata, IsTacticCustomField, true);
                        _TacticListMonth = GetMonthWiseValueList(_TacticDataTable);
                        _TacticList = _TacticListMonth.Select(tac => new ProjectedTacticModel
                        {
                            TacticId = tac.Id,
                            StartMonth = tac.StartMonth,
                            EndMonth = tac.EndMonth,
                            Value = tac.Value,
                            Year = tac.StartYear
                        }).Distinct().ToList();
                        ProjectedTrendList = GetProjectedTrendModel(_TacticList);
                        ProjectedTrendList = (from _prjTac in ProjectedTrendList
                                              group _prjTac by new
                                              {
                                                  _prjTac.PlanTacticId,
                                                  _prjTac.Month,
                                                  _prjTac.Value,
                                                  _prjTac.TrendValue
                                              } into tac
                                              select new ProjectedTrendModel
                                              {
                                                  PlanTacticId = tac.Key.PlanTacticId,
                                                  Month = tac.Key.Month,
                                                  Value = tac.Key.Value,
                                                  TrendValue = tac.Key.TrendValue
                                              }).Distinct().ToList();
                        #endregion
                    }
                    else
                    {
                        ProjectedTrendList = CalculateProjectedTrend(_tacticdata, includeMonth, revStageCode);
                    }
                }
                /// Add By Nishant Sheth : 07-July-2015 
                /// Desc : Fill card section with filter option , Ticket no:#1397 



                CardSectionListModel = GetCardSectionDefaultData(_tacticdata, ActualTacticTrendList, ProjectedTrendList, _cmpgnMappingList.ToList(), option, (IsQuarterly.ToLower() == "quarterly" ? true : false), ParentLabel, isTacticCustomFieldCardSection, customFieldTypeCardSection, customFieldIdCardSection);
                objCardSectionModel.CardSectionListModel = CardSectionListModel;
                //objRevenueToPlanModel.CardSectionModel = objCardSectionModel;
                TempData["RevenueCardList"] = null;
                TempData["RevenueCardList"] = CardSectionListModel;// For Pagination Sorting and searching
                //string FilterParentLabel = ViewBag.ParentLabel;
                //string FilterchildlabelType = ViewBag.childlabelType;
                //string FilterchildId = ViewBag.childId;
                //string Filteroption = ViewBag.option;
                objRevenueToPlanModel.CardSectionModel = RevenueCardSectionModelWithFilter(0, 5, "", Enums.SortByRevenue.Revenue.ToString());
                objRevenueToPlanModel.CardSectionModel.TotalRecords = CardSectionListModel.Count();
                //ViewBag.ParentLabel = FilterParentLabel;
                //ViewBag.childlabelType = FilterchildlabelType;
                //ViewBag.childId = FilterchildId;
                //ViewBag.option = Filteroption;
                // End By Nishant Sheth
                #region "Revenue Model Values"

                #region "Get Basic Model"
                bool _isquarterly = false;
                if (!string.IsNullOrEmpty(IsQuarterly) && IsQuarterly.Equals(Enums.ViewByAllocated.Quarterly.ToString()))
                    _isquarterly = true;
                BasicModel objBasicModel = GetValuesListByTimeFrame(ActualTacticTrendList, ProjectedTrendList, option, _isquarterly);
                #endregion
                
                #region Header values
                objRevenueToPlanModel.RevenueHeaderModel = GetRevenueHeaderValue(objBasicModel, option).RevenueHeaderModel;
                #endregion

                #region "Set Linechart & Revenue Overview data to model"
                objLineChartData = GetCombinationLineChartData(objBasicModel);
                objRevenueToPlanModel.LineChartModel = objLineChartData != null ? objLineChartData : new lineChartData();
                #endregion

                #endregion

                #region "Revenue To Plan"

                #region "Calculate Barchart data by TimeFrame"
                BarChartModel objBarChartModel = new BarChartModel();
                List<BarChartSeries> lstSeries = new List<BarChartSeries>();
                List<string> _Categories = new List<string>();
                _Categories = objBasicModel.Categories;
                double catLength = _Categories != null ? _Categories.Count : 0;
                List<double> serData1 = new List<double>();
                List<double> serData2 = new List<double>();
                List<double> serData3 = new List<double>();
                double _Actual = 0, _Projected = 0, _Goal = 0, Actual_Projected = 0;
                bool _IsQuarterly = objBasicModel.IsQuarterly;
                int _compareValue = 0;
                _compareValue = _IsQuarterly ? GetCurrentQuarterNumber() : currentMonth;
                serData1.Add(0); // Insert blank data at 1st index of list to Add padding to Graph.
                serData2.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.
                serData3.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.

                if (!_IsQuarterly)
                {
                    serData1.Add(0); // Insert blank data at 1st index of list to Add padding to Graph.
                    serData2.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.
                    serData3.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.
                }

                for (int i = 0; i < catLength; i++)
                {
                    _Actual = objBasicModel.ActualList[i] != null ? objBasicModel.ActualList[i] : 0;
                    _Projected = objBasicModel.ProjectedList[i] != null ? objBasicModel.ProjectedList[i] : 0;
                    _Goal = objBasicModel.GoalList[i] != null ? objBasicModel.GoalList[i] : 0;
                    Actual_Projected = _Actual + _Projected;
                    //serData1.Add(Actual_Projected);
                    serData2.Add(_Goal);
                    if ((i + 1) <= (_compareValue))
                    {
                        serData1.Add(Actual_Projected);
                        serData3.Add(0);
                    }
                    else
                    {
                        serData1.Add(0);
                        serData3.Add(Actual_Projected);
                    }
                }

                BarChartSeries _chartSeries1 = new BarChartSeries();
                _chartSeries1.name = "Actual";
                _chartSeries1.data = serData1;
                _chartSeries1.type = "column";
                lstSeries.Add(_chartSeries1);

                BarChartSeries _chartSeries2 = new BarChartSeries();
                _chartSeries2.name = "Goal";
                _chartSeries2.data = serData2;
                _chartSeries2.type = "column";
                lstSeries.Add(_chartSeries2);

                BarChartSeries _chartSeries3 = new BarChartSeries();
                _chartSeries3.name = "Projected";
                _chartSeries3.data = serData3;
                _chartSeries3.type = "column";
                lstSeries.Add(_chartSeries3);

                List<string> _barChartCategories = new List<string>();
                if (!_IsQuarterly)
                {
                    _barChartCategories.Add(string.Empty);
                    _barChartCategories.Add(string.Empty);
                    _barChartCategories.AddRange(_Categories);
                }
                else
                {
                    _barChartCategories.Add(string.Empty);
                    _barChartCategories.AddRange(_Categories);
                }

                objBarChartModel.series = lstSeries;
                objBarChartModel.categories = _barChartCategories;

                objRevenueToPlanModel.RevenueToPlanBarChartModel = objBarChartModel;
                #endregion

                #region "Calculate DataTable"
                RevenueDataTable objRevenueDataTable = new RevenueDataTable();
                RevenueSubDataTableModel objSubDataModel = new RevenueSubDataTableModel();
                objRevenueDataTable.Categories = _Categories;
                objRevenueDataTable.ActualList = objBasicModel.ActualList;
                objRevenueDataTable.ProjectedList = objBasicModel.ProjectedList;
                objRevenueDataTable.GoalList = objBasicModel.GoalList;

                //if ParentLabel is "Campaign" or ParentLabel is "customfield" and CustomfieldOptionId selected "All" then do all calculation without weightage apply.
                if (ParentLabel.Equals(Common.RevenueCampaign) || ((ParentLabel.Contains(Common.TacticCustomTitle) || ParentLabel.Contains(Common.CampaignCustomTitle) || ParentLabel.Contains(Common.ProgramCustomTitle)) && _customfieldOptionId.Equals(0)))
                {
                    objSubDataModel = GetRevenueToPlanDataByCampaign(_tacticdata, objBasicModel.timeframeOption, objBasicModel.IsQuarterly, objBasicModel);
                }
                else
                {
                    RevenueContrinutionData _TacticOptionModel = new RevenueContrinutionData();
                    _TacticOptionModel = _TacticOptionList.Where(tac => tac.CustomFieldOptionid.Equals(_customfieldOptionId)).FirstOrDefault();
                    _TacticOptionModel = _TacticOptionModel != null ? _TacticOptionModel : new RevenueContrinutionData();
                    objSubDataModel = GetRevenueToPlanDataByCustomField(customfieldId, customFieldType, _tacticdata, _TacticOptionModel, objBasicModel.timeframeOption, IsTacticCustomField, objBasicModel.IsQuarterly, objBasicModel);
                }
                objRevenueDataTable.SubDataModel = objSubDataModel;
                objRevenueDataTable.IsQuarterly = objBasicModel.IsQuarterly;
                objRevenueDataTable.timeframeOption = objBasicModel.timeframeOption;
                objRevenueToPlanModel.RevenueToPlanDataModel = objRevenueDataTable;
                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return PartialView("_RevenueToPlan", objRevenueToPlanModel);
        }

        public RevenueSubDataTableModel GetRevenueToPlanDataByCustomField(int _CustomfieldId, string _CustomFieldType, List<TacticStageValue> TacticData, RevenueContrinutionData _TacticOptionObject, string timeFrameOption, bool _IsTacticCustomField, bool IsQuarterly, BasicModel _BasicModel)
        {
            #region "Declare local variables"

            #region "Quarterly Trend Varaibles"
            List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };
            double ActualQ1 = 0, ActualQ2 = 0, ActualQ3 = 0, ActualQ4 = 0, TotalTrendQ1 = 0, TotalTrendQ2 = 0, TotalTrendQ3 = 0, TotalTrendQ4 = 0;
            List<double> ActualList = new List<double>();
            #endregion
            List<string> IncludeCurrentMonth = new List<string>();
            #endregion

            #region "Declare local variables for RevenueDataTable"
            RevenueSubDataTableModel objSubDataTableModel = new RevenueSubDataTableModel();
            List<string> PerformanceList = new List<string>();
            #endregion
            try
            {
                #region "Get Year list"
                List<string> yearlist = new List<string>();
                yearlist.Add(timeFrameOption);
                IncludeCurrentMonth = GetMonthWithYearUptoCurrentMonth(yearlist);
                #endregion

                #region "Code for TOPRevenue"
                #region "Declare Local Variables"
                List<string> RevenueList = new List<string>();
                #endregion

                #region "Evaluate Customfield Option wise Sparkline chart data"
                #region "Get Revenue Data by CustomfieldOption  wise"
                List<double> _monthTrendList = new List<double>();
                TotalTrendQ1 = TotalTrendQ2 = TotalTrendQ3 = TotalTrendQ4 = 0;
                #region "Calculate Trend for Single CustomFieldOption value"

                #region "Calcualte Actual & Projected value Quarterly"
                //added by  Dashrath Prajapati- PL #1422
                ActualList = _BasicModel.ActualList;
                if (IsQuarterly)
                {
                    

                    TotalTrendQ1 = TotalTrendQ1 + (ActualList.ToList()[0]);
                    TotalTrendQ2 = TotalTrendQ1 + (ActualList.ToList()[1]);
                    TotalTrendQ3 = TotalTrendQ2 + (ActualList.ToList()[2]);
                    TotalTrendQ4 = TotalTrendQ3 + (ActualList.ToList()[3]);

                    if (timeFrameOption.ToLower() == DateTime.Now.Year.ToString().ToLower())
                    {
                        int _quater = ((DateTime.Now.Month - 1) / 3) + 1;
                        if (_quater == 1)
                        {
                            TotalTrendQ2 = TotalTrendQ3 = TotalTrendQ4 = 0;
                        }
                        else if (_quater == 2)
                        {
                            TotalTrendQ3 = TotalTrendQ4 = 0;
                        }
                        else if (_quater == 3)
                        {
                            TotalTrendQ4 = 0;
                        }
                    }

                }
                else
                {
                    double _actualval, _actualtotal = 0;

                    int currentEndMonth = 12;
                    if (timeFrameOption.ToLower() == DateTime.Now.Year.ToString().ToLower())
                    {
                        currentEndMonth = Convert.ToInt32(DateTime.Now.Month);
                    }
                    for (int i = 0; i < 12; i++)
                    {
                        _actualval = ActualList.ToList()[i];
                        if (currentEndMonth > i)
                        {
                            if (_actualval != 0.0)
                            {
                                _actualtotal = _actualtotal + _actualval;
                            }
                        }
                        else
                        {
                            _actualtotal = 0;
                        }
                        _monthTrendList.Add(_actualtotal);
                    }
                }
                //end
                #endregion
                #endregion
                #endregion

                #region "Set Trend data to Revenue List"
                if (IsQuarterly)
                {
                    RevenueList.Add(TotalTrendQ1.ToString());
                    RevenueList.Add(TotalTrendQ2.ToString());
                    RevenueList.Add(TotalTrendQ3.ToString());
                    RevenueList.Add(TotalTrendQ4.ToString());
                }
                else
                {
                    foreach (double _trend in _monthTrendList)
                    {
                        RevenueList.Add(_trend.ToString());
                    }
                }
                #endregion

                #endregion
                #endregion

                #region "Code for Top Performance"
                #region "Declare Local Variables"
                List<TacticDataTable> TacticDataTable = new List<TacticDataTable>();
                List<TacticStageValue> fltrTacticData = new List<TacticStageValue>();
                List<TacticMonthValue> TacticListMonth = new List<TacticMonthValue>();
                double Act_ProjQ1 = 0, Act_ProjQ2 = 0, Act_ProjQ3 = 0, Act_ProjQ4 = 0, GoalQ1 = 0, GoalQ2 = 0, GoalQ3 = 0, GoalQ4 = 0;
                #endregion

                #region "Evaluate Customfield Option wise Sparkline chart data"
                fltrTacticData = TacticData.Where(tac => _TacticOptionObject.planTacticList.Contains(tac.TacticObj.PlanTacticId)).ToList();
                #region "Get Actuals List"
                #endregion

                #endregion

                #region "Calculate Total for Proj.Vs Goal & Trend"

                List<ProjectedTacticModel> lstTotalTacticModel = new List<ProjectedTacticModel>();
                TacticListMonth = new List<TacticMonthValue>();
                TacticDataTable = GetTacticDataTablebyStageCode(_CustomfieldId, _TacticOptionObject.CustomFieldOptionid.ToString(), _CustomFieldType, Enums.InspectStage.Revenue, fltrTacticData, _IsTacticCustomField, true);
                TacticListMonth = GetMonthWiseValueList(TacticDataTable);
                lstTotalTacticModel = TacticListMonth.Select(tac => new ProjectedTacticModel
                {
                    TacticId = tac.Id,
                    StartMonth = tac.StartMonth,
                    EndMonth = tac.EndMonth,
                    Value = tac.Value,
                    Year = tac.StartYear
                }).Distinct().ToList();

                List<ProjectedTrendModel> lstTotalProjectedTrendModel = GetProjectedTrendModel(lstTotalTacticModel);
                lstTotalProjectedTrendModel = (from _prjTac in lstTotalProjectedTrendModel
                                               group _prjTac by new
                                               {
                                                   _prjTac.PlanTacticId,
                                                   _prjTac.Month,
                                                   _prjTac.Value,
                                                   _prjTac.TrendValue
                                               } into tac
                                               select new ProjectedTrendModel
                                               {
                                                   PlanTacticId = tac.Key.PlanTacticId,
                                                   Month = tac.Key.Month,
                                                   Value = tac.Key.Value,
                                                   TrendValue = tac.Key.TrendValue
                                               }).Distinct().ToList();

                if (IsQuarterly)
                {
                    #region "if timeframe Quarterly"

                     GoalQ1 = GoalQ2 = GoalQ3 = GoalQ4 = 0;

                    #region "Calculate Trend Quarterly"

                    #region "Newly added Code"

                    GoalQ1 = lstTotalProjectedTrendModel.Where(_proj => Q1.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ2 = lstTotalProjectedTrendModel.Where(_proj => Q2.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ3 = lstTotalProjectedTrendModel.Where(_proj => Q3.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ4 = lstTotalProjectedTrendModel.Where(_proj => Q4.Contains(_proj.Month)).Sum(_proj => _proj.Value);

                    TotalTrendQ1 = GoalQ1 > 0 ? (((ActualList.ToList()[0] - GoalQ1) / GoalQ1) * 100) : 0;// Change By Nishant #1424
                    TotalTrendQ2 = GoalQ2 > 0 ? (((ActualList.ToList()[1] - GoalQ2) / GoalQ2) * 100) : 0;// Change By Nishant #1424
                    TotalTrendQ3 = GoalQ3 > 0 ? (((ActualList.ToList()[2] - GoalQ3) / GoalQ3) * 100) : 0;// Change By Nishant #1424
                    TotalTrendQ4 = GoalQ4 > 0 ? (((ActualList.ToList()[3] - GoalQ4) / GoalQ4) * 100) : 0;// Change By Nishant #1424

                    #endregion

                    #endregion

                    #region "Add Total Trend value to List"
                    PerformanceList.Add(TotalTrendQ1.ToString());
                    PerformanceList.Add(TotalTrendQ2.ToString());
                    PerformanceList.Add(TotalTrendQ3.ToString());
                    PerformanceList.Add(TotalTrendQ4.ToString());
                    #endregion
                    #endregion
                }
                else
                {
                    #region "Get Total Trend value on Monthly basis"
                    double _TotalTrendValue = 0, _totalGoal = 0;

                    for (int i = 1; i <= 12; i++)
                    {
                      
                        _totalGoal = lstTotalProjectedTrendModel.Where(_proj => _proj.Month.Equals(PeriodPrefix.ToString() + i)).Sum(_proj => _proj.Value);
                        _TotalTrendValue = _totalGoal > 0 ? ((((ActualList.ToList()[i - 1]) - _totalGoal) / _totalGoal) * 100) : 0;//Change By Nishant #1424
                        PerformanceList.Add(_TotalTrendValue.ToString());
                    }
                    #endregion
                }

                #endregion
                #endregion

                #region "Code for TOPCost"

                #region "Declare Local Variables"
                List<TacticMonthValue> CurrentMonthCostList = new List<TacticMonthValue>();
                List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItemList = new List<Plan_Campaign_Program_Tactic_LineItem>();
                List<Plan_Campaign_Program_Tactic_LineItem_Actual> tblLineItemActualList = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                List<int> TacticIds = new List<int>();
                List<int> LineItemIds = new List<int>();
                List<TacticMonthValue> TacticCostData = new List<TacticMonthValue>();
                List<string> CostList = new List<string>();
                #endregion

                TacticIds = TacticData.Select(tac => tac.TacticObj.PlanTacticId).ToList();
                tblTacticLineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(line => TacticIds.Contains(line.PlanTacticId) && line.IsDeleted.Equals(false)).ToList();
                LineItemIds = tblTacticLineItemList.Select(line => line.PlanLineItemId).ToList();
                tblLineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineActual => LineItemIds.Contains(lineActual.PlanLineItemId)).ToList();

                _monthTrendList = new List<double>();
                TotalTrendQ1 = TotalTrendQ2 = TotalTrendQ3 = TotalTrendQ4 = 0;

                #region "Calculate Trend for Single CustomFieldOption value"
                fltrTacticData = TacticData.Where(tac => _TacticOptionObject.planTacticList.Contains(tac.TacticObj.PlanTacticId)).ToList();

                #region "Get Cost by LineItem"
                TacticCostData = GetActualCostDataByWeightage(_CustomfieldId, _TacticOptionObject.CustomFieldOptionid.ToString(), _CustomFieldType, fltrTacticData, tblTacticLineItemList, tblLineItemActualList, _IsTacticCustomField);
                CurrentMonthCostList = TacticCostData.Where(actual => IncludeCurrentMonth.Contains(actual.Month)).ToList();
                #endregion

                #region "Calcualte Actual & Projected value Quarterly"
                if (IsQuarterly)
                {
                    ActualQ1 = ActualQ2 = ActualQ3 = ActualQ4 = 0;

                    ActualQ1 = CurrentMonthCostList.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                    // return record from list which contains Q1 or Q2 months : Summed Up (Q1 + Q2) Actuals Value
                    ActualQ2 = CurrentMonthCostList.Where(actual => Q2.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                    // return record from list which contains Q1,Q2 or Q3 months : Summed Up (Q1 + Q2 + Q3) Actuals Value
                    ActualQ3 = CurrentMonthCostList.Where(actual => Q3.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                    // return record from list which contains Q1,Q2, Q3 or Q4 months : Summed Up (Q1 + Q2 + Q3 + Q4) Actuals Value
                    ActualQ4 = CurrentMonthCostList.Where(actual => Q4.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);

                    TotalTrendQ1 = TotalTrendQ1 + (ActualQ1);
                    TotalTrendQ2 = TotalTrendQ2 + (ActualQ2);
                    TotalTrendQ3 = TotalTrendQ3 + (ActualQ3);
                    TotalTrendQ4 = TotalTrendQ4 + (ActualQ4);

                }
                else
                {
                    string _curntPeriod = string.Empty;
                    double _actualval = 0;

                    for (int i = 1; i <= 12; i++)
                    {
                        _curntPeriod = PeriodPrefix.ToString() + i;
                        _actualval = CurrentMonthCostList.Where(actual => _curntPeriod.Equals(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                        _monthTrendList.Add(_actualval);
                    }
                }
                #endregion

                #endregion

                #region "Set Trend data to Cost List"
                if (IsQuarterly)
                {
                    CostList.Add(TotalTrendQ1.ToString());
                    CostList.Add(TotalTrendQ2.ToString());
                    CostList.Add(TotalTrendQ3.ToString());
                    CostList.Add(TotalTrendQ4.ToString());
                }
                else
                {
                    foreach (double _trend in _monthTrendList)
                    {
                        CostList.Add(_trend.ToString());
                    }
                }
                #endregion

                #endregion

                #region "Code for ROI"

                #region "Declare Local Variables"
                double costActualQ1 = 0, costActualQ2 = 0, costActualQ3 = 0, costActualQ4 = 0;
                List<string> ROIList = new List<string>();
                #endregion

                #region "Calculate Total for Proj.Vs Goal & Trend"

                #region "Calcualte Actual & Projected value Quarterly"
                if (IsQuarterly)
                {
                    costActualQ1 = costActualQ2 = costActualQ3 = costActualQ4 = 0;

                    TotalTrendQ1 = TotalTrendQ2 = TotalTrendQ3 = TotalTrendQ4 = 0;

                    //// Get Actual Revenue value upto currentmonth by Quarterly.

                    //// Get Actual Cost value upto currentmonth by Quarterly.
                    costActualQ1 = CurrentMonthCostList.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                    costActualQ2 = CurrentMonthCostList.Where(actual => Q2.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                    costActualQ3 = CurrentMonthCostList.Where(actual => Q3.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                    costActualQ4 = CurrentMonthCostList.Where(actual => Q4.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);


                    TotalTrendQ1 = (costActualQ1) != 0 ? ((((ActualList.ToList()[0]) - (costActualQ1)) / (costActualQ1)) * 100) : 0;//Change By Nishant #1423
                    TotalTrendQ2 = (costActualQ2) != 0 ? ((((ActualList.ToList()[1]) - (costActualQ2)) / (costActualQ2)) * 100) : 0;//Change By Nishant #1423
                    TotalTrendQ3 = (costActualQ3) != 0 ? ((((ActualList.ToList()[2]) - (costActualQ3)) / (costActualQ3)) * 100) : 0;//Change By Nishant #1423
                    TotalTrendQ4 = (costActualQ4) != 0 ? ((((ActualList.ToList()[3]) - (costActualQ4)) / (costActualQ4)) * 100) : 0;//Change By Nishant #1423

                    ROIList.Add(Math.Round(TotalTrendQ1, 2).ToString());
                    ROIList.Add(Math.Round(TotalTrendQ2, 2).ToString());
                    ROIList.Add(Math.Round(TotalTrendQ3, 2).ToString());
                    ROIList.Add(Math.Round(TotalTrendQ4, 2).ToString());
                }
                else
                {
                    double _revactual = 0, _costActual = 0, _TotalTrend = 0;
                    string _curntPeriod = string.Empty;
                    for (int _month = 1; _month <= 12; _month++)
                    {
                        _curntPeriod = PeriodPrefix.ToString() + _month;
                        _revactual = ActualList.ToList()[_month - 1];
                        _costActual = CurrentMonthCostList.Where(actual => _curntPeriod.Equals(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                        _TotalTrend = (_costActual) != 0 ? ((((_revactual) - (_costActual)) / (_costActual)) * 100) : 0;//Change By Nishant #1423
                        ROIList.Add(Math.Round(_TotalTrend, 2).ToString());
                    }
                }
                #endregion

                #endregion

                #endregion

                #region "Add all list to Master Model"
                objSubDataTableModel.PerformanceList = PerformanceList;
                objSubDataTableModel.CostList = CostList;
                objSubDataTableModel.ROIList = ROIList;
                objSubDataTableModel.RevenueList = RevenueList;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objSubDataTableModel;
        }

        public RevenueSubDataTableModel GetRevenueToPlanDataByCampaign(List<TacticStageValue> TacticData, string timeFrameOption, bool IsQuarterly, BasicModel _BasicModel)
        {
            #region "Declare local variables"
            string revStageCode = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            List<string> revStageCodeList = new List<string> { revStageCode };
            #region "Quarterly Trend Varaibles"
            List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };
            double ActualQ1 = 0, ActualQ2 = 0, ActualQ3 = 0, ActualQ4 = 0, TotalTrendQ1 = 0, TotalTrendQ2 = 0, TotalTrendQ3 = 0, TotalTrendQ4 = 0;
            List<double> ActualList = new List<double>();
            #endregion
            List<string> IncludeCurrentMonth = new List<string>();
            #endregion
            #region "Declare local variables for RevenueDataTable"
            RevenueSubDataTableModel objSubDataTableModel = new RevenueSubDataTableModel();
            List<string> PerformanceList = new List<string>();
            #endregion
            try
            {
                #region "Get Year list"
                List<string> yearlist = new List<string>();
                yearlist.Add(timeFrameOption);
                IncludeCurrentMonth = GetMonthWithYearUptoCurrentMonth(yearlist);
                #endregion

                #region "Code for TOPRevenue"
                #region "Declare Local Variables"
                List<string> RevenueList = new List<string>();
                List<ActualTacticListByStage> revActualTacticStageList = new List<ActualTacticListByStage>();
                List<Plan_Campaign_Program_Tactic_Actual> revActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                #endregion

                revActualTacticStageList = GetActualListUpToCurrentMonthByStageCode(TacticData, timeFrameOption, revStageCodeList, false);
                if (revActualTacticStageList != null)
                {
                    revActualTacticList = revActualTacticStageList.Where(act => act.StageCode.Equals(revStageCode)).Select(act => act.ActualTacticList).FirstOrDefault();
                }
                List<double> _monthTrendList = new List<double>();
                TotalTrendQ1 = TotalTrendQ2 = TotalTrendQ3 = TotalTrendQ4 = 0;

                #region "Calcualte Actual & Projected value Quarterly"
                //added by  Dashrath Prajapati- PL #1422
                ActualList = _BasicModel.ActualList;
                if (IsQuarterly)
                {
                   

                    TotalTrendQ1 = TotalTrendQ1 + (ActualList.ToList()[0]);
                    TotalTrendQ2 = TotalTrendQ1 + (ActualList.ToList()[1]);
                    TotalTrendQ3 = TotalTrendQ2 + (ActualList.ToList()[2]);
                    TotalTrendQ4 = TotalTrendQ3 + (ActualList.ToList()[3]);

                    if (timeFrameOption.ToLower() == DateTime.Now.Year.ToString().ToLower())
                    {
                        int _quater = ((DateTime.Now.Month - 1) / 3) + 1;
                        if (_quater == 1)
                        {
                            TotalTrendQ2 = TotalTrendQ3 = TotalTrendQ4 = 0;
                        }
                        else if (_quater == 2)
                        {
                            TotalTrendQ3 = TotalTrendQ4 = 0;
                        }
                        else if (_quater == 3)
                        {
                            TotalTrendQ4 = 0;
                        }
                    }
                }
                else
                {
                    double _actualval, _actualtotal = 0;

                    int currentEndMonth = 12;
                    if (timeFrameOption.ToLower() == DateTime.Now.Year.ToString().ToLower())
                    {
                        currentEndMonth = Convert.ToInt32(DateTime.Now.Month);
                    }
                    for (int i = 0; i < 12; i++)
                    {
                        _actualval = ActualList.ToList()[i];
                        if (currentEndMonth > i)
                        {
                            if (_actualval != 0.0)
                            {
                                _actualtotal = _actualtotal + _actualval;
                            }
                        }
                        else
                        {
                            _actualtotal = 0;

                        }
                        _monthTrendList.Add(_actualtotal);
                    }
                }
                //end
                #endregion

                #region "Set Trend data to Revenue List"
                if (IsQuarterly)
                {
                    RevenueList.Add(TotalTrendQ1.ToString());
                    RevenueList.Add(TotalTrendQ2.ToString());
                    RevenueList.Add(TotalTrendQ3.ToString());
                    RevenueList.Add(TotalTrendQ4.ToString());
                }
                else
                {
                    foreach (double _trend in _monthTrendList)
                    {
                        RevenueList.Add(_trend.ToString());
                    }
                }
                #endregion

                #endregion

                #region "Code for Top Performance"
                #region "Declare Local Variables"
                List<TacticMonthValue> TacticListMonth = new List<TacticMonthValue>();
                double GoalQ1 = 0, GoalQ2 = 0, GoalQ3 = 0, GoalQ4 = 0;
                #endregion

                #region "Calculate Total for Proj.Vs Goal & Trend"
                List<ProjectedTacticModel> lstTotalTacticModel = new List<ProjectedTacticModel>();
                TacticListMonth = new List<TacticMonthValue>();
                TacticListMonth = GetProjectedRevenueDataWithVelocity(TacticData);
                lstTotalTacticModel = TacticListMonth.Select(tac => new ProjectedTacticModel
                {
                    TacticId = tac.Id,
                    StartMonth = tac.StartMonth,
                    EndMonth = tac.EndMonth,
                    Value = tac.Value,
                    Year = tac.StartYear
                }).Distinct().ToList();

                List<ProjectedTrendModel> lstTotalProjectedTrendModel = GetProjectedTrendModel(lstTotalTacticModel);
                lstTotalProjectedTrendModel = (from _prjTac in lstTotalProjectedTrendModel
                                               group _prjTac by new
                                               {
                                                   _prjTac.PlanTacticId,
                                                   _prjTac.Month,
                                                   _prjTac.Value,
                                                   _prjTac.TrendValue
                                               } into tac
                                               select new ProjectedTrendModel
                                               {
                                                   PlanTacticId = tac.Key.PlanTacticId,
                                                   Month = tac.Key.Month,
                                                   Value = tac.Key.Value,
                                                   TrendValue = tac.Key.TrendValue
                                               }).Distinct().ToList();

                if (IsQuarterly)
                {
                    #region "if timeframe Quarterly"
                     GoalQ1 = GoalQ2 = GoalQ3 = GoalQ4 = 0;

                    #region "Calculate Trend Quarterly"
                    #region "Newly added Code"



                    GoalQ1 = lstTotalProjectedTrendModel.Where(_proj => Q1.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ2 = lstTotalProjectedTrendModel.Where(_proj => Q2.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ3 = lstTotalProjectedTrendModel.Where(_proj => Q3.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ4 = lstTotalProjectedTrendModel.Where(_proj => Q4.Contains(_proj.Month)).Sum(_proj => _proj.Value);

                    TotalTrendQ1 = GoalQ1 > 0 ? (((ActualList.ToList()[0] - GoalQ1) / GoalQ1) * 100) : 0;// Change By Nishant #1424
                    TotalTrendQ2 = GoalQ2 > 0 ? (((ActualList.ToList()[1] - GoalQ2) / GoalQ2) * 100) : 0;// Change By Nishant #1424
                    TotalTrendQ3 = GoalQ3 > 0 ? (((ActualList.ToList()[2] - GoalQ3) / GoalQ3) * 100) : 0;// Change By Nishant #1424
                    TotalTrendQ4 = GoalQ4 > 0 ? (((ActualList.ToList()[3] - GoalQ4) / GoalQ4) * 100) : 0;// Change By Nishant #1424

                    #endregion

                    #endregion

                    #region "Add Total Trend value to List"
                    PerformanceList.Add(TotalTrendQ1.ToString());
                    PerformanceList.Add(TotalTrendQ2.ToString());
                    PerformanceList.Add(TotalTrendQ3.ToString());
                    PerformanceList.Add(TotalTrendQ4.ToString());
                    #endregion
                    #endregion
                }
                else
                {
                    #region "Get Total Trend value on Monthly basis"
                    double _TotalTrendValue = 0, _totalGoal = 0;

                    for (int i = 1; i <= 12; i++)
                    {

                        _totalGoal = lstTotalProjectedTrendModel.Where(_proj => _proj.Month.Equals(PeriodPrefix.ToString() + i)).Sum(_proj => _proj.Value);
                        _TotalTrendValue = _totalGoal > 0 ? ((((ActualList.ToList()[i - 1]) - _totalGoal) / _totalGoal) * 100) : 0; // Change By Nishant #1424
                        PerformanceList.Add(_TotalTrendValue.ToString());
                    }
                    #endregion
                }

                #endregion

                #endregion

                #region "Code for TOPCost"
                #region "Declare Local Variables"
                List<TacticMonthValue> CurrentMonthCostList = new List<TacticMonthValue>();
                List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItemList = new List<Plan_Campaign_Program_Tactic_LineItem>();
                List<Plan_Campaign_Program_Tactic_LineItem_Actual> tblLineItemActualList = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                List<int> TacticIds = new List<int>();
                List<int> LineItemIds = new List<int>();
                List<TacticMonthValue> TacticCostData = new List<TacticMonthValue>();
                #endregion

                #region "Declare Local Variables for Top Cost"
                List<string> CostList = new List<string>();
                #endregion

                #region "Evaluate Customfield Option wise Sparkline chart data"

                TacticIds = TacticData.Select(tac => tac.TacticObj.PlanTacticId).ToList();
                tblTacticLineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(line => TacticIds.Contains(line.PlanTacticId) && line.IsDeleted.Equals(false)).ToList();
                LineItemIds = tblTacticLineItemList.Select(line => line.PlanLineItemId).ToList();
                tblLineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineActual => LineItemIds.Contains(lineActual.PlanLineItemId)).ToList();

                _monthTrendList = new List<double>();
                TotalTrendQ1 = TotalTrendQ2 = TotalTrendQ3 = TotalTrendQ4 = 0;

                #region "Get ActualCost Data"
                TacticCostData = GetActualCostData(TacticData, tblTacticLineItemList, tblLineItemActualList);
                CurrentMonthCostList = TacticCostData.Where(actual => IncludeCurrentMonth.Contains(actual.Month)).ToList();

                #endregion

                #region "Calcualte Actual & Projected value Quarterly"
                if (IsQuarterly)
                {
                    ActualQ1 = ActualQ2 = ActualQ3 = ActualQ4 = 0;
                    ActualQ1 = CurrentMonthCostList.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                    // return record from list which contains Q1 or Q2 months : Summed Up (Q1 + Q2) Actuals Value
                    ActualQ2 = CurrentMonthCostList.Where(actual => Q2.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                    // return record from list which contains Q1,Q2 or Q3 months : Summed Up (Q1 + Q2 + Q3) Actuals Value
                    ActualQ3 = CurrentMonthCostList.Where(actual => Q3.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                    // return record from list which contains Q1,Q2, Q3 or Q4 months : Summed Up (Q1 + Q2 + Q3 + Q4) Actuals Value
                    ActualQ4 = CurrentMonthCostList.Where(actual => Q4.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);

                    TotalTrendQ1 = TotalTrendQ1 + (ActualQ1);
                    TotalTrendQ2 = TotalTrendQ2 + (ActualQ2);
                    TotalTrendQ3 = TotalTrendQ3 + (ActualQ3);
                    TotalTrendQ4 = TotalTrendQ4 + (ActualQ4);

                }
                else
                {
                    string _curntPeriod = string.Empty;
                    double _actualval = 0;

                    for (int i = 1; i <= 12; i++)
                    {
                        _curntPeriod = PeriodPrefix.ToString() + i;
                        _actualval = CurrentMonthCostList.Where(actual => _curntPeriod.Equals(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                        _monthTrendList.Add(_actualval);
                    }
                }
                #endregion

                #region "Set Trend data to Cost List"
                if (IsQuarterly)
                {
                    CostList.Add(TotalTrendQ1.ToString());
                    CostList.Add(TotalTrendQ2.ToString());
                    CostList.Add(TotalTrendQ3.ToString());
                    CostList.Add(TotalTrendQ4.ToString());
                }
                else
                {
                    foreach (double _trend in _monthTrendList)
                    {
                        CostList.Add(_trend.ToString());
                    }
                }
                #endregion

                #endregion
                #endregion

                #region "Code for ROI"
                #region "Declare Local Variables"
                double costActualQ1 = 0, costActualQ2 = 0, costActualQ3 = 0, costActualQ4 = 0;
                #endregion

                #region "Declare Local variables for ROI Trend"
                List<string> ROIList = new List<string>();
                #endregion

                #region "Calculate Total for Proj.Vs Goal & Trend"
                #region "Calcualte Actual & Projected value Quarterly"
                if (IsQuarterly)
                {
                    costActualQ1 = costActualQ2 = costActualQ3 = costActualQ4 = 0;

                    TotalTrendQ1 = TotalTrendQ2 = TotalTrendQ3 = TotalTrendQ4 = 0;

                    //// Get Actual Revenue value upto currentmonth by Quarterly.


                    //// Get Actual Cost value upto currentmonth by Quarterly.
                    costActualQ1 = CurrentMonthCostList.Where(actual => Q1.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                    costActualQ2 = CurrentMonthCostList.Where(actual => Q2.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                    costActualQ3 = CurrentMonthCostList.Where(actual => Q3.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                    costActualQ4 = CurrentMonthCostList.Where(actual => Q4.Contains(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);

                    TotalTrendQ1 = (costActualQ1) != 0 ? ((((ActualList.ToList()[0]) - (costActualQ1)) / (costActualQ1)) * 100) : 0; // Change By Nishant #1423
                    TotalTrendQ2 = (costActualQ2) != 0 ? ((((ActualList.ToList()[1]) - (costActualQ2)) / (costActualQ2)) * 100) : 0; // Change By Nishant #1423
                    TotalTrendQ3 = (costActualQ3) != 0 ? ((((ActualList.ToList()[2]) - (costActualQ3)) / (costActualQ3)) * 100) : 0; // Change By Nishant #1423
                    TotalTrendQ4 = (costActualQ4) != 0 ? ((((ActualList.ToList()[3]) - (costActualQ4)) / (costActualQ4)) * 100) : 0; // Change By Nishant #1423

                    ROIList.Add(Math.Round(TotalTrendQ1, 2).ToString());
                    ROIList.Add(Math.Round(TotalTrendQ2, 2).ToString());
                    ROIList.Add(Math.Round(TotalTrendQ3, 2).ToString());
                    ROIList.Add(Math.Round(TotalTrendQ4, 2).ToString());
                }
                else
                {
                    double _revactual = 0, _costActual = 0, _TotalTrend = 0;
                    string _curntPeriod = string.Empty;
                    for (int _month = 1; _month <= 12; _month++)
                    {
                        _curntPeriod = PeriodPrefix.ToString() + _month;
                        _revactual = ActualList.ToList()[_month - 1];
                        _costActual = CurrentMonthCostList.Where(actual => _curntPeriod.Equals(!string.IsNullOrEmpty(actual.Month) ? actual.Month.Substring(actual.Month.Length - 2) : string.Empty)).Sum(actual => actual.Value);
                        _TotalTrend = (_costActual) != 0 ? ((((_revactual) - (_costActual)) / (_costActual) * 100)) : 0;// Change By Nishant #1423
                        ROIList.Add(Math.Round(_TotalTrend, 2).ToString());
                    }
                }
                #endregion

                #endregion

                #endregion

                #region "Add all list to Master Model"
                objSubDataTableModel.PerformanceList = PerformanceList;
                objSubDataTableModel.CostList = CostList;
                objSubDataTableModel.ROIList = ROIList;
                objSubDataTableModel.RevenueList = RevenueList;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objSubDataTableModel;
        }

        public PartialViewResult SearchSortPaginataionRevenue(int PageNo = 0, int PageSize = 5, string SearchString = "", string SortBy = "", string ParentLabel = "", string childlabelType = "", string option = "")
        {
            ViewBag.ParentLabel = ParentLabel;
            ViewBag.childlabelType = childlabelType;
            ViewBag.option = option;
            CardSectionModel cardModel = new CardSectionModel();
            cardModel = RevenueCardSectionModelWithFilter(PageNo, PageSize, SearchString, SortBy);
            //cardModel.TotalRecords = cardModel.TotalRecords;
            return PartialView("_ReportCardSection", cardModel);
        }
        /// <summary>
        /// Cretaed By Nishant Sheth
        /// Desc: Get the limited data for card section with pagination, search, and sorting features on revenue.
        /// </summary>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <param name="SearchString"></param>
        /// <param name="SortBy"></param>
        /// <returns></returns>
        public CardSectionModel RevenueCardSectionModelWithFilter(int PageNo = 0, int PageSize = 5, string SearchString = "", string SortBy = "")
        {
            #region Declartion local variables

            List<CardSectionListModel> objCardSectionList = (List<CardSectionListModel>)TempData["RevenueCardList"];
            TempData["RevenueCardList"] = objCardSectionList;
            CardSectionModel cardModel = new CardSectionModel();
        #endregion
            cardModel.TotalRecords = objCardSectionList.Count();
            if (SortBy == Enums.SortByRevenue.Cost.ToString())
            {
                objCardSectionList = objCardSectionList.Where(x => x.title.ToLower().Contains((!string.IsNullOrEmpty(SearchString) ? SearchString.ToLower().Trim() : x.title.ToLower()))).OrderByDescending(x => x.CostCardValues.Actual_Projected).Skip(PageNo * PageSize).Take(PageSize).ToList();
            }
            else if (SortBy == Enums.SortByRevenue.ROI.ToString())
            {
                objCardSectionList = objCardSectionList.Where(x => x.title.ToLower().Contains((!string.IsNullOrEmpty(SearchString) ? SearchString.ToLower().Trim() : x.title.ToLower()))).OrderByDescending(x => x.ROICardValues.Actual_Projected).Skip(PageNo * PageSize).Take(PageSize).ToList();
            }
            else
            {
                objCardSectionList = objCardSectionList.Where(x => x.title.ToLower().Contains((!string.IsNullOrEmpty(SearchString) ? SearchString.ToLower().Trim() : x.title.ToLower()))).OrderByDescending(x => x.RevenueCardValues.Actual_Projected).Skip(PageNo * PageSize).Take(PageSize).ToList();
                //objCardSectionList = objCardSectionList.OrderByDescending(x => x.RevenueCardValues.Actual_Projected).Skip(PageNo * PageSize).Take(PageSize).ToList();
            }
            cardModel.CardSectionListModel = objCardSectionList;
            if (!string.IsNullOrEmpty(SearchString))
            {
                cardModel.TotalRecords = objCardSectionList.Count();
            }
            cardModel.CuurentPageNum = PageNo;
            return cardModel;
        }
        #endregion

        #region "Conversion Report"
        /// <summary>
        ///  Get Header value for reveneue #1397
        ///  Created By Nishant Sheth
        /// </summary>
        public ReportModel GetConverstionHeaderValue(BasicModel objBasicModel, string timeFrameOption)
        {
            double _actualval, _actualtotal = 0, _projectedval, _projectedtotal = 0, _goalval, _goaltotal = 0, _goalYTD = 0;
            double _ActualPercentage, _ProjectedPercentage;
            string currentyear = DateTime.Now.Year.ToString();
            int currentEndMonth = 12;
            ReportModel objReportModel = new ReportModel();
            Projected_Goal objProjectedGoal = new Projected_Goal();
            objProjectedGoal.ActualPercentageIsnegative = true;
            objProjectedGoal.ProjectedPercentageIsnegative = true;
            List<string> categories = new List<string>();
            categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
            int categorieslength = 4;
            categorieslength = categories.Count;   // Set categories list count.
            List<ProjectedTrendModel> ProjectedTrendModelList = new List<ProjectedTrendModel>();
            if (objBasicModel.IsQuarterly)
            {

                //List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
                //List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
                //List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
                //List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };

                //List<string> _curntQuarterList = new List<string>();

                //for (int i = 1; i <= categorieslength; i++)
                //{
                //    #region "Get Quarter list based on loop value"
                //    if (i == 1)
                //    {
                //        _curntQuarterList = Q1.Where(q1 => Convert.ToInt32(q1.Replace("Y", "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                //    }
                //    else if (i == 2)
                //    {
                //        _curntQuarterList = Q2.Where(q2 => Convert.ToInt32(q2.Replace("Y", "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                //    }
                //    else if (i == 3)
                //    {
                //        _curntQuarterList = Q3.Where(q3 => Convert.ToInt32(q3.Replace("Y", "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                //    }
                //    else if (i == 4)
                //    {
                //        _curntQuarterList = Q4.Where(q4 => Convert.ToInt32(q4.Replace("Y", "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                //    }
                //    #endregion

                //}

                _actualtotal = objBasicModel.ActualList.Sum(actual => actual);
                _projectedtotal = objBasicModel.ProjectedList.Sum(projected => projected);
                _goaltotal = objBasicModel.GoalList.Sum(goal => goal);
                _goalYTD = objBasicModel.GoalYTD.Sum(goalYTD => goalYTD);
            }
            else
            {

                if (timeFrameOption.ToLower() == currentyear.ToLower())
                {
                    currentEndMonth = Convert.ToInt32(DateTime.Now.Month);
                }
                #region Calculate GoalYTD
                for (int i = 0; i < 12; i++)
                {
                    _goalval = objBasicModel.GoalList.ToList()[i];
                    if (currentEndMonth > i)
                    {
                        if (_goalval != 0.0)
                        {
                            _goalYTD = _goalYTD + _goalval;
                        }
                    }
                    else
                    {
                        _goalYTD += 0;
                    }
                    //_monthTrendList.Add(_actualtotal);
                }
                #endregion

                #region Calculate GoalTotoal/Goal Year
                _goaltotal = objBasicModel.GoalList.Sum(goal => goal);
                #endregion

                #region Calculate Actual Value
                _actualtotal = objBasicModel.ActualList.Sum(actual => actual);
                //for (int i = 0; i < 12; i++)
                //{
                //    _actualval = objBasicModel.ActualList.ToList()[i];
                //    if (currentEndMonth > i)
                //    {
                //        if (_actualval != 0.0)
                //        {
                //            _actualtotal = _actualtotal + _actualval;
                //        }
                //    }
                //    else
                //    {
                //        _actualtotal += 0;
                //    }
                //    //_monthTrendList.Add(_actualtotal);
                //}
                #endregion

                #region Calculate Projected Value
                _projectedtotal = objBasicModel.ProjectedList.Sum(projected => projected);
                #endregion
            }
            _ActualPercentage = _goalYTD != 0 ? (((_actualtotal - _goalYTD) / _goalYTD) * 100) : 0;
            if (_ActualPercentage > 0)
            {
                objProjectedGoal.ActualPercentageIsnegative = false;
            }
            _ProjectedPercentage = _goaltotal != 0 ? (((_projectedtotal - _goaltotal) / _goaltotal) * 100) : 0;
            if (_ProjectedPercentage > 0)
            {
                objProjectedGoal.ProjectedPercentageIsnegative = false;
            }
            objProjectedGoal.GoalYTD = Convert.ToString(_goalYTD);
            objProjectedGoal.GoalYear = Convert.ToString(_goaltotal);
            objProjectedGoal.Actual_Projected = Convert.ToString(_actualtotal);
            objProjectedGoal.Projected = Convert.ToString(_projectedtotal);
            objProjectedGoal.ActualPercentage = Convert.ToString(_ActualPercentage);
            objProjectedGoal.ProjectedPercentage = Convert.ToString(_ProjectedPercentage);
            objReportModel.RevenueHeaderModel = objProjectedGoal;

            return objReportModel;
        }


        #region "Get Conversion data Main method"
        //added for new Main method of conversion partial view page-Dashrath Prajapati
        [AuthorizeUser(Enums.ApplicationActivity.ReportView)]
        public ActionResult GetWaterFallData(string timeFrameOption = "thisquarter", string isQuarterly = "true")
        {
            #region "Declare Variables"
            ReportModel objReportModel = new ReportModel();
            Projected_Goal objProjectedGoal = new Projected_Goal();
            List<TacticwiseOverviewModel> OverviewModelList = new List<TacticwiseOverviewModel>();
            List<ActualTrendModel> ActualTacticTrendList = new List<ActualTrendModel>();
            List<ProjectedTrendModel> ProjectedTrendList = new List<ProjectedTrendModel>();
            conversion_Projected_Goal_LineChart objProjected_Goal_LineChart = new conversion_Projected_Goal_LineChart();
            ConversionToPlanModel objConversionToPlanModel = new ConversionToPlanModel();
            ConversionDataTable objconversionDataTable = new ConversionDataTable();
            ConversionSubDataTableModel objSubDataModel = new ConversionSubDataTableModel();

            lineChartData objLineChartData = new lineChartData();
            bool IsTillCurrentMonth = true;
            string strMQLStageCode = Enums.InspectStage.MQL.ToString();
            string strCWStageCode = Enums.InspectStage.CW.ToString();
            string revStageCode = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            string inqStageCode = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
            string mqlStageCode = Enums.InspectStageValues[strMQLStageCode].ToString();
            string cwStageCode = Enums.InspectStageValues[strCWStageCode].ToString();
            string INQStageLabel = Common.GetLabel(Common.StageModeINQ);
            string MQLStageLabel = Common.GetLabel(Common.StageModeMQL);
            string CWStageLabel = Common.GetLabel(Common.StageModeCW);
            #endregion

            // Add BY Nishant Sheth
            TempData["ConversionCard"] = null;
            ViewBag.ConvParentLabel = Common.RevenueCampaign;
            ViewBag.ConvchildlabelType = Common.RevenueCampaign;
            ViewBag.ConvchildId = 0;
            ViewBag.Convoption = timeFrameOption;
            #region TempData for Back Button
            TempData["ConvBackParentLabel"] = Common.RevenueCampaign;
            TempData["ConvBackChildLabel"] = "";
            TempData["ConvBackId"] = "";
            TempData["ConvIsBack"] = "";
            TempData["ConvHeadHireachy"] = "Waterfall";
            TempData["ConvHeadTitle"] = "Waterfall";
            TempData["ConvBackWithCustom"] = Common.RevenueCampaign;

            TempData["ConvHeadHireachy"] = string.Empty;
            TempData["ConvBackParentHireachy"] = string.Empty;
            TempData["ConvBackChildHireachy"] = string.Empty;
            TempData["ConvBackHeadTitle"] = string.Empty;
            TempData["ConvBackIdHireachy"] = string.Empty;
            TempData["ConvBackDummyId"] = string.Empty;
            //ViewBag.HeadTitleHearichy = "";
            #endregion
            // End By Nishant Sheth

            //// Get list of month display in view
            ViewBag.MonthTitle = GetDisplayMonthListForReport(timeFrameOption);
            //// get tactic list
            List<Plan_Campaign_Program_Tactic> tacticlist = GetTacticForReporting();

            // Fetch the respectives Campaign Ids and Program Ids from the tactic list
            List<int> campaignlist = tacticlist.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
            List<int> programlist = tacticlist.Select(tactic => tactic.PlanProgramId).ToList();

            //// Calculate Value for ecah tactic
            List<TacticStageValue> tacticStageList = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            //// Store Tactic Data into TempData for future used i.e. not calculate value each time when it called
            TempData["ReportData"] = tacticStageList;

            #region "Set Parent DDL data to ViewBag"
            //// conversion summary view by dropdown
            List<ViewByModel> lstParentConversionSummery = new List<ViewByModel>();
            lstParentConversionSummery.Add(new ViewByModel { Text = Common.RevenueCampaign, Value = Common.RevenueCampaign });
            lstParentConversionSummery = lstParentConversionSummery.Where(s => !string.IsNullOrEmpty(s.Text)).ToList();
            //Concat the Campaign and Program custom fields data with exsiting one. 
            var lstCustomFields = Common.GetCustomFields(tacticlist.Select(tactic => tactic.PlanTacticId).ToList(), programlist, campaignlist);
            lstParentConversionSummery = lstParentConversionSummery.Concat(lstCustomFields).ToList();
            ViewBag.parentConvertionSummery = lstParentConversionSummery;
            #endregion
            // Get child tab list
          

            //// conversion performance view by dropdown
            List<ViewByModel> lstParentConversionPerformance = new List<ViewByModel>();
            lstParentConversionPerformance.Add(new ViewByModel { Text = Common.Plan, Value = Common.Plan });
            lstParentConversionPerformance.Add(new ViewByModel { Text = Common.Trend, Value = Common.Trend });
            lstParentConversionPerformance.Add(new ViewByModel { Text = Common.Actuals, Value = Common.Actuals });
            lstParentConversionPerformance = lstParentConversionPerformance.Where(sort => !string.IsNullOrEmpty(sort.Text)).OrderBy(sort => sort.Text, new AlphaNumericComparer()).ToList();
            ViewBag.parentConvertionPerformance = lstParentConversionPerformance;
            //added by dashrath prajapati

            //// Set View By Allocated values.
            List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
            lstViewByAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarterly", Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstViewByAllocated = lstViewByAllocated.Where(modal => !string.IsNullOrEmpty(modal.Text)).ToList();
            ViewBag.ViewByAllocated = lstViewByAllocated;
            //Header section of report

            OverviewModelList = new List<TacticwiseOverviewModel>();
            ActualTacticTrendList = new List<ActualTrendModel>();
            ProjectedTrendList = new List<ProjectedTrendModel>();

            List<string> includeMonth = GetMonthListForReport(timeFrameOption);
            ProjectedTrendList = CalculateProjectedTrend(tacticStageList, includeMonth, mqlStageCode);
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<string> ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add(revStageCode);
            ActualStageCodeList.Add(inqStageCode);
            ActualStageCodeList.Add(mqlStageCode);
            ActualStageCodeList.Add(cwStageCode);
            ActualTacticStageList = GetActualListInTacticInterval(tacticStageList, timeFrameOption, ActualStageCodeList, IsTillCurrentMonth);

            List<ActualTrendModel> ActualTacticTrendModelList = GetActualTrendModelForRevenueOverview(tacticStageList, ActualTacticStageList);
            ActualTacticTrendList = ActualTacticTrendModelList.Where(actual => actual.StageCode.Equals(mqlStageCode)).ToList();
            OverviewModelList = GetTacticwiseActualProjectedRevenueList(ActualTacticTrendList, ProjectedTrendList);

            //objProjectedGoal = new Projected_Goal();
            //objProjectedGoal = GetRevenueOverviewData(OverviewModelList, timeFrameOption);
            //objReportModel.RevenueHeaderModel = objProjectedGoal != null ? objProjectedGoal : new Projected_Goal();
            //up o hereOverviewModelList

            #region Set Header Value
            BasicModel objBasicConverstionHeader = GetValuesListByTimeFrame(ActualTacticTrendList, ProjectedTrendList, timeFrameOption, (isQuarterly.ToLower() == "quarterly" ? true : false));
            objReportModel.RevenueHeaderModel = GetConverstionHeaderValue(objBasicConverstionHeader, timeFrameOption).RevenueHeaderModel;
            #endregion
            
            

            #region "Set Child DDL data to ViewBag"
            // Get child tab list
            List<ViewByModel> lstChildRevenueToPlan = new List<ViewByModel>();
            lstChildRevenueToPlan.Add(new ViewByModel { Text = "All", Value = "0" });
            List<ViewByModel> childCustomFieldOptionList = new List<ViewByModel>();
            if (lstParentConversionSummery.Count > 0)
                childCustomFieldOptionList = GetChildLabelDataViewByModel(lstParentConversionSummery.First().Value, timeFrameOption);

            ViewBag.ChildTabListRevenueToPlan = lstChildRevenueToPlan.Concat(childCustomFieldOptionList).ToList();
            #endregion

            #region "Bind Total Total TQL"

            List<ViewByModel> lstTotalTQL = new List<ViewByModel>();
            lstTotalTQL.Add(new ViewByModel { Text = "Total" + " " + INQStageLabel, Value = "0" });
            lstTotalTQL.Add(new ViewByModel { Text = "Total" + " " + MQLStageLabel, Value = "1" });
            lstTotalTQL.Add(new ViewByModel { Text = "Total" + " " + CWStageLabel, Value = "2" });

            ViewBag.TotalTQLConversion = lstTotalTQL.ToList();
            #endregion

            #region "Bind TQL,MQL,CW To plan"
            List<ViewByModel> _lstAllocated = new List<ViewByModel>();

            _lstAllocated.Add(new ViewByModel { Text = "INQ To Plan", Value = inqStageCode.ToString() });
            _lstAllocated.Add(new ViewByModel { Text = "MQL To Plan", Value = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString() });
            _lstAllocated.Add(new ViewByModel { Text = "CW To Plan", Value = Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString() });
            ViewBag.lstAllocated = _lstAllocated;
            #endregion

            #region "Set Campaign,Program,Tactic list to ViewBag"
            List<Plan_Campaign_Program_Tactic> _lstTactic = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).ToList();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();

            List<int> campaignIds = tacticlist.Where(t => t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == Sessions.User.ClientId).Select(t => t.Plan_Campaign_Program.PlanCampaignId).Distinct().ToList<int>();
            var campaignList = db.Plan_Campaign.Where(pc => campaignIds.Contains(pc.PlanCampaignId))
                    .Select(pcp => new { PlanCampaignId = pcp.PlanCampaignId, Title = pcp.Title })
                    .OrderBy(pcp => pcp.Title).ToList();
            campaignList = campaignList.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
            var lstCampaignList = campaignList;
            lstCampaignList.Insert(0, new { PlanCampaignId = 0, Title = "All Campaigns" });

            _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();

            //// Get Program list for dropdown
            var programList = db.Plan_Campaign_Program.Where(pc => campaignIds.Contains(pc.PlanCampaignId))
                   .Select(c => new { PlanProgramId = c.PlanProgramId, Title = c.Title })
                   .OrderBy(pcp => pcp.Title).ToList();
            programList = programList.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
            var lstProgramList = programList;
            lstProgramList.Insert(0, new { PlanProgramId = 0, Title = "All Programs" });

            //// Get tactic list for dropdown
            var tacticListinner = tacticlist.Select(t => new { PlanTacticId = t.PlanTacticId, Title = t.Title })
                .OrderBy(pcp => pcp.Title).ToList();
            tacticListinner = tacticListinner.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
            var lstTacticList = tacticListinner;
            lstTacticList.Insert(0, new { PlanTacticId = 0, Title = "All Tactics" });

            //// Set DDL List in ViewBag.
            ViewBag.CampaignDropdownList = lstCampaignList;
            ViewBag.ProgramDropdownList = lstProgramList;
            ViewBag.TacticDropdownList = lstTacticList;
            #endregion

            bool IsQuarterly = false;

            if (!string.IsNullOrEmpty(isQuarterly) && !isQuarterly.Equals(Enums.ViewByAllocated.Monthly.ToString()))
            {
                IsQuarterly = true;
            }
            if (IsQuarterly)
                ViewBag.SelectedTimeFrame = Enums.PlanAllocatedBy.quarters.ToString();
            else
                ViewBag.SelectedTimeFrame = Enums.PlanAllocatedBy.months.ToString();
            //up to here

            //for using card
            ProjectedTrendList = new List<ProjectedTrendModel>();
            ActualTacticStageList = new List<ActualTacticListByStage>();
            ActualTacticTrendList = new List<ActualTrendModel>();
            ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add(inqStageCode);
            ProjectedTrendList = CalculateProjectedTrend(tacticStageList, includeMonth, inqStageCode);

            ActualTacticStageList = GetActualListInTacticInterval(tacticStageList, timeFrameOption, ActualStageCodeList, IsTillCurrentMonth);
            ActualTacticTrendList = GetActualTrendModelForRevenueOverview(tacticStageList, ActualTacticStageList);




            #region "Get Basic model"
            BasicModel objBasicModelDataTable = GetValuesListByTimeFrame(ActualTacticTrendList, ProjectedTrendList, timeFrameOption, IsQuarterly);
            #endregion

            #region "Calculate DataTable"
            List<string> _Categories = new List<string>();
            _Categories = objBasicModelDataTable.Categories;
            double catLength = _Categories != null ? _Categories.Count : 0;
            objSubDataModel = GetConversionToPlanDataByCampaign(tacticStageList, timeFrameOption, objBasicModelDataTable.IsQuarterly, inqStageCode, objBasicModelDataTable); //method change for first time getting  
            objconversionDataTable.SubDataModel = objSubDataModel;
            objconversionDataTable.Categories = _Categories;
            objconversionDataTable.ActualList = objBasicModelDataTable.ActualList;
            objconversionDataTable.ProjectedList = objBasicModelDataTable.ProjectedList;
            objconversionDataTable.GoalList = objBasicModelDataTable.GoalList;
            objconversionDataTable.IsQuarterly = objBasicModelDataTable.IsQuarterly;
            objconversionDataTable.timeframeOption = objBasicModelDataTable.timeframeOption;
            objConversionToPlanModel.ConversionToPlanDataTableModel = objconversionDataTable;
            #endregion

            #region "Set Linechart & Revenue Overview data to model for combine"
            objLineChartData = GetCombinationLineChartData(objBasicModelDataTable);
            objConversionToPlanModel.LineChartModel = objLineChartData;
            #endregion

            #region "Barchart"
            #region "Calculate Barchart data by TimeFrame"
            BarChartModel objBarChartModel = new BarChartModel();
            List<BarChartSeries> lstSeries = new List<BarChartSeries>();
            _Categories = objBasicModelDataTable.Categories;
            catLength = _Categories != null ? _Categories.Count : 0;
            List<double> serData1 = new List<double>();
            List<double> serData2 = new List<double>();
            List<double> serData3 = new List<double>();
            double _Actual = 0, _Projected = 0, _Goal = 0, Actual_Projected = 0, _plotBandFromValue = 0;//, _plotBandToValue = 0, _PlotBandFromValue = 0;
            bool _IsQuarterly = objBasicModelDataTable.IsQuarterly;
            int _compareValue = 0;
            serData1.Add(0); // Insert blank data at 1st index of list to Add padding to Graph.
            serData2.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.
            serData3.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.

            if (!_IsQuarterly)
            {
                serData1.Add(0); // Insert blank data at 1st index of list to Add padding to Graph.
                serData2.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.
                serData3.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.
            }
            _compareValue = IsQuarterly ? GetCurrentQuarterNumber() : currentMonth;

            #region "Set PlotBand From & To values"
            _plotBandFromValue = IsQuarterly && (_compareValue != _lastQuarterValue) ? (_compareValue + _constQuartPlotBandPadding) : 0;
            objBarChartModel.plotBandFromValue = _plotBandFromValue;
            objBarChartModel.plotBandToValue = _plotBandFromValue > 0 ? (_PlotBandToValue) : 0;
            #endregion

            for (int i = 0; i < catLength; i++)
            {
                _Actual = objBasicModelDataTable.ActualList[i] != null ? objBasicModelDataTable.ActualList[i] : 0;
                _Projected = objBasicModelDataTable.ProjectedList[i] != null ? objBasicModelDataTable.ProjectedList[i] : 0;
                _Goal = objBasicModelDataTable.GoalList[i] != null ? objBasicModelDataTable.GoalList[i] : 0;
                Actual_Projected = _Actual + _Projected;
                //serData1.Add(Actual_Projected);
                serData2.Add(_Goal);
                if ((i + 1) <= (_compareValue))
                {
                    serData1.Add(Actual_Projected);
                    serData3.Add(0);
                }
                else
                {
                    serData1.Add(0);
                    serData3.Add(Actual_Projected);
                }
            }
            List<string> _barChartCategories = new List<string>();
            if (!IsQuarterly)
            {
                _barChartCategories.Add(string.Empty);
                _barChartCategories.Add(string.Empty);
                _barChartCategories.AddRange(_Categories);
            }
            else
            {
                _barChartCategories.Add(string.Empty);
                _barChartCategories.AddRange(_Categories);
            }

            BarChartSeries _chartSeries1 = new BarChartSeries();
            _chartSeries1.name = "Actual";
            _chartSeries1.data = serData1;
            _chartSeries1.type = "column";
            lstSeries.Add(_chartSeries1);

            BarChartSeries _chartSeries2 = new BarChartSeries();
            _chartSeries2.name = "Goal";
            _chartSeries2.data = serData2;
            _chartSeries2.type = "column";
            lstSeries.Add(_chartSeries2);

            BarChartSeries _chartSeries3 = new BarChartSeries();
            _chartSeries3.name = "Projected";
            _chartSeries3.data = serData3;
            _chartSeries3.type = "column";
            lstSeries.Add(_chartSeries3);

            objBarChartModel.series = lstSeries;
            objBarChartModel.categories = _barChartCategories;
            objConversionToPlanModel.ConversionToPlanBarChartModel = objBarChartModel;
            #endregion
            #endregion

            #region "CardSection Model"
            CardSectionModel objCardSectionModel = new CardSectionModel();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();
            CardSectionListModel = GetConversionCardSectionList(tacticStageList, _cmpgnMappingList, timeFrameOption, IsQuarterly, Common.RevenueCampaign.ToString(),false, "", 0);
            //CardSectionListModel = GetConversionCardSectionDefaultData(tacticStageList, OverviewCardModelList, _cmpgnMappingList, timeFrameOption, IsQuarterly);
            objCardSectionModel.CardSectionListModel = CardSectionListModel;
            //objReportModel.CardSectionModel = objCardSectionModel;
            TempData["ConverstionCardList"] = null;
            TempData["ConverstionCardList"] = CardSectionListModel;// For Pagination Sorting and searching
            objReportModel.CardSectionModel = ConverstionCardSectionModelWithFilter(0, 5, "", Enums.SortByWaterFall.INQ.ToString());
            objReportModel.CardSectionModel.TotalRecords = CardSectionListModel.Count();
            #endregion
            objReportModel.ConversionToPlanModel = objConversionToPlanModel;
            return PartialView("_ReportConversion", objReportModel);
        }
        #endregion

        #region "Get conversion to plan data based on campaign and code wise -Dashrath Prajapati"
        public ConversionSubDataTableModel GetConversionToPlanDataByCampaign(List<TacticStageValue> TacticData, string timeFrameOption, bool IsQuarterly, string Code, BasicModel BasicModelData)
        {
            #region "Declare local variables"
            string revStageCode = Code;
            List<string> revStageCodeList = new List<string> { revStageCode };
            #region "Quarterly Trend Varaibles"
            List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };
            double TotalTrendQ1 = 0, TotalTrendQ2 = 0, TotalTrendQ3 = 0, TotalTrendQ4 = 0;
            List<double> ActualList = new List<double>();
            #endregion
            #endregion

            #region "Declare local variables for RevenueDataTable"
            ConversionSubDataTableModel objSubDataTableModel = new ConversionSubDataTableModel();
            List<string> PerformanceList = new List<string>();
            #endregion
            try
            {
                #region "Code for TOPRevenue"
                #region "Declare Local Variables"
                List<string> RevenueList = new List<string>();
                #endregion

                List<double> _monthTrendList = new List<double>();
                TotalTrendQ1 = TotalTrendQ2 = TotalTrendQ3 = TotalTrendQ4 = 0;

                #region "Calcualte Actual & Projected value Quarterly"
                ActualList = BasicModelData.ActualList; //added by  Dashrath Prajapati- PL #1422
                if (IsQuarterly)
                {
                    

                    TotalTrendQ1 = TotalTrendQ1 + (ActualList.ToList()[0]);
                    TotalTrendQ2 = TotalTrendQ1 + (ActualList.ToList()[1]);
                    TotalTrendQ3 = TotalTrendQ2 + (ActualList.ToList()[2]);
                    TotalTrendQ4 = TotalTrendQ3 + (ActualList.ToList()[3]);

                    if (timeFrameOption.ToLower() == DateTime.Now.Year.ToString().ToLower())
                    {
                        int _quater = ((DateTime.Now.Month - 1) / 3) + 1;
                        if (_quater == 1)
                        {
                            TotalTrendQ2 = TotalTrendQ3 = TotalTrendQ4 = 0;
                        }
                        else if (_quater == 2)
                        {
                            TotalTrendQ3 = TotalTrendQ4 = 0;
                        }
                        else if (_quater == 3)
                        {
                            TotalTrendQ4 = 0;
                        }
                    }
                }
                else
                {
                    //string _curntPeriod = string.Empty;
                    double _actualval, _actualtotal = 0;

                    int currentEndMonth = 12;
                    if (timeFrameOption.ToLower() == DateTime.Now.Year.ToString().ToLower())
                    {
                        currentEndMonth = Convert.ToInt32(DateTime.Now.Month);
                    }
                    for (int i = 0; i < 12; i++)
                    {
                        _actualval = ActualList.ToList()[i];
                        if (currentEndMonth > i)
                        {
                            if (_actualval != 0.0)
                            {
                                _actualtotal = _actualtotal + _actualval;
                            }
                        }
                        else
                        {
                            _actualtotal = 0;
                        }
                        _monthTrendList.Add(_actualtotal);
                    }
                }
                //end
                #endregion

                #region "Set Trend data to Revenue List"
                if (IsQuarterly)
                {
                    RevenueList.Add(TotalTrendQ1.ToString());
                    RevenueList.Add(TotalTrendQ2.ToString());
                    RevenueList.Add(TotalTrendQ3.ToString());
                    RevenueList.Add(TotalTrendQ4.ToString());
                }
                else
                {
                    foreach (double _trend in _monthTrendList)
                    {
                        RevenueList.Add(_trend.ToString());
                    }
                }
                #endregion
                #endregion

                #region "Code for Top Performance"
                #region "Declare Local Variables"
                List<TacticMonthValue> TacticListMonth = new List<TacticMonthValue>();
                List<string> includeMonth = GetMonthListForReport(timeFrameOption);
                double GoalQ1 = 0, GoalQ2 = 0, GoalQ3 = 0, GoalQ4 = 0;
                #endregion

                #region "Calculate Total for Proj.Vs Goal & Trend"
                List<ProjectedTacticModel> lstTotalTacticModel = new List<ProjectedTacticModel>();
                TacticListMonth = new List<TacticMonthValue>();
                if (Code.Equals(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()))
                {
                    // Get TacticDataTable list of Projected MQL.
                    TacticListMonth = GetProjectedMQLDataWithVelocity(TacticData).Where(mr => includeMonth.Contains(mr.Month)).ToList();
                }
                else if (Code.Equals(Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString()))
                {
                    // Get TacticDataTable list of Projected CW.
                    TacticListMonth = GetProjectedCWDataWithVelocity(TacticData).Where(mr => includeMonth.Contains(mr.Month)).ToList();
                }
                else if (Code.Equals(Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString()))
                {
                    // Get TacticDataTable list of Projected INQ.
                    TacticListMonth = GetProjectedINQDataWithVelocity(TacticData).Where(mr => includeMonth.Contains(mr.Month)).ToList();
                }
                lstTotalTacticModel = TacticListMonth.Select(tac => new ProjectedTacticModel
                {
                    TacticId = tac.Id,
                    StartMonth = tac.StartMonth,
                    EndMonth = tac.EndMonth,
                    Value = tac.Value,
                    Year = tac.StartYear
                }).Distinct().ToList();

                List<ProjectedTrendModel> lstTotalProjectedTrendModel = GetProjectedTrendModel(lstTotalTacticModel);
                lstTotalProjectedTrendModel = (from _prjTac in lstTotalProjectedTrendModel
                                               group _prjTac by new
                                               {
                                                   _prjTac.PlanTacticId,
                                                   _prjTac.Month,
                                                   _prjTac.Value,
                                                   _prjTac.TrendValue
                                               } into tac
                                               select new ProjectedTrendModel
                                               {
                                                   PlanTacticId = tac.Key.PlanTacticId,
                                                   Month = tac.Key.Month,
                                                   Value = tac.Key.Value,
                                                   TrendValue = tac.Key.TrendValue
                                               }).Distinct().ToList();

                if (IsQuarterly)
                {
                    #region "if timeframe Quarterly"
                    GoalQ1 = GoalQ2 = GoalQ3 = GoalQ4 = 0;

                    #region "Calculate Trend Quarterly"
                    #region "Newly added Code"


                    GoalQ1 = lstTotalProjectedTrendModel.Where(_proj => Q1.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ2 = lstTotalProjectedTrendModel.Where(_proj => Q2.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ3 = lstTotalProjectedTrendModel.Where(_proj => Q3.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ4 = lstTotalProjectedTrendModel.Where(_proj => Q4.Contains(_proj.Month)).Sum(_proj => _proj.Value);

                    TotalTrendQ1 = GoalQ1 > 0 ? (((ActualList.ToList()[0] - GoalQ1) / GoalQ1) * 100) : 0;// Change By Nishant #1424
                    TotalTrendQ2 = GoalQ2 > 0 ? (((ActualList.ToList()[1] - GoalQ2) / GoalQ2) * 100) : 0;// Change By Nishant #1424
                    TotalTrendQ3 = GoalQ3 > 0 ? (((ActualList.ToList()[2] - GoalQ3) / GoalQ3) * 100) : 0;// Change By Nishant #1424
                    TotalTrendQ4 = GoalQ4 > 0 ? (((ActualList.ToList()[3] - GoalQ4) / GoalQ4) * 100) : 0;// Change By Nishant #1424
                    #endregion
                    #endregion

                    #region "Add Total Trend value to List"
                    PerformanceList.Add(TotalTrendQ1.ToString());
                    PerformanceList.Add(TotalTrendQ2.ToString());
                    PerformanceList.Add(TotalTrendQ3.ToString());
                    PerformanceList.Add(TotalTrendQ4.ToString());
                    #endregion
                    #endregion
                }
                else
                {
                    #region "Get Total Trend value on Monthly basis"
                    double _TotalTrendValue = 0, _totalGoal = 0;

                    for (int i = 1; i <= 12; i++)
                    {

                        _totalGoal = lstTotalProjectedTrendModel.Where(_proj => _proj.Month.Equals(PeriodPrefix.ToString() + i)).Sum(_proj => _proj.Value);
                        _TotalTrendValue = _totalGoal > 0 ? ((((ActualList.ToList()[i - 1]) - _totalGoal) / _totalGoal) * 100) : 0;// Change By Nishant #1424
                        PerformanceList.Add(_TotalTrendValue.ToString());
                    }
                    #endregion
                }
                #endregion

                #endregion

                #region "Add all list to Master Model"
                objSubDataTableModel.PerformanceList = PerformanceList;
                objSubDataTableModel.RevenueList = RevenueList;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objSubDataTableModel;
        }
        #endregion

        #region "GetConversion plan by custom field of conversion part-Dashrath Prajapati"
        public ConversionSubDataTableModel GetConversionToPlanDataByCustomField(int _CustomfieldId, string _CustomFieldType, List<TacticStageValue> TacticData, RevenueContrinutionData _TacticOptionObject, string timeFrameOption, bool _IsTacticCustomField, bool IsQuarterly, string Code, BasicModel _BasicModel)
        {
            #region "Declare local variables"
            #region "Quarterly Trend Varaibles"
            List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };
            double ActualQ1 = 0, ActualQ2 = 0, ActualQ3 = 0, ActualQ4 = 0, TotalTrendQ1 = 0, TotalTrendQ2 = 0, TotalTrendQ3 = 0, TotalTrendQ4 = 0;
            List<double> ActualList = new List<double>();
            #endregion
            #endregion

            #region "Declare local variables for RevenueDataTable"
            ConversionSubDataTableModel objSubDataTableModel = new ConversionSubDataTableModel();
            List<string> PerformanceList = new List<string>();
            #endregion
            try
            {
                Enums.InspectStage resultCode = new Enums.InspectStage();
                string strINQtageCode = Enums.InspectStage.ProjectedStageValue.ToString();
                if (Code.Equals(strINQtageCode))
                    resultCode = Enums.InspectStage.INQ;
                else
                    resultCode = (Enums.InspectStage)Enum.Parse(typeof(Enums.InspectStage), Code);

                #region "Code for TOPRevenue"
                #region "Declare Local Variables"
                List<string> RevenueList = new List<string>();
                #endregion

                #region "Evaluate Customfield Option wise Sparkline chart data"
                #region "Get Revenue Data by CustomfieldOption  wise"
                List<double> _monthTrendList = new List<double>();
                TotalTrendQ1 = TotalTrendQ2 = TotalTrendQ3 = TotalTrendQ4 = 0;
                #region "Calculate Trend for Single CustomFieldOption value"
                #region "Calcualte Actual & Projected value Quarterly"
                //added by  Dashrath Prajapati- PL #1422
                ActualList = _BasicModel.ActualList;
                if (IsQuarterly)
                {
                    ActualQ1 = ActualQ2 = ActualQ3 = ActualQ4 = 0;

                    ActualQ1 = ActualList.ToList()[0];
                    ActualQ2 = ActualList.ToList()[1];
                    ActualQ3 = ActualList.ToList()[2];
                    ActualQ4 = ActualList.ToList()[3];

                    TotalTrendQ1 = TotalTrendQ1 + (ActualQ1);
                    TotalTrendQ2 = TotalTrendQ1 + (ActualQ2);
                    TotalTrendQ3 = TotalTrendQ2 + (ActualQ3);
                    TotalTrendQ4 = TotalTrendQ3 + (ActualQ4);
                    string currentyear = DateTime.Now.Year.ToString();
                    if (timeFrameOption.ToLower() == currentyear.ToLower())
                    {
                        int _quater = ((DateTime.Now.Month - 1) / 3) + 1;
                        if (_quater == 1)
                        {
                            TotalTrendQ2 = TotalTrendQ3 = TotalTrendQ4 = 0;
                        }
                        else if (_quater == 2)
                        {
                            TotalTrendQ3 = TotalTrendQ4 = 0;
                        }
                        else if (_quater == 3)
                        {
                            TotalTrendQ4 = 0;
                        }
                    }

                }
                else
                {
                    double _actualval, _actualtotal = 0;
                    string currentyear = DateTime.Now.Year.ToString();
                    int currentEndMonth = 12;
                    if (timeFrameOption.ToLower() == currentyear.ToLower())
                    {
                        currentEndMonth = Convert.ToInt32(DateTime.Now.Month);
                    }
                    for (int i = 0; i < 12; i++)
                    {
                        _actualval = ActualList.ToList()[i];
                        if (currentEndMonth > i)
                        {
                            if (_actualval != 0.0)
                            {
                                _actualtotal = _actualtotal + _actualval;
                            }
                        }
                        else
                        {
                            _actualtotal = 0;
                        }
                        _monthTrendList.Add(_actualtotal);
                    }
                }
                //end
                #endregion
                #endregion
                #endregion

                #region "Set Trend data to Revenue List"
                if (IsQuarterly)
                {
                    RevenueList.Add(TotalTrendQ1.ToString());
                    RevenueList.Add(TotalTrendQ2.ToString());
                    RevenueList.Add(TotalTrendQ3.ToString());
                    RevenueList.Add(TotalTrendQ4.ToString());
                }
                else
                {
                    foreach (double _trend in _monthTrendList)
                    {
                        RevenueList.Add(_trend.ToString());
                    }
                }
                #endregion

                #endregion
                #endregion

                #region "Code for Top Performance"
                #region "Declare Local Variables"
                List<TacticDataTable> TacticDataTable = new List<TacticDataTable>();
                List<TacticStageValue> fltrTacticData = new List<TacticStageValue>();
                List<TacticMonthValue> TacticListMonth = new List<TacticMonthValue>();
                double Act_ProjQ1 = 0, Act_ProjQ2 = 0, Act_ProjQ3 = 0, Act_ProjQ4 = 0, GoalQ1 = 0, GoalQ2 = 0, GoalQ3 = 0, GoalQ4 = 0;
                #endregion

                #region "Evaluate Customfield Option wise Sparkline chart data"

                fltrTacticData = TacticData.Where(tac => _TacticOptionObject.planTacticList.Contains(tac.TacticObj.PlanTacticId)).ToList();
                #region "Get Actuals List"
                #endregion

                #endregion

                #region "Calculate Total for Proj.Vs Goal & Trend"
                List<ProjectedTacticModel> lstTotalTacticModel = new List<ProjectedTacticModel>();
                TacticListMonth = new List<TacticMonthValue>();
                TacticDataTable = GetTacticDataTablebyStageCode(_CustomfieldId, _TacticOptionObject.CustomFieldOptionid.ToString(), _CustomFieldType, resultCode, fltrTacticData, _IsTacticCustomField, true);
                TacticListMonth = GetMonthWiseValueList(TacticDataTable);
                lstTotalTacticModel = TacticListMonth.Select(tac => new ProjectedTacticModel
                {
                    TacticId = tac.Id,
                    StartMonth = tac.StartMonth,
                    EndMonth = tac.EndMonth,
                    Value = tac.Value,
                    Year = tac.StartYear
                }).Distinct().ToList();

                List<ProjectedTrendModel> lstTotalProjectedTrendModel = GetProjectedTrendModel(lstTotalTacticModel);
                lstTotalProjectedTrendModel = (from _prjTac in lstTotalProjectedTrendModel
                                               group _prjTac by new
                                               {
                                                   _prjTac.PlanTacticId,
                                                   _prjTac.Month,
                                                   _prjTac.Value,
                                                   _prjTac.TrendValue
                                               } into tac
                                               select new ProjectedTrendModel
                                               {
                                                   PlanTacticId = tac.Key.PlanTacticId,
                                                   Month = tac.Key.Month,
                                                   Value = tac.Key.Value,
                                                   TrendValue = tac.Key.TrendValue
                                               }).Distinct().ToList();

                if (IsQuarterly)
                {
                    #region "if timeframe Quarterly"
                    Act_ProjQ1 = Act_ProjQ2 = Act_ProjQ3 = Act_ProjQ4 = GoalQ1 = GoalQ2 = GoalQ3 = GoalQ4 = 0;

                    #region "Calculate Trend Quarterly"
                    #region "Newly added Code"
                    Act_ProjQ1 = ActualList.ToList()[0];
                    Act_ProjQ2 = ActualList.ToList()[1];
                    Act_ProjQ3 = ActualList.ToList()[2];
                    Act_ProjQ4 = ActualList.ToList()[3];

                    GoalQ1 = lstTotalProjectedTrendModel.Where(_proj => Q1.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ2 = lstTotalProjectedTrendModel.Where(_proj => Q2.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ3 = lstTotalProjectedTrendModel.Where(_proj => Q3.Contains(_proj.Month)).Sum(_proj => _proj.Value);
                    GoalQ4 = lstTotalProjectedTrendModel.Where(_proj => Q4.Contains(_proj.Month)).Sum(_proj => _proj.Value);

                    TotalTrendQ1 = GoalQ1 > 0 ? (((Act_ProjQ1 - GoalQ1) / GoalQ1) * 100) : 0; // Change By Nishant Sheth : #1424
                    TotalTrendQ2 = GoalQ2 > 0 ? (((Act_ProjQ2 - GoalQ2) / GoalQ2) * 100) : 0; // Change By Nishant Sheth : #1424
                    TotalTrendQ3 = GoalQ3 > 0 ? (((Act_ProjQ3 - GoalQ3) / GoalQ3) * 100) : 0; // Change By Nishant Sheth : #1424
                    TotalTrendQ4 = GoalQ4 > 0 ? (((Act_ProjQ4 - GoalQ4) / GoalQ4) * 100) : 0; // Change By Nishant Sheth : #1424
                    #endregion
                    #endregion

                    #region "Add Total Trend value to List"
                    PerformanceList.Add(TotalTrendQ1.ToString());
                    PerformanceList.Add(TotalTrendQ2.ToString());
                    PerformanceList.Add(TotalTrendQ3.ToString());
                    PerformanceList.Add(TotalTrendQ4.ToString());
                    #endregion
                    #endregion
                }
                else
                {
                    #region "Get Total Trend value on Monthly basis"
                    double _totalActual = 0, _totalTrend = 0, _TotalTrendValue = 0, _totalGoal = 0, _totalActual_Projected = 0;
                    string _curntPeriod = string.Empty;
                    for (int i = 1; i <= 12; i++)
                    {
                        _curntPeriod = PeriodPrefix.ToString() + i;
                        _totalActual = ActualList.ToList()[i - 1];
                        _totalTrend = lstTotalProjectedTrendModel.Where(_projTrend => _projTrend.Month.Equals(_curntPeriod)).Sum(_projTrend => _projTrend.TrendValue);
                        _totalActual_Projected = _totalActual;
                        _totalGoal = lstTotalProjectedTrendModel.Where(_proj => _proj.Month.Equals(_curntPeriod)).Sum(_proj => _proj.Value);
                        _TotalTrendValue = _totalGoal > 0 ? (((_totalActual_Projected - _totalGoal) / _totalGoal) * 100) : 0; // Change By Nishant Sheth : #1424
                        PerformanceList.Add(_TotalTrendValue.ToString());
                    }
                    #endregion
                }
                #endregion
                #endregion

                #region "Add all list to Master Model"
                objSubDataTableModel.PerformanceList = PerformanceList;
                objSubDataTableModel.RevenueList = RevenueList;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objSubDataTableModel;
        }
        #endregion

        /// <summary>
        /// This method will return the data of filter combine chart and Conversiondatatable result based on filter
        /// </summary>
        /// <param name= ParentLabel ,childlabelType , childId ,  option ,Quarterly, code> </param>
        /// <returns>Return json data of filtered combine chart and Conversiondatatable </returns>
        #region "Get combine chart and Conversiondatatable result based on filter -dashrath Prajapati"
        public ActionResult GetTopConversionToPlanByCustomFilter(string ParentLabel = "", string childlabelType = "", string childId = "", string option = "", string IsQuarterly = "Quarterly", string code = "", bool isDetails = false, string BackHeadTitle = "", bool IsBackClick = false, string DrpChange = "CampaignDrp", string marsterCustomField = "", int masterCustomFieldOptionId = 0)
        {
            #region "Declare Local Variables"
            List<TacticStageValue> TacticData = (List<TacticStageValue>)TempData["ReportData"];
            TempData["ReportData"] = TempData["ReportData"];
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<ActualTrendModel> ActualTacticTrendList = new List<ActualTrendModel>();
            string StageCode = code;
            List<string> ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add(StageCode);//
            bool IsTillCurrentMonth = true;
            List<string> includeMonth = GetMonthListForReport(option);
            List<ProjectedTrendModel> ProjectedTrendList = new List<ProjectedTrendModel>();
            lineChartData objLineChartData = new lineChartData();
            string strCampaign = Common.RevenueCampaign;//
            int customfieldId = 0;
            ReportModel objReportModel = new ReportModel();
            bool IsCampaignCustomField = false, IsProgramCustomField = false, IsTacticCustomField = false;
            List<TacticStageValue> _tacticdata = new List<TacticStageValue>();
            List<int> PlanTacticIdsList = new List<int>();
            // RevenueToPlanModel objRevenueToPlanModel = new RevenueToPlanModel();
            ConversionToPlanModel objConversionToPlanModel = new ConversionToPlanModel();
            int _customfieldOptionId = 0;
            List<RevenueContrinutionData> _TacticOptionList = new List<RevenueContrinutionData>();

            string customFieldType = string.Empty;
            // Add By Nishant Sheth
            List<Plan_Campaign_Program_Tactic> tacticlist = new List<Plan_Campaign_Program_Tactic>();
            List<int> campaignlist = new List<int>();
            List<int> programlist = new List<int>();
            List<TacticStageValue> Tacticdata = new List<TacticStageValue>();
            List<TacticMappingItem> _cmpgnMappingList = new List<TacticMappingItem>();
            List<Plan_Campaign_Program_Tactic> _lstTactic = new List<Plan_Campaign_Program_Tactic>();
            CardSectionModel objCardSectionModel = new CardSectionModel();
            List<CardSectionListModel> CardSectionListModel = new List<CardSectionListModel>();


            tacticlist = GetTacticForReporting();
            Tacticdata = Common.GetTacticStageRelation(tacticlist, IsReport: true);
            // End By Nishant Sheth
            #endregion
            /// Declarion For Card Section 
            /// Nishant Sheth
            /// 
            ViewBag.ConvParentLabel = ParentLabel;
            ViewBag.ConvchildId = childId;
            ViewBag.Convoption = option;
            List<ProjectedTrendModel> MqlProjected = new List<ProjectedTrendModel>();// Header projected
            List<ActualTrendModel> MqlActual = new List<ActualTrendModel>();
            #region TempData for Back Button

            if (DrpChange != "CampaignDrp")
            {
                #region TempData for Back Button
                TempData["ConvBackParentLabel"] = Common.RevenueCampaign;
                TempData["ConvBackChildLabel"] = "";
                TempData["ConvBackId"] = "";
                TempData["ConvIsBack"] = "";
                TempData["ConvHeadHireachy"] = "Waterfall";
                TempData["ConvHeadTitle"] = "Waterfall";
                TempData["ConvBackWithCustom"] = Common.RevenueCampaign;

                TempData["ConvHeadHireachy"] = string.Empty;
                TempData["ConvBackParentHireachy"] = string.Empty;
                TempData["ConvBackChildHireachy"] = string.Empty;
                TempData["ConvBackHeadTitle"] = string.Empty;
                TempData["ConvBackIdHireachy"] = string.Empty;
                TempData["ConvBackDummyId"] = string.Empty;

                #endregion

            }

            TempData["ConvBackWithCustom"] = ParentLabel;

            string HeadHireachy = TempData["ConvHeadHireachy"] as string;
            string BackParentHireachy = TempData["ConvBackParentHireachy"] as string;
            string BackChildHireachy = TempData["ConvBackChildHireachy"] as string;
            string BackTitleHireachy = TempData["ConvBackHeadTitle"] as string;
            string BackIdHireachy = TempData["ConvBackIdHireachy"] as string;
            string BackDummyId = TempData["ConvBackDummyId"] as string;
            string[] BackParentArray = { };
            string[] BackChildArray = { };
            string[] BackIdArray = { };
            string[] BackDummyArray = { };

            if (DrpChange != "CampaignDrp" || isDetails)
            {
                HeadHireachy = HeadHireachy + "," + HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(BackHeadTitle));
                BackParentHireachy = BackParentHireachy + "," + ParentLabel;
                BackChildHireachy = BackChildHireachy + "," + childlabelType;
            }

            string TempDummyid = ((!string.IsNullOrEmpty(ParentLabel) ? ParentLabel.Substring(0, 3) : "Parent") + "-" + (!string.IsNullOrEmpty(childlabelType) ? childlabelType.Substring(0, 3) : "Child") + childId);

            BackDummyId = BackDummyId + "," + TempDummyid;

            BackDummyArray = BackDummyId.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            List<string> dummyids = BackDummyArray.ToList<string>();

            var readdummy = from n in dummyids where n == TempDummyid select new { n };
            var x = dummyids.GroupBy(s => s).Select(s => new { id = s.Key, count = s.Key.Count() }).ToList().Where(s => s.count == 1 && s.id == TempDummyid).Select(s => s.id).ToList();


            if ((!IsBackClick && isDetails) || DrpChange == "true")
            {
                BackIdHireachy = BackIdHireachy + "," + childId;
            }

            BackDummyArray = dummyids.Distinct().ToArray();

            TempData["ConvBackDummyId"] = BackDummyArray;

            TempData["ConvHeadHireachy"] = HeadHireachy;
            TempData["ConvBackParentHireachy"] = BackParentHireachy;
            TempData["ConvBackChildHireachy"] = BackChildHireachy;
            TempData["ConvBackIdHireachy"] = BackIdHireachy;

            string[] HeadHireachyArray = HeadHireachy.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            BackParentArray = BackParentHireachy.Split(',').Distinct().ToArray().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            BackChildArray = BackChildHireachy.Split(',').Distinct().ToArray().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            //if (isDetails)
            BackIdArray = BackIdHireachy.Split(',').ToArray().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            if (BackParentArray.Count() > 1)
            {
                if (BackIdArray.Count() == 4)
                {
                    TempData["ConvBackParentLabel"] = Convert.ToString(BackParentArray[BackParentArray.Count() - 1]);
                }
                else
                {
                    if (BackParentArray[0].Contains(Common.CampaignCustomTitle))
                    {
                    TempData["ConvBackParentLabel"] = Convert.ToString(BackParentArray[BackParentArray.Count() - 2]);
                        if (BackIdArray.Count() == 3 && childlabelType == Common.RevenueProgram)
                        {
                            TempData["ConvBackParentLabel"] = Convert.ToString(BackParentArray[BackParentArray.Count() - 1]);
                        }
                    }
                    else
                    {
                        TempData["ConvBackParentLabel"] = Convert.ToString(BackParentArray[BackParentArray.Count() - 2]);
                    }
                }
            }
            else
            {
                TempData["ConvBackParentLabel"] = (BackParentArray.Count() > 0) ? Convert.ToString(BackParentArray[0]) : string.Empty;
            }

            if (BackChildArray.Count() > 1)
            {
                TempData["ConvBackChildLabel"] = Convert.ToString(BackChildArray[BackChildArray.Count() - 2]);
            }
            else
            {
                TempData["ConvBackChildLabel"] = string.Empty;
            }

            if (BackIdArray.Count() > 1)
            {
                TempData["ConvBackId"] = Convert.ToString(BackIdArray[BackIdArray.Count() - 2]);
                if (BackIdArray.Count() == 2 && BackParentArray[0].Contains(Common.ProgramCustomTitle) && ParentLabel == Common.RevenueCampaign && childlabelType == Common.RevenueProgram)
                {
                    TempData["ConvIsDispalyCustomFieldChildDDL"] = true;
                }
            }
            else
            {
                TempData["ConvBackId"] = "0";
            }

            if (HeadHireachyArray.Count() == 0)
            {
                TempData["ConvHeadTitle"] = "Waterfall";
            }
            else
            {
                TempData["ConvHeadTitle"] = Convert.ToString(HeadHireachyArray[HeadHireachyArray.Count() - 1]);
            }


            if (IsBackClick)
            {
                if (marsterCustomField.Contains(Common.CampaignCustomTitle))
                {
                    if (BackIdArray.Count() == 3 && childlabelType == Common.RevenueCampaign)
                    {
                        TempData["ConvIsDispalyCustomFieldChildDDL"] = true;
                    }
                }
                marsterCustomField = string.Empty;
                BackParentArray = BackParentHireachy.Split(',').Distinct().ToArray().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                BackChildArray = BackChildHireachy.Split(',').Distinct().ToArray().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                BackIdArray = BackIdHireachy.Split(',').ToArray().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                if (BackDummyArray.Count() > 0)
                {
                    List<string> y = BackDummyArray.ToList<string>();
                    y.RemoveAt(BackDummyArray.Count() - 1);
                    BackDummyArray = y.ToArray();
                    TempData["ConvBackDummyId"] = string.Join(",", BackDummyArray);
                }

                if (BackParentArray.Count() > 1)
                {
                    List<string> y = BackParentArray.ToList<string>();
                    y.RemoveAt(BackParentArray.Count() - 1);
                    BackParentArray = y.ToArray();
                    TempData["ConvBackParentHireachy"] = string.Join(",", BackParentArray);
                }
                if (BackChildArray.Count() > 0)
                {
                    List<string> y = BackChildArray.ToList<string>();
                    y.RemoveAt(BackChildArray.Count() - 1);
                    BackChildArray = y.ToArray();
                    TempData["ConvBackChildHireachy"] = string.Join(",", BackChildArray);
                }
                if (BackIdArray.Count() > 0)
                {
                    List<string> y = BackIdArray.ToList<string>();
                    //if (BackChildArray[0] != Common.RevenueCampaign && BackChildArray.Count() != 1)
                    {
                        y.RemoveAt(BackIdArray.Count() - 1);
                    }
                    BackIdArray = y.ToArray();
                    TempData["ConvBackIdHireachy"] = string.Join(",", BackIdArray);
                }

                if (BackParentArray.Count() > 1)
                {
                    TempData["ConvBackParentLabel"] = Convert.ToString(BackParentArray[BackParentArray.Count() - 1]);
                }
                else
                {
                    TempData["ConvBackParentLabel"] = Convert.ToString(BackParentArray[0]);
                }

                if (BackChildArray.Count() > 1)
                {
                    TempData["ConvBackChildLabel"] = Convert.ToString(BackChildArray[BackChildArray.Count() - 1]);
                }
                else
                {
                    TempData["ConvBackChildLabel"] = string.Empty;
                }

                if (BackIdArray.Count() > 1)
                {
                    TempData["ConvBackId"] = Convert.ToString(BackIdArray[BackIdArray.Count() - 2]);
                }
                else
                {
                    TempData["ConvBackId"] = "0";
                }

                if (HeadHireachyArray.Count() > 0)
                {
                    List<string> y = HeadHireachyArray.ToList<string>();
                    y.RemoveAt(HeadHireachyArray.Count() - 1);
                    HeadHireachyArray = y.ToArray();
                    TempData["ConvHeadHireachy"] = string.Join(",", HeadHireachyArray);
                    if (HeadHireachyArray.Count() == 0)
                    {
                        TempData["ConvHeadTitle"] = "Waterfall";
                    }
                    else
                    {
                        TempData["ConvHeadTitle"] = Convert.ToString(HeadHireachyArray[HeadHireachyArray.Count() - 1]);
                    }
                }
            }

            #endregion

            string customFieldOptionIdCardSection = string.Empty;
            int customFieldIdCardSection = 0;
            bool isTacticCustomFieldCardSection = false;
            string customFieldTypeCardSection = string.Empty;
            if (masterCustomFieldOptionId > 0)
            {
                if (marsterCustomField.Contains(Common.CampaignCustomTitle))
                {
                    int mastercustomfieldIdInner = Convert.ToInt32(marsterCustomField.Replace(Common.CampaignCustomTitle, ""));
                    List<int> campaignIds = new List<int>();
                    campaignIds = TacticData.Select(p => p.TacticObj.Plan_Campaign_Program.PlanCampaignId).Distinct().ToList();
                    string customfiledvalue = Convert.ToString(masterCustomFieldOptionId);
                    campaignIds = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && campaignIds.Contains(c.EntityId) && c.Value == customfiledvalue).Select(c => c.EntityId).ToList();
                    TacticData = TacticData.Where(t => campaignIds.Contains(t.TacticObj.Plan_Campaign_Program.PlanCampaignId)).ToList();
                }
                else if (marsterCustomField.Contains(Common.ProgramCustomTitle))
                {
                    int mastercustomfieldIdInner = Convert.ToInt32(marsterCustomField.Replace(Common.ProgramCustomTitle, ""));
                    List<int> programIds = new List<int>();
                    programIds = TacticData.Select(p => p.TacticObj.PlanProgramId).Distinct().ToList();
                    string customfiledvalue = Convert.ToString(masterCustomFieldOptionId);
                    programIds = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && programIds.Contains(c.EntityId) && c.Value == customfiledvalue).Select(c => c.EntityId).ToList();
                    TacticData = TacticData.Where(t => programIds.Contains(t.TacticObj.PlanProgramId)).ToList();
                }
                else if (marsterCustomField.Contains(Common.TacticCustomTitle))
                {
                    int mastercustomfieldIdInner = Convert.ToInt32(marsterCustomField.Replace(Common.TacticCustomTitle, ""));
                    customFieldIdCardSection = mastercustomfieldIdInner;
                    isTacticCustomFieldCardSection = true;
                    customFieldTypeCardSection = db.CustomFields.Where(c => c.CustomFieldId == mastercustomfieldIdInner).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                    List<int> tacticIds = new List<int>();
                    tacticIds = TacticData.Select(p => p.TacticObj.PlanTacticId).Distinct().ToList();
                    string customfiledvalue = Convert.ToString(masterCustomFieldOptionId);
                    customFieldOptionIdCardSection = customfiledvalue;
                    tacticIds = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && tacticIds.Contains(c.EntityId) && c.Value == customfiledvalue).Select(c => c.EntityId).ToList();
                    TacticData = TacticData.Where(t => tacticIds.Contains(t.TacticObj.PlanTacticId)).ToList();
                }
            }

            try
            {
                //PlanTacticIdsList
                if (ParentLabel.Equals(strCampaign))
                {
                    if (childlabelType == Common.RevenueCampaign)
                    {
                        int campaignid = Convert.ToInt32(childId);
                        _tacticdata = TacticData.Where(pcpt => pcpt.TacticObj.Plan_Campaign_Program.PlanCampaignId == campaignid).Select(t => t).ToList();
                    }
                    else if (childlabelType == Common.RevenueProgram)
                    {
                        int programid = Convert.ToInt32(childId);
                        _tacticdata = TacticData.Where(pcpt => pcpt.TacticObj.PlanProgramId == programid).Select(t => t).ToList();
                    }
                    else if (childlabelType == Common.RevenueTactic)
                    {
                        int tacticid = Convert.ToInt32(childId);
                        _tacticdata = TacticData.Where(pcpt => pcpt.TacticObj.PlanTacticId == tacticid).Select(t => t).ToList();
                    }
                    else
                    {
                        _tacticdata = TacticData.ToList();
                    }

                    ActualTacticStageList = GetActualListInTacticInterval(_tacticdata, option, ActualStageCodeList, IsTillCurrentMonth);
                    ActualTacticTrendList = GetActualTrendModelForRevenueOverview(_tacticdata, ActualTacticStageList);

                    List<string> MqlStageList = new List<string>();
                    MqlStageList.Add(Enums.Stage.MQL.ToString());

                    List<ActualTacticListByStage> MqlTacticStageLsit = GetActualListInTacticInterval(_tacticdata, option, MqlStageList, IsTillCurrentMonth);

                    MqlActual = GetActualTrendModelForRevenueOverview(_tacticdata, MqlTacticStageLsit);

                    #region "Conversion : Get Tacticwise Actual_Projected Vs Goal Model data "
                    ProjectedTrendList = CalculateProjectedTrend(_tacticdata, includeMonth, StageCode);
                    if (StageCode != Enums.Stage.MQL.ToString())
                    {
                        MqlProjected = CalculateProjectedTrend(_tacticdata, includeMonth, Enums.Stage.MQL.ToString());
                    }
                    else
                    {
                        MqlProjected = ProjectedTrendList;
                    }
                    #endregion

                    #region Mapping Items for Card Section
                    /// Add By Nishant Sheth
                   
                    // Fetch the respectives Campaign Ids and Program Ids from the tactic list
                    campaignlist = tacticlist.Select(t => t.Plan_Campaign_Program.PlanCampaignId).ToList();
                    programlist = tacticlist.Select(t => t.PlanProgramId).ToList();
                   

                    if (childlabelType.Contains(Common.RevenueTactic))
                    {
                        // if (DrpChange != "CampaignDrp" || isDetails || IsBackClick)
                        {
                            ViewBag.ConvchildlabelType = Common.RevenueTactic;
                        }

                        _lstTactic = tacticlist.ToList();

                        if (!string.IsNullOrEmpty(childId) ? Convert.ToInt32(childId) > 0 : false)
                        {
                            _lstTactic = _lstTactic.Where(t => t.PlanTacticId == (Convert.ToInt32(childId) > 0 ? Convert.ToInt32(childId) : t.PlanTacticId))
                                .ToList();
                        }

                        _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _parentId = pc.PlanTacticId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Title })
                            .Select(pct => new TacticMappingItem { ParentId = pct.Key._parentId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
                    }
                    else if (childlabelType.Contains(Common.RevenueProgram))
                    {
                        //if (DrpChange != "CampaignDrp" || isDetails || IsBackClick)
                        {
                            ViewBag.ConvchildlabelType = Common.RevenueTactic;
                        }

                        _lstTactic = tacticlist.ToList();
                        //if (!string.IsNullOrEmpty(childId) ? Convert.ToInt32(childId) > 0 : false)
                        {
                            _lstTactic = _lstTactic.Where(t => t.PlanProgramId == (Convert.ToInt32(childId) > 0 ? Convert.ToInt32(childId) : t.PlanProgramId))
                                .ToList();
                        }
                        _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _parentId = pc.PlanTacticId, _childId = pc.PlanTacticId, _parentTitle = pc.Title })
                            .Select(pct => new TacticMappingItem { ParentId = pct.Key._parentId, ChildId = pct.Key._childId, ParentTitle = pct.Key._parentTitle }).ToList();
                    }
                    else if (childlabelType.Contains(Common.RevenueCampaign))
                    {
                        // if (DrpChange != "CampaignDrp" || isDetails || IsBackClick)
                        {
                            ViewBag.ConvchildlabelType = Common.RevenueProgram;
                        }
                        _lstTactic = tacticlist.ToList();

                        if (!string.IsNullOrEmpty(childId) ? Convert.ToInt32(childId) > 0 : false)
                        {
                            _lstTactic = _lstTactic.Where(t => t.Plan_Campaign_Program.PlanCampaignId == (Convert.ToInt32(childId) > 0 ? Convert.ToInt32(childId) : t.Plan_Campaign_Program.PlanCampaignId))
                                .ToList();
                        }

                        _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _parentId = pc.PlanProgramId, _childId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Title })
                            .Select(pct => new TacticMappingItem { ParentId = pct.Key._parentId, ChildId = pct.Key._childId, ParentTitle = pct.Key._parentTitle }).ToList();
                    }
                    else
                    {
                        //if (DrpChange != "CampaignDrp" || isDetails || IsBackClick)
                        {
                            ViewBag.ConvchildlabelType = Common.RevenueCampaign;
                        }
                        _lstTactic = tacticlist.ToList();
                        _cmpgnMappingList = _lstTactic.GroupBy(pc => new { _campaignId = pc.Plan_Campaign_Program.PlanCampaignId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title }).Select(pct => new TacticMappingItem { ParentId = pct.Key._campaignId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();
                    }
                    // End By Nishant Sheth
                    #endregion
                }
                else if (ParentLabel.Contains(Common.TacticCustomTitle) || ParentLabel.Contains(Common.CampaignCustomTitle) || ParentLabel.Contains(Common.ProgramCustomTitle))
                {

                    _customfieldOptionId = !string.IsNullOrEmpty(childId) ? int.Parse(childId) : 0;
                    if (ParentLabel.Contains(Common.TacticCustomTitle))
                    {
                        customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.TacticCustomTitle, ""));
                        IsTacticCustomField = true;
                        if (Convert.ToInt32(childId) > 0)
                        {
                            ViewBag.ConvParentLabel = Common.RevenueCampaign;// Add By Nishant Sheth
                            ViewBag.ConvchildlabelType = Common.RevenueTactic;
                        }
                    }
                    else if (ParentLabel.Contains(Common.CampaignCustomTitle))
                    {
                        customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.CampaignCustomTitle, ""));
                        IsCampaignCustomField = true;
                        if (Convert.ToInt32(childId) > 0)
                        {
                            ViewBag.ConvParentLabel = Common.RevenueCampaign;// Add By Nishant Sheth
                            ViewBag.ConvchildlabelType = Common.RevenueCampaign;
                        }
                    }
                    else if (ParentLabel.Contains(Common.ProgramCustomTitle))
                    {
                        customfieldId = Convert.ToInt32(ParentLabel.Replace(Common.ProgramCustomTitle, ""));
                        IsProgramCustomField = true;
                        if (Convert.ToInt32(childId) > 0)
                        {
                            ViewBag.ConvParentLabel = Common.RevenueCampaign;// Add By Nishant Sheth
                            ViewBag.ConvchildlabelType = Common.RevenueProgram;
                        }
                    }
                    else
                    {
                        TacticData = TacticData.ToList();
                    }

                    #region "New Code"
                    List<int> entityids = new List<int>();
                    if (IsTacticCustomField)
                    {
                        entityids = TacticData.Select(t => t.TacticObj.PlanTacticId).ToList();
                    }
                    else if (IsCampaignCustomField)
                    {
                        entityids = TacticData.Select(t => t.TacticObj.Plan_Campaign_Program.PlanCampaignId).ToList();
                    }
                    else
                    {
                        entityids = TacticData.Select(t => t.TacticObj.PlanProgramId).ToList();
                    }

                    customFieldType = db.CustomFields.Where(c => c.CustomFieldId == customfieldId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                    var cusomfieldEntity = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && entityids.Contains(c.EntityId)).ToList();
                    if (customFieldType == Enums.CustomFieldType.DropDownList.ToString())
                    {
                        var optionlist = cusomfieldEntity.Select(c => Convert.ToInt32(c.Value)).ToList();
                        _TacticOptionList = (from cfo in db.CustomFieldOptions
                                             where cfo.CustomFieldId == customfieldId && optionlist.Contains(cfo.CustomFieldOptionId) && cfo.IsDeleted == false
                                             select cfo).ToList().GroupBy(pc => new { id = pc.CustomFieldOptionId, title = pc.Value }).Select(pc =>
                                      new RevenueContrinutionData
                                      {
                                          Title = pc.Key.title,
                                          CustomFieldOptionid = pc.Key.id,
                                          // Fetch the filtered list based upon custom fields type
                                          planTacticList = TacticData.Where(t => cusomfieldEntity.Where(c => c.Value == pc.Key.id.ToString()).Select(c => c.EntityId).ToList().Contains(IsCampaignCustomField ? t.TacticObj.Plan_Campaign_Program.PlanCampaignId :
                                              (IsProgramCustomField ? t.TacticObj.PlanProgramId : t.TacticObj.PlanTacticId))).Select(t => t.TacticObj.PlanTacticId).ToList()
                                      }).ToList();
                    }
                    else if (customFieldType == Enums.CustomFieldType.TextBox.ToString())
                    {
                        _TacticOptionList = cusomfieldEntity.GroupBy(pc => new { title = pc.Value }).Select(pc =>
                                    new RevenueContrinutionData
                                    {
                                        Title = pc.Key.title,
                                        planTacticList = pc.Select(c => c.EntityId).ToList()
                                    }).ToList();
                    }
                    #endregion
                    if (_customfieldOptionId > 0)
                    {
                        PlanTacticIdsList = _TacticOptionList.Where(rev => rev.CustomFieldOptionid.Equals(_customfieldOptionId)).Select(rev => rev.planTacticList).FirstOrDefault();
                    }
                    else
                    {
                        _TacticOptionList.ForEach(rev => PlanTacticIdsList.AddRange(rev.planTacticList));
                    }
                    PlanTacticIdsList = PlanTacticIdsList != null ? PlanTacticIdsList.Distinct().ToList() : new List<int>();
                    #region "filter TacticData based on Customfield"

                    _tacticdata = TacticData.Where(t => PlanTacticIdsList.Contains(t.TacticObj.PlanTacticId)).ToList();

                    #endregion

                    #region Add CampaginList For CardSection Base on CustomFieldOption
                    // Add BY Nishant Sheth
                    if (ParentLabel.Contains(Common.TacticCustomTitle) || ParentLabel.Contains(Common.CampaignCustomTitle) || ParentLabel.Contains(Common.ProgramCustomTitle))
                    {
                        if (_customfieldOptionId > 0)
                        {
                            tacticlist = _tacticdata.Select(t => t.TacticObj).ToList();
                            if (ParentLabel.Contains(Common.TacticCustomTitle))
                            {
                                _cmpgnMappingList = tacticlist.GroupBy(pc => new { _parentId = pc.PlanTacticId, _tacticId = pc.PlanTacticId, _parentTitle = pc.Title })
                                 .Select(pct => new TacticMappingItem { ParentId = pct.Key._parentId, ChildId = pct.Key._tacticId, ParentTitle = pct.Key._parentTitle }).ToList();

                            }
                            else if (ParentLabel.Contains(Common.ProgramCustomTitle))
                            {

                                _cmpgnMappingList = tacticlist.GroupBy(pc => new { _parentId = pc.PlanProgramId, _childId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Title })
                                    .Select(pct => new TacticMappingItem { ParentId = pct.Key._parentId, ChildId = pct.Key._childId, ParentTitle = pct.Key._parentTitle }).ToList();

                            }
                            else if (ParentLabel.Contains(Common.CampaignCustomTitle))
                            {

                                _cmpgnMappingList = tacticlist.GroupBy(pc => new { _parentId = pc.Plan_Campaign_Program.PlanCampaignId, _childId = pc.PlanTacticId, _parentTitle = pc.Plan_Campaign_Program.Plan_Campaign.Title })
                                    .Select(pct => new TacticMappingItem { ParentId = pct.Key._parentId, ChildId = pct.Key._childId, ParentTitle = pct.Key._parentTitle }).ToList();

                            }
                        }
                        else
                        {
                            foreach (var data in _TacticOptionList)
                            {
                                if (data.planTacticList.Count > 0)
                                {
                                    data.planTacticList.ForEach(innerdata => _cmpgnMappingList.Add(new TacticMappingItem { ParentTitle = data.Title, ParentId = data.CustomFieldOptionid, ChildId = innerdata }));
                                }
                                else
                                {
                                    _cmpgnMappingList.Add(new TacticMappingItem { ParentTitle = data.Title, ParentId = data.CustomFieldOptionid, ChildId = 0 });
                                }
                            }
                        }

                    }
                    #endregion

                    #region "Get ActualTrend Model list"
                    List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                    ActualTacticStageList = GetActualListInTacticInterval(_tacticdata, option, ActualStageCodeList, IsTillCurrentMonth);//
                    ActualTacticList = ActualTacticStageList.Where(actual => actual.StageCode.Equals(StageCode)).Select(actual => actual.ActualTacticList).FirstOrDefault();
                    //set code as per selection of dropdown
                    if (_customfieldOptionId > 0)
                    {
                        List<ActualDataTable> ActualRevenueDataTable = new List<ActualDataTable>();
                        if (code.Equals(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()))
                        {
                            ActualRevenueDataTable = GetActualTacticDataTablebyStageCode(customfieldId, _customfieldOptionId.ToString(), customFieldType, Enums.InspectStage.MQL, ActualTacticList, _tacticdata, IsTacticCustomField);
                        }
                        if (code.Equals(Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString()))
                        {
                            ActualRevenueDataTable = GetActualTacticDataTablebyStageCode(customfieldId, _customfieldOptionId.ToString(), customFieldType, Enums.InspectStage.CW, ActualTacticList, _tacticdata, IsTacticCustomField);
                        }
                        if (code.Equals(Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString()))
                        {
                            ActualRevenueDataTable = GetActualTacticDataTablebyStageCode(customfieldId, _customfieldOptionId.ToString(), customFieldType, Enums.InspectStage.INQ, ActualTacticList, _tacticdata, IsTacticCustomField);
                        }
                        //ProjectedStageValue
                        ActualTacticTrendList = GetActualTrendModelForRevenue(_tacticdata, ActualRevenueDataTable, StageCode);
                        if (StageCode != Enums.Stage.MQL.ToString())
                        {
                            MqlActual = GetActualTrendModelForRevenue(_tacticdata, ActualRevenueDataTable, Enums.Stage.MQL.ToString());
                        }
                        else
                        {
                            MqlActual = ActualTacticTrendList;
                        }
                    }
                    else
                    {
                        ActualTacticTrendList = GetActualTrendModelForRevenueOverview(_tacticdata, ActualTacticStageList);
                        MqlActual = GetActualTrendModelForRevenueOverview(_tacticdata, ActualTacticStageList).Where(act => act.StageCode.Equals(Enums.Stage.MQL)).ToList();
                    }
                    #endregion
                    //cmplete
                    if (_customfieldOptionId > 0)
                    {
                        #region "Get Tactic data by Weightage for Projected by StageCode(Revenue)"
                        List<TacticDataTable> _TacticDataTable = new List<TacticDataTable>();
                        List<TacticMonthValue> _TacticListMonth = new List<TacticMonthValue>();
                        List<ProjectedTacticModel> _TacticList = new List<ProjectedTacticModel>();
                        if (code.Equals(Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()))
                        {
                            _TacticDataTable = GetTacticDataTablebyStageCode(customfieldId, _customfieldOptionId.ToString(), customFieldType, Enums.InspectStage.MQL, _tacticdata, IsTacticCustomField, true);
                        }
                        if (code.Equals(Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString()))
                        {
                            _TacticDataTable = GetTacticDataTablebyStageCode(customfieldId, _customfieldOptionId.ToString(), customFieldType, Enums.InspectStage.CW, _tacticdata, IsTacticCustomField, true);
                        }
                        if (code.Equals(Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString()))
                        {
                            _TacticDataTable = GetTacticDataTablebyStageCode(customfieldId, _customfieldOptionId.ToString(), customFieldType, Enums.InspectStage.INQ, _tacticdata, IsTacticCustomField, true);
                        }
                        _TacticListMonth = GetMonthWiseValueList(_TacticDataTable);
                        _TacticList = _TacticListMonth.Select(tac => new ProjectedTacticModel
                        {
                            TacticId = tac.Id,
                            StartMonth = tac.StartMonth,
                            EndMonth = tac.EndMonth,
                            Value = tac.Value,
                            Year = tac.StartYear
                        }).Distinct().ToList();
                        ProjectedTrendList = GetProjectedTrendModel(_TacticList);
                        ProjectedTrendList = (from _prjTac in ProjectedTrendList
                                              group _prjTac by new
                                              {
                                                  _prjTac.PlanTacticId,
                                                  _prjTac.Month,
                                                  _prjTac.Value,
                                                  _prjTac.TrendValue
                                              } into tac
                                              select new ProjectedTrendModel
                                              {
                                                  PlanTacticId = tac.Key.PlanTacticId,
                                                  Month = tac.Key.Month,
                                                  Value = tac.Key.Value,
                                                  TrendValue = tac.Key.TrendValue
                                              }).Distinct().ToList();
                        #endregion

                        // Mql Projected for header value 
                        _TacticDataTable = GetTacticDataTablebyStageCode(customfieldId, _customfieldOptionId.ToString(), customFieldType, Enums.InspectStage.MQL, _tacticdata, IsTacticCustomField, true);
                        _TacticListMonth = GetMonthWiseValueList(_TacticDataTable);
                        _TacticList = _TacticListMonth.Select(tac => new ProjectedTacticModel
                        {
                            TacticId = tac.Id,
                            StartMonth = tac.StartMonth,
                            EndMonth = tac.EndMonth,
                            Value = tac.Value,
                            Year = tac.StartYear
                        }).Distinct().ToList();
                        MqlProjected = GetProjectedTrendModel(_TacticList);
                        MqlProjected = (from _prjTac in ProjectedTrendList
                                        group _prjTac by new
                                        {
                                            _prjTac.PlanTacticId,
                                            _prjTac.Month,
                                            _prjTac.Value,
                                            _prjTac.TrendValue
                                        } into tac
                                        select new ProjectedTrendModel
                                        {
                                            PlanTacticId = tac.Key.PlanTacticId,
                                            Month = tac.Key.Month,
                                            Value = tac.Key.Value,
                                            TrendValue = tac.Key.TrendValue
                                        }).Distinct().ToList();

                    }
                    else
                    {
                        ProjectedTrendList = CalculateProjectedTrend(_tacticdata, includeMonth, StageCode);
                        if (StageCode != Enums.Stage.MQL.ToString())
                        {
                            MqlProjected = CalculateProjectedTrend(_tacticdata, includeMonth, Enums.Stage.MQL.ToString());
                        }
                    }
                }

                #region Set Header Value
                BasicModel objBasicConverstionHeader = new BasicModel();
                //List<ActualTrendModel> MqlActual = ActualTacticTrendList.Where(act => act.StageCode.Equals(Enums.Stage.MQL)).ToList();

                objBasicConverstionHeader = GetValuesListByTimeFrame(MqlActual, MqlProjected, option, (IsQuarterly.ToLower() == "quarterly" ? true : false));
                Projected_Goal objHeaderProjected = new Projected_Goal();
                objHeaderProjected = GetConverstionHeaderValue(objBasicConverstionHeader, option).RevenueHeaderModel;
                objConversionToPlanModel.RevenueHeaderModel = objHeaderProjected;
                #endregion

                /// Add By Nishant Sheth : 07-July-2015 
                /// Desc : Fill card section with filter option , Ticket no:#1397 
                objCardSectionModel = new CardSectionModel();
                CardSectionListModel = new List<CardSectionListModel>();

              


                CardSectionListModel = GetConversionCardSectionList(_tacticdata,_cmpgnMappingList, option, (IsQuarterly.ToLower() == "quarterly" ? true : false), ParentLabel, false, "", 0);
                //CardSectionListModel = GetCardSectionDefaultData(_tacticdata, ActualTacticTrendList, ProjectedTrendList, OverviewModelList, _cmpgnMappingList.ToList(), option, (IsQuarterly.ToLower() == "quarterly" ? true : false), "", "", IsTacticCustomField, customFieldType, customfieldId);
                objCardSectionModel.CardSectionListModel = CardSectionListModel;
                TempData["ConverstionCardList"] = null;
                TempData["ConverstionCardList"] = CardSectionListModel;// For Pagination Sorting and searching
                objCardSectionModel = ConverstionCardSectionModelWithFilter(0, 5, "", Enums.SortByWaterFall.INQ.ToString());
                objCardSectionModel.TotalRecords = CardSectionListModel.Count();
                TempData["ConversionCard"] = null;
                TempData["ConversionCard"] = objCardSectionModel;

                // End By Nishant Sheth

                #region "Revenue Model Values"

                #region "Get Basic Model"
                bool _isquarterly = false;
                if (!string.IsNullOrEmpty(IsQuarterly) && IsQuarterly.Equals(Enums.ViewByAllocated.Quarterly.ToString()))
                    _isquarterly = true;
                BasicModel objBasicModel = GetValuesListByTimeFrame(ActualTacticTrendList, ProjectedTrendList, option, _isquarterly);
                #endregion

                #region "Set Linechart & Revenue Overview data to model for combinechart"
                objLineChartData = GetCombinationLineChartData(objBasicModel);
                objConversionToPlanModel.LineChartModel = objLineChartData != null ? objLineChartData : new lineChartData();
                #endregion

                #region "Set line chart data"

                List<conversion_Projected_Goal_LineChart> Projected_Goal_LineChartList = new List<conversion_Projected_Goal_LineChart>();
                conversion_Projected_Goal_LineChart objProjected_Goal_LineChart = new conversion_Projected_Goal_LineChart();

                objLineChartData = new lineChartData();
                ConversionOverviewModel objConversionOverviewModel = new ConversionOverviewModel();
                objLineChartData = GetLineChartData(objBasicModel);
                string strINQtageCode = Enums.InspectStage.ProjectedStageValue.ToString();
                objProjected_Goal_LineChart = new conversion_Projected_Goal_LineChart();
                objProjected_Goal_LineChart.StageCode = strINQtageCode;
                objProjected_Goal_LineChart.linechartdata = objLineChartData != null ? objLineChartData : new lineChartData();
                Projected_Goal_LineChartList.Add(objProjected_Goal_LineChart);
                objConversionOverviewModel.Projected_LineChartList = Projected_Goal_LineChartList != null ? Projected_Goal_LineChartList : (new List<conversion_Projected_Goal_LineChart>());
                objConversionToPlanModel.conversionOverviewModel = objConversionOverviewModel;
                objReportModel.conversionOverviewModel = objConversionOverviewModel;
                #endregion
                #endregion

                #region "Calculate DataTable"
                List<string> _Categories = new List<string>();
                _Categories = objBasicModel.Categories;

                ConversionDataTable objConversionDataTable = new ConversionDataTable();
                ConversionSubDataTableModel objSubDataModel = new ConversionSubDataTableModel();
                objConversionDataTable.Categories = _Categories;
                objConversionDataTable.ActualList = objBasicModel.ActualList;
                objConversionDataTable.ProjectedList = objBasicModel.ProjectedList;
                objConversionDataTable.GoalList = objBasicModel.GoalList;

                //if ParentLabel is "Campaign" or ParentLabel is "customfield" and CustomfieldOptionId selected "All" then do all calculation without weightage apply.
                if (ParentLabel.Equals(strCampaign) || ((ParentLabel.Contains(Common.TacticCustomTitle) || ParentLabel.Contains(Common.CampaignCustomTitle) || ParentLabel.Contains(Common.ProgramCustomTitle)) && _customfieldOptionId.Equals(0)))
                {
                    objSubDataModel = GetConversionToPlanDataByCampaign(_tacticdata, objBasicModel.timeframeOption, objBasicModel.IsQuarterly, StageCode, objBasicModel);
                }
                else
                {
                    RevenueContrinutionData _TacticOptionModel = new RevenueContrinutionData();
                    _TacticOptionModel = _TacticOptionList.Where(tac => tac.CustomFieldOptionid.Equals(_customfieldOptionId)).FirstOrDefault();
                    _TacticOptionModel = _TacticOptionModel != null ? _TacticOptionModel : new RevenueContrinutionData();
                    objSubDataModel = GetConversionToPlanDataByCustomField(customfieldId, customFieldType, _tacticdata, _TacticOptionModel, objBasicModel.timeframeOption, IsTacticCustomField, objBasicModel.IsQuarterly, StageCode, objBasicModel);
                }
                objConversionDataTable.SubDataModel = objSubDataModel;
                objConversionDataTable.IsQuarterly = objBasicModel.IsQuarterly;
                objConversionDataTable.timeframeOption = objBasicModel.timeframeOption;
                objConversionToPlanModel.ConversionToPlanDataTableModel = objConversionDataTable;
                objReportModel.ConversionToPlanModel = objConversionToPlanModel;
                #endregion

                #region "Calculate Barchart data by TimeFrame"
                BarChartModel objBarChartModel = new BarChartModel();
                List<BarChartSeries> lstSeries = new List<BarChartSeries>();
                _Categories = new List<string>();
                _Categories = objBasicModel.Categories;
                double catLength = _Categories != null ? _Categories.Count : 0;
                List<double> serData1 = new List<double>();
                List<double> serData2 = new List<double>();
                List<double> serData3 = new List<double>();
                double _Actual = 0, _Projected = 0, _Goal = 0, Actual_Projected = 0;
                bool _IsQuarterly = objBasicModel.IsQuarterly;
                int _compareValue = 0;
                _compareValue = _IsQuarterly ? GetCurrentQuarterNumber() : currentMonth;
                serData1.Add(0); // Insert blank data at 1st index of list to Add padding to Graph.
                serData2.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.
                serData3.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.

                if (!_IsQuarterly)
                {
                    serData1.Add(0); // Insert blank data at 1st index of list to Add padding to Graph.
                    serData2.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.
                    serData3.Add(0);// Insert blank data at 1st index of list to Add padding to Graph.
                }

                for (int i = 0; i < catLength; i++)
                {
                    _Actual = objBasicModel.ActualList[i] != null ? objBasicModel.ActualList[i] : 0;
                    _Projected = objBasicModel.ProjectedList[i] != null ? objBasicModel.ProjectedList[i] : 0;
                    _Goal = objBasicModel.GoalList[i] != null ? objBasicModel.GoalList[i] : 0;
                    Actual_Projected = _Actual + _Projected;
                    serData2.Add(_Goal);
                    if ((i + 1) <= (_compareValue))
                    {
                        serData1.Add(Actual_Projected);
                        serData3.Add(0);
                    }
                    else
                    {
                        serData1.Add(0);
                        serData3.Add(Actual_Projected);
                    }
                }

                BarChartSeries _chartSeries1 = new BarChartSeries();
                _chartSeries1.name = "Actual";
                _chartSeries1.data = serData1;
                _chartSeries1.type = "column";
                lstSeries.Add(_chartSeries1);

                BarChartSeries _chartSeries2 = new BarChartSeries();
                _chartSeries2.name = "Goal";
                _chartSeries2.data = serData2;
                _chartSeries2.type = "column";
                lstSeries.Add(_chartSeries2);

                BarChartSeries _chartSeries3 = new BarChartSeries();
                _chartSeries3.name = "Projected";
                _chartSeries3.data = serData3;
                _chartSeries3.type = "column";
                lstSeries.Add(_chartSeries3);

                List<string> _barChartCategories = new List<string>();
                if (!_IsQuarterly)
                {
                    _barChartCategories.Add(string.Empty);
                    _barChartCategories.Add(string.Empty);
                    _barChartCategories.AddRange(_Categories);
                }
                else
                {
                    _barChartCategories.Add(string.Empty);
                    _barChartCategories.AddRange(_Categories);
                }

                objBarChartModel.series = lstSeries;
                objBarChartModel.categories = _barChartCategories;
                objConversionToPlanModel.ConversionToPlanBarChartModel = objBarChartModel;
                objReportModel.ConversionToPlanModel = objConversionToPlanModel;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return Json(objReportModel, JsonRequestBehavior.AllowGet);
            return PartialView("_ConversionToPlan", objReportModel.ConversionToPlanModel);
        }
        #endregion

        /// <summary>
        /// Cretaed By Nishant Sheth
        /// Desc: Get the limited data for card section with pagination, search, and sorting features on Converstion.
        /// </summary>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <param name="SearchString"></param>
        /// <param name="SortBy"></param>
        /// <returns></returns>
        /// 
        public PartialViewResult SearchSortPaginataionConverstion(int PageNo = 0, int PageSize = 5, string SearchString = "", string SortBy = "", string ParentLabel = "", string childlabelType = "", string option = "")
        {
            ViewBag.ConvParentLabel = ParentLabel;
            ViewBag.ConvchildlabelType = childlabelType;
            ViewBag.Convoption = option;
            CardSectionModel cardModel = new CardSectionModel();
            cardModel = ConverstionCardSectionModelWithFilter(PageNo, PageSize, SearchString, SortBy);
            //cardModel.TotalRecords = cardModel.TotalRecords;
            return PartialView("_ConversionCardSection", cardModel);
        }
        public CardSectionModel ConverstionCardSectionModelWithFilter(int PageNo = 0, int PageSize = 5, string SearchString = "", string SortBy = "")
        {
            #region Declartion local variables

            List<CardSectionListModel> objCardSectionList = (List<CardSectionListModel>)TempData["ConverstionCardList"];
            TempData["ConverstionCardList"] = objCardSectionList;
            CardSectionModel cardModel = new CardSectionModel();
        #endregion
            cardModel.TotalRecords = objCardSectionList.Count();
            if (SortBy == Enums.SortByWaterFall.ADS.ToString())
            {
                objCardSectionList = objCardSectionList.Where(x => x.title.ToLower().Contains((!string.IsNullOrEmpty(SearchString) ? SearchString.ToLower().Trim() : x.title.ToLower()))).OrderByDescending(x => x.ADSCardValues.Actual_Projected).Skip(PageNo * PageSize).Take(PageSize).ToList();
            }
            else if (SortBy == Enums.SortByWaterFall.CW.ToString())
            {
                objCardSectionList = objCardSectionList.Where(x => x.title.ToLower().Contains((!string.IsNullOrEmpty(SearchString) ? SearchString.ToLower().Trim() : x.title.ToLower()))).OrderByDescending(x => x.CWCardValues.Actual_Projected).Skip(PageNo * PageSize).Take(PageSize).ToList();
            }
            else if ((SortBy == Enums.SortByWaterFall.MQL.ToString()))
            {
                objCardSectionList = objCardSectionList.Where(x => x.title.ToLower().Contains((!string.IsNullOrEmpty(SearchString) ? SearchString.ToLower().Trim() : x.title.ToLower()))).OrderByDescending(x => x.TQLCardValues.Actual_Projected).Skip(PageNo * PageSize).Take(PageSize).ToList();
                //objCardSectionList = objCardSectionList.OrderByDescending(x => x.RevenueCardValues.Actual_Projected).Skip(PageNo * PageSize).Take(PageSize).ToList();
            }
            else
            {
                objCardSectionList = objCardSectionList.Where(x => x.title.ToLower().Contains((!string.IsNullOrEmpty(SearchString) ? SearchString.ToLower().Trim() : x.title.ToLower()))).OrderByDescending(x => x.INQCardValues.Actual_Projected).Skip(PageNo * PageSize).Take(PageSize).ToList();
                //objCardSectionList = objCardSectionList.OrderByDescending(x => x.RevenueCardValues.Actual_Projected).Skip(PageNo * PageSize).Take(PageSize).ToList();
            }
            cardModel.CardSectionListModel = objCardSectionList;
            if (!string.IsNullOrEmpty(SearchString))
            {
                cardModel.TotalRecords = objCardSectionList.Count();
            }
            cardModel.CuurentPageNum = PageNo;
            return cardModel;
        }
        #endregion
        public List<CardSectionListModel> GetConversionCardSectionList(List<TacticStageValue> _TacticData, List<TacticMappingItem> TacticMappingList, string timeframeOption, bool IsQuarterly, string ParentLabel = "", bool IsTacticCustomField = false, string CustomFieldType = "", int customFieldId = 0)
        {
            #region "Declare local variables"
            List<CardSectionListModel> objCardSectionList = new List<CardSectionListModel>();
            CardSectionListModel objCardSection = new CardSectionListModel();
            CardSectionListSubModel objCardSectionSubModel = new CardSectionListSubModel();

            int ParentId = 0;
            List<int> ParentIdsList = new List<int>();
            List<int> _ChildIdsList = new List<int>();  // TacticIds List.
            string strParentTitle = string.Empty;
            List<ActualTrendModel> ActualTacticTrendList = new List<ActualTrendModel>();
            List<ActualTrendModel> ActualTacticTrendListOverall = new List<ActualTrendModel>();
            List<ProjectedTrendModel> ProjectedTrendList = new List<ProjectedTrendModel>();
            #endregion

           
            #region "Projected goal value"
            bool IsTillCurrentMonth = true;
            List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();
            List<conversion_Projected_Goal_LineChart> Projected_Goal_LineChartList = new List<conversion_Projected_Goal_LineChart>();
            conversion_Projected_Goal_LineChart objProjected_Goal_LineChart = new conversion_Projected_Goal_LineChart();
            

            ConversionOverviewModel objConversionOverviewModel = new ConversionOverviewModel();
            Projected_Goal objProjectedGoal = new Projected_Goal();


            List<string> includeMonth = GetMonthListForReport(timeframeOption);
            //List<TacticStageValue> Tacticdata = Common.GetTacticStageRelation(tacticlist, IsReport: true);




           // string strMQLStageCode = Enums.InspectStage.MQL.ToString();
           // string strCWStageCode = Enums.InspectStage.CW.ToString();
            double _inqActual = 0, _mqlActual = 0, _cwActual = 0;// stageVolumePercntg = 0;
            string revStageCode = Enums.InspectStage.Revenue.ToString();
            //string inqStageCode = Enums.InspectStage.INQ.ToString();
            string mqlStageCode = Enums.InspectStage.MQL.ToString();
            string cwStageCode = Enums.InspectStage.CW.ToString();
            string projectedStageCode = Enums.InspectStage.ProjectedStageValue.ToString();

            #endregion

            List<string> ActualStageCodeList = new List<string>();
            ActualStageCodeList.Add(revStageCode);
            ActualStageCodeList.Add(projectedStageCode);
            ActualStageCodeList.Add(mqlStageCode);
            ActualStageCodeList.Add(cwStageCode);
            List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
            ActualTacticStageList = GetActualListInTacticInterval(_TacticData, timeframeOption, ActualStageCodeList, IsTillCurrentMonth);//
            ActualTacticTrendListOverall = GetActualTrendModelForRevenueOverview(_TacticData, ActualTacticStageList);
            List<TacticDataTable> _TacticDataTable = new List<TacticDataTable>();
            List<TacticMonthValue> _TacticListMonth = new List<TacticMonthValue>();
            List<ProjectedTacticModel> _TacticList = new List<ProjectedTacticModel>();
            double ProjvsGoal = 0, Percentage = 0, _ConversePercentage = 0, _value = 0;


            ParentIdsList = TacticMappingList.Select(card => card.ParentId).Distinct().ToList();
            List<TacticStageValue> fltrTacticData = new List<TacticStageValue>();

            //double _stagevolumeMQL = 0;
            //double _stagebenchmarkMQL = 0;
            //double restpercentageMQL = 0;
            //double _stagevolumeCW = 0;
            //double _stagebenchmarkCW = 0;
            //double restpercentageCW = 0;
            try
            {
                if (ParentLabel != Common.RevenueCampaign)
                {
                    if (ParentLabel.Contains(Common.CampaignCustomTitle))
                    {
                        ParentLabel = Common.CampaignCustomTitle;
                    }
                    else if (ParentLabel.Contains(Common.ProgramCustomTitle))
                    {
                        ParentLabel = Common.ProgramCustomTitle;
                    }
                    else
                    {
                        ParentLabel = Common.TacticCustomTitle;
                    }
                }
                foreach (int _ParentId in ParentIdsList)
                {
                    // Get ChildIds(Tactic) List.
                    fltrTacticData = new List<TacticStageValue>();
                    _ChildIdsList = TacticMappingList.Where(card => card.ParentId.Equals(_ParentId)).Select(card => card.ChildId).ToList();
                    fltrTacticData = _TacticData.Where(tac => _ChildIdsList.Contains(tac.TacticObj.PlanTacticId)).ToList();
                    #region "Set Default Values"
                    strParentTitle = TacticMappingList.Where(card => card.ParentId.Equals(_ParentId)).Select(card => card.ParentTitle).FirstOrDefault();
                    ParentId = _ParentId;
                    #endregion

                    objCardSection = new CardSectionListModel();

                    #region "Add Static Values to Model"
                    objCardSection.title = HttpUtility.HtmlDecode(strParentTitle);      // Set ParentTitle Ex. (Campaign1) 
                  
                    objCardSection.MasterParentlabel = ParentLabel;   // Set ParentLabel: Selected value from ViewBy Dropdownlist. Ex. (Campaign)
                    objCardSection.FieldId = _ParentId;       // Set ParentId: Card Item(Campaign, Program, Tactic or CustomfieldOption) Id.

                    List<ActualDataTable> ActualRevenueDataTable = new List<ActualDataTable>();
                    ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();

                    #region "Insert Cardsection Sub model data"
                    if (IsTacticCustomField)
                    {
                        ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                        ActualRevenueDataTable = new List<ActualDataTable>();
                        ActualTacticTrendList = new List<ActualTrendModel>();
                        ActualTacticList = ActualTacticStageList.Where(actual => actual.StageCode.Equals(projectedStageCode)).Select(actual => actual.ActualTacticList).FirstOrDefault();
                        ActualTacticList = ActualTacticList.Where(actual => _ChildIdsList.Contains(actual.PlanTacticId)).ToList();
                        ActualRevenueDataTable = GetActualTacticDataTablebyStageCode(customFieldId, ParentId.ToString(), CustomFieldType, Enums.InspectStage.INQ, ActualTacticList, _TacticData, IsTacticCustomField);
                        ActualTacticTrendList = GetActualTrendModelForRevenue(_TacticData, ActualRevenueDataTable, Enums.InspectStage.INQ.ToString());

                        ProjectedTrendList = new List<ProjectedTrendModel>();
                        _TacticDataTable = new List<TacticDataTable>();
                        _TacticListMonth = new List<TacticMonthValue>();
                        _TacticList = new List<ProjectedTacticModel>();
                        _TacticDataTable = GetTacticDataTablebyStageCode(customFieldId, _ParentId.ToString(), CustomFieldType, Enums.InspectStage.INQ, fltrTacticData, IsTacticCustomField, true);
                        _TacticListMonth = GetMonthWiseValueList(_TacticDataTable);
                        _TacticList = _TacticListMonth.Select(tac => new ProjectedTacticModel
                        {
                            TacticId = tac.Id,
                            StartMonth = tac.StartMonth,
                            EndMonth = tac.EndMonth,
                            Value = tac.Value,
                            Year = tac.StartYear
                        }).Distinct().ToList();
                        ProjectedTrendList = GetProjectedTrendModel(_TacticList);
                        ProjectedTrendList = (from _prjTac in ProjectedTrendList
                                              group _prjTac by new
                                              {
                                                  _prjTac.PlanTacticId,
                                                  _prjTac.Month,
                                                  _prjTac.Value,
                                                  _prjTac.TrendValue
                                              } into tac
                                              select new ProjectedTrendModel
                                              {
                                                  PlanTacticId = tac.Key.PlanTacticId,
                                                  Month = tac.Key.Month,
                                                  Value = tac.Key.Value,
                                                  TrendValue = tac.Key.TrendValue
                                              }).Distinct().ToList();

                        _inqActual = 0;
                        _inqActual = ActualTacticTrendList.Sum(actual => actual.TrendValue);
                        // Start convertion CardSection SubModel Data
                        objCardSectionSubModel = new CardSectionListSubModel();
                        objCardSectionSubModel.CardType = Enums.InspectStage.INQ.ToString();
                        objCardSectionSubModel.Actual_Projected = _inqActual;
                        objCardSectionSubModel.Goal = ProjectedTrendList.Sum(goal => goal.Value);
                        ProjvsGoal = objCardSectionSubModel.Goal != 0 ? ((objCardSectionSubModel.Actual_Projected - objCardSectionSubModel.Goal) / objCardSectionSubModel.Goal) : 0;
                        Percentage = ProjvsGoal * 100; // Calculate Percentage based on Actual_Projected & Goal value.
                        if (Percentage > 0)
                            objCardSectionSubModel.IsNegative = false;
                        else
                            objCardSectionSubModel.IsNegative = true;
                        objCardSectionSubModel.Percentage = Percentage;
                        objCardSection.INQCardValues = objCardSectionSubModel;
                        // End convertion CardSection SubModel Data

                        ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                        ActualRevenueDataTable = new List<ActualDataTable>();
                        ActualTacticTrendList = new List<ActualTrendModel>();
                        ActualTacticList = ActualTacticStageList.Where(actual => actual.StageCode.Equals(mqlStageCode)).Select(actual => actual.ActualTacticList).FirstOrDefault();
                        ActualTacticList = ActualTacticList.Where(actual => _ChildIdsList.Contains(actual.PlanTacticId)).ToList();
                        ActualRevenueDataTable = GetActualTacticDataTablebyStageCode(customFieldId, ParentId.ToString(), CustomFieldType, Enums.InspectStage.MQL, ActualTacticList, _TacticData, IsTacticCustomField);
                        ActualTacticTrendList = GetActualTrendModelForRevenue(_TacticData, ActualRevenueDataTable, Enums.InspectStage.MQL.ToString());

                        ProjectedTrendList = new List<ProjectedTrendModel>();
                        _TacticDataTable = new List<TacticDataTable>();
                        _TacticListMonth = new List<TacticMonthValue>();
                        _TacticList = new List<ProjectedTacticModel>();
                        _TacticDataTable = GetTacticDataTablebyStageCode(customFieldId, _ParentId.ToString(), CustomFieldType, Enums.InspectStage.MQL, fltrTacticData, IsTacticCustomField, true);
                        _TacticListMonth = GetMonthWiseValueList(_TacticDataTable);
                        _TacticList = _TacticListMonth.Select(tac => new ProjectedTacticModel
                        {
                            TacticId = tac.Id,
                            StartMonth = tac.StartMonth,
                            EndMonth = tac.EndMonth,
                            Value = tac.Value,
                            Year = tac.StartYear
                        }).Distinct().ToList();
                        ProjectedTrendList = GetProjectedTrendModel(_TacticList);
                        ProjectedTrendList = (from _prjTac in ProjectedTrendList
                                              group _prjTac by new
                                              {
                                                  _prjTac.PlanTacticId,
                                                  _prjTac.Month,
                                                  _prjTac.Value,
                                                  _prjTac.TrendValue
                                              } into tac
                                              select new ProjectedTrendModel
                                              {
                                                  PlanTacticId = tac.Key.PlanTacticId,
                                                  Month = tac.Key.Month,
                                                  Value = tac.Key.Value,
                                                  TrendValue = tac.Key.TrendValue
                                              }).Distinct().ToList();

                        

                        // Start convertion CardSection SubModel Data
                        objCardSectionSubModel = new CardSectionListSubModel();
                        objCardSectionSubModel.CardType = Enums.InspectStage.MQL.ToString();
                        _mqlActual = 0;
                        _mqlActual = ActualTacticTrendList.Sum(actual => actual.TrendValue); ;
                        objCardSectionSubModel.Actual_Projected = _mqlActual;
                        objCardSectionSubModel.Goal = ProjectedTrendList.Sum(goal => goal.Value);
                        ProjvsGoal = objCardSectionSubModel.Goal != 0 ? ((objCardSectionSubModel.Actual_Projected - objCardSectionSubModel.Goal) / objCardSectionSubModel.Goal) : 0;
                        Percentage = ProjvsGoal * 100; // Calculate Percentage based on Actual_Projected & Goal value.
                        if (Percentage > 0)
                            objCardSectionSubModel.IsNegative = false;
                        else
                            objCardSectionSubModel.IsNegative = true;
                        objCardSectionSubModel.Percentage = Percentage;
                        //objCardSectionSubModel.RestPercentage = Convert.ToDouble(restpercentageMQL);
                        #region  "ConversePercentage Added by dashrath prajapati"
                        _value = 0;
                        _value = _inqActual > 0 ? (_mqlActual / _inqActual) : 0;
                        _ConversePercentage = _value * 100;
                        objCardSectionSubModel.ConversePercentage = _ConversePercentage;
                        #endregion
                        objCardSection.TQLCardValues = objCardSectionSubModel;
                        // End convertion CardSection SubModel Data
                        double cwActualvalue = 0;
                        ActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                        ActualRevenueDataTable = new List<ActualDataTable>();
                        ActualTacticTrendList = new List<ActualTrendModel>();
                        ActualTacticList = ActualTacticStageList.Where(actual => actual.StageCode.Equals(cwStageCode)).Select(actual => actual.ActualTacticList).FirstOrDefault();
                        ActualTacticList = ActualTacticList.Where(actual => _ChildIdsList.Contains(actual.PlanTacticId)).ToList();
                        ActualRevenueDataTable = GetActualTacticDataTablebyStageCode(customFieldId, ParentId.ToString(), CustomFieldType, Enums.InspectStage.CW, ActualTacticList, _TacticData, IsTacticCustomField);
                        ActualTacticTrendList = GetActualTrendModelForRevenue(_TacticData, ActualRevenueDataTable, Enums.InspectStage.CW.ToString());

                        ProjectedTrendList = new List<ProjectedTrendModel>();
                        _TacticDataTable = new List<TacticDataTable>();
                        _TacticListMonth = new List<TacticMonthValue>();
                        _TacticList = new List<ProjectedTacticModel>();
                        _TacticDataTable = GetTacticDataTablebyStageCode(customFieldId, _ParentId.ToString(), CustomFieldType, Enums.InspectStage.CW, fltrTacticData, IsTacticCustomField, true);
                        _TacticListMonth = GetMonthWiseValueList(_TacticDataTable);
                        _TacticList = _TacticListMonth.Select(tac => new ProjectedTacticModel
                        {
                            TacticId = tac.Id,
                            StartMonth = tac.StartMonth,
                            EndMonth = tac.EndMonth,
                            Value = tac.Value,
                            Year = tac.StartYear
                        }).Distinct().ToList();
                        ProjectedTrendList = GetProjectedTrendModel(_TacticList);
                        ProjectedTrendList = (from _prjTac in ProjectedTrendList
                                              group _prjTac by new
                                              {
                                                  _prjTac.PlanTacticId,
                                                  _prjTac.Month,
                                                  _prjTac.Value,
                                                  _prjTac.TrendValue
                                              } into tac
                                              select new ProjectedTrendModel
                                              {
                                                  PlanTacticId = tac.Key.PlanTacticId,
                                                  Month = tac.Key.Month,
                                                  Value = tac.Key.Value,
                                                  TrendValue = tac.Key.TrendValue
                                              }).Distinct().ToList();

                        

                        // Start convertion CardSection SubModel Data
                        objCardSectionSubModel = new CardSectionListSubModel();
                        objCardSectionSubModel.CardType = Enums.InspectStage.CW.ToString();
                        cwActualvalue = 0;
                        cwActualvalue = ActualTacticTrendList.Sum(actual => actual.TrendValue);
                        objCardSectionSubModel.Actual_Projected = cwActualvalue;
                        objCardSectionSubModel.Goal = ProjectedTrendList.Sum(goal => goal.Value);
                        ProjvsGoal = objCardSectionSubModel.Goal != 0 ? ((objCardSectionSubModel.Actual_Projected - objCardSectionSubModel.Goal) / objCardSectionSubModel.Goal) : 0;
                        Percentage = ProjvsGoal * 100; // Calculate Percentage based on Actual_Projected & Goal value.
                        if (Percentage > 0)
                            objCardSectionSubModel.IsNegative = false;
                        else
                            objCardSectionSubModel.IsNegative = true;
                        objCardSectionSubModel.Percentage = Percentage;
                        //objCardSectionSubModel.RestPercentage = Convert.ToDouble(restpercentageCW);
                        #region  "ConversePercentage Added by dashrath prajapati"
                        _value = 0;
                        _value = _mqlActual > 0 ? (cwActualvalue / _mqlActual) : 0;
                        _ConversePercentage = _value * 100;
                        objCardSectionSubModel.ConversePercentage = _ConversePercentage;
                        #endregion
                        objCardSection.CWCardValues = objCardSectionSubModel;
                        // End convertion CardSection SubModel Data
                        

                    }
                    else
                    {
                        ProjectedTrendList = new List<ProjectedTrendModel>();
                        ProjectedTrendList = CalculateProjectedTrend(fltrTacticData, includeMonth, Enums.InspectStage.ProjectedStageValue.ToString());
                        _inqActual = ActualTacticTrendListOverall.Where(actual => actual.StageCode == projectedStageCode && _ChildIdsList.Contains(actual.PlanTacticId)).Sum(actual => actual.TrendValue);
                        // Start convertion CardSection SubModel Data
                        objCardSectionSubModel = new CardSectionListSubModel();
                        objCardSectionSubModel.CardType = Enums.InspectStage.INQ.ToString();
                        objCardSectionSubModel.Actual_Projected = _inqActual;
                        objCardSectionSubModel.Goal = ProjectedTrendList.Sum(goal => goal.Value);
                        ProjvsGoal = objCardSectionSubModel.Goal != 0 ? ((objCardSectionSubModel.Actual_Projected - objCardSectionSubModel.Goal) / objCardSectionSubModel.Goal) : 0;
                        Percentage = ProjvsGoal * 100; // Calculate Percentage based on Actual_Projected & Goal value.
                        if (Percentage > 0)
                            objCardSectionSubModel.IsNegative = false;
                        else
                            objCardSectionSubModel.IsNegative = true;
                        objCardSectionSubModel.Percentage = Percentage;
                        objCardSection.INQCardValues = objCardSectionSubModel;
                        // End convertion CardSection SubModel Data

                        ProjectedTrendList = new List<ProjectedTrendModel>();
                        ProjectedTrendList = CalculateProjectedTrend(fltrTacticData, includeMonth, Enums.InspectStage.MQL.ToString());
                        _mqlActual = 0;
                        _mqlActual = ActualTacticTrendListOverall.Where(actual => actual.StageCode == mqlStageCode && _ChildIdsList.Contains(actual.PlanTacticId)).Sum(actual => actual.TrendValue);
                       

                        // Start convertion CardSection SubModel Data
                        objCardSectionSubModel = new CardSectionListSubModel();
                        //objCardSectionSubModel.CardType = Enums.InspectStage.TQL.ToString();
                        objCardSectionSubModel.CardType = Enums.InspectStage.MQL.ToString(); // Change By Nishat Sheth
                        objCardSectionSubModel.Actual_Projected = _mqlActual;
                        objCardSectionSubModel.Goal = ProjectedTrendList.Sum(goal => goal.Value);
                        ProjvsGoal = objCardSectionSubModel.Goal != 0 ? ((objCardSectionSubModel.Actual_Projected - objCardSectionSubModel.Goal) / objCardSectionSubModel.Goal) : 0;
                        Percentage = ProjvsGoal * 100; // Calculate Percentage based on Actual_Projected & Goal value.
                        if (Percentage > 0)
                            objCardSectionSubModel.IsNegative = false;
                        else
                            objCardSectionSubModel.IsNegative = true;
                        objCardSectionSubModel.Percentage = Percentage;
                        //objCardSectionSubModel.RestPercentage = Convert.ToDouble(restpercentageMQL);
                        #region  "ConversePercentage Added by dashrath prajapati"
                        _value = 0;
                        _value = _inqActual > 0 ? (_mqlActual / _inqActual) : 0;
                        _ConversePercentage = _value * 100;
                        objCardSectionSubModel.ConversePercentage = _ConversePercentage;
                        #endregion

                        objCardSection.TQLCardValues = objCardSectionSubModel;
                        // End convertion CardSection SubModel Data
                        ProjectedTrendList = new List<ProjectedTrendModel>();
                        _cwActual = 0;
                        _cwActual = ActualTacticTrendListOverall.Where(actual => actual.StageCode == cwStageCode && _ChildIdsList.Contains(actual.PlanTacticId)).Sum(actual => actual.TrendValue);

                        ProjectedTrendList = CalculateProjectedTrend(fltrTacticData, includeMonth, Enums.InspectStage.CW.ToString());
                       
                       

                        // Start convertion CardSection SubModel Data
                        objCardSectionSubModel = new CardSectionListSubModel();
                        objCardSectionSubModel.CardType = Enums.InspectStage.CW.ToString();
                        objCardSectionSubModel.Actual_Projected = _cwActual;
                        objCardSectionSubModel.Goal = ProjectedTrendList.Sum(goal => goal.Value);
                        ProjvsGoal = objCardSectionSubModel.Goal != 0 ? ((objCardSectionSubModel.Actual_Projected - objCardSectionSubModel.Goal) / objCardSectionSubModel.Goal) : 0;
                        Percentage = ProjvsGoal * 100; // Calculate Percentage based on Actual_Projected & Goal value.
                        if (Percentage > 0)
                            objCardSectionSubModel.IsNegative = false;
                        else
                            objCardSectionSubModel.IsNegative = true;
                        objCardSectionSubModel.Percentage = Percentage;
                        //objCardSectionSubModel.RestPercentage = Convert.ToDouble(restpercentageCW);
                        #region  "ConversePercentage Added by dashrath prajapati"
                        _value = 0;
                        _value = _mqlActual > 0 ? (_cwActual / _mqlActual) : 0;
                        _ConversePercentage = _value * 100;
                        objCardSectionSubModel.ConversePercentage = _ConversePercentage;
                        #endregion
                        objCardSection.CWCardValues = objCardSectionSubModel;
                        // End convertion CardSection SubModel Data

                       

                    }

                    #endregion
                    objCardSectionList.Add(objCardSection);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objCardSectionList;
        }


        public List<CardSectionListModel> GetCardSectionDefaultData(List<TacticStageValue> _TacticData, List<ActualTrendModel> ActualTacticTrendList, List<ProjectedTrendModel> ProjectedTrendList, List<TacticMappingItem> TacticMappingList, string timeframeOption, bool IsQuarterly, string ParentLabel = "", bool IsTacticCustomField = false, string CustomFieldType = "", int customFieldId = 0)
        {
            #region "Declare local variables"
            List<CardSectionListModel> objCardSectionList = new List<CardSectionListModel>();
            CardSectionListModel objCardSection = new CardSectionListModel();
            CardSectionListSubModel objCardSectionSubModel;
            string strParentTitle = string.Empty;
            List<int> ParentIdsList = new List<int>();
            List<int> _ChildIdsList = new List<int>();      // TacticIds List.
            List<TacticStageValue> fltrTacticData = new List<TacticStageValue>();
            string revStageCode = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
            List<string> revStageCodeList = new List<string> { revStageCode };
            List<string> IncludeCurrentMonth = new List<string>();
            List<TacticwiseOverviewModel> _fltrTacticwiseData = new List<TacticwiseOverviewModel>();
            double ProjvsGoal = 0, Percentage = 0;
            lineChartData objLineChartData = new lineChartData();
            #region "Quarterly Trend Varaibles"
            //List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            //List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            //List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            //List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };
            //string strActual, strProjected, strTrendValue;
            //double ActualQ1 = 0, ActualQ2 = 0, ActualQ3 = 0, ActualQ4 = 0, TotalRevenueTypeCol = 0, TotalTrendQ1 = 0, TotalTrendQ2 = 0, TotalTrendQ3 = 0, TotalTrendQ4 = 0;


            #endregion




            #endregion

            ParentIdsList = TacticMappingList.Select(card => card.ParentId).Distinct().ToList();

            try
            {

                #region "Get Year list"
                List<string> yearlist = new List<string>();
                yearlist.Add(timeframeOption);
                IncludeCurrentMonth = GetMonthWithYearUptoCurrentMonth(yearlist);
                #endregion

                string costStageCode = Enums.InspectStageValues[Enums.InspectStage.Cost.ToString()].ToString();
                List<TacticMonthValue> CurrentMonthCostList = new List<TacticMonthValue>();
                List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItemList = new List<Plan_Campaign_Program_Tactic_LineItem>();
                List<Plan_Campaign_Program_Tactic_LineItem_Actual> tblLineItemActualList = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                List<int> TacticIds = new List<int>();
                List<int> LineItemIds = new List<int>();
                List<TacticMonthValue> TacticCostData = new List<TacticMonthValue>();

                TacticIds = _TacticData.Select(tac => tac.TacticObj.PlanTacticId).ToList();
                tblTacticLineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(line => TacticIds.Contains(line.PlanTacticId) && line.IsDeleted.Equals(false)).ToList();
                LineItemIds = tblTacticLineItemList.Select(line => line.PlanLineItemId).ToList();
                tblLineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineActual => LineItemIds.Contains(lineActual.PlanLineItemId)).ToList();
                if (!IsTacticCustomField)
                {
                    TacticCostData = GetActualCostData(_TacticData, tblTacticLineItemList, tblLineItemActualList);
                    CurrentMonthCostList = TacticCostData.Where(actual => IncludeCurrentMonth.Contains(actual.Month)).ToList();
                }

                List<ActualTacticListByStage> ActualTacticStageList = new List<ActualTacticListByStage>();

                List<Plan_Campaign_Program_Tactic_Actual> lstActuals = new List<Plan_Campaign_Program_Tactic_Actual>();

                List<ActualDataTable> _revActualDataTable = new List<ActualDataTable>();
                List<ActualDataTable> CurrentMonthActualTacticList = new List<ActualDataTable>();

                List<Plan_Campaign_Program_Tactic_Actual> _revActualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                ActualTacticStageList = GetActualListUpToCurrentMonthByStageCode(_TacticData, timeframeOption, revStageCodeList, false);
                if (ActualTacticStageList != null)
                {
                    _revActualTacticList = ActualTacticStageList.Where(act => act.StageCode.Equals(revStageCode)).Select(act => act.ActualTacticList).FirstOrDefault();
                }


                List<TacticMonthValue> innerCurrentMonthCostList = new List<TacticMonthValue>();
                List<ActualTrendModel> inneractuallist = new List<ActualTrendModel>();

                if (ParentLabel != Common.RevenueCampaign)
                {
                    if (ParentLabel.Contains(Common.CampaignCustomTitle))
                    {
                        ParentLabel = Common.CampaignCustomTitle;
                    }
                    else if (ParentLabel.Contains(Common.ProgramCustomTitle))
                    {
                        ParentLabel = Common.ProgramCustomTitle;
                    }
                    else
                    {
                        ParentLabel = Common.TacticCustomTitle;
                    }
                }

                List<Plan_Campaign_Program_Tactic_LineItem> lstTacticLineItem = new List<Plan_Campaign_Program_Tactic_LineItem>();
                //.Select(l => new Plan_Campaign_Program_Tactic_LineItem { PlanLineItemId = l.PlanLineItemId, PlanTacticId = l.PlanTacticId})
                lstTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(line => TacticIds.Contains(line.PlanTacticId) && line.IsDeleted == false).ToList();
                var lineitemsids = lstTacticLineItem.Select(ln => ln.PlanLineItemId).ToList();
                List<Plan_Campaign_Program_Tactic_LineItem_Cost> tblLineItemCost = new List<Plan_Campaign_Program_Tactic_LineItem_Cost>();
                tblLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(line => lineitemsids.Contains(line.PlanLineItemId)).ToList();
                List<Plan_Campaign_Program_Tactic_Cost> tblTacticCostList = new List<Plan_Campaign_Program_Tactic_Cost>();
                tblTacticCostList = db.Plan_Campaign_Program_Tactic_Cost.Where(line => TacticIds.Contains(line.PlanTacticId)).ToList();

                int currentEndMonth = 12;
                if (currentYear == timeframeOption)
                {
                    currentEndMonth = DateTime.Now.Month;
                }
                List<string> periodList = new List<string>();
                for (int i = 1; i <= currentEndMonth; i++)
			    {
                    periodList.Add(PeriodPrefix + i);
			    }
                ActualTacticTrendList = ActualTacticTrendList.Where(actual => periodList.Contains(actual.Month)).ToList();
                #region "iterate each card item"
                foreach (int _ParentId in ParentIdsList)
                {

                    double costActual = 0;
                    double revenueActual = 0;
                    double costGoal = 0;
                    double revenueGoal = 0;
                    fltrTacticData = new List<TacticStageValue>();
                    innerCurrentMonthCostList = new List<TacticMonthValue>();
                    inneractuallist = new List<ActualTrendModel>();
                    // Get ChildIds(Tactic) List.
                    _ChildIdsList = TacticMappingList.Where(card => card.ParentId.Equals(_ParentId)).Select(card => card.ChildId).ToList();

                    fltrTacticData = _TacticData.Where(tac => _ChildIdsList.Contains(tac.TacticObj.PlanTacticId)).ToList();
                    #region "Set Default Values"
                    strParentTitle = TacticMappingList.Where(card => card.ParentId.Equals(_ParentId)).Select(card => card.ParentTitle).FirstOrDefault();
                    //ParentId = _ParentId;
                    //ParentId = _ChildId;// Change By Nishant
                    #endregion

                    objCardSection = new CardSectionListModel();

                    objCardSection.title = HttpUtility.HtmlDecode(strParentTitle);      // Set ParentTitle Ex. (Campaign1) 
                    objCardSection.MasterParentlabel = ParentLabel;   // Set ParentLabel: Selected value from ViewBy Dropdownlist. Ex. (Campaign)
                    objCardSection.FieldId = _ParentId;       // Set ParentId: Card Item(Campaign, Program, Tactic or CustomfieldOption) Id.

                    if (IsTacticCustomField)
                    {

                        lstActuals = _revActualTacticList.Where(ta => _ChildIdsList.Contains(ta.PlanTacticId)).ToList();
                        //// Get Actuals Tactic list by weightage for Revenue.
                        _revActualDataTable = GetActualTacticDataTablebyStageCode(customFieldId, _ParentId.ToString(), CustomFieldType, Enums.InspectStage.Revenue, lstActuals, _TacticData, IsTacticCustomField);

                        //// Get ActualList upto CurrentMonth.
                        CurrentMonthActualTacticList = _revActualDataTable.Where(actual => IncludeCurrentMonth.Contains(_TacticData.Where(tac => tac.TacticObj.PlanTacticId.Equals(actual.PlanTacticId)).FirstOrDefault().TacticYear + actual.Period)).ToList();

                        #region "Get Tactic data by Weightage for Projected by StageCode(Revenue)"
                        List<TacticDataTable> _TacticDataTable = new List<TacticDataTable>();
                        List<TacticMonthValue> _TacticListMonth = new List<TacticMonthValue>();
                        List<ProjectedTacticModel> _TacticList = new List<ProjectedTacticModel>();

                        _TacticDataTable = GetTacticDataTablebyStageCode(customFieldId, _ParentId.ToString(), CustomFieldType, Enums.InspectStage.Revenue, _TacticData, IsTacticCustomField, true);
                        _TacticListMonth = GetMonthWiseValueList(_TacticDataTable);
                        _TacticList = _TacticListMonth.Select(tac => new ProjectedTacticModel
                        {
                            TacticId = tac.Id,
                            StartMonth = tac.StartMonth,
                            EndMonth = tac.EndMonth,
                            Value = tac.Value,
                            Year = tac.StartYear
                        }).Distinct().ToList();
                        ProjectedTrendList = GetProjectedTrendModel(_TacticList);
                        ProjectedTrendList = (from _prjTac in ProjectedTrendList
                                              group _prjTac by new
                                              {
                                                  _prjTac.PlanTacticId,
                                                  _prjTac.Month,
                                                  _prjTac.Value,
                                                  _prjTac.TrendValue
                                              } into tac
                                              select new ProjectedTrendModel
                                              {
                                                  PlanTacticId = tac.Key.PlanTacticId,
                                                  Month = tac.Key.Month,
                                                  Value = tac.Key.Value,
                                                  TrendValue = tac.Key.TrendValue
                                              }).Distinct().ToList();
                        #endregion
                        inneractuallist = CurrentMonthActualTacticList.Select(actual => new ActualTrendModel { PlanTacticId = actual.PlanTacticId, Month = actual.Period, TrendValue = actual.ActualValue }).ToList();
                        revenueActual = CurrentMonthActualTacticList.Sum(data => data.ActualValue);
                        revenueGoal = ProjectedTrendList.Sum(data => data.Value);

                        TacticCostData = GetActualCostDataByWeightage(customFieldId, _ParentId.ToString(), CustomFieldType, fltrTacticData, tblTacticLineItemList, tblLineItemActualList, IsTacticCustomField);
                        CurrentMonthCostList = TacticCostData.Where(actual => IncludeCurrentMonth.Contains(actual.Month)).ToList();
                        List<TacticMonthValue> ProjectedDatatable = new List<TacticMonthValue>();
                        ProjectedDatatable = GetProjectedCostData(customFieldId, _ParentId.ToString(), CustomFieldType, fltrTacticData, IsTacticCustomField,lstTacticLineItem,tblLineItemCost,tblTacticCostList);
                        innerCurrentMonthCostList = CurrentMonthCostList;
                        costActual = innerCurrentMonthCostList.Sum(innercost => innercost.Value);
                        costGoal = ProjectedDatatable.Sum(innergoalcost => innergoalcost.Value);

                    }
                    else
                    {

                        inneractuallist = ActualTacticTrendList.Where(actual => _ChildIdsList.Contains(actual.PlanTacticId)).ToList();
                        List<ProjectedTrendModel> innerGoallist = new List<ProjectedTrendModel>();
                        innerGoallist = ProjectedTrendList.Where(actual => _ChildIdsList.Contains(actual.PlanTacticId)).ToList();

                        revenueActual = inneractuallist.Sum(data => data.TrendValue);
                        revenueGoal = innerGoallist.Sum(data => data.Value);


                        innerCurrentMonthCostList = CurrentMonthCostList.Where(cost => _ChildIdsList.Contains(cost.Id)).ToList();

                        List<TacticMonthValue> ProjectedDatatable = new List<TacticMonthValue>();
                        ProjectedDatatable = GetProjectedCostData(customFieldId, _ParentId.ToString(), CustomFieldType, fltrTacticData, IsTacticCustomField, lstTacticLineItem, tblLineItemCost, tblTacticCostList);

                        costActual = innerCurrentMonthCostList.Sum(innercost => innercost.Value);
                        costGoal = ProjectedDatatable.Sum(innergoalcost => innergoalcost.Value);

                    }
                    //foreach (int _ChildId in _ChildIdsList) //Add By Nishant Sheth
                    //{
                        //objCardSection.FieldId = _ChildIdsList.FirstOrDefault();        // Set Child Id: Card Item(Campaign, Program, Tactic or CustomfieldOption) Id. By Nishant Sheth
                        //objCardSection.ChildId = _ChildId;

                        #region "Calculate Revenue"
                        objCardSectionSubModel = new CardSectionListSubModel();

                        // Start Revenue CardSection SubModel Data

                        objCardSectionSubModel.CardType = Enums.TOPRevenueType.Revenue.ToString();

                        objCardSectionSubModel.Actual_Projected = revenueActual;
                        objCardSectionSubModel.Goal = revenueGoal;

                        ProjvsGoal = objCardSectionSubModel.Goal != 0 ? ((objCardSectionSubModel.Actual_Projected - objCardSectionSubModel.Goal) / objCardSectionSubModel.Goal) : 0;
                        Percentage = ProjvsGoal * 100; // Calculate Percentage based on Actual_Projected & Goal value.
                        if (Percentage > 0)
                            objCardSectionSubModel.IsNegative = false;
                        else
                            objCardSectionSubModel.IsNegative = true;
                        objCardSectionSubModel.Percentage = Percentage;

                        // End Revenue CardSection SubModel Data

                        objCardSection.RevenueCardValues = objCardSectionSubModel;
                        #endregion

                        #region "Calculate Cost"
                        // Start Cost CardSection SubModel Data
                        objCardSectionSubModel = new CardSectionListSubModel();
                        objCardSectionSubModel.CardType = Enums.TOPRevenueType.Cost.ToString();
                        objCardSectionSubModel.Actual_Projected = costActual;

                        objCardSectionSubModel.Goal = costGoal;
                        ProjvsGoal = objCardSectionSubModel.Goal != 0 ? ((objCardSectionSubModel.Actual_Projected - objCardSectionSubModel.Goal) / objCardSectionSubModel.Goal) : 0;
                        Percentage = ProjvsGoal * 100; // Calculate Percentage based on Actual_Projected & Goal value.
                        if (Percentage > 0)
                            objCardSectionSubModel.IsNegative = false;
                        else
                            objCardSectionSubModel.IsNegative = true;
                        objCardSectionSubModel.Percentage = Percentage;
                        objCardSection.CostCardValues = objCardSectionSubModel;
                        // End Cost CardSection SubModel Data

                        #endregion

                        #region "Calculate ROI"
                        // Start ROI CardSection SubModel Data
                        objCardSectionSubModel = new CardSectionListSubModel();
                        objCardSectionSubModel.CardType = Enums.TOPRevenueType.ROI.ToString();
                        objCardSectionSubModel.Actual_Projected = (costActual) != 0 ? ((((revenueActual) - (costActual)) / (costActual)) * 100) : 0;

                        objCardSectionSubModel.Goal = (costGoal) != 0 ? ((((revenueGoal) - (costGoal)) / (costGoal)) * 100) : 0; ;
                        ProjvsGoal = objCardSectionSubModel.Goal != 0 ? ((objCardSectionSubModel.Actual_Projected - objCardSectionSubModel.Goal) / objCardSectionSubModel.Goal) : 0;
                        Percentage = ProjvsGoal * 100; // Calculate Percentage based on Actual_Projected & Goal value.
                        if (Percentage > 0)
                            objCardSectionSubModel.IsNegative = false;
                        else
                            objCardSectionSubModel.IsNegative = true;
                        objCardSectionSubModel.Percentage = Percentage;

                        objCardSection.ROICardValues = objCardSectionSubModel;
                        // End ROI CardSection SubModel Data

                        #endregion

                        #region "line chart"
                        BasicModel _basicmodel = GetCardValuesListByTimeFrame(inneractuallist, innerCurrentMonthCostList, timeframeOption);
                        objLineChartData = new lineChartData();
                        objLineChartData = GetCardLineChartData(_basicmodel, timeframeOption);
                        objCardSection.LineChartData = objLineChartData;

                        #endregion
                        // Add Multiple fixed same values to Model
                        objCardSectionList.Add(objCardSection);


                }

                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objCardSectionList;
        }

        #region"Card LineChartData method -dashrath"
        public BasicModel GetCardValuesListByTimeFrame(List<ActualTrendModel> ActualTrendList, List<TacticMonthValue> CurrentMonthCostList, string timeframeOption)
        {
            BasicModel objBasicModel = new BasicModel();
            int categorieslength = 4;
            List<double> _actuallist = new List<double>();
            List<double> _costlist = new List<double>();
            List<string> _curntQuarterList = new List<string>();
            List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };
            double _Actual, _cost = 0;
            List<string> categories = new List<string>();
            categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
            int currentEndMonth = 12;
            if (currentYear == timeframeOption)
            {
                currentEndMonth = DateTime.Now.Month;
            }

            foreach (var item in CurrentMonthCostList)
            {

                string _monthval = item.Month;
                _monthval = _monthval.Replace(timeframeOption, "");
                item.Month = _monthval;
            }
            for (int i = 1; i <= categorieslength; i++)
            {
                #region "Get Quarter list based on loop value"
                if (i == 1)
                {
                    _curntQuarterList = Q1;
                }
                else if (i == 2)
                {
                    _curntQuarterList = Q2;
                }
                else if (i == 3)
                {
                    _curntQuarterList = Q3;
                }
                else if (i == 4)
                {
                    _curntQuarterList = Q4;
                }
                #endregion

                _Actual = ActualTrendList.Where(actual => _curntQuarterList.Contains(actual.Month)).Sum(actual => actual.TrendValue);
                _actuallist.Add(_Actual);

                _cost = CurrentMonthCostList.Where(costactual => _curntQuarterList.Contains(costactual.Month)).Sum(costactual => costactual.Value);
                _costlist.Add(_cost);
            }
            objBasicModel.Categories = categories;
            objBasicModel.ActualList = _actuallist;
            objBasicModel.CostList = _costlist;
            return objBasicModel;

        }

        public lineChartData GetCardLineChartData(BasicModel objBasicModel, string timeframeOption)
        {
            List<string> categories = new List<string>();
            categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
            #region "Declare Local Varialbles"
            List<series> lstseries = new List<series>();
            lineChartData LineChartData = new lineChartData();
            bool IsDisplay = false, IsQuarterly = true;
            List<double?> serData1 = new List<double?>();
            List<double?> serData2 = new List<double?>();
            double TodayValue = 0, catLength = 0;
            string curntPeriod = string.Empty, currentYear = DateTime.Now.Year.ToString();
            #endregion
            try
            {

                #region "Get Today Plot Value"
                if (currentYear == timeframeOption)
                {
                    IsDisplay = true;
                    TodayValue = GetTodayPlotValue(timeframeOption, IsQuarterly);
                }
                #endregion

                #region "Get Series list"

                if (objBasicModel == null)
                    return LineChartData;
                catLength = objBasicModel.Categories.Count;   // Set categories list count.

                #region "Quarterly Calculate ActualReveneue & ActualCost"

                double _Actual = 0, _Cost = 0, _totActul = 0, _totalcost = 0;

                for (int i = 0; i < catLength; i++)
                {
                    _Actual = objBasicModel.ActualList[i] != null ? objBasicModel.ActualList[i] : 0;
                    _totActul = _totActul + _Actual;
                    _Cost = objBasicModel.CostList[i] != null ? objBasicModel.CostList[i] : 0;
                    _totalcost = _totalcost + _Cost;
                    serData1.Add(_totActul);
                    serData2.Add(_totalcost);
                }

                series objSeries1 = new series();
                objSeries1.name = "Revenue";
                objSeries1.data = serData1;
                marker objMarker1 = new marker();
                objMarker1.symbol = "circle";
                objSeries1.marker = objMarker1;

                series objSeries2 = new series();
                objSeries2.name = "Cost";
                objSeries2.data = serData2;
                marker objMarker2 = new marker();
                objMarker2.symbol = "circle";
                objSeries2.marker = objMarker2;

                lstseries.Add(objSeries1);
                lstseries.Add(objSeries2);
                #endregion
                #endregion
                #region "Set Series, Categories & Marker data to Model"
                categories = objBasicModel.Categories != null ? objBasicModel.Categories : new List<string>();
                LineChartData.categories = categories;
                LineChartData.series = lstseries;
                // Set IsDisplay & TodayValue to Plot line on Linechart graph.
                LineChartData.isDisplay = IsDisplay.ToString();
                LineChartData.todayValue = TodayValue.ToString();
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return LineChartData;
            //return Json(RevenueLineChartData, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult LoadReportCardSectionPartial(string ParentLabel = "", string childlabelType = "", string childId = "", string option = "")
        {
            ViewBag.ParentLabel = ParentLabel;
            ViewBag.childlabelType = childlabelType;
            ViewBag.childId = childId;
            ViewBag.option = option;

            CardSectionModel cardModel = new CardSectionModel();
            cardModel = (CardSectionModel)TempData["CardData"];
            return PartialView("_ReportCardSection", cardModel);
        }
        public ActionResult LoadConverstionCardSectionPartial(string ParentLabel = "", string childlabelType = "", string childId = "", string option = "")
        {
            ViewBag.ConvParentLabel = ParentLabel;
            ViewBag.ConvchildlabelType = childlabelType;
            ViewBag.ConvchildId = childId;
            ViewBag.Convoption = option;

            CardSectionModel cardModel = new CardSectionModel();
            cardModel = (CardSectionModel)TempData["ConversionCard"];
            return PartialView("_ConversionCardSection", cardModel);
        }
        //#endregion

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Program List.
        /// </summary>
        /// <param name="id"> Id</param>
        /// <returns>Returns Json Result of Program List.</returns>
        public JsonResult LoadCampaignDropDownDynamic(string marsterCustomField = "", int masterCustomFieldOptionId = 0)
        {
            // Modified by Arpita Soni  for Ticket #1148 on 01/28/2014
            List<Plan_Campaign_Program_Tactic> TacticList = GetTacticForReporting();
            List<int> campaignIds = new List<int>();
            campaignIds = TacticList.Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).Distinct().ToList<int>();
            if (masterCustomFieldOptionId > 0)
            {
                if (marsterCustomField.Contains(Common.CampaignCustomTitle))
                {
                    int customfieldId = Convert.ToInt32(marsterCustomField.Replace(Common.CampaignCustomTitle, ""));
                    string customfiledvalue = Convert.ToString(masterCustomFieldOptionId);
                    campaignIds = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && campaignIds.Contains(c.EntityId) && c.Value == customfiledvalue).Select(c => c.EntityId).ToList();
                }
            }

            var campaignList = db.Plan_Campaign.Where(pc => campaignIds.Contains(pc.PlanCampaignId))
                    .Select(campaign => new { PlanCampaignId = campaign.PlanCampaignId, Title = campaign.Title })
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
        public JsonResult LoadProgramDropDownDynamic(string id, string marsterCustomField = "", int masterCustomFieldOptionId = 0)
        {
            // Modified by Arpita Soni  for Ticket #1148 on 01/28/2014
            List<Plan_Campaign_Program_Tactic> TacticList = GetTacticForReporting();
            List<Plan_Campaign_Program> ProgramList = new List<Plan_Campaign_Program>();
            if (id != null && id != "")
            {
                int campaignid = Convert.ToInt32(id);
                ProgramList = TacticList.Where(tactic => tactic.Plan_Campaign_Program.PlanCampaignId == campaignid).Select(tactic => tactic.Plan_Campaign_Program).Distinct().ToList();
            }
            else
            {
                ProgramList = TacticList.Select(tactic => tactic.Plan_Campaign_Program).Distinct().ToList();
            }
            if (masterCustomFieldOptionId > 0)
            {
                if (marsterCustomField.Contains(Common.CampaignCustomTitle))
                {
                        int customfieldId = Convert.ToInt32(marsterCustomField.Replace(Common.CampaignCustomTitle, ""));
                        List<int> campaignIds = new List<int>();
                        campaignIds = ProgramList.Select(p => p.PlanCampaignId).Distinct().ToList();
                        string customfiledvalue = Convert.ToString(masterCustomFieldOptionId);
                        campaignIds = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && campaignIds.Contains(c.EntityId) && c.Value == customfiledvalue).Select(c => c.EntityId).ToList();
                        ProgramList = ProgramList.Where(p => campaignIds.Contains(p.PlanCampaignId)).ToList();
                }
                else if (marsterCustomField.Contains(Common.ProgramCustomTitle))
                {
                        int customfieldId = Convert.ToInt32(marsterCustomField.Replace(Common.ProgramCustomTitle, ""));
                        List<int> programIds = new List<int>();
                        programIds = ProgramList.Select(p => p.PlanProgramId).Distinct().ToList();
                        string customfiledvalue = Convert.ToString(masterCustomFieldOptionId);
                        programIds = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && programIds.Contains(c.EntityId) && c.Value == customfiledvalue).Select(c => c.EntityId).ToList();
                        ProgramList = ProgramList.Where(p => programIds.Contains(p.PlanProgramId)).ToList();
                }
            }

            var programListfinal = ProgramList.Select(program => new { PlanProgramId = program.PlanProgramId, Title = program.Title })
                    .OrderBy(pcp => pcp.Title).ToList();
            if (programListfinal == null)
                return Json(new { });
            programListfinal = programListfinal.Where(program => !string.IsNullOrEmpty(program.Title)).OrderBy(program => program.Title, new AlphaNumericComparer()).ToList();
            return Json(programListfinal, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Tactic List.
        /// </summary>
        /// <param name="id"> Id</param>
        /// <returns>Returns Json Result of Tactic List.</returns>
        public JsonResult LoadTacticDropDownDynamic(string id, string type = "", string marsterCustomField = "", int masterCustomFieldOptionId = 0)
        {
            List<Plan_Campaign_Program_Tactic> TacticList = GetTacticForReporting();

            // Modified by Arpita Soni  for Ticket #1148 on 01/28/2014
            if (id != null && id != "")
            {
                if (type == Common.RevenueCampaign)
                {
                    int campaignid = Convert.ToInt32(id);
                    TacticList = TacticList.Where(t => t.Plan_Campaign_Program.PlanCampaignId == campaignid).ToList();
                }
                else
                {
                    int programid = Convert.ToInt32(id);
                    TacticList = TacticList.Where(t => t.PlanProgramId == programid).ToList();
                }
            }
            if (masterCustomFieldOptionId > 0)
            {
                if (marsterCustomField.Contains(Common.CampaignCustomTitle))
                {
                        int customfieldId = Convert.ToInt32(marsterCustomField.Replace(Common.CampaignCustomTitle, ""));
                        List<int> campaignIds = new List<int>();
                        campaignIds = TacticList.Select(p => p.Plan_Campaign_Program.PlanCampaignId).Distinct().ToList();
                        string customfiledvalue = Convert.ToString(masterCustomFieldOptionId);
                        campaignIds = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && campaignIds.Contains(c.EntityId) && c.Value == customfiledvalue).Select(c => c.EntityId).ToList();
                        TacticList = TacticList.Where(t => campaignIds.Contains(t.Plan_Campaign_Program.PlanCampaignId)).ToList();
                }
                else if (marsterCustomField.Contains(Common.ProgramCustomTitle))
                {
                        int customfieldId = Convert.ToInt32(marsterCustomField.Replace(Common.ProgramCustomTitle, ""));
                        List<int> programIds = new List<int>();
                        programIds = TacticList.Select(p => p.PlanProgramId).Distinct().ToList();
                        string customfiledvalue = Convert.ToString(masterCustomFieldOptionId);
                        programIds = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && programIds.Contains(c.EntityId) && c.Value == customfiledvalue).Select(c => c.EntityId).ToList();
                        TacticList = TacticList.Where(t => programIds.Contains(t.PlanProgramId)).ToList();
                }
                else if (marsterCustomField.Contains(Common.TacticCustomTitle))
                {
                        int customfieldId = Convert.ToInt32(marsterCustomField.Replace(Common.TacticCustomTitle, ""));
                        List<int> tacticIds = new List<int>();
                        tacticIds = TacticList.Select(p => p.PlanTacticId).Distinct().ToList();
                        string customfiledvalue = Convert.ToString(masterCustomFieldOptionId);
                        tacticIds = db.CustomField_Entity.Where(c => c.CustomFieldId == customfieldId && tacticIds.Contains(c.EntityId) && c.Value == customfiledvalue).Select(c => c.EntityId).ToList();
                        TacticList = TacticList.Where(t => tacticIds.Contains(t.PlanTacticId)).ToList();
                }
            }

            var tacticListinner = TacticList
                   .Select(tactic => new { PlanTacticId = tactic.PlanTacticId, Title = tactic.Title })
                   .OrderBy(pcp => pcp.Title).ToList();
            if (tacticListinner == null)
                return Json(new { });
            tacticListinner = tacticListinner.Where(tactic => !string.IsNullOrEmpty(tactic.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
            return Json(tacticListinner, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }

}

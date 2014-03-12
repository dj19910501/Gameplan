using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Models
{
    public class SummaryReportModel
    {
        //For Revenue Summary
        public string PlanStatus { get; set; } //(Below or Above or At-Par) Plan

        //// Modified By: Maninder Singh Wadhva to address TFS Bug 296:Close and realize numbers in Revenue Summary are incorrectly calculated.
        public string ProjectedRevenue { get; set; }
        public double MQLs { get; set; }
        public string Revenue { get; set; }
        public string MQLsPercentage { get; set; }
        public string RevenuePercentage { get; set; }

        //For Waterfall Conversion Summary
        public string OverallConversionPlanStatus { get; set; } //(Below or Above or At-Par) Plan
        public string ISQsStatus { get; set; }
        public string MQLsStatus { get; set; }
        public string CWsStatus { get; set; }
        public List<WaterfallConversionSummaryChart> chartData = new List<WaterfallConversionSummaryChart>();

        /* To resolve Bug 312: Report plan selector needs to be moved */
        
        //For BusinessUnit Selector / Plans selector
        public List<SelectListItem> lstBusinessUnit { get; set; }
        public List<SelectListItem> lstAllPlans { get; set; }

        /* To resolve Bug 312: Report plan selector needs to be moved */
    }

    public class WaterfallConversionSummaryChart
    {
        public string Actual { get; set; }
        public string Projected { get; set; }
        public string Stage { get; set; }
    }

    public class FilterDropdownValues
    {
        //For BusinessUnit Selector / Plans selector
        public List<SelectListItem> lstBusinessUnit { get; set; }
        public List<SelectListItem> lstAllPlans { get; set; }

    }

    /// <summary>
    /// Class of TacticId & Projected Revenue Calculated.
    /// </summary>
    public class ProjectedRevenueClass
    {
        public int PlanTacticId { get; set; }
        public double ProjectedRevenue { get; set; }
    }
}
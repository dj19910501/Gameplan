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
        public double ProjectedRevenue { get; set; }
        public double MQLs { get; set; }
        public string Revenue { get; set; }
        public double MQLsPercentage { get; set; }
        public double RevenuePercentage { get; set; }

        //For Waterfall Conversion Summary
        public string OverallConversionPlanStatus { get; set; } //(Below or Above or At-Par) Plan
        public string ISQsStatus { get; set; }
        public double INQPerValue { get; set; }
        public string MQLsStatus { get; set; }
        public double MQLPerValue { get; set; }
        public string CWsStatus { get; set; }
        public List<WaterfallConversionSummaryChart> chartData = new List<WaterfallConversionSummaryChart>();

        /* To resolve Bug 312: Report plan selector needs to be moved */
        
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
    
    #region "Report"
    
    #region "Revenue"
    #region "LineChart Entity"
    public class Projected_Goal
    {
        public string Name { get; set; } /// this properties used in conversion page.
        public string year { get; set; }
        public string Actual_Projected { get; set; }
        public string Goal { get; set; }
        public string Percentage { get; set; }
        public bool IsnegativePercentage { get; set; }
    }
    public class lineChartData
    {
        public List<string> categories { get; set; }
        public List<series> series { get; set; }
        public string isDisplay { get; set; }
        public string todayValue { get; set; }
    }
    public class series
    {
        public string name { get; set; }
        public List<double> data { get; set; }
        public marker marker { get; set; }
    }
    public class marker
    {
        public string symbol { get; set; }
    }
    public class ReportOverviewModel
    { 
        public RevenueOverviewModel revenueOverviewModel { get; set; }
        public ConversionOverviewModel conversionOverviewModel { get; set; }
    }
    public class RevenueOverviewModel
    {
        public lineChartData linechartdata { get; set; }
        public Projected_Goal projected_goal { get; set; }
        public List<sparkLineCharts> SparkLineChartsData { get; set; }
    }
    #endregion

    #region "Sparkline chart Entity"
    public class sparkLineCharts
    {
        public string sparklinechartId { get; set; }
        public string ChartHeader { get; set; }
        public string CustomfieldDDLId { get; set; }
        public List<sparklineData> sparklinechartdata { get; set; }
        //public bool IsRevenue { get; set; }
        public bool IsOddSequence { get; set; }
        public Helpers.Enums.TOPRevenueType TOPRevenueType { get; set; }
        public List<string> RevenueTypeColumns { get; set; }
        //public TOPRevenueTypeColumns RevenueTypeColumns { get; set; }
    }
    public class sparklineData
    {
        public string Name { get; set; }
        public string RevenueTypeValue { get; set; }
        public bool IsPositive { get; set; }
        public string Trend { get; set; }
        public bool IsTotal { get; set; }
        public bool IsPercentage { get; set; }
        public bool Is_Pos_Neg_Status { get; set; }
        public double Value { get; set; }
        public string Tooltip_Prefix { get; set; }
        public string Tooltip_Suffix { get; set; }
    }
    #endregion  
    
    #endregion

    #region"Conversion"
    public class ConversionOverviewModel
    {
        public List<conversion_Projected_Goal_LineChart> Projected_LineChartList { get; set; }
    }
    public class conversion_Projected_Goal_LineChart
    {
        public string StageCode { get; set; }
        public lineChartData linechartdata { get; set; }
        public Projected_Goal projected_goal { get; set; }
        public Conversion_Benchmark_Model Stage_Benchmark { get; set; }
    }
    public class Conversion_Benchmark_Model
    {
        public string stagename { get; set; }
        public string stageVolume { get; set; }
        public string Benchmark { get; set; }
        //public string PercentageDifference { get; set; }
     }
    public class Stage_Benchmark
    {
        public string StageCode { get; set;}
        public double Benchmark { get; set; }
    }
    #endregion

    public class ActualDataTable
    {
        public int PlanTacticId { get; set; }
        public string Period { get; set; }
        public string StageTitle { get; set; }
        public double ActualValue { get; set; }
    }

    public class ProjectedTrendModel
    {
        public int PlanTacticId { get; set; }
        public int NoTacticMonths { get; set; }
        public double Value { get; set; }
        public double TrendValue { get; set; }
        public string Month { get; set; }
    }

    public class ActualTrendModel
    {
        public int PlanTacticId { get; set; }
        public double TrendValue { get; set; }
        public string Month { get; set; }
        public string StageCode { get; set; }
    }

    public class TacticMonthInterval
    {
        public int PlanTacticId { get; set; }
        public string Month { get; set; }
    }

    public class TacticwiseOverviewModel
    {
        public int PlanTacticId { get; set; }
        public double Actual_ProjectedValue { get; set; }
        public double Goal { get; set; }
        public double Percentage { get; set; }
    }

    public class ProjectedTacticModel
    {
        public int TacticId { get; set; }
        public int StartMonth { get; set; }
        public int EndMonth { get; set; }
        public double Value { get; set; }
        public int Year { get; set; }
    }

    public class ActualTacticListByStage
    {
        public List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList { get; set; }
        public string StageCode { get; set;}
    }

    #endregion
}
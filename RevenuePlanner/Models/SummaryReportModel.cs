using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Models
{
    /// <summary>
    /// Class of TacticId & Projected Revenue Calculated.
    /// </summary>
    public class ProjectedRevenueClass
    {
        public int PlanTacticId { get; set; }
        public double ProjectedRevenue { get; set; }
    }

    #region "Report"

    #region "Revenue Overview"
    #region "LineChart Entity"
    public class Projected_Goal
    {
        public string Name { get; set; } /// this properties used in conversion page.
        public string year { get; set; }
        public string Actual_Projected { get; set; }
        public string Goal { get; set; }
        public string Percentage { get; set; }
        public bool IsnegativePercentage { get; set; }
        public string Projected { get; set; }
        public string GoalYTD { get; set; }// Add BY Nishant Sheth #1397
        public string GoalYear { get; set; }// Add BY Nishant Sheth #1397
        public string ActualPercentage { get; set; }// Add BY Nishant Sheth #1397
        public bool ActualPercentageIsnegative { get; set; }// Add BY Nishant Sheth #1397
        public string ProjectedPercentage { get; set; }// Add BY Nishant Sheth #1397
        public bool ProjectedPercentageIsnegative { get; set; }// Add BY Nishant Sheth #1397
    }
    public class lineChartData
    {
        public List<string> categories { get; set; }
        public List<series> series { get; set; }
        public string isDisplay { get; set; }
        public string todayValue { get; set; }
        public double pointLabelWidth { get; set; }

    }
    public class series
    {
        public string name { get; set; }
        public List<double?> data { get; set; }
        public marker marker { get; set; }
        private bool ShowInLegend = true;
        public bool showInLegend
        {
            get
            {
                return ShowInLegend;
            }
            set
            {
                ShowInLegend = value;
            }
        }
        public bool shadow { get; set; }
    }
    public class marker
    {
        public string symbol { get; set; }
    }
    public class ReportOverviewModel
    {
        public RevenueOverviewModel revenueOverviewModel { get; set; }
        public ConversionOverviewModel conversionOverviewModel { get; set; }
        public FinancialOverviewModel financialOverviewModel { get; set; }
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
        public string PercentageDifference { get; set; }
        public bool IsNegativePercentage { get; set; }
    }
    public class Stage_Benchmark
    {
        public string StageCode { get; set; }
        public double Benchmark { get; set; }
    }

    public class ConversionOverviewToPlanModel
    {
        public List<ConversionToPlanModel> conversionOverviewPlanModel { get; set; }
    }
    public class ConversionToPlanModel
    {
        public ConversionOverviewModel conversionOverviewModel { get; set; }
        public ConversionDataTable ConversionToPlanDataTableModel { get; set; }
        public BarChartModel ConversionToPlanBarChartModel { get; set; }
        public lineChartData LineChartModel { get; set; }//dashrath
        public Projected_Goal RevenueHeaderModel { get; set; } // Add By Nishant SHeth
    }
    public class ConversionDataTable
    {
        public List<string> Categories { get; set; }
        public List<double> ActualList { get; set; }
        public List<double> ProjectedList { get; set; }
        public List<double> GoalList { get; set; }
        //public List<double> CostList { get; set; }
        //public List<double> PerformanceList { get; set; }
        //public List<double> ROIList { get; set; }
        public ConversionSubDataTableModel SubDataModel { get; set; }
        public List<double> TotalRevenueList { get; set; }
        public bool IsQuarterly { get; set; }
        public string timeframeOption { get; set; }
    }

    public class ConversionSubDataTableModel
    {
        public List<string> PerformanceList { get; set; }
        public List<string> CostList { get; set; }
        public List<string> ROIList { get; set; }
        public List<string> RevenueList { get; set; }
    }
    #endregion

    #region "Financial Overview"
    public class FinancialOverviewModel
    {
        public double TotalBudgetAllocated { get; set; }
        public double TotalBudgetUnAllocated { get; set; }
        public double PlannedCostvsBudget { get; set; }
        public double ActualCostvsBudet { get; set; }
        public BarChartModel PlannedCostBarChartModel { get; set; }
        public BarChartModel ActualCostBarChartModel { get; set; }
        public BarChartModel MainBarChartModel { get; set; }
        public List<double> MainPlannedCostList { get; set; }
        public List<double> MainActualCostList { get; set; }
        public List<double> MainBudgetCostList { get; set; }
        public int CategoriesCount { get; set; }
    }
    #endregion

    #region "Revenue"

    public class ReportModel
    {
        public double Actual_Projected { get; set; }
        public double Goal { get; set; }
        public Projected_Goal RevenueHeaderModel { get; set; }
        public lineChartData RevenueLineChartModel { get; set; }
        public RevenueToPlanModel RevenueToPlanModel { get; set; }
        public Projected_Goal ConversionHeaderModel { get; set; } //dashrath
        public ConversionOverviewModel conversionOverviewModel { get; set; }//dashrath
        public ConversionToPlanModel ConversionToPlanModel { get; set; }//dashrath
        public CardSectionModel CardSectionModel { get; set; }
    }
    public class RevenueToPlanModel
    {
        public BarChartModel RevenueToPlanBarChartModel { get; set; }
        public RevenueDataTable RevenueToPlanDataModel { get; set; }
        public lineChartData LineChartModel { get; set; }
        public CardSectionModel CardSectionModel { get; set; }
        public Projected_Goal RevenueHeaderModel { get; set; } // Add By Nishant SHeth
    }
    public class RevenueDataTable
    {
        public List<string> Categories { get; set; }
        public List<double> ActualList { get; set; }
        public List<double> ProjectedList { get; set; }
        public List<double> GoalList { get; set; }
        //public List<double> CostList { get; set; }
        //public List<double> PerformanceList { get; set; }
        //public List<double> ROIList { get; set; }
        public RevenueSubDataTableModel SubDataModel { get; set; }
        public List<double> TotalRevenueList { get; set; }
        public bool IsQuarterly { get; set; }
        public string timeframeOption { get; set; }
    }

    public class RevenueSubDataTableModel
    {
        public List<string> PerformanceList { get; set; }
        public List<string> CostList { get; set; }
        public List<string> ROIList { get; set; }
        public List<string> RevenueList { get; set; }
    }


    #endregion

    #region "Basic Model"
    public class ActualDataTable
    {
        public int PlanTacticId { get; set; }
        public string Period { get; set; }
        public string StageTitle { get; set; }
        public double ActualValue { get; set; }
        public int PlanYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ProjectedTrendModel
    {
        public int PlanTacticId { get; set; }
        public int NoTacticMonths { get; set; }
        public double Value { get; set; }
        public double TrendValue { get; set; }
        public string Month { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Year { get; set; }
    }

    public class ActualTrendModel
    {
        public int PlanTacticId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Trend { get; set; }
        public double Value { get; set; }
        public string Month { get; set; }
        public string StageCode { get; set; }
        public int Year { get; set; }
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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double VeloCity { get; set; }
    }

    public class ActualTacticListByStage
    {
        public List<Plan_Campaign_Program_Tactic_Actual> ActualTacticList { get; set; }
        public string StageCode { get; set; }
    }

    public class BarChartModel
    {
        public List<string> categories { get; set; }
        public List<BarChartSeries> series { get; set; }
        public List<BarChartSeriesScatter> scatterdata { get; set; }
        public double plotBandFromValue { get; set; }
        public double plotBandToValue { get; set; }
    }

    public class BarChartSeries
    {
        public string name { get; set; }
        public List<double> data { get; set; }
        public string type { get; set; }
    }
    public class BarChartSeriesScatter
    {
        public List<double> data { get; set; }
        public string type { get; set; }
    }
    public class TacticActualCostModel
    {
        public int PlanTacticId { get; set; }
        public bool IsLineItemExist { get; set; }
        public List<BudgetedValue> ActualList { get; set; }
    }

    public class BasicModel
    {
        public List<string> Categories { get; set; }
        public List<double> ActualList { get; set; }
        public List<double> ProjectedList { get; set; }
        public List<double> GoalList { get; set; }
        public List<double> CostList { get; set; }//cost list added by dashrath
        public bool IsQuarterly { get; set; }
        public string timeframeOption { get; set; }
        public List<double> GoalYTD { get; set; }// Add By Nishant Sheth
    }

    public class CardSectionModel
    {
        public List<CardSectionListModel> CardSectionListModel { get; set; }
        public int TotalRecords { get; set; } // Add By Nishant Sheth
        public int CuurentPageNum { get; set; }// Add By Nishant Sheth
        //public List<Projected_Goal> RevenueHeaderModel { get; set; } // Add By Nishant SHeth
    }
    public class RevenueCardList
    {
        public static List<CardSectionListModel> CardSectionListModel { get; set; }
    }
    public class CardSectionListModel
    {
        public string title { get; set; }
        public CardSectionListSubModel INQCardValues { get; set; }//dashrath
        public CardSectionListSubModel TQLCardValues { get; set; }//dashrath
        public CardSectionListSubModel CWCardValues { get; set; }//dashrath
        public CardSectionListSubModel ADSCardValues { get; set; }//dashrath

        public CardSectionListSubModel RevenueCardValues { get; set; }
        public CardSectionListSubModel CostCardValues { get; set; }
        public CardSectionListSubModel ROICardValues { get; set; }
        public CardSectionListSubModel CostPackageCardValues { get; set; }// Add By Nishant Sheth
        public CardSectionListSubModel ROIPackageCardValues { get; set; }// Add By Nishant Sheth
        public lineChartData LineChartData { get; set; }
        public string MasterParentlabel { get; set; }
        public double FieldId { get; set; }
        public string FieldType { get; set; }
        public Projected_Goal RevenueHeaderModel { get; set; } // Add By Nishant SHeth
        public string RoiPackageTitle { get; set; } // Add By Nishant Sheth
        public bool IsPackage { get; set; } // Add By Nishant Sheth
        public int AnchorTacticId { get; set; } // Add By Nishant Sheth
    }
    public class CardSectionListSubModel
    {
        public string CardType { get; set; }
        public double Actual_Projected { get; set; }
        public double Goal { get; set; }
        public double Percentage { get; set; }
        //public double RestPercentage { get; set; }
        public double ConversePercentage { get; set; }//dashrath
        public bool? IsNegative { get; set; }
    }
    public class TacticMappingItem
    {
        public int ParentId { get; set; }
        public string ParentTitle { get; set; }
        public int ChildId { get; set; }
    }
    /// <summary>
    /// Add By Nishant Sheth
    /// #2376 Get/set the values for ROI Package Card section
    /// </summary>
    public class ROIPackageCardTacticData
    {
        public List<ActualTrendModel> ActualTacticTrendList { get; set; }
        public List<ProjectedTrendModel> ProjectedTrendList { get; set; }
        public List<TacticStageValue> TacticData { get; set; }
        public Dictionary<int, string> ROIAnchorTactic { get; set; }
    }
    #endregion

    // Add By Nishant Sheth
    public class MultiYearModel
    {
        public int EntityId { get; set; }
        public int Year { get; set; }
        public string Period { get; set; }
        public double Value { get; set; }
    }
    #endregion
}
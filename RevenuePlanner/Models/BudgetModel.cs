using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class BudgetMonth
    {
        public double BudgetY1 { get; set; } //Q1
        public double BudgetY2 { get; set; }
        public double BudgetY3 { get; set; }
        public double BudgetY4 { get; set; } //Q2
        public double BudgetY5 { get; set; }
        public double BudgetY6 { get; set; }
        public double BudgetY7 { get; set; } //Q3
        public double BudgetY8 { get; set; }
        public double BudgetY9 { get; set; }
        public double BudgetY10 { get; set; } //Q4
        public double BudgetY11 { get; set; }
        public double BudgetY12 { get; set; }

        public double CostY1 { get; set; } //Q1
        public double CostY2 { get; set; }
        public double CostY3 { get; set; }
        public double CostY4 { get; set; } //Q2
        public double CostY5 { get; set; }
        public double CostY6 { get; set; }
        public double CostY7 { get; set; } //Q3
        public double CostY8 { get; set; }
        public double CostY9 { get; set; }
        public double CostY10 { get; set; } //Q4
        public double CostY11 { get; set; }
        public double CostY12 { get; set; }

        public double ActualY1 { get; set; } //Q1
        public double ActualY2 { get; set; }
        public double ActualY3 { get; set; }
        public double ActualY4 { get; set; } //Q2
        public double ActualY5 { get; set; }
        public double ActualY6 { get; set; }
        public double ActualY7 { get; set; } //Q3
        public double ActualY8 { get; set; }
        public double ActualY9 { get; set; }
        public double ActualY10 { get; set; } //Q4
        public double ActualY11 { get; set; }
        public double ActualY12 { get; set; }

    }
    public class ViewByModel
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
    public class PlanBudgetModel
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public string ParentActivityId { get; set; }
        public bool IsOwner { get; set; }
        public int CreatedBy { get; set; }
        public BudgetMonth MonthValues { get; set; }
        public BudgetMonth ChildMonthValues { get; set; }
        public BudgetMonth NextYearMonthValues { get; set; }
        public double TotalUnallocatedBudget { get; set; }
        public double YearlyBudget { get; set; }
        public double TotalAllocatedCost { get; set; }
        public double TotalActuals { get; set; }
        public bool isBudgetEditable { get; set; }
        public bool isCostEditable { get; set; }
        public bool isActualEditable { get; set; }
        public List<CustomField_Entity> CustomFieldEntities { get; set; }
        public string CustomFieldType { get; set; }
        public int? LineItemTypeId { get; set; }
        public int Weightage { get; set; }
        public bool isAfterApproved { get; set; }
        public string ColorCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int LinkTacticId { get; set; }
        public int TacticTypeId { get; set; }
        public string MachineName { get; set; }
        public double UnallocatedBudget { get; set; }
        public string PlanYear { get; set; }
        public string AssetType { get; set; }
        public Nullable<int> AnchorTacticID { get; set; }
        public string TaskId { get; set; }
        public double UnallocatedCost { get; set; }
        public string LinkedPlanName { get; set; }
        public string CalendarHoneycombpackageIDs { get; set; }

    }

    public class BudgetModel
    {
        public string Id { get; set; }
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public bool IsOwner { get; set; }

        public BudgetMonth Month { get; set; }
        public BudgetMonth SumMonth { get; set; }
        public BudgetMonth ParentMonth { get; set; }
        public BudgetMonth BudgetMonth { get; set; }

        public double Allocated { get; set; }
        public double Budgeted { get; set; }
        public double MainBudgeted { get; set; }

        public string ParentActivityId { get; set; }
        public List<CustomField_Entity> CustomFieldEntities { get; set; }
        public int Weightage { get; set; }
        public string CustomFieldType { get; set; }
        public bool isEditable { get; set; }
        public int CreatedBy { get; set; }
        public bool isAfterApproved { get; set; }
        public int? LineItemTypeId { get; set; }

        public int LinkTacticId { get; set; }
        public double PlannedCost { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<string, double> MonthlyCosts { get; set; }
    }

    public class BudgetModelReport
    {
        public string Id { get; set; }
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public string ParentActivityId { get; set; }
        public string TabActivityId { get; set; }

        public BudgetMonth MonthPlanned { get; set; }
        public BudgetMonth SumMonthPlanned { get; set; }
        public BudgetMonth ParentMonthPlanned { get; set; }

        public BudgetMonth MonthActual { get; set; }
        public BudgetMonth SumMonthActual { get; set; }
        public BudgetMonth ParentMonthActual { get; set; }

        public BudgetMonth MonthAllocated { get; set; }
        public BudgetMonth SumMonthAllocated { get; set; }
        public BudgetMonth ChildMonthAllocated { get; set; }

        public double Allocated { get; set; }
        public double Budgeted { get; set; }
        public double Actual { get; set; }
        public double Planned { get; set; }
        
        public int? LineItemTypeId { get; set; }
        public int Weightage { get; set; }
        public string CustomFieldType { get; set; }
        public int CustomFieldID { get; set; }
        
    }
    public class BudgetedValue
    {
        public string Period { get; set; }
        public double Value { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class BudgetReportTab
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }
    public class CustomBudgetModel
    {
        public string Id { get; set; }
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public bool IsOwner { get; set; }

        public BudgetMonth Month { get; set; }
        public BudgetMonth SumMonth { get; set; }
        public BudgetMonth ParentMonth { get; set; }

        public double Allocated { get; set; }
        public double Budgeted { get; set; }

        public string ParentActivityId { get; set; }
        List<int> CustomFieldEntities { get; set; }
    }
    public class entCustomFieldOption_EntityMapping
    {
        public int CusotmFieldOptionId { get; set; }
        public List<int> CusotmFieldEntityIds { get; set; }
    }
    //Added By Maitri Gandhi #1852: Convert Finance Report Grid to DHTMLX Tree Grid
    public class BudgetDHTMLXGridModel
    {       
        public string SetHeader { get; set; }
        public string ColType { get; set; }
        public string Width { get; set; }
        public string ColSorting { get; set; }
        public string ColumnIds { get; set; }
        public List<string> AttachHeader { get; set; }
        public string HiddenTab { get; set; }
		public string ColAlign { get; set; } // Added by Arpita Soni for Ticket #2634 on 09/26/2016
        
        public BudgetDHTMLXGrid Grid { get; set; }
    }
    public class BudgetDHTMLXGrid
    {
        public List<BudgetDHTMLXGridDataModel> rows { get; set; }
        public List<BudgetHead> head { get; set; }
    }
    public class BudgetHead
    {
        public List<BudgetOptions> options { get; set; }
        public string value { get; set; }
        public int width { get; set; }
        public string align { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public string sort { get; set; }
    }
    public class BudgetOptions
    {
        public string id { get; set; }
        public string value { get; set; }
    }
    public class BudgetDHTMLXGridDataModel
    {
        public string id { get; set; }
        public string open { get; set; }
        public string bgColor { get; set; }
        public List<Budgetdataobj> data { get; set; }
        //public Planuserdatagrid userdata { get; set; }
        public List<BudgetDHTMLXGridDataModel> rows { get; set; }
    }
    public class Budgetdataobj
    {
        public string value { get; set; }
        public string locked { get; set; }
        public string style { get; set; }
        public string actval { get; set; }
        public string type { get; set; }
    }
}
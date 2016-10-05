using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Models
{
    public class BudgetMonth
    {
        public double Jan { get; set; } //Q1
        public double Feb { get; set; }
        public double Mar { get; set; }
        public double Apr { get; set; } //Q2
        public double May { get; set; }
        public double Jun { get; set; }
        public double Jul { get; set; } //Q3
        public double Aug { get; set; }
        public double Sep { get; set; }
        public double Oct { get; set; } //Q4
        public double Nov { get; set; }
        public double Dec { get; set; }

        public double CJan { get; set; } //Q1
        public double CFeb { get; set; }
        public double CMar { get; set; }
        public double CApr { get; set; } //Q2
        public double CMay { get; set; }
        public double CJun { get; set; }
        public double CJul { get; set; } //Q3
        public double CAug { get; set; }
        public double CSep { get; set; }
        public double COct { get; set; } //Q4
        public double CNov { get; set; }
        public double CDec { get; set; }

        public double AJan { get; set; } //Q1
        public double AFeb { get; set; }
        public double AMar { get; set; }
        public double AApr { get; set; } //Q2
        public double AMay { get; set; }
        public double AJun { get; set; }
        public double AJul { get; set; } //Q3
        public double AAug { get; set; }
        public double ASep { get; set; }
        public double AOct { get; set; } //Q4
        public double ANov { get; set; }
        public double ADec { get; set; }

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
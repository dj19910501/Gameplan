using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace RevenuePlanner.Models
{
    public class FinanceModelHeaders
    {
        public double Budget { get; set; }
        public double Forecast { get; set; }
        public double Planned { get; set; }
        public double Actual { get; set; }
        public string BudgetTitle { get; set; }
        public string ForecastTitle { get; set; }
        public string PlannedTitle { get; set; }
        public string ActualTitle { get; set; }

    }
    public class FinanceModel
    {
        public FinanceModelHeaders FinanemodelheaderObj { get; set; }
        public DhtmlXGridRowModel DhtmlXGridRowModelObj { get; set; }
        public List<UserPermission> Userpermission { get; set; }

    }
    public class UserPermission
    {
        public int budgetID { get; set; }
        public string id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string BusssinessUnit { get; set; }
        public string Region { get; set; }
        public int Permission { get; set; }
        public string createdby { get; set; }
        public bool IsOwner { get; set; }

    }

    // Add By Nishant Sheth
    // Get list of columns and data wtih xml format
    public class ImportData
    {
        public DataTable MarketingBudgetColumns { get; set; }
        public XmlDocument XmlData { get; set; }
        public string ErrorMsg { get; set; }
    }

    // Add By Nishant Sheth
    public class XmlColumns
    {
        public int ColumnIndex { get; set; }
        public string ColumName { get; set; }
    }

    public class UserBudgetPermission
    {
        public int UserId { get; set; }
        public int PermisssionCode { get; set; }
        public int BudgetDetailId { get; set; }
    }
    //public class FinanceParentChildModel
    //{
    //    public Int32 Id { get; set; }

    //    public String Name { get; set; }

    //    public List<double?> Budget { get; set; }

    //    public List<double?> ForeCast { get; set; }

    //    public double BudgetTotal { get; set; }

    //    public double ForeCastTotal { get; set; }

    //    public IEnumerable<FinanceParentChildModel> Children { get; set; }
    //}
    // Add By Nishant Sheth
    // #2325 Add class to return checked budget item 
    public class BudgetCheckedItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Values { get; set; }
    } 

    public class DhtmlXGridRowModel
    {
        public List<DhtmlxGridRowDataModel> rows { get; set; }
        public FinanceModelHeaders FinanemodelheaderObj { get; set; }// Add By Nishant Sheth
        public string setHeader { get; set; }
        public string attachHeader { get; set; }
        public string setInitWidths { get; set; }
        public string setColAlign { get; set; }
        public string setColValidators { get; set; }
        public string setColumnIds { get; set; }
        public string setColTypes { get; set; }
        public string setColumnsVisibility { get; set; }
        public bool enableTreeCellEdit { get; set; }
        public List<String> CustColumnsList { get; set; }
        public string ColumneditLevel { get; set; }
        public string BudgetColName { get; set; }
        public string ForecastColName { get; set; }
        public string PlanColName { get; set; }
        public string ActualColName { get; set; }
        public string HeaderStyle { get; set; }
        public List<Head> head { get; set; }
        public string setColSorting { get; set; } //Added by Maitri Gandhi on 15-03-2016 for #2049 [to include soring in the finance grid]
        public List<string> setCellTextStyle { get; set; }  //Added by Komal Rawal on 08-06-2016 for #2244 when no permission show all data in grey and dash
    }
    public class DhtmlxGridRowDataModel
    {
        public string id { get; set; }
        public List<string> data { get; set; }
        public userdata userdata { get; set; }
        public List<row_attrs> row_attrs { get; set; }
        public List<DhtmlxGridRowDataModel> rows { get; set; }
        public string Detailid { get; set; }
        public FinanceModelHeaders FinanemodelheaderObj { get; set; }
        public string style { get; set; } //Added by Komal Rawal on 08-06-2016 for #2244 when no permission show all data in grey and dash
   
    }
    public class Head
    {
        public List<Options> options { get; set; }
        public string value { get; set; }
        public int width { get; set; }
        public string align { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public string sort { get; set; }    //Added by Maitri Gandhi on 15-03-2016 for #2049 [For EditBudget page]
    }
    public class Options
    {
        public int id { get; set; }
        public string value { get; set; }
    }
    public class userdata
    {
        public string id { get; set; }
        public string idwithName { get; set; }
        public string row_attrs { get; set; }
        public string row_locked { get; set; }
        public string isTitleEdit { get; set; }
    }
    public class row_attrs
    {
        public string id { get; set; }
    }
    public class BudgetAmount
    {
        public List<double?> Budget { get; set; }
        public List<double?> ForeCast { get; set; }
        public List<double?> Plan { get; set; }
        public List<double?> Actual { get; set; }
    }
    public class DeleteRowID
    {
        public int Id { get; set; }

    }
    public class LineItemDropdownModel
    {
        public List<ViewByModel> list { get; set; }
        public int parentId { get; set; }
    }
    // Add By Nishant Sheth
    // Use for marketing budget custom columns list
    public class CustomColumnModel
    {
        public int CustomFieldId { get; set; }
        public int CustomColumSetId { get; set; }
        public string ColName { get; set; }
        public string ValidationType { get; set; }
    }
}
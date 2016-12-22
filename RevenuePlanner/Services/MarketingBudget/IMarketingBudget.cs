using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

namespace RevenuePlanner.Services.MarketingBudget
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get { return string.Format("{0} {1}", FirstName, LastName); } }
    }
    public class UserBudgetPermission
    {
        public int BudgetID { get; set; }
        public User User { get; set; }
        public string Role { get; set; }
        public int Permission { get; set; }
        public bool IsOwner { get; set; }
    }

    public class UserBudgetPermissionDetail
    {
        public int UserId { get; set; }
        public int PermisssionCode { get; set; }
        public int BudgetDetailId { get; set; }
    }

    public class MarketingActivities
    {
        public List<BindDropdownData> ListofBudgets { get; set; }
        public List<BindDropdownData> TimeFrame { get; set; }
        public List<BindDropdownData> Columnset { get; set; }
        public List<BindDropdownData> FilterColumns { get; set; }
        public List<string> StandardCols { get; set; }
    }
    public class BudgetItem
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Title { get; set; }
        public int userCount { get; set; }
        public User Owner { get; set; }
        public BudgetLineData BudgetItemData { get; set; }

    }



    public class BudgetGridDataModel
    {
        public List<BudgetGridRowModel> rows { get; set; }
        public List<GridDataStyle> head { get; set; }
        public FinanceModelHeaders FinanemodelheaderObj { get; set; }
    }

    public class BudgetGridRowModel
    {
        public string id { get; set; }
        public List<string> data { get; set; }
        public List<BudgetGridRowModel> rows { get; set; }
        public UserData userdata { get; set; }
        public string Detailid { get; set; }
        public string style { get; set; } //when no permission show all data in grey and dash

    }

    public class UserData
    {
        public string lo { get; set; } // lock non editable cells.
        public string isTitleEdit { get; set; } //checks if title is editable
        public string per { get; set; } //sets each row permission.

    }
    public class GridDataStyle
    {
        public string value { get; set; }
        public int width { get; set; }
        public string align { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public string sort { get; set; }
        public List<Options> options { get; set; }  // bind options for dropdown list
    }

    // options for dropdown list
    public class Options
    {
        public int id { get; set; }
        public string value { get; set; }
    }
    public class BudgetGridModel
    {
        public List<GridDataStyle> GridDataStyleList { get; set; }
        public string attachedHeader { get; set; }
			   public List<string> nonePermissonIDs { get; set; } //set Three Dash for rollup columns
        public List<int> colIndexes { get; set; }   //set numberformat to rollup columns
        public BudgetGridDataModel objGridDataModel { get; set; }
    }
    public enum ViewByType
    {
        MonthlyForTheYear = 0, QuarterlyForTheYear, MonthlyForQ1, MonthlyForQ2, MonthlyForQ3, MonthlyForQ4
    }

    [Flags]
    public enum BudgetColumnFlag { Budget = 0, Planned = 2, Actual = 4 }

    public enum BudgetCloumn { Y1 = 1, Y2, Y3, Y4, Y5, Y6, Y7, Y8, Y9, Y10, Y11, Y12, Q1, Q2, Q3, Q4, Total, Balance }

    public class BudgetLineData
    {
        public ViewByType ViewBy { get; set; }
        public BudgetColumnFlag ColumnFlag { get; set; }

        //The above two will indicate what to expect in the following:
        public Dictionary<BudgetCloumn, double> BudgetData { get; set; }
        public Dictionary<BudgetCloumn, double> PlannedCostData { get; set; }
        public Dictionary<BudgetCloumn, double> ActualData { get; set; }
    }

    public class BudgetSummary
    {
        public int Id { get; set; }
        public string BudgetTitle { get; set; }
        public double Budget { get; set; }
        public double Planned { get; set; }
        public double Actual { get; set; }
    }

    public class LineItemAccountAssociation
    {
        public int LineItemId;
        public int AccountId;
        public double AllocatedAmount;
    }

    public class PlanAccountAssociation
    {
        public int PlanId;
        public int AccountId;
        public double AllocatedAmount;
    }

    public class AllocatedLineItemForAccount : LineItemAccountAssociation
    {
        public string LineItemTitle { get; set; }
        public string TacticTitle { get; set; }
        public string ProgramTitle { get; set; }
        public string CampaignTitle { get; set; }
        public string PlanTitle { get; set; }

        //NOTE: the following three are totals for the line item 
        //No monthly or quarterly allocation is needed here
        public double Budget { get; set; }
        public double Planned { get; set; }
        public double Actual { get; set; }

        //Total balance remaining on the allocating account
        //NOTE: an account could be used by multiple line items. 
        //This is the total balance after allocations to all line items   
        public double AllocatingdAccountBalance { get; set; }
    }

    /// <summary>
    /// This is the account info with regard to a line item
    /// 
    /// </summary>
    public class LineItemAllocatingAccount : LineItemAccountAssociation
    {
        public string AccountTitle { get; set; }

        /// <summary>
        /// Total balance remaining
        /// </summary>
        public double Balance { get; set; }
    }

    /// <summary>
    /// This is the account info with regard to a line item
    /// 
    /// </summary>
    public class PlanAllocatingAccount : PlanAccountAssociation
    {
        public string AccountTitle { get; set; }

        /// <summary>
        /// Total balance remaining
        /// </summary>
        public double Balance { get; set; }
    }
    public class BindDropdownData
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool IsChecked { get; set; } // Use for checkbox value
    }

    public class BudgetDetailforDeletion
    {
        public int Id { get; set; }
        public int BudgetId { get; set; }
        public int? ParentId { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class DeleteRowID
    {
        public int Id { get; set; }
    }
    // Get list of columns and data wtih xml format for Import
    public class BudgetImportData
    {
        public DataTable MarketingBudgetColumns { get; set; }
        public XmlDocument XmlData { get; set; }
        public string ErrorMsg { get; set; }
    }

    // This Model used for showing Marketing Budge Headers
    public class MarketingBudgetHeadsUp
    {
        public double Budget { get; set; }
        public double Forecast { get; set; }
        public double Planned { get; set; }
        public double Actual { get; set; }
    }
    // this model used for binding line item for budget
    public class LineItemDropdownModel
    {
        public List<ViewByModel> list { get; set; }
        public int parentId { get; set; }
    }
    //modle used for bind line item header
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
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string BusssinessUnit { get; set; }
        public string Region { get; set; }
        public int Permission { get; set; }
        public string createdby { get; set; }
        public bool IsOwner { get; set; }
    }
    public class LineItemDetail
    {
        public BudgetGridModel LineItemGridData { get; set; }
        public bool childLineItemCount { get; set; }
    }
    //Use for User Column Attribute
    public class ColumnAttributeDetail
    {
        public string AttributeType { get; set; }
        public string AttributeId { get; set; }
        public string ColumnOrder { get; set; }
    }
    /// <summary>
    /// Operational interface for budget related data retrieval or manipulations 
    /// </summary>
    public interface IMarketingBudget
    {
        List<BindDropdownData> GetBudgetlist(int ClientId); //mainbudget dropdown

        List<BindDropdownData> GetColumnSet(int ClientId);// Column set dropdown

        List<Budget_Columns> GetColumns(int ColumnSetId);// Column set dropdown

        int SaveNewBudget(string BudgetName, int ClientId, int UserId);
        int  SaveNewBudgetDetail(int BudgetId, string BudgetDetailName, int ParentId ,int ClientId, int UserId, string mainTimeFrame = "Yearly");
        DataSet GetBudgetDefaultData(int budgetId, string timeframe, int ClientID, int UserID, string CommaSeparatedUserIds, double Exchangerate);
        BudgetGridModel GetBudgetGridData(int budgetId, string viewByType, int ClientID, int UserID, double Exchangerate, string CurSymbol,List<BDSService.User> lstUser);

        MarketingBudgetHeadsUp GetFinanceHeaderValues(int BudgetId, double ExchangeRate,List<BDSService.User> lstUser,bool IsLineItem=false); // Header values
        List<BDSService.User> GetUserListByClientId(int ClientID); // List of users for specific client
        List<PlanOptions> GetOwnerListForDropdown(int ClientId, Guid ApplicationId, List<BDSService.User> lstUsers); // Owner list for current client
        /// <summary>
        /// Update budget data only!
        /// Planned and actuals are NOT updated through this interface
        /// 
        /// Should return a full monthly allocated data with total and balance for the budgt data only 
        /// 
        /// </summary>
        /// <param name="budgetId"></param>
        /// <param name="columnIndex"></param>
        /// <param name="oldValue"></param>
        /// in case of quarterly view, thsi old value is required to correctly update at monthly level!
        /// <param name="newValue"></param>
        /// New values expected to see update update (regardless monthly or quarterly viewz)
        /// <returns></returns>
        Dictionary<BudgetCloumn, double> UpdateBudgetCell(int budgetId, BudgetCloumn columnIndex, double oldValue, double newValue);
        int DeleteBudget(int selectedBudgetId, int ClientId);

        // Methods for import budget files
        BudgetImportData GetXLSXData(string viewByType, string fileLocation, int ClientId, int BudgetDetailId = 0, double PlanExchangeRate = 0, string CurrencySymbol = "$");
        BudgetImportData GetXLSData(string viewByType, DataSet ds, int ClientId, int BudgetDetailId = 0, double PlanExchangeRate = 0, string CurrencySymbol = "$");
        int ImportMarketingFinance(XmlDocument XMLData, DataTable ImportBudgetCol, int UserID, int ClientID, int BudgetDetailId = 0);
        void UpdateTaskName(int budgetId, int budgetDetailId, int parentId, int ClientId, string nValue);

        void UpdateOwnerName(int budgetDetailId, List<string> listItems, int ownerId, int clientId);
        void UpdateTotalAmount(int budgetDetailId, string nValue, string columnName, int clientId, double planExchangeRate = 1.0);

        void SaveBudgetorForecast(int budgetDetailId, string nValue, int clientId, bool isForecast = false, string allocationType = "quarters", string period = "", double planExchangeRate = 1.0);
        void SaveCustomColumnValues(int customfieldId, Budget_Columns customCol, int budgetDetailId, string nValue, int userId, int clientId, double planExchangeRate = 1.0);
        List<Budget_Columns> GetBudgetColumn(int clientId);
        //Methods for user permissiopn and lineitems for budget
        LineItemDropdownModel GetParentLineItemBudgetDetailslist(int BudgetDetailId = 0, int ClientId=0);
        List<ViewByModel> GetChildLineItemBudgetDetailslist(int ParentBudgetDetailId = 0, int ClientId=0);
        LineItemDetail GetLineItemGrid(int BudgetDetailId, int ClientId , string IsQuaterly = "quarters", double PlanExchangeRate=1.0);
        List<Budget_Permission> GetUserList(int BudgetId);
        List<UserPermission> FilterByBudget(int BudgetId, Guid ApplicationId);
        string CheckUserPermission(int BudgetId, int ClientId, int UserId);
        UserModel GetuserRecord(int Id, int UserId, Guid ApplicationId);
        FinanceModel EditPermission(int BudgetId, Guid ApplicationId, List<Budget_Permission> UserList, int UserId, int ClientId);
        List<ViewByModel> GetChildBudget(int BudgetId);
        List<BDSService.User> GetAllUserList(int ClientId, int UserId, Guid ApplicationId);
        void DeleteUserForBudget(List<int> BudgetDetailIds, int UserID);
        void SaveUSerPermission(List<UserBudgetPermissionDetail> UserData, string ChildItems, string ParentID, int UserId);
        int SaveUserColumnView(List<ColumnAttributeDetail> AttributeDetail, int UserId);
        List<ColumnAttributeDetail> GetUserColumnView(int UserId, out bool IsSelectall);
        //end
    }
}

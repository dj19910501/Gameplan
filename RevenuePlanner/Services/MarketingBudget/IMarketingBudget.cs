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
    }

    public class BudgetGridModel
    {
        public List<GridDataStyle> GridDataStyleList { get; set; }
        public string attachedHeader { get; set; }
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

    /// <summary>
    /// Operational interface for budget related data retrieval or manipulations 
    /// </summary>
    public interface IMarketingBudget
    {
        List<BudgetItem> GetBudgetData(int budgetId, ViewByType viewByType, BudgetColumnFlag columnsRequested); //initial budget display
        BudgetSummary GetBudgetSummary(int budgetId); //HUD display 
        List<UserBudgetPermission> GetUserPermissionsForAccount(int accountId); //manage users  
        List<AllocatedLineItemForAccount> GetAllocatedLineItemsForAccount(int accountId); //show linked line items  
        void LinkLineItemsToAccounts(List<LineItemAccountAssociation> lineItemAccountAssociations); //link a line item 
        void LinkPlansToAccounts(List<PlanAccountAssociation> planAccountAssociations); //link a plan  
        List<LineItemAllocatingAccount> GetAccountsForLineItem(int lineItemId); //line item page  
        List<PlanAllocatingAccount> GetAccountsForPlan(int planId); //plan page  

        List<BindDropdownData> GetBudgetlist(int ClientId); //mainbudget dropdown

        List<BindDropdownData> GetColumnSet(int ClientId);// Column set dropdown

        List<Budget_Columns> GetColumns(int ColumnSetId);// Column set dropdown

        int SaveNewBudget(string BudgetName, int ClientId, int UserId);
        void  SaveNewBudgetDetail(int BudgetId, string BudgetDetailName, int ParentId ,int ClientId, int UserId, string mainTimeFrame = "Yearly");
        DataSet GetBudgetDefaultData(int budgetId, string timeframe, int ClientID, int UserID, string CommaSeparatedUserIds, double Exchangerate);
        BudgetGridModel GetBudgetGridData(int budgetId, string viewByType, BudgetColumnFlag columnsRequested, int ClientID, int UserID, double Exchangerate, string CurSymbol,List<BDSService.User> lstUser);

        MarketingBudgetHeadsUp GetFinanceHeaderValues(int BudgetId, double ExchangeRate,List<BDSService.User> lstUser); // Header values
        List<BDSService.User> GetUserListByClientId(int ClientID); // List of users for specific client

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
    }
}

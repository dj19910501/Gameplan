using System;
using System.Collections.Generic;

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

    public class BudgetItem
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Title { get; set; }
        public int userCount { get; set; }
        public User Owner { get; set; }
        public BudgetLineData BudgetItemData { get; set; }
    }

    public enum ViewByType
    {
        MonthlyForTheYear = 0, QuarterlyForTheYear, MonthlyForQ1, MonthlyForQ2, MonthlyForQ3, MonthlyForQ4
    }

    [Flags]
    public enum BudgetColumnFlag { Budget=0, Planned=2, Actual=4}

    public enum BudgetCloumn { Y1=1, Y2, Y3, Y4, Y5, Y6, Y7, Y8, Y9, Y10, Y11, Y12, Q1, Q2, Q3, Q4, Total, Balance}

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

    public class AllocatedLineItemForAccount: LineItemAccountAssociation
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
    }
}

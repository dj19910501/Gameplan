using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Services
{
    public interface IPlanTactic
    {
        void AddBalanceLineItem(int tacticId, double Cost, int UserId);
        
        BudgetDHTMLXGridModel GetCostAllocationLineItemInspectPopup(int curTacticId);

        double UpdateBalanceLineItemCost(int PlanTacticId);

        void SaveLineItemMonthlyCostAllocation(int EntityId, double monthlycost, string month, bool isTotalCost);

        void SaveLineItemQuarterlyCostAllocation(int EntityId, double monthlycost, string month, bool isTotalCost);

        void SaveTacticMonthlyCostAllocation(int EntityId, double monthlycost, string month, bool isTotalCost);

        void SaveTacticQuarterlyCostAllocation(int EntityId, double monthlycost, string month, bool isTotalCost);
    }
}
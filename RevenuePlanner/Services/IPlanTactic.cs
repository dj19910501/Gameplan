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

        BudgetDHTMLXGridModel GetCostAllocationLineItemInspectPopup(int curTacticId, string AllocatedBy, int UserId, int ClientId, double PlanExchangeRate, bool IsPlanEditable);

        double UpdateBalanceLineItemCost(int PlanTacticId);

        void SaveTotalTacticCost(int EntityId, double newCost);

        void SaveTotalLineItemCost(int EntityId, double newCost);

        void SaveLineItemCostAllocation(int EntityId, double monthlycost, string month, int UserId, string AllocatedBy, bool isLinkedLineItem = false);

        void SaveTacticCostAllocation(int EntityId, double monthlycost, string month, int UserId, string AllocatedBy, bool isLinkedTactic = false);
    }
}
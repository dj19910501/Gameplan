﻿using RevenuePlanner.Helpers;
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

        BudgetDHTMLXGridModel GetCostAllocationLineItemInspectPopup(int curTacticId, string AllocatedBy, int UserId, int ClientId, double PlanExchangeRate);

        double UpdateBalanceLineItemCost(int PlanTacticId);

        void SaveLineItemMonthlyCostAllocation(int EntityId, double monthlycost, string month, bool isTotalCost,int UserId);

        void SaveLineItemQuarterlyCostAllocation(int EntityId, double monthlycost, string month, bool isTotalCost, int UserId);

        void SaveTacticMonthlyCostAllocation(int EntityId, double monthlycost, string month, bool isTotalCost, int UserId);

        void SaveTacticQuarterlyCostAllocation(int EntityId, double monthlycost, string month, bool isTotalCost, int UserId);
    }
}
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Services
{
    public interface IBudget
    {
        BudgetDHTMLXGridModel GetBudget(string PlanIds, double PlanExchangeRate, Enums.ViewBy viewBy, string year = "", string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "");
        List<PlanBudgetModel> SetCustomFieldRestriction(List<PlanBudgetModel> BudgetModel);
    }
}
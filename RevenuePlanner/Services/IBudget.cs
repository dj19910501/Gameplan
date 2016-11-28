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
        BudgetDHTMLXGridModel GetBudget(int ClientId, int UserID, string PlanIds, double PlanExchangeRate, string viewBy, string year = "", string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "", string Searchtext = "", string SearchBy = "", bool IsFromCache = false);
        List<PlanBudgetModel> SetCustomFieldRestriction(List<PlanBudgetModel> BudgetModel, int UserId, int ClientId);
    }
}
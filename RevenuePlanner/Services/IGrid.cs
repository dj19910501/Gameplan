using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevenuePlanner.Services
{
    public interface IGrid
    {
        //List<GridDefaultModel> GetGridDefaultData(string PlanIds, Guid ClientId);
        //GridCustomColumnData GetGridCustomFieldData(string PlanIds, Guid ClientId);
        PlanMainDHTMLXGrid GetPlanGrid(string PlanIds, Guid ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds, string PlanCurrencySymbol, double PlanExchangeRate);
    }
}

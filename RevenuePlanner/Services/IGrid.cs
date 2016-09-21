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
        // Add By Nishant Sheth
        // Get plan grid data
        PlanMainDHTMLXGrid GetPlanGrid(string PlanIds, int ClientId, List<int> ownerIds, string TacticTypeid, string StatusIds, string customFieldIds, string PlanCurrencySymbol, double PlanExchangeRate);
    }
}

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
        PlanMainDHTMLXGrid GetPlanGrid(string PlanIds, int ClientId, string onerIds, string TacticTypeid, string StatusIds, string customFieldIds, string PlanCurrencySymbol, double PlanExchangeRate, int UserId);

        // Start: Calendar related functions
        List<calendarDataModel> GetPlanCalendarData(string planIds, string ownerIds, string tactictypeIds, string statusIds, string timeframe, string planYear);
        List<calendarDataModel> SetOwnerNameAndPermission(List<calendarDataModel> lstCalendarDataModel);
        // End: Calendar related functions
    }
}

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
        PlanMainDHTMLXGridHomeGrid GetPlanGrid(string PlanIds, int ClientId, string onerIds, string TacticTypeid, string StatusIds, string customFieldIds, string PlanCurrencySymbol, double PlanExchangeRate, int UserId, EntityPermission objPermission, List<int> lstSubordinatesIds, string viewBy, string SearchText="",string SearchBy="", bool IsFromCache=false);
        List<PlanGridFilters> GetGridFilterData(int ClientId, int UserId, bool UserSaveView=false);
        // End plan grid data

        // Start: Calendar related functions
        List<calendarDataModel> GetPlanCalendarData(string planIds, string ownerIds, string tactictypeIds, string statusIds, string customFieldIds, string timeframe, string planYear, string viewby, string Searchtext = "", string SearchBy="");
        List<calendarDataModel> SetOwnerNameAndPermission(List<calendarDataModel> lstCalendarDataModel);
        // End: Calendar related functions

        List<PlanOptionsTacticType> GetTacticTypeListForHeader(string strPlanIds, int ClientId);
        List<PlanOptionsTacticType> GetLineItemTypeListForHeader(string strPlanIds, int ClientId);
    }
}

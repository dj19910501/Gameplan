using RevenuePlanner.Models;
using System;
using System.Collections.Generic;


namespace RevenuePlanner.Services
{
    public interface IAlerts
    {
        List<vClientWise_EntityList> SearchEntities(Guid ClientId);
        int SaveAlert(AlertRuleDetail objRule);
        bool IsAlertRuleExists(int EntityID, int Completiongoal, int indicatorGoal, string Indicator, string Comparison);

    }
}

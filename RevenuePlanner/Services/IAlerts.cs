using RevenuePlanner.Models;
using System;
using System.Collections.Generic;


namespace RevenuePlanner.Services
{
    public interface IAlerts
    {
        List<vClientWise_EntityList> SearchEntities(int ClientId);
     
		  List<AlertRuleDetail> GetAletRuleList(int UserId, int ClientId);
        int UpdateAlertRule(AlertRuleDetail objRule, int UserId);
        int DeleteAlertRule(int RuleId);
        int DisableAlertRule(int RuleId, bool RuleOn);
        List<Alert> GetAlertAummary(int UserId);
        List<User_Notification_Messages> GetNotificationListing(int UserId);
		 int UpdateAlert_Notification_IsRead(string Type, int UserId);
         int DismissAlert_Notification(string Type, int Id);
         int AddUpdate_AlertRule(AlertRuleDetail objRule, int ClientId, int UserId, int RuleId);
    }
}

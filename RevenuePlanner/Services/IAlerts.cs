﻿using RevenuePlanner.Models;
using System;
using System.Collections.Generic;


namespace RevenuePlanner.Services
{
    public interface IAlerts
    {
        List<vClientWise_EntityList> SearchEntities(Guid ClientId);
     
		  List<AlertRuleDetail> GetAletRuleList(Guid UserId, Guid ClientId);
        int UpdateAlertRule(AlertRuleDetail objRule, Guid UserId);
        int DeleteAlertRule(int RuleId);
        int DisableAlertRule(int RuleId, bool RuleOn);
        List<Alert> GetAlertAummary(Guid UserId);
        List<User_Notification_Messages> GetNotificationListing(Guid UserId);
		 int UpdateAlert_Notification_IsRead(string Type, Guid UserId);
         int DismissAlert_Notification(string Type, int Id);
         int AddUpdate_AlertRule(AlertRuleDetail objRule, Guid ClientId, Guid UserId, int RuleId);
    }
}
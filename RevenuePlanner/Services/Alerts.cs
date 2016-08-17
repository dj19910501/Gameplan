using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace RevenuePlanner.Services
{
    
    public class Alerts : IAlerts
    {
        private MRPEntities objDbMrpEntities;
        public Alerts()
        {
            objDbMrpEntities = new MRPEntities();
        }

        #region Method to get Client wise entity list
        public List<vClientWise_EntityList> SearchEntities(Guid ClientId)
        {
            List<vClientWise_EntityList> EntityList = new List<vClientWise_EntityList>();
            try
            {
              EntityList=  objDbMrpEntities.vClientWise_EntityList.Where(a => a.ClientId == ClientId).ToList();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return EntityList;
        }
        #endregion

        #region Method to save Alert Rule
        public int SaveAlert(AlertRuleDetail objRule, Guid ClientId, Guid UserId)
        {
            Alert_Rules objalertRule = new Alert_Rules();
            int result = 0;
            try
            {
                objalertRule.EntityId = Int32.Parse(objRule.EntityID);
                objalertRule.EntityType = objRule.EntityType;
                objalertRule.Indicator = objRule.Indicator;
                objalertRule.IndicatorComparision = objRule.IndicatorComparision; 
                objalertRule.IndicatorGoal = Int32.Parse(objRule.IndicatorGoal);
                objalertRule.CompletionGoal = Int32.Parse(objRule.CompletionGoal);
                objalertRule.Frequency = objRule.Frequency;
                if (objRule.Frequency == Convert.ToString(SyncFrequencys.Weekly))
                    objalertRule.DayOfWeek = Convert.ToByte(objRule.DayOfWeek);
                if (objRule.Frequency == Convert.ToString(SyncFrequencys.Monthly))
                {
                    if (objRule.DateOfMonth != null)
                        objalertRule.DateOfMonth = Convert.ToByte(objRule.DateOfMonth);
                    else
                        objalertRule.DateOfMonth = 10;
                }
                objalertRule.ClientId = ClientId;
                objalertRule.UserId = UserId;
                objalertRule.CreatedDate = DateTime.Now;
                objalertRule.CreatedBy = UserId;
                objalertRule.RuleSummary = objRule.RuleSummary;
                objalertRule.LastProcessingDate = DateTime.Now;
                objalertRule.IsDisabled = false;

                objDbMrpEntities.Entry(objalertRule).State = EntityState.Added;
                result = objDbMrpEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
           return result;
        }
        #endregion

        #region method to check alert rule already exists or not
        public bool IsAlertRuleExists(int EntityID, int Completiongoal, int indicatorGoal, string Indicator, string Comparison, Guid UserId,int Ruleid)
        {
            bool IsExists = false;
            try
            {
                if(Ruleid!=0)
                {
                    IsExists = objDbMrpEntities.Alert_Rules.Any(a => a.EntityId == EntityID && a.CompletionGoal == Completiongoal && a.IndicatorGoal == indicatorGoal && a.Indicator == Indicator && a.IndicatorComparision == Comparison && a.RuleId!=Ruleid && a.UserId==UserId);

                }
                else
                    IsExists = objDbMrpEntities.Alert_Rules.Any(a => a.EntityId == EntityID && a.CompletionGoal == Completiongoal && a.IndicatorGoal == indicatorGoal && a.Indicator == Indicator && a.IndicatorComparision == Comparison && a.UserId == UserId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

            }
            return IsExists;
        }
        #endregion
        #region method to get list or alert rules created
        public List<AlertRuleDetail> GetAletRuleList(Guid UserId, Guid ClientId)
        {
            List<AlertRuleDetail> lsAlerttRule = new List<AlertRuleDetail>();
            try
            {

                var RuleList = (from ar in objDbMrpEntities.Alert_Rules
                                join ent in objDbMrpEntities.vClientWise_EntityList on ar.EntityId equals ent.EntityId
                                where ar.UserId == UserId && ar.EntityType == ent.Entity && ar.ClientId == ClientId
                                select new { ar, ent.EntityTitle }).OrderBy(a => a.ar.IsDisabled).ThenByDescending(a=>a.ar.CreatedDate).ToList();

                lsAlerttRule = RuleList.Select(a => new AlertRuleDetail
                {
                    EntityID = Convert.ToString(a.ar.EntityId),
                    EntityType = a.ar.EntityType,
                    Indicator = a.ar.Indicator,
                    IndicatorGoal = Convert.ToString(a.ar.IndicatorGoal),
                    IndicatorComparision = a.ar.IndicatorComparision,
                    CompletionGoal = Convert.ToString(a.ar.CompletionGoal),
                    Frequency = a.ar.Frequency,
                    DateOfMonth = Convert.ToString(a.ar.DateOfMonth),
                    DayOfWeek = Convert.ToString(a.ar.DayOfWeek),
                    RuleSummary = a.ar.RuleSummary,
                    RuleId = a.ar.RuleId,
                    EntityName = a.EntityTitle,
                    IsDisable=a.ar.IsDisabled
                }).ToList();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

            }
            return lsAlerttRule;
        }
        #endregion
        #region method to update Alert rule
        public int UpdateAlertRule(AlertRuleDetail objRule, Guid UserId)
        {
           
            int result = 0;
            try
            {
                Alert_Rules objalertRule = objDbMrpEntities.Alert_Rules.Where(a => a.RuleId == objRule.RuleId && a.UserId == UserId).FirstOrDefault();
                if (objalertRule != null)
                {
                    objalertRule.EntityId = Int32.Parse(objRule.EntityID);
                    objalertRule.EntityType = objRule.EntityType;
                    objalertRule.Indicator = objRule.Indicator;
                    objalertRule.IndicatorComparision = objRule.IndicatorComparision; ;
                    objalertRule.IndicatorGoal = Int32.Parse(objRule.IndicatorGoal);
                    objalertRule.CompletionGoal = Int32.Parse(objRule.CompletionGoal);
                    objalertRule.Frequency = objRule.Frequency;
                    if (objRule.Frequency == Convert.ToString(SyncFrequencys.Weekly))
                        objalertRule.DayOfWeek = Convert.ToByte(objRule.DayOfWeek);
                    if (objRule.Frequency == Convert.ToString(SyncFrequencys.Monthly))
                    {
                        if (objRule.DateOfMonth != null)
                            objalertRule.DateOfMonth = Convert.ToByte(objRule.DateOfMonth);
                        else
                            objalertRule.DateOfMonth = 10;
                    }
                  
                    objalertRule.ModifiedDate = DateTime.Now;
                    objalertRule.ModifiedBy = UserId;
                    objalertRule.RuleSummary = objRule.RuleSummary;
                    objalertRule.LastProcessingDate = DateTime.Now;
                    objalertRule.IsDisabled = false;

                    objDbMrpEntities.Entry(objalertRule).State = EntityState.Modified;
                    result = objDbMrpEntities.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return result;
        }
        #endregion
        #region method to delete alert rule
        public int DeleteAlertRule(int RuleId)
        {
            int result = 0;
            try
            {
                //delete alert related to rule
                var lstAlerts = objDbMrpEntities.Alerts.Where(a => a.RuleId == RuleId).ToList();
                lstAlerts.ForEach(alert => objDbMrpEntities.Entry(alert).State = EntityState.Deleted);
                objDbMrpEntities.SaveChanges();

                //delete rule
                Alert_Rules objrule = objDbMrpEntities.Alert_Rules.Where(a => a.RuleId == RuleId).FirstOrDefault();
                objDbMrpEntities.Entry(objrule).State = EntityState.Deleted;
                result = objDbMrpEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

            }
            return result;
        }
        #endregion
        #region method to delete alert rule
        public int DisableAlertRule(int RuleId, bool RuleOn)
        {
            int result = 0;
            bool Isdisable = RuleOn==true?false:true;
            try
            {
               
                Alert_Rules objrule = objDbMrpEntities.Alert_Rules.Where(a => a.RuleId == RuleId).FirstOrDefault();
                objrule.IsDisabled = Isdisable;
                objDbMrpEntities.Entry(objrule).State = EntityState.Modified;
                result = objDbMrpEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

            }
            return result;
        }
        #endregion
        #region method to get alert and noti summary
        public List<Alert> GetAlertAummary(Guid UserId)
        {
            List<Alert> lstAlerts = new List<Alert>();
            try
            {

                lstAlerts = objDbMrpEntities.Alerts.Where(a => a.UserId == UserId).OrderByDescending(a => a.CreatedDate).ToList();
               
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

            }
            return lstAlerts;
        }

        public List<User_Notification_Messages> GetNotificationListing(Guid UserId)
        {
            List<User_Notification_Messages> lstNotifications = new List<User_Notification_Messages>();
            try
            {

                lstNotifications = objDbMrpEntities.User_Notification_Messages.Where(a => a.RecipientId == UserId).OrderByDescending(a => a.CreatedDate).ToList();

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

            }
            return lstNotifications;
        }

        #endregion
		   #region method to update Alert rule
        public int UpdateAlert_Notification_IsRead(string Type, Guid UserId)
        {

            int result = 0;
            var Connection = objDbMrpEntities.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            using (SqlCommand command = new SqlCommand("UpdateAlert_Notification", Connection))
            {
                try
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", UserId.ToString());
                    command.Parameters.AddWithValue("@Type", Type.ToLower());

                    SqlDataAdapter adp = new SqlDataAdapter(command);
                    command.CommandTimeout = 0;
                    result = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    return result;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
                }
            }

            return result;
        }
        #endregion
    }
}
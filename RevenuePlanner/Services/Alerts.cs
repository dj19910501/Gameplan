using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
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
        public int SaveAlert(AlertRuleDetail objRule)
        {
            Alert_Rules objalertRule = new Alert_Rules();
            int result = 0;
            try
            {
                objalertRule.EntityId = Convert.ToInt32(objRule.EntityID);
                objalertRule.EntityType = objRule.EntityType;
                objalertRule.Indicator = objRule.Indicator;
                objalertRule.IndicatorComparision = objRule.IndicatorComparision; ;
                objalertRule.IndicatorGoal = Convert.ToInt32(objRule.IndicatorGoal);
                objalertRule.CompletionGoal = Convert.ToInt32(objRule.CompletionGoal);
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
                objalertRule.ClientId = Sessions.User.ClientId;
                objalertRule.UserId = Sessions.User.UserId;
                objalertRule.CreatedDate = DateTime.Now;
                objalertRule.CreatedBy = Sessions.User.UserId;
                objalertRule.RuleSummary = objRule.RuleSummary;
                objalertRule.LastProcessingDate = DateTime.Now;
                objalertRule.NextProcessingDate = DateTime.Now;
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
        public bool IsAlertRuleExists(int EntityID, int Completiongoal, int indicatorGoal, string Indicator, string Comparison)
        {
            bool IsExists = false;
            try
            {
                IsExists = objDbMrpEntities.Alert_Rules.Any(a => a.EntityId == EntityID && a.CompletionGoal == Completiongoal && a.IndicatorGoal == indicatorGoal && a.Indicator == Indicator && a.IndicatorComparision == Comparison);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

            }
            return IsExists;
        }
        #endregion
    }
}
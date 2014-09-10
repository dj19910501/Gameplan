﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevenuePlanner.Models;
using System.Data;

namespace Integration
{
    public enum FrequencyType
    {
        Hourly,
        Daily,
        Weekly,
        Monthly
    }

    public class ScheduledExternalIntegration
    {
        MRPEntities db = new MRPEntities();


        public void ScheduledSync()
        {
            //string dayWeek = DateTime.Now.DayOfWeek.ToString();
            //TimeSpan ts = new TimeSpan(DateTime.Now.Hour, 0, 0);
            DateTime currentDate = DateTime.Now;
            int todaysDay = currentDate.Day;
            int currentHour = currentDate.Hour;
            var lstIntegrationInstanceId = db.SyncFrequencies.Where(varS => varS.NextSyncDate.Value.Day == todaysDay &&
                                                                            varS.NextSyncDate.Value.Hour == currentHour &&
                                                                            varS.IntegrationInstance.IsDeleted == false)////Modified by Mitesh Vaishnav For PL ticket #743 -Actuals Inspect: User Name for Scheduler Integration (Add condition for checking isDeleted flag)
                                                             .ToList()
                                                             .Select(varS => varS.IntegrationInstanceId);

            foreach (var id in lstIntegrationInstanceId)
            {
                UpdateNextSyncDate(id);
                ExternalIntegration objInt = new ExternalIntegration(id);
                objInt.Sync();
            }

            //foreach (var id in lstIntegrationInstanceId)
            //{
            //    //var t = Task.Factory.StartNew(() => UpdateNextSyncDate(id),TaskCreationOptions.LongRunning);
            //    UpdateNextSyncDate(id);
            //    //ExternalIntegration objInt = new ExternalIntegration(id);
            //    var t = Task.Factory.StartNew(() => new ExternalIntegration(id).Sync(), TaskCreationOptions.LongRunning);
            //}

            //Parallel.ForEach(lstIntegrationInstanceId, id =>
            //{
            //    UpdateNextSyncDate(id);
            //    ExternalIntegration objInt = new ExternalIntegration(id);
            //    objInt.Sync();
            //});

        }

        /// <summary>
        /// Dharmraj
        /// Function to update next sync date
        /// </summary>
        /// <param name="integrationInstanceId"></param>
        private void UpdateNextSyncDate(int integrationInstanceId)
        {
            var objSyncFrequency = db.SyncFrequencies.FirstOrDefault(varS => varS.IntegrationInstanceId == integrationInstanceId);
            if (objSyncFrequency != null)
            {
                if (objSyncFrequency.Frequency == FrequencyType.Hourly.ToString())
                {
                    objSyncFrequency.NextSyncDate = objSyncFrequency.NextSyncDate.Value.AddHours(1);
                }
                else if (objSyncFrequency.Frequency == FrequencyType.Daily.ToString())
                {
                    objSyncFrequency.NextSyncDate = objSyncFrequency.NextSyncDate.Value.AddDays(1);
                }
                else if (objSyncFrequency.Frequency == FrequencyType.Weekly.ToString())
                {
                    objSyncFrequency.NextSyncDate = objSyncFrequency.NextSyncDate.Value.AddDays(7);
                }
                else if (objSyncFrequency.Frequency == FrequencyType.Monthly.ToString())
                {
                    objSyncFrequency.NextSyncDate = objSyncFrequency.NextSyncDate.Value.AddMonths(1);
                }

                db.Entry(objSyncFrequency).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}

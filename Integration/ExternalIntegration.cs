using Integration.Salesforce;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Integration.Eloqua;
using Integration.Helper;
using System.Text;
using System.Threading;

namespace Integration
{
    public enum EntityType
    {
        IntegrationInstance,
        Campaign,
        Program,
        Tactic,
        ImprovementTactic,
        ImprovementCamapign,
        ImprovementProgram
    }

    public enum StatusResult
    {
        Success,
        Error
    }

    public enum Operation
    {
        Create,
        Update,
        Delete,
        Import_Actuals,
        Pull_Responses,
        Pull_ClosedWon,
        Import_Cost,
        Pull_QualifiedLeads
    }



    /// <summary>
    /// Enum for tactic.
    /// Added By: Maninder Singh Wadhva.
    /// Date: 11/27/2013
    /// </summary>
    public enum TacticStatus
    {
        Created = 0,
        Submitted = 1,
        Decline = 2,
        Approved = 3,
        InProgress = 4,
        Complete = 5,
    }


    public class ExternalIntegration
    {
        int? _integrationInstanceId { get; set; }
        int _id { get; set; }
        Guid _userId { get; set; }
        EntityType _entityType { get; set; }
        string _integrationType { get; set; }
        bool _isResultError { get; set; }
        Guid _applicationId { get; set; }
        private List<SyncError> _lstAllSyncError = new List<SyncError>();
        MRPEntities db = new MRPEntities();
        /// <summary>
        /// Data Dictionary to hold tactic status values.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/27/2013
        /// </summary>
        public static Dictionary<string, string> TacticStatusValues = new Dictionary<string, string>()
        {
            {TacticStatus.Created.ToString(), "Created"},
            {TacticStatus.Submitted.ToString(), "Submitted"},
            {TacticStatus.Decline.ToString(), "Declined"},
            {TacticStatus.Approved.ToString(), "Approved"},
            {TacticStatus.InProgress.ToString(), "In-Progress"},
            {TacticStatus.Complete.ToString(), "Complete"}
        };

        public ExternalIntegration(int id, Guid applicationId, Guid UserId = new Guid(), EntityType entityType = EntityType.IntegrationInstance)
        {
            _id = id;
            _userId = UserId;
            _entityType = entityType;
            _applicationId = applicationId;
        }

        /// <summary>
        /// call sync method based on sync entity selected
        /// </summary>
        public void Sync()
        {
            if (EntityType.Tactic.Equals(_entityType))
            {
                SyncTactic();
            }
            else if (EntityType.Program.Equals(_entityType))
            {
                SyncProgram();
            }
            else if (EntityType.Campaign.Equals(_entityType))
            {
                SyncCampaing();
            }
            else if (EntityType.ImprovementTactic.Equals(_entityType))//new code added for #532 by uday
            {
                SyncImprovementTactic();
            }
            else
            {
                SyncInstance();
            }

            SendSyncErrorEmail();
        }

        /// <summary>
        /// ImprovementTactic synchronization
        /// </summary>
        private void SyncImprovementTactic()//new code added for #532 by uday
        {
            /// Write query to get integration instance id and integration type.
            _integrationInstanceId = db.Plan_Improvement_Campaign_Program_Tactic.Single(t => t.ImprovementPlanTacticId == _id).Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model.IntegrationInstanceId;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Code;
                IdentifyIntegration();
            }
        }

        /// <summary>
        /// Tactic synchronization
        /// </summary>
        private void SyncTactic()
        {
            /// Write query to get integration instance id and integration type.
            _integrationInstanceId = db.Plan_Campaign_Program_Tactic.Single(t => t.PlanTacticId == _id).Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceId;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Code;
                IdentifyIntegration();
            }
        }

        /// <summary>
        /// Program synchronization
        /// </summary>
        private void SyncProgram()
        {
            /// Write query to get integration instance id and integration type.
            _integrationInstanceId = db.Plan_Campaign_Program.Single(p => p.PlanProgramId == _id).Plan_Campaign.Plan.Model.IntegrationInstanceId;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Code;
                IdentifyIntegration();
            }
        }

        /// <summary>
        /// Campaign synchronization
        /// </summary>
        private void SyncCampaing()
        {
            /// Write query to get integration instance id and integration type.
            _integrationInstanceId = db.Plan_Campaign.Single(c => c.PlanCampaignId == _id).Plan.Model.IntegrationInstanceId;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Code;
                IdentifyIntegration();
            }
        }

        /// <summary>
        /// synchronization all data integrated with selected Instance.
        /// </summary>
        private void SyncInstance()
        {
            _integrationInstanceId = _id;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Code;
                IdentifyIntegration();
            }
        }

        /// <summary>
        /// call intergration api and sync data based on selected IntegrationType 
        /// </summary>
        private void IdentifyIntegration()
        {
            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
            Common.IsAutoSync = false;

            if (_userId == Guid.Empty)
            {
                _userId = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).CreatedBy;
                Common.IsAutoSync = true;
            }

            _isResultError = false;
            IntegrationInstanceLog instanceLogStart = new IntegrationInstanceLog();
            instanceLogStart.IntegrationInstanceId = Convert.ToInt32(_integrationInstanceId);
            instanceLogStart.SyncStart = DateTime.Now;
            instanceLogStart.CreatedBy = _userId;
            instanceLogStart.CreatedDate = DateTime.Now;
            db.Entry(instanceLogStart).State = EntityState.Added;
            int resulValue = db.SaveChanges();

            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Common.PrepareInfoRow("Start Time", instanceLogStart.SyncStart.ToString()), Enums.SyncStatus.Header, DateTime.Now));

            if (resulValue > 0)
            {
                int integrationinstanceLogId = instanceLogStart.IntegrationInstanceLogId;
                IntegrationInstanceLog instanceLogEnd = db.IntegrationInstanceLogs.SingleOrDefault(instance => instance.IntegrationInstanceLogId == integrationinstanceLogId);
                if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Salesforce.ToString()))
                {
                    IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, integrationinstanceLogId, _applicationId);
                    if (integrationSalesforceClient.IsAuthenticated)
                    {
                        _isResultError = integrationSalesforceClient.SyncData();
                    }
                    else
                    {
                        instanceLogEnd.ErrorDescription = "Authentication Failed :" + integrationSalesforceClient._ErrorMessage;
                        _isResultError = true;
                    }
                }
                else if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Eloqua.ToString()))
                {
                    IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, integrationinstanceLogId, _applicationId);
                    if (integrationEloquaClient.IsAuthenticated)
                    {
                        //// Start - Modified by Sohel Pathan on 03/01/2015 for PL ticket #1068
                        List<SyncError> lstSyncError = new List<SyncError>();
                        _isResultError = integrationEloquaClient.SyncData(out lstSyncError);
                        _lstAllSyncError.AddRange(lstSyncError);
                        //// End - Modified by Sohel Pathan on 03/01/2015 for PL ticket #1068
                    }
                    else
                    {
                        instanceLogEnd.ErrorDescription = "Authentication Failed :" + integrationEloquaClient._ErrorMessage;
                        _isResultError = true;
                        //// Start - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                        _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, "Authentication Failed :" + integrationEloquaClient._ErrorMessage, Enums.SyncStatus.Error, DateTime.Now));
                        //// End - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                    }
                }

                int integrationinstanceId = Convert.ToInt32(_integrationInstanceId);
                IntegrationInstance integrationInstance = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == integrationinstanceId);
                if (_isResultError)
                {
                    instanceLogEnd.Status = StatusResult.Error.ToString();
                    integrationInstance.LastSyncStatus = StatusResult.Error.ToString();
                }
                else
                {
                    instanceLogEnd.Status = StatusResult.Success.ToString();
                    integrationInstance.LastSyncStatus = StatusResult.Success.ToString();
                }

                instanceLogStart.SyncEnd = DateTime.Now;
                integrationInstance.LastSyncDate = DateTime.Now;
                db.Entry(instanceLogStart).State = EntityState.Modified;
                db.Entry(integrationInstance).State = EntityState.Modified;
                db.SaveChanges();
                //// Start - Added by Sohel Pathan on 09/01/2015 for PL ticket #1068
                _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Common.PrepareInfoRow("End Time", instanceLogStart.SyncEnd.ToString()), Enums.SyncStatus.Header, DateTime.Now));
                TimeSpan ElapsedTicks = instanceLogStart.SyncEnd.Value.Subtract(instanceLogStart.SyncStart);
                DateTime ElapsedTime = new DateTime(ElapsedTicks.Ticks);
                _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Common.PrepareInfoRow("Elapsed Time", ElapsedTime.ToString("HH:mm:ss")), Enums.SyncStatus.Header, DateTime.Now));
                if (_integrationInstanceId.HasValue)
                {
                    string InstanceName = Common.GetInstanceName(_integrationInstanceId.Value);
                    _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Common.PrepareInfoRow("Instance Name used for synching", InstanceName), Enums.SyncStatus.Header, DateTime.Now));
                }
                //// End - Added by Sohel Pathan on 09/01/2015 for PL ticket #1068
            }
        }
        
        /// <summary>
        /// Function to send an error email while IntegrationInstance sync of Eloqua
        /// </summary>
        private void SendSyncErrorEmail()
        {
            try
            {
                if (_lstAllSyncError.Count > 0)
                {
                    //// Retrieve mail template from db.
                    MRPEntities db = new MRPEntities();
                    string SyncIntegrationError = Enums.Custom_Notification.SyncIntegrationError.ToString();
                    Notification objNotification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(SyncIntegrationError));

                    if (objNotification != null)
                    {
                        //// get userdetails of the logged in user
                        string ClientName = string.Empty;
                        try
                        {
                            BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
                            ClientName = objBDSUserRepository.GetClientName(_userId);
                            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Common.PrepareInfoRow("Client Name", ClientName), Enums.SyncStatus.Header, DateTime.Now));
                        }
                        catch
                        {
                            //// Service related exception
                            return;
                        }
                        
                        ///// Set info row data for no of tactic processed with count
                        _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Common.PrepareInfoRow("Number of Activities liable for synching", Common.tacticSyncTotal.ToString()), Enums.SyncStatus.Header, DateTime.Now));
                        _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Common.PrepareInfoRow("Number of Activities successfully synched", Common.tacticSyncSuccess.ToString()), Enums.SyncStatus.Header, DateTime.Now));
                        int tacticSyncFailed = Common.tacticSyncTotal - Common.tacticSyncSuccess;
                        _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Common.PrepareInfoRow("Number of Activities failed due to some reason", tacticSyncFailed.ToString()), Enums.SyncStatus.Header, DateTime.Now));
                        /////

                        //// Replace mail template tags with corresponding data
                        string emailBody = string.Empty;
                        emailBody = objNotification.EmailContent;
                        emailBody = emailBody.Replace("[NameToBeReplaced]", string.Empty);
                        emailBody = emailBody.Replace("Dear ,", "Hi,"); 
                        
                        string _errorMailBody = Common.PrepareSyncErroEmailBody(_lstAllSyncError);
                        emailBody = emailBody.Replace("[ErrorBody]", _errorMailBody);
                                                
                        //// Get list email ids to whom email to be sent
                        string errorMailTo = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationErrorMailTo");
                        if (!string.IsNullOrEmpty(errorMailTo))
                        {
                            string fromMail = System.Configuration.ConfigurationManager.AppSettings.Get("FromMail");
                            if (!string.IsNullOrEmpty(fromMail))
                            {
                                //// Send mail to multiple users
                                string[] lstEmailTo = errorMailTo.Split(';');
                                for (int emailId = 0; emailId < lstEmailTo.Length; emailId++)
                                {
                                    string toEmail = lstEmailTo.ElementAt(emailId);
                                    ThreadStart threadStart = delegate() { Common.SendMail(toEmail.Trim(), fromMail, emailBody, objNotification.Subject, Convert.ToString(System.Net.Mail.MailPriority.High)); };
                                    Thread thread = new Thread(threadStart);
                                    thread.Start();
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                //// Mail sending exception
                return;
            }
        }

        public List<string> GetTargetDataMember()
        {
            _integrationInstanceId = _id;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Code;
            }

            if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Salesforce.ToString()))
            {
                IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0, _applicationId);
                if (integrationSalesforceClient.IsAuthenticated)
                {
                    return integrationSalesforceClient.GetTargetDataType("Campaign");
                }
            }
            else if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Eloqua.ToString()))
            {
                IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0, _applicationId);
                if (integrationEloquaClient.IsAuthenticated)
                {
                    return integrationEloquaClient.GetTargetDataType();
                }
            }

            return null;
        }

        /// <summary>
        /// Added By Dharmraj on 8-8-2014, Ticket #658
        /// </summary>
        /// <returns>Returns list of properties of salesforce opportunuty object</returns>
        public List<string> GetTargetDataMemberCloseDeal()
        {
            _integrationInstanceId = _id;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Code;
            }

            if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Salesforce.ToString()))
            {
                IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0, _applicationId);
                if (integrationSalesforceClient.IsAuthenticated)
                {
                    return integrationSalesforceClient.GetTargetDataType("Opportunity");
                }
            }

            return null;
        }

        /// <summary>
        /// Added By Dharmraj on 13-8-2014, Ticket #680
        /// </summary>
        /// <returns>Returns list of properties of salesforce CampaignMember or Eloqua Contact object based in integration type</returns>
        public List<string> GetTargetDataMemberPulling()
        {
            _integrationInstanceId = _id;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Code;
            }

            if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Salesforce.ToString()))
            {
                IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0, _applicationId);
                if (integrationSalesforceClient.IsAuthenticated)
                {
                    return integrationSalesforceClient.GetTargetDataType("CampaignMember");
                }
            }
            //// Start - Added by Sohel Pathan on 23/12/2014 for PL ticket #1061
            else if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Eloqua.ToString()))
            {
                IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0, _applicationId);
                if (integrationEloquaClient.IsAuthenticated)
                {
                    var lstContactFields = integrationEloquaClient.GetContactFields();
                    return lstContactFields.Select(contactField => contactField.Value).ToList();
                }
            }
            //// End - Added by Sohel Pathan on 23/12/2014 for PL ticket #1061

            return null;
        }

        /// <summary>
        /// Function to get Eloqua contact target data member list
        /// </summary>
        /// <returns>returns contact contact data member dictionary</returns>
        public Dictionary<string, string> GetEloquaContactTargetDataMemberList()
        {
            _integrationInstanceId = _id;
            IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0, _applicationId);
            if (integrationEloquaClient.IsAuthenticated)
            {
                var lstContactFields = integrationEloquaClient.GetContactFields();
                return lstContactFields;
            }
            return null;
        }
    }
}

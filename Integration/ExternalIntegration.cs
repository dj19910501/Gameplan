using Integration.Salesforce;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using SalesforceSharp;
using Integration.Eloqua;
using Integration.Helper;

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
        Import_Actuals
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

        public ExternalIntegration(int id, Guid UserId = new Guid(), EntityType entityType = EntityType.IntegrationInstance)
        {
            _id = id;
            _userId = UserId;
            _entityType = entityType;
        }

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
        }

        private void SyncImprovementTactic()//new code added for #532 by uday
        {
            /// Write query to get integration instance id and integration type.
            _integrationInstanceId = db.Plan_Improvement_Campaign_Program_Tactic.Single(t => t.ImprovementPlanTacticId == _id).Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model.IntegrationInstanceId;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Title;
                IdentifyIntegration();
            }
        }

        private void SyncTactic()
        {
            /// Write query to get integration instance id and integration type.
            _integrationInstanceId = db.Plan_Campaign_Program_Tactic.Single(t => t.PlanTacticId == _id).Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceId;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Title;
                IdentifyIntegration();
            }
        }

        private void SyncProgram()
        {
            /// Write query to get integration instance id and integration type.
            _integrationInstanceId = db.Plan_Campaign_Program.Single(p => p.PlanProgramId == _id).Plan_Campaign.Plan.Model.IntegrationInstanceId;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Title;
                IdentifyIntegration();
            }
        }

        private void SyncCampaing()
        {
            /// Write query to get integration instance id and integration type.
            _integrationInstanceId = db.Plan_Campaign.Single(c => c.PlanCampaignId == _id).Plan.Model.IntegrationInstanceId;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Title;
                IdentifyIntegration();
            }
        }

        private void SyncInstance()
        {
            _integrationInstanceId = _id;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Title;
                IdentifyIntegration();
            }
        }

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

            if (resulValue > 0)
            {
                int integrationinstanceLogId = instanceLogStart.IntegrationInstanceLogId;
                IntegrationInstanceLog instanceLogEnd = db.IntegrationInstanceLogs.SingleOrDefault(instance => instance.IntegrationInstanceLogId == integrationinstanceLogId);
            if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Salesforce.ToString()))
            {
                    IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, integrationinstanceLogId);
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
                    IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, integrationinstanceLogId);
                    if (integrationEloquaClient.IsAuthenticated)
                    {
                        _isResultError = integrationEloquaClient.SyncData();
                    }
                    else
                    {
                        instanceLogEnd.ErrorDescription = "Authentication Failed :" + integrationEloquaClient._ErrorMessage;
                        _isResultError = true;
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
            }
        }

        public List<string> GetTargetDataMember()
        {
            _integrationInstanceId = _id;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Title;
            }

            if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Salesforce.ToString()))
            {
                IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0);
                if (integrationSalesforceClient.IsAuthenticated)
                {
                    return integrationSalesforceClient.GetTargetDataType("Campaign");
                }
            }
            else if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Eloqua.ToString()))
            {
                IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0);
                if (integrationEloquaClient.IsAuthenticated)
                {
                    return integrationEloquaClient.GetTargetDataType();
                }
            }

            return null;
        }

        public List<string> GetTargetDataMemberCloseDeal()
        {
            _integrationInstanceId = _id;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Title;
            }

            if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Salesforce.ToString()))
            {
                IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0);
                if (integrationSalesforceClient.IsAuthenticated)
                {
                    return integrationSalesforceClient.GetTargetDataType("Opportunity");
                }
            }

            return null;
        }
    }
}

using Integration.Salesforce;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration
{
    public enum EntityType
    {
        IntegrationInstance,
        Campaign,
        Program,
        Tactic,
        ImprovementTactic
    }

    public enum IntegrationType
    {
        Salesforce,
        Eloqua
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
        EntityType _entityType { get; set; }
        string _integrationType { get; set; }
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

        public ExternalIntegration(int id, EntityType entityType = EntityType.IntegrationInstance)
        {
            _id = id;
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
            else
            {
                SyncInstance();
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
            if (_integrationType.Equals(IntegrationType.Salesforce.ToString()))
            {
                IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType);
                if (integrationSalesforceClient.IsAuthenticated)
                {
                    integrationSalesforceClient.SyncData();
                }
            }
            else if (_integrationType.Equals(IntegrationType.Eloqua.ToString()))
            {

            }
        }

        public List<string> GetTargetDataMember()
        {
            _integrationInstanceId = _id;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Title;
            }

            if (_integrationType.Equals(IntegrationType.Salesforce.ToString()))
            {
                IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType);
                if (integrationSalesforceClient.IsAuthenticated)
                {
                    return integrationSalesforceClient.GetTargetDataType();
                }
            }
            else if (_integrationType.Equals(IntegrationType.Eloqua.ToString()))
            {

            }

            return null;
        }
    }
}

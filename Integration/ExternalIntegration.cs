using Integration.Salesforce;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Integration.Eloqua;
using Integration.WorkFront;
using Integration.Helper;
using System.Text;
using System.Threading;
using Integration.Marketo;

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
        Pull_QualifiedLeads,
        Get_SFDCID_For_Marketo
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
        int _userId { get; set; }
        EntityType _entityType { get; set; }
        string _integrationType { get; set; }
        public bool _isResultError { get; set; }
        Guid _applicationId { get; set; }
        bool _isTacticMoved { get; set; }
        private List<SyncError> _lstAllSyncError = new List<SyncError>();
        MRPEntities db = new MRPEntities();
        private bool IsInActiveInstance = false;
        public bool _isAuthenticationError = false;
        private bool _isInActiveInstance
        {
            get
            {
                return IsInActiveInstance;
            }
            set
            {
                IsInActiveInstance = value;
            }
        }
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

        public ExternalIntegration(int id, Guid applicationId, int UserId = 0, EntityType entityType = EntityType.IntegrationInstance,bool isTacticMoved=false)
        {
            _id = id;
            _userId = UserId;
            _entityType = entityType;
            _applicationId = applicationId;
            _isTacticMoved = isTacticMoved;
        }

        /// <summary>
        /// call sync method based on sync entity selected
        /// </summary>
        public void Sync()
        {

            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            
            try
            {
            Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Check entity type");
           
            if (EntityType.Tactic.Equals(_entityType))
            {

                Plan_Campaign_Program_Tactic tacticForIntegration = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == _id);
                int? sfdcInstanceId = 0, eloquaInstanceId = 0, workfrontInstanceId = 0,marketoInstanceId=0;
                Model objModel = new Model();
                objModel = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == _id).Plan_Campaign_Program.Plan_Campaign.Plan.Model;
                #region "Get Salesforce & Eloqua InstanceId from Plan_Campaign_Program_Tactic table"
                /// Get Salesforce Instance Id from Model table based on TacticID.
                if (objModel != null)
                {
                    sfdcInstanceId = objModel.IntegrationInstanceId;
                    eloquaInstanceId = objModel.IntegrationInstanceEloquaId;
                    workfrontInstanceId = objModel.IntegrationInstanceIdProjMgmt; //added Brad Gray 10-13-2015 PL#1514
                    marketoInstanceId = objModel.IntegrationInstanceMarketoID;
                }
                #endregion

                #region "Execute Syncing process for Salesforce, Eloqua, & WorkFront"
                if (sfdcInstanceId.HasValue && sfdcInstanceId.Value > 0)
                {
                    if (tacticForIntegration.IsSyncSalesForce != null && (bool)tacticForIntegration.IsSyncSalesForce == true)
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync Tactic Instance with Salesforce started - Initiated by Approved Flow");
                        SyncTactic(sfdcInstanceId.Value);
                        Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync Tactic Instance with Salesforce started - Initiated by Approved Flow");
                    }
                }
                if (eloquaInstanceId.HasValue && eloquaInstanceId.Value > 0)
                {
                    if (tacticForIntegration.IsSyncEloqua != null && (bool)tacticForIntegration.IsSyncEloqua == true)
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync Tactic Instance with Eloqua started - Initiated by Approved Flow");
                        SyncTactic(eloquaInstanceId.Value);
                        Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync Tactic Instance with Eloqua started - Initiated by Approved Flow");
                    }
                }
                if (workfrontInstanceId.HasValue && workfrontInstanceId.Value > 0) //added Brad Gray 10-13-2015 PL#1514
                {
                    if (tacticForIntegration.IsSyncWorkFront != null && (bool)tacticForIntegration.IsSyncWorkFront == true)
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync Tactic Instance with WorkFront started - Initiated by Approved Flow");
                        SyncTactic(workfrontInstanceId.Value);
                        Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync Tactic Instance with WorkFront started - Initiated by Approved Flow");
                    }
                }
                if (marketoInstanceId.HasValue && marketoInstanceId.Value > 0)
                {
                    if (tacticForIntegration.IsSyncMarketo != null && (bool)tacticForIntegration.IsSyncMarketo == true)
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync Tactic Instance with Marketo started - Initiated by Approved Flow");
                        SyncTactic(marketoInstanceId.Value);
                        Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync Tactic Instance with Marketo started - Initiated by Approved Flow");
                    }
                }
                #endregion
            }
            // Start : These below functions(i.e For Program & Campaign) not call in current functiopnality - To be used in future
            else if (EntityType.Program.Equals(_entityType))
            {
                Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync Program Instance started - Initiated by Approved Flow");
                SyncProgram();
                Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync Program Instance ended - Initiated by Approved Flow");
            }
            else if (EntityType.Campaign.Equals(_entityType))
            {
                Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync Campaign Instance started - Initiated by Approved Flow");
                SyncCampaing();
                Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync Campaign Instance ended - Initiated by Approved Flow");
            }
            // End : These below functions(i.e For Program & Campaign) not call in current functiopnality - To be used in future
            else if (EntityType.ImprovementTactic.Equals(_entityType))//new code added for #532 by uday
            {
                //SyncImprovementTactic();
                int? sfdcInstanceId = 0, eloquaInstanceId = 0;
                Model objModel = new Model();
                objModel = db.Plan_Improvement_Campaign_Program_Tactic.FirstOrDefault(t => t.ImprovementPlanTacticId == _id).Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model;
                #region "Get Salesforce & Eloqua InstanceId from Plan_Improvement_Campaign_Program_Tactic table"
                /// Get Salesforce Instance Id from Model table based on TacticID.
                if (objModel != null)
                {
                    sfdcInstanceId = objModel.IntegrationInstanceId;
                    eloquaInstanceId = objModel.IntegrationInstanceEloquaId;
                }
                #endregion

                #region "Execute Syncing process for Salesforce & Eloqua"
                if (sfdcInstanceId.HasValue && sfdcInstanceId.Value > 0)
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync ImprovementPlanTactic Instance with Salesforce started - Initiated by Approved Flow");
                    SyncImprovementTactic(sfdcInstanceId.Value);
                    Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync ImprovementPlanTactic Instance with Salesforce started - Initiated by Approved Flow");
                }
                if (eloquaInstanceId.HasValue && eloquaInstanceId.Value > 0)
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync ImprovementPlanTactic Instance with Eloqua started - Initiated by Approved Flow");
                    SyncImprovementTactic(eloquaInstanceId.Value);
                    Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync ImprovementPlanTactic Instance with Eloqua started - Initiated by Approved Flow");
                }
                #endregion
            }
            else
            {
                _integrationInstanceId = _id;
                var Instance = db.IntegrationInstances.Where(i => i.IntegrationInstanceId == _id).FirstOrDefault();
                if (Instance != null && Instance.IsActive)
                {
                    if (Instance.LastSyncStatus != Enums.SyncStatusValues[Enums.SyncStatus.InProgress.ToString()].ToString())
                    {
                        Instance.LastSyncStatus = Enums.SyncStatusValues[Enums.SyncStatus.InProgress.ToString()].ToString();
                        db.Entry(Instance).State = EntityState.Modified;
                        int resulValue = db.SaveChanges();
                        Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync Instance started - Initiated by Sync Now or Scheduler");
                        SyncInstance();
                        Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync Instance ended - Initiated by Sync Now or Scheduler");
                    }
                    else
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Sync process for this instance is already running.");
                    }
                }
                else
                {
                    _isInActiveInstance = true; // Manage this flag at SendSyncErrorEmail and PrepareSyncErroMailHeader function.
                    IntegrationInstanceLog instanceLogStart = new IntegrationInstanceLog();
                    instanceLogStart.IntegrationInstanceId = Convert.ToInt32(_id);
                    instanceLogStart.SyncStart = DateTime.Now;
                    instanceLogStart.CreatedBy = _userId;
                    instanceLogStart.CreatedDate = DateTime.Now;
                    instanceLogStart.ErrorDescription = "Instance have inactive status.";
                    instanceLogStart.Status = Enums.SyncStatus.Error.ToString();
                    db.Entry(instanceLogStart).State = EntityState.Added;
                    Instance.LastSyncStatus = StatusResult.Error.ToString();
                    Instance.LastSyncDate = DateTime.Now;
                    instanceLogStart.SyncEnd = DateTime.Now;
                    if (_userId == 0)
                        instanceLogStart.IsAutoSync = true;      // PL ticket #1449: Update IntegrationInstanceLog that Sync process by Auth or Manual.
                    else
                        Instance.ForceSyncUser = _userId;
                    db.Entry(Instance).State = EntityState.Modified;
                    int resulValue = db.SaveChanges();
                    Common.SaveIntegrationInstanceLogDetails(_id, instanceLogStart.IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Instance have inactive status.");
                    //_lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Pull Responses Sync Status - ", "Success"), Enums.SyncStatus.Header, DateTime.Now));
                    _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, EntityType.IntegrationInstance.ToString(), "Instance have inactive status.", Enums.SyncStatus.Error, DateTime.Now));
                    if (_id > 0)
                    {
                        string InstanceName = Common.GetInstanceName(_id);
                        _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Instance Name used for syncing", InstanceName), Enums.SyncStatus.Header, DateTime.Now));
                    }
                }
            }

            SendSyncErrorEmail(_integrationType);
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Error occurred while syncing data based on entity: " + exMessage);
            }

        }

        /// <summary>
        /// ImprovementTactic synchronization
        /// </summary>
        private void SyncImprovementTactic(int instanceId)//new code added for #532 by uday
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            //int? integrationInstanceId = 0;
            Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Get Integration Instance Id based on ImprovementPlanTacticId.");
            /// Write query to get integration instance id and integration type.
            /// Get Salesforce InstanceID from Model table based on ImprovementPlanTacticId.
            //integrationInstanceId = db.Plan_Improvement_Campaign_Program_Tactic.FirstOrDefault(t => t.ImprovementPlanTacticId == _id).Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model.IntegrationInstanceId;

            // if Salesforce InstanceID is null or 0 then retreive EloquaID from Model table.
            //if (!integrationInstanceId.HasValue || integrationInstanceId <= 0)
            //{
            //    integrationInstanceId = db.Plan_Improvement_Campaign_Program_Tactic.FirstOrDefault(t => t.ImprovementPlanTacticId == _id).Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model.IntegrationInstanceEloquaId;
            //}
            _integrationInstanceId = instanceId;

            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Code;
                var Instance = db.IntegrationInstances.Where(i => i.IntegrationInstanceId == _integrationInstanceId).FirstOrDefault();
                if (Instance != null && Instance.IsActive)
                {
                    IdentifyIntegration();
                }
                else
                {
                    IntegrationInstanceLog instanceLogStart = new IntegrationInstanceLog();
                    instanceLogStart.IntegrationInstanceId = Convert.ToInt32(_integrationInstanceId);
                    instanceLogStart.SyncStart = DateTime.Now;
                    instanceLogStart.CreatedBy = _userId;
                    instanceLogStart.CreatedDate = DateTime.Now;
                    instanceLogStart.ErrorDescription = "Instance have inactive status.";
                    instanceLogStart.Status = Enums.SyncStatus.Error.ToString();
                    db.Entry(instanceLogStart).State = EntityState.Added;
                    Instance.LastSyncStatus = StatusResult.Error.ToString();
                    db.Entry(Instance).State = EntityState.Modified;
                    int resulValue = db.SaveChanges();
                    Common.SaveIntegrationInstanceLogDetails(_id, instanceLogStart.IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Instance have inactive status.");
                    _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.ImprovementTactic, EntityType.IntegrationInstance.ToString(), "Instance have inactive status.", Enums.SyncStatus.Error, DateTime.Now));
                }
            }
        }

        /// <summary>
        /// Tactic synchronization
        /// </summary>
        private void SyncTactic(int instanceId)
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            int? integrationInstanceId =0;
            try
            {
                Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Get Integration Instance Id based on PlanTacticId.");
                /// Write query to get integration instance id and integration type.
                /// Get Salesforce Instance Id from Model table based on TacticID.
                //integrationInstanceId = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == _id).Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceId;

                // if Salesforce InstanceID is null or 0 then retreive EloquaID from Model table.
                //if (!integrationInstanceId.HasValue || integrationInstanceId <= 0)
                //{
                //    integrationInstanceId = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == _id).Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceEloquaId;
                //}

                integrationInstanceId = instanceId;

                int? integrationInstanceProjectManagmentId = db.Plan_Campaign_Program_Tactic.Single(t => t.PlanTacticId == _id).Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt;


                //Modified by Brad Gray 07/24/2015 for WorkFront sync and separating instance ids
                if (integrationInstanceId.HasValue)
                {
                    _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == integrationInstanceId).IntegrationType.Code;
                    _integrationInstanceId = integrationInstanceId;//db.Plan_Campaign_Program_Tactic.Single(t => t.PlanTacticId == _id).Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceId;
                }
                else if (integrationInstanceProjectManagmentId.HasValue)
                {
                    _integrationType = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == integrationInstanceProjectManagmentId).IntegrationType.Code;
                    _integrationInstanceId = db.Plan_Campaign_Program_Tactic.Single(t => t.PlanTacticId == _id).Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt;
                }
                if (_integrationType != null && _integrationInstanceId.HasValue)
                {
                    var Instance = db.IntegrationInstances.Where(i => i.IntegrationInstanceId == _integrationInstanceId).FirstOrDefault();
                    if (Instance != null && Instance.IsActive)
                    {
                        IdentifyIntegration();
                    }
                    else
                    {

                        IntegrationInstanceLog instanceLogStart = new IntegrationInstanceLog();
                        instanceLogStart.IntegrationInstanceId = Convert.ToInt32(_integrationInstanceId);
                        instanceLogStart.SyncStart = DateTime.Now;
                        instanceLogStart.CreatedBy = _userId;
                        instanceLogStart.CreatedDate = DateTime.Now;
                        instanceLogStart.ErrorDescription = "Instance have inactive status.";
                        instanceLogStart.Status = Enums.SyncStatus.Error.ToString();
                        db.Entry(instanceLogStart).State = EntityState.Added;

                        Instance.LastSyncStatus = StatusResult.Error.ToString();
                        db.Entry(Instance).State = EntityState.Modified;
                        int resulValue = db.SaveChanges();
                        Common.SaveIntegrationInstanceLogDetails(_id, instanceLogStart.IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Instance have inactive status.");
                        _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, EntityType.IntegrationInstance.ToString(), "Instance have inactive status.", Enums.SyncStatus.Error, DateTime.Now));
                    }
                }
                else
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "The Model of this tactic do not have integration enabled.");
                }
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while proccessing SyncTactic :-" + exMessage);
            }
        }

        /// <summary>
        /// Program synchronization
        /// </summary>
        private void SyncProgram()
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            int? integrationInstanceId = 0;
            try
            {
            /// Write query to get integration instance id and integration type.
            /// Get Salesforce InstanceID from Model table based on ProgramId.
                integrationInstanceId = db.Plan_Campaign_Program.FirstOrDefault(p => p.PlanProgramId == _id).Plan_Campaign.Plan.Model.IntegrationInstanceId;
            // if Salesforce InstanceID is null or 0 then retreive EloquaID from Model table.
            //if (!integrationInstanceId.HasValue || integrationInstanceId <= 0)
            //{
            //    integrationInstanceId = db.Plan_Campaign_Program.FirstOrDefault(t => t.PlanProgramId == _id).Plan_Campaign.Plan.Model.IntegrationInstanceEloquaId;
            //}
            _integrationInstanceId = integrationInstanceId;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Code;
                var Instance = db.IntegrationInstances.Where(i => i.IntegrationInstanceId == _integrationInstanceId).FirstOrDefault();
                if (Instance != null && Instance.IsActive)
                {
                    IdentifyIntegration();
                }
                else
                {
                    IntegrationInstanceLog instanceLogStart = new IntegrationInstanceLog();
                    instanceLogStart.IntegrationInstanceId = Convert.ToInt32(_integrationInstanceId);
                    instanceLogStart.SyncStart = DateTime.Now;
                    instanceLogStart.CreatedBy = _userId;
                    instanceLogStart.CreatedDate = DateTime.Now;
                    instanceLogStart.ErrorDescription = "Instance have inactive status.";
                    instanceLogStart.Status = Enums.SyncStatus.Error.ToString();
                    db.Entry(instanceLogStart).State = EntityState.Added;
                    Instance.LastSyncStatus = StatusResult.Error.ToString();
                    db.Entry(Instance).State = EntityState.Modified;
                    int resulValue = db.SaveChanges();
                    Common.SaveIntegrationInstanceLogDetails(_id, instanceLogStart.IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Instance have inactive status.");
                    _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Program, EntityType.IntegrationInstance.ToString(), "Instance have inactive status.", Enums.SyncStatus.Error, DateTime.Now));
                }
            }
        }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while proccessing SyncProgram :-" + exMessage);
            }
        }

        /// <summary>
        /// Campaign synchronization
        /// </summary>
        private void SyncCampaing()
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            int? integrationInstanceId = 0;
            try
            {
            /// Write query to get integration instance id and integration type.
                integrationInstanceId = db.Plan_Campaign.FirstOrDefault(c => c.PlanCampaignId == _id).Plan.Model.IntegrationInstanceId;

            // if Salesforce InstanceID is null or 0 then retreive EloquaID from Model table.
            //if (!integrationInstanceId.HasValue || integrationInstanceId <= 0)
            //{
            //    integrationInstanceId = db.Plan_Campaign.FirstOrDefault(c => c.PlanCampaignId == _id).Plan.Model.IntegrationInstanceEloquaId;
            //}
            _integrationInstanceId = integrationInstanceId;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Code;
                var Instance = db.IntegrationInstances.Where(i => i.IntegrationInstanceId == _integrationInstanceId).FirstOrDefault();
                if (Instance != null && Instance.IsActive)
                {
                    IdentifyIntegration();
                }
                else
                {
                    IntegrationInstanceLog instanceLogStart = new IntegrationInstanceLog();
                    instanceLogStart.IntegrationInstanceId = Convert.ToInt32(_integrationInstanceId);
                    instanceLogStart.SyncStart = DateTime.Now;
                    instanceLogStart.CreatedBy = _userId;
                    instanceLogStart.CreatedDate = DateTime.Now;
                    instanceLogStart.ErrorDescription = "Instance have inactive status.";
                    instanceLogStart.Status = Enums.SyncStatus.Error.ToString();
                    db.Entry(instanceLogStart).State = EntityState.Added;
                    Instance.LastSyncStatus = StatusResult.Error.ToString();
                    db.Entry(Instance).State = EntityState.Modified;
                    int resulValue = db.SaveChanges();
                    Common.SaveIntegrationInstanceLogDetails(_id, instanceLogStart.IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Instance have inactive status.");
                    _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Campaign, EntityType.IntegrationInstance.ToString(), "Instance have inactive status.", Enums.SyncStatus.Error, DateTime.Now));
                }
            }
        }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while proccessing SyncCampaign :-" + exMessage);
            }
        }

        /// <summary>
        /// synchronization all data integrated with selected Instance.
        /// </summary>
        private void SyncInstance()
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {
            _integrationInstanceId = _id;
            if (_integrationInstanceId.HasValue)
            {
                _integrationType = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).IntegrationType.Code;
                IdentifyIntegration();
            }
        }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while synchronizing all tactic data. :-" + exMessage);
            }
        }

        /// <summary>
        /// call intergration api and sync data based on selected IntegrationType 
        /// </summary>
        private void IdentifyIntegration()
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string LogEndErrorDescription = string.Empty, LogEndStatus = string.Empty;
            try
            {
            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
            Common.IsAutoSync = false;

            if (_userId == 0)
            {
                    _userId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).CreatedBy;
                Common.IsAutoSync = true;
            }

            _isResultError = false;
                int resulValue = 0;
                Common.SaveIntegrationInstanceLogDetails(_id,null, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Inserting log primary details to IntegrationInstanceLog table.");
            IntegrationInstanceLog instanceLogStart = new IntegrationInstanceLog();
                try
                {
            instanceLogStart.IntegrationInstanceId = Convert.ToInt32(_integrationInstanceId);
            instanceLogStart.SyncStart = DateTime.Now;
            instanceLogStart.CreatedBy = _userId;
            instanceLogStart.CreatedDate = DateTime.Now;
            db.Entry(instanceLogStart).State = EntityState.Added;
                    resulValue = db.SaveChanges();
                }
                catch (Exception ex)
                {
                    string exMessage = Common.GetInnermostException(ex);
                    Common.SaveIntegrationInstanceLogDetails(_id,null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while inserting log into IntegrationInstanceLog table. : " + exMessage);
                }
                Common.SaveIntegrationInstanceLogDetails(_id,null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Inserting log primary details to IntegrationInstanceLog table.");

            bool IsAuthenticationError = false;

            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Start Time", instanceLogStart.SyncStart.ToString()), Enums.SyncStatus.Header, DateTime.Now));

                //if (resulValue > 0)
                //{
                int integrationinstanceLogId = instanceLogStart.IntegrationInstanceLogId;

                try
                {
                    if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Salesforce.ToString()))
                    {
                        IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, integrationinstanceLogId, _applicationId);
                        if (integrationSalesforceClient.IsAuthenticated)
                        {
                            Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Authentication with salesforce Success.");
                            if (!_isTacticMoved)
                            {
                                Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync Data Start.");
                                List<SyncError> lstSyncError = new List<SyncError>();
                                //_isResultError = integrationSalesforceClient.SyncData(out lstSyncError);
                                _isResultError = integrationSalesforceClient.SyncSFDCDataByAPI(out lstSyncError);
                                _lstAllSyncError.AddRange(lstSyncError);
                                Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync Data End.");
                            }
                            else
                            {
                                Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync Moved TacticData Start.");
                                _isResultError = integrationSalesforceClient.SyncMovedTacticData();
                                Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync Moved TacticData End.");
                            }
                        }
                        else
                        {
                            LogEndErrorDescription = "Authentication Failed :" + integrationSalesforceClient._ErrorMessage;
                            Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, LogEndErrorDescription);

                            _isResultError = true;
                            IsAuthenticationError = true;
                            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, "Authentication Failed :" + integrationSalesforceClient._ErrorMessage, Enums.SyncStatus.Error, DateTime.Now));
                        }
                    }
                    else if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Eloqua.ToString()))
                    {
                        IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, integrationinstanceLogId, _applicationId);
                        if (integrationEloquaClient.IsAuthenticated)
                        {
                            Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Authentication with Eloqua Success.");

                            Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync Data Start.");
                            //// Start - Modified by Sohel Pathan on 03/01/2015 for PL ticket #1068
                            List<SyncError> lstSyncError = new List<SyncError>();
                            _isResultError = integrationEloquaClient.SyncData(out lstSyncError);
                            _lstAllSyncError.AddRange(lstSyncError);
                            //// End - Modified by Sohel Pathan on 03/01/2015 for PL ticket #1068
                            Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync Data End.");
                        }
                        else
                        {
                            LogEndErrorDescription = "Authentication Failed :" + integrationEloquaClient._ErrorMessage;
                            Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, LogEndErrorDescription);
                            _isResultError = true;
                            IsAuthenticationError = true;
                            //// Start - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, "Authentication Failed :" + integrationEloquaClient._ErrorMessage, Enums.SyncStatus.Error, DateTime.Now));
                            //// End - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                        }
                    }
                    ///Added by Brad Gray for WorkFront Sync PL ticket #1368
                    ///WorkFront is in play at a tactic and instance level
                    ///WorkFront templates are tied to instance integration ID
                    ///instance sync must find all tactics tied to the instance and update accordingly
                    else if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.WorkFront.ToString()))
                    {
                        IntegrationWorkFrontSession integrationWorkFrontClient = new IntegrationWorkFrontSession(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, integrationinstanceLogId, _applicationId);
                        if (integrationWorkFrontClient.IsAuthenticated)
                        {
                            Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Authentication with WorkFront Success.");
                            List<SyncError> lstSyncError = new List<SyncError>();
                            _isResultError = integrationWorkFrontClient.SyncData(out lstSyncError);
                            _lstAllSyncError.AddRange(lstSyncError);
                            if (lstSyncError.Count > 0 && _isResultError)
                            {
                                Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "WorkFront Sync Error");
                                foreach (SyncError e in lstSyncError)
                                {
                                    Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.None, e.SectionName, Enums.MessageLabel.Error, e.Message);
                                }
                            }
                            else
                            {
                                Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Success: Sync with WorkFront");
                            }
                        }
                        else
                        {
                            LogEndErrorDescription = "Authentication Failed :" + integrationWorkFrontClient.errorMessage;
                            Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, LogEndErrorDescription);
                            _isResultError = true;
                            IsAuthenticationError = true;
                            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, "Authentication Failed :" + integrationWorkFrontClient.errorMessage, Enums.SyncStatus.Error, DateTime.Now));
                        }
                    }
                    else if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Marketo.ToString())) //Added by Rahul Shah for PL #2194
                    {
                        ApiIntegration MarketoAPI = new ApiIntegration(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, integrationinstanceLogId, _applicationId);
                        MarketoAPI.AuthenticateforMarketo();
                        //_isAuthenticationError = MarketoAPI.IsAuthenticated;
                        if (MarketoAPI.IsAuthenticated)
                        {
                            IntegrationMarketoClient integrationMarketoClient = new IntegrationMarketoClient(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, integrationinstanceLogId, _applicationId);
                            Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync Data Start.");
                            List<SyncError> lstSyncError = new List<SyncError>();
                            _isResultError = integrationMarketoClient.SyncData(out lstSyncError);
                            _lstAllSyncError.AddRange(lstSyncError);
                            Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync Data End.");
                        }
                        else
                        {
                            LogEndErrorDescription = "Authentication Failed.";
                            Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, LogEndErrorDescription);
                            _isResultError = true;
                            IsAuthenticationError = true;
                            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, "Authentication Failed.", Enums.SyncStatus.Error, DateTime.Now));
                        }
                    }
                }
                catch (Exception ex)
                {
                    string exMessage = Common.GetInnermostException(ex);
                    _isResultError = true;
                    Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occured while identifying integration type: " + exMessage);
                    _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, "Error occured while identifying integration type.", Enums.SyncStatus.Error, DateTime.Now));
                }
                int integrationinstanceId = Convert.ToInt32(_integrationInstanceId);
                using (MRPEntities dbouter = new MRPEntities())
                {
                    IntegrationInstance integrationInstance = new IntegrationInstance();
                    integrationInstance = dbouter.IntegrationInstances.SingleOrDefault(instance => instance.IntegrationInstanceId == integrationinstanceId);
                        
                    if (_isResultError)
                    {
                                LogEndStatus = StatusResult.Error.ToString();
                        integrationInstance.LastSyncStatus = StatusResult.Error.ToString();
                        if (!IsAuthenticationError)
                        {
                                    string errorSections = string.Join(", ", dbouter.IntegrationInstanceSections.Where(integrationSection => integrationSection.IntegrationInstanceLogId == integrationinstanceLogId && integrationSection.Status == "Error").Select(integrationSection => integrationSection.SectionName).ToList());
                                    LogEndErrorDescription = "Error in section(s): " + errorSections;
                        }
                    }
                    else
                    {
                                LogEndStatus = StatusResult.Success.ToString();
                        integrationInstance.LastSyncStatus = StatusResult.Success.ToString();
                    }
                    integrationInstance.LastSyncDate = DateTime.Now;

                    // Modified by Viral Kadiya related to PL ticket #1449 - Display Last Auto Sync Date, Last Force Sync Date & Last Force Sync User.
                    if (!Common.IsAutoSync)
                        integrationInstance.ForceSyncUser = _userId;
                    
                    dbouter.Entry(integrationInstance).State = EntityState.Modified;
                    dbouter.SaveChanges();
                }

                IntegrationInstanceLog instanceLogEnd = db.IntegrationInstanceLogs.SingleOrDefault(instance => instance.IntegrationInstanceLogId == integrationinstanceLogId);
                instanceLogEnd.Status = LogEndStatus;
                instanceLogEnd.ErrorDescription = LogEndErrorDescription;
                instanceLogEnd.IsAutoSync = Common.IsAutoSync;      // PL ticket #1449: Update IntegrationInstanceLog that Sync process by Auth or Manual.
                db.Entry(instanceLogEnd).State = EntityState.Modified;

                instanceLogStart.SyncEnd = DateTime.Now;
                db.Entry(instanceLogStart).State = EntityState.Modified;
                db.SaveChanges();

                //// Start - Added by Sohel Pathan on 09/01/2015 for PL ticket #1068
                _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("End Time", instanceLogStart.SyncEnd.ToString()), Enums.SyncStatus.Header, DateTime.Now));
                TimeSpan ElapsedTicks = instanceLogStart.SyncEnd.Value.Subtract(instanceLogStart.SyncStart);
                DateTime ElapsedTime = new DateTime(ElapsedTicks.Ticks);
                _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Elapsed Time", ElapsedTime.ToString("HH:mm:ss")), Enums.SyncStatus.Header, DateTime.Now));
                if (_integrationInstanceId.HasValue)
                {
                    string InstanceName = Common.GetInstanceName(_integrationInstanceId.Value);
                    _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Instance Name used for syncing", InstanceName), Enums.SyncStatus.Header, DateTime.Now));
                }
                //// End - Added by Sohel Pathan on 09/01/2015 for PL ticket #1068
                //}
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while proccessing IdentifyIntegration : " + exMessage);
            }
        }

        /// <summary>
        /// Function to prepare sync error email header data
        /// </summary>
        private void PrepareSyncErroMailHeader(string integrationInstanceType)
        {
            //// get userdetails of the logged in user
            string ClientName = string.Empty;
            try
            {
                BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
                ClientName = objBDSUserRepository.GetClientNameEx(_userId);
                _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Client Name", ClientName), Enums.SyncStatus.Header, DateTime.Now));
            }
            catch
            {
                //// Service related exception
                return;
            }

            if (_isInActiveInstance) // Integration Instance Inactive then do not need to show Number of Activities and Pull information in Summary email.
                return;

            //// Set info row data for no of tactic processed with count
            Int32 SuccessTacticCount = 0;
            SuccessTacticCount = _lstAllSyncError.Where(syncError => syncError.SyncStatus == Enums.SyncStatus.Success && syncError.EntityId > 0)
                                                    .GroupBy(item => new { EntityId = item.EntityId })
                                                    .Select(groupItem => groupItem).Count();

            Int32 FailedTacticCount = 0;
            FailedTacticCount = _lstAllSyncError.Where(syncError => syncError.SyncStatus == Enums.SyncStatus.Error && syncError.EntityId > 0)
                                                    .GroupBy(item => new { EntityId = item.EntityId })
                                                    .Select(groupItem => groupItem).Count();

            Int32 TotalTacticCount = 0;
            TotalTacticCount = SuccessTacticCount + FailedTacticCount;

            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Number of Activities available for syncing - Push Tactic Data", TotalTacticCount.ToString()), Enums.SyncStatus.Header, DateTime.Now));
            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Number of Activities successfully synced - Push Tactic Data", SuccessTacticCount.ToString()), Enums.SyncStatus.Header, DateTime.Now));
            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Number of Activities failed due to some reason - Push Tactic Data", FailedTacticCount.ToString()), Enums.SyncStatus.Header, DateTime.Now));

            #region "Instance wise add Pull process status to Summary Email"
            bool isImport = false;
            if (_integrationInstanceId.HasValue)
            {
                isImport = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId.Equals(_integrationInstanceId.Value)).IsImportActuals;
            }
            if (isImport)
            {
                #region "Get Client wise MQL Permission"
                int IntegrationTypeId=0;
                int ClientId = 0;
                IntegrationInstance objInstance = db.IntegrationInstances.Where(inst => inst.IntegrationInstanceId == _integrationInstanceId.Value).FirstOrDefault();
                bool isMQLPermission = false;
                if(objInstance != null && objInstance.IntegrationInstanceId >0)
                {
                    string strPermissionCode_MQL = Enums.ClientIntegrationPermissionCode.MQL.ToString();
                    IntegrationTypeId= objInstance.IntegrationTypeId;
                    ClientId = objInstance.ClientId;
                    if (db.Client_Integration_Permission.Any(intPermission => (intPermission.ClientId.Equals(ClientId)) && (intPermission.IntegrationTypeId.Equals(IntegrationTypeId)) && (intPermission.PermissionCode.ToUpper().Equals(strPermissionCode_MQL.ToUpper()))))
                    {
                        isMQLPermission = true;
                    }
                } 
                #endregion

                if (integrationInstanceType.Equals(Enums.IntegrationType.Eloqua.ToString()))
                {
                    #region "Pull MQL Status"
                    if (isMQLPermission) // if client has MQL permission then render MQL status to Email
                    {
                        bool PullMQLError = false;
                        PullMQLError = _lstAllSyncError.Where(syncError => syncError.SyncStatus == Enums.SyncStatus.Error && syncError.SectionName == Enums.IntegrationInstanceSectionName.PullMQL.ToString()).Any();
                        if (PullMQLError)
                        {
                            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Pull MQL Sync Status - ", "Failed"), Enums.SyncStatus.Header, DateTime.Now));
                        }
                        else
                        {
                            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Pull MQL Sync Status - ", "Success"), Enums.SyncStatus.Header, DateTime.Now));
                        }
                    }
                    #endregion

                    #region "Pull Responses Status"
                    bool PullResponsesError = false;
                    PullResponsesError = _lstAllSyncError.Where(syncError => syncError.SyncStatus == Enums.SyncStatus.Error && syncError.SectionName == Enums.IntegrationInstanceSectionName.PullResponses.ToString()).Any();
                    if (PullResponsesError)
                    {
                        _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Pull Responses Sync Status - ", "Failed"), Enums.SyncStatus.Header, DateTime.Now));
                    }
                    else
                    {
                        _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Pull Responses Sync Status - ", "Success"), Enums.SyncStatus.Header, DateTime.Now));
                    }
                    #endregion
                }
                else if (integrationInstanceType.Equals(Enums.IntegrationType.Salesforce.ToString()))
                {
                    #region "Pull Closed Deals Status"
                    bool PullClosedDealsError = false;
                    PullClosedDealsError = _lstAllSyncError.Where(syncError => syncError.SyncStatus == Enums.SyncStatus.Error && syncError.SectionName == Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString()).Any();
                    if (PullClosedDealsError)
                    {
                        _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Pull Closed Deals Sync Status - ", "Failed"), Enums.SyncStatus.Header, DateTime.Now));
                    }
                    else
                    {
                        _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Pull Closed Deals Sync Status - ", "Success"), Enums.SyncStatus.Header, DateTime.Now));
                    }
                    #endregion

                    #region "Pull Responses Status"
                    bool PullResponsesError = false;
                    PullResponsesError = _lstAllSyncError.Where(syncError => syncError.SyncStatus == Enums.SyncStatus.Error && syncError.SectionName == Enums.IntegrationInstanceSectionName.PullResponses.ToString()).Any();
                    if (PullResponsesError)
                    {
                        _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Pull Responses Sync Status - ", "Failed"), Enums.SyncStatus.Header, DateTime.Now));
                    }
                    else
                    {
                        _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Pull Responses Sync Status - ", "Success"), Enums.SyncStatus.Header, DateTime.Now));
                    }
                    #endregion

                    #region "Pull MQL Status"
                    if (isMQLPermission) // if client has MQL permission then render MQL status to Email
                    {
                        bool PullMQLError = false;
                        PullMQLError = _lstAllSyncError.Where(syncError => syncError.SyncStatus == Enums.SyncStatus.Error && syncError.SectionName == Enums.IntegrationInstanceSectionName.PullMQL.ToString()).Any();
                        if (PullMQLError)
                        {
                            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Pull MQL Sync Status - ", "Failed"), Enums.SyncStatus.Header, DateTime.Now));
                        }
                        else
                        {
                            _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, Common.PrepareInfoRow("Pull MQL Sync Status - ", "Success"), Enums.SyncStatus.Header, DateTime.Now));
                        } 
                    }
                    #endregion
                }
            } 
            #endregion
            /////
        }

        /// <summary>
        /// Function to send an error email while IntegrationInstance sync of Eloqua
        /// </summary>
        private void SendSyncErrorEmail(string integrationInstanceType)
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {
                if (_lstAllSyncError.Count > 0)
                {
                    //// In sucess case there is no need of sending error email.
                    //bool isMailSendRequired = _lstAllSyncError.Where(syncError => syncError.SyncStatus != Enums.SyncStatus.Header && syncError.SyncStatus != Enums.SyncStatus.Success).Any();
                    //if (isMailSendRequired)
                    //{
                        //// Retrieve mail template from db.
                        MRPEntities db = new MRPEntities();
                        string SyncIntegrationError = Enums.Custom_Notification.SyncIntegrationError.ToString();
                        Notification objNotification = (Notification)db.Notifications.FirstOrDefault(n => n.NotificationInternalUseOnly.Equals(SyncIntegrationError));

                        if (objNotification != null)
                        {
                            PrepareSyncErroMailHeader(integrationInstanceType);

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
                    //}
                }
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while sending Summary Email : " + exMessage);
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
                    return integrationSalesforceClient.GetSFDCObjectList("Campaign");
                    //return integrationSalesforceClient.GetTargetDataType("Campaign");
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
            //Added by Brad Gray for ticket # 1367, Modified y Brad Gray 20 March 2016 PL#2070
            else if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.WorkFront.ToString()))
            {
                IntegrationWorkFrontSession integrationWorkFrontClient = new IntegrationWorkFrontSession(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0, _applicationId);
                return integrationWorkFrontClient.getWorkFrontFields();
            }
            else if (_integrationType.Equals(Integration.Helper.Enums.IntegrationType.Marketo.ToString())) //Added by Rahul Shah on 18/05/2016 for PL#2184
            {

                ApiIntegration integrationMarketoClient = new ApiIntegration(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0, _applicationId);
                //if (integrationMarketoClient.IsAuthenticated)
                {
                    return integrationMarketoClient.GetTargetCustomTags().Select(ext => ext.Key).ToList();                    
                }
            }
            return null;
        }

        /// <summary>
        /// Added By Dharmraj on 8-8-2014, Ticket #658
        /// </summary>
        /// <returns>Returns list of properties of salesforce opportunuty object</returns>
        public List<PullClosedDealModel> GetTargetDataMemberCloseDeal()
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
                    return integrationSalesforceClient.GetSFDCPullClosedDealsObjectList("Opportunity");
                    //return integrationSalesforceClient.GetPullClosedDealsTargetDataType("Opportunity");
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
                    return integrationSalesforceClient.GetSFDCObjectList("CampaignMember");
                    //return integrationSalesforceClient.GetTargetDataType("CampaignMember");
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

        /// <summary>
        /// Get SFDC Id with tactic name from salesforce and update ids into plan
        /// Created by : Nishant Sheth
        /// </summary>
        public void SyncSFDCMarketo()
        {
            IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient(_id, 0, _entityType, _userId, 0, _applicationId);
            if(integrationSalesforceClient.IsAuthenticated)
            {
                integrationSalesforceClient.SyncSFDCMarketo();
            }

        }
    }
}

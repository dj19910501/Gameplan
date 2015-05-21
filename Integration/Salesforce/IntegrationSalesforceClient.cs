using RevenuePlanner.Models;
using SalesforceSharp;
using SalesforceSharp.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Transactions;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Configuration;
using Integration.Helper;

/*
 *  Author: 
 *  Created Date: 
 *  Purpose: Integration with salesforce  
  */

namespace Integration.Salesforce
{
    public enum StageValue
    {
        INQ,
        MQL,
        CW,
        Revenue
    }

    public class IntegrationSalesforceClient
    {
        static readonly Guid BDSApplicationCode = Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]);

        public string _username { get; set; }
        public string _password { get; set; }
        public string _consumerKey { get; set; }
        public string _consumerSecret { get; set; }
        public string _securityToken { get; set; }
        public string _apiURL { get; set; }

        private MRPEntities db = new MRPEntities();
        private SalesforceClient _client { get; set; }
        private Dictionary<string, string> _mappingCampaign { get; set; }
        private Dictionary<string, string> _mappingProgram { get; set; }
        private Dictionary<string, string> _mappingTactic { get; set; }
        private Dictionary<string, string> _mappingImprovementCampaign { get; set; }
        private Dictionary<string, string> _mappingImprovementProgram { get; set; }
        private Dictionary<string, string> _mappingImprovementTactic { get; set; }
        private Dictionary<Guid, string> _mappingUser { get; set; }
        private Dictionary<string, string> _mappingCustomFields { get; set; }      // Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
        private int _integrationInstanceId { get; set; }
        private int _id { get; set; }
        private Guid _userId { get; set; }
        private int _integrationInstanceLogId { get; set; }
        private EntityType _entityType { get; set; }
        private readonly string objectName;
        private string _parentId { get; set; }
        private string ColumnParentId = "ParentId";
        private string ColumnId = "Id";
        private string UnableToUpdate = "Unable to update";
        private string UnableToDelete = "Unable to delete";
        private bool _isAuthenticated { get; set; }
        private bool _isResultError { get; set; }
        public string _ErrorMessage { get; set; }
        private int _integrationInstanceSectionId { get; set; }
        //Start - Added by Mitesh Vaishnav for PL ticket #1002 Custom Naming: Integration
        private Guid _clientId { get; set; }
        private bool _CustomNamingPermissionForInstance = false;
        private bool IsClientAllowedForCustomNaming = false;
        Guid _applicationId = Guid.Empty;
        //End - Added by Mitesh Vaishnav for PL ticket #1002 Custom Naming: Integration
        private Dictionary<int, string> _mappingTactic_ActualCost { get; set; }

        public bool IsAuthenticated
        {
            get
            {
                return _isAuthenticated;
            }
        }

        public IntegrationSalesforceClient()
        {
        }

        public IntegrationSalesforceClient(int integrationInstanceId, int id, EntityType entityType, Guid userId, int integrationInstanceLogId, Guid applicationId)
        {
            _integrationInstanceId = integrationInstanceId;
            _id = id;
            _entityType = entityType;
            _userId = userId;
            _integrationInstanceLogId = integrationInstanceLogId;
            _applicationId = applicationId;
            this.objectName = "Campaign";

            SetIntegrationInstanceDetail();
            //// Authenticate
            this.Authenticate();
        }

        private void SetIntegrationInstanceDetail()
        {
            string ConsumerKey = "ConsumerKey";
            string ConsumerSecret = "ConsumerSecret";
            string SecurityToken = "SecurityToken";

            IntegrationInstance integrationInstance = db.IntegrationInstances.Where(instance => instance.IntegrationInstanceId == _integrationInstanceId).FirstOrDefault();
            _CustomNamingPermissionForInstance = integrationInstance.CustomNamingPermission;
            Dictionary<string, string> attributeKeyPair = db.IntegrationInstance_Attribute.Where(attribute => attribute.IntegrationInstanceId == _integrationInstanceId).Select(attribute => new { attribute.IntegrationTypeAttribute.Attribute, attribute.Value }).ToDictionary(attribute => attribute.Attribute, attribute => attribute.Value);
            //// Get integration instance and set below properties using integrationInstanceId property.
            this._securityToken = attributeKeyPair[SecurityToken]; //"6to1YjygSTkAiZUusnuoJBAN";
            this._consumerKey = attributeKeyPair[ConsumerKey]; // "3MVG9zJJ_hX_0bb.x24JN3A5KwgO2gmkr5JfDDUx6U8FrvE_cFweCf7y3OkkLZeSkQDraDWZIrFcNqSvnAil_";
            this._consumerSecret = attributeKeyPair[ConsumerSecret];  //"2775499149223461438";
            this._username = integrationInstance.Username;//"brijmohan.bhavsar@indusa.com";
            this._password = Common.Decrypt(integrationInstance.Password); //"brijmohan";
            this._apiURL = integrationInstance.IntegrationType.APIURL; //"https://test.salesforce.com/services/oauth2/token";
        }

        public void Authenticate()
        {
            _client = new SalesforceClient();
            var authFlow = new UsernamePasswordAuthenticationFlow(_consumerKey, _consumerSecret, _username, _password + _securityToken);
            authFlow.TokenRequestEndpointUrl = _apiURL;
            try
            {
                _client.Authenticate(authFlow);
                _isAuthenticated = true;
            }
            catch (SalesforceException ex)
            {
                //Console.WriteLine("Authentication failed: {0} : {1}", ex.Error, ex.Message);
                _ErrorMessage = GetErrorMessage(ex);
                _isAuthenticated = false;
            }
        }

        /// <summary>
        /// Modified By Dharmraj on 6-8-2014, Ticket #658
        /// </summary>
        /// <param name="objectName">Sales force object name</param>
        /// <returns>Returns property list of salesforce object</returns>
        public List<string> GetTargetDataType(string objectName)
        {
            List<string> TargetDataTypeList = new List<string>();
            string metadata = _client.ReadMetaData(objectName);
            JObject data = JObject.Parse(metadata);
            foreach (var result in data["fields"])
            {
                TargetDataTypeList.Add((string)result["name"]);
            }
            return TargetDataTypeList.OrderBy(q => q).ToList();
        }

        /// <summary>
        /// Function to sync data from gameplan to salesforce.
        /// </summary>
        /// <returns>returns flag for sync status</returns>
        public bool SyncData()
        {
            // Insert log into IntegrationInstanceSection, Dharmraj PL#684
            _integrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), DateTime.Now, _userId);
            _isResultError = false;
            SetMappingDetails();
            bool IsInstanceSync = false;

            try
            {
                if (EntityType.Tactic.Equals(_entityType))
                {
                    Plan_Campaign_Program_Tactic planTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == _id).FirstOrDefault();
                    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    List<int> tacticIdList = new List<int>() { planTactic.PlanTacticId };
                    _mappingTactic_ActualCost = Common.CalculateActualCostTacticslist(tacticIdList);
                    _mappingCustomFields = CreateMappingCustomFieldDictionary(tacticIdList, Enums.EntityType.Tactic.ToString());
                    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    planTactic = SyncTacticData(planTactic);
                    db.SaveChanges();
                }
                else if (EntityType.Program.Equals(_entityType))
                {
                    Plan_Campaign_Program planProgram = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == _id).FirstOrDefault();
                    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    List<int> programIdList = new List<int>() { planProgram.PlanProgramId };
                    _mappingCustomFields = CreateMappingCustomFieldDictionary(programIdList, Enums.EntityType.Program.ToString());
                    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    planProgram = SyncProgramData(planProgram);
                    db.SaveChanges();
                }
                else if (EntityType.Campaign.Equals(_entityType))
                {
                    Plan_Campaign planCampaign = db.Plan_Campaign.Where(campaign => campaign.PlanCampaignId == _id).FirstOrDefault();
                    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    List<int> campaignIdList = new List<int>() { planCampaign.PlanCampaignId };
                    _mappingCustomFields = CreateMappingCustomFieldDictionary(campaignIdList, Enums.EntityType.Campaign.ToString());
                    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    planCampaign = SyncCampaingData(planCampaign);
                    db.SaveChanges();
                }
                else if (EntityType.ImprovementTactic.Equals(_entityType))
                {
                    Plan_Improvement_Campaign_Program_Tactic planImprovementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(imptactic => imptactic.ImprovementPlanTacticId == _id).FirstOrDefault();
                    planImprovementTactic = SyncImprovementData(planImprovementTactic);
                    db.SaveChanges();
                }
                else
                {
                    IsInstanceSync = true;
                    SyncInstanceData();
                }
            }
            catch (Exception e)
            {
                _isResultError = true;
                _ErrorMessage = e.Message;
            }

            if (_isResultError)
            {
                // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                Common.UpdateIntegrationInstanceSection(_integrationInstanceSectionId, StatusResult.Error, _ErrorMessage);
            }
            else
            {
                // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                Common.UpdateIntegrationInstanceSection(_integrationInstanceSectionId, StatusResult.Success, string.Empty);
            }

            if (IsInstanceSync)
            {
                bool isImport = false;
                isImport = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId.Equals(_integrationInstanceId)).IsImportActuals;
                if (isImport)
                {
                    //GetDataForTacticandUpdate();  // Commented by Sohel Pathan on 12/09/2014 for PL ticket #773
                    PullingResponses();
                    PullingCWRevenue();
                }
            }

            return _isResultError;
        }

        private class CampaignMember
        {
            public string CampaignId { get; set; }
            public DateTime FirstRespondedDate { get; set; }
        }

        private void PullingResponses()
        {
            // Insert log into IntegrationInstanceSection, Dharmraj PL#684
            int IntegrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), DateTime.Now, _userId);

            List<Plan> lstPlans = db.Plans.Where(p => p.Model.IntegrationInstanceIdINQ == _integrationInstanceId && p.Model.Status.Equals("Published")).ToList();
            Guid ClientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;

            //// Get Eloqua integration type Id.
            var eloquaIntegrationType = db.IntegrationTypes.Where(type => type.Code == "Eloqua" && type.IsDeleted == false).Select(type => type.IntegrationTypeId).FirstOrDefault();
            int eloquaIntegrationTypeId = Convert.ToInt32(eloquaIntegrationType);

            //// Get All EloquaIntegrationTypeIds to retrieve  EloquaPlanIds.
            List<int> lstEloquaIntegrationTypeIds = db.IntegrationInstances.Where(instance => instance.IntegrationTypeId.Equals(eloquaIntegrationTypeId) && instance.IsDeleted.Equals(false) && instance.ClientId.Equals(ClientId)).Select(s => s.IntegrationInstanceId).ToList();

            //// Get all PlanIds whose Tactic data PUSH on Eloqua.
            List<int> lstEloquaPlanIds = lstPlans.Where(objplan => lstEloquaIntegrationTypeIds.Contains(objplan.Model.IntegrationInstanceId.Value)).Select(objplan => objplan.PlanId).ToList();

            //// Get All PlanIds.
            List<int> AllplanIds = lstPlans.Select(objplan => objplan.PlanId).ToList();

            //// Get SalesForce PlanIds.
            List<int> lstSalesForceplanIds = lstPlans.Where(objplan => !lstEloquaPlanIds.Contains(objplan.PlanId)).Select(plan => plan.PlanId).ToList();

            int INQStageId = db.Stages.FirstOrDefault(s => s.ClientId == ClientId && s.Code == Common.StageINQ && s.IsDeleted == false).StageId;
            // Get List of status after Approved Status
            List<string> statusList = Common.GetStatusListAfterApproved();

            //// Get All Approved,IsDeployedToIntegration true and IsDeleted false Tactic list.
            List<Plan_Campaign_Program_Tactic> lstAllTactics = db.Plan_Campaign_Program_Tactic.Where(tactic => AllplanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
                                                                                                           tactic.IsDeployedToIntegration == true &&
                                                                                                           statusList.Contains(tactic.Status) &&
                                                                                                           tactic.IsDeleted == false).ToList();

            //// Get list of EloquaIntegrationInstanceTacticID(EloquaId).
            List<EloquaIntegrationInstanceTactic_Model_Mapping> lstEloquaIntegrationInstanceTacticIds = lstAllTactics.Where(tactic => lstEloquaPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && tactic.IntegrationInstanceTacticId != null).Select(_tac => new EloquaIntegrationInstanceTactic_Model_Mapping
                                                                                                                                                                                                                                                                                     {
                                                                                                                                                                                                                                                                                         EloquaIntegrationInstanceTacticId = _tac.IntegrationInstanceTacticId,
                                                                                                                                                                                                                                                                                         ModelIntegrationInstanceId = _tac.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceId.Value
                                                                                                                                                                                                                                                                                     }).ToList();

            if (lstEloquaIntegrationInstanceTacticIds == null)
                lstEloquaIntegrationInstanceTacticIds = new List<EloquaIntegrationInstanceTactic_Model_Mapping>();

            //// Add IntegrationEloquaClient object to Mapping list for distinct ModelIntegrationInstanceId.
            //// Get Mapping List of SalesForceIntegrationInstanceTactic Ids(CRMIds) based on EloquaIntegrationInstanceTacticID(EloquaId).
            List<CRM_EloquaMapping> lstSalesForceIntegrationInstanceTacticIds = new List<CRM_EloquaMapping>();
            foreach (int _ModelIntegrationInstanceId in lstEloquaIntegrationInstanceTacticIds.Select(tac => tac.ModelIntegrationInstanceId).Distinct())
            {
                try
                {
                    Integration.Eloqua.IntegrationEloquaClient integrationEloquaClient = new Integration.Eloqua.IntegrationEloquaClient(_ModelIntegrationInstanceId, 0, _entityType, _userId, _integrationInstanceLogId, _applicationId);
                    foreach (EloquaIntegrationInstanceTactic_Model_Mapping _EloquaTac in lstEloquaIntegrationInstanceTacticIds.Where(_eloqua => _eloqua.ModelIntegrationInstanceId.Equals(_ModelIntegrationInstanceId)))
                    {
                        //objEloqua.IntegrationEloquaClient = integrationEloquaClient;
                        if (!string.IsNullOrEmpty(_EloquaTac.EloquaIntegrationInstanceTacticId))
                        {

                            Integration.Eloqua.EloquaCampaign objEloqua = new Integration.Eloqua.EloquaCampaign();

                            ////Get SalesForceIntegrationTacticId based on EloquaIntegrationTacticId.
                            objEloqua = integrationEloquaClient.GetEloquaCampaign(_EloquaTac.EloquaIntegrationInstanceTacticId);
                            if (objEloqua != null)
                            {
                                var _Tactic = lstAllTactics.Where(s => s.IntegrationInstanceTacticId == _EloquaTac.EloquaIntegrationInstanceTacticId).Select(_tac => _tac).FirstOrDefault();
                                lstSalesForceIntegrationInstanceTacticIds.Add(
                                                                          new CRM_EloquaMapping
                                                                          {
                                                                              CRMId = !string.IsNullOrEmpty(objEloqua.crmId) ? objEloqua.crmId : string.Empty,
                                                                              EloquaId = _EloquaTac.EloquaIntegrationInstanceTacticId,
                                                                              PlanTacticId = _Tactic != null ? _Tactic.PlanTacticId : 0,
                                                                              StartDate = _Tactic != null ? _Tactic.StartDate : (new DateTime()),
                                                                          });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            //// Get SalesForce tactic list
            List<Plan_Campaign_Program_Tactic> lstSalesForceTactic = lstAllTactics.Where(_tac => lstSalesForceplanIds.Contains(_tac.Plan_Campaign_Program.Plan_Campaign.PlanId) && _tac.StageId == INQStageId && _tac.IntegrationInstanceTacticId != null).ToList();
            string SalesForceintegrationTacticIds = String.Join("','", (from tactic in lstSalesForceTactic select tactic.IntegrationInstanceTacticId));

            //// Get Eloqua tactic list
            List<Plan_Campaign_Program_Tactic> lstEloquaTactic = lstAllTactics.Where(tactic => lstEloquaPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && tactic.StageId == INQStageId && tactic.IntegrationInstanceTacticId != null).ToList();
            string SalesForceintegrationTacticIdsByEloquaId = String.Join("','", from _Tactic in lstEloquaTactic
                                                                                 join _ElqTactic in lstSalesForceIntegrationInstanceTacticIds on _Tactic.IntegrationInstanceTacticId equals _ElqTactic.EloquaId
                                                                                 select _ElqTactic.CRMId);

            //// Merge SalesForce & Eloqua IntegrationTacticIds by comma(',').
            string AllIntegrationTacticIds = !string.IsNullOrEmpty(SalesForceintegrationTacticIdsByEloquaId) ? (!string.IsNullOrEmpty(SalesForceintegrationTacticIds) ? (SalesForceintegrationTacticIds + "','" + SalesForceintegrationTacticIdsByEloquaId) : SalesForceintegrationTacticIdsByEloquaId) : SalesForceintegrationTacticIds;
            AllIntegrationTacticIds = AllIntegrationTacticIds.Trim(new char[] { ',' });
            //For Testing
            //integrationTacticIds = "'701f00000003S9R','701f00000002cGG','701f00000002cGL'";
            if (AllIntegrationTacticIds != string.Empty)
            {
                try
                {
                    string CampaignId = string.Empty;// "CampaignId";
                    string FirstRespondedDate = string.Empty;//"FirstRespondedDate";
                    string Status = string.Empty;//"Status";

                    var listPullMapping = db.IntegrationInstanceDataTypeMappingPulls.Where(instance => instance.IntegrationInstanceId == _integrationInstanceId && instance.GameplanDataTypePull.Type == Common.StageINQ)
                       .Select(mapping => new { mapping.GameplanDataTypePull.ActualFieldName, mapping.TargetDataType }).ToList();
                    bool ErrorFlag = false;
                    if (listPullMapping.Count > 0)
                    {
                        if (listPullMapping.Where(mapping => mapping.ActualFieldName == Enums.PullResponseActualField.CampaignID.ToString()).Any())
                        {
                            CampaignId = listPullMapping.FirstOrDefault(mapping => mapping.ActualFieldName == Enums.PullResponseActualField.CampaignID.ToString()).TargetDataType;
                        }
                        if (listPullMapping.Where(mapping => mapping.ActualFieldName == Enums.PullResponseActualField.Timestamp.ToString()).Any())
                        {
                            FirstRespondedDate = listPullMapping.FirstOrDefault(mapping => mapping.ActualFieldName == Enums.PullResponseActualField.Timestamp.ToString()).TargetDataType;
                        }
                        if (listPullMapping.Where(mapping => mapping.ActualFieldName == Enums.PullResponseActualField.Status.ToString()).Any())
                        {
                            Status = listPullMapping.FirstOrDefault(mapping => mapping.ActualFieldName == Enums.PullResponseActualField.Status.ToString()).TargetDataType;
                        }

                        if (CampaignId != string.Empty && FirstRespondedDate != string.Empty && Status != string.Empty)
                        {
                            List<CampaignMember> CampaignMemberList = new List<CampaignMember>();
                            var responsePull = _client.Query<object>("SELECT " + CampaignId + "," + FirstRespondedDate + " FROM CampaignMember WHERE " + CampaignId + " IN ('" + AllIntegrationTacticIds + "') AND " + Status + "= '" + Common.Responded + "'");

                            foreach (var resultin in responsePull)
                            {
                                string TacticResult = resultin.ToString();
                                JObject jobj = JObject.Parse(TacticResult);
                                CampaignMember objCampaign = new CampaignMember();
                                int _PlanTacticId = 0;
                                try
                                {
                                    string campaignid = Convert.ToString(jobj[CampaignId]);
                                    if (!AllIntegrationTacticIds.Contains(campaignid))
                                    {
                                        campaignid = campaignid.Substring(0, 15);
                                    }
                                    objCampaign.CampaignId = campaignid;
                                    objCampaign.FirstRespondedDate = Convert.ToDateTime(jobj[FirstRespondedDate]);
                                    CampaignMemberList.Add(objCampaign);
                                }
                                catch (SalesforceException e)
                                {
                                    ErrorFlag = true;
                                    _ErrorMessage = GetErrorMessage(e);
                                    string TacticId = Convert.ToString(jobj[CampaignId]); ////CRMId

                                    //// check whether TacticId(CRMId) exist in field IntegrationInstanceTacticID field of SalesForceTactic list.
                                    var tactic = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
                                    if (tactic != null)
                                        _PlanTacticId = tactic.PlanTacticId;
                                    else                                        //// if Tactic not exist then retrieve PlanTacticId from EloquaTactic list.
                                        _PlanTacticId = lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && _SalTac.CRMId.Equals(TacticId)).Select(s => s.PlanTacticId).FirstOrDefault();

                                    IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                    instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                    instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                    instanceTactic.EntityId = _PlanTacticId;
                                    instanceTactic.EntityType = EntityType.Tactic.ToString();
                                    instanceTactic.Status = StatusResult.Error.ToString();
                                    instanceTactic.Operation = Operation.Pull_Responses.ToString();
                                    instanceTactic.SyncTimeStamp = DateTime.Now;
                                    instanceTactic.CreatedDate = DateTime.Now;
                                    instanceTactic.ErrorDescription = GetErrorMessage(e);
                                    instanceTactic.CreatedBy = _userId;
                                    db.Entry(instanceTactic).State = EntityState.Added;
                                }
                                catch (Exception e)
                                {
                                    ErrorFlag = true;
                                    _ErrorMessage = e.Message;
                                    string TacticId = Convert.ToString(jobj[CampaignId]); ////CRMId

                                    //// check whether TacticId(CRMId) exist in field IntegrationInstanceTacticID field of SalesForceTactic list.
                                    var tactic = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
                                    if (tactic != null)
                                        _PlanTacticId = tactic.PlanTacticId;
                                    else                                        //// if Tactic not exist then retrieve PlanTacticId from EloquaTactic list.
                                        _PlanTacticId = lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && _SalTac.CRMId.Equals(TacticId)).Select(s => s.PlanTacticId).FirstOrDefault();

                                    IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                    instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                    instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                    instanceTactic.EntityId = _PlanTacticId;
                                    instanceTactic.EntityType = EntityType.Tactic.ToString();
                                    instanceTactic.Status = StatusResult.Error.ToString();
                                    instanceTactic.Operation = Operation.Pull_Responses.ToString();
                                    instanceTactic.SyncTimeStamp = DateTime.Now;
                                    instanceTactic.CreatedDate = DateTime.Now;
                                    instanceTactic.ErrorDescription = e.Message;
                                    instanceTactic.CreatedBy = _userId;
                                    db.Entry(instanceTactic).State = EntityState.Added;
                                }

                            }
                            List<Plan_Campaign_Program_Tactic> lstCRM_EloquaTactics = (from _Tactic in lstEloquaTactic
                                                                                       join _ElqTactic in lstSalesForceIntegrationInstanceTacticIds on _Tactic.IntegrationInstanceTacticId equals _ElqTactic.EloquaId
                                                                                       where _ElqTactic.CRMId != null
                                                                                       select _Tactic).ToList();

                            List<Plan_Campaign_Program_Tactic> lstMergedTactics = lstSalesForceTactic;
                            lstCRM_EloquaTactics.ForEach(_elqTactic => lstMergedTactics.Add(_elqTactic));

                            lstMergedTactics = lstMergedTactics.Distinct().ToList();
                            List<int> OuterTacticIds = lstMergedTactics.Select(t => t.PlanTacticId).ToList();
                            List<Plan_Campaign_Program_Tactic_Actual> OuteractualTacticList = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => OuterTacticIds.Contains(actual.PlanTacticId) && actual.StageTitle == Common.StageProjectedStageValue).ToList();
                            OuteractualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                            db.SaveChanges();

                            if (CampaignMemberList.Count > 0)
                            {
                                var CampaignMemberListGroup = CampaignMemberList.GroupBy(cl => new { CampaignId = cl.CampaignId, Month = cl.FirstRespondedDate.ToString("MM/yyyy") }).Select(cl =>
                                    new
                                    {
                                        CampaignId = cl.Key.CampaignId,
                                        TacticId = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null ? (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).PlanTacticId) : (lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && _SalTac.CRMId.Equals(cl.Key.CampaignId)).Select(s => s.PlanTacticId).FirstOrDefault()),
                                        Period = "Y" + Convert.ToDateTime(cl.Key.Month).Month,
                                        IsYear = (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year == Convert.ToDateTime(cl.Key.Month).Year)) || ((lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && _SalTac.CRMId.Equals(cl.Key.CampaignId)).Select(_SalTac => _SalTac.StartDate) != null) && (lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && _SalTac.CRMId.Equals(cl.Key.CampaignId)).Select(_SalTac => _SalTac.StartDate.Value).FirstOrDefault().Year == Convert.ToDateTime(cl.Key.Month).Year)) ? true : false,
                                        Count = cl.Count()
                                    }).Where(cm => cm.IsYear).ToList();

                                foreach (var tactic in lstMergedTactics)
                                {
                                    var innerCampaignMember = CampaignMemberListGroup.Where(cm => cm.TacticId == tactic.PlanTacticId).ToList();
                                    foreach (var objCampaignMember in innerCampaignMember)
                                    {
                                        Plan_Campaign_Program_Tactic_Actual objPlanTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                        objPlanTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                        objPlanTacticActual.PlanTacticId = objCampaignMember.TacticId;
                                        objPlanTacticActual.Period = objCampaignMember.Period;
                                        objPlanTacticActual.StageTitle = Common.StageProjectedStageValue;
                                        objPlanTacticActual.Actualvalue = objCampaignMember.Count;
                                        objPlanTacticActual.CreatedBy = _userId;
                                        objPlanTacticActual.CreatedDate = DateTime.Now;
                                        db.Entry(objPlanTacticActual).State = EntityState.Added;
                                    }

                                    tactic.LastSyncDate = DateTime.Now;
                                    tactic.ModifiedDate = DateTime.Now;
                                    tactic.ModifiedBy = _userId;

                                    IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                    instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                    instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                    instanceTactic.EntityId = tactic.PlanTacticId;
                                    instanceTactic.EntityType = EntityType.Tactic.ToString();
                                    instanceTactic.Status = StatusResult.Success.ToString();
                                    instanceTactic.Operation = Operation.Pull_Responses.ToString();
                                    instanceTactic.SyncTimeStamp = DateTime.Now;
                                    instanceTactic.CreatedDate = DateTime.Now;
                                    instanceTactic.CreatedBy = _userId;
                                    db.Entry(instanceTactic).State = EntityState.Added;
                                }

                                db.SaveChanges();
                            }

                            if (ErrorFlag)
                            {
                                _isResultError = true;
                                // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, _ErrorMessage);
                            }
                            else
                            {
                                // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                            }
                        }
                        else
                        {
                            _isResultError = true;
                            // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                            Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgMappingNotFoundForSalesforcePullResponse);
                        }
                    }
                    else
                    {
                        // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    }
                }
                catch (SalesforceException e)
                {
                    string msg = GetErrorMessage(e);
                    // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, msg);
                }
            }
            else
            {
                // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
            }

        }

        /// <summary>
        /// Tactic moved to another program then update program into integration system
        /// Created by : Mitesh Vaishnav
        /// </summary>
        /// <returns>If success then true else flase</returns>
        public bool SyncMovedTacticData()
        {
            _integrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), DateTime.Now, _userId);
            _isResultError = false;
            SetMappingDetails();
           
           
            bool IsInstanceSync = false;

            Plan_Campaign_Program_Tactic planTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == _id).FirstOrDefault();
            Plan_Campaign_Program planProgram = planTactic.Plan_Campaign_Program;
            _parentId = planProgram.IntegrationInstanceProgramId;
            if (string.IsNullOrWhiteSpace(_parentId))
            {
                Plan_Campaign planCampaign = planTactic.Plan_Campaign_Program.Plan_Campaign;
                _parentId = planCampaign.IntegrationInstanceCampaignId;
                if (string.IsNullOrWhiteSpace(_parentId))
                {
                    IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                    instanceLogCampaign.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                    instanceLogCampaign.IntegrationInstanceId = _integrationInstanceId;
                    instanceLogCampaign.EntityId = planCampaign.PlanCampaignId;
                    instanceLogCampaign.EntityType = EntityType.Campaign.ToString();
                    instanceLogCampaign.Operation = Operation.Create.ToString();
                    instanceLogCampaign.SyncTimeStamp = DateTime.Now;
                    try
                    {
                        List<int> campaignIdList = new List<int>() { planCampaign.PlanCampaignId };
                        var lstCustomFieldsCampaign = CreateMappingCustomFieldDictionary(campaignIdList, Enums.EntityType.Campaign.ToString());
                        if (lstCustomFieldsCampaign.Count > 0)
                        {
                            if (_mappingCustomFields != null)
                            {
                                _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsCampaign).ToDictionary(c => c.Key, c => c.Value);
                            }
                            else
                            {
                                //lstCustomFieldsCampaign.ToList().ForEach(c=>_mappingCustomFields.Add(c.Key,c.Value));
                                _mappingCustomFields = new Dictionary<string, string>();
                                foreach (var item in lstCustomFieldsCampaign)
                                {
                                    _mappingCustomFields.Add(item.Key.ToString(), item.Value.ToString());
                                }
                            }
                        }
                        _parentId = CreateCampaign(planCampaign);
                        planCampaign.IntegrationInstanceCampaignId = _parentId;
                        planCampaign.LastSyncDate = DateTime.Now;
                        planCampaign.ModifiedDate = DateTime.Now;
                        planCampaign.ModifiedBy = _userId;
                        db.Entry(planCampaign).State = EntityState.Modified;
                        instanceLogCampaign.Status = StatusResult.Success.ToString();
                    }
                    catch (SalesforceException e)
                    {
                        _isResultError = true;
                        instanceLogCampaign.Status = StatusResult.Error.ToString();
                        instanceLogCampaign.ErrorDescription = GetErrorMessage(e);
                    }
                    instanceLogCampaign.CreatedBy = this._userId;
                    instanceLogCampaign.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogCampaign).State = EntityState.Added;
                }

                if (!string.IsNullOrWhiteSpace(_parentId))
                {
                    IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
                    instanceLogProgram.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                    instanceLogProgram.IntegrationInstanceId = _integrationInstanceId;
                    instanceLogProgram.EntityId = planProgram.PlanProgramId;
                    instanceLogProgram.EntityType = EntityType.Program.ToString();
                    instanceLogProgram.Operation = Operation.Create.ToString();
                    instanceLogProgram.SyncTimeStamp = DateTime.Now;
                    try
                    {
                        // Start - Added by Sohel Pathan on 09/12/2014 for PL ticket #995, 996, & 997
                        List<int> programIdList = new List<int>() { planProgram.PlanProgramId };
                        var lstCustomFieldsProgram = CreateMappingCustomFieldDictionary(programIdList, Enums.EntityType.Program.ToString());
                        if (lstCustomFieldsProgram.Count > 0)
                        {
                            if (_mappingCustomFields != null)
                            {
                            _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsProgram).ToDictionary(c => c.Key, c => c.Value);
                            }
                            else{
                                //lstCustomFieldsProgram.ToList().ForEach(c=>_mappingCustomFields.Add(c.Key,c.Value));
                                _mappingCustomFields = new Dictionary<string, string>();
                                foreach (var item in lstCustomFieldsProgram)
                                {
                                    _mappingCustomFields.Add(item.Key.ToString(), item.Value.ToString());
                                }
                            }
                        }

                        // End - Added by Sohel Pathan on 09/12/2014 for PL ticket #995, 996, & 997

                        _parentId = CreateProgram(planProgram);
                        planProgram.IntegrationInstanceProgramId = _parentId;
                        planProgram.LastSyncDate = DateTime.Now;
                        planProgram.ModifiedDate = DateTime.Now;
                        planProgram.ModifiedBy = _userId;
                        instanceLogProgram.Status = StatusResult.Success.ToString();
                    }
                    catch (SalesforceException e)
                    {
                        _isResultError = true;
                        instanceLogProgram.Status = StatusResult.Error.ToString();
                        instanceLogProgram.ErrorDescription = GetErrorMessage(e);
                    }

                    instanceLogProgram.CreatedBy = this._userId;
                    instanceLogProgram.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogProgram).State = EntityState.Added;
                }
            }
            if (!string.IsNullOrWhiteSpace(_parentId))
            {
                IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                instanceLogTactic.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                instanceLogTactic.IntegrationInstanceId = _integrationInstanceId;
                instanceLogTactic.EntityId = planTactic.PlanTacticId;
                instanceLogTactic.EntityType = EntityType.Tactic.ToString();
                instanceLogTactic.Operation = Operation.Create.ToString();
                instanceLogTactic.SyncTimeStamp = DateTime.Now;
                try
                {
                    Dictionary<string, object> tactic = new Dictionary<string, object>();
                    tactic.Add(ColumnParentId, _parentId);
                    bool updateSuccess = _client.Update(objectName, planTactic.IntegrationInstanceTacticId, tactic);
                    if (updateSuccess)
                    {
                        planTactic.LastSyncDate = DateTime.Now;
                        planTactic.ModifiedDate = DateTime.Now;
                        planTactic.ModifiedBy = _userId;
                        instanceLogTactic.Status = StatusResult.Success.ToString();
                    }
                    else
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = UnableToUpdate;
                    }
                    // End Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                }
                catch (SalesforceException e)
                {
                    _isResultError = true;
                    instanceLogTactic.Status = StatusResult.Error.ToString();
                    instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }
            return _isResultError;
        }

        private class OpportunityMember
        {
            public string OpportunityId { get; set; }
            public string CampaignId { get; set; }
            public DateTime CloseDate { get; set; }
            public DateTime CreatedDate { get; set; }
            public double Amount { get; set; }
        }

        private class ContactRoleMember
        {
            public string OpportunityId { get; set; }
            public bool IsPrimary { get; set; }
            public string ContactId { get; set; }
            public int Count { get; set; }
        }

        private class ContactCampaignMember
        {
            public string CampaignId { get; set; }
            public DateTime RespondedDate { get; set; }
            public string ContactId { get; set; }
        }

        private void PullingCWRevenue()
        {
            // Insert log into IntegrationInstanceSection, Dharmraj PL#684
            int IntegrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), DateTime.Now, _userId);

            List<Plan> lstPlans = db.Plans.Where(p => p.Model.IntegrationInstanceIdCW == _integrationInstanceId && p.Model.Status.Equals("Published")).ToList();
            Guid ClientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;

            //// Get Eloqua integration type Id.
            var eloquaIntegrationType = db.IntegrationTypes.Where(type => type.Code == "Eloqua" && type.IsDeleted == false).Select(type => type.IntegrationTypeId).FirstOrDefault();
            int eloquaIntegrationTypeId = Convert.ToInt32(eloquaIntegrationType);

            //// Get All EloquaIntegrationTypeIds to retrieve  EloquaPlanIds.
            List<int> lstEloquaIntegrationTypeIds = db.IntegrationInstances.Where(instance => instance.IntegrationTypeId.Equals(eloquaIntegrationTypeId) && instance.IsDeleted.Equals(false) && instance.ClientId.Equals(ClientId)).Select(s => s.IntegrationInstanceId).ToList();

            //// Get all PlanIds whose Tactic data PUSH on Eloqua.
            List<int> lstEloquaPlanIds = lstPlans.Where(objplan => lstEloquaIntegrationTypeIds.Contains(objplan.Model.IntegrationInstanceId.Value)).Select(objplan => objplan.PlanId).ToList();

            //// Get All PlanIds.
            List<int> AllplanIds = lstPlans.Select(objplan => objplan.PlanId).ToList();

            //// Get SalesForce PlanIds.
            List<int> lstSalesForceplanIds = lstPlans.Where(objplan => !lstEloquaPlanIds.Contains(objplan.PlanId)).Select(plan => plan.PlanId).ToList();

            // Get List of status after Approved Status
            List<string> statusList = Common.GetStatusListAfterApproved();

            //// Get All Approved,IsDeployedToIntegration true and IsDeleted false Tactic list.
            List<Plan_Campaign_Program_Tactic> lstAllTactics = db.Plan_Campaign_Program_Tactic.Where(tactic => AllplanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
                                                                                                           tactic.IsDeployedToIntegration == true &&
                                                                                                           statusList.Contains(tactic.Status) &&
                                                                                                           tactic.IsDeleted == false).ToList();

            //// Get list of EloquaIntegrationInstanceTacticID(EloquaId).
            List<EloquaIntegrationInstanceTactic_Model_Mapping> lstEloquaIntegrationInstanceTacticIds = lstAllTactics.Where(tactic => lstEloquaPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && tactic.IntegrationInstanceTacticId != null).Select(_tac => new EloquaIntegrationInstanceTactic_Model_Mapping
            {
                EloquaIntegrationInstanceTacticId = _tac.IntegrationInstanceTacticId,
                ModelIntegrationInstanceId = _tac.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceId.Value
            }).ToList();

            if (lstEloquaIntegrationInstanceTacticIds == null)
                lstEloquaIntegrationInstanceTacticIds = new List<EloquaIntegrationInstanceTactic_Model_Mapping>();

            //// Add IntegrationEloquaClient object to Mapping list for distinct ModelIntegrationInstanceId.
            //// Get Mapping List of SalesForceIntegrationInstanceTactic Ids(CRMIds) based on EloquaIntegrationInstanceTacticID(EloquaId).
            List<CRM_EloquaMapping> lstSalesForceIntegrationInstanceTacticIds = new List<CRM_EloquaMapping>();
            foreach (int _ModelIntegrationInstanceId in lstEloquaIntegrationInstanceTacticIds.Select(tac => tac.ModelIntegrationInstanceId).Distinct())
            {
                try
                {
                    Integration.Eloqua.IntegrationEloquaClient integrationEloquaClient = new Integration.Eloqua.IntegrationEloquaClient(_ModelIntegrationInstanceId, 0, _entityType, _userId, _integrationInstanceLogId, _applicationId);
                    foreach (EloquaIntegrationInstanceTactic_Model_Mapping _EloquaTac in lstEloquaIntegrationInstanceTacticIds.Where(_eloqua => _eloqua.ModelIntegrationInstanceId.Equals(_ModelIntegrationInstanceId)))
                    {
                        if (!string.IsNullOrEmpty(_EloquaTac.EloquaIntegrationInstanceTacticId))
                        {

                            Integration.Eloqua.EloquaCampaign objEloqua = new Integration.Eloqua.EloquaCampaign();

                            ////Get SalesForceIntegrationTacticId based on EloquaIntegrationTacticId.
                            objEloqua = integrationEloquaClient.GetEloquaCampaign(_EloquaTac.EloquaIntegrationInstanceTacticId);
                            if (objEloqua != null)
                            {
                                var _Tactic = lstAllTactics.Where(s => s.IntegrationInstanceTacticId == _EloquaTac.EloquaIntegrationInstanceTacticId).Select(_tac => _tac).FirstOrDefault();
                                lstSalesForceIntegrationInstanceTacticIds.Add(
                                                                          new CRM_EloquaMapping
                                                                          {
                                                                              CRMId = !string.IsNullOrEmpty(objEloqua.crmId) ? objEloqua.crmId : string.Empty,
                                                                              EloquaId = _EloquaTac.EloquaIntegrationInstanceTacticId,
                                                                              PlanTacticId = _Tactic != null ? _Tactic.PlanTacticId : 0,
                                                                              StartDate = _Tactic != null ? _Tactic.StartDate : (new DateTime()),
                                                                          });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            //// Get SalesForce tactic list
            List<Plan_Campaign_Program_Tactic> lstSalesForceTactic = lstAllTactics.Where(_tac => lstSalesForceplanIds.Contains(_tac.Plan_Campaign_Program.Plan_Campaign.PlanId) && _tac.IntegrationInstanceTacticId != null).ToList();
            string SalesForceintegrationTacticIds = String.Join("','", (from tactic in lstSalesForceTactic select tactic.IntegrationInstanceTacticId));

            //// Get Eloqua tactic list
            List<Plan_Campaign_Program_Tactic> lstEloquaTactic = lstAllTactics.Where(tactic => lstEloquaPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && tactic.IntegrationInstanceTacticId != null).ToList();
            string SalesForceintegrationTacticIdsByEloquaId = String.Join("','", from _Tactic in lstEloquaTactic
                                                                                 join _ElqTactic in lstSalesForceIntegrationInstanceTacticIds on _Tactic.IntegrationInstanceTacticId equals _ElqTactic.EloquaId
                                                                                 select _ElqTactic.CRMId);

            //// Merge SalesForce & Eloqua IntegrationTacticIds by comma(',').
            string AllIntegrationTacticIds = !string.IsNullOrEmpty(SalesForceintegrationTacticIdsByEloquaId) ? (!string.IsNullOrEmpty(SalesForceintegrationTacticIds) ? (SalesForceintegrationTacticIds + "','" + SalesForceintegrationTacticIdsByEloquaId) : SalesForceintegrationTacticIdsByEloquaId) : SalesForceintegrationTacticIds;
            AllIntegrationTacticIds = AllIntegrationTacticIds.Trim(new char[] { ',' });

            if (AllIntegrationTacticIds != string.Empty)
            {
                try
                {
                    string CampaignId = string.Empty;// "CampaignId";
                    string CloseDate = string.Empty;// "CloseDate";
                    string Amount = string.Empty;// "Amount";
                    string StageName = string.Empty;// "StageName";
                    string ResponseDate = string.Empty;// "ResponseDate";
                    string LastModifiedDate = string.Empty;// "LastModifiedDate";
                    var listPullMapping = db.IntegrationInstanceDataTypeMappingPulls.Where(instance => instance.IntegrationInstanceId == _integrationInstanceId && instance.GameplanDataTypePull.Type == Common.StageCW)
                        .Select(mapping => new { mapping.GameplanDataTypePull.ActualFieldName, mapping.TargetDataType }).ToList();
                    bool ErrorFlag = false;
                    if (listPullMapping.Count > 0)
                    {
                        if (listPullMapping.Where(mapping => mapping.ActualFieldName == Enums.PullCWActualField.CampaignID.ToString()).Any())
                        {
                            CampaignId = listPullMapping.FirstOrDefault(mapping => mapping.ActualFieldName == Enums.PullCWActualField.CampaignID.ToString()).TargetDataType;
                        }
                        if (listPullMapping.Where(mapping => mapping.ActualFieldName == Enums.PullCWActualField.Timestamp.ToString()).Any())
                        {
                            CloseDate = listPullMapping.FirstOrDefault(mapping => mapping.ActualFieldName == Enums.PullCWActualField.Timestamp.ToString()).TargetDataType;
                        }
                        if (listPullMapping.Where(mapping => mapping.ActualFieldName == Enums.PullCWActualField.Amount.ToString()).Any())
                        {
                            Amount = listPullMapping.FirstOrDefault(mapping => mapping.ActualFieldName == Enums.PullCWActualField.Amount.ToString()).TargetDataType;
                        }
                        if (listPullMapping.Where(mapping => mapping.ActualFieldName == Enums.PullCWActualField.Stage.ToString()).Any())
                        {
                            StageName = listPullMapping.FirstOrDefault(mapping => mapping.ActualFieldName == Enums.PullCWActualField.Stage.ToString()).TargetDataType;
                        }
                        if (listPullMapping.Where(mapping => mapping.ActualFieldName == Enums.PullCWActualField.ResponseDate.ToString()).Any())
                        {
                            ResponseDate = listPullMapping.FirstOrDefault(mapping => mapping.ActualFieldName == Enums.PullCWActualField.ResponseDate.ToString()).TargetDataType;
                        }
                        if (listPullMapping.Where(mapping => mapping.ActualFieldName == Enums.PullCWActualField.LastModifiedDate.ToString()).Any())
                        {
                            LastModifiedDate = listPullMapping.FirstOrDefault(mapping => mapping.ActualFieldName == Enums.PullCWActualField.LastModifiedDate.ToString()).TargetDataType;
                        }

                        if (CampaignId != string.Empty && CloseDate != string.Empty && Amount != string.Empty && StageName != string.Empty && ResponseDate != string.Empty && LastModifiedDate != string.Empty)
                        {
                            bool isDoneFirstPullCW = false;
                            isDoneFirstPullCW = db.IntegrationInstances.Where(instance => instance.IntegrationInstanceId == _integrationInstanceId).Select(instance => instance.IsFirstPullCW).FirstOrDefault();
                            string opportunityGetQueryWhere = string.Empty;
                            string opportunityGetQuery = string.Empty;

                            string currentDate = DateTime.UtcNow.ToString(Common.DateFormatForSalesforce);
                            string cwsection = Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString();
                            string statusSuccess = StatusResult.Success.ToString();
                            var lastsync = isDoneFirstPullCW ? db.IntegrationInstanceSections.Where(instanceSection => instanceSection.IntegrationInstanceId == _integrationInstanceId && instanceSection.SectionName == cwsection && instanceSection.Status == statusSuccess).OrderByDescending(instancesection => instancesection.IntegrationInstanceSectionId).Select(instanceSection => instanceSection.SyncEnd).FirstOrDefault() :
                                                                lstAllTactics.OrderByDescending(tactic => tactic.CreatedDate).Select(tactic => tactic.CreatedDate).FirstOrDefault();
                            string lastSyncDate = string.Empty;
                            if (lastsync != null)
                            {
                                lastSyncDate = Convert.ToDateTime(lastsync).ToUniversalTime().ToString(Common.DateFormatForSalesforce);
                            }
                            if (lastSyncDate != string.Empty)
                            {
                                opportunityGetQueryWhere = " WHERE " + StageName + "= '" + Common.ClosedWon + "' AND " + LastModifiedDate + " > " + lastSyncDate + " AND " + LastModifiedDate + " < " + currentDate;
                            }
                            else
                            {
                                opportunityGetQueryWhere = " WHERE " + StageName + "= '" + Common.ClosedWon + "' AND " + LastModifiedDate + " < " + currentDate;
                            }

                            string opportunityRoleQuery = "SELECT ContactId,IsPrimary,OpportunityId FROM OpportunityContactRole WHERE OpportunityId IN (SELECT Id FROM Opportunity" + opportunityGetQueryWhere + ")";

                            opportunityGetQuery = "SELECT Id," + CampaignId + "," + CloseDate + "," + Amount + ",CreatedDate FROM Opportunity" + opportunityGetQueryWhere;
                            List<OpportunityMember> OpportunityMemberListInitial = new List<OpportunityMember>();
                            var cwRecords = _client.Query<object>(opportunityGetQuery);
                            int errorcount = 0;
                            foreach (var resultin in cwRecords)
                            {
                                string TacticResult = resultin.ToString();
                                JObject jobj = JObject.Parse(TacticResult);
                                OpportunityMember objOpp = new OpportunityMember();
                                try
                                {
                                    if (Convert.ToString(jobj[Amount]) != null && Convert.ToString(jobj[CloseDate]) != null)
                                    {
                                        string campaignid = Convert.ToString(jobj[CampaignId]);
                                        objOpp.CampaignId = campaignid;
                                        objOpp.OpportunityId = Convert.ToString(jobj["Id"]);
                                        objOpp.CloseDate = Convert.ToDateTime(jobj[CloseDate]);
                                        objOpp.CreatedDate = Convert.ToDateTime(jobj["CreatedDate"]);
                                        objOpp.Amount = Convert.ToDouble(jobj[Amount]);
                                        OpportunityMemberListInitial.Add(objOpp);
                                    }
                                }
                                catch (SalesforceException e)
                                {
                                    errorcount++;
                                    _ErrorMessage = GetErrorMessage(e);
                                    continue;
                                }
                                catch (Exception e)
                                {
                                    errorcount++;
                                    _ErrorMessage = e.Message;
                                    continue;
                                }

                            }
                            if (cwRecords.Count > 0)
                            {
                                if (errorcount > 0)
                                {
                                    ErrorFlag = true;
                                    IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                    instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                    instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                    instanceTactic.EntityId = 0;
                                    instanceTactic.EntityType = EntityType.Tactic.ToString();
                                    instanceTactic.Status = StatusResult.Error.ToString();
                                    instanceTactic.Operation = Operation.Pull_ClosedWon.ToString();
                                    instanceTactic.SyncTimeStamp = DateTime.Now;
                                    instanceTactic.CreatedDate = DateTime.Now;
                                    instanceTactic.ErrorDescription = Common.OpportunityObjectError + _ErrorMessage;
                                    instanceTactic.CreatedBy = _userId;
                                    db.Entry(instanceTactic).State = EntityState.Added;
                                }
                            }

                            if (!ErrorFlag && OpportunityMemberListInitial.Count > 0)
                            {
                                // Get Primary contact for opportunity
                                List<string> opportunitysids = (from opp in OpportunityMemberListInitial select opp.OpportunityId).ToList();
                                List<ContactRoleMember> ContactRoleListInitial = new List<ContactRoleMember>(_client.Query<ContactRoleMember>(opportunityRoleQuery));
                                ContactRoleListInitial = ContactRoleListInitial.Where(crl => opportunitysids.Contains(crl.OpportunityId)).ToList();
                                List<ContactRoleMember> ContactRoleList = new List<ContactRoleMember>();
                                ContactRoleList = ContactRoleListInitial.Where(cr => cr.IsPrimary).GroupBy(cr => new { cr.ContactId, cr.OpportunityId }).Select(cr => new ContactRoleMember { ContactId = cr.Key.ContactId, OpportunityId = cr.Key.OpportunityId }).ToList();

                                var opportunityprimaryid = ContactRoleList.Select(crn => crn.OpportunityId).ToList();
                                List<ContactRoleMember> ContactRoleListNextNonPrimary = new List<ContactRoleMember>();
                                ContactRoleListNextNonPrimary = ContactRoleListInitial.Where(cr => !opportunityprimaryid.Contains(cr.OpportunityId) && !cr.IsPrimary).GroupBy(cr => new { cr.OpportunityId }).Select(cr => new ContactRoleMember { OpportunityId = cr.Key.OpportunityId, Count = cr.Count() }).ToList().Where(crn => crn.Count == 1).ToList();
                                var opportunityidssinglecontact = ContactRoleListNextNonPrimary.Select(crn => crn.OpportunityId).ToList();
                                ContactRoleListInitial.Where(cr => !opportunityprimaryid.Contains(cr.OpportunityId) && !cr.IsPrimary && opportunityidssinglecontact.Contains(cr.OpportunityId)).ToList().ForEach(crl => ContactRoleList.Add(crl));

                                // Get campaign member from contact based on responded
                                List<string> contactid = (from contact in ContactRoleList select contact.ContactId).ToList();

                                var Contactmemberlist = _client.Query<object>("SELECT " + CampaignId + "," + ResponseDate + ",ContactId FROM CampaignMember WHERE ContactId IN (SELECT ContactId FROM OpportunityContactRole) AND HasResponded = True ORDER BY " + ResponseDate + " DESC");
                                List<ContactCampaignMember> ContactCampaignMemberList = new List<ContactCampaignMember>();
                                errorcount = 0;
                                foreach (var resultin in Contactmemberlist)
                                {
                                    string TacticResult = resultin.ToString();
                                    JObject jobj = JObject.Parse(TacticResult);
                                    ContactCampaignMember objCampaign = new ContactCampaignMember();
                                    int _PlanTacticId = 0;
                                    try
                                    {
                                        if (contactid.Contains(Convert.ToString(jobj["ContactId"])))
                                        {
                                            string str = Convert.ToString(jobj[ResponseDate]);
                                            if (Convert.ToString(jobj[ResponseDate]) != "")
                                            {
                                                string campaignid = Convert.ToString(jobj[CampaignId]);
                                                objCampaign.CampaignId = campaignid;
                                                objCampaign.ContactId = Convert.ToString(jobj["ContactId"]);
                                                objCampaign.RespondedDate = Convert.ToDateTime(jobj[ResponseDate]);
                                                ContactCampaignMemberList.Add(objCampaign);
                                            }
                                        }
                                    }
                                    catch (SalesforceException e)
                                    {
                                        errorcount++;
                                        string TacticId = Convert.ToString(jobj[CampaignId]);
                                        if (TacticId != string.Empty)
                                        {

                                            //// check whether TacticId(CRMId) exist in field IntegrationInstanceTacticID field of SalesForceTactic list.
                                            var tactic = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
                                            if (tactic != null)
                                                _PlanTacticId = tactic.PlanTacticId;
                                            else                                        //// if Tactic not exist then retrieve PlanTacticId from EloquaTactic list.
                                                _PlanTacticId = lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && (_SalTac.CRMId == TacticId || _SalTac.CRMId == TacticId.Substring(0, 15))).Select(s => s.PlanTacticId).FirstOrDefault();
                                            if (_PlanTacticId != 0)
                                            {
                                                ErrorFlag = true;
                                                _ErrorMessage = GetErrorMessage(e);
                                                IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                                instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                                instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                                instanceTactic.EntityId = _PlanTacticId;
                                                instanceTactic.EntityType = EntityType.Tactic.ToString();
                                                instanceTactic.Status = StatusResult.Error.ToString();
                                                instanceTactic.Operation = Operation.Pull_ClosedWon.ToString();
                                                instanceTactic.SyncTimeStamp = DateTime.Now;
                                                instanceTactic.CreatedDate = DateTime.Now;
                                                instanceTactic.ErrorDescription = Common.CampaignMemberObjectError + GetErrorMessage(e);
                                                instanceTactic.CreatedBy = _userId;
                                                db.Entry(instanceTactic).State = EntityState.Added;
                                            }
                                        }
                                        continue;
                                    }
                                    catch (Exception e)
                                    {
                                        errorcount++;
                                        string TacticId = Convert.ToString(jobj[CampaignId]);
                                        if (TacticId != string.Empty)
                                        {

                                            //// check whether TacticId(CRMId) exist in field IntegrationInstanceTacticID field of SalesForceTactic list.
                                            var tactic = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
                                            if (tactic != null)
                                                _PlanTacticId = tactic.PlanTacticId;
                                            else                                        //// if Tactic not exist then retrieve PlanTacticId from EloquaTactic list.
                                                _PlanTacticId = lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && (_SalTac.CRMId == TacticId || _SalTac.CRMId == TacticId.Substring(0, 15))).Select(s => s.PlanTacticId).FirstOrDefault();
                                            if (_PlanTacticId != 0)
                                            {
                                                ErrorFlag = true;
                                                _ErrorMessage = e.Message;
                                                IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                                instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                                instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                                instanceTactic.EntityId = _PlanTacticId;
                                                instanceTactic.EntityType = EntityType.Tactic.ToString();
                                                instanceTactic.Status = StatusResult.Error.ToString();
                                                instanceTactic.Operation = Operation.Pull_ClosedWon.ToString();
                                                instanceTactic.SyncTimeStamp = DateTime.Now;
                                                instanceTactic.CreatedDate = DateTime.Now;
                                                instanceTactic.ErrorDescription = Common.CampaignMemberObjectError + e.Message;
                                                instanceTactic.CreatedBy = _userId;
                                                db.Entry(instanceTactic).State = EntityState.Added;
                                            }
                                        }
                                        continue;
                                    }

                                }

                                if (!ErrorFlag)
                                {
                                    ContactCampaignMemberList = (from element in ContactCampaignMemberList
                                                                 group element by element.ContactId
                                                                     into groups
                                                                     select groups.OrderByDescending(p => p.RespondedDate).First()).ToList();

                                    List<OpportunityMember> OpportunityMemberList = new List<OpportunityMember>();
                                    OpportunityMemberList = (from om in OpportunityMemberListInitial
                                                             join crm in ContactRoleList on om.OpportunityId equals crm.OpportunityId
                                                             join ccml in ContactCampaignMemberList on crm.ContactId equals ccml.ContactId
                                                             where om.CreatedDate >= ccml.RespondedDate && om.Amount != 0
                                                             select new OpportunityMember
                                                             {
                                                                 OpportunityId = om.OpportunityId,
                                                                 CampaignId = !AllIntegrationTacticIds.Contains(ccml.CampaignId) ? ccml.CampaignId.Substring(0, 15) : ccml.CampaignId,
                                                                 CloseDate = om.CloseDate,
                                                                 CreatedDate = om.CreatedDate,
                                                                 Amount = om.Amount
                                                             }).ToList();

                                    //// Get Tactics from Eloqualist those CRMID value does not null.
                                    List<Plan_Campaign_Program_Tactic> lstCRM_EloquaTactics = (from _Tactic in lstEloquaTactic
                                                                                               join _ElqTactic in lstSalesForceIntegrationInstanceTacticIds on _Tactic.IntegrationInstanceTacticId equals _ElqTactic.EloquaId
                                                                                               where _ElqTactic.CRMId != null
                                                                                               select _Tactic).ToList();

                                    //// Merge SalesForce & Eloqua Tactic list.
                                    List<Plan_Campaign_Program_Tactic> lstMergedTactics = lstSalesForceTactic;
                                    lstCRM_EloquaTactics.ForEach(_elqTactic => lstMergedTactics.Add(_elqTactic));
                                    lstMergedTactics = lstMergedTactics.Distinct().ToList();

                                    if (OpportunityMemberList.Count > 0)
                                    {
                                        var OpportunityMemberListGroup = OpportunityMemberList.GroupBy(cl => new { CampaignId = cl.CampaignId, Month = cl.CloseDate.ToString("MM/yyyy") }).Select(cl =>
                                            new
                                            {
                                                CampaignId = cl.Key.CampaignId,
                                                TacticId = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null ? (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).PlanTacticId) : (lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && _SalTac.CRMId.Equals(cl.Key.CampaignId)).Select(s => s.PlanTacticId).FirstOrDefault()),
                                                Period = "Y" + Convert.ToDateTime(cl.Key.Month).Month,
                                                IsYear = (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year == Convert.ToDateTime(cl.Key.Month).Year)) || ((lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && _SalTac.CRMId.Equals(cl.Key.CampaignId)).Select(_SalTac => _SalTac.StartDate) != null) && (lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && _SalTac.CRMId.Equals(cl.Key.CampaignId)).Select(_SalTac => _SalTac.StartDate.Value).FirstOrDefault().Year == Convert.ToDateTime(cl.Key.Month).Year)) ? true : false,
                                                Count = cl.Count(),
                                                Revenue = cl.Sum(c => c.Amount)
                                            }).Where(om => om.IsYear).ToList();

                                        var tacticidactual = OpportunityMemberListGroup.Select(opptactic => opptactic.TacticId).Distinct().ToList();

                                        List<Plan_Campaign_Program_Tactic_Actual> OuteractualTacticList = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => tacticidactual.Contains(actual.PlanTacticId) && (actual.StageTitle == Common.StageRevenue || actual.StageTitle == Common.StageCW)).ToList();
                                        if (!isDoneFirstPullCW)
                                        {
                                            OuteractualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                            db.SaveChanges();
                                        }

                                        lstMergedTactics = lstMergedTactics.Where(lstmerge => tacticidactual.Contains(lstmerge.PlanTacticId)).Distinct().ToList();
                                        foreach (var tactic in lstMergedTactics)
                                        {
                                            var innerOpportunityMember = OpportunityMemberListGroup.Where(cm => cm.TacticId == tactic.PlanTacticId).ToList();
                                            foreach (var objOpportunityMember in innerOpportunityMember)
                                            {
                                                var innertacticactualcw = OuteractualTacticList.FirstOrDefault(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.Period == objOpportunityMember.Period && tacticActual.StageTitle == Common.StageCW);
                                                if (innertacticactualcw != null && isDoneFirstPullCW)
                                                {
                                                    innertacticactualcw.Actualvalue = innertacticactualcw.Actualvalue + objOpportunityMember.Count;
                                                    innertacticactualcw.ModifiedDate = DateTime.Now;
                                                    innertacticactualcw.ModifiedBy = _userId;
                                                    db.Entry(innertacticactualcw).State = EntityState.Modified;
                                                }
                                                else
                                                {
                                                    Plan_Campaign_Program_Tactic_Actual objPlanTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                                    objPlanTacticActual.PlanTacticId = objOpportunityMember.TacticId;
                                                    objPlanTacticActual.Period = objOpportunityMember.Period;
                                                    objPlanTacticActual.StageTitle = Common.StageCW;
                                                    objPlanTacticActual.Actualvalue = objOpportunityMember.Count;
                                                    objPlanTacticActual.CreatedBy = _userId;
                                                    objPlanTacticActual.CreatedDate = DateTime.Now;
                                                    db.Entry(objPlanTacticActual).State = EntityState.Added;
                                                }

                                                var innertacticactualrevenue = OuteractualTacticList.FirstOrDefault(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.Period == objOpportunityMember.Period && tacticActual.StageTitle == Common.StageRevenue);
                                                if (innertacticactualrevenue != null && isDoneFirstPullCW)
                                                {
                                                    innertacticactualrevenue.Actualvalue = innertacticactualrevenue.Actualvalue + objOpportunityMember.Revenue;
                                                    innertacticactualrevenue.ModifiedDate = DateTime.Now;
                                                    innertacticactualrevenue.ModifiedBy = _userId;
                                                    db.Entry(innertacticactualrevenue).State = EntityState.Modified;
                                                }
                                                else
                                                {
                                                    Plan_Campaign_Program_Tactic_Actual objPlanTacticActualRevenue = new Plan_Campaign_Program_Tactic_Actual();
                                                    objPlanTacticActualRevenue.PlanTacticId = objOpportunityMember.TacticId;
                                                    objPlanTacticActualRevenue.Period = objOpportunityMember.Period;
                                                    objPlanTacticActualRevenue.StageTitle = Common.StageRevenue;
                                                    objPlanTacticActualRevenue.Actualvalue = objOpportunityMember.Revenue;
                                                    objPlanTacticActualRevenue.CreatedBy = _userId;
                                                    objPlanTacticActualRevenue.CreatedDate = DateTime.Now;
                                                    db.Entry(objPlanTacticActualRevenue).State = EntityState.Added;
                                                }
                                            }

                                            tactic.LastSyncDate = DateTime.Now;
                                            tactic.ModifiedDate = DateTime.Now;
                                            tactic.ModifiedBy = _userId;

                                            IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                            instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                            instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                            instanceTactic.EntityId = tactic.PlanTacticId;
                                            instanceTactic.EntityType = EntityType.Tactic.ToString();
                                            instanceTactic.Status = StatusResult.Success.ToString();
                                            instanceTactic.Operation = Operation.Pull_ClosedWon.ToString();
                                            instanceTactic.SyncTimeStamp = DateTime.Now;
                                            instanceTactic.CreatedDate = DateTime.Now;
                                            instanceTactic.CreatedBy = _userId;
                                            db.Entry(instanceTactic).State = EntityState.Added;
                                        }

                                    }
                                }

                            }
                            db.SaveChanges();
                            if (ErrorFlag)
                            {
                                _isResultError = true;
                                // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, _ErrorMessage);
                            }
                            else
                            {
                                // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                                if (!isDoneFirstPullCW)
                                {
                                    using (MRPEntities dbinner = new MRPEntities())
                                    {
                                        IntegrationInstance integrationinstancecw = dbinner.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId);
                                        integrationinstancecw.IsFirstPullCW = true;
                                        dbinner.Entry(integrationinstancecw).State = EntityState.Modified;
                                        dbinner.SaveChanges();
                                        IntegrationInstance integrationInstance = new IntegrationInstance();
                                        integrationInstance = dbinner.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId);
                                    }

                                }
                            }

                        }
                        else
                        {
                            _isResultError = true;
                            // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                            Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgMappingNotFoundForSalesforcePullCW);
                        }
                    }
                    else
                    {
                        // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    }
                }
                catch (SalesforceException e)
                {
                    string msg = GetErrorMessage(e);
                    // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, msg);
                }
            }
            else
            {
                // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
            }
        }

        private class ImportCostMember
        {
            public string CampaignId { get; set; }
            public double actualCost { get; set; }
        }


        /// <summary>
        /// Function to set mapping details.
        /// </summary>
        private void SetMappingDetails()
        {
            // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
            string Campaign_EntityType = Enums.EntityType.Campaign.ToString();
            string Program_EntityType = Enums.EntityType.Program.ToString();
            string Tactic_EntityType = Enums.EntityType.Tactic.ToString();
            string Global = Enums.IntegrantionDataTypeMappingTableName.Global.ToString();
            string Plan_Campaign = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign.ToString();
            string Plan_Campaign_Program = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program.ToString();
            string Plan_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program_Tactic.ToString();
            string Plan_Improvement_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Improvement_Campaign_Program_Tactic.ToString();
            string Plan_Improvement_Campaign_Program = Enums.IntegrantionDataTypeMappingTableName.Plan_Improvement_Campaign_Program.ToString();
            string Plan_Improvement_Campaign = Enums.IntegrantionDataTypeMappingTableName.Plan_Improvement_Campaign.ToString();
            // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997

            // Start - Modified by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
            List<IntegrationInstanceDataTypeMapping> dataTypeMapping = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.IntegrationInstanceId.Equals(_integrationInstanceId)).ToList();
            _mappingTactic = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign_Program_Tactic
                                                    || gameplandata.GameplanDataType.TableName == Global) : gameplandata.CustomField.EntityType == Tactic_EntityType) &&
                                                    (gameplandata.GameplanDataType != null ? !gameplandata.GameplanDataType.IsGet : true))
                                                .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

            _mappingProgram = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign_Program
                                                    || gameplandata.GameplanDataType.TableName == Global) : gameplandata.CustomField.EntityType == Program_EntityType) &&
                                                    (gameplandata.GameplanDataType != null ? !gameplandata.GameplanDataType.IsGet : true))
                                                .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

            _mappingCampaign = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign
                                                    || gameplandata.GameplanDataType.TableName == Global) : gameplandata.CustomField.EntityType == Campaign_EntityType) &&
                                                    (gameplandata.GameplanDataType != null ? !gameplandata.GameplanDataType.IsGet : true))
                                                .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
            // End - Modified by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997

            // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
            dataTypeMapping = dataTypeMapping.Where(gp => gp.GameplanDataType != null).Select(gp => gp).ToList();
            // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997

            // Start - Modified by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
            _mappingImprovementTactic = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType.TableName == Plan_Improvement_Campaign_Program_Tactic
                                                            || (gameplandata.GameplanDataType.TableName == Global && gameplandata.GameplanDataType.IsImprovement == true)) &&
                                                                !gameplandata.GameplanDataType.IsGet)
                                            .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                            .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

            _mappingImprovementProgram = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType.TableName == Plan_Improvement_Campaign_Program
                                                            || (gameplandata.GameplanDataType.TableName == Global && gameplandata.GameplanDataType.IsImprovement == true)) &&
                                                                !gameplandata.GameplanDataType.IsGet)
                                           .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                           .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

            _mappingImprovementCampaign = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType.TableName == Plan_Improvement_Campaign
                                                            || (gameplandata.GameplanDataType.TableName == Global && gameplandata.GameplanDataType.IsImprovement == true)) &&
                                                                !gameplandata.GameplanDataType.IsGet)
                                           .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                           .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
            // End - Modified by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997

            _clientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;

            BDSService.BDSServiceClient objBDSservice = new BDSService.BDSServiceClient();
            _mappingUser = objBDSservice.GetUserListByClientId(_clientId).Select(u => new { u.UserId, u.FirstName, u.LastName }).ToDictionary(u => u.UserId, u => u.FirstName + " " + u.LastName);
            var clientActivityList = db.Client_Activity.Where(clientActivity => clientActivity.ClientId == _clientId).ToList();
            var ApplicationActivityList = objBDSservice.GetClientApplicationactivitylist(_applicationId);
            var clientApplicationActivityList = (from c in clientActivityList
                                                 join ca in ApplicationActivityList on c.ApplicationActivityId equals ca.ApplicationActivityId
                                                 select new
                                                 {
                                                     Code = ca.Code,
                                                     ActivityTitle = ca.ActivityTitle,
                                                     clientId = c.ClientId
                                                 }).Select(c => c).ToList();
            IsClientAllowedForCustomNaming = clientApplicationActivityList.Where(clientActivity => clientActivity.Code == Enums.clientAcivity.CustomCampaignNameConvention.ToString()).Any();


        }

        private Plan_Campaign SyncCampaingData(Plan_Campaign planCampaign)
        {
            Enums.Mode currentMode = Common.GetMode(planCampaign.IsDeleted, planCampaign.IsDeployedToIntegration, planCampaign.IntegrationInstanceCampaignId, planCampaign.Status);
            if (currentMode.Equals(Enums.Mode.Create))
            {
                IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                instanceLogCampaign.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                instanceLogCampaign.IntegrationInstanceId = _integrationInstanceId;
                instanceLogCampaign.EntityId = planCampaign.PlanCampaignId;
                instanceLogCampaign.EntityType = EntityType.Campaign.ToString();
                instanceLogCampaign.Operation = Operation.Create.ToString();
                instanceLogCampaign.SyncTimeStamp = DateTime.Now;
                try
                {
                    planCampaign.IntegrationInstanceCampaignId = CreateCampaign(planCampaign);
                    planCampaign.LastSyncDate = DateTime.Now;
                    planCampaign.ModifiedDate = DateTime.Now;
                    planCampaign.ModifiedBy = _userId;
                    instanceLogCampaign.Status = StatusResult.Success.ToString();
                }
                catch (SalesforceException e)
                {
                    instanceLogCampaign.Status = StatusResult.Error.ToString();
                    instanceLogCampaign.ErrorDescription = GetErrorMessage(e);
                }
                instanceLogCampaign.CreatedBy = this._userId;
                instanceLogCampaign.CreatedDate = DateTime.Now;
                db.Entry(instanceLogCampaign).State = EntityState.Added;
            }
            else if (currentMode.Equals(Enums.Mode.Update))
            {
                IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                instanceLogCampaign.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                instanceLogCampaign.IntegrationInstanceId = _integrationInstanceId;
                instanceLogCampaign.EntityId = planCampaign.PlanCampaignId;
                instanceLogCampaign.EntityType = EntityType.Campaign.ToString();
                instanceLogCampaign.Operation = Operation.Update.ToString();
                instanceLogCampaign.SyncTimeStamp = DateTime.Now;
                try
                {
                    if (UpdateCampaign(planCampaign))
                    {
                        planCampaign.LastSyncDate = DateTime.Now;
                        planCampaign.ModifiedDate = DateTime.Now;
                        planCampaign.ModifiedBy = _userId;
                        instanceLogCampaign.Status = StatusResult.Success.ToString();
                    }
                    else
                    {
                        instanceLogCampaign.Status = StatusResult.Error.ToString();
                        instanceLogCampaign.ErrorDescription = UnableToUpdate;
                    }
                }
                catch (SalesforceException e)
                {
                    if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                    {
                        //planCampaign.IntegrationInstanceCampaignId = null;
                        //planCampaign = SyncCampaingData(planCampaign);
                        return planCampaign;
                    }
                    else
                    {
                        instanceLogCampaign.Status = StatusResult.Error.ToString();
                        instanceLogCampaign.ErrorDescription = GetErrorMessage(e);
                    }
                }

                instanceLogCampaign.CreatedBy = this._userId;
                instanceLogCampaign.CreatedDate = DateTime.Now;
                db.Entry(instanceLogCampaign).State = EntityState.Added;
            }
            else if (currentMode.Equals(Enums.Mode.Delete))
            {
                var tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.PlanCampaignId == planCampaign.PlanCampaignId).ToList();
                var programList = db.Plan_Campaign_Program.Where(program => program.PlanCampaignId == planCampaign.PlanCampaignId).ToList();
                if (tacticList.Where(tactic => tactic.IsDeployedToIntegration && !tactic.IsDeleted).Count() == 0)
                {
                    // Set null value if delete true to integrationinstance..id
                    tacticList = tacticList.Where(tactic => tactic.IntegrationInstanceTacticId != null).ToList();
                    foreach (var t in tacticList)
                    {
                        IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                        instanceLogTactic.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                        instanceLogTactic.IntegrationInstanceId = _integrationInstanceId;
                        instanceLogTactic.EntityId = t.PlanTacticId;
                        instanceLogTactic.EntityType = EntityType.Tactic.ToString();
                        instanceLogTactic.Operation = Operation.Delete.ToString();
                        instanceLogTactic.SyncTimeStamp = DateTime.Now;
                        try
                        {
                            if (Delete(t.IntegrationInstanceTacticId))
                            {
                                t.IntegrationInstanceTacticId = null;
                                t.LastSyncDate = DateTime.Now;
                                t.ModifiedDate = DateTime.Now;
                                t.ModifiedBy = _userId;
                                instanceLogTactic.Status = StatusResult.Success.ToString();
                            }
                            else
                            {
                                instanceLogTactic.Status = StatusResult.Error.ToString();
                                instanceLogTactic.ErrorDescription = UnableToDelete;
                            }
                        }
                        catch (SalesforceException e)
                        {
                            if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                            {
                                t.IntegrationInstanceTacticId = null;
                                t.LastSyncDate = DateTime.Now;
                                t.ModifiedDate = DateTime.Now;
                                t.ModifiedBy = _userId;
                                instanceLogTactic.Status = StatusResult.Success.ToString();
                            }
                            else
                            {
                                instanceLogTactic.Status = StatusResult.Error.ToString();
                                instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                            }
                        }

                        instanceLogTactic.CreatedBy = this._userId;
                        instanceLogTactic.CreatedDate = DateTime.Now;
                        db.Entry(instanceLogTactic).State = EntityState.Added;
                    }


                    if (programList.Where(program => program.IsDeployedToIntegration && !program.IsDeleted).Count() == 0)
                    {
                        // Set null value if delete true to integrationinstance..id
                        programList = programList.Where(program => program.IntegrationInstanceProgramId != null).ToList();
                        foreach (var p in programList)
                        {
                            IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
                            instanceLogProgram.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                            instanceLogProgram.IntegrationInstanceId = _integrationInstanceId;
                            instanceLogProgram.EntityId = p.PlanProgramId;
                            instanceLogProgram.EntityType = EntityType.Program.ToString();
                            instanceLogProgram.Operation = Operation.Delete.ToString();
                            instanceLogProgram.SyncTimeStamp = DateTime.Now;
                            try
                            {
                                if (Delete(p.IntegrationInstanceProgramId))
                                {
                                    p.IntegrationInstanceProgramId = null;
                                    p.LastSyncDate = DateTime.Now;
                                    p.ModifiedDate = DateTime.Now;
                                    p.ModifiedBy = _userId;
                                    instanceLogProgram.Status = StatusResult.Success.ToString();
                                }
                                else
                                {
                                    instanceLogProgram.Status = StatusResult.Error.ToString();
                                    instanceLogProgram.ErrorDescription = UnableToDelete;
                                }
                            }
                            catch (SalesforceException e)
                            {
                                if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                                {
                                    p.IntegrationInstanceProgramId = null;
                                    p.LastSyncDate = DateTime.Now;
                                    p.ModifiedDate = DateTime.Now;
                                    p.ModifiedBy = _userId;
                                    instanceLogProgram.Status = StatusResult.Success.ToString();
                                }
                                else
                                {
                                    instanceLogProgram.Status = StatusResult.Error.ToString();
                                    instanceLogProgram.ErrorDescription = GetErrorMessage(e);
                                }
                            }

                            instanceLogProgram.CreatedBy = this._userId;
                            instanceLogProgram.CreatedDate = DateTime.Now;
                            db.Entry(instanceLogProgram).State = EntityState.Added;
                        }

                        // Set null value if delete true to integrationinstance..id
                        IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                        instanceLogCampaign.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                        instanceLogCampaign.IntegrationInstanceId = _integrationInstanceId;
                        instanceLogCampaign.EntityId = planCampaign.PlanCampaignId;
                        instanceLogCampaign.EntityType = EntityType.Campaign.ToString();
                        instanceLogCampaign.Operation = Operation.Delete.ToString();
                        instanceLogCampaign.SyncTimeStamp = DateTime.Now;
                        try
                        {
                            if (Delete(planCampaign.IntegrationInstanceCampaignId))
                            {
                                planCampaign.IntegrationInstanceCampaignId = null;
                                planCampaign.LastSyncDate = DateTime.Now;
                                planCampaign.ModifiedDate = DateTime.Now;
                                planCampaign.ModifiedBy = _userId;
                                instanceLogCampaign.Status = StatusResult.Success.ToString();
                            }
                            else
                            {
                                instanceLogCampaign.Status = StatusResult.Error.ToString();
                                instanceLogCampaign.ErrorDescription = UnableToDelete;
                            }
                        }
                        catch (SalesforceException e)
                        {
                            if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                            {
                                planCampaign.IntegrationInstanceCampaignId = null;
                                planCampaign.LastSyncDate = DateTime.Now;
                                planCampaign.ModifiedDate = DateTime.Now;
                                planCampaign.ModifiedBy = _userId;
                                instanceLogCampaign.Status = StatusResult.Success.ToString();
                            }
                            else
                            {
                                instanceLogCampaign.Status = StatusResult.Error.ToString();
                                instanceLogCampaign.ErrorDescription = GetErrorMessage(e);
                            }
                        }

                        instanceLogCampaign.CreatedBy = this._userId;
                        instanceLogCampaign.CreatedDate = DateTime.Now;
                        db.Entry(instanceLogCampaign).State = EntityState.Added;
                    }
                }
            }

            return planCampaign;
        }

        private Plan_Campaign_Program SyncProgramData(Plan_Campaign_Program planProgram)
        {
            //// Get program based on _id property.
            Enums.Mode currentMode = Common.GetMode(planProgram.IsDeleted, planProgram.IsDeployedToIntegration, planProgram.IntegrationInstanceProgramId, planProgram.Status);
            if (currentMode.Equals(Enums.Mode.Create))
            {
                Plan_Campaign planCampaign = planProgram.Plan_Campaign;
                _parentId = planCampaign.IntegrationInstanceCampaignId;
                if (string.IsNullOrWhiteSpace(_parentId))
                {
                    IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                    instanceLogCampaign.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                    instanceLogCampaign.IntegrationInstanceId = _integrationInstanceId;
                    instanceLogCampaign.EntityId = planCampaign.PlanCampaignId;
                    instanceLogCampaign.EntityType = EntityType.Campaign.ToString();
                    instanceLogCampaign.Operation = Operation.Create.ToString();
                    instanceLogCampaign.SyncTimeStamp = DateTime.Now;
                    try
                    {
                        // Start - Added by Sohel Pathan on 09/12/2014 for PL ticket #995, 996, & 997
                        List<int> campaignIdList = new List<int>() { planCampaign.PlanCampaignId };
                        var lstCustomFieldsCampaign = CreateMappingCustomFieldDictionary(campaignIdList, Enums.EntityType.Campaign.ToString());
                        if (lstCustomFieldsCampaign.Count > 0)
                        {
                            _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsCampaign).ToDictionary(c => c.Key, c => c.Value);
                        }
                        // End - Added by Sohel Pathan on 09/12/2014 for PL ticket #995, 996, & 997

                        _parentId = CreateCampaign(planCampaign);
                        planCampaign.IntegrationInstanceCampaignId = _parentId;
                        planCampaign.LastSyncDate = DateTime.Now;
                        planCampaign.ModifiedDate = DateTime.Now;
                        planCampaign.ModifiedBy = _userId;
                        db.Entry(planCampaign).State = EntityState.Modified;
                        instanceLogCampaign.Status = StatusResult.Success.ToString();
                    }
                    catch (SalesforceException e)
                    {
                        instanceLogCampaign.Status = StatusResult.Error.ToString();
                        instanceLogCampaign.ErrorDescription = GetErrorMessage(e);
                    }
                    instanceLogCampaign.CreatedBy = this._userId;
                    instanceLogCampaign.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogCampaign).State = EntityState.Added;
                }

                if (!string.IsNullOrWhiteSpace(_parentId))
                {
                    IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
                    instanceLogProgram.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                    instanceLogProgram.IntegrationInstanceId = _integrationInstanceId;
                    instanceLogProgram.EntityId = planProgram.PlanProgramId;
                    instanceLogProgram.EntityType = EntityType.Program.ToString();
                    instanceLogProgram.Operation = Operation.Create.ToString();
                    instanceLogProgram.SyncTimeStamp = DateTime.Now;
                    try
                    {
                        planProgram.IntegrationInstanceProgramId = CreateProgram(planProgram);
                        planProgram.LastSyncDate = DateTime.Now;
                        planProgram.ModifiedDate = DateTime.Now;
                        planProgram.ModifiedBy = _userId;
                        instanceLogProgram.Status = StatusResult.Success.ToString();
                    }
                    catch (SalesforceException e)
                    {
                        instanceLogProgram.Status = StatusResult.Error.ToString();
                        instanceLogProgram.ErrorDescription = GetErrorMessage(e);
                    }

                    instanceLogProgram.CreatedBy = this._userId;
                    instanceLogProgram.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogProgram).State = EntityState.Added;
                }

            }
            else if (currentMode.Equals(Enums.Mode.Update))
            {
                IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
                instanceLogProgram.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                instanceLogProgram.IntegrationInstanceId = _integrationInstanceId;
                instanceLogProgram.EntityId = planProgram.PlanProgramId;
                instanceLogProgram.EntityType = EntityType.Program.ToString();
                instanceLogProgram.Operation = Operation.Update.ToString();
                instanceLogProgram.SyncTimeStamp = DateTime.Now;
                try
                {
                    if (UpdateProgram(planProgram))
                    {
                        planProgram.LastSyncDate = DateTime.Now;
                        planProgram.ModifiedDate = DateTime.Now;
                        planProgram.ModifiedBy = _userId;
                        instanceLogProgram.Status = StatusResult.Success.ToString();
                    }
                    else
                    {
                        instanceLogProgram.Status = StatusResult.Error.ToString();
                        instanceLogProgram.ErrorDescription = UnableToUpdate;
                    }
                }
                catch (SalesforceException e)
                {
                    if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                    {
                       // planProgram.IntegrationInstanceProgramId = null;
                       // planProgram = SyncProgramData(planProgram);
                        return planProgram;
                    }
                    else
                    {
                        instanceLogProgram.Status = StatusResult.Error.ToString();
                        instanceLogProgram.ErrorDescription = GetErrorMessage(e);
                    }
                }

                instanceLogProgram.CreatedBy = this._userId;
                instanceLogProgram.CreatedDate = DateTime.Now;
                db.Entry(instanceLogProgram).State = EntityState.Added;
            }
            else if (currentMode.Equals(Enums.Mode.Delete))
            {
                var tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanProgramId == planProgram.PlanProgramId).ToList();
                if (tacticList.Where(tactic => tactic.IsDeployedToIntegration && !tactic.IsDeleted).Count() == 0)
                {
                    tacticList = tacticList.Where(tactic => tactic.IntegrationInstanceTacticId != null).ToList();
                    foreach (var t in tacticList)
                    {
                        IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                        instanceLogTactic.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                        instanceLogTactic.IntegrationInstanceId = _integrationInstanceId;
                        instanceLogTactic.EntityId = t.PlanTacticId;
                        instanceLogTactic.EntityType = EntityType.Tactic.ToString();
                        instanceLogTactic.Operation = Operation.Delete.ToString();
                        instanceLogTactic.SyncTimeStamp = DateTime.Now;
                        try
                        {
                            if (Delete(t.IntegrationInstanceTacticId))
                            {
                                t.IntegrationInstanceTacticId = null;
                                t.LastSyncDate = DateTime.Now;
                                t.ModifiedDate = DateTime.Now;
                                t.ModifiedBy = _userId;
                                instanceLogTactic.Status = StatusResult.Success.ToString();
                            }
                            else
                            {
                                instanceLogTactic.Status = StatusResult.Error.ToString();
                                instanceLogTactic.ErrorDescription = UnableToDelete;
                            }
                        }
                        catch (SalesforceException e)
                        {
                            if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                            {
                                t.IntegrationInstanceTacticId = null;
                                t.LastSyncDate = DateTime.Now;
                                t.ModifiedDate = DateTime.Now;
                                t.ModifiedBy = _userId;
                                instanceLogTactic.Status = StatusResult.Success.ToString();
                            }
                            else
                            {
                                instanceLogTactic.Status = StatusResult.Error.ToString();
                                instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                            }
                        }

                        instanceLogTactic.CreatedBy = this._userId;
                        instanceLogTactic.CreatedDate = DateTime.Now;
                        db.Entry(instanceLogTactic).State = EntityState.Added;
                    }

                    IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
                    instanceLogProgram.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                    instanceLogProgram.IntegrationInstanceId = _integrationInstanceId;
                    instanceLogProgram.EntityId = planProgram.PlanProgramId;
                    instanceLogProgram.EntityType = EntityType.Program.ToString();
                    instanceLogProgram.Operation = Operation.Delete.ToString();
                    instanceLogProgram.SyncTimeStamp = DateTime.Now;
                    try
                    {
                        if (Delete(planProgram.IntegrationInstanceProgramId))
                        {
                            planProgram.IntegrationInstanceProgramId = null;
                            planProgram.LastSyncDate = DateTime.Now;
                            planProgram.ModifiedDate = DateTime.Now;
                            planProgram.ModifiedBy = _userId;
                            instanceLogProgram.Status = StatusResult.Success.ToString();
                        }
                        else
                        {
                            instanceLogProgram.Status = StatusResult.Error.ToString();
                            instanceLogProgram.ErrorDescription = UnableToDelete;
                        }
                    }
                    catch (SalesforceException e)
                    {
                        if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                        {
                            planProgram.IntegrationInstanceProgramId = null;
                            planProgram.LastSyncDate = DateTime.Now;
                            planProgram.ModifiedDate = DateTime.Now;
                            planProgram.ModifiedBy = _userId;
                            instanceLogProgram.Status = StatusResult.Success.ToString();
                        }
                        else
                        {
                            instanceLogProgram.Status = StatusResult.Error.ToString();
                            instanceLogProgram.ErrorDescription = GetErrorMessage(e);
                        }
                    }

                    instanceLogProgram.CreatedBy = this._userId;
                    instanceLogProgram.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogProgram).State = EntityState.Added;
                }
            }

            return planProgram;
        }

        private Plan_Campaign_Program_Tactic SyncTacticData(Plan_Campaign_Program_Tactic planTactic)
        {
            Enums.Mode currentMode = Common.GetMode(planTactic.IsDeleted, planTactic.IsDeployedToIntegration, planTactic.IntegrationInstanceTacticId, planTactic.Status);
            if (currentMode.Equals(Enums.Mode.Create))
            {
                Plan_Campaign_Program planProgram = planTactic.Plan_Campaign_Program;
                _parentId = planProgram.IntegrationInstanceProgramId;
                if (string.IsNullOrWhiteSpace(_parentId))
                {
                    Plan_Campaign planCampaign = planTactic.Plan_Campaign_Program.Plan_Campaign;
                    _parentId = planCampaign.IntegrationInstanceCampaignId;
                    if (string.IsNullOrWhiteSpace(_parentId))
                    {
                        IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                        instanceLogCampaign.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                        instanceLogCampaign.IntegrationInstanceId = _integrationInstanceId;
                        instanceLogCampaign.EntityId = planCampaign.PlanCampaignId;
                        instanceLogCampaign.EntityType = EntityType.Campaign.ToString();
                        instanceLogCampaign.Operation = Operation.Create.ToString();
                        instanceLogCampaign.SyncTimeStamp = DateTime.Now;
                        try
                        {
                            // Start - Added by Sohel Pathan on 09/12/2014 for PL ticket #995, 996, & 997
                            List<int> campaignIdList = new List<int>() { planCampaign.PlanCampaignId };
                            var lstCustomFieldsCampaign = CreateMappingCustomFieldDictionary(campaignIdList, Enums.EntityType.Campaign.ToString());
                            if (lstCustomFieldsCampaign.Count > 0)
                            {
                                _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsCampaign).ToDictionary(c => c.Key, c => c.Value);
                            }
                            // End - Added by Sohel Pathan on 09/12/2014 for PL ticket #995, 996, & 997

                            _parentId = CreateCampaign(planCampaign);
                            planCampaign.IntegrationInstanceCampaignId = _parentId;
                            planCampaign.LastSyncDate = DateTime.Now;
                            planCampaign.ModifiedDate = DateTime.Now;
                            planCampaign.ModifiedBy = _userId;
                            db.Entry(planCampaign).State = EntityState.Modified;
                            instanceLogCampaign.Status = StatusResult.Success.ToString();
                        }
                        catch (SalesforceException e)
                        {
                            instanceLogCampaign.Status = StatusResult.Error.ToString();
                            instanceLogCampaign.ErrorDescription = GetErrorMessage(e);
                        }
                        instanceLogCampaign.CreatedBy = this._userId;
                        instanceLogCampaign.CreatedDate = DateTime.Now;
                        db.Entry(instanceLogCampaign).State = EntityState.Added;
                    }

                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
                        instanceLogProgram.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                        instanceLogProgram.IntegrationInstanceId = _integrationInstanceId;
                        instanceLogProgram.EntityId = planProgram.PlanProgramId;
                        instanceLogProgram.EntityType = EntityType.Program.ToString();
                        instanceLogProgram.Operation = Operation.Create.ToString();
                        instanceLogProgram.SyncTimeStamp = DateTime.Now;
                        try
                        {
                            // Start - Added by Sohel Pathan on 09/12/2014 for PL ticket #995, 996, & 997
                            List<int> programIdList = new List<int>() { planProgram.PlanProgramId };
                            var lstCustomFieldsProgram = CreateMappingCustomFieldDictionary(programIdList, Enums.EntityType.Program.ToString());
                            if (lstCustomFieldsProgram.Count > 0)
                            {
                                _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsProgram).ToDictionary(c => c.Key, c => c.Value);
                            }

                            // End - Added by Sohel Pathan on 09/12/2014 for PL ticket #995, 996, & 997

                            _parentId = CreateProgram(planProgram);
                            planProgram.IntegrationInstanceProgramId = _parentId;
                            planProgram.LastSyncDate = DateTime.Now;
                            planProgram.ModifiedDate = DateTime.Now;
                            planProgram.ModifiedBy = _userId;
                            instanceLogProgram.Status = StatusResult.Success.ToString();
                        }
                        catch (SalesforceException e)
                        {
                            instanceLogProgram.Status = StatusResult.Error.ToString();
                            instanceLogProgram.ErrorDescription = GetErrorMessage(e);
                        }

                        instanceLogProgram.CreatedBy = this._userId;
                        instanceLogProgram.CreatedDate = DateTime.Now;
                        db.Entry(instanceLogProgram).State = EntityState.Added;
                    }
                }
                if (!string.IsNullOrWhiteSpace(_parentId))
                {
                    IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                    instanceLogTactic.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                    instanceLogTactic.IntegrationInstanceId = _integrationInstanceId;
                    instanceLogTactic.EntityId = planTactic.PlanTacticId;
                    instanceLogTactic.EntityType = EntityType.Tactic.ToString();
                    instanceLogTactic.Operation = Operation.Create.ToString();
                    instanceLogTactic.SyncTimeStamp = DateTime.Now;
                    try
                    {
                        planTactic.IntegrationInstanceTacticId = CreateTactic(planTactic);
                        planTactic.LastSyncDate = DateTime.Now;
                        planTactic.ModifiedDate = DateTime.Now;
                        planTactic.ModifiedBy = _userId;
                        instanceLogTactic.Status = StatusResult.Success.ToString();
                        //Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                        Plan_Campaign_Program_Tactic_Comment objTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                        objTacticComment.PlanTacticId = planTactic.PlanTacticId;
                        objTacticComment.Comment = Common.TacticSyncedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                        objTacticComment.CreatedDate = DateTime.Now;
                        ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                        if (Common.IsAutoSync)
                        {
                            objTacticComment.CreatedBy = new Guid();
                        }
                        else
                        {
                            objTacticComment.CreatedBy = this._userId;
                        }
                        db.Entry(objTacticComment).State = EntityState.Added;
                        db.Plan_Campaign_Program_Tactic_Comment.Add(objTacticComment);
                        // End Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                    }
                    catch (SalesforceException e)
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                    }

                    instanceLogTactic.CreatedBy = this._userId;
                    instanceLogTactic.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogTactic).State = EntityState.Added;
                }
            }
            else if (currentMode.Equals(Enums.Mode.Update))
            {
                IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                instanceLogTactic.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                instanceLogTactic.IntegrationInstanceId = _integrationInstanceId;
                instanceLogTactic.EntityId = planTactic.PlanTacticId;
                instanceLogTactic.EntityType = EntityType.Tactic.ToString();
                instanceLogTactic.Operation = Operation.Update.ToString();
                instanceLogTactic.SyncTimeStamp = DateTime.Now;
                try
                {
                    if (UpdateTactic(planTactic))
                    {
                        planTactic.LastSyncDate = DateTime.Now;
                        planTactic.ModifiedDate = DateTime.Now;
                        planTactic.ModifiedBy = _userId;
                        instanceLogTactic.Status = StatusResult.Success.ToString();
                    }
                    else
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = UnableToUpdate;
                    }
                }
                catch (SalesforceException e)
                {
                    if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                    {
                        //planTactic.IntegrationInstanceTacticId = null;
                       // planTactic = SyncTacticData(planTactic);
                        return planTactic;
                    }
                    else
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                    }
                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }
            else if (currentMode.Equals(Enums.Mode.Delete))
            {
                IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                instanceLogTactic.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                instanceLogTactic.IntegrationInstanceId = _integrationInstanceId;
                instanceLogTactic.EntityId = planTactic.PlanTacticId;
                instanceLogTactic.EntityType = EntityType.Tactic.ToString();
                instanceLogTactic.Operation = Operation.Delete.ToString();
                instanceLogTactic.SyncTimeStamp = DateTime.Now;
                try
                {
                    if (Delete(planTactic.IntegrationInstanceTacticId))
                    {
                        planTactic.IntegrationInstanceTacticId = null;
                        planTactic.LastSyncDate = DateTime.Now;
                        planTactic.ModifiedDate = DateTime.Now;
                        planTactic.ModifiedBy = _userId;
                        instanceLogTactic.Status = StatusResult.Success.ToString();
                    }
                    else
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = UnableToDelete;
                    }
                }
                catch (SalesforceException e)
                {
                    if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                    {
                        planTactic.IntegrationInstanceTacticId = null;
                        planTactic.LastSyncDate = DateTime.Now;
                        planTactic.ModifiedDate = DateTime.Now;
                        planTactic.ModifiedBy = _userId;
                        instanceLogTactic.Status = StatusResult.Success.ToString();
                    }
                    else
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                    }
                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }

            return planTactic;
        }

        private Plan_Improvement_Campaign SyncImprovementCampaingData(Plan_Improvement_Campaign planIMPCampaign)
        {
            var tacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId == planIMPCampaign.ImprovementPlanCampaignId && tactic.IntegrationInstanceTacticId != null).ToList();
            if (tacticList.Count == 0)
            {
                // Set null value if delete true to integrationinstance..id
                var programList = db.Plan_Improvement_Campaign_Program.Where(program => program.ImprovementPlanCampaignId == planIMPCampaign.ImprovementPlanCampaignId && program.IntegrationInstanceProgramId != null).ToList();
                foreach (var p in programList)
                {
                    IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
                    instanceLogProgram.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                    instanceLogProgram.IntegrationInstanceId = _integrationInstanceId;
                    instanceLogProgram.EntityId = p.ImprovementPlanProgramId;
                    instanceLogProgram.EntityType = EntityType.ImprovementProgram.ToString();
                    instanceLogProgram.Operation = Operation.Delete.ToString();
                    instanceLogProgram.SyncTimeStamp = DateTime.Now;
                    try
                    {
                        if (Delete(p.IntegrationInstanceProgramId))
                        {
                            p.IntegrationInstanceProgramId = null;
                            p.LastSyncDate = DateTime.Now;
                            instanceLogProgram.Status = StatusResult.Success.ToString();
                        }
                        else
                        {
                            instanceLogProgram.Status = StatusResult.Error.ToString();
                            instanceLogProgram.ErrorDescription = UnableToDelete;
                        }
                    }
                    catch (SalesforceException e)
                    {
                        if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                        {
                            p.IntegrationInstanceProgramId = null;
                            p.LastSyncDate = DateTime.Now;
                            instanceLogProgram.Status = StatusResult.Success.ToString();
                        }
                        else
                        {
                            instanceLogProgram.Status = StatusResult.Error.ToString();
                            instanceLogProgram.ErrorDescription = GetErrorMessage(e);
                        }
                    }

                    instanceLogProgram.CreatedBy = this._userId;
                    instanceLogProgram.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogProgram).State = EntityState.Added;
                }

                if (!string.IsNullOrWhiteSpace(planIMPCampaign.IntegrationInstanceCampaignId))
                {
                    // Set null value if delete true to integrationinstance..id
                    IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                    instanceLogCampaign.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                    instanceLogCampaign.IntegrationInstanceId = _integrationInstanceId;
                    instanceLogCampaign.EntityId = planIMPCampaign.ImprovementPlanCampaignId;
                    instanceLogCampaign.EntityType = EntityType.ImprovementCamapign.ToString();
                    instanceLogCampaign.Operation = Operation.Delete.ToString();
                    instanceLogCampaign.SyncTimeStamp = DateTime.Now;
                    try
                    {
                        if (Delete(planIMPCampaign.IntegrationInstanceCampaignId))
                        {
                            planIMPCampaign.IntegrationInstanceCampaignId = null;
                            planIMPCampaign.LastSyncDate = DateTime.Now;
                            instanceLogCampaign.Status = StatusResult.Success.ToString();
                        }
                        else
                        {
                            instanceLogCampaign.Status = StatusResult.Error.ToString();
                            instanceLogCampaign.ErrorDescription = UnableToDelete;
                        }
                    }
                    catch (SalesforceException e)
                    {
                        if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                        {
                            planIMPCampaign.IntegrationInstanceCampaignId = null;
                            planIMPCampaign.LastSyncDate = DateTime.Now;
                            instanceLogCampaign.Status = StatusResult.Success.ToString();
                        }
                        else
                        {
                            instanceLogCampaign.Status = StatusResult.Error.ToString();
                            instanceLogCampaign.ErrorDescription = GetErrorMessage(e);
                        }
                    }

                    instanceLogCampaign.CreatedBy = this._userId;
                    instanceLogCampaign.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogCampaign).State = EntityState.Added;
                }
            }

            return planIMPCampaign;
        }

        private Plan_Improvement_Campaign_Program_Tactic SyncImprovementData(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            Enums.Mode currentMode = Common.GetMode(planIMPTactic.IsDeleted, planIMPTactic.IsDeployedToIntegration, planIMPTactic.IntegrationInstanceTacticId, planIMPTactic.Status);
            if (currentMode.Equals(Enums.Mode.Create))
            {
                Plan_Improvement_Campaign_Program planIMPProgram = planIMPTactic.Plan_Improvement_Campaign_Program;
                _parentId = planIMPProgram.IntegrationInstanceProgramId;
                if (string.IsNullOrWhiteSpace(_parentId))
                {
                    Plan_Improvement_Campaign planIMPCampaign = planIMPTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign;
                    _parentId = planIMPCampaign.IntegrationInstanceCampaignId;
                    if (string.IsNullOrWhiteSpace(_parentId))
                    {
                        IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                        instanceLogCampaign.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                        instanceLogCampaign.IntegrationInstanceId = _integrationInstanceId;
                        instanceLogCampaign.EntityId = planIMPCampaign.ImprovementPlanCampaignId;
                        instanceLogCampaign.EntityType = EntityType.ImprovementCamapign.ToString();
                        instanceLogCampaign.Operation = Operation.Create.ToString();
                        instanceLogCampaign.SyncTimeStamp = DateTime.Now;
                        try
                        {
                            _parentId = CreateImprovementCampaign(planIMPCampaign);
                            planIMPCampaign.IntegrationInstanceCampaignId = _parentId;
                            planIMPCampaign.LastSyncDate = DateTime.Now;
                            db.Entry(planIMPCampaign).State = EntityState.Modified;
                            instanceLogCampaign.Status = StatusResult.Success.ToString();
                        }
                        catch (SalesforceException e)
                        {
                            instanceLogCampaign.Status = StatusResult.Error.ToString();
                            instanceLogCampaign.ErrorDescription = GetErrorMessage(e);
                        }
                        instanceLogCampaign.CreatedBy = this._userId;
                        instanceLogCampaign.CreatedDate = DateTime.Now;
                        db.Entry(instanceLogCampaign).State = EntityState.Added;
                    }

                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
                        instanceLogProgram.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                        instanceLogProgram.IntegrationInstanceId = _integrationInstanceId;
                        instanceLogProgram.EntityId = planIMPProgram.ImprovementPlanProgramId;
                        instanceLogProgram.EntityType = EntityType.ImprovementProgram.ToString();
                        instanceLogProgram.Operation = Operation.Create.ToString();
                        instanceLogProgram.SyncTimeStamp = DateTime.Now;
                        try
                        {
                            _parentId = CreateImprovementProgram(planIMPProgram);
                            planIMPProgram.IntegrationInstanceProgramId = _parentId;
                            planIMPProgram.LastSyncDate = DateTime.Now;
                            db.Entry(planIMPProgram).State = EntityState.Modified;
                            instanceLogProgram.Status = StatusResult.Success.ToString();
                        }
                        catch (SalesforceException e)
                        {
                            instanceLogProgram.Status = StatusResult.Error.ToString();
                            instanceLogProgram.ErrorDescription = GetErrorMessage(e);
                        }

                        instanceLogProgram.CreatedBy = this._userId;
                        instanceLogProgram.CreatedDate = DateTime.Now;
                        db.Entry(instanceLogProgram).State = EntityState.Added;
                    }
                }

                if (!string.IsNullOrWhiteSpace(_parentId))
                {
                    IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                    instanceLogTactic.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                    instanceLogTactic.IntegrationInstanceId = _integrationInstanceId;
                    instanceLogTactic.EntityId = planIMPTactic.ImprovementPlanTacticId;
                    instanceLogTactic.EntityType = EntityType.ImprovementTactic.ToString();
                    instanceLogTactic.Operation = Operation.Create.ToString();
                    instanceLogTactic.SyncTimeStamp = DateTime.Now;
                    try
                    {
                        planIMPTactic.IntegrationInstanceTacticId = CreateImprovementTactic(planIMPTactic);
                        planIMPTactic.LastSyncDate = DateTime.Now;
                        planIMPTactic.ModifiedDate = DateTime.Now;
                        planIMPTactic.ModifiedBy = _userId;
                        instanceLogTactic.Status = StatusResult.Success.ToString();
                        //Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                        Plan_Improvement_Campaign_Program_Tactic_Comment objImpTacticComment = new Plan_Improvement_Campaign_Program_Tactic_Comment();
                        objImpTacticComment.ImprovementPlanTacticId = planIMPTactic.ImprovementPlanTacticId;
                        objImpTacticComment.Comment = Common.ImprovementTacticSyncedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                        objImpTacticComment.CreatedDate = DateTime.Now;
                        ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                        if (Common.IsAutoSync)
                        {
                            objImpTacticComment.CreatedBy = new Guid();
                        }
                        else
                        {
                            objImpTacticComment.CreatedBy = this._userId;
                        }

                        db.Entry(objImpTacticComment).State = EntityState.Added;
                        db.Plan_Improvement_Campaign_Program_Tactic_Comment.Add(objImpTacticComment);
                        //End: Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                    }
                    catch (SalesforceException e)
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                    }

                    instanceLogTactic.CreatedBy = this._userId;
                    instanceLogTactic.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogTactic).State = EntityState.Added;
                }

            }
            else if (currentMode.Equals(Enums.Mode.Update))
            {
                IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                instanceLogTactic.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                instanceLogTactic.IntegrationInstanceId = _integrationInstanceId;
                instanceLogTactic.EntityId = planIMPTactic.ImprovementPlanTacticId;
                instanceLogTactic.EntityType = EntityType.ImprovementTactic.ToString();
                instanceLogTactic.Operation = Operation.Update.ToString();
                instanceLogTactic.SyncTimeStamp = DateTime.Now;
                try
                {
                    if (UpdateImprovementTactic(planIMPTactic))
                    {
                        planIMPTactic.LastSyncDate = DateTime.Now;
                        planIMPTactic.ModifiedDate = DateTime.Now;
                        planIMPTactic.ModifiedBy = _userId;
                        instanceLogTactic.Status = StatusResult.Success.ToString();
                    }
                    else
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = UnableToUpdate;
                    }
                }
                catch (SalesforceException e)
                {
                    if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                    {
                       // planIMPTactic.IntegrationInstanceTacticId = null;
                       // planIMPTactic = SyncImprovementData(planIMPTactic);
                        return planIMPTactic;
                    }
                    else
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                    }
                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }
            else if (currentMode.Equals(Enums.Mode.Delete))
            {
                IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                instanceLogTactic.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                instanceLogTactic.IntegrationInstanceId = _integrationInstanceId;
                instanceLogTactic.EntityId = planIMPTactic.ImprovementPlanTacticId;
                instanceLogTactic.EntityType = EntityType.ImprovementTactic.ToString();
                instanceLogTactic.Operation = Operation.Delete.ToString();
                instanceLogTactic.SyncTimeStamp = DateTime.Now;
                try
                {
                    if (Delete(planIMPTactic.IntegrationInstanceTacticId))
                    {
                        planIMPTactic.IntegrationInstanceTacticId = null;
                        planIMPTactic.LastSyncDate = DateTime.Now;
                        planIMPTactic.ModifiedDate = DateTime.Now;
                        planIMPTactic.ModifiedBy = _userId;
                        instanceLogTactic.Status = StatusResult.Success.ToString();
                    }
                    else
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = UnableToUpdate;
                    }
                }
                catch (SalesforceException e)
                {
                    if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                    {
                        planIMPTactic.IntegrationInstanceTacticId = null;
                        planIMPTactic.LastSyncDate = DateTime.Now;
                        planIMPTactic.ModifiedDate = DateTime.Now;
                        planIMPTactic.ModifiedBy = _userId;
                        instanceLogTactic.Status = StatusResult.Success.ToString();
                    }
                    else
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                    }
                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }

            return planIMPTactic;
        }

        /// <summary>
        /// Function to Synchronize instance data.
        /// </summary>
        private void SyncInstanceData()
        {
            List<int> planIds = db.Plans.Where(p => p.Model.IntegrationInstanceId == _integrationInstanceId && p.Model.Status.Equals("Published")).Select(p => p.PlanId).ToList();
            try
            {
                using (var scope = new TransactionScope())
                {
                    List<Plan_Campaign> campaignList = db.Plan_Campaign.Where(campaign => planIds.Contains(campaign.PlanId)).ToList();
                    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    List<int> campaignIdList = campaignList.Select(c => c.PlanCampaignId).ToList();
                    _mappingCustomFields = CreateMappingCustomFieldDictionary(campaignIdList, Enums.EntityType.Campaign.ToString());
                    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    for (int index = 0; index < campaignList.Count; index++)
                    {
                        campaignList[index] = SyncCampaingData(campaignList[index]);
                    }
                    db.SaveChanges();
                    scope.Complete();
                }

                using (var scope = new TransactionScope())
                {
                    List<Plan_Campaign_Program> programList = db.Plan_Campaign_Program.Where(program => planIds.Contains(program.Plan_Campaign.PlanId)).ToList();
                    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    List<int> programIdList = programList.Select(c => c.PlanProgramId).ToList();
                    _mappingCustomFields = CreateMappingCustomFieldDictionary(programIdList, Enums.EntityType.Program.ToString());
                    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    for (int index = 0; index < programList.Count; index++)
                    {
                        programList[index] = SyncProgramData(programList[index]);
                    }
                    db.SaveChanges();
                    scope.Complete();
                }

                using (var scope = new TransactionScope())
                {
                    List<Plan_Campaign_Program_Tactic> tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).ToList();
                    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    List<int> tacticIdList = tacticList.Select(c => c.PlanTacticId).ToList();
                    _mappingTactic_ActualCost = Common.CalculateActualCostTacticslist(tacticIdList);
                    _mappingCustomFields = CreateMappingCustomFieldDictionary(tacticIdList, Enums.EntityType.Tactic.ToString());
                    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    for (int index = 0; index < tacticList.Count; index++)
                    {
                        tacticList[index] = SyncTacticData(tacticList[index]);
                    }
                    db.SaveChanges();
                    scope.Complete();
                }

                using (var scope = new TransactionScope())
                {
                    List<Plan_Improvement_Campaign_Program_Tactic> tacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(tactic => planIds.Contains(tactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).ToList();
                    for (int index = 0; index < tacticList.Count; index++)
                    {
                        tacticList[index] = SyncImprovementData(tacticList[index]);
                    }
                    db.SaveChanges();
                    scope.Complete();
                }

                using (var scope = new TransactionScope())
                {
                    List<Plan_Improvement_Campaign> campaignList = db.Plan_Improvement_Campaign.Where(campaign => planIds.Contains(campaign.ImprovePlanId)).ToList();
                    for (int index = 0; index < campaignList.Count; index++)
                    {
                        campaignList[index] = SyncImprovementCampaingData(campaignList[index]);
                    }
                    db.SaveChanges();
                    scope.Complete();
                }
            }
            catch (SalesforceException e)
            {
                _isResultError = true;
                _ErrorMessage = GetErrorMessage(e);
            }
        }

        private string CreateCampaign(Plan_Campaign planCampaign)
        {
            Dictionary<string, object> campaign = GetCampaign(planCampaign);

            if (_mappingTactic.ContainsKey("Title") && planCampaign != null)
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                if (campaign.ContainsKey(titleMappedValue))
                {
                    campaign[titleMappedValue] = Common.TruncateName(campaign[titleMappedValue].ToString());
                }
            }

            string campaignId = _client.Create(objectName, campaign);
            return campaignId;
        }

        private string CreateProgram(Plan_Campaign_Program planProgram)
        {
            Dictionary<string, object> program = GetProgram(planProgram, Enums.Mode.Create);

            if (_mappingTactic.ContainsKey("Title") && planProgram != null)
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                if (program.ContainsKey(titleMappedValue))
                {
                    program[titleMappedValue] = Common.TruncateName(program[titleMappedValue].ToString());
                }
            }

            string programId = _client.Create(objectName, program);
            return programId;
        }

        private string CreateTactic(Plan_Campaign_Program_Tactic planTactic)
        {
            Dictionary<string, object> tactic = GetTactic(planTactic, Enums.Mode.Create);
            if (_mappingTactic.ContainsKey("Title") && planTactic != null && _CustomNamingPermissionForInstance && IsClientAllowedForCustomNaming)
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                if (tactic.ContainsKey(titleMappedValue))
                {
                    tactic[titleMappedValue] = Common.GenerateCustomName(planTactic, _clientId);
                    planTactic.TacticCustomName = tactic[titleMappedValue].ToString();
                    tactic[titleMappedValue] = Common.TruncateName(planTactic.TacticCustomName.ToString());
                }
            }
            string tacticId = _client.Create(objectName, tactic);
            return tacticId;
        }

        private string CreateImprovementCampaign(Plan_Improvement_Campaign planIMPCampaign)
        {
            Dictionary<string, object> campaign = GetImprovementCampaign(planIMPCampaign);

            if (_mappingTactic.ContainsKey("Title") && planIMPCampaign != null)
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                if (campaign.ContainsKey(titleMappedValue))
                {
                    campaign[titleMappedValue] = Common.TruncateName(campaign[titleMappedValue].ToString());
                }
            }

            string campaignId = _client.Create(objectName, campaign);
            return campaignId;
        }

        private string CreateImprovementProgram(Plan_Improvement_Campaign_Program planIMPProgram)
        {
            Dictionary<string, object> program = GetImprovementProgram(planIMPProgram, Enums.Mode.Create);

            if (_mappingTactic.ContainsKey("Title") && planIMPProgram != null)
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                if (program.ContainsKey(titleMappedValue))
                {
                    program[titleMappedValue] = Common.TruncateName(program[titleMappedValue].ToString());
                }
            }

            string programId = _client.Create(objectName, program);
            return programId;
        }

        private string CreateImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            Dictionary<string, object> tactic = GetImprovementTactic(planIMPTactic, Enums.Mode.Create);

            if (_mappingTactic.ContainsKey("Title") && planIMPTactic != null)
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                if (tactic.ContainsKey(titleMappedValue))
                {
                    tactic[titleMappedValue] = Common.TruncateName(tactic[titleMappedValue].ToString());
                }
            }

            string tacticId = _client.Create(objectName, tactic);
            return tacticId;
        }

        private bool UpdateCampaign(Plan_Campaign planCampaign)
        {
            Dictionary<string, object> campaign = GetCampaign(planCampaign);

            if (_mappingTactic.ContainsKey("Title") && planCampaign != null)
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                if (campaign.ContainsKey(titleMappedValue))
                {
                    campaign[titleMappedValue] = Common.TruncateName(campaign[titleMappedValue].ToString());
                }
            }

            return _client.Update(objectName, planCampaign.IntegrationInstanceCampaignId, campaign);
        }

        private bool UpdateProgram(Plan_Campaign_Program planProgram)
        {
            Dictionary<string, object> program = GetProgram(planProgram, Enums.Mode.Update);

            if (_mappingTactic.ContainsKey("Title") && planProgram != null)
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                if (program.ContainsKey(titleMappedValue))
                {
                    program[titleMappedValue] = Common.TruncateName(program[titleMappedValue].ToString());
                }
            }

            return _client.Update(objectName, planProgram.IntegrationInstanceProgramId, program);
        }

        private bool UpdateTactic(Plan_Campaign_Program_Tactic planTactic)
        {

            Dictionary<string, object> tactic = GetTactic(planTactic, Enums.Mode.Update);
            if (!string.IsNullOrEmpty(planTactic.TacticCustomName) && _mappingTactic.ContainsKey("Title"))
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();

                if (tactic.ContainsKey(titleMappedValue))
                {
                    tactic[titleMappedValue] = Common.TruncateName(planTactic.TacticCustomName.ToString());
                }
            }
            return _client.Update(objectName, planTactic.IntegrationInstanceTacticId, tactic);
        }

        private bool UpdateImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            Dictionary<string, object> tactic = GetImprovementTactic(planIMPTactic, Enums.Mode.Update);

            if (_mappingTactic.ContainsKey("Title") && planIMPTactic != null)
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                if (tactic.ContainsKey(titleMappedValue))
                {
                    tactic[titleMappedValue] = Common.TruncateName(tactic[titleMappedValue].ToString());
                }
            }

            return _client.Update(objectName, planIMPTactic.IntegrationInstanceTacticId, tactic);
        }

        private bool Delete(string recordid)
        {
            return _client.Delete(objectName, recordid);
        }

        private string GetErrorMessage(SalesforceException e)
        {
            _isResultError = true;
            return e.Error + " : " + e.Message;
        }

        #region helper
        private Dictionary<string, object> GetCampaign(Plan_Campaign planCampaign)
        {
            Dictionary<string, object> campaign = GetTargetKeyValue<Plan_Campaign>(planCampaign, _mappingCampaign);
            return campaign;
        }

        private Dictionary<string, object> GetProgram(Plan_Campaign_Program planProgram, Enums.Mode mode)
        {
            Dictionary<string, object> program = GetTargetKeyValue<Plan_Campaign_Program>(planProgram, _mappingProgram);

            if (mode.Equals(Enums.Mode.Create))
            {
                program.Add(ColumnParentId, _parentId);
            }
            return program;
        }

        private Dictionary<string, object> GetTactic(Plan_Campaign_Program_Tactic planTactic, Enums.Mode mode)
        {
            Dictionary<string, object> tactic = GetTargetKeyValue<Plan_Campaign_Program_Tactic>(planTactic, _mappingTactic);
            if (mode.Equals(Enums.Mode.Create))
            {
                tactic.Add(ColumnParentId, _parentId);
            }

            return tactic;
        }

        private Dictionary<string, object> GetImprovementCampaign(Plan_Improvement_Campaign planIMPCampaign)
        {
            Dictionary<string, object> campaign = GetTargetKeyValue<Plan_Improvement_Campaign>(planIMPCampaign, _mappingImprovementCampaign);
            return campaign;
        }

        private Dictionary<string, object> GetImprovementProgram(Plan_Improvement_Campaign_Program planIMPProgram, Enums.Mode mode)
        {
            Dictionary<string, object> program = GetTargetKeyValue<Plan_Improvement_Campaign_Program>(planIMPProgram, _mappingImprovementProgram);

            if (mode.Equals(Enums.Mode.Create))
            {
                program.Add(ColumnParentId, _parentId);
            }
            return program;
        }

        private Dictionary<string, object> GetImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic, Enums.Mode mode)
        {
            Dictionary<string, object> tactic = GetTargetKeyValue<Plan_Improvement_Campaign_Program_Tactic>(planIMPTactic, _mappingImprovementTactic);
            if (mode.Equals(Enums.Mode.Create))
            {
                tactic.Add(ColumnParentId, _parentId);
            }

            return tactic;
        }


        private Dictionary<string, object> GetTargetKeyValue<T>(object obj, Dictionary<string, string> mappingDataType)
        {
            string status = "Status";
            string createdBy = "CreatedBy";
            string statDate = "StartDate";
            string endDate = "EndDate";
            string effectiveDate = "EffectiveDate";
            string costActual = "CostActual";   // Added by Sohel Pathan on 11/09/2014 for PL ticket #773
            string tacticType = "TacticType";   //// Added by Sohel Pathan on 29/01/2015 for PL ticket #1113

            Type sourceType = ((T)obj).GetType();
            PropertyInfo[] sourceProps = sourceType.GetProperties();
            Dictionary<string, object> keyvaluepair = new Dictionary<string, object>();

            foreach (KeyValuePair<string, string> mapping in mappingDataType)
            {
                PropertyInfo propInfo = sourceProps.FirstOrDefault(property => property.Name.Equals(mapping.Key));
                if (propInfo != null)
                {
                    string value = Convert.ToString(propInfo.GetValue(((T)obj), null));
                    if (mapping.Key == status)
                    {
                        value = GetSalesForceStatus(value);
                    }
                    else if (mapping.Key == createdBy)
                    {
                        value = _mappingUser[Guid.Parse(value)];
                    }
                    else if (mapping.Key == statDate || mapping.Key == endDate || mapping.Key == effectiveDate)
                    {
                        value = Convert.ToDateTime(value).ToString("yyyy-MM-ddThh:mm:ss+hh:mm");
                    }
                    // Start - Added by Sohel Pathan on 11/09/2014 for PL ticket #773
                    else if (mapping.Key == costActual)
                    {
                        value = GetActualCostbyPlanTacticId(((Plan_Campaign_Program_Tactic)obj).PlanTacticId);
                    }
                    // End - Added by Sohel Pathan on 11/09/2014 for PL ticket #773
                    //// Start - Added by Sohel Pathan on 29/01/2015 for PL ticket #1113
                    else if (mapping.Key == tacticType)
                    {
                        value = ((Plan_Campaign_Program_Tactic)obj).TacticType.Title;
                    }
                    //// End - Added by Sohel Pathan on 29/01/2015 for PL ticket #1113

                    keyvaluepair.Add(mapping.Value, value);
                }
                // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                else
                {
                    var mappedData = MapCustomField<T>(obj, sourceProps, mapping);
                    if (mappedData != null)
                    {
                        if (mappedData.Length > 0)
                        {
                            keyvaluepair.Add(mappedData[0].ToString(), mappedData[1].ToString());
                        }
                    }
                }
                // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
            }

            return keyvaluepair;
        }

        /// <summary>
        /// Added By: Sohel Pathan
        /// Added Date: 03/12/2014
        /// Description: Map Custom Field Data for Integration
        /// </summary>
        /// <typeparam name="T">Plan or improvement tactic type.</typeparam>
        /// <param name="obj">Plan or improvement tactic.</param>
        /// <param name="sourceProps">Array of properties for given obj.</param>
        /// <param name="mapping">Mapping field item</param>
        /// <returns>String array with two elements: one Key and one value, to be added in custom field maaping dictionary</returns>
        private string[] MapCustomField<T>(object obj, PropertyInfo[] sourceProps, KeyValuePair<string, string> mapping)
        {
            if (_mappingCustomFields != null)
            {
                if (_mappingCustomFields.Count > 0)
                {
                    string mappingKey = string.Empty;
                    string EntityType = ((T)obj).GetType().BaseType.Name;
                    string EntityTypeId = string.Empty;
                    PropertyInfo propInfoCustom = null;
                    string EntityTypeInitial = string.Empty;

                    if (EntityType == Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program_Tactic.ToString())
                    {
                        EntityTypeInitial = "T";
                        propInfoCustom = sourceProps.FirstOrDefault(property => property.Name.Equals("PlanTacticId", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (EntityType == Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program.ToString())
                    {
                        EntityTypeInitial = "P";
                        propInfoCustom = sourceProps.FirstOrDefault(property => property.Name.Equals("PlanProgramId", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (EntityType == Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign.ToString())
                    {
                        EntityTypeInitial = "C";
                        propInfoCustom = sourceProps.FirstOrDefault(property => property.Name.Equals("PlanCampaignId", StringComparison.OrdinalIgnoreCase));
                    }

                    if (propInfoCustom != null)
                    {
                        EntityTypeId = Convert.ToString(propInfoCustom.GetValue(((T)obj), null));
                    }
                    mappingKey = EntityTypeInitial + "-" + EntityTypeId + "-" + mapping.Key;

                    if (_mappingCustomFields.ContainsKey(mappingKey))
                    {
                        return new string[] { mapping.Value, _mappingCustomFields[mappingKey] };
                    }
                }
            }
            return null;
        }

        private string GetSalesForceStatus(string status)
        {
            string planned = "Planned";
            string InProgress = "In Progress";
            string completed = "Completed";
            string aborted = "Aborted";

            if (status == ExternalIntegration.TacticStatusValues[TacticStatus.Decline.ToString()].ToString())
            {
                return aborted;
            }
            else if (status == ExternalIntegration.TacticStatusValues[TacticStatus.InProgress.ToString()].ToString())
            {
                return InProgress;
            }
            else if (status == ExternalIntegration.TacticStatusValues[TacticStatus.Complete.ToString()].ToString())
            {
                return completed;
            }
            else
            {
                return planned;
            }
        }

        #endregion

        /// <summary>
        /// Added by : Sohel Pathan
        /// Added Date : 03/12/2014
        /// Description : Prepare a dictionary for Custom Fields with CustomFieldId and its value.
        /// </summary>
        /// <param name="EntityIdList">List of Entity Ids with which Custom Fields are associated like PlanCampaignIds for Campaign Entity Type</param>
        /// <param name="EntityType">Type of Entity with which Custom Fields are associated like Campaign, Program or Tactic</param>
        /// <returns>returns dictionary of custom field mapping</returns>
        private Dictionary<string, string> CreateMappingCustomFieldDictionary(List<int> EntityIdList, string EntityType)
        {
            var CustomFieldList = db.CustomField_Entity.Where(ce => EntityIdList.Contains(ce.EntityId) && ce.CustomField.EntityType == EntityType)
                                                        .Select(ce => new { ce.CustomField, ce.CustomFieldEntityId, ce.CustomFieldId, ce.EntityId, ce.Value, ce.CustomField.AbbreviationForMulti }).ToList();
            List<int> CustomFieldIdList = CustomFieldList.Select(cf => cf.CustomFieldId).Distinct().ToList();
            var CustomFieldOptionList = db.CustomFieldOptions.Where(cfo => CustomFieldIdList.Contains(cfo.CustomFieldId)).Select(cfo => new { cfo.CustomFieldOptionId, cfo.Value });

            Dictionary<string, string> CustomFieldsList = new Dictionary<string, string>();
            string EntityTypeInitial = EntityType.Substring(0, 1);

            foreach (var item in CustomFieldList)
            {
                string customKey = EntityTypeInitial + "-" + item.EntityId + "-" + item.CustomFieldId;
                if (item.CustomField.CustomFieldType.Name == Enums.CustomFieldType.TextBox.ToString())
                {
                    CustomFieldsList.Add(customKey, item.Value);
                }
                else if (item.CustomField.CustomFieldType.Name == Enums.CustomFieldType.DropDownList.ToString())
                {
                    if (CustomFieldsList.ContainsKey(customKey))
                    {
                        CustomFieldsList[customKey] = item.AbbreviationForMulti;    //// Added by Sohel Pathan on 29/01/2015 for PL ticket #1142
                    }
                    else
                    {
                        int CustomFieldOptionId = Convert.ToInt32(item.Value);
                        CustomFieldsList.Add(customKey, CustomFieldOptionList.Where(cfo => cfo.CustomFieldOptionId == CustomFieldOptionId).Select(cfo => cfo.Value).FirstOrDefault());
                    }
                }
            }

            return CustomFieldsList;
        }

        /// <summary>
        /// Created By : Viral Kadiya
        /// Created Date : 27/02/2015
        /// Desciption : Get ActualCost based on PlanTacticId
        /// </summary>
        /// <param name="PlanTacticId"></param>
        /// <returns>Actual cost of a Tactic</returns>
        public string GetActualCostbyPlanTacticId(int PlanTacticId)
        {
            string strActualCost = "0";
            try
            {
                strActualCost = _mappingTactic_ActualCost.ToList().Where(tac => tac.Key.Equals(PlanTacticId)).Select(tac => tac.Value).FirstOrDefault();
                return strActualCost;
            }
            catch
            {
                return strActualCost;
            }
        }
    }
}

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
        private Dictionary<int, string> _mappingVertical { get; set; }
        private Dictionary<int, string> _mappingAudience { get; set; }
        private Dictionary<Guid, string> _mappingGeography { get; set; }
        private Dictionary<Guid, string> _mappingBusinessunit { get; set; }
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

        public IntegrationSalesforceClient(int integrationInstanceId, int id, EntityType entityType, Guid userId, int integrationInstanceLogId)
        {
            _integrationInstanceId = integrationInstanceId;
            _id = id;
            _entityType = entityType;
            _userId = userId;
            _integrationInstanceLogId = integrationInstanceLogId;
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

            IntegrationInstance integrationInstance = db.IntegrationInstances.Where(instance => instance.IntegrationInstanceId == _integrationInstanceId).SingleOrDefault();
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
            if (EntityType.Tactic.Equals(_entityType))
            {
                Plan_Campaign_Program_Tactic planTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == _id).SingleOrDefault();
                // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                List<int> tacticIdList = new List<int>() { planTactic.PlanTacticId };
                _mappingCustomFields = CreateMappingCustomFieldDictionary(tacticIdList, Enums.EntityType.Tactic.ToString());
                // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                planTactic = SyncTacticData(planTactic);
                db.SaveChanges();
            }
            else if (EntityType.Program.Equals(_entityType))
            {
                Plan_Campaign_Program planProgram = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == _id).SingleOrDefault();
                // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                List<int> programIdList = new List<int>() { planProgram.PlanProgramId };
                _mappingCustomFields = CreateMappingCustomFieldDictionary(programIdList, Enums.EntityType.Program.ToString());
                // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                planProgram = SyncProgramData(planProgram);
                db.SaveChanges();
            }
            else if (EntityType.Campaign.Equals(_entityType))
            {
                Plan_Campaign planCampaign = db.Plan_Campaign.Where(campaign => campaign.PlanCampaignId == _id).SingleOrDefault();
                // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                List<int> campaignIdList = new List<int>() { planCampaign.PlanCampaignId };
                _mappingCustomFields = CreateMappingCustomFieldDictionary(campaignIdList, Enums.EntityType.Campaign.ToString());
                // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                planCampaign = SyncCampaingData(planCampaign);
                db.SaveChanges();
            }
            else if (EntityType.ImprovementTactic.Equals(_entityType))
            {
                Plan_Improvement_Campaign_Program_Tactic planImprovementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(imptactic => imptactic.ImprovementPlanTacticId == _id).SingleOrDefault();
                planImprovementTactic = SyncImprovementData(planImprovementTactic);
                db.SaveChanges();
            }
            else
            {
                IsInstanceSync = true;
                SyncInstanceData();
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
                isImport = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId.Equals(_integrationInstanceId)).IsImportActuals;
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

            List<int> planIds = db.Plans.Where(p => p.Model.IntegrationInstanceIdINQ == _integrationInstanceId && p.Model.Status.Equals("Published")).Select(p => p.PlanId).ToList();
            Guid ClientId = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;
            int INQStageId = db.Stages.SingleOrDefault(s => s.ClientId == ClientId && s.Code == Common.StageINQ).StageId;
            // Get List of status after Approved Status
            List<string> statusList = Common.GetStatusListAfterApproved();
            List<Plan_Campaign_Program_Tactic> tacticList = db.Plan_Campaign_Program_Tactic.Where(t => planIds.Contains(t.Plan_Campaign_Program.Plan_Campaign.PlanId) && t.StageId == INQStageId && statusList.Contains(t.Status) && t.IsDeployedToIntegration && !t.IsDeleted && t.IntegrationInstanceTacticId != null).ToList();
            string integrationTacticIds = String.Join("','", (from tactic in tacticList select tactic.IntegrationInstanceTacticId));
            //For Testing
            //integrationTacticIds = "'701f00000003S9R','701f00000002cGG','701f00000002cGL'";
            if (integrationTacticIds != string.Empty)
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
                            var responsePull = _client.Query<object>("SELECT " + CampaignId + "," + FirstRespondedDate + " FROM CampaignMember WHERE " + CampaignId + " IN ('" + integrationTacticIds + "') AND " + Status + "= '" + Common.Responded + "'");

                            foreach (var resultin in responsePull)
                            {
                                string TacticResult = resultin.ToString();
                                JObject jobj = JObject.Parse(TacticResult);
                                CampaignMember objCampaign = new CampaignMember();
                                try
                                {
                                    objCampaign.CampaignId = Convert.ToString(jobj[CampaignId]);
                                    objCampaign.FirstRespondedDate = Convert.ToDateTime(jobj[FirstRespondedDate]);
                                }
                                catch (SalesforceException e)
                                {
                                    ErrorFlag = true;
                                    string TacticId = Convert.ToString(jobj[ColumnId]);
                                    var tactic = tacticList.SingleOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
                                    IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                    instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                    instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                    instanceTactic.EntityId = tactic.PlanTacticId;
                                    instanceTactic.EntityType = EntityType.Tactic.ToString();
                                    instanceTactic.Status = StatusResult.Error.ToString();
                                    instanceTactic.Operation = Operation.Import_Cost.ToString();
                                    instanceTactic.SyncTimeStamp = DateTime.Now;
                                    instanceTactic.CreatedDate = DateTime.Now;
                                    instanceTactic.ErrorDescription = GetErrorMessage(e);
                                    instanceTactic.CreatedBy = _userId;
                                    db.Entry(instanceTactic).State = EntityState.Added;
                                }
                                CampaignMemberList.Add(objCampaign);
                            }

                            List<int> OuterTacticIds = tacticList.Select(t => t.PlanTacticId).ToList();
                            List<Plan_Campaign_Program_Tactic_Actual> OuteractualTacticList = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => OuterTacticIds.Contains(actual.PlanTacticId) && actual.StageTitle == Common.StageProjectedStageValue).ToList();
                            OuteractualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                            db.SaveChanges();

                            if (CampaignMemberList.Count > 0)
                            {
                                var CampaignMemberListGroup = CampaignMemberList.GroupBy(cl => new { CampaignId = cl.CampaignId, Month = cl.FirstRespondedDate.ToString("MM/yyyy") }).Select(cl =>
                                    new
                                    {
                                        CampaignId = cl.Key.CampaignId,
                                        TacticId = tacticList.Single(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).PlanTacticId,
                                        Period = "Y" + Convert.ToDateTime(cl.Key.Month).Month,
                                        IsYear = (tacticList.Single(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year == Convert.ToDateTime(cl.Key.Month).Year) ? true : false,
                                        Count = cl.Count()
                                    }
                                    ).Where(cm => cm.IsYear).ToList();



                                foreach (var tactic in tacticList)
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
                                // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, string.Empty);
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

        private class OpportunityMember
        {
            public string CampaignId { get; set; }
            public DateTime CloseDate { get; set; }
            public double Amount { get; set; }
        }

        private void PullingCWRevenue()
        {
            // Insert log into IntegrationInstanceSection, Dharmraj PL#684
            int IntegrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), DateTime.Now, _userId);

            List<int> planIds = db.Plans.Where(p => p.Model.IntegrationInstanceIdCW == _integrationInstanceId && p.Model.Status.Equals("Published")).Select(p => p.PlanId).ToList();
            Guid ClientId = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;
            // Get List of status after Approved Status
            List<string> statusList = Common.GetStatusListAfterApproved();
            List<Plan_Campaign_Program_Tactic> tacticList = db.Plan_Campaign_Program_Tactic.Where(t => planIds.Contains(t.Plan_Campaign_Program.Plan_Campaign.PlanId) && statusList.Contains(t.Status) && t.IsDeployedToIntegration && !t.IsDeleted && t.IntegrationInstanceTacticId != null).ToList();
            string integrationTacticIds = String.Join("','", (from tactic in tacticList select tactic.IntegrationInstanceTacticId));
            if (integrationTacticIds != string.Empty)
            {
                try
                {
                    string CampaignId = string.Empty;// "CampaignId";
                    string CloseDate = string.Empty;// "CloseDate";
                    string Amount = string.Empty;// "Amount";
                    string StageName = string.Empty;// "StageName";
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

                        if (CampaignId != string.Empty && CloseDate != string.Empty && Amount != string.Empty && StageName != string.Empty)
                        {

                            List<OpportunityMember> OpportunityMemberList = new List<OpportunityMember>();
                            var cwRecords = _client.Query<object>("SELECT " + CampaignId + "," + CloseDate + "," + Amount + " FROM Opportunity WHERE " + CampaignId + " IN ('" + integrationTacticIds + "') AND " + StageName + "= '" + Common.ClosedWon + "'");
                            foreach (var resultin in cwRecords)
                            {
                                string TacticResult = resultin.ToString();
                                JObject jobj = JObject.Parse(TacticResult);
                                OpportunityMember objOpp = new OpportunityMember();
                                try
                                {
                                    objOpp.CampaignId = Convert.ToString(jobj[CampaignId]);
                                    objOpp.CloseDate = Convert.ToDateTime(jobj[CloseDate]);
                                    objOpp.Amount = Convert.ToDouble(jobj[Amount]);
                                }
                                catch (SalesforceException e)
                                {
                                    ErrorFlag = true;
                                    string TacticId = Convert.ToString(jobj[ColumnId]);
                                    var tactic = tacticList.SingleOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
                                    IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                    instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                    instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                    instanceTactic.EntityId = tactic.PlanTacticId;
                                    instanceTactic.EntityType = EntityType.Tactic.ToString();
                                    instanceTactic.Status = StatusResult.Error.ToString();
                                    instanceTactic.Operation = Operation.Import_Cost.ToString();
                                    instanceTactic.SyncTimeStamp = DateTime.Now;
                                    instanceTactic.CreatedDate = DateTime.Now;
                                    instanceTactic.ErrorDescription = GetErrorMessage(e);
                                    instanceTactic.CreatedBy = _userId;
                                    db.Entry(instanceTactic).State = EntityState.Added;
                                }
                                OpportunityMemberList.Add(objOpp);
                            }

                            List<int> OuterTacticIds = tacticList.Select(t => t.PlanTacticId).ToList();
                            List<Plan_Campaign_Program_Tactic_Actual> OuteractualTacticList = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => OuterTacticIds.Contains(actual.PlanTacticId) && (actual.StageTitle == Common.StageRevenue || actual.StageTitle == Common.StageCW)).ToList();
                            OuteractualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                            db.SaveChanges();

                            if (OpportunityMemberList.Count > 0)
                            {
                                var OpportunityMemberListGroup = OpportunityMemberList.GroupBy(cl => new { CampaignId = cl.CampaignId, Month = cl.CloseDate.ToString("MM/yyyy") }).Select(cl =>
                                    new
                                    {
                                        CampaignId = cl.Key.CampaignId,
                                        TacticId = tacticList.Single(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).PlanTacticId,
                                        Period = "Y" + Convert.ToDateTime(cl.Key.Month).Month,
                                        IsYear = (tacticList.Single(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year == Convert.ToDateTime(cl.Key.Month).Year) ? true : false,
                                        Count = cl.Count(),
                                        Revenue = cl.Sum(c => c.Amount)
                                    }
                                    ).Where(om => om.IsYear).ToList();



                                foreach (var tactic in tacticList)
                                {
                                    var innerOpportunityMember = OpportunityMemberListGroup.Where(cm => cm.TacticId == tactic.PlanTacticId).ToList();
                                    foreach (var objOpportunityMember in innerOpportunityMember)
                                    {
                                        Plan_Campaign_Program_Tactic_Actual objPlanTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                        objPlanTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                        objPlanTacticActual.PlanTacticId = objOpportunityMember.TacticId;
                                        objPlanTacticActual.Period = objOpportunityMember.Period;
                                        objPlanTacticActual.StageTitle = Common.StageCW;
                                        objPlanTacticActual.Actualvalue = objOpportunityMember.Count;
                                        objPlanTacticActual.CreatedBy = _userId;
                                        objPlanTacticActual.CreatedDate = DateTime.Now;
                                        db.Entry(objPlanTacticActual).State = EntityState.Added;

                                        Plan_Campaign_Program_Tactic_Actual objPlanTacticActualRevenue = new Plan_Campaign_Program_Tactic_Actual();
                                        objPlanTacticActualRevenue = new Plan_Campaign_Program_Tactic_Actual();
                                        objPlanTacticActualRevenue.PlanTacticId = objOpportunityMember.TacticId;
                                        objPlanTacticActualRevenue.Period = objOpportunityMember.Period;
                                        objPlanTacticActualRevenue.StageTitle = Common.StageRevenue;
                                        objPlanTacticActualRevenue.Actualvalue = objOpportunityMember.Revenue;
                                        objPlanTacticActualRevenue.CreatedBy = _userId;
                                        objPlanTacticActualRevenue.CreatedDate = DateTime.Now;
                                        db.Entry(objPlanTacticActualRevenue).State = EntityState.Added;
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
                                db.SaveChanges();
                            }

                            if (ErrorFlag)
                            {
                                // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, string.Empty);
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

        //private void GetDataForTacticandUpdate()
        //{
        //    // Insert log into IntegrationInstanceSection, Dharmraj PL#684
        //    int IntegrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.ImportActual.ToString(), DateTime.Now, _userId);

        //    string actualCost = "CostActual";

        //    var fieldname = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.IntegrationInstanceId.Equals(_integrationInstanceId) && mapping.GameplanDataType.TableName == "Plan_Campaign_Program_Tactic" &&
        //                                                                    mapping.GameplanDataType.ActualFieldName == actualCost).Select(mapping => mapping.TargetDataType).FirstOrDefault();
        //    if (fieldname != null)
        //    {
        //        // Change id for in which it get actual value
        //        List<int> planIds = db.Plans.Where(p => p.Model.IntegrationInstanceId == _integrationInstanceId && p.Model.Status.Equals("Published")).Select(p => p.PlanId).ToList();
        //        Guid ClientId = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;
        //        // Get List of status after Approved Status
        //        List<string> statusList = Common.GetStatusListAfterApproved();
        //        List<Plan_Campaign_Program_Tactic> tacticList = db.Plan_Campaign_Program_Tactic.Where(t => planIds.Contains(t.Plan_Campaign_Program.Plan_Campaign.PlanId) && statusList.Contains(t.Status) && t.IsDeployedToIntegration && t.IntegrationInstanceTacticId != null).ToList();
        //        string integrationTacticIds = String.Join("','", (from tactic in tacticList select tactic.IntegrationInstanceTacticId));
        //        if (integrationTacticIds != string.Empty)
        //        {
        //            try
        //            {
        //                bool ErrorFlag = false;
        //                List<ImportCostMember> ImportCostMemberList = new List<ImportCostMember>();
        //                var ActualCostList = _client.Query<object>("SELECT " + fieldname + "," + ColumnId + " FROM " + this.objectName + " WHERE " + ColumnId + " in ('" + integrationTacticIds + "')");

        //                foreach (var resultin in ActualCostList)
        //                {
        //                    string TacticResult = resultin.ToString();
        //                    JObject jobj = JObject.Parse(TacticResult);
        //                    ImportCostMember objImport = new ImportCostMember();
        //                    try
        //                    {
        //                        objImport.CampaignId = Convert.ToString(jobj[ColumnId]);
        //                        objImport.actualCost = Convert.ToDouble(jobj[fieldname]);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        ErrorFlag = true;
        //                        string TacticId =Convert.ToString(jobj[ColumnId]);
        //                        var tactic = tacticList.SingleOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
        //                        IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
        //                        instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
        //                        instanceTactic.IntegrationInstanceId = _integrationInstanceId;
        //                        instanceTactic.EntityId = tactic.PlanTacticId;
        //                        instanceTactic.EntityType = EntityType.Tactic.ToString();
        //                        instanceTactic.Status = StatusResult.Error.ToString();
        //                        instanceTactic.Operation = Operation.Import_Cost.ToString();
        //                        instanceTactic.SyncTimeStamp = DateTime.Now;
        //                        instanceTactic.CreatedDate = DateTime.Now;
        //                        instanceTactic.ErrorDescription = ex.Message;
        //                        instanceTactic.CreatedBy = _userId;
        //                        db.Entry(instanceTactic).State = EntityState.Added;
        //                    }
        //                    ImportCostMemberList.Add(objImport);
        //                }
        //                db.SaveChanges();

        //                if (ImportCostMemberList.Count > 0)
        //                {
        //                    List<string> IntegrationTacticIdList = ImportCostMemberList.Select(import => import.CampaignId).Distinct().ToList();
        //                    List<Plan_Campaign_Program_Tactic> innerTacticList = tacticList.Where(t => IntegrationTacticIdList.Contains(t.IntegrationInstanceTacticId)).ToList();

        //                    //Added by dharmraj for ticket #733 : Actual cost - Changes related to integraton with Eloqua/SF.
        //                    List<Plan_Campaign_Program_Tactic_Actual> actualTacicList = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => IntegrationTacticIdList.Contains(ta.Plan_Campaign_Program_Tactic.IntegrationInstanceTacticId) && ta.StageTitle == Common.StageCost).ToList();
        //                    actualTacicList.ForEach(t => db.Entry(t).State = EntityState.Deleted);
        //                    db.SaveChanges();

        //                    foreach (var tactic in innerTacticList)
        //                    {
        //                        // Start Added by dharmraj for ticket #733 : Actual cost - Changes related to integraton with Eloqua/SF.
        //                        //tactic.CostActual = ImportCostMemberList.SingleOrDefault(import => import.CampaignId == tactic.IntegrationInstanceTacticId).actualCost;
        //                        double actualValue = ImportCostMemberList.SingleOrDefault(import => import.CampaignId == tactic.IntegrationInstanceTacticId).actualCost;
        //                        int totalMonth = 0;
        //                        if (tactic.StartDate.Month == tactic.EndDate.Month)
        //                        {
        //                            totalMonth = 1;
        //                        }
        //                        else
        //                        {
        //                            totalMonth = tactic.EndDate.Month - tactic.StartDate.Month + 1;
        //                        }

        //                        double actualValueTotalTemp = 0;
        //                        for (int iMonth = tactic.StartDate.Month; iMonth <= tactic.EndDate.Month; iMonth++)
        //                        {
        //                            double actualValueMonthWise = Math.Round(actualValue / totalMonth);
        //                            actualValueTotalTemp += actualValueMonthWise;
        //                            if (iMonth == tactic.EndDate.Month && actualValueTotalTemp != actualValue)
        //                            {
        //                                actualValueMonthWise = actualValueMonthWise - (actualValueTotalTemp - actualValue);
        //                            }

        //                            Plan_Campaign_Program_Tactic_Actual actualTactic = new Plan_Campaign_Program_Tactic_Actual();
        //                            actualTactic.Actualvalue = actualValueMonthWise;
        //                            actualTactic.PlanTacticId = tactic.PlanTacticId;
        //                            actualTactic.Period = "Y" + iMonth;
        //                            actualTactic.StageTitle = Common.StageCost;
        //                            //change date & created by
        //                            actualTactic.CreatedDate = DateTime.Now;
        //                            actualTactic.CreatedBy = _userId;
        //                            db.Entry(actualTactic).State = EntityState.Added;
        //                        }

        //                        // End Added by dharmraj for ticket #733 : Actual cost - Changes related to integraton with Eloqua/SF.

        //                        tactic.ModifiedBy = _userId;
        //                        tactic.ModifiedDate = DateTime.Now;
        //                        tactic.LastSyncDate = DateTime.Now;

        //                        IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
        //                        instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
        //                        instanceTactic.IntegrationInstanceId = _integrationInstanceId;
        //                        instanceTactic.EntityId = tactic.PlanTacticId;
        //                        instanceTactic.EntityType = EntityType.Tactic.ToString();
        //                        instanceTactic.Status = StatusResult.Success.ToString();
        //                        instanceTactic.Operation = Operation.Import_Cost.ToString();
        //                        instanceTactic.SyncTimeStamp = DateTime.Now;
        //                        instanceTactic.CreatedDate = DateTime.Now;
        //                        instanceTactic.CreatedBy = _userId;
        //                        db.Entry(instanceTactic).State = EntityState.Added;
        //                    }
        //                    db.SaveChanges();
        //                }

        //                if (ErrorFlag)
        //                {
        //                    // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
        //                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, string.Empty);
        //                }
        //                else
        //                {
        //                    // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
        //                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
        //                }
        //            }
        //            catch (SalesforceException e)
        //            {
        //                string msg = GetErrorMessage(e);                       
        //                // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
        //                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, msg);
        //            }
        //        }
        //        else
        //        {
        //            // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
        //            Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
        //        }
        //    }
        //    else
        //    {
        //        // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
        //        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, Common.msgMappingFieldsNotFound);
        //    }
        //}

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

            Guid clientId = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;
            _mappingVertical = db.Verticals.Where(v => v.ClientId == clientId).Select(v => new { v.VerticalId, v.Title })
                                .ToDictionary(v => v.VerticalId, v => v.Title);
            _mappingAudience = db.Audiences.Where(a => a.ClientId == clientId).Select(a => new { a.AudienceId, a.Title })
                                .ToDictionary(a => a.AudienceId, a => a.Title);
            _mappingBusinessunit = db.BusinessUnits.Where(b => b.ClientId == clientId).Select(b => new { b.BusinessUnitId, b.Title })
                                .ToDictionary(b => b.BusinessUnitId, b => b.Title);
            _mappingGeography = db.Geographies.Where(g => g.ClientId == clientId).Select(g => new { g.GeographyId, g.Title })
                                .ToDictionary(g => g.GeographyId, g => g.Title);

            BDSService.BDSServiceClient objBDSservice = new BDSService.BDSServiceClient();
            _mappingUser = objBDSservice.GetUserListByClientId(clientId).Select(u => new { u.UserId, u.FirstName, u.LastName }).ToDictionary(u => u.UserId, u => u.FirstName + " " + u.LastName);

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
                        planCampaign.IntegrationInstanceCampaignId = null;
                        planCampaign = SyncCampaingData(planCampaign);
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
                        planProgram.IntegrationInstanceProgramId = null;
                        planProgram = SyncProgramData(planProgram);
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
                        planTactic.IntegrationInstanceTacticId = null;
                        planTactic = SyncTacticData(planTactic);
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
                        planIMPTactic.IntegrationInstanceTacticId = null;
                        planIMPTactic = SyncImprovementData(planIMPTactic);
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
            string campaignId = _client.Create(objectName, campaign);
            return campaignId;
        }

        private string CreateProgram(Plan_Campaign_Program planProgram)
        {
            Dictionary<string, object> program = GetProgram(planProgram, Enums.Mode.Create);
            string programId = _client.Create(objectName, program);
            return programId;
        }

        private string CreateTactic(Plan_Campaign_Program_Tactic planTactic)
        {
            Dictionary<string, object> tactic = GetTactic(planTactic, Enums.Mode.Create);
            if (_mappingTactic.ContainsKey("Title") && planTactic!=null)
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                if (tactic.ContainsKey(titleMappedValue))
                {
                    tactic[titleMappedValue] = Common.GenerateCustomName(planTactic, planTactic.BusinessUnit.ClientId);
                }
            }
            string tacticId = _client.Create(objectName, tactic);
            return tacticId;
        }

        private string CreateImprovementCampaign(Plan_Improvement_Campaign planIMPCampaign)
        {
            Dictionary<string, object> campaign = GetImprovementCampaign(planIMPCampaign);
            string campaignId = _client.Create(objectName, campaign);
            return campaignId;
        }

        private string CreateImprovementProgram(Plan_Improvement_Campaign_Program planIMPProgram)
        {
            Dictionary<string, object> program = GetImprovementProgram(planIMPProgram, Enums.Mode.Create);
            string programId = _client.Create(objectName, program);
            return programId;
        }

        private string CreateImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            Dictionary<string, object> tactic = GetImprovementTactic(planIMPTactic, Enums.Mode.Create);
            string tacticId = _client.Create(objectName, tactic);
            return tacticId;
        }

        private bool UpdateCampaign(Plan_Campaign planCampaign)
        {
            Dictionary<string, object> campaign = GetCampaign(planCampaign);
            return _client.Update(objectName, planCampaign.IntegrationInstanceCampaignId, campaign);
        }

        private bool UpdateProgram(Plan_Campaign_Program planProgram)
        {
            Dictionary<string, object> program = GetProgram(planProgram, Enums.Mode.Update);
            return _client.Update(objectName, planProgram.IntegrationInstanceProgramId, program);
        }

        private bool UpdateTactic(Plan_Campaign_Program_Tactic planTactic)
        {
            Dictionary<string, object> tactic = GetTactic(planTactic, Enums.Mode.Update);
            return _client.Update(objectName, planTactic.IntegrationInstanceTacticId, tactic);
        }

        private bool UpdateImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            Dictionary<string, object> tactic = GetImprovementTactic(planIMPTactic, Enums.Mode.Update);
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
            string verticalId = "VerticalId";
            string audienceId = "AudienceId";
            string businessUnitId = "BusinessUnitId";
            string geographyid = "GeographyId";
            string createdBy = "CreatedBy";
            string statDate = "StartDate";
            string endDate = "EndDate";
            string effectiveDate = "EffectiveDate";
            string costActual = "CostActual";   // Added by Sohel Pathan on 11/09/2014 for PL ticket #773

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
                    else if (mapping.Key == verticalId)
                    {
                        value = _mappingVertical[Convert.ToInt32(value)];
                    }
                    else if (mapping.Key == audienceId)
                    {
                        value = _mappingAudience[Convert.ToInt32(value)];
                    }
                    else if (mapping.Key == geographyid)
                    {
                        value = _mappingGeography[Guid.Parse(value)];
                    }
                    else if (mapping.Key == businessUnitId)
                    {
                        value = _mappingBusinessunit[Guid.Parse(value)];
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
                        value = Common.CalculateActualCost(((Plan_Campaign_Program_Tactic)obj).PlanTacticId);
                    }
                    // End - Added by Sohel Pathan on 11/09/2014 for PL ticket #773

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
                                                        .Select(ce => new { ce.CustomField, ce.CustomFieldEntityId, ce.CustomFieldId, ce.EntityId, ce.Value }).ToList();
            List<int> CustomFieldIdList = CustomFieldList.Select(cf => cf.CustomFieldId).Distinct().ToList();
            var CustomFieldOptionList = db.CustomFieldOptions.Where(cfo => CustomFieldIdList.Contains(cfo.CustomFieldId)).Select(cfo => new { cfo.CustomFieldOptionId, cfo.Value });

            Dictionary<string, string> CustomFieldsList = new Dictionary<string, string>();
            string EntityTypeInitial = EntityType.Substring(0, 1);

            foreach (var item in CustomFieldList)
            {
                if (item.CustomField.CustomFieldType.Name == Enums.CustomFieldType.TextBox.ToString())
                {
                    CustomFieldsList.Add(EntityTypeInitial + "-" + item.EntityId + "-" + item.CustomFieldId, item.Value);
                }
                else if (item.CustomField.CustomFieldType.Name == Enums.CustomFieldType.DropDownList.ToString())
                {
                    int CustomFieldOptionId = Convert.ToInt32(item.Value);
                    CustomFieldsList.Add(EntityTypeInitial + "-" + item.EntityId + "-" + item.CustomFieldId, CustomFieldOptionList.Where(cfo => cfo.CustomFieldOptionId == CustomFieldOptionId).Select(cfo => cfo.Value).FirstOrDefault());
                }
            }

            return CustomFieldsList;
        }
    }
}

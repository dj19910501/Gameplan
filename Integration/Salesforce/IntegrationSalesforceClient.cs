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
using System.Text;
using System.Data.Common;
using System.Net;
using System.Data.SqlClient;

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
        private List<CustomFiledMapping> _mappingCustomFields { get; set; }  // Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
        private List<CampaignNameConvention> SequencialOrderedTableList { get; set; }
        private int _integrationInstanceId { get; set; }
        private int _id { get; set; }
        private Guid _userId { get; set; }
        private int _integrationInstanceLogId { get; set; }
        private EntityType _entityType { get; set; }
        private readonly string objectName;
        private string _parentId { get; set; }
        private string ColumnParentId = "ParentId";
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
        private List<string> statusList { get; set; }
        bool _IsSFDCWithMarketo = false; // Add By Nishant Sheth // Make Hireachy for Marketo sync on SFDC
        private List<SyncError> _lstSyncError = new List<SyncError>();
        private List<SalesForceObjectFieldDetails> lstSalesforceFieldDetail = new List<SalesForceObjectFieldDetails>();
        private string PeriodChar = "Y";
        private string PlanName = string.Empty;
        private DateTime? startDate; // Use this startDate varialbe while push linked tactic to SFDC.
        private int sfdcTitleLengthLimit=255;
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
            Dictionary<string, string> sfdcCredentials = new Dictionary<string, string>();
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            #region "Commented code: SFDC authenticate through Integration Web API"
            ///// Added by Bhavesh
            ///// Date: 28/7/2015
            ///// Ticket : #1385	Enable TLS 1.1 or higher Encryption for Salesforce
            ///// Start : #1385
            //if (Common.EnableTLS1AndHigher == "true")
            //{
            //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //}
            ///// End : #1385

            //_client = new SalesforceClient();
            //var authFlow = new UsernamePasswordAuthenticationFlow(_consumerKey, _consumerSecret, _username, _password + _securityToken); 
            #endregion
            int entityId = _integrationInstanceId;
            //authFlow.TokenRequestEndpointUrl = _apiURL;   // commented by viral #2251.
            try
            {
                if (_entityType.Equals(EntityType.Tactic) || _entityType.Equals(EntityType.ImprovementTactic))
                    entityId = _id;
                Common.SaveIntegrationInstanceLogDetails(entityId, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Salesforce Authentication start.");
                //_client.Authenticate(authFlow);

                sfdcCredentials = GetsfdcCredentials();

                _isAuthenticated = (new ApiIntegration()).AuthenticateforSFDC(sfdcCredentials, _applicationId.ToString(), _clientId.ToString());

                Common.SaveIntegrationInstanceLogDetails(entityId, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Salesforce Authentication end.");
            }
            catch (SalesforceException ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                _ErrorMessage = exMessage;
                _isAuthenticated = false;
                Common.SaveIntegrationInstanceLogDetails(entityId, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "An error ocured while authenticating with Salesforce: " + exMessage);
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
        /// Created by Viral on 14 June 2016
        /// </summary>
        /// <param name="objectName">Sales force object name ex. Campaign</param>
        /// <returns>Returns property list of salesforce object</returns>
        public List<string> GetSFDCObjectList()
        {
            List<string> TargetDataTypeList = new List<string>();
            List<SalesForceFieldsDetails> lstAllTargetDataTypeList = new List<SalesForceFieldsDetails>();
            lstAllTargetDataTypeList = (new ApiIntegration()).GetSFDCmetaDataFields(GetsfdcCredentials(), _applicationId.ToString(), _clientId.ToString());
            TargetDataTypeList = lstAllTargetDataTypeList.Select(fld => fld.Name).ToList();
            return TargetDataTypeList.OrderBy(q => q).ToList();
        }

        /// <summary>
        /// Modified By Dharmraj on 6-8-2014, Ticket #658
        /// </summary>
        /// <param name="objectName">Sales force object name</param>
        /// <returns>Returns property list of salesforce object</returns>
        public List<PullClosedDealModel> GetPullClosedDealsTargetDataType(string objectName)
        {
            List<string> TargetDataTypeList = new List<string>();
            string metadata = _client.ReadMetaData(objectName);
            JObject data = JObject.Parse(metadata);
            List<PullClosedDealModel> pickListresult = new List<PullClosedDealModel>();
            PullClosedDealModel objPickItem;
            List<string> picklistValues;
            foreach (var result in data["fields"])
            {
                objPickItem = new PullClosedDealModel();
                objPickItem.fieldname = (string)result["name"];
                var picklist = result["picklistValues"];
                objPickItem.IsPicklistExist = (picklist != null && picklist.Count() > 0) ? true : false;

                if (objPickItem.IsPicklistExist)
                {
                    picklistValues = new List<string>();
                    picklist.ToList().ForEach(item => picklistValues.Add((string)item["label"]));
                    objPickItem.pickList = picklistValues;
                }
                else
                    objPickItem.pickList = new List<string>();

                pickListresult.Add(objPickItem);
            }
            return pickListresult;
        }

        /// <summary>
        /// Modified By Mitesh on 25-07-2015
        /// To Fetch salesforce target field details 
        /// </summary>
        /// <param name="objectName">Sales force object name</param>
        /// <returns>Returns property list with length and data type of sales-force object</returns>
        public List<SalesForceObjectFieldDetails> GetTargetDataTypeWithLengthAndDatatype(string objectName)
        {
            List<SalesForceObjectFieldDetails> TargetDataTypeList = new List<SalesForceObjectFieldDetails>();
            string metadata = _client.ReadMetaData(objectName);
            JObject data = JObject.Parse(metadata);
            foreach (var result in data["fields"])
            {
                SalesForceObjectFieldDetails objFieldDetails = new SalesForceObjectFieldDetails();
                objFieldDetails.TargetField = (string)result["name"];
                objFieldDetails.Length = (int)result["length"];
                objFieldDetails.TargetDatatype = ((string)result["soapType"]).Replace("xsd:", "");
                TargetDataTypeList.Add(objFieldDetails);

            }
            return TargetDataTypeList.OrderBy(q => q.TargetField).ToList();
        }

        /// <summary>
        /// Created by Viral
        /// To Fetch salesforce target field details 
        /// </summary>
        /// <param name="objectName">Sales force object name</param>
        /// <returns>Returns property list with length and data type of sales-force object</returns>
        public List<SalesForceObjectFieldDetails> GetSFDCTargetFieldList()
        {
            List<SalesForceObjectFieldDetails> TargetDataTypeList = new List<SalesForceObjectFieldDetails>();
            List<SalesForceFieldsDetails> lstAllTargetDataTypeList = new List<SalesForceFieldsDetails>();
            SalesForceObjectFieldDetails objFieldDetails;
            lstAllTargetDataTypeList = (new ApiIntegration()).GetSFDCmetaDataFields(GetsfdcCredentials(), _applicationId.ToString(), _clientId.ToString());
            foreach (SalesForceFieldsDetails objSFDField in lstAllTargetDataTypeList)
            {
                objFieldDetails = new SalesForceObjectFieldDetails();
                objFieldDetails.TargetField = objSFDField.Name;
                objFieldDetails.Length = objSFDField.Length;
                objFieldDetails.TargetDatatype = objSFDField.SoapType.Replace("xsd:", "");
                TargetDataTypeList.Add(objFieldDetails);

                // Assign SFDC Title field length to Global variable(i.e. sfdcTitleLengthLimit)
                if (objSFDField.Name.Equals("Name"))
                    sfdcTitleLengthLimit = objSFDField.Length;
            }
            

            return TargetDataTypeList.OrderBy(q => q.TargetField).ToList();
        }

        /// <summary>
        /// Get SFDC require Authenticate details ex. ConsumerKey,ClientSecret,SecurityToken,Username,Password,APIUrl
        /// </summary>
        /// <returns> returns dictionary of SFDC credentials</returns>
        public Dictionary<string, string> GetsfdcCredentials()
        {
            Dictionary<string, string> sfdcCredentials = new Dictionary<string, string>();
            sfdcCredentials.Add("ConsumerKey", _consumerKey);
            sfdcCredentials.Add("ClientSecret", _consumerSecret);
            sfdcCredentials.Add("SecurityToken", _securityToken);
            sfdcCredentials.Add("Username", _username);
            sfdcCredentials.Add("Password", _password);
            sfdcCredentials.Add("APIUrl", _apiURL);
            return sfdcCredentials;
        }

        public class SalesForceObjectFieldDetails
        {
            public string Section { get; set; }
            public string TargetField { get; set; }
            public string SourceField { get; set; }
            public int Length { get; set; }
            public string TargetDatatype { get; set; }
            public string SourceDatatype { get; set; }
        }

        /// <summary>
        /// Function to sync data from gameplan to salesforce.
        /// </summary>
        /// <returns>returns flag for sync status</returns>
        public bool SyncData(out List<SyncError> lstSyncError)
        {
            lstSyncError = new List<SyncError>();
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            // Insert log into IntegrationInstanceSection, Dharmraj PL#684
            _integrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), DateTime.Now, _userId);
            _isResultError = false;
            /// Set client Id based on integration instance.
            _clientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;

            bool IsInstanceSync = false;
            statusList = Common.GetStatusListAfterApproved();
            StringBuilder sbMessage = new StringBuilder();
            int logRecordSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LogRecordSize"]);
            int pushRecordBatchSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IntegrationPushRecordBatchSize"]);
            try
            {

                if (EntityType.Tactic.Equals(_entityType))
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "SyncTacticData process start.");
                    //TODO: Add here log for get tactic : 
                    List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).ToList();
                    Plan_Campaign_Program_Tactic planTactic = tblPlanTactic.Where(tactic => tactic.PlanTacticId == _id && statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).FirstOrDefault();
                    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    if (planTactic != null)
                    {
                        SyncDatabyEntity(planTactic);
                        #region "Old Code: commented on 03/23/2016"
                        //Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                        //_isResultError = SetMappingDetails();
                        //if (!_isResultError)
                        //{
                        //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");

                        //    List<int> tacticIdList = new List<int>() { planTactic.PlanTacticId };
                        //    _mappingTactic_ActualCost = Common.CalculateActualCostTacticslist(tacticIdList);
                        //    List<int> programIdList = new List<int>() { planTactic.PlanProgramId };
                        //    var lstCustomFieldsprogram = CreateMappingCustomFieldDictionary(programIdList, Enums.EntityType.Program.ToString());
                        //    List<int> campaignIdList = new List<int>() { planTactic.Plan_Campaign_Program.PlanCampaignId };
                        //    var lstCustomFieldsCampaign = CreateMappingCustomFieldDictionary(campaignIdList, Enums.EntityType.Campaign.ToString());
                        //    _mappingCustomFields = CreateMappingCustomFieldDictionary(tacticIdList, Enums.EntityType.Tactic.ToString());

                        //    if (lstCustomFieldsprogram.Count > 0)
                        //    {
                        //        _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsprogram).ToList();
                        //}
                        //    if (lstCustomFieldsCampaign.Count > 0)
                        //    {
                        //        _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsCampaign).ToList();
                        //    }
                        //    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997

                        //    #region "Get Linked Tactic"
                        //    int linkedTacticId = 0;
                        //    Plan_Campaign_Program_Tactic objLinkedTactic = new Plan_Campaign_Program_Tactic();
                        //    if (planTactic != null)
                        //    {
                        //        linkedTacticId = (planTactic.LinkedTacticId.HasValue) ? planTactic.LinkedTacticId.Value : 0;
                        //        //if (linkedTacticId <= 0)
                        //        //{
                        //        //    objLinkedTactic = tblPlanTactic.Where(tactic => tactic.LinkedTacticId == planTactic.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                        //        //    linkedTacticId = objLinkedTactic != null ? objLinkedTactic.PlanTacticId : 0;
                        //        //}
                        //    }
                        //    if (linkedTacticId > 0)
                        //    {
                        //        objLinkedTactic = tblPlanTactic.Where(tactic => tactic.PlanTacticId == linkedTacticId).FirstOrDefault();
                        //    }
                        //    #endregion

                        //    planTactic = SyncTacticData(planTactic, ref sbMessage, objLinkedTactic);

                        //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //    db.SaveChanges();
                        //}
                        //else
                        //{
                        //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Error, "Set Mapping details.");
                        //} 
                        #endregion
                    }
                    else
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Tactic does not exist.");
                    }
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncTacticData process end.");
                }
                else if (EntityType.Program.Equals(_entityType))
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "SyncProgramData process start.");
                    Plan_Campaign_Program planProgram = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == _id && statusList.Contains(program.Status) && program.IsDeployedToIntegration && !program.IsDeleted).FirstOrDefault();
                    if (planProgram != null)
                    {
                        SyncDatabyEntity(planProgram);
                        #region "Old Code: commented on 03/23/2016"
                        //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                        //    _isResultError = SetMappingDetails();
                        //    if (!_isResultError)
                        //    {
                        //        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");

                        //    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //    List<int> programIdList = new List<int>() { planProgram.PlanProgramId };
                        //    List<Plan_Campaign_Program> programList = db.Plan_Campaign_Program.Where(program => program.PlanProgramId.Equals(planProgram.PlanProgramId) && !program.IsDeleted).ToList();

                        //    List<int> campaignIdList = new List<int>() { planProgram.PlanCampaignId };

                        //    _mappingCustomFields = CreateMappingCustomFieldDictionary(campaignIdList, Enums.EntityType.Campaign.ToString());
                        //    //_mappingCustomFields = CreateMappingCustomFieldDictionary(programIdList, Enums.EntityType.Program.ToString());
                        //    //if (lstCustomFieldsCampaign.Count > 0)
                        //    //{
                        //    //    _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsCampaign).ToList();
                        //    //}
                        //    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //    //planProgram = SyncProgramData(planProgram, ref sbMessage);

                        //    #region "Create CampaignIdList, ProgramIdList & TacticIdList"
                        //    //List<Plan_Campaign> campaignList = db.Plan_Campaign.Where(campaign => planIds.Contains(campaign.PlanId) && !campaign.IsDeleted).ToList();
                        //    //List<int> campaignIdList = campaignList.Select(c => c.PlanCampaignId).ToList();

                        //    List<Plan_Campaign_Program_Tactic> tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).ToList();

                        //    List<Plan_Campaign_Program_Tactic> tacticList = tblTactic.Where(tactic => programIdList.Contains(tactic.PlanProgramId)).ToList();

                        //    //List<int> campaignIdForTactic = tacticList.Where(tactic => string.IsNullOrWhiteSpace(tactic.Plan_Campaign_Program.Plan_Campaign.IntegrationInstanceCampaignId)).Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
                        //    List<int> programIdForTactic = tacticList.Where(tactic => string.IsNullOrWhiteSpace(tactic.Plan_Campaign_Program.IntegrationInstanceProgramId)).Select(tactic => tactic.PlanProgramId).ToList();

                        //    int page = 0;
                        //    int total = 0;
                        //    //int pageSize = 10;
                        //    int maxpage = 0;

                        //    //campaignList = campaignList.Where(campaign => statusList.Contains(campaign.Status) && campaign.IsDeployedToIntegration).ToList();
                        //    //campaignIdList = campaignList.Select(c => c.PlanCampaignId).Distinct().ToList();
                        //    //if (campaignIdList.Count > 0)
                        //    //{
                        //    //    campaignIdList.Concat(campaignIdForTactic);
                        //    //}
                        //    //else
                        //    //{
                        //    //    campaignIdList = campaignIdForTactic;
                        //    //}
                        //    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997

                        //    programList = programList.Where(program => statusList.Contains(program.Status) && program.IsDeployedToIntegration).ToList();
                        //    programIdList = programList.Select(c => c.PlanProgramId).Distinct().ToList();
                        //    if (programIdList.Count > 0)
                        //    {
                        //        programIdList.Concat(programIdForTactic);
                        //    }
                        //    else
                        //    {
                        //        programIdList = programIdForTactic;
                        //    }

                        //    //if (programList.Count > 0 || tacticList.Count > 0)
                        //    //{
                        //    //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                        //    //    _isResultError = SetMappingDetails();
                        //    //    if (!_isResultError)
                        //    //    {
                        //    //        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                        //    //    }
                        //    //    else
                        //    //    {
                        //    //        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Error, "Set Mapping details.");
                        //    //        return;
                        //    //    }
                        //    //}
                        //    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //    #endregion

                        //    #region "Sync Program Data"
                        //    if (programList.Count > 0)
                        //    {
                        //        try
                        //        {
                        //            #region "Get Program Customfield list"
                        //            // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //            programIdList = programList.Select(c => c.PlanProgramId).ToList();

                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Program.");
                        //            var lstCustomFieldsprogram = CreateMappingCustomFieldDictionary(programIdList, Enums.EntityType.Program.ToString());
                        //            if (_mappingCustomFields == null)
                        //                _mappingCustomFields = new List<CustomFiledMapping>();
                        //            _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsprogram).ToList();
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Program.");

                        //            // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //            #endregion

                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "SyncProgramData process start.");
                        //            page = 0;
                        //            total = programList.Count;
                        //            maxpage = (total / pushRecordBatchSize);
                        //            List<Plan_Campaign_Program> lstPagedlistProgram = new List<Plan_Campaign_Program>();
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, "Total No. of Program: " + total);
                        //            while (page <= maxpage)
                        //            {
                        //                lstPagedlistProgram = new List<Plan_Campaign_Program>();
                        //                lstPagedlistProgram = programList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList();

                        //                sbMessage = new StringBuilder();

                        //                for (int index = 0; index < lstPagedlistProgram.Count; index++)
                        //                {
                        //                    lstPagedlistProgram[index] = SyncProgramData(lstPagedlistProgram[index], ref sbMessage);

                        //                    // Save 10 log records to Table.
                        //                    if (((index + 1) % logRecordSize) == 0)
                        //                    {
                        //                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //                        sbMessage = new StringBuilder();
                        //                    }
                        //                }

                        //                if (!string.IsNullOrEmpty(sbMessage.ToString()))
                        //                {
                        //                    // Save remaining log records to Table.
                        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //                }
                        //                db.SaveChanges();
                        //                page++;
                        //            }
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncProgramData process end.");
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            _isResultError = true;
                        //            string exMessage = Common.GetInnermostException(ex);
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing Program data to Salesforce: " + exMessage);
                        //        }
                        //    }
                        //    #endregion

                        //    #region "Sync Tactic Data"
                        //    if (tacticList.Count > 0)
                        //    {
                        //        try
                        //        {
                        //            #region "Get Tacic Customfield list & Actual Cost"
                        //            // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //            List<int> tacticIdList = tacticList.Select(c => c.PlanTacticId).Distinct().ToList();
                        //            _mappingTactic_ActualCost = Common.CalculateActualCostTacticslist(tacticIdList);

                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Tactic.");
                        //            var lstCustomFieldstactic = CreateMappingCustomFieldDictionary(tacticIdList, Enums.EntityType.Tactic.ToString());
                        //            if (_mappingCustomFields == null)
                        //                _mappingCustomFields = new List<CustomFiledMapping>();
                        //            _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldstactic).ToList();
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Tactic.");
                        //            // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //            #endregion

                        //            page = 0;
                        //            total = tacticList.Count;
                        //            maxpage = (total / pushRecordBatchSize);
                        //            List<Plan_Campaign_Program_Tactic> lstPagedlistTactic = new List<Plan_Campaign_Program_Tactic>();
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, "Total No. of Tactics: " + total);
                        //            Plan_Campaign_Program_Tactic srcTactic, objLinkedTactic;
                        //            while (page <= maxpage)
                        //            {
                        //                lstPagedlistTactic = new List<Plan_Campaign_Program_Tactic>();
                        //                lstPagedlistTactic = tacticList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList();

                        //                sbMessage = new StringBuilder();
                        //                for (int index = 0; index < lstPagedlistTactic.Count; index++)
                        //                {

                        //                    #region "Get Linked Tactic"
                        //                    int linkedTacticId = 0;
                        //                    srcTactic = new Plan_Campaign_Program_Tactic();
                        //                    objLinkedTactic = new Plan_Campaign_Program_Tactic();
                        //                    srcTactic = lstPagedlistTactic[index];
                        //                    if (srcTactic != null)
                        //                    {
                        //                        linkedTacticId = (srcTactic != null && srcTactic.LinkedTacticId.HasValue) ? srcTactic.LinkedTacticId.Value : 0;
                        //                        //if (linkedTacticId <= 0)
                        //                        //{
                        //                        //    objLinkedTactic = tblTactic.Where(tactic => tactic.LinkedTacticId == srcTactic.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                        //                        //    linkedTacticId = objLinkedTactic != null ? objLinkedTactic.PlanTacticId : 0;
                        //                        //}
                        //                    }
                        //                    if (linkedTacticId > 0)
                        //                    {
                        //                        objLinkedTactic = tblTactic.Where(tactic => tactic.PlanTacticId == linkedTacticId).FirstOrDefault();
                        //                    }
                        //                    #endregion

                        //                    lstPagedlistTactic[index] = SyncTacticData(lstPagedlistTactic[index], ref sbMessage, objLinkedTactic);

                        //                    // Save 10 log records to Table.
                        //                    if (((index + 1) % logRecordSize) == 0)
                        //                    {
                        //                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //                        sbMessage = new StringBuilder();
                        //                    }
                        //                }

                        //                if (!string.IsNullOrEmpty(sbMessage.ToString()))
                        //                {
                        //                    // Save remaining log records to Table.
                        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //                    sbMessage = new StringBuilder();
                        //                }
                        //                db.SaveChanges();
                        //                page++;
                        //            }
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncTacticData process end.");
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            _isResultError = true;
                        //            string exMessage = Common.GetInnermostException(ex);
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing Tactic data to Salesforce: " + exMessage);
                        //        }
                        //    }
                        //    #endregion

                        //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //    db.SaveChanges();
                        //}
                        //    else
                        //    {
                        //        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Error, "Set Mapping details.");
                        //    }
                        #endregion
                    }
                    else
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Program does not exist.");
                    }
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncProgramData process end.");
                }
                else if (EntityType.Campaign.Equals(_entityType))
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "SyncCampaingData process start.");
                    Plan_Campaign planCampaign = db.Plan_Campaign.Where(campaign => campaign.PlanCampaignId == _id && statusList.Contains(campaign.Status) && campaign.IsDeployedToIntegration && !campaign.IsDeleted).FirstOrDefault();
                    if (planCampaign != null)
                    {
                        SyncDatabyEntity(planCampaign);
                        #region "Old Code: commented on 03/23/2016"
                        //Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                        //_isResultError = SetMappingDetails();
                        //if (!_isResultError)
                        //{
                        //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");

                        //    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //    List<int> campaignIdList = new List<int>() { planCampaign.PlanCampaignId };
                        //    //_mappingCustomFields = CreateMappingCustomFieldDictionary(campaignIdList, Enums.EntityType.Campaign.ToString());
                        //    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //    //planCampaign = SyncCampaingData(planCampaign, ref sbMessage);

                        //    #region "Sync Campaign, Program, Tactic"

                        //    #region "Create CampaignIdList,ProgramIdList,TacticIdList"
                        //    List<Plan_Campaign> campaignList = new List<Plan_Campaign>();
                        //    campaignList.Add(planCampaign);
                        //    //List<int> campaignIdList = campaignList.Select(c => c.PlanCampaignId).ToList();
                        //    List<Plan_Campaign_Program> programList = db.Plan_Campaign_Program.Where(program => campaignIdList.Contains(program.PlanCampaignId) && !program.IsDeleted).ToList();
                        //    List<int> programIdList = programList.Select(c => c.PlanProgramId).ToList();
                        //    List<Plan_Campaign_Program_Tactic> tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).ToList();

                        //    List<Plan_Campaign_Program_Tactic> tacticList = tblTactic.Where(tactic => programIdList.Contains(tactic.PlanProgramId)).ToList();

                        //    List<int> campaignIdForTactic = tacticList.Where(tactic => string.IsNullOrWhiteSpace(tactic.Plan_Campaign_Program.Plan_Campaign.IntegrationInstanceCampaignId)).Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
                        //    List<int> programIdForTactic = tacticList.Where(tactic => string.IsNullOrWhiteSpace(tactic.Plan_Campaign_Program.IntegrationInstanceProgramId)).Select(tactic => tactic.PlanProgramId).ToList();

                        //    int page = 0;
                        //    int total = 0;
                        //    //int pageSize = 10;
                        //    int maxpage = 0;

                        //    campaignList = campaignList.Where(campaign => statusList.Contains(campaign.Status) && campaign.IsDeployedToIntegration).ToList();
                        //    campaignIdList = campaignList.Select(c => c.PlanCampaignId).Distinct().ToList();
                        //    if (campaignIdList.Count > 0)
                        //    {
                        //        campaignIdList.Concat(campaignIdForTactic);
                        //    }
                        //    else
                        //    {
                        //        campaignIdList = campaignIdForTactic;
                        //    }
                        //    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997

                        //    programList = programList.Where(program => statusList.Contains(program.Status) && program.IsDeployedToIntegration).ToList();
                        //    programIdList = programList.Select(c => c.PlanProgramId).Distinct().ToList();
                        //    if (programIdList.Count > 0)
                        //    {
                        //        programIdList.Concat(programIdForTactic);
                        //    }
                        //    else
                        //    {
                        //        programIdList = programIdForTactic;
                        //    }
                        //    #endregion

                        //    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997

                        //    #region "Sync Campaign Data"
                        //    if (campaignList.Count > 0)
                        //    {
                        //        try
                        //        {
                        //            #region "Get Campaign CustomFieldlist"
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Campaign.");
                        //_mappingCustomFields = CreateMappingCustomFieldDictionary(campaignIdList, Enums.EntityType.Campaign.ToString());
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Campaign.");

                        //            #endregion

                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "SyncCampaingData process start.");
                        //            page = 0;
                        //            total = campaignList.Count;
                        //            maxpage = (total / pushRecordBatchSize);
                        //            List<Plan_Campaign> lstPagedlistCampaign = new List<Plan_Campaign>();
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, "Total No. of Campaign: " + total);
                        //            while (page <= maxpage)
                        //            {
                        //                lstPagedlistCampaign = new List<Plan_Campaign>();
                        //                lstPagedlistCampaign = campaignList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList();

                        //                sbMessage = new StringBuilder();

                        //                for (int index = 0; index < lstPagedlistCampaign.Count; index++)
                        //                {
                        //                    lstPagedlistCampaign[index] = SyncCampaingData(lstPagedlistCampaign[index], ref sbMessage);

                        //                    // Save 10 log records to Table.
                        //                    if (((index + 1) % logRecordSize) == 0)
                        //                    {
                        //                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //                        sbMessage = new StringBuilder();
                        //                    }
                        //                }

                        //                if (!string.IsNullOrEmpty(sbMessage.ToString()))
                        //                {
                        //                    // Save remaining log records to Table.
                        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //                }
                        //                db.SaveChanges();
                        //                page++;
                        //            }
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncCampaingData process end.");
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            _isResultError = true;
                        //            string exMessage = Common.GetInnermostException(ex);
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing Campaign data to Salesforce: " + exMessage);
                        //        }
                        //    }
                        //    #endregion

                        //    #region "Sync Program Data"
                        //    if (programList.Count > 0)
                        //    {
                        //        try
                        //        {
                        //            #region "Get Program Customfield list"
                        //            // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //            programIdList = programList.Select(c => c.PlanProgramId).ToList();

                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Program.");
                        //            var lstCustomFieldsprogram = CreateMappingCustomFieldDictionary(programIdList, Enums.EntityType.Program.ToString());
                        //            if (_mappingCustomFields == null)
                        //                _mappingCustomFields = new List<CustomFiledMapping>();
                        //            _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsprogram).ToList();
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Program.");

                        //            // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //            #endregion

                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "SyncProgramData process start.");
                        //            page = 0;
                        //            total = programList.Count;
                        //            maxpage = (total / pushRecordBatchSize);
                        //            List<Plan_Campaign_Program> lstPagedlistProgram = new List<Plan_Campaign_Program>();
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, "Total No. of Program: " + total);
                        //            while (page <= maxpage)
                        //            {
                        //                lstPagedlistProgram = new List<Plan_Campaign_Program>();
                        //                lstPagedlistProgram = programList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList();

                        //                sbMessage = new StringBuilder();

                        //                for (int index = 0; index < lstPagedlistProgram.Count; index++)
                        //                {
                        //                    lstPagedlistProgram[index] = SyncProgramData(lstPagedlistProgram[index], ref sbMessage);

                        //                    // Save 10 log records to Table.
                        //                    if (((index + 1) % logRecordSize) == 0)
                        //                    {
                        //                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //                        sbMessage = new StringBuilder();
                        //                    }
                        //                }

                        //                if (!string.IsNullOrEmpty(sbMessage.ToString()))
                        //                {
                        //                    // Save remaining log records to Table.
                        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //                }
                        //                db.SaveChanges();
                        //                page++;
                        //            }
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncProgramData process end.");
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            _isResultError = true;
                        //            string exMessage = Common.GetInnermostException(ex);
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing Program data to Salesforce: " + exMessage);
                        //        }
                        //    }
                        //    #endregion

                        //    #region "Sync Tactic Data"
                        //    if (tacticList.Count > 0)
                        //    {
                        //        try
                        //        {
                        //            #region "Get Tacic Customfield list & Actual Cost"
                        //            // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //            List<int> tacticIdList = tacticList.Select(c => c.PlanTacticId).Distinct().ToList();
                        //            _mappingTactic_ActualCost = Common.CalculateActualCostTacticslist(tacticIdList);

                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Tactic.");
                        //            var lstCustomFieldstactic = CreateMappingCustomFieldDictionary(tacticIdList, Enums.EntityType.Tactic.ToString());
                        //            if (_mappingCustomFields == null)
                        //                _mappingCustomFields = new List<CustomFiledMapping>();
                        //            _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldstactic).ToList();
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Tactic.");
                        //            // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                        //            #endregion

                        //            page = 0;
                        //            total = tacticList.Count;
                        //            maxpage = (total / pushRecordBatchSize);
                        //            List<Plan_Campaign_Program_Tactic> lstPagedlistTactic = new List<Plan_Campaign_Program_Tactic>();
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, "Total No. of Tactics: " + total);
                        //            Plan_Campaign_Program_Tactic srcTactic, objLinkedTactic;
                        //            while (page <= maxpage)
                        //            {
                        //                lstPagedlistTactic = new List<Plan_Campaign_Program_Tactic>();
                        //                lstPagedlistTactic = tacticList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList();

                        //                sbMessage = new StringBuilder();
                        //                for (int index = 0; index < lstPagedlistTactic.Count; index++)
                        //                {
                        //                    #region "Get Linked Tactic"
                        //                    int linkedTacticId = 0;
                        //                    srcTactic = new Plan_Campaign_Program_Tactic();
                        //                    objLinkedTactic = new Plan_Campaign_Program_Tactic();
                        //                    srcTactic = lstPagedlistTactic[index];
                        //                    if (srcTactic != null)
                        //                    {
                        //                        linkedTacticId = (srcTactic != null && srcTactic.LinkedTacticId.HasValue) ? srcTactic.LinkedTacticId.Value : 0;
                        //                        //if (linkedTacticId <= 0)
                        //                        //{
                        //                        //    objLinkedTactic = tblTactic.Where(tactic => tactic.LinkedTacticId == srcTactic.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                        //                        //    linkedTacticId = objLinkedTactic != null ? objLinkedTactic.PlanTacticId : 0;
                        //                        //}
                        //                    }
                        //                    if (linkedTacticId > 0)
                        //                    {
                        //                        objLinkedTactic = tblTactic.Where(tactic => tactic.PlanTacticId == linkedTacticId).FirstOrDefault();
                        //                    }
                        //                    #endregion

                        //                    lstPagedlistTactic[index] = SyncTacticData(lstPagedlistTactic[index], ref sbMessage, objLinkedTactic);

                        //                    // Save 10 log records to Table.
                        //                    if (((index + 1) % logRecordSize) == 0)
                        //                    {
                        //                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //                        sbMessage = new StringBuilder();
                        //                    }
                        //                }

                        //                if (!string.IsNullOrEmpty(sbMessage.ToString()))
                        //                {
                        //                    // Save remaining log records to Table.
                        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //                    sbMessage = new StringBuilder();
                        //                }
                        //                db.SaveChanges();
                        //                page++;
                        //            }
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncTacticData process end.");
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            _isResultError = true;
                        //            string exMessage = Common.GetInnermostException(ex);
                        //            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing Tactic data to Salesforce: " + exMessage);
                        //        }
                        //    }
                        //    #endregion

                        //    #endregion

                        //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                        //    db.SaveChanges();
                        //}
                        //else
                        //{
                        //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Error, "Set Mapping details.");
                        //} 
                        #endregion
                    }
                    else
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Campaign does not exist.");
                    }
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncCampaingData process end.");
                }
                else if (EntityType.ImprovementTactic.Equals(_entityType))
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "SyncImprovementData process start.");
                    Plan_Improvement_Campaign_Program_Tactic planImprovementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(imptactic => imptactic.ImprovementPlanTacticId == _id && statusList.Contains(imptactic.Status) && imptactic.IsDeployedToIntegration && !imptactic.IsDeleted).FirstOrDefault();
                    if (planImprovementTactic != null)
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                        _isResultError = SetMappingDetails();
                        if (!_isResultError)
                        {
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");

                            planImprovementTactic = SyncImprovementData(planImprovementTactic, ref sbMessage);
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                            db.SaveChanges();
                        }
                        else
                        {
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Error, "Set Mapping details.");
                        }
                    }
                    else
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "ImprovementTactic does not exist.");
                    }
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncImprovementData process end.");
                }
                else
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Syncing process with multiple tactic.");
                    IsInstanceSync = true;
                    //SyncInstanceData();
                    SyncDatabyEntity();
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Syncing process with multiple tactic.");
                }
            }
            catch (Exception e)
            {
                string exMessage = Common.GetInnermostException(e);
                _isResultError = true;
                _ErrorMessage = exMessage;
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while syncing Tactic :- " + exMessage);
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
                IntegrationInstance objInstance = new IntegrationInstance();
                objInstance = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId.Equals(_integrationInstanceId));
                isImport = objInstance != null ? objInstance.IsImportActuals : false;
                if (isImport)
                {
                    //GetDataForTacticandUpdate();  // Commented by Sohel Pathan on 12/09/2014 for PL ticket #773
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Pulling Response execution start.");
                    PullingResponses();
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Pulling Response execution end.");

                    #region "Pull MQL based on client level MQL permission for SFDC"
                    // Pulling MQL from SFDC based on client level MQL permission for SFDC.
                    string strPermissionCode_MQL = Enums.ClientIntegrationPermissionCode.MQL.ToString();
                    if (db.Client_Integration_Permission.Any(intPermission => (intPermission.ClientId.Equals(_clientId)) && (intPermission.IntegrationTypeId.Equals(objInstance.IntegrationTypeId)) && (intPermission.PermissionCode.ToUpper().Equals(strPermissionCode_MQL.ToUpper()))))
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Pulling MQL execution start.");
                        PullingMQL();
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Pulling MQL execution end.");
                    }
                    #endregion

                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Pulling CWRevenue execution start.");
                    PullingCWRevenue();
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Pulling CWRevenue execution end.");
                }
                else
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pulling Data does not procceed due to IsImportActuals field not enabled for this Integration Instance.");
                }
            }
            lstSyncError.AddRange(_lstSyncError);
            return _isResultError;
        }

        private class CampaignMember
        {
            public string CampaignId { get; set; }
            public DateTime FirstRespondedDate { get; set; }
        }

        private void PullingResponses()
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string published = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString();
            string EloquaCode = Enums.IntegrationType.Eloqua.ToString();
            try
            {
                // Insert log into IntegrationInstanceSection, Dharmraj PL#684
                int IntegrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), DateTime.Now, _userId);

                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Creating Eloqua & Model IntegrationInstanceId mapping list start.");
                //List<Plan> lstPlans = db.Plans.Where(p => p.Model.IntegrationInstanceIdINQ == _integrationInstanceId && p.Model.Status.Equals(published)).ToList();

                #region "Check for pulling Responses whether Model/Plan associated or not with current Instance"
                List<Model> lstModels = db.Models.Where(objmdl => objmdl.IntegrationInstanceIdINQ == _integrationInstanceId && objmdl.Status.Equals("Published") && objmdl.IsActive == true).ToList();
                if (lstModels == null || lstModels.Count <= 0)
                {
                    // Save & display Message: No single Model associated with current Instance.
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Responses: There is no single Model associated with this Instance to pull Responses.", Enums.SyncStatus.Info, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull Responses: There is no single Model associated with this Instance to pull Responses.");
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    return; // no error.
                }
                List<int> ModelIds = lstModels.Select(mdl => mdl.ModelId).ToList();
                List<Plan> lstPlans = db.Plans.Where(objplan => ModelIds.Contains(objplan.Model.ModelId) && objplan.IsActive == true).ToList();
                if (lstPlans == null || lstPlans.Count <= 0)
                {
                    // Save & display Message: No single Plan associated with current Instance.
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Responses: There is no single Plan associated with this Instance to pull Responses.", Enums.SyncStatus.Info, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull Responses: There is no single Plan associated with this Instance to pull Responses.");
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    return; // no error.
                }
                #endregion

                Guid ClientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;

                //// Get Eloqua integration type Id.
                var eloquaIntegrationType = db.IntegrationTypes.Where(type => type.Code == EloquaCode && type.IsDeleted == false).Select(type => type.IntegrationTypeId).FirstOrDefault();
                int eloquaIntegrationTypeId = Convert.ToInt32(eloquaIntegrationType);

                //// Get All EloquaIntegrationTypeIds to retrieve  EloquaPlanIds.
                List<int> lstEloquaIntegrationTypeIds = db.IntegrationInstances.Where(instance => instance.IntegrationTypeId.Equals(eloquaIntegrationTypeId) && instance.IsDeleted.Equals(false) && instance.ClientId.Equals(ClientId)).Select(s => s.IntegrationInstanceId).ToList();

                //// Get all PlanIds whose Tactic data PUSH on Eloqua.
                //List<int> lstEloquaPlanIds = lstPlans.Where(objplan => lstEloquaIntegrationTypeIds.Contains(objplan.Model.IntegrationInstanceId.Value)).Select(objplan => objplan.PlanId).ToList();

                //// Get All PlanIds.
                List<int> AllplanIds = lstPlans.Select(objplan => objplan.PlanId).ToList();

                //// Get SalesForce PlanIds.
                //  List<int> lstSalesForceplanIds = lstPlans.Where(objplan => !lstEloquaPlanIds.Contains(objplan.PlanId)).Select(plan => plan.PlanId).ToList();

                int INQStageId = db.Stages.FirstOrDefault(s => s.ClientId == ClientId && s.Code == Common.StageINQ && s.IsDeleted == false).StageId;
                // Get List of status after Approved Status
                List<string> statusList = Common.GetStatusListAfterApproved();

                List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeployedToIntegration == true &&
                                                                                                               statusList.Contains(tactic.Status) &&
                                                                                                               tactic.StageId == INQStageId &&
                                                                                                               tactic.IsDeleted == false && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).ToList();

                //// Get All Approved,IsDeployedToIntegration true and IsDeleted false Tactic list.
                List<Plan_Campaign_Program_Tactic> lstAllTactics = tblPlanTactic.Where(tactic => AllplanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).ToList();

                //// Get list of EloquaIntegrationInstanceTacticID(EloquaId).
                List<EloquaIntegrationInstanceTactic_Model_Mapping> lstEloquaIntegrationInstanceTacticIds = lstAllTactics.Where(tactic => string.IsNullOrEmpty(tactic.IntegrationInstanceTacticId) && !string.IsNullOrEmpty(tactic.IntegrationInstanceEloquaId)).Select(_tac => new EloquaIntegrationInstanceTactic_Model_Mapping
                                                                                                                                                                                                                                                                                         {
                                                                                                                                                                                                                                                                                             EloquaIntegrationInstanceTacticId = _tac.IntegrationInstanceEloquaId,
                                                                                                                                                                                                                                                                                             ModelIntegrationInstanceId = _tac.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceEloquaId.Value,
                                                                                                                                                                                                                                                                                             PlanTacticId = _tac.PlanTacticId
                                                                                                                                                                                                                                                                                         }).ToList();
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Creating Eloqua & Model IntegrationInstanceId mapping list end.");

                if (lstEloquaIntegrationInstanceTacticIds == null)
                    lstEloquaIntegrationInstanceTacticIds = new List<EloquaIntegrationInstanceTactic_Model_Mapping>();

                //// Add IntegrationEloquaClient object to Mapping list for distinct ModelIntegrationInstanceId.
                //// Get Mapping List of SalesForceIntegrationInstanceTactic Ids(CRMIds) based on EloquaIntegrationInstanceTacticID(EloquaId).
                //List<CRM_EloquaMapping> lstSalesForceIntegrationInstanceTacticIds = new List<CRM_EloquaMapping>();
                Integration.Eloqua.EloquaCampaign objEloqua;
                Plan_Campaign_Program_Tactic objUpdTactic;
                int cntrUpdate = 0;
                Integration.Eloqua.IntegrationEloquaClient integrationEloquaClient;
                foreach (int _ModelIntegrationInstanceId in lstEloquaIntegrationInstanceTacticIds.Select(tac => tac.ModelIntegrationInstanceId).Distinct())
                {
                    try
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Authenticate Eloqua Instance for mapping Salesforce & Eloqua.");
                        integrationEloquaClient = new Integration.Eloqua.IntegrationEloquaClient(_ModelIntegrationInstanceId, 0, _entityType, _userId, _integrationInstanceLogId, _applicationId);
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Authenticate Eloqua Instance for mapping Salesforce & Eloqua.");
                        foreach (EloquaIntegrationInstanceTactic_Model_Mapping _EloquaTac in lstEloquaIntegrationInstanceTacticIds.Where(_eloqua => _eloqua.ModelIntegrationInstanceId.Equals(_ModelIntegrationInstanceId)))
                        {
                            //objEloqua.IntegrationEloquaClient = integrationEloquaClient;
                            if (!string.IsNullOrEmpty(_EloquaTac.EloquaIntegrationInstanceTacticId))
                            {

                                objEloqua = new Integration.Eloqua.EloquaCampaign();

                                ////Get SalesForceIntegrationTacticId based on EloquaIntegrationTacticId.
                                objEloqua = integrationEloquaClient.GetEloquaCampaign(_EloquaTac.EloquaIntegrationInstanceTacticId);
                                if (objEloqua != null && !string.IsNullOrEmpty(objEloqua.crmId))
                                {
                                    objUpdTactic = new Plan_Campaign_Program_Tactic();
                                    objUpdTactic = lstAllTactics.Where(s => s.PlanTacticId == _EloquaTac.PlanTacticId).FirstOrDefault();
                                    if (objUpdTactic != null)
                                    {
                                        objUpdTactic.IntegrationInstanceTacticId = objEloqua.crmId;
                                        db.Entry(objUpdTactic).State = EntityState.Modified;
                                        cntrUpdate++;
                                    }
                                    //lstSalesForceIntegrationInstanceTacticIds.Add(
                                    //                                          new CRM_EloquaMapping
                                    //                                          {
                                    //                                              CRMId = !string.IsNullOrEmpty(objEloqua.crmId) ? objEloqua.crmId : string.Empty,
                                    //                                              EloquaId = _EloquaTac.EloquaIntegrationInstanceTacticId,
                                    //                                              PlanTacticId = _Tactic != null ? _Tactic.PlanTacticId : 0,
                                    //                                              StartDate = _Tactic != null ? _Tactic.StartDate : (new DateTime()),
                                    //                                          });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _isResultError = true;
                        string exMessage = Common.GetInnermostException(ex);
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Responses: System error occurred while pulling EloquaId for Salesforce and Eloqua mapping. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while create mapping list of EloquaId and SalesforceId. Exception - " + exMessage);
                        continue;
                    }
                }
                if (cntrUpdate > 0)
                    db.SaveChanges();
                //// Get SalesForce tactic list
                List<Plan_Campaign_Program_Tactic> lstSalesForceTactic = lstAllTactics.Where(_tac => _tac.IntegrationInstanceTacticId != null).ToList();
                // Add By Nishant Sheth
                #region Plan To Salesforce Push and pull process for marketo instance
                List<int> MarketoModel = lstModels.Where(model => model.IntegrationInstanceMarketoID != null).Select(model => model.ModelId).ToList(); // get list of model which have marketo instance
                List<Plan_Campaign_Program_Tactic> marketoTactic = new List<Plan_Campaign_Program_Tactic>();
                if (MarketoModel.Count > 0)
                {
                    marketoTactic = SyncSFDCMarketo(MarketoModel, IntegrationInstanceSectionId, INQStageId);// Modified By Nishant Sheth for add stage level
                }
                if (marketoTactic.Count > 0)
                {
                    lstSalesForceTactic.AddRange(marketoTactic);
                }
                #endregion
                if (lstSalesForceTactic.Count() > 0)
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
                                int TotalPullResonsesCount = 0, ProcessedResponsesCount = 0;
                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Getting CampaignList from CampaignMember Table within Salesforce start.");

                                List<CampaignMember> CampaignMemberList = new List<CampaignMember>();
                                int pagesize = 400;
                                int pagecount = 0;
                                int totalrecords = lstSalesForceTactic.Count();
                                int maxpage = (totalrecords / pagesize);
                                while (pagecount <= maxpage)
                                {
                                    string AllIntegrationTacticIds = String.Join("','", lstSalesForceTactic.Skip(pagecount * pagesize).Take(pagesize).Select(tactic => tactic.IntegrationInstanceTacticId).ToList());
                                    AllIntegrationTacticIds = AllIntegrationTacticIds.Trim(new char[] { ',' });
                                    var responsePull = _client.Query<object>("SELECT " + CampaignId + "," + FirstRespondedDate + " FROM CampaignMember WHERE " + CampaignId + " IN ('" + AllIntegrationTacticIds + "') AND " + Status + "= '" + Common.Responded + "'");

                                    #region "Declare local variables"
                                    int _PlanTacticId = 0;
                                    CampaignMember objCampaign;
                                    JObject jobj;
                                    string TacticResult = string.Empty, campaignid = string.Empty;
                                    #endregion

                                    foreach (var resultin in responsePull)
                                    {
                                        #region "Initialize local variables"
                                        TacticResult = resultin.ToString();
                                        jobj = JObject.Parse(TacticResult);
                                        objCampaign = new CampaignMember();
                                        _PlanTacticId = 0;
                                        campaignid = string.Empty;
                                        #endregion

                                        try
                                        {

                                            campaignid = Convert.ToString(jobj[CampaignId]);
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
                                            string exMessage = Common.GetInnermostException(e);
                                            ErrorFlag = true;
                                            _ErrorMessage = exMessage;
                                            string TacticId = Convert.ToString(jobj[CampaignId]); ////CRMId

                                            //// check whether TacticId(CRMId) exist in field IntegrationInstanceTacticID field of SalesForceTactic list.
                                            var tactic = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
                                            if (tactic != null)
                                                _PlanTacticId = tactic.PlanTacticId;
                                            //else                                        //// if Tactic not exist then retrieve PlanTacticId from EloquaTactic list.
                                            //    _PlanTacticId = lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && _SalTac.CRMId.Equals(TacticId)).Select(s => s.PlanTacticId).FirstOrDefault();

                                            IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                            instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                            instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                            instanceTactic.EntityId = _PlanTacticId;
                                            instanceTactic.EntityType = EntityType.Tactic.ToString();
                                            instanceTactic.Status = StatusResult.Error.ToString();
                                            instanceTactic.Operation = Operation.Pull_Responses.ToString();
                                            instanceTactic.SyncTimeStamp = DateTime.Now;
                                            instanceTactic.CreatedDate = DateTime.Now;
                                            instanceTactic.ErrorDescription = exMessage;
                                            instanceTactic.CreatedBy = _userId;
                                            db.Entry(instanceTactic).State = EntityState.Added;
                                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting Campaign from Salesforce. Exception - " + exMessage);
                                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Responses: Error occurred while getting Campaign from Salesforce. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                            _isResultError = true;
                                        }
                                        catch (Exception e)
                                        {
                                            string exMessage = Common.GetInnermostException(e);
                                            ErrorFlag = true;
                                            _ErrorMessage = exMessage;
                                            string TacticId = Convert.ToString(jobj[CampaignId]); ////CRMId

                                            //// check whether TacticId(CRMId) exist in field IntegrationInstanceTacticID field of SalesForceTactic list.
                                            var tactic = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
                                            if (tactic != null)
                                                _PlanTacticId = tactic.PlanTacticId;
                                            //else                                        //// if Tactic not exist then retrieve PlanTacticId from EloquaTactic list.
                                            //    _PlanTacticId = lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && _SalTac.CRMId.Equals(TacticId)).Select(s => s.PlanTacticId).FirstOrDefault();

                                            IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                            instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                            instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                            instanceTactic.EntityId = _PlanTacticId;
                                            instanceTactic.EntityType = EntityType.Tactic.ToString();
                                            instanceTactic.Status = StatusResult.Error.ToString();
                                            instanceTactic.Operation = Operation.Pull_Responses.ToString();
                                            instanceTactic.SyncTimeStamp = DateTime.Now;
                                            instanceTactic.CreatedDate = DateTime.Now;
                                            instanceTactic.ErrorDescription = exMessage;
                                            instanceTactic.CreatedBy = _userId;
                                            db.Entry(instanceTactic).State = EntityState.Added;
                                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting Campaign from Salesforce. Exception - " + exMessage);
                                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Responses: Error occurred while getting Campaign from Salesforce. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                            _isResultError = true;
                                        }
                                    }
                                    pagecount++;
                                }
                                //List<Plan_Campaign_Program_Tactic> lstCRM_EloquaTactics = (from _Tactic in lstEloquaTactic
                                //                                                           join _ElqTactic in lstSalesForceIntegrationInstanceTacticIds on _Tactic.IntegrationInstanceEloquaId equals _ElqTactic.EloquaId
                                //                                                           where _ElqTactic.CRMId != null
                                //                                                           select _Tactic).ToList();

                                List<Plan_Campaign_Program_Tactic> lstMergedTactics = lstSalesForceTactic;
                                //lstCRM_EloquaTactics.ForEach(_elqTactic => lstMergedTactics.Add(_elqTactic));

                                lstMergedTactics = lstMergedTactics.Distinct().ToList();
                                List<int> OuterTacticIds = lstMergedTactics.Select(t => t.PlanTacticId).ToList();

                                //Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Removing ActualTactic start.");
                                //List<Plan_Campaign_Program_Tactic_Actual> OuteractualTacticList = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => OuterTacticIds.Contains(actual.PlanTacticId) && actual.StageTitle == Common.StageProjectedStageValue).ToList();
                                //OuteractualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                //db.SaveChanges();
                                //Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Removing ActualTactic end.");
                                List<int> linkedTactics = new List<int>();
                                List<int> lstMultiLinkedTactic = new List<int>();
                                if (CampaignMemberList.Count > 0)
                                {
                                    #region "Remove Actuals values"

                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Create LinkedTactic mapping list start.");
                                    #region "Create LinkedTactic Mapping list"
                                    Dictionary<int, int> lstlinkedTacticMapping = new Dictionary<int, int>();
                                    int linkedTacticId;
                                    foreach (var tac in lstMergedTactics)
                                    {
                                        linkedTacticId = 0;
                                        #region "Retrieve linkedTactic"
                                        linkedTacticId = (tac != null && tac.LinkedTacticId.HasValue) ? tac.LinkedTacticId.Value : 0;
                                        //if (linkedTacticId <= 0)
                                        //{
                                        //    var lnkPCPT = tblPlanTactic.Where(tactic => tactic.LinkedTacticId == tac.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                                        //    linkedTacticId = lnkPCPT != null ? lnkPCPT.PlanTacticId : 0;
                                        //}

                                        if (linkedTacticId > 0)
                                        {
                                            lstlinkedTacticMapping.Add(tac.PlanTacticId, linkedTacticId);
                                        }
                                        #endregion
                                    }
                                    #endregion
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Create LinkedTactic mapping list end.");

                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Removing ActualTactic start.");


                                    if (lstlinkedTacticMapping.Count > 0)
                                    {
                                        linkedTactics = lstlinkedTacticMapping.Select(lnkdTac => lnkdTac.Value).ToList();
                                    }

                                    List<Plan_Campaign_Program_Tactic_Actual> tblActuals = new List<Plan_Campaign_Program_Tactic_Actual>();
                                    if (linkedTactics != null && linkedTactics.Count > 0)
                                    {
                                        tblActuals = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => (OuterTacticIds.Contains(actual.PlanTacticId) || linkedTactics.Contains(actual.PlanTacticId)) && actual.StageTitle == Common.StageProjectedStageValue).ToList();
                                    }
                                    else
                                    {
                                        tblActuals = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => OuterTacticIds.Contains(actual.PlanTacticId) && actual.StageTitle == Common.StageProjectedStageValue).ToList();
                                    }
                                    //Modification Start Viral 18Feb2016 #2006 H9_QA - SFDC integration bug
                                    //List<Plan_Campaign_Program_Tactic_Actual> OuteractualTacticList = tblActuals.Where(actual => OuterTacticIds.Contains(actual.PlanTacticId)).ToList();
                                    //OuteractualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                    if (tblActuals != null && tblActuals.Count > 0)
                                    {
                                        try
                                        {
                                            db.Configuration.AutoDetectChangesEnabled = false;
                                            List<Plan_Campaign_Program_Tactic_Actual> OuteractualTacticList = tblActuals.Where(actual => OuterTacticIds.Contains(actual.PlanTacticId)).ToList();
                                            OuteractualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                        }
                                        finally
                                        {
                                            db.Configuration.AutoDetectChangesEnabled = true;
                                        }
                                        //Modification End Viral 18Feb2016 #2006 H9_QA - SFDC integration bug
                                        // Remove linked Tactic's Actuals.

                                        if (linkedTactics.Count > 0)
                                        {

                                            #region "Get list of linked Tactics that multiyear or not"
                                            Plan_Campaign_Program_Tactic objLnkTac = null;
                                            int yeardiff = 0, cntr = 0, perdNum = 12; bool isMultilinkedTactic = false;
                                            List<Plan_Campaign_Program_Tactic_Actual> linkedactualTacticList = null;
                                            List<string> lstLinkedPeriods = new List<string>();
                                            foreach (int linkdTacId in linkedTactics)
                                            {
                                                try
                                                {
                                                    objLnkTac = new Plan_Campaign_Program_Tactic();
                                                    objLnkTac = tblPlanTactic.Where(tac => tac.PlanTacticId == linkdTacId).FirstOrDefault();
                                                    if (objLnkTac != null)
                                                    {
                                                        yeardiff = objLnkTac.EndDate.Year - objLnkTac.StartDate.Year;
                                                        isMultilinkedTactic = yeardiff > 0 ? true : false;
                                                        linkedactualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                                                        if (isMultilinkedTactic)
                                                        {
                                                            // remove linked tactic respective months actuals data.
                                                            lstLinkedPeriods = new List<string>();
                                                            cntr = 12 * yeardiff;
                                                            for (int i = 1; i <= cntr; i++)
                                                            {
                                                                lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
                                                            }
                                                            lstMultiLinkedTactic.Add(linkdTacId);
                                                            linkedactualTacticList = tblActuals.Where(actual => linkdTacId == actual.PlanTacticId && lstLinkedPeriods.Contains(actual.Period)).ToList();
                                                            if (linkedactualTacticList != null && linkedactualTacticList.Count > 0)
                                                            {
                                                                try
                                                                {
                                                                    db.Configuration.AutoDetectChangesEnabled = false;
                                                                    linkedactualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                                                }
                                                                finally
                                                                {
                                                                    db.Configuration.AutoDetectChangesEnabled = true;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            linkedactualTacticList = tblActuals.Where(actual => linkdTacId == actual.PlanTacticId).ToList();
                                                            if (linkedactualTacticList != null && linkedactualTacticList.Count > 0)
                                                            {
                                                                try
                                                                {
                                                                    db.Configuration.AutoDetectChangesEnabled = false;
                                                                    linkedactualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                                                }
                                                                finally
                                                                {
                                                                    db.Configuration.AutoDetectChangesEnabled = true;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Error occurred on remove linked tactic : Linked TacticId-" + linkdTacId.ToString() + Common.GetInnermostException(ex));
                                                }
                                            }

                                            #endregion

                                            //List<Plan_Campaign_Program_Tactic_Actual> linkedactualTacticList = tblActuals.Where(actual => linkedTactics.Contains(actual.PlanTacticId)).ToList();
                                            //linkedactualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                        }

                                        db.SaveChanges();
                                    }
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Removing ActualTactic end.");
                                    #endregion

                                    lstSalesForceTactic = lstSalesForceTactic.OrderBy(tac => tac.PlanTacticId).ToList(); // Add By Nishant Sheth // For pull inq, mql, cw #2188
                                    var CampaignMemberListGroup = CampaignMemberList.GroupBy(cl => new { CampaignId = cl.CampaignId, Month = cl.FirstRespondedDate.ToString("MM/yyyy") }).Select(cl =>
                                        new
                                        {
                                            CampaignId = cl.Key.CampaignId,
                                            TacticId = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null ? (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).PlanTacticId) : 0,
                                            //Period = "Y" + Convert.ToDateTime(cl.Key.Month).Month,
                                            //IsYear = (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year == Convert.ToDateTime(cl.Key.Month).Year)) ? true : false,
                                            Period = (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null) && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year < Convert.ToDateTime(cl.Key.Month).Year) ? "Y" + (((Convert.ToDateTime(cl.Key.Month).Year - (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year)) * 12) + Convert.ToDateTime(cl.Key.Month).Month) : "Y" + Convert.ToDateTime(cl.Key.Month).Month,
                                            IsYear = (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year <= Convert.ToDateTime(cl.Key.Month).Year) && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).EndDate.Year >= Convert.ToDateTime(cl.Key.Month).Year)) ? true : false,
                                            Count = cl.Count()
                                        }).Where(cm => cm.IsYear).ToList();

                                    //Set count of total pulled responses from Salesforce.
                                    TotalPullResonsesCount = CampaignMemberListGroup != null ? CampaignMemberListGroup.Sum(cnt => cnt.Count) : 0;

                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Create PlanTacticActual list and insert Tactic log.");

                                    int linkedTacId = 0;
                                    Plan_Campaign_Program_Tactic objLinkedTactic = null;
                                    bool isMultiYearlinkedTactic = false;
                                    int yearDiff = 0;

                                    try
                                    {
                                        db.Configuration.AutoDetectChangesEnabled = false;
                                        foreach (var tactic in lstMergedTactics)
                                        {
                                            try
                                            {
                                                var innerCampaignMember = CampaignMemberListGroup.Where(cm => cm.TacticId == tactic.PlanTacticId).ToList();
                                                if (linkedTactics != null && linkedTactics.Count > 0) // check whether linkedTactics exist or not.
                                                    linkedTacId = lstlinkedTacticMapping.FirstOrDefault(tac => tac.Key == tactic.PlanTacticId).Value;
                                                if (linkedTacId > 0) // check whether linkedTactics exist or not.
                                                {
                                                    objLinkedTactic = new Plan_Campaign_Program_Tactic();
                                                    objLinkedTactic = tblPlanTactic.Where(tac => tac.PlanTacticId == linkedTacId).FirstOrDefault();
                                                }
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
                                                    ProcessedResponsesCount = ProcessedResponsesCount + objCampaignMember.Count;

                                                    // Add linked Tacitc Actual Values.
                                                    if (linkedTacId > 0 && objLinkedTactic != null) // check whether linkedTactics exist or not.
                                                    {
                                                        yearDiff = objLinkedTactic.EndDate.Year - objLinkedTactic.StartDate.Year;
                                                        isMultiYearlinkedTactic = yearDiff > 0 ? true : false;

                                                        string orgPeriod = objCampaignMember.Period;
                                                        string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                        int NumPeriod = int.Parse(numPeriod);
                                                        if (isMultiYearlinkedTactic)
                                                        {
                                                            Plan_Campaign_Program_Tactic_Actual objLinkedTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                                            objLinkedTacticActual.PlanTacticId = linkedTacId;           // LinkedTactic Id.
                                                            objLinkedTacticActual.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                                            objLinkedTacticActual.StageTitle = Common.StageProjectedStageValue;
                                                            objLinkedTacticActual.Actualvalue = objCampaignMember.Count;
                                                            objLinkedTacticActual.CreatedBy = _userId;
                                                            objLinkedTacticActual.CreatedDate = DateTime.Now;
                                                            db.Entry(objLinkedTacticActual).State = EntityState.Added;
                                                        }
                                                        else
                                                        {
                                                            if (NumPeriod > 12)
                                                            {
                                                                int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
                                                                int div = NumPeriod / 12;    // In case of 24, Y12.
                                                                if (rem > 0 || div > 1)
                                                                {
                                                                    Plan_Campaign_Program_Tactic_Actual objLinkedTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                                                    objLinkedTacticActual.PlanTacticId = linkedTacId;           // LinkedTactic Id.
                                                                    objLinkedTacticActual.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());                            // For March, Y3(i.e 15%12 = 3)     
                                                                    objLinkedTacticActual.StageTitle = Common.StageProjectedStageValue;
                                                                    objLinkedTacticActual.Actualvalue = objCampaignMember.Count;
                                                                    objLinkedTacticActual.CreatedBy = _userId;
                                                                    objLinkedTacticActual.CreatedDate = DateTime.Now;
                                                                    db.Entry(objLinkedTacticActual).State = EntityState.Added;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                tactic.LastSyncDate = DateTime.Now;
                                                //tactic.ModifiedDate = DateTime.Now;
                                                tactic.ModifiedBy = _userId;

                                                // Update linked Tactic lastSync Date,ModifiedDate & ModifiedBy.
                                                if (linkedTacId > 0 && objLinkedTactic != null) // check whether linkedTactics exist or not.
                                                {

                                                    objLinkedTactic.LastSyncDate = DateTime.Now;
                                                    //objLinkedTactic.ModifiedDate = DateTime.Now;
                                                    objLinkedTactic.ModifiedBy = _userId;
                                                }

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
                                            catch (Exception ex)
                                            {
                                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Error occurred on insert Response on Actual table : TacticId-" + tactic.PlanTacticId.ToString() + Common.GetInnermostException(ex));
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        db.Configuration.AutoDetectChangesEnabled = true;
                                    }

                                    db.SaveChanges();
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Create PlanTacticActual list and insert Tactic log.");
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
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull Responses: Total Contact(s) - " + TotalPullResonsesCount.ToString() + ", " + ProcessedResponsesCount.ToString() + " contact(s) were processed and pulled in database.");
                                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Responses: Total Contact(s) - " + TotalPullResonsesCount.ToString() + ", " + ProcessedResponsesCount.ToString() + " contact(s) were processed and pulled in database.", Enums.SyncStatus.Info, DateTime.Now));
                                }
                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Getting CampaignList from CampaignMember Table within Salesforce end.");
                            }
                            else
                            {
                                _isResultError = true;
                                // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgMappingNotFoundForSalesforcePullResponse);
                                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Responses: " + Common.msgMappingNotFoundForSalesforcePullResponse, Enums.SyncStatus.Error, DateTime.Now));
                            }
                        }
                        else
                        {
                            // Update IntegrationInstanceSection log with Error status, modified by Mitesh Vaishnav for internal review point on 07-07-2015
                            _isResultError = true;
                            Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgMappingNotFoundForSalesforcePullResponse);
                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Responses: " + Common.msgMappingNotFoundForSalesforcePullResponse, Enums.SyncStatus.Error, DateTime.Now));
                        }
                    }
                    catch (SalesforceException e)
                    {
                        _isResultError = true;
                        string exMessage = Common.GetInnermostException(e);
                        // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, exMessage);
                        //Common.SaveIntegrationInstanceLogDetails(_id, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while Pulling Campaign from Salesforce:- " + e.Message);
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Responses: Error occurred while pulling campaign from Salesforce. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    }
                }
                else
                {
                    // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                }
            }
            catch (Exception ex)
            {
                _isResultError = true;
                string exMessage = Common.GetInnermostException(ex);
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Error occurred while Pulling Resoponses from Salesforce. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while Pulling Resoponses from Salesforce. Exception -" + exMessage);
            }

        }

        /// <summary>
        /// Tactic moved to another program then update program into integration system
        /// Created by : Mitesh Vaishnav
        /// </summary>
        /// <returns>If success then true else flase</returns>
        public bool SyncMovedTacticData()
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {

                _integrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), DateTime.Now, _userId);
                _isResultError = false;

                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                _isResultError = SetMappingDetails();
                if (!_isResultError)
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");

                    Plan_Campaign_Program_Tactic planTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == _id).FirstOrDefault();
                    Plan_Campaign_Program planProgram = planTactic.Plan_Campaign_Program;
                    _parentId = planProgram.IntegrationInstanceProgramId;
                    if (string.IsNullOrWhiteSpace(_parentId))
                    {
                        Plan_Campaign planCampaign = planTactic.Plan_Campaign_Program.Plan_Campaign;
                        _parentId = planCampaign.IntegrationInstanceCampaignId;
                        if (string.IsNullOrWhiteSpace(_parentId))
                        {
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Campaign creation process start.");
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
                                        _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsCampaign).ToList();
                                    }
                                    else
                                    {
                                        //lstCustomFieldsCampaign.ToList().ForEach(c=>_mappingCustomFields.Add(c.Key,c.Value));
                                        _mappingCustomFields = lstCustomFieldsCampaign;
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
                                string exMessage = Common.GetInnermostException(e);
                                _isResultError = true;
                                instanceLogCampaign.Status = StatusResult.Error.ToString();
                                instanceLogCampaign.ErrorDescription = exMessage;
                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while creating Campaign:- " + exMessage);
                            }
                            instanceLogCampaign.CreatedBy = this._userId;
                            instanceLogCampaign.CreatedDate = DateTime.Now;
                            db.Entry(instanceLogCampaign).State = EntityState.Added;
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Campaign creation process end.");
                        }

                        if (!string.IsNullOrWhiteSpace(_parentId))
                        {
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Program creation process start.");
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
                                        _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsProgram).ToList();
                                    }
                                    else
                                    {
                                        //lstCustomFieldsProgram.ToList().ForEach(c=>_mappingCustomFields.Add(c.Key,c.Value));
                                        _mappingCustomFields = lstCustomFieldsProgram;
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
                                string exMessage = Common.GetInnermostException(e);
                                _isResultError = true;
                                instanceLogProgram.Status = StatusResult.Error.ToString();
                                instanceLogProgram.ErrorDescription = exMessage;
                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while creating Program:- " + exMessage);
                            }

                            instanceLogProgram.CreatedBy = this._userId;
                            instanceLogProgram.CreatedDate = DateTime.Now;
                            db.Entry(instanceLogProgram).State = EntityState.Added;
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Program creation process end.");
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(_parentId))
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Tactic creation process start.");
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
                            string exMessage = Common.GetInnermostException(e);
                            _isResultError = true;
                            instanceLogTactic.Status = StatusResult.Error.ToString();
                            instanceLogTactic.ErrorDescription = exMessage;
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while creating Tactic:- " + exMessage);
                        }

                        instanceLogTactic.CreatedBy = this._userId;
                        instanceLogTactic.CreatedDate = DateTime.Now;
                        db.Entry(instanceLogTactic).State = EntityState.Added;
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Tactic creation process end.");
                    }
                }
                else
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Error, "Set Mapping details.");
                }
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while SyncMovedTacticData function executing:- " + exMessage);
            }
            return _isResultError;
        }

        /// <summary>
        /// Pulling MQL from SFDC and dumped to Actual table
        /// Created by : Viral Kadiya
        /// </summary>
        private void PullingMQL()
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string published = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString();
            string EloquaCode = Enums.IntegrationType.Eloqua.ToString();
            string MQLtitle = string.Empty;
            int MQLStageId = 0, MQLLevel = 0;
            try
            {
                Guid ClientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;

                #region "Get MQL StageId & Title from Stage Table"
                Stage stageMQL = db.Stages.FirstOrDefault(s => s.ClientId == ClientId && s.Code == Common.StageMQL && s.IsDeleted == false);

                if (stageMQL != null)
                {
                    MQLStageId = stageMQL.StageId;
                    // Get MQL stage title to render in Integration Summary Email.
                    //MQLtitle = stageMQL.Title;   // this line commented to make consistancy between Elouqa & Salesforce. In Eloqua used static MQL stage title to render messages under Symmary Email.
                    // Get MQL Level
                    if (stageMQL.Level.HasValue)
                        MQLLevel = stageMQL.Level.Value;  // use MQLLevel to filter tactic.
                }

                // if MQL stage title not exist then set "MQL" default value.
                if (string.IsNullOrEmpty(MQLtitle))
                    MQLtitle = "MQL";               // To make consistancy between Elouqa & Salesforce, Used static MQL stage title as defined in eloqua to render messages under Symmary Email.
                #endregion

                // Insert log into IntegrationInstanceSection, Dharmraj PL#684
                int IntegrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), DateTime.Now, _userId);

                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Creating Eloqua & Model IntegrationInstanceId mapping list start.");

                #region "Check for pulling Responses whether Model/Plan associated or not with current Instance"
                List<Model> lstModels = db.Models.Where(objmdl => objmdl.IntegrationInstanceIdMQL == _integrationInstanceId && objmdl.Status.Equals("Published") && objmdl.IsActive == true).ToList();
                if (lstModels == null || lstModels.Count <= 0)
                {
                    // Save & display Message: No single Model associated with current Instance.
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull " + MQLtitle + ": There is no single Model associated with this Instance to pull " + MQLtitle + ".", Enums.SyncStatus.Info, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull " + MQLtitle + ": There is no single Model associated with this Instance to pull " + MQLtitle + ".");
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    return; // no error.
                }
                List<int> ModelIds = lstModels.Select(mdl => mdl.ModelId).ToList();
                List<Plan> lstPlans = db.Plans.Where(objplan => ModelIds.Contains(objplan.Model.ModelId) && objplan.IsActive == true).ToList();
                if (lstPlans == null || lstPlans.Count <= 0)
                {
                    // Save & display Message: No single Plan associated with current Instance.
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull " + MQLtitle + ": There is no single Plan associated with this Instance to pull " + MQLtitle + ".", Enums.SyncStatus.Info, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull " + MQLtitle + ": There is no single Plan associated with this Instance to pull " + MQLtitle + ".");
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    return; // no error.
                }
                #endregion

                //// Get Eloqua integration type Id.
                var eloquaIntegrationType = db.IntegrationTypes.Where(type => type.Code == EloquaCode && type.IsDeleted == false).Select(type => type.IntegrationTypeId).FirstOrDefault();
                int eloquaIntegrationTypeId = Convert.ToInt32(eloquaIntegrationType);

                //// Get All EloquaIntegrationTypeIds to retrieve  EloquaPlanIds.
                List<int> lstEloquaIntegrationTypeIds = db.IntegrationInstances.Where(instance => instance.IntegrationTypeId.Equals(eloquaIntegrationTypeId) && instance.IsDeleted.Equals(false) && instance.ClientId.Equals(ClientId)).Select(s => s.IntegrationInstanceId).ToList();

                //// Get all PlanIds whose Tactic data PUSH on Eloqua.
                //List<int> lstEloquaPlanIds = lstPlans.Where(objplan => lstEloquaIntegrationTypeIds.Contains(objplan.Model.IntegrationInstanceId.Value)).Select(objplan => objplan.PlanId).ToList();

                //// Get All PlanIds.
                List<int> AllplanIds = lstPlans.Select(objplan => objplan.PlanId).ToList();

                //// Get SalesForce PlanIds.
                //  List<int> lstSalesForceplanIds = lstPlans.Where(objplan => !lstEloquaPlanIds.Contains(objplan.PlanId)).Select(plan => plan.PlanId).ToList();


                // Get List of status after Approved Status
                List<string> statusList = Common.GetStatusListAfterApproved();


                List<Plan_Campaign_Program_Tactic> tblPlanTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeployedToIntegration == true &&
                                                                                                               statusList.Contains(tactic.Status) &&
                                                                                                               tactic.Stage.Level <= MQLLevel &&
                                                                                                               tactic.IsDeleted == false && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).ToList();

                //// Get All Approved,IsDeployedToIntegration true and IsDeleted false Tactic list.
                List<Plan_Campaign_Program_Tactic> lstAllTactics = tblPlanTactic.Where(tactic => AllplanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).ToList();

                //// Get list of EloquaIntegrationInstanceTacticID(EloquaId).
                List<EloquaIntegrationInstanceTactic_Model_Mapping> lstEloquaIntegrationInstanceTacticIds = lstAllTactics.Where(tactic => string.IsNullOrEmpty(tactic.IntegrationInstanceTacticId) && !string.IsNullOrEmpty(tactic.IntegrationInstanceEloquaId)).Select(_tac => new EloquaIntegrationInstanceTactic_Model_Mapping
                {
                    EloquaIntegrationInstanceTacticId = _tac.IntegrationInstanceEloquaId,
                    ModelIntegrationInstanceId = _tac.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceEloquaId.Value,
                    PlanTacticId = _tac.PlanTacticId
                }).ToList();
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Creating Eloqua & Model IntegrationInstanceId mapping list end.");

                if (lstEloquaIntegrationInstanceTacticIds == null)
                    lstEloquaIntegrationInstanceTacticIds = new List<EloquaIntegrationInstanceTactic_Model_Mapping>();

                //// Add IntegrationEloquaClient object to Mapping list for distinct ModelIntegrationInstanceId.
                //// Get Mapping List of SalesForceIntegrationInstanceTactic Ids(CRMIds) based on EloquaIntegrationInstanceTacticID(EloquaId).
                //List<CRM_EloquaMapping> lstSalesForceIntegrationInstanceTacticIds = new List<CRM_EloquaMapping>();
                Integration.Eloqua.EloquaCampaign objEloqua;
                Plan_Campaign_Program_Tactic objUpdTactic;
                int cntrUpdate = 0;
                Integration.Eloqua.IntegrationEloquaClient integrationEloquaClient;
                foreach (int _ModelIntegrationInstanceId in lstEloquaIntegrationInstanceTacticIds.Select(tac => tac.ModelIntegrationInstanceId).Distinct())
                {
                    try
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Authenticate Eloqua Instance for mapping Salesforce & Eloqua.");
                        integrationEloquaClient = new Integration.Eloqua.IntegrationEloquaClient(_ModelIntegrationInstanceId, 0, _entityType, _userId, _integrationInstanceLogId, _applicationId);
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Authenticate Eloqua Instance for mapping Salesforce & Eloqua.");
                        foreach (EloquaIntegrationInstanceTactic_Model_Mapping _EloquaTac in lstEloquaIntegrationInstanceTacticIds.Where(_eloqua => _eloqua.ModelIntegrationInstanceId.Equals(_ModelIntegrationInstanceId)))
                        {
                            //objEloqua.IntegrationEloquaClient = integrationEloquaClient;
                            if (!string.IsNullOrEmpty(_EloquaTac.EloquaIntegrationInstanceTacticId))
                            {

                                objEloqua = new Integration.Eloqua.EloquaCampaign();

                                ////Get SalesForceIntegrationTacticId based on EloquaIntegrationTacticId.
                                objEloqua = integrationEloquaClient.GetEloquaCampaign(_EloquaTac.EloquaIntegrationInstanceTacticId);
                                if (objEloqua != null && !string.IsNullOrEmpty(objEloqua.crmId))
                                {
                                    objUpdTactic = new Plan_Campaign_Program_Tactic();
                                    objUpdTactic = lstAllTactics.Where(s => s.PlanTacticId == _EloquaTac.PlanTacticId).FirstOrDefault();
                                    if (objUpdTactic != null)
                                    {
                                        objUpdTactic.IntegrationInstanceTacticId = objEloqua.crmId;
                                        db.Entry(objUpdTactic).State = EntityState.Modified;
                                        cntrUpdate++;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _isResultError = true;
                        string exMessage = Common.GetInnermostException(ex);
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull " + MQLtitle + ": System error occurred while pulling EloquaId for Salesforce and Eloqua mapping. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while create mapping list of EloquaId and SalesforceId. Exception - " + exMessage);
                        continue;
                    }
                }
                if (cntrUpdate > 0)
                    db.SaveChanges();

                //// Get SalesForce tactic list
                List<Plan_Campaign_Program_Tactic> lstSalesForceTactic = lstAllTactics.Where(_tac => _tac.IntegrationInstanceTacticId != null).ToList();

                // Add By Nishant Sheth
                #region Plan To Salesforce Push and pull process for marketo instance
                List<int> MarketoModel = lstModels.Where(model => model.IntegrationInstanceMarketoID != null).Select(model => model.ModelId).ToList();// get list of model which havemarketo instance
                List<Plan_Campaign_Program_Tactic> marketoTactic = new List<Plan_Campaign_Program_Tactic>();
                if (MarketoModel.Count > 0)
                {
                    marketoTactic = SyncSFDCMarketo(MarketoModel, IntegrationInstanceSectionId, MQLLevel);
                }
                if (marketoTactic.Count > 0)
                {
                    lstSalesForceTactic.AddRange(marketoTactic);
                }
                #endregion

                if (lstSalesForceTactic.Count() > 0)
                {
                    try
                    {
                        string CampaignId = string.Empty;// "CampaignId";
                        string FirstRespondedDate = string.Empty;//"FirstRespondedDate";
                        string Status = string.Empty;//"Status";

                        var listPullMapping = db.IntegrationInstanceDataTypeMappingPulls.Where(instance => instance.IntegrationInstanceId == _integrationInstanceId && instance.GameplanDataTypePull.Type == Common.StageMQL)
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
                                int TotalPullResonsesCount = 0, ProcessedResponsesCount = 0;
                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Getting CampaignList from CampaignMember Table within Salesforce start.");

                                List<CampaignMember> CampaignMemberList = new List<CampaignMember>();
                                int pagesize = 400;
                                int pagecount = 0;
                                int totalrecords = lstSalesForceTactic.Count();
                                int maxpage = (totalrecords / pagesize);
                                while (pagecount <= maxpage)
                                {
                                    string AllIntegrationTacticIds = String.Join("','", lstSalesForceTactic.Skip(pagecount * pagesize).Take(pagesize).Select(tactic => tactic.IntegrationInstanceTacticId).ToList());
                                    AllIntegrationTacticIds = AllIntegrationTacticIds.Trim(new char[] { ',' });
                                    var responsePull = _client.Query<object>("SELECT " + CampaignId + "," + FirstRespondedDate + " FROM CampaignMember WHERE " + CampaignId + " IN ('" + AllIntegrationTacticIds + "') AND " + Status + "= '" + Common.Responded + "'");

                                    #region "Declare local variables"
                                    int _PlanTacticId = 0;
                                    CampaignMember objCampaign;
                                    JObject jobj;
                                    string TacticResult = string.Empty, campaignid = string.Empty;
                                    #endregion

                                    foreach (var resultin in responsePull)
                                    {
                                        #region "Initialize local variables"
                                        TacticResult = resultin.ToString();
                                        jobj = JObject.Parse(TacticResult);
                                        objCampaign = new CampaignMember();
                                        _PlanTacticId = 0;
                                        campaignid = string.Empty;
                                        #endregion

                                        try
                                        {

                                            campaignid = Convert.ToString(jobj[CampaignId]);
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
                                            string exMessage = Common.GetInnermostException(e);
                                            ErrorFlag = true;
                                            _ErrorMessage = exMessage;
                                            string TacticId = Convert.ToString(jobj[CampaignId]); ////CRMId

                                            //// check whether TacticId(CRMId) exist in field IntegrationInstanceTacticID field of SalesForceTactic list.
                                            var tactic = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
                                            if (tactic != null)
                                                _PlanTacticId = tactic.PlanTacticId;
                                            //else                                        //// if Tactic not exist then retrieve PlanTacticId from EloquaTactic list.
                                            //    _PlanTacticId = lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && _SalTac.CRMId.Equals(TacticId)).Select(s => s.PlanTacticId).FirstOrDefault();

                                            IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                            instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                            instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                            instanceTactic.EntityId = _PlanTacticId;
                                            instanceTactic.EntityType = EntityType.Tactic.ToString();
                                            instanceTactic.Status = StatusResult.Error.ToString();
                                            instanceTactic.Operation = Operation.Pull_QualifiedLeads.ToString();
                                            instanceTactic.SyncTimeStamp = DateTime.Now;
                                            instanceTactic.CreatedDate = DateTime.Now;
                                            instanceTactic.ErrorDescription = exMessage;
                                            instanceTactic.CreatedBy = _userId;
                                            db.Entry(instanceTactic).State = EntityState.Added;
                                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting Campaign from Salesforce. Exception - " + exMessage);
                                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull " + MQLtitle + ": Error occurred while getting Campaign from Salesforce. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                            _isResultError = true;
                                        }
                                        catch (Exception e)
                                        {
                                            string exMessage = Common.GetInnermostException(e);
                                            ErrorFlag = true;
                                            _ErrorMessage = exMessage;
                                            string TacticId = Convert.ToString(jobj[CampaignId]); ////CRMId

                                            //// check whether TacticId(CRMId) exist in field IntegrationInstanceTacticID field of SalesForceTactic list.
                                            var tactic = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
                                            if (tactic != null)
                                                _PlanTacticId = tactic.PlanTacticId;
                                            //else                                        //// if Tactic not exist then retrieve PlanTacticId from EloquaTactic list.
                                            //    _PlanTacticId = lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && _SalTac.CRMId.Equals(TacticId)).Select(s => s.PlanTacticId).FirstOrDefault();

                                            IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                            instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                            instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                            instanceTactic.EntityId = _PlanTacticId;
                                            instanceTactic.EntityType = EntityType.Tactic.ToString();
                                            instanceTactic.Status = StatusResult.Error.ToString();
                                            instanceTactic.Operation = Operation.Pull_QualifiedLeads.ToString();
                                            instanceTactic.SyncTimeStamp = DateTime.Now;
                                            instanceTactic.CreatedDate = DateTime.Now;
                                            instanceTactic.ErrorDescription = exMessage;
                                            instanceTactic.CreatedBy = _userId;
                                            db.Entry(instanceTactic).State = EntityState.Added;
                                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting Campaign from Salesforce. Exception - " + exMessage);
                                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull " + MQLtitle + ": Error occurred while getting Campaign from Salesforce. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                            _isResultError = true;
                                        }
                                    }
                                    pagecount++;
                                }
                                //List<Plan_Campaign_Program_Tactic> lstCRM_EloquaTactics = (from _Tactic in lstEloquaTactic
                                //                                                           join _ElqTactic in lstSalesForceIntegrationInstanceTacticIds on _Tactic.IntegrationInstanceEloquaId equals _ElqTactic.EloquaId
                                //                                                           where _ElqTactic.CRMId != null
                                //                                                           select _Tactic).ToList();

                                List<Plan_Campaign_Program_Tactic> lstMergedTactics = lstSalesForceTactic;
                                //lstCRM_EloquaTactics.ForEach(_elqTactic => lstMergedTactics.Add(_elqTactic));

                                lstMergedTactics = lstMergedTactics.Distinct().ToList();
                                List<int> OuterTacticIds = lstMergedTactics.Select(t => t.PlanTacticId).ToList();

                                List<int> linkedTactics = new List<int>();
                                List<int> lstMultiLinkedTactic = new List<int>();
                                if (CampaignMemberList.Count > 0)
                                {
                                    #region "Remove Actuals values"

                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Create LinkedTactic mapping list start.");
                                    #region "Create LinkedTactic Mapping list"
                                    Dictionary<int, int> lstlinkedTacticMapping = new Dictionary<int, int>();
                                    int linkedTacticId;
                                    foreach (var tac in lstMergedTactics)
                                    {
                                        linkedTacticId = 0;
                                        #region "Retrieve linkedTactic"
                                        linkedTacticId = (tac != null && tac.LinkedTacticId.HasValue) ? tac.LinkedTacticId.Value : 0;
                                        //if (linkedTacticId <= 0)
                                        //{
                                        //    var lnkPCPT = tblPlanTactic.Where(tactic => tactic.LinkedTacticId == tac.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                                        //    linkedTacticId = lnkPCPT != null ? lnkPCPT.PlanTacticId : 0;
                                        //}

                                        if (linkedTacticId > 0)
                                        {
                                            lstlinkedTacticMapping.Add(tac.PlanTacticId, linkedTacticId);
                                        }
                                        #endregion
                                    }
                                    #endregion
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Create LinkedTactic mapping list end.");

                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Removing ActualTactic start.");


                                    if (lstlinkedTacticMapping.Count > 0)
                                    {
                                        linkedTactics = lstlinkedTacticMapping.Select(lnkdTac => lnkdTac.Value).ToList();
                                    }

                                    List<Plan_Campaign_Program_Tactic_Actual> tblActuals = new List<Plan_Campaign_Program_Tactic_Actual>();
                                    if (linkedTactics != null && linkedTactics.Count > 0)
                                    {
                                        tblActuals = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => (OuterTacticIds.Contains(actual.PlanTacticId) || linkedTactics.Contains(actual.PlanTacticId)) && actual.StageTitle == Common.MQLStageValue).ToList();
                                    }
                                    else
                                    {
                                        tblActuals = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => OuterTacticIds.Contains(actual.PlanTacticId) && actual.StageTitle == Common.MQLStageValue).ToList();
                                    }
                                    //Modification Start Viral 18Feb2016 #2006 H9_QA - SFDC integration bug
                                    //List<Plan_Campaign_Program_Tactic_Actual> OuteractualTacticList = tblActuals.Where(actual => OuterTacticIds.Contains(actual.PlanTacticId)).ToList();
                                    //OuteractualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                    if (tblActuals != null && tblActuals.Count > 0)
                                    {
                                        try
                                        {
                                            db.Configuration.AutoDetectChangesEnabled = false;
                                            List<Plan_Campaign_Program_Tactic_Actual> OuteractualTacticList = tblActuals.Where(actual => OuterTacticIds.Contains(actual.PlanTacticId)).ToList();
                                            OuteractualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                        }
                                        finally
                                        {
                                            db.Configuration.AutoDetectChangesEnabled = true;
                                        }
                                        //Modification End Viral 18Feb2016 #2006 H9_QA - SFDC integration bug
                                        // Remove linked Tactic's Actuals.

                                        if (linkedTactics.Count > 0)
                                        {

                                            #region "Get list of linked Tactics that multiyear or not"
                                            Plan_Campaign_Program_Tactic objLnkTac = null;
                                            int yeardiff = 0, cntr = 0, perdNum = 12; bool isMultilinkedTactic = false;
                                            List<Plan_Campaign_Program_Tactic_Actual> linkedactualTacticList = null;
                                            List<string> lstLinkedPeriods = new List<string>();
                                            foreach (int linkdTacId in linkedTactics)
                                            {
                                                try
                                                {
                                                    objLnkTac = new Plan_Campaign_Program_Tactic();
                                                    objLnkTac = tblPlanTactic.Where(tac => tac.PlanTacticId == linkdTacId).FirstOrDefault();

                                                    if (objLnkTac != null)
                                                    {
                                                        yeardiff = objLnkTac.EndDate.Year - objLnkTac.StartDate.Year;
                                                        isMultilinkedTactic = yeardiff > 0 ? true : false;
                                                        linkedactualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                                                        if (isMultilinkedTactic)
                                                        {
                                                            // remove linked tactic respective months actuals data.
                                                            lstLinkedPeriods = new List<string>();
                                                            cntr = 12 * yeardiff;
                                                            for (int i = 1; i <= cntr; i++)
                                                            {
                                                                lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
                                                            }
                                                            lstMultiLinkedTactic.Add(linkdTacId);
                                                            linkedactualTacticList = tblActuals.Where(actual => linkdTacId == actual.PlanTacticId && lstLinkedPeriods.Contains(actual.Period)).ToList();
                                                            if (linkedactualTacticList != null && linkedactualTacticList.Count > 0)
                                                            {
                                                                try
                                                                {
                                                                    db.Configuration.AutoDetectChangesEnabled = false;
                                                                    linkedactualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                                                }
                                                                finally
                                                                {
                                                                    db.Configuration.AutoDetectChangesEnabled = true;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            linkedactualTacticList = tblActuals.Where(actual => linkdTacId == actual.PlanTacticId).ToList();
                                                            if (linkedactualTacticList != null && linkedactualTacticList.Count > 0)
                                                            {
                                                                try
                                                                {
                                                                    db.Configuration.AutoDetectChangesEnabled = false;
                                                                    linkedactualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                                                }
                                                                finally
                                                                {
                                                                    db.Configuration.AutoDetectChangesEnabled = true;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Error occurred on remove linked tactic : Linked TacticId-" + linkdTacId.ToString() + Common.GetInnermostException(ex));
                                                }
                                            }
                                            #endregion

                                            //List<Plan_Campaign_Program_Tactic_Actual> linkedactualTacticList = tblActuals.Where(actual => linkedTactics.Contains(actual.PlanTacticId)).ToList();
                                            //linkedactualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                        }

                                        db.SaveChanges();

                                    }
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Removing ActualTactic end.");

                                    #endregion

                                    lstSalesForceTactic = lstSalesForceTactic.OrderBy(tac => tac.PlanTacticId).ToList(); // Add By Nishant Sheth // For pull inq, mql, cw #2188
                                    var CampaignMemberListGroup = CampaignMemberList.GroupBy(cl => new { CampaignId = cl.CampaignId, Month = cl.FirstRespondedDate.ToString("MM/yyyy") }).Select(cl =>
                                        new
                                        {
                                            CampaignId = cl.Key.CampaignId,
                                            TacticId = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null ? (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).PlanTacticId) : 0,
                                            //Period = "Y" + Convert.ToDateTime(cl.Key.Month).Month,
                                            //IsYear = (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year == Convert.ToDateTime(cl.Key.Month).Year)) ? true : false,
                                            Period = (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null) && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year < Convert.ToDateTime(cl.Key.Month).Year) ? "Y" + (((Convert.ToDateTime(cl.Key.Month).Year - (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year)) * 12) + Convert.ToDateTime(cl.Key.Month).Month) : "Y" + Convert.ToDateTime(cl.Key.Month).Month,
                                            IsYear = (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year <= Convert.ToDateTime(cl.Key.Month).Year) && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).EndDate.Year >= Convert.ToDateTime(cl.Key.Month).Year)) ? true : false,
                                            Count = cl.Count()
                                        }).Where(cm => cm.IsYear).ToList();

                                    //Set count of total pulled responses from Salesforce.
                                    TotalPullResonsesCount = CampaignMemberListGroup != null ? CampaignMemberListGroup.Sum(cnt => cnt.Count) : 0;

                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Create PlanTacticActual list and insert Tactic log.");

                                    int linkedTacId = 0;
                                    Plan_Campaign_Program_Tactic objLinkedTactic = null;
                                    bool isMultiYearlinkedTactic = false;
                                    int yearDiff = 0;

                                    try
                                    {
                                        db.Configuration.AutoDetectChangesEnabled = false;
                                        foreach (var tactic in lstMergedTactics)
                                        {
                                            try
                                            {
                                                var innerCampaignMember = CampaignMemberListGroup.Where(cm => cm.TacticId == tactic.PlanTacticId).ToList();
                                                if (linkedTactics != null && linkedTactics.Count > 0) // check whether linkedTactics exist or not.
                                                    linkedTacId = lstlinkedTacticMapping.FirstOrDefault(tac => tac.Key == tactic.PlanTacticId).Value;
                                                if (linkedTacId > 0) // check whether linkedTactics exist or not.
                                                {
                                                    objLinkedTactic = new Plan_Campaign_Program_Tactic();
                                                    objLinkedTactic = tblPlanTactic.Where(tac => tac.PlanTacticId == linkedTacId).FirstOrDefault();
                                                }
                                                foreach (var objCampaignMember in innerCampaignMember)
                                                {
                                                    Plan_Campaign_Program_Tactic_Actual objPlanTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                                    objPlanTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                                    objPlanTacticActual.PlanTacticId = objCampaignMember.TacticId;
                                                    objPlanTacticActual.Period = objCampaignMember.Period;
                                                    // Note: we use StageProjectedStageValue istead of MQLStageValue to make consistency between Save Actual data from Actual inspect screen & this screen. But It should be MQLStageValue.
                                                    objPlanTacticActual.StageTitle = Common.MQLStageValue; //Common.MQLStageValue; In Actual load screen, on MQL value save time "ProjectedStageValue" value inserted instead of "MQL" in table "Plan_Campaign_Program_Tactic_Actual". It should be "MQL".
                                                    objPlanTacticActual.Actualvalue = objCampaignMember.Count;
                                                    objPlanTacticActual.CreatedBy = _userId;
                                                    objPlanTacticActual.CreatedDate = DateTime.Now;
                                                    db.Entry(objPlanTacticActual).State = EntityState.Added;
                                                    ProcessedResponsesCount = ProcessedResponsesCount + objCampaignMember.Count;

                                                    // Add linked Tacitc Actual Values.
                                                    if (linkedTacId > 0 && objLinkedTactic != null) // check whether linkedTactics exist or not.
                                                    {
                                                        yearDiff = objLinkedTactic.EndDate.Year - objLinkedTactic.StartDate.Year;
                                                        isMultiYearlinkedTactic = yearDiff > 0 ? true : false;

                                                        string orgPeriod = objCampaignMember.Period;
                                                        string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                        int NumPeriod = int.Parse(numPeriod);
                                                        if (isMultiYearlinkedTactic)
                                                        {
                                                            Plan_Campaign_Program_Tactic_Actual objLinkedTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                                            objLinkedTacticActual.PlanTacticId = linkedTacId;           // LinkedTactic Id.
                                                            objLinkedTacticActual.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                                            objLinkedTacticActual.StageTitle = Common.MQLStageValue;
                                                            objLinkedTacticActual.Actualvalue = objCampaignMember.Count;
                                                            objLinkedTacticActual.CreatedBy = _userId;
                                                            objLinkedTacticActual.CreatedDate = DateTime.Now;
                                                            db.Entry(objLinkedTacticActual).State = EntityState.Added;
                                                        }
                                                        else
                                                        {
                                                            if (NumPeriod > 12)
                                                            {
                                                                int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
                                                                int div = NumPeriod / 12;    // In case of 24, Y12.
                                                                if (rem > 0 || div > 1)
                                                                {
                                                                    Plan_Campaign_Program_Tactic_Actual objLinkedTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                                                    objLinkedTacticActual.PlanTacticId = linkedTacId;           // LinkedTactic Id.
                                                                    objLinkedTacticActual.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());                            // For March, Y3(i.e 15%12 = 3)     
                                                                    objLinkedTacticActual.StageTitle = Common.MQLStageValue;
                                                                    objLinkedTacticActual.Actualvalue = objCampaignMember.Count;
                                                                    objLinkedTacticActual.CreatedBy = _userId;
                                                                    objLinkedTacticActual.CreatedDate = DateTime.Now;
                                                                    db.Entry(objLinkedTacticActual).State = EntityState.Added;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                tactic.LastSyncDate = DateTime.Now;
                                                //tactic.ModifiedDate = DateTime.Now;
                                                tactic.ModifiedBy = _userId;

                                                // Update linked Tactic lastSync Date,ModifiedDate & ModifiedBy.
                                                if (linkedTacId > 0 && objLinkedTactic != null) // check whether linkedTactics exist or not.
                                                {

                                                    objLinkedTactic.LastSyncDate = DateTime.Now;
                                                    //objLinkedTactic.ModifiedDate = DateTime.Now;
                                                    objLinkedTactic.ModifiedBy = _userId;
                                                }

                                                IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                                instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                                instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                                instanceTactic.EntityId = tactic.PlanTacticId;
                                                instanceTactic.EntityType = EntityType.Tactic.ToString();
                                                instanceTactic.Status = StatusResult.Success.ToString();
                                                instanceTactic.Operation = Operation.Pull_QualifiedLeads.ToString();
                                                instanceTactic.SyncTimeStamp = DateTime.Now;
                                                instanceTactic.CreatedDate = DateTime.Now;
                                                instanceTactic.CreatedBy = _userId;
                                                db.Entry(instanceTactic).State = EntityState.Added;
                                            }
                                            catch (Exception ex)
                                            {
                                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Error occurred on insert MQL on Actual table : TacticId-" + tactic.PlanTacticId.ToString() + Common.GetInnermostException(ex));
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        db.Configuration.AutoDetectChangesEnabled = true;
                                    }

                                    db.SaveChanges();
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Create PlanTacticActual list and insert Tactic log.");
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
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull " + MQLtitle + ": Total Contact(s) - " + TotalPullResonsesCount.ToString() + ", " + ProcessedResponsesCount.ToString() + " contact(s) were processed and pulled in database.");
                                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull " + MQLtitle + ": Total Contact(s) - " + TotalPullResonsesCount.ToString() + ", " + ProcessedResponsesCount.ToString() + " contact(s) were processed and pulled in database.", Enums.SyncStatus.Info, DateTime.Now));
                                }
                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Getting CampaignList from CampaignMember Table within Salesforce end.");
                            }
                            else
                            {
                                _isResultError = true;
                                // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgMappingNotFoundForSalesforcePullResponse);
                                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull " + MQLtitle + ": " + Common.msgMappingNotFoundForSalesforcePullResponse, Enums.SyncStatus.Error, DateTime.Now));
                            }
                        }
                        else
                        {
                            // Update IntegrationInstanceSection log with Error status, modified by Mitesh Vaishnav for internal review point on 07-07-2015
                            _isResultError = true;
                            Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgMappingNotFoundForSalesforcePullResponse);
                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull " + MQLtitle + ": " + Common.msgMappingNotFoundForSalesforcePullResponse, Enums.SyncStatus.Error, DateTime.Now));
                        }
                    }
                    catch (SalesforceException e)
                    {
                        _isResultError = true;
                        string exMessage = Common.GetInnermostException(e);
                        // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, exMessage);
                        //Common.SaveIntegrationInstanceLogDetails(_id, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while Pulling Campaign from Salesforce:- " + e.Message);
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull " + MQLtitle + ": Error occurred while pulling campaign from Salesforce. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    }
                }
                else
                {
                    // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                }
            }
            catch (Exception ex)
            {
                _isResultError = true;
                string exMessage = Common.GetInnermostException(ex);
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Error occurred while Pulling " + MQLtitle + " from Salesforce. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while Pulling " + MQLtitle + " from Salesforce. Exception -" + exMessage);
            }

        }

        /// <summary>
        /// Get SFDC Id with tactic name from salesforce and update ids into plan
        /// Created by : Nishant Sheth
        /// </summary>
        public List<Plan_Campaign_Program_Tactic> SyncSFDCMarketo(List<int> MarketoModelIds = null, int IntegrationInstanceSectionId = 0, int StageLevel = 0)
        {
            if (MarketoModelIds == null)
            {
                MarketoModelIds = new List<int>();
            }

            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            List<string> statusList = Common.GetStatusListAfterApproved();
            // Get list of marketo ids which not have salesforce id (IntegrationInstanceTacticId)
            List<Plan_Campaign_Program_Tactic> ListOfMarketoTactic = new List<Plan_Campaign_Program_Tactic>();

            ListOfMarketoTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.IntegrationInstanceMarketoID != null && tac.IsDeleted == false
                    && tac.IsDeployedToIntegration == true
                && statusList.Contains(tac.Status) && MarketoModelIds.Contains(tac.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId)).ToList();

            if (ListOfMarketoTactic.Count > 0)
            {

                var ListOfMarketoWithoutSFDC = ListOfMarketoTactic.Where(tac => tac.IntegrationInstanceTacticId == null).ToList();
                if (ListOfMarketoWithoutSFDC.Count > 0)
                {

                    string AllMarketoTitles = String.Join("','", ListOfMarketoWithoutSFDC.Where(tac => !string.IsNullOrEmpty(tac.TacticCustomName))
                    .Select(tac => tac.TacticCustomName.Trim()).ToArray());
                    AllMarketoTitles = AllMarketoTitles.Trim(new char[] { ',' });

                    var responsePull = _client.Query<object>("SELECT Id,Name FROM Campaign  WHERE Name IN ('" + AllMarketoTitles + "')");

                    List<Plan_Campaign_Program_Tactic> PlanTactic = new List<Plan_Campaign_Program_Tactic>();
                    JObject jobj;
                    // Create salesforce type table
                    DataTable DtTable = new DataTable();
                    DtTable.Columns.Add("Id", typeof(string));
                    DtTable.Columns.Add("Name", typeof(string));

                    foreach (var resultin in responsePull)
                    {
                        jobj = JObject.Parse(resultin.ToString());
                        try
                        {
                            var UpdateRecord = ListOfMarketoWithoutSFDC.Where(tac => tac.TacticCustomName == Convert.ToString(jobj["Name"])).FirstOrDefault();
                            if (UpdateRecord != null)
                            {
                                UpdateRecord.IntegrationInstanceTacticId = Convert.ToString(jobj["Id"]);
                            }
                            DataRow dr = DtTable.NewRow();
                            dr["Id"] = Convert.ToString(jobj["Id"]);
                            dr["Name"] = Convert.ToString(jobj["Name"]);
                            DtTable.Rows.Add(dr);
                        }
                        catch (SalesforceException e)
                        {
                            string exMessage = Common.GetInnermostException(e);
                            _ErrorMessage = exMessage;
                            int TacticId = ListOfMarketoWithoutSFDC.Where(tac => tac.Title == Convert.ToString(jobj["Name"])).Select(tac => tac.PlanTacticId).FirstOrDefault();

                            IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                            instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;// To do : Need to change wtih IntegrationInstanceSectionId
                            instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                            instanceTactic.EntityId = TacticId;
                            instanceTactic.EntityType = EntityType.Tactic.ToString();
                            instanceTactic.Status = StatusResult.Error.ToString();
                            instanceTactic.Operation = Operation.Get_SFDCID_For_Marketo.ToString();
                            instanceTactic.SyncTimeStamp = DateTime.Now;
                            instanceTactic.CreatedDate = DateTime.Now;
                            instanceTactic.ErrorDescription = exMessage;
                            instanceTactic.CreatedBy = _userId;
                            db.Entry(instanceTactic).State = EntityState.Added;
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting Campaign Id from Salesforce. Exception - " + exMessage);

                        }
                        catch (Exception e)
                        {
                            string exMessage = Common.GetInnermostException(e);
                            _ErrorMessage = exMessage;
                            int TacticId = ListOfMarketoWithoutSFDC.Where(tac => tac.Title == Convert.ToString(jobj["Name"])).Select(tac => tac.PlanTacticId).FirstOrDefault();

                            IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                            instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;// To do : Need to change wtih IntegrationInstanceSectionId
                            instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                            instanceTactic.EntityId = TacticId;
                            instanceTactic.EntityType = EntityType.Tactic.ToString();
                            instanceTactic.Status = StatusResult.Error.ToString();
                            instanceTactic.Operation = Operation.Get_SFDCID_For_Marketo.ToString();
                            instanceTactic.SyncTimeStamp = DateTime.Now;
                            instanceTactic.CreatedDate = DateTime.Now;
                            instanceTactic.ErrorDescription = exMessage;
                            instanceTactic.CreatedBy = _userId;
                            db.Entry(instanceTactic).State = EntityState.Added;
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting Campaign Id from Salesforce. Exception - " + exMessage);

                        }
                    }

                    var SalesForce = new SqlParameter("SalesForce", SqlDbType.Structured);
                    SalesForce.Value = DtTable;
                    SalesForce.TypeName = "dbo.SalesforceType";
                    StoredProcedure objSp = new StoredProcedure();
                    // Call stored procedure for update salesforce ids of marketo tactic
                    objSp.ExecuteStoreProcedure(db, "UpdateSalesforceIdForMarketoTactic", SalesForce);
                    List<TacticLinkedTacMapping> lstTac_LinkTacMapping = new List<TacticLinkedTacMapping>();
                    List<int> tacticIdList = new List<int>();
                    List<int> lstProcessTacIds = new List<int>();
                    List<Plan_Campaign> campaignList = new List<Plan_Campaign>();
                    List<Plan_Campaign_Program> programList = new List<Plan_Campaign_Program>();
                    List<int> MarketoTacticIds = ListOfMarketoWithoutSFDC.Select(a => a.PlanTacticId).ToList();
                    // ListOfMarketoTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => MarketoTacticIds.Contains(tactic.PlanTacticId)).ToList();
                    _clientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;
                    #region "Create Tactic-Linked Tactic mapping list:- Push latest year plan tactic as Plan Tactic and other as linked"
                    if (ListOfMarketoWithoutSFDC != null && ListOfMarketoWithoutSFDC.Count > 0 && DtTable.Rows.Count > 0)
                    {
                        string published = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString();
                        List<Plan> lstPlan = db.Plans.Where(p => p.Model.IntegrationInstanceId == _integrationInstanceId && p.Model.Status.Equals(published)).ToList();
                        List<int> planIds = new List<int>();
                        List<Plan_Campaign_Program_Tactic> tblTactic = new List<Plan_Campaign_Program_Tactic>();
                        if (lstPlan != null && lstPlan.Count > 0)
                        {
                            planIds = lstPlan.Select(p => p.PlanId).ToList();
                            campaignList = db.Plan_Campaign.Where(campaign => planIds.Contains(campaign.PlanId) && !campaign.IsDeleted).ToList();
                            List<int> campaignIdList = campaignList.Select(c => c.PlanCampaignId).ToList();
                            programList = db.Plan_Campaign_Program.Where(program => campaignIdList.Contains(program.PlanCampaignId) && !program.IsDeleted).ToList();
                            List<int> programIdList = programList.Select(c => c.PlanProgramId).ToList();
                            tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).ToList();
                            ListOfMarketoWithoutSFDC = tblTactic.Where(tactic => programIdList.Contains(tactic.PlanProgramId)).ToList();
                        }

                        var lstTac_LinkTacMappIds = ListOfMarketoWithoutSFDC.Where(tac => tac.LinkedTacticId.HasValue).Select(tac => new { PlanTacticId = tac.PlanTacticId, PlanTacic = tac, LinkedTacticId = tac.LinkedTacticId.Value }).ToList();
                        if (lstTac_LinkTacMappIds != null && lstTac_LinkTacMappIds.Count > 0)
                        {
                            #region "Declare local variables"
                            TacticLinkedTacMapping objTacLinkMapping;
                            Plan_Campaign_Program_Tactic linkedTactic;
                            string strOrgnlTacPlanyear, strLnkdTacPlanYear;
                            int orgnlTacPlanYear, lnkdTacPlanYear;
                            #endregion

                            foreach (var tac in lstTac_LinkTacMappIds.ToList())
                            {
                                #region "Initialize local variables"
                                objTacLinkMapping = new TacticLinkedTacMapping();
                                linkedTactic = new Plan_Campaign_Program_Tactic();
                                strOrgnlTacPlanyear = string.Empty; strLnkdTacPlanYear = string.Empty;
                                orgnlTacPlanYear = 0; lnkdTacPlanYear = 0;
                                #endregion

                                // get linked tactic
                                if (EntityType.Tactic.Equals(_entityType))
                                {
                                    linkedTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == tac.LinkedTacticId && statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).FirstOrDefault();
                                }
                                else if (EntityType.Campaign.Equals(_entityType) || EntityType.Program.Equals(_entityType))
                                {
                                    linkedTactic = tblTactic.Where(tactic => tactic.PlanTacticId == tac.LinkedTacticId).FirstOrDefault();
                                }
                                else
                                {
                                    linkedTactic = ListOfMarketoWithoutSFDC.Where(tactc => tactc.PlanTacticId == tac.LinkedTacticId).FirstOrDefault();
                                }
                                if (linkedTactic != null)
                                {
                                    if (!ListOfMarketoWithoutSFDC.Contains(linkedTactic))
                                        ListOfMarketoWithoutSFDC.Add(linkedTactic);

                                    #region "Get both tactics Plan year"
                                    strOrgnlTacPlanyear = tac.PlanTacic.Plan_Campaign_Program.Plan_Campaign.Plan.Year; // Get Orginal Tactic Plan Year.
                                    strLnkdTacPlanYear = linkedTactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year; // Get Linked Tactic Plan Year.

                                    //parse plan year to int.
                                    if (!string.IsNullOrEmpty(strOrgnlTacPlanyear) && !string.IsNullOrEmpty(strLnkdTacPlanYear))
                                    {
                                        orgnlTacPlanYear = int.Parse(strOrgnlTacPlanyear);
                                        lnkdTacPlanYear = int.Parse(strLnkdTacPlanYear);
                                    }
                                    #endregion

                                    #region "Insert latest plan year tactic to lstTac_LinkTacMapping list"
                                    //Identify latest Tactic and add it to "lstTac_LinkTacMapping" list to push latest tactics to SFDC.
                                    if (lnkdTacPlanYear > orgnlTacPlanYear)
                                    {
                                        objTacLinkMapping.PlanTactic = linkedTactic; // set latest plan year tactic as PlanTactic to model and pass to SFDC.
                                        objTacLinkMapping.LinkedTactic = tac.PlanTacic; // set old tactic as LinkedTactic and update its IntegrationInstanceTacticId & Comment after pushing origional Tactic.
                                        ListOfMarketoWithoutSFDC.Remove(tac.PlanTacic);    // Remove the old tactic from tacticlist which is not going to push to SFDC.
                                    }
                                    else
                                    {
                                        objTacLinkMapping.PlanTactic = tac.PlanTacic; // set latest plan year tactic as PlanTactic to model and pass to SFDC.
                                        objTacLinkMapping.LinkedTactic = linkedTactic; // set old tactic as LinkedTactic and update its IntegrationInstanceTacticId & Comment after pushing origional Tactic.
                                        ListOfMarketoWithoutSFDC.Remove(linkedTactic);    // Remove the old tactic from tacticlist which is not going to push to SFDC.
                                    }
                                    #endregion

                                    #region "Remove linked Tactic from current list."
                                    var objLnkdTac = lstTac_LinkTacMappIds.FirstOrDefault(t => t.PlanTacticId == tac.LinkedTacticId);
                                    if (objLnkdTac != null)
                                    {
                                        lstTac_LinkTacMappIds.Remove(objLnkdTac);
                                    }
                                    #endregion

                                    lstTac_LinkTacMapping.Add(objTacLinkMapping);
                                }
                            }
                        }
                        tacticIdList = ListOfMarketoWithoutSFDC.Select(tac => tac.PlanTacticId).ToList();
                        #region "Validate Mappings with SFDC fields"
                        if (campaignList.Count > 0 || programList.Count > 0 || ListOfMarketoWithoutSFDC.Count > 0)
                        {

                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                            _isResultError = SetMappingDetails();
                            if (!_isResultError)
                            {
                                SyncEntityData<Plan_Campaign_Program_Tactic>(EntityType.Tactic.ToString(), ListOfMarketoWithoutSFDC, tacticIdList, ref lstProcessTacIds, lstTac_LinkTacMapping);
                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                            }
                            else
                            {
                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Error, "Set Mapping details.");
                            }
                        }
                        #endregion

                    }

                    #endregion

                }
                ListOfMarketoTactic.AddRange(ListOfMarketoWithoutSFDC);
            }
            if (StageLevel > 0)
            {
                ListOfMarketoTactic = ListOfMarketoTactic.Where(tac => tac.Stage.Level <= StageLevel && tac.IntegrationInstanceTacticId != null).Distinct().ToList();
            }
            return ListOfMarketoTactic;
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
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string published = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString();
            string EloquaCode = Enums.IntegrationType.Eloqua.ToString();
            try
            {
                // Insert log into IntegrationInstanceSection, Dharmraj PL#684
                int IntegrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), DateTime.Now, _userId);

                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Creating Eloqua & Model IntegrationInstanceId mapping list start.");
                //List<Plan> lstPlans = db.Plans.Where(p => p.Model.IntegrationInstanceIdCW == _integrationInstanceId && p.Model.Status.Equals(published)).ToList();

                #region "Check for pulling CW whether Model/Plan associated or not with current Instance"
                List<Model> lstModels = db.Models.Where(objmdl => objmdl.IntegrationInstanceIdCW == _integrationInstanceId && objmdl.Status.Equals("Published") && objmdl.IsActive == true).ToList();
                if (lstModels == null || lstModels.Count <= 0)
                {
                    // Save & display Message: No single Model associated with current Instance.
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Pull Closed Deals: There is no single Model associated with this Instance to pull CWs.", Enums.SyncStatus.Info, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull Closed Deals: There is no single Model associated with this Instance to pull CWs.");
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    return; // no error.
                }
                List<int> ModelIds = lstModels.Select(mdl => mdl.ModelId).ToList();
                List<Plan> lstPlans = db.Plans.Where(objplan => ModelIds.Contains(objplan.Model.ModelId) && objplan.IsActive == true).ToList();
                if (lstPlans == null || lstPlans.Count <= 0)
                {
                    // Save & display Message: No single Plan associated with current Instance.
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Pull Closed Deals: There is no single Plan associated with this Instance to pull CWs.", Enums.SyncStatus.Info, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull Closed Deals: There is no single Plan associated with this Instance to pull CWs.");
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    return; // no error.
                }
                #endregion

                Guid ClientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;

                //// Get Eloqua integration type Id.
                var eloquaIntegrationType = db.IntegrationTypes.Where(type => type.Code == EloquaCode && type.IsDeleted == false).Select(type => type.IntegrationTypeId).FirstOrDefault();
                int eloquaIntegrationTypeId = Convert.ToInt32(eloquaIntegrationType);

                //// Get All EloquaIntegrationTypeIds to retrieve  EloquaPlanIds.
                List<int> lstEloquaIntegrationTypeIds = db.IntegrationInstances.Where(instance => instance.IntegrationTypeId.Equals(eloquaIntegrationTypeId) && instance.IsDeleted.Equals(false) && instance.ClientId.Equals(ClientId)).Select(s => s.IntegrationInstanceId).ToList();

                //// Get all PlanIds whose Tactic data PUSH on Eloqua.
                //List<int> lstEloquaPlanIds = lstPlans.Where(objplan => lstEloquaIntegrationTypeIds.Contains(objplan.Model.IntegrationInstanceId.Value)).Select(objplan => objplan.PlanId).ToList();

                //// Get All PlanIds.
                List<int> AllplanIds = lstPlans.Select(objplan => objplan.PlanId).ToList();

                //// Get SalesForce PlanIds.
                //List<int> lstSalesForceplanIds = lstPlans.Where(objplan => !lstEloquaPlanIds.Contains(objplan.PlanId)).Select(plan => plan.PlanId).ToList();

                // Get List of status after Approved Status
                List<string> statusList = Common.GetStatusListAfterApproved();

                #region "Testing purpose code"
                //Plan_Campaign_Program_Tactic objTact = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId.Equals(44494)).FirstOrDefault();
                //objTact.IntegrationInstanceTacticId = string.Empty;
                //objTact.IntegrationInstanceEloquaId = "53180";
                //db.Entry(objTact).State = EntityState.Modified;
                //db.SaveChanges(); 
                #endregion

                List<Plan_Campaign_Program_Tactic> tblPlanTactic = new List<Plan_Campaign_Program_Tactic>();
                tblPlanTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeployedToIntegration == true &&
                                                                                                               statusList.Contains(tactic.Status) &&
                                                                                                               tactic.IsDeleted == false && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).ToList();

                //// Get All Approved,IsDeployedToIntegration true and IsDeleted false Tactic list.
                List<Plan_Campaign_Program_Tactic> lstAllTactics = tblPlanTactic.Where(tactic => AllplanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).ToList();

                //// Get list of EloquaIntegrationInstanceTacticID(EloquaId).
                List<EloquaIntegrationInstanceTactic_Model_Mapping> lstEloquaIntegrationInstanceTacticIds = lstAllTactics.Where(tactic => string.IsNullOrEmpty(tactic.IntegrationInstanceTacticId) && !string.IsNullOrEmpty(tactic.IntegrationInstanceEloquaId)).Select(_tac => new EloquaIntegrationInstanceTactic_Model_Mapping
                {
                    EloquaIntegrationInstanceTacticId = _tac.IntegrationInstanceEloquaId,
                    ModelIntegrationInstanceId = _tac.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceEloquaId.Value,
                    PlanTacticId = _tac.PlanTacticId
                }).ToList();
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Creating Eloqua & Model IntegrationInstanceId mapping list end.");

                if (lstEloquaIntegrationInstanceTacticIds == null)
                    lstEloquaIntegrationInstanceTacticIds = new List<EloquaIntegrationInstanceTactic_Model_Mapping>();

                //// Add IntegrationEloquaClient object to Mapping list for distinct ModelIntegrationInstanceId.
                //// Get Mapping List of SalesForceIntegrationInstanceTactic Ids(CRMIds) based on EloquaIntegrationInstanceTacticID(EloquaId).
                //List<CRM_EloquaMapping> lstSalesForceIntegrationInstanceTacticIds = new List<CRM_EloquaMapping>();
                Plan_Campaign_Program_Tactic objUpdTactic;
                Integration.Eloqua.IntegrationEloquaClient integrationEloquaClient;
                int cntrUpdate = 0;
                foreach (int _ModelIntegrationInstanceId in lstEloquaIntegrationInstanceTacticIds.Select(tac => tac.ModelIntegrationInstanceId).Distinct())
                {
                    try
                    {
                        integrationEloquaClient = new Integration.Eloqua.IntegrationEloquaClient(_ModelIntegrationInstanceId, 0, _entityType, _userId, _integrationInstanceLogId, _applicationId);
                        foreach (EloquaIntegrationInstanceTactic_Model_Mapping _EloquaTac in lstEloquaIntegrationInstanceTacticIds.Where(_eloqua => _eloqua.ModelIntegrationInstanceId.Equals(_ModelIntegrationInstanceId)))
                        {
                            if (!string.IsNullOrEmpty(_EloquaTac.EloquaIntegrationInstanceTacticId))
                            {

                                Integration.Eloqua.EloquaCampaign objEloqua = new Integration.Eloqua.EloquaCampaign();

                                ////Get SalesForceIntegrationTacticId based on EloquaIntegrationTacticId.
                                objEloqua = integrationEloquaClient.GetEloquaCampaign(_EloquaTac.EloquaIntegrationInstanceTacticId);
                                if (objEloqua != null && !string.IsNullOrEmpty(objEloqua.crmId))
                                {
                                    objUpdTactic = new Plan_Campaign_Program_Tactic();
                                    objUpdTactic = lstAllTactics.Where(s => s.PlanTacticId == _EloquaTac.PlanTacticId).FirstOrDefault();
                                    if (objUpdTactic != null)
                                    {
                                        objUpdTactic.IntegrationInstanceTacticId = objEloqua.crmId;
                                        db.Entry(objUpdTactic).State = EntityState.Modified;
                                        cntrUpdate++;
                                    }
                                    //var _Tactic = lstAllTactics.Where(s => s.IntegrationInstanceEloquaId == _EloquaTac.EloquaIntegrationInstanceTacticId).Select(_tac => _tac).FirstOrDefault();
                                    //lstSalesForceIntegrationInstanceTacticIds.Add(
                                    //                                          new CRM_EloquaMapping
                                    //                                          {
                                    //                                              CRMId = !string.IsNullOrEmpty(objEloqua.crmId) ? objEloqua.crmId : string.Empty,
                                    //                                              EloquaId = _EloquaTac.EloquaIntegrationInstanceTacticId,
                                    //                                              PlanTacticId = _Tactic != null ? _Tactic.PlanTacticId : 0,
                                    //                                              StartDate = _Tactic != null ? _Tactic.StartDate : (new DateTime()),
                                    //                                          });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _isResultError = true;
                        string exMessage = Common.GetInnermostException(ex);
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Pull Closed Deals: System error occurred while pulling EloquaId for Salesforce and Eloqua mapping. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while create mapping list of EloquaId and SalesforceId. Exception - " + exMessage);
                        continue;
                    }
                }
                if (cntrUpdate > 0)
                    db.SaveChanges();

                //// Get SalesForce tactic list
                List<Plan_Campaign_Program_Tactic> lstSalesForceTactic = lstAllTactics.Where(_tac => _tac.IntegrationInstanceTacticId != null).ToList();
                // Add By Nishant Sheth
                #region Plan To Salesforce Push and pull process for marketo instance
                List<int> MarketoModel = lstModels.Where(model => model.IntegrationInstanceMarketoID != null).Select(model => model.ModelId).ToList();
                List<Plan_Campaign_Program_Tactic> marketoTactic = new List<Plan_Campaign_Program_Tactic>();
                if (MarketoModel.Count > 0)
                {
                    marketoTactic = SyncSFDCMarketo(MarketoModel, IntegrationInstanceSectionId);
                }
                if (marketoTactic.Count > 0)
                {
                    lstSalesForceTactic.AddRange(marketoTactic);
                }
                #endregion

                string AllIntegrationTacticIds = String.Join("','", (from tactic in lstSalesForceTactic select tactic.IntegrationInstanceTacticId));

                //// Get Eloqua tactic list
                //List<Plan_Campaign_Program_Tactic> lstEloquaTactic = lstAllTactics.Where(tactic => string.IsNullOrEmpty(tactic.IntegrationInstanceTacticId) && !string.IsNullOrEmpty(tactic.IntegrationInstanceEloquaId)).ToList();
                //string SalesForceintegrationTacticIdsByEloquaId = String.Join("','", from _Tactic in lstEloquaTactic
                //                                                                     join _ElqTactic in lstSalesForceIntegrationInstanceTacticIds on _Tactic.IntegrationInstanceEloquaId equals _ElqTactic.EloquaId
                //                                                                     select _ElqTactic.CRMId);

                //// Merge SalesForce & Eloqua IntegrationTacticIds by comma(',').
                //string AllIntegrationTacticIds = !string.IsNullOrEmpty(SalesForceintegrationTacticIdsByEloquaId) ? (!string.IsNullOrEmpty(SalesForceintegrationTacticIds) ? (SalesForceintegrationTacticIds + "','" + SalesForceintegrationTacticIdsByEloquaId) : SalesForceintegrationTacticIdsByEloquaId) : SalesForceintegrationTacticIds;
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
                                int TotalPullCWCount = 0, ProcessedCWCount = 0;
                                string currentDate = DateTime.UtcNow.ToString(Common.DateFormatForSalesforce);
                                string cwsection = Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString();
                                string statusSuccess = StatusResult.Success.ToString();
                                var lastsync = isDoneFirstPullCW ? db.IntegrationInstanceSections.Where(instanceSection => instanceSection.IntegrationInstanceId == _integrationInstanceId && instanceSection.SectionName == cwsection && instanceSection.Status == statusSuccess).OrderByDescending(instancesection => instancesection.IntegrationInstanceSectionId).Select(instanceSection => instanceSection.SyncEnd).FirstOrDefault() :
                                                                    lstAllTactics.OrderByDescending(tactic => tactic.CreatedDate).Select(tactic => tactic.CreatedDate).FirstOrDefault();

                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Getting Contactlist from OpportunityContactRole Table within Salesforce start.");
                                string lastSyncDate = string.Empty;
                                if (lastsync != null)
                                {
                                    //lastsync = lastsync.Value.AddMinutes(-30);
                                    lastSyncDate = Convert.ToDateTime(lastsync).ToUniversalTime().ToString(Common.DateFormatForSalesforce);
                                }
                                string strCWStagename = string.Empty;
                                try
                                {
                                    strCWStagename = Common.GetClosedWonMappingField(_id);
                                }
                                catch (Exception ex)
                                {
                                    string exMessage = Common.GetInnermostException(ex);
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Please check your Closed Won field mapping value. Exception - " + exMessage);
                                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Pull Closed Deals: Please check your Closed Won field mapping value. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                }

                                if (lastSyncDate != string.Empty)
                                {
                                    opportunityGetQueryWhere = " WHERE " + StageName + "= '" + strCWStagename + "' AND " + LastModifiedDate + " > " + lastSyncDate + " AND " + LastModifiedDate + " < " + currentDate;
                                }
                                else
                                {
                                    opportunityGetQueryWhere = " WHERE " + StageName + "= '" + strCWStagename + "' AND " + LastModifiedDate + " < " + currentDate;
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
                                        if (jobj[Amount] != null && !string.IsNullOrEmpty(Convert.ToString(jobj[Amount])) && jobj[CloseDate] != null && !string.IsNullOrEmpty(Convert.ToString(jobj[CloseDate])))
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
                                        _isResultError = true;
                                        string exMessage = Common.GetInnermostException(e);
                                        errorcount++;
                                        _ErrorMessage = exMessage;
                                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting data from Opportunity table. Exception - " + exMessage);
                                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Pull Closed Deals: System error occurred while pulling data from Opportunity. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                        continue;
                                    }
                                    catch (Exception e)
                                    {
                                        _isResultError = true;
                                        string exMessage = Common.GetInnermostException(e);
                                        errorcount++;
                                        _ErrorMessage = exMessage;
                                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting data from Opportunity table. Exception - " + exMessage);
                                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Pull Closed Deals: System error occurred while pulling data from Opportunity. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                        continue;
                                    }

                                }
                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Getting Contactlist from OpportunityContactRole Table within Salesforce end.");

                                if (cwRecords.Count > 0 && errorcount > 0)
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

                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Getting CampaignList from CampaignMember Table within Salesforce start.");
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
                                                if (jobj[ResponseDate] != null && !string.IsNullOrEmpty(Convert.ToString(jobj[ResponseDate])))
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
                                            string exMessage = Common.GetInnermostException(e);
                                            errorcount++;
                                            string TacticId = Convert.ToString(jobj[CampaignId]);
                                            if (TacticId != string.Empty)
                                            {

                                                //// check whether TacticId(CRMId) exist in field IntegrationInstanceTacticID field of SalesForceTactic list.
                                                var tactic = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
                                                if (tactic != null)
                                                    _PlanTacticId = tactic.PlanTacticId;
                                                //else                                        //// if Tactic not exist then retrieve PlanTacticId from EloquaTactic list.
                                                //    _PlanTacticId = lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && (_SalTac.CRMId == TacticId || _SalTac.CRMId == TacticId.Substring(0, 15))).Select(s => s.PlanTacticId).FirstOrDefault();
                                                if (_PlanTacticId != 0)
                                                {
                                                    ErrorFlag = true;
                                                    _ErrorMessage = exMessage;
                                                    IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                                    instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                                    instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                                    instanceTactic.EntityId = _PlanTacticId;
                                                    instanceTactic.EntityType = EntityType.Tactic.ToString();
                                                    instanceTactic.Status = StatusResult.Error.ToString();
                                                    instanceTactic.Operation = Operation.Pull_ClosedWon.ToString();
                                                    instanceTactic.SyncTimeStamp = DateTime.Now;
                                                    instanceTactic.CreatedDate = DateTime.Now;
                                                    instanceTactic.ErrorDescription = Common.CampaignMemberObjectError + exMessage;
                                                    instanceTactic.CreatedBy = _userId;
                                                    db.Entry(instanceTactic).State = EntityState.Added;
                                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting Campaign from CampaignMember table within Salesforce. Exception - " + exMessage);
                                                }
                                            }
                                            _isResultError = true;
                                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Pull Closed Deals: System error occurred while pulling Campaign data. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                            continue;
                                        }
                                        catch (Exception e)
                                        {
                                            errorcount++;
                                            string exMessage = Common.GetInnermostException(e);
                                            string TacticId = Convert.ToString(jobj[CampaignId]);
                                            if (TacticId != string.Empty)
                                            {

                                                //// check whether TacticId(CRMId) exist in field IntegrationInstanceTacticID field of SalesForceTactic list.
                                                var tactic = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == TacticId);
                                                if (tactic != null)
                                                    _PlanTacticId = tactic.PlanTacticId;
                                                //else                                        //// if Tactic not exist then retrieve PlanTacticId from EloquaTactic list.
                                                //    _PlanTacticId = lstSalesForceIntegrationInstanceTacticIds.Where(_SalTac => _SalTac.CRMId != null && (_SalTac.CRMId == TacticId || _SalTac.CRMId == TacticId.Substring(0, 15))).Select(s => s.PlanTacticId).FirstOrDefault();
                                                if (_PlanTacticId != 0)
                                                {
                                                    ErrorFlag = true;
                                                    _ErrorMessage = exMessage;
                                                    IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                                    instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                                    instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                                                    instanceTactic.EntityId = _PlanTacticId;
                                                    instanceTactic.EntityType = EntityType.Tactic.ToString();
                                                    instanceTactic.Status = StatusResult.Error.ToString();
                                                    instanceTactic.Operation = Operation.Pull_ClosedWon.ToString();
                                                    instanceTactic.SyncTimeStamp = DateTime.Now;
                                                    instanceTactic.CreatedDate = DateTime.Now;
                                                    instanceTactic.ErrorDescription = Common.CampaignMemberObjectError + exMessage;
                                                    instanceTactic.CreatedBy = _userId;
                                                    db.Entry(instanceTactic).State = EntityState.Added;
                                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting Campaign from CampaignMember table within Salesforce. Exception: " + exMessage);
                                                }
                                            }
                                            _isResultError = true;
                                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Pull Closed Deals: System error occurred while pulling Campaign data. Exception: " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                            continue;
                                        }
                                    }
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Getting CampaignList from CampaignMember Table within Salesforce end.");

                                    if (!ErrorFlag)
                                    {
                                        //Get Contact-CampaignMember list with last responded date 
                                        ContactCampaignMemberList = (from element in ContactCampaignMemberList
                                                                     //group element by new { element.ContactId, element.CampaignId }   
                                                                     group element by new { element.ContactId }                          // Modified for PL ticket #2026 - Get most recent responded CampaignMember
                                                                         into groups
                                                                         select groups.OrderByDescending(p => p.RespondedDate).First()).ToList();

                                        List<OpportunityMember> OpportunityMemberList = new List<OpportunityMember>();
                                        OpportunityMemberList = (from om in OpportunityMemberListInitial
                                                                 join crm in ContactRoleList on om.OpportunityId equals crm.OpportunityId
                                                                 //join ccml in ContactCampaignMemberList on new {crm.ContactId,om.CampaignId} equals new {ccml.ContactId,ccml.CampaignId}    // Modified for PL ticket #2026 - filter Opportunity member list with Campaign Id.
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
                                        //List<Plan_Campaign_Program_Tactic> lstCRM_EloquaTactics = (from _Tactic in lstEloquaTactic
                                        //                                                           join _ElqTactic in lstSalesForceIntegrationInstanceTacticIds on _Tactic.IntegrationInstanceEloquaId equals _ElqTactic.EloquaId
                                        //                                                           where _ElqTactic.CRMId != null
                                        //                                                           select _Tactic).ToList();

                                        //// Merge SalesForce & Eloqua Tactic list.
                                        List<Plan_Campaign_Program_Tactic> lstMergedTactics = lstSalesForceTactic;
                                        //lstCRM_EloquaTactics.ForEach(_elqTactic => lstMergedTactics.Add(_elqTactic));
                                        lstMergedTactics = lstMergedTactics.Distinct().ToList();

                                        if (OpportunityMemberList.Count > 0)
                                        {
                                            lstSalesForceTactic = lstSalesForceTactic.OrderBy(tac => tac.PlanTacticId).ToList(); // Add By Nishant Sheth // For pull inq, mql, cw #2188
                                            var OpportunityMemberListGroup = OpportunityMemberList.GroupBy(cl => new { CampaignId = cl.CampaignId, Month = cl.CloseDate.ToString("MM/yyyy") }).Select(cl =>
                                                new
                                                {
                                                    CampaignId = cl.Key.CampaignId,
                                                    TacticId = lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null ? (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).PlanTacticId) : 0,
                                                    Period = (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null) && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year < Convert.ToDateTime(cl.Key.Month).Year) ? "Y" + (((Convert.ToDateTime(cl.Key.Month).Year - (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year)) * 12) + Convert.ToDateTime(cl.Key.Month).Month) : "Y" + Convert.ToDateTime(cl.Key.Month).Month,
                                                    IsYear = (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId) != null && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).StartDate.Year <= Convert.ToDateTime(cl.Key.Month).Year) && (lstSalesForceTactic.FirstOrDefault(t => t.IntegrationInstanceTacticId == cl.Key.CampaignId).EndDate.Year >= Convert.ToDateTime(cl.Key.Month).Year)) ? true : false,
                                                    Count = cl.Count(),
                                                    Revenue = cl.Sum(c => c.Amount)
                                                }).Where(om => om.IsYear).ToList();

                                            var tacticidactual = OpportunityMemberListGroup.Select(opptactic => opptactic.TacticId).Distinct().ToList();

                                            //Set count of total pulled responses from Salesforce.
                                            TotalPullCWCount = OpportunityMemberListGroup != null ? OpportunityMemberListGroup.Sum(cnt => cnt.Count) : 0;

                                            #region "Create LinkedTactic Mapping list"
                                            Dictionary<int, int> lstlinkedTacticMapping = new Dictionary<int, int>();
                                            int linkedTacticId = 0;
                                            foreach (var tac in lstMergedTactics)
                                            {
                                                linkedTacticId = 0;
                                                #region "Retrieve linkedTactic"
                                                linkedTacticId = (tac != null && tac.LinkedTacticId.HasValue) ? tac.LinkedTacticId.Value : 0;
                                                //if (linkedTacticId <= 0)
                                                //{
                                                //    var lnkPCPT = tblPlanTactic.Where(tactic => tactic.LinkedTacticId == tac.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                                                //    linkedTacticId = lnkPCPT != null ? lnkPCPT.PlanTacticId : 0;
                                                //}

                                                if (linkedTacticId > 0)
                                                {
                                                    lstlinkedTacticMapping.Add(tac.PlanTacticId, linkedTacticId);
                                                }
                                                #endregion
                                            }
                                            #endregion

                                            List<Plan_Campaign_Program_Tactic_Actual> tblPlanActual = new List<Plan_Campaign_Program_Tactic_Actual>();
                                            List<int> LinkedTacticActualsId = null;
                                            if (lstlinkedTacticMapping.Count > 0)
                                            {
                                                LinkedTacticActualsId = new List<int>();
                                                LinkedTacticActualsId = lstlinkedTacticMapping.Where(tac => tacticidactual.Contains(tac.Key)).Select(tac => tac.Value).ToList();
                                                tblPlanActual = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => (tacticidactual.Contains(actual.PlanTacticId) || LinkedTacticActualsId.Contains(actual.PlanTacticId)) && (actual.StageTitle == Common.StageRevenue || actual.StageTitle == Common.StageCW)).ToList();
                                            }
                                            else
                                            {
                                                tblPlanActual = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => tacticidactual.Contains(actual.PlanTacticId) && (actual.StageTitle == Common.StageRevenue || actual.StageTitle == Common.StageCW)).ToList();
                                            }

                                            List<Plan_Campaign_Program_Tactic_Actual> OuteractualTacticList = tblPlanActual.Where(actual => tacticidactual.Contains(actual.PlanTacticId)).ToList();
                                            List<Plan_Campaign_Program_Tactic_Actual> LinkedActualTacticList = null;
                                            List<int> lstMultiLinkedTactic = new List<int>();
                                            if (!isDoneFirstPullCW && tblPlanActual != null && tblPlanActual.Count > 0)
                                            {

                                                try
                                                {
                                                    db.Configuration.AutoDetectChangesEnabled = false;
                                                    OuteractualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                                }
                                                finally
                                                {
                                                    db.Configuration.AutoDetectChangesEnabled = true;
                                                }
                                                if (LinkedTacticActualsId != null && LinkedTacticActualsId.Count > 0)
                                                {
                                                    LinkedActualTacticList = tblPlanActual.Where(actual => LinkedTacticActualsId.Contains(actual.PlanTacticId)).ToList();
                                                    if (LinkedActualTacticList != null && LinkedActualTacticList.Count > 0)
                                                    {
                                                        //LinkedActualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);

                                                        // Remove linked Tactic's Actuals.
                                                        List<int> linkedTactics = new List<int>();
                                                        linkedTactics = LinkedActualTacticList.Select(actl => actl.PlanTacticId).ToList();
                                                        #region "Get list of linked Tactics that multiyear or not"
                                                        Plan_Campaign_Program_Tactic objLnkTac = null;
                                                        int yeardiff = 0, cntr = 0, perdNum = 12; bool isMultilinkedTactic = false;
                                                        List<Plan_Campaign_Program_Tactic_Actual> linkedactualTacticList = null;
                                                        List<string> lstLinkedPeriods = new List<string>();
                                                        foreach (int linkdTacId in linkedTactics)
                                                        {
                                                            try
                                                            {
                                                                objLnkTac = new Plan_Campaign_Program_Tactic();
                                                                objLnkTac = tblPlanTactic.Where(tac => tac.PlanTacticId == linkdTacId).FirstOrDefault();
                                                                if (objLnkTac != null)
                                                                {
                                                                    yeardiff = objLnkTac.EndDate.Year - objLnkTac.StartDate.Year;
                                                                    isMultilinkedTactic = yeardiff > 0 ? true : false;
                                                                    linkedactualTacticList = new List<Plan_Campaign_Program_Tactic_Actual>();
                                                                    if (isMultilinkedTactic)
                                                                    {
                                                                        // remove linked tactic respective months actuals data.
                                                                        lstLinkedPeriods = new List<string>();
                                                                        cntr = 12 * yeardiff;
                                                                        for (int i = 1; i <= cntr; i++)
                                                                        {
                                                                            lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
                                                                        }
                                                                        lstMultiLinkedTactic.Add(linkdTacId);
                                                                        linkedactualTacticList = LinkedActualTacticList.Where(actual => linkdTacId == actual.PlanTacticId && lstLinkedPeriods.Contains(actual.Period)).ToList();
                                                                        if (linkedactualTacticList != null && linkedactualTacticList.Count > 0)
                                                                        {
                                                                            try
                                                                            {
                                                                                db.Configuration.AutoDetectChangesEnabled = false;
                                                                                linkedactualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                                                            }
                                                                            finally
                                                                            {
                                                                                db.Configuration.AutoDetectChangesEnabled = true;
                                                                            }
                                                                        }

                                                                    }
                                                                    else
                                                                    {
                                                                        linkedactualTacticList = LinkedActualTacticList.Where(actual => linkdTacId == actual.PlanTacticId).ToList();
                                                                        if (linkedactualTacticList != null && linkedactualTacticList.Count > 0)
                                                                        {
                                                                            try
                                                                            {
                                                                                db.Configuration.AutoDetectChangesEnabled = false;
                                                                                linkedactualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                                                            }
                                                                            finally
                                                                            {
                                                                                db.Configuration.AutoDetectChangesEnabled = true;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Error occurred on remove linked tactic : Linked TacticId-" + linkdTacId.ToString() + Common.GetInnermostException(ex));
                                                            }
                                                        }
                                                        #endregion

                                                        //List<Plan_Campaign_Program_Tactic_Actual> linkedactualTacticList = tblActuals.Where(actual => linkedTactics.Contains(actual.PlanTacticId)).ToList();
                                                        //linkedactualTacticList.ForEach(actual => db.Entry(actual).State = EntityState.Deleted);
                                                    }
                                                }
                                                db.SaveChanges();
                                            }

                                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Create PlanTacticActual list and insert Tactic log.");
                                            lstMergedTactics = lstMergedTactics.Where(lstmerge => tacticidactual.Contains(lstmerge.PlanTacticId)).Distinct().ToList();
                                            try
                                            {
                                                db.Configuration.AutoDetectChangesEnabled = false;
                                                foreach (var tactic in lstMergedTactics)
                                                {
                                                    try
                                                    {
                                                        var innerOpportunityMember = OpportunityMemberListGroup.Where(cm => cm.TacticId == tactic.PlanTacticId).ToList();
                                                        int lnkdTacId = 0;
                                                        if (lstlinkedTacticMapping != null && lstlinkedTacticMapping.Count > 0)
                                                            lnkdTacId = lstlinkedTacticMapping.FirstOrDefault(tac => tac.Key == tactic.PlanTacticId).Value;

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
                                                                ProcessedCWCount = ProcessedCWCount + objOpportunityMember.Count;
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

                                                            #region "Add/Update  Linked Tactic Actuals"
                                                            if (lnkdTacId > 0)
                                                            {
                                                                LinkedActualTacticList = tblPlanActual.Where(actual => actual.PlanTacticId == lnkdTacId).ToList();
                                                                string orgPeriod = objOpportunityMember.Period;
                                                                string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                                                int NumPeriod = int.Parse(numPeriod), yearDiff = 1;
                                                                string lnkePeriod = string.Empty;
                                                                //int yearDiff = linkedTactic.EndDate.Year - linkedTactic.StartDate.Year;
                                                                if (lstMultiLinkedTactic.ToList().Any(tacId => tacId == lnkdTacId)) //Is linked tactic Multiyear
                                                                    lnkePeriod = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();   // (12*1)+3 = 15 => For March(Y15) month. 
                                                                else
                                                                {
                                                                    if (NumPeriod > 12)
                                                                    {
                                                                        int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
                                                                        int div = NumPeriod / 12;    // In case of 24, Y12.
                                                                        if (rem > 0 || div > 1)
                                                                        {
                                                                            lnkePeriod = PeriodChar + (div > 1 ? "12" : rem.ToString());                            // For March, Y3(i.e 15%12 = 3)     
                                                                        }
                                                                    }
                                                                    //lnkePeriod = 
                                                                }

                                                                if (!string.IsNullOrEmpty(lnkePeriod))
                                                                {
                                                                    var lnkdActualCW = LinkedActualTacticList.FirstOrDefault(tacticActual => tacticActual.PlanTacticId == lnkdTacId && tacticActual.Period == lnkePeriod && tacticActual.StageTitle == Common.StageCW);
                                                                    if (lnkdActualCW != null && isDoneFirstPullCW)
                                                                    {
                                                                        lnkdActualCW.Actualvalue = innertacticactualcw.Actualvalue + objOpportunityMember.Count;
                                                                        lnkdActualCW.ModifiedDate = DateTime.Now;
                                                                        lnkdActualCW.ModifiedBy = _userId;
                                                                        db.Entry(lnkdActualCW).State = EntityState.Modified;
                                                                    }
                                                                    else
                                                                    {
                                                                        Plan_Campaign_Program_Tactic_Actual objlnkdTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                                                        objlnkdTacticActual.PlanTacticId = lnkdTacId;
                                                                        objlnkdTacticActual.Period = lnkePeriod;
                                                                        objlnkdTacticActual.StageTitle = Common.StageCW;
                                                                        objlnkdTacticActual.Actualvalue = objOpportunityMember.Count;
                                                                        objlnkdTacticActual.CreatedBy = _userId;
                                                                        objlnkdTacticActual.CreatedDate = DateTime.Now;
                                                                        db.Entry(objlnkdTacticActual).State = EntityState.Added;
                                                                    }

                                                                    var lnkdActualRevenue = LinkedActualTacticList.FirstOrDefault(tacticActual => tacticActual.PlanTacticId == lnkdTacId && tacticActual.Period == lnkePeriod && tacticActual.StageTitle == Common.StageRevenue);
                                                                    if (lnkdActualRevenue != null && isDoneFirstPullCW)
                                                                    {
                                                                        lnkdActualRevenue.Actualvalue = innertacticactualrevenue.Actualvalue + objOpportunityMember.Revenue;
                                                                        lnkdActualRevenue.ModifiedDate = DateTime.Now;
                                                                        lnkdActualRevenue.ModifiedBy = _userId;
                                                                        db.Entry(lnkdActualRevenue).State = EntityState.Modified;
                                                                    }
                                                                    else
                                                                    {
                                                                        Plan_Campaign_Program_Tactic_Actual objlnkdTacticActualRevenue = new Plan_Campaign_Program_Tactic_Actual();
                                                                        objlnkdTacticActualRevenue.PlanTacticId = lnkdTacId;
                                                                        objlnkdTacticActualRevenue.Period = lnkePeriod;
                                                                        objlnkdTacticActualRevenue.StageTitle = Common.StageRevenue;
                                                                        objlnkdTacticActualRevenue.Actualvalue = objOpportunityMember.Revenue;
                                                                        objlnkdTacticActualRevenue.CreatedBy = _userId;
                                                                        objlnkdTacticActualRevenue.CreatedDate = DateTime.Now;
                                                                        db.Entry(objlnkdTacticActualRevenue).State = EntityState.Added;
                                                                    }
                                                                }
                                                            }
                                                            #endregion
                                                        }

                                                        tactic.LastSyncDate = DateTime.Now;
                                                        //tactic.ModifiedDate = DateTime.Now;
                                                        tactic.ModifiedBy = _userId;

                                                        // Update linked Tactic LastSyncDate,ModifiedDate & ModifiedBy.
                                                        if (lnkdTacId > 0) // check whether linkedTactics exist or not.
                                                        {
                                                            Plan_Campaign_Program_Tactic objLinkedTactic = new Plan_Campaign_Program_Tactic();
                                                            objLinkedTactic = tblPlanTactic.Where(tac => tac.PlanTacticId == lnkdTacId).FirstOrDefault();
                                                            if (objLinkedTactic != null)
                                                            {
                                                                objLinkedTactic.LastSyncDate = DateTime.Now;
                                                                //objLinkedTactic.ModifiedDate = DateTime.Now;
                                                                objLinkedTactic.ModifiedBy = _userId;
                                                            }
                                                        }

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
                                                    catch (Exception ex)
                                                    {
                                                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Error occurred on insert CW on Actual table : TacticId-" + tactic.PlanTacticId.ToString() + Common.GetInnermostException(ex));
                                                    }
                                                }
                                            }
                                            finally
                                            {
                                                db.Configuration.AutoDetectChangesEnabled = true;
                                            }
                                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Create or Modified PlanTacticActual record and insert Tactic log.");
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
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull Closed Deals: Total CW - " + TotalPullCWCount.ToString() + ", " + ProcessedCWCount.ToString() + " CW(s) were processed and pulled in database.");
                                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Closed Deals: Total CW - " + TotalPullCWCount.ToString() + ", " + ProcessedCWCount.ToString() + " CW(s) were processed and pulled in database.", Enums.SyncStatus.Info, DateTime.Now));
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
                                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Pull Closed Deals: " + Common.msgMappingNotFoundForSalesforcePullCW, Enums.SyncStatus.Error, DateTime.Now));
                            }
                        }
                        else
                        {
                            _isResultError = true;
                            // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                            Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgMappingNotFoundForSalesforcePullCW);
                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Pull Closed Deals: " + Common.msgMappingNotFoundForSalesforcePullCW, Enums.SyncStatus.Error, DateTime.Now));

                            // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                            //Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                        }
                    }
                    catch (SalesforceException e)
                    {
                        _isResultError = true;
                        string exMessage = Common.GetInnermostException(e);
                        // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, exMessage);
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Pull Closed Deals: Error occurred while Pulling Closed Deals from Salesforce. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    }
                    catch (Exception e)
                    {
                        _isResultError = true;
                        string exMessage = Common.GetInnermostException(e);
                        // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, exMessage);
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Pull Closed Deals: Error occurred while Pulling Closed Deals from Salesforce. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    }
                }
                else
                {
                    // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                }
            }
            catch (Exception ex)
            {
                _isResultError = true;
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while Pulling Closed Deals. Exception - " + exMessage);
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullClosedDeals.ToString(), "Error occurred while Pulling Closed Deals. Exception - " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
            }
        }

        private class ImportCostMember
        {
            public string CampaignId { get; set; }
            public double actualCost { get; set; }
        }

        /// <summary>
        /// Added by Mitesh Vaishnav on 28-07-2015
        /// To Identify data mismatch between salesforce target field and gp source field
        /// </summary>
        /// <param name="lstSourceTargetMapping">list of configured field mapping for source field and target field </param>
        /// <param name="lstSalesForceFieldDetails">sales force target field details with data type</param>
        /// <param name="Section">list of mis matched source field and target field</param>
        /// <returns></returns>
        private List<SalesForceObjectFieldDetails> IdentifyDataTypeMisMatch(Dictionary<string, string> lstSourceTargetMapping, List<SalesForceObjectFieldDetails> lstSalesForceFieldDetails, string Section)
        {
            List<SalesForceObjectFieldDetails> lstMappingMisMatch = new List<SalesForceObjectFieldDetails>();
            SalesForceObjectFieldDetails objfeildDetails;
            string strCustmName;
            int custId = 0;
            foreach (KeyValuePair<string, string> entry in lstSourceTargetMapping)
            {
                if (Enums.ActualFieldDatatype.ContainsKey(entry.Key) && lstSalesForceFieldDetails.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault() != null)
                {
                    if (!Enums.ActualFieldDatatype[entry.Key].Contains(lstSalesForceFieldDetails.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault().TargetDatatype))
                    {
                        objfeildDetails = new SalesForceObjectFieldDetails();
                        objfeildDetails.SourceField = entry.Key;
                        objfeildDetails.TargetField = entry.Value;
                        objfeildDetails.Section = Section;
                        objfeildDetails.TargetDatatype = lstSalesForceFieldDetails.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault().TargetDatatype;
                        objfeildDetails.SourceDatatype = Enums.ActualFieldDatatype[entry.Key].ToString();
                        lstMappingMisMatch.Add(objfeildDetails);
                    }

                }
                else if (lstSalesForceFieldDetails.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault() != null)
                {
                    if (!Enums.ActualFieldDatatype[Enums.ActualFields.Other.ToString()].Contains(lstSalesForceFieldDetails.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault().TargetDatatype))
                    {
                        objfeildDetails = new SalesForceObjectFieldDetails();
                        strCustmName = string.Empty;
                        custId = 0;
                        if(int.TryParse(entry.Key,out custId))
                        {
                            strCustmName = db.CustomFields.Where(custm => custm.CustomFieldId == custId && custm.IsDeleted == false).Select(v => v.Name).FirstOrDefault();
                        }
                        objfeildDetails.SourceField = string.IsNullOrEmpty(strCustmName) ? entry.Key : strCustmName;
                        objfeildDetails.TargetField = entry.Value;
                        objfeildDetails.Section = Section;
                        objfeildDetails.TargetDatatype = lstSalesForceFieldDetails.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault().TargetDatatype;
                        objfeildDetails.SourceDatatype = Enums.ActualFieldDatatype[Enums.ActualFields.Other.ToString()].ToString();
                        lstMappingMisMatch.Add(objfeildDetails);
                    }
                }
            }
            return lstMappingMisMatch;
        }

        /// <summary>
        /// Function to set mapping details.
        /// modified by Mitesh vaishnav : convert return type void to bool 
        /// if mapping has not data type mismatch and not any other exception then it returns false else true.
        /// </summary>
        private bool SetMappingDetails()
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
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997

            try
            {

                // Start - Modified by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                List<IntegrationInstanceDataTypeMapping> dataTypeMapping = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.IntegrationInstanceId.Equals(_integrationInstanceId)).ToList();
                if (!dataTypeMapping.Any()) // check if there is no field mapping configure then log error to IntegrationInstanceLogDetails table.
                {
                    Enums.EntityType _entityTypeSection = (Enums.EntityType)Enum.Parse(typeof(Enums.EntityType), Convert.ToString(_entityType), true);
                    _ErrorMessage = "You have not configure any single field with Salesforce field.";
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, _entityTypeSection, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), _ErrorMessage, Enums.SyncStatus.Error, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "You have not configure any single field with Salesforce field.");
                    return true;    // return true value that means error exist and do not proceed for the further mapping list.
                }
                lstSalesforceFieldDetail = GetTargetDataTypeWithLengthAndDatatype(Common.SalesForceCampaignObject.ToString());

                List<SalesForceObjectFieldDetails> lstMappingMisMatch = new List<SalesForceObjectFieldDetails>();

                _mappingTactic = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign_Program_Tactic
                                                        || gameplandata.GameplanDataType.TableName == Global) : gameplandata.CustomField.EntityType == Tactic_EntityType) &&
                                                        (gameplandata.GameplanDataType != null ? !gameplandata.GameplanDataType.IsGet : true))
                                                    .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                    .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                lstMappingMisMatch = IdentifyDataTypeMisMatch(_mappingTactic, lstSalesforceFieldDetail, Enums.EntityType.Tactic.ToString());


                _mappingProgram = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign_Program
                                                        || gameplandata.GameplanDataType.TableName == Global) : gameplandata.CustomField.EntityType == Program_EntityType) &&
                                                        (gameplandata.GameplanDataType != null ? !gameplandata.GameplanDataType.IsGet : true))
                                                    .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                    .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                lstMappingMisMatch = lstMappingMisMatch.Concat(IdentifyDataTypeMisMatch(_mappingProgram, lstSalesforceFieldDetail, Enums.EntityType.Program.ToString())).ToList();


                _mappingCampaign = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign
                                                        || gameplandata.GameplanDataType.TableName == Global) : gameplandata.CustomField.EntityType == Campaign_EntityType) &&
                                                        (gameplandata.GameplanDataType != null ? !gameplandata.GameplanDataType.IsGet : true))
                                                    .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                    .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                lstMappingMisMatch = lstMappingMisMatch.Concat(IdentifyDataTypeMisMatch(_mappingCampaign, lstSalesforceFieldDetail, Enums.EntityType.Campaign.ToString())).ToList();

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
                lstMappingMisMatch = lstMappingMisMatch.Concat(IdentifyDataTypeMisMatch(_mappingImprovementTactic, lstSalesforceFieldDetail, Enums.EntityType.ImprovementTactic.ToString())).ToList();



                _mappingImprovementProgram = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType.TableName == Plan_Improvement_Campaign_Program
                                                                || (gameplandata.GameplanDataType.TableName == Global && gameplandata.GameplanDataType.IsImprovement == true)) &&
                                                                    !gameplandata.GameplanDataType.IsGet)
                                               .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                               .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                lstMappingMisMatch = lstMappingMisMatch.Concat(IdentifyDataTypeMisMatch(_mappingImprovementProgram, lstSalesforceFieldDetail, Enums.EntityType.ImprovementProgram.ToString())).ToList();



                _mappingImprovementCampaign = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType.TableName == Plan_Improvement_Campaign
                                                                || (gameplandata.GameplanDataType.TableName == Global && gameplandata.GameplanDataType.IsImprovement == true)) &&
                                                                    !gameplandata.GameplanDataType.IsGet)
                                               .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                               .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                lstMappingMisMatch = lstMappingMisMatch.Concat(IdentifyDataTypeMisMatch(_mappingImprovementCampaign, lstSalesforceFieldDetail, Enums.EntityType.ImprovementCampaign.ToString())).ToList();
                // End - Modified by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997

                #region "Remove mismatch record from  Mapping list"
                foreach (SalesForceObjectFieldDetails objMisMatchItem in lstMappingMisMatch)
                {
                    if (objMisMatchItem.Section.Equals(Enums.EntityType.Tactic.ToString()))
                    {
                        _mappingTactic.Remove(objMisMatchItem.SourceField);
                    }
                    else if (objMisMatchItem.Section.Equals(Enums.EntityType.Program.ToString()))
                    {
                        _mappingProgram.Remove(objMisMatchItem.SourceField);
                    }
                    else if (objMisMatchItem.Section.Equals(Enums.EntityType.Campaign.ToString()))
                    {
                        _mappingCampaign.Remove(objMisMatchItem.SourceField);
                    }
                    else if (objMisMatchItem.Section.Equals(Enums.EntityType.ImprovementTactic.ToString()))
                    {
                        _mappingImprovementTactic.Remove(objMisMatchItem.SourceField);
                    }
                    else if (objMisMatchItem.Section.Equals(Enums.EntityType.ImprovementProgram.ToString()))
                    {
                        _mappingImprovementProgram.Remove(objMisMatchItem.SourceField);
                    }
                    else if (objMisMatchItem.Section.Equals(Enums.EntityType.ImprovementCampaign.ToString()))
                    {
                        _mappingImprovementCampaign.Remove(objMisMatchItem.SourceField);
                    }

                }
                #endregion

                foreach (var Section in lstMappingMisMatch.Select(m => m.Section).Distinct().ToList())
                {
                    string msg = "Data type mismatch for " +
                                string.Join(",", lstMappingMisMatch.Where(m => m.Section == Section).Select(m => m.SourceField).ToList()) +
                                " in salesforce for " + Section + ".";
                    Enums.EntityType entityTypeSection = (Enums.EntityType)Enum.Parse(typeof(Enums.EntityType), Section, true);
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, entityTypeSection, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), msg, Enums.SyncStatus.Info, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, msg);
                }

                try
                {
                    BDSService.BDSServiceClient objBDSservice = new BDSService.BDSServiceClient();
                    _mappingUser = objBDSservice.GetUserListByClientId(_clientId).Select(u => new { u.UserId, u.FirstName, u.LastName }).ToDictionary(u => u.UserId, u => u.FirstName + " " + u.LastName);

                    if (_CustomNamingPermissionForInstance)
                    {
                        // Get sequence for custom name of tactic
                        SequencialOrderedTableList = db.CampaignNameConventions.Where(c => c.ClientId == _clientId && c.IsDeleted == false).OrderBy(c => c.Sequence).ToList();
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

                    return false;
                }
                catch (Exception ex)
                {
                    string exMessage = Common.GetInnermostException(ex);
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while executing BDS Service:- " + exMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting Mapping list:- " + exMessage);
                return false;
            }

        }

        private Plan_Campaign SyncCampaingData(Plan_Campaign planCampaign, ref StringBuilder sbMessage)
        {
            StringBuilder sb = new StringBuilder();
            Enums.Mode currentMode = Common.GetMode(planCampaign.IntegrationInstanceCampaignId);
            PlanName = planCampaign.Plan.Title;
            // TODO: Common below code for Update & Create state. 
            IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
            instanceLogCampaign.IntegrationInstanceSectionId = _integrationInstanceSectionId;
            instanceLogCampaign.IntegrationInstanceId = _integrationInstanceId;
            instanceLogCampaign.EntityId = planCampaign.PlanCampaignId;
            instanceLogCampaign.EntityType = EntityType.Campaign.ToString();
            if (currentMode.Equals(Enums.Mode.Create))
            {
                instanceLogCampaign.Operation = Operation.Create.ToString();
                instanceLogCampaign.SyncTimeStamp = DateTime.Now;
                try
                {
                    planCampaign.IntegrationInstanceCampaignId = CreateCampaign(planCampaign);
                    planCampaign.LastSyncDate = DateTime.Now;
                    planCampaign.ModifiedDate = DateTime.Now;
                    planCampaign.ModifiedBy = _userId;
                    instanceLogCampaign.Status = StatusResult.Success.ToString();

                    #region "Add Campaign Synced comment to Plan_Campaign_Program_Tactic_Comment table"
                    //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                    if (!Common.IsAutoSync)
                    {
                        Plan_Campaign_Program_Tactic_Comment objCampaignComment = new Plan_Campaign_Program_Tactic_Comment();
                        objCampaignComment.PlanCampaignId = planCampaign.PlanCampaignId;
                        objCampaignComment.Comment = Common.CampaignSyncedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                        objCampaignComment.CreatedDate = DateTime.Now;
                        ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                        //if (Common.IsAutoSync)
                        //{
                        //    objCampaignComment.CreatedBy = new Guid();
                        //}
                        //else
                        //{
                        objCampaignComment.CreatedBy = this._userId;
                        //}
                        db.Entry(objCampaignComment).State = EntityState.Added;
                        db.Plan_Campaign_Program_Tactic_Comment.Add(objCampaignComment);
                    }
                    #endregion

                    sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Campaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Create.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                }
                catch (SalesforceException e)
                {
                    _isResultError = true;
                    string exMessage = "System error occurred while creating campaign \"" + planCampaign.Title + "\": " + Common.GetInnermostException(e);
                    sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                    instanceLogCampaign.Status = StatusResult.Error.ToString();
                    instanceLogCampaign.ErrorDescription = exMessage;
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Campaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                }
                catch (Exception)
                {
                    _isResultError = true;
                    string exMessage = "System error occurred while creating campaign \"" + planCampaign.Title + "\".";
                    sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                    instanceLogCampaign.Status = StatusResult.Error.ToString();
                    instanceLogCampaign.ErrorDescription = exMessage;
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Campaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                }


            }
            else if (currentMode.Equals(Enums.Mode.Update))
            {
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

                        #region "Add Campaign Synced comment to Plan_Campaign_Program_Tactic_Comment table"
                        //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                        if (!Common.IsAutoSync)
                        {
                            Plan_Campaign_Program_Tactic_Comment objCampaignComment = new Plan_Campaign_Program_Tactic_Comment();
                            objCampaignComment.PlanCampaignId = planCampaign.PlanCampaignId;
                            objCampaignComment.Comment = Common.CampaignUpdatedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                            objCampaignComment.CreatedDate = DateTime.Now;
                            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                            //if (Common.IsAutoSync)
                            //{
                            //    objCampaignComment.CreatedBy = new Guid();
                            //}
                            //else
                            //{
                            objCampaignComment.CreatedBy = this._userId;
                            //}
                            db.Entry(objCampaignComment).State = EntityState.Added;
                            db.Plan_Campaign_Program_Tactic_Comment.Add(objCampaignComment);
                        }
                        #endregion

                        sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Campaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Update.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                    }
                    else
                    {
                        _isResultError = true;
                        string exMessage = "System error occurred while update campaign \"" + planCampaign.Title + "\".";
                        sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogCampaign.Status = StatusResult.Error.ToString();
                        instanceLogCampaign.ErrorDescription = exMessage;
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Campaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    }
                }
                catch (SalesforceException e)
                {
                    if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                    {
                        //planCampaign.IntegrationInstanceCampaignId = null;
                        //planCampaign = SyncCampaingData(planCampaign);
                        sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Campaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Update.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                        return planCampaign;
                    }
                    else
                    {
                        _isResultError = true;
                        string exMessage = "System error occurred while updating campaign \"" + planCampaign.Title + "\": " + Common.GetInnermostException(e);
                        sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogCampaign.Status = StatusResult.Error.ToString();
                        instanceLogCampaign.ErrorDescription = exMessage;
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Campaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    }
                }
                catch (Exception)
                {
                    _isResultError = true;
                    string exMessage = "System error occurred while updating campaign \"" + planCampaign.Title + "\".";
                    sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                    instanceLogCampaign.Status = StatusResult.Error.ToString();
                    instanceLogCampaign.ErrorDescription = exMessage;
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Campaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                }
            }
            instanceLogCampaign.CreatedBy = this._userId;
            instanceLogCampaign.CreatedDate = DateTime.Now;
            db.Entry(instanceLogCampaign).State = EntityState.Added;
            sbMessage.Append(sb.ToString());
            return planCampaign;
        }

        private Plan_Campaign_Program SyncProgramData(Plan_Campaign_Program planProgram, ref StringBuilder sbMessage)
        {
            StringBuilder sb = new StringBuilder();
            //// Get program based on _id property.
            Enums.Mode currentMode = Common.GetMode(planProgram.IntegrationInstanceProgramId);
            PlanName = planProgram.Plan_Campaign.Plan.Title;
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
                        _parentId = CreateCampaign(planCampaign);
                        planCampaign.IntegrationInstanceCampaignId = _parentId;
                        planCampaign.LastSyncDate = DateTime.Now;
                        planCampaign.ModifiedDate = DateTime.Now;
                        planCampaign.ModifiedBy = _userId;
                        db.Entry(planCampaign).State = EntityState.Modified;
                        instanceLogCampaign.Status = StatusResult.Success.ToString();

                        #region "Add Campaign Synced comment to Plan_Campaign_Program_Tactic_Comment table"
                        //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                        if (!Common.IsAutoSync)
                        {
                            Plan_Campaign_Program_Tactic_Comment objCampaignComment = new Plan_Campaign_Program_Tactic_Comment();
                            objCampaignComment.PlanCampaignId = planCampaign.PlanCampaignId;
                            objCampaignComment.Comment = Common.CampaignSyncedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                            objCampaignComment.CreatedDate = DateTime.Now;
                            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                            //if (Common.IsAutoSync)
                            //{
                            //    objCampaignComment.CreatedBy = new Guid();
                            //}
                            //else
                            //{
                            objCampaignComment.CreatedBy = this._userId;
                            //}
                            db.Entry(objCampaignComment).State = EntityState.Added;
                            db.Plan_Campaign_Program_Tactic_Comment.Add(objCampaignComment);
                        }
                        #endregion

                        sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                    }
                    catch (SalesforceException e)
                    {
                        _isResultError = true;
                        string exMessage = "System error occurred while creating campaign \"" + planCampaign.Title + "\": " + Common.GetInnermostException(e);
                        sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogCampaign.Status = StatusResult.Error.ToString();
                        instanceLogCampaign.ErrorDescription = exMessage;
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    }
                    catch (Exception)
                    {
                        _isResultError = true;
                        string exMessage = "System error occurred while creating campaign \"" + planCampaign.Title + "\".";
                        sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogCampaign.Status = StatusResult.Error.ToString();
                        instanceLogCampaign.ErrorDescription = exMessage;
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
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

                        #region "Add Program Synced with Salesforce comment to Plan_Campaign_Program_Tactic_Comment table"
                        //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                        if (!Common.IsAutoSync)
                        {
                            Plan_Campaign_Program_Tactic_Comment objProgramComment = new Plan_Campaign_Program_Tactic_Comment();
                            objProgramComment.PlanProgramId = planProgram.PlanProgramId;
                            objProgramComment.Comment = Common.ProgramSyncedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                            objProgramComment.CreatedDate = DateTime.Now;
                            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                            //if (Common.IsAutoSync)
                            //{
                            //    objProgramComment.CreatedBy = new Guid();
                            //}
                            //else
                            //{
                            objProgramComment.CreatedBy = this._userId;
                            //}
                            db.Entry(objProgramComment).State = EntityState.Added;
                            db.Plan_Campaign_Program_Tactic_Comment.Add(objProgramComment);
                        }
                        #endregion

                        sb.Append("Program: " + planProgram.PlanProgramId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Create.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                    }
                    catch (SalesforceException e)
                    {
                        _isResultError = true;
                        string exMessage = "System error occurred while creating program \"" + planProgram.Title + "\": " + Common.GetInnermostException(e);
                        sb.Append("Program: " + planProgram.PlanProgramId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogProgram.Status = StatusResult.Error.ToString();
                        instanceLogProgram.ErrorDescription = exMessage;
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    }
                    catch (Exception)
                    {
                        _isResultError = true;
                        string exMessage = "System error occurred while creating program \"" + planProgram.Title + "\". ";
                        sb.Append("Program: " + planProgram.PlanProgramId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogProgram.Status = StatusResult.Error.ToString();
                        instanceLogProgram.ErrorDescription = exMessage;
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
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
                // Add By Nishant Sheth
                //Make Hireachy for Marketo sync on SFDC
                if (_IsSFDCWithMarketo)
                {
                    _parentId = planProgram.Plan_Campaign.IntegrationInstanceCampaignId;
                }
                try
                {
                    if (UpdateProgram(planProgram))
                    {
                        planProgram.LastSyncDate = DateTime.Now;
                        planProgram.ModifiedDate = DateTime.Now;
                        planProgram.ModifiedBy = _userId;
                        instanceLogProgram.Status = StatusResult.Success.ToString();

                        #region "Add Program Synced with Salesforce comment to Plan_Campaign_Program_Tactic_Comment table"
                        //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                        if (!Common.IsAutoSync)
                        {
                            Plan_Campaign_Program_Tactic_Comment objProgramComment = new Plan_Campaign_Program_Tactic_Comment();
                            objProgramComment.PlanProgramId = planProgram.PlanProgramId;
                            objProgramComment.Comment = Common.ProgramUpdatedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                            objProgramComment.CreatedDate = DateTime.Now;
                            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                            //if (Common.IsAutoSync)
                            // {
                            //objProgramComment.CreatedBy = new Guid();
                            //}
                            //else
                            //{
                            objProgramComment.CreatedBy = this._userId;
                            // }
                            db.Entry(objProgramComment).State = EntityState.Added;
                            db.Plan_Campaign_Program_Tactic_Comment.Add(objProgramComment);
                        }
                        #endregion

                        sb.Append("Program: " + planProgram.PlanProgramId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Update.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                    }
                    else
                    {
                        _isResultError = true;
                        string exMessage = "System error occurred while updating program \"" + planProgram.Title + "\". ";
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                        sb.Append("Program: " + planProgram.PlanProgramId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogProgram.Status = StatusResult.Error.ToString();
                        instanceLogProgram.ErrorDescription = exMessage;
                    }
                }
                catch (SalesforceException e)
                {
                    if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                    {
                        // planProgram.IntegrationInstanceProgramId = null;
                        // planProgram = SyncProgramData(planProgram);
                        sb.Append("Program: " + planProgram.PlanProgramId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(Convert.ToInt32(planProgram.PlanProgramId), Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Update.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                        return planProgram;
                    }
                    else
                    {
                        _isResultError = true;
                        string exMessage = "System error occurred while updating program \"" + planProgram.Title + "\": " + Common.GetInnermostException(e);
                        sb.Append("Program: " + planProgram.PlanProgramId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogProgram.Status = StatusResult.Error.ToString();
                        instanceLogProgram.ErrorDescription = exMessage;
                        _lstSyncError.Add(Common.PrepareSyncErrorList(planProgram.PlanProgramId, Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    }
                }
                catch (Exception)
                {
                    _isResultError = true;
                    string exMessage = "System error occurred while updating program \"" + planProgram.Title + "\". ";
                    sb.Append("Program: " + planProgram.PlanProgramId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                    instanceLogProgram.Status = StatusResult.Error.ToString();
                    instanceLogProgram.ErrorDescription = exMessage;
                    _lstSyncError.Add(Common.PrepareSyncErrorList(planProgram.PlanProgramId, Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                }
                instanceLogProgram.CreatedBy = this._userId;
                instanceLogProgram.CreatedDate = DateTime.Now;
                db.Entry(instanceLogProgram).State = EntityState.Added;
            }
            sbMessage.Append(sb.ToString());
            return planProgram;
        }

        private Plan_Campaign_Program_Tactic SyncTacticData(Plan_Campaign_Program_Tactic planTactic, ref StringBuilder sbMessage)
        {
            //System.Diagnostics.Trace.WriteLine("Step SyncTacticData 1 Start:" + DateTime.Now.ToString());
            StringBuilder sb = new StringBuilder();
            //bool islinktacticdata = false;
            Enums.Mode currentMode = Common.GetMode(planTactic.IntegrationInstanceTacticId);
            // Add By Nishant Sheth
            //Make Hireachy for Marketo sync on SFDC
            if (_IsSFDCWithMarketo)
            {
                if (currentMode == Enums.Mode.Create)
                {
                    var SFDCgetTacticId = _client.Query<object>("SELECT Id FROM Campaign  WHERE Name IN ('" + planTactic.TacticCustomName + "')");
                    JObject jobj;
                    string SFDCIntegrationInstanceTacticId = "";
                    foreach (var resultin in SFDCgetTacticId)
                    {
                        jobj = JObject.Parse(resultin.ToString());
                        SFDCIntegrationInstanceTacticId = Convert.ToString(jobj["Id"]);
                    }
                    if (!string.IsNullOrEmpty(SFDCIntegrationInstanceTacticId))
                    {
                        planTactic.IntegrationInstanceTacticId = SFDCIntegrationInstanceTacticId;
                        currentMode = Enums.Mode.Update;
                    }
                    else
                    {
                        // Add by Nishant Sheth
                        // Desc :: #2280 : if tatic is not sync by marketo to salesforce then tactic is not created in salesforce
                        // Modified Condition by nishant sheth
                        // Desc :: #2289 : if tactic is not sync with marketo and pull instance set for sfdc then tactic should not be created in sfdc.
                        if (planTactic.IsSyncMarketo.HasValue && planTactic.IsSyncMarketo.Value)
                        {
                            return planTactic;
                        }
                    }
                }
            }

            #region "Old Code: commented on 04/04/2016 for PL ticket #2024"
            // If Tactic is linked then sync latest year Program & Campaign to Salesforce.
            //if (lnkdTactic != null && lnkdTactic.PlanTacticId > 0)
            //{
            //    string orgnPlanYear = planTactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year;
            //    string lnkdPlanYear = lnkdTactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year;
            //    if (!string.IsNullOrEmpty(orgnPlanYear) && !string.IsNullOrEmpty(lnkdPlanYear))
            //    {
            //        if (int.Parse(orgnPlanYear) > int.Parse(lnkdPlanYear))
            //            islinktacticdata = false;
            //        else
            //            islinktacticdata = true;
            //    }
            //}
            //if (islinktacticdata)
            //{
            //    PlanName = lnkdTactic.Plan_Campaign_Program.Plan_Campaign.Plan.Title;
            //}
            //else
            //{
            //    PlanName = planTactic.Plan_Campaign_Program.Plan_Campaign.Plan.Title;
            //} 
            #endregion
            PlanName = planTactic.Plan_Campaign_Program.Plan_Campaign.Plan.Title;
            //System.Diagnostics.Trace.WriteLine("Step SyncTacticData 1 End:" + DateTime.Now.ToString());
            if (currentMode.Equals(Enums.Mode.Create))
            {
                #region "OldCode: commented on 04/04/2016 for PL ticket #2024"
                //Plan_Campaign_Program planProgram = new Plan_Campaign_Program();
                //if (islinktacticdata)
                //{
                //    planProgram = lnkdTactic.Plan_Campaign_Program;
                //}
                //else
                //{
                //    planProgram = planTactic.Plan_Campaign_Program;
                //} 
                #endregion

                Plan_Campaign_Program planProgram = planTactic.Plan_Campaign_Program;
                _parentId = planProgram.IntegrationInstanceProgramId;
                if (string.IsNullOrWhiteSpace(_parentId))
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
                            _parentId = CreateCampaign(planCampaign);
                            planCampaign.IntegrationInstanceCampaignId = _parentId;
                            planCampaign.LastSyncDate = DateTime.Now;
                            planCampaign.ModifiedDate = DateTime.Now;
                            planCampaign.ModifiedBy = _userId;
                            db.Entry(planCampaign).State = EntityState.Modified;
                            instanceLogCampaign.Status = StatusResult.Success.ToString();

                            #region "Add Campaign Synced comment to Plan_Campaign_Program_Tactic_Comment table"
                            //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                            if (!Common.IsAutoSync)
                            {
                                Plan_Campaign_Program_Tactic_Comment objCampaignComment = new Plan_Campaign_Program_Tactic_Comment();
                                objCampaignComment.PlanCampaignId = planCampaign.PlanCampaignId;
                                objCampaignComment.Comment = Common.CampaignSyncedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                                objCampaignComment.CreatedDate = DateTime.Now;
                                ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                                //if (Common.IsAutoSync)
                                //{
                                //    objCampaignComment.CreatedBy = new Guid();
                                //}
                                //else
                                //{
                                objCampaignComment.CreatedBy = this._userId;
                                //}
                                db.Entry(objCampaignComment).State = EntityState.Added;
                                db.Plan_Campaign_Program_Tactic_Comment.Add(objCampaignComment);
                            }
                            #endregion

                            sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        }
                        catch (SalesforceException e)
                        {
                            _isResultError = true;
                            string exMessage = "System error occurred while creating campaign \"" + planCampaign.Title + "\": " + Common.GetInnermostException(e);
                            _parentId = string.Empty;
                            sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                            instanceLogCampaign.Status = StatusResult.Error.ToString();
                            instanceLogCampaign.ErrorDescription = exMessage;
                            // To get Total Tactic count and insert unique record into _lstSyncError list, below list we added EntityId as PlanTactic.PlanTacticId instead of planCampaign.PlanCampaignId
                            _lstSyncError.Add(Common.PrepareSyncErrorList(planTactic.PlanTacticId, Enums.EntityType.Campaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                        }
                        catch (Exception ex)
                        {
                            _isResultError = true;
                            string exMessage = "System error occurred while creating campaign \"" + planCampaign.Title + "\"."; //Common.GetInnermostException(e);
                            _parentId = string.Empty;
                            sb.Append("Campaign: " + planCampaign.PlanCampaignId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                            instanceLogCampaign.Status = StatusResult.Error.ToString();
                            instanceLogCampaign.ErrorDescription = "System error occurred while creating campaign \"" + planCampaign.Title + "\":" + Common.GetInnermostException(ex) + "\": " + ex.StackTrace;
                            // To get Total Tactic count and insert unique record into _lstSyncError list, below list we added EntityId as PlanTactic.PlanTacticId instead of planCampaign.PlanCampaignId
                            _lstSyncError.Add(Common.PrepareSyncErrorList(planTactic.PlanTacticId, Enums.EntityType.Campaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
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

                            _parentId = CreateProgram(planProgram);
                            planProgram.IntegrationInstanceProgramId = _parentId;
                            planProgram.LastSyncDate = DateTime.Now;
                            planProgram.ModifiedDate = DateTime.Now;
                            planProgram.ModifiedBy = _userId;
                            instanceLogProgram.Status = StatusResult.Success.ToString();

                            #region "Add Program Synced comment to Plan_Campaign_Program_Tactic_Comment table"
                            //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                            if (!Common.IsAutoSync)
                            {
                                Plan_Campaign_Program_Tactic_Comment objProgramComment = new Plan_Campaign_Program_Tactic_Comment();
                                objProgramComment.PlanProgramId = planProgram.PlanProgramId;
                                objProgramComment.Comment = Common.ProgramSyncedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                                objProgramComment.CreatedDate = DateTime.Now;
                                ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                                //if (Common.IsAutoSync)
                                //{
                                //    objProgramComment.CreatedBy = new Guid();
                                //}
                                //else
                                //{
                                objProgramComment.CreatedBy = this._userId;
                                //}
                                db.Entry(objProgramComment).State = EntityState.Added;
                                db.Plan_Campaign_Program_Tactic_Comment.Add(objProgramComment);
                            }
                            #endregion

                            sb.Append("Program: " + planProgram.PlanProgramId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        }
                        catch (SalesforceException e)
                        {
                            _isResultError = true;
                            string exMessage = "System error occurred while creating program \"" + planProgram.Title + "\": " + Common.GetInnermostException(e);
                            _parentId = string.Empty;
                            sb.Append("Program: " + planProgram.PlanProgramId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                            instanceLogProgram.Status = StatusResult.Error.ToString();
                            instanceLogProgram.ErrorDescription = exMessage;
                            // To get Total Tactic count and insert unique record into _lstSyncError list, below list we added EntityId as PlanTactic.PlanTacticId instead of planProgram.PlanProgramId
                            _lstSyncError.Add(Common.PrepareSyncErrorList(planTactic.PlanTacticId, Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                        }
                        catch (Exception ex)
                        {
                            _isResultError = true;
                            string exMessage = "System error occurred while creating program \"" + planProgram.Title + "\". ";//Common.GetInnermostException(e);
                            _parentId = string.Empty;
                            sb.Append("Program: " + planProgram.PlanProgramId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                            instanceLogProgram.Status = StatusResult.Error.ToString();
                            instanceLogProgram.ErrorDescription = "System error occurred while creating program \"" + planProgram.Title + "\": " + Common.GetInnermostException(ex) + "\": " + ex.StackTrace;
                            // To get Total Tactic count and insert unique record into _lstSyncError list, below list we added EntityId as PlanTactic.PlanTacticId instead of planProgram.PlanProgramId
                            _lstSyncError.Add(Common.PrepareSyncErrorList(planTactic.PlanTacticId, Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
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

                        #region "Old Code: Update comment"
                        ////Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                        //#region "Add Tactic Synced comment to Plan_Campaign_Program_Tactic_Comment table"
                        ////Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                        //Plan_Campaign_Program_Tactic_Comment objTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                        //if (!Common.IsAutoSync)
                        //{
                        //    objTacticComment.PlanTacticId = planTactic.PlanTacticId;
                        //    objTacticComment.Comment = Common.TacticSyncedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                        //    objTacticComment.CreatedDate = DateTime.Now;
                        //    ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                        //    //if (Common.IsAutoSync)
                        //    //{
                        //    //    objTacticComment.CreatedBy = new Guid();
                        //    //}
                        //    //else
                        //    //{
                        //    objTacticComment.CreatedBy = this._userId;
                        //    //}
                        //    db.Entry(objTacticComment).State = EntityState.Added;
                        //    db.Plan_Campaign_Program_Tactic_Comment.Add(objTacticComment);
                        //}
                        //#endregion
                        //// End Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic

                        //#region "Update Linked Tactic IntegrationInstanceTacticId & Tactic Comment Table"
                        //if (lnkdTactic != null && lnkdTactic.PlanTacticId > 0)
                        //{
                        //    lnkdTactic.IntegrationInstanceTacticId = planTactic.IntegrationInstanceTacticId;
                        //    lnkdTactic.TacticCustomName = planTactic.TacticCustomName;
                        //    lnkdTactic.LastSyncDate = DateTime.Now;
                        //    lnkdTactic.ModifiedDate = DateTime.Now;
                        //    lnkdTactic.ModifiedBy = _userId;
                        //    if (!Common.IsAutoSync)
                        //    {
                        //        // Add linked tactic comment
                        //        Plan_Campaign_Program_Tactic_Comment objLinkedTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                        //        objLinkedTacticComment.PlanTacticId = lnkdTactic.PlanTacticId;
                        //        objLinkedTacticComment.Comment = objTacticComment.Comment;
                        //        objLinkedTacticComment.CreatedDate = DateTime.Now;
                        //        objLinkedTacticComment.CreatedBy = objTacticComment.CreatedBy;
                        //        db.Entry(objLinkedTacticComment).State = EntityState.Added;
                        //        db.Plan_Campaign_Program_Tactic_Comment.Add(objLinkedTacticComment);
                        //}
                        //}
                        //#endregion 
                        #endregion

                        sb.Append("Tactic: " + planTactic.PlanTacticId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(Convert.ToInt32(planTactic.PlanTacticId), Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Create.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                    }
                    catch (SalesforceException e)
                    {
                        _isResultError = true;
                        string exMessage = "System error occurred while creating tactic \"" + planTactic.Title + "\": " + Common.GetInnermostException(e);
                        _parentId = string.Empty;
                        sb.Append("Tactic: " + planTactic.PlanTacticId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = exMessage;
                        _lstSyncError.Add(Common.PrepareSyncErrorList(planTactic.PlanTacticId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    }
                    catch (Exception ex)
                    {
                        _isResultError = true;
                        string exMessage = "System error occurred while creating tactic \"" + planTactic.Title + "\".";// Common.GetInnermostException(e);
                        _parentId = string.Empty;
                        sb.Append("Tactic: " + planTactic.PlanTacticId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = "System error occurred while syncing tactic \"" + planTactic.Title + "\": " + Common.GetInnermostException(ex);
                        _lstSyncError.Add(Common.PrepareSyncErrorList(planTactic.PlanTacticId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
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
                // Add By Nishant Sheth
                //Make Hireachy for Marketo sync on SFDC
                if (_IsSFDCWithMarketo)
                {
                    _parentId = planTactic.Plan_Campaign_Program.IntegrationInstanceProgramId;
                }
                try
                {
                    //System.Diagnostics.Trace.WriteLine("Step SyncTacticData 2 Start:" + DateTime.Now.ToString());
                    if (UpdateTactic(planTactic))
                    {
                        //System.Diagnostics.Trace.WriteLine("Step SyncTacticData 2 End:" + DateTime.Now.ToString());
                        //System.Diagnostics.Trace.WriteLine("Step SyncTacticData 3 Start:" + DateTime.Now.ToString());
                        planTactic.LastSyncDate = DateTime.Now;
                        planTactic.ModifiedDate = DateTime.Now;
                        planTactic.ModifiedBy = _userId;
                        instanceLogTactic.Status = StatusResult.Success.ToString();
                        #region "Old Code"
                        //#region "Add Tactic Update comment to Plan_Campaign_Program_Tactic_Comment table"
                        ////Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                        //Plan_Campaign_Program_Tactic_Comment objTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                        //if (!Common.IsAutoSync)
                        //{
                        //    objTacticComment.PlanTacticId = planTactic.PlanTacticId;
                        //    objTacticComment.Comment = Common.TacticUpdatedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                        //    objTacticComment.CreatedDate = DateTime.Now;
                        //    ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                        //    //if (Common.IsAutoSync)
                        //    //{
                        //    //    objTacticComment.CreatedBy = new Guid();
                        //    //}
                        //    //else
                        //    //{
                        //    objTacticComment.CreatedBy = this._userId;
                        //    //}
                        //    db.Entry(objTacticComment).State = EntityState.Added;
                        //    db.Plan_Campaign_Program_Tactic_Comment.Add(objTacticComment);
                        //}
                        //#endregion
                        ////System.Diagnostics.Trace.WriteLine("Step SyncTacticData 3 End:" + DateTime.Now.ToString());
                        //#region "Update Linked Tactic IntegrationInstanceTacticId & Tactic Comment Table"
                        //if (lnkdTactic != null && lnkdTactic.PlanTacticId > 0)
                        //{
                        //    lnkdTactic.TacticCustomName = planTactic.TacticCustomName;
                        //    lnkdTactic.LastSyncDate = DateTime.Now;
                        //    lnkdTactic.ModifiedDate = DateTime.Now;
                        //    lnkdTactic.ModifiedBy = _userId;
                        //    if (!Common.IsAutoSync)
                        //    {
                        //        // Add linked tactic comment
                        //        Plan_Campaign_Program_Tactic_Comment objLinkedTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                        //        objLinkedTacticComment.PlanTacticId = lnkdTactic.PlanTacticId;
                        //        objLinkedTacticComment.Comment = objTacticComment.Comment;
                        //        objLinkedTacticComment.CreatedDate = DateTime.Now;
                        //        objLinkedTacticComment.CreatedBy = objTacticComment.CreatedBy;
                        //        db.Entry(objLinkedTacticComment).State = EntityState.Added;
                        //        db.Plan_Campaign_Program_Tactic_Comment.Add(objLinkedTacticComment);
                        //}
                        //}
                        //#endregion 
                        #endregion

                        sb.Append("Tactic: " + planTactic.PlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(Convert.ToInt32(planTactic.PlanTacticId), Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Update.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                    }
                    else
                    {
                        string exMessage = "System error occurred while updating tactic \"" + planTactic.Title + "\". ";
                        _isResultError = true;
                        _lstSyncError.Add(Common.PrepareSyncErrorList(planTactic.PlanTacticId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                        sb.Append("Tactic: " + planTactic.PlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = exMessage;
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
                        _isResultError = true;
                        string exMessage = "System error occurred while updating tactic \"" + planTactic.Title + "\": " + Common.GetInnermostException(e);
                        _lstSyncError.Add(Common.PrepareSyncErrorList(planTactic.PlanTacticId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = exMessage;
                    }
                }
                catch (Exception ex)
                {
                    _isResultError = true;
                    string exMessage = "System error occurred while updating tactic \"" + planTactic.Title + "\". ";//Common.GetInnermostException(e);
                    _lstSyncError.Add(Common.PrepareSyncErrorList(planTactic.PlanTacticId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    instanceLogTactic.Status = StatusResult.Error.ToString();
                    instanceLogTactic.ErrorDescription = "System error occurred while updating tactic \"" + planTactic.Title + "\": " + Common.GetInnermostException(ex) + "\": " + ex.StackTrace;
                }
                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }
            sbMessage.Append(sb.ToString());

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
                            string exMessage = Common.GetInnermostException(e);
                            instanceLogProgram.Status = StatusResult.Error.ToString();
                            instanceLogProgram.ErrorDescription = exMessage;
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
                            string exMessage = Common.GetInnermostException(e);
                            instanceLogCampaign.Status = StatusResult.Error.ToString();
                            instanceLogCampaign.ErrorDescription = exMessage;
                        }
                    }

                    instanceLogCampaign.CreatedBy = this._userId;
                    instanceLogCampaign.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogCampaign).State = EntityState.Added;
                }
            }

            return planIMPCampaign;
        }

        private Plan_Improvement_Campaign_Program_Tactic SyncImprovementData(Plan_Improvement_Campaign_Program_Tactic planIMPTactic, ref StringBuilder sbMessage)
        {
            StringBuilder sb = new StringBuilder();
            Enums.Mode currentMode = Common.GetMode(planIMPTactic.IntegrationInstanceTacticId);
            PlanName = planIMPTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Title;
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
                            sb.Append("ImprovementCampaign: " + planIMPCampaign.ImprovementPlanCampaignId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        }
                        catch (SalesforceException e)
                        {
                            _parentId = string.Empty;
                            _isResultError = true;
                            sb.Append("ImprovementCampaign: " + planIMPCampaign.ImprovementPlanCampaignId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                            instanceLogCampaign.Status = StatusResult.Error.ToString();
                            instanceLogCampaign.ErrorDescription = "System error occurred while creating Improvement Campaign \"" + planIMPCampaign.Title + "\": " + Common.GetInnermostException(e);
                            // To get Total Tactic count and insert unique record into _lstSyncError list, below list we added EntityId as planIMPTactic.ImprovementPlanTacticId instead of planIMPCampaign.ImprovementPlanCampaignId
                            _lstSyncError.Add(Common.PrepareSyncErrorList(planIMPTactic.ImprovementPlanTacticId, Enums.EntityType.ImprovementCampaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "System error occurred while syncing improvement campaign \"" + planIMPCampaign.Title + "\".", Enums.SyncStatus.Error, DateTime.Now));
                        }
                        catch (Exception ex)
                        {
                            _isResultError = true;
                            string exMessage = "System error occurred while creating Improvement Campaign \"" + planIMPCampaign.Title + "\".";// Common.GetInnermostException(e);
                            _parentId = string.Empty;
                            sb.Append("Tactic: " + planIMPCampaign.ImprovementPlanCampaignId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                            instanceLogCampaign.Status = StatusResult.Error.ToString();
                            instanceLogCampaign.ErrorDescription = "System error occurred while creating Improvement Campaign \"" + planIMPCampaign.Title + "\":" + Common.GetInnermostException(ex) + ex.StackTrace;
                            // To get Total Tactic count and insert unique record into _lstSyncError list, below list we added EntityId as planIMPTactic.ImprovementPlanTacticId instead of planIMPCampaign.ImprovementPlanCampaignId
                            _lstSyncError.Add(Common.PrepareSyncErrorList(planIMPTactic.ImprovementPlanTacticId, Enums.EntityType.ImprovementCampaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
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
                            sb.Append("ImprovementProgram: " + planIMPProgram.ImprovementPlanProgramId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Success.ToString() + "); ");

                        }
                        catch (SalesforceException e)
                        {
                            _parentId = string.Empty;
                            string exMessage = "System error occurred while creating Improvement Program \"" + planIMPProgram.Title + "\": " + Common.GetInnermostException(e);
                            _isResultError = true;
                            sb.Append("ImprovementProgram: " + planIMPProgram.ImprovementPlanProgramId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                            instanceLogProgram.Status = StatusResult.Error.ToString();
                            instanceLogProgram.ErrorDescription = exMessage;
                            // To get Total Tactic count and insert unique record into _lstSyncError list, below list we added EntityId as planIMPTactic.ImprovementPlanTacticId instead of planIMPProgram.ImprovementPlanProgramId
                            _lstSyncError.Add(Common.PrepareSyncErrorList(planIMPTactic.ImprovementPlanTacticId, Enums.EntityType.ImprovementProgram, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                        }
                        catch (Exception ex)
                        {
                            _isResultError = true;
                            string exMessage = "System error occurred while creating Improvement Program \"" + planIMPProgram.Title + "\".";// Common.GetInnermostException(e);
                            _parentId = string.Empty;
                            sb.Append("Tactic: " + planIMPProgram.ImprovementPlanProgramId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                            instanceLogProgram.Status = StatusResult.Error.ToString();
                            instanceLogProgram.ErrorDescription = "System error occurred while creating Improvement Program \"" + planIMPProgram.Title + "\":" + Common.GetInnermostException(ex) + ex.StackTrace;
                            // To get Total Tactic count and insert unique record into _lstSyncError list, below list we added EntityId as planIMPTactic.ImprovementPlanTacticId instead of planIMPProgram.ImprovementPlanProgramId
                            _lstSyncError.Add(Common.PrepareSyncErrorList(planIMPTactic.ImprovementPlanTacticId, Enums.EntityType.ImprovementProgram, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
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

                        #region "Add ImprovementTactic comment to Plan_Improvement_Campaign_Program_Tactic_Comment table"
                        //Modified by Rahul Shah on 08/03/2016 for PL #1978 . 
                        if (!Common.IsAutoSync)
                        {
                            //Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                            Plan_Improvement_Campaign_Program_Tactic_Comment objImpTacticComment = new Plan_Improvement_Campaign_Program_Tactic_Comment();
                            objImpTacticComment.ImprovementPlanTacticId = planIMPTactic.ImprovementPlanTacticId;
                            objImpTacticComment.Comment = Common.ImprovementTacticSyncedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                            objImpTacticComment.CreatedDate = DateTime.Now;
                            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                            //if (Common.IsAutoSync)
                            //{
                            //    objImpTacticComment.CreatedBy = new Guid();
                            //}
                            //else
                            //{
                            objImpTacticComment.CreatedBy = this._userId;
                            //}

                            db.Entry(objImpTacticComment).State = EntityState.Added;
                            db.Plan_Improvement_Campaign_Program_Tactic_Comment.Add(objImpTacticComment);
                        }
                        //End: Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic 
                        #endregion

                        sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(Convert.ToInt32(planIMPTactic.ImprovementPlanTacticId), Enums.EntityType.ImprovementTactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Create.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                    }
                    catch (SalesforceException e)
                    {
                        _parentId = string.Empty;
                        _isResultError = true;
                        string exMessage = "System error occurred while creating Improvement tactic \"" + planIMPTactic.Title + "\": " + Common.GetInnermostException(e);
                        sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = exMessage;
                        _lstSyncError.Add(Common.PrepareSyncErrorList(planIMPTactic.ImprovementPlanTacticId, Enums.EntityType.ImprovementTactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    }
                    catch (Exception ex)
                    {
                        _parentId = string.Empty;
                        _isResultError = true;
                        string exMessage = "System error occurred while creating Improvement tactic \"" + planIMPTactic.Title + "\".";
                        sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = "System error occurred while creating Improvement tactic \"" + planIMPTactic.Title + "\":" + Common.GetInnermostException(ex) + ex.StackTrace;
                        _lstSyncError.Add(Common.PrepareSyncErrorList(planIMPTactic.ImprovementPlanTacticId, Enums.EntityType.ImprovementTactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
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

                        #region "Add ImprovementTactic comment to Plan_Improvement_Campaign_Program_Tactic_Comment table"'
                        //Modified by Rahul Shah on 08/03/2016 for PL #1978 . 
                        if (!Common.IsAutoSync)
                        {
                            //Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                            Plan_Improvement_Campaign_Program_Tactic_Comment objImpTacticComment = new Plan_Improvement_Campaign_Program_Tactic_Comment();
                            objImpTacticComment.ImprovementPlanTacticId = planIMPTactic.ImprovementPlanTacticId;
                            objImpTacticComment.Comment = Common.ImprovementTacticUpdatedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                            objImpTacticComment.CreatedDate = DateTime.Now;
                            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                            //if (Common.IsAutoSync)
                            //{
                            //    objImpTacticComment.CreatedBy = new Guid();
                            //}
                            //else
                            //{
                            objImpTacticComment.CreatedBy = this._userId;
                            //}

                            db.Entry(objImpTacticComment).State = EntityState.Added;
                            db.Plan_Improvement_Campaign_Program_Tactic_Comment.Add(objImpTacticComment);
                        }
                        //End: Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic 
                        #endregion

                        sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(Convert.ToInt32(planIMPTactic.ImprovementPlanTacticId), Enums.EntityType.ImprovementTactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Update.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                    }
                    else
                    {
                        _isResultError = true;
                        string exMessage = "System error occurred while updating Improvement tactic \"" + planIMPTactic.Title + "\".";
                        _lstSyncError.Add(Common.PrepareSyncErrorList(planIMPTactic.ImprovementPlanTacticId, Enums.EntityType.ImprovementTactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                        sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = exMessage;
                    }
                }
                catch (SalesforceException e)
                {
                    if (e.Error.Equals(SalesforceError.EntityIsDeleted))
                    {
                        // planIMPTactic.IntegrationInstanceTacticId = null;
                        // planIMPTactic = SyncImprovementData(planIMPTactic);
                        sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(Convert.ToInt32(planIMPTactic.ImprovementPlanTacticId), Enums.EntityType.ImprovementTactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Update.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                        return planIMPTactic;
                    }
                    else
                    {
                        _parentId = string.Empty;
                        _isResultError = true;
                        string exMessage = "System error occurred while updating Improvement tactic \"" + planIMPTactic.Title + "\": " + Common.GetInnermostException(e);
                        sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = exMessage;
                        _lstSyncError.Add(Common.PrepareSyncErrorList(planIMPTactic.ImprovementPlanTacticId, Enums.EntityType.ImprovementTactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    }
                }
                catch (Exception ex)
                {
                    _parentId = string.Empty;
                    _isResultError = true;
                    string exMessage = "System error occurred while updating Improvement tactic \"" + planIMPTactic.Title + "\".";
                    sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                    instanceLogTactic.Status = StatusResult.Error.ToString();
                    instanceLogTactic.ErrorDescription = "System error occurred while updating Improvement tactic \"" + planIMPTactic.Title + "\":" + Common.GetInnermostException(ex) + ex.StackTrace;
                    _lstSyncError.Add(Common.PrepareSyncErrorList(planIMPTactic.ImprovementPlanTacticId, Enums.EntityType.ImprovementTactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                }
                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }
            sbMessage.Append(sb.ToString());
            return planIMPTactic;
        }

        #region "Old Code: Modified by Viral PL ticket #2024"
        /// <summary>
        /// Function to Synchronize instance data.
        /// </summary>
        //private void SyncInstanceData()
        //{
        //    string published = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString();
        //    string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
        //    StringBuilder sbMessage;
        //    int logRecordSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LogRecordSize"]);
        //    int pushRecordBatchSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IntegrationPushRecordBatchSize"]);
        //    string strActualCostActualField = "CostActual"; // static variable value to identify that instance has mapped Actual cost field in Instance mapping.
        //    try
        //    {
        //        List<Plan> lstPlan = db.Plans.Where(p => p.Model.IntegrationInstanceId == _integrationInstanceId && p.Model.Status.Equals(published)).ToList();
        //        if (lstPlan != null && lstPlan.Count > 0)
        //        {
        //            List<int> planIds = lstPlan.Select(p => p.PlanId).ToList();
        //            //System.Diagnostics.Trace.WriteLine("Step 1 Start:" + DateTime.Now.ToString());
        //            #region "Create CampaignIdList, ProgramIdList & TacticIdList"
        //            List<Plan_Campaign> campaignList = db.Plan_Campaign.Where(campaign => planIds.Contains(campaign.PlanId) && !campaign.IsDeleted).ToList();
        //            List<int> campaignIdList = campaignList.Select(c => c.PlanCampaignId).ToList();
        //            List<Plan_Campaign_Program> programList = db.Plan_Campaign_Program.Where(program => campaignIdList.Contains(program.PlanCampaignId) && !program.IsDeleted).ToList();
        //            List<int> programIdList = programList.Select(c => c.PlanProgramId).ToList();
        //            List<Plan_Campaign_Program_Tactic> tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).ToList();
        //            List<Plan_Campaign_Program_Tactic> tacticList = tblTactic.Where(tactic => programIdList.Contains(tactic.PlanProgramId)).ToList();
        //            List<int> LinkedTacticIds = new List<int>();
        //            List<TacticLinkedTacMapping> lstTac_LinkTacMapping = new List<TacticLinkedTacMapping>();
        //            if (tacticList != null && tacticList.Count > 0)
        //            {
        //                var lstTac_LinkTacMappIds = tacticList.Where(tac => tac.LinkedTacticId.HasValue).Select(tac => new { PlanTacticId = tac.PlanTacticId, PlanTacic = tac, LinkedTacticId = tac.LinkedTacticId.Value }).ToList();
        //                if (lstTac_LinkTacMappIds != null && lstTac_LinkTacMappIds.Count > 0)
        //                {
        //                    #region "Declare local variables"
        //                    TacticLinkedTacMapping objTacLinkMapping;
        //                    Plan_Campaign_Program_Tactic linkedTactic;
        //                    string strOrgnlTacPlanyear, strLnkdTacPlanYear;
        //                    int orgnlTacPlanYear, lnkdTacPlanYear;
        //                    #endregion

        //                    foreach (var tac in lstTac_LinkTacMappIds.ToList())
        //                    {
        //                        #region "Initialize local variables"
        //                        objTacLinkMapping = new TacticLinkedTacMapping();
        //                        linkedTactic = new Plan_Campaign_Program_Tactic();
        //                        strOrgnlTacPlanyear = string.Empty; strLnkdTacPlanYear = string.Empty;
        //                        orgnlTacPlanYear = 0; lnkdTacPlanYear = 0;
        //                        #endregion

        //                        // get linked tactic
        //                        linkedTactic = tacticList.Where(tactc => tactc.PlanTacticId == tac.LinkedTacticId).FirstOrDefault();
        //                        if (linkedTactic != null)
        //                        {
        //                            #region "Get both tactics Plan year"
        //                            strOrgnlTacPlanyear = tac.PlanTacic.Plan_Campaign_Program.Plan_Campaign.Plan.Year; // Get Orginal Tactic Plan Year.
        //                            strLnkdTacPlanYear = linkedTactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year; // Get Linked Tactic Plan Year.

        //                            //parse plan year to int.
        //                            if (!string.IsNullOrEmpty(strOrgnlTacPlanyear) && !string.IsNullOrEmpty(strLnkdTacPlanYear))
        //                            {
        //                                orgnlTacPlanYear = int.Parse(strOrgnlTacPlanyear);
        //                                lnkdTacPlanYear = int.Parse(strLnkdTacPlanYear);
        //                            }
        //                            #endregion

        //                            #region "Insert latest plan year tactic to lstTac_LinkTacMapping list"
        //                            //Identify latest Tactic and add it to "lstTac_LinkTacMapping" list to push latest tactics to SFDC.
        //                            if (lnkdTacPlanYear > orgnlTacPlanYear)
        //                            {
        //                                objTacLinkMapping.PlanTactic = linkedTactic; // set latest plan year tactic as PlanTactic to model and pass to SFDC.
        //                                objTacLinkMapping.LinkedTactic = tac.PlanTacic; // set old tactic as LinkedTactic and update its IntegrationInstanceTacticId & Comment after pushing origional Tactic.
        //                                tacticList.Remove(tac.PlanTacic);    // Remove the old tactic from tacticlist which is not going to push to SFDC.
        //                            }
        //                            else
        //                            {
        //                                objTacLinkMapping.PlanTactic = tac.PlanTacic; // set latest plan year tactic as PlanTactic to model and pass to SFDC.
        //                                objTacLinkMapping.LinkedTactic = linkedTactic; // set old tactic as LinkedTactic and update its IntegrationInstanceTacticId & Comment after pushing origional Tactic.
        //                                tacticList.Remove(linkedTactic);    // Remove the old tactic from tacticlist which is not going to push to SFDC.
        //                            }
        //                            #endregion

        //                            #region "Remove linked Tactic from current list."
        //                            var objLnkdTac = lstTac_LinkTacMappIds.FirstOrDefault(t => t.PlanTacticId == tac.LinkedTacticId);
        //                            if (objLnkdTac != null)
        //                            {
        //                                lstTac_LinkTacMappIds.Remove(objLnkdTac);
        //                            }
        //                            #endregion

        //                            lstTac_LinkTacMapping.Add(objTacLinkMapping);
        //                        }
        //                    }
        //                }
        //            }
        //            List<int> campaignIdForTactic = tacticList.Where(tactic => string.IsNullOrWhiteSpace(tactic.Plan_Campaign_Program.Plan_Campaign.IntegrationInstanceCampaignId)).Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
        //            List<int> programIdForTactic = tacticList.Where(tactic => string.IsNullOrWhiteSpace(tactic.Plan_Campaign_Program.IntegrationInstanceProgramId)).Select(tactic => tactic.PlanProgramId).ToList();
        //            //System.Diagnostics.Trace.WriteLine("Step 1 End:" + DateTime.Now.ToString());
        //            //System.Diagnostics.Trace.WriteLine("Step 2 Start:" + DateTime.Now.ToString());
        //            int page = 0;
        //            int total = 0;
        //            //int pageSize = 10;
        //            int maxpage = 0;

        //            campaignList = campaignList.Where(campaign => statusList.Contains(campaign.Status) && campaign.IsDeployedToIntegration).ToList();
        //            campaignIdList = campaignList.Select(c => c.PlanCampaignId).Distinct().ToList();
        //            if (campaignIdList.Count > 0)
        //            {
        //                campaignIdList.Concat(campaignIdForTactic);
        //            }
        //            else
        //            {
        //                campaignIdList = campaignIdForTactic;
        //            }
        //            // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997

        //            programList = programList.Where(program => statusList.Contains(program.Status) && program.IsDeployedToIntegration).ToList();
        //            programIdList = programList.Select(c => c.PlanProgramId).Distinct().ToList();
        //            if (programIdList.Count > 0)
        //            {
        //                programIdList.Concat(programIdForTactic);
        //            }
        //            else
        //            {
        //                programIdList = programIdForTactic;
        //            }

        //            if (campaignList.Count > 0 || programList.Count > 0 || tacticList.Count > 0)
        //            {
        //                #region "Validate Mappings with SFDC fields"
        //                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
        //                _isResultError = SetMappingDetails();
        //                if (!_isResultError)
        //                {
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
        //                }
        //                else
        //                {
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Error, "Set Mapping details.");
        //                    return;
        //                }
        //                #endregion
        //            }

        //            //System.Diagnostics.Trace.WriteLine("Step 2 End:" + DateTime.Now.ToString());
        //            // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
        //            #endregion

        //            #region "Sync Campaign Data"
        //            if (campaignList.Count > 0)
        //            {
        //                try
        //                {
        //                    //System.Diagnostics.Trace.WriteLine("Step 3 Start:" + DateTime.Now.ToString());
        //                    #region "Get Campaign CustomFieldlist"
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Campaign.");
        //                    _mappingCustomFields = CreateMappingCustomFieldDictionary(campaignIdList, Enums.EntityType.Campaign.ToString());
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Campaign.");

        //                    #endregion
        //                    //System.Diagnostics.Trace.WriteLine("Step 3 End:" + DateTime.Now.ToString());
        //                    //System.Diagnostics.Trace.WriteLine("Step 4 Start:" + DateTime.Now.ToString());
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "SyncCampaingData process start.");
        //                    page = 0;
        //                    total = campaignList.Count;
        //                    maxpage = (total / pushRecordBatchSize);
        //                    List<Plan_Campaign> lstPagedlistCampaign = new List<Plan_Campaign>();
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, "Total No. of Campaign: " + total);
        //                    while (page <= maxpage)
        //                    {
        //                        lstPagedlistCampaign = new List<Plan_Campaign>();
        //                        lstPagedlistCampaign = campaignList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList();

        //                        sbMessage = new StringBuilder();

        //                        for (int index = 0; index < lstPagedlistCampaign.Count; index++)
        //                        {
        //                            lstPagedlistCampaign[index] = SyncCampaingData(lstPagedlistCampaign[index], ref sbMessage);

        //                            // Save 10 log records to Table.
        //                            if (((index + 1) % logRecordSize) == 0)
        //                            {
        //                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
        //                                sbMessage = new StringBuilder();
        //                            }
        //                        }

        //                        if (!string.IsNullOrEmpty(sbMessage.ToString()))
        //                        {
        //                            // Save remaining log records to Table.
        //                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
        //                        }
        //                        db.SaveChanges();
        //                        page++;
        //                    }
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncCampaingData process end.");
        //                    //System.Diagnostics.Trace.WriteLine("Step 4 End:" + DateTime.Now.ToString());
        //                }
        //                catch (Exception ex)
        //                {
        //                    _isResultError = true;
        //                    string exMessage = Common.GetInnermostException(ex);
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing Campaign data to Salesforce: " + exMessage);
        //                }
        //            } 
        //            #endregion
        //            //System.Diagnostics.Trace.WriteLine("Step 5 Start:" + DateTime.Now.ToString());
        //            #region "Sync Program Data"
        //            if (programList.Count > 0)
        //            {
        //                try
        //                {
        //                    #region "Get Program Customfield list"
        //                    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
        //                    programIdList = programList.Select(c => c.PlanProgramId).ToList();

        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Program.");
        //                    var lstCustomFieldsprogram = CreateMappingCustomFieldDictionary(programIdList, Enums.EntityType.Program.ToString());
        //                    if (_mappingCustomFields == null)
        //                        _mappingCustomFields = new List<CustomFiledMapping>();
        //                    _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldsprogram).ToList();
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Program.");

        //                    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
        //                    #endregion

        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "SyncProgramData process start.");
        //                    page = 0;
        //                    total = programList.Count;
        //                    maxpage = (total / pushRecordBatchSize);
        //                    List<Plan_Campaign_Program> lstPagedlistProgram = new List<Plan_Campaign_Program>();
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, "Total No. of Program: " + total);
        //                    while (page <= maxpage)
        //                    {
        //                        lstPagedlistProgram = new List<Plan_Campaign_Program>();
        //                        lstPagedlistProgram = programList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList();

        //                        sbMessage = new StringBuilder();

        //                        for (int index = 0; index < lstPagedlistProgram.Count; index++)
        //                        {
        //                            lstPagedlistProgram[index] = SyncProgramData(lstPagedlistProgram[index], ref sbMessage);

        //                            // Save 10 log records to Table.
        //                            if (((index + 1) % logRecordSize) == 0)
        //                            {
        //                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
        //                                sbMessage = new StringBuilder();
        //                            }
        //                        }

        //                        if (!string.IsNullOrEmpty(sbMessage.ToString()))
        //                        {
        //                            // Save remaining log records to Table.
        //                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
        //                        }
        //                        db.SaveChanges();
        //                        page++;
        //                    }
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncProgramData process end.");
        //                }
        //                catch (Exception ex)
        //                {
        //                    _isResultError = true;
        //                    string exMessage = Common.GetInnermostException(ex);
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing Program data to Salesforce: " + exMessage);
        //                }
        //            } 
        //            #endregion

        //            //System.Diagnostics.Trace.WriteLine("Step 5 End:" + DateTime.Now.ToString());

        //            #region "Make list of updated tactic after last sync and sync to SFDC"

        //            #region "Create list of created/updated tactics after last sync"
        //            int minSyncTacCount = 10;
        //            List<Plan_Campaign_Program_Tactic> syncTactics = new List<Plan_Campaign_Program_Tactic>();
        //            DateTime? lastInstanceSyncDate = null;
        //            List<Plan_Campaign_Program_Tactic> lstUpdatedTactic = new List<Plan_Campaign_Program_Tactic>();

        //            #region "Identify list of tactic updated/created"
        //            List<int> lstCreatedTacIds = new List<int>();
        //            // Start - Get list of created tactic.
        //            var lstCreatedTactic = tacticList.Where(tac => string.IsNullOrEmpty(tac.IntegrationInstanceTacticId)).Select(tac => tac).ToList(); // Get list of created tactics.
        //            if (lstCreatedTactic != null && lstCreatedTactic.Count > 0)
        //            {
        //                syncTactics.AddRange(lstCreatedTactic); // Add tactics those must be create into salesforce.
        //                lstCreatedTacIds = lstCreatedTactic.Select(tac => tac.PlanTacticId).ToList();
        //            }
        //            // End - Get list of created tactic.
        //            // Start - Get update tactic list.
        //            lstUpdatedTactic = tacticList.Where(tac => !lstCreatedTacIds.Contains(tac.PlanTacticId)).Select(tac => tac).ToList(); // Get list of tactic those going for update to salesforce.
        //            // End - Get update tactic list.

        //            #endregion

        //            if (tacticList.Count > minSyncTacCount)
        //            {
        //                // if there is no updated tactic means all created tactic then no need to get latest sync start date.
        //                if (lstUpdatedTactic != null && lstUpdatedTactic.Count > 0)
        //                {
        //                    #region "Determine last sync date"
        //                    if (_integrationInstanceId > 0)
        //                    {
        //                        string strPushTacticData = Enums.IntegrationInstanceSectionName.PushTacticData.ToString();
        //                        string strSyncSuccessStatus = Enums.SyncStatus.Success.ToString();
        //                        var lstInstanceSections = db.IntegrationInstanceSections.Where(inst => inst.IntegrationInstanceId == _integrationInstanceId && inst.SectionName == strPushTacticData && inst.Status == strSyncSuccessStatus).OrderByDescending(inst => inst.SyncStart).Select(inst => new { IntegrationInstanceSectionId = inst.IntegrationInstanceSectionId, SyncStart = inst.SyncStart, SyncEnd = inst.SyncEnd }).ToList();
        //                        if (lstInstanceSections != null && lstInstanceSections.Count > 0)
        //                        {
        //                            string strTacticType = EntityType.Tactic.ToString();
        //                            List<int> InstanceSectionIds = lstInstanceSections.Select(sec => sec.IntegrationInstanceSectionId).ToList();
        //                            var tblPlanEntityLog = db.IntegrationInstancePlanEntityLogs.Where(log => InstanceSectionIds.Contains(log.IntegrationInstanceSectionId) && log.EntityType == strTacticType).Select(log => new { IntegrationInstanceSectionId = log.IntegrationInstanceSectionId, EntityId = log.EntityId }).ToList();
        //                            //var lstInstSections = (from entLog in db.IntegrationInstancePlanEntityLogs
        //                            //                       join sec in lstInstanceSections on entLog.IntegrationInstanceSectionId equals sec.IntegrationInstanceSectionId

        //                            // Get most recent sync section based on IntegrationInstancePlanEntityLog table tactic pushed count.
        //                            foreach (var section in lstInstanceSections)
        //                            {
        //                                var lstEntity = tblPlanEntityLog.Where(log => log.IntegrationInstanceSectionId == section.IntegrationInstanceSectionId).Select(log => log.EntityId).ToList();
        //                                // Get Sync date of Section; if pushced tactic count more than 1 for this specific "PushTacticData" section.
        //                                if (lstEntity != null && lstEntity.Count > 1)
        //                                {
        //                                    lastInstanceSyncDate = section.SyncEnd;       // Get most recent sync "PushTacticData" section syncStart Date.
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    #endregion
        //                }
        //            }
        //            #endregion

        //            #region "Filter 'lstUpdatedTactic' list based on  lastInstanceSyncDate"
        //            if (lastInstanceSyncDate.HasValue)
        //            {
        //                #region "Declared local variables"
        //                Dictionary<string, string> lstMappingTacticFields = new Dictionary<string, string>();
        //                string strPlanName = Enums.SFDCGlobalFields.PlanName.ToString();
        //                lstMappingTacticFields = _mappingTactic;
        //                #endregion

        //                #region "Plan Modified Date: PlanName field configured in Salesforce Instance then identify then recently updated all plans and push respective plan tactics to 'syncTactics' list"
        //                if (_mappingTactic.ContainsKey(strPlanName))    // if PlanName field configured in Salesforce Instance then identify then recently updated all plans.
        //                {
        //                    #region "Get All Plan Modified tactics after last Sync Date"
        //                    var lstUpdatePlan = lstPlan.Where(pln => pln.ModifiedDate >= lastInstanceSyncDate.Value).ToList();
        //                    // Identify the PlanName updated after last instance sync date and Add all respective plan tactics to "syncTactics" list.
        //                    if (lstUpdatePlan != null && lstUpdatePlan.Count > 0)
        //                    {
        //                        var updtPlanIds = lstUpdatePlan.Select(pln => pln.PlanId).ToList();
        //                        var lstPlanUpdateTactics = lstUpdatedTactic.Where(tac => updtPlanIds.Contains(tac.Plan_Campaign_Program.Plan_Campaign.PlanId));
        //                        if (lstPlanUpdateTactics != null && lstPlanUpdateTactics.Count() > 0)
        //                        {
        //                            // Add modified tactics to "syncTactics" list.
        //                            syncTactics.AddRange(lstPlanUpdateTactics);

        //                            // remove modified tactics list from updated tactic list and assign to same variable.
        //                            lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !lstPlanUpdateTactics.Contains(updTac)).ToList();
        //                        }
        //                    }
        //                    #endregion

        //                    #region "Linked Tactic Plan Name Modified: Get list of Tactic"
        //                    if (lstUpdatedTactic != null && lstUpdatedTactic.Count > 0)
        //                    {
        //                        var linkedTactic = lstUpdatedTactic.Where(tac => (tac.LinkedTacticId.HasValue) && (tac.LinkedTacticId.Value > 0)).Select(tac => tac).ToList();
        //                        if (linkedTactic != null && linkedTactic.Count > 0)
        //                        {
        //                            List<int> linkedPlanIds = new List<int>();
        //                            linkedPlanIds = linkedTactic.Where(tac => (tac.LinkedPlanId.HasValue) && (tac.LinkedPlanId.Value > 0)).Select(tac => tac.LinkedPlanId.Value).ToList();
        //                            var linkedUpdtPlanIds = db.Plans.Where(pln => (linkedPlanIds.Contains(pln.PlanId)) && (pln.ModifiedDate >= lastInstanceSyncDate)).Select(pln => pln.PlanId).ToList();
        //                            if (linkedUpdtPlanIds != null && linkedUpdtPlanIds.Count > 0)
        //                            {
        //                                linkedTactic = linkedTactic.Where(tac => tac.LinkedPlanId.HasValue && linkedUpdtPlanIds.Contains(tac.LinkedPlanId.Value)).ToList();

        //                                // Add modified tactics to syncTactics list.
        //                                syncTactics.AddRange(linkedTactic);

        //                                // remove modified tactics list from updated tactic list and assign to same variable.
        //                                lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !linkedTactic.Contains(updTac)).ToList();
        //                            }
        //                        }
        //                    }

        //                    #endregion
        //                }
        //                #endregion

        //                #region "Tactic Modified date: Get list of tactics updated after last Instance Sync date"
        //                if (lstUpdatedTactic != null && lstUpdatedTactic.Count > 0)
        //                {
        //                    var lstModifdTactics = lstUpdatedTactic.Where(tac => tac.ModifiedDate >= lastInstanceSyncDate.Value).ToList();
        //                    if (lstModifdTactics != null && lstModifdTactics.Count > 0)
        //                    {
        //                        // Add modified tactics to syncTactics list.
        //                        syncTactics.AddRange(lstModifdTactics);

        //                        // remove modified tactics list from updated tactic list and assign to same variable.
        //                        lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !lstModifdTactics.Contains(updTac)).ToList();
        //                    }
        //                }
        //                #endregion

        //                #region "Budgeted(Planned) Cost: Get all tactics updated planned cost after last Instance Sync Date"
        //                string strBudgetedCostActualField = "Cost";
        //                if (lstUpdatedTactic != null && lstUpdatedTactic.Count > 0 && _mappingTactic.ContainsKey(strBudgetedCostActualField))
        //                {
        //                    var lstBdgtCostUpdateTactics = (from updTac in lstUpdatedTactic
        //                                                    join bcost in db.Plan_Campaign_Program_Tactic_Cost on updTac.PlanTacticId equals bcost.PlanTacticId
        //                                                    where bcost.CreatedDate >= lastInstanceSyncDate
        //                                                    select updTac).ToList();
        //                    if (lstBdgtCostUpdateTactics != null && lstBdgtCostUpdateTactics.Count > 0)
        //                    {
        //                        // Add modified tactics to syncTactics list.
        //                        syncTactics.AddRange(lstBdgtCostUpdateTactics);

        //                        // remove modified tactics list from updated tactic list and assign to same variable.
        //                        lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !lstBdgtCostUpdateTactics.Contains(updTac)).ToList();
        //                    }

        //                }
        //                #endregion

        //                #region "Actual Cost: Get all tactics updated actual cost after last Instance Sync Date"

        //                if (lstUpdatedTactic != null && lstUpdatedTactic.Count > 0 && _mappingTactic.ContainsKey(strActualCostActualField))
        //                {
        //                    #region "Get updated tactic for those child line items cost updated"
        //                    var lstlineActualCostUpdateTactics = (from updTac in lstUpdatedTactic
        //                                                          join line in db.Plan_Campaign_Program_Tactic_LineItem on updTac.PlanTacticId equals line.PlanTacticId
        //                                                          join lineCost in db.Plan_Campaign_Program_Tactic_LineItem_Actual on line.PlanLineItemId equals lineCost.PlanLineItemId
        //                                                          where line.IsDeleted == true && lineCost.CreatedDate >= lastInstanceSyncDate
        //                                                          select updTac).ToList();
        //                    if (lstlineActualCostUpdateTactics != null && lstlineActualCostUpdateTactics.Count > 0)
        //                    {
        //                        // Add modified tactics to syncTactics list.
        //                        syncTactics.AddRange(lstlineActualCostUpdateTactics);

        //                        // remove modified tactics list from updated tactic list and assign to same variable.
        //                        lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !lstlineActualCostUpdateTactics.Contains(updTac)).ToList();
        //                    }
        //                    #endregion

        //                    #region "Get updated tactic for those modified it's cost"
        //                    if (lstUpdatedTactic != null && lstUpdatedTactic.Count > 0)
        //                    {
        //                        string costStageTitle = "Cost";
        //                        var lstActualCostUpdateTactics = (from updTac in lstUpdatedTactic
        //                                                          join tacActual in db.Plan_Campaign_Program_Tactic_Actual on updTac.PlanTacticId equals tacActual.PlanTacticId
        //                                                          where tacActual.StageTitle == costStageTitle && tacActual.CreatedDate >= lastInstanceSyncDate
        //                                                          select updTac).ToList();
        //                        if (lstActualCostUpdateTactics != null && lstActualCostUpdateTactics.Count > 0)
        //                        {
        //                            // Add modified tactics to syncTactics list.
        //                            syncTactics.AddRange(lstActualCostUpdateTactics);

        //                            // remove modified tactics list from updated tactic list and assign to same variable.
        //                            lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !lstActualCostUpdateTactics.Contains(updTac)).ToList();
        //                        }
        //                    }
        //                    #endregion

        //                }
        //                #endregion
        //            }

        //            #endregion

        //            #endregion

        //            // update "tacticlist" by filtered Created & Updated tactics, if system get lastInstanceSyncDate else remains as it is.
        //            if (lastInstanceSyncDate.HasValue)
        //            {
        //                tacticList = syncTactics;
        //            }

        //            #region "Sync Tactic Data"
        //            if (tacticList.Count > 0)
        //            {
        //                List<int> lstProcessTacIds = new List<int>();
        //                try
        //                {
        //                    //System.Diagnostics.Trace.WriteLine("Step 6 Start:" + DateTime.Now.ToString());
        //                    #region "Get Tacic Customfield list & Actual Cost"
        //                    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
        //                    List<int> tacticIdList = tacticList.Select(c => c.PlanTacticId).Distinct().ToList();

        //                    // if user has mapped Actualcost field under Instance mapping section then retrieve ActualCost values for all tactics.
        //                    if (_mappingTactic.ContainsKey(strActualCostActualField))
        //                    _mappingTactic_ActualCost = Common.CalculateActualCostTacticslist(tacticIdList);

        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Tactic.");
        //                    var lstCustomFieldstactic = CreateMappingCustomFieldDictionary(tacticIdList, Enums.EntityType.Tactic.ToString());
        //                    if (_mappingCustomFields == null)
        //                        _mappingCustomFields = new List<CustomFiledMapping>();
        //                    _mappingCustomFields = _mappingCustomFields.Concat(lstCustomFieldstactic).ToList();
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for Tactic.");
        //                    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
        //                    #endregion
        //                    //System.Diagnostics.Trace.WriteLine("Step 6 End:" + DateTime.Now.ToString());
        //                    page = 0;
        //                    total = tacticList.Count;
        //                    maxpage = (total / pushRecordBatchSize);
        //                    List<Plan_Campaign_Program_Tactic> lstPagedlistTactic = new List<Plan_Campaign_Program_Tactic>();
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, "Total No. of Tactics: " + total);
        //                    Plan_Campaign_Program_Tactic srcTactic,objLinkedTactic;
        //                    while (page <= maxpage)
        //                    {
        //                        //System.Diagnostics.Trace.WriteLine("Step paging" + page.ToString() + " Start:" + DateTime.Now.ToString());
        //                        lstPagedlistTactic = new List<Plan_Campaign_Program_Tactic>();
        //                        lstPagedlistTactic = tacticList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList();

        //                        sbMessage = new StringBuilder();
        //                        for (int index = 0; index < lstPagedlistTactic.Count; index++)
        //                        {
        //                            //System.Diagnostics.Trace.WriteLine("Step Linked Tactic" + (index + 1).ToString() + " Start:" + DateTime.Now.ToString());
        //                            #region "Get Linked Tactic"
        //                            int linkedTacticId = 0;
        //                            srcTactic = new Plan_Campaign_Program_Tactic();
        //                            objLinkedTactic = new Plan_Campaign_Program_Tactic();
        //                            srcTactic = lstPagedlistTactic[index];
        //                            if (srcTactic != null)
        //                            {
        //                                linkedTacticId = (srcTactic != null && srcTactic.LinkedTacticId.HasValue) ? srcTactic.LinkedTacticId.Value : 0;
        //                            }
        //                            if (linkedTacticId > 0)
        //                            {
        //                                objLinkedTactic = tacticList.Where(tactic => tactic.PlanTacticId == linkedTacticId).FirstOrDefault();
        //                            }
        //                            #endregion
        //                            //System.Diagnostics.Trace.WriteLine("Step Linked Tactic" + (index + 1).ToString() + "-" + linkedTacticId.ToString() + " End:" + DateTime.Now.ToString());

        //                            //System.Diagnostics.Trace.WriteLine("Step Tactic" + (index + 1).ToString() + " Start:" + DateTime.Now.ToString());
        //                            lstPagedlistTactic[index] = SyncTacticData(lstPagedlistTactic[index], ref sbMessage);
        //                            //System.Diagnostics.Trace.WriteLine("Step Tactic" + (index + 1).ToString() + " End:" + DateTime.Now.ToString());
        //                            lstProcessTacIds.Add(lstPagedlistTactic[index].PlanTacticId);
        //                            // Save 10 log records to Table.
        //                            //if (((index + 1) % logRecordSize) == 0)
        //                            //{
        //                            //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
        //                            //    sbMessage = new StringBuilder();
        //                            //}

        //                        }

        //                        if (!string.IsNullOrEmpty(sbMessage.ToString()))
        //                        {
        //                            // Save remaining log records to Table.
        //                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
        //                            sbMessage = new StringBuilder();
        //                        }
        //                        db.SaveChanges();
        //                        //System.Diagnostics.Trace.WriteLine("Step paging" + page.ToString() + " End:" + DateTime.Now.ToString());
        //                        page++;
        //                    }
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncTacticData process end.");
        //                    //System.Diagnostics.Trace.WriteLine("CallingSP Start:" + DateTime.Now.ToString());
        //                    UpdateLinkedTacticComment(lstProcessTacIds, tacticList, lstCreatedTacIds, lstTac_LinkTacMapping);
        //                    //System.Diagnostics.Trace.WriteLine("CallingSP End:" + DateTime.Now.ToString());
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Updating Tactic log & Comment details end.");
        //                }
        //                catch (Exception ex)
        //                {
        //                    _isResultError = true;
        //                    string exMessage = Common.GetInnermostException(ex);
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing Tactic data to Salesforce: " + exMessage);
        //                }

        //            } 
        //            #endregion

        //            #region "Sync Improvement Tactic Data"
        //            List<Plan_Improvement_Campaign_Program_Tactic> improvetacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(tactic => planIds.Contains(tactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).ToList();
        //            if (improvetacticList.Count > 0)
        //            {
        //                try
        //                {
        //                    page = 0;
        //                    total = improvetacticList.Count;
        //                    maxpage = (total / pushRecordBatchSize);
        //                    List<Plan_Improvement_Campaign_Program_Tactic> lstPagedlistIMPTactic = new List<Plan_Improvement_Campaign_Program_Tactic>();
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, "Total No. of ImprovementTactic: " + total);
        //                    while (page <= maxpage)
        //                    {
        //                        lstPagedlistIMPTactic = new List<Plan_Improvement_Campaign_Program_Tactic>();
        //                        lstPagedlistIMPTactic = improvetacticList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList();

        //                        sbMessage = new StringBuilder();

        //                        for (int index = 0; index < lstPagedlistIMPTactic.Count; index++)
        //                        {
        //                            lstPagedlistIMPTactic[index] = SyncImprovementData(lstPagedlistIMPTactic[index], ref sbMessage);

        //                            // Save 10 log records to Table.
        //                            if (((index + 1) % logRecordSize) == 0)
        //                            {
        //                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
        //                                sbMessage = new StringBuilder();
        //                            }
        //                        }

        //                        if (!string.IsNullOrEmpty(sbMessage.ToString()))
        //                        {
        //                            // Save remaining log records to Table.
        //                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
        //                        }
        //                        db.SaveChanges();
        //                        page++;
        //                    }
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncImprovementData process end.");
        //                }
        //                catch (Exception ex)
        //                {
        //                    _isResultError = true;
        //                    string exMessage = Common.GetInnermostException(ex);
        //                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing ImprovementTactic data to Salesforce: " + exMessage);
        //                }
        //            } 
        //            #endregion

        //            #region "Old-Code"
        //            // We remove deletion flow from salesforce so now below code not require
        //            //List<Plan_Improvement_Campaign> impcampaignList = db.Plan_Improvement_Campaign.Where(campaign => planIds.Contains(campaign.ImprovePlanId)).ToList();
        //            //if (impcampaignList.Count() > 0)
        //            //{
        //            //    page = 0;
        //            //    total = impcampaignList.Count();
        //            //    maxpage = (total / pageSize);
        //            //    List<Plan_Improvement_Campaign> lstPagedlistIMPCampaign = new List<Plan_Improvement_Campaign>();
        //            //    while (page <= maxpage)
        //            //    {
        //            //        lstPagedlistIMPCampaign = new List<Plan_Improvement_Campaign>();
        //            //        lstPagedlistIMPCampaign = impcampaignList.Skip(page * pageSize).Take(pageSize).ToList();
        //            //        using (var scope = new TransactionScope())
        //            //        {
        //            //            for (int index = 0; index < lstPagedlistIMPCampaign.Count; index++)
        //            //            {
        //            //                lstPagedlistIMPCampaign[index] = SyncImprovementCampaingData(lstPagedlistIMPCampaign[index]);
        //            //            }
        //            //            db.SaveChanges();
        //            //            scope.Complete();
        //            //        }
        //            //        page++;
        //            //    }
        //            //} 
        //            #endregion
        //        }
        //    }
        //    catch (SalesforceException e)
        //    {
        //        _isResultError = true;
        //        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "System error occurred while syncing data with Salesforce.", Enums.SyncStatus.Error, DateTime.Now));
        //        _ErrorMessage = Common.GetInnermostException(e);
        //        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while syncing with multiple Tactic: " + _ErrorMessage);
        //    }
        //    catch (Exception e)
        //    {
        //        string exMessage = Common.GetInnermostException(e);
        //        _isResultError = true;
        //        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "System error occurred while syncing data with Salesforce.", Enums.SyncStatus.Error, DateTime.Now));
        //        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while syncing with multiple Tactic: " + exMessage);
        //    }
        //} 
        #endregion

        private void SyncDatabyEntity(object objEntity = null)
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {
                //System.Diagnostics.Trace.WriteLine("Step 1 Start:" + DateTime.Now.ToString());
                #region "Create CampaignIdList, ProgramIdList & TacticIdList"
                #region "Declare local varialbes"
                List<int> campaignIdList = new List<int>();
                List<int> programIdList = new List<int>();
                List<int> tacticIdList = new List<int>();
                List<int> LinkedTacticIds = new List<int>();
                List<Plan> lstPlan = new List<Plan>();
                List<int> planIds = new List<int>();
                List<Plan_Campaign> campaignList = new List<Plan_Campaign>();
                List<Plan_Campaign_Program> programList = new List<Plan_Campaign_Program>();
                List<Plan_Campaign_Program_Tactic> tacticList = new List<Plan_Campaign_Program_Tactic>();
                List<Plan_Campaign_Program_Tactic> tblTactic = new List<Plan_Campaign_Program_Tactic>();
                List<TacticLinkedTacMapping> lstTac_LinkTacMapping = new List<TacticLinkedTacMapping>();
                List<int> lstCreatedTacIds = new List<int>();
                List<Plan_Campaign_Program_Tactic> lstCreatedTactic = new List<Plan_Campaign_Program_Tactic>();
                List<Plan_Campaign_Program_Tactic> lstUpdatedTactic = new List<Plan_Campaign_Program_Tactic>();
                List<int> lstProcessTacIds = new List<int>();
                List<SFDCWithMarketoList> IsSFDCWithMarketoList = new List<SFDCWithMarketoList>();

                #endregion

                #region "Get Parent-Child Entity list based on EntityType"
                if (EntityType.Campaign.Equals(_entityType))
                {
                    if (objEntity != null)
                    {
                        campaignList.Add((Plan_Campaign)objEntity);
                        campaignIdList = campaignList.Select(cmpgn => cmpgn.PlanCampaignId).ToList();
                    }
                    programList = db.Plan_Campaign_Program.Where(program => campaignIdList.Contains(program.PlanCampaignId) && !program.IsDeleted).ToList();
                    programIdList = programList.Select(c => c.PlanProgramId).ToList();
                    tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).ToList();
                    tacticList = tblTactic.Where(tactic => programIdList.Contains(tactic.PlanProgramId)).ToList();
                }
                else if (EntityType.Program.Equals(_entityType))
                {
                    if (objEntity != null)
                    {
                        programList.Add((Plan_Campaign_Program)objEntity);
                        programIdList = programList.Select(prg => prg.PlanProgramId).ToList();
                    }
                    tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).ToList();
                    tacticList = tblTactic.Where(tactic => programIdList.Contains(tactic.PlanProgramId)).ToList();
                }
                else if (EntityType.Tactic.Equals(_entityType))
                {
                    if (objEntity != null)
                    {
                        tacticList.Add((Plan_Campaign_Program_Tactic)objEntity);
                    }
                    //tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).ToList();
                }
                else
                {
                    string published = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString();
                    // Add By Nishant Sheth
                    //Make Hireachy for Marketo sync on SFDC
                    // Modified By Nishant Sheth
                    // Modified Date: 15-Jun-2016
                    // Desc ::  Get model list which are configure like salesforce integration instance as pull for marketo and same instance for another model to push salesforce.
                    IsSFDCWithMarketoList = db.Models.Where(model => model.IsDeleted == false &&
                       (model.IntegrationInstanceIdINQ == _integrationInstanceId || model.IntegrationInstanceIdMQL == _integrationInstanceId
                       || model.IntegrationInstanceIdCW == _integrationInstanceId || model.IntegrationInstanceId == _integrationInstanceId)
                       && (model.IntegrationInstanceMarketoID != _integrationInstanceId || model.IntegrationInstanceMarketoID == null))
                       .Select(model => new SFDCWithMarketoList
                       {
                           ModelId = model.ModelId,
                           IntegrationInstanceMarketoID = model.IntegrationInstanceMarketoID,
                           IntegrationInstanceId = model.IntegrationInstanceId
                       }).ToList();

                    if (IsSFDCWithMarketoList.Count > 0)
                    {
                        //var IsCheckMarketoModel = IsSFDCWithMarketoList.Where(model => model.IntegrationInstanceMarketoID != null).Any();

                        if (IsSFDCWithMarketoList.Where(model => model.IntegrationInstanceMarketoID != null).Any())
                    {
                        _IsSFDCWithMarketo = true;
                            List<int> SFDCWithMarketoModelIds = new List<int>();
                            SFDCWithMarketoModelIds = IsSFDCWithMarketoList.Select(model => model.ModelId).ToList();
                            lstPlan = db.Plans.Where(p => SFDCWithMarketoModelIds.Contains(p.ModelId) && p.Model.Status.Equals(published)).ToList();
                        }
                        else
                        {
                            _IsSFDCWithMarketo = false;
                            lstPlan = db.Plans.Where(p => p.Model.IntegrationInstanceId == _integrationInstanceId && p.Model.Status.Equals(published)).ToList();
                        }
                    }
                    else
                    {
                        _IsSFDCWithMarketo = false;
                        lstPlan = db.Plans.Where(p => p.Model.IntegrationInstanceId == _integrationInstanceId && p.Model.Status.Equals(published)).ToList();
                    }
                    // End By Nishant Sheth

                    if (lstPlan != null && lstPlan.Count > 0)
                    {
                        planIds = lstPlan.Select(p => p.PlanId).ToList();
                        campaignList = db.Plan_Campaign.Where(campaign => planIds.Contains(campaign.PlanId) && !campaign.IsDeleted).ToList();
                        campaignIdList = campaignList.Select(c => c.PlanCampaignId).ToList();
                        programList = db.Plan_Campaign_Program.Where(program => campaignIdList.Contains(program.PlanCampaignId) && !program.IsDeleted).ToList();
                        programIdList = programList.Select(c => c.PlanProgramId).ToList();
                        // Add By Nishant Sheth
                        //Make Hireachy for Marketo sync on SFDC
                        if (IsSFDCWithMarketoList.Count > 0)
                        {
                            tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted
                            && (tactic.IsSyncMarketo.HasValue && tactic.IsSyncMarketo.Value == true) || (tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true)).ToList();
                        }
                        else
                        {
                            tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted
                            && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).ToList();
                        }
                        // End By Nishant Sheth
                        tacticList = tblTactic.Where(tactic => programIdList.Contains(tactic.PlanProgramId)).ToList();


                    }
                }
                #endregion

                #region "Create Tactic-Linked Tactic mapping list:- Push latest year plan tactic as Plan Tactic and other as linked"
                if (tacticList != null && tacticList.Count > 0)
                {
                    var lstTac_LinkTacMappIds = tacticList.Where(tac => tac.LinkedTacticId.HasValue).Select(tac => new { PlanTacticId = tac.PlanTacticId, PlanTacic = tac, LinkedTacticId = tac.LinkedTacticId.Value }).ToList();
                    if (lstTac_LinkTacMappIds != null && lstTac_LinkTacMappIds.Count > 0)
                    {
                        #region "Declare local variables"
                        TacticLinkedTacMapping objTacLinkMapping;
                        Plan_Campaign_Program_Tactic linkedTactic;
                        string strOrgnlTacPlanyear, strLnkdTacPlanYear;
                        int orgnlTacPlanYear, lnkdTacPlanYear;
                        #endregion

                        foreach (var tac in lstTac_LinkTacMappIds.ToList())
                        {
                            #region "Initialize local variables"
                            objTacLinkMapping = new TacticLinkedTacMapping();
                            linkedTactic = new Plan_Campaign_Program_Tactic();
                            strOrgnlTacPlanyear = string.Empty; strLnkdTacPlanYear = string.Empty;
                            orgnlTacPlanYear = 0; lnkdTacPlanYear = 0;
                            #endregion

                            // get linked tactic
                            if (EntityType.Tactic.Equals(_entityType))
                            {
                                linkedTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == tac.LinkedTacticId && statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && tactic.IsSyncSalesForce.HasValue && tactic.IsSyncSalesForce.Value == true).FirstOrDefault();
                            }
                            else if (EntityType.Campaign.Equals(_entityType) || EntityType.Program.Equals(_entityType))
                            {
                                linkedTactic = tblTactic.Where(tactic => tactic.PlanTacticId == tac.LinkedTacticId).FirstOrDefault();
                            }
                            else
                            {
                                linkedTactic = tacticList.Where(tactc => tactc.PlanTacticId == tac.LinkedTacticId).FirstOrDefault();
                            }
                            if (linkedTactic != null)
                            {
                                if (!tacticList.Contains(linkedTactic))
                                    tacticList.Add(linkedTactic);

                                #region "Get both tactics Plan year"
                                strOrgnlTacPlanyear = tac.PlanTacic.Plan_Campaign_Program.Plan_Campaign.Plan.Year; // Get Orginal Tactic Plan Year.
                                strLnkdTacPlanYear = linkedTactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year; // Get Linked Tactic Plan Year.

                                //parse plan year to int.
                                if (!string.IsNullOrEmpty(strOrgnlTacPlanyear) && !string.IsNullOrEmpty(strLnkdTacPlanYear))
                                {
                                    orgnlTacPlanYear = int.Parse(strOrgnlTacPlanyear);
                                    lnkdTacPlanYear = int.Parse(strLnkdTacPlanYear);
                                }
                                #endregion

                                #region "Insert latest plan year tactic to lstTac_LinkTacMapping list"
                                //Identify latest Tactic and add it to "lstTac_LinkTacMapping" list to push latest tactics to SFDC.
                                if (lnkdTacPlanYear > orgnlTacPlanYear)
                                {
                                    objTacLinkMapping.PlanTactic = linkedTactic; // set latest plan year tactic as PlanTactic to model and pass to SFDC.
                                    objTacLinkMapping.LinkedTactic = tac.PlanTacic; // set old tactic as LinkedTactic and update its IntegrationInstanceTacticId & Comment after pushing origional Tactic.
                                    tacticList.Remove(tac.PlanTacic);    // Remove the old tactic from tacticlist which is not going to push to SFDC.
                                }
                                else
                                {
                                    objTacLinkMapping.PlanTactic = tac.PlanTacic; // set latest plan year tactic as PlanTactic to model and pass to SFDC.
                                    objTacLinkMapping.LinkedTactic = linkedTactic; // set old tactic as LinkedTactic and update its IntegrationInstanceTacticId & Comment after pushing origional Tactic.
                                    tacticList.Remove(linkedTactic);    // Remove the old tactic from tacticlist which is not going to push to SFDC.
                                }
                                #endregion

                                #region "Remove linked Tactic from current list."
                                var objLnkdTac = lstTac_LinkTacMappIds.FirstOrDefault(t => t.PlanTacticId == tac.LinkedTacticId);
                                if (objLnkdTac != null)
                                {
                                    lstTac_LinkTacMappIds.Remove(objLnkdTac);
                                }
                                #endregion

                                lstTac_LinkTacMapping.Add(objTacLinkMapping);
                            }
                        }
                    }
                }
                #endregion

                tacticIdList = tacticList.Select(tac => tac.PlanTacticId).ToList();
                List<int> campaignIdForTactic = tacticList.Where(tactic => string.IsNullOrWhiteSpace(tactic.Plan_Campaign_Program.Plan_Campaign.IntegrationInstanceCampaignId)).Select(tactic => tactic.Plan_Campaign_Program.PlanCampaignId).ToList();
                List<int> programIdForTactic = tacticList.Where(tactic => string.IsNullOrWhiteSpace(tactic.Plan_Campaign_Program.IntegrationInstanceProgramId)).Select(tactic => tactic.PlanProgramId).ToList();
                //System.Diagnostics.Trace.WriteLine("Step 1 End:" + DateTime.Now.ToString());
                //System.Diagnostics.Trace.WriteLine("Step 2 Start:" + DateTime.Now.ToString());

                // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997

                #region "Get CampaignIds"
                // Get CampaignId list to push to SFDC in case of Entity type "Campaign" or "Sync Now".
                if ((!EntityType.Tactic.Equals(_entityType)) && (!EntityType.Program.Equals(_entityType)))
                {
                    // Add By Nishant Sheth
                    //Make Hireachy for Marketo sync on SFDC
                    if (IsSFDCWithMarketoList.Count > 0)
                    {
                        campaignList = campaignList.Where(campaign => statusList.Contains(campaign.Status)).ToList();
                    }
                    else
                    {
                        campaignList = campaignList.Where(campaign => statusList.Contains(campaign.Status) && campaign.IsDeployedToIntegration).ToList();
                    }
                    // End By Nishant Sheth
                    campaignIdList = campaignList.Select(c => c.PlanCampaignId).Distinct().ToList();
                    if (campaignIdList.Count > 0)
                    {
                        campaignIdList.Concat(campaignIdForTactic);
                    }
                    else
                    {
                        campaignIdList = campaignIdForTactic;
                    }
                }
                #endregion

                #region "Get ProgramIds"
                if (!EntityType.Tactic.Equals(_entityType))
                {
                    // Add By Nishant Sheth
                    //Make Hireachy for Marketo sync on SFDC
                    if (IsSFDCWithMarketoList.Count > 0)
                    {
                        programList = programList.Where(program => statusList.Contains(program.Status)).ToList();
                    }
                    else
                    {
                        programList = programList.Where(program => statusList.Contains(program.Status) && program.IsDeployedToIntegration).ToList();
                    }
                    // End By Nishant Sheth
                    programIdList = programList.Select(c => c.PlanProgramId).Distinct().ToList();
                    if (programIdList.Count > 0)
                    {
                        programIdList.Concat(programIdForTactic);
                    }
                    else
                    {
                        programIdList = programIdForTactic;
                    }
                }
                #endregion

                #region "Validate Mappings with SFDC fields"
                if (campaignList.Count > 0 || programList.Count > 0 || tacticList.Count > 0)
                {

                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                    _isResultError = SetMappingDetails();
                    if (!_isResultError)
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                    }
                    else
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Error, "Set Mapping details.");
                        return;
                    }
                }
                #endregion
                //System.Diagnostics.Trace.WriteLine("Step 2 End:" + DateTime.Now.ToString());
                // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                #endregion


                #region "Identify list of tactic updated/created"

                // Start - Get list of created tactic.
                lstCreatedTactic = tacticList.Where(tac => string.IsNullOrEmpty(tac.IntegrationInstanceTacticId)).Select(tac => tac).ToList(); // Get list of created tactics.
                if (lstCreatedTactic != null && lstCreatedTactic.Count > 0)
                {
                    lstCreatedTacIds = lstCreatedTactic.Select(tac => tac.PlanTacticId).ToList();
                }
                // End - Get list of created tactic.
                // Start - Get update tactic list.
                lstUpdatedTactic = tacticList.Where(tac => !lstCreatedTacIds.Contains(tac.PlanTacticId)).Select(tac => tac).ToList(); // Get list of tactic those going for update to salesforce.
                // End - Get update tactic list.

                #endregion

                if (EntityType.Tactic.Equals(_entityType))
                {
                    SyncEntityData<Plan_Campaign_Program_Tactic>(EntityType.Tactic.ToString(), tacticList, tacticIdList, ref lstProcessTacIds, lstTac_LinkTacMapping);         // Push Tactics to SFDC.
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Updating Tactic log & Comment details Start.");
                    UpdateLinkedTacticComment(lstProcessTacIds, tacticList, lstCreatedTacIds, lstTac_LinkTacMapping);
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Updating Tactic log & Comment details End.");
                }
                else if (EntityType.Program.Equals(_entityType))
                {
                    SyncEntityData<Plan_Campaign_Program>(EntityType.Program.ToString(), programList, programIdList, ref lstProcessTacIds, lstTac_LinkTacMapping);      // Push Programs to SFDC.
                    SyncEntityData<Plan_Campaign_Program_Tactic>(EntityType.Tactic.ToString(), tacticList, tacticIdList, ref lstProcessTacIds, lstTac_LinkTacMapping);         // Push Tactics to SFDC.
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Updating Tactic log & Comment details Start.");
                    UpdateLinkedTacticComment(lstProcessTacIds, tacticList, lstCreatedTacIds, lstTac_LinkTacMapping);
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Updating Tactic log & Comment details End.");
                }
                else
                {
                    // Execute this code snippet in case of syncing specific Campaign or Instance.
                    SyncEntityData<Plan_Campaign>(EntityType.Campaign.ToString(), campaignList, campaignIdList, ref lstProcessTacIds, lstTac_LinkTacMapping);   // Push Campaign to SFDC.
                    SyncEntityData<Plan_Campaign_Program>(EntityType.Program.ToString(), programList, programIdList, ref lstProcessTacIds, lstTac_LinkTacMapping);      // Push Programs to SFDC.

                    // if user make Sync Now then get only updated tactics after lastsync date of that Instance.
                    if (!EntityType.Campaign.Equals(_entityType))
                    {
                        tacticList = GetLastSyncUpdatedTactics(tacticList, lstCreatedTactic, lstUpdatedTactic, lstPlan);
                        tacticIdList = tacticList.Select(tac => tac.PlanTacticId).ToList();
                    }
                    SyncEntityData<Plan_Campaign_Program_Tactic>(EntityType.Tactic.ToString(), tacticList, tacticIdList, ref lstProcessTacIds, lstTac_LinkTacMapping);         // Push Tactics to SFDC.

                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Updating Tactic log & Comment details Start.");
                    UpdateLinkedTacticComment(lstProcessTacIds, tacticList, lstCreatedTacIds, lstTac_LinkTacMapping);
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Updating Tactic log & Comment details End.");
                    if (!EntityType.Campaign.Equals(_entityType))
                    {
                        #region "Sync Improvement Tactic Data"
                        List<Plan_Improvement_Campaign_Program_Tactic> improvetacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(tactic => planIds.Contains(tactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId) && statusList.Contains(tactic.Status) && !tactic.IsDeleted && tactic.IsDeployedToIntegration == true).ToList();
                        if (improvetacticList.Count > 0)
                        {
                            List<int> lstimprovetacticIds = new List<int>();
                            SyncEntityData<Plan_Improvement_Campaign_Program_Tactic>(EntityType.ImprovementTactic.ToString(), improvetacticList, lstimprovetacticIds, ref lstProcessTacIds, lstTac_LinkTacMapping);         // Push Tactics to SFDC.
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                _isResultError = true;
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "System error occurred while syncing data with Salesforce.", Enums.SyncStatus.Error, DateTime.Now));
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while syncing with multiple Tactic: " + exMessage);
            }
        }

        private void SyncEntityData<T>(string entityType, List<T> entityList, List<int> entityIdList, ref List<int> lstProcessTacIds, List<TacticLinkedTacMapping> lstTac_LinkedTacMapping)
        {
            #region "Declare local variables"
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            StringBuilder sbMessage;
            int logRecordSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LogRecordSize"]);
            int pushRecordBatchSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IntegrationPushRecordBatchSize"]);
            string strActualCostActualField = "CostActual"; // static variable value to identify that instance has mapped Actual cost field in Instance mapping.
            int tacticId = 0;
            #endregion
            try
            {

                #region "Convert EntityList"
                int page = 0, total = 0, maxpage = 0;
                lstProcessTacIds = new List<int>(); // initialize varialbe for each entity.
                #endregion

                #region "Sync each entity Data"
                if (entityList != null && entityList.Count > 0)
                {
                    try
                    {
                        // Get ActualCost for all tactics if user has made mapping of ActualCost under Instance configuration.
                        if (EntityType.Tactic.ToString().Equals(entityType) && _mappingTactic.ContainsKey(strActualCostActualField))
                            _mappingTactic_ActualCost = Common.CalculateActualCostTacticslist(entityIdList);
                        if (!EntityType.ImprovementTactic.ToString().Equals(entityType))
                        {
                            #region "Get Entity CustomFieldlist"
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for " + entityType + ".");
                            if (_mappingCustomFields != null && _mappingCustomFields.Count > 0)
                                _mappingCustomFields.AddRange(CreateMappingCustomFieldDictionary(entityIdList, entityType));
                            else
                            {
                                _mappingCustomFields = new List<CustomFiledMapping>();
                                _mappingCustomFields = CreateMappingCustomFieldDictionary(entityIdList, entityType);
                            }
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get CustomField mapping dictionary for " + entityType + ".");

                            #endregion
                        }
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync" + entityType + "Data process start.");
                        page = 0;
                        total = entityList.Count;
                        maxpage = (total / pushRecordBatchSize);

                        var lstPagedEntityList = (dynamic)null;
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, "Total No. of " + entityType + ": " + total);
                        while (page <= maxpage)
                        {
                            sbMessage = new StringBuilder();
                            if (EntityType.Campaign.ToString().Equals(entityType) && entityList is List<Plan_Campaign>)
                            {
                                lstPagedEntityList = new List<Plan_Campaign>();
                                lstPagedEntityList = entityList.ToList().Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList() as List<Plan_Campaign>;
                                if (lstPagedEntityList != null && lstPagedEntityList.Count > 0)
                                {
                                    for (int index = 0; index < lstPagedEntityList.Count; index++)
                                    {
                                        lstPagedEntityList[index] = SyncCampaingData(lstPagedEntityList[index], ref sbMessage);
                                        lstProcessTacIds.Add(lstPagedEntityList[index].PlanCampaignId);
                                        #region "Old Code"
                                        // Save 10 log records to Table.
                                        //if (((index + 1) % logRecordSize) == 0)
                                        //{
                                        //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                                        //    sbMessage = new StringBuilder();
                                        //} 
                                        #endregion
                                    }
                                }
                            }
                            else if (EntityType.Program.ToString().Equals(entityType) && entityList is List<Plan_Campaign_Program>)
                            {
                                lstPagedEntityList = new List<Plan_Campaign_Program>();
                                lstPagedEntityList = entityList.ToList().Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList() as List<Plan_Campaign_Program>;
                                if (lstPagedEntityList != null && lstPagedEntityList.Count > 0)
                                {
                                    for (int index = 0; index < lstPagedEntityList.Count; index++)
                                    {
                                        lstPagedEntityList[index] = SyncProgramData(lstPagedEntityList[index], ref sbMessage);
                                        lstProcessTacIds.Add(lstPagedEntityList[index].PlanProgramId);
                                        #region "Old Code"
                                        // Save 10 log records to Table.
                                        //if (((index + 1) % logRecordSize) == 0)
                                        //{
                                        //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                                        //    sbMessage = new StringBuilder();
                                        //} 
                                        #endregion
                                    }
                                }
                            }
                            else if (EntityType.Tactic.ToString().Equals(entityType) && entityList is List<Plan_Campaign_Program_Tactic>)
                            {
                                lstPagedEntityList = new List<Plan_Campaign_Program_Tactic>();
                                lstPagedEntityList = entityList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList() as List<Plan_Campaign_Program_Tactic>;
                                if (lstPagedEntityList != null && lstPagedEntityList.Count > 0)
                                {
                                    for (int index = 0; index < lstPagedEntityList.Count; index++)
                                    {
                                        startDate = null; //#2097: reset startDate global variable.
                                        tacticId = lstPagedEntityList[index].PlanTacticId;
                                        if (lstTac_LinkedTacMapping != null && lstTac_LinkedTacMapping.Count > 0)
                                        {
                                            //#2097: Get Orgional Tactic.
                                            var lnkdTac = lstTac_LinkedTacMapping.Where(tac => tac.PlanTactic.PlanTacticId == tacticId).Select(tac => tac.LinkedTactic).FirstOrDefault();
                                            if (lnkdTac != null && lnkdTac.PlanTacticId > 0)
                                            {
                                                startDate = lnkdTac.StartDate;  //#2097: Set Orgional Tactic start date to push old tactic date to SFDC.  
                                            }
                                        }
                                        lstPagedEntityList[index] = SyncTacticData(lstPagedEntityList[index], ref sbMessage);
                                        // Add By Nishant Sheth
                                        // Desc :: #2289 : Mismtach count in email while marketo tactics not pushing in salesforce.
                                        if (_IsSFDCWithMarketo)
                                        {
                                            if (lstPagedEntityList[index].IntegrationInstanceTacticId != null)
                                            {
                                        lstProcessTacIds.Add(tacticId);
                                            }
                                        }
                                        else
                                        {
                                            lstProcessTacIds.Add(tacticId);
                                        }
                                        startDate = null; //#2097: reset startDate global variable.
                                        #region "Old Code"
                                        // Save 10 log records to Table.
                                        //if (((index + 1) % logRecordSize) == 0)
                                        //{
                                        //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                                        //    sbMessage = new StringBuilder();
                                        //} 
                                        #endregion
                                    }
                                }
                            }
                            else if (EntityType.ImprovementTactic.ToString().Equals(entityType) && entityList is List<Plan_Improvement_Campaign_Program_Tactic>)
                            {
                                lstPagedEntityList = new List<Plan_Improvement_Campaign_Program_Tactic>();
                                lstPagedEntityList = entityList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList() as List<Plan_Improvement_Campaign_Program_Tactic>;
                                if (lstPagedEntityList != null && lstPagedEntityList.Count > 0)
                                {
                                    for (int index = 0; index < lstPagedEntityList.Count; index++)
                                    {
                                        lstPagedEntityList[index] = SyncImprovementData(lstPagedEntityList[index], ref sbMessage);
                                        lstProcessTacIds.Add(lstPagedEntityList[index].ImprovementPlanTacticId);
                                        #region "Old Code"
                                        // Save 10 log records to Table.
                                        //if (((index + 1) % logRecordSize) == 0)
                                        //{
                                        //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                                        //    sbMessage = new StringBuilder();
                                        //} 
                                        #endregion
                                    }
                                }
                            }
                            //if (!string.IsNullOrEmpty(sbMessage.ToString()))
                            //{
                            //    // Save remaining log records to Table.
                            //    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                            //}
                            db.SaveChanges();
                            page++;
                        }
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync" + entityType + "Data process end.");
                        //System.Diagnostics.Trace.WriteLine("Step 4 End:" + DateTime.Now.ToString());
                    }
                    catch (Exception ex)
                    {
                        _isResultError = true;
                        string exMessage = Common.GetInnermostException(ex);
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing " + entityType + " data to Salesforce: " + exMessage);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                _isResultError = true;
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing " + entityType + " data to Salesforce: " + exMessage);
            }
        }

        private List<Plan_Campaign_Program_Tactic> GetLastSyncUpdatedTactics(List<Plan_Campaign_Program_Tactic> tacticList, List<Plan_Campaign_Program_Tactic> lstCreatedTactic, List<Plan_Campaign_Program_Tactic> lstUpdatedTactic, List<Plan> lstPlan)
        {
            string strActualCostActualField = "CostActual"; // static variable value to identify that instance has mapped Actual cost field in Instance mapping.
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            bool isInstanceMappingUpdate = false;
            try
            {
                #region "Make list of updated tactic after last sync and sync to SFDC"

                #region "Create list of created/updated tactics after last sync"
                int minSyncTacCount = 10;
                List<Plan_Campaign_Program_Tactic> syncTactics = new List<Plan_Campaign_Program_Tactic>();
                DateTime? lastInstanceSyncDate = null;

                #region "Add created tactics to push tactic list"
                if (lstCreatedTactic != null && lstCreatedTactic.Count > 0)
                {
                    syncTactics.AddRange(lstCreatedTactic); // Add tactics those must be create into salesforce.
                }
                #endregion

                // if there is no updated tactic means all created tactic then no need to get latest sync start date.
                if (lstUpdatedTactic != null && lstUpdatedTactic.Count > 0 && lstUpdatedTactic.Count > minSyncTacCount)
                {
                    #region "Determine last successfully synced instance date"
                    if (_integrationInstanceId > 0)
                    {
                        string strPushTacticData = Enums.IntegrationInstanceSectionName.PushTacticData.ToString();
                        string strSyncSuccessStatus = Enums.SyncStatus.Success.ToString();
                        #region "Old Code"
                        //var lstInstanceSections = db.IntegrationInstanceSections.Where(inst => inst.IntegrationInstanceId == _integrationInstanceId && inst.SectionName == strPushTacticData && inst.Status == strSyncSuccessStatus).OrderByDescending(inst => inst.SyncStart).Select(inst => new { IntegrationInstanceSectionId = inst.IntegrationInstanceSectionId, SyncStart = inst.SyncStart, SyncEnd = inst.SyncEnd }).ToList();
                        //if (lstInstanceSections != null && lstInstanceSections.Count > 0)
                        //{
                        //    string strTacticType = EntityType.Tactic.ToString();
                        //    List<int> InstanceSectionIds = lstInstanceSections.Select(sec => sec.IntegrationInstanceSectionId).ToList();
                        //    var tblPlanEntityLog = db.IntegrationInstancePlanEntityLogs.Where(log => InstanceSectionIds.Contains(log.IntegrationInstanceSectionId) && log.EntityType == strTacticType).Select(log => new { IntegrationInstanceSectionId = log.IntegrationInstanceSectionId, EntityId = log.EntityId }).ToList();
                        //    //var lstInstSections = (from entLog in db.IntegrationInstancePlanEntityLogs
                        //    //                       join sec in lstInstanceSections on entLog.IntegrationInstanceSectionId equals sec.IntegrationInstanceSectionId

                        //    // Get most recent sync section based on IntegrationInstancePlanEntityLog table tactic pushed count.
                        //    foreach (var section in lstInstanceSections)
                        //    {
                        //        var lstEntity = tblPlanEntityLog.Where(log => log.IntegrationInstanceSectionId == section.IntegrationInstanceSectionId).Select(log => log.EntityId).ToList();
                        //        // Get Sync date of Section; if pushced tactic count more than 1 for this specific "PushTacticData" section.
                        //        if (lstEntity != null && lstEntity.Count > 1)
                        //        {
                        //            lastInstanceSyncDate = section.SyncEnd;       // Get most recent sync "PushTacticData" section syncStart Date.
                        //            break;
                        //        }
                        //    }
                        //} 
                        #endregion
                        #region "Get last sync date"
                        var lstInstanceSections = db.IntegrationInstanceSections.Where(inst => inst.IntegrationInstanceId == _integrationInstanceId && inst.SectionName == strPushTacticData).OrderByDescending(inst => inst.SyncStart).Select(inst => new { IntegrationInstanceSectionId = inst.IntegrationInstanceSectionId, SyncStart = inst.SyncStart, SyncEnd = inst.SyncEnd, Status = inst.Status }).ToList();
                        if (lstInstanceSections != null && lstInstanceSections.Count > 0)
                        {
                            string strTacticType = EntityType.Tactic.ToString();
                            List<int> InstanceSectionIds = lstInstanceSections.Select(sec => sec.IntegrationInstanceSectionId).ToList();
                            var tblPlanEntityLog = db.IntegrationInstancePlanEntityLogs.Where(log => InstanceSectionIds.Contains(log.IntegrationInstanceSectionId) && log.EntityType == strTacticType).Select(log => new { IntegrationInstanceSectionId = log.IntegrationInstanceSectionId, EntityId = log.EntityId, Status = log.Status }).ToList();

                            // Get most recent sync section based on IntegrationInstancePlanEntityLog table tactic pushed count.
                            foreach (var section in lstInstanceSections)
                            {
                                if (section.Status != null)
                                {
                                    if (section.Status.Equals(Enums.SyncStatus.Success.ToString()))
                                    {
                                        var lstEntity = tblPlanEntityLog.Where(log => log.IntegrationInstanceSectionId == section.IntegrationInstanceSectionId).Select(log => log.EntityId).ToList();
                                        // Get Sync date of Section; if pushced tactic count more than 1 for this specific "PushTacticData" section.
                                        if (lstEntity != null && lstEntity.Count > 1)
                                        {
                                            lastInstanceSyncDate = section.SyncEnd;       // Get most recent sync "PushTacticData" section syncStart Date.
                                            break;
                                        }
                                    }
                                    else if (section.Status.Equals(Enums.SyncStatus.Error.ToString()))
                                    {
                                        var lstEntity = tblPlanEntityLog.Where(log => log.IntegrationInstanceSectionId == section.IntegrationInstanceSectionId).Select(log => new { EntityId = log.EntityId, Status = log.Status }).ToList();
                                        // Get Sync date of Section; if pushced tactic count more than 1 for this specific "PushTacticData" section.
                                        if (lstEntity != null && lstEntity.Count > 1)
                                        {
                                            string strErrorStatus = Enums.SyncStatus.Error.ToString();
                                            List<int> lstEntityIds = lstEntity.Where(ent => ent.Status.Equals(strErrorStatus)).Select(ent => ent.EntityId).ToList();
                                            var lstFailedTactics = db.Plan_Campaign_Program_Tactic.Where(tac => lstEntityIds.Contains(tac.PlanTacticId) && statusList.Contains(tac.Status) && tac.IsDeployedToIntegration && !tac.IsDeleted && tac.IsSyncSalesForce.HasValue && tac.IsSyncSalesForce.Value == true).ToList();
                                            if (lstFailedTactics != null && lstFailedTactics.Count > 0)
                                            {
                                                // Add modified tactics to syncTactics list.
                                                syncTactics.AddRange(lstFailedTactics);

                                                // remove modified tactics list from updated tactic list and assign to same variable.
                                                lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !lstFailedTactics.Contains(updTac)).ToList();
                                            }
                                            lastInstanceSyncDate = section.SyncEnd;       // Get most recent sync "PushTacticData" section syncStart Date.
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion

                #region "Identify Instance mapping updated after last sync date"
                if (lastInstanceSyncDate.HasValue)
                {
                    var lstPushInstnceMapping = db.IntegrationInstanceDataTypeMappings.Where(inst => inst.IntegrationInstanceId == _integrationInstanceId && inst.CreatedDate >= lastInstanceSyncDate.Value).ToList();
                    if (lstPushInstnceMapping != null && lstPushInstnceMapping.Count > 0)
                        isInstanceMappingUpdate = true;
                }
                #endregion

                #region "Filter 'lstUpdatedTactic' list based on  lastInstanceSyncDate"
                // if Instance Mapping updated after last sync date then push all the tactics rather than updated.
                if (lastInstanceSyncDate.HasValue && !isInstanceMappingUpdate)
                {
                    #region "Declared local variables"
                    Dictionary<string, string> lstMappingTacticFields = new Dictionary<string, string>();
                    string strPlanName = Enums.SFDCGlobalFields.PlanName.ToString();
                    lstMappingTacticFields = _mappingTactic;
                    #endregion

                    #region "Plan Modified Date: PlanName field configured in Salesforce Instance then identify then recently updated all plans and push respective plan tactics to 'syncTactics' list"
                    if (_mappingTactic.ContainsKey(strPlanName))    // if PlanName field configured in Salesforce Instance then identify then recently updated all plans.
                    {
                        #region "Get All Plan Modified tactics after last Sync Date"
                        if (lstPlan != null)
                        {
                            var lstUpdatePlan = lstPlan.Where(pln => pln.ModifiedDate >= lastInstanceSyncDate.Value).ToList();
                            // Identify the PlanName updated after last instance sync date and Add all respective plan tactics to "syncTactics" list.
                            if (lstUpdatePlan != null && lstUpdatePlan.Count > 0)
                            {
                                var updtPlanIds = lstUpdatePlan.Select(pln => pln.PlanId).ToList();
                                var lstPlanUpdateTactics = lstUpdatedTactic.Where(tac => updtPlanIds.Contains(tac.Plan_Campaign_Program.Plan_Campaign.PlanId));
                                if (lstPlanUpdateTactics != null && lstPlanUpdateTactics.Count() > 0)
                                {
                                    // Add modified tactics to "syncTactics" list.
                                    syncTactics.AddRange(lstPlanUpdateTactics);

                                    // remove modified tactics list from updated tactic list and assign to same variable.
                                    lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !lstPlanUpdateTactics.Contains(updTac)).ToList();
                                }
                            }
                        }
                        #endregion

                        #region "Linked Tactic Plan Name Modified: Get list of Tactic"
                        if (lstUpdatedTactic != null && lstUpdatedTactic.Count > 0)
                        {
                            var linkedTactic = lstUpdatedTactic.Where(tac => (tac.LinkedTacticId.HasValue) && (tac.LinkedTacticId.Value > 0)).Select(tac => tac).ToList();
                            if (linkedTactic != null && linkedTactic.Count > 0)
                            {
                                List<int> linkedPlanIds = new List<int>();
                                linkedPlanIds = linkedTactic.Where(tac => (tac.LinkedPlanId.HasValue) && (tac.LinkedPlanId.Value > 0)).Select(tac => tac.LinkedPlanId.Value).ToList();
                                var linkedUpdtPlanIds = db.Plans.Where(pln => (linkedPlanIds.Contains(pln.PlanId)) && (pln.ModifiedDate >= lastInstanceSyncDate)).Select(pln => pln.PlanId).ToList();
                                if (linkedUpdtPlanIds != null && linkedUpdtPlanIds.Count > 0)
                                {
                                    linkedTactic = linkedTactic.Where(tac => tac.LinkedPlanId.HasValue && linkedUpdtPlanIds.Contains(tac.LinkedPlanId.Value)).ToList();

                                    // Add modified tactics to syncTactics list.
                                    syncTactics.AddRange(linkedTactic);

                                    // remove modified tactics list from updated tactic list and assign to same variable.
                                    lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !linkedTactic.Contains(updTac)).ToList();
                                }
                            }
                        }

                        #endregion
                    }
                    #endregion

                    #region "Tactic Modified date: Get list of tactics updated after last Instance Sync date"
                    if (lstUpdatedTactic != null && lstUpdatedTactic.Count > 0)
                    {
                        var lstModifdTactics = lstUpdatedTactic.Where(tac => tac.ModifiedDate >= lastInstanceSyncDate.Value).ToList();
                        if (lstModifdTactics != null && lstModifdTactics.Count > 0)
                        {
                            // Add modified tactics to syncTactics list.
                            syncTactics.AddRange(lstModifdTactics);

                            // remove modified tactics list from updated tactic list and assign to same variable.
                            lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !lstModifdTactics.Contains(updTac)).ToList();
                        }
                    }
                    #endregion

                    #region "Budgeted(Planned) Cost: Get all tactics updated planned cost after last Instance Sync Date"
                    string strBudgetedCostActualField = "Cost";
                    if (lstUpdatedTactic != null && lstUpdatedTactic.Count > 0 && _mappingTactic.ContainsKey(strBudgetedCostActualField))
                    {
                        var lstBdgtCostUpdateTactics = (from updTac in lstUpdatedTactic
                                                        join bcost in db.Plan_Campaign_Program_Tactic_Cost on updTac.PlanTacticId equals bcost.PlanTacticId
                                                        where bcost.CreatedDate >= lastInstanceSyncDate
                                                        select updTac).ToList();
                        if (lstBdgtCostUpdateTactics != null && lstBdgtCostUpdateTactics.Count > 0)
                        {
                            // Add modified tactics to syncTactics list.
                            syncTactics.AddRange(lstBdgtCostUpdateTactics);

                            // remove modified tactics list from updated tactic list and assign to same variable.
                            lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !lstBdgtCostUpdateTactics.Contains(updTac)).ToList();
                        }

                    }
                    #endregion

                    #region "Actual Cost: Get all tactics updated actual cost after last Instance Sync Date"

                    if (lstUpdatedTactic != null && lstUpdatedTactic.Count > 0 && _mappingTactic.ContainsKey(strActualCostActualField))
                    {
                        #region "Get updated tactic for those child line items cost updated"
                        var lstlineActualCostUpdateTactics = (from updTac in lstUpdatedTactic
                                                              join line in db.Plan_Campaign_Program_Tactic_LineItem on updTac.PlanTacticId equals line.PlanTacticId
                                                              join lineCost in db.Plan_Campaign_Program_Tactic_LineItem_Actual on line.PlanLineItemId equals lineCost.PlanLineItemId
                                                              where line.IsDeleted == true && lineCost.CreatedDate >= lastInstanceSyncDate
                                                              select updTac).ToList();
                        if (lstlineActualCostUpdateTactics != null && lstlineActualCostUpdateTactics.Count > 0)
                        {
                            // Add modified tactics to syncTactics list.
                            syncTactics.AddRange(lstlineActualCostUpdateTactics);

                            // remove modified tactics list from updated tactic list and assign to same variable.
                            lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !lstlineActualCostUpdateTactics.Contains(updTac)).ToList();
                        }
                        #endregion

                        #region "Get updated tactic for those modified it's cost"
                        if (lstUpdatedTactic != null && lstUpdatedTactic.Count > 0)
                        {
                            string costStageTitle = "Cost";
                            var lstActualCostUpdateTactics = (from updTac in lstUpdatedTactic
                                                              join tacActual in db.Plan_Campaign_Program_Tactic_Actual on updTac.PlanTacticId equals tacActual.PlanTacticId
                                                              where tacActual.StageTitle == costStageTitle && tacActual.CreatedDate >= lastInstanceSyncDate
                                                              select updTac).ToList();
                            if (lstActualCostUpdateTactics != null && lstActualCostUpdateTactics.Count > 0)
                            {
                                // Add modified tactics to syncTactics list.
                                syncTactics.AddRange(lstActualCostUpdateTactics);

                                // remove modified tactics list from updated tactic list and assign to same variable.
                                lstUpdatedTactic = lstUpdatedTactic.Where(updTac => !lstActualCostUpdateTactics.Contains(updTac)).ToList();
                            }
                        }
                        #endregion

                    }
                    #endregion
                }

                #endregion

                #endregion

                // update "tacticlist" by filtered Created & Updated tactics, if system get lastInstanceSyncDate else remains as it is.
                if (lastInstanceSyncDate.HasValue && !isInstanceMappingUpdate)
                {
                    tacticList = syncTactics;
                    tacticList = tacticList.Distinct().ToList();
                }
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                _isResultError = true;
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "System error occurred while syncing data with Salesforce.", Enums.SyncStatus.Error, DateTime.Now));
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting last updated tactics: " + exMessage);

            }
            return tacticList;
        }

        private string CreateCampaign(Plan_Campaign planCampaign)
        {
            string campaignId = string.Empty;
            try
            {
                Dictionary<string, object> campaign = GetCampaign(planCampaign);

                //Added by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                if (_mappingCampaign.ContainsKey("ActivityType"))
                {
                    string activityType = Enums.EntityType.Campaign.ToString();
                    string ActivityTypeMappedValue = _mappingCampaign["ActivityType"].ToString();
                    campaign.Add(ActivityTypeMappedValue, activityType);
                }
                //End by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC

                campaignId = _client.Create(objectName, campaign);
            }
            catch (Exception)
            {
                throw;
            }
            return campaignId;
        }

        private string CreateProgram(Plan_Campaign_Program planProgram)
        {
            Dictionary<string, object> program = new Dictionary<string, object>();
            string programId = string.Empty;
            try
            {
                program = GetProgram(planProgram, Enums.Mode.Create);

                //Added by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                if (_mappingProgram.ContainsKey("ActivityType"))
                {
                    string activityType = Enums.EntityType.Program.ToString();
                    program.Add(_mappingProgram["ActivityType"].ToString(), activityType);
                }
                //End by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                programId = _client.Create(objectName, program);
            }
            catch (Exception)
            {
                throw;
            }
            return programId;
        }

        private string CreateTactic(Plan_Campaign_Program_Tactic planTactic)
        {
            string tacticId = string.Empty;
            Dictionary<string, object> tactic = GetTactic(planTactic, Enums.Mode.Create);
            if (_mappingTactic.ContainsKey("Title") && planTactic != null && _CustomNamingPermissionForInstance && IsClientAllowedForCustomNaming)
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                if (tactic.ContainsKey(titleMappedValue))
                {
                    tactic[titleMappedValue] = (planTactic.TacticCustomName == null) ? (Common.GenerateCustomName(planTactic, SequencialOrderedTableList, _mappingCustomFields)) : (planTactic.TacticCustomName);
                    planTactic.TacticCustomName = tactic[titleMappedValue].ToString();
                    int valuelength = lstSalesforceFieldDetail.Where(sfdetail => sfdetail.TargetField == titleMappedValue).FirstOrDefault().Length;
                    string customvalue = planTactic.TacticCustomName;
                    if (valuelength != 0)
                    {
                        customvalue = (customvalue.Length > valuelength) ? (customvalue.Substring(0, valuelength - 1)) : customvalue;
                    }
                    tactic[titleMappedValue] = customvalue;
                }
            }
            //Added by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
            if (_mappingTactic.ContainsKey("ActivityType"))
            {
                string activityType = Enums.EntityType.Tactic.ToString();
                tactic.Add(_mappingTactic["ActivityType"].ToString(), activityType);
            }
            //End by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
            tacticId = _client.Create(objectName, tactic);
            return tacticId;
        }

        private string CreateImprovementCampaign(Plan_Improvement_Campaign planIMPCampaign)
        {
            try
            {
                Dictionary<string, object> campaign = GetImprovementCampaign(planIMPCampaign);

                //Added by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                if (_mappingTactic.ContainsKey("ActivityType"))
                {
                    string activityType = Enums.EntityType.ImprovementCampaign.ToString();
                    campaign.Add(_mappingTactic["ActivityType"].ToString(), activityType);
                }
                //End by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                string campaignId = _client.Create(objectName, campaign);
                return campaignId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string CreateImprovementProgram(Plan_Improvement_Campaign_Program planIMPProgram)
        {
            try
            {
                Dictionary<string, object> program = GetImprovementProgram(planIMPProgram, Enums.Mode.Create);

                //Added by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                if (_mappingTactic.ContainsKey("ActivityType"))
                {
                    string activityType = Enums.EntityType.ImprovementProgram.ToString();
                    program.Add(_mappingTactic["ActivityType"].ToString(), activityType);
                }
                //End by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                string programId = _client.Create(objectName, program);
                return programId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string CreateImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            try
            {
                Dictionary<string, object> tactic = GetImprovementTactic(planIMPTactic, Enums.Mode.Create);

                //Added by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                if (_mappingTactic.ContainsKey("ActivityType"))
                {
                    string activityType = Enums.EntityType.ImprovementTactic.ToString();
                    tactic.Add(_mappingTactic["ActivityType"].ToString(), activityType);
                }
                //End by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                string tacticId = _client.Create(objectName, tactic);
                return tacticId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool UpdateCampaign(Plan_Campaign planCampaign)
        {
            try
            {
                Dictionary<string, object> campaign = GetCampaign(planCampaign);

                //Added by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                if (_mappingCampaign.ContainsKey("ActivityType"))
                {
                    string activityType = Enums.EntityType.Campaign.ToString();
                    string ActivityTypeMappedValue = _mappingCampaign["ActivityType"].ToString();
                    campaign.Add(ActivityTypeMappedValue, activityType);
                }
                //End by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC

                return _client.Update(objectName, planCampaign.IntegrationInstanceCampaignId, campaign);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool UpdateProgram(Plan_Campaign_Program planProgram)
        {
            try
            {
                Dictionary<string, object> program = GetProgram(planProgram, Enums.Mode.Update);

                //Added by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                if (_mappingProgram.ContainsKey("ActivityType"))
                {
                    string activityType = Enums.EntityType.Program.ToString();
                    program.Add(_mappingProgram["ActivityType"].ToString(), activityType);
                }
                //End by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC

                return _client.Update(objectName, planProgram.IntegrationInstanceProgramId, program);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool UpdateTactic(Plan_Campaign_Program_Tactic planTactic)
        {
            try
            {
                Dictionary<string, object> tactic = GetTactic(planTactic, Enums.Mode.Update);
                if (!string.IsNullOrEmpty(planTactic.TacticCustomName) && _mappingTactic.ContainsKey("Title"))
                {
                    string titleMappedValue = _mappingTactic["Title"].ToString();

                    if (tactic.ContainsKey(titleMappedValue))
                    {
                        int valuelength = lstSalesforceFieldDetail.Where(sfdetail => sfdetail.TargetField == titleMappedValue).FirstOrDefault().Length;
                        string customvalue = planTactic.TacticCustomName;
                        if (valuelength != 0)
                        {
                            customvalue = customvalue.Length > valuelength ? customvalue.Substring(0, valuelength - 1) : customvalue;
                        }
                        tactic[titleMappedValue] = customvalue;
                    }
                }
                //Added by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                if (_mappingTactic.ContainsKey("ActivityType"))
                {
                    string activityType = Enums.EntityType.Tactic.ToString();
                    tactic.Add(_mappingTactic["ActivityType"].ToString(), activityType);
                }
                //End by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                return _client.Update(objectName, planTactic.IntegrationInstanceTacticId, tactic);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool UpdateImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            try
            {
                Dictionary<string, object> tactic = GetImprovementTactic(planIMPTactic, Enums.Mode.Update);

                //Added by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC
                if (_mappingTactic.ContainsKey("ActivityType"))
                {
                    string activityType = Enums.EntityType.ImprovementTactic.ToString();
                    tactic.Add(_mappingTactic["ActivityType"].ToString(), activityType);
                }
                //End by Mitesh Vaishnav for PL ticket 1335 - Integration - Gameplan type field for SFDC

                return _client.Update(objectName, planIMPTactic.IntegrationInstanceTacticId, tactic);
            }
            catch (Exception)
            {
                throw;
            }
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
            string PlanName = string.Empty;
            Dictionary<string, object> campaign = new Dictionary<string, object>();
            try
            {
                campaign = GetTargetKeyValue<Plan_Campaign>(planCampaign, _mappingCampaign);
            }
            catch (Exception)
            {
                throw;
            }
            return campaign;
        }

        private Dictionary<string, object> GetProgram(Plan_Campaign_Program planProgram, Enums.Mode mode)
        {
            string PlanName = string.Empty;
            Dictionary<string, object> program = new Dictionary<string, object>();
            try
            {
                program = GetTargetKeyValue<Plan_Campaign_Program>(planProgram, _mappingProgram);

                if (mode.Equals(Enums.Mode.Create))
                {
                    program.Add(ColumnParentId, _parentId);
                }
                // Add By Nishant Sheth
                //Make Hireachy for Marketo sync on SFDC
                if (_IsSFDCWithMarketo && !mode.Equals(Enums.Mode.Create))
                {
                    program.Add(ColumnParentId, _parentId);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return program;
        }

        private Dictionary<string, object> GetTactic(Plan_Campaign_Program_Tactic planTactic, Enums.Mode mode)
        {
            Dictionary<string, object> tactic = new Dictionary<string, object>();
            try
            {
                string PlanName = string.Empty;
                tactic = GetTargetKeyValue<Plan_Campaign_Program_Tactic>(planTactic, _mappingTactic);
                if (mode.Equals(Enums.Mode.Create))
                {
                    tactic.Add(ColumnParentId, _parentId);
                }
                // Add By Nishant Sheth
                //Make Hireachy for Marketo sync on SFDC
                if (_IsSFDCWithMarketo && !mode.Equals(Enums.Mode.Create))
                {
                    tactic.Add(ColumnParentId, _parentId);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return tactic;
        }

        private Dictionary<string, object> GetImprovementCampaign(Plan_Improvement_Campaign planIMPCampaign)
        {
            string PlanName = string.Empty;
            Dictionary<string, object> campaign = new Dictionary<string, object>();
            try
            {
                campaign = GetTargetKeyValue<Plan_Improvement_Campaign>(planIMPCampaign, _mappingImprovementCampaign);
            }
            catch (Exception)
            {
                throw;
            }
            return campaign;
        }

        private Dictionary<string, object> GetImprovementProgram(Plan_Improvement_Campaign_Program planIMPProgram, Enums.Mode mode)
        {
            string PlanName = string.Empty;
            Dictionary<string, object> program = new Dictionary<string, object>();
            try
            {
                program = GetTargetKeyValue<Plan_Improvement_Campaign_Program>(planIMPProgram, _mappingImprovementProgram);

                if (mode.Equals(Enums.Mode.Create))
                {
                    program.Add(ColumnParentId, _parentId);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return program;
        }

        private Dictionary<string, object> GetImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic, Enums.Mode mode)
        {
            Dictionary<string, object> tactic = new Dictionary<string, object>();
            try
            {
                tactic = GetTargetKeyValue<Plan_Improvement_Campaign_Program_Tactic>(planIMPTactic, _mappingImprovementTactic);
                if (mode.Equals(Enums.Mode.Create))
                {
                    tactic.Add(ColumnParentId, _parentId);
                }
            }
            catch (Exception)
            {
                throw;
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
            string tacticTitle = "Title";   //// Added by Viral kadiya on 02/17/2016 for PL ticket #1916
            Dictionary<string, object> keyvaluepair = new Dictionary<string, object>();

            try
            {
                Type sourceType = ((T)obj).GetType();
                PropertyInfo[] sourceProps = sourceType.GetProperties();
                foreach (KeyValuePair<string, string> mapping in mappingDataType)
                {
                    string value = string.Empty;
                    PropertyInfo propInfo = sourceProps.FirstOrDefault(property => property.Name.Equals(mapping.Key));
                    if (propInfo != null)
                    {
                        value = Convert.ToString(propInfo.GetValue(((T)obj), null));
                        if (mapping.Key == status)
                        {
                            value = GetSalesForceStatus(value);
                        }
                        else if (mapping.Key == createdBy)
                        {
                            value = _mappingUser[Guid.Parse(value)];
                            int valuelength = lstSalesforceFieldDetail.Where(sfdetail => sfdetail.TargetField == mapping.Value).FirstOrDefault().Length;
                            value = value.Length > valuelength ? value.Substring(0, valuelength - 1) : value;
                        }
                        else if (mapping.Key == statDate || mapping.Key == endDate || mapping.Key == effectiveDate)
                        {
                            // #2097: In case of push Linked Tactic,get startdate value from global varialbe(i.e. "startDate")
                            if (mapping.Key == statDate && startDate.HasValue && obj is Plan_Campaign_Program_Tactic)
                                value = Convert.ToDateTime(startDate).ToString("yyyy-MM-ddThh:mm:ss+hh:mm");
                            else
                                value = Convert.ToDateTime(value).ToString("yyyy-MM-ddThh:mm:ss+hh:mm");  // If not linked tactic then pick start date from respecitve entity(i.e. Campaign,Program,Tactic)
                        }
                        //// Start - Added by Sohel Pathan on 29/01/2015 for PL ticket #1113
                        else if (mapping.Key == tacticType)
                        {
                            value = Convert.ToString(((Plan_Campaign_Program_Tactic)obj).TacticType.Title);
                            int valuelength = lstSalesforceFieldDetail.Where(sfdetail => sfdetail.TargetField == mapping.Value).FirstOrDefault().Length;
                            value = value.Length > valuelength ? value.Substring(0, valuelength - 1) : value;
                        }
                        else
                        {
                            int valuelength = lstSalesforceFieldDetail.Where(sfdetail => sfdetail.TargetField == mapping.Value).FirstOrDefault().Length;
                            if (valuelength != 0)
                            {
                                value = value.Length > valuelength ? value.Substring(0, valuelength - 1) : value;
                            }
                        }
                        //// End - Added by Sohel Pathan on 29/01/2015 for PL ticket #1113

                        //// Start - Added by Viral Kadiya on 02/16/2016 for PL ticket #1916
                        if (mapping.Key == tacticTitle)
                        {
                            value = System.Web.HttpUtility.HtmlDecode(value);
                        }
                        //// End - Added by Viral Kadiya on 02/16/2016 for PL ticket #1916
                        keyvaluepair.Add(mapping.Value, value);
                    }
                    // Start - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                    else
                    {
                        if (mapping.Key == costActual)
                        {
                            value = GetActualCostbyPlanTacticId(((Plan_Campaign_Program_Tactic)obj).PlanTacticId);
                            keyvaluepair.Add(mapping.Value, value);
                        }
                        else if (mapping.Key == Enums.SFDCGlobalFields.PlanName.ToString())
                        {
                            value = !string.IsNullOrEmpty(PlanName) ? PlanName : string.Empty;
                            int valuelength = lstSalesforceFieldDetail.Where(sfdetail => sfdetail.TargetField == mapping.Value).FirstOrDefault().Length;
                            value = value.Length > valuelength ? value.Substring(0, valuelength - 1) : value;
                            keyvaluepair.Add(mapping.Value, value);
                        }
                        else
                        {
                            int customid;
                            if (Int32.TryParse(mapping.Key, out customid))
                            {
                                int valuelength = 0;
                                value = MapCustomField<T>(obj, sourceProps, mapping.Key);
                                if (value != string.Empty)
                                {
                                    if (value.Length > 0)
                                    {
                                        valuelength = lstSalesforceFieldDetail.Where(sfdetail => sfdetail.TargetField == mapping.Value).FirstOrDefault().Length;
                                        if (valuelength != 0)
                                        {
                                            value = value.Length > valuelength ? value.Substring(0, valuelength - 1) : value;
                                        }
                                        keyvaluepair.Add(mapping.Value, value);
                                    }
                                }
                            }
                        }
                    }
                    // End - Added by Sohel Pathan on 03/12/2014 for PL ticket #995, 996, & 997
                }
                //Added by Mitesh Vaishnav for PL ticket 1473
                //All campaigns have "IsActive" flag true by default
                if (keyvaluepair.Count > 0)
                {
                    keyvaluepair.Add("IsActive", true);
                }
            }
            catch (Exception)
            {

                throw;
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
        private string MapCustomField<T>(object obj, PropertyInfo[] sourceProps, string Key)
        {
            if (_mappingCustomFields != null)
            {
                if (_mappingCustomFields.Count > 0)
                {
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
                    var mapobj = _mappingCustomFields.Where(map => map.EntityId == Convert.ToInt32(EntityTypeId) && map.CustomFieldId == Convert.ToInt32(Key)).FirstOrDefault();
                    if (mapobj != null)
                    {
                        return mapobj.Value;
                    }
                    //if (_mappingCustomFields.ContainsKey(mappingKey))
                    //{
                    //    return new string[] { mapping.Value, _mappingCustomFields[mappingKey] };
                    //}
                }
            }
            return string.Empty;
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
        private List<CustomFiledMapping> CreateMappingCustomFieldDictionary(List<int> EntityIdList, string EntityType)
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            List<CustomFiledMapping> CustomFieldsList = new List<CustomFiledMapping>();
            try
            {
                if (EntityIdList.Count > 0)
                {
                    string idList = string.Join(",", EntityIdList);

                    String Query = "select distinct '" + EntityType.Substring(0, 1) + "-' + cast(EntityId as nvarchar) + '-' + cast(Extent1.CustomFieldID as nvarchar) as keyv, " +
                        "cast([Extent1].[CustomFieldId] as nvarchar) as CustomFieldId," +
                        "cast(EntityId as nvarchar) as EntityId," +
                        "case  " +
                           "    when A.keyi is not null then Extent2.AbbreviationForMulti " +
                           "    when Extent3.[Name]='TextBox' then Extent1.Value " +
                           "    when Extent3.[Name]='DropDownList' then Extent4.Value " +
                        "End as ValueV , " +
                            "case  " +
                               "    when A.keyi is not null then Extent2.AbbreviationForMulti" +
                               "   when Extent3.[Name]='TextBox' then Extent1.Value " +
                               "    when Extent3.[Name]='DropDownList' then CASE WHEN Extent4.Abbreviation IS nOT NULL THEN Extent4.Abbreviation ELSE Extent4.Value END" +
                                "   END as CustomName" +

                        " from CustomField_Entity Extent1 " +
                            "INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] AND [Extent2].[IsDeleted] = 0 " +
                        "INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId " +
                        "Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and cast(Extent1.Value as nvarchar)=cast(Extent4.CustomFieldOptionID as nvarchar)" +
                        "Left Outer join ( " +
                        "select '" + EntityType.Substring(0, 1) + "-' + cast(EntityId as nvarchar) + '-' + cast(Extent1.CustomFieldID as nvarchar) as keyi " +

                        " from CustomField_Entity Extent1 " +
                        "INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] " +
                        "INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId " +
                        "Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and Extent1.Value=Extent4.CustomFieldOptionID " +
                        "WHERE ([Extent1].[EntityId] IN (" + idList + ")) " +
                        "group by '" + EntityType.Substring(0, 1) + "-' + cast(EntityId as nvarchar) + '-' + cast(Extent1.CustomFieldID as nvarchar) " +
                        "having count(*) > 1 " +
                        ") A on A.keyi='" + EntityType.Substring(0, 1) + "-' + cast(EntityId as nvarchar) + '-' + cast(Extent1.CustomFieldID as nvarchar) " +
                        "WHERE ([Extent1].[EntityId] IN (" + idList + ")) " +
                        "order by keyv";

                    MRPEntities mp = new MRPEntities();
                    DbConnection conn = mp.Database.Connection;
                    conn.Open();
                    DbCommand comm = conn.CreateCommand();
                    comm.CommandText = Query;
                    DbDataReader ddr = comm.ExecuteReader();


                    while (ddr.Read())
                    {
                        CustomFieldsList.Add(new CustomFiledMapping { CustomFieldId = !ddr.IsDBNull(1) ? Convert.ToInt32(ddr.GetString(1)) : 0, EntityId = !ddr.IsDBNull(2) ? Convert.ToInt32(ddr.GetString(2)) : 0, Value = !ddr.IsDBNull(3) ? Convert.ToString(ddr.GetString(3)) : string.Empty, CustomNameValue = !ddr.IsDBNull(4) ? Convert.ToString(ddr.GetString(4)) : string.Empty });

                    }
                    conn.Close();
                    mp.Dispose();
                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get Mapping detail for " + EntityType + " custom field(s), Found " + CustomFieldsList.Count().ToString() + " custome field mapping");
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while mapping CustomFieldDictionary with it's value:- " + exMessage);
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

        public void UpdateLinkedTacticComment(List<int> lstProcessTacIds, List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstCreatedTacIds, List<TacticLinkedTacMapping> lstTac_LinkTacMapping)
        {
            string query = string.Empty;
            try
            {
                #region "Update linkedTactic Comment & IntegrationInstanceTacticId"
                // if any tactic processed(push).
                if (lstProcessTacIds != null && lstProcessTacIds.Count > 0)
                {
                    #region "Declare local varialbles"
                    // Get created linked TacticIds.
                    List<int> lstCreateLinkedTacIds = new List<int>();
                    List<int> lstUpdateTacIds = new List<int>();
                    List<int> lstUpdateLinkedTacIds = new List<int>();
                    string strCreatedLinkedTacIds = string.Empty, strUpdatedLinkedTacIds = string.Empty, strCreatedTacIds = string.Empty, strUpdatedTacIds = string.Empty;
                    #endregion

                    // if linked tactic exist.
                    if (lstTac_LinkTacMapping != null && lstTac_LinkTacMapping.Count > 0)
                    {
                        if (lstCreatedTacIds != null && lstCreatedTacIds.Count > 0)
                        {
                            // get only tactics created to SFDC.
                            lstCreatedTacIds = lstCreatedTacIds.Where(crtId => lstProcessTacIds.Contains(crtId)).ToList();
                            lstCreateLinkedTacIds = lstTac_LinkTacMapping.Where(tac => lstCreatedTacIds.Contains(tac.PlanTactic.PlanTacticId)).Select(tac => tac.LinkedTactic.PlanTacticId).ToList();

                            // get only tactics update to SFDC.
                            lstUpdateTacIds = tacticList.Where(tac => !lstCreatedTacIds.Contains(tac.PlanTacticId) && lstProcessTacIds.Contains(tac.PlanTacticId)).Select(tac => tac.PlanTacticId).ToList();
                        }
                        else
                        {
                            // get only tactics update to SFDC.
                            lstUpdateTacIds = tacticList.Where(tac => lstProcessTacIds.Contains(tac.PlanTacticId)).Select(tac => tac.PlanTacticId).ToList();
                        }

                        // Get Updated linked TacIds.

                        if (lstUpdateTacIds != null && lstUpdateTacIds.Count > 0)
                        {
                            lstUpdateLinkedTacIds = lstTac_LinkTacMapping.Where(tac => lstUpdateTacIds.Contains(tac.PlanTactic.PlanTacticId)).Select(tac => tac.LinkedTactic.PlanTacticId).ToList();
                        }
                    }
                    else
                    {
                        // Get list of only tactics but not linked tactics.
                        if (lstCreatedTacIds != null && lstCreatedTacIds.Count > 0)
                        {
                            // get only tactics created to SFDC.
                            lstCreatedTacIds = lstCreatedTacIds.Where(crtId => lstProcessTacIds.Contains(crtId)).ToList();
                            // get only tactics update to SFDC.
                            lstUpdateTacIds = tacticList.Where(tac => !lstCreatedTacIds.Contains(tac.PlanTacticId) && lstProcessTacIds.Contains(tac.PlanTacticId)).Select(tac => tac.PlanTacticId).ToList();
                        }
                        else
                        {
                            // get only tactics update to SFDC.
                            lstUpdateTacIds = tacticList.Where(tac => lstProcessTacIds.Contains(tac.PlanTacticId)).Select(tac => tac.PlanTacticId).ToList();
                        }
                    }

                    #region "Linked Tactic: Create comma separated created & updated linked tactic"
                    if (lstCreateLinkedTacIds != null && lstCreateLinkedTacIds.Count > 0)
                        strCreatedLinkedTacIds = string.Join(",", lstCreateLinkedTacIds);
                    if (lstUpdateLinkedTacIds != null && lstUpdateLinkedTacIds.Count > 0)
                        strUpdatedLinkedTacIds = string.Join(",", lstUpdateLinkedTacIds);
                    #endregion

                    #region "Plan Tactic: Create comma separated created & updated Plan Tactic Ids"
                    if (lstCreatedTacIds != null && lstCreatedTacIds.Count > 0)
                        strCreatedTacIds = string.Join(",", lstCreatedTacIds);
                    if (lstUpdateTacIds != null && lstUpdateTacIds.Count > 0)
                        strUpdatedTacIds = string.Join(",", lstUpdateTacIds);
                    #endregion



                    MRPEntities mp = new MRPEntities();
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = mp.Database.Connection.ConnectionString;
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UpdateTacticInstanceTacticId_Comment", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@strCreatedTacIds", strCreatedTacIds);
                    cmd.Parameters.AddWithValue("@strUpdatedTacIds", strUpdatedTacIds);
                    cmd.Parameters.AddWithValue("@strUpdateComment", Common.TacticUpdatedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString());
                    cmd.Parameters.AddWithValue("@strCreateComment", Common.TacticSyncedComment + Integration.Helper.Enums.IntegrationType.Salesforce.ToString());
                    cmd.Parameters.AddWithValue("@isAutoSync", Common.IsAutoSync);
                    cmd.Parameters.AddWithValue("@userId", _userId);
                    cmd.Parameters.AddWithValue("@integrationType", Enums.IntegrationType.Salesforce.ToString()); //Added by Rahul Shah for PL #2194
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    mp.Dispose();
                }

                #endregion

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateLinkedTacticCommentForAPI(List<int> lstProcessTacIds, List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstCreatedTacIds, Dictionary<int, int> lstTac_LinkTacMapping, string strCrtCampaignIds, string strUpdCampaignIds, string strCrtProgramIds, string strUpdProgramIds, string strCrtImprvTacIds, string strUpdImprvTacIds)
        {
            string query = string.Empty;
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {
                string strCreatedLinkedTacIds = string.Empty, strUpdatedLinkedTacIds = string.Empty, strCreatedTacIds = string.Empty, strUpdatedTacIds = string.Empty;
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "UpdateLinkedTacticCommentForAPI method start.");
                #region "Update linkedTactic Comment & IntegrationInstanceTacticId"
                // if any tactic processed(push).
                if (lstProcessTacIds != null && lstProcessTacIds.Count > 0)
                {
                    #region "Declare local varialbles"
                    // Get created linked TacticIds.
                    List<int> lstCreateLinkedTacIds = new List<int>();
                    List<int> lstUpdateTacIds = new List<int>();
                    List<int> lstUpdateLinkedTacIds = new List<int>();
                    #endregion

                    // if linked tactic exist.
                    if (lstTac_LinkTacMapping != null && lstTac_LinkTacMapping.Count > 0)
                    {
                        if (lstCreatedTacIds != null && lstCreatedTacIds.Count > 0)
                        {
                            // get only tactics created to Marketo.
                            lstCreatedTacIds = lstCreatedTacIds.Where(crtId => lstProcessTacIds.Contains(crtId)).ToList();
                            lstCreateLinkedTacIds = lstTac_LinkTacMapping.Where(tac => lstCreatedTacIds.Contains(tac.Key)).Select(tac => tac.Value).ToList();

                            // get only tactics update to Marketo.
                            lstUpdateTacIds = tacticList.Where(tac => !lstCreatedTacIds.Contains(tac.PlanTacticId) && lstProcessTacIds.Contains(tac.PlanTacticId)).Select(tac => tac.PlanTacticId).ToList();
                        }
                        else
                        {
                            // get only tactics update to Marketo.
                            lstUpdateTacIds = tacticList.Where(tac => lstProcessTacIds.Contains(tac.PlanTacticId)).Select(tac => tac.PlanTacticId).ToList();
                        }

                        // Get Updated linked TacIds.

                        if (lstUpdateTacIds != null && lstUpdateTacIds.Count > 0)
                        {
                            lstUpdateLinkedTacIds = lstTac_LinkTacMapping.Where(tac => lstUpdateTacIds.Contains(tac.Key)).Select(tac => tac.Value).ToList();
                        }
                    }
                    else
                    {
                        // Get list of only tactics but not linked tactics.
                        if (lstCreatedTacIds != null && lstCreatedTacIds.Count > 0)
                        {
                            // get only tactics created to Marketo.
                            lstCreatedTacIds = lstCreatedTacIds.Where(crtId => lstProcessTacIds.Contains(crtId)).ToList();
                            // get only tactics update to Marketo.
                            lstUpdateTacIds = tacticList.Where(tac => !lstCreatedTacIds.Contains(tac.PlanTacticId) && lstProcessTacIds.Contains(tac.PlanTacticId)).Select(tac => tac.PlanTacticId).ToList();
                        }
                        else
                        {
                            // get only tactics update to Marketo.
                            lstUpdateTacIds = tacticList.Where(tac => lstProcessTacIds.Contains(tac.PlanTacticId)).Select(tac => tac.PlanTacticId).ToList();
                        }
                    }

                    #region "Linked Tactic: Create comma separated created & updated linked tactic"
                    if (lstCreateLinkedTacIds != null && lstCreateLinkedTacIds.Count > 0)
                        strCreatedLinkedTacIds = string.Join(",", lstCreateLinkedTacIds);
                    if (lstUpdateLinkedTacIds != null && lstUpdateLinkedTacIds.Count > 0)
                        strUpdatedLinkedTacIds = string.Join(",", lstUpdateLinkedTacIds);
                    #endregion

                    #region "Plan Tactic: Create comma separated created & updated Plan Tactic Ids"
                    if (lstCreatedTacIds != null && lstCreatedTacIds.Count > 0)
                        strCreatedTacIds = string.Join(",", lstCreatedTacIds);
                    if (lstUpdateTacIds != null && lstUpdateTacIds.Count > 0)
                        strUpdatedTacIds = string.Join(",", lstUpdateTacIds);
                    #endregion
                }
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "UpdateTacticInstanceTacticId_Comment execution start.");
                    MRPEntities mp = new MRPEntities();
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = mp.Database.Connection.ConnectionString;
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UpdateTacticInstanceTacticId_Comment_API", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@strCreatedTacIds", strCreatedTacIds);
                    cmd.Parameters.AddWithValue("@strUpdatedTacIds", strUpdatedTacIds);
                    cmd.Parameters.AddWithValue("@strCrtCampaignIds", strCrtCampaignIds);
                    cmd.Parameters.AddWithValue("@strUpdCampaignIds", strUpdCampaignIds);
                    cmd.Parameters.AddWithValue("@strCrtProgramIds", strCrtProgramIds);
                    cmd.Parameters.AddWithValue("@strUpdProgramIds", strUpdProgramIds);
                    cmd.Parameters.AddWithValue("@strCrtImprvmntTacIds", strCrtImprvTacIds);
                    cmd.Parameters.AddWithValue("@strUpdImprvmntTacIds", strUpdImprvTacIds);
                    cmd.Parameters.AddWithValue("@isAutoSync", Common.IsAutoSync);
                    cmd.Parameters.AddWithValue("@userId", _userId);
                    cmd.Parameters.AddWithValue("@integrationType", Enums.IntegrationType.Salesforce.ToString());
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    mp.Dispose();
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "UpdateTacticInstanceTacticId_Comment execution end.");
                

                #endregion

            }
            catch (Exception)
            {
                throw;
            }

            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "UpdateLinkedTacticCommentForAPI method end.");
        }

        #region "Push SFDC Data by API Methods"
        /// <summary>
        /// Function to sync data from gameplan to salesforce through Common Integration WEB API.
        /// </summary>
        /// <returns>Return Result Error Value</returns>
        public bool SyncSFDCDataByAPI(out List<SyncError> lstSyncError)
        {
            lstSyncError = new List<SyncError>();
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            // Insert log into IntegrationInstanceSection, Dharmraj PL#684
            _integrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), DateTime.Now, _userId);
            _isResultError = false;
            /// Set client Id based on integration instance.
            _clientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;
            

            bool IsInstanceSync = false;
            statusList = Common.GetStatusListAfterApproved();
            StringBuilder sbMessage = new StringBuilder();
            int logRecordSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LogRecordSize"]);
            int pushRecordBatchSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IntegrationPushRecordBatchSize"]);
            List<SalesForceObjectFieldDetails> lstMappingMisMatch = new List<SalesForceObjectFieldDetails>();
            List<fieldMapping> lstMisMatchFields = new List<fieldMapping>();
            try
            {
                #region "Validate SFDC Instance Field Mappings"
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                lstMappingMisMatch = ValidateMappingDetails();
                if (!_isResultError)
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Mapping details.");
                }
                else
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Error, "Set Mapping details.");
                    return _isResultError;
                }
                #endregion

                #region "Identify 3-Way integration of Marketo-SFDC"
                string published = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString();

                if (db.Models.Any(model => model.IsDeleted == false && model.IntegrationInstanceId == null &&
                   (model.IntegrationInstanceIdINQ == _integrationInstanceId || model.IntegrationInstanceIdMQL == _integrationInstanceId
                   || model.IntegrationInstanceIdCW == _integrationInstanceId)
                   && model.IntegrationInstanceMarketoID != null && model.IntegrationInstanceMarketoID != _integrationInstanceId))
                {
                    _IsSFDCWithMarketo = true;
                }
                #endregion

                #region "Get Integration TypeId"
                int integrationTypeId=0;
                integrationTypeId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId.Equals(_integrationInstanceId)).IntegrationTypeId;
                #endregion

                #region "Get Field Mappinglist"
                DataSet dsFieldMappings = new DataSet();
                DataTable dtFieldMappings = new DataTable();
                StoredProcedure objSp = new StoredProcedure();
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "callling GetSFDCFieldMappings to get mapping fields.");
                dsFieldMappings = (new StoredProcedure()).GetSFDCFieldMappings(_clientId, Convert.ToInt32(integrationTypeId), Convert.ToInt32(_integrationInstanceId), _IsSFDCWithMarketo);
                dtFieldMappings = dsFieldMappings.Tables[0];
                
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "convert filled map datatable to list .");
                List<Integration.fieldMapping> lstFiledMap = dtFieldMappings.AsEnumerable().Select(m => new Integration.fieldMapping
                {
                    sourceFieldName = m.Field<string>("sourceFieldName"),
                    destinationFieldName = m.Field<string>("destinationFieldName"),
                    fieldType = m.Field<string>("fieldType")
                }).ToList();

                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "creating a parameter list to call the StoreProcedure from API.");

                #endregion

                #region "Remove MisMatch mapping fields from fieldMapping list"
                
                #region "Declare local variables"
                #endregion


                lstMisMatchFields = (from p in lstFiledMap
                                     join l in lstMappingMisMatch on new { sourcefield = p.sourceFieldName, targetfield = p.destinationFieldName } equals new { sourcefield = l.SourceField, targetfield = l.TargetField }
                                     select p).ToList();
                if(lstMisMatchFields != null && lstMisMatchFields.Count >0)
                    lstFiledMap.RemoveAll(itm => lstMisMatchFields.Contains(itm));    // Remove all mis matched source fields from Field Mapping list. 

                #endregion

                #region "Make SP Parameter list"

                List<SpParameters> lstparams = new List<SpParameters>();
                SpParameters objSPParams;

                objSPParams = new SpParameters();
                objSPParams.parameterValue = _entityType.ToString();
                objSPParams.name = "entityType";
                lstparams.Add(objSPParams);

                objSPParams = new SpParameters();
                objSPParams.parameterValue = _id;
                objSPParams.name = "id";
                lstparams.Add(objSPParams);

                objSPParams = new SpParameters();
                objSPParams.parameterValue = _clientId.ToString();
                objSPParams.name = "clientId";
                lstparams.Add(objSPParams);

                objSPParams = new SpParameters();
                objSPParams.parameterValue = sfdcTitleLengthLimit;
                objSPParams.name = "SFDCTitleLengthLimit";
                lstparams.Add(objSPParams);

                objSPParams = new SpParameters();
                objSPParams.parameterValue = _integrationInstanceLogId;
                objSPParams.name = "integrationInstanceLogId";
                lstparams.Add(objSPParams);

                objSPParams = new SpParameters();
                objSPParams.parameterValue = IsClientAllowedForCustomNaming;
                objSPParams.name = "isClientAllowCustomName";
                lstparams.Add(objSPParams);

                #endregion

                #region "Make SFDC Credential Dictionary"
                Dictionary<string,string> salesforceCredentials = new Dictionary<string,string>();
                salesforceCredentials.Add("ConsumerKey", _consumerKey);
                salesforceCredentials.Add("ClientSecret", _consumerSecret);
                salesforceCredentials.Add("SecurityToken", _securityToken);
                salesforceCredentials.Add("Username", _username);
                salesforceCredentials.Add("Password", _password);
                salesforceCredentials.Add("APIUrl", _apiURL);
                #endregion

                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "parameter list created to call the StoreProcedure from API.");

                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Call Api to push Marketo Data and return error log.");
                
                #region "Declare Local Variables"
                List<Integration.LogDetails> logDetailsList = new List<LogDetails>();
                List<SFDCAPICampaignResult> lstCampaigndata = new List<SFDCAPICampaignResult>();
                ReturnObject ro = new ReturnObject();
                string spSFDCPUshDataName = string.Empty; 
                #endregion
                
                // Pass SP based on sync process whether it's 3-way between SFDC & Marketo or not
                if (_IsSFDCWithMarketo)
                    spSFDCPUshDataName = "spGetSalesforceMarketo3WayData";
                else
                    spSFDCPUshDataName = "spGetSalesforceData";

                #region "Call SFDC Integration API"
                try
                {
                    ro = (new ApiIntegration()).SFDCData_Push(spSFDCPUshDataName, lstFiledMap, _applicationId.ToString(), _clientId, lstparams, salesforceCredentials);
                }
                catch (Exception)
                {
                    // This catch remains blank to continue log execution process getting from Integration WEB API.
                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Marketo Log detail list get successfully.");
                
                #endregion

                #region "Get Log details"
                if (ro != null && ro.logs != null)
                    logDetailsList = ro.logs;
                if (ro != null && ro.logs != null)
                    lstCampaigndata = ro.campaignIds;
                #endregion

                #region "Insert Logs & Update SFDC Id to respective table"

                #region "Declare local variables"
                string crtMode = Operation.Create.ToString();
                IntegrationInstancePlanEntityLog instanceEntity = new IntegrationInstancePlanEntityLog();
                IntegrationInstanceLogDetail instanceLogDetail = new IntegrationInstanceLogDetail();
                Dictionary<int, string> TacticMarketoProgMappingIds = new Dictionary<int, string>();
                Dictionary<int, DateTime> TacticMarketoLastSyncDates = new Dictionary<int, DateTime>();

                // Declare all Plan entities table variables
                List<Plan_Campaign> tblCampaigns = new List<Plan_Campaign>();
                List<Plan_Campaign_Program> tblPrograms = new List<Plan_Campaign_Program>();
                List<Plan_Campaign_Program_Tactic> tblTactics = new List<Plan_Campaign_Program_Tactic>();
                List<Plan_Improvement_Campaign> tblImprvCampaigns = new List<Plan_Improvement_Campaign>();
                List<Plan_Improvement_Campaign_Program> tblImprvPrograms = new List<Plan_Improvement_Campaign_Program>();
                List<Plan_Improvement_Campaign_Program_Tactic> tblImprvTactics = new List<Plan_Improvement_Campaign_Program_Tactic>();

                // Plan Entities variables
                string campObjType= Enums.EntityType.Campaign.ToString().ToUpper();
                string progObjType = Enums.EntityType.Program.ToString().ToUpper();
                string tacObjType = Enums.EntityType.Tactic.ToString().ToUpper();
                string ImprvCampObjType = Enums.EntityType.ImprovementCampaign.ToString().ToUpper();
                string ImprvProgObjType = Enums.EntityType.ImprovementProgram.ToString().ToUpper();
                string ImprvtacObjType = Enums.EntityType.ImprovementTactic.ToString().ToUpper();

                List<int> lstallOthrEntIds = new List<int>();
                List<int> lstpshOthrEntIds = new List<int>();

                List<int> lstallTacIds = new List<int>();
                List<int> lstpshTacIds = new List<int>();
                #endregion

                #region "Get all pushed Campaigns data"
                if (logDetailsList.Count > 0 && logDetailsList != null)
                    lstallOthrEntIds = logDetailsList.Where(log => log.ObjectType.ToUpper() == campObjType && log.SourceId.HasValue).Select(log => log.SourceId.Value).ToList();
                if (lstCampaigndata != null && lstCampaigndata.Count > 0)
                    lstpshOthrEntIds = lstCampaigndata.Where(tac => tac.ObjectType.ToUpper() == campObjType).Select(tac => Convert.ToInt32(tac.SourceId)).ToList();
                lstallOthrEntIds.AddRange(lstpshOthrEntIds);
                tblCampaigns = db.Plan_Campaign.Where(ent => lstallOthrEntIds.Contains(ent.PlanCampaignId)).ToList();
                #endregion

                #region "Get all pushed Programs data"
                lstallOthrEntIds = new List<int>(); // reset variables.
                lstpshOthrEntIds = new List<int>(); // reset variables.

                if (logDetailsList.Count > 0 && logDetailsList != null)
                    lstallOthrEntIds = logDetailsList.Where(log => log.ObjectType.ToUpper() == progObjType && log.SourceId.HasValue).Select(log => log.SourceId.Value).ToList();
                if (lstCampaigndata != null && lstCampaigndata.Count > 0)
                    lstpshOthrEntIds = lstCampaigndata.Where(tac => tac.ObjectType.ToUpper() == progObjType).Select(tac => Convert.ToInt32(tac.SourceId)).ToList();
                lstallOthrEntIds.AddRange(lstpshOthrEntIds);
                tblPrograms = db.Plan_Campaign_Program.Where(ent => lstallOthrEntIds.Contains(ent.PlanProgramId)).ToList();
                #endregion

                #region "Get all pushed tactic data"
                if (logDetailsList.Count > 0 && logDetailsList != null)
                    lstallTacIds = logDetailsList.Where(log => log.ObjectType.ToUpper() == tacObjType && log.SourceId.HasValue).Select(log => log.SourceId.Value).ToList();
                if (lstCampaigndata != null && lstCampaigndata.Count > 0)
                    lstpshTacIds = lstCampaigndata.Where(tac => tac.ObjectType.ToUpper() == tacObjType).Select(tac => Convert.ToInt32(tac.SourceId)).ToList();
                lstallTacIds.AddRange(lstpshTacIds);
                tblTactics = db.Plan_Campaign_Program_Tactic.Where(tac => lstallTacIds.Contains(tac.PlanTacticId)).ToList(); 
                #endregion

                #region "Get all pushed Improvement Campaigns data"
                lstallOthrEntIds = new List<int>(); // reset variables.
                lstpshOthrEntIds = new List<int>(); // reset variables.

                if (logDetailsList.Count > 0 && logDetailsList != null)
                    lstallOthrEntIds = logDetailsList.Where(log => log.ObjectType.ToUpper() == ImprvCampObjType && log.SourceId.HasValue).Select(log => log.SourceId.Value).ToList();
                if (lstCampaigndata != null && lstCampaigndata.Count > 0)
                    lstpshOthrEntIds = lstCampaigndata.Where(tac => tac.ObjectType.ToUpper() == ImprvCampObjType).Select(tac => Convert.ToInt32(tac.SourceId)).ToList();
                lstallOthrEntIds.AddRange(lstpshOthrEntIds);

                tblImprvCampaigns = db.Plan_Improvement_Campaign.Where(ent => lstallOthrEntIds.Contains(ent.ImprovementPlanCampaignId)).ToList();
                #endregion

                #region "Get all pushed Improvement Program data"
                lstallOthrEntIds = new List<int>(); // reset variables.
                lstpshOthrEntIds = new List<int>(); // reset variables.

                if (logDetailsList.Count > 0 && logDetailsList != null)
                    lstallOthrEntIds = logDetailsList.Where(log => log.ObjectType.ToUpper() == ImprvProgObjType && log.SourceId.HasValue).Select(log => log.SourceId.Value).ToList();
                if (lstCampaigndata != null && lstCampaigndata.Count > 0)
                    lstpshOthrEntIds = lstCampaigndata.Where(tac => tac.ObjectType.ToUpper() == ImprvProgObjType).Select(tac => Convert.ToInt32(tac.SourceId)).ToList();
                lstallOthrEntIds.AddRange(lstpshOthrEntIds);

                tblImprvPrograms = db.Plan_Improvement_Campaign_Program.Where(ent => lstallOthrEntIds.Contains(ent.ImprovementPlanProgramId)).ToList();
                #endregion
                
                #region "Get all pushed Improvement Tactic data"
                lstallOthrEntIds = new List<int>(); // reset variables.
                lstpshOthrEntIds = new List<int>(); // reset variables.

                if (logDetailsList.Count > 0 && logDetailsList != null)
                    lstallOthrEntIds = logDetailsList.Where(log => log.ObjectType.ToUpper() == ImprvtacObjType && log.SourceId.HasValue).Select(log => log.SourceId.Value).ToList();
                if (lstCampaigndata != null && lstCampaigndata.Count > 0)
                    lstpshOthrEntIds = lstCampaigndata.Where(tac => tac.ObjectType.ToUpper() == ImprvtacObjType).Select(tac => Convert.ToInt32(tac.SourceId)).ToList();
                lstallOthrEntIds.AddRange(lstpshOthrEntIds);

                tblImprvTactics = db.Plan_Improvement_Campaign_Program_Tactic.Where(ent => lstallOthrEntIds.Contains(ent.ImprovementPlanTacticId)).ToList();
                #endregion

                #region "Declare Enums & local variables"
                string preIntgrtnWebAPIMsg = "Error throws from Integration Web API: ";
                string strEventAuthentication = Enums.MarketoAPIEventNames.Authentication.ToString();
                string strFailure = "FAILURE";
                int entId = 0;
                string entTitle, exMessage;
                #endregion

                #region "Check log for integration instance section"
                string strSalesforcePush = "SalesforcePush";
                //isExist = logDetailsList.Where(log => (log.EventName.Equals(strSalesforcePush)) && log.Status.ToUpper().Equals("FAILURE")).Any();
                if (logDetailsList.Any(log => (log.EventName.ToUpper() == strSalesforcePush.ToUpper()) && log.Status.ToUpper() == strFailure))
                {
                    _isResultError = true;
                    _ErrorMessage = logDetailsList.Where(log => (log.EventName.ToUpper() == strSalesforcePush.ToUpper()) && log.Status.ToUpper() == strFailure).Select(err => err.Description).FirstOrDefault();//"Error in getting data from source, to push to marketo";
                }
                #endregion

                #region "Original Log for Push SFDC"
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Insert Log detail into IntegrationInstanceLogDetails start.");
                //Insert Failure error log to IntegrationInstanceLogDetails table 
                foreach (var logdetail in logDetailsList.Where(log => log.Status.ToUpper() == strFailure))
                {
                    //if (logdetail.Status.ToUpper().Equals("FAILURE"))
                    //{

                    instanceLogDetail = new IntegrationInstanceLogDetail();
                    entId = Convert.ToInt32(logdetail.SourceId);
                    instanceLogDetail.EntityId = entId;
                    instanceLogDetail.IntegrationInstanceLogId = _integrationInstanceLogId;
                    instanceLogDetail.LogTime = logdetail.EndTimeStamp;
                    instanceLogDetail.LogDescription = preIntgrtnWebAPIMsg + logdetail.Description;
                    db.Entry(instanceLogDetail).State = EntityState.Added;
                    //}
                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Insert Log detail into IntegrationInstanceLogDetails end.");
                #endregion
               
                #region "Entity Logs for each tactic"
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Insert Log detail into IntegrationInstancePlanEntityLog start.");
                List<Integration.LogDetails> logDetailsList1 = logDetailsList.Where(log => (log.EventName.ToUpper() == strSalesforcePush.ToUpper()) && log.Status.ToUpper() == strFailure && log.SourceId.HasValue && log.SourceId.Value > 0).ToList();
                int pushCntr = 0;
                try
                {
                    #region "Insert error logs to PlanEntityLog table"
                    foreach (var logdetail in logDetailsList1)
                    {

                        db.Configuration.AutoDetectChangesEnabled = false;
                        //Insert  log into IntegrationInstancePlanEntityLog table 
                        entId = Convert.ToInt32(logdetail.SourceId);
                        instanceEntity = new IntegrationInstancePlanEntityLog();
                        instanceEntity.IntegrationInstanceId = _integrationInstanceId;
                        instanceEntity.EntityId = entId;
                        instanceEntity.EntityType = logdetail.ObjectType;//Enums.EntityType.Tactic.ToString();
                        instanceEntity.SyncTimeStamp = logdetail.EndTimeStamp;
                        if (logdetail.Mode != null)
                            instanceEntity.Operation = logdetail.Mode.ToString();
                        if (logdetail.Status.ToUpper() == strFailure)
                        {
                            instanceEntity.Status = Enums.SyncStatus.Error.ToString();
                        }
                        else
                        {
                            instanceEntity.Status = logdetail.Status.ToString();
                        }
                        instanceEntity.ErrorDescription = logdetail.Description;
                        instanceEntity.CreatedBy = _userId;
                        instanceEntity.CreatedDate = DateTime.Now;
                        instanceEntity.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                        db.Entry(instanceEntity).State = EntityState.Added;

                        if (!_lstSyncError.Any(lserr => lserr.EntityId.Equals(entId)))
                        {
                            if (logdetail.Status.ToUpper() == strFailure)
                            {
                                //Add Failure Log for Summary Email.
                                _isResultError = true;
                                entTitle = string.Empty;

                                #region " Insert error logs for each entity to  summary email list"
                                if (logdetail.ObjectType.ToUpper() == campObjType)
                                {
                                    entTitle = tblCampaigns.Where(ent => ent.PlanCampaignId == entId).Select(ent => ent.Title).FirstOrDefault();
                                    exMessage = "System error occurred while create/update " + logdetail.ObjectType + " \"" + entTitle + "\": " + logdetail.Description;
                                    _lstSyncError.Add(Common.PrepareSyncErrorList(entId, Enums.EntityType.Campaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                }
                                else if (logdetail.ObjectType.ToUpper() == progObjType)
                                {
                                    entTitle = tblPrograms.Where(ent => ent.PlanProgramId == entId).Select(ent => ent.Title).FirstOrDefault();
                                    exMessage = "System error occurred while create/update " + logdetail.ObjectType + " \"" + entTitle + "\": " + logdetail.Description;
                                    _lstSyncError.Add(Common.PrepareSyncErrorList(entId, Enums.EntityType.Program, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                }
                                else if (logdetail.ObjectType.ToUpper() == tacObjType)
                                {
                                    entTitle = tblTactics.Where(ent => ent.PlanTacticId == entId).Select(ent => ent.Title).FirstOrDefault();
                                    exMessage = "System error occurred while create/update " + logdetail.ObjectType + " \"" + entTitle + "\": " + logdetail.Description;
                                    _lstSyncError.Add(Common.PrepareSyncErrorList(entId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                }
                                if (logdetail.ObjectType.ToUpper() == ImprvCampObjType)
                                {
                                    entTitle = tblImprvCampaigns.Where(ent => ent.ImprovementPlanCampaignId == entId).Select(ent => ent.Title).FirstOrDefault();
                                    exMessage = "System error occurred while create/update " + logdetail.ObjectType + " \"" + entTitle + "\": " + logdetail.Description;
                                    _lstSyncError.Add(Common.PrepareSyncErrorList(entId, Enums.EntityType.ImprovementCampaign, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                }
                                if (logdetail.ObjectType.ToUpper() == ImprvProgObjType)
                                {
                                    entTitle = tblImprvPrograms.Where(ent => ent.ImprovementPlanProgramId == entId).Select(ent => ent.Title).FirstOrDefault();
                                    exMessage = "System error occurred while create/update " + logdetail.ObjectType + " \"" + entTitle + "\": " + logdetail.Description;
                                    _lstSyncError.Add(Common.PrepareSyncErrorList(entId, Enums.EntityType.ImprovementProgram, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                }
                                if (logdetail.ObjectType.ToUpper() == ImprvtacObjType)
                                {
                                    entTitle = tblImprvTactics.Where(ent => ent.ImprovementPlanTacticId == entId).Select(ent => ent.Title).FirstOrDefault();
                                    exMessage = "System error occurred while create/update " + logdetail.ObjectType + " \"" + entTitle + "\": " + logdetail.Description;
                                    _lstSyncError.Add(Common.PrepareSyncErrorList(entId, Enums.EntityType.ImprovementTactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                } 
                                #endregion
                            }
                            else
                            {
                                if (logdetail.ObjectType.ToUpper() == tacObjType)
                                {
                                    //Add Success Log for Summary Email.
                                    exMessage = logdetail.Mode != null ? logdetail.Mode.ToString() : string.Empty;
                                    _lstSyncError.Add(Common.PrepareSyncErrorList(entId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Success, DateTime.Now));
                                }
                            }
                        }
                        pushCntr++;
                        if (((pushCntr) % pushRecordBatchSize) == 0)
                        {
                            pushCntr = 0;
                            db.SaveChanges();
                        }
                    } 
                    #endregion
                    
                    // if there is any record remaining to save then save by below check.
                    if (pushCntr > 0)
                        db.SaveChanges();
                    pushCntr = 0;

                    // Insert logs for all successfully pushed sfdc data.
                    #region "Insert logs to PlanEntityLog table for all successfully pushed sfdc data"
                    foreach (SFDCAPICampaignResult objEnt in lstCampaigndata)
                    {

                        db.Configuration.AutoDetectChangesEnabled = false;
                        //Insert  log into IntegrationInstancePlanEntityLog table 
                        entId = Convert.ToInt32(objEnt.SourceId);
                        instanceEntity = new IntegrationInstancePlanEntityLog();
                        instanceEntity.IntegrationInstanceId = _integrationInstanceId;
                        instanceEntity.EntityId = entId;
                        instanceEntity.EntityType = objEnt.ObjectType;//Enums.EntityType.Tactic.ToString();
                        instanceEntity.SyncTimeStamp = objEnt.EndTimeStamp;
                        instanceEntity.Operation = objEnt.Mode;
                        instanceEntity.Status = "Success";
                        instanceEntity.ErrorDescription = string.Empty;
                        instanceEntity.CreatedBy = _userId;
                        instanceEntity.CreatedDate = DateTime.Now;
                        instanceEntity.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                        db.Entry(instanceEntity).State = EntityState.Added;

                        //Add Success Log for Summary Email.
                        if (objEnt.ObjectType.ToUpper() == tacObjType)
                        {
                            exMessage = objEnt.Mode.ToString();
                            _lstSyncError.Add(Common.PrepareSyncErrorList(entId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Success, DateTime.Now));
                        }

                        pushCntr++;
                        if (((pushCntr) % pushRecordBatchSize) == 0)
                        {
                            pushCntr = 0;
                            db.SaveChanges();
                        }
                    } 
                    #endregion
                    
                    // if there is any record remaining to save then save by below check.
                    if (pushCntr > 0)
                        db.SaveChanges();
                    pushCntr = 0;
                }
                finally
                {
                    db.Configuration.AutoDetectChangesEnabled = true;
                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Insert Log detail into IntegrationInstancePlanEntityLog end.");
                #endregion

                #region "Update new created Campaign's SFDC Id"
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Update Campaign's SFDC Id start.");
                List<int> lstCreatedCampIds = new List<int>();
                Dictionary<int, string> lstCampSFDCIdMapping = lstCampaigndata.Where(ent => ent.ObjectType.ToUpper() == campObjType && ent.Mode.ToUpper() == crtMode.ToUpper()).ToDictionary(ent => Convert.ToInt32(ent.SourceId), ent => ent.CampaignId.ToString());
                Dictionary<int, DateTime> lstCampSyncMapping = lstCampaigndata.Where(ent => ent.ObjectType.ToUpper() == campObjType && ent.Mode.ToUpper() == crtMode.ToUpper()).ToDictionary(ent => Convert.ToInt32(ent.SourceId), ent => ent.EndTimeStamp);

                if (lstCampSFDCIdMapping != null && lstCampSFDCIdMapping.Count > 0)
                {
                    string strSFDCId;
                    DateTime campLastSync;
                    lstCreatedCampIds = lstCampSFDCIdMapping.Select(ent => ent.Key).ToList();
                    List<Plan_Campaign> lstCreatedCamps = tblCampaigns.Where(ent => lstCreatedCampIds.Contains(ent.PlanCampaignId)).ToList();
                    pushCntr = 0;
                    try
                    {
                        foreach (Plan_Campaign ent in lstCreatedCamps)
                        {
                            db.Configuration.AutoDetectChangesEnabled = false;
                            #region "Update LastSync & SalesforceId for created Campaign"
                            strSFDCId = string.Empty;
                            strSFDCId = lstCampSFDCIdMapping.Where(prg => prg.Key == ent.PlanCampaignId).Select(prg => prg.Value).FirstOrDefault();
                            campLastSync = lstCampSyncMapping.Where(prg => prg.Key == ent.PlanCampaignId).Select(prg => prg.Value).FirstOrDefault();
                            if (strSFDCId != null)
                            {
                                ent.IntegrationInstanceCampaignId = strSFDCId;
                                ent.LastSyncDate = campLastSync; // Modified By Rahul shah // To add last sync date
                                db.Entry(ent).State = EntityState.Modified;
                            }
                            else
                            {
                                _isResultError = true;
                                _ErrorMessage = "Error updating Salesforce id for Campaign - " + ent.Title;
                            }
                            #endregion
                            pushCntr++;
                            if (((pushCntr) % pushRecordBatchSize) == 0)
                            {
                                pushCntr = 0;
                                db.SaveChanges();
                            }
                            // End By Nishant Sheth
                        }
                        if (pushCntr > 0)
                            db.SaveChanges();
                    }
                    finally
                    {
                        db.Configuration.AutoDetectChangesEnabled = true;
                    }
                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Update Campaign's SFDC Id end.");
                #endregion

                #region "Update new created Program's SFDC Id"
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Update Program's SFDC Id start.");
                List<int> lstCreatedProgIds = new List<int>();
                Dictionary<int, string> lstProgSFDCIdMapping = lstCampaigndata.Where(ent => ent.ObjectType.ToUpper() == progObjType && ent.Mode.ToUpper() == crtMode.ToUpper()).ToDictionary(ent => Convert.ToInt32(ent.SourceId), ent => ent.CampaignId.ToString());
                Dictionary<int, DateTime> lstProgSyncMapping = lstCampaigndata.Where(ent => ent.ObjectType.ToUpper() == progObjType && ent.Mode.ToUpper() == crtMode.ToUpper()).ToDictionary(ent => Convert.ToInt32(ent.SourceId), ent => ent.EndTimeStamp);

                if (lstProgSFDCIdMapping != null && lstProgSFDCIdMapping.Count > 0)
                {
                    string strSFDCId;
                    DateTime progLastSync;
                    lstCreatedProgIds = lstProgSFDCIdMapping.Select(ent => ent.Key).ToList();
                    List<Plan_Campaign_Program> lstCreatedProgs = tblPrograms.Where(ent => lstCreatedProgIds.Contains(ent.PlanProgramId)).ToList();
                    pushCntr = 0;
                    try
                    {
                        foreach (Plan_Campaign_Program ent in lstCreatedProgs)
                        {
                            db.Configuration.AutoDetectChangesEnabled = false;
                            #region "Update LastSync & SalesforceId for created Program"
                            strSFDCId = string.Empty;
                            strSFDCId = lstProgSFDCIdMapping.Where(prg => prg.Key == ent.PlanProgramId).Select(prg => prg.Value).FirstOrDefault();
                            progLastSync = lstProgSyncMapping.Where(prg => prg.Key == ent.PlanProgramId).Select(prg => prg.Value).FirstOrDefault();
                            if (strSFDCId != null)
                            {
                                ent.IntegrationInstanceProgramId = strSFDCId;
                                ent.LastSyncDate = progLastSync; // Modified By Rahul shah // To add last sync date
                                db.Entry(ent).State = EntityState.Modified;
                            }
                            else
                            {
                                _isResultError = true;
                                _ErrorMessage = "Error updating Salesforce id for Program - " + ent.Title;
                            }
                            #endregion
                            pushCntr++;
                            if (((pushCntr) % pushRecordBatchSize) == 0)
                            {
                                pushCntr = 0;
                                db.SaveChanges();
                            }
                            // End By Nishant Sheth
                        }
                        if (pushCntr > 0)
                            db.SaveChanges();
                    }
                    finally
                    {
                        db.Configuration.AutoDetectChangesEnabled = true;
                    }
                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Update Program's SFDC Id end.");
                #endregion

                #region "Update new created Tactic's SFDC Id"
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Update Tactic's SFDC Id start.");
                List<int> lstCreatedTacIds = new List<int>();
                Dictionary<int, string> lstTactSFDCIdMapping = lstCampaigndata.Where(ent => ent.ObjectType.ToUpper() == tacObjType && ent.Mode.ToUpper() == crtMode.ToUpper()).ToDictionary(ent => Convert.ToInt32(ent.SourceId), ent => ent.CampaignId.ToString());
                Dictionary<int, DateTime> lstTactSyncMapping = lstCampaigndata.Where(ent => ent.ObjectType.ToUpper() == tacObjType && ent.Mode.ToUpper() == crtMode.ToUpper()).ToDictionary(ent => Convert.ToInt32(ent.SourceId), ent => ent.EndTimeStamp);

                if (lstTactSFDCIdMapping != null && lstTactSFDCIdMapping.Count > 0)
                {
                    string EntityType = Enums.EntityType.Tactic.ToString();
                    
                    string strSFDCId;
                    DateTime tacLastSync;
                    lstCreatedTacIds = lstTactSFDCIdMapping.Select(tac => tac.Key).ToList();
                    List<Plan_Campaign_Program_Tactic> lstCreatedTacs = tblTactics.Where(tac => lstCreatedTacIds.Contains(tac.PlanTacticId)).ToList();
                    pushCntr = 0;
                    try
                    {
                        foreach (Plan_Campaign_Program_Tactic tac in lstCreatedTacs)
                        {
                            db.Configuration.AutoDetectChangesEnabled = false;
                            #region "Update LastSync & SalesforceId for created Campaign, Program & Tactic"
                            strSFDCId = string.Empty;
                            strSFDCId = lstTactSFDCIdMapping.Where(prg => prg.Key == tac.PlanTacticId).Select(prg => prg.Value).FirstOrDefault();
                            tacLastSync = lstTactSyncMapping.Where(prg => prg.Key == tac.PlanTacticId).Select(prg => prg.Value).FirstOrDefault();
                            if (strSFDCId != null)
                            {
                                tac.IntegrationInstanceTacticId = strSFDCId;
                                tac.LastSyncDate = tacLastSync; // Modified By Rahul shah // To add last sync date
                                db.Entry(tac).State = EntityState.Modified;
                            }
                            else
                            {
                                _isResultError = true;
                                _ErrorMessage = "Error updating Salesforce id for Tactic - " + tac.Title;
                            } 
                            #endregion
                            pushCntr++;
                            if (((pushCntr) % pushRecordBatchSize) == 0)
                            {
                                pushCntr = 0;
                                db.SaveChanges();
                            }
                            // End By Nishant Sheth
                        }
                        if (pushCntr > 0)
                            db.SaveChanges();
                    }
                    finally
                    {
                        db.Configuration.AutoDetectChangesEnabled = true;
                    }
                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Update Tactic's SFDC Id end.");
                #endregion

                #region "Update new created Improvement Campaign's SFDC Id"
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Update Improvement Campaign's SFDC Id start.");
                List<int> lstCreatedImprvCampIds = new List<int>();
                Dictionary<int, string> lstImprvCampSFDCIdMapping = lstCampaigndata.Where(ent => ent.ObjectType.ToUpper() == ImprvCampObjType && ent.Mode.ToUpper() == crtMode.ToUpper()).ToDictionary(ent => Convert.ToInt32(ent.SourceId), ent => ent.CampaignId.ToString());
                Dictionary<int, DateTime> lstImprvCampSyncMapping = lstCampaigndata.Where(ent => ent.ObjectType.ToUpper() == ImprvCampObjType && ent.Mode.ToUpper() == crtMode.ToUpper()).ToDictionary(ent => Convert.ToInt32(ent.SourceId), ent => ent.EndTimeStamp);

                if (lstImprvCampSFDCIdMapping != null && lstImprvCampSFDCIdMapping.Count > 0)
                {
                    string strSFDCId;
                    DateTime ImprvCampLastSync;
                    lstCreatedImprvCampIds = lstImprvCampSFDCIdMapping.Select(ent => ent.Key).ToList();
                    List<Plan_Improvement_Campaign> lstCreatedImprvCamp = tblImprvCampaigns.Where(ent => lstCreatedImprvCampIds.Contains(ent.ImprovementPlanCampaignId)).ToList();
                    pushCntr = 0;
                    try
                    {
                        foreach (Plan_Improvement_Campaign ent in lstCreatedImprvCamp)
                        {
                            db.Configuration.AutoDetectChangesEnabled = false;
                            #region "Update LastSync & SalesforceId for created Program"
                            strSFDCId = string.Empty;
                            strSFDCId = lstImprvCampSFDCIdMapping.Where(prg => prg.Key == ent.ImprovementPlanCampaignId).Select(prg => prg.Value).FirstOrDefault();
                            ImprvCampLastSync = lstImprvCampSyncMapping.Where(prg => prg.Key == ent.ImprovementPlanCampaignId).Select(prg => prg.Value).FirstOrDefault();
                            if (strSFDCId != null)
                            {
                                ent.IntegrationInstanceCampaignId = strSFDCId;
                                ent.LastSyncDate = ImprvCampLastSync; // Modified By Rahul shah // To add last sync date
                                db.Entry(ent).State = EntityState.Modified;
                            }
                            else
                            {
                                _isResultError = true;
                                _ErrorMessage = "Error updating Salesforce id for Improvement Campaign - " + ent.Title;
                            }
                            #endregion
                            pushCntr++;
                            if (((pushCntr) % pushRecordBatchSize) == 0)
                            {
                                pushCntr = 0;
                                db.SaveChanges();
                            }
                            // End By Nishant Sheth
                        }
                        if (pushCntr > 0)
                            db.SaveChanges();
                    }
                    finally
                    {
                        db.Configuration.AutoDetectChangesEnabled = true;
                    }
                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Update Improvement Campaign's SFDC Id end.");
                #endregion

                #region "Update new created Improvement Program's SFDC Id"
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Update Improvement Program's SFDC Id start.");
                List<int> lstCreatedImprvPrgIds = new List<int>();
                Dictionary<int, string> lstImprvPrgSFDCIdMapping = lstCampaigndata.Where(ent => ent.ObjectType.ToUpper() == ImprvProgObjType && ent.Mode.ToUpper() == crtMode.ToUpper()).ToDictionary(ent => Convert.ToInt32(ent.SourceId), ent => ent.CampaignId.ToString());
                Dictionary<int, DateTime> lstImprvPrgSyncMapping = lstCampaigndata.Where(ent => ent.ObjectType.ToUpper() == ImprvProgObjType && ent.Mode.ToUpper() == crtMode.ToUpper()).ToDictionary(ent => Convert.ToInt32(ent.SourceId), ent => ent.EndTimeStamp);

                if (lstImprvPrgSFDCIdMapping != null && lstImprvPrgSFDCIdMapping.Count > 0)
                {
                    string strSFDCId;
                    DateTime ImprvPrgLastSync;
                    lstCreatedImprvPrgIds = lstImprvPrgSFDCIdMapping.Select(ent => ent.Key).ToList();
                    List<Plan_Improvement_Campaign_Program> lstCreatedImprvPrg = tblImprvPrograms.Where(ent => lstCreatedImprvPrgIds.Contains(ent.ImprovementPlanProgramId)).ToList();
                    pushCntr = 0;
                    try
                    {
                        foreach (Plan_Improvement_Campaign_Program ent in lstCreatedImprvPrg)
                        {
                            db.Configuration.AutoDetectChangesEnabled = false;
                            #region "Update LastSync & SalesforceId for created Program"
                            strSFDCId = string.Empty;
                            strSFDCId = lstImprvPrgSFDCIdMapping.Where(prg => prg.Key == ent.ImprovementPlanProgramId).Select(prg => prg.Value).FirstOrDefault();
                            ImprvPrgLastSync = lstImprvPrgSyncMapping.Where(prg => prg.Key == ent.ImprovementPlanProgramId).Select(prg => prg.Value).FirstOrDefault();
                            if (strSFDCId != null)
                            {
                                ent.IntegrationInstanceProgramId = strSFDCId;
                                ent.LastSyncDate = ImprvPrgLastSync; // Modified By Rahul shah // To add last sync date
                                db.Entry(ent).State = EntityState.Modified;
                            }
                            else
                            {
                                _isResultError = true;
                                _ErrorMessage = "Error updating Salesforce id for Improvement Program - " + ent.Title;
                            }
                            #endregion
                            pushCntr++;
                            if (((pushCntr) % pushRecordBatchSize) == 0)
                            {
                                pushCntr = 0;
                                db.SaveChanges();
                            }
                            // End By Nishant Sheth
                        }
                        if (pushCntr > 0)
                            db.SaveChanges();
                    }
                    finally
                    {
                        db.Configuration.AutoDetectChangesEnabled = true;
                    }
                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Update Improvement Program's SFDC Id end.");
                #endregion

                #region "Update new created Improvement Tactic's SFDC Id"
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Update Improvement Tactic's SFDC Id start.");
                List<int> lstCreatedImprvTacIds = new List<int>();
                Dictionary<int, string> lstImprvTacSFDCIdMapping = lstCampaigndata.Where(ent => ent.ObjectType.ToUpper() == ImprvtacObjType && ent.Mode.ToUpper() == crtMode.ToUpper()).ToDictionary(ent => Convert.ToInt32(ent.SourceId), ent => ent.CampaignId.ToString());
                Dictionary<int, DateTime> lstImprvTacSyncMapping = lstCampaigndata.Where(ent => ent.ObjectType.ToUpper() == ImprvtacObjType && ent.Mode.ToUpper() == crtMode.ToUpper()).ToDictionary(ent => Convert.ToInt32(ent.SourceId), ent => ent.EndTimeStamp);

                if (lstImprvTacSFDCIdMapping != null && lstImprvTacSFDCIdMapping.Count > 0)
                {
                    string strSFDCId;
                    DateTime ImprvTacLastSync;
                    lstCreatedImprvTacIds = lstImprvTacSFDCIdMapping.Select(ent => ent.Key).ToList();
                    List<Plan_Improvement_Campaign_Program_Tactic> lstCreatedImprvTac = tblImprvTactics.Where(ent => lstCreatedImprvTacIds.Contains(ent.ImprovementPlanTacticId)).ToList();
                    pushCntr = 0;
                    try
                    {
                        foreach (Plan_Improvement_Campaign_Program_Tactic ent in lstCreatedImprvTac)
                        {
                            db.Configuration.AutoDetectChangesEnabled = false;
                            #region "Update LastSync & SalesforceId for created Program"
                            strSFDCId = string.Empty;
                            strSFDCId = lstImprvTacSFDCIdMapping.Where(prg => prg.Key == ent.ImprovementPlanTacticId).Select(prg => prg.Value).FirstOrDefault();
                            ImprvTacLastSync = lstImprvTacSyncMapping.Where(prg => prg.Key == ent.ImprovementPlanTacticId).Select(prg => prg.Value).FirstOrDefault();
                            if (strSFDCId != null)
                            {
                                ent.IntegrationInstanceTacticId = strSFDCId;
                                ent.LastSyncDate = ImprvTacLastSync; // Modified By Rahul shah // To add last sync date
                                db.Entry(ent).State = EntityState.Modified;
                            }
                            else
                            {
                                _isResultError = true;
                                _ErrorMessage = "Error updating Salesforce id for ImprovementTactic - " + ent.Title;
                            }
                            #endregion
                            pushCntr++;
                            if (((pushCntr) % pushRecordBatchSize) == 0)
                            {
                                pushCntr = 0;
                                db.SaveChanges();
                            }
                            // End By Nishant Sheth
                        }
                        if (pushCntr > 0)
                            db.SaveChanges();
                    }
                    finally
                    {
                        db.Configuration.AutoDetectChangesEnabled = true;
                    }
                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Update Improvement Tactic's SFDC Id end.");
                #endregion

                db.SaveChanges();

                #region "Create Tactic-Linked Tactic Mapping"
                Dictionary<int, int> lstTacLinkIdMappings = new Dictionary<int, int>();
                lstTacLinkIdMappings = tblTactics.Where(tac => lstallTacIds.Contains(tac.PlanTacticId) && tac.LinkedTacticId.HasValue).ToDictionary(tac => tac.PlanTacticId, tac => tac.LinkedTacticId.Value);
                #endregion

                #region "Make list of Created/Updated Campaign, Program and Improvement Tactic Ids"
                string strCrtCampaignIds, strUpdCampaignIds, strCrtProgramIds, strUpdProgramIds, strCrtImprvTacIds, strUpdImprvTacIds;
                strCrtCampaignIds = strUpdCampaignIds = strCrtProgramIds = strUpdProgramIds = strCrtImprvTacIds = strUpdImprvTacIds = string.Empty;
                List<int> lstUpdEntIds = new List<int>();
                // Get Created Campaigns
                if (lstCreatedCampIds != null && lstCreatedCampIds.Count > 0)
                    strCrtCampaignIds = string.Join(",", lstCreatedCampIds);

                // Get Updated Campaigns
                lstUpdEntIds = new List<int>();
                lstUpdEntIds = tblCampaigns.Where(ent => !lstCreatedCampIds.Contains(ent.PlanCampaignId)).Select(ent => ent.PlanCampaignId).ToList();
                if (lstUpdEntIds != null && lstUpdEntIds.Count > 0)
                    strUpdCampaignIds = string.Join(",", lstUpdEntIds);

                // Get Created Programs
                if (lstCreatedProgIds != null && lstCreatedProgIds.Count > 0)
                    strCrtProgramIds = string.Join(",", lstCreatedProgIds);

                // Get Updated Programs
                lstUpdEntIds = new List<int>();
                lstUpdEntIds = tblPrograms.Where(ent => !lstCreatedProgIds.Contains(ent.PlanProgramId)).Select(ent => ent.PlanProgramId).ToList();
                if (lstUpdEntIds != null && lstUpdEntIds.Count > 0)
                    strUpdProgramIds = string.Join(",", lstUpdEntIds);

                // Get Created Improvement Tactic
                if (lstCreatedImprvTacIds != null && lstCreatedImprvTacIds.Count > 0)
                    strCrtImprvTacIds = string.Join(",", lstCreatedImprvTacIds);

                // Get Updated Improvement Tactics
                lstUpdEntIds = new List<int>();
                lstUpdEntIds = tblImprvTactics.Where(ent => !lstCreatedImprvTacIds.Contains(ent.ImprovementPlanTacticId)).Select(ent => ent.ImprovementPlanTacticId).ToList();
                if (lstUpdEntIds != null && lstUpdEntIds.Count > 0)
                    strUpdImprvTacIds = string.Join(",", lstUpdEntIds);

                #endregion

                UpdateLinkedTacticCommentForAPI(lstallTacIds, tblTactics, lstCreatedTacIds, lstTacLinkIdMappings,strCrtCampaignIds,strUpdCampaignIds,strCrtProgramIds,strUpdProgramIds,strCrtImprvTacIds,strUpdImprvTacIds);

                #endregion
            }
            catch (Exception e)
            {
                string exMessage = Common.GetInnermostException(e);
                _isResultError = true;
                _ErrorMessage = exMessage;
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while syncing Tactic :- " + exMessage);
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
                IntegrationInstance objInstance = new IntegrationInstance();
                objInstance = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId.Equals(_integrationInstanceId));
                isImport = objInstance != null ? objInstance.IsImportActuals : false;
                if (isImport)
                {
                    //GetDataForTacticandUpdate();  // Commented by Sohel Pathan on 12/09/2014 for PL ticket #773
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Pulling Response execution start.");
                    PullingResponses();
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Pulling Response execution end.");

                    #region "Pull MQL based on client level MQL permission for SFDC"
                    // Pulling MQL from SFDC based on client level MQL permission for SFDC.
                    string strPermissionCode_MQL = Enums.ClientIntegrationPermissionCode.MQL.ToString();
                    if (db.Client_Integration_Permission.Any(intPermission => (intPermission.ClientId.Equals(_clientId)) && (intPermission.IntegrationTypeId.Equals(objInstance.IntegrationTypeId)) && (intPermission.PermissionCode.ToUpper().Equals(strPermissionCode_MQL.ToUpper()))))
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Pulling MQL execution start.");
                        PullingMQL();
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Pulling MQL execution end.");
                    }
                    #endregion

                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Pulling CWRevenue execution start.");
                    PullingCWRevenue();
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Pulling CWRevenue execution end.");
                }
                else
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pulling Data does not procceed due to IsImportActuals field not enabled for this Integration Instance.");
                }
            }
            lstSyncError.AddRange(_lstSyncError);
            return _isResultError;
        }

        /// <summary>
        ///  Validate SFDC mapping fields for Campaign, Program, Tactic & Improvement Campaign, Program & Tactic
        /// </summary>
        /// <returns>Returns invalid mapping fields list</returns>
        private List<SalesForceObjectFieldDetails> ValidateMappingDetails()
        {
            #region "Declare local variables"
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
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            List<SalesForceObjectFieldDetails> lstMappingMisMatch = new List<SalesForceObjectFieldDetails>(); 
            #endregion
            try
            {

                #region "Validate if user has not mapped any single field"
                List<IntegrationInstanceDataTypeMapping> dataTypeMapping = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.IntegrationInstanceId.Equals(_integrationInstanceId)).ToList();
                if (!dataTypeMapping.Any()) // check if there is no field mapping configure then log error to IntegrationInstanceLogDetails table.
                {
                    Enums.EntityType _entityTypeSection = (Enums.EntityType)Enum.Parse(typeof(Enums.EntityType), Convert.ToString(_entityType), true);
                    _ErrorMessage = "You have not configure any single field with Salesforce field.";
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, _entityTypeSection, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), _ErrorMessage, Enums.SyncStatus.Error, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "You have not configure any single field with Salesforce field.");
                    _isResultError = true;    // return true value that means error exist and do not proceed for the further mapping list.
                } 
                #endregion

                lstSalesforceFieldDetail = GetSFDCTargetFieldList();

                #region " Verify Field mapping for each entity "
                _mappingTactic = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign_Program_Tactic
                                                                || gameplandata.GameplanDataType.TableName == Global) : gameplandata.CustomField.EntityType == Tactic_EntityType) &&
                                                                (gameplandata.GameplanDataType != null ? !gameplandata.GameplanDataType.IsGet : true))
                                                            .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                            .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                lstMappingMisMatch = IdentifyDataTypeMisMatch(_mappingTactic, lstSalesforceFieldDetail, Enums.EntityType.Tactic.ToString());


                _mappingProgram = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign_Program
                                                        || gameplandata.GameplanDataType.TableName == Global) : gameplandata.CustomField.EntityType == Program_EntityType) &&
                                                        (gameplandata.GameplanDataType != null ? !gameplandata.GameplanDataType.IsGet : true))
                                                    .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                    .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                lstMappingMisMatch = lstMappingMisMatch.Concat(IdentifyDataTypeMisMatch(_mappingProgram, lstSalesforceFieldDetail, Enums.EntityType.Program.ToString())).ToList();


                _mappingCampaign = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign
                                                        || gameplandata.GameplanDataType.TableName == Global) : gameplandata.CustomField.EntityType == Campaign_EntityType) &&
                                                        (gameplandata.GameplanDataType != null ? !gameplandata.GameplanDataType.IsGet : true))
                                                    .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                    .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                lstMappingMisMatch = lstMappingMisMatch.Concat(IdentifyDataTypeMisMatch(_mappingCampaign, lstSalesforceFieldDetail, Enums.EntityType.Campaign.ToString())).ToList();
                
                dataTypeMapping = dataTypeMapping.Where(gp => gp.GameplanDataType != null).Select(gp => gp).ToList();
                
                _mappingImprovementTactic = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType.TableName == Plan_Improvement_Campaign_Program_Tactic
                                                                || (gameplandata.GameplanDataType.TableName == Global && gameplandata.GameplanDataType.IsImprovement == true)) &&
                                                                    !gameplandata.GameplanDataType.IsGet)
                                                .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                                .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                lstMappingMisMatch = lstMappingMisMatch.Concat(IdentifyDataTypeMisMatch(_mappingImprovementTactic, lstSalesforceFieldDetail, Enums.EntityType.ImprovementTactic.ToString())).ToList();



                _mappingImprovementProgram = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType.TableName == Plan_Improvement_Campaign_Program
                                                                || (gameplandata.GameplanDataType.TableName == Global && gameplandata.GameplanDataType.IsImprovement == true)) &&
                                                                    !gameplandata.GameplanDataType.IsGet)
                                               .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                               .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                lstMappingMisMatch = lstMappingMisMatch.Concat(IdentifyDataTypeMisMatch(_mappingImprovementProgram, lstSalesforceFieldDetail, Enums.EntityType.ImprovementProgram.ToString())).ToList();



                _mappingImprovementCampaign = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType.TableName == Plan_Improvement_Campaign
                                                                || (gameplandata.GameplanDataType.TableName == Global && gameplandata.GameplanDataType.IsImprovement == true)) &&
                                                                    !gameplandata.GameplanDataType.IsGet)
                                               .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                               .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                lstMappingMisMatch = lstMappingMisMatch.Concat(IdentifyDataTypeMisMatch(_mappingImprovementCampaign, lstSalesforceFieldDetail, Enums.EntityType.ImprovementCampaign.ToString())).ToList(); 
                #endregion

                #region "Remove mismatch record from  Mapping list"
                //foreach (SalesForceObjectFieldDetails objMisMatchItem in lstMappingMisMatch)
                //{
                //    if (objMisMatchItem.Section.Equals(Enums.EntityType.Tactic.ToString()))
                //    {
                //        _mappingTactic.Remove(objMisMatchItem.SourceField);
                //    }
                //    else if (objMisMatchItem.Section.Equals(Enums.EntityType.Program.ToString()))
                //    {
                //        _mappingProgram.Remove(objMisMatchItem.SourceField);
                //    }
                //    else if (objMisMatchItem.Section.Equals(Enums.EntityType.Campaign.ToString()))
                //    {
                //        _mappingCampaign.Remove(objMisMatchItem.SourceField);
                //    }
                //    else if (objMisMatchItem.Section.Equals(Enums.EntityType.ImprovementTactic.ToString()))
                //    {
                //        _mappingImprovementTactic.Remove(objMisMatchItem.SourceField);
                //    }
                //    else if (objMisMatchItem.Section.Equals(Enums.EntityType.ImprovementProgram.ToString()))
                //    {
                //        _mappingImprovementProgram.Remove(objMisMatchItem.SourceField);
                //    }
                //    else if (objMisMatchItem.Section.Equals(Enums.EntityType.ImprovementCampaign.ToString()))
                //    {
                //        _mappingImprovementCampaign.Remove(objMisMatchItem.SourceField);
                //    }

                //}
                #endregion

                #region "Add data type mismatch message to lstSyncErrror list to display in Summary email"
                string otherDataType = Enums.ActualFieldDatatype[Enums.ActualFields.Other.ToString()].ToString();
                string misMatchfields;
                //int custmId=0;
                //List<int> lstCustomFieldIds=lstMappingMisMatch.Where(map => map.SourceDatatype == otherDataType && int.TryParse(map.SourceField,out custmId)).Select(key=> int.Parse(key.SourceField)).ToList();
                //Dictionary<int,string> tblCustomFields = db.CustomFields.Where(custm => lstCustomFieldIds.Contains(custm.CustomFieldId) && custm.IsDeleted==false).ToDictionary(k=> k.CustomFieldId,v=>v.Name);
                foreach (var Section in lstMappingMisMatch.Select(m => m.Section).Distinct().ToList())
                {
                    misMatchfields =  string.Empty;
                    misMatchfields = string.Join(",", lstMappingMisMatch.Where(m => m.Section == Section).Select(m => m.SourceField).ToList());

                    // check that any customfield mismatch record exist or not.
                    //if (lstMappingMisMatch.Any(map => map.Section == Section && map.SourceDatatype == otherDataType && int.TryParse(map.SourceField, out custmId)))
                    //{
                    //    //Append comma separated customfield title to misMatchfields variable. 
                    //    foreach (SalesForceObjectFieldDetails item in lstMappingMisMatch.Where(map => map.Section == Section && map.SourceDatatype == otherDataType && int.TryParse(map.SourceField, out custmId)))
                    //    {
                    //        strCustomName=string.Empty;
                    //        strCustomName=tblCustomFields.Where(custm=> custm.Key.ToString() == item.SourceField).Select(custm=>custm.Value).FirstOrDefault();
                    //        misMatchfields += "," + strCustomName;
                    //    }
                    //}
                    //misMatchfields = misMatchfields.TrimStart(new char[] { ',' }).TrimEnd(new char[] { ',' });
                    
                    string msg = "Data type mismatch for " + misMatchfields +" in salesforce for " + Section + ".";
                    Enums.EntityType entityTypeSection = (Enums.EntityType)Enum.Parse(typeof(Enums.EntityType), Section, true);

                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, entityTypeSection, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), msg, Enums.SyncStatus.Info, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, msg);
                } 
                #endregion

                #region "Identify that client has Custom name generate permission or not"
                try
                {
                    BDSService.BDSServiceClient objBDSservice = new BDSService.BDSServiceClient();
                    _mappingUser = objBDSservice.GetUserListByClientId(_clientId).Select(u => new { u.UserId, u.FirstName, u.LastName }).ToDictionary(u => u.UserId, u => u.FirstName + " " + u.LastName);

                    if (_CustomNamingPermissionForInstance)
                    {
                        // Get sequence for custom name of tactic
                        SequencialOrderedTableList = db.CampaignNameConventions.Where(c => c.ClientId == _clientId && c.IsDeleted == false).OrderBy(c => c.Sequence).ToList();
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
                }
                catch (Exception ex)
                {
                    string exMessage = Common.GetInnermostException(ex);
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while executing BDS Service:- " + exMessage);
                    _isResultError = true;
                } 
                #endregion
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while getting Mapping list:- " + exMessage);
                _isResultError = true;
            }
            return lstMappingMisMatch;
        } 
        #endregion

    }
}
public class linkedTacticMultipleList
{
    public int LinkedTacticId { get; set; }
    public List<string> lstLinkedPeriods { get; set; }
}
public class TacticLinkedTacMapping
{
    public Plan_Campaign_Program_Tactic PlanTactic { get; set; }
    public Plan_Campaign_Program_Tactic LinkedTactic { get; set; }
    public string PlanName { get; set; }
}
public class SFDCWithMarketoList
{
    public int ModelId { get; set; }
    public int? IntegrationInstanceMarketoID { get; set; }
    public int? IntegrationInstanceId { get; set; }
}
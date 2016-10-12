//// File used to Eloqua Interaction and manipulation of data.

#region Using

using Integration.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Transactions;
using System.Data.Common;
using System.Text.RegularExpressions;

#endregion

namespace Integration.Eloqua
{
    public class IntegrationEloquaClient
    {
        #region Variables

        private MRPEntities db = new MRPEntities();
        static readonly Guid BDSApplicationCode = Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]);

        private RestClient _client;
        public string _username { get; set; }
        public string _password { get; set; }
        public string _companyName { get; set; }
        public string _apiURL { get; set; }
        public string _apiVersion { get; set; }
        private bool _isAuthenticated { get; set; }
        public int _integrationInstanceId { get; set; }
        private int _id { get; set; }
        private int _userId { get; set; }
        private int _integrationInstanceLogId { get; set; }
        private EntityType _entityType { get; set; }
        public string _ErrorMessage { get; set; }
        private bool _isResultError { get; set; }

        private Dictionary<string, string> _mappingTactic { get; set; }
        private Dictionary<string, string> _mappingImprovementTactic { get; set; }
        private Dictionary<int, string> _mappingUser { get; set; }
        private List<CustomFiledMapping> _mappingCustomFields { get; set; }  // Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
        private List<CampaignNameConvention> SequencialOrderedTableList { get; set; }
        private List<string> IntegrationInstanceTacticIds { get; set; }
        private Dictionary<string, string> campaignMetadata { get; set; }
        private Dictionary<string, string> customFields { get; set; }
        private static string NotFound = "NotFound";
        //Start - Added by Mitesh Vaishnav for PL ticket #1002 Custom Naming: Integration
        private int _clientId { get; set; }
        private bool _CustomNamingPermissionForInstance = false;
        private bool IsClientAllowedForCustomNaming = false;
        private Guid _applicationId = Guid.Empty;
        private List<SyncError> _lstSyncError = new List<SyncError>();
        //End - Added by Mitesh Vaishnav for PL ticket #1002 Custom Naming: Integration
        private Dictionary<int, string> _mappingTactic_ActualCost { get; set; }
        private Dictionary<int, int> _mappingPlan_FolderId { get; set; }
        private string titleMappedValue = "name";
        private List<string> statusList { get; set; }
        private string _AccessToken { get; set; }
        public string _eloquaClientID { get; set; }
        public string _ClientSecret { get; set; }
        public DateTime? startDate { get; set; }
        #endregion

        #region Properties

        private int _integrationInstanceSectionId { get; set; }

        public bool IsAuthenticated
        {
            get
            {
                return _isAuthenticated;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Default constructor.
        /// </summary>
        public IntegrationEloquaClient()
        {
            InitEloqua();
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Parameterized constructor.
        /// </summary>
        /// <param name="integrationInstanceId">Integration instance Id.</param>
        /// <param name="id">Entity Id.</param>
        /// <param name="entityType">Entity type.</param>
        /// <param name="userId">User Id.</param>
        /// <param name="integrationInstanceLogId">Integration instance log id.</param>
        public IntegrationEloquaClient(int integrationInstanceId, int id, EntityType entityType, int userId, int integrationInstanceLogId, Guid applicationId)
        {
            InitEloqua();

            _integrationInstanceId = integrationInstanceId;
            _id = id;
            _entityType = entityType;
            _userId = userId;
            _integrationInstanceLogId = integrationInstanceLogId;
            _applicationId = applicationId;

            SetIntegrationInstanceDetail();
            this.Authenticate();
            if (IsAuthenticated)
            {
                customFields = GetCustomFields();
            }
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to initialize metadata of campaign core object.
        /// </summary>
        private void InitEloqua()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            campaignMetadata = new Dictionary<string, string>();
            campaignMetadata.Add("name", "string,text");
            campaignMetadata.Add("description", "string,text");
            campaignMetadata.Add("startAt", "date,datetime");
            campaignMetadata.Add("endAt", "date,datetime");
            campaignMetadata.Add("budgetedCost", "int,double,float,numeric");
            campaignMetadata.Add("currentStatus", "string,text");
            campaignMetadata.Add("actualCost", "int,double,float,numeric");
        }

        /// <summary>
        /// Function to set integration instance detail.
        /// Added By: Maninder Singh Wadhva
        /// </summary>
        private void SetIntegrationInstanceDetail()
        {
            //Modified by Komal Rawal for ticket #1118
            string Companyname = "Company Name";
            IntegrationInstance integrationInstance = db.IntegrationInstances.Where(instance => instance.IntegrationInstanceId == _integrationInstanceId).FirstOrDefault();
            _CustomNamingPermissionForInstance = integrationInstance.CustomNamingPermission;
            Dictionary<string, string> attributeKeyPair = db.IntegrationInstance_Attribute.Where(attribute => attribute.IntegrationInstanceId == _integrationInstanceId).Select(attribute => new { attribute.IntegrationTypeAttribute.Attribute, attribute.Value }).ToDictionary(attribute => attribute.Attribute, attribute => attribute.Value);
            this._companyName = attributeKeyPair[Companyname];
            this._username = integrationInstance.Username;
            this._password = Common.Decrypt(integrationInstance.Password);
            this._apiURL = integrationInstance.IntegrationType.APIURL;
            this._apiVersion = integrationInstance.IntegrationType.APIVersion;
            this._eloquaClientID = attributeKeyPair[Common.eloquaClientIdLabel];
            this._ClientSecret = attributeKeyPair[Common.eloquaClientSecretLabel];
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to get custom fields list.
        /// </summary>
        /// <returns>Returns list of custom fields.</returns>
        public Dictionary<string, string> GetCustomFields()
        {
            Dictionary<string, string> customFields = new Dictionary<string, string>();
            RestRequest request = new RestRequest(Method.GET)
            {
                RequestFormat = DataFormat.Json,
                Resource = string.Format("/assets/campaign/fields?search={0}&depth=complete",
                                  "*")
            };
            request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);

            IRestResponse response = _client.Execute(request);

            JObject data = JObject.Parse(response.Content);
            foreach (var result in data["elements"])
            {
                if (result != null)
                {
                    customFields.Add((string)result["id"], (string)result["name"]);
                }
            }

            return customFields;
        }

        /// <summary>
        /// Added By: Sohel Pathan
        /// Added Date: 22/12/2014
        /// Function to get contact fields list for Eloqua.
        /// </summary>
        /// <returns>Returns list of contact fields.</returns>
        public Dictionary<string, string> GetContactFields()
        {
            Dictionary<string, string> contactFields = new Dictionary<string, string>();
            RestRequest request = new RestRequest(Method.GET)
            {
                RequestFormat = DataFormat.Json,
                Resource = string.Format("/assets/contact/fields?search={0}&depth=complete", "*")
            };
            request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);
            IRestResponse response = _client.Execute(request);

            JObject data = JObject.Parse(response.Content);
            foreach (var result in data["elements"])
            {
                if (result != null)
                {
                    contactFields.Add((string)result["internalName"], (string)result["name"]);
                }
            }

            return contactFields;
        }

        #endregion

        /// <summary>
        /// Function to get target data type of eloqua instance.
        /// </summary>
        /// <returns>Returns list of target data type of eloqua instance.</returns>
        public List<string> GetTargetDataType()
        {
            List<string> targetDataTypeList = customFields.Select(customField => customField.Value).ToList();
            targetDataTypeList.AddRange(campaignMetadata.Select(target => target.Key).ToList());
            return targetDataTypeList.OrderBy(targetDataType => targetDataType).ToList();
        }

        public List<EloquaObjectFieldDetails> GetEloquaFieldDetail()
        {
            var request = new RestRequest(Method.GET) { Resource = "/assets/campaign/fields?depth=complete", RequestFormat = DataFormat.Json, RootElement = "elements" };
            request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);
            var response = _client.Execute<List<CampaignFieldObject>>(request);
            List<EloquaObjectFieldDetails> TargetDataTypeList = new List<EloquaObjectFieldDetails>();
            TargetDataTypeList = response.Data.Select(rs => new EloquaObjectFieldDetails()
                 {
                     TargetField = rs.name,
                     TargetDatatype = rs.dataType,
                 }).ToList();
            EloquaObjectFieldDetails objEloquafield;
            campaignMetadata.ToList().ForEach(fixTarget =>
                                                {
                                                    objEloquafield = new EloquaObjectFieldDetails();
                                                    objEloquafield.TargetField = fixTarget.Key;
                                                    objEloquafield.TargetDatatype = fixTarget.Value;
                                                    TargetDataTypeList.Add(objEloquafield);
                                                });
            return TargetDataTypeList.OrderBy(q => q.TargetField).ToList();

        }

        public class EloquaObjectFieldDetails
        {
            public string Section { get; set; }
            public string TargetField { get; set; }
            public string SourceField { get; set; }
            public string TargetDatatype { get; set; }
            public string SourceDatatype { get; set; }
        }

        public class CampaignFieldObject
        {
            public int id { get; set; } //"1",
            public string name { get; set; } // "Campaign Type",
            public string dataType { get; set; } //"text",
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to sync data from gameplan to eloqua.
        /// </summary>
        /// <returns>returns flag for sync status</returns>
        public bool SyncData(out List<SyncError> lstSyncError)
        {
            lstSyncError = new List<SyncError>();    //// Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {
                // Insert log into IntegrationInstanceSection, Dharmraj PL#684

                _integrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), DateTime.Now, _userId);
                _isResultError = false;
                // TODO: Move this function in internal level so it called if tactic exist
                // Set Client ID based on integration instance.
                _clientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;
                statusList = Common.GetStatusListAfterApproved();
                bool IsInstanceSync = false;
                StringBuilder sb = new StringBuilder();
                if (EntityType.Tactic.Equals(_entityType))
                {
                    List<Plan_Campaign_Program_Tactic> tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && statusList.Contains(tactic.Status) && tactic.IsSyncEloqua.HasValue && tactic.IsSyncEloqua.Value == true).ToList();
                    Plan_Campaign_Program_Tactic planTactic = tblTactic.Where(tactic => tactic.PlanTacticId == _id).FirstOrDefault();
                    if (planTactic != null)
                    {
                        _isResultError = SetMappingDetails();
                        if (!_isResultError)
                        {
                            // Start - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
                            List<int> tacticIdList = new List<int>() { planTactic.PlanTacticId };
                            CreateMappingCustomFieldDictionary(tacticIdList, Enums.EntityType.Tactic.ToString());
                            // End - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997

                            _mappingTactic_ActualCost = Common.CalculateActualCostTacticslist(tacticIdList);
                            List<int> planIdList = new List<int>() { planTactic.Plan_Campaign_Program.Plan_Campaign.PlanId };
                            SetMappingEloquaFolderIdsPlanId(planIdList);


                            #region "Get Linked Tactic"
                            int linkedTacticId = 0;
                            Plan_Campaign_Program_Tactic objLinkedTactic = new Plan_Campaign_Program_Tactic();
                            if (planTactic != null)
                            {
                                linkedTacticId = (planTactic.LinkedTacticId.HasValue) ? planTactic.LinkedTacticId.Value : 0;
                                //if (linkedTacticId <= 0)
                                //{
                                //    objLinkedTactic = tblTactic.Where(tactic => tactic.LinkedTacticId == planTactic.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                                //    linkedTacticId = objLinkedTactic != null ? objLinkedTactic.PlanTacticId : 0;
                                //}
                            }
                            if (linkedTacticId > 0)
                            {
                                objLinkedTactic = tblTactic.Where(tactic => tactic.PlanTacticId == linkedTacticId).FirstOrDefault();
                            }
                            #endregion

                            planTactic = SyncTacticData(planTactic, ref sb, objLinkedTactic);
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sb.ToString());
                            db.SaveChanges();
                        }
                    }
                }
                else if (EntityType.ImprovementTactic.Equals(_entityType))
                {
                    Plan_Improvement_Campaign_Program_Tactic planImprovementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(imptactic => imptactic.ImprovementPlanTacticId == _id && statusList.Contains(imptactic.Status) && imptactic.IsDeployedToIntegration && !imptactic.IsDeleted).FirstOrDefault();
                    if (planImprovementTactic != null)
                    {
                        _isResultError = SetMappingDetails();
                        if (!_isResultError)
                        {
                            List<int> planIdList = new List<int>() { planImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId };
                            SetMappingEloquaFolderIdsPlanId(planIdList);
                            planImprovementTactic = SyncImprovementData(planImprovementTactic, ref sb);
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    IsInstanceSync = true;
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Syncing process with multiple tactic.");
                    SyncInstanceData();
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Syncing process with multiple tactic.");
                }

                if (_isResultError)
                {
                    // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(_integrationInstanceSectionId, StatusResult.Error, _ErrorMessage);
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, _ErrorMessage);
                }
                else
                {
                    // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(_integrationInstanceSectionId, StatusResult.Success, string.Empty);
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Pushing data process completed.");
                }

                ////Added By: Maninder Singh Wadhva
                ////Added Date: 08/20/2014
                ////Ticket #717 	Pulling from Eloqua - Actual Cost 
                if (IsInstanceSync)
                {
                    bool isImport = false;
                    isImport = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId.Equals(_integrationInstanceId)).IsImportActuals;

                    //// Check isimport flag.
                    if (isImport)
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Import actuals is enabled");
                        string strPermissionCode_MQL = Enums.ClientIntegrationPermissionCode.MQL.ToString();
                        int IntegrationTypeId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId.Equals(_integrationInstanceId)).IntegrationTypeId;

                        //// Pull responses from Eloqua
                        GetDataForTacticandUpdate();

                        if (db.Client_Integration_Permission.Any(intPermission => (intPermission.ClientId.Equals(_clientId)) && (intPermission.IntegrationTypeId.Equals(IntegrationTypeId)) && (intPermission.PermissionCode.ToUpper().Equals(strPermissionCode_MQL.ToUpper()))))
                            GetDataPullEloqua();

                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Import actuals is enabled");
                    }
                    else
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Import actuals is not enabled");
                    }
                }

                lstSyncError.AddRange(_lstSyncError);
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while push/pull multiple tactic data: " + exMessage);
            }
            return _isResultError;
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Function to set mapping details.
        /// </summary>
        private bool SetMappingDetails()
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get Mapping detail for Integration Instance");
            // Start - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
            string Global = Enums.IntegrantionDataTypeMappingTableName.Global.ToString();
            string Tactic_EntityType = Enums.EntityType.Tactic.ToString();
            string Plan_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program_Tactic.ToString();
            string Plan_Improvement_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Improvement_Campaign_Program_Tactic.ToString();
            // End - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
            try
            {
                // Start - Modified by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
                List<EloquaObjectFieldDetails> lstMappingMisMatch = new List<EloquaObjectFieldDetails>();
                EloquaObjectFieldDetails objfeildDetails;
                List<EloquaObjectFieldDetails> lstEloquaforceFieldDetail = GetEloquaFieldDetail();

                List<IntegrationInstanceDataTypeMapping> dataTypeMapping = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.IntegrationInstanceId.Equals(_integrationInstanceId)).ToList();
                if (!dataTypeMapping.Any()) // check if there is no field mapping configure then log error to IntegrationInstanceLogDetails table.
                {
                    Enums.EntityType entityType = _entityType.Equals(EntityType.IntegrationInstance) ? Enums.EntityType.Tactic : ((Enums.EntityType)Enum.Parse(typeof(Enums.EntityType), Convert.ToString(_entityType), true));
                    _ErrorMessage = "You have not configure any single field with Eloqua field.";
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, entityType, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), _ErrorMessage, Enums.SyncStatus.Error, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, _ErrorMessage);
                    return true;    // return true value that means error exist and do not proceed for the further mapping list.
                }
                _mappingTactic = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign_Program_Tactic
                                                    || gameplandata.GameplanDataType.TableName == Global) : gameplandata.CustomField.EntityType == Tactic_EntityType) &&
                                                    (gameplandata.GameplanDataType != null ? !gameplandata.GameplanDataType.IsGet : true))
                                                .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                foreach (KeyValuePair<string, string> entry in _mappingTactic)
                {
                    if (Enums.ActualFieldDatatype.ContainsKey(entry.Key) && lstEloquaforceFieldDetail.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault() != null)
                    {
                        if (!Enums.ActualFieldDatatype[entry.Key].Contains(lstEloquaforceFieldDetail.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault().TargetDatatype))
                        {
                            objfeildDetails = new EloquaObjectFieldDetails();
                            objfeildDetails.SourceField = entry.Key;
                            objfeildDetails.TargetField = entry.Value;
                            objfeildDetails.Section = Enums.EntityType.Tactic.ToString();
                            objfeildDetails.TargetDatatype = lstEloquaforceFieldDetail.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault().TargetDatatype;
                            objfeildDetails.SourceDatatype = Enums.ActualFieldDatatype[entry.Key].ToString();
                            lstMappingMisMatch.Add(objfeildDetails);
                        }

                    }
                    else if (lstEloquaforceFieldDetail.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault() != null)
                    {
                        if (!Enums.ActualFieldDatatype[Enums.ActualFields.Other.ToString()].Contains(lstEloquaforceFieldDetail.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault().TargetDatatype))
                        {
                            objfeildDetails = new EloquaObjectFieldDetails();
                            objfeildDetails.SourceField = entry.Key;
                            objfeildDetails.TargetField = entry.Value;
                            objfeildDetails.Section = Enums.EntityType.Tactic.ToString();
                            objfeildDetails.TargetDatatype = lstEloquaforceFieldDetail.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault().TargetDatatype;
                            objfeildDetails.SourceDatatype = Enums.ActualFieldDatatype[entry.Key].ToString();
                            lstMappingMisMatch.Add(objfeildDetails);
                        }
                    }
                }
                // End - Modified by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997

                // Start - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
                dataTypeMapping = dataTypeMapping.Where(gp => gp.GameplanDataType != null).Select(gp => gp).ToList();
                // End - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997

                // Start - Modified by Sohel Pathan on 05/12/2014 for PL ticket #995, 996, & 997
                _mappingImprovementTactic = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType.TableName == Plan_Improvement_Campaign_Program_Tactic
                                                                    || (gameplandata.GameplanDataType.TableName == Global && gameplandata.GameplanDataType.IsImprovement == true)) &&
                                                                       !gameplandata.GameplanDataType.IsGet)
                                                .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                                .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                foreach (KeyValuePair<string, string> entry in _mappingImprovementTactic)
                {
                    if (Enums.ActualFieldDatatype.ContainsKey(entry.Key) && lstEloquaforceFieldDetail.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault() != null)
                    {
                        if (!Enums.ActualFieldDatatype[entry.Key].Contains(lstEloquaforceFieldDetail.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault().TargetDatatype))
                        {
                            objfeildDetails = new EloquaObjectFieldDetails();
                            objfeildDetails.SourceField = entry.Key;
                            objfeildDetails.TargetField = entry.Value;
                            objfeildDetails.Section = Enums.EntityType.ImprovementTactic.ToString();
                            objfeildDetails.TargetDatatype = lstEloquaforceFieldDetail.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault().TargetDatatype;
                            objfeildDetails.SourceDatatype = Enums.ActualFieldDatatype[entry.Key].ToString();
                            lstMappingMisMatch.Add(objfeildDetails);
                        }

                    }
                    else if (lstEloquaforceFieldDetail.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault() != null)
                    {
                        if (!Enums.ActualFieldDatatype[Enums.ActualFields.Other.ToString()].Contains(lstEloquaforceFieldDetail.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault().TargetDatatype))
                        {
                            objfeildDetails = new EloquaObjectFieldDetails();
                            objfeildDetails.SourceField = entry.Key;
                            objfeildDetails.TargetField = entry.Value;
                            objfeildDetails.Section = Enums.EntityType.ImprovementTactic.ToString();
                            objfeildDetails.TargetDatatype = lstEloquaforceFieldDetail.Where(Sfd => Sfd.TargetField == entry.Value).FirstOrDefault().TargetDatatype;
                            objfeildDetails.SourceDatatype = Enums.ActualFieldDatatype[entry.Key].ToString();
                            lstMappingMisMatch.Add(objfeildDetails);
                        }
                    }
                }
                // End - Modified by Sohel Pathan on 05/12/2014 for PL ticket #995, 996, & 997

                #region "Remove mismatch record from  Mapping list"
                foreach (EloquaObjectFieldDetails objMisMatchItem in lstMappingMisMatch)
                {
                    if (objMisMatchItem.Section.Equals(Enums.EntityType.Tactic.ToString()))
                    {
                        _mappingTactic.Remove(objMisMatchItem.SourceField);
                    }
                    else if (objMisMatchItem.Section.Equals(Enums.EntityType.ImprovementTactic.ToString()))
                    {
                        _mappingImprovementTactic.Remove(objMisMatchItem.SourceField);
                    }
                }
                #endregion

                foreach (var Section in lstMappingMisMatch.Select(m => m.Section).Distinct().ToList())
                {
                    string msg = "Data type mismatch for " +
                                string.Join(",", lstMappingMisMatch.Where(m => m.Section == Section).Select(m => m.SourceField).ToList()) +
                                " in Eloqua for " + Section + ".";
                    Enums.EntityType entityTypeSection = (Enums.EntityType)Enum.Parse(typeof(Enums.EntityType), Section, true);
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, entityTypeSection, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), msg, Enums.SyncStatus.Info, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, msg);
                }
                //if (lstMappingMisMatch.Count > 0)
                //{
                //    return true;
                //}

                BDSService.BDSServiceClient objBDSservice = new BDSService.BDSServiceClient();
                _mappingUser = objBDSservice.GetUserListByClientIdEx(_clientId).Select(u => new { u.ID, u.FirstName, u.LastName }).ToDictionary(u => u.ID, u => u.FirstName + " " + u.LastName);

                if (_CustomNamingPermissionForInstance)
                {
                    // Get Sequence of custom name for tactic
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

                IntegrationInstanceTacticIds = new List<string>();
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get Mapping detail for Integration Instance");
                return false;
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                if (ex is System.ServiceModel.EndpointNotFoundException)
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Auth Service Exception: Get Mapping detail for Integration Instance" + exMessage);
                }
                else
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Entity Exception: Get Mapping detail for Integration Instance" + exMessage);
                }
                return true;

            }

        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to Synchronize instance data.
        /// </summary>
        private void SyncInstanceData()
        {
            int logRecordSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LogRecordSize"]);
            int pushRecordBatchSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IntegrationPushRecordBatchSize"]);
            string published = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString();
            StringBuilder sbMessage;
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            List<int> planIds = db.Plans.Where(p => p.Model.IntegrationInstanceEloquaId == _integrationInstanceId && p.Model.Status.Equals(published)).Select(p => p.PlanId).ToList();
            SetMappingEloquaFolderIdsPlanId(planIds);
            try
            {
                int page = 0;
                int total = 0;
                //int pageSize = 10;
                int maxpage = 0;
                List<Plan_Campaign_Program_Tactic> tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted && tactic.IsSyncEloqua.HasValue && tactic.IsSyncEloqua.Value == true).ToList();
                List<Plan_Campaign_Program_Tactic> tacticList = tblTactic.Where(tactic => planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).ToList();
                List<Plan_Improvement_Campaign_Program_Tactic> IMPtacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(tactic => planIds.Contains(tactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId) && statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted).ToList();
                if (tacticList.Count > 0 || IMPtacticList.Count > 0)
                {
                    _isResultError = SetMappingDetails();
                    if (_isResultError)
                    {
                        return;
                    }
                }

                if (tacticList.Count > 0)
                {
                    try
                    {
                        // Start - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
                        List<int> tacticIdList = tacticList.Select(c => c.PlanTacticId).ToList();
                        _mappingTactic_ActualCost = Common.CalculateActualCostTacticslist(tacticIdList);
                        CreateMappingCustomFieldDictionary(tacticIdList, Enums.EntityType.Tactic.ToString());
                        // End - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
                        page = 0;
                        total = tacticList.Count;
                        maxpage = (total / pushRecordBatchSize);
                        List<Plan_Campaign_Program_Tactic> lstPagedlistTactic = new List<Plan_Campaign_Program_Tactic>();
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, "Total No. of Tactics: " + total);
                        Plan_Campaign_Program_Tactic srcTactic, objLinkedTactic;
                        while (page <= maxpage)
                        {
                            lstPagedlistTactic = new List<Plan_Campaign_Program_Tactic>();
                            lstPagedlistTactic = tacticList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList();

                            sbMessage = new StringBuilder();

                            for (int index = 0; index < lstPagedlistTactic.Count; index++)
                            {

                                #region "Get Linked Tactic"
                                int linkedTacticId = 0;
                                srcTactic = new Plan_Campaign_Program_Tactic();
                                objLinkedTactic = new Plan_Campaign_Program_Tactic();
                                srcTactic = lstPagedlistTactic[index];
                                if (srcTactic != null)
                                {
                                    linkedTacticId = (srcTactic != null && srcTactic.LinkedTacticId.HasValue) ? srcTactic.LinkedTacticId.Value : 0;
                                    //if (linkedTacticId <= 0)
                                    //{
                                    //    objLinkedTactic = tblTactic.Where(tactic => tactic.LinkedTacticId == srcTactic.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                                    //    linkedTacticId = objLinkedTactic != null ? objLinkedTactic.PlanTacticId : 0;
                                    //}
                                }
                                if (linkedTacticId > 0)
                                {
                                    objLinkedTactic = tblTactic.Where(tactic => tactic.PlanTacticId == linkedTacticId).FirstOrDefault();
                                }
                                #endregion

                                lstPagedlistTactic[index] = SyncTacticData(lstPagedlistTactic[index], ref sbMessage, objLinkedTactic);

                                // Save 10 log records to Table.
                                if (((index + 1) % logRecordSize) == 0)
                                {
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                                    sbMessage = new StringBuilder();
                                }
                            }
                            if (!string.IsNullOrEmpty(sbMessage.ToString()))
                            {
                                // Save remaining log records to Table.
                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                            }
                            db.SaveChanges();
                            page++;
                        }

                    }
                    catch (Exception ex)
                    {
                        _isResultError = true;
                        string exMessage = Common.GetInnermostException(ex);
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing Tactic data to Eloqua: " + exMessage);
                    }
                }

                if (IMPtacticList.Count > 0)
                {
                    try
                    {
                        page = 0;
                        total = IMPtacticList.Count;
                        maxpage = (total / pushRecordBatchSize);
                        List<Plan_Improvement_Campaign_Program_Tactic> lstPagedlistIMPTactic = new List<Plan_Improvement_Campaign_Program_Tactic>();
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, "Total No. of ImprovementTactics: " + total);
                        while (page <= maxpage)
                        {
                            lstPagedlistIMPTactic = new List<Plan_Improvement_Campaign_Program_Tactic>();
                            lstPagedlistIMPTactic = IMPtacticList.Skip(page * pushRecordBatchSize).Take(pushRecordBatchSize).ToList();

                            sbMessage = new StringBuilder();

                            for (int index = 0; index < lstPagedlistIMPTactic.Count; index++)
                            {
                                lstPagedlistIMPTactic[index] = SyncImprovementData(lstPagedlistIMPTactic[index], ref sbMessage);

                                // Save 10 log records to Table.
                                if (((index + 1) % logRecordSize) == 0)
                                {
                                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                                    sbMessage = new StringBuilder();
                                }
                            }
                            if (!string.IsNullOrEmpty(sbMessage.ToString()))
                            {
                                // Save remaining log records to Table.
                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.None, sbMessage.ToString());
                            }
                            db.SaveChanges();
                            page++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _isResultError = true;
                        string exMessage = Common.GetInnermostException(ex);
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while pushing ImprovementTactic data to Eloqua: " + exMessage);
                    }
                }
            }
            catch (Exception e)
            {
                string exMessage = Common.GetInnermostException(e);
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "System error occurred while syncing data with Eloqua" + exMessage);
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "System error occurred while syncing data with Eloqua.", Enums.SyncStatus.Error, DateTime.Now));
                _ErrorMessage = exMessage;//GetErrorMessage(e);
                _isResultError = true;
            }
        }

        // Get All responses from integration instance external server
        private void GetDataForTacticandUpdate()
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                Common.SaveIntegrationInstanceLogDetails(_integrationInstanceId, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "pulling response from Eloqua");
                EloquaResponse objEloquaResponse = new EloquaResponse();
                List<SyncError> lstSyncError = new List<SyncError>();
                bool isError = objEloquaResponse.GetTacticResponse(_integrationInstanceId, _userId, _integrationInstanceLogId, _applicationId, out lstSyncError);
                _lstSyncError.AddRange(lstSyncError);
                if (isError)
                {
                    _isResultError = true;
                }
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_integrationInstanceId, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "System error occurred while pulling response from Eloqua. Exception :" + exMessage);
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "System error occurred while pulling response from Eloqua. ", Enums.SyncStatus.Error, DateTime.Now));
                _ErrorMessage = exMessage;
                _isResultError = true;
            }
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to create eloqua campaign.
        /// </summary>
        /// <param name="tactic">Plan tactic or improvement tactic.</param>
        /// <returns>Returns id of campaign create on eloqua.</returns>
        private string CreateEloquaCampaign(IDictionary<string, object> tactic)
        {
            RestRequest request = new RestRequest(Method.POST)
            {
                Resource = "/assets/campaign",
                RequestFormat = DataFormat.Json
            };
            request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);
            if (tactic.ContainsKey(titleMappedValue))
            {
                tactic[titleMappedValue] = Common.TruncateName(tactic[titleMappedValue].ToString());
            }

            request.AddBody(tactic);
            IRestResponse<EloquaCampaign> response = _client.Execute<EloquaCampaign>(request);

            string tactidId = string.Empty;
            if (response != null && response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == HttpStatusCode.Created)
            {
                tactidId = response.Data.id;
                int entityid = EntityType.Tactic.Equals(_entityType) || EntityType.ImprovementTactic.Equals(_entityType) ? _id : Convert.ToInt32(tactidId); // if single Tactic syncing process call then Insert PlanTacticId to Log list and if Instance syncing process call then insert EloquaId to log list. 
                _lstSyncError.Add(Common.PrepareSyncErrorList(entityid, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Create.ToString(), Enums.SyncStatus.Success, DateTime.Now));
            }
            else
            {
                //// Start - Added by Sohel Pathan on 05/01/2015 for PL ticket #1068
                //if (tactic.ContainsKey("Title"))
                //{
                //    _lstSyncError.Add(Common.PrepareSyncErrorList(int.Parse(tactic["id"].ToString()), Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "Eloqua exception occurred while creating campaign at Eloqua for tactic : \"" + tactic["Title"] + "\"", Enums.SyncStatus.Error, DateTime.Now));
                //}
                //else
                //{
                //    _lstSyncError.Add(Common.PrepareSyncErrorList(int.Parse(tactic["id"].ToString()), Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "Eloqua exception occurred while creating campaign at Eloqua for a tactic.", Enums.SyncStatus.Error, DateTime.Now));
                //}
                //// End - Added by Sohel Pathan on 05/01/2015 for PL ticket #1068
                throw new Exception(response.StatusCode.ToString(), response.ErrorException);
            }

            return tactidId;
        }

        /// <summary>
        /// Function to get Eloqua Campaign.
        /// Added By: Maninder Singh
        /// Added Date: 08/20/2014
        /// Ticket #717 Pulling from Eloqua - Actual Cost 
        /// </summary>
        /// <param name="eloquaCampaignId">Eloqua campaign Id.</param>
        /// <returns>Returns eloqua campaign object.</returns>
        public EloquaCampaign GetEloquaCampaign(string elouqaCampaignId)
        {
            RestRequest request = new RestRequest(Method.GET)
            {
                Resource = "/assets/campaign/" + elouqaCampaignId,
                RequestFormat = DataFormat.Json
            };
            request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);
            IRestResponse<EloquaCampaign> response = _client.Execute<EloquaCampaign>(request);
            return response.Data;
        }


        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to update eloqua campaign.
        /// </summary>
        /// <param name="id">Integration instance id.</param>
        /// <param name="tactic">Plan tactic or improvement tactic.</param>
        /// <returns>Returns flag to determine whether update was successfull or not.</returns>
        private bool UpdateEloquaCampaign(string id, IDictionary<string, object> tactic)
        {
            IntegrationInstanceTacticIds.Add(Convert.ToString(id));
            RestRequest request = new RestRequest(Method.PUT)
            {
                Resource = string.Format("/assets/campaign/{0}", id),
                RequestFormat = DataFormat.Json
            };
            request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);
            tactic.Remove("folderId");
            if (tactic.ContainsKey(titleMappedValue))
            {
                tactic[titleMappedValue] = Common.TruncateName(tactic[titleMappedValue].ToString());
            }

            request.AddBody(tactic);

            IRestResponse<EloquaCampaign> response = _client.Execute<EloquaCampaign>(request);

            if (response.Data != null)
            {
                int entityid = EntityType.Tactic.Equals(_entityType) || EntityType.ImprovementTactic.Equals(_entityType) ? _id : Convert.ToInt32(id); // if single Tactic syncing process call then Insert PlanTacticId to Log list and if Instance syncing process call then insert EloquaId to log list. 
                _lstSyncError.Add(Common.PrepareSyncErrorList(entityid, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Update.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                return response.Data.id.Equals(id);
            }
            else
            {
                //// Start - Added by Sohel Pathan on 05/01/2015 for PL ticket #1068
                //if (tactic.ContainsKey("Title"))
                //{
                //    _lstSyncError.Add(Common.PrepareSyncErrorList(int.Parse(tactic["id"].ToString()), Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "Eloqua exception occurred while updating campaign at Eloqua for tactic: \"" + tactic["Title"] + "\"", Enums.SyncStatus.Error, DateTime.Now));
                //}
                //else
                //{
                //    _lstSyncError.Add(Common.PrepareSyncErrorList(int.Parse(tactic["id"].ToString()), Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "Eloqua exception occurred while updating campaign at Eloqua for a tactic.", Enums.SyncStatus.Error, DateTime.Now));
                //}
                //// End - Added by Sohel Pathan on 05/01/2015 for PL ticket #1068
                throw new Exception(string.Format("[{0}] [{1}]", response.StatusCode.ToString(), response.StatusDescription), response.ErrorException);
            }
        }

        #region Improvement Tactic

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to synchronize improvement tactic.
        /// </summary>
        /// <param name="planIMPTactic">Improvement tactic.</param>
        /// <returns>Returns updated improvement tactic.</returns>
        private Plan_Improvement_Campaign_Program_Tactic SyncImprovementData(Plan_Improvement_Campaign_Program_Tactic planIMPTactic, ref StringBuilder sbMessage)
        {
            StringBuilder sb = new StringBuilder();
            Enums.Mode currentMode = Common.GetMode(planIMPTactic.IntegrationInstanceEloquaId);
            int _ImprvmntTacticFolderId = 0;
            _ImprvmntTacticFolderId = GetEloquaFolderIdByPlanId(planIMPTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId);


            if (currentMode.Equals(Enums.Mode.Create))
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
                    planIMPTactic.IntegrationInstanceEloquaId = CreateImprovementTactic(planIMPTactic, _ImprvmntTacticFolderId);
                    planIMPTactic.LastSyncDate = DateTime.Now;
                    planIMPTactic.ModifiedDate = DateTime.Now;
                    planIMPTactic.ModifiedBy = _userId;
                    instanceLogTactic.Status = StatusResult.Success.ToString();

                    #region "Add ImprovementTactic synced comment to Plan_Improvement_Campaign_Program_Tactic_Comment table"
                    //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                    if (!Common.IsAutoSync)
                    {
                        //Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                        Plan_Improvement_Campaign_Program_Tactic_Comment objImpTacticComment = new Plan_Improvement_Campaign_Program_Tactic_Comment();
                        objImpTacticComment.ImprovementPlanTacticId = planIMPTactic.ImprovementPlanTacticId;
                        objImpTacticComment.Comment = Common.ImprovementTacticSyncedComment + Integration.Helper.Enums.IntegrationType.Eloqua.ToString();
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
                    #endregion
                    //End : Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                    sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Success.ToString() + ")");
                }
                catch (Exception)
                {
                    string message = "System error occurred while creating improvement tactic \"" + planIMPTactic.Title + "\" at Eloqua.";
                    sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + ")");
                    _lstSyncError.Add(Common.PrepareSyncErrorList(planIMPTactic.ImprovementPlanTacticId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), message, Enums.SyncStatus.Error, DateTime.Now));
                    _ErrorMessage = Common.msgChildLevelError.ToString();
                    instanceLogTactic.Status = StatusResult.Error.ToString();
                    instanceLogTactic.ErrorDescription = message;//Common.GetInnermostException(e);//GetErrorMessage(e);
                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;

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
                    if (UpdateImprovementTactic(planIMPTactic, 0))
                    {
                        planIMPTactic.LastSyncDate = DateTime.Now;
                        planIMPTactic.ModifiedDate = DateTime.Now;
                        planIMPTactic.ModifiedBy = _userId;

                        #region "Add ImprovementTactic synced comment to Plan_Improvement_Campaign_Program_Tactic_Comment table"
                        //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                        if (!Common.IsAutoSync)
                        {
                            //Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                            Plan_Improvement_Campaign_Program_Tactic_Comment objImpTacticComment = new Plan_Improvement_Campaign_Program_Tactic_Comment();
                            objImpTacticComment.ImprovementPlanTacticId = planIMPTactic.ImprovementPlanTacticId;
                            objImpTacticComment.Comment = Common.ImprovementTacticUpdatedComment + Integration.Helper.Enums.IntegrationType.Eloqua.ToString();
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
                        #endregion

                        instanceLogTactic.Status = StatusResult.Success.ToString();
                        sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Success.ToString() + ")");
                    }
                    else
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = Common.UnableToUpdate;
                        sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + ")");
                    }
                }
                catch (Exception e)
                {
                    _ErrorMessage = Common.msgChildLevelError.ToString();
                    if (e.Message.Contains(NotFound))// || e.Message.Contains(InternalServerError))
                    {
                        //planIMPTactic = SyncImprovementData(planIMPTactic);
                        sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Success.ToString() + ")");
                        return planIMPTactic;
                    }
                    else
                    {
                        string message = "System error occurred while updating improvement Tactic \"" + planIMPTactic.Title + "\" at Eloqua.";
                        sb.Append("ImprovementTactic: " + planIMPTactic.ImprovementPlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + ")");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(planIMPTactic.ImprovementPlanTacticId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), message, Enums.SyncStatus.Error, DateTime.Now));
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = message;
                    }
                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }

            sbMessage.Append(sb.ToString());
            return planIMPTactic;
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to create improvement tactic.
        /// </summary>
        /// <param name="planIMPTactic">Improvement tactic.</param>
        /// <returns>Returns id of improvement tactic created on eloqua.</returns>
        private string CreateImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic, int folderId)
        {
            IDictionary<string, object> tactic = GetImprovementTactic(planIMPTactic, Enums.Mode.Create, folderId);

            if (_mappingTactic.ContainsKey("Title"))
            {
                titleMappedValue = _mappingTactic["Title"].ToString();
            }

            return CreateEloquaCampaign(tactic);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to update improvement tactictactic on eloqua.
        /// </summary>
        /// <param name="planIMPTactic">Improvement tactic.</param>
        /// <returns>Returns flag to determine whether udpate was successfull or not.</returns>
        private bool UpdateImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic, int folderId)
        {
            IDictionary<string, object> tactic = GetImprovementTactic(planIMPTactic, Enums.Mode.Update, folderId);

            if (_mappingTactic.ContainsKey("Title"))
            {
                titleMappedValue = _mappingTactic["Title"].ToString();
            }

            return UpdateEloquaCampaign(planIMPTactic.IntegrationInstanceEloquaId, tactic);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to get improvement tactic.
        /// </summary>
        /// <param name="planIMPTactic">Improvement tactic.</param>
        /// <param name="mode">Mode of operations.</param>
        /// <returns>Returns improvement tactic.</returns>
        private IDictionary<string, object> GetImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic, Enums.Mode mode, int folderId)
        {
            IDictionary<string, object> tactic = GetTargetKeyValue<Plan_Improvement_Campaign_Program_Tactic>(planIMPTactic, _mappingImprovementTactic);
            tactic.Add("type", "Campaign");
            tactic.Add("id", planIMPTactic.IntegrationInstanceEloquaId);

            if (folderId > 0)
            {
                tactic.Add("folderId", folderId);
            }

            return tactic;
        }

        #endregion

        #region Tactic

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to get plan tactic.
        /// </summary>
        /// <param name="planTactic">Plan tactic.</param>
        /// <param name="mode">Mode of operation.</param>
        /// <returns>Returns plan tactic.</returns>
        private IDictionary<string, object> GetTactic(Plan_Campaign_Program_Tactic planTactic, Enums.Mode mode, int folderId)
        {
            IDictionary<string, object> tactic = GetTargetKeyValue<Plan_Campaign_Program_Tactic>(planTactic, _mappingTactic);
            tactic.Add("type", "Campaign");
            tactic.Add("id", planTactic.IntegrationInstanceEloquaId);

            if (folderId > 0)
            {
                tactic.Add("folderId", folderId);
            }

            return tactic;
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to update plan tactic on eloqua.
        /// </summary>
        /// <param name="planTactic">Plan tactic.</param>
        /// <returns>Returns flag to determine whether udpate was successfull or not.</returns>
        private bool UpdateTactic(Plan_Campaign_Program_Tactic planTactic, int folderId)
        {
            IDictionary<string, object> tactic = GetTactic(planTactic, Enums.Mode.Update, folderId);
            if (!string.IsNullOrEmpty(planTactic.TacticCustomName) && _mappingTactic.ContainsKey("Title"))
            {
                titleMappedValue = _mappingTactic["Title"].ToString();

                if (tactic.ContainsKey(titleMappedValue))
                {
                    tactic[titleMappedValue] = planTactic.TacticCustomName;
                }
            }
            return UpdateEloquaCampaign(planTactic.IntegrationInstanceEloquaId, tactic);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to synchronize tactic data.
        /// </summary>
        /// <param name="planTactic">Plan tactic.</param>
        /// <returns>Returns updated plan tactic.</returns>
        private Plan_Campaign_Program_Tactic SyncTacticData(Plan_Campaign_Program_Tactic planTactic, ref StringBuilder sbMessage, Plan_Campaign_Program_Tactic lnkdTactic)
        {
            StringBuilder sb = new StringBuilder();
            //this will be replaced while optimization
            Enums.Mode currentMode = Common.GetMode(planTactic.IntegrationInstanceEloquaId);
            int _tacFolderId = 0;
            _tacFolderId = GetEloquaFolderIdByPlanId(planTactic.Plan_Campaign_Program.Plan_Campaign.PlanId);

            #region "Get Original Tactic StartDate"
            //#2097: If Tactic is linked then sync origional year tactic's startdate to Salesforce.
            if (lnkdTactic != null && lnkdTactic.PlanTacticId > 0)
            {
                string orgnPlanYear = planTactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year;
                string lnkdPlanYear = lnkdTactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year;
                if (!string.IsNullOrEmpty(orgnPlanYear) && !string.IsNullOrEmpty(lnkdPlanYear))
                {
                    // if linkedTactic is orgional tactic then set it's startdate to global variable.
                    if (int.Parse(orgnPlanYear) > int.Parse(lnkdPlanYear))
                    {
                        startDate = lnkdTactic.StartDate;
                    }
                }
            }
            #endregion

            if (currentMode.Equals(Enums.Mode.Create))
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
                    planTactic.IntegrationInstanceEloquaId = CreateTactic(planTactic, _tacFolderId);
                    planTactic.LastSyncDate = DateTime.Now;
                    planTactic.ModifiedDate = DateTime.Now;
                    planTactic.ModifiedBy = _userId;
                    instanceLogTactic.Status = StatusResult.Success.ToString();
                    Plan_Campaign_Program_Tactic_Comment objTacticComment = new Plan_Campaign_Program_Tactic_Comment();

                    #region "Add synced Tactic comment to Plan_Campaign_Program_Tactic_Comment table"
                    //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                    if (!Common.IsAutoSync)
                    {
                        //Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                        objTacticComment.PlanTacticId = planTactic.PlanTacticId;
                        objTacticComment.Comment = Common.TacticSyncedComment + Integration.Helper.Enums.IntegrationType.Eloqua.ToString();
                        objTacticComment.CreatedDate = DateTime.Now;
                        ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                        //if (Common.IsAutoSync)
                        //{
                        //    objTacticComment.CreatedBy = new Guid();
                        //}
                        //else
                        //{
                        objTacticComment.CreatedBy = this._userId;
                        //}

                        db.Entry(objTacticComment).State = EntityState.Added;
                        db.Plan_Campaign_Program_Tactic_Comment.Add(objTacticComment);
                    }
                    // End Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic 
                    #endregion

                    #region "Update Linked Tactic IntegrationInstanceTacticId & Tactic Comment Table"
                    if (lnkdTactic != null && lnkdTactic.PlanTacticId > 0)
                    {
                        lnkdTactic.IntegrationInstanceEloquaId = planTactic.IntegrationInstanceEloquaId;
                        lnkdTactic.TacticCustomName = planTactic.TacticCustomName;
                        lnkdTactic.LastSyncDate = DateTime.Now;
                        lnkdTactic.ModifiedDate = DateTime.Now;
                        lnkdTactic.ModifiedBy = _userId;

                        if (!Common.IsAutoSync)
                        {
                            // Add linked tactic comment
                            Plan_Campaign_Program_Tactic_Comment objLinkedTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                            objLinkedTacticComment.PlanTacticId = lnkdTactic.PlanTacticId;
                            objLinkedTacticComment.Comment = objTacticComment.Comment;
                            objLinkedTacticComment.CreatedDate = DateTime.Now;
                            objLinkedTacticComment.CreatedBy = objTacticComment.CreatedBy;
                            db.Entry(objLinkedTacticComment).State = EntityState.Added;
                            db.Plan_Campaign_Program_Tactic_Comment.Add(objLinkedTacticComment);
                        }
                    }
                    #endregion
                    sb.Append("Tactic: " + planTactic.PlanTacticId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                }
                catch (Exception)
                {
                    _isResultError = true;
                    string message = "System error occurred while creating tactic \"" + planTactic.Title + "\" at Eloqua.";
                    //// Start - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                    _lstSyncError.Add(Common.PrepareSyncErrorList(planTactic.PlanTacticId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), message, Enums.SyncStatus.Error, DateTime.Now));
                    _ErrorMessage = Common.msgChildLevelError.ToString();
                    //// End - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                    instanceLogTactic.Status = StatusResult.Error.ToString();
                    instanceLogTactic.ErrorDescription = message;//Common.GetInnermostException(e);
                    sb.Append("Tactic: " + planTactic.PlanTacticId.ToString() + "(" + Operation.Create.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
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
                    if (UpdateTactic(planTactic, 0))
                    {
                        planTactic.LastSyncDate = DateTime.Now;
                        planTactic.ModifiedDate = DateTime.Now;
                        planTactic.ModifiedBy = _userId;

                        #region "Add update Tactic comment to Plan_Campaign_Program_Tactic_Comment table"
                        Plan_Campaign_Program_Tactic_Comment objTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                        //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                        if (!Common.IsAutoSync)
                        {
                            //Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                            objTacticComment.PlanTacticId = planTactic.PlanTacticId;
                            objTacticComment.Comment = Common.TacticUpdatedComment + Integration.Helper.Enums.IntegrationType.Eloqua.ToString();
                            objTacticComment.CreatedDate = DateTime.Now;
                            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
                            //if (Common.IsAutoSync)
                            //{
                            //    objTacticComment.CreatedBy = new Guid();
                            //}
                            //else
                            //{
                            objTacticComment.CreatedBy = this._userId;
                            //}

                            db.Entry(objTacticComment).State = EntityState.Added;
                            db.Plan_Campaign_Program_Tactic_Comment.Add(objTacticComment);
                            // End Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic 
                        }
                        #endregion

                        #region "Update Linked Tactic IntegrationInstanceTacticId & Tactic Comment Table"
                        if (lnkdTactic != null && lnkdTactic.PlanTacticId > 0)
                        {
                            lnkdTactic.TacticCustomName = planTactic.TacticCustomName;
                            lnkdTactic.LastSyncDate = DateTime.Now;
                            lnkdTactic.ModifiedDate = DateTime.Now;
                            lnkdTactic.ModifiedBy = _userId;
                            if (!Common.IsAutoSync)
                            {
                                // Add linked tactic comment
                                Plan_Campaign_Program_Tactic_Comment objLinkedTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                                objLinkedTacticComment.PlanTacticId = lnkdTactic.PlanTacticId;
                                objLinkedTacticComment.Comment = objTacticComment.Comment;
                                objLinkedTacticComment.CreatedDate = DateTime.Now;
                                objLinkedTacticComment.CreatedBy = objTacticComment.CreatedBy;
                                db.Entry(objLinkedTacticComment).State = EntityState.Added;
                                db.Plan_Campaign_Program_Tactic_Comment.Add(objLinkedTacticComment);
                            }
                        }
                        #endregion
                        instanceLogTactic.Status = StatusResult.Success.ToString();
                        sb.Append("Tactic: " + planTactic.PlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                    }
                    else
                    {
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = Common.UnableToUpdate;
                        sb.Append("Tactic: " + planTactic.PlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                    }
                }
                catch (Exception e)
                {
                    //// Start - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                    //// End - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                    _ErrorMessage = Common.msgChildLevelError.ToString();

                    if (e.Message.Contains(NotFound))
                    {
                        sb.Append("Tactic: " + planTactic.PlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Success.ToString() + "); ");
                        return planTactic;
                    }
                    else
                    {
                        _isResultError = true;
                        string message = "System error occurred while updating tactic \"" + planTactic.Title + "\" at Eloqua.";
                        _lstSyncError.Add(Common.PrepareSyncErrorList(planTactic.PlanTacticId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), message, Enums.SyncStatus.Error, DateTime.Now));
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = message; //Common.GetInnermostException(e);
                        sb.Append("Tactic: " + planTactic.PlanTacticId.ToString() + "(" + Operation.Update.ToString() + ", " + StatusResult.Error.ToString() + "); ");
                    }

                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }
            startDate = null; //Reset startDate global variable to null.
            sbMessage.Append(sb.ToString());
            return planTactic;
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to create plan tactic.
        /// </summary>
        /// <param name="planTactic">Plan tactic.</param>
        /// <returns>Returns id of tactic created on eloqua.</returns>
        private string CreateTactic(Plan_Campaign_Program_Tactic planTactic, int folderId)
        {
            IDictionary<string, object> tactic = GetTactic(planTactic, Enums.Mode.Create, folderId);
            if (_mappingTactic.ContainsKey("Title") && planTactic != null && _CustomNamingPermissionForInstance && IsClientAllowedForCustomNaming)//_clientActivityList.Where(clientActivity=>clientActivity.Code==Enums.clientAcivity.CustomCampaignNameConvention.ToString()).Any() && 
            {
                titleMappedValue = _mappingTactic["Title"].ToString();
                if (tactic.ContainsKey(titleMappedValue))
                {
                    tactic[titleMappedValue] = (planTactic.TacticCustomName == null) ? (Common.GenerateCustomName(planTactic, SequencialOrderedTableList, _mappingCustomFields)) : (planTactic.TacticCustomName);
                    planTactic.TacticCustomName = tactic[titleMappedValue].ToString();
                }
            }
            return CreateEloquaCampaign(tactic);
        }

        #endregion

        #region Eloqua Folder Search and Set

        /// <summary>
        /// Class to store folder search response.
        /// </summary>
        public class folderResponse
        {
            public List<elements> elements { get; set; }
            public string page { get; set; }
            public string pageSize { get; set; }
            public string total { get; set; }
        }

        /// <summary>
        /// Class to store folder search response element(s).
        /// </summary>
        public class elements
        {
            public string type { get; set; }
            public string id { get; set; }
            public string createdAt { get; set; }
            public string createdBy { get; set; }
            public string depth { get; set; }
            public string folderId { get; set; }
            public string name { get; set; }
            public string updatedAt { get; set; }
            public string updatedBy { get; set; }
            public string isSystem { get; set; }
            public string description { get; set; }
        }

        /// <summary>
        /// Search for folder Id to upload tactic data in Eloqua.
        /// </summary>
        /// <param name="planTactic"> Tactic obj.</param>
        /// <returns> Return folder Id.</returns>
        public void SetMappingEloquaFolderIdsPlanId(List<int> PlanIds)
        {
            int folderId = 0;
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Mapping Eloqua Folder Ids");
            //// Get Plan Id for selected Tactic.
            var tblplan = db.Plans.Where(pc => PlanIds.Contains(pc.PlanId) && pc.IsDeleted.Equals(false)).Select(pc => new { PlanId = pc.PlanId, Title = pc.Title, FolderPath = pc.EloquaFolderPath }).ToList();
            Dictionary<int, int> dictPlanFolderId = new Dictionary<int, int>();
            foreach (var _plan in tblplan)
            {
                //// Get Folder Path for selected plan.
                string folderPath = _plan.FolderPath;

                //// Check weather folder path exist or not.
                if (folderPath != null)
                {
                    //// Remove first occurrence of "/" from folder path if exist.
                    folderPath = (folderPath[0].ToString() == "/") ? folderPath.Remove(0, 1) : folderPath;

                    //// Remove last occurrence of "/" from folder path if exist.
                    folderPath = (folderPath[folderPath.Length - 1].ToString() == "/") ? folderPath.Remove(folderPath.Length - 1, 1) : folderPath;

                    //// Convert folder path into String array.
                    string[] folderPathArray = folderPath.Split('/');

                    //// Call function to search folder name from Eloqua.
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Search folder in eloqua");
                    IRestResponse response = SearchFolderInEloqua(folderPathArray.Last());
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Search folder in eloqua");

                    //// Convert Json response into class.
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Deserialize eloqa folder search response");
                    folderResponse respo = JsonConvert.DeserializeObject<folderResponse>(response.Content);

                    //// If response is null skip.
                    if (respo != null)
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Deserialize eloqa folder search response");
                        if (respo.elements.Count > 1)
                        {
                            //// If folder search result get more than one result.

                            //// Search for all folder.
                            response = SearchFolderInEloqua("*");

                            //// Convert Json response into class.
                            respo = JsonConvert.DeserializeObject<folderResponse>(response.Content);

                            //// Get Id of folder from Eloqua for root folder of folder path defined in plan.
                            var parentId = respo.elements.Where(p => p.name.Equals(folderPathArray.First())).Select(p => new { p.id }).FirstOrDefault();

                            string parentFolderId = Convert.ToString(parentId.id); ;

                            //// Iterate folder path array and find folder.
                            for (int i = 1; i < folderPathArray.Length; i++)
                            {
                                //// Get list of folder which contain parent Id.
                                var list = respo.elements.Where(p => p.folderId == parentFolderId && p.folderId != null).ToList().Select(p => new { p.name, p.id, p.folderId }).ToList();

                                //// Iterate list and get folder id as parent id.
                                foreach (var item in list)
                                {
                                    if (item.name.ToString() == folderPathArray.ElementAt(i))
                                    {
                                        parentFolderId = item.id;
                                    }
                                }
                            }

                            //// Check weather selected folder is correct folder or not.
                            parentFolderId = respo.elements.Where(p => p.id == parentFolderId && p.name == folderPathArray.Last()).Select(p => p.id).FirstOrDefault();

                            if (parentFolderId != null)
                            {
                                folderId = Convert.ToInt32(parentFolderId);
                            }
                            else
                            {
                                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, parentFolderId + " folder does not exists for plan \"" + _plan.Title + "\".");
                                //// Start - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                                _lstSyncError.Add(Common.PrepareSyncErrorList(_plan.PlanId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), parentFolderId + " folder does not exists for plan \"" + _plan.Title + "\".", Enums.SyncStatus.Info, DateTime.Now));
                                //// End - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                                folderId = 0;
                            }
                        }
                        else if (respo.elements.Count > 0)
                        {
                            //// If folder search result get only one result.
                            int _folderId;
                            int.TryParse(respo.elements.FirstOrDefault().id, out _folderId);
                            folderId = _folderId;
                        }
                        else
                        {
                            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Folder path has not been specified for plan \"" + _plan.Title);
                            //// Start - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                            _lstSyncError.Add(Common.PrepareSyncErrorList(_plan.PlanId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "Folder path has not been specified for plan \"" + _plan.Title + "\".", Enums.SyncStatus.Info, DateTime.Now));
                            //// End - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068

                            //// default value return.
                            folderId = 0;
                        }
                    }
                    else
                    {
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Deserialize eloqa folder search response");
                    }
                }
                else
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Folder path has not been specified for plan \"" + _plan.Title);
                    //// Start - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                    _lstSyncError.Add(Common.PrepareSyncErrorList(_plan.PlanId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "Folder path has not been specified for plan \"" + _plan.Title + "\".", Enums.SyncStatus.Info, DateTime.Now));
                    //// End - Added by Sohel Pathan on 03/01/2015 for PL ticket #1068
                }
                dictPlanFolderId.Add(_plan.PlanId, folderId);
            }
            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Mapping Eloqua Folder Ids");
            _mappingPlan_FolderId = dictPlanFolderId;
        }

        /// <summary>
        /// Search for folder Id to upload tactic data in Eloqua.
        /// </summary>
        /// <param name="planTactic"> Tactic obj.</param>
        /// <returns> Return folder Id.</returns>
        public int GetEloquaFolderIdByPlanId(int PlanId)
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            //Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get folder id from GP database");
            int folderId = 0;
            try
            {
                folderId = _mappingPlan_FolderId.ToList().Where(_planFldr => _planFldr.Key.Equals(PlanId)).Select(_planFldr => _planFldr.Value).FirstOrDefault();
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Error occurred while Get folder id from GP database: " + exMessage);
            }
            //Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get folder id from GP database");
            return folderId;
        }

        /// <summary>
        /// Search folder name from Eloqua.
        /// </summary>
        /// <param name="FolderName"> Folder Name for Search</param>
        /// <returns>Return folder search response.</returns>
        public IRestResponse SearchFolderInEloqua(string FolderName)
        {
            RestRequest request = new RestRequest(Method.GET)
            {
                RequestFormat = DataFormat.Json,
                Resource = string.Format("/assets/campaign/folders?search={0}&depth=complete", "'" + FolderName + "'")
            };
            request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);
            IRestResponse response = _client.Execute(request);

            return response;
        }

        /// <summary>
        /// Function to get Eloqua CampaignId by CRMId(SalesforceId).
        /// Added By: Viral Kadiya
        /// Added Date: 01/02/2015
        /// </summary>
        /// <param name="eloquaCampaignId">Eloqua campaign Id.</param>
        /// <returns>Returns eloqua campaign object.</returns>
        public string GetEloquaCampaignIdByCRMId(string crmId)
        {
            #region "Old Code"
            //string strSearchTerm = "crmId='" + crmId.ToString() + "'";  // Get Campaign data using Search query based on CRMId.
            //string _EloquaCampaignId = string.Empty;
            //RestRequest request = new RestRequest(Method.GET)
            //{
            //    Resource = string.Format("/assets/campaigns?search={0}&depth=complete",
            //                      strSearchTerm),
            //    RequestFormat = DataFormat.Json
            //};

            //IRestResponse response = _client.Execute(request);
            ////// Manipulation of contact list response and store into model
            //string TacticResult = response.Content.ToString();
            //if (!string.IsNullOrEmpty(TacticResult))
            //{
            //    JObject joResponse = JObject.Parse(TacticResult);
            //    JArray elementsArray = (JArray)joResponse["elements"];
            //    if (elementsArray.Count > 0)
            //    {
            //        _EloquaCampaignId = elementsArray[0]["id"].ToString();
            //    }
            //} 
            #endregion

            string _EloquaCampaignId = string.Empty;
            _EloquaCampaignId = GetEloquaCampaignIdBy_Short_Long_CRMId(crmId);

            // if EloquaCampaignID blank for long CRMID then get EloquaCampaignID for Short CRMID.
            if (string.IsNullOrEmpty(_EloquaCampaignId) || _EloquaCampaignId == "0")
            {
                crmId = crmId.Substring(0, 15);
                _EloquaCampaignId = GetEloquaCampaignIdBy_Short_Long_CRMId(crmId);
            }

            return _EloquaCampaignId;
        }

        /// <summary>
        /// Function to get Eloqua CampaignId by long or short CRMId(SalesforceId).
        /// Added By: Viral Kadiya
        /// Added Date: 09/02/2015
        /// </summary>
        /// <param name="crmId">Salesforce Id.</param>
        /// <returns>Returns eloqua campaign ID.</returns>
        public string GetEloquaCampaignIdBy_Short_Long_CRMId(string crmId)
        {
            string strSearchTerm = "crmId='" + crmId.ToString() + "'";  // Get Campaign data using Search query based on CRMId.
            string eloquaCampaignId = string.Empty;
            RestRequest request = new RestRequest(Method.GET)
            {
                Resource = string.Format("/assets/campaigns?search={0}&depth=complete",
                                  strSearchTerm),
                RequestFormat = DataFormat.Json
            };
            request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);
            IRestResponse response = _client.Execute(request);
            //// Manipulation of contact list response and store into model
            string TacticResult = response.Content.ToString();
            if (!string.IsNullOrEmpty(TacticResult))
            {
                JObject joResponse = JObject.Parse(TacticResult);
                JArray elementsArray = (JArray)joResponse["elements"];
                if (elementsArray.Count > 0)
                {
                    eloquaCampaignId = elementsArray[0]["id"].ToString();
                }
            }
            return eloquaCampaignId;
        }
        #endregion

        #region Common

        /// <summary>
        /// Search and Get Eloqua Root folder Id.
        /// </summary>
        /// <returns>Return Eloqua Root folder Id.</returns>
        public int GetEloquaRootFolderId()
        {
            IRestResponse response = SearchFolderInEloqua("*");

            //// Convert Json response into class.
            folderResponse respo = JsonConvert.DeserializeObject<folderResponse>(response.Content);

            if (respo != null)
            {
                var parentId = respo.elements.Where(p => p.description == "Root" && p.isSystem.ToLower() == "true").Select(p => new { p.id }).FirstOrDefault();

                return Convert.ToInt32(parentId.id);
            }

            return 0;
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to delete eloqua campaign.
        /// </summary>
        /// <param name="id">Plan or improvement tactic id.</param>
        /// <returns>Returns flag to determine whether delete was successfull or not.</returns>
        private bool Delete(string id)
        {
            RestRequest request = new RestRequest(Method.DELETE)
            {
                Resource = "/assets/campaign/" + id,
                RequestFormat = DataFormat.Json
            };
            request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);
            IRestResponse<object> response = _client.Execute<object>(request);
            //// Modified by: Maninder
            //// Modified Date: 08/26/2014
            //// Ticket: #729 	Approve Tactic Functionality Not Working
            //// To handle case where tactic are already deleted from Eloqua site.
            if (response != null)
            {
                if ((response.ResponseStatus == ResponseStatus.Error && response.StatusCode == HttpStatusCode.NotFound) ||
                    (HttpStatusCode.OK == response.StatusCode && response.ResponseStatus == ResponseStatus.Completed))
                {
                    _lstSyncError.Add(Common.PrepareSyncErrorList(Convert.ToInt32(id), Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), Enums.Mode.Delete.ToString(), Enums.SyncStatus.Success, DateTime.Now));
                    return true;
                }

                //// Start - Added by Sohel Pathan on 09/01/2015 for PL ticket #1068
                _lstSyncError.Add(Common.PrepareSyncErrorList(int.Parse(id), Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "\"" + id + "\" - tactic not deleted.", Enums.SyncStatus.Error, DateTime.Now));
                //// End - Added by Sohel Pathan on 09/01/2015 for PL ticket #1068
                return false;
            }
            else
            {
                //// Start - Added by Sohel Pathan on 05/01/2015 for PL ticket #1068
                _lstSyncError.Add(Common.PrepareSyncErrorList(int.Parse(id), Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), "Eloqua exception occurred while deleting campaign at Eloqua for tactic Id \"" + id + "\".", Enums.SyncStatus.Error, DateTime.Now));
                //// End - Added by Sohel Pathan on 05/01/2015 for PL ticket #1068
                throw new Exception(string.Format("[{0}] [{1}]", response.StatusCode.ToString(), response.StatusDescription), response.ErrorException);
            }
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to get target key value pair.
        /// </summary>
        /// <typeparam name="T">Plan or improvement tactic type.</typeparam>
        /// <param name="obj">Plan or improvement tactic.</param>
        /// <param name="mappingDataType">Mapping data type.</param>
        /// <returns>Returns target key value pair.</returns>
        private IDictionary<string, object> GetTargetKeyValue<T>(object obj, Dictionary<string, string> mappingDataType)
        {
            string createdBy = "CreatedBy";
            string statDate = "StartDate";
            string endDate = "EndDate";
            string effectiveDate = "EffectiveDate";
            string costActual = "CostActual";   // Added by Sohel Pathan on 11/09/2014 for PL ticket #773
            string tacticType = "TacticType";   //// Added by Sohel Pathan on 29/01/2015 for PL ticket #1113
            string tacticTitle = "Title";   //// Added by Viral kadiya on 02/17/2016 for PL ticket #1916

            Type sourceType = ((T)obj).GetType();
            PropertyInfo[] sourceProps = sourceType.GetProperties();
            Dictionary<string, object> keyvaluepair = new Dictionary<string, object>();
            List<FieldValue> fieldValues = new List<FieldValue>();

            foreach (KeyValuePair<string, string> mapping in mappingDataType)
            {
                string value = string.Empty;
                PropertyInfo propInfo = sourceProps.FirstOrDefault(property => property.Name.Equals(mapping.Key));
                if (propInfo != null)
                {
                    value = Convert.ToString(propInfo.GetValue(((T)obj), null));
                    if (mapping.Key == createdBy)
                    {
                        value = _mappingUser[Convert.ToInt32(value)];
                    }
                    else if (mapping.Key == statDate || mapping.Key == endDate || mapping.Key == effectiveDate)
                    {
                        // #2097: In case of push Linked Tactic,get startdate value from global varialbe(i.e. "startDate")
                        if (mapping.Key == statDate && startDate.HasValue && obj is Plan_Campaign_Program_Tactic)
                            value = ConvertToUnixEpoch(Convert.ToDateTime(startDate)).ToString();
                        else
                            value = ConvertToUnixEpoch(Convert.ToDateTime(value)).ToString();  // If not linked tactic then pick start date from respecitve entity(i.e.Tactic,ImprovementTactic)
                    }
                    //// Start - Added by Sohel Pathan on 29/01/2015 for PL ticket #1113
                    else if (mapping.Key == tacticType)
                    {
                        value = ((Plan_Campaign_Program_Tactic)obj).TacticType.Title;
                    }
                    //// End - Added by Sohel Pathan on 29/01/2015 for PL ticket #1113
                    //// Start - Added by Viral Kadiya on 02/16/2016 for PL ticket #1916
                    if (mapping.Key == tacticTitle)
                    {
                        value = System.Web.HttpUtility.HtmlDecode(value);
                    }
                    //// End - Added by Viral Kadiya on 02/16/2016 for PL ticket #1916
                }
                // Start - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
                else
                {
                    if (mapping.Key == costActual)
                    {
                        value = GetActualCostbyPlanTacticId(((Plan_Campaign_Program_Tactic)obj).PlanTacticId);
                    }
                    else
                    {
                        value = MapCustomField<T>(obj, sourceProps, mapping.Key);
                    }
                }
                // End - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
                if (campaignMetadata.Select(target => target.Key).ToList().Contains(mapping.Value))
                {
                    keyvaluepair.Add(mapping.Value, value);
                }
                else
                {
                    var customFieldId = customFields.Where(customField => customField.Value.Equals(mapping.Value)).Select(customField => customField).FirstOrDefault();
                    if (customFieldId.Key != null)
                    {
                        fieldValues.Add(new FieldValue { id = customFieldId.Key, type = "FieldValue", value = value });
                    }
                }
                // End - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
            }

            if (fieldValues.Count > 0)
            {
                keyvaluepair.Add("fieldValues", fieldValues);
            }

            return keyvaluepair;
        }

        /// <summary>
        /// Added By: Sohel Pathan
        /// Added Date: 04/12/2014
        /// Description: Map Custom Field Data for Integration
        /// </summary>
        /// <typeparam name="T">Plan or improvement tactic type</typeparam>
        /// <param name="obj">Plan or improvement tactic</param>
        /// <param name="sourceProps">Array of properties for given obj</param>
        /// <param name="mapping">Mapping field item</param>
        /// <returns>returns object of FieldValue</returns>
        private string MapCustomField<T>(object obj, PropertyInfo[] sourceProps, string key)
        {
            if (_mappingCustomFields != null)
            {
                if (_mappingCustomFields.Count > 0)
                {
                    string mappingKey = string.Empty;
                    string EntityType = ((T)obj).GetType().BaseType.Name;
                    string EntityTypeId = string.Empty;
                    PropertyInfo propInfoCustom = null;

                    if (EntityType == Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program_Tactic.ToString())
                    {
                        propInfoCustom = sourceProps.FirstOrDefault(property => property.Name.Equals("PlanTacticId", StringComparison.OrdinalIgnoreCase));
                    }

                    if (propInfoCustom != null)
                    {
                        EntityTypeId = Convert.ToString(propInfoCustom.GetValue(((T)obj), null));
                    }
                    var mapobj = _mappingCustomFields.Where(map => map.EntityId == Convert.ToInt32(EntityTypeId) && map.CustomFieldId == Convert.ToInt32(key)).FirstOrDefault();
                    if (mapobj != null)
                    {
                        return mapobj.Value;
                    }
                }
            }
            return string.Empty;
        }

        //// Modified By: Maninder Singh Wadhva
        //// Modified Date: 08/11/2014  	
        //// Ticket: #675 Integration - Verify tactic data push from GP to Eloqua with recent UI changes
        private static DateTime _unixEpochTime = new DateTime(1970, 1, 1, 0, 0, 0);

        private static long ConvertToUnixEpoch(DateTime date)
        {
            try
            {
                _unixEpochTime = new DateTime(1970, 1, 1, 0, 0, 0);
                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                DateTime _unixEpochTimeOtherTimeZone = TimeZoneInfo.ConvertTimeFromUtc(_unixEpochTime, timeZone);
                DateTime dateOtherTimeZone = TimeZoneInfo.ConvertTimeFromUtc(date, timeZone);
                double difference = date.Subtract(dateOtherTimeZone).TotalSeconds;
                return (long)(new TimeSpan(dateOtherTimeZone.Ticks - _unixEpochTimeOtherTimeZone.Ticks).TotalSeconds + difference);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new Exception("The registry does not define the Central Standard Time zone.");
            }
            catch (InvalidTimeZoneException)
            {
                throw new Exception("Registry data on the Central Standard Time zone has been corrupted.");
            }
        }

        /// <summary>
        /// Convert timestamp to Date.
        /// </summary>
        /// <param name="MQLDate"> Time stamp date.</param>
        /// <returns>return Date.</returns>
        public DateTime ConvertTimestampToDateTime(string MQLDate)
        {
            _unixEpochTime = new DateTime(1970, 1, 1, 0, 0, 0);

            // This is an example of a UNIX timestamp for the date/time 11-04-2005 09:25.
            double timestamp = Convert.ToDouble(MQLDate);

            // Get short time from date
            string shortTimeString = _unixEpochTime.ToShortTimeString();

            // Add the number of seconds in UNIX timestamp to be converted.
            _unixEpochTime = _unixEpochTime.AddSeconds(timestamp);

            // The dateTime now contains the right date/time so to format the string,
            // use the standard formatting methods of the DateTime object.
            string date = _unixEpochTime.ToShortDateString() + " " + shortTimeString;

            return Convert.ToDateTime(date);
        }

        /// <summary>
        /// To Get formatted error message
        /// Added by Dharmraj on 20-8-2014, #684
        /// </summary>
        /// <param name="e"></param>
        /// <returns>Error Message</returns>
        private string GetErrorMessage(Exception e)
        {
            _isResultError = true;
            if (e.InnerException != null)
            {
                return string.Format("{0}: {1}", e.Message, e.InnerException.Message);
            }
            else
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Added by : Sohel Pathan
        /// Added Date : 04/12/2014
        /// Description : Prepare a dictionary for Custom Fields with CustomFieldId and its value.
        /// </summary>
        /// <param name="EntityIdList">List of Entity Ids with which Custom Fields are associated like PlanCampaignIds for Campaign Entity Type</param>
        /// <param name="EntityType">Type of Entity with which Custom Fields are associated like Campaign, Program or Tactic</param>
        private void CreateMappingCustomFieldDictionary(List<int> EntityIdList, string EntityType)
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get Mapping detail for " + EntityType + " custom field(s)");
            try
            {
                if (EntityIdList.Count > 0)
                {
                    _mappingCustomFields = new List<CustomFiledMapping>();
                    string idList = string.Join(",", EntityIdList);

                    // In Eloqua, It uses MappingKey format like 'CustomfieldId-EntityId' (ex.: '56-39073') to retrieve customfield value from list in "MapCustomField" method.
                    String Query = "select distinct cast(Extent1.CustomFieldID as nvarchar) + '-' + cast(EntityId as nvarchar) as keyv, " +
                        "cast([Extent1].[CustomFieldId] as nvarchar) as CustomFieldId," +
                        "cast(EntityId as nvarchar) as EntityId," +
                        "case  " +
                           "    when A.keyi is not null then Extent2.AbbreviationForMulti " +
                           "    when Extent3.[Name]='TextBox' then Extent1.Value " +
                           "    when Extent3.[Name]='DropDownList' then Extent4.Value " +
                        "End as ValueV, " +
                        "case  " +
                           "    when A.keyi is not null then Extent2.AbbreviationForMulti" +
                           "   when Extent3.[Name]='TextBox' then Extent1.Value " +
                           "    when Extent3.[Name]='DropDownList' then CASE WHEN Extent4.Abbreviation IS nOT NULL THEN Extent4.Abbreviation ELSE Extent4.Value END" +
                            "   END as CustomName " +

                        " from CustomField_Entity Extent1 " +
                        "INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] AND [Extent2].[IsDeleted] = 0 " +
                        "INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId " +
                        "Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and cast(Extent1.Value as nvarchar)=cast(Extent4.CustomFieldOptionID as nvarchar)" +
                        "Left Outer join ( " +
                        "select cast(Extent1.CustomFieldID as nvarchar) + '-' + cast(EntityId as nvarchar) as keyi " +

                        " from CustomField_Entity Extent1 " +
                        "INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] " +
                        "INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId " +
                        "Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and Extent1.Value=Extent4.CustomFieldOptionID " +
                        "WHERE ([Extent1].[EntityId] IN (" + idList + ")) " +
                        "group by cast(Extent1.CustomFieldID as nvarchar) + '-' + cast(EntityId as nvarchar) " +
                        "having count(*) > 1 " +
                        ") A on A.keyi=cast(Extent1.CustomFieldID as nvarchar) + '-' + cast(EntityId as nvarchar) " +
                        "WHERE ([Extent1].[EntityId] IN (" + idList + ")) " +
                        "order by keyv";

                    using (MRPEntities mp = new MRPEntities())
                    {
                        DbConnection conn = mp.Database.Connection;
                        conn.Open();
                        DbCommand comm = conn.CreateCommand();
                        comm.CommandText = Query;
                        DbDataReader ddr = comm.ExecuteReader();

                        while (ddr.Read())
                        {
                            _mappingCustomFields.Add(new CustomFiledMapping { CustomFieldId = !ddr.IsDBNull(1) ? Convert.ToInt32(ddr.GetString(1)) : 0, EntityId = !ddr.IsDBNull(2) ? Convert.ToInt32(ddr.GetString(2)) : 0, Value = !ddr.IsDBNull(3) ? Convert.ToString(ddr.GetString(3)) : string.Empty, CustomNameValue = !ddr.IsDBNull(4) ? Convert.ToString(ddr.GetString(4)) : string.Empty });
                        }
                        conn.Close();
                    }
                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get Mapping detail for " + EntityType + " custom field(s), Found " + _mappingCustomFields.Count().ToString() + " custome field mapping");
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Get Mapping detail for " + EntityType + " custom field(s): " + exMessage);
            }

        }

        #endregion

        #region Authentication

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to authenticate credentials of eloqua instance.
        /// </summary>
        public void Authenticate()
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            int entityId = _integrationInstanceId;
            try
            {
                if (_entityType.Equals(EntityType.Tactic) || _entityType.Equals(EntityType.ImprovementTactic))
                    entityId = _id;

                Common.SaveIntegrationInstanceLogDetails(entityId, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Eloqua Authentication start.");
                //_apiURL = GetInstanceURL();   // commented by Viral Kadiya to Implement PL ticket #910: Implement OAuth authentication of Eloqua.
                if (!string.IsNullOrWhiteSpace(_apiURL))
                {
                    //_client = this.AuthenticateBase();
                    this.OAuthAuthenticateBase();   // Added by Viral Kadiya to Implement PL ticket #910: Implement OAuth authentication of Eloqua.
                }
                Common.SaveIntegrationInstanceLogDetails(entityId, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Eloqua Authentication end.");
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(entityId, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "An error ocured while authenticating with Eloqua: " + exMessage);
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, "An error ocured while authenticating with Eloqua.", Enums.SyncStatus.Error, DateTime.Now));
                _isResultError = true;
                _ErrorMessage = exMessage;
                _isAuthenticated = false;
            }
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to get instance url.
        /// </summary>
        /// <returns>Returns standard url.</returns>
        private string GetInstanceURL()
        {

            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            int entityId = _integrationInstanceId;
            if (_entityType.Equals(EntityType.Tactic) || _entityType.Equals(EntityType.ImprovementTactic))
                entityId = _id;
            Common.SaveIntegrationInstanceLogDetails(entityId, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Get instance URL");
            RestClient baseClient = AuthenticateBase();


            RestRequest request = new RestRequest(Method.GET)
            {
                Resource = "/id",
                RequestFormat = DataFormat.Json
            };
            request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);
            string url = string.Empty;
            IRestResponse<InstanceURL> instanceURL = baseClient.Execute<InstanceURL>(request);
            if (instanceURL.ResponseStatus == ResponseStatus.Completed && instanceURL.StatusCode == HttpStatusCode.OK)
            {
                url = instanceURL.Data.urls.apis.rest.standard.Replace("{version}", _apiVersion);
                Common.SaveIntegrationInstanceLogDetails(entityId, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Get instance URL");
            }
            else
            {
                Common.SaveIntegrationInstanceLogDetails(entityId, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "An error ocured while get instance URL with Eloqua." + instanceURL.Content);
                throw new Exception(instanceURL.Content);
            }

            return url;
        }

        /// <summary>
        /// Function to authenticate eloqua instance credentials.
        /// </summary>
        /// <returns>Returns client object.</returns>
        private RestClient AuthenticateBase()
        {
            return new RestClient(_apiURL)
            {
                Authenticator = new HttpBasicAuthenticator(_companyName + "\\" + _username, _password)
            };
        }

        /// <summary>
        /// Function to authenticate eloqua instance credentials.
        /// </summary>
        /// <returns>Returns client object.</returns>
        private void OAuthAuthenticateBase()
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {
                //const string clientId = "745d7e5e-1265-41b3-83ec-d8724a033f98";
                //const string clientSecret = "1ZXwfhoxJfEq0SlvVuj~0ticvR6lmC74vkg2YaqsOSZJOqcqizH~hcduhCGg6zT9y4VRrSVRnoV3XCW3XWoCFTceDYE~pKn2pBSK";

                //IRestClient restClient = new RestClient(_apiURL);
                RestClient restClient = new RestClient(_apiURL);
                restClient.Authenticator = new HttpBasicAuthenticator(_eloquaClientID, _ClientSecret);
                RestRequest request = new RestRequest("/auth/oauth2/token?grant_type=password&scope=full&username=" + _companyName + "\\" + _username + "&password=" + _password);
                // RestRequest request = new RestRequest("/auth/oauth2/token?grant_type=password&scope=full&username=" + "ZebraTechnologiesCorp" + "\\" + "Bulldog.Gameplan" + "&password=" + "tew@U4RapheS");
                request.Method = Method.POST;
                IRestResponse response = restClient.Post(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _isAuthenticated = true;
                    Eloqua_RefreshToken objOAuthTokens = JsonConvert.DeserializeObject<Eloqua_RefreshToken>(response.Content);
                    _AccessToken = objOAuthTokens.access_token;
                    restClient.BaseUrl = GetInstanceURL();
                    //insertation start #2310 Kausha 23/06/2013 Following is added to Save/Update eloqua base url.
                    #region save base url in to EntityIntegration_Attribute table
                    if (!string.IsNullOrEmpty(restClient.BaseUrl))
                    {
                        //string[] baseArray=restClient.BaseUrl.ToLower().Split(',');
                        string[] baseArray = Regex.Split(restClient.BaseUrl.ToLower(), "api");
                        if (baseArray != null)
                        {
                            if (baseArray.Count() > 0)
                            {
                                //  Common.Save_EditEloquaBaseUrl(_entityType,_integrationInstanceId);   
                                if(_integrationInstanceId>0)
                                {
                                    MRPEntities ent = new MRPEntities();
                                    string entityType = Convert.ToString(Helper.Enums.EntityType.IntegrationInstance);
                                    var instanceData = ent.EntityIntegration_Attribute.Where(data => data.EntityId == _integrationInstanceId && data.IntegrationinstanceId == _integrationInstanceId && data.EntityType.ToLower() == entityType.ToLower()).FirstOrDefault();
                                    using (MRPEntities db = new MRPEntities())
                                    {
                                        EntityIntegration_Attribute objLogDetails = new EntityIntegration_Attribute();
                                        objLogDetails.EntityId = _integrationInstanceId;
                                        objLogDetails.EntityType = entityType;
                                        objLogDetails.IntegrationinstanceId = _integrationInstanceId;
                                        objLogDetails.AttrType = Convert.ToString(Helper.Enums.EntityIntegrationAttribute.EloquaBaseUrl);
                                        objLogDetails.AttrValue = baseArray[0];
                                        objLogDetails.CreatedDate = DateTime.Now;
                                        if (instanceData == null)
                                            db.Entry(objLogDetails).State = System.Data.EntityState.Added;
                                        else
                                        {
                                            objLogDetails.ID = instanceData.ID;

                                            db.Entry(objLogDetails).State = System.Data.EntityState.Modified;
                                        }

                                        db.SaveChanges();
                                    }
                                }
                               
                            }
                        }
                    }

                    #endregion
                    //insertation end #2310
                }
                else
                    _isAuthenticated = false;

                _client = restClient;
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_integrationInstanceId, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "An error ocured while OAuth authentication with Eloqua: " + exMessage);
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, "An error ocured while OAuth authentication with Eloqua.", Enums.SyncStatus.Error, DateTime.Now));
                _isResultError = true;
                _ErrorMessage = exMessage;
                _isAuthenticated = false;
            }
        }

        #region "Update Access Token based on Refresh Token"
        //private IRestResponse GetUpdatedAccessToken(string strRefreshToken)
        //{
        //    const string clientId = "745d7e5e-1265-41b3-83ec-d8724a033f98";
        //    const string clientSecret = "1ZXwfhoxJfEq0SlvVuj~0ticvR6lmC74vkg2YaqsOSZJOqcqizH~hcduhCGg6zT9y4VRrSVRnoV3XCW3XWoCFTceDYE~pKn2pBSK";
        //    string strEloquaLoginUrl = "https://login.eloqua.com";
        //    IRestClient restClient = new RestClient(strEloquaLoginUrl);
        //    restClient.Authenticator = new HttpBasicAuthenticator(clientId, clientSecret);
        //    IRestRequest request = new RestRequest("/auth/oauth2/token?grant_type=refresh_token&scope=full&refresh_token=" + strRefreshToken);
        //    request.Method = Method.POST;
        //    IRestResponse response = restClient.Post(request);
        //    return response;
        //} 
        #endregion

        #endregion

        //This function is called from test project so no need of error logging
        public string TestGenerateCustomName(Plan_Campaign_Program_Tactic planTactic, int clientId)
        {
            string customName = "";
            if (planTactic != null)
            {
                customName = Common.GenerateCustomName(planTactic, SequencialOrderedTableList, _mappingCustomFields);
            }
            return customName;
        }

        #region Contact List Manipulation

        /// <summary>
        /// Function to sync from Eloqua to Gameplan.
        /// </summary>
        public void GetDataPullEloqua()
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {
                EloquaResponse objEloquaResponse = new EloquaResponse();
                List<SyncError> lstSyncError = new List<SyncError>();
                Common.SaveIntegrationInstanceLogDetails(_integrationInstanceId, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Tactic MQLs");
                //try catch is handdled in this method so internal error logging is not required
                bool isError = objEloquaResponse.SetTacticMQLs(_integrationInstanceId, _userId, _integrationInstanceLogId, _applicationId, EntityType.Tactic, out lstSyncError);
                _lstSyncError.AddRange(lstSyncError);
                if (isError)
                {
                    _isResultError = true;
                }
                Common.SaveIntegrationInstanceLogDetails(_integrationInstanceId, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Tactic MQLs");
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(_integrationInstanceId, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "System error occurred while pulling mql from Eloqua :" + exMessage);
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "System error occurred while pulling mql from Eloqua.", Enums.SyncStatus.Error, DateTime.Now));
                _ErrorMessage = exMessage;
                _isResultError = true;
            }
        }

        /// <summary>
        /// Function to get Eloqua Contact List.
        /// Added By: Pratik
        /// Added Date: 12/30/2014
        /// Ticket #1060  -  
        /// </summary>
        /// <param name="elouqaContactListId">Eloqua contact list Id.</param>
        /// <param name="eloquaViewId"> Eloqua view Id.</param>
        /// <returns>Returns eloqua campaign object.</returns>
        public IRestResponse GetEloquaContactList(string elouqaContactListId, string eloquaViewId, int page)
        {
            RestRequest request = new RestRequest(Method.GET)
            {
                Resource = string.Format("/data/contact/view/{0}/contacts/list/{1}?page={2}", eloquaViewId, elouqaContactListId, page),
                RequestFormat = DataFormat.Json
            };
            request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);
            IRestResponse response = _client.Execute(request);

            return response;
        }

        /// <summary>
        /// Function to get Eloqua Contact List Detail.
        /// Added By: Pratik
        /// Added Date: 12/30/2014
        /// Ticket #1060  -  
        /// </summary>
        /// <param name="elouqaContactListId">Eloqua contact list Id.</param>
        /// <param name="eloquaViewId"> Eloqua view Id.</param>
        /// <returns>Returns eloqua campaign object.</returns>
        public IRestResponse GetEloquaContactListDetails(string elouqaContactListId)
        {
            try
            {
                RestRequest request = new RestRequest(Method.GET)
                {
                    Resource = string.Format("/assets/contact/list/{0}", elouqaContactListId),
                    RequestFormat = DataFormat.Json
                };
                request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);
                IRestResponse response = _client.Execute(request);

                return response;

            }
            catch (Exception)
            {
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "An error occurred while getting contact list from Eloqua.", Enums.SyncStatus.Error, DateTime.Now));
                throw;
            }
        }

        /// <summary>
        /// Function to put Eloqua Contact List Detail.
        /// Added By: Pratik
        /// Added Date: 12/30/2014
        /// Ticket #1060  -  
        /// </summary>
        /// <param name="elouqaContactListId">Eloqua contact list Id.</param>
        /// <param name="eloquaViewId"> Eloqua view Id.</param>
        /// <returns>Returns eloqua campaign object.</returns>
        public void PutEloquaContactListDetails(ContactListDetailModel contactListDetails, string elouqaContactListId)
        {
            try
            {
                RestRequest request = new RestRequest(Method.PUT)
                {
                    Resource = string.Format("/assets/contact/list/{0}", elouqaContactListId),
                    RequestFormat = DataFormat.Json
                };
                request.AddParameter("Authorization", "Bearer " + _AccessToken, ParameterType.HttpHeader);
                request.AddBody(contactListDetails);

                IRestResponse response = _client.Execute(request);
            }
            catch (Exception)
            {
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "An error occurred while updating contact list from Eloqua.", Enums.SyncStatus.Error, DateTime.Now));
                throw;
            }
        }

        #endregion

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

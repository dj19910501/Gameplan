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
using System.Transactions;

#endregion
/*
 *  Author: 
 *  Created Date: 
 *  Purpose: Integration with Eloqua  
  */

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
        public string _instance { get; set; }
        public string _apiURL { get; set; }
        public string _apiVersion { get; set; }
        private bool _isAuthenticated { get; set; }
        private int _integrationInstanceId { get; set; }
        private int _id { get; set; }
        private Guid _userId { get; set; }
        private int _integrationInstanceLogId { get; set; }
        private EntityType _entityType { get; set; }
        public string _ErrorMessage { get; set; }
        private bool _isResultError { get; set; }

        private Dictionary<string, string> _mappingTactic { get; set; }
        private Dictionary<string, string> _mappingImprovementTactic { get; set; }
        private Dictionary<int, string> _mappingVertical { get; set; }
        private Dictionary<int, string> _mappingAudience { get; set; }
        private Dictionary<Guid, string> _mappingGeography { get; set; }
        private Dictionary<Guid, string> _mappingBusinessunit { get; set; }
        private Dictionary<Guid, string> _mappingUser { get; set; }
        private Dictionary<string, string> _mappingCustomFields { get; set; }  // Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
        private List<string> IntegrationInstanceTacticIds { get; set; }
        private List<string> campaignMetadata { get; set; }
        private Dictionary<string, string> customFields { get; set; }
        private static string NotFound = "NotFound";
        //Start - Added by Mitesh Vaishnav for PL ticket #1002 Custom Naming: Integration
        private Guid _clientId { get; set; }
        private bool _CustomNamingPermissionForInstance = false;
        private bool IsClientAllowedForCustomNaming = false;
        private Guid _applicationId = Guid.Empty;
        //private List<BDSService.ClientApplicationActivity> _clientActivityList { get; set; }
        //End - Added by Mitesh Vaishnav for PL ticket #1002 Custom Naming: Integration

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
        public IntegrationEloquaClient(int integrationInstanceId, int id, EntityType entityType, Guid userId, int integrationInstanceLogId, Guid applicationId)
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
            campaignMetadata = new List<string>();
            campaignMetadata.Add("name");
            campaignMetadata.Add("description");
            campaignMetadata.Add("startAt");
            campaignMetadata.Add("endAt");
            campaignMetadata.Add("budgetedCost");
            campaignMetadata.Add("currentStatus");
            campaignMetadata.Add("actualCost");
        }

        /// <summary>
        /// Function to set integration instance detail.
        /// Added By: Maninder Singh Wadhva
        /// </summary>
        private void SetIntegrationInstanceDetail()
        {
            IntegrationInstance integrationInstance = db.IntegrationInstances.Where(instance => instance.IntegrationInstanceId == _integrationInstanceId).SingleOrDefault();
            _CustomNamingPermissionForInstance = integrationInstance.CustomNamingPermission;
            this._instance = integrationInstance.Instance;
            this._username = integrationInstance.Username;
            this._password = Common.Decrypt(integrationInstance.Password);
            this._apiURL = integrationInstance.IntegrationType.APIURL;
            this._apiVersion = integrationInstance.IntegrationType.APIVersion;
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to authenticate credentials of eloqua instance.
        /// </summary>
        public void Authenticate()
        {
            try
            {
                _apiURL = GetInstanceURL();
                if (!string.IsNullOrWhiteSpace(_apiURL))
                {
                    _client = this.AuthenticateBase();
                    _isAuthenticated = true;
                }
            }
            catch (Exception ex)
            {
                _isResultError = true;
                _ErrorMessage = GetErrorMessage(ex);
                _isAuthenticated = false;
            }
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
                Resource = string.Format("/assets/campaign/fields?search={0}&page={1}&count={2}&depth=complete",
                                  "*", 1, 100)
            };

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
        /// Function to get target data type of eloqua instance.
        /// </summary>
        /// <returns>Returns list of target data type of eloqua instance.</returns>
        public List<string> GetTargetDataType()
        {
            List<string> targetDataTypeList = customFields.Select(customField => customField.Value).ToList();
            targetDataTypeList.AddRange(campaignMetadata);
            return targetDataTypeList.OrderBy(targetDataType => targetDataType).ToList();
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to sync data from gameplan to eloqua.
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
                // Start - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
                List<int> tacticIdList = new List<int>() { planTactic.PlanTacticId };
                CreateMappingCustomFieldDictionary(tacticIdList, Enums.EntityType.Tactic.ToString());
                // End - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
                planTactic = SyncTacticData(planTactic);
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
                Common.UpdateIntegrationInstanceSection(_integrationInstanceSectionId, StatusResult.Error, string.Empty);
            }
            else
            {
                // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                Common.UpdateIntegrationInstanceSection(_integrationInstanceSectionId, StatusResult.Success, string.Empty);
            }

            ////Added By: Maninder Singh Wadhva
            ////Added Date: 08/20/2014
            ////Ticket #717 	Pulling from Eloqua - Actual Cost 
            if (IsInstanceSync)
            {
                bool isImport = false;
                isImport = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId.Equals(_integrationInstanceId)).IsImportActuals;

                //// Check isimport flag.
                if (isImport)
                {
                    //// Pulling actual cost.
                    //PullingActualCost();  // Commented by Sohel Pathan on 11/09/2014 for PL ticket #773
                    //// Pull responses from Eloqua
                    GetDataForTacticandUpdate();

                }
            }

            return _isResultError;
        }

        ///// <summary>
        ///// Function to pull actual cost
        ///// Added By: Maninder Singh
        ///// Added Date: 08/20/2014
        ///// Ticket #717 Pulling from Eloqua - Actual Cost 
        ///// </summary>
        //private void PullingActualCost()
        //{
        //    int IntegrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.ImportActual.ToString(), DateTime.Now, _userId);

        //    string actualCost = "CostActual";
        //    var fieldname = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.IntegrationInstanceId.Equals(_integrationInstanceId) && mapping.GameplanDataType.TableName == "Plan_Campaign_Program_Tactic" &&
        //                                                                    mapping.GameplanDataType.ActualFieldName == actualCost).Select(mapping => mapping.TargetDataType).FirstOrDefault();

        //    //// Checking whether actual cost field exist in mapping.
        //    if (fieldname != null)
        //    {
        //        //// Getting list of plans whose model have current integration instance configured for pushing tactic.
        //        List<int> planIds = db.Plans.Where(p => p.Model.IntegrationInstanceId == _integrationInstanceId && p.Model.Status.Equals("Published")).Select(p => p.PlanId).ToList();

        //        // Get List of status after Approved Status.
        //        List<string> statusList = Common.GetStatusListAfterApproved();

        //        //// Getting list of apporved/in-progress/completed tactic of above plan.
        //        List<Plan_Campaign_Program_Tactic> tacticList = db.Plan_Campaign_Program_Tactic.Where(t => planIds.Contains(t.Plan_Campaign_Program.Plan_Campaign.PlanId) && statusList.Contains(t.Status) && t.IsDeployedToIntegration && t.IntegrationInstanceTacticId != null).ToList();
        //        if (tacticList != null && tacticList.Count > 0)
        //        {
        //            try
        //            {
        //                bool ErrorFlag = false;

        //                //// List to hold eloqua campaign fetched using api.
        //                List<EloquaCampaign> importEloquaCampaignList = new List<EloquaCampaign>();

        //                //// Iterating over each tactic and fetching details from api.
        //                foreach (Plan_Campaign_Program_Tactic tactic in tacticList)
        //                {
        //                    EloquaCampaign eloquaCampaign = new EloquaCampaign();

        //                    try
        //                    {
        //                        //// Getting details from api.
        //                        eloquaCampaign = GetEloquaCampaign(tactic.IntegrationInstanceTacticId);
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        //// Logging error for tactic entity.
        //                        ErrorFlag = true;
        //                        IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
        //                        instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
        //                        instanceTactic.IntegrationInstanceId = _integrationInstanceId;
        //                        instanceTactic.EntityId = tactic.PlanTacticId;
        //                        instanceTactic.EntityType = EntityType.Tactic.ToString();
        //                        instanceTactic.Status = StatusResult.Error.ToString();
        //                        instanceTactic.Operation = Operation.Import_Cost.ToString();
        //                        instanceTactic.SyncTimeStamp = DateTime.Now;
        //                        instanceTactic.CreatedDate = DateTime.Now;
        //                        instanceTactic.ErrorDescription = GetErrorMessage(e);
        //                        instanceTactic.CreatedBy = _userId;
        //                        db.Entry(instanceTactic).State = EntityState.Added;
        //                    }

        //                    //// Adding fetched eloqua campaign object to list.
        //                    importEloquaCampaignList.Add(eloquaCampaign);
        //                }

        //                db.SaveChanges();

        //                //// Checking whether atleast one eloqua campaign object is fetched.
        //                if (importEloquaCampaignList.Count > 0)
        //                {
        //                    //// Creating a distinct list of campaign fetched using api.
        //                    List<string> integrationTacticIdList = importEloquaCampaignList.Select(import => import.id).Distinct().ToList();

        //                    //// Getting list of tactic whose actualCost needs to be updated.
        //                    List<Plan_Campaign_Program_Tactic> innerTacticList = tacticList.Where(t => integrationTacticIdList.Contains(t.IntegrationInstanceTacticId)).ToList();

        //                    //Added by dharmraj for ticket #733 : Actual cost - Changes related to integraton with Eloqua/SF.
        //                    List<Plan_Campaign_Program_Tactic_Actual> actualTacicList = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => integrationTacticIdList.Contains(ta.Plan_Campaign_Program_Tactic.IntegrationInstanceTacticId) && ta.StageTitle == Common.StageCost).ToList();
        //                    actualTacicList.ForEach(t => db.Entry(t).State = EntityState.Deleted);
        //                    db.SaveChanges();

        //                    //// Iterating over each tactic
        //                    foreach (var tactic in innerTacticList)
        //                    {
        //                        // Start Added by dharmraj for ticket #733 : Actual cost - Changes related to integraton with Eloqua/SF.
        //                        //// Setting actual cost
        //                        //tactic.CostActual = importEloquaCampaignList.SingleOrDefault(import => import.id == tactic.IntegrationInstanceTacticId).actualCost;
        //                        double actualValue = importEloquaCampaignList.SingleOrDefault(import => import.id == tactic.IntegrationInstanceTacticId).actualCost;
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

        //                        //// Setting log.
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
        //                    // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
        //                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, string.Empty);
        //                }
        //                else
        //                {
        //                    // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
        //                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
        //                }
        //            }
        //            catch (Exception e)
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
        /// Added By: Maninder Singh Wadhva.
        /// Function to set mapping details.
        /// </summary>
        private void SetMappingDetails()
        {
            // Start - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
            string Global = Enums.IntegrantionDataTypeMappingTableName.Global.ToString();
            string Tactic_EntityType = Enums.EntityType.Tactic.ToString();
            string Plan_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program_Tactic.ToString();
            string Plan_Improvement_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Improvement_Campaign_Program_Tactic.ToString();
            // End - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997

            // Start - Modified by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
            List<IntegrationInstanceDataTypeMapping> dataTypeMapping = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.IntegrationInstanceId.Equals(_integrationInstanceId)).ToList();
            _mappingTactic = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign_Program_Tactic
                                                || gameplandata.GameplanDataType.TableName == Global) : gameplandata.CustomField.EntityType == Tactic_EntityType) &&
                                                (gameplandata.GameplanDataType != null ? !gameplandata.GameplanDataType.IsGet : true))
                                            .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                            .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
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
            // End - Modified by Sohel Pathan on 05/12/2014 for PL ticket #995, 996, & 997

            _clientId = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;

            _mappingVertical = db.Verticals.Where(v => v.ClientId == _clientId).Select(v => new { v.VerticalId, v.Title })
                                .ToDictionary(v => v.VerticalId, v => v.Title);
            _mappingAudience = db.Audiences.Where(a => a.ClientId == _clientId).Select(a => new { a.AudienceId, a.Title })
                                .ToDictionary(a => a.AudienceId, a => a.Title);
            _mappingBusinessunit = db.BusinessUnits.Where(b => b.ClientId == _clientId).Select(b => new { b.BusinessUnitId, b.Title })
                                .ToDictionary(b => b.BusinessUnitId, b => b.Title);
            _mappingGeography = db.Geographies.Where(g => g.ClientId == _clientId).Select(g => new { g.GeographyId, g.Title })
                                .ToDictionary(g => g.GeographyId, g => g.Title);

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

            IntegrationInstanceTacticIds = new List<string>();
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to Synchronize instance data.
        /// </summary>
        private void SyncInstanceData()
        {
            List<int> planIds = db.Plans.Where(p => p.Model.IntegrationInstanceId == _integrationInstanceId && p.Model.Status.Equals("Published")).Select(p => p.PlanId).ToList();

            try
            {
                using (var scope = new TransactionScope())
                {
                    List<Plan_Campaign_Program_Tactic> tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).ToList();
                    // Start - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
                    List<int> tacticIdList = tacticList.Select(c => c.PlanTacticId).ToList();
                    CreateMappingCustomFieldDictionary(tacticIdList, Enums.EntityType.Tactic.ToString());
                    // End - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
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

            }
            catch (Exception e)
            {
                _ErrorMessage = GetErrorMessage(e);
            }
        }

        // Get All responses from integration instance external server
        private void GetDataForTacticandUpdate()
        {
            try
            {
                EloquaResponse objEloquaResponse = new EloquaResponse();
                objEloquaResponse.GetTacticResponse(_integrationInstanceId, _userId, _integrationInstanceLogId);
            }
            catch (Exception ex)
            {
                _ErrorMessage = GetErrorMessage(ex);
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

            request.AddBody(tactic);
            IRestResponse<EloquaCampaign> response = _client.Execute<EloquaCampaign>(request);

            string tactidId = string.Empty;
            if (response != null && response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == HttpStatusCode.Created)
            {
                tactidId = response.Data.id;
            }
            else
            {
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
        private EloquaCampaign GetEloquaCampaign(string elouqaCampaignId)
        {
            RestRequest request = new RestRequest(Method.GET)
            {
                Resource = "/assets/campaign/" + elouqaCampaignId,
                RequestFormat = DataFormat.Json
            };

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
            request.AddBody(tactic);

            IRestResponse<EloquaCampaign> response = _client.Execute<EloquaCampaign>(request);

            if (response.Data != null)
            {
                return response.Data.id.Equals(id);
            }
            else
            {
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
        private Plan_Improvement_Campaign_Program_Tactic SyncImprovementData(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            Enums.Mode currentMode = Common.GetMode(planIMPTactic.IsDeleted, planIMPTactic.IsDeployedToIntegration, planIMPTactic.IntegrationInstanceTacticId, planIMPTactic.Status);

            if (currentMode == Enums.Mode.Update)
            {
                int _folderId = 0;
                _folderId = GetEloquaFolderIdImprovementTactic(planIMPTactic);

                _folderId = (_folderId.ToString() == "0") ? GetEloquaRootFolderId() : _folderId;

                try
                {
                    int GPFolderId = GetEloquaCampaign(planIMPTactic.IntegrationInstanceTacticId).folderId;
                    if (GPFolderId != _folderId)
                    {
                        currentMode = Enums.Mode.Create;
                        planIMPTactic.IntegrationInstanceTacticId = null;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains(NotFound))
                    {
                        planIMPTactic.IntegrationInstanceTacticId = null;
                        planIMPTactic = SyncImprovementData(planIMPTactic);
                        return planIMPTactic;
                    }
                }
            }

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
                    planIMPTactic.IntegrationInstanceTacticId = CreateImprovementTactic(planIMPTactic);
                    planIMPTactic.LastSyncDate = DateTime.Now;
                    planIMPTactic.ModifiedDate = DateTime.Now;
                    planIMPTactic.ModifiedBy = _userId;
                    instanceLogTactic.Status = StatusResult.Success.ToString();

                    //Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                    Plan_Improvement_Campaign_Program_Tactic_Comment objImpTacticComment = new Plan_Improvement_Campaign_Program_Tactic_Comment();
                    objImpTacticComment.ImprovementPlanTacticId = planIMPTactic.ImprovementPlanTacticId;
                    objImpTacticComment.Comment = Common.ImprovementTacticSyncedComment + Integration.Helper.Enums.IntegrationType.Eloqua.ToString();
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
                    //End : Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                }
                catch (Exception e)
                {
                    instanceLogTactic.Status = StatusResult.Error.ToString();
                    instanceLogTactic.ErrorDescription = GetErrorMessage(e);
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
                        instanceLogTactic.ErrorDescription = Common.UnableToUpdate;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains(NotFound))// || e.Message.Contains(InternalServerError))
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
                        instanceLogTactic.ErrorDescription = Common.UnableToUpdate;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains(NotFound))
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
        /// Added By: Maninder Singh Wadhva
        /// Function to create improvement tactic.
        /// </summary>
        /// <param name="planIMPTactic">Improvement tactic.</param>
        /// <returns>Returns id of improvement tactic created on eloqua.</returns>
        private string CreateImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            IDictionary<string, object> tactic = GetImprovementTactic(planIMPTactic, Enums.Mode.Create);
            return CreateEloquaCampaign(tactic);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to update improvement tactictactic on eloqua.
        /// </summary>
        /// <param name="planIMPTactic">Improvement tactic.</param>
        /// <returns>Returns flag to determine whether udpate was successfull or not.</returns>
        private bool UpdateImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            IDictionary<string, object> tactic = GetImprovementTactic(planIMPTactic, Enums.Mode.Update);
            return UpdateEloquaCampaign(planIMPTactic.IntegrationInstanceTacticId, tactic);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to get improvement tactic.
        /// </summary>
        /// <param name="planIMPTactic">Improvement tactic.</param>
        /// <param name="mode">Mode of operations.</param>
        /// <returns>Returns improvement tactic.</returns>
        private IDictionary<string, object> GetImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic, Enums.Mode mode)
        {
            IDictionary<string, object> tactic = GetTargetKeyValue<Plan_Improvement_Campaign_Program_Tactic>(planIMPTactic, _mappingImprovementTactic);
            tactic.Add("type", "Campaign");
            tactic.Add("id", planIMPTactic.IntegrationInstanceTacticId);

            int _folderId = 0;
            _folderId = GetEloquaFolderIdImprovementTactic(planIMPTactic);

            if (_folderId > 0)
            {
                tactic.Add("folderId", _folderId);
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
        private IDictionary<string, object> GetTactic(Plan_Campaign_Program_Tactic planTactic, Enums.Mode mode)
        {
            IDictionary<string, object> tactic = GetTargetKeyValue<Plan_Campaign_Program_Tactic>(planTactic, _mappingTactic);
            tactic.Add("type", "Campaign");
            tactic.Add("id", planTactic.IntegrationInstanceTacticId);

            int _folderId = 0;
            _folderId = GetEloquaFolderIdTactic(planTactic);

            if (_folderId > 0)
            {
                tactic.Add("folderId", _folderId);
            }

            return tactic;
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to update plan tactic on eloqua.
        /// </summary>
        /// <param name="planTactic">Plan tactic.</param>
        /// <returns>Returns flag to determine whether udpate was successfull or not.</returns>
        private bool UpdateTactic(Plan_Campaign_Program_Tactic planTactic)
        {
            IDictionary<string, object> tactic = GetTactic(planTactic, Enums.Mode.Update);
            if (!string.IsNullOrEmpty(planTactic.TacticCustomName) && _mappingTactic.ContainsKey("Title"))
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                tactic[titleMappedValue] = planTactic.TacticCustomName;
            }
            return UpdateEloquaCampaign(planTactic.IntegrationInstanceTacticId, tactic);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to synchronize tactic data.
        /// </summary>
        /// <param name="planTactic">Plan tactic.</param>
        /// <returns>Returns updated plan tactic.</returns>
        private Plan_Campaign_Program_Tactic SyncTacticData(Plan_Campaign_Program_Tactic planTactic)
        {
            Enums.Mode currentMode = Common.GetMode(planTactic.IsDeleted, planTactic.IsDeployedToIntegration, planTactic.IntegrationInstanceTacticId, planTactic.Status);

            if (currentMode == Enums.Mode.Update)
            {
                int _folderId = 0;
                _folderId = GetEloquaFolderIdTactic(planTactic);

                _folderId = (_folderId.ToString() == "0") ? GetEloquaRootFolderId() : _folderId;

                try
                {
                    int GPFolderId = GetEloquaCampaign(planTactic.IntegrationInstanceTacticId).folderId;
                    if (GPFolderId != _folderId)
                    {
                        currentMode = Enums.Mode.Create;
                        planTactic.IntegrationInstanceTacticId = null;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains(NotFound))
                    {
                        planTactic.IntegrationInstanceTacticId = null;
                        planTactic = SyncTacticData(planTactic);
                        return planTactic;
                    }
                }
            }

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
                    planTactic.IntegrationInstanceTacticId = CreateTactic(planTactic);
                    planTactic.LastSyncDate = DateTime.Now;
                    planTactic.ModifiedDate = DateTime.Now;
                    planTactic.ModifiedBy = _userId;
                    instanceLogTactic.Status = StatusResult.Success.ToString();

                    //Added by Mitesh Vaishnav for PL Ticket 534 :When a tactic is synced a comment should be created in that tactic
                    Plan_Campaign_Program_Tactic_Comment objTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                    objTacticComment.PlanTacticId = planTactic.PlanTacticId;
                    objTacticComment.Comment = Common.TacticSyncedComment + Integration.Helper.Enums.IntegrationType.Eloqua.ToString();
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
                catch (Exception e)
                {
                    instanceLogTactic.Status = StatusResult.Error.ToString();
                    instanceLogTactic.ErrorDescription = GetErrorMessage(e);
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
                        instanceLogTactic.ErrorDescription = Common.UnableToUpdate;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains(NotFound))
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
                        instanceLogTactic.ErrorDescription = Common.UnableToDelete;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains(NotFound))
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

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to create plan tactic.
        /// </summary>
        /// <param name="planTactic">Plan tactic.</param>
        /// <returns>Returns id of tactic created on eloqua.</returns>
        private string CreateTactic(Plan_Campaign_Program_Tactic planTactic)
        {
            IDictionary<string, object> tactic = GetTactic(planTactic, Enums.Mode.Create);
            if (_mappingTactic.ContainsKey("Title") && planTactic != null && _CustomNamingPermissionForInstance && IsClientAllowedForCustomNaming)//_clientActivityList.Where(clientActivity=>clientActivity.Code==Enums.clientAcivity.CustomCampaignNameConvention.ToString()).Any() && 
            {
                string titleMappedValue = _mappingTactic["Title"].ToString();
                if (tactic.ContainsKey(titleMappedValue))
                {
                    tactic[titleMappedValue] = Common.GenerateCustomName(planTactic, planTactic.BusinessUnit.ClientId);
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
        public int GetEloquaFolderIdTactic(Plan_Campaign_Program_Tactic planTactic)
        {
            int folderId = 0;

            //// Get Plan Id for selected Tactic.
            var planId = db.Plan_Campaign.Where(pc => pc.PlanCampaignId.Equals(db.Plan_Campaign_Program.Where(prog => prog.PlanProgramId.Equals(planTactic.PlanProgramId)).Select(prog => prog.PlanCampaignId).FirstOrDefault())).Select(pc => pc.PlanId).FirstOrDefault();

            //// Get Folder Path for selected plan.
            var folderPath = db.Plans.Where(p => p.PlanId.Equals(planId)).Select(p => p.EloquaFolderPath).FirstOrDefault();

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
                IRestResponse response = SearchFolderInEloqua(folderPathArray.Last());

                //// Convert Json response into class.
                folderResponse respo = JsonConvert.DeserializeObject<folderResponse>(response.Content);

                //// If response is null skip.
                if (respo != null)
                {

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
                            return folderId = Convert.ToInt32(parentFolderId);
                        }
                        else
                        {
                            return folderId = 0;
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
                        //// default value return.
                        return 0;
                    }
                }
            }

            return folderId;
        }

        /// <summary>
        /// Search for folder Id to upload improvement tactic data in Eloqua.
        /// </summary>
        /// <param name="planIMPTactic">Improvement tactic obj.</param>
        /// <returns> Return folder Id.</returns>
        public int GetEloquaFolderIdImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            int folderId = 0;

            //// Get Plan Id for selected Tactic.
            var planId = db.Plan_Improvement_Campaign.Where(pc => pc.ImprovementPlanCampaignId.Equals(db.Plan_Improvement_Campaign_Program.Where(prog => prog.ImprovementPlanProgramId.Equals(planIMPTactic.ImprovementPlanProgramId)).Select(prog => prog.ImprovementPlanCampaignId).FirstOrDefault())).Select(pc => pc.ImprovePlanId).SingleOrDefault();

            //// Get Folder Path for selected plan.
            var folderPath = db.Plans.Where(p => p.PlanId.Equals(planId)).Select(p => p.EloquaFolderPath).FirstOrDefault();

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
                IRestResponse response = SearchFolderInEloqua(folderPathArray.Last());

                //// Convert Json response into class.
                folderResponse respo = JsonConvert.DeserializeObject<folderResponse>(response.Content);

                //// If response is null skip.
                if (respo != null)
                {

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
                            return folderId = Convert.ToInt32(parentFolderId);
                        }
                        else
                        {
                            return folderId = 0;
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
                        //// default value return.
                        return 0;
                    }
                }
            }

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

            IRestResponse response = _client.Execute(request);

            return response;
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
                    return true;
                }

                return false;
            }
            else
            {
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
            List<FieldValue> fieldValues = new List<FieldValue>();

            foreach (KeyValuePair<string, string> mapping in mappingDataType)
            {
                PropertyInfo propInfo = sourceProps.FirstOrDefault(property => property.Name.Equals(mapping.Key));
                if (propInfo != null)
                {
                    string value = Convert.ToString(propInfo.GetValue(((T)obj), null));
                    if (mapping.Key == verticalId)
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
                        value = ConvertToUnixEpoch(Convert.ToDateTime(value)).ToString(); ////Convert.ToDateTime(value).ToString("yyyy-MM-ddThh:mm:ss+hh:mm");
                    }
                    // Start - Added by Sohel Pathan on 11/09/2014 for PL ticket #773
                    else if (mapping.Key == costActual)
                    {
                        value = Common.CalculateActualCost(((Plan_Campaign_Program_Tactic)obj).PlanTacticId);
                    }
                    // End - Added by Sohel Pathan on 11/09/2014 for PL ticket #773

                    if (campaignMetadata.Contains(mapping.Value))
                    {
                        keyvaluepair.Add(mapping.Value, value);
                    }
                    else
                    {
                        var customFieldId = customFields.Where(customField => customField.Value.Equals(mapping.Value)).Select(customField => customField).FirstOrDefault();
                        if (customFieldId.Key != null && customFieldId.Value != null)
                        {
                            fieldValues.Add(new FieldValue { id = customFieldId.Key, type = "FieldValue", value = value });
                        }
                    }
                }
                // Start - Added by Sohel Pathan on 04/12/2014 for PL ticket #995, 996, & 997
                else
                {
                    var mappedData = MapCustomField<T>(obj, sourceProps, mapping);
                    if (mappedData != null)
                    {
                        fieldValues.Add(mappedData);
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
        private FieldValue MapCustomField<T>(object obj, PropertyInfo[] sourceProps, KeyValuePair<string, string> mapping)
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
                    mappingKey = mapping.Key + "-" + EntityTypeId;

                    if (_mappingCustomFields.ContainsKey(mappingKey))
                    {
                        var customFieldId = customFields.Where(customField => customField.Value.Equals(mapping.Value)).Select(customField => customField).FirstOrDefault();
                        if (customFieldId.Key != null && customFieldId.Value != null)
                        {
                            return new FieldValue { id = customFieldId.Key, type = "FieldValue", value = _mappingCustomFields[mappingKey] };
                        }
                    }
                }
            }
            return null;
        }

        //// Modified By: Maninder Singh Wadhva
        //// Modified Date: 08/11/2014  	
        //// Ticket: #675 Integration - Verify tactic data push from GP to Eloqua with recent UI changes
        //private static DateTime _unixEpochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static DateTime _unixEpochTime = new DateTime(1970, 1, 1, 0, 0, 0);

        //// Modified By: Maninder Singh Wadhva
        //// Modified Date: 08/11/2014  	
        //// Ticket: #675 Integration - Verify tactic data push from GP to Eloqua with recent UI changes
        //private static long ConvertToUnixEpoch(DateTime date)
        //{
        //    return (long)new TimeSpan(date.Ticks - _unixEpochTime.Ticks).TotalSeconds;
        //}

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

        #endregion

        /// <summary>
        /// Added By: Maninder Singh Wadhva
        /// Function to get instance url.
        /// </summary>
        /// <returns>Returns standard url.</returns>
        private string GetInstanceURL()
        {
            RestClient baseClient = AuthenticateBase();
            RestRequest request = new RestRequest(Method.GET)
            {
                Resource = "/id",
                RequestFormat = DataFormat.Json
            };

            string url = string.Empty;
            IRestResponse<InstanceURL> instanceURL = baseClient.Execute<InstanceURL>(request);
            if (instanceURL.ResponseStatus == ResponseStatus.Completed && instanceURL.StatusCode == HttpStatusCode.OK)
            {
                url = instanceURL.Data.urls.apis.rest.standard.Replace("{version}", _apiVersion);
            }
            else
            {
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
                Authenticator = new HttpBasicAuthenticator(_instance + "\\" + _username, _password)
            };
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
            var CustomFieldList = db.CustomField_Entity.Where(ce => EntityIdList.Contains(ce.EntityId) && ce.CustomField.EntityType == EntityType)
                                                        .Select(ce => new { ce.CustomField, ce.CustomFieldEntityId, ce.CustomFieldId, ce.EntityId, ce.Value }).ToList();
            List<int> CustomFieldIdList = CustomFieldList.Select(cf => cf.CustomFieldId).Distinct().ToList();
            var CustomFieldOptionList = db.CustomFieldOptions.Where(cfo => CustomFieldIdList.Contains(cfo.CustomFieldId)).Select(cfo => new { cfo.CustomFieldOptionId, cfo.Value });

            _mappingCustomFields = new Dictionary<string, string>();

            foreach (var item in CustomFieldList)
            {
                if (item.CustomField.CustomFieldType.Name == Enums.CustomFieldType.TextBox.ToString())
                {
                    _mappingCustomFields.Add(item.CustomFieldId + "-" + item.EntityId, item.Value);
                }
                else if (item.CustomField.CustomFieldType.Name == Enums.CustomFieldType.DropDownList.ToString())
                {
                    int CustomFieldOptionId = Convert.ToInt32(item.Value);
                    _mappingCustomFields.Add(item.CustomFieldId + "-" + item.EntityId, CustomFieldOptionList.Where(cfo => cfo.CustomFieldOptionId == CustomFieldOptionId).Select(cfo => cfo.Value).FirstOrDefault());
                }
            }
        }
        public string TestGenerateCustomName(Plan_Campaign_Program_Tactic planTactic, Guid clientId)
        {
            string customName = "";
            if (planTactic != null)
            {
                customName = Common.GenerateCustomName(planTactic, planTactic.BusinessUnit.ClientId);
            }
            return customName;
        }
    }
}

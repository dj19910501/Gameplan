using RevenuePlanner.Models;
using SalesforceSharp;
using SalesforceSharp.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Transactions;
using System.Dynamic;
using System.Reflection;
using Integration.BDSService;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.IO;
using System.Configuration;

namespace Integration.Salesforce
{
    public enum Mode
    {
        Create,
        Update,
        Delete,
        None
    }

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
        private List<string> IntegrationInstanceTacticIds { get; set; }
        private bool _isAuthenticated { get; set; }
        private bool _isResultError { get; set; }
        public string _ErrorMessage { get; set; }

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
            this._password = Decrypt(integrationInstance.Password); //"brijmohan";
            this._apiURL = integrationInstance.IntegrationType.APIURL; //"https://test.salesforce.com/services/oauth2/token";
        }

        /// <summary>
        /// Decrypt string
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        private static string Decrypt(string strText)
        {
            byte[] byKey = null;
            byte[] IV = { 0X12, 0X34, 0X56, 0X78, 0X90, 0XAB, 0XCD, 0XEF };
            byte[] inputByteArray = new byte[strText.Length + 1];

            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes(((string)("&%#@?,:*")).Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                inputByteArray = Convert.FromBase64String(strText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);

                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = System.Text.Encoding.UTF8;

                return encoding.GetString(ms.ToArray());

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
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

        public List<string> GetTargetDataType()
        {
            List<string> TargetDataTypeList = new List<string>();
            string metadata = _client.ReadMetaData(this.objectName);
            JObject data = JObject.Parse(metadata);
            foreach (var result in data["fields"])
            {
                TargetDataTypeList.Add((string)result["name"]);
            }
            return TargetDataTypeList.OrderBy(q => q).ToList();
        }

        public bool SyncData()
        {
            _isResultError = false;
            SetMappingDetails();

            if (EntityType.Tactic.Equals(_entityType))
            {
                List<string> statusList = GetStatusListAfterApproved();
                Plan_Campaign_Program_Tactic planTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == _id && statusList.Contains(tactic.Status)).SingleOrDefault();
                planTactic = SyncTacticData(planTactic);
                db.SaveChanges();
            }
            else if (EntityType.Program.Equals(_entityType))
            {
                List<string> statusList = GetStatusListAfterApproved();
                Plan_Campaign_Program planProgram = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == _id && statusList.Contains(program.Status)).SingleOrDefault();
                planProgram = SyncProgramData(planProgram);
                db.SaveChanges();
            }
            else if (EntityType.Campaign.Equals(_entityType))
            {
                List<string> statusList = GetStatusListAfterApproved();
                Plan_Campaign planCampaign = db.Plan_Campaign.Where(campaign => campaign.PlanCampaignId == _id && statusList.Contains(campaign.Status)).SingleOrDefault();
                planCampaign = SyncCampaingData(planCampaign);
                db.SaveChanges();
            }
            else if (EntityType.ImprovementTactic.Equals(_entityType))
            {
                List<string> statusList = GetStatusListAfterApproved();
                Plan_Improvement_Campaign_Program_Tactic planImprovementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(imptactic => imptactic.ImprovementPlanTacticId == _id && statusList.Contains(imptactic.Status)).SingleOrDefault();
                planImprovementTactic = SyncImprovementData(planImprovementTactic);
                db.SaveChanges();
            }
            else
            {
                SyncInstanceData();
            }
            // When isimport actual flag true then allow to get actual.
            bool isImport = false;
            isImport = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId.Equals(_integrationInstanceId)).IsImportActuals;
            if (isImport)
            {
                if (IntegrationInstanceTacticIds.Count > 0)
                {
                    GetDataForTacticandUpdate();
                }
            }

            return _isResultError;
        }

        private void GetDataForTacticandUpdate()
        {
            string actualCost = "CostActual";
            List<IntegrationInstanceDataTypeMapping> dataTypeMapping = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.IntegrationInstanceId.Equals(_integrationInstanceId)).ToList();
            Dictionary<string, string> mappingGetTactic = dataTypeMapping.Where(gameplandata => gameplandata.GameplanDataType.TableName == "Plan_Campaign_Program_Tactic" &&
                                                                   gameplandata.GameplanDataType.IsGet)
                                            .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                            .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

            string columnList = String.Join(",", (from r in mappingGetTactic.AsEnumerable() select r.Value));
            if (columnList != string.Empty)
            {
                columnList += " , " + ColumnId;
            }
            string integrationTacticIds = String.Join("','", (from r in IntegrationInstanceTacticIds select r));
            List<Plan_Campaign_Program_Tactic> tacticList = db.Plan_Campaign_Program_Tactic.Where(t => IntegrationInstanceTacticIds.Contains(t.IntegrationInstanceTacticId)).ToList();
            try
            {
            var AllRecords = _client.Query<object>("SELECT " + columnList + " FROM " + this.objectName + " WHERE " + ColumnId + " in ('" + integrationTacticIds + "')");

            //put below code in transaction
            List<Plan_Campaign_Program_Tactic_Actual> actualTacicList = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => IntegrationInstanceTacticIds.Contains(ta.Plan_Campaign_Program_Tactic.IntegrationInstanceTacticId)).ToList();
            actualTacicList.ForEach(t => db.Entry(t).State = EntityState.Deleted);
            db.SaveChanges();


            foreach (var resultin in AllRecords)
            {
                string TacticResult = resultin.ToString();
                JObject jobj = JObject.Parse(TacticResult);
                string idvalue = Convert.ToString(jobj[ColumnId]);
                Plan_Campaign_Program_Tactic tactic = tacticList.Where(t => t.IntegrationInstanceTacticId == idvalue).Single();
                tactic.CostActual = 0;
                tactic.INQsActual = 0;
                tactic.MQLsActual = 0;
                tactic.CWsActual = 0;
                tactic.RevenuesActual = 0;

                foreach (var mapping in mappingGetTactic)
                {
                    if (jobj[mapping.Value] != null && Convert.ToString(jobj[mapping.Value]) != string.Empty)
                    {
                        if (mapping.Key == actualCost)
                        {

                            int index = tacticList.IndexOf(tactic);
                            if (index >= 0)
                            {
                                tactic.CostActual = Convert.ToInt32(jobj[mapping.Value]);
                                tacticList[index] = tactic;
                            }
                        }
                        else
                        {
                            double actualValue = Convert.ToDouble(jobj[mapping.Value]);
                            int totalMonth = 0;
                            if (tactic.StartDate.Month == tactic.EndDate.Month)
                            {
                                totalMonth = 1;
                            }
                            else
                            {
                                totalMonth = tactic.EndDate.Month - tactic.StartDate.Month + 1;
                            }
                            for (int iMonth = tactic.StartDate.Month; iMonth <= tactic.EndDate.Month; iMonth++)
                            {
                                Plan_Campaign_Program_Tactic_Actual actualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                actualTactic.Actualvalue = Math.Round(actualValue / totalMonth);
                                actualTactic.PlanTacticId = tactic.PlanTacticId;
                                actualTactic.Period = "Y" + iMonth;
                                actualTactic.StageTitle = mapping.Key;
                                //change date & created by
                                actualTactic.CreatedDate = DateTime.Now;
                                    actualTactic.CreatedBy = _userId;
                                db.Entry(actualTactic).State = EntityState.Added;
                            }

                            if (mapping.Key == StageValue.INQ.ToString())
                            {
                                tactic.INQsActual = Convert.ToInt32(actualValue);
                            }
                            else if (mapping.Key == StageValue.MQL.ToString())
                            {
                                tactic.MQLsActual = actualValue;
                            }
                            else if (mapping.Key == StageValue.CW.ToString())
                            {
                                tactic.CWsActual = actualValue;
                            }
                            else if (mapping.Key == StageValue.Revenue.ToString())
                            {
                                tactic.RevenuesActual = actualValue;
                            }
                        }
                    }
                }

                    tactic.LastSyncDate = DateTime.Now;
                    tactic.ModifiedDate = DateTime.Now;
                    tactic.ModifiedBy = _userId;

                    IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                    instanceTactic.IntegrationInstanceLogId = _integrationInstanceLogId;
                    instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                    instanceTactic.EntityId = tactic.PlanTacticId;
                    instanceTactic.EntityType = EntityType.Tactic.ToString();
                    instanceTactic.Status = StatusResult.Success.ToString();
                    instanceTactic.Operation = Operation.Import_Actuals.ToString();
                    instanceTactic.SyncTimeStamp = DateTime.Now;
                    instanceTactic.CreatedDate = DateTime.Now;
                    instanceTactic.CreatedBy = _userId;
                    db.Entry(instanceTactic).State = EntityState.Added;
            }
            db.SaveChanges();
            }
            catch (SalesforceException e)
            {
                string msg = GetErrorMessage(e);
                foreach (var t in tacticList)
                {
                    IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                    instanceTactic.IntegrationInstanceLogId = _integrationInstanceLogId;
                    instanceTactic.IntegrationInstanceId = _integrationInstanceId;
                    instanceTactic.EntityId = t.PlanTacticId;
                    instanceTactic.EntityType = EntityType.Tactic.ToString();
                    instanceTactic.Status = StatusResult.Error.ToString();
                    instanceTactic.ErrorDescription = msg;
                    instanceTactic.Operation = Operation.Import_Actuals.ToString();
                    instanceTactic.SyncTimeStamp = DateTime.Now;
                    instanceTactic.CreatedDate = DateTime.Now;
                    instanceTactic.CreatedBy = _userId;
                    db.Entry(instanceTactic).State = EntityState.Added;
                }
                db.SaveChanges();
            }
        }

        private void SetMappingDetails()
        {
            List<IntegrationInstanceDataTypeMapping> dataTypeMapping = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.IntegrationInstanceId.Equals(_integrationInstanceId)).ToList();
            _mappingTactic = dataTypeMapping.Where(gameplandata => gameplandata.GameplanDataType.TableName == "Plan_Campaign_Program_Tactic" &&
                                                                   !gameplandata.GameplanDataType.IsStage &&
                                                                   !gameplandata.GameplanDataType.IsGet)
                                            .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                            .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

            _mappingProgram = dataTypeMapping.Where(gameplandata => gameplandata.GameplanDataType.TableName == "Plan_Campaign_Program" &&
                                                       !gameplandata.GameplanDataType.IsStage &&
                                                       !gameplandata.GameplanDataType.IsGet)
                                .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

            _mappingCampaign = dataTypeMapping.Where(gameplandata => gameplandata.GameplanDataType.TableName == "Plan_Campaign" &&
                                           !gameplandata.GameplanDataType.IsStage &&
                                           !gameplandata.GameplanDataType.IsGet)
                    .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                    .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

            _mappingImprovementTactic = dataTypeMapping.Where(gameplandata => gameplandata.GameplanDataType.TableName == "Plan_Improvement_Campaign_Program_Tactic" &&
                                                                   !gameplandata.GameplanDataType.IsStage &&
                                                                   !gameplandata.GameplanDataType.IsGet)
                                            .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                            .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

            _mappingImprovementProgram = dataTypeMapping.Where(gameplandata => gameplandata.GameplanDataType.TableName == "Plan_Improvement_Campaign_Program" &&
                                                                  !gameplandata.GameplanDataType.IsStage &&
                                                                  !gameplandata.GameplanDataType.IsGet)
                                           .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                           .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

            _mappingImprovementCampaign = dataTypeMapping.Where(gameplandata => gameplandata.GameplanDataType.TableName == "Plan_Improvement_Campaign" &&
                                                                  !gameplandata.GameplanDataType.IsStage &&
                                                                  !gameplandata.GameplanDataType.IsGet)
                                           .Select(mapping => new { mapping.GameplanDataType.ActualFieldName, mapping.TargetDataType })
                                           .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

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
            _mappingUser = objBDSservice.GetUserListByClientId(clientId, BDSApplicationCode).Select(u => new { u.UserId, u.FirstName, u.LastName }).ToDictionary(u => u.UserId, u => u.FirstName + "" + u.LastName);

            IntegrationInstanceTacticIds = new List<string>();
        }

        private Plan_Campaign SyncCampaingData(Plan_Campaign planCampaign)
        {
            //// Get campaign based on _id property.
            Mode currentMode = GetMode(planCampaign.IsDeleted, planCampaign.IsDeployedToIntegration, planCampaign.IntegrationInstanceCampaignId, planCampaign.Status);
            if (currentMode.Equals(Mode.Create))
            {
                IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                instanceLogCampaign.IntegrationInstanceLogId = _integrationInstanceLogId;
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
            else if (currentMode.Equals(Mode.Update))
            {
                IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                instanceLogCampaign.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                    instanceLogCampaign.Status = StatusResult.Error.ToString();
                    instanceLogCampaign.ErrorDescription = GetErrorMessage(e);
                }

                instanceLogCampaign.CreatedBy = this._userId;
                instanceLogCampaign.CreatedDate = DateTime.Now;
                db.Entry(instanceLogCampaign).State = EntityState.Added;
            }
            else if (currentMode.Equals(Mode.Delete))
            {
                // Set null value if delete true to integrationinstance..id
                var tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.PlanCampaignId == planCampaign.PlanCampaignId && tactic.IntegrationInstanceTacticId != null).ToList();
                foreach (var t in tacticList)
                {
                    IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                    instanceLogTactic.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                    }

                    instanceLogTactic.CreatedBy = this._userId;
                    instanceLogTactic.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogTactic).State = EntityState.Added;
                }

                // Set null value if delete true to integrationinstance..id
                var programList = db.Plan_Campaign_Program.Where(program => program.PlanCampaignId == planCampaign.PlanCampaignId && program.IntegrationInstanceProgramId != null).ToList();
                foreach (var p in programList)
                {
                    IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
                    instanceLogProgram.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                        instanceLogProgram.Status = StatusResult.Error.ToString();
                        instanceLogProgram.ErrorDescription = GetErrorMessage(e);
                    }

                    instanceLogProgram.CreatedBy = this._userId;
                    instanceLogProgram.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogProgram).State = EntityState.Added;
                }

                // Set null value if delete true to integrationinstance..id
                IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                instanceLogCampaign.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                    instanceLogCampaign.Status = StatusResult.Error.ToString();
                    instanceLogCampaign.ErrorDescription = GetErrorMessage(e);
                }

                instanceLogCampaign.CreatedBy = this._userId;
                instanceLogCampaign.CreatedDate = DateTime.Now;
                db.Entry(instanceLogCampaign).State = EntityState.Added;

            }

            return planCampaign;
        }

        private Plan_Campaign_Program SyncProgramData(Plan_Campaign_Program planProgram)
        {
            //// Get program based on _id property.
            Mode currentMode = GetMode(planProgram.IsDeleted, planProgram.IsDeployedToIntegration, planProgram.IntegrationInstanceProgramId, planProgram.Status);
            if (currentMode.Equals(Mode.Create))
            {
                Plan_Campaign planCampaign = planProgram.Plan_Campaign;
                _parentId = planCampaign.IntegrationInstanceCampaignId;
                if (string.IsNullOrWhiteSpace(_parentId))
                {
                    IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                    instanceLogCampaign.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                    instanceLogProgram.IntegrationInstanceLogId = _integrationInstanceLogId;
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
            else if (currentMode.Equals(Mode.Update))
            {
                IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
                instanceLogProgram.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                    instanceLogProgram.Status = StatusResult.Error.ToString();
                    instanceLogProgram.ErrorDescription = GetErrorMessage(e);
                }

                instanceLogProgram.CreatedBy = this._userId;
                instanceLogProgram.CreatedDate = DateTime.Now;
                db.Entry(instanceLogProgram).State = EntityState.Added;
            }
            else if (currentMode.Equals(Mode.Delete))
            {
                var tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanProgramId == planProgram.PlanProgramId && tactic.IntegrationInstanceTacticId != null).ToList();
                foreach (var t in tacticList)
                {
                    IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                    instanceLogTactic.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                        instanceLogTactic.Status = StatusResult.Error.ToString();
                        instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                    }

                    instanceLogTactic.CreatedBy = this._userId;
                    instanceLogTactic.CreatedDate = DateTime.Now;
                    db.Entry(instanceLogTactic).State = EntityState.Added;
                }

                IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
                instanceLogProgram.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                    instanceLogProgram.Status = StatusResult.Error.ToString();
                    instanceLogProgram.ErrorDescription = GetErrorMessage(e);
                }

                instanceLogProgram.CreatedBy = this._userId;
                instanceLogProgram.CreatedDate = DateTime.Now;
                db.Entry(instanceLogProgram).State = EntityState.Added;
            }

            return planProgram;
        }

        private Plan_Campaign_Program_Tactic SyncTacticData(Plan_Campaign_Program_Tactic planTactic)
        {
            Mode currentMode = GetMode(planTactic.IsDeleted, planTactic.IsDeployedToIntegration, planTactic.IntegrationInstanceTacticId, planTactic.Status);
            if (currentMode.Equals(Mode.Create))
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
                        instanceLogCampaign.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                        instanceLogProgram.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                    instanceLogTactic.IntegrationInstanceLogId = _integrationInstanceLogId;
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
            else if (currentMode.Equals(Mode.Update))
            {
                IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                instanceLogTactic.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                    instanceLogTactic.Status = StatusResult.Error.ToString();
                    instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }
            else if (currentMode.Equals(Mode.Delete))
            {
                IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                instanceLogTactic.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                    instanceLogTactic.Status = StatusResult.Error.ToString();
                    instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }

            return planTactic;
        }

        private Plan_Improvement_Campaign SyncImprovementCampaingData(Plan_Improvement_Campaign planIMPCampaign)
        {
            if (!string.IsNullOrWhiteSpace(planIMPCampaign.IntegrationInstanceCampaignId))
            {
                if (planIMPCampaign.Plan.IsDeleted)
                {
                    // Set null value if delete true to integrationinstance..id
                    var tacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId == planIMPCampaign.ImprovementPlanCampaignId && tactic.IntegrationInstanceTacticId != null).ToList();
                    foreach (var t in tacticList)
                    {
                        IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                        instanceLogTactic.IntegrationInstanceLogId = _integrationInstanceLogId;
                        instanceLogTactic.IntegrationInstanceId = _integrationInstanceId;
                        instanceLogTactic.EntityId = t.ImprovementPlanTacticId;
                        instanceLogTactic.EntityType = EntityType.ImprovementTactic.ToString();
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
                            instanceLogTactic.Status = StatusResult.Error.ToString();
                            instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                        }

                        instanceLogTactic.CreatedBy = this._userId;
                        instanceLogTactic.CreatedDate = DateTime.Now;
                        db.Entry(instanceLogTactic).State = EntityState.Added;
                    }

                    // Set null value if delete true to integrationinstance..id
                    var programList = db.Plan_Improvement_Campaign_Program.Where(program => program.ImprovementPlanCampaignId == planIMPCampaign.ImprovementPlanCampaignId && program.IntegrationInstanceProgramId != null).ToList();
                    foreach (var p in programList)
                    {
                        IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
                        instanceLogProgram.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                            instanceLogProgram.Status = StatusResult.Error.ToString();
                            instanceLogProgram.ErrorDescription = GetErrorMessage(e);
                        }

                        instanceLogProgram.CreatedBy = this._userId;
                        instanceLogProgram.CreatedDate = DateTime.Now;
                        db.Entry(instanceLogProgram).State = EntityState.Added;
                    }


                    // Set null value if delete true to integrationinstance..id
                    IntegrationInstancePlanEntityLog instanceLogCampaign = new IntegrationInstancePlanEntityLog();
                    instanceLogCampaign.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                        instanceLogCampaign.Status = StatusResult.Error.ToString();
                        instanceLogCampaign.ErrorDescription = GetErrorMessage(e);
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
            Mode currentMode = GetMode(planIMPTactic.IsDeleted, planIMPTactic.IsDeployedToIntegration, planIMPTactic.IntegrationInstanceTacticId, planIMPTactic.Status);
            if (currentMode.Equals(Mode.Create))
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
                        instanceLogCampaign.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                        instanceLogProgram.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                    instanceLogTactic.IntegrationInstanceLogId = _integrationInstanceLogId;
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
            else if (currentMode.Equals(Mode.Update))
            {
                IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                instanceLogTactic.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                    instanceLogTactic.Status = StatusResult.Error.ToString();
                    instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }
            else if (currentMode.Equals(Mode.Delete))
            {
                IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
                instanceLogTactic.IntegrationInstanceLogId = _integrationInstanceLogId;
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
                    instanceLogTactic.Status = StatusResult.Error.ToString();
                    instanceLogTactic.ErrorDescription = GetErrorMessage(e);
                }

                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }

            return planIMPTactic;
        }

        private void SyncInstanceData()
        {
            List<int> planIds = db.Plans.Where(p => p.Model.IntegrationInstanceId == _integrationInstanceId && p.Model.Status.Equals("Published")).Select(p => p.PlanId).ToList();
            List<string> statusList = GetStatusListAfterApproved();

            try
            {
                using (var scope = new TransactionScope())
                {
                    List<Plan_Campaign> campaignList = db.Plan_Campaign.Where(campaign => planIds.Contains(campaign.PlanId) && statusList.Contains(campaign.Status)).ToList();
                    for (int index = 0; index < campaignList.Count; index++)
                    {
                        campaignList[index] = SyncCampaingData(campaignList[index]);
                    }
                    db.SaveChanges();
                    scope.Complete();
                }

                using (var scope = new TransactionScope())
                {
                    List<Plan_Campaign_Program> programList = db.Plan_Campaign_Program.Where(program => planIds.Contains(program.Plan_Campaign.PlanId) && statusList.Contains(program.Status)).ToList();
                    for (int index = 0; index < programList.Count; index++)
                    {
                        programList[index] = SyncProgramData(programList[index]);
                    }
                    db.SaveChanges();
                    scope.Complete();
                }

                using (var scope = new TransactionScope())
                {
                    List<Plan_Campaign_Program_Tactic> tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && statusList.Contains(tactic.Status)).ToList();
                    for (int index = 0; index < tacticList.Count; index++)
                    {
                        tacticList[index] = SyncTacticData(tacticList[index]);
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

                using (var scope = new TransactionScope())
                {
                    List<Plan_Improvement_Campaign_Program_Tactic> tacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(tactic => planIds.Contains(tactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId) && statusList.Contains(tactic.Status)).ToList();
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
                throw;
            }
        }

        private Mode GetMode(bool isDeleted, bool isDeployedToIntegration, string integrationInstanceTacticId, string status)
        {
            if (!isDeleted && isDeployedToIntegration && string.IsNullOrWhiteSpace(integrationInstanceTacticId))
            {
                return Mode.Create;
            }
            else if (!isDeleted && isDeployedToIntegration && !string.IsNullOrWhiteSpace(integrationInstanceTacticId))
            {
                return Mode.Update;
            }
            else if (isDeleted && !string.IsNullOrWhiteSpace(integrationInstanceTacticId))
            {
                return Mode.Delete;
            }
            else if (!isDeleted && !string.IsNullOrWhiteSpace(integrationInstanceTacticId))
            {
                return Mode.Delete;
            }
            else
            {
                return Mode.None;
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
            Dictionary<string, object> program = GetProgram(planProgram, Mode.Create);
            string programId = _client.Create(objectName, program);
            return programId;
        }

        private string CreateTactic(Plan_Campaign_Program_Tactic planTactic)
        {
            Dictionary<string, object> tactic = GetTactic(planTactic, Mode.Create);
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
            Dictionary<string, object> program = GetImprovementProgram(planIMPProgram, Mode.Create);
            string programId = _client.Create(objectName, program);
            return programId;
        }

        private string CreateImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            Dictionary<string, object> tactic = GetImprovementTactic(planIMPTactic, Mode.Create);
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
            Dictionary<string, object> program = GetProgram(planProgram, Mode.Update);
            return _client.Update(objectName, planProgram.IntegrationInstanceProgramId, program);
        }

        private bool UpdateTactic(Plan_Campaign_Program_Tactic planTactic)
        {
            Dictionary<string, object> tactic = GetTactic(planTactic, Mode.Update);
            IntegrationInstanceTacticIds.Add(Convert.ToString(planTactic.IntegrationInstanceTacticId));
            return _client.Update(objectName, planTactic.IntegrationInstanceTacticId, tactic);
        }

        private bool UpdateImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic)
        {
            Dictionary<string, object> tactic = GetImprovementTactic(planIMPTactic, Mode.Update);
            return _client.Update(objectName, planIMPTactic.IntegrationInstanceTacticId, tactic);
        }

        private bool Delete(string recordid)
        {
            return _client.Delete(objectName, recordid);
        }

        private static List<string> GetStatusListAfterApproved()
        {
            List<string> tacticStatus = new List<string>();
            tacticStatus.Add(ExternalIntegration.TacticStatusValues[TacticStatus.Approved.ToString()].ToString());
            tacticStatus.Add(ExternalIntegration.TacticStatusValues[TacticStatus.InProgress.ToString()].ToString());
            tacticStatus.Add(ExternalIntegration.TacticStatusValues[TacticStatus.Complete.ToString()].ToString());
            tacticStatus.Add(ExternalIntegration.TacticStatusValues[TacticStatus.Decline.ToString()].ToString());
            return tacticStatus;
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

        private Dictionary<string, object> GetProgram(Plan_Campaign_Program planProgram, Mode mode)
        {
            Dictionary<string, object> program = GetTargetKeyValue<Plan_Campaign_Program>(planProgram, _mappingProgram);

            if (mode.Equals(Mode.Create))
            {
                program.Add(ColumnParentId, _parentId);
            }
            return program;
        }

        private Dictionary<string, object> GetTactic(Plan_Campaign_Program_Tactic planTactic, Mode mode)
        {
            Dictionary<string, object> tactic = GetTargetKeyValue<Plan_Campaign_Program_Tactic>(planTactic, _mappingTactic);
            if (mode.Equals(Mode.Create))
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

        private Dictionary<string, object> GetImprovementProgram(Plan_Improvement_Campaign_Program planIMPProgram, Mode mode)
        {
            Dictionary<string, object> program = GetTargetKeyValue<Plan_Improvement_Campaign_Program>(planIMPProgram, _mappingImprovementProgram);

            if (mode.Equals(Mode.Create))
            {
                program.Add(ColumnParentId, _parentId);
            }
            return program;
        }

        private Dictionary<string, object> GetImprovementTactic(Plan_Improvement_Campaign_Program_Tactic planIMPTactic, Mode mode)
        {
            Dictionary<string, object> tactic = GetTargetKeyValue<Plan_Improvement_Campaign_Program_Tactic>(planIMPTactic, _mappingImprovementTactic);
            if (mode.Equals(Mode.Create))
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

                    keyvaluepair.Add(mapping.Value, value);
                }
            }

            return keyvaluepair;
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
    }
}

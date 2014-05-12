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

namespace Integration.Salesforce
{
    public enum Mode
    {
        Create,
        Update,
        Delete,
        None
    }

    public class IntegrationSalesforceClient
    {
        int _integrationInstanceId { get; set; }
        int _id { get; set; }
        EntityType _entityType { get; set; }
        string _parentId { get; set; }

        string _username { get; set; }
        string _password { get; set; }
        string _consumerKey { get; set; }
        string _consumerSecret { get; set; }
        string _securityToken { get; set; }
        string _apiURL { get; set; }
      
        SalesforceClient _client { get; set; }

        readonly string objectName;
        Dictionary<string, string> _mappingCampaign { get; set; }
        Dictionary<string, string> _mappingProgram { get; set; }
        Dictionary<string, string> _mappingTactic { get; set; }
        Dictionary<string, string> _mappingImprovement { get; set; }
        Dictionary<int, string> _mappingVertical { get; set; }
        Dictionary<int, string> _mappingAudience { get; set; }
        Dictionary<Guid, string> _mappingGeography { get; set; }
        Dictionary<Guid, string> _mappingBusinessunit { get; set; }
        Dictionary<Guid, string> _mappingUser { get; set; }

        MRPEntities db = new MRPEntities();
        string ColumnParentId = "ParentId";

        public bool _isAuthenticated { get; set; }

        public IntegrationSalesforceClient(int integrationInstanceId, int id, EntityType entityType)
        {
            _integrationInstanceId = integrationInstanceId;
            _id = id;
            _entityType = entityType;
            this.objectName = "Campaign";

            SetIntegrationInstanceDetail();
            //// Authenticate
            _isAuthenticated = this.Authenticate();
        }

        public IntegrationSalesforceClient(Dictionary<string, string> integrationInstanceDetail)
        {
            SetIntegrationInstanceDetail(integrationInstanceDetail);
            //// Authenticate
            _isAuthenticated = this.Authenticate();
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
            this._password = integrationInstance.Password; //"brijmohan";
            this._apiURL = integrationInstance.IntegrationType.APIURL; //"https://test.salesforce.com/services/oauth2/token";
        }

        private void SetIntegrationInstanceDetail(Dictionary<string, string> integrationInstanceDetail)
        {
            string ConsumerKey = "ConsumerKey";
            string ConsumerSecret = "ConsumerSecret";
            string SecurityToken = "SecurityToken";
            string Username = "Username";
            string Password = "Password";
            string APIURL = "APIURL";

            //// set below properties using dictionary.
            this._securityToken = integrationInstanceDetail[SecurityToken]; //"6to1YjygSTkAiZUusnuoJBAN";
            this._consumerKey = integrationInstanceDetail[ConsumerKey]; // "3MVG9zJJ_hX_0bb.x24JN3A5KwgO2gmkr5JfDDUx6U8FrvE_cFweCf7y3OkkLZeSkQDraDWZIrFcNqSvnAil_";
            this._consumerSecret = integrationInstanceDetail[ConsumerSecret];  //"2775499149223461438";
            this._username = integrationInstanceDetail[Username];//"brijmohan.bhavsar@indusa.com";
            this._password = integrationInstanceDetail[Password]; //"brijmohan";
            this._apiURL = integrationInstanceDetail[APIURL]; //"https://test.salesforce.com/services/oauth2/token";
        }

        public bool Authenticate()
        {
            _client = new SalesforceClient();
            var authFlow = new UsernamePasswordAuthenticationFlow(_consumerKey, _consumerSecret, _username, _password + _securityToken);
            authFlow.TokenRequestEndpointUrl = _apiURL;
            try
            {
                _client.Authenticate(authFlow);
                return true;
            }
            catch (SalesforceException ex)
            {
                //Console.WriteLine("Authentication failed: {0} : {1}", ex.Error, ex.Message);
                return false;
            }
        }

        public void SyncData()
        {
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
            else
            {
               SyncInstanceData();
            }
        }

        private void SetMappingDetails()
        {
            List<IntegrationInstanceDataTypeMapping> dataTypeMapping = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.GameplanDataType.IntegrationTypeId.Equals(2)).ToList();
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
            Guid applicationId = Guid.Parse("1c10d4b9-7931-4a7c-99e9-a158ce158951");
            _mappingUser = objBDSservice.GetUserListByClientId(clientId, applicationId).Select(u => new { u.UserId, u.FirstName, u.LastName}).ToDictionary(u => u.UserId, u => u.FirstName + "" + u.LastName);
            
        }

        private void SyncTacticData()
        {
            try
            {
                using (var scope = new TransactionScope())
                {

                    //// Get tactic based on _id property.
                    Plan_Campaign_Program_Tactic planTactic = db.Plan_Campaign_Program_Tactic.Single(tactic => tactic.PlanTacticId == _id);
                    if (planTactic.Status.Equals("In-Progress") || planTactic.Status.Equals("Approved"))
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
                                    _parentId = CreateCampaign(planCampaign);
                                    planCampaign.IntegrationInstanceCampaignId = _parentId;
                                    db.Entry(planCampaign).State = EntityState.Modified;
                                    db.SaveChanges();
                                }

                                _parentId = CreateProgram(planProgram);
                                planProgram.IntegrationInstanceProgramId = _parentId;
                                db.Entry(planProgram).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            planTactic.IntegrationInstanceTacticId = CreateTactic(planTactic);
                            db.Entry(planTactic).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (currentMode.Equals(Mode.Update))
                        {
                            if (UpdateTactic(planTactic))
                            {
                                // Get data & update actual in actual table & actual cost
                                //dynamic objtactic = new ExpandoObject();
                                //objTactic = _client.FindById<objtactic>(planTactic.IntegrationInstanceTacticId);
                                //db.Entry(planTactic).State = EntityState.Modified;
                                //db.SaveChanges();
                            }
                        }
                        else if (currentMode.Equals(Mode.Delete))
                        {
                            planTactic.IntegrationInstanceTacticId = Delete(planTactic.IntegrationInstanceTacticId);
                        }
                    }
                    scope.Complete();
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }

        private void SyncProgramData()
        {
            try
            {
                using (var scope = new TransactionScope())
                {

                    //// Get program based on _id property.
                    Plan_Campaign_Program planProgram = db.Plan_Campaign_Program.Single(program => program.PlanProgramId == _id);
                    if (planProgram.Status.Equals("In-Progress") || planProgram.Status.Equals("Approved"))
                    {
                        Mode currentMode = GetMode(planProgram.IsDeleted, planProgram.IsDeployedToIntegration, planProgram.IntegrationInstanceProgramId, planProgram.Status);
                        if (currentMode.Equals(Mode.Create))
                        {
                            Plan_Campaign planCampaign = planProgram.Plan_Campaign;
                            _parentId = planCampaign.IntegrationInstanceCampaignId;
                            if (string.IsNullOrWhiteSpace(_parentId))
                            {
                                _parentId = CreateCampaign(planCampaign);
                                planCampaign.IntegrationInstanceCampaignId = _parentId;
                                db.Entry(planCampaign).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            planProgram.IntegrationInstanceProgramId = CreateProgram(planProgram);
                            db.Entry(planProgram).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (currentMode.Equals(Mode.Update))
                        {
                            if (UpdateProgram(planProgram))
                            {
                            }
                        }
                        else if (currentMode.Equals(Mode.Delete))
                        {

                            var tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanProgramId == planProgram.PlanProgramId && tactic.IntegrationInstanceTacticId != null).ToList();
                            tacticList.ForEach(t => { t.IntegrationInstanceTacticId = Delete(t.IntegrationInstanceTacticId); });

                            planProgram.IntegrationInstanceProgramId = Delete(planProgram.IntegrationInstanceProgramId);

                            db.SaveChanges();
                        }
                    }
                    scope.Complete();
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }

        private void SyncCampaingData()
        {
            try
            {
                using (var scope = new TransactionScope())
                {

                    //// Get campaign based on _id property.
                    Plan_Campaign planCampaign = db.Plan_Campaign.Single(campaign => campaign.PlanCampaignId == _id);
                    if (planCampaign.Status.Equals("In-Progress") || planCampaign.Status.Equals("Approved"))
                    {
                        Mode currentMode = GetMode(planCampaign.IsDeleted, planCampaign.IsDeployedToIntegration, planCampaign.IntegrationInstanceCampaignId, planCampaign.Status);
                        if (currentMode.Equals(Mode.Create))
                        {
                            planCampaign.IntegrationInstanceCampaignId = CreateCampaign(planCampaign);
                            db.Entry(planCampaign).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (currentMode.Equals(Mode.Update))
                        {
                            if (UpdateCampaign(planCampaign))
                            {
                            }
                        }
                        else if (currentMode.Equals(Mode.Delete))
                        {
                            var tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.PlanCampaignId == planCampaign.PlanCampaignId && tactic.IntegrationInstanceTacticId != null).ToList();
                            tacticList.ForEach(t => { t.IntegrationInstanceTacticId = Delete(t.IntegrationInstanceTacticId); });

                            var programList = db.Plan_Campaign_Program.Where(program => program.PlanCampaignId == planCampaign.PlanCampaignId && program.IntegrationInstanceProgramId != null).ToList();
                            programList.ForEach(p => { p.IntegrationInstanceProgramId = Delete(p.IntegrationInstanceProgramId); });

                            planCampaign.IntegrationInstanceCampaignId = Delete(planCampaign.IntegrationInstanceCampaignId);

                            db.SaveChanges();
                        }
                    }
                    scope.Complete();
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }

        private Plan_Campaign SyncCampaingData(Plan_Campaign planCampaign)
        {
            //// Get campaign based on _id property.
            Mode currentMode = GetMode(planCampaign.IsDeleted, planCampaign.IsDeployedToIntegration, planCampaign.IntegrationInstanceCampaignId, planCampaign.Status);
            if (currentMode.Equals(Mode.Create))
            {
                planCampaign.IntegrationInstanceCampaignId = CreateCampaign(planCampaign);
            }
            else if (currentMode.Equals(Mode.Update))
            {
                if (UpdateCampaign(planCampaign))
                {
                }
            }
            else if (currentMode.Equals(Mode.Delete))
            {
                // Set null value if delete true to integrationinstance..id
                var tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.PlanCampaignId == planCampaign.PlanCampaignId && tactic.IntegrationInstanceTacticId != null).ToList();
                tacticList.ForEach(t => { t.IntegrationInstanceTacticId = Delete(t.IntegrationInstanceTacticId); });

                // Set null value if delete true to integrationinstance..id
                var programList = db.Plan_Campaign_Program.Where(program => program.PlanCampaignId == planCampaign.PlanCampaignId && program.IntegrationInstanceProgramId != null).ToList();
                programList.ForEach(p => { p.IntegrationInstanceProgramId = Delete(p.IntegrationInstanceProgramId); });

                db.SaveChanges();

                // Set null value if delete true to integrationinstance..id
                planCampaign.IntegrationInstanceCampaignId = Delete(planCampaign.IntegrationInstanceCampaignId);
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
                    _parentId = CreateCampaign(planCampaign);
                    planCampaign.IntegrationInstanceCampaignId = _parentId;
                    db.Entry(planCampaign).State = EntityState.Modified;
                    db.SaveChanges();
                }

                planProgram.IntegrationInstanceProgramId = CreateProgram(planProgram);
            }
            else if (currentMode.Equals(Mode.Update))
            {
                if (UpdateProgram(planProgram))
                {
                }
            }
            else if (currentMode.Equals(Mode.Delete))
            {
                var tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanProgramId == planProgram.PlanProgramId && tactic.IntegrationInstanceTacticId != null).ToList();
                tacticList.ForEach(t => { t.IntegrationInstanceTacticId = Delete(t.IntegrationInstanceTacticId); });
                db.SaveChanges();

                planProgram.IntegrationInstanceProgramId = Delete(planProgram.IntegrationInstanceProgramId); ;
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
                        _parentId = CreateCampaign(planCampaign);
                        planCampaign.IntegrationInstanceCampaignId = _parentId;
                        db.Entry(planCampaign).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    _parentId = CreateProgram(planProgram);
                    planProgram.IntegrationInstanceProgramId = _parentId;
                    db.Entry(planProgram).State = EntityState.Modified;
                    db.SaveChanges();
                }

                planTactic.IntegrationInstanceTacticId = CreateTactic(planTactic);
            }
            else if (currentMode.Equals(Mode.Update))
            {
                if (UpdateTactic(planTactic))
                {
                    // Get data & update actual in actual table & actual cost
                    //dynamic objtactic = new ExpandoObject();
                    //objTactic = _client.FindById<objtactic>(planTactic.IntegrationInstanceTacticId);
                    //db.Entry(planTactic).State = EntityState.Modified;
                    //db.SaveChanges();
                }
            }
            else if (currentMode.Equals(Mode.Delete))
            {
                planTactic.IntegrationInstanceTacticId = Delete(planTactic.IntegrationInstanceTacticId);
            }

            return planTactic;
        }

        private void SyncInstanceData()
        {
            List<int> planIds = db.Plans.Where(p => p.Model.IntegrationInstanceId == _integrationInstanceId && p.Model.Status.Equals("Published")).Select(p => p.PlanId).ToList();
            //planIds.Add(235);
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
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private Mode GetMode(bool isDeleted, bool isDeployedToIntegration, string integrationInstanceTacticId, string status)
        {
            if (status == ExternalIntegration.TacticStatusValues[TacticStatus.Decline.ToString()].ToString() && !string.IsNullOrWhiteSpace(integrationInstanceTacticId))
            {
                return Mode.Delete;
            }
            else if (!isDeleted && isDeployedToIntegration && string.IsNullOrWhiteSpace(integrationInstanceTacticId))
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
            return _client.Update(objectName, planTactic.IntegrationInstanceTacticId, tactic);
        }

        private string Delete(string recordid)
        {
            if (_client.Delete(objectName, recordid))
            {
                return null;
            }
            else
            {
                return recordid;
            }
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
                    else if (mapping.Key == statDate || mapping.Key == endDate)
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

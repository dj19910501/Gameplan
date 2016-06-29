using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using Integration.Helper;
using System.Collections.Generic;
using System.Linq;
using RevenuePlanner.Models;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Web;


namespace Integration.WorkFront
{

    /// <summary>
    /// Session class control the entire session. Highest level for actions that can be completed in the session.
    /// Creates the ClientHandling instance using appropriate credentials.
    /// Some actions include searching for projects, editing projects, deleting projects, attaching templates, etc.
    /// Actions called in this class call the WorkFrontClientHandling instance
    /// IntegrationWorkFrontSession class intended to be the layer below the integration interface and should be the 
    /// layer to handle all WorkFront API related exceptions. 
    /// Merged into Gp project->IntegrationWorkFrontSession throws ClientException to integrate exception handling
    /// into Gp exception handling.
    /// </summary>
    public class IntegrationWorkFrontSession
    {
        private ClientHandling client;
        private EntityType _entityType { get; set; }
        private int _entityID { get; set; }
        private JToken _user { get; set; }
        private String _userGroupID{ get; set; } //can be used to find projects attached to the users group
        private string _username { get; set; }
        private string _password { get; set; }
        private string _companyName { get; set; }
        private Guid _applicationId = Guid.Empty;
        private Guid _userId { get; set; }
        private Dictionary<string, string> _mappingTacticPushData { get; set; }
        private Dictionary<string, string> _mappingCustomFieldPullData { get; set; }
        private Dictionary<string, string> _mappingTacticCombined { get; set; }
        private List<string> _customFieldIds { get; set; }
        private Guid _clientId { get; set; }
        //api URL must be prepended with the company name
        private string apiURL;
        private string _apiURL { get { return apiURL; } set { apiURL = string.Concat(string.Concat("https://", _companyName), value); } }
        private bool _isResultError { get; set; }
        private int _integrationInstanceSectionId { get; set; }
        private int _integrationInstanceId { get; set; }
        private int _integrationInstanceLogId { get; set; }
        private string _errorMessage;
        public string errorMessage
        {
            get
            {
                return _errorMessage;
            }
        }
        private MRPEntities db = new MRPEntities();
        private bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get
            {
                return _isAuthenticated;
            }
        }
        private List<string> statusList { get; set; }

        /// <summary>
        /// Empty Constructor - for further improvements
        /// Does not do anything but create the session instance
        /// </summary>
        public IntegrationWorkFrontSession()
        {

        }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        /// <param name="integrationInstanceId">Integration instance Id.</param>
        /// <param name="id">Entity Id.</param>
        /// <param name="entityType">Entity type.</param>
        /// <param name="userId">User Id.</param>
        /// <param name="integrationInstanceLogId">Integration instance log id.</param>
        public IntegrationWorkFrontSession(int integrationInstanceId, int id, EntityType entityType, Guid userId, int integrationInstanceLogId, Guid applicationId)
        {
            _integrationInstanceId = integrationInstanceId;
            _entityID = id;
            _entityType = entityType;
            _userId = userId;
            _integrationInstanceLogId = integrationInstanceLogId;
            _applicationId = applicationId;
            _isResultError = false;
            try
            {
                GetIntegrationInstanceDetails();
                doLogin_Authentication();
            }
            catch (Exception ex)
            {
                //catch and keep the error, but let Gp decide how to handle 
                this._errorMessage = ex.Message;
                _isResultError = true;
            }

        }

        /// <summary>
        /// Session constructor using passed credentials. Creates a ClientHandling instance to handle finer 
        /// client control.
        /// </summary>
        /// <param name="url">
        /// The URL to the api of the WorkFront/AtTask API.
        /// For example: "http://yourcompany.attask-ondemand.com/attask/api"
        /// </param>
        /// <param name="userID">
        /// UserID for login purposes
        /// </param>
        /// <param name="pass">
        /// User password for login purposes
        /// </param>
        public IntegrationWorkFrontSession(string companyname, string url, string userID, string pass)
        {
            try
            {
                this._username = userID;
                this._password = pass;
                this._companyName = companyname;
                this._apiURL = url;
                doLogin_Authentication();
                if (isAuthenticated())
                {
                    _isResultError = false;
                }
                else { _isResultError = true; }

            }
            catch(Exception ex)
            {
                //catch and keep the error, but let Gp decide how to handle 
                this._errorMessage = ex.Message;
                _isResultError = true;
            }
        }

        /// <summary>
        /// Sets authenticated flag to true if logged in, false if not
        /// </summary>
        private void doLogin_Authentication()
        {
            try
            {
                client = new ClientHandling(this._apiURL);
                client.Login(_username, _password);
                //Get user object of the user that is signed in
                _user = client.Get(ObjCode.USER, client.UserID, "homeGroupID");
                _userGroupID = _user["data"].Value<string>("homeGroupID");
                _isAuthenticated = true;
            }
            catch (Exception ex)
            {
                _isAuthenticated = false;
                _errorMessage = ex.Message;
                Common.SaveIntegrationInstanceLogDetails(_integrationInstanceId, null, Enums.MessageOperation.Start, "Sync Tactic", Enums.MessageLabel.Success, "Authentication Failure : " + ex.Message);
            }

        }

        /// <returns>
        /// flag to true if logged in, false if not
        /// </summary>
        public bool isAuthenticated()
        {
            return client.IsSignedIn;
        }

        /// <summary>
        /// Function to get & set integration instance details.
        /// </summary>
        private void GetIntegrationInstanceDetails()
        {
            string Companyname = "Company Name";
            IntegrationInstance integrationInstance = db.IntegrationInstances.Where(instance => instance.IntegrationInstanceId == _integrationInstanceId).FirstOrDefault();
            Dictionary<string, string> attributeKeyPair = db.IntegrationInstance_Attribute.Where(attribute => attribute.IntegrationInstanceId == _integrationInstanceId).Select(attribute => new { attribute.IntegrationTypeAttribute.Attribute, attribute.Value }).ToDictionary(attribute => attribute.Attribute, attribute => attribute.Value);
            this._companyName = attributeKeyPair[Companyname];
            this._username = integrationInstance.Username;
            this._password = Common.Decrypt(integrationInstance.Password);
            this._apiURL = integrationInstance.IntegrationType.APIURL;
        }


        /// <summary>
        /// Starting point for syncing all data
        /// Determines sync level (tactic or instance) and pulls the mapping details from Gp preferences
        /// calls saves for database changes made in lower methods.
        /// Maintains integration logs.
        /// </summary>
        /// <param name="lstSyncError">
        /// error list for tracking from Gp integration controller
        /// </param>
        /// <returns>returns flag for sync error</returns>
        public bool SyncData(out List<SyncError> lstSyncError)
        {
            List<SyncError> SyncErrors = new List<SyncError>();
            try
            {
                statusList = Common.GetStatusListAfterApproved();
                // Insert log into IntegrationInstanceSection
                _integrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), DateTime.Now, _userId);
                _isResultError = false;
                if (EntityType.Tactic.Equals(_entityType)) //_entityType is Tactic on tactic approval
                {
                    //need to sync the tactic's program first

                    Plan_Campaign_Program_Tactic planTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == _entityID && statusList.Contains(tactic.Status) && tactic.IsDeployedToIntegration && !tactic.IsDeleted).FirstOrDefault();
                    Plan_Campaign_Program planProgram = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == planTactic.PlanProgramId).FirstOrDefault();
					// Added by Arpita Soni for Ticket #2201 on 06/20/2016
                    bool isPlanToPortfolio = false;

                    IntegrationWorkFrontTacticSetting approvalSettings = db.IntegrationWorkFrontTacticSettings.SingleOrDefault(t => t.TacticId == planTactic.PlanTacticId && t.IsDeleted == false);
                    if (approvalSettings != null && approvalSettings.TacticApprovalObject == Enums.WorkFrontTacticApprovalObject.Project2.ToString())
                    {
                        isPlanToPortfolio = true;
                    }

                    if (isPlanToPortfolio)
                    {
                        bool syncError = syncPlantoPortfolio(planTactic.Plan_Campaign_Program.Plan_Campaign.Plan, ref SyncErrors);
                        if (syncError) { throw new ClientException("Cannot Sync the Plan associated with Tactic " + planTactic.Title); }
                    }
                    else
                    {
                        bool syncError = syncProgram(planProgram, ref SyncErrors);
                        if (syncError) { throw new ClientException("Cannot Sync the Program associated with Tactic " + planTactic.Title); }
                    }
                    db.SaveChanges(); //must save changes or tactic can fail
                    if (planTactic != null)
                    {
                        List<int> tacticIdList = new List<int>() { planTactic.PlanTacticId };
                        _isResultError = SetMappingDetails(ref SyncErrors);
                        _isResultError = CreateCustomFieldIdList(tacticIdList, ref SyncErrors);
                        List<int> plnaIdList = new List<int>() { planTactic.Plan_Campaign_Program.Plan_Campaign.PlanId };
                        _isResultError = SyncTactic(planTactic, ref SyncErrors);
                        db.SaveChanges();
                    }
                }
                else
                {
                    _isResultError = this.SyncInstanceData(ref SyncErrors); //this must find all tactics and sync
                    db.SaveChanges();
                }

            }
            catch(Exception ex) {
                SyncError error = new SyncError();
                error.EntityId = _integrationInstanceId;
                error.SectionName = "Sync Data";
                error.EntityType = Enums.EntityType.IntegrationInstance;
                error.Message = ex.Message;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
                _isResultError = true;
            }
            finally
            {
                lstSyncError = SyncErrors;
                if (_isResultError)
                {
                    Common.UpdateIntegrationInstanceSection(_integrationInstanceSectionId, StatusResult.Error, _errorMessage);
                }
                else
                {
                    Common.UpdateIntegrationInstanceSection(_integrationInstanceSectionId, StatusResult.Success, string.Empty);
                }
            }
            return _isResultError;
        }

        /// <summary>
        /// Instance Level sync method 
        /// Retrieves list of all tactics tied to the integrationinstance and deployed to integration, then called further methods to sync
        /// those tactics
        /// </summary>
        /// <param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool SyncInstanceData(ref List<SyncError> SyncErrors)
        {
            bool syncError = false;
            try
            {
                SyncInstanceTemplates(ref SyncErrors);
                SyncRequestQueues(ref SyncErrors);
                SyncInstanceUsers(ref SyncErrors);
                //Retrieves list of all tactics tied to the integrated programs and deployed to integrationtic
                List<Plan_Campaign_Program_Tactic> tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted == false && tactic.IsDeployedToIntegration == true &&
                    tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt == _integrationInstanceId && statusList.Contains(tactic.Status)).ToList();
                if (tacticList.Count() > 0)
                {
                    _isResultError = SetMappingDetails(ref SyncErrors);
                    if (_isResultError)
                    {
                        throw new ClientException("error in field mapping");
                    }
                    List<int> tacticIdList = tacticList.Select(c => c.PlanTacticId).ToList();
                    CreateCustomFieldIdList(tacticIdList, ref SyncErrors);
                    foreach (Plan_Campaign_Program_Tactic tactic in tacticList)
                    {
                        syncError = (syncError || SyncTactic(tactic, ref SyncErrors));
                    }
                }



            }
            catch(Exception ex)
            {
                syncError = true;
                SyncError error = new SyncError();
                error.EntityId = _integrationInstanceId;
                error.EntityType = Enums.EntityType.IntegrationInstance;
                error.SectionName = "Sync SyncInstanceData";
                error.Message = ex.Message;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
            }
            return syncError;

        }

        /// <summary>
        /// Program Level sync method 
        /// Gameplan programs are tied to WorkFront Portfolios to allow for a similar heirarchical structure
        /// sets up integration logs, create new portfolio or updates existing portfolio
        /// Both creation and update sync comments are created for display in tactic review comment log.
        /// Searches for all program tactics that are deployed to the integration and calls method to sync those tactics.
        /// </summary>
        ///  <param name="program">
        /// the program to by synced
        /// </param>
        ///<param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool syncProgram(Plan_Campaign_Program program, ref List<SyncError> SyncErrors)
        {
            bool syncError = false;
            IntegrationInstancePlanEntityLog instanceLogProgram = new IntegrationInstancePlanEntityLog();
            try
            {
                //Each program must be linked to a WorkFront portfolio
                IntegrationWorkFrontPortfolio portfolio = db.IntegrationWorkFrontPortfolios.Where(port => port.PlanProgramId == program.PlanProgramId && port.IntegrationInstanceId == program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt && port.IsDeleted == false).FirstOrDefault();
                Enums.Mode currentMode;
                if(portfolio != null)
                {
                    JToken existingInWorkFront = client.Search(ObjCode.PORTFOLIO, new { ID = portfolio.PortfolioId});
                    if (existingInWorkFront == null || !existingInWorkFront["data"].HasValues) //portfolio was deleted from WorkFront
                    {
                        portfolio.IsDeleted = true;
                        db.Entry(instanceLogProgram).State = EntityState.Modified;
                        currentMode = Enums.Mode.Create;

                    }
                    else
                    {
                        currentMode = Common.GetMode(portfolio.PortfolioId);
                    }
                }
                else { currentMode = Enums.Mode.Create; }

                //logging begin
                instanceLogProgram.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                instanceLogProgram.IntegrationInstanceId = _integrationInstanceId;
                instanceLogProgram.EntityId = program.PlanProgramId;
                instanceLogProgram.EntityType = EntityType.Tactic.ToString();
                instanceLogProgram.SyncTimeStamp = DateTime.Now;
                instanceLogProgram.Status = StatusResult.Success.ToString();
                instanceLogProgram.CreatedBy = this._userId;
                instanceLogProgram.CreatedDate = DateTime.Now;

                //logging end

                program.LastSyncDate = DateTime.Now;
                program.ModifiedDate = DateTime.Now;
                program.ModifiedBy = _userId;
                db.Entry(program).State = EntityState.Modified;
                string defaultPortfolioName = program.Plan_Campaign.Title + " : " + program.Title;

                if (currentMode.Equals(Enums.Mode.Create))
                {
                    instanceLogProgram.Operation = Operation.Create.ToString();
                    program.LastSyncDate = DateTime.Now;
                    program.ModifiedDate = DateTime.Now;
                    program.ModifiedBy = _userId;
                    JToken createdPortfolio = client.Create(ObjCode.PORTFOLIO, new { name = HttpUtility.UrlEncode(defaultPortfolioName)});
                    if (createdPortfolio == null || !createdPortfolio["data"].HasValues)
                    {
                        throw new ClientException("WorkFront Portfolio Not Created for Program " + program.Title + ".");
                    }
                    portfolio = new IntegrationWorkFrontPortfolio();
                    portfolio.IntegrationInstanceId = (int) program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt;
                    portfolio.PortfolioName = defaultPortfolioName;
                    portfolio.IsDeleted = false;
                    portfolio.PortfolioId = (string) createdPortfolio["data"]["ID"];
                    portfolio.PlanProgramId = program.PlanProgramId;
                    db.Entry(portfolio).State = EntityState.Added;
                    syncError = false;
                }
                else if (currentMode.Equals(Enums.Mode.Update))
                {
                    instanceLogProgram.Operation = Operation.Update.ToString();
                    JToken updatePortfolio = client.Update(ObjCode.PORTFOLIO, new { id = portfolio.PortfolioId, name = HttpUtility.UrlEncode(defaultPortfolioName) });
                    if (updatePortfolio == null || !updatePortfolio["data"].HasValues)
                    {
                        throw new ClientException("Portfolio name is not updated for program " + program.Title + ".");
                    }
                    JToken existingPortfolio = client.Search(ObjCode.PORTFOLIO, new { ID = portfolio.PortfolioId });
                    if (existingPortfolio == null) { throw new ClientException("Portfolio Not found in WorkFront but exists in Database."); }
                    portfolio.PortfolioName = (string)existingPortfolio["data"][0]["name"];
                    portfolio.IsDeleted = false;
                    db.Entry(portfolio).State = EntityState.Modified;
                    syncError = false;
                }

                if (!syncError) //don't leave a sync comment if it didn't sync
                {
                    //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                    if (!Common.IsAutoSync)
                    {
                        //Add program review comment when synced
                        Plan_Campaign_Program_Tactic_Comment objProgramComment = new Plan_Campaign_Program_Tactic_Comment();
                        objProgramComment.PlanProgramId = program.PlanProgramId;
                        objProgramComment.Comment = Common.ProgramSyncedComment + Integration.Helper.Enums.IntegrationType.WorkFront.ToString();
                        objProgramComment.CreatedDate = DateTime.Now;
                        //if (Common.IsAutoSync)
                        //{
                        //    objProgramComment.CreatedBy = new Guid();
                        //}
                        //else { 
                        objProgramComment.CreatedBy = this._userId;
                        //}

                        db.Entry(objProgramComment).State = EntityState.Added;
                        db.Plan_Campaign_Program_Tactic_Comment.Add(objProgramComment);
                        //add success message to IntegrationInstanceLogDetails
                    }

                }

                if (syncError){
                    throw new ClientException("Sycn Error for Program " + program.Title);
                }
                else { Common.SaveIntegrationInstanceLogDetails(_integrationInstanceId, null, Enums.MessageOperation.Start, "Sync Program", Enums.MessageLabel.Success, "Sync success - Program " + program.Title); }
            }
            catch (Exception ex)
            {
                syncError = true;
                SyncError error = new SyncError();
                error.EntityId = _integrationInstanceId;
                error.EntityType = Enums.EntityType.IntegrationInstance;
                error.SectionName = "Sync Program";
                error.Message = ex.Message;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
            }
            finally
            {
                db.Entry(instanceLogProgram).State = EntityState.Added;
                db.SaveChanges();
            }
            return syncError;
        }

        /// <summary>
        /// Plan Level sync method  - Use Case 3
        /// Gameplan plans are tied to WorkFront Portfolios to allow for a similar heirarchical structure
        /// sets up integration logs, create new portfolio or updates existing portfolio
        /// Both creation and update sync comments are created for display in tactic review comment log.
        /// Searches for all plan tactics that are deployed to the integration and calls method to sync those tactics.
        /// </summary>
        ///  <param name="plan">
        /// the plan to by synced
        /// </param>
        ///<param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool syncPlantoPortfolio(Plan plan, ref List<SyncError> SyncErrors)
        {
            bool syncError = false;
            IntegrationInstancePlanEntityLog instanceLogPlan = new IntegrationInstancePlanEntityLog();
            try
            {
                //Each program must be linked to a WorkFront portfolio
                IntegrationWorkFrontPortfolio portfolio = db.IntegrationWorkFrontPortfolios.Where(port => port.PlanId == plan.PlanId && port.IntegrationInstanceId == plan.Model.IntegrationInstanceIdProjMgmt && port.IsDeleted == false).FirstOrDefault();
                Enums.Mode currentMode;
                if (portfolio != null)
                {
                    JToken existingInWorkFront = client.Search(ObjCode.PORTFOLIO, new { ID = portfolio.PortfolioId });
                    if (existingInWorkFront == null || !existingInWorkFront["data"].HasValues) //portfolio was deleted from WorkFront
                    {
                        portfolio.IsDeleted = true;
                        db.Entry(instanceLogPlan).State = EntityState.Modified;
                        currentMode = Enums.Mode.Create;
                    }
                    else
                    {
                        currentMode = Common.GetMode(portfolio.PortfolioId);
                    }
                }
                else { currentMode = Enums.Mode.Create; }

                //logging begin
                instanceLogPlan.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                instanceLogPlan.IntegrationInstanceId = _integrationInstanceId;
                instanceLogPlan.EntityId = plan.PlanId;
                instanceLogPlan.EntityType = EntityType.Tactic.ToString();
                instanceLogPlan.SyncTimeStamp = DateTime.Now;
                instanceLogPlan.Status = StatusResult.Success.ToString();
                instanceLogPlan.CreatedBy = this._userId;
                instanceLogPlan.CreatedDate = DateTime.Now;

                //logging end

                //plan.LastSyncDate = DateTime.Now;
                plan.ModifiedDate = DateTime.Now;
                plan.ModifiedBy = _userId;
                db.Entry(plan).State = EntityState.Modified;
                string defaultPortfolioName = plan.Title;

                if (currentMode.Equals(Enums.Mode.Create))
                {
                    instanceLogPlan.Operation = Operation.Create.ToString();
                    //plan.LastSyncDate = DateTime.Now;
                    plan.ModifiedDate = DateTime.Now;
                    plan.ModifiedBy = _userId;
                    JToken createdPortfolio = client.Create(ObjCode.PORTFOLIO, new { name = HttpUtility.UrlEncode(defaultPortfolioName)});
                    if (createdPortfolio == null || !createdPortfolio["data"].HasValues)
                    {
                        throw new ClientException("WorkFront Portfolio Not Created for Plan " + plan.Title + ".");
                    }
                    portfolio = new IntegrationWorkFrontPortfolio();
                    portfolio.IntegrationInstanceId = (int)plan.Model.IntegrationInstanceIdProjMgmt;
                    portfolio.PortfolioName = defaultPortfolioName;
                    portfolio.IsDeleted = false;
                    portfolio.PortfolioId = (string)createdPortfolio["data"]["ID"];
                    portfolio.PlanId = plan.PlanId;
                    db.Entry(portfolio).State = EntityState.Added;
                    syncError = false;
                }
                else if (currentMode.Equals(Enums.Mode.Update))
                {
                    instanceLogPlan.Operation = Operation.Update.ToString();
                    JToken updatePortfolio = client.Update(ObjCode.PORTFOLIO, new { id = portfolio.PortfolioId, name = HttpUtility.UrlEncode(defaultPortfolioName) });
                    if (updatePortfolio == null || !updatePortfolio["data"].HasValues)
                    {
                        throw new ClientException("Portfolio name is not updated for Plan " + plan.Title + ".");
                    }
                    JToken existingPortfolio = client.Search(ObjCode.PORTFOLIO, new { ID = portfolio.PortfolioId });
                    if (existingPortfolio == null) { throw new ClientException("Portfolio Not found in WorkFront but exists in Database."); }
                    portfolio.PortfolioName = (string)existingPortfolio["data"][0]["name"];
                    portfolio.IsDeleted = false;
                    db.Entry(portfolio).State = EntityState.Modified;
                    syncError = false;
                }

                if (syncError)
                {
                    throw new ClientException("Sycn Error for Plan " + plan.Title);
                }
                else { Common.SaveIntegrationInstanceLogDetails(_integrationInstanceId, null, Enums.MessageOperation.Start, "Sync Plan", Enums.MessageLabel.Success, "Sync success - Plan " + plan.Title); }
            }
            catch (Exception ex)
            {
                syncError = true;
                SyncError error = new SyncError();
                error.EntityId = _integrationInstanceId;
                error.EntityType = Enums.EntityType.IntegrationInstance;
                error.SectionName = "Sync Plan";
                error.Message = ex.Message;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
            }
            finally
            {
                db.Entry(instanceLogPlan).State = EntityState.Added;
                db.SaveChanges();
            }
            return syncError;
        }

        /// <summary>
        /// Plan Level sync method  - Use Case 3
        /// Gameplan plans are tied to WorkFront Portfolios to allow for a similar heirarchical structure
        /// sets up integration logs, create new portfolio or updates existing portfolio
        /// Both creation and update sync comments are created for display in tactic review comment log.
        /// Searches for all plan tactics that are deployed to the integration and calls method to sync those tactics.
        /// </summary>
        ///  <param name="plan">
        /// the plan to by synced
        /// </param>
        ///<param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool syncPlanCampaignToWFProgram(Plan_Campaign_Program_Tactic tactic, ref List<SyncError> SyncErrors, ref IntegrationInstancePlanEntityLog instanceLogTactic)
        {
            bool tacticError = false;

            Enums.Mode currentMode = Common.GetMode(tactic.Plan_Campaign_Program.Plan_Campaign.IntegrationWorkFrontProgramID);

            //program portfolio information -- all programs should be linked to a portfolio in WorkFront - regardless of sync type
            IntegrationWorkFrontPortfolio portfolioInfo = CreatePortfolio(tactic, ref SyncErrors);

            //if IntegrationWorkFrontProjectID doesn't exist in WorkFront, create a new one
            if (currentMode == Enums.Mode.Update)
            {
                JToken checkExists = client.Search(ObjCode.PROGRAM, new { ID = tactic.Plan_Campaign_Program.Plan_Campaign.IntegrationWorkFrontProgramID, map = true });
                if (checkExists == null || checkExists["data"].HasValues == false)
                { currentMode = Enums.Mode.Create; }
            }

            if (currentMode.Equals(Enums.Mode.Create))
            {
                instanceLogTactic.Operation = Operation.Create.ToString();

                //consolidated creation : create project with appropriate template, timeline & place in correct portfolio
                JToken program = client.Create(ObjCode.PROGRAM, new
                {
                    name = HttpUtility.UrlEncode(tactic.Plan_Campaign_Program.Plan_Campaign.Title),
                    portfolioID = portfolioInfo.PortfolioId
                });
                if (program == null || !program["data"].HasValues)
                {
                    throw new ClientException("Program Not Created for Tactic " + tactic.Title + ".");
                }

                //Enter information into WorkFront portfolio to project mapping table in database
                IntegrationWorkFrontProgram_Mapping createdPortfolioProject = new IntegrationWorkFrontProgram_Mapping();
                createdPortfolioProject.PortfolioTableId = portfolioInfo.Id;
                createdPortfolioProject.ProgramId = (string)program["data"]["ID"];
                db.Entry(createdPortfolioProject).State = EntityState.Added;
                tactic.Plan_Campaign_Program.Plan_Campaign.IntegrationWorkFrontProgramID = (string)program["data"]["ID"];
                
                //moved comment section to only leave a comment on successful project creation, PL#1871 : Brad Gray 01/07/2016
                if (!tacticError) //don't leave a sync comment if it didn't sync
                {
                    //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                    if (!Common.IsAutoSync)
                    {
                        //Add tactic review comment when sync tactic
                        Plan_Campaign_Program_Tactic_Comment objTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                        objTacticComment.PlanCampaignId = tactic.Plan_Campaign_Program.PlanCampaignId;
                        objTacticComment.Comment = Common.CampaignSyncedComment + Integration.Helper.Enums.IntegrationType.WorkFront.ToString();
                        objTacticComment.CreatedDate = DateTime.Now;
                        objTacticComment.CreatedBy = this._userId;

                        db.Entry(objTacticComment).State = EntityState.Added;
                        db.Plan_Campaign_Program_Tactic_Comment.Add(objTacticComment);
                    }
                }
            }
            else if (currentMode.Equals(Enums.Mode.Update))
            {
                instanceLogTactic.Operation = Operation.Update.ToString();
                JToken updateProgram = client.Update(ObjCode.PROGRAM, new { id = tactic.Plan_Campaign_Program.Plan_Campaign.IntegrationWorkFrontProgramID, name = HttpUtility.UrlEncode(tactic.Plan_Campaign_Program.Plan_Campaign.Title) });
                if (updateProgram == null || !updateProgram["data"].HasValues)
                {
                    throw new ClientException("Program name is not updated for tactic " + tactic.Title + ".");
                }
            }
            return tacticError;
        }


        ///<param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool SyncInstanceTemplates(ref List<SyncError> SyncErrors)
        {
            bool syncError = false;
            //update WorkFront templates for the instance
            try
            {
                JToken templateInfoFromWorkFront = client.Search(ObjCode.TEMPLATE, new { fields = "ID" });
                //Get the list of templates in the database
                List<IntegrationWorkFrontTemplate> templatesFromDB = db.IntegrationWorkFrontTemplates.Where(template => template.IntegrationInstanceId  == _integrationInstanceId && template.IsDeleted==0).ToList();
                List<string> templateIdsFromDB = templatesFromDB.Select(tmpl => tmpl.TemplateId).ToList();
                //For comparison purposes, create a list of templates from WorkFront
                List<string> templateIdsFromWorkFront = new List<string>();

                foreach (var template in templateInfoFromWorkFront["data"])
                {
                    string templID = template["ID"].ToString().Trim(); //WorkFront template IDs come with excess whitespace
                    templateIdsFromWorkFront.Add(templID);
                    if ( !templateIdsFromDB.Contains(templID))
                    {
                        IntegrationWorkFrontTemplate newTemplate = new IntegrationWorkFrontTemplate();
                        newTemplate.IntegrationInstanceId = _integrationInstanceId;
                        newTemplate.TemplateId = templID;
                        newTemplate.Template_Name = template["name"].ToString();
                        newTemplate.IsDeleted = 0;
                        db.Entry(newTemplate).State = EntityState.Added;
                    }
                    else {
                        IntegrationWorkFrontTemplate templateToEdit = db.IntegrationWorkFrontTemplates.Where(t => t.TemplateId == templID).FirstOrDefault();
                        templateToEdit.Template_Name = template["name"].ToString();
                        db.Entry(templateToEdit).State = EntityState.Modified;
                    }
                }

                //templates in the database that are not in WorkFront need to be set to deleted in database 
                List<string> inDatabaseButNotInWorkFront = templateIdsFromDB.Except(templateIdsFromWorkFront).ToList();
                List<IntegrationWorkFrontTemplate> templatesToDelete = db.IntegrationWorkFrontTemplates.Where(t => inDatabaseButNotInWorkFront.Contains(t.TemplateId)).ToList();
                if(templatesToDelete.Count>0)
                {
                    MRPEntities mp = new MRPEntities();
                    DbConnection conn = mp.Database.Connection;
                    conn.Open();
                    DbCommand cmd = conn.CreateCommand();
                    StringBuilder query = new StringBuilder();
                    foreach (IntegrationWorkFrontTemplate template in templatesToDelete)
                    {
                        template.IsDeleted = 1;
                        db.Entry(template).State = EntityState.Modified;
                        //Added 11 Sept 2015, Brad Gray, PL#1374 - remove template ID from tactic type when template is set to deleted
                        query.Append("update [dbo].[TacticType] set [WorkFront Template] = null where [WorkFront Template] = '" + template.TemplateId + "';");
                    }
                    cmd.CommandText = query.ToString();
                    cmd.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                syncError = true;
                SyncError error = new SyncError();
                error.EntityId = _integrationInstanceId;
                error.EntityType = Enums.EntityType.IntegrationInstance;
                error.SectionName = "Sync Integration Templates";
                error.Message = ex.Message;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
            }
            return syncError;
        }

        ///Retrieve Requests from WorkFront and store in Plan. Any requests deleted in WorkFront are marked as deleted in Plan db
        ///Plan creates and tracks requests. Do not need to pull in other requests from WorkFront as they aren't tied to Plan projects
        ///If request is in database but not in WorkFront, create the request. This behavior is different than templates, where we set the db entry to IsDeleted.
        ///<param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool SyncRequestQueues(ref List<SyncError> SyncErrors)
        {
            bool syncError = false;
            //update WorkFront request queues for the instance
            try
            {
                string searchFields = "allowedOpTaskTypes,projectID,project,isPublic";

                //request queues can be created against templates. search only for request queues created against a project
                JToken qFromWorkFront = client.Search(ObjCode.QUEUE, new { fields = searchFields, isPublic= "0", isPublic_Mod="ne", projectID_Mod = "notnull" });
                //Get the list of request queus in the database
                List<IntegrationWorkFrontRequestQueue> queuesFromDB = db.IntegrationWorkFrontRequestQueues.Where(q => q.IntegrationInstanceId  == _integrationInstanceId && q.IsDeleted==false).ToList();
                List<string> qIdsFromDB = queuesFromDB.Select(q => q.RequestQueueId).ToList();
                //For comparison purposes, create a list of templates from WorkFront
                List<string> queueIdsFromWorkFront = new List<string>();

                foreach (var queue in qFromWorkFront["data"])
                {
                    string qID = queue["ID"].ToString().Trim();
                    queueIdsFromWorkFront.Add(qID);

                    if ( !qIdsFromDB.Contains(qID) ) //found a queue in WorkFront that isn't in the database. add it to database
                    {
                        IntegrationWorkFrontRequestQueue newQueue = new IntegrationWorkFrontRequestQueue();
                        newQueue.IntegrationInstanceId = _integrationInstanceId;
                        newQueue.RequestQueueId = qID;
                        newQueue.RequestQueueName = queue["project"]["name"].ToString();
                        newQueue.IsDeleted = false;
                        db.Entry(newQueue).State = EntityState.Added;
                    }
                    else {
                        IntegrationWorkFrontRequestQueue queueToEdit = db.IntegrationWorkFrontRequestQueues.Where(q => q.RequestQueueId == qID && q.IntegrationInstanceId == _integrationInstanceId).FirstOrDefault();
                        queueToEdit.RequestQueueName = queue["project"]["name"].ToString(); //update the name in the database
                        db.Entry(queueToEdit).State = EntityState.Modified;
                    }
                }

                //queues in the database that are not in WorkFront need to be set to deleted in database 
                List<string> inDatabaseButNotInWorkFront = qIdsFromDB.Except(queueIdsFromWorkFront).ToList();
                List<IntegrationWorkFrontRequestQueue> queuesToDelete = db.IntegrationWorkFrontRequestQueues.Where(t => inDatabaseButNotInWorkFront.Contains(t.RequestQueueId)).ToList();
                if (queuesToDelete.Count > 0)
                {
                    queuesToDelete.Select(c => { c.IsDeleted = true; return c; }).ToList();
                    db.Entry(queuesToDelete).State = EntityState.Modified;
                }

                db.SaveChanges();

            }
            catch (Exception ex)
            {
                syncError = true;
                SyncError error = new SyncError();
                error.EntityId = _integrationInstanceId;
                error.EntityType = Enums.EntityType.IntegrationInstance;
                error.SectionName = "Sync Integration Requests";
                error.Message = ex.Message;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
            }
            return syncError;
        }

        ///Retrieve Users from WorkFront and store in Plan. Any users deactivated in WorkFront are marked as deleted in Plan db
        ///Currently retrieves only user name and WorkFront ID.
        ///Users from WorkFront are tied to the integration instance. Users do not have to exist in Plan, nor is a user currently tied to any Plan user
        ///Added 1/13/2016 by Brad Gray for PL#1895
        ///<param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool SyncInstanceUsers(ref List<SyncError> SyncErrors)
        {
            bool syncError = false;
            //update WorkFront users for the instance
            try
            {
                JToken usersFromWorkFront = client.Search(ObjCode.USER, new { fields = "name,ID,isActive", isActive="true" });
                //Get the list of WorkFront users in the database
                List<IntegrationWorkFrontUser> usersFromDB = db.IntegrationWorkFrontUsers.Where(user => user.IntegrationInstanceId == _integrationInstanceId && user.IsDeleted == false).ToList();
                List<string> userIdsFromDB = usersFromDB.Select(q => q.WorkFrontUserId).ToList();
                //For comparison purposes, create a list of users from WorkFront
                List<string> usersInWorkFront = new List<string>();

                foreach (var user in usersFromWorkFront["data"])
                {
                    string userID = user["ID"].ToString().Trim();
                    usersInWorkFront.Add(userID);

                    if (!userIdsFromDB.Contains(userID)) //found a user in WorkFront that isn't in the database. add it to database
                    {
                        IntegrationWorkFrontUser newUser = new IntegrationWorkFrontUser();
                        newUser.WorkFrontUserId = userID;
                        newUser.WorkFrontUserName = user["name"].ToString();
                        newUser.IsDeleted = false;
                        newUser.IntegrationInstanceId = _integrationInstanceId;
                        db.Entry(newUser).State = EntityState.Added;
                    }
                    else
                    {
                        IntegrationWorkFrontUser userToEdit = db.IntegrationWorkFrontUsers.Where(u => u.WorkFrontUserId  == userID && u.IntegrationInstanceId == _integrationInstanceId).FirstOrDefault();
                        userToEdit.WorkFrontUserName = user["name"].ToString(); //update the name in the database
                        db.Entry(userToEdit).State = EntityState.Modified;
                    }
                }

                //users in the database that are not in WorkFront or are set to deactivated in Workfront need to be set to deleted in database 
                List<string> inDatabaseButNotInWorkFront = userIdsFromDB.Except(usersInWorkFront).ToList();
                List<IntegrationWorkFrontUser> userToDelete = db.IntegrationWorkFrontUsers.Where(user => inDatabaseButNotInWorkFront.Contains(user.WorkFrontUserId)).ToList(); //userID could be duplicated in Plan - set all of them to deleted
                if (userToDelete.Count > 0)
                {
                    userToDelete.Select(c => { c.IsDeleted = true; return c; }).ToList();
                    db.Entry(userToDelete).State = EntityState.Modified;
                }

            }
            catch (Exception ex)
            {
                syncError = true;
                SyncError error = new SyncError();
                error.EntityId = _integrationInstanceId;
                error.EntityType = Enums.EntityType.IntegrationInstance;
                error.SectionName = "Sync WorkFront Users";
                error.Message = ex.Message;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
            }
            return syncError;
        }

        ///Retrieve Users from WorkFront and store in Plan. Any users deactivated in WorkFront are marked as deleted in Plan db
        ///Currently retrieves only user name and WorkFront ID.
        ///Users from WorkFront are tied to the integration instance. Users do not have to exist in Plan, nor is a user currently tied to any Plan user
        ///Added 1/13/2016 by Brad Gray for PL#1895
        ///<param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool SyncInstanceCustomFields(ref List<SyncError> SyncErrors)
        {
            bool syncError = false;
            //update WorkFront users for the instance
            try
            {
                JToken usersFromWorkFront = client.Search(ObjCode.USER, new { fields = "name,ID,isActive", isActive = "true" });
                //Get the list of WorkFront users in the database
                List<IntegrationWorkFrontUser> usersFromDB = db.IntegrationWorkFrontUsers.Where(user => user.IntegrationInstanceId == _integrationInstanceId && user.IsDeleted == false).ToList();
                List<string> userIdsFromDB = usersFromDB.Select(q => q.WorkFrontUserId).ToList();
                //For comparison purposes, create a list of users from WorkFront
                List<string> usersInWorkFront = new List<string>();

                foreach (var user in usersFromWorkFront["data"])
                {
                    string userID = user["ID"].ToString().Trim();
                    usersInWorkFront.Add(userID);

                    if (!userIdsFromDB.Contains(userID)) //found a user in WorkFront that isn't in the database. add it to database
                    {
                        IntegrationWorkFrontUser newUser = new IntegrationWorkFrontUser();
                        newUser.WorkFrontUserId = userID;
                        newUser.WorkFrontUserName = user["name"].ToString();
                        newUser.IsDeleted = false;
                        newUser.IntegrationInstanceId = _integrationInstanceId;
                        db.Entry(newUser).State = EntityState.Added;
                    }
                    else
                    {
                        IntegrationWorkFrontUser userToEdit = db.IntegrationWorkFrontUsers.Where(u => u.WorkFrontUserId == userID && u.IntegrationInstanceId == _integrationInstanceId).FirstOrDefault();
                        userToEdit.WorkFrontUserName = user["name"].ToString(); //update the name in the database
                        db.Entry(userToEdit).State = EntityState.Modified;
                    }
                }

                //users in the database that are not in WorkFront or are set to deactivated in Workfront need to be set to deleted in database 
                List<string> inDatabaseButNotInWorkFront = userIdsFromDB.Except(usersInWorkFront).ToList();
                List<IntegrationWorkFrontUser> userToDelete = db.IntegrationWorkFrontUsers.Where(user => inDatabaseButNotInWorkFront.Contains(user.WorkFrontUserId)).ToList(); //userID could be duplicated in Plan - set all of them to deleted
                if (userToDelete.Count > 0)
                {
                    userToDelete.Select(c => { c.IsDeleted = true; return c; }).ToList();
                    db.Entry(userToDelete).State = EntityState.Modified;
                }

            }
            catch (Exception ex)
            {
                syncError = true;
                SyncError error = new SyncError();
                error.EntityId = _integrationInstanceId;
                error.EntityType = Enums.EntityType.IntegrationInstance;
                error.SectionName = "Sync WorkFront Users";
                error.Message = ex.Message;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
            }
            return syncError;
        }

        /// <summary>
        /// Tactic Level sync method 
        /// sets up tactic integration logs
        /// Syncing a tactic will require either syncing with a WorkFront project or syncing with a WorkFront Request
        /// Calls the appropriate method depending on the sync type 
        /// Create / Sync the project or request, then sync the portfolio to organize the project or request 
        /// Commenting should be delegated to the sync type methods as they should only be created on object creation
        /// catches the exceptions thrown by both sync type methods
        /// </summary>
        ///  <param name="tactic">
        /// the tactic to by synced
        /// </param>
        ///<param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool SyncTactic(Plan_Campaign_Program_Tactic tactic, ref List<SyncError> SyncErrors)
        {
            bool tacticError = false;
			// Added by Arpita Soni for Ticket #2201 on 06/20/2016
            bool isPlanToPortfolio = false;
            if (tactic.IsSyncWorkFront == false) { return tacticError; }  //setting at tactic level determining if the tactic should be synced - Added by Brad Gray 24 Jan 2016 PL#1921
            IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
            //moved syncerror out of catch block & renamed; 14 March 2016 by Brad Gray - PL#1941
            SyncError syncResult = new SyncError();
            syncResult.EntityId = tactic.PlanTacticId;
            syncResult.EntityType = Enums.EntityType.Tactic;
            syncResult.SectionName = "Sync Tactic Data";

            try
            {
                //logging begin
                instanceLogTactic.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                instanceLogTactic.IntegrationInstanceId = _integrationInstanceId;
                instanceLogTactic.EntityId = tactic.PlanTacticId;
                instanceLogTactic.EntityType = EntityType.Tactic.ToString();
                instanceLogTactic.SyncTimeStamp = DateTime.Now;
                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
				// Added by Arpita Soni for Ticket #2201 on 06/20/2016
                IntegrationWorkFrontTacticSetting approvalSettings = db.IntegrationWorkFrontTacticSettings.SingleOrDefault(t => t.TacticId == tactic.PlanTacticId && t.IsDeleted == false);
                if (approvalSettings != null && approvalSettings.TacticApprovalObject == Enums.WorkFrontTacticApprovalObject.Project2.ToString())
                {
                    isPlanToPortfolio = true;
                }
                //Updates made Jan 2016 by Brad Gray PL#1851 & PL#1897 - separate tactic sync types to acommodate different use cases
                if (tactic.IntegrationWorkFrontProjectID != null) // if there's a project ID, we need to sync the tactic
                {
                    if (isPlanToPortfolio)
                    {
                        tacticError = syncPlanCampaignToWFProgram(tactic, ref SyncErrors, ref instanceLogTactic);
                        tacticError = SyncTacticProjectForPlanToPortfolio(tactic, ref SyncErrors, ref instanceLogTactic);
                    }
                    else{
                        tacticError = SyncTacticProject(tactic, ref SyncErrors, ref instanceLogTactic);
                    }
                }
                else//no project ID, so check the tactic approval behavior options
                {
                    if (approvalSettings == null) { throw new ClientException("Could not find Tactic WorkFront Approval Settings"); }
                    if (approvalSettings.TacticApprovalObject == Enums.WorkFrontTacticApprovalObject.Project.ToString()
                        || approvalSettings.TacticApprovalObject == Enums.WorkFrontTacticApprovalObject.Project2.ToString())
                    {
                        //we need to sync the project, so send to SyncTacticProject
                        if (isPlanToPortfolio)
                        {
                            tacticError = syncPlanCampaignToWFProgram(tactic, ref SyncErrors, ref instanceLogTactic);
                            tacticError = SyncTacticProjectForPlanToPortfolio(tactic, ref SyncErrors, ref instanceLogTactic);
                        }
                        else
                        {
                            tacticError = SyncTacticProject(tactic, ref SyncErrors, ref instanceLogTactic);
                        }
                    }
                    else
                    {
                        tacticError = SyncTacticRequest(tactic, ref SyncErrors, ref instanceLogTactic);
                    }
                }

                tactic.LastSyncDate = DateTime.Now;
                tactic.ModifiedDate = DateTime.Now;
                tactic.ModifiedBy = _userId;
                db.Entry(tactic).State = EntityState.Modified;
                // Modified by Arpita Soni for Ticket #2304 on 06/24/2016
                if (!tacticError)
                {
                    instanceLogTactic.Status = StatusResult.Success.ToString();
                    //add success message to IntegrationInstanceLogDetails
                    Common.SaveIntegrationInstanceLogDetails(_integrationInstanceId, null, Enums.MessageOperation.Start, "Sync Tactic", Enums.MessageLabel.Success, "Sync success on Tactic " + tactic.Title);
                    //update errorlist with success 
                    //added 14 March 2016 by Brad Gray - PL#1941
                    syncResult.Message = "In tactic " + tactic.Title + " : Success";
                    syncResult.SyncStatus = Enums.SyncStatus.Success;
                }
                else
                {
                    instanceLogTactic.Status = StatusResult.Error.ToString();
                    //update errorlist with error
                    syncResult.Message = "In tactic " + tactic.Title + " : Error";
                    syncResult.SyncStatus = Enums.SyncStatus.Error;
                }
            }
            catch(Exception ex)
            {
                //updated 14 March 2016 by Brad Gray - PL#1941
                tacticError = true;
                instanceLogTactic.Status = StatusResult.Error.ToString();
                instanceLogTactic.ErrorDescription = ex.Message;
                syncResult.Message = "In tactic " + tactic.Title + " : " + ex.Message;
                syncResult.SyncStatus = Enums.SyncStatus.Error;
                syncResult.TimeStamp = DateTime.Now;
                
            }
            finally
            {
                db.Entry(instanceLogTactic).State = EntityState.Added;
                //moved from catch block 14 March 2016 by Brad Gray - PL#1941 
                if (!tacticError)
                {
                    SyncErrors.Add(syncResult);
                }
            }
            return tacticError;
        }

        /// <summary>
        /// Tactic Level sync method when syncing a tactic with a WorkFront project
        /// tactic integration logs should already be set up by the calling method
        /// Create new project or update tactic with existing project
        /// Creates project with attached template.
        /// All in one api command not yet found, so method is completed with a few api calls.
        /// First: create the project with projectName. Second: attach the template (currently hard coded).
        /// Third: Edit the scheduling mode to schedule from the completion date, and edit the completion date itself.
        /// Workfront scheduling is based off the completion date by starting the project earlier based off the template
        /// timeline.
        /// Both creation and update sync comments are created for display in tactic review comment log.
        /// Does not catch exceptions - let main SyncTactic method handle these
        /// </summary>
        /// <param name="tactic">
        /// the tactic to by synced
        /// </param>
        /// <param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool SyncTacticProject(Plan_Campaign_Program_Tactic tactic, ref List<SyncError> SyncErrors, ref IntegrationInstancePlanEntityLog instanceLogTactic)
        {
            bool tacticError = false;

            TacticType tacticType = db.TacticTypes.Where(type => type.TacticTypeId == tactic.TacticTypeId).FirstOrDefault();
            Enums.Mode currentMode = Common.GetMode(tactic.IntegrationWorkFrontProjectID);

            //program portfolio information -- all programs should be linked to a portfolio in WorkFront - regardless of sync type
            IntegrationWorkFrontPortfolio portfolioInfo = db.IntegrationWorkFrontPortfolios.Where(port => port.PlanProgramId == tactic.PlanProgramId &&
                port.IntegrationInstanceId == tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt && port.IsDeleted == false).FirstOrDefault();
            JToken existingInWorkFront = null;
            if (portfolioInfo != null) // If Gameplan doesn't have an ID - can't search for a portfolio.
            {
                existingInWorkFront = client.Search(ObjCode.PORTFOLIO, new { ID = portfolioInfo.PortfolioId });
            }

            if (portfolioInfo == null || existingInWorkFront == null || !existingInWorkFront["data"].HasValues || portfolioInfo.PortfolioId != existingInWorkFront["data"][0]["ID"].ToString())
            {
                bool programError = syncProgram(tactic.Plan_Campaign_Program, ref SyncErrors); //force program sync to create a portfolio if none found
                if (programError) { throw new ClientException("Cannot Sync the Program associated with Tactic. Tactic: " + tactic.Title + "; Program: " + tactic.Plan_Campaign_Program.Title); }
                portfolioInfo = db.IntegrationWorkFrontPortfolios.Where(port => port.PlanProgramId == tactic.PlanProgramId &&
                port.IntegrationInstanceId == tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt && port.IsDeleted == false).FirstOrDefault(); //try again to get info

                if (portfolioInfo == null) //check again to ensure we got information
                {
                    throw new ClientException("Cannot determine portfolio for tactic " + tactic.Title); //it didn't work - throw excecption
                }
            }

            // if IntegrationWorkFrontProjectID doesn't exist in WorkFront, create a new one
            if (currentMode == Enums.Mode.Update)
            {
                JToken checkExists = client.Search(ObjCode.PROJECT, new { ID = tactic.IntegrationWorkFrontProjectID, map = true });
                if (checkExists == null || checkExists["data"].HasValues == false)
                { currentMode = Enums.Mode.Create; }
            }

            if (currentMode.Equals(Enums.Mode.Create))
            {
                instanceLogTactic.Operation = Operation.Create.ToString();
                if (tacticType.IntegrationWorkFrontTemplate == null || tacticType.IntegrationWorkFrontTemplate.TemplateId == null)
                {
                    throw new ClientException("Tactic Type to Template Mapping Not Found for tactic " + tactic.Title + ".");
                }

                string templateToUse = tacticType.IntegrationWorkFrontTemplate.TemplateId;
                JToken templateInfo = client.Search(ObjCode.TEMPLATE, new { ID = templateToUse });
                if (templateInfo == null || !templateInfo["data"].HasValues)
                {
                    throw new ClientException("Template " + templateToUse + " not Found in WorkFront");
                }
                string templateID = (string)templateInfo["data"][0]["ID"];

                //consolidated creation : create project with appropriate template, timeline & place in correct portfolio
                JToken project = client.Create(ObjCode.PROJECT, new
                {
                    name = HttpUtility.UrlEncode(tactic.Title),
                    templateID = templateID,
                    scheduleMode = 'C',
                    plannedCompletionDate = tactic.StartDate,
                    portfolioID = portfolioInfo.PortfolioId,
                    groupID = _userGroupID
                });
                if (project == null || !project["data"].HasValues)
                {
                    throw new ClientException("Project Not Created for Tactic " + tactic.Title + ".");
                }

                //Enter information into WorkFront portfolio to project mapping table in database
                IntegrationWorkFrontPortfolio_Mapping createdPortfolioProject = new IntegrationWorkFrontPortfolio_Mapping();
                createdPortfolioProject.PortfolioTableId = portfolioInfo.Id;
                createdPortfolioProject.ProjectId = (string)project["data"]["ID"];
                db.Entry(createdPortfolioProject).State = EntityState.Added;
                tactic.IntegrationWorkFrontProjectID = (string)project["data"]["ID"];
                tacticError = UpdateTacticInfo(tactic, portfolioInfo, ref SyncErrors);

                //moved comment section to only leave a comment on successful project creation, PL#1871 : Brad Gray 01/07/2016
                if (!tacticError) //don't leave a sync comment if it didn't sync
                {
                    //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                    if (!Common.IsAutoSync)
                    {
                        //Add tactic review comment when sync tactic
                        Plan_Campaign_Program_Tactic_Comment objTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                        objTacticComment.PlanTacticId = tactic.PlanTacticId;
                        objTacticComment.Comment = Common.TacticSyncedComment + Integration.Helper.Enums.IntegrationType.WorkFront.ToString();
                        objTacticComment.CreatedDate = DateTime.Now;
                        //if (Common.IsAutoSync)
                        //{
                        //   objTacticComment.CreatedBy = new Guid();
                        //}
                        //else
                        //{
                        objTacticComment.CreatedBy = this._userId;
                        //}
                        db.Entry(objTacticComment).State = EntityState.Added;
                        db.Plan_Campaign_Program_Tactic_Comment.Add(objTacticComment);
                    }
                }
            }
            else if (currentMode.Equals(Enums.Mode.Update))
            {
                instanceLogTactic.Operation = Operation.Update.ToString();
                JToken updateProject = client.Update(ObjCode.PROJECT, new { id = tactic.IntegrationWorkFrontProjectID, name = HttpUtility.UrlEncode(tactic.Title) });
                if (updateProject == null || !updateProject["data"].HasValues)
                {
                    throw new ClientException("Project name is not updated for tactic " + tactic.Title + ".");
                }
                tacticError = UpdateTacticInfo(tactic, portfolioInfo, ref SyncErrors);
            }

            return tacticError;
        }

        /// <summary>
        /// Tactic Level sync method when syncing a tactic with a WorkFront project
        /// tactic integration logs should already be set up by the calling method
        /// Create new project or update tactic with existing project
        /// Creates project with attached template.
        /// All in one api command not yet found, so method is completed with a few api calls.
        /// First: create the project with projectName. Second: attach the template (currently hard coded).
        /// Third: Edit the scheduling mode to schedule from the completion date, and edit the completion date itself.
        /// Workfront scheduling is based off the completion date by starting the project earlier based off the template
        /// timeline.
        /// Both creation and update sync comments are created for display in tactic review comment log.
        /// Does not catch exceptions - let main SyncTactic method handle these
        /// </summary>
        /// <param name="tactic">
        /// the tactic to by synced
        /// </param>
        /// <param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool SyncTacticProjectForPlanToPortfolio(Plan_Campaign_Program_Tactic tactic, ref List<SyncError> SyncErrors, ref IntegrationInstancePlanEntityLog instanceLogTactic)
        {
            bool tacticError = false;

            TacticType tacticType = db.TacticTypes.Where(type => type.TacticTypeId == tactic.TacticTypeId).FirstOrDefault();
            Enums.Mode currentMode = Common.GetMode(tactic.IntegrationWorkFrontProjectID);

            //program portfolio information -- all programs should be linked to a portfolio in WorkFront - regardless of sync type
            IntegrationWorkFrontPortfolio portfolioInfo = CreatePortfolio(tactic,ref SyncErrors);

            //if IntegrationWorkFrontProjectID doesn't exist in WorkFront, create a new one
            if (currentMode == Enums.Mode.Update)
            {
                JToken checkExists = client.Search(ObjCode.PROJECT, new { ID = tactic.IntegrationWorkFrontProjectID, map = true });
                if (checkExists == null || checkExists["data"].HasValues == false)
                { currentMode = Enums.Mode.Create; }
            }

            if (currentMode.Equals(Enums.Mode.Create))
            {
                instanceLogTactic.Operation = Operation.Create.ToString();
                if (tacticType.IntegrationWorkFrontTemplate == null || tacticType.IntegrationWorkFrontTemplate.TemplateId == null)
                {
                    throw new ClientException("Tactic Type to Template Mapping Not Found for tactic " + tactic.Title + ".");
                }

                string templateToUse = tacticType.IntegrationWorkFrontTemplate.TemplateId;
                JToken templateInfo = client.Search(ObjCode.TEMPLATE, new { ID = templateToUse });
                if (templateInfo == null || !templateInfo["data"].HasValues)
                {
                    throw new ClientException("Template " + templateToUse + " not Found in WorkFront");
                }
                string templateID = (string)templateInfo["data"][0]["ID"];

                JToken checkProgramExists = client.Search(ObjCode.PROGRAM, new { ID = tactic.Plan_Campaign_Program.Plan_Campaign.IntegrationWorkFrontProgramID });
                if (checkProgramExists == null || !checkProgramExists["data"].HasValues)
                {
                    throw new ClientException("Program " + tactic.Plan_Campaign_Program.Plan_Campaign.IntegrationWorkFrontProgramID + " not Found in WorkFront");
                }
                string programId = (string)checkProgramExists["data"][0]["ID"];
                //consolidated creation : create project with appropriate template, timeline & place in correct portfolio
                JToken project = client.Create(ObjCode.PROJECT, new
                {
                    name = HttpUtility.UrlEncode(tactic.Title),
                    templateID = templateID,
                    scheduleMode = 'C',
                    plannedCompletionDate = tactic.StartDate,
                    portfolioID = portfolioInfo.PortfolioId,
                    groupID = _userGroupID,
                    programID = programId
                });
                if (project == null || !project["data"].HasValues)
                {
                    throw new ClientException("Project Not Created for Tactic " + tactic.Title + ".");
                }

                //Enter information into WorkFront portfolio to project mapping table in database
                IntegrationWorkFrontPortfolio_Mapping createdPortfolioProject = new IntegrationWorkFrontPortfolio_Mapping();
                createdPortfolioProject.PortfolioTableId = portfolioInfo.Id;
                createdPortfolioProject.ProjectId = (string)project["data"]["ID"];
                db.Entry(createdPortfolioProject).State = EntityState.Added;
                tactic.IntegrationWorkFrontProjectID = (string)project["data"]["ID"];
                tacticError = UpdateTacticInfo(tactic, portfolioInfo, ref SyncErrors, true);

                //moved comment section to only leave a comment on successful project creation, PL#1871 : Brad Gray 01/07/2016
                if (!tacticError) //don't leave a sync comment if it didn't sync
                {
                    //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                    if (!Common.IsAutoSync)
                    {
                        //Add tactic review comment when sync tactic
                        Plan_Campaign_Program_Tactic_Comment objTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                        objTacticComment.PlanTacticId = tactic.PlanTacticId;
                        objTacticComment.Comment = Common.TacticSyncedComment + Integration.Helper.Enums.IntegrationType.WorkFront.ToString();
                        objTacticComment.CreatedDate = DateTime.Now;
                        objTacticComment.CreatedBy = this._userId;
                        db.Entry(objTacticComment).State = EntityState.Added;
                        db.Plan_Campaign_Program_Tactic_Comment.Add(objTacticComment);
                    }
                }
            }
            else if (currentMode.Equals(Enums.Mode.Update))
            {
                instanceLogTactic.Operation = Operation.Update.ToString();
                JToken updateProject = client.Update(ObjCode.PROJECT, new { id = tactic.IntegrationWorkFrontProjectID, name = HttpUtility.UrlEncode(tactic.Title) });
                if (updateProject == null || !updateProject["data"].HasValues)
                {
                    throw new ClientException("Project name is not updated for tactic " + tactic.Title + ".");
                }
                tacticError = UpdateTacticInfo(tactic, portfolioInfo, ref SyncErrors, true);
            }

            return tacticError;
        }

        private IntegrationWorkFrontPortfolio CreatePortfolio(Plan_Campaign_Program_Tactic tactic, ref List<SyncError> SyncErrors)
        {
            //program portfolio information -- all programs should be linked to a portfolio in WorkFront - regardless of sync type
            IntegrationWorkFrontPortfolio portfolioInfo = db.IntegrationWorkFrontPortfolios.Where(port => port.PlanId == tactic.Plan_Campaign_Program.Plan_Campaign.PlanId &&
                port.IntegrationInstanceId == tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt && port.IsDeleted == false).FirstOrDefault();
            JToken existingInWorkFront = null;
            if (portfolioInfo != null) // If Gameplan doesn't have an ID - can't search for a portfolio.
            {
                existingInWorkFront = client.Search(ObjCode.PORTFOLIO, new { ID = portfolioInfo.PortfolioId });
            }

            if (portfolioInfo == null || existingInWorkFront == null || !existingInWorkFront["data"].HasValues || portfolioInfo.PortfolioId != existingInWorkFront["data"][0]["ID"].ToString())
            {
                bool planError = syncPlantoPortfolio(tactic.Plan_Campaign_Program.Plan_Campaign.Plan, ref SyncErrors); //force plan sync to create a portfolio if none found

                if (planError) { throw new ClientException("Cannot Sync the Plan associated with Tactic. Tactic: " + tactic.Title + "; Plan: " + tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Title); }
                portfolioInfo = db.IntegrationWorkFrontPortfolios.Where(port => port.PlanId == tactic.Plan_Campaign_Program.Plan_Campaign.PlanId &&
                port.IntegrationInstanceId == tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt && port.IsDeleted == false).FirstOrDefault(); //try again to get info

                if (portfolioInfo == null) //check again to ensure we got information
                {
                    throw new ClientException("Cannot determine portfolio for tactic " + tactic.Title); //it didn't work - throw excecption
                }
            }
            return portfolioInfo;
        }

        /// <summary>
        /// Tactic Level sync method when syncing a tactic with a WorkFront request
        /// tactic integration logs should already be set up by the calling method
        /// need to update the log operation to either 'update' or 'create'
        /// Create new request or update tactic with existing request
        /// Creates request against selected request queue and assigns request to selected workfront user
        /// Need to check request to see if it has been converted to a project. If it has, capture the project ID, store it, and resync the tactic
        /// Only create comment on object creation
        /// Create only - Do not retrieve them, as there can be requests in WorkFront that are not supposed to by synced with Plan
        ///Plan creates and tracks requests. Do not need to pull in other requests from WorkFront as they aren't tied to Plan projects
        ///If request is in database but not in WorkFront, set the db entry to IsDeleted and set WorkFrontRequestStatus to 'deleted'
        ///<param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        /// </summary>
        ///  <param name="tactic">
        /// the tactic to by synced
        /// </param>
        ///<param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool SyncTacticRequest(Plan_Campaign_Program_Tactic tactic, ref List<SyncError> SyncErrors, ref IntegrationInstancePlanEntityLog instanceLogTactic )
        {
            bool syncError = false;
            //update WorkFront requests for the instance
            try
            {
                //Get the list of requests in the database
                IntegrationWorkFrontRequest requestFromDB = db.IntegrationWorkFrontRequests.SingleOrDefault(r => r.PlanTacticId == tactic.PlanTacticId && r.IntegrationInstanceId == _integrationInstanceId && r.IsDeleted == false);
                if (requestFromDB == null) { throw new ClientException("No available request settings in tactic " + tactic.Title);}

                if (requestFromDB.RequestId == null) //no request ID in DB yet - need to create it and push to WorkFront
                {
                    string assignee = db.IntegrationWorkFrontUsers.Single(u => u.Id == requestFromDB.WorkFrontUserId).WorkFrontUserId;
                    string requestQueueId = db.IntegrationWorkFrontRequestQueues.Single(q => q.Id == requestFromDB.QueueId).RequestQueueId;
                    JToken queue = client.Search(ObjCode.QUEUE, new {ID = requestQueueId, fields="projectID"});
                    string projectForRequest = queue["data"][0]["projectID"].ToString();
                    JToken request = client.Create(ObjCode.OPTASK, new { projectID = projectForRequest, name = HttpUtility.UrlEncode(requestFromDB.RequestName), isHelpDesk = "true", opTaskType = "REQ", assignedToID = assignee });
                    if (request == null)
                    {
                        throw new ClientException("Cannot create request for tactic " + tactic.Title);
                    }
                    else
                    {
                        requestFromDB.RequestId = request["data"]["ID"].ToString();
                        requestFromDB.WorkFrontRequestStatus = request["data"]["status"].ToString();
                    }
                    //Modified by Rahul Shah on 02/03/2016 for PL #1978 . 
                    if (!Common.IsAutoSync)
                    {
                        //Add tactic review comment on initial creation
                        Plan_Campaign_Program_Tactic_Comment objTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                        objTacticComment.PlanTacticId = tactic.PlanTacticId;
                        objTacticComment.Comment = Common.TacticSyncedComment + Integration.Helper.Enums.IntegrationType.WorkFront.ToString();
                        objTacticComment.CreatedDate = DateTime.Now;
                        //if (Common.IsAutoSync)
                        //{
                        //   objTacticComment.CreatedBy = new Guid();
                        //}
                        //else
                        //{
                        objTacticComment.CreatedBy = this._userId;
                        //}
                        db.Entry(objTacticComment).State = EntityState.Added;
                        db.Plan_Campaign_Program_Tactic_Comment.Add(objTacticComment);
                    }
                }
                else
                {
                    JToken requestInfoFromWorkFront = client.Search(ObjCode.OPTASK, new { fields = "assignedToID,categoryID,isHelpDesk,resolvingObjCode,resolvingObjID,status", isHelpDesk = "true", ID = requestFromDB.RequestId });
                    if (requestInfoFromWorkFront != null) //sync it
                    {
                        requestFromDB.RequestName = requestInfoFromWorkFront["data"][0]["name"].ToString();
                        requestFromDB.WorkFrontRequestStatus = requestInfoFromWorkFront["data"][0]["status"].ToString();
                        requestFromDB.ResolvingObjType = requestInfoFromWorkFront["data"][0]["resolvingObjCode"].ToString();
                        requestFromDB.ResolvingObjId = requestInfoFromWorkFront["data"][0]["resolvingObjID"].ToString();

                        if (requestFromDB.ResolvingObjType == ObjCode.PROJECT.ToString().ToUpper() && requestFromDB.ResolvingObjId != null)
                        {
                            //request has been converted to a project - store it in in the tactic table
                            tactic.IntegrationWorkFrontProjectID = requestFromDB.ResolvingObjId;
                            //Save the data && resync the tactic so it syncs with the project now
                            db.Entry(requestFromDB).State = EntityState.Modified;
                            db.SaveChanges();
                            SyncTacticProject(tactic, ref SyncErrors, ref instanceLogTactic);

                        }
                    }
                    else //it's in the database and not set to deleted. Set it to deleted and set the status to deleted
                    {
                        requestFromDB.IsDeleted = true;
                        requestFromDB.WorkFrontRequestStatus = "Deleted";
                    }
                    db.Entry(requestFromDB).State = EntityState.Modified;
                }
            }
            catch (Exception ex)
            {
                syncError = true;
                SyncError error = new SyncError();
                error.EntityId = _integrationInstanceId;
                error.EntityType = Enums.EntityType.IntegrationInstance;
                error.SectionName = "Sync Tactic Request";
                error.Message = ex.Message;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
            }
            return syncError;
        }


        /// <summary>
        /// Tactic Level update method 
        /// Used for updating existign tactics.
        /// Update happens in two steps - write to WorkFront, then read from WorkFront and write to Gp
        /// Per Workfront API guide, Editing must be done with project ID. 
        /// Edits can be either by listing fields separated by '&' or by JSON ('updates=JSON')
        /// Builds a JSON from update parameters to use the JSON edit method.
        /// client.update() returns the edited project for verification and further use.
        /// </summary>
        ///  <param name="tactic">
        /// the tactic to by synced
        /// </param>
        /// <param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// Bool: true if sync error, false otherwise
        /// </returns>
        private bool UpdateTacticInfo(Plan_Campaign_Program_Tactic tactic, IntegrationWorkFrontPortfolio portfolioInfo, ref List<SyncError> SyncErrors, bool isPlanToPortfolio = false)
        {
            bool updateError = false;
            try
            {
                StringBuilder updateList = new StringBuilder("{"); //use for update JSON
                JToken projectForEdits = client.Search(ObjCode.PROJECT, new { id = tactic.IntegrationWorkFrontProjectID, fields = "portfolioID" });
                if (projectForEdits == null) { throw new ClientException("Cannot find project for updating in tactic " + tactic.Title); }

                //---------Begin Portfolio Organization -----------------------//
                //Ensure projects are under the correct portfolio in WorkFront
                string workFrontPortfolio = (string) projectForEdits["data"][0]["portfolioID"];
                if (workFrontPortfolio == null || workFrontPortfolio != portfolioInfo.PortfolioId) //project is not organized into a correct portfolio in WorkFront
                {
                    updateList.Append("portfolioID: '" + portfolioInfo.PortfolioId + "',"); //add the correct portfolio information to the updates JSON
                }

                //Ensure mapping is correct in Gameplan
                IntegrationWorkFrontPortfolio_Mapping existingMap = db.IntegrationWorkFrontPortfolio_Mapping.Where(map => map.ProjectId == tactic.IntegrationWorkFrontProjectID).FirstOrDefault();
                if (existingMap == null) //the mapping doesn't exist
                {
                    IntegrationWorkFrontPortfolio_Mapping newMap = new IntegrationWorkFrontPortfolio_Mapping();
                    newMap.ProjectId = tactic.IntegrationWorkFrontProjectID;
                    newMap.PortfolioTableId = portfolioInfo.Id;
                    newMap.IsDeleted = false;
                    db.Entry(newMap).State = EntityState.Added;
                }
                else if (existingMap.PortfolioTableId != portfolioInfo.Id) //project is mapped to the wrong portfolio
                {
                    existingMap.IsDeleted = true;
                    IntegrationWorkFrontPortfolio_Mapping newMap = new IntegrationWorkFrontPortfolio_Mapping();
                    newMap.ProjectId = tactic.IntegrationWorkFrontProjectID;
                    newMap.PortfolioTableId = portfolioInfo.Id;
                    newMap.IsDeleted = false;
                    db.Entry(newMap).State = EntityState.Added;
                    db.Entry(existingMap).State = EntityState.Modified;
                }
                //---------End Portfolio Organization -----------------------//

                //Begin push to WorkFront only sync
                //Modified by Brad Gray 20 March 2016 PL#2070

                var last = _mappingTacticPushData.Last();
                updateList.Append(GenerateUpdateData(tactic, isPlanToPortfolio));
                JToken jUpdates = JToken.FromObject(updateList.ToString());
                
                JToken project = client.Update(ObjCode.PROJECT, new { id = tactic.IntegrationWorkFrontProjectID, fields = "parameterValues,status", updates = jUpdates });
                if (project == null) { throw new ClientException("Update not completed on " + tactic.Title + "."); }
                //End push to WorkFront only sync

                //Begin read only sync - Modified by Brad Gray 27 March 2016 PL#2084
                foreach (KeyValuePair<String, String> tacticField in _mappingCustomFieldPullData) 
                {
          
                    int CustomFieldId;
                    String updateValue = "";
                    if(!Int32.TryParse(tacticField.Key, out CustomFieldId)){throw new ClientException("Can only push data into Plan custom fields");}
                    CustomField_Entity cfe = db.CustomField_Entity.Where(field => field.CustomFieldId == CustomFieldId && tactic.PlanTacticId == field.EntityId 
                        && field.CustomField.IsGet == true).FirstOrDefault();

                    if (tacticField.Value == Fields.WorkFrontField.WORKFRONTPROJECTSTATUS.ToAPIString())
                    {
                        updateValue = (string)project["data"]["status"];
                    }
                    else if (project["data"]["parameterValues"].HasValues)
                    {
                       
                        String temp1 = tacticField.Value.ToString();
                        String param = temp1.Substring(1, temp1.Length - 2); //remove starting & ending ' characters

                        if (project["data"]["parameterValues"][param] != null)
                        {
                            updateValue = project["data"]["parameterValues"][param].ToString(); 
                        }
                    }
                    else
                    {
                        updateValue = "Cannot update";
                    }
                  
                   if (cfe == null) //need to create a CustomFieldEntity
                   {
                      CustomField_Entity customField = new CustomField_Entity();
                      customField.CustomFieldId = CustomFieldId;
                      customField.EntityId = tactic.PlanTacticId;
                      customField.Value = updateValue;
                      customField.CreatedDate = DateTime.Now;
                      customField.CreatedBy = _userId;
                      db.Entry(customField).State = EntityState.Added;
                    }
                    else
                    {
                      cfe.Value = updateValue;
                      db.Entry(cfe).State = EntityState.Modified;
                    }
                 }
                //End read only sync
            }
            catch(Exception ex)
            {
                updateError = true;
                SyncError error = new SyncError();
                error.EntityId = tactic.PlanTacticId;
                error.EntityType = Enums.EntityType.Tactic;
                error.SectionName = "Update Tactic Data";
                error.Message = "In tactic " + tactic.Title + ": " + ex.Message;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
            }
            return updateError;
        }

        /// <summary>
        /// Method to create an update string for use with API JSON update
        /// Used for updating existing tactics.
        /// Originally part of UpdateTactic method. Separated out as it started growing
        /// Brad Gray 20 March 2016 PL#2071
        /// </summary>
        ///  <param name="tactic">
        /// the tactic to by synced
        /// </param>
        /// <returns>
        /// StringBuilder - fields for JSON update
        /// </returns>
        private StringBuilder GenerateUpdateData(Plan_Campaign_Program_Tactic tactic,bool isPlanToPortfolio = false)
        {
            // int notUsedFieldCount = 0;
            var last = _mappingTacticPushData.Last();
            StringBuilder updateList = new StringBuilder();
            foreach (KeyValuePair<String, String> tacticField in _mappingTacticPushData) //create JSON for editing
            {
                // Modified by Arpita Soni for Ticket #2304 on 06/27/2016
                if (tacticField.Key == Fields.GameplanField.TITLE.ToString())
                {
                    // Modified by Arpita Soni for Ticket #2320 on 06/29/2016
                    updateList.Append(tacticField.Value + ":'" + HttpUtility.UrlEncode(HttpUtility.HtmlDecode(tactic.Title.Replace(@"\",@"\\").Replace("'", "\\'"))) + "'");
                }
                else if (tacticField.Key == Fields.GameplanField.DESCRIPTION.ToString())
                {
                    updateList.Append(tacticField.Value + ":'" + HttpUtility.UrlEncode(tactic.Description) + "'");
                }
                else if (tacticField.Key == Fields.GameplanField.START_DATE.ToAPIString())
                {
                    updateList.Append(tacticField.Value + ":'" + tactic.StartDate + "'");
                }
                else if (tacticField.Key == Fields.GameplanField.END_DATE.ToAPIString())
                {
                    updateList.Append(tacticField.Value + ":'" + tactic.EndDate + "'");
                }
                else if (tacticField.Key == Fields.GameplanField.PARENT_PROGRAM.ToAPIString())
                {
                    // Modified by Arpita Soni for Ticket #2320 on 06/29/2016
                    updateList.Append(tacticField.Value + ":'" + HttpUtility.UrlEncode(HttpUtility.HtmlDecode(tactic.Plan_Campaign_Program.Title.Replace(@"\", @"\\").Replace("'", "\\'"))) + "'");
                }
                else if (tacticField.Key == Fields.GameplanField.PARENT_CAMPAIGN.ToAPIString())
                {
                    // Modified by Arpita Soni for Ticket #2320 on 06/29/2016
                    updateList.Append(tacticField.Value + ":'" + HttpUtility.UrlEncode(HttpUtility.HtmlDecode(tactic.Plan_Campaign_Program.Plan_Campaign.Title.Replace(@"\", @"\\").Replace("'", "\\'"))) + "'");
                }
                else if (tacticField.Key == Fields.GameplanField.PROGRAM_END.ToAPIString())
                {
                    updateList.Append(tacticField.Value + ":'" + tactic.Plan_Campaign_Program.EndDate + "'");
                }
                else if (tacticField.Key == Fields.GameplanField.PROGRAM_OWNER.ToAPIString())
                {
                    BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                    BDSService.User user = objBDSServiceClient.GetTeamMemberDetails(tactic.Plan_Campaign_Program.Plan_Campaign.CreatedBy, _applicationId);
                    updateList.Append(tacticField.Value + ":'" + HttpUtility.UrlEncode(user.Email) + "'");
                }
                else if (tacticField.Key == Fields.GameplanField.PROGRAM_START.ToAPIString())
                {
                    updateList.Append(tacticField.Value + ":'" + tactic.Plan_Campaign_Program.StartDate + "'");
                }
                else if (tacticField.Key == Fields.GameplanField.CAMPAIGN_END.ToAPIString())
                {
                    updateList.Append(tacticField.Value + ":'" + tactic.Plan_Campaign_Program.Plan_Campaign.EndDate + "'");
                }
                else if (tacticField.Key == Fields.GameplanField.CAMPAIGN_OWNER.ToAPIString())
                {
                    BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                    BDSService.User user = objBDSServiceClient.GetTeamMemberDetails(tactic.Plan_Campaign_Program.Plan_Campaign.CreatedBy, _applicationId);
                    updateList.Append(tacticField.Value + ":'" + HttpUtility.UrlEncode(user.Email) + "'");
                }
                else if (tacticField.Key == Fields.GameplanField.CAMPAIGN_START.ToAPIString())
                {
                    updateList.Append(tacticField.Value + ":'" + tactic.Plan_Campaign_Program.Plan_Campaign.StartDate + "'");
                }
                else if (_customFieldIds.Contains(tacticField.Key))
                {
                    int keyAsInt;
                    if (!Int32.TryParse(tacticField.Key, out keyAsInt)) { throw new ClientException("Error converting Custom Field ID to integer"); }
                    CustomField_Entity cfe = db.CustomField_Entity.Where(field => field.CustomFieldId == keyAsInt && tactic.PlanTacticId == field.EntityId && field.CustomField.IsGet == false).FirstOrDefault();
                    if (!(cfe == null))
                    {
                        updateList.Append(tacticField.Value + ":'" + HttpUtility.UrlEncode(cfe.Value) + "'");
                    }
                    // else { notUsedFieldCount++; }
                }
                else if (tacticField.Key == Fields.GameplanField.PROGRAM_NAME.ToAPIString() && isPlanToPortfolio)
                {
                    // Modified by Arpita Soni for Ticket #2320 on 06/29/2016
                    // Push plan program to attribute of the WF project
                    updateList.Append(tacticField.Value + ":'" + HttpUtility.UrlEncode(HttpUtility.HtmlDecode(tactic.Plan_Campaign_Program.Title.Replace(@"\", @"\\").Replace("'", "\\'"))) + "'");
                }
                else { continue; }
                updateList.Append(",");
            }

            updateList.Remove(updateList.Length - 1, 1); //remove the final comma(s) - there will always be at least one final comma
            
            updateList.Append("}");
           return updateList;
        }



        /// <summary>
        /// Function to set mapping details.
        /// </summary>
        /// <param name="SyncErrors">
        /// error list for tracking
        /// </param>
        /// <returns>
        /// true if errors encountered
        /// </returns>
        private bool SetMappingDetails(ref List<SyncError> SyncErrors)
        {
            List<string> mappingTypeErrors = new List<string>();
            bool mappingError = false;
            try
            {
                string Global = Enums.IntegrantionDataTypeMappingTableName.Global.ToString();
                string Tactic_EntityType = Enums.EntityType.Tactic.ToString();
                string Plan_Campaign = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign.ToString();
                string Plan_Campaign_Program = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program.ToString();
                string Plan_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program_Tactic.ToString();
                string Plan_Improvement_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Improvement_Campaign_Program_Tactic.ToString();
                List<Fields.WorkFrontField> wfFields = Fields.GetWorkFrontFieldDetails();
                List<IntegrationInstanceDataTypeMapping> dataTypeMapping = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.IntegrationInstanceId.Equals(_integrationInstanceId)).ToList();
                _mappingTacticPushData = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ?  ( (gameplandata.GameplanDataType.TableName == Plan_Campaign_Program_Tactic
                                                    || gameplandata.GameplanDataType.TableName == Global || gameplandata.GameplanDataType.TableName == Plan_Campaign_Program
                                                    || gameplandata.GameplanDataType.TableName == Plan_Campaign)  && gameplandata.GameplanDataType.IsGet == false) : (gameplandata.CustomField.EntityType == Tactic_EntityType && gameplandata.CustomField.IsGet == false) ) )
                                                .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                _mappingCustomFieldPullData = dataTypeMapping.Where(gameplandata => gameplandata.CustomFieldId != null && gameplandata.CustomField.IsGet == true && gameplandata.CustomField.EntityType == Tactic_EntityType)
                                                .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);
                _mappingTacticCombined = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign_Program_Tactic
                                                    || gameplandata.GameplanDataType.TableName == Global || gameplandata.GameplanDataType.TableName == Plan_Campaign_Program
                                                    || gameplandata.GameplanDataType.TableName == Plan_Campaign) :
                                                    (gameplandata.CustomField.EntityType == Tactic_EntityType) ))
                                                .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

                foreach (KeyValuePair<string, string> entry in _mappingTacticCombined.Where(mt => Enums.ActualFieldDatatype.Keys.Contains(mt.Key)))
                {
                    if (Enums.ActualFieldDatatype.ContainsKey(entry.Key) && wfFields.Where(wf => wf.apiString == entry.Value).FirstOrDefault() != null)
                    {

                        if (!Enums.ActualFieldDatatype[entry.Key].Contains(wfFields.Where(wf => wf.apiString == entry.Value).FirstOrDefault().dataType))
                        {

                            mappingTypeErrors.Add("Cannot map " + entry.Key + " to " + entry.Value + ".");
                            mappingError = true;
                        }
                    }
                }

                dataTypeMapping = dataTypeMapping.Where(gp => gp.GameplanDataType != null).Select(gp => gp).ToList();
                _clientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId).ClientId;
                BDSService.BDSServiceClient objBDSservice = new BDSService.BDSServiceClient();
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
                if (mappingError) { throw new ClientException("Error in field mapping"); }
            }
            catch(Exception ex)
            {
                string mappingErrors = string.Join(",", mappingTypeErrors);
                SyncError error = new SyncError();
                error.EntityId = _integrationInstanceId;
                error.SectionName = "Sync Data";
                error.EntityType = Enums.EntityType.IntegrationInstance;
                error.Message = string.Concat(ex.Message, " : ", mappingErrors) ;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
            }

            return mappingError;
        }

        /// <summary>
        /// Adds an issue to a project by project ID. Not likely to be used by Gp. 
        /// Displays the issue after creation with default info for visual verification.
        /// </summary>
        /// <param name="projectID">
        /// ID of the project as used by Workfront as a string. ProjectID is safest as Workfront allows
        /// for duplicated names
        /// </param>
        private void AddIssue(string projectID)
        {
            Console.WriteLine("** Adding issues to project...");
            for(int i = 1; i <= 3; i++) {
                string issueName = "issue " + i.ToString();
                client.Create(ObjCode.ISSUE, new { name = HttpUtility.UrlEncode(issueName), projectID = projectID });
            }
            Console.WriteLine(client.Search(ObjCode.ISSUE, new { projectID = projectID, fields = "projectID" }));
            Console.WriteLine("Done");
            Console.WriteLine();
        }

        /// <summary>
        /// Searches for and displays all issues, messages, and submessages attached to a project.
        /// Currently uses 3 nested loops to display issues, messages and submessages.
        /// Likely a more efficient way to obtain and display everything.
        /// Note information doesn't contain the posters name, just the ID, so a crossreference must be made to 
        /// USER objcode.
        /// </summary>
        /// <param name="projectID">
        /// ID of the project as used by Workfront as a string. ProjectID is safest as Workfront allows
        /// for duplicated names
        /// </param>
        private void SearchProjectIssues(string projectID)
        {
            Console.WriteLine("*Searching for Project Issues\n");
            string desiredFields = "updates:enteredByName,updates:replies:ownerID, updates:replies:noteText, updates:replies:isPrivate";
            JToken issues = client.Search(ObjCode.ISSUE, new { projectID = projectID, fields = desiredFields });
            foreach (var j in issues["data"].Children())
            {
                Console.WriteLine("Issue Name : " + j["name"]);
                foreach(var k in j["updates"].Children())
                {
                    Console.WriteLine(k["message"] + " - " + k["enteredByName"]);
                    if( k["replies"].HasValues)
                    {
                        foreach(var reply in k["replies"].Children())
                        {
                            JToken replyUserInfo = client.Search(ObjCode.USER, new { ID = reply["ownerID"] });
                            Console.WriteLine("--> " + reply["noteText"] + " - " + replyUserInfo["data"][0]["name"]);
                        }
                    }
                }
                Console.WriteLine(" ");
            }
        }

        /// <summary>Returns a list of all workfront fields as an API as stored in Fields.cs  </summary>
        /// <returns>
        /// list of all workfront fields as strings
        /// </returns>
        public List<string> getWorkFrontFields()
        {
            List<string> WorkfrontCustomFields = GetCustomFieldList();
            List<string> WorkFrontFields = Fields.ReturnAllWorkFrontFields_AsAPI();
            if (WorkfrontCustomFields.Count > 0) { WorkFrontFields.AddRange(WorkfrontCustomFields); }
            return WorkFrontFields;

        }

        /// <summary>
        /// Returns a list of all workfront custom fields
        /// Retrieves all custom fields (parameters) from the workfront instance
        /// Preprends "'DE:" to each field's name and adds to a list. This DE: is needed
        /// as "'DE: <fieldname>'" is used by workfront for field access
        /// Added by Brad Gray 20 March 2016 PL#2070
        /// </summary>
        /// <returns>
        /// list of all workfront custom fields as strings
        /// </returns>
        private List<string> GetCustomFieldList()
        {
            List<string> customNames = new List<string>();
            try
            {
                if(client!=null)
                {
                    JToken custom = client.Search(ObjCode.PARAMETER, new { displayType = "TEXT" });
                    if (custom == null || !custom.HasValues) { return customNames; }
                    foreach (var param in custom["data"].Children())
                    {
                        customNames.Add("'DE:" + param["name"]+"'");
                    }
                }
               
               
            }
            catch (Exception)
            {
                throw new ClientException("Failed to retrieve custom fields");
            }

            return customNames;
        }

        /// <summary>
        /// Description : Taken and modified from Sohel Pathan's method CreateMappingCustomFieldDictionary in IntegrationEloquaClient.cs
        /// Provides a List of Custom Field IDs for comparing against _mappingTactic Dictionary.
        /// </summary>
        /// <param name="EntityIdList">List of tactic ids for comparing against custom field entity ids.</param>
        /// <param name="SyncErrors"> error list for tracking </param>
        ///<returns> true if errors encountered </param>
        private bool CreateCustomFieldIdList(List<int> EntityIdList, ref List<SyncError> SyncErrors)
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            bool mappingError = false;
            try
            {
                if (EntityIdList.Count > 0)
                {
                    _customFieldIds = new List<string>();
                    string idList = string.Join(",", EntityIdList);
                    //create query to return all custom field and custom field entity info for the entity (currently always instance)
                    String Query = "select cast(cf.CustomFieldId as nvarchar) from [dbo].[CustomField] cf inner join [dbo].[CustomField_Entity]" +
                            "cfe on cfe.CustomFieldId = cf.CustomFieldId where cf.EntityType = 'Tactic' and cf.IsDeleted = 0 and cfe.EntityId in (" + idList + ")";

                    MRPEntities mp = new MRPEntities();
                    DbConnection conn = mp.Database.Connection;
                    conn.Open();
                    DbCommand comm = conn.CreateCommand();
                    comm.CommandText = Query;
                    DbDataReader ddr = comm.ExecuteReader();

                    while (ddr.Read())
                    {
                        _customFieldIds.Add(!ddr.IsDBNull(0) ? ddr.GetString(0) : string.Empty);
                    }
                    conn.Close();
                    mp.Dispose();
                }
            }
            catch (Exception ex)
            {
                string exMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                mappingError = true;
                SyncError error = new SyncError();
                error.EntityId = _integrationInstanceId;
                error.SectionName = "Sync Data";
                error.EntityType = Enums.EntityType.IntegrationInstance;
                error.Message = ex.Message;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
            }
            return mappingError;
        }

    }
}

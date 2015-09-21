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
        private Dictionary<string, string> _mappingTactic { get; set; }
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

                     bool syncError = syncProgram(planProgram, ref SyncErrors);
                     if (syncError) { throw new ClientException("Cannot Sync the Program associated with Tactic " + planTactic.Title); }
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
                             
               if (currentMode.Equals(Enums.Mode.Create))
               {
                   instanceLogProgram.Operation = Operation.Create.ToString();
                   program.LastSyncDate = DateTime.Now;
                   program.ModifiedDate = DateTime.Now;
                   program.ModifiedBy = _userId;
                   string defaultPortfolioName = program.Plan_Campaign.Title + " : " + program.Title;
                   JToken createdPortfolio = client.Create(ObjCode.PORTFOLIO, new { name = defaultPortfolioName});
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
                   JToken existingPortfolio = client.Search(ObjCode.PORTFOLIO, new { ID = portfolio.PortfolioId });
                   if (existingPortfolio == null) { throw new ClientException("Portfolio Not found in WorkFront but exists in Database."); }
                   portfolio.PortfolioName = (string)existingPortfolio["data"][0]["name"];
                   portfolio.IsDeleted = false;
                   db.Entry(portfolio).State = EntityState.Modified;
                   syncError = false;
               }

               if (!syncError) //don't leave a sync comment if it didn't sync
               {
                   //Add program review comment when synced
                   Plan_Campaign_Program_Tactic_Comment objProgramComment = new Plan_Campaign_Program_Tactic_Comment();
                   objProgramComment.PlanProgramId = program.PlanProgramId;
                   objProgramComment.Comment = Common.ProgramSyncedComment + Integration.Helper.Enums.IntegrationType.WorkFront.ToString();
                   objProgramComment.CreatedDate = DateTime.Now;
                   if (Common.IsAutoSync)
                   {
                       objProgramComment.CreatedBy = new Guid();
                   }
                   else { objProgramComment.CreatedBy = this._userId;    }

                   db.Entry(objProgramComment).State = EntityState.Added;
                   db.Plan_Campaign_Program_Tactic_Comment.Add(objProgramComment);
                   //add success message to IntegrationInstanceLogDetails

                   
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



        /// <summary>
        /// Tactic Level sync method 
        /// sets up tactic integration logs, create new tactic or updates existing tactic
        /// Creates project with attached template.
        /// All in one api command not yet found, so method is completed with a few api calls.
        /// First: create the project with projectName. Second: attach the template (currently hard coded).
        /// Third: Edit the scheduling mode to schedule from the completion date, and edit the completion date itself.
        /// Workfront scheduling is based off the completion date by starting the project earlier based off the template
        /// timeline.
        /// Both creation and update sync comments are created for display in tactic review comment log.
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
            TacticType tacticType = db.TacticTypes.Where(type =>  type.TacticTypeId == tactic.TacticTypeId).FirstOrDefault();
            
            IntegrationInstancePlanEntityLog instanceLogTactic = new IntegrationInstancePlanEntityLog();
            try
            {
                Enums.Mode currentMode = Common.GetMode(tactic.IntegrationWorkFrontProjectID);
                //if IntegrationWorkFrontProjectID doesn't exist in WorkFront, create a new one
                if (currentMode == Enums.Mode.Update)
                {
                    JToken checkExists = client.Search(ObjCode.PROJECT, new { ID = tactic.IntegrationWorkFrontProjectID, map = true });
                    if (checkExists == null || checkExists["data"].HasValues == false)
                    { currentMode = Enums.Mode.Create; }
                }
                //logging begin
                instanceLogTactic.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                instanceLogTactic.IntegrationInstanceId = _integrationInstanceId;
                instanceLogTactic.EntityId = tactic.PlanTacticId;
                instanceLogTactic.EntityType = EntityType.Tactic.ToString();
                instanceLogTactic.SyncTimeStamp = DateTime.Now;
                instanceLogTactic.Status = StatusResult.Success.ToString();
                instanceLogTactic.CreatedBy = this._userId;
                instanceLogTactic.CreatedDate = DateTime.Now;
                
                //logging end

                tactic.LastSyncDate = DateTime.Now;
                tactic.ModifiedDate = DateTime.Now;
                tactic.ModifiedBy = _userId;
                db.Entry(tactic).State = EntityState.Modified;

                //program portfolio information -- all programs should be linked to a portfolio in WorkFront
                IntegrationWorkFrontPortfolio portfolioInfo = db.IntegrationWorkFrontPortfolios.Where(port => port.PlanProgramId == tactic.PlanProgramId &&
                    port.IntegrationInstanceId == tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt && port.IsDeleted == false).FirstOrDefault();
                if (portfolioInfo == null) 
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


                if (currentMode.Equals(Enums.Mode.Create))
                {
                    instanceLogTactic.Operation = Operation.Create.ToString();
                    string templateToUse = tacticType.WorkFront_Template;
                    if (templateToUse == null)
                    {
                        throw new ClientException("Tactic Type to Template Mapping Not Found for tactic " + tactic.Title + ".");
                    }
                    JToken templateInfo = client.Search(ObjCode.TEMPLATE, new { ID = templateToUse });
                    if (templateInfo == null || !templateInfo["data"].HasValues)
                    {
                        throw new ClientException("Template " + templateToUse + " not Found in WorkFront");
                    }
                    string templateID = (string)templateInfo["data"][0]["ID"];

                    //consolidated creation : create project with appropriate template, timeline & place in correct portfolio
                    JToken project = client.Create(ObjCode.PROJECT, new { name = tactic.Title, templateID = templateID, scheduleMode = 'C', 
                        plannedCompletionDate = tactic.StartDate, portfolioID = portfolioInfo.PortfolioId, groupID = _userGroupID });
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
                    //tactic.IntegrationInstanceTacticId = (string)project["data"]["ID"]; //needed only as place filler for Common.GetMode. Not used elsewhere.
                    tacticError = UpdateTacticInfo(tactic, portfolioInfo, ref SyncErrors);
                }
                else if (currentMode.Equals(Enums.Mode.Update))
                {
                    instanceLogTactic.Operation = Operation.Update.ToString();
                    
                    tacticError = UpdateTacticInfo(tactic, portfolioInfo, ref SyncErrors);

                    
                }

                if (!tacticError) //don't leave a sync comment if it didn't sync
                {
                    //Add tactic review comment when sync tactic
                    Plan_Campaign_Program_Tactic_Comment objTacticComment = new Plan_Campaign_Program_Tactic_Comment();
                    objTacticComment.PlanTacticId = tactic.PlanTacticId;
                    objTacticComment.Comment = Common.TacticSyncedComment + Integration.Helper.Enums.IntegrationType.WorkFront.ToString();
                    objTacticComment.CreatedDate = DateTime.Now;
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
                    //add success message to IntegrationInstanceLogDetails
                    Common.SaveIntegrationInstanceLogDetails(_integrationInstanceId, null, Enums.MessageOperation.Start, "Sync Tactic", Enums.MessageLabel.Success, "Sync success on Tactic " + tactic.Title);
                }
            }
            catch(Exception ex)
            {
                tacticError = true;
                instanceLogTactic.Status = StatusResult.Error.ToString();
                instanceLogTactic.ErrorDescription = ex.Message;
                SyncError error = new SyncError();
                error.EntityId = tactic.PlanTacticId;
                error.EntityType = Enums.EntityType.Tactic;
                error.SectionName = "Sync Tactic Data";
                error.Message = "In tactic " + tactic.Title + " : " + ex.Message;
                error.SyncStatus = Enums.SyncStatus.Error;
                error.TimeStamp = DateTime.Now;
                SyncErrors.Add(error);
            }
            finally
            {
                db.Entry(instanceLogTactic).State = EntityState.Added;
            }
            return tacticError;
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
        private bool UpdateTacticInfo(Plan_Campaign_Program_Tactic tactic, IntegrationWorkFrontPortfolio portfolioInfo, ref List<SyncError> SyncErrors)
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
                int notUsedFieldCount = 0;
                var last = _mappingTactic.Last();
                foreach (var tacticField in _mappingTactic) //create JSON for editing
                {
                    if (tacticField.Key == Fields.ToStringEnums(Fields.GamePlanFields.TITLE))
                    {
                        updateList.Append(tacticField.Value + ":'" + tactic.Title + "'");
                    }
                    else if (tacticField.Key ==Fields.ToStringEnums(Fields.GamePlanFields.DESCRIPTION))
                    {
                        updateList.Append(tacticField.Value + ":'" + tactic.Description + "'");
                    }
                    else if (tacticField.Key ==Fields.ToStringEnums(Fields.GamePlanFields.START_DATE) )
                    {
                        updateList.Append(tacticField.Value + ":'" + tactic.StartDate + "'");
                    }
                    else if (tacticField.Key ==Fields.ToStringEnums(Fields.GamePlanFields.END_DATE))
                    {
                        updateList.Append(tacticField.Value + ":'" + tactic.EndDate + "'");
                    }
                     else if (tacticField.Key ==Fields.ToStringEnums(Fields.GamePlanFields.COST))
                    {
                        updateList.Append(tacticField.Value + ":'" + tactic.Cost + "'");
                    }
                    else if (tacticField.Key ==Fields.ToStringEnums(Fields.GamePlanFields.TACTIC_BUDGET))
                    {
                        updateList.Append(tacticField.Value + ":'" + tactic.TacticBudget + "'");
                    }
                    else if (tacticField.Key ==Fields.ToStringEnums(Fields.GamePlanFields.STATUS))
                    {
                        updateList.Append(tacticField.Value + ":'" + tactic.Status + "'");
                    }
                    else if (_customFieldIds.Contains(tacticField.Key))
                    {
                        int keyAsInt;
                        if (!Int32.TryParse(tacticField.Key, out keyAsInt)) { throw new ClientException("Error converting Custom Field ID to integer"); }
                        CustomField_Entity cfe = db.CustomField_Entity.Where(field => field.CustomFieldId == keyAsInt && tactic.PlanTacticId == field.EntityId && field.CustomField.IsGet == false).FirstOrDefault();
                        if ( !(cfe == null) ) 
                        {
                         updateList.Append(tacticField.Value + ":'" + cfe.Value + "'");
                        }else { notUsedFieldCount++; }
                    }
                    else { notUsedFieldCount++; }
                    updateList.Append(",");
                }

                updateList.Remove(updateList.Length - (notUsedFieldCount+1), notUsedFieldCount+1); //remove the final comma(s) - there will always be at least one final comma
                updateList.Append("}");
                JToken jUpdates = JToken.FromObject(updateList.ToString());
                JToken project = client.Update(ObjCode.PROJECT, new { id = tactic.IntegrationWorkFrontProjectID, updates = jUpdates });
                if (project == null) { throw new ClientException("Update not completed on " + tactic.Title + "."); }
                //End push to WorkFront only sync

                //Begin read only sync
                tactic.TacticCustomName = (string)project["data"]["name"];
                if (_mappingTactic.ContainsValue(Fields.WorkFrontField.WORKFRONTPROJECTSTATUS.ToAPIString()))
                {
                    CustomField_Entity customField;
                    string status = (string)project["data"]["status"];
                    foreach (var tacticField in _mappingTactic) 
                    {
                        if (tacticField.Value == Fields.WorkFrontField.WORKFRONTPROJECTSTATUS.ToAPIString())
                        {
                            int CustomFieldID = Convert.ToInt32(tacticField.Key); 
                            var CustomFieldData= db.CustomField_Entity.Where(ce => tactic.PlanTacticId == ce.EntityId && ce.CustomFieldId == CustomFieldID)
                                                      .Select(ce => new { ce }).ToList();
                            if( CustomFieldData.Count==0) //need to create a CustomFieldEntity
                            {
                                customField = new CustomField_Entity();
                                customField.CustomFieldId = CustomFieldID;
                                customField.EntityId = tactic.PlanTacticId;
                                customField.Value = status;
                                customField.CreatedDate = DateTime.Now;
                                customField.CreatedBy = _userId;
                                db.Entry(customField).State = EntityState.Added;
                            }
                            else{
                                customField = CustomFieldData[0].ce;
                                customField.Value = status;
                                db.Entry(customField).State = EntityState.Modified;
                            }
                          
                        }
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
                string Plan_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program_Tactic.ToString();
                string Plan_Improvement_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Improvement_Campaign_Program_Tactic.ToString();
                List<Fields.WorkFrontField> wfFields = Fields.GetWorkFrontFieldDetails();
                List<IntegrationInstanceDataTypeMapping> dataTypeMapping = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.IntegrationInstanceId.Equals(_integrationInstanceId)).ToList();
                _mappingTactic = dataTypeMapping.Where(gameplandata => (gameplandata.GameplanDataType != null ? (gameplandata.GameplanDataType.TableName == Plan_Campaign_Program_Tactic
                                                    || gameplandata.GameplanDataType.TableName == Global) : gameplandata.CustomField.EntityType == Tactic_EntityType) &&
                                                    (gameplandata.GameplanDataType != null ? !gameplandata.GameplanDataType.IsGet : true))
                                                .Select(mapping => new { ActualFieldName = mapping.GameplanDataType != null ? mapping.GameplanDataType.ActualFieldName : mapping.CustomFieldId.ToString(), mapping.TargetDataType })
                                                .ToDictionary(mapping => mapping.ActualFieldName, mapping => mapping.TargetDataType);

                foreach (KeyValuePair<string, string> entry in _mappingTactic.Where(mt => Enums.ActualFieldDatatype.Keys.Contains(mt.Key)))
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
                    client.Create(ObjCode.ISSUE, new { name = issueName, projectID = projectID });
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
            return Fields.ReturnAllWorkFrontFields_AsAPI();
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

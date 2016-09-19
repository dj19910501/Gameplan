using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using Integration.Helper;
using RevenuePlanner.Models;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;


namespace Integration.Marketo
{
    class IntegrationMarketoClient
    {
        private MRPEntities db = new MRPEntities();
        StoredProcedure objSp = new StoredProcedure();
        private int _integrationInstanceSectionId { get; set; }
        private int _integrationTypeId { get; set; }
        private int _integrationInstanceLogId { get; set; }
        private int _integrationInstanceId { get; set; }
        private bool _isResultError { get; set; }
        private int _clientId { get; set; }
        private EntityType _entityType { get; set; }
        private int _id { get; set; }
        private int _userId { get; set; }
        public string _ErrorMessage { get; set; }
        Guid _applicationId = Guid.Empty;


        private List<SyncError> _lstSyncError = new List<SyncError>();
        private List<string> statusList { get; set; }

        public IntegrationMarketoClient()
        {
        }

        public IntegrationMarketoClient(int integrationInstanceId, int id, EntityType entityType, int userId, int integrationInstanceLogId, Guid applicationId)
        {
            _integrationInstanceId = integrationInstanceId;
            _id = id;
            _entityType = entityType;
            _userId = userId;
            _integrationInstanceLogId = integrationInstanceLogId;
            _applicationId = applicationId;

            //SetIntegrationInstanceDetail();

            //this.AuthenticateforMarketo();
        }

        public bool SyncData(out List<SyncError> lstSyncError)
        {

            lstSyncError = new List<SyncError>();
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "SyncData method start.");
            _integrationInstanceSectionId = Common.CreateIntegrationInstanceSection(_integrationInstanceLogId, _integrationInstanceId, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), DateTime.Now, _userId);
            _isResultError = false;
            /// Set client Id based on integration instance.            
            var IntegrationInstanceslist = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == _integrationInstanceId);
            _clientId = IntegrationInstanceslist.ClientId;
            _integrationTypeId = IntegrationInstanceslist.IntegrationTypeId;

            StringBuilder sbMessage = new StringBuilder();
            //int logRecordSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LogRecordSize"]);
            //int pushRecordBatchSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IntegrationPushRecordBatchSize"]);
            try
            {
                int TitleLengthLimit = 255; // Keep fix.
                string tactic = Enums.EntityType.Tactic.ToString();
                DataTable dtFieldMappings = new DataTable();
                DataSet dsFieldMappings = new DataSet();
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Check DataType Mapping Start.");
                #region "Validate Mappings configured or not"
                List<IntegrationInstanceDataTypeMapping> dataTypeMapping = db.IntegrationInstanceDataTypeMappings.Where(mapping => mapping.IntegrationInstanceId.Equals(_integrationInstanceId)).ToList();
                if (!dataTypeMapping.Any()) // check if there is no field mapping configure then log error to IntegrationInstanceLogDetails table.
                {
                    Enums.EntityType _entityTypeSection = (Enums.EntityType)Enum.Parse(typeof(Enums.EntityType), Convert.ToString(_entityType), true);
                    _ErrorMessage = "You have not configure any single field with Marketo field.";
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, _entityTypeSection, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), _ErrorMessage, Enums.SyncStatus.Error, DateTime.Now));
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "You have not configure any single field with Marketo field.");
                    _isResultError = true;    // return true value that means error exist and do not proceed for the further mapping list.
                }
                #endregion
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Check DataType Mapping End.");

                if (!_isResultError)
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "callling GetFieldMappings to get mapping fields.");
                    dsFieldMappings = objSp.GetFieldMappings(tactic, _clientId, Convert.ToInt32(_integrationTypeId), Convert.ToInt32(_integrationInstanceId));
                    dtFieldMappings = dsFieldMappings.Tables[0];

                    ApiIntegration integrationMarketoClient = new ApiIntegration(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0, _applicationId);
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "convert filled map datatable to list .");
                    List<Integration.fieldMapping> lstFiledMap = dtFieldMappings.AsEnumerable().Select(m => new Integration.fieldMapping
                    {
                        sourceFieldName = m.Field<string>("sourceFieldName"),
                        destinationFieldName = m.Field<string>("destinationFieldName"),
                        marketoFieldType = m.Field<string>("marketoFieldType")
                    }).ToList();

                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "creating a parameter list to call the StoreProcedure from API.");
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
                    objSPParams.parameterValue = TitleLengthLimit;
                    objSPParams.name = "SFDCTitleLengthLimit";
                    lstparams.Add(objSPParams);

                    objSPParams = new SpParameters();
                    objSPParams.parameterValue = _integrationInstanceLogId;
                    objSPParams.name = "integrationInstanceLogId";
                    lstparams.Add(objSPParams);
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "parameter list created to call the StoreProcedure from API.");
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Call Api to push Marketo Data and return error log.");
                    List<Integration.LogDetails> logDetailsList = new List<LogDetails>();
                    try
                    {
                        logDetailsList = integrationMarketoClient.MarketoData_Push("spGetMarketoData", lstFiledMap, _clientId, lstparams);
                    }
                    catch (Exception)
                    {
                        // This catch remains blank to continue log execution process getting from Integration WEB API.
                    }
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "Marketo Log detail list get successfully.");

                    IntegrationInstancePlanEntityLog instanceEntity = new IntegrationInstancePlanEntityLog();

                    IntegrationInstanceLogDetail instanceLogDetail = new IntegrationInstanceLogDetail();

                    Dictionary<int, string> TacticMarketoProgMappingIds = new Dictionary<int, string>();
                    Dictionary<int, DateTime> TacticMarketoLastSyncDates = new Dictionary<int, DateTime>();

                    List<Plan_Campaign_Program_Tactic> tblTactics = new List<Plan_Campaign_Program_Tactic>();
                    List<int> lstSourceIds = new List<int>();
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Marketo Log detail insert into database start.");
                    if (logDetailsList.Count > 0 && logDetailsList != null)
                    {
                        lstSourceIds = logDetailsList.Where(log => log.SourceId.HasValue).Select(log => log.SourceId.Value).ToList();
                        tblTactics = db.Plan_Campaign_Program_Tactic.Where(tac => lstSourceIds.Contains(tac.PlanTacticId)).ToList();

                        #region "Declare Enums & local variables"
                        bool isExist = false;
                        string strEventAPICall, strEventAuthentication, strErrorConnection, strGetProgramData, strInvalidConnection, strNoRecord, strFetchUserInfo, strParameter, strSystemError;
                        string strInsertProgram, strUpdateProgram, strRequiredTagNotExist, strDataMapping, strFieldMapping;
                        //strEventAPICall = strEventAuthentication = strErrorConnection = strGetProgramData = strInvalidConnection = strNoRecord = strFetchUserInfo = strParameter = strSystemError = string.Empty;


                        strEventAuthentication = Enums.MarketoAPIEventNames.Authentication.ToString();

                        //Common message (Single time)
                        strErrorConnection = Enums.MarketoAPIEventNames.ErrorInSettingDestinationConnectin.ToString();
                        strGetProgramData = Enums.MarketoAPIEventNames.GetProgramData.ToString();
                        strInvalidConnection = Enums.MarketoAPIEventNames.InvalidConnection.ToString();
                        strNoRecord = Enums.MarketoAPIEventNames.NoRecord.ToString();
                        strFetchUserInfo = Enums.MarketoAPIEventNames.FetchUserInfo.ToString();
                        strParameter = Enums.MarketoAPIEventNames.Parameter.ToString();
                        strSystemError = Enums.MarketoAPIEventNames.SystemError.ToString();
                        strEventAPICall = Enums.MarketoAPIEventNames.APIcall.ToString();
                        strFieldMapping = Enums.MarketoAPIEventNames.FieldMapping.ToString();

                        //for Each Entity Log
                        strInsertProgram = Enums.MarketoAPIEventNames.InsertProgram.ToString();
                        strUpdateProgram = Enums.MarketoAPIEventNames.UpdateProgram.ToString();
                        strRequiredTagNotExist = Enums.MarketoAPIEventNames.RequiredTagNotExist.ToString();
                        strDataMapping = Enums.MarketoAPIEventNames.DataMapping.ToString();

                        int entId = 0;
                        string tacticTitle, exMessage; 
                        #endregion

                        #region "Authentication Log"
                        //foreach (var logdetail in logDetailsList.Where(log => log.EventName.Equals(strEventAuthentication)))
                        //{
                        //    LogEndErrorDescription = "Authentication Failed :" + integrationSalesforceClient._ErrorMessage;
                        //    Common.SaveIntegrationInstanceLogDetails(_id, integrationinstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, LogEndErrorDescription);

                        //    _isResultError = true;
                        //    _isA = true;
                        //    _lstAllSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, string.Empty, "Authentication Failed :" + integrationSalesforceClient._ErrorMessage, Enums.SyncStatus.Error, DateTime.Now));
                        //}
                        #endregion

                        #region "Check log for integration instance section"
                        isExist = logDetailsList.Where(log => (log.EventName.Equals(strErrorConnection) || log.EventName.Equals(strGetProgramData) || log.EventName.Equals(strInvalidConnection) || log.EventName.Equals(strNoRecord) || log.EventName.Equals(strFetchUserInfo) || log.EventName.Equals(strParameter) || log.EventName.Equals(strSystemError) || log.EventName.Equals(strFieldMapping)) && log.Status.ToUpper().Equals("FAILURE")).Any();
                        if (isExist)
                        {
                            _isResultError = true;
                            _ErrorMessage = "Error in getting data from source, to push to marketo";
                        }
                        #endregion

                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Insert Log detail into IntegrationInstanceLogDetails start.");
                        #region "Original Log for Push Marketo"
                        //Insert Failure error log to IntegrationInstanceLogDetails table 
                        foreach (var logdetail in logDetailsList.Where(log => (log.EventName.Equals(strErrorConnection) || log.EventName.Equals(strGetProgramData) || log.EventName.Equals(strInvalidConnection) || log.EventName.Equals(strNoRecord) || log.EventName.Equals(strFetchUserInfo) || log.EventName.Equals(strParameter) || log.EventName.Equals(strSystemError) || log.EventName.Equals(strFieldMapping)) && log.Status.ToUpper().Equals("FAILURE")))
                        {
                            //if (logdetail.Status.ToUpper().Equals("FAILURE"))
                            //{

                            instanceLogDetail = new IntegrationInstanceLogDetail();
                            entId = Convert.ToInt32(logdetail.SourceId);
                            instanceLogDetail.EntityId = entId;
                            instanceLogDetail.IntegrationInstanceLogId = _integrationInstanceLogId;
                            instanceLogDetail.LogTime = logdetail.EndTimeStamp;
                            instanceLogDetail.LogDescription = logdetail.Description;
                            db.Entry(instanceLogDetail).State = EntityState.Added;
                            //}
                        }
                        #endregion
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Insert Log detail into IntegrationInstanceLogDetails end.");
                        #region "Entity Logs for each tactic"
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Insert Log detail into IntegrationInstancePlanEntityLog start.");
                        List<Integration.LogDetails> logDetailsList1 = logDetailsList.Where(log => (log.EventName.Equals(strInsertProgram) || log.EventName.Equals(strUpdateProgram)) || (log.EventName.Equals(strEventAPICall) && log.Status.ToUpper().Equals("FAILURE")) || (log.EventName.Equals(strDataMapping) && log.Status.ToUpper().Equals("FAILURE")) || (log.EventName.Equals(strRequiredTagNotExist) && log.Status.ToUpper().Equals("FAILURE"))).ToList();
                        int pushRecordBatchSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IntegrationPushRecordBatchSize"]);
                        int pushCntr = 0;
                        try
                        {
                            foreach (var logdetail in logDetailsList1)
                            {
                                
                                db.Configuration.AutoDetectChangesEnabled = false;
                                //Insert  log into IntegrationInstancePlanEntityLog table 
                                entId = Convert.ToInt32(logdetail.SourceId);
                                instanceEntity = new IntegrationInstancePlanEntityLog();
                                instanceEntity.IntegrationInstanceId = _integrationInstanceId;
                                instanceEntity.EntityId = entId;
                                instanceEntity.EntityType = Enums.EntityType.Tactic.ToString();
                                instanceEntity.SyncTimeStamp = logdetail.EndTimeStamp;
                                instanceEntity.Operation = logdetail.Mode.ToString();
                                if (logdetail.Status.ToUpper().Equals("FAILURE"))
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
                                    if (logdetail.Status.ToUpper().Equals("FAILURE"))
                                    {
                                        //Add Failure Log for Summary Email.
                                        _isResultError = true;
                                        tacticTitle = tblTactics.Where(tac => tac.PlanTacticId == entId).Select(tac => tac.Title).FirstOrDefault();
                                        exMessage = "System error occurred while create/update tactic \"" + tacticTitle + "\": " + logdetail.Description;
                                        _lstSyncError.Add(Common.PrepareSyncErrorList(entId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                                    }
                                    else
                                    {
                                        //Add Success Log for Summary Email.
                                        exMessage = logdetail.Mode.ToString();
                                        _lstSyncError.Add(Common.PrepareSyncErrorList(entId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Success, DateTime.Now));
                                    }
                                }
                                if (((pushCntr + 1) % pushRecordBatchSize) == 0)
                                    db.SaveChanges();
                                pushCntr++;
                            }
                        }
                        finally
                        {
                            db.Configuration.AutoDetectChangesEnabled = true;
                        }
                        #endregion
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Insert Log detail into IntegrationInstancePlanEntityLog end.");
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Update Tactic's Marketo ProgramId Mappig list start.");
                        #region "Update Tactic's Marketo ProgramId Mappig list"
                        string strCreateMode = Operation.Create.ToString();
                        List<int> lstCreatedTacIds = new List<int>();

                        TacticMarketoProgMappingIds = logDetailsList.Where(log => (log.EventName.Equals(strInsertProgram) || log.EventName.Equals(strUpdateProgram)) && log.SourceId.HasValue && log.EntityId.HasValue).ToDictionary(log => log.SourceId.Value, log => log.EntityId.Value.ToString());
                        var marketoURL = logDetailsList.Where(log => (log.EventName.Equals(strInsertProgram) || log.EventName.Equals(strUpdateProgram)) && log.SourceId.HasValue && log.EntityId.HasValue).ToDictionary(log => log.SourceId.Value, log => log.Url);
                        TacticMarketoLastSyncDates = logDetailsList.Where(log => (log.EventName.Equals(strInsertProgram) || log.EventName.Equals(strUpdateProgram)) && log.SourceId.HasValue && log.EntityId.HasValue).ToDictionary(log => log.SourceId.Value, log => log.EndTimeStamp);
                        if (TacticMarketoProgMappingIds != null && TacticMarketoProgMappingIds.Count > 0)
                        {
                            string  AttrMarketoUrl= Enums.EntityIntegrationAttribute.MarketoUrl.ToString();
                            string  EntityType= Enums.EntityType.Tactic.ToString();
                            List<EntityIntegration_Attribute> ListEntityIntegrationAttr = new List<EntityIntegration_Attribute>();
                            EntityIntegration_Attribute objEntityIntegrationAttr = new EntityIntegration_Attribute();
                            if (marketoURL.Count > 0)
                            {
                                ListEntityIntegrationAttr = db.EntityIntegration_Attribute.Where(attr => attr.IntegrationinstanceId == _integrationInstanceId
                                    && attr.AttrType==AttrMarketoUrl && attr.EntityType==EntityType).ToList();
                            }
                            string strMarketoProgId, strMarketoURL;
                            DateTime strMarketoLastSync;
                            lstCreatedTacIds = TacticMarketoProgMappingIds.Select(tac => tac.Key).ToList();
                            List<Plan_Campaign_Program_Tactic> lstCreatedTacs = tblTactics.Where(tac => lstCreatedTacIds.Contains(tac.PlanTacticId)).ToList();
                            pushCntr = 0;
                            try
                            {
                                foreach (Plan_Campaign_Program_Tactic tac in lstCreatedTacs)
                                {
                                    db.Configuration.AutoDetectChangesEnabled = false;
                                    strMarketoProgId = strMarketoURL = string.Empty;
                                    strMarketoProgId = TacticMarketoProgMappingIds.Where(prg => prg.Key == tac.PlanTacticId).Select(prg => prg.Value).FirstOrDefault();
                                    strMarketoLastSync = TacticMarketoLastSyncDates.Where(prg => prg.Key == tac.PlanTacticId).Select(prg => prg.Value).FirstOrDefault();
                                    if (strMarketoProgId != null)
                                    {
                                        tac.IntegrationInstanceMarketoID = strMarketoProgId;
                                        tac.LastSyncDate = strMarketoLastSync; // Modified By Rahul shah // To add last sync date
                                        db.Entry(tac).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        _isResultError = true;
                                        _ErrorMessage = "Error updating Marketo Program id for Tactic - " + tac.Title;
                                    }
                                    // Add By Nishant Sheth
                                    // Description :: #2134 Add Marketo Pushed Program url.
                                    strMarketoURL = marketoURL.Where(prg => prg.Key == tac.PlanTacticId).Select(prg => prg.Value).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(strMarketoURL))
                                    {

                                        objEntityIntegrationAttr = ListEntityIntegrationAttr.Where(attr => attr.EntityId == tac.PlanTacticId).FirstOrDefault();
                                        if (objEntityIntegrationAttr == null)
                                        {
                                            objEntityIntegrationAttr = new EntityIntegration_Attribute();
                                            objEntityIntegrationAttr.EntityId = tac.PlanTacticId;
                                            objEntityIntegrationAttr.EntityType = Enums.EntityType.Tactic.ToString();
                                            objEntityIntegrationAttr.AttrType = Enums.EntityIntegrationAttribute.MarketoUrl.ToString();
                                            objEntityIntegrationAttr.AttrValue = strMarketoURL;
                                            objEntityIntegrationAttr.IntegrationinstanceId = _integrationInstanceId;
                                            objEntityIntegrationAttr.CreatedDate = DateTime.Now;
                                            db.Entry(objEntityIntegrationAttr).State = EntityState.Added;
                                        }
                                        else
                                        {
                                            objEntityIntegrationAttr.AttrValue = strMarketoURL;
                                            db.Entry(objEntityIntegrationAttr).State = EntityState.Modified;
                                        }
                                    }
                                    if (((pushCntr + 1) % pushRecordBatchSize) == 0)
                                        db.SaveChanges();
                                    pushCntr++;
                                    // End By Nishant Sheth
                                }
                            }
                            finally
                            {
                                db.Configuration.AutoDetectChangesEnabled = true;
                            }
                        }
                        #endregion

                        db.SaveChanges();
                        #region "Create Tactic-Linked Tactic Mapping"
                        Dictionary<int, int> lstTacLinkIdMappings = new Dictionary<int, int>();
                        lstTacLinkIdMappings = tblTactics.Where(tac => lstSourceIds.Contains(tac.PlanTacticId) && tac.LinkedTacticId.HasValue).ToDictionary(tac => tac.PlanTacticId, tac => tac.LinkedTacticId.Value);
                        #endregion

                        UpdateLinkedTacticComment(lstSourceIds, tblTactics, lstCreatedTacIds, lstTacLinkIdMappings);

                    }

                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SyncData method end.");

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
                Common.UpdateIntegrationInstanceSection(_integrationInstanceSectionId, StatusResult.Error, _ErrorMessage);
            }
            else
            {
                Common.UpdateIntegrationInstanceSection(_integrationInstanceSectionId, StatusResult.Success, string.Empty);
            }


            lstSyncError.AddRange(_lstSyncError);
            return _isResultError;
        }

        public void UpdateLinkedTacticComment(List<int> lstProcessTacIds, List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstCreatedTacIds, Dictionary<int, int> lstTac_LinkTacMapping)
        {
            string query = string.Empty;
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {

                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "UpdateLinkedTacticComment method start.");
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


                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "UpdateTacticInstanceTacticId_Comment execution start.");
                    MRPEntities mp = new MRPEntities();
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = mp.Database.Connection.ConnectionString;
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UpdateTacticInstanceTacticId_Comment", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@strCreatedTacIds", strCreatedTacIds);
                    cmd.Parameters.AddWithValue("@strUpdatedTacIds", strUpdatedTacIds);
                    cmd.Parameters.AddWithValue("@strUpdateComment", Common.TacticUpdatedComment + Integration.Helper.Enums.IntegrationType.Marketo.ToString());
                    cmd.Parameters.AddWithValue("@strCreateComment", Common.TacticSyncedComment + Integration.Helper.Enums.IntegrationType.Marketo.ToString());
                    cmd.Parameters.AddWithValue("@isAutoSync", Common.IsAutoSync);
                    cmd.Parameters.AddWithValue("@userId", _userId);
                    cmd.Parameters.AddWithValue("@integrationType", Enums.IntegrationType.Marketo.ToString());
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    mp.Dispose();
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "UpdateTacticInstanceTacticId_Comment execution end.");
                }

                #endregion

            }
            catch (Exception)
            {
                throw;
            }

            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "UpdateLinkedTacticComment method end.");
        }



    }
}

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
        private Guid _clientId { get; set; }
        private EntityType _entityType { get; set; }
        private int _id { get; set; }
        private Guid _userId { get; set; }
        public string _ErrorMessage { get; set; }
        Guid _applicationId = Guid.Empty;


        private List<SyncError> _lstSyncError = new List<SyncError>();
        private List<string> statusList { get; set; }

        public IntegrationMarketoClient()
        {
        }

        public IntegrationMarketoClient(int integrationInstanceId, int id, EntityType entityType, Guid userId, int integrationInstanceLogId, Guid applicationId)
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
            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "SyncData method start.");
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
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Validate Mappnigs configured or not.");
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


                if (!_isResultError)
                {
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "callling GetFieldMappings to get mapping fields.");
                    dsFieldMappings = objSp.GetFieldMappings(tactic, _clientId, Convert.ToInt32(_integrationTypeId), Convert.ToInt32(_integrationInstanceId));
                    dtFieldMappings = dsFieldMappings.Tables[0];

                    ApiIntegration integrationMarketoClient = new ApiIntegration(Convert.ToInt32(_integrationInstanceId), _id, _entityType, _userId, 0, _applicationId);
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "convert filled map datatable to list .");
                    List<Integration.fieldMapping> lstFiledMap = dtFieldMappings.AsEnumerable().Select(m => new Integration.fieldMapping
                    {
                        sourceFieldName = m.Field<string>("sourceFieldName"),
                        destinationFieldName = m.Field<string>("destinationFieldName"),
                        marketoFieldType = m.Field<string>("marketoFieldType")
                    }).ToList();

                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "creating a parameter list to call the StoreProcedure from API.");
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
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Call Api to push Marketo Data and return error log.");

                    List<Integration.LogDetails> logDetailsList = integrationMarketoClient.MarketoData_Push("spGetMarketoData", lstFiledMap, _clientId, lstparams);
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Marketo Log detail list get successfully.");
                    List<IntegrationInstancePlanEntityLog> instanceEntityList = new List<IntegrationInstancePlanEntityLog>();
                    IntegrationInstancePlanEntityLog instanceEntity = new IntegrationInstancePlanEntityLog();
                    Dictionary<int, string> TacticMarketoProgMappingIds = new Dictionary<int, string>();
                    List<Plan_Campaign_Program_Tactic> tblTactics = new List<Plan_Campaign_Program_Tactic>();
                    List<int> lstSourceIds = new List<int>();
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Marketo Log detail insert into database start.");
                    if (logDetailsList.Count > 0 && logDetailsList != null)
                    {
                        lstSourceIds = logDetailsList.Where(log => log.SourceId.HasValue).Select(log => log.SourceId.Value).ToList();
                        tblTactics = db.Plan_Campaign_Program_Tactic.Where(tac => lstSourceIds.Contains(tac.PlanTacticId)).ToList();
                        string strEventAPICall = Enums.MarketoAPIEventNames.APIcall.ToString();
                        string strEventAuthentication = Enums.MarketoAPIEventNames.Authentication.ToString();
                        int entId = 0;
                        string tacticTitle, exMessage;

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


                        #region "Entity Logs for each tactic"
                        foreach (var logdetail in logDetailsList.Where(log => log.EventName.Equals(strEventAPICall)))
                        {
                            entId = Convert.ToInt32(logdetail.SourceId);
                            instanceEntity = new IntegrationInstancePlanEntityLog();
                            instanceEntity.IntegrationInstanceId = _integrationInstanceId;
                            instanceEntity.EntityId = entId;
                            instanceEntity.EntityType = Enums.EntityType.Tactic.ToString();
                            instanceEntity.SyncTimeStamp = logdetail.EndTimeStamp;
                            instanceEntity.Operation = logdetail.Mode.ToString();
                            instanceEntity.Status = logdetail.Status.ToString();
                            instanceEntity.ErrorDescription = logdetail.Description;
                            instanceEntity.CreatedBy = _userId;
                            instanceEntity.CreatedDate = DateTime.Now;
                            instanceEntity.IntegrationInstanceSectionId = _integrationInstanceSectionId;
                            instanceEntityList.Add(instanceEntity);
                            db.Entry(instanceEntityList).State = EntityState.Added;

                            if (logdetail.Status.ToUpper().Equals("FAILURE"))
                            {
                                _isResultError = true;
                                tacticTitle = tblTactics.Where(tac => tac.PlanTacticId == entId).Select(tac => tac.Title).FirstOrDefault();
                                exMessage = "System error occurred while create/update tactic \"" + tacticTitle + "\": " + logdetail.Description;
                                _lstSyncError.Add(Common.PrepareSyncErrorList(entId, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PushTacticData.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                            }

                        } 
                        #endregion

                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Update Tactic's Marketo ProgramId Mappig list start.");
                        #region "Update Tactic's Marketo ProgramId Mappig list"
                        string strCreateMode = Operation.Create.ToString();
                        List<int> lstCreatedTacIds = new List<int>();

                        TacticMarketoProgMappingIds = logDetailsList.Where(log => log.EventName.Equals(strEventAPICall) && log.Mode.Equals(strCreateMode) && log.SourceId.HasValue).ToDictionary(log => log.SourceId.Value, log => log.ProgramId.Value.ToString());
                        if (TacticMarketoProgMappingIds != null && TacticMarketoProgMappingIds.Count > 0)
                        {
                            string strMarketoProgId;
                            lstCreatedTacIds = TacticMarketoProgMappingIds.Select(tac => tac.Key).ToList();
                            List<Plan_Campaign_Program_Tactic> lstCreatedTacs = tblTactics.Where(tac => lstCreatedTacIds.Contains(tac.PlanTacticId)).ToList();
                            foreach (Plan_Campaign_Program_Tactic tac in lstCreatedTacs)
                            {
                                strMarketoProgId = string.Empty;
                                strMarketoProgId = TacticMarketoProgMappingIds.Where(prg => prg.Key == tac.PlanTacticId).Select(prg => prg.Value).FirstOrDefault();
                                tac.IntegrationInstanceMarketoID = strMarketoProgId;
                                db.Entry(tac).State = EntityState.Modified;
                            }
                        }
                        #endregion

                        db.SaveChanges();
                        Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Update Tactic's Marketo ProgramId Mappig list start.");
                        #region "Create Tactic-Linked Tactic Mapping"
                        Dictionary<int, int> lstTacLinkIdMappings = new Dictionary<int, int>();
                        lstTacLinkIdMappings = tblTactics.Where(tac => lstSourceIds.Contains(tac.PlanTacticId) && tac.LinkedTacticId.HasValue).ToDictionary(tac => tac.PlanTacticId, tac => tac.LinkedTacticId.Value);
                        #endregion
                        
                        UpdateLinkedTacticComment(lstSourceIds, tblTactics, lstCreatedTacIds, lstTacLinkIdMappings);
                        
                    }

                }
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "SyncData method end.");
                //IntegrationInstanceslist.LastSyncStatus = "Error";
                //db.Entry(IntegrationInstanceslist).State = EntityState.Modified;
                //db.SaveChanges();
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



        public void UpdateLinkedTacticComment(List<int> lstProcessTacIds, List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstCreatedTacIds, Dictionary<int,int> lstTac_LinkTacMapping)
        {
            string query = string.Empty;
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {
                
                Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "UpdateLinkedTacticComment method start.");
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


                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "UpdateTacticInstanceTacticId_Comment execution start.");
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
                    Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "UpdateTacticInstanceTacticId_Comment execution end.");
                }

                #endregion

            }
            catch (Exception)
            {
                throw;
            }

            Common.SaveIntegrationInstanceLogDetails(_id, _integrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "UpdateLinkedTacticComment method end.");
        }

       

    }
}

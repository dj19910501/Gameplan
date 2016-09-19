//// File Used to PUT/GET response(s).

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Tamir.SharpSsh;
using Tamir.SharpSsh.jsch;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Data;
using RevenuePlanner.Models;
using Integration.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Globalization;

#endregion

namespace Integration.Eloqua
{
    public class EloquaResponse
    {
        #region Variables

        private static string archiveFolder = "/Archived/";
        private static string rootResponseFolder = ConfigurationManager.AppSettings["EloquaResponseFolderPath"].ToString();
        private static string eloquaCampaignIDColumn = "EloquaCampaignID";
        private static string eloquaResponseDateTimeColumn = "ResponseDateTime";
        private static string responsedateformat = "yyyy-MM-dd";
        private MRPEntities db = new MRPEntities();
        private string PeriodChar = "Y";
        #endregion

        #region Functions

        /// <summary>
        /// Added By Dharmraj, 5-8-2014
        /// </summary>
        /// <param name="_ftpURL"></param>
        /// <param name="_UserName"></param>
        /// <param name="_Password"></param>
        /// <param name="_Port"></param>
        /// <returns>True if SFTP connects successfully otherwise False</returns>
        public bool AuthenticateSFTP(string _ftpURL, string _UserName, string _Password, int _Port)
        {
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {
                Common.SaveIntegrationInstanceLogDetails(0, null, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "SFTP Authentication");
                //connect SFTP
                Sftp client = new Sftp(_ftpURL, _UserName, _Password);
                client.Connect(_Port);
                Common.SaveIntegrationInstanceLogDetails(0, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "SFTP Authentication");
                return client.Connected;
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(0, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "SFTP Authentication Error: " + exMessage);
                return false;
            }
        }

        /// <summary>
        /// Function to manipulate tactic actual data.
        /// </summary>
        /// <param name="IntegrationInstanceId">Integration instance id.</param>
        /// <param name="_userId">Logged in User Id</param>
        /// <param name="IntegrationInstanceLogId">Integration Instance Log Id.</param>
        /// <param name="_applicationId">Application Id.</param>
        /// <param name="_entityType">Entity Type.</param>
        public bool SetTacticMQLs(int IntegrationInstanceId, int _userId, int IntegrationInstanceLogId, Guid _applicationId, EntityType _entityType, out List<SyncError> _lstSyncError)
        {
            _lstSyncError = new List<SyncError>();
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            //// Insert log into IntegrationInstanceSection
            int IntegrationInstanceSectionId = Common.CreateIntegrationInstanceSection(IntegrationInstanceLogId, IntegrationInstanceId, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), DateTime.Now, _userId);
            Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Set Tactic MQLs");
            try
            {
                #region "Check for pulling MQLs whether Model/Plan associated or not with current Instance"
                //// PlanIDs which has configured for "Pull MQL" from Eloqua instances
                List<Model> lstModels = db.Models.Where(objmdl => objmdl.IntegrationInstanceIdMQL == IntegrationInstanceId && objmdl.Status.Equals("Published") && objmdl.IsActive == true).ToList();
                if (lstModels == null || lstModels.Count <= 0)
                {
                    // Save & display Message: No single Model associated with current Instance.
                    Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull MQL: There is no single Model associated with this Instance to pull MQLs.");
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull MQL: There is no single Model associated with this Instance to pull MQLs.", Enums.SyncStatus.Info, DateTime.Now));
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    return false; // no error.
                }
                List<int> ModelIds = lstModels.Select(mdl => mdl.ModelId).ToList();
                List<Plan> lstPlans = db.Plans.Where(objplan => ModelIds.Contains(objplan.Model.ModelId) && objplan.IsActive == true).ToList();
                if (lstPlans == null || lstPlans.Count <= 0)
                {
                    // Save & display Message: No single Plan associated with current Instance.
                    Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull MQL: There is no single Plan associated with this Instance to pull MQLs.");
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull MQL: There is no single Plan associated with this Instance to pull MQLs.", Enums.SyncStatus.Info, DateTime.Now));
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    return false; // no error.
                } 
                #endregion
                
                int _ClientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == IntegrationInstanceId).ClientId;

                #region "Old Code"
                //// Get SalesForce integration type Id.
                //string salesforceCode = Enums.IntegrationType.Salesforce.ToString();
                //var salesforceIntegrationType = db.IntegrationTypes.Where(type => type.Code == salesforceCode && type.IsDeleted == false).Select(type => type.IntegrationTypeId).FirstOrDefault();
                //int SalesForceintegrationTypeId = Convert.ToInt32(salesforceIntegrationType);

                //List<int> salesforceIntegrationType = db.IntegrationTypes.Where(type => type.Code == salesforceCode && type.IsDeleted == false).Select(type => type.IntegrationTypeId).ToList();

                //// Get All SalesForceIntegrationTypeIds to retrieve  SalesForcePlanIds.
                //List<int> lstSalesForceIntegrationTypeIds = db.IntegrationInstances.Where(instance => instance.IntegrationTypeId.Equals(SalesForceintegrationTypeId) && instance.IsDeleted.Equals(false) && instance.ClientId.Equals(_ClientId)).Select(s => s.IntegrationInstanceId).ToList();
                //List<int> lstSalesForceIntegrationTypeIds = db.IntegrationInstances.Where(instance => salesforceIntegrationType.Contains(instance.IntegrationTypeId) && instance.IsDeleted.Equals(false) && instance.ClientId.Equals(_ClientId)).Select(s => s.IntegrationInstanceId).ToList();

                //// Get all PlanIds whose Tactic data PUSH on SalesForce.
                // List<int> lstSalesForcePlanIds = lstPlans.Where(objplan => lstSalesForceIntegrationTypeIds.Contains(objplan.Model.IntegrationInstanceId.Value)).Select(objplan => objplan.PlanId).ToList();
                
                #endregion
                
                //// Get All PlanIds.
                List<int> AllplanIds = lstPlans.Select(objplan => objplan.PlanId).ToList();

                Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull MQL: Total Model(s) - " + lstModels.Count + ", Total Plan(s) - " + AllplanIds.Count + " associated with this Instance.");
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull MQL: Total Model(s) - " + lstModels.Count + ", Total Plan(s) - " + AllplanIds.Count + " associated with this Instance.", Enums.SyncStatus.Info, DateTime.Now));

                //// Get Eloqua PlanIds.
                //List<int> lstEloquaplanIds = lstPlans.Where(objplan => !lstSalesForcePlanIds.Contains(objplan.PlanId)).Select(plan => plan.PlanId).ToList();
                try
                {
                    ////local variables declaration
                    string CampaignIdValue = string.Empty, MQLDateValue = string.Empty, ViewIdValue = string.Empty, ListIdValue = string.Empty;
                    int CampaignId = 0, MQLDateId = 0, ViewId = 0, ListId = 0;
                    string CampaignIdDisplpayFieldName = string.Empty, MQlDateDisplpayFieldName = string.Empty;

                    //// Get Eloqua integration type Id.
                    string eloquaCode = Enums.IntegrationType.Eloqua.ToString();
                    var eloquaIntegrationTypeId = db.IntegrationTypes.Where(type => type.Code == eloquaCode && type.IsDeleted == false).Select(type => type.IntegrationTypeId).FirstOrDefault();
                    int integrationTypeId = Convert.ToInt32(eloquaIntegrationTypeId);

                    //// Get pull data type from table for specific integration id and MQL
                    var listPullDataType = db.GameplanDataTypePulls.Where(objGameplanDataTypepull => objGameplanDataTypepull.IntegrationTypeId == integrationTypeId && objGameplanDataTypepull.Type == Common.StageMQL).ToList();

                    //// Start - Added by Sohel Pathan on 02/01/2015 for PL ticket #1068
                    if (listPullDataType.Count == 0)
                    {
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Data types for pull responses is not defined in DB.", Enums.SyncStatus.Error, DateTime.Now));
                    }
                    else
                    {
                        CampaignIdDisplpayFieldName = listPullDataType.FirstOrDefault(pullDataType => pullDataType.ActualFieldName == Enums.CustomeFieldNameMQL.CampaignId.ToString()).DisplayFieldName;
                        MQlDateDisplpayFieldName = listPullDataType.FirstOrDefault(pullDataType => pullDataType.ActualFieldName == Enums.CustomeFieldNameMQL.MQLDate.ToString()).DisplayFieldName;
                    }
                    //// End - Added by Sohel Pathan on 02/01/2015 for PL ticket #1068

                    //// Get data type pull id into local variables for MQL data type
                    CampaignId = listPullDataType.SingleOrDefault(pullDataType => pullDataType.ActualFieldName == Enums.CustomeFieldNameMQL.CampaignId.ToString()).GameplanDataTypePullId;
                    MQLDateId = listPullDataType.SingleOrDefault(pullDataType => pullDataType.ActualFieldName == Enums.CustomeFieldNameMQL.MQLDate.ToString()).GameplanDataTypePullId;
                    ViewId = listPullDataType.SingleOrDefault(pullDataType => pullDataType.ActualFieldName == Enums.CustomeFieldNameMQL.ViewId.ToString()).GameplanDataTypePullId;
                    ListId = listPullDataType.SingleOrDefault(pullDataType => pullDataType.ActualFieldName == Enums.CustomeFieldNameMQL.ListId.ToString()).GameplanDataTypePullId;

                    //// Get pull mapping data from database for integration id.
                    var listPullMapping = db.IntegrationInstanceDataTypeMappingPulls.Where(pullMapping => pullMapping.IntegrationInstanceId == IntegrationInstanceId).ToList();

                    //// Get data type mapping pull target data type into local variables for integration instance id.
                    CampaignIdValue = listPullMapping.Where(pullMapping => pullMapping.GameplanDataTypePullId == CampaignId).Select(pullMapping => pullMapping.TargetDataType).SingleOrDefault();
                    MQLDateValue = listPullMapping.Where(pullMapping => pullMapping.GameplanDataTypePullId == MQLDateId).Select(pullMapping => pullMapping.TargetDataType).SingleOrDefault();
                    ViewIdValue = listPullMapping.Where(pullMapping => pullMapping.GameplanDataTypePullId == ViewId).Select(pullMapping => pullMapping.TargetDataType).SingleOrDefault();
                    ListIdValue = listPullMapping.Where(pullMapping => pullMapping.GameplanDataTypePullId == ListId).Select(pullMapping => pullMapping.TargetDataType).SingleOrDefault();

                    //// Start - Added by Sohel Pathan on 02/01/2015 for PL ticket #1068
                    if (listPullMapping.Count > 0)
                    {
                        if (string.IsNullOrEmpty(CampaignIdValue))
                        {
                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Data type mapping for : " + CampaignIdDisplpayFieldName + " not found.", Enums.SyncStatus.Error, DateTime.Now));
                        }
                        if (string.IsNullOrEmpty(MQLDateValue))
                        {
                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Data type mapping for : " + MQlDateDisplpayFieldName + " not found.", Enums.SyncStatus.Error, DateTime.Now));
                        }
                        if (string.IsNullOrEmpty(ViewIdValue))
                        {
                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Data type mapping for : " + listPullDataType.Where(dataType => dataType.ActualFieldName == Enums.CustomeFieldNameMQL.ViewId.ToString()).Select(dataType => dataType.DisplayFieldName).FirstOrDefault() + " not found.", Enums.SyncStatus.Error, DateTime.Now));
                        }
                        if (string.IsNullOrEmpty(ListIdValue))
                        {
                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Data type mapping for : " + listPullDataType.Where(dataType => dataType.ActualFieldName == Enums.CustomeFieldNameMQL.ListId.ToString()).Select(dataType => dataType.DisplayFieldName).FirstOrDefault() + " not found.", Enums.SyncStatus.Error, DateTime.Now));
                        }
                    }
                    //// End - Added by Sohel Pathan on 02/01/2015 for PL ticket #1068

                    if (CampaignIdValue != null && MQLDateValue != null && ViewIdValue != null && ListIdValue != null)
                    {
                        #region Get Eloqua respopnse

                            //// Initialize eloqua integration instance
                            IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient(Convert.ToInt32(IntegrationInstanceId), 0, EntityType.IntegrationInstance, _userId, IntegrationInstanceLogId, _applicationId);

                            //// define models for data manipulation from Eloqua response
                            ContactListDetailModel contactListDetails = new ContactListDetailModel();
                            List<elements> element = new List<elements>();
                            bool isError = false;
                            string errormsg = string.Empty;
                            int TotalContactCount = 0, ProcessedContactCount = 0;
                            //// allowed to enter only authenticated user.
                            if (integrationEloquaClient.IsAuthenticated)
                            {
                                //// Get list detail from eloqua for particular list id.
                                var lstcontactDetails = integrationEloquaClient.GetEloquaContactListDetails(ListIdValue.ToString());

                                //// Deserialize Object and store response into ContactListDetail Model
                                contactListDetails = JsonConvert.DeserializeObject<ContactListDetailModel>(lstcontactDetails.Content);
                                int page = 1;
                                while (page > 0)
                                {
                                    //// Get contact list for particular ViewId and List Id.
                                    var lstcontacts = integrationEloquaClient.GetEloquaContactList(ListIdValue.ToString(), ViewIdValue.ToString(), page);

                                    //// Manipulation of contact list response and store into model
                                    string TacticResult = lstcontacts.Content.ToString();
                                    if (!string.IsNullOrEmpty(TacticResult))
                                    {
                                        JObject joResponse = JObject.Parse(TacticResult);
                                        JArray elementsArray = (JArray)joResponse["elements"];

                                        //bool isAllCampaignIdExists = true;
                                        //bool isAllMQLDateExists = true;
                                        // Log: Add count of contact(s) find from eloqua based on ViewID & ListID.
                                        Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull MQL: Total contact(s) - " + AllplanIds.Count + " retrieved from Eloqua based on ViewID - " + ViewIdValue.ToString() + " and ListID - " + ListIdValue.ToString() + ".");
                                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull MQL: Total contact(s) - " + AllplanIds.Count + " retrieved from Eloqua based on ViewID - " + ViewIdValue.ToString() + " and ListID - " + ListIdValue.ToString() + ".", Enums.SyncStatus.Info, DateTime.Now));

                                        for (int i = 0; i < elementsArray.Count(); i++)
                                        {
                                            elements elementsInner = new elements();
                                            if (elementsArray[i][CampaignIdValue] != null && elementsArray[i][MQLDateValue] != null)
                                            {
                                                if (!string.IsNullOrEmpty(elementsArray[i][CampaignIdValue].ToString()) && !string.IsNullOrEmpty(elementsArray[i][MQLDateValue].ToString()))
                                                {
                                                    elementsInner.CampaignId = elementsArray[i][CampaignIdValue].ToString();
                                                    elementsInner.peroid = integrationEloquaClient.ConvertTimestampToDateTime(elementsArray[i][MQLDateValue].ToString());
                                                    elementsInner.peroid = new DateTime(elementsInner.peroid.Year, elementsInner.peroid.Month, 1);
                                                    elementsInner.contactId = elementsArray[i]["contactId"].ToString();
                                                    elementsInner.type = elementsArray[i]["type"].ToString();
                                                    element.Add(elementsInner);
                                                }
                                            }
                                        }

                                        if (elementsArray.Count == 0)
                                        {
                                            if (page == 1)
                                            {
                                                errormsg = "No contact data found in Eloqua contact object.";
                                                Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, errormsg);
                                                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), errormsg, Enums.SyncStatus.Error, DateTime.Now));
                                                isError = true;
                                            }
                                            page = 0;
                                        }
                                        else
                                        {
                                            page++;
                                        }
                                        //if (elementsArray.Count > 0 && listPullMapping.Count > 0)
                                        //{
                                        //    if (!isAllCampaignIdExists)
                                        //    {
                                        //        errormsg += CampaignIdValue + " for one or many record(s) does not exists.";
                                        //        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), CampaignIdValue + " for one or many record(s) does not exists.", Enums.SyncStatus.Error, DateTime.Now));
                                        //        isError = true;
                                        //    }
                                        //    if (!isAllMQLDateExists)
                                        //    {
                                        //        errormsg += MQLDateValue + " for one or many record(s) does not exists.";
                                        //        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), MQLDateValue + " for one or many record(s) does not exists.", Enums.SyncStatus.Error, DateTime.Now));
                                        //        isError = true;
                                        //    }
                                        //}

                                    }
                                    else
                                    {
                                        if (page == 1)
                                        {
                                            errormsg = "No contact data found in Eloqua contact object.";
                                            Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, errormsg);
                                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), errormsg, Enums.SyncStatus.Error, DateTime.Now));
                                            isError = true;
                                        }
                                        page = 0;
                                    }
                                }
                            }
                            else
                            {
                                Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Authorization for " + Enums.IntegrationType.Eloqua.ToString() + " has been failed");
                                errormsg = "Authorization for " + Enums.IntegrationType.Eloqua.ToString() + " has been failed.";
                                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), errormsg, Enums.SyncStatus.Error, DateTime.Now));
                                isError = true;
                            }
                            if (isError)
                            {
                                // Update IntegrationInstanceSection log with Error status
                                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, errormsg);
                                return true;
                            }

                            //// get distinct campaign id for filter.
                            var campaignIds = element.Select(objelement => objelement.CampaignId).Distinct().ToList();

                            #endregion

                        #region Get Tactic List

                            //// Get tactic status list
                            List<string> lstApproveStatus = Common.GetStatusListAfterApproved();

                            List<Plan_Campaign_Program_Tactic> tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeployedToIntegration == true &&
                                                                                                                           lstApproveStatus.Contains(tactic.Status) &&
                                                                                                                           tactic.IsDeleted == false).ToList();

                            //// Get All Approved,IsDeployedToIntegration true and IsDeleted false Tactic list.
                            List<Plan_Campaign_Program_Tactic> lstAllTactics = tblTactic.Where(tactic => AllplanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).ToList();
                            // Log: Total No. of Tactics find filter by PlanIds.
                            //// Get MQL Level
                            var MQLLevel = db.Stages.Where(ObjStage => ObjStage.Code == Common.StageMQL && ObjStage.ClientId == _ClientId && ObjStage.IsDeleted == false).Select(ObjStage => ObjStage.Level).FirstOrDefault();

                            //// Get list of SalesForceIntegrationInstanceTacticID(CRMId).
                            //List<string> lstSalesForceIntegrationInstanceTacticIds = lstAllTactics.Where(tactic => string.IsNullOrEmpty(tactic.IntegrationInstanceEloquaId) && !string.IsNullOrEmpty(tactic.IntegrationInstanceTacticId)).Select(_tac => _tac.IntegrationInstanceTacticId).ToList();

                            Dictionary<int, string> lstSFDCID_TacticIDMapping = lstAllTactics.Where(tactic => string.IsNullOrEmpty(tactic.IntegrationInstanceEloquaId) && !string.IsNullOrEmpty(tactic.IntegrationInstanceTacticId)).ToDictionary(_tac => _tac.PlanTacticId, _tac => _tac.IntegrationInstanceTacticId);

                            if (lstSFDCID_TacticIDMapping == null)
                                lstSFDCID_TacticIDMapping = new Dictionary<int,string>();

                            // Log: Add Total no. of Salesforce related Ids.
                            if (lstSFDCID_TacticIDMapping.Count > 0)
                            {
                                Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull MQL: System pulling MQLs based on SalesforceId for number of record(s) - " + lstSFDCID_TacticIDMapping.Count + ".");
                                //_lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull MQL: System pulling MQLs based on SalesforceId for number of record(s) - " + lstSFDCID_TacticIDMapping.Count + ".", Enums.SyncStatus.Info, DateTime.Now));
                            }

                            //// Get Mapping List of EloquaIntegrationInstanceTactic Ids based on SalesForceIntegrationInstanceTacticID(CRMId).
                            //List<CRM_EloquaMapping> lstEloquaIntegrationInstanceTacticIds = new List<CRM_EloquaMapping>();
                            //string shortSalIntegInstanceTacticId = string.Empty;
                            string strEloquaId = string.Empty;
                            Plan_Campaign_Program_Tactic objUpdTactic, objlinkedTactic = null;
                            int cntrUpdateTac = 0, linkedTacticId =0;
                            foreach (KeyValuePair<int,string> _SalTac in lstSFDCID_TacticIDMapping)
                            {
                                if (!string.IsNullOrEmpty(_SalTac.Value))
                                {
                                    //shortSalIntegInstanceTacticId = _SalTac.Substring(0, 15);
                                    strEloquaId = integrationEloquaClient.GetEloquaCampaignIdByCRMId(_SalTac.Value);
                                    if(!string.IsNullOrEmpty(strEloquaId))
                                    {
                                        objUpdTactic = lstAllTactics.Where(tac => tac.PlanTacticId.Equals(_SalTac.Key)).FirstOrDefault();
                                        if (objUpdTactic != null)
                                        {
                                            objUpdTactic.IntegrationInstanceEloquaId = strEloquaId;
                                            db.Entry(objUpdTactic).State = EntityState.Modified;
                                            cntrUpdateTac++;

                                            #region "Update Linked Tactic IntegrationInstanceEloquaId value"
                                            #region "Retrieve linkedTactic"
                                            linkedTacticId = (objUpdTactic != null && objUpdTactic.LinkedTacticId.HasValue) ? objUpdTactic.LinkedTacticId.Value : 0;
                                            //if (linkedTacticId <= 0)
                                            //{
                                            //    var lnkPCPT = tblTactic.Where(tactic => tactic.LinkedTacticId == objUpdTactic.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                                            //    linkedTacticId = lnkPCPT != null ? lnkPCPT.PlanTacticId : 0;
                                            //}
                                            #endregion

                                            if (linkedTacticId > 0)
                                            {
                                                objlinkedTactic = new Plan_Campaign_Program_Tactic();
                                                objlinkedTactic = tblTactic.Where(tac => (tac.PlanTacticId == linkedTacticId) && (tac.IsDeleted == false)).FirstOrDefault();
                                                if (objlinkedTactic != null)
                                                {
                                                objlinkedTactic.IntegrationInstanceEloquaId = strEloquaId;
                                                db.Entry(objlinkedTactic).State = EntityState.Modified;
                                            }
                                            }
                                            #endregion
                                        }
                                    }
                                    //lstEloquaIntegrationInstanceTacticIds.Add(
                                    //                                          new CRM_EloquaMapping
                                    //                                          {
                                    //                                              CRMId = _SalTac,
                                    //                                              ShortCRMId = shortSalIntegInstanceTacticId,
                                    //                                              EloquaId = integrationEloquaClient.GetEloquaCampaignIdByCRMId(_SalTac)
                                    //                                          });
                                }
                            }
                            if(cntrUpdateTac >0)
                                db.SaveChanges();

                            //// Get Eloqua tactic list
                            List<Plan_Campaign_Program_Tactic> lstTactic = lstAllTactics.Where(tactic => (campaignIds.Contains(tactic.IntegrationInstanceEloquaId)) && tactic.Stage.Level <= MQLLevel).ToList();

                            ////// Get SalesForce tactic list
                            //List<Plan_Campaign_Program_Tactic> lstSalesForceTactic = lstAllTactics.Where(tactic => lstSalesForcePlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
                            //                                                                                               campaignIds.Contains(lstEloquaIntegrationInstanceTacticIds.Where(_instance => (_instance.CRMId == tactic.IntegrationInstanceTacticId) || (_instance.ShortCRMId == tactic.IntegrationInstanceTacticId)).Select(d => d.EloquaId).FirstOrDefault()) &&
                            //                                                                                               tactic.Stage.Level <= MQLLevel).ToList();

                            // Log: Add count of tactic on which Pulling MQL wil process.
                            Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull MQL: Total number of Tactic(s) - " + lstTactic.Count + " on which system processed Pull MQLs.");
                            _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull MQL: Total number of Tactic(s) - " + lstTactic.Count + " on which system processed Pull MQLs.", Enums.SyncStatus.Info, DateTime.Now));
                            #endregion

                            #region "Create LinkedTactic Mapping list"
                            Dictionary<int, int> lstlinkedTacticMapping = new Dictionary<int, int>();
                            foreach (var tac in lstTactic)
                            {
                                linkedTacticId = 0;
                                #region "Retrieve linkedTactic"
                                linkedTacticId = (tac != null && tac.LinkedTacticId.HasValue) ? tac.LinkedTacticId.Value : 0;
                                //if (linkedTacticId <= 0)
                                //{
                                //    var lnkPCPT = tblTactic.Where(tactic => tactic.LinkedTacticId == tac.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                                //    linkedTacticId = lnkPCPT != null ? lnkPCPT.PlanTacticId : 0;
                                //}

                                if (linkedTacticId > 0)
                                {
                                    lstlinkedTacticMapping.Add(tac.PlanTacticId, linkedTacticId);
                                }
                                #endregion
                            }
                            #endregion

                        #region Manipulate with Tactic Actual Data


                            #region "Create Actual Temp Table"
                            List<int> linkedTacticIds = new List<int>(), TacticIds = new List<int>();
                            List<Plan_Campaign_Program_Tactic_Actual> tblPlanTacticActual = new List<Plan_Campaign_Program_Tactic_Actual>();
                            TacticIds = lstTactic.Select(tac => tac.PlanTacticId).ToList();
                            if (lstlinkedTacticMapping.Count > 0)
                            {
                                linkedTacticIds = lstlinkedTacticMapping.Select(lnkTac => lnkTac.Value).ToList();
                            }

                            if (linkedTacticIds != null && linkedTacticIds.Count > 0)
                                tblPlanTacticActual = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => (linkedTacticIds.Contains(actual.PlanTacticId) || TacticIds.Contains(actual.PlanTacticId))).ToList();
                            else
                                tblPlanTacticActual = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => TacticIds.Contains(actual.PlanTacticId)).ToList();
                            #endregion

                            string contactIds = string.Empty;
                            List<int> processlinkedplantacticids = new List<int>();
                            // Insert or Update tactic actual.
                            foreach (var objTactic in lstTactic)
                            {
                                if (processlinkedplantacticids.Contains(objTactic.PlanTacticId))
                                {
                                    continue;
                                }
                                DateTime tacticStartDate = new DateTime(objTactic.StartDate.Year, 1, 1);
                                DateTime tacticEndDate = new DateTime(objTactic.EndDate.Year, 12, 31).AddDays(1).AddTicks(-1);

                                //// if IntegrationTacticID is SalesforceID(CRMID) then retrieve EloquaID based on CRMID from lstEloquaIntegrationInstanceTacticIds list.
                                string objIntegrationInstanceTacticId = string.Empty;
                                //if (lstEloquaIntegrationInstanceTacticIds.Any(_instance => _instance.CRMId == objTactic.IntegrationInstanceTacticId))
                                //    objIntegrationInstanceTacticId = lstEloquaIntegrationInstanceTacticIds.Where(_instance => (_instance.CRMId == objTactic.IntegrationInstanceTacticId) || (_instance.ShortCRMId == objTactic.IntegrationInstanceTacticId)).Select(_instance => _instance.EloquaId).FirstOrDefault();
                                //else
                                    objIntegrationInstanceTacticId = objTactic.IntegrationInstanceEloquaId;

                                //// filter list based on period for tactic start and end date.
                                List<elements> lstTacticContacts = element.Where(Objelement => !string.IsNullOrEmpty(Objelement.CampaignId) && Objelement.CampaignId == objIntegrationInstanceTacticId && Objelement.peroid >= tacticStartDate && Objelement.peroid <= tacticEndDate && Objelement.peroid != null).Select(Objelement => Objelement).ToList();

                                contactIds = contactIds + "," + string.Join(",", lstTacticContacts.Select(t => t.contactId).Distinct().ToList());

                                Dictionary<string, int> tempYearMonthDictionary = new Dictionary<string, int>();

                                var lstYearMonth = lstTacticContacts.Select(t => new { Month = t.peroid.Month, Year = t.peroid.Year }).Distinct().ToList();

                                foreach (var item in lstYearMonth)
                                {
                                    //string tmpPeriod = "Y" + item.Month;
                                    string tmpPeriod = (tacticStartDate.Year < item.Year) ? ("Y" + (((item.Year - tacticStartDate.Year) * 12) + item.Month)) : ("Y" + item.Month.ToString());
                                    tempYearMonthDictionary.Add(tmpPeriod, lstTacticContacts.Where(t => (t.peroid.Month == item.Month) && (t.peroid.Year == item.Year)).ToList().Count());
                                }

                                linkedTacticId = 0;
                                // Get linked TacticId from mapping list.
                                if (lstlinkedTacticMapping != null && lstlinkedTacticMapping.Count > 0) // check whether linkedTactics exist or not.
                                    linkedTacticId = lstlinkedTacticMapping.FirstOrDefault(tac => tac.Key == objTactic.PlanTacticId).Value;
                                if (linkedTacticId > 0)
                                {
                                    processlinkedplantacticids.Add(linkedTacticId);
                                }
                                foreach (var item in tempYearMonthDictionary)
                                {
                                    var objTacticActual = tblPlanTacticActual.FirstOrDefault(tacticActual => tacticActual.PlanTacticId == objTactic.PlanTacticId && tacticActual.Period == item.Key && tacticActual.StageTitle == Common.MQLStageValue);

                                    if (objTacticActual != null)
                                    {
                                        objTacticActual.Actualvalue = objTacticActual.Actualvalue + item.Value;
                                        objTacticActual.ModifiedDate = DateTime.Now;
                                        objTacticActual.ModifiedBy = _userId;
                                        db.Entry(objTacticActual).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        Plan_Campaign_Program_Tactic_Actual actualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                        actualTactic.Actualvalue = item.Value;
                                        actualTactic.PlanTacticId = objTactic.PlanTacticId;
                                        actualTactic.Period = item.Key;
                                        actualTactic.StageTitle = Common.MQLStageValue;
                                        actualTactic.CreatedDate = DateTime.Now;
                                        actualTactic.CreatedBy = _userId;
                                        db.Entry(actualTactic).State = EntityState.Added;
                                    }

                                    #region "Convert linked Tactic Period"
                                    string orgPeriod = string.Empty,numPeriod = string.Empty,lnkePeriod = string.Empty;
                                    int NumPeriod = 0, yearDiff = 1;
                                    Plan_Campaign_Program_Tactic linkedTactic = new Plan_Campaign_Program_Tactic();
                                    if (linkedTacticId > 0)
                                    {
                                        orgPeriod = item.Key;
                                        numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                        NumPeriod = int.Parse(numPeriod);
                                        linkedTactic = tblTactic.Where(tac => tac.PlanTacticId == linkedTacticId).FirstOrDefault();
                                        if (linkedTactic != null)
                                        {
                                        yearDiff = linkedTactic.EndDate.Year - linkedTactic.StartDate.Year;
                                        
                                        if (yearDiff > 0) //Is linked tactic Multiyear
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
                                            }
                                        }
                                    }
                                    #endregion

                                    #region "Create/Update linked Tactic MQL value"
                                    if (linkedTacticId > 0 && !string.IsNullOrEmpty(lnkePeriod))
                                    {
                                        var objLinkedTacticActual = tblPlanTacticActual.FirstOrDefault(tacticActual => tacticActual.PlanTacticId == linkedTacticId && tacticActual.Period == lnkePeriod && tacticActual.StageTitle == Common.MQLStageValue);
                                        if (objLinkedTacticActual != null)
                                        {
                                            objLinkedTacticActual.Actualvalue = objTacticActual.Actualvalue + item.Value;
                                            objLinkedTacticActual.ModifiedDate = DateTime.Now;
                                            objLinkedTacticActual.ModifiedBy = _userId;
                                            db.Entry(objLinkedTacticActual).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            Plan_Campaign_Program_Tactic_Actual actualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                            actualTactic.Actualvalue = item.Value;
                                            actualTactic.PlanTacticId = linkedTacticId;
                                            actualTactic.Period = lnkePeriod;
                                            actualTactic.StageTitle = Common.MQLStageValue;
                                            actualTactic.CreatedDate = DateTime.Now;
                                            actualTactic.CreatedBy = _userId;
                                            db.Entry(actualTactic).State = EntityState.Added;
                                        }
                                    }
                                    #endregion

                                    //// check tactic is of MQL or other type for plan tactic id.
                                    if (objTactic.Stage.Code.ToLower() == Common.StageMQL.ToLower())
                                    {
                                        //// MQL type data so update/create projected stage value and MQL value in actual table
                                        var innerTacticActual = tblPlanTacticActual.FirstOrDefault(tacticActual => tacticActual.PlanTacticId == objTactic.PlanTacticId && tacticActual.Period == item.Key && tacticActual.StageTitle == Common.StageProjectedStageValue);

                                        if (innerTacticActual != null)
                                        {
                                            innerTacticActual.Actualvalue = innerTacticActual.Actualvalue + item.Value;
                                            innerTacticActual.ModifiedDate = DateTime.Now;
                                            innerTacticActual.ModifiedBy = _userId;
                                            db.Entry(innerTacticActual).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            Plan_Campaign_Program_Tactic_Actual actualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                            actualTactic.Actualvalue = item.Value;
                                            actualTactic.PlanTacticId = objTactic.PlanTacticId;
                                            actualTactic.Period = item.Key;
                                            actualTactic.StageTitle = Common.StageProjectedStageValue;
                                            actualTactic.CreatedDate = DateTime.Now;
                                            actualTactic.CreatedBy = _userId;
                                            db.Entry(actualTactic).State = EntityState.Added;
                                        }
                                        #region "Create/Update linked Tactic Projected Stage value"
                                        if (linkedTacticId > 0 && !string.IsNullOrEmpty(lnkePeriod))
                                        {
                                            var objLinkedTacticActual = tblPlanTacticActual.FirstOrDefault(tacticActual => tacticActual.PlanTacticId == linkedTacticId && tacticActual.Period == lnkePeriod && tacticActual.StageTitle == Common.StageProjectedStageValue);
                                            if (objLinkedTacticActual != null)
                                            {
                                                objLinkedTacticActual.Actualvalue = innerTacticActual.Actualvalue + item.Value;
                                                objLinkedTacticActual.ModifiedDate = DateTime.Now;
                                                objLinkedTacticActual.ModifiedBy = _userId;
                                                db.Entry(objLinkedTacticActual).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                Plan_Campaign_Program_Tactic_Actual lnkdactualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                                lnkdactualTactic.Actualvalue = item.Value;
                                                lnkdactualTactic.PlanTacticId = linkedTacticId;
                                                lnkdactualTactic.Period = lnkePeriod;
                                                lnkdactualTactic.StageTitle = Common.StageProjectedStageValue;
                                                lnkdactualTactic.CreatedDate = DateTime.Now;
                                                lnkdactualTactic.CreatedBy = _userId;
                                                db.Entry(lnkdactualTactic).State = EntityState.Added;
                                            }
                                        }
                                        #endregion
                                    }
                                }

                                objTactic.LastSyncDate = DateTime.Now;
                                objTactic.ModifiedDate = DateTime.Now;
                                objTactic.ModifiedBy = _userId;

                                if (linkedTacticId > 0)   // check whether linkedTactics exist or not.
                                {
                                    Plan_Campaign_Program_Tactic objLinkedTactic = new Plan_Campaign_Program_Tactic();
                                    objLinkedTactic = tblTactic.Where(tac => tac.PlanTacticId == linkedTacticId).FirstOrDefault();
                                    if (objLinkedTactic != null)
                                    {
                                    objLinkedTactic.LastSyncDate = DateTime.Now;
                                    objLinkedTactic.ModifiedDate = DateTime.Now;
                                    objLinkedTactic.ModifiedBy = _userId;
                                    }
                                }

                                // Insert Log
                                IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                instanceTactic.IntegrationInstanceId = IntegrationInstanceId;
                                instanceTactic.EntityId = objTactic.PlanTacticId;
                                instanceTactic.EntityType = _entityType.ToString();
                                instanceTactic.Status = StatusResult.Success.ToString();
                                instanceTactic.Operation = Operation.Pull_QualifiedLeads.ToString();
                                instanceTactic.SyncTimeStamp = DateTime.Now;
                                instanceTactic.CreatedDate = DateTime.Now;
                                instanceTactic.CreatedBy = _userId;
                                instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                db.Entry(instanceTactic).State = EntityState.Added;
                            }

                            db.SaveChanges();

                            #endregion

                        #region Update Eloqua Contact List

                            ////  get distinct contact id(s) and set property value of membership Deletions 
                            if (contactIds != string.Empty)
                            {
                                contactIds = contactIds.Remove(0, 1);
                                List<string> contactIdslist = contactIds.Split(',').ToList();
                                contactIdslist = contactIdslist.Distinct().ToList();
                                contactListDetails.membershipDeletions = contactIdslist;
                            }

                            //// update contact id in eloqua for updated tactic contact(s) 
                            integrationEloquaClient.PutEloquaContactListDetails(contactListDetails, ListIdValue.ToString());

                            #endregion
                        if (contactListDetails != null)
                        {
                            TotalContactCount = !string.IsNullOrEmpty(contactListDetails.count) ? Convert.ToInt32(contactListDetails.count) : 0;
                            ProcessedContactCount = contactListDetails.membershipDeletions != null ? contactListDetails.membershipDeletions.Count : 0;
                        }
                        Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull MQL: Total contact(s) - " + TotalContactCount + ", " + ProcessedContactCount + " contact(s) were processed and pulled in database.");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Pull MQL: Total contact(s) - " + TotalContactCount + ", " + ProcessedContactCount + " contact(s) were processed and pulled in database.", Enums.SyncStatus.Info, DateTime.Now));
                        // Update IntegrationInstanceSection log with Success status,
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    }
                    else
                    {
                        Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Data type mapping for pull mql is not found");
                        _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "Data type mapping for pull mql is not found.", Enums.SyncStatus.Error, DateTime.Now));
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgMappingNotFoundForEloquaPullMQL);
                    }
                }
                catch (Exception e)
                {
                    string exMessage = Common.GetInnermostException(e);
                    Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "System error occurred while pulling mql from Eloqua : " + exMessage);
                    _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "System error occurred while pulling mql from Eloqua.", Enums.SyncStatus.Error, DateTime.Now));
                    // Update IntegrationInstanceSection log with Error status
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, exMessage);
                    return true;
                }
                
                Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Set Tactic MQLs");
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Set Tactic MQLs : " + exMessage);
                _lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), "System error occurred while pulling mql from Eloqua.", Enums.SyncStatus.Error, DateTime.Now));
                // Update IntegrationInstanceSection log with Error status
                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, exMessage);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Added By Dharmraj, 5-8-2014
        /// Function to retrive INQ response from Eloqua
        /// </summary>
        /// <param name="IntegrationInstanceId"></param>
        /// <returns></returns>
        public bool GetTacticResponse(int IntegrationInstanceId, int _userId, int IntegrationInstanceLogId, Guid _applicationId, out List<SyncError> lstSyncError)
        {
            lstSyncError = new List<SyncError>();
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            // Insert log into IntegrationInstanceSection, Dharmraj PL#684
            int IntegrationInstanceSectionId = Common.CreateIntegrationInstanceSection(IntegrationInstanceLogId, IntegrationInstanceId, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), DateTime.Now, _userId);
            try
            {
                // PlanIDs which has configured for "Pull response" from Eloqua instances
                //List<int> planIds = db.Plans.Where(p => p.Model.IntegrationInstanceIdINQ == IntegrationInstanceId && p.Model.Status.Equals("Published")).Select(p => p.PlanId).ToList();

                #region "Check for pulling Responses whether Model/Plan associated or not with current Instance"
                //// PlanIDs which has configured for "Pull response" from Eloqua instances
                List<Model> lstModels = db.Models.Where(objmdl => objmdl.IntegrationInstanceIdINQ == IntegrationInstanceId && objmdl.Status.Equals("Published") && objmdl.IsActive==true).ToList();
                if (lstModels == null || lstModels.Count <= 0)
                {
                    // Save & display Message: No single Model associated with current Instance.
                    Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull Responses: There is no single Model associated with this Instance to pull Responses.");
                    lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Responses: There is no single Model associated with this Instance to pull Responses.", Enums.SyncStatus.Info, DateTime.Now));
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    return false; // no error.
                }
                List<int> ModelIds = lstModels.Select(mdl => mdl.ModelId).ToList();
                List<Plan> lstPlans = db.Plans.Where(objplan => ModelIds.Contains(objplan.Model.ModelId) && objplan.IsActive == true).ToList();
                if (lstPlans == null || lstPlans.Count <= 0)
                {
                    // Save & display Message: No single Plan associated with current Instance.
                    Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull Responses: There is no single Plan associated with this Instance to pull Responses.");
                    lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Responses: There is no single Plan associated with this Instance to pull Responses.", Enums.SyncStatus.Info, DateTime.Now));
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    return false; // no error.
                }
                #endregion

                List<int> planIds = lstPlans.Select(objPlan => objPlan.PlanId).ToList();
                var objIntegrationInstanceExternalServer = db.IntegrationInstanceExternalServers.FirstOrDefault(i => i.IntegrationInstanceId == IntegrationInstanceId);
                if (objIntegrationInstanceExternalServer == null)
                {
                    lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), Common.msgExternalServerNotConfigured, Enums.SyncStatus.Error, DateTime.Now));
                    // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgExternalServerNotConfigured);
                    return true;
                    //throw new Exception(Common.msgExternalServerNotConfigured);
                }
                string InstanceId = IntegrationInstanceId.ToString();
                string _ftpURL = objIntegrationInstanceExternalServer.SFTPServerName;
                string _UserName = objIntegrationInstanceExternalServer.SFTPUserName;
                string _Password = Common.Decrypt(objIntegrationInstanceExternalServer.SFTPPassword);
                int _Port = Convert.ToInt32(objIntegrationInstanceExternalServer.SFTPPort);
                string SFTPSourcePath = objIntegrationInstanceExternalServer.SFTPFileLocation;
                if (SFTPSourcePath.Substring(SFTPSourcePath.Length - 1) != "/")
                {
                    SFTPSourcePath = SFTPSourcePath + "/";
                }
                string SFTPArchivePath = SFTPSourcePath + archiveFolder;//"Gameplan/" + archiveFolder;
                string localDestpath = rootResponseFolder;
                string extension = string.Empty;
                string filepath = string.Empty;
                ArrayList srclist = new ArrayList();
                Dictionary<string, bool> pathList = new Dictionary<string, bool>();
                Dictionary<string, bool> pathListarchived = new Dictionary<string, bool>();
                int uploadedrecord = 0;
                if (Directory.Exists(localDestpath))
                {
                    //Create local directory
                    if (!Directory.Exists(localDestpath + InstanceId))
                    {
                        Directory.CreateDirectory(localDestpath + InstanceId);
                    }
                    if (!Directory.Exists(localDestpath + InstanceId + archiveFolder))
                    {
                        Directory.CreateDirectory(localDestpath + InstanceId + archiveFolder);
                    }
                }
                else
                {
                    Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Eloqua response folder path does not exists.");
                    lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Eloqua response folder path does not exists.", Enums.SyncStatus.Info, DateTime.Now));
                    // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, string.Format(Common.msgDirectoryNotFound, localDestpath));
                    return true;
                    //throw new Exception(string.Format(Common.msgDirectoryNotFound, localDestpath));
                }

                string localRunnungPath = localDestpath + InstanceId + "/";
                string localArchivePath = localDestpath + InstanceId + archiveFolder;

                try
                {
                    Sftp client = new Sftp(_ftpURL, _UserName, _Password);
                    try
                    {
                        //connect SFTP
                        client.Connect(_Port);
                        bool isConnected = client.Connected;
                    }
                    catch (Exception ex)
                    {
                        string exMessage = Common.GetInnermostException(ex);
                        Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, Common.msgNotConnectToExternalServer + exMessage);
                        lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), Common.msgNotConnectToExternalServer, Enums.SyncStatus.Error, DateTime.Now));
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, exMessage);
                        //throw new Exception(Common.msgNotConnectToExternalServer, ex.InnerException);
                        return true;
                    }

                    try
                    {
                        srclist = client.GetFileList(SFTPSourcePath);
                    }
                    catch (Exception)
                    {
                        string exMessage = "Pull Responses: " + objIntegrationInstanceExternalServer.SFTPFileLocation + " path does not exist at External Server. Please configure proper FileLocation in ExternalServer Configuration screen.";
                        Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, exMessage);
                        lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), exMessage, Enums.SyncStatus.Error, DateTime.Now));
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, exMessage);
                        return true;
                    }
                    srclist.Remove(".");
                    srclist.Remove("..");

                    if (srclist.Count > 0)
                    {
                        // Download all files in local folder
                        foreach (var objfiles in srclist)
                        {
                            extension = Path.GetExtension(objfiles.ToString());
                            if (extension.ToLower().Trim() == ".xls" || extension.ToLower().Trim() == ".xlsx" || extension.ToLower().Trim() == ".csv")
                            {
                                client.Get(SFTPSourcePath + "/" + objfiles.ToString(), localRunnungPath);
                                pathList.Add(localRunnungPath + "/" + objfiles.ToString(), extension.ToLower().Trim() == ".csv" ? true : false);
                            }
                        }
                        List<EloquaResponseModel> lstResponse = new List<EloquaResponseModel>();

                        if (pathList != null && pathList.Count > 0)
                        {
                            foreach (string FullfileName in pathList.Keys)
                            {
                                string fileName = System.IO.Path.GetFileName(FullfileName).ToString();

                                //Convert Excel file to DataTable object and add in list
                                DataTable dt = Common.ToDataTable(FullfileName);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    uploadedrecord += dt.Rows.Count;
                                    var lstColumns = setarrExcelColumn(dt);
                                    if (lstColumns.Contains(eloquaCampaignIDColumn.ToLower()) && lstColumns.Contains(eloquaResponseDateTimeColumn.ToLower()))
                                    {
                                        var lstResult = dt.AsEnumerable().Where(a => !string.IsNullOrEmpty(a.Field<string>(eloquaResponseDateTimeColumn))).GroupBy(a => new { eloquaId = a[eloquaCampaignIDColumn], date = pathList[FullfileName] ? DateTime.ParseExact((a[eloquaResponseDateTimeColumn].ToString().Split(' ')[0]).ToString(), responsedateformat, CultureInfo.InvariantCulture).ToString("MM/yyyy") : Convert.ToDateTime(a[eloquaResponseDateTimeColumn]).ToString("MM/yyyy") })
                                                                      .Select(a => new { id = a.Key, items = a.ToList().Count });
                                        foreach (var item in lstResult)
                                        {
                                            lstResponse.Add(new EloquaResponseModel()
                                            {
                                                eloquaTacticId = item.id.eloquaId.ToString(),
                                                //externalTacticId = item.id.externalId.ToString(),
                                                peroid = Convert.ToDateTime(item.id.date),
                                                responseCount = item.items
                                            });
                                        }
                                        pathListarchived.Add(FullfileName, true);
                                    }
                                    else
                                    {
                                        lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "File : " + fileName + " : " + Common.msgRequiredColumnNotExistEloquaPullResponse, Enums.SyncStatus.Info, DateTime.Now));

                                        // Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgRequiredColumnNotExistEloquaPullResponse);
                                        //throw new Exception(Common.msgRequiredColumnNotExistEloquaPullResponse);
                                    }
                                }
                            }
                        }
                        else //File location (directory) is exist, but empty – Success
                        {
                            // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                            Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, Common.msgFileNotFound);
                            Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgFileNotFound);
                            lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), Common.msgFileNotFound, Enums.SyncStatus.Error, DateTime.Now));
                            return true;
                        }
                        List<string> lstApproveStatus = Common.GetStatusListAfterApproved();
                        #region "Get CRMID/SFDCID based on EloquaID and Update "
                        if (lstResponse.Count > 0)
                        {
                            
                            List<string> lstEloquaId = lstResponse.Where(t => !string.IsNullOrEmpty(t.eloquaTacticId)).Select(t => t.eloquaTacticId).ToList();
                            var tblTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
                                                                                                                           tactic.IsDeployedToIntegration == true &&
                                                                                                                           lstApproveStatus.Contains(tactic.Status) &&
                                                                                                                           tactic.IsDeleted == false &&
                                                                                                                           tactic.Stage.Code == Common.StageINQ).ToList();
                            List<string> lstNotExistEloquaId = (from p in lstEloquaId.AsEnumerable()
                                        where !tblTactic.Select(tac => tac.IntegrationInstanceEloquaId).Contains(p)
                                        select p).Distinct().ToList();
                            IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient(Convert.ToInt32(IntegrationInstanceId), 0, EntityType.IntegrationInstance, _userId, IntegrationInstanceLogId, _applicationId);
                            Integration.Eloqua.EloquaCampaign objEloqua;
                            List<EloquaCampaign> lstEloquaSFDCIDMapping = new List<EloquaCampaign>();
                            Plan_Campaign_Program_Tactic objUpdateTactic, objlinkedTactic = null;
                            int cntrUpdateTac = 0,linkedTacticId = 0;

                            foreach (string eloquaId in lstNotExistEloquaId)
                            {
                                objEloqua = new Integration.Eloqua.EloquaCampaign();
                                ////Get SalesForceIntegrationTacticId based on EloquaIntegrationTacticId.
                                objEloqua = integrationEloquaClient.GetEloquaCampaign(eloquaId);
                                if(objEloqua != null && !string.IsNullOrEmpty(objEloqua.crmId))
                                {
                                    objUpdateTactic = new Plan_Campaign_Program_Tactic();
                                    objUpdateTactic = tblTactic.Where(tac => (tac.IntegrationInstanceTacticId == objEloqua.crmId || tac.IntegrationInstanceTacticId.Substring(0,15)  == objEloqua.crmId.Substring(0, 15))).FirstOrDefault();
                                    if (objUpdateTactic != null && objUpdateTactic.PlanTacticId > 0)
                                    {
                                        objUpdateTactic.IntegrationInstanceEloquaId = eloquaId;
                                        db.Entry(objUpdateTactic).State = EntityState.Modified;
                                        cntrUpdateTac++;

                                        #region "Update Linked Tactic IntegrationInstanceEloquaId value"
                                            #region "Retrieve linkedTactic"
                                            linkedTacticId = (objUpdateTactic != null && objUpdateTactic.LinkedTacticId.HasValue) ? objUpdateTactic.LinkedTacticId.Value : 0;
                                            //if (linkedTacticId <= 0)
                                            //{
                                            //    var lnkPCPT = tblTactic.Where(tactic => tactic.LinkedTacticId == objUpdateTactic.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                                            //    linkedTacticId = lnkPCPT != null ? lnkPCPT.PlanTacticId : 0;
                                            //}
                                            #endregion

                                            if (linkedTacticId > 0)
                                            {
                                                objlinkedTactic = new Plan_Campaign_Program_Tactic();
                                                objlinkedTactic = tblTactic.Where(tac => (tac.PlanTacticId == linkedTacticId) && (tac.IsDeleted == false)).FirstOrDefault();
                                                if (objlinkedTactic != null)
                                                {
                                                objlinkedTactic.IntegrationInstanceEloquaId = eloquaId;
                                                db.Entry(objlinkedTactic).State = EntityState.Modified;
                                            } 
                                            } 
                                        #endregion
                                    }
                                }
                                lstEloquaSFDCIDMapping.Add(objEloqua);
                            }
                            if (cntrUpdateTac >0)
                                db.SaveChanges();
                        }
                        //return false;
                        #endregion

                        if (lstResponse.Count > 0)
                        {
                            var lstEloquaTacticId = lstResponse.Where(t => !string.IsNullOrEmpty(t.eloquaTacticId)).Select(t => t.eloquaTacticId).ToList();
                            //var lstExternalTacticId = lstResponse.Where(t => !string.IsNullOrEmpty(t.externalTacticId)).Select(t => t.externalTacticId).ToList();
                            //var lstExternalTacticIdSub = lstResponse.Where(t => !string.IsNullOrEmpty(t.externalTacticId)).Select(t => t.externalTacticId.Substring(0, 15)).ToList();
                            
                            List<Plan_Campaign_Program_Tactic> tblPlanTactic = new List<Plan_Campaign_Program_Tactic>();
                            tblPlanTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeployedToIntegration == true &&
                                                                                                                           lstApproveStatus.Contains(tactic.Status) &&
                                                                                                                           tactic.IsDeleted == false &&
                                                                                                                           tactic.Stage.Code == Common.StageINQ).ToList();

                            List<Plan_Campaign_Program_Tactic> lstTactic = tblPlanTactic.Where(tactic => planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
                                                                                                                               (lstEloquaTacticId.Contains(tactic.IntegrationInstanceEloquaId))).ToList();

                            #region "Create LinkedTactic Mapping list"
                            Dictionary<int, int> lstlinkedTacticMapping = new Dictionary<int, int>();
                            int linkedTacticId = 0;
                            foreach (var tac in lstTactic)
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

                            #region "Create Actual Temp Table"
                            List<int> linkedTacticIds = new List<int>(), TacticIds = new List<int>();
                            List<Plan_Campaign_Program_Tactic_Actual> tblPlanTacticActual = new List<Plan_Campaign_Program_Tactic_Actual>();
                            TacticIds = lstTactic.Select(tac => tac.PlanTacticId).ToList();
                            if (lstlinkedTacticMapping.Count > 0)
                            {
                                linkedTacticIds = lstlinkedTacticMapping.Select(lnkTac => lnkTac.Value).ToList();
                            }

                            if (linkedTacticIds != null && linkedTacticIds.Count > 0)
                                tblPlanTacticActual = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => (linkedTacticIds.Contains(actual.PlanTacticId) || TacticIds.Contains(actual.PlanTacticId)) && actual.StageTitle == Common.StageProjectedStageValue).ToList();
                            else
                                tblPlanTacticActual = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => TacticIds.Contains(actual.PlanTacticId) && actual.StageTitle == Common.StageProjectedStageValue).ToList(); 
                            #endregion
                            List<int> processlinkedplantacticids = new List<int>();
                            // Insert or Update tactic actuals.
                            foreach (var objTactic in lstTactic)
                            {
                                if (processlinkedplantacticids.Contains(objTactic.PlanTacticId))
                                {
                                    continue;
                                }
                                DateTime tacticStartDate = new DateTime(objTactic.StartDate.Year, 1, 1);
                                DateTime tacticEndDate = new DateTime(objTactic.EndDate.Year, 12, 31).AddDays(1).AddTicks(-1);
                                    List<EloquaResponseModel> lstTacticResponse = lstResponse.Where(r => (r.eloquaTacticId == objTactic.IntegrationInstanceEloquaId) &&
                                                                                r.peroid >= tacticStartDate && r.peroid <= tacticEndDate).ToList();
                                linkedTacticId = 0;
                                // Get linked TacticId from mapping list.
                                if (lstlinkedTacticMapping != null && lstlinkedTacticMapping.Count > 0) // check whether linkedTactics exist or not.
                                    linkedTacticId = lstlinkedTacticMapping.FirstOrDefault(tac => tac.Key == objTactic.PlanTacticId).Value;
                                if (linkedTacticId > 0)
                                {
                                    processlinkedplantacticids.Add(linkedTacticId);
                                }
                                foreach (EloquaResponseModel item in lstTacticResponse)
                                {
                                    string actualPeriod = (tacticStartDate.Year < item.peroid.Year) ? ("Y" + (((item.peroid.Year - tacticStartDate.Year) * 12) + item.peroid.Month )) : ("Y" + item.peroid.Month.ToString());
                                    var objTacticActual = tblPlanTacticActual.FirstOrDefault(a => a.PlanTacticId == objTactic.PlanTacticId && a.Period == actualPeriod && a.StageTitle == Common.StageProjectedStageValue);
                                    if (objTacticActual != null)
                                    {
                                        objTacticActual.Actualvalue = objTacticActual.Actualvalue + item.responseCount;
                                        objTacticActual.ModifiedDate = DateTime.Now;
                                        objTacticActual.ModifiedBy = _userId;
                                        db.Entry(objTacticActual).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        Plan_Campaign_Program_Tactic_Actual actualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                        actualTactic.Actualvalue = item.responseCount;
                                        actualTactic.PlanTacticId = objTactic.PlanTacticId;
                                        actualTactic.Period = actualPeriod;//"Y" + item.peroid.Month;
                                        actualTactic.StageTitle = Common.StageProjectedStageValue;
                                        actualTactic.CreatedDate = DateTime.Now;
                                        actualTactic.CreatedBy = _userId;
                                        db.Entry(actualTactic).State = EntityState.Added;
                                    }

                                    #region "Convert linked Tactic Period"
                                    string orgPeriod = string.Empty, numPeriod = string.Empty, lnkePeriod = string.Empty;
                                    int NumPeriod = 0, yearDiff = 1;
                                    Plan_Campaign_Program_Tactic linkedTactic = new Plan_Campaign_Program_Tactic();
                                    if (linkedTacticId > 0)
                                    {
                                        orgPeriod = actualPeriod;
                                        numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                        NumPeriod = int.Parse(numPeriod);
                                        linkedTactic = tblPlanTactic.Where(tac => tac.PlanTacticId == linkedTacticId).FirstOrDefault();
                                        if (linkedTactic != null)
                                        {
                                        yearDiff = linkedTactic.EndDate.Year - linkedTactic.StartDate.Year;

                                        if (yearDiff > 0) //Is linked tactic Multiyear
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
                                            }
                                        }
                                    }
                                    #endregion

                                    #region "Create/Update linked Tactic Actuals value"
                                    if (linkedTacticId > 0 && !string.IsNullOrEmpty(lnkePeriod))
                                    {
                                        var objLinkedTacticActual = tblPlanTacticActual.FirstOrDefault(a => a.PlanTacticId == linkedTacticId && a.Period == lnkePeriod && a.StageTitle == Common.StageProjectedStageValue);
                                        if (objLinkedTacticActual != null)
                                        {
                                            objLinkedTacticActual.Actualvalue = objTacticActual.Actualvalue + item.responseCount;
                                            objLinkedTacticActual.ModifiedDate = DateTime.Now;
                                            objLinkedTacticActual.ModifiedBy = _userId;
                                            db.Entry(objLinkedTacticActual).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            Plan_Campaign_Program_Tactic_Actual linkedActualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                            linkedActualTactic.Actualvalue = item.responseCount;
                                            linkedActualTactic.PlanTacticId = linkedTacticId;
                                            linkedActualTactic.Period = lnkePeriod;//"Y" + item.peroid.Month;
                                            linkedActualTactic.StageTitle = Common.StageProjectedStageValue;
                                            linkedActualTactic.CreatedDate = DateTime.Now;
                                            linkedActualTactic.CreatedBy = _userId;
                                            db.Entry(linkedActualTactic).State = EntityState.Added;
                                        } 
                                    }
                                    #endregion
                                    db.SaveChanges();
                                    lstResponse.Remove(item);
                                }

                                objTactic.LastSyncDate = DateTime.Now;
                                objTactic.ModifiedDate = DateTime.Now;
                                objTactic.ModifiedBy = _userId;

                                #region "Update linked tactic LastSyncDate, ModifiedDate, ModifiedBy"
                                if (linkedTacticId > 0)
                                {
                                    Plan_Campaign_Program_Tactic objLinkedTactic = new Plan_Campaign_Program_Tactic();
                                    objLinkedTactic = tblPlanTactic.Where(tac => tac.PlanTacticId == linkedTacticId).FirstOrDefault();
                                    if (objLinkedTactic != null)
                                    {
                                    objLinkedTactic.LastSyncDate = DateTime.Now;
                                    objLinkedTactic.ModifiedDate = DateTime.Now;
                                    objLinkedTactic.ModifiedBy = _userId;
                                    }
                                } 
                                #endregion

                                // Insert Log
                                IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                instanceTactic.IntegrationInstanceId = IntegrationInstanceId;
                                instanceTactic.EntityId = objTactic.PlanTacticId;
                                instanceTactic.EntityType = EntityType.Tactic.ToString();
                                instanceTactic.Status = StatusResult.Success.ToString();
                                instanceTactic.Operation = Operation.Import_Actuals.ToString();
                                instanceTactic.SyncTimeStamp = DateTime.Now;
                                instanceTactic.CreatedDate = DateTime.Now;
                                instanceTactic.CreatedBy = _userId;
                                instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                db.Entry(instanceTactic).State = EntityState.Added;
                            }
                        }
                        db.SaveChanges();
                        // Process Unprocess Data
                        DateTime pastdate = DateTime.Now.AddMonths(-6);
                        var unproceessdatalist = db.IntegrationInstance_UnprocessData.Where(data => data.IntegrationInstanceId == IntegrationInstanceId && data.CreatedDate >= pastdate).ToList();


                        if (unproceessdatalist.Count > 0)
                        {
                            var lstEloquaTacticId = unproceessdatalist.Where(t => !string.IsNullOrEmpty(t.EloquaCampaignID)).Select(t => t.EloquaCampaignID).ToList();
                            //var lstExternalTacticId = unproceessdatalist.Where(t => !string.IsNullOrEmpty(t.ExternalCampaignID)).Select(t => t.ExternalCampaignID).ToList();
                            //var lstExternalTacticIdSub = unproceessdatalist.Where(t => !string.IsNullOrEmpty(t.ExternalCampaignID)).Select(t => t.ExternalCampaignID.Substring(0, 15)).ToList();
                            //List<string> lstApproveStatus = Common.GetStatusListAfterApproved();

                            List<Plan_Campaign_Program_Tactic> tblUnProcessedPlanTactic = new List<Plan_Campaign_Program_Tactic>();
                            tblUnProcessedPlanTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeployedToIntegration == true &&
                                                                                                                           lstApproveStatus.Contains(tactic.Status) &&
                                                                                                                           tactic.IsDeleted == false &&
                                                                                                                           tactic.Stage.Code == Common.StageINQ).ToList();

                            List<Plan_Campaign_Program_Tactic> lstTactic = tblUnProcessedPlanTactic.Where(tactic => planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
                                                                                                                               (lstEloquaTacticId.Contains(tactic.IntegrationInstanceEloquaId))).ToList();

                            #region "Create LinkedTactic Mapping list"
                            Dictionary<int, int> lstlinkedTacticMapping = new Dictionary<int, int>();
                            int linkedTacticId = 0;
                            foreach (var tac in lstTactic)
                            {
                                linkedTacticId = 0;
                                #region "Retrieve linkedTactic"
                                linkedTacticId = (tac != null && tac.LinkedTacticId.HasValue) ? tac.LinkedTacticId.Value : 0;
                                //if (linkedTacticId <= 0)
                                //{
                                //    var lnkPCPT = tblUnProcessedPlanTactic.Where(tactic => tactic.LinkedTacticId == tac.PlanTacticId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                                //    linkedTacticId = lnkPCPT != null ? lnkPCPT.PlanTacticId : 0;
                                //}

                                if (linkedTacticId > 0)
                                {
                                    lstlinkedTacticMapping.Add(tac.PlanTacticId, linkedTacticId);
                                }
                                #endregion
                            }
                            #endregion

                            #region "Create Actual Temp Table"
                            List<int> linkedTacticIds = new List<int>(), TacticIds = new List<int>();
                            List<Plan_Campaign_Program_Tactic_Actual> tblPlanTacticActual = new List<Plan_Campaign_Program_Tactic_Actual>();
                            TacticIds = lstTactic.Select(tac => tac.PlanTacticId).ToList();
                            if (lstlinkedTacticMapping.Count > 0)
                            {
                                linkedTacticIds = lstlinkedTacticMapping.Select(lnkTac => lnkTac.Value).ToList();
                            }

                            if (linkedTacticIds != null && linkedTacticIds.Count > 0)
                                tblPlanTacticActual = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => (linkedTacticIds.Contains(actual.PlanTacticId) || TacticIds.Contains(actual.PlanTacticId)) && actual.StageTitle == Common.StageProjectedStageValue).ToList();
                            else
                                tblPlanTacticActual = db.Plan_Campaign_Program_Tactic_Actual.Where(actual => TacticIds.Contains(actual.PlanTacticId) && actual.StageTitle == Common.StageProjectedStageValue).ToList();
                            #endregion
                            List<int> processlinkedplantacticids = new List<int>();
                            // Insert or Update tactic actuals.
                            foreach (var objTactic in lstTactic)
                            {
                                if (processlinkedplantacticids.Contains(objTactic.PlanTacticId))
                                {
                                    continue;
                                }
                                DateTime tacticStartDate = new DateTime(objTactic.StartDate.Year, 1, 1);
                                DateTime tacticEndDate = new DateTime(objTactic.EndDate.Year, 12, 31).AddDays(1).AddTicks(-1);
                                    var lstTacticResponse = unproceessdatalist.Where(r => (r.EloquaCampaignID == objTactic.IntegrationInstanceEloquaId) &&
                                                                                r.ResponseDateTime >= tacticStartDate && r.ResponseDateTime <= tacticEndDate);
                                string unprocessdatalog =string.Empty;
                                linkedTacticId = 0;
                                // Get linked TacticId from mapping list.
                                if (lstlinkedTacticMapping != null && lstlinkedTacticMapping.Count > 0) // check whether linkedTactics exist or not.
                                    linkedTacticId = lstlinkedTacticMapping.FirstOrDefault(tac => tac.Key == objTactic.PlanTacticId).Value;
                                if (linkedTacticId > 0)
                                {
                                    processlinkedplantacticids.Add(linkedTacticId);
                                }
                                foreach (var item in lstTacticResponse)
                                {
                                    string actualPeriod = (tacticStartDate.Year < item.ResponseDateTime.Year) ? ("Y" + (((item.ResponseDateTime.Year - tacticStartDate.Year) * 12) + item.ResponseDateTime.Month)) : ("Y" + item.ResponseDateTime.Month.ToString());
                                    var objTacticActual = tblPlanTacticActual.FirstOrDefault(a => a.PlanTacticId == objTactic.PlanTacticId && a.Period == actualPeriod);
                                    if (objTacticActual != null)
                                    {
                                        objTacticActual.Actualvalue = objTacticActual.Actualvalue + item.ResponseCount;
                                        objTacticActual.ModifiedDate = DateTime.Now;
                                        objTacticActual.ModifiedBy = _userId;
                                        db.Entry(objTacticActual).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        Plan_Campaign_Program_Tactic_Actual actualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                        actualTactic.Actualvalue = item.ResponseCount;
                                        actualTactic.PlanTacticId = objTactic.PlanTacticId;
                                        actualTactic.Period = actualPeriod;//"Y" + item.ResponseDateTime.Month;
                                        actualTactic.StageTitle = Common.StageProjectedStageValue;
                                        actualTactic.CreatedDate = DateTime.Now;
                                        actualTactic.CreatedBy = _userId;
                                        db.Entry(actualTactic).State = EntityState.Added;
                                    }

                                    #region "Convert linked Tactic Period"
                                    string orgPeriod = string.Empty, numPeriod = string.Empty, lnkePeriod = string.Empty;
                                    int NumPeriod = 0, yearDiff = 1;
                                    Plan_Campaign_Program_Tactic linkedTactic = new Plan_Campaign_Program_Tactic();
                                    if (linkedTacticId > 0)
                                    {
                                        orgPeriod = actualPeriod;
                                        numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                                        NumPeriod = int.Parse(numPeriod);
                                        linkedTactic = tblUnProcessedPlanTactic.Where(tac => tac.PlanTacticId == linkedTacticId).FirstOrDefault();
                                        if (linkedTactic != null)
                                        {
                                        yearDiff = linkedTactic.EndDate.Year - linkedTactic.StartDate.Year;

                                        if (yearDiff > 0) //Is linked tactic Multiyear
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
                                            }
                                        }
                                    }
                                    #endregion

                                    #region "Create/Update linked Tactic Actuals value"
                                    if (linkedTacticId > 0 && !string.IsNullOrEmpty(lnkePeriod))
                                    {
                                        var objLinkedTacticActual = tblPlanTacticActual.FirstOrDefault(a => a.PlanTacticId == linkedTacticId && a.Period == actualPeriod);
                                        if (objLinkedTacticActual != null)
                                        {
                                            objTacticActual.Actualvalue = objTacticActual.Actualvalue + item.ResponseCount;
                                            objTacticActual.ModifiedDate = DateTime.Now;
                                            objTacticActual.ModifiedBy = _userId;
                                            db.Entry(objTacticActual).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            Plan_Campaign_Program_Tactic_Actual actualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                            actualTactic.Actualvalue = item.ResponseCount;
                                            actualTactic.PlanTacticId = linkedTacticId;
                                            actualTactic.Period = actualPeriod;//"Y" + item.ResponseDateTime.Month;
                                            actualTactic.StageTitle = Common.StageProjectedStageValue;
                                            actualTactic.CreatedDate = DateTime.Now;
                                            actualTactic.CreatedBy = _userId;
                                            db.Entry(actualTactic).State = EntityState.Added;
                                        }
                                    }
                                    #endregion

                                    unprocessdatalog += "(" + item.IntegrationInstanceId + "," + item.ResponseCount + "," + item.CreatedDate + ")";  
                                    db.Entry(item).State = EntityState.Deleted;
                                }

                                objTactic.LastSyncDate = DateTime.Now;
                                objTactic.ModifiedDate = DateTime.Now;
                                objTactic.ModifiedBy = _userId;

                                #region "Update linked tactic LastSyncDate, ModifiedDate, ModifiedBy"
                                if (linkedTacticId > 0)
                                {
                                    Plan_Campaign_Program_Tactic objLinkedTactic = new Plan_Campaign_Program_Tactic();
                                    objLinkedTactic = tblUnProcessedPlanTactic.Where(tac => tac.PlanTacticId == linkedTacticId).FirstOrDefault();
                                    if (objLinkedTactic != null)
                                    {
                                    objLinkedTactic.LastSyncDate = DateTime.Now;
                                    objLinkedTactic.ModifiedDate = DateTime.Now;
                                    objLinkedTactic.ModifiedBy = _userId;
                                    }
                                }
                                #endregion

                                // Insert Log
                                IntegrationInstancePlanEntityLog instanceTactic = new IntegrationInstancePlanEntityLog();
                                instanceTactic.IntegrationInstanceId = IntegrationInstanceId;
                                instanceTactic.EntityId = objTactic.PlanTacticId;
                                instanceTactic.EntityType = EntityType.Tactic.ToString();
                                instanceTactic.Status = StatusResult.Success.ToString();
                                instanceTactic.Operation = Operation.Import_Actuals.ToString();
                                instanceTactic.SyncTimeStamp = DateTime.Now;
                                instanceTactic.ErrorDescription = "Process from IntegrationInstance_UnprocessData tabel (IntegrationInstanceId,ResponseCount,CreatedDate)" + unprocessdatalog;
                                instanceTactic.CreatedDate = DateTime.Now;
                                instanceTactic.CreatedBy = _userId;
                                instanceTactic.IntegrationInstanceSectionId = IntegrationInstanceSectionId;
                                db.Entry(instanceTactic).State = EntityState.Added;
                            }
                        }

                        if (lstResponse.Count > 0)
                        {
                            foreach (var res in lstResponse)
                            {
                                IntegrationInstance_UnprocessData unprocessobj = new IntegrationInstance_UnprocessData();
                                unprocessobj.IntegrationInstanceId = IntegrationInstanceId;
                                unprocessobj.EloquaCampaignID = res.eloquaTacticId;
                                unprocessobj.ExternalCampaignID = string.Empty; //res.externalTacticId;
                                unprocessobj.ResponseDateTime = res.peroid;
                                unprocessobj.ResponseCount = res.responseCount;
                                unprocessobj.CreatedDate = DateTime.Now;
                                unprocessobj.CreatedBy = _userId;
                                db.Entry(unprocessobj).State = EntityState.Added;
                            }
                            Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Info, "Pull Responses: Total records (" + uploadedrecord + ") uploaded, " + lstResponse.Sum(l => l.responseCount).ToString() + " record(s) were not processed and stored in database; these will be processed automatically later by the system.");
                            lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "Pull Responses: Total records (" + uploadedrecord + ") uploaded, " + lstResponse.Sum(l => l.responseCount).ToString() + " record(s) were not processed and stored in database; these will be processed automatically later by the system.", Enums.SyncStatus.Info, DateTime.Now));
                        }


                        //save all data
                        db.SaveChanges();

                        if (pathListarchived != null && pathListarchived.Count > 0)
                        {
                            foreach (string FullfileName in pathListarchived.Keys)
                            {
                                string fileName = System.IO.Path.GetFileName(FullfileName).ToString();

                                // Move local file to local archived folder
                                string ProcssingFilePath = FullfileName;
                                string fileext = Path.GetExtension(fileName);
                                string filen = fileName.Replace(fileext, "");

                                string strDateTime = System.DateTime.Now.Year.ToString() + "_" + System.DateTime.Now.Month.ToString() + "_" + System.DateTime.Now.Day.ToString() + "_" + System.DateTime.Now.Hour.ToString() + "_" + System.DateTime.Now.Minute.ToString() + "_" + System.DateTime.Now.Second.ToString();
                                string ArchiveFilePath = localArchivePath + filen + "_archived_" + strDateTime + fileext;
                                string SFTPArchiveFilePathNew = SFTPArchivePath + filen + "_archived_" + strDateTime + fileext;
                                if (File.Exists(ProcssingFilePath))
                                {
                                    System.IO.File.Copy(ProcssingFilePath, ArchiveFilePath, true);
                                    File.Delete(ProcssingFilePath);

                                    try
                                    {
                                        // Make directory on external server if not exist
                                        client.Mkdir(SFTPArchivePath.Remove(SFTPArchivePath.Length - 1));
                                    }
                                    catch (Exception ex)
                                    {
                                        string exMessage = Common.GetInnermostException(ex);
                                        Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "An error occurred while creating directory at external server." + exMessage);
                                        //lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "An error occurred while creating directory at external server.", Enums.SyncStatus.Error, DateTime.Now));
                                    }

                                    // Upload processed local file to external server archived folder
                                    client.Put(ArchiveFilePath, SFTPArchiveFilePathNew);

                                    var prop = client.GetType().GetProperty("SftpChannel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                    var methodInfo = prop.GetGetMethod(true);
                                    var sftpChannel = methodInfo.Invoke(client, null);
                                    string rmfile = SFTPSourcePath + "/" + filen + fileext;
                                    ((ChannelSftp)sftpChannel).rm(rmfile);
                                }
                            }
                        }
                        // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                        Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "");
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    }
                    else //File location (directory) is exist, but empty – Success
                    {
                        // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                        Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, Common.msgFileNotFound);
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgFileNotFound);
                        lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), Common.msgFileNotFound, Enums.SyncStatus.Error, DateTime.Now));
                    }
                }
                catch (Exception ex)
                {
                    string exMessage = Common.GetInnermostException(ex);
                    Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "System error occurred while processing tactic response from Eloqua. Exception: " + exMessage);
                    lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "System error occurred while processing tactic response from Eloqua. Exception: " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                    // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, exMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                string exMessage = Common.GetInnermostException(ex);
                Common.SaveIntegrationInstanceLogDetails(IntegrationInstanceId, IntegrationInstanceLogId, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Error, "Instance have inactive status.");
                lstSyncError.Add(Common.PrepareSyncErrorList(0, Enums.EntityType.Tactic, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), "System error occurred while processing tactic response from Eloqua. Exception: " + exMessage, Enums.SyncStatus.Error, DateTime.Now));
                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, exMessage);
                return true;
            }
            return false;
        }

        public List<string> setarrExcelColumn(DataTable dt)
        {
            int columnCount = dt.Columns.Count;
            List<string> ExcelColumns = new List<string>();
            for (int i = 0; i <= dt.Columns.Count - 1; i++)
            {
                string tempColumnName = Convert.ToString(dt.Columns[i]);
                tempColumnName = tempColumnName.Trim().ToLower();
                if (!ExcelColumns.Contains(tempColumnName))
                {
                    ExcelColumns.Add(tempColumnName);
                }
            }
            return ExcelColumns;
        }

        #endregion
    }
}
public class EloquaSalesforceModel
{
    public string EloquaId { get; set; }
    public string CRMId { get; set; }
}
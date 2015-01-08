﻿//// File Used to PUT/GET response(s).

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

#endregion

namespace Integration.Eloqua
{
    public class EloquaResponse
    {
        #region Variables

        private static string archiveFolder = "/Archived/";
        private static string rootResponseFolder = ConfigurationManager.AppSettings["EloquaResponseFolderPath"].ToString();
        private static string eloquaCampaignIDColumn = "EloquaCampaignID";
        private static string externalCampaignIDColumn = "ExternalCampaignID";
        private static string eloquaResponseDateTimeColumn = "ResponseDateTime";
        private MRPEntities db = new MRPEntities();

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
            try
            {
                //connect SFTP
                Sftp client = new Sftp(_ftpURL, _UserName, _Password);
                client.Connect(_Port);
                return client.Connected;
            }
            catch (Exception ex)
            {
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
        public void SetTacticMQLs(int IntegrationInstanceId, Guid _userId, int IntegrationInstanceLogId, Guid _applicationId, EntityType _entityType, out StringBuilder _errorMailBody)
        {
            _errorMailBody = new StringBuilder(string.Empty);
            //// Insert log into IntegrationInstanceSection
            int IntegrationInstanceSectionId = Common.CreateIntegrationInstanceSection(IntegrationInstanceLogId, IntegrationInstanceId, Enums.IntegrationInstanceSectionName.PullMQL.ToString(), DateTime.Now, _userId);

            //// PlanIDs which has configured for "Pull MQL" from Eloqua instances
            List<Plan> lstPlans = db.Plans.Where(objplan => objplan.Model.IntegrationInstanceIdMQL == IntegrationInstanceId && objplan.Model.Status.Equals("Published")).ToList();
            Guid _ClientId = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId == IntegrationInstanceId).ClientId;

            //// Get SalesForce integration type Id.
            string salesforceCode = Enums.IntegrationType.Salesforce.ToString();
            var salesforceIntegrationType = db.IntegrationTypes.Where(type => type.Code == salesforceCode && type.IsDeleted == false).Select(type => type.IntegrationTypeId).FirstOrDefault();
            int SalesForceintegrationTypeId = Convert.ToInt32(salesforceIntegrationType);

            //// Get All SalesForceIntegrationTypeIds to retrieve  SalesForcePlanIds.
            List<int> lstSalesForceIntegrationTypeIds = db.IntegrationInstances.Where(instance => instance.IntegrationTypeId.Equals(SalesForceintegrationTypeId) && instance.IsDeleted.Equals(false) && instance.ClientId.Equals(_ClientId)).Select(s => s.IntegrationInstanceId).ToList();

            //// Get all PlanIds whose Tactic data PUSH on SalesForce.
            List<int> lstSalesForcePlanIds = lstPlans.Where(objplan => lstSalesForceIntegrationTypeIds.Contains(objplan.Model.IntegrationInstanceId.Value)).Select(objplan => objplan.PlanId).ToList();

            //// Get All PlanIds.
            List<int> AllplanIds = lstPlans.Select(objplan => objplan.PlanId).ToList();

            //// Get Eloqua PlanIds.
            List<int> lstEloquaplanIds = lstPlans.Where(objplan => !lstSalesForcePlanIds.Contains(objplan.PlanId)).Select(plan => plan.PlanId).ToList();
            if (AllplanIds.Count > 0)
            {
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
                        _errorMailBody.Append(DateTime.Now.ToString() + " - Data types for pull responses is not defined in DB.<br>");

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
                        if (CampaignIdValue.Equals(string.Empty))
                        {
                            _errorMailBody.Append(DateTime.Now.ToString() + " - Data type mapping for : " + CampaignIdDisplpayFieldName + " not found<br>");
                        }
                        if (MQLDateValue.Equals(string.Empty))
                        {
                            _errorMailBody.Append(DateTime.Now.ToString() + " - Data type mapping for : " + MQlDateDisplpayFieldName + " not found<br>");
                        }
                        if (ViewIdValue.Equals(string.Empty))
                        {
                            _errorMailBody.Append(DateTime.Now.ToString() + " - Data type mapping for : " + listPullDataType.Where(dataType => dataType.ActualFieldName == Enums.CustomeFieldNameMQL.ViewId.ToString()).Select(dataType => dataType.DisplayFieldName).FirstOrDefault() + " not found<br>");
                        }
                        if (ListIdValue.Equals(string.Empty))
                        {
                            _errorMailBody.Append(DateTime.Now.ToString() + " - Data type mapping for : " + listPullDataType.Where(dataType => dataType.ActualFieldName == Enums.CustomeFieldNameMQL.ListId.ToString()).Select(dataType => dataType.DisplayFieldName).FirstOrDefault() + " not found<br>");
                        }
                    }
                    //// End - Added by Sohel Pathan on 02/01/2015 for PL ticket #1068

                    if (CampaignIdValue != null && MQLDateValue != null && ViewIdValue != null && ListIdValue != null)
                    {
                        #region Get Eloqua respopnse

                        //// Initialize eloqua integration instance
                        IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient(Convert.ToInt32(IntegrationInstanceId), 0, _entityType, _userId, IntegrationInstanceLogId, _applicationId);

                        //// define models for data manipulation from Eloqua response
                        ContactListDetailModel contactListDetails = new ContactListDetailModel();
                        List<elements> element = new List<elements>();

                        //// allowed to enter only authenticated user.
                        if (integrationEloquaClient.IsAuthenticated)
                        {
                            //// Get list detail from eloqua for particular list id.
                            var lstcontactDetails = integrationEloquaClient.GetEloquaContactListDetails(ListIdValue.ToString());

                            //// Deserialize Object and store response into ContactListDetail Model
                            contactListDetails = JsonConvert.DeserializeObject<ContactListDetailModel>(lstcontactDetails.Content);

                            //// Get contact list for particular ViewId and List Id.
                            var lstcontacts = integrationEloquaClient.GetEloquaContactList(ListIdValue.ToString(), ViewIdValue.ToString());

                            //// Manipulation of contact list response and store into model
                            string TacticResult = lstcontacts.Content.ToString();
                            if (!string.IsNullOrEmpty(TacticResult))
                            {
                                JObject joResponse = JObject.Parse(TacticResult);
                                JArray elementsArray = (JArray)joResponse["elements"];

                                bool isAllCampaignIdExists = true;
                                bool isAllMQLDateExists = true;

                                for (int i = 0; i < elementsArray.Count(); i++)
                                {
                                    elements elementsInner = new elements();
                                    if (elementsArray[i][CampaignIdValue] != null)
                                    {
                                        if (!string.IsNullOrEmpty(elementsArray[i][CampaignIdValue].ToString()))
                                        {
                                            elementsInner.CampaignId = elementsArray[i][CampaignIdValue].ToString();
                                        }
                                        else
                                        {
                                            isAllCampaignIdExists = false;
                                        }

                                        if (elementsArray[i][MQLDateValue] != null)
                                        {
                                            if (elementsArray[i][MQLDateValue].ToString() != string.Empty && elementsArray[i][MQLDateValue] != null)
                                            {
                                                elementsInner.peroid = integrationEloquaClient.ConvertTimestampToDateTime(elementsArray[i][MQLDateValue].ToString());
                                            }
                                            else
                                            {
                                                isAllMQLDateExists = false;
                                            }
                                        }
                                        else
                                        {
                                            isAllMQLDateExists = false;
                                        }

                                        elementsInner.contactId = elementsArray[i]["contactId"].ToString();
                                        elementsInner.type = elementsArray[i]["type"].ToString();
                                        element.Add(elementsInner);
                                    }

                                    if (elementsArray.Count > 0 && listPullMapping.Count > 0)
                                    {
                                        if (!isAllCampaignIdExists)
                                        {
                                            _errorMailBody.Append(DateTime.Now.ToString() + " - " + CampaignIdValue + " for one or many record(s) does not exists." + "<br>");
                                        }
                                        if (!isAllMQLDateExists)
                                        {
                                            _errorMailBody.Append(DateTime.Now.ToString() + " - " + MQLDateValue + " for one or many record(s) does not exists." + "<br>");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _errorMailBody.Append(DateTime.Now.ToString() + " - No contact data found in Eloqua contact object.<br>");
                            }
                        }
                        else
                        {
                            _errorMailBody.Append(DateTime.Now.ToString() + " - Authorization for " + Enums.IntegrationType.Eloqua.ToString() + " has been failed." + "<br>");
                        }

                        //// get distinct campaign id for filter.
                        var campaignIds = element.Select(objelement => objelement.CampaignId).Distinct().ToList();

                        #endregion

                        #region Get Tactic List

                        //// Get tactic status list
                        List<string> lstApproveStatus = Common.GetStatusListAfterApproved();

                        //// Get All Approved,IsDeployedToIntegration true and IsDeleted false Tactic list.
                        List<Plan_Campaign_Program_Tactic> lstAllTactics = db.Plan_Campaign_Program_Tactic.Where(tactic => AllplanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
                                                                                                                       tactic.IsDeployedToIntegration == true &&
                                                                                                                       lstApproveStatus.Contains(tactic.Status) &&
                                                                                                                       tactic.IsDeleted == false).ToList();

                        //// Get MQL Level
                        var MQLLevel = db.Stages.Where(ObjStage => ObjStage.Code == Common.StageMQL && ObjStage.ClientId == _ClientId).Select(ObjStage => ObjStage.Level).FirstOrDefault();

                        //// Get list of SalesForceIntegrationInstanceTacticID(CRMId).
                        List<string> lstSalesForceIntegrationInstanceTacticIds = lstAllTactics.Where(tactic => lstSalesForcePlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && tactic.IntegrationInstanceTacticId != null).Select(_tac => _tac.IntegrationInstanceTacticId).ToList();

                        if (lstSalesForceIntegrationInstanceTacticIds == null)
                            lstSalesForceIntegrationInstanceTacticIds = new List<string>();

                        //// Get Mapping List of EloquaIntegrationInstanceTactic Ids based on SalesForceIntegrationInstanceTacticID(CRMId).
                        List<CRM_EloquaMapping> lstEloquaIntegrationInstanceTacticIds = new List<CRM_EloquaMapping>();
                        foreach (string _SalTac in lstSalesForceIntegrationInstanceTacticIds)
                        {
                            if (!string.IsNullOrEmpty(_SalTac))
                            {
                                lstEloquaIntegrationInstanceTacticIds.Add(
                                                                          new CRM_EloquaMapping
                                                                          {
                                                                              CRMId = _SalTac,
                                                                              EloquaId = integrationEloquaClient.GetEloquaCampaignIdByCRMId(_SalTac)
                                                                          });
                            }
                        }

                        //// Get Eloqua tactic list
                        List<Plan_Campaign_Program_Tactic> lstEloquaTactic = lstAllTactics.Where(tactic => lstEloquaplanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
                                                                                                                       campaignIds.Contains(tactic.IntegrationInstanceTacticId) &&
                                                                                                                       tactic.Stage.Level <= MQLLevel).ToList();

                        //// Get SalesForce tactic list
                        List<Plan_Campaign_Program_Tactic> lstSalesForceTactic = lstAllTactics.Where(tactic => lstSalesForcePlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
                                                                                                                       campaignIds.Contains(lstEloquaIntegrationInstanceTacticIds.Where(_instance => _instance.CRMId == tactic.IntegrationInstanceTacticId).Select(d => d.EloquaId).FirstOrDefault()) &&
                                                                                                                       tactic.Stage.Level <= MQLLevel).ToList();
                        //// Merge list of Eloqua & SalesForce Tactics.
                        List<Plan_Campaign_Program_Tactic> lstTactic = lstEloquaTactic;
                        lstSalesForceTactic.ForEach(_salesTac => lstTactic.Add(_salesTac));

                        #endregion

                        #region Manipulate with Tactic Actual Data

                        string contactIds = string.Empty;

                        // Insert or Update tactic actual.
                        foreach (var objTactic in lstTactic)
                        {
                            DateTime tacticStartDate = new DateTime(objTactic.StartDate.Year, objTactic.StartDate.Month, 1);
                            DateTime tacticEndDate = new DateTime(objTactic.EndDate.Year, objTactic.EndDate.Month, 1);

                            //// if IntegrationTacticID is SalesforceID(CRMID) then retrieve EloquaID based on CRMID from lstEloquaIntegrationInstanceTacticIds list.
                            string objIntegrationInstanceTacticId = string.Empty;
                            if (lstEloquaIntegrationInstanceTacticIds.Any(_instance => _instance.CRMId == objTactic.IntegrationInstanceTacticId))
                                objIntegrationInstanceTacticId = lstEloquaIntegrationInstanceTacticIds.Where(_instance => _instance.CRMId == objTactic.IntegrationInstanceTacticId).Select(_instance => _instance.EloquaId).FirstOrDefault();
                            else
                                objIntegrationInstanceTacticId = objTactic.IntegrationInstanceTacticId;

                            //// filter list based on period for tactic start and end date.
                            List<elements> lstTacticResponse = element.Where(Objelement => Objelement.CampaignId.Contains(objIntegrationInstanceTacticId) && Objelement.peroid >= tacticStartDate && Objelement.peroid <= tacticEndDate && Objelement.peroid != null).Select(Objelement => Objelement).ToList();

                            foreach (var item in lstTacticResponse)
                            {
                                string tmpPeriod = "Y" + item.peroid.Month.ToString();

                                //// check tactic is of MQL or other type for plan tactic id.
                                var ObjMQL = db.Plan_Campaign_Program_Tactic.FirstOrDefault(a => a.PlanTacticId == objTactic.PlanTacticId && a.Stage.Code == Common.StageMQL);

                                if (ObjMQL == null)
                                {
                                    var objTacticActual = db.Plan_Campaign_Program_Tactic_Actual.FirstOrDefault(tacticActual => tacticActual.PlanTacticId == objTactic.PlanTacticId && tacticActual.Period == tmpPeriod && tacticActual.StageTitle == Common.MQLStageValue);

                                    if (objTacticActual != null)
                                    {
                                        objTacticActual.Actualvalue = objTacticActual.Actualvalue + 1;
                                        objTacticActual.ModifiedDate = DateTime.Now;
                                        objTacticActual.ModifiedBy = _userId;
                                        db.Entry(objTacticActual).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        Plan_Campaign_Program_Tactic_Actual actualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                        actualTactic.Actualvalue = 1;
                                        actualTactic.PlanTacticId = objTactic.PlanTacticId;
                                        actualTactic.Period = "Y" + item.peroid.Month;
                                        actualTactic.StageTitle = Common.StageProjectedStageValue;
                                        actualTactic.CreatedDate = DateTime.Now;
                                        actualTactic.CreatedBy = _userId;
                                        db.Entry(actualTactic).State = EntityState.Added;
                                    }
                                }
                                else
                                {
                                    //// MQL type data so update/create projected stage value and MQL value in actual table
                                    var objTacticActual = db.Plan_Campaign_Program_Tactic_Actual.Where(tacticActual => tacticActual.PlanTacticId == objTactic.PlanTacticId && tacticActual.Period == tmpPeriod).ToList();

                                    if (objTacticActual.Count() > 0)
                                    {
                                        foreach (var itemActual in objTacticActual)
                                        {
                                            itemActual.Actualvalue = itemActual.Actualvalue + 1;
                                            itemActual.ModifiedDate = DateTime.Now;
                                            itemActual.ModifiedBy = _userId;
                                            db.Entry(itemActual).State = EntityState.Modified;
                                        }
                                    }
                                    else
                                    {
                                        Plan_Campaign_Program_Tactic_Actual actualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                        actualTactic.Actualvalue = 1;
                                        actualTactic.PlanTacticId = objTactic.PlanTacticId;
                                        actualTactic.Period = "Y" + item.peroid.Month;
                                        actualTactic.StageTitle = Common.MQLStageValue;
                                        actualTactic.CreatedDate = DateTime.Now;
                                        actualTactic.CreatedBy = _userId;
                                        db.Entry(actualTactic).State = EntityState.Added;

                                        actualTactic = new Plan_Campaign_Program_Tactic_Actual();
                                        actualTactic.Actualvalue = 1;
                                        actualTactic.PlanTacticId = objTactic.PlanTacticId;
                                        actualTactic.Period = "Y" + item.peroid.Month;
                                        actualTactic.StageTitle = Common.StageProjectedStageValue;
                                        actualTactic.CreatedDate = DateTime.Now;
                                        actualTactic.CreatedBy = _userId;
                                        db.Entry(actualTactic).State = EntityState.Added;
                                    }
                                }

                                contactIds = contactIds + "," + item.contactId;
                            }

                            objTactic.LastSyncDate = DateTime.Now;
                            objTactic.ModifiedDate = DateTime.Now;
                            objTactic.ModifiedBy = _userId;

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

                        // Update IntegrationInstanceSection log with Success status,
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    }
                    else
                    {
                        _errorMailBody.Append(DateTime.Now.ToString() + " - Data type mapping for pull mql is not found.<br>");
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgMappingNotFoundForEloquaPullMQL);
                    }
                }
                catch (Exception e)
                {
                    _errorMailBody.Append(DateTime.Now.ToString() + " - System error occured while pulling response from Eloqua.<br>");
                    string msg = e.Message;
                    // Update IntegrationInstanceSection log with Error status
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, msg);
                }
            }
            else
            {
                // Update IntegrationInstanceSection log with Success status,
                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
            }
        }

        /// <summary>
        /// Added By Dharmraj, 5-8-2014
        /// Function to retrive INQ response from Eloqua
        /// </summary>
        /// <param name="IntegrationInstanceId"></param>
        /// <returns></returns>
        public void GetTacticResponse(int IntegrationInstanceId, Guid _userId, int IntegrationInstanceLogId, out StringBuilder _errorMailBody)
        {
            _errorMailBody = new StringBuilder(string.Empty);

            // Insert log into IntegrationInstanceSection, Dharmraj PL#684
            int IntegrationInstanceSectionId = Common.CreateIntegrationInstanceSection(IntegrationInstanceLogId, IntegrationInstanceId, Enums.IntegrationInstanceSectionName.PullResponses.ToString(), DateTime.Now, _userId);

            // PlanIDs which has configured for "Pull response" from Eloqua instances
            List<int> planIds = db.Plans.Where(p => p.Model.IntegrationInstanceIdINQ == IntegrationInstanceId && p.Model.Status.Equals("Published")).Select(p => p.PlanId).ToList();
            if (planIds.Count > 0)
            {
                var objIntegrationInstanceExternalServer = db.IntegrationInstanceExternalServers.FirstOrDefault(i => i.IntegrationInstanceId == IntegrationInstanceId);
                if (objIntegrationInstanceExternalServer == null)
                {
                    _errorMailBody.Append(DateTime.Now.ToString() + " - External server settings has not been configured.<br>");
                    // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, Common.msgExternalServerNotConfigured);
                    throw new Exception(Common.msgExternalServerNotConfigured);
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
                ArrayList Listpath = new ArrayList();
                DataTable dt = new DataTable();

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
                    _errorMailBody.Append(DateTime.Now.ToString() + " - Eloqua response folder path does not exists.<br>");
                    // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, string.Format(Common.msgDirectoryNotFound, localDestpath));
                    throw new Exception(string.Format(Common.msgDirectoryNotFound, localDestpath));
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
                        _errorMailBody.Append(DateTime.Now.ToString() + " - An error occured while connecting to external server via SFTP.<br>");
                        throw new Exception(Common.msgNotConnectToExternalServer, ex.InnerException);
                    }

                    srclist = client.GetFileList(SFTPSourcePath);
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
                                Listpath.Add(localRunnungPath + "/" + objfiles.ToString());
                            }
                        }

                        if (Listpath != null && Listpath.Count > 0)
                        {
                            foreach (string FullfileName in Listpath)
                            {
                                string fileName = System.IO.Path.GetFileName(FullfileName).ToString();
                                dt = new DataTable();

                                //Convert Excel file to DataTable object
                                dt = Common.ToDataTable(FullfileName);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    var lstColumns = setarrExcelColumn(dt);
                                    if (lstColumns.Contains(eloquaCampaignIDColumn.ToLower()) && lstColumns.Contains(externalCampaignIDColumn.ToLower()) && lstColumns.Contains(eloquaResponseDateTimeColumn.ToLower()))
                                    {
                                        var lstResult = dt.AsEnumerable().GroupBy(a => new { eloquaId = a[eloquaCampaignIDColumn], externalId = a[externalCampaignIDColumn], date = Convert.ToDateTime(a[eloquaResponseDateTimeColumn]).ToString("MM/yyyy") })
                                                                      .Select(a => new { id = a.Key, items = a.ToList().Count });

                                        List<EloquaResponseModel> lstResponse = new List<EloquaResponseModel>();
                                        foreach (var item in lstResult)
                                        {
                                            lstResponse.Add(new EloquaResponseModel()
                                            {
                                                eloquaTacticId = item.id.eloquaId.ToString(),
                                                externalTacticId = item.id.externalId.ToString(),
                                                peroid = Convert.ToDateTime(item.id.date),
                                                responseCount = item.items
                                            });
                                        }

                                        if (lstResponse.Count > 0)
                                        {
                                            var lstEloquaTacticId = lstResponse.Select(t => t.eloquaTacticId).ToList();
                                            var lstExternalTacticId = lstResponse.Select(t => t.externalTacticId).ToList();
                                            List<string> lstApproveStatus = Common.GetStatusListAfterApproved();
                                            List<Plan_Campaign_Program_Tactic> lstTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) &&
                                                                                                                                           (lstExternalTacticId.Contains(tactic.IntegrationInstanceTacticId) || lstEloquaTacticId.Contains(tactic.IntegrationInstanceTacticId)) &&
                                                                                                                                           tactic.IsDeployedToIntegration == true &&
                                                                                                                                           lstApproveStatus.Contains(tactic.Status) &&
                                                                                                                                           tactic.IsDeleted == false &&
                                                                                                                                           tactic.Stage.Code == Common.StageINQ).ToList();
                                            // Insert or Update tactic actuals.
                                            foreach (var objTactic in lstTactic)
                                            {
                                                DateTime tacticStartDate = new DateTime(objTactic.StartDate.Year, objTactic.StartDate.Month, 1);
                                                DateTime tacticEndDate = new DateTime(objTactic.EndDate.Year, objTactic.EndDate.Month, 1);
                                                var lstTacticResponse = lstResponse.Where(r => (r.eloquaTacticId == objTactic.IntegrationInstanceTacticId || r.externalTacticId == objTactic.IntegrationInstanceTacticId) &&
                                                                                                r.peroid >= tacticStartDate && r.peroid <= tacticEndDate);
                                                foreach (var item in lstTacticResponse)
                                                {
                                                    string tmpPeriod = "Y" + item.peroid.Month.ToString();
                                                    var objTacticActual = db.Plan_Campaign_Program_Tactic_Actual.FirstOrDefault(a => a.PlanTacticId == objTactic.PlanTacticId && a.Period == tmpPeriod && a.StageTitle == Common.StageProjectedStageValue);
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
                                                        actualTactic.Period = "Y" + item.peroid.Month;
                                                        actualTactic.StageTitle = Common.StageProjectedStageValue;
                                                        actualTactic.CreatedDate = DateTime.Now;
                                                        actualTactic.CreatedBy = _userId;
                                                        db.Entry(actualTactic).State = EntityState.Added;
                                                    }
                                                }

                                                objTactic.LastSyncDate = DateTime.Now;
                                                objTactic.ModifiedDate = DateTime.Now;
                                                objTactic.ModifiedBy = _userId;

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

                                            db.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        _errorMailBody.Append(DateTime.Now.ToString() + " - Required column(s) does not exist in eloqua response.<br>");
                                        throw new Exception(Common.msgRequiredColumnNotExistEloquaPullResponse);
                                    }
                                }

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
                                        _errorMailBody.Append(DateTime.Now.ToString() + " - An error occured while creating directory at external server.<br>");
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
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
                    }
                    else //File location (directory) is exist, but empty – Success
                    {
                        // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                        Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, Common.msgFileNotFound);
                    }
                }
                catch (Exception ex)
                {
                    _errorMailBody.Append(DateTime.Now.ToString() + " - System error occured while processing tactic response from Eloqua.<br>");
                    // Update IntegrationInstanceSection log with Error status, Dharmraj PL#684
                    Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Error, ex.Message);
                    throw ex;
                }
            }
            else
            {
                // Update IntegrationInstanceSection log with Success status, Dharmraj PL#684
                Common.UpdateIntegrationInstanceSection(IntegrationInstanceSectionId, StatusResult.Success, string.Empty);
            }
        }

        public List<string> setarrExcelColumn(DataTable dt)
        {
            int columnCount = dt.Columns.Count;
            List<string> ExcelColumns = new List<string>();
            for (int i = 0; i <= dt.Columns.Count - 1; i++)
            {
                string tempColumnName = Convert.ToString(dt.Columns[i]);
                tempColumnName = tempColumnName.Trim().ToLower();
                ExcelColumns.Add(tempColumnName);
            }
            return ExcelColumns;
        }

        #endregion
    }
}

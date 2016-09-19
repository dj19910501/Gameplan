//// Controller to handle external service method(s).
//// It includes method(s) to interact with database, eloqua etc.

#region Using

using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using Integration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using Integration.Salesforce;
using Integration.Eloqua;
using Integration.WorkFront;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web.Caching;

#endregion

/*
 * Author : Sohel Pathan
 * Created Date : 
 * Purpose : External Service Integration
*/

namespace RevenuePlanner.Controllers
{
    public class ExternalServiceController : CommonController
    {
        #region Variables
        private MRPEntities db = new MRPEntities();
        string DateFormat = "MM/dd/yy h:mm tt";
        #endregion

        //public ExternalServiceController()
        //{
        //    if (System.Web.HttpContext.Current.Cache["CommonMsg"] == null)
        //    {
        //        Common.xmlMsgFilePath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.FullName + "\\" + System.Configuration.ConfigurationManager.AppSettings.Get("XMLCommonMsgFilePath");//Modify by Akashdeep Kadia on 09/05/2016 to resolve PL ticket #989.
        //        Common.objCached.loadMsg(Common.xmlMsgFilePath);
        //        System.Web.HttpContext.Current.Cache["CommonMsg"] = Common.objCached;
        //        CacheDependency dependency = new CacheDependency(Common.xmlMsgFilePath);
        //        System.Web.HttpContext.Current.Cache.Insert("CommonMsg", Common.objCached, dependency);
        //    }
        //    else
        //    {
        //        Common.objCached = (Message)System.Web.HttpContext.Current.Cache["CommonMsg"];

        //    }
        //}

        #region Integration Listing

        /// <summary>
        /// Integration data listing
        /// </summary>
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]  // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult Index()
        {


            //-- Get list of IntegrationTypes
            IList<SelectListItem> IntegrationList = new List<SelectListItem>();
            try
            {
                // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
                ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

                ViewBag.CurrentUserRole = Convert.ToString(Sessions.User.RoleCode);
                var dbList = db.IntegrationTypes.Where(it => it.IsDeleted.Equals(false)).Select(it => it).ToList();
                IntegrationList = dbList.Select(it => new SelectListItem() { Text = it.Title, Value = it.IntegrationTypeId.ToString(), Selected = false })
                                .OrderBy(it => it.Text, new AlphaNumericComparer()).ToList();
                TempData["ExternalFieldList"] = new SelectList(IntegrationList, "Value", "Text", IntegrationList.First());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return View();
        }

        /// <summary>
        /// Generate IntegrationService Listing.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetIntegrationServiceListings()
        {
            UpdateIntegrationInstacneLastSyncStatus();
            //// Get Integration Instance List.

            var IntegrationInstanceList = db.IntegrationInstances
                                            .Where(ii => ii.IsDeleted.Equals(false) && ii.ClientId == Sessions.User.CID)
                                            .Select(ii => ii).ToList();

            List<BDSService.User> lstUser = null;
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            lstUser = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId).ToList();
            List<IntegrationInstanceListing> returnList = new List<IntegrationInstanceListing>();
            IntegrationInstanceListing objInst = null;
            string strForceSyncUser;
            //// Return IntegrationInstance list with specific fields.
            if (IntegrationInstanceList != null && IntegrationInstanceList.Count > 0)
            {
                List<int> lstIntegrationInstanceIds = IntegrationInstanceList.Select(ii => ii.IntegrationInstanceId).ToList();
                var lstAutoSyncInstanceLog = (from log in db.IntegrationInstanceLogs
                                              where lstIntegrationInstanceIds.Contains(log.IntegrationInstanceId) && log.IsAutoSync == true
                                              group log by new { log.IntegrationInstanceId } into logdtls
                                              select logdtls.OrderByDescending(l => l.SyncEnd).FirstOrDefault()).ToList().
                                             Select(log => new
                {
                    IntegrationInstanceId = log.IntegrationInstanceId,
                    SyncStart = log.SyncStart,
                    SyncEnd = log.SyncEnd,
                    IsAutoSync = log.IsAutoSync
                });

                var lstForceSyncInstanceLog = (from log in db.IntegrationInstanceLogs
                                               where lstIntegrationInstanceIds.Contains(log.IntegrationInstanceId) && (log.IsAutoSync == false || log.IsAutoSync == null)
                                               group log by new { log.IntegrationInstanceId } into logdtls
                                               select logdtls.OrderByDescending(l => l.SyncEnd).FirstOrDefault()).ToList().
                                             Select(log => new
                                             {
                                                 IntegrationInstanceId = log.IntegrationInstanceId,
                                                 SyncStart = log.SyncStart,
                                                 SyncEnd = log.SyncEnd,
                                                 IsAutoSync = log.IsAutoSync
                                             });

                foreach (var inst in IntegrationInstanceList)
                {
                    objInst = new IntegrationInstanceListing();

                    #region "Get last force sync date"
                    string strForceSyncDate = string.Empty;
                    if (lstForceSyncInstanceLog != null && lstForceSyncInstanceLog.Count() > 0)
                    {
                        var ForceSynclog = lstForceSyncInstanceLog.Where(log => log.IntegrationInstanceId == inst.IntegrationInstanceId).FirstOrDefault();
                        if (ForceSynclog != null && ForceSynclog.SyncEnd.HasValue)
                            strForceSyncDate = Convert.ToDateTime(ForceSynclog.SyncEnd.Value).ToString(DateFormat);
                        else
                            strForceSyncDate = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                    }
                    #endregion

                    #region "Get last Auto sync date"
                    string strAutoSyncDate = string.Empty;
                    if (lstAutoSyncInstanceLog != null && lstAutoSyncInstanceLog.Count() > 0)
                    {
                        var AutoSynclog = lstAutoSyncInstanceLog.Where(log => log.IntegrationInstanceId == inst.IntegrationInstanceId).FirstOrDefault();
                        if (AutoSynclog != null && AutoSynclog.SyncEnd.HasValue)
                            strAutoSyncDate = Convert.ToDateTime(AutoSynclog.SyncEnd.Value).ToString(DateFormat);
                        else
                            strAutoSyncDate = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                    }
                    #endregion

                    objInst.IntegrationInstanceId = inst.IntegrationInstanceId;
                    objInst.IntegrationTypeId = inst.IntegrationTypeId;
                    objInst.Instance = (inst.Instance == null || inst.Instance.ToString() == "null") ? "" : inst.Instance;
                    objInst.Provider = (inst.IntegrationType == null || string.IsNullOrEmpty(inst.IntegrationType.Title)) ? "" : inst.IntegrationType.Title;
                    objInst.LastSyncStatus = string.IsNullOrWhiteSpace(inst.LastSyncStatus) ? Common.TextForModelIntegrationInstanceTypeOrLastSyncNull : inst.LastSyncStatus;
                    objInst.LastSyncDate = strForceSyncDate;  // Last Force Sync Date
                    objInst.AutoLastSyncDate = strAutoSyncDate; // Last Sync Date of Win Service.

                    #region "Get Force Sync User"
                    strForceSyncUser = string.Empty;
                    if (inst.ForceSyncUser != 0 && lstUser != null && lstUser.Count > 0)
                        strForceSyncUser = lstUser.Where(a => a.ID == inst.ForceSyncUser).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault();
                    else
                        strForceSyncUser = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                    objInst.ForceSyncUser = strForceSyncUser;
                    #endregion
                    if (objInst != null)
                    {
                        returnList.Add(objInst);
                    }
                }
                if (returnList != null && returnList.Count > 0)
                {
                    returnList = returnList.OrderByDescending(intgrtn => intgrtn.Instance, new AlphaNumericComparer()).ToList();
                }
            }
            return Json(returnList, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Created By: Viral Kadiya
        /// Created Date: 02/19/2016
        /// Update last sync status from "In-Progress" to "Error" for those instances running continuously and not stoped.
        /// </summary>
        /// <returns></returns>
        public void UpdateIntegrationInstacneLastSyncStatus()
        {
            try
            {
                string strIntegrationTimeout = System.Configuration.ConfigurationManager.AppSettings["IntegrationTimeOut"];
                int cmprHrDiff = 0;
                if (!string.IsNullOrEmpty(strIntegrationTimeout))
                    cmprHrDiff = Convert.ToInt32(strIntegrationTimeout);
                string errorDescription = "Instance stopped by system forcefully since it was runnig from long time.";
                DateTime currentdate = System.DateTime.Now;
                string strInProgressStatus = Enums.SyncStatusValues[Enums.SyncStatus.InProgress.ToString()].ToString(); // Get In-Progress status.
                //// Get Integration Instance List.
                List<IntegrationInstance> IntegrationInstanceList = db.IntegrationInstances
                                                .Where(ii => ii.IsDeleted.Equals(false) && ii.ClientId == Sessions.User.CID && ii.LastSyncStatus == strInProgressStatus)
                                                .Select(ii => ii).ToList();

                if (IntegrationInstanceList != null && IntegrationInstanceList.Count > 0)
                {
                    #region "Declare local variables"
                    List<int> lstInstanceIds = new List<int>();
                    IntegrationInstanceLog objIntegrationInstancelog;
                    TimeSpan timeDiff = new TimeSpan();
                    bool isDataUpdated = false;
                    #endregion

                    lstInstanceIds = IntegrationInstanceList.Select(inst => inst.IntegrationInstanceId).ToList();
                    List<IntegrationInstanceLog> tblInstanceLogs = db.IntegrationInstanceLogs.Where(log => lstInstanceIds.Contains(log.IntegrationInstanceId)).ToList();    // get IntegrationInstance logs

                    foreach (IntegrationInstance instance in IntegrationInstanceList)
                    {
                        objIntegrationInstancelog = new IntegrationInstanceLog();
                        // get last integration instance log from IntegrationInstanceLogs table.
                        objIntegrationInstancelog = tblInstanceLogs.Where(log => log.IntegrationInstanceId == instance.IntegrationInstanceId).OrderByDescending(log => log.SyncStart).FirstOrDefault();
                        if (objIntegrationInstancelog != null)
                        {
                            var syncstartdt = objIntegrationInstancelog.SyncStart;
                            timeDiff = (System.DateTime.Now - syncstartdt);
                            if (timeDiff.TotalHours > cmprHrDiff)   // if sync process running from more than 4 hrs than stop it.
                            {
                                //// Update last sync status for instances running from long time.
                                objIntegrationInstancelog.SyncEnd = System.DateTime.Now;
                                objIntegrationInstancelog.Status = Enums.SyncStatus.Error.ToString();
                                objIntegrationInstancelog.ErrorDescription = errorDescription;
                                db.Entry(objIntegrationInstancelog).State = EntityState.Modified;

                                // Update IntegrationInstance table
                                instance.LastSyncStatus = Enums.SyncStatus.Error.ToString();    // update last sync status.
                                instance.LastSyncDate = System.DateTime.Now;                    // update last sync date.
                                db.Entry(instance).State = EntityState.Modified;
                                isDataUpdated = true;
                            }
                        }
                    }
                    if (isDataUpdated)
                        db.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        /// <summary>
        /// This will return view for Integration folder.
        /// </summary>
        /// <param name="id">Integration id.</param>
        /// <param name="TypeId">Integration type id.</param>        
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]
        //[HttpPost]
        public ActionResult GetIntegrationFolder(int TypeId, int id = 0)
        {
            try
            {
                ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
                string status = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()];
                ViewBag.IntegrationInstanceId = id;
                ViewBag.IntegrationTypeId = TypeId;

                IntegrationType integrationTypeObj = GetIntegrationTypeById(TypeId);

                if (integrationTypeObj != null)
                {
                    ViewBag.IntegrationTypeCode = integrationTypeObj.Code;
                }

                int clientId = Sessions.User.CID;

                ////Get published plan list year for logged in client.
                var objPlan = (from _pln in db.Plans
                               join _mdl in db.Models on _pln.ModelId equals _mdl.ModelId
                               where _mdl.ClientId == clientId && _mdl.IsDeleted == false && _pln.IsDeleted == false && _pln.Status == status
                               select _pln).OrderBy(_pln => _pln.Year).ToList().Select(_pln => _pln.Year).Distinct().ToList();

                ViewBag.Year = objPlan;
            }
            catch (Exception)
            {

                throw;
            }

            return View("IntegrationFolder");
        }

        /// <summary>
        /// This method will return the partial view for Integration Folder published Plan listing for selected year
        /// </summary>
        /// <param name="Year">Year.</param>
        /// <returns>Return partial view.</returns>
        public PartialViewResult GetIntegrationFolderPlanList(string Year)
        {
            List<IntegrationPlanList> objIntegrationPlanList = new List<IntegrationPlanList>();

            try
            {
                int clientId = Sessions.User.CID;
                string status = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()];
                int Int_Year = Convert.ToInt32(!string.IsNullOrEmpty(Year) ? Convert.ToInt32(Year) : 0);

                // Get the list of plan, filtered by Year selected and published plan for logged in client.
                if (Int_Year > 0)
                {
                    objIntegrationPlanList = (from _pln in db.Plans
                                              join _mdl in db.Models on _pln.ModelId equals _mdl.ModelId
                                              where _mdl.ClientId == clientId && _mdl.IsDeleted == false &&
                                              _pln.IsDeleted == false && _pln.Year == Year && _pln.Status == status
                                              select new { _pln, _mdl }).OrderByDescending(d => d._pln.ModifiedDate ?? d._pln.CreatedDate).ThenBy(d => d._pln.Title).ToList().Select(d => new IntegrationPlanList { PlanId = d._pln.PlanId, PlanTitle = d._pln.Title, FolderPath = d._pln.EloquaFolderPath }).ToList();

                    //// Set permission for the plan on bases of BU permission
                    foreach (var item in objIntegrationPlanList)
                    {
                        item.Permission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return PartialView("_IntegrationFolderPlanList", objIntegrationPlanList);
        }

        /// <summary>
        /// This will save plan list for eloqua folder path.
        /// </summary>
        /// <param name="IntegrationPlanList">IntegrationPlanList Model list.</param>
        /// <returns>Return json value.</returns>
        [HttpPost]
        public JsonResult SaveIntegrationFolderPlanList(List<IntegrationPlanList> IntegrationPlanList)
        {
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        if (IntegrationPlanList.Count > 0)
                        {
                            //// Iterate Integration model list and save it to database.
                            Plan objPlan = null;
                            foreach (var item in IntegrationPlanList)
                            {
                                objPlan = new Plan();
                                objPlan = db.Plans.Where(_pln => _pln.PlanId == item.PlanId).FirstOrDefault();
                                objPlan.EloquaFolderPath = item.FolderPath;
                                db.Entry(objPlan).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }

                        scope.Complete();
                        return Json(new { IsSaved = true, Message = Common.objCached.IntegrationFolderPathSaved.ToString() });
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { IsSaved = false, Message = Common.objCached.ErrorOccured });
        }

        #endregion

        #region Markto Folder Plan List

        /// <summary>
        /// This will return view for Marketo folder.
        /// </summary>
        /// <param name="id">Integration id.</param>
        /// <param name="TypeId">Integration type id.</param>        
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]
        //[HttpPost]
        public ActionResult GetMarketoFolder(int TypeId, int id = 0)
        {
            try
            {
                ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
                string status = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()];
                ViewBag.IntegrationInstanceId = id;
                ViewBag.IntegrationTypeId = TypeId;

                IntegrationType integrationTypeObj = GetIntegrationTypeById(TypeId);

                if (integrationTypeObj != null)
                {
                    ViewBag.IntegrationTypeCode = integrationTypeObj.Code;
                }

                int clientId = Sessions.User.CID;

                ////Get published plan list year for logged in client.
                var objPlan = (from _pln in db.Plans
                               join _mdl in db.Models on _pln.ModelId equals _mdl.ModelId
                               where _mdl.ClientId == clientId && _mdl.IsDeleted == false && _pln.IsDeleted == false && _pln.Status == status
                               select _pln).OrderBy(_pln => _pln.Year).ToList().Select(_pln => _pln.Year).Distinct().ToList();

                ViewBag.Year = objPlan;
            }
            catch (Exception)
            {

                throw;
            }

            return View("MarketoFolder");
        }

        /// <summary>
        /// Created By Nishant Sheth
        /// Created Date : 21-May-2016
        /// This method will return the partial view for Marketo Integration Folder published Plan listing for selected year
        /// </summary>
        /// <param name="Year">Year.</param>
        /// <returns>Return partial view.</returns>
        public PartialViewResult GetMarketoFolderPlanList(string Year, int IntegrationInstanceId = 0)
        {
            List<IntegrationPlanList> objIntegrationPlanList = new List<IntegrationPlanList>();

            try
            {
                int clientId = Sessions.User.CID;
                string status = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()];
                int Int_Year = Convert.ToInt32(!string.IsNullOrEmpty(Year) ? Convert.ToInt32(Year) : 0);

                // Get the list of plan, filtered by Year selected and published plan for logged in client.
                if (Int_Year > 0)
                {
                    string EntityType = Enums.EntityType.Plan.ToString();

                    var ListMarketoEntity = db.MarketoEntityValueMappings.Where(a => a.IntegrationInstanceId == IntegrationInstanceId).ToList();

                    objIntegrationPlanList = (from _pln in db.Plans
                                              join _mdl in db.Models on _pln.ModelId equals _mdl.ModelId
                                              where
                                              _mdl.ClientId == clientId && _mdl.IsDeleted == false &&
                                              _pln.IsDeleted == false && _pln.Year == Year && _pln.Status == status
                                              select new { _pln, _mdl })
                                                 .OrderByDescending(d => d._pln.ModifiedDate ?? d._pln.CreatedDate).ThenBy(d => d._pln.Title).ToList()
                                                 .Select(d => new IntegrationPlanList
                                                 {
                                                     PlanId = d._pln.PlanId,
                                                     PlanTitle = d._pln.Title,
                                                     CampaignfolderValue = ListMarketoEntity.Where(a => int.Parse(a.EntityID.ToString()) == d._pln.PlanId && a.EntityType == EntityType).Select(a => a.MarketoCampaignFolderId).FirstOrDefault()
                                                 }).ToList();

                    //// Set permission for the plan on bases of BU permission
                    foreach (var item in objIntegrationPlanList)
                    {
                        item.Permission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                    }

                    ApiIntegration ObjApiintegration = new ApiIntegration(Enums.ApiIntegrationData.CampaignFolderList.ToString(), IntegrationInstanceId);

                    var CampaignFolderList = ObjApiintegration.GetMarketoCampaignFolderList().data.ToList();

                    ViewBag.CampaignFolderList = CampaignFolderList.Select(list => new SelectListItem
                    {
                        Text = list.Value,
                        Value = list.Key
                    }).ToList();

                    ViewBag.IntegrationInstanceId = IntegrationInstanceId;

                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return PartialView("_MarketoFolderPlanList", objIntegrationPlanList);
        }

        /// <summary>
        /// This will save plan list for eloqua folder path.
        /// </summary>
        /// <param name="IntegrationPlanList">IntegrationPlanList Model list.</param>
        /// <returns>Return json value.</returns>
        [HttpPost]
        public JsonResult SaveMarketoCampaignFolderPlanList(List<IntegrationPlanList> IntegrationPlanList, int IntegrationInstanceId = 0, string Year = "")
        {
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        if (IntegrationPlanList.Count > 0)
                        {
                            if (string.IsNullOrEmpty(Year))
                            {
                                Year = DateTime.Now.Year.ToString();
                            }                            

                            // Get list of plan ids from database with IntegrationInstanceId and EntityType.
                            string EntityType = Enums.EntityType.Plan.ToString();
                            var lstCampaignFolderPlan = mrp.MarketoEntityValueMappings.Where(data => data.IntegrationInstanceId == IntegrationInstanceId && data.EntityType == EntityType).ToList();

                            // Get list of model list
                            var SelectedPlanList = IntegrationPlanList.Where(data => data.CampaignfolderValue != null && int.Parse(data.CampaignfolderValue) > 0
                                && data.Year == Year).ToList();

                            // Get List of planid which have allready value in database but in model set as 0
                            List<int> lstDeleteEntityId = new List<int>();
                            lstDeleteEntityId = lstCampaignFolderPlan.Select(dbData => int.Parse(dbData.EntityID.ToString())).Except(SelectedPlanList.Select(data => data.PlanId)).ToList();
                            if (lstDeleteEntityId.Count > 0)
                            {
                                var GetDeletePlanYearlist = mrp.Plans.Where(dbData => lstDeleteEntityId.Contains(dbData.PlanId) && dbData.Year == Year)
                                    .Select(plan => plan.PlanId).ToList();
                                lstDeleteEntityId = new List<int>();
                                GetDeletePlanYearlist.ForEach(data => { lstDeleteEntityId.Add(data); });
                            }
                            // Set list for remove planids entry from database
                            List<MarketoEntityValueMapping> lstDeleteCampaignFolderPlan = lstCampaignFolderPlan.
                                Where(data => lstDeleteEntityId.Contains(int.Parse(data.EntityID.ToString()))).ToList();

                            // Set state for Delete
                            lstDeleteCampaignFolderPlan.ForEach(data =>
                                {
                                    mrp.Entry(data).State = EntityState.Deleted;
                                });

                            List<MarketoEntityValueMapping> lstUpdateCampaignFolderPlan = new List<MarketoEntityValueMapping>(); // list for update records
                            List<MarketoEntityValueMapping> lstAddCampaignFolderPlan = new List<MarketoEntityValueMapping>(); // list for add records
                            var objAddCampaign = new MarketoEntityValueMapping();

                            foreach (var item in SelectedPlanList)
                            {
                                var CheckisExist = lstCampaignFolderPlan.Where(data => int.Parse(data.EntityID.ToString()) == item.PlanId).FirstOrDefault();
                                if (CheckisExist != null)
                                {
                                    CheckisExist.MarketoCampaignFolderId = item.CampaignfolderValue;
                                    lstUpdateCampaignFolderPlan.Add(CheckisExist);
                                }
                                else
                                {
                                    objAddCampaign = new MarketoEntityValueMapping();
                                    objAddCampaign.MarketoCampaignFolderId = item.CampaignfolderValue;
                                    objAddCampaign.LastModifiedBy = Sessions.User.ID;
                                    objAddCampaign.LastModifiedDate = DateTime.Now.Date;
                                    objAddCampaign.EntityType = Enums.EntityType.Plan.ToString();
                                    objAddCampaign.EntityID = item.PlanId;
                                    objAddCampaign.IntegrationInstanceId = IntegrationInstanceId;
                                    lstAddCampaignFolderPlan.Add(objAddCampaign);
                                }
                            }

                            lstUpdateCampaignFolderPlan.ForEach(data => { mrp.Entry(data).State = EntityState.Modified; }); // set state for update
                            lstAddCampaignFolderPlan.ForEach(data => { mrp.Entry(data).State = EntityState.Added; });// set state for add

                            mrp.SaveChanges();
                        }

                        scope.Complete();
                        return Json(new { IsSaved = true, Message = Common.objCached.MarketoCampaignSaved.ToString() });
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { IsSaved = false, Message = Common.objCached.ErrorOccured });
        }
        #endregion

        #region Add Integration

        /// <summary>
        /// Add new Integration Service
        /// </summary>
        /// <param name="integrationTypeId"></param>
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]  // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult create(int integrationTypeId)
        {
            IntegrationModel objModelToView = new IntegrationModel();
            try
            {
                // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
                ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

                ViewBag.integrationTypeId = integrationTypeId;

                //// Add IntegrationTypeAttributes to IntegrationModel.
                objModelToView.IntegrationTypeAttributes = GetIntegrationTypeAttributesModelById(integrationTypeId);

                //// Add IntegrationTypeModel data to Integration Model.
                IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
                IntegrationType integrationTypeObj = GetIntegrationTypeById(integrationTypeId);
                objIntegrationTypeModel.Title = integrationTypeObj.Title;
                objIntegrationTypeModel.Code = integrationTypeObj.Code;

                objModelToView.IntegrationType = objIntegrationTypeModel;
                objModelToView.IntegrationTypeId = integrationTypeId;

                populateSyncFreqData();
                objModelToView.ExternalServer = GetExternalServerModelByInstanceId(0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(objModelToView);
        }

        /// <summary>
        /// Save new integration Service
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]    // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult create(IntegrationModel form)
        {
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

            ViewBag.integrationTypeId = form.IntegrationTypeId;

            //// Check Integration Credentials.
            if (!TestIntegrationCredentialsWithForm(form))
            {
                TempData["ErrorMessage"] = Common.objCached.TestIntegrationFail;
                form = reCreateView(form);
                return View(form);
            }
            try
            {
                using (var scope = new TransactionScope())
                {
                    //// Handle Duplication.
                    var isDuplicate = db.IntegrationInstances.Where(_intgrtn => _intgrtn.Instance == form.Instance && _intgrtn.ClientId == Sessions.User.CID && _intgrtn.IntegrationType.IntegrationTypeId == form.IntegrationTypeId).Any();
                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = Common.objCached.IntegrationDuplicate;
                        form = reCreateView(form);
                        return View(form);
                    }

                    // Save data
                    IntegrationInstance objIntegrationInstance = new IntegrationInstance();
                    objIntegrationInstance.ClientId = Sessions.User.CID;
                    objIntegrationInstance.CreatedBy = Sessions.User.ID;
                    objIntegrationInstance.CreatedDate = DateTime.Now;
                    objIntegrationInstance.Instance = form.Instance;
                    objIntegrationInstance.IsDeleted = false;
                    objIntegrationInstance.IsImportActuals = form.IsImportActuals;
                    objIntegrationInstance.IsActive = form.IsActive;
                    objIntegrationInstance.Password = Common.Encrypt(form.Password);
                    objIntegrationInstance.Username = form.Username;
                    objIntegrationInstance.IntegrationTypeId = form.IntegrationTypeId;
                    db.Entry(objIntegrationInstance).State = System.Data.EntityState.Added;
                    db.IntegrationInstances.Add(objIntegrationInstance);
                    int IntegrationInstancesCount = db.SaveChanges();

                    SyncFrequency objSyncFrequency = new SyncFrequency();
                    objSyncFrequency.IntegrationInstanceId = objIntegrationInstance.IntegrationInstanceId;
                    objSyncFrequency.CreatedBy = Sessions.User.ID;
                    objSyncFrequency.CreatedDate = DateTime.Now;
                    objSyncFrequency.Frequency = form.SyncFrequency.Frequency;
                    if (form.SyncFrequency.Frequency == SyncFrequencys.Weekly.ToString())
                        objSyncFrequency.DayofWeek = form.SyncFrequency.DayofWeek;
                    else if (form.SyncFrequency.Frequency == SyncFrequencys.Monthly.ToString())
                        objSyncFrequency.Day = form.SyncFrequency.Day;
                    if (form.SyncFrequency.Frequency != SyncFrequencys.Hourly.ToString())
                    {
                        if (form.SyncFrequency.Time.Length == 8)
                        {
                            int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                            if (form.SyncFrequency.Time.Substring(5, 2) == "PM" && hour != 12)
                                hour = hour + 12;
                            objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                        }
                    }

                    //// Set NextSyncDate to SyncFrequency list.
                    if (form.SyncFrequency.Frequency == SyncFrequencys.Hourly.ToString())
                    {
                        DateTime currentDateTime = DateTime.Now.AddHours(1);
                        objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0);
                    }
                    else if (form.SyncFrequency.Frequency == SyncFrequencys.Daily.ToString())
                    {
                        DateTime currentDateTime = DateTime.Now.AddDays(1);
                        TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                        objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, time.Hours, time.Minutes, time.Seconds);
                    }
                    else if (form.SyncFrequency.Frequency == SyncFrequencys.Weekly.ToString())
                    {
                        TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                        DateTime nextDate = GetNextDateForDay(DateTime.Now, (DayOfWeek)Enum.Parse(typeof(DayOfWeek), objSyncFrequency.DayofWeek), time);
                        objSyncFrequency.NextSyncDate = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, time.Hours, time.Minutes, time.Seconds);
                    }
                    else if (form.SyncFrequency.Frequency == SyncFrequencys.Monthly.ToString())
                    {
                        DateTime currentDateTime = DateTime.Now;
                        if (Convert.ToInt32(objSyncFrequency.Day) <= currentDateTime.Day)
                        {
                            currentDateTime.AddMonths(1);
                        }
                        TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                        objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, Convert.ToInt32(objSyncFrequency.Day), time.Hours, time.Minutes, time.Seconds);
                    }

                    db.Entry(objSyncFrequency).State = System.Data.EntityState.Added;
                    db.SyncFrequencies.Add(objSyncFrequency);
                    int SyncFrequenciesCount = db.SaveChanges();

                    //// Handle IntegrationTypeAttributes.
                    if (form.IntegrationTypeAttributes != null)
                    {
                        IntegrationInstance_Attribute objIntegrationInstance_Attribute = null;
                        foreach (var item in form.IntegrationTypeAttributes)
                        {
                            objIntegrationInstance_Attribute = new IntegrationInstance_Attribute();
                            objIntegrationInstance_Attribute.CreatedBy = Sessions.User.ID;
                            objIntegrationInstance_Attribute.CreatedDate = DateTime.Now;
                            objIntegrationInstance_Attribute.IntegrationInstanceId = objIntegrationInstance.IntegrationInstanceId;
                            objIntegrationInstance_Attribute.IntegrationTypeAttributeId = item.IntegrationTypeAttributeId;
                            objIntegrationInstance_Attribute.Value = item.Value;
                            db.Entry(objIntegrationInstance_Attribute).State = System.Data.EntityState.Added;
                            db.IntegrationInstance_Attribute.Add(objIntegrationInstance_Attribute);
                            db.SaveChanges();
                        }
                    }

                    if (IntegrationInstancesCount > 0 && SyncFrequenciesCount > 0)
                    {
                        scope.Complete();
                        TempData["SuccessMessage"] = Common.objCached.IntegrationAdded;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                        form = reCreateView(form);
                        return View(form);
                    }
                }
            }
            catch
            {
                TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                form = reCreateView(form);
                return View(form);
            }
        }

        /// <summary>
        /// Action to Get Next date for Day.  
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="desiredDay"></param>
        /// <returns></returns>
        //public static DateTime GetNextDateForDay(DateTime startDate, DayOfWeek desiredDay, TimeSpan timeset)
        //{
        //    int start = (int)startDate.DayOfWeek;
        //    TimeSpan todaytime = startDate.TimeOfDay;
        //    int target = (int)desiredDay;
        //    int daycount = 7;
        //    if (target == start)
        //    {
        //        if (todaytime < timeset)
        //        {
        //            daycount = 0;
        //        }
        //    }
        //    if (target >= start)
        //    {
        //        target += daycount;
        //    }
        //    return startDate.AddDays(target - start);
        //}
        //Added by Rahul Rahul Shah to set the logic for nect sync date
        public static DateTime GetNextDateForDay(DateTime startDate, DayOfWeek desiredDay, TimeSpan timeset)
        {
            int start = (int)startDate.DayOfWeek;
            TimeSpan todaytime = startDate.TimeOfDay;
            int target = (int)desiredDay;
            int Totaldaycount = 7;
            int daycount = 0;
            if (target == start)
            {
                if (todaytime < timeset)
                {
                    daycount = 0;
                }
                else
                {
                    daycount = Totaldaycount;
                }
            }
            else if (target > start)
            {
                daycount = target - start;
            }
            else
            {
                daycount = Totaldaycount - (start - target);
            }
            return startDate.AddDays(daycount);
        }

        #endregion

        #region Common Functions

        /// <summary>
        /// Populate sync frequency combos
        /// </summary>
        public void populateSyncFreqData()
        {
            #region " Bind Sync Frequency Dropdown List "
            List<SelectListItem> lstSyncFreq = new List<SelectListItem>();

            //// Add Static fields to Frequency List.
            SelectListItem objItem1 = new SelectListItem();
            objItem1.Text = SyncFrequencys.Hourly.ToString();
            objItem1.Value = SyncFrequencys.Hourly.ToString();
            lstSyncFreq.Add(objItem1);

            SelectListItem objItem2 = new SelectListItem();
            objItem2.Text = SyncFrequencys.Daily.ToString();
            objItem2.Value = SyncFrequencys.Daily.ToString();
            lstSyncFreq.Add(objItem2);

            SelectListItem objItem3 = new SelectListItem();
            objItem3.Text = SyncFrequencys.Weekly.ToString();
            objItem3.Value = SyncFrequencys.Weekly.ToString();
            lstSyncFreq.Add(objItem3);

            SelectListItem objItem4 = new SelectListItem();
            objItem4.Text = SyncFrequencys.Monthly.ToString();
            objItem4.Value = SyncFrequencys.Monthly.ToString();
            lstSyncFreq.Add(objItem4);

            TempData["lstSyncFreq"] = new SelectList(lstSyncFreq, "Value", "Text", lstSyncFreq.First());
            #endregion

            #region " Bind Hours Dropdown List "
            List<SelectListItem> lst24Hours = GetHoursList();
            TempData["lst24Hours"] = new SelectList(lst24Hours, "Value", "Text", lst24Hours.First());
            #endregion

            #region " Bind WeekDays Dropdown List "
            List<SelectListItem> lstWeekdays = GetWeekDaysList();
            TempData["lstWeekdays"] = new SelectList(lstWeekdays, "Value", "Text", lstWeekdays.First());
            #endregion

            #region " Bind Delete Dropdown list "
            List<SelectListItem> lstDelete = new List<SelectListItem>();
            SelectListItem Item1 = new SelectListItem();
            Item1.Text = "No";
            Item1.Value = "false";
            lstDelete.Add(Item1);

            SelectListItem Item2 = new SelectListItem();
            Item2.Text = "Yes";
            Item2.Value = "true";
            lstDelete.Add(Item2);

            TempData["lstDelete"] = new SelectList(lstDelete, "Value", "Text", lstDelete.First());
            #endregion

            TempData["DeleteConfirmationMsg"] = Common.objCached.IntegrationDeleteConfirmationMsg;
            TempData["InActiveConfirmationMsg"] = Common.objCached.IntegrationInActiveConfirmationMsg;
        }

        /// <summary>
        /// Populate Time options combo
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult populateTimeOptions()
        {
            List<SelectListItem> lst24Hours = GetHoursList();
            return Json(new SelectList(lst24Hours, "Value", "Text", lst24Hours.First()), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Populate Weekdays combo
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult populateWeekDayOptions()
        {
            List<SelectListItem> lstWeekdays = GetWeekDaysList();
            return Json(new SelectList(lstWeekdays, "Value", "Text", lstWeekdays.First()), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Weekdays list
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetWeekDaysList()
        {
            List<SelectListItem> lstWeekdays = new List<SelectListItem>();
            foreach (var item in Enum.GetValues(typeof(DayOfWeek)))
            {
                SelectListItem objTime = new SelectListItem();
                objTime.Text = item.ToString();
                objTime.Value = item.ToString();
                lstWeekdays.Add(objTime);
            }
            return lstWeekdays;
        }

        /// <summary>
        /// Get Hours list
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetHoursList()
        {
            List<SelectListItem> lst24Hours = new List<SelectListItem>();
            DateTime dtToday = DateTime.Today;
            DateTime dtTomorrow = DateTime.Today.AddDays(1);

            while (dtToday < dtTomorrow)
            {
                SelectListItem objTime = new SelectListItem();
                objTime.Text = dtToday.ToString("hh:00 tt");
                objTime.Value = dtToday.ToString("hh:00 tt");
                lst24Hours.Add(objTime);
                dtToday = dtToday.AddHours(1);
            }
            return lst24Hours;
        }


        /// <summary>
        /// Generate view from passed "form".
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IntegrationModel reCreateView(IntegrationModel form)
        {
            form.IntegrationTypeAttributes = GetIntegrationTypeAttributesModelById(form.IntegrationTypeId);

            IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
            var integrationTypeObj = GetIntegrationTypeById(form.IntegrationTypeId);
            objIntegrationTypeModel.Title = integrationTypeObj.Title;
            objIntegrationTypeModel.Code = integrationTypeObj.Code;
            form.IntegrationType = objIntegrationTypeModel;

            populateSyncFreqData();

            return form;
        }

        #endregion

        #region Update Integration

        /// <summary>
        /// Populate existing integration service.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="TypeId"></param>
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]  // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        //[HttpPost]
        public ActionResult editIntegration(int id = 0, int TypeId = 0)
        {
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            int IntegrationTypeId = 0;
            var record = (dynamic)null;
            IntegrationModel objView = new IntegrationModel();
            int clientId = Sessions.User.CID;
            string strPermissionCode_MQL = Enums.ClientIntegrationPermissionCode.MQL.ToString();
            bool IsMQLShow = false;
            bool IsPermission = false;
            if (id == 0)
            {
                objView.IntegrationTypeAttributes = GetIntegrationTypeAttributesModelById(TypeId);
                IntegrationTypeId = TypeId;
                IsPermission = true;
                ViewBag.IntegrationPermission = string.Empty;
            }
            else
            {
                record = db.IntegrationInstances
                                    .Where(ii => ii.IsDeleted.Equals(false) && ii.ClientId == Sessions.User.CID && ii.IntegrationInstanceId == id)
                                    .Select(ii => ii).FirstOrDefault();
                if (record != null)
                {
                    IsPermission = true;
                    objView.Instance = record.Instance;
                    objView.Username = record.Username;
                    objView.Password = Common.Decrypt(record.Password);
                    objView.IntegrationInstanceId = record.IntegrationInstanceId;
                    objView.IntegrationTypeId = record.IntegrationTypeId;
                    objView.IsActive = record.IsActive;
                    objView.IsImportActuals = record.IsImportActuals;

                    IntegrationTypeId = record.IntegrationTypeId;

                    var recordSync = db.SyncFrequencies
                                            .Where(freq => freq.IntegrationInstanceId == id && freq.IntegrationInstance.ClientId == Sessions.User.CID)
                                            .Select(freq => freq).FirstOrDefault();

                    SyncFrequencyModel objSync = new SyncFrequencyModel();
                    if (recordSync != null)
                    {
                        objSync.Day = !string.IsNullOrEmpty(recordSync.Day) ? recordSync.Day : string.Empty;
                        objSync.DayofWeek = !string.IsNullOrEmpty(recordSync.DayofWeek) ? recordSync.DayofWeek : string.Empty;
                        objSync.Frequency = recordSync.Frequency;

                        // Set Time data to SyncFrequencyModel Object.
                        if (recordSync.Time.HasValue)
                        {
                            if (recordSync.Time.Value.Hours > 12)
                                objSync.Time = recordSync.Time.Value.Hours.ToString().PadLeft(2, '0') + ":00 " + "PM";
                            else
                                objSync.Time = recordSync.Time.Value.Hours.ToString().PadLeft(2, '0') + ":00 " + "AM";
                        }
                        objSync.IntegrationInstanceId = recordSync.IntegrationInstanceId;
                    }
                    objView.SyncFrequency = objSync;

                    var recordAttribute = db.IntegrationInstance_Attribute
                                        .Where(attr => attr.IntegrationTypeAttributeId == attr.IntegrationTypeAttribute.IntegrationTypeAttributeId && attr.IntegrationInstanceId == id && attr.IntegrationInstance.ClientId == Sessions.User.CID)
                                        .Select(attr => attr).ToList();

                    //// Add IntegrationType Attributes data to List.
                    List<IntegrationTypeAttributeModel> lstObjIntegrationTypeAttributeModel = new List<IntegrationTypeAttributeModel>();
                    foreach (var item in recordAttribute)
                    {
                        // Add By Nishant Sheth  
                        // Desc :: Add Host details for marketo instance
                        if (item.IntegrationTypeAttribute.Attribute == Enums.IntegrationTypeAttribute.Host.ToString())
                        {
                            string Host = item.Value.ToLower();
                            string[] HostSplit = Host.Split(new string[] { "/rest" }, StringSplitOptions.None);
                            if (HostSplit.Count() > 0)
                            {
                                Host = HostSplit[0];
                            }
                            Host = Host.TrimEnd('/');
                            item.Value = Host;
                        }
                        // End By Nishant Sheth
                        IntegrationTypeAttributeModel objIntegrationTypeAttributeModel = new IntegrationTypeAttributeModel();
                        objIntegrationTypeAttributeModel.Attribute = item.IntegrationTypeAttribute.Attribute;
                        objIntegrationTypeAttributeModel.AttributeType = item.IntegrationTypeAttribute.AttributeType;
                        objIntegrationTypeAttributeModel.IntegrationTypeAttributeId = item.IntegrationTypeAttribute.IntegrationTypeAttributeId;
                        objIntegrationTypeAttributeModel.IntegrationTypeId = item.IntegrationTypeAttribute.IntegrationTypeId;
                        objIntegrationTypeAttributeModel.Value = item.Value;
                        lstObjIntegrationTypeAttributeModel.Add(objIntegrationTypeAttributeModel);
                    }

                    objView.IntegrationTypeAttributes = lstObjIntegrationTypeAttributeModel;
                    ViewBag.IntegrationPermission = string.Empty;
                }
                else
                {
                    objView.IntegrationTypeAttributes = GetIntegrationTypeAttributesModelById(TypeId);
                    IntegrationTypeId = TypeId;
                    ViewBag.IntegrationPermission = "You do not have permission to view this integration instance";
                }
            }

            if (IsPermission)
            {
                if (db.Client_Integration_Permission.Any(intPermission => (intPermission.ClientId.Equals(clientId)) && (intPermission.IntegrationTypeId.Equals(IntegrationTypeId)) && (intPermission.PermissionCode.ToUpper().Equals(strPermissionCode_MQL.ToUpper()))))
                    IsMQLShow = true;

                //// Add IntegrationType data to List.
                IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
                var integrationTypeObj = GetIntegrationTypeById(IntegrationTypeId);
                objIntegrationTypeModel.Title = integrationTypeObj.Title;
                objIntegrationTypeModel.Code = integrationTypeObj.Code;
                objView.IntegrationType = objIntegrationTypeModel;

                objView.IntegrationTypeId = IntegrationTypeId;
                ViewBag.integrationTypeId = IntegrationTypeId;

                //// Flag to check whether MQL field show or not at Edit or View mode based on clientID.
                ViewBag.IsMQLShow = IsMQLShow;

                populateSyncFreqData();
                if (id > 0)
                {
                    objView.GameplanDataTypeModelList = GetGameplanDataTypeList(id);   // Added by Sohel Pathan on 05/08/2014 for PL ticket #656 and #681
                    objView.CustomReadOnlyDataModelList = GetPlanCustomGetFields(id);   //Added by Brad Gray 26 March 2016 for PL #2084
                    objView.ExternalServer = GetExternalServerModelByInstanceId(id);

                    // Dharmraj Start : #658: Integration - UI - Pulling Revenue - Salesforce.com
                    objView.GameplanDataTypePullModelList = GetGameplanDataTypePullListClosedDeal(id);
                    // Dharmraj End : #658: Integration - UI - Pulling Revenue - Salesforce.com
                    // Dharmraj Start : #680: Integration - UI - Pull responses from Salesforce
                    if (objIntegrationTypeModel.Code.Equals(Enums.IntegrationType.Salesforce.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        //Modified by Rahul Shah on 26/02/2016 for PL #2017
                        objView.GameplanDataTypePullRevenueModelList = GetGameplanDataTypePullingList(id, Enums.IntegrationType.Salesforce.ToString(), IsMQLShow);
                        if (objView.GameplanDataTypePullRevenueModelList != null && objView.GameplanDataTypePullRevenueModelList.Count > 0 && IsMQLShow)
                        {
                            objView.GameplanDataTypePullMQLModelList = objView.GameplanDataTypePullRevenueModelList.Where(temp => temp.Type == Enums.GameplanDatatypePullType.MQL.ToString()).ToList();
                            objView.GameplanDataTypePullRevenueModelList = objView.GameplanDataTypePullRevenueModelList.Where(temp => temp.Type == Enums.GameplanDatatypePullType.INQ.ToString()).ToList();
                        }

                    }
                    // Dharmraj End : #680: Integration - UI - Pull responses from Salesforce
                    //// Start - Added by Sohel Pathan on 22/12/2014 for PL ticket #1061
                    else if (objIntegrationTypeModel.Code.Equals(Enums.IntegrationType.Eloqua.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        objView.GameplanDataTypePullMQLModelList = GetGameplanDataTypePullingList(id, Enums.IntegrationType.Eloqua.ToString());
                    }
                    //// End - Added by Sohel Pathan on 22/12/2014 for PL ticket #1061
                }
            }
            //// Check  whether MQL permission exist or not for this clientID. If record exist then Display MQL tab or not.
            return View("edit", objView);
        }

        /// <summary>
        /// Update existing integration service
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]    // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult edit(IntegrationModel form)
        {

            if (!TestIntegrationCredentialsWithForm(form))
            {
                TempData["ErrorMessage"] = Common.objCached.TestIntegrationFail;
                form = reCreateView(form);
                return View(form);
            }
            try
            {
                // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
                ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
                ViewBag.integrationTypeId = form.IntegrationTypeId;

                var isDuplicate = db.IntegrationInstances.Where(_intgrt => _intgrt.Instance == form.Instance && _intgrt.ClientId == Sessions.User.CID && _intgrt.IntegrationType.IntegrationTypeId == form.IntegrationTypeId
                    && _intgrt.IntegrationInstanceId != form.IntegrationInstanceId).Any();

                if (isDuplicate)
                {
                    TempData["ErrorMessage"] = Common.objCached.IntegrationDuplicate;
                    form = reCreateView(form);
                    return View(form);
                }

                bool IntegrationRemoved = true;
                int SyncFrequenciesCount = 0, IntegrationInstancesCount = 0;

                using (var scope = new TransactionScope())
                {
                    // update data
                    IntegrationInstance objIntegrationInstance = db.IntegrationInstances.Where(_intgrt => _intgrt.IntegrationInstanceId == form.IntegrationInstanceId && _intgrt.IsDeleted.Equals(false) &&
                        _intgrt.ClientId == Sessions.User.CID).FirstOrDefault();

                    if (objIntegrationInstance != null)
                    {
                        //// Update IntergrationInstance data to Table.
                        objIntegrationInstance.ClientId = Sessions.User.CID;
                        objIntegrationInstance.ModifiedBy = Sessions.User.ID;
                        objIntegrationInstance.ModifiedDate = DateTime.Now;
                        objIntegrationInstance.Instance = form.Instance;
                        objIntegrationInstance.IsDeleted = form.IsDeleted;
                        objIntegrationInstance.IsImportActuals = form.IsImportActuals;
                        objIntegrationInstance.IsActive = form.IsActive;
                        objIntegrationInstance.Password = Common.Encrypt(form.Password);
                        objIntegrationInstance.Username = form.Username;
                        db.Entry(objIntegrationInstance).State = System.Data.EntityState.Modified;
                        IntegrationInstancesCount = db.SaveChanges();

                        //// Add SyncFrequency data to List.
                        SyncFrequency objSyncFrequency = db.SyncFrequencies.Where(freq => freq.IntegrationInstanceId == form.IntegrationInstanceId && freq.IntegrationInstance.IntegrationInstanceId == form.IntegrationInstanceId).FirstOrDefault();
                        if (objSyncFrequency != null)
                        {
                            //// Handle Time,DayofWeek,Day fields to Form based on Frequency.
                            objSyncFrequency.Frequency = form.SyncFrequency.Frequency;
                            if (form.SyncFrequency.Frequency == SyncFrequencys.Hourly.ToString())
                            {
                                objSyncFrequency.Time = null;
                                objSyncFrequency.DayofWeek = null;
                                objSyncFrequency.Day = null;

                                //// Handle NextSyncDate based on Frequency.
                                DateTime currentDateTime = DateTime.Now.AddHours(1);
                                objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0);
                            }
                            else if (form.SyncFrequency.Frequency == SyncFrequencys.Daily.ToString())
                            {
                                if (form.SyncFrequency.Time.Length == 8)
                                {
                                    int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                    if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.PM.ToString() && hour != 12)
                                        hour = hour + 12;
                                    objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                }
                                objSyncFrequency.DayofWeek = null;
                                objSyncFrequency.Day = null;

                                //// Handle NextSyncDate based on Frequency.
                                DateTime currentDateTime = DateTime.Now.AddDays(1);
                                TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                                objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, time.Hours, time.Minutes, time.Seconds);
                            }
                            else if (form.SyncFrequency.Frequency == SyncFrequencys.Weekly.ToString())
                            {
                                if (form.SyncFrequency.Time.Length == 8)
                                {
                                    int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                    if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.PM.ToString() && hour != 12)
                                        hour = hour + 12;
                                    objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                }
                                objSyncFrequency.Day = null;
                                objSyncFrequency.DayofWeek = form.SyncFrequency.DayofWeek;
                                TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                                //// Handle NextSyncDate based on Frequency.
                                DateTime nextDate = GetNextDateForDay(DateTime.Now, (DayOfWeek)Enum.Parse(typeof(DayOfWeek), objSyncFrequency.DayofWeek), time);
                                objSyncFrequency.NextSyncDate = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, time.Hours, time.Minutes, time.Seconds);
                            }
                            else if (form.SyncFrequency.Frequency == SyncFrequencys.Monthly.ToString())
                            {
                                if (form.SyncFrequency.Time.Length == 8)
                                {
                                    int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                    if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.PM.ToString() && hour != 12)
                                        hour = hour + 12;
                                    objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                }
                                objSyncFrequency.DayofWeek = null;
                                objSyncFrequency.Day = form.SyncFrequency.Day;

                                //// Handle NextSyncDate based on Frequency.
                                DateTime currentDateTime = DateTime.Now;
                                if (Convert.ToInt32(objSyncFrequency.Day) <= currentDateTime.Day)
                                {
                                    currentDateTime = currentDateTime.AddMonths(1);
                                }
                                TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                                objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, Convert.ToInt32(objSyncFrequency.Day), time.Hours, time.Minutes, time.Seconds);
                            }

                            db.Entry(objSyncFrequency).State = System.Data.EntityState.Modified;
                            SyncFrequenciesCount = db.SaveChanges();
                        }

                        //// Add Integration Type Attributes.
                        if (form.IntegrationTypeAttributes != null)
                        {
                            IntegrationInstance_Attribute objIntegrationInstance_Attribute = null;
                            foreach (var item in form.IntegrationTypeAttributes)
                            {
                                objIntegrationInstance_Attribute = db.IntegrationInstance_Attribute.Where(_attr => _attr.IntegrationInstanceId == form.IntegrationInstanceId
                                        && _attr.IntegrationTypeAttributeId == item.IntegrationTypeAttributeId && _attr.IntegrationInstance.IntegrationTypeId == form.IntegrationTypeId).FirstOrDefault();

                                if (objIntegrationInstance_Attribute != null)
                                {
                                    objIntegrationInstance_Attribute.Value = item.Value;
                                    db.Entry(objIntegrationInstance_Attribute).State = System.Data.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    if (Convert.ToString(form.IsDeleted).ToLower() == "true")
                    {
                        IntegrationRemoved = Common.DeleteIntegrationInstance(form.IntegrationInstanceId, true);
                    }

                    // Status changed from Active to InActive, So remove all the integration dependency with Models.
                    if (form.IsActiveStatuChanged == true && form.IsActive == false)
                    {
                        // Remove association of Integrartion from Plan
                        List<Model> ModelList = db.Models.Where(_mdl => _mdl.IsDeleted.Equals(false)).ToList();

                        // Get Salesforce Model list and remove respective record from that table.
                        List<Model> salModelList = ModelList.Where(_mdl => (_mdl.IntegrationInstanceId == form.IntegrationInstanceId)).ToList();
                        salModelList.ForEach(_mdl => { _mdl.IntegrationInstanceId = null; _mdl.ModifiedDate = DateTime.Now; _mdl.ModifiedBy = Sessions.User.ID; db.Entry(_mdl).State = EntityState.Modified; });

                        // Get Eloqua Model list and remove respective record from that table.
                        List<Model> elqModelList = ModelList.Where(_mdl => (_mdl.IntegrationInstanceEloquaId == form.IntegrationInstanceId)).ToList();
                        elqModelList.ForEach(_mdl => { _mdl.IntegrationInstanceEloquaId = null; _mdl.ModifiedDate = DateTime.Now; _mdl.ModifiedBy = Sessions.User.ID; db.Entry(_mdl).State = EntityState.Modified; });
                        db.SaveChanges();
                    }
                    scope.Complete();
                }

                if (IntegrationInstancesCount > 0 && SyncFrequenciesCount > 0 && IntegrationRemoved != false)
                {
                    if (Convert.ToString(form.IsDeleted).ToLower() == "true")
                    {
                        TempData["SuccessMessage"] = Common.objCached.IntegrationDeleted;
                        TempData["ErrorMessage"] = "";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["SuccessMessage"] = Common.objCached.IntegrationEdited;
                        TempData["ErrorMessage"] = "";
                        form = reCreateView(form);
                        return View(form);
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                    form = reCreateView(form);
                    return View(form);
                }
            }
            catch
            {
                TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                form = reCreateView(form);
                return View(form);
            }
        }

        #endregion

        /// <summary>
        /// Sync data to external service based on Integration selection
        /// </summary>
        /// <param name="id">Integration Instance Id</param>
        /// <param name="UserId">user id of the logged in user</param>
        /// <returns>returns json result object with sync status flag and sync timestamp</returns>
        public JsonResult SyncNow(int id, string UserId = "")
        {
            //// Check whether UserId is loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            string currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            try
            {
                Message.SaveIntegrationInstanceLogDetails(id, null, Enums.MessageOperation.None, currentMethodName, Enums.MessageLabel.Success, "create external integration");
                ExternalIntegration externalIntegration = new ExternalIntegration(id, Sessions.ApplicationId, Sessions.User.ID);

                Message.SaveIntegrationInstanceLogDetails(id, null, Enums.MessageOperation.Start, currentMethodName, Enums.MessageLabel.Success, "Sync Start");
                externalIntegration.Sync();
                Message.SaveIntegrationInstanceLogDetails(id, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Sync End");

                IntegrationInstance integrationInstance = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId.Equals(id));

                //// Return Status and lastSync value in json format.
                string status = integrationInstance.LastSyncStatus;
                if (integrationInstance.LastSyncDate.HasValue)
                {
                    #region "Get ForceSync User"
                    BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                    string strForceUser = string.Empty;
                    if (integrationInstance.ForceSyncUser != 0)
                    {
                        var ForceUser = objBDSServiceClient.GetTeamMemberDetailsEx(integrationInstance.ForceSyncUser, Sessions.ApplicationId);
                        if (ForceUser != null)
                        {
                            strForceUser = ForceUser.FirstName + " " + ForceUser.LastName;
                        }
                    }
                    else
                        strForceUser = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                    #endregion

                    return Json(new { status = integrationInstance.LastSyncStatus, lastSync = Convert.ToDateTime(integrationInstance.LastSyncDate).ToString(DateFormat), forceSyncUser = strForceUser }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string exMessage = ex.InnerException != null ? ex.InnerException.ToString() : ex.Message;
                Message.SaveIntegrationInstanceLogDetails(id, null, Enums.MessageOperation.End, currentMethodName, Enums.MessageLabel.Success, "Error occurred while syncing data to external service: " + exMessage);
                return Json(new { status = "Error", lastSync = DateTime.Now.ToString(DateFormat) }, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        /// <summary>
        /// Authenticate integration credentials for external service.
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult TestIntegration(IntegrationModel form)
        {
            if (TestIntegrationCredentialsWithForm(form))
            {
                return Json(new { status = 1, SuccessMessage = Common.objCached.TestIntegrationSuccess }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = 0, ErrorMessage = Common.objCached.TestIntegrationFail }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Action to Test Integration Credentials of Form and return bool value.
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public bool TestIntegrationCredentialsWithForm(IntegrationModel form)
        {
            bool isAuthenticated = false;

            if (form.Instance != "" && form.Username != "" && form.Password != "")
            {
                if (form.IntegrationType.Code.Equals(Integration.Helper.Enums.IntegrationType.Eloqua.ToString()))
                {
                    //Added By Komal Rawal for ticket #1118
                    //// Check Credentials whether Authenticate or not for Eloqua Client.
                    if (form.IntegrationTypeAttributes.Count > 0)
                    {
                        List<int> attributeIds = form.IntegrationTypeAttributes.Select(attr => attr.IntegrationTypeAttributeId).ToList();
                        List<IntegrationTypeAttribute> integrationTypeAttributes = db.IntegrationTypeAttributes.Where(attr => attributeIds.Contains(attr.IntegrationTypeAttributeId)).ToList();
                        string companyname, eloquaClientId, eloquaClientSecret;
                        companyname = eloquaClientId = eloquaClientSecret = string.Empty;

                        //// Get ConsumerKey based on Attributes.
                        foreach (IntegrationTypeAttribute integrationTypeAttribute in integrationTypeAttributes)
                        {
                            if (integrationTypeAttribute.Attribute.Equals("Company Name"))
                            {
                                companyname = form.IntegrationTypeAttributes.FirstOrDefault(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                            }
                            if (integrationTypeAttribute.Attribute.ToUpper().Equals(Common.eloquaClientIdLabel.ToUpper()))
                            {
                                eloquaClientId = form.IntegrationTypeAttributes.FirstOrDefault(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                            }
                            if (integrationTypeAttribute.Attribute.ToUpper().Equals((Common.eloquaClientSecretLabel.ToUpper())))
                            {
                                eloquaClientSecret = form.IntegrationTypeAttributes.FirstOrDefault(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                            }
                        }
                        //eloquaClientId = form.IntegrationTypeAttributes.FirstOrDefault(attr => attr.Attribute.ToUpper().Equals(Common.eloquaClientIdLabel.ToUpper())).Value;
                        //eloquaClientSecret = form.IntegrationTypeAttributes.FirstOrDefault(attr => attr.Attribute.ToUpper().Equals(Common.eloquaClientSecretLabel.ToUpper())).Value;
                        if (!string.IsNullOrWhiteSpace(companyname))
                        {
                            //// Check Credentials whether Authenticate or not for Eloqua Client.
                            IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient();
                            integrationEloquaClient._integrationInstanceId = form.IntegrationInstanceId;
                            integrationEloquaClient._companyName = companyname;// "TechnologyPartnerBulldog";
                            integrationEloquaClient._username = form.Username;// "Brij.Bhavsar";
                            integrationEloquaClient._password = form.Password;//"Brij1234";
                            RevenuePlanner.Models.IntegrationType integrationType = GetIntegrationTypeById(form.IntegrationTypeId);
                            integrationEloquaClient._apiURL = integrationType.APIURL;
                            integrationEloquaClient._apiVersion = integrationType.APIVersion;
                            integrationEloquaClient._eloquaClientID = eloquaClientId;
                            integrationEloquaClient._ClientSecret = eloquaClientSecret;
                            integrationEloquaClient.Authenticate();
                            isAuthenticated = integrationEloquaClient.IsAuthenticated;
                        }

                    }
                }

                //Added by Brad Gray for ticket #1365
                else if (form.IntegrationType.Code.Equals(Integration.Helper.Enums.IntegrationType.WorkFront.ToString()))
                {
                    if (form.IntegrationTypeAttributes.Count > 0)
                    {
                        List<int> attributeIds = form.IntegrationTypeAttributes.Select(attr => attr.IntegrationTypeAttributeId).ToList();
                        List<IntegrationTypeAttribute> integrationTypeAttributes = db.IntegrationTypeAttributes.Where(attr => attributeIds.Contains(attr.IntegrationTypeAttributeId)).ToList();
                        string companyname = string.Empty;


                        //// Get ConsumerKey based on Attributes.
                        foreach (IntegrationTypeAttribute integrationTypeAttribute in integrationTypeAttributes)
                        {
                            if (integrationTypeAttribute.Attribute.Equals("Company Name"))
                            {
                                companyname = form.IntegrationTypeAttributes.FirstOrDefault(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(companyname))
                        {
                            RevenuePlanner.Models.IntegrationType integrationType = GetIntegrationTypeById(form.IntegrationTypeId);
                            //Creates, authenticates and logs user in
                            IntegrationWorkFrontSession integrationWorkFrontClient = new IntegrationWorkFrontSession(companyname, integrationType.APIURL, form.Username, form.Password);
                            isAuthenticated = integrationWorkFrontClient.isAuthenticated();
                        }

                    }
                }

                else if (form.IntegrationType.Code.Equals(Integration.Helper.Enums.IntegrationType.Salesforce.ToString()) && form.IntegrationTypeAttributes != null)
                {
                    //// Check Credentials whether Authenticate or not for Salesforce Client.
                    if (form.IntegrationTypeAttributes.Count > 0)
                    {
                        List<int> attributeIds = form.IntegrationTypeAttributes.Select(attr => attr.IntegrationTypeAttributeId).ToList();
                        List<IntegrationTypeAttribute> integrationTypeAttributes = db.IntegrationTypeAttributes.Where(attr => attributeIds.Contains(attr.IntegrationTypeAttributeId)).ToList();
                        string consumerKey = string.Empty;
                        string consumerSecret = string.Empty;
                        string securityToken = string.Empty;

                        //// Get ConsumerKey based on Attributes.
                        foreach (IntegrationTypeAttribute integrationTypeAttribute in integrationTypeAttributes)
                        {
                            if (integrationTypeAttribute.Attribute.Equals("ConsumerKey"))
                            {
                                consumerKey = form.IntegrationTypeAttributes.FirstOrDefault(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                            }
                            else if (integrationTypeAttribute.Attribute.Equals("ConsumerSecret"))
                            {
                                consumerSecret = form.IntegrationTypeAttributes.FirstOrDefault(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                            }
                            else if (integrationTypeAttribute.Attribute.Equals("SecurityToken"))
                            {
                                securityToken = form.IntegrationTypeAttributes.FirstOrDefault(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(consumerKey) && !string.IsNullOrWhiteSpace(consumerSecret) && !string.IsNullOrWhiteSpace(securityToken))
                        {
                            IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient();
                            integrationSalesforceClient._username = form.Username;
                            integrationSalesforceClient._password = form.Password;
                            integrationSalesforceClient._consumerKey = consumerKey;
                            integrationSalesforceClient._consumerSecret = consumerSecret;
                            integrationSalesforceClient._securityToken = securityToken;
                            integrationSalesforceClient._apiURL = GetIntegrationTypeById(form.IntegrationTypeId).APIURL;
                            integrationSalesforceClient.Authenticate();
                            isAuthenticated = integrationSalesforceClient.IsAuthenticated;
                        }
                    }
                }
                else if (form.IntegrationType.Code.Equals(Integration.Helper.Enums.IntegrationType.Marketo.ToString()) && form.IntegrationTypeAttributes != null)
                {
                    //Added by Rahul shah on 16/05/2016 for PL#2184. to check Marketo Authentication.
                    if (form.IntegrationTypeAttributes.Count > 0)
                    {
                        List<int> attributeIds = form.IntegrationTypeAttributes.Select(attr => attr.IntegrationTypeAttributeId).ToList();
                        List<IntegrationTypeAttribute> integrationTypeAttributes = db.IntegrationTypeAttributes.Where(attr => attributeIds.Contains(attr.IntegrationTypeAttributeId)).ToList();
                        string marketoClientId, marketoClientSecret, marketoHost;
                        marketoClientId = marketoClientSecret = marketoHost = string.Empty;
                        foreach (IntegrationTypeAttribute integrationTypeAttribute in integrationTypeAttributes)
                        {
                            if (integrationTypeAttribute.Attribute.ToUpper().Equals(Common.eloquaClientIdLabel.ToUpper()))
                            {
                                marketoClientId = form.IntegrationTypeAttributes.FirstOrDefault(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                            }
                            if (integrationTypeAttribute.Attribute.ToUpper().Equals((Common.eloquaClientSecretLabel.ToUpper())))
                            {
                                marketoClientSecret = form.IntegrationTypeAttributes.FirstOrDefault(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                            }
                            if (integrationTypeAttribute.Attribute.ToUpper().Equals((Enums.IntegrationTypeAttribute.Host.ToString().ToUpper())))
                            {
                                marketoHost = form.IntegrationTypeAttributes.FirstOrDefault(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                                if (!string.IsNullOrEmpty(marketoHost))
                                {
                                    string Host = marketoHost.ToLower();
                                    string[] HostSplit = Host.Split(new string[] { "/rest" }, StringSplitOptions.None);
                                    if (HostSplit.Count() > 0)
                                    {
                                        Host = HostSplit[0];
                                    }
                                    Host = Host.TrimEnd('/');
                                    marketoHost = Host;
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(marketoClientId) && !string.IsNullOrWhiteSpace(marketoClientSecret))
                        {
                            ApiIntegration AI = new ApiIntegration();
                            //AI._host = GetIntegrationTypeById(form.IntegrationTypeId).APIURL; // Commented By Nishant Sheth
                            AI._host = marketoHost;
                            AI._clientid = marketoClientId;
                            AI._clientsecret = marketoClientSecret;
                            AI.AuthenticateforMarketo();
                            isAuthenticated = AI.IsAuthenticated;

                        }
                    }
                }
            }

            return isAuthenticated;
        }

        #region Map Data Types

        /// <summary>
        /// Map External Service - Gameplan Data Types (Fields)
        /// </summary>
        /// <param name="id"></param>
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]  // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult MapDataTypes(int id)
        {
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

            try
            {
                ExternalIntegration objEx = new ExternalIntegration(id, Sessions.ApplicationId);
                List<string> ExternalFields = objEx.GetTargetDataMember();
                if (ExternalFields == null)
                {
                    ExternalFields = new List<string>();
                }
                ViewData["ExternalFieldList"] = ExternalFields.OrderBy(list => list, new AlphaNumericComparer());
                ViewBag.IntegrationInstanceId = id;

                //// Get IntegrationTypeName based on IntegrationInstanceId
                string integrationTypeName = (from intgrtn in db.IntegrationInstances
                                              join intgrtnType in db.IntegrationTypes on intgrtn.IntegrationTypeId equals intgrtnType.IntegrationTypeId
                                              where intgrtn.IsDeleted == false && intgrtnType.IsDeleted == false && intgrtn.IntegrationInstanceId == id
                                              select intgrtnType.Title).FirstOrDefault();

                if (string.IsNullOrEmpty(integrationTypeName))
                    ViewBag.IntegrationTypeName = "";
                else
                    ViewBag.IntegrationTypeName = integrationTypeName;

                //// Start - Added by :- Sohel Pathan on 28/05/2014 for PL #494 filter gameplan datatype by client id 
                var lstStages = db.Stages.Where(stage => stage.IsDeleted == false && stage.ClientId == Sessions.User.CID).Select(stage => new { Code = stage.Code, Title = stage.Title }).ToList();
                var listStageCode = lstStages.Select(stage => stage.Code).ToList();

                //// Get GamePlanDataType Stage Zero data.
                List<GameplanDataTypeModel> listGameplanDataTypeStageZero = new List<GameplanDataTypeModel>();
                listGameplanDataTypeStageZero = (from intgrtn in db.IntegrationInstances
                                                 join gmpln in db.GameplanDataTypes on intgrtn.IntegrationTypeId equals gmpln.IntegrationTypeId
                                                 join intMap in db.IntegrationInstanceDataTypeMappings on gmpln.GameplanDataTypeId equals intMap.GameplanDataTypeId into mapping
                                                 from _map in mapping.Where(map => map.IntegrationInstanceId == id).DefaultIfEmpty()
                                                 where intgrtn.IntegrationInstanceId == id && gmpln.IsDeleted == false
                                                 select new GameplanDataTypeModel
                                                 {
                                                     GameplanDataTypeId = gmpln.GameplanDataTypeId,
                                                     IntegrationTypeId = gmpln.IntegrationTypeId,
                                                     TableName = gmpln.TableName,
                                                     ActualFieldName = gmpln.ActualFieldName,
                                                     DisplayFieldName = gmpln.DisplayFieldName,
                                                     IsGet = gmpln.IsGet,
                                                     IntegrationInstanceDataTypeMappingId = _map.IntegrationInstanceDataTypeMappingId,
                                                     IntegrationInstanceId = intgrtn.IntegrationInstanceId,
                                                     TargetDataType = _map.TargetDataType
                                                 }).ToList();

                //// Get GamePlanDataType Stage One data.
                List<GameplanDataTypeModel> listGameplanDataTypeStageOne = new List<GameplanDataTypeModel>();
                listGameplanDataTypeStageOne = listGameplanDataTypeStageZero.Where(gpDataType => listStageCode.Contains(gpDataType.ActualFieldName)).ToList();

                foreach (var item in listGameplanDataTypeStageOne)
                {
                    item.DisplayFieldName = item.DisplayFieldName + "[" + lstStages.Where(stage => stage.Code == item.ActualFieldName).Select(stage => stage.Title).FirstOrDefault() + "]";
                }

                listGameplanDataTypeStageZero.AddRange(listGameplanDataTypeStageOne);

                if (listGameplanDataTypeStageZero != null && listGameplanDataTypeStageZero.Count > 0)
                {
                    TempData["TargetFieldInvalidMsg"] = Common.objCached.TargetFieldInvalidMsg;
                    return View(listGameplanDataTypeStageZero.OrderBy(map => map.DisplayFieldName).ToList());
                }
                //// End - Added by :- Sohel Pathan on 28/05/2014 for PL #494 filter gameplan datatype by client id
                else
                {
                    TempData["TargetFieldInvalidMsg"] = "";
                    TempData["ErrorMessage"] = Common.objCached.DataTypeMappingNotConfigured;
                    return RedirectToAction("Edit", new { id = id });
                }
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = Common.objCached.DataTypeMappingNotConfigured + e.Message;
                ErrorSignal.FromCurrentContext().Raise(e);
                return RedirectToAction("Edit", new { id = id });
            }
        }

        /// <summary>
        /// Map External Service - Gameplan Data Types (Fields)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]    // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult MapDataTypes(int id, IList<GameplanDataTypeModel> form)
        {
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        //// Delete data from IntegrationInstanceDataTypeMapping table based on List of Ids.
                        List<int> lstIds = mrp.IntegrationInstanceDataTypeMappings.Where(_map => _map.IntegrationInstanceId == id).Select(_map => _map.IntegrationInstanceDataTypeMappingId).ToList();
                        if (lstIds != null && lstIds.Count > 0)
                        {
                            foreach (int ids in lstIds)
                            {
                                IntegrationInstanceDataTypeMapping obj = mrp.IntegrationInstanceDataTypeMappings.Where(_map => _map.IntegrationInstanceDataTypeMappingId == ids).FirstOrDefault();
                                if (obj != null)
                                {
                                    mrp.Entry(obj).State = EntityState.Deleted;
                                    mrp.SaveChanges();
                                }
                            }
                        }

                        //// Add Integration Instance DataTypeMapping data to IntegrationInstanceDataTypeMapping table.
                        foreach (GameplanDataTypeModel obj in form)
                        {
                            if (!string.IsNullOrEmpty(obj.TargetDataType))
                            {
                                IntegrationInstanceDataTypeMapping objMapping = new IntegrationInstanceDataTypeMapping();
                                int instanceId;
                                int.TryParse(Convert.ToString(obj.IntegrationInstanceId), out instanceId);
                                objMapping.IntegrationInstanceId = instanceId;
                                objMapping.GameplanDataTypeId = obj.GameplanDataTypeId;
                                objMapping.TargetDataType = obj.TargetDataType;
                                objMapping.CreatedDate = DateTime.Now;
                                objMapping.CreatedBy = Sessions.User.ID;
                                mrp.Entry(objMapping).State = EntityState.Added;
                                mrp.SaveChanges();
                            }
                        }
                        scope.Complete();
                    }
                }
                TempData["SuccessMessage"] = Common.objCached.DataTypeMappingSaveSuccess;
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = Common.objCached.ErrorOccured + e.Message;
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return RedirectToAction("MapDataTypes", new { id = id });
        }

        #region Get Gampeplan DataType List
        /// <summary>
        /// Get list of gameplan DataType Mapping list
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>05/08/2014</CreatedDate>
        /// <param name="id">Integration Instance Id</param>
        /// <returns>Returns list of gameplan data type model</returns>
        public IList<GameplanDataTypeModel> GetGameplanDataTypeList(int id)
        {
            List<GameplanDataTypeModel> listGameplanDataTypeStageZero = new List<GameplanDataTypeModel>();
            try
            {
                // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
                ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
                ExternalIntegration objEx = new ExternalIntegration(id, Sessions.ApplicationId);
                List<string> ExternalFields = objEx.GetTargetDataMember();
                if (ExternalFields == null)
                {
                    ExternalFields = new List<string>();
                }
                ViewData["ExternalFieldList"] = ExternalFields.OrderBy(list => list, new AlphaNumericComparer());
            }
            catch
            {
                ViewData["ExternalFieldList"] = new List<string>();
            }
            finally
            {
                listGameplanDataTypeStageZero = GetGameplanDataTypeListFromDB(id);
            }
            return listGameplanDataTypeStageZero;
        }
        #endregion

        #region Get Plan Read-Only Custom field List
        /// <summary>
        /// Get list of read-only custom fields
        /// These fields should not be included in the GameplanDataType list
        /// adapted heavily from GetGameplanDataTypeListFromDB method
        /// added by Brad Gray 26 March 2016, PL#2084
        /// </summary>
        /// Added by Brad Gray on 26 March 2016, Ticket #
        /// <param name="id"></param>
        /// <returns>Returns GameplanDataTypePullModel List</returns>
        public IList<GameplanDataTypeModel> GetPlanCustomGetFields(int id)
        {
            List<GameplanDataTypeModel> listCustGet = new List<GameplanDataTypeModel>();
            string Eloqua = Enums.IntegrationType.Eloqua.ToString();
            string Tactic_EntityType = Enums.EntityType.Tactic.ToString();
            string Campaign_EntityType = Enums.EntityType.Campaign.ToString();
            string Program_EntityType = Enums.EntityType.Program.ToString();
            string Plan_Campaign = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign.ToString();
            string Plan_Campaign_Program = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program.ToString();
            string Plan_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program_Tactic.ToString();

            try
            {
                var integrationType = (from intgrtn in db.IntegrationInstances
                                       join _type in db.IntegrationTypes on intgrtn.IntegrationTypeId equals _type.IntegrationTypeId
                                       where intgrtn.IsDeleted == false && _type.IsDeleted == false && intgrtn.IntegrationInstanceId == id
                                       select new { _type.Title, _type.IntegrationTypeId, _type.Code }).FirstOrDefault();

                string integrationTypeName = string.Empty;
                string integrationTypeCode = string.Empty;
                int IntegrationTypeId = 0;
                if (integrationType != null)
                {
                    integrationTypeName = integrationType.Title;
                    IntegrationTypeId = integrationType.IntegrationTypeId;
                    integrationTypeCode = integrationType.Code;
                }

                listCustGet = (from custm in db.CustomFields
                               join m1 in db.IntegrationInstanceDataTypeMappings on custm.CustomFieldId equals m1.CustomFieldId into mapping
                               from m in mapping.Where(map => map.IntegrationInstanceId == id).DefaultIfEmpty()
                               where custm.IsDeleted == false && custm.ClientId == Sessions.User.CID && custm.IsGet == true &&
                               (integrationTypeCode == Eloqua ? (custm.EntityType == Tactic_EntityType) : 1 == 1)
                               select new GameplanDataTypeModel
                               {
                                   GameplanDataTypeId = custm.CustomFieldId,   // For Custom Fields CustomFieldId is GameplanDataType Id in Mapping
                                   IntegrationTypeId = IntegrationTypeId,
                                   TableName = custm.EntityType == Campaign_EntityType ? Plan_Campaign : (custm.EntityType == Program_EntityType ? Plan_Campaign_Program : (custm.EntityType == Tactic_EntityType ? Plan_Campaign_Program_Tactic : string.Empty)),
                                   ActualFieldName = custm.Name,
                                   DisplayFieldName = custm.Name,
                                   IsGet = custm.IsGet,
                                   IntegrationInstanceDataTypeMappingId = m.IntegrationInstanceDataTypeMappingId,
                                   IntegrationInstanceId = id,
                                   TargetDataType = m.TargetDataType,
                                   IsCustomField = true
                               }).ToList();
            }
            catch
            {
                TempData["DataMappingErrorMessage"] = Common.objCached.ErrorOccured;
            }

            return listCustGet;
        }

        #endregion

        #region Get Gampeplan DataType Pull List
        /// <summary>
        /// Get list of gameplan DataType Pull Mapping list
        /// </summary>
        /// Added by Dharmraj on 8-8-2014, Ticket #658
        /// <param name="id"></param>
        /// <returns>Returns GameplanDataTypePullModel List</returns>
        public IList<GameplanDataTypePullModel> GetGameplanDataTypePullListClosedDeal(int id)
        {
            List<GameplanDataTypePullModel> listGameplanDataTypePullZero = new List<GameplanDataTypePullModel>();
            try
            {
                ExternalIntegration objEx = new ExternalIntegration(id, Sessions.ApplicationId);
                List<Integration.Helper.PullClosedDealModel> lstPullClosedDealModel = new List<Integration.Helper.PullClosedDealModel>();
                lstPullClosedDealModel = objEx.GetTargetDataMemberCloseDeal();
                System.Web.HttpContext.Current.Cache["closedDealsPickList"] = lstPullClosedDealModel;
                List<string> ExternalFieldsCloseDeal = lstPullClosedDealModel.Select(item => item.fieldname).OrderBy(s => s).ToList();
                if (ExternalFieldsCloseDeal == null)
                {
                    ExternalFieldsCloseDeal = new List<string>();
                }
                ViewData["ExternalFieldListPull"] = ExternalFieldsCloseDeal.OrderBy(list => list, new AlphaNumericComparer());
            }
            catch
            {
                ViewData["ExternalFieldListPull"] = new List<string>();
            }
            finally
            {
                listGameplanDataTypePullZero = GetGameplanDataTypePullListFromDB(id, Enums.GameplanDatatypePullType.CW);
                if (listGameplanDataTypePullZero.Any(field => field.Type == "CW" && field.ActualFieldName == "CW") && listGameplanDataTypePullZero.Any(field => field.Type == "CW" && field.ActualFieldName == "Stage"))
                {
                    var stageItem = listGameplanDataTypePullZero.Where(field => field.Type == "CW" && field.ActualFieldName == "Stage").FirstOrDefault();
                    int stageIndex = listGameplanDataTypePullZero.IndexOf(stageItem);
                    var cwItem = listGameplanDataTypePullZero.Where(field => field.Type == "CW" && field.ActualFieldName == "CW").FirstOrDefault();
                    int cwOldIndex = listGameplanDataTypePullZero.IndexOf(cwItem);
                    ViewBag.cwTargetType = cwItem.TargetDataType != null ? cwItem.TargetDataType : string.Empty;
                    listGameplanDataTypePullZero.RemoveAt(cwOldIndex);
                    listGameplanDataTypePullZero.Insert(stageIndex + 1, cwItem);
                }
            }
            return listGameplanDataTypePullZero;
        }

        public JsonResult BindClosedDealsPickList(int instanceId, string stageName = "")
        {
            List<string> pickList = new List<string>();
            List<Integration.Helper.PullClosedDealModel> lstPullClosedDealModel = new List<Integration.Helper.PullClosedDealModel>();
            try
            {
                // Set/get common messages cache
                if (System.Web.HttpContext.Current.Cache["closedDealsPickList"] == null)
                {
                    ExternalIntegration objEx = new ExternalIntegration(instanceId, Sessions.ApplicationId);
                    lstPullClosedDealModel = objEx.GetTargetDataMemberCloseDeal();
                    System.Web.HttpContext.Current.Cache["closedDealsPickList"] = lstPullClosedDealModel;
                    //CacheDependency dependency = new CacheDependency(Common.xmlMsgFilePath);
                    //System.Web.HttpContext.Current.Cache.Insert("closedDealsPickList", );
                }
                else
                {
                    lstPullClosedDealModel = (List<Integration.Helper.PullClosedDealModel>)System.Web.HttpContext.Current.Cache["closedDealsPickList"];
                }
                if (!string.IsNullOrEmpty(stageName) && stageName.ToLower() != "select")
                    pickList = lstPullClosedDealModel.Where(field => field.fieldname == stageName).FirstOrDefault().pickList;
                else
                    pickList = new List<string>();
            }
            catch (Exception)
            {
                throw;
            }
            return Json(pickList, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Get Gampeplan DataType Pull Revenue/MQL List
        /// <summary>
        /// Get list of gameplan DataType Pull Revenue/MQL Mapping list
        /// </summary>
        /// Added by Dharmraj on 13-8-2014, Ticket #680
        /// Modified by Sohel Pathan on 23/12/2014, Ticket #
        /// <param name="id">Integration Instance Id</param>
        /// <param name="integrationType">integration type code</param>
        /// <returns>Returns GameplanDataTypePullModel List</returns>
        public IList<GameplanDataTypePullModel> GetGameplanDataTypePullingList(int id, string integrationType, bool isMQLShow = false)//Modified by Rahul Shah on 26/02/2016 for PL #2017
        {
            List<GameplanDataTypePullModel> listGameplanDataTypePulling = new List<GameplanDataTypePullModel>();
            bool isSalesForce = false;
            try
            {
                ExternalIntegration objEx = new ExternalIntegration(id, Sessions.ApplicationId);
                if (integrationType.Equals(Enums.IntegrationType.Salesforce.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    List<string> lstExternalFieldsPulling = new List<string>();
                    lstExternalFieldsPulling = objEx.GetTargetDataMemberPulling();
                    ViewData["ExternalFieldListPulling"] = lstExternalFieldsPulling.OrderBy(list => list, new AlphaNumericComparer());
                }
                else if (integrationType.Equals(Enums.IntegrationType.Eloqua.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    List<SelectListItem> lstExternalFieldsPulling = new List<SelectListItem>();
                    lstExternalFieldsPulling = Common.GetEloquaContactTargetDataMemberSelectList(id);
                    ViewData["ExternalFieldListPulling"] = lstExternalFieldsPulling.OrderBy(list => list.Text, new AlphaNumericComparer());
                }
            }
            catch
            {
                ViewData["ExternalFieldListPulling"] = new List<string>();
            }
            finally
            {
                if (integrationType.Equals(Enums.IntegrationType.Salesforce.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    isSalesForce = true;
                    listGameplanDataTypePulling = GetGameplanDataTypePullListFromDB(id, Enums.GameplanDatatypePullType.INQ, isSalesForce, isMQLShow);
                }
                else if (integrationType.Equals(Enums.IntegrationType.Eloqua.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    listGameplanDataTypePulling = GetGameplanDataTypePullListFromDB(id, Enums.GameplanDatatypePullType.MQL);
                }
            }
            return listGameplanDataTypePulling;
        }
        #endregion

        #region Function get Gameplan DataTypes list from DB.
        /// <summary>
        /// Get gameplan datatype list from database based on IntegrationInstanceId
        /// </summary>
        /// <param name="id">Integration Instance Id</param>
        /// <returns>returns list of gameplan datatype list with custom fields</returns>
        public List<GameplanDataTypeModel> GetGameplanDataTypeListFromDB(int id)
        {
            try
            {
                ViewBag.IntegrationInstanceId = id;

                var integrationType = (from intgrtn in db.IntegrationInstances
                                       join _type in db.IntegrationTypes on intgrtn.IntegrationTypeId equals _type.IntegrationTypeId
                                       where intgrtn.IsDeleted == false && _type.IsDeleted == false && intgrtn.IntegrationInstanceId == id
                                       select new { _type.Title, _type.IntegrationTypeId, _type.Code }).FirstOrDefault();

                string integrationTypeName = string.Empty;
                string integrationTypeCode = string.Empty;
                int IntegrationTypeId = 0;
                if (integrationType != null)
                {
                    integrationTypeName = integrationType.Title;
                    IntegrationTypeId = integrationType.IntegrationTypeId;
                    integrationTypeCode = integrationType.Code;
                }

                if (string.IsNullOrEmpty(integrationTypeName))
                    ViewBag.IntegrationTypeName = "";
                else
                    ViewBag.IntegrationTypeName = integrationTypeName;

                TempData["TargetFieldInvalidMsg"] = Common.objCached.TargetFieldInvalidMsg;

                //// Start - Added by :- Sohel Pathan on 28/05/2014 for PL #494 filter gameplan datatype by client id 
                #region "Set Enum Variables"
                string Eloqua = Enums.IntegrationType.Eloqua.ToString();
                string Marketo = Enums.IntegrationType.Marketo.ToString(); //Added by Rahul Shah on 13/05/2016 for PL #2184
                string WorkFront = Enums.IntegrationType.WorkFront.ToString();
                string Plan_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program_Tactic.ToString();
                string Plan_Improvement_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Improvement_Campaign_Program_Tactic.ToString();
                string Global = Enums.IntegrantionDataTypeMappingTableName.Global.ToString();
                #endregion

                //// Get GamePlan Stage Zero data.
                List<GameplanDataTypeModel> listGameplanDataTypeStageZero = new List<GameplanDataTypeModel>();
                listGameplanDataTypeStageZero = (from intgrtn in db.IntegrationInstances
                                                 join gmpln in db.GameplanDataTypes on intgrtn.IntegrationTypeId equals gmpln.IntegrationTypeId
                                                 join intMap in db.IntegrationInstanceDataTypeMappings on gmpln.GameplanDataTypeId equals intMap.GameplanDataTypeId into mapping
                                                 from _map in mapping.Where(map => map.IntegrationInstanceId == id).DefaultIfEmpty()
                                                 where intgrtn.IntegrationInstanceId == id && gmpln.IsDeleted == false &&
                                                 (integrationTypeCode == Eloqua ? (gmpln.TableName == Plan_Campaign_Program_Tactic || gmpln.TableName == Plan_Improvement_Campaign_Program_Tactic || gmpln.TableName == Global) : 1 == 1)
                                                 select new GameplanDataTypeModel
                                                 {
                                                     GameplanDataTypeId = gmpln.GameplanDataTypeId,
                                                     IntegrationTypeId = gmpln.IntegrationTypeId,
                                                     TableName = gmpln.TableName,
                                                     ActualFieldName = gmpln.ActualFieldName,
                                                     DisplayFieldName = gmpln.DisplayFieldName,
                                                     IsGet = gmpln.IsGet,
                                                     IntegrationInstanceDataTypeMappingId = _map.IntegrationInstanceDataTypeMappingId,
                                                     IntegrationInstanceId = intgrtn.IntegrationInstanceId,
                                                     TargetDataType = _map.TargetDataType
                                                 }).ToList();

                List<GameplanDataTypeModel> listGameplanDataTypeCustomFields = new List<GameplanDataTypeModel>();
                #region "Declare Enum Variables"
                    //// Start - Added by :- Sohel Pathan on 03/12/2014 for PL #993
                    string Campaign_EntityType = Enums.EntityType.Campaign.ToString();
                    string Program_EntityType = Enums.EntityType.Program.ToString();
                    string Tactic_EntityType = Enums.EntityType.Tactic.ToString();
                    string Plan_Campaign = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign.ToString();
                    string Plan_Campaign_Program = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program.ToString();
                    #endregion

                //// Get GamePlan Custom Fields data.
                // Updated 23 March 2016 PL#2083 by Brad Gray to not retrieve fields where custom field 'IsGet' == true
                //Modified by Rahul Shah for PL #2184. here on tactic customfied list displayed for Marketo.
                listGameplanDataTypeCustomFields = (from custm in db.CustomFields
                                                    join m1 in db.IntegrationInstanceDataTypeMappings on custm.CustomFieldId equals m1.CustomFieldId into mapping
                                                    from m in mapping.Where(map => map.IntegrationInstanceId == id).DefaultIfEmpty()
                                                    where custm.IsDeleted == false && custm.ClientId == Sessions.User.CID && custm.IsGet == false &&
                                                    ((integrationTypeCode == Eloqua ? (custm.EntityType == Tactic_EntityType) : (integrationTypeCode == Marketo ? (custm.EntityType == Tactic_EntityType) : (integrationTypeCode == WorkFront ? (custm.EntityType == Tactic_EntityType) : 1 == 1)))) //Modified by Viral for PL #2266.
                                                    select new GameplanDataTypeModel
                                                    {
                                                        GameplanDataTypeId = custm.CustomFieldId,   // For Custom Fields CustomFieldId is GameplanDataType Id in Mapping
                                                        IntegrationTypeId = IntegrationTypeId,
                                                        TableName = custm.EntityType == Campaign_EntityType ? Plan_Campaign : (custm.EntityType == Program_EntityType ? Plan_Campaign_Program : (custm.EntityType == Tactic_EntityType ? Plan_Campaign_Program_Tactic : string.Empty)), 
                                                        ActualFieldName = custm.Name,
                                                        DisplayFieldName = custm.Name,
                                                        IsGet = false,
                                                        IntegrationInstanceDataTypeMappingId = m.IntegrationInstanceDataTypeMappingId,
                                                        IntegrationInstanceId = id,
                                                        TargetDataType = m.TargetDataType,
                                                        IsCustomField = true
                                                    }).ToList();
                //// End - Added by :- Sohel Pathan on 03/12/2014 for PL #993

                if (listGameplanDataTypeStageZero != null && listGameplanDataTypeStageZero.Count > 0)
                {
                    listGameplanDataTypeStageZero.AddRange(listGameplanDataTypeCustomFields);
                    return listGameplanDataTypeStageZero.OrderBy(map => map.TableName).ToList();
                }
                //// End - Added by :- Sohel Pathan on 28/05/2014 for PL #494 filter gameplan datatype by client id
                else
                {
                    if (listGameplanDataTypeCustomFields != null && listGameplanDataTypeCustomFields.Count > 0)
                    {
                        listGameplanDataTypeStageZero = listGameplanDataTypeCustomFields;
                        return listGameplanDataTypeStageZero.OrderBy(map => map.TableName).ToList();
                    }
                    else
                    {
                        TempData["DataMappingErrorMessage"] = Common.objCached.DataTypeMappingNotConfigured;
                        return listGameplanDataTypeStageZero = new List<GameplanDataTypeModel>();
                    }
                }
            }
            catch
            {
                TempData["TargetFieldInvalidMsg"] = "";
                TempData["DataMappingErrorMessage"] = Common.objCached.ErrorOccured;
                return new List<GameplanDataTypeModel>();
            }
        }
        #endregion

        #region Function get Gameplan DataTypes Pull list from DB.
        /// <summary>
        /// Get gameplan datatype list Pull from database based on IntegrationInstanceId
        /// Added by Dharmraj on 8-8-2014, Ticket #658
        /// </summary>
        /// <param name="id">ID of Integration instance</param>
        /// <param name="gameplanDatatypePullType"></param>
        /// <returns>Returns list of GameplanDataTypePullModel objects</returns>
        public List<GameplanDataTypePullModel> GetGameplanDataTypePullListFromDB(int id, Enums.GameplanDatatypePullType gameplanDatatypePullType, bool isSalesForce = false, bool isMQLShow = false)//Modified by Rahul Shah on 26/02/2016 for PL #2017
        {
            try
            {
                ViewBag.IntegrationInstanceId = id;
                //// Get Integration instance Title
                var integrationTypeObj = (from integrationInstance in db.IntegrationInstances
                                          join integartionType in db.IntegrationTypes on integrationInstance.IntegrationTypeId equals integartionType.IntegrationTypeId
                                          where integrationInstance.IsDeleted == false && integartionType.IsDeleted == false && integrationInstance.IntegrationInstanceId == id
                                          select integartionType);
                string integrationTypeName = integrationTypeObj.Select(integartionType => integartionType.Title).FirstOrDefault();
                string integrationTypeCode = integrationTypeObj.Select(integartionType => integartionType.Code).FirstOrDefault();

                if (string.IsNullOrEmpty(integrationTypeName))
                    ViewBag.IntegrationTypeName = "";
                else
                    ViewBag.IntegrationTypeName = integrationTypeName;

                TempData["ClosedDealInvalidMsg"] = Common.objCached.CloseDealTargetFieldInvalidMsg;

                List<GameplanDataTypePullModel> listGameplanDataTypePullZero = new List<GameplanDataTypePullModel>();
                //// Get list of GameplanDatatypePull objects when integration instance type is Salesforce
                //// Modified by Sohel Pathan on 22/12/2014 for PL #1061, OR condition has been added for Eloqua
                if (integrationTypeCode.Equals(Enums.IntegrationType.Salesforce.ToString(), StringComparison.OrdinalIgnoreCase) || integrationTypeCode.Equals(Enums.IntegrationType.Eloqua.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    //// Get list of All GameplanDataTypePullModel from DB by IntegrationInstance ID
                    //Modified by Rahul Shah on 26/02/2016 for PL #2017
                    List<string> strGameplanDatatypePullType = new List<string>();
                    strGameplanDatatypePullType.Add(gameplanDatatypePullType.ToString());
                    if (isSalesForce && isMQLShow)
                    {
                        strGameplanDatatypePullType.Add(Enums.GameplanDatatypePullType.MQL.ToString());
                    }

                    listGameplanDataTypePullZero = (from II in db.IntegrationInstances
                                                    join GDP in db.GameplanDataTypePulls on II.IntegrationTypeId equals GDP.IntegrationTypeId
                                                    join IIDMP in db.IntegrationInstanceDataTypeMappingPulls on GDP.GameplanDataTypePullId equals IIDMP.GameplanDataTypePullId into mapping
                                                    from m in mapping.Where(map => map.IntegrationInstanceId == id).DefaultIfEmpty()
                                                    where II.IntegrationInstanceId == id && strGameplanDatatypePullType.Contains(GDP.Type) && GDP.IsDeleted == false
                                                    select new GameplanDataTypePullModel
                                                    {
                                                        GameplanDataTypePullId = GDP.GameplanDataTypePullId,
                                                        IntegrationTypeId = GDP.IntegrationTypeId,
                                                        ActualFieldName = GDP.ActualFieldName,
                                                        DisplayFieldName = GDP.DisplayFieldName,
                                                        IntegrationInstanceDataTypeMappingPullId = m.IntegrationInstanceDataTypeMappingPullId,
                                                        IntegrationInstanceId = II.IntegrationInstanceId,
                                                        TargetDataType = m.TargetDataType,
                                                        Type = GDP.Type
                                                    }).ToList();
                }

                if (listGameplanDataTypePullZero != null && listGameplanDataTypePullZero.Count > 0)
                {
                    TempData["PullingTargetFieldInvalidMsg"] = Common.objCached.PullingTargetFieldInvalidMsg.ToString();
                    return listGameplanDataTypePullZero.ToList();
                }
                else
                {
                    TempData["DataMappingPullErrorMessage"] = Common.objCached.DataTypeMappingNotConfigured;
                    return listGameplanDataTypePullZero = new List<GameplanDataTypePullModel>();
                }
            }
            catch
            {
                TempData["ClosedDealInvalidMsg"] = "";
                TempData["DataMappingPullErrorMessage"] = Common.objCached.ErrorOccured;
                return new List<GameplanDataTypePullModel>();
            }
        }
        #endregion

        #region Save DataType Mapping using Ajax call
        /// <summary>
        /// Save gameplan mapping data type using ajax call
        /// Precondition: all objects in form have equal IsGet value
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>05/08/2014</CreatedDate>
        /// <param name="form">form object of post request</param>
        /// <param name="IntegrationInstanceId">Integration Id</param>
        /// <param name="UserId">user of current logged in user</param>
        /// <returns>returns json result with data save status flag and success/error message</returns>
        [HttpPost]
        public JsonResult SaveDataMapping(IList<GameplanDataTypeModel> form, int IntegrationInstanceId, string UserId = "")
        {
            //// Check whether UserId is loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

            //Added 26 Mar 2016 Brad Gray PL#2084
            //every GameplanDataTypeModel in the list should have the same IsGet properties
            //either they'll all be true or false
            //this will help determine which mapping types we're storing & removing

            bool isGet = false;
            if (form.Count > 0)
            {
                isGet = form.FirstOrDefault().IsGet;
            }

            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        //// Delete record from IntegrationInstanceDataTypeMapping table.

                        //Get a list of mappings with the correct instance Id & the correct isGet value
                        //if the CustomFieldId value is null, add the object to the list if the GameplanDataType.IsGet is correct
                        //otherwise, only add the object to the list if the IsGet value is correct
                        //Added by Brad Gray, 27 Mar 2016 PL#2084
                        List<int> lstIds = mrp.IntegrationInstanceDataTypeMappings.Where(_map => _map.IntegrationInstanceId == IntegrationInstanceId
                             && (_map.CustomFieldId == null ? (_map.GameplanDataType.IsGet == isGet) : (_map.CustomField.IsGet == isGet))).Select(_map => _map.IntegrationInstanceDataTypeMappingId).ToList();
                        if (lstIds != null && lstIds.Count > 0)
                        {
                            foreach (int ids in lstIds)
                            {
                                IntegrationInstanceDataTypeMapping obj = mrp.IntegrationInstanceDataTypeMappings.Where(_map => _map.IntegrationInstanceDataTypeMappingId == ids).FirstOrDefault();
                                if (obj != null)
                                    mrp.Entry(obj).State = EntityState.Deleted;
                            }
                        }

                        //// Add new record to IntegrationInstanceDataTypeMapping table.
                        IntegrationInstanceDataTypeMapping objMapping = null;
                        int instanceId;
                        foreach (GameplanDataTypeModel obj in form)
                        {
                            objMapping = new IntegrationInstanceDataTypeMapping();
                            instanceId = 0;
                            if (!string.IsNullOrEmpty(obj.TargetDataType))
                            {
                                int.TryParse(Convert.ToString(obj.IntegrationInstanceId), out instanceId);
                                objMapping.IntegrationInstanceId = instanceId;
                                //// Start - Modified by :- Sohel Pathan on 03/12/2014 for PL #993
                                if (obj.IsCustomField)
                                {
                                    objMapping.CustomFieldId = obj.GameplanDataTypeId;      // For Custom Fields CustomFieldId is GameplanDataType Id in Mapping
                                }
                                else
                                {
                                    objMapping.GameplanDataTypeId = obj.GameplanDataTypeId;
                                }
                                //// End - Modified by :- Sohel Pathan on 03/12/2014 for PL #993
                                objMapping.TargetDataType = obj.TargetDataType;
                                objMapping.CreatedDate = DateTime.Now;
                                objMapping.CreatedBy = Sessions.User.ID;
                                mrp.Entry(objMapping).State = EntityState.Added;
                            }
                        }
                        mrp.SaveChanges();
                        scope.Complete();
                    }
                }
                return Json(new { status = 1, Message = Common.objCached.DataTypeMappingSaveSuccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { status = 0, Message = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Save DataType Mapping Pull using Ajax call
        /// <summary>
        /// Save gameplan mapping data type Pull using ajax call
        /// </summary>
        /// <CreatedBy>Dharmraj</CreatedBy>
        /// <CreatedDate>08/08/2014</CreatedDate>
        /// <param name="IntegrationInstanceId">ID of integration instance</param>
        /// <param name="form">All values of form controls</param>
        /// <param name="UserId"></param>
        /// <returns>Returns status = 1 and success message on success and status = 0 and failure message on error</returns>
        [HttpPost]
        public JsonResult SaveDataMappingPullCloseDeal(IList<GameplanDataTypePullModel> form, int IntegrationInstanceId, string UserId = "", string closedwon = "")
        {
            //// Check whether UserId is loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            try
            {
                SaveDataMappingPull(form, IntegrationInstanceId, closedwon);
                return Json(new { status = 1, Message = Common.objCached.DataTypeMappingPullSaveSuccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { status = 0, Message = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Save gameplan mapping data type Pull using ajax call
        /// </summary>
        /// <CreatedBy>Dharmraj</CreatedBy>
        /// <CreatedDate>14/08/2014</CreatedDate>
        /// <param name="IntegrationInstanceId">ID of integration instance</param>
        /// <param name="form">All values of form controls</param>
        /// <param name="IntegrationType">integration type code</param>
        /// <param name="UserId">user id of logged in user</param>
        /// <returns>Returns status = 1 and success message on success and status = 0 and failure message on error</returns>
        [HttpPost]
        public JsonResult SaveDataMappingPulling(IList<GameplanDataTypePullModel> form, int IntegrationInstanceId, string IntegrationType = "", string UserId = "", string closedwon = "")
        {
            //// Check whether UserId is loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            if (form != null)
            {
                try
                {
                    SaveDataMappingPull(form, IntegrationInstanceId);
                    if (IntegrationType.Equals(Enums.IntegrationType.Eloqua.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return Json(new { status = 1, Message = Common.objCached.DataTypeMappingPullMQLSaveSuccess }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = 1, Message = Common.objCached.DataTypeMappingPullSaveSuccess }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                }
            }
            return Json(new { status = 0, Message = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Save gameplan mapping data type Pull
        /// Added By Dharmraj on 14-8-2014, #680
        /// </summary>
        /// <param name="form">List of GameplanDataTypePullModel objects</param>
        /// <param name="IntegrationInstanceId">ID of integration instance</param>
        public void SaveDataMappingPull(IList<GameplanDataTypePullModel> form, int IntegrationInstanceId, string cwValue = "")
        {
            using (MRPEntities mrp = new MRPEntities())
            {
                using (var scope = new TransactionScope())
                {
                    //// Delete all old IntegrationInstanceDataTypeMappingPull entries by integrationInstanceId and GaneplanDatatypePull type
                    string GameplanDatatypePullType = form[0].Type;
                    List<int> lstIds = mrp.IntegrationInstanceDataTypeMappingPulls.Where(map => map.IntegrationInstanceId == IntegrationInstanceId && map.GameplanDataTypePull.Type == GameplanDatatypePullType).Select(map => map.IntegrationInstanceDataTypeMappingPullId).ToList();
                    if (lstIds != null && lstIds.Count > 0)
                    {
                        List<IntegrationInstanceDataTypeMappingPull> lstDataMappingPull = mrp.IntegrationInstanceDataTypeMappingPulls.Where(m => lstIds.Contains(m.IntegrationInstanceDataTypeMappingPullId)).Select(mapping => mapping).ToList();
                        lstDataMappingPull.ForEach(mapping => mrp.Entry(mapping).State = EntityState.Deleted);
                        mrp.SaveChanges();
                    }

                    //// Add new IntegrationInstanceDataTypeMappingPull entry for new GameplanDataTypeMappingPull
                    foreach (GameplanDataTypePullModel obj in form)
                    {
                        if (!string.IsNullOrEmpty(obj.ActualFieldName)) // Added by rahul Shah on 21/11/2015. Because of unit test case failue
                        {
                            if (obj.ActualFieldName.Equals("CW"))       // set TargetDataType for CW value.
                                obj.TargetDataType = cwValue;
                        }
                        if (!string.IsNullOrEmpty(obj.TargetDataType))
                        {
                            IntegrationInstanceDataTypeMappingPull objMappingPull = new IntegrationInstanceDataTypeMappingPull();
                            int instanceId;
                            int.TryParse(Convert.ToString(obj.IntegrationInstanceId), out instanceId);
                            objMappingPull.IntegrationInstanceId = instanceId;
                            objMappingPull.GameplanDataTypePullId = obj.GameplanDataTypePullId;
                            objMappingPull.TargetDataType = obj.TargetDataType;
                            objMappingPull.CreatedDate = DateTime.Now;
                            objMappingPull.CreatedBy = Sessions.User.ID;
                            mrp.Entry(objMappingPull).State = EntityState.Added;
                        }
                    }
                    mrp.SaveChanges();
                    scope.Complete();
                }
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Added By : Kalpesh Sharma Save the value of General Settings in Integration #682
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveGeneralSetting(IntegrationModel form)
        {
            string rtnMessage = "";
            int IntegrationID = 0;
            int result = SaveIntegrationSettings(form, ref rtnMessage, ref IntegrationID);

            if (result == 0)
            {
                TempData["ErrorMessageGeneralSetting"] = rtnMessage;
                return Json(new { status = 0, ErrorMessage = rtnMessage, Id = IntegrationID }, JsonRequestBehavior.AllowGet);
            }

            if (result == 3)
            {
                TempData["SuccessMessage"] = rtnMessage;
            }

            ViewData["SuccessMessageGeneralSetting"] = rtnMessage;
            return Json(new { status = result, SuccessMessage = rtnMessage, Id = IntegrationID }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By : Kalpesh Sharma 08/08/2014 to Delete the Integration Instance
        /// </summary>
        /// <param name="form"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public int DeleteIntegrationSettings(IntegrationModel form, ref string message)
        {
            int Result = 0;

            IntegrationInstance objIntegrationInstance = db.IntegrationInstances.Where(intgrtn => intgrtn.IntegrationInstanceId == form.IntegrationInstanceId && intgrtn.IsDeleted.Equals(false) &&
                            intgrtn.ClientId == Sessions.User.CID).FirstOrDefault();

            if (objIntegrationInstance != null)
            {
                objIntegrationInstance.IsDeleted = true;
                Common.DeleteIntegrationInstance(form.IntegrationInstanceId, true);

                //// Delete External Service Record based on IntegrationInstanceId.
                DeleteExternalServer(form.IntegrationInstanceId);
                db.SaveChanges();
                message = Common.objCached.IntegrationDeleted;
                Result = 3;
            }
            else
            {
                message = Common.objCached.ErrorOccured;
            }

            return Result;
        }


        /// <summary>
        /// /// Added By : Kalpesh Sharma Save the value of General Settings in Integration #682
        /// </summary>
        /// <param name="form"></param>
        /// <param name="message"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public int SaveIntegrationSettings(IntegrationModel form, ref string message, ref int ID)
        {
            int Status = 0;
            bool IsAddOperation = (form.IntegrationInstanceId == 0 ? true : false);
            int objIntegrationInstanceId = 0;
            bool isUserNameChanged = false;     //// Added by Sohel Pathan on 29/12/2014 for an Internal Review Point

            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

            //// Check the Form action is requested to deleted operation or not. 
            if (Convert.ToString(form.IsDeleted).ToLower() == "true" && !IsAddOperation)
            {
                ID = form.IntegrationInstanceId;
                return (DeleteIntegrationSettings(form, ref message));
            }

            if (TestIntegrationCredentialsWithForm(form) && !form.IsDeleted)
            {
                try
                {
                    var isDuplicate = (dynamic)null;

                    if (IsAddOperation)
                    {
                        isDuplicate = db.IntegrationInstances.Where(intgrtn => intgrtn.Instance == form.Instance && intgrtn.ClientId == Sessions.User.CID && intgrtn.IntegrationType.IntegrationTypeId == form.IntegrationTypeId &&
                            intgrtn.IsDeleted == false).Any();
                    }
                    else
                    {
                        isDuplicate = db.IntegrationInstances.Where(intgrtn => intgrtn.Instance == form.Instance && intgrtn.ClientId == Sessions.User.CID && intgrtn.IntegrationType.IntegrationTypeId == form.IntegrationTypeId
                                        && intgrtn.IsDeleted == false && intgrtn.IntegrationInstanceId != form.IntegrationInstanceId).Any();
                    }

                    if (!isDuplicate && form.IntegrationInstanceId >= 0)
                    {
                        int SyncFrequenciesCount = 0, IntegrationInstancesCount = 0;

                        using (var scope = new TransactionScope())
                        {
                            //// update data
                            IntegrationInstance objIntegrationInstance;

                            if (IsAddOperation)
                            {
                                objIntegrationInstance = new IntegrationInstance();
                                objIntegrationInstance.CreatedDate = DateTime.Now;
                                objIntegrationInstance.IsDeleted = false;
                                objIntegrationInstance.IntegrationTypeId = form.IntegrationTypeId;
                                //// Added By : Kalpesh Sharma #781 Synchronization with Scheduler
                                objIntegrationInstance.CreatedBy = Sessions.User.ID;
                                //Added by RahuL Shah for PL #2184
                                if (form.IntegrationType.Code == Enums.IntegrationInstanceType.Marketo.ToString())
                                {
                                    objIntegrationInstance.CustomNamingPermission = true;
                                }
                            }
                            else
                            {
                                objIntegrationInstance = db.IntegrationInstances.Where(intgrtn => intgrtn.IntegrationInstanceId == form.IntegrationInstanceId && intgrtn.IsDeleted.Equals(false) &&
                                                        intgrtn.ClientId == Sessions.User.CID).FirstOrDefault();
                                objIntegrationInstance.IsDeleted = (Convert.ToString(form.IsDeleted).ToLower() == "true" ? true : false);
                                objIntegrationInstance.ModifiedBy = Sessions.User.ID;
                                objIntegrationInstance.ModifiedDate = DateTime.Now;

                                //// Start - Added by Sohel Pathan on 29/12/2014 for an Internal Review Point
                                if ((!(string.IsNullOrEmpty(objIntegrationInstance.Username))) && (!(string.IsNullOrEmpty(form.Username))) && (objIntegrationInstance.Username != form.Username))
                                {
                                    isUserNameChanged = true;
                                }
                                //// End - Added by Sohel Pathan on 29/12/2014 for an Internal Review Point
                            }

                            objIntegrationInstance.ClientId = Sessions.User.CID;
                            objIntegrationInstance.Instance = form.Instance;
                            objIntegrationInstance.IsImportActuals = form.IsImportActuals;
                            objIntegrationInstance.IsActive = form.IsActive;
                            if (form.IntegrationType.Code != Enums.IntegrationInstanceType.Marketo.ToString())
                            {
                                objIntegrationInstance.Password = Common.Encrypt(form.Password);
                                objIntegrationInstance.Username = form.Username;
                            }
                            else
                            {
                                objIntegrationInstance.Password = Common.Encrypt("Marketo");
                                objIntegrationInstance.Username = "Marketo";

                            }

                            if (IsAddOperation)
                            {
                                db.Entry(objIntegrationInstance).State = System.Data.EntityState.Added;
                                db.IntegrationInstances.Add(objIntegrationInstance);
                            }
                            else
                            {
                                db.Entry(objIntegrationInstance).State = System.Data.EntityState.Modified;
                            }

                            IntegrationInstancesCount = db.SaveChanges();
                            objIntegrationInstanceId = objIntegrationInstance.IntegrationInstanceId;
                            form.IntegrationTypeId = !string.IsNullOrEmpty(Convert.ToString(objIntegrationInstance.IntegrationTypeId)) ? objIntegrationInstance.IntegrationTypeId : 0;
                            ViewBag.integrationTypeId = form.IntegrationTypeId;

                            SyncFrequency objSyncFrequency;
                            if (IsAddOperation)
                            {
                                objSyncFrequency = new SyncFrequency();
                                objSyncFrequency.IntegrationInstanceId = objIntegrationInstance.IntegrationInstanceId;
                                objSyncFrequency.CreatedBy = Sessions.User.ID;
                                objSyncFrequency.CreatedDate = DateTime.Now;

                                if (form.SyncFrequency.Frequency == SyncFrequencys.Weekly)
                                    objSyncFrequency.DayofWeek = form.SyncFrequency.DayofWeek;
                                else if (form.SyncFrequency.Frequency == SyncFrequencys.Monthly)
                                    objSyncFrequency.Day = form.SyncFrequency.Day;
                                if (form.SyncFrequency.Frequency != SyncFrequencys.Hourly)
                                {
                                    if (form.SyncFrequency.Time.Length == 8)
                                    {
                                        int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                        if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.PM && hour != 12)
                                            hour = hour + 12;
                                        else if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.AM && hour == 12)
                                            hour = hour - 12;// Updated By Bhavesh Dobariya Date : 22 Dec 2015 Ticket : #1808
                                        objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                    }
                                }

                                //// Get NextSyncDate based on Frequency.
                                if (form.SyncFrequency.Frequency == SyncFrequencys.Hourly)
                                {
                                    DateTime currentDateTime = DateTime.Now.AddHours(1);
                                    objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0);
                                }
                                else if (form.SyncFrequency.Frequency == SyncFrequencys.Daily)
                                {
                                    DateTime currentDateTime = DateTime.Now.AddDays(1);
                                    TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                                    objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, time.Hours, time.Minutes, time.Seconds);
                                }
                                else if (form.SyncFrequency.Frequency == SyncFrequencys.Weekly)
                                {
                                    TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                                    DateTime nextDate = GetNextDateForDay(DateTime.Now, (DayOfWeek)Enum.Parse(typeof(DayOfWeek), objSyncFrequency.DayofWeek), time);

                                    objSyncFrequency.NextSyncDate = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, time.Hours, time.Minutes, time.Seconds);
                                }
                                else if (form.SyncFrequency.Frequency == SyncFrequencys.Monthly)
                                {
                                    DateTime currentDateTime = DateTime.Now;
                                    if (Convert.ToInt32(objSyncFrequency.Day) <= currentDateTime.Day)
                                    {
                                        currentDateTime.AddMonths(1);
                                    }
                                    TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                                    objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, Convert.ToInt32(objSyncFrequency.Day), time.Hours, time.Minutes, time.Seconds);
                                }

                            }
                            else
                            {
                                objSyncFrequency = db.SyncFrequencies.Where(freq => freq.IntegrationInstanceId == form.IntegrationInstanceId && freq.IntegrationInstance.IntegrationInstanceId == form.IntegrationInstanceId).FirstOrDefault();

                                if (objSyncFrequency != null)
                                {

                                    if (form.SyncFrequency.Frequency == SyncFrequencys.Hourly)
                                    {
                                        objSyncFrequency.Time = null;
                                        objSyncFrequency.DayofWeek = null;
                                        objSyncFrequency.Day = null;

                                        //// Handle NextSyncDate
                                        DateTime currentDateTime = DateTime.Now.AddHours(1);
                                        objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0);
                                    }
                                    else if (form.SyncFrequency.Frequency == SyncFrequencys.Daily)
                                    {
                                        if (form.SyncFrequency.Time.Length == 8)
                                        {
                                            int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                            if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.PM && hour != 12)
                                                hour = hour + 12;
                                            else if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.AM && hour == 12)
                                                hour = hour - 12;// Updated By Bhavesh Dobariya Date : 22 Dec 2015 Ticket : #1808
                                            objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                        }
                                        objSyncFrequency.DayofWeek = null;
                                        objSyncFrequency.Day = null;

                                        //// Handle NextSyncDate
                                        DateTime currentDateTime = DateTime.Now.AddDays(1);
                                        TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                                        objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, time.Hours, time.Minutes, time.Seconds);
                                    }
                                    else if (form.SyncFrequency.Frequency == SyncFrequencys.Weekly)
                                    {
                                        if (form.SyncFrequency.Time.Length == 8)
                                        {
                                            int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                            if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.PM && hour != 12)
                                                hour = hour + 12;
                                            else if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.AM && hour == 12)
                                                hour = hour - 12;// Updated By Bhavesh Dobariya Date : 22 Dec 2015 Ticket : #1808
                                            objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                        }
                                        objSyncFrequency.Day = null;
                                        objSyncFrequency.DayofWeek = form.SyncFrequency.DayofWeek;
                                        TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                                        //// Handle NextSyncDate
                                        DateTime nextDate = GetNextDateForDay(DateTime.Now, (DayOfWeek)Enum.Parse(typeof(DayOfWeek), objSyncFrequency.DayofWeek), time);
                                        objSyncFrequency.NextSyncDate = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, time.Hours, time.Minutes, time.Seconds);
                                    }
                                    else if (form.SyncFrequency.Frequency == SyncFrequencys.Monthly)
                                    {
                                        if (form.SyncFrequency.Time.Length == 8)
                                        {
                                            int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                            if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.PM && hour != 12)
                                                hour = hour + 12;
                                            else if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.AM && hour == 12)
                                                hour = hour - 12;// Updated By Bhavesh Dobariya Date : 22 Dec 2015 Ticket : #1808
                                            objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                        }
                                        objSyncFrequency.DayofWeek = null;
                                        objSyncFrequency.Day = form.SyncFrequency.Day;

                                        //// Handle NextSyncDate
                                        DateTime currentDateTime = DateTime.Now;
                                        if (Convert.ToInt32(objSyncFrequency.Day) <= currentDateTime.Day)
                                        {
                                            currentDateTime = currentDateTime.AddMonths(1);
                                        }
                                        TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                                        objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, Convert.ToInt32(objSyncFrequency.Day), time.Hours, time.Minutes, time.Seconds);
                                    }
                                }
                            }

                            objSyncFrequency.Frequency = form.SyncFrequency.Frequency;

                            if (IsAddOperation)
                            {
                                db.Entry(objSyncFrequency).State = System.Data.EntityState.Added;
                                db.SyncFrequencies.Add(objSyncFrequency);
                            }
                            else
                            {
                                db.Entry(objSyncFrequency).State = System.Data.EntityState.Modified;
                            }
                            SyncFrequenciesCount = db.SaveChanges();

                            //// Add Integration Type Attributes to IntegrationInstance_Attribute table.
                            if (form.IntegrationTypeAttributes != null)
                            {
                                // Add By Nishant Sheth
                                // Desc :: #2184 Observation for host 
                                string marketoHost = Enums.IntegrationTypeAttribute.Host.ToString();
                                var IntegrationAttrTypeIds = form.IntegrationTypeAttributes.Select(item => item.IntegrationTypeAttributeId).ToList();
                                var MarketoHostAttrTypeId = db.IntegrationTypeAttributes.Where(attr => IntegrationAttrTypeIds.Contains(attr.IntegrationTypeAttributeId)
                                    && attr.Attribute == marketoHost).Select(attr => attr.IntegrationTypeAttributeId).FirstOrDefault();
                                IntegrationInstance_Attribute objIntegrationInstance_Attribute;
                                foreach (var item in form.IntegrationTypeAttributes)
                                {

                                    if (IsAddOperation)
                                    {
                                        objIntegrationInstance_Attribute = new IntegrationInstance_Attribute();
                                        objIntegrationInstance_Attribute.CreatedBy = Sessions.User.ID;
                                        objIntegrationInstance_Attribute.CreatedDate = DateTime.Now;
                                        objIntegrationInstance_Attribute.IntegrationInstanceId = objIntegrationInstance.IntegrationInstanceId;
                                        objIntegrationInstance_Attribute.IntegrationTypeAttributeId = item.IntegrationTypeAttributeId;
                                    }
                                    else
                                    {
                                        objIntegrationInstance_Attribute = db.IntegrationInstance_Attribute.Where(attr => attr.IntegrationInstanceId == form.IntegrationInstanceId
                                            && attr.IntegrationTypeAttributeId == item.IntegrationTypeAttributeId && attr.IntegrationInstance.IntegrationTypeId == form.IntegrationTypeId).FirstOrDefault();
                                    }

                                    // Add By Nishant Sheth 
                                    // Desc :: Add Host details for marketo instance
                                    if (MarketoHostAttrTypeId == item.IntegrationTypeAttributeId)
                                    {
                                        string Host = item.Value.ToLower();
                                        string[] HostSplit = Host.Split(new string[] { "/rest" }, StringSplitOptions.None);
                                        if (HostSplit.Count() > 0)
                                        {
                                            Host = HostSplit[0];
                                        }
                                        Host = Host.TrimEnd('/');
                                        item.Value = Host;
                                    }
                                    // End By Nishant Sheth

                                    objIntegrationInstance_Attribute.Value = item.Value;
                                    db.Entry(objIntegrationInstance_Attribute).State = (IsAddOperation ? System.Data.EntityState.Added : System.Data.EntityState.Modified);
                                    if (IsAddOperation)
                                    {
                                        db.IntegrationInstance_Attribute.Add(objIntegrationInstance_Attribute);
                                    }
                                }
                                db.SaveChanges();
                            }

                            //// Status changed from Active to InActive, So remove all the integration dependency with Models.
                            if (form.IsActiveStatuChanged == true && form.IsActive == false && !IsAddOperation)
                            {
                                List<Model> ModelList = db.Models.Where(_mdl => _mdl.IsDeleted.Equals(false)).ToList();

                                //// Remove association of Integrartion from Plan
                                ModelList.Where(_mdl => _mdl.IntegrationInstanceId == form.IntegrationInstanceId).ToList().ForEach(
                                    ObjIntegrationInstance =>
                                    {
                                        ObjIntegrationInstance.IntegrationInstanceId = null;
                                        ObjIntegrationInstance.ModifiedDate = DateTime.Now;
                                        ObjIntegrationInstance.ModifiedBy = Sessions.User.ID;
                                        db.Entry(ObjIntegrationInstance).State = EntityState.Modified;
                                    });

                                //// Remove association of Integrartion from Plan
                                ModelList.Where(_mdl => _mdl.IntegrationInstanceEloquaId == form.IntegrationInstanceId).ToList().ForEach(
                                    ObjIntegrationInstance =>
                                    {
                                        ObjIntegrationInstance.IntegrationInstanceEloquaId = null;
                                        ObjIntegrationInstance.ModifiedDate = DateTime.Now;
                                        ObjIntegrationInstance.ModifiedBy = Sessions.User.ID;
                                        db.Entry(ObjIntegrationInstance).State = EntityState.Modified;
                                    });

                                //// Identify IntegrationInstanceId for INQ in Model Table and set reference null
                                ModelList.Where(_mdl => _mdl.IntegrationInstanceIdINQ == form.IntegrationInstanceId).ToList().ForEach(
                                    INQ =>
                                    {
                                        INQ.IntegrationInstanceIdINQ = null;
                                        INQ.ModifiedDate = DateTime.Now;
                                        INQ.ModifiedBy = Sessions.User.ID;
                                        db.Entry(INQ).State = EntityState.Modified;
                                    });

                                //// Identify IntegrationInstanceId for MQL in Model Table and set reference null
                                ModelList.Where(_mdl => _mdl.IntegrationInstanceIdMQL == form.IntegrationInstanceId).ToList().ForEach(
                                    MQL =>
                                    {
                                        MQL.IntegrationInstanceIdMQL = null;
                                        MQL.ModifiedDate = DateTime.Now;
                                        MQL.ModifiedBy = Sessions.User.ID;
                                        db.Entry(MQL).State = EntityState.Modified;
                                    });

                                //// Identify IntegrationInstanceId for CW in Model Table and set reference null
                                ModelList.Where(_mdl => _mdl.IntegrationInstanceIdCW == form.IntegrationInstanceId).ToList().ForEach(
                                    CW =>
                                    {
                                        CW.IntegrationInstanceIdCW = null;
                                        CW.ModifiedDate = DateTime.Now;
                                        CW.ModifiedBy = Sessions.User.ID;
                                        db.Entry(CW).State = EntityState.Modified;
                                    });

                                //// Identify IntegrationInstanceId for Project Management in Model Table and set reference null
                                ModelList.Where(_mdl => _mdl.IntegrationInstanceIdProjMgmt == form.IntegrationInstanceId).ToList().ForEach(
                                    ProjMgmt =>
                                    {
                                        ProjMgmt.IntegrationInstanceIdProjMgmt = null;
                                        ProjMgmt.ModifiedDate = DateTime.Now;
                                        ProjMgmt.ModifiedBy = Sessions.User.ID;
                                        db.Entry(ProjMgmt).State = EntityState.Modified;
                                    });

                                db.SaveChanges();
                            }
                            scope.Complete();
                        }

                        //// Set Message and Status.
                        if (IntegrationInstancesCount > 0 && SyncFrequenciesCount > 0)
                        {
                            if (IsAddOperation)
                            {
                                message = Common.objCached.IntegrationAdded;
                                Status = 1;
                            }
                            //// Start - Added by Sohel Pathan on 29/12/2014 for an Internal Review Point
                            else if (isUserNameChanged)
                            {
                                //// If Username has been changed, means settings are saved for other user
                                //// So, other tabs need to be refreshed to fill mapping data as per new user
                                message = Common.objCached.IntegrationEdited;
                                Status = 1;
                            }
                            //// Start - Added by Sohel Pathan on 29/12/2014 for an Internal Review Point
                            else
                            {
                                message = Common.objCached.IntegrationEdited;
                                Status = 2;
                            }

                            ID = objIntegrationInstanceId;
                        }
                        else
                        {
                            message = Common.objCached.ErrorOccured;
                            Status = 0;
                        }
                    }
                    else
                    {
                        Status = 0;
                        message = Common.objCached.IntegrationDuplicate;
                    }
                }
                catch
                {
                    Status = 0;
                    message = Common.objCached.ErrorOccured;
                }
            }
            else
            {
                Status = 0;
                message = Common.objCached.TestIntegrationFail;
            }

            return Status;
        }

        #region External Service PL#679

        /// <summary>
        /// Save the data to database
        /// </summary>
        /// <param name="frm"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveExternalForm(IntegrationInstanceExternalServerModel frm)
        {
            if (ModelState.IsValid)
            {
                if (!ValidateLocation(frm.IntegrationInstanceExternalServerId, frm.SFTPFileLocation))
                {
                    return Json(new { status = 0, ErrorMessage = string.Format(Common.objCached.DuplicateFileLocation, frm.SFTPFileLocation) });
                }
                if (!IsConnected(frm))
                {
                    return Json(new { status = 0, ErrorMessage = Common.objCached.ConnectionFail });
                }
                int ret = SaveExternalServer(frm);
                return Json(new { status = ret, ErrorMessage = Common.objCached.ServerConfigurationSaved });
            }
            return Json(new { status = 0, ErrorMessage = Common.objCached.ErrorOccured });
        }

        /// <summary>
        /// Validate location for the instance
        /// </summary>
        /// <param name="IntegrationInstanceExternalServerId"></param>
        /// <param name="SFTPFileLocation"></param>
        /// <returns></returns>
        private bool ValidateLocation(int IntegrationInstanceExternalServerId, string SFTPFileLocation)
        {
            IntegrationInstanceExternalServer obj = new IntegrationInstanceExternalServer();
            if (IntegrationInstanceExternalServerId == 0)
            {
                obj = db.IntegrationInstanceExternalServers.Where(ext => ext.IsDeleted == false && ext.SFTPFileLocation.ToLower() == SFTPFileLocation.ToLower()).FirstOrDefault();
            }
            else
            {
                obj = db.IntegrationInstanceExternalServers.Where(ext => ext.IntegrationInstanceExternalServerId != IntegrationInstanceExternalServerId && ext.IsDeleted == false && ext.SFTPFileLocation.ToLower() == SFTPFileLocation.ToLower()).FirstOrDefault();
            }
            if (obj == null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Try to connect with provided details
        /// </summary>
        /// <param name="frm"></param>
        /// <returns></returns>
        private bool IsConnected(IntegrationInstanceExternalServerModel frm)
        {
            try
            {
                EloquaResponse er = new EloquaResponse();
                int Port = 0;
                int.TryParse(Convert.ToString(frm.SFTPPort), out Port);
                if (Port == 0)
                    Port = int.Parse(ConfigurationManager.AppSettings["SFTPDefaultPort"].ToString());
                return er.AuthenticateSFTP(frm.SFTPServerName, frm.SFTPUserName, frm.SFTPPassword, Port);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Add or Update the data
        /// </summary>
        /// <param name="frm"></param>
        /// <returns></returns>
        private int SaveExternalServer(IntegrationInstanceExternalServerModel frm)
        {
            //// IntegrationInstanceExternalServerId is 0 then Insert new record else update record.
            if (frm.IntegrationInstanceExternalServerId == 0)
            {
                IntegrationInstanceExternalServer obj = new IntegrationInstanceExternalServer();
                obj.IntegrationInstanceId = frm.IntegrationInstanceId;
                obj.SFTPServerName = frm.SFTPServerName;
                obj.SFTPFileLocation = frm.SFTPFileLocation;
                obj.SFTPUserName = frm.SFTPUserName;
                obj.SFTPPassword = Common.Encrypt(frm.SFTPPassword);
                if (string.IsNullOrEmpty(frm.SFTPPort))
                {
                    obj.SFTPPort = ConfigurationManager.AppSettings["SFTPDefaultPort"].ToString();
                }
                else
                {
                    obj.SFTPPort = frm.SFTPPort;
                }
                obj.CreatedBy = Sessions.User.ID;
                obj.CreatedDate = DateTime.Now;
                obj.IsDeleted = false;
                db.Entry(obj).State = System.Data.EntityState.Added;
                db.IntegrationInstanceExternalServers.Add(obj);
                db.SaveChanges();
                return obj.IntegrationInstanceExternalServerId;
            }
            else
            {
                IntegrationInstanceExternalServer obj = db.IntegrationInstanceExternalServers.Where(i => i.IntegrationInstanceExternalServerId == frm.IntegrationInstanceExternalServerId).SingleOrDefault();
                if (obj != null)
                {
                    obj.SFTPServerName = frm.SFTPServerName;
                    obj.SFTPFileLocation = frm.SFTPFileLocation;
                    obj.SFTPUserName = frm.SFTPUserName;
                    obj.SFTPPassword = Common.Encrypt(frm.SFTPPassword);
                    if (string.IsNullOrEmpty(frm.SFTPPort))
                    {
                        obj.SFTPPort = ConfigurationManager.AppSettings["SFTPDefaultPort"].ToString();
                    }
                    else
                    {
                        obj.SFTPPort = frm.SFTPPort;
                    }
                    obj.ModifiedBy = Sessions.User.ID;
                    obj.ModifiedDate = DateTime.Now;
                    db.Entry(obj).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                    return obj.IntegrationInstanceExternalServerId;
                }
            }
            return 1;
        }

        /// <summary>
        /// Delete the data
        /// </summary>
        /// <param name="IntegrationInstanceId"></param>
        /// <returns></returns>
        private int DeleteExternalServer(int IntegrationInstanceId)
        {
            IntegrationInstanceExternalServer obj = db.IntegrationInstanceExternalServers.Where(ext => ext.IntegrationInstanceId == IntegrationInstanceId).FirstOrDefault();
            if (obj != null)
            {
                obj.IsDeleted = true;
                obj.ModifiedBy = Sessions.User.ID;
                obj.ModifiedDate = DateTime.Now;
                db.Entry(obj).State = System.Data.EntityState.Modified;
                return db.SaveChanges();
            }
            return 0;
        }

        /// <summary>
        /// Get External server data.
        /// </summary>
        /// <param name="IntegrationInstanceId"></param>
        /// <returns>Return IntegrationInstanceExternalServerModel data.</returns>
        private IntegrationInstanceExternalServerModel GetExternalServerModelByInstanceId(int IntegrationInstanceId)
        {
            IntegrationInstanceExternalServerModel model = new IntegrationInstanceExternalServerModel();
            model.IntegrationInstanceId = IntegrationInstanceId;
            IntegrationInstanceExternalServer obj = db.IntegrationInstanceExternalServers.Where(ext => ext.IntegrationInstanceId == IntegrationInstanceId && ext.IsDeleted == false).FirstOrDefault();
            if (obj != null)
            {
                model = new IntegrationInstanceExternalServerModel();
                model.IntegrationInstanceExternalServerId = obj.IntegrationInstanceExternalServerId;
                model.IntegrationInstanceId = obj.IntegrationInstanceId;
                model.SFTPServerName = obj.SFTPServerName;
                model.SFTPFileLocation = obj.SFTPFileLocation;
                model.SFTPUserName = obj.SFTPUserName;
                model.SFTPPassword = Common.Decrypt(obj.SFTPPassword);
                model.SFTPPort = obj.SFTPPort;
            }
            return model;
        }

        #endregion

        /// <summary>
        /// /// Added By : Viral Kadiya to Get IntegrationType based on IntegrationTypeId.
        /// </summary>
        /// <param name="integrationTypeId"></param>
        /// <returns>Return IntegrationType object.</returns>
        public IntegrationType GetIntegrationTypeById(int integrationTypeId)
        {
            IntegrationType objIntegrationType = null;
            try
            {
                objIntegrationType = db.IntegrationTypes.Where(integrationtype => integrationtype.IsDeleted.Equals(false) && integrationtype.IntegrationTypeId == integrationTypeId).FirstOrDefault();
                if (objIntegrationType == null)
                    objIntegrationType = new IntegrationType();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objIntegrationType;
        }

        /// <summary>
        /// /// Added By : Viral Kadiya to Get List of IntegrationType Attributes Model based on IntegrationTypeId.
        /// </summary>
        /// <param name="integrationTypeId"></param>
        /// <returns>Return IntegrationType object.</returns>
        public List<IntegrationTypeAttributeModel> GetIntegrationTypeAttributesModelById(int integrationTypeId)
        {
            List<IntegrationTypeAttributeModel> objIntegrationTypeAttributes = null;
            try
            {
                objIntegrationTypeAttributes = db.IntegrationTypeAttributes.Where(attr => attr.IsDeleted.Equals(false) && attr.IntegrationType.IntegrationTypeId == integrationTypeId)
                .Select(attr => new IntegrationTypeAttributeModel
                {
                    Attribute = attr.Attribute,
                    AttributeType = attr.AttributeType,
                    IntegrationTypeAttributeId = attr.IntegrationTypeAttributeId,
                    Value = "",
                }).ToList();
                if (objIntegrationTypeAttributes == null)
                    objIntegrationTypeAttributes = new List<IntegrationTypeAttributeModel>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objIntegrationTypeAttributes;
        }

        public void SetPullClosedDealsCache(List<Integration.Helper.PullClosedDealModel> pullClosedDealsModel)
        {
            // Set/get common messages cache
            if (System.Web.HttpContext.Current.Cache["closedDealsPickList"] == null)
            {
                System.Web.HttpContext.Current.Cache["closedDealsPickList"] = pullClosedDealsModel;
                //CacheDependency dependency = new CacheDependency(Common.xmlMsgFilePath);
                //System.Web.HttpContext.Current.Cache.Insert("closedDealsPickList", );
            }
        }

    }
}

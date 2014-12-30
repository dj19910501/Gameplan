﻿//// Controller to handle external service method(s).
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

        #region Integration Listing

        /// <summary>
        /// Integration data listing
        /// </summary>
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]  // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult Index()
        {
            // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

            ViewBag.CurrentUserRole = Convert.ToString(Sessions.User.RoleCode);

            //-- Get list of IntegrationTypes
            IList<SelectListItem> IntegrationList = new List<SelectListItem>();
            try
            {
                var dbList = db.IntegrationTypes.Where(it => it.IsDeleted.Equals(false)).Select(it => it).ToList();
                IntegrationList = dbList.Select(it => new SelectListItem() { Text = it.Title, Value = it.IntegrationTypeId.ToString(), Selected = false })
                                .OrderBy(it => it.Text).ToList();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            TempData["ExternalFieldList"] = new SelectList(IntegrationList, "Value", "Text", IntegrationList.First());

            return View();
        }

        /// <summary>
        /// Generate IntegrationService Listing.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetIntegrationServiceListings()
        {
            //// Get Integration Instance List.
            var IntegrationInstanceList = db.IntegrationInstances
                                            .Where(ii => ii.IsDeleted.Equals(false) && ii.ClientId == Sessions.User.ClientId && ii.IntegrationType.IntegrationTypeId == ii.IntegrationTypeId)
                                            .Select(ii => ii).ToList();

            //// Return IntegrationInstance list with specific fields.
            var returnList = IntegrationInstanceList.Select(intgrtn => new
            {
                IntegrationInstanceId = intgrtn.IntegrationInstanceId,
                IntegrationTypeId = intgrtn.IntegrationTypeId,
                Instance = (intgrtn.Instance == null || intgrtn.Instance.ToString() == "null") ? "" : intgrtn.Instance.ToString(),
                Provider = (intgrtn.IntegrationType.Title == null || intgrtn.IntegrationType.Title.ToString() == "null") ? "" : intgrtn.IntegrationType.Title.ToString(),
                LastSyncStatus = string.IsNullOrWhiteSpace(intgrtn.LastSyncStatus) ? Common.TextForModelIntegrationInstanceTypeOrLastSyncNull : intgrtn.LastSyncStatus.ToString(),
                LastSyncDate = (intgrtn.LastSyncDate.HasValue ? Convert.ToDateTime(intgrtn.LastSyncDate).ToString(DateFormat) : Common.TextForModelIntegrationInstanceTypeOrLastSyncNull),
            }).OrderByDescending(intgrtn => intgrtn.Instance).ToList();

            return Json(returnList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get IntegrationType List to fill IntegrationType Combo
        /// </summary>
        /// <returns></returns>
        public JsonResult GetIntegrationTypeList()
        {
            IList<SelectListItem> IntegrationList = new List<SelectListItem>();
            try
            {
                //// Get IntegrationTypes list.
                var dbList = db.IntegrationTypes.Where(it => it.IsDeleted.Equals(false)).Select(it => it).ToList();
                IntegrationList = dbList.Select(it => new SelectListItem() { Text = it.Title, Value = it.IntegrationTypeId.ToString(), Selected = false })
                                .OrderBy(it => it.Text).ToList();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            if (IntegrationList != null)
            {
                SelectListItem objDefaultEntry = new SelectListItem();
                objDefaultEntry.Value = "0";
                objDefaultEntry.Text = "Add New Integration";
                objDefaultEntry.Selected = true;
                IntegrationList.Insert(0, objDefaultEntry);
            }

            return Json(IntegrationList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This will return view for Integration folder.
        /// </summary>
        /// <param name="id">Integration id.</param>
        /// <param name="TypeId">Integration type id.</param>        
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]
        public ActionResult GetIntegrationFolder(int TypeId, int id = 0)
        {
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            string status = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()];

            try
            {
                ViewBag.IntegrationInstanceId = id;
                ViewBag.IntegrationTypeId = TypeId;

                IntegrationType integrationTypeObj = GetIntegrationTypeById(TypeId);

                if (integrationTypeObj != null)
                {
                    ViewBag.IntegrationTypeCode = integrationTypeObj.Code;
                }

                Guid clientId = Sessions.User.ClientId;

                ////Get published plan list year for logged in client.
                var objPlan = (from _pln in db.Plans
                               join _mdl in db.Models on _pln.ModelId equals _mdl.ModelId
                               join bu in db.BusinessUnits on _mdl.BusinessUnitId equals bu.BusinessUnitId
                               where bu.ClientId == clientId && bu.IsDeleted == false && _mdl.IsDeleted == false && _pln.IsDeleted == false && _pln.Status == status
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
                Guid clientId = Sessions.User.ClientId;
                string status = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()];
                int Int_Year = Convert.ToInt32(!string.IsNullOrEmpty(Year) ? Convert.ToInt32(Year) : 0);

                List<string> lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList();

                //// Get Custom Restriction model.
                List<UserCustomRestrictionModel> lstUserCustomRestriction = new List<UserCustomRestrictionModel>();
                lstUserCustomRestriction = Common.GetUserCustomRestriction();
                int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
                int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                lstUserCustomRestriction = lstUserCustomRestriction.Where(_custmRestrctn => (_custmRestrctn.Permission == ViewOnlyPermission || _custmRestrctn.Permission == ViewEditPermission) && _custmRestrctn.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).ToList();

                List<Guid> lstBUs = new List<Guid>();
                foreach (UserCustomRestrictionModel o in lstUserCustomRestriction)
                {
                    lstBUs.Add(new Guid(o.CustomFieldId));
                }

                // Get the list of plan, filtered by Business Unit , Year selected and published plan for logged in client.
                if (Int_Year > 0)
                {
                    objIntegrationPlanList = (from _pln in db.Plans
                                              join _mdl in db.Models on _pln.ModelId equals _mdl.ModelId
                                              join bu in db.BusinessUnits on _mdl.BusinessUnitId equals bu.BusinessUnitId
                                              where bu.ClientId == clientId && bu.IsDeleted == false && _mdl.IsDeleted == false &&
                                              _pln.IsDeleted == false && _pln.Year == Year && _pln.Status == status && lstBUs.Contains(_mdl.BusinessUnitId)
                                              select new { _pln, _mdl }).OrderByDescending(d => d._pln.ModifiedDate ?? d._pln.CreatedDate).ThenBy(d => d._pln.Title).ToList().Select(d => new IntegrationPlanList { PlanId = d._pln.PlanId, PlanTitle = d._pln.Title, FolderPath = d._pln.EloquaFolderPath, BUId = d._mdl.BusinessUnitId }).ToList();

                    //// Set permission for the plan on bases of BU permission
                    foreach (var item in objIntegrationPlanList)
                    {
                        foreach (var list in lstUserCustomRestriction)
                        {
                            if (item.BUId.ToString() == list.CustomFieldId)
                            {
                                item.Permission = list.Permission;
                            }
                        }
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
                            foreach (var item in IntegrationPlanList)
                            {
                                Plan objPlan = db.Plans.Where(_pln => _pln.PlanId == item.PlanId).FirstOrDefault();
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

        #region Add Integration

        /// <summary>
        /// Add new Integration Service
        /// </summary>
        /// <param name="integrationTypeId"></param>
        /// <returns></returns>
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]  // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult create(int integrationTypeId)
        {
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

            ViewBag.integrationTypeId = integrationTypeId;
            IntegrationModel objModelToView = new IntegrationModel();

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
            objModelToView.ExternalServer = GetExternalServer(0);
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
                    var isDuplicate = db.IntegrationInstances.Where(_intgrtn => _intgrtn.Instance == form.Instance && _intgrtn.ClientId == Sessions.User.ClientId && _intgrtn.IntegrationType.IntegrationTypeId == form.IntegrationTypeId).Any();
                    if (isDuplicate)
                    {
                        TempData["ErrorMessage"] = Common.objCached.IntegrationDuplicate;
                        form = reCreateView(form);
                        return View(form);
                    }

                    // Save data
                    IntegrationInstance objIntegrationInstance = new IntegrationInstance();
                    objIntegrationInstance.ClientId = Sessions.User.ClientId;
                    objIntegrationInstance.CreatedBy = Sessions.User.UserId;
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
                    objSyncFrequency.CreatedBy = Sessions.User.UserId;
                    objSyncFrequency.CreatedDate = DateTime.Now;
                    objSyncFrequency.Frequency = form.SyncFrequency.Frequency;
                    if (form.SyncFrequency.Frequency == "Weekly")
                        objSyncFrequency.DayofWeek = form.SyncFrequency.DayofWeek;
                    else if (form.SyncFrequency.Frequency == "Monthly")
                        objSyncFrequency.Day = form.SyncFrequency.Day;
                    if (form.SyncFrequency.Frequency != "Hourly")
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
                    if (form.SyncFrequency.Frequency == "Hourly")
                    {
                        DateTime currentDateTime = DateTime.Now.AddHours(1);
                        objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0);
                    }
                    else if (form.SyncFrequency.Frequency == "Daily")
                    {
                        DateTime currentDateTime = DateTime.Now.AddDays(1);
                        TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                        objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, time.Hours, time.Minutes, time.Seconds);
                    }
                    else if (form.SyncFrequency.Frequency == "Weekly")
                    {
                        DateTime nextDate = GetNextDateForDay(DateTime.Now, (DayOfWeek)Enum.Parse(typeof(DayOfWeek), objSyncFrequency.DayofWeek));
                        TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                        objSyncFrequency.NextSyncDate = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, time.Hours, time.Minutes, time.Seconds);
                    }
                    else if (form.SyncFrequency.Frequency == "Monthly")
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
                        foreach (var item in form.IntegrationTypeAttributes)
                        {
                            IntegrationInstance_Attribute objIntegrationInstance_Attribute = new IntegrationInstance_Attribute();
                            objIntegrationInstance_Attribute.CreatedBy = Sessions.User.UserId;
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
        public static DateTime GetNextDateForDay(DateTime startDate, DayOfWeek desiredDay)
        {
            // (There has to be a better way to do this, perhaps mathematically.)
            // Traverse this week
            DateTime nextDate = startDate;
            while (nextDate.DayOfWeek != desiredDay)
                nextDate = nextDate.AddDays(1D);

            return nextDate;
        }

        #endregion

        #region Common Functions

        /// <summary>
        /// Populate sync frequency combos
        /// </summary>
        public void populateSyncFreqData()
        {
            List<SelectListItem> lstSyncFreq = new List<SelectListItem>();

            //// Add Static fields to Frequency List.
            SelectListItem objItem1 = new SelectListItem();
            objItem1.Text = "Hourly";
            objItem1.Value = "Hourly";
            lstSyncFreq.Add(objItem1);

            SelectListItem objItem2 = new SelectListItem();
            objItem2.Text = "Daily";
            objItem2.Value = "Daily";
            lstSyncFreq.Add(objItem2);

            SelectListItem objItem3 = new SelectListItem();
            objItem3.Text = "Weekly";
            objItem3.Value = "Weekly";
            lstSyncFreq.Add(objItem3);

            SelectListItem objItem4 = new SelectListItem();
            objItem4.Text = "Monthly";
            objItem4.Value = "Monthly";
            lstSyncFreq.Add(objItem4);

            TempData["lstSyncFreq"] = new SelectList(lstSyncFreq, "Value", "Text", lstSyncFreq.First());

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

            TempData["lst24Hours"] = new SelectList(lst24Hours, "Value", "Text", lst24Hours.First());

            List<SelectListItem> lstWeekdays = new List<SelectListItem>();
            foreach (var item in Enum.GetValues(typeof(DayOfWeek)))
            {
                SelectListItem objTime = new SelectListItem();
                objTime.Text = item.ToString();
                objTime.Value = item.ToString();
                lstWeekdays.Add(objTime);
            }

            TempData["lstWeekdays"] = new SelectList(lstWeekdays, "Value", "Text", lstWeekdays.First());

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

            return Json(new SelectList(lst24Hours, "Value", "Text", lst24Hours.First()), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Populate Weekdays combo
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult populateWeekDayOptions()
        {
            List<SelectListItem> lstWeekdays = new List<SelectListItem>();
            foreach (var item in Enum.GetValues(typeof(DayOfWeek)))
            {
                SelectListItem objTime = new SelectListItem();
                objTime.Text = item.ToString();
                objTime.Value = item.ToString();
                lstWeekdays.Add(objTime);
            }

            return Json(new SelectList(lstWeekdays, "Value", "Text", lstWeekdays.First()), JsonRequestBehavior.AllowGet);
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
        public ActionResult edit(int id = 0, int TypeId = 0)
        {
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            int IntegrationTypeId = 0;
            var record = (dynamic)null;
            IntegrationModel objView = new IntegrationModel();

            if (id == 0)
            {
                objView.IntegrationTypeAttributes = GetIntegrationTypeAttributesModelById(TypeId);
                IntegrationTypeId = TypeId;
            }
            else
            {
                record = db.IntegrationInstances
                                    .Where(ii => ii.IsDeleted.Equals(false) && ii.ClientId == Sessions.User.ClientId && ii.IntegrationInstanceId == id)
                                    .Select(ii => ii).FirstOrDefault();
                objView.Instance = record.Instance;
                objView.Username = record.Username;
                objView.Password = Common.Decrypt(record.Password);
                objView.IntegrationInstanceId = record.IntegrationInstanceId;
                objView.IntegrationTypeId = record.IntegrationTypeId;
                objView.IsActive = record.IsActive;
                objView.IsImportActuals = record.IsImportActuals;

                IntegrationTypeId = record.IntegrationTypeId;

                var recordSync = db.SyncFrequencies
                                        .Where(freq => freq.IntegrationInstanceId == id && freq.IntegrationInstance.ClientId == Sessions.User.ClientId)
                                        .Select(freq => freq).FirstOrDefault();

                SyncFrequencyModel objSync = new SyncFrequencyModel();
                if (recordSync != null)
                {
                    if (recordSync.Day != null)
                        objSync.Day = recordSync.Day;
                    if (recordSync.DayofWeek != null)
                        objSync.DayofWeek = recordSync.DayofWeek;
                    objSync.Frequency = recordSync.Frequency;
                    if (recordSync.Time.HasValue == true)
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
                                    .Where(attr => attr.IntegrationTypeAttributeId == attr.IntegrationTypeAttribute.IntegrationTypeAttributeId && attr.IntegrationInstanceId == id && attr.IntegrationInstance.ClientId == Sessions.User.ClientId)
                                    .Select(attr => attr).ToList();

                //// Add IntegrationType Attributes data to List.
                List<IntegrationTypeAttributeModel> lstObjIntegrationTypeAttributeModel = new List<IntegrationTypeAttributeModel>();
                foreach (var item in recordAttribute)
                {
                    IntegrationTypeAttributeModel objIntegrationTypeAttributeModel = new IntegrationTypeAttributeModel();
                    objIntegrationTypeAttributeModel.Attribute = item.IntegrationTypeAttribute.Attribute;
                    objIntegrationTypeAttributeModel.AttributeType = item.IntegrationTypeAttribute.AttributeType;
                    objIntegrationTypeAttributeModel.IntegrationTypeAttributeId = item.IntegrationTypeAttribute.IntegrationTypeAttributeId;
                    objIntegrationTypeAttributeModel.IntegrationTypeId = item.IntegrationTypeAttribute.IntegrationTypeId;
                    objIntegrationTypeAttributeModel.Value = item.Value;
                    lstObjIntegrationTypeAttributeModel.Add(objIntegrationTypeAttributeModel);
                }

                objView.IntegrationTypeAttributes = lstObjIntegrationTypeAttributeModel;
            }

            //// Add IntegrationType data to List.
            IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
            var integrationTypeObj = GetIntegrationTypeById(IntegrationTypeId);
            objIntegrationTypeModel.Title = integrationTypeObj.Title;
            objIntegrationTypeModel.Code = integrationTypeObj.Code;
            objView.IntegrationType = objIntegrationTypeModel;

            objView.IntegrationTypeId = IntegrationTypeId;
            ViewBag.integrationTypeId = IntegrationTypeId;

            populateSyncFreqData();
            if (id > 0)
            {
                objView.GameplanDataTypeModelList = GetGameplanDataTypeList(id);   // Added by Sohel Pathan on 05/08/2014 for PL ticket #656 and #681
                objView.ExternalServer = GetExternalServer(id);

                // Dharmraj Start : #658: Integration - UI - Pulling Revenue - Salesforce.com
                objView.GameplanDataTypePullModelList = GetGameplanDataTypePullList(id);
                // Dharmraj End : #658: Integration - UI - Pulling Revenue - Salesforce.com
                // Dharmraj Start : #680: Integration - UI - Pull responses from Salesforce
                objView.GameplanDataTypePullRevenueModelList = GetGameplanDataTypePullRevenueList(id);
                // Dharmraj End : #680: Integration - UI - Pull responses from Salesforce
            }

            return View(objView);
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
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.integrationTypeId = form.IntegrationTypeId;

            if (!TestIntegrationCredentialsWithForm(form))
            {
                TempData["ErrorMessage"] = Common.objCached.TestIntegrationFail;
                form = reCreateView(form);
                return View(form);
            }
            try
            {
                var isDuplicate = db.IntegrationInstances.Where(_intgrt => _intgrt.Instance == form.Instance && _intgrt.ClientId == Sessions.User.ClientId && _intgrt.IntegrationType.IntegrationTypeId == form.IntegrationTypeId
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
                        _intgrt.ClientId == Sessions.User.ClientId).FirstOrDefault();

                    if (objIntegrationInstance != null)
                    {
                        objIntegrationInstance.ClientId = Sessions.User.ClientId;
                        objIntegrationInstance.ModifiedBy = Sessions.User.UserId;
                        objIntegrationInstance.ModifiedDate = DateTime.Now;
                        objIntegrationInstance.Instance = form.Instance;
                        if (Convert.ToString(form.IsDeleted).ToLower() == "true")
                        {
                            objIntegrationInstance.IsDeleted = true;
                        }
                        else
                        {
                            objIntegrationInstance.IsDeleted = false;
                        }
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
                            if (form.SyncFrequency.Frequency == "Hourly")
                            {
                                objSyncFrequency.Time = null;
                                objSyncFrequency.DayofWeek = null;
                                objSyncFrequency.Day = null;
                            }
                            else if (form.SyncFrequency.Frequency == "Daily")
                            {
                                if (form.SyncFrequency.Time.Length == 8)
                                {
                                    int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                    if (form.SyncFrequency.Time.Substring(6, 2) == "PM" && hour != 12)
                                        hour = hour + 12;
                                    objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                }
                                objSyncFrequency.DayofWeek = null;
                                objSyncFrequency.Day = null;
                            }
                            else if (form.SyncFrequency.Frequency == "Weekly")
                            {
                                if (form.SyncFrequency.Time.Length == 8)
                                {
                                    int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                    if (form.SyncFrequency.Time.Substring(6, 2) == "PM" && hour != 12)
                                        hour = hour + 12;
                                    objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                }
                                objSyncFrequency.Day = null;
                                objSyncFrequency.DayofWeek = form.SyncFrequency.DayofWeek;
                            }
                            else if (form.SyncFrequency.Frequency == "Monthly")
                            {
                                if (form.SyncFrequency.Time.Length == 8)
                                {
                                    int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                    if (form.SyncFrequency.Time.Substring(6, 2) == "PM" && hour != 12)
                                        hour = hour + 12;
                                    objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                }
                                objSyncFrequency.DayofWeek = null;
                                objSyncFrequency.Day = form.SyncFrequency.Day;
                            }

                            //// Handle NextSyncDate based on Frequency.
                            if (form.SyncFrequency.Frequency == "Hourly")
                            {
                                DateTime currentDateTime = DateTime.Now.AddHours(1);
                                objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0);
                            }
                            else if (form.SyncFrequency.Frequency == "Daily")
                            {
                                DateTime currentDateTime = DateTime.Now.AddDays(1);
                                TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                                objSyncFrequency.NextSyncDate = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, time.Hours, time.Minutes, time.Seconds);
                            }
                            else if (form.SyncFrequency.Frequency == "Weekly")
                            {
                                DateTime nextDate = GetNextDateForDay(DateTime.Now, (DayOfWeek)Enum.Parse(typeof(DayOfWeek), objSyncFrequency.DayofWeek));
                                TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                                objSyncFrequency.NextSyncDate = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, time.Hours, time.Minutes, time.Seconds);
                            }
                            else if (form.SyncFrequency.Frequency == "Monthly")
                            {
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
                            foreach (var item in form.IntegrationTypeAttributes)
                            {
                                IntegrationInstance_Attribute objIntegrationInstance_Attribute = db.IntegrationInstance_Attribute.Where(_attr => _attr.IntegrationInstanceId == form.IntegrationInstanceId
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

                        var objModelList = db.Models.Where(_mdl => _mdl.IsDeleted.Equals(false) && _mdl.IntegrationInstanceId == form.IntegrationInstanceId).ToList();

                        if (objModelList != null)
                        {
                            foreach (var item in objModelList)
                            {
                                item.IntegrationInstanceId = null;
                                item.ModifiedDate = DateTime.Now;
                                item.ModifiedBy = Sessions.User.UserId;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
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
            try
            {
                ExternalIntegration externalIntegration = new ExternalIntegration(id,Sessions.ApplicationId, Sessions.User.UserId);
                externalIntegration.Sync();
                IntegrationInstance integrationInstance = db.IntegrationInstances.FirstOrDefault(instance => instance.IntegrationInstanceId.Equals(id));

                //// Return Status and lastSync value in json format.
                string status = integrationInstance.LastSyncStatus;
                if (integrationInstance.LastSyncDate.HasValue)
                {
                    return Json(new { status = integrationInstance.LastSyncStatus, lastSync = Convert.ToDateTime(integrationInstance.LastSyncDate).ToString(DateFormat) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
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
                    //// Check Credentials whether Authenticate or not for Eloqua Client.
                    IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient();
                    integrationEloquaClient._instance = form.Instance;// "TechnologyPartnerBulldog";
                    integrationEloquaClient._username = form.Username;// "Brij.Bhavsar";
                    integrationEloquaClient._password = form.Password;//"Brij1234";
                    RevenuePlanner.Models.IntegrationType integrationType = GetIntegrationTypeById(form.IntegrationTypeId);
                    integrationEloquaClient._apiURL = integrationType.APIURL;
                    integrationEloquaClient._apiVersion = integrationType.APIVersion;
                    integrationEloquaClient.Authenticate();
                    isAuthenticated = integrationEloquaClient.IsAuthenticated;
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
                ViewData["ExternalFieldList"] = ExternalFields;
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
                var lstStages = db.Stages.Where(stage => stage.IsDeleted == false && stage.ClientId == Sessions.User.ClientId).Select(stage => new { Code = stage.Code, Title = stage.Title }).ToList();
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
                listGameplanDataTypeStageOne = (from intgrtn in db.IntegrationInstances
                                                join gmpln in db.GameplanDataTypes on intgrtn.IntegrationTypeId equals gmpln.IntegrationTypeId
                                                join intMap in db.IntegrationInstanceDataTypeMappings on gmpln.GameplanDataTypeId equals intMap.GameplanDataTypeId into mapping
                                                from _map in mapping.Where(map => map.IntegrationInstanceId == id).DefaultIfEmpty()
                                                where intgrtn.IntegrationInstanceId == id && gmpln.IsDeleted == false && listStageCode.Contains(gmpln.ActualFieldName)
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
                                objMapping.CreatedBy = Sessions.User.UserId;
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
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            List<GameplanDataTypeModel> listGameplanDataTypeStageZero = new List<GameplanDataTypeModel>();

            try
            {
                ExternalIntegration objEx = new ExternalIntegration(id, Sessions.ApplicationId);
                List<string> ExternalFields = objEx.GetTargetDataMember();
                if (ExternalFields == null)
                {
                    ExternalFields = new List<string>();
                }
                ViewData["ExternalFieldList"] = ExternalFields;
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

        #region Get Gampeplan DataType Pull List
        /// <summary>
        /// Get list of gameplan DataType Pull Mapping list
        /// </summary>
        /// Added by Dharmraj on 8-8-2014, Ticket #658
        /// <param name="id"></param>
        /// <returns>Returns GameplanDataTypePullModel List</returns>
        public IList<GameplanDataTypePullModel> GetGameplanDataTypePullList(int id)
        {
            List<GameplanDataTypePullModel> listGameplanDataTypePullZero = new List<GameplanDataTypePullModel>();
            try
            {
                ExternalIntegration objEx = new ExternalIntegration(id, Sessions.ApplicationId);
                List<string> ExternalFieldsCloseDeal = objEx.GetTargetDataMemberCloseDeal();
                if (ExternalFieldsCloseDeal == null)
                {
                    ExternalFieldsCloseDeal = new List<string>();
                }
                ViewData["ExternalFieldListPull"] = ExternalFieldsCloseDeal;
            }
            catch
            {
                ViewData["ExternalFieldListPull"] = new List<string>();
            }
            finally
            {
                listGameplanDataTypePullZero = GetGameplanDataTypePullListFromDB(id, Enums.GameplanDatatypePullType.CW);
            }
            return listGameplanDataTypePullZero;
        }
        #endregion

        #region Get Gampeplan DataType Pull Revenue List
        /// <summary>
        /// Get list of gameplan DataType Pull Revenue Mapping list
        /// </summary>
        /// Added by Dharmraj on 13-8-2014, Ticket #680
        /// <param name="id">Integration Instance Id</param>
        /// <returns>Returns GameplanDataTypePullModel List</returns>
        public IList<GameplanDataTypePullModel> GetGameplanDataTypePullRevenueList(int id)
        {
            List<GameplanDataTypePullModel> listGameplanDataTypePullZero = new List<GameplanDataTypePullModel>();

            try
            {
                ExternalIntegration objEx = new ExternalIntegration(id, Sessions.ApplicationId);
                List<string> ExternalFieldsCloseDeal = objEx.GetTargetDataMemberRevenue();
                if (ExternalFieldsCloseDeal == null)
                {
                    ExternalFieldsCloseDeal = new List<string>();
                }
                ViewData["ExternalFieldListPullRevenue"] = ExternalFieldsCloseDeal;
            }
            catch
            {
                ViewData["ExternalFieldListPullRevenue"] = new List<string>();
            }
            finally
            {
                listGameplanDataTypePullZero = GetGameplanDataTypePullListFromDB(id, Enums.GameplanDatatypePullType.INQ);
            }
            return listGameplanDataTypePullZero;
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

                listGameplanDataTypeStageZero.ForEach(stage => stage.DisplayFieldName = stage.DisplayFieldName.Replace("Audience", Common.CustomLabelFor(Enums.CustomLabelCode.Audience)));

                #region "Declare Enum Variables"
                //// Start - Added by :- Sohel Pathan on 03/12/2014 for PL #993
                string Campaign_EntityType = Enums.EntityType.Campaign.ToString();
                string Program_EntityType = Enums.EntityType.Program.ToString();
                string Tactic_EntityType = Enums.EntityType.Tactic.ToString();
                string Plan_Campaign = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign.ToString();
                string Plan_Campaign_Program = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program.ToString(); 
                #endregion

                //// Get GamePlan Custom Fields data.
                List<GameplanDataTypeModel> listGameplanDataTypeCustomFields = new List<GameplanDataTypeModel>();
                listGameplanDataTypeCustomFields = (from custm in db.CustomFields
                                                    join m1 in db.IntegrationInstanceDataTypeMappings on custm.CustomFieldId equals m1.CustomFieldId into mapping
                                                    from m in mapping.Where(map => map.IntegrationInstanceId == id).DefaultIfEmpty()
                                                    where custm.IsDeleted == false && custm.ClientId == Sessions.User.ClientId &&
                                                    (integrationTypeCode == Eloqua ? (custm.EntityType == Tactic_EntityType) : 1 == 1)
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
        public List<GameplanDataTypePullModel> GetGameplanDataTypePullListFromDB(int id, Enums.GameplanDatatypePullType gameplanDatatypePullType)
        {
            try
            {
                ViewBag.IntegrationInstanceId = id;
                // Get Integration instance Title
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
                //Get list of GameplanDatatypePull objects when integration instance type is Salesforce
                if (integrationTypeCode == Enums.IntegrationType.Salesforce.ToString())
                {
                    // Get list of All GameplanDataTypePullModel from DB by IntegrationInstance ID
                    string strGameplanDatatypePullType = gameplanDatatypePullType.ToString();
                    listGameplanDataTypePullZero = (from II in db.IntegrationInstances
                                                    join GDP in db.GameplanDataTypePulls on II.IntegrationTypeId equals GDP.IntegrationTypeId
                                                    join IIDMP in db.IntegrationInstanceDataTypeMappingPulls on GDP.GameplanDataTypePullId equals IIDMP.GameplanDataTypePullId into mapping
                                                    from m in mapping.Where(map => map.IntegrationInstanceId == id).DefaultIfEmpty()
                                                    where II.IntegrationInstanceId == id && GDP.Type == strGameplanDatatypePullType && GDP.IsDeleted == false
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

            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        //// Delete record from IntegrationInstanceDataTypeMapping table.
                        List<int> lstIds = mrp.IntegrationInstanceDataTypeMappings.Where(_map => _map.IntegrationInstanceId == IntegrationInstanceId).Select(_map => _map.IntegrationInstanceDataTypeMappingId).ToList();
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
                        foreach (GameplanDataTypeModel obj in form)
                        {
                            if (!string.IsNullOrEmpty(obj.TargetDataType))
                            {
                                IntegrationInstanceDataTypeMapping objMapping = new IntegrationInstanceDataTypeMapping();
                                int instanceId;
                                int.TryParse(Convert.ToString(obj.IntegrationInstanceId), out instanceId);
                                objMapping.IntegrationInstanceId = instanceId;
                                //// Start - Modified by :- Sohel Pathan on 03/12/2014 for PL #993
                                if (obj.IsCustomField.Equals(true))
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
                                objMapping.CreatedBy = Sessions.User.UserId;
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
        public JsonResult SaveDataMappingPullCloseDeal(IList<GameplanDataTypePullModel> form, int IntegrationInstanceId, string UserId = "")
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
                SaveDataMappingPull(form, IntegrationInstanceId);
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
        /// <param name="id">ID of integration instance</param>
        /// <param name="form">All values of form controls</param>
        /// <returns>Returns status = 1 and success message on success and status = 0 and failure message on error</returns>
        [HttpPost]
        public JsonResult SaveDataMappingPullRevenue(IList<GameplanDataTypePullModel> form, int IntegrationInstanceId, string UserId = "")
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
                SaveDataMappingPull(form, IntegrationInstanceId);
                return Json(new { status = 1, Message = Common.objCached.DataTypeMappingPullSaveSuccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { status = 0, Message = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Save gameplan mapping data type Pull
        /// Added By Dharmraj on 14-8-2014, #680
        /// </summary>
        /// <param name="form">List of GameplanDataTypePullModel objects</param>
        /// <param name="IntegrationInstanceId">ID of integration instance</param>
        public void SaveDataMappingPull(IList<GameplanDataTypePullModel> form, int IntegrationInstanceId)
        {
            using (MRPEntities mrp = new MRPEntities())
            {
                using (var scope = new TransactionScope())
                {
                    // Delete all old IntegrationInstanceDataTypeMappingPull entries by integrationInstanceId and GaneplanDatatypePull type
                    string GameplanDatatypePullType = form[0].Type;
                    List<int> lstIds = mrp.IntegrationInstanceDataTypeMappingPulls.Where(map => map.IntegrationInstanceId == IntegrationInstanceId && map.GameplanDataTypePull.Type == GameplanDatatypePullType).Select(map => map.IntegrationInstanceDataTypeMappingPullId).ToList();
                    if (lstIds != null && lstIds.Count > 0)
                    {
                        foreach (int ids in lstIds)
                        {
                            IntegrationInstanceDataTypeMappingPull obj = mrp.IntegrationInstanceDataTypeMappingPulls.Where(m => m.IntegrationInstanceDataTypeMappingPullId == ids).SingleOrDefault();
                            if (obj != null)
                            {
                                mrp.Entry(obj).State = EntityState.Deleted;
                            }
                        }
                        mrp.SaveChanges();

                    }

                    // Add new IntegrationInstanceDataTypeMappingPull entry for new GameplanDataTypeMappingPull
                    foreach (GameplanDataTypePullModel obj in form)
                    {
                        if (!string.IsNullOrEmpty(obj.TargetDataType))
                        {
                            IntegrationInstanceDataTypeMappingPull objMappingPull = new IntegrationInstanceDataTypeMappingPull();
                            int instanceId;
                            int.TryParse(Convert.ToString(obj.IntegrationInstanceId), out instanceId);
                            objMappingPull.IntegrationInstanceId = instanceId;
                            objMappingPull.GameplanDataTypePullId = obj.GameplanDataTypePullId;
                            objMappingPull.TargetDataType = obj.TargetDataType;
                            objMappingPull.CreatedDate = DateTime.Now;
                            objMappingPull.CreatedBy = Sessions.User.UserId;
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
                            intgrtn.ClientId == Sessions.User.ClientId).FirstOrDefault();

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
                        isDuplicate = db.IntegrationInstances.Where(intgrtn => intgrtn.Instance == form.Instance && intgrtn.ClientId == Sessions.User.ClientId && intgrtn.IntegrationType.IntegrationTypeId == form.IntegrationTypeId &&
                                        intgrtn.IsDeleted == false).Any();
                    }
                    else
                    {
                        isDuplicate = db.IntegrationInstances.Where(intgrtn => intgrtn.Instance == form.Instance && intgrtn.ClientId == Sessions.User.ClientId && intgrtn.IntegrationType.IntegrationTypeId == form.IntegrationTypeId
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
                                objIntegrationInstance.CreatedBy = Sessions.User.UserId;
                            }
                            else
                            {
                                objIntegrationInstance = db.IntegrationInstances.Where(intgrtn => intgrtn.IntegrationInstanceId == form.IntegrationInstanceId && intgrtn.IsDeleted.Equals(false) &&
                                                        intgrtn.ClientId == Sessions.User.ClientId).FirstOrDefault();
                                objIntegrationInstance.IsDeleted = (Convert.ToString(form.IsDeleted).ToLower() == "true" ? true : false);
                                objIntegrationInstance.ModifiedBy = Sessions.User.UserId;
                                objIntegrationInstance.ModifiedDate = DateTime.Now;

                                //// Start - Added by Sohel Pathan on 29/12/2014 for an Internal Review Point
                                if ((!(string.IsNullOrEmpty(objIntegrationInstance.Username))) && (!(string.IsNullOrEmpty(form.Username))) && (objIntegrationInstance.Username != form.Username))
                                {
                                    isUserNameChanged = true;
                                }
                                //// End - Added by Sohel Pathan on 29/12/2014 for an Internal Review Point
                            }

                            objIntegrationInstance.ClientId = Sessions.User.ClientId;
                            objIntegrationInstance.Instance = form.Instance;
                            objIntegrationInstance.IsImportActuals = form.IsImportActuals;
                            objIntegrationInstance.IsActive = form.IsActive;
                            objIntegrationInstance.Password = Common.Encrypt(form.Password);
                            objIntegrationInstance.Username = form.Username;

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
                                objSyncFrequency.CreatedBy = Sessions.User.UserId;
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
                                        if (form.SyncFrequency.Time.Substring(5, 2) == SyncFrequencys.PM && hour != 12)
                                            hour = hour + 12;
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
                                    DateTime nextDate = GetNextDateForDay(DateTime.Now, (DayOfWeek)Enum.Parse(typeof(DayOfWeek), objSyncFrequency.DayofWeek));
                                    TimeSpan time = (TimeSpan)objSyncFrequency.Time;
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
                                    }
                                    else if (form.SyncFrequency.Frequency == SyncFrequencys.Daily)
                                    {
                                        if (form.SyncFrequency.Time.Length == 8)
                                        {
                                            int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                            if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.PM && hour != 12)
                                                hour = hour + 12;
                                            objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                        }
                                        objSyncFrequency.DayofWeek = null;
                                        objSyncFrequency.Day = null;
                                    }
                                    else if (form.SyncFrequency.Frequency == SyncFrequencys.Weekly)
                                    {
                                        if (form.SyncFrequency.Time.Length == 8)
                                        {
                                            int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                            if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.PM && hour != 12)
                                                hour = hour + 12;
                                            objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                        }
                                        objSyncFrequency.Day = null;
                                        objSyncFrequency.DayofWeek = form.SyncFrequency.DayofWeek;
                                    }
                                    else if (form.SyncFrequency.Frequency == SyncFrequencys.Monthly)
                                    {
                                        if (form.SyncFrequency.Time.Length == 8)
                                        {
                                            int hour = Convert.ToInt16(form.SyncFrequency.Time.Substring(0, 2));
                                            if (form.SyncFrequency.Time.Substring(6, 2) == SyncFrequencys.PM && hour != 12)
                                                hour = hour + 12;
                                            objSyncFrequency.Time = new TimeSpan(hour, 0, 0);
                                        }
                                        objSyncFrequency.DayofWeek = null;
                                        objSyncFrequency.Day = form.SyncFrequency.Day;
                                    }

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
                                        DateTime nextDate = GetNextDateForDay(DateTime.Now, (DayOfWeek)Enum.Parse(typeof(DayOfWeek), objSyncFrequency.DayofWeek));
                                        TimeSpan time = (TimeSpan)objSyncFrequency.Time;
                                        objSyncFrequency.NextSyncDate = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, time.Hours, time.Minutes, time.Seconds);
                                    }
                                    else if (form.SyncFrequency.Frequency == SyncFrequencys.Monthly)
                                    {
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
                                foreach (var item in form.IntegrationTypeAttributes)
                                {
                                    IntegrationInstance_Attribute objIntegrationInstance_Attribute;
                                    if (IsAddOperation)
                                    {
                                        objIntegrationInstance_Attribute = new IntegrationInstance_Attribute();
                                        objIntegrationInstance_Attribute.CreatedBy = Sessions.User.UserId;
                                        objIntegrationInstance_Attribute.CreatedDate = DateTime.Now;
                                        objIntegrationInstance_Attribute.IntegrationInstanceId = objIntegrationInstance.IntegrationInstanceId;
                                        objIntegrationInstance_Attribute.IntegrationTypeAttributeId = item.IntegrationTypeAttributeId;
                                    }
                                    else
                                    {
                                        objIntegrationInstance_Attribute = db.IntegrationInstance_Attribute.Where(attr => attr.IntegrationInstanceId == form.IntegrationInstanceId
                                            && attr.IntegrationTypeAttributeId == item.IntegrationTypeAttributeId && attr.IntegrationInstance.IntegrationTypeId == form.IntegrationTypeId).FirstOrDefault();
                                    }

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
                                //// Remove association of Integrartion from Plan
                                db.Models.Where(_mdl => _mdl.IsDeleted.Equals(false) && _mdl.IntegrationInstanceId == form.IntegrationInstanceId).ToList().ForEach(
                                    ObjIntegrationInstance =>
                                    {
                                        ObjIntegrationInstance.IntegrationInstanceIdINQ = null;
                                        ObjIntegrationInstance.ModifiedDate = DateTime.Now;
                                        ObjIntegrationInstance.ModifiedBy = Sessions.User.UserId;
                                        db.Entry(ObjIntegrationInstance).State = EntityState.Modified;
                                    });

                                //// Identify IntegrationInstanceId for INQ in Model Table and set reference null
                                db.Models.Where(_mdl => _mdl.IsDeleted.Equals(false) && _mdl.IntegrationInstanceIdINQ == form.IntegrationInstanceId).ToList().ForEach(
                                    INQ =>
                                    {
                                        INQ.IntegrationInstanceIdINQ = null;
                                        INQ.ModifiedDate = DateTime.Now;
                                        INQ.ModifiedBy = Sessions.User.UserId;
                                        db.Entry(INQ).State = EntityState.Modified;
                                    });

                                //// Identify IntegrationInstanceId for MQL in Model Table and set reference null
                                db.Models.Where(_mdl => _mdl.IsDeleted.Equals(false) && _mdl.IntegrationInstanceIdMQL == form.IntegrationInstanceId).ToList().ForEach(
                                    MQL =>
                                    {
                                        MQL.IntegrationInstanceIdMQL = null;
                                        MQL.ModifiedDate = DateTime.Now;
                                        MQL.ModifiedBy = Sessions.User.UserId;
                                        db.Entry(MQL).State = EntityState.Modified;
                                    });

                                //// Identify IntegrationInstanceId for CW in Model Table and set reference null
                                db.Models.Where(_mdl => _mdl.IsDeleted.Equals(false) && _mdl.IntegrationInstanceIdCW == form.IntegrationInstanceId).ToList().ForEach(
                                    CW =>
                                    {
                                        CW.IntegrationInstanceIdINQ = null;
                                        CW.ModifiedDate = DateTime.Now;
                                        CW.ModifiedBy = Sessions.User.UserId;
                                        db.Entry(CW).State = EntityState.Modified;
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
                    Port = 22;
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
                    obj.SFTPPort = "22";
                }
                else
                {
                    obj.SFTPPort = frm.SFTPPort;
                }
                obj.CreatedBy = Sessions.User.UserId;
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
                        obj.SFTPPort = "22";
                    }
                    else
                    {
                        obj.SFTPPort = frm.SFTPPort;
                    }
                    obj.ModifiedBy = Sessions.User.UserId;
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
                obj.ModifiedBy = Sessions.User.UserId;
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
        private IntegrationInstanceExternalServerModel GetExternalServer(int IntegrationInstanceId)
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
    }
}

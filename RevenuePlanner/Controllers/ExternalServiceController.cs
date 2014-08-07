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
            var IntegrationInstanceList = db.IntegrationInstances
                                            .Where(ii => ii.IsDeleted.Equals(false) && ii.ClientId == Sessions.User.ClientId && ii.IntegrationType.IntegrationTypeId == ii.IntegrationTypeId)
                                            .Select(ii => ii).ToList();

            var returnList = IntegrationInstanceList.Select(a => new
            {
                IntegrationInstanceId = a.IntegrationInstanceId,
                IntegrationTypeId = a.IntegrationTypeId,
                Instance = (a.Instance == null || a.Instance.ToString() == "null") ? "" : a.Instance.ToString(),
                Provider = (a.IntegrationType.Title == null || a.IntegrationType.Title.ToString() == "null") ? "" : a.IntegrationType.Title.ToString(),
                LastSyncStatus = string.IsNullOrWhiteSpace(a.LastSyncStatus) ? "" : a.LastSyncStatus.ToString(),
                LastSyncDate = (a.LastSyncDate.HasValue ? Convert.ToDateTime(a.LastSyncDate).ToString(DateFormat) : ""),
            }).OrderByDescending(a => a.Instance).ToList();

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

            objModelToView.IntegrationTypeAttributes = db.IntegrationTypeAttributes.Where(a => a.IsDeleted.Equals(false) && a.IntegrationType.IntegrationTypeId == integrationTypeId)
                .Select(a => new IntegrationTypeAttributeModel
                {
                    Attribute = a.Attribute,
                    AttributeType = a.AttributeType,
                    IntegrationTypeAttributeId = a.IntegrationTypeAttributeId,
                    Value = "",
                })
                .ToList();

            IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();

            objIntegrationTypeModel.Title = db.IntegrationTypes.Where(a => a.IsDeleted.Equals(false) && a.IntegrationTypeId == integrationTypeId).Select(a => a.Title).FirstOrDefault();

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

            if (TestIntegrationCredentialsWithForm(form))
            {
                try
                {
                    using (var scope = new TransactionScope())
                    {
                        var isDuplicate = db.IntegrationInstances.Where(a => a.Instance == form.Instance && a.ClientId == Sessions.User.ClientId && a.IntegrationType.IntegrationTypeId == form.IntegrationTypeId).Any();

                        if (!isDuplicate)
                        {
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
                                DateTime nextDate = GetNextDateForDay(DateTime.Now,(DayOfWeek)Enum.Parse(typeof(DayOfWeek),objSyncFrequency.DayofWeek));
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
                        else
                        {
                            TempData["ErrorMessage"] = Common.objCached.IntegrationDuplicate;
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
            else
            {
                TempData["ErrorMessage"] = Common.objCached.TestIntegrationFail;
                form = reCreateView(form);
                return View(form);
            }
        }


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
            form.IntegrationTypeAttributes = db.IntegrationTypeAttributes.Where(a => a.IsDeleted.Equals(false) && a.IntegrationType.IntegrationTypeId == form.IntegrationTypeId)
                        .Select(a => new IntegrationTypeAttributeModel { Attribute = a.Attribute, AttributeType = a.AttributeType, IntegrationTypeAttributeId = a.IntegrationTypeAttributeId })
                        .ToList();

            IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
            objIntegrationTypeModel.Title = db.IntegrationTypes.Where(a => a.IsDeleted.Equals(false) && a.IntegrationTypeId == form.IntegrationTypeId).Select(a => a.Title).FirstOrDefault();
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
                objView.IntegrationTypeAttributes = db.IntegrationTypeAttributes.Where(a => a.IsDeleted.Equals(false) && a.IntegrationType.IntegrationTypeId == TypeId)
                  .Select(a => new IntegrationTypeAttributeModel
                  {
                      Attribute = a.Attribute,
                      AttributeType = a.AttributeType,
                      IntegrationTypeAttributeId = a.IntegrationTypeAttributeId,
                      Value = "",
                  })
                  .ToList();

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
                                        .Where(s => s.IntegrationInstanceId == id && s.IntegrationInstance.ClientId == Sessions.User.ClientId)
                                        .Select(s => s).FirstOrDefault();

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
                                    .Where(a => a.IntegrationTypeAttributeId == a.IntegrationTypeAttribute.IntegrationTypeAttributeId && a.IntegrationInstanceId == id && a.IntegrationInstance.ClientId == Sessions.User.ClientId)
                                    .Select(a => a).ToList();

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

            IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
            objIntegrationTypeModel.Title = db.IntegrationTypes.Where(a => a.IsDeleted.Equals(false) && a.IntegrationTypeId == IntegrationTypeId).Select(a => a.Title).FirstOrDefault();
            objView.IntegrationType = objIntegrationTypeModel;

            objView.IntegrationTypeId = IntegrationTypeId;
            ViewBag.integrationTypeId = IntegrationTypeId;

            populateSyncFreqData();
            if (id > 0)
            {
                objView.GameplanDataTypeModelList = GetGameplanDataTypeList(id);   // Added by Sohel Pathan on 05/08/2014 for PL ticket #656 and #681
                objView.ExternalServer = GetExternalServer(id);    
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

            if (TestIntegrationCredentialsWithForm(form))
            {
                try
                {
                    var isDuplicate = db.IntegrationInstances.Where(a => a.Instance == form.Instance && a.ClientId == Sessions.User.ClientId && a.IntegrationType.IntegrationTypeId == form.IntegrationTypeId
                        && a.IntegrationInstanceId != form.IntegrationInstanceId).Any();

                    if (!isDuplicate)
                    {
                        bool IntegrationRemoved = true;
                        int SyncFrequenciesCount = 0, IntegrationInstancesCount = 0;

                        using (var scope = new TransactionScope())
                        {
                            // update data
                            IntegrationInstance objIntegrationInstance = db.IntegrationInstances.Where(a => a.IntegrationInstanceId == form.IntegrationInstanceId && a.IsDeleted.Equals(false) &&
                                a.ClientId == Sessions.User.ClientId).FirstOrDefault();

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

                                SyncFrequency objSyncFrequency = db.SyncFrequencies.Where(a => a.IntegrationInstanceId == form.IntegrationInstanceId && a.IntegrationInstance.IntegrationInstanceId == form.IntegrationInstanceId).FirstOrDefault();

                                if (objSyncFrequency != null)
                                {
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

                                if (form.IntegrationTypeAttributes != null)
                                {
                                    foreach (var item in form.IntegrationTypeAttributes)
                                    {
                                        IntegrationInstance_Attribute objIntegrationInstance_Attribute = db.IntegrationInstance_Attribute.Where(a => a.IntegrationInstanceId == form.IntegrationInstanceId
                                                && a.IntegrationTypeAttributeId == item.IntegrationTypeAttributeId && a.IntegrationInstance.IntegrationTypeId == form.IntegrationTypeId).FirstOrDefault();

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

                                var objModelList = db.Models.Where(a => a.IsDeleted.Equals(false) && a.IntegrationInstanceId == form.IntegrationInstanceId).ToList();

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
                    else
                    {
                        TempData["ErrorMessage"] = Common.objCached.IntegrationDuplicate;
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
            else
            {
                TempData["ErrorMessage"] = Common.objCached.TestIntegrationFail;
                form = reCreateView(form);
                return View(form);
            }
        }

        #endregion
        
        /// <summary>
        /// Sync service
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult SyncNow(int id, string UserId = "")
        {
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
                ExternalIntegration externalIntegration = new ExternalIntegration(id, Sessions.User.UserId);
                externalIntegration.Sync();
                IntegrationInstance integrationInstance = db.IntegrationInstances.Single(instance => instance.IntegrationInstanceId.Equals(id));
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

        public bool TestIntegrationCredentialsWithForm(IntegrationModel form)
        {
            bool isAuthenticated = false;

            if (form.Instance != "" && form.Username != "" && form.Password != "")
            {
                if (form.IntegrationType.Title.Equals(Integration.Helper.Enums.IntegrationType.Eloqua.ToString()))
                {
                    string eloqua = Integration.Helper.Enums.IntegrationType.Eloqua.ToString();
                    IntegrationEloquaClient integrationEloquaClient = new IntegrationEloquaClient();
                    integrationEloquaClient._instance = form.Instance;// "TechnologyPartnerBulldog";
                    integrationEloquaClient._username = form.Username;// "Brij.Bhavsar";
                    integrationEloquaClient._password = form.Password;//"Brij1234";
                    RevenuePlanner.Models.IntegrationType integrationType = db.IntegrationTypes.Single(intgtype => intgtype.Title.Equals(eloqua));
                    integrationEloquaClient._apiURL = integrationType.APIURL;
                    integrationEloquaClient._apiVersion = integrationType.APIVersion;
                    integrationEloquaClient.Authenticate();
                    isAuthenticated = integrationEloquaClient.IsAuthenticated;
                }
                else if (form.IntegrationType.Title.Equals(Integration.Helper.Enums.IntegrationType.Salesforce.ToString()) && form.IntegrationTypeAttributes != null)
                {
                    if (form.IntegrationTypeAttributes.Count > 0)
                    {
                        List<int> attributeIds = form.IntegrationTypeAttributes.Select(attr => attr.IntegrationTypeAttributeId).ToList();
                        List<IntegrationTypeAttribute> integrationTypeAttributes = db.IntegrationTypeAttributes.Where(attr => attributeIds.Contains(attr.IntegrationTypeAttributeId)).ToList();
                        string consumerKey = string.Empty;
                        string consumerSecret = string.Empty;
                        string securityToken = string.Empty;

                        foreach (IntegrationTypeAttribute integrationTypeAttribute in integrationTypeAttributes)
                        {
                            if (integrationTypeAttribute.Attribute.Equals("ConsumerKey"))
                            {
                                consumerKey = form.IntegrationTypeAttributes.Single(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                            }
                            else if (integrationTypeAttribute.Attribute.Equals("ConsumerSecret"))
                            {
                                consumerSecret = form.IntegrationTypeAttributes.Single(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                            }
                            else if (integrationTypeAttribute.Attribute.Equals("SecurityToken"))
                            {
                                securityToken = form.IntegrationTypeAttributes.Single(attr => attr.IntegrationTypeAttributeId.Equals(integrationTypeAttribute.IntegrationTypeAttributeId)).Value;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(consumerKey) && !string.IsNullOrWhiteSpace(consumerSecret) && !string.IsNullOrWhiteSpace(securityToken))
                        {
                            string salesforce = Integration.Helper.Enums.IntegrationType.Salesforce.ToString();
                            IntegrationSalesforceClient integrationSalesforceClient = new IntegrationSalesforceClient();
                            integrationSalesforceClient._username = form.Username;
                            integrationSalesforceClient._password = form.Password;
                            integrationSalesforceClient._consumerKey = consumerKey;
                            integrationSalesforceClient._consumerSecret = consumerSecret;
                            integrationSalesforceClient._securityToken = securityToken;
                            integrationSalesforceClient._apiURL = db.IntegrationTypes.Single(intgtype => intgtype.Title.Equals(salesforce)).APIURL;
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
        [AuthorizeUser(Enums.ApplicationActivity.IntegrationCredentialCreateEdit)]  // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult MapDataTypes(int id)
        {
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

            //if (!Sessions.IsClientAdmin && !Sessions.IsSystemAdmin)
            //{
            //    return RedirectToAction("Index", "NoAccess");
            //}
            try
            {
                ExternalIntegration objEx = new ExternalIntegration(id);
                List<string> ExternalFields = objEx.GetTargetDataMember();
                if (ExternalFields == null)
                {
                    ExternalFields = new List<string>();
                }
                ViewData["ExternalFieldList"] = ExternalFields;
                ViewBag.IntegrationInstanceId = id;
                string integrationTypeName = (from i in db.IntegrationInstances
                                              join t in db.IntegrationTypes on i.IntegrationTypeId equals t.IntegrationTypeId
                                              where i.IsDeleted == false && t.IsDeleted == false && i.IntegrationInstanceId == id
                                              select t.Title).SingleOrDefault();
                if (string.IsNullOrEmpty(integrationTypeName)) ViewBag.IntegrationTypeName = ""; else ViewBag.IntegrationTypeName = integrationTypeName;

                //// Start - Added by :- Sohel Pathan on 28/05/2014 for PL #494 filter gameplan datatype by client id 
                var lstStages = db.Stages.Where(a => a.IsDeleted == false && a.ClientId == Sessions.User.ClientId).Select(a => new { Code = a.Code, Title = a.Title }).ToList();
                var listStageCode = lstStages.Select(s => s.Code).ToList();

                List<GameplanDataTypeModel> listGameplanDataTypeStageZero = new List<GameplanDataTypeModel>();
                listGameplanDataTypeStageZero = (from i in db.IntegrationInstances
                                                 join d in db.GameplanDataTypes on i.IntegrationTypeId equals d.IntegrationTypeId
                                                 join m1 in db.IntegrationInstanceDataTypeMappings on d.GameplanDataTypeId equals m1.GameplanDataTypeId into mapping
                                                 from m in mapping.Where(map => map.IntegrationInstanceId == id).DefaultIfEmpty()
                                                 where i.IntegrationInstanceId == id && d.IsDeleted == false && d.IsStage == false
                                                 select new GameplanDataTypeModel
                                                 {
                                                     GameplanDataTypeId = d.GameplanDataTypeId,
                                                     IntegrationTypeId = d.IntegrationTypeId,
                                                     TableName = d.TableName,
                                                     ActualFieldName = d.ActualFieldName,
                                                     DisplayFieldName = d.DisplayFieldName,
                                                     IsGet = d.IsGet,
                                                     IntegrationInstanceDataTypeMappingId = m.IntegrationInstanceDataTypeMappingId,
                                                     IntegrationInstanceId = i.IntegrationInstanceId,
                                                     TargetDataType = m.TargetDataType
                                                 }).ToList();

                List<GameplanDataTypeModel> listGameplanDataTypeStageOne = new List<GameplanDataTypeModel>();
                listGameplanDataTypeStageOne = (from i in db.IntegrationInstances
                                                 join d in db.GameplanDataTypes on i.IntegrationTypeId equals d.IntegrationTypeId
                                                 join m1 in db.IntegrationInstanceDataTypeMappings on d.GameplanDataTypeId equals m1.GameplanDataTypeId into mapping
                                                 from m in mapping.Where(map => map.IntegrationInstanceId == id).DefaultIfEmpty()
                                                where i.IntegrationInstanceId == id && d.IsDeleted == false && d.IsStage == true && listStageCode.Contains(d.ActualFieldName)
                                                 select new GameplanDataTypeModel
                                                 {
                                                     GameplanDataTypeId = d.GameplanDataTypeId,
                                                     IntegrationTypeId = d.IntegrationTypeId,
                                                     TableName = d.TableName,
                                                     ActualFieldName = d.ActualFieldName,
                                                     DisplayFieldName = d.DisplayFieldName,
                                                     IsGet = d.IsGet,
                                                     IntegrationInstanceDataTypeMappingId = m.IntegrationInstanceDataTypeMappingId,
                                                     IntegrationInstanceId = i.IntegrationInstanceId,
                                                     TargetDataType = m.TargetDataType
                                                 }).ToList();

                foreach (var item in listGameplanDataTypeStageOne)
                {
                    item.DisplayFieldName = item.DisplayFieldName + "[" + lstStages.Where(a => a.Code == item.ActualFieldName).Select(a => a.Title).FirstOrDefault() + "]";
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
                        List<int> lstIds = mrp.IntegrationInstanceDataTypeMappings.Where(m => m.IntegrationInstanceId == id).Select(m => m.IntegrationInstanceDataTypeMappingId).ToList();
                        if (lstIds != null && lstIds.Count > 0)
                        {
                            foreach (int ids in lstIds)
                            {
                                IntegrationInstanceDataTypeMapping obj = mrp.IntegrationInstanceDataTypeMappings.Where(m => m.IntegrationInstanceDataTypeMappingId == ids).SingleOrDefault();
                                if (obj != null)
                                {
                                    mrp.Entry(obj).State = EntityState.Deleted;
                                    mrp.SaveChanges();
                                }
                            }

                        }
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
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<GameplanDataTypeModel> GetGameplanDataTypeList(int id)
        {
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            
            List<GameplanDataTypeModel> listGameplanDataTypeStageZero = new List<GameplanDataTypeModel>();

            try
            {
                ExternalIntegration objEx = new ExternalIntegration(id);
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

        #region Function get Gameplan DataTypes list from DB.
        /// <summary>
        /// Get gameplan datatype list from database based on IntegrationInstanceId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<GameplanDataTypeModel> GetGameplanDataTypeListFromDB(int id)
        {
            try
            {
                ViewBag.IntegrationInstanceId = id;
                string integrationTypeName = (from i in db.IntegrationInstances
                                              join t in db.IntegrationTypes on i.IntegrationTypeId equals t.IntegrationTypeId
                                              where i.IsDeleted == false && t.IsDeleted == false && i.IntegrationInstanceId == id
                                              select t.Title).SingleOrDefault();
                if (string.IsNullOrEmpty(integrationTypeName))
                {
                    ViewBag.IntegrationTypeName = "";
                }
                else
                {
                    ViewBag.IntegrationTypeName = integrationTypeName;
                }

                TempData["TargetFieldInvalidMsg"] = Common.objCached.TargetFieldInvalidMsg;

                //// Start - Added by :- Sohel Pathan on 28/05/2014 for PL #494 filter gameplan datatype by client id 
                string Eloqua = Enums.IntegrationType.Eloqua.ToString();
                string Plan_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Campaign_Program_Tactic.ToString();
                string Plan_Improvement_Campaign_Program_Tactic = Enums.IntegrantionDataTypeMappingTableName.Plan_Improvement_Campaign_Program_Tactic.ToString();

                List<GameplanDataTypeModel> listGameplanDataTypeStageZero = new List<GameplanDataTypeModel>();
                listGameplanDataTypeStageZero = (from i in db.IntegrationInstances
                                                 join d in db.GameplanDataTypes on i.IntegrationTypeId equals d.IntegrationTypeId
                                                 join m1 in db.IntegrationInstanceDataTypeMappings on d.GameplanDataTypeId equals m1.GameplanDataTypeId into mapping
                                                 from m in mapping.Where(map => map.IntegrationInstanceId == id).DefaultIfEmpty()
                                                 where i.IntegrationInstanceId == id && d.IsDeleted == false //&& d.IsStage == false
                                                 && d.IsGet != true &&
                                                 (integrationTypeName == Eloqua ? (d.TableName == Plan_Campaign_Program_Tactic || d.TableName == Plan_Improvement_Campaign_Program_Tactic) : 1 == 1)
                                                 select new GameplanDataTypeModel
                                                 {
                                                     GameplanDataTypeId = d.GameplanDataTypeId,
                                                     IntegrationTypeId = d.IntegrationTypeId,
                                                     TableName = d.TableName,
                                                     ActualFieldName = d.ActualFieldName,
                                                     DisplayFieldName = d.DisplayFieldName,
                                                     IsGet = d.IsGet,
                                                     IntegrationInstanceDataTypeMappingId = m.IntegrationInstanceDataTypeMappingId,
                                                     IntegrationInstanceId = i.IntegrationInstanceId,
                                                     TargetDataType = m.TargetDataType
                                                 }).ToList();

                if (listGameplanDataTypeStageZero != null && listGameplanDataTypeStageZero.Count > 0)
                {
                    return listGameplanDataTypeStageZero.OrderBy(map => map.TableName).ToList();
                }
                //// End - Added by :- Sohel Pathan on 28/05/2014 for PL #494 filter gameplan datatype by client id
                else
                {
                    TempData["DataMappingErrorMessage"] = Common.objCached.DataTypeMappingNotConfigured;
                    return listGameplanDataTypeStageZero = new List<GameplanDataTypeModel>();
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

        #region Save DataType Mapping using Ajax call
        /// <summary>
        /// Save gameplan mapping data type using ajax call
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>05/08/2014</CreatedDate>
        /// <param name="id"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveDataMapping(IList<GameplanDataTypeModel> form, int IntegrationInstanceId, string UserId = "")
        {
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
                        List<int> lstIds = mrp.IntegrationInstanceDataTypeMappings.Where(m => m.IntegrationInstanceId == IntegrationInstanceId).Select(m => m.IntegrationInstanceDataTypeMappingId).ToList();
                        if (lstIds != null && lstIds.Count > 0)
                        {
                            foreach (int ids in lstIds)
                            {
                                IntegrationInstanceDataTypeMapping obj = mrp.IntegrationInstanceDataTypeMappings.Where(m => m.IntegrationInstanceDataTypeMappingId == ids).SingleOrDefault();
                                if (obj != null)
                                {
                                    mrp.Entry(obj).State = EntityState.Deleted;
                                    mrp.SaveChanges();
                                }
                            }

                        }
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
                return Json(new { status = 1, Message = Common.objCached.DataTypeMappingSaveSuccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { status = 0, Message = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);
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
        /// /// Added By : Kalpesh Sharma Save the value of General Settings in Integration #682
        /// </summary>
        /// <param name="form"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public int SaveIntegrationSettings(IntegrationModel form, ref string message, ref int ID)
        {
            int Status = 0;
            bool IsAddOperation = (form.IntegrationInstanceId == 0 ? true : false);
            int objIntegrationInstanceId = 0;

            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

            if (TestIntegrationCredentialsWithForm(form))
            {
                try
                {
                    var isDuplicate = (dynamic)null;

                    if (IsAddOperation)
                    {
                        isDuplicate = db.IntegrationInstances.Where(a => a.Instance == form.Instance && a.ClientId == Sessions.User.ClientId && a.IntegrationType.IntegrationTypeId == form.IntegrationTypeId &&
                            a.IsDeleted == false).Any();
                    }
                    else
                    {
                        isDuplicate = db.IntegrationInstances.Where(a => a.Instance == form.Instance && a.ClientId == Sessions.User.ClientId && a.IntegrationType.IntegrationTypeId == form.IntegrationTypeId
                         && a.IntegrationInstanceId != form.IntegrationInstanceId).Any();
                    }

                    if ((!isDuplicate && form.IntegrationInstanceId == 0) || form.IsDeleted || form.IntegrationInstanceId > 0)
                    {
                        bool IntegrationRemoved = true;
                        int SyncFrequenciesCount = 0, IntegrationInstancesCount = 0;

                        using (var scope = new TransactionScope())
                        {
                            // update data
                            IntegrationInstance objIntegrationInstance;

                            if (IsAddOperation)
                            {
                                objIntegrationInstance = new IntegrationInstance();
                                objIntegrationInstance.CreatedDate = DateTime.Now;
                                objIntegrationInstance.IsDeleted = false;
                                objIntegrationInstance.IntegrationTypeId = form.IntegrationTypeId;
                            }
                            else
                            {
                                objIntegrationInstance = db.IntegrationInstances.Where(a => a.IntegrationInstanceId == form.IntegrationInstanceId && a.IsDeleted.Equals(false) &&
                                a.ClientId == Sessions.User.ClientId).FirstOrDefault();
                                objIntegrationInstance.IsDeleted = (Convert.ToString(form.IsDeleted).ToLower() == "true" ? true : false);
                            }

                            objIntegrationInstance.ClientId = Sessions.User.ClientId;
                            objIntegrationInstance.ModifiedBy = Sessions.User.UserId;
                            objIntegrationInstance.ModifiedDate = DateTime.Now;
                            objIntegrationInstance.Instance = form.Instance;
                            objIntegrationInstance.IsImportActuals = form.IsImportActuals;
                            objIntegrationInstance.IsActive = form.IsActive;
                            objIntegrationInstance.Password = Common.Encrypt(form.Password);
                            objIntegrationInstance.Username = form.Username;

                            db.Entry(objIntegrationInstance).State = (IsAddOperation ? System.Data.EntityState.Added : System.Data.EntityState.Modified);

                            if (IsAddOperation)
                            {
                                db.IntegrationInstances.Add(objIntegrationInstance);
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

                            }
                            else
                            {
                                objSyncFrequency = db.SyncFrequencies.Where(a => a.IntegrationInstanceId == form.IntegrationInstanceId && a.IntegrationInstance.IntegrationInstanceId == form.IntegrationInstanceId).FirstOrDefault();

                                if (objSyncFrequency != null)
                                {

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
                                }
                            }

                            objSyncFrequency.Frequency = form.SyncFrequency.Frequency;
                            db.Entry(objSyncFrequency).State = (IsAddOperation ? System.Data.EntityState.Added : System.Data.EntityState.Modified);
                            if (IsAddOperation)
                            {
                                db.SyncFrequencies.Add(objSyncFrequency);
                            }
                            SyncFrequenciesCount = db.SaveChanges();


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
                                        objIntegrationInstance_Attribute = db.IntegrationInstance_Attribute.Where(a => a.IntegrationInstanceId == form.IntegrationInstanceId
                                            && a.IntegrationTypeAttributeId == item.IntegrationTypeAttributeId && a.IntegrationInstance.IntegrationTypeId == form.IntegrationTypeId).FirstOrDefault();
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

                            if (Convert.ToString(form.IsDeleted).ToLower() == "true" && !IsAddOperation)
                            {
                                IntegrationRemoved = Common.DeleteIntegrationInstance(form.IntegrationInstanceId, true);
                            }

                            // Status changed from Active to InActive, So remove all the integration dependency with Models.
                            if (form.IsActiveStatuChanged == true && form.IsActive == false && !IsAddOperation)
                            {
                                // Remove association of Integrartion from Plan

                                var objModelList = db.Models.Where(a => a.IsDeleted.Equals(false) && a.IntegrationInstanceId == form.IntegrationInstanceId).ToList();

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

                        if (IntegrationInstancesCount > 0 && SyncFrequenciesCount > 0)
                        {
                            if (IsAddOperation)
                            {
                                message = Common.objCached.IntegrationAdded;
                                Status = 1;
                            }
                            else if (Convert.ToString(form.IsDeleted).ToLower() == "true" && IntegrationRemoved != false)
                            {
                                message = Common.objCached.IntegrationDeleted;
                                Status = 3;
                            }
                            else if (!IsAddOperation)
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
                obj = db.IntegrationInstanceExternalServers.Where(i => i.IsDeleted == false && i.SFTPFileLocation.ToLower() == SFTPFileLocation.ToLower()).SingleOrDefault();
            }
            else
            {
                obj = db.IntegrationInstanceExternalServers.Where(i => i.IntegrationInstanceExternalServerId != IntegrationInstanceExternalServerId && i.IsDeleted == false && i.SFTPFileLocation.ToLower() == SFTPFileLocation.ToLower()).SingleOrDefault();
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
                    //db.IntegrationInstanceExternalServers.Add(obj);
                    db.SaveChanges();
                    return obj.IntegrationInstanceExternalServerId;
                }
            }
            return 1;
        }

        /// <summary>
        /// Delete the data
        /// </summary>
        /// <param name="frm"></param>
        /// <returns></returns>
        private int DeleteExternalServer(int IntegrationInstanceId)
        {
            IntegrationInstanceExternalServer obj = db.IntegrationInstanceExternalServers.Where(i => i.IntegrationInstanceId == IntegrationInstanceId).FirstOrDefault();
            if (obj != null)
            {
                db.Entry(obj).State = System.Data.EntityState.Deleted;
                return db.SaveChanges();
            }
            return 1;
        }

        /// <summary>
        /// Add or Update the data
        /// </summary>
        /// <param name="frm"></param>
        /// <returns></returns>
        private IntegrationInstanceExternalServerModel GetExternalServer(int IntegrationInstanceId)
        {
            IntegrationInstanceExternalServerModel model = new IntegrationInstanceExternalServerModel();
            model.IntegrationInstanceId = IntegrationInstanceId;
            IntegrationInstanceExternalServer obj = db.IntegrationInstanceExternalServers.Where(i => i.IntegrationInstanceId == IntegrationInstanceId && i.IsDeleted == false).FirstOrDefault();
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
    }
}

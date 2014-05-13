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

namespace RevenuePlanner.Controllers
{
    public class ExternalServiceController : Controller
    {
        #region Variables
        private MRPEntities db = new MRPEntities();
        #endregion

        public ActionResult Index()
        {
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
                LastSyncStatus = (a.LastSyncStatus == null || a.LastSyncStatus.ToString() == "null") ? "" : a.LastSyncStatus.ToString(),
                LastSyncDate = (a.LastSyncDate == null || a.LastSyncDate.ToString() == "null") ? "" : a.LastSyncDate.ToString(),
            }).OrderByDescending(a => a.Instance).ToList();

            return Json(returnList, JsonRequestBehavior.AllowGet);
        }

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

        public ActionResult create(int integrationTypeId)
        {
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

            return View(objModelToView);
        }

        [HttpPost]
        public ActionResult create(IntegrationModel form)
        {
            ViewBag.integrationTypeId = form.IntegrationTypeId;

            if (TestIntegrationCredentials(form.Instance, form.Username, form.Password))
            {
                try
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
        }

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

        public ActionResult edit(int id)
        {
            var record = db.IntegrationInstances
                                    .Where(ii => ii.IsDeleted.Equals(false) && ii.ClientId == Sessions.User.ClientId && ii.IntegrationInstanceId == id)
                                    .Select(ii => ii).FirstOrDefault();

            IntegrationModel objView = new IntegrationModel();
            objView.Instance = record.Instance;
            objView.Username = record.Username;
            objView.Password = Common.Decrypt(record.Password);
            objView.IntegrationInstanceId = record.IntegrationInstanceId;
            objView.IntegrationTypeId = record.IntegrationTypeId;
            objView.IsActive = record.IsActive;
            objView.IsImportActuals = record.IsImportActuals;

            var IntegrationTypeId = record.IntegrationTypeId;

            var recordSync = db.SyncFrequencies
                                    .Where(s => s.IntegrationInstanceId == id && s.IntegrationInstance.ClientId == Sessions.User.ClientId) 
                                    .Select(s => s).FirstOrDefault();

            SyncFrequencyModel objSync = new SyncFrequencyModel();
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

            IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
            objIntegrationTypeModel.Title = db.IntegrationTypes.Where(a => a.IsDeleted.Equals(false) && a.IntegrationTypeId == IntegrationTypeId).Select(a => a.Title).FirstOrDefault();
            objView.IntegrationType = objIntegrationTypeModel;

            populateSyncFreqData();
            
            return View(objView);
        }

        [HttpPost]
        public ActionResult edit(IntegrationModel form)
        {
            ViewBag.integrationTypeId = form.IntegrationTypeId;

            try
            {
                var isDuplicate = db.IntegrationInstances.Where(a => a.Instance == form.Instance && a.ClientId == Sessions.User.ClientId && a.IntegrationType.IntegrationTypeId == form.IntegrationTypeId
                    && a.IntegrationInstanceId != form.IntegrationInstanceId).Any();

                if (!isDuplicate)
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
                        if (form.IsDeleted != null)
                        {
                            if (Convert.ToString(form.IsDeleted).ToLower() == "true")
                            {
                                objIntegrationInstance.IsDeleted = true;
                            }
                            else
                            {
                                objIntegrationInstance.IsDeleted = false;
                            }
                        }
                        objIntegrationInstance.IsImportActuals = form.IsImportActuals;
                        objIntegrationInstance.IsActive = form.IsActive;
                        objIntegrationInstance.Password = Common.Encrypt(form.Password);
                        objIntegrationInstance.Username = form.Username;
                        db.Entry(objIntegrationInstance).State = System.Data.EntityState.Modified;
                        int IntegrationInstancesCount = db.SaveChanges();

                        SyncFrequency objSyncFrequency = db.SyncFrequencies.Where(a => a.IntegrationInstanceId == form.IntegrationInstanceId && a.IntegrationInstance.IntegrationInstanceId == form.IntegrationInstanceId).FirstOrDefault();
                        
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
                        
                        db.Entry(objSyncFrequency).State = System.Data.EntityState.Modified;
                        int SyncFrequenciesCount = db.SaveChanges();

                        if (form.IntegrationTypeAttributes != null)
                        {
                            foreach (var item in form.IntegrationTypeAttributes)
                            {
                                IntegrationInstance_Attribute objIntegrationInstance_Attribute = db.IntegrationInstance_Attribute.Where(a => a.IntegrationInstanceId == form.IntegrationInstanceId
                                        && a.IntegrationTypeAttributeId == item.IntegrationTypeAttributeId && a.IntegrationInstance.IntegrationTypeId == form.IntegrationTypeId).FirstOrDefault();

                                objIntegrationInstance_Attribute.Value = item.Value;
                                db.Entry(objIntegrationInstance_Attribute).State = System.Data.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }

                        if (IntegrationInstancesCount > 0 && SyncFrequenciesCount > 0)
                        {
                            if (Convert.ToString(form.IsDeleted).ToLower() == "true")
                            {
                                TempData["SuccessMessage"] = Common.objCached.IntegrationDeleted;
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                TempData["SuccessMessage"] = Common.objCached.IntegrationEdited;
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

        public JsonResult SyncNow(int id)
        {
            try
            {
                //ExternalIntegration externalIntegration = new ExternalIntegration(id);
                //externalIntegration.Sync();
                return Json(new { status = "Active" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { status = "Error" }, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult TestIntegration(string instanceName, string userName, string password)
        {
            if (TestIntegrationCredentials(instanceName, userName, password))
            {
                return Json(new { status = 1, SuccessMessage = Common.objCached.TestIntegrationSuccess }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = 0, ErrorMessage = Common.objCached.TestIntegrationFail }, JsonRequestBehavior.AllowGet);
            }
        }

        public bool TestIntegrationCredentials(string instanceName, string userName, string password)
        {
            return true;
        }

        #region Map Data Types

        /// <summary>
        /// Map External Service - Gameplan Data Types (Fields)
        /// </summary>
        public ActionResult MapDataTypes(int id)
        {
            if (!Sessions.IsClientAdmin && !Sessions.IsSystemAdmin)
            {
                return RedirectToAction("Index", "NoAccess");
            }
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

                List<GameplanDataTypeModel> listGameplanDataTypeModel = new List<GameplanDataTypeModel>();
                listGameplanDataTypeModel = (from i in db.IntegrationInstances
                                             join d in db.GameplanDataTypes on i.IntegrationTypeId equals d.IntegrationTypeId
                                             join m1 in db.IntegrationInstanceDataTypeMappings on d.GameplanDataTypeId equals m1.GameplanDataTypeId into mapping
                                             from m in mapping.Where(map => map.IntegrationInstanceId == id).DefaultIfEmpty()
                                             where i.IntegrationInstanceId == id && d.IsDeleted == false
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
                                             }
                                             ).ToList();
                if (listGameplanDataTypeModel != null && listGameplanDataTypeModel.Count > 0)
                {
                    return View(listGameplanDataTypeModel.OrderBy(map => map.DisplayFieldName).ToList());
                }
                else
                {
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
        public ActionResult MapDataTypes(int id, IList<GameplanDataTypeModel> form)
        {
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

        #endregion

    }
}

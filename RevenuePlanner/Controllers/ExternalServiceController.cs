using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
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
            }).OrderByDescending(a => a.Provider).ToList();

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

            objModelToView.IntegrationTypeAttributes = db.IntegrationTypeAttributes.Where(a => a.IsDeleted.Equals(false) && a.IntegrationTypeId == integrationTypeId)
                .Select(a => new IntegrationTypeAttributeModel { Attribute = a.Attribute, AttributeType = a.AttributeType, IntegrationTypeAttributeId = a.IntegrationTypeAttributeId })
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
                    var password = form.Password;
                    objIntegrationInstance.Password = form.Password; // Common.ComputeSingleHash(password); //Single hash password
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
                        if (form.SyncFrequency.Time.Length == 7)
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

                    foreach (var item in form.IntegrationTypeAttributes)
                    {
                        IntegrationInstance_Attribute objIntegrationInstance_Attribute = new IntegrationInstance_Attribute();
                        objIntegrationInstance_Attribute.CreatedBy = Sessions.User.UserId;
                        objIntegrationInstance_Attribute.CreatedDate = DateTime.Now;
                        objIntegrationInstance_Attribute.IntegrationInstanceId = objIntegrationInstance.IntegrationInstanceId;
                        objIntegrationInstance_Attribute.IntegrationTypeAttributeId = item.IntegrationTypeAttributeId;
                        objIntegrationInstance_Attribute.Value = item.Attribute;
                        db.Entry(objIntegrationInstance_Attribute).State = System.Data.EntityState.Added;
                        db.IntegrationInstance_Attribute.Add(objIntegrationInstance_Attribute);
                    }

                    int IntegrationInstance_AttributeCount = db.SaveChanges();

                    if (IntegrationInstance_AttributeCount > 0 && IntegrationInstancesCount > 0 && SyncFrequenciesCount > 0)
                    {
                        TempData["SuccessMessage"] = Common.objCached.UserAdded;
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
                    TempData["ErrorMessage"] = Common.objCached.UserDuplicate;
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
                objTime.Text = dtToday.ToString("hh : tt");
                objTime.Value = dtToday.ToString("hh : tt");
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
        }

        public IntegrationModel reCreateView(IntegrationModel form)
        {
            form.IntegrationTypeAttributes = db.IntegrationTypeAttributes.Where(a => a.IsDeleted.Equals(false) && a.IntegrationTypeId == form.IntegrationTypeId)
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
            objView.Password = record.Password;
            objView.IntegrationInstanceId = record.IntegrationInstanceId;
            objView.IntegrationTypeId = record.IntegrationTypeId;
            objView.IsActive = record.IsActive;
            objView.IsImportActuals = record.IsImportActuals;

            var IntegrationTypeId = record.IntegrationTypeId;

            var recordSync = db.SyncFrequencies
                                    .Where(s => s.IntegrationInstanceId == id) 
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
                    objSync.Time = recordSync.Time.Value.Hours.ToString("hh") + " : " + "PM";
                else
                    objSync.Time = recordSync.Time.Value.Hours.ToString("hh") + " : " + "AM";
            }
            objSync.IntegrationInstanceId = recordSync.IntegrationInstanceId;

            objView.SyncFrequency = objSync;

            var recordAttribute = db.IntegrationInstance_Attribute
                                .Where(a => a.IntegrationTypeAttributeId == a.IntegrationTypeAttribute.IntegrationTypeAttributeId && a.IntegrationInstanceId == id)
                                .Select(a => a).ToList();

            List<IntegrationInstance_AttributeModel> lstObjIntegrationInstance_AttributeModel = new List<IntegrationInstance_AttributeModel>();
            List<IntegrationTypeAttributeModel> lstObjIntegrationTypeAttributeModel = new List<IntegrationTypeAttributeModel>();
            foreach (var item in recordAttribute)
            {
                IntegrationInstance_AttributeModel objIntegrationInstance_AttributeModel = new IntegrationInstance_AttributeModel();
                objIntegrationInstance_AttributeModel.IntegrationInstanceId = item.IntegrationInstanceId;
                objIntegrationInstance_AttributeModel.IntegrationTypeAttributeId = item.IntegrationTypeAttributeId;
                objIntegrationInstance_AttributeModel.Value = item.Value;
                lstObjIntegrationInstance_AttributeModel.Add(objIntegrationInstance_AttributeModel);

                IntegrationTypeAttributeModel objIntegrationTypeAttributeModel = new IntegrationTypeAttributeModel();
                objIntegrationTypeAttributeModel.Attribute = item.IntegrationTypeAttribute.Attribute;
                objIntegrationTypeAttributeModel.AttributeType = item.IntegrationTypeAttribute.AttributeType;
                objIntegrationTypeAttributeModel.IntegrationTypeAttributeId = item.IntegrationTypeAttribute.IntegrationTypeAttributeId;
                objIntegrationTypeAttributeModel.IntegrationTypeId = item.IntegrationTypeAttribute.IntegrationTypeId;
                lstObjIntegrationTypeAttributeModel.Add(objIntegrationTypeAttributeModel);
            }

            objView.IntegrationTypeAttributes = lstObjIntegrationTypeAttributeModel;
            objView.IntegrationInstance_Attribute = lstObjIntegrationInstance_AttributeModel;

            IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
            objIntegrationTypeModel.Title = db.IntegrationTypes.Where(a => a.IsDeleted.Equals(false) && a.IntegrationTypeId == IntegrationTypeId).Select(a => a.Title).FirstOrDefault();
            objView.IntegrationType = objIntegrationTypeModel;

            populateSyncFreqData();

            return View(objView);
        }


    }
}


using Elmah;
using Newtonsoft.Json;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Reflection;
using System.Transactions;
using System.Web.Mvc;
using System.Xml;
using System.Text.RegularExpressions;
using RevenuePlanner.BDSService;

namespace RevenuePlanner.Controllers
{
    /// <summary>
    /// Model controller 
    /// </summary>
    public class ModelController : CommonController
    {
        #region Variables

        private MRPEntities db = new MRPEntities();
        const string CR = "CR";
        const string SV = "SV";
        const string ModelDraft = "draft";
        const string ModelPublished = "published";
        static Random rnd = new Random();
        int NumberOfTouches, Acquisition_NumberofTouches, ListAcquisitions;
        double NormalErosion, UnsubscribeRate, CTRDelivered, RegistrationRate, ListAcquisitionsNormalErosion, ListAcquisitionsUnsubscribeRate, ListAcquisitionsCTRDelivered,
            Acquisition_CostperContact, Acquisition_RegistrationRate;

        int Impressions, TP_PrintMediaBudget, TDM_DigitalMediaBudget, CSC_NonGuaranteedCPLBudget, GC_GuaranteedCPLBudget, PPC_ClickThroughs;
        double PPC_CostperClickThrough, PPC_RegistrationRate, GC_CostperLead, CSC_CostperLead, TDM_CostperLead, TP_CostperLead, ClickThroughRate, InboundRegistrationRate;

        int NumberofContacts; double EventsBudget, ContactToInquiryConversion;
        #endregion

        #region Create/Edit/Version Model Input

        /// <summary>
        /// Default view for listing models
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Create view for model for current year, business unit of logged-in user
        /// </summary>
        /// <returns></returns>
        public ActionResult Create(int id = 0)
        {
            Common.Permission permission = Common.GetPermission(ActionItem.Model);
            switch (permission)
            {
                case Common.Permission.FullAccess:
                    break;
                case Common.Permission.NoAccess:
                    return RedirectToAction("Index", "NoAccess");
                case Common.Permission.NotAnEntity:
                    break;
                case Common.Permission.ViewOnly:
                    ViewBag.IsViewOnly = "true";
                    break;
            }
            ViewBag.ModelId = id;
            var businessunit = db.Models.Where(b => b.ModelId == id && b.IsDeleted == false).OrderByDescending(c => c.CreatedDate).Select(u => u.BusinessUnitId).FirstOrDefault();
            var IsBenchmarked = (id == 0) ? true : db.Models.Where(b => b.ModelId == id && b.IsDeleted == false).OrderByDescending(c => c.CreatedDate).Select(u => u.IsBenchmarked).FirstOrDefault();
            ViewBag.BusinessUnitId = Convert.ToString(businessunit);
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;
            ViewBag.IsBenchmarked = (IsBenchmarked != null) ? IsBenchmarked : true;
            BaselineModel objBaselineModel = FillInitialData(id, businessunit);
            string Title = objBaselineModel.Versions.Where(s => s.IsLatest == true).Select(s => s.Title).FirstOrDefault();
            if (Title != null && Title != string.Empty)
            {
                ViewBag.Msg = string.Format(Common.objCached.ModelTacticTypeNotexist, Title);
            }
            /*TFS point 252: editing a published model
               Added by Nirav Shah on 18 feb 2014
             */
            ViewBag.ModelPublishEdit = Common.objCached.ModelPublishEdit;
            ViewBag.ModelPublishCreateNew = Common.objCached.ModelPublishCreateNew;
            ViewBag.ModelPublishComfirmation = Common.objCached.ModelPublishComfirmation;
            ViewBag.Flag = false;
            if (id != 0)
            {
                ViewBag.Flag = chekckParentPublishModel(id);
            }
            return View(objBaselineModel);
        }

        /// <summary>
        /// Model Versioning change 02-Jan-2014
        /// </summary>
        /// <param name="ModelId"></param>
        /// <param name="BusinessUnitId"></param>
        /// <returns></returns>
        private List<ModelVersion> GetVersions(int ModelId, Guid BusinessUnitId)
        {
            List<ModelVersion> lstVersions = new List<ModelVersion>();
            if (ModelId != 0)
            {
                var versions = (from m in db.Models where m.IsDeleted == false && m.ModelId == ModelId select m).FirstOrDefault();
                if (versions != null)
                {
                    //Current Model
                    ModelVersion ver = new ModelVersion();
                    ver.ModelId = versions.ModelId;
                    ver.BusinessUnitId = versions.BusinessUnitId;
                    ver.Title = versions.Title;
                    ver.Status = versions.Status;
                    ver.Version = versions.Version;
                    ver.IsLatest = false;
                    lstVersions.Add(ver);

                    //Parent of ModelId
                    while (versions.Model2 != null)
                    {
                        versions = versions.Model2;
                        ver = new ModelVersion();
                        ver.ModelId = versions.ModelId;
                        ver.BusinessUnitId = versions.BusinessUnitId;
                        ver.Title = versions.Title;
                        ver.Status = versions.Status;
                        ver.Version = versions.Version;
                        ver.IsLatest = false;
                        lstVersions.Add(ver);
                    }

                    //Child of ModelId
                    Model child = GetChild(ModelId);
                    if (child != null)
                    {
                        ver = new ModelVersion();
                        ver.ModelId = child.ModelId;
                        ver.BusinessUnitId = child.BusinessUnitId;
                        ver.Title = child.Title;
                        ver.Status = child.Status;
                        ver.Version = child.Version;
                        ver.IsLatest = false;
                        lstVersions.Add(ver);

                        while (GetChild(child.ModelId) != null)
                        {
                            child = GetChild(child.ModelId);
                            ver = new ModelVersion();
                            ver.ModelId = child.ModelId;
                            ver.BusinessUnitId = child.BusinessUnitId;
                            ver.Title = child.Title;
                            ver.Status = child.Status;
                            ver.Version = child.Version;
                            ver.IsLatest = false;
                            lstVersions.Add(ver);
                        }
                    }

                }
            }
            if (lstVersions != null && lstVersions.Count > 0)
            {
                lstVersions = lstVersions.OrderByDescending(m => m.ModelId).ToList();
                lstVersions.First().IsLatest = true;
            }
            return lstVersions.Take(20).ToList();
        }

        private Model GetChild(int ModelId)
        {
            return (from m in db.Models where m.IsDeleted == false && m.ParentModelId == ModelId select m).FirstOrDefault();
        }

        /// <summary>
        /// Fill data for latest model for current year, business unit of logged-in user
        /// </summary>
        /// <returns></returns>
        public BaselineModel FillInitialData(int id, Guid BusinessUnitId)
        {
            var List = GetBusinessUnitsByClient();
            TempData["BusinessUnitList"] = new SelectList(List, "BusinessUnitId", "Title");

            var FunnelList = db.Funnels.Where(c => c.IsDeleted == false).ToDictionary(c => c.FunnelId, c => c.Description);
            TempData["FunnelList"] = FunnelList;

            BaselineModel objBaselineModel = new BaselineModel();
            objBaselineModel.Versions = GetVersions(id, BusinessUnitId);
            //Start Model versioning change 02-Jan-2014
            //List<ModelVersion> lstVersions = (from m in db.Models
            //                                  where m.IsDeleted == false && m.BusinessUnitId == BusinessUnitId
            //                                  orderby m.CreatedDate descending
            //                                  select new ModelVersion
            //                                  {
            //                                      ModelId = m.ModelId,
            //                                      BusinessUnitId = m.BusinessUnitId,
            //                                      Title = m.Title,
            //                                      Version = m.Version,
            //                                      Status = m.Status,
            //                                      IsLatest = false
            //                                  }).ToList();
            //if (lstVersions != null && lstVersions.Count > 0)
            //{
            //    lstVersions.FirstOrDefault().IsLatest = true;
            //    objBaselineModel.Versions = lstVersions.Take(20).ToList();
            //}
            //End Model versioning change 02-Jan-2014

            List<ModelStage> listmodelstage = new List<ModelStage>();

            var StageList = db.Stages.Where(d => d.IsDeleted == false && d.ClientId == Sessions.User.ClientId).OrderBy(c => c.Level).ToList();
            if (StageList != null && StageList.Count > 0)
            {
                var maxlevel = StageList.Max(t => t.Level);
                foreach (var s in StageList)
                {
                    ModelStage objModelStage = new ModelStage();
                    objModelStage.StageId = s.StageId;
                    if (s.Level < maxlevel)
                    {
                        int NextLevel = Convert.ToInt32(s.Level) + 1;
                        /*changed by Nirav Shah on 2 APR 2013*/
                        string stage1 = s.Title;
                        string stage2 = StageList.Where(stg => stg.Level == NextLevel).Select(stg => stg.Title).FirstOrDefault();
                        objModelStage.ConversionTitle = stage1 + " → " + stage2;
                        //Start Manoj 03Feb2014 - Bug 115:Model Creation - Velocity Labels same as Conversion Labels
                        objModelStage.VelocityTitle = stage1 + " → " + stage2;
                        //End Manoj 03Feb2014 - Bug 115:Model Creation - Velocity Labels same as Conversion Labels
                    }
                    else
                    {
                        objModelStage.ConversionTitle = s.Title;
                        //Start Manoj 03Feb2014 - Bug 115:Model Creation - Velocity Labels same as Conversion Labels
                        objModelStage.VelocityTitle = s.Title;
                        //End Manoj 03Feb2014 - Bug 115:Model Creation - Velocity Labels same as Conversion Labels
                    }
                    //Start Manoj 03Feb2014 - Bug 115:Model Creation - Velocity Labels same as Conversion Labels
                    //objModelStage.VelocityTitle = s.Title;
                    //End Manoj 03Feb2014 - Bug 115:Model Creation - Velocity Labels same as Conversion Labels
                    objModelStage.Description = s.Description;
                    objModelStage.StageId = s.StageId;
                    objModelStage.Level = Convert.ToInt32(s.Level);
                    objModelStage.Funnel = s.Funnel;
                    objModelStage.Code = s.Code;
                    listmodelstage.Add(objModelStage);
                }
            }
            objBaselineModel.lstmodelstage = listmodelstage;
            if (listmodelstage.Count() == 0)
            {
                TempData["StageNotExist"] = Common.objCached.StageNotDefined;
            }
            return objBaselineModel;
        }

        /// <summary>
        /// Saves the current model and create a new model version
        /// Modified By Maninder Singh Wadhva to address TFS Bug#239
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="hdnSTAGEMCR"></param>
        /// <param name="txtMCR"></param>
        /// <param name="hdnSTAGEMSV"></param>
        /// <param name="txtMSV"></param>
        /// <param name="hdnSTAGETCR"></param>
        /// <param name="txtTCR"></param>
        /// <param name="hdnSTAGETSV"></param>
        /// <param name="txtTSV"></param>
        /// <param name="hdnSTAGESCR"></param>
        /// <param name="txtSCR"></param>
        /// <param name="hdnSTAGESSV"></param>
        /// <param name="txtSSV"></param>
        /// <param name="txtMarketing"></param>
        /// <param name="txtTeleprospecting"></param>
        /// <param name="txtSales"></param>
        /// <returns></returns>
        [HttpPost]
        //public ActionResult Create(FormCollection collection, ICollection<string> hdnSTAGEMCR, ICollection<string> txtMCR, ICollection<string> hdnSTAGEMSV, ICollection<string> txtMSV, ICollection<string> hdnSTAGETCR, ICollection<string> txtTCR, ICollection<string> hdnSTAGETSV, ICollection<string> txtTSV, ICollection<string> hdnSTAGESCR, ICollection<string> txtSCR, ICollection<string> hdnSTAGESSV, ICollection<string> txtSSV, ICollection<string> txtMarketing, ICollection<string> txtTeleprospecting, ICollection<string> txtSales)
        /*changed by Nirav Shah on 2 APR 2013*/
        public ActionResult Create(FormCollection collection, ICollection<string> txtStageId, ICollection<string> txtTargetStage, ICollection<string> txtMCR, ICollection<string> txtMSV, ICollection<string> txtMarketing, ICollection<string> txtTeleprospecting, ICollection<string> txtSales)
        {
            int intFunnelMarketing = 0;
            int intFunnelTeleprospecting = 0;
            int intFunnelSales = 0;
            string mode = Convert.ToString(Request.Form["whichButton"]);
            string effectiveDate = Convert.ToString(Request.Form["EffectiveDate"]);
            bool IsBenchmarked = Convert.ToBoolean(Request.Form["IsBenchmarked"]);
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;
            int currentModelId = 0;
            int intModelid = 0;
            string Title = string.Empty;
            bool? isbenchmarkdb = null;
            int.TryParse(Convert.ToString(Request.Form["CurrentModelId"]), out currentModelId);
            string redirectModelZero = string.Empty;
            Guid tempBU = Guid.Parse(collection["BusinessUnitId"]);
            try
            {
                using (MRPEntities db = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {


                        Model objModel = new Model();
                        objModel.Title = Convert.ToString(collection["Title"]);
                        if (mode == "version")
                        {

                            var version = db.Models.Where(m => m.IsDeleted == false && m.BusinessUnitId == tempBU).OrderByDescending(t => t.CreatedDate).Select(s => s.Version).FirstOrDefault();
                            if (version != null && version != "")
                            {
                                objModel.Version = Convert.ToString((Convert.ToDouble(version) + 0.1));
                            }
                            else
                            {
                                objModel.Version = "1.0";
                            }
                            //Start Model versioning change 02-Jan-2014
                            objModel.ParentModelId = currentModelId;
                            //End Model versioning change 02-Jan-2014
                            objModel.Year = DateTime.Now.Year;
                            objModel.AddressableContacts = Convert.ToInt64(Convert.ToString(collection["AddressableContract"]).Replace(",", "").Replace("$", ""));
                            objModel.Status = Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Draft.ToString())).Value;
                            objModel.BusinessUnitId = Guid.Parse(collection["BusinessUnitId"]);
                            objModel.IsActive = true;
                            objModel.IsDeleted = false;
                            objModel.CreatedDate = DateTime.Now;
                            objModel.CreatedBy = Sessions.User.UserId;
                            objModel.IsBenchmarked = IsBenchmarked;
                            db.Models.Add(objModel);
                            int resModel = db.SaveChanges();
                            intModelid = objModel.ModelId;

                        }
                        else
                        {
                            intModelid = currentModelId;
                            if (intModelid == 0)
                            {
                                objModel.Version = "1.0";
                                objModel.Year = DateTime.Now.Year;
                                objModel.AddressableContacts = Convert.ToInt64(Convert.ToString(collection["AddressableContract"]).Replace(",", "").Replace("$", ""));
                                objModel.Status = Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Draft.ToString())).Value;
                                objModel.BusinessUnitId = Guid.Parse(collection["BusinessUnitId"]);
                                objModel.IsActive = true;
                                objModel.IsDeleted = false;
                                objModel.CreatedDate = DateTime.Now;
                                objModel.CreatedBy = Sessions.User.UserId;
                                objModel.IsBenchmarked = IsBenchmarked;
                                db.Models.Add(objModel);
                                int resModel = db.SaveChanges();
                                intModelid = objModel.ModelId;
                            }
                            else
                            {
                                Model obj = db.Models.Where(m => m.ModelId == intModelid).FirstOrDefault();
                                isbenchmarkdb = obj.IsBenchmarked;
                                if (obj != null)
                                {
                                    obj.AddressableContacts = Convert.ToInt64(Convert.ToString(collection["AddressableContract"]).Replace(",", "").Replace("$", ""));
                                    obj.BusinessUnitId = Guid.Parse(collection["BusinessUnitId"]);
                                    obj.ModifiedDate = DateTime.Now;
                                    obj.ModifiedBy = Sessions.User.UserId;
                                    obj.IsBenchmarked = IsBenchmarked;
                                    db.Entry(obj).State = EntityState.Modified;
                                    int result = db.SaveChanges();
                                }
                            }
                        }

                        Model_Funnel objModel_Funnel = new Model_Funnel();
                        objModel_Funnel.ModelId = intModelid;
                        objModel_Funnel.FunnelId = Convert.ToInt32(Request.Form["hdn_FunnelMarketing"]);

                        string[] strtxtMarketing = txtMarketing.ToArray();

                        if (strtxtMarketing.Length > 0)
                        {
                            if (strtxtMarketing.Length == 2)
                            {
                                long intValue = 0;
                                double doubleValue = 0.0;
                                long.TryParse(Convert.ToString(strtxtMarketing[0]).Replace(",", "").Replace("$", ""), out intValue);
                                double.TryParse(Convert.ToString(strtxtMarketing[1]).Replace(",", "").Replace("$", ""), out doubleValue);

                                objModel_Funnel.ExpectedLeadCount = intValue;
                                TempData["MarketingLeads"] = intValue;
                                objModel_Funnel.AverageDealSize = doubleValue;
                            }
                        }

                        Model_Funnel objmfunnel = db.Model_Funnel.Where(f => f.ModelId == objModel_Funnel.ModelId && f.FunnelId == objModel_Funnel.FunnelId).FirstOrDefault();
                        if (objmfunnel == null)
                        {
                            objModel_Funnel.CreatedBy = Sessions.User.UserId;
                            objModel_Funnel.CreatedDate = DateTime.Now;
                            db.Model_Funnel.Add(objModel_Funnel);
                            int resModel_FunnelMarketing = db.SaveChanges();
                            intFunnelMarketing = objModel_Funnel.ModelFunnelId;
                        }
                        else
                        {
                            objmfunnel.ModifiedBy = Sessions.User.UserId;
                            objmfunnel.ModifiedDate = DateTime.Now;
                            objmfunnel.ExpectedLeadCount = objModel_Funnel.ExpectedLeadCount;
                            objmfunnel.AverageDealSize = objModel_Funnel.AverageDealSize;
                            db.Entry(objmfunnel).State = EntityState.Modified;
                            int resModel_FunnelMarketing = db.SaveChanges();
                            intFunnelMarketing = objmfunnel.ModelFunnelId;
                        }

                        objModel_Funnel = new Model_Funnel();
                        objModel_Funnel.ModelId = intModelid;
                        objModel_Funnel.FunnelId = Convert.ToInt32(Request.Form["hdn_FunnelTeleprospecting"]);
                        //foreach (string tel in txtTeleprospecting)
                        //{
                        //    int intValue = 0;
                        //    double doubleValue = 0.0;
                        //    int.TryParse(Convert.ToString(tel).Replace(",", "").Replace("$", ""), out intValue);
                        //    double.TryParse(Convert.ToString(tel).Replace(",", "").Replace("$", ""), out doubleValue);

                        //    objModel_Funnel.ExpectedLeadCount = intValue;
                        //    objModel_Funnel.AverageDealSize = doubleValue;
                        //}
                        string[] strtxtTeleprospecting = txtTeleprospecting.ToArray();

                        if (strtxtTeleprospecting.Length > 0)
                        {
                            if (strtxtTeleprospecting.Length == 2)
                            {
                                long intValue = 0;
                                double doubleValue = 0.0;
                                long.TryParse(Convert.ToString(strtxtTeleprospecting[0]).Replace(",", "").Replace("$", ""), out intValue);
                                double.TryParse(Convert.ToString(strtxtTeleprospecting[1]).Replace(",", "").Replace("$", ""), out doubleValue);

                                objModel_Funnel.ExpectedLeadCount = intValue;
                                objModel_Funnel.AverageDealSize = doubleValue;
                            }
                        }
                        Model_Funnel tmodelfunnel = db.Model_Funnel.Where(f => f.ModelId == objModel_Funnel.ModelId && f.FunnelId == objModel_Funnel.FunnelId).FirstOrDefault();
                        if (tmodelfunnel == null)
                        {
                            objModel_Funnel.CreatedBy = Sessions.User.UserId;
                            objModel_Funnel.CreatedDate = DateTime.Now;
                            db.Model_Funnel.Add(objModel_Funnel);
                            int resModel_FunnelTeleprospecting = db.SaveChanges();
                            intFunnelTeleprospecting = objModel_Funnel.ModelFunnelId;
                        }
                        else
                        {
                            tmodelfunnel.ModifiedBy = Sessions.User.UserId;
                            tmodelfunnel.ModifiedDate = DateTime.Now;
                            tmodelfunnel.ExpectedLeadCount = objModel_Funnel.ExpectedLeadCount;
                            tmodelfunnel.AverageDealSize = objModel_Funnel.AverageDealSize;
                            db.Entry(tmodelfunnel).State = EntityState.Modified;
                            int resModel_FunnelTeleprospecting = db.SaveChanges();
                            intFunnelTeleprospecting = tmodelfunnel.ModelFunnelId;
                        }

                        objModel_Funnel = new Model_Funnel();
                        objModel_Funnel.ModelId = intModelid;
                        objModel_Funnel.FunnelId = Convert.ToInt32(Request.Form["hdn_FunnelSales"]);
                        //foreach (string sal in txtSales)
                        //{
                        //    int intValue = 0;
                        //    double doubleValue = 0.0;
                        //    int.TryParse(Convert.ToString(sal).Replace(",", "").Replace("$", ""), out intValue);
                        //    double.TryParse(Convert.ToString(sal).Replace(",", "").Replace("$", ""), out doubleValue);
                        //    objModel_Funnel.ExpectedLeadCount = intValue;
                        //    objModel_Funnel.AverageDealSize = doubleValue;
                        //}
                        string[] strttxtSales = txtSales.ToArray();

                        if (strttxtSales.Length > 0)
                        {
                            if (strttxtSales.Length == 2)
                            {
                                long intValue = 0;
                                double doubleValue = 0.0;
                                long.TryParse(Convert.ToString(strttxtSales[0]).Replace(",", "").Replace("$", ""), out intValue);
                                double.TryParse(Convert.ToString(strttxtSales[1]).Replace(",", "").Replace("$", ""), out doubleValue);

                                objModel_Funnel.ExpectedLeadCount = intValue;
                                objModel_Funnel.AverageDealSize = doubleValue;
                            }
                        }
                        objModel_Funnel.CreatedBy = Sessions.User.UserId;
                        objModel_Funnel.CreatedDate = DateTime.Now;


                        Model_Funnel smodelfunnel = db.Model_Funnel.Where(f => f.ModelId == objModel_Funnel.ModelId && f.FunnelId == objModel_Funnel.FunnelId).FirstOrDefault();
                        if (smodelfunnel == null)
                        {
                            objModel_Funnel.CreatedBy = Sessions.User.UserId;
                            objModel_Funnel.CreatedDate = DateTime.Now;
                            db.Model_Funnel.Add(objModel_Funnel);
                            int resModel_FunnelSales = db.SaveChanges();
                            intFunnelSales = objModel_Funnel.ModelFunnelId;
                        }
                        else
                        {
                            smodelfunnel.ModifiedBy = Sessions.User.UserId;
                            smodelfunnel.ModifiedDate = DateTime.Now;
                            smodelfunnel.ExpectedLeadCount = objModel_Funnel.ExpectedLeadCount;
                            smodelfunnel.AverageDealSize = objModel_Funnel.AverageDealSize;
                            db.Entry(smodelfunnel).State = EntityState.Modified;
                            int resModel_FunnelSales = db.SaveChanges();
                            intFunnelSales = smodelfunnel.ModelFunnelId;
                        }
                        if (IsBenchmarked == false)
                        {
                            /*changed by Nirav Shah on 2 APR 2013*/
                            string[] strtxtTargetStage = txtTargetStage.ToArray();
                            string[] strhdnSTAGEId = txtStageId.ToArray();
                            //Marketing Conversion Rates
                            if (txtStageId != null && txtMCR != null)
                            {
                                string[] strtxtMCR = txtMCR.ToArray();
                                SaveInputs(strhdnSTAGEId, strtxtMCR, strtxtTargetStage, intFunnelMarketing, Enums.StageType.CR.ToString());

                            }

                            if (txtStageId != null && txtMSV != null)
                            {
                                string[] strtxtMSV = txtMSV.ToArray();
                                SaveInputs(strhdnSTAGEId, strtxtMSV, strtxtTargetStage, intFunnelMarketing, Enums.StageType.SV.ToString());
                            }

                            ////Marketing Conversion Rates
                            //if (hdnSTAGEMCR != null && txtMCR != null)
                            //{
                            //    string[] strhdnSTAGEMCR = hdnSTAGEMCR.ToArray();
                            //    string[] strtxtMCR = txtMCR.ToArray();
                            //    SaveInputs(strhdnSTAGEMCR, strtxtMCR, intFunnelMarketing, Enums.StageType.CR.ToString());
                            //}

                            ////Marketing Stage Velocity
                            //if (hdnSTAGEMSV != null && txtMSV != null)
                            //{
                            //    string[] strhdnSTAGEMSV = hdnSTAGEMSV.ToArray();
                            //    string[] strtxtMSV = txtMSV.ToArray();
                            //    SaveInputs(strhdnSTAGEMSV, strtxtMSV, intFunnelMarketing, Enums.StageType.SV.ToString());
                            //}
                            ////Teleprospecting Conversion Rates
                            //if (hdnSTAGETCR != null && txtTCR != null)
                            //{
                            //    string[] strhdnSTAGETCR = hdnSTAGETCR.ToArray();
                            //    string[] strtxtTCR = txtTCR.ToArray();
                            //    SaveInputs(strhdnSTAGETCR, strtxtTCR, intFunnelTeleprospecting, Enums.StageType.CR.ToString());
                            //}
                            ////Teleprospecting Stage Velocity
                            //if (hdnSTAGETSV != null && txtTSV != null)
                            //{
                            //    string[] strhdnSTAGETSV = hdnSTAGETSV.ToArray();
                            //    string[] strtxtTSV = txtTSV.ToArray();
                            //    SaveInputs(strhdnSTAGETSV, strtxtTSV, intFunnelTeleprospecting, Enums.StageType.SV.ToString());
                            //}
                            ////Sales Conversion Rates
                            //if (hdnSTAGESCR != null && txtSCR != null)
                            //{
                            //    string[] strhdnSTAGESCR = hdnSTAGESCR.ToArray();
                            //    string[] strtxtSCR = txtSCR.ToArray();
                            //    SaveInputs(strhdnSTAGESCR, strtxtSCR, intFunnelSales, Enums.StageType.CR.ToString());
                            //}
                            ////Sales Stage Velocity
                            //if (hdnSTAGESSV != null && txtSSV != null)
                            //{
                            //    string[] strhdnSTAGESSV = hdnSTAGESSV.ToArray();
                            //    string[] strtxtSSV = txtSSV.ToArray();
                            //    SaveInputs(strhdnSTAGESSV, strtxtSSV, intFunnelSales, Enums.StageType.SV.ToString());
                            //}
                        }
                        else
                        {
                            XMLRead(intFunnelMarketing, intFunnelTeleprospecting, intFunnelSales);
                        }
                        if (isbenchmarkdb != null)
                        {
                            if (isbenchmarkdb != IsBenchmarked)
                            {
                                if (IsBenchmarked == false)
                                {
                                    Common.InsertChangeLog(Sessions.ModelId, 0, objModel.ModelId, "Inputs", Enums.ChangeLog_ComponentType.updatedto, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.advanced);
                                }
                                else
                                {
                                    Common.InsertChangeLog(Sessions.ModelId, 0, objModel.ModelId, "Inputs", Enums.ChangeLog_ComponentType.updatedto, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.benchmarked);
                                }
                            }
                            else
                            {
                                Common.InsertChangeLog(intModelid, currentModelId, intModelid, "Inputs", Enums.ChangeLog_ComponentType.empty, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.updated, "");
                            }
                        }
                        Sessions.ModelId = intModelid;
                        int intAddressableContacts = 0;
                        int.TryParse(Convert.ToString(collection["AddressableContract"]).Replace(",", "").Replace("$", ""), out intAddressableContacts);
                        TempData["AddressableContacts"] = intAddressableContacts;
                        // save data in Model_Funnel table

                        TempData["SuccessMessage"] = string.Format(Common.objCached.ModelSaveSuccess, objModel.Title);
                        if (mode == "version")
                        {
                            ObjectParameter returnValue = new ObjectParameter("ReturnValue", 0);
                            db.SaveModelInboundOutboundEvent(currentModelId, intModelid, DateTime.Now, Sessions.User.UserId, returnValue);
                        }

                        /* Bug 18:Model - results screen does not show for some models */
                        if (mode.Equals("publish"))
                        {

                            bool isTacticTypeExist = false;
                            bool isModelPublished = PublishModel(currentModelId, effectiveDate, out isTacticTypeExist);
                            if (isModelPublished.Equals(true))
                            {
                                redirectModelZero = "ModelZero";
                                scope.Complete();
                            }
                            else
                            {
                                if (isTacticTypeExist.Equals(false))
                                {
                                    TempData["SuccessMessage"] = string.Empty;
                                    TempData["ErrorMessage"] = string.Format(Common.objCached.ModelTacticTypeNotexist, Title);
                                }
                                else
                                {
                                    TempData["SuccessMessage"] = string.Empty;
                                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                                }
                            }
                        }
                        else
                        {
                            string Status = db.Models.Where(m => m.ModelId == Sessions.ModelId).Select(m => m.Status).SingleOrDefault();
                            redirectModelZero = "Tactics";
                            scope.Complete();
                        }
                        //Save Model Calculations
                        try
                        {
                            CalculateModelResults(intModelid);
                        }
                        catch (Exception e)
                        {
                            ErrorSignal.FromCurrentContext().Raise(e);
                        }


                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    TempData["SuccessMessage"] = string.Empty;
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                    TempData["SuccessMessage"] = string.Empty;
                }
            }
            /* Bug 18:Model - results screen does not show for some models */
            if (redirectModelZero == "ModelZero")
            {
                TempData["SuccessMessage"] = string.Format(Common.objCached.ModelPublishSuccess);
                TempData["ErrorMessage"] = string.Empty;
                return RedirectToAction("ModelZero");
            }
            else if (redirectModelZero == "Tactics")
            {
                return RedirectToAction("Tactics", new { id = intModelid });
            }
            ViewBag.ModelId = currentModelId;
            ViewBag.BusinessUnitId = Convert.ToString(tempBU);
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;
            ViewBag.IsBenchmarked = IsBenchmarked;
            ViewBag.ModelPublishEdit = Common.objCached.ModelPublishEdit;
            ViewBag.ModelPublishCreateNew = Common.objCached.ModelPublishCreateNew;
            ViewBag.ModelPublishComfirmation = Common.objCached.ModelPublishComfirmation;
            ViewBag.Flag = false;
            if (currentModelId != 0)
            {
                ViewBag.Flag = chekckParentPublishModel(currentModelId);
            }
            BaselineModel objBaselineModel = FillInitialData(currentModelId, tempBU);
            return View(objBaselineModel);
        }

        /// <summary>
        /// Function to save imputs for different Funnel Stage Type (CR/ SV).
        /// </summary>
        /// <param name="itemIds">Funnel item Ids.</param>
        /// <param name="itemLabels">Funnel Item Labels.</param>
        /// <param name="funnelId">Funnel Id.</param>
        /// <param name="stageType">Stage Type: CR/ SV.</param>
        private bool SaveInputs(string[] itemIds, string[] itemLabels, string[] strtxtTargetStage, int funnelId, string stageType)
        {
            if (itemIds.Length > 0 && itemLabels.Length > 0 && funnelId > 0 && !string.IsNullOrWhiteSpace(stageType))
            {
                for (int i = 0; i < itemIds.Length; i++)
                {
                    double doubleValue = 0.0;
                    double.TryParse(Convert.ToString(itemLabels[i]).Replace(",", "").Replace("$", ""), out doubleValue);

                    bool boolValue = false;
                    bool.TryParse(strtxtTargetStage[i], out boolValue);

                    Model_Funnel_Stage objModel_Funnel_Stage = new Model_Funnel_Stage();
                    objModel_Funnel_Stage.ModelFunnelId = funnelId;
                    objModel_Funnel_Stage.StageId = Convert.ToInt32(itemIds[i]);
                    objModel_Funnel_Stage.StageType = stageType;
                    objModel_Funnel_Stage.Value = doubleValue;
                    objModel_Funnel_Stage.AllowedTargetStage = boolValue;
                    Model_Funnel_Stage mModel_Funnel_Stage = db.Model_Funnel_Stage.Where(f => f.ModelFunnelId == objModel_Funnel_Stage.ModelFunnelId && f.StageId == objModel_Funnel_Stage.StageId && f.StageType == objModel_Funnel_Stage.StageType).FirstOrDefault();
                    if (mModel_Funnel_Stage == null)
                    {
                        objModel_Funnel_Stage.CreatedDate = DateTime.Now;
                        objModel_Funnel_Stage.CreatedBy = Sessions.User.UserId;
                        db.Model_Funnel_Stage.Add(objModel_Funnel_Stage);
                        int resModel_Funnel_Stage = db.SaveChanges();
                    }
                    else
                    {
                        mModel_Funnel_Stage.Value = objModel_Funnel_Stage.Value;
                        mModel_Funnel_Stage.ModifiedBy = Sessions.User.UserId;
                        mModel_Funnel_Stage.ModifiedDate = DateTime.Now;
                        if (stageType == Enums.StageType.CR.ToString())
                        {
                            mModel_Funnel_Stage.AllowedTargetStage = boolValue;
                        }
                        else
                        {
                            mModel_Funnel_Stage.AllowedTargetStage = false;
                        }
                        db.Entry(mModel_Funnel_Stage).State = EntityState.Modified;
                        int resModel_Funnel_Stage = db.SaveChanges();
                    }
                }
            }
            return true;
        }

        public JsonResult CheckTargetStage(int ModelId, int StageId)
        {
            var existintactic = db.TacticTypes.Where(t => t.ModelId == ModelId && t.StageId == StageId).FirstOrDefault();
            if (existintactic == null)
            {

                return Json("notexist", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("exist", JsonRequestBehavior.AllowGet);
            }
        }

        #region Benchmark Inputs Functions

        /// <summary>
        /// Read XML for Benchmark Inputs.
        /// </summary>
        /// <param name="intFunnelMarketing"></param>
        /// <param name="intFunnelTeleprospecting"></param>
        /// <param name="intFunnelSales"></param>
        /// <returns></returns>
        public void XMLRead(int intFunnelMarketing, int intFunnelTeleprospecting, int intFunnelSales)
        {
            if (System.IO.File.Exists(Common.xmlBenchmarkFilePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Common.xmlBenchmarkFilePath);
                BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
                List<Client> objlcient = objBDSUserRepository.GetClientList();
                string clientcode = objlcient.Where(c => c.ClientId == Sessions.User.ClientId).Select(c => c.Code).FirstOrDefault();
                bool clientflag = false;
                foreach (XmlNode _class in xmlDoc.SelectNodes(@"/Model/ModelInput"))
                {
                    foreach (XmlElement element1 in _class.SelectNodes(@"ClientCode"))
                    {
                        if (element1.HasAttribute("value"))
                        {
                            if (element1.Attributes["value"].Value.Equals(clientcode) == true)
                            {
                                foreach (XmlElement element in element1.SelectNodes(@"stage"))
                                {
                                    readdata(element, intFunnelMarketing);
                                }

                                clientflag = true;
                            }
                        }
                    }
                    if (clientflag == false)
                    {
                        foreach (XmlElement element in _class.SelectNodes(@"stage"))
                        {
                            readdata(element, intFunnelMarketing);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// To read specific nodes from Benchmark Inputs XML.
        /// </summary>
        /// <param name="_class"></param>
        /// <param name="clientexists"></param>
        /// <param name="intFunnelSales"></param>
        /// <param name="intFunnelTeleprospecting"></param>
        /// <param name="intFunnelMarketing"></param>
        /// <returns></returns>
        public void readdata(XmlElement element, int intFunnelMarketing)
        {
            string stageCode = string.Empty;
            double cr = 0;
            double sv = 0;
            bool targetStage = false;
            int StageId = 0;
            if (element.HasAttribute("code"))
            {
                stageCode = element.Attributes["code"].Value;
                StageId = retStageId(stageCode);
            }
            if (element.HasAttribute("cr"))
            {
                double.TryParse(element.Attributes["cr"].Value, out cr);
            }
            if (element.HasAttribute("sv"))
            {
                double.TryParse(element.Attributes["sv"].Value, out sv);
            }
            if (element.HasAttribute("targetstage"))
            {
                bool.TryParse(element.Attributes["targetstage"].Value, out targetStage);
            }

            ModelfunnelStage_Benchmark(intFunnelMarketing, StageId, "CR", cr, targetStage);
            ModelfunnelStage_Benchmark(intFunnelMarketing, StageId, "SV", sv, false);
        }

        /// <summary>
        /// Get StageId for given StageName.
        /// </summary>
        /// <param name="stagename"></param>
        /// <returns>Returns StageId for given StageName</returns>
        public int retStageId(string code)
        {
            int StageId = db.Stages.Where(s => s.Code.ToLower() == code.ToLower() && s.IsDeleted == false && s.ClientId == Sessions.User.ClientId).Select(s => s.StageId).FirstOrDefault();
            return StageId;
        }

        /// <summary>
        /// Save values to Model_Funnel_Stage table for Benchmark Inputs XML.
        /// </summary>
        /// <param name="ModelFunnelId"></param>
        /// <param name="StageId"></param>
        /// <param name="StageType"></param>
        /// <param name="Value"></param>
        /// <returns>Returns sucess value</returns>
        public int ModelfunnelStage_Benchmark(int ModelFunnelId, int StageId, string StageType, double Value, bool stageValue)
        {
            int result = 0;
            Model_Funnel_Stage objModel_Funnel_Stage = new Model_Funnel_Stage();
            objModel_Funnel_Stage.ModelFunnelId = ModelFunnelId;
            objModel_Funnel_Stage.StageId = StageId;
            objModel_Funnel_Stage.StageType = StageType;
            objModel_Funnel_Stage.Value = Value;


            Model_Funnel_Stage tsvModel_Funnel_Stage = db.Model_Funnel_Stage.Where(f => f.ModelFunnelId == objModel_Funnel_Stage.ModelFunnelId && f.StageId == objModel_Funnel_Stage.StageId && f.StageType == objModel_Funnel_Stage.StageType).FirstOrDefault();
            if (tsvModel_Funnel_Stage == null)
            {
                objModel_Funnel_Stage.AllowedTargetStage = stageValue;
                objModel_Funnel_Stage.CreatedDate = DateTime.Now;
                objModel_Funnel_Stage.CreatedBy = Sessions.User.UserId;
                db.Model_Funnel_Stage.Add(objModel_Funnel_Stage);
            }
            else
            {
                tsvModel_Funnel_Stage.AllowedTargetStage = stageValue;
                tsvModel_Funnel_Stage.Value = objModel_Funnel_Stage.Value;
                tsvModel_Funnel_Stage.ModifiedBy = Sessions.User.UserId;
                tsvModel_Funnel_Stage.ModifiedDate = DateTime.Now;
                db.Entry(tsvModel_Funnel_Stage).State = EntityState.Modified;
            }
            result = db.SaveChanges();
            return result;
        }

        #endregion

        /// <summary>
        /// Check already exist model
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="BusinessUnitId"></param>
        /// <returns></returns>
        public JsonResult CheckDuplicateModelTitle(string Title, string BusinessUnitId)
        {
            Guid BUID = Guid.Parse(BusinessUnitId);
            var obj = db.Models.Where(m => m.IsDeleted == false && m.BusinessUnitId == BUID && m.Title.Trim().ToLower() == Title.Trim().ToLower()).FirstOrDefault();
            if (obj == null)
            {
                return Json("notexist", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("exist", JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Function to get businessunit list
        /// </summary>
        /// <returns></returns>
        public List<BaselineModel> GetBusinessUnitsByClient()
        {
            return (from c in db.BusinessUnits.Where(c => c.IsDeleted == false && c.ClientId == Sessions.User.ClientId).ToList()
                    select new BaselineModel
                    {
                        BusinessUnitId = c.BusinessUnitId,
                        Title = c.Title
                    }).ToList<BaselineModel>();
        }

        /// <summary>
        /// Ajax request to get the model version based on the tab clicked
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetModelData(int id = 0)
        {
            var model = id;
            if (model != 0)
            {
                var modellist = db.Models.Where(m => m.ModelId == model).Select(n => new { n.ModelId, n.BusinessUnitId, n.Title, n.Version, n.Year, n.AddressableContacts, n.Status, n.IsActive, n.IsDeleted }).ToList();
                var modelfunnelist = db.Model_Funnel.Where(m => m.ModelId == model && m.Funnel.Title.ToLower() == "marketing").OrderBy(d => d.ModelFunnelId).Select(s => s.ModelFunnelId).ToList();
                var modelfunnelistall = db.Model_Funnel.Where(m => m.ModelId == model).OrderBy(d => d.ModelFunnelId).Select(d => new { d.ModelFunnelId, d.ModelId, d.FunnelId, d.ExpectedLeadCount, d.AverageDealSize }).ToList();
                var modelfunnelstagelist = db.Model_Funnel_Stage.Where(m => modelfunnelist.Contains(m.ModelFunnelId)).OrderBy(d => d.ModelFunnelStageId).Select(n => new { n.ModelFunnelStageId, n.ModelFunnelId, n.StageId, n.StageType, n.Value, n.AllowedTargetStage }).ToList();
                JsonConvert.SerializeObject(modellist, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                JsonConvert.SerializeObject(modelfunnelistall, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                JsonConvert.SerializeObject(modelfunnelstagelist, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                return Json(new { lstmodellist = modellist, lstmodelfunnelist = modelfunnelistall, lstmodelfunnelstagelist = modelfunnelstagelist }, JsonRequestBehavior.AllowGet);
            }


            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// View for no model exist in case of current year, business unit of logged-in user
        /// </summary>
        /// <returns></returns>
        public ActionResult ModelZero()
        {
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;
            ViewBag.IsViewOnly = "false";
            try
            {
                if (Sessions.RolePermission != null)
                {
                    Common.Permission permission = Common.GetPermission(ActionItem.Model);
                    switch (permission)
                    {
                        case Common.Permission.FullAccess:
                            break;
                        case Common.Permission.NoAccess:
                            return RedirectToAction("Index", "NoAccess");
                        case Common.Permission.NotAnEntity:
                            break;
                        case Common.Permission.ViewOnly:
                            ViewBag.IsViewOnly = "true";
                            break;
                    }
                }

                //Check whether the logged-in user has a Model built for his/ her Business Unit 
                ViewBag.ModelExists = false;
                if (Sessions.User != null)
                {
                    Model objModel = new Model();
                    if (Sessions.IsSystemAdmin || Sessions.IsClientAdmin || Sessions.IsDirector)
                    {
                        Guid clientId = Sessions.User.ClientId;
                        objModel = (from m in db.Models
                                    join bu in db.BusinessUnits on m.BusinessUnitId equals bu.BusinessUnitId
                                    where bu.ClientId == clientId && bu.IsDeleted == false && m.IsDeleted == false
                                    select m).OrderBy(q => q.Status).ThenBy(r => r.CreatedDate).FirstOrDefault();
                    }
                    else
                    {
                        objModel = db.Models.Where(m => m.BusinessUnitId == Sessions.User.BusinessUnitId && m.IsDeleted == false).FirstOrDefault();
                    }
                    if (objModel != null)
                    {
                        ViewBag.ModelExists = true;
                    }
                    //else
                    //{
                    //    return RedirectToAction("Create");
                    //}
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return View();
        }

        /// <summary>
        /// Get last child
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private Model GetLatestModelVersion(Model obj)
        {
            Model objModel = (from m in db.Models where m.ParentModelId == obj.ModelId select m).FirstOrDefault();
            if (objModel != null)
            {
                return GetLatestModelVersion(objModel);
            }
            else
            {
                return obj;
            }
        }

        /// <summary>
        /// Get latest published
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private Model GetLatestPublishedVersion(Model obj)
        {

            Model objModel = (from m in db.Models where m.ModelId == obj.ParentModelId select m).FirstOrDefault();
            if (objModel != null)
            {
                if (Convert.ToString(objModel.Status).ToLower() == ModelPublished)
                {
                    return objModel;
                }
                else
                {
                    return GetLatestPublishedVersion(objModel);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Added By: Kuber Joshi.
        /// Action to show Model List.
        /// </summary>
        public JsonResult GetModelList(string listType)
        {
            List<Model> objModelList = new List<Model>();
            try
            {
                if (!String.IsNullOrWhiteSpace(listType))
                {
                    Guid clientId = Sessions.User.ClientId;
                    List<Guid> objBusinessUnit = new List<Guid>();
                    if (Sessions.IsSystemAdmin || Sessions.IsClientAdmin || Sessions.IsDirector)
                    {
                        objBusinessUnit = db.BusinessUnits.Where(bu => bu.ClientId == clientId && bu.IsDeleted == false).Select(bu => bu.BusinessUnitId).ToList();
                    }
                    else
                    {
                        objBusinessUnit = db.BusinessUnits.Where(bu => bu.BusinessUnitId == Sessions.User.BusinessUnitId && bu.IsDeleted == false).Select(bu => bu.BusinessUnitId).ToList();
                    }
                    List<Model> lstModels = (from m in db.Models
                                             where m.IsDeleted == false && objBusinessUnit.Contains(m.BusinessUnitId) && (m.ParentModelId == 0 || m.ParentModelId == null)
                                             select m).ToList();
                    if (lstModels != null && lstModels.Count > 0)
                    {
                        foreach (Model obj in lstModels)
                        {
                            objModelList.Add(GetLatestModelVersion(obj));
                        }
                    }
                    //////List<Model> objPublishedModelList = new List<Model>();
                    //////foreach (Model obj in objModelList)
                    //////{
                    //////    if (Convert.ToString(obj.Status).ToLower() == ModelDraft)
                    //////    {
                    //////        Model obj1 = GetLatestPublishedVersion(obj);
                    //////        if (obj1 != null)
                    //////        {
                    //////            objPublishedModelList.Add(obj1);
                    //////        }
                    //////    }
                    //////}
                    //////foreach (Model obj in objPublishedModelList)
                    //////{
                    //////    objModelList.Add(obj);
                    //////}
                    if (listType.ToLower() == "active")
                    {
                        objModelList = objModelList.Where(m => m.Status.ToLower() == ModelPublished).ToList();
                    }

                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            var lstModel = objModelList.Select(p => new
            {
                id = p.ModelId,
                title = p.Title,
                businessUnit = p.BusinessUnit.Title,
                version = p.Version,
                status = p.Status,
                isOwner = (Sessions.User.UserId == p.CreatedBy || Sessions.IsSystemAdmin || Sessions.IsClientAdmin || Sessions.IsDirector) ? 0 : 1,/*added by Nirav Shah  on 14 feb 2014  for 256:Model list - add delete option for model and -	Delete option will be available for owner or director or system admin or client Admin */
                effectiveDate = p.EffectiveDate.HasValue == true ? p.EffectiveDate.Value.Date.ToString("M/d/yy") : "",  /* Added by Sohel on 08/04/2014 for PL #424 to show Effective Date Column*/
            }).OrderBy(p => p.title);
            return Json(lstModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Load overview
        /// </summary>
        /// <param name="title"></param>
        /// <param name="BusinessUnitId"></param>
        /// <returns></returns>
        public ActionResult LoadModelOverview(string title, string BusinessUnitId, int ModelId)
        {
            ModelOverView m = new ModelOverView();
            var List = GetBusinessUnitsByClient();
            TempData["BusinessUnitList"] = new SelectList(List, "BusinessUnitId", "Title");
            m.ModelId = ModelId;
            m.Title = title;
            m.BusinessUnitId = string.IsNullOrEmpty(BusinessUnitId) || BusinessUnitId == "0" ? Sessions.User.BusinessUnitId : Guid.Parse(BusinessUnitId);
            m.BusinessUnitName = Convert.ToString(List.Where(l => l.BusinessUnitId == m.BusinessUnitId).Select(l => l.Title).FirstOrDefault());
            return PartialView("_modeloverview", m);
        }

        /// <summary>
        /// Load contacts and inquiry
        /// </summary>
        /// <param name="AC"></param>
        /// <param name="MLeads"></param>
        /// <param name="MSize"></param>
        /// <param name="TLeads"></param>
        /// <param name="TSize"></param>
        /// <param name="SLeads"></param>
        /// <param name="SSize"></param>
        /// <returns></returns>
        /// modified datatype of MSize,TSize and SSize from int to double
        public ActionResult LoadContactInquiry(int AC, int MLeads, double MSize, int TLeads, double TSize, int SLeads, double SSize)
        {
            var FunnelList = db.Funnels.Where(c => c.IsDeleted == false).ToDictionary(c => c.FunnelId, c => c.Description);
            TempData["FunnelList"] = FunnelList;
            ContactInquiry m = new ContactInquiry();
            m.AddressableContract = AC;
            m.MarketingDealSize = MSize;
            m.MarketingLeads = MLeads;
            m.TeleprospectingDealSize = TSize;
            m.TeleprospectingLeads = TLeads;
            m.SalesDealSize = SSize;
            m.SalesLeads = SLeads;
            return PartialView("_contactinquiry", m);
        }

        /*added by Nirav Shah  on 14 feb 2014  for 256:Model list - add delete option for model*/
        /// <summary>
        /// Added By: Nirav Shah
        /// Action to delete Model from Model listing screen.
        /// </summary>
        public JsonResult deleteModel(int id)
        {
            try
            {
                Model objModel = db.Models.Where(model => model.ModelId == id).FirstOrDefault();
                if (objModel != null)
                {
                    var objPlan = db.Plans.Where(plan => plan.ModelId == id && plan.IsDeleted == false).ToList();
                    if (objPlan.Count == 0)
                    {
                        /*TFS point 252: editing a published model
                          Added by Nirav Shah on 18 feb 2014
                         * change : add check before delete tactic it will check if parent published model has used these tactics.
                         */
                        using (var scope = new TransactionScope())
                        {
                            objModel.IsDeleted = true;
                            db.Entry(objModel).State = EntityState.Modified;
                            int result = db.SaveChanges();
                            //Parent of ModelId
                            while (objModel.Model2 != null)
                            {
                                objModel = objModel.Model2;
                                objModel.IsDeleted = true;
                                if (objModel.Status.ToLower() != Enums.ModelStatus.Draft.ToString().ToLower())
                                {
                                    objPlan = db.Plans.Where(plan => plan.ModelId == objModel.ModelId && plan.IsDeleted == false).ToList();
                                    if (objPlan.Count != 0)
                                    {
                                        return Json(new { errorMsg = string.Format(Common.objCached.ModelDeleteParentDependency, objModel.Title, objModel.Version) }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                db.Entry(objModel).State = EntityState.Modified;
                                result = db.SaveChanges();
                            }
                            scope.Complete();
                            return Json(new { successmsg = string.Format(Common.objCached.ModelDeleteSuccess, objModel.Title) }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { errorMsg = string.Format(Common.objCached.ModelDeleteDependency, objModel.Title) }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { errorMsg = string.Format(Common.objCached.NoModelFound, objModel.Title) }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                return Json(new { errorMsg = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Audience view
        /// <summary>
        /// call Audience View
        /// Modified By Maninder Singh Wadhva to address TFS Bug#239
        /// </summary>
        /// <returns></returns>
        public ActionResult Audience(int id = 0, int modelvid = 0)
        {
            Common.Permission permission = Common.GetPermission(ActionItem.Model);
            switch (permission)
            {
                case Common.Permission.FullAccess:
                    break;
                case Common.Permission.NoAccess:
                    return RedirectToAction("Index", "NoAccess");
                case Common.Permission.NotAnEntity:
                    break;
                case Common.Permission.ViewOnly:
                    ViewBag.IsViewOnly = "true";
                    break;
            }

            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;
            //ViewBag.MId = Convert.ToString(id);
            ViewBag.MId = id;
            var intmodelid = 0;
            if (modelvid != 0)
            {
                intmodelid = modelvid;
            }
            else
            {
                intmodelid = id;
            }
            var IsBenchmarked = (intmodelid == 0) ? true : db.Model_Audience_Outbound.Where(b => b.ModelId == intmodelid).OrderByDescending(c => c.CreatedDate).Select(u => u.IsBenchmarked).FirstOrDefault();

            AudiencePlanModel objAudiencePlanModel = FillAudiencePlanModel(modelvid, intmodelid, id, (IsBenchmarked != null) ? Convert.ToBoolean(IsBenchmarked) : false);
            ViewBag.ModelPublishComfirmation = Common.objCached.ModelPublishComfirmation;
            ViewBag.Flag = false;
            if (id != 0)
            {
                ViewBag.Flag = chekckParentPublishModel(id);
            }
            return View(objAudiencePlanModel);
        }

        /// <summary>
        /// Function to fill audience plan model object.
        /// Added By Maninder Singh Wadhva to address TFS Bug#239
        /// </summary>
        /// <param name="modelvid">Model Version Id.</param>
        /// <param name="intmodelid">Model Id.</param>
        /// <param name="id">Model Id.</param>
        /// <param name="IsBenchmarked">Is benchmark flag.</param>
        /// <returns>Returns audience plan model.</returns>
        private AudiencePlanModel FillAudiencePlanModel(int modelvid, int intmodelid, int id, bool IsBenchmarked)
        {
            AudiencePlanModel objAudiencePlanModel = new AudiencePlanModel();
            objAudiencePlanModel = GetAudienceDataByModelID(intmodelid);
            ViewBag.IsBenchmarked = (IsBenchmarked != null) ? IsBenchmarked : true;

            List<ModelVersion> lstVersions = GetVersions(id, Sessions.User.BusinessUnitId);
            if (lstVersions != null && lstVersions.Count > 0)
            {
                lstVersions.FirstOrDefault().IsLatest = true;
                if (modelvid != id && intmodelid != 0)
                {
                    var objmod = db.Models.Where(m => m.ModelId == intmodelid).FirstOrDefault();
                    if (objmod != null)
                    {
                        foreach (var item in lstVersions)
                        {
                            item.Title = objmod.Title;
                            if (!string.IsNullOrEmpty(objmod.Status))
                            {
                                item.Status = Convert.ToString(objmod.Status).ToUpper();
                            }
                        }
                    }

                }

                objAudiencePlanModel.Versions = lstVersions;
            }
            else
            {
                objAudiencePlanModel.Versions = new List<ModelVersion>();
            }

            string Title = objAudiencePlanModel.Versions.Where(s => s.IsLatest == true).Select(s => s.Title).FirstOrDefault();
            if (Title != null && Title != string.Empty)
            {
                ViewBag.Msg = string.Format(Common.objCached.ModelTacticTypeNotexist, Title);
            }

            return objAudiencePlanModel;
        }
        #endregion

        #region Audience Post
        /// <summary>
        /// Modified By Maninder Singh Wadhva to address TFS Bug#239
        /// </summary>
        /// <param name="form"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Audience(AudiencePlanModel form, FormCollection collection)
        {
            string qtr = "";
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;
            bool IsBenchmarked = Convert.ToBoolean(Request.Form["IsBenchmarked"]);
            string effectiveDate = Convert.ToString(Request.Form["EffectiveDate"]);
            ViewBag.IsBenchmarked = IsBenchmarked;
            var mid = 0;
            string redirectModelZero = string.Empty;
            try
            {
                using (MRPEntities db = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        Guid createdby = new Guid();
                        DateTime createddate = new DateTime();
                        bool updateflag = false;

                        bool? isbenchmarkdb = null;

                        if (Request.Form["MID"] != "")
                        {
                            mid = Convert.ToInt32(Convert.ToString(Request.Form["MID"]));
                            ViewBag.MId = mid;
                        }

                        var objModel_Audience_Outbound_Exist = db.Model_Audience_Outbound.Where(m => m.ModelId == mid).FirstOrDefault();
                        if (objModel_Audience_Outbound_Exist != null)
                        {
                            updateflag = true;
                            isbenchmarkdb = objModel_Audience_Outbound_Exist.IsBenchmarked;
                        }


                        if (IsBenchmarked == false)
                        {
                            for (int i = 0; i < 4; i++)
                            {

                                Model_Audience_Outbound objModel_Audience_Outbound = new Model_Audience_Outbound();

                                objModel_Audience_Outbound.ModelId = mid;
                                objModel_Audience_Outbound.IsBenchmarked = false;

                                objModel_Audience_Outbound.ListAcquisitionsNormalErosion = 0;
                                objModel_Audience_Outbound.ListAcquisitionsUnsubscribeRate = 0;
                                objModel_Audience_Outbound.ListAcquisitionsCTRDelivered = 0;
                                objModel_Audience_Outbound.CTRDelivered = 0;
                                objModel_Audience_Outbound.RegistrationRate = 0;

                                if (i == 0)
                                {
                                    qtr = Convert.ToString(Enums.Quarter.Q1);
                                    if (updateflag)
                                    {
                                        objModel_Audience_Outbound = db.Model_Audience_Outbound.Where(m => m.ModelId == mid && m.Quarter == qtr).FirstOrDefault();
                                    }
                                    objModel_Audience_Outbound.NormalErosion = form.Q1NormalErosion;
                                    objModel_Audience_Outbound.UnsubscribeRate = form.Q1UnsubscribeRate;
                                    objModel_Audience_Outbound.NumberofTouches = Convert.ToInt32(Convert.ToString(collection["NumberOfTouchesQ1"]).Replace(",", ""));
                                    objModel_Audience_Outbound.ListAcquisitions = Convert.ToInt32(Convert.ToString(collection["ListAcquisitionsQ1"]).Replace(",", ""));
                                    objModel_Audience_Outbound.Acquisition_NumberofTouches = Convert.ToInt32(Convert.ToString(collection["Acquisition_NumberofTouchesQ1"]).Replace(",", ""));
                                    objModel_Audience_Outbound.ListAcquisitionsNormalErosion = form.Q1OutBoundErosion;
                                    objModel_Audience_Outbound.ListAcquisitionsUnsubscribeRate = form.Q1OutBoundUnsubscribeRate;
                                    objModel_Audience_Outbound.ListAcquisitionsCTRDelivered = form.Q1OutboundCTRDelivered;
                                    objModel_Audience_Outbound.CTRDelivered = form.Q1CTRDelivered;
                                    objModel_Audience_Outbound.RegistrationRate = form.Q1RegistrationRate;
                                    objModel_Audience_Outbound.Acquisition_CostperContact = Convert.ToDouble(Convert.ToString(collection["Acquisition_CostperContactQ1"]).Replace(",", ""));
                                    objModel_Audience_Outbound.Acquisition_RegistrationRate = form.Acquisition_RegistrationRate;

                                }
                                if (i == 1)
                                {
                                    qtr = Convert.ToString(Enums.Quarter.Q2);
                                    if (updateflag)
                                    {
                                        objModel_Audience_Outbound = db.Model_Audience_Outbound.Where(m => m.ModelId == mid && m.Quarter == qtr).FirstOrDefault();
                                    }
                                    objModel_Audience_Outbound.NumberofTouches = Convert.ToInt32(Convert.ToString(collection["NumberOfTouchesQ2"]).Replace(",", ""));
                                    objModel_Audience_Outbound.ListAcquisitions = Convert.ToInt32(Convert.ToString(collection["ListAcquisitionsQ2"]).Replace(",", ""));
                                    objModel_Audience_Outbound.Acquisition_NumberofTouches = Convert.ToInt32(Convert.ToString(collection["Acquisition_NumberofTouchesQ2"]).Replace(",", ""));
                                }
                                if (i == 2)
                                {
                                    qtr = Convert.ToString(Enums.Quarter.Q3);
                                    if (updateflag)
                                    {
                                        objModel_Audience_Outbound = db.Model_Audience_Outbound.Where(m => m.ModelId == mid && m.Quarter == qtr).FirstOrDefault();
                                    }
                                    objModel_Audience_Outbound.NumberofTouches = Convert.ToInt32(Convert.ToString(collection["NumberOfTouchesQ3"]).Replace(",", ""));
                                    objModel_Audience_Outbound.ListAcquisitions = Convert.ToInt32(Convert.ToString(collection["ListAcquisitionsQ3"]).Replace(",", ""));
                                    objModel_Audience_Outbound.Acquisition_NumberofTouches = Convert.ToInt32(Convert.ToString(collection["Acquisition_NumberofTouchesQ3"]).Replace(",", ""));
                                }
                                if (i == 3)
                                {
                                    qtr = Convert.ToString(Enums.Quarter.Q4);
                                    if (updateflag)
                                    {
                                        objModel_Audience_Outbound = db.Model_Audience_Outbound.Where(m => m.ModelId == mid && m.Quarter == qtr).FirstOrDefault();
                                    }
                                    objModel_Audience_Outbound.NumberofTouches = Convert.ToInt32(Convert.ToString(collection["NumberOfTouchesQ4"]).Replace(",", ""));
                                    objModel_Audience_Outbound.ListAcquisitions = Convert.ToInt32(Convert.ToString(collection["ListAcquisitionsQ4"]).Replace(",", ""));
                                    objModel_Audience_Outbound.Acquisition_NumberofTouches = Convert.ToInt32(Convert.ToString(collection["Acquisition_NumberofTouchesQ4"]).Replace(",", ""));
                                }

                                objModel_Audience_Outbound.Quarter = "Q" + (i + 1);
                                //objModel_Audience_Outbound.ListAcquisitions = form.ListAcquisitionsQ1; // comment by Nirav Shah on 4 Jan 2014

                                //Newly Added Fields by kunal on 23-dec
                                //start

                                if (updateflag)
                                {

                                    objModel_Audience_Outbound.ModifiedBy = Sessions.User.UserId;
                                    objModel_Audience_Outbound.ModifiedDate = DateTime.Now;
                                    db.Entry(objModel_Audience_Outbound).State = EntityState.Modified;

                                }
                                else
                                {
                                    objModel_Audience_Outbound.CreatedBy = Sessions.User.UserId;
                                    objModel_Audience_Outbound.CreatedDate = DateTime.Now;
                                    db.Model_Audience_Outbound.Add(objModel_Audience_Outbound);

                                }


                                int resobjModel_Audience_Outbound = db.SaveChanges();
                            }



                            Model_Audience_Inbound objModel_Audience_Inbound = new Model_Audience_Inbound();
                            if (updateflag)
                            {
                                objModel_Audience_Inbound = db.Model_Audience_Inbound.Where(m => m.ModelId == mid).FirstOrDefault();
                            }
                            objModel_Audience_Inbound.ModelId = mid;
                            objModel_Audience_Inbound.Quarter = Convert.ToString(Enums.Quarter.ALLQ);
                            objModel_Audience_Inbound.Impressions = Convert.ToInt32(Convert.ToString(collection["Impressions"]).Replace(",", ""));
                            objModel_Audience_Inbound.ClickThroughRate = form.ClickThroughRate;
                            objModel_Audience_Inbound.RegistrationRate = form.InboundRegistrationRate;
                            objModel_Audience_Inbound.PPC_ClickThroughs = Convert.ToInt32(Convert.ToString(collection["PPC_ClickThroughs"]).Replace(",", ""));
                            objModel_Audience_Inbound.PPC_CostperClickThrough = Convert.ToDouble(Convert.ToString(collection["PPC_CostperClickThrough"]).Replace(",", ""));
                            objModel_Audience_Inbound.PPC_RegistrationRate = form.PPC_RegistrationRate;
                            objModel_Audience_Inbound.GC_GuaranteedCPLBudget = Convert.ToInt32(Convert.ToString(collection["GC_GuaranteedCPLBudget"]).Replace(",", ""));
                            objModel_Audience_Inbound.GC_CostperLead = Convert.ToDouble(Convert.ToString(collection["GC_CostperLead"]).Replace(",", ""));
                            objModel_Audience_Inbound.CSC_NonGuaranteedCPLBudget = Convert.ToInt32(Convert.ToString(collection["CSC_NonGuaranteedCPLBudget"]).Replace(",", ""));
                            objModel_Audience_Inbound.CSC_CostperLead = Convert.ToDouble(Convert.ToString(collection["CSC_CostperLead"]).Replace(",", ""));
                            objModel_Audience_Inbound.TDM_DigitalMediaBudget = Convert.ToInt32(Convert.ToString(collection["TDM_DigitalMediaBudget"]).Replace(",", ""));
                            objModel_Audience_Inbound.TDM_CostperLead = Convert.ToDouble(Convert.ToString(collection["TDM_CostperLead"]).Replace(",", ""));
                            objModel_Audience_Inbound.TP_PrintMediaBudget = Convert.ToInt32(Convert.ToString(collection["TP_PrintMediaBudget"]).Replace(",", ""));
                            objModel_Audience_Inbound.TP_CostperLead = Convert.ToDouble(Convert.ToString(collection["TP_CostperLead"]).Replace(",", ""));

                            if (updateflag)
                            {

                                objModel_Audience_Inbound.ModifiedBy = Sessions.User.UserId;
                                objModel_Audience_Inbound.ModifiedDate = DateTime.Now;
                                db.Entry(objModel_Audience_Inbound).State = EntityState.Modified;

                            }
                            else
                            {
                                objModel_Audience_Inbound.CreatedBy = Sessions.User.UserId;
                                objModel_Audience_Inbound.CreatedDate = DateTime.Now;

                                db.Model_Audience_Inbound.Add(objModel_Audience_Inbound);
                            }

                            int resobjModel_Audience_Inbound = db.SaveChanges();


                            for (int i = 0; i < 4; i++)
                            {
                                Model_Audience_Event objModel_Audience_Event = new Model_Audience_Event();

                                objModel_Audience_Event.ModelId = mid;
                                objModel_Audience_Event.Quarter = "Q" + (i + 1);
                                if (i == 0)
                                {
                                    if (updateflag)
                                    {
                                        qtr = Convert.ToString(Enums.Quarter.Q1);
                                        objModel_Audience_Event = db.Model_Audience_Event.Where(m => m.ModelId == mid && m.Quarter == qtr).FirstOrDefault();
                                    }
                                    objModel_Audience_Event.NumberofContacts = Convert.ToInt64(Convert.ToString(collection["NumberofContactsQ1"]).Replace(",", ""));
                                    objModel_Audience_Event.EventsBudget = Convert.ToDouble(Convert.ToString(collection["EventsBudgetQ1"]).Replace(",", ""));
                                    objModel_Audience_Event.ContactToInquiryConversion = form.ContactToInquiryConversion;
                                }
                                if (i == 1)
                                {
                                    if (updateflag)
                                    {
                                        qtr = Convert.ToString(Enums.Quarter.Q2);
                                        objModel_Audience_Event = db.Model_Audience_Event.Where(m => m.ModelId == mid && m.Quarter == qtr).FirstOrDefault();
                                    }
                                    objModel_Audience_Event.NumberofContacts = Convert.ToInt64(Convert.ToString(collection["NumberofContactsQ2"]).Replace(",", ""));
                                    objModel_Audience_Event.EventsBudget = Convert.ToDouble(Convert.ToString(collection["EventsBudgetQ2"]).Replace(",", ""));
                                }
                                if (i == 2)
                                {
                                    if (updateflag)
                                    {
                                        qtr = Convert.ToString(Enums.Quarter.Q3);
                                        objModel_Audience_Event = db.Model_Audience_Event.Where(m => m.ModelId == mid && m.Quarter == qtr).FirstOrDefault();
                                    }
                                    objModel_Audience_Event.NumberofContacts = Convert.ToInt64(Convert.ToString(collection["NumberofContactsQ3"]).Replace(",", ""));
                                    objModel_Audience_Event.EventsBudget = Convert.ToDouble(Convert.ToString(collection["EventsBudgetQ3"]).Replace(",", ""));
                                }
                                if (i == 3)
                                {
                                    if (updateflag)
                                    {
                                        qtr = Convert.ToString(Enums.Quarter.Q4);
                                        objModel_Audience_Event = db.Model_Audience_Event.Where(m => m.ModelId == mid && m.Quarter == qtr).FirstOrDefault();
                                    }
                                    objModel_Audience_Event.NumberofContacts = Convert.ToInt64(Convert.ToString(collection["NumberofContactsQ4"]).Replace(",", ""));
                                    objModel_Audience_Event.EventsBudget = Convert.ToDouble(Convert.ToString(collection["EventsBudgetQ4"]).Replace(",", ""));
                                }

                                if (updateflag)
                                {

                                    objModel_Audience_Event.ModifiedBy = Sessions.User.UserId;
                                    objModel_Audience_Event.ModifiedDate = DateTime.Now;
                                    db.Entry(objModel_Audience_Event).State = EntityState.Modified;

                                }
                                else
                                {
                                    objModel_Audience_Event.CreatedBy = Sessions.User.UserId;
                                    objModel_Audience_Event.CreatedDate = DateTime.Now;

                                    db.Model_Audience_Event.Add(objModel_Audience_Event);
                                }

                                int resobjModel_Audience_Event = db.SaveChanges();
                            }

                        }
                        else
                        {
                            XMLReadAudience(mid, IsBenchmarked, updateflag);

                        }
                        if (isbenchmarkdb != null)
                        {
                            if (isbenchmarkdb != IsBenchmarked)
                            {
                                if (IsBenchmarked == false)
                                {
                                    Common.InsertChangeLog(mid, 0, mid, "Audience", Enums.ChangeLog_ComponentType.updatedto, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.advanced);
                                }
                                else
                                {
                                    Common.InsertChangeLog(mid, 0, mid, "Audience", Enums.ChangeLog_ComponentType.updatedto, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.benchmarked);
                                }
                            }
                            else if (updateflag == true)
                            {
                                Common.InsertChangeLog(mid, 0, mid, "Audience", Enums.ChangeLog_ComponentType.empty, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.updated);
                            }
                        }
                        // scope.Complete();
                        /* Bug 18:Model - results screen does not show for some models */
                        string mode = Convert.ToString(Request.Form["isPublishButton"]);
                        if (mode.Equals("publish"))
                        {
                            bool isTacticTypeExist = false;

                            bool isModelPublished = PublishModel(mid, effectiveDate, out isTacticTypeExist);
                            if (isModelPublished.Equals(true))
                            {
                                scope.Complete();
                                redirectModelZero = "ModelZero";
                            }
                            else
                            {
                                if (isTacticTypeExist.Equals(false))
                                {
                                    string title = db.Models.Single(m => m.ModelId.Equals(mid)).Title;
                                    TempData["ErrorMessage"] = string.Format(Common.objCached.ModelTacticTypeNotexist, title);
                                    TempData["SuccessMessage"] = string.Empty;
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                                    TempData["SuccessMessage"] = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            scope.Complete();
                            redirectModelZero = "Tactics";
                        }

                        //Save Model Calculations
                        try
                        {
                            CalculateModelResults(mid);
                        }
                        catch (Exception e)
                        {
                            ErrorSignal.FromCurrentContext().Raise(e);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            AudiencePlanModel objAudiencePlanModel = FillAudiencePlanModel(0, mid, mid, IsBenchmarked);
            ViewBag.ModelPublishComfirmation = Common.objCached.ModelPublishComfirmation;
            ViewBag.Flag = false;
            if (mid != 0)
            {
                ViewBag.Flag = chekckParentPublishModel(mid);
            }
            if (redirectModelZero == "ModelZero")
            {
                TempData["SuccessMessage"] = string.Format(Common.objCached.ModelPublishSuccess);
                TempData["ErrorMessage"] = string.Empty;
                return RedirectToAction("ModelZero");
            }
            else if (redirectModelZero == "Tactics")
            {
                TempData["SuccessMessage"] = Common.objCached.ModelAudienceSaveSuccess;
                TempData["ErrorMessage"] = string.Empty;
                return RedirectToAction("Tactics", new { id = mid });
            }
            return View(objAudiencePlanModel);
        }
        #region Benchmark Audience Functions

        /// <summary>
        /// Read XML for Benchmark Audience.
        /// </summary>
        /// <param name="ModelId"></param>
        /// <param name="IsBenchmarked"></param>
        /// <returns></returns>
        public void XMLReadAudience(int ModelId, bool IsBenchmarked, bool updateflag)
        {
            if (System.IO.File.Exists(Common.xmlBenchmarkFilePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Common.xmlBenchmarkFilePath);
                string stage = "", value = "", Quarter = "";
                foreach (XmlNode _class in xmlDoc.SelectNodes(@"/Model/modelAudience"))
                {
                    foreach (XmlElement element in _class.SelectNodes(@"Quarter1/Outbound"))
                    {
                        Quarter = Convert.ToString(Enums.Quarter.Q1);
                        if (element.HasAttribute("stage"))
                        {
                            stage = Convert.ToString((element.Attributes["stage"].Value));
                            stage = Regex.Split(stage, Convert.ToString(Enums.Quarter.Q1))[0];
                            value = element.Attributes["value"].Value;
                            getOutboundData(stage, value, false, Quarter, ModelId, IsBenchmarked, updateflag);
                        }
                    }
                    stage = ""; value = "";
                    getOutboundData(stage, value, true, Quarter, ModelId, IsBenchmarked, updateflag);

                    foreach (XmlElement element in _class.SelectNodes(@"Quarter1/Inbound"))
                    {
                        Quarter = Convert.ToString(Enums.Quarter.ALLQ);
                        if (element.HasAttribute("stage"))
                        {
                            stage = Convert.ToString((element.Attributes["stage"].Value));
                            value = element.Attributes["value"].Value;
                            getInboundData(stage, value, false, Quarter, ModelId, updateflag);
                        }
                    }
                    stage = ""; value = "";
                    getInboundData(stage, value, true, Quarter, ModelId, updateflag);

                    foreach (XmlElement element in _class.SelectNodes(@"Quarter1/Events"))
                    {
                        Quarter = Convert.ToString(Enums.Quarter.Q1);
                        if (element.HasAttribute("stage"))
                        {
                            stage = Convert.ToString((element.Attributes["stage"].Value));
                            stage = Regex.Split(stage, Convert.ToString(Enums.Quarter.Q1))[0];
                            value = element.Attributes["value"].Value;
                            getEventsData(stage, value, false, Quarter, ModelId, updateflag);
                        }
                    }
                    stage = ""; value = "";
                    getEventsData(stage, value, true, Quarter, ModelId, updateflag);

                    foreach (XmlElement element in _class.SelectNodes(@"Quarter2/Outbound"))
                    {
                        Quarter = Convert.ToString(Enums.Quarter.Q2);
                        if (element.HasAttribute("stage"))
                        {
                            stage = Convert.ToString((element.Attributes["stage"].Value));
                            stage = Regex.Split(stage, Convert.ToString(Enums.Quarter.Q2))[0];
                            value = element.Attributes["value"].Value;
                            getOutboundData(stage, value, false, Quarter, ModelId, IsBenchmarked, updateflag);
                        }
                    }
                    stage = ""; value = "";
                    getOutboundData(stage, value, true, Quarter, ModelId, IsBenchmarked, updateflag);

                    foreach (XmlElement element in _class.SelectNodes(@"Quarter2/Events"))
                    {
                        Quarter = Convert.ToString(Enums.Quarter.Q2);
                        if (element.HasAttribute("stage"))
                        {
                            stage = Convert.ToString((element.Attributes["stage"].Value));
                            stage = Regex.Split(stage, Convert.ToString(Enums.Quarter.Q2))[0];
                            value = element.Attributes["value"].Value;
                            getEventsData(stage, value, false, Quarter, ModelId, updateflag);
                        }
                    }
                    stage = ""; value = "";
                    getEventsData(stage, value, true, Quarter, ModelId, updateflag);

                    foreach (XmlElement element in _class.SelectNodes(@"Quarter3/Outbound"))
                    {
                        Quarter = Convert.ToString(Enums.Quarter.Q3);
                        if (element.HasAttribute("stage"))
                        {
                            stage = Convert.ToString((element.Attributes["stage"].Value));
                            stage = Regex.Split(stage, Convert.ToString(Enums.Quarter.Q3))[0];
                            value = element.Attributes["value"].Value;
                            getOutboundData(stage, value, false, Quarter, ModelId, IsBenchmarked, updateflag);
                        }
                    }
                    stage = ""; value = "";
                    getOutboundData(stage, value, true, Quarter, ModelId, IsBenchmarked, updateflag);

                    foreach (XmlElement element in _class.SelectNodes(@"Quarter3/Events"))
                    {
                        Quarter = Convert.ToString(Enums.Quarter.Q3);
                        if (element.HasAttribute("stage"))
                        {
                            stage = Convert.ToString((element.Attributes["stage"].Value));
                            stage = Regex.Split(stage, Convert.ToString(Enums.Quarter.Q3))[0];
                            value = element.Attributes["value"].Value;
                            getEventsData(stage, value, false, Quarter, ModelId, updateflag);
                        }
                    }
                    stage = ""; value = "";
                    getEventsData(stage, value, true, Quarter, ModelId, updateflag);

                    foreach (XmlElement element in _class.SelectNodes(@"Quarter4/Outbound"))
                    {
                        Quarter = Convert.ToString(Enums.Quarter.Q4);
                        if (element.HasAttribute("stage"))
                        {
                            stage = Convert.ToString((element.Attributes["stage"].Value));
                            stage = Regex.Split(stage, Convert.ToString(Enums.Quarter.Q4))[0];
                            value = element.Attributes["value"].Value;
                            getOutboundData(stage, value, false, Quarter, ModelId, IsBenchmarked, updateflag);
                        }
                    }
                    stage = ""; value = "";
                    getOutboundData(stage, value, true, Quarter, ModelId, IsBenchmarked, updateflag);

                    foreach (XmlElement element in _class.SelectNodes(@"Quarter4/Events"))
                    {
                        Quarter = Convert.ToString(Enums.Quarter.Q4);
                        if (element.HasAttribute("stage"))
                        {
                            stage = Convert.ToString((element.Attributes["stage"].Value));
                            stage = Regex.Split(stage, Convert.ToString(Enums.Quarter.Q4))[0];
                            value = element.Attributes["value"].Value;
                            getEventsData(stage, value, false, Quarter, ModelId, updateflag);
                        }
                    }
                    stage = ""; value = "";
                    getEventsData(stage, value, true, Quarter, ModelId, updateflag);
                }
            }
        }

        /// <summary>
        /// Read Outbound data from Benchmark Audience XML.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="value"></param>
        /// <param name="flag"></param>
        /// <param name="Quarter"></param>
        /// <param name="ModelId"></param>
        /// <param name="IsBenchmarked"></param>
        /// <returns></returns>
        public void getOutboundData(string stage, string value, bool flag, string Quarter, int ModelId, bool IsBenchmarked, bool updateflag)
        {
            if (stage == "NormalErosion")
            {
                NormalErosion = Convert.ToDouble(value);
            }
            else if (stage == "UnsubscribeRate")
            {
                UnsubscribeRate = Convert.ToDouble(value);
            }
            else if (stage == "NumberOfTouches")
            {
                NumberOfTouches = Convert.ToInt32(value);
            }
            else if (stage == "CTRDelivered")
            {
                CTRDelivered = Convert.ToDouble(value);
            }
            else if (stage == "RegistrationRate")
            {
                RegistrationRate = Convert.ToDouble(value);
            }
            else if (stage == "ListAcquisitions")
            {
                ListAcquisitions = Convert.ToInt32(value);
            }
            else if (stage == "Acquisition_CostperContact")
            {
                Acquisition_CostperContact = Convert.ToDouble(value);
            }
            else if (stage == "Acquisition_Erosion")
            {
                ListAcquisitionsNormalErosion = Convert.ToDouble(value);
            }
            else if (stage == "Acquisition_UnsubscribeRate")
            {
                ListAcquisitionsUnsubscribeRate = Convert.ToDouble(value);
            }
            else if (stage == "Acquisition_NumberofTouches")
            {
                Acquisition_NumberofTouches = Convert.ToInt32(value);
            }
            else if (stage == "AcquisitionCTRDelivered")
            {
                ListAcquisitionsCTRDelivered = Convert.ToDouble(value);
            }
            else if (stage == "Acquisition_RegistrationRate")
            {
                Acquisition_RegistrationRate = Convert.ToDouble(value);
            }
            if (flag == true)
            {
                InsertOutBound(ModelId, NormalErosion, UnsubscribeRate, NumberOfTouches, ListAcquisitions, Acquisition_NumberofTouches, CTRDelivered, RegistrationRate, Quarter,
                   ListAcquisitionsNormalErosion, ListAcquisitionsUnsubscribeRate, ListAcquisitionsCTRDelivered, Acquisition_CostperContact, Acquisition_RegistrationRate, IsBenchmarked, updateflag);
                NumberOfTouches = 0; Acquisition_NumberofTouches = 0; ListAcquisitions = 0; NormalErosion = 0; UnsubscribeRate = 0; CTRDelivered = 0; RegistrationRate = 0; ListAcquisitionsNormalErosion = 0;
                ListAcquisitionsUnsubscribeRate = 0; ListAcquisitionsCTRDelivered = 0; Acquisition_CostperContact = 0; Acquisition_RegistrationRate = 0;
            }
        }

        /// <summary>
        /// Read Inbound data from Benchmark Audience XML.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="value"></param>
        /// <param name="flag"></param>
        /// <param name="Quarter"></param>
        /// <param name="ModelId"></param>
        /// <returns></returns>
        public void getInboundData(string stage, string value, bool flag, string Quarter, int ModelId, bool updateflag)
        {

            if (stage == "Impressions")
            {
                Impressions = Convert.ToInt32(value);
            }
            else if (stage == "ClickThroughRate")
            {
                ClickThroughRate = Convert.ToDouble(value);
            }
            else if (stage == "InboundRegistrationRate")
            {
                InboundRegistrationRate = Convert.ToDouble(value);
            }
            else if (stage == "PPC_ClickThroughs")
            {
                PPC_ClickThroughs = Convert.ToInt32(value);
            }
            else if (stage == "PPC_CostperClickThrough")
            {
                PPC_CostperClickThrough = Convert.ToDouble(value);
            }
            else if (stage == "PPC_RegistrationRate")
            {
                PPC_RegistrationRate = Convert.ToDouble(value);
            }
            else if (stage == "GC_GuaranteedCPLBudget")
            {
                GC_GuaranteedCPLBudget = Convert.ToInt32(value);
            }
            else if (stage == "GC_CostperLead")
            {
                GC_CostperLead = Convert.ToDouble(value);
            }
            else if (stage == "CSC_NonGuaranteedCPLBudget")
            {
                CSC_NonGuaranteedCPLBudget = Convert.ToInt32(value);
            }
            else if (stage == "CSC_CostperLead")
            {
                CSC_CostperLead = Convert.ToDouble(value);
            }
            else if (stage == "TDM_CostperLead")
            {
                TDM_CostperLead = Convert.ToDouble(value);
            }
            else if (stage == "TDM_DigitalMediaBudget")
            {
                TDM_DigitalMediaBudget = Convert.ToInt32(value);
            }
            else if (stage == "TP_PrintMediaBudget")
            {
                TP_PrintMediaBudget = Convert.ToInt32(value);
            }
            else if (stage == "TP_CostperLead")
            {
                TP_CostperLead = Convert.ToDouble(value);
            }
            if (flag == true)
            {
                InsertInBound(ModelId, Quarter, Impressions, ClickThroughRate, InboundRegistrationRate, PPC_ClickThroughs, PPC_CostperClickThrough, PPC_RegistrationRate, GC_GuaranteedCPLBudget,
                              GC_CostperLead, CSC_NonGuaranteedCPLBudget, CSC_CostperLead, TDM_DigitalMediaBudget, TDM_CostperLead, TP_PrintMediaBudget, TP_CostperLead, updateflag);

                Impressions = 0; ClickThroughRate = 0; InboundRegistrationRate = 0; PPC_ClickThroughs = 0; PPC_CostperClickThrough = 0; PPC_RegistrationRate = 0; GC_GuaranteedCPLBudget = 0;
                GC_CostperLead = 0; CSC_NonGuaranteedCPLBudget = 0; CSC_CostperLead = 0; TDM_DigitalMediaBudget = 0; TDM_CostperLead = 0; TP_PrintMediaBudget = 0; TP_CostperLead = 0;
            }
        }

        /// <summary>
        /// Read Events data from Benchmark Audience XML.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="value"></param>
        /// <param name="flag"></param>
        /// <param name="Quarter"></param>
        /// <param name="ModelId"></param>
        /// <returns></returns>
        public void getEventsData(string stage, string value, bool flag, string Quarter, int ModelId, bool updateflag)
        {
            if (stage == "NumberofContacts")
            {
                NumberofContacts = Convert.ToInt32(value);
            }
            else if (stage == "ContactToInquiryConversion")
            {
                ContactToInquiryConversion = Convert.ToDouble(value);
            }
            else if (stage == "EventsBudget")
            {
                EventsBudget = Convert.ToDouble(value);
            }
            if (flag == true)
            {
                InsertEvents(ModelId, Quarter, NumberofContacts, EventsBudget, ContactToInquiryConversion, updateflag);
                NumberofContacts = 0; EventsBudget = 0; ContactToInquiryConversion = 0;
            }
        }

        /// <summary>
        /// Save values to Model_Audience_Outbound table for Benchmark Audience XML.
        /// </summary>
        /// <param name="ModelId"></param>
        /// <param name="NormalErosion"></param>
        /// <param name="UnsubscribeRate"></param>
        /// <param name="NumberOfTouches"></param>
        /// <param name="ListAcquisitions"></param>
        /// <param name="Acquisition_NumberofTouches"></param>
        /// <param name="CTRDelivered"></param>
        /// <param name="RegistrationRate"></param>
        /// <param name="Quarter"></param>
        /// <param name="ListAcquisitionsNormalErosion"></param>
        /// <param name="ListAcquisitionsUnsubscribeRate"></param>
        /// <param name="ListAcquisitionsCTRDelivered"></param>
        /// <param name="Acquisition_CostperContact"></param>
        /// <param name="Acquisition_RegistrationRate"></param>
        /// <param name="IsBenchmarked"></param>
        /// <returns></returns>
        public void InsertOutBound(int ModelId, double NormalErosion, double UnsubscribeRate, int NumberOfTouches, int ListAcquisitions,
             int Acquisition_NumberofTouches, double CTRDelivered, double RegistrationRate, string Quarter,
             double ListAcquisitionsNormalErosion, double ListAcquisitionsUnsubscribeRate, double ListAcquisitionsCTRDelivered, double Acquisition_CostperContact,
             double Acquisition_RegistrationRate, bool IsBenchmarked, bool updateflag)
        {
            Model_Audience_Outbound objModel_Audience_Outbound = new Model_Audience_Outbound();
            if (updateflag)
            {
                objModel_Audience_Outbound = db.Model_Audience_Outbound.Where(m => m.ModelId == ModelId && m.Quarter == Quarter).FirstOrDefault();
            }
            objModel_Audience_Outbound.ModelId = ModelId;
            objModel_Audience_Outbound.NormalErosion = NormalErosion;
            objModel_Audience_Outbound.UnsubscribeRate = UnsubscribeRate;
            objModel_Audience_Outbound.NumberofTouches = NumberOfTouches;
            objModel_Audience_Outbound.ListAcquisitions = ListAcquisitions;
            objModel_Audience_Outbound.Acquisition_NumberofTouches = Acquisition_NumberofTouches;
            objModel_Audience_Outbound.CTRDelivered = CTRDelivered;
            objModel_Audience_Outbound.RegistrationRate = RegistrationRate;
            objModel_Audience_Outbound.Quarter = Quarter;
            objModel_Audience_Outbound.ListAcquisitionsNormalErosion = ListAcquisitionsNormalErosion;
            objModel_Audience_Outbound.ListAcquisitionsUnsubscribeRate = ListAcquisitionsUnsubscribeRate;
            objModel_Audience_Outbound.ListAcquisitionsCTRDelivered = ListAcquisitionsCTRDelivered;
            objModel_Audience_Outbound.Acquisition_CostperContact = Acquisition_CostperContact;
            objModel_Audience_Outbound.Acquisition_RegistrationRate = Acquisition_RegistrationRate;

            objModel_Audience_Outbound.IsBenchmarked = true;

            if (updateflag)
            {
                objModel_Audience_Outbound.ModifiedBy = Sessions.User.UserId;
                objModel_Audience_Outbound.ModifiedDate = DateTime.Now;
                db.Entry(objModel_Audience_Outbound).State = EntityState.Modified;
            }
            else
            {
                objModel_Audience_Outbound.CreatedBy = Sessions.User.UserId;
                objModel_Audience_Outbound.CreatedDate = DateTime.Now;
                db.Model_Audience_Outbound.Add(objModel_Audience_Outbound);
            }

            int resobjModel_Audience_Outbound = db.SaveChanges();
        }

        /// <summary>
        /// Save values to Model_Audience_Inbound table for Benchmark Audience XML.
        /// </summary>
        /// <param name="ModelId"></param>
        /// <param name="Quarter"></param>
        /// <param name="Impressions"></param>
        /// <param name="ClickThroughRate"></param>
        /// <param name="RegistrationRate"></param>
        /// <param name="PPC_ClickThroughs"></param>
        /// <param name="PPC_CostperClickThrough"></param>
        /// <param name="PPC_RegistrationRate"></param>
        /// <param name="GC_GuaranteedCPLBudget"></param>
        /// <param name="GC_CostperLead"></param>
        /// <param name="CSC_NonGuaranteedCPLBudget"></param>
        /// <param name="CSC_CostperLead"></param>
        /// <param name="TDM_DigitalMediaBudget"></param>
        /// <param name="TDM_CostperLead"></param>
        /// <param name="TP_PrintMediaBudget"></param>
        /// <param name="TP_CostperLead"></param>
        /// <returns></returns>
        public void InsertInBound(int ModelId, string Quarter, int Impressions, double ClickThroughRate, double RegistrationRate,
         int PPC_ClickThroughs, double PPC_CostperClickThrough, double PPC_RegistrationRate, int GC_GuaranteedCPLBudget,
         double GC_CostperLead, int CSC_NonGuaranteedCPLBudget, double CSC_CostperLead, int TDM_DigitalMediaBudget,
         double TDM_CostperLead, int TP_PrintMediaBudget, double TP_CostperLead, bool updateflag)
        {
            Model_Audience_Inbound objModel_Audience_Inbound = new Model_Audience_Inbound();
            if (updateflag)
            {
                objModel_Audience_Inbound = db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId).FirstOrDefault();
            }
            objModel_Audience_Inbound.ModelId = ModelId;
            objModel_Audience_Inbound.Quarter = Quarter;
            objModel_Audience_Inbound.Impressions = Impressions;
            objModel_Audience_Inbound.ClickThroughRate = ClickThroughRate;
            objModel_Audience_Inbound.RegistrationRate = RegistrationRate;
            objModel_Audience_Inbound.PPC_ClickThroughs = PPC_ClickThroughs;
            objModel_Audience_Inbound.PPC_CostperClickThrough = PPC_CostperClickThrough;
            objModel_Audience_Inbound.PPC_RegistrationRate = PPC_RegistrationRate;
            objModel_Audience_Inbound.GC_GuaranteedCPLBudget = GC_GuaranteedCPLBudget;
            objModel_Audience_Inbound.GC_CostperLead = GC_CostperLead;
            objModel_Audience_Inbound.CSC_NonGuaranteedCPLBudget = CSC_NonGuaranteedCPLBudget;
            objModel_Audience_Inbound.CSC_CostperLead = CSC_CostperLead;
            objModel_Audience_Inbound.TDM_DigitalMediaBudget = TDM_DigitalMediaBudget;
            objModel_Audience_Inbound.TDM_CostperLead = TDM_CostperLead;
            objModel_Audience_Inbound.TP_PrintMediaBudget = TP_PrintMediaBudget;
            objModel_Audience_Inbound.TP_CostperLead = TP_CostperLead;

            if (updateflag)
            {
                objModel_Audience_Inbound.ModifiedBy = Sessions.User.UserId;
                objModel_Audience_Inbound.ModifiedDate = DateTime.Now;
                db.Entry(objModel_Audience_Inbound).State = EntityState.Modified;
            }
            else
            {
                objModel_Audience_Inbound.CreatedBy = Sessions.User.UserId;
                objModel_Audience_Inbound.CreatedDate = DateTime.Now;
                db.Model_Audience_Inbound.Add(objModel_Audience_Inbound);
            }
            int resobjModel_Audience_Inbound = db.SaveChanges();
        }

        /// <summary>
        /// Save values to Model_Audience_Event table for Benchmark Audience XML.
        /// </summary>
        /// <param name="ModelId"></param>
        /// <param name="Quarter"></param>
        /// <param name="NumberofContacts"></param>
        /// <param name="EventsBudget"></param>
        /// <param name="ContactToInquiryConversion"></param>
        /// <returns></returns>
        public void InsertEvents(int ModelId, string Quarter, int NumberofContacts, double EventsBudget, double ContactToInquiryConversion, bool updateflag)
        {
            Model_Audience_Event objModel_Audience_Event = new Model_Audience_Event();
            if (updateflag)
            {
                objModel_Audience_Event = db.Model_Audience_Event.Where(m => m.ModelId == ModelId && m.Quarter == Quarter).FirstOrDefault();
            }
            objModel_Audience_Event.ModelId = ModelId;
            objModel_Audience_Event.Quarter = Quarter;
            objModel_Audience_Event.NumberofContacts = NumberofContacts;
            objModel_Audience_Event.EventsBudget = EventsBudget;
            objModel_Audience_Event.ContactToInquiryConversion = ContactToInquiryConversion;

            if (updateflag)
            {
                objModel_Audience_Event.ModifiedBy = Sessions.User.UserId;
                objModel_Audience_Event.ModifiedDate = DateTime.Now;
                db.Entry(objModel_Audience_Event).State = EntityState.Modified;
            }
            else
            {
                objModel_Audience_Event.CreatedBy = Sessions.User.UserId;
                objModel_Audience_Event.CreatedDate = DateTime.Now;
                db.Model_Audience_Event.Add(objModel_Audience_Event);
            }
            int resobjModel_Audience_Event = db.SaveChanges();
        }

        #endregion

        #endregion

        #region Model Review (Results)

        #region Load Model Review (Results)

        /// <summary>
        /// Added By: Kuber Joshi
        /// Action to show Results.
        /// </summary>

        public ActionResult Results(int mid1, int mid2)
        {
            bool noDataForModel1 = false;
            bool noDataForModel2 = false;

            Common.Permission permission = Common.GetPermission(ActionItem.Model);
            switch (permission)
            {
                case Common.Permission.FullAccess:
                    break;
                case Common.Permission.NoAccess:
                    return RedirectToAction("Index", "NoAccess");
                case Common.Permission.NotAnEntity:
                    break;
                case Common.Permission.ViewOnly:
                    ViewBag.IsViewOnly = "true";
                    break;
            }

            /* Bug 18:Model - results screen does not show for some models */

            //Check and insert Model Results data
            var objResultsM1 = db.ModelReviews.Where(m => m.Model_Funnel.ModelId == mid1).FirstOrDefault();
            if (objResultsM1 == null)
            {
                //Insert Results data for Model 1
                CalculateModelResults(mid1);
                noDataForModel1 = true;

            }
            var objResultsM2 = db.ModelReviews.Where(m => m.Model_Funnel.ModelId == mid2).FirstOrDefault();
            if (objResultsM2 == null)
            {
                //Insert Results data for Model 2
                CalculateModelResults(mid2);
                noDataForModel2 = true;
            }

            if (noDataForModel1 || noDataForModel2)
            {
                return RedirectToAction("Results", "Model", new { mid1 = mid1, mid2 = mid2 });
            }

            /* Bug 18:Model - results screen does not show for some models */

            var businessunit = db.Models.Where(b => b.ModelId == mid1 && b.IsDeleted == false).OrderByDescending(c => c.CreatedDate).Select(u => u.BusinessUnitId).FirstOrDefault();
            ViewBag.BusinessUnitId = Convert.ToString(businessunit);
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;
            BaselineModel objBaselineModel = FillInitialData(mid1, businessunit);
            ViewData["BaselineModel"] = objBaselineModel;

            List<ModelReview> modelReview = db.ModelReviews.Where(m => m.Model_Funnel.ModelId == mid1).ToList(); //Version 1
            List<ModelReview> compModelReview = db.ModelReviews.Where(m => m.Model_Funnel.ModelId == mid2).ToList(); //Version 2
            List<ModelResults> viewModelResultsList = new List<ModelResults>();
            foreach (var item in modelReview)
            {
                double percVal = 0;
                ModelResults objModelResults = new ModelResults();
                objModelResults.ModelFunnelId = item.ModelFunnelId;
                objModelResults.FunnelFieldId = item.FunnelFieldId;
                objModelResults.Funnel = item.Model_Funnel.Funnel.Title.ToLower();
                objModelResults.FieldCode = item.Funnel_Field.Field.Title.ToLower();
                objModelResults.FieldDesc = item.Funnel_Field.Field.Description;
                objModelResults.MarketingSourced = Math.Round(Convert.ToDouble(item.MarketingSourced), 2);
                objModelResults.MarketingDays = Math.Round(Convert.ToDouble(item.MarketingDays), 0);
                objModelResults.ProspectingSourced = Math.Round(Convert.ToDouble(item.ProspectingSourced), 2);
                objModelResults.ProspectingDays = Math.Round(Convert.ToDouble(item.ProspectingDays), 0);
                objModelResults.SalesSourced = Math.Round(Convert.ToDouble(item.SalesSourced), 2);
                objModelResults.SalesDays = Math.Round(Convert.ToDouble(item.SalesDays), 0);
                objModelResults.BlendedTotal_v1 = Math.Round(Convert.ToDouble(item.BlendedTotal), 2);
                objModelResults.BlendedDays_v1 = Math.Round(Convert.ToDouble(item.BlendedDays), 0);
                objModelResults.BlendedTotal_v2 = Math.Round(Convert.ToDouble(compModelReview.Where(p => p.FunnelFieldId == item.FunnelFieldId).Select(m => m.BlendedTotal).FirstOrDefault()), 2);
                objModelResults.BlendedDays_v2 = Math.Round(Convert.ToDouble(compModelReview.Where(p => p.FunnelFieldId == item.FunnelFieldId).Select(m => m.BlendedDays).FirstOrDefault()), 0);
                if (objModelResults.BlendedTotal_v1 != null && objModelResults.BlendedTotal_v2 != null && objModelResults.BlendedTotal_v2 > 0)
                {
                    if (objModelResults.BlendedTotal_v1 == objModelResults.BlendedTotal_v2)
                    {
                        objModelResults.BlendedTotalGrade = 0;
                    }
                    else if (objModelResults.BlendedTotal_v1 > objModelResults.BlendedTotal_v2)
                    {
                        objModelResults.BlendedTotalGrade = 1;
                    }
                    else
                    {
                        objModelResults.BlendedTotalGrade = 2;
                    }
                    percVal = Convert.ToDouble((objModelResults.BlendedTotal_v1 / objModelResults.BlendedTotal_v2) * 100);
                    objModelResults.BlendedTotal_Perc = Convert.ToString(Math.Round(percVal, 1)) + "%";
                }
                percVal = 0.0;
                if (objModelResults.BlendedDays_v1 != null && objModelResults.BlendedDays_v2 != null && objModelResults.BlendedDays_v2 > 0)
                {
                    if (objModelResults.BlendedDays_v1 == objModelResults.BlendedDays_v2)
                    {
                        objModelResults.BlendedDaysGrade = 0;
                    }
                    else if (objModelResults.BlendedDays_v1 > objModelResults.BlendedDays_v2)
                    {
                        objModelResults.BlendedDaysGrade = 1;
                    }
                    else
                    {
                        objModelResults.BlendedDaysGrade = 2;
                    }
                    percVal = Convert.ToDouble((objModelResults.BlendedDays_v1 / objModelResults.BlendedDays_v2) * 100);
                    objModelResults.BlendedDays_Perc = Convert.ToString(Math.Round(percVal, 1)) + "%";
                }
                viewModelResultsList.Add(objModelResults);
            }

            ViewBag.Version1Title = db.Models.Where(m => m.ModelId == mid1).Select(p => p.Version).FirstOrDefault();
            ViewBag.Version2Title = db.Models.Where(m => m.ModelId == mid2).Select(p => p.Version).FirstOrDefault();
            ViewBag.Version1Id = mid1;
            ViewBag.Version2Id = mid2;

            ViewBag.Msgcannotpublish = string.Empty;
            string Title = objBaselineModel.Versions.Where(s => s.IsLatest == true).Select(s => s.Title).FirstOrDefault();
            if (Title != null && Title != string.Empty)
            {
                ViewBag.Msgcannotpublish = string.Format(Common.objCached.ModelTacticTypeNotexist, Title);
            }

            return View(viewModelResultsList.AsEnumerable());
        }


        #endregion

        #region Save Model Calculations

        /// <summary>
        /// Added By: Kuber Joshi
        /// To calculate & save Model Results.
        /// </summary>
        private void CalculateModelResults(int id)
        {
            if (id > 0)
            {
                #region Funnel
                string Marketing = Convert.ToString(Enums.Funnel.Marketing).ToLower();
                string Teleprospecting = Convert.ToString(Enums.Funnel.Teleprospecting).ToLower();
                string Sales = Convert.ToString(Enums.Funnel.Sales).ToLower();
                #endregion

                #region Funnel Field
                string FF_DatabaseSize = Convert.ToString(Enums.FunnelField.DatabaseSize).ToLower();
                string ConversionGate_SUS = Convert.ToString(Enums.FunnelField.ConversionGate_SUS).ToLower();
                string FF_OutboundGeneratedInquiries = Convert.ToString(Enums.FunnelField.OutboundGeneratedInquiries).ToLower();
                string FF_InboundInquiries = Convert.ToString(Enums.FunnelField.InboundInquiries).ToLower();
                string FF_TotalInquiries = Convert.ToString(Enums.FunnelField.TotalInquiries).ToLower();
                string ConversionGate_INQ = Convert.ToString(Enums.FunnelField.ConversionGate_INQ).ToLower();
                string FF_AQL = Convert.ToString(Enums.FunnelField.AQL).ToLower();
                string ConversionGate_AQL = Convert.ToString(Enums.FunnelField.ConversionGate_AQL).ToLower();
                string FF_TAL = Convert.ToString(Enums.FunnelField.TAL).ToLower();
                string ConversionGate_TAL = Convert.ToString(Enums.FunnelField.ConversionGate_TAL).ToLower();
                string FF_TQL = Convert.ToString(Enums.FunnelField.TQL).ToLower();
                string FF_TGL = Convert.ToString(Enums.FunnelField.TGL).ToLower();
                string FF_MQL = Convert.ToString(Enums.FunnelField.MQL).ToLower();
                string ConversionGate_TQL = Convert.ToString(Enums.FunnelField.ConversionGate_TQL).ToLower();
                string FF_SAL = Convert.ToString(Enums.FunnelField.SAL).ToLower();
                string FF_SGL = Convert.ToString(Enums.FunnelField.SGL).ToLower();
                string ConversionGate_SAL = Convert.ToString(Enums.FunnelField.ConversionGate_SAL).ToLower();
                string FF_SQL = Convert.ToString(Enums.FunnelField.SQL).ToLower();
                string ConversionGate_SQL = Convert.ToString(Enums.FunnelField.ConversionGate_SQL).ToLower();
                string ClosedWon = Convert.ToString(Enums.FunnelField.ClosedWon).ToLower();
                string BlendedFunnelCR_Times = Convert.ToString(Enums.FunnelField.BlendedFunnelCR_Times).ToLower();
                string FF_AverageDealsSize = Convert.ToString(Enums.FunnelField.AverageDealsSize).ToLower();
                string ExpectedRevenue = Convert.ToString(Enums.FunnelField.ExpectedRevenue).ToLower();
                #endregion

                int FunnelFieldId, Marketing_ModelFunnelId, Teleprospecting_ModelFunnelId, Sales_ModelFunnelId;
                ModelCalculation objModelCalculation = new ModelCalculation();
                objModelCalculation.ModelId = id;

                try
                {
                    Marketing_ModelFunnelId = Convert.ToInt32(db.Model_Funnel.Where(m => m.ModelId == id && m.Funnel.Title == Marketing).Select(m => m.ModelFunnelId).FirstOrDefault()); //Marketing Funnel
                    Teleprospecting_ModelFunnelId = Convert.ToInt32(db.Model_Funnel.Where(m => m.ModelId == id && m.Funnel.Title == Teleprospecting).Select(m => m.ModelFunnelId).FirstOrDefault()); //Teleprospecting Funnel
                    Sales_ModelFunnelId = Convert.ToInt32(db.Model_Funnel.Where(m => m.ModelId == id && m.Funnel.Title == Sales).Select(m => m.ModelFunnelId).FirstOrDefault()); //Sales Funnel

                    //Clear existing review data for all Funnels
                    var objMarketing = db.ModelReviews.Where(mr => mr.ModelFunnelId == Marketing_ModelFunnelId).ToList();
                    foreach (var item in objMarketing)
                    {
                        db.ModelReviews.Remove(item);
                        db.SaveChanges();
                    }
                    var objTeleprospecting = db.ModelReviews.Where(mr => mr.ModelFunnelId == Teleprospecting_ModelFunnelId).ToList();
                    foreach (var item in objTeleprospecting)
                    {
                        db.ModelReviews.Remove(item);
                        db.SaveChanges();
                    }
                    var objSales = db.ModelReviews.Where(mr => mr.ModelFunnelId == Sales_ModelFunnelId).ToList();
                    foreach (var item in objSales)
                    {
                        db.ModelReviews.Remove(item);
                        db.SaveChanges();
                    }

                    #region Marketing Funnel

                    //Database Size
                    double DBSIZEPST = Math.Round(objModelCalculation.GetDBSIZEPST(id), 2);
                    double DBACQPST = Math.Round(objModelCalculation.GetDBACQPST(id), 2);
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == FF_DatabaseSize).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double MarketingSourced_Mkt_DatabaseSize = objModelCalculation.MarketingSourced_Mkt_DatabaseSize(false, DBSIZEPST, DBACQPST);
                    SaveModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, MarketingSourced_Mkt_DatabaseSize, 0, 0, 0, 0, 0);

                    //Conversion Gate 1 (SUS)
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == ConversionGate_SUS).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double MarketingSourced_Mkt_SUS_ConversionGate = objModelCalculation.MarketingSourced_Mkt_SUS_ConversionGate(false);
                    SaveModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, MarketingSourced_Mkt_SUS_ConversionGate, 0, 0, 0, 0, 0);

                    //Outbound Generated Inquiries
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == FF_OutboundGeneratedInquiries).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double MarketingSourced_Mkt_OutboundGeneratedInquiries = objModelCalculation.MarketingSourced_Mkt_OutboundGeneratedInquiries();
                    double MarketingStageDays_Mkt_OutboundGeneratedInquiries = objModelCalculation.MarketingStageDays_Mkt_OutboundGeneratedInquiries(false);
                    SaveModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, MarketingSourced_Mkt_OutboundGeneratedInquiries, MarketingStageDays_Mkt_OutboundGeneratedInquiries, 0, 0, 0, 0);

                    //Inbound Inquiries
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == FF_InboundInquiries).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double MarketingSourced_Mkt_InboundInquiries = objModelCalculation.MarketingSourced_Mkt_InboundInquiries(false);
                    double MarketingStageDays_Mkt_InboundInquiries = objModelCalculation.MarketingStageDays_Mkt_InboundInquiries(false);
                    SaveModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, MarketingSourced_Mkt_InboundInquiries, MarketingStageDays_Mkt_InboundInquiries, 0, 0, 0, 0);

                    //Total Inquiries
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == FF_TotalInquiries).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double MarketingSourced_Mkt_TotalInquiries = objModelCalculation.MarketingSourced_Mkt_TotalInquiries();
                    double MarketingStageDays_Mkt_TotalInquiries = objModelCalculation.MarketingStageDays_Mkt_TotalInquiries(MarketingSourced_Mkt_TotalInquiries);
                    SaveModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, MarketingSourced_Mkt_TotalInquiries, MarketingStageDays_Mkt_TotalInquiries, 0, 0, 0, 0);

                    //Conversion Gate 2 (INQ)
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == ConversionGate_INQ).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double MarketingSourced_Mkt_INQ_ConversionGate = objModelCalculation.MarketingSourced_Mkt_INQ_ConversionGate(false);
                    SaveModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, MarketingSourced_Mkt_INQ_ConversionGate, 0, 0, 0, 0, 0);

                    //AQL
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == FF_AQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double MarketingSourced_Mkt_AQL = objModelCalculation.MarketingSourced_Mkt_AQL();
                    double MarketingStageDays_Mkt_AQL = objModelCalculation.MarketingStageDays_Mkt_AQL(false);
                    SaveModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, MarketingSourced_Mkt_AQL, MarketingStageDays_Mkt_AQL, 0, 0, 0, 0);

                    //Conversion Gate 3 (AQL)
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == ConversionGate_AQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double MarketingSourced_Mkt_AQL_ConversionGate = objModelCalculation.MarketingSourced_Mkt_AQL_ConversionGate(false);
                    SaveModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, MarketingSourced_Mkt_AQL_ConversionGate, 0, 0, 0, 0, 0);

                    #endregion

                    //#region Teleprospecting Funnel

                    ////TAL
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Teleprospecting && f.Field.Title == FF_TAL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double MarketingSourced_Tele_TAL = objModelCalculation.MarketingSourced_Tele_TAL();
                    //double MarketingStageDays_Tele_TAL = objModelCalculation.MarketingStageDays_Tele_TAL(false);
                    //SaveModelCalculations(Teleprospecting_ModelFunnelId, FunnelFieldId, MarketingSourced_Tele_TAL, MarketingStageDays_Tele_TAL, 0, 0, 0, 0);

                    ////Conversion Gate 1 (TAL)
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Teleprospecting && f.Field.Title == ConversionGate_TAL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double MarketingSourced_Tele_TAL_ConversionGate = objModelCalculation.MarketingSourced_Tele_TAL_ConversionGate(false);
                    //SaveModelCalculations(Teleprospecting_ModelFunnelId, FunnelFieldId, MarketingSourced_Tele_TAL_ConversionGate, 0, 0, 0, 0, 0);

                    ////TQL
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Teleprospecting && f.Field.Title == FF_TQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double MarketingSourced_Tele_TQL = objModelCalculation.MarketingSourced_Tele_TQL();
                    //SaveModelCalculations(Teleprospecting_ModelFunnelId, FunnelFieldId, MarketingSourced_Tele_TQL, 0, 0, 0, 0, 0);

                    ////TGL
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Teleprospecting && f.Field.Title == FF_TGL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double TeleprospectingSourced_Tele_TGL = objModelCalculation.TeleprospectingSourced_Tele_TGL(false);
                    //SaveModelCalculations(Teleprospecting_ModelFunnelId, FunnelFieldId, 0, 0, TeleprospectingSourced_Tele_TGL, 0, 0, 0);

                    ////Total Marketing Qualitfied Leads (MQLs)
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Teleprospecting && f.Field.Title == FF_MQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double MarketingSourced_Tele_TotalMarketingQualifiedLeadsMQL = objModelCalculation.MarketingSourced_Tele_TotalMarketingQualifiedLeadsMQL();
                    //double MarketingStageDays_Tele_MQL = objModelCalculation.MarketingStageDays_Tele_MQL();
                    //double TeleprospectingSourced_Tele_TotalMarketingQualifiedLeadsMQL = objModelCalculation.TeleprospectingSourced_Tele_TotalMarketingQualifiedLeadsMQL();
                    //double TeleprospectingStageDays_Tele_TotalMarketingQualifiedLeadsMQL = objModelCalculation.TeleprospectingStageDays_Tele_TotalMarketingQualifiedLeadsMQL();
                    //SaveModelCalculations(Teleprospecting_ModelFunnelId, FunnelFieldId, MarketingSourced_Tele_TotalMarketingQualifiedLeadsMQL, MarketingStageDays_Tele_MQL, TeleprospectingSourced_Tele_TotalMarketingQualifiedLeadsMQL, TeleprospectingStageDays_Tele_TotalMarketingQualifiedLeadsMQL, 0, 0);

                    ////Conversion Gate 2 (TQL)
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Teleprospecting && f.Field.Title == ConversionGate_TQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double MarketingSourced_Tele_TQL_ConversionGate = objModelCalculation.MarketingSourced_Tele_TQL_ConversionGate(false);
                    //double TeleprospectingSourced_Tele_TQL_ConversionGate = objModelCalculation.TeleprospectingSourced_Tele_TQL_ConversionGate(false);
                    //SaveModelCalculations(Teleprospecting_ModelFunnelId, FunnelFieldId, MarketingSourced_Tele_TQL_ConversionGate, 0, TeleprospectingSourced_Tele_TQL_ConversionGate, 0, 0, 0);

                    //#endregion

                    //#region Sales Funnel

                    ////SAL
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Sales && f.Field.Title == FF_SAL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double MarketingSourced_Sales_SAL = objModelCalculation.MarketingSourced_Sales_SAL();
                    //double MarketingStageDays_Sales_SAL = objModelCalculation.MarketingStageDays_Sales_SAL();
                    //double TeleprospectingSourced_Sales_SAL = objModelCalculation.TeleprospectingSourced_Sales_SAL();
                    //double TeleprospectingStageDays_Sales_SAL = objModelCalculation.TeleprospectingStageDays_Sales_SAL();
                    //SaveModelCalculations(Sales_ModelFunnelId, FunnelFieldId, MarketingSourced_Sales_SAL, MarketingStageDays_Sales_SAL, TeleprospectingSourced_Sales_SAL, TeleprospectingStageDays_Sales_SAL, 0, 0);

                    ////SGL
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Sales && f.Field.Title == FF_SGL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double SalesSourced_Sales_SGL = objModelCalculation.SalesSourced_Sales_SGL(false);
                    //double SalesStageDays_Sales_SGL = objModelCalculation.SalesStageDays_Sales_SGL();
                    //SaveModelCalculations(Sales_ModelFunnelId, FunnelFieldId, 0, 0, 0, 0, SalesSourced_Sales_SGL, SalesStageDays_Sales_SGL);

                    ////Conversion Gate 1 (SAL)
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Sales && f.Field.Title == ConversionGate_SAL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double MarketingSourced_Sales_SAL_ConversionGate = objModelCalculation.MarketingSourced_Sales_SAL_ConversionGate(false);
                    //double TeleprospectingSourced_Sales_SAL_ConversionGate = objModelCalculation.TeleprospectingSourced_Sales_SAL_ConversionGate(false);
                    //double SalesSourced_Sales_SAL_ConversionGate = objModelCalculation.SalesSourced_Sales_SAL_ConversionGate(false);
                    //SaveModelCalculations(Sales_ModelFunnelId, FunnelFieldId, MarketingSourced_Sales_SAL_ConversionGate, 0, TeleprospectingSourced_Sales_SAL_ConversionGate, 0, SalesSourced_Sales_SAL_ConversionGate, 0);

                    ////SQL
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Sales && f.Field.Title == FF_SQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double MarketingSourced_Sales_SQL = objModelCalculation.MarketingSourced_Sales_SQL();
                    //double MarketingStageDays_Sales_SQL = objModelCalculation.MarketingStageDays_Sales_SQL();
                    //double TeleprospectingSourced_Sales_SQL = objModelCalculation.TeleprospectingSourced_Sales_SQL();
                    //double TeleprospectingStageDays_Sales_SQL = objModelCalculation.TeleprospectingStageDays_Sales_SQL();
                    //double SalesSourced_Sales_SQL = objModelCalculation.SalesSourced_Sales_SQL();
                    //double SalesStageDays_Sales_SQL = objModelCalculation.SalesStageDays_Sales_SQL();
                    //SaveModelCalculations(Sales_ModelFunnelId, FunnelFieldId, MarketingSourced_Sales_SQL, MarketingStageDays_Sales_SQL, TeleprospectingSourced_Sales_SQL, TeleprospectingStageDays_Sales_SQL, SalesSourced_Sales_SQL, SalesStageDays_Sales_SQL);

                    ////Conversion Gate 2 (SQL)
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Sales && f.Field.Title == ConversionGate_SQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double MarketingSourced_Sales_SQL_ConversionGate = objModelCalculation.MarketingSourced_Sales_SQL_ConversionGate(false);
                    //double TeleprospectingSourced_Sales_SQL_ConversionGate = objModelCalculation.TeleprospectingSourced_Sales_SQL_ConversionGate(false);
                    //double SalesSourced_Sales_SQL_ConversionGate = objModelCalculation.SalesSourced_Sales_SQL_ConversionGate(false);
                    //SaveModelCalculations(Sales_ModelFunnelId, FunnelFieldId, MarketingSourced_Sales_SQL_ConversionGate, 0, TeleprospectingSourced_Sales_SQL_ConversionGate, 0, SalesSourced_Sales_SQL_ConversionGate, 0);

                    ////Closed Won
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Sales && f.Field.Title == ClosedWon).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double MarketingSourced_Sales_CW = objModelCalculation.MarketingSourced_Sales_CW();
                    //double TeleprospectingSourced_Sales_CW = objModelCalculation.TeleprospectingSourced_Sales_CW();
                    //double SalesSourced_Sales_CW = objModelCalculation.SalesSourced_Sales_CW();
                    //SaveModelCalculations(Sales_ModelFunnelId, FunnelFieldId, MarketingSourced_Sales_CW, 0, TeleprospectingSourced_Sales_CW, 0, SalesSourced_Sales_CW, 0);

                    //#endregion

                    //BLENDED FUNNEL CONVERSION RATE AND TIMES
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == BlendedFunnelCR_Times).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double MarketingStageDays_BlendedFull = objModelCalculation.MarketingStageDays_BlendedFull();
                    //double SalesStageDays_BlendedFull = objModelCalculation.SalesStageDays_BlendedFull();
                    SaveModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, 0, MarketingStageDays_BlendedFull, 0, 0, 0, 0);

                    //Average Deals Size
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == FF_AverageDealsSize).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double MarketingSourced_Average_Deal_Size = objModelCalculation.MarketingSourced_Average_Deal_Size(false);
                    //double TeleprospectingSourced_Average_Deal_Size = objModelCalculation.TeleprospectingSourced_Average_Deal_Size(false);
                    //double SalesSourced_Average_Deal_Size = objModelCalculation.SalesSourced_Average_Deal_Size(false);
                    SaveModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, MarketingSourced_Average_Deal_Size, 0, 0, 0, 0, 0);

                    //Expected Revenue From Model
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == ExpectedRevenue).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double MarketingSourced_Expected_Revenue = objModelCalculation.MarketingSourced_Expected_Revenue();
                    //double TeleprospectingSourced_Expected_Revenue = objModelCalculation.TeleprospectingSourced_Expected_Revenue();
                    //double SalesSourced_Expected_Revenue = objModelCalculation.SalesSourced_Expected_Revenue();
                    SaveModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, MarketingSourced_Expected_Revenue, 0, 0, 0, 0, 0);

                    #region Blended

                    //Database Size - Kunal
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == FF_DatabaseSize).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double BlendedTotal_Mkt_DatabaseSize = objModelCalculation.BlendedTotal_Mkt_DatabaseSize();
                    UpdateModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, BlendedTotal_Mkt_DatabaseSize, 0);

                    //Conversion Gate 1 (SUS)
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == ConversionGate_SUS).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double BlendedTotal_Mkt_SUS_ConversionGate = objModelCalculation.BlendedTotal_Mkt_SUS_ConversionGate();
                    UpdateModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, BlendedTotal_Mkt_SUS_ConversionGate, 0);

                    //Outbound Generated Inquiries
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == FF_OutboundGeneratedInquiries).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double BlendedTotal_Mkt_OutboundGeneratedInquiries = objModelCalculation.BlendedTotal_Mkt_OutboundGeneratedInquiries();
                    double BlendedTotalDays_Mkt_OutboundGeneratedInquiries = objModelCalculation.BlendedTotalDays_Mkt_OutboundGeneratedInquiries();
                    UpdateModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, BlendedTotal_Mkt_OutboundGeneratedInquiries, BlendedTotalDays_Mkt_OutboundGeneratedInquiries);

                    //Inbound Inquiries
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == FF_InboundInquiries).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double BlendedTotal_Mkt_InboundInquiries = objModelCalculation.BlendedTotal_Mkt_InboundInquiries();
                    double BlendedTotalDays_Mkt_InboundInquiries = objModelCalculation.BlendedTotalDays_Mkt_InboundInquiries();
                    UpdateModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, BlendedTotal_Mkt_InboundInquiries, BlendedTotalDays_Mkt_InboundInquiries);

                    //Total Inquiries
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == FF_TotalInquiries).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double BlendedTotal_Mkt_TotalInquiries = objModelCalculation.BlendedTotal_Mkt_TotalInquiries();
                    double BlendedTotalDays_Mkt_TotalInquiries = objModelCalculation.BlendedTotalDays_Mkt_TotalInquiries();
                    UpdateModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, BlendedTotal_Mkt_TotalInquiries, BlendedTotalDays_Mkt_TotalInquiries);

                    //Conversion Gate 2 (INQ)
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == ConversionGate_INQ).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double BlendedTotal_Mkt_INQ_ConversionGate = objModelCalculation.BlendedTotal_Mkt_INQ_ConversionGate();
                    UpdateModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, BlendedTotal_Mkt_INQ_ConversionGate, 0);

                    //AQL
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == FF_AQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double BlendedTotal_Mkt_AQL = objModelCalculation.BlendedTotal_Mkt_AQL();
                    double BlendedTotalDays_Mkt_AQL = objModelCalculation.BlendedTotalDays_Mkt_AQL();
                    UpdateModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, BlendedTotal_Mkt_AQL, BlendedTotalDays_Mkt_AQL);

                    //Conversion Gate 3 (AQL)
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == ConversionGate_AQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double BlendedTotal_Mkt_AQL_ConversionGate = objModelCalculation.BlendedTotal_Mkt_AQL_ConversionGate();
                    UpdateModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, BlendedTotal_Mkt_AQL_ConversionGate, 0);

                    //TAL
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Teleprospecting && f.Field.Title == FF_TAL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double BlendedTotal_Tele_TAL = objModelCalculation.BlendedTotal_Tele_TAL();
                    //double BlendedTotalDays_Tele_TAL = objModelCalculation.BlendedTotalDays_Tele_TAL();
                    //UpdateModelCalculations(Teleprospecting_ModelFunnelId, FunnelFieldId, BlendedTotal_Tele_TAL, BlendedTotalDays_Tele_TAL);

                    ////Conversion Gate 1 (TAL)
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Teleprospecting && f.Field.Title == ConversionGate_TAL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double BlendedTotal_Tele_TAL_ConversionGate = objModelCalculation.BlendedTotal_Tele_TAL_ConversionGate();
                    //UpdateModelCalculations(Teleprospecting_ModelFunnelId, FunnelFieldId, BlendedTotal_Tele_TAL_ConversionGate, 0);

                    ////TQL
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Teleprospecting && f.Field.Title == FF_TQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double BlendedTotal_Tele_TQL = objModelCalculation.BlendedTotal_Tele_TQL();
                    //UpdateModelCalculations(Teleprospecting_ModelFunnelId, FunnelFieldId, BlendedTotal_Tele_TQL, 0);

                    ////TGL
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Teleprospecting && f.Field.Title == FF_TGL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double BlendedTotal_Tele_TGL = objModelCalculation.BlendedTotal_Tele_TGL();
                    //UpdateModelCalculations(Teleprospecting_ModelFunnelId, FunnelFieldId, BlendedTotal_Tele_TGL, 0);

                    ////Total Marketing Qualitfied Leads (MQLs)
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Teleprospecting && f.Field.Title == FF_MQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double BlendedTotal_Tele_TotalMarketingQualifiedLeadsMQL = objModelCalculation.BlendedTotal_Tele_TotalMarketingQualifiedLeadsMQL();
                    //double BlendedTotalDays_Tele_TotalMarketingQualifiedLeadsMQL = objModelCalculation.BlendedTotalDays_Tele_TotalMarketingQualifiedLeadsMQL(BlendedTotal_Tele_TotalMarketingQualifiedLeadsMQL);
                    //UpdateModelCalculations(Teleprospecting_ModelFunnelId, FunnelFieldId, BlendedTotal_Tele_TotalMarketingQualifiedLeadsMQL, BlendedTotalDays_Tele_TotalMarketingQualifiedLeadsMQL);

                    ////SAL
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Sales && f.Field.Title == FF_SAL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double BlendedTotal_Sales_SAL = objModelCalculation.BlendedTotal_Sales_SAL();
                    //double BlendedTotalDays_Sales_SAL = objModelCalculation.BlendedTotalDays_Sales_SAL(BlendedTotal_Sales_SAL);
                    //UpdateModelCalculations(Sales_ModelFunnelId, FunnelFieldId, BlendedTotal_Sales_SAL, BlendedTotalDays_Sales_SAL);

                    ////Conversion Gate 2 (TQL)
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Teleprospecting && f.Field.Title == ConversionGate_TQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double BlendedTotal_Tele_TQL_ConversionGate = objModelCalculation.BlendedTotal_Tele_TQL_ConversionGate();
                    //UpdateModelCalculations(Teleprospecting_ModelFunnelId, FunnelFieldId, BlendedTotal_Tele_TQL_ConversionGate, 0);

                    ////SGL
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Sales && f.Field.Title == FF_SGL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double BlendedTotal_Sales_SGL = objModelCalculation.BlendedTotal_Sales_SGL();
                    //double BlendedTotalDays_Sales_SGL = objModelCalculation.BlendedTotalDays_Sales_SGL();
                    //UpdateModelCalculations(Sales_ModelFunnelId, FunnelFieldId, BlendedTotal_Sales_SGL, BlendedTotalDays_Sales_SGL);

                    ////SQL
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Sales && f.Field.Title == FF_SQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double BlendedTotal_Sales_SQL = objModelCalculation.BlendedTotal_Sales_SQL();
                    //double BlendedTotalDays_Sales_SQL = objModelCalculation.BlendedTotalDays_Sales_SQL(BlendedTotal_Sales_SQL);
                    //UpdateModelCalculations(Sales_ModelFunnelId, FunnelFieldId, BlendedTotal_Sales_SQL, BlendedTotalDays_Sales_SQL);

                    ////Conversion Gate 1 (SAL)
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Sales && f.Field.Title == ConversionGate_SAL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double BlendedTotal_Sales_SAL_ConversionGate = objModelCalculation.BlendedTotal_Sales_SAL_ConversionGate();
                    //UpdateModelCalculations(Sales_ModelFunnelId, FunnelFieldId, BlendedTotal_Sales_SAL_ConversionGate, 0);

                    ////Closed Won
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Sales && f.Field.Title == ClosedWon).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double BlendedTotal_Sales_CW = objModelCalculation.BlendedTotal_Sales_CW();
                    //UpdateModelCalculations(Sales_ModelFunnelId, FunnelFieldId, BlendedTotal_Sales_CW, 0);

                    ////Conversion Gate 2 (SQL)
                    //FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Sales && f.Field.Title == ConversionGate_SQL).Select(m => m.FunnelFieldId).FirstOrDefault());
                    //double BlendedTotal_Sales_SQL_ConversionGate = objModelCalculation.BlendedTotal_Sales_SQL_ConversionGate();
                    //UpdateModelCalculations(Sales_ModelFunnelId, FunnelFieldId, BlendedTotal_Sales_SQL_ConversionGate, 0);

                    #endregion

                    //BLENDED FUNNEL CONVERSION RATE AND TIMES
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == BlendedFunnelCR_Times).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double BlendedTotal_BlendedFull = objModelCalculation.BlendedTotal_BlendedFull();
                    double BlendedTotalDays_BlendedFull = objModelCalculation.BlendedTotalDays_BlendedFull(); //Check
                    UpdateModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, BlendedTotal_BlendedFull, BlendedTotalDays_BlendedFull);

                    //Expected Revenue From Model
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == ExpectedRevenue).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double BlendedTotal_Expected_Revenue = objModelCalculation.BlendedTotal_Expected_Revenue();
                    UpdateModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, BlendedTotal_Expected_Revenue, 0);

                    //Average Deals Size
                    FunnelFieldId = Convert.ToInt32(db.Funnel_Field.Where(f => f.Funnel.Title == Marketing && f.Field.Title == FF_AverageDealsSize).Select(m => m.FunnelFieldId).FirstOrDefault());
                    double BlendedTotal_AverageDealSize = objModelCalculation.BlendedTotal_AverageDealSize();
                    UpdateModelCalculations(Marketing_ModelFunnelId, FunnelFieldId, BlendedTotal_AverageDealSize, 0);
                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                }
            }
        }

        private void SaveModelCalculations(int modelFunnelId, int funnelFieldId, double marketingSourced, double marketingDays, double prospectingSourced, double prospectingDays, double salesSourced, double salesDays)
        {
            try
            {
                double defBlendedTotal = 0;
                double defBlendedDays = 0;

                marketingSourced = double.IsNaN(marketingSourced) ? 0 : marketingSourced;
                marketingDays = double.IsNaN(marketingDays) ? 0 : marketingDays;
                prospectingSourced = double.IsNaN(prospectingSourced) ? 0 : prospectingSourced;
                prospectingDays = double.IsNaN(prospectingDays) ? 0 : prospectingDays;
                salesSourced = double.IsNaN(salesSourced) ? 0 : salesSourced;
                salesDays = double.IsNaN(salesDays) ? 0 : salesDays;

                var objDuplicateCheck = db.ModelReviews.Where(m => m.ModelFunnelId == modelFunnelId && m.ModelFunnelId == funnelFieldId).FirstOrDefault();
                if (objDuplicateCheck == null)
                {
                    ModelReview objModelReview = new ModelReview();
                    objModelReview.ModelFunnelId = modelFunnelId;
                    objModelReview.FunnelFieldId = funnelFieldId;
                    objModelReview.MarketingSourced = marketingSourced;
                    objModelReview.MarketingDays = marketingDays;
                    objModelReview.ProspectingSourced = prospectingSourced;
                    objModelReview.ProspectingDays = prospectingDays;
                    objModelReview.SalesSourced = salesSourced;
                    objModelReview.SalesDays = salesDays;
                    objModelReview.BlendedTotal = defBlendedTotal;
                    objModelReview.BlendedDays = defBlendedDays;
                    objModelReview.IsBaseline = false;
                    objModelReview.CreatedDate = DateTime.Now;
                    objModelReview.CreatedBy = Sessions.User.UserId;
                    db.Entry(objModelReview).State = EntityState.Added;
                    db.ModelReviews.Add(objModelReview);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
        }

        private void UpdateModelCalculations(int modelFunnelId, int funnelFieldId, double blendedTotal, double blendedDays)
        {
            try
            {
                blendedTotal = double.IsNaN(blendedTotal) ? 0 : blendedTotal;
                blendedDays = double.IsNaN(blendedDays) ? 0 : blendedDays;

                var objModelReview = db.ModelReviews.Where(m => m.ModelFunnelId == modelFunnelId && m.FunnelFieldId == funnelFieldId).FirstOrDefault();
                if (objModelReview != null)
                {
                    objModelReview.BlendedTotal = blendedTotal;
                    objModelReview.BlendedDays = blendedDays;
                    db.Entry(objModelReview).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
        }

        #endregion

        #endregion

        #region Tactic Selection

        /// <summary>
        /// Added By: Nirav Shah
        /// Action to show Tactics screen.
        /// </summary>
        public ActionResult Tactics(int id = 0)
        {
            Common.Permission permission = Common.GetPermission(ActionItem.Model);
            switch (permission)
            {
                case Common.Permission.FullAccess:
                    break;
                case Common.Permission.NoAccess:
                    return RedirectToAction("Index", "NoAccess");
                case Common.Permission.NotAnEntity:
                    break;
                case Common.Permission.ViewOnly:
                    ViewBag.IsViewOnly = "true";
                    break;
            }
            int Modelid = id;

            var objModel = db.Models.SingleOrDefault(varM => varM.ModelId == Modelid);
            ViewBag.IsModelIntegrated = objModel.IntegrationInstanceId == null ? false : true;

            Tactic_TypeModel objTacticTypeModel = FillInitialTacticData(id);
            objTacticTypeModel.ModelId = id;
            ViewBag.Version = id;
            string Title = objTacticTypeModel.Versions.Where(s => s.IsLatest == true).Select(s => s.Title).FirstOrDefault();
            ViewBag.ModelPublishComfirmation = Common.objCached.ModelPublishComfirmation;
            ViewBag.Published = Enums.ModelStatus.Published.ToString().ToLower();
            ViewBag.Flag = false;
            if (id != 0)
            {
                ViewBag.Flag = chekckParentPublishModel(id);
            }
            string StageType = Enums.StageType.CR.ToString();
            string ModelTitle = db.Models.Where(m => m.IsDeleted == false && m.ModelId == Modelid).Select(s => s.Title).FirstOrDefault();
            string Marketing = Convert.ToString(Enums.Funnel.Marketing).ToLower();
            Model_Funnel_Stage objStage = db.Model_Funnel_Stage.Where(s => s.StageType == StageType && s.Model_Funnel.ModelId == Modelid && s.AllowedTargetStage == true && s.Model_Funnel.Funnel.Title == Marketing).OrderBy(s => s.Stage.Level).Distinct().FirstOrDefault();
            if (objStage == null)
            {
                TempData["ErrorMessage"] = string.Format(Common.objCached.StageNotExist);
            }
            return View(objTacticTypeModel);
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Fill initialTacticdata version wise.
        /// </summary>
        public Tactic_TypeModel FillInitialTacticData(int Modelid = 0)
        {
            Tactic_TypeModel objTacticTypeModel = new Tactic_TypeModel();
            Guid BusinessUnitId = db.Models.Where(m => m.IsDeleted == false && m.ModelId == Modelid).Select(m => m.BusinessUnitId).FirstOrDefault();
            string Status = db.Models.Where(m => m.IsDeleted == false && m.ModelId == Modelid).Select(m => m.Status).FirstOrDefault();
            objTacticTypeModel.Versions = GetVersions(Modelid, Sessions.User.BusinessUnitId);
            objTacticTypeModel.Status = Status;
            return objTacticTypeModel;
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to show Version.
        /// </summary>
        public JsonResult FillVersion(int id)
        {
            var objTactic = db.Models.Where(s => s.ModelId == id).Select(s => s).ToList();
            var obj = objTactic.Select(p => new
            {
                Title = p.Title,
                Status = p.Status,
            }).Select(p => p).Distinct();
            return Json(obj, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to show Tactics List by ModelId.
        /// </summary>
        public JsonResult GetTacticDatabyid(int id)
        {
            /*  TFS Bug - 179 : Improper behavior when editing Tactic in model 
                Changed By : Nirav shah on 6 Feb 2014
                Change : add some check in lstTitle,TacticList,Masterlist and add new deletedTacticlist
            */
            //return all tactics of model
            var lstModelsTactic = from f in db.TacticTypes where f.ModelId == id && f.ClientId == Sessions.User.ClientId && (f.IsDeleted == null || f.IsDeleted == false) select f;

            //distinct title of all tactic of model
            var lstDistinctModelTacticTitle = lstModelsTactic.Select(s => s.Title).Distinct().ToList();

            //returns deleted list of matched tactics
            var deletedTacticlist = from f in db.TacticTypes where !lstDistinctModelTacticTitle.Contains(f.Title) && f.ClientId == Sessions.User.ClientId && f.ModelId == null && (f.IsDeleted == null || f.IsDeleted == false) select f;

            //distinct title of deleted tactic of model
            var deletedTacticTitle = deletedTacticlist.Select(s => s.Title).Distinct().ToList();

            //returns all models of the client
            var objTacticList = deletedTacticlist.Union(lstModelsTactic).ToList();

            //returns master list of matched tactics
            var lstMstaerTactic = from f in db.TacticTypes where !lstDistinctModelTacticTitle.Contains(f.Title) && !deletedTacticTitle.Contains(f.Title) && f.ClientId == null && (f.IsDeleted == null || f.IsDeleted == false) select f;

            //returns all tactics
            objTacticList = objTacticList.Union(lstMstaerTactic).ToList();
            var allTactic = objTacticList.Select(p => new
            {
                id = p.TacticTypeId,
                clientid = p.ClientId,
                modelId = p.ModelId,/*  TFS Bug - 179 : Improper behavior when editing Tactic in model   Changed By : Nirav shah on 6 Feb 2014        Change : add modelId = p.ModelId,    */
                title = p.Title,
                Stage = (p.StageId == null) ? "-" : p.Stage.Title, // Modified by dharmraj for ticket #475, Old line : Stage = (p.StageId == null) ? "-" : p.Stage.Code
                /*changed by Nirav Shah on 2 APR 2013*/
                // inquiries = (p.ProjectedInquiries == null) ? 0 : p.ProjectedInquiries,
                mqls = (p.ProjectedMQLs == null) ? 0 : p.ProjectedMQLs,
                revenue = (p.ProjectedRevenue == null) ? 0 : p.ProjectedRevenue,
                IsDeployedToIntegration = p.IsDeployedToIntegration
            }).Select(p => p).Distinct().OrderBy(p => p.title);

            return Json(allTactic, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to show Tactics Detail.
        /// </summary>
        public PartialViewResult DetailTacticData(int id = 0, int ModelId = 0)
        {
            Tactic_TypeModel tm = new Tactic_TypeModel();
            try
            {
                var objModel = db.Models.SingleOrDefault(varM => varM.ModelId == ModelId);
                ViewBag.IsModelIntegrated = objModel.IntegrationInstanceId == null ? false : true;

                /*changed by Nirav Shah on 2 APR 2013*/
                //ViewBag.Stages = db.Stages.Where(s => s.IsDeleted == false && s.ClientId == Sessions.User.ClientId);
                string Marketing = Convert.ToString(Enums.Funnel.Marketing).ToLower();
                string StageType = Enums.StageType.CR.ToString();
                //Changed by dharmraj for ticket #475
                //ViewBag.Stages = db.Model_Funnel_Stage.Where(s => s.Model_Funnel.ModelId == ModelId && s.AllowedTargetStage == true && s.StageType == StageType && s.Model_Funnel.Funnel.Title == Marketing).Select(n => new { n.StageId, n.Stage.Code }).Distinct().ToList();
                ViewBag.Stages = db.Model_Funnel_Stage.Where(s => s.Model_Funnel.ModelId == ModelId && 
                                                                  s.AllowedTargetStage == true && 
                                                                  s.StageType == StageType && 
                                                                  s.Model_Funnel.Funnel.Title == Marketing)
                                                      .Select(n => new { n.StageId, n.Stage.Title })
                                                      .Distinct()
                                                      .ToList();
                ViewBag.IsCreated = false;
                TacticType mtp = db.TacticTypes.Where(m => m.TacticTypeId.Equals(id)).FirstOrDefault();
                tm.TacticTypeId = mtp.TacticTypeId;
                tm.Title = mtp.Title;
                tm.ClientId = mtp.ClientId;
                tm.Description = mtp.Description;
                /*changed for TFS bug 176 : Model Creation - Tactic Defaults should Allow values of zero changed by Nirav Shah on 7 feb 2014*/
                /*changed by Nirav Shah on 2 APR 2013*/
                // tm.ProjectedInquiries = (mtp.ProjectedInquiries != null) ? mtp.ProjectedInquiries : 0;
                tm.ProjectedMQLs = (mtp.ProjectedMQLs != null) ? mtp.ProjectedMQLs : 0;
                tm.ProjectedRevenue = (mtp.ProjectedRevenue != null) ? mtp.ProjectedRevenue : 0;
                /*end changes*/

                // added by dharmraj for ticket #433 Integration - Model Screen Tactic List
                tm.IsDeployedToIntegration = mtp.IsDeployedToIntegration;


                tm.StageId = mtp.StageId;
                tm.ModelId = (mtp.ModelId == 0 || mtp.ModelId == null) ? ModelId : mtp.ModelId;
                if (mtp.ModelId != null && mtp.ClientId != null)
                {
                    ViewBag.IsDeployed = true;
                }
                else
                {
                    ViewBag.IsDeployed = false;
                }
                //Start Manoj Limbachiya 05May2014 PL#458
                ViewBag.CanDelete = false;
                if (mtp.ModelId != null || mtp.ClientId != null)
                {
                   ViewBag.CanDelete = true;
                }
                //End Manoj Limbachiya 05May2014 PL#458
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return PartialView("CreateTactic", tm);
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to show Create Tactic screen.
        /// </summary>
        public PartialViewResult CreateTacticData(int ModelId = 0)
        {
            var objModel = db.Models.SingleOrDefault(varM => varM.ModelId == ModelId);
            ViewBag.IsModelIntegrated = objModel.IntegrationInstanceId == null ? false : true;

            //ViewBag.Stages = db.Stages.Where(s => s.IsDeleted == false && s.ClientId == Sessions.User.ClientId);
            /*changed by Nirav Shah on 2 APR 2013*/
            string StageType = Enums.StageType.CR.ToString();
            string Marketing = Convert.ToString(Enums.Funnel.Marketing).ToLower();
            ViewBag.Stages = db.Model_Funnel_Stage.Where(s => s.Model_Funnel.ModelId == ModelId && s.AllowedTargetStage == true && s.StageType == StageType && s.Model_Funnel.Funnel.Title == Marketing).Select(n => new { n.StageId, n.Stage.Title }).Distinct().ToList();
            ViewBag.IsCreated = true;
            Tactic_TypeModel tm = new Tactic_TypeModel();
            /*changed for TFS bug 176 : Model Creation - Tactic Defaults should Allow values of zero changed by Nirav Shah on 7 feb 2014*/
            //tm.ProjectedInquiries = 0;
            tm.ProjectedMQLs = 0;
            tm.ProjectedRevenue = 0;   /*end changes*/
            //Start Manoj Limbachiya 05May2014 PL#458
            ViewBag.CanDelete = false;
            //End Manoj Limbachiya 05May2014 PL#458
            return PartialView("CreateTactic", tm);
        }
        /// <summary>
        /// Delete tactic type from client level and model level
        /// Author: Manoj Limbachiya
        /// Reference: PL#458
        /// Date: 05May2014
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteTactic(int id = 0)
        {
            TacticType ttModel = db.TacticTypes.Where(t => t.TacticTypeId == id).SingleOrDefault();
            if (ttModel != null)
            {
                int ModelId = ttModel.Model.ModelId;
                Model objModel = db.Models.Where(m => m.ModelId == ModelId).SingleOrDefault();
                if (objModel != null)
                {
                    if (objModel.Status.ToLower() == ModelPublished)
                    {
                        List<TacticType> lstTactictTypes = db.TacticTypes.Where(t => t.ModelId == ModelId && (t.IsDeleted == null || t.IsDeleted == false)).ToList();
                        if (lstTactictTypes.Count == 1)
                        {
                            return Json(new { status = "ERROR", Message = string.Format(Common.objCached.ModelTacticCannotDelete) });
                        }
                    }
                }
            }

            Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(p => p.TacticTypeId == id).FirstOrDefault();
            if (pcpt != null)
            {
                if (pcpt.IsDeleted || pcpt.Plan_Campaign_Program.IsDeleted || pcpt.Plan_Campaign_Program.Plan_Campaign.IsDeleted)
                {
                    db.Plan_Campaign_Program_Tactic.Attach(pcpt);
                    db.Entry(pcpt).State = EntityState.Deleted;
                    db.SaveChanges();
                    pcpt = null;
                }
            }
            if (pcpt == null)
            {
                TacticType tt = db.TacticTypes.Where(t => t.TacticTypeId == id).SingleOrDefault();
                if (tt != null)
                {
                    db.TacticTypes.Attach(tt);
                    db.Entry(tt).State = EntityState.Deleted;
                    db.SaveChanges();
                    Common.InsertChangeLog(Sessions.ModelId, 0, tt.TacticTypeId, tt.Title, Enums.ChangeLog_ComponentType.tactictype, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.removed);
                    db.Dispose();
                    TempData["SuccessMessage"] = string.Format(Common.objCached.ModelTacticDeleted, tt.Title);
                    return Json(new { status = "SUCCESS" });
                }
            }
            else
            {
                TacticType tt = db.TacticTypes.Where(t => t.TacticTypeId == id).SingleOrDefault();
                if (tt != null)
                {
                    tt.IsDeleted = true;
                    tt.ModifiedBy = Sessions.User.UserId;
                    tt.ModifiedDate = DateTime.Now;
                    db.TacticTypes.Attach(tt);
                    db.Entry(tt).State = EntityState.Modified;
                    db.SaveChanges();
                    Common.InsertChangeLog(Sessions.ModelId, 0, tt.TacticTypeId, tt.Title, Enums.ChangeLog_ComponentType.tactictype, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.removed);
                    db.Dispose();
                    TempData["SuccessMessage"] = string.Format(Common.objCached.ModelTacticDeleted, tt.Title);
                    return Json(new { status = "SUCCESS" });
                }
            }
            return Json(new { status = "ERROR", Message = string.Format(Common.objCached.ErrorOccured) });
        }
        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to Save Tactic data .
        /// </summary>
        [HttpPost]
        public ActionResult SaveTactic(string Title, string Description, int? StageId, int ProjectedMQLs, /*int ProjectedInquiries, */int ProjectedRevenue, int TacticTypeId, string modelID, bool isDeployedToIntegration)
        {
            try
            {
                TacticType objtactic = new TacticType();
                int ModelId;
                int.TryParse(modelID, out ModelId);
                objtactic.Title = Title;
                objtactic.Description = Description;
                objtactic.ProjectedMQLs = ProjectedMQLs;
                objtactic.ProjectedRevenue = ProjectedRevenue;
                /*changed by Nirav Shah on 2 APR 2013*/
                // objtactic.ProjectedInquiries = ProjectedInquiries;
                objtactic.StageId = StageId;
                int intRandomColorNumber = rnd.Next(colorcodeList.Count);
                objtactic.ColorCode = Convert.ToString(colorcodeList[intRandomColorNumber]);
                objtactic.CreatedDate = System.DateTime.Now;
                objtactic.ClientId = Sessions.User.ClientId;
                objtactic.CreatedBy = Sessions.User.UserId;
                objtactic.ModelId = ModelId;

                // Added by Dharmraj for ticket #433 Integration - Model Screen Tactic List
                objtactic.IsDeployedToIntegration = isDeployedToIntegration;

                /* Change TFS Bug - 166 : Improper behavior when editing Tactic in model 
                   Changed By : Nirav shah on 6 Feb 2014
                 */
                if (TacticTypeId == 0)
                {
                    var exist = db.TacticTypes.Where(t => t.ModelId == ModelId && t.ClientId == Sessions.User.ClientId && t.Title.ToLower() == Title.ToLower() && (t.IsDeleted == null || t.IsDeleted == false)).ToList();
                    if (exist.Count == 0)
                    {
                        db.TacticTypes.Attach(objtactic);
                        db.Entry(objtactic).State = EntityState.Added;
                        int result = db.SaveChanges();
                        Common.InsertChangeLog((int)ModelId, 0, objtactic.TacticTypeId, objtactic.Title, Enums.ChangeLog_ComponentType.tactictype, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.added);
                        db.Dispose();
                        TempData["SuccessMessage"] = string.Format(Common.objCached.ModelNewTacticSaveSucess, Title);
                    }
                    else
                    {
                        return Json(new { errormsg = Common.objCached.DuplicateTacticExits });
                    }
                }
                else
                {
                    var existTactic = db.TacticTypes.Where(t => (t.ModelId == ModelId || t.ModelId == null) && t.ClientId == Sessions.User.ClientId && t.Title.ToLower() == Title.ToLower() && t.TacticTypeId != TacticTypeId && (t.IsDeleted == null || t.IsDeleted == false)).ToList();
                    var addNewTactic = db.TacticTypes.Where(t => (t.ModelId == null || t.ModelId == 0) && (t.ClientId == Guid.Empty || t.ClientId == null) && t.TacticTypeId == TacticTypeId).ToList();
                    if (addNewTactic.Count == 1)
                    {
                        /*  TFS Bug - 179 : Improper behavior when editing Tactic in model 
                          Changed By : Nirav shah on 6 Feb 2014
                          Change : add new condition for duplicate
                      */
                        if (existTactic.Count == 0)
                        {
                            db.TacticTypes.Attach(objtactic);
                            db.Entry(objtactic).State = EntityState.Added;
                            int result = db.SaveChanges();
                            Common.InsertChangeLog((int)ModelId, 0, objtactic.TacticTypeId, objtactic.Title, Enums.ChangeLog_ComponentType.tactictype, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.added);
                            db.Dispose();
                            TempData["SuccessMessage"] = string.Format(Common.objCached.ModelNewTacticSaveSucess, Title);
                        }
                        else
                        {
                            return Json(new { errormsg = Common.objCached.DuplicateTacticExits });
                        }
                    }
                    else if (addNewTactic.Count == 0)
                    {
                        /*  TFS Bug - 179 : Improper behavior when editing Tactic in model 
                            Changed By : Nirav shah on 6 Feb 2014
                            Change : add new condition t.ModelId == null
                        */
                        if (existTactic.Count == 0)
                        {
                            var getPreviousTacticTypeId = db.TacticTypes.Where(t => t.TacticTypeId == TacticTypeId).Select(t => t.PreviousTacticTypeId).FirstOrDefault();
                            objtactic.PreviousTacticTypeId = getPreviousTacticTypeId;
                            objtactic.TacticTypeId = TacticTypeId;
                            MRPEntities dbedit = new MRPEntities();
                            dbedit.Entry(objtactic).State = EntityState.Modified;
                            int result = dbedit.SaveChanges();
                            Common.InsertChangeLog((int)ModelId, 0, objtactic.TacticTypeId, objtactic.Title, Enums.ChangeLog_ComponentType.tactictype, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.updated);
                            dbedit.Dispose();
                            TempData["SuccessMessage"] = string.Format(Common.objCached.ModelTacticEditSucess, Title);
                        }
                        else
                        {
                            return Json(new { errormsg = Common.objCached.DuplicateTacticExits });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { redirect = Url.Action("Tactics") });
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Modified By Maninder Singh Wadhva to address TFS Bug#239
        /// Action to Save all checked Tactic data .
        /// </summary>
        [HttpPost]
        public ActionResult saveAllTactic(string ids, string rejids, int ModelId, bool isModelPublished, string EffectiveDate)
        {
            string errorMessage = string.Empty, successMessage = string.Empty;
            try
            {
                TempData["ErrorMessage"] = string.Empty;
                TempData["SuccessMessage"] = string.Empty;
                string Title = string.Empty;
                TacticType objtactic = new TacticType();
                string[] id = ids.Split(',');
                int result;
                string[] rejid = rejids.Split(',');
                bool msgshow = false;
                if (rejid.Length > 0)
                {
                    for (int i = 0; i < rejid.Length; i++)
                    {
                        int tacticId;
                        string[] strArr = rejid[i].Replace("rej", "").Split('_');
                        int.TryParse(strArr[0], out tacticId);
                        if (tacticId != 0)
                        {
                            TacticType rejobj = db.TacticTypes.Where(t => t.TacticTypeId == tacticId).FirstOrDefault();
                            if (rejobj != null)
                            {
                                /*TFS point 252: editing a published model
                                  Added by Nirav Shah on 18 feb 2014
                               */
                                bool flag = true;

                                if (rejobj.ModelId != null)
                                {
                                    Model objModel = db.Models.Where(t => t.ModelId == rejobj.ModelId).FirstOrDefault();
                                    Title = objModel.Title;
                                    if (objModel.Status.ToLower() == Enums.ModelStatus.Draft.ToString().ToLower())
                                    {
                                        while (objModel.Model2 != null)
                                        {
                                            objModel = objModel.Model2;
                                            var objPlan = db.Plans.Where(plan => plan.ModelId == objModel.ModelId).ToList();
                                            if (objPlan.Count > 0)
                                            {
                                                var objTacticList = db.TacticTypes.Where(t => t.ModelId == objModel.ModelId).ToList();
                                                if (objTacticList.Count != 0)
                                                {
                                                    foreach (var objTactics in objTacticList)
                                                    {
                                                        Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(p => p.TacticTypeId == objTactics.TacticTypeId && p.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId == objModel.ModelId && p.IsDeleted == false).FirstOrDefault();
                                                        if (pcpt != null)
                                                        {
                                                            if (rejobj.Title.Trim().ToLower() == objTactics.Title.Trim().ToLower())
                                                            {
                                                                flag = false;
                                                                if (msgshow == false)
                                                                {
                                                                    msgshow = true;
                                                                    errorMessage = string.Format(Common.objCached.ModelPublishTacticDelete, pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Title);
                                                                    TempData["ErrorMessage"] = string.Format(Common.objCached.ModelPublishTacticDelete, pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Title);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (flag == true)
                                    {
                                        Plan_Campaign_Program_Tactic pt = db.Plan_Campaign_Program_Tactic.Where(p => p.TacticTypeId == tacticId).FirstOrDefault();
                                        if (pt == null)
                                        {
                                            /*  TFS Bug - 179 : Improper behavior when editing Tactic in model 
                                                Changed By : Nirav shah on 6 Feb 2014
                                                Change : add new check if samme tactic title with mdelid=null found then delete current tactic other wise update tactic set modelid=null
                                            */
                                            var existTactic = db.TacticTypes.Where(t => t.ModelId == null && t.ClientId == Sessions.User.ClientId && t.Title.ToLower() == rejobj.Title.ToLower() && t.TacticTypeId != tacticId).ToList();
                                            if (existTactic.Count > 0)
                                            {
                                                db.TacticTypes.Attach(rejobj);
                                                db.Entry(rejobj).State = EntityState.Deleted;
                                                result = db.SaveChanges();
                                            }
                                            else
                                            {
                                                rejobj.ModelId = null;
                                                rejobj.PreviousTacticTypeId = null;
                                                rejobj.IsDeployedToIntegration = false;
                                                db.TacticTypes.Attach(rejobj);
                                                db.Entry(rejobj).State = EntityState.Modified;
                                                result = db.SaveChanges();
                                            }/*end 6 feb change*/
                                            Common.InsertChangeLog(Sessions.ModelId, 0, rejobj.TacticTypeId, rejobj.Title, Enums.ChangeLog_ComponentType.tactictype, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.removed);
                                            /*
                                             * changed by : Nirav Shah on 31 Jan 2013
                                             * Bug 19:Model - should not be able to publish a model with no tactics selected */
                                            if (msgshow == false)
                                            {
                                                msgshow = true;
                                                successMessage = string.Format(Common.objCached.ModifiedTactic);
                                                TempData["SuccessMessage"] = string.Format(Common.objCached.ModifiedTactic);
                                            }
                                        }
                                        else
                                        {
                                            if (msgshow == false)
                                            {
                                                msgshow = true;
                                                errorMessage = string.Format(Common.objCached.DeleteTacticDependency);
                                                TempData["ErrorMessage"] = string.Format(Common.objCached.DeleteTacticDependency);
                                            }
                                        }
                                        /*End Nirav Changes */
                                    }
                                }
                            }
                        }
                    }
                }
                if (id.Length > 0)
                {
                    for (int i = 0; i < id.Length; i++)
                    {
                        int tacticId;
                        string[] strArr = id[i].Replace("rej", "").Split('_');
                        int.TryParse(strArr[0], out tacticId);
                        bool IsDeployToIntegration = Convert.ToBoolean(strArr[1]);
                        if (tacticId != 0)
                        {
                            int tid = Convert.ToInt32(Convert.ToString(strArr[0]));
                            var obj = db.TacticTypes.Where(t => t.TacticTypeId == tid).FirstOrDefault();
                            objtactic.Title = obj.Title;
                            objtactic.Description = obj.Description;
                            objtactic.ProjectedMQLs = obj.ProjectedMQLs;
                            objtactic.ProjectedRevenue = obj.ProjectedRevenue;
                            //objtactic.ProjectedInquiries = obj.ProjectedInquiries;
                            string StageType = Enums.StageType.CR.ToString();
                            Model_Funnel_Stage objStage = db.Model_Funnel_Stage.Where(s => s.StageType == StageType && s.Model_Funnel.ModelId == ModelId && s.AllowedTargetStage == true).OrderBy(s => s.Stage.Level).Distinct().FirstOrDefault();
                            if (objStage != null)
                            {
                                objtactic.StageId = objStage.StageId;
                            }
                            else
                            {
                                TempData["ErrorMessage"] = string.Format(Common.objCached.StageNotExist);
                                errorMessage = string.Format(Common.objCached.StageNotExist);
                                return Json(new { errorMessage }, JsonRequestBehavior.AllowGet);
                            }
                            //objtactic.StageId = (obj.StageId == null) ? db.Model_Funnel_Stage.Where(s => s.StageType == StageType && s.Model_Funnel.ModelId == ModelId && s.AllowedTargetStage == true).OrderBy(s => s.Stage.Level).Distinct().Select(s => s.StageId).FirstOrDefault() : obj.StageId;
                            int intRandomColorNumber = rnd.Next(colorcodeList.Count);
                            objtactic.ColorCode = Convert.ToString(colorcodeList[intRandomColorNumber]);
                            objtactic.CreatedDate = DateTime.Now;
                            objtactic.ClientId = Sessions.User.ClientId;
                            objtactic.CreatedBy = Sessions.User.UserId;
                            objtactic.ModelId = ModelId;
                            objtactic.PreviousTacticTypeId = obj.PreviousTacticTypeId;

                            // Added by dharmraj for ticket #433 Integration - Model Screen Tactic List
                            objtactic.IsDeployedToIntegration = IsDeployToIntegration;

                            if (obj.ModelId == null && obj.ClientId == null)
                            {
                                MRPEntities dbadd = new MRPEntities();
                                dbadd.Entry(objtactic).State = EntityState.Added;
                                result = dbadd.SaveChanges();
                                dbadd.Dispose();
                                Common.InsertChangeLog(Sessions.ModelId, 0, obj.TacticTypeId, obj.Title, Enums.ChangeLog_ComponentType.tactictype, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.added);
                            }
                            else
                            {
                                objtactic.TacticTypeId = obj.TacticTypeId;
                                MRPEntities dbedit = new MRPEntities();
                                dbedit.Entry(objtactic).State = EntityState.Modified;
                                result = dbedit.SaveChanges();
                                dbedit.Dispose();
                            }
                            /*
                                    * changed by : Nirav Shah on 31 Jan 2013
                                    * Bug 19:Model - should not be able to publish a model with no tactics selected */
                            if (msgshow == false)
                            {
                                msgshow = true;
                                successMessage = string.Format(Common.objCached.ModifiedTactic);
                                TempData["SuccessMessage"] = string.Format(Common.objCached.ModifiedTactic);
                            }
                        }
                    }
                }

                if (isModelPublished)
                {
                    bool isTacticTypeExist = false;

                    bool isPublished = PublishModel(ModelId, EffectiveDate, out isTacticTypeExist);
                    if (isPublished.Equals(true))
                    {
                        successMessage = string.Format(Common.objCached.ModelPublishSuccess);
                        TempData["SuccessMessage"] = string.Format(Common.objCached.ModelPublishSuccess);
                        TempData["ErrorMessage"] = string.Empty;
                        return Json(new { successMessage }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {

                        TempData["SuccessMessage"] = string.Empty;
                        if (isTacticTypeExist.Equals(false))
                        {
                            TempData["ErrorMessage"] = string.Format(Common.objCached.ModelTacticTypeNotexist, Title);
                            errorMessage = string.Format(Common.objCached.ModelTacticTypeNotexist, Title);
                            return Json(new { errorMessage }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                            errorMessage = Common.objCached.ErrorOccured;
                            return Json(new { errorMessage }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                TempData["SuccessMessage"] = string.Empty;
                return Json(new { errorMessage = e.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
            if (successMessage != string.Empty)
            {
                return Json(new { successMessage }, JsonRequestBehavior.AllowGet);
                //  return Json(new { redirect = Url.Action("ModelZero") });
            }
            else if (errorMessage != string.Empty)
            {
                return Json(new { errorMessage }, JsonRequestBehavior.AllowGet);
                //  return Json(new { redirect = Url.Action("ModelZero") });
            }
            else
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }

        }

        #region Other
        /// <summary>
        /// Added By: Nirav Shah.
        /// Color code list for get random color .
        /// </summary>
        List<string> colorcodeList = new List<string> { "27a4e5", "6ae11f", "bbb748", "bf6a4b", "ca3cce", "7c4bbf", "1af3c9", "f1eb13", "c7893b", "e42233", "a636d6", "2940e2", "0b3d58", "244c0a", "414018", "472519", "4b134d", "2c1947", "055e4d", "555305", "452f14", "520a10", "3e1152", "0c1556", "73c4ee", "9ceb6a", "d2cf86", "d59e89", "dc80df", "a989d5", "6bf7dc", "f6f263", "dab17d", "eb6e7a", "c57de4", "7483ec", "1472a3", "479714", "7f7c2f", "86472f", "8e2590", "542f86", "09af8f", "a6a10a", "875c26", "9e1320", "741f98", "1627a0" };
        #endregion
        #endregion

        #region GetAudienceDataByModelID
        public AudiencePlanModel GetAudienceDataByModelID(int ModelId)
        {

            //var businessunit = db.BusinessUnits.Where(b => b.BusinessUnitId == Sessions.User.BusinessUnitId && b.ClientId == Sessions.User.ClientId && b.IsDeleted == false).OrderBy(c => c.Title).Select(u => u.BusinessUnitId).ToList();
            //var ModelId = 0;

            //if (businessunit != null)
            //{
            //    ModelId = db.Models.Where(m => businessunit.Contains(m.BusinessUnitId) && m.Year == DateTime.Now.Year && m.IsActive == true && m.IsDeleted == false).OrderByDescending(c => c.ModelId).Select(d => d.ModelId).FirstOrDefault();
            //}

            var objfunnel = db.Funnels.Where(m => m.IsDeleted == false && m.Title == "Marketing").OrderBy(o => o.FunnelId).Select(m => m.FunnelId).ToList();
            var objmod = db.Models.Where(m => m.ModelId == ModelId).FirstOrDefault();

            long addressableContacts = 0;
            long marketingExpectedLeadCount = 0;

            if (objmod != null)
            {

                addressableContacts = objmod.AddressableContacts;
                if (objfunnel != null)
                {
                    var objModel_Funnel = db.Model_Funnel.Where(m => m.ModelId == objmod.ModelId && objfunnel.Contains(m.FunnelId)).FirstOrDefault();
                    if (objModel_Funnel != null)
                    {
                        marketingExpectedLeadCount = objModel_Funnel.ExpectedLeadCount;
                    }
                }

            }

            AudiencePlanModel objAudiencePlanModel = new AudiencePlanModel();
            if (ModelId != 0)
            {
                objAudiencePlanModel.Q1UsableDatabase = Convert.ToInt64(addressableContacts);
                objAudiencePlanModel.AllQOrigInboundInquiriesTotal = Convert.ToInt64(marketingExpectedLeadCount);
                var objModel_Audience_Outbound = db.Model_Audience_Outbound.Where(m => m.ModelId == ModelId).ToList();
                if (objModel_Audience_Outbound != null)
                {
                    if (objModel_Audience_Outbound.Count > 0)
                    {
                        foreach (var item in objModel_Audience_Outbound)
                        {

                            if (item.Quarter == Convert.ToString(Enums.Quarter.Q1))
                            {
                                objAudiencePlanModel.Q1NormalErosion = Convert.ToDouble(item.NormalErosion);
                                objAudiencePlanModel.NormalErosion = Convert.ToDouble(item.NormalErosion);
                                objAudiencePlanModel.NumberOfTouchesQ1 = Convert.ToInt32(item.NumberofTouches);
                                objAudiencePlanModel.ListAcquisitionsQ1 = Convert.ToInt32(item.ListAcquisitions);
                                objAudiencePlanModel.Acquisition_NumberofTouchesQ1 = Convert.ToInt32(item.Acquisition_NumberofTouches);
                                objAudiencePlanModel.Q1CTRDelivered = Convert.ToDouble(item.CTRDelivered);
                                objAudiencePlanModel.CTRDelivered = Convert.ToDouble(item.CTRDelivered);
                                objAudiencePlanModel.Q1RegistrationRate = Convert.ToDouble(item.RegistrationRate);
                                objAudiencePlanModel.RegistrationRate = Convert.ToDouble(item.RegistrationRate);
                                objAudiencePlanModel.Q1UnsubscribeRate = Convert.ToDouble(item.UnsubscribeRate);
                                objAudiencePlanModel.UnsubscribeRate = Convert.ToDouble(item.UnsubscribeRate);
                                objAudiencePlanModel.Acquisition_CostperContactQ1 = Convert.ToDouble(item.Acquisition_CostperContact);
                                objAudiencePlanModel.Acquisition_RegistrationRate = Convert.ToDouble(item.Acquisition_RegistrationRate);
                                objAudiencePlanModel.Q1OutBoundErosion = Convert.ToDouble(item.ListAcquisitionsNormalErosion);
                                objAudiencePlanModel.Q1OutBoundUnsubscribeRate = Convert.ToDouble(item.ListAcquisitionsUnsubscribeRate);
                                objAudiencePlanModel.Q1OutboundCTRDelivered = Convert.ToDouble(item.ListAcquisitionsCTRDelivered);
                            }
                            if (item.Quarter == Convert.ToString(Enums.Quarter.Q2))
                            {
                                objAudiencePlanModel.NumberOfTouchesQ2 = Convert.ToInt32(item.NumberofTouches);
                                objAudiencePlanModel.ListAcquisitionsQ2 = Convert.ToInt32(item.ListAcquisitions);
                                objAudiencePlanModel.Acquisition_NumberofTouchesQ2 = Convert.ToInt32(item.Acquisition_NumberofTouches);
                            }
                            if (item.Quarter == Convert.ToString(Enums.Quarter.Q3))
                            {
                                objAudiencePlanModel.NumberOfTouchesQ3 = Convert.ToInt32(item.NumberofTouches);
                                objAudiencePlanModel.ListAcquisitionsQ3 = Convert.ToInt32(item.ListAcquisitions);
                                objAudiencePlanModel.Acquisition_NumberofTouchesQ3 = Convert.ToInt32(item.Acquisition_NumberofTouches);
                            }
                            if (item.Quarter == Convert.ToString(Enums.Quarter.Q4))
                            {
                                objAudiencePlanModel.NumberOfTouchesQ4 = Convert.ToInt32(item.NumberofTouches);
                                objAudiencePlanModel.ListAcquisitionsQ4 = Convert.ToInt32(item.ListAcquisitions);
                                objAudiencePlanModel.Acquisition_NumberofTouchesQ4 = Convert.ToInt32(item.Acquisition_NumberofTouches);
                            }



                        }
                    }
                }

                var objModel_Audience_Inbound = db.Model_Audience_Inbound.Where(m => m.ModelId == ModelId).ToList();

                if (objModel_Audience_Inbound != null)
                {
                    if (objModel_Audience_Inbound.Count > 0)
                    {
                        foreach (var item in objModel_Audience_Inbound)
                        {
                            objAudiencePlanModel.Impressions = Convert.ToInt32(item.Impressions);
                            objAudiencePlanModel.ClickThroughRate = Convert.ToDouble(item.ClickThroughRate);
                            objAudiencePlanModel.Visits = Convert.ToInt32(item.Visits);
                            objAudiencePlanModel.InboundRegistrationRate = Convert.ToDouble(item.RegistrationRate);
                            objAudiencePlanModel.PPC_ClickThroughs = Convert.ToInt32(item.PPC_ClickThroughs);
                            objAudiencePlanModel.PPC_CostperClickThrough = Convert.ToDouble(item.PPC_CostperClickThrough);
                            objAudiencePlanModel.PPC_RegistrationRate = Convert.ToDouble(item.PPC_RegistrationRate);
                            objAudiencePlanModel.GC_GuaranteedCPLBudget = Convert.ToInt32(item.GC_GuaranteedCPLBudget);
                            objAudiencePlanModel.GC_CostperLead = Convert.ToInt32(item.GC_CostperLead);
                            objAudiencePlanModel.CSC_NonGuaranteedCPLBudget = Convert.ToInt32(item.CSC_NonGuaranteedCPLBudget);
                            objAudiencePlanModel.CSC_CostperLead = Convert.ToDouble(item.CSC_CostperLead);
                            objAudiencePlanModel.TDM_DigitalMediaBudget = Convert.ToInt32(item.TDM_DigitalMediaBudget);
                            objAudiencePlanModel.TDM_CostperLead = Convert.ToDouble(item.TDM_CostperLead);
                            objAudiencePlanModel.TP_PrintMediaBudget = Convert.ToInt32(item.TP_PrintMediaBudget);
                            objAudiencePlanModel.TP_CostperLead = Convert.ToDouble(item.TP_CostperLead);
                        }
                    }
                }

                var objModel_Audience_Event = db.Model_Audience_Event.Where(m => m.ModelId == ModelId).ToList();

                if (objModel_Audience_Event != null)
                {
                    if (objModel_Audience_Event.Count > 0)
                    {
                        foreach (var item in objModel_Audience_Event)
                        {
                            if (item.Quarter == Convert.ToString(Enums.Quarter.Q1))
                            {
                                objAudiencePlanModel.NumberofContactsQ1 = Convert.ToInt64(item.NumberofContacts);
                                objAudiencePlanModel.EventsBudgetQ1 = Convert.ToDouble(item.EventsBudget);
                                objAudiencePlanModel.ContactToInquiryConversion = Convert.ToDouble(item.ContactToInquiryConversion);
                            }
                            if (item.Quarter == Convert.ToString(Enums.Quarter.Q2))
                            {
                                objAudiencePlanModel.NumberofContactsQ2 = Convert.ToInt64(item.NumberofContacts);
                                objAudiencePlanModel.EventsBudgetQ2 = Convert.ToDouble(item.EventsBudget);
                            }
                            if (item.Quarter == Convert.ToString(Enums.Quarter.Q3))
                            {
                                objAudiencePlanModel.NumberofContactsQ3 = Convert.ToInt64(item.NumberofContacts);
                                objAudiencePlanModel.EventsBudgetQ3 = Convert.ToDouble(item.EventsBudget);
                            }
                            if (item.Quarter == Convert.ToString(Enums.Quarter.Q4))
                            {
                                objAudiencePlanModel.NumberofContactsQ4 = Convert.ToInt64(item.NumberofContacts);
                                objAudiencePlanModel.EventsBudgetQ4 = Convert.ToDouble(item.EventsBudget);
                            }

                        }
                    }
                }
            }

            return objAudiencePlanModel;
        }
        #endregion

        #region Integration

        /// <summary>
        /// Added By: Dharmraj Mangukiya
        /// Action to show model integration screen.
        /// </summary>
        public ActionResult Integration(int id = 0)
        {
            ViewBag.ModelId = id;

            var objModel = db.Models.SingleOrDefault(b => b.ModelId == id && b.IsDeleted == false);

            ViewBag.ModelStatus = objModel.Status;
            ViewBag.ModelTitle = objModel.Title;
            
            var businessunit = db.Models.Where(b => b.ModelId == id && b.IsDeleted == false).OrderByDescending(c => c.CreatedDate).Select(u => u.BusinessUnitId).FirstOrDefault();
            var IsBenchmarked = (id == 0) ? true : db.Models.Where(b => b.ModelId == id && b.IsDeleted == false).OrderByDescending(c => c.CreatedDate).Select(u => u.IsBenchmarked).FirstOrDefault();
            ViewBag.BusinessUnitId = Convert.ToString(businessunit);
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;
            ViewBag.IsBenchmarked = (IsBenchmarked != null) ? IsBenchmarked : true;
            BaselineModel objBaselineModel = FillInitialData(id, businessunit);
            string Title = objBaselineModel.Versions.Where(s => s.IsLatest == true).Select(s => s.Title).FirstOrDefault();
            if (Title != null && Title != string.Empty)
            {
                ViewBag.Msg = string.Format(Common.objCached.ModelTacticTypeNotexist, Title);
            }
            
            ViewBag.ModelPublishEdit = Common.objCached.ModelPublishEdit;
            ViewBag.ModelPublishCreateNew = Common.objCached.ModelPublishCreateNew;
            ViewBag.ModelPublishComfirmation = Common.objCached.ModelPublishComfirmation;
            ViewBag.Flag = false;
            if (id != 0)
            {
                ViewBag.Flag = chekckParentPublishModel(id);
            }
            return View(objBaselineModel);
        }

        /// <summary>
        /// Added By: Dharmraj mangukiya
        /// Action to show Integrations List by ModelId.
        /// </summary>
        public JsonResult GetIntegrationDatabyid(int id)
        {
            var objModel = db.Models.FirstOrDefault(varM => varM.ModelId == id);
            int integrationInstanceId = objModel.IntegrationInstanceId == null ? 0 : Convert.ToInt32(objModel.IntegrationInstanceId);

            var lstIntegrationInstance = db.IntegrationInstances.Where(varI => varI.IsDeleted == false && varI.IsActive == true && varI.ClientId == Sessions.User.ClientId).ToList().Select(varI => varI);

            //returns all Integrations
            var allIntegrationInstance = lstIntegrationInstance.Select(p => new
            {
                id = p.IntegrationInstanceId,
                clientid = p.ClientId,
                provider = p.IntegrationType.Title,
                instance = p.Instance,
                lastSync = GetFormatedDate(p.LastSyncDate),
                target = p.IntegrationInstanceId == integrationInstanceId ? true : false
            }).Select(p => p).Distinct().OrderBy(p => p.provider);

            return Json(allIntegrationInstance, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Function to return datetime in formatted pattern.
        /// </summary>
        /// <param name="objDate"></param>
        /// <returns></returns>
        public string GetFormatedDate(DateTime? objDate)
        {
            if (objDate == null)
                return string.Empty;
            else
                return Convert.ToDateTime(objDate).ToString("MMM dd") + " at " + Convert.ToDateTime(objDate).ToString("hh:mm tt");
        }

        /// <summary>
        /// Added By: Dharmraj mangukiya
        /// Action to save integration for model by integrationInstanceId and modelId.
        /// </summary>
        public JsonResult SaveAllIntegration(int modelId, int integrationId)
        {
            bool returnValue = false;
            string message = string.Empty;

            try
            {
                var businessunit = db.Models.Where(b => b.ModelId == modelId && b.IsDeleted == false).OrderByDescending(c => c.CreatedDate).Select(u => u.BusinessUnitId).FirstOrDefault();

                List<ModelVersion> lstVersions = GetVersions(modelId, businessunit);

                foreach (var version in lstVersions)
                {
                    var objModel = db.Models.SingleOrDefault(varM => varM.ModelId == version.ModelId);

                    if (objModel.IntegrationInstanceId == integrationId)
                    {
                        objModel.IntegrationInstanceId = null;
                    }
                    else
                    {
                        objModel.IntegrationInstanceId = integrationId;
                    }

                    db.Entry(objModel).State = EntityState.Modified;
                    db.SaveChanges();    
                }

                message = Common.objCached.ModelIntegrationSaveSuccess;

                returnValue = true;
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                message = Common.objCached.ErrorOccured;
            }

            var obj = new { returnValue = returnValue, message = message };

            return Json(obj, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region ChangeLog
        /// <summary>
        /// Load overview
        /// </summary>
        /// <param name="title"></param>
        /// <param name="BusinessUnitId"></param>
        /// <returns></returns>
        public ActionResult LoadChangeLog(int objectId)
        {
            List<ChangeLog_ViewModel> lst_changelog = new List<ChangeLog_ViewModel>();
            try
            {
                lst_changelog = Common.GetChangeLogs(Enums.ChangeLog_TableName.Model.ToString(), objectId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
            }
            return PartialView("_ChangeLog", lst_changelog);
        }

        #endregion

        #region Model Publish
        /// <summary>
        /// Added By: Dharmraj Mangukiya
        /// Action to publish Model.
        /// </summary>
        public JsonResult ModelPublish(int ModelId, string EffectiveDate)
        {
            bool isTacticTypeExist = false;
            string errorMessage = string.Empty, successMessage = string.Empty;
            Model objModel = db.Models.Where(t => t.ModelId == ModelId).FirstOrDefault();
            string Title = objModel.Title;

            bool isPublished = PublishModel(ModelId, EffectiveDate, out isTacticTypeExist);
            if (isPublished.Equals(true))
            {
                successMessage = string.Format(Common.objCached.ModelPublishSuccess);
                TempData["SuccessMessage"] = string.Format(Common.objCached.ModelPublishSuccess);
                TempData["ErrorMessage"] = string.Empty;
                return Json(new { successMessage }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                TempData["SuccessMessage"] = string.Empty;
                if (isTacticTypeExist.Equals(false))
                {
                    TempData["ErrorMessage"] = string.Format(Common.objCached.ModelTacticTypeNotexist, Title);
                    errorMessage = string.Format(Common.objCached.ModelTacticTypeNotexist, Title);
                    return Json(new { errorMessage }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                    errorMessage = Common.objCached.ErrorOccured;
                    return Json(new { errorMessage }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// Function to publish model.
        /// Added By Maninder Singh Wadhva to address TFS Bug#239
        /// </summary>
        /// <param name="modelId">Model Id.</param>
        /// <param name="isTacticTypeExist">Tactic type exist flag.</param>
        /// <returns>Flag to indicate whether model published sucessfully.</returns>
        public bool PublishModel(int modelId, string effectiveDate, out bool isTacticTypeExist)
        {
            isTacticTypeExist = false;
            int cntSelectedTactic = db.TacticTypes.Where(TacticTypes => TacticTypes.ModelId == modelId).ToList().Count;
            Model objModel = db.Models.Where(t => t.ModelId == modelId).FirstOrDefault();
            bool isPublish = false;
            try
            {
                if (cntSelectedTactic > 0)
                {
                    isTacticTypeExist = true;
                    if (objModel != null)
                    {
                        objModel.Status = Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value;
                        if (!string.IsNullOrEmpty(effectiveDate))
                        {
                            objModel.EffectiveDate = DateTime.Parse(effectiveDate);
                        }
                        else
                        {
                            objModel.EffectiveDate = System.DateTime.Now;
                        }
                        db.Models.Attach(objModel);
                        db.Entry(objModel).State = EntityState.Modified;
                        Common.InsertChangeLog(modelId, 0, modelId, objModel.Title, Enums.ChangeLog_ComponentType.model, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.published);
                        int result = db.SaveChanges();
                        if (result > 0)
                        {

                            isPublish = true;
                            /*TFS point 252: editing a published model
                              Added by Nirav Shah on 18 feb 2014
                             * Update ModelId in plan and set latest published modelid.
                           */
                            while (objModel.Model2 != null)
                            {
                                objModel = objModel.Model2;
                                if (objModel.Status.ToLower() != Enums.ModelStatus.Draft.ToString().ToLower())
                                {
                                    var objPlan = db.Plans.Where(plan => plan.ModelId == objModel.ModelId).ToList();
                                    if (objPlan.Count != 0)
                                    {
                                        foreach (var updPlan in objPlan)
                                        {
                                            UpdatePCPT(modelId, objModel.ModelId, updPlan.PlanId);
                                            updPlan.ModelId = modelId;
                                            db.Entry(updPlan).State = EntityState.Modified;
                                            result = db.SaveChanges();
                                        }
                                    }
                                }
                            }

                        }


                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return isPublish;
        }
        #endregion

        #region functions
        public void UpdatePCPT(int NewVersionmodelId, int oldVersionModelId, int PlanId)
        {
            var tacticTypeList = db.TacticTypes.Where(t => t.ModelId == NewVersionmodelId).ToList();
            var listPCPT = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted == false && pcpt.TacticTypeId != null && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId == oldVersionModelId && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId == PlanId).ToList();
            foreach (var pcpt in listPCPT)
            {
                foreach (var tactic in tacticTypeList)
                {
                    db.Plan_Campaign_Program_Tactic.Attach(pcpt);
                    db.Entry(pcpt).State = EntityState.Modified;
                    if (pcpt.TacticType.TacticTypeId == tactic.PreviousTacticTypeId)
                    {
                        pcpt.TacticTypeId = tactic.TacticTypeId;
                        int result = db.SaveChanges();
                    }
                }
            }
        }

        public bool chekckParentPublishModel(int id)
        {
            bool flag = true;
            Model objModel = db.Models.Where(t => t.ModelId == id && t.ParentModelId == null).FirstOrDefault();
            if (objModel != null)
            {
                flag = false;
            }
            return flag;
        }

        #endregion

    }

}

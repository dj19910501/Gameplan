using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;

namespace RevenuePlanner.Controllers
{
    public class BoostController : CommonController
    {
        #region Variable Delaration
        private MRPEntities db = new MRPEntities();

        static Random rnd = new Random();
        #endregion

        #region Methods

        #region Best In Class

        /// <summary>
        /// Best In Class
        /// </summary>
        public ActionResult BestInClass()
        {
            if (Sessions.IsPlanner)
            {
                return RedirectToAction("Index", "NoAccess");
            }

            List<BestInClassModel> listBestInClassModel = new List<BestInClassModel>();
            //added by uday for ticket #501 deleted old code and added new parameters like stagetype_cr etc.
            try
            {
                string StageType_CR = Enums.StageType.CR.ToString();
                string StageType_SV = Enums.StageType.SV.ToString();
                string StageType_Size = Enums.StageType.Size.ToString();
                string CW = Enums.Stage.CW.ToString();
                var bicfilter = db.BestInClasses.Where(b => b.Stage.ClientId == Sessions.User.ClientId).OrderBy(o => o.StageId).ToList();
                var stagefilter = db.Stages.Where(n => n.IsDeleted == false && n.ClientId == Sessions.User.ClientId).ToList();
                foreach (var item in stagefilter.Where(m => m.Level != null && m.Code != CW).OrderBy(o => o.Level).ToList())
                    {
                        BestInClassModel objBestInClassModel = new BestInClassModel();
                        objBestInClassModel.StageName = Common.GetReplacedString(item.ConversionTitle);
                        objBestInClassModel.ConversionValue = bicfilter.Where(b => b.StageId == item.StageId && b.StageType == StageType_CR).Select(v => v.Value).FirstOrDefault();
                        objBestInClassModel.VelocityValue = bicfilter.Where(b => b.StageId == item.StageId && b.StageType == StageType_SV).Select(v => v.Value).FirstOrDefault();
                        objBestInClassModel.StageID_CR = item.StageId;
                        objBestInClassModel.StageID_SV = item.StageId;
                        objBestInClassModel.StageType = StageType_CR;

                            listBestInClassModel.Add(objBestInClassModel);
                }
               
                foreach (var itemSize in db.Stages.Where(n => n.IsDeleted == false && n.ClientId == Sessions.User.ClientId && n.Level == null).ToList())
                {
                    BestInClassModel objBestInClassModel = new BestInClassModel();
                        objBestInClassModel.StageID_Size = itemSize.StageId;
                        objBestInClassModel.StageName = itemSize.Title;
                        objBestInClassModel.StageType = StageType_Size;
                        objBestInClassModel.ConversionValue = bicfilter.Where(b => b.StageId == itemSize.StageId && b.StageType == StageType_Size).Select(v => v.Value).FirstOrDefault();
                    listBestInClassModel.Add(objBestInClassModel);
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return View(listBestInClassModel.AsEnumerable());
        }

        /// <summary>
        /// Action to Save BIC.
        /// </summary>
        /// <param name="bic">List of BIC.</param>
        /// <returns>Returns JsonResult.</returns>
        [HttpPost]
        public JsonResult SaveBIC(List<BestInClassModel> bic)
        {
            //added by uday for PL ticket #501 added an extra line to save stage type in 
            try
            {
                if (bic != null)
                {
                    string StageType_CR = Enums.StageType.CR.ToString();
                    string StageType_SV = Enums.StageType.SV.ToString();
                    string StageType_Size = Enums.StageType.Size.ToString();

                    // Reset existing BIC values for specific Client
                    foreach (var item in db.BestInClasses.Where(n => n.Stage.ClientId == Sessions.User.ClientId).ToList())
                    {
                        db.Entry(item).State = EntityState.Modified;
                        db.BestInClasses.Remove(item);
                            db.SaveChanges();
                        }

                  

                    foreach (var t in bic)
                    {
                        BestInClass objBestInClass = new BestInClass();
                        if (t.StageType != null)
                        {
                            if (t.StageType == StageType_CR)
                            {
                                objBestInClass.StageId = t.StageID_CR;
                                objBestInClass.StageType = t.StageType;
                                objBestInClass.Value = t.ConversionValue;
                            }
                            else if (t.StageType == StageType_SV)
                            {
                                objBestInClass.StageId = t.StageID_SV;
                                objBestInClass.StageType = t.StageType;
                                objBestInClass.Value = t.VelocityValue;
                            }
                            else if (t.StageType == StageType_Size)
                            {
                                objBestInClass.StageId = t.StageID_Size;
                                objBestInClass.StageType = t.StageType;
                                objBestInClass.Value = t.ConversionValue;
                            }
                        }
                        objBestInClass.CreatedBy = Sessions.User.UserId;
                        objBestInClass.CreatedDate = DateTime.Now;
                        db.Entry(objBestInClass).State = EntityState.Added;
                        db.BestInClasses.Add(objBestInClass);
                        db.SaveChanges();
                    }
                    return Json(new { msg = "Best In Class values saved successfully." });
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
           
            return Json(new { id = 0 });
        }

        #endregion

        #region Improvement Tactic
        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to show Improvement Tactic list.
        /// </summary>
        public ActionResult Index()
        {
            if (Sessions.IsPlanner)
            {
                return RedirectToAction("Index", "NoAccess");
            }
            return View();
        }
        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to show Improvement Tactic list.
        /// </summary>
        public JsonResult ImprovementTacticList()
        {
            //Modified By : Mitesh Vaishnav on 03/06/2014 for Customized Target stage - Boost Improvement Tactic
            string StageTypeCW = Enums.Stage.CW.ToString();

            var objImprovementTactic = db.ImprovementTacticTypes.Where(s => s.IsDeleted == false && s.ClientId == Sessions.User.ClientId).Select(s => s).ToList();
            var ImprovementTacticList = objImprovementTactic.Select(itt => new
            {
                Id = itt.ImprovementTacticTypeId,
                Title = itt.Title,
                Cost = itt.Cost,
                IsDeployed = itt.IsDeployed,
                TargetStage = (db.Stages.Where(stages => stages.IsDeleted == false && stages.ClientId == Sessions.User.ClientId && stages.Code != StageTypeCW)//Add M
                                        .OrderBy(stages => stages.Level).ToList())
                                         .Select(ittmobj => new
                                         {
                                            Stages = ittmobj.Title,
                                            Active = TargetStage(itt.ImprovementTacticTypeId, ittmobj.StageId)
                                         }).Select(ittmobj => ittmobj),
                IsDeployedToIntegration = itt.IsDeployedToIntegration
            }).Select(itt => itt);
            return Json(ImprovementTacticList.OrderBy(t => t.Title).ToList(), JsonRequestBehavior.AllowGet); // Modified By :- Sohel Pathan on 28/04/2014 for Internal Review Points #9 to provide sorting for Listings
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to show Improvement Tactics Detail.
        /// </summary>
        public PartialViewResult DetailImprovementTacticData(int id = 0)
        {
            BoostImprovementTacticModel bittobj = new BoostImprovementTacticModel();

            string StageType_CR = Enums.StageType.CR.ToString();
            string StageType_SV = Enums.StageType.SV.ToString();
            string StageType_Size = Enums.MetricType.Size.ToString();
            string StageTypeCW = Enums.Stage.CW.ToString();
            double weight = 0;
            /* check the mode if id has value 0 then its create mode other wise edit mode */
            if (id != 0)
            {
                ImprovementTacticType ittobj = db.ImprovementTacticTypes.Where(itt => itt.IsDeleted == false && itt.ImprovementTacticTypeId == id).FirstOrDefault();
                bittobj.Title = ittobj.Title;
                bittobj.Description = ittobj.Description;
                bittobj.Cost = ittobj.Cost;
                bittobj.IsDeployed = ittobj.IsDeployed;
                bittobj.ImprovementTacticTypeId = id;

                bittobj.IsDeployedToIntegration = ittobj.IsDeployedToIntegration;

                ViewBag.Title = "Tactic Detail";
                ViewBag.CanDelete = true;        //// Added by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                ViewBag.IsCreated = false;       //// Added by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
            }
            else
            {
                bittobj.ImprovementTacticTypeId = 0;
                bittobj.Cost = 0;
                bittobj.IsDeployed = false;

                bittobj.IsDeployedToIntegration = false;

                ViewBag.Title = "New Tactic";
                ViewBag.CanDelete = false;     //// Added by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                ViewBag.IsCreated = true;      //// Added by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
            }
            
            /*get the metrics related to improvement Tactic and display in view*/
            List<MetricModel> listMetrics = new List<MetricModel>();
            List<MetricModel> listMetricssize = new List<MetricModel>();
            // Modified by Mitesh Vaishnav on 03/06/2014 for Customized Target stage - Boost Improvement Tactic
            var stageFilterCR = db.Stages.Where(n => n.IsDeleted == false && n.ClientId == Sessions.User.ClientId && n.ImprovementTacticType_Metric.Where(ittm => ittm.StageId == n.StageId && ittm.StageType == StageType_CR).FirstOrDefault().StageType == StageType_CR).ToList();
            var stageFilterSV = db.Stages.Where(n => n.IsDeleted == false && n.ClientId == Sessions.User.ClientId && n.ImprovementTacticType_Metric.Where(ittm => ittm.StageId == n.StageId && ittm.StageType == StageType_SV).FirstOrDefault().StageType == StageType_SV).ToList();
            foreach (var itemCR in stageFilterCR.Where(m => m.Level != null && m.Code != StageTypeCW).OrderBy(o => o.Level).ToList())
            {
                foreach (var itemSV in stageFilterSV.Where(m => m.Level != null && m.Code != StageTypeCW).OrderBy(o => o.Level).ToList())
                {
                    if (itemCR.Level == itemSV.Level)
                    {
                        MetricModel Metricsobj = new MetricModel();
                        Metricsobj.MetricType = StageType_CR;
                        Metricsobj.MetricName = itemCR.ConversionTitle;
                        weight = db.ImprovementTacticType_Metric.Where(itm => itm.StageId == itemCR.StageId && itm.ImprovementTacticTypeId == id && itm.StageType == StageType_CR).Select(v => v.Weight).FirstOrDefault();
                        Metricsobj.ConversionValue = weight;
                        weight = db.ImprovementTacticType_Metric.Where(itm => itm.StageId == itemSV.StageId && itm.ImprovementTacticTypeId == id && itm.StageType == StageType_SV).Select(v => v.Weight).FirstOrDefault();
                        Metricsobj.VelocityValue = weight;
                        Metricsobj.MetricID_CR = itemCR.StageId;
                        Metricsobj.MetricID_SV = itemSV.StageId;
                        listMetrics.Add(Metricsobj);
                    }
                }


            }
            foreach (var item in db.Stages.Where(n => n.IsDeleted == false && n.ClientId == Sessions.User.ClientId && n.Level == null))
            {
                MetricModel Metricsobj = new MetricModel();
                Metricsobj.MetricID_Size = item.StageId;
                Metricsobj.MetricType = item.ImprovementTacticType_Metric.Where(ittm => ittm.StageId == item.StageId).FirstOrDefault().StageType;
                Metricsobj.MetricName = item.Title;
                weight = db.ImprovementTacticType_Metric.Where(itm => itm.StageId == item.StageId && itm.ImprovementTacticTypeId == id).Select(v => v.Weight).FirstOrDefault();
                Metricsobj.ConversionValue = weight;
                listMetricssize.Add(Metricsobj);
            }

            bittobj.listMetrics = listMetrics;
            bittobj.listMetricssize = listMetricssize;
            return PartialView("CreateImprovementTactic", bittobj);
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to save Improvement Tactics Details in respective tables.
        /// </summary>
        public ActionResult saveImprovementTacticData(int improvementId, string improvementDetails, bool status, double cost, string desc, string title, bool deployToIntegrationStatus, string UserId = "")
        {
            string successMessage = string.Empty;
            string ErrorMessage = string.Empty;
            string[] value = improvementDetails.Split(',');
            try
            {
                /* if id !=0 then its update into db other wise add new record in db*/
                if (improvementId != 0)
                {
                    ImprovementTacticType objIt = db.ImprovementTacticTypes.Where(t => t.ImprovementTacticTypeId == improvementId && t.IsDeleted.Equals(false)).FirstOrDefault();       //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                    var existTactic = db.ImprovementTacticTypes.Where(t => t.ClientId == Sessions.User.ClientId && t.Title.ToLower() == title.ToLower() && t.ImprovementTacticTypeId != improvementId && t.IsDeleted.Equals(false)).ToList();       //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                    if (existTactic.Count == 0)
                    {
                        /*edit new improvementTactic record*/
                        objIt.ImprovementTacticTypeId = improvementId;
                        objIt.Title = title;
                        objIt.Description = desc;
                        objIt.Cost = cost;
                        objIt.IsDeployed = status;
                        objIt.IsDeployedToIntegration = deployToIntegrationStatus;
                        db.Entry(objIt).State = EntityState.Modified;
                        int result = db.SaveChanges();
                        while (db.ImprovementTacticType_Metric.Where(itm => itm.ImprovementTacticTypeId == improvementId).Count() != 0)
                        {
                            /*remove old data based on improvementTacticId*/
                            ImprovementTacticType_Metric Itm = db.ImprovementTacticType_Metric.Where(itm => itm.ImprovementTacticTypeId == improvementId).FirstOrDefault();
                            db.Entry(Itm).State = EntityState.Deleted;
                            result = db.SaveChanges();
                        }
                        successMessage = string.Format(Common.objCached.EditImprovementTacticSaveSucess);
                    }
                    else
                    {
                        ErrorMessage = string.Format(Common.objCached.DuplicateImprovementTacticExits);
                        return Json(new { errormsg = ErrorMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    /*Add new improvementTactic record*/
                    ImprovementTacticType objIt = new ImprovementTacticType();
                    var existTactic = db.ImprovementTacticTypes.Where(t => t.ClientId == Sessions.User.ClientId && t.Title.ToLower() == title.ToLower() && t.IsDeleted.Equals(false)).ToList(); //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                    if (existTactic.Count == 0)
                    {
                        objIt.Title = title;
                        objIt.Description = desc;
                        objIt.Cost = cost;
                        objIt.IsDeployed = status;
                        objIt.ClientId = Sessions.User.ClientId;
                        objIt.CreatedBy = Sessions.User.UserId;
                        objIt.IsDeleted = false;
                        objIt.CreatedDate = System.DateTime.Now;
                        int intRandomColorNumber = rnd.Next(colorcodeList.Count);
                        objIt.ColorCode = Convert.ToString(colorcodeList[intRandomColorNumber]);
                        objIt.IsDeployedToIntegration = deployToIntegrationStatus;

                        db.ImprovementTacticTypes.Attach(objIt);
                        db.Entry(objIt).State = EntityState.Added;
                        int result = db.SaveChanges();
                        
                        improvementId = objIt.ImprovementTacticTypeId;
                        successMessage = string.Format(Common.objCached.NewImprovementTacticSaveSucess);
                    }
                    else
                    {
                        ErrorMessage = string.Format(Common.objCached.DuplicateImprovementTacticExits);
                        return Json(new { errormsg = ErrorMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                //Modified by Mitesh Vaishnav on 03/06/2014 for Customized Target stage - Boost Improvement Tactic
                /*add into improvementType_metric table based on improvementId*/
                improvementDetails = improvementDetails.Replace(@"\", "");
                var stageValueList = JsonConvert.DeserializeObject<List<StageDetails>>(improvementDetails);
                foreach (var item in stageValueList)
                {
                    ImprovementTacticType_Metric objItm = new ImprovementTacticType_Metric();
                    int MetricId = 0;
                    int.TryParse(item.StageId, out MetricId);
                    objItm.StageId = MetricId;
                    double Weight = 0.0;
                    double.TryParse(item.Value, out Weight);
                    objItm.Weight = Weight;
                    objItm.ImprovementTacticTypeId = improvementId;
                    objItm.CreatedDate = System.DateTime.Now;
                    objItm.CreatedBy = Sessions.User.UserId;
                    objItm.StageType = item.StageType;
                    MRPEntities dbAdd = new MRPEntities();
                    dbAdd.ImprovementTacticType_Metric.Attach(objItm);
                    dbAdd.Entry(objItm).State = EntityState.Added;
                    int result = dbAdd.SaveChanges();
                    dbAdd.Dispose();

                }
                TempData["SuccessMessage"] = successMessage;
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                TempData["SuccessMessage"] = string.Empty;
                TempData["ErrorMessage"] = e.InnerException.Message;
                return Json(new { errormsg = e.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { redirect = Url.Action("Index") });
        }

        /// <summary>
        /// Added By: Dharmraj mangukiya
        /// Action to save deploy to integration flag for improvement tactic type
        /// </summary>
        public JsonResult SaveDeployedToIntegrationStatus(int id, bool isDeployedToIntegration, string UserId = "")
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            bool returnValue = false;
            string message = string.Empty;

            try
            {
                var objImprovementTacticType = db.ImprovementTacticTypes.SingleOrDefault(varI => varI.ImprovementTacticTypeId == id && varI.IsDeleted.Equals(false));   //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.

                objImprovementTacticType.IsDeployedToIntegration = isDeployedToIntegration;
                db.Entry(objImprovementTacticType).State = EntityState.Modified;
                db.SaveChanges();

                message = Common.objCached.DeployedToIntegrationStatusSaveSuccess;

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

        /// <summary>
        /// Added By: Dharmraj mangukiya
        /// Action to save deploy status for improvement tactic type
        /// </summary>
        public JsonResult SaveDeployeStatus(int id, bool isDeployed, string UserId = "")
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            bool returnValue = false;
            string message = string.Empty;

            try
            {
                var objImprovementTacticType = db.ImprovementTacticTypes.SingleOrDefault(varI => varI.ImprovementTacticTypeId == id && varI.IsDeleted.Equals(false));   //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.

                objImprovementTacticType.IsDeployed = isDeployed;
                db.Entry(objImprovementTacticType).State = EntityState.Modified;
                db.SaveChanges();

                message = Common.objCached.EditImprovementTacticSaveSucess;

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

        /// <summary>
        /// Added By: Sohel Pathan
        /// Action to delete Improvement Tactics from respective tables.
        /// </summary>
        /// <param name="improvementId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteImprovementTactic(int improvementId)
        {
            string successMessage = string.Empty;
            string ErrorMessage = string.Empty;
            try
            {
                if (improvementId != 0)
                {
                    using (var scope = new TransactionScope())
                    {
                        var isDependent = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.IsDeleted.Equals(false) && t.ImprovementTacticTypeId == improvementId).Count();

                        if (isDependent <= 0)
                        {
                            ImprovementTacticType objIt = db.ImprovementTacticTypes.Where(t => t.IsDeleted.Equals(false) && t.ImprovementTacticTypeId == improvementId).FirstOrDefault();

                            if (objIt != null)
                            {
                                objIt.IsDeleted = true;
                                objIt.ModifiedBy = Sessions.User.UserId;
                                objIt.ModifiedDate = DateTime.Now;
                                db.Entry(objIt).State = EntityState.Modified;
                                db.SaveChanges();
                                successMessage = string.Format(Common.objCached.DeleteImprovementTacticSaveSucess, objIt.Title.ToString());
                                TempData["SuccessMessage"] = successMessage;
                            }

                            scope.Complete();
                            return Json(new { status = 0, succMsg = successMessage }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            TempData["SuccessMessage"] = string.Empty;
                            return Json(new { status = 1, errormsg = Common.objCached.ImprovementTacticReferencesPlanError.ToString() }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    TempData["SuccessMessage"] = string.Empty;
                    return Json(new { status = 1,  errormsg = "No record found for this id." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                TempData["SuccessMessage"] = string.Empty;
                return Json(new { status = 1, errormsg = e.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #endregion

        #region Other
        public bool TargetStage(int id, int stageId)
        {
            bool active = true;
            var chkStage = db.ImprovementTacticType_Metric.Where(ittm => ittm.ImprovementTacticTypeId == id && ittm.StageId == stageId).ToList();
            if (chkStage.Count == 0)
            {
                active = false;
            }
            return active;
        }
        List<string> colorcodeList = new List<string> { "27a4e5", "6ae11f", "bbb748", "bf6a4b", "ca3cce", "7c4bbf", "1af3c9", "f1eb13", "c7893b", "e42233", "a636d6", "2940e2", "0b3d58", "244c0a", "414018", "472519", "4b134d", "2c1947", "055e4d", "555305", "452f14", "520a10", "3e1152", "0c1556", "73c4ee", "9ceb6a", "d2cf86", "d59e89", "dc80df", "a989d5", "6bf7dc", "f6f263", "dab17d", "eb6e7a", "c57de4", "7483ec", "1472a3", "479714", "7f7c2f", "86472f", "8e2590", "542f86", "09af8f", "a6a10a", "875c26", "9e1320", "741f98", "1627a0" };
        public class StageDetails
        {
            public string StageId { get; set; }
            public string StageType { get; set; }
            public string Value { get; set; }
        }
        #endregion

    }
}

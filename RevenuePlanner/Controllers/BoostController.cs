using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            if (!Sessions.IsSystemAdmin)
            {
                return RedirectToAction("Index", "NoAccess");
            }

            List<BestInClassModel> listBestInClassModel = new List<BestInClassModel>();
            try
            {
                string MetricType_CR = Enums.MetricType.CR.ToString();
                string MetricType_SV = Enums.MetricType.SV.ToString();
                string MetricType_Size = Enums.MetricType.Size.ToString();

                foreach (var itemCR in db.Metrics.Where(n => n.IsDeleted == false && n.MetricType == MetricType_CR).ToList())
                {
                    foreach (var itemSV in db.Metrics.Where(n => n.IsDeleted == false && n.MetricType == MetricType_SV).ToList())
                    {
                        BestInClassModel objBestInClassModel = new BestInClassModel();
                        if (itemCR.Level == itemSV.Level)
                        {
                            objBestInClassModel.MetricType = itemCR.MetricType;
                            objBestInClassModel.MetricName = itemCR.MetricName;
                            objBestInClassModel.ConversionValue = itemCR.BestInClasses.Where(bic => bic.MetricId == itemCR.MetricId).Select(v => v.Value).FirstOrDefault();
                            objBestInClassModel.VelocityValue = itemSV.BestInClasses.Where(bic => bic.MetricId == itemSV.MetricId).Select(v => v.Value).FirstOrDefault();
                            objBestInClassModel.MetricID_CR = itemCR.MetricId;
                            objBestInClassModel.MetricID_SV = itemSV.MetricId;

                            listBestInClassModel.Add(objBestInClassModel);
                            break;
                        };
                    }
                }
                foreach (var itemSize in db.Metrics.Where(n => n.IsDeleted == false && n.MetricType == MetricType_Size).ToList())
                {
                    BestInClassModel objBestInClassModel = new BestInClassModel();
                    objBestInClassModel.MetricID_Size = itemSize.MetricId;
                    objBestInClassModel.MetricType = itemSize.MetricType;
                    objBestInClassModel.MetricName = itemSize.MetricName;
                    objBestInClassModel.ConversionValue = itemSize.BestInClasses.Where(bic => bic.MetricId == itemSize.MetricId).Select(v => v.Value).FirstOrDefault();
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
            try
            {
                if (bic != null)
                {
                    string MetricType_CR = Enums.MetricType.CR.ToString();
                    string MetricType_SV = Enums.MetricType.SV.ToString();
                    string MetricType_Size = Enums.MetricType.Size.ToString();

                    // Reset existing BIC values
                    foreach (var item in db.BestInClasses.ToList())
                    {
                        db.Entry(item).State = EntityState.Modified;
                        db.BestInClasses.Remove(item);
                        db.SaveChanges();
                    }

                    var bicEntries = (from t in bic
                                      select new { t.MetricID_CR, t.MetricID_SV, t.MetricID_Size, t.MetricName, t.MetricType, t.ConversionValue, t.VelocityValue }).FirstOrDefault();

                    foreach (var t in bic)
                    {
                        BestInClass objBestInClass = new BestInClass();
                        if (t.MetricType != null)
                        {
                            if (t.MetricType == MetricType_CR)
                            {
                                objBestInClass.MetricId = t.MetricID_CR;
                                objBestInClass.Value = t.ConversionValue;
                            }
                            else if (t.MetricType == MetricType_SV)
                            {
                                objBestInClass.MetricId = t.MetricID_SV;
                                objBestInClass.Value = t.VelocityValue;
                            }
                            else if (t.MetricType == MetricType_Size)
                            {
                                objBestInClass.MetricId = t.MetricID_Size;
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
            if (!Sessions.IsSystemAdmin)
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
            var objImprovementTactic = db.ImprovementTacticTypes.Where(s => s.IsDeleted == false && s.ClientId == Sessions.User.ClientId).Select(s => s).ToList();
            var ImprovementTacticList = objImprovementTactic.Select(itt => new
            {
                Id = itt.ImprovementTacticTypeId,
                Title = itt.Title,
                Cost = itt.Cost,
                IsDeployed = itt.IsDeployed,
                TargetStage = (db.Metrics.ToList().Where(metrics => metrics.IsDeleted == false && metrics.ClientId == Sessions.User.ClientId).Select(metrics => metrics).Distinct().OrderBy(metrics => metrics.Level).ToList()).Select(ittmobj => new
                    {
                        Stages = ittmobj.MetricCode,
                        Active = TargetStage(itt.ImprovementTacticTypeId, ittmobj.MetricCode)
                    }).Select(ittmobj => ittmobj).Distinct()
            }).Select(itt => itt);
            return Json(ImprovementTacticList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to show Improvement Tactics Detail.
        /// </summary>
        public PartialViewResult DetailImprovementTacticData(int id = 0)
        {
            BoostImprovementTacticModel bittobj = new BoostImprovementTacticModel();
            string MetricType_CR = Enums.MetricType.CR.ToString();
            string MetricType_SV = Enums.MetricType.SV.ToString();
            string MetricType_Size = Enums.MetricType.Size.ToString();
            double weight = 0;
            /* check the mode if id has value 0 then its create mode other wise edit mode */
            if (id != 0)
            {
                ImprovementTacticType ittobj = db.ImprovementTacticTypes.Where(itt => itt.IsDeleted == false && itt.ImprovementTacticTypeId == id).FirstOrDefault();
                bittobj.Title = ittobj.Title;
                bittobj.Description = ittobj.Description;
                bittobj.Cost = ittobj.Cost ;
                bittobj.IsDeployed = ittobj.IsDeployed;
                bittobj.ImprovementTacticTypeId = id;
                ViewBag.Title = "Tactic Detail";

            }
            else
            {
                bittobj.ImprovementTacticTypeId = 0;
                bittobj.Cost = 0;
                bittobj.IsDeployed = false;
                ViewBag.Title = "New Tactic";
            }
            /*get the metrics related to improvement Tactic and display in view*/
            List<MetricModel> listMetrics = new List<MetricModel>();
            List<MetricModel> listMetricssize = new List<MetricModel>();
            foreach (var itemCR in db.Metrics.Where(n => n.IsDeleted == false && n.MetricType == MetricType_CR && n.ClientId == Sessions.User.ClientId).Distinct().OrderBy(o => o.Level).ToList())
            {
                foreach (var itemSV in db.Metrics.Where(n => n.IsDeleted == false && n.MetricType == MetricType_SV && n.ClientId == Sessions.User.ClientId).Distinct().OrderBy(o => o.Level).ToList())
                {
                    if (itemCR.Level == itemSV.Level)
                    {
                        MetricModel Metricsobj = new MetricModel();
                        Metricsobj.MetricType = itemCR.MetricType;
                        Metricsobj.MetricName = itemCR.MetricName;
                        weight = db.ImprovementTacticType_Metric.Where(itm => itm.MetricId == itemCR.MetricId && itm.ImprovementTacticTypeId == id).Select(v => v.Weight).FirstOrDefault();
                        Metricsobj.ConversionValue = weight;

                        weight = db.ImprovementTacticType_Metric.Where(itm => itm.MetricId == itemSV.MetricId && itm.ImprovementTacticTypeId == id).Select(v => v.Weight).FirstOrDefault();
                        Metricsobj.VelocityValue = weight;
                        Metricsobj.MetricID_CR = itemCR.MetricId;
                        Metricsobj.MetricID_SV = itemSV.MetricId;
                        listMetrics.Add(Metricsobj);
                        break;
                    };
                }
            }

            foreach (var itemSize in db.Metrics.Where(n => n.IsDeleted == false && n.MetricType == MetricType_Size && n.ClientId == Sessions.User.ClientId).Distinct().ToList())
            {
                MetricModel Metricsobj = new MetricModel();
                Metricsobj.MetricID_Size = itemSize.MetricId;
                Metricsobj.MetricType = itemSize.MetricType;
                Metricsobj.MetricName = itemSize.MetricName;
                weight = db.ImprovementTacticType_Metric.Where(itm => itm.MetricId == itemSize.MetricId && itm.ImprovementTacticTypeId == id).Select(v => v.Weight).FirstOrDefault();
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
        public ActionResult saveImprovementTacticData(int improvementId, string improvementDetails, bool status, double cost, string desc, string title)
        {
            string successMessage = string.Empty;
            string ErrorMessage = string.Empty;
            string[] value = improvementDetails.Split(',');
            try
            {
                /* if id !=0 then its update into db other wise add new record in db*/
                if (improvementId != 0)
                {
                    ImprovementTacticType objIt = db.ImprovementTacticTypes.Where(t => t.ImprovementTacticTypeId == improvementId).FirstOrDefault();
                    var existTactic = db.ImprovementTacticTypes.Where(t => t.ClientId == Sessions.User.ClientId && t.Title.ToLower() == title.ToLower() && t.ImprovementTacticTypeId != improvementId).ToList();
                    if (existTactic.Count == 0)
                    {
                        /*edit new improvementTactic record*/
                        objIt.ImprovementTacticTypeId = improvementId;
                        objIt.Title = title;
                        objIt.Description = desc;
                        objIt.Cost = cost;
                        objIt.IsDeployed = status;
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
                    var existTactic = db.ImprovementTacticTypes.Where(t => t.ClientId == Sessions.User.ClientId && t.Title.ToLower() == title.ToLower()).ToList();
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
                /*add into improvementType_metric table based on improvementId*/
                for (int i = 0; i < value.Length / 2; i++)
                {
                    ImprovementTacticType_Metric objItm = new ImprovementTacticType_Metric();
                    int MetricId = 0;
                    int.TryParse(value[i], out MetricId);
                    objItm.MetricId = MetricId;
                    double Weight = 0.0;
                    double.TryParse(value[i + 1], out Weight);
                    objItm.Weight = Weight;
                    objItm.ImprovementTacticTypeId = improvementId;
                    objItm.CreatedDate = System.DateTime.Now;
                    objItm.CreatedBy = Sessions.User.UserId;
                    db.ImprovementTacticType_Metric.Attach(objItm);
                    db.Entry(objItm).State = EntityState.Added;
                    int result = db.SaveChanges();
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
        #endregion

        #endregion

        #region Other
        public bool TargetStage(int id, string stagecode)
        {
            bool active = true;
            var chkStage = db.ImprovementTacticType_Metric.Where(ittm => ittm.ImprovementTacticTypeId == id && ittm.Metric.MetricCode == stagecode).ToList();
            if (chkStage.Count == 0)
            {
                active = false;
            }
            return active;
        }
        List<string> colorcodeList = new List<string> { "27a4e5", "6ae11f", "bbb748", "bf6a4b", "ca3cce", "7c4bbf", "1af3c9", "f1eb13", "c7893b", "e42233", "a636d6", "2940e2", "0b3d58", "244c0a", "414018", "472519", "4b134d", "2c1947", "055e4d", "555305", "452f14", "520a10", "3e1152", "0c1556", "73c4ee", "9ceb6a", "d2cf86", "d59e89", "dc80df", "a989d5", "6bf7dc", "f6f263", "dab17d", "eb6e7a", "c57de4", "7483ec", "1472a3", "479714", "7f7c2f", "86472f", "8e2590", "542f86", "09af8f", "a6a10a", "875c26", "9e1320", "741f98", "1627a0" };
        #endregion

    }
}

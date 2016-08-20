using Elmah;
using Newtonsoft.Json;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Transactions;
using System.Web.Caching;
using System.Web.Mvc;

namespace RevenuePlanner.Controllers
{
    public class BoostController : CommonController
    {
        #region Variable Delaration
        private MRPEntities db = new MRPEntities();
        static Random rnd = new Random();
        public RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();
        #endregion
        //public BoostController()
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
        #region Methods

        #region Best In Class

        /// <summary>
        /// Best In Class
        /// </summary>
        /// <returns>Return BestInClass View.</returns>
        [AuthorizeUser(Enums.ApplicationActivity.BoostBestInClassNumberEdit)]    // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult BestInClass()
        {
            // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsBoostBestInClassNumberEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostBestInClassNumberEdit);
            ViewBag.IsBoostImprovementTacticCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostImprovementTacticCreateEdit);

            List<BestInClassModel> listBestInClassModel = new List<BestInClassModel>();
            //added by uday for ticket #501 deleted old code and added new parameters like stagetype_cr etc.
            try
            {
                #region "Enum Variables"
                string StageType_CR = Enums.StageType.CR.ToString();
                string StageType_SV = Enums.StageType.SV.ToString();
                string StageType_Size = Enums.StageType.Size.ToString();
                string CW = Enums.Stage.CW.ToString();
                #endregion

                var bicfilter = db.BestInClasses.Where(cls => cls.Stage.ClientId == Sessions.User.ClientId).OrderBy(cls => cls.StageId).ToList();
                var stagefilter = GetStageListbyClientId(Sessions.User.ClientId);

                //// Add all  BestInClassModel details to list except CW Stage Code.
                foreach (var item in stagefilter.Where(stage => stage.Level != null && stage.Code != CW).OrderBy(stage => stage.Level).ToList())
                    {
                        BestInClassModel objBestInClassModel = new BestInClassModel();
                        objBestInClassModel.StageName = Common.GetReplacedString(item.ConversionTitle);
                        objBestInClassModel.ConversionValue = bicfilter.Where(cls => cls.StageId == item.StageId && cls.StageType == StageType_CR).Select(cls => cls.Value).FirstOrDefault();
                        objBestInClassModel.VelocityValue = bicfilter.Where(cls => cls.StageId == item.StageId && cls.StageType == StageType_SV).Select(cls => cls.Value).FirstOrDefault();
                        objBestInClassModel.StageID_CR = item.StageId;
                        objBestInClassModel.StageID_SV = item.StageId;
                        objBestInClassModel.StageType = StageType_CR;
                            listBestInClassModel.Add(objBestInClassModel);
                }
               
                //// Add Stage level null BestInClassModel details to list.
                List<Stage> lstStages = GetStageListbyClientId(Sessions.User.ClientId);
                foreach (var itemSize in lstStages.Where(stage => stage.Level == null))
                {
                    BestInClassModel objBestInClassModel = new BestInClassModel();
                        objBestInClassModel.StageID_Size = itemSize.StageId;
                        objBestInClassModel.StageName = itemSize.Title;
                        objBestInClassModel.StageType = StageType_Size;
                        objBestInClassModel.ConversionValue = bicfilter.Where(cls => cls.StageId == itemSize.StageId && cls.StageType == StageType_Size).Select(cls => cls.Value).FirstOrDefault();
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
                    foreach (var item in db.BestInClasses.Where(cls => cls.Stage.ClientId == Sessions.User.ClientId).ToList())
                    {
                        db.Entry(item).State = EntityState.Modified;
                        db.BestInClasses.Remove(item);
                            db.SaveChanges();
                        }
                    foreach (var bCls in bic)
                    {
                        BestInClass objBestInClass = new BestInClass();

                        //// Add StageId,StageType,Value to list based on StageType.
                        if (bCls.StageType != null)
                        {
                            if (bCls.StageType == StageType_CR)
                            {
                                objBestInClass.StageId = bCls.StageID_CR;
                                objBestInClass.StageType = bCls.StageType;
                                objBestInClass.Value = bCls.ConversionValue;
                            }
                            else if (bCls.StageType == StageType_SV)
                            {
                                objBestInClass.StageId = bCls.StageID_SV;
                                objBestInClass.StageType = bCls.StageType;
                                objBestInClass.Value = bCls.VelocityValue;
                            }
                            else if (bCls.StageType == StageType_Size)
                            {
                                objBestInClass.StageId = bCls.StageID_Size;
                                objBestInClass.StageType = bCls.StageType;
                                objBestInClass.Value = bCls.ConversionValue;
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
            // Start - Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
            var IsBoostImprovementTacticCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostImprovementTacticCreateEdit);
            var IsBoostBestInClassNumberEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostBestInClassNumberEdit);

            //// if BestInClass editing rights then redirect to "BestInClass" else "NoAccess" action of Index View.
            if (IsBoostImprovementTacticCreateEditAuthorized == false && IsBoostBestInClassNumberEditAuthorized == true)
            {
                return RedirectToAction("BestInClass");
            }
            else if (IsBoostImprovementTacticCreateEditAuthorized == false && IsBoostBestInClassNumberEditAuthorized == false)
            {
                return RedirectToAction("Index", "NoAccess");
            }
            
            ViewBag.IsBoostImprovementTacticCreateEditAuthorized = IsBoostImprovementTacticCreateEditAuthorized;
            ViewBag.IsBoostBestInClassNumberEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostBestInClassNumberEdit);
            // End - Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic

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

            var objImprovementTactic = db.ImprovementTacticTypes.Where(_imprvTacType => _imprvTacType.IsDeleted == false && _imprvTacType.ClientId == Sessions.User.ClientId).Select(_imprvTacType => _imprvTacType).ToList();
            var ImprovementTacticList = objImprovementTactic.Select(itt => new
            {
                Id = itt.ImprovementTacticTypeId,
                Title = itt.Title,
                Cost = objCurrency.GetValueByExchangeRate(itt.Cost), //Modified by Rahul Shah for PL #2501 to apply multi currency on boost screen
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
            return Json(ImprovementTacticList.OrderBy(_imprvTac => _imprvTac.Title,new AlphaNumericComparer()).ToList(), JsonRequestBehavior.AllowGet); // Modified By :- Sohel Pathan on 28/04/2014 for Internal Review Points #9 to provide sorting for Listings
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to show Improvement Tactics Detail.
        /// </summary>
        /// <param name="id">ImprovementTacticTypeId</param>
        /// <returns>Return _InspectPopupImprovementTacticType PartialView</returns>
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
                ImprovementTacticType ittobj = GetImprovementTacticTypeRecordbyId(id);
                bittobj.Title = System.Web.HttpUtility.HtmlDecode(ittobj.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
                bittobj.Description = System.Web.HttpUtility.HtmlDecode(ittobj.Description);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
                bittobj.Cost = objCurrency.GetValueByExchangeRate(ittobj.Cost); //Modified by Rahul Shah for PL #2501 to apply multi currency on boost screen
                bittobj.IsDeployed = ittobj.IsDeployed;
                bittobj.ImprovementTacticTypeId = id;
                bittobj.ColorCode = ittobj.ColorCode;
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
            List<Stage> lstStages = GetStageListbyClientId(Sessions.User.ClientId);
            var stageFilterCR = lstStages;//modified by Mitesh Vaishnav on 13/06/2014 to address #500 Customized Target stage - Boost Improvement Tactic 
            var stageFilterSV = lstStages;//modified by Mitesh Vaishnav on 13/06/2014 to address #500 Customized Target stage - Boost Improvement Tactic 
            
            //// Add MetricModel data to list except CW Stage Type data.
            MetricModel Metricsobj = null;
            foreach (var itemCR in stageFilterCR.Where(stage => stage.Level != null && stage.Code != StageTypeCW).OrderBy(stage => stage.Level).ToList())
            {
                foreach (var itemSV in stageFilterSV.Where(stage => stage.Level != null && stage.Code != StageTypeCW).OrderBy(stage => stage.Level).ToList())
                {
                    if (itemCR.Level == itemSV.Level)
                    {
                        Metricsobj = new MetricModel();
                        Metricsobj.MetricType = StageType_CR;
                        Metricsobj.MetricName = Common.GetReplacedString(itemCR.ConversionTitle);
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

            //// Add null Stage Type MetricModel data to list.
            foreach (var item in lstStages.Where(stage => stage.Level == null))
            {
                Metricsobj = new MetricModel();
                Metricsobj.MetricID_Size = item.StageId;
                //start: Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 73
                var stageTypeList = item.ImprovementTacticType_Metric.Where(ittm => ittm.StageId == item.StageId).FirstOrDefault();
                if (stageTypeList != null)
                {
                    Metricsobj.MetricType = stageTypeList.StageType;
                }
                else
                {
                    Metricsobj.MetricType = Enums.MetricType.Size.ToString();
                }
                //End : Modified by Mitesh Vaishnav on 21/07/2014 for functional review point 73
                Metricsobj.MetricName = item.Title;
                weight = db.ImprovementTacticType_Metric.Where(itm => itm.StageId == item.StageId && itm.ImprovementTacticTypeId == id).Select(v => v.Weight).FirstOrDefault();
                Metricsobj.ConversionValue = weight;
                listMetricssize.Add(Metricsobj);
            }

            bittobj.listMetrics = listMetrics;
            bittobj.listMetricssize = listMetricssize;
            return PartialView("_InspectPopupImprovementTacticType", bittobj);
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to save Improvement Tactics Details in respective tables.
        /// </summary>
        /// <param name="improvementId"></param>
        /// <param name="improvementDetails"></param>
        /// <param name="status"></param>
        /// <param name="cost"></param>
        /// <param name="desc"></param>
        /// <param name="title"></param>
        /// <param name="deployToIntegrationStatus"></param>
        /// <param name="UserId"></param>
        /// <returns>Return Save/Error messagee</returns>
        [AuthorizeUser(Enums.ApplicationActivity.BoostImprovementTacticCreateEdit)]    // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
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
                    ImprovementTacticType objIt = GetImprovementTacticTypeRecordbyId(improvementId);       //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                    var existTactic = db.ImprovementTacticTypes.Where(_imprvTac => _imprvTac.ClientId == Sessions.User.ClientId && _imprvTac.Title.ToLower() == title.ToLower() && _imprvTac.ImprovementTacticTypeId != improvementId && _imprvTac.IsDeleted.Equals(false)).ToList();       //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                    if (existTactic.Count == 0)
                    {
                        /*edit new improvementTactic record*/
                        objIt.ImprovementTacticTypeId = improvementId;
                        objIt.Title = title;
                        objIt.Description = desc;
                        objIt.Cost = objCurrency.SetValueByExchangeRate(cost); //Modified by Rahul Shah for PL #2501 to apply multi currency on boost screen
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
                    var existTactic = db.ImprovementTacticTypes.Where(_imprvTac => _imprvTac.ClientId == Sessions.User.ClientId && _imprvTac.Title.ToLower() == title.ToLower() && _imprvTac.IsDeleted.Equals(false)).ToList(); //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                    if (existTactic.Count == 0)
                    {
                        objIt.Title = title;
                        objIt.Description = desc;
                        objIt.Cost = objCurrency.SetValueByExchangeRate(cost); //Modified by Rahul Shah for PL #2501 to apply multi currency on boost screen
                        objIt.IsDeployed = status;
                        objIt.ClientId = Sessions.User.ClientId;
                        objIt.CreatedBy = Sessions.User.UserId;
                        objIt.IsDeleted = false;
                        objIt.CreatedDate = System.DateTime.Now;
                        int intRandomColorNumber = rnd.Next(Common.ColorcodeList.Count);
                        objIt.ColorCode = Convert.ToString(Common.ColorcodeList[intRandomColorNumber]);
                        objIt.IsDeployedToIntegration = deployToIntegrationStatus;
                      //  db.ImprovementTacticTypes.Attach(objIt);
                        db.Entry(objIt).State = EntityState.Added;
                        db.ImprovementTacticTypes.Add(objIt);
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
                ImprovementTacticType_Metric objItm = null;
                int MetricId = 0;
                double Weight = 0.0;
                MRPEntities dbAdd = null;
                foreach (var item in stageValueList)
                {
                    objItm = new ImprovementTacticType_Metric();
                    Weight = 0.0;
                    MetricId = 0;
                    int.TryParse(item.StageId, out MetricId);
                    objItm.StageId = MetricId;
                    double.TryParse(item.Value, out Weight);
                    objItm.Weight = Weight;
                    objItm.ImprovementTacticTypeId = improvementId;
                    objItm.CreatedDate = System.DateTime.Now;
                    objItm.CreatedBy = Sessions.User.UserId;
                    objItm.StageType = item.StageType;
                    dbAdd = new MRPEntities();
                    dbAdd.ImprovementTacticType_Metric.Attach(objItm);
                    dbAdd.Entry(objItm).State = EntityState.Added;
                    dbAdd.SaveChanges();
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
        /// <param name="id"></param>
        /// <param name="isDeployedToIntegration"></param>
        /// <param name="UserId"></param>
        public JsonResult SaveDeployedToIntegrationStatus(int id, bool isDeployedToIntegration, string UserId = "")
        {
            //// Check whether UserId is current loggined user or not.
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
                //// Update IsDeployedToIntegration field in Improvement Tactic Types table.
                var objImprovementTacticType = GetImprovementTacticTypeRecordbyId(id);   //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
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
        /// <param name="id"></param>
        /// <param name="isDeployed"></param>
        /// <param name="UserId"></param>
        /// <returns>Return Message in Json result.</returns>
        public JsonResult SaveDeployeStatus(int id, bool isDeployed, string UserId = "")
        {
            //// Check whether UserId is current loggined user or not.
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
                //// Update IsDeployed field in ImprovementTacticTypes table.
                var objImprovementTacticType = GetImprovementTacticTypeRecordbyId(id);   //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
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
        [AuthorizeUser(Enums.ApplicationActivity.BoostImprovementTacticCreateEdit)]    // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public JsonResult deleteImprovementTactic(int improvementId)
        {
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsBoostBestInClassNumberEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostBestInClassNumberEdit);
            ViewBag.IsBoostImprovementTacticCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostImprovementTacticCreateEdit);

            string successMessage = string.Empty;
            string ErrorMessage = string.Empty;
            try
            {
                if (improvementId != 0)
                {
                    using (var scope = new TransactionScope())
                    {
                        var isDependent = db.Plan_Improvement_Campaign_Program_Tactic.Where(_tac => _tac.IsDeleted.Equals(false) && _tac.ImprovementTacticTypeId == improvementId).Count();

                        ////  if Improvement Tactic does not dependent to other field then delete it.
                        if (isDependent <= 0)
                        {
                            ImprovementTacticType objIt = GetImprovementTacticTypeRecordbyId(improvementId);

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
                            return Json(new { status = 0, succMsg = successMessage, redirect = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { status = 1,  errormsg = Common.objCached.NoRecordFound }, JsonRequestBehavior.AllowGet);
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

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to show Improvement Tactic list.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="stageId"></param>
        public bool TargetStage(int id, int stageId)
        {
            bool active = true;
            //// Get No. of Count ImprovementTacticType_Metric record based on ImprovementTacticTypeId & StageId.
            var chkStage = db.ImprovementTacticType_Metric.Where(ittm => ittm.ImprovementTacticTypeId == id && ittm.StageId == stageId).ToList();
            if (chkStage.Count == 0)
                active = false;
            return active;
        }
        
        public class StageDetails
        {
            public string StageId { get; set; }
            public string StageType { get; set; }
            public string Value { get; set; }
        }

        /// <summary>
        /// Added By: Viral Kadiya.
        /// Get Stage list by ClientId.
        /// </summary>
        /// <param name="clientId"></param>
        public List<Stage> GetStageListbyClientId(Guid clientId)
        {
            List<Stage> lstStages = new List<Stage>();
            try
            {
                lstStages = db.Stages.Where(stage => stage.IsDeleted == false && stage.ClientId == clientId).ToList();
                if(lstStages == null)
                    lstStages = new List<Stage>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstStages;
        }

        /// <summary>
        /// Added By: Viral Kadiya.
        /// Get Stage list by ClientId.
        /// </summary>
        /// <param name="clientId"></param>
        public ImprovementTacticType GetImprovementTacticTypeRecordbyId(int improvementTacticTypeId)
        {
            ImprovementTacticType objImprvmentTacticType;
            try
            {
                objImprvmentTacticType = db.ImprovementTacticTypes.Where(_imprvTacticType => _imprvTacticType.IsDeleted == false && _imprvTacticType.ImprovementTacticTypeId == improvementTacticTypeId).FirstOrDefault();
                if (objImprvmentTacticType == null)
                    objImprvmentTacticType = new ImprovementTacticType();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objImprvmentTacticType;
        }
        #endregion

    }
}

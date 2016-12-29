using Elmah;
using Newtonsoft.Json;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Xml;
using System.Text.RegularExpressions;
using Integration;
using System.IO;
using System.Reflection;
using System.Web.Caching;

/*
 *  Author: 
 *  Created Date: 
 *  Purpose: Model related methods
  */
namespace RevenuePlanner.Controllers
{
    /// <summary>
    /// Model controller 
    /// </summary>
    public class ModelController : CommonController
    {
        #region Local Variables for ModelController
        private MRPEntities objDbMrpEntities = new MRPEntities();
        static Random rnd = new Random();
        string strVersion = "version";
        public RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency(); //Added by Rahul Shah for PL #2498 to apply multi currency
        public double PlanExchangeRate = Sessions.PlanExchangeRate;
        #endregion
        //public ModelController()
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
        #region Create/Edit/Version Model Input

        /// <summary>
        /// Create view for model for current year, business unit of logged-in user
        /// </summary>
        /// <param name="id">model id</param>
        /// <returns>returns strongly typed(BaselineModel object) Create view</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]    //// Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        //[HttpPost]
        public ActionResult CreateModel(int id = 0)
        {
            //// Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ModelCreateEdit);

            ViewBag.ModelId = id;
            var IsBenchmarked = (id == 0) ? true : objDbMrpEntities.Models.Where(model => model.ModelId == id && model.IsDeleted == false).OrderByDescending(model => model.CreatedDate).Select(model => model.IsBenchmarked).FirstOrDefault();
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;
            ViewBag.IsBenchmarked = (IsBenchmarked != null) ? IsBenchmarked : true;
            BaselineModel objBaselineModel = new BaselineModel();

            //// Fill Model data
            try
            {
                objBaselineModel = FillInitialData(id);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
            }

            string Title = string.Empty;
            if (objBaselineModel != null && objBaselineModel.Versions != null && objBaselineModel.Versions.Count > 0)
            {
                Title = objBaselineModel.Versions.Where(version => version.IsLatest == true).Select(version => version.Title).FirstOrDefault();
            }

            if (Title != null && Title != string.Empty)
            {
                ViewBag.Msg = string.Format(Common.objCached.ModelTacticTypeNotexist, Title);
                ViewBag.ModelPermission = string.Empty;
            }
            else if (id == 0)
            {
                ViewBag.ModelPermission = string.Empty;
            }
            else
            {
                ViewBag.ModelPermission = "You do not have permission to view this model";
            }

            //// TFS point 252: editing a published model, Added by Nirav Shah on 18 feb 2014
            ViewBag.ModelPublishEdit = Common.objCached.ModelPublishEdit;
            ViewBag.ModelPublishCreateNew = Common.objCached.ModelPublishCreateNew;
            ViewBag.ModelPublishComfirmation = Common.objCached.ModelPublishComfirmation;
            ViewBag.Flag = false;

            if (id != 0)
            {
                ViewBag.Flag = chekckParentPublishModel(id);
                //// Added by Sohel Pathan on 07/07/2014 for Internal Review Points to implement custom restriction logic on Business unit.
                ViewBag.IsOwner = objDbMrpEntities.Models.Where(model => model.IsDeleted.Equals(false) && model.ModelId == id && model.CreatedBy == Sessions.User.ID).Any();
            }
            else
            {
                //// For create mode
                ViewBag.IsOwner = true;
            }

            ViewBag.IsAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ModelCreateEdit);

            return View("Create", objBaselineModel);
        }

        /// <summary>
        /// Function to get list of Model Versions
        /// </summary>
        /// <param name="ModelId">model id</param>
        /// <returns>returns list of ModelVersions objects</returns>
        private List<ModelVersion> GetModelVersions(int ModelId)
        {
            List<ModelVersion> lstVersions = new List<ModelVersion>();
            if (ModelId != 0)
            {
                var versions = (from model in objDbMrpEntities.Models where model.IsDeleted == false && model.ClientId == Sessions.User.CID && model.ModelId == ModelId select model).FirstOrDefault();
                if (versions != null)
                {
                    //// Current Model
                    ModelVersion objModelVersion = new ModelVersion();
                    objModelVersion.ModelId = versions.ModelId;
                    objModelVersion.Title = versions.Title;
                    objModelVersion.Status = versions.Status;
                    objModelVersion.Version = versions.Version;
                    objModelVersion.IsLatest = false;
                    lstVersions.Add(objModelVersion);

                    //// Parent of ModelId
                    while (versions.Model2 != null)
                    {
                        versions = versions.Model2;
                        objModelVersion = new ModelVersion();
                        objModelVersion.ModelId = versions.ModelId;
                        objModelVersion.Title = versions.Title;
                        objModelVersion.Status = versions.Status;
                        objModelVersion.Version = versions.Version;
                        objModelVersion.IsLatest = false;
                        lstVersions.Add(objModelVersion);
                    }

                    //// Child of ModelId
                    Model objChildModel = GetChild(ModelId);
                    if (objChildModel != null)
                    {
                        objModelVersion = new ModelVersion();
                        objModelVersion.ModelId = objChildModel.ModelId;
                        objModelVersion.Title = objChildModel.Title;
                        objModelVersion.Status = objChildModel.Status;
                        objModelVersion.Version = objChildModel.Version;
                        objModelVersion.IsLatest = false;
                        lstVersions.Add(objModelVersion);

                        while (GetChild(objChildModel.ModelId) != null)
                        {
                            objChildModel = GetChild(objChildModel.ModelId);
                            objModelVersion = new ModelVersion();
                            objModelVersion.ModelId = objChildModel.ModelId;
                            objModelVersion.Title = objChildModel.Title;
                            objModelVersion.Status = objChildModel.Status;
                            objModelVersion.Version = objChildModel.Version;
                            objModelVersion.IsLatest = false;
                            lstVersions.Add(objModelVersion);
                        }
                    }
                }
            }
            if (lstVersions != null && lstVersions.Count > 0)
            {
                lstVersions = lstVersions.OrderByDescending(model => model.ModelId).ToList();
                lstVersions.First().IsLatest = true;
            }
            return lstVersions.Take(20).ToList();
        }

        /// <summary>
        /// Function to get child version on Model
        /// </summary>
        /// <param name="ModelId">Model id</param>
        /// <returns>returns child model as model object</returns>
        private Model GetChild(int ModelId)
        {
            return (from model in objDbMrpEntities.Models where model.IsDeleted == false && model.ParentModelId == ModelId select model).FirstOrDefault();
        }

        /// <summary>
        /// Function to fill data for latest model for current year, business unit of logged-in user
        /// </summary>
        /// <param name="ModelId">selected model id</param>
        /// <returns>retuns BaselineModel object</returns>
        public BaselineModel FillInitialData(int ModelId)
        {

            BaselineModel objBaselineModel = new BaselineModel();
            //// Retrieve all version of selected model
            objBaselineModel.Versions = GetModelVersions(ModelId);
            if (ModelId != 0 && (objBaselineModel.Versions == null || objBaselineModel.Versions.Count == 0))
            {
                objBaselineModel = null;
            }
            else
            {
                //// changes done by uday for #497
                List<ModelStage> listModelStage = new List<ModelStage>();
                string CW = Convert.ToString(Enums.Stage.CW);

                var StageList = objDbMrpEntities.Stages.Where(stage => stage.IsDeleted == false && stage.ClientId == Sessions.User.CID && stage.Level != null && stage.Code != CW).OrderBy(stage => stage.Level).ToList();
                if (StageList != null && StageList.Count > 0)
                {
                    foreach (var objStage in StageList)
                    {
                        ModelStage objModelStage = new ModelStage();
                        objModelStage.StageId = objStage.StageId;
                        objModelStage.ConversionTitle = Common.GetReplacedString(objStage.ConversionTitle);
                        objModelStage.VelocityTitle = Common.GetReplacedString(objStage.ConversionTitle);
                        objModelStage.Description = objStage.Description;
                        objModelStage.StageId = objStage.StageId;
                        objModelStage.Level = Convert.ToInt32(objStage.Level);
                        objModelStage.Funnel = objStage.Funnel;
                        objModelStage.Code = objStage.Code;
                        listModelStage.Add(objModelStage);
                    }
                }

                objBaselineModel.lstmodelstage = listModelStage;
                if (listModelStage.Count() == 0)
                {
                    TempData["StageNotExist"] = Common.objCached.StageNotDefined;
                }
            }
            return objBaselineModel;
        }

        #region Add/Update Model or model versioning
        /// <summary>
        /// Saves the current model and create a new model version
        /// Modified By Maninder Singh Wadhva to address TFS Bug#239
        /// </summary>
        /// <param name="collection">form data in post request</param>
        /// <param name="txtStageId">string collection of stage id(s)</param>
        /// <param name="txtTargetStage">string collectionof target stage(s)</param>
        /// <param name="txtMCR">string collection of marketting conversion rates</param>
        /// <param name="txtMSV">string collection of marketting stage velocity rates</param>
        /// <param name="txtMarketing">string collection of marketing</param>
        /// <param name="txtTeleprospecting">string collection of teleprospecting</param>
        /// <param name="txtSales">string collection of sales</param>
        /// <returns>returns strongly typed(BaselineModel object) Create view</returns>
        [HttpPost]
        //// changed by Nirav Shah on 2 APR 2013
        //// Modified By : Kalpesh Sharma #560 Method to Specify a Name for Cloned Model
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]    //// Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult Create(FormCollection collection, ICollection<string> txtStageId, ICollection<string> txtTargetStage, ICollection<string> txtMCR, ICollection<string> txtMSV)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            int intFunnelTeleprospecting = 0;
            int intFunnelSales = 0;
            string mode = Convert.ToString(Request.Form["whichButton"]);
            string effectiveDate = Convert.ToString(Request.Form["EffectiveDate"]);
            bool IsBenchmarked = Convert.ToBoolean(Request.Form["IsBenchmarked"]);
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;
            int currentModelId = 0;
            int intModelid = 0;
            string Title = string.Empty;
            bool? isBenchmarkDb = null;
            int.TryParse(Convert.ToString(Request.Form["CurrentModelId"]), out currentModelId);
            string redirectModelZero = string.Empty;
            double averageDealSize = 0;
            if (collection["txtMarketing"] != null)
            {
                double doubleValue = 0.0;
                //Modified by Rahul Shah for PL #2498.
                double.TryParse(Convert.ToString(collection["txtMarketing"]).Replace(",", "").Replace(Sessions.PlanCurrencySymbol, ""), out doubleValue);
                //averageDealSize = doubleValue;
                averageDealSize = objCurrency.SetValueByExchangeRate(double.Parse(Convert.ToString(doubleValue)), PlanExchangeRate);
            }
            try
            {
                using (MRPEntities objDbMrpEntities = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        Model objModel = new Model();
                        objModel.Title = Convert.ToString(collection["Title"]);
                        if (mode == strVersion)
                        {
                            OtherModelEntries(currentModelId, true, Convert.ToString(collection["Title"]), IsBenchmarked, ref intModelid);
                        }
                        else
                        {
                            intModelid = currentModelId;
                            if (intModelid == 0)
                            {
                                objModel.Version = "1.0";
                                objModel.Year = DateTime.Now.Year;
                                objModel.Status = Enums.ModelStatusValues.Single(modelStatus => modelStatus.Key.Equals(Enums.ModelStatus.Draft.ToString())).Value;
                                objModel.ClientId = Sessions.User.CID;
                                objModel.IsActive = true;
                                objModel.IsDeleted = false;
                                objModel.CreatedDate = DateTime.Now;
                                objModel.CreatedBy = Sessions.User.ID;
                                objModel.IsBenchmarked = IsBenchmarked;
                                objModel.AverageDealSize = averageDealSize;
                                objDbMrpEntities.Models.Add(objModel);
                                int resModel = objDbMrpEntities.SaveChanges();
                                intModelid = objModel.ModelId;

                                //// Start - Added by Sohel Pathan on 17/07/2014 for PL ticket #594 
                                //// Insert TacticType entry from ClientTacticType table of logged in client.
                                var lstClientTacticType = objDbMrpEntities.ClientTacticTypes.Where(clientTacticType => clientTacticType.ClientId == Sessions.User.CID && clientTacticType.IsDeleted == false).ToList();
                                if (lstClientTacticType != null)
                                {
                                    if (lstClientTacticType.Count > 0)
                                    {
                                        TacticType objTacticType;
                                        foreach (var clientTacticType in lstClientTacticType)
                                        {
                                            objTacticType = new TacticType();
                                            objTacticType.ColorCode = clientTacticType.ColorCode;
                                            objTacticType.CreatedBy = Sessions.User.ID;
                                            objTacticType.CreatedDate = DateTime.Now;
                                            objTacticType.Description = clientTacticType.Description;
                                            objTacticType.IsDeleted = false;
                                            objTacticType.IsDeployedToIntegration = false;
                                            objTacticType.IsDeployedToModel = false;
                                            objTacticType.ModelId = intModelid;
                                            objTacticType.Title = clientTacticType.Title;
                                            objTacticType.Abbreviation = clientTacticType.Abbreviation;
                                            objTacticType.AssetType = Enums.AssetType.Promotion.ToString();
                                            objDbMrpEntities.TacticTypes.Add(objTacticType);
                                        }

                                        objDbMrpEntities.SaveChanges();
                                    }
                                }
                                //// End - Added by Sohel Pathan on 17/07/2014 for PL ticket #594

                                //// Added By : Kalpesh Sharma PL #697 Default Line item type
                                LineItemType objLineItemType = objDbMrpEntities.LineItemTypes.Where(lineItemType => lineItemType.ModelId == intModelid).FirstOrDefault();
                                if (objLineItemType == null)
                                {
                                    objLineItemType = new LineItemType();
                                    objLineItemType.ModelId = intModelid;
                                    objLineItemType.Title = Enums.LineItemTypes.None.ToString();
                                    objLineItemType.Description = Enums.LineItemTypes.None.ToString();
                                    objLineItemType.IsDeleted = false;
                                    objLineItemType.CreatedDate = DateTime.Now;
                                    objLineItemType.CreatedBy = Sessions.User.ID;
                                    objDbMrpEntities.LineItemTypes.Add(objLineItemType);
                                    objDbMrpEntities.SaveChanges();
                                }
                            }
                            else
                            {
                                Model objExistingModel = objDbMrpEntities.Models.Where(model => model.ModelId == intModelid).FirstOrDefault();
                                isBenchmarkDb = objExistingModel.IsBenchmarked;
                                if (objModel != null)
                                {
                                    objExistingModel.ClientId = Sessions.User.CID;
                                    objExistingModel.ModifiedDate = DateTime.Now;
                                    objExistingModel.ModifiedBy = Sessions.User.ID;
                                    objExistingModel.IsBenchmarked = IsBenchmarked;
                                    objExistingModel.AverageDealSize = averageDealSize;
                                    objDbMrpEntities.Entry(objExistingModel).State = EntityState.Modified;
                                    objDbMrpEntities.SaveChanges();
                                }
                            }
                        }

                        if (IsBenchmarked == false)
                        {
                            //// changed by Nirav Shah on 2 APR 2013
                            string[] strtxtTargetStage = txtTargetStage.ToArray();
                            string[] strhdnSTAGEId = txtStageId.ToArray();
                            //// Marketing Conversion Rates
                            if (txtStageId != null && txtMCR != null)
                            {
                                string[] strtxtMCR = txtMCR.ToArray();
                                SaveModelFunnelStageInputs(strhdnSTAGEId, strtxtMCR, strtxtTargetStage, intModelid, Enums.StageType.CR.ToString());
                            }
                            if (txtStageId != null && txtMSV != null)
                            {
                                string[] strtxtMSV = txtMSV.ToArray();
                                SaveModelFunnelStageInputs(strhdnSTAGEId, strtxtMSV, strtxtTargetStage, intModelid, Enums.StageType.SV.ToString());
                            }
                        }
                        else
                        {
                            try
                            {
                                //// Read bench mark inputs from benchmark xml file and insert it into db.
                                XMLRead(intModelid, intFunnelTeleprospecting, intFunnelSales);
                            }
                            catch (Exception objException)
                            {
                                ErrorSignal.FromCurrentContext().Raise(objException);

                                //// Flag to indicate unavailability of web service.
                                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                                //// Ticket: 942 Exception handeling in Gameplan.
                                if (objException is System.ServiceModel.EndpointNotFoundException)
                                {
                                    return RedirectToAction("ServiceUnavailable", "Login");
                                }
                            }
                        }

                        if (isBenchmarkDb != null)
                        {
                            if (isBenchmarkDb != IsBenchmarked)
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
                        }

                        Sessions.ModelId = intModelid;
                        TempData["SuccessMessage"] = string.Format(Common.objCached.ModelSaveSuccess, objModel.Title);

                        if (mode == strVersion)
                        {
                            ObjectParameter returnValue = new ObjectParameter("ReturnValue", 0);
                        }
                        //// Bug 18:Model - results screen does not show for some models
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
                            string Status = objDbMrpEntities.Models.Where(model => model.ModelId == Sessions.ModelId).Select(model => model.Status).FirstOrDefault();
                            redirectModelZero = "Tactics";
                            scope.Complete();
                        }
                    }
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
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
            //// Bug 18:Model - results screen does not show for some models
            if (redirectModelZero == "ModelZero")
            {
                TempData["SuccessMessage"] = string.Format(Common.objCached.ModelPublishSuccess);
                TempData["ErrorMessage"] = string.Empty;
                return RedirectToAction("ModelZero");
            }
            else if (redirectModelZero == "Tactics")
            {
                TempData["modelIdForTactics"] = intModelid;
                return RedirectToAction("Tactics", "Model");
            }
            ViewBag.ModelId = currentModelId;
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
            BaselineModel objBaselineModel = new BaselineModel();

            //// Start - Added by Sohel Pathan on 01/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            try
            {
                objBaselineModel = FillInitialData(currentModelId);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
            }
            //// End - Added by Sohel Pathan on 30/06/2014 for PL ticket #563 to apply custom restriction logic on Business Units

            return View(objBaselineModel);
        }
        #endregion

        /// <summary>
        /// Function to save imputs for different Funnel Stage Type (CR/ SV).
        /// </summary>
        /// <param name="itemIds">string array of Funnel item Ids.</param>
        /// <param name="itemLabels">string array of Funnel Item Labels.</param>
        /// <param name="strtxtTargetStage">string array of target stage(s)</param>
        /// <param name="funnelId">Funnel Id.</param>
        /// <param name="stageType">Stage Type: CR/ SV.</param>
        /// <returns>returns flag as per db operation status</returns>
        private bool SaveModelFunnelStageInputs(string[] itemIds, string[] itemLabels, string[] strtxtTargetStage, int modelId, string stageType)
        {
            if (itemIds.Length > 0 && itemLabels.Length > 0 && modelId > 0 && !string.IsNullOrWhiteSpace(stageType))
            {
                for (int item = 0; item < itemIds.Length; item++)
                {
                    double doubleValue = 0.0;
                    double.TryParse(Convert.ToString(itemLabels[item]).Replace(",", "").Replace("$", ""), out doubleValue);

                    bool boolValue = false;
                    bool.TryParse(strtxtTargetStage[item], out boolValue);

                    Model_Stage objModel_Funnel_Stage = new Model_Stage();
                    objModel_Funnel_Stage.ModelId = modelId;
                    objModel_Funnel_Stage.StageId = Convert.ToInt32(itemIds[item]);
                    objModel_Funnel_Stage.StageType = stageType;
                    objModel_Funnel_Stage.Value = doubleValue;
                    objModel_Funnel_Stage.AllowedTargetStage = boolValue;
                    Model_Stage existingModelFunnelStage = objDbMrpEntities.Model_Stage.Where(modelFunnelStage => modelFunnelStage.ModelId == objModel_Funnel_Stage.ModelId && modelFunnelStage.StageId == objModel_Funnel_Stage.StageId && modelFunnelStage.StageType == objModel_Funnel_Stage.StageType).FirstOrDefault();
                    Model_Stage checkEditModelFunnelStage = objDbMrpEntities.Model_Stage.Where(modelFunnelStage => modelFunnelStage.ModelId == objModel_Funnel_Stage.ModelId && modelFunnelStage.StageId == objModel_Funnel_Stage.StageId && modelFunnelStage.StageType == objModel_Funnel_Stage.StageType && modelFunnelStage.Value == objModel_Funnel_Stage.Value && modelFunnelStage.AllowedTargetStage == objModel_Funnel_Stage.AllowedTargetStage).FirstOrDefault();
                    if (checkEditModelFunnelStage == null && ViewBag.EditFlag == false)
                    {
                        ViewBag.EditFlag = true;
                    }
                    if (existingModelFunnelStage == null)
                    {
                        objModel_Funnel_Stage.CreatedDate = DateTime.Now;
                        objModel_Funnel_Stage.CreatedBy = Sessions.User.ID;
                        objDbMrpEntities.Model_Stage.Add(objModel_Funnel_Stage);
                        objDbMrpEntities.SaveChanges();
                    }
                    else
                    {
                        //// PL #497 	Customized Target stage - Model Inputs By Udaya on 06/12/2014
                        existingModelFunnelStage.Value = objModel_Funnel_Stage.Value;
                        existingModelFunnelStage.ModifiedBy = Sessions.User.ID;
                        existingModelFunnelStage.ModifiedDate = DateTime.Now;
                        existingModelFunnelStage.AllowedTargetStage = boolValue;
                        objDbMrpEntities.Entry(existingModelFunnelStage).State = EntityState.Modified;
                        objDbMrpEntities.SaveChanges();
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Json method to check target stage of a model
        /// </summary>
        /// <param name="ModelId">model id</param>
        /// <param name="StageId">stage id</param>
        /// <returns>returns json object</returns>
        public JsonResult CheckTargetStage(int ModelId, int StageId)
        {
            //Modified By Komal Rawal for #1244
            var objTacticType = objDbMrpEntities.TacticTypes.Where(tacticType => tacticType.ModelId == ModelId && tacticType.StageId == StageId && tacticType.IsDeleted == false).FirstOrDefault();
            if (objTacticType == null)
            {
                return Json("notexist", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("exist", JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Created By : Sohel Pathan
        /// Created Date : 08/09/2014
        /// Description : Json metho to check weather for logged in user client stage(s) and corresponding target stage(s) are exists.
        /// </summary>
        /// <returns>Error Message for Target Stage Benchmark in Json Format</returns>
        public JsonResult CheckTargetStageBenchMark()
        {
            //// Check Benchmark file exists at specified path or not
            if (System.IO.File.Exists(Common.xmlBenchmarkFilePath))
            {
                //// created xml doc object
                XmlDocument xmlDoc = new XmlDocument();

                //// load Benchmark file into xml doc object
                xmlDoc.Load(Common.xmlBenchmarkFilePath);

                //// load object of BDSServiceClient
                BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
                string clientcode = string.Empty;

                try
                {
                    //// get Client code from logged in client id.
                    clientcode = objBDSUserRepository.GetClientByIdEx(Sessions.User.CID).Code;
                }
                catch (Exception objException)
                {
                    ErrorSignal.FromCurrentContext().Raise(objException);

                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    if (objException is System.ServiceModel.EndpointNotFoundException)
                    {
                        return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                    }
                }

                bool stageExistsForClient = false;
                bool anyTargetStageExists = false;

                List<Stage> stageList = objDbMrpEntities.Stages.Where(stage => stage.IsDeleted == false && stage.ClientId == Sessions.User.CID).ToList();

                //// parse xml doc file and check stage and its corresponding target stage is exist. Based on parsing set the flags for stage and stage target.
                foreach (XmlNode _class in xmlDoc.SelectNodes(@"/Model/ModelInput"))
                {
                    foreach (XmlElement element1 in _class.SelectNodes(@"ClientCode"))
                    {
                        if (element1.HasAttribute("value"))
                        {
                            if (element1.Attributes["value"].Value.Equals(clientcode) == true)
                            {
                                if (element1.SelectNodes(@"stage").Count == stageList.Count)
                                {
                                    foreach (XmlElement element in element1.SelectNodes(@"stage"))
                                    {
                                        if (stageList.Exists(x => x.Code == element.Attributes["code"].Value))
                                        {
                                            if (element.HasAttribute("targetstage"))
                                            {
                                                if (element.Attributes["targetstage"].Value.Equals("true"))
                                                {
                                                    anyTargetStageExists = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //// when database stages and xml stages for client code doesn't matches.
                                            return Json(new { errorMessage = Common.objCached.StagesConfigurationMissMatch }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                                else
                                {
                                    //// when database stages and xml stages for client doesn't matches.
                                    return Json(new { errorMessage = Common.objCached.StagesConfigurationMissMatch }, JsonRequestBehavior.AllowGet);
                                }

                                stageExistsForClient = true;
                            }
                        }
                    }
                }

                if (!stageExistsForClient)
                {
                    //// check stage exist for client or not and return error message for the same
                    return Json(new { errorMessage = Common.objCached.StageNotDefined }, JsonRequestBehavior.AllowGet);
                }
                else if (!anyTargetStageExists)
                {
                    //// check target stage exist for client or not and return error message for the same
                    return Json(new { errorMessage = Common.objCached.StageNotExist }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //// if Benchmark xml file doesn't exist at specified path then return error message for the same
                return Json(new { errorMessage = Common.objCached.StageNotDefined }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { errorMessage = string.Empty }, JsonRequestBehavior.AllowGet);
        }

        #region Benchmark Inputs Functions

        /// <summary>
        /// Read Benchmark XML for Benchmark Inputs.
        /// </summary>
        /// <param name="intFunnelMarketing">Funnel marketing value</param>
        /// <param name="intFunnelTeleprospecting">Funnel teleprospecting value</param>
        /// <param name="intFunnelSales">Funnel sales value</param>
        /// <returns></returns>
        public void XMLRead(int intModelid, int intFunnelTeleprospecting, int intFunnelSales)
        {
            if (System.IO.File.Exists(Common.xmlBenchmarkFilePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Common.xmlBenchmarkFilePath);


                //// Retrieve client code
                BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
                string clientcode = objBDSUserRepository.GetClientByIdEx(Sessions.User.CID).Code;
                bool isClientDataExists = false;

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
                                    readdata(element, intModelid);
                                }

                                isClientDataExists = true;
                            }
                        }
                    }

                    if (isClientDataExists == false)
                    {
                        foreach (XmlElement element in _class.SelectNodes(@"stage"))
                        {
                            readdata(element, intModelid);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Function to read specific nodes from Benchmark Inputs XML.
        /// </summary>
        /// <param name="element">xml node element</param>
        /// <param name="intFunnelMarketing">funnel marketing value</param>
        public void readdata(XmlElement element, int intModelid)
        {
            string stageCode = string.Empty;
            double cr = 0;
            double sv = 0;
            bool targetStage = false;
            int StageId = 0;
            if (element.HasAttribute("code"))
            {
                stageCode = element.Attributes["code"].Value;
                StageId = GetStageIdByStageCode(stageCode);
            }
            if (element.HasAttribute(Enums.StageType.CR.ToString().ToLower()))
            {
                double.TryParse(element.Attributes[Enums.StageType.CR.ToString().ToLower()].Value, out cr);
            }
            if (element.HasAttribute(Enums.StageType.SV.ToString().ToLower()))
            {
                double.TryParse(element.Attributes[Enums.StageType.SV.ToString().ToLower()].Value, out sv);
            }
            if (element.HasAttribute("targetstage"))
            {
                bool.TryParse(element.Attributes["targetstage"].Value, out targetStage);
            }

            SaveModelfunnelStageBenchmarkData(intModelid, StageId, Enums.StageType.CR.ToString(), cr, targetStage);
            SaveModelfunnelStageBenchmarkData(intModelid, StageId, Enums.StageType.SV.ToString().ToLower(), sv, false);
        }

        /// <summary>
        /// Function to get StageId for given StageName.
        /// </summary>
        /// <param name="stagename">stage name</param>
        /// <returns>Returns StageId for given StageName</returns>
        public int GetStageIdByStageCode(string code)
        {
            int StageId = objDbMrpEntities.Stages.Where(stage => stage.Code.ToLower() == code.ToLower() && stage.IsDeleted == false && stage.ClientId == Sessions.User.CID).Select(stage => stage.StageId).FirstOrDefault();
            return StageId;
        }

        /// <summary>
        /// Function to save values to Model_Funnel_Stage table for Benchmark Inputs XML.
        /// </summary>
        /// <param name="ModelFunnelId">model funnel id</param>
        /// <param name="StageId">stage id</param>
        /// <param name="StageType">stage type</param>
        /// <param name="Value">stage value</param>
        /// <param name="stageValue">flag to indicate stage value</param>
        /// <returns>returns flag as per DB operation status</returns>
        public int SaveModelfunnelStageBenchmarkData(int intModelid, int StageId, string StageType, double Value, bool stageValue)
        {
            int result = 0;

            Model_Stage objModel_Funnel_Stage = new Model_Stage();
            objModel_Funnel_Stage.ModelId = intModelid;
            objModel_Funnel_Stage.StageId = StageId;
            objModel_Funnel_Stage.StageType = StageType;
            objModel_Funnel_Stage.Value = Value;

            Model_Stage tsvModel_Funnel_Stage = objDbMrpEntities.Model_Stage.Where(modelFunnelStage => modelFunnelStage.ModelId == objModel_Funnel_Stage.ModelId && modelFunnelStage.StageId == objModel_Funnel_Stage.StageId && modelFunnelStage.StageType == objModel_Funnel_Stage.StageType).FirstOrDefault();
            if (tsvModel_Funnel_Stage == null)
            {
                objModel_Funnel_Stage.AllowedTargetStage = stageValue;
                objModel_Funnel_Stage.CreatedDate = DateTime.Now;
                objModel_Funnel_Stage.CreatedBy = Sessions.User.ID;
                objDbMrpEntities.Model_Stage.Add(objModel_Funnel_Stage);
            }
            else
            {
                tsvModel_Funnel_Stage.AllowedTargetStage = stageValue;
                tsvModel_Funnel_Stage.Value = objModel_Funnel_Stage.Value;
                tsvModel_Funnel_Stage.ModifiedBy = Sessions.User.ID;
                tsvModel_Funnel_Stage.ModifiedDate = DateTime.Now;
                objDbMrpEntities.Entry(tsvModel_Funnel_Stage).State = EntityState.Modified;
            }
            result = objDbMrpEntities.SaveChanges();
            return result;
        }

        #endregion

        /// <summary>
        /// Json method to check already exist model by model name
        /// model duplication check by model title
        /// </summary>
        /// <param name="Title">model title(name)</param>
        /// <returns></returns>
        public JsonResult CheckDuplicateModelTitle(string Title)
        {
            var objModel = objDbMrpEntities.Models.Where(model => model.IsDeleted == false && model.ClientId == Sessions.User.CID && model.Title.Trim().ToLower() == Title.Trim().ToLower()).FirstOrDefault();
            if (objModel == null)
            {
                return Json("notexist", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("exist", JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Json method to get the model version data based on the tab clicked
        /// </summary>
        /// <param name="id">model id</param>
        /// <returns>Json result object for model data</returns>
        public JsonResult GetModelData(int id = 0)
        {
            int modelId = id;
            if (modelId != 0)
            {
                var lstModel = objDbMrpEntities.Models.Where(model => model.ModelId == modelId).Select(model => new { model.ModelId, model.ClientId, model.Title, model.Version, model.Year, model.Status, model.IsActive, model.IsDeleted, model.AverageDealSize }).ToList();
                var lstModelFunnelStage = objDbMrpEntities.Model_Stage.Where(modelFunnelStage => modelFunnelStage.ModelId == modelId).OrderBy(modelFunnelStage => modelFunnelStage.ModelStageId).Select(modelFunnelStage => new { modelFunnelStage.ModelStageId, modelFunnelStage.StageId, modelFunnelStage.StageType, modelFunnelStage.Value, modelFunnelStage.AllowedTargetStage }).ToList();

                JsonConvert.SerializeObject(lstModel, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                JsonConvert.SerializeObject(lstModelFunnelStage, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                return Json(new { lstmodellist = lstModel, lstmodelfunnelstagelist = lstModelFunnelStage }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        #region ModelZero
        /// <summary>
        /// Action method that reurns view for no model exist in case of current year, business unit of logged-in user
        /// </summary>
        /// <returns>returns ModelZero view</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]  //// Added by Sohel Pathan on 24/06/2014 for PL ticket #519 to implement user permission Logic
        public ActionResult ModelZero()
        {
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;

            try
            {
                //// Check whether the logged-in user has a Model built for his/ her Business Unit 
                ViewBag.ModelExists = false;
                if (Sessions.User != null)
                {
                    Model objModel = new Model();
                    if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin))
                    {
                        int clientId = Sessions.User.CID;
                        objModel = (from model in objDbMrpEntities.Models
                                    where model.ClientId == clientId && model.IsDeleted == false
                                    select model).OrderBy(model => model.Status).ThenBy(model => model.CreatedDate).FirstOrDefault();
                    }
                    if (objModel != null)
                    {
                        ViewBag.ModelExists = true;
                    }
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
            }

            return View();
        }
        #endregion

        /// <summary>
        /// Functio to get latest version of the model
        /// </summary>
        /// <param name="objModel">model object</param>
        /// <returns>returns model object</returns>
        private Model GetLatestModelVersion(Model objModel)
        {
            Model versionedModel = (from model in objDbMrpEntities.Models where model.ParentModelId == objModel.ModelId select model).FirstOrDefault();
            if (versionedModel != null)
            {
                return GetLatestModelVersion(versionedModel);
            }
            else
            {
                return objModel;
            }
        }

        /// <summary>
        /// Added By: Kuber Joshi.
        /// Action method to show Model List.
        /// </summary>
        /// <param name="listType">list type</param>
        /// <returns>returns model list as json object</returns>
        public JsonResult GetModelList(string listType)
        {
            try
            {
                List<Model> objModelList = new List<Model>();
                if (!String.IsNullOrWhiteSpace(listType))
                {
                    int clientId = Sessions.User.CID;
                    List<Model> lstModels = (from model in objDbMrpEntities.Models
                                             where model.IsDeleted == false && model.ClientId == clientId && (model.ParentModelId == 0 || model.ParentModelId == null)
                                             select model).ToList();

                    if (lstModels != null && lstModels.Count > 0)
                    {
                        foreach (Model objModel in lstModels)
                        {
                            objModelList.Add(GetLatestModelVersion(objModel));
                        }
                    }
                    if (listType.Equals("active", StringComparison.OrdinalIgnoreCase))
                    {
                        objModelList = objModelList.Where(model => model.Status.ToLower() == Convert.ToString(Enums.ModelStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.ModelStatus.Published.ToString())).Value).ToLower()).ToList();
                    }
                }

                var isAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ModelCreateEdit);

                var lstModel = objModelList.Select(model => new
                {
                    id = model.ModelId,
                    title = model.Title,
                    version = model.Version,
                    status = model.Status,
                    //// Modified by Sohel Pathan on 07/07/2014 for Internal Review Points to implement custom restriction logic on Business unit.
                    isOwner = (isAuthorized == true) ? 0 : 1, //// added by Nirav Shah  on 14 feb 2014  for 256:Model list - add delete option for model and -	Delete option will be available for owner or director or system admin or client Admin
                    effectiveDate = model.EffectiveDate.HasValue == true ? model.EffectiveDate.Value.Date.ToString("M/d/yy") : "",  //// Added by Sohel on 08/04/2014 for PL #424 to show Effective Date Column
                }).OrderBy(model => model.title, new AlphaNumericComparer());

                return Json(lstModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = "#" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Action method to return LoadModelOverview view
        /// </summary>
        /// <param name="title">model title</param>
        /// <returns>returns LoadModelOverview view</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]    //// Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult LoadModelOverview(string title, int ModelId)
        {
            ModelOverView objModelOverView = new ModelOverView();

            try
            {
                ViewBag.IsServiceUnavailable = false;
                objModelOverView.ModelId = ModelId;
                objModelOverView.Title = title;
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    ViewBag.IsServiceUnavailable = true;
                }
            }

            return PartialView("_modeloverview", objModelOverView);
        }

        /// <summary>
        /// Action method that returns LoadContactInquiry view
        /// </summary>
        /// <param name="AC">addressable contacts</param>
        /// <param name="MLeads">marketing leads</param>
        /// <param name="MSize">marketing average deal size</param>
        /// <param name="TLeads">teleprospecting expected lead count</param>
        /// <param name="TSize">teleprospecting average deal size</param>
        /// <param name="SLeads">sales lead count</param>
        /// <param name="SSize">sales average deal size</param>
        /// <returns>returns LoadContactInquiry view</returns>
        //// modified datatype of MSize,TSize and SSize from int to double
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]    //// Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult LoadContactInquiry(double MSize)
        {
            ViewBag.MarketingDealSize = MSize;

            return PartialView("_contactinquiry");
        }

        /// <summary>
        /// Added By: Nirav Shah
        /// added by Nirav Shah  on 14 feb 2014  for 256:Model list - add delete option for model
        /// Action to delete Model from Model listing screen.
        /// </summary>
        /// <param name="id">model id</param>
        /// <param name="UserId">logged in user id</param>
        /// <returns>returns json object</returns>
        public JsonResult deleteModel(int id, Guid userId = new Guid())
        {
            // Get UserId Integer Id from Guid Ticket #2955
            int uId = Common.GetIntegerUserId(userId);
            //// Check for cross user login request
            if (uId != 0)
            {
                if (!Sessions.User.ID.Equals(uId))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                Model objModel = objDbMrpEntities.Models.Where(model => model.ModelId == id).FirstOrDefault();
                if (objModel != null)
                {
                    if (objModel.ClientId != Sessions.User.CID)
                    {
                        throw new Exception(string.Format("Invalid model ID: {0}", id));
                    }

                    var objPlan = objDbMrpEntities.Plans.Where(plan => plan.ModelId == id && plan.IsDeleted == false).ToList();
                    if (objPlan.Count == 0)
                    {
                        //// TFS point 252: editing a published model, Added by Nirav Shah on 18 feb 2014
                        //// change : add check before delete tactic it will check if parent published model has used these tactics.
                        using (var scope = new TransactionScope())
                        {
                            objModel.IsDeleted = true;
                            objDbMrpEntities.Entry(objModel).State = EntityState.Modified;

                            //// Start - Added by Sohel Pathan on 22/08/2014 for Internal Review Points #90
                            var lstTacticType = objDbMrpEntities.TacticTypes.Where(tacticType => tacticType.ModelId == id && tacticType.IsDeleted == false).Select(tacticType => tacticType).ToList();
                            lstTacticType.ForEach(tacticType => { tacticType.IsDeleted = true; objDbMrpEntities.Entry(tacticType).State = EntityState.Modified; });

                            var lstLineItemType = objDbMrpEntities.LineItemTypes.Where(lineItemType => lineItemType.ModelId == id && lineItemType.IsDeleted == false).Select(lineItemType => lineItemType).ToList();
                            lstLineItemType.ForEach(lineItemType => { lineItemType.IsDeleted = true; objDbMrpEntities.Entry(lineItemType).State = EntityState.Modified; });
                            //// End - Added by Sohel Pathan on 22/08/2014 for Internal Review Points #90

                            //// Parent of ModelId
                            while (objModel.Model2 != null)
                            {
                                objModel = objModel.Model2;
                                objModel.IsDeleted = true;

                                //// Start - Added by Sohel Pathan on 22/08/2014 for Internal Review Points #90
                                lstTacticType = objDbMrpEntities.TacticTypes.Where(tacticType => tacticType.ModelId == objModel.ModelId && tacticType.IsDeleted == false).Select(tacticType => tacticType).ToList();
                                lstTacticType.ForEach(tacticType => { tacticType.IsDeleted = true; objDbMrpEntities.Entry(tacticType).State = EntityState.Modified; });

                                lstLineItemType = objDbMrpEntities.LineItemTypes.Where(lineItemType => lineItemType.ModelId == objModel.ModelId && lineItemType.IsDeleted == false).Select(lineItemType => lineItemType).ToList();
                                lstLineItemType.ForEach(lineItemType => { lineItemType.IsDeleted = true; objDbMrpEntities.Entry(lineItemType).State = EntityState.Modified; });
                                //// End - Added by Sohel Pathan on 22/08/2014 for Internal Review Points #90

                                if (objModel.Status.ToLower() != Enums.ModelStatus.Draft.ToString().ToLower())
                                {
                                    objPlan = objDbMrpEntities.Plans.Where(plan => plan.ModelId == objModel.ModelId && plan.IsDeleted == false).ToList();
                                    if (objPlan.Count != 0)
                                    {
                                        return Json(new { errorMsg = string.Format(Common.objCached.ModelDeleteParentDependency, objModel.Title, objModel.Version) }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                objDbMrpEntities.Entry(objModel).State = EntityState.Modified;
                            }
                            objDbMrpEntities.SaveChanges();   //// Added by Sohel Pathan on 22/08/2014 for Internal Review Points
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
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
                return Json(new { errorMsg = objException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Tactic Selection

        /// <summary>
        /// Added By: Nirav Shah
        /// Action method to show Tactics view.
        /// </summary>
        /// <param name="id">model id</param>
        /// <param name="showMessage">boolean flag that indicated to show error/success messages or not</param>
        /// <returns>returns Tactics view</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]    //// Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult Tactics(int id = 0, bool showMessage = true)
        {
            if (TempData["modelIdForTactics"] != null)
            {
                if (id <= 0)
                {
                    id = (int)TempData["modelIdForTactics"];
                    TempData["modelIdForTactics"] = id;
                }
            }
            int Modelid = id;
            Model objModel = objDbMrpEntities.Models.Where(model => model.ModelId == Modelid).Select(model => model).FirstOrDefault();
            //// Modified by Mitesh Vaishnav for  on 06/08/2014 PL ticket #683 and Brad Gray on 07/23/2015 for PL#1448

            if (objModel != null && (objModel.IntegrationInstanceId != null || objModel.IntegrationInstanceIdCW != null || objModel.IntegrationInstanceIdINQ != null || objModel.IntegrationInstanceIdMQL != null || objModel.IntegrationInstanceIdProjMgmt != null || objModel.IntegrationInstanceEloquaId != null || objModel.IntegrationInstanceMarketoID != null))
            {
                ViewBag.IsModelIntegrated = true;
            }
            else
            {
                ViewBag.IsModelIntegrated = false;
            }
            //// End :Modified by Mitesh Vaishnav on 06/08/2014 for PL ticket #683
            Tactic_TypeModel objTacticTypeModel = FillInitialTacticData(id);
            objTacticTypeModel.ModelId = id;
            ViewBag.Version = id;
            string Title = objTacticTypeModel.Versions.Where(version => version.IsLatest == true).Select(version => version.Title).FirstOrDefault();
            ViewBag.ModelPublishComfirmation = Common.objCached.ModelPublishComfirmation;
            ViewBag.Published = Enums.ModelStatus.Published.ToString().ToLower();
            ViewBag.Flag = false;
            if (id != 0)
            {
                ViewBag.Flag = chekckParentPublishModel(id);
                //// Added by Sohel Pathan on 07/07/2014 for Internal Review Points to implement custom restriction logic on Business unit.
                ViewBag.IsOwner = objDbMrpEntities.Models.Where(model => model.IsDeleted.Equals(false) && model.ModelId == id && model.CreatedBy == Sessions.User.ID).Any();
            }
            else
            {
                ViewBag.IsOwner = true; //// Added by Sohel Pathan on 07/07/2014 for Internal Review Points to implement custom restriction logic on Business unit.
            }

            ViewBag.IsAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ModelCreateEdit);

            string StageType = Enums.StageType.CR.ToString();
            string ModelTitle = objDbMrpEntities.Models.Where(model => model.IsDeleted == false && model.ModelId == Modelid).Select(model => model.Title).FirstOrDefault();
            Model_Stage objStage = objDbMrpEntities.Model_Stage.Where(modelFunnelStage => modelFunnelStage.StageType == StageType && modelFunnelStage.ModelId == Modelid && modelFunnelStage.AllowedTargetStage == true).OrderBy(modelFunnelStage => modelFunnelStage.Stage.Level).Distinct().FirstOrDefault();
            if (objStage == null)
            {
                TempData["ErrorMessage"] = string.Format(Common.objCached.StageNotExist);
            }
            //// Added by :- Sohel Pathan on 06/06/2014 for PL ticket #516.
            ViewBag.TargetStageNotAssociatedWithModelMsg = string.Format(Common.objCached.TargetStageNotAssociatedWithModelMsg);

            if (showMessage == false)
            {
                TempData["SuccessMessage"] = string.Empty;
            }

            return View("Tactics", objTacticTypeModel);
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Function to fill initialTacticdata version wise.
        /// </summary>
        /// <param name="Modelid">model id</param>
        /// <returns>returns Tactic_TypeModel object</returns>
        public Tactic_TypeModel FillInitialTacticData(int Modelid = 0)
        {
            Tactic_TypeModel objTacticTypeModel = new Tactic_TypeModel();
            string Status = objDbMrpEntities.Models.Where(model => model.IsDeleted == false && model.ModelId == Modelid).Select(model => model.Status).FirstOrDefault();
            objTacticTypeModel.Versions = GetModelVersions(Modelid);
            objTacticTypeModel.Status = Status;
            return objTacticTypeModel;
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action method to show Version.
        /// </summary>
        /// <param name="id">model id</param>
        /// <returns>returns json result object</returns>
        public JsonResult FillVersion(int id)
        {
            var objModel = objDbMrpEntities.Models.Where(model => model.ModelId == id).Select(model => model).ToList();
            var objReturnModel = objModel.Select(model => new
            {
                Title = model.Title,
                Status = model.Status,
            }).Select(model => model).Distinct();
            return Json(objReturnModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action method to show Tactics List by ModelId.
        /// </summary>
        /// <param name="id">mdoel id</param>
        /// <returns>returns json result object</returns>
        public JsonResult GetTacticDatabyid(int id)
        {
            //// Added by Dharmraj , ticket #592 Tactic type data model
            var objTacticList = objDbMrpEntities.TacticTypes.Where(tacticType => tacticType.ModelId == id && tacticType.IsDeleted == false).ToList();
            PlanExchangeRate = Sessions.PlanExchangeRate;
            //// Start - Added by :- Sohel Pathan on 06/06/2014 for PL ticket #516.
            string StageType = Enums.StageType.CR.ToString();
            var stagesList = objDbMrpEntities.Model_Stage.Where(modelFunnelStage => modelFunnelStage.ModelId == id && modelFunnelStage.AllowedTargetStage == true && modelFunnelStage.StageType == StageType)
                                                                .Select(modelFunnelStage => modelFunnelStage.StageId).Distinct().ToList();
            //// End - Added by :- Sohel Pathan on 06/06/2014 for PL ticket #516.

            var allTacticTypes = objTacticList.Select(tacticType => new
            {
                id = tacticType.TacticTypeId,
                clientid = Sessions.User.CID,
                modelId = tacticType.ModelId, //// TFS Bug - 179 : Improper behavior when editing Tactic in model   Changed By : Nirav shah on 6 Feb 2014 Change : add modelId = p.ModelId,    
                title = tacticType.Title,
                Stage = (tacticType.StageId == null) ? "-" : tacticType.Stage.Title, //// Modified by dharmraj for ticket #475, Old line : Stage = (p.StageId == null) ? "-" : p.Stage.Code
                //// changes done by uday for PL #497 changed projectedmlqs to projectedstagevalue
                ProjectedStageValue = (tacticType.ProjectedStageValue == null) ? 0 : tacticType.ProjectedStageValue,
                revenue = (tacticType.ProjectedRevenue == null) ? 0 : objCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(tacticType.ProjectedRevenue)),PlanExchangeRate), //Added by Rahul Shah for PL #2498.
                //revenue = (tacticType.ProjectedRevenue == null) ? 0 : tacticType.ProjectedRevenue,//Commented by Rahul Shah for PL #2498.
                IsDeployedToIntegration = tacticType.IsDeployedToIntegration,
                IsTargetStageOfModel = (tacticType.StageId == null) ? true : stagesList.Contains(Convert.ToInt32(tacticType.StageId)),     //// Added by :- Sohel Pathan on 06/06/2014 for PL ticket #516.
                IsDeployedToModel = tacticType.IsDeployedToModel, //// added by dharmraj for #592 : Tactic type data model
                currentWorkFrontTemplate = tacticType.WorkFrontTemplateId, //added by Brad Gray 7/28/2015 PL#1374, #1373 - updated by Brad Gray 1/7/2016 PL#1856
                AssetType = tacticType.AssetType
            }).Select(tacticType => tacticType).Distinct().OrderBy(tacticType => tacticType.title, new AlphaNumericComparer());

            return Json(allTacticTypes, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action method to show Tactics Detail.
        /// </summary>
        /// <param name="id">tactic type id</param>
        /// <param name="ModelId">model id</param>
        /// <returns>returns CreateTactic partial view</returns>
        public PartialViewResult DetailTacticData(int id = 0, int ModelId = 0)
        {
            Tactic_TypeModel objTacticTypeMdoel = new Tactic_TypeModel();
            try
            {
                PlanExchangeRate = Sessions.PlanExchangeRate;
                bool isIntegratedWithMarketo = false;
                var objModel = objDbMrpEntities.Models.Where(model => model.ModelId == ModelId).FirstOrDefault();
                ViewBag.CampaignFolderList = "";
                ViewBag.DDLProgramType = "";
                ViewBag.DDLChannel = "";
                //// Modified by Mitesh Vaishnav for  on 06/08/2014 PL ticket #683
                if (objModel.IntegrationInstanceId != null || objModel.IntegrationInstanceIdCW != null || objModel.IntegrationInstanceIdINQ != null || objModel.IntegrationInstanceIdMQL != null || objModel.IntegrationInstanceIdProjMgmt != null || objModel.IntegrationInstanceEloquaId != null || objModel.IntegrationInstanceMarketoID != null)
                {
                    ViewBag.IsModelIntegrated = true;
                    //Begin added by Brad Gray 7/28/2015 PL#1374, #1373
                    IntegrationInstance intInstanceProjMgmt = objModel.IntegrationInstance6;
                    bool isIntegratedWithWorkFront = false;
                    var Marketoinstance = objModel.IntegrationInstance3;

                    List<IntegrationWorkFrontTemplate> workFrontTemplates = new List<IntegrationWorkFrontTemplate>();
                    if ((intInstanceProjMgmt != null) && (intInstanceProjMgmt.IntegrationType.Code == Enums.IntegrationInstanceType.WorkFront.ToString()))
                    {
                        isIntegratedWithWorkFront = true;
                    }
                    if (Marketoinstance != null && (Marketoinstance.IntegrationType.Code == Enums.IntegrationInstanceType.Marketo.ToString()))
                    {
                        isIntegratedWithMarketo = true;
                    }
                    workFrontTemplates = objDbMrpEntities.IntegrationWorkFrontTemplates.Where(modelTemplate => modelTemplate.IntegrationInstanceId == objModel.IntegrationInstanceIdProjMgmt &&
                                                                  modelTemplate.IsDeleted == 0).OrderBy(modelTemplate => modelTemplate.Template_Name).ToList();

                    ViewBag.WorkFrontTemplates = workFrontTemplates.Select(modelTemplate => new { modelTemplate.ID, modelTemplate.Template_Name }).Distinct().ToList();
                    ViewBag.isIntegratedWithWorkFront = isIntegratedWithWorkFront;
                    //End addition by Brad Gray for PL#1734
                    ViewBag.isIntegratedWithMarketo = isIntegratedWithMarketo;
                    if (objModel.IntegrationInstanceMarketoID != null)
                    {
                        ApiIntegration ObjApiintegration = new ApiIntegration(Enums.ApiIntegrationData.Programtype.ToString(), objModel.IntegrationInstanceMarketoID);
                        MarketoDataObject CampaignFolderList = ObjApiintegration.GetProgramChannellistData();

                        ViewBag.DDLProgramType = CampaignFolderList.program.Select(list => new
                        {
                            Text = list.Key,
                            Value = list.Value,
                        }).OrderBy(model => model.Text, new AlphaNumericComparer()).ToList();


                        ViewBag.DDLChannel = CampaignFolderList.channels.Select(list => new
                        {
                            id = list.id,
                            ChannelName = list.name,
                            Parentprogramid = list.applicableProgramType
                        }).OrderBy(model => model.ChannelName, new AlphaNumericComparer()).ToList();
                    }
                }
                else
                {
                    ViewBag.IsModelIntegrated = false;
                }
                //// End :Modified by Mitesh Vaishnav on 06/08/2014 for PL ticket #683

                //// changed by Nirav Shah on 2 APR 2013
                string StageType = Enums.StageType.CR.ToString();
                //// Changed by dharmraj for ticket #475
                ViewBag.Stages = objDbMrpEntities.Model_Stage.Where(modelFunnelStage => modelFunnelStage.ModelId == ModelId &&
                                                                  modelFunnelStage.AllowedTargetStage == true &&
                                                                  modelFunnelStage.StageType == StageType).OrderBy(modelFunnelStage => modelFunnelStage.Stage.Level)
                                                      .Select(modelFunnelStage => new { modelFunnelStage.StageId, modelFunnelStage.Stage.Title }).Distinct().ToList();
                ViewBag.IsCreated = false;

                //Added By Komal rawal for #2356 add roi package tactic type selection
                Dictionary<string, string> AssetType = new Dictionary<string, string>();
                AssetType.Add(Enums.AssetType.Asset.ToString(), Enums.AssetType.Asset.ToString());
                AssetType.Add(Enums.AssetType.Promotion.ToString(), Enums.AssetType.Promotion.ToString());

                ViewBag.BindAssetType = AssetType.Select(list => new
                {
                    ID = list.Key,
                    Title = list.Value
                }).ToList();
                //End
                TacticType objTacticType = objDbMrpEntities.TacticTypes.Where(tacticType => tacticType.TacticTypeId.Equals(id)).FirstOrDefault();
                objTacticTypeMdoel.TacticTypeId = objTacticType.TacticTypeId;
                objTacticTypeMdoel.Title = System.Web.HttpUtility.HtmlDecode(objTacticType.Title);  /////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
                objTacticTypeMdoel.ClientId = Sessions.User.CID;
                objTacticTypeMdoel.Description = System.Web.HttpUtility.HtmlDecode(objTacticType.Description);  ////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
                //// changed for TFS bug 176 : Model Creation - Tactic Defaults should Allow values of zero changed by Nirav Shah on 7 feb 2014
                //// changed by Nirav Shah on 2 APR 2013
                //// changes done by uday for PL #497 changed projectedmlqs to projectedstagevalue
                objTacticTypeMdoel.ProjectedStageValue = (objTacticType.ProjectedStageValue != null) ? objTacticType.ProjectedStageValue : 0;
                objTacticTypeMdoel.ProjectedRevenue = (objTacticType.ProjectedRevenue != null) ? objCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(objTacticType.ProjectedRevenue)),PlanExchangeRate) : 0; //Modified by Rahul Shah for PL #2498.

                //// added by dharmraj for ticket #433 Integration - Model Screen Tactic List
                objTacticTypeMdoel.IsDeployedToIntegration = objTacticType.IsDeployedToIntegration;
                objTacticTypeMdoel.StageId = objTacticType.StageId;
                objTacticTypeMdoel.ModelId = objTacticType.ModelId;
                objTacticTypeMdoel.WorkFrontTemplateId = objTacticType.WorkFrontTemplateId; //updated 1/7/2016 by Brad Gray PL#1856
                objTacticTypeMdoel.AssetType = objTacticType.AssetType;    //Added By Komal rawal for #2356 add roi package tactic type selection

                //// added by Dharmraj, ticket #592 : Tactic type data model
                ViewBag.IsDeployed = objTacticType.IsDeployedToModel;

                //// Start Manoj Limbachiya 05May2014 PL#458
                ViewBag.CanDelete = false;
                if (objTacticType.ModelId != 0)
                {
                    ViewBag.CanDelete = true;
                }
                //// End Manoj Limbachiya 05May2014 PL#458

                //Added By Komal Rawal for #2216 
                MarketoEntityValueMapping SelectCampaignFolderValue;
                if (isIntegratedWithMarketo)
                {
                    var EntityType = Enums.FilterLabel.TacticType.ToString();
                    SelectCampaignFolderValue = objDbMrpEntities.MarketoEntityValueMappings.Where(val => val.EntityID == objTacticTypeMdoel.TacticTypeId && val.EntityType == EntityType).FirstOrDefault();
                    if (SelectCampaignFolderValue != null)
                    {

                        objTacticTypeMdoel.programType = SelectCampaignFolderValue.ProgramType;
                        objTacticTypeMdoel.Channel = SelectCampaignFolderValue.Channel;
                    }

                }
                //// Start - Added By Sohel Pathan on 16/06/2014 for PL ticket #528
                ViewBag.ModelStatus = objModel.Status;
                ViewBag.TacticTypeStageId = objTacticType.StageId;
                ViewBag.ChangeTargetStageMsg = Common.objCached.ChangeTargetStageMsg;
                //// End - Added By Sohel Pathan on 16/06/2014 for PL ticket #528
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }

            return PartialView("CreateTactic", objTacticTypeMdoel);
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action method to show Create Tactic screen.
        /// </summary>
        /// <param name="ModelId">model id</param>
        /// <returns>returns CreateTactic partial view</returns>
        public PartialViewResult CreateTacticData(int ModelId = 0)
        {
            var objModel = objDbMrpEntities.Models.Where(model => model.ModelId == ModelId).FirstOrDefault();
            //// Modified by Mitesh Vaishnav for  on 06/08/2014 PL ticket #683
            if (objModel.IntegrationInstanceId != null || objModel.IntegrationInstanceIdCW != null || objModel.IntegrationInstanceIdINQ != null || objModel.IntegrationInstanceIdMQL != null || objModel.IntegrationInstanceIdProjMgmt != null || objModel.IntegrationInstanceEloquaId != null || objModel.IntegrationInstanceMarketoID != null)
            {
                ViewBag.IsModelIntegrated = true;
                var intInstanceProjMgmt = objModel.IntegrationInstance6;
                var Marketoinstance = objModel.IntegrationInstance6;
                bool isIntegratedWithWorkFront = false;
                bool isIntegratedWithMarketo = false;
                List<IntegrationWorkFrontTemplate> workFrontTemplates = new List<IntegrationWorkFrontTemplate>();
                if ((intInstanceProjMgmt != null) && (intInstanceProjMgmt.IntegrationType.Code == Enums.IntegrationInstanceType.WorkFront.ToString()))
                {
                    isIntegratedWithWorkFront = true;
                }
                if (Marketoinstance != null && (Marketoinstance.IntegrationType.Code == Enums.IntegrationInstanceType.Marketo.ToString()))
                {
                    isIntegratedWithMarketo = true;
                }
                ViewBag.WorkFrontTemplates = objDbMrpEntities.IntegrationWorkFrontTemplates.Where(modelTemplate => modelTemplate.IntegrationInstanceId == objModel.IntegrationInstanceIdProjMgmt &&
                                                             modelTemplate.IsDeleted == 0).OrderBy(modelTemplate => modelTemplate.Template_Name)
                                                 .Select(modelTemplate => new { modelTemplate.ID, modelTemplate.Template_Name }).Distinct().ToList();
                ViewBag.isIntegratedWithWorkFront = isIntegratedWithWorkFront;
                //End addition by Brad Gray for PL#1734
                ViewBag.isIntegratedWithMarketo = isIntegratedWithMarketo;
                //Added By Komal Rawal for #2134 to get list of dropdown using api
                if (objModel.IntegrationInstanceMarketoID != null)
                {
                    ApiIntegration ObjApiintegration = new ApiIntegration(Enums.ApiIntegrationData.Programtype.ToString(), objModel.IntegrationInstanceMarketoID);
                    MarketoDataObject CampaignFolderList = ObjApiintegration.GetProgramChannellistData();

                    ViewBag.DDLProgramType = CampaignFolderList.program.Select(list => new
                    {
                        Text = list.Key,
                        Value = list.Value,
                    }).OrderBy(model => model.Text, new AlphaNumericComparer()).ToList();


                    ViewBag.DDLChannel = CampaignFolderList.channels.Select(list => new
                    {
                        id = list.id,
                        ChannelName = list.name,
                        Parentprogramid = list.applicableProgramType
                    }).OrderBy(model => model.ChannelName, new AlphaNumericComparer()).ToList();
                }
            }
            else
            {
                ViewBag.IsModelIntegrated = false;
            }
            //// End :Modified by Mitesh Vaishnav on 06/08/2014 for PL ticket #683

            //// changed by Nirav Shah on 2 APR 2013
            string StageType = Enums.StageType.CR.ToString();
            ViewBag.Stages = objDbMrpEntities.Model_Stage.Where(modelFunnelStage => modelFunnelStage.ModelId == ModelId && modelFunnelStage.AllowedTargetStage == true && modelFunnelStage.StageType == StageType).Select(modelFunnelStage => new { modelFunnelStage.StageId, modelFunnelStage.Stage.Title }).Distinct().ToList();
            ViewBag.IsCreated = true;

            //Added By Komal rawal for #2356 add roi package tactic type selection
            Dictionary<string, string> AssetType = new Dictionary<string, string>();
            AssetType.Add(Enums.AssetType.Asset.ToString(), Enums.AssetType.Asset.ToString());
            AssetType.Add(Enums.AssetType.Promotion.ToString(), Enums.AssetType.Promotion.ToString());

            ViewBag.BindAssetType = AssetType.Select(list => new
            {
                ID = list.Key,
                Title = list.Value
            });

            //End
            Tactic_TypeModel objTacticType = new Tactic_TypeModel();
            //// changed for TFS bug 176 : Model Creation - Tactic Defaults should Allow values of zero changed by Nirav Shah on 7 feb 2014
            objTacticType.ProjectedStageValue = 0;
            objTacticType.ProjectedRevenue = 0;
            //// Start Manoj Limbachiya 05May2014 PL#458
            ViewBag.CanDelete = false;
            //// End Manoj Limbachiya 05May2014 PL#458
            //// Start Manoj Limbachiya PL # 486
            ViewBag.IsDeployed = true;
            //// End Manoj Limbachiya PL # 486

            return PartialView("CreateTactic", objTacticType);
        }

        /// <summary>
        /// Delete tactic type from client level and model level
        /// Author: Manoj Limbachiya
        /// Reference: PL#458
        /// Date: 05May2014
        /// </summary>
        /// <param name="id">tactic type id</param>
        /// <param name="UserId">logged in user id</param>  ////Added by Sohel Pathan on 19/06/2014 for PL ticket #536
        /// <returns></returns>
        [HttpPost]
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]    //// Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult DeleteTactic(int id = 0, Guid userId = new Guid())
        {
            // Get UserId Integer Id from Guid Ticket #2955
            int uId = Common.GetIntegerUserId(userId);
            //// Start - Added by Sohel Pathan on 19/06/2014 for PL ticket #536
            //// Cross client user login check
            if (uId != 0)
            {
                if (!Sessions.User.ID.Equals(uId))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            //// End - Added by Sohel Pathan on 19/06/2014 for PL ticket #536

            TacticType objTacticType = objDbMrpEntities.TacticTypes.Where(tacticType => tacticType.TacticTypeId == id).FirstOrDefault();
            if (objTacticType != null)   //// Modified by Sohel Pathan on 19/06/2014 for PL ticket #536
            {
                int ModelId = objTacticType.Model.ModelId;
                Model objModel = objDbMrpEntities.Models.Where(model => model.ModelId == ModelId).FirstOrDefault();
                if (objModel != null)
                {
                    if (objModel.Status.ToLower() == Convert.ToString(Enums.ModelStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.ModelStatus.Published.ToString())).Value).ToLower())
                    {
                        List<TacticType> lstTactictTypes = objDbMrpEntities.TacticTypes.Where(tacticType => tacticType.ModelId == ModelId && tacticType.IsDeployedToModel == true && (tacticType.IsDeleted == null || tacticType.IsDeleted == false)).ToList();
                        if (lstTactictTypes.Count == 1)
                        {
                            return Json(new { status = "ERROR", Message = string.Format(Common.objCached.ModelTacticCannotDelete) });
                        }
                    }
                }
            }

            TacticType objTacticTypeDB = objDbMrpEntities.TacticTypes.Where(tacticType => tacticType.TacticTypeId == id).FirstOrDefault();
            if (objTacticTypeDB != null)
            {
                objTacticTypeDB.IsDeleted = true;
                objTacticTypeDB.ModifiedBy = Sessions.User.ID;
                objTacticTypeDB.ModifiedDate = DateTime.Now;
                objDbMrpEntities.TacticTypes.Attach(objTacticTypeDB);
                objDbMrpEntities.Entry(objTacticTypeDB).State = EntityState.Modified;
                //Modified By Komal rawal for #2216 deleting tactic type marketo mapping 
                string EntityType = Enums.FilterLabel.TacticType.ToString();
                MarketoEntityValueMapping DeleteTacticTypeMapping = objDbMrpEntities.MarketoEntityValueMappings.Where(Entity => Entity.EntityID == id && Entity.EntityType == EntityType).FirstOrDefault();
                if (DeleteTacticTypeMapping != null)
                {
                    objDbMrpEntities.Entry(DeleteTacticTypeMapping).State = EntityState.Deleted;
                }
                //End

                objDbMrpEntities.SaveChanges();
                //Added By Komal rawal for #2356 Delete associated package on deletion of Tactic type
                bool DeleteAllpackage = false;
                if(objTacticTypeDB.AssetType == Enums.AssetType.Asset.ToString())
                {
                    DeleteAllpackage = true;
                }
                UpdatePackageDetails(id, DeleteAllpackage);
                //End
                //remove media code of tactic
                if (Sessions.IsMediaCodePermission == true)
                {
                    List<int> tacticId = new List<int>();
                    tacticId.Add(id);
                    Common.RemoveTacticMediaCode(tacticId);
                }
                //end
                Common.InsertChangeLog(Sessions.ModelId, 0, objTacticTypeDB.TacticTypeId, objTacticTypeDB.Title, Enums.ChangeLog_ComponentType.tactictype, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.removed);

                //// Dispose db object
                objDbMrpEntities.Dispose();
                TempData["SuccessMessage"] = string.Format(Common.objCached.ModelTacticDeleted, objTacticTypeDB.Title);

                return Json(new { status = "SUCCESS" });
            }

            return Json(new { status = "ERROR", Message = string.Format(Common.objCached.ErrorOccured) });
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Action to Save Tactic data .
        /// Changed ProjectedStageValue,ProjectedRevenue parameters datatype int to double for not saving tactic with large value. by dharmraj
        /// changes done by uday for PL #497 changed projectedmlqs to projectedstagevalue
        /// </summary>
        /// <param name="Title">tactic type name</param>
        /// <param name="Description">decription</param>
        /// <param name="StageId">stage id</param>
        /// <param name="ProjectedStageValue">Projected Stage Value</param>
        /// <param name="ProjectedRevenue">Projected Revenue</param>
        /// <param name="TacticTypeId">Tactic Type Id</param>
        /// <param name="modelID">model id</param>
        /// <param name="isDeployedToIntegration">isDeployedToIntegration flag</param>
        /// <param name="isDeployedToModel">isDeployedToModel flag</param>
        /// <param name="WorkFrontTemplate">template id of workfront template as a string - Added by Brad Gray 7/24/2015 for PL#1373, 1374</param>
        /// <returns>returns json result object</returns>
        [HttpPost]
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]    //// Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult SaveTactic(string Title, string Description, int? StageId, double ProjectedStageValue, double ProjectedRevenue, int TacticTypeId, string modelID, bool isDeployedToIntegration, bool isDeployedToModel, string WorkFrontTemplate, string AssetType, string ProgramType = "", string Channel = "", bool DeleteAllPackage = false)
        {
            try
            {
                PlanExchangeRate = Sessions.PlanExchangeRate;
                TacticType objtactic = new TacticType();
                int ModelId;
                int.TryParse(modelID, out ModelId);
                objtactic.Title = Title;
                objtactic.Description = Description;
                ////changes done by uday for PL #497 changed projectedmlqs to projectedstagevalue
                objtactic.ProjectedStageValue = ProjectedStageValue;
                objtactic.ProjectedRevenue = objCurrency.SetValueByExchangeRate(double.Parse(Convert.ToString(ProjectedRevenue)), PlanExchangeRate); //Modified by Rahul Shah for PL #2498.
                //objtactic.ProjectedRevenue = ProjectedRevenue;
                //// changed by Nirav Shah on 2 APR 2013
                objtactic.StageId = StageId;
                int intRandomColorNumber = rnd.Next(Common.ColorcodeList.Count);
                objtactic.ColorCode = Convert.ToString(Common.ColorcodeList[intRandomColorNumber]);
                objtactic.CreatedDate = System.DateTime.Now;
                objtactic.CreatedBy = Sessions.User.ID;
                //// Start Manoj Limbachiya PL # 486
                objtactic.ModelId = ModelId;
                objtactic.IsDeployedToModel = isDeployedToModel;
                objtactic.AssetType = AssetType;//Added By Komal rawal for #2356 add roi package tactic type selection

              
                
                //begin added by Brad Gray 01/7/2016 PL#1856
                int parsedTemplateId;
                bool templateResult = Int32.TryParse(WorkFrontTemplate, out parsedTemplateId);
                if (templateResult == false)
                {
                    objtactic.WorkFrontTemplateId = null;
                }
                else
                {
                    objtactic.WorkFrontTemplateId = parsedTemplateId; //added Brad Gray 07/24/2015 PL#1374 tactic type to workfront template mapping - updated 1/7/2016 by Brad GrayPL#1856
                }
                //end PL#1856
                Model objModel = objDbMrpEntities.Models.Where(model => model.ModelId == ModelId).FirstOrDefault();
                if (!isDeployedToModel)
                {

                    if (objModel != null)
                    {
                        if (objModel.Status.ToLower() == Convert.ToString(Enums.ModelStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.ModelStatus.Published.ToString())).Value).ToLower())
                        {
                            List<TacticType> lstTactictTypes = objDbMrpEntities.TacticTypes.Where(tacticType => tacticType.ModelId == ModelId && tacticType.IsDeployedToModel == true && (tacticType.IsDeleted == null || tacticType.IsDeleted == false)).ToList();
                            if (lstTactictTypes.Count == 1)
                            {
                                return Json(new { errormsg = Common.objCached.TacticReqForPublishedModel });
                            }
                        }
                    }
                }
                ////End Manoj Limbachiya PL # 486



                //// Added by Dharmraj for ticket #433 Integration - Model Screen Tactic List
                objtactic.IsDeployedToIntegration = isDeployedToIntegration;

                //// Change TFS Bug - 166 : Improper behavior when editing Tactic in model 
                //// Changed By : Nirav shah on 6 Feb 2014
                if (TacticTypeId == 0)
                {
                    var existingTacticTypes = objDbMrpEntities.TacticTypes.Where(tacticType => tacticType.ModelId == ModelId && tacticType.Title.ToLower() == Title.ToLower() && (tacticType.IsDeleted == null || tacticType.IsDeleted == false)).ToList();
                    if (existingTacticTypes.Count == 0)
                    {
                        objtactic.IsDeleted = false;
                        objDbMrpEntities.TacticTypes.Attach(objtactic);
                        objDbMrpEntities.Entry(objtactic).State = EntityState.Added;
                        objDbMrpEntities.SaveChanges();

                        Common.InsertChangeLog((int)ModelId, 0, objtactic.TacticTypeId, objtactic.Title, Enums.ChangeLog_ComponentType.tactictype, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.added);
                        objDbMrpEntities.Dispose();

                        TempData["SuccessMessage"] = string.Format(Common.objCached.ModelNewTacticSaveSucess, Title);
                    }
                    else
                    {
                        string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    //// Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                        return Json(new { errormsg = strDuplicateMessage });
                    }
                    //Added By Komal Rawal for #2216  saving of channel and program type
                    if (objModel.IntegrationInstanceMarketoID != null)
                    {
                        SaveMarketoSettings(objtactic.TacticTypeId, objModel.IntegrationInstanceMarketoID, Enums.FilterLabel.TacticType.ToString(), ProgramType, Channel);
                    }

                }
                else
                {
                    UpdatePackageDetails(TacticTypeId, DeleteAllPackage);     //Added By Komal rawal for #2356 add roi package tactic type selection
                    //remove media code of tactic
                    if (AssetType == Convert.ToString(Enums.AssetType.Asset) && Sessions.IsMediaCodePermission==true)
                    {
                        var AssociatedTacticIds = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(list => list.TacticTypeId == TacticTypeId && list.IsDeleted == false).Select(list => list.PlanTacticId).ToList();
                        var mediCodeTacticd = objDbMrpEntities.Tactic_MediaCodes.Where(a => AssociatedTacticIds.Contains(a.TacticId)).Select(a => a.TacticId).ToList();
                        Common.RemoveTacticMediaCode(mediCodeTacticd);
                    }
                    //end
                    var existingTacticTypes = objDbMrpEntities.TacticTypes.Where(tacticType => (tacticType.ModelId == ModelId) && tacticType.Title.ToLower() == Title.ToLower() && tacticType.TacticTypeId != TacticTypeId && (tacticType.IsDeleted == null || tacticType.IsDeleted == false)).ToList();

                    //// TFS Bug - 179 : Improper behavior when editing Tactic in model 
                    //// Changed By : Nirav shah on 6 Feb 2014
                    //// Change : add new condition t.ModelId == null
                    if (existingTacticTypes.Count == 0)
                    {
                        var objTacticType = objDbMrpEntities.TacticTypes.Where(tacticType => tacticType.TacticTypeId == TacticTypeId).FirstOrDefault();
                        if (objTacticType != null)
                        {
                            objtactic.Abbreviation = objTacticType.Abbreviation;
                            objtactic.PreviousTacticTypeId = objTacticType.PreviousTacticTypeId;
                        }
                        objtactic.TacticTypeId = TacticTypeId;
                        objtactic.IsDeleted = false;

                        MRPEntities dbedit = new MRPEntities();
                        dbedit.Entry(objtactic).State = EntityState.Modified;
                        dbedit.SaveChanges();

                        Common.InsertChangeLog((int)ModelId, 0, objtactic.TacticTypeId, objtactic.Title, Enums.ChangeLog_ComponentType.tactictype, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.updated);
                        dbedit.Dispose();
                        TempData["SuccessMessage"] = string.Format(Common.objCached.ModelTacticEditSucess, Title);

                        //#2063: Tactic 'Deployed To Integration' not defaulting to on
                        // Added by Viral on 04/08/2016
                        #region "Update DeployToIntegration settings for all tactics related to Model & TacticType"
                        if (isDeployedToModel)
                        {
                            //Model objModel = objDbMrpEntities.Models.Where(model => model.ModelId == ModelId).FirstOrDefault();
                            string strModelStatus = objDbMrpEntities.Models.Where(model => model.ModelId == ModelId).Select(mdl => mdl.Status).FirstOrDefault();
                            int sfdcInstanceId = 0, elqaInstanceId = 0, workfrontInstanceId = 0, marketoInstanceId = 0;
                            if (!string.IsNullOrEmpty(strModelStatus) && Enums.ModelStatus.Published.ToString().Equals(strModelStatus))
                            {
                                if (objModel.IntegrationInstanceId.HasValue)
                                    sfdcInstanceId = objModel.IntegrationInstanceId.Value;
                                if (objModel.IntegrationInstanceEloquaId.HasValue)
                                    elqaInstanceId = objModel.IntegrationInstanceEloquaId.Value;
                                if (objModel.IntegrationInstanceIdProjMgmt.HasValue)
                                    workfrontInstanceId = objModel.IntegrationInstanceIdProjMgmt.Value;
                                if (objModel.IntegrationInstanceMarketoID.HasValue) //Added by Rahul Shah for PL #2189 
                                    marketoInstanceId = objModel.IntegrationInstanceMarketoID.Value;


                                List<int> lstTacticTypeIds = new List<int>();
                                lstTacticTypeIds.Add(objTacticType.TacticTypeId);
                                // Get list of tactics related to TacticType.
                                //List<Plan_Campaign_Program_Tactic> lstTactics = objTacticType.Plan_Campaign_Program_Tactic.ToList();
                                List<Plan_Campaign_Program_Tactic> lstTactics = GetPlanTacticsByTacticType(objModel.ModelId, lstTacticTypeIds);
                                if (lstTactics != null && lstTactics.Count > 0)
                                {
                                    lstTactics = lstTactics.Where(tac => tac.IsDeleted == false).ToList();
                                    if (lstTactics != null && lstTactics.Count > 0)
                                    {
                                        #region "Update Tactics IsDeployedToIntegration settings"
                                        try
                                        {
                                            objDbMrpEntities.Configuration.AutoDetectChangesEnabled = false;
                                            if (isDeployedToIntegration)
                                            {
                                                lstTactics.ForEach(tac =>
                                                {
                                                    if (sfdcInstanceId > 0)
                                                        tac.IsSyncSalesForce = true;         // Set SFDC setting to True if Salesforce instance mapped under Tactic's Model.
                                                    if (elqaInstanceId > 0)
                                                        tac.IsSyncEloqua = true;             // Set Eloqua setting to True if Eloqua instance mapped under Tactic's Model.
                                                    if (workfrontInstanceId > 0)
                                                        tac.IsSyncWorkFront = true;          // Set WorkFront setting to True if WorkFront instance mapped under Tactic's Model.
                                                    if (marketoInstanceId > 0)
                                                        tac.IsSyncMarketo = true;
                                                    tac.IsDeployedToIntegration = true;
                                                    objDbMrpEntities.Entry(tac).State = EntityState.Modified;
                                                });
                                            }
                                            else
                                            {
                                                lstTactics.ForEach(tac =>
                                                {
                                                    tac.IsDeployedToIntegration = false;
                                                    tac.IsSyncSalesForce = tac.IsSyncEloqua = tac.IsSyncWorkFront = false;
                                                    objDbMrpEntities.Entry(tac).State = EntityState.Modified;
                                                });
                                            }
                                        }
                                        finally
                                        {
                                            objDbMrpEntities.Configuration.AutoDetectChangesEnabled = true;
                                        }
                                        objDbMrpEntities.SaveChanges();
                                        #endregion
                                    }
                                }
                            }
                        }
                        #endregion

                        //Added By Komal Rawal for #2216  saving of channel and program type
                        if (objModel.IntegrationInstanceMarketoID != null)
                        {
                            SaveMarketoSettings(TacticTypeId, objModel.IntegrationInstanceMarketoID, Enums.FilterLabel.TacticType.ToString(), ProgramType, Channel);
                        }
                    }
                    else
                    {
                        string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    //// Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                        return Json(new { errormsg = strDuplicateMessage });
                    }
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }

            return Json(new { redirect = Url.Action("Tactics") });
        }

        /// <summary>
        /// Added By: Nirav Shah.
        /// Modified By Maninder Singh Wadhva to address TFS Bug#239
        /// Modified by Dharmraj for ticket #592, #593 : Tactic type data model - Tactic screen
        /// Action to Save all checked Tactic data .
        /// </summary>
        /// <param name="ids">comma separated list of selected tactic type id(s)</param>
        /// <param name="rejids">comma separated list of rejected tactic type id(s)</param>
        /// <param name="ModelId">model if</param>
        /// <param name="isModelPublished">isModelPublished flag</param>
        /// <param name="EffectiveDate">effective date of model</param>
        /// <param name="UserId">user id of logged in user</param>  Added by Sohel Pathan on 31/12/2014 for PL ticket #1063
        /// <returns>returns json object</returns>
        [HttpPost]
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]    //// Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult saveAllTactic(string ids, string rejids, int ModelId, bool isModelPublished, string EffectiveDate, Guid userId = new Guid())
        {
            // Get UserId Integer Id from Guid Ticket #2955
            int uId = Common.GetIntegerUserId(userId);
            //// Start - Added by Sohel Pathan on 31/12/2014 for PL ticket #1063
            //// Check cross user login
            if (uId != 0)
            {
                if (!Sessions.User.ID.Equals(uId))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            //// End - Added by Sohel Pathan on 31/12/2014 for PL ticket #1063

            string errorMessage = string.Empty, successMessage = string.Empty;
            try
            {
                TempData["ErrorMessage"] = string.Empty;
                TempData["SuccessMessage"] = string.Empty;
                string Title = string.Empty;
                TacticType objtactic = new TacticType();
                string[] id = ids.Split(',');
                //// added by Dharmraj to distinct id array, 10-7-2014
                id = id.Distinct().ToArray();
                int result;
                string[] rejid = rejids.Split(',');
                //// added by Dharmraj to distinct id array, 10-7-2014
                rejid = rejid.Distinct().ToArray();
                bool msgshow = false;
                Model objModel = objDbMrpEntities.Models.Where(model => model.ModelId == ModelId).FirstOrDefault();
                if (ids == "" && objModel.Status.ToLower() == Convert.ToString(Enums.ModelStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.ModelStatus.Published.ToString())).Value).ToLower())
                {
                    msgshow = true;
                    errorMessage = string.Format(Common.objCached.TacticReqForPublishedModel);
                    TempData["ErrorMessage"] = string.Format(Common.objCached.TacticReqForPublishedModel);
                }
                else
                {
                    string strModelStatus = objModel.Status.ToLower();
                    int sfdcInstanceId = 0, elqaInstanceId = 0, workfrontInstanceId = 0, marketoInstanceId = 0;
                    #region "Get Integration Instance Set for Model"
                    if (!string.IsNullOrEmpty(strModelStatus) && Enums.ModelStatus.Published.ToString().ToLower().Equals(strModelStatus))
                    {
                        if (objModel.IntegrationInstanceId.HasValue)
                            sfdcInstanceId = objModel.IntegrationInstanceId.Value;
                        if (objModel.IntegrationInstanceEloquaId.HasValue)
                            elqaInstanceId = objModel.IntegrationInstanceEloquaId.Value;
                        if (objModel.IntegrationInstanceIdProjMgmt.HasValue)
                            workfrontInstanceId = objModel.IntegrationInstanceIdProjMgmt.Value;
                        if (objModel.IntegrationInstanceMarketoID.HasValue)  //added by Rahul Shah on 17/05/2016 for PL #2189 
                            marketoInstanceId = objModel.IntegrationInstanceMarketoID.Value;
                    }
                    #endregion

                    if (rejid.Length > 0)
                    {
                        #region "Declare local variables"
                        int tacticId;
                        List<int> lstRejTacticTypeIds = new List<int>();
                        List<Plan_Campaign_Program_Tactic> lstAllRejTactics = new List<Plan_Campaign_Program_Tactic>();
                        #endregion
                        #region "Get TacticTypeIds"
                        for (int i = 0; i < rejid.Length; i++)
                        {
                            string[] strArr = rejid[i].Replace("rej", "").Split('_');
                            int.TryParse(strArr[0], out tacticId);
                            if (tacticId != 0)
                                lstRejTacticTypeIds.Add(tacticId);
                        }
                        if (lstRejTacticTypeIds != null && lstRejTacticTypeIds.Count > 0)
                            lstAllRejTactics = GetPlanTacticsByTacticType(objModel.ModelId, lstRejTacticTypeIds);
                        #endregion

                        for (int i = 0; i < rejid.Length; i++)
                        {
                            string[] strArr = rejid[i].Replace("rej", "").Split('_');
                            int.TryParse(strArr[0], out tacticId);
                            if (tacticId != 0)
                            {
                                TacticType rejTacticType = objDbMrpEntities.TacticTypes.Where(tacticType => tacticType.TacticTypeId == tacticId).FirstOrDefault();
                                if (rejTacticType != null && rejTacticType.IsDeployedToModel != false)  ////Modified by Mitesh Vaishnav on 22/07/2014 for PL ticket #612 I had added condition for only deploed model
                                {
                                    rejTacticType.IsDeployedToModel = false;
                                    rejTacticType.IsDeployedToIntegration = false;
                                    rejTacticType.IsDeleted = false;
                                    objDbMrpEntities.TacticTypes.Attach(rejTacticType);
                                    objDbMrpEntities.Entry(rejTacticType).State = EntityState.Modified;

                                    //#2063: Tactic 'Deployed To Integration' not defaulting to on
                                    // Added by Viral on 04/08/2016
                                    #region "Update DeployToIntegration settings for all tactics related to Model & TacticType"

                                    if (!string.IsNullOrEmpty(strModelStatus) && Enums.ModelStatus.Published.ToString().ToLower().Equals(strModelStatus) && lstAllRejTactics != null && lstAllRejTactics.Count > 0)
                                    {
                                        // Get list of tactics related to TacticType.
                                        List<Plan_Campaign_Program_Tactic> lstTactics = lstAllRejTactics.Where(tac => tac.TacticTypeId == rejTacticType.TacticTypeId).ToList();
                                        if (lstTactics != null && lstTactics.Count > 0)
                                        {
                                            lstTactics = lstTactics.Where(tac => tac.IsDeleted == false).ToList();
                                            if (lstTactics != null && lstTactics.Count > 0)
                                            {
                                                #region "Update Tactics IsDeployedToIntegration settings"
                                                try
                                                {
                                                    objDbMrpEntities.Configuration.AutoDetectChangesEnabled = false;
                                                    lstTactics.ForEach(tac =>
                                                    {
                                                        tac.IsDeployedToIntegration = false;
                                                        tac.IsSyncEloqua = tac.IsSyncSalesForce = tac.IsSyncWorkFront = tac.IsSyncMarketo = false;
                                                        objDbMrpEntities.Entry(tac).State = EntityState.Modified;
                                                    });
                                                }
                                                finally
                                                {
                                                    objDbMrpEntities.Configuration.AutoDetectChangesEnabled = true;
                                                }
                                                #endregion
                                            }
                                        }
                                    }
                                    #endregion

                                    result = objDbMrpEntities.SaveChanges();

                                    //// changed by : Nirav Shah on 31 Jan 2013
                                    ////  Bug 19:Model - should not be able to publish a model with no tactics selected */
                                    if (msgshow == false)
                                    {
                                        msgshow = true;
                                        successMessage = string.Format(Common.objCached.ModifiedTactic);
                                        TempData["SuccessMessage"] = string.Format(Common.objCached.ModifiedTactic);
                                    }
                                }
                            }
                        }
                    }

                    if (id.Length > 0)
                    {
                        #region "local variables"
                        int tacticId = 0, tid = 0, intRandomColorNumber = 0;
                        string[] strArr;
                        bool IsDeployToIntegration;
                        TacticType tacticType;
                        string StageType = Enums.StageType.CR.ToString();
                        Model_Stage objStage;
                        MRPEntities dbedit;
                        List<int> lstTacticTypeIds = new List<int>();
                        List<Plan_Campaign_Program_Tactic> lstAllTactics = new List<Plan_Campaign_Program_Tactic>();
                        #endregion
                        #region "Get TacticTypeIds"
                        for (int i = 0; i < id.Length; i++)
                        {
                            strArr = id[i].Replace("rej", "").Split('_');
                            int.TryParse(strArr[0], out tacticId);
                            if (tacticId != 0)
                                lstTacticTypeIds.Add(tacticId);
                        }
                        if (lstTacticTypeIds != null && lstTacticTypeIds.Count > 0)
                            lstAllTactics = GetPlanTacticsByTacticType(objModel.ModelId, lstTacticTypeIds);
                        #endregion
                        for (int i = 0; i < id.Length; i++)
                        {
                            strArr = id[i].Replace("rej", "").Split('_');
                            int.TryParse(strArr[0], out tacticId);
                            if (tacticId != 0)
                            {
                                IsDeployToIntegration = Convert.ToBoolean(strArr[1]);
                                tid = Convert.ToInt32(Convert.ToString(strArr[0]));
                                tacticType = new TacticType();
                                tacticType = objDbMrpEntities.TacticTypes.Where(tacType => tacType.TacticTypeId == tid).FirstOrDefault();
                                objtactic.Title = tacticType.Title;
                                objtactic.Description = tacticType.Description;
                                //// changes done by uday for PL #497 changed projectedmlqs to projectedstagevalue
                                objtactic.ProjectedStageValue = tacticType.ProjectedStageValue;
                                objtactic.ProjectedRevenue = tacticType.ProjectedRevenue;                               
                                objtactic.Abbreviation = tacticType.Abbreviation;
                                objtactic.AssetType = tacticType.AssetType;

                                objStage = new Model_Stage();
                                objStage = objDbMrpEntities.Model_Stage.Where(modelFunnelStage => modelFunnelStage.StageType == StageType && modelFunnelStage.ModelId == ModelId && modelFunnelStage.AllowedTargetStage == true).OrderBy(modelFunnelStage => modelFunnelStage.Stage.Level).Distinct().FirstOrDefault();
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
                                objtactic.StageId = (tacticType.StageId == null) ? objDbMrpEntities.Model_Stage.Where(modelFunnelStage => modelFunnelStage.StageType == StageType && modelFunnelStage.ModelId == ModelId && modelFunnelStage.AllowedTargetStage == true).OrderBy(modelFunnelStage => modelFunnelStage.Stage.Level).Distinct().Select(modelFunnelStage => modelFunnelStage.StageId).FirstOrDefault() : tacticType.StageId;   //// Line uncommented by Sohel Pathan on 16/06/2014 for PL ticket #528.
                                intRandomColorNumber = rnd.Next(Common.ColorcodeList.Count);
                                objtactic.ColorCode = Convert.ToString(Common.ColorcodeList[intRandomColorNumber]);
                                objtactic.CreatedDate = DateTime.Now;
                                objtactic.CreatedBy = Sessions.User.ID;
                                objtactic.ModelId = ModelId;
                                objtactic.PreviousTacticTypeId = tacticType.PreviousTacticTypeId;

                                //// Added by dharmraj for ticket #433 Integration - Model Screen Tactic List
                                objtactic.IsDeployedToIntegration = IsDeployToIntegration;
                                objtactic.IsDeployedToModel = true;
                                objtactic.TacticTypeId = tacticType.TacticTypeId;
                                objtactic.IsDeleted = false;
                                objtactic.WorkFrontTemplateId = tacticType.WorkFrontTemplateId; // Added Brad Gray 07/25/2015 WorkFront Template to Tactic Type mapping - updated 1/7/2016 by Brad Gray PL#1856

                                dbedit = new MRPEntities();
                                dbedit.Entry(objtactic).State = EntityState.Modified;

                                //#2063: Tactic 'Deployed To Integration' not defaulting to on
                                // Added by Viral on 04/08/2016
                                #region "Update DeployToIntegration settings for all tactics related to Model & TacticType"
                                if (!string.IsNullOrEmpty(strModelStatus) && Enums.ModelStatus.Published.ToString().ToLower().Equals(strModelStatus) && lstAllTactics != null && lstAllTactics.Count > 0)
                                {
                                    // Get list of tactics related to TacticType.
                                    List<Plan_Campaign_Program_Tactic> lstTactics = lstAllTactics.Where(tac => tac.TacticTypeId == objtactic.TacticTypeId).ToList();
                                    if (lstTactics != null && lstTactics.Count > 0)
                                    {
                                        lstTactics = lstTactics.Where(tac => tac.IsDeleted == false).ToList();
                                        if (lstTactics != null && lstTactics.Count > 0)
                                        {
                                            #region "Update Tactics IsDeployedToIntegration settings"
                                            try
                                            {
                                                objDbMrpEntities.Configuration.AutoDetectChangesEnabled = false;
                                                lstTactics.ForEach(tac =>
                                                {
                                                    tac.IsDeployedToIntegration = IsDeployToIntegration;
                                                    if (sfdcInstanceId > 0)
                                                        tac.IsSyncSalesForce = true;
                                                    if (workfrontInstanceId > 0)
                                                        tac.IsSyncWorkFront = true;
                                                    if (elqaInstanceId > 0)
                                                        tac.IsSyncEloqua = true;
                                                    if (marketoInstanceId > 0)  //added by Rahul Shah on 17/05/2016 for PL #2189
                                                        tac.IsSyncMarketo = true;
                                                    objDbMrpEntities.Entry(tac).State = EntityState.Modified;
                                                });
                                            }
                                            finally
                                            {
                                                objDbMrpEntities.Configuration.AutoDetectChangesEnabled = true;
                                            }
                                            objDbMrpEntities.SaveChanges();
                                            #endregion
                                        }
                                    }
                                }
                                #endregion

                                result = dbedit.SaveChanges();
                                dbedit.Dispose();
                                //// changed by : Nirav Shah on 31 Jan 2013
                                //// Bug 19:Model - should not be able to publish a model with no tactics selected */
                                if (msgshow == false)
                                {
                                    msgshow = true;
                                    successMessage = string.Format(Common.objCached.ModifiedTactic);
                                    TempData["SuccessMessage"] = string.Format(Common.objCached.ModifiedTactic);
                                }
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
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
                TempData["SuccessMessage"] = string.Empty;
                return Json(new { errorMessage = objException.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
            if (successMessage != string.Empty)
            {
                return Json(new { successMessage }, JsonRequestBehavior.AllowGet);
            }
            else if (errorMessage != string.Empty)
            {
                return Json(new { errorMessage }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
        }

        public List<Plan_Campaign_Program_Tactic> GetPlanTacticsByTacticType(int ModelId, List<int> lstTacticTypeIds)
        {
            List<Plan_Campaign_Program_Tactic> lstPlanTactics = new List<Plan_Campaign_Program_Tactic>();
            try
            {
                lstPlanTactics = (from mdl in objDbMrpEntities.Models
                                  where (mdl.IsDeleted == false) && (mdl.ModelId == ModelId)
                                  join plan in objDbMrpEntities.Plans on mdl.ModelId equals plan.ModelId
                                  join cmpgn in objDbMrpEntities.Plan_Campaign on plan.PlanId equals cmpgn.PlanId
                                  where cmpgn.IsDeleted == false
                                  join prg in objDbMrpEntities.Plan_Campaign_Program on cmpgn.PlanCampaignId equals prg.PlanCampaignId
                                  where prg.IsDeleted == false
                                  join tac in objDbMrpEntities.Plan_Campaign_Program_Tactic on prg.PlanProgramId equals tac.PlanProgramId
                                  where (tac.IsDeleted == false) && (lstTacticTypeIds.Contains(tac.TacticTypeId))
                                  select tac
                                  ).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstPlanTactics;
        }

        #endregion

        #region Integration

        /// <summary>
        /// Added By: Dharmraj Mangukiya
        /// Action method to show model integration screen.
        /// </summary>
        /// <param name="id">model id</param>
        /// <returns>returns Integration view</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]    //// Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult Integration(int id = 0)
        {
            //// Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

            ViewBag.ModelId = id;

            var objModel = objDbMrpEntities.Models.Where(model => model.ModelId == id && model.IsDeleted == false).FirstOrDefault();

            ViewBag.ModelStatus = objModel.Status;
            ViewBag.ModelTitle = objModel.Title;
            var IsBenchmarked = (id == 0) ? true : objDbMrpEntities.Models.Where(model => model.ModelId == id && model.IsDeleted == false).OrderByDescending(model => model.CreatedDate).Select(model => model.IsBenchmarked).FirstOrDefault();
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;
            ViewBag.IsBenchmarked = (IsBenchmarked != null) ? IsBenchmarked : true;
            BaselineModel objBaselineModel = new BaselineModel();

            try
            {
                objBaselineModel = FillInitialData(id);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
            }

            string Title = objBaselineModel.Versions.Where(version => version.IsLatest == true).Select(version => version.Title).FirstOrDefault();

            //// Added By Kalpesh Sharma Functional and code review #560 07-16-2014
            ViewBag.LatestModelID = objBaselineModel.Versions.Where(version => version.IsLatest == true).Select(version => version.ModelId).FirstOrDefault();

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
                //// Added by Sohel Pathan on 07/07/2014 for Internal Review Points to implement custom restriction logic on Business unit.
                ViewBag.IsOwner = objDbMrpEntities.Models.Where(model => model.IsDeleted.Equals(false) && model.ModelId == id && model.CreatedBy == Sessions.User.ID).Any();
            }
            else
            {
                ViewBag.IsOwner = true; //// Added by Sohel Pathan on 07/07/2014 for Internal Review Points to implement custom restriction logic on Business unit.
            }

            return View(objBaselineModel);
        }

        /// <summary>
        /// Added By: Dharmraj mangukiya
        /// Action method to show Integrations List by ModelId.
        /// </summary>
        /// <param name="id">model id</param>
        /// <returns>returns json result object</returns>
        public JsonResult GetIntegrationDatabyid(int id)
        {
            Model objModel = objDbMrpEntities.Models.FirstOrDefault(model => model.ModelId == id);
            int integrationInstanceId = objModel.IntegrationInstanceId == null ? 0 : Convert.ToInt32(objModel.IntegrationInstanceId);
            var lstIntegrationInstance = objDbMrpEntities.IntegrationInstances.Where(integrationInstace => integrationInstace.IsDeleted == false && integrationInstace.IsActive == true && integrationInstace.ClientId == Sessions.User.CID).ToList().Select(integrationInstace => integrationInstace);

            //// Retrieve all Integrations
            var allIntegrationInstance = lstIntegrationInstance.Select(integrationInstace => new
            {
                id = integrationInstace.IntegrationInstanceId,
                clientid = integrationInstace.ClientId,
                provider = integrationInstace.IntegrationType.Title,
                instance = integrationInstace.Instance,
                lastSync = GetFormatedDate(integrationInstace.LastSyncDate),
                target = integrationInstace.IntegrationInstanceId == integrationInstanceId ? true : false
            }).Select(integrationInstace => integrationInstace).Distinct().OrderBy(integrationInstace => integrationInstace.provider);

            return Json(allIntegrationInstance, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Function to return datetime in formatted pattern.
        /// </summary>
        /// <param name="objDate">date object</param>
        /// <returns>returns date in string form</returns>
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
        /// <param name="modelId">model id</param>
        /// <param name="integrationId">integration instance id</param>
        /// <param name="UserId">legged in user id</param>
        /// <returns>returns json result object</returns>
        public JsonResult SaveAllIntegration(int modelId, int integrationId, Guid userId = new Guid())
        {
            // Get UserId Integer Id from Guid Ticket #2955
            int uId = Common.GetIntegerUserId(userId);
            //// Cross user login check
            if (uId != 0)
            {
                if (!Sessions.User.ID.Equals(uId))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            bool returnValue = false;
            string message = string.Empty;

            try
            {
                List<ModelVersion> lstVersions = GetModelVersions(modelId);

                foreach (var version in lstVersions)
                {
                    Model objModel = objDbMrpEntities.Models.FirstOrDefault(model => model.ModelId == version.ModelId && model.ClientId == Sessions.User.CID);

                    if (objModel.IntegrationInstanceId == integrationId)
                    {
                        objModel.IntegrationInstanceId = null;
                    }
                    else
                    {
                        objModel.IntegrationInstanceId = integrationId;
                    }

                    objModel.ModifiedBy = Sessions.User.ID;
                    objModel.ModifiedDate = DateTime.Now;

                    objDbMrpEntities.Entry(objModel).State = EntityState.Modified;
                    objDbMrpEntities.SaveChanges();
                }

                Common.DeleteIntegrationInstance(integrationId, false, modelId);

                message = Common.objCached.ModelIntegrationSaveSuccess;

                returnValue = true;
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
                message = Common.objCached.ErrorOccured;
            }

            var objResult = new { returnValue = returnValue, message = message };

            return Json(objResult, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ChangeLog

        /// <summary>
        /// Action to load change log of activities
        /// </summary>
        /// <param name="objectId">entity id</param>
        /// <returns>returns ChangeLog partial view</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]    //// Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult LoadChangeLog(int objectId)
        {
            List<ChangeLog_ViewModel> lstChangelog = new List<ChangeLog_ViewModel>();
            try
            {
                lstChangelog = Common.GetChangeLogs(Enums.ChangeLog_TableName.Model.ToString(), objectId);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
            }
            return PartialView("_ChangeLog", lstChangelog);
        }

        #endregion

        #region Model Publish

        /// <summary>
        /// Added By: Dharmraj Mangukiya
        /// Action to publish Model.
        /// </summary>
        /// <param name="ModelId">model if</param>
        /// <param name="EffectiveDate">effective date of model</param>
        /// <returns>returns json result object</returns>
        public JsonResult ModelPublish(int ModelId, string EffectiveDate)
        {
            bool isTacticTypeExist = false;
            string errorMessage = string.Empty, successMessage = string.Empty;
            Model objModel = objDbMrpEntities.Models.Where(model => model.ModelId == ModelId).FirstOrDefault();
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
        /// <param name="modelId">model Id.</param>
        /// <param name="effectiveDate">effective date of model</param>
        /// <param name="isTacticTypeExist">tactic type exist flag(out parameter)</param>
        /// <returns>Flag to indicate whether model published sucessfully</returns>
        public bool PublishModel(int modelId, string effectiveDate, out bool isTacticTypeExist)
        {
            isTacticTypeExist = false;
            int cntSelectedTactic = objDbMrpEntities.TacticTypes.Where(TacticTypes => TacticTypes.ModelId == modelId && TacticTypes.IsDeployedToModel == true).ToList().Count;
            Model objModel = objDbMrpEntities.Models.Where(model => model.ModelId == modelId).FirstOrDefault();
            bool isPublish = false;
            try
            {
                if (cntSelectedTactic > 0)
                {
                    isTacticTypeExist = true;
                    if (objModel != null)
                    {
                        objModel.Status = Enums.ModelStatusValues.Single(modelStatus => modelStatus.Key.Equals(Enums.ModelStatus.Published.ToString())).Value;
                        if (!string.IsNullOrEmpty(effectiveDate))
                        {
                            objModel.EffectiveDate = DateTime.Parse(effectiveDate);
                        }
                        else
                        {
                            objModel.EffectiveDate = System.DateTime.Now;
                        }
                        objDbMrpEntities.Models.Attach(objModel);
                        objDbMrpEntities.Entry(objModel).State = EntityState.Modified;

                        Common.InsertChangeLog(modelId, 0, modelId, objModel.Title, Enums.ChangeLog_ComponentType.model, Enums.ChangeLog_TableName.Model, Enums.ChangeLog_Actions.published);

                        int result = objDbMrpEntities.SaveChanges();
                        if (result > 0)
                        {
                            isPublish = true;
                            // Add By Nishant Sheth
                            // Desc : #2225 performance issue with publishing model
                            objDbMrpEntities.PublishModel(modelId, Sessions.User.ID);
                            Sessions.PlanUserSavedViews = null;
                            // End By Nishant Sheth
                            #endregion
                        }
                    }
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }

            return isPublish;
        }

        #region Common Functions for Model

        /// <summary>
        /// Function to check parent published model
        /// </summary>
        /// <param name="modelId">model id</param>
        /// <returns>returns flag based on model has parent published model</returns>
        public bool chekckParentPublishModel(int modelId)
        {
            bool isParentPublishedModel = true;
            Model objModel = objDbMrpEntities.Models.Where(model => model.ModelId == modelId && model.ClientId == Sessions.User.CID && model.ParentModelId == null).FirstOrDefault();
            if (objModel != null)
            {
                isParentPublishedModel = false;
            }
            return isParentPublishedModel;
        }

        #endregion

        #region Duplicate/Clone Model

        /// <summary>
        /// Duplicate/Clone an existing model.
        /// </summary>
        /// <AddedBy>Sohel Pathan</AddedBy>
        /// <CreatedDate>06.06.2014</CreatedDate>
        /// <param name="modelId">modelId of the Model to be cloned.</param>
        /// <param name="title">title of model</param>
        /// <returns>Success or Error message in JSON result format</returns>
        //// Modified By : Kalpesh Sharma #560 Method to Specify a Name for Cloned Model
        [HttpPost]
        public JsonResult DuplicateModel(int modelId, string title)
        {
            if (modelId > 0)
            {
                int NewModelID = 0;
                try
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {
                            //// Added By : Kalpesh Sharma #560 Method to Specify a Name for Cloned Model
                            OtherModelEntries(modelId, false, title, false, ref NewModelID);


                            #region Clone Model_Funnel_Stage table entries

                            var oldModel_Funnel_Stage = mrp.Model_Stage.Where(modelFunnelStage => modelFunnelStage.ModelId == modelId).ToList();
                            if (oldModel_Funnel_Stage != null)
                            {
                                if (oldModel_Funnel_Stage.Count > 0)
                                {
                                    foreach (var objModel_Funnel_Stage in oldModel_Funnel_Stage)
                                    {
                                        Model_Stage newModel_Funnel_Stage = new Model_Stage();

                                        newModel_Funnel_Stage = objModel_Funnel_Stage;
                                        newModel_Funnel_Stage.CreatedDate = DateTime.Now;
                                        newModel_Funnel_Stage.CreatedBy = Sessions.User.ID;
                                        newModel_Funnel_Stage.ModifiedBy = 0;
                                        newModel_Funnel_Stage.ModifiedDate = null;
                                        newModel_Funnel_Stage.ModelId = NewModelID;
                                        mrp.Model_Stage.Add(newModel_Funnel_Stage);
                                    }

                                    //// Added By Kalpesh Sharma Functional and code review #560 07-16-2014   
                                    mrp.SaveChanges();

                                }
                            }
                            #endregion

                            scope.Complete();
                        }
                    }

                    TempData["SuccessMessage"] = Common.objCached.ModelDuplicated;
                    return Json(new { status = 1, msg = Common.objCached.ModelDuplicated }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception objException)
                {
                    Common.objCached.ModelDuplicated = string.Empty;
                    ErrorSignal.FromCurrentContext().Raise(objException);
                    return Json(new { status = 0, msg = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                Common.objCached.ModelDuplicated = string.Empty;
                return Json(new { status = 0, msg = "Invalid model Id." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Added By : Kalpesh Sharma #560 Method to Specify a Name for Cloned Model
        /// When Duplicate model popup will be open at that time we have to set default value in textbox (Old Model Name + TImestamp value) 
        /// </summary>
        /// <param name="modelId">model id</param>
        /// <returns>returns josn result object</returns>
        [HttpPost]
        public JsonResult GetDefaultDuplicateModelName(int modelId)
        {
            string result = string.Empty;
            int StatusOpt = 0;
            string ModelName = string.Empty;

            if (modelId > 0)
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    var oldModel = mrp.Models.Where(model => model.ModelId == modelId && model.IsDeleted.Equals(false)).FirstOrDefault();
                    if (oldModel != null)
                    {
                        //// Added By Kalpesh Sharma #560: Method to Specify a Name for Cloned Model 07-11-2014
                        result = oldModel.Title + " " + DateTime.Now.ToString("MMddyy_HHmmss");
                        ModelName = oldModel.Title;
                        StatusOpt = 1;
                    }
                }
            }
            else
            {
                StatusOpt = 0;
                result = "Invalid model id";
            }
            return Json(new { status = StatusOpt, msg = result, name = ModelName }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By Kalpesh Sharma #560: Method to Specify a Name for Cloned Model 07-11-2014
        /// When Duplicate model popup will be open at that time we have to set default value in textbox (Old Model Name + TImestamp value) 
        /// </summary>
        /// <param name="title">model title</param>
        /// <param name="modelId">model id</param>
        /// <returns>returns json object</returns>
        [HttpPost]
        public JsonResult CheckDuplicateModelTitleByID(string title, int modelId)
        {
            string result = string.Empty;

            if (modelId > 0)
            {
                var clientId = objDbMrpEntities.Models.Where(model => model.ModelId == modelId && model.IsDeleted == false).OrderByDescending(model => model.CreatedDate).Select(model => model.ClientId).FirstOrDefault();

                if (!string.IsNullOrEmpty(Convert.ToString(clientId)))
                {
                    var obj = objDbMrpEntities.Models.Where(model => model.IsDeleted == false && model.ClientId == clientId && model.Title.Trim().ToLower() == title.Trim().ToLower()).FirstOrDefault();
                    if (obj == null)
                    {
                        return Json("notexist", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("exist", JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json("exist", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By : Kalpesh Sharma #560 Method to Specify a Name for Cloned Model 
        /// Clone the other model tables based upon old Model , This function is replace the SaveModelInboundOutboundEvent Store Procedure.  
        /// </summary>
        /// <param name="OldModelID">model id of old model</param>
        /// <param name="IsVersion">IsVersion flag</param>
        /// <param name="Title">title of model</param>
        /// <param name="IsBenchmarked">IsBenchmarked flag</param>
        /// <param name="newModelId">newModelId (out parameter)</param>
        public void OtherModelEntries(int OldModelID, bool IsVersion, string Title, bool IsBenchmarked, ref int newModelId)
        {
            using (MRPEntities objMrpEntities = new MRPEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    #region Clone Model table entries

                    Model newModel = new Model();
                    List<Model> tblModels = objMrpEntities.Models.Where(model => model.IsDeleted.Equals(false)).ToList();
                    var oldModel = tblModels.Where(model => model.ModelId == OldModelID).FirstOrDefault();
                    if (!IsVersion)
                    {
                        //  var oldModel = objMrpEntities.Models.Where(model => model.ModelId == OldModelID && model.IsDeleted.Equals(false)).FirstOrDefault();
                        if (oldModel != null)
                        {
                            newModel = oldModel;
                            newModel.Version = "1.0";
                            newModel.ParentModelId = null;
                            //// Added By : Kalpesh Sharma #694 :Clone Model with Integration Tab
                            newModel.IntegrationInstanceId = null;
                            newModel.IntegrationInstanceIdCW = null;
                            newModel.IntegrationInstanceIdINQ = null;
                            newModel.IntegrationInstanceIdMQL = null;
                            newModel.IntegrationInstanceIdProjMgmt = null; //Added Brad Gray 23 July 2015 PL#1448
                            newModel.IntegrationInstanceEloquaId = null;
                            newModel.IntegrationInstanceMarketoID = null; //Added By komal Rawal for #2190
                            newModel.EffectiveDate = null;
                            newModel.Model1 = null;
                            newModel.Model2 = null;
                        }
                    }
                    else
                    {
                        newModel.ParentModelId = OldModelID;
                        newModel.IsActive = true;
                        newModel.IsDeleted = false;
                        newModel.Year = DateTime.Now.Year;
                        newModel.IsBenchmarked = IsBenchmarked;
                        //// Added by Mitesh Vaishnav for PL ticket #659 


                        newModel.IntegrationInstanceId = oldModel.IntegrationInstanceId;
                        newModel.IntegrationInstanceIdCW = oldModel.IntegrationInstanceIdCW;
                        newModel.IntegrationInstanceIdINQ = oldModel.IntegrationInstanceIdINQ;
                        newModel.IntegrationInstanceIdMQL = oldModel.IntegrationInstanceIdMQL;
                        newModel.IntegrationInstanceIdProjMgmt = oldModel.IntegrationInstanceIdProjMgmt; //Added Brad Gray 23 July 2015 PL#1448
                        newModel.IntegrationInstanceEloquaId = oldModel.IntegrationInstanceEloquaId;
                        newModel.IntegrationInstanceMarketoID = oldModel.IntegrationInstanceMarketoID; //Added By komal Rawal for #2190
                        newModel.ClientId = oldModel.ClientId;
                        newModel.AverageDealSize = oldModel.AverageDealSize;
                        ////End :Added by Mitesh Vaishnav for PL ticket #659 
                        //// title condition added by uday for review point on 5-6-2014 bcoz version clashes when two users are creating version of same buisiness unit.

                        var version = tblModels.Where(model => model.ClientId.Equals(Sessions.User.CID) && model.Title == Title).OrderByDescending(model => model.CreatedDate).Select(model => model.Version).FirstOrDefault();
                        if (version != null && version != "")
                        {
                            newModel.Version = Convert.ToString((Convert.ToDecimal(version) + (decimal)0.1)); //Modified By Komal Rawal for #1245
                        }
                        else
                        {
                            newModel.Version = "1.0";
                        }
                    }

                    newModel.Title = Title;
                    newModel.Status = Enums.ModelStatusValues.FirstOrDefault(modelStatus => modelStatus.Key.Equals(Enums.ModelStatus.Draft.ToString())).Value;
                    newModel.CreatedDate = DateTime.Now;
                    newModel.CreatedBy = Sessions.User.ID;
                    newModel.ModifiedBy = 0;
                    newModel.ModifiedDate = null;
                    objMrpEntities.Models.Add(newModel);
                    objMrpEntities.SaveChanges();
                    newModelId = newModel.ModelId;
                    #endregion

                    //// Added By : Kalpesh Sharma PL #697 Default Line item type
                    //// Added By : #827 New Version of Model does not Add Entry in LineItemType table
                    LineItemType objLineItemType = objDbMrpEntities.LineItemTypes.Where(lineItemType => lineItemType.ModelId == newModel.ModelId).FirstOrDefault();
                    if (objLineItemType == null)
                    {
                        objLineItemType = new LineItemType();
                        objLineItemType.ModelId = newModelId;
                        objLineItemType.Title = Enums.LineItemTypes.None.ToString();
                        objLineItemType.Description = Enums.LineItemTypes.None.ToString();
                        objLineItemType.IsDeleted = false;
                        objLineItemType.CreatedDate = DateTime.Now;
                        objLineItemType.CreatedBy = Sessions.User.ID;
                        objDbMrpEntities.LineItemTypes.Add(objLineItemType);
                        objDbMrpEntities.SaveChanges();
                    }

                    #region Clone TacticTypes table entries

                    var oldTacticTypes = objMrpEntities.TacticTypes.Where(tacticType => tacticType.ModelId == OldModelID && (tacticType.IsDeleted == null ? false : tacticType.IsDeleted) == false).ToList();

                    if (oldTacticTypes != null)
                    {
                        if (oldTacticTypes.Count > 0)
                        {
                            TacticType newTacticTypes;
                            MarketoEntityValueMapping NewTacticMapping;
                            MarketoEntityValueMapping OldTacticMapping;
                            foreach (var tacticType in oldTacticTypes)
                            {
                                newTacticTypes = new TacticType();
                                newTacticTypes = tacticType;
                                newTacticTypes.ModelId = newModelId;
                                //// Start - Added by Sohel Pathan on 20/08/2014 for PL ticket #713
                                if (IsVersion)
                                {
                                    newTacticTypes.PreviousTacticTypeId = tacticType.TacticTypeId;
                                }
                                else
                                {
                                    newTacticTypes.PreviousTacticTypeId = null;
                                }
                                //// End - Added by Sohel Pathan on 20/08/2014 for PL ticket #713
                                newTacticTypes.CreatedDate = DateTime.Now;
                                newTacticTypes.CreatedBy = Sessions.User.ID;
                                newTacticTypes.ModifiedBy = 0;
                                newTacticTypes.ModifiedDate = null;
                                objMrpEntities.TacticTypes.Add(newTacticTypes);


                            }
                            objMrpEntities.SaveChanges();  //// Shifted by Sohel Pathan on 20/08/2014 for PL ticket #713 from foreach loop to outside.

                            if (IsVersion)
                            {
                                //Modified By Komal rawal for #2190 to maintain program type and channel of previous Tactic type when  creates new version of model
                                List<MarketoEntityValueMapping> PreviousMapping = objDbMrpEntities.MarketoEntityValueMappings.Where(inst => inst.IntegrationInstanceId == oldModel.IntegrationInstanceMarketoID).ToList();
                                var OldMappedTacticTypeIds = PreviousMapping.Select(id => id.EntityID).ToList();
                                var PreviousTacticTypes = oldTacticTypes.Where(ids => OldMappedTacticTypeIds.Contains(ids.PreviousTacticTypeId)).Select(list => list).ToList();

                                foreach (var TacticType in PreviousTacticTypes)
                                {

                                    NewTacticMapping = new MarketoEntityValueMapping();
                                    OldTacticMapping = new MarketoEntityValueMapping();
                                    OldTacticMapping = PreviousMapping.Where(id => id.EntityID == TacticType.PreviousTacticTypeId).FirstOrDefault();
                                    NewTacticMapping = OldTacticMapping;
                                    NewTacticMapping.EntityID = TacticType.TacticTypeId; //New TacticType id
                                    objDbMrpEntities.Entry(NewTacticMapping).State = EntityState.Added; ;

                                }

                                objMrpEntities.SaveChanges();
                                //End
                            }
                        }
                    }
                    #endregion


                    scope.Complete();
                }
            }
        }

        #endregion

        #region New Integration

        /// <summary>
        /// Added by Mitesh Vaishnav for PL ticket #659 
        /// action method for save integration instance of model
        /// </summary>
        /// <param name="id">model id</param>
        /// <param name="objBaselineModel">BaselineModel object</param>
        /// <param name="IsIntegrationChanged">IsIntegrationChanged flag</param>
        /// <returns>returns json result object</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]
        public JsonResult SaveIntegration(int id, BaselineModel objBaselineModel, bool IsIntegrationChanged = false, bool IsIntegrationEloquaChanged = false, bool IsIntegrationMarketoChanged = false)
        {
            //// set values of objDbMrpEntities.Model object as per posted values and update objDbMrpEntities.Model
            string message = string.Empty;
            // using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    var objModel = objDbMrpEntities.Models.Where(model => model.ModelId == id && model.IsDeleted == false).FirstOrDefault();
                    if (IsIntegrationChanged)
                    {
                        int integrationId = Convert.ToInt32(objModel.IntegrationInstanceId);
                        Common.DeleteIntegrationInstance(integrationId, false, id);
                    }
                    if (IsIntegrationEloquaChanged)
                    {
                        int integrationId = Convert.ToInt32(objModel.IntegrationInstanceEloquaId);
                        Common.DeleteIntegrationInstance(integrationId, false, id, true);
                    }
                    if (IsIntegrationMarketoChanged) //Added by Komal Rawal for PL#2190
                    {
                        int integrationId = Convert.ToInt32(objModel.IntegrationInstanceMarketoID);
                        Common.DeleteIntegrationInstance(integrationId, false, id, false, true);
                    }
                    if (objModel != null)
                    {
                        objModel.IntegrationInstanceId = objBaselineModel.IntegrationInstanceId;
                        objModel.IntegrationInstanceIdCW = objBaselineModel.IntegrationInstanceIdCW;
                        objModel.IntegrationInstanceIdINQ = objBaselineModel.IntegrationInstanceIdINQ;
                        objModel.IntegrationInstanceIdMQL = objBaselineModel.IntegrationInstanceIdMQL;
                        objModel.IntegrationInstanceIdProjMgmt = objBaselineModel.IntegrationInstanceIdProjMgmt; //added Brad Gray 22 July 2015 for PL#1448
                        objModel.IntegrationInstanceEloquaId = objBaselineModel.IntegrationInstanceEloquaId; //added Bhavesh Dobariya #1534
                        objModel.IntegrationInstanceMarketoID = objBaselineModel.IntegrationInstanceMarketoID == 0 ? null : objBaselineModel.IntegrationInstanceMarketoID; //added Komal Rawal #2190
                        objModel.ModifiedBy = Sessions.User.ID;
                        objModel.ModifiedDate = DateTime.Now;
                        objDbMrpEntities.Entry(objModel).State = EntityState.Modified;
                        int result = objDbMrpEntities.SaveChanges();
                        if (result > 0)
                        {
                            ViewBag.ModelId = id;
                            ViewBag.ModelStatus = objModel.Status;
                            ViewBag.ModelTitle = objModel.Title;
                            message = Common.objCached.ModelIntegrationSaveSuccess;
                            TempData["SuccessMessageIntegration"] = Common.objCached.IntegrationSelectionSaved;
                        }
                    }
                }
                catch (Exception objException)
                {
                    ErrorSignal.FromCurrentContext().Raise(objException);
                    message = Common.objCached.ErrorOccured;
                    TempData["ErrorMessageIntegration"] = message;
                }

                // scope.Complete();
            }

            return Json(new { status = false, Id = id }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added by Mitesh Vaishnav for PL ticket #659 
        /// view for list : Integration instances of model
        /// </summary>
        /// <param name="id">model id</param>
        /// <returns>returns IntegrationOverview with list of Integration instances</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]
        [HttpPost]
        public ActionResult IntegrationOverview(int id = 0)
        {
            ViewBag.ModelId = id;
            var objModel = objDbMrpEntities.Models.FirstOrDefault(model => model.ModelId == id && model.IsDeleted == false);
            ViewBag.ModelStatus = objModel.Status;
            ViewBag.ModelTitle = objModel.Title;
            var IsBenchmarked = (id == 0) ? true : objDbMrpEntities.Models.Where(model => model.ModelId == id && model.IsDeleted == false).OrderByDescending(model => model.CreatedDate).Select(model => model.IsBenchmarked).FirstOrDefault();
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;
            ViewBag.LatestModelID = id;
            ViewBag.ModelPublishEdit = Common.objCached.ModelPublishEdit;
            ViewBag.ModelPublishCreateNew = Common.objCached.ModelPublishCreateNew;
            ViewBag.ModelPublishComfirmation = Common.objCached.ModelPublishComfirmation;
            //// Set flag for  existing user is owner or not
            ViewBag.Flag = false;
            if (id != 0)
            {
                ViewBag.Flag = chekckParentPublishModel(id);
                ViewBag.IsOwner = objDbMrpEntities.Models.Where(model => model.IsDeleted.Equals(false) && model.ModelId == id && model.CreatedBy == Sessions.User.ID).Any();  //// Added by Sohel Pathan on 07/07/2014 for Internal Review Points to implement custom restriction logic on Business unit.
            }
            else
            {
                ViewBag.IsOwner = true;
            }

            List<IntegrationSelectionModel> lstIntegrationOverview = new List<IntegrationSelectionModel>();
            List<int?> ModelInstances = new List<int?>() { objModel.IntegrationInstanceId, objModel.IntegrationInstanceIdCW, objModel.IntegrationInstanceIdINQ, objModel.IntegrationInstanceIdMQL, objModel.IntegrationInstanceIdProjMgmt, objModel.IntegrationInstanceEloquaId, objModel.IntegrationInstanceMarketoID }; //Modified by Komal Rawal for PL#2190

            //// creating List of model integration instance 
            var lstInstance = (from integrationInstance in objDbMrpEntities.IntegrationInstances
                               where ModelInstances.Contains(integrationInstance.IntegrationInstanceId)
                               select integrationInstance).ToList();
            foreach (var key in Enums.IntegrationActivity.Keys)
            {
                IntegrationSelectionModel objIntSelection = new IntegrationSelectionModel();
                if (key.ToString() == "IntegrationInstanceId")
                {
                    var objInstance = lstInstance.Where(instance => instance.IntegrationInstanceId == objModel.IntegrationInstanceId).FirstOrDefault();
                    objIntSelection.Setup = Enums.IntegrationActivity[key].ToString();
                    if (objInstance != null)
                    {
                        objIntSelection.Instance = objInstance.Instance;
                        objIntSelection.IntegrationType = objInstance.IntegrationType.Title != null ? objInstance.IntegrationType.Title : Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = objInstance.LastSyncDate != null ? Convert.ToDateTime(objInstance.LastSyncDate).ToString(Common.DateFormatForModelIntegrationLastSync) : "---";

                    }
                    else
                    {
                        objIntSelection.Instance = Common.TextForModelIntegrationInstanceNull;
                        objIntSelection.IntegrationType = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                    }
                    lstIntegrationOverview.Add(objIntSelection);
                }
                else if (key.ToString() == "IntegrationInstanceEloquaId")
                {
                    var objInstance = lstInstance.Where(instance => instance.IntegrationInstanceId == objModel.IntegrationInstanceEloquaId).FirstOrDefault();
                    objIntSelection.Setup = Enums.IntegrationActivity[key].ToString();
                    if (objInstance != null)
                    {
                        objIntSelection.Instance = objInstance.Instance;
                        objIntSelection.IntegrationType = objInstance.IntegrationType.Title != null ? objInstance.IntegrationType.Title : Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = objInstance.LastSyncDate != null ? Convert.ToDateTime(objInstance.LastSyncDate).ToString(Common.DateFormatForModelIntegrationLastSync) : "---";

                    }
                    else
                    {
                        objIntSelection.Instance = Common.TextForModelIntegrationInstanceNull;
                        objIntSelection.IntegrationType = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                    }
                    lstIntegrationOverview.Add(objIntSelection);
                }
                else if (key.ToString() == "IntegrationInstanceMarketoId") //Added by Komal Rawal for PL#2190
                {
                    var objInstance = lstInstance.Where(instance => instance.IntegrationInstanceId == objModel.IntegrationInstanceMarketoID).FirstOrDefault();
                    objIntSelection.Setup = Enums.IntegrationActivity[key].ToString();
                    if (objInstance != null)
                    {
                        objIntSelection.Instance = objInstance.Instance;
                        objIntSelection.IntegrationType = objInstance.IntegrationType.Title != null ? objInstance.IntegrationType.Title : Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = objInstance.LastSyncDate != null ? Convert.ToDateTime(objInstance.LastSyncDate).ToString(Common.DateFormatForModelIntegrationLastSync) : "---";

                    }
                    else
                    {
                        objIntSelection.Instance = Common.TextForModelIntegrationInstanceNull;
                        objIntSelection.IntegrationType = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                    }
                    lstIntegrationOverview.Add(objIntSelection);

                }
                else if (key.ToString() == "IntegrationInstanceIdINQ")
                {
                    var objInstance = lstInstance.Where(instance => instance.IntegrationInstanceId == objModel.IntegrationInstanceIdINQ).FirstOrDefault();
                    objIntSelection.Setup = Enums.IntegrationActivity[key].ToString();
                    if (objInstance != null)
                    {
                        objIntSelection.Instance = objInstance.Instance;
                        objIntSelection.IntegrationType = objInstance.IntegrationType.Title != null ? objInstance.IntegrationType.Title : Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = objIntSelection.LastSync = objInstance.LastSyncDate != null ? Convert.ToDateTime(objInstance.LastSyncDate).ToString(Common.DateFormatForModelIntegrationLastSync) : "---";
                    }
                    else
                    {
                        objIntSelection.Instance = Common.TextForModelIntegrationInstanceNull;
                        objIntSelection.IntegrationType = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                    }
                    lstIntegrationOverview.Add(objIntSelection);
                }
                else if (key.ToString() == "IntegrationInstanceIdMQL")
                {
                    var objInstance = lstInstance.Where(instance => instance.IntegrationInstanceId == objModel.IntegrationInstanceIdMQL).FirstOrDefault();
                    objIntSelection.Setup = Enums.IntegrationActivity[key].ToString();
                    if (objInstance != null)
                    {
                        objIntSelection.Instance = objInstance.Instance;
                        objIntSelection.IntegrationType = objInstance.IntegrationType.Title != null ? objInstance.IntegrationType.Title : Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = objIntSelection.LastSync = objInstance.LastSyncDate != null ? Convert.ToDateTime(objInstance.LastSyncDate).ToString(Common.DateFormatForModelIntegrationLastSync) : "---";
                    }
                    else
                    {
                        objIntSelection.Instance = Common.TextForModelIntegrationInstanceNull;
                        objIntSelection.IntegrationType = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                    }
                    lstIntegrationOverview.Add(objIntSelection);
                }
                else if (key.ToString() == "IntegrationInstanceIdCW")
                {
                    var objInstance = lstInstance.Where(instance => instance.IntegrationInstanceId == objModel.IntegrationInstanceIdCW).FirstOrDefault();
                    objIntSelection.Setup = Enums.IntegrationActivity[key].ToString();
                    if (objInstance != null)
                    {
                        objIntSelection.Instance = objInstance.Instance;
                        objIntSelection.IntegrationType = objInstance.IntegrationType.Title != null ? objInstance.IntegrationType.Title : Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = objIntSelection.LastSync = objInstance.LastSyncDate != null ? Convert.ToDateTime(objInstance.LastSyncDate).ToString(Common.DateFormatForModelIntegrationLastSync) : "---";
                    }
                    else
                    {
                        objIntSelection.Instance = Common.TextForModelIntegrationInstanceNull;
                        objIntSelection.IntegrationType = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                    }
                    lstIntegrationOverview.Add(objIntSelection);
                }
                else if (key.ToString() == "IntegrationInstanceIdProjMgmt") //added by Brad Gray for PL#1448
                {
                    var objInstance = lstInstance.Where(instance => instance.IntegrationInstanceId == objModel.IntegrationInstanceIdProjMgmt).FirstOrDefault();
                    objIntSelection.Setup = Enums.IntegrationActivity[key].ToString();
                    if (objInstance != null)
                    {
                        objIntSelection.Instance = objInstance.Instance;
                        objIntSelection.IntegrationType = objInstance.IntegrationType.Title != null ? objInstance.IntegrationType.Title : Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = objIntSelection.LastSync = objInstance.LastSyncDate != null ? Convert.ToDateTime(objInstance.LastSyncDate).ToString(Common.DateFormatForModelIntegrationLastSync) : "---";
                    }
                    else
                    {
                        objIntSelection.Instance = Common.TextForModelIntegrationInstanceNull;
                        objIntSelection.IntegrationType = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                        objIntSelection.LastSync = Common.TextForModelIntegrationInstanceTypeOrLastSyncNull;
                    }
                    lstIntegrationOverview.Add(objIntSelection);
                }
            }

            ViewBag.IsAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ModelCreateEdit);

            return View(lstIntegrationOverview);
        }

        /// <summary>
        /// Added by Mitesh Vaishnav for PL ticket #659 
        /// view for Integration instance selection 
        /// </summary>
        /// <param name="id">model id</param>
        /// <returns>returns IntegrationSelection view</returns>
        [AuthorizeUser(Enums.ApplicationActivity.ModelCreateEdit)]
        [HttpPost]
        public ActionResult IntegrationSelection(int id = 0)
        {
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.ModelId = id;
            var objModel = objDbMrpEntities.Models.SingleOrDefault(model => model.ModelId == id && model.IsDeleted == false);
            ViewBag.ModelStatus = objModel.Status;
            ViewBag.ModelTitle = objModel.Title;
            ViewBag.ActiveMenu = Enums.ActiveMenu.Model;

            BaselineModel objBaselineModel = new BaselineModel();
            try
            {
                objBaselineModel = FillInitialData(id);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
            }

            ViewBag.LatestModelID = objBaselineModel.Versions.Where(version => version.IsLatest == true).Select(version => version.ModelId).FirstOrDefault();
            objBaselineModel.IntegrationInstanceId = objModel.IntegrationInstanceId;
            objBaselineModel.IntegrationInstanceIdCW = objModel.IntegrationInstanceIdCW;
            objBaselineModel.IntegrationInstanceIdINQ = objModel.IntegrationInstanceIdINQ;
            objBaselineModel.IntegrationInstanceIdMQL = objModel.IntegrationInstanceIdMQL;
            objBaselineModel.IntegrationInstanceIdProjMgmt = objModel.IntegrationInstanceIdProjMgmt; //Added Brad Gray 23 July 2015 PL#1448
            objBaselineModel.IntegrationInstanceEloquaId = objModel.IntegrationInstanceEloquaId; //Added Bhavesh #1534 27aug 2015
            objBaselineModel.IntegrationInstanceMarketoID = objModel.IntegrationInstanceMarketoID;//Added by Komal #2190 12May 2016
            ViewBag.ModelPublishEdit = Common.objCached.ModelPublishEdit;
            ViewBag.ModelPublishCreateNew = Common.objCached.ModelPublishCreateNew;
            ViewBag.ModelPublishComfirmation = Common.objCached.ModelPublishComfirmation;
            //// Set flag for  existing user is owner or not
            ViewBag.Flag = false;
            if (id != 0)
            {
                ViewBag.Flag = chekckParentPublishModel(id);
                ViewBag.IsOwner = objDbMrpEntities.Models.Where(model => model.IsDeleted.Equals(false) && model.ModelId == id && model.CreatedBy == Sessions.User.ID).Any();
            }
            else
            {
                ViewBag.IsOwner = true;
            }

            //// List of integration instance of model
            //Modified by Rahul Shah on 26/02/2016 for PL #2017 
            List<lstInstance> lstInstance = objDbMrpEntities.IntegrationInstances.Where(instance => instance.ClientId == Sessions.User.CID && instance.IsDeleted == false)
                .Select(instance => new lstInstance
                {
                    InstanceName = instance.Instance,
                    InstanceId = instance.IntegrationInstanceId,
                    Type = instance.IntegrationType.Title,
                    Code = instance.IntegrationType.Code
                }).OrderBy(ins => ins.InstanceName).ToList();


            ViewData["IntegrationInstances"] = lstInstance;

            string insType = Enums.IntegrationInstanceType.Salesforce.ToString();
            string elqType = Enums.IntegrationInstanceType.Eloqua.ToString();
            string workfrontType = Enums.IntegrationInstanceType.WorkFront.ToString(); //Added by Brad Gray for PL#1448
            string MarketoType = Enums.IntegrationInstanceType.Marketo.ToString(); //Added by Komal Rawal for PL#2190
            ViewData["IntegrationInstancesSalesforce"] = lstInstance.Where(instance => instance.Code == insType);
            ViewData["IntegrationInstancesProjMgmt"] = lstInstance.Where(instance => instance.Code == workfrontType); //Added by Brad Gray for PL#1448
            ViewData["IntegrationInstancesWithoutProjMgmt"] = lstInstance.Where(instance => instance.Code != workfrontType && instance.Code != MarketoType); //Added by Brad Gray for PL#1448
            ViewData["IntegrationInstancesEloqua"] = lstInstance.Where(instance => instance.Code == elqType);
            ViewData["IntegrationInstancesMarketo"] = lstInstance.Where(instance => instance.Code == MarketoType);//Added by Komal Rawal for PL#2190

            #region "Filtered MQL Eloqua Integration Instances based on Client Integration Permisssion"
            int clientId = Sessions.User.CID;
            string strPermissionCode_MQL = Enums.ClientIntegrationPermissionCode.MQL.ToString();
            //Modified by Rahul Shah on 26/02/2016 for PL #2017. to bind mql list for SFDC depending on Permission.
            var IntegrationTypeList = objDbMrpEntities.IntegrationTypes.Where(type => type.Code == elqType || type.Code == insType && type.IsDeleted == false).ToList();
            var eloquaIntegrationType = IntegrationTypeList.Where(type => type.Code == elqType).Select(type => type.IntegrationTypeId).FirstOrDefault();
            var salesforceIntegrationType = IntegrationTypeList.Where(type => type.Code == insType).Select(type => type.IntegrationTypeId).FirstOrDefault();
            //var eloquaIntegrationType = objDbMrpEntities.IntegrationTypes.Where(type => type.Code == elqType && type.IsDeleted == false).Select(type => type.IntegrationTypeId).FirstOrDefault();
            int eloquaIntegrationTypeId = Convert.ToInt32(eloquaIntegrationType);
            int salesforceIntegrationTypeId = Convert.ToInt32(salesforceIntegrationType);

            List<lstInstance> lstInstancefilter = new List<lstInstance>();
            var clientPermission = objDbMrpEntities.Client_Integration_Permission.Where(intPermission => (intPermission.ClientId.Equals(clientId)) && (intPermission.IntegrationTypeId.Equals(eloquaIntegrationTypeId) || intPermission.IntegrationTypeId.Equals(salesforceIntegrationTypeId)) && (intPermission.PermissionCode.ToUpper().Equals(strPermissionCode_MQL.ToUpper()))).ToList();
            var isEloquaOrSfdc = clientPermission.Where(intPermission => (intPermission.ClientId.Equals(clientId)) && (intPermission.IntegrationTypeId.Equals(eloquaIntegrationTypeId) || intPermission.IntegrationTypeId.Equals(salesforceIntegrationTypeId)) && (intPermission.PermissionCode.ToUpper().Equals(strPermissionCode_MQL.ToUpper()))).Any();
            var isEloqua = clientPermission.Where(intPermission => (intPermission.ClientId.Equals(clientId)) && (intPermission.IntegrationTypeId.Equals(eloquaIntegrationTypeId)) && (intPermission.PermissionCode.ToUpper().Equals(strPermissionCode_MQL.ToUpper()))).Any();
            var isSFDC = clientPermission.Where(intPermission => (intPermission.ClientId.Equals(clientId)) && (intPermission.IntegrationTypeId.Equals(salesforceIntegrationTypeId)) && (intPermission.PermissionCode.ToUpper().Equals(strPermissionCode_MQL.ToUpper()))).Any();
            if (isEloquaOrSfdc)
            {
                if (isEloqua)
                {
                    var lstInstanceEloqua = lstInstance.Where(instance => instance.Code == elqType).ToList();
                    if (lstInstanceEloqua != null && lstInstanceEloqua.Count > 0)
                    {
                        lstInstancefilter.AddRange(lstInstanceEloqua);
                    }
                }
                if (isSFDC)
                {
                    var lstInstanceSFDC = lstInstance.Where(instance => instance.Code == insType).ToList();
                    if (lstInstanceSFDC != null && lstInstanceSFDC.Count > 0)
                    {
                        lstInstancefilter.AddRange(lstInstanceSFDC);
                    }
                }
                if (lstInstancefilter != null && lstInstancefilter.Count > 0)
                {
                    lstInstancefilter = lstInstancefilter.OrderBy(ins => ins.InstanceName).ToList();
                    ViewData["MQLFilteredEloquaIntegrationInstances"] = lstInstancefilter;
                }
                else
                {
                    ViewData["MQLFilteredEloquaIntegrationInstances"] = Enumerable.Empty<entIntegrationInstance>();
                }
            }
            else
                ViewData["MQLFilteredEloquaIntegrationInstances"] = Enumerable.Empty<entIntegrationInstance>();
            #endregion

            ViewBag.IsAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ModelCreateEdit);

            return View(objBaselineModel);
        }

        #endregion


        //Added By Komal Rawal for #2134 to save marketo settings
        public void SaveMarketoSettings(int EntityId, int? InstanceId, string EntityType, string ProgramType, string Channel)
        {
            try
            {
                using (objDbMrpEntities = new MRPEntities())
                {
                    MarketoEntityValueMapping MarketoValueExists = objDbMrpEntities.MarketoEntityValueMappings.Where(set => set.EntityID == EntityId).FirstOrDefault();

                    MarketoEntityValueMapping ObjMarketoSettings;
                    if (MarketoValueExists == null) //If no tactic type entry exists for Marketo in the database. Create a new one
                    {
                        ObjMarketoSettings = new MarketoEntityValueMapping();
                        objDbMrpEntities.Entry(ObjMarketoSettings).State = EntityState.Added;
                    }
                    else
                    {
                        ObjMarketoSettings = MarketoValueExists;
                        objDbMrpEntities.Entry(ObjMarketoSettings).State = EntityState.Modified;
                    }

                    ObjMarketoSettings.EntityID = EntityId;
                    ObjMarketoSettings.IntegrationInstanceId = InstanceId;
                    ObjMarketoSettings.EntityType = EntityType;
                    ObjMarketoSettings.ProgramType = ProgramType;
                    ObjMarketoSettings.Channel = Channel;
                    ObjMarketoSettings.LastModifiedDate = DateTime.Now;
                    ObjMarketoSettings.LastModifiedBy = Sessions.User.ID;
                    objDbMrpEntities.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        //Added By Komal rawal for #2356 add roi package tactic type selection to get all associated packages
        public JsonResult GetListofAssociatedPackage (int TacticTypeID)
       {
            var AssociatedTacticIds = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(list => list.TacticTypeId == TacticTypeID && list.IsDeleted == false).Select(list => list.PlanTacticId).ToList();

            var AssociatedPackageList = objDbMrpEntities.ROI_PackageDetail.Where(list => AssociatedTacticIds.Contains(list.PlanTacticId) && list.PlanTacticId == list.AnchorTacticID).Select(list => list.Plan_Campaign_Program_Tactic.Title).ToList();


            return Json(new { PackageList  = AssociatedPackageList }, JsonRequestBehavior.AllowGet);

        }

        //Added By Komal rawal for #2356 add roi package tactic type selection to update package detail on changinf of asset type
        public void UpdatePackageDetails(int TacticTypeId, bool DeleteAllPackage)
        {
            try
            {
                var AssociatedTacticIds = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(list => list.TacticTypeId == TacticTypeId && list.IsDeleted == false).Select(list => list.PlanTacticId).ToList();

                List<ROI_PackageDetail> AssociatedPackageList = objDbMrpEntities.ROI_PackageDetail.Where(list => AssociatedTacticIds.Contains(list.PlanTacticId)).Select(list => list).ToList();
                List<int> AnchorTacticIDs = AssociatedPackageList.Where(list => list.AnchorTacticID == list.PlanTacticId).Select(list => list.AnchorTacticID).ToList();
                if(AssociatedPackageList.Count > 0)
                {
                    if (DeleteAllPackage == true)
                    {
                        List<ROI_PackageDetail> PackageDetail = objDbMrpEntities.ROI_PackageDetail.Where(list => AnchorTacticIDs.Contains(list.AnchorTacticID)).ToList();
                        PackageDetail.ForEach(a => { objDbMrpEntities.Entry(a).State = EntityState.Deleted; });
                    }
                    else
                    {
                        List<ROI_PackageDetail> PackageDetail = AssociatedPackageList.Where(list => !AnchorTacticIDs.Contains(list.PlanTacticId)).ToList();
                        PackageDetail.ForEach(list => { objDbMrpEntities.Entry(list).State = EntityState.Deleted; });
                    }
                }
                

                objDbMrpEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

        }


    }



}

public class entIntegrationInstance
{
    public string InstanceName { get; set; }
    public int InstanceId { get; set; }
    public string Type { get; set; }
    public string Code { get; set; }
}

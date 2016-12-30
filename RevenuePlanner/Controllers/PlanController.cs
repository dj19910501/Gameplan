using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Transactions;
using Newtonsoft.Json;
using System.Text;
using RevenuePlanner.BDSService;
using RevenuePlanner.Services;
using System.Globalization;
using Integration;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data.OleDb;
using Excel;

/*
 * Added By :
 * Added Date :
 * Description : Plan related events and methods
 */

namespace RevenuePlanner.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class PlanController : CommonController
    {
        #region Variables

        private MRPEntities db = new MRPEntities();
        private BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
        IBudget IBudgetObj;
        IGrid objGrid;
        IPlanTactic objPlanTactic;
        IColumnView objcolumnview;
        public PlanController()
        {
            IBudgetObj = new RevenuePlanner.Services.Budget();
            objGrid = new Grid();
            objPlanTactic = new PlanTactic();
            objcolumnview = new ColumnView();
        }


        private DateTime CalendarStartDate;
        private DateTime CalendarEndDate;
        private string PeriodChar = "Y";
        public const string TacticCustomTitle = "TacticCustom";
        private const string Open = "1";
        private const string PlanBackgroundColor = "#e6e6e6"; //#b4cfd5 Revert color #2312 for #2446 28-7-2016
        private const string CampaignBackgroundColor = "#c6ebf3";//#c9eef6
        private const string ProgramBackgroundColor = "#dff0f8";//#dbf8ff
        private const string TacticBackgroundColor = "#e4f1e1";//#dfdfdf
        private const string LineItemBackgroundColor = "#ffffff";
        private const string Plan = "Plan";
        private const string OtherBackgroundColor = "#f2f2f2";
        private const string Campaign = "Campaign";
        private const string Program = "Program";
        private const string Tactic = "Tactic";
        private const string LineItem = "LineItem";
        private const string Ro = "ro";
        private const string Tree = "tree";
        // private const string ActivityType = "ActivityType";
        private const string PlannedCost = "Planned Cost";
        private const string Total = "Total";
        CacheObject objCache = new CacheObject(); // Add By Nishant Sheth // Desc:: For get values from cache
        StoredProcedure objSp = new StoredProcedure();// Add By Nishant Sheth // Desc:: For get values with storedprocedure
        public RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();
        IImportData objcommonimportData = new Services.ImportData(); //service to use import methods
        #endregion

        #region variable for load plan grid
        double totalmqlCSV = 0;
        double totalrevenueCSV = 0;
        double totalPlannedCostCSV = 0;
        string OwnerNameCsv = "";
        List<Plan_Tactic_Values> ListTacticMQLValue = new List<Plan_Tactic_Values>();

        List<Plan_Tactic_LineItem_Values> LineItemList = new List<Plan_Tactic_LineItem_Values>();
        List<Stage> stageList = new List<Stage>();
        List<User> lstUserDetails = new List<User>();
        List<int> lstCustomFieldsRequired = new List<int>();
        //    List<CustomField_Entity> tacticcustomfieldsentity = new List<CustomField_Entity>();
        List<TacticType> TacticTypeListForHC = new List<TacticType>();
        List<Plan_Campaign_Program_Tactic_LineItem> DBLineItemList = new List<Plan_Campaign_Program_Tactic_LineItem>();
        public double PlanExchangeRate = Sessions.PlanExchangeRate;
        #endregion

        #region Calculate Plan Budget
        /// <summary>
        /// Calculate Plan budget based on goal type selected
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>15/07/2014</CreatedDate>
        /// <param name="modelId">Model id of selected plan</param>
        /// <param name="goalType">goal type of selected plan</param>
        /// <param name="goalValue">goal value for goal type of selected plan</param>
        /// <returns>returns json result object</returns>
        public JsonResult CalculateBudget(int modelId, string goalType, string goalValue)
        {
            string msg1 = "", msg2 = "";
            string input1 = "0", input2 = "0";
            double ADS = 0;
            PlanExchangeRate = Sessions.PlanExchangeRate;
            try
            {
                if (modelId != 0)
                {
                    double ADSValue = db.Models.FirstOrDefault(m => m.ModelId == modelId).AverageDealSize;
                    ADS = ADSValue;
                }

                if (goalType.ToString() != "")
                {
                    BudgetAllocationModel objBudgetAllocationModel = new BudgetAllocationModel();
                    // Start - Modified by Sohel Pathan on 09/12/2014 for PL ticket #975
                    bool isGoalValueExists = false;
                    goalValue = goalValue.Replace(",", "");
                    if (goalValue != "" && Convert.ToDouble(goalValue) != 0)
                    {
                        isGoalValueExists = true;
                        objBudgetAllocationModel = Common.CalculateBudgetInputs(modelId, goalType, goalValue, ADS);
                    }

                    List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.CID && stage.IsDeleted == false).Select(stage => stage).ToList();

                    //// Set Input & Message based on GoalType value.
                    if (goalType.ToString().ToLower() == Enums.PlanGoalType.INQ.ToString().ToLower())
                    {
                        msg1 = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(stage => stage.Title.ToLower()).FirstOrDefault();
                        msg2 = " in revenue";
                        input1 = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.MQLValue.ToString();
                        //input2 = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.RevenueValue.ToString();
                        input2 = isGoalValueExists.Equals(false) ? "0" : objCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(objBudgetAllocationModel.RevenueValue)), PlanExchangeRate).ToString();

                    }
                    else if (goalType.ToString().ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower())
                    {
                        msg1 = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.INQ.ToString().ToLower()).Select(stage => stage.Title.ToLower()).FirstOrDefault();
                        msg2 = " in revenue";
                        input1 = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.INQValue.ToString();
                        //input2 = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.RevenueValue.ToString();
                        input2 = isGoalValueExists.Equals(false) ? "0" : objCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(objBudgetAllocationModel.RevenueValue)), PlanExchangeRate).ToString();

                    }
                    else if (goalType.ToString().ToLower() == Enums.PlanGoalType.Revenue.ToString().ToLower())
                    {
                        msg1 = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(stage => stage.Title.ToLower()).FirstOrDefault();
                        msg2 = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.INQ.ToString().ToLower()).Select(stage => stage.Title.ToLower()).FirstOrDefault();
                        input1 = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.MQLValue.ToString();
                        input2 = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.INQValue.ToString();
                    }
                    // End - Modified by Sohel Pathan on 09/12/2014 for PL ticket #975
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { msg1 = msg1, msg2 = msg2, input1 = input1, input2 = input2, ADS = objCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(ADS)), PlanExchangeRate) }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region GetPlanByPlanID

        /// <summary>
        /// Get plan by plan id
        /// </summary>
        /// <param name="planid"></param>
        public async Task<JsonResult> GetPlanByPlanID(int planid, string year = "", string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "", string TabId = "", bool IsHeaderActuals = false)
        {
            try
            {
                await Task.Delay(1);
                return Json(new
                {
                    lstHomePlanModelHeader = Common.GetPlanHeaderValue(planid, year, CustomFieldId, OwnerIds, TacticTypeids, StatusIds, TabId: TabId, IsHeaderActuals: IsHeaderActuals),// Modified By Nishant Sheth Desc header value wrong with plan tab
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Get Multiple plans data

        public async Task<JsonResult> GetHeaderforPlanByMultiplePlanIDs(string planid, string activeMenu, string year, string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "", bool IsGridView = false)
        {
            planid = System.Web.HttpUtility.UrlDecode(planid);
            List<int> planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(p => int.Parse(p)).ToList();

            try
            {
                await Task.Delay(1);
                return Json(new
                {
                    lstHomePlanModelHeader = Common.GetPlanHeaderValueForPlans(planIds, activeMenu, year, CustomFieldId, OwnerIds, TacticTypeids, StatusIds, IsGridView),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region "Apply To Calendar"
        /// <summary>
        /// Function to update status of current plan.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 12/04/2013
        /// </summary>
        /// <param name="UserId">used id of logged in user</param>  Added by Sohel Pathan on 31/12/2014 for PL ticket #1059
        /// <returns>Returns ApplyToCalendar action result.</returns>
        [HttpPost]
        //Modified By Komal Rawal for new UI
        public ActionResult PublishPlan(Guid userGuid = new Guid(), int planId = 0)
        {
            // Get UserId Integer Id from Guid Ticket #2954
            int userId = Common.GetIntegerUserId(userGuid);
            try
            {
                //// Check cross user login check
                if (userId != 0)
                {
                    if (Sessions.User.ID != userId)
                    {
                        TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                        return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                    }
                }

                //// Update Plan status to Published.
                if (planId == 0)
                    planId = Sessions.PlanId;
                var plan = db.Plans.FirstOrDefault(p => p.PlanId.Equals(planId));
                plan.Status = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()];
                plan.ModifiedBy = Sessions.User.ID;
                plan.ModifiedDate = DateTime.Now;

                int returnValue = db.SaveChanges();
                Common.InsertChangeLog(Sessions.PlanId, 0, Sessions.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.published, "", plan.CreatedBy);
                ViewBag.ActiveMenu = RevenuePlanner.Helpers.Enums.ActiveMenu.Plan;
                return Json(new { activeMenu = Enums.ActiveMenu.Plan.ToString(), currentPlanId = Sessions.PlanId, succmsg = Common.objCached.ModelPublishSuccess }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { errorMessage = Common.objCached.ErrorOccured.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Assortment Mix        
        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Tactic Type INQ,MQL,Cost,Stage Value.
        /// </summary>
        /// <param name="tacticTypeId">Tactic Type Id</param>
        /// <returns>Returns Json Result of Tactic Type. </returns>
        [HttpPost]
        public JsonResult LoadTacticTypeValue(int tacticTypeId)
        {
            TacticType tt = db.TacticTypes.Where(tacType => tacType.TacticTypeId == tacticTypeId).FirstOrDefault();
            PlanExchangeRate = Sessions.PlanExchangeRate;
            return Json(new
            {
                revenue = tt.ProjectedRevenue == null ? 0 : objCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(tt.ProjectedRevenue)), PlanExchangeRate),
                IsDeployedToIntegration = tt.IsDeployedToIntegration,
                stageId = tt.StageId,
                stageTitle = tt.Stage.Title,
                TacticTypeName = tt.Title,
                projectedStageValue = tt.ProjectedStageValue == null ? 0 : tt.ProjectedStageValue
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region "Duplicate"

        /// <summary>
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="CloneType"></param>
        /// <param name="Id"></param>
        /// <param name="title"></param>
        /// <param name="CalledFromBudget"></param>
        /// <returns></returns>
        public ActionResult Clone(string CloneType, int Id, string title, string CalledFromBudget = "")
        {
            int rtResult = 0;

            if (Sessions.User == null)
            {
                TempData["ErrorMessage"] = Common.objCached.SessionExpired;
                return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                if (!string.IsNullOrEmpty(CloneType) && Id > 0)
                {
                    Clonehelper objClonehelper = new Clonehelper();
                    //// Create Clone by CloneType.
                    if (CloneType == Enums.DuplicationModule.Plan.ToString())
                    {
                        rtResult = objClonehelper.ToClone("", CloneType, Id, Id);
                        Plan objPlan = db.Plans.Where(p => p.PlanId == Id).FirstOrDefault();
                        title = objPlan != null ? objPlan.Title : string.Empty;
                    }
                    else
                    {
                        rtResult = objClonehelper.ToClone("", CloneType, Id);
                    }
                    if (CloneType == Enums.DuplicationModule.LineItem.ToString())
                    {
                        CloneType = "Line Item";
                    }

                }

                if (rtResult >= 1)
                {
                    // Added By : Kalpesh Sharma
                    // #953 Invalid Characters Tactic, Program, Campaign Names While Duplicating
                    title = System.Net.WebUtility.HtmlDecode(title);
                    string strMessage = string.Format(Common.objCached.CloneDuplicated, CloneType);     // Modified by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.

                    bool IsBudgetView = false;
                    bool isGridView = false;
                    if (string.Compare(CalledFromBudget, "budget", true) == 0)
                    {
                        IsBudgetView = true;
                    }
                    else if (string.Compare(CalledFromBudget, "grid", true) == 0)
                    {
                        isGridView = true;
                    }
                    ViewBag.CampaignID = Session["CampaignID"];
                    TempData["SuccessMessageDeletedPlan"] = strMessage;
                    return Json(new { redirect = Url.Action("Index", "Home", new { activeMenu = Enums.ActiveMenu.Plan, currentPlanId = Id, IsBudgetView = IsBudgetView, isGridView = isGridView }), planId = Id, Id = rtResult, msg = strMessage });
                }
                return Json(new { });

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                return Json(new { returnValue = rtResult });
            }
        }

        #endregion

        #region "Boost Method"
        
        //Added By komal Rawal for #1432
        [HttpPost]
        public ActionResult DeleteImprovementTacticFromGrid(int id = 0, bool RedirectType = false)
        {
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        int returnValue;
                        string Title = "";
                        // Chage flag isDeleted to true.
                        Plan_Improvement_Campaign_Program_Tactic pcpt = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTac => _imprvTac.ImprovementPlanTacticId == id).FirstOrDefault();
                        pcpt.IsDeleted = true;
                        db.Entry(pcpt).State = EntityState.Modified;
                        returnValue = db.SaveChanges();
                        Title = pcpt.Title;
                        returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcpt.ImprovementPlanTacticId, pcpt.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed, "", pcpt.CreatedBy);
                        if (returnValue >= 1)
                        {
                            scope.Complete();
                            TempData["SuccessMessageDeletedPlan"] = string.Format(Common.objCached.ImprovementTacticDeleteSuccess, Title);

                            return Json(new { redirect = Url.Action("Index", "Home", new { activeMenu = Enums.ActiveMenu.Plan, currentPlanId = Sessions.PlanId, isGridView = true }) });

                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { });
        }
        //End

        /// <summary>
        /// Get Improvement Stage With Improvement Calculation.
        /// </summary>
        /// <param name="ImprovementPlanTacticId">ImprovementPlanTacticId</param>
        /// <param name="ImprovementTacticTypeId">ImprovementTacticTypeId</param>
        /// <param name="EffectiveDate">EffectiveDate</param>
        /// <returns>Return list of ImprovementStages object.</returns>
        public List<ImprovementStage> GetImprovementStages(int ImprovementPlanTacticId, int ImprovementTacticTypeId, DateTime EffectiveDate)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            List<ImprovementStage> ImprovementMetric = new List<ImprovementStage>();
            string CR = Enums.StageType.CR.ToString();
            //// Get List of Stages Associated with selected Improvement Tactic Type.
            ImprovementMetric = (from im in db.ImprovementTacticType_Metric
                                 where im.ImprovementTacticTypeId == ImprovementTacticTypeId
                                 select new ImprovementStage
                                 {
                                     StageId = im.StageId,
                                     StageCode = im.Stage.Code,
                                     StageName = im.StageType == CR ? im.Stage.ConversionTitle : im.Stage.Title,
                                     StageType = im.StageType,
                                     BaseLineRate = 0,
                                     PlanWithoutTactic = 0,
                                     PlanWithTactic = 0,
                                     ClientId = im.Stage.ClientId,
                                 }).ToList();

            //// Get Model Id.
            int ModelId = db.Plans.Where(_pln => _pln.PlanId == Sessions.PlanId).Select(_pln => _pln.ModelId).FirstOrDefault();

            //// Get Model id based on effective Date.
            ModelId = Common.GetModelId(EffectiveDate, ModelId);

            ////// Get Funnelid for Marketing Funnel.
            //string Marketing = Enums.Funnel.Marketing.ToString();
            //int funnelId = db.Funnels.Where(_funl => _funl.Title == Marketing).Select(_funl => _funl.FunnelId).FirstOrDefault();

            //// Loop Execute for Each Stage/Metric.
            double modelvalue = 0, bestInClassValue = 0, TotalWeightWithTactic = 0, TotalWeightWithoutTactic = 0, improvementWeight = 0;
            int TotalCountWithTactic = 0, TotalCountWithoutTactic = 0;
            foreach (var im in ImprovementMetric)
            {
                //// Get Baseline value based on SatgeType.

                if (im.StageType == Enums.StageType.Size.ToString())
                {
                    modelvalue = db.Models.Where(m => m.ModelId == ModelId).Select(m => m.AverageDealSize).FirstOrDefault();
                    modelvalue = objCurrency.GetValueByExchangeRate(modelvalue, PlanExchangeRate);
                }
                else
                {
                    modelvalue = db.Model_Stage.Where(mfs => mfs.ModelId == ModelId && mfs.StageId == im.StageId && mfs.StageType == im.StageType).Select(mfs => mfs.Value).FirstOrDefault();
                    if (im.StageType == Enums.StageType.CR.ToString())
                    {
                        modelvalue = modelvalue / 100;
                    }
                }

                //// Get BestInClas value for MetricId.
                bestInClassValue = db.BestInClasses.
                    Where(bic => bic.StageId == im.StageId && bic.StageType == im.StageType).
                    Select(bic => bic.Value).FirstOrDefault();

                //// Modified by Maninder singh wadhva for Ticket#159
                if (im.StageType == Enums.StageType.CR.ToString() && bestInClassValue != 0)
                {
                    bestInClassValue = bestInClassValue / 100;
                }

                //// Declare variable.
                TotalCountWithTactic = TotalCountWithoutTactic = 0;
                TotalWeightWithTactic = TotalWeightWithoutTactic = 0;

                //// If ImprovementPlanTacticId is 0 i.e. Improvement Tactic In Create Mode.
                if (ImprovementPlanTacticId == 0)
                {

                    //// Get ImprovementTactic & its Weight based on filter criteria for MetricId.
                    var improveTacticList = (from pit in db.Plan_Improvement_Campaign_Program_Tactic
                                             join itm in db.ImprovementTacticType_Metric on pit.ImprovementTacticTypeId equals itm.ImprovementTacticTypeId
                                             where pit.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && itm.ImprovementTacticType.IsDeployed == true
                                             && itm.StageId == im.StageId
                                             && itm.StageType == im.StageType
                                             && itm.Weight > 0 && pit.IsDeleted == false
                                             select new { ImprovemetPlanTacticId = pit.ImprovementPlanTacticId, Weight = itm.Weight }).ToList();

                    //// Calculate Total ImprovementCount for PlanWithoutTactic
                    TotalCountWithoutTactic = improveTacticList.Count();

                    //// Calculate Total ImprovementWeight for PlanWithoutTactic
                    improvementWeight = improveTacticList.Count() == 0 ? 0 : improveTacticList.Sum(itl => itl.Weight);
                    TotalWeightWithoutTactic = improvementWeight;

                    //// Get ImprovementTacticType & its Weight based on filter criteria for MetricId & current ImprovementTacticType.
                    var improvementCountWithTacticList = (from itt in db.ImprovementTacticType_Metric
                                                          where itt.ImprovementTacticType.IsDeployed == true
                                                          && itt.StageId == im.StageId
                                                            && itt.StageType == im.StageType
                                                          && itt.Weight > 0 && itt.ImprovementTacticTypeId == ImprovementTacticTypeId
                                                          select new { ImprovementTacticTypeId = itt.ImprovementTacticTypeId, Weight = itt.Weight }).ToList();

                    //// Calculate Total ImprovementCount for PlanWithTactic
                    TotalCountWithTactic = TotalCountWithoutTactic;
                    TotalCountWithTactic += improvementCountWithTacticList.Count() > 0 ? 1 : 0;

                    //// Calculate Total ImprovementWeight for PlanWithTactic
                    TotalWeightWithTactic = TotalWeightWithoutTactic;
                    TotalWeightWithTactic += improvementCountWithTacticList.Count() == 0 ? 0 : improvementCountWithTacticList.Sum(itl => itl.Weight);
                }
                //// If ImprovementPlanTacticId is not 0 i.e. Improvement Tactic In Edit Mode.
                else
                {

                    //// Get ImprovementTactic & its Weight based on filter criteria for MetricId with current tactic.
                    var improvementCountWithTacticList = (from pit in db.Plan_Improvement_Campaign_Program_Tactic
                                                          join itm in db.ImprovementTacticType_Metric on pit.ImprovementTacticTypeId equals itm.ImprovementTacticTypeId
                                                          where pit.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && itm.ImprovementTacticType.IsDeployed == true
                                                           && itm.StageId == im.StageId
                                                            && itm.StageType == im.StageType
                                                          && itm.Weight > 0 && pit.IsDeleted == false
                                                          select new { ImprovemetPlanTacticId = pit.ImprovementPlanTacticId, Weight = itm.Weight }).ToList();

                    //// Get ImprovementTactic & its Weight based on filter criteria for MetricId & without current improvement tactic.
                    var improveTacticList = (from pit in improvementCountWithTacticList
                                             where pit.ImprovemetPlanTacticId != ImprovementPlanTacticId
                                             select new { ImprovemetPlanTacticId = pit.ImprovemetPlanTacticId, Weight = pit.Weight }).ToList();

                    //// Calculate Total ImprovementCount for PlanWithoutTactic
                    TotalCountWithoutTactic = improveTacticList.Count();

                    //// Calculate Total ImprovementWeight for PlanWithoutTactic
                    improvementWeight = improveTacticList.Count() == 0 ? 0 : improveTacticList.Sum(itl => itl.Weight);
                    TotalWeightWithoutTactic = improvementWeight;

                    //// Calculate Total ImprovementCount for PlanWithTactic
                    TotalCountWithTactic += improvementCountWithTacticList.Count();

                    //// Calculate Total ImprovementWeight for PlanWithTactic
                    TotalWeightWithTactic += improvementCountWithTacticList.Count() == 0 ? 0 : improvementCountWithTacticList.Sum(itl => itl.Weight);
                }

                //// Calculate value based on Metric type.
                if (im.StageType == Enums.StageType.CR.ToString())
                {
                    im.BaseLineRate = modelvalue * 100;
                    im.PlanWithoutTactic = Common.GetImprovement(im.StageType, bestInClassValue, modelvalue, TotalCountWithoutTactic, TotalWeightWithoutTactic) * 100;
                    im.PlanWithTactic = Common.GetImprovement(im.StageType, bestInClassValue, modelvalue, TotalCountWithTactic, TotalWeightWithTactic) * 100;
                }
                else
                {
                    im.BaseLineRate = modelvalue;
                    im.PlanWithoutTactic = Common.GetImprovement(im.StageType, bestInClassValue, modelvalue, TotalCountWithoutTactic, TotalWeightWithoutTactic);
                    im.PlanWithTactic = Common.GetImprovement(im.StageType, bestInClassValue, modelvalue, TotalCountWithTactic, TotalWeightWithTactic);
                }
            }

            return ImprovementMetric;
        }        
        #endregion

        #region Budget Allocation

        /// <summary>
        /// save plan/campaign/program/tactic's monthly/yearly budget values
        /// </summary>
        /// <param name="entityId">plan/campaign/program/tactic id</param>
        /// <param name="section">plan/campaign/program/tactic</param>
        /// <param name="month">month of budget</param>
        /// <param name="inputs">values of budget for month/year</param>
        /// <returns>json flag for success or failure</returns>

        // This method will need to be refactored as part of ticket#2680
        public JsonResult SaveBudgetCell(string entityId, string section, string month, string inputs, bool isquarter)
        {
            try
            {
                string MonthName = string.Empty;
                string CellYear = string.Empty;
                Common.RemoveGridCacheObject();
                if (!string.IsNullOrEmpty(month))
                {
                    string[] MonthYear = month.Split('-');
                    if (MonthYear != null && MonthYear.Count() > 1)
                    {
                        MonthName = MonthYear[0];
                        CellYear = MonthYear[1];
                    }
                }
                int EntityId = Convert.ToInt32(entityId);
                var popupValues = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(inputs);
                double monthlyBudget = popupValues.Where(popup => popup.Key == Common.MonthlyBudgetForEntity).Any() && popupValues.Where(popup => popup.Key == Common.MonthlyBudgetForEntity).FirstOrDefault().Value != "" ? Convert.ToDouble(popupValues.Where(popup => popup.Key == Common.MonthlyBudgetForEntity).FirstOrDefault().Value) : popupValues.Where(popup => popup.Key == Common.MonthlyBudgetForEntity).FirstOrDefault().Value == "" ? 0 : -1;
                double yearlyBudget = popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).Any() && popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).FirstOrDefault().Value != "" ? Convert.ToDouble(popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).FirstOrDefault().Value) : popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).FirstOrDefault().Value == "" ? 0 : -1;
                if (popupValues.Where(popup => popup.Key == Common.MonthlyBudgetForEntity).Any() && popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).Any())
                {
                    //Commented by Preet Shah on 10/12/2016 For allowed negative values PL #2850.
                    // if (monthlyBudget >= 0 && yearlyBudget >= 0)
                    // {
                    string period = Enums.monthList[MonthName].ToString();
                    string PlanYear = GetPlanYear(section, EntityId);

                    int PlanYearDiff = Convert.ToInt32(CellYear) - Convert.ToInt32(PlanYear);
                    if (PlanYearDiff > 0)
                    {
                        int MonthCnt = Convert.ToInt32(period.Replace(PeriodChar, ""));
                        period = "Y" + Convert.ToString(MonthCnt + (PlanYearDiff * 12));
                    }
                    if (string.Compare(section, Enums.Section.Plan.ToString(), true) == 0)
                    {
                        var objPlan = db.Plans.Where(p => p.PlanId == EntityId && p.IsDeleted.Equals(false)).FirstOrDefault();
                        objPlan.Budget = yearlyBudget;
                        if (!isquarter)
                        {
                            if (objPlan.Plan_Budget.Where(pb => pb.Period == period).Any())
                            {
                                objPlan.Plan_Budget.Where(pb => pb.Period == period).FirstOrDefault().Value = monthlyBudget;
                            }
                            else
                            {
                                Plan_Budget objPlanBudget = new Plan_Budget();
                                objPlanBudget.PlanId = EntityId;
                                objPlanBudget.Period = period;
                                objPlanBudget.Value = monthlyBudget;
                                objPlanBudget.CreatedBy = Sessions.User.ID;
                                objPlanBudget.CreatedDate = DateTime.Now;
                                db.Entry(objPlanBudget).State = EntityState.Added;
                            }

                        }
                        else
                        {
                            //// Get Previous budget allocation data by PlanId.
                            var PrevPlanBudgetAllocationList = db.Plan_Budget.Where(pb => pb.PlanId == EntityId).Select(pb => pb).ToList();
                            int QuarterCnt = 1, j = 1;
                            bool isExists;
                            List<Plan_Budget> thisQuartersMonthList;
                            Plan_Budget thisQuarterFirstMonthBudget, objPlanBudget;
                            double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;

                            QuarterCnt = Convert.ToInt32(period.Replace(PeriodChar, ""));

                            //// Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                            isExists = false;
                            if (PrevPlanBudgetAllocationList != null && PrevPlanBudgetAllocationList.Count > 0)
                            {
                                //// Get current Quarter months budget.
                                thisQuartersMonthList = new List<Plan_Budget>();
                                thisQuartersMonthList = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                thisQuarterFirstMonthBudget = new Plan_Budget();
                                thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                if (thisQuarterFirstMonthBudget != null)
                                {
                                    //if (true)
                                    //{
                                    thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                    //// Get quarter total budget. 
                                    thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                    newValue = monthlyBudget;

                                    if (thisQuarterTotalBudget != newValue)
                                    {
                                        //// Get budget difference.
                                        BudgetDiff = newValue - thisQuarterTotalBudget;
                                        if (BudgetDiff > 0)
                                        {
                                            //// Set quarter first month budget value.
                                            thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            j = 1;
                                            while (thisQuarterFirstMonthBudget != null)
                                            {
                                                //if ( == null)
                                                // {
                                                BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                //Commented by Preet Shah on 10/12/2016 For allowed negative values PL #2850
                                                //if (BudgetDiff <= 0)
                                                //    thisQuarterFirstMonthBudget.Value = 0;
                                                //else
                                                if (j < 2)
                                                {
                                                    thisQuarterFirstMonthBudget.Value = BudgetDiff;
                                                    db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                }

                                                //}
                                                if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                {
                                                    thisQuarterFirstMonthBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                }
                                                else
                                                    thisQuarterFirstMonthBudget = null;

                                                j = j + 1;
                                            }
                                        }
                                    }
                                    //}
                                    //else
                                    //{
                                    //    thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                    //}
                                    isExists = true;
                                }
                            }
                            //// if previous values does not exist then insert new values.
                            if (!isExists)
                            {
                                //// End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                objPlanBudget = new Plan_Budget();
                                objPlanBudget.PlanId = EntityId;
                                objPlanBudget.Period = PeriodChar + QuarterCnt;
                                objPlanBudget.Value = Convert.ToDouble(monthlyBudget);
                                objPlanBudget.CreatedBy = Sessions.User.ID;
                                objPlanBudget.CreatedDate = DateTime.Now;
                                db.Entry(objPlanBudget).State = EntityState.Added;
                            }
                        }
                        db.Entry(objPlan).State = EntityState.Modified;
                    }
                    else if (string.Compare(section, Enums.Section.Campaign.ToString(), true) == 0)
                    {
                        var objCampaign = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == EntityId && pc.IsDeleted.Equals(false)).FirstOrDefault();
                        objCampaign.CampaignBudget = yearlyBudget;
                        if (!isquarter)
                        {

                            if (objCampaign.Plan_Campaign_Budget.Where(pcb => pcb.Period == period).Any())
                            {
                                objCampaign.Plan_Campaign_Budget.Where(pcb => pcb.Period == period).FirstOrDefault().Value = monthlyBudget;
                            }
                            else
                            {
                                Plan_Campaign_Budget objCampaignBudget = new Plan_Campaign_Budget();
                                objCampaignBudget.PlanCampaignId = EntityId;
                                objCampaignBudget.Period = period;
                                objCampaignBudget.Value = monthlyBudget;
                                objCampaignBudget.CreatedBy = Sessions.User.ID;
                                objCampaignBudget.CreatedDate = DateTime.Now;
                                db.Entry(objCampaignBudget).State = EntityState.Added;
                            }

                        }
                        else
                        {
                            //// Get Previous budget allocation data by PlanId.
                            var PrevCampaignBudgetAllocationList = db.Plan_Campaign_Budget.Where(pb => pb.PlanCampaignId == EntityId).Select(pb => pb).ToList();
                            int QuarterCnt = 1, j = 1;
                            bool isExists;
                            List<Plan_Campaign_Budget> thisQuartersMonthList;
                            Plan_Campaign_Budget thisQuarterFirstMonthBudget, objCampaignBudget;
                            double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;

                            QuarterCnt = Convert.ToInt32(period.Replace(PeriodChar, ""));

                            //// Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                            isExists = false;
                            if (PrevCampaignBudgetAllocationList != null && PrevCampaignBudgetAllocationList.Count > 0)
                            {
                                //// Get current Quarter months budget.
                                thisQuartersMonthList = new List<Plan_Campaign_Budget>();
                                thisQuartersMonthList = PrevCampaignBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                thisQuarterFirstMonthBudget = new Plan_Campaign_Budget();
                                thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                if (thisQuarterFirstMonthBudget != null)
                                {
                                    //if (true)
                                    //{
                                    thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                    //// Get quarter total budget. 
                                    thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                    newValue = monthlyBudget;

                                    if (thisQuarterTotalBudget != newValue)
                                    {
                                        //// Get budget difference.
                                        BudgetDiff = newValue - thisQuarterTotalBudget;
                                        if (BudgetDiff > 0)
                                        {
                                            //// Set quarter first month budget value.
                                            thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            j = 1;
                                            while (thisQuarterFirstMonthBudget != null)
                                            {
                                                //if ( == null)
                                                // {
                                                BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                //Commented by Preet Shah on 10/12/2016 For allowed negative values PL #2850
                                                //if (BudgetDiff <= 0)
                                                //    thisQuarterFirstMonthBudget.Value = 0;
                                                //else
                                                if (j < 2)
                                                {
                                                    thisQuarterFirstMonthBudget.Value = BudgetDiff;
                                                    db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                }

                                                //}
                                                if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                {
                                                    thisQuarterFirstMonthBudget = PrevCampaignBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                }
                                                else
                                                    thisQuarterFirstMonthBudget = null;

                                                j = j + 1;
                                            }
                                        }
                                    }
                                    //}
                                    //else
                                    //{
                                    //    thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                    //}
                                    isExists = true;
                                }
                            }
                            //// if previous values does not exist then insert new values.
                            if (!isExists)
                            {
                                //// End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                objCampaignBudget = new Plan_Campaign_Budget();
                                objCampaignBudget.PlanCampaignId = EntityId;
                                objCampaignBudget.Period = PeriodChar + QuarterCnt;
                                objCampaignBudget.Value = Convert.ToDouble(monthlyBudget);
                                objCampaignBudget.CreatedBy = Sessions.User.ID;
                                objCampaignBudget.CreatedDate = DateTime.Now;
                                db.Entry(objCampaignBudget).State = EntityState.Added;
                            }
                        }
                        db.Entry(objCampaign).State = EntityState.Modified;
                    }
                    else if (string.Compare(section, Enums.Section.Program.ToString(), true) == 0)
                    {
                        var objProgram = db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == EntityId && pcp.IsDeleted.Equals(false)).FirstOrDefault();
                        objProgram.ProgramBudget = yearlyBudget;
                        if (!isquarter)
                        {
                            if (objProgram.Plan_Campaign_Program_Budget.Where(pcpb => pcpb.Period == period).Any())
                            {
                                objProgram.Plan_Campaign_Program_Budget.Where(pcpb => pcpb.Period == period).FirstOrDefault().Value = monthlyBudget;
                            }
                            else
                            {
                                Plan_Campaign_Program_Budget objProgramBudget = new Plan_Campaign_Program_Budget();
                                objProgramBudget.PlanProgramId = EntityId;
                                objProgramBudget.Period = period;
                                objProgramBudget.Value = monthlyBudget;
                                objProgramBudget.CreatedBy = Sessions.User.ID;
                                objProgramBudget.CreatedDate = DateTime.Now;
                                db.Entry(objProgramBudget).State = EntityState.Added;
                            }

                        }
                        else
                        {
                            //// Get Previous budget allocation data by PlanId.
                            var PrevProgramBudgetAllocationList = db.Plan_Campaign_Program_Budget.Where(pb => pb.PlanProgramId == EntityId).Select(pb => pb).ToList();
                            int QuarterCnt = 1, j = 1;
                            bool isExists;
                            List<Plan_Campaign_Program_Budget> thisQuartersMonthList;
                            Plan_Campaign_Program_Budget thisQuarterFirstMonthBudget, objProgramBudget;
                            double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;

                            QuarterCnt = Convert.ToInt32(period.Replace(PeriodChar, ""));

                            //// Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                            isExists = false;
                            if (PrevProgramBudgetAllocationList != null && PrevProgramBudgetAllocationList.Count > 0)
                            {
                                //// Get current Quarter months budget.
                                thisQuartersMonthList = new List<Plan_Campaign_Program_Budget>();
                                thisQuartersMonthList = PrevProgramBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                thisQuarterFirstMonthBudget = new Plan_Campaign_Program_Budget();
                                thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                if (thisQuarterFirstMonthBudget != null)
                                {
                                    //if (true)
                                    //{
                                    thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                    //// Get quarter total budget. 
                                    thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                    newValue = monthlyBudget;

                                    if (thisQuarterTotalBudget != newValue)
                                    {
                                        //// Get budget difference.
                                        BudgetDiff = newValue - thisQuarterTotalBudget;
                                        if (BudgetDiff > 0)
                                        {
                                            //// Set quarter first month budget value.
                                            thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            j = 1;
                                            while (thisQuarterFirstMonthBudget != null)
                                            {
                                                //if ( == null)
                                                // {
                                                BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                //Commented by Preet Shah on 10/12/2016 For allowed negative values PL #2850
                                                //if (BudgetDiff <= 0)
                                                //    thisQuarterFirstMonthBudget.Value = 0;
                                                //else
                                                if (j < 2)
                                                {
                                                    thisQuarterFirstMonthBudget.Value = BudgetDiff;
                                                    db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                }

                                                //}
                                                if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                {
                                                    thisQuarterFirstMonthBudget = PrevProgramBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                }
                                                else
                                                    thisQuarterFirstMonthBudget = null;

                                                j = j + 1;
                                            }
                                        }
                                    }
                                    //}
                                    //else
                                    //{
                                    //    thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                    //}
                                    isExists = true;
                                }
                            }
                            //// if previous values does not exist then insert new values.
                            if (!isExists)
                            {
                                //// End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                objProgramBudget = new Plan_Campaign_Program_Budget();
                                objProgramBudget.PlanProgramId = EntityId;
                                objProgramBudget.Period = PeriodChar + QuarterCnt;
                                objProgramBudget.Value = Convert.ToDouble(monthlyBudget);
                                objProgramBudget.CreatedBy = Sessions.User.ID;
                                objProgramBudget.CreatedDate = DateTime.Now;
                                db.Entry(objProgramBudget).State = EntityState.Added;
                            }
                        }
                        db.Entry(objProgram).State = EntityState.Modified;
                    }
                    else if (string.Compare(section, Enums.Section.Tactic.ToString(), true) == 0)
                    {
                        var objTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == EntityId && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
                        objTactic.TacticBudget = yearlyBudget;
                        if (!isquarter)
                        {

                            #region LinkedTactic updation

                            int linkedTacticId = objTactic.LinkedTacticId.GetValueOrDefault();
                            Plan_Campaign_Program_Tactic LinkedTactic = new Plan_Campaign_Program_Tactic();
                            if (linkedTacticId > 0)
                            {
                                LinkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == linkedTacticId && pcpt.IsDeleted == false).FirstOrDefault();

                                if (LinkedTactic != null)
                                {
                                    int yearDiff = LinkedTactic.EndDate.Year - LinkedTactic.StartDate.Year;
                                    bool isMultiYearLinkedTactic = yearDiff > 0 ? true : false;
                                    int mnthCounter = 12 * yearDiff;
                                    int mnt = Convert.ToInt32(period.Replace(PeriodChar, ""));
                                    string linkedPeriod = string.Empty;
                                    if (!isMultiYearLinkedTactic)
                                    {
                                        if ((mnt - 12) > 0)
                                        {
                                            linkedPeriod = PeriodChar + Convert.ToString((mnt - 12));
                                        }

                                    }
                                    else
                                    {
                                        linkedPeriod = PeriodChar + Convert.ToString((mnt + 12));
                                    }
                                    if (linkedPeriod != string.Empty)
                                    {
                                        if (LinkedTactic.Plan_Campaign_Program_Tactic_Budget.Where(pcptc => pcptc.Period == linkedPeriod).Any())
                                        {
                                            LinkedTactic.Plan_Campaign_Program_Tactic_Budget.Where(pcptc => pcptc.Period == linkedPeriod).FirstOrDefault().Value = monthlyBudget;
                                        }
                                        else
                                        {
                                            Plan_Campaign_Program_Tactic_Budget objLinkedTacticBudget = new Plan_Campaign_Program_Tactic_Budget();
                                            objLinkedTacticBudget.PlanTacticId = linkedTacticId;
                                            objLinkedTacticBudget.Period = linkedPeriod;
                                            objLinkedTacticBudget.Value = monthlyBudget;
                                            objLinkedTacticBudget.CreatedBy = Sessions.User.ID;
                                            objLinkedTacticBudget.CreatedDate = DateTime.Now;
                                            db.Entry(objLinkedTacticBudget).State = EntityState.Added;
                                        }
                                    }
                                }
                            }


                            #endregion

                            if (objTactic.Plan_Campaign_Program_Tactic_Budget.Where(pcptc => pcptc.Period == period).Any())
                            {
                                objTactic.Plan_Campaign_Program_Tactic_Budget.Where(pcptc => pcptc.Period == period).FirstOrDefault().Value = monthlyBudget;
                            }
                            else
                            {
                                Plan_Campaign_Program_Tactic_Budget objTacticBudget = new Plan_Campaign_Program_Tactic_Budget();
                                objTacticBudget.PlanTacticId = EntityId;
                                objTacticBudget.Period = period;
                                objTacticBudget.Value = monthlyBudget;
                                objTacticBudget.CreatedBy = Sessions.User.ID;
                                objTacticBudget.CreatedDate = DateTime.Now;
                                db.Entry(objTacticBudget).State = EntityState.Added;
                            }
                        }
                        else
                        {
                            //// Get Previous budget allocation data by PlanId.
                            var PrevTacticBudgetAllocationList = db.Plan_Campaign_Program_Tactic_Budget.Where(pb => pb.PlanTacticId == EntityId).Select(pb => pb).ToList();
                            int QuarterCnt = 1, j = 1;
                            bool isExists;
                            List<Plan_Campaign_Program_Tactic_Budget> thisQuartersMonthList;
                            Plan_Campaign_Program_Tactic_Budget thisQuarterFirstMonthBudget, objTacticBudget;
                            double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;

                            QuarterCnt = Convert.ToInt32(period.Replace(PeriodChar, ""));
                            // double BudgetforLinkedTactic = -1;
                            //// Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                            isExists = false;
                            if (PrevTacticBudgetAllocationList != null && PrevTacticBudgetAllocationList.Count > 0)
                            {
                                //// Get current Quarter months budget.
                                thisQuartersMonthList = new List<Plan_Campaign_Program_Tactic_Budget>();
                                thisQuartersMonthList = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                thisQuarterFirstMonthBudget = new Plan_Campaign_Program_Tactic_Budget();
                                thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();
                                if (thisQuarterFirstMonthBudget != null)
                                {
                                    //if (true)
                                    //{
                                    thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                    //// Get quarter total budget. 
                                    thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                    newValue = monthlyBudget;

                                    if (thisQuarterTotalBudget != newValue)
                                    {
                                        //// Get budget difference.
                                        BudgetDiff = newValue - thisQuarterTotalBudget;
                                        if (BudgetDiff > 0)
                                        {
                                            //// Set quarter first month budget value.
                                            thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            j = 1;
                                            while (thisQuarterFirstMonthBudget != null)
                                            {
                                                //if ( == null)
                                                // {
                                                BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                //Commented by Preet Shah on 10/12/2016 For allowed negative values PL #2850
                                                //if (BudgetDiff <= 0)
                                                //    thisQuarterFirstMonthBudget.Value = 0;
                                                //else
                                                if (j < 2)
                                                {
                                                    thisQuarterFirstMonthBudget.Value = BudgetDiff;
                                                    db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                }

                                                //}
                                                if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                {
                                                    thisQuarterFirstMonthBudget = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                }
                                                else
                                                    thisQuarterFirstMonthBudget = null;

                                                j = j + 1;
                                            }
                                        }
                                    }
                                    //}
                                    //else
                                    //{
                                    //    thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                    //}
                                    isExists = true;
                                }
                            }
                            //// if previous values does not exist then insert new values.
                            if (!isExists)
                            {
                                //// End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                objTacticBudget = new Plan_Campaign_Program_Tactic_Budget();
                                objTacticBudget.PlanTacticId = EntityId;
                                objTacticBudget.Period = PeriodChar + QuarterCnt;
                                objTacticBudget.Value = Convert.ToDouble(monthlyBudget);
                                objTacticBudget.CreatedBy = Sessions.User.ID;
                                objTacticBudget.CreatedDate = DateTime.Now;
                                db.Entry(objTacticBudget).State = EntityState.Added;
                            }

                            #region LinkedTactic updation

                            int linkedTacticId = objTactic.LinkedTacticId.GetValueOrDefault();
                            Plan_Campaign_Program_Tactic LinkedTactic = new Plan_Campaign_Program_Tactic();
                            if (linkedTacticId > 0)
                            {
                                LinkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == linkedTacticId && pcpt.IsDeleted == false).FirstOrDefault();

                                if (LinkedTactic != null)
                                {
                                    int yearDiff = LinkedTactic.EndDate.Year - LinkedTactic.StartDate.Year;
                                    bool isMultiYearLinkedTactic = yearDiff > 0 ? true : false;

                                    int mnt = Convert.ToInt32(period.Replace(PeriodChar, ""));
                                    string linkedPeriod = string.Empty;
                                    if (!isMultiYearLinkedTactic)
                                    {
                                        if ((mnt - 12) > 0)
                                        {
                                            linkedPeriod = PeriodChar + Convert.ToString((mnt - 12));
                                        }

                                    }
                                    else
                                    {
                                        linkedPeriod = PeriodChar + Convert.ToString((mnt + 12));
                                    }
                                    if (!string.IsNullOrEmpty(linkedPeriod))
                                    {
                                        if (LinkedTactic.Plan_Campaign_Program_Tactic_Budget.Where(pcptc => pcptc.Period == linkedPeriod).Any())
                                        {

                                            LinkedTactic.Plan_Campaign_Program_Tactic_Budget.Where(pcptc => pcptc.Period == linkedPeriod).FirstOrDefault().Value = monthlyBudget;
                                        }
                                        else
                                        {
                                            Plan_Campaign_Program_Tactic_Budget objLinkedTacticBudget = new Plan_Campaign_Program_Tactic_Budget();
                                            objLinkedTacticBudget.PlanTacticId = linkedTacticId;
                                            objLinkedTacticBudget.Period = linkedPeriod;
                                            objLinkedTacticBudget.Value = monthlyBudget;
                                            objLinkedTacticBudget.CreatedBy = Sessions.User.ID;
                                            objLinkedTacticBudget.CreatedDate = DateTime.Now;
                                            db.Entry(objLinkedTacticBudget).State = EntityState.Added;
                                        }
                                    }
                                }
                            }


                            #endregion
                        }
                        objTactic.ModifiedBy = Sessions.User.ID;
                        objTactic.ModifiedDate = DateTime.Now;
                        db.Entry(objTactic).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    return Json(new { budgetMonth = monthlyBudget, budgetYear = yearlyBudget, isSuccess = true });
                    // }
                }
                else if (popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).Any())
                {
                    if (string.Compare(section, Enums.Section.Plan.ToString(), true) == 0)
                    {
                        var objPlan = db.Plans.Where(p => p.PlanId == EntityId && p.IsDeleted.Equals(false)).FirstOrDefault();
                        objPlan.Budget = yearlyBudget;
                        if (yearlyBudget == 0)
                        {
                            objPlan.AllocatedBy = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString();
                        }
                        db.Entry(objPlan).State = EntityState.Modified;
                    }
                    else if (string.Compare(section, Enums.Section.Campaign.ToString(), true) == 0)
                    {
                        var objCampaign = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == EntityId && pc.IsDeleted.Equals(false)).FirstOrDefault();
                        objCampaign.CampaignBudget = yearlyBudget;
                        db.Entry(objCampaign).State = EntityState.Modified;
                    }
                    else if (string.Compare(section, Enums.Section.Program.ToString(), true) == 0)
                    {
                        var objProgram = db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == EntityId && pcp.IsDeleted.Equals(false)).FirstOrDefault();
                        objProgram.ProgramBudget = yearlyBudget;
                        db.Entry(objProgram).State = EntityState.Modified;
                    }
                    else if (string.Compare(section, Enums.Section.Tactic.ToString(), true) == 0)
                    {
                        var objTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == EntityId && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
                        if (objTactic.Plan_Campaign_Program_Tactic_Budget.ToList().Count == 0)
                        {
                            int startmonth = objTactic.StartDate.Month;
                            Plan_Campaign_Program_Tactic_Budget objtacticbudget = new Plan_Campaign_Program_Tactic_Budget();
                            objtacticbudget.Period = PeriodChar + startmonth;
                            objtacticbudget.PlanTacticId = objTactic.PlanTacticId;
                            objtacticbudget.Value = yearlyBudget;
                            objtacticbudget.CreatedBy = Sessions.User.ID;
                            objtacticbudget.CreatedDate = DateTime.Now;
                            db.Entry(objtacticbudget).State = EntityState.Added;
                        }
                        objTactic.TacticBudget = yearlyBudget;
                        objTactic.ModifiedBy = Sessions.User.ID;
                        objTactic.ModifiedDate = DateTime.Now;
                        db.Entry(objTactic).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    return Json(new { budgetMonth = 0, budgetYear = yearlyBudget, isSuccess = true });
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { isSuccess = false });
        }

        /// <summary>
        /// save plan/campaign/program/tactic's monthly/yearly budget values
        /// </summary>
        /// <param name="entityId">plan/campaign/program/tactic id</param>
        /// <param name="section">plan/campaign/program/tactic</param>
        /// <param name="month">month of budget</param>
        /// <param name="inputs">values of budget for month/year</param>
        /// <returns>json flag for success or failure</returns>

        // This method will need to be refactored as part of ticket#2679
        public JsonResult SavePlannedCell(string entityId, string section, string month, string inputs, string tab, bool isquarter)
        {
            try
            {
                string MonthName = string.Empty;
                string CellYear = string.Empty;
                Common.RemoveGridCacheObject();
                if (!string.IsNullOrEmpty(month))
                {
                    string[] MonthYear = month.Split('-');
                    if (MonthYear != null && MonthYear.Count() > 1)
                    {
                        MonthName = MonthYear[0];
                        CellYear = MonthYear[1];
                    }
                }
                int EntityId = Convert.ToInt32(entityId);
                var popupValues = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(inputs);

                double monthlycost = popupValues.Where(popup => popup.Key == Common.MonthlyCostForEntity).Any() && popupValues.Where(popup => popup.Key == Common.MonthlyCostForEntity).FirstOrDefault().Value != "" ? Convert.ToDouble(popupValues.Where(popup => popup.Key == Common.MonthlyCostForEntity).FirstOrDefault().Value) : popupValues.Where(popup => popup.Key == Common.MonthlyCostForEntity).FirstOrDefault().Value == "" ? 0 : -1;
                double yearlycost = popupValues.Where(popup => popup.Key == Common.YearlyCostForEntity).Any() && popupValues.Where(popup => popup.Key == Common.YearlyCostForEntity).FirstOrDefault().Value != "" ? Convert.ToDouble(popupValues.Where(popup => popup.Key == Common.YearlyCostForEntity).FirstOrDefault().Value) : popupValues.Where(popup => popup.Key == Common.YearlyCostForEntity).FirstOrDefault().Value == "" ? 0 : -1;
                if (popupValues.Where(popup => popup.Key == Common.MonthlyCostForEntity).Any())
                {
                    string period = Enums.monthList[MonthName].ToString();
                    string PlanYear = GetPlanYear(section, EntityId);
                    int PlanYearDiff = Convert.ToInt32(CellYear) - Convert.ToInt32(PlanYear);
                    if (PlanYearDiff > 0)
                    {
                        int MonthCnt = Convert.ToInt32(period.Replace(PeriodChar, ""));
                        period = "Y" + Convert.ToString(MonthCnt + (PlanYearDiff * 12));
                    }
                    if (string.Compare(section, Enums.Section.Tactic.ToString(), true) == 0)
                    {
                        if (tab == Enums.BudgetTab.Actual.ToString())
                        {
                            var objTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == EntityId && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
                            string costStageTitle = Enums.InspectStage.Cost.ToString();
                            if (!isquarter)
                            {
                                if (objTactic.Plan_Campaign_Program_Tactic_Actual.Where(pcptc => pcptc.Period == period && pcptc.StageTitle == costStageTitle).Any())
                                {
                                    objTactic.Plan_Campaign_Program_Tactic_Actual.Where(pcptc => pcptc.Period == period && pcptc.StageTitle == costStageTitle).FirstOrDefault().Actualvalue = monthlycost;
                                }
                                else
                                {
                                    Plan_Campaign_Program_Tactic_Actual objTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                    objTacticActual.PlanTacticId = EntityId;
                                    objTacticActual.Period = period;
                                    objTacticActual.StageTitle = costStageTitle;
                                    objTacticActual.Actualvalue = monthlycost;
                                    objTacticActual.CreatedBy = Sessions.User.ID;
                                    objTacticActual.CreatedDate = DateTime.Now;
                                    db.Entry(objTacticActual).State = EntityState.Added;
                                }


                                #region LinkedTactic updation

                                int linkedTacticId = objTactic.LinkedTacticId.GetValueOrDefault();

                                Plan_Campaign_Program_Tactic LinkedTactic = new Plan_Campaign_Program_Tactic();
                                if (linkedTacticId > 0)
                                {
                                    LinkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == linkedTacticId && pcpt.IsDeleted == false).FirstOrDefault();
                                    if (LinkedTactic != null)
                                    {
                                        int yearDiff = LinkedTactic.EndDate.Year - LinkedTactic.StartDate.Year;
                                        bool isMultiYearLinkedTactic = yearDiff > 0 ? true : false;
                                        int mnthCounter = 12 * yearDiff;
                                        int mnt = Convert.ToInt32(period.Replace(PeriodChar, ""));
                                        string linkedPeriod = string.Empty;
                                        if (!isMultiYearLinkedTactic)
                                        {
                                            if ((mnt - 12) > 0)
                                            {
                                                linkedPeriod = PeriodChar + Convert.ToString((mnt - 12));
                                            }

                                        }
                                        else
                                        {
                                            linkedPeriod = PeriodChar + Convert.ToString((mnt + 12));
                                        }
                                        if (!string.IsNullOrEmpty(linkedPeriod))
                                        {
                                            if (LinkedTactic.Plan_Campaign_Program_Tactic_Actual.Where(pcptc => pcptc.Period == linkedPeriod).Any())
                                            {
                                                LinkedTactic.Plan_Campaign_Program_Tactic_Actual.Where(pcptc => pcptc.Period == linkedPeriod).FirstOrDefault().Actualvalue = monthlycost;
                                            }
                                            else
                                            {
                                                Plan_Campaign_Program_Tactic_Actual objLinkedTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                                objLinkedTacticActual.PlanTacticId = linkedTacticId;
                                                objLinkedTacticActual.Period = linkedPeriod;
                                                objLinkedTacticActual.StageTitle = costStageTitle;
                                                objLinkedTacticActual.Actualvalue = monthlycost;
                                                objLinkedTacticActual.CreatedBy = Sessions.User.ID;
                                                objLinkedTacticActual.CreatedDate = DateTime.Now;
                                                db.Entry(objLinkedTacticActual).State = EntityState.Added;
                                            }
                                        }
                                    }
                                }


                                #endregion
                            }
                            else
                            {
                                //// Get Previous budget allocation data by PlanId.
                                var PrevTacticBudgetAllocationList = objTactic.Plan_Campaign_Program_Tactic_Actual.Where(pcpta => pcpta.StageTitle == costStageTitle).Select(pb => pb).ToList();
                                int QuarterCnt = 1, j = 1;
                                bool isExists;
                                List<Plan_Campaign_Program_Tactic_Actual> thisQuartersMonthList;
                                Plan_Campaign_Program_Tactic_Actual thisQuarterFirstMonthBudget, objTacticBudget;
                                double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;

                                QuarterCnt = Convert.ToInt32(period.Replace(PeriodChar, ""));

                                //// Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                isExists = false;
                                if (PrevTacticBudgetAllocationList != null && PrevTacticBudgetAllocationList.Count > 0)
                                {
                                    //// Get current Quarter months budget.
                                    thisQuartersMonthList = new List<Plan_Campaign_Program_Tactic_Actual>();
                                    thisQuartersMonthList = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                    thisQuarterFirstMonthBudget = new Plan_Campaign_Program_Tactic_Actual();
                                    thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                    if (thisQuarterFirstMonthBudget != null)
                                    {
                                        //if (true)
                                        //{
                                        thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Actualvalue);
                                        //// Get quarter total budget. 
                                        thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Actualvalue + thisQuarterOtherMonthBudget;
                                        newValue = monthlycost;

                                        if (thisQuarterTotalBudget != newValue)
                                        {
                                            //// Get budget difference.
                                            BudgetDiff = newValue - thisQuarterTotalBudget;
                                            if (BudgetDiff > 0)
                                            {
                                                //// Set quarter first month budget value.
                                                thisQuarterFirstMonthBudget.Actualvalue = thisQuarterFirstMonthBudget.Actualvalue + BudgetDiff;
                                                db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                j = 1;
                                                while (thisQuarterFirstMonthBudget != null)
                                                {
                                                    //if ( == null)
                                                    // {
                                                    BudgetDiff = thisQuarterFirstMonthBudget.Actualvalue + BudgetDiff;

                                                    //Commented by Preet Shah on 10/12/2016 For allowed negative values PL #2850
                                                    //if (BudgetDiff <= 0)
                                                    //    thisQuarterFirstMonthBudget.Value = 0;
                                                    //else
                                                    if (j < 2)
                                                    {
                                                        thisQuarterFirstMonthBudget.Actualvalue = BudgetDiff;
                                                        db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                    }

                                                    //}
                                                    if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                    {
                                                        thisQuarterFirstMonthBudget = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                    }
                                                    else
                                                        thisQuarterFirstMonthBudget = null;

                                                    j = j + 1;
                                                }
                                            }
                                        }
                                        //}
                                        //else
                                        //{
                                        //    thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                        //}
                                        isExists = true;
                                    }
                                }
                                //// if previous values does not exist then insert new values.
                                if (!isExists)
                                {
                                    //// End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                    objTacticBudget = new Plan_Campaign_Program_Tactic_Actual();
                                    objTacticBudget.PlanTacticId = EntityId;
                                    objTacticBudget.Period = PeriodChar + QuarterCnt;
                                    objTacticBudget.StageTitle = costStageTitle;
                                    objTacticBudget.Actualvalue = Convert.ToDouble(monthlycost);
                                    objTacticBudget.CreatedBy = Sessions.User.ID;
                                    objTacticBudget.CreatedDate = DateTime.Now;
                                    db.Entry(objTacticBudget).State = EntityState.Added;
                                }

                                #region LinkedTactic updation

                                int linkedTacticId = objTactic.LinkedTacticId.GetValueOrDefault();

                                Plan_Campaign_Program_Tactic LinkedTactic = new Plan_Campaign_Program_Tactic();
                                if (linkedTacticId > 0)
                                {
                                    LinkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == linkedTacticId && pcpt.IsDeleted == false).FirstOrDefault();

                                    if (LinkedTactic != null)
                                    {
                                        int yearDiff = LinkedTactic.EndDate.Year - LinkedTactic.StartDate.Year;
                                        bool isMultiYearLinkedTactic = yearDiff > 0 ? true : false;
                                        int mnthCounter = 12 * yearDiff;
                                        int mnt = Convert.ToInt32(period.Replace(PeriodChar, ""));
                                        string linkedPeriod = string.Empty;
                                        if (!isMultiYearLinkedTactic)
                                        {
                                            if ((mnt - 12) > 0)
                                            {
                                                linkedPeriod = PeriodChar + Convert.ToString((mnt - 12));
                                            }

                                        }
                                        else
                                        {
                                            linkedPeriod = PeriodChar + Convert.ToString((mnt + 12));
                                        }
                                        if (!string.IsNullOrEmpty(linkedPeriod))
                                        {
                                            if (LinkedTactic.Plan_Campaign_Program_Tactic_Actual.Where(pcptc => pcptc.Period == linkedPeriod).Any())
                                            {
                                                LinkedTactic.Plan_Campaign_Program_Tactic_Actual.Where(pcptc => pcptc.Period == linkedPeriod).FirstOrDefault().Actualvalue = monthlycost;
                                            }
                                            else
                                            {
                                                Plan_Campaign_Program_Tactic_Actual objLinkedTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                                objLinkedTacticActual.PlanTacticId = linkedTacticId;
                                                objLinkedTacticActual.Period = linkedPeriod;
                                                objLinkedTacticActual.StageTitle = costStageTitle;
                                                objLinkedTacticActual.Actualvalue = monthlycost;
                                                objLinkedTacticActual.CreatedBy = Sessions.User.ID;
                                                objLinkedTacticActual.CreatedDate = DateTime.Now;
                                                db.Entry(objLinkedTacticActual).State = EntityState.Added;
                                            }
                                        }
                                    }
                                }


                                #endregion
                            }
                            objTactic.ModifiedBy = Sessions.User.ID;
                            objTactic.ModifiedDate = DateTime.Now;
                            db.Entry(objTactic).State = EntityState.Modified;
                        }
                    }
                    else if (string.Compare(section, Enums.Section.LineItem.ToString(), true) == 0)
                    {
                        if (tab == Enums.BudgetTab.Actual.ToString())
                        {
                            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
                            var objLineTactic = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt => pcpt.PlanLineItemId == EntityId && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
                            if (objLineTactic != null)
                            {
                                objTactic = objLineTactic.Plan_Campaign_Program_Tactic;
                            }
                            if (!isquarter)
                            {
                                if (objLineTactic.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(pcptc => pcptc.Period == period).Any())
                                {
                                    objLineTactic.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(pcptc => pcptc.Period == period).FirstOrDefault().Value = monthlycost;
                                }
                                else
                                {
                                    Plan_Campaign_Program_Tactic_LineItem_Actual objLineItemActual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                    objLineItemActual.PlanLineItemId = EntityId;
                                    objLineItemActual.Period = period;
                                    objLineItemActual.Value = monthlycost;
                                    objLineItemActual.CreatedBy = Sessions.User.ID;
                                    objLineItemActual.CreatedDate = DateTime.Now;
                                    db.Entry(objLineItemActual).State = EntityState.Added;
                                }

                                #region LinkedTactic updation

                                int linkedLineItemId = objLineTactic.LinkedLineItemId.GetValueOrDefault();
                                Plan_Campaign_Program_Tactic_LineItem LinkedLineItem = new Plan_Campaign_Program_Tactic_LineItem();
                                if (linkedLineItemId > 0)
                                {
                                    LinkedLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt => pcpt.PlanLineItemId == linkedLineItemId && pcpt.IsDeleted == false).FirstOrDefault();

                                    if (LinkedLineItem != null)
                                    {
                                        int yearDiff = LinkedLineItem.Plan_Campaign_Program_Tactic.EndDate.Year - LinkedLineItem.Plan_Campaign_Program_Tactic.StartDate.Year;
                                        bool isMultiYearLinkedTactic = yearDiff > 0 ? true : false;
                                        int mnthCounter = 12 * yearDiff;
                                        int mnt = Convert.ToInt32(period.Replace(PeriodChar, ""));
                                        string linkedPeriod = string.Empty;
                                        if (!isMultiYearLinkedTactic)
                                        {
                                            if ((mnt - 12) > 0)
                                            {
                                                linkedPeriod = PeriodChar + Convert.ToString((mnt - 12));
                                            }

                                        }
                                        else
                                        {
                                            linkedPeriod = PeriodChar + Convert.ToString((mnt + 12));
                                        }
                                        if (!string.IsNullOrEmpty(linkedPeriod))
                                        {
                                            if (LinkedLineItem.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(pcptc => pcptc.Period == linkedPeriod).Any())
                                            {
                                                LinkedLineItem.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(pcptc => pcptc.Period == linkedPeriod).FirstOrDefault().Value = monthlycost;
                                            }
                                            else
                                            {
                                                Plan_Campaign_Program_Tactic_LineItem_Actual objLinkedLineItem = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                                objLinkedLineItem.PlanLineItemId = linkedLineItemId;
                                                objLinkedLineItem.Period = linkedPeriod;
                                                objLinkedLineItem.Value = monthlycost;
                                                objLinkedLineItem.CreatedBy = Sessions.User.ID;
                                                objLinkedLineItem.CreatedDate = DateTime.Now;
                                                db.Entry(objLinkedLineItem).State = EntityState.Added;
                                            }
                                        }
                                    }
                                }


                                #endregion
                            }
                            else
                            {
                                //// Get Previous budget allocation data by PlanId.
                                var PrevTacticBudgetAllocationList = objLineTactic.Plan_Campaign_Program_Tactic_LineItem_Actual.Select(pb => pb).ToList();
                                int QuarterCnt = 1, j = 1;
                                bool isExists;
                                List<Plan_Campaign_Program_Tactic_LineItem_Actual> thisQuartersMonthList;
                                Plan_Campaign_Program_Tactic_LineItem_Actual thisQuarterFirstMonthBudget, objTacticBudget;
                                double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;

                                QuarterCnt = Convert.ToInt32(period.Replace(PeriodChar, ""));

                                //// Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                isExists = false;
                                if (PrevTacticBudgetAllocationList != null && PrevTacticBudgetAllocationList.Count > 0)
                                {
                                    //// Get current Quarter months budget.
                                    thisQuartersMonthList = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                                    thisQuartersMonthList = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                    thisQuarterFirstMonthBudget = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                    thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                    if (thisQuarterFirstMonthBudget != null)
                                    {
                                        //if (true)
                                        //{
                                        thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                        //// Get quarter total budget. 
                                        thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                        newValue = monthlycost;

                                        if (thisQuarterTotalBudget != newValue)
                                        {
                                            //// Get budget difference.
                                            BudgetDiff = newValue - thisQuarterTotalBudget;
                                            if (BudgetDiff > 0)
                                            {
                                                //// Set quarter first month budget value.
                                                thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                                db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                j = 1;
                                                while (thisQuarterFirstMonthBudget != null)
                                                {
                                                    //if ( == null)
                                                    // {
                                                    BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                    //Commented by Preet Shah on 10/12/2016 For allowed negative values PL #2850
                                                    //if (BudgetDiff <= 0)
                                                    //    thisQuarterFirstMonthBudget.Value = 0;
                                                    //else
                                                    if (j < 2)
                                                    {
                                                        thisQuarterFirstMonthBudget.Value = BudgetDiff;
                                                        db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                    }

                                                    //}
                                                    if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                    {
                                                        thisQuarterFirstMonthBudget = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                    }
                                                    else
                                                        thisQuarterFirstMonthBudget = null;

                                                    j = j + 1;
                                                }
                                            }
                                        }
                                        //}
                                        //else
                                        //{
                                        //    thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                        //}
                                        isExists = true;
                                    }
                                }
                                //// if previous values does not exist then insert new values.
                                if (!isExists)
                                {
                                    //// End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                    objTacticBudget = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                    objTacticBudget.PlanLineItemId = EntityId;
                                    objTacticBudget.Period = PeriodChar + QuarterCnt;
                                    objTacticBudget.Value = Convert.ToDouble(monthlycost);
                                    objTacticBudget.CreatedBy = Sessions.User.ID;
                                    objTacticBudget.CreatedDate = DateTime.Now;
                                    db.Entry(objTacticBudget).State = EntityState.Added;
                                }

                                #region LinkedTactic updation

                                int linkedLineItemId = objLineTactic.LinkedLineItemId.GetValueOrDefault();
                                Plan_Campaign_Program_Tactic_LineItem LinkedLineItem = new Plan_Campaign_Program_Tactic_LineItem();
                                if (linkedLineItemId > 0)
                                {
                                    LinkedLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt => pcpt.PlanLineItemId == linkedLineItemId && pcpt.IsDeleted == false).FirstOrDefault();

                                    if (LinkedLineItem != null)
                                    {
                                        int yearDiff = LinkedLineItem.Plan_Campaign_Program_Tactic.EndDate.Year - LinkedLineItem.Plan_Campaign_Program_Tactic.StartDate.Year;
                                        bool isMultiYearLinkedTactic = yearDiff > 0 ? true : false;
                                        int mnthCounter = 12 * yearDiff;
                                        int mnt = Convert.ToInt32(period.Replace(PeriodChar, ""));
                                        string linkedPeriod = string.Empty;
                                        if (!isMultiYearLinkedTactic)
                                        {
                                            if ((mnt - 12) > 0)
                                            {
                                                linkedPeriod = PeriodChar + Convert.ToString((mnt - 12));
                                            }

                                        }
                                        else
                                        {
                                            linkedPeriod = PeriodChar + Convert.ToString((mnt + 12));
                                        }
                                        if (!string.IsNullOrEmpty(linkedPeriod))
                                        {
                                            if (LinkedLineItem.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(pcptc => pcptc.Period == linkedPeriod).Any())
                                            {
                                                LinkedLineItem.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(pcptc => pcptc.Period == linkedPeriod).FirstOrDefault().Value = monthlycost;
                                            }
                                            else
                                            {
                                                Plan_Campaign_Program_Tactic_LineItem_Actual objLinkedLineItem = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                                objLinkedLineItem.PlanLineItemId = linkedLineItemId;
                                                objLinkedLineItem.Period = linkedPeriod;
                                                objLinkedLineItem.Value = monthlycost;
                                                objLinkedLineItem.CreatedBy = Sessions.User.ID;
                                                objLinkedLineItem.CreatedDate = DateTime.Now;
                                                db.Entry(objLinkedLineItem).State = EntityState.Added;
                                            }
                                        }
                                    }
                                }


                                #endregion
                            }
                            if (objTactic != null && objTactic.PlanTacticId > 0)
                            {
                                objTactic.ModifiedBy = Sessions.User.ID;
                                objTactic.ModifiedDate = DateTime.Now;
                                db.Entry(objTactic).State = EntityState.Modified;
                            }
                            db.Entry(objLineTactic).State = EntityState.Modified;
                        }
                    }
                }
                db.SaveChanges();
                return Json(new { budgetMonth = monthlycost, budgetYear = yearlycost, isSuccess = true });
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { isSuccess = false });
        }

        /// <summary>
        /// <createdby>Nandish Shah</createdby>
        /// Get Plan Year based on Section
        /// </summary>
        /// <returns>jPlane Year</returns>
        private string GetPlanYear(string section, int EntityId)
        {
            string PlanYear = string.Empty;
            if (string.Compare(section, Enums.Section.Plan.ToString(), true) == 0)
            {
                PlanYear = db.Plans.Where(p => p.PlanId == EntityId && p.IsDeleted.Equals(false)).Select(p => p.Year).FirstOrDefault();
            }
            else if (string.Compare(section, Enums.Section.Campaign.ToString(), true) == 0)
            {
                PlanYear = (from pc in db.Plan_Campaign
                            join p in db.Plans on pc.PlanId equals p.PlanId
                            where pc.PlanCampaignId == EntityId && p.IsDeleted == false && pc.IsDeleted == false
                            select p.Year).FirstOrDefault();
            }
            else if (string.Compare(section, Enums.Section.Program.ToString(), true) == 0)
            {
                PlanYear = (from pcp in db.Plan_Campaign_Program
                            join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                            join p in db.Plans on pc.PlanId equals p.PlanId
                            where pcp.PlanProgramId == EntityId && p.IsDeleted == false && pc.IsDeleted == false
                            select p.Year).FirstOrDefault();
            }
            else if (string.Compare(section, Enums.Section.Tactic.ToString(), true) == 0)
            {
                PlanYear = (from pcpt in db.Plan_Campaign_Program_Tactic
                            join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                            join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                            join p in db.Plans on pc.PlanId equals p.PlanId
                            where pcpt.PlanTacticId == EntityId && p.IsDeleted == false && pc.IsDeleted == false
                            select p.Year).FirstOrDefault();
            }
            else if (string.Compare(section, Enums.Section.LineItem.ToString(), true) == 0)
            {
                PlanYear = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                            join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                            join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                            join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                            join p in db.Plans on pc.PlanId equals p.PlanId
                            where pcptl.PlanLineItemId == EntityId && p.IsDeleted == false && pc.IsDeleted == false
                            select p.Year).FirstOrDefault();
            }
            return PlanYear;
        }
        #endregion

        #region Budget Review Section
        /// <summary>
        /// View fro the initial render page 
        /// </summary>
        /// <returns></returns>
        public ActionResult Budgeting(int PlanId = 0, bool isGridView = false, int selectedid = 0) // Added by Komal Rawal for 2013 to identify grid view // add selectedid by devanshi for pl #2213
        {
            // Added by Arpita Soni for Ticket #2202 on 05/24/2016 
            if (PlanId == 0)
            {
                //Modified By Komal rawal for #2283 if session plan id is 0 then get plan from last view data or the first plan of the year.
                if (Sessions.PlanId == 0)
                {
                    var Label = Enums.FilterLabel.Plan.ToString();
                    var FinalSetOfPlanSelected = "";
                    var LastSetOfPlanSelected = new List<string>();
                    var SetOFLastViews = db.Plan_UserSavedViews.Where(listview => listview.Userid == Sessions.User.ID).ToList();
                    var SetOfPlanSelected = SetOFLastViews.Where(listview => listview.FilterName == Label && listview.Userid == Sessions.User.ID).ToList();

                    FinalSetOfPlanSelected = SetOfPlanSelected.Where(view => view.IsDefaultPreset == true).Select(listview => listview.FilterValues).FirstOrDefault();
                    if (FinalSetOfPlanSelected == null)
                    {
                        FinalSetOfPlanSelected = SetOfPlanSelected.Where(view => view.ViewName == null).Select(listview => listview.FilterValues).FirstOrDefault();
                    }
                    if (FinalSetOfPlanSelected != null)
                    {
                        LastSetOfPlanSelected = FinalSetOfPlanSelected.Split(',').ToList();
                    }
                    if (LastSetOfPlanSelected.Count > 0 && LastSetOfPlanSelected != null)
                    {
                        PlanId = Convert.ToInt32(LastSetOfPlanSelected.FirstOrDefault());
                    }
                    if (PlanId == 0)
                    {
                        List<Plan> tblPlan = db.Plans.Where(plan => plan.IsDeleted == false && plan.Model.ClientId == Sessions.User.CID).ToList();

                        string year = Convert.ToString(DateTime.Now.Year);
                        var checkcurrentplan = tblPlan.Where(plan => !plan.IsDeleted && plan.Year == year).OrderBy(p => p.Title).FirstOrDefault();
                        if (checkcurrentplan != null)
                        {
                            PlanId = tblPlan.OrderBy(p => p.Title).FirstOrDefault().PlanId;
                        }
                        else
                        {
                            PlanId = tblPlan.OrderByDescending(p => Convert.ToInt32(p.Year)).OrderBy(p => p.Title).ToList().FirstOrDefault().PlanId;
                        }
                    }

                }
                else
                {
                    PlanId = Sessions.PlanId;
                }
                //End

            }
            ViewBag.ActiveMenu = Enums.ActiveMenu.Finance;
            HomePlanModel planmodel = new Models.HomePlanModel();
            try
            {
                bool IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
                ViewBag.IsPlanCreateAuthorized = IsPlanCreateAuthorized;
                var objPlan = db.Plans.FirstOrDefault(_plan => _plan.PlanId == PlanId);
                var IsQuarter = objPlan.Year;
                ViewBag.PlanId = PlanId;
                ViewBag.GridView = isGridView; // Added by Komal Rawal for 2013 to identify grid view

                bool IsPlanCreateAll = false;
                if (IsPlanCreateAuthorized)
                {
                    IsPlanCreateAll = true;
                }
                else
                {
                    if ((objPlan.CreatedBy.Equals(Sessions.User.ID)))
                    {
                        IsPlanCreateAll = true;
                    }
                }


                //// Set ViewBy list.
                string entTacticType = Enums.EntityType.Tactic.ToString();
                List<ViewByModel> lstViewBy = new List<ViewByModel>();
                lstViewBy.Add(new ViewByModel { Text = "Campaigns", Value = "0" });
                List<CustomField> lstTacticCustomfield = db.CustomFields.Where(custom => custom.IsDeleted.Equals(false) && custom.EntityType.Equals(entTacticType) && custom.ClientId.Equals(Sessions.User.CID) && custom.IsDisplayForFilter.Equals(true)).ToList();
                if (lstTacticCustomfield != null && lstTacticCustomfield.Count > 0)
                {
                    lstTacticCustomfield = lstTacticCustomfield.Where(s => !string.IsNullOrEmpty(s.Name)).OrderBy(s => s.Name, new AlphaNumericComparer()).ToList();
                    lstTacticCustomfield.ForEach(custom => { lstViewBy.Add(new ViewByModel { Text = custom.Name, Value = TacticCustomTitle + custom.CustomFieldId.ToString() }); });
                }
                ViewBag.ViewBy = lstViewBy;

                #region BudgetingPlanList
                //Modified By Komal Rawal for new homepage UI

                //HomePlan objHomePlan = new HomePlan();
                //List<SelectListItem> planList;
                ////if (Bid == "false")
                ////{
                //planList = Common.GetPlan().Select(_pln => new SelectListItem() { Text = _pln.Title, Value = _pln.PlanId.ToString() }).OrderBy(_pln => _pln.Text).ToList();
                //if (planList.Count > 0)
                //{
                //    var objexists = planList.Where(_pln => _pln.Value == Sessions.PlanId.ToString()).ToList();
                //    if (objexists.Count != 0)
                //    {
                //        planList.FirstOrDefault(_pln => _pln.Value.Equals(Sessions.PlanId.ToString())).Selected = true;
                //    }
                //    /*changed by Nirav for plan consistency on 14 apr 2014*/

                //    if (!Common.IsPlanPublished(Sessions.PlanId))
                //    {
                //        string planPublishedStatus = Enums.PlanStatus.Published.ToString();
                //        var activeplan = db.Plans.Where(_pln => _pln.PlanId == Sessions.PlanId && _pln.IsDeleted == false && _pln.Status == planPublishedStatus).ToList();
                //        if (activeplan.Count > 0)
                //            Sessions.PublishedPlanId = Sessions.PlanId;
                //        else
                //            Sessions.PublishedPlanId = 0;
                //    }
                //}
                //objHomePlan.plans = planList;

                planmodel.objplanhomemodelheader = Common.GetPlanHeaderValue(PlanId, onlyplan: true);
                ViewBag.IsPlanCreateAll = IsPlanCreateAll;
                ViewBag.IsQuarter = IsQuarter;
                //added by devanshi for PL #2213
                ViewBag.SelectedId = selectedid.ToString();
                #endregion
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
            }
            return View(planmodel);

        }
        #endregion

        #region "Actuals Tab of LineItem related functions"

        /// <summary>
        /// Added By: Viral Kadiya.
        /// Action to Get Actuals cost Value Of line item.
        /// </summary>
        /// <param name="planLineItemId">Plan line item Id.</param>
        /// <returns>Returns Json Result of line item actuals Value.</returns>
        public JsonResult GetActualsLineitemData(int planLineItemId)
        {
            try
            {
                string modifiedBy = string.Empty;
                modifiedBy = Common.ActualLineItemModificationMessageByPlanLineItemId(planLineItemId);
                string strLastUpdatedBy = modifiedBy != string.Empty ? modifiedBy : null;
                PlanExchangeRate = Sessions.PlanExchangeRate;
                //// Get LineItem monthly Actual data.
                List<string> lstActualAllocationMonthly = Common.lstMonthly;
                // Add By Nishant Sheth 
                // Desc:: for add multiple years regarding #1765
                // To create the period of the year dynamically base on tactic period
                var lstActualLineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(actl => actl.PlanLineItemId.Equals(planLineItemId)).ToList();

                int GlobalYearDiffrence = 0;
                List<listMonthDynamic> lstMonthlyDynamicActual = new List<listMonthDynamic>();

                lstActualLineItemActualList.ForEach(lineItem =>
                {
                    List<string> lstMonthlyExtended = new List<string>();
                    int YearDiffrence = Convert.ToInt32(Convert.ToInt32(lineItem.Plan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.EndDate.Year) - Convert.ToInt32(lineItem.Plan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.StartDate.Year));
                    if (lineItem.PlanLineItemId == planLineItemId)
                    {
                        GlobalYearDiffrence = YearDiffrence;
                    }
                    string periodPrefix = "Y";
                    int baseYear = 0;
                    for (int i = 0; i < (YearDiffrence + 1); i++)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            lstMonthlyExtended.Add(periodPrefix + Convert.ToString(j + baseYear));
                        }
                        baseYear = baseYear + 12;
                    }
                    lstMonthlyDynamicActual.Add(new listMonthDynamic { Id = lineItem.PlanLineItemId, listMonthly = lstMonthlyExtended });
                });

                if (lstActualLineItemActualList.Count == 0)
                {
                    var LineItemTactic = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.PlanLineItemId == planLineItemId).FirstOrDefault();
                    List<string> lstMonthlyExtended = new List<string>();
                    int YearDiffrence = Convert.ToInt32(Convert.ToInt32(LineItemTactic.Plan_Campaign_Program_Tactic.EndDate.Year) - Convert.ToInt32(LineItemTactic.Plan_Campaign_Program_Tactic.StartDate.Year));
                    if (LineItemTactic.PlanLineItemId == planLineItemId)
                    {
                        GlobalYearDiffrence = YearDiffrence;
                    }
                    string periodPrefix = "Y";
                    int baseYear = 0;
                    for (int i = 0; i < (YearDiffrence + 1); i++)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            lstMonthlyExtended.Add(periodPrefix + Convert.ToString(j + baseYear));
                        }
                        baseYear = baseYear + 12;
                    }
                    lstMonthlyDynamicActual.Add(new listMonthDynamic { Id = LineItemTactic.PlanLineItemId, listMonthly = lstMonthlyExtended });
                }

                var lstActualLineItemCost = lstActualLineItemActualList.Where(actl => actl.PlanLineItemId.Equals(planLineItemId)).ToList()
                    .Where(actl => lstMonthlyDynamicActual.Where(a => a.Id == actl.PlanLineItemId).Select(a => a.listMonthly).FirstOrDefault().Contains(actl.Period))
                                                              .Select(actl => new
                                                              {
                                                                  actl.PlanLineItemId,
                                                                  actl.Period,
                                                                  actl.Value
                                                              }).ToList();

                //Set the custom array for fecthed Line item Actual data .
                // Change by Nishant sheth
                // Desc :: #1765 - add period condition to get value
                var ActualCostData = lstMonthlyDynamicActual.Where(a => a.Id == planLineItemId).Select(a => a.listMonthly).FirstOrDefault().Select(period => new
                {
                    periodTitle = period,
                    costValue = lstActualLineItemCost.FirstOrDefault(c => c.Period == period && c.PlanLineItemId == planLineItemId) == null ? "" : objCurrency.GetValueByExchangeRate(lstActualLineItemCost.FirstOrDefault(lineCost => lineCost.Period == period && lineCost.PlanLineItemId == planLineItemId).Value, PlanExchangeRate).ToString() //Modified by Rahul Shah for PL #2511 to apply multi currency

                });
                var projectedData = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(line => line.PlanLineItemId.Equals(planLineItemId));
                double projectedval = projectedData != null ? objCurrency.GetValueByExchangeRate(projectedData.Cost, PlanExchangeRate) : 0; //Modified by Rahul Shah for PL #2511 to apply multi currency

                var returndata = new { ActualData = ActualCostData, ProjectedValue = projectedval, LastUpdatedBy = strLastUpdatedBy };

                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Viral Kadiya.
        /// Action to Get Actuals cost Value Of line item.
        /// </summary>
        /// <param name="strActualsData">Actual Data</param>
        /// <param name="strPlanItemId">Plan line item Id.</param>
        /// <param name="LineItemTitle">LineItem Title</param>
        /// <returns>Returns Json Result of line item actuals Value.</returns>
        public JsonResult SaveActualsLineitemData(string strActualsData, string strPlanItemId, string LineItemTitle = "")
        {
            string[] arrActualCostInputValues = strActualsData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int PlanLineItemId = int.Parse(strPlanItemId);
            int tid = db.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanLineItemId == PlanLineItemId).FirstOrDefault().PlanTacticId;
            PlanExchangeRate = Sessions.PlanExchangeRate;
            //Added By Komal Rawal for LinkedLineItem change PL ticket #1853
            Plan_Campaign_Program_Tactic_LineItem Lineitemobj = new Plan_Campaign_Program_Tactic_LineItem();
            Plan_Campaign_Program_Tactic ObjLinkedTactic = new Plan_Campaign_Program_Tactic();

            var LinkedLineitemId = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.PlanLineItemId == PlanLineItemId && id.IsDeleted == false).Select(id => id.LinkedLineItemId).FirstOrDefault();

            int? LinkedTacticId = null;
            if (LinkedLineitemId != null)
            {
                Lineitemobj = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.PlanLineItemId == LinkedLineitemId && id.IsDeleted == false).ToList().FirstOrDefault();
                LinkedTacticId = Lineitemobj.Plan_Campaign_Program_Tactic.PlanTacticId;
                ObjLinkedTactic = Lineitemobj.Plan_Campaign_Program_Tactic;

            }
            //else
            //{
            //    Lineitemobj = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.LinkedLineItemId == PlanLineItemId && id.IsDeleted == false).ToList().FirstOrDefault();
            //    if (Lineitemobj != null)
            //    {

            //        LinkedLineitemId = Lineitemobj.PlanLineItemId;
            //        LinkedTacticId = Lineitemobj.Plan_Campaign_Program_Tactic.LinkedTacticId;

            //    }

            //}


            //End



            //// Get Duplicate record to check duplication.
            var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                           join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                           join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                           join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                           where pcptl.Title.Trim().ToLower().Equals(LineItemTitle.Trim().ToLower()) && !pcptl.PlanLineItemId.Equals(PlanLineItemId) && pcptl.IsDeleted.Equals(false)
                                           && pcpt.PlanTacticId == tid
                           select pcpt).FirstOrDefault();

            //Check duplicate for Linked LineItem 
            //Added By Komal Rawal for PL ticket #1853
            //var LinkedLi = new Plan_Campaign_Program_Tactic();
            //if (LinkedTacticId != null && LinkedTacticId.HasValue)
            //{
            //    LinkedLi = (from pcptli in db.Plan_Campaign_Program_Tactic_LineItem
            //                join pcpt in db.Plan_Campaign_Program_Tactic on pcptli.PlanTacticId equals pcpt.PlanTacticId
            //                join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
            //                join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
            //                where pcptli.Title.Trim().ToLower().Equals(LineItemTitle.Trim().ToLower()) && pcptli.PlanLineItemId != LinkedLineitemId.Value && pcptli.IsDeleted.Equals(false)
            //                                && pcpt.PlanTacticId == LinkedTacticId.Value
            //                select pcpt).FirstOrDefault();

            //}
            //else
            //{
            //    LinkedLi = null;
            //}
            //End

            //// if duplicate record exist then return Duplicate message.
            if (pcptvar != null)
            {
                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                //string strduplicatedlinkedlineitem = "";
                //if (LinkedLi != null)
                //{
                //    strduplicatedlinkedlineitem = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()] + " in the linkedtactic");    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                //}

                return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
            }
            else
            {
                //// Update LineItem Data to Plan_Campaign_Program_Tactic_LineItem table.
                Plan_Campaign_Program_Tactic_LineItem objPCPTL = db.Plan_Campaign_Program_Tactic_LineItem.Where(c => c.PlanLineItemId == PlanLineItemId).FirstOrDefault();
                if (objPCPTL != null && !string.IsNullOrEmpty(LineItemTitle))
                {
                    objPCPTL.Title = LineItemTitle;
                    objPCPTL.ModifiedBy = Sessions.User.ID;
                    objPCPTL.ModifiedDate = DateTime.Now;
                    db.Entry(objPCPTL).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (LinkedLineitemId != null && !string.IsNullOrEmpty(LineItemTitle))
                {
                    Lineitemobj.Title = LineItemTitle;
                    Lineitemobj.ModifiedBy = Sessions.User.ID;
                    Lineitemobj.ModifiedDate = DateTime.Now;
                    db.Entry(Lineitemobj).State = EntityState.Modified;
                    db.SaveChanges();

                }
                var PrevActualAllocationListTactics = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(actl => actl.PlanLineItemId == PlanLineItemId).Select(actl => actl).ToList();
                if (PrevActualAllocationListTactics != null && PrevActualAllocationListTactics.Count > 0)
                {
                    PrevActualAllocationListTactics.ForEach(_tac => db.Entry(_tac).State = EntityState.Deleted);
                    int result = db.SaveChanges();
                }
                //Insert actual cost of tactic
                int saveresult = 0;
                bool isdataupdate = false;
                for (int i = 0; i < arrActualCostInputValues.Length; i++)
                {
                    //// Insert LineItem Actual data to Plan_Campaign_Program_Tactic_LineItem_Actual table.
                    if (!string.IsNullOrWhiteSpace(arrActualCostInputValues[i]))
                    {
                        Plan_Campaign_Program_Tactic_LineItem_Actual obPlanCampaignProgramTacticActual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                        obPlanCampaignProgramTacticActual.PlanLineItemId = PlanLineItemId;
                        obPlanCampaignProgramTacticActual.Period = PeriodChar + (i + 1);
                        obPlanCampaignProgramTacticActual.Value = objCurrency.SetValueByExchangeRate(Convert.ToDouble(arrActualCostInputValues[i]), PlanExchangeRate); //Modified by Rahul Shah for PL #2511 to apply multi currency
                        obPlanCampaignProgramTacticActual.CreatedBy = Sessions.User.ID;
                        obPlanCampaignProgramTacticActual.CreatedDate = DateTime.Now;
                        db.Entry(obPlanCampaignProgramTacticActual).State = EntityState.Added;
                        isdataupdate = true;
                    }
                }
                saveresult = db.SaveChanges();

                //Added By Komal Rawal for LinkedLineItem change PL ticket #1853
                if (LinkedLineitemId != null)
                {
                    #region "Update Linked Tactic Actual data"
                    List<Plan_Campaign_Program_Tactic_LineItem_Actual> tblLineItemData = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(id => (id.PlanLineItemId == PlanLineItemId) || (id.PlanLineItemId == LinkedLineitemId)).ToList();
                    List<Plan_Campaign_Program_Tactic_LineItem_Actual> lstLineItemData = tblLineItemData.Where(id => id.PlanLineItemId == PlanLineItemId).ToList();

                    var yearDiff = ObjLinkedTactic.EndDate.Year - ObjLinkedTactic.StartDate.Year;
                    var isMultiYearlinkedTactic = yearDiff > 0 ? true : false;
                    int cntr = 0, perdNum = 12;
                    List<string> lstLinkedPeriods = new List<string>();
                    if (lstLineItemData != null && lstLineItemData.Count > 0)
                    {
                        if (isMultiYearlinkedTactic)
                        {
                            cntr = 12 * yearDiff;
                            for (int i = 1; i <= cntr; i++)
                            {
                                lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
                            }
                            List<Plan_Campaign_Program_Tactic_LineItem_Actual> linkedLineItemData = tblLineItemData.Where(id => id.PlanLineItemId == LinkedLineitemId && lstLinkedPeriods.Contains(id.Period)).ToList();
                            if (linkedLineItemData != null && linkedLineItemData.Count > 0)
                                linkedLineItemData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                        }
                        else
                        {
                            List<Plan_Campaign_Program_Tactic_LineItem_Actual> linkedLineItemData = tblLineItemData.Where(id => id.PlanLineItemId == LinkedLineitemId).ToList();
                            if (linkedLineItemData != null && linkedLineItemData.Count > 0)
                                linkedLineItemData.ForEach(bdgt => db.Entry(bdgt).State = EntityState.Deleted);
                        }

                        Plan_Campaign_Program_Tactic_LineItem_Actual objlinkedActual = null;
                        foreach (Plan_Campaign_Program_Tactic_LineItem_Actual item in lstLineItemData)
                        {
                            string orgPeriod = item.Period;
                            string numPeriod = orgPeriod.Replace(PeriodChar, string.Empty);
                            int NumPeriod = int.Parse(numPeriod);
                            if (isMultiYearlinkedTactic)
                            {
                                //PeriodChar + ((12 * yearDiff) + int.Parse(numPeriod)).ToString();   // (12*1)+3 = 15 => For March(Y15) month.
                                objlinkedActual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                objlinkedActual.PlanLineItemId = Convert.ToInt32(LinkedLineitemId);
                                objlinkedActual.Period = PeriodChar + ((12 * yearDiff) + NumPeriod).ToString();
                                objlinkedActual.Value = item.Value;
                                objlinkedActual.CreatedBy = item.CreatedBy;
                                objlinkedActual.CreatedDate = item.CreatedDate;
                                db.Entry(objlinkedActual).State = EntityState.Added;
                                db.Plan_Campaign_Program_Tactic_LineItem_Actual.Add(objlinkedActual);
                            }
                            else
                            {
                                if (NumPeriod > 12)
                                {
                                    int rem = NumPeriod % 12;    // For March, Y3(i.e 15%12 = 3)  
                                    int div = NumPeriod / 12;    // In case of 24, Y12.
                                    if (rem > 0 || div > 1)
                                    {
                                        objlinkedActual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                        objlinkedActual.PlanLineItemId = Convert.ToInt32(LinkedLineitemId);
                                        objlinkedActual.Period = PeriodChar + (div > 1 ? "12" : rem.ToString());
                                        objlinkedActual.Value = item.Value;
                                        objlinkedActual.CreatedBy = item.CreatedBy;
                                        objlinkedActual.CreatedDate = item.CreatedDate;
                                        db.Entry(objlinkedActual).State = EntityState.Added;
                                        db.Plan_Campaign_Program_Tactic_LineItem_Actual.Add(objlinkedActual);
                                    }
                                }
                            }
                        }


                        db.SaveChanges();


                    }
                    #endregion

                }
                //End
                int pid = db.Plan_Campaign_Program_Tactic.Where(_tac => _tac.PlanTacticId == tid).FirstOrDefault().PlanProgramId;
                int cid = db.Plan_Campaign_Program.Where(_prgrm => _prgrm.PlanProgramId == pid).FirstOrDefault().PlanCampaignId;
                if (saveresult > 0 || !isdataupdate)
                {
                    string strMessage = Common.objCached.PlanEntityActualsUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                    return Json(new { id = strPlanItemId, TabValue = "Actuals", msg = strMessage, planCampaignID = cid, planProgramID = pid, planTacticID = tid });
                }
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        /// <summary>
        /// Function to prepare list of custom fields and customfield options
        /// </summary>
        /// <param name="customFieldListOut"></param>
        /// <param name="customFieldOptionsListOut"></param>
        public void GetCustomFieldAndOptions(out List<CustomFieldsForFilter> customFieldListOut, out List<CustomFieldsForFilter> customFieldOptionsListOut)
        {
            customFieldListOut = new List<CustomFieldsForFilter>();
            customFieldOptionsListOut = new List<CustomFieldsForFilter>();

            //// Get list of custom fields
            string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
            string EntityTypeTactic = Enums.EntityType.Tactic.ToString();
            var lstCustomField = db.CustomFields.Where(customField => customField.ClientId == Sessions.User.CID && customField.IsDeleted.Equals(false) &&
                                                                customField.EntityType.Equals(EntityTypeTactic) && customField.CustomFieldType.Name.Equals(DropDownList) &&
                                                                customField.IsDisplayForFilter.Equals(true) && customField.CustomFieldOptions.Count() > 0)
                                                                .Select(customField => new
                                                                {
                                                                    customField.Name,
                                                                    customField.CustomFieldId
                                                                }).ToList();

            List<int> lstCustomFieldId = new List<int>();

            if (lstCustomField.Count > 0)
            {
                //// Get list of Custom Field Ids
                lstCustomFieldId = lstCustomField.Select(customField => customField.CustomFieldId).Distinct().ToList();

                //// Get list of custom field options
                var lstCustomFieldOptions = db.CustomFieldOptions
                                                            .Where(customFieldOption => lstCustomFieldId.Contains(customFieldOption.CustomFieldId) && customFieldOption.IsDeleted == false)
                                                            .Select(customFieldOption => new
                                                            {
                                                                customFieldOption.CustomFieldId,
                                                                customFieldOption.CustomFieldOptionId,
                                                                customFieldOption.Value
                                                            }).ToList();

                //// Get default custom restriction value
                bool IsDefaultCustomRestrictionsViewable = Common.IsDefaultCustomRestrictionsViewable();

                //// Custom Restrictions
                var userCustomRestrictionList = Common.GetUserCustomRestrictionsList(Sessions.User.ID, true);   //// Modified by Sohel Pathan on 15/01/2015 for PL ticket #1139

                if (userCustomRestrictionList.Count() > 0)
                {
                    int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
                    int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                    int NonePermission = (int)Enums.CustomRestrictionPermission.None;

                    foreach (var customFieldId in lstCustomFieldId)
                    {
                        if (userCustomRestrictionList.Where(customRestriction => customRestriction.CustomFieldId == customFieldId).Count() > 0)
                        {
                            //// Prepare list of allowed Custom Field Option Ids
                            List<int> lstAllowedCustomFieldOptionIds = userCustomRestrictionList.Where(customRestriction => customRestriction.CustomFieldId == customFieldId &&
                                                                                                    (customRestriction.Permission == ViewOnlyPermission || customRestriction.Permission == ViewEditPermission))
                                                                                                .Select(customRestriction => customRestriction.CustomFieldOptionId).ToList();

                            List<int> lstRestrictedCustomFieldOptionIds = userCustomRestrictionList.Where(customRestriction => customRestriction.CustomFieldId == customFieldId &&
                                                                                                            customRestriction.Permission == NonePermission)
                                                                                                    .Select(customRestriction => customRestriction.CustomFieldOptionId).ToList();

                            //// Get list of custom field options
                            var lstAllowedCustomFieldOption = lstCustomFieldOptions.Where(customFieldOption => customFieldOption.CustomFieldId == customFieldId &&
                                                                                        lstAllowedCustomFieldOptionIds.Contains(customFieldOption.CustomFieldOptionId))
                                                                                    .Select(customFieldOption => new
                                                                                    {
                                                                                        customFieldOption.CustomFieldId,
                                                                                        customFieldOption.CustomFieldOptionId,
                                                                                        customFieldOption.Value
                                                                                    }).ToList();

                            if (lstAllowedCustomFieldOption.Count > 0)
                            {
                                customFieldListOut.AddRange(lstCustomField.Where(customField => customField.CustomFieldId == customFieldId)
                                                                        .Select(customField => new CustomFieldsForFilter()
                                                                        {
                                                                            CustomFieldId = customField.CustomFieldId,
                                                                            Title = customField.Name
                                                                        }).ToList());

                                customFieldOptionsListOut.AddRange(lstAllowedCustomFieldOption.Select(customFieldOption => new CustomFieldsForFilter()
                                {
                                    CustomFieldId = customFieldOption.CustomFieldId,
                                    CustomFieldOptionId = customFieldOption.CustomFieldOptionId,
                                    Title = customFieldOption.Value
                                }).ToList());

                            }

                            //// If default custom restiction is viewable then select custom field and option who dont have restrictions
                            if (IsDefaultCustomRestrictionsViewable)
                            {
                                var lstNewCustomFieldOptions = lstCustomFieldOptions.Where(option => !lstAllowedCustomFieldOptionIds.Contains(option.CustomFieldOptionId) && !lstRestrictedCustomFieldOptionIds.Contains(option.CustomFieldOptionId) && option.CustomFieldId == customFieldId).ToList();
                                if (lstNewCustomFieldOptions.Count() > 0)
                                {
                                    //// If CustomField is not added in out list then add it
                                    if (!(customFieldListOut.Where(customField => customField.CustomFieldId == customFieldId).Any()))
                                    {
                                        customFieldListOut.AddRange(lstCustomField.Where(customField => customField.CustomFieldId == customFieldId)
                                                                                    .Select(customField => new CustomFieldsForFilter()
                                                                                    {
                                                                                        CustomFieldId = customField.CustomFieldId,
                                                                                        Title = customField.Name
                                                                                    }).ToList());
                                    }

                                    ///// Add custom field options that are not in Custom Restriction list
                                    customFieldOptionsListOut.AddRange(lstNewCustomFieldOptions.Select(customFieldOption => new CustomFieldsForFilter()
                                    {
                                        CustomFieldId = customFieldOption.CustomFieldId,
                                        CustomFieldOptionId = customFieldOption.CustomFieldOptionId,
                                        Title = customFieldOption.Value
                                    }).ToList());
                                }
                            }

                        }
                        else if (IsDefaultCustomRestrictionsViewable)
                        {
                            if (lstCustomFieldOptions.Where(option => option.CustomFieldId == customFieldId).Count() > 0)
                            {
                                customFieldListOut.AddRange(lstCustomField.Where(customField => customField.CustomFieldId == customFieldId).Select(customField => new CustomFieldsForFilter()
                                {
                                    CustomFieldId = customField.CustomFieldId,
                                    Title = customField.Name
                                }).ToList());

                                customFieldOptionsListOut.AddRange(lstCustomFieldOptions.Where(option => option.CustomFieldId == customFieldId).Select(customFieldOption => new CustomFieldsForFilter()
                                {
                                    CustomFieldId = customFieldOption.CustomFieldId,
                                    CustomFieldOptionId = customFieldOption.CustomFieldOptionId,
                                    Title = customFieldOption.Value
                                }).ToList());
                            }
                        }
                    }
                }
                else if (IsDefaultCustomRestrictionsViewable)
                {
                    //// If default custom restriction is viewable than add all custom fields for search filter
                    customFieldListOut = lstCustomField.Select(customField => new CustomFieldsForFilter()
                    {
                        CustomFieldId = customField.CustomFieldId,
                        Title = customField.Name
                    }).ToList();

                    customFieldOptionsListOut = lstCustomFieldOptions.Select(customFieldOption => new CustomFieldsForFilter()
                    {
                        CustomFieldId = customFieldOption.CustomFieldId,
                        CustomFieldOptionId = customFieldOption.CustomFieldOptionId,
                        Title = customFieldOption.Value
                    }).ToList();
                }
            }

            if (customFieldListOut.Count() > 0)
            {
                ////// Sort custom fields by name
                customFieldListOut = customFieldListOut.OrderBy(customField => customField.Title).ToList();
            }
            if (customFieldOptionsListOut.Count() > 0)
            {
                ////// Sort custom field option list by value and custom field id
                customFieldOptionsListOut = customFieldOptionsListOut.OrderBy(customFieldOption => customFieldOption.CustomFieldId).ThenBy(customFieldOption => customFieldOption.Title).ToList();
            }
        }

        #region load Gridview for home
        /// <summary>
        /// Action method to load editable gridview for plan.
        /// </summary>
        /// <CreatedBy>Rahul Shah</CreatedBy> 
        /// <CreatedDate>24/09/2016</CreatedDate>
        /// <param name="planIds">plan Ids</param>
        /// <param name="ownerIds">owner Ids</param>
        /// <param name="TacticTypeid">TacticType Ids</param>
        /// <param name="StatusIds">Status Ids</param>
        /// <param name="customFieldIds">customField Ids</param>
        /// <returns>returns partial view HomeGrid</returns>
        [CompressAttribute]
        public ActionResult GetHomeGridData()
        {
            return PartialView("_HomeGrid");
        }

        [CompressAttribute]
        public JsonResult GetHomeGridDataJSON(string planIds, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds, string viewBy, bool isLoginFirst = false, string SearchText = "",string SearchBy = "", bool IsFromCache = false)// pass parameter IsFromCache to search the grid data using cache
        {
            PlanMainDHTMLXGridHomeGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGridHomeGrid();
            List<PlanOptionsTacticType> Tactictypelist = new List<PlanOptionsTacticType>();
            List<PlanOptionsTacticType> LineItemtypelist = new List<PlanOptionsTacticType>();
            bool UserSaveView = false;
            try
            {
                #region Set Permission
                EntityPermission objPermission = new EntityPermission();
                List<BDSService.UserHierarchy> lstUserHierarchy = new List<BDSService.UserHierarchy>();
                if (Sessions.UserHierarchyList != null)
                {
                    lstUserHierarchy = Sessions.UserHierarchyList;
                }
                else
                {
                    lstUserHierarchy = objBDSServiceClient.GetUserHierarchyEx(Sessions.User.CID, Sessions.ApplicationId);
                }
               

                List<int> lstSubordinatesIds = Common.GetAllSubordinateslist(lstUserHierarchy, Sessions.User.ID);

                objPermission.PlanCreate = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
                objPermission.PlanEditAll = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                objPermission.PlanEditSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                #endregion

                if (string.IsNullOrEmpty(viewBy))
                    viewBy = PlanGanttTypes.Tactic.ToString();
                if (isLoginFirst)
                {
                    if (Sessions.PlanUserSavedViews != null && Sessions.PlanUserSavedViews.Count > 0)
                    {
                        UserSaveView = true;
                    }

                    PlanGridFilters objFilter = objGrid.GetGridFilterData(Sessions.User.CID, Sessions.User.ID, Sessions.FilterPresetName, UserSaveView).FirstOrDefault();
                    
                    planIds = objFilter.PlanIds;
                    ownerIds = objFilter.OwnerIds;
                    TacticTypeid = objFilter.TacticTypeIds;
                    StatusIds = objFilter.StatusIds;
                    customFieldIds = objFilter.CustomFieldIds;
                }
                int ClientID = Sessions.User.CID;
                string PlanCurrencySymbol = Sessions.PlanCurrencySymbol;
                double PlanExchangeRate = Sessions.PlanExchangeRate;
                int UserID = Sessions.User.ID;
                objPlanMainDHTMLXGrid = objGrid.GetPlanGrid(planIds, ClientID, ownerIds, TacticTypeid, StatusIds, customFieldIds, PlanCurrencySymbol, PlanExchangeRate, UserID, objPermission, lstSubordinatesIds, viewBy, SearchText,SearchBy, IsFromCache);
                Sessions.FilterPresetName = null; // Make null default Filter Preset name hence plan grid load.
                Tactictypelist = objGrid.GetTacticTypeListForHeader(planIds, Sessions.User.CID);
                LineItemtypelist = objGrid.GetLineItemTypeListForHeader(planIds, Sessions.User.CID);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            var jsonResult = Json(new { data = objPlanMainDHTMLXGrid, tType = Tactictypelist, lType = LineItemtypelist }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
            //return Json(new { result = objPlanMainDHTMLXGrid }, JsonRequestBehavior.AllowGet);
            //return PartialView("_HomeGrid", objPlanMainDHTMLXGrid);
        }
        #endregion
       
        #region Save gridview detail from home
        /// <summary>
        /// Added By:Devanshi Gandhi
        /// Action to Save Tactic from grid.
        /// </summary>
        /// <param name="form">Form object of Plan_Campaign_Program_TacticModel.</param>

        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveGridDetail(string UpdateType, string UpdateColumn, string UpdateVal, int id = 0, string CustomFieldInput = "", string ColumnType = "", string oValue = "")
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            string PeriodChar = "Y";
            int oldOwnerId = 0;
            string UpdateValue = null;
            int oldProgramId = 0;
            string oldProgramTitle = "";
            int oldCampaignId = 0;
            UpdateColumn = UpdateColumn.Trim();
            int yearDiff = 0, perdNum = 12, cntr = 0;
            bool isMultiYearlinkedTactic = false;
            List<string> lstLinkedPeriods = new List<string>();
            List<int> dependantcustomfieldid = new List<int>();
            List<CustomfieldIDValues> customfieldidvalues = new List<CustomfieldIDValues>();
            try
            {             
                Common.RemoveGridCacheObject();
                // Get UserId Integer Id from Guid Ticket #2955
                if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                {
                    UpdateVal = Convert.ToString(Common.GetIntegerUserId(new Guid(UpdateVal)));
                } 
                #region update Plan Detail
                if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.plan.ToString())
                {
                    Plan plan = db.Plans.Where(_plan => _plan.PlanId == id).ToList().FirstOrDefault();
                    oldOwnerId = plan.CreatedBy;
                    if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.TaskName.ToString())
                    {
                        plan.Title = UpdateVal.Trim();
                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                    {

                        plan.CreatedBy = Convert.ToInt32(UpdateVal);
                    }

                    db.Entry(plan).State = EntityState.Modified;
                    db.SaveChanges();

                    //Modified by Rahul Shah on 09/03/2016 for PL #1939
                    int result = Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated, "", plan.CreatedBy);
                    if (result > 0)
                    {

                        if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                            SendEmailnotification(plan.PlanId, id, oldOwnerId, Convert.ToInt32(UpdateVal), plan.Title, plan.Title, plan.Title, plan.Title, Enums.Section.Plan.ToString().ToLower(), "", UpdateColumn);

                    }

                    var OwnerName = "";
                    if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                    {
                        OwnerName = GetOwnerName(UpdateVal);
                    }
                    return Json(new { OwnerName = OwnerName }, JsonRequestBehavior.AllowGet);
                }
                #endregion
                #region update tactic detail
                if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.tactic.ToString())
                {
                    Plan_Campaign_Program_Tactic pcpobj = db.Plan_Campaign_Program_Tactic.Where(pcptobjw => pcptobjw.PlanTacticId.Equals(id)).FirstOrDefault();
                    oldOwnerId = pcpobj.CreatedBy;
                    List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = new List<Plan_Campaign_Program_Tactic_LineItem>();
                    double totalLineitemCost = 0;
                    double otherLineItemCost = 0, tacticCost = 0;
                    int linkedTacticId = 0;
                    Plan_Campaign_Program_Tactic linkedTactic = new Plan_Campaign_Program_Tactic();
                    linkedTacticId = (pcpobj != null && pcpobj.LinkedTacticId.HasValue) ? pcpobj.LinkedTacticId.Value : 0;
                    if (linkedTacticId > 0)
                        linkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId == linkedTacticId).FirstOrDefault(); // Get LinkedTactic object

                    // update tactic detail
                    if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.TaskName.ToString())
                    {
                        var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                      join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                      join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                      where pcpt.Title.Trim().ToLower().Equals(UpdateVal) && !pcpt.PlanTacticId.Equals(id) && pcpt.IsDeleted.Equals(false)
                                      && pcp.PlanProgramId == pcpobj.Plan_Campaign_Program.PlanProgramId    //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
                                      select pcp).FirstOrDefault();
                        //// Get Linked Tactic duplicate record.
                        Plan_Campaign_Program_Tactic dupLinkedTactic = null;
                        if (linkedTacticId > 0)
                        {
                            linkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId == linkedTacticId).FirstOrDefault(); // Get LinkedTactic object

                            dupLinkedTactic = (from pcpt in db.Plan_Campaign_Program_Tactic
                                               join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                               join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                               where pcpt.Title.Trim().ToLower().Equals(UpdateVal.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(linkedTacticId) && pcpt.IsDeleted.Equals(false)
                                                && pcp.PlanProgramId == linkedTactic.PlanProgramId
                                               select pcpt).FirstOrDefault();
                        }
                        if (dupLinkedTactic != null)
                        {
                            string strDuplicateMessage = string.Format(Common.objCached.LinkedPlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                            return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                        }
                        else if (pcpvar != null)
                        {
                            string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                            return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                        }
                        else
                        {
                            pcpobj.Title = UpdateVal;
                            if (linkedTacticId > 0)
                                linkedTactic.Title = UpdateVal;
                        }

                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.StartDate.ToString())
                    {
                        if (!string.IsNullOrEmpty(UpdateVal))
                        {
                            if (pcpobj.LinkedTacticId != null)
                            {
                                if (Convert.ToInt32(pcpobj.EndDate.Year) - Convert.ToInt32(pcpobj.StartDate.Year) > 0)
                                {

                                    if (Convert.ToInt32(pcpobj.EndDate.Year) - Convert.ToInt32(Convert.ToDateTime(UpdateVal).Year) == 0)
                                    {
                                        pcpobj.LinkedTacticId = null;
                                        pcpobj.LinkedPlanId = null;
                                        linkedTactic.LinkedPlanId = null;
                                        linkedTactic.LinkedTacticId = null;

                                        pcpobj.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.IsDeleted == false).ToList().ForEach(
                                            pcptl =>
                                            {
                                                pcptl.LinkedLineItemId = null;

                                            });
                                        linkedTactic.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.IsDeleted == false).ToList().ForEach(
                                            pcpt2 =>
                                            {
                                                pcpt2.LinkedLineItemId = null;
                                            });
                                    }
                                }
                                else
                                {
                                    if (Convert.ToInt32(pcpobj.EndDate.Year) - Convert.ToInt32(Convert.ToDateTime(UpdateVal).Year) > 0)
                                    {
                                        //string linkedYear = string.Format(Common.objCached.LinkedTacticExtendedYear, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                        return Json(new { IsExtended = true }, JsonRequestBehavior.AllowGet);
                                    }

                                }
                            }
                            pcpobj.StartDate = Convert.ToDateTime(UpdateVal);
                            var PStartDate = pcpobj.Plan_Campaign_Program.StartDate;
                            if (PStartDate > Convert.ToDateTime(UpdateVal))
                            {
                                pcpobj.Plan_Campaign_Program.StartDate = Convert.ToDateTime(UpdateVal);
                            }

                            var CStartDate = pcpobj.Plan_Campaign_Program.Plan_Campaign.StartDate;

                            if (CStartDate > Convert.ToDateTime(UpdateVal))
                            {
                                pcpobj.Plan_Campaign_Program.Plan_Campaign.StartDate = Convert.ToDateTime(UpdateVal);
                            }
                            //db.Entry(pcpobj).State = EntityState.Modified;
                            //db.SaveChanges();
                        }
                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.EndDate.ToString())
                    {
                        //  var pstartdate = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(pcpobj.PlanProgramId)).FirstOrDefault().StartDate;
                        if (!string.IsNullOrEmpty(UpdateVal))
                        {
                            if (pcpobj.LinkedTacticId != null)
                            {
                                if (Convert.ToInt32(pcpobj.EndDate.Year) - Convert.ToInt32(pcpobj.StartDate.Year) > 0)
                                {

                                    if (Convert.ToInt32(Convert.ToDateTime(UpdateVal).Year) - Convert.ToInt32(pcpobj.StartDate.Year) == 0)
                                    {
                                        pcpobj.LinkedTacticId = null;
                                        pcpobj.LinkedPlanId = null;
                                        linkedTactic.LinkedPlanId = null;
                                        linkedTactic.LinkedTacticId = null;

                                        pcpobj.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.IsDeleted == false).ToList().ForEach(
                                            pcptl =>
                                            {
                                                pcptl.LinkedLineItemId = null;

                                            });
                                        linkedTactic.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.IsDeleted == false).ToList().ForEach(
                                            pcpt2 =>
                                            {
                                                pcpt2.LinkedLineItemId = null;
                                            });
                                    }
                                }
                                else
                                {
                                    if (Convert.ToInt32(Convert.ToDateTime(UpdateVal).Year) - Convert.ToInt32(pcpobj.StartDate.Year) > 0)
                                    {
                                        //User is trying to extend an already extended tactic, no problem, we now allow user to do that.
                                        //But first of all, we will break the link with the previously linked tactic (backward links only). 
                                        pcpobj.LinkedPlanId = null;
                                        pcpobj.LinkedTacticId = null;
                                    }

                                }
                            }

                            pcpobj.EndDate = Convert.ToDateTime(UpdateVal);
                            var PEndDate = pcpobj.Plan_Campaign_Program.EndDate.ToString("MM/dd/yyyy");
                            if (Convert.ToDateTime(PEndDate) < Convert.ToDateTime(UpdateVal))
                            {
                                pcpobj.Plan_Campaign_Program.EndDate = Convert.ToDateTime(UpdateVal);
                            }

                            var CEndDate = pcpobj.Plan_Campaign_Program.Plan_Campaign.EndDate.ToString("MM/dd/yyyy");

                            if (Convert.ToDateTime(CEndDate) < Convert.ToDateTime(UpdateVal))
                            {
                                pcpobj.Plan_Campaign_Program.Plan_Campaign.EndDate = Convert.ToDateTime(UpdateVal);
                            }
                            //db.Entry(pcpobj).State = EntityState.Modified;
                            //db.SaveChanges();
                        }
                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.PlannedCost.ToString())
                    {
                        // Add By Nishant Sheth #2497
                        double NewCost = 0;
                        double.TryParse(UpdateVal, out NewCost);
                        // Convert value from other currency to USD
                        if (!string.IsNullOrEmpty(UpdateVal))
                        {
                            NewCost = objCurrency.SetValueByExchangeRate(NewCost, PlanExchangeRate);
                        }
                        pcpobj.Cost = NewCost;
                        objPlanTactic.SaveTotalTacticCost(id, NewCost);
                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.TacticType.ToString())
                    {
                        int tactictypeid = Convert.ToInt32(UpdateVal);
                        int oldTactictypeId = pcpobj.TacticTypeId;
                        TacticType tt = db.TacticTypes.Where(tacType => tacType.TacticTypeId == tactictypeid).FirstOrDefault();
                        // Added by Arpita Soni for Ticket #3254 on 07/19/2016
                        // Remove tactics from package when tactic type is changed
                        if ((pcpobj.ROI_PackageDetail != null && pcpobj.ROI_PackageDetail.Count > 0) &&
                                    pcpobj.TacticType.AssetType != tt.AssetType)
                        {
                            HomeController objHome = new HomeController();
                            bool IsPromotion = false;
                            if (pcpobj.TacticType.AssetType == Convert.ToString(Enums.AssetType.Promotion) &&
                                     tt.AssetType == Convert.ToString(Enums.AssetType.Asset))
                            {
                                IsPromotion = true;
                            }
                            objHome.UnpackageTactics(pcpobj.PlanTacticId, IsPromotion);
                        }
                        //added by devanshi for #2373 on 22-7-2016 to remove all media code when asset type changes to asset
                        if (tt.AssetType == Convert.ToString(Enums.AssetType.Asset) && Sessions.IsMediaCodePermission == true)
                        {
                            List<int> intacticids = new List<int>();
                            intacticids.Add(id);
                            if (linkedTacticId > 0)
                                intacticids.Add(linkedTacticId);
                            Common.RemoveTacticMediaCode(intacticids);
                        }
                        //end
                        pcpobj.TacticTypeId = tactictypeid;
                        pcpobj.ProjectedStageValue = tt.ProjectedStageValue == null ? 0 : tt.ProjectedStageValue;

                        pcpobj.StageId = tt.StageId == null ? 0 : (int)tt.StageId;

                        if (tactictypeid > 0)
                        {
                            double Cost = tt.ProjectedRevenue != null && tt.ProjectedRevenue.HasValue ? tt.ProjectedRevenue.Value : 0;
                            objPlanTactic.SaveTotalTacticCost(id, Cost);
                            pcpobj.Cost = Cost;
                        }

                        if (linkedTacticId > 0 && tactictypeid > 0)
                        {
                            int destModelId = linkedTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId;

                            string srcTacticTypeTitle = db.TacticTypes.FirstOrDefault(type => type.TacticTypeId == tactictypeid).Title;
                            TacticType destTacticType = db.TacticTypes.FirstOrDefault(_tacType => _tacType.ModelId == destModelId && _tacType.IsDeleted == false && _tacType.IsDeployedToModel == true && _tacType.Title == srcTacticTypeTitle);
                            //// Check whether source Entity TacticType in list of TacticType of destination Model exist or not.
                            if (destTacticType != null)
                            {
                                linkedTactic.TacticTypeId = destTacticType.TacticTypeId;
                                linkedTactic.ProjectedStageValue = destTacticType.ProjectedStageValue == null ? 0 : destTacticType.ProjectedStageValue;
                                linkedTactic.StageId = destTacticType.StageId == null ? 0 : (int)destTacticType.StageId;
                            }
                        }

                        // Added by Viral Kadiya related to PL ticket #2108.
                        #region "Update DeployToIntegration & Instnace toggle on Tactic Type update"
                        if (tactictypeid > 0 && oldTactictypeId != tactictypeid)
                        {
                            // Added by Viral Kadiya related to PL ticket #2108: When we update tactic type, then the integration need to look at the model. If under model integration, there are any integration mapped then the switched for these needs to be turned on as well.
                            int sfdcInstanceId = 0, elqaInstanceId = 0, workfrontInstanceId = 0;
                            #region "Get SFDC, Elqoua, & WorkFront InstanceId from Model by Plan"
                            Model objModel = new Model();
                            Plan objPlan = new Plan();

                            objPlan = pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan;
                            if (objPlan != null)
                            {
                                objModel = objPlan.Model;
                                if (objModel != null)
                                {
                                    if (objModel.IntegrationInstanceId.HasValue)
                                        sfdcInstanceId = objModel.IntegrationInstanceId.Value;
                                    if (objModel.IntegrationInstanceEloquaId.HasValue)
                                        elqaInstanceId = objModel.IntegrationInstanceEloquaId.Value;
                                    if (objModel.IntegrationInstanceIdProjMgmt.HasValue)
                                        workfrontInstanceId = objModel.IntegrationInstanceIdProjMgmt.Value;
                                }
                            }
                            #endregion

                            #region "Get IsDeployedToIntegration by TacticTypeId"
                            int TacticTypeId = 0;
                            bool isDeployedToIntegration = false;

                            TacticTypeId = tactictypeid;
                            TacticType objTacType = new TacticType();
                            objTacType = db.TacticTypes.Where(tacType => tacType.TacticTypeId == TacticTypeId).FirstOrDefault();
                            if (objTacType != null && objTacType.IsDeployedToIntegration)
                            {
                                isDeployedToIntegration = true;
                            }
                            pcpobj.IsDeployedToIntegration = isDeployedToIntegration;
                            #endregion

                            #region "Update Instnce toggle based on TacticType & Model settings"
                            if (isDeployedToIntegration)
                            {
                                if (sfdcInstanceId > 0)
                                    pcpobj.IsSyncSalesForce = true;         // Set SFDC setting to True if Salesforce instance mapped under Tactic's Model.
                                if (elqaInstanceId > 0)
                                    pcpobj.IsSyncEloqua = true;             // Set Eloqua setting to True if Eloqua instance mapped under Tactic's Model.
                                if (workfrontInstanceId > 0)
                                    pcpobj.IsSyncWorkFront = true;          // Set WorkFront setting to True if WorkFront instance mapped under Tactic's Model.
                            }
                            else
                            {
                                pcpobj.IsSyncSalesForce = false;         // Set SFDC setting to false if isDeployedToIntegration false.
                                pcpobj.IsSyncEloqua = false;             // Set Eloqua setting to True if isDeployedToIntegration false.
                                pcpobj.IsSyncWorkFront = false;          // Set WorkFront setting to True if isDeployedToIntegration false.
                            }
                            #endregion
                            if (linkedTacticId > 0)
                            {
                                linkedTactic.IsDeployedToIntegration = pcpobj.IsDeployedToIntegration;
                                linkedTactic.IsSyncSalesForce = pcpobj.IsSyncSalesForce;
                                linkedTactic.IsSyncEloqua = pcpobj.IsSyncEloqua;
                            }
                        }
                        #endregion
                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.TargetStageGoal.ToString())
                    {
                        pcpobj.ProjectedStageValue = Convert.ToDouble(UpdateVal);
                        if (linkedTacticId > 0)
                            linkedTactic.ProjectedStageValue = pcpobj.ProjectedStageValue;

                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                    {

                        pcpobj.CreatedBy = Convert.ToInt32(UpdateVal);
                        if (linkedTacticId > 0)
                            linkedTactic.CreatedBy = pcpobj.CreatedBy;
                    }
                    else if (UpdateColumn == "ParentID")
                    {
                        oldProgramId = pcpobj.PlanProgramId;
                        oldCampaignId = pcpobj.Plan_Campaign_Program.PlanCampaignId;
                        oldProgramTitle = pcpobj.Plan_Campaign_Program.Title;
                        pcpobj.PlanProgramId = Convert.ToInt32(UpdateVal);
                        db.Entry(pcpobj).State = EntityState.Modified;
                        db.SaveChanges();
                        pcpobj = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(id)).FirstOrDefault();
                        DateTime PStartDate = pcpobj.Plan_Campaign_Program.StartDate;
                        DateTime PEndDate = pcpobj.Plan_Campaign_Program.EndDate;
                        DateTime CStartDate = pcpobj.Plan_Campaign_Program.Plan_Campaign.StartDate;
                        DateTime CEndDate = pcpobj.Plan_Campaign_Program.Plan_Campaign.EndDate;
                        DateTime StartDate = pcpobj.StartDate;
                        DateTime EndDate = pcpobj.EndDate;
                        if (PStartDate > StartDate)
                        {
                            pcpobj.Plan_Campaign_Program.StartDate = StartDate;
                        }

                        if (EndDate > PEndDate)
                        {
                            pcpobj.Plan_Campaign_Program.EndDate = EndDate;
                        }

                        if (CStartDate > StartDate)
                        {
                            pcpobj.Plan_Campaign_Program.Plan_Campaign.StartDate = StartDate;
                        }

                        if (EndDate > CEndDate)
                        {
                            pcpobj.Plan_Campaign_Program.Plan_Campaign.EndDate = EndDate;
                        }

                        if (linkedTacticId > 0)
                        {
                            if (linkedTactic.Plan_Campaign_Program.StartDate > linkedTactic.StartDate)
                            {
                                linkedTactic.Plan_Campaign_Program.StartDate = linkedTactic.StartDate;
                            }

                            if (linkedTactic.EndDate > linkedTactic.Plan_Campaign_Program.EndDate)
                            {
                                linkedTactic.Plan_Campaign_Program.EndDate = linkedTactic.EndDate;
                            }

                            if (linkedTactic.Plan_Campaign_Program.Plan_Campaign.StartDate > linkedTactic.StartDate)
                            {
                                linkedTactic.Plan_Campaign_Program.Plan_Campaign.StartDate = linkedTactic.StartDate;
                            }

                            if (linkedTactic.EndDate > linkedTactic.Plan_Campaign_Program.Plan_Campaign.EndDate)
                            {
                                linkedTactic.Plan_Campaign_Program.Plan_Campaign.EndDate = linkedTactic.EndDate;
                            }
                        }

                    }
                    ///Added by Rahul Shah for Save Custom Field from Plan Grid PL #2594
                    else if (UpdateColumn.ToString().IndexOf("custom") >= 0)
                    {
                        List<CustomField_Entity> prevCustomFieldList = db.CustomField_Entity.Where(custField => custField.EntityId == id && custField.CustomField.EntityType == UpdateType).ToList();
                        List<CustomFieldOption> customfieldoption = db.CustomFieldOptions.Where(a => a.CustomField.ClientId == Sessions.User.CID).ToList();
                        if (CustomFieldInput != null && CustomFieldInput != "")
                        {
                            List<CustomFieldStageWeight> customFields = JsonConvert.DeserializeObject<List<CustomFieldStageWeight>>(CustomFieldInput); //Deserialize Json Data to List.
                            int CustomFieldId = customFields.Select(cust => cust.CustomFieldId).FirstOrDefault(); // Get Custom Field Id 
                            List<string> CustomfieldValue = customFields.Select(cust => cust.Value).ToList();// Get Custom Field Option Value

                            Dictionary<int, string> CustomFieldOptionIds = new Dictionary<int, string>();
                            CustomFieldOptionIds = customfieldoption.Where(log => log.CustomFieldId == CustomFieldId && CustomfieldValue.Contains(log.Value)).ToDictionary(log => log.CustomFieldOptionId, log => log.Value.ToString());// Get Key Value pair for Customfield option id and its value according to Value.



                            // add method for saving dependent custom fields values.
                            if (!string.IsNullOrEmpty(oValue) && !ColumnType.Equals(Convert.ToString(Enums.ColumnType.ed), StringComparison.CurrentCultureIgnoreCase))
                            {
                                dependantcustomfieldid = SaveDependentCustomfield(oValue, CustomFieldOptionIds, CustomFieldId, customfieldoption, prevCustomFieldList, UpdateType);

                            }
                            prevCustomFieldList.Where(custField => custField.CustomFieldId == CustomFieldId).ToList().ForEach(custField => db.Entry(custField).State = EntityState.Deleted);
                            if (customFields.Count != 0)
                            {
                                CustomField_Entity objcustomFieldEntity;
                                string value = string.Empty;
                                foreach (var item in customFields)
                                {

                                    if (ColumnType.ToString().ToUpper() == Enums.ColumnType.ed.ToString().ToUpper())
                                    {
                                        value = item.Value.Trim().ToString();
                                    }
                                    else
                                    {
                                        value = CustomFieldOptionIds.Where(cust => cust.Value.Equals(item.Value.Trim().ToString())).Select(cust => cust.Key.ToString()).FirstOrDefault();
                                    }

                                    if (item.Value.Trim().ToString() != null && item.Value.Trim().ToString() != "")
                                    {
                                        objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = id;
                                        objcustomFieldEntity.CustomFieldId = item.CustomFieldId;

                                        objcustomFieldEntity.Value = value;

                                        objcustomFieldEntity = AssignValuetoCommonProperties(objcustomFieldEntity, item);
                                        db.CustomField_Entity.Add(objcustomFieldEntity);
                                    }
                                }
                            }

                            if (linkedTacticId > 0)
                            {
                                List<CustomField_Entity> prevLinkCustomFieldList = db.CustomField_Entity.Where(custField => custField.EntityId == linkedTacticId && custField.CustomField.EntityType == UpdateType && custField.CustomFieldId == CustomFieldId).ToList();
                                prevLinkCustomFieldList.ForEach(custField => db.Entry(custField).State = EntityState.Deleted);

                                if (customFields.Count != 0)
                                {
                                    CustomField_Entity objcustomFieldEntity;
                                    foreach (var item in customFields)
                                    {
                                        if (item.Value.Trim().ToString() != null && item.Value.Trim().ToString() != "")
                                        {
                                            objcustomFieldEntity = new CustomField_Entity();
                                            objcustomFieldEntity.EntityId = linkedTacticId;
                                            objcustomFieldEntity.CustomFieldId = item.CustomFieldId;
                                            if (ColumnType.ToString().ToUpper() == Enums.ColumnType.ed.ToString().ToUpper())
                                            {
                                                objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                            }
                                            else
                                            {
                                                objcustomFieldEntity.Value = CustomFieldOptionIds.Where(cust => cust.Value.Equals(item.Value.Trim().ToString())).Select(cust => cust.Key.ToString()).FirstOrDefault();
                                            }

                                            objcustomFieldEntity = AssignValuetoCommonProperties(objcustomFieldEntity, item);
                                            db.CustomField_Entity.Add(objcustomFieldEntity);
                                        }
                                    }
                                }
                            }
                            db.SaveChanges();
                            if (dependantcustomfieldid != null && dependantcustomfieldid.Count > 0)
                                customfieldidvalues = GetDeletedCustomFieldValue(dependantcustomfieldid, id, UpdateType, customfieldoption);
                        }
                    }
                    if (linkedTacticId > 0)
                    {
                        linkedTactic.Cost = pcpobj.Cost;
                        linkedTactic.ModifiedBy = Sessions.User.ID;
                        linkedTactic.ModifiedDate = DateTime.Now;
                        db.Entry(linkedTactic).State = EntityState.Modified;
                    }
                    pcpobj.ModifiedBy = Sessions.User.ID;
                    pcpobj.ModifiedDate = DateTime.Now;

                    db.Entry(pcpobj).State = EntityState.Modified;
                    db.SaveChanges();

                    if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.PlannedCost.ToString() || UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.TargetStageGoal.ToString())
                    {
                        GetCacheValue();
                    }
                    int result = 1;

                    if (Common.CheckAfterApprovedStatus(pcpobj.Status))
                    {
                        result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated, "", pcpobj.CreatedBy);
                    }
                    if (result > 0)
                    {

                        if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                        {
                            UpdateValue = UpdateVal;
                        }

                        SendEmailnotification(pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId, id, oldOwnerId, Convert.ToInt32(UpdateValue), pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.Title.ToString(), pcpobj.Plan_Campaign_Program.Plan_Campaign.Title.ToString(), pcpobj.Plan_Campaign_Program.Title.ToString(), pcpobj.Title.ToString(), Enums.Section.Tactic.ToString().ToLower(), "", UpdateColumn, pcpobj.Status);
                        if (UpdateColumn == "ParentID")
                        {
                            if (pcpobj.IntegrationInstanceTacticId != null && oldProgramId > 0)
                            {
                                if (pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance.IntegrationType.Code == Enums.IntegrationInstanceType.Salesforce.ToString())
                                {
                                    ExternalIntegration externalIntegration = new ExternalIntegration(pcpobj.PlanTacticId, Sessions.ApplicationId, 0, EntityType.Tactic, true);
                                    externalIntegration.Sync();

                                }
                            }
                            if (oldProgramId > 0)
                            {
                                var actionSuffix = oldProgramTitle + " to " + pcpobj.Plan_Campaign_Program.Title;
                                Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.moved, actionSuffix, pcpobj.CreatedBy);
                            }
                            Common.ChangeProgramStatus(pcpobj.PlanProgramId, false);
                            var PlanCampaignId = db.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false) && program.PlanProgramId == pcpobj.PlanProgramId).Select(program => program.PlanCampaignId).Single();
                            Common.ChangeCampaignStatus(PlanCampaignId, false);
                            if (oldProgramId > 0)
                            {
                                Common.ChangeProgramStatus(oldProgramId, false);
                                Common.ChangeCampaignStatus(oldCampaignId, false);
                            }
                        }

                    }
                    //Added By Rahul Shah on 16/10/2015 for PL 1559
                    //Added By Komal Rawal to update owner in HoneyComb
                    var OwnerName = "";
                    if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                    {
                        OwnerName = GetOwnerName(UpdateVal);
                    }
                    //Added By Viral on 04/22/2016 for PL 2112
                    string noneAllocated = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString();
                    string defaultAllocated = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString();
                    if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.TacticType.ToString())
                    {
                        tacticCost = (pcpobj.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == pcpobj.PlanTacticId && s.IsDeleted == false)).Count() > 0
                                                                    && pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy != noneAllocated && pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy != defaultAllocated
                                                                    ?
                                                                    ((pcpobj.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == pcpobj.PlanTacticId && s.IsDeleted == false)).Sum(a => a.Cost))
                                                                     : pcpobj.Cost;
                    }
                    totalLineitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == id && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);

                    return Json(new { lineItemCost = totalLineitemCost, OtherLineItemCost = otherLineItemCost, OwnerName = OwnerName, TacticCost = tacticCost, linkTacticId = linkedTacticId, DependentCustomfield = customfieldidvalues }, JsonRequestBehavior.AllowGet);
                }
                #endregion
                #region update program detail

                if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.program.ToString())
                {
                    Plan_Campaign_Program pcpobj = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(id)).FirstOrDefault();
                    oldOwnerId = pcpobj.CreatedBy;
                    if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.TaskName.ToString())
                    {
                        var pcpvar = (from pcp in db.Plan_Campaign_Program
                                      join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                      where pcp.Title.Trim().ToLower().Equals(UpdateVal.Trim().ToLower()) && !pcp.PlanProgramId.Equals(id) && pcp.IsDeleted.Equals(false)
                                      && pc.PlanCampaignId == pcpobj.Plan_Campaign.PlanCampaignId
                                      select pcp).FirstOrDefault();
                        //// if duplicate record exist then return with duplication message.
                        if (pcpvar != null)
                        {
                            string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                            return Json(new { IsSaved = false, errormsg = strDuplicateMessage });
                        }
                        else
                            pcpobj.Title = UpdateVal;
                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.StartDate.ToString())
                    {
                        if (!string.IsNullOrEmpty(UpdateVal))
                        {
                            pcpobj.StartDate = Convert.ToDateTime(UpdateVal);

                            if (pcpobj.Plan_Campaign.StartDate > Convert.ToDateTime(UpdateVal))
                            {
                                pcpobj.Plan_Campaign.StartDate = Convert.ToDateTime(UpdateVal);
                            }
                        }
                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.EndDate.ToString())
                    {
                        if (!string.IsNullOrEmpty(UpdateVal))
                        {
                            pcpobj.EndDate = Convert.ToDateTime(UpdateVal);
                            if (Convert.ToDateTime(UpdateVal) > pcpobj.Plan_Campaign.EndDate)
                            {
                                pcpobj.Plan_Campaign.EndDate = Convert.ToDateTime(UpdateVal);
                            }
                        }
                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                    {

                        pcpobj.CreatedBy = Convert.ToInt32(UpdateVal);
                    }

                    db.Entry(pcpobj).State = EntityState.Modified;
                    db.SaveChanges();

                    int result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanProgramId, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated, "", pcpobj.CreatedBy);

                    if (result > 0)
                    {

                        if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                        {
                            UpdateValue = UpdateVal;
                        }

                        SendEmailnotification(pcpobj.Plan_Campaign.Plan.PlanId, id, oldOwnerId, Convert.ToInt32(UpdateValue), pcpobj.Plan_Campaign.Plan.Title.ToString(), pcpobj.Plan_Campaign.Title.ToString(), pcpobj.Title.ToString(), pcpobj.Title.ToString(), Enums.Section.Program.ToString().ToLower(), "", UpdateColumn);

                    }

                    //Added By Komal Rawal to update owner in HoneyComb
                    var OwnerName = "";
                    if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                    {
                        OwnerName = GetOwnerName(UpdateVal);
                    }
                    ///Added by Rahul Shah for Save Custom Field from Plan Grid PL #2594
                    else if (UpdateColumn.ToString().IndexOf("custom") >= 0)
                    {
                        if (CustomFieldInput != null && CustomFieldInput != "")
                        {
                            List<CustomFieldStageWeight> customFields = JsonConvert.DeserializeObject<List<CustomFieldStageWeight>>(CustomFieldInput); //Deserialize Json Data to List.
                            int CustomFieldId = customFields.Select(cust => cust.CustomFieldId).FirstOrDefault(); // Get Custom Field Id 
                            List<string> CustomfieldValue = customFields.Select(cust => cust.Value).ToList(); // Get Custom Field Option Value 
                            List<CustomFieldOption> customfieldoption = db.CustomFieldOptions.Where(a => a.CustomField.ClientId == Sessions.User.CID).ToList();

                            Dictionary<int, string> CustomFieldOptionIds = new Dictionary<int, string>();
                            CustomFieldOptionIds = db.CustomFieldOptions.Where(log => log.CustomFieldId == CustomFieldId && CustomfieldValue.Contains(log.Value)).ToDictionary(log => log.CustomFieldOptionId, log => log.Value.ToString()); // Get Key Value pair for Customfield option id and its value according to Value.
                            List<CustomField_Entity> prevCustomFieldList = db.CustomField_Entity.Where(custField => custField.EntityId == id && custField.CustomField.EntityType == UpdateType && custField.CustomFieldId == CustomFieldId).ToList();

                            // add method for saving dependent custom fields values.
                            if (!string.IsNullOrEmpty(oValue) && !ColumnType.Equals(Convert.ToString(Enums.ColumnType.ed), StringComparison.CurrentCultureIgnoreCase))
                            {
                                dependantcustomfieldid = SaveDependentCustomfield(oValue, CustomFieldOptionIds, CustomFieldId, customfieldoption, prevCustomFieldList, UpdateType);

                            }
                            prevCustomFieldList.ForEach(custField => db.Entry(custField).State = EntityState.Deleted);
                            if (customFields.Count != 0)
                            {
                                CustomField_Entity objcustomFieldEntity;
                                foreach (var item in customFields)
                                {
                                    if (item.Value.Trim().ToString() != null && item.Value.Trim().ToString() != "")
                                    {
                                        objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = id;
                                        objcustomFieldEntity.CustomFieldId = item.CustomFieldId;
                                        if (ColumnType.ToString().ToUpper() == Enums.ColumnType.ed.ToString().ToUpper())
                                        {
                                            objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        }
                                        else
                                        {
                                            objcustomFieldEntity.Value = CustomFieldOptionIds.Where(cust => cust.Value.Equals(item.Value.Trim().ToString())).Select(cust => cust.Key.ToString()).FirstOrDefault();
                                        }

                                        objcustomFieldEntity = AssignValuetoCommonProperties(objcustomFieldEntity, item);
                                        db.CustomField_Entity.Add(objcustomFieldEntity);
                                    }
                                }
                            }
                            db.SaveChanges();
                            if (dependantcustomfieldid != null && dependantcustomfieldid.Count > 0)
                                customfieldidvalues = GetDeletedCustomFieldValue(dependantcustomfieldid, id, UpdateType, customfieldoption);
                        }
                    }
                    return Json(new { OwnerName = OwnerName, DependentCustomfield = customfieldidvalues }, JsonRequestBehavior.AllowGet);
                }


                #endregion
                #region update Campaign detail
                if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.campaign.ToString())
                {
                    int planId = db.Plan_Campaign.Where(_plan => _plan.PlanCampaignId.Equals(id)).FirstOrDefault().PlanId;

                    Plan_Campaign pcobj = db.Plan_Campaign.Where(pcobjw => pcobjw.PlanCampaignId.Equals(id) && pcobjw.IsDeleted.Equals(false)).FirstOrDefault();
                    oldOwnerId = pcobj.CreatedBy;
                    if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.TaskName.ToString())
                    {
                        var pc = db.Plan_Campaign.Where(plancampaign => (plancampaign.PlanId.Equals(planId) && plancampaign.IsDeleted.Equals(false) && plancampaign.Title.Trim().ToLower().Equals(UpdateVal.Trim().ToLower())
                            && !plancampaign.PlanCampaignId.Equals(id))).FirstOrDefault();

                        //// If record exist then return duplicatino message.
                        if (pc != null)
                        {
                            string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                            return Json(new { isSaved = false, errormsg = strDuplicateMessage });
                        }
                        else
                            pcobj.Title = UpdateVal;
                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.StartDate.ToString())
                    {
                        if (!string.IsNullOrEmpty(UpdateVal))
                            pcobj.StartDate = Convert.ToDateTime(UpdateVal);

                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.EndDate.ToString())
                    {
                        if (!string.IsNullOrEmpty(UpdateVal))
                            pcobj.EndDate = Convert.ToDateTime(UpdateVal);

                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                    {

                        pcobj.CreatedBy = Convert.ToInt32(UpdateVal);
                    }

                    db.Entry(pcobj).State = EntityState.Modified;
                    db.SaveChanges();

                    int result = Common.InsertChangeLog(Sessions.PlanId, null, pcobj.PlanCampaignId, pcobj.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated, "", pcobj.CreatedBy);

                    if (result > 0)
                    {
                        if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                        {
                            UpdateValue = UpdateVal;
                        }
                        SendEmailnotification(pcobj.Plan.PlanId, id, oldOwnerId, Convert.ToInt32(UpdateValue), pcobj.Plan.Title, pcobj.Title, pcobj.Title, pcobj.Title, Enums.Section.Campaign.ToString().ToLower(), "", UpdateColumn);

                    }

                    //Added By Komal Rawal to update owner in HoneyComb
                    var OwnerName = "";
                    if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                    {
                        OwnerName = GetOwnerName(UpdateVal);
                    }
                    ///Added by Rahul Shah for Save Custom Field from Plan Grid PL #2594
                    else if (UpdateColumn.ToString().IndexOf("custom") >= 0)
                    {
                        if (CustomFieldInput != null && CustomFieldInput != "")
                        {
                            List<CustomFieldStageWeight> customFields = JsonConvert.DeserializeObject<List<CustomFieldStageWeight>>(CustomFieldInput);
                            int CustomFieldId = customFields.Select(cust => cust.CustomFieldId).FirstOrDefault();// Get Custom Field Id 
                            List<string> CustomfieldValue = customFields.Select(cust => cust.Value).ToList(); // Get Custom Field Option Value 

                            Dictionary<int, string> CustomFieldOptionIds = new Dictionary<int, string>();
                            CustomFieldOptionIds = db.CustomFieldOptions.Where(log => log.CustomFieldId == CustomFieldId && CustomfieldValue.Contains(log.Value)).ToDictionary(log => log.CustomFieldOptionId, log => log.Value.ToString()); // Get Key Value pair for Customfield option id and its value according to Value.
                            List<CustomFieldOption> customfieldoption = db.CustomFieldOptions.Where(a => a.CustomField.ClientId == Sessions.User.CID).ToList();

                            List<CustomField_Entity> prevCustomFieldList = db.CustomField_Entity.Where(custField => custField.EntityId == id && custField.CustomField.EntityType == UpdateType && custField.CustomFieldId == CustomFieldId).ToList();
                            // add method for saving dependent custom fields values.
                            if (!string.IsNullOrEmpty(oValue) && !ColumnType.Equals(Convert.ToString(Enums.ColumnType.ed), StringComparison.CurrentCultureIgnoreCase))
                            {
                                dependantcustomfieldid = SaveDependentCustomfield(oValue, CustomFieldOptionIds, CustomFieldId, customfieldoption, prevCustomFieldList, UpdateType);

                            }
                            prevCustomFieldList.ForEach(custField => db.Entry(custField).State = EntityState.Deleted);
                            if (customFields.Count != 0)
                            {
                                CustomField_Entity objcustomFieldEntity;
                                foreach (var item in customFields)
                                {
                                    if (item.Value.Trim().ToString() != null && item.Value.Trim().ToString() != "")
                                    {
                                        objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = id;
                                        objcustomFieldEntity.CustomFieldId = item.CustomFieldId;
                                        if (ColumnType.ToString().ToUpper() == Enums.ColumnType.ed.ToString().ToUpper())
                                        {
                                            objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        }
                                        else
                                        {
                                            objcustomFieldEntity.Value = CustomFieldOptionIds.Where(cust => cust.Value.Equals(item.Value.Trim().ToString())).Select(cust => cust.Key.ToString()).FirstOrDefault();
                                        }

                                        objcustomFieldEntity = AssignValuetoCommonProperties(objcustomFieldEntity, item);
                                        db.CustomField_Entity.Add(objcustomFieldEntity);
                                    }
                                }
                            }
                            db.SaveChanges();
                            if (dependantcustomfieldid != null && dependantcustomfieldid.Count > 0)
                                customfieldidvalues = GetDeletedCustomFieldValue(dependantcustomfieldid, id, UpdateType, customfieldoption);
                        }
                    }
                    return Json(new { OwnerName = OwnerName, DependentCustomfield = customfieldidvalues }, JsonRequestBehavior.AllowGet);
                }

                #endregion
                #region update LineItem Detail
                if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.lineitem.ToString())
                {
                    double totalLineItemCost;
                    Plan_Campaign_Program_Tactic_LineItem objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobjw => pcpobjw.PlanLineItemId.Equals(id));
                    oldOwnerId = objLineitem.CreatedBy; //Added by Rahul Shah on 17/03/2016 for PL #2068
                    var objTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == objLineitem.PlanTacticId);
                    var LinkedTacticId = objTactic.LinkedTacticId;
                    int PlanTacticId = objTactic.PlanTacticId;

                    #region "Retrieve Linked Plan Line Item"
                    int linkedLineItemId = 0;
                    linkedLineItemId = (objLineitem != null && objLineitem.LinkedLineItemId.HasValue) ? objLineitem.LinkedLineItemId.Value : 0;
                    if (linkedLineItemId <= 0)
                    {
                        var lnkPlanLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(tac => tac.LinkedLineItemId == objLineitem.PlanLineItemId).FirstOrDefault();    // Take first Tactic bcz Tactic can linked with single plan.
                        linkedLineItemId = lnkPlanLineItem != null ? lnkPlanLineItem.PlanLineItemId : 0;
                    }
                    #endregion
                    var ObjLinkedTactic = db.Plan_Campaign_Program_Tactic.Where(LinkID => LinkID.PlanTacticId == LinkedTacticId).ToList().FirstOrDefault();
                    if (ObjLinkedTactic != null)
                    {
                        yearDiff = ObjLinkedTactic.EndDate.Year - ObjLinkedTactic.StartDate.Year;
                        isMultiYearlinkedTactic = yearDiff > 0 ? true : false;
                        cntr = 12 * yearDiff;
                        for (int i = 1; i <= cntr; i++)
                        {
                            lstLinkedPeriods.Add(PeriodChar + (perdNum + i).ToString());
                        }
                    }
                    Plan_Campaign_Program_Tactic_LineItem linkedLineItem = new Plan_Campaign_Program_Tactic_LineItem();
                    if (linkedLineItemId > 0)
                        linkedLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcpobjw => pcpobjw.PlanLineItemId == linkedLineItemId).FirstOrDefault(); // Get LinkedTactic object


                    //Added By Rahul Shah on 16/10/2015 for PL 1559
                    double tacticostNew = objTactic.Plan_Campaign_Program_Tactic_Cost.Select(tactic => tactic.Value).Sum();
                    if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.TaskName.ToString())
                    {
                        //// Get Linked Tactic duplicate record.
                        Plan_Campaign_Program_Tactic_LineItem dupLinkedLineItem = null;
                        if (linkedLineItemId > 0)
                        {
                            linkedLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcpobjw => pcpobjw.PlanLineItemId == linkedLineItemId).FirstOrDefault(); // Get LinkedTactic object

                            dupLinkedLineItem = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                                 join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                                                 join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                                 join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                                 where pcptl.Title.Trim().ToLower().Equals(UpdateVal.Trim().ToLower()) && !pcptl.PlanLineItemId.Equals(linkedLineItemId) && pcptl.IsDeleted.Equals(false)
                                                                 && pcpt.PlanTacticId == linkedLineItem.PlanTacticId
                                                 select pcptl).FirstOrDefault();
                        }

                        //// Get duplicate record to check duplication.
                        var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                       where pcptl.Title.Trim().ToLower().Equals(UpdateVal.Trim().ToLower()) && pcptl.PlanTacticId == objLineitem.PlanTacticId && !pcptl.PlanLineItemId.Equals(id) && pcptl.IsDeleted.Equals(false)
                                       select pcptl).FirstOrDefault();

                        //// if duplicate record exist then return Duplicate message.
                        if (dupLinkedLineItem != null)
                        {
                            string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()] + " in the linkedtactic");
                            return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                        }
                        else if (pcptvar != null)
                        {
                            string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);
                            return Json(new { isSaved = false, errormsg = strDuplicateMessage });
                        }
                        else
                        {
                            objLineitem.Title = UpdateVal.Trim();
                            if (linkedLineItemId > 0)
                                linkedLineItem.Title = UpdateVal.Trim();
                        }
                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.TacticType.ToString())
                    {
                        int lineitemTypeid = Convert.ToInt32(UpdateVal);
                        objLineitem.LineItemTypeId = lineitemTypeid;

                        #region "update linked lineitem lineItem Type"
                        if (linkedLineItemId > 0 && lineitemTypeid > 0)
                        {
                            int destModelId = linkedLineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId;

                            string srcLineItemTypeTitle = db.LineItemTypes.FirstOrDefault(type => type.LineItemTypeId == lineitemTypeid).Title;
                            LineItemType destLineItemType = db.LineItemTypes.FirstOrDefault(_tacType => _tacType.ModelId == destModelId && _tacType.IsDeleted == false && _tacType.Title == srcLineItemTypeTitle);
                            //// Check whether source Entity TacticType in list of TacticType of destination Model exist or not.
                            if (destLineItemType != null)
                            {
                                linkedLineItem.LineItemTypeId = destLineItemType.LineItemTypeId;
                            }
                        }
                        #endregion

                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.PlannedCost.ToString())
                    {
                        // Modified by Arpita Soni for Ticket #2634 on 09/23/2016
                        double newLineItemCost = 0;
                        // Apply exchange rate on new value and perform operations in USD form
                        if (!string.IsNullOrEmpty(UpdateVal))
                            newLineItemCost = objCurrency.SetValueByExchangeRate(double.Parse(UpdateVal), PlanExchangeRate);
                        objLineitem.Cost = newLineItemCost;
                        objPlanTactic.SaveTotalLineItemCost(id, newLineItemCost);
                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                    {
                        objLineitem.CreatedBy = Convert.ToInt32(UpdateVal);
                    }
                    else if (UpdateColumn.ToString().IndexOf("custom") >= 0)
                    {
                        if (CustomFieldInput != null && CustomFieldInput != "")
                        {
                            List<CustomFieldStageWeight> customFields = JsonConvert.DeserializeObject<List<CustomFieldStageWeight>>(CustomFieldInput); //Deserialize Json Data to List.
                            int CustomFieldId = customFields.Select(cust => cust.CustomFieldId).FirstOrDefault(); // Get Custom Field Id 
                            List<string> CustomfieldValue = customFields.Select(cust => cust.Value).ToList();// Get Custom Field Option Value
                            List<CustomFieldOption> customfieldoption = db.CustomFieldOptions.Where(a => a.CustomField.ClientId == Sessions.User.CID).ToList();
                            Dictionary<int, string> CustomFieldOptionIds = new Dictionary<int, string>();
                            CustomFieldOptionIds = customfieldoption.Where(log => log.CustomFieldId == CustomFieldId && CustomfieldValue.Contains(log.Value)).ToDictionary(log => log.CustomFieldOptionId, log => log.Value.ToString());// Get Key Value pair for Customfield option id and its value according to Value.


                            List<CustomField_Entity> prevCustomFieldList = db.CustomField_Entity.Where(custField => custField.EntityId == id && custField.CustomField.EntityType == UpdateType).ToList();
                            // add method for saving dependent custom fields values.
                            if (!string.IsNullOrEmpty(oValue) && !ColumnType.Equals(Convert.ToString(Enums.ColumnType.ed), StringComparison.CurrentCultureIgnoreCase))
                            {
                                dependantcustomfieldid = SaveDependentCustomfield(oValue, CustomFieldOptionIds, CustomFieldId, customfieldoption, prevCustomFieldList, UpdateType);

                            }
                            prevCustomFieldList.Where(custField => custField.CustomFieldId == CustomFieldId).ToList().ForEach(custField => db.Entry(custField).State = EntityState.Deleted);
                            if (customFields.Count != 0)
                            {
                                CustomField_Entity objcustomFieldEntity;
                                string value = string.Empty;
                                foreach (var item in customFields)
                                {

                                    if (ColumnType.ToString().ToUpper() == Enums.ColumnType.ed.ToString().ToUpper())
                                    {
                                        value = item.Value.Trim().ToString();
                                    }
                                    else
                                    {
                                        value = CustomFieldOptionIds.Where(cust => cust.Value.Equals(item.Value.Trim().ToString())).Select(cust => cust.Key.ToString()).FirstOrDefault();
                                    }

                                    if (item.Value.Trim().ToString() != null && item.Value.Trim().ToString() != "")
                                    {
                                        objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = id;
                                        objcustomFieldEntity.CustomFieldId = item.CustomFieldId;

                                        objcustomFieldEntity.Value = value;

                                        objcustomFieldEntity = AssignValuetoCommonProperties(objcustomFieldEntity, item);
                                        db.CustomField_Entity.Add(objcustomFieldEntity);
                                    }
                                }
                            }

                            if (linkedLineItemId > 0)
                            {
                                List<CustomField_Entity> prevLinkCustomFieldList = db.CustomField_Entity.Where(custField => custField.EntityId == linkedLineItemId && custField.CustomField.EntityType == UpdateType && custField.CustomFieldId == CustomFieldId).ToList();
                                prevLinkCustomFieldList.ForEach(custField => db.Entry(custField).State = EntityState.Deleted);

                                if (customFields.Count != 0)
                                {
                                    CustomField_Entity objcustomFieldEntity;
                                    foreach (var item in customFields)
                                    {
                                        if (item.Value.Trim().ToString() != null && item.Value.Trim().ToString() != "")
                                        {
                                            objcustomFieldEntity = new CustomField_Entity();
                                            objcustomFieldEntity.EntityId = linkedLineItemId;
                                            objcustomFieldEntity.CustomFieldId = item.CustomFieldId;
                                            if (ColumnType.ToString().ToUpper() == Enums.ColumnType.ed.ToString().ToUpper())
                                            {
                                                objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                            }
                                            else
                                            {
                                                objcustomFieldEntity.Value = CustomFieldOptionIds.Where(cust => cust.Value.Equals(item.Value.Trim().ToString())).Select(cust => cust.Key.ToString()).FirstOrDefault();
                                            }

                                            objcustomFieldEntity = AssignValuetoCommonProperties(objcustomFieldEntity, item);
                                            db.CustomField_Entity.Add(objcustomFieldEntity);
                                        }
                                    }
                                }
                            }
                            db.SaveChanges();
                            if (dependantcustomfieldid != null && dependantcustomfieldid.Count > 0)
                                customfieldidvalues = GetDeletedCustomFieldValue(dependantcustomfieldid, id, UpdateType, customfieldoption);
                        }
                    }
                    objLineitem.ModifiedBy = Sessions.User.ID;
                    objLineitem.ModifiedDate = DateTime.Now;
                    db.Entry(objLineitem).State = EntityState.Modified;

                    #region "Update linked lineItem ModifiedBy & ModifiedDate"
                    if (linkedLineItemId > 0)
                    {
                        linkedLineItem.ModifiedBy = Sessions.User.ID;
                        linkedLineItem.ModifiedDate = DateTime.Now;
                        linkedLineItem.Cost = objLineitem.Cost;
                        //Modified By Komal Rawal for #1974
                        //Desc: To Enable edit owner feature from Lineitem popup
                        if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                        {
                            linkedLineItem.CreatedBy = Convert.ToInt32(UpdateVal);
                        }
                        //ENd
                        db.Entry(linkedLineItem).State = EntityState.Modified;
                    }

                    #endregion
                    int result = Common.InsertChangeLog(objTactic.Plan_Campaign_Program.Plan_Campaign.PlanId, null, objLineitem.PlanLineItemId, objLineitem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated, "", objLineitem.CreatedBy);
                    db.SaveChanges();
                    totalLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == objTactic.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
                    //Added by Rahul Shah on 17/03/2016 for PL #2068
                    if (result > 0)
                    {
                        if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                            SendEmailnotification(objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId, id, oldOwnerId, Convert.ToInt32(UpdateVal), objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Title.ToString(), objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Title.ToString(), objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Title.ToString(), objLineitem.Plan_Campaign_Program_Tactic.Title.ToString(), Enums.Section.LineItem.ToString().ToLower(), objLineitem.Title.ToString(), UpdateColumn);
                    }

                    return Json(new { lineItemCost = totalLineItemCost, tacticCost = objTactic.Cost, linkTacticId = LinkedTacticId, DependentCustomfield = customfieldidvalues }, JsonRequestBehavior.AllowGet);
                }
                #endregion
               
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { errormsg = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { errormsg = "" });
        }
        #region method to save dependent customfield values
        private List<int> SaveDependentCustomfield(string oValue, Dictionary<int, string> CustomFieldOptionIds, int CustomFieldId, List<CustomFieldOption> customfieldoption, List<CustomField_Entity> prevCustomFieldList, string Updatetype)
        {
            List<string> oOptionList = oValue.Split(',').ToList();
            List<string> NOptionList = CustomFieldOptionIds.Values.ToList();
            List<string> DeleteOption = oOptionList.Except(NOptionList).ToList();
            List<int> dependantcustomfieldid = new List<int>();

            List<CustomFieldDependency> CsutomfieldDependancy = db.CustomFieldDependencies.Where(a => a.CustomField.ClientId == Sessions.User.CID).ToList();
            List<int> optionids = customfieldoption.Where(a => DeleteOption.Contains(a.Value) && a.CustomFieldId == CustomFieldId).Select(a => a.CustomFieldOptionId).ToList();
            if (optionids != null && optionids.Count > 0)
                dependantcustomfieldid = DeleteDependantCustomfield(optionids, CsutomfieldDependancy, prevCustomFieldList, dependantcustomfieldid, Updatetype);
            return dependantcustomfieldid.Distinct().ToList();
        }
        #endregion
        private List<int> DeleteDependantCustomfield(List<int> optionids, List<CustomFieldDependency> CsutomfieldDependancy, List<CustomField_Entity> prevCustomFieldList, List<int> dependantcustomfieldid, string Updatetype)
        {
            if (optionids != null && optionids.Count > 0)
            {
                List<string> dependancyoptionid = CsutomfieldDependancy.Where(a => optionids.Contains(a.ParentOptionId) && !string.IsNullOrEmpty(Convert.ToString(a.ChildOptionId))).Select(a => a.ChildOptionId.ToString()).ToList();
                List<string> dependancyoptionidText = CsutomfieldDependancy.Where(a => optionids.Contains(a.ParentOptionId) && string.IsNullOrEmpty(Convert.ToString(a.ChildOptionId))).Select(a => a.ChildCustomFieldId.ToString()).ToList();
                if (dependancyoptionidText != null && dependancyoptionidText.Count > 0)
                {
                    prevCustomFieldList.Where(a => dependancyoptionidText.Contains(a.CustomFieldId.ToString())).ToList().ForEach(custField => db.Entry(custField).State = EntityState.Deleted);
                    dependantcustomfieldid.AddRange(dependancyoptionidText.Select(a => int.Parse(a)).ToList());
                }
                if (dependancyoptionid != null)
                {
                    var deletecustomfield = prevCustomFieldList.Where(a => dependancyoptionid.Contains((a.Value))).ToList();
                    var deleteentityid = deletecustomfield.Select(a => a.CustomFieldEntityId).ToList();

                    dependantcustomfieldid.AddRange(deletecustomfield.Select(a => a.CustomFieldId).ToList());
                    prevCustomFieldList.Where(custField => deleteentityid.Contains(custField.CustomFieldEntityId)).ToList().ForEach(custField => db.Entry(custField).State = EntityState.Deleted);
                    optionids = dependancyoptionid.Select(a => Convert.ToInt32(a)).ToList();
                    DeleteDependantCustomfield(optionids, CsutomfieldDependancy, prevCustomFieldList, dependantcustomfieldid, Updatetype);
                }
            }
            return dependantcustomfieldid;

        }
        private List<CustomfieldIDValues> GetDeletedCustomFieldValue(List<int> dependantcustomfieldid, int id, string Updatetype, List<CustomFieldOption> customfieldoption)
        {
            List<CustomfieldIDValues> lstCustomField = new List<CustomfieldIDValues>();
            CustomfieldIDValues objecustomfieldvalue;
            List<CustomField_Entity> lstentityvalue = db.CustomField_Entity.Where(a => a.EntityId == id).ToList();

            foreach (var item in dependantcustomfieldid)
            {
                var lstOptionvalue = lstentityvalue.Where(a => a.CustomFieldId == item).ToList();
                List<int> Optionvalue = lstOptionvalue.Select(a => Convert.ToInt32(a.Value)).ToList();
                objecustomfieldvalue = new CustomfieldIDValues();
                if (Optionvalue != null && Optionvalue.Count > 0)
                {
                    string finalvalue = string.Join(",", customfieldoption.Where(a => Optionvalue.Contains(a.CustomFieldOptionId)).Select(a => a.Value).ToList());

                    objecustomfieldvalue.CustomFieldId = string.Format("custom_" + item + ":" + Updatetype);
                    objecustomfieldvalue.OptionValue = finalvalue;
                }
                else
                {
                    objecustomfieldvalue.CustomFieldId = string.Format("custom_" + item + ":" + Updatetype);
                    objecustomfieldvalue.OptionValue = string.Empty;
                }
                lstCustomField.Add(objecustomfieldvalue);
            }
            return lstCustomField;
        }
        /// <summary>
        /// Following method is created to assign value to some common properties.
        /// </summary>
        /// <param name="objcustomFieldEntity"></param>
        /// <param name="objCustomFieldStageWeight"></param>
        /// <returns></returns>
        private CustomField_Entity AssignValuetoCommonProperties(CustomField_Entity objcustomFieldEntity, CustomFieldStageWeight objCustomFieldStageWeight)
        {
            objcustomFieldEntity.CreatedDate = DateTime.Now;
            objcustomFieldEntity.CreatedBy = Sessions.User.ID;
            objcustomFieldEntity.Weightage = (byte)objCustomFieldStageWeight.Weight;
            objcustomFieldEntity.CostWeightage = (byte)objCustomFieldStageWeight.CostWeight;
            return objcustomFieldEntity;
        }

        #endregion

        public JsonResult CalculateMQL(int tactictid, int TacticTypeId, double projectedStageValue, bool RedirectType, bool isTacticTypeChange, int? StageID)
        {
            DateTime StartDate = new DateTime();
            DateTime EndDate = new DateTime();
            string stageMQL = Enums.Stage.MQL.ToString();
            int tacticStageLevel = 0;
            try
            {
                int levelMQL = db.Stages.Single(stage => stage.ClientId.Equals(Sessions.User.CID) && stage.Code.Equals(stageMQL) && stage.IsDeleted == false).Level.Value;

                if (isTacticTypeChange)
                {
                    if (StageID == null)
                    {
                        Stage objstage = db.TacticTypes.FirstOrDefault(t => t.TacticTypeId == TacticTypeId).Stage;

                        if (objstage != null)
                        {
                            string stagelevel = objstage.Level.ToString();
                            tacticStageLevel = Convert.ToInt32(stagelevel);
                        }
                        else
                            tacticStageLevel = 0;
                    }
                    else
                    {
                        int Stageid = (int)StageID;
                        tacticStageLevel = Convert.ToInt32(db.Stages.FirstOrDefault(t => t.StageId == Stageid).Level);
                    }
                }

                var tacticdata = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanTacticId == tactictid).FirstOrDefault();
                StartDate = tacticdata.StartDate;
                EndDate = tacticdata.EndDate;
                int PlanId = tacticdata.Plan_Campaign_Program.Plan_Campaign.PlanId;
                int StageId = 0;
                if (StageID == null)
                    StageId = tacticdata.StageId;
                else
                    StageId = (int)StageID;

                int modelId = db.Plans.Where(p => p.PlanId == PlanId).Select(p => p.ModelId).FirstOrDefault();

                Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
                objTactic.StartDate = StartDate;
                objTactic.EndDate = EndDate;
                objTactic.StageId = StageId;
                objTactic.Plan_Campaign_Program = new Plan_Campaign_Program() { Plan_Campaign = new Plan_Campaign() { PlanId = Sessions.PlanId, Plan = new Plan() { } } };
                objTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId = modelId;
                objTactic.ProjectedStageValue = projectedStageValue;
                var lstTacticStageRelation = Common.GetTacticStageRelationForSingleTactic(objTactic, false);
                double calculatedMQL = 0;
                double CalculatedRevenue = 0;
                calculatedMQL = lstTacticStageRelation.MQLValue;
                CalculatedRevenue = lstTacticStageRelation.RevenueValue;
                CalculatedRevenue = Math.Round(CalculatedRevenue, 2); // Modified by Sohel Pathan on 16/09/2014 for PL ticket #760
                calculatedMQL = Math.Round(calculatedMQL, 0, MidpointRounding.AwayFromZero);
                if (tacticStageLevel < levelMQL)
                {
                    return Json(new { mql = calculatedMQL, revenue = CalculatedRevenue });
                }
                else if (tacticStageLevel == levelMQL)
                {
                    return Json(new { mql = projectedStageValue, revenue = CalculatedRevenue });
                }
                else if (tacticStageLevel > levelMQL)
                {
                    return Json(new { mql = "N/A", revenue = CalculatedRevenue });
                }
                else
                {
                    return Json(new { mql = 0, revenue = CalculatedRevenue });
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
            }

        }

        #region "Send Email Notification For Owner changed"
        public void SendEmailnotification(int PlanID, int ChangeID, int oldOwnerID, int NewOwnerID, string PlanTitle, string CampaignTitle, string ProgramTitle, string Title, string section, string LineItemTitle = "", string UpdateColumn = "", string TacticStatus = "") //modified by Rahul Shah on 17/03/2016 for PL #2068 to add line item email notification
        {

            try
            {
                List<int> lst_Userids = new List<int>();
                List<string> CurrentOwnerName = new List<string>();
                List<string> CurrentOwnerEmail = new List<string>();
                lst_Userids.Add(oldOwnerID);
                List<int> List_NotificationUserIds = new List<int>();
                if (Enums.Section.Program.ToString().ToLower() == section)
                {
                    List_NotificationUserIds = Common.GetAllNotificationUserIds(lst_Userids, Enums.Custom_Notification.ProgramIsEdited.ToString().ToLower());
                }
                else if (Enums.Section.Campaign.ToString().ToLower() == section)
                {
                    List_NotificationUserIds = Common.GetAllNotificationUserIds(lst_Userids, Enums.Custom_Notification.CampaignIsEdited.ToString().ToLower());
                }
                else if (Enums.Section.Tactic.ToString().ToLower() == section)
                {
                    List_NotificationUserIds = Common.GetAllNotificationUserIds(lst_Userids, Enums.Custom_Notification.TacticIsEdited.ToString().ToLower());
                }


                if (List_NotificationUserIds.Count > 0 || (NewOwnerID != 0 && NewOwnerID != oldOwnerID))
                {
                    if (Sessions.User != null)
                    {
                        List<string> lstRecepientEmail = new List<string>();
                        List<User> UsersDetails = new List<BDSService.User>();
                        var teamMembers = new List<int> { oldOwnerID, Sessions.User.ID };
                        if (NewOwnerID != oldOwnerID && NewOwnerID != 0)
                        {
                            teamMembers = new List<int> { NewOwnerID, oldOwnerID, Sessions.User.ID };
                        }
                        try
                        {
                            UsersDetails = objBDSServiceClient.GetMultipleTeamMemberDetailsEx(teamMembers, Sessions.ApplicationId);
                        }
                        catch (Exception e)
                        {
                            ErrorSignal.FromCurrentContext().Raise(e);


                        }
                        string strURL = GetNotificationURLbyStatus(PlanID, ChangeID, section);
                        //Added by KOmal rawal to send mail for #2485 on 22-08-2016
                        #region send entity edited email notification
                        if (List_NotificationUserIds.Count > 0 && oldOwnerID != Sessions.User.ID)
                        {
                            var CurrentOwner = UsersDetails.Where(u => u.ID == oldOwnerID).Select(u => u).FirstOrDefault();
                            string oldownername = CurrentOwner.FirstName;
                            CurrentOwnerName.Add(oldownername);
                            CurrentOwnerEmail.Add(CurrentOwner.Email);
                            if (Enums.Section.Program.ToString().ToLower() == section)
                            {
                                Common.SendNotificationMail(CurrentOwnerEmail, CurrentOwnerName, Title, PlanTitle, Enums.Custom_Notification.ProgramIsEdited.ToString(), "", section, ChangeID, PlanID, strURL);
                            }
                            else if (Enums.Section.Campaign.ToString().ToLower() == section)
                            {
                                Common.SendNotificationMail(CurrentOwnerEmail, CurrentOwnerName, Title, PlanTitle, Enums.Custom_Notification.CampaignIsEdited.ToString(), "", section, ChangeID, PlanID, strURL);
                            }
                            else if (Enums.Section.Tactic.ToString().ToLower() == section && Common.CheckAfterApprovedStatus(TacticStatus))
                            {
                                Common.SendNotificationMail(CurrentOwnerEmail, CurrentOwnerName, Title, PlanTitle, Enums.Custom_Notification.TacticIsEdited.ToString(), "", section, ChangeID, PlanID, strURL);
                            }

                        }
                        #endregion
                        //ENd
                        //Send Email Notification For Owner changed.
                        if (NewOwnerID != oldOwnerID && NewOwnerID != 0 && UpdateColumn == Enums.PlanGrid_Column["owner"] && NewOwnerID != Sessions.User.ID)
                        {
                            var NewOwner = UsersDetails.Where(u => u.ID == NewOwnerID).Select(u => u).FirstOrDefault();
                            var ModifierUser = UsersDetails.Where(u => u.ID == Sessions.User.ID).Select(u => u).FirstOrDefault();
                            if (NewOwner.Email != string.Empty)
                            {
                                lstRecepientEmail.Add(NewOwner.Email);
                            }
                            string NewOwnerName = NewOwner.FirstName + " " + NewOwner.LastName;
                            string ModifierName = ModifierUser.FirstName + " " + ModifierUser.LastName;
                            List<int> NewOwnerIDs = new List<int>();
                            NewOwnerIDs.Add(NewOwnerID);
                            List_NotificationUserIds = Common.GetAllNotificationUserIds(NewOwnerIDs, Enums.Custom_Notification.EntityOwnershipAssigned.ToString().ToLower());
                            if (Enums.Section.LineItem.ToString().ToLower() == section)
                            {
                                //List_NotificationUserIds = lstRecepientEmail;
                                //TODO: the above line doen't make sense, email or user ID? 
                            }
                            if (List_NotificationUserIds.Count > 0)
                            {

                                var ComponentType = Enums.ChangeLog_ComponentType.tactic;
                                ////Added by Rahul Shah on 10/09/2015 fo PL Ticket #1521
                                if (Enums.Section.Program.ToString().ToLower() == section)
                                {
                                    ComponentType = Enums.ChangeLog_ComponentType.program;
                                    Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Program.ToString().ToLower(), strURL);
                                }
                                else if (Enums.Section.Campaign.ToString().ToLower() == Campaign)
                                {
                                    ComponentType = Enums.ChangeLog_ComponentType.campaign;
                                    Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Campaign.ToString().ToLower(), strURL);
                                }
                                else if (Enums.Section.Plan.ToString().ToLower() == section)
                                {
                                    ComponentType = Enums.ChangeLog_ComponentType.plan;
                                    Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, Title, PlanTitle, PlanTitle, PlanTitle, Enums.Section.Plan.ToString().ToLower(), strURL);
                                }
                                //Added by Rahul Shah on 17/03/2016 for PL #2068
                                else if (Enums.Section.LineItem.ToString().ToLower() == section)
                                {
                                    ComponentType = Enums.ChangeLog_ComponentType.lineitem;
                                    Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.LineItem.ToString().ToLower(), strURL, LineItemTitle);
                                }
                                else
                                {
                                    Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Tactic.ToString().ToLower(), strURL);
                                }
                                //Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Campaign.ToString().ToLower(), strURL); ////Added by Rahul Shah on 03/09/2015 fo PL Ticket #1521
                                //Changes made regarding #2484 save notifications by komal rawal on 16-08-2016
                                Common.InsertChangeLog(PlanID, null, ChangeID, Title, ComponentType, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.ownerchanged, "", NewOwnerID);
                            }
                        }



                    }
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
        }



        #endregion

        #region
        /// <summary>
        /// Function to get Notification URL.
        /// Added By: Viral Kadiya on 12/4/2014.
        /// </summary>
        /// <param name="planId">Plan Id.</param>
        /// <param name="planTacticId">Plan Tactic Id.</param>
        /// <param name="section">Section.</param>
        /// <returns>Return NotificationURL.</returns>
        public string GetNotificationURLbyStatus(int planId = 0, int planTacticId = 0, string section = "")
        {
            string strURL = string.Empty;
            try
            {
                if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, planTacticId = planTacticId, activeMenu = "Plan" }, Request.Url.Scheme);
                else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, planProgramId = planTacticId, activeMenu = "Plan" }, Request.Url.Scheme);
                else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, planCampaignId = planTacticId, activeMenu = "Plan" }, Request.Url.Scheme);
                else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, planTacticId = planTacticId, isImprovement = true, activeMenu = "Plan" }, Request.Url.Scheme);
                else if (section == Convert.ToString(Enums.Section.Plan).ToLower())
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, planId = planTacticId, activeMenu = "Plan" }, Request.Url.Scheme);
                //Added by Rahul Shah on 17/03/2016 for PL #2068
                else if (section == Convert.ToString(Enums.Section.LineItem).ToLower())
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, planLineItemId = planTacticId, activeMenu = "Plan" }, Request.Url.Scheme);


            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return strURL;
        }
        #endregion
        #region method for getting goal value for homegrid
        protected void GetGoalValue(List<Plan> plandetail, int modelId, List<Stage> stageList, Plangrid objplangrid)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            string MQLLable = string.Empty;
            string INQLable = string.Empty;
            string MQLValue = string.Empty;
            string INQValue = string.Empty;
            string Revenue = string.Empty;
            string CWLable = string.Empty;
            string CWValue = string.Empty;
            try
            {
                if (plandetail != null)
                {

                    string goalType = plandetail.FirstOrDefault().GoalType;
                    string goalValue = plandetail.FirstOrDefault().GoalValue.ToString();

                    double ADS = 0;

                    if (modelId != 0)
                    {
                        double ADSValue = db.Models.FirstOrDefault(m => m.ModelId == modelId).AverageDealSize;
                        ADS = ADSValue;
                    }

                    if (goalType.ToString() != "")
                    {
                        BudgetAllocationModel objBudgetAllocationModel = new BudgetAllocationModel();
                        bool isGoalValueExists = false;
                        goalValue = goalValue.Replace(",", "");
                        if (goalValue != "" && Convert.ToInt64(goalValue) != 0)
                        {
                            isGoalValueExists = true;
                            objBudgetAllocationModel = Common.CalculateBudgetInputs(modelId, goalType, goalValue, ADS, true);
                        }

                        MQLLable = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(stage => stage.Title.ToLower()).FirstOrDefault();
                        INQLable = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.INQ.ToString().ToLower()).Select(stage => stage.Title.ToLower()).FirstOrDefault();
                        CWLable = stageList.Where(stage => stage.Code.ToLower() == Enums.Stage.CW.ToString().ToLower()).Select(stage => stage.Title.ToLower()).FirstOrDefault();
                        //values
                        MQLValue = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.MQLValue.ToString();
                        INQValue = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.INQValue.ToString();
                        Revenue = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.RevenueValue.ToString();
                        CWValue = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.CWValue.ToString();
                        //// Set Input & Message based on GoalType value.
                        if (goalType.ToString().ToLower() == Enums.PlanGoalType.INQ.ToString().ToLower())
                            INQValue = goalValue.ToString();

                        else if (goalType.ToString().ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower())
                            MQLValue = goalValue.ToString();

                        else if (goalType.ToString().ToLower() == Enums.PlanGoalType.Revenue.ToString().ToLower())
                            Revenue = goalValue.ToString();

                        else if (goalType.ToString().ToLower() == Enums.Stage.CW.ToString())
                            CWValue = goalValue.ToString();

                    }

                }

                objplangrid.MQLLable = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(MQLLable);
                // objimprovement.MQLLable = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(MQLLable);
                objplangrid.INQLable = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(INQLable);
                objplangrid.MQLValue = MQLValue;
                objplangrid.INQValue = INQValue;
                objplangrid.Revenue = Convert.ToString(objCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(Revenue)), PlanExchangeRate));// Add By Nishant Sheth #2497
                objplangrid.CWLable = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(CWLable);
                objplangrid.CWValue = CWValue;
                //ViewBag.MQLLable = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(MQLLable);
                //ViewBag.INQLable = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(INQLable);
                //ViewBag.MQLValue = MQLValue;
                //ViewBag.INQValue = INQValue;
                //ViewBag.Revenue = Revenue;
                //ViewBag.CWLable = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(CWLable);
                //ViewBag.CWValue = CWValue;

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
        }
        #endregion

        #region method to get owner full name by int
        public string GetUserName(int Userint)
        {

            var userName = lstUserDetails.Where(user => user.ID == Userint).ToList();

            if (userName.Count > 0)
            {
                return Userint.ToString();
            }
            return "";
        }
        #endregion

        #region method to calculate tactic cost , revenue and MQl
        protected void CalculateTacticCostRevenue(int? modelId, List<int> lsttacticId, List<Plan_Campaign_Program_Tactic> programtactic, int PlanId)
        {

            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(mdl => mdl.ClientId == Sessions.User.CID && mdl.IsDeleted == false);
            int MainModelId = (int)modelId;
            double TotalMqls = 0;
            double TotalCost = 0;
            try
            {

                while (modelId != null)
                {
                    var model = ModelList.Where(mdl => mdl.ModelId == modelId).Select(mdl => mdl).FirstOrDefault();
                    modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                    modelId = model.ParentModelId;
                }
                List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
                List<StageList> stageListType = Common.GetStageList();
                DBLineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcptl => pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == PlanId && pcptl.IsDeleted.Equals(false)).Select(pcptl => pcptl).ToList();
                DBLineItemList = DBLineItemList.Where(pcptl => lsttacticId.Contains(pcptl.PlanTacticId)).ToList();
                List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTactic => _imprvTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && _imprvTactic.IsDeleted == false).Select(_imprvTactic => _imprvTactic).ToList();
                List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(mdl => mdl.ModelId).ToList());
                var improvementTacticTypeIds = improvementActivities.Select(imptype => imptype.ImprovementTacticTypeId).ToList();
                List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();


                List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelationForSinglePlan(programtactic, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, false);

                ListTacticMQLValue = (from tactic in TacticDataWithoutImprovement
                                      select new Plan_Tactic_Values
                                      {
                                          PlanTacticId = tactic.TacticObj.PlanTacticId,
                                          MQL = tactic.MQLValue,//Math.Round(tactic.MQLValue, 0, MidpointRounding.AwayFromZero),// Comment by Bhavesh Ticket #1817. Date : 09-01-2016
                                          Revenue = tactic.RevenueValue,
                                          CampaignId = tactic.TacticObj.Plan_Campaign_Program.Plan_Campaign.PlanCampaignId,
                                          Programid = tactic.TacticObj.Plan_Campaign_Program.PlanProgramId
                                      }).ToList();
                LineItemList = (from LineItem in DBLineItemList
                                select new Plan_Tactic_LineItem_Values
                                {
                                    PlanTacticId = LineItem.PlanTacticId,
                                    Cost = LineItem.Cost,
                                    CampaignId = LineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanCampaignId,
                                    Programid = LineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanProgramId
                                }).ToList();
                TotalMqls = ListTacticMQLValue.Sum(tactic => tactic.MQL);
                TotalCost = DBLineItemList.Sum(l => l.Cost);
                //ViewBag.TotalCost = TotalCost;
                //ViewBag.TotalMqls = TotalMqls;
                //  objimprovement.TotalCost = TotalCost;
                //  objimprovement.TotalMqls = TotalMqls;

                //End



            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
        }
        #endregion

        #region method to get max min date
        [HttpPost]
        public JsonResult GetMinMaxDate(int Parentid, string UpdateType, int updatedid = 0)
        {
            string TactMinDate = string.Empty;
            string TactMaxDate = string.Empty;
            string ProgMinDate = string.Empty;
            string ProgMaxDate = string.Empty;
            try
            {

                var ProgramTactic = db.Plan_Campaign_Program_Tactic.Where(tact => tact.PlanProgramId == updatedid).ToList();
                if (ProgramTactic != null && ProgramTactic.Count() > 0)
                {
                    TactMinDate = ProgramTactic.Min(r => r.StartDate).ToString("MM/dd/yyyy");
                    TactMaxDate = ProgramTactic.Max(r => r.EndDate).ToString("MM/dd/yyyy");
                }
                var CampaignProg = db.Plan_Campaign_Program.Where(tact => tact.PlanCampaignId == Parentid).ToList();
                if (CampaignProg != null && CampaignProg.Count() > 0)
                {
                    ProgMinDate = CampaignProg.Min(r => r.StartDate).ToString("MM/dd/yyyy");
                    ProgMaxDate = CampaignProg.Max(r => r.EndDate).ToString("MM/dd/yyyy");
                }

            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

            }
            return Json(new { TactMinDate = TactMinDate, TactMaxDate = TactMaxDate, ProgMinDate = ProgMinDate, ProgMaxDate = ProgMaxDate });
        }
        #endregion

        #region method to check permission of tactic , campaign or program by owner id
        [HttpPost]
        public JsonResult CheckPermissionByOwner(int NewOwnerID, string UpdateType, int updatedid = 0)
        {
            string cellTextColor = string.Empty;
            string CellBackGroundcolor = string.Empty;
            string IsLocked = string.Empty;
            List<Plan_Campaign_Program_Tactic> TacticfilterList = new List<Plan_Campaign_Program_Tactic>();
            List<int> CustomTacticids = new List<int>();
            List<int> lsteditableEntityIds = new List<int>();
            int OwnerID = NewOwnerID;
            bool IsEditable = false;
            try
            {
                if (TempData["lsteditableEntityIds"] != null)
                {
                    lsteditableEntityIds = (List<int>)TempData["lsteditableEntityIds"];
                    TempData["lsteditableEntityIds"] = lsteditableEntityIds;
                }
                List<int> lstSubordinatesIds = new List<int>();
                bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                if (IsTacticAllowForSubordinates)
                {
                    lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.ID);
                }
                if (TempData["TacticfilterList"] != null)
                {
                    TacticfilterList = (List<Plan_Campaign_Program_Tactic>)TempData["TacticfilterList"];
                    TempData["TacticfilterList"] = TacticfilterList;
                    IsEditable = OwnerID.Equals(Sessions.User.ID) == true ? true : false;
                    if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.tactic.ToString())
                    {
                        CustomTacticids = new List<int>(updatedid);
                        CellBackGroundcolor = "background-color:#dfdfdf;";
                    }
                    else if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.program.ToString())
                    {
                        CustomTacticids = TacticfilterList.Where(tact => tact.PlanProgramId == updatedid).Select(tact => tact.PlanTacticId).ToList();
                        // CellBackGroundcolor = "background-color:#E4F1E1;";
                    }
                    else if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.campaign.ToString())
                    {
                        CustomTacticids = TacticfilterList.Where(tact => tact.Plan_Campaign_Program.PlanCampaignId == updatedid).Select(tact => tact.PlanTacticId).ToList();
                        //CellBackGroundcolor = "background-color:#E4F1E1;";
                    }
                    else if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.plan.ToString())
                    {
                        //Modified by Rahul Shah on 09/03/2016 for PL #1939
                        if (OwnerID == Sessions.User.ID)
                        {
                            IsEditable = true;
                        }
                        else if (IsPlanEditAllAuthorized)
                        {
                            IsEditable = true;
                        }
                        else if (IsTacticAllowForSubordinates)
                        {
                            IsEditable = false;
                        }
                        //CellBackGroundcolor = "background-color:#E4F1E1;";
                    }
                    if (IsEditable == false)
                    {
                        if (lstSubordinatesIds.Contains(OwnerID))
                        {

                            if (CustomTacticids.Count > 0 && lsteditableEntityIds.Select(x => x).Intersect(CustomTacticids).Count() != CustomTacticids.Count)
                            {
                                IsLocked = "1";
                                cellTextColor = "color:#727272; " + CellBackGroundcolor + "";
                            }
                            else
                            {
                                cellTextColor = "color:#000; " + CellBackGroundcolor + "";
                                IsLocked = "0";
                            }
                        }
                        else
                        {
                            IsLocked = "1";
                            cellTextColor = "color:#727272; " + CellBackGroundcolor + "";

                        }
                    }
                    else
                    {
                        cellTextColor = "style='color:#000; " + CellBackGroundcolor + "'";
                        IsLocked = "0";
                    }
                    //if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.plan.ToString()) {
                    //    //Modified by Rahul Shah on 09/03/2016 for PL #1939
                    //    cellTextColor = "color:#000; " + CellBackGroundcolor + "";
                    //}
                }


            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

            }
            return Json(new { IsLocked = IsLocked, cellTextColor = cellTextColor });
        }
        #endregion

        #region "Common Methods"
        /// <summary>
        /// Added By: Viral Kadiya.
        /// Action to Get TacticTypes based on ModelId.
        /// </summary>
        /// <param name="ModelId">Model Id</param>
        /// <returns>Returns list of TacticTypes</returns>
        public List<string> GetTacticTypeListbyModelId(int ModelId)
        {
            List<string> lstTacticType = new List<string>();
            try
            {
                lstTacticType = db.TacticTypes.Where(tacType => tacType.ModelId == ModelId).Select(tacType => tacType.Title).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstTacticType;
        }

        /// <summary>
        /// Added By: Viral Kadiya.
        /// Action to Set Editable list of Campaign,Program,Tactic,ImprovementTactic list to ViewBag by PlanId.
        /// </summary>
        /// <param name="PlanId">Plan Id</param>
        /// <returns>Returns Editable list of Campaign,Program,Tactic Ids</returns>
        public void SetEditableListIdsByPlanId(int PlanId)
        {
            #region "Declare Variables"
            List<int> lstIds = new List<int>();
            List<int> lst_CampaignIds = new List<int>();
            List<Plan_Campaign> lst_AllCampaigns = new List<Plan_Campaign>();
            List<int> lst_ProgramIds = new List<int>();
            List<Plan_Campaign_Program> lst_AllPrograms = new List<Plan_Campaign_Program>();
            List<int> lst_TacticIds = new List<int>();
            List<Plan_Campaign_Program_Tactic> lst_Tactics = new List<Plan_Campaign_Program_Tactic>();
            List<int> lst_ImprovementTacticIds = new List<int>();
            List<Plan_Improvement_Campaign_Program_Tactic> lst_ImprvTactics = new List<Plan_Improvement_Campaign_Program_Tactic>();
            #endregion

            try
            {
                //// Get List of SubOrdinateIds based on Current LoginUserId.
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                List<int> lstSubordinatesIds = new List<int>();
                if (IsPlanEditSubordinatesAuthorized)
                    lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.ID);

                //// Get List of all campaigns based on PlanId.
                lst_AllCampaigns = (from _campagn in db.Plan_Campaign
                                    where _campagn.IsDeleted.Equals(false) && _campagn.PlanId.Equals(PlanId)
                                    select _campagn).ToList();

                if (lst_AllCampaigns == null)
                    lst_AllCampaigns = new List<Plan_Campaign>();

                //// Get List of all Programs based on PlanCampaignIds.
                lst_AllPrograms = (from _prgrm in db.Plan_Campaign_Program
                                   where _prgrm.IsDeleted.Equals(false)
                                   select _prgrm).ToList().Where(_prgrm => lst_AllCampaigns.Select(_campaign => _campaign.PlanCampaignId).Contains(_prgrm.PlanCampaignId)).ToList();
                if (lst_AllPrograms == null)
                    lst_AllPrograms = new List<Plan_Campaign_Program>();

                //// Get list of Tactics those created by Current User or SubOrdinatesIds.
                lst_Tactics = (from _tac in db.Plan_Campaign_Program_Tactic
                               where _tac.IsDeleted.Equals(false) && _tac.CreatedBy.Equals(Sessions.User.ID) || lstSubordinatesIds.Contains(_tac.CreatedBy)
                               select _tac).ToList().Where(_tac => lst_AllPrograms.Select(_prgram => _prgram.PlanProgramId).Contains(_tac.PlanProgramId)).ToList();

                //// Get Improvement Tactics those created by Current User.
                lst_ImprvTactics = (from _imprvCampagn in db.Plan_Improvement_Campaign
                                    where _imprvCampagn.ImprovePlanId == PlanId
                                    join _imprvPrgrm in db.Plan_Improvement_Campaign_Program on _imprvCampagn.ImprovementPlanCampaignId equals _imprvPrgrm.ImprovementPlanCampaignId
                                    join _imprvTactic in db.Plan_Improvement_Campaign_Program_Tactic on _imprvPrgrm.ImprovementPlanProgramId equals _imprvTactic.ImprovementPlanProgramId
                                    where _imprvTactic.IsDeleted.Equals(false) && _imprvTactic.CreatedBy.Equals(Sessions.User.ID)
                                    select _imprvTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

                lst_CampaignIds = lst_AllCampaigns.Where(_campagn => _campagn.CreatedBy.Equals(Sessions.User.ID)).Select(_campagn => _campagn.PlanCampaignId).ToList();
                lst_ProgramIds = lst_AllPrograms.Where(_prgram => _prgram.CreatedBy.Equals(Sessions.User.ID)).Select(_prgram => _prgram.PlanProgramId).ToList();

                if (lst_Tactics.Count() > 0)
                {
                    //// Check custrom restriction permissions for Tactic.
                    List<int> lstTacticIds = lst_Tactics.Select(tactic => tactic.PlanTacticId).ToList();
                    List<int> editableTacticIds = Common.GetEditableTacticList(Sessions.User.ID, Sessions.User.CID, lstTacticIds, false);
                    if (editableTacticIds.Count() > 0)
                    {
                        editableTacticIds.ForEach(tactic => lst_TacticIds.Add(tactic));
                    }
                }

                if (lst_ImprvTactics.Count() > 0)
                {
                    lst_ImprvTactics.ForEach(improvementTactic => lst_ImprovementTacticIds.Add(improvementTactic.ImprovementPlanTacticId));
                }

                //// Convert list of Ids to Comma separated string.
                string strCampaignIds, strProgramIds, strTacticIds, strImprovementTacticIds;
                strCampaignIds = strProgramIds = strTacticIds = strImprovementTacticIds = string.Empty;

                lst_CampaignIds.ForEach(id => { strCampaignIds += id + ","; });
                lst_ProgramIds.ForEach(id => { strProgramIds += id + ","; });
                lst_TacticIds.ForEach(id => { strTacticIds += id + ","; });
                lst_ImprovementTacticIds.ForEach(id => { strImprovementTacticIds += id + ","; });

                //// Load comma separated string Ids to ViewBag.
                ViewBag.EditablCampaignIds = strCampaignIds.TrimEnd(',');
                ViewBag.EditablProgramIds = strProgramIds.TrimEnd(',');
                ViewBag.EditablTacticIds = strTacticIds.TrimEnd(',');
                ViewBag.EditablImprovementTacticIds = strImprovementTacticIds.TrimEnd(',');
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region method to convert number in k, m formate

        /// <summary>
        /// Added By devanshi gandhi/ Bhavesh Dobariya to hadle format at server side and avoide at client side - Change made to improve performance of grid view
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public string ConvertNumberToRoundFormate(double num)
        {
            long i = (long)Math.Pow(10, (int)Math.Max(0, Math.Log10(num) - 2));
            num = num / i * i;

            if (num >= 100000000000000)
                return (num / 100000000000000D).ToString("0.##") + "Q";
            if (num >= 100000000000)
                return (num / 100000000000D).ToString("0.##") + "T";
            if (num >= 1000000000)
                return (num / 1000000000D).ToString("0.##") + "B";
            if (num >= 1000000)
                return (num / 1000000D).ToString("0.##") + "M";
            if (num >= 1000)
                return (num / 1000D).ToString("0.##") + "K";

            if (num != 0.0)
                return num < 1 ? (num.ToString().Contains(".") ? num.ToString("#,#0.00") : num.ToString("#,#")) : num.ToString("#,#");
            else
                return "0";


        }
        #endregion

        #region GetOwnerName and TacticTypeName
        /// <summary>
        /// Added By: Komal Rawal.
        /// Action to get all UserNames and TacticType
        /// </summary>
        public string GettactictypeName(int TacticTypeID)
        {
            if (TacticTypeListForHC.Count == 0 || TacticTypeListForHC == null)
            {
                TacticTypeListForHC = db.TacticTypes.Where(tt => tt.TacticTypeId == TacticTypeID && tt.IsDeleted == false).Select(tt => tt).ToList();
            }
            var TacticType = TacticTypeListForHC.Where(tt => tt.TacticTypeId == TacticTypeID && tt.IsDeleted == false).Select(tt => tt.Title).FirstOrDefault();

            return TacticType;
        }

        /// <summary>
        /// This takes a string userID since the ID may come from a string field for general usage (including int user ID) 
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public string GetOwnerName(string userID)
        {
            var OwnerName = "";
            try
            {
                if (userID != "")
                {
                    var intuserId = Convert.ToInt32(userID);
                    if (lstUserDetails == null || lstUserDetails.Count == 0)
                    {
                        lstUserDetails = objBDSServiceClient.GetUserListByClientIdEx(Sessions.User.CID);
                    }

                    var userName = lstUserDetails.Where(user => user.ID == intuserId).Select(user => new
                    {
                        FirstName = user.FirstName,
                        Lastname = user.LastName
                    }).FirstOrDefault();


                    if (userName != null)
                    {
                        OwnerName = userName.FirstName + " " + userName.Lastname;
                    }
                }

                return OwnerName.ToString();
            }
            catch (Exception e)
            {

                if (e is System.Data.EntityException || e is System.Data.SqlClient.SqlException)
                {

                    ErrorSignal.FromCurrentContext().Raise(e);
                }

            }
            return OwnerName.ToString();

        }

        public string GetOwnerNameCSV(string userId /* even though the data type may not be an int, let's use userId to reflect he fact it takes a user ID*/
                                        , List<User> lstUserDetailsData)
        {
            var OwnerName = "";
            try
            {
                if (userId != string.Empty)
                {
                    if (lstUserDetailsData == null || lstUserDetailsData.Count == 0)
                    {
                        lstUserDetailsData = objBDSServiceClient.GetUserListByClientIdEx(Sessions.User.CID);
                    }

                    var userName = lstUserDetailsData.Where(user => user.ID == Convert.ToInt32(userId)).Select(user => new
                    {
                        FirstName = user.FirstName,
                        Lastname = user.LastName
                    }).FirstOrDefault();


                    if (userName != null)
                    {
                        OwnerName = userName.FirstName + " " + userName.Lastname;
                    }
                }

                return OwnerName.ToString();
            }
            catch (Exception e)
            {

                if (e is System.Data.EntityException || e is System.Data.SqlClient.SqlException)
                {

                    ErrorSignal.FromCurrentContext().Raise(e);
                }

            }
            return OwnerName.ToString();

        }

        #endregion

        #region "Feature: Copy or Link Tactic/Program/Campaign between Plan"

        #region "Bind Planlist & Tree list"
        public ActionResult LoadCopyEntityPopup(string entityId, string section, string PopupType, string RedirectType) //Modified by Rahul Shah on 12/04/2016 for PL #2038
        {
            CopyEntiyBetweenPlanModel objModel = new CopyEntiyBetweenPlanModel();
            try
            {
                string planId = !string.IsNullOrEmpty(entityId) ? entityId.Split(new char[] { '_' })[0] : string.Empty;
                string year = string.Empty;
                entityId = !string.IsNullOrEmpty(entityId) ? entityId.Split(new char[] { '_' })[1] : string.Empty;
                ViewBag.RedirectType = RedirectType;//Added by Rahul Shah on 12/04/2016 for PL #2038
                // Get Source Entity Title to display on Popup & success message.
                #region "Get Source Entity Title"
                int sourceEntityId = !string.IsNullOrEmpty(entityId) ? int.Parse(entityId) : 0;
                string srcEntityTitle = string.Empty;
                if (sourceEntityId > 0 && string.IsNullOrEmpty(srcEntityTitle))
                {
                    if (section == Enums.Section.Campaign.ToString())
                    {
                        Plan_Campaign objCampaign = db.Plan_Campaign.Where(campagn => campagn.PlanCampaignId == sourceEntityId).FirstOrDefault();
                        if (objCampaign != null)
                        {
                            srcEntityTitle = objCampaign.Title;
                            planId = string.IsNullOrEmpty(planId) ? objCampaign.PlanId.ToString() : planId;    // if planId value is null then set through Entity.
                        }
                    }
                    else if (section == Enums.Section.Program.ToString())
                    {
                        Plan_Campaign_Program objProgram = db.Plan_Campaign_Program.Where(prg => prg.PlanProgramId == sourceEntityId).FirstOrDefault();
                        if (objProgram != null)
                        {
                            srcEntityTitle = objProgram.Title;
                            planId = string.IsNullOrEmpty(planId) ? objProgram.Plan_Campaign.PlanId.ToString() : planId;    // if planId value is null then set through Entity.
                        }
                    }
                    else if (section == Enums.Section.Tactic.ToString())
                    {
                        Plan_Campaign_Program_Tactic objTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanTacticId == sourceEntityId).FirstOrDefault();
                        if (objTactic != null)
                        {
                            srcEntityTitle = objTactic.Title;
                            planId = string.IsNullOrEmpty(planId) ? objTactic.Plan_Campaign_Program.Plan_Campaign.PlanId.ToString() : planId;    // if planId value is null then set through Entity.
                            year = objTactic.Plan_Campaign_Program.Plan_Campaign.Plan.Year.ToString();
                        }
                    }
                }
                #endregion

                #region "Get Plan List"
                //Modified by Rahul Shah for PL#1846
                HomePlanModelHeader planmodel = new Models.HomePlanModelHeader();
                List<SelectListItem> lstPlans = new List<SelectListItem>();
                // var PlanId = Sessions.PlanId;
                if (PopupType.ToString() == Enums.ModelTypeText.Linking.ToString())
                {
                    if (!string.IsNullOrEmpty(year))
                    {
                        var lstPlanAll = Common.GetPlan();
                        lstPlanAll = lstPlanAll.Where(a => Convert.ToInt32(a.Year) == Convert.ToInt32(year) + 1).ToList();
                        lstPlans = lstPlanAll.Select(plan => new SelectListItem() { Text = plan.Title, Value = plan.PlanId.ToString() }).OrderBy(plan => plan.Text).ToList();
                    }

                }
                else
                {
                    planmodel = Common.GetPlanHeaderValue(int.Parse(planId), onlyplan: true);
                    lstPlans = planmodel.plans;
                }

                #endregion

                // Handle user can not copy entity (Tactic/Program/Campaign) to the source Plan.
                if (lstPlans != null && lstPlans.Count > 0 && !string.IsNullOrEmpty(planId))
                {
                    var objPlan = lstPlans.Where(plan => plan.Value == planId).FirstOrDefault();
                    if (objPlan != null)
                        lstPlans.Remove(objPlan);
                }

                ViewBag.plans = lstPlans;
                //Modified by Rahul Shah for PL #1961. display Message when NO Plan Exsit for Link tactic. 
                ViewBag.isPlanExist = true;
                if (lstPlans == null || lstPlans.Count == 0)
                {
                    ViewBag.isPlanExist = false;
                    //return Json(true, JsonRequestBehavior.AllowGet);
                }
                #region "Get Model"

                //int selectedPlanId = lstPlans != null && lstPlans.Count > 0 && !string.IsNullOrEmpty(planId) ? Convert.ToInt32(lstPlans.FirstOrDefault().Value) : 0;
                int selectedPlanId = 0;
                if (lstPlans != null && lstPlans.Count > 0 && !string.IsNullOrEmpty(planId))
                {
                    selectedPlanId = Convert.ToInt32(lstPlans.FirstOrDefault().Value);
                }
                //selectedPlanId = 14832;
                objModel = GetParentEntitySelectionList(selectedPlanId);
                objModel.srcSectionType = section;
                objModel.srcEntityId = entityId;
                ViewBag.PopupType = PopupType;
                objModel.srcPlanId = planId;
                //objModel.HeaderTitle = HttpUtility.HtmlDecode(srcEntityTitle);
                objModel.HeaderTitle = string.Empty;
                ViewBag.HeaderTitle = srcEntityTitle;
                #endregion

            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return PartialView("~/Views/Plan/_CopyEntity.cshtml", objModel);
        }
        public JsonResult RefreshParentEntitySelectionList(string planId)
        {
            int PlanId = 0;
            PlanId = !string.IsNullOrEmpty(planId) ? Convert.ToInt32(planId) : 0;
            #region "Get Model"
            CopyEntiyBetweenPlanModel objModel = new CopyEntiyBetweenPlanModel();
            objModel = GetParentEntitySelectionList(PlanId, true);
            #endregion

            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        private CopyEntiyBetweenPlanModel GetParentEntitySelectionList(int planId, bool isAjaxCall = false)
        {

            CopyEntiyBetweenPlanModel mainGridData = new CopyEntiyBetweenPlanModel();
            List<DhtmlxGridRowDataModel> gridRowData = new List<DhtmlxGridRowDataModel>();
            try
            {

                #region "GetFinancial Parent-Child Data"

                var dataTable = new DataTable();
                dataTable.Columns.Add("Id", typeof(Int32));
                dataTable.Columns.Add("ParentId", typeof(Int32));
                dataTable.Columns.Add("RowId", typeof(String));
                dataTable.Columns.Add("Name", typeof(String));

                #region Set Tree Grid Properties and methods

                //setHeader.Append("Task Name,,,");// Default 1st 4 columns header
                //setInitWidths.Append("200,100,50,");
                //setColAlign.Append("left,center,center,");
                //setColTypes.Append("tree,ro,ro,");
                //setColValidators.Append("CustomNameValid,,,");
                //setColumnIds.Append("title,action,addrow,");
                //HeaderStyle.Append("text-align:center;border-right:0px solid #d4d4d4;,border-left:0px solid #d4d4d4;,,");
                //if (!_IsBudgetCreate_Edit && !_IsForecastCreate_Edit)
                //{
                //    setColumnsVisibility.Append("false,false,true,");
                //}
                //else
                //{
                //    setColumnsVisibility.Append("false,false,false,");
                //}
                //foreach (var columns in objColumns)
                //{
                //    setHeader.Append(columns.CustomField.Name + ",");
                //    setInitWidths.Append("100,");
                //    setColAlign.Append("center,");
                //    setColTypes.Append("ro,");
                //    setColValidators.Append(",");
                //    setColumnIds.Append(columns.CustomField.Name + ",");
                //    if (Listofcheckedcol.Contains(columns.CustomFieldId.ToString()))
                //    {
                //        setColumnsVisibility.Append("false,");
                //    }
                //    else
                //    {
                //        setColumnsVisibility.Append("true,");
                //    }
                //    HeaderStyle.Append("text-align:center;,");
                //}
                //setHeader.Append("User,Line Items,Owner");
                //setInitWidths.Append("100,100,100");
                //setColAlign.Append("center,center,center");
                //setColTypes.Append("ro,ro,ro");
                //setColumnIds.Append("action,lineitems,owner");
                //setColumnsVisibility.Append("false,false,false");
                //HeaderStyle.Append("text-align:center;,text-align:center;,text-align:center;");

                //string trimSetheader = setHeader.ToString().TrimEnd(',');
                //string trimAttachheader = attachHeader.ToString().TrimEnd(',');
                //string trimSetInitWidths = setInitWidths.ToString().TrimEnd(',');
                //string trimSetColAlign = setColAlign.ToString().TrimEnd(',');
                //string trimSetColValidators = setColValidators.ToString().TrimEnd(',');
                //string trimSetColumnIds = setColumnIds.ToString().TrimEnd(',');
                //string trimSetColTypes = setColTypes.ToString().TrimEnd(',');
                //string trimSetColumnsVisibility = setColumnsVisibility.ToString().TrimEnd(',');
                //string trimHeaderStyle = HeaderStyle.ToString().TrimEnd(',');
                #endregion

                Plan objPlan = db.Plans.Where(plan => plan.PlanId == planId).FirstOrDefault();
                ParentChildEntityMapping objPlanMapping = new ParentChildEntityMapping();
                objPlanMapping.Id = objPlan.PlanId;
                objPlanMapping.Name = isAjaxCall ? HttpUtility.HtmlDecode(objPlan.Title) : HttpUtility.HtmlEncode(objPlan.Title);
                objPlanMapping.ParentId = 0;
                objPlanMapping.RowId = Regex.Replace(objPlan.Title.Trim().Replace("_", ""), @"[^0-9a-zA-Z]+", "") + "_" + objPlan.PlanId.ToString() + "_" + "0" + "_" + Enums.Section.Plan.ToString() + "_" + objPlan.PlanId.ToString();

                List<DhtmlxGridRowDataModel> lstData = new List<DhtmlxGridRowDataModel>();
                DhtmlxGridRowDataModel objModeldata = new DhtmlxGridRowDataModel();
                objModeldata = CreateSubItem(objPlanMapping, objPlan.PlanId, Enums.Section.Campaign, objPlan.PlanId, isAjaxCall);
                lstData.Add(objModeldata);

                #endregion

                mainGridData.rows = lstData;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return mainGridData;
        }
        private DhtmlxGridRowDataModel CreateSubItem(ParentChildEntityMapping row, int parentId, Enums.Section section, int destPlanId, bool isAjaxCall)
        {
            List<DhtmlxGridRowDataModel> children = new List<DhtmlxGridRowDataModel>();
            List<ParentChildEntityMapping> lstChildren = new List<ParentChildEntityMapping>();
            if (section != Enums.Section.Tactic)
            {
                lstChildren = GetChildrenEntityList(parentId, section, destPlanId, isAjaxCall);
            }
            if (lstChildren != null && lstChildren.Count() > 0)
                lstChildren = lstChildren.OrderBy(child => child.Name, new AlphaNumericComparer()).ToList();
            Enums.Section childSection = new Enums.Section();
            if (section.Equals(Enums.Section.Campaign))
                childSection = Enums.Section.Program;
            else if (section.Equals(Enums.Section.Program))
                childSection = Enums.Section.Tactic;

            children = lstChildren
              .Select(r => CreateSubItem(r, r.Id, childSection, destPlanId, isAjaxCall))
              .ToList();
            List<string> ParentData = new List<string>();
            ParentData.Add(row.Name);
            return new DhtmlxGridRowDataModel { id = row.RowId, data = ParentData, rows = children };
        }
        private List<ParentChildEntityMapping> GetChildrenEntityList(int parentId, Enums.Section section, int destPlanId, bool isAjaxCall)
        {
            List<ParentChildEntityMapping> lstMapping = null;
            ParentChildEntityMapping objCampaign = null;
            ParentChildEntityMapping objProgram = null;
            try
            {
                if (section.Equals(Enums.Section.Campaign))
                {
                    List<Plan_Campaign> lstCampaigns = db.Plan_Campaign.Where(cmpgn => cmpgn.PlanId == parentId && cmpgn.IsDeleted == false).ToList();
                    lstMapping = new List<ParentChildEntityMapping>();
                    lstCampaigns.ForEach(cmpgn =>
                    {
                        objCampaign = new ParentChildEntityMapping();
                        objCampaign.Id = cmpgn.PlanCampaignId;
                        objCampaign.ParentId = parentId;
                        objCampaign.Name = isAjaxCall ? HttpUtility.HtmlDecode(cmpgn.Title) : HttpUtility.HtmlEncode(cmpgn.Title);
                        objCampaign.RowId = Regex.Replace(cmpgn.Title.Trim().Replace("_", ""), @"[^0-9a-zA-Z]+", "") + "_" + cmpgn.PlanCampaignId.ToString() + "_" + parentId.ToString() + "_" + section.ToString() + "_" + destPlanId.ToString();
                        lstMapping.Add(objCampaign);
                    });
                }
                else if (section.Equals(Enums.Section.Program))
                {
                    List<Plan_Campaign_Program> lstPrograms = db.Plan_Campaign_Program.Where(prgrm => prgrm.PlanCampaignId == parentId && prgrm.IsDeleted == false).ToList();
                    lstMapping = new List<ParentChildEntityMapping>();
                    lstPrograms.ForEach(prgrm =>
                    {
                        objProgram = new ParentChildEntityMapping();
                        objProgram.Id = prgrm.PlanProgramId;
                        objProgram.ParentId = parentId;
                        objProgram.Name = isAjaxCall ? HttpUtility.HtmlDecode(prgrm.Title) : HttpUtility.HtmlEncode(prgrm.Title);
                        objProgram.RowId = Regex.Replace(prgrm.Title.Trim().Replace("_", ""), @"[^0-9a-zA-Z]+", "") + "_" + prgrm.PlanProgramId.ToString() + "_" + parentId.ToString() + "_" + section.ToString() + "_" + destPlanId.ToString();
                        lstMapping.Add(objProgram);
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return lstMapping;
        }
        #endregion

        #region "Copy Entities from One Plan to Another"
        /// <summary>
        /// Created By: Viral Kadiya on 11/20/2015 for PL ticket #1748: Abiliy to move a tactic/program/campign between different plans.
        /// Copy Entity(i.e. Campaign,Program & Tactic from one plan to another).
        /// </summary>
        /// <param name="srcPlanEntityId">Source EntityId like PlanTacticId </param>
        /// <param name="destPlanEntityId">Destination EntityId: Under which source entity copied</param>
        /// <param name="CloneType">CloneType: Entity will be copied</param>
        /// <returns></returns>
        public JsonResult ClonetoOtherPlan(string CloneType, string srcEntityId, string destEntityID, string srcPlanID, string destPlanID, string sourceEntityTitle, string redirecttype = "")//Modified by Rahul Shah on 12/04/2016 for PL #2038
        {
            string sourceEntityHtmlDecodedTitle = string.Empty;
            int sourceEntityId = !string.IsNullOrEmpty(srcEntityId) ? Convert.ToInt32(srcEntityId) : 0;
            int destEntityId = !string.IsNullOrEmpty(destEntityID) ? Convert.ToInt32(destEntityID) : 0;
            int srcPlanId = !string.IsNullOrEmpty(srcPlanID) ? Convert.ToInt32(srcPlanID) : 0;
            int destPlanId = !string.IsNullOrEmpty(destPlanID) ? Convert.ToInt32(destPlanID) : 0;
            bool isdifferModel = false;
            string destPlanTitle = string.Empty;
            List<Plan> tblPlan = Common.GetPlan();

            // when user click on "Copy To" option from Plan Grid then source planId will be null.
            #region "Get Source PlanId"
            if (sourceEntityId > 0 && string.IsNullOrEmpty(srcPlanID))
            {
                if (CloneType == Enums.Section.Campaign.ToString())
                {
                    Plan_Campaign objCampaign = db.Plan_Campaign.Where(campagn => campagn.PlanCampaignId == sourceEntityId).FirstOrDefault();
                    srcPlanId = objCampaign.PlanId;
                }
                else if (CloneType == Enums.Section.Program.ToString())
                {
                    Plan_Campaign_Program objProgram = db.Plan_Campaign_Program.Where(prg => prg.PlanProgramId == sourceEntityId).FirstOrDefault();
                    srcPlanId = objProgram.Plan_Campaign.PlanId;
                }
                else if (CloneType == Enums.Section.Tactic.ToString())
                {
                    Plan_Campaign_Program_Tactic objTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanTacticId == sourceEntityId).FirstOrDefault();
                    srcPlanId = objTactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
                }
            }
            #endregion
            try
            {
                bool isValid = true;
                #region "verify all required scenarios"
                string invalidTacticIds = string.Empty;
                // Get Source and Destination Plan ModelId.
                int sourceModelId = 0; int destModelId = 0;
                //var lstPlan = db.Plans.Where(plan => plan.IsDeleted == false).ToList();
                //var lstPlan = db.Plans.Where(plan => plan.PlanId == srcPlanId || plan.PlanId == destPlanId).ToList();
                sourceModelId = tblPlan.Where(plan => plan.PlanId == srcPlanId).FirstOrDefault().ModelId;
                Plan destPlan = tblPlan.Where(plan => plan.PlanId == destPlanId).FirstOrDefault();
                destModelId = destPlan.ModelId;
                destPlanTitle = destPlan.Title;
                List<PlanTactic_TacticTypeMapping> lstTacticTypeMapping = new List<PlanTactic_TacticTypeMapping>();
                // verify that source & destination model are same or not.
                if (sourceModelId > 0 && !sourceModelId.Equals(destModelId))
                {
                    isdifferModel = true;
                    lstTacticTypeMapping = CheckTacticTypeIdToDestinationModel(CloneType, sourceEntityId, destModelId, ref invalidTacticIds);
                    if (!string.IsNullOrEmpty(invalidTacticIds))
                        isValid = false;
                }
                if (!isValid)
                    return Json(new { msg = Common.objCached.TacticTypeConflictMessage, isSuccess = false }, JsonRequestBehavior.AllowGet);
                //Return:- quiet code execution process and give warning message like "Source and Destination plan refer to different Model this may cause invalid data".
                #endregion

                #region "if verify all above scenarios then create Clone"
                int rtResult = 0;

                if (Sessions.User == null)
                {
                    TempData["ErrorMessage"] = Common.objCached.SessionExpired;
                    return Json(new { msg = Common.objCached.SessionExpired, isSuccess = false }, JsonRequestBehavior.AllowGet);
                }

                try
                {
                    if (!string.IsNullOrEmpty(CloneType) && sourceEntityId > 0)
                    {
                        Clonehelper objClonehelper = new Clonehelper();

                        //// Create Clone by CloneType e.g Plan,Campaign,Program,Tactic,LineItem
                        rtResult = objClonehelper.CloneToOtherPlan(lstTacticTypeMapping, CloneType, sourceEntityId, srcPlanId, destEntityId, isdifferModel);
                        UpdateParentEntityStartEndData(sourceEntityId, destEntityId, CloneType, srcPlanId, destPlanId);
                    }
                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                    return Json(new { msg = Common.objCached.ExceptionErrorMessage, isSuccess = false }, JsonRequestBehavior.AllowGet);
                }
                #endregion
                // Get Source Entity Title to display on success message.
                #region "Get Source Entity Title"
                if (sourceEntityId > 0 && string.IsNullOrEmpty(sourceEntityTitle))
                {
                    if (CloneType == Enums.Section.Campaign.ToString())
                    {
                        Plan_Campaign objCampaign = db.Plan_Campaign.Where(campagn => campagn.PlanCampaignId == sourceEntityId).FirstOrDefault();
                        sourceEntityTitle = objCampaign.Title;
                    }
                    else if (CloneType == Enums.Section.Program.ToString())
                    {
                        Plan_Campaign_Program objProgram = db.Plan_Campaign_Program.Where(prg => prg.PlanProgramId == sourceEntityId).FirstOrDefault();
                        sourceEntityTitle = objProgram.Title;
                    }
                    else if (CloneType == Enums.Section.Tactic.ToString())
                    {
                        Plan_Campaign_Program_Tactic objTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanTacticId == sourceEntityId).FirstOrDefault();
                        sourceEntityTitle = objTactic.Title;
                    }
                }
                sourceEntityHtmlDecodedTitle = HttpUtility.HtmlDecode(sourceEntityTitle);
                #endregion
            }
            catch (Exception ex)
            {

                ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { msg = Common.objCached.ExceptionErrorMessage, isSuccess = false }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }
            //Modified by Rahul Shah on 12/04/2016 for PL #2038
            var RequsetedModule = redirecttype;
            if (!string.IsNullOrEmpty(RequsetedModule) && RequsetedModule == Enums.InspectPopupRequestedModules.Index.ToString())
            {
                return Json(new { isSuccess = true, redirect = Url.Action("Index"), msg = Common.objCached.CloneEntitySuccessMessage.Replace("{0}", CloneType).Replace("{1}", sourceEntityHtmlDecodedTitle).Replace("{2}", destPlanTitle), opt = Enums.InspectPopupRequestedModules.Index.ToString(), Id = srcEntityId }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { msg = Common.objCached.CloneEntitySuccessMessage.Replace("{0}", CloneType).Replace("{1}", sourceEntityHtmlDecodedTitle).Replace("{2}", destPlanTitle), opt = "", isSuccess = true }, JsonRequestBehavior.AllowGet);
        }

        public List<PlanTactic_TacticTypeMapping> CheckTacticTypeIdToDestinationModel(string CloneType, int sourceEntityId, int destModelId, ref string invalidTacticIds)
        {
            //string invalidTacticIds = string.Empty;
            StringBuilder invalidtact = new StringBuilder();
            List<PlanTactic_TacticTypeMapping> lstTacticTypeMapping = new List<PlanTactic_TacticTypeMapping>();
            try
            {
                if (CloneType == Enums.DuplicationModule.Tactic.ToString())
                {
                    Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
                    objTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanTacticId == sourceEntityId && tac.IsDeleted == false).FirstOrDefault();
                    //// Check whether source Entity TacticType in list of TacticType of destination Model exist or not.
                    var lstTacticType = from _tacType in db.TacticTypes
                                        where _tacType.ModelId == destModelId && _tacType.IsDeleted == false && _tacType.IsDeployedToModel == true && _tacType.Title == objTactic.TacticType.Title
                                        select _tacType;
                    if (lstTacticType == null || lstTacticType.Count() == 0)
                        invalidTacticIds = sourceEntityId.ToString();
                    else
                    {

                        PlanTactic_TacticTypeMapping objTacticMapping = new PlanTactic_TacticTypeMapping();
                        objTacticMapping.PlanTacticId = sourceEntityId; //Source PlanTacticId
                        objTacticMapping.TacticTypeId = lstTacticType.FirstOrDefault().TacticTypeId; // Destination Model TacticTypeId.
                        objTacticMapping.TargetStageId = lstTacticType.FirstOrDefault().StageId; // Destination Model StageId.
                        lstTacticTypeMapping.Add(objTacticMapping);
                    }
                }
                else if (CloneType == Enums.DuplicationModule.Program.ToString() || CloneType == Enums.DuplicationModule.Campaign.ToString())
                {
                    List<Plan_Campaign_Program_Tactic> TacticList = new List<Plan_Campaign_Program_Tactic>();
                    if (CloneType == Enums.DuplicationModule.Program.ToString())
                    {
                        TacticList = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanProgramId == sourceEntityId && tac.IsDeleted == false).ToList();
                    }
                    else if (CloneType == Enums.DuplicationModule.Campaign.ToString())
                    {
                        List<int> childProgramIds = db.Plan_Campaign_Program.Where(prg => prg.PlanCampaignId == sourceEntityId && prg.IsDeleted == false).Select(prg => prg.PlanProgramId).ToList();
                        TacticList = db.Plan_Campaign_Program_Tactic.Where(tac => childProgramIds.Contains(tac.PlanProgramId)).ToList();
                    }
                    var childTactiList = TacticList.Select(tac => new { PlanTacticId = tac.PlanTacticId, TacticTypeId = tac.TacticTypeId, TacticTypeTitle = tac.TacticType.Title }).ToList();
                    List<string> lstTacticTypetitles = childTactiList.Select(tac => tac.TacticTypeTitle).ToList();
                    var lstTacticType = from _tacType in db.TacticTypes
                                        where _tacType.ModelId == destModelId && _tacType.IsDeleted == false && _tacType.IsDeployedToModel == true && lstTacticTypetitles.Contains(_tacType.Title)
                                        select _tacType;
                    if (lstTacticType != null && lstTacticType.Count() > 0)
                    {
                        PlanTactic_TacticTypeMapping objTacticMapping = null;

                        foreach (var childTactic in childTactiList)
                        {
                            if (!lstTacticType.Any(tacType => tacType.Title == childTactic.TacticTypeTitle))
                            {
                                invalidtact.Append(childTactic.PlanTacticId.ToString() + ",");

                                //invalidTacticIds += childTactic.PlanTacticId.ToString() + ",";
                            }
                            else
                            {
                                var objTacType = lstTacticType.FirstOrDefault(tacType => tacType.Title == childTactic.TacticTypeTitle);
                                if (objTacType != null)
                                {
                                    objTacticMapping = new PlanTactic_TacticTypeMapping();
                                    objTacticMapping.PlanTacticId = childTactic.PlanTacticId;
                                    objTacticMapping.TacticTypeId = objTacType.TacticTypeId;
                                    objTacticMapping.TargetStageId = objTacType.StageId; // Destination Model StageId.
                                    lstTacticTypeMapping.Add(objTacticMapping);
                                }
                            }
                        }

                    }
                    else
                    {
                        // Add all Ids as Invalid TacticId.
                        foreach (var childTactic in childTactiList)
                        {
                            invalidtact.Append(childTactic.PlanTacticId.ToString() + ",");
                            //invalidTacticIds += childTactic.PlanTacticId.ToString() + ",";
                        }
                    }
                    invalidTacticIds = invalidtact.ToString();
                }

            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return lstTacticTypeMapping;
        }

        /// <summary>
        /// Created By: Viral Kadiya on 11/23/2015 for PL ticket #1748: Abiliy to move a tactic/program/campign between different plans.
        /// Update Parent Entity(i.e. Campaign,Program) Start-End Date.
        /// </summary>
        /// <param name="srcPlanEntityId">Source EntityId like PlanTacticId </param>
        /// <param name="destPlanEntityId">Destination EntityId: Under which source entity copied</param>
        /// <param name="CloneType">CloneType: Entity will be copied</param>
        /// <returns></returns>
        public void UpdateParentEntityStartEndData(int srcPlanEntityId, int destPlanEntityId, string CloneType, int srcPlanId, int destPlanId)
        {
            try
            {
                bool isDifferPlanyear = false;
                #region "Identify that source & destination point to different year or not"
                if (srcPlanId != destPlanId)
                {
                    isDifferPlanyear = true;
                }
                #endregion

                #region "Update Parent Entity Start-End Date"
                Clonehelper objClonehelper = new Clonehelper();
                if (CloneType == Enums.DuplicationModule.Tactic.ToString())
                {
                    var srcTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(tac => tac.PlanTacticId == srcPlanEntityId);
                    var destProgram = db.Plan_Campaign_Program.FirstOrDefault(prg => prg.PlanProgramId == destPlanEntityId);

                    if (isDifferPlanyear)
                    {
                        destProgram.StartDate = objClonehelper.GetResultDate(srcTactic.StartDate, destProgram.StartDate, true, isParentUpdate: true);

                        destProgram.Plan_Campaign.StartDate = objClonehelper.GetResultDate(srcTactic.StartDate, destProgram.Plan_Campaign.StartDate, true, isParentUpdate: true);

                        destProgram.EndDate = objClonehelper.GetResultDate(srcTactic.EndDate, destProgram.EndDate, false, isParentUpdate: true);

                        destProgram.Plan_Campaign.EndDate = objClonehelper.GetResultDate(srcTactic.EndDate, destProgram.Plan_Campaign.EndDate, false, isParentUpdate: true);
                    }
                    else
                    {
                        if (destProgram.StartDate > srcTactic.StartDate)
                        {
                            destProgram.StartDate = srcTactic.StartDate;
                        }
                        if (destProgram.Plan_Campaign.StartDate > srcTactic.StartDate)
                        {
                            destProgram.Plan_Campaign.StartDate = srcTactic.StartDate;
                        }

                        if (srcTactic.EndDate > destProgram.EndDate)
                        {
                            destProgram.EndDate = srcTactic.EndDate;
                        }
                        if (srcTactic.EndDate > destProgram.Plan_Campaign.EndDate)
                        {
                            destProgram.Plan_Campaign.EndDate = srcTactic.EndDate;
                        }
                    }

                    db.Entry(destProgram).State = EntityState.Modified;
                }
                else if (CloneType == Enums.DuplicationModule.Program.ToString())
                {
                    var srcTactic = db.Plan_Campaign_Program.FirstOrDefault(prg => prg.PlanProgramId == srcPlanEntityId);
                    var destCampaign = db.Plan_Campaign.FirstOrDefault(cmpgn => cmpgn.PlanCampaignId == destPlanEntityId);

                    if (isDifferPlanyear)
                    {
                        destCampaign.StartDate = objClonehelper.GetResultDate(srcTactic.StartDate, destCampaign.StartDate, true, isParentUpdate: true);
                        destCampaign.EndDate = objClonehelper.GetResultDate(srcTactic.EndDate, destCampaign.EndDate, false, isParentUpdate: true);
                    }
                    else
                    {
                        if (destCampaign.StartDate > srcTactic.StartDate)
                        {
                            destCampaign.StartDate = srcTactic.StartDate;
                        }
                        if (srcTactic.EndDate > destCampaign.EndDate)
                        {
                            destCampaign.EndDate = srcTactic.EndDate;
                        }
                    }
                    db.Entry(destCampaign).State = EntityState.Modified;
                }
                db.SaveChanges();
                #endregion
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        #endregion

        #region "Link Entities from One Plan to Another"

        /// <summary>
        /// Created By: Rahul Shah on 12/30/2015 for PL ticket #1846: Abiliy to link a tactic between different plans.
        /// Link Tactic from one plan to another.
        /// </summary>
        /// <param name="srcPlanEntityId">Source EntityId like PlanTacticId </param>
        /// <param name="destPlanEntityId">Destination EntityId: Under which source entity copied</param>
        /// <param name="CloneType">CloneType: Entity will be copied</param>
        /// <returns></returns>
        public JsonResult LinktoOtherPlan(string CloneType, string srcEntityId, string destEntityID, string srcPlanID, string destPlanID, string sourceEntityTitle, string redirecttype = "")//Modified by Rahul Shah on 12/04/2016 for PL #2038
        {
            string sourceEntityHtmlDecodedTitle = string.Empty;
            int sourceEntityId = !string.IsNullOrEmpty(srcEntityId) ? Convert.ToInt32(srcEntityId) : 0;
            int destEntityId = !string.IsNullOrEmpty(destEntityID) ? Convert.ToInt32(destEntityID) : 0;
            int srcPlanId = !string.IsNullOrEmpty(srcPlanID) ? Convert.ToInt32(srcPlanID) : 0;
            int destPlanId = !string.IsNullOrEmpty(destPlanID) ? Convert.ToInt32(destPlanID) : 0;
            bool isdifferModel = false;
            string destPlanTitle = string.Empty;
            List<Plan> tblPlan = Common.GetPlan();
            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
            // when user click on "Copy To" option from Plan Grid then source planId will be null.
            #region "Get Source PlanId"
            if (sourceEntityId > 0 && string.IsNullOrEmpty(srcPlanID))
            {

                if (CloneType == Enums.Section.Tactic.ToString())
                {
                    objTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanTacticId == sourceEntityId).FirstOrDefault();
                    srcPlanId = objTactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
                }
            }
            #endregion
            try
            {
                bool isValid = true;
                #region "verify all required scenarios"
                string invalidTacticIds = string.Empty;
                // Get Source and Destination Plan ModelId.
                int sourceModelId = 0; int destModelId = 0;

                sourceModelId = tblPlan.Where(plan => plan.PlanId == srcPlanId).FirstOrDefault().ModelId;
                Plan destPlan = tblPlan.Where(plan => plan.PlanId == destPlanId).FirstOrDefault();
                destModelId = destPlan.ModelId;
                destPlanTitle = destPlan.Title;
                //Model sourceModel = new Model();
                //Model destModel = new Model();

                //sourceModel = db.Models.Where(mod => mod.ModelId == sourceModelId).FirstOrDefault();
                var getModelData = db.Models.Where(mod => mod.ModelId == sourceModelId || mod.ModelId == destModelId).ToList();
                var sourceModel = getModelData.Where(mod => mod.ModelId == sourceModelId).FirstOrDefault();
                var destModel = getModelData.Where(mod => mod.ModelId == destModelId).FirstOrDefault();
                List<PlanTactic_TacticTypeMapping> lstTacticTypeMapping = new List<PlanTactic_TacticTypeMapping>();
                // verify that source & destination model are same or not.
                if (sourceModelId > 0 && !sourceModelId.Equals(destModelId))
                {

                    //if (sourceModel.IntegrationInstanceId != null || sourceModel.IntegrationInstanceEloquaId != null)
                    //{
                    //    //destModel = db.Models.Where(mod => mod.ModelId == destModelId).FirstOrDefault();
                    //    if (sourceModel.IntegrationInstanceId != destModel.IntegrationInstanceId || sourceModel.IntegrationInstanceEloquaId != destModel.IntegrationInstanceEloquaId)
                    //    {
                    //        return Json(new { msg = Common.objCached.ModelTypeConflict, isSuccess = false }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}
                    if (sourceModel.IntegrationInstanceId != destModel.IntegrationInstanceId || sourceModel.IntegrationInstanceEloquaId != destModel.IntegrationInstanceEloquaId || sourceModel.IntegrationInstanceIdINQ != destModel.IntegrationInstanceIdINQ || sourceModel.IntegrationInstanceIdCW != destModel.IntegrationInstanceIdCW || sourceModel.IntegrationInstanceIdMQL != destModel.IntegrationInstanceIdMQL)
                    {
                        return Json(new { msg = Common.objCached.ModelTypeConflict, isSuccess = false }, JsonRequestBehavior.AllowGet);
                    }
                    isdifferModel = true;
                    lstTacticTypeMapping = CheckDetailSourceandDestinationModel(CloneType, sourceEntityId, sourceModelId, destModelId, ref invalidTacticIds);
                    if (!string.IsNullOrEmpty(invalidTacticIds))
                        isValid = false;

                }
                //Modified by Rahul Shah on 22/02/2016 for PL #1961.
                var startDate = db.Plan_Campaign.Where(st => st.PlanId == destPlanId && st.IsDeleted == false).Select(st => st.StartDate).Min();
                var endDate = db.Plan_Campaign.Where(st => st.PlanId == destPlanId && st.IsDeleted == false).Select(st => st.EndDate).Max();
                if ((endDate.Year) - (startDate.Year) > 0)
                {
                    return Json(new { msg = Common.objCached.ExtendedProgram, isSuccess = false }, JsonRequestBehavior.AllowGet);
                }
                if (!isValid)
                    return Json(new { msg = Common.objCached.TacticTypeConflictMessageforLinking, isSuccess = false }, JsonRequestBehavior.AllowGet);
                //Return:- quiet code execution process and give warning message like "Source and Destination plan refer to different Model this may cause invalid data".
                #endregion
                //check source tactic exis or not in destination Program.
                bool isTacticExist = db.Plan_Campaign_Program_Tactic.Any(tact => tact.PlanProgramId == destEntityId && tact.Title.Trim() == sourceEntityTitle.Trim() && tact.IsDeleted == false);
                if (isTacticExist)
                    return Json(new { msg = Common.objCached.LinkEntityAlreadyExist.Replace("{0}", sourceEntityTitle.ToString()), isSuccess = false }, JsonRequestBehavior.AllowGet);

                #region "if verify all above scenarios then create Clone"
                int rtResult = 0;

                if (Sessions.User == null)
                {
                    TempData["ErrorMessage"] = Common.objCached.SessionExpired;
                    return Json(new { msg = Common.objCached.SessionExpired, isSuccess = false }, JsonRequestBehavior.AllowGet);
                }

                try
                {
                    if (!string.IsNullOrEmpty(CloneType) && sourceEntityId > 0)
                    {
                        Clonehelper objClonehelper = new Clonehelper();

                        //// Create Clone by CloneType e.g Plan,Campaign,Program,Tactic,LineItem
                        rtResult = objClonehelper.LinkToOtherPlan(lstTacticTypeMapping, CloneType, sourceEntityId, srcPlanId, destEntityId, isdifferModel);
                        #region "Update Source tactic Modified By & Date while linking tactic"
                        if (rtResult > 0)
                        {
                            objTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanTacticId == sourceEntityId).FirstOrDefault();
                            objTactic.ModifiedBy = Sessions.User.ID;
                            objTactic.ModifiedDate = System.DateTime.Now;
                            db.Entry(objTactic).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        #endregion
                        UpdateParentEntityStartEndDataforLinking(sourceEntityId, destEntityId, CloneType, srcPlanId, destPlanId);

                        // Add By Nishant Sheth
                        // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
                        DataSet dsPlanCampProgTac = new DataSet();
                        dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(string.Join(",", Sessions.PlanPlanIds));
                        objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

                        List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
                        objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

                        var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
                        objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

                        var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
                        objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);

                        var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
                        objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);
                        objCache.AddCache(Enums.CacheObject.PlanTacticListforpackageing.ToString(), customtacticList);  //Added by Komal Rawal for #2358 show all tactics in package even if they are not filtered

                        var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
                        objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);
                    }
                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                    return Json(new { msg = Common.objCached.ExceptionErrorMessage, isSuccess = false }, JsonRequestBehavior.AllowGet);
                }
                #endregion
                // Get Source Entity Title to display on success message.
                //#region "Get Source Entity Title"
                //if (sourceEntityId > 0 && string.IsNullOrEmpty(sourceEntityTitle))
                //{

                //    if (CloneType == Enums.Section.Tactic.ToString())
                //    {
                //        Plan_Campaign_Program_Tactic objTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanTacticId == sourceEntityId).FirstOrDefault();
                //        sourceEntityTitle = objTactic.Title;
                //    }
                //}
                sourceEntityHtmlDecodedTitle = HttpUtility.HtmlDecode(sourceEntityTitle);
                //#endregion
            }
            catch (Exception ex)
            {

                ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { msg = Common.objCached.ExceptionErrorMessageforLinking, isSuccess = false }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }
            //Modified by Rahul Shah on 12/04/2016 for PL #2038
            var RequsetedModule = redirecttype;
            if (!string.IsNullOrEmpty(RequsetedModule) && RequsetedModule == Enums.InspectPopupRequestedModules.Index.ToString())
            {
                return Json(new { isSuccess = true, redirect = Url.Action("Index"), msg = Common.objCached.LinkEntitySuccessMessage.Replace("{0}", CloneType).Replace("{1}", sourceEntityHtmlDecodedTitle).Replace("{2}", destPlanTitle), clonetype = CloneType, opt = Enums.InspectPopupRequestedModules.Index.ToString(), Id = srcEntityId, sourceEntityHtmlDecodedTitle = sourceEntityHtmlDecodedTitle, destPlanTitle = destPlanTitle }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { msg = Common.objCached.LinkEntitySuccessMessage.Replace("{0}", CloneType).Replace("{1}", sourceEntityHtmlDecodedTitle).Replace("{2}", destPlanTitle), isSuccess = true, opt = "", clonetype = CloneType, sourceEntityHtmlDecodedTitle = sourceEntityHtmlDecodedTitle, destPlanTitle = destPlanTitle }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Created By: Rahul Shah on 01/01/2016 for PL ticket #1846: Abiliy to move a tactic/program/campign between different plans.
        /// Update Parent Entity(i.e. Campaign,Program) Start-End Date.
        /// </summary>
        /// <param name="srcPlanEntityId">Source EntityId like PlanTacticId </param>
        /// <param name="destPlanEntityId">Destination EntityId: Under which source entity linked</param>
        /// <param name="CloneType">CloneType: Entity will be linked</param>
        /// <returns></returns>
        public void UpdateParentEntityStartEndDataforLinking(int srcPlanEntityId, int destPlanEntityId, string CloneType, int srcPlanId, int destPlanId)
        {
            try
            {
                //bool isDifferPlanyear = false;
                //#region "Identify that source & destination point to different year or not"
                //if (srcPlanId != destPlanId)
                //{
                //    isDifferPlanyear = true;
                //}
                //#endregion
                DateTime StartDate = DateTime.Now;
                DateTime EndDate = DateTime.Now;
                #region "Update Parent Entity Start-End Date"
                Clonehelper objClonehelper = new Clonehelper();
                if (CloneType == Enums.DuplicationModule.Tactic.ToString())
                {
                    //var srcTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(tac => tac.PlanTacticId == srcPlanEntityId);
                    var destProgram = db.Plan_Campaign_Program.FirstOrDefault(prg => prg.PlanProgramId == destPlanEntityId && prg.IsDeleted == false);
                    var destTact = db.Plan_Campaign_Program_Tactic.Where(tact => tact.PlanProgramId == destPlanEntityId && tact.IsDeleted == false).ToList();

                    StartDate = destTact.Select(tact => tact.StartDate).Min();
                    EndDate = destTact.Select(tact => tact.EndDate).Max();
                    //if (isDifferPlanyear)
                    //{
                    //    destProgram.StartDate = objClonehelper.GetResultDate(srcTactic.StartDate, destProgram.StartDate, true, isParentUpdate: true);

                    //    destProgram.Plan_Campaign.StartDate = objClonehelper.GetResultDate(srcTactic.StartDate, destProgram.Plan_Campaign.StartDate, true, isParentUpdate: true);

                    //    destProgram.EndDate = objClonehelper.GetResultDate(srcTactic.EndDate, destProgram.EndDate, false, isParentUpdate: true);

                    //    destProgram.Plan_Campaign.EndDate = objClonehelper.GetResultDate(srcTactic.EndDate, destProgram.Plan_Campaign.EndDate, false, isParentUpdate: true);
                    //}
                    //else
                    //{
                    if (destProgram.StartDate > StartDate)
                    {
                        destProgram.StartDate = StartDate;
                    }
                    if (destProgram.Plan_Campaign.StartDate > StartDate)
                    {
                        destProgram.Plan_Campaign.StartDate = StartDate;
                    }

                    if (EndDate > destProgram.EndDate)
                    {
                        destProgram.EndDate = EndDate;
                    }
                    if (EndDate > destProgram.Plan_Campaign.EndDate)
                    {
                        destProgram.Plan_Campaign.EndDate = EndDate;
                    }
                    //}

                    db.Entry(destProgram).State = EntityState.Modified;
                }

                db.SaveChanges();
                #endregion
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public List<PlanTactic_TacticTypeMapping> CheckDetailSourceandDestinationModel(string CloneType, int sourceEntityId, int sourceModelId, int destModelId, ref string invalidTacticIds)
        {
            //string invalidTacticIds = string.Empty;
            List<PlanTactic_TacticTypeMapping> lstTacticTypeMapping = new List<PlanTactic_TacticTypeMapping>();
            try
            {
                if (CloneType == Enums.DuplicationModule.Tactic.ToString())
                {
                    Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
                    objTactic = db.Plan_Campaign_Program_Tactic.Where(tac => tac.PlanTacticId == sourceEntityId && tac.IsDeleted == false).FirstOrDefault();

                    //// Check whether source Entity TacticType in list of TacticType of destination Model exist or not.
                    if (objTactic != null)
                    {
                        var lstTacticType = from _tacType in db.TacticTypes
                                            where _tacType.ModelId == destModelId && _tacType.IsDeleted == false && _tacType.IsDeployedToModel == true && _tacType.Title == objTactic.TacticType.Title && _tacType.StageId == objTactic.StageId
                                            select _tacType;
                        if (lstTacticType == null || lstTacticType.Count() == 0)
                            invalidTacticIds = sourceEntityId.ToString();
                        else
                        {

                            PlanTactic_TacticTypeMapping objTacticMapping = new PlanTactic_TacticTypeMapping();
                            objTacticMapping.PlanTacticId = sourceEntityId; //Source PlanTacticId
                            objTacticMapping.TacticTypeId = lstTacticType.FirstOrDefault().TacticTypeId; // Destination Model TacticTypeId.
                            objTacticMapping.TargetStageId = lstTacticType.FirstOrDefault().StageId; // Destination Model StageId.
                            lstTacticTypeMapping.Add(objTacticMapping);
                        }
                    }



                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return lstTacticTypeMapping;
        }

        #endregion
        #endregion

        /// <summary>
        /// Added by Rushil Bhuptani on 14/06/2016 for ticket #2227
        /// Import excel file data and update database with imported data.
        /// </summary>
        [HttpPost]
        public ActionResult ExcelFileUpload()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            bool isPlanYearExist = true;
            try
            {
                if (Request.Files[0].ContentLength > 0)
                {
                    string fileExtension =
                        System.IO.Path.GetExtension(Request.Files[0].FileName);

                    if (fileExtension == ".xls" || fileExtension == ".xlsx")
                    {
                        string fileLocation = Server.MapPath("~/Content/") + Request.Files[0].FileName;
                        if (System.IO.File.Exists(fileLocation))
                        {
                            GC.Collect();
                            System.IO.File.Delete(fileLocation);
                        }
                        Request.Files[0].SaveAs(fileLocation);
                        string excelConnectionString = string.Empty;
                        //Convert excel file in to datatable
                        if (fileExtension == ".xls")
                        {
                            excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                                                    fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\"";
                            IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(Request.Files[0].InputStream);
                            excelReader.IsFirstRowAsColumnNames = true;
                            ds = excelReader.AsDataSet();
                            if (ds == null)
                            {
                                return Json(new { msg = "error", error = "Invalid data." }, JsonRequestBehavior.AllowGet);
                            }
                            dt = ds.Tables[0];
                        }

                        else if (fileExtension == ".xlsx")
                        {
                            dt = GetXLSX(fileLocation);
                            if (dt == null)
                            {
                                return Json(new { msg = "error", error = Common.objCached.InvalidImportData }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        //if excel will be blank then following message will be appear.
                        if (dt.Rows.Count == 0 || dt.Rows[0][0] == DBNull.Value)
                        {
                            return Json(new { msg = "error", error = Common.objCached.InvalidImportData }, JsonRequestBehavior.AllowGet);
                        }
                        //if ActivityId will be null then following message will be appear.
                        foreach (DataRow row in dt.Rows)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(row["ActivityId"]).Trim()))
                            {
                                return Json(new { msg = "error", error = Common.objCached.ImportValidation.Replace("{0}", "ActivityId") }, JsonRequestBehavior.AllowGet);

                            }
                        }

                        StoredProcedure objSp = new StoredProcedure();
                        //Check data is uploaded monthly or quarterly
                        bool isMonthly = false;
                        string[] columnNames = dt.Columns.Cast<DataColumn>()
                               .Select(x => x.ColumnName)
                               .ToArray();
                        if (columnNames.Where(w => w.ToLower().Contains("jan")).Any())
                            isMonthly = true;
                        //Following is method using which we can specify import data as per type.

                        DataTable dtplanYearData = objcommonimportData.ReturnPlanYearData(dt.Copy());
                        DataTable dtBudget = objcommonimportData.GetPlanBudgetDataByType(dtplanYearData.Copy(), "budget", isMonthly);
                        if (dtplanYearData.Rows.Count != dt.Rows.Count)
                            isPlanYearExist = false;
                        //if type will be null then following message will be appear.
                        for (int i = 0; i < dtplanYearData.Rows.Count - 1; i++)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dtplanYearData.Rows[i]["Type"]).Trim()))
                            {
                                return Json(new { msg = "error", error = Common.objCached.ImportValidation.Replace("{0}", "Type") }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        //Filter budget data table with  plan,tactic,lineitem,campaign and program.
                        if (dtBudget != null)
                        {
                            if (dtBudget.Rows.Count > 0)
                                dtBudget = dtBudget.AsEnumerable().Where(row => row.Field<String>("type").ToLower() == "plan" || row.Field<String>("type").ToLower() == "tactic" || row.Field<String>("type").ToLower() == "campaign" || row.Field<String>("type").ToLower() == "program").CopyToDataTable();
                        }
                        //Filter actual data table with only plan,tactic and lineitem
                        DataTable dtActual = objcommonimportData.GetPlanBudgetDataByType(dtplanYearData.Copy(), "actual", isMonthly);
                        if (dtActual != null)
                        {
                            if (dtActual.Rows.Count > 0)
                                dtActual = dtActual.AsEnumerable().Where(row => row.Field<String>("type").ToLower() == "plan" || row.Field<String>("type").ToLower() == "tactic" || row.Field<String>("type").ToLower() == "lineitem").CopyToDataTable();
                        }

                        //Filter plan(cost) data table with only plan,tactic and lineitem
                        DataTable dtCost = objcommonimportData.GetPlanBudgetDataByType(dtplanYearData.Copy(), "planned", isMonthly);
                        if (dtCost != null)
                        {
                            if (dtCost.Rows.Count > 0)
                                dtCost = dtCost.AsEnumerable().Where(row => row.Field<String>("type").ToLower() == "plan" || row.Field<String>("type").ToLower() == "tactic" || row.Field<String>("type").ToLower() == "lineitem").CopyToDataTable();
                        }
                        //Import PlanBudgetData
                        DataSet dataResponsebudget = objSp.ImportPlanBudgetList(dtBudget, isMonthly, Sessions.User.ID);
                        //Import PlanActualData
                        DataSet dataResponseactual = objSp.ImportPlanActualList(dtActual, isMonthly, Sessions.User.ID);
                        //Import PlanCostData
                        DataSet dataResponsecost = objSp.ImportPlanCostList(dtCost, isMonthly, Sessions.User.ID);

                        if (dataResponsebudget == null || dataResponseactual == null || dataResponsecost == null)
                        {
                            return Json(new { msg = "error", error = Common.objCached.InvalidImportData }, JsonRequestBehavior.AllowGet);
                        }
                        //following message will be displayed if data will not match existing activityid                        
                        if (dataResponsebudget.Tables[0].Rows.Count > 0 || dataResponseactual.Tables[0].Rows.Count > 0 || dataResponsecost.Tables[0].Rows.Count > 0)
                        {
                            return Json(new { conflict = true, message = Common.objCached.ImportSuccessMessage + " Warning: " + Common.objCached.ActivityIdInvalid }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("process"))
                {
                    return Json(new { msg = "error", error = Common.objCached.FileUsedMessage }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { msg = "error", error = Common.objCached.InvalidImportData }, JsonRequestBehavior.AllowGet);
                }
            }
            if (isPlanYearExist)
                return Json(new { conflict = false, message = Common.objCached.ImportSuccessMessage }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { conflict = false, message = Common.objCached.ImportSuccessMessage + " Warning: " + Common.objCached.ActivityIdInvalid },

    JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Added by Rushil Bhuptani on 16/06/2016 for ticket #2227
        /// Method that store xls file data in dataset.
        /// </summary>
        /// <param name="excelConnectionString">OLEDB connection string.</param>
        /// <returns>Dataset containing xls file data.</returns>
        private DataSet GetXLS(string excelConnectionString)
        {
            DataSet ds = new DataSet();
            OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
            try
            {
                excelConnection.Open();
                DataTable dt = new DataTable();

                dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                if (dt == null)
                {
                    //return true;
                }
                String[] excelSheets = new String[dt.Rows.Count];
                int t = 0;
                //excel data saves in temp file here.
                foreach (DataRow row in dt.Rows)
                {
                    excelSheets[t] = row["TABLE_NAME"].ToString();
                    t++;
                }
                OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);


                string query = string.Format("Select * from [{0}]", excelSheets[0]);
                using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                {
                    dataAdapter.Fill(ds);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }

            return ds;
        }

        /// <summary>
        /// Added by Rushil Bhuptani on 16/06/2016 for ticket #2227
        /// Method that store xlsx file data in data table.
        /// </summary>
        /// <param name="fileLocation">Path of file where it is stored.</param>
        /// <returns>Datatable containing xlsx file data.</returns>
        private DataTable GetXLSX(string fileLocation)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fileLocation, false))
                {
                    //Read the first Sheet from Excel file.
                    Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();

                    //Get the Worksheet instance.
                    Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;

                    //Fetch all the rows present in the Worksheet.
                    IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();

                    //Loop through the Worksheet rows.
                    foreach (Row row in rows)
                    {
                        //Use the first row to add columns to DataTable.
                        if (row.RowIndex.Value == 1)
                        {
                            foreach (Cell cell in row.Descendants<Cell>())
                            {
                                dt.Columns.Add(GetValue(doc, cell));
                            }
                        }
                        else
                        {
                            //Add rows to DataTable.
                            dt.Rows.Add();
                            int i = 0;
                            foreach (Cell cell in row.Descendants<Cell>())
                            {
                                dt.Rows[dt.Rows.Count - 1][i] = GetValue(doc, cell);
                                i++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }
            return dt;
        }

        /// <summary>
        /// Added by Rushil Bhuptani on 16/06/2016 for ticket #2227
        /// Get the value of cell from excel sheet.
        /// </summary>
        /// <param name="doc">Excel document.</param>
        /// <param name="cell">Cell of excel document.</param>
        /// <returns>The value of cell.</returns>
        private string GetValue(SpreadsheetDocument doc, Cell cell)
        {
            string value = cell.CellValue.InnerText;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
            }
            return value;
        }

        /// <summary>
        /// Get budget,planned and actuals grid data
        /// </summary>
        /// <param name="PlanIds">comma seperated planids string</param>
        /// <param name="PlanExchangeRate">currency related plan exchange rate</param>
        /// <param name="viewBy">view by to display data as per view by select e.g. campagn,customfields</param>
        /// <param name="year">timeframe selected year</param>
        /// <param name="CustomFieldId">filters list for customfield</param>
        /// <param name="OwnerIds">filtered owner ids</param>
        /// <param name="TacticTypeids">filtered tactic type ids</param>
        /// <param name="StatusIds">filter status ids</param>
        /// <param name="Year">selected year of plans from timeframe </param>
        /// <returns></returns>
        [CompressAttribute]
        public ActionResult GetBudgetData(string PlanIds, string ViewBy, string OwnerIds = "", string TactictypeIds = "", string StatusIds = "", string CustomFieldIds = "", string year = "", string SearchText = "", string SearchBy = "", bool IsFromCache = false) // pass parameter IsFromCache to search the budget grid data using cache
        {
            IBudget Iobj = new RevenuePlanner.Services.Budget();
            int UserID = Sessions.User.ID;
            int ClientId = Sessions.User.CID;
            if (string.IsNullOrEmpty(ViewBy))
            {
                ViewBy = PlanGanttTypes.Tactic.ToString();
            }
            BudgetDHTMLXGridModel budgetModel = Iobj.GetBudget(ClientId, UserID, PlanIds, PlanExchangeRate, ViewBy, year, CustomFieldIds, OwnerIds, TactictypeIds, StatusIds, SearchText, SearchBy,IsFromCache);
            string strThisMonth = Enums.UpcomingActivities.ThisYearMonthly.ToString();
            if (year.ToLower() == strThisMonth.ToLower())
            {
                ViewBag.isquarter = false;
            }
            else
            {
                ViewBag.isquarter = true;
            }
            return PartialView("~/Views/Budget/Budget.cshtml", budgetModel);

        }
        
        /// <summary>
        /// Method to get dependant custom field option for plan grid
        /// Added by : devanshi
        /// </summary>
        /// <param name="customfieldId"></param>
        /// <param name="entityid"></param>
        /// <param name="parentoptionId"></param>
        /// <returns></returns>
        public JsonResult GetdependantOptionlist(int customfieldId, int entityid, List<int> parentoptionId, string Customfieldtype = "")
        {
            List<CustomField_Entity> entitycustomfieldvalue = db.CustomField_Entity.Where(a => a.EntityId == entityid).ToList();
            List<CustomFieldDependency> dependancy = db.CustomFieldDependencies.Where(a => a.ChildCustomFieldId == customfieldId).ToList();
            List<Options> CustomFieldOptionList = new List<Options>();
            bool IstextBoxDependent = false;
            if (entitycustomfieldvalue != null && parentoptionId != null && parentoptionId.Count > 0)
            {
                foreach (int parentoptid in parentoptionId)
                {
                    var isexist = entitycustomfieldvalue.Where(a => a.Value == Convert.ToString(parentoptid)).Any();
                    if (isexist)
                    {
                        if (!Customfieldtype.Equals(Convert.ToString(Enums.ColumnType.ed), StringComparison.CurrentCultureIgnoreCase))
                        {
                            CustomFieldOptionList.AddRange(dependancy.Where(a => a.ParentOptionId == parentoptid && a.IsDeleted == false).Select(a => new Options
                            {
                                value = a.CustomFieldOption.Value
                            }).ToList());
                        }
                    }
                    else
                    {
                        if (Customfieldtype.Equals(Convert.ToString(Enums.ColumnType.ed), StringComparison.CurrentCultureIgnoreCase))
                            IstextBoxDependent = true;
                    }
                }
            }
            return Json(new { optionlist = CustomFieldOptionList, IstextBoxDependent = IstextBoxDependent }, JsonRequestBehavior.AllowGet);
        }

        public void GetCacheValue()
        {

            // Add By Nishant Sheth
            // Desc :: get records from cache dataset for Plan,Campaign,Program,Tactic
            var PlanId = string.Join(",", Sessions.PlanPlanIds);
            DataSet dsPlanCampProgTac = new DataSet();
            dsPlanCampProgTac = objSp.GetListPlanCampaignProgramTactic(PlanId);
            objCache.AddCache(Enums.CacheObject.dsPlanCampProgTac.ToString(), dsPlanCampProgTac);

            List<Plan> lstPlans = Common.GetSpPlanList(dsPlanCampProgTac.Tables[0]);
            objCache.AddCache(Enums.CacheObject.Plan.ToString(), lstPlans);

            var lstCampaign = Common.GetSpCampaignList(dsPlanCampProgTac.Tables[1]).ToList();
            objCache.AddCache(Enums.CacheObject.Campaign.ToString(), lstCampaign);

            var lstProgramPer = Common.GetSpCustomProgramList(dsPlanCampProgTac.Tables[2]);
            objCache.AddCache(Enums.CacheObject.Program.ToString(), lstProgramPer);
            //var tacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && lstPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactic => tactic).ToList();

            var customtacticList = Common.GetSpCustomTacticList(dsPlanCampProgTac.Tables[3]);
            objCache.AddCache(Enums.CacheObject.CustomTactic.ToString(), customtacticList);
            objCache.AddCache(Enums.CacheObject.PlanTacticListforpackageing.ToString(), customtacticList);  //Added by Komal Rawal for #2358 show all tactics in package even if they are not filtered
            // Add By Nishant Sheth
            // Desc :: Set tatcilist for original db/modal format
            var tacticList = Common.GetTacticFromCustomTacticList(customtacticList);
            objCache.AddCache(Enums.CacheObject.Tactic.ToString(), tacticList);
        }

        #region Export to Excel

        public void DownloadCSV()
        {
            DataTable dtTable = new DataTable();
            DataRow dtRow;

            dtTable.Columns.Add("SNo", typeof(int));
            dtTable.Columns.Add("Address", typeof(string));

            for (int i = 0; i <= 9; i++)
            {
                dtRow = dtTable.NewRow();
                dtRow[0] = i;
                dtRow[1] = "Address " + i.ToString();
                dtTable.Rows.Add(dtRow);
            }

            Response.ContentType = "Application/x-excel";
            Response.AddHeader("content-disposition", "attachment;filename=test.csv");
            Response.Write(ExportToCSVFile(dtTable));
            Response.End();

        }

        private string ExportToCSVFile(DataTable dtTable)
        {
            StringBuilder sbldr = new StringBuilder();
            if (dtTable.Columns.Count != 0)
            {
                foreach (DataColumn col in dtTable.Columns)
                {
                    sbldr.Append(col.ColumnName + ',');
                }
                sbldr.Append("\r\n");
                foreach (DataRow row in dtTable.Rows)
                {
                    foreach (DataColumn column in dtTable.Columns)
                    {
                        sbldr.Append(row[column].ToString().Replace(",", ";") + ','); // To handle the comma char from values
                        //sbldr.Append(row[column].ToString() + ',');
                    }
                    sbldr.Append("\r\n");
                }
            }
            return HtmlDecodeString(sbldr.ToString());
        }

        public void ExportCsvDataTable()
        {
            string FileName = HttpUtility.HtmlDecode(Convert.ToString(Session["FileName"]));
            DataTable CSVDataTable = (DataTable)Session["CSVDataTable"];
            Response.ContentType = "Application/x-excel";
            Response.ContentEncoding = System.Text.Encoding.Default; // Add By Nishant Sheth // #2502 For handle multicurrency symbol in exported file
            Response.Charset = "UTF-8";
            //added by devanshi to replace invalid character for filename
            FileName = string.Join("_", FileName.Split(Path.GetInvalidFileNameChars()));

            // Modified By Nishant Sheth
            // Export csv does not work in Firefox #2430
            if (!string.IsNullOrEmpty(FileName))
            {
                FileName = FileName + ".csv";
            }
            Response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", System.IO.Path.GetFileName(FileName)));
            // End By Nishant Sheth
            Response.Write(ExportToCSVFile(CSVDataTable));
            Response.End();
        }

        public JsonResult ExportToCsv(string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds, string HoneycombIds = null, int PlanId = 0)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();

            List<int> filterTacticType = string.IsNullOrWhiteSpace(TacticTypeid) ? new List<int>() : TacticTypeid.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();
            List<string> filterStatus = string.IsNullOrWhiteSpace(StatusIds) ? new List<string>() : StatusIds.Split(',').Select(tactictype => tactictype).ToList();
            List<int> filterOwner = string.IsNullOrWhiteSpace(ownerIds) ? new List<int>() : ownerIds.Split(',').Select(owner => int.Parse(owner)).ToList();
            List<string> lstFilteredCustomFieldOptionIds = new List<string>();
            List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();

            DataTable dtTable = new DataTable();
            DataSet dsExportCsv = new DataSet();
            DataTable dtCSV = new DataTable();
            DataTable dtCSVCost = new DataTable();
            dsExportCsv = objSp.GetExportCSV(PlanId, HoneycombIds);
            dtCSV = dsExportCsv.Tables[0];
            dtCSVCost = dsExportCsv.Tables[0];

            //var useridslist = dtCSV.Rows.Cast<DataRow>().Select(x => Guid.Parse(x.Field<string>("CreatedBy"))).ToList();
            //string strContatedIndividualList = string.Join(",", useridslist.Select(tactic => tactic.ToString()));

            var listOfClientId = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
            stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.CID && stage.IsDeleted == false).Select(stage => stage).ToList();
            string MQLTitle = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(stage => stage.Title).FirstOrDefault();
            ViewBag.MQLTitle = MQLTitle;

            //DataColumnCollection columns = dsExportCsv.Tables[0].Columns;
            DataTable dtColums = dsExportCsv.Tables[1];

            string[] ListEnumCol = Enum.GetNames(typeof(Enums.DownloadCSV));
            string[] ListNotEnumCol = Enum.GetNames(typeof(Enums.NotDownloadCSV));
            foreach (var colEnums in ListEnumCol)
            {
                string dtcolname = Convert.ToString(colEnums);
                if (colEnums == Enums.DownloadCSV.MQL.ToString())
                {
                    dtcolname = MQLTitle;
                }
                dtTable.Columns.Add(dtcolname, typeof(string));
            }

            for (int i = 0; i < dtColums.Rows.Count; i++)
            {
                if (!ListEnumCol.Contains(dtColums.Rows[i][0].ToString()) && !ListNotEnumCol.Contains(dtColums.Rows[i][0].ToString()))
                {
                    if (dtTable.Columns.Contains(Convert.ToString(dtColums.Rows[i][0])))
                    {
                        dtTable.Columns.Add(" " + dtColums.Rows[i][0].ToString() + " ", typeof(string));
                    }
                    else
                    {
                        dtTable.Columns.Add(dtColums.Rows[i][0].ToString(), typeof(string));
                    }
                }
            }
            dtTable.AcceptChanges();

            DataColumnCollection columnNames = dtTable.Columns;

            var PlanList = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Plan.ToString()).Select(x => x.Field<int>("EntityId")).ToList();
            int modelId = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Plan.ToString()).Select(x => int.Parse(x.Field<string>("ModelId"))).FirstOrDefault();

            var TacticIds = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Tactic.ToString()).Select(x => x.Field<int>("EntityId")).ToList();

            var PlanTactics = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(PlanId) && _tactic.IsDeleted.Equals(false)).ToList();
            string FileName = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Tactic.ToString()
            || x.Field<string>("Section") == Enums.Section.Program.ToString()
            || x.Field<string>("Section") == Enums.Section.Campaign.ToString()
            || x.Field<string>("Section") == Enums.Section.Plan.ToString()).Select(x => x.Field<string>("Plan")).FirstOrDefault();
            ViewBag.CSVFileName = FileName;
            var PlanTacticIds = PlanTactics.Select(a => a.PlanTacticId).ToList();
            var progTactic = PlanTactics.Where(_tactic => TacticIds.Contains(_tactic.PlanTacticId) && _tactic.IsDeleted.Equals(false)).ToList();
            string section = Enums.Section.Tactic.ToString();
            var cusomfield = db.CustomFields.Where(customField => customField.EntityType == section && customField.ClientId == Sessions.User.CID && customField.IsDeleted == false).ToList();
            var customfieldidlist = cusomfield.Select(c => c.CustomFieldId).ToList();

            var lstAllTacticCustomFieldEntitiesanony = db.CustomField_Entity.Where(customFieldEntity => customfieldidlist.Contains(customFieldEntity.CustomFieldId))
                                                                                                   .Select(customFieldEntity => new { EntityId = customFieldEntity.EntityId, CustomFieldId = customFieldEntity.CustomFieldId, Value = customFieldEntity.Value }).Distinct().ToList();

            List<CustomField_Entity> customfieldlist = (from tbl in lstAllTacticCustomFieldEntitiesanony
                                                        join lst in TacticIds on tbl.EntityId equals lst
                                                        select new CustomField_Entity
                                                        {
                                                            EntityId = tbl.EntityId,
                                                            CustomFieldId = tbl.CustomFieldId,
                                                            Value = tbl.Value
                                                        }).ToList();
            if (dtCSV.Rows.Count > 0)
            {
                if (HoneycombIds == null)
                {
                    CalculateTacticCostRevenue(modelId, TacticIds, progTactic, PlanId);
                }
                else
                {
                    if (modelId == 0)
                    {
                        modelId = PlanTactics.Select(a => a.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId).FirstOrDefault();
                    }
                    CalculateTacticCostRevenue(modelId, PlanTacticIds, PlanTactics, PlanId);
                }
            }

            //// Custom Field Filter Criteria.
            List<string> filteredCustomFields = string.IsNullOrWhiteSpace(customFieldIds) ? new List<string>() : customFieldIds.Split(',').Select(customFieldId => customFieldId.ToString()).ToList();
            if (filteredCustomFields.Count > 0)
            {
                filteredCustomFields.ForEach(customField =>
                {
                    string[] splittedCustomField = customField.Split('_');
                    lstCustomFieldFilter.Add(new CustomFieldFilter { CustomFieldId = int.Parse(splittedCustomField[0]), OptionId = splittedCustomField[1] });
                    lstFilteredCustomFieldOptionIds.Add(splittedCustomField[1]);
                });
            }

            if (filterOwner.Count > 0 || filterTacticType.Count > 0 || filterStatus.Count > 0 || filteredCustomFields.Count > 0)
            {
                progTactic = progTactic.Where(pcptobj => (filterOwner.Contains(pcptobj.CreatedBy)) &&
                                         (filterTacticType.Contains(pcptobj.TacticType.TacticTypeId)) &&
                                         (filterStatus.Contains(pcptobj.Status))).ToList();

                //// Apply Custom restriction for None type
                if (progTactic.Count() > 0)
                {

                    if (filteredCustomFields.Count > 0)
                    {
                        TacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, TacticIds, customfieldlist);
                        //// get Allowed Entity Ids
                        progTactic = progTactic.Where(tacticlist => TacticIds.Contains(tacticlist.PlanTacticId)).ToList();
                    }

                }
            }

            var FilterTacticIds = progTactic.Select(a => a.PlanTacticId).ToList();
            if (HoneycombIds != null)
            {
                var TacticIdsList = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Tactic.ToString())
                    .Select(x => int.Parse(x.Field<string>("ParentId"))).ToList();

                var ProgramIdsList = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Program.ToString())
                    .Select(x => int.Parse(x.Field<string>("ParentId"))).ToList();

                var CampaignIdsList = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Campaign.ToString())
                    .Select(x => int.Parse(x.Field<string>("ParentId"))).ToList();

                var PlanIdsList = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Plan.ToString())
                    .Select(x => int.Parse(x.Field<string>("ParentId"))).ToList();

                var MinParentId = 0;
                var MinSection = "";
                if (PlanIdsList.Count > 0)
                {
                    MinParentId = PlanIdsList.Min();
                    MinSection = Enums.Section.Plan.ToString();
                }
                else if (CampaignIdsList.Count > 0)
                {
                    MinParentId = CampaignIdsList.Min();
                    MinSection = Enums.Section.Campaign.ToString();
                }
                else if (ProgramIdsList.Count > 0)
                {
                    MinParentId = ProgramIdsList.Min();
                    MinSection = Enums.Section.Program.ToString();
                }
                else if (TacticIdsList.Count > 0)
                {
                    MinParentId = TacticIdsList.Min();
                    MinSection = Enums.Section.Tactic.ToString();
                }

                var dd = dtCSV
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<string>("ParentId") == Convert.ToString(MinParentId) && row.Field<string>("Section") == MinSection).ToList();

                for (int i = 0; i < dtCSV.Rows.Count; i++)
                {
                    if (dtTable.Rows.Count == 0)
                    {
                        var items = GetTopLevelRows(dtCSV, MinParentId, MinSection)
                                .Select(row => CreateItem(dtCSV, row, columnNames, dtTable, listOfClientId, PlanTacticIds))
                                .ToList();
                    }
                    else
                    {
                        MinParentId = Convert.ToInt32(dtCSV.Rows[i]["ParentId"].ToString());
                        MinSection = Convert.ToString(dtCSV.Rows[i]["Section"].ToString());
                        var EntityId = dtCSV.Rows[i]["EntityId"].ToString();
                        var CheckRecordExist = dtTable.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == MinSection && x.Field<string>("EntityId") == EntityId)
                            .Select(x => x.Field<string>("EntityId")).ToList();

                        if (CheckRecordExist.Count == 0)
                        {

                            var items = GetTopLevelRows(dtCSV, MinParentId, MinSection)
                                    .Select(row => CreateItem(dtCSV, row, columnNames, dtTable, listOfClientId, PlanTacticIds))
                                    .ToList();
                        }
                    }
                }

            }
            else
            {
                #region Default Hireachy
                for (int plan = 0; plan < PlanList.Count; plan++)
                {
                    totalmqlCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => TacticIds.Contains(l.PlanTacticId)).Sum(l => l.MQL) : 0;
                    totalrevenueCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => TacticIds.Contains(l.PlanTacticId)).Sum(l => l.Revenue) : 0;
                    DataRow[] dr = dtCSV.Select("EntityId = " + PlanList[plan] + "AND Section = '" + Enums.Section.Plan.ToString() + "'");

                    OwnerNameCsv = GetOwnerNameCSV(dr[0][Enums.NotDownloadCSV.CreatedBy.ToString()].ToString(), listOfClientId);
                    var CampList = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Campaign.ToString()
                        && int.Parse(x.Field<string>("ParentId")) == PlanList[plan]).OrderBy(x => x.Field<string>("Campaign")).Select(x => x.Field<int>("EntityId")).ToList();

                    var PlanProgramList = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Program.ToString()
                          && CampList.Contains(int.Parse(x.Field<string>("ParentId")))).Select(x => x.Field<int>("EntityId")).ToList();

                    var PlanTacticCostlist = dtCSVCost.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Tactic.ToString()
                                   && PlanProgramList.Contains(int.Parse(x.Field<string>("ParentId")))).Select(x => double.Parse(x.Field<string>("PlannedCost"))).ToList();

                    totalPlannedCostCSV = PlanTacticCostlist.Sum();

                    DataRowsInsert(dr, dtTable, columnNames);

                    for (int camp = 0; camp < CampList.Count; camp++)
                    {
                        totalmqlCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => CampList[camp] == l.CampaignId).Sum(l => l.MQL) : 0;
                        totalrevenueCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => CampList[camp] == l.CampaignId).Sum(l => l.Revenue) : 0;
                        var ProgramList = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Program.ToString()
                           && int.Parse(x.Field<string>("ParentId")) == CampList[camp]).OrderBy(x => x.Field<string>("Program")).Select(x => x.Field<int>("EntityId")).ToList();

                        var CampTacticCostlist = dtCSVCost.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Tactic.ToString()
                                       && ProgramList.Contains(int.Parse(x.Field<string>("ParentId")))).Select(x => double.Parse(x.Field<string>("PlannedCost"))).ToList();

                        totalPlannedCostCSV = CampTacticCostlist.Sum();

                        dr = dtCSV.Select("EntityId = " + CampList[camp] + "AND Section ='" + Enums.Section.Campaign.ToString() + "'");
                        OwnerNameCsv = GetOwnerNameCSV(dr[0][Enums.NotDownloadCSV.CreatedBy.ToString()].ToString(), listOfClientId);
                        DataRowsInsert(dr, dtTable, columnNames);

                        for (int prog = 0; prog < ProgramList.Count; prog++)
                        {
                            totalmqlCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => ProgramList[prog] == l.Programid).Sum(l => l.MQL) : 0;
                            totalrevenueCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => ProgramList[prog] == l.Programid).Sum(l => l.Revenue) : 0;
                            var ProgTacticCostlist = dtCSVCost.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Tactic.ToString()
                                       && int.Parse(x.Field<string>("ParentId")) == ProgramList[prog]).Select(x => double.Parse(x.Field<string>("PlannedCost"))).ToList();
                            totalPlannedCostCSV = ProgTacticCostlist.Sum();

                            dr = dtCSV.Select("EntityId = " + ProgramList[prog] + "AND Section = '" + Enums.Section.Program.ToString() + "'");
                            OwnerNameCsv = GetOwnerNameCSV(dr[0][Enums.NotDownloadCSV.CreatedBy.ToString()].ToString(), listOfClientId);
                            DataRowsInsert(dr, dtTable, columnNames);

                            var TacticList = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.Tactic.ToString()
                                && int.Parse(x.Field<string>("ParentId")) == ProgramList[prog]).OrderBy(x => x.Field<string>("Tactic")).Select(x => x.Field<int>("EntityId")).ToList();

                            for (int Tac = 0; Tac < TacticList.Count; Tac++)
                            {
                                if (FilterTacticIds.Contains(TacticList[Tac]))
                                {
                                    totalmqlCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => TacticList[Tac] == l.PlanTacticId).Sum(l => l.MQL) : 0;
                                    totalrevenueCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => TacticList[Tac] == l.PlanTacticId).Sum(l => l.Revenue) : 0;
                                    dr = dtCSV.Select("EntityId = " + TacticList[Tac] + "AND Section = '" + Enums.Section.Tactic.ToString() + "'");
                                    totalPlannedCostCSV = Convert.ToDouble(dr[0][Enums.DownloadCSV.PlannedCost.ToString()]);
                                    OwnerNameCsv = GetOwnerNameCSV(dr[0][Enums.NotDownloadCSV.CreatedBy.ToString()].ToString(), listOfClientId);
                                    DataRowsInsert(dr, dtTable, columnNames);

                                    var LineitemList = dtCSV.Rows.Cast<DataRow>().Where(x => x.Field<string>("Section") == Enums.Section.LineItem.ToString()
                                        && int.Parse(x.Field<string>("ParentId")) == TacticList[Tac]).OrderBy(x => x.Field<string>("LineItem")).Select(x => x.Field<int>("EntityId")).ToList();

                                    for (int line = 0; line < LineitemList.Count; line++)
                                    {
                                        totalmqlCSV = 0;
                                        totalrevenueCSV = 0;
                                        dr = dtCSV.Select("EntityId = " + LineitemList[line] + "AND Section = '" + Enums.Section.LineItem.ToString() + "'");
                                        totalPlannedCostCSV = Convert.ToDouble(dr[0][Enums.DownloadCSV.PlannedCost.ToString()]);
                                        OwnerNameCsv = GetOwnerNameCSV(dr[0][Enums.NotDownloadCSV.CreatedBy.ToString()].ToString(), listOfClientId);
                                        DataRowsInsert(dr, dtTable, columnNames);
                                    }
                                }
                            }
                        }

                    }
                }
                #endregion
            }
            dtTable.Columns.Remove(Enums.DownloadCSV.EntityId.ToString());
            dtTable.Columns.Remove(Enums.DownloadCSV.Section.ToString());
            //Added By Komal rawal for 2102 for honey comb export line item column not required
            if (HoneycombIds != null)
            {
                dtTable.Columns.Remove(Enums.DownloadCSV.Lineitem.ToString());
            }
            Session["CSVDataTable"] = dtTable;
            Session["FileName"] = FileName;
            return Json(new { data = FileName }, JsonRequestBehavior.AllowGet); ;
        }

        public void DataRowsInsert(DataRow[] dr, DataTable dt, DataColumnCollection columns)
        {
            for (int i = 0; i < dr.Length; i++)
            {
                DataRow row = dt.NewRow();
                for (int j = 0; j < columns.Count; j++)
                {
                    // Modified Condition By Nishant Sheth //#2345 :: TQL/Qualified Leads Values are not display in exported csv file
                    if (dr[i].Table.Columns.Contains(columns[j].ToString()) || dr[i].Table.Columns.Contains(Enums.DownloadCSV.MQL.ToString()))
                    {
                        string MqlColName = Convert.ToString(ViewBag.MQLTitle);
                        ViewBag.MQLTitle = MqlColName;

                        if (columns[j].ToString() == MqlColName)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(row[columns["Lineitem"].ToString()])))
                            {
                                row[columns[j].ToString()] = "--";
                            }
                            else
                            {
                                row[columns[j].ToString()] = (Int64)totalmqlCSV;
                            }
                        }
                        else if (columns[j].ToString() == Enums.DownloadCSV.Revenue.ToString())
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(row[columns["Lineitem"].ToString()])))
                            {
                                row[columns[j].ToString()] = "--";
                            }
                            else
                            {
                                row[columns[j].ToString()] = Sessions.PlanCurrencySymbol + Math.Round(totalrevenueCSV, 2);
                            }
                        }
                        else if (columns[j].ToString() == Enums.DownloadCSV.Owner.ToString())
                        {
                            row[columns[j].ToString()] = OwnerNameCsv;
                        }
                        else if (columns[j].ToString() == Enums.DownloadCSV.PlannedCost.ToString())
                        {
                            row[columns[j].ToString()] = Sessions.PlanCurrencySymbol + totalPlannedCostCSV;
                        }
                        else if (columns[j].ToString() == Enums.DownloadCSV.EndDate.ToString())
                        {
                            string EndDate = Convert.ToString(dr[i][columns[j].ToString()]).Split(';').Max();
                            row[columns[j].ToString()] = EndDate;
                        }
                        else
                        {

                            if (columns[j].ToString() == MqlColName)
                            {
                                row[MqlColName] = Convert.ToString(dr[i][Enums.DownloadCSV.MQL.ToString()]);
                            }
                            else
                            {
                                if (dr[i].Table.Columns.Contains(columns[j].ToString()))
                                {
                                    row[columns[j].ToString()] = Convert.ToString(dr[i][columns[j].ToString()]);
                                }
                                else
                                {
                                    row[columns[j].ToString()] = "--";
                                }
                            }
                        }
                    }
                    else
                    {
                        row[columns[j].ToString()] = Convert.ToString(DBNull.Value);
                    }
                }
                dt.Rows.Add(row);
                dt.AcceptChanges();
            }
        }

        private string HtmlDecodeString(string source)
        {
            source = source.Replace("&amp;", "&");
            source = source.Replace("–", "-");
            return HttpUtility.HtmlDecode(source.ToString());
        }

        //get rows of the top-level items
        IEnumerable<DataRow> GetTopLevelRows(DataTable dataTable, int minParentId, string Section)
        {
            return dataTable
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<string>("ParentId") == Convert.ToString(minParentId) && row.Field<string>("Section") == Section);
        }

        class CSVItem
        {
            public Int32 Id { get; set; }
            public String Section { get; set; }
            public IEnumerable<CSVItem> Children { get; set; }
        }

        //create an item including the child collection. This function will recurse down the hierarchy
        CSVItem CreateItem(DataTable dataTable, DataRow row, DataColumnCollection columnNames, DataTable insertDataTable, List<User> listOfClientId, List<int> TacticIdsList)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            var id = row.Field<Int32>("EntityId");
            var Section = row.Field<String>("Section");
            var ChildSection = Section;
            if (Section == Enums.Section.Plan.ToString())
            {
                ChildSection = Enums.Section.Campaign.ToString();
            }
            else if (Section == Enums.Section.Campaign.ToString())
            {
                ChildSection = Enums.Section.Program.ToString();
            }
            else if (Section == Enums.Section.Program.ToString())
            {
                ChildSection = Enums.Section.Tactic.ToString();
            }

            //var Section = row.Field<string>("Section");
            if (Section == Enums.Section.Plan.ToString())
            {
                totalmqlCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => TacticIdsList.Contains(l.PlanTacticId)).Sum(l => l.MQL) : 0;
                totalrevenueCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => TacticIdsList.Contains(l.PlanTacticId)).Sum(l => l.Revenue) : 0;
                totalPlannedCostCSV = LineItemList.Count > 0 ? LineItemList.Where(l => TacticIdsList.Contains(l.PlanTacticId)).Sum(l => l.Cost) : 0;
            }
            else if (Section == Enums.Section.Campaign.ToString())
            {
                totalmqlCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => l.CampaignId == id).Sum(l => l.MQL) : 0;
                totalrevenueCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => l.CampaignId == id).Sum(l => l.Revenue) : 0;
                totalPlannedCostCSV = LineItemList.Count > 0 ? LineItemList.Where(l => l.CampaignId == id).Sum(l => l.Cost) : 0;
            }
            else if (Section == Enums.Section.Program.ToString())
            {
                totalmqlCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => l.Programid == id).Sum(l => l.MQL) : 0;
                totalrevenueCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => l.Programid == id).Sum(l => l.Revenue) : 0;
                totalPlannedCostCSV = LineItemList.Count > 0 ? LineItemList.Where(l => l.Programid == id).Sum(l => l.Cost) : 0;
            }
            else if (Section == Enums.Section.Tactic.ToString())
            {
                totalmqlCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => l.PlanTacticId == id).Sum(l => l.MQL) : 0;
                totalrevenueCSV = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => l.PlanTacticId == id).Sum(l => l.Revenue) : 0;
                totalPlannedCostCSV = LineItemList.Count > 0 ? LineItemList.Where(l => l.PlanTacticId == id).Sum(l => l.Cost) : 0;
            }
            totalPlannedCostCSV = objCurrency.GetValueByExchangeRate(totalPlannedCostCSV, PlanExchangeRate); // Add By Nishant Sheth #2502 : Export csv with multi-currency
            DataRow[] dr = dataTable.Select("EntityId = " + id + "AND Section = '" + Section + "'");

            OwnerNameCsv = GetOwnerNameCSV(dr[0][Enums.NotDownloadCSV.CreatedBy.ToString()].ToString(), listOfClientId);

            DataRowsInsert(dr, insertDataTable, columnNames);

            var children = GetChildren(dataTable, id, ChildSection)
             .Select(r => CreateItem(dataTable, r, columnNames, insertDataTable, listOfClientId, TacticIdsList))
             .ToList();
            //return row;
            return new CSVItem { Id = id, Section = Section, Children = children };
        }

        //get the children of a specific item
        IEnumerable<DataRow> GetChildren(DataTable dataTable, Int32 parentId, string Section)
        {
            return dataTable
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<string>("ParentId") == Convert.ToString(parentId) && row.Field<string>("Section") == Section);
        }

        #endregion

    }
}

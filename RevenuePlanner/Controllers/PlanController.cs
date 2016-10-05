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
using System.Reflection;
using System.Web.Caching;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data.OleDb;
using Excel;
using System.Runtime.CompilerServices;

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

        #region Create

        /// <summary>
        /// Function to create Plan
        /// </summary>
        /// <param name="id">PlanId</param>
        /// <param name="isPlanSelecter">Flag to check that return from PlanSelecter.</param>
        /// <returns></returns>
        /// added By Komal rawal for new homePage Ui of Plan
        /// 
        public ActionResult CreatePlan(int id = 0, bool isPlanSelecter = false, bool isGridView = false, bool IsPlanChange = false)// Added by Komal Rawal for 2013 to identify grid view
        {
            /*Added by Mitesh Vaishnav on 25/07/2014 for PL ticket 619*/
            if (isPlanSelecter == true)
            {
                TempData["isPlanSelecter"] = true;
            }
            PlanExchangeRate = Sessions.PlanExchangeRate;
            /*End :Added by Mitesh Vaishnav on 25/07/2014 for PL ticket 619*/
            // Added by dharmraj to check user activity permission
            bool IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            ViewBag.IsPlanCreateAuthorized = IsPlanCreateAuthorized;

            bool IsPlanCreateAll = false;
            ViewBag.GridView = isGridView; // Added by Komal Rawal for 2013 to identify grid view
            ViewBag.IsPlanChange = IsPlanChange; // Added by Komal Rawal for 2072 to identify if plan is changed

            try
            {
                if (id > 0)
                {
                    // Added by Dharmraj Mangukiya for edit authentication of plan, PL ticket #519
                    var objplan = db.Plans.FirstOrDefault(m => m.PlanId == id && m.IsDeleted == false);
                    bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                    bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                    //Get all subordinates of current user upto n level
                    var lstSubOrdinates = new List<int>();
                    try
                    {
                        lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.ID);
                    }
                    catch (Exception e)
                    {
                        ErrorSignal.FromCurrentContext().Raise(e);

                        //To handle unavailability of BDSService
                        if (e is System.ServiceModel.EndpointNotFoundException)
                        {
                            return RedirectToAction("ServiceUnavailable", "Login");
                        }
                    }

                    //// Set flag to check whether his Own Plan & Subordinate Plan editable or not.
                    bool isPlanDefinationDisable = true;

                    if (IsPlanCreateAuthorized)
                    {
                        IsPlanCreateAll = true;
                    }
                    else
                    {
                        if (objplan.CreatedBy.Equals(Sessions.User.ID))
                        {
                            IsPlanCreateAll = true;
                        }
                    }
                    if (id == 0 && !IsPlanCreateAuthorized)
                    {
                        return AuthorizeUserAttribute.RedirectToNoAccess();
                    }
                    if (objplan.CreatedBy.Equals(Sessions.User.ID)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
                    {
                        isPlanDefinationDisable = false;
                    }
                    else if (IsPlanEditAllAuthorized)
                    {
                        isPlanDefinationDisable = false;
                    }
                    else if (IsPlanEditSubordinatesAuthorized)
                    {
                        if (lstSubOrdinates.Contains(objplan.CreatedBy))
                        {
                            isPlanDefinationDisable = false;
                        }
                    }
                    //Modified by Mitesh Vaishnav for internal review point related to "Edit All Plan" permission
                    ViewBag.IsPlanDefinationDisable = isPlanDefinationDisable;
                    ViewBag.IsPlanCreateAll = IsPlanCreateAll;

                }
                else
                {
                    ViewBag.IsPlanCreateAll = true;
                }

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
            }

            PlanModel objPlanModel = new PlanModel();
            try
            {
                ViewBag.ActiveMenu = Enums.ActiveMenu.Plan;
                Sessions.PlanId = id;/*added by Nirav for plan consistency on 14 apr 2014*/

                var List = GetModelName();
                if (List == null || List.Count == 0)
                {
                    //Modified by Mitesh Vaishnav for functional review point 64
                    TempData["IsNoModel"] = true;
                    return RedirectToAction("PlanSelector");
                    //End: Modified by Mitesh Vaishnav for functional review point 64

                }
                List.Insert(0, new PlanModel { ModelId = 0, ModelTitle = "select" }); // Added by dharmraj to add default select item in model dropdown
                TempData["selectList"] = new SelectList(List, "ModelId", "ModelTitle");
                /*Modified by Mitesh Vaishnav for PL ticket #622*/
                List<SelectListItem> Listyear = new List<SelectListItem>();
                int yr = DateTime.Now.Year;
                for (int i = 0; i < 5; i++)
                {
                    Listyear.Add(new SelectListItem { Text = (yr + i).ToString(), Value = (yr + i).ToString(), Selected = false });
                }
                var year = Listyear;
                TempData["selectYearList"] = new SelectList(year, "Value", "Text");
                /*End :Modified by Mitesh Vaishnav for PL ticket #622*/

                var GoalTypeList = Common.GetGoalTypeList(Sessions.User.CID);
                var AllocatedByList = Common.GetAllocatedByList();      // Added by Sohel Pathan on 05/08/2014

                //added by kunal to fill the plan data in edit mode - 01/17/2014
                if (id != 0)
                {
                    var objplan = db.Plans.Where(plan => plan.PlanId == id && plan.IsDeleted == false).FirstOrDefault();/*changed by Nirav for plan consistency on 14 apr 2014*/
                    objPlanModel.PlanId = objplan.PlanId;
                    objPlanModel.ModelId = objplan.ModelId;
                    objPlanModel.Title = objplan.Title;
                    objPlanModel.Year = objplan.Year;
                    objPlanModel.GoalType = GoalTypeList.Where(a => a.Value == objplan.GoalType).Select(a => a.Value).FirstOrDefault();
                    //objPlanModel.GoalValue = Convert.ToString(objplan.GoalValue);                  
                    if (Convert.ToString(objPlanModel.GoalType).ToUpper() == Enums.PlanGoalType.Revenue.ToString().ToUpper())
                    {
                        objPlanModel.GoalValue = objCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(objplan.GoalValue)), PlanExchangeRate).ToString();
                    }
                    else
                    {
                        objPlanModel.GoalValue = Convert.ToString(objplan.GoalValue);
                    }
                    objPlanModel.AllocatedBy = objplan.AllocatedBy;
                    //objPlanModel.Budget = objplan.Budget;
                    objPlanModel.Budget = objCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(objplan.Budget)), PlanExchangeRate);
                    objPlanModel.Version = objplan.Version;
                    objPlanModel.ModelTitle = objplan.Model.Title + " " + objplan.Model.Version;
                    double TotalAllocatedCampaignBudget = 0;
                    var PlanCampaignBudgetList = db.Plan_Campaign_Budget.Where(pcb => pcb.Plan_Campaign.PlanId == objplan.PlanId && pcb.Plan_Campaign.IsDeleted == false).Select(a => a.Value).ToList();
                    if (PlanCampaignBudgetList.Count > 0)
                    {
                        TotalAllocatedCampaignBudget = PlanCampaignBudgetList.Sum();
                    }
                    objPlanModel.TotalAllocatedCampaignBudget = TotalAllocatedCampaignBudget;
                    #region "In edit mode, plan year might be of previous year. We included previous year"
                    int planYear = 0; //plan's year
                    int.TryParse(objplan.Year, out planYear);
                    if (planYear != 0 && planYear < yr)
                    {
                        for (int i = planYear; i < yr; i++)
                        {
                            Listyear.Add(new SelectListItem { Text = (i).ToString(), Value = (i).ToString(), Selected = false });//Modified by Pratik for PL ticket #1089
                        }

                        year = Listyear.OrderBy(objyear => objyear.Value).ToList();//Modified by Pratik for PL ticket #1089
                        TempData["selectYearList"] = new SelectList(year, "Value", "Text");//Modified by Mitesh Vaishnav for PL ticket #622
                    }

                    //Added By Komal Rawal for #1176
                    if (objplan.Status == Enums.PlanStatus.Published.ToString())
                    {
                        bool IsModelCreateEdit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ModelCreateEdit);
                        ViewBag.modelcreateedit = IsModelCreateEdit;
                    }
                    else
                    {
                        ViewBag.modelcreateedit = true;

                    }
                    //End
                    #endregion
                }
                else
                {
                    objPlanModel.Title = "Plan Title";
                    objPlanModel.GoalValue = "0";
                    objPlanModel.Budget = 0;
                    objPlanModel.Year = DateTime.Now.Year.ToString(); // Added by dharmraj to add default year in year dropdown
                    ViewBag.modelcreateedit = true;
                }
                //end
                var IsQuarter = objPlanModel.Year;
                ViewBag.IsQuarter = IsQuarter;
                objPlanModel.objplanhomemodelheader = Common.GetPlanHeaderValue(id);

                TempData["goalTypeList"] = GoalTypeList;
                // TempData["allocatedByList"] = Common.GetAllocatedByList(); // Modified by Sohel Pathan on 05/08/2014
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return View(objPlanModel);

        }


        #region NoModel

        /// <summary>
        /// Call NoModel when no baseline model exist for current client.
        /// </summary>
        /// <returns></returns>
        public ActionResult NoModel()
        {
            return View();
        }

        #endregion  NoModel

        /// <summary>
        /// POST: Save Plan
        /// </summary>
        /// <param name="objPlanModel"></param>
        /// <param name="BudgetInputValues"></param>
        /// <param name="RedirectType"></param>
        /// <param name="UserId"></param> Added by Sohel Pathan on 07/08/2014 for PL ticket #672
        /// <returns></returns>
        [HttpPost]
        public JsonResult SavePlan(PlanModel objPlanModel, string BudgetInputValues = "", string RedirectType = "", int UserId = 0)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            try
            {
                // Start - Added by Sohel Pathan on 07/08/2014 for PL ticket #672
                if (UserId != 0)
                {
                    if (Sessions.User.ID != UserId)
                    {
                        TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                        return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                    }
                }
                // End - Added by Sohel Pathan on 07/08/2014 for PL ticket #672
                if (ModelState.IsValid)
                {
                    Plan plan = new Plan();
                    string oldAllocatedBy = "", newAllocatedBy = "";

                    //// Add Mode
                    if (objPlanModel.PlanId == 0)
                    {
                        string planDraftStatus = Enums.PlanStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.PlanStatus.Draft.ToString())).Value;
                        plan.Status = planDraftStatus;
                        plan.CreatedDate = System.DateTime.Now;
                        plan.CreatedBy = Sessions.User.ID;
                        plan.IsActive = true;
                        plan.IsDeleted = false;
                        double version = 0;
                        var plantable = db.Plans.Where(m => m.ModelId == objPlanModel.ModelId && m.IsActive == true && m.IsDeleted == false).FirstOrDefault();
                        if (plantable != null)
                        {
                            version = Convert.ToDouble(plantable.Version) + 0.1;
                        }
                        else
                        {
                            version = 1;
                        }
                        plan.Version = version.ToString();
                        plan.Title = objPlanModel.Title.Trim();
                        plan.GoalType = objPlanModel.GoalType;
                        if (objPlanModel.GoalValue != null)
                        {
                            plan.GoalValue = Convert.ToInt64(objPlanModel.GoalValue.Trim().Replace(",", "").Replace(Sessions.PlanCurrencySymbol, ""));
                            if (Convert.ToString(objPlanModel.GoalType).ToUpper() == Enums.PlanGoalType.Revenue.ToString().ToUpper())
                            {
                                plan.GoalValue = objCurrency.SetValueByExchangeRate(double.Parse(Convert.ToString(plan.GoalValue)), PlanExchangeRate);
                            }
                        }
                        else
                        {
                            plan.GoalValue = 0;
                        }
                        plan.AllocatedBy = objPlanModel.AllocatedBy;
                        plan.Budget = Convert.ToDouble(objPlanModel.Budget.ToString().Trim().Replace(",", "").Replace(Sessions.PlanCurrencySymbol, ""));
                        plan.ModelId = objPlanModel.ModelId;
                        plan.Year = objPlanModel.Year;
                        db.Plans.Add(plan);
                    }
                    else //// Edit Mode
                    {
                        plan = db.Plans.Where(_plan => _plan.PlanId == objPlanModel.PlanId).ToList().FirstOrDefault();

                        oldAllocatedBy = plan.AllocatedBy;
                        newAllocatedBy = objPlanModel.AllocatedBy;

                        //Check whether the user wants to switch the Model for this Plan
                        if (plan.ModelId != objPlanModel.ModelId)
                        {
                            //Check whether the Model switching is valid or not - check whether Model to switch to has all of the tactics present that are present in the plan
                            List<string> lstTactic = CheckModelTacticType(objPlanModel.PlanId, objPlanModel.ModelId);
                            if (lstTactic.Count() > 0)
                            {
                                string msg = RevenuePlanner.Helpers.Common.objCached.CannotSwitchModelForPlan.Replace("[TacticNameToBeReplaced]", String.Join(" ,", lstTactic));
                                return Json(new { id = -1, imsg = msg });
                            }
                            else
                            {
                                //Update the TacticTypeIds based on new Modeld
                                UpdateTacticType(objPlanModel.PlanId, objPlanModel.ModelId);
                            }
                        }

                        plan.Title = objPlanModel.Title.Trim();
                        plan.GoalType = objPlanModel.GoalType;
                        if (objPlanModel.GoalValue != null)
                        {
                            plan.GoalValue = Convert.ToInt64(objPlanModel.GoalValue.Trim().Replace(",", "").Replace(Sessions.PlanCurrencySymbol, ""));
                            if (Convert.ToString(objPlanModel.GoalType).ToUpper() == Enums.PlanGoalType.Revenue.ToString().ToUpper())
                            {
                                plan.GoalValue = objCurrency.SetValueByExchangeRate(double.Parse(Convert.ToString(plan.GoalValue)), PlanExchangeRate);
                            }

                        }
                        else
                        {
                            plan.GoalValue = 0;
                        }
                        plan.AllocatedBy = objPlanModel.AllocatedBy;
                        plan.Description = objPlanModel.Description;    /* Added by Sohel Pathan on 04/08/2014 for PL ticket #623 */
                        plan.Budget = Convert.ToDouble(objPlanModel.Budget.ToString().Trim().Replace(",", "").Replace(Sessions.PlanCurrencySymbol, ""));
                        plan.ModelId = objPlanModel.ModelId;
                        if (plan.Year != objPlanModel.Year) //// Added by Sohel Pathan on 12/01/2015 for PL ticket #1102
                        {
                            plan.Year = objPlanModel.Year;
                            Common.UpdatePlanYearOfActivities(objPlanModel.PlanId, Convert.ToInt32(objPlanModel.Year)); //// Added by Sohel Pathan on 12/01/2015 for PL ticket #1102
                        }
                        plan.ModifiedBy = Sessions.User.ID;
                        plan.ModifiedDate = System.DateTime.Now;
                        db.Entry(plan).State = EntityState.Modified;

                        // Start - Added by Sohel Pathan on 01/08/2014 for PL ticket #623
                        #region Update Budget Allocation Value
                        if (BudgetInputValues != "")
                        {
                            string[] arrBudgetInputValues = BudgetInputValues.Split(',');

                            var PrevPlanBudgetAllocationList = db.Plan_Budget.Where(pb => pb.PlanId == objPlanModel.PlanId).Select(pb => pb).ToList();

                            if (arrBudgetInputValues.Length == 12)
                            {
                                bool isExists;
                                Plan_Budget updatePlanBudget, objPlanBudget;
                                double newValue = 0;
                                for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                {
                                    // Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                    isExists = false;
                                    if (PrevPlanBudgetAllocationList != null && PrevPlanBudgetAllocationList.Count > 0)
                                    {
                                        updatePlanBudget = new Plan_Budget();
                                        updatePlanBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (i + 1))).FirstOrDefault();
                                        if (updatePlanBudget != null)
                                        {
                                            if (arrBudgetInputValues[i] != "")
                                            {
                                                newValue = Convert.ToDouble(arrBudgetInputValues[i]);
                                                if (updatePlanBudget.Value != newValue)
                                                {
                                                    updatePlanBudget.Value = newValue;
                                                    db.Entry(updatePlanBudget).State = EntityState.Modified;
                                                }
                                            }
                                            else
                                            {
                                                db.Entry(updatePlanBudget).State = EntityState.Deleted;
                                            }
                                            isExists = true;
                                        }
                                    }
                                    if (!isExists && arrBudgetInputValues[i] != "")
                                    {
                                        // End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                        objPlanBudget = new Plan_Budget();
                                        objPlanBudget.PlanId = objPlanModel.PlanId;
                                        objPlanBudget.Period = PeriodChar + (i + 1);
                                        objPlanBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                        objPlanBudget.CreatedBy = Sessions.User.ID;
                                        objPlanBudget.CreatedDate = DateTime.Now;
                                        db.Entry(objPlanBudget).State = EntityState.Added;
                                    }
                                }
                            }
                            else if (arrBudgetInputValues.Length == 4)
                            {
                                int QuarterCnt = 1, j = 1;
                                bool isExists;
                                List<Plan_Budget> thisQuartersMonthList;
                                Plan_Budget thisQuarterFirstMonthBudget, objPlanBudget;
                                double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;
                                for (int i = 0; i < 4; i++)
                                {
                                    // Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                    isExists = false;
                                    if (PrevPlanBudgetAllocationList != null && PrevPlanBudgetAllocationList.Count > 0)
                                    {
                                        thisQuartersMonthList = new List<Plan_Budget>();
                                        thisQuarterFirstMonthBudget = new Plan_Budget();
                                        thisQuartersMonthList = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                        thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                        if (thisQuarterFirstMonthBudget != null)
                                        {
                                            if (arrBudgetInputValues[i] != "")
                                            {
                                                thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                                thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                                newValue = Convert.ToDouble(arrBudgetInputValues[i]);

                                                if (thisQuarterTotalBudget != newValue)
                                                {
                                                    BudgetDiff = newValue - thisQuarterTotalBudget;
                                                    if (BudgetDiff > 0)
                                                    {
                                                        thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                                        db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        j = 1;
                                                        while (BudgetDiff < 0)
                                                        {
                                                            if (thisQuarterFirstMonthBudget != null)
                                                            {
                                                                BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                                if (BudgetDiff <= 0)
                                                                    thisQuarterFirstMonthBudget.Value = 0;
                                                                else
                                                                    thisQuarterFirstMonthBudget.Value = BudgetDiff;

                                                                db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                            }
                                                            if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                            {
                                                                thisQuarterFirstMonthBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                            }

                                                            j = j + 1;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                            }
                                            isExists = true;
                                        }
                                    }
                                    if (!isExists && arrBudgetInputValues[i] != "")
                                    {
                                        // End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                        objPlanBudget = new Plan_Budget();
                                        objPlanBudget.PlanId = objPlanModel.PlanId;
                                        objPlanBudget.Period = PeriodChar + QuarterCnt;
                                        objPlanBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                        objPlanBudget.CreatedBy = Sessions.User.ID;
                                        objPlanBudget.CreatedDate = DateTime.Now;
                                        db.Entry(objPlanBudget).State = EntityState.Added;
                                    }
                                    QuarterCnt = QuarterCnt + 3;
                                }
                            }
                        }
                        #endregion
                        // End - Added by Sohel Pathan on 01/08/2014 for PL ticket #623
                    }

                    int result = db.SaveChanges();

                    //// Insert Changelog.
                    if (objPlanModel.PlanId == 0)
                    {
                        Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added, "", plan.CreatedBy);
                    }
                    else
                    {
                        Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated, "", plan.CreatedBy);
                    }
                    if (result > 0)
                    {
                        Sessions.PlanId = plan.PlanId;
                        //Create default Plan Improvement Campaign, Program
                        int returnValue = CreatePlanImprovementCampaignAndProgram();
                    }

                    if (RedirectType.ToLower() == "budgeting")
                    {
                        TempData["SuccessMessage"] = Common.objCached.PlanSaved;
                        return Json(new { id = Sessions.PlanId, redirect = Url.Action("Budgeting", new { PlanId = Sessions.PlanId }) });
                    }
                    else
                    {
                        return Json(new { id = Sessions.PlanId, redirect = Url.Action("Assortment", new { ismsg = "Plan Saved Successfully." }) });
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { id = 0 });
        }

        //Added By Komal Rawal for new Home Page UI
        public JsonResult SavePlanDefination(PlanModel objPlanModel, int UserId = 0, bool IsPlanChange = false) //Modified BY Komal Rawal for #2072 to check if plan is changed
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            try
            {
                var ReloadAllFilters = false;
                if (UserId != 0)
                {
                    if (Sessions.User.ID != UserId)
                    {
                        TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                        return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                    }
                }

                if (ModelState.IsValid)
                {
                    Plan plan = new Plan();
                    string oldAllocatedBy = "", newAllocatedBy = "";

                    //// Add Mode
                    if (objPlanModel.PlanId == 0)
                    {
                        ReloadAllFilters = true;
                        string planDraftStatus = Enums.PlanStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.PlanStatus.Draft.ToString())).Value;
                        plan.Status = planDraftStatus;
                        plan.CreatedDate = System.DateTime.Now;
                        plan.CreatedBy = Sessions.User.ID;
                        plan.IsActive = true;
                        plan.IsDeleted = false;
                        double version = 0;
                        var plantable = db.Plans.Where(m => m.ModelId == objPlanModel.ModelId && m.IsActive == true && m.IsDeleted == false).FirstOrDefault();
                        if (plantable != null)
                        {
                            version = Convert.ToDouble(plantable.Version) + 0.1;
                        }
                        else
                        {
                            version = 1;
                        }
                        plan.Version = version.ToString();
                        plan.Title = objPlanModel.Title.Trim();
                        plan.GoalType = objPlanModel.GoalType;
                        if (objPlanModel.GoalValue != null)
                        {
                            plan.GoalValue = Convert.ToInt64(objPlanModel.GoalValue.Trim().Replace(",", "").Replace(Sessions.PlanCurrencySymbol, ""));
                            if (Convert.ToString(objPlanModel.GoalType).ToUpper() == Enums.PlanGoalType.Revenue.ToString().ToUpper())
                            {
                                plan.GoalValue = objCurrency.SetValueByExchangeRate(double.Parse(Convert.ToString(plan.GoalValue)), PlanExchangeRate);
                            }
                        }
                        else
                        {
                            plan.GoalValue = 0;
                        }
                        plan.AllocatedBy = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString();
                        //plan.Budget = Convert.ToDouble(objPlanModel.Budget.ToString().Trim().Replace(",", "").Replace("$", ""));
                        plan.Budget = objCurrency.SetValueByExchangeRate(double.Parse(Convert.ToString(objPlanModel.Budget).Trim().Replace(",", "").Replace(Sessions.PlanCurrencySymbol, "")), PlanExchangeRate);
                        plan.ModelId = objPlanModel.ModelId;
                        plan.Year = objPlanModel.Year;
                        db.Plans.Add(plan);
                    }
                    else //// Edit Mode
                    {
                        //Modified BY komal Rawal for #2072 to reload filters only if plan is changed in Plan defination
                        if (IsPlanChange == true)
                        {
                            ReloadAllFilters = true;
                        }
                        plan = db.Plans.Where(_plan => _plan.PlanId == objPlanModel.PlanId).ToList().FirstOrDefault();

                        oldAllocatedBy = plan.AllocatedBy;
                        newAllocatedBy = objPlanModel.AllocatedBy;

                        //Check whether the user wants to switch the Model for this Plan
                        if (plan.ModelId != objPlanModel.ModelId)
                        {
                            //Check whether the Model switching is valid or not - check whether Model to switch to has all of the tactics present that are present in the plan
                            List<string> lstTactic = CheckModelTacticType(objPlanModel.PlanId, objPlanModel.ModelId);
                            if (lstTactic.Count() > 0)
                            {
                                string msg = RevenuePlanner.Helpers.Common.objCached.CannotSwitchModelForPlan.Replace("[TacticNameToBeReplaced]", String.Join(" ,", lstTactic));
                                return Json(new { id = -1, imsg = msg });
                            }
                            else
                            {
                                //Update the TacticTypeIds based on new Modeld
                                UpdateTacticType(objPlanModel.PlanId, objPlanModel.ModelId);
                            }
                        }

                        plan.Title = objPlanModel.Title.Trim();
                        plan.GoalType = objPlanModel.GoalType;
                        if (objPlanModel.GoalValue != null)
                        {
                            plan.GoalValue = Convert.ToInt64(objPlanModel.GoalValue.Trim().Replace(",", "").Replace(Sessions.PlanCurrencySymbol, ""));
                            if (Convert.ToString(objPlanModel.GoalType).ToUpper() == Enums.PlanGoalType.Revenue.ToString().ToUpper())
                            {
                                plan.GoalValue = objCurrency.SetValueByExchangeRate(double.Parse(Convert.ToString(plan.GoalValue)), PlanExchangeRate);
                            }
                        }
                        else
                        {
                            plan.GoalValue = 0;
                        }
                        plan.AllocatedBy = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString();
                        plan.Description = objPlanModel.Description;
                        //plan.Budget = Convert.ToDouble(objPlanModel.Budget.ToString().Trim().Replace(",", "").Replace("$", ""));
                        plan.Budget = objCurrency.SetValueByExchangeRate(double.Parse(Convert.ToString(objPlanModel.Budget).Trim().Replace(",", "").Replace(Sessions.PlanCurrencySymbol, "")), PlanExchangeRate);
                        plan.ModelId = objPlanModel.ModelId;
                        if (plan.Year != objPlanModel.Year)
                        {
                            plan.Year = objPlanModel.Year;
                            Common.UpdatePlanYearOfActivities(objPlanModel.PlanId, Convert.ToInt32(objPlanModel.Year));
                        }
                        plan.ModifiedBy = Sessions.User.ID;
                        plan.ModifiedDate = System.DateTime.Now;
                        db.Entry(plan).State = EntityState.Modified;

                    }

                    int result = db.SaveChanges();

                    //// Insert Changelog.
                    if (objPlanModel.PlanId == 0)
                    {
                        Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added, "", plan.CreatedBy);
                    }
                    else
                    {
                        Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated, "", plan.CreatedBy);
                    }

                    if (result > 0)
                    {
                        Sessions.PlanId = plan.PlanId;
                        //Create default Plan Improvement Campaign, Program
                        int returnValue = CreatePlanImprovementCampaignAndProgram();
                    }
                    return Json(new { id = plan.PlanId, redirect = Url.Action("Index", "Home", new { activeMenu = Enums.ActiveMenu.Plan, currentPlanId = plan.PlanId, ismsg = "Plan Saved Successfully.", IsPlanSelector = ReloadAllFilters }) });

                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { id = 0 });
        }
        //End

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

        #endregion Create

        #region Switch Model for Plan

        /*Added by Kuber Joshi on 11 Apr 2014 for TFS Point 220 : Ability to switch models for a plan*/

        /// <summary>
        /// Check whether the Model switching for the Plan is valid or not
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        private List<string> CheckModelTacticType(int planId, int modelId)
        {
            //Check if any Tactic exists for this Plan
            var lstPlan_Campaign_Program_Tactic = (from pc in db.Plan_Campaign
                                                   join pcp in db.Plan_Campaign_Program on pc.PlanCampaignId equals pcp.PlanCampaignId
                                                   join pcpt in db.Plan_Campaign_Program_Tactic on pcp.PlanProgramId equals pcpt.PlanProgramId
                                                   where pc.PlanId == planId && pcpt.IsDeleted == false
                                                   select pcpt).ToList();
            if (lstPlan_Campaign_Program_Tactic == null || lstPlan_Campaign_Program_Tactic.Count() <= 0)
                return new List<string>();


            var objPlan_Campaign_Program_Tactic = lstPlan_Campaign_Program_Tactic.FirstOrDefault();
            if (objPlan_Campaign_Program_Tactic == null)
                return new List<string>();

            List<string> lstTacticType = GetTacticTypeListbyModelId(modelId);
            return (lstPlan_Campaign_Program_Tactic.Where(_tacType => !lstTacticType.Contains(_tacType.TacticType.Title)).Select(_tacType => _tacType.Title)).ToList();
        }

        /// <summary>
        /// Update the TacticTypeIds based on new Modeld
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        private void UpdateTacticType(int planId, int modelId)
        {
            //Check if any Tactic exists for this Plan
            var lstPlan_Campaign_Program_Tactic = (from pc in db.Plan_Campaign
                                                   join pcp in db.Plan_Campaign_Program on pc.PlanCampaignId equals pcp.PlanCampaignId
                                                   join pcpt in db.Plan_Campaign_Program_Tactic on pcp.PlanProgramId equals pcpt.PlanProgramId
                                                   where pc.PlanId == planId && pcpt.IsDeleted == false
                                                   select pcpt).ToList();
            if (lstPlan_Campaign_Program_Tactic == null || lstPlan_Campaign_Program_Tactic.Count() <= 0)
                return;

            var objPlan_Campaign_Program_Tactic = lstPlan_Campaign_Program_Tactic.FirstOrDefault();
            if (objPlan_Campaign_Program_Tactic == null)
                return;

            List<string> lstTacticType = GetTacticTypeListbyModelId(modelId);
            List<Plan_Campaign_Program_Tactic> lstTactic = lstPlan_Campaign_Program_Tactic.Where(_tacType => lstTacticType.Contains(_tacType.TacticType.Title)).Select(_tacType => _tacType).ToList();

            //// Update TacticType.
            foreach (var tactic in lstTactic)
            {
                if (tactic != null)
                {
                    int newTacticTypeId = db.TacticTypes.Where(tacType => tacType.ModelId == modelId && tacType.Title == tactic.TacticType.Title).Select(tacType => tacType.TacticTypeId).FirstOrDefault();
                    if (newTacticTypeId > 0)
                    {
                        tactic.ModifiedBy = Sessions.User.ID;
                        tactic.ModifiedDate = DateTime.Now;
                        tactic.TacticTypeId = newTacticTypeId; //Update TacticTypeId column in Plan_Campaign_Program_Tactic Table based on the new model selected
                        db.Entry(tactic).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }

        #endregion

        #region GetModelName
        /*Added by Nirav shah on 20 feb 2014 for TFS Point 252 : editing a published model*/
        /// <summary>
        /// Get last child
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private Model GetLatestModelVersion(Model obj)
        {
            Model objModel = (from model in db.Models where model.ParentModelId == obj.ModelId select model).FirstOrDefault();
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

            Model objModel = (from model in db.Models where model.ModelId == obj.ParentModelId select model).FirstOrDefault();
            if (objModel != null)
            {
                //// Check status that Published or not.
                if (Convert.ToString(objModel.Status).ToLower() == Convert.ToString(Enums.ModelStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.ModelStatus.Published.ToString())).Value).ToLower())
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
        /// Function to get model list
        /// </summary>
        /// <returns></returns>
        public List<PlanModel> GetModelName()
        {
            // Customer DropDown
            List<PlanModel> lstPlanModel = new List<PlanModel>();
            List<Model> objModelList = new List<Model>();
            List<Model> lstModels = new List<Model>();
            try
            {
                int clientId = Sessions.User.CID;
                string strPublish = Convert.ToString(Enums.ModelStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.ModelStatus.Published.ToString())).Value);
                string strDraft = Convert.ToString(Enums.ModelStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.ModelStatus.Draft.ToString())).Value);
                /*Added by Nirav shah on 20 feb 2014 for TFS Point 252 : editing a published model*/
                lstModels = (from mdl in db.Models
                             where mdl.IsDeleted == false && mdl.ClientId == clientId && (mdl.ParentModelId == 0 || mdl.ParentModelId == null)
                             select mdl).ToList();
                if (lstModels != null && lstModels.Count > 0)
                {
                    foreach (Model obj in lstModels)
                    {
                        objModelList.Add(GetLatestModelVersion(obj));
                    }
                }

                //// Insert Drafted Model record to List.
                List<Model> objModelDraftList = objModelList.Where(mdl => mdl.Status == strDraft).ToList();
                objModelList = objModelList.Where(mdl => mdl.Status == strPublish).ToList();

                if (objModelDraftList != null && objModelDraftList.Count > 0)
                {
                    foreach (Model obj in objModelDraftList)
                    {
                        objModelList.Add(GetLatestPublishedVersion(obj));
                    }
                }
                /*end Nirav changes*/
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            foreach (var model in objModelList)
            {
                if (model != null)
                {
                    PlanModel objPlanModel = new PlanModel();
                    objPlanModel.ModelId = model.ModelId;
                    objPlanModel.ModelTitle = model.Title + " " + model.Version;
                    lstPlanModel.Add(objPlanModel);
                }
            }
            return lstPlanModel.OrderBy(mdl => mdl.ModelTitle, new AlphaNumericComparer()).ToList();
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
        //commented by dhvani
        ///// <summary>
        ///// Added By : Sohel Pathan
        ///// Added Date : 22/09/2014
        ///// Description : Get plan/home header data for multiple plans
        ///// </summary>
        ///// <param name="strPlanIds">Comma separated list of plan ids</param>
        ///// <param name="activeMenu">Get Active Menu</param>
        ///// <returns>returns Json object with values required to show in plan/home header</returns>
        //public JsonResult GetPlanByMultiplePlanIDs(string planid, string activeMenu, string year, string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "", string TabId = "")
        //{
        //    planid = System.Web.HttpUtility.UrlDecode(planid);
        //    List<int> planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(p => int.Parse(p)).ToList();

        //    try
        //    {
        //        return Json(new
        //        {
        //            lstHomePlanModelHeader = Common.GetPlanHeaderValueForMultiplePlans(planIds, activeMenu, year, CustomFieldId, OwnerIds, TacticTypeids, StatusIds),
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e)
        //    {
        //        ErrorSignal.FromCurrentContext().Raise(e);
        //    }
        //    return Json(new { }, JsonRequestBehavior.AllowGet);
        //}
        //// Add By Nishant Sheth 
        //// Desc:: For get header values with sync proces and performance
        //public async Task<JsonResult> GetPlanByMultiplePlanIDsPer(string planid, string activeMenu, string year, string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "", string TabId = "")
        //{
        //    planid = System.Web.HttpUtility.UrlDecode(planid);
        //    List<int> planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(p => int.Parse(p)).ToList();

        //    try
        //    {
        //        await Task.Delay(1);
        //        return Json(new
        //        {
        //            lstHomePlanModelHeader = Common.GetPlanHeaderValueForMultiplePlansPer(planIds, activeMenu, year, CustomFieldId, OwnerIds, TacticTypeids, StatusIds),
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e)
        //    {
        //        ErrorSignal.FromCurrentContext().Raise(e);
        //    }
        //    return Json(new { }, JsonRequestBehavior.AllowGet);
        //}

        //Add by Komal Rawal on 12/09/2016
        //Desc : To get header values.
        public async Task<JsonResult> GetHeaderforPlanByMultiplePlanIDs(string planid, string activeMenu, string year, string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "" , bool IsGridView = false)
        {
            planid = System.Web.HttpUtility.UrlDecode(planid);
            List<int> planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(p => int.Parse(p)).ToList();

            try
            {
                await Task.Delay(1);
                return Json(new
                {
                    lstHomePlanModelHeader = Common.GetPlanHeaderValueForPlans(planIds, activeMenu, year, CustomFieldId, OwnerIds, TacticTypeids, StatusIds,IsGridView),
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
        /// Function to return ApplyToCalendar view.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 12/04/2013
        /// </summary>
        /// <param name="ismsg"></param>
        /// <param name="isError"></param>
        /// <returns>Returns view as action result.</returns>
        public ActionResult ApplyToCalendar(string ismsg = "", bool isError = false)
        {
            try
            {
                // To get permission status for Plan Edit , By dharmraj PL #519
                //Get all subordinates of current user upto n level
                var lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.ID);
                // Get current user permission for edit own and subordinates plans.
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                var objPlan = db.Plans.FirstOrDefault(_plan => _plan.PlanId == Sessions.PlanId);
                bool IsPlanEditable = false;

                // Added by dharmraj to check user activity permission
                bool IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
                ViewBag.IsPlanCreateAuthorized = IsPlanCreateAuthorized;

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


                //// Check whether his own & SubOrdinate Plan editable or Not.
                if (objPlan.CreatedBy.Equals(Sessions.User.ID)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditAllAuthorized)
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditSubordinatesAuthorized)
                {
                    if (lstOwnAndSubOrdinates.Contains(objPlan.CreatedBy))
                    {
                        IsPlanEditable = true;
                    }
                }
                ViewBag.IsPlanEditable = IsPlanEditable;
                ViewBag.IsPlanCreateAll = IsPlanCreateAll;

                //// Set Editable list of Campaign, Program, Tactic & ImprvementTactic Ids to ViewBag by PlanId.
                SetEditableListIdsByPlanId(Sessions.PlanId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
            }


            var plan = db.Plans.FirstOrDefault(_plan => _plan.PlanId.Equals(Sessions.PlanId));
            HomePlanModel planModel = new HomePlanModel();
            planModel.objplanhomemodelheader = Common.GetPlanHeaderValue(Sessions.PlanId);
            planModel.PlanId = plan.PlanId;
            planModel.PlanTitle = plan.Title;

            //// Modified By Maninder Singh Wadhva to Address PL#203           
            planModel.LastUpdatedDate = GetLastUpdatedDate(plan);

            /*changed by nirav Shah on 9 Jan 2014*/

            ViewBag.SuccessMessageDuplicatePlan = TempData["SuccessMessageDuplicatePlan"];
            ViewBag.ErrorMessageDuplicatePlan = TempData["ErrorMessageDuplicatePlan"];

            string strPreviousUrl = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : string.Empty;
            if (strPreviousUrl.ToLower().Contains("applytocalendar"))
            {

                //// Set Duplicate,Delete,Error Plan messages to TempData.
                if (TempData["SuccessMessageDuplicatePlan"] != null)
                {
                    if (TempData["SuccessMessageDuplicatePlan"].ToString() != string.Empty) ismsg = TempData["SuccessMessageDuplicatePlan"].ToString();
                }                   /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid
                  changed by : Nirav Shah on 13 feb 2014*/
                if (TempData["SuccessMessageDeletedPlan"] != null)
                {
                    if (TempData["SuccessMessageDeletedPlan"].ToString() != string.Empty) ismsg = TempData["SuccessMessageDeletedPlan"].ToString();
                }
                if (TempData["ErrorMessageDuplicatePlan"] != null)
                {
                    if (TempData["ErrorMessageDuplicatePlan"].ToString() != string.Empty)
                    {
                        ismsg = TempData["ErrorMessageDuplicatePlan"].ToString();
                        isError = true;
                    }
                }
            }

            ViewBag.Msg = ismsg;
            ViewBag.isError = isError;
            return View(planModel);
        }

        /// <summary>
        /// Function to return _ApplytoCalendarPlanList Partialview.
        /// </summary>
        /// <returns>Returns Partialview as action result.</returns>
        public ActionResult PlanList()
        {
            HomePlan objHomePlan = new HomePlan();
            List<SelectListItem> planList;
            /*changed by Nirav for plan consistency on 14 apr 2014*/
            int bId = Sessions.User.CID;
            planList = Common.GetPlan().Where(_plan => _plan.Model.ClientId == bId).Select(_plan => new SelectListItem() { Text = _plan.Title, Value = _plan.PlanId.ToString() }).OrderBy(_plan => _plan.Text).ToList();
            if (planList.Count > 0)
            {
                var objexists = planList.Where(_plan => _plan.Value == Sessions.PlanId.ToString()).ToList();
                if (objexists.Count != 0)
                {
                    planList.FirstOrDefault(_plan => _plan.Value.Equals(Sessions.PlanId.ToString())).Selected = true;
                }
                else
                {
                    planList.FirstOrDefault().Selected = true;
                    int planID = 0;
                    int.TryParse(planList.Select(_plan => _plan.Value).FirstOrDefault(), out planID);
                    Sessions.PlanId = planID;

                    //// if plan published then set planId to Session.
                    if (!Common.IsPlanPublished(Sessions.PlanId))
                    {
                        string planPublishedStatus = Enums.PlanStatus.Published.ToString();
                        var activeplan = db.Plans.Where(_plan => _plan.PlanId == Sessions.PlanId && _plan.IsDeleted == false && _plan.Status == planPublishedStatus).ToList();
                        if (activeplan.Count > 0)
                            Sessions.PublishedPlanId = planID;
                        else
                            Sessions.PublishedPlanId = 0;
                    }
                }
            }

            objHomePlan.plans = planList;

            return PartialView("_ApplytoCalendarPlanList", objHomePlan);
        }

        #region Get Collaborator Details

        /// <summary>
        /// Get Collaborator Details for current plan.
        /// Modified By Maninder Singh Wadhva to Address PL#203
        /// </summary>
        /// <param name="currentPlanId">PlanId</param>
        /// <returns>Json Result.</returns>
        public JsonResult GetCollaboratorDetails(int currentPlanId)
        {
            JsonResult collaboratorsImage = null;

            try
            {
                collaboratorsImage = Common.GetCollaboratorImage(currentPlanId);
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

            return Json(new
            {
                collaboratorsImage = collaboratorsImage,
                name = collaboratorsImage,
                lastUpdateDate = String.Format("{0:g}", Common.GetLastUpdatedDate(currentPlanId))
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        /// <summary>
        /// Function to get last updated date time for current plan.
        /// </summary>
        /// <param name="plan">Plan.</param>
        /// <returns>Returns last updated date time.</returns>
        private DateTime GetLastUpdatedDate(Plan plan)
        {
            List<DateTime?> lastUpdatedDate = new List<DateTime?>();

            //// Insert CreatedDate & ModiefiedDate to list.
            if (plan.CreatedDate != null)
                lastUpdatedDate.Add(plan.CreatedDate);

            if (plan.ModifiedDate != null)
                lastUpdatedDate.Add(plan.ModifiedDate);

            //// Get Tactic list by PlanId.
            var planTactic = db.Plan_Campaign_Program_Tactic.Where(_tac => _tac.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(_tac => _tac);

            if (planTactic.Count() > 0)
            {
                var planTacticModifiedDate = planTactic.ToList().Select(_tac => _tac.ModifiedDate).Max();
                lastUpdatedDate.Add(planTacticModifiedDate);

                var planTacticCreatedDate = planTactic.ToList().Select(_tac => _tac.CreatedDate).Max();
                lastUpdatedDate.Add(planTacticCreatedDate);

                var planProgramModifiedDate = planTactic.ToList().Select(_tac => _tac.Plan_Campaign_Program.ModifiedDate).Max();
                lastUpdatedDate.Add(planProgramModifiedDate);

                var planProgramCreatedDate = planTactic.ToList().Select(_tac => _tac.Plan_Campaign_Program.CreatedDate).Max();
                lastUpdatedDate.Add(planProgramCreatedDate);

                var planCampaignModifiedDate = planTactic.ToList().Select(_tac => _tac.Plan_Campaign_Program.Plan_Campaign.ModifiedDate).Max();
                lastUpdatedDate.Add(planCampaignModifiedDate);

                var planCampaignCreatedDate = planTactic.ToList().Select(_tac => _tac.Plan_Campaign_Program.Plan_Campaign.CreatedDate).Max();
                lastUpdatedDate.Add(planCampaignCreatedDate);

                //// Insert PlantTactic Comment created Date.
                var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(_tacComment => _tacComment.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId))
                                                                               .Select(_tacComment => _tacComment);
                if (planTacticComment.Count() > 0)
                {
                    var planTacticCommentCreatedDate = planTacticComment.ToList().Select(pc => pc.CreatedDate).Max();
                    lastUpdatedDate.Add(planTacticCommentCreatedDate);
                }
            }

            //// Return recent Plan Update Date.
            return Convert.ToDateTime(lastUpdatedDate.Max());
        }

        /// <summary>
        /// Function to get gantt data.
        /// Added By: Maninde Singh Wadhva.
        /// Modified By Maninder Singh Wadhva PL Ticket#47
        /// Date: 12/04/2013
        /// </summary>
        /// <param name="planId">Plan id for which gantt data to be fetched.</param>
        /// <param name="isQuater"></param>
        /// <returns>Json Result.</returns>
        public JsonResult GetGanttData(int planId, string isQuater)
        {
            Sessions.PlanId = planId;
            Plan plan = db.Plans.FirstOrDefault(_plan => _plan.PlanId.Equals(Sessions.PlanId));
            bool isPublished = plan.Status.Equals(Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString());
            List<object> ganttTaskData = new List<object>();
            try
            {
                ganttTaskData = GetTaskDetailTactic(plan, isQuater);
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
                    return Json(new { serviceUnavailable = "#" }, JsonRequestBehavior.AllowGet);
                }
            }


            //// Modified By Maninder Singh Wadhva PL Ticket#47
            List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(improveTactic => improveTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(planId) &&
                                                                                      improveTactic.IsDeleted.Equals(false) &&
                                                                                      (improveTactic.EffectiveDate > CalendarEndDate).Equals(false))
                                                                               .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

            ganttTaskData = Common.AppendImprovementTaskData(ganttTaskData, improvementTactic, CalendarStartDate, CalendarEndDate, true);

            //// Modified By Maninder Singh Wadhva PL Ticket#47
            #region "Tactic"
            return Json(new
            {
                taskData = ganttTaskData,
                planYear = plan.Year,
                isPublished = isPublished
            }, JsonRequestBehavior.AllowGet);
            #endregion
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 12/04/2013
        /// Function to get GANTT chart task detail for Tactic.
        /// Modified: To show task whose either start or end date or both date are outside current view.
        /// </summary>
        /// <param name="plan">Plan</param>
        /// <param name="isQuarter">Flag to indicate whether to fetch data for current Quarter.</param>
        /// <returns>Returns list of task for GANNT CHART.</returns>
        public List<object> GetTaskDetailTactic(Plan plan, string isQuater)
        {
            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;

            Common.GetPlanGanttStartEndDate(plan.Year, isQuater, ref CalendarStartDate, ref CalendarEndDate);

            var taskDataCampaign = db.Plan_Campaign.Where(_campgn => _campgn.PlanId.Equals(plan.PlanId) && _campgn.IsDeleted.Equals(false))
                                                   .ToList()
                                                   .Where(_campgn => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                            CalendarEndDate,
                                                                                                            _campgn.StartDate,
                                                                                                            _campgn.EndDate).Equals(false))
                                                    .Select(_campgn => new
                                                    {
                                                        id = string.Format("C{0}", _campgn.PlanCampaignId),
                                                        text = _campgn.Title,
                                                        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, _campgn.StartDate),
                                                        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                                                  CalendarEndDate,
                                                                                                  _campgn.StartDate,
                                                                                                  _campgn.EndDate),
                                                        progress = GetCampaignProgress(plan, _campgn),//progress = 0,
                                                        open = true,
                                                        color = Common.COLORC6EBF3_WITH_BORDER,
                                                        PlanCampaignId = _campgn.PlanCampaignId,
                                                        IsHideDragHandleLeft = _campgn.StartDate < CalendarStartDate,
                                                        IsHideDragHandleRight = _campgn.EndDate > CalendarEndDate,
                                                        Status = _campgn.Status       //// Added by Sohel on 19/05/2014 for PL #425 to Show status of tactics on ApplyToCalender screen
                                                    }).Select(_campgn => _campgn).OrderBy(_campgn => _campgn.text);

            var NewtaskDataCampaign = taskDataCampaign.Select(_campgn => new
            {
                id = _campgn.id,
                text = _campgn.text,
                start_date = _campgn.start_date,
                duration = _campgn.duration,
                progress = _campgn.progress,
                open = _campgn.open,
                color = _campgn.color + (_campgn.progress == 1 ? " stripe" : (_campgn.progress > 0 ? "stripe" : "")),
                PlanCampaignId = _campgn.PlanCampaignId,
                IsHideDragHandleLeft = _campgn.IsHideDragHandleLeft,
                IsHideDragHandleRight = _campgn.IsHideDragHandleRight,
                Status = _campgn.Status       //// Added by Sohel on 19/05/2014 for PL #425 to Show status of tactics on ApplyToCalender screen
            });

            var taskDataProgram = db.Plan_Campaign_Program.Where(prgrm => prgrm.Plan_Campaign.PlanId.Equals(plan.PlanId) &&
                                                                      prgrm.IsDeleted.Equals(false))
                                                          .ToList()
                                                          .Where(prgrm => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                  CalendarEndDate,
                                                                                                                  prgrm.StartDate,
                                                                                                                  prgrm.EndDate).Equals(false))
                                                          .Select(prgrm => new
                                                          {
                                                              id = string.Format("C{0}_P{1}", prgrm.PlanCampaignId, prgrm.PlanProgramId),
                                                              text = prgrm.Title,
                                                              start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, prgrm.StartDate),
                                                              duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                                                        CalendarEndDate,
                                                                                                        prgrm.StartDate,
                                                                                                        prgrm.EndDate),
                                                              progress = GetProgramProgress(plan, prgrm), //progress = 0,
                                                              open = true,
                                                              parent = string.Format("C{0}", prgrm.PlanCampaignId),
                                                              color = Common.COLOR27A4E5,
                                                              PlanProgramId = prgrm.PlanProgramId,
                                                              IsHideDragHandleLeft = prgrm.StartDate < CalendarStartDate,
                                                              IsHideDragHandleRight = prgrm.EndDate > CalendarEndDate,
                                                              Status = prgrm.Status     //// Added by Sohel on 19/05/2014 for PL #425 to Show status of tactics on ApplyToCalender screen
                                                          }).Select(prgrm => prgrm).Distinct().OrderBy(prgrm => prgrm.text).ToList();

            var NewtaskDataProgram = taskDataProgram.Select(task => new
            {
                id = task.id,
                text = task.text,
                start_date = task.start_date,
                duration = task.duration,
                progress = task.progress,
                open = task.open,
                parent = task.parent,
                color = task.color + (task.progress == 1 ? " stripe stripe-no-border" : (task.progress > 0 ? "stripe" : "")),
                PlanProgramId = task.PlanProgramId,
                IsHideDragHandleLeft = task.IsHideDragHandleLeft,
                IsHideDragHandleRight = task.IsHideDragHandleRight,
                Status = task.Status       //// Added by Sohel on 19/05/2014 for PL #425 to Show status of tactics on ApplyToCalender screen
            });

            // Added By Bhavesh : 25-June-2014 : #538 Custom Restriction
            var taskDataTactic = db.Plan_Campaign_Program_Tactic.Where(_tac => _tac.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId) &&
                                                                            _tac.IsDeleted.Equals(false))
                                                                .ToList()
                                                                .Where(_tac => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                        CalendarEndDate,
                                                                                                                        _tac.StartDate,
                                                                                                                        _tac.EndDate).Equals(false))
                                                                .Select(_tac => new
                                                                {
                                                                    id = string.Format("C{0}_P{1}_T{2}", _tac.Plan_Campaign_Program.PlanCampaignId, _tac.Plan_Campaign_Program.PlanProgramId, _tac.PlanTacticId),
                                                                    text = _tac.Title,
                                                                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, _tac.StartDate),
                                                                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                                                              CalendarEndDate,
                                                                                                              _tac.StartDate,
                                                                                                              _tac.EndDate),
                                                                    progress = GetTacticProgress(plan, _tac),//progress = 0,
                                                                    open = true,
                                                                    parent = string.Format("C{0}_P{1}", _tac.Plan_Campaign_Program.PlanCampaignId, _tac.Plan_Campaign_Program.PlanProgramId),
                                                                    color = Common.COLORC6EBF3_WITH_BORDER,
                                                                    plantacticid = _tac.PlanTacticId,
                                                                    IsHideDragHandleLeft = _tac.StartDate < CalendarStartDate,
                                                                    IsHideDragHandleRight = _tac.EndDate > CalendarEndDate,
                                                                    Status = _tac.Status       //// Added by Sohel on 19/05/2014 for PL #425 to Show status of tactics on ApplyToCalender screen
                                                                }).OrderBy(_tac => _tac.text).ToList();

            List<int> lstAllowedEntityIds = new List<int>();
            if (taskDataTactic.Count() > 0)
            {
                List<int> lstPlanTacticId = taskDataTactic.Select(tactic => tactic.plantacticid).Distinct().ToList();
                lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.ID, Sessions.User.CID, lstPlanTacticId, false);
            }

            var NewTaskDataTactic = taskDataTactic.Where(task => lstAllowedEntityIds.Count.Equals(0) || lstAllowedEntityIds.Contains(task.plantacticid)).Select(task => new
            {
                id = task.id,
                text = task.text,
                start_date = task.start_date,
                duration = task.duration,
                progress = task.progress,
                open = task.open,
                parent = task.parent,
                color = task.color + (task.progress == 1 ? " stripe" : ""),
                plantacticid = task.plantacticid,
                IsHideDragHandleLeft = task.IsHideDragHandleLeft,
                IsHideDragHandleRight = task.IsHideDragHandleRight,
                Status = task.Status       //// Added by Sohel on 19/05/2014 for PL #425 to Show status of tactics on ApplyToCalender screen
            });

            return NewtaskDataCampaign.Concat<object>(NewTaskDataTactic).Concat<object>(NewtaskDataProgram).ToList<object>();
        }

        /// <summary>
        /// Function to get tactic progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 2nd april, 2013
        /// <param name="plan"></param>
        /// <param name="planCampaignProgramTactic"></param>
        /// <returns>return progress B/W 0 and 1</returns>
        public double GetTacticProgress(Plan plan, Plan_Campaign_Program_Tactic planCampaignProgramTactic)
        {
            double result = 0;
            // List of all improvement tactic.
            List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(improveTactic => improveTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(plan.PlanId) &&
                                                                                      improveTactic.IsDeleted.Equals(false) &&
                                                                                      (improveTactic.EffectiveDate > CalendarEndDate).Equals(false))
                                                                               .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

            if (improvementTactic.Count > 0)
            {
                DateTime minDate = improvementTactic.Select(t => t.EffectiveDate).Min(); // Minimun date of improvement tactic

                DateTime tacticStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaignProgramTactic.StartDate)); // start Date of tactic

                if (tacticStartDate > minDate) // If any tactic affected by at least one improvement tactic.
                {
                    result = 1;
                }
            }
            return result;
        }

        /// <summary>
        /// Function to get program progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 2nd april, 2013
        /// <param name="plan"></param>
        /// <param name="planCampaignProgram"></param>
        /// <returns>return progress B/W 0 and 1</returns>
        public double GetProgramProgress(Plan plan, Plan_Campaign_Program planCampaignProgram)
        {
            double result = 0;
            // List of all improvement tactic.
            List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(improveTactic => improveTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(plan.PlanId) &&
                                                                                      improveTactic.IsDeleted.Equals(false) &&
                                                                                      (improveTactic.EffectiveDate > CalendarEndDate).Equals(false))
                                                                               .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

            if (improvementTactic.Count > 0)
            {
                DateTime minDate = improvementTactic.Select(_imprvTactic => _imprvTactic.EffectiveDate).Min(); // Minimun date of improvement tactic

                // Start date of program
                DateTime programStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaignProgram.StartDate));

                // List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = planCampaignProgram.Plan_Campaign_Program_Tactic.Where(_tac => _tac.IsDeleted.Equals(false) && (_tac.StartDate > minDate).Equals(true))
                                                                                              .Select(_tac => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, _tac.StartDate)) })
                                                                                              .ToList();

                if (lstAffectedTactic.Count > 0)
                {
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(_tac => _tac.startDate).Min(); // minimum start Date of tactics
                    if (tacticMinStartDate > minDate) // If any tactic affected by at least one improvement tactic.
                    {
                        double programDuration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, planCampaignProgram.StartDate, planCampaignProgram.EndDate);

                        // difference b/w program start date and tactic minimum date
                        double daysDifference = (tacticMinStartDate - programStartDate).TotalDays;

                        if (daysDifference > 0) // If no. of days are more then zero then it will return progress
                        {
                            result = (daysDifference / programDuration);
                        }
                        else
                        {
                            result = 1;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Function to get campaign progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 2nd april, 2013
        /// <param name="plan"></param>
        /// <param name="planCampaign"></param>
        /// <returns>return progress B/W 0 and 1</returns>
        public double GetCampaignProgress(Plan plan, Plan_Campaign planCampaign)
        {
            double result = 0;
            // List of all improvement tactic.
            List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(improveTactic => improveTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(plan.PlanId) &&
                                                                                      improveTactic.IsDeleted.Equals(false) &&
                                                                                      (improveTactic.EffectiveDate > CalendarEndDate).Equals(false))
                                                                               .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

            if (improvementTactic.Count > 0)
            {
                DateTime minDate = improvementTactic.Select(t => t.EffectiveDate).Min(); // Minimun date of improvement tactic

                // Start date of Campaign
                DateTime campaignStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaign.StartDate));

                // List of all tactics
                var lstTactic = db.Plan_Campaign_Program_Tactic.Where(_tac => _tac.Plan_Campaign_Program.PlanCampaignId.Equals(planCampaign.PlanCampaignId) &&
                                                                            _tac.IsDeleted.Equals(false))
                                                                .Select(_tac => _tac)
                                                                .ToList()
                                                                .Where(_tac => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                        CalendarEndDate,
                                                                                                                        _tac.StartDate,
                                                                                                                        _tac.EndDate).Equals(false));

                // List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = lstTactic.Where(_tac => (_tac.StartDate > minDate).Equals(true))
                                                 .Select(_tac => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, _tac.StartDate)) })
                                                 .ToList();

                if (lstAffectedTactic.Count > 0)
                {
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(t => t.startDate).Min(); // minimum start Date of tactics
                    if (tacticMinStartDate > minDate) // If any tactic affected by at least one improvement tactic.
                    {
                        double campaignDuration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, planCampaign.StartDate, planCampaign.EndDate);

                        // difference b/w campaign start date and tactic minimum date
                        double daysDifference = (tacticMinStartDate - campaignStartDate).TotalDays;

                        if (daysDifference > 0) // If no. of days are more then zero then it will return progress
                        {
                            result = (daysDifference / campaignDuration);
                        }
                        else
                        {
                            result = 1;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Function to update status of current plan.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 12/04/2013
        /// </summary>
        /// <param name="UserId">used id of logged in user</param>  Added by Sohel Pathan on 31/12/2014 for PL ticket #1059
        /// <returns>Returns ApplyToCalendar action result.</returns>
        [HttpPost]
        //   [ActionName("ApplyToCalendar")] 
        //Modified By Komal Rawal for new UI
        public ActionResult PublishPlan(int UserId = 0, int PlanId=0)
        {
            int IntPlanId = PlanId;
            try
            {
                //// Check cross user login check
                if (UserId != 0)
                {
                    if (Sessions.User.ID != UserId)
                    {
                        TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                        return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                    }
                }

                //// Update Plan status to Published.
                if (IntPlanId == 0)
                    IntPlanId = Sessions.PlanId;
                var plan = db.Plans.FirstOrDefault(p => p.PlanId.Equals(IntPlanId));
                plan.Status = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()];
                plan.ModifiedBy = Sessions.User.ID;
                plan.ModifiedDate = DateTime.Now;

                int returnValue = db.SaveChanges();
                Common.InsertChangeLog(Sessions.PlanId, 0, Sessions.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.published, "", plan.CreatedBy);
                ViewBag.ActiveMenu = RevenuePlanner.Helpers.Enums.ActiveMenu.Plan;
                return Json(new { activeMenu = Enums.ActiveMenu.Plan.ToString(), currentPlanId = Sessions.PlanId }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { errorMessage = Common.objCached.ErrorOccured.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 12/04/2013
        /// Function to update start and end date for tactic.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isPlanCampaign"></param>
        /// <param name="startDate">Start date field.</param>
        /// <param name="duration">Duration of task.</param>
        /// <param name="isPlanProgram"></param>
        /// <param name="isPlanTactic"></param>
        /// <returns>Returns json result that indicate whether date was updated successfully.</returns>
        public JsonResult UpdateStartEndDate(int id, string startDate, double duration, bool isPlanCampaign, bool isPlanProgram, bool isPlanTactic)
        {
            int returnValue = 0;
            if (isPlanCampaign)
            {
                //// Getting campaign to be updated.
                var planCampaign = db.Plan_Campaign.FirstOrDefault(pc => pc.PlanCampaignId.Equals(id));
                bool isApproved = planCampaign.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());

                //// Setting start and end date.
                planCampaign.StartDate = DateTime.Parse(startDate);
                DateTime endDate = DateTime.Parse(startDate);
                endDate = endDate.AddDays(duration);
                planCampaign.EndDate = endDate;

                //// Setting modified date and modified by field.
                planCampaign.ModifiedBy = Sessions.User.ID;
                planCampaign.ModifiedDate = DateTime.Now;

                //// Saving changes.
                returnValue = db.SaveChanges();
                if (isApproved)
                    Common.InsertChangeLog(Sessions.PlanId, 0, planCampaign.PlanCampaignId, planCampaign.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated, "", planCampaign.CreatedBy);

            }
            else if (isPlanProgram)
            {
                //// Getting program to be updated.
                var planProgram = db.Plan_Campaign_Program.FirstOrDefault(pc => pc.PlanProgramId.Equals(id));
                bool isApproved = planProgram.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());

                //// Setting start and end date.
                planProgram.StartDate = DateTime.Parse(startDate);
                DateTime endDate = DateTime.Parse(startDate);
                endDate = endDate.AddDays(duration);
                planProgram.EndDate = endDate;

                //// Setting modified date and modified by field.
                planProgram.ModifiedBy = Sessions.User.ID;
                planProgram.ModifiedDate = DateTime.Now;

                //// Saving changes.
                returnValue = db.SaveChanges();

                if (isApproved)
                    Common.InsertChangeLog(Sessions.PlanId, 0, planProgram.PlanProgramId, planProgram.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated, "", planProgram.CreatedBy);
            }
            else if (isPlanTactic)
            {
                //// Getting plan tactic to be updated.
                var planTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(pt => pt.PlanTacticId.Equals(id));

                bool isApproved = planTactic.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());

                try
                {
                    //// Changing status of tactic to submitted.
                    bool isDirectorLevelUser = false;
                    // Added by dharmraj for Ticket #537

                    var lstUserHierarchy = objBDSServiceClient.GetUserHierarchyEx(Sessions.User.CID, Sessions.ApplicationId);
                    var lstSubordinates = lstUserHierarchy.Where(u => u.MID == Sessions.User.ID).ToList().Select(u => u.UID).ToList();
                    if (lstSubordinates.Count > 0)
                    {
                        if (lstSubordinates.Contains(planTactic.CreatedBy))
                        {
                            isDirectorLevelUser = true;
                        }
                    }

                    if (!isDirectorLevelUser)
                    {
                        DateTime todaydate = DateTime.Now;
                        DateTime startDateform = DateTime.Parse(startDate);
                        DateTime endDateform = DateTime.Parse(startDate);
                        endDateform = endDateform.AddDays(duration);
                        /// Modified by:   Dharmraj
                        /// Modified date: 2-Sep-2014
                        /// Purpose:       #625 Changing the dates on an approved tactic needs to go through the approval process
                        // To check whether status is Approved or not
                        if (Common.CheckAfterApprovedStatus(planTactic.Status))
                        {
                            if (planTactic.EndDate != endDateform || planTactic.StartDate != startDateform)
                            {
                                planTactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                Common.mailSendForTactic(planTactic.PlanTacticId, planTactic.Status, planTactic.Title, section: Convert.ToString(Enums.Section.Tactic).ToLower());
                            }
                            else
                            {
                                if (todaydate > startDateform && todaydate < endDateform)
                                {
                                    planTactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                                }
                                else if (todaydate > planTactic.EndDate)
                                {
                                    planTactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                                }
                            }
                        }
                    }
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

                //End Manoj Limbachiya  Ticket #363 Tactic Creation - Do not automatically submit a tactic

                //// Setting start and end date.
                planTactic.StartDate = DateTime.Parse(startDate);
                DateTime endDate = DateTime.Parse(startDate);
                endDate = endDate.AddDays(duration);
                planTactic.EndDate = endDate;

                //// Setting modified date and modified by field.
                planTactic.ModifiedBy = Sessions.User.ID;
                planTactic.ModifiedDate = DateTime.Now;

                //// Saving changes.
                returnValue = db.SaveChanges();

                //// Start - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                //-- Update Camapign and Program according to tactic status
                Common.ChangeProgramStatus(planTactic.PlanProgramId, false);
                var PlanCampaignId = db.Plan_Campaign_Program.Where(prgrm => prgrm.IsDeleted.Equals(false) && prgrm.PlanProgramId == planTactic.PlanProgramId).Select(prgrm => prgrm.PlanCampaignId).FirstOrDefault();
                Common.ChangeCampaignStatus(PlanCampaignId, false);
                //// End - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425

                if (isApproved)
                    Common.InsertChangeLog(Sessions.PlanId, 0, planTactic.PlanTacticId, planTactic.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated, "", planTactic.CreatedBy);
            }

            //// Checking whether operation was successfully or not.
            if (returnValue > 0)
                return Json(true, JsonRequestBehavior.AllowGet);
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Assortment Mix

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Assortment.
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="programId"></param>
        /// <param name="tacticId"></param>
        /// <param name="EditObject"></param>
        /// <param name="ismsg"></param>
        /// <param name="isError"></param>
        /// <returns>Returns View Of Assortment.</returns>
        public ActionResult Assortment(int campaignId = 0, int programId = 0, int tacticId = 0, string ismsg = "", string EditObject = "", bool isError = false)
        {
            if (campaignId != 0)
            {
                if (EditObject == "ImprovementTactic")
                    Sessions.PlanId = db.Plan_Improvement_Campaign.Where(_campgn => _campgn.ImprovementPlanCampaignId == campaignId).Select(_campgn => _campgn.ImprovePlanId).FirstOrDefault();
                else
                    Sessions.PlanId = db.Plan_Campaign.Where(_campgn => _campgn.PlanCampaignId == campaignId).Select(_campgn => _campgn.PlanId).FirstOrDefault();
            }

            // Added by dharmraj to check user activity permission
            bool IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            ViewBag.IsPlanCreateAuthorized = IsPlanCreateAuthorized;

            Plan plan = db.Plans.FirstOrDefault(_plan => _plan.PlanId.Equals(Sessions.PlanId));
            if (plan != null)
            {
                // Added by Dharmraj Mangukiya for edit authentication of plan, PL ticket #519
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);

                //// Check Users own & Subordinate plan Editable or not.
                bool IsPlanCreateAll = false;
                if (IsPlanCreateAuthorized)
                {
                    IsPlanCreateAll = true;
                }
                else
                {
                    if (plan.CreatedBy.Equals(Sessions.User.ID))
                    {
                        IsPlanCreateAll = true;
                    }
                }
                if (IsPlanEditSubordinatesAuthorized)
                {
                    //Get all subordinates of current user upto n level
                    var lstSubOrdinates = new List<int>();
                    try
                    {
                        lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.ID);
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

                    if (lstSubOrdinates.Contains(plan.CreatedBy))
                    {
                        IsPlanCreateAll = true;
                    }
                }

                ViewBag.IsPlanCreateAll = IsPlanCreateAll;

            }


            ViewBag.PlanId = plan.PlanId;

            //// Set PlanModel defination data to ViewBag.
            PlanModel pm = new PlanModel();
            pm.ModelTitle = plan.Model.Title + " " + plan.Model.Version;
            pm.Title = plan.Title;
            var GoalTypeList = Common.GetGoalTypeList(Sessions.User.CID);
            pm.GoalType = plan.GoalType;
            pm.GoalTypeDisplay = GoalTypeList.Where(a => a.Value == plan.GoalType).Select(a => a.Text).FirstOrDefault();
            pm.GoalValue = plan.GoalValue.ToString();
            var AllocatedByList = Common.GetAllocatedByList();      // Added by Sohel Pathan on 11/08/2014 for PL ticket #566
            pm.AllocatedBy = AllocatedByList.Where(a => a.Value == plan.AllocatedBy).Select(a => a.Text).FirstOrDefault(); // Modified by Sohel Pathan on 11/08/2014 for PL ticket #566
            pm.ModelId = plan.ModelId;
            pm.Budget = plan.Budget;
            pm.Year = plan.Year;
            ViewBag.PlanDefinition = pm;

            //// Set ActiveMenu,CampaignId,ProgramId,TacticId values to ViewBag.
            ViewBag.ActiveMenu = Enums.ActiveMenu.Plan;
            ViewBag.CampaignID = (campaignId <= 0) ? Session["CampaignID"] : campaignId;
            ViewBag.ProgramID = (programId <= 0) ? Session["ProgramID"] : programId;
            ViewBag.TacticID = (tacticId <= 0) ? Session["TacticID"] : tacticId;
            Session["CampaignID"] = Session["ProgramID"] = Session["TacticID"] = 0;

            if (TempData["SuccessMessageDuplicatePlan"] != null)
            {
                if (TempData["SuccessMessageDuplicatePlan"].ToString() != string.Empty) ismsg = TempData["SuccessMessageDuplicatePlan"].ToString();
            }
            /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid
              changed by : Nirav Shah on 13 feb 2014*/
            if (TempData["SuccessMessageDeletedPlan"] != null)
            {
                if (TempData["SuccessMessageDeletedPlan"].ToString() != string.Empty) ismsg = TempData["SuccessMessageDeletedPlan"].ToString();
            }
            if (TempData["ErrorMessageDuplicatePlan"] != null)
            {
                if (TempData["ErrorMessageDuplicatePlan"].ToString() != string.Empty)
                {
                    ismsg = TempData["ErrorMessageDuplicatePlan"].ToString();
                    isError = true;
                }
            }

            ViewBag.EditOjbect = EditObject;
            ViewBag.Msg = ismsg;
            ViewBag.isError = isError;

            int improvementProgramId = db.Plan_Improvement_Campaign_Program.Where(prgrm => prgrm.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId).Select(prgrm => prgrm.ImprovementPlanProgramId).FirstOrDefault();
            if (improvementProgramId != 0)
            {
                ViewBag.ImprovementPlanProgramId = improvementProgramId;
            }
            else
            {
                CreatePlanImprovementCampaignAndProgram();
                ViewBag.ImprovementPlanProgramId = db.Plan_Improvement_Campaign_Program.Where(p => p.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId).Select(p => p.ImprovementPlanProgramId).FirstOrDefault();
            }
            string mqlStage = Enums.Stage.MQL.ToString();
            string MQLStageLabel = Common.GetLabel(Common.StageModeMQL);
            if (!string.IsNullOrEmpty(MQLStageLabel))
            {
                mqlStage = MQLStageLabel;
            }
            ViewBag.MQLLabel = mqlStage;
            return View("Assortment");
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Camapign , Program & Tactic.
        /// </summary>
        /// <returns>Returns Json Result.</returns>
        public JsonResult GetCampaign()
        {
            List<Plan_Campaign> CampaignList = db.Plan_Campaign.Where(pc => pc.PlanId.Equals(Sessions.PlanId) && pc.IsDeleted.Equals(false)).Select(pc => pc).ToList();
            List<int> CampaignIds = CampaignList.Select(pc => pc.PlanCampaignId).ToList();
            List<Plan_Campaign_Program> ProgramList = db.Plan_Campaign_Program.Where(pcp => CampaignIds.Contains(pcp.PlanCampaignId) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp).ToList();
            List<int> ProgramIds = ProgramList.Select(pcp => pcp.PlanProgramId).ToList();
            List<Plan_Campaign_Program_Tactic> TacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => ProgramIds.Contains(pcpt.PlanProgramId) && pcpt.IsDeleted.Equals(false)).Select(pcpt => pcpt).ToList();
            List<int> TacticIds = TacticList.Select(pcpt => pcpt.PlanTacticId).ToList();
            List<Plan_Campaign_Program_Tactic_LineItem> LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcptl => TacticIds.Contains(pcptl.PlanTacticId) && pcptl.IsDeleted.Equals(false)).Select(pcptl => pcptl).ToList();
            List<int> LineItemIds = LineItemList.Select(pcptl => pcptl.PlanLineItemId).ToList();
            bool IsTacticExist = false;
            if (TacticList != null && TacticList.Count > 0)
                IsTacticExist = true;
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTactic => _imprvTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && _imprvTactic.IsDeleted == false).Select(_imprvTactic => _imprvTactic).ToList();
            //Added By Bhavesh For Performance Issue #955
            List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
            List<StageList> stageListType = Common.GetStageList();
            int? ModelId = db.Plans.Where(_plan => _plan.PlanId == Sessions.PlanId).Select(_plan => _plan.ModelId).FirstOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(mdl => mdl.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(mdl => mdl.ModelId == ModelId).Select(mdl => mdl).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(mdl => mdl.ModelId).ToList());

            var improvementTacticTypeIds = improvementActivities.Select(imptype => imptype.ImprovementTacticTypeId).ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.CID && stage.IsDeleted == false).Select(stage => stage).ToList();

            List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelationForSinglePlan(TacticList, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, false);

            List<Plan_Tactic_Values> ListTacticMQLValue = (from tactic in TacticDataWithoutImprovement
                                                           select new Plan_Tactic_Values
                                                           {
                                                               PlanTacticId = tactic.TacticObj.PlanTacticId,
                                                               MQL = Math.Round(tactic.MQLValue, 0, MidpointRounding.AwayFromZero),
                                                               Revenue = tactic.RevenueValue
                                                           }).ToList();

            List<int> lstEditableTacticIds = new List<int>();
            List<int> lstAllowedEntityIds = new List<int>();
            if (CampaignList.Count() > 0)
            {
                List<int> lstPlanTacticIds = TacticList.Select(tactic => tactic.PlanTacticId).ToList();
                lstEditableTacticIds = Common.GetEditableTacticList(Sessions.User.ID, Sessions.User.CID, lstPlanTacticIds, false);
                lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.ID, Sessions.User.CID, lstPlanTacticIds, false);
            }

            //// Get Campaign data.
            var campaignobj = CampaignList.Select(p => new
            {
                id = p.PlanCampaignId,
                title = p.Title,
                description = p.Description,
                cost = LineItemList.Where(l => (TacticList.Where(pcpt => (ProgramList.Where(pcp => pcp.PlanCampaignId == p.PlanCampaignId).Select(pcp => pcp.PlanProgramId).ToList()).Contains(pcpt.PlanProgramId)).Select(pcpt => pcpt.PlanTacticId)).Contains(l.PlanTacticId)).Sum(l => l.Cost),
                mqls = ListTacticMQLValue.Where(lt => (TacticList.Where(pcpt => (ProgramList.Where(pcp => pcp.PlanCampaignId == p.PlanCampaignId).Select(pcp => pcp.PlanProgramId).ToList()).Contains(pcpt.PlanProgramId)).Select(pcpt => pcpt.PlanTacticId)).Contains(lt.PlanTacticId)).Sum(lt => lt.MQL),
                isOwner = Sessions.User.ID == p.CreatedBy ? 0 : 1,
                programs = (ProgramList.Where(pcp => pcp.PlanCampaignId.Equals(p.PlanCampaignId))).Select(pcpj => new
                {
                    id = pcpj.PlanProgramId,
                    title = pcpj.Title,
                    description = pcpj.Description,
                    cost = LineItemList.Where(l => (TacticList.Where(pcpt => pcpt.PlanProgramId == pcpj.PlanProgramId).Select(pcpt => pcpt.PlanTacticId)).Contains(l.PlanTacticId)).Sum(l => l.Cost),
                    mqls = ListTacticMQLValue.Where(lt => (TacticList.Where(pcpt => pcpt.PlanProgramId == pcpj.PlanProgramId).Select(pcpt => pcpt.PlanTacticId)).Contains(lt.PlanTacticId)).Sum(lt => lt.MQL),
                    isOwner = Sessions.User.ID == pcpj.CreatedBy ? 0 : 1,
                    tactics = (TacticList.Where(pcpt => pcpt.PlanProgramId.Equals(pcpj.PlanProgramId) && lstAllowedEntityIds.Contains(pcpt.PlanTacticId)).Select(pcpt => pcpt).ToList()).Select(pcptj => new
                    {
                        id = pcptj.PlanTacticId,
                        title = pcptj.Title,
                        description = pcptj.Description,
                        cost = LineItemList.Where(l => l.PlanTacticId == pcptj.PlanTacticId).Sum(l => l.Cost),
                        mqls = ListTacticMQLValue.Where(lt => lt.PlanTacticId == pcptj.PlanTacticId).Sum(lt => lt.MQL),
                        isOwner = Sessions.User.ID == pcptj.CreatedBy ? ((TacticIds.Count.Equals(0) || lstEditableTacticIds.Contains(pcptj.PlanTacticId)) ? 0 : 1) : 1,
                        lineitems = (LineItemList.Where(pcptl => pcptl.PlanTacticId.Equals(pcptj.PlanTacticId))).Select(pcptlj => new
                        {
                            id = pcptlj.PlanLineItemId,
                            type = pcptlj.LineItemTypeId,
                            title = pcptlj.Title,
                            cost = pcptlj.Cost
                        }).Select(pcptlj => pcptlj).Distinct().OrderByDescending(pcptlj => pcptlj.type).OrderBy(pc => pc.title, new AlphaNumericComparer())
                    }).Select(pcptj => pcptj).Distinct().OrderBy(pcptj => pcptj.id).OrderBy(pc => pc.title, new AlphaNumericComparer())
                }).Select(pcpj => pcpj).Distinct().OrderBy(pcpj => pcpj.id).OrderBy(pc => pc.title, new AlphaNumericComparer())
            }).Select(p => p).Distinct().OrderBy(p => p.id).OrderBy(pc => pc.title, new AlphaNumericComparer());

            //Start : Check isOwner flag for program and campaign based on tactics custom restrictions, Ticket #577, By Dharmraj
            var lstCampaignTmp = campaignobj.Select(c => new
            {
                id = c.id,
                title = c.title,
                description = c.description,
                cost = c.cost,
                mqls = c.mqls,
                isOwner = c.isOwner,
                programs = c.programs.Select(p => new
                {
                    id = p.id,
                    title = p.title,
                    description = p.description,
                    cost = p.cost,
                    mqls = p.mqls,
                    isOwner = p.isOwner == 0 ? ((TacticList.Where(tactic => tactic.PlanProgramId == p.id).Count() == p.tactics.Count()) ? (p.tactics.Any(t => t.isOwner == 1) ? 1 : 0) : 1) : 1,
                    tactics = p.tactics
                })
            });

            var lstCampaign = lstCampaignTmp.Select(campgn => new
            {
                id = campgn.id,
                title = campgn.title,
                description = campgn.description,
                cost = campgn.cost,
                mqls = campgn.mqls,
                isOwner = campgn.isOwner == 0 ? (campgn.programs.Any(p => p.isOwner == 1) ? 1 : 0) : 1,
                programs = campgn.programs,
                istacticexist = IsTacticExist
            });

            return Json(lstCampaign, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Added By: Dharmraj Mangukiya.
        /// Action to Get month/Quarter wise budget Value Of campaign.
        /// </summary>
        /// <param name="id">Plan Campaign Id.</param>
        /// <returns>Returns Json Result of campaign budget allocation Value.</returns>
        public JsonResult GetBudgetAllocationCampaignData(int id)
        {
            try
            {
                List<string> lstMonthly = Common.lstMonthly;

                var objPlanCampaign = db.Plan_Campaign.FirstOrDefault(campgn => campgn.PlanCampaignId == id && campgn.IsDeleted == false);    // Modified by Sohel Pathan on 04/09/2014 for PL ticket #758
                int planId = objPlanCampaign == null ? 0 : objPlanCampaign.PlanId;
                var objPlan = db.Plans.FirstOrDefault(_plan => _plan.PlanId == planId && _plan.IsDeleted == false);   // Modified by Sohel Pathan on 04/09/2014 for PL ticket #758

                var lstAllCampaign = db.Plan_Campaign.Where(_campgn => _campgn.PlanId == planId && _campgn.IsDeleted == false).ToList();
                var planCampaignId = lstAllCampaign.Select(_campgn => _campgn.PlanCampaignId);
                var lstAllProgram = db.Plan_Campaign_Program.Where(_prgram => _prgram.PlanCampaignId == id && _prgram.IsDeleted == false).ToList();
                var ProgramId = lstAllProgram.Select(_prgram => _prgram.PlanProgramId);

                // Add By Nishant Sheth 
                // Desc:: for add multiple years regarding #1765
                // To create the period of the year dynamically base on item period
                int GlobalYearDiffrence = 0;
                List<listMonthDynamic> lstMonthlyDynamicProgram = new List<listMonthDynamic>();
                List<listMonthDynamic> lstMonthlyDynamicCampaign = new List<listMonthDynamic>();

                lstAllProgram.ForEach(program =>
                {
                    List<string> lstMonthlyExtended = new List<string>();
                    int YearDiffrence = Convert.ToInt32(Convert.ToInt32(program.EndDate.Year) - Convert.ToInt32(program.StartDate.Year));

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
                    lstMonthlyDynamicProgram.Add(new listMonthDynamic { Id = program.PlanProgramId, listMonthly = lstMonthlyExtended });
                });


                lstAllCampaign.ForEach(camp =>
                {
                    List<string> lstMonthlyExtended = new List<string>();
                    int YearDiffrence = Convert.ToInt32(Convert.ToInt32(camp.EndDate.Year) - Convert.ToInt32(camp.StartDate.Year));
                    if (camp.PlanCampaignId == id)
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
                    lstMonthlyDynamicCampaign.Add(new listMonthDynamic { Id = camp.PlanCampaignId, listMonthly = lstMonthlyExtended });
                });




                //// Get list of Budget.
                // Change by Nishant sheth
                // Desc :: #1765 - add period condition to get value
                var lstCampaignBudget = db.Plan_Campaign_Budget.Where(_campgnBdgt => planCampaignId.Contains(_campgnBdgt.PlanCampaignId)).ToList()
                    .Where(_campgnBdgt => lstMonthlyDynamicCampaign.Where(a => a.Id == _campgnBdgt.PlanCampaignId).Select(a => a.listMonthly).FirstOrDefault().Contains(_campgnBdgt.Period))
                                                               .Select(_campgnBdgt => new
                                                               {
                                                                   _campgnBdgt.PlanCampaignBudgetId,
                                                                   _campgnBdgt.PlanCampaignId,
                                                                   _campgnBdgt.Period,
                                                                   _campgnBdgt.Value
                                                               }).ToList();

                // Change by Nishant sheth
                // Desc :: #1765 - add period condition to get value
                var lstProgramBudget = db.Plan_Campaign_Program_Budget.Where(_prgrmBdgt => ProgramId.Contains(_prgrmBdgt.PlanProgramId)).ToList()
                    .Where(_prgrmBdgt => lstMonthlyDynamicProgram.Where(a => a.Id == _prgrmBdgt.PlanProgramId).Select(a => a.listMonthly).FirstOrDefault().Contains(_prgrmBdgt.Period))
                                                               .Select(_prgrmBdgt => new
                                                               {
                                                                   _prgrmBdgt.PlanProgramBudgetId,
                                                                   _prgrmBdgt.PlanProgramId,
                                                                   _prgrmBdgt.Period,
                                                                   _prgrmBdgt.Value
                                                               }).ToList();

                var lstPlanBudget = db.Plan_Budget.Where(_plnBdgt => _plnBdgt.PlanId == planId);

                double allCampaignBudget = lstAllCampaign.Sum(_campgn => _campgn.CampaignBudget);
                double planBudget = objPlan.Budget;
                double planRemainingBudget = planBudget - allCampaignBudget;

                // Start - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                if (objPlan.AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString())
                {
                    //// Calculate Quarterly Budget Allocation Value.
                    List<PlanBudgetAllocationValue> lstPlanBudgetAllocationValue = new List<PlanBudgetAllocationValue>();
                    string[] quarterPeriods = Common.quarterPeriods;
                    for (int i = 0; i < (12 * (GlobalYearDiffrence + 1)); i++)
                    {
                        if ((i + 1) % 3 == 0)
                        {
                            PlanBudgetAllocationValue objPlanBudgetAllocationValue = new PlanBudgetAllocationValue();
                            objPlanBudgetAllocationValue.periodTitle = PeriodChar + (i - 1).ToString();
                            objPlanBudgetAllocationValue.budgetValue = lstCampaignBudget.Where(_campBdgt => quarterPeriods.Contains(_campBdgt.Period) && _campBdgt.PlanCampaignId == id).FirstOrDefault() == null ? "" : lstCampaignBudget.Where(_campBdgt => quarterPeriods.Contains(_campBdgt.Period) && _campBdgt.PlanCampaignId == id).Select(_campBdgt => _campBdgt.Value).Sum().ToString();
                            objPlanBudgetAllocationValue.remainingMonthlyBudget = (lstPlanBudget.Where(_plnBdgt => quarterPeriods.Contains(_plnBdgt.Period)).FirstOrDefault() == null ? 0 : lstPlanBudget.Where(_plnBdgt => quarterPeriods.Contains(_plnBdgt.Period)).Select(_plnBdgt => _plnBdgt.Value).Sum()) - (lstCampaignBudget.Where(_plnBdgt => quarterPeriods.Contains(_plnBdgt.Period)).Sum(_plnBdgt => _plnBdgt.Value));
                            objPlanBudgetAllocationValue.programMonthlyBudget = lstProgramBudget.Where(_prgrmBdgt => quarterPeriods.Contains(_prgrmBdgt.Period)).Select(_prgrmBdgt => _prgrmBdgt.Value).Sum();

                            /// Add into return list
                            lstPlanBudgetAllocationValue.Add(objPlanBudgetAllocationValue);

                            quarterPeriods = new string[] { PeriodChar + (i + 2), PeriodChar + (i + 3), PeriodChar + (i + 4) };
                        }
                    }

                    var objBudgetAllocationData = new { budgetData = lstPlanBudgetAllocationValue, planRemainingBudget = planRemainingBudget };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                // End - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                else
                {
                    //// Set Budget Data.
                    // Change by Nishant sheth
                    // Desc :: #1765 - add period condition to get value
                    var budgetData = lstMonthlyDynamicCampaign.FirstOrDefault().listMonthly != null ? (lstMonthlyDynamicCampaign.Where(a => a.Id == id).FirstOrDefault().listMonthly.Select(period => new
                    {
                        periodTitle = period,
                        budgetValue = lstCampaignBudget.FirstOrDefault(_campBdgt => _campBdgt.Period == period && _campBdgt.PlanCampaignId == id) == null ? "" : lstCampaignBudget.FirstOrDefault(_campBdgt => _campBdgt.Period == period && _campBdgt.PlanCampaignId == id).Value.ToString(),
                        remainingMonthlyBudget = (lstPlanBudget.FirstOrDefault(_plnBdgt => _plnBdgt.Period == period) == null ? 0 : lstPlanBudget.FirstOrDefault(_plnBdgt => _plnBdgt.Period == period).Value) - (lstCampaignBudget.Where(_campgnBdgt => _campgnBdgt.Period == period).Sum(_campgnBdgt => _campgnBdgt.Value)),
                        programMonthlyBudget = lstProgramBudget.Where(_prgrmBdgt => _prgrmBdgt.Period == period).Sum(_prgrmBdgt => _prgrmBdgt.Value)
                    })) : (lstMonthly.Select(period => new
                    {
                        periodTitle = period,
                        budgetValue = lstCampaignBudget.FirstOrDefault(_campBdgt => _campBdgt.Period == period && _campBdgt.PlanCampaignId == id) == null ? "" : lstCampaignBudget.FirstOrDefault(_campBdgt => _campBdgt.Period == period && _campBdgt.PlanCampaignId == id).Value.ToString(),
                        remainingMonthlyBudget = (lstPlanBudget.FirstOrDefault(_plnBdgt => _plnBdgt.Period == period) == null ? 0 : lstPlanBudget.FirstOrDefault(_plnBdgt => _plnBdgt.Period == period).Value) - (lstCampaignBudget.Where(_campgnBdgt => _campgnBdgt.Period == period).Sum(_campgnBdgt => _campgnBdgt.Value)),
                        programMonthlyBudget = lstProgramBudget.Where(_prgrmBdgt => _prgrmBdgt.Period == period).Sum(_prgrmBdgt => _prgrmBdgt.Value)
                    }));

                    var objBudgetAllocationData = new { budgetData = budgetData, planRemainingBudget = planRemainingBudget };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Delete Campaign.
        /// </summary>
        /// <param name="id">Campaign Id.</param>
        /// <param name="RedirectType"></param>
        /// <param name="closedTask"></param>
        /// <param name="UserId"></param>
        /// <param name="CalledFromBudget"></param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid
            changed by : Nirav Shah on 13 feb 2014
            Changed : add new Parameter  RedirectType
         */
        public ActionResult DeleteCampaign(int id = 0, bool RedirectType = false, string closedTask = null, int UserId = 0, string CalledFromBudget = "")
        {
            //// Check whether UserId is loggined User or Not.
            if (UserId != 0)
            {
                if (Sessions.User.ID != UserId)
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        ////Modified by Mitesh Vaishnav for functional review point - removing sp
                        int returnValue = Common.PlanTaskDelete(Enums.Section.Campaign.ToString(), id);
                        int cid = 0;
                        int pid = 0;
                        string Title = "";

                        if (returnValue != 0)
                        {
                            Plan_Campaign pc = db.Plan_Campaign.Where(campgn => campgn.PlanCampaignId == id).FirstOrDefault();
                            Title = pc.Title;
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pc.PlanCampaignId, pc.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed, "", pc.CreatedBy);
                            if (returnValue >= 1)
                            {
                                scope.Complete();
                                /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid
                                  changed by : Nirav Shah on 13 feb 2014
                                  Changed : set message and based on request redirect page.
                                */

                                if (!string.IsNullOrEmpty(CalledFromBudget))
                                {
                                    TempData["SuccessMessage"] = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                                    return Json(new { redirect = Url.Action("Budgeting", new { PlanId = Sessions.PlanId, type = CalledFromBudget }) });
                                }
                                else
                                {
                                    TempData["SuccessMessageDeletedPlan"] = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                                    if (RedirectType)
                                    {
                                        if (closedTask != null)
                                        {
                                            TempData["ClosedTask"] = closedTask;
                                        }
                                        return Json(new { redirect = Url.Action("ApplyToCalendar") });
                                    }
                                    else
                                    {
                                        return Json(new { redirect = Url.Action("Assortment", new { campaignId = cid, programId = pid }) });
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

            return Json(new { });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Delete Program.
        /// </summary>
        /// <param name="id">Program Id.</param>
        /// <param name="RedirectType"></param>
        /// <param name="closedTask"></param>
        /// <param name="UserId"></param>
        /// <param name="CalledFromBudget"></param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/
        public ActionResult DeleteProgram(int id = 0, bool RedirectType = false, string closedTask = null, int UserId = 0, string CalledFromBudget = "")
        {
            //// Check whether UserId is loggined User or Not.
            if (UserId != 0)
            {
                if (Sessions.User.ID != UserId)
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        ////Modified by Mitesh Vaishnav for functional review point - removing sp
                        int returnValue = Common.PlanTaskDelete(Enums.Section.Program.ToString(), id);
                        int cid = 0;
                        int pid = 0;
                        string Title = "";

                        if (returnValue != 0)
                        {
                            Plan_Campaign_Program pc = db.Plan_Campaign_Program.Where(prgrm => prgrm.PlanProgramId == id).FirstOrDefault();
                            cid = pc.PlanCampaignId;
                            Title = pc.Title;
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pc.PlanProgramId, pc.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed, "", pc.CreatedBy);
                            if (returnValue >= 1)
                            {
                                Common.ChangeCampaignStatus(pc.PlanCampaignId, false);     //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                scope.Complete();
                                /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/

                                if (!string.IsNullOrEmpty(CalledFromBudget))
                                {
                                    TempData["SuccessMessage"] = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                                    return Json(new { redirect = Url.Action("Budgeting", new { PlanId = Sessions.PlanId, type = CalledFromBudget, expand = "campaign" + cid.ToString() }) });
                                }
                                else
                                {
                                    TempData["SuccessMessageDeletedPlan"] = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                                    if (RedirectType)
                                    {
                                        if (closedTask != null)
                                        {
                                            TempData["ClosedTask"] = closedTask;
                                        }
                                        return Json(new { redirect = Url.Action("ApplyToCalendar") });
                                    }
                                    else
                                    {
                                        return Json(new { redirect = Url.Action("Assortment", new { campaignId = cid, programId = pid }) });
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

            return Json(new { });
        }

        /// <summary>
        /// Action to Delete Tactic.
        /// </summary>
        /// <param name="id">Tactic Id.</param>
        /// <param name="RedirectType"></param>
        /// <param name="closedTask"></param>
        /// <param name="UserId"></param>
        /// <param name="CalledFromBudget"></param>
        /// <returns>Returns Action Result.</returns>
        /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     
         * changed by : Nirav Shah on 13 feb 2014
           Add delete tactic feature
         */
        [HttpPost]
        public ActionResult DeleteTactic(int id = 0, bool RedirectType = false, string closedTask = null, int UserId = 0, string CalledFromBudget = "")
        {
            //// Check whether UserId is loggined User or Not.
            if (UserId != 0)
            {
                if (Sessions.User.ID != UserId)
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        ////Modified by Mitesh Vaishnav for functional review point - removing sp
                        int returnValue = Common.PlanTaskDelete(Enums.Section.Tactic.ToString(), id);
                        int cid = 0;
                        int pid = 0;
                        string Title = "";

                        if (returnValue != 0)
                        {
                            Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(_tac => _tac.PlanTacticId == id).FirstOrDefault();
                            cid = pcpt.Plan_Campaign_Program.PlanCampaignId;
                            pid = pcpt.PlanProgramId;
                            Title = pcpt.Title;
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcpt.PlanTacticId, pcpt.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed, "", pcpt.CreatedBy);
                            if (returnValue >= 1)
                            {
                                //// Start - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                Common.ChangeProgramStatus(pcpt.PlanProgramId, false);
                                var PlanCampaignId = db.Plan_Campaign_Program.Where(_prgrm => _prgrm.IsDeleted.Equals(false) && _prgrm.PlanProgramId == pcpt.PlanProgramId).Select(_prgrm => _prgrm.PlanCampaignId).FirstOrDefault();
                                Common.ChangeCampaignStatus(PlanCampaignId, false);
                                //// End - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                scope.Complete();


                                if (!string.IsNullOrEmpty(CalledFromBudget))
                                {
                                    TempData["SuccessMessage"] = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                                    return Json(new { redirect = Url.Action("Budgeting", new { PlanId = Sessions.PlanId, type = CalledFromBudget, expand = "program" + pid.ToString() }) });
                                }
                                else
                                {
                                    TempData["SuccessMessageDeletedPlan"] = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                                    if (RedirectType)
                                    {
                                        if (closedTask != null)
                                        {
                                            TempData["ClosedTask"] = closedTask;
                                        }
                                        return Json(new { redirect = Url.Action("ApplyToCalendar") });
                                    }
                                    else
                                    {
                                        return Json(new { redirect = Url.Action("Assortment", new { campaignId = cid, programId = pid }) });
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
            return Json(new { });
        }

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

        /// <summary>
        /// Added By Mitesh
        /// Action to delete line item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="RedirectType"></param>
        /// <param name="closedTask"></param>
        /// <param name="UserId"></param>
        /// <param name="CalledFromBudget"></param>
        /// <returns></returns>
        public ActionResult DeleteLineItem(int id = 0, bool RedirectType = false, string closedTask = null, int UserId = 0, string CalledFromBudget = "")
        {
            //// Check whether UserId is loggined User or Not.
            if (UserId != 0)
            {
                if (Sessions.User.ID != UserId)
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        ////Modified by Mitesh Vaishnav for functional review point - removing sp
                        int returnValue = Common.PlanTaskDelete(Enums.Section.LineItem.ToString(), id);
                        int cid = 0;
                        int pid = 0;
                        int tid = 0;
                        string Title = "";

                        if (returnValue != 0)
                        {
                            Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.Where(p => p.PlanLineItemId == id).FirstOrDefault();

                            // Start added by dharmraj to handle "Other" line item
                            //// Handle "Other" LineItem.
                            var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == pcptl.Plan_Campaign_Program_Tactic.PlanTacticId && l.LineItemTypeId == null);
                            double totalLoneitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == pcptl.Plan_Campaign_Program_Tactic.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
                            if (objOtherLineItem == null)
                            {
                                Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                objNewLineitem.PlanTacticId = pcptl.Plan_Campaign_Program_Tactic.PlanTacticId;
                                // Modified by Arpita Soni for Ticket #2634 on 09/22/2016
                                objNewLineitem.Title = Common.LineItemTitleDefault;
                                if (pcptl.Plan_Campaign_Program_Tactic.Cost > totalLoneitemCost)
                                {
                                    objNewLineitem.Cost = pcptl.Plan_Campaign_Program_Tactic.Cost - totalLoneitemCost;
                                }
                                else
                                {
                                    objNewLineitem.Cost = 0;
                                }
                                objNewLineitem.Description = string.Empty;
                                objNewLineitem.CreatedBy = Sessions.User.ID;
                                objNewLineitem.CreatedDate = DateTime.Now;
                                db.Entry(objNewLineitem).State = EntityState.Added;
                                db.SaveChanges();
                            }
                            else
                            {
                                objOtherLineItem.IsDeleted = false;
                                if (pcptl.Plan_Campaign_Program_Tactic.Cost > totalLoneitemCost)
                                {
                                    objOtherLineItem.Cost = pcptl.Plan_Campaign_Program_Tactic.Cost - totalLoneitemCost;
                                }
                                else
                                {
                                    objOtherLineItem.Cost = 0;
                                    objOtherLineItem.IsDeleted = true;
                                }
                                db.Entry(objOtherLineItem).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            // End added by dharmraj to handle "Other" line item

                            //// Get Campaign,Program,Tactic Id.
                            cid = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId;
                            pid = pcptl.Plan_Campaign_Program_Tactic.PlanProgramId;
                            tid = pcptl.PlanTacticId;
                            Title = pcptl.Title;
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcptl.PlanLineItemId, pcptl.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed, "", pcptl.CreatedBy);
                            if (returnValue >= 1)
                            {
                                //// Start - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                var planProgramId = pcptl.Plan_Campaign_Program_Tactic.PlanProgramId;
                                Common.ChangeProgramStatus(planProgramId, false);
                                var PlanCampaignId = db.Plan_Campaign_Program.Where(_prgrm => _prgrm.IsDeleted.Equals(false) && _prgrm.PlanProgramId == pcptl.Plan_Campaign_Program_Tactic.PlanProgramId).Select(_prgrm => _prgrm.PlanCampaignId).FirstOrDefault();
                                Common.ChangeCampaignStatus(PlanCampaignId, false);
                                //// End - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425

                                scope.Complete();

                                //// Handle Message & Return URL.
                                if (!string.IsNullOrEmpty(CalledFromBudget))
                                {
                                    TempData["SuccessMessage"] = string.Format("Line Item {0} deleted successfully", Title);
                                    return Json(new { redirect = Url.Action("Budgeting", new { PlanId = Sessions.PlanId, type = CalledFromBudget, expand = "tactic" + tid.ToString() }) });
                                }
                                else
                                {
                                    TempData["SuccessMessageDeletedPlan"] = string.Format("Line Item {0} deleted successfully", Title);
                                    if (RedirectType)
                                    {
                                        if (closedTask != null)
                                        {
                                            TempData["ClosedTask"] = closedTask;
                                        }
                                        return Json(new { redirect = Url.Action("ApplyToCalendar") });
                                    }
                                    else
                                    {
                                        return Json(new { redirect = Url.Action("Assortment", new { campaignId = cid, programId = pid, tacticId = tid }) });
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
            return Json(new { });
        }
        //End :Added by Mitesh Vaishnav for PL ticket 619

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

                    if (!string.IsNullOrEmpty(CalledFromBudget))
                    {
                        TempData["SuccessMessage"] = strMessage;
                        TempData["SuccessMessageDeletedPlan"] = "";
                        //return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget }) });

                        string expand = CloneType.ToLower().Replace(" ", "");
                        if (expand == "campaign")
                            return Json(new { redirect = Url.Action("Budgeting", new { PlanId = Id, type = CalledFromBudget }), Id = rtResult, msg = strMessage });
                        else
                            return Json(new { redirect = Url.Action("Budgeting", new { PlanId = Id, type = CalledFromBudget, expand = expand + Id.ToString() }), Id = rtResult, msg = strMessage });
                    }
                    else
                    {
                        ViewBag.CampaignID = Session["CampaignID"];
                        TempData["SuccessMessageDeletedPlan"] = strMessage;
                        return Json(new { redirect = Url.Action("Index", "Home", new { activeMenu = Enums.ActiveMenu.Plan, currentPlanId = Id }), planId = Id, Id = rtResult, msg = strMessage });
                    }
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

        #region Plan Selector screen

        /// <summary>
        /// This method will return the view for Plan selector screen
        /// </summary>
        /// <returns></returns>
        public ActionResult PlanSelector()
        {

            ViewBag.ActiveMenu = Enums.ActiveMenu.Plan;
            try
            {
                // To get permission status for Plan create, By dharmraj PL #519
                ViewBag.IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return View();
        }

        /// <summary>
        /// This method will return the JSON result for Plan listing
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        public JsonResult GetPlanSelectorData(string Year)
        {
            string str_Year = Convert.ToString(!string.IsNullOrEmpty(Year) ? Convert.ToInt32(Year) : 0);
            int Int_Year = Convert.ToInt32(!string.IsNullOrEmpty(Year) ? Convert.ToInt32(Year) : 0);
            List<Plan> objPlan = new List<Plan>();
            List<Plan_Selector> lstPlanSelector = new List<Plan_Selector>();
            try
            {
                int clientId = Sessions.User.CID;

                // Get the list of plan, filtered by Business Unit and Year selected
                if (Int_Year > 0)
                {
                    var modelids = db.Models.Where(model => model.ClientId == clientId && model.IsDeleted == false).Select(model => model.ModelId).ToList();
                    objPlan = (from p in db.Plans
                               where modelids.Contains(p.ModelId) && p.IsDeleted == false && p.Year == str_Year
                               select p).OrderByDescending(p => p.ModifiedDate ?? p.CreatedDate).ThenBy(p => p.Title).ToList();
                }
                List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.CID && stage.IsDeleted == false).Select(stage => stage).ToList();
                if (objPlan != null && objPlan.Count > 0)
                {
                    //Get all subordinates of current user upto n level
                    var lstOwnAndSubOrdinates = new List<int>();

                    try
                    {
                        lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.ID);
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

                    // Get current user permission for edit own and subordinates plans.
                    bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                    // To get permission status for Plan Edit, By dharmraj PL #519
                    bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);

                    var modelids = objPlan.Where(plan => plan.GoalType.ToLower() != Enums.PlanGoalType.MQL.ToString().ToLower()).Select(plan => plan.ModelId).ToList();

                    //int modelfunnelmarketingid = db.Funnels.Where(funnel => funnel.Title == marketing).Select(funnel => funnel.FunnelId).FirstOrDefault();
                    //var modelFunnelList = db.Model_Funnel.Where(modelfunnel => modelids.Contains(modelfunnel.ModelId) && modelfunnel.FunnelId == modelfunnelmarketingid).ToList();
                    //var modelfunnelids = modelFunnelList.Select(modelfunnel => modelfunnel.ModelFunnelId).ToList();
                    string CR = Enums.StageType.CR.ToString();
                    List<Model_Stage> modelFunnelStageList = db.Model_Stage.Where(modelfunnelstage => modelids.Contains(modelfunnelstage.ModelId) && modelfunnelstage.StageType == CR).ToList();
                    foreach (var item in objPlan)
                    {
                        var LastUpdated = !string.IsNullOrEmpty(Convert.ToString(item.ModifiedDate)) ? item.ModifiedDate : item.CreatedDate;
                        Plan_Selector objPlanSelector = new Plan_Selector();
                        objPlanSelector.PlanId = item.PlanId;
                        objPlanSelector.PlanTitle = item.Title;
                        objPlanSelector.LastUpdated = LastUpdated.Value.Date.ToString("M/d/yy");
                        // Start - Modified by Sohel Pathan on 15/07/2014 for PL ticket #566
                        if (item.GoalType.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower())
                        {
                            item.GoalValue = item.GoalValue.Equals(double.NaN) ? 0 : item.GoalValue;  // Added by Viral Kadiya on 11/24/2014 to resolve PL ticket #990.
                            objPlanSelector.MQLS = item.GoalValue.ToString("#,##0");
                        }
                        else
                        {
                            // Get ADS value

                            double ADSValue = item.Model.AverageDealSize;
                            // List<int> funnelids = modelFunnelList.Where(modelfunnel => modelfunnel.ModelId == item.ModelId).Select(modelfunnel => modelfunnel.ModelFunnelId).ToList();
                            objPlanSelector.MQLS = Common.CalculateMQLOnly(item.ModelId, item.GoalType, item.GoalValue.ToString(), ADSValue, stageList, modelFunnelStageList).ToString("#,##0"); ;
                        }
                        // End - Modified by Sohel Pathan on 15/07/2014 for PL ticket #566
                        objPlanSelector.Budget = (item.Budget).ToString("#,##0");
                        objPlanSelector.Status = item.Status;

                        // Start - Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                        bool IsPlanEditable = false;
                        // Added to check edit status for current user by dharmraj for #538
                        if (item.CreatedBy.Equals(Sessions.User.ID)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
                        {
                            IsPlanEditable = true;
                        }
                        else if (IsPlanEditAllAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                        {
                            IsPlanEditable = true;
                        }
                        else if (IsPlanEditSubordinatesAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                        {
                            if (lstOwnAndSubOrdinates.Contains(item.CreatedBy))
                            {
                                IsPlanEditable = true;
                            }
                        }

                        objPlanSelector.IsPlanEditable = IsPlanEditable;

                        // End - Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units

                        lstPlanSelector.Add(objPlanSelector);
                    }
                }

                lstPlanSelector = lstPlanSelector.OrderBy(planlist => planlist.PlanTitle, new AlphaNumericComparer()).ToList();
                return Json(new { lstPlanSelector }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Method to get the list of years
        /// </summary>
        /// <returns></returns>
        public JsonResult GetYearsTab()
        {
            int clientId = Sessions.User.CID;
            var objPlan = (from _pln in db.Plans
                           join _mdl in db.Models on _pln.ModelId equals _mdl.ModelId
                           where _mdl.ClientId == clientId && _mdl.IsDeleted == false && _pln.IsDeleted == false
                           select _pln).OrderBy(_pln => _pln.Year).ToList();

            /* Modified by Sohel on 08/04/2014 for PL #424 to Show year's tab starting from left to right i.e. 2010, 2011, 2012..., Ordering has been changed.*/
            var lstYears = objPlan.OrderByDescending(_pln => _pln.Year).Select(_pln => _pln.Year).Distinct().Take(10).ToList();

            return Json(lstYears, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Json method to delete the plan
        /// </summary>
        /// <param name="PlanId">Plan id</param>
        /// <param name="UserId">User id of logged in user</param>
        /// <returns>returns json result object</returns>
        public JsonResult DeletePlan(int PlanId, int UserId = 0)
        {
            //// check whether UserId is currently loggined user or not.
            if (UserId != 0)
            {
                if (Sessions.User.ID != UserId)
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            int returnValue = 0;
            if (PlanId > 0)
            {
                //// Delete Plan
                returnValue = Common.PlanTaskDelete(Enums.Section.Plan.ToString(), PlanId);
            }
            if (returnValue > 0)
            {
                //// Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                return Json(new { successmsg = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Plan.ToString()]) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var PlanName = db.Plans.Where(p => p.PlanId == PlanId).ToList().Select(p => p.Title).FirstOrDefault();
                return Json(new { errorMsg = string.Format(Common.objCached.PlanDeleteError, PlanName) }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region "Boost Method"
        /// <summary>
        /// Function to update effective date of improvement tactic.
        /// Added By Maninder Singh Wadhva for PL Ticket#47.
        /// </summary>
        /// <param name="id">Improvement tactic id.</param>
        /// <param name="effectiveDate">Effective date.</param>
        /// <returns>Returns flag to indicate whether effective date is updated successfully or not.</returns>
        public JsonResult UpdateEffectiveDateImprovement(int id, string effectiveDate)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    //// Getting plan tactic to be updated.
                    var planImprovementTactic = db.Plan_Improvement_Campaign_Program_Tactic.FirstOrDefault(improvementTactic => improvementTactic.ImprovementPlanTacticId.Equals(id));

                    bool isApproved = planImprovementTactic.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());

                    if (isApproved)
                    {
                        //// Add condition if it is approved than it require to change status as submitted.
                        //// Changing status of tactic to submitted.
                        planImprovementTactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                    }
                    //// Setting start and end date.
                    planImprovementTactic.EffectiveDate = DateTime.Parse(effectiveDate);

                    //// Setting modified date and modified by field.
                    planImprovementTactic.ModifiedBy = Sessions.User.ID;
                    planImprovementTactic.ModifiedDate = DateTime.Now;

                    //// Saving changes.
                    int returnValue = db.SaveChanges();

                    if (isApproved)
                    {
                        returnValue = Common.InsertChangeLog(planImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, 0, planImprovementTactic.ImprovementPlanTacticId, planImprovementTactic.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated, "", planImprovementTactic.CreatedBy);
                    }

                    if (returnValue > 0)
                    {
                        scope.Complete();
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to create default Plan Improvement Campaign & Program
        /// </summary>
        /// <returns>Returns ImprovementPlanCampaignId or -1 if duplicate exists</returns>
        public int CreatePlanImprovementCampaignAndProgram()
        {
            int retVal = -1;

            try
            {
                //Fetch Plan details
                Plan_Improvement_Campaign objPlan = new Plan_Improvement_Campaign();
                objPlan = db.Plan_Improvement_Campaign.Where(_cmpgn => _cmpgn.ImprovePlanId == Sessions.PlanId).FirstOrDefault();
                if (objPlan == null)
                {
                    // Setup default title for improvement campaign.
                    string planImprovementCampaignTitle = Common.ImprovementActivities;

                    //// Insert Campaign data to Plan_Improvement_Campaign table.
                    Plan_Improvement_Campaign picobj = new Plan_Improvement_Campaign();
                    picobj.ImprovePlanId = Sessions.PlanId;
                    picobj.Title = planImprovementCampaignTitle;
                    picobj.CreatedBy = Sessions.User.ID;
                    picobj.CreatedDate = DateTime.Now;
                    db.Entry(picobj).State = EntityState.Added;
                    int result = db.SaveChanges();
                    retVal = picobj.ImprovementPlanCampaignId;
                    if (retVal > 0)
                    {
                        //// Insert Program data to Plan_Improvement_Campaign_Program table.
                        Plan_Improvement_Campaign_Program pipobj = new Plan_Improvement_Campaign_Program();
                        pipobj.CreatedBy = Sessions.User.ID;
                        pipobj.CreatedDate = DateTime.Now;
                        pipobj.ImprovementPlanCampaignId = retVal;
                        // Setup default title for improvement Program.
                        pipobj.Title = Common.ImprovementProgram;
                        db.Entry(pipobj).State = EntityState.Added;
                        result = db.SaveChanges();
                        retVal = pipobj.ImprovementPlanProgramId;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return retVal;
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Improvement Tactic list for Plan Campaign.
        /// </summary>
        /// <returns>Returns Json Result.</returns>
        public JsonResult GetImprovementTactic(int PlanId)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            //Modified By Komal Rawal for #1432 to get improvement tactics according to current plan .
            var tactics = db.Plan_Improvement_Campaign_Program_Tactic.Where(pc => pc.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(PlanId) && pc.IsDeleted.Equals(false)).Select(pc => pc).ToList();
            var tacticobj = tactics.Select(_tac => new
            {
                id = _tac.ImprovementPlanTacticId,
                title = _tac.Title,
                cost = objCurrency.GetValueByExchangeRate(_tac.Cost, PlanExchangeRate), // Modified By Nishant Sheth // To apply Multi-Currency rates
                ImprovementProgramId = _tac.ImprovementPlanProgramId,
                isOwner = Sessions.User.ID == _tac.CreatedBy ? 0 : 1,
            }).Select(_tac => _tac).Distinct().OrderBy(_tac => _tac.id).OrderBy(tac => tac.title, new AlphaNumericComparer());

            return Json(tacticobj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Delete improvement tactic.
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="RedirectType">Redirect Type</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteImprovementTactic(int id = 0, bool RedirectType = false)
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
                            if (RedirectType)
                            {
                                return Json(new { redirect = Url.Action("ApplyToCalendar") });
                            }
                            else
                            {
                                return Json(new { redirect = Url.Action("Assortment") });
                            }
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

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Delete Improvement Tactic.
        /// </summary>
        /// <param name="id">Improvement Tactic Id.</param>
        /// <param name="AssortmentType"></param>
        /// <param name="RedirectType"></param>
        /// <returns>Returns Partial View Of Delete Improvement Tactic.</returns>
        public PartialViewResult ShowDeleteImprovementTactic(int id = 0, bool AssortmentType = false, bool RedirectType = false)
        {
            ViewBag.AssortmentType = AssortmentType;
            ViewBag.ImprovementPlanTacticId = id;
            ViewBag.RedirectType = RedirectType;
            int ImprovementTacticTypeId = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTac => _imprvTac.ImprovementPlanTacticId == id).Select(_imprvTac => _imprvTac.ImprovementTacticTypeId).FirstOrDefault();
            DateTime EffectiveDate = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTac => _imprvTac.ImprovementPlanTacticId == id).Select(_imprvTac => _imprvTac.EffectiveDate).FirstOrDefault();
            List<ImprovementStage> ImprovementMetric = GetImprovementStages(id, ImprovementTacticTypeId, EffectiveDate);
            string CR = Enums.StageType.CR.ToString();
            string SV = Enums.StageType.SV.ToString();
            double conversionRateHigher = ImprovementMetric.Where(im => im.StageType == CR).Select(im => im.PlanWithTactic).Sum();
            double conversionRateLower = ImprovementMetric.Where(im => im.StageType == CR).Select(im => im.PlanWithoutTactic).Sum();

            double stageVelocityHigher = ImprovementMetric.Where(im => im.StageType == SV).Select(im => im.PlanWithTactic).Sum();
            double stageVelocityLower = ImprovementMetric.Where(im => im.StageType == SV).Select(im => im.PlanWithoutTactic).Sum();

            #region "Variables"
            string conversionUpDownString = string.Empty;
            string velocityUpDownString = string.Empty;
            string planNegativePositive = string.Empty;
            string Decreases = "Decreases";
            string Increases = "Increases";
            string Negative = "negatively";
            string Positive = "positively";
            #endregion

            //// Set conversion Up-Down.
            double ConversionValue = conversionRateHigher - conversionRateLower;
            if (ConversionValue < 0)
                conversionUpDownString = Increases;
            else
                conversionUpDownString = Decreases;

            //// Set Velocity Up-Down.
            double VelocityValue = stageVelocityHigher - stageVelocityLower;
            if (VelocityValue <= 0)
            {
                velocityUpDownString = Increases;
                planNegativePositive = Negative;
            }
            else
            {
                velocityUpDownString = Decreases;
                planNegativePositive = Positive;
            }

            #region "Set ViewBag Value"
            ViewBag.ConversionValue = Math.Abs(Math.Round(ConversionValue, 2));
            ViewBag.VelocityValue = Math.Abs(Math.Round(VelocityValue, 2));
            ViewBag.ConversionUpDownString = conversionUpDownString;
            ViewBag.VelocityUpDownString = velocityUpDownString;
            ViewBag.NegativePositiveString = planNegativePositive;
            #endregion

            int NoOfTactic = 0;
            var ListOfLessEffectiveDate = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTac => _imprvTac.ImprovementPlanTacticId != id && _imprvTac.EffectiveDate <= EffectiveDate && _imprvTac.IsDeleted == false && _imprvTac.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId).ToList();
            if (ListOfLessEffectiveDate.Count() == 0)
            {
                var ListOfGreaterEffectiveDate = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTac => _imprvTac.ImprovementPlanTacticId != id && _imprvTac.IsDeleted == false && _imprvTac.EffectiveDate >= EffectiveDate && _imprvTac.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId).Select(_imprvTac => _imprvTac).OrderBy(_imprvTac => _imprvTac.EffectiveDate).ToList();
                if (ListOfGreaterEffectiveDate.Count() == 0)
                {
                    NoOfTactic = db.Plan_Campaign_Program_Tactic.Where(_tac => _tac.StartDate >= EffectiveDate && _tac.IsDeleted == false && _tac.Plan_Campaign_Program.Plan_Campaign.PlanId == Sessions.PlanId).ToList().Count();
                }
                else
                {
                    DateTime NextEffectiveDate = ListOfGreaterEffectiveDate.Min(l => l.EffectiveDate);
                    NoOfTactic = db.Plan_Campaign_Program_Tactic.Where(_tac => _tac.StartDate >= EffectiveDate && _tac.StartDate < NextEffectiveDate && _tac.IsDeleted == false && _tac.Plan_Campaign_Program.Plan_Campaign.PlanId == Sessions.PlanId).ToList().Count();
                }
            }
            ViewBag.NumberOfTactic = NoOfTactic;
            return PartialView("_DeleteImprovementTactic");
        }

        /// <summary>
        /// Get Container value for improvement tactic.
        /// Modified By Maninder Singh Wadhva, Ticket#406.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetImprovementContainerValue()
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            List<Plan_Campaign_Program_Tactic> tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(Sessions.PlanId) &&
                                                                             tactic.IsDeleted == false)
                                                                 .ToList();
            //// Getting list of improvement activites.
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTac => _imprvTac.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && _imprvTac.IsDeleted == false).Select(_imprvTac => _imprvTac).ToList();

            //Added By Bhavesh For Performance Issue #955
            List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
            List<StageList> stageListType = Common.GetStageList();
            int? ModelId = db.Plans.Where(_pln => _pln.PlanId == Sessions.PlanId).Select(_pln => _pln.ModelId).FirstOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(_mdl => _mdl.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(_mdl => _mdl.ModelId == ModelId).Select(_mdl => _mdl).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(_mdl => _mdl.ModelId).ToList());

            var improvementTacticTypeIds = improvementActivities.Select(imptype => imptype.ImprovementTacticTypeId).Distinct().ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.CID && stage.IsDeleted == false).Select(stage => stage).ToList();
            //End #955

            List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelationForSinglePlan(tacticList, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, false);
            List<TacticStageValue> TacticDataWithImprovement = Common.GetTacticStageRelationForSinglePlan(tacticList, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, true);

            //// Calculating MQL difference.
            double? improvedMQL = TacticDataWithImprovement.Sum(_imprvTac => _imprvTac.MQLValue);
            double planMQL = TacticDataWithoutImprovement.Sum(_imprvTac => _imprvTac.MQLValue);
            double differenceMQL = Convert.ToDouble(improvedMQL) - planMQL;

            //// Calculating CW difference.
            double? improvedCW = TacticDataWithImprovement.Sum(_imprvTac => _imprvTac.CWValue);
            double planCW = TacticDataWithoutImprovement.Sum(_imprvTac => _imprvTac.CWValue);
            double differenceCW = Convert.ToDouble(improvedCW) - planCW;

            string stageTypeSV = Enums.StageType.SV.ToString();
            double improvedSV = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSV);
            double sv = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSV, false);
            double differenceSV = Convert.ToDouble(improvedSV) - sv;

            //// Calcualting Deal size.
            string stageTypeSize = Enums.StageType.Size.ToString();
            double improvedDealSize = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSize);
            double averageDealSize = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSize, false);
            double differenceDealSize = improvedDealSize - averageDealSize;

            //// Calculation of Revenue
            double? improvedProjectedRevenue = TacticDataWithImprovement.Sum(t => t.RevenueValue);
            double projectedRevenue = TacticDataWithoutImprovement.Sum(t => t.RevenueValue);
            double differenceProjectedRevenue = Convert.ToDouble(improvedProjectedRevenue) - projectedRevenue;

            double improvedCost = improvementActivities.Sum(improvementActivity => improvementActivity.Cost);
            return Json(new
            {
                MQL = Math.Round(differenceMQL),
                CW = Math.Round(differenceCW, 1),
                ADS = Math.Round(objCurrency.GetValueByExchangeRate(differenceDealSize, PlanExchangeRate)),// Modified By Nishant Sheth // To apply Multi-Currency rates
                Velocity = Math.Round(differenceSV),
                Revenue = Math.Round(objCurrency.GetValueByExchangeRate(differenceProjectedRevenue, PlanExchangeRate), 1),// Modified By Nishant Sheth // To apply Multi-Currency rates
                Cost = Math.Round(objCurrency.GetValueByExchangeRate(improvedCost, PlanExchangeRate))// Modified By Nishant Sheth // To apply Multi-Currency rates
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Suggested Recommended Improvement Type
        /// Added by Bhavesh
        /// Pl Ticket 289,377,378
        /// </summary>
        /// <returns></returns>
        public JsonResult GetRecommendedImprovementTacticType()
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            var improvementTacticList = db.ImprovementTacticTypes.Where(_imprvTacType => _imprvTacType.IsDeployed == true && _imprvTacType.ClientId == Sessions.User.CID && _imprvTacType.IsDeleted.Equals(false)).ToList();       //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
            List<Plan_Campaign_Program_Tactic> marketingActivities = db.Plan_Campaign_Program_Tactic.Where(_tac => _tac.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(Sessions.PlanId) && _tac.IsDeleted == false).Select(_tac => _tac).ToList();
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTac => _imprvTac.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && _imprvTac.IsDeleted == false).Select(_imprvTac => _imprvTac).ToList();
            double projectedRevenueWithoutTactic = 0;
            string stageTypeSize = Enums.StageType.Size.ToString();

            //Added By Bhavesh For Performance Issue #955
            List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
            List<StageList> stageListType = Common.GetStageList();
            int? ModelId = db.Plans.Where(_pln => _pln.PlanId == Sessions.PlanId).Select(_pln => _pln.ModelId).FirstOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(_mdl => _mdl.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(_mdl => _mdl.ModelId == ModelId).Select(_mdl => _mdl).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(_mdl => _mdl.ModelId).ToList());

            var improvementTacticTypeIds = improvementTacticList.Select(imptype => imptype.ImprovementTacticTypeId).ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.CID && stage.IsDeleted == false).Select(stage => stage).ToList();

            List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelationForSinglePlan(marketingActivities, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, false);
            List<TacticStageValue> TacticDataWithImprovement = Common.GetTacticStageRelationForSinglePlan(marketingActivities, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, true);

            //// Checking whether improvement activities exist.
            if (improvementActivities.Count() > 0)
            {
                double improvedDealSize = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSize);
                List<double> revenueList = new List<double>();
                TacticDataWithImprovement.ForEach(_imprvTac => revenueList.Add(_imprvTac.CWValue * improvedDealSize));
                projectedRevenueWithoutTactic = revenueList.Sum();
            }
            else
            {
                projectedRevenueWithoutTactic = TacticDataWithoutImprovement.Sum(_imprvTac => _imprvTac.RevenueValue);
            }

            List<SuggestedImprovementActivities> suggestedImproveentActivities = new List<SuggestedImprovementActivities>();

            SuggestedImprovementActivities suggestedImprovement;
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivitiesWithType, impExits;
            Plan_Improvement_Campaign_Program_Tactic ptcpt;
            List<TacticStageValue> tacticStageValueInnerList;
            double improvedAverageDealSizeForProjectedRevenue = 0, improvedValue = 0, projectedRevenueWithoutTacticTemp = 0, tempValue = 0;
            foreach (var imptactic in improvementTacticList)
            {
                suggestedImprovement = new SuggestedImprovementActivities();
                improvementActivitiesWithType = impExits = new List<Plan_Improvement_Campaign_Program_Tactic>();
                improvementActivitiesWithType = (from ia in improvementActivities select ia).ToList();
                impExits = improvementActivities.Where(ia => ia.ImprovementTacticTypeId == imptactic.ImprovementTacticTypeId).ToList();
                suggestedImprovement.isOwner = false;
                if (impExits.Count() == 0)
                {
                    ptcpt = new Plan_Improvement_Campaign_Program_Tactic();
                    ptcpt.ImprovementTacticTypeId = imptactic.ImprovementTacticTypeId;
                    ptcpt.EffectiveDate = DateTime.Now.Date;
                    ptcpt.ImprovementPlanTacticId = 0;
                    improvementActivitiesWithType.Add(ptcpt);
                    suggestedImprovement.isExits = false;
                    suggestedImprovement.ImprovementPlanTacticId = 0;
                    suggestedImprovement.Cost = imptactic.Cost;
                }
                else
                {
                    improvementActivitiesWithType = improvementActivitiesWithType.Where(sa => sa.ImprovementTacticTypeId != imptactic.ImprovementTacticTypeId).ToList();
                    suggestedImprovement.isExits = true;
                    suggestedImprovement.ImprovementPlanTacticId = improvementActivities.Where(ia => ia.ImprovementTacticTypeId == imptactic.ImprovementTacticTypeId).Select(ia => ia.ImprovementPlanTacticId).FirstOrDefault();
                    suggestedImprovement.Cost = improvementActivities.Where(ia => ia.ImprovementTacticTypeId == imptactic.ImprovementTacticTypeId).Select(ia => ia.Cost).FirstOrDefault();
                    if (Sessions.User.ID == improvementActivities.Where(ia => ia.ImprovementTacticTypeId == imptactic.ImprovementTacticTypeId).Select(ia => ia.CreatedBy).FirstOrDefault())
                    {
                        suggestedImprovement.isOwner = true;
                    }
                }

                tacticStageValueInnerList = Common.GetTacticStageValueListForSuggestedImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementTacticTypeMetric, marketingActivities, improvementActivitiesWithType, stageList);
                improvedAverageDealSizeForProjectedRevenue = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivitiesWithType, improvementTacticTypeMetric, stageTypeSize);
                improvedValue = 0;

                //// Checking whether marketing and improvement activities exist.
                if (marketingActivities.Count() > 0)
                {
                    improvedValue = tacticStageValueInnerList.Sum(_tac => _tac.CWValue * improvedAverageDealSizeForProjectedRevenue);
                }
                projectedRevenueWithoutTacticTemp = projectedRevenueWithoutTactic;
                if (suggestedImprovement.isExits)
                {
                    tempValue = 0;
                    tempValue = projectedRevenueWithoutTacticTemp;
                    projectedRevenueWithoutTacticTemp = improvedValue;
                    improvedValue = tempValue;
                }

                suggestedImprovement.ImprovementTacticTypeId = imptactic.ImprovementTacticTypeId;
                suggestedImprovement.ImprovementTacticTypeTitle = imptactic.Title;

                //Added By Bhavesh :  #515 ignore negative revenue lift
                if (improvedValue < projectedRevenueWithoutTacticTemp)
                {
                    improvedValue = projectedRevenueWithoutTacticTemp;
                }
                //End By Bhavesh : #515
                suggestedImprovement.ProjectedRevenueWithoutTactic = projectedRevenueWithoutTacticTemp;
                suggestedImprovement.ProjectedRevenueWithTactic = improvedValue;
                if (projectedRevenueWithoutTacticTemp != 0)
                {
                    suggestedImprovement.ProjectedRevenueLift = ((improvedValue - projectedRevenueWithoutTacticTemp) / projectedRevenueWithoutTacticTemp) * 100;
                }
                if (imptactic.Cost != 0)
                {
                    suggestedImprovement.RevenueToCostRatio = (improvedValue - projectedRevenueWithoutTacticTemp) / imptactic.Cost;
                }

                suggestedImproveentActivities.Add(suggestedImprovement);
            }

            var datalist = suggestedImproveentActivities.Select(si => new
            {
                ImprovementPlanTacticId = si.ImprovementPlanTacticId,
                ImprovementTacticTypeId = si.ImprovementTacticTypeId,
                ImprovementTacticTypeTitle = si.ImprovementTacticTypeTitle,
                Cost = objCurrency.GetValueByExchangeRate(si.Cost, PlanExchangeRate),// Modified By Nishant Sheth // To Convert in selected currency rate
                ProjectedRevenueWithoutTactic = Math.Round(objCurrency.GetValueByExchangeRate(si.ProjectedRevenueWithoutTactic, PlanExchangeRate), 1),// Modified By Nishant Sheth // To Convert in selected currency rate
                ProjectedRevenueWithTactic = Math.Round(objCurrency.GetValueByExchangeRate(si.ProjectedRevenueWithTactic, PlanExchangeRate), 1),// Modified By Nishant Sheth // To Convert in selected currency rate
                ProjectedRevenueLift = Math.Round(objCurrency.GetValueByExchangeRate(si.ProjectedRevenueLift, PlanExchangeRate), 1),// Modified By Nishant Sheth // To Convert in selected currency rate
                RevenueToCostRatio = Math.Round(si.RevenueToCostRatio, 1),
                IsExits = si.isExits,
                IsOwner = si.isOwner
            }).OrderBy(si => si.ImprovementTacticTypeTitle).ToList();
            return Json(new { data = datalist }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add Improvement Tactic from Suggested Reccomndetion Improvement tactic
        /// Added By Bhavesh
        /// Pl Ticket 289,377,378
        /// </summary>
        /// <param name="improvementPlanProgramId"></param>
        /// <param name="improvementTacticTypeId"></param>
        /// <returns></returns>
        public JsonResult AddSuggestedImprovementTactic(int improvementPlanProgramId, int improvementTacticTypeId)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            //// Check for duplicate exist or not.
            var pcpvar = (from pcpt in db.Plan_Improvement_Campaign_Program_Tactic
                          where pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && pcpt.ImprovementTacticTypeId == improvementTacticTypeId && pcpt.IsDeleted.Equals(false)
                          select pcpt).FirstOrDefault();

            if (pcpvar != null)
            {
                return Json(new { errormsg = Common.objCached.SameImprovementTypeExits });
            }
            else
            {
                //// Add Tactic data to Plan_Improvement_Campaign_Program_Tactic.
                Plan_Improvement_Campaign_Program_Tactic picpt = new Plan_Improvement_Campaign_Program_Tactic();
                // Add By Nishant Sheth
                // Desc :: To Remove Double database trip and Convert cost value based on exchange rate
                var SingleImprovementTactic = db.ImprovementTacticTypes.Where(itactic => itactic.ImprovementTacticTypeId == improvementTacticTypeId && itactic.IsDeleted.Equals(false)).FirstOrDefault();

                picpt.ImprovementPlanProgramId = improvementPlanProgramId;
                picpt.Title = SingleImprovementTactic.Title;
                picpt.ImprovementTacticTypeId = improvementTacticTypeId;
                picpt.Cost = objCurrency.GetValueByExchangeRate(SingleImprovementTactic.Cost, PlanExchangeRate);
                picpt.EffectiveDate = DateTime.Now.Date;
                picpt.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString();
                picpt.CreatedBy = Sessions.User.ID;
                picpt.CreatedDate = DateTime.Now;
                db.Entry(picpt).State = EntityState.Added;
                var Title = picpt.Title;
                int result = db.SaveChanges();
                //// Insert change log entry.
                result = Common.InsertChangeLog(Sessions.PlanId, null, picpt.ImprovementPlanTacticId, picpt.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added, "", picpt.CreatedBy);
                if (result >= 1)
                {
                    TempData["SuccessMessageDeletedPlan"] = Common.objCached.ImprovementTacticStatusSuccessfully.Replace("{0}", Title + " " + "added");
                    // return Json(new { redirect = Url.Action("Assortment") }); 
                    //Modified By Komal rawal for #1432
                    return Json(new { redirect = Url.Action("Index", "Home", new { activeMenu = Enums.ActiveMenu.Plan, currentPlanId = Sessions.PlanId, isGridView = true }) });
                }
                return Json(new { });
            }
        }

        /// <summary>
        /// Get List of Improvement Tactic for ADS Grid
        /// Added by Bhavesh
        /// Pl Ticket 289,377,378
        /// </summary>
        /// <param name="SuggestionIMPTacticIdList"></param>
        /// <returns></returns>
        public JsonResult GetADSImprovementTacticType(string SuggestionIMPTacticIdList)
        {
            List<int> plantacticids = new List<int>();
            if (SuggestionIMPTacticIdList.ToString() != string.Empty)
            {
                plantacticids = SuggestionIMPTacticIdList.Split(',').Select(int.Parse).ToList<int>();
            }
            List<Plan_Improvement_Campaign_Program_Tactic> tblImprovementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTac => _imprvTac.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && _imprvTac.IsDeleted == false).ToList();
            List<Plan_Campaign_Program_Tactic> marketingActivities = db.Plan_Campaign_Program_Tactic.Where(_tac => _tac.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(Sessions.PlanId) && _tac.IsDeleted == false).Select(_tac => _tac).ToList();
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = tblImprovementTactic.OrderBy(_imprvTac => _imprvTac.ImprovementPlanTacticId).Select(_imprvTac => _imprvTac).ToList();
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivitiesWithIncluded = tblImprovementTactic.Where(_imprvTac => !plantacticids.Contains(_imprvTac.ImprovementPlanTacticId)).OrderBy(_imprvTac => _imprvTac.ImprovementPlanTacticId).Select(_imprvTac => _imprvTac).ToList();

            string stageTypeSize = Enums.StageType.Size.ToString();

            //Added By Bhavesh For Performance Issue #955
            List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
            List<StageList> stageListType = Common.GetStageList();
            int? ModelId = db.Plans.Where(_pln => _pln.PlanId == Sessions.PlanId).Select(_pln => _pln.ModelId).FirstOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(_mdl => _mdl.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(_mdl => _mdl.ModelId == ModelId).Select(_mdl => _mdl).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(_mdl => _mdl.ModelId).ToList());

            var improvementTacticTypeIds = improvementActivities.Select(imptype => imptype.ImprovementTacticTypeId).Distinct().ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.CID && stage.IsDeleted == false).Select(stage => stage).ToList();
            //End #955

            List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelationForSinglePlan(marketingActivities, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, false);
            List<TacticStageValue> TacticDataWithImprovement = Common.GetTacticStageRelationForSinglePlan(marketingActivities, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, true);

            double improvedValue = 0;
            double adsWithTactic = 0;
            double? dealSize = null;
            double improvedAverageDealSizeForProjectedRevenue = 0;

            //// Checking whether improvement activities exist.
            if (improvementActivitiesWithIncluded.Count() > 0)
            {
                //// Getting deal size improved based on improvement activities.
                dealSize = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivitiesWithIncluded, improvementTacticTypeMetric, stageTypeSize);
            }
            if (dealSize != null)
            {
                improvedAverageDealSizeForProjectedRevenue = Convert.ToDouble(dealSize);
            }

            adsWithTactic = improvedAverageDealSizeForProjectedRevenue;

            //// Checking whether marketing and improvement activities exist.
            if (marketingActivities.Count() > 0 && improvementActivitiesWithIncluded.Count() > 0)
            {
                //// Getting Projected Reveneue or Closed Won improved based on marketing and improvement activities.
                List<double> revenueList = new List<double>();
                TacticDataWithImprovement.ForEach(t => revenueList.Add(t.CWValue * improvedAverageDealSizeForProjectedRevenue));
                improvedValue = revenueList.Sum();
            }
            else
            {
                improvedValue = TacticDataWithoutImprovement.Sum(_tac => _tac.RevenueValue);
            }

            List<SuggestedImprovementActivities> suggestedImprovementActivities = new List<SuggestedImprovementActivities>();
            SuggestedImprovementActivities suggestedImprovement;
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivitiesWithType;
            bool impExits;
            List<int> excludedids;
            double? dealSizeInner = null;
            double improvedAverageDealSizeForProjectedRevenueInner = 0, projectedRevenueWithoutTactic = 0, projectedRevenueWithoutTacticTemp = 0, adsTempValue = 0, tempValue = 0, tempADSValue = 0;
            foreach (var imptactic in improvementActivities)
            {
                suggestedImprovement = new SuggestedImprovementActivities();
                improvementActivitiesWithType = new List<Plan_Improvement_Campaign_Program_Tactic>();
                improvementActivitiesWithType = (from ia in improvementActivities select ia).ToList();

                impExits = plantacticids.Contains(imptactic.ImprovementPlanTacticId);
                if (!impExits)
                {
                    improvementActivitiesWithType = improvementActivitiesWithType.Where(sa => sa.ImprovementPlanTacticId != imptactic.ImprovementPlanTacticId && !plantacticids.Contains(sa.ImprovementPlanTacticId)).ToList();
                    suggestedImprovement.isExits = true;
                }
                else
                {
                    excludedids = new List<int>();
                    excludedids = (from _pln in plantacticids select _pln).ToList();
                    excludedids.Remove(imptactic.ImprovementPlanTacticId);
                    improvementActivitiesWithType = improvementActivitiesWithType.Where(sa => !excludedids.Contains(sa.ImprovementPlanTacticId)).ToList();
                    suggestedImprovement.isExits = false;
                }

                suggestedImprovement.ImprovementPlanTacticId = imptactic.ImprovementPlanTacticId;

                dealSizeInner = null;
                improvedAverageDealSizeForProjectedRevenueInner = 0;
                //// Checking whether improvement activities exist.
                if (improvementActivitiesWithType.Count() > 0)
                {
                    //// Getting deal size improved based on improvement activities.
                    dealSizeInner = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivitiesWithType, improvementTacticTypeMetric, stageTypeSize);
                }
                else
                {
                    dealSizeInner = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivitiesWithType, improvementTacticTypeMetric, stageTypeSize, false);
                }
                if (dealSizeInner != null)
                {
                    improvedAverageDealSizeForProjectedRevenueInner = Convert.ToDouble(dealSizeInner);
                }

                projectedRevenueWithoutTactic = 0;
                List<TacticStageValue> tacticStageValueInnerList = Common.GetTacticStageValueListForSuggestedImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementTacticTypeMetric, marketingActivities, improvementActivitiesWithType, stageList);

                //// Checking whether marketing and improvement activities exist.
                if (marketingActivities.Count() > 0)
                {
                    //// Getting Projected Reveneue or Closed Won improved based on marketing and improvement activities.
                    projectedRevenueWithoutTactic = tacticStageValueInnerList.Sum(t => t.CWValue * improvedAverageDealSizeForProjectedRevenueInner);
                }

                projectedRevenueWithoutTacticTemp = improvedValue;
                adsTempValue = adsWithTactic;
                if (!suggestedImprovement.isExits)
                {
                    tempValue = 0;
                    tempValue = projectedRevenueWithoutTacticTemp;
                    projectedRevenueWithoutTacticTemp = projectedRevenueWithoutTactic;
                    projectedRevenueWithoutTactic = tempValue;

                    tempADSValue = 0;
                    tempADSValue = improvedAverageDealSizeForProjectedRevenueInner;
                    improvedAverageDealSizeForProjectedRevenueInner = adsTempValue;
                    adsTempValue = tempADSValue;
                }

                suggestedImprovement.isOwner = true;
                suggestedImprovement.ImprovementTacticTypeId = imptactic.ImprovementTacticTypeId;
                suggestedImprovement.ImprovementTacticTypeTitle = imptactic.ImprovementTacticType.Title;
                suggestedImprovement.Cost = imptactic.Cost;
                suggestedImprovement.ProjectedRevenueWithoutTactic = improvedAverageDealSizeForProjectedRevenueInner;// projectedRevenueWithoutTactic;
                suggestedImprovement.ProjectedRevenueWithTactic = adsTempValue;// improvedValue;

                if (projectedRevenueWithoutTactic != 0)
                {
                    suggestedImprovement.ProjectedRevenueLift = ((projectedRevenueWithoutTacticTemp - projectedRevenueWithoutTactic) / projectedRevenueWithoutTactic) * 100;
                }
                if (imptactic.Cost != 0)
                {
                    suggestedImprovement.RevenueToCostRatio = (projectedRevenueWithoutTacticTemp - projectedRevenueWithoutTactic) / imptactic.Cost;
                }

                suggestedImprovementActivities.Add(suggestedImprovement);
            }

            var datalist = suggestedImprovementActivities.Select(si => new
            {
                ImprovementPlanTacticId = si.ImprovementPlanTacticId,
                ImprovementTacticTypeId = si.ImprovementTacticTypeId,
                ImprovementTacticTypeTitle = si.ImprovementTacticTypeTitle,
                Cost = si.Cost,
                ProjectedRevenueWithoutTactic = Math.Round(si.ProjectedRevenueWithoutTactic, 1),
                ProjectedRevenueWithTactic = Math.Round(si.ProjectedRevenueWithTactic, 1),
                ProjectedRevenueLift = Math.Round(si.ProjectedRevenueLift, 1),
                RevenueToCostRatio = Math.Round(si.RevenueToCostRatio, 1),
                IsExits = si.isExits
            }).OrderBy(si => si.ImprovementPlanTacticId).ToList();
            return Json(new { data = datalist }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Header value for Suggested Grid
        /// Added by Bhavesh
        /// Pl Ticket 289,377,378
        /// </summary>
        /// <param name="SuggestionIMPTacticIdList"></param>
        /// <returns></returns>
        public JsonResult GetHeaderValueForSuggestedImprovement(string SuggestionIMPTacticIdList)
        {

            List<int> plantacticids = new List<int>();
            if (SuggestionIMPTacticIdList.ToString() != string.Empty)
            {
                plantacticids = SuggestionIMPTacticIdList.Split(',').Select(int.Parse).ToList<int>();
            }

            //// Calculating CW difference.
            //// Getting list of marketing activites.
            List<Plan_Campaign_Program_Tactic> marketingActivities = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(Sessions.PlanId) &&
                                                                             tactic.IsDeleted == false).ToList();
            //// Get Main Improvement Tactic List
            List<Plan_Improvement_Campaign_Program_Tactic> mainImprovementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(_tac => _tac.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && _tac.IsDeleted == false).Select(_tac => _tac).ToList();

            //// Getting list of improvement activites.
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = mainImprovementActivities.Where(t => !plantacticids.Contains(t.ImprovementPlanTacticId)).Select(t => t).ToList();

            //Added By Bhavesh For Performance Issue #955
            List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
            List<StageList> stageListType = Common.GetStageList();
            int? ModelId = db.Plans.Where(_pln => _pln.PlanId == Sessions.PlanId).Select(_pln => _pln.ModelId).FirstOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(_mdl => _mdl.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(_mdl => _mdl.ModelId == ModelId).Select(_mdl => _mdl).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(_mdl => _mdl.ModelId).ToList());

            var improvementTacticTypeIds = mainImprovementActivities.Select(imptype => imptype.ImprovementTacticTypeId).Distinct().ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.CID && stage.IsDeleted == false).Select(stage => stage).ToList();
            //End #955

            List<TacticStageValue> tacticStageValueInnerList = Common.GetTacticStageValueListForSuggestedImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementTacticTypeMetric, marketingActivities, improvementActivities, stageList);

            double? improvedCW = null;

            //// Checking whether marketing and improvement activities exist.
            if (marketingActivities.Count() > 0)
            {
                //// Getting Projected Reveneue or Closed Won improved based on marketing and improvement activities.
                improvedCW = tacticStageValueInnerList.Sum(_tac => _tac.CWValue);
            }

            List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelationForSinglePlan(marketingActivities, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, mainImprovementActivities, modelDateList, MainModelId, stageList, false);
            double planCW = TacticDataWithoutImprovement.Sum(cw => cw.CWValue);
            double differenceCW = Convert.ToDouble(improvedCW) - planCW;

            string stageTypeSize = Enums.StageType.Size.ToString();
            //// Calcualting Deal size.
            double averageDealSize = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSize, false);
            double differenceDealSize = 0;
            //// Checking whether improvement activities exist.
            if (improvementActivities.Count() > 0)
            {
                double improvedDealSize = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSize, true);
                differenceDealSize = improvedDealSize - averageDealSize;
            }

            // Calculate Velocity
            double? improvedSV = null;
            double differenceSV = 0;
            string stageTypeSV = Enums.StageType.SV.ToString();
            //// Checking whether improvement activities exist.
            if (improvementActivities.Count() > 0)
            {
                //// Getting velocity improved based on improvement activities.
                improvedSV = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSV, true);
                double sv = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSV, false);
                differenceSV = Convert.ToDouble(improvedSV) - sv;
            }

            return Json(new
            {
                CW = Math.Round(differenceCW, 1),
                ADS = Math.Round(differenceDealSize),
                Velocity = Math.Round(differenceSV)
            }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Get List of Improvement Tactic For Conversion & Velocity Grid
        /// Added by Bhavesh
        /// Pl Ticket 289,377,378
        /// </summary>
        /// <param name="SuggestionIMPTacticIdList"></param>
        /// <param name="isConversion"></param>
        /// <returns></returns>
        public JsonResult GetConversionImprovementTacticType(string SuggestionIMPTacticIdList, bool isConversion = true)
        {
            List<int> plantacticids = new List<int>();
            if (SuggestionIMPTacticIdList.ToString() != string.Empty)
            {
                plantacticids = SuggestionIMPTacticIdList.Split(',').Select(int.Parse).ToList<int>();
            }

            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTac => _imprvTac.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && _imprvTac.IsDeleted == false).OrderBy(_imprvTac => _imprvTac.ImprovementPlanTacticId).Select(_imprvTac => _imprvTac).ToList();
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivitiesWithIncluded = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTac => _imprvTac.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && _imprvTac.IsDeleted == false && !plantacticids.Contains(_imprvTac.ImprovementPlanTacticId)).OrderBy(_imprvTac => _imprvTac.ImprovementPlanTacticId).Select(_imprvTac => _imprvTac).ToList();
            string stageCR = Enums.MetricType.CR.ToString();
            string StageType = stageCR;
            if (!isConversion)
            {
                StageType = Enums.MetricType.SV.ToString();
            }

            string CW = Enums.Stage.CW.ToString();
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.CID && stage.IsDeleted == false && stage.Level != null && stage.Code != CW).ToList();
            List<int> impTacticTypeIds = improvementActivities.Select(ia => ia.ImprovementTacticTypeId).Distinct().ToList();
            List<ImprovementTacticType_Metric> improvedMetrcList = db.ImprovementTacticType_Metric.Where(im => impTacticTypeIds.Contains(im.ImprovementTacticTypeId)).ToList();

            //// suggest Improvement Tactics.
            #region " Declare Local Variables "
            List<SuggestedImprovementActivitiesConversion> suggestedImprovementActivitiesConversion = new List<SuggestedImprovementActivitiesConversion>();
            SuggestedImprovementActivitiesConversion sIconversion;
            List<ImprovedMetricWeight> imList;
            ImprovedMetricWeight _imprvdMetric;
            List<ImprovementTacticType_Metric> weightValue;
            bool impExits;
            #endregion
            foreach (var imptactic in improvementActivities)
            {
                sIconversion = new SuggestedImprovementActivitiesConversion();
                sIconversion.ImprovementPlanTacticId = imptactic.ImprovementPlanTacticId;
                sIconversion.ImprovementTacticTypeId = imptactic.ImprovementTacticTypeId;
                sIconversion.ImprovementTacticTypeTitle = imptactic.ImprovementTacticType.Title;
                sIconversion.Cost = imptactic.Cost;
                imList = new List<ImprovedMetricWeight>();
                foreach (var m in stageList)
                {
                    _imprvdMetric = new ImprovedMetricWeight();
                    _imprvdMetric.MetricId = m.StageId;
                    _imprvdMetric.Level = m.Level;
                    weightValue = new List<ImprovementTacticType_Metric>();
                    weightValue = improvedMetrcList.Where(iml => iml.ImprovementTacticTypeId == imptactic.ImprovementTacticTypeId
                       && iml.StageId == m.StageId
                       && iml.StageType == StageType
                       ).ToList();
                    if (weightValue.Count > 0)
                    {
                        _imprvdMetric.Value = weightValue.Sum(imp => imp.Weight);
                    }
                    else
                    {
                        _imprvdMetric.Value = 0;
                    }
                    imList.Add(_imprvdMetric);
                }
                sIconversion.ImprovedMetricsWeight = imList;
                impExits = false;
                impExits = plantacticids.Contains(imptactic.ImprovementPlanTacticId);
                if (!impExits)
                {
                    sIconversion.isExits = true;
                }
                else
                {
                    sIconversion.isExits = false;
                }

                suggestedImprovementActivitiesConversion.Add(sIconversion);
            }

            //Added By Bhavesh For Performance Issue #955
            List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
            List<StageList> stageListType = Common.GetStageList();
            int? ModelId = db.Plans.Where(_pln => _pln.PlanId == Sessions.PlanId).Select(_pln => _pln.ModelId).FirstOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(_mdl => _mdl.IsDeleted == false);
            int MainModelId = (int)ModelId;
            Model _model;
            while (ModelId != null)
            {
                _model = new Model();
                _model = ModelList.Where(_mdl => _mdl.ModelId == ModelId).Select(_mdl => _mdl).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = _model.ModelId, ParentModelId = _model.ParentModelId, EffectiveDate = _model.EffectiveDate });
                ModelId = _model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(_mdl => _mdl.ModelId).ToList());

            var improvementTacticTypeIds = improvementActivities.Select(imptype => imptype.ImprovementTacticTypeId).Distinct().ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            //End #955

            List<StageRelation> stageRelationList = Common.CalculateStageValueForSuggestedImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivitiesWithIncluded, improvementTacticTypeMetric, true);
            List<int> finalMetricList = stageList.Select(stage => stage.StageId).ToList();

            var datalist = suggestedImprovementActivitiesConversion.Select(si => new
            {
                ImprovementPlanTacticId = si.ImprovementPlanTacticId,
                ImprovementTacticTypeId = si.ImprovementTacticTypeId,
                ImprovementTacticTypeTitle = si.ImprovementTacticTypeTitle,
                Cost = si.Cost,
                IsExits = si.isExits,
                MetricList = si.ImprovedMetricsWeight
            }).Select(si => si).OrderBy(si => si.ImprovementPlanTacticId);


            var dataMetricList = stageList.Select(ml => new
            {
                MetricId = ml.StageId,
                Title = StageType == stageCR ? ml.ConversionTitle : ml.Title,
                Level = ml.Level
            }).OrderBy(ml => ml.Level);

            var dataFinalMetricList = stageRelationList.Where(him => finalMetricList.Contains(him.StageId) && him.StageType == StageType).Select(him => new
            {
                MetricId = him.StageId,
                Value = StageType == stageCR ? him.Value * 100 : him.Value,
                Level = stageList.FirstOrDefault(stage => stage.StageId == him.StageId).Level
            }).OrderBy(him => him.Level);

            return Json(new { data = datalist, datametriclist = dataMetricList, datafinalmetriclist = dataFinalMetricList }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Delete Improvement Tactic on click of Save & Continue
        /// Added by Bhavesh
        /// Pl Ticket 289,377,378
        /// </summary>
        /// <param name="SuggestionIMPTacticIdList"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public JsonResult DeleteSuggestedBoxImprovementTactic(string SuggestionIMPTacticIdList, int UserId = 0)
        {
            //// Check whether UserId is loggined User or Not.
            if (UserId != 0)
            {
                if (Sessions.User.ID != UserId)
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            List<int> plantacticids = new List<int>();
            if (SuggestionIMPTacticIdList.ToString() != string.Empty)
            {
                plantacticids = SuggestionIMPTacticIdList.Split(',').Select(int.Parse).ToList<int>();
            }
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        foreach (int pid in plantacticids)
                        {
                            int returnValue = 0;
                            //// Update Improvement Tactic data to Table Plan_Improvement_Campaign_Program_Tactic.
                            Plan_Improvement_Campaign_Program_Tactic pcpt = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTac => _imprvTac.ImprovementPlanTacticId == pid).FirstOrDefault();
                            if (pcpt.CreatedBy == Sessions.User.ID)
                            {
                                pcpt.IsDeleted = true;
                                db.Entry(pcpt).State = EntityState.Modified;
                                returnValue = db.SaveChanges();
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcpt.ImprovementPlanTacticId, pcpt.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed, "", pcpt.CreatedBy);
                            }
                        }
                        scope.Complete();
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { data = true });
        }

        #endregion

        #region Budget Allocation

        #region Save Budget Allocation

        /// <summary>
        /// Save budget allocation data for plan
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>22/07/2014</CreatedDate>
        /// <param name="planId"></param>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public JsonResult SaveBudgetAllocation(int planId, string inputs)
        {
            int retVal = 0;
            bool isDBSaveChanges = false;
            string[] inputValues = inputs.Split(',');
            try
            {
                if (planId != 0)
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        if (inputs != "")
                        {
                            var PrevPlanBudgetAllocationList = db.Plan_Budget.Where(pb => pb.PlanId == planId).Select(pb => pb).ToList();

                            //-- Insert new budget allocation for plan
                            if (inputValues.Length == 12)
                            {
                                //-- Monthly Budget Allocation
                                bool isExists;
                                Plan_Budget updatePlanBudget, objPlan_Budget;
                                double newValue;
                                for (int i = 0; i < 12; i++)
                                {
                                    isExists = false;
                                    if (PrevPlanBudgetAllocationList != null && PrevPlanBudgetAllocationList.Count > 0)
                                    {
                                        updatePlanBudget = new Plan_Budget();
                                        updatePlanBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (i + 1))).FirstOrDefault();
                                        if (updatePlanBudget != null)
                                        {
                                            // Added by Dharmraj to handle case were user update empty or zero allocation value.
                                            if (inputValues[i] == "")
                                            {
                                                db.Entry(updatePlanBudget).State = EntityState.Deleted;
                                                isDBSaveChanges = true;
                                            }
                                            else
                                            {
                                                newValue = Convert.ToInt64(inputValues[i]);
                                                if (updatePlanBudget.Value != newValue)
                                                {
                                                    updatePlanBudget.Value = newValue;
                                                    db.Entry(updatePlanBudget).State = EntityState.Modified;
                                                    isDBSaveChanges = true;
                                                }
                                            }
                                            isExists = true;
                                        }
                                    }
                                    //// If record not exist then Inser new Record.
                                    if (!isExists && inputValues[i] != "")  // Modified by Sohel Pathan on 02/09/2014 for Internal Review Point
                                    {
                                        objPlan_Budget = new Plan_Budget();
                                        objPlan_Budget.PlanId = planId;
                                        objPlan_Budget.Period = PeriodChar + (i + 1).ToString();
                                        objPlan_Budget.Value = Convert.ToInt64(inputValues[i]);
                                        objPlan_Budget.CreatedDate = DateTime.Now;
                                        objPlan_Budget.CreatedBy = Sessions.User.ID;
                                        db.Plan_Budget.Add(objPlan_Budget);
                                        isDBSaveChanges = true;
                                    }
                                }
                            }
                            else if (inputValues.Length == 4)
                            {
                                //-- Quarterly Budget Allocation
                                int QuarterCnt = 1, j = 1;
                                bool isExists;
                                List<Plan_Budget> thisQuartersMonthList;
                                Plan_Budget thisQuarterFirstMonthBudget, objPlan_Budget;
                                double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;
                                for (int i = 0; i < inputValues.Length; i++)
                                {
                                    isExists = false;
                                    if (PrevPlanBudgetAllocationList != null)
                                    {
                                        if (PrevPlanBudgetAllocationList.Count > 0)
                                        {
                                            thisQuartersMonthList = new List<Plan_Budget>();
                                            thisQuartersMonthList = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                            thisQuarterFirstMonthBudget = new Plan_Budget();
                                            thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                            if (thisQuarterFirstMonthBudget != null)
                                            {
                                                if (inputValues[i] != "")
                                                {
                                                    thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                                    thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                                    newValue = Convert.ToDouble(inputValues[i]);

                                                    if (thisQuarterTotalBudget != newValue)
                                                    {
                                                        BudgetDiff = newValue - thisQuarterTotalBudget;
                                                        if (BudgetDiff > 0)
                                                        {
                                                            thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                            isDBSaveChanges = true;
                                                        }
                                                        else
                                                        {
                                                            j = 1;
                                                            while (BudgetDiff < 0)
                                                            {
                                                                if (thisQuarterFirstMonthBudget != null)
                                                                {
                                                                    BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                                    if (BudgetDiff <= 0)
                                                                        thisQuarterFirstMonthBudget.Value = 0;
                                                                    else
                                                                        thisQuarterFirstMonthBudget.Value = BudgetDiff;

                                                                    db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                                    isDBSaveChanges = true;
                                                                }
                                                                if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                                {
                                                                    thisQuarterFirstMonthBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                                }

                                                                j = j + 1;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                                    isDBSaveChanges = true;
                                                }
                                                isExists = true;
                                            }
                                        }
                                    }
                                    //// If record not exist then Inser new Record.
                                    if (!isExists && inputValues[i] != "")  // Modified by Sohel Pathan on 04/09/2014 for PL ticket #642
                                    {
                                        objPlan_Budget = new Plan_Budget();
                                        objPlan_Budget.PlanId = planId;
                                        objPlan_Budget.Period = PeriodChar + (QuarterCnt).ToString();
                                        objPlan_Budget.Value = Convert.ToInt64(inputValues[i]);
                                        objPlan_Budget.CreatedDate = DateTime.Now;
                                        objPlan_Budget.CreatedBy = Sessions.User.ID;
                                        db.Plan_Budget.Add(objPlan_Budget);
                                        isDBSaveChanges = true;
                                    }
                                    QuarterCnt = QuarterCnt + 3;
                                }
                            }
                        }

                        if (isDBSaveChanges)
                        {
                            retVal = db.SaveChanges();
                        }
                        else
                        {
                            retVal = 1;
                        }
                        scope.Complete();
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { id = retVal, redirect = Url.Action("Assortment"), ismsg = "Plan Saved Successfully." }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Get Plan Budget Allocation

        /// <summary>
        /// Get plan budget allocation by planId
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>22/07/2014</CreatedDate>
        /// <param name="planId"></param>
        /// <param name="allocatedBy"></param>
        /// <returns></returns>
        public JsonResult GetAllocatedBudgetForPlan(int planId, string allocatedBy)
        {
            try
            {
                if (planId != 0)
                {
                    List<string> MonthlyList = Common.lstMonthly;
                    var planBudgetAllocationList = db.Plan_Budget.Where(pb => pb.PlanId == planId).OrderBy(a => a.PlanBudgetId).Select(pb => new
                    {
                        pb.Period,
                        pb.Value
                    }).ToList();
                    var planCampaignBudgetAllocationList = db.Plan_Campaign_Budget.Where(pcb => pcb.Plan_Campaign.PlanId == planId && pcb.Plan_Campaign.IsDeleted == false).OrderBy(pcb => pcb.PlanCampaignBudgetId).Select(pb => new
                    {
                        pb.Period,
                        pb.Value
                    }).ToList();

                    if (allocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
                    {
                        List<PlanBudgetAllocationValue> lstPlanBudgetAllocationValue = new List<PlanBudgetAllocationValue>();
                        List<PlanBudgetAllocationValue> lstPlanCampaignBudgetAllocationValue = new List<PlanBudgetAllocationValue>();

                        //// Set Quarterly Plan budget values.
                        string[] quarterPeriods = Common.quarterPeriods;
                        PlanBudgetAllocationValue objPlanBudgetAllocationValue;
                        for (int i = 0; i < 12; i++)
                        {
                            if ((i + 1) % 3 == 0)
                            {
                                objPlanBudgetAllocationValue = new PlanBudgetAllocationValue();
                                objPlanBudgetAllocationValue.periodTitle = PeriodChar + (i - 1).ToString();
                                objPlanBudgetAllocationValue.budgetValue = planBudgetAllocationList.Where(a => quarterPeriods.Contains(a.Period)).FirstOrDefault() == null ? "" : planBudgetAllocationList.Where(a => quarterPeriods.Contains(a.Period)).Select(a => a.Value).Sum().ToString();
                                objPlanBudgetAllocationValue.campaignMonthlyBudget = planCampaignBudgetAllocationList.Where(pcb => quarterPeriods.Contains(pcb.Period)).Sum(pcb => pcb.Value);

                                /// Add into return list
                                lstPlanBudgetAllocationValue.Add(objPlanBudgetAllocationValue);

                                quarterPeriods = new string[] { PeriodChar + (i + 2), PeriodChar + (i + 3), PeriodChar + (i + 4) };
                            }
                        }

                        return Json(new { status = 1, planBudgetAllocationList = lstPlanBudgetAllocationValue }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var returnPlanBudgetList = MonthlyList.Select(period => new
                        {
                            periodTitle = period,
                            budgetValue = planBudgetAllocationList.Where(pb => pb.Period == period).FirstOrDefault() == null ? "" : planBudgetAllocationList.Where(pb => pb.Period == period).Select(pb => pb.Value).FirstOrDefault().ToString(),
                            campaignMonthlyBudget = planCampaignBudgetAllocationList.Where(pcb => pcb.Period == period).Sum(pcb => pcb.Value)
                        }).ToList();

                        return Json(new { status = 1, planBudgetAllocationList = returnPlanBudgetList }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { status = 0 }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Get Budget Allocation Data For Plan For Slide Panel

        /// <summary>
        /// Action to Get monthly or quarterly budget value of Plan.
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>31/07/2014</CreatedDate>
        /// <param name="id">Plan Id.</param>
        /// <returns>Returns Json Result of plan budget allocation Value.</returns>
        public JsonResult GetBudgetAllocationPlanData(int id)
        {
            try
            {
                List<string> lstMonthly = Common.lstMonthly;
                var objPlan = db.Plans.FirstOrDefault(p => p.PlanId == id && p.IsDeleted == false);    // Modified by Sohel Pathan on 04/09/2014 for PL ticket #642

                var lstAllCampaign = db.Plan_Campaign.Where(_camp => _camp.PlanId == id && _camp.IsDeleted == false).ToList();

                // Add By Nishant Sheth 
                // Desc:: for add multiple years regarding #1765
                // To create the period of the year dynamically base on item period
                var maxdateCampaign = lstAllCampaign.OrderByDescending(a => a.EndDate).FirstOrDefault();
                int GlobalYearDiffrence = 0;

                listMonthDynamic MonthlyDynamicCampaign = new listMonthDynamic();
                List<listMonthDynamic> lstMonthlyDynamicCampaign = new List<listMonthDynamic>();
                if (maxdateCampaign != null)
                {
                    List<string> lstMonthlyExtended = new List<string>();
                    int YearDiffrence = Convert.ToInt32(Convert.ToInt32(maxdateCampaign.EndDate.Year) - Convert.ToInt32(maxdateCampaign.StartDate.Year));
                    GlobalYearDiffrence = YearDiffrence;
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
                    MonthlyDynamicCampaign.listMonthly = lstMonthlyExtended;
                    MonthlyDynamicCampaign.Id = maxdateCampaign.PlanCampaignId;
                }

                if (lstAllCampaign != null && lstAllCampaign.Count > 0)
                {
                    lstAllCampaign.ForEach(camp =>
                    {
                        List<string> lstMonthlyExtended = new List<string>();
                        int YearDiffrence = Convert.ToInt32(Convert.ToInt32(camp.EndDate.Year) - Convert.ToInt32(camp.StartDate.Year));

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

                        lstMonthlyDynamicCampaign.Add(new listMonthDynamic { Id = camp.PlanCampaignId, listMonthly = lstMonthlyExtended });
                    });
                }

                // Change by Nishant sheth
                // Desc :: #1765 - add period condition to get value
                var lstPlanBudget = db.Plan_Budget.Where(pb => pb.PlanId == id)
                                                               .Select(pb => new
                                                               {
                                                                   pb.PlanBudgetId,
                                                                   pb.PlanId,
                                                                   pb.Period,
                                                                   pb.Value
                                                               }).ToList()
                                                               .Where(a => (MonthlyDynamicCampaign.listMonthly != null ? MonthlyDynamicCampaign.listMonthly.Contains(a.Period) : lstMonthly.Contains(a.Period))).ToList();

                var planCampaignId = lstAllCampaign.Select(_camp => _camp.PlanCampaignId);
                // Change by Nishant sheth
                // Desc :: #1765 - add period condition to get value
                var lstCampaignBudget = db.Plan_Campaign_Budget.Where(_budgt => planCampaignId.Contains(_budgt.PlanCampaignId)).ToList()
                                                               .Select(_budgt => new
                                                               {
                                                                   _budgt.PlanCampaignBudgetId,
                                                                   _budgt.PlanCampaignId,
                                                                   _budgt.Period,
                                                                   _budgt.Value
                                                               }).ToList()
                        .Where(_budget => (lstMonthlyDynamicCampaign.FirstOrDefault().listMonthly != null ?
                            lstMonthlyDynamicCampaign.Where(a => a.Id == _budget.PlanCampaignId).Select(a => a.listMonthly).FirstOrDefault().Contains(_budget.Period) :
                            lstMonthly.Contains(_budget.Period)))
                            .ToList();

                double allCampaignBudget = lstAllCampaign.Sum(_cmpagn => _cmpagn.CampaignBudget);
                double planBudget = objPlan.Budget;
                double planRemainingBudget = planBudget - allCampaignBudget;

                // Start - Added by Sohel Pathan on 27/08/2014 for PL ticket #642
                if (objPlan.AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString())
                {
                    //// Set Quarterly Budget Allcation value.
                    List<PlanBudgetAllocationValue> lstPlanBudgetAllocationValue = new List<PlanBudgetAllocationValue>();
                    string[] quarterPeriods = Common.quarterPeriods;
                    for (int i = 0; i < (12 * (GlobalYearDiffrence + 1)); i++)
                    {
                        if ((i + 1) % 3 == 0)
                        {
                            PlanBudgetAllocationValue objPlanBudgetAllocationValue = new PlanBudgetAllocationValue();
                            objPlanBudgetAllocationValue.periodTitle = PeriodChar + (i - 1).ToString();
                            objPlanBudgetAllocationValue.budgetValue = lstPlanBudget.Where(a => quarterPeriods.Contains(a.Period) && a.PlanId == id).FirstOrDefault() == null ? "" : lstPlanBudget.Where(a => quarterPeriods.Contains(a.Period) && a.PlanId == id).Select(a => a.Value).Sum().ToString();
                            objPlanBudgetAllocationValue.campaignMonthlyBudget = lstCampaignBudget.Where(a => quarterPeriods.Contains(a.Period)).Select(a => a.Value).Sum();

                            /// Add into return list
                            lstPlanBudgetAllocationValue.Add(objPlanBudgetAllocationValue);

                            quarterPeriods = new string[] { PeriodChar + (i + 2), PeriodChar + (i + 3), PeriodChar + (i + 4) };
                        }
                    }

                    var objBudgetAllocationData = new { budgetData = lstPlanBudgetAllocationValue, planRemainingBudget = planRemainingBudget };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                // End - Added by Sohel Pathan on 27/08/2014 for PL ticket #642
                else
                {
                    // Change by Nishant sheth
                    // Desc :: #1765 - add period condition to get value
                    var budgetData = MonthlyDynamicCampaign.listMonthly != null ? (MonthlyDynamicCampaign.listMonthly.Select(period => new
                    {
                        periodTitle = period,
                        budgetValue = lstPlanBudget.FirstOrDefault(pb => pb.Period == period && pb.PlanId == id) == null ? "" : lstPlanBudget.FirstOrDefault(pb => pb.Period == period && pb.PlanId == id).Value.ToString(),
                        campaignMonthlyBudget = lstCampaignBudget.Where(c => c.Period == period).Sum(c => c.Value)
                    })) : (lstMonthly.Select(period => new
                    {
                        periodTitle = period,
                        budgetValue = lstPlanBudget.FirstOrDefault(pb => pb.Period == period && pb.PlanId == id) == null ? "" : lstPlanBudget.FirstOrDefault(pb => pb.Period == period && pb.PlanId == id).Value.ToString(),
                        campaignMonthlyBudget = lstCampaignBudget.Where(c => c.Period == period).Sum(c => c.Value)
                    }));

                    var objBudgetAllocationData = new { budgetData = budgetData, planRemainingBudget = planRemainingBudget };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        #endregion
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
                    if (monthlyBudget >= 0 && yearlyBudget >= 0)
                    {
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
                                        if (monthlyBudget >= 0)
                                        {
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
                                                    while (BudgetDiff < 0)
                                                    {
                                                        if (thisQuarterFirstMonthBudget != null)
                                                        {
                                                            BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                            if (BudgetDiff <= 0)
                                                                thisQuarterFirstMonthBudget.Value = 0;
                                                            else
                                                                thisQuarterFirstMonthBudget.Value = BudgetDiff;

                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                        }
                                                        if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                        {
                                                            thisQuarterFirstMonthBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                        }

                                                        j = j + 1;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                        }
                                        isExists = true;
                                    }
                                }
                                //// if previous values does not exist then insert new values.
                                if (!isExists && monthlyBudget >= 0)
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
                                        if (monthlyBudget >= 0)
                                        {
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
                                                    while (BudgetDiff < 0)
                                                    {
                                                        if (thisQuarterFirstMonthBudget != null)
                                                        {
                                                            BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                            if (BudgetDiff <= 0)
                                                                thisQuarterFirstMonthBudget.Value = 0;
                                                            else
                                                                thisQuarterFirstMonthBudget.Value = BudgetDiff;

                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                        }
                                                        if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                        {
                                                            thisQuarterFirstMonthBudget = PrevCampaignBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                        }

                                                        j = j + 1;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                        }
                                        isExists = true;
                                    }
                                }
                                //// if previous values does not exist then insert new values.
                                if (!isExists && monthlyBudget >= 0)
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
                                        if (monthlyBudget >= 0)
                                        {
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
                                                    while (BudgetDiff < 0)
                                                    {
                                                        if (thisQuarterFirstMonthBudget != null)
                                                        {
                                                            BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                            if (BudgetDiff <= 0)
                                                                thisQuarterFirstMonthBudget.Value = 0;
                                                            else
                                                                thisQuarterFirstMonthBudget.Value = BudgetDiff;

                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                        }
                                                        if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                        {
                                                            thisQuarterFirstMonthBudget = PrevProgramBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                        }

                                                        j = j + 1;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                        }
                                        isExists = true;
                                    }
                                }
                                //// if previous values does not exist then insert new values.
                                if (!isExists && monthlyBudget >= 0)
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
                                        if (monthlyBudget >= 0)
                                        {
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
                                                    while (BudgetDiff < 0)
                                                    {
                                                        if (thisQuarterFirstMonthBudget != null)
                                                        {
                                                            BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                            if (BudgetDiff <= 0)
                                                                thisQuarterFirstMonthBudget.Value = 0;
                                                            else
                                                                thisQuarterFirstMonthBudget.Value = BudgetDiff;

                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                        }
                                                        if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                        {
                                                            thisQuarterFirstMonthBudget = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                        }

                                                        j = j + 1;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                        }
                                        isExists = true;
                                    }
                                }
                                //// if previous values does not exist then insert new values.
                                if (!isExists && monthlyBudget >= 0)
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
                    }
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
                        if (tab == Enums.BudgetTab.Planned.ToString())
                        {
                            if (!isquarter)
                            {
                                var objTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == EntityId && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
                                List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == EntityId).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                                var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();
                                if (lineitemcostlist.Where(pcptc => pcptc.Period == period).Any())
                                {
                                    var costlineitemperiod = lineitemcostlist.Where(pcptlc => pcptlc.Period == period).Sum(pcptlc => pcptlc.Value);
                                    if (monthlycost < costlineitemperiod)
                                    {
                                        // Added by Viral Kadiya for Pl ticket #1970.
                                        string strReduceTacticPlannedCostMessage = string.Format(Common.objCached.TacticPlanedCostReduce, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                                        return Json(new { isSuccess = false, errormsg = strReduceTacticPlannedCostMessage });
                                        //monthlycost = costlineitemperiod;
                                    }
                                }
                                double tacticost = 0;
                                tacticost = objTactic.Plan_Campaign_Program_Tactic_Cost.Select(tactic => tactic.Value).Sum();
                                if (objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == period).Any())
                                {
                                    var objtacticcost = objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == period).FirstOrDefault().Value;
                                    objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == period).FirstOrDefault().Value = monthlycost;
                                    tacticost += monthlycost - objtacticcost;
                                }
                                else
                                {
                                    Plan_Campaign_Program_Tactic_Cost objTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                    objTacticCost.PlanTacticId = EntityId;
                                    objTacticCost.Period = period;
                                    objTacticCost.Value = monthlycost;
                                    objTacticCost.CreatedBy = Sessions.User.ID;
                                    objTacticCost.CreatedDate = DateTime.Now;
                                    db.Entry(objTacticCost).State = EntityState.Added;
                                    tacticost += monthlycost;
                                }

                                #region LinkedTactic updation

                                int linkedTacticId = objTactic.LinkedTacticId.GetValueOrDefault();
                                bool isLinkedQuarter = false;
                                Plan_Campaign_Program_Tactic LinkedTactic = new Plan_Campaign_Program_Tactic();
                                if (linkedTacticId > 0)
                                {
                                    LinkedTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == linkedTacticId && pcpt.IsDeleted == false).FirstOrDefault();
                                    isLinkedQuarter = LinkedTactic.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString() ? true : false;

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
                                            if (LinkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == linkedPeriod).Any())
                                            {
                                                LinkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == linkedPeriod).FirstOrDefault().Value = monthlycost;
                                            }
                                            else
                                            {
                                                Plan_Campaign_Program_Tactic_Cost objLinkedTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                                objLinkedTacticCost.PlanTacticId = linkedTacticId;
                                                objLinkedTacticCost.Period = linkedPeriod;
                                                objLinkedTacticCost.Value = monthlycost;
                                                objLinkedTacticCost.CreatedBy = Sessions.User.ID;
                                                objLinkedTacticCost.CreatedDate = DateTime.Now;
                                                db.Entry(objLinkedTacticCost).State = EntityState.Added;
                                            }
                                        }
                                    }
                                }


                                #endregion

                                double totalLineitemCost = 0;
                                if (objtotalLineitemCost != null && objtotalLineitemCost.Count() > 0)
                                {
                                    totalLineitemCost = objtotalLineitemCost.Sum(l => l.Cost);
                                }
                                if (totalLineitemCost > yearlycost)
                                {
                                    // Added by Viral Kadiya for Pl ticket #1970.
                                    string strReduceTacticPlannedCostMessage = string.Format(Common.objCached.TacticPlanedCostReduce, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                                    return Json(new { isSuccess = false, errormsg = strReduceTacticPlannedCostMessage });
                                    // yearlycost = totalLineitemCost;
                                }

                                if (tacticost > yearlycost)
                                {
                                    if (objTactic.Cost != yearlycost)
                                    {
                                        double diffcost = tacticost - yearlycost;
                                        int endmonth = 12;
                                        while (diffcost > 0 && endmonth != 0)
                                        {
                                            if (objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                                            {
                                                //Modified by komal Rawal
                                                double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + endmonth).Sum(lineitem => lineitem.Value);
                                                double objtacticcost = objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                                                var DiffMonthCost = objtacticcost - tacticlineitemcostmonth;
                                                if (DiffMonthCost > 0)
                                                {
                                                    if (DiffMonthCost > diffcost)
                                                    {
                                                        objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - diffcost;
                                                        diffcost = 0;
                                                    }
                                                    else
                                                    {
                                                        objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - DiffMonthCost;
                                                        diffcost = diffcost - DiffMonthCost;
                                                    }
                                                }
                                                //END
                                            }
                                            if (endmonth > 0)
                                            {
                                                endmonth -= 1;
                                            }
                                            // if not reduce value than what happen
                                        }
                                    }
                                    else
                                    {
                                        yearlycost = tacticost;
                                    }
                                }
                                else if (tacticost < yearlycost)
                                {
                                    if (objTactic.Cost != yearlycost)
                                    {
                                        double diffcost = yearlycost - tacticost;
                                        int startmonth = objTactic.StartDate.Month;
                                        if (objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                                        {
                                            objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value += diffcost;
                                        }
                                        else
                                        {
                                            Plan_Campaign_Program_Tactic_Cost objTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                            objTacticCost.PlanTacticId = EntityId;
                                            objTacticCost.Period = PeriodChar + startmonth;
                                            objTacticCost.Value = diffcost;
                                            objTacticCost.CreatedBy = Sessions.User.ID;
                                            objTacticCost.CreatedDate = DateTime.Now;
                                            db.Entry(objTacticCost).State = EntityState.Added;
                                        }
                                    }
                                    else
                                    {
                                        yearlycost = tacticost;
                                    }
                                }
                                objTactic.Cost = yearlycost;
                                objTactic.ModifiedBy = Sessions.User.ID;
                                objTactic.ModifiedDate = DateTime.Now;
                                db.Entry(objTactic).State = EntityState.Modified;
                                //// Calculate Total LineItem Cost.
                                Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                                if (objOtherLineItem == null)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = EntityId;
                                    // Modified by Arpita Soni for Ticket #2634 on 09/22/2016
                                    objNewLineitem.Title = Common.LineItemTitleDefault;
                                    if (yearlycost > totalLineitemCost)
                                    {
                                        objNewLineitem.Cost = yearlycost - totalLineitemCost;
                                    }
                                    else
                                    {
                                        objNewLineitem.Cost = 0;
                                    }
                                    objNewLineitem.Description = string.Empty;
                                    objNewLineitem.CreatedBy = Sessions.User.ID;
                                    objNewLineitem.CreatedDate = DateTime.Now;
                                    db.Entry(objNewLineitem).State = EntityState.Added;
                                }
                                else
                                {
                                    objOtherLineItem.IsDeleted = false;
                                    if (yearlycost > totalLineitemCost)
                                    {
                                        objOtherLineItem.Cost = yearlycost - totalLineitemCost;
                                    }
                                    else
                                    {
                                        objOtherLineItem.Cost = 0;
                                        objOtherLineItem.IsDeleted = true;
                                    }
                                    db.Entry(objOtherLineItem).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                var objTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == EntityId && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
                                //// Get Previous budget allocation data by PlanId.
                                var PrevTacticBudgetAllocationList = objTactic.Plan_Campaign_Program_Tactic_Cost.Select(pb => pb).ToList();
                                int QuarterCnt = 1, j = 1;
                                bool isExists;
                                List<Plan_Campaign_Program_Tactic_Cost> thisQuartersMonthList;
                                Plan_Campaign_Program_Tactic_Cost thisQuarterFirstMonthBudget, objTacticBudget;
                                double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;

                                QuarterCnt = Convert.ToInt32(period.Replace(PeriodChar, ""));

                                // lineitem cost
                                List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == EntityId).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                                var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();
                                if (lineitemcostlist.Where(pcptc => pcptc.Period == (PeriodChar + (QuarterCnt)) || pcptc.Period == (PeriodChar + (QuarterCnt + 1)) || pcptc.Period == (PeriodChar + (QuarterCnt + 2))).Any())
                                {
                                    var costlineitemperiod = lineitemcostlist.Where(pcptc => pcptc.Period == (PeriodChar + (QuarterCnt)) || pcptc.Period == (PeriodChar + (QuarterCnt + 1)) || pcptc.Period == (PeriodChar + (QuarterCnt + 2))).Sum(pcptlc => pcptlc.Value);
                                    if (monthlycost < costlineitemperiod)
                                    {
                                        // Added by Viral Kadiya for Pl ticket #1970.
                                        string strReduceTacticPlannedCostMessage = string.Format(Common.objCached.TacticPlanedCostReduce, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                                        return Json(new { isSuccess = false, errormsg = strReduceTacticPlannedCostMessage });
                                        //monthlycost = costlineitemperiod;
                                    }
                                }

                                double tacticost = 0;
                                tacticost = objTactic.Plan_Campaign_Program_Tactic_Cost.Select(tactic => tactic.Value).Sum();

                                //// Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                isExists = false;
                                if (PrevTacticBudgetAllocationList != null && PrevTacticBudgetAllocationList.Count > 0)
                                {
                                    //// Get current Quarter months budget.
                                    thisQuartersMonthList = new List<Plan_Campaign_Program_Tactic_Cost>();
                                    thisQuartersMonthList = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                    thisQuarterFirstMonthBudget = new Plan_Campaign_Program_Tactic_Cost();
                                    thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                    if (thisQuarterFirstMonthBudget != null)
                                    {
                                        if (monthlycost >= 0)
                                        {
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
                                                    tacticost += BudgetDiff;
                                                    db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                }
                                                else
                                                {
                                                    j = 1;
                                                    while (BudgetDiff < 0)
                                                    {
                                                        if (thisQuarterFirstMonthBudget != null)
                                                        {
                                                            BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                            if (BudgetDiff <= 0)
                                                            {
                                                                tacticost -= thisQuarterFirstMonthBudget.Value;
                                                                thisQuarterFirstMonthBudget.Value = 0;
                                                            }
                                                            else
                                                            {
                                                                tacticost -= (thisQuarterFirstMonthBudget.Value - BudgetDiff);
                                                                thisQuarterFirstMonthBudget.Value = BudgetDiff;
                                                            }

                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                        }
                                                        if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                        {
                                                            thisQuarterFirstMonthBudget = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                        }

                                                        j = j + 1;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                        }
                                        isExists = true;
                                    }
                                }
                                //// if previous values does not exist then insert new values.
                                if (!isExists && monthlycost >= 0)
                                {
                                    //// End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                    objTacticBudget = new Plan_Campaign_Program_Tactic_Cost();
                                    objTacticBudget.PlanTacticId = EntityId;
                                    objTacticBudget.Period = PeriodChar + QuarterCnt;
                                    objTacticBudget.Value = Convert.ToDouble(monthlycost);
                                    objTacticBudget.CreatedBy = Sessions.User.ID;
                                    objTacticBudget.CreatedDate = DateTime.Now;
                                    db.Entry(objTacticBudget).State = EntityState.Added;
                                    tacticost += monthlycost;
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
                                            if (LinkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == linkedPeriod).Any())
                                            {
                                                LinkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == linkedPeriod).FirstOrDefault().Value = monthlycost;
                                            }
                                            else
                                            {
                                                Plan_Campaign_Program_Tactic_Cost objLinkedTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                                objLinkedTacticCost.PlanTacticId = linkedTacticId;
                                                objLinkedTacticCost.Period = linkedPeriod;
                                                objLinkedTacticCost.Value = monthlycost;
                                                objLinkedTacticCost.CreatedBy = Sessions.User.ID;
                                                objLinkedTacticCost.CreatedDate = DateTime.Now;
                                                db.Entry(objLinkedTacticCost).State = EntityState.Added;
                                            }
                                        }
                                    }
                                }


                                #endregion

                                double totalLineitemCost = 0;
                                if (objtotalLineitemCost != null && objtotalLineitemCost.Count() > 0)
                                {
                                    totalLineitemCost = objtotalLineitemCost.Sum(l => l.Cost);
                                }
                                if (totalLineitemCost > yearlycost)
                                {
                                    // Added by Viral Kadiya for Pl ticket #1970.
                                    string strReduceTacticPlannedCostMessage = string.Format(Common.objCached.TacticPlanedCostReduce, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                                    return Json(new { isSuccess = false, errormsg = strReduceTacticPlannedCostMessage });
                                    //yearlycost = totalLineitemCost;
                                }

                                if (tacticost > yearlycost)
                                {
                                    if (objTactic.Cost != yearlycost)
                                    {
                                        double diffcost = tacticost - yearlycost;
                                        int endmonth = 12;
                                        while (diffcost > 0 && endmonth != 0)
                                        {
                                            if (objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                                            {
                                                //Modified by komal Rawal
                                                double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + endmonth).Sum(lineitem => lineitem.Value);
                                                double objtacticcost = objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                                                var DiffMonthCost = objtacticcost - tacticlineitemcostmonth;
                                                if (DiffMonthCost > 0)
                                                {
                                                    if (DiffMonthCost > diffcost)
                                                    {
                                                        objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - diffcost;
                                                        diffcost = 0;
                                                    }
                                                    else
                                                    {
                                                        objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - DiffMonthCost;
                                                        diffcost = diffcost - DiffMonthCost;
                                                    }
                                                }
                                                //END
                                            }
                                            if (endmonth > 0)
                                            {
                                                endmonth -= 1;
                                            }
                                            // if not reduce value than what happen
                                        }
                                    }
                                    else
                                    {
                                        yearlycost = tacticost;
                                    }
                                }
                                else if (tacticost < yearlycost)
                                {
                                    if (objTactic.Cost != yearlycost)
                                    {
                                        double diffcost = yearlycost - tacticost;
                                        int startmonth = objTactic.StartDate.Month;
                                        if (objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                                        {
                                            objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value += diffcost;
                                        }
                                        else
                                        {
                                            Plan_Campaign_Program_Tactic_Cost objTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                            objTacticCost.PlanTacticId = EntityId;
                                            objTacticCost.Period = PeriodChar + startmonth;
                                            objTacticCost.Value = diffcost;
                                            objTacticCost.CreatedBy = Sessions.User.ID;
                                            objTacticCost.CreatedDate = DateTime.Now;
                                            db.Entry(objTacticCost).State = EntityState.Added;
                                        }
                                    }
                                    else
                                    {
                                        yearlycost = tacticost;
                                    }
                                }
                                objTactic.Cost = yearlycost;
                                objTactic.ModifiedBy = Sessions.User.ID;
                                objTactic.ModifiedDate = DateTime.Now;
                                db.Entry(objTactic).State = EntityState.Modified;
                                //// Calculate Total LineItem Cost.
                                Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                                if (objOtherLineItem == null)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = EntityId;
                                    // Modified by Arpita Soni for Ticket #2634 on 09/22/2016
                                    objNewLineitem.Title = Common.LineItemTitleDefault;
                                    if (yearlycost > totalLineitemCost)
                                    {
                                        objNewLineitem.Cost = yearlycost - totalLineitemCost;
                                    }
                                    else
                                    {
                                        objNewLineitem.Cost = 0;
                                    }
                                    objNewLineitem.Description = string.Empty;
                                    objNewLineitem.CreatedBy = Sessions.User.ID;
                                    objNewLineitem.CreatedDate = DateTime.Now;
                                    db.Entry(objNewLineitem).State = EntityState.Added;
                                }
                                else
                                {
                                    objOtherLineItem.IsDeleted = false;
                                    if (yearlycost > totalLineitemCost)
                                    {
                                        objOtherLineItem.Cost = yearlycost - totalLineitemCost;
                                    }
                                    else
                                    {
                                        objOtherLineItem.Cost = 0;
                                        objOtherLineItem.IsDeleted = true;
                                    }
                                    db.Entry(objOtherLineItem).State = EntityState.Modified;
                                }
                            }
                        }
                        else if (tab == Enums.BudgetTab.Actual.ToString())
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
                                        if (monthlycost >= 0)
                                        {
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
                                                    while (BudgetDiff < 0)
                                                    {
                                                        if (thisQuarterFirstMonthBudget != null)
                                                        {
                                                            BudgetDiff = thisQuarterFirstMonthBudget.Actualvalue + BudgetDiff;

                                                            if (BudgetDiff <= 0)
                                                                thisQuarterFirstMonthBudget.Actualvalue = 0;
                                                            else
                                                                thisQuarterFirstMonthBudget.Actualvalue = BudgetDiff;

                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                        }
                                                        if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                        {
                                                            thisQuarterFirstMonthBudget = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                        }

                                                        j = j + 1;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                        }
                                        isExists = true;
                                    }
                                }
                                //// if previous values does not exist then insert new values.
                                if (!isExists && monthlycost >= 0)
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
                        if (tab == Enums.BudgetTab.Planned.ToString())
                        {
                            if (!isquarter)
                            {
                                var objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt => pcpt.PlanLineItemId == EntityId && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
                                int plantacticid = objLineitem.PlanTacticId;
                                List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == plantacticid).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                                var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();


                                //if (lineitemcostlist.Where(pcptc => pcptc.Period == period).Any())
                                //{
                                //    var costlineitemperiod = lineitemcostlist.Where(pcptlc => pcptlc.Period == period).Sum(pcptlc => pcptlc.Value);
                                //    if (monthlycost < costlineitemperiod)
                                //    {
                                //        monthlycost = costlineitemperiod;
                                //    }
                                //}

                                double tacticost = 0;
                                double lineitemtotalcost = 0;
                                var objTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == plantacticid && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
                                List<Plan_Campaign_Program_Tactic_Cost> tacticostslist = objTactic.Plan_Campaign_Program_Tactic_Cost.ToList();
                                tacticost = objTactic.Plan_Campaign_Program_Tactic_Cost.Select(tactic => tactic.Value).Sum();
                                bool isAdded = false;
                                lineitemtotalcost = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Select(tactic => tactic.Value).Sum();
                                if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == period).Any())
                                {
                                    var objlineitemcost = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == period).FirstOrDefault().Value;
                                    objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == period).FirstOrDefault().Value = monthlycost;
                                    lineitemtotalcost += monthlycost - objlineitemcost;
                                }
                                else
                                {
                                    Plan_Campaign_Program_Tactic_LineItem_Cost objlineitemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                    objlineitemCost.PlanLineItemId = EntityId;
                                    objlineitemCost.Period = period;
                                    objlineitemCost.Value = monthlycost;
                                    objlineitemCost.CreatedBy = Sessions.User.ID;
                                    objlineitemCost.CreatedDate = DateTime.Now;
                                    db.Entry(objlineitemCost).State = EntityState.Added;
                                    lineitemtotalcost += monthlycost;
                                    isAdded = true;
                                }


                                if (tacticostslist.Where(pcptc => pcptc.Period == period).Any())
                                {
                                    var tacticmonthcost = tacticostslist.Where(pcptc => pcptc.Period == period).FirstOrDefault().Value;
                                    double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == period).Sum(lineitem => lineitem.Value); //modified by komal Rawal
                                    if (isAdded)
                                    {
                                        tacticlineitemcostmonth = tacticlineitemcostmonth + monthlycost;
                                    }
                                    tacticostslist.Where(pcptc => pcptc.Period == period).FirstOrDefault().Value = tacticlineitemcostmonth;
                                    objTactic.Cost = objTactic.Cost + (tacticlineitemcostmonth - tacticmonthcost);
                                    //}
                                }
                                else
                                {
                                    double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == period).Sum(lineitem => lineitem.Value); //modified by komal Rawal
                                    Plan_Campaign_Program_Tactic_Cost objtacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                    // Add By Nishant Sheth #2538 issue month values are not stored properly
                                    if (monthlycost > tacticlineitemcostmonth)
                                    {
                                        tacticlineitemcostmonth = monthlycost - tacticlineitemcostmonth;
                                    }
                                    else
                                    {
                                        tacticlineitemcostmonth = tacticlineitemcostmonth - monthlycost;
                                    }
                                    objtacticCost.PlanTacticId = plantacticid;
                                    objtacticCost.Period = period;
                                    objtacticCost.Value = monthlycost; // Change By Nishant Sheth #2538
                                    objtacticCost.CreatedBy = Sessions.User.ID;
                                    objtacticCost.CreatedDate = DateTime.Now;
                                    db.Entry(objtacticCost).State = EntityState.Added;
                                    objTactic.Cost = objTactic.Cost + tacticlineitemcostmonth;
                                }

                                if (lineitemtotalcost > yearlycost)
                                {
                                    if (objLineitem.Cost != yearlycost)
                                    {
                                        double diffcost = lineitemtotalcost - yearlycost;
                                        int endmonth = 12;
                                        while (diffcost > 0 && endmonth != 0)
                                        {
                                            if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                                            {
                                                double objlineitemcost = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                                                if (objlineitemcost > diffcost)
                                                {
                                                    objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objlineitemcost - diffcost;
                                                    diffcost = 0;
                                                }
                                                else
                                                {
                                                    objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = 0;
                                                    diffcost = diffcost - objlineitemcost;
                                                }
                                            }
                                            if (endmonth > 0)
                                            {
                                                endmonth -= 1;
                                            }
                                            // if not reduce value than what happen
                                        }
                                    }
                                    else
                                    {
                                        yearlycost = lineitemtotalcost;
                                    }
                                }
                                else if (lineitemtotalcost < yearlycost)
                                {
                                    if (objLineitem.Cost != yearlycost)
                                    {
                                        double diffcost = yearlycost - lineitemtotalcost;
                                        int startmonth = objTactic.StartDate.Month;
                                        double lineitemextracost = diffcost;
                                        if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                                        {
                                            lineitemextracost += objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value;
                                            objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value += diffcost;
                                        }
                                        else
                                        {
                                            Plan_Campaign_Program_Tactic_LineItem_Cost objlineitemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                            objlineitemCost.PlanLineItemId = EntityId;
                                            objlineitemCost.Period = PeriodChar + startmonth;
                                            objlineitemCost.Value = diffcost;
                                            objlineitemCost.CreatedBy = Sessions.User.ID;
                                            objlineitemCost.CreatedDate = DateTime.Now;
                                            db.Entry(objlineitemCost).State = EntityState.Added;
                                        }

                                        if (tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                                        {
                                            var tacticmonthcost = tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value;
                                            double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.PlanLineItemId != EntityId && lineitem.Period == PeriodChar + startmonth).Sum(lineitem => lineitem.Value) + lineitemextracost;
                                            if (tacticlineitemcostmonth > tacticmonthcost)
                                            {
                                                tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value = tacticlineitemcostmonth;
                                                objTactic.Cost = objTactic.Cost + (tacticlineitemcostmonth - tacticmonthcost);
                                            }
                                        }
                                        else
                                        {
                                            double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.PlanLineItemId != EntityId && lineitem.Period == period).Sum(lineitem => lineitem.Value) + lineitemextracost;
                                            Plan_Campaign_Program_Tactic_Cost objtacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                            objtacticCost.PlanTacticId = plantacticid;
                                            objtacticCost.Period = period;
                                            objtacticCost.Value = tacticlineitemcostmonth;
                                            objtacticCost.CreatedBy = Sessions.User.ID;
                                            objtacticCost.CreatedDate = DateTime.Now;
                                            db.Entry(objtacticCost).State = EntityState.Added;
                                            objTactic.Cost = objTactic.Cost + tacticlineitemcostmonth;
                                        }
                                    }
                                    else
                                    {
                                        yearlycost = lineitemtotalcost;
                                    }
                                }
                                objLineitem.Cost = yearlycost;
                                db.Entry(objLineitem).State = EntityState.Modified;
                                objTactic.ModifiedBy = Sessions.User.ID;
                                objTactic.ModifiedDate = DateTime.Now;
                                db.Entry(objTactic).State = EntityState.Modified;
                                //// Calculate Total LineItem Cost.
                                Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                                double tacticlineitemcost = objtotalLineitemCost.Where(lineitem => lineitem.PlanLineItemId != EntityId).Sum(lineitem => lineitem.Cost) + yearlycost;
                                if (objOtherLineItem == null)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = EntityId;
                                    // Modified by Arpita Soni for Ticket #2634 on 09/22/2016
                                    objNewLineitem.Title = Common.LineItemTitleDefault;
                                    if (objTactic.Cost > tacticlineitemcost)
                                    {
                                        objNewLineitem.Cost = objTactic.Cost - tacticlineitemcost;
                                    }
                                    else
                                    {
                                        objNewLineitem.Cost = 0;
                                    }
                                    objNewLineitem.Description = string.Empty;
                                    objNewLineitem.CreatedBy = Sessions.User.ID;
                                    objNewLineitem.CreatedDate = DateTime.Now;
                                    db.Entry(objNewLineitem).State = EntityState.Added;
                                }
                                else
                                {
                                    objOtherLineItem.IsDeleted = false;
                                    if (objTactic.Cost > tacticlineitemcost)
                                    {
                                        objOtherLineItem.Cost = objTactic.Cost - tacticlineitemcost;
                                    }
                                    else
                                    {
                                        objOtherLineItem.Cost = 0;
                                        objOtherLineItem.IsDeleted = true;
                                    }
                                    db.Entry(objOtherLineItem).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                var objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt => pcpt.PlanLineItemId == EntityId && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
                                int plantacticid = objLineitem.PlanTacticId;
                                List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == plantacticid).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                                var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();

                                //// Get Previous budget allocation data by PlanId.
                                var PrevTacticBudgetAllocationList = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Select(pb => pb).ToList();
                                int QuarterCnt = 1, j = 1;
                                bool isExists;
                                List<Plan_Campaign_Program_Tactic_LineItem_Cost> thisQuartersMonthList;
                                Plan_Campaign_Program_Tactic_LineItem_Cost thisQuarterFirstMonthBudget, objTacticBudget;
                                double thisQuarterOtherMonthBudget = 0, thisQuarterTotalBudget = 0, newValue = 0, BudgetDiff = 0;

                                QuarterCnt = Convert.ToInt32(period.Replace(PeriodChar, ""));

                                double tacticost = 0;
                                double lineitemtotalcost = 0;
                                var objTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == plantacticid && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
                                List<Plan_Campaign_Program_Tactic_Cost> tacticostslist = objTactic.Plan_Campaign_Program_Tactic_Cost.ToList();
                                tacticost = objTactic.Plan_Campaign_Program_Tactic_Cost.Select(tactic => tactic.Value).Sum();

                                double LineItemSelfDiffVal = 0;

                                //// Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                isExists = false;
                                if (PrevTacticBudgetAllocationList != null && PrevTacticBudgetAllocationList.Count > 0)
                                {
                                    //// Get current Quarter months budget.
                                    thisQuartersMonthList = new List<Plan_Campaign_Program_Tactic_LineItem_Cost>();
                                    thisQuartersMonthList = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                    thisQuarterFirstMonthBudget = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                    thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();
                                    lineitemtotalcost = PrevTacticBudgetAllocationList.Sum(lineitembudget => lineitembudget.Value);
                                    if (thisQuarterFirstMonthBudget != null)
                                    {
                                        if (monthlycost >= 0)
                                        {
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
                                                    lineitemtotalcost += BudgetDiff;
                                                    db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                }
                                                else
                                                {
                                                    j = 1;
                                                    while (BudgetDiff < 0)
                                                    {
                                                        if (thisQuarterFirstMonthBudget != null)
                                                        {
                                                            BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                            if (BudgetDiff <= 0)
                                                            {
                                                                lineitemtotalcost -= thisQuarterFirstMonthBudget.Value;
                                                                thisQuarterFirstMonthBudget.Value = 0;
                                                            }
                                                            else
                                                            {
                                                                lineitemtotalcost -= (thisQuarterFirstMonthBudget.Value - BudgetDiff);
                                                                thisQuarterFirstMonthBudget.Value = BudgetDiff;
                                                            }

                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                        }
                                                        if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                        {
                                                            thisQuarterFirstMonthBudget = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                        }

                                                        j = j + 1;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                        }
                                        isExists = true;
                                    }
                                }

                                //// if previous values does not exist then insert new values.
                                if (!isExists && monthlycost >= 0)
                                {
                                    //// End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                    objTacticBudget = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                    objTacticBudget.PlanLineItemId = EntityId;
                                    objTacticBudget.Period = PeriodChar + QuarterCnt;
                                    objTacticBudget.Value = Convert.ToDouble(monthlycost);
                                    objTacticBudget.CreatedBy = Sessions.User.ID;
                                    objTacticBudget.CreatedDate = DateTime.Now;
                                    db.Entry(objTacticBudget).State = EntityState.Added;
                                    tacticost += monthlycost;
                                }



                                if (lineitemtotalcost > yearlycost)
                                {
                                    if (objLineitem.Cost != yearlycost)
                                    {
                                        double diffcost = lineitemtotalcost - yearlycost;
                                        int endmonth = 12;
                                        int endmonthactual = endmonth;
                                        while (diffcost > 0 && endmonth != 0)
                                        {
                                            if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                                            {
                                                double objlineitemcost = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                                                if (objlineitemcost > diffcost)
                                                {
                                                    objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objlineitemcost - diffcost;
                                                    diffcost = 0;
                                                }
                                                else
                                                {
                                                    objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = 0;
                                                    diffcost = diffcost - objlineitemcost;
                                                }
                                            }
                                            if (endmonth > 0)
                                            {
                                                endmonth -= 1;
                                            }
                                            // if not reduce value than what happen
                                        }
                                    }
                                    else
                                    {
                                        yearlycost = lineitemtotalcost;
                                    }
                                }
                                else if (lineitemtotalcost < yearlycost)
                                {
                                    if (objLineitem.Cost != yearlycost)
                                    {
                                        double diffcost = yearlycost - lineitemtotalcost;
                                        int startmonth = objTactic.StartDate.Month;
                                        if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                                        {
                                            objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value += diffcost;
                                        }
                                        else
                                        {
                                            Plan_Campaign_Program_Tactic_LineItem_Cost objlineitemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                            objlineitemCost.PlanLineItemId = EntityId;
                                            objlineitemCost.Period = PeriodChar + startmonth;
                                            objlineitemCost.Value = diffcost;
                                            objlineitemCost.CreatedBy = Sessions.User.ID;
                                            objlineitemCost.CreatedDate = DateTime.Now;
                                            db.Entry(objlineitemCost).State = EntityState.Added;
                                        }
                                    }
                                    else
                                    {
                                        yearlycost = lineitemtotalcost;
                                    }
                                }
                                if (!isExists)
                                {
                                    objLineitem.Cost = yearlycost + Convert.ToDouble(monthlycost);
                                }
                                else
                                {
                                    objLineitem.Cost = yearlycost;
                                }

                                List<string> QuarterList;
                                double monthlyTotalLineItemCost = 0, monthlyTotalTacticCost = 0, diffCost = 0, tacticinnerCost = 0;

                                for (int quarterno = 1; quarterno <= 12; quarterno += 3)
                                {
                                    ///QurterList which contains list of month as per quarter. e.g. for Q1, list is Y1,Y2 and Y3
                                    QuarterList = new List<string>();
                                    monthlyTotalLineItemCost = 0;
                                    monthlyTotalTacticCost = 0;
                                    diffCost = 0;
                                    tacticinnerCost = 0;
                                    for (int J = 0; J < 3; J++)
                                    {
                                        QuarterList.Add(PeriodChar + (quarterno + J).ToString());
                                    }
                                    //string period = PeriodChar + QuarterCnt.ToString();
                                    //monthlyTotalLineItemCost = PrevTacticBudgetAllocationList.Where(lineCost => QuarterList.Contains(lineCost.Period)).ToList().Sum(lineCost => lineCost.Value);
                                    monthlyTotalLineItemCost = lineitemcostlist.Where(lineCost => QuarterList.Contains(lineCost.Period)).ToList().Sum(lineCost => lineCost.Value);

                                    if (!isExists && QuarterList.Where(q => q == period).Any())
                                    {
                                        monthlyTotalLineItemCost = monthlyTotalLineItemCost + Convert.ToDouble(monthlycost);
                                    }
                                    monthlyTotalTacticCost = tacticostslist.Where(_tacCost => QuarterList.Contains(_tacCost.Period)).ToList().Sum(_tacCost => _tacCost.Value);
                                    if (monthlyTotalLineItemCost > monthlyTotalTacticCost)
                                    {
                                        period = QuarterList[0].ToString();
                                        diffCost = monthlyTotalLineItemCost - monthlyTotalTacticCost;
                                        if (diffCost >= 0)
                                        {
                                            if (tacticostslist.Where(_tacCost => _tacCost.Period == period).Any())
                                            {
                                                var _tacCost = tacticostslist.Where(_tac => _tac.Period == period).FirstOrDefault();
                                                _tacCost.Value = _tacCost.Value + diffCost;
                                                db.Entry(_tacCost).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                Plan_Campaign_Program_Tactic_Cost objtacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                                objtacticCost.PlanTacticId = plantacticid;
                                                objtacticCost.Period = period;
                                                objtacticCost.Value = diffCost;
                                                objtacticCost.CreatedBy = Sessions.User.ID;
                                                objtacticCost.CreatedDate = DateTime.Now;
                                                db.Entry(objtacticCost).State = EntityState.Added;

                                            }
                                        }
                                        //period = QuarterList[0].ToString();
                                        //diffCost = monthlyTotalLineItemCost - monthlyTotalTacticCost;
                                        //int periodCount = 0;
                                        //////If cost diffrence is lower than 0 than reduce it from quarter in series of 1st month of quarter,2nd month of quarter...
                                        //while (diffCost < 0)
                                        //{
                                        //    period = QuarterList[periodCount].ToString();
                                        //    tacticinnerCost = tacticostslist.Where(_tacCost => _tacCost.Period == period).ToList().Sum(_tacCost => _tacCost.Value);
                                        //    if ((diffCost + tacticinnerCost) >= 0)
                                        //    {
                                        //        tacticostslist.Where(_tacCost => _tacCost.Period == period).ToList().ForEach(_tacCost => { _tacCost.Value = _tacCost.Value + diffCost; db.Entry(_tacCost).State = EntityState.Modified; });
                                        //    }
                                        //    else
                                        //    {
                                        //        tacticostslist.Where(_tacCost => _tacCost.Period == period).ToList().ForEach(_tacCost => { _tacCost.Value = 0; db.Entry(_tacCost).State = EntityState.Modified; });
                                        //    }
                                        //    diffCost = diffCost + tacticinnerCost;
                                        //    periodCount = periodCount + 1;
                                        //}
                                        objTactic.Cost = objTactic.Cost + diffCost;
                                    }
                                    else if (monthlyTotalLineItemCost < monthlyTotalTacticCost)
                                    {
                                        // Plan_Campaign_Program_Tactic_LineItem OtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                                        if (QuarterList.Where(q => q == period).Any())
                                        {
                                            period = QuarterList[0].ToString();
                                            diffCost = LineItemSelfDiffVal; //monthlyTotalLineItemCost - monthlyTotalTacticCost;
                                            //if (OtherLineItem != null)
                                            //{
                                            //    diffCost += OtherLineItem.Cost;
                                            //}
                                            if (diffCost < 0)
                                            {
                                                objTactic.Cost = objTactic.Cost + diffCost;
                                                int periodCount = 0;
                                                ////If cost diffrence is lower than 0 than reduce it from quarter in series of 1st month of quarter,2nd month of quarter...
                                                while (diffCost < 0)
                                                {
                                                    period = QuarterList[periodCount].ToString();
                                                    tacticinnerCost = tacticostslist.Where(_tacCost => _tacCost.Period == period).ToList().Sum(_tacCost => _tacCost.Value);
                                                    if ((diffCost + tacticinnerCost) >= 0)
                                                    {
                                                        tacticostslist.Where(_tacCost => _tacCost.Period == period).ToList().ForEach(_tacCost => { _tacCost.Value = _tacCost.Value + diffCost; db.Entry(_tacCost).State = EntityState.Modified; });
                                                    }
                                                    else
                                                    {
                                                        tacticostslist.Where(_tacCost => _tacCost.Period == period).ToList().ForEach(_tacCost => { _tacCost.Value = 0; db.Entry(_tacCost).State = EntityState.Modified; });
                                                    }
                                                    diffCost = diffCost + tacticinnerCost;
                                                    periodCount = periodCount + 1;
                                                }
                                            }
                                            else if (diffCost > 0)
                                            {
                                                Plan_Campaign_Program_Tactic_LineItem OtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                                                double otherCost = OtherLineItem.Cost;
                                                if (OtherLineItem != null && diffCost > otherCost)
                                                {
                                                    objTactic.Cost += diffCost - otherCost;
                                                }
                                            }
                                        }


                                    }
                                }
                                objTactic.ModifiedBy = Sessions.User.ID;
                                objTactic.ModifiedDate = DateTime.Now;
                                db.Entry(objTactic).State = EntityState.Modified;
                                db.Entry(objLineitem).State = EntityState.Modified;
                                //// Calculate Total LineItem Cost.
                                Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                                double totallineitemcost = objtotalLineitemCost.Where(lineitem => lineitem.PlanLineItemId != EntityId).Sum(lineitem => lineitem.Cost) + yearlycost;
                                if (!isExists)
                                {
                                    totallineitemcost += Convert.ToDouble(monthlycost);
                                }
                                if (objOtherLineItem == null)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = EntityId;
                                    // Modified by Arpita Soni for Ticket #2634 on 09/22/2016
                                    objNewLineitem.Title = Common.LineItemTitleDefault;
                                    if (objTactic.Cost > totallineitemcost)
                                    {
                                        objNewLineitem.Cost = objTactic.Cost - totallineitemcost;
                                        objNewLineitem.IsDeleted = false;
                                    }
                                    else
                                    {
                                        objNewLineitem.Cost = 0;
                                        objNewLineitem.IsDeleted = true;
                                    }
                                    objNewLineitem.Description = string.Empty;
                                    objNewLineitem.CreatedBy = Sessions.User.ID;
                                    objNewLineitem.CreatedDate = DateTime.Now;
                                    db.Entry(objNewLineitem).State = EntityState.Added;
                                }
                                else
                                {
                                    objOtherLineItem.IsDeleted = false;
                                    if (objTactic.Cost > totallineitemcost)
                                    {
                                        objOtherLineItem.Cost = objTactic.Cost - totallineitemcost;
                                    }
                                    else
                                    {
                                        objOtherLineItem.Cost = 0;
                                        objOtherLineItem.IsDeleted = true;
                                    }
                                    db.Entry(objOtherLineItem).State = EntityState.Modified;
                                }
                            }
                        }
                        else if (tab == Enums.BudgetTab.Actual.ToString())
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
                                        if (monthlycost >= 0)
                                        {
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
                                                    while (BudgetDiff < 0)
                                                    {
                                                        if (thisQuarterFirstMonthBudget != null)
                                                        {
                                                            BudgetDiff = thisQuarterFirstMonthBudget.Value + BudgetDiff;

                                                            if (BudgetDiff <= 0)
                                                                thisQuarterFirstMonthBudget.Value = 0;
                                                            else
                                                                thisQuarterFirstMonthBudget.Value = BudgetDiff;

                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                        }
                                                        if ((QuarterCnt + j) <= (QuarterCnt + 2))
                                                        {
                                                            thisQuarterFirstMonthBudget = PrevTacticBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
                                                        }

                                                        j = j + 1;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            thisQuartersMonthList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                        }
                                        isExists = true;
                                    }
                                }
                                //// if previous values does not exist then insert new values.
                                if (!isExists && monthlycost >= 0)
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
                else if (popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).Any())
                {
                    if (string.Compare(section, Enums.Section.Tactic.ToString(), true) == 0 && tab == Enums.BudgetTab.Planned.ToString())
                    {
                        Plan_Campaign_Program_Tactic pcpobj = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(EntityId)).FirstOrDefault();
                        //// check that Tactic cost count greater than 0 OR Plan's AllocatedBy is None or Defaults.
                        var YearlyCost = Convert.ToDouble(popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).FirstOrDefault().Value);

                        List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = new List<Plan_Campaign_Program_Tactic_LineItem>();
                        double totalLineitemCost = 0;
                        tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == pcpobj.PlanTacticId).ToList();
                        List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                        //Modified By komal rawal
                        var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                        List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();
                        //End

                        if (objtotalLineitemCost != null && objtotalLineitemCost.Count() > 0)
                            totalLineitemCost = objtotalLineitemCost.Sum(l => l.Cost);
                        if (totalLineitemCost > YearlyCost)
                        {
                            YearlyCost = totalLineitemCost;
                            string strReduceTacticPlannedCostMessage = string.Format(Common.objCached.TacticPlanedCostReduce, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                            return Json(new { isSuccess = false, errormsg = strReduceTacticPlannedCostMessage });

                        }

                        if (!pcpobj.Plan_Campaign_Program_Tactic_Cost.Any())
                        {
                            pcpobj.Cost = 0;
                        }
                        //Added By komal Rawal for #1249
                        if (YearlyCost > pcpobj.Cost)
                        {
                            var diffcost = YearlyCost - pcpobj.Cost;
                            int startmonth = pcpobj.StartDate.Month;

                            if (pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                            {
                                pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value += diffcost;

                            }
                            else
                            {
                                Plan_Campaign_Program_Tactic_Cost objTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                objTacticCost.PlanTacticId = pcpobj.PlanTacticId;
                                objTacticCost.Period = PeriodChar + startmonth;
                                objTacticCost.Value = diffcost;
                                objTacticCost.CreatedBy = Sessions.User.ID;
                                objTacticCost.CreatedDate = DateTime.Now;
                                db.Entry(objTacticCost).State = EntityState.Added;
                            }

                        }
                        else if (YearlyCost < pcpobj.Cost)
                        {
                            var diffcost = pcpobj.Cost - YearlyCost;
                            int endmonth = 12;
                            while (diffcost > 0 && endmonth != 0)
                            {
                                if (pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                                {
                                    //Modified by komal Rawal
                                    double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + endmonth).Sum(lineitem => lineitem.Value);
                                    double objtacticcost = pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                                    var DiffMonthCost = objtacticcost - tacticlineitemcostmonth;
                                    if (DiffMonthCost > 0)
                                    {
                                        if (DiffMonthCost > diffcost)
                                        {
                                            pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - diffcost;
                                            diffcost = 0;
                                        }
                                        else
                                        {
                                            pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - DiffMonthCost;
                                            diffcost = diffcost - DiffMonthCost;
                                        }
                                    }
                                    //END
                                }
                                if (endmonth > 0)
                                {
                                    endmonth -= 1;
                                }

                            }

                        }

                        pcpobj.Cost = YearlyCost;
                        Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                        if (objOtherLineItem == null)
                        {
                            Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                            objNewLineitem.PlanTacticId = pcpobj.PlanTacticId;
                            // Modified by Arpita Soni for Ticket #2634 on 09/22/2016
                            objNewLineitem.Title = Common.LineItemTitleDefault;
                            if (pcpobj.Cost > totalLineitemCost)
                            {
                                objNewLineitem.Cost = pcpobj.Cost - totalLineitemCost;
                            }
                            else
                            {
                                objNewLineitem.Cost = 0;
                            }
                            objNewLineitem.Description = string.Empty;
                            objNewLineitem.CreatedBy = Sessions.User.ID;
                            objNewLineitem.CreatedDate = DateTime.Now;
                            db.Entry(objNewLineitem).State = EntityState.Added;
                        }
                        else
                        {
                            objOtherLineItem.IsDeleted = false;
                            if (pcpobj.Cost > totalLineitemCost)
                            {
                                objOtherLineItem.Cost = pcpobj.Cost - totalLineitemCost;
                            }
                            else
                            {
                                objOtherLineItem.Cost = 0;
                                objOtherLineItem.IsDeleted = true;
                            }
                            db.Entry(objOtherLineItem).State = EntityState.Modified;
                        }
                        pcpobj.ModifiedBy = Sessions.User.ID;
                        pcpobj.ModifiedDate = DateTime.Now;
                        db.Entry(pcpobj).State = EntityState.Modified;

                        //End
                    }
                    else if (string.Compare(section, Enums.Section.LineItem.ToString(), true) == 0 && tab == Enums.BudgetTab.Planned.ToString())
                    {
                        Plan_Campaign_Program_Tactic_LineItem objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobjw => pcpobjw.PlanLineItemId.Equals(EntityId));
                        var YearlyCost = Convert.ToDouble(popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).FirstOrDefault().Value);
                        var objTactic = objLineitem.Plan_Campaign_Program_Tactic;
                        if (YearlyCost > objLineitem.Cost)
                        {
                            var diffcost = YearlyCost - objLineitem.Cost;
                            int startmonth = objTactic.StartDate.Month; ;

                            if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                            {
                                objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value += diffcost;
                            }
                            else
                            {
                                Plan_Campaign_Program_Tactic_LineItem_Cost objlineitemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                objlineitemCost.PlanLineItemId = objLineitem.PlanLineItemId;
                                objlineitemCost.Period = PeriodChar + startmonth;
                                objlineitemCost.Value = diffcost;
                                objlineitemCost.CreatedBy = Sessions.User.ID;
                                objlineitemCost.CreatedDate = DateTime.Now;
                                db.Entry(objlineitemCost).State = EntityState.Added;
                            }
                            db.SaveChanges();
                            List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == objTactic.PlanTacticId).ToList();
                            List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                            var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                            List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();

                            List<Plan_Campaign_Program_Tactic_Cost> tacticostslist = objTactic.Plan_Campaign_Program_Tactic_Cost.ToList();
                            double tacticost = objTactic.Plan_Campaign_Program_Tactic_Cost.Select(tactic => tactic.Value).Sum();

                            if (tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                            {
                                var tacticmonthcost = tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value;
                                double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + startmonth).Sum(lineitem => lineitem.Value);//modified by komal Rawal
                                if (tacticlineitemcostmonth > tacticmonthcost)
                                {
                                    tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value = tacticlineitemcostmonth;
                                    objTactic.Cost = objTactic.Cost + (tacticlineitemcostmonth - tacticmonthcost);
                                }
                            }
                            else
                            {
                                double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + startmonth).Sum(lineitem => lineitem.Value);//modified by komal Rawal
                                Plan_Campaign_Program_Tactic_Cost objtacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                objtacticCost.PlanTacticId = objTactic.PlanTacticId;
                                objtacticCost.Period = PeriodChar + startmonth;
                                objtacticCost.Value = tacticlineitemcostmonth;
                                objtacticCost.CreatedBy = Sessions.User.ID;
                                objtacticCost.CreatedDate = DateTime.Now;
                                db.Entry(objtacticCost).State = EntityState.Added;
                                objTactic.Cost = objTactic.Cost + tacticlineitemcostmonth;
                            }

                        }
                        else if (YearlyCost < objLineitem.Cost)
                        {
                            double diffcost, setTacCost, tacDiffCost;
                            diffcost = setTacCost = tacDiffCost = objLineitem.Cost - YearlyCost;
                            int endmonth = 12;
                            while (diffcost > 0 && endmonth != 0)
                            {
                                if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                                {
                                    double objtacticcost = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                                    if (objtacticcost > diffcost)
                                    {
                                        objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - diffcost;
                                        diffcost = 0;
                                    }
                                    else
                                    {
                                        objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = 0;
                                        diffcost = diffcost - objtacticcost;
                                    }
                                }
                                if (endmonth > 0)
                                {
                                    endmonth -= 1;
                                }

                            }

                            #region "Reduce Tactic cost while reduceing line item cost"
                            //var diffcost = pcpobj.Cost - Convert.ToDouble(UpdateVal);

                            List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = new List<Plan_Campaign_Program_Tactic_LineItem>();
                            tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == objTactic.PlanTacticId).ToList();

                            List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                            var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();

                            List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();

                            Plan_Campaign_Program_Tactic ObjLinkedTactic = null;

                            ReduceTacticPlannedCost(ref objTactic, ref ObjLinkedTactic, ref lineitemcostlist, tacDiffCost);
                            #endregion

                        }

                        objLineitem.Cost = YearlyCost;
                        //// Calculate TotalLineItemCost.
                        double totalLineitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == objTactic.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);

                        //// Insert or Modified LineItem Data.
                        var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == objTactic.PlanTacticId && l.LineItemTypeId == null);

                        //// if Tactic total cost is greater than totalLineItem cost then Insert LineItem record otherwise delete objOtherLineItem data from table Plan_Campaign_Program_Tactic_LineItem. 
                        if ((objOtherLineItem == null) && (objTactic.Cost > totalLineitemCost))
                        {
                            //// Insert New record to table.
                            Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                            objNewLineitem.PlanTacticId = objTactic.PlanTacticId;
                            // Modified by Arpita Soni for Ticket #2634 on 09/22/2016
                            objNewLineitem.Title = Common.LineItemTitleDefault;
                            if (objTactic.Cost > totalLineitemCost)
                            {
                                objNewLineitem.Cost = objTactic.Cost - totalLineitemCost;
                            }
                            else
                            {
                                objNewLineitem.Cost = 0;
                            }

                            objNewLineitem.Description = string.Empty;
                            objNewLineitem.CreatedBy = Sessions.User.ID;
                            objNewLineitem.CreatedDate = DateTime.Now;
                            db.Entry(objNewLineitem).State = EntityState.Added;
                            db.SaveChanges();
                        }
                        else if (objOtherLineItem != null)
                        {
                            objOtherLineItem.IsDeleted = false;
                            if (objTactic.Cost > totalLineitemCost)
                            {
                                objOtherLineItem.Cost = objTactic.Cost - totalLineitemCost;
                            }
                            else
                            {
                                objOtherLineItem.Cost = 0;
                                objOtherLineItem.IsDeleted = true;
                            }
                            db.Entry(objOtherLineItem).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        db.Entry(objLineitem).State = EntityState.Modified;
                        objTactic.ModifiedBy = Sessions.User.ID;
                        objTactic.ModifiedDate = DateTime.Now;
                        db.Entry(objTactic).State = EntityState.Modified;
                        //End
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



        /// <summary>
        /// Get the plan list for the selected business unit
        /// </summary>
        /// <param name="Bid"></param>
        /// <returns></returns>
        public ActionResult BudgetPlanList()
        {
            HomePlan objHomePlan = new HomePlan();
            List<SelectListItem> planList;
            //if (Bid == "false")
            //{
            planList = Common.GetPlan().Select(_pln => new SelectListItem() { Text = _pln.Title, Value = _pln.PlanId.ToString() }).OrderBy(_pln => _pln.Text).ToList();
            if (planList.Count > 0)
            {
                var objexists = planList.Where(_pln => _pln.Value == Sessions.PlanId.ToString()).ToList();
                if (objexists.Count != 0)
                {
                    planList.FirstOrDefault(_pln => _pln.Value.Equals(Sessions.PlanId.ToString())).Selected = true;
                }
                /*changed by Nirav for plan consistency on 14 apr 2014*/

                if (!Common.IsPlanPublished(Sessions.PlanId))
                {
                    string planPublishedStatus = Enums.PlanStatus.Published.ToString();
                    var activeplan = db.Plans.Where(_pln => _pln.PlanId == Sessions.PlanId && _pln.IsDeleted == false && _pln.Status == planPublishedStatus).ToList();
                    if (activeplan.Count > 0)
                        Sessions.PublishedPlanId = Sessions.PlanId;
                    else
                        Sessions.PublishedPlanId = 0;
                }
            }
            objHomePlan.plans = planList;

            return PartialView("_BudgetingPlanList", objHomePlan);
        }
        
        #endregion

        /// <summary>
        /// Manage lines items if cost is allocated to other
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<BudgetModel> ManageLineItems(List<BudgetModel> model)
        {
            foreach (BudgetModel l in model.Where(l => l.ActivityType == ActivityType.ActivityTactic))
            {
                //// Calculate Line Difference.
                BudgetMonth lineDiff = new BudgetMonth();
                List<BudgetModel> lines = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId).ToList();
                BudgetModel otherLine = lines.Where(ol => ol.LineItemTypeId == null).FirstOrDefault();
                lines = lines.Where(ol => ol.LineItemTypeId != null).ToList();
                if (otherLine != null)
                {
                    if (lines.Count > 0)
                    {
                        lineDiff.Jan = l.Month.Jan - lines.Sum(lmon => (double?)lmon.Month.Jan) ?? 0;
                        lineDiff.Feb = l.Month.Feb - lines.Sum(lmon => (double?)lmon.Month.Feb) ?? 0;
                        lineDiff.Mar = l.Month.Mar - lines.Sum(lmon => (double?)lmon.Month.Mar) ?? 0;
                        lineDiff.Apr = l.Month.Apr - lines.Sum(lmon => (double?)lmon.Month.Apr) ?? 0;
                        lineDiff.May = l.Month.May - lines.Sum(lmon => (double?)lmon.Month.May) ?? 0;
                        lineDiff.Jun = l.Month.Jun - lines.Sum(lmon => (double?)lmon.Month.Jun) ?? 0;
                        lineDiff.Jul = l.Month.Jul - lines.Sum(lmon => (double?)lmon.Month.Jul) ?? 0;
                        lineDiff.Aug = l.Month.Aug - lines.Sum(lmon => (double?)lmon.Month.Aug) ?? 0;
                        lineDiff.Sep = l.Month.Sep - lines.Sum(lmon => (double?)lmon.Month.Sep) ?? 0;
                        lineDiff.Oct = l.Month.Oct - lines.Sum(lmon => (double?)lmon.Month.Oct) ?? 0;
                        lineDiff.Nov = l.Month.Nov - lines.Sum(lmon => (double?)lmon.Month.Nov) ?? 0;
                        lineDiff.Dec = l.Month.Dec - lines.Sum(lmon => (double?)lmon.Month.Dec) ?? 0;

                        lineDiff.Jan = lineDiff.Jan < 0 ? 0 : lineDiff.Jan;
                        lineDiff.Feb = lineDiff.Feb < 0 ? 0 : lineDiff.Feb;
                        lineDiff.Mar = lineDiff.Mar < 0 ? 0 : lineDiff.Mar;
                        lineDiff.Apr = lineDiff.Apr < 0 ? 0 : lineDiff.Apr;
                        lineDiff.May = lineDiff.May < 0 ? 0 : lineDiff.May;
                        lineDiff.Jun = lineDiff.Jun < 0 ? 0 : lineDiff.Jun;
                        lineDiff.Jul = lineDiff.Jul < 0 ? 0 : lineDiff.Jul;
                        lineDiff.Aug = lineDiff.Aug < 0 ? 0 : lineDiff.Aug;
                        lineDiff.Sep = lineDiff.Sep < 0 ? 0 : lineDiff.Sep;
                        lineDiff.Oct = lineDiff.Oct < 0 ? 0 : lineDiff.Oct;
                        lineDiff.Nov = lineDiff.Nov < 0 ? 0 : lineDiff.Nov;
                        lineDiff.Dec = lineDiff.Dec < 0 ? 0 : lineDiff.Dec;

                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().Month = lineDiff;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().ParentMonth = lineDiff;

                        double allocated = l.Allocated - lines.Sum(l1 => l1.Allocated);
                        allocated = allocated < 0 ? 0 : allocated;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().Allocated = allocated;
                    }
                    else
                    {
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().Month = l.Month;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().ParentMonth = l.Month;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().Allocated = l.Allocated < 0 ? 0 : l.Allocated;
                    }
                }
            }
            return model;
        }

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

        #region "Add Actual"

        /// <summary>
        /// Action method to return AddActual view
        /// </summary>
        /// <returns>returns AddActual view</returns>
        public ActionResult AddActual(int Planid, bool isGridView = false) // Added by Komal Rawal for 2013 to identify grid view
        {
            HomePlanModel planmodel = new Models.HomePlanModel();

            try
            {
                //Added by Rahul Shah  on 05/08/2016 for PL #2461. display Plan Menu when user click Add Actual on report tab.
                var AppId = Sessions.User.UserApplicationId.Where(o => o.ApplicationTitle == Enums.ApplicationCode.MRP.ToString()).Select(o => o.ApplicationId).FirstOrDefault();
                Sessions.AppMenus = objBDSServiceClient.GetMenu(AppId, Sessions.User.RoleId);
                Sessions.RolePermission = objBDSServiceClient.GetPermission(AppId, Sessions.User.RoleId);
                if (Sessions.AppMenus != null)
                {
                    var isAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ModelCreateEdit);
                    var item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.Model.ToString().ToUpper());
                    if (item != null && !isAuthorized)
                    {
                        Sessions.AppMenus.Remove(item);
                    }

                    isAuthorized = (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostImprovementTacticCreateEdit) ||
                        AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostBestInClassNumberEdit));
                    item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.Boost.ToString().ToUpper());
                    if (item != null && !isAuthorized)
                    {
                        Sessions.AppMenus.Remove(item);
                    }

                    isAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ReportView);
                    item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.Report.ToString().ToUpper());
                    if (item != null && !isAuthorized)
                    {
                        Sessions.AppMenus.Remove(item);
                    }

                    isAuthorized = (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit) ||
                          AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView) ||
                          AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit) ||
                          AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView));

                    item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.MarketingBudget.ToString().ToUpper());
                    if (item != null && !isAuthorized)
                    {
                        Sessions.AppMenus.Remove(item);
                    }

                    isAuthorized = Sessions.AppMenus.Select(x => x.Code.ToLower()).Contains(Enums.ActiveMenu.Plan.ToString().ToLower());
                    item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.Finance.ToString().ToUpper());
                    if (item != null && !isAuthorized)
                    {
                        Sessions.AppMenus.Remove(item);
                    }
                }

                ViewBag.GridView = isGridView; // Added by Komal Rawal for 2013 to identify grid view
                //var LastSetOfViews = Common.PlanUserSavedViews;
                //var Label = Enums.FilterLabel.Plan.ToString();
                //var SetOfPlanSelected = LastSetOfViews.Where(view => view.FilterName == Label && view.Userid == Sessions.User.ID).ToList();
                //var SavedPlanIds = SetOfPlanSelected.Select(view => view.FilterValues).ToList();
                //if (SavedPlanIds != null && SavedPlanIds.Count > 0)
                //{
                //    ViewBag.LastSavedPlanIDs = String.Join(",", SavedPlanIds);
                //}
                //else
                //{

                //    ViewBag.LastSavedPlanIDs = null;
                //}
                //List<string> tacticStatus = Common.GetStatusListAfterApproved();
                //// Tthis is inititalized as 0 bcoz to get the status for tactics.
                //string planGanttType = PlanGanttTypes.Tactic.ToString();
                ViewBag.AddActualFlag = true;     // Added by Arpita Soni on 01/17/2015 for Ticket #1090 
                //List<RevenuePlanner.BDSService.User> lstIndividuals = GetIndividualsByPlanId(Planid.ToString(), planGanttType, Enums.ActiveMenu.Plan.ToString(), true);
                ////Start - Modified by Mitesh Vaishnav for PL ticket 972 - Add Actuals - Filter section formatting

                //// Fetch individual's records distinct
                //planmodel.objIndividuals = lstIndividuals.Select(user => new
                //{
                //    UserId = user.UserId,
                //    FirstName = user.FirstName,
                //    LastName = user.LastName
                //}).ToList().Distinct().Select(user => new RevenuePlanner.BDSService.User()
                //{
                //    UserId = user.UserId,
                //    FirstName = user.FirstName,
                //    LastName = user.LastName
                //}).ToList().OrderBy(user => string.Format("{0} {1}", user.FirstName, user.LastName)).ToList();
                ////End - Modified by Mitesh Vaishnav for PL ticket 972 - Add Actuals - Filter section formatting

                //List<TacticType> objTacticType = new List<TacticType>();

                ////// Modified By: Maninder Singh for TFS Bug#282: Extra Tactics Displaying in Add Actual Screen
                //objTacticType = (from tactic in db.Plan_Campaign_Program_Tactic
                //                 where tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == Planid && tacticStatus.Contains(tactic.Status) && tactic.IsDeleted == false
                //                 select tactic.TacticType).Distinct().OrderBy(tactic => tactic.Title).ToList();

                //ViewBag.TacticTypeList = objTacticType;

                //// Added by Dharmraj Mangukiya to implement custom restrictions PL ticket #537
                //// Get current user permission for edit own and subordinates plans.
                List<int> lstOwnAndSubOrdinates = new List<int>();
                try
                {
                    lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.ID);
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
                        return RedirectToAction("ServiceUnavailable", "Login");
                    }
                }

                bool IsPlanEditable = false;
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                var objPlan = db.Plans.FirstOrDefault(plan => plan.PlanId == Planid);
                var IsQuarter = objPlan.Year;
                //// Added by Dharmraj for #712 Edit Own and Subordinate Plan
                if (objPlan.CreatedBy.Equals(Sessions.User.ID))
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditAllAuthorized)
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditSubordinatesAuthorized)
                {
                    if (lstOwnAndSubOrdinates.Contains(objPlan.CreatedBy))
                    {
                        IsPlanEditable = true;
                    }
                }

                ViewBag.IsPlanEditable = IsPlanEditable;
                ViewBag.IsNewPlanEnable = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
                ViewBag.PlanId = Planid;
                ViewBag.IsQuarter = IsQuarter;
                //// Start - Added by Sohel Pathan on 22/01/2015 for PL ticket #1144
                List<CustomFieldsForFilter> lstCustomField = new List<CustomFieldsForFilter>();
                List<CustomFieldsForFilter> lstCustomFieldOption = new List<CustomFieldsForFilter>();

                //// Retrive Custom Fields and CustomFieldOptions list
                GetCustomFieldAndOptions(out lstCustomField, out lstCustomFieldOption);
                // 1765
                planmodel.SelectedPlanYear = objPlan.Year;
                // 1765
                planmodel.lstCustomFields = lstCustomField;
                planmodel.lstCustomFieldOptions = lstCustomFieldOption;
                planmodel.objplanhomemodelheader = Common.GetPlanHeaderValue(Planid);
                //// End - Added by Sohel Pathan on 22/01/2015 for PL ticket #1144
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
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
            }

            return View("AddActual", planmodel);
        }

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

        #endregion

        public PartialViewResult LoadImprovementGrid(int id)
        {
            PlanExchangeRate = Sessions.PlanExchangeRate;
            PlanImprovement objimprovement = new PlanImprovement();
            bool IsTacticExist = false;
            double TotalMqls = 0;
            double TotalCost = 0;
            //Added By komal rawal for #1432
            List<Plan_Campaign_Program> ProgramList = db.Plan_Campaign_Program.Where(pcp => pcp.Plan_Campaign.PlanId.Equals(id) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp).ToList();
            List<int> ProgramIds = ProgramList.Select(pcp => pcp.PlanProgramId).ToList();
            List<Plan_Campaign_Program_Tactic> TacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => ProgramIds.Contains(pcpt.PlanProgramId) && pcpt.IsDeleted.Equals(false)).Select(pcpt => pcpt).ToList();
            List<int> TacticIds = TacticList.Select(pcpt => pcpt.PlanTacticId).ToList();
            List<Plan_Campaign_Program_Tactic_LineItem> LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcptl => TacticIds.Contains(pcptl.PlanTacticId) && pcptl.IsDeleted.Equals(false)).Select(pcptl => pcptl).ToList();
            List<int> LineItemIds = LineItemList.Select(pcptl => pcptl.PlanLineItemId).ToList();

            if (TacticList != null && TacticList.Count > 0)
                IsTacticExist = true;
            objimprovement.IsTacticExists = IsTacticExist;
            var NoOfPrograms = ProgramIds.Count();

            //To Get Mql And Cost
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTactic => _imprvTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(id) && _imprvTactic.IsDeleted == false).Select(_imprvTactic => _imprvTactic).ToList();
            List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
            List<StageList> stageListType = Common.GetStageList();
            int? ModelId = db.Plans.Where(_plan => _plan.PlanId == id).Select(_plan => _plan.ModelId).FirstOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(mdl => mdl.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(mdl => mdl.ModelId == ModelId).Select(mdl => mdl).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(mdl => mdl.ModelId).ToList());

            var improvementTacticTypeIds = improvementActivities.Select(imptype => imptype.ImprovementTacticTypeId).ToList();

            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();

            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.CID && stage.IsDeleted == false).Select(stage => stage).ToList();

            List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelationForSinglePlan(TacticList, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, false);

            List<Plan_Tactic_Values> ListTacticMQLValue = (from tactic in TacticDataWithoutImprovement
                                                           select new Plan_Tactic_Values
                                                           {
                                                               PlanTacticId = tactic.TacticObj.PlanTacticId,
                                                               MQL = Math.Round(tactic.MQLValue, 0, MidpointRounding.AwayFromZero),
                                                               Revenue = objCurrency.GetValueByExchangeRate(tactic.RevenueValue, PlanExchangeRate)// Modified By Nishant Sheth // To Convert in selected currency rate
                                                           }).ToList();

            TotalMqls = ListTacticMQLValue.Sum(tactic => tactic.MQL);
            TotalCost = objCurrency.GetValueByExchangeRate(LineItemList.Sum(l => l.Cost), PlanExchangeRate); // Modified By Nishant Sheth // To Convert in selected currency rate
            objimprovement.TotalCost = TotalCost;
            objimprovement.TotalMqls = TotalMqls;
            objimprovement.Progrmas = NoOfPrograms;

            //End


            int improvementProgramId = db.Plan_Improvement_Campaign_Program.Where(prgrm => prgrm.Plan_Improvement_Campaign.ImprovePlanId == id).Select(prgrm => prgrm.ImprovementPlanProgramId).FirstOrDefault();
            if (improvementProgramId != 0)
            {
                objimprovement.ImprovementPlanProgramId = improvementProgramId;
            }
            else
            {
                objimprovement.ImprovementPlanProgramId = CreatePlanImprovementCampaignAndProgram();
                // ViewBag.ImprovementPlanProgramId = db.Plan_Improvement_Campaign_Program.Where(p => p.Plan_Improvement_Campaign.ImprovePlanId == id).Select(p => p.ImprovementPlanProgramId).FirstOrDefault();
            }
            return PartialView("_GridImprovement", objimprovement);
            //End
        }

        //Added by Devanshi gandhi for #1431

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
        public ActionResult GetHomeGridData(string planIds, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds, string viewBy)
        {
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();
            try
            {
                #region Set Permission
                EntityPermission objPermission = new EntityPermission();
                List<int> lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.ID);

                objPermission.PlanCreate = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
                objPermission.PlanEditAll = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                objPermission.PlanEditSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                #endregion
				
				 if (string.IsNullOrEmpty(viewBy))
                    viewBy = PlanGanttTypes.Tactic.ToString();

                ViewBag.CustomAttributOption = objcolumnview.GetCustomFiledOptionList(Sessions.User.CID);
                ViewBag.TacticTypelist = objGrid.GetTacticTypeListForHeader(planIds, Sessions.User.CID);
                ViewBag.LineItemTypelist = objGrid.GetLineItemTypeListForHeader(planIds, Sessions.User.CID);
                objPlanMainDHTMLXGrid = objGrid.GetPlanGrid(planIds, Sessions.User.CID, ownerIds, TacticTypeid, StatusIds, customFieldIds, Sessions.PlanCurrencySymbol, Sessions.PlanExchangeRate, Sessions.User.ID, objPermission, lstSubordinatesIds, viewBy);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            return PartialView("_HomeGrid", objPlanMainDHTMLXGrid);
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Get home grid data from cache memory 
        /// </summary>
        /// <returns></returns>
        public ActionResult GetHomeGridDataFromCache(string viewBy)
        {
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();
            try
            {
                objPlanMainDHTMLXGrid = objGrid.GetPlanGridDataFromCache(Sessions.User.CID, Sessions.User.ID, viewBy);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            return PartialView("_HomeGrid", objPlanMainDHTMLXGrid);
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
        public ActionResult SaveGridDetail(string UpdateType, string UpdateColumn, string UpdateVal, int id = 0, string CustomFieldInput = "", string ColumnType = "")
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
            
            try
            {
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

                                        return Json(new { IsExtended = true }, JsonRequestBehavior.AllowGet);
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
                        // Convert value from other currency to USD
                        if (!string.IsNullOrEmpty(UpdateVal))
                        {
                            UpdateVal = Convert.ToString(objCurrency.SetValueByExchangeRate(double.Parse(UpdateVal), PlanExchangeRate));
                        }
                        tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == id).ToList();
                        UpdateTacticPlannedCost(ref pcpobj, ref linkedTactic, ref totalLineitemCost, UpdateVal, tblTacticLineItem, linkedTacticId, yearDiff);
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
                            tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == id).ToList();
                            double Cost = tt.ProjectedRevenue != null && tt.ProjectedRevenue.HasValue ? tt.ProjectedRevenue.Value : 0;
                            UpdateTacticPlannedCost(ref pcpobj, ref linkedTactic, ref totalLineitemCost, Cost.ToString(), tblTacticLineItem, linkedTacticId, yearDiff);

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
                    else if(UpdateColumn.ToString().IndexOf("custom") >= 0) {
                        if (CustomFieldInput != null && CustomFieldInput != "") {
                            List<CustomFieldStageWeight> customFields = JsonConvert.DeserializeObject<List<CustomFieldStageWeight>>(CustomFieldInput); //Deserialize Json Data to List.
                            int CustomFieldId = customFields.Select(cust => cust.CustomFieldId).FirstOrDefault(); // Get Custom Field Id 
                            List<string> CustomfieldValue = customFields.Select(cust => cust.Value).ToList();// Get Custom Field Option Value

                            Dictionary<int ,string> CustomFieldOptionIds = new Dictionary<int, string>();
                            CustomFieldOptionIds = db.CustomFieldOptions.Where(log => log.CustomFieldId == CustomFieldId && CustomfieldValue.Contains(log.Value)).ToDictionary(log => log.CustomFieldOptionId, log => log.Value.ToString());// Get Key Value pair for Customfield option id and its value according to Value.

                            List<CustomField_Entity> prevCustomFieldList = db.CustomField_Entity.Where(custField => custField.EntityId == id && custField.CustomField.EntityType == UpdateType && custField.CustomFieldId == CustomFieldId).ToList();
                            prevCustomFieldList.ForEach(custField => db.Entry(custField).State = EntityState.Deleted);
                            if (customFields.Count != 0)
                            {
                                CustomField_Entity objcustomFieldEntity;                                
                                foreach (var item in customFields)
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
                                    objcustomFieldEntity.CreatedDate = DateTime.Now;
                                    objcustomFieldEntity.CreatedBy = Sessions.User.ID;
                                    objcustomFieldEntity.Weightage = (byte)item.Weight;
                                    objcustomFieldEntity.CostWeightage = (byte)item.CostWeight;
                                    db.CustomField_Entity.Add(objcustomFieldEntity);
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
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.ID;
                                        objcustomFieldEntity.Weightage = (byte)item.Weight;
                                        objcustomFieldEntity.CostWeightage = (byte)item.CostWeight;
                                        db.CustomField_Entity.Add(objcustomFieldEntity);
                                    }
                                }
                            }
                            db.SaveChanges();
                        }
                    }
                    if (linkedTacticId > 0)
                    {
                        linkedTactic.ModifiedBy = Sessions.User.ID;
                        linkedTactic.ModifiedDate = DateTime.Now;
                        db.Entry(linkedTactic).State = EntityState.Modified;
                    }
                    pcpobj.ModifiedBy = Sessions.User.ID;
                    pcpobj.ModifiedDate = DateTime.Now;

                    db.Entry(pcpobj).State = EntityState.Modified;
                    db.SaveChanges();
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
                        //// Calculate TotalLineItem cost.
                        if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.PlannedCost.ToString() || UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.TacticType.ToString())
                        {
                            Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                            if (objOtherLineItem == null)
                            {
                                Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                objNewLineitem.PlanTacticId = pcpobj.PlanTacticId;
                                // Modified by Arpita Soni for Ticket #2634 on 09/22/2016
                                objNewLineitem.Title = Common.LineItemTitleDefault;
                                if (pcpobj.Cost > totalLineitemCost)
                                {
                                    objNewLineitem.Cost = pcpobj.Cost - totalLineitemCost;
                                }
                                else
                                {
                                    objNewLineitem.Cost = 0;
                                }
                                objNewLineitem.Description = string.Empty;
                                objNewLineitem.CreatedBy = Sessions.User.ID;
                                objNewLineitem.CreatedDate = DateTime.Now;
                                db.Entry(objNewLineitem).State = EntityState.Added;
                                db.SaveChanges();
                                if (linkedTacticId > 0)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem objNewLinkedLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLinkedLineitem.PlanTacticId = linkedTacticId;
                                    objNewLinkedLineitem.Title = objNewLineitem.Title;
                                    objNewLinkedLineitem.Cost = objNewLineitem.Cost;
                                    objNewLinkedLineitem.Description = string.Empty;
                                    objNewLinkedLineitem.CreatedBy = Sessions.User.ID;
                                    objNewLinkedLineitem.CreatedDate = DateTime.Now;
                                    objNewLinkedLineitem.LinkedLineItemId = objNewLineitem.PlanLineItemId;
                                    db.Entry(objNewLinkedLineitem).State = EntityState.Added;
                                    db.SaveChanges();

                                    objNewLineitem.LinkedLineItemId = objNewLinkedLineitem.PlanLineItemId;
                                    db.Entry(objNewLineitem).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                objOtherLineItem.IsDeleted = false;
                                if (pcpobj.Cost > totalLineitemCost)
                                {
                                    objOtherLineItem.Cost = pcpobj.Cost - totalLineitemCost;
                                    //Added By Rahul Shah on 16/10/2015 for PL 1559
                                    otherLineItemCost = objOtherLineItem.Cost;
                                }
                                else
                                {
                                    objOtherLineItem.Cost = 0;
                                    objOtherLineItem.IsDeleted = true;
                                }
                                db.Entry(objOtherLineItem).State = EntityState.Modified;

                                #region "Updte linked other lineItem"
                                if (linkedTacticId > 0)
                                {
                                    List<Plan_Campaign_Program_Tactic_LineItem> tblLinkedTacticLineItem = new List<Plan_Campaign_Program_Tactic_LineItem>();
                                    //double totalLineitemCost = 0;
                                    tblLinkedTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == linkedTacticId).ToList();

                                    List<Plan_Campaign_Program_Tactic_LineItem> objtotalLinkedLineitemCost = tblLinkedTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                                    //objtotalLinkedLineitemCost
                                    double totalLinkedLineitemCost = 0;
                                    if (objtotalLinkedLineitemCost != null && objtotalLinkedLineitemCost.Count() > 0)
                                        totalLinkedLineitemCost = objtotalLinkedLineitemCost.Sum(l => l.Cost);

                                    Plan_Campaign_Program_Tactic_LineItem objLinkedOtherLineItem = tblLinkedTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);

                                    objLinkedOtherLineItem.IsDeleted = false;
                                    if (linkedTactic.Cost > totalLineitemCost)
                                    {
                                        objLinkedOtherLineItem.Cost = linkedTactic.Cost - totalLineitemCost;
                                    }
                                    else
                                    {
                                        objLinkedOtherLineItem.Cost = 0;
                                        objLinkedOtherLineItem.IsDeleted = true;
                                    }
                                    db.Entry(objLinkedOtherLineItem).State = EntityState.Modified;
                                }
                                #endregion
                            }
                            db.SaveChanges();
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


                    return Json(new { lineItemCost = totalLineitemCost, OtherLineItemCost = otherLineItemCost, OwnerName = OwnerName, TacticCost = tacticCost }, JsonRequestBehavior.AllowGet);
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

                            Dictionary<int, string> CustomFieldOptionIds = new Dictionary<int, string>();
                            CustomFieldOptionIds = db.CustomFieldOptions.Where(log => log.CustomFieldId == CustomFieldId && CustomfieldValue.Contains(log.Value)).ToDictionary(log => log.CustomFieldOptionId, log => log.Value.ToString()); // Get Key Value pair for Customfield option id and its value according to Value.

                            List<CustomField_Entity> prevCustomFieldList = db.CustomField_Entity.Where(custField => custField.EntityId == id && custField.CustomField.EntityType == UpdateType && custField.CustomFieldId == CustomFieldId).ToList(); 
                            prevCustomFieldList.ForEach(custField => db.Entry(custField).State = EntityState.Deleted);
                            if (customFields.Count != 0)
                            {
                                CustomField_Entity objcustomFieldEntity;
                                foreach (var item in customFields)
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
                                    objcustomFieldEntity.CreatedDate = DateTime.Now;
                                    objcustomFieldEntity.CreatedBy = Sessions.User.ID;
                                    objcustomFieldEntity.Weightage = (byte)item.Weight;
                                    objcustomFieldEntity.CostWeightage = (byte)item.CostWeight;
                                    db.CustomField_Entity.Add(objcustomFieldEntity);
                                }
                            }
                            db.SaveChanges();
                        }
                    }
                    return Json(new { OwnerName = OwnerName }, JsonRequestBehavior.AllowGet);
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

                            List<CustomField_Entity> prevCustomFieldList = db.CustomField_Entity.Where(custField => custField.EntityId == id && custField.CustomField.EntityType == UpdateType && custField.CustomFieldId == CustomFieldId).ToList();
                            prevCustomFieldList.ForEach(custField => db.Entry(custField).State = EntityState.Deleted);
                            if (customFields.Count != 0)
                            {
                                CustomField_Entity objcustomFieldEntity;
                                foreach (var item in customFields)
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
                                    objcustomFieldEntity.CreatedDate = DateTime.Now;
                                    objcustomFieldEntity.CreatedBy = Sessions.User.ID;
                                    objcustomFieldEntity.Weightage = (byte)item.Weight;
                                    objcustomFieldEntity.CostWeightage = (byte)item.CostWeight;
                                    db.CustomField_Entity.Add(objcustomFieldEntity);
                                }
                            }
                            db.SaveChanges();
                        }
                    }
                    return Json(new { OwnerName = OwnerName }, JsonRequestBehavior.AllowGet);
                }

                #endregion
                #region update LineItem Detail
                if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.lineitem.ToString())
                {
                    Plan_Campaign_Program_Tactic_LineItem objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobjw => pcpobjw.PlanLineItemId.Equals(id));
                    oldOwnerId = objLineitem.CreatedBy; //Added by Rahul Shah on 17/03/2016 for PL #2068
                    var objTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == objLineitem.PlanTacticId);
                    var LinkedTacticId = objTactic.LinkedTacticId;
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
                    }
                    else if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                    {
                        objLineitem.CreatedBy = Convert.ToInt32(UpdateVal);
                    }

                    objLineitem.ModifiedBy = Sessions.User.ID;
                    objLineitem.ModifiedDate = DateTime.Now;
                    db.Entry(objLineitem).State = EntityState.Modified;

                    #region "Update linked lineItem ModifiedBy & ModifiedDate"
                    if (linkedLineItemId > 0)
                    {
                        linkedLineItem.ModifiedBy = Sessions.User.ID;
                        linkedLineItem.ModifiedDate = DateTime.Now;
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

                    //Added by Rahul Shah on 17/03/2016 for PL #2068
                    if (result > 0)
                    {
                        if (UpdateColumn == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString())
                            SendEmailnotification(objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId, id, oldOwnerId, Convert.ToInt32(UpdateVal), objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Title.ToString(), objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Title.ToString(), objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Title.ToString(), objLineitem.Plan_Campaign_Program_Tactic.Title.ToString(), Enums.Section.LineItem.ToString().ToLower(), objLineitem.Title.ToString(), UpdateColumn);
                    }
                    
                    // Modified by Arpita Soni for Ticket #2634 on 09/22/2016
                    objPlanTactic.UpdateBalanceLineItemCost(objTactic.PlanTacticId);

                    return Json(new { tacticCost = objTactic.Cost }, JsonRequestBehavior.AllowGet);
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

        public void ReduceTacticPlannedCost(ref Plan_Campaign_Program_Tactic objTactic, ref Plan_Campaign_Program_Tactic ObjLinkedTactic, ref List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist, double lineitem_diffCost)
        {
            try
            {
                #region "Reduce Tactic cost while reduceing line item cost"
                //var diffcost = pcpobj.Cost - Convert.ToDouble(UpdateVal);
                double diffLinkCost, setTacCost, tacDiffCost;
                diffLinkCost = setTacCost = tacDiffCost = lineitem_diffCost;
                int endmonth = 24;
                while (tacDiffCost > 0 && endmonth != 0)
                {
                    if (objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                    {
                        double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + endmonth).Sum(lineitem => lineitem.Value);
                        double objtacticcost = objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                        var DiffMonthCost = objtacticcost - tacticlineitemcostmonth;
                        if (DiffMonthCost > 0)
                        {
                            if (DiffMonthCost > tacDiffCost)
                            {
                                objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - tacDiffCost;
                                tacDiffCost = 0;
                            }
                            else
                            {
                                objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - DiffMonthCost;
                                tacDiffCost = tacDiffCost - DiffMonthCost;
                            }
                        }
                    }
                    if (ObjLinkedTactic != null && ObjLinkedTactic.LinkedTacticId > 0)
                    {
                        if (ObjLinkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                        {
                            double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + endmonth).Sum(lineitem => lineitem.Value);
                            double objtacticcost = ObjLinkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                            var DiffMonthCost = objtacticcost - tacticlineitemcostmonth;
                            if (DiffMonthCost > 0)
                            {
                                if (DiffMonthCost > diffLinkCost)
                                {
                                    ObjLinkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - diffLinkCost;
                                    diffLinkCost = 0;
                                }
                                else
                                {
                                    ObjLinkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - DiffMonthCost;
                                    diffLinkCost = diffLinkCost - DiffMonthCost;
                                }
                            }
                        }
                    }
                    if (endmonth > 0)
                    {
                        endmonth -= 1;
                    }

                }

                objTactic.Cost = (objTactic.Cost) - setTacCost;
                if (ObjLinkedTactic != null && ObjLinkedTactic.LinkedTacticId > 0)
                {
                    ObjLinkedTactic.Cost = (ObjLinkedTactic.Cost) - diffLinkCost;
                    db.Entry(ObjLinkedTactic).State = EntityState.Modified;
                }

                db.Entry(objTactic).State = EntityState.Modified;
                db.SaveChanges();
                #endregion
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        #endregion

        public void UpdateTacticPlannedCost(ref Plan_Campaign_Program_Tactic pcpobj, ref Plan_Campaign_Program_Tactic linkedTactic, ref double totalLineitemCost, string UpdateVal, List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem, int linkedTacticId = 0, int yearDiff = 0)
        {

            try
            {
                if (tblTacticLineItem == null)
                    tblTacticLineItem = new List<Plan_Campaign_Program_Tactic_LineItem>();
                if (pcpobj == null)
                    return;
                int id = pcpobj.PlanTacticId;
                if ((db.Plan_Campaign_Program_Tactic_Cost.Where(_tacCost => _tacCost.PlanTacticId == id).ToList()).Count() == 0 ||
                                    pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower()
                                    || pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                {
                    pcpobj.Cost = Convert.ToDouble(UpdateVal);
                }
                if (linkedTacticId > 0)
                {
                    linkedTactic.Cost = pcpobj.Cost;
                }


                List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();

                if (objtotalLineitemCost != null && objtotalLineitemCost.Count() > 0)
                    totalLineitemCost = objtotalLineitemCost.Sum(l => l.Cost);
                if (totalLineitemCost > Convert.ToDouble(UpdateVal))
                {
                    // Added by Viral Kadiya for Pl ticket #1970.
                    string strReduceTacticPlannedCostMessage = string.Format(Common.objCached.TacticPlanedCostReduce, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                    //return Json(new { IsError = true, errormsg = strReduceTacticPlannedCostMessage });
                    //UpdateVal = totalLineitemCost.ToString();
                }
                if (Convert.ToDouble(UpdateVal) > pcpobj.Cost)
                {
                    var diffcost = Convert.ToDouble(UpdateVal) - pcpobj.Cost;
                    int startmonth = pcpobj.StartDate.Month;

                    if (pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                    {
                        pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value += diffcost;
                    }
                    else
                    {
                        Plan_Campaign_Program_Tactic_Cost objTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                        objTacticCost.PlanTacticId = pcpobj.PlanTacticId;
                        objTacticCost.Period = PeriodChar + startmonth;
                        objTacticCost.Value = diffcost;
                        objTacticCost.CreatedBy = Sessions.User.ID;
                        objTacticCost.CreatedDate = DateTime.Now;
                        db.Entry(objTacticCost).State = EntityState.Added;
                    }
                    //Add linked Tactic TacticCost data
                    if (linkedTacticId > 0)
                    {
                        if (linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                        {
                            linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value += diffcost;
                        }
                        else
                        {
                            Plan_Campaign_Program_Tactic_Cost lnkTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                            lnkTacticCost.PlanTacticId = linkedTacticId;
                            lnkTacticCost.Period = PeriodChar + startmonth;
                            lnkTacticCost.Value = diffcost;
                            lnkTacticCost.CreatedBy = Sessions.User.ID;
                            lnkTacticCost.CreatedDate = DateTime.Now;
                            db.Entry(lnkTacticCost).State = EntityState.Added;
                        }
                    }
                }
                else if (Convert.ToDouble(UpdateVal) < pcpobj.Cost)
                {
                    var diffcost = pcpobj.Cost - Convert.ToDouble(UpdateVal);
                    double diffLinkCost = diffcost;
                    int endmonth = 12 * (yearDiff + 1);
                    while (diffcost > 0 && endmonth != 0)
                    {
                        if (pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                        {
                            double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + endmonth).Sum(lineitem => lineitem.Value);
                            double objtacticcost = pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                            var DiffMonthCost = objtacticcost - tacticlineitemcostmonth;
                            if (DiffMonthCost > 0)
                            {
                                if (DiffMonthCost > diffcost)
                                {
                                    pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - diffcost;
                                    diffcost = 0;
                                }
                                else
                                {
                                    pcpobj.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - DiffMonthCost;
                                    diffcost = diffcost - DiffMonthCost;
                                }
                            }
                        }
                        if (linkedTacticId > 0)
                        {
                            if (linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).Any())
                            {
                                double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == PeriodChar + endmonth).Sum(lineitem => lineitem.Value);
                                double objtacticcost = linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value;
                                var DiffMonthCost = objtacticcost - tacticlineitemcostmonth;
                                if (DiffMonthCost > 0)
                                {
                                    if (DiffMonthCost > diffLinkCost)
                                    {
                                        linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - diffLinkCost;
                                        diffLinkCost = 0;
                                    }
                                    else
                                    {
                                        linkedTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == PeriodChar + endmonth).FirstOrDefault().Value = objtacticcost - DiffMonthCost;
                                        diffLinkCost = diffLinkCost - DiffMonthCost;
                                    }
                                }
                            }
                        }
                        if (endmonth > 0)
                        {
                            endmonth -= 1;
                        }

                    }

                }

                pcpobj.Cost = Convert.ToDouble(UpdateVal);
                // update linked tactic cost value.
                if (linkedTacticId > 0)
                {
                    yearDiff = linkedTactic.EndDate.Year - linkedTactic.StartDate.Year;
                    //linkedTactic.Cost = pcpobj.Cost;
                    List<Plan_Campaign_Program_Tactic_Cost> lstLinkeTac = new List<Plan_Campaign_Program_Tactic_Cost>();
                    if (yearDiff > 0) // is MultiYear Tactic
                    {
                        lstLinkeTac = db.Plan_Campaign_Program_Tactic_Cost.Where(per => per.PlanTacticId == linkedTacticId).ToList();
                        lstLinkeTac = lstLinkeTac.Where(per => per.PlanTacticId == linkedTacticId && Convert.ToInt32(per.Period.Replace(PeriodChar, string.Empty)) > 12).ToList();
                        //lstLinkeTac = lstLinkeTac.Where(per => per.PlanTacticId == linkedTacticId && int.Parse(per.Period.Replace(PeriodChar, string.Empty)) > 12).ToList();
                    }
                    else
                        lstLinkeTac = db.Plan_Campaign_Program_Tactic_Cost.Where(per => per.PlanTacticId == linkedTacticId).ToList();
                    if (lstLinkeTac != null && lstLinkeTac.Count > 0)
                    {
                        linkedTactic.Cost = lstLinkeTac.Sum(tac => tac.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

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
                int StageId = 0;
                if (StageID == null)
                    StageId = tacticdata.StageId;
                else
                    StageId = (int)StageID;

                int modelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).FirstOrDefault();

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

        #region "Get Data for Export to CSV with all Attribute"
        //public void GetDataforExportCSV()
        //{
        //    //double totalcost = 0;
        //    double totalmql = 0;
        //    double totalrevenue = 0;
        //    DataSet dsHomeGridDataCSV = new DataSet();

        //    SqlParameter[] para1 = new SqlParameter[1];

        //    para1[0] = new SqlParameter()
        //    {
        //        ParameterName = "PlanId",
        //        Value = Sessions.PlanPlanIds[0]
        //    };
        //    var dataList = db.Database.SqlQuery<Custom_CSV>("spGridDataList @PlanId", para1).ToList();
        //    var PlanList = dataList.Where(ids => ids.Section.ToString() == Enums.Section.Plan.ToString()).ToList();
        //    var FileName = PlanList.Select(file => file.Plan).FirstOrDefault();
        //    var CampaignList = dataList.Where(ids => ids.Section.ToString() == Enums.Section.Campaign.ToString()).ToList();

        //    List<Custom_CSV> Hdata = new List<Custom_CSV>();
        //    int modelId = dataList.Where(p => p.Section.ToString() == Enums.Section.Plan.ToString()).Select(mId => Convert.ToInt32(mId.ModelId)).FirstOrDefault();
        //    int PlanId = dataList.Where(p => p.Section.ToString() == Enums.Section.Plan.ToString()).Select(mId => Convert.ToInt32(mId.Id)).FirstOrDefault();
        //    var ProgramIds = dataList.Where(ids => ids.Section.ToString() == Enums.Section.Program.ToString()).Select(ids => ids.Id).ToList();
        //    var progTactic = db.Plan_Campaign_Program_Tactic.Where(_tactic => ProgramIds.Contains(_tactic.PlanProgramId) && _tactic.IsDeleted.Equals(false)).ToList();
        //    var TacticIds = progTactic.Select(ids => ids.PlanTacticId).ToList();
        //    stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.CID && stage.IsDeleted == false).Select(stage => stage).ToList();
        //    string MQLTitle = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(stage => stage.Title).FirstOrDefault();
        //    ViewBag.MQLTitle = MQLTitle;

        //    CalculateTacticCostRevenue(modelId, TacticIds, progTactic, PlanId);

        //    if (PlanList.Count > 0)
        //    {
        //        totalmql = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => TacticIds.Contains(l.PlanTacticId)).Sum(l => l.MQL) : 0;
        //        totalrevenue = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => TacticIds.Contains(l.PlanTacticId)).Sum(l => l.Revenue) : 0;
        //        var pl = PlanList.FirstOrDefault();
        //        pl.MQLS = totalmql;
        //        pl.Revenue = totalrevenue;
        //        pl.Owner = GetOwnerName(pl.CreatedBy.ToString());
        //        Hdata.Add(pl);
        //        if (CampaignList.Count > 0)
        //        {

        //            foreach (var campaignitem in CampaignList)
        //            {
        //                var mqlcamp = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => campaignitem.Id == l.CampaignId).Sum(l => l.MQL) : 0;
        //                var revenuecamp = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => campaignitem.Id == l.CampaignId).Sum(l => l.Revenue) : 0;
        //                campaignitem.MQLS = mqlcamp;
        //                campaignitem.Revenue = revenuecamp;
        //                campaignitem.Owner = GetOwnerName(campaignitem.CreatedBy.ToString());
        //                Hdata.Add(campaignitem);

        //                var ProgramList = dataList.FindAll(x => x.ParentId == campaignitem.Id).ToList();

        //                if (ProgramList.Count > 0)
        //                {

        //                    foreach (var programitem in ProgramList)
        //                    {

        //                        var mqlprog = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => programitem.Id == l.Programid).Sum(l => l.MQL) : 0;
        //                        var revenueprog = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => programitem.Id == l.Programid).Sum(l => l.Revenue) : 0;
        //                        programitem.MQLS = mqlprog;
        //                        programitem.Revenue = revenueprog;
        //                        programitem.Owner = GetOwnerName(programitem.CreatedBy.ToString());
        //                        Hdata.Add(programitem);
        //                        var TacticList = dataList.FindAll(x => x.ParentId == programitem.Id).ToList();
        //                        if (TacticList.Count > 0)
        //                        {

        //                            foreach (var tacticitem in TacticList)
        //                            {
        //                                var mqltact = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => tacticitem.Id == l.PlanTacticId).Sum(l => l.MQL) : 0;
        //                                var revenuetact = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => tacticitem.Id == l.PlanTacticId).Sum(l => l.Revenue) : 0;
        //                                tacticitem.MQLS = mqltact;
        //                                tacticitem.Revenue = revenuetact;
        //                                tacticitem.Owner = GetOwnerName(tacticitem.CreatedBy.ToString());
        //                                Hdata.Add(tacticitem);
        //                                var LineItemList = dataList.FindAll(x => x.ParentId == tacticitem.Id).ToList();

        //                                if (LineItemList.Count > 0)
        //                                {
        //                                    foreach (var lineitem in LineItemList)
        //                                    {
        //                                        lineitem.Owner = GetOwnerName(lineitem.CreatedBy.ToString());
        //                                        Hdata.Add(lineitem);
        //                                    }
        //                                }
        //                            }
        //                        }

        //                    }
        //                }
        //            }


        //        }
        //    }


        //    SqlParameter[] para = new SqlParameter[2];

        //    para[0] = new SqlParameter()
        //    {
        //        ParameterName = "PlanId",
        //        Value = Sessions.PlanPlanIds[0]
        //    };
        //    para[1] = new SqlParameter()
        //    {
        //        ParameterName = "ClientId",
        //        Value = Sessions.User.CID
        //    };
        //    var customfieldlist = db.Database.SqlQuery<Custom_CustomFieldHeader>("spCustomfieldData @PlanId,@ClientId", para).ToList();

        //    //Build the CSV file data as a Comma separated string.
        //    string csv = string.Empty;


        //    //Add the Header row for CSV file.
        //    int colum = 0;
        //    foreach (var i in Hdata.FirstOrDefault().GetType().GetProperties())
        //    {
        //        switch (i.Name)
        //        {
        //            case "Plan":
        //            case "Campaign":
        //            case "Program":
        //            case "Tactic":
        //            case "LineItem":
        //            case "StartDate":
        //            case "EndDate":
        //            case "PlanCost":
        //            case "Type":
        //            case "Owner":
        //            case "Revenue":
        //            case "EloquaId":
        //            case "SFDCId":
        //                csv += i.Name + ',';
        //                colum += colum;
        //                break;
        //            case "TargetStageValue":
        //                csv += "Target Stage Value" + ',';
        //                break;
        //            case "MQLS":
        //                csv += MQLTitle + ',';
        //                break;
        //            case "ExternalName":
        //                csv += "External Name" + ',';
        //                break;
        //            default:
        //                break;
        //        }

        //    }
        //    Dictionary<string, string> Heads = new Dictionary<string, string>();
        //    string camp = string.Empty, prog = string.Empty, tact = string.Empty, line = string.Empty;
        //    foreach (var item in customfieldlist)
        //    {
        //        System.Collections.ArrayList newList = new System.Collections.ArrayList();
        //        System.Collections.ArrayList newHeadVal = new System.Collections.ArrayList();

        //        var vHead = item.Header.Split(',').ToArray();
        //        var vHval = item.Value.Split(',').ToArray();

        //        for (int i = 0; i < vHead.Length; i++)
        //        {
        //            if (!newList.Contains(vHead[i]))
        //            {
        //                newList.Add(vHead[i]);
        //                newHeadVal.Add(vHval[i]);
        //            }
        //            else
        //            {
        //                newHeadVal[newList.IndexOf(vHead[i])] += ';' + Convert.ToString(vHval[i]);
        //            }


        //        }

        //        item.Header = string.Join(",", newList.ToArray());
        //        item.Value = string.Join(",", newHeadVal.ToArray());

        //        foreach (var i in item.Header.Split(','))
        //        {
        //            if (!Heads.ContainsKey(i))
        //            {

        //                switch (item.EntityType)
        //                {
        //                    case "Campaign":
        //                        camp += i + ',';
        //                        break;
        //                    case "Program":
        //                        prog += i + ',';
        //                        break;
        //                    case "Tactic":
        //                        tact += i + ',';
        //                        break;
        //                    case "Lineitem":
        //                        line += i + ',';
        //                        break;
        //                    default:
        //                        break;
        //                }
        //                Heads.Add(i, item.EntityType);
        //                //csv += i + ',';

        //            }

        //        }
        //    }
        //    csv += camp + prog + tact + line;
        //    csv += "\r\n";
        //    //Rows
        //    foreach (var item in Hdata)
        //    {

        //        foreach (var j in item.GetType().GetProperties())
        //        {
        //            switch (j.Name)
        //            {
        //                case "Plan":
        //                case "Campaign":
        //                case "Program":
        //                case "Tactic":
        //                case "LineItem":
        //                case "StartDate":
        //                case "EndDate":
        //                case "PlanCost":
        //                case "Type":
        //                case "Owner":
        //                case "TargetStageValue":
        //                case "MQLS":
        //                case "ExternalName":
        //                case "EloquaId":
        //                case "SFDCId":
        //                    csv += j.GetValue(item) + ",";
        //                    break;
        //                case "Revenue":
        //                    csv += j.GetValue(item) == null ? j.GetValue(item) : "$" + j.GetValue(item) + ",";
        //                    break;
        //                case "Id":
        //                    if (customfieldlist.Find(t => t.EntityId == item.Id) != null)
        //                    {
        //                        if (customfieldlist.Find(t => t.EntityId == item.Id).EntityType == "Campaign")
        //                        {
        //                            string camVal = customfieldlist.Find(t => t.EntityId == item.Id).Value;
        //                            string camHead = customfieldlist.Find(t => t.EntityId == item.Id).Header;
        //                            if (camHead.Count(x => x == ',') == camp.Count(x => x == ','))
        //                                csv += camVal + "," + Regex.Replace(prog, "[a-zA-Z0-9 -]", "") + Regex.Replace(tact, "[a-zA-Z0-9 -]", "") + Regex.Replace(line, "[a-zA-Z0-9 -]", "");
        //                            else
        //                            {
        //                                csv += camVal + "," + Regex.Replace(prog, "[a-zA-Z0-9 -]", "") + Regex.Replace(tact, "[a-zA-Z0-9 -]", "") + Regex.Replace(line, "[a-zA-Z0-9 -]", "");

        //                            }
        //                        }
        //                        else if (customfieldlist.Find(t => t.EntityId == item.Id).EntityType == "Program")
        //                        {
        //                            csv += Regex.Replace(camp, "[a-zA-Z0-9 -]", "") + customfieldlist.Find(t => t.EntityId == item.Id).Value + "," + Regex.Replace(tact, "[a-zA-Z0-9 -]", "") + Regex.Replace(line, "[a-zA-Z0-9 -]", "");
        //                        }
        //                        else if (customfieldlist.Find(t => t.EntityId == item.Id).EntityType == "Tactic")
        //                        {
        //                            csv += Regex.Replace(camp, "[a-zA-Z0-9 -]", "") + Regex.Replace(prog, "[a-zA-Z0-9 -]", "") + customfieldlist.Find(t => t.EntityId == item.Id).Value + "," + Regex.Replace(line, "[a-zA-Z0-9 -]", "");
        //                        }
        //                        else if (customfieldlist.Find(t => t.EntityId == item.Id).EntityType == "Lineitem")
        //                        {
        //                            csv += Regex.Replace(camp, "[a-zA-Z0-9 -]", "") + Regex.Replace(prog, "[a-zA-Z0-9 -]", "") + Regex.Replace(tact, "[a-zA-Z0-9 -]", "") + customfieldlist.Find(t => t.EntityId == item.Id).Value;
        //                        }
        //                    }
        //                    break;
        //                default:
        //                    break;
        //            }

        //        }
        //        csv += "\r\n";
        //    }
        //    Response.ContentType = "Application/x-msexcel";
        //    Response.AddHeader("content-disposition", "attachment;filename=  " + FileName + ".csv");

        //    Response.Write(csv);
        //    Response.End();

        //}

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
            List<int> filterOwner = string.IsNullOrWhiteSpace(ownerIds) ? new List<int>() : ownerIds.Split(',').Select(owner => Convert.ToInt32(owner)).ToList();
            List<string> lstFilteredCustomFieldOptionIds = new List<string>();
            List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();

            DataTable dtTable = new DataTable();
            DataSet dsExportCsv = new DataSet();
            DataTable dtCSV = new DataTable();
            DataTable dtCSVCost = new DataTable();
            dsExportCsv = objSp.GetExportCSV(PlanId, HoneycombIds);
            dtCSV = dsExportCsv.Tables[0];
            dtCSVCost = dsExportCsv.Tables[0];

            //var useridslist = dtCSV.Rows.Cast<DataRow>().Select(x => int.Parse(x.Field<string>("CreatedBy"))).ToList();
            //string strContatedIndividualList = string.Join(",", useridslist.Select(tactic => tactic.ToString()));

            var listOfClientId = objBDSServiceClient.GetUserListByClientIdEx(Sessions.User.CID);
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
                    dtTable.Columns.Add(dtColums.Rows[i][0].ToString(), typeof(string));
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

        #region parent child hireachy combination
        // Add By Nishant Sheth

        //get the children of a specific item
        IEnumerable<DataRow> GetChildren(DataTable dataTable, Int32 parentId, string Section)
        {
            return dataTable
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<string>("ParentId") == Convert.ToString(parentId) && row.Field<string>("Section") == Section);
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
        #endregion

        #endregion

        /// <summary>
        /// Added by Rushil Bhuptani on 14/06/2016 for ticket #2227
        /// Import excel file data and update database with imported data.
        /// </summary>
        [HttpPost]
        //public ActionResult ExcelFileUpload()
        //{
        //    DataSet ds = new DataSet();
        //    DataTable dt = new DataTable();
        //    PlanExchangeRate = Sessions.PlanExchangeRate;
        //    try
        //    {
        //        if (Request.Files[0].ContentLength > 0)
        //        {
        //            string fileExtension =
        //                System.IO.Path.GetExtension(Request.Files[0].FileName);

        //            if (fileExtension == ".xls" || fileExtension == ".xlsx")
        //            {
        //                string fileLocation = Server.MapPath("~/Content/") + Request.Files[0].FileName;
        //                if (System.IO.File.Exists(fileLocation))
        //                {
        //                    GC.Collect();
        //                    //GC.WaitForPendingFinalizers(); 
        //                    System.IO.File.Delete(fileLocation);
        //                }
        //                Request.Files[0].SaveAs(fileLocation);
        //                string excelConnectionString = string.Empty;

        //                if (fileExtension == ".xls")
        //                {
        //                    excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
        //                                            fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\"";
        //                    IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(Request.Files[0].InputStream);
        //                    excelReader.IsFirstRowAsColumnNames = true;
        //                    //ds = GetXLS(excelConnectionString);
        //                    ds = excelReader.AsDataSet();
        //                    if (ds == null)
        //                    {
        //                        return Json(new { msg = "error", error = "Invalid data." }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    dt = ds.Tables[0];
        //                }
        //                else if (fileExtension == ".xlsx")
        //                {
        //                    dt = GetXLSX(fileLocation);
        //                    if (dt == null)
        //                    {
        //                        return Json(new { msg = "error", error = "Invalid data." }, JsonRequestBehavior.AllowGet);
        //                    }

        //                }

        //                if (dt.Rows.Count == 0 || dt.Rows[0][0] == DBNull.Value)
        //                {
        //                    return Json(new { msg = "error", error = "Invalid data." }, JsonRequestBehavior.AllowGet);
        //                }

        //                if (Convert.ToInt32(dt.Rows[0][0]) != Sessions.PlanId)
        //                {
        //                    return Json(new { msg = "error", error = "Data getting uploaded does not relate to specific plan." }, JsonRequestBehavior.AllowGet);
        //                }

        //                foreach (DataRow row in dt.Rows)
        //                {
        //                    if (string.IsNullOrEmpty(row["ActivityId"].ToString().Trim()))
        //                    {
        //                        return Json(new { msg = "error", error = "ActivityId must have a proper value." }, JsonRequestBehavior.AllowGet);
        //                    }
        //                }

        //                StoredProcedure objSp = new StoredProcedure();

        //                dt.Columns.RemoveAt(dt.Columns.Count - 1);
        //                if (dt.Rows[dt.Rows.Count - 1][0].ToString().Trim() ==
        //                    "This document was made with dhtmlx library. http://dhtmlx.com")
        //                {
        //                    dt.Rows.RemoveAt(dt.Rows.Count - 1);
        //                }

        //                for (int i = 0; i < dt.Columns.Count; i++)
        //                {
        //                    //insertation start 20/08/2016 #2504 Multi-Currency: used SetValueByExchangeRate method to update cell value
        //                    RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();
        //                    for (int j = 0; j < dt.Rows.Count; j++)
        //                    {
        //                        if (i > 1 && dt.Rows[j][i].ToString().Trim() != "")
        //                        {
        //                            dt.Rows[j][i] = dt.Rows[j][i].ToString().Replace(",", "").Replace("---", "");
        //                            if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[j][i])))
        //                            {
        //                                double value = 0;
        //                                double.TryParse(Convert.ToString(dt.Rows[j][i]), out value);
        //                                dt.Rows[j][i] = Convert.ToString(objCurrency.SetValueByExchangeRate(value, PlanExchangeRate));

        //                            }
        //                        }
        //                    }
        //                    //insertation End 20/08/2016 #2504 Multi-Currency: used SetValueByExchangeRate method to update cell value
        //                }
        //                dt.AcceptChanges();

        //                bool isMonthly = dt.Columns.Count > 7;

        //                var dataResponse = objSp.GetPlanBudgetList(dt, isMonthly, Sessions.User.ID);

        //                if (dataResponse == null)
        //                {
        //                    return Json(new { msg = "error", error = "Invalid data." }, JsonRequestBehavior.AllowGet);
        //                }

        //                // Added by Rushil Bhuptani on 21/06/2016 for ticket #2267 for showing message for conflicting data.
        //                if (dataResponse.Tables[0].Rows.Count > 0)
        //                {
        //                    return Json(new { conflict = true, message = "Tactics that were not part of exported file were not added or updated." }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message.Contains("process"))
        //        {
        //            return Json(new { msg = "error", error = "File is being used by another process." }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { msg = "error", error = "Invalid data." }, JsonRequestBehavior.AllowGet);
        //        }
        //    }

        //    return Json(new { conflict = false, message = "File imported successfully." }, JsonRequestBehavior.AllowGet); ;


        //}
        public ActionResult ExcelFileUpload()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
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
                        DataTable dtBudget = objcommonimportData.GetPlanBudgetDataByType(dt.Copy(), "budget", isMonthly);
                        //if type will be null then following message will be appear.
                        foreach (DataRow row in dtBudget.Rows)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(row["Type"]).Trim()))
                            {
                                return Json(new { msg = "error", error = Common.objCached.ImportValidation.Replace("{0}", "Type") }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        //Filter budget data table with  plan,tactic,lineitem,campaign and program.
                        dtBudget = dtBudget.AsEnumerable()
         .Where(row => row.Field<String>("type").ToLower() == "plan" || row.Field<String>("type").ToLower() == "tactic" || row.Field<String>("type").ToLower() == "campaign" || row.Field<String>("type").ToLower() == "program").CopyToDataTable();

                        //Filter actual data table with only plan,tactic and lineitem
                        DataTable dtActual = objcommonimportData.GetPlanBudgetDataByType(dt.Copy(), "actual", isMonthly);
                        dtActual = dtActual.AsEnumerable()
         .Where(row => row.Field<String>("type").ToLower() == "plan" || row.Field<String>("type").ToLower() == "tactic" || row.Field<String>("type").ToLower() == "lineitem").CopyToDataTable();

                        //Filter plan(cost) data table with only plan,tactic and lineitem
                        DataTable dtCost = objcommonimportData.GetPlanBudgetDataByType(dt.Copy(), "planned", isMonthly);
                        dtCost = dtCost.AsEnumerable()
         .Where(row => row.Field<String>("type").ToLower() == "plan" || row.Field<String>("type").ToLower() == "tactic" || row.Field<String>("type").ToLower() == "lineitem").CopyToDataTable();

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
                            return Json(new { conflict = true, message = Common.objCached.ActivityIdInvalid }, JsonRequestBehavior.AllowGet);
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

            return Json(new { conflict = false, message = Common.objCached.ImportSuccessMessage }, JsonRequestBehavior.AllowGet); ;

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
        public ActionResult GetBudgetData(string PlanIds,string ViewBy, string OwnerIds = "", string TactictypeIds = "", string StatusIds = "", string CustomFieldIds = "", string year = "")
        {
            IBudget Iobj = new RevenuePlanner.Services.Budget();
            int UserID = Sessions.User.ID;
            int ClientId = Sessions.User.CID;  
           if (string.IsNullOrEmpty(ViewBy))
            {
                ViewBy = PlanGanttTypes.Tactic.ToString();
            }
            BudgetDHTMLXGridModel budgetModel = Iobj.GetBudget(ClientId, UserID, PlanIds, PlanExchangeRate, ViewBy, year, CustomFieldIds, OwnerIds, TactictypeIds, StatusIds);
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


    }
}

﻿using Elmah;
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
using System.Globalization;
using Integration;
using System.Diagnostics;
using RestSharp.Contrib;
using System.Xml.Linq;
using System.Xml;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

/*
 * Added By :
 * Added Date :
 * Description : Plan related events and methos
 */

namespace RevenuePlanner.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class PlanController : CommonController
    {
        #region Variables

        private MRPEntities db = new MRPEntities();
        private BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();

        private DateTime CalendarStartDate;
        private DateTime CalendarEndDate;
        private string PeriodChar = "Y";
        public const string TacticCustomTitle = "TacticCustom";

        #endregion
        #region variable for load plan grid
        List<Plan_Tactic_Values> ListTacticMQLValue = new List<Plan_Tactic_Values>();

        List<Plan_Tactic_LineItem_Values> LineItemList = new List<Plan_Tactic_LineItem_Values>();
        List<Stage> stageList = new List<Stage>();
        List<User> lstUserDetails = new List<User>();
        List<int> lstCustomFieldsRequired = new List<int>();
    //    List<CustomField_Entity> tacticcustomfieldsentity = new List<CustomField_Entity>();
        List<TacticType> TacticTypeListForHC = new List<TacticType>();
        List<Plan_Campaign_Program_Tactic_LineItem> DBLineItemList = new List<Plan_Campaign_Program_Tactic_LineItem>();

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
        public ActionResult CreatePlan(int id = 0, bool isPlanSelecter = false)
        {
            /*Added by Mitesh Vaishnav on 25/07/2014 for PL ticket 619*/
            if (isPlanSelecter == true)
            {
                TempData["isPlanSelecter"] = true;
            }
            /*End :Added by Mitesh Vaishnav on 25/07/2014 for PL ticket 619*/
            // Added by dharmraj to check user activity permission
            bool IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            ViewBag.IsPlanCreateAuthorized = IsPlanCreateAuthorized;

            bool IsPlanCreateAll = false;


            try
            {
                if (id > 0)
                {
                    // Added by Dharmraj Mangukiya for edit authentication of plan, PL ticket #519
                    var objplan = db.Plans.FirstOrDefault(m => m.PlanId == id && m.IsDeleted == false);
                    bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                    bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                    //Get all subordinates of current user upto n level
                    var lstSubOrdinates = new List<Guid>();
                    try
                    {
                        lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
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
                        if (objplan.CreatedBy.Equals(Sessions.User.UserId))
                        {
                            IsPlanCreateAll = true;
                        }
                    }
                    if (id == 0 && !IsPlanCreateAuthorized)
                    {
                        return AuthorizeUserAttribute.RedirectToNoAccess();
                    }
                    if (objplan.CreatedBy.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
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

                var GoalTypeList = Common.GetGoalTypeList(Sessions.User.ClientId);
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
                    objPlanModel.GoalValue = Convert.ToString(objplan.GoalValue);
                    objPlanModel.AllocatedBy = objplan.AllocatedBy;
                    objPlanModel.Budget = objplan.Budget;
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
                TempData["allocatedByList"] = Common.GetAllocatedByList(); // Modified by Sohel Pathan on 05/08/2014
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
        public JsonResult SavePlan(PlanModel objPlanModel, string BudgetInputValues = "", string RedirectType = "", string UserId = "")
        {

            try
            {
                // Start - Added by Sohel Pathan on 07/08/2014 for PL ticket #672
                if (!string.IsNullOrEmpty(UserId))
                {
                    if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
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
                        plan.CreatedBy = Sessions.User.UserId;
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
                            plan.GoalValue = Convert.ToInt64(objPlanModel.GoalValue.Trim().Replace(",", "").Replace("$", ""));
                        }
                        else
                        {
                            plan.GoalValue = 0;
                        }
                        plan.AllocatedBy = objPlanModel.AllocatedBy;
                        plan.Budget = Convert.ToDouble(objPlanModel.Budget.ToString().Trim().Replace(",", "").Replace("$", ""));
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
                            plan.GoalValue = Convert.ToInt64(objPlanModel.GoalValue.Trim().Replace(",", "").Replace("$", ""));
                        }
                        else
                        {
                            plan.GoalValue = 0;
                        }
                        plan.AllocatedBy = objPlanModel.AllocatedBy;
                        plan.Description = objPlanModel.Description;    /* Added by Sohel Pathan on 04/08/2014 for PL ticket #623 */
                        plan.Budget = Convert.ToDouble(objPlanModel.Budget.ToString().Trim().Replace(",", "").Replace("$", ""));
                        plan.ModelId = objPlanModel.ModelId;
                        if (plan.Year != objPlanModel.Year) //// Added by Sohel Pathan on 12/01/2015 for PL ticket #1102
                        {
                            plan.Year = objPlanModel.Year;
                            Common.UpdatePlanYearOfActivities(objPlanModel.PlanId, Convert.ToInt32(objPlanModel.Year)); //// Added by Sohel Pathan on 12/01/2015 for PL ticket #1102
                        }
                        plan.ModifiedBy = Sessions.User.UserId;
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
                                        objPlanBudget.CreatedBy = Sessions.User.UserId;
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
                                        objPlanBudget.CreatedBy = Sessions.User.UserId;
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
                        Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                    }
                    else
                    {
                        Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
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
        public JsonResult SavePlanDefination(PlanModel objPlanModel, string UserId = "")
        {

            try
            {

                if (!string.IsNullOrEmpty(UserId))
                {
                    if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
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
                        string planDraftStatus = Enums.PlanStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.PlanStatus.Draft.ToString())).Value;
                        plan.Status = planDraftStatus;
                        plan.CreatedDate = System.DateTime.Now;
                        plan.CreatedBy = Sessions.User.UserId;
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
                            plan.GoalValue = Convert.ToInt64(objPlanModel.GoalValue.Trim().Replace(",", "").Replace("$", ""));
                        }
                        else
                        {
                            plan.GoalValue = 0;
                        }
                        plan.AllocatedBy = objPlanModel.AllocatedBy;
                        plan.Budget = Convert.ToDouble(objPlanModel.Budget.ToString().Trim().Replace(",", "").Replace("$", ""));
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
                            plan.GoalValue = Convert.ToInt64(objPlanModel.GoalValue.Trim().Replace(",", "").Replace("$", ""));
                        }
                        else
                        {
                            plan.GoalValue = 0;
                        }
                        plan.AllocatedBy = objPlanModel.AllocatedBy;
                        plan.Description = objPlanModel.Description;
                        plan.Budget = Convert.ToDouble(objPlanModel.Budget.ToString().Trim().Replace(",", "").Replace("$", ""));
                        plan.ModelId = objPlanModel.ModelId;
                        if (plan.Year != objPlanModel.Year)
                        {
                            plan.Year = objPlanModel.Year;
                            Common.UpdatePlanYearOfActivities(objPlanModel.PlanId, Convert.ToInt32(objPlanModel.Year));
                        }
                        plan.ModifiedBy = Sessions.User.UserId;
                        plan.ModifiedDate = System.DateTime.Now;
                        db.Entry(plan).State = EntityState.Modified;

                    }

                    int result = db.SaveChanges();

                    //// Insert Changelog.
                    if (objPlanModel.PlanId == 0)
                    {
                        Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                    }
                    else
                    {
                        Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                    }

                    if (result > 0)
                    {
                        Sessions.PlanId = plan.PlanId;
                        //Create default Plan Improvement Campaign, Program
                        int returnValue = CreatePlanImprovementCampaignAndProgram();
                    }
                    return Json(new { id = plan.PlanId, redirect = Url.Action("Index", "Home", new { activeMenu = Enums.ActiveMenu.Plan, currentPlanId = plan.PlanId, ismsg = "Plan Saved Successfully." }) });

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
                    if (goalValue != "" && Convert.ToInt64(goalValue) != 0)
                    {
                        isGoalValueExists = true;
                        objBudgetAllocationModel = Common.CalculateBudgetInputs(modelId, goalType, goalValue, ADS);
                    }

                    List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();

                    //// Set Input & Message based on GoalType value.
                    if (goalType.ToString().ToLower() == Enums.PlanGoalType.INQ.ToString().ToLower())
                    {
                        msg1 = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(stage => stage.Title.ToLower()).FirstOrDefault();
                        msg2 = " in revenue";
                        input1 = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.MQLValue.ToString();
                        input2 = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.RevenueValue.ToString();

                    }
                    else if (goalType.ToString().ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower())
                    {
                        msg1 = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.INQ.ToString().ToLower()).Select(stage => stage.Title.ToLower()).FirstOrDefault();
                        msg2 = " in revenue";
                        input1 = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.INQValue.ToString();
                        input2 = isGoalValueExists.Equals(false) ? "0" : objBudgetAllocationModel.RevenueValue.ToString();
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
            return Json(new { msg1 = msg1, msg2 = msg2, input1 = input1, input2 = input2, ADS = ADS }, JsonRequestBehavior.AllowGet);
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
                        tactic.ModifiedBy = Sessions.User.UserId;
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
                Guid clientId = Sessions.User.ClientId;
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
        public async Task<JsonResult> GetPlanByPlanID(int planid, string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "")
        {
            try
            {
                await Task.Delay(1);
                return Json(new
                {
                    lstHomePlanModelHeader = Common.GetPlanHeaderValue(planid, CustomFieldId, OwnerIds, TacticTypeids, StatusIds),
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

        /// <summary>
        /// Added By : Sohel Pathan
        /// Added Date : 22/09/2014
        /// Description : Get plan/home header data for multiple plans
        /// </summary>
        /// <param name="strPlanIds">Comma separated list of plan ids</param>
        /// <param name="activeMenu">Get Active Menu</param>
        /// <returns>returns Json object with values required to show in plan/home header</returns>
        public JsonResult GetPlanByMultiplePlanIDs(string planid, string activeMenu, string year, string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "")
        {
            planid = System.Web.HttpUtility.UrlDecode(planid);
            List<int> planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(p => int.Parse(p)).ToList();

            try
            {
                return Json(new
                {
                    lstHomePlanModelHeader = Common.GetPlanHeaderValueForMultiplePlans(planIds, activeMenu, year, CustomFieldId, OwnerIds, TacticTypeids, StatusIds),
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
                var lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
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
                    if ((objPlan.CreatedBy.Equals(Sessions.User.UserId)))
                    {
                        IsPlanCreateAll = true;
                    }
                }


                //// Check whether his own & SubOrdinate Plan editable or Not.
                if (objPlan.CreatedBy.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
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
            Guid bId = Sessions.User.ClientId;
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
                lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstPlanTacticId, false);
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
        public ActionResult PublishPlan(string UserId = "")
        {
            try
            {
                //// Check cross user login check
                if (!string.IsNullOrEmpty(UserId))
                {
                    if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                    {
                        TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                        return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                    }
                }

                //// Update Plan status to Published.
                var plan = db.Plans.FirstOrDefault(p => p.PlanId.Equals(Sessions.PlanId));
                plan.Status = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()];
                plan.ModifiedBy = Sessions.User.UserId;
                plan.ModifiedDate = DateTime.Now;

                int returnValue = db.SaveChanges();
                Common.InsertChangeLog(Sessions.PlanId, 0, Sessions.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.published);
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
                planCampaign.ModifiedBy = Sessions.User.UserId;
                planCampaign.ModifiedDate = DateTime.Now;

                //// Saving changes.
                returnValue = db.SaveChanges();
                if (isApproved)
                    Common.InsertChangeLog(Sessions.PlanId, 0, planCampaign.PlanCampaignId, planCampaign.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);

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
                planProgram.ModifiedBy = Sessions.User.UserId;
                planProgram.ModifiedDate = DateTime.Now;

                //// Saving changes.
                returnValue = db.SaveChanges();

                if (isApproved)
                    Common.InsertChangeLog(Sessions.PlanId, 0, planProgram.PlanProgramId, planProgram.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
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

                    var lstUserHierarchy = objBDSServiceClient.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
                    var lstSubordinates = lstUserHierarchy.Where(u => u.ManagerId == Sessions.User.UserId).ToList().Select(u => u.UserId).ToList();
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
                planTactic.ModifiedBy = Sessions.User.UserId;
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
                    Common.InsertChangeLog(Sessions.PlanId, 0, planTactic.PlanTacticId, planTactic.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
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
                    if (plan.CreatedBy.Equals(Sessions.User.UserId))
                    {
                        IsPlanCreateAll = true;
                    }
                }
                if (IsPlanEditSubordinatesAuthorized)
                {
                    //Get all subordinates of current user upto n level
                    var lstSubOrdinates = new List<Guid>();
                    try
                    {
                        lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
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
            var GoalTypeList = Common.GetGoalTypeList(Sessions.User.ClientId);
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
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();

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
                lstEditableTacticIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstPlanTacticIds, false);
                lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstPlanTacticIds, false);
            }

            //// Get Campaign data.
            var campaignobj = CampaignList.Select(p => new
            {
                id = p.PlanCampaignId,
                title = p.Title,
                description = p.Description,
                cost = LineItemList.Where(l => (TacticList.Where(pcpt => (ProgramList.Where(pcp => pcp.PlanCampaignId == p.PlanCampaignId).Select(pcp => pcp.PlanProgramId).ToList()).Contains(pcpt.PlanProgramId)).Select(pcpt => pcpt.PlanTacticId)).Contains(l.PlanTacticId)).Sum(l => l.Cost),
                mqls = ListTacticMQLValue.Where(lt => (TacticList.Where(pcpt => (ProgramList.Where(pcp => pcp.PlanCampaignId == p.PlanCampaignId).Select(pcp => pcp.PlanProgramId).ToList()).Contains(pcpt.PlanProgramId)).Select(pcpt => pcpt.PlanTacticId)).Contains(lt.PlanTacticId)).Sum(lt => lt.MQL),
                isOwner = Sessions.User.UserId == p.CreatedBy ? 0 : 1,
                programs = (ProgramList.Where(pcp => pcp.PlanCampaignId.Equals(p.PlanCampaignId))).Select(pcpj => new
                {
                    id = pcpj.PlanProgramId,
                    title = pcpj.Title,
                    description = pcpj.Description,
                    cost = LineItemList.Where(l => (TacticList.Where(pcpt => pcpt.PlanProgramId == pcpj.PlanProgramId).Select(pcpt => pcpt.PlanTacticId)).Contains(l.PlanTacticId)).Sum(l => l.Cost),
                    mqls = ListTacticMQLValue.Where(lt => (TacticList.Where(pcpt => pcpt.PlanProgramId == pcpj.PlanProgramId).Select(pcpt => pcpt.PlanTacticId)).Contains(lt.PlanTacticId)).Sum(lt => lt.MQL),
                    isOwner = Sessions.User.UserId == pcpj.CreatedBy ? 0 : 1,
                    tactics = (TacticList.Where(pcpt => pcpt.PlanProgramId.Equals(pcpj.PlanProgramId) && lstAllowedEntityIds.Contains(pcpt.PlanTacticId)).Select(pcpt => pcpt).ToList()).Select(pcptj => new
                    {
                        id = pcptj.PlanTacticId,
                        title = pcptj.Title,
                        description = pcptj.Description,
                        cost = LineItemList.Where(l => l.PlanTacticId == pcptj.PlanTacticId).Sum(l => l.Cost),
                        mqls = ListTacticMQLValue.Where(lt => lt.PlanTacticId == pcptj.PlanTacticId).Sum(lt => lt.MQL),
                        isOwner = Sessions.User.UserId == pcptj.CreatedBy ? ((TacticIds.Count.Equals(0) || lstEditableTacticIds.Contains(pcptj.PlanTacticId)) ? 0 : 1) : 1,
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
        public ActionResult DeleteCampaign(int id = 0, bool RedirectType = false, string closedTask = null, string UserId = "", string CalledFromBudget = "")
        {
            //// Check whether UserId is loggined User or Not.
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
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pc.PlanCampaignId, pc.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
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
        public ActionResult DeleteProgram(int id = 0, bool RedirectType = false, string closedTask = null, string UserId = "", string CalledFromBudget = "")
        {
            //// Check whether UserId is loggined User or Not.
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
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pc.PlanProgramId, pc.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
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
        public ActionResult DeleteTactic(int id = 0, bool RedirectType = false, string closedTask = null, string UserId = "", string CalledFromBudget = "")
        {
            //// Check whether UserId is loggined User or Not.
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
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcpt.PlanTacticId, pcpt.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
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

            return Json(new
            {
                revenue = tt.ProjectedRevenue == null ? 0 : tt.ProjectedRevenue,
                IsDeployedToIntegration = tt.IsDeployedToIntegration,
                stageId = tt.StageId,
                stageTitle = tt.Stage.Title,
                TacticTypeName = tt.Title,
                projectedStageValue = tt.ProjectedStageValue == null ? 0 : tt.ProjectedStageValue
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Dharmraj Mangukiya.
        /// Action to Get month/Quarter wise cost Value Of line item.
        /// </summary>
        /// <param name="id">Plan line item Id.</param>
        /// <param name="tid">Tactic Id</param>
        /// <returns>Returns Json Result of line item cost allocation Value.</returns>
        public JsonResult GetCostAllocationLineItemData(int id, int tid)
        {
            try
            {
                //Reintialize the Monthly list for Actual Allocation object 
                List<string> MonthlyList = Common.lstMonthly;
                var objPlanLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(_line => _line.PlanLineItemId == id && _line.IsDeleted == false); // Modified by :- Sohel Pathan on 05/09/2014 for PL ticket #749

                //get planId from database.
                int planId = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == objPlanLineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
                var objPlan = db.Plans.FirstOrDefault(_plan => _plan.PlanId == planId && _plan.IsDeleted == false);   // Modified by :- Sohel Pathan on 05/09/2014 for PL ticket #749
                var objPlanTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(_tac => _tac.PlanTacticId == tid && _tac.IsDeleted == false);    // Modified by :- Sohel Pathan on 01/09/2014 for PL ticket #745

                //// Get LineItem Cost.
                var lstAllLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(line => line.PlanTacticId == tid && line.IsDeleted == false).ToList();
                var planLineItemId = lstAllLineItem.Select(line => line.PlanLineItemId);
                var lstLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lineCost => planLineItemId.Contains(lineCost.PlanLineItemId)).ToList()
                                                               .Select(lineCost => new
                                                               {
                                                                   lineCost.PlanLineItemBudgetId,
                                                                   lineCost.PlanLineItemId,
                                                                   lineCost.Period,
                                                                   lineCost.Value
                                                               }).ToList();

                var lstTacticCost = db.Plan_Campaign_Program_Tactic_Cost.Where(_tacCost => _tacCost.PlanTacticId == tid).ToList();

                //// Calculate Total LineItem Cost
                double totalLoneitemCost = lstAllLineItem.Where(line => line.LineItemTypeId != null && line.IsDeleted == false).ToList().Sum(line => line.Cost);
                double TacticCost = objPlanTactic.Cost;
                double diffCost = TacticCost - totalLoneitemCost;
                double otherLineItemCost = diffCost < 0 ? 0 : diffCost;

                //Added By : Kalpesh Sharma #697 08/26/2014
                //Fetch the Line items actual by LineItemId and give it to the object.
                var lstActualLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lnActual => planLineItemId.Contains(lnActual.PlanLineItemId)).ToList()
                                                              .Select(lnActual => new
                                                              {
                                                                  lnActual.PlanLineItemId,
                                                                  lnActual.Period,
                                                                  lnActual.Value
                                                              }).ToList();

                //Set the custom array for fecthed Line item Actual data .
                var ActualCostData = MonthlyList.Select(period => new
                {
                    periodTitle = period,
                    costValue = lstActualLineItemCost.FirstOrDefault(lnCost => lnCost.Period == period && lnCost.PlanLineItemId == id) == null ? "" : lstActualLineItemCost.FirstOrDefault(lnCost => lnCost.Period == period && lnCost.PlanLineItemId == id).Value.ToString()
                });

                // Start - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
                //// Set Quarterly Budget allocation data.
                if (objPlan.AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString())
                {
                    List<PlanBudgetAllocationValue> lstPlanBudgetAllocationValue = new List<PlanBudgetAllocationValue>();
                    string[] quarterPeriods = Common.quarterPeriods;
                    for (int i = 0; i < 12; i++)
                    {
                        if ((i + 1) % 3 == 0)
                        {
                            PlanBudgetAllocationValue objPlanBudgetAllocationValue = new PlanBudgetAllocationValue();
                            objPlanBudgetAllocationValue.periodTitle = PeriodChar + (i - 1).ToString();
                            objPlanBudgetAllocationValue.costValue = lstLineItemCost.Where(lnCost => quarterPeriods.Contains(lnCost.Period) && lnCost.PlanLineItemId == id).FirstOrDefault() == null ? "" : lstLineItemCost.Where(lnCost => quarterPeriods.Contains(lnCost.Period) && lnCost.PlanLineItemId == id).Select(lnCost => lnCost.Value).Sum().ToString();
                            objPlanBudgetAllocationValue.remainingMonthlyCost = (lstTacticCost.Where(tacCost => quarterPeriods.Contains(tacCost.Period)).FirstOrDefault() == null ? 0 : lstTacticCost.Where(tacCost => quarterPeriods.Contains(tacCost.Period)).Select(tacCost => tacCost.Value).Sum()) - (lstLineItemCost.Where(lnCost => quarterPeriods.Contains(lnCost.Period)).Sum(lnCost => lnCost.Value));

                            /// Add into return list
                            lstPlanBudgetAllocationValue.Add(objPlanBudgetAllocationValue);

                            quarterPeriods = new string[] { PeriodChar + (i + 2), PeriodChar + (i + 3), PeriodChar + (i + 4) };
                        }
                    }

                    var objBudgetAllocationData = new { costData = lstPlanBudgetAllocationValue, ActualCostData = ActualCostData, otherLineItemCost = otherLineItemCost };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                // End - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
                else
                {
                    var costData = MonthlyList.Select(period => new
                    {
                        periodTitle = period,
                        costValue = lstLineItemCost.FirstOrDefault(lnCost => lnCost.Period == period && lnCost.PlanLineItemId == id) == null ? "" : lstLineItemCost.FirstOrDefault(lnCost => lnCost.Period == period && lnCost.PlanLineItemId == id).Value.ToString(),
                        remainingMonthlyCost = (lstTacticCost.FirstOrDefault(tacCost => tacCost.Period == period) == null ? 0 : lstTacticCost.FirstOrDefault(tacCost => tacCost.Period == period).Value) - (lstLineItemCost.Where(lnCost => lnCost.Period == period).Sum(lnCost => lnCost.Value))
                    });

                    var objBudgetAllocationData = new { costData = costData, ActualCostData = ActualCostData, otherLineItemCost = otherLineItemCost };
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
        /// Added By Mitesh
        /// Action to delete line item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="RedirectType"></param>
        /// <param name="closedTask"></param>
        /// <param name="UserId"></param>
        /// <param name="CalledFromBudget"></param>
        /// <returns></returns>
        public ActionResult DeleteLineItem(int id = 0, bool RedirectType = false, string closedTask = null, string UserId = "", string CalledFromBudget = "")
        {
            //// Check whether UserId is loggined User or Not.
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
                                objNewLineitem.Title = Common.LineItemTitleDefault + pcptl.Plan_Campaign_Program_Tactic.Title;
                                if (pcptl.Plan_Campaign_Program_Tactic.Cost > totalLoneitemCost)
                                {
                                    objNewLineitem.Cost = pcptl.Plan_Campaign_Program_Tactic.Cost - totalLoneitemCost;
                                }
                                else
                                {
                                    objNewLineitem.Cost = 0;
                                }
                                objNewLineitem.Description = string.Empty;
                                objNewLineitem.CreatedBy = Sessions.User.UserId;
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
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcptl.PlanLineItemId, pcptl.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
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
                Guid clientId = Sessions.User.ClientId;

                // Get the list of plan, filtered by Business Unit and Year selected
                if (Int_Year > 0)
                {
                    var modelids = db.Models.Where(model => model.ClientId == clientId && model.IsDeleted == false).Select(model => model.ModelId).ToList();
                    objPlan = (from p in db.Plans
                               where modelids.Contains(p.ModelId) && p.IsDeleted == false && p.Year == str_Year
                               select p).OrderByDescending(p => p.ModifiedDate ?? p.CreatedDate).ThenBy(p => p.Title).ToList();
                }
                List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
                if (objPlan != null && objPlan.Count > 0)
                {
                    //Get all subordinates of current user upto n level
                    var lstOwnAndSubOrdinates = new List<Guid>();

                    try
                    {
                        lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
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
                        if (item.CreatedBy.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
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
            Guid clientId = Sessions.User.ClientId;
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
        public JsonResult DeletePlan(int PlanId, string UserId = "")
        {
            //// check whether UserId is currently loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
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
                    planImprovementTactic.ModifiedBy = Sessions.User.UserId;
                    planImprovementTactic.ModifiedDate = DateTime.Now;

                    //// Saving changes.
                    int returnValue = db.SaveChanges();

                    if (isApproved)
                    {
                        returnValue = Common.InsertChangeLog(planImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, 0, planImprovementTactic.ImprovementPlanTacticId, planImprovementTactic.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
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
                    picobj.CreatedBy = Sessions.User.UserId;
                    picobj.CreatedDate = DateTime.Now;
                    db.Entry(picobj).State = EntityState.Added;
                    int result = db.SaveChanges();
                    retVal = picobj.ImprovementPlanCampaignId;
                    if (retVal > 0)
                    {
                        //// Insert Program data to Plan_Improvement_Campaign_Program table.
                        Plan_Improvement_Campaign_Program pipobj = new Plan_Improvement_Campaign_Program();
                        pipobj.CreatedBy = Sessions.User.UserId;
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
            //Modified By Komal Rawal for #1432 to get improvement tactics according to current plan .
            var tactics = db.Plan_Improvement_Campaign_Program_Tactic.Where(pc => pc.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(PlanId) && pc.IsDeleted.Equals(false)).Select(pc => pc).ToList();
            var tacticobj = tactics.Select(_tac => new
            {
                id = _tac.ImprovementPlanTacticId,
                title = _tac.Title,
                cost = _tac.Cost,
                ImprovementProgramId = _tac.ImprovementPlanProgramId,
                isOwner = Sessions.User.UserId == _tac.CreatedBy ? 0 : 1,
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
                        returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcpt.ImprovementPlanTacticId, pcpt.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
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
                        returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcpt.ImprovementPlanTacticId, pcpt.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
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
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
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
                ADS = Math.Round(differenceDealSize),
                Velocity = Math.Round(differenceSV),
                Revenue = Math.Round(differenceProjectedRevenue, 1),
                Cost = Math.Round(improvedCost)
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
            var improvementTacticList = db.ImprovementTacticTypes.Where(_imprvTacType => _imprvTacType.IsDeployed == true && _imprvTacType.ClientId == Sessions.User.ClientId && _imprvTacType.IsDeleted.Equals(false)).ToList();       //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
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
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();

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
                    if (Sessions.User.UserId == improvementActivities.Where(ia => ia.ImprovementTacticTypeId == imptactic.ImprovementTacticTypeId).Select(ia => ia.CreatedBy).FirstOrDefault())
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
                Cost = si.Cost,
                ProjectedRevenueWithoutTactic = Math.Round(si.ProjectedRevenueWithoutTactic, 1),
                ProjectedRevenueWithTactic = Math.Round(si.ProjectedRevenueWithTactic, 1),
                ProjectedRevenueLift = Math.Round(si.ProjectedRevenueLift, 1),
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
                picpt.ImprovementPlanProgramId = improvementPlanProgramId;
                picpt.Title = db.ImprovementTacticTypes.Where(itactic => itactic.ImprovementTacticTypeId == improvementTacticTypeId && itactic.IsDeleted.Equals(false)).Select(itactic => itactic.Title).FirstOrDefault();     //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                picpt.ImprovementTacticTypeId = improvementTacticTypeId;
                picpt.Cost = db.ImprovementTacticTypes.Where(itactic => itactic.ImprovementTacticTypeId == improvementTacticTypeId && itactic.IsDeleted.Equals(false)).Select(itactic => itactic.Cost).FirstOrDefault();       //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                picpt.EffectiveDate = DateTime.Now.Date;
                picpt.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString();
                picpt.CreatedBy = Sessions.User.UserId;
                picpt.CreatedDate = DateTime.Now;
                db.Entry(picpt).State = EntityState.Added;
                var Title = picpt.Title;
                int result = db.SaveChanges();
                //// Insert change log entry.
                result = Common.InsertChangeLog(Sessions.PlanId, null, picpt.ImprovementPlanTacticId, picpt.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
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
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
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
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
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
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false && stage.Level != null && stage.Code != CW).ToList();
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
        public JsonResult DeleteSuggestedBoxImprovementTactic(string SuggestionIMPTacticIdList, string UserId = "")
        {
            //// Check whether UserId is loggined User or Not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
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
                            if (pcpt.CreatedBy == Sessions.User.UserId)
                            {
                                pcpt.IsDeleted = true;
                                db.Entry(pcpt).State = EntityState.Modified;
                                returnValue = db.SaveChanges();
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcpt.ImprovementPlanTacticId, pcpt.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
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
                                        objPlan_Budget.CreatedBy = Sessions.User.UserId;
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
                                        objPlan_Budget.CreatedBy = Sessions.User.UserId;
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
        public JsonResult SaveBudgetCell(string entityId, string section, string month, string inputs, bool isquarter)
        {
            Dictionary<string, string> monthList = new Dictionary<string, string>() { 
                {Enums.Months.January.ToString(), "Y1" },
                {Enums.Months.February.ToString(), "Y2" },
                {Enums.Months.March.ToString(), "Y3" },
                {Enums.Months.April.ToString(), "Y4" },
                {Enums.Months.May.ToString(), "Y5" },
                {Enums.Months.June.ToString(), "Y6" },
                {Enums.Months.July.ToString(), "Y7" },
                {Enums.Months.August.ToString(), "Y8" },
                {Enums.Months.September.ToString(), "Y9" },
                {Enums.Months.October.ToString(), "Y10" },
                {Enums.Months.November.ToString(), "Y11" },
                {Enums.Months.December.ToString(), "Y12" },
                {"Quarter 1", "Y1" },
                {"Quarter 2", "Y4" },
                {"Quarter 3", "Y7" },
                {"Quarter 4", "Y10" }
                };
            try
            {
                int EntityId = Convert.ToInt32(entityId);
                var popupValues = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(inputs);
                double monthlyBudget = popupValues.Where(popup => popup.Key == Common.MonthlyBudgetForEntity).Any() && popupValues.Where(popup => popup.Key == Common.MonthlyBudgetForEntity).FirstOrDefault().Value != "" ? Convert.ToDouble(popupValues.Where(popup => popup.Key == Common.MonthlyBudgetForEntity).FirstOrDefault().Value) : popupValues.Where(popup => popup.Key == Common.MonthlyBudgetForEntity).FirstOrDefault().Value == "" ? 0 : -1;
                double yearlyBudget = popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).Any() && popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).FirstOrDefault().Value != "" ? Convert.ToDouble(popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).FirstOrDefault().Value) : popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).FirstOrDefault().Value == "" ? 0 : -1;
                if (popupValues.Where(popup => popup.Key == Common.MonthlyBudgetForEntity).Any() && popupValues.Where(popup => popup.Key == Common.YearlyBudgetForEntity).Any())
                {
                    if (monthlyBudget >= 0 && yearlyBudget >= 0)
                    {
                        string period = monthList[month].ToString();
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
                                    objPlanBudget.CreatedBy = Sessions.User.UserId;
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
                                    objPlanBudget.CreatedBy = Sessions.User.UserId;
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
                                    objCampaignBudget.CreatedBy = Sessions.User.UserId;
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
                                    objCampaignBudget.CreatedBy = Sessions.User.UserId;
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
                                    objProgramBudget.CreatedBy = Sessions.User.UserId;
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
                                    objProgramBudget.CreatedBy = Sessions.User.UserId;
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
                                    objTacticBudget.CreatedBy = Sessions.User.UserId;
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
                                    objTacticBudget.CreatedBy = Sessions.User.UserId;
                                    objTacticBudget.CreatedDate = DateTime.Now;
                                    db.Entry(objTacticBudget).State = EntityState.Added;
                                }
                            }
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
                            objtacticbudget.CreatedBy = Sessions.User.UserId;
                            objtacticbudget.CreatedDate = DateTime.Now;
                            db.Entry(objtacticbudget).State = EntityState.Added;
                        }
                        objTactic.TacticBudget = yearlyBudget;
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
        public JsonResult SavePlannedCell(string entityId, string section, string month, string inputs, string tab, bool isquarter)
        {
            Dictionary<string, string> monthList = new Dictionary<string, string>() { 
                {Enums.Months.January.ToString(), "Y1" },
                {Enums.Months.February.ToString(), "Y2" },
                {Enums.Months.March.ToString(), "Y3" },
                {Enums.Months.April.ToString(), "Y4" },
                {Enums.Months.May.ToString(), "Y5" },
                {Enums.Months.June.ToString(), "Y6" },
                {Enums.Months.July.ToString(), "Y7" },
                {Enums.Months.August.ToString(), "Y8" },
                {Enums.Months.September.ToString(), "Y9" },
                {Enums.Months.October.ToString(), "Y10" },
                {Enums.Months.November.ToString(), "Y11" },
                {Enums.Months.December.ToString(), "Y12" },
                {"Quarter 1", "Y1" },
                {"Quarter 2", "Y4" },
                {"Quarter 3", "Y7" },
                {"Quarter 4", "Y10" }
                };
            try
            {
                int EntityId = Convert.ToInt32(entityId);
                var popupValues = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(inputs);

                double monthlycost = popupValues.Where(popup => popup.Key == Common.MonthlyCostForEntity).Any() && popupValues.Where(popup => popup.Key == Common.MonthlyCostForEntity).FirstOrDefault().Value != "" ? Convert.ToDouble(popupValues.Where(popup => popup.Key == Common.MonthlyCostForEntity).FirstOrDefault().Value) : popupValues.Where(popup => popup.Key == Common.MonthlyCostForEntity).FirstOrDefault().Value == "" ? 0 : -1;
                double yearlycost = popupValues.Where(popup => popup.Key == Common.YearlyCostForEntity).Any() && popupValues.Where(popup => popup.Key == Common.YearlyCostForEntity).FirstOrDefault().Value != "" ? Convert.ToDouble(popupValues.Where(popup => popup.Key == Common.YearlyCostForEntity).FirstOrDefault().Value) : popupValues.Where(popup => popup.Key == Common.YearlyCostForEntity).FirstOrDefault().Value == "" ? 0 : -1;
                if (popupValues.Where(popup => popup.Key == Common.MonthlyCostForEntity).Any())
                {
                    string period = monthList[month].ToString();
                    if (string.Compare(section, Enums.Section.Tactic.ToString(), true) == 0)
                    {
                        if (tab == Enums.BudgetTab.Planned.ToString())
                        {
                            if (!isquarter)
                            {
                                List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == EntityId).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                                var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                                List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();
                                if (lineitemcostlist.Where(pcptc => pcptc.Period == period).Any())
                                {
                                    var costlineitemperiod = lineitemcostlist.Where(pcptlc => pcptlc.Period == period).Sum(pcptlc => pcptlc.Value);
                                    if (monthlycost < costlineitemperiod)
                                    {
                                        monthlycost = costlineitemperiod;
                                    }
                                }
                                double tacticost = 0;
                                var objTactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == EntityId && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
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
                                    objTacticCost.CreatedBy = Sessions.User.UserId;
                                    objTacticCost.CreatedDate = DateTime.Now;
                                    db.Entry(objTacticCost).State = EntityState.Added;
                                    tacticost += monthlycost;
                                }

                                double totalLineitemCost = 0;
                                if (objtotalLineitemCost != null && objtotalLineitemCost.Count() > 0)
                                {
                                    totalLineitemCost = objtotalLineitemCost.Sum(l => l.Cost);
                                }
                                if (totalLineitemCost > yearlycost)
                                {
                                    yearlycost = totalLineitemCost;
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
                                            objTacticCost.CreatedBy = Sessions.User.UserId;
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
                                db.Entry(objTactic).State = EntityState.Modified;
                                //// Calculate Total LineItem Cost.
                                Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                                if (objOtherLineItem == null)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = EntityId;
                                    objNewLineitem.Title = Common.LineItemTitleDefault + objTactic.Title;
                                    if (yearlycost > totalLineitemCost)
                                    {
                                        objNewLineitem.Cost = yearlycost - totalLineitemCost;
                                    }
                                    else
                                    {
                                        objNewLineitem.Cost = 0;
                                    }
                                    objNewLineitem.Description = string.Empty;
                                    objNewLineitem.CreatedBy = Sessions.User.UserId;
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
                                        monthlycost = costlineitemperiod;
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
                                    objTacticBudget.CreatedBy = Sessions.User.UserId;
                                    objTacticBudget.CreatedDate = DateTime.Now;
                                    db.Entry(objTacticBudget).State = EntityState.Added;
                                    tacticost += monthlycost;
                                }

                                double totalLineitemCost = 0;
                                if (objtotalLineitemCost != null && objtotalLineitemCost.Count() > 0)
                                {
                                    totalLineitemCost = objtotalLineitemCost.Sum(l => l.Cost);
                                }
                                if (totalLineitemCost > yearlycost)
                                {
                                    yearlycost = totalLineitemCost;
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
                                            objTacticCost.CreatedBy = Sessions.User.UserId;
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
                                db.Entry(objTactic).State = EntityState.Modified;
                                //// Calculate Total LineItem Cost.
                                Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                                if (objOtherLineItem == null)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = EntityId;
                                    objNewLineitem.Title = Common.LineItemTitleDefault + objTactic.Title;
                                    if (yearlycost > totalLineitemCost)
                                    {
                                        objNewLineitem.Cost = yearlycost - totalLineitemCost;
                                    }
                                    else
                                    {
                                        objNewLineitem.Cost = 0;
                                    }
                                    objNewLineitem.Description = string.Empty;
                                    objNewLineitem.CreatedBy = Sessions.User.UserId;
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
                                    objTacticActual.CreatedBy = Sessions.User.UserId;
                                    objTacticActual.CreatedDate = DateTime.Now;
                                    db.Entry(objTacticActual).State = EntityState.Added;
                                }
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
                                    objTacticBudget.CreatedBy = Sessions.User.UserId;
                                    objTacticBudget.CreatedDate = DateTime.Now;
                                    db.Entry(objTacticBudget).State = EntityState.Added;
                                }
                            }
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
                                    objlineitemCost.CreatedBy = Sessions.User.UserId;
                                    objlineitemCost.CreatedDate = DateTime.Now;
                                    db.Entry(objlineitemCost).State = EntityState.Added;
                                    lineitemtotalcost += monthlycost;
                                }


                                if (tacticostslist.Where(pcptc => pcptc.Period == period).Any())
                                {
                                    var tacticmonthcost = tacticostslist.Where(pcptc => pcptc.Period == period).FirstOrDefault().Value;
                                    double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == period).Sum(lineitem => lineitem.Value); //modified by komal Rawal
                                    if (tacticlineitemcostmonth > tacticmonthcost)
                                    {
                                        tacticostslist.Where(pcptc => pcptc.Period == period).FirstOrDefault().Value = tacticlineitemcostmonth;
                                        objTactic.Cost = objTactic.Cost + (tacticlineitemcostmonth - tacticmonthcost);
                                    }
                                }
                                else
                                {
                                    double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.Period == period).Sum(lineitem => lineitem.Value); //modified by komal Rawal
                                    Plan_Campaign_Program_Tactic_Cost objtacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                    objtacticCost.PlanTacticId = plantacticid;
                                    objtacticCost.Period = period;
                                    objtacticCost.Value = tacticlineitemcostmonth;
                                    objtacticCost.CreatedBy = Sessions.User.UserId;
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
                                            objlineitemCost.CreatedBy = Sessions.User.UserId;
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
                                            objtacticCost.CreatedBy = Sessions.User.UserId;
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
                                db.Entry(objTactic).State = EntityState.Modified;
                                //// Calculate Total LineItem Cost.
                                Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                                double tacticlineitemcost = objtotalLineitemCost.Where(lineitem => lineitem.PlanLineItemId != EntityId).Sum(lineitem => lineitem.Cost) + yearlycost;
                                if (objOtherLineItem == null)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = EntityId;
                                    objNewLineitem.Title = Common.LineItemTitleDefault + objTactic.Title;
                                    if (objTactic.Cost > tacticlineitemcost)
                                    {
                                        objNewLineitem.Cost = objTactic.Cost - tacticlineitemcost;
                                    }
                                    else
                                    {
                                        objNewLineitem.Cost = 0;
                                    }
                                    objNewLineitem.Description = string.Empty;
                                    objNewLineitem.CreatedBy = Sessions.User.UserId;
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
                                    objTacticBudget.CreatedBy = Sessions.User.UserId;
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
                                            objlineitemCost.CreatedBy = Sessions.User.UserId;
                                            objlineitemCost.CreatedDate = DateTime.Now;
                                            db.Entry(objlineitemCost).State = EntityState.Added;
                                        }
                                    }
                                    else
                                    {
                                        yearlycost = lineitemtotalcost;
                                    }
                                }
                                objLineitem.Cost = yearlycost;

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
                                    monthlyTotalLineItemCost = PrevTacticBudgetAllocationList.Where(lineCost => QuarterList.Contains(lineCost.Period)).ToList().Sum(lineCost => lineCost.Value);
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
                                                objtacticCost.CreatedBy = Sessions.User.UserId;
                                                objtacticCost.CreatedDate = DateTime.Now;
                                                db.Entry(objtacticCost).State = EntityState.Added;

                                            }
                                        }
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

                                        objTactic.Cost = objTactic.Cost + diffCost;
                                    }
                                }

                                db.Entry(objTactic).State = EntityState.Modified;
                                db.Entry(objLineitem).State = EntityState.Modified;
                                //// Calculate Total LineItem Cost.
                                Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                                double totallineitemcost = objtotalLineitemCost.Where(lineitem => lineitem.PlanLineItemId != EntityId).Sum(lineitem => lineitem.Cost) + yearlycost;
                                if (objOtherLineItem == null)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = EntityId;
                                    objNewLineitem.Title = Common.LineItemTitleDefault + objTactic.Title;
                                    if (objTactic.Cost > totallineitemcost)
                                    {
                                        objNewLineitem.Cost = objTactic.Cost - totallineitemcost;
                                    }
                                    else
                                    {
                                        objNewLineitem.Cost = 0;
                                    }
                                    objNewLineitem.Description = string.Empty;
                                    objNewLineitem.CreatedBy = Sessions.User.UserId;
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
                                    }
                                    db.Entry(objOtherLineItem).State = EntityState.Modified;
                                }
                            }
                        }
                        else if (tab == Enums.BudgetTab.Actual.ToString())
                        {
                            var objLineTactic = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt => pcpt.PlanLineItemId == EntityId && pcpt.IsDeleted.Equals(false)).FirstOrDefault();
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
                                    objLineItemActual.CreatedBy = Sessions.User.UserId;
                                    objLineItemActual.CreatedDate = DateTime.Now;
                                    db.Entry(objLineItemActual).State = EntityState.Added;
                                }
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
                                    objTacticBudget.CreatedBy = Sessions.User.UserId;
                                    objTacticBudget.CreatedDate = DateTime.Now;
                                    db.Entry(objTacticBudget).State = EntityState.Added;
                                }
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
                                objTacticCost.CreatedBy = Sessions.User.UserId;
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
                            objNewLineitem.Title = Common.LineItemTitleDefault + pcpobj.Title;
                            if (pcpobj.Cost > totalLineitemCost)
                            {
                                objNewLineitem.Cost = pcpobj.Cost - totalLineitemCost;
                            }
                            else
                            {
                                objNewLineitem.Cost = 0;
                            }
                            objNewLineitem.Description = string.Empty;
                            objNewLineitem.CreatedBy = Sessions.User.UserId;
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
                            }
                            db.Entry(objOtherLineItem).State = EntityState.Modified;
                        }

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
                                objlineitemCost.CreatedBy = Sessions.User.UserId;
                                objlineitemCost.CreatedDate = DateTime.Now;
                                db.Entry(objlineitemCost).State = EntityState.Added;
                            }

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
                                objtacticCost.CreatedBy = Sessions.User.UserId;
                                objtacticCost.CreatedDate = DateTime.Now;
                                db.Entry(objtacticCost).State = EntityState.Added;
                                objTactic.Cost = objTactic.Cost + tacticlineitemcostmonth;
                            }
                            db.Entry(objTactic).State = EntityState.Modified;
                        }
                        else if (YearlyCost < objLineitem.Cost)
                        {
                            var diffcost = objLineitem.Cost - YearlyCost;
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

                        }

                        objLineitem.Cost = YearlyCost;
                        //// Calculate TotalLineItemCost.
                        double totalLineitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == objTactic.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);

                        //// Insert or Modified LineItem Data.
                        var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == objTactic.PlanTacticId && l.LineItemTypeId == null);

                        //// if Tactic total cost is greater than totalLineItem cost then Insert LineItem record otherwise delete objOtherLineItem data from table Plan_Campaign_Program_Tactic_LineItem. 
                        if (objOtherLineItem == null)
                        {
                            //// Insert New record to table.
                            Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                            objNewLineitem.PlanTacticId = objTactic.PlanTacticId;
                            objNewLineitem.Title = Common.LineItemTitleDefault + objTactic.Title;
                            if (objTactic.Cost > totalLineitemCost)
                            {
                                objNewLineitem.Cost = objTactic.Cost - totalLineitemCost;
                            }
                            else
                            {
                                objNewLineitem.Cost = 0;
                            }

                            objNewLineitem.Description = string.Empty;
                            objNewLineitem.CreatedBy = Sessions.User.UserId;
                            objNewLineitem.CreatedDate = DateTime.Now;
                            db.Entry(objNewLineitem).State = EntityState.Added;
                            db.SaveChanges();
                        }
                        else
                        {
                            objOtherLineItem.IsDeleted = false;
                            if (objTactic.Cost > totalLineitemCost)
                            {
                                objOtherLineItem.Cost = objTactic.Cost - totalLineitemCost;
                            }
                            else
                            {
                                objOtherLineItem.Cost = 0;
                            }
                            db.Entry(objOtherLineItem).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        db.Entry(objLineitem).State = EntityState.Modified;

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
        #endregion

        #region Budget Review Section

        /// <summary>
        /// View fro the initial render page 
        /// </summary>
        /// <returns></returns>
        public ActionResult Budgeting(int PlanId)
        {
            ViewBag.ActiveMenu = Enums.ActiveMenu.Plan;
            HomePlanModel planmodel = new Models.HomePlanModel();
            try
            {
                bool IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
                ViewBag.IsPlanCreateAuthorized = IsPlanCreateAuthorized;
                var objPlan = db.Plans.FirstOrDefault(_plan => _plan.PlanId == PlanId);
                var IsQuarter = objPlan.Year;
                ViewBag.PlanId = PlanId;

                bool IsPlanCreateAll = false;
                if (IsPlanCreateAuthorized)
                {
                    IsPlanCreateAll = true;
                }
                else
                {
                    if ((objPlan.CreatedBy.Equals(Sessions.User.UserId)))
                    {
                        IsPlanCreateAll = true;
                    }
                }


                //// Set ViewBy list.
                string entTacticType = Enums.EntityType.Tactic.ToString();
                List<ViewByModel> lstViewBy = new List<ViewByModel>();
                lstViewBy.Add(new ViewByModel { Text = "Campaigns", Value = "0" });
                List<CustomField> lstTacticCustomfield = db.CustomFields.Where(custom => custom.IsDeleted.Equals(false) && custom.EntityType.Equals(entTacticType) && custom.ClientId.Equals(Sessions.User.ClientId) && custom.IsDisplayForFilter.Equals(true)).ToList();
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

                planmodel.objplanhomemodelheader = Common.GetPlanHeaderValue(PlanId);
                ViewBag.IsPlanCreateAll = IsPlanCreateAll;
                ViewBag.IsQuarter = IsQuarter;
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
        /// model for the allocated tab
        /// </summary>
        /// <param name="PlanId"></param>
        /// <returns></returns>
        public ActionResult GetAllocatedBugetData(int PlanId)
        {
            List<Guid> lstSubordinatesIds = new List<Guid>();
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
            }

            var campaign = db.Plan_Campaign.Where(pc => pc.PlanId.Equals(PlanId) && pc.IsDeleted.Equals(false)).Select(pc => pc).ToList();
            var campaignobj = campaign.Select(_campgn => new
            {
                id = _campgn.PlanCampaignId,
                title = _campgn.Title,
                description = _campgn.Description,
                isOwner = Sessions.User.UserId == _campgn.CreatedBy ? 1 : 0,
                Budgeted = _campgn.CampaignBudget,
                Budget = _campgn.Plan_Campaign_Budget.Select(b1 => new BudgetedValue { Period = b1.Period, Value = b1.Value }).ToList(),
                createdBy = _campgn.CreatedBy,
                programs = (db.Plan_Campaign_Program.Where(pcp => pcp.PlanCampaignId.Equals(_campgn.PlanCampaignId) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp).ToList()).Select(pcpj => new
                {
                    id = pcpj.PlanProgramId,
                    title = pcpj.Title,
                    description = pcpj.Description,
                    Budgeted = pcpj.ProgramBudget,
                    Budget = pcpj.Plan_Campaign_Program_Budget.Select(_budgt => new BudgetedValue { Period = _budgt.Period, Value = _budgt.Value }).ToList(),
                    isOwner = Sessions.User.UserId == pcpj.CreatedBy ? 1 : 0,
                    createdBy = pcpj.CreatedBy,
                    tactics = (db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId.Equals(pcpj.PlanProgramId) && pcpt.IsDeleted.Equals(false)).Select(pcpt => pcpt).ToList()).Select(pcptj => new
                    {
                        id = pcptj.PlanTacticId,
                        title = pcptj.Title,
                        description = pcptj.Description,
                        Cost = pcptj.TacticBudget,
                        Budget = pcptj.Plan_Campaign_Program_Tactic_Budget.Select(t => new BudgetedValue { Period = t.Period, Value = t.Value }).ToList(),
                        isOwner = Sessions.User.UserId == pcptj.CreatedBy ? 1 : 0,
                        createBy = pcptj.CreatedBy

                    }).Select(pcptj => pcptj).Distinct().OrderBy(pcptj => pcptj.id)

                }).Select(pcpj => pcpj).Distinct().OrderBy(pcpj => pcpj.id)
            }).Select(_campgn => _campgn).Distinct().OrderBy(_campgn => _campgn.id);

            var lstCampaignTmp = campaignobj.Select(_campgn => new
            {
                id = _campgn.id,
                title = _campgn.title,
                description = _campgn.description,
                isOwner = _campgn.isOwner,
                Budgeted = _campgn.Budgeted,
                Budget = _campgn.Budget,
                createdBy = _campgn.createdBy,
                programs = _campgn.programs.Select(_prgrm => new
                {
                    id = _prgrm.id,
                    title = _prgrm.title,
                    description = _prgrm.description,
                    Budgeted = _prgrm.Budgeted,
                    Budget = _prgrm.Budget,
                    isOwner = _prgrm.isOwner,
                    createdBy = _prgrm.createdBy,
                    tactics = _prgrm.tactics.Select(_tactic => new
                    {
                        id = _tactic.id,
                        title = _tactic.title,
                        description = _tactic.description,
                        Budgeted = _tactic.Cost,
                        Budget = _tactic.Budget,
                        isOwner = _tactic.isOwner,
                        createdBy = _tactic.createBy
                    })
                })
            });

            var lstCampaign = lstCampaignTmp.Select(_camgn => new
            {
                id = _camgn.id,
                title = _camgn.title,
                description = _camgn.description,
                Budget = _camgn.Budget,
                Budgeted = _camgn.Budgeted,
                isOwner = _camgn.isOwner == 1 ? (_camgn.programs.Where(p => p.tactics.Any(t => t.isOwner == 0)).Select(p => p.tactics).ToList().Count > 0 ? 0 : 1) : 0,
                createdBy = _camgn.createdBy,
                programs = _camgn.programs.Select(_prgm => new
                {
                    id = _prgm.id,
                    title = _prgm.title,
                    description = _prgm.description,
                    Budget = _prgm.Budget,
                    Budgeted = _prgm.Budgeted,
                    isOwner = _prgm.isOwner == 1 ? (_prgm.tactics.Any(_tactic => _tactic.isOwner == 0) ? 0 : 1) : 0,
                    createdBy = _prgm.createdBy,
                    tactics = _prgm.tactics
                })
            });

            string AllocatedBy = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();
            List<BudgetModel> model = new List<BudgetModel>();
            BudgetModel obj;
            Plan objPlan = db.Plans.FirstOrDefault(_pln => _pln.PlanId.Equals(PlanId));
            string parentPlanId = "0", parentCampaignId = "0", parentProgramId = "0";
            if (objPlan != null)
            {
                AllocatedBy = objPlan.AllocatedBy;
                List<BudgetedValue> lst = (from bv in objPlan.Plan_Budget
                                           select new BudgetedValue
                                           {
                                               Period = bv.Period,
                                               Value = bv.Value
                                           }).ToList();
                //// Insert Plan data to Model.
                obj = new BudgetModel();
                obj.ActivityId = objPlan.PlanId.ToString();
                obj.ActivityName = objPlan.Title;
                obj.ActivityType = ActivityType.ActivityPlan;
                obj.ParentActivityId = "0";
                obj.Budgeted = objPlan.Budget;
                obj.IsOwner = true;
                obj.isEditable = false;
                obj.CreatedBy = objPlan.CreatedBy;
                obj = GetMonthWiseData(obj, lst);
                model.Add(obj);
                parentPlanId = objPlan.PlanId.ToString();
                foreach (var c in lstCampaign)
                {
                    //// Insert Campaign data to Model.
                    obj = new BudgetModel();
                    obj.ActivityId = c.id.ToString();
                    obj.ActivityName = c.title;
                    obj.ActivityType = ActivityType.ActivityCampaign;
                    obj.ParentActivityId = parentPlanId.ToString();
                    obj.IsOwner = Convert.ToBoolean(c.isOwner);
                    obj = GetMonthWiseData(obj, c.Budget);
                    obj.Budgeted = c.Budgeted;
                    obj.CreatedBy = c.createdBy;
                    obj.isEditable = false;
                    model.Add(obj);
                    parentCampaignId = c.id.ToString();
                    foreach (var p in c.programs)
                    {
                        //// Insert Program data to Model.
                        obj = new BudgetModel();
                        obj.ActivityId = p.id.ToString();
                        obj.ActivityName = p.title;
                        obj.ActivityType = ActivityType.ActivityProgram;
                        obj.ParentActivityId = parentCampaignId;
                        obj.IsOwner = Convert.ToBoolean(p.isOwner);
                        obj = GetMonthWiseData(obj, p.Budget);
                        obj.Budgeted = p.Budgeted;
                        obj.CreatedBy = p.createdBy;
                        obj.isEditable = false;
                        model.Add(obj);
                        parentProgramId = p.id.ToString();
                        foreach (var t in p.tactics)
                        {
                            //// Insert Tactic data to Model.
                            obj = new BudgetModel();
                            obj.ActivityId = t.id.ToString();
                            obj.ActivityName = t.title;
                            obj.ActivityType = ActivityType.ActivityTactic;
                            obj.ParentActivityId = parentProgramId;
                            obj.IsOwner = Convert.ToBoolean(t.isOwner);
                            obj = GetMonthWiseData(obj, t.Budget);
                            obj.CreatedBy = t.createdBy;
                            obj.isEditable = false;
                            obj.Budgeted = t.Budgeted;
                            model.Add(obj);
                        }
                    }
                }
            }

            //Added by Mitesh Vaishnav - set flag for editing campaign/program/tactic/plan 
            string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
            string EntityTypeTactic = Enums.EntityType.Tactic.ToString();
            bool isDisplayForFilter = false;
            bool IsCustomFeildExist = Common.IsCustomFeildExist(Enums.EntityType.Tactic.ToString(), Sessions.User.ClientId);
            var CustomFieldexists = db.CustomFields.Where(customfield => customfield.ClientId == Sessions.User.ClientId && customfield.EntityType.Equals(EntityTypeTactic) &&
                                                                        (customfield.IsRequired && !isDisplayForFilter) && customfield.IsDeleted.Equals(false)
                                                                         ).Any();
            List<string> tacIds = model.Where(t => t.ActivityType == EntityTypeTactic).Select(t => t.ActivityId).ToList();
            tacIds = tacIds.Select(t => t.Replace("cpt_", "")).ToList();
            List<int> intList = tacIds.ConvertAll(s => Int32.Parse(s));
            var Entities = db.CustomField_Entity.Where(entityid => intList.Contains(entityid.EntityId)).Select(entityid => entityid).ToList();

            var lstAllTacticCustomFieldEntities = db.CustomField_Entity.Where(customFieldEntity => customFieldEntity.CustomField.ClientId == Sessions.User.ClientId &&
                                                                                                        customFieldEntity.CustomField.IsDeleted.Equals(false) &&
                                                                                                        customFieldEntity.CustomField.EntityType.Equals(EntityTypeTactic) &&
                                                                                                        customFieldEntity.CustomField.CustomFieldType.Name.Equals(DropDownList) &&
                                                                                                        (isDisplayForFilter ? customFieldEntity.CustomField.IsDisplayForFilter.Equals(true) : true) &&
                                                                                                        intList.Contains(customFieldEntity.EntityId))
                                                                                                .Select(customFieldEntity => customFieldEntity).Distinct().ToList(); //todo : able to move up
            List<RevenuePlanner.Models.CustomRestriction> userCustomRestrictionList = Common.GetUserCustomRestrictionsList(Sessions.User.UserId, true);
            foreach (var item in model)
            {
                if (item.ActivityType == ActivityType.ActivityPlan)
                {
                    bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                    if (item.CreatedBy == Sessions.User.UserId || IsPlanEditAllAuthorized)
                    {
                        item.isEditable = true;
                    }
                    else if (lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        item.isEditable = true;
                    }
                }
                else if (item.ActivityType == ActivityType.ActivityCampaign)
                {
                    if (item.CreatedBy == Sessions.User.UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        //item.programs.ToList().ForEach(p => p.tactics.ToList().ForEach(t => planTacticIds.Add(t.id)));
                        var modeltacticIds = model.Where(minner => minner.ActivityType == ActivityType.ActivityProgram && minner.ParentActivityId == item.ActivityId).Select(minner => minner.ActivityId).ToList();
                        model.Where(m => m.ActivityType == ActivityType.ActivityTactic && model.Where(minner => minner.ActivityType == ActivityType.ActivityProgram && minner.ParentActivityId == item.ActivityId).Select(minner => minner.ActivityId).ToList().Contains(m.ParentActivityId)).ToList().ForEach(t => planTacticIds.Add(Convert.ToInt32(t.ActivityId)));
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
                        if (lstAllowedEntityIds.Count == planTacticIds.Count)
                        {
                            item.isEditable = true;
                        }
                        else
                        {
                            item.isEditable = false;
                        }
                    }
                }
                else if (item.ActivityType == ActivityType.ActivityProgram)
                {
                    if (item.CreatedBy == Sessions.User.UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        model.Where(m => m.ActivityType == ActivityType.ActivityTactic && m.ParentActivityId == item.ActivityId).ToList().ForEach(t => planTacticIds.Add(Convert.ToInt32(t.ActivityId)));
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
                        if (lstAllowedEntityIds.Count == planTacticIds.Count)
                        {
                            item.isEditable = true;
                        }
                        else
                        {
                            item.isEditable = false;
                        }
                    }
                }
                else if (item.ActivityType == ActivityType.ActivityTactic)
                {
                    if (item.CreatedBy == Sessions.User.UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        planTacticIds.Add(Convert.ToInt32(item.ActivityId));
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
                        if (lstAllowedEntityIds.Count == planTacticIds.Count)
                        {
                            item.isEditable = true;
                        }
                        else
                        {
                            item.isEditable = false;
                        }
                    }
                }
            }
            //End Added by Mitesh Vaishnav - set flag for editing campaign/program/tactic/plan 

            // Start - Added by Sohel Pathan on 08/09/2014 for PL ticket #642
            // Set budget for quarters
            if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower())
            {
                foreach (BudgetModel bm in model)
                {
                    bm.Month.Jan = bm.Month.Jan + bm.Month.Feb + bm.Month.Mar;
                    bm.Month.Apr = bm.Month.Apr + bm.Month.May + bm.Month.Jun;
                    bm.Month.Jul = bm.Month.Jul + bm.Month.Aug + bm.Month.Sep;
                    bm.Month.Oct = bm.Month.Oct + bm.Month.Nov + bm.Month.Dec;
                    bm.Month.Feb = 0;
                    bm.Month.Mar = 0;
                    bm.Month.May = 0;
                    bm.Month.Jun = 0;
                    bm.Month.Aug = 0;
                    bm.Month.Sep = 0;
                    bm.Month.Nov = 0;
                    bm.Month.Dec = 0;
                }
            }
            // End - Added by Sohel Pathan on 08/09/2014 for PL ticket #642

            ViewBag.AllocatedBy = AllocatedBy;
            model = SetupValues(model);

            //Month wise allocated for header 
            BudgetMonth a = new BudgetMonth();
            BudgetMonth b = new BudgetMonth();
            BudgetMonth PercAllocated = new BudgetMonth();
            a = model.Where(m => m.ActivityType == ActivityType.ActivityPlan).Select(m => m.Month).FirstOrDefault();
            b.Jan = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign).Sum(m => m.Month.Jan);
            b.Feb = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign).Sum(m => m.Month.Feb);
            b.Mar = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign).Sum(m => m.Month.Mar);
            b.Apr = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign).Sum(m => m.Month.Apr);
            b.May = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign).Sum(m => m.Month.May);
            b.Jun = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign).Sum(m => m.Month.Jun);
            b.Jul = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign).Sum(m => m.Month.Jul);
            b.Aug = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign).Sum(m => m.Month.Aug);
            b.Sep = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign).Sum(m => m.Month.Sep);
            b.Oct = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign).Sum(m => m.Month.Oct);
            b.Nov = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign).Sum(m => m.Month.Nov);
            b.Dec = model.Where(m => m.ActivityType == ActivityType.ActivityCampaign).Sum(m => m.Month.Dec);

            PercAllocated.Jan = (a.Jan == 0 && b.Jan == 0) ? 0 : (a.Jan == 0 && b.Jan > 0) ? 101 : b.Jan / a.Jan * 100;
            PercAllocated.Feb = (a.Feb == 0 && b.Feb == 0) ? 0 : (a.Feb == 0 && b.Feb > 0) ? 101 : b.Feb / a.Feb * 100;
            PercAllocated.Mar = (a.Mar == 0 && b.Mar == 0) ? 0 : (a.Mar == 0 && b.Mar > 0) ? 101 : b.Mar / a.Mar * 100;
            PercAllocated.Apr = (a.Apr == 0 && b.Apr == 0) ? 0 : (a.Apr == 0 && b.Apr > 0) ? 101 : b.Apr / a.Apr * 100;
            PercAllocated.May = (a.May == 0 && b.May == 0) ? 0 : (a.May == 0 && b.May > 0) ? 101 : b.May / a.May * 100;
            PercAllocated.Jun = (a.Jun == 0 && b.Jun == 0) ? 0 : (a.Jun == 0 && b.Jun > 0) ? 101 : b.Jun / a.Jun * 100;
            PercAllocated.Jul = (a.Jul == 0 && b.Jul == 0) ? 0 : (a.Jul == 0 && b.Jul > 0) ? 101 : b.Jul / a.Jul * 100;
            PercAllocated.Aug = (a.Aug == 0 && b.Aug == 0) ? 0 : (a.Aug == 0 && b.Aug > 0) ? 101 : b.Aug / a.Aug * 100;
            PercAllocated.Sep = (a.Sep == 0 && b.Sep == 0) ? 0 : (a.Sep == 0 && b.Sep > 0) ? 101 : b.Sep / a.Sep * 100;
            PercAllocated.Oct = (a.Oct == 0 && b.Oct == 0) ? 0 : (a.Oct == 0 && b.Oct > 0) ? 101 : b.Oct / a.Oct * 100;
            PercAllocated.Nov = (a.Nov == 0 && b.Nov == 0) ? 0 : (a.Nov == 0 && b.Nov > 0) ? 101 : b.Nov / a.Nov * 100;
            PercAllocated.Dec = (a.Dec == 0 && b.Dec == 0) ? 0 : (a.Dec == 0 && b.Dec > 0) ? 101 : b.Dec / a.Dec * 100;

            ViewBag.PercAllocated = PercAllocated;
            Sessions.PlanId = PlanId;
            return PartialView("_AllocatedBudget", model);
        }

        /// <summary>
        /// set monthly data for the object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="lst"></param>
        /// <returns></returns>
        private BudgetModel GetMonthWiseData(BudgetModel obj, List<BudgetedValue> lst)
        {
            //// Set Monthly data.
            BudgetMonth month = new BudgetMonth();
            month.Jan = lst.Where(v => v.Period.ToUpper() == Common.Jan).Select(v => v.Value).FirstOrDefault();
            month.Feb = lst.Where(v => v.Period.ToUpper() == Common.Feb).Select(v => v.Value).FirstOrDefault();
            month.Mar = lst.Where(v => v.Period.ToUpper() == Common.Mar).Select(v => v.Value).FirstOrDefault();
            month.Apr = lst.Where(v => v.Period.ToUpper() == Common.Apr).Select(v => v.Value).FirstOrDefault();
            month.May = lst.Where(v => v.Period.ToUpper() == Common.May).Select(v => v.Value).FirstOrDefault();
            month.Jun = lst.Where(v => v.Period.ToUpper() == Common.Jun).Select(v => v.Value).FirstOrDefault();
            month.Jul = lst.Where(v => v.Period.ToUpper() == Common.Jul).Select(v => v.Value).FirstOrDefault();
            month.Aug = lst.Where(v => v.Period.ToUpper() == Common.Aug).Select(v => v.Value).FirstOrDefault();
            month.Sep = lst.Where(v => v.Period.ToUpper() == Common.Sep).Select(v => v.Value).FirstOrDefault();
            month.Oct = lst.Where(v => v.Period.ToUpper() == Common.Oct).Select(v => v.Value).FirstOrDefault();
            month.Nov = lst.Where(v => v.Period.ToUpper() == Common.Nov).Select(v => v.Value).FirstOrDefault();
            month.Dec = lst.Where(v => v.Period.ToUpper() == Common.Dec).Select(v => v.Value).FirstOrDefault();
            obj.Month = month;

            obj.Allocated = lst.Sum(v => v.Value);

            return obj;
        }

        /// <summary>
        /// set monthly data for the object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="lst"></param>
        /// <returns></returns>
        private BudgetModel GetMonthWiseDataForBudget(BudgetModel obj, List<BudgetedValue> lst)
        {
            //// Set Monthly data.
            BudgetMonth month = new BudgetMonth();
            month.Jan = lst.Where(v => v.Period.ToUpper() == Common.Jan).Select(v => v.Value).FirstOrDefault();
            month.Feb = lst.Where(v => v.Period.ToUpper() == Common.Feb).Select(v => v.Value).FirstOrDefault();
            month.Mar = lst.Where(v => v.Period.ToUpper() == Common.Mar).Select(v => v.Value).FirstOrDefault();
            month.Apr = lst.Where(v => v.Period.ToUpper() == Common.Apr).Select(v => v.Value).FirstOrDefault();
            month.May = lst.Where(v => v.Period.ToUpper() == Common.May).Select(v => v.Value).FirstOrDefault();
            month.Jun = lst.Where(v => v.Period.ToUpper() == Common.Jun).Select(v => v.Value).FirstOrDefault();
            month.Jul = lst.Where(v => v.Period.ToUpper() == Common.Jul).Select(v => v.Value).FirstOrDefault();
            month.Aug = lst.Where(v => v.Period.ToUpper() == Common.Aug).Select(v => v.Value).FirstOrDefault();
            month.Sep = lst.Where(v => v.Period.ToUpper() == Common.Sep).Select(v => v.Value).FirstOrDefault();
            month.Oct = lst.Where(v => v.Period.ToUpper() == Common.Oct).Select(v => v.Value).FirstOrDefault();
            month.Nov = lst.Where(v => v.Period.ToUpper() == Common.Nov).Select(v => v.Value).FirstOrDefault();
            month.Dec = lst.Where(v => v.Period.ToUpper() == Common.Dec).Select(v => v.Value).FirstOrDefault();
            obj.BudgetMonth = month;

            return obj;
        }
        /// <summary>
        /// set other data for model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<BudgetModel> SetupValues(List<BudgetModel> model)
        {
            BudgetModel plan = model.Where(_mdl => _mdl.ActivityType == ActivityType.ActivityPlan).FirstOrDefault();
            if (plan != null)
            {
                plan.ParentMonth = new BudgetMonth();
                plan.Allocated = plan.Budgeted;
                plan.SumMonth = plan.Month;
                List<BudgetModel> campaigns = model.Where(_mdl => _mdl.ActivityType == ActivityType.ActivityCampaign && _mdl.ParentActivityId == plan.ActivityId).ToList();
                BudgetMonth SumCampaign = new BudgetMonth();
                foreach (BudgetModel c in campaigns)
                {
                    //// Set Campaign monthly data.
                    c.Allocated = c.Budgeted;
                    SumCampaign.Jan += c.Month.Jan;
                    SumCampaign.Feb += c.Month.Feb;
                    SumCampaign.Mar += c.Month.Mar;
                    SumCampaign.Apr += c.Month.Apr;
                    SumCampaign.May += c.Month.May;
                    SumCampaign.Jun += c.Month.Jun;
                    SumCampaign.Jul += c.Month.Jul;
                    SumCampaign.Aug += c.Month.Aug;
                    SumCampaign.Sep += c.Month.Sep;
                    SumCampaign.Oct += c.Month.Oct;
                    SumCampaign.Nov += c.Month.Nov;
                    SumCampaign.Dec += c.Month.Dec;
                    c.SumMonth = SumCampaign;
                    c.ParentMonth = new BudgetMonth();
                    List<BudgetModel> programs = model.Where(p => p.ActivityType == ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).ToList();
                    BudgetMonth SumProgram = new BudgetMonth();
                    foreach (BudgetModel p in programs)
                    {
                        //// Set Program monthly data.
                        p.Allocated = p.Budgeted;
                        SumProgram.Jan += p.Month.Jan;
                        SumProgram.Feb += p.Month.Feb;
                        SumProgram.Mar += p.Month.Mar;
                        SumProgram.Apr += p.Month.Apr;
                        SumProgram.May += p.Month.May;
                        SumProgram.Jun += p.Month.Jun;
                        SumProgram.Jul += p.Month.Jul;
                        SumProgram.Aug += p.Month.Aug;
                        SumProgram.Sep += p.Month.Sep;
                        SumProgram.Oct += p.Month.Oct;
                        SumProgram.Nov += p.Month.Nov;
                        SumProgram.Dec += p.Month.Dec;
                        p.SumMonth = SumProgram;
                        p.ParentMonth = new BudgetMonth();
                        List<BudgetModel> tactics = model.Where(t => t.ActivityType == ActivityType.ActivityTactic && t.ParentActivityId == p.ActivityId).ToList();
                        BudgetMonth SumTactic = new BudgetMonth();
                        foreach (BudgetModel t in tactics)
                        {
                            t.Allocated = t.Budgeted;
                            SumTactic.Jan += t.Month.Jan;
                            SumTactic.Feb += t.Month.Feb;
                            SumTactic.Mar += t.Month.Mar;
                            SumTactic.Apr += t.Month.Apr;
                            SumTactic.May += t.Month.May;
                            SumTactic.Jun += t.Month.Jun;
                            SumTactic.Jul += t.Month.Jul;
                            SumTactic.Aug += t.Month.Aug;
                            SumTactic.Sep += t.Month.Sep;
                            SumTactic.Oct += t.Month.Oct;
                            SumTactic.Nov += t.Month.Nov;
                            SumTactic.Dec += t.Month.Dec;
                            t.SumMonth = SumTactic;
                            t.ParentMonth = new BudgetMonth();
                        }
                    }
                }
                //finding remaining
                campaigns = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == plan.ActivityId).ToList();
                SumCampaign = new BudgetMonth();
                foreach (BudgetModel c in campaigns)
                {
                    //// Set Campaign monthly parent data.
                    c.ParentMonth.Jan = plan.Month.Jan - c.SumMonth.Jan;
                    c.ParentMonth.Feb = plan.Month.Feb - c.SumMonth.Feb;
                    c.ParentMonth.Mar = plan.Month.Mar - c.SumMonth.Mar;
                    c.ParentMonth.Apr = plan.Month.Apr - c.SumMonth.Apr;
                    c.ParentMonth.May = plan.Month.May - c.SumMonth.May;
                    c.ParentMonth.Jun = plan.Month.Jun - c.SumMonth.Jun;
                    c.ParentMonth.Jul = plan.Month.Jul - c.SumMonth.Jul;
                    c.ParentMonth.Aug = plan.Month.Aug - c.SumMonth.Aug;
                    c.ParentMonth.Sep = plan.Month.Sep - c.SumMonth.Sep;
                    c.ParentMonth.Oct = plan.Month.Oct - c.SumMonth.Oct;
                    c.ParentMonth.Nov = plan.Month.Nov - c.SumMonth.Nov;
                    c.ParentMonth.Dec = plan.Month.Dec - c.SumMonth.Dec;

                    List<BudgetModel> programs = model.Where(p => p.ActivityType == ActivityType.ActivityProgram && p.ParentActivityId == c.ActivityId).ToList();
                    BudgetMonth SumProgram = new BudgetMonth();
                    foreach (BudgetModel p in programs)
                    {
                        //// Set Campaign monthly parent data.
                        p.ParentMonth.Jan = c.Month.Jan - p.SumMonth.Jan;
                        p.ParentMonth.Feb = c.Month.Feb - p.SumMonth.Feb;
                        p.ParentMonth.Mar = c.Month.Mar - p.SumMonth.Mar;
                        p.ParentMonth.Apr = c.Month.Apr - p.SumMonth.Apr;
                        p.ParentMonth.May = c.Month.May - p.SumMonth.May;
                        p.ParentMonth.Jun = c.Month.Jun - p.SumMonth.Jun;
                        p.ParentMonth.Jul = c.Month.Jul - p.SumMonth.Jul;
                        p.ParentMonth.Aug = c.Month.Aug - p.SumMonth.Aug;
                        p.ParentMonth.Sep = c.Month.Sep - p.SumMonth.Sep;
                        p.ParentMonth.Oct = c.Month.Oct - p.SumMonth.Oct;
                        p.ParentMonth.Nov = c.Month.Nov - p.SumMonth.Nov;
                        p.ParentMonth.Dec = c.Month.Dec - p.SumMonth.Dec;
                        List<BudgetModel> tactics = model.Where(t => t.ActivityType == ActivityType.ActivityTactic && t.ParentActivityId == p.ActivityId).ToList();
                        BudgetMonth SumTactic = new BudgetMonth();
                        foreach (BudgetModel t in tactics)
                        {
                            t.ParentMonth.Jan = p.Month.Jan - t.SumMonth.Jan;
                            t.ParentMonth.Feb = p.Month.Feb - t.SumMonth.Feb;
                            t.ParentMonth.Mar = p.Month.Mar - t.SumMonth.Mar;
                            t.ParentMonth.Apr = p.Month.Apr - t.SumMonth.Apr;
                            t.ParentMonth.May = p.Month.May - t.SumMonth.May;
                            t.ParentMonth.Jun = p.Month.Jun - t.SumMonth.Jun;
                            t.ParentMonth.Jul = p.Month.Jul - t.SumMonth.Jul;
                            t.ParentMonth.Aug = p.Month.Aug - t.SumMonth.Aug;
                            t.ParentMonth.Sep = p.Month.Sep - t.SumMonth.Sep;
                            t.ParentMonth.Oct = p.Month.Oct - t.SumMonth.Oct;
                            t.ParentMonth.Nov = p.Month.Nov - t.SumMonth.Nov;
                            t.ParentMonth.Dec = p.Month.Dec - t.SumMonth.Dec;
                        }
                    }
                }
            }

            return model;
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

        /// <summary>
        /// Set the model for planned and actuals
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="budgetTab"></param>
        /// <param name="viewBy"></param>
        /// <returns></returns>
        public ActionResult GetBudgetedData(int PlanId, Enums.BudgetTab budgetTab = Enums.BudgetTab.Planned, Enums.ViewBy viewBy = Enums.ViewBy.Campaign)
        {
            //int _viewBy = 6;//1025;
            List<Guid> lstSubordinatesIds = new List<Guid>();
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
            }
            TempData["ViewBy"] = (int)viewBy;//_viewBy;//
            string AllocatedBy = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();
            string entTacticType = Enums.EntityType.Tactic.ToString();
            List<BudgetModel> model = new List<BudgetModel>();
            BudgetModel obj;

            //// Set Campaign data to Create Model.
            var campaign = db.Plan_Campaign.Where(pc => pc.PlanId.Equals(PlanId) && pc.IsDeleted.Equals(false)).Select(pc => pc).ToList();
            var campaignobj = campaign.Select(p => new
            {
                id = "c_" + p.PlanCampaignId.ToString(),
                title = p.Title,
                description = p.Description,
                isOwner = Sessions.User.UserId == p.CreatedBy ? 0 : 1,
                Budget = p.Plan_Campaign_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(),
                BudgetMonth = p.Plan_Campaign_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(),
                MainBudgeted = p.CampaignBudget,
                createdBy = p.CreatedBy,
                programs = (db.Plan_Campaign_Program.Where(pcp => pcp.PlanCampaignId.Equals(p.PlanCampaignId) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp).ToList()).Select(pcpj => new
                {
                    id = "cp_" + pcpj.PlanProgramId.ToString(),
                    title = pcpj.Title,
                    description = pcpj.Description,
                    isOwner = Sessions.User.UserId == pcpj.CreatedBy ? 0 : 1,
                    Budget = pcpj.Plan_Campaign_Program_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(),
                    BudgetMonth = pcpj.Plan_Campaign_Program_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(),
                    MainBudgeted = pcpj.ProgramBudget,
                    createdBy = pcpj.CreatedBy,
                    tactic = (db.Plan_Campaign_Program_Tactic.Where(pcp => pcp.PlanProgramId.Equals(pcpj.PlanProgramId) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp).ToList()).Select(pctj => new
                    {
                        id = "cpt_" + pctj.PlanTacticId.ToString(),
                        title = pctj.Title,
                        description = pctj.Description,
                        isOwner = Sessions.User.UserId == pctj.CreatedBy ? 0 : 1,
                        Cost = pctj.Cost,
                        Budget = budgetTab == Enums.BudgetTab.Planned ? pctj.Plan_Campaign_Program_Tactic_Cost.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList()
                                                                  : pctj.Plan_Campaign_Program_Tactic_Actual.Where(b => b.StageTitle == "Cost").Select(b => new BudgetedValue { Period = b.Period, Value = b.Actualvalue }).ToList(),
                        BudgetMonth = pctj.Plan_Campaign_Program_Tactic_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(),
                        CustomFieldEntities = db.CustomField_Entity.Where(entity => entity.EntityId.Equals(pctj.PlanTacticId) && entity.CustomField.EntityType.Equals(entTacticType)).ToList(),
                        MainBudgeted = pctj.TacticBudget,
                        createdBy = pctj.CreatedBy,
                        isAfterApproved = Common.CheckAfterApprovedStatus(pctj.Status),
                        lineitems = (db.Plan_Campaign_Program_Tactic_LineItem.Where(pcp => pcp.PlanTacticId.Equals(pctj.PlanTacticId) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp).ToList()).Select(pclj => new
                        {
                            id = "cptl_" + pclj.PlanLineItemId.ToString(),
                            title = pclj.Title,
                            description = pclj.Description,
                            isOwner = Sessions.User.UserId == pclj.CreatedBy ? 0 : 1,
                            Cost = pclj.Cost,
                            createdBy = pclj.CreatedBy,
                            lineitemtype = pclj.LineItemTypeId,
                            Budget = budgetTab == Enums.BudgetTab.Planned ? pclj.Plan_Campaign_Program_Tactic_LineItem_Cost.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList()
                                                                     : pclj.Plan_Campaign_Program_Tactic_LineItem_Actual.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(),
                        }).Select(pclj => pclj).Distinct().OrderBy(pclj => pclj.id)
                    }).Select(pctj => pctj).Distinct().OrderBy(pctj => pctj.id)
                }).Select(pcpj => pcpj).Distinct().OrderBy(pcpj => pcpj.id)
            }).Select(p => p).Distinct().OrderBy(p => p.id);
            //// Create BudgetModel based on PlanId.
            Plan objPlan = db.Plans.FirstOrDefault(_pln => _pln.PlanId.Equals(PlanId));
            string parentPlanId = "0", parentCampaignId = "0", parentProgramId = "0", parentTacticId = "0", parentCustomId = "0";
            if (objPlan != null)
            {
                AllocatedBy = objPlan.AllocatedBy;
                List<BudgetedValue> lst = (from _bdgt in objPlan.Plan_Budget
                                           select new BudgetedValue
                                           {
                                               Period = _bdgt.Period,
                                               Value = _bdgt.Value
                                           }).ToList();

                //// Set Plan data to BudgetModel.
                obj = new BudgetModel();
                obj.Id = objPlan.PlanId.ToString();
                obj.ActivityId = "plan_" + objPlan.PlanId.ToString();
                obj.ActivityName = objPlan.Title;
                obj.ActivityType = ActivityType.ActivityPlan;
                obj.ParentActivityId = "0";
                obj.Budgeted = objPlan.Budget;
                obj.IsOwner = true;
                obj = GetMonthWiseData(obj, lst);
                obj = GetMonthWiseDataForBudget(obj, lst);
                obj.MainBudgeted = objPlan.Budget;
                obj.CreatedBy = objPlan.CreatedBy;
                obj.isAfterApproved = false;
                obj.isEditable = false;
                model.Add(obj);
                parentPlanId = "plan_" + objPlan.PlanId.ToString();
                foreach (var c in campaignobj)
                {
                    //// Set Campaign data to BudgetModel.
                    obj = new BudgetModel();
                    obj.Id = c.id.Replace("c_", "");
                    obj.ActivityId = c.id;
                    obj.ActivityName = c.title;
                    obj.ActivityType = ActivityType.ActivityCampaign;
                    obj.ParentActivityId = parentPlanId;
                    obj.IsOwner = Convert.ToBoolean(c.isOwner);
                    obj = GetMonthWiseData(obj, c.Budget);
                    obj = GetMonthWiseDataForBudget(obj, c.BudgetMonth);
                    obj.MainBudgeted = c.MainBudgeted;
                    obj.CreatedBy = c.createdBy;
                    obj.isEditable = false;
                    obj.isAfterApproved = false;
                    model.Add(obj);
                    parentCampaignId = c.id;
                    foreach (var p in c.programs)
                    {
                        //// Set Program data to BudgetModel.
                        obj = new BudgetModel();
                        obj.Id = p.id.Replace("cp_", "");
                        obj.ActivityId = p.id;
                        obj.ActivityName = p.title;
                        obj.ActivityType = ActivityType.ActivityProgram;
                        obj.ParentActivityId = parentCampaignId;
                        obj.IsOwner = Convert.ToBoolean(p.isOwner);
                        obj = GetMonthWiseData(obj, p.Budget);
                        obj = GetMonthWiseDataForBudget(obj, p.BudgetMonth);
                        obj.MainBudgeted = p.MainBudgeted;
                        obj.CreatedBy = p.createdBy;
                        obj.isEditable = false;
                        obj.isAfterApproved = false;
                        model.Add(obj);
                        parentProgramId = p.id;
                        foreach (var t in p.tactic)
                        {
                            //// Set Tactic data to BudgetModel.
                            obj = new BudgetModel();
                            obj.Id = t.id.Replace("cpt_", "");
                            obj.ActivityId = t.id;
                            obj.ActivityName = t.title;
                            obj.ActivityType = ActivityType.ActivityTactic;
                            obj.ParentActivityId = parentProgramId;
                            obj.IsOwner = Convert.ToBoolean(t.isOwner);
                            obj.Budgeted = t.Cost;
                            obj = GetMonthWiseData(obj, t.Budget);
                            obj = GetMonthWiseDataForBudget(obj, t.BudgetMonth);
                            obj.CustomFieldEntities = t.CustomFieldEntities;
                            obj.MainBudgeted = t.MainBudgeted;
                            obj.CreatedBy = t.createdBy;
                            obj.isEditable = false;
                            obj.isAfterApproved = t.isAfterApproved;
                            model.Add(obj);
                            parentTacticId = t.id;
                            foreach (var l in t.lineitems)
                            {
                                //// Set line item data to BudgetModel.
                                obj = new BudgetModel();
                                obj.Id = l.id.Replace("cptl_", "");
                                obj.ActivityId = l.id;
                                obj.ActivityName = l.title;
                                obj.ActivityType = ActivityType.ActivityLineItem;
                                obj.ParentActivityId = parentTacticId;
                                obj.Budgeted = l.Cost;
                                obj.IsOwner = Convert.ToBoolean(l.isOwner);
                                obj = GetMonthWiseData(obj, l.Budget);
                                obj.ParentMonth = obj.Month;
                                obj.CreatedBy = l.createdBy;
                                obj.isEditable = false;
                                obj.isAfterApproved = t.isAfterApproved;
                                obj.LineItemTypeId = l.lineitemtype;
                                obj.BudgetMonth = new BudgetMonth();
                                model.Add(obj);
                            }
                        }
                    }
                }
            }

            //Added by Mitesh Vaishnav - set flag for editing campaign/program/tactic/plan 
            string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
            string EntityTypeTactic = Enums.EntityType.Tactic.ToString();
            bool isDisplayForFilter = false;
            bool IsCustomFeildExist = Common.IsCustomFeildExist(Enums.EntityType.Tactic.ToString(), Sessions.User.ClientId);
            var CustomFieldexists = db.CustomFields.Where(customfield => customfield.ClientId == Sessions.User.ClientId && customfield.EntityType.Equals(EntityTypeTactic) &&
                                                                        (customfield.IsRequired && !isDisplayForFilter) && customfield.IsDeleted.Equals(false)
                                                                         ).Any();
            List<string> tacIds = model.Where(t => t.ActivityType == EntityTypeTactic).Select(t => t.ActivityId).ToList();
            tacIds = tacIds.Select(t => t.Replace("cpt_", "")).ToList();
            List<int> intList = tacIds.ConvertAll(s => Int32.Parse(s));
            var Entities = db.CustomField_Entity.Where(entityid => intList.Contains(entityid.EntityId)).Select(entityid => entityid).ToList();

            var lstAllTacticCustomFieldEntities = db.CustomField_Entity.Where(customFieldEntity => customFieldEntity.CustomField.ClientId == Sessions.User.ClientId &&
                                                                                                        customFieldEntity.CustomField.IsDeleted.Equals(false) &&
                                                                                                        customFieldEntity.CustomField.EntityType.Equals(EntityTypeTactic) &&
                                                                                                        customFieldEntity.CustomField.CustomFieldType.Name.Equals(DropDownList) &&
                                                                                                        (isDisplayForFilter ? customFieldEntity.CustomField.IsDisplayForFilter.Equals(true) : true) &&
                                                                                                        intList.Contains(customFieldEntity.EntityId))
                                                                                                .Select(customFieldEntity => customFieldEntity).Distinct().ToList(); //todo : able to move up
            List<RevenuePlanner.Models.CustomRestriction> userCustomRestrictionList = Common.GetUserCustomRestrictionsList(Sessions.User.UserId, true);
            foreach (var item in model)
            {

                if (item.ActivityType == ActivityType.ActivityTactic)
                {
                    if (item.CreatedBy == Sessions.User.UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        planTacticIds.Add(Convert.ToInt32(item.ActivityId.Replace("cpt_", "")));
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
                        if (lstAllowedEntityIds.Count == planTacticIds.Count)
                        {
                            item.isEditable = true;
                        }
                        else
                        {
                            item.isEditable = false;
                        }
                    }
                }
                else if (item.ActivityType == ActivityType.ActivityLineItem)
                {
                    if (item.CreatedBy == Sessions.User.UserId)
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        planTacticIds.Add(Convert.ToInt32(item.ParentActivityId.Replace("cpt_", "")));
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
                        if (lstAllowedEntityIds.Count == planTacticIds.Count)
                        {
                            item.isEditable = true;
                        }
                        else
                        {
                            item.isEditable = false;
                        }

                    }
                }
            }
            //End Added by Mitesh Vaishnav - set flag for editing campaign/program/tactic/plan 

            int ViewByID = (int)viewBy;//_viewBy;
            //// if ViewBy is any CustomField then Create Model in this hierarchy level : Plan > CustomField > Campaign > Program > Tactic > LineItem
            if (ViewByID > 0)
            {
                string prefix = "custom_"; // Append Prefix to ActivityId for each model record.
                string prefixId = "";
                List<BudgetModel> modelCustom = new List<BudgetModel>();
                List<string> CampaingIds;
                List<CustomField_Entity> fltrModelCustomFieldEntities = new List<CustomField_Entity>(); // Contains list of CustomfieldEntities filtered by CustomFieldId(ViewBy) from Fielter BudgetModel list.
                string CustomFieldTypeName = db.CustomFields.Where(_customfield => _customfield.CustomFieldId.Equals(ViewByID) && _customfield.EntityType.Equals(entTacticType) && _customfield.ClientId.Equals(Sessions.User.ClientId) && _customfield.IsDeleted.Equals(false)).Select(_customfield => _customfield.CustomFieldType.Name).FirstOrDefault();

                #region "Plan Hierarchy - First Level"
                //Add plan BudgetModel to the Custom model.
                BudgetModel objPlanAud = model.Where(m => m.ActivityType == ActivityType.ActivityPlan).FirstOrDefault();
                modelCustom.Add(objPlanAud);
                #endregion

                #region "Get CustomFieldEntity list from filtered BudgetModel list"
                //// Create CustomFielEnities list(fltrModelCustomFieldEntities) based on selected CustomField.
                List<BudgetModel> fltrCustomFieldBudgetModel = model.Where(m => m.CustomFieldEntities != null && m.CustomFieldEntities.Count > 0 && m.ActivityType == ActivityType.ActivityTactic && m.CustomFieldEntities.Select(customentity => customentity.CustomFieldId).Contains(ViewByID)).ToList();
                List<CustomField_Entity> _custmfieldEntity;
                foreach (BudgetModel _model in fltrCustomFieldBudgetModel)
                {
                    _custmfieldEntity = new List<CustomField_Entity>();
                    _custmfieldEntity = _model.CustomFieldEntities.Where(s => s.CustomFieldId.Equals(ViewByID)).ToList();
                    if (_custmfieldEntity != null)
                        _custmfieldEntity.ForEach(_custmfieldentity => fltrModelCustomFieldEntities.Add(_custmfieldentity));
                }
                #endregion

                //// Customise BudgetModel based on CustomFieldType.
                if (CustomFieldTypeName.Equals(Enums.CustomFieldType.DropDownList.ToString()))
                {
                    #region "Create  Mapping list of CustomfieldOption with CustomfieldEntities"
                    List<entCustomFieldOption_EntityMapping> lstMapCustomFieldOption_EntityIds = new List<entCustomFieldOption_EntityMapping>(); // Mapping of each distinct CustomFieldOption with CustomFieldEntities. 
                    List<int> lstCustomFieldEntityIdsByOptionID = new List<int>();
                    List<string> DistCustomFieldOptionIDs = fltrModelCustomFieldEntities.Select(_ent => _ent.Value).Distinct().ToList(); // Get distinct CustomFieldOptionIds from filtered CustomFieldEntitylist.

                    //// Get distinct CustomFieldOption list by Ids.
                    List<CustomFieldOption> lstdistCustomFieldOptions = db.CustomFieldOptions.ToList().Where(_option => DistCustomFieldOptionIDs.ToList().Contains(_option.CustomFieldOptionId.ToString()) && _option.IsDeleted == false).ToList();
                    entCustomFieldOption_EntityMapping objOption_EntityMapping;
                    foreach (string OptionId in DistCustomFieldOptionIDs)
                    {
                        objOption_EntityMapping = new entCustomFieldOption_EntityMapping();
                        //// Get CustomFieldEntity list by OptionId.
                        lstCustomFieldEntityIdsByOptionID = fltrModelCustomFieldEntities.Where(_fltrEntity => _fltrEntity.Value.Equals(OptionId)).Distinct().Select(_fltrEntity => _fltrEntity.CustomFieldEntityId).ToList();
                        //// Set CustomFieldOptionId to Entity.
                        objOption_EntityMapping.CusotmFieldOptionId = !string.IsNullOrEmpty(OptionId) ? Convert.ToInt32(OptionId.ToString()) : 0;
                        //// Set CustomFieldEntities for distinct CustomFieldOptionId to Entity.
                        if (lstCustomFieldEntityIdsByOptionID != null)
                        {
                            objOption_EntityMapping.CusotmFieldEntityIds = lstCustomFieldEntityIdsByOptionID;
                            lstMapCustomFieldOption_EntityIds.Add(objOption_EntityMapping);
                        }
                    }
                    #endregion

                    List<BudgetModel> lstTactic, lstTacticModel, lstProgram, lstTactics, lstLines;
                    BudgetModel objProgram, objCampaign;
                    //// loop of each CustomFieldOption to create hierarchy level like this for Dropdownlist CustomFieldType: CustomField > Campaign > Program > Tactic > LineItem
                    foreach (entCustomFieldOption_EntityMapping custom_Option in lstMapCustomFieldOption_EntityIds)
                    {
                        #region "CustomField Hierarchy - Second Level"
                        //// Add CustomField data into BudgetModel to create CustomField level to Hierarchy.
                        obj = new BudgetModel();
                        obj.ActivityId = prefix + custom_Option.CusotmFieldOptionId;
                        obj.ActivityName = lstdistCustomFieldOptions.Where(_option => _option.CustomFieldOptionId.Equals(custom_Option.CusotmFieldOptionId)).Select(_option => _option.Value).FirstOrDefault();
                        obj.ActivityType = ActivityType.ActivityCustomField;
                        obj.ParentActivityId = objPlanAud.ActivityId;
                        obj.Budgeted = 0;
                        obj.IsOwner = true;
                        obj.Month = new BudgetMonth();
                        obj.BudgetMonth = new BudgetMonth();
                        modelCustom.Add(obj);
                        #endregion

                        parentCustomId = prefix + custom_Option.CusotmFieldOptionId; // Set ParentCustomId value using into child record. 
                        prefixId = "custom_" + custom_Option.CusotmFieldOptionId + "_"; // prefixId to append with it's child record ActivityId.

                        //Add all tactics and its line items
                        lstTactic = new List<BudgetModel>();

                        #region "List of Tactic those contains current CustomFieldEntity"
                        //// Get list of Tactics for those which contains current CustomFieldEntity.
                        lstTacticModel = new List<BudgetModel>();
                        foreach (int EntityId in custom_Option.CusotmFieldEntityIds)
                        {
                            lstTacticModel = model.Where(m => m.CustomFieldEntities != null && m.CustomFieldEntities.Count > 0 && m.ActivityType == ActivityType.ActivityTactic && m.CustomFieldEntities.Select(_entity => _entity.CustomFieldEntityId).Contains(EntityId)).Distinct().ToList();
                            if (lstTacticModel != null && lstTacticModel.Count > 0)
                                lstTactic.AddRange(lstTacticModel);
                        }
                        lstTactic = lstTactic.Distinct().ToList();
                        #endregion

                        #region "Create list of CampaignIds for each filtered Tactics"
                        CampaingIds = new List<string>();
                        foreach (BudgetModel objTactic in lstTactic)
                        {
                            objProgram = new BudgetModel();
                            objProgram = model.Where(m => m.ActivityId == objTactic.ParentActivityId && m.ActivityType == ActivityType.ActivityProgram).FirstOrDefault();
                            if (objProgram != null)
                            {
                                objCampaign = new BudgetModel();
                                objCampaign = model.Where(m => m.ActivityId == objProgram.ParentActivityId && m.ActivityType == ActivityType.ActivityCampaign).FirstOrDefault();
                                if (objCampaign != null)
                                {
                                    if (!CampaingIds.Contains(objCampaign.ActivityId))
                                    {
                                        CampaingIds.Add(objCampaign.ActivityId);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region " Create Campaign,Program,Tactic & LineItem hierarchy level "
                        //// Set Program related data for each Campaign Ids. 
                        foreach (string campaignid in CampaingIds)
                        {
                            #region "Campaign Hierarchy - Third Level"
                            objCampaign = new BudgetModel();
                            objCampaign = model.Where(_mdl => _mdl.ActivityId == campaignid && _mdl.ActivityType == ActivityType.ActivityCampaign).FirstOrDefault();
                            modelCustom.Add(GetClone(objCampaign, prefixId + objCampaign.ActivityId, parentCustomId));
                            #endregion

                            lstProgram = new List<BudgetModel>();
                            lstProgram = model.Where(_mdl => _mdl.ParentActivityId == campaignid && _mdl.ActivityType == ActivityType.ActivityProgram).ToList();
                            foreach (BudgetModel _objProgram in lstProgram)
                            {
                                lstTactics = new List<BudgetModel>();
                                lstTactics = lstTactic.Where(m => m.ParentActivityId == _objProgram.ActivityId).ToList();
                                lstTactics.ForEach(mdlTactic => mdlTactic.CustomFieldType = Enums.CustomFieldType.DropDownList.ToString());
                                if (lstTactics != null && lstTactics.Count() > 0)
                                {
                                    #region "Program Hierarchy - Fourth Level"
                                    modelCustom.Add(GetClone(_objProgram, prefixId + _objProgram.ActivityId, prefixId + _objProgram.ParentActivityId));
                                    #endregion

                                    foreach (BudgetModel objT in lstTactics)
                                    {
                                        #region "Tactic Hierarchy - Fifth Level"
                                        modelCustom.Add(GetClone(objT, prefixId + objT.ActivityId, prefixId + objT.ParentActivityId));
                                        #endregion
                                        lstLines = new List<BudgetModel>();
                                        lstLines = model.Where(_mdl => _mdl.ParentActivityId == objT.ActivityId && _mdl.ActivityType == ActivityType.ActivityLineItem).ToList();
                                        foreach (BudgetModel objL in lstLines)
                                        {
                                            #region "LineItem Hierarchy - Sixth Level"
                                            modelCustom.Add(GetClone(objL, prefixId + objL.ActivityId, prefixId + objL.ParentActivityId));
                                            #endregion
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    List<BudgetModel> lstTactic, lstProgram, lstTactics, lstLines;
                    BudgetModel objProgram, objCampaign;
                    //// loop of each CustomFieldEntity to create hierarchy level like this for Textbox CustomFieldType: CustomField > Campaign > Program > Tactic > LineItem
                    foreach (CustomField_Entity custmEntity in fltrModelCustomFieldEntities)
                    {
                        #region "CustomField Hierarchy - Second Level"
                        //// Add CustomField data into BudgetModel to create CustomField level to Hierarchy.
                        obj = new BudgetModel();
                        int objId = custmEntity.CustomFieldEntityId;
                        obj.ActivityId = prefix + custmEntity.CustomFieldEntityId;
                        obj.ActivityName = custmEntity.Value;
                        obj.ActivityType = ActivityType.ActivityCustomField;
                        obj.ParentActivityId = objPlanAud.ActivityId;
                        obj.Budgeted = 0;
                        obj.IsOwner = true;
                        obj.Month = new BudgetMonth();
                        modelCustom.Add(obj);
                        #endregion

                        parentCustomId = prefix + custmEntity.CustomFieldEntityId; // Set ParentCustomId value using into child record. 
                        prefixId = "customText_" + custmEntity.CustomFieldEntityId + "_"; // prefixId to append with it's child record ActivityId.

                        //Add all tactics and its line items
                        lstTactic = new List<BudgetModel>();
                        lstTactic = fltrCustomFieldBudgetModel.Where(_mdl => _mdl.CustomFieldEntities.Select(_ent => _ent.CustomFieldEntityId).ToList().Contains(custmEntity.CustomFieldEntityId)).ToList();

                        #region "Create list of CampaignIds for each filtered Tactics"
                        CampaingIds = new List<string>();
                        foreach (BudgetModel objTactic in lstTactic)
                        {
                            objProgram = new BudgetModel();
                            objProgram = model.Where(m => m.ActivityId == objTactic.ParentActivityId && m.ActivityType == ActivityType.ActivityProgram).FirstOrDefault();
                            if (objProgram != null)
                            {
                                objCampaign = new BudgetModel();
                                objCampaign = model.Where(m => m.ActivityId == objProgram.ParentActivityId && m.ActivityType == ActivityType.ActivityCampaign).FirstOrDefault();
                                if (objCampaign != null)
                                {
                                    if (!CampaingIds.Contains(objCampaign.ActivityId))
                                    {
                                        CampaingIds.Add(objCampaign.ActivityId);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region " Create Campaign,Program,Tactic & LineItem hierarchy level "
                        //// Set Program related data for each Campaign Ids. 
                        foreach (string campaingid in CampaingIds)
                        {
                            objCampaign = new BudgetModel();
                            objCampaign = model.Where(_mdl => _mdl.ActivityId == campaingid && _mdl.ActivityType == ActivityType.ActivityCampaign).FirstOrDefault();
                            modelCustom.Add(GetClone(objCampaign, prefixId + objCampaign.ActivityId, parentCustomId));
                            lstProgram = new List<BudgetModel>();
                            lstProgram = model.Where(_mdl => _mdl.ParentActivityId == campaingid && _mdl.ActivityType == ActivityType.ActivityProgram).ToList();
                            foreach (BudgetModel _objProgram in lstProgram)
                            {
                                lstTactics = new List<BudgetModel>();
                                lstTactics = lstTactic.Where(_mdl => _mdl.ParentActivityId == _objProgram.ActivityId).ToList();
                                lstTactics.ForEach(mdlTactic => mdlTactic.CustomFieldType = Enums.CustomFieldType.TextBox.ToString());
                                if (lstTactics.Count() > 0)
                                {
                                    modelCustom.Add(GetClone(_objProgram, prefixId + _objProgram.ActivityId, prefixId + _objProgram.ParentActivityId));
                                    foreach (BudgetModel objT in lstTactics)
                                    {
                                        lstLines = new List<BudgetModel>();
                                        lstLines = model.Where(_mdl => _mdl.ParentActivityId == objT.ActivityId && _mdl.ActivityType == ActivityType.ActivityLineItem).ToList();
                                        modelCustom.Add(GetClone(objT, prefixId + objT.ActivityId, prefixId + objT.ParentActivityId));
                                        foreach (BudgetModel objL in lstLines)
                                        {
                                            modelCustom.Add(GetClone(objL, prefixId + objL.ActivityId, prefixId + objL.ParentActivityId));
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
                model = modelCustom;
            }

            //Set actual for quarters
            if (AllocatedBy == "quarters")  // Modified by Sohel Pathan on 08/09/2014 for PL ticket #642.
            {
                foreach (BudgetModel bm in model)
                {
                    if (bm.ActivityType == ActivityType.ActivityLineItem || bm.ActivityType == ActivityType.ActivityTactic)
                    {
                        bm.Month.Jan = bm.Month.Jan + bm.Month.Feb + bm.Month.Mar;
                        bm.Month.Apr = bm.Month.Apr + bm.Month.May + bm.Month.Jun;
                        bm.Month.Jul = bm.Month.Jul + bm.Month.Aug + bm.Month.Sep;
                        bm.Month.Oct = bm.Month.Oct + bm.Month.Nov + bm.Month.Dec;
                        bm.Month.Feb = 0;
                        bm.Month.Mar = 0;
                        bm.Month.May = 0;
                        bm.Month.Jun = 0;
                        bm.Month.Aug = 0;
                        bm.Month.Sep = 0;
                        bm.Month.Nov = 0;
                        bm.Month.Dec = 0;
                    }

                    if (bm.ActivityType != ActivityType.ActivityLineItem)
                    {
                        bm.BudgetMonth.Jan = bm.BudgetMonth.Jan + bm.BudgetMonth.Feb + bm.BudgetMonth.Mar;
                        bm.BudgetMonth.Apr = bm.BudgetMonth.Apr + bm.BudgetMonth.May + bm.BudgetMonth.Jun;
                        bm.BudgetMonth.Jul = bm.BudgetMonth.Jul + bm.BudgetMonth.Aug + bm.BudgetMonth.Sep;
                        bm.BudgetMonth.Oct = bm.BudgetMonth.Oct + bm.BudgetMonth.Nov + bm.BudgetMonth.Dec;
                        bm.BudgetMonth.Feb = 0;
                        bm.BudgetMonth.Mar = 0;
                        bm.BudgetMonth.May = 0;
                        bm.BudgetMonth.Jun = 0;
                        bm.BudgetMonth.Aug = 0;
                        bm.BudgetMonth.Sep = 0;
                        bm.BudgetMonth.Nov = 0;
                        bm.BudgetMonth.Dec = 0;
                    }
                }
            }
            //Threre is no need to manage lines for actuals
            if (budgetTab == Enums.BudgetTab.Planned)
            {
                model = ManageLineItems(model);
            }
            ViewBag.AllocatedBy = AllocatedBy;
            ViewBag.ViewBy = ViewByID;//(int)viewBy;
            ViewBag.Tab = (int)budgetTab;

            #region "Calculate Monthly Budget from Bottom to Top for Hierarchy level like: LineItem > Tactic > Program > Campaign > CustomField(if filtered) > Plan"


            //// Set ViewBy data to model.
            //// Calculate monthly Tactic budget from it's child budget i.e LineItem
            model = CalculateBottomUp(model, ActivityType.ActivityTactic, ActivityType.ActivityLineItem, budgetTab);

            //// Calculate monthly Program budget from it's child budget i.e Tactic
            model = CalculateBottomUp(model, ActivityType.ActivityProgram, ActivityType.ActivityTactic, budgetTab);

            //// Calculate monthly Campaign budget from it's child budget i.e Program
            model = CalculateBottomUp(model, ActivityType.ActivityCampaign, ActivityType.ActivityProgram, budgetTab);

            //// Customize BudgetModel based on ViewBy selection value.
            if (ViewByID > 0)   // if viewby is customfield.
            {
                //// Calculate monthly CustomField budget from it's child budget i.e Campaign
                model = CalculateBottomUp(model, ActivityType.ActivityCustomField, ActivityType.ActivityCampaign, budgetTab);
                //// Calculate monthly Plan budget from it's child budget i.e CustomField
                model = CalculateBottomUp(model, ActivityType.ActivityPlan, ActivityType.ActivityCustomField, budgetTab);
            }
            else
            {
                //// Calculate monthly Plan budget from it's child budget i.e Campaign
                model = CalculateBottomUp(model, ActivityType.ActivityPlan, ActivityType.ActivityCampaign, budgetTab);
            }
            #endregion

            //// Set LineItem monthly budget cost by it's parent tactic weightage.
            model = SetLineItemCostByWeightage(model);

            #region "Calculate header monthly allocated Percentage values"
            BudgetMonth a = new BudgetMonth();
            BudgetMonth child = new BudgetMonth();
            BudgetMonth PercAllocated = new BudgetMonth();
            child = model.Where(_mdl => _mdl.ActivityType == ActivityType.ActivityPlan).Select(_mdl => _mdl.Month).FirstOrDefault();
            a = model.Where(_mdl => _mdl.ActivityType == ActivityType.ActivityPlan).Select(_mdl => _mdl.ParentMonth).FirstOrDefault();

            PercAllocated.Jan = (a.Jan == 0 && child.Jan == 0) ? 0 : (a.Jan == 0 && child.Jan > 0) ? 101 : child.Jan / a.Jan * 100;
            PercAllocated.Feb = (a.Feb == 0 && child.Feb == 0) ? 0 : (a.Feb == 0 && child.Feb > 0) ? 101 : child.Feb / a.Feb * 100;
            PercAllocated.Mar = (a.Mar == 0 && child.Mar == 0) ? 0 : (a.Mar == 0 && child.Mar > 0) ? 101 : child.Mar / a.Mar * 100;
            PercAllocated.Apr = (a.Apr == 0 && child.Apr == 0) ? 0 : (a.Apr == 0 && child.Apr > 0) ? 101 : child.Apr / a.Apr * 100;
            PercAllocated.May = (a.May == 0 && child.May == 0) ? 0 : (a.May == 0 && child.May > 0) ? 101 : child.May / a.May * 100;
            PercAllocated.Jun = (a.Jun == 0 && child.Jun == 0) ? 0 : (a.Jun == 0 && child.Jun > 0) ? 101 : child.Jun / a.Jun * 100;
            PercAllocated.Jul = (a.Jul == 0 && child.Jul == 0) ? 0 : (a.Jul == 0 && child.Jul > 0) ? 101 : child.Jul / a.Jul * 100;
            PercAllocated.Aug = (a.Aug == 0 && child.Aug == 0) ? 0 : (a.Aug == 0 && child.Aug > 0) ? 101 : child.Aug / a.Aug * 100;
            PercAllocated.Sep = (a.Sep == 0 && child.Sep == 0) ? 0 : (a.Sep == 0 && child.Sep > 0) ? 101 : child.Sep / a.Sep * 100;
            PercAllocated.Oct = (a.Oct == 0 && child.Oct == 0) ? 0 : (a.Oct == 0 && child.Oct > 0) ? 101 : child.Oct / a.Oct * 100;
            PercAllocated.Nov = (a.Nov == 0 && child.Nov == 0) ? 0 : (a.Nov == 0 && child.Nov > 0) ? 101 : child.Nov / a.Nov * 100;
            PercAllocated.Dec = (a.Dec == 0 && child.Dec == 0) ? 0 : (a.Dec == 0 && child.Dec > 0) ? 101 : child.Dec / a.Dec * 100;

            ViewBag.PercAllocated = PercAllocated;
            #endregion

            Sessions.PlanId = PlanId;
            return PartialView("_Budget", model);
        }

        /// <summary>
        /// Create Clone.
        /// </summary>
        /// <param name="obj">BudgetModel object</param>
        /// <param name="Id">ActivityId</param>
        /// <param name="ParentId">ParentActivityId</param>
        /// <returns>Return BudgetModel clone.</returns>
        private BudgetModel GetClone(BudgetModel obj, string Id, string ParentId)
        {
            BudgetModel tmp = new BudgetModel();
            tmp.ActivityId = Id;
            tmp.ParentActivityId = ParentId;
            tmp.ActivityName = obj.ActivityName;
            tmp.ActivityType = obj.ActivityType;
            tmp.Allocated = obj.Allocated;
            tmp.MainBudgeted = obj.MainBudgeted;
            if (obj.ActivityType.Equals(ActivityType.ActivityTactic))
            {
                //// if CustomFieldType is Dropdownlist then retrieve weightage from CustomFieldEntity or StageWeight table O/W take default 100% for Textbox type.
                if (obj.CustomFieldType.Equals(Enums.CustomFieldType.DropDownList.ToString()))
                {
                    string[] strTactic = Id.Split('_');
                    int weightage = 0;
                    if (strTactic != null && strTactic.Length > 0)
                    {
                        string CustomfieldOptionId = strTactic[1] != null ? strTactic[1] : string.Empty; // Get CustomfieldOptionId from Tactic ActivityId.
                        int TacticId = strTactic[3] != null ? int.Parse(strTactic[3]) : 0; // Get PlanTacticId from Tactic ActivityId.
                        if (obj.CustomFieldEntities != null && obj.CustomFieldEntities.Count > 0)
                        {
                            //// Get CustomFieldEntity based on EntityId and CustomFieldOptionId from CustomFieldEntities.
                            var _custment = obj.CustomFieldEntities.Where(_ent => _ent.EntityId.Equals(TacticId) && _ent.Value.Equals(CustomfieldOptionId)).FirstOrDefault();
                            if (_custment == null)
                                weightage = 0;
                            else if (_custment.CostWeightage != null && Convert.ToInt32(_custment.CostWeightage.Value) > 0) // Get CostWeightage from table CustomFieldEntity.
                                weightage = Convert.ToInt32(_custment.CostWeightage.Value);
                            //else
                            //{
                            //    string constCostStageTitle = Enums.InspectStage.Cost.ToString();
                            //    var stgweightage = db.CustomField_Entity_StageWeight.Where(_stageweight => _stageweight.CustomFieldEntityId.Equals(_custment.CustomFieldEntityId) && _stageweight.StageTitle.Equals(constCostStageTitle)).Select(_stageweight => _stageweight.Weightage).FirstOrDefault();
                            //    weightage = stgweightage != null ? stgweightage : 0;
                            //}
                        }
                    }
                    tmp.Weightage = weightage;
                }
                else
                {
                    tmp.Weightage = 100;
                }
            }
            tmp.Id = obj.Id;
            tmp.IsOwner = obj.IsOwner;
            tmp.Month = obj.Month;
            tmp.ParentMonth = obj.ParentMonth;
            tmp.SumMonth = obj.SumMonth;
            tmp.BudgetMonth = obj.BudgetMonth;
            return tmp;
        }

        /// <summary>
        /// Calculate the bottom up planeed cost
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ParentActivityType"></param>
        /// <param name="ChildActivityType"></param>
        /// <param name="budgetTab"></param>
        /// <returns></returns>
        public List<BudgetModel> CalculateBottomUp(List<BudgetModel> model, string ParentActivityType, string ChildActivityType, Enums.BudgetTab budgetTab)
        {
            int _ViewById = ViewBag.ViewBy != null ? (int)ViewBag.ViewBy : 0;
            int weightage = 100;

            if (budgetTab == Enums.BudgetTab.Actual && ParentActivityType == ActivityType.ActivityTactic)
            {
                List<BudgetModel> LineCheck;
                foreach (BudgetModel l in model.Where(_mdl => _mdl.ActivityType == ParentActivityType))
                {
                    LineCheck = new List<BudgetModel>();
                    LineCheck = model.Where(lines => lines.ParentActivityId == l.ActivityId && lines.ActivityType == ActivityType.ActivityLineItem).ToList();
                    if (LineCheck.Count() > 0)
                    {
                        //// check if ViewBy is Campaign selected then set weightage value to 100;
                        if (_ViewById > 0)
                            weightage = l.Weightage;

                        BudgetMonth parent = new BudgetMonth();
                        parent.Jan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Jan * weightage) / 100) ?? 0;
                        parent.Feb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Feb * weightage) / 100) ?? 0;
                        parent.Mar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Mar * weightage) / 100) ?? 0;
                        parent.Apr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Apr * weightage) / 100) ?? 0;
                        parent.May = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.May * weightage) / 100) ?? 0;
                        parent.Jun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Jun * weightage) / 100) ?? 0;
                        parent.Jul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Jul * weightage) / 100) ?? 0;
                        parent.Aug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Aug * weightage) / 100) ?? 0;
                        parent.Sep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Sep * weightage) / 100) ?? 0;
                        parent.Oct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Oct * weightage) / 100) ?? 0;
                        parent.Nov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Nov * weightage) / 100) ?? 0;
                        parent.Dec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Dec * weightage) / 100) ?? 0;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonth = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().Month;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().Month = parent;
                    }
                    else
                    {
                        model.Where(_mdl => _mdl.ActivityId == l.ActivityId).FirstOrDefault().ParentMonth = model.Where(_mdl => _mdl.ActivityId == l.ActivityId).FirstOrDefault().Month;
                    }
                }
            }
            else
            {
                BudgetMonth parent;
                foreach (BudgetModel l in model.Where(l => l.ActivityType == ParentActivityType))
                {
                    parent = new BudgetMonth();
                    if (ParentActivityType.Equals(ActivityType.ActivityTactic))
                    {
                        //// check if ViewBy is Campaign selected then set weightage value to 100;
                        if (_ViewById > 0)
                            weightage = l.Weightage;

                        parent.Jan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Jan * weightage) / 100) ?? 0;
                        parent.Feb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Feb * weightage) / 100) ?? 0;
                        parent.Mar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Mar * weightage) / 100) ?? 0;
                        parent.Apr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Apr * weightage) / 100) ?? 0;
                        parent.May = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.May * weightage) / 100) ?? 0;
                        parent.Jun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Jun * weightage) / 100) ?? 0;
                        parent.Jul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Jul * weightage) / 100) ?? 0;
                        parent.Aug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Aug * weightage) / 100) ?? 0;
                        parent.Sep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Sep * weightage) / 100) ?? 0;
                        parent.Oct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Oct * weightage) / 100) ?? 0;
                        parent.Nov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Nov * weightage) / 100) ?? 0;
                        parent.Dec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.Month.Dec * weightage) / 100) ?? 0;
                    }
                    else
                    {
                        parent.Jan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Month.Jan) ?? 0;
                        parent.Feb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Month.Feb) ?? 0;
                        parent.Mar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Month.Mar) ?? 0;
                        parent.Apr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Month.Apr) ?? 0;
                        parent.May = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Month.May) ?? 0;
                        parent.Jun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Month.Jun) ?? 0;
                        parent.Jul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Month.Jul) ?? 0;
                        parent.Aug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Month.Aug) ?? 0;
                        parent.Sep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Month.Sep) ?? 0;
                        parent.Oct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Month.Oct) ?? 0;
                        parent.Nov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Month.Nov) ?? 0;
                        parent.Dec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Month.Dec) ?? 0;

                        model.Where(_mdl => _mdl.ActivityId == l.ActivityId).FirstOrDefault().Allocated = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.Allocated) ?? 0;
                    }
                    model.Where(_mdl => _mdl.ActivityId == l.ActivityId).FirstOrDefault().ParentMonth = model.Where(_mdl => _mdl.ActivityId == l.ActivityId).FirstOrDefault().Month;
                    model.Where(_mdl => _mdl.ActivityId == l.ActivityId).FirstOrDefault().Month = parent;
                }
            }
            return model;
        }

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

        /// <summary>
        /// Action to Get Program Budget allocation data.
        /// </summary>
        /// <param name="CampaignId"></param>
        /// <param name="PlanProgramId"></param>
        /// <returns></returns>
        public JsonResult GetBudgetAllocationProgrmaData(int CampaignId, int PlanProgramId)
        {
            try
            {
                List<string> lstMonthly = Common.lstMonthly;
                var objPlanCampaign = db.Plan_Campaign.FirstOrDefault(_camp => _camp.PlanCampaignId == CampaignId && _camp.IsDeleted == false);    // Modified by Sohel Pathan on 04/09/2014 for PL ticket #758

                var objPlan = db.Plans.FirstOrDefault(_pln => _pln.PlanId == objPlanCampaign.PlanId && _pln.IsDeleted == false);  //Sessions.PlanId  // Modified by Sohel Pathan on 04/09/2014 for PL ticket #758

                var lstSelectedProgram = db.Plan_Campaign_Program.Where(_prgrm => _prgrm.PlanCampaignId == CampaignId && _prgrm.IsDeleted == false).ToList();

                var lstPlanProgramTactics = db.Plan_Campaign_Program_Tactic.Where(_tac => _tac.PlanProgramId == PlanProgramId && _tac.IsDeleted == false).Select(_tac => _tac).ToList();  // Modified by :- Sohel Pathan on 01/09/2014 for PL ticket #745

                var lstPlanProgramTacticsId = lstPlanProgramTactics.Select(_tac => _tac.PlanTacticId);
                // Add By Nishant Sheth 
                // Desc:: for add multiple years regarding #1765
                // To create the period of the year dynamically base on tactic period
                int GlobalYearDiffrence = 0;
                List<listMonthDynamic> lstMonthlyDynamicProgram = new List<listMonthDynamic>();
                List<listMonthDynamic> lstMonthlyDynamicCampaign = new List<listMonthDynamic>();
                List<listMonthDynamic> lstMonthlyDynamicTactic = new List<listMonthDynamic>();
                lstSelectedProgram.ForEach(program =>
                {
                    List<string> lstMonthlyExtended = new List<string>();
                    int YearDiffrence = Convert.ToInt32(Convert.ToInt32(program.EndDate.Year) - Convert.ToInt32(program.StartDate.Year));
                    if (program.PlanProgramId == PlanProgramId)
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
                    lstMonthlyDynamicProgram.Add(new listMonthDynamic { Id = program.PlanProgramId, listMonthly = lstMonthlyExtended });
                });

                if (objPlanCampaign != null)
                {
                    List<string> lstMonthlyExtended = new List<string>();
                    int YearDiffrence = Convert.ToInt32(Convert.ToInt32(objPlanCampaign.EndDate.Year) - Convert.ToInt32(objPlanCampaign.StartDate.Year));

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
                    lstMonthlyDynamicCampaign.Add(new listMonthDynamic { Id = objPlanCampaign.PlanCampaignId, listMonthly = lstMonthlyExtended });
                }

                lstPlanProgramTactics.ForEach(tactic =>
                {
                    List<string> lstMonthlyExtended = new List<string>();
                    int YearDiffrence = Convert.ToInt32(Convert.ToInt32(tactic.EndDate.Year) - Convert.ToInt32(tactic.StartDate.Year));

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
                    lstMonthlyDynamicTactic.Add(new listMonthDynamic { Id = tactic.PlanTacticId, listMonthly = lstMonthlyExtended });
                });

                var planProgramIds = lstSelectedProgram.Select(_prgrm => _prgrm.PlanProgramId);

                // Change by Nishant sheth
                // Desc :: #1765 - add period condition to get value
                var lstCampaignBudget = db.Plan_Campaign_Budget.Where(_budgt => _budgt.PlanCampaignId == CampaignId).ToList()
                    .Where(_budget => lstMonthlyDynamicCampaign.Where(a => a.Id == _budget.PlanCampaignId).Select(a => a.listMonthly).FirstOrDefault().Contains(_budget.Period))
                                                               .Select(_budgt => new
                                                               {
                                                                   _budgt.PlanCampaignBudgetId,
                                                                   _budgt.PlanCampaignId,
                                                                   _budgt.Period,
                                                                   _budgt.Value
                                                               }).ToList();

                // Change by Nishant sheth
                // Desc :: #1765 - add period condition to get value
                var lstProgramBudget = db.Plan_Campaign_Program_Budget.Where(_budgt => planProgramIds.Contains(_budgt.PlanProgramId)).ToList()
                    .Where(_budget => lstMonthlyDynamicProgram.Where(a => a.Id == _budget.PlanProgramId).Select(a => a.listMonthly).FirstOrDefault().Contains(_budget.Period))
                                                               .Select(_budgt => new
                                                               {
                                                                   _budgt.PlanProgramBudgetId,
                                                                   _budgt.PlanProgramId,
                                                                   _budgt.Period,
                                                                   _budgt.Value
                                                               }).ToList();


                // Change by Nishant sheth
                // Desc :: #1765 - add period condition to get value
                var lstTacticsBudget = db.Plan_Campaign_Program_Tactic_Cost.Where(_tacCost => lstPlanProgramTacticsId.Contains(_tacCost.PlanTacticId)).ToList()
                    .Where(_tacCost => lstMonthlyDynamicTactic.Where(a => a.Id == _tacCost.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(_tacCost.Period))
                                                               .Select(_tacCost => new
                                                               {
                                                                   _tacCost.PlanTacticBudgetId,
                                                                   _tacCost.PlanTacticId,
                                                                   _tacCost.Period,
                                                                   _tacCost.Value
                                                               }).ToList();

                double allCampaignBudget = lstCampaignBudget.Sum(_cmpgnBudgt => _cmpgnBudgt.Value);
                double allProgramBudget = lstSelectedProgram.Sum(_prgrmBudgt => _prgrmBudgt.ProgramBudget);
                double planRemainingBudget = (objPlanCampaign.CampaignBudget - allProgramBudget);

                if (objPlan.AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString())
                {
                    //// Set Quarterly Budget Allocation value.
                    List<PlanBudgetAllocationValue> lstPlanBudgetAllocationValue = new List<PlanBudgetAllocationValue>();
                    string[] quarterPeriods = Common.quarterPeriods;
                    for (int i = 0; i < (12 * (GlobalYearDiffrence + 1)); i++)
                    {
                        if ((i + 1) % 3 == 0)
                        {
                            PlanBudgetAllocationValue objPlanBudgetAllocationValue = new PlanBudgetAllocationValue();
                            objPlanBudgetAllocationValue.periodTitle = PeriodChar + (i - 1).ToString();
                            objPlanBudgetAllocationValue.budgetValue = lstProgramBudget.Where(c => quarterPeriods.Contains(c.Period) && c.PlanProgramId == PlanProgramId).FirstOrDefault() == null ? "" : lstProgramBudget.Where(c => quarterPeriods.Contains(c.Period) && c.PlanProgramId == PlanProgramId).Select(a => a.Value).Sum().ToString();
                            objPlanBudgetAllocationValue.remainingMonthlyBudget = lstCampaignBudget.Where(p => quarterPeriods.Contains(p.Period)).FirstOrDefault() == null ? 0 : lstCampaignBudget.Where(p => quarterPeriods.Contains(p.Period)).Sum(a => a.Value) - (lstProgramBudget.Where(c => quarterPeriods.Contains(c.Period)).Sum(c => c.Value));
                            objPlanBudgetAllocationValue.programMonthlyBudget = lstTacticsBudget.Where(c => quarterPeriods.Contains(c.Period)).Sum(c => c.Value);

                            /// Add into return list
                            lstPlanBudgetAllocationValue.Add(objPlanBudgetAllocationValue);

                            quarterPeriods = new string[] { PeriodChar + (i + 2), PeriodChar + (i + 3), PeriodChar + (i + 4) };
                        }
                    }

                    var objBudgetAllocationData = new { budgetData = lstPlanBudgetAllocationValue, planRemainingBudget = planRemainingBudget };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // Change by Nishant sheth
                    // Desc :: #1765 - add period condition to get value
                    var budgetData = lstMonthlyDynamicProgram.Where(a => a.Id == PlanProgramId).Select(a => a.listMonthly).FirstOrDefault().Select(period => new
                    {
                        periodTitle = period,
                        budgetValue = lstProgramBudget.FirstOrDefault(c => c.Period == period && c.PlanProgramId == PlanProgramId) == null ? "" : lstProgramBudget.FirstOrDefault(c => c.Period == period && c.PlanProgramId == PlanProgramId).Value.ToString(),
                        remainingMonthlyBudget = (lstCampaignBudget.FirstOrDefault(p => p.Period == period) == null ? 0 : lstCampaignBudget.FirstOrDefault(p => p.Period == period).Value) - (lstProgramBudget.Where(c => c.Period == period).Sum(c => c.Value)),
                        programMonthlyBudget = lstTacticsBudget.Where(c => c.Period == period).Sum(c => c.Value)
                    });

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
        /// Added  By : Kalpesh Sharma PL #605
        /// </summary>
        /// <param name="PlanProgramId"></param>
        /// <param name="PlanTacticId"></param>
        /// <returns></returns>
        public JsonResult GetBudgetAllocationTactics(int PlanProgramId, int PlanTacticId)
        {
            try
            {
                List<string> lstMonthly = Common.lstMonthly;


                var lstTactic = db.Plan_Campaign_Program_Tactic.Where(_tac => _tac.PlanProgramId == PlanProgramId && _tac.IsDeleted == false)
                    .ToList();

                // Add By Nishant Sheth 
                // Desc:: for add multiple years regarding #1765
                // To create the period of the year dynamically base on tactic period
                int GlobalYearDiffrence = 0;
                List<listMonthDynamic> lstMonthlyDynamic = new List<listMonthDynamic>();
                lstTactic.ForEach(tactic =>
                {
                    List<string> lstMonthlyExtended = new List<string>();
                    int YearDiffrence = Convert.ToInt32(Convert.ToInt32(tactic.EndDate.Year) - Convert.ToInt32(tactic.StartDate.Year));
                    if (tactic.PlanTacticId == PlanTacticId)
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
                    lstMonthlyDynamic.Add(new listMonthDynamic { Id = tactic.PlanTacticId, listMonthly = lstMonthlyExtended });
                });

                ////// start-Added by Mitesh Vaishnav for PL ticket #571
                //// Actual cost portion added exact under "lstMonthly" array because Actual cost portion is independent from the monthly/quarterly selection made by the user at the plan level.
                var tacticLineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == PlanTacticId && l.IsDeleted == false).Select(l => l.PlanLineItemId).ToList();
                bool isLineItemForTactic = false;////flag for line items count of tactic.If tactic has any line item than flag set to true else false
                List<Plan_Campaign_Program_Tactic_LineItem_Actual> actualCostAllocationData = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                if (tacticLineItemList.Count == 0)
                {
                    ////object for filling input of Actual Cost Allocation
                    string costTitle = Enums.InspectStage.Cost.ToString();
                    // Change by Nishant sheth
                    // Desc :: #1765 - add period condition to get value
                    actualCostAllocationData = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => ta.PlanTacticId == PlanTacticId && ta.StageTitle == costTitle)
                        .ToList().Select(ta => new Plan_Campaign_Program_Tactic_LineItem_Actual
                    {
                        PlanLineItemId = 0,
                        Period = ta.Period,
                        Value = ta.Actualvalue
                    }).ToList()
                    .Where(ta => lstMonthlyDynamic.Where(a => a.Id == ta.Plan_Campaign_Program_Tactic_LineItem.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(ta.Period))
                    .ToList();
                }
                else
                {
                    // Change by Nishant sheth
                    // Desc :: #1765 - add period condition to get value
                    var actualLineItem = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(al => tacticLineItemList.Contains(al.PlanLineItemId)).ToList()
                        .Where(al => lstMonthlyDynamic.Where(a => a.Id == al.Plan_Campaign_Program_Tactic_LineItem.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(al.Period))
                        .ToList();
                    ////object for filling input of Actual Cost Allocation
                    actualCostAllocationData = lstMonthlyDynamic.Where(a => a.Id == PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Select(m => new Plan_Campaign_Program_Tactic_LineItem_Actual
                    {
                        PlanLineItemId = 0,
                        Period = m,
                        Value = actualLineItem.Where(al => al.Period == m).Sum(al => al.Value)
                    }).ToList();
                    isLineItemForTactic = true;
                }
                //// End-Added by Mitesh Vaishnav for PL ticket #571

                int planid = db.Plan_Campaign_Program.Where(p => p.PlanProgramId == PlanProgramId && p.IsDeleted.Equals(false)).Select(p => p.Plan_Campaign.PlanId).FirstOrDefault();

                var objPlan = db.Plans.FirstOrDefault(_pln => _pln.PlanId == planid && _pln.IsDeleted == false);   // Modified by Sohel Pathan on 04/09/2014 for PL ticket #759

                var lstSelectedProgram = db.Plan_Campaign_Program.Where(_prgrm => _prgrm.PlanProgramId == PlanProgramId && _prgrm.IsDeleted == false).ToList();

                var planProgramIds = lstSelectedProgram.Select(_prgrm => _prgrm.PlanProgramId);
                // Change by Nishant sheth
                // Desc :: #1765 - add period condition to get value
                var lstProgramBudget = db.Plan_Campaign_Program_Budget.Where(_prgBdgt => planProgramIds.Contains(_prgBdgt.PlanProgramId)).ToList()
                    .Where(_prgBdgt => lstMonthlyDynamic.Where(a => a.Id == PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(_prgBdgt.Period))
                                                               .Select(_prgBdgt => new
                                                               {
                                                                   _prgBdgt.PlanProgramBudgetId,
                                                                   _prgBdgt.PlanProgramId,
                                                                   _prgBdgt.Period,
                                                                   _prgBdgt.Value
                                                               }).ToList();

                //var lstTactic = db.Plan_Campaign_Program_Tactic.Where(_tac => _tac.PlanProgramId == PlanProgramId && _tac.IsDeleted == false).ToList();
                var lstPlanProgramTactics = lstTactic.Select(_tac => _tac.PlanTacticId).ToList();  // Modified by :- Sohel Pathan on 01/09/2014 for PL ticket #745

                var CostTacticsBudget = lstTactic.Sum(c => c.TacticBudget); // Modified by :- Sohel Pathan on 01/09/2014 for PL ticket #745
                // Change by Nishant sheth
                // Desc :: #1765 - add period condition to get value
                var lstTacticsBudget = db.Plan_Campaign_Program_Tactic_Budget.Where(_tacCost => lstPlanProgramTactics.Contains(_tacCost.PlanTacticId)).ToList()
                    .Where(_tacCost => lstMonthlyDynamic.Where(a => a.Id == _tacCost.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(_tacCost.Period))
                                                               .Select(_tacCost => new
                                                               {
                                                                   _tacCost.PlanTacticBudgetId,
                                                                   _tacCost.PlanTacticId,
                                                                   _tacCost.Period,
                                                                   _tacCost.Value
                                                               }).ToList();
                // Change by Nishant sheth
                // Desc :: #1765 - add period condition to get value
                var lstTacticsLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lineCost => tacticLineItemList.Contains(lineCost.PlanLineItemId)).ToList()
                    .Where(lineCost => lstMonthlyDynamic.Where(a => a.Id == lineCost.Plan_Campaign_Program_Tactic_LineItem.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(lineCost.Period))
                                                               .Select(lineCost => new
                                                               {
                                                                   lineCost.PlanLineItemBudgetId,
                                                                   lineCost.PlanLineItemId,
                                                                   lineCost.Period,
                                                                   lineCost.Value
                                                               }).ToList();

                //// Calculate Plan Remaining Budget.
                var objPlanCampaignProgram = db.Plan_Campaign_Program.FirstOrDefault(_prgrm => _prgrm.PlanProgramId == PlanProgramId && _prgrm.IsDeleted == false);   // Modified by :- Sohel Pathan on 01/09/2014 for PL ticket #745
                double allProgramBudget = lstSelectedProgram.Sum(_prgrm => _prgrm.ProgramBudget);
                double planRemainingBudget = (objPlanCampaignProgram.ProgramBudget - (!string.IsNullOrEmpty(Convert.ToString(CostTacticsBudget)) ? CostTacticsBudget : 0));

                // Start - Added by Sohel Pathan on 04/09/2014 for PL ticket #759

                //// Set Quarterly budget allocation value to List.
                if (objPlan.AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString())
                {
                    List<PlanBudgetAllocationValue> lstPlanBudgetAllocationValue = new List<PlanBudgetAllocationValue>();
                    string[] quarterPeriods = Common.quarterPeriods;
                    PlanBudgetAllocationValue objPlanBudgetAllocationValue;

                    for (int i = 0; i < (12 * (GlobalYearDiffrence + 1)); i++)
                    {
                        if ((i + 1) % 3 == 0)
                        {
                            objPlanBudgetAllocationValue = new PlanBudgetAllocationValue();
                            objPlanBudgetAllocationValue.periodTitle = PeriodChar + (i - 1).ToString();
                            objPlanBudgetAllocationValue.budgetValue = lstTacticsBudget.Where(a => quarterPeriods.Contains(a.Period) && a.PlanTacticId == PlanTacticId).FirstOrDefault() == null ? "" : lstTacticsBudget.Where(a => quarterPeriods.Contains(a.Period) && a.PlanTacticId == PlanTacticId).Select(a => a.Value).Sum().ToString();
                            objPlanBudgetAllocationValue.remainingMonthlyBudget = (lstProgramBudget.Where(a => quarterPeriods.Contains(a.Period)).FirstOrDefault() == null ? 0 : lstProgramBudget.Where(a => quarterPeriods.Contains(a.Period)).Select(a => a.Value).Sum()) - (lstTacticsBudget.Where(a => quarterPeriods.Contains(a.Period)).Sum(c => c.Value));
                            objPlanBudgetAllocationValue.programMonthlyBudget = lstTacticsLineItemCost.Where(a => quarterPeriods.Contains(a.Period)).Select(a => a.Value).Sum();

                            /// Add into return list
                            lstPlanBudgetAllocationValue.Add(objPlanBudgetAllocationValue);

                            quarterPeriods = new string[] { PeriodChar + (i + 2), PeriodChar + (i + 3), PeriodChar + (i + 4) };
                        }
                    }

                    var objBudgetAllocationData = new { budgetData = lstPlanBudgetAllocationValue, planRemainingBudget = planRemainingBudget, actualCostData = actualCostAllocationData, IsLineItemForTactic = isLineItemForTactic };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // End - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
                    // Change by Nishant sheth
                    // Desc :: #1765 - add period condition to get value
                    var budgetData = lstMonthlyDynamic.Where(a => a.Id == PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Select(period => new
                    {
                        periodTitle = period,
                        budgetValue = lstTacticsBudget.FirstOrDefault(_tacBdgt => _tacBdgt.Period == period && _tacBdgt.PlanTacticId == PlanTacticId) == null ? "" : lstTacticsBudget.FirstOrDefault(_tacBdgt => _tacBdgt.Period == period && _tacBdgt.PlanTacticId == PlanTacticId).Value.ToString(),
                        remainingMonthlyBudget = (lstProgramBudget.FirstOrDefault(_prgBdgt => _prgBdgt.Period == period) == null ? 0 : lstProgramBudget.FirstOrDefault(_prgBdgt => _prgBdgt.Period == period).Value) - (lstTacticsBudget.Where(_tacBdgt => _tacBdgt.Period == period).Sum(_tacBdgt => _tacBdgt.Value)),
                        programMonthlyBudget = lstTacticsLineItemCost.Where(lineCost => lineCost.Period == period).Sum(lineCost => lineCost.Value)

                    });

                    var objBudgetAllocationData = new { budgetData = budgetData, planRemainingBudget = planRemainingBudget, actualCostData = actualCostAllocationData, IsLineItemForTactic = isLineItemForTactic };
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
        /// Calculate the LineItem cost value based on it's parent Tactic weightage.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<BudgetModel> SetLineItemCostByWeightage(List<BudgetModel> model)
        {
            int _ViewById = ViewBag.ViewBy != null ? (int)ViewBag.ViewBy : 0;
            int weightage = 100;
            foreach (BudgetModel l in model.Where(_mdl => _mdl.ActivityType == ActivityType.ActivityTactic))
            {
                BudgetMonth parent = new BudgetMonth();
                List<BudgetModel> lstLineItems = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId).ToList();

                //// check if ViewBy is Campaign selected then set weightage value to 100;
                if (_ViewById > 0)
                    weightage = l.Weightage;

                foreach (BudgetModel line in lstLineItems)
                {
                    BudgetMonth lineBudget = new BudgetMonth();
                    lineBudget.Jan = (double)(line.Month.Jan * weightage) / 100;
                    lineBudget.Feb = (double)(line.Month.Feb * weightage) / 100;
                    lineBudget.Mar = (double)(line.Month.Mar * weightage) / 100;
                    lineBudget.Apr = (double)(line.Month.Apr * weightage) / 100;
                    lineBudget.May = (double)(line.Month.May * weightage) / 100;
                    lineBudget.Jun = (double)(line.Month.Jun * weightage) / 100;
                    lineBudget.Jul = (double)(line.Month.Jul * weightage) / 100;
                    lineBudget.Aug = (double)(line.Month.Aug * weightage) / 100;
                    lineBudget.Sep = (double)(line.Month.Sep * weightage) / 100;
                    lineBudget.Oct = (double)(line.Month.Oct * weightage) / 100;
                    lineBudget.Nov = (double)(line.Month.Nov * weightage) / 100;
                    lineBudget.Dec = (double)(line.Month.Dec * weightage) / 100;
                    line.Month = lineBudget;
                }
            }
            return model;
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
                    costValue = lstActualLineItemCost.FirstOrDefault(c => c.Period == period && c.PlanLineItemId == planLineItemId) == null ? "" : lstActualLineItemCost.FirstOrDefault(lineCost => lineCost.Period == period && lineCost.PlanLineItemId == planLineItemId).Value.ToString()
                });
                var projectedData = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(line => line.PlanLineItemId.Equals(planLineItemId));
                double projectedval = projectedData != null ? projectedData.Cost : 0;

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

            //// Get Duplicate record to check duplication.
            var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                           join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                           join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                           join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                           where pcptl.Title.Trim().ToLower().Equals(LineItemTitle.Trim().ToLower()) && !pcptl.PlanLineItemId.Equals(PlanLineItemId) && pcptl.IsDeleted.Equals(false)
                                           && pcpt.PlanTacticId == tid
                           select pcpt).FirstOrDefault();

            //// if duplicate record exist then return Duplicate message.
            if (pcptvar != null)
            {
                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
            }
            else
            {
                //// Update LineItem Data to Plan_Campaign_Program_Tactic_LineItem table.
                Plan_Campaign_Program_Tactic_LineItem objPCPTL = db.Plan_Campaign_Program_Tactic_LineItem.Where(c => c.PlanLineItemId == PlanLineItemId).FirstOrDefault();
                if (objPCPTL != null && !string.IsNullOrEmpty(LineItemTitle))
                {
                    objPCPTL.Title = LineItemTitle;
                    objPCPTL.ModifiedBy = Sessions.User.UserId;
                    objPCPTL.ModifiedDate = DateTime.Now;
                    db.Entry(objPCPTL).State = EntityState.Modified;
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
                        obPlanCampaignProgramTacticActual.Value = Convert.ToDouble(arrActualCostInputValues[i]);
                        obPlanCampaignProgramTacticActual.CreatedBy = Sessions.User.UserId;
                        obPlanCampaignProgramTacticActual.CreatedDate = DateTime.Now;
                        db.Entry(obPlanCampaignProgramTacticActual).State = EntityState.Added;
                        isdataupdate = true;
                    }
                }
                saveresult = db.SaveChanges();

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
        public ActionResult AddActual(int Planid)
        {
            HomePlanModel planmodel = new Models.HomePlanModel();

            try
            {
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
                List<Guid> lstOwnAndSubOrdinates = new List<Guid>();
                try
                {
                    lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
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
                if (objPlan.CreatedBy.Equals(Sessions.User.UserId))
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
        /// Function to get list of users based on selected plan id(s), tab and active menu
        /// </summary>
        /// <param name="PlanId">comma separated list of plan id(s)</param>
        /// <param name="ViewBy">/viewBy option selected from dropdown</param>
        /// <param name="ActiveMenu">current active menu</param>
        /// <param name="IsForAddActuals">flag to check call from Add Actual screen</param>
        /// <returns>returns list of users</returns>
        private List<RevenuePlanner.BDSService.User> GetIndividualsByPlanId(string PlanId, string ViewBy, string ActiveMenu, bool IsForAddActuals = false)
        {
            List<int> PlanIds = string.IsNullOrWhiteSpace(PlanId) ? new List<int>() : PlanId.Split(',').Select(plan => int.Parse(plan)).ToList();

            //// Added by :- Sohel Pathan on 17/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, ActiveMenu.ToLower());
            List<string> status = Common.GetStatusListAfterApproved();

            // Start - Added by Arpita Soni on 01/17/2015 for Ticket #1090 
            //// To remove owner of submitted tactics from filter list
            //if (IsForAddActuals)
            //{
            //    status.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            //}
            // End - Added by Arpita Soni on 01/17/2015 for Ticket #1090

            //// Select Tactics of selected plans
            var campaignList = db.Plan_Campaign.Where(campaign => campaign.IsDeleted.Equals(false) && PlanIds.Contains(campaign.PlanId)).Select(campaign => campaign.PlanCampaignId).ToList();
            var programList = db.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false) && campaignList.Contains(program.PlanCampaignId)).Select(program => program.PlanProgramId).ToList();
            var tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && programList.Contains(tactic.PlanProgramId)).Select(tactic => tactic);

            BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();

            List<int> lstAllowedEntityIds = new List<int>();

            //// Added by :- Sohel Pathan on 21/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
            if (ActiveMenu.Equals(Enums.ActiveMenu.Plan.ToString()))
            {
                //List<string> statusCD = new List<string>();
                //statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                //statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

                var TacticUserList = tacticList.Distinct().ToList();
                //     TacticUserList = TacticUserList.Where(tactic => status.Contains(tactic.Status) || ((tactic.CreatedBy == Sessions.User.UserId && !ViewBy.Equals(GanttTabs.Request.ToString())) ? statusCD.Contains(tactic.Status) : false)).Distinct().ToList();
                if (IsForAddActuals)
                {
                    TacticUserList = TacticUserList.Where(tactic => status.Contains(tactic.Status)).Distinct().ToList();
                }
                else
                {
                    TacticUserList = TacticUserList.Where(tactic => !ViewBy.Equals(GanttTabs.Request.ToString())).Distinct().ToList();
                }

                if (TacticUserList.Count > 0)
                {
                    List<int> planTacticIds = TacticUserList.Select(tactic => tactic.PlanTacticId).ToList();
                    lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);

                    //// Custom Restrictions applied
                    TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.UserId).ToList();
                }

                string strContatedIndividualList = string.Join(",", TacticUserList.Select(tactic => tactic.CreatedBy.ToString()));
                var individuals = bdsUserRepository.GetMultipleTeamMemberName(strContatedIndividualList);

                return individuals;
            }
            else
            {
                //// Modified by :- Sohel Pathan on 17/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
                //   var TacticUserList = tacticList.Where(tactic => status.Contains(tactic.Status)).Select(tactic => tactic).Distinct().ToList();

                List<string> statusCD = new List<string>();
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());
                var TacticUserList = tacticList.Distinct().ToList();
                TacticUserList = TacticUserList.Where(tactic => status.Contains(tactic.Status) || (!ViewBy.Equals(GanttTabs.Request.ToString()) ? statusCD.Contains(tactic.Status) : false)).Distinct().ToList();

                if (TacticUserList.Count > 0)
                {
                    List<int> planTacticIds = TacticUserList.Select(tactic => tactic.PlanTacticId).ToList();
                    lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);


                    // Custom Restrictions applied
                    TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.UserId).ToList();
                }

                string strContatedIndividualList = string.Join(",", TacticUserList.Select(tactic => tactic.CreatedBy.ToString()));
                var individuals = bdsUserRepository.GetMultipleTeamMemberName(strContatedIndividualList);

                return individuals;
            }
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
            var lstCustomField = db.CustomFields.Where(customField => customField.ClientId == Sessions.User.ClientId && customField.IsDeleted.Equals(false) &&
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
                var userCustomRestrictionList = Common.GetUserCustomRestrictionsList(Sessions.User.UserId, true);   //// Modified by Sohel Pathan on 15/01/2015 for PL ticket #1139

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
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();

            List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelationForSinglePlan(TacticList, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, false);

            List<Plan_Tactic_Values> ListTacticMQLValue = (from tactic in TacticDataWithoutImprovement
                                                           select new Plan_Tactic_Values
                                                           {
                                                               PlanTacticId = tactic.TacticObj.PlanTacticId,
                                                               MQL = Math.Round(tactic.MQLValue, 0, MidpointRounding.AwayFromZero),
                                                               Revenue = tactic.RevenueValue
                                                           }).ToList();

            TotalMqls = ListTacticMQLValue.Sum(tactic => tactic.MQL);
            TotalCost = LineItemList.Sum(l => l.Cost);
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
        /// Action method to load editable           gridview for plan only.
        /// </summary>
        /// <CreatedBy>Devanshi gandhi</CreatedBy> 
        /// <CreatedDate>10/7/2015</CreatedDate>
        /// <param name="planId">plan Id</param>
        /// <returns>returns partial view HomeGrid</returns>
        public async Task<ActionResult> LoadHomeGrid(string PlanId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds)
        {
            bool IsPlanCreateAll = false;
            bool IsPlanCreateAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            bool IsFiltered = false;
            string statusAllocatedByNone = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower();
            string statusAllocatedByDefault = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();

            string xmlstring = string.Empty;
            string xmlUserlist = string.Empty;
            string stageMQL = Enums.Stage.MQL.ToString();
            Plangrid objplangrid = new Plangrid();

            TacticTypeListForHC = db.TacticTypes.Where(tt => tt.IsDeleted == false).Select(tt => tt).ToList();
            //Added By Komal rawal for honeycomb feature
            Dictionary<string, string> ColorCodelist = db.EntityTypeColors.ToDictionary(e => e.EntityType.ToLower(), e => e.ColorCode);
            var TacticColor = ColorCodelist[Enums.EntityType.Tactic.ToString().ToLower()];
            var ProgramColor = ColorCodelist[Enums.EntityType.Program.ToString().ToLower()];
            var CampaignColor = ColorCodelist[Enums.EntityType.Campaign.ToString().ToLower()];
            var PlanColor = ColorCodelist[Enums.EntityType.Plan.ToString().ToLower()];
			//Declare json object list & variable
            List<PlanDHTMLXGridDataModel> gridjsonlist = new List<PlanDHTMLXGridDataModel>();
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();
            
            try
            {
                List<int> planIds = string.IsNullOrWhiteSpace(PlanId) ? new List<int>() : PlanId.Split(',').Select(plan => int.Parse(plan)).ToList();
                //// Owner filter criteria.
                List<Guid> filterOwner = string.IsNullOrWhiteSpace(ownerIds) ? new List<Guid>() : ownerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();

                //Modified by komal rawal for #1283
                //TacticType filter criteria
                List<int> filterTacticType = string.IsNullOrWhiteSpace(TacticTypeid) ? new List<int>() : TacticTypeid.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();
                //TacticType filter criteria
                List<string> filterStatus = string.IsNullOrWhiteSpace(StatusIds) ? new List<string>() : StatusIds.Split(',').Select(tactictype => tactictype).ToList();

                if (IsPlanCreateAllAuthorized)
                {
                    IsPlanCreateAll = true;
                }
                List<Guid> lstSubordinatesIds = new List<Guid>();
                bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                if (IsTacticAllowForSubordinates)
                {
                    lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
                }
                //Modified By Komal Rawal for #1499
                // Get current user permission for edit own and subordinates plans.
                bool IsPlanEditable = false;
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);


                stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
                string MQLTitle = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(stage => stage.Title).FirstOrDefault();


                int MainPlanID = 0;

                MainPlanID = Convert.ToInt32(planIds[0].ToString());
                List<Plan> lstplandetail = db.Plans.Where(plan => planIds.Contains(plan.PlanId) && plan.IsActive.Equals(true) && plan.IsDeleted == false).ToList();
                int modelId = lstplandetail.Where(p => p.PlanId == MainPlanID).Select(p => p.ModelId).FirstOrDefault();
                string Year = lstplandetail.Where(p => p.PlanId == MainPlanID).Select(p => p.Year).FirstOrDefault(); //Added by Rahul Shah on 30/11/2015 for PL#1764. 
                // Generate Tactic Type dropdown data on click of edit dropdown cell
                // Get Tactic Type List : Added By Bhavesh : 17-Nov-2015 : Changes related to line item dropdown allow editable in grid view
                List<TacticTypeModel> TacticTypeList = db.TacticTypes.Where(tactype => (tactype.IsDeleted == null || tactype.IsDeleted == false) && tactype.IsDeployedToModel && tactype.ModelId == modelId).Select(tacttype => new TacticTypeModel { TacticTypeId = tacttype.TacticTypeId, Title = tacttype.Title }).ToList().OrderBy(tactype => tactype.Title).ToList();
                // Store Tactic Type list in view bag
                ViewBag.TacticTypelist = TacticTypeList;
                ViewBag.Year = Year; //Added by Rahul Shah on 30/11/2015 for PL#1764. 
				// Generate Json Header
                objPlanMainDHTMLXGrid.head = GenerateJsonHeader(MQLTitle, modelId, TacticTypeList, lstplandetail.Select(plan => plan.Year).FirstOrDefault());
                GetGoalValue(lstplandetail, modelId, stageList, objplangrid); // for plan grid header to bind goal detail


                var lstcampaigndetail = db.Plan_Campaign.Where(campaign => planIds.Contains(campaign.PlanId) && campaign.IsDeleted == false).ToList();

                List<int> lstCampaignId = lstcampaigndetail.Select(campaign => campaign.PlanCampaignId).ToList();

                List<Plan_Campaign_Program> programdetail = db.Plan_Campaign_Program.Where(prog => lstCampaignId.Contains(prog.PlanCampaignId) && prog.IsDeleted == false).ToList();

                List<int> lstprogramId = programdetail.Select(program => program.PlanProgramId).ToList();
                var programtactic = db.Plan_Campaign_Program_Tactic.Where(_tactic => lstprogramId.Contains(_tactic.PlanProgramId) && _tactic.IsDeleted.Equals(false)).ToList();

                List<int> lsttacticId = programtactic.Select(tactic => tactic.PlanTacticId).ToList();
                string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
                string section = Enums.Section.Tactic.ToString();
                var cusomfield = db.CustomFields.Where(customField => customField.EntityType == section && customField.ClientId == Sessions.User.ClientId && customField.IsDeleted == false).ToList();
                var customfieldidlist = cusomfield.Select(c => c.CustomFieldId).ToList();
                    
                var lstAllTacticCustomFieldEntitiesanony = db.CustomField_Entity.Where(customFieldEntity => customfieldidlist.Contains(customFieldEntity.CustomFieldId))
                                                                                                       .Select(customFieldEntity => new { EntityId = customFieldEntity.EntityId, CustomFieldId = customFieldEntity.CustomFieldId, Value = customFieldEntity.Value }).Distinct().ToList();

                List<CustomField_Entity> customfieldlist = (from tbl in lstAllTacticCustomFieldEntitiesanony
                                                            join lst in lsttacticId on tbl.EntityId equals lst
                                                            select new CustomField_Entity
                                                            {
                                                                EntityId = tbl.EntityId,
                                                                CustomFieldId = tbl.CustomFieldId,
                                                                Value = tbl.Value
                                                            }).ToList();

                CalculateTacticCostRevenue(modelId, lsttacticId, programtactic, MainPlanID);

                List<string> lstFilteredCustomFieldOptionIds = new List<string>();
                List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                List<Plan_Campaign_Program_Tactic> TacticfilterList = new List<Plan_Campaign_Program_Tactic>();
                List<int> lstTacticIds = new List<int>();
                List<int> lstAllowedEntityIds = new List<int>();
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
                //Modified By Komal Rawal for #1499
                TacticfilterList = programtactic.Where(pcptobj => pcptobj.IsDeleted == false).ToList();
                lstTacticIds = TacticfilterList.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                if (filterOwner.Count > 0 || filterTacticType.Count > 0 || filterStatus.Count > 0 || filteredCustomFields.Count > 0)
                {
                    IsFiltered = true;
                    TacticfilterList = TacticfilterList.Where(pcptobj => (filterOwner.Count.Equals(0) || filterOwner.Contains(pcptobj.CreatedBy)) &&
                                             (filterTacticType.Count.Equals(0) || filterTacticType.Contains(pcptobj.TacticType.TacticTypeId)) &&
                                             (filterStatus.Count.Equals(0) || filterStatus.Contains(pcptobj.Status))).ToList();

                    //// Apply Custom restriction for None type
                    if (TacticfilterList.Count() > 0)
                    {

                        if (filteredCustomFields.Count > 0)
                        {
                            lstTacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, lstTacticIds, customfieldlist);
                            //// get Allowed Entity Ids
                            TacticfilterList = TacticfilterList.Where(tacticlist => lstTacticIds.Contains(tacticlist.PlanTacticId)).ToList();
                        }

                    }
                }
                int intmodelId = (int)modelId;
                var lstLineItemType = db.LineItemTypes.Where(litemtype => litemtype.ModelId == intmodelId).Select(lineitemtype => new { lineitemtype.LineItemTypeId, lineitemtype.Title }).ToList();
                string strLineType = string.Empty;
                // Store Line item type list in Viewbag
                // Generate dropdown for line item type on click of cell
                // Added by bhavesh : 17-Nov-2015 : Allow line item type editable
				ViewBag.lineitemtype = lstLineItemType;
				// Comment by bhavesh TO load line item type later
                //foreach (var typelist in lstLineItemType)
                //{
                //    strLineType = strLineType + "<option value='" + typelist.LineItemTypeId + "'>" + HttpUtility.HtmlEncode((typelist.Title)) + "</option>";
                //}
            
                //to improve performance logic changed
                // Commented By Nishant Sheth 
                // Desc:: To resolve the #1790 observation
                //List<string> collaboratorIds = new List<string>();
                //var planTacticModifiedBy = programtactic.Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
                //var planTacticCreatedBy = programtactic.Select(t => t.CreatedBy.ToString()).ToList();
                //var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => lstTacticIds.Contains(pc.Plan_Campaign_Program_Tactic.PlanTacticId));
                //var planTacticCommentCreatedBy = programtactic.Select(pc => pc.CreatedBy.ToString()).ToList();
                //collaboratorIds.AddRange(planTacticCreatedBy);
                //collaboratorIds.AddRange(planTacticModifiedBy);
                //collaboratorIds.AddRange(planTacticCommentCreatedBy);

                //List<Guid> collaboratorIdsList = collaboratorIds.Distinct().ToList<string>().Select(Guid.Parse).ToList();

                List<int> lsteditableEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false, customfieldlist);
                lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false, customfieldlist);
                TacticfilterList = TacticfilterList.Where(tacticlist => lstAllowedEntityIds.Contains(tacticlist.PlanTacticId) || tacticlist.CreatedBy == Sessions.User.UserId).Select(tacticlist => tacticlist).ToList();
                TempData["TacticfilterList"] = TacticfilterList;
                TempData["lsteditableEntityIds"] = lsteditableEntityIds;
                //End
                int PlanCnt = 1, CampCnt = 1, ProgCnt = 1;
                bool IsPlan = false;
                int planid = 0;
                string type = string.Empty;
                string Startdate = string.Empty;
                string Enddate = string.Empty;
                double totalcost = 0;
                double totalmql = 0;
                double totalrevenue = 0;
                List<Plan_Campaign> Campaignfilterlst = new List<Plan_Campaign>();
                List<Plan_Campaign_Program> Programfilterlst = new List<Plan_Campaign_Program>();
                List<Plan_Campaign_Program_Tactic> finalTacticfilterList = new List<Plan_Campaign_Program_Tactic>();
                List<Plan_Campaign_Program_Tactic_LineItem> finalLineitem = new List<Plan_Campaign_Program_Tactic_LineItem>();
                string cellTextColor = string.Empty;
                string IsEditable = string.Empty;
                List<int> CustomTacticids = new List<int>();
                if (!IsFiltered)
                {
                    IsPlan = true;

                }
                else
                {
                    if (TacticfilterList.Count() > 0)
                    {
                        IsPlan = true;
                    }
                    else
                    {
                        IsPlan = false;
                    }
                }

			   // Declare Variable for XML to JSON
                PlanDHTMLXGridDataModel gridjsonlistplanobj = new PlanDHTMLXGridDataModel();
                string openstateone ="1",lockedstateone ="1";
                string lockedstatezero ="0";
                string bgcolorPlan = "#E6E6E6";
                string bgcolorCampaign = "#C6EBF3";
                string bgcolorProgram = "#DFF0F8";
                string bgcolorTactic = "#E4F1E1";
                string bgcolorLineItem = "#ffffff";
                
                
                string stylecolorblack = "color:#000";
                
                string doubledesh = "--";
                string typero = "ro";
                string dollarsymbol = "$";
                if (IsPlan)
                {
                    foreach (var planitem in lstplandetail)
                    {
                        gridjsonlistplanobj = new PlanDHTMLXGridDataModel();

                        if (planitem.CreatedBy.Equals(Sessions.User.UserId))
                        {
                            IsPlanEditable = true;
                        }
                        else if (IsPlanEditAllAuthorized)
                        {
                            IsPlanEditable = true;
                        }
                        else if (IsPlanEditSubordinatesAuthorized)
                        {
                            if (lstSubordinatesIds.Contains(planitem.CreatedBy))
                            {
                                IsPlanEditable = true;
                            }
                        }

                        planid = planitem.PlanId;
                        if (lstcampaigndetail.Count > 0)
                        {
                            Startdate = lstcampaigndetail.Where(campaign => campaign.PlanId == planid && campaign.IsDeleted == false).ToList().Min(r => r.StartDate).ToString("MM/dd/yyyy");
                            Enddate = lstcampaigndetail.Where(campaign => campaign.PlanId == planid && campaign.IsDeleted == false).ToList().Max(r => r.EndDate).ToString("MM/dd/yyyy");
                        }
                        else
                        {
                            Startdate = new DateTime(Convert.ToInt32(planitem.Year), 1, 1).ToString("MM/dd/yyyy");
                            Enddate = new DateTime(Convert.ToInt32(planitem.Year), 12, 31).ToString("MM/dd/yyyy");
                        }


                        totalcost = LineItemList.Count > 0 ? LineItemList.Where(l => lsttacticId.Contains(l.PlanTacticId)).Sum(l => l.Cost) : 0;
                        totalmql = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => lsttacticId.Contains(l.PlanTacticId)).Sum(l => l.MQL) : 0;
                        totalrevenue = ListTacticMQLValue.Count > 0 ? ListTacticMQLValue.Where(l => lsttacticId.Contains(l.PlanTacticId)).Sum(l => l.Revenue) : 0;


                        if (IsPlanCreateAll == false)
                        {
                            if (planitem.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(planitem.CreatedBy))
                                IsPlanCreateAll = true;
                            else
                                IsPlanCreateAll = false;
                        }
                       
                        // Added ID,open,bgcolor
                        gridjsonlistplanobj.id = "plan." + PlanCnt;
                        gridjsonlistplanobj.open = openstateone;
                        gridjsonlistplanobj.bgColor = bgcolorPlan;

                        // Declare Data List and Object variable
                        List<Plandataobj> plandataobjlist = new List<Plandataobj>();
                        Plandataobj plandataobj = new Plandataobj();

                        plandataobj.value = "Plan";
                        plandataobjlist.Add(plandataobj);

                        plandataobj = new Plandataobj();
                        plandataobj.value = HttpUtility.HtmlEncode(planitem.Title);
                        plandataobj.locked = IsPlanEditable ? lockedstatezero : lockedstateone;
                        plandataobjlist.Add(plandataobj);
                      
                        plandataobj = new Plandataobj();
                        //#1780
                        //plandataobj.value = "<div class=grid_Search id=Plan></div>" + (IsPlanCreateAll ? "<div class=grid_add id=Plan alt=" + planitem.PlanId + " per=" + IsPlanCreateAll.ToString().ToLower() + "></div> " : "") + "<div class=add_Remove_Entity onclick=javascript:AddRemoveEntity(this) id=PlanAdd altId=" + planitem.PlanId + " per=" + IsPlanCreateAll.ToString().ToLower() +"></div>";
                        plandataobj.value = "<div class=grid_Search id=Plan></div>" + (IsPlanCreateAll ? "<div class=grid_add id=Plan alt=" + planitem.PlanId + " per=" + IsPlanCreateAll.ToString().ToLower() + "></div> " : "") + "<div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this) id=PlanAdd TacticType= '" + doubledesh + "' OwnerName= '" + GetOwnerName(planitem.CreatedBy.ToString()) + "' TaskName=" + Uri.EscapeDataString(planitem.Title) + " ColorCode='" + PlanColor + "' altId=" + planitem.PlanId + " per=" + IsPlanCreateAll.ToString().ToLower() + "></div>";
                        plandataobjlist.Add(plandataobj);

                        plandataobj = new Plandataobj();
                        plandataobj.value = planitem.PlanId.ToString();
                        plandataobjlist.Add(plandataobj);

                        plandataobj = new Plandataobj();
                        plandataobj.value = Startdate;
                        plandataobj.locked = lockedstateone;
                        plandataobjlist.Add(plandataobj);
                 
                        plandataobj = new Plandataobj();
                        plandataobj.value = Enddate;
                        plandataobj.locked = lockedstateone;
                        plandataobjlist.Add(plandataobj);
                 
                        plandataobj = new Plandataobj();
                        plandataobj.value = totalcost.ToString();
                        plandataobj.actval = totalcost.ToString();
                        plandataobjlist.Add(plandataobj);
                
                        plandataobj = new Plandataobj();
                        plandataobj.value = doubledesh;
                        plandataobj.type = typero;
                        plandataobjlist.Add(plandataobj);
                 
                        plandataobj = new Plandataobj();
                        plandataobj.value = Common.GetUserName(planitem.CreatedBy.ToString());
                        plandataobj.type = typero;
                        plandataobjlist.Add(plandataobj);
                
                        plandataobj = new Plandataobj();
                        plandataobj.value = doubledesh;
                        plandataobj.type = typero;
                        plandataobjlist.Add(plandataobj);
                
                        plandataobj = new Plandataobj();
                        plandataobj.value = ConvertNumberToRoundFormate(totalmql).ToString();
                        plandataobj.actval = totalmql.ToString();
                        plandataobjlist.Add(plandataobj);
                
                        plandataobj = new Plandataobj();
                        plandataobj.value = dollarsymbol + ConvertNumberToRoundFormate(totalrevenue).ToString();
                        plandataobj.actval = totalrevenue.ToString();
                        plandataobjlist.Add(plandataobj);
                        
                        gridjsonlistplanobj.data = plandataobjlist;
                        Campaignfilterlst = lstcampaigndetail.Where(campaign => campaign.PlanId == planid && campaign.IsDeleted == false).OrderBy(c => c.Title).ToList();// Ticket #1753 : Add default sorting for task name : Added By Bhavesh : Date - 17-Nov-2015 : Addd orderby clause for Campaign title
                        CampCnt = 1;
                        if (Campaignfilterlst.Count > 0)
                        {

                            List<PlanDHTMLXGridDataModel> campaignrowsobjlist = new List<PlanDHTMLXGridDataModel>();
                            PlanDHTMLXGridDataModel campaignrowsobj = new PlanDHTMLXGridDataModel();
                            var lstcampaignTaskData = Campaignfilterlst.Select((taskdata, index) => new
                            {
                                totalcost = LineItemList.Where(l => l.CampaignId == taskdata.PlanCampaignId).Sum(l => l.Cost),
                                totalmql = ListTacticMQLValue.Where(l => l.CampaignId == taskdata.PlanCampaignId).Sum(l => l.MQL),

                                PlanCampaignId = taskdata.PlanCampaignId,
                                totalrevenue = ListTacticMQLValue.Where(l => l.CampaignId == taskdata.PlanCampaignId).Sum(l => l.Revenue),
                                Title = taskdata.Title,
                                StartDate = taskdata.StartDate,
                                EndDate = taskdata.EndDate,
                                IsCampEditable = (taskdata.CreatedBy.Equals(Sessions.User.UserId)) == true ? true : false,
                                CreatedBy = GetUserName(taskdata.CreatedBy),
                                IsPlanCreateAll = IsPlanCreateAll == false ? (taskdata.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(taskdata.CreatedBy)) ? true : false : true,
                                CreatedByID = taskdata.CreatedBy,
                                tacticids = TacticfilterList.Where(t => t.Plan_Campaign_Program.PlanCampaignId == taskdata.PlanCampaignId).Any()
                            });
                            foreach (var Campaignitem in lstcampaignTaskData)
                            {
                                if (!IsFiltered)
                                {
                                    IsPlan = true;

                                }
                                else
                                {
                                    if (Campaignitem.tacticids == true)
                                    {
                                        IsPlan = true;
                                    }
                                    else
                                    {
                                        IsPlan = false;
                                    }
                                }


                                if (IsPlan)
                                {
                                    //Modified By Komal Rawal for #1505
                                    if (Campaignitem.IsCampEditable == false)
                                    {
                                        if (lstSubordinatesIds.Contains(Campaignitem.CreatedByID))
                                        {

                                            CustomTacticids = TacticfilterList.Where(tact => tact.Plan_Campaign_Program.PlanCampaignId == Campaignitem.PlanCampaignId).Select(tact => tact.PlanTacticId).ToList();
                                            //Modified By Komal Rawal for #1545 to check custom restrictions.
                                            if (CustomTacticids.Count > 0 && lsteditableEntityIds.Select(x => x).Intersect(CustomTacticids).Count() != CustomTacticids.Count)
                                            {
                                                IsEditable = lockedstateone; // readonly
                                                cellTextColor = "";
                                            }
                                            else
                                            {
                                                cellTextColor = stylecolorblack;
                                                IsEditable = lockedstatezero;// editable
                                            }

                                        }
                                        else
                                        {
                                            IsEditable = lockedstateone;
                                            cellTextColor = "";

                                        }

                                    }
                                    else
                                    {
                                        cellTextColor = stylecolorblack;
                                        IsEditable = lockedstatezero;
                                    }
                                    //End

                                    campaignrowsobj = new PlanDHTMLXGridDataModel();
                                    campaignrowsobj.id = "camp." + PlanCnt + "." + CampCnt;
                                    campaignrowsobj.open = openstateone;
                                    campaignrowsobj.bgColor = bgcolorCampaign;
                                    List<Plandataobj> campaigndataobjlist = new List<Plandataobj>();
                                    
                                    Plandataobj campaigndataobj = new Plandataobj();
                                    campaigndataobj.value = "Campaign";
                                    campaigndataobjlist.Add(campaigndataobj);

                                    campaigndataobj = new Plandataobj();
                                    campaigndataobj.value = HttpUtility.HtmlEncode(Campaignitem.Title);
                                    campaigndataobj.locked = IsEditable;
                                    campaigndataobj.style = cellTextColor;
                                    campaigndataobjlist.Add(campaigndataobj);

                                    campaigndataobj = new Plandataobj();
                                    //campaigndataobj.value = "<div class=grid_Search id=CP></div>" + (Campaignitem.IsPlanCreateAll ? "<div class=grid_add id=Campaign alt=" + planitem.PlanId + "_" + Campaignitem.PlanCampaignId + " per=" + Campaignitem.IsPlanCreateAll.ToString().ToLower() + "></div>" : "") +  "<div class=add_Remove_Entity id=CampaignAdd onclick=javascript:AddRemoveEntity(this) altId=" + planitem.PlanId + "_" + Campaignitem.PlanCampaignId + " per=" + Campaignitem.IsPlanCreateAll.ToString().ToLower() + "></div>";
                                    campaigndataobj.value = "<div class=grid_Search id=CP></div>" + (Campaignitem.IsPlanCreateAll ? "<div class=grid_add id=Campaign alt=" + planitem.PlanId + "_" + Campaignitem.PlanCampaignId + " per=" + Campaignitem.IsPlanCreateAll.ToString().ToLower() + "> </div>" : "") + "<div class=honeycombbox-icon-gantt id=CampaignAdd onclick=javascript:AddRemoveEntity(this) TacticType= '" + doubledesh + "' ColorCode='" + CampaignColor + "'  OwnerName= '" + GetOwnerName(Campaignitem.CreatedBy) + "' TaskName=" + Uri.EscapeDataString(Campaignitem.Title) + " altId=" + planitem.PlanId + "_" + Campaignitem.PlanCampaignId + " per=" + Campaignitem.IsPlanCreateAll.ToString().ToLower() + "></div>";
                                    campaigndataobjlist.Add(campaigndataobj);

                                    campaigndataobj = new Plandataobj();
                                    campaigndataobj.value = Campaignitem.PlanCampaignId.ToString();
                                    campaigndataobjlist.Add(campaigndataobj);

                                    campaigndataobj = new Plandataobj();
                                    campaigndataobj.value = Campaignitem.StartDate.ToString("MM/dd/yyyy");
                                    campaigndataobj.locked = IsEditable;
                                    campaigndataobj.style = cellTextColor;
                                    campaigndataobjlist.Add(campaigndataobj);

                                    campaigndataobj = new Plandataobj();
                                    campaigndataobj.value = Campaignitem.EndDate.ToString("MM/dd/yyyy");
                                    campaigndataobj.locked = IsEditable;
                                    campaigndataobj.style = cellTextColor;
                                    campaigndataobjlist.Add(campaigndataobj);

                                    campaigndataobj = new Plandataobj();
                                    campaigndataobj.value = Campaignitem.totalcost.ToString();
                                    campaigndataobj.actval = Campaignitem.totalcost.ToString();
                                    campaigndataobjlist.Add(campaigndataobj);

                                    campaigndataobj = new Plandataobj();
                                    campaigndataobj.value = doubledesh;
                                    campaigndataobj.type = typero;
                                    campaigndataobjlist.Add(campaigndataobj);

                                    campaigndataobj = new Plandataobj();
                                    campaigndataobj.value = Campaignitem.CreatedBy.ToString();
                                    campaigndataobj.locked = IsEditable;
                                    campaigndataobj.style = cellTextColor;
                                    campaigndataobjlist.Add(campaigndataobj);

                                    campaigndataobj = new Plandataobj();
                                    campaigndataobj.value = doubledesh;
                                    campaigndataobjlist.Add(campaigndataobj);

                                    campaigndataobj = new Plandataobj();
                                    campaigndataobj.value = ConvertNumberToRoundFormate(Campaignitem.totalmql).ToString();
                                    campaigndataobj.actval = Campaignitem.totalmql.ToString();
                                    campaigndataobjlist.Add(campaigndataobj);

                                    campaigndataobj = new Plandataobj();
                                    campaigndataobj.value = dollarsymbol + ConvertNumberToRoundFormate(Campaignitem.totalrevenue).ToString();
                                    campaigndataobj.actval = Campaignitem.totalrevenue.ToString();
                                    campaigndataobjlist.Add(campaigndataobj);

                                    campaignrowsobj.data = campaigndataobjlist;


                                    Programfilterlst = programdetail.Where(prog => prog.PlanCampaignId == Campaignitem.PlanCampaignId && prog.IsDeleted == false).OrderBy(p => p.Title).ToList();// Ticket #1753 : Add default sorting for task name : Added By Bhavesh : Date - 17-Nov-2015 : Addd orderby clause for Program title
                                    if (Programfilterlst != null && Programfilterlst.Count > 0)
                                    {
                                        Startdate = Programfilterlst.Min(r => r.StartDate).ToString("MM/dd/yyyy");
                                        Enddate = Programfilterlst.Max(r => r.EndDate).ToString("MM/dd/yyyy");
                                        Planuserdatagrid campaignuserdata = new Planuserdatagrid();
                                        campaignuserdata.psdate = Startdate;
                                        campaignuserdata.pedate = Enddate;

                                        var CampaignTactic = programtactic.Where(tact => tact.Plan_Campaign_Program.PlanCampaignId == Campaignitem.PlanCampaignId).ToList();
                                        if (CampaignTactic != null && CampaignTactic.Count > 0)
                                        {
                                            Startdate = CampaignTactic.Min(r => r.StartDate).ToString("MM/dd/yyyy");
                                            Enddate = CampaignTactic.Max(r => r.EndDate).ToString("MM/dd/yyyy");
                                            campaignuserdata.tsdate = Startdate;
                                            campaignuserdata.tedate = Enddate;
                                        }

                                        campaignrowsobj.userdata = campaignuserdata;
                                        ProgCnt = 1;

                                        var lstprogramTaskData = Programfilterlst.Select((taskdata, index) => new
                                        {
                                            totalcost = LineItemList.Where(l => l.Programid == taskdata.PlanProgramId).Sum(l => l.Cost),
                                            totalmql = ListTacticMQLValue.Where(l => l.Programid == taskdata.PlanProgramId).Sum(l => l.MQL),
                                            totalrevenue = ListTacticMQLValue.Where(l => l.Programid == taskdata.PlanProgramId).Sum(l => l.Revenue),
                                            PlanProgramId = taskdata.PlanProgramId,
                                            Title = taskdata.Title,
                                            StartDate = taskdata.StartDate,
                                            EndDate = taskdata.EndDate,
                                            CreatedBy = GetUserName(taskdata.CreatedBy),
                                            IsPlanCreateAll = IsPlanCreateAll == false ? (taskdata.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(taskdata.CreatedBy)) ? true : false : true,
                                            IsProgEditable = taskdata.CreatedBy.Equals(Sessions.User.UserId),
                                            CreatedByID = taskdata.CreatedBy,
                                            tacticids = !IsFiltered ? true : TacticfilterList.Where(t => t.PlanProgramId == taskdata.PlanProgramId).Any()

                                        });
                                        List<PlanDHTMLXGridDataModel> programrowsobjlist = new List<PlanDHTMLXGridDataModel>();
                                        PlanDHTMLXGridDataModel programrowsobj = new PlanDHTMLXGridDataModel();
                                        foreach (var Programitem in lstprogramTaskData)
                                        {
                                            if (Programitem.tacticids)
                                            {
                                                cellTextColor = stylecolorblack;
                                                IsEditable = lockedstatezero;
                                                //Modified By Komal Rawal for #1505
                                                if (!Programitem.IsProgEditable)
                                                {
                                                    if (lstSubordinatesIds.Contains(Programitem.CreatedByID))
                                                    {
                                                        CustomTacticids = TacticfilterList.Where(tact => tact.PlanProgramId == Programitem.PlanProgramId).Select(tact => tact.PlanTacticId).ToList();

                                                        // if (CustomTacticids.Count > 0 &&  lsteditableEntityIds.Select(x => x).Intersect(CustomTacticids).Any() == false)
                                                        if (CustomTacticids.Count > 0 && lsteditableEntityIds.Select(x => x).Intersect(CustomTacticids).Count() != CustomTacticids.Count)
                                                        {
                                                            IsEditable = lockedstateone;
                                                            cellTextColor = "";
                                                        }
                                                        else
                                                        {
                                                            cellTextColor = stylecolorblack;
                                                            IsEditable = lockedstatezero;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        IsEditable = lockedstateone;
                                                        cellTextColor = "";

                                                    }
                                                }
                                                else
                                                {
                                                    cellTextColor = stylecolorblack;
                                                    IsEditable = lockedstatezero;
                                                }
                                                //End

                                                programrowsobj = new PlanDHTMLXGridDataModel();
                                                programrowsobj.id = "prog." + PlanCnt + "." + CampCnt + "." + ProgCnt;
                                                programrowsobj.open = openstateone;
                                                programrowsobj.bgColor = bgcolorProgram;
                                                List<Plandataobj> programdataobjlist = new List<Plandataobj>();
                                                
                                                Plandataobj programdataobj = new Plandataobj();
                                                programdataobj.value = "Program";
                                                programdataobjlist.Add(programdataobj);

                                                programdataobj = new Plandataobj();
                                                programdataobj.value = HttpUtility.HtmlEncode(Programitem.Title);
                                                programdataobj.locked = IsEditable;
                                                programdataobj.style = cellTextColor;
                                                programdataobjlist.Add(programdataobj);

                                                programdataobj = new Plandataobj();
                                                //programdataobj.value = "<div class=grid_Search id=PP></div>" + (Programitem.IsPlanCreateAll ? "<div class=grid_add id=Program alt=_" + Campaignitem.PlanCampaignId + "_" + Programitem.PlanProgramId + " per=" + Programitem.IsPlanCreateAll.ToString().ToLower() + "></div>" : "") + " <div class=add_Remove_Entity id=ProgramAdd onclick=javascript:AddRemoveEntity(this); altId=_" + Campaignitem.PlanCampaignId + "_" + Programitem.PlanProgramId + " per=" + Programitem.IsPlanCreateAll.ToString().ToLower() + "></div>";
                                                programdataobj.value = "<div class=grid_Search id=PP></div>" + (Programitem.IsPlanCreateAll ? "<div class=grid_add id=Program alt=_" + Campaignitem.PlanCampaignId + "_" + Programitem.PlanProgramId + " per=" + Programitem.IsPlanCreateAll.ToString().ToLower() + "></div>" : "") + " <div class=honeycombbox-icon-gantt id=ProgramAdd onclick=javascript:AddRemoveEntity(this); TacticType= '" + doubledesh + "' ColorCode='" + ProgramColor + "' OwnerName= '" + GetOwnerName(Programitem.CreatedBy) + "'  TaskName=" + Uri.EscapeDataString(Programitem.Title) + "  altId=_" + Campaignitem.PlanCampaignId + "_" + Programitem.PlanProgramId + " per=" + Programitem.IsPlanCreateAll.ToString().ToLower() + "></div>";
                                                programdataobjlist.Add(programdataobj);

                                                programdataobj = new Plandataobj();
                                                programdataobj.value = Programitem.PlanProgramId.ToString();
                                                programdataobjlist.Add(programdataobj);

                                                programdataobj = new Plandataobj();
                                                programdataobj.value = Programitem.StartDate.ToString("MM/dd/yyyy");
                                                programdataobj.locked = IsEditable;
                                                programdataobj.style = cellTextColor;
                                                programdataobjlist.Add(programdataobj);

                                                programdataobj = new Plandataobj();
                                                programdataobj.value = Programitem.EndDate.ToString("MM/dd/yyyy");
                                                programdataobj.locked = IsEditable;
                                                programdataobj.style = cellTextColor;
                                                programdataobjlist.Add(programdataobj);

                                                programdataobj = new Plandataobj();
                                                programdataobj.value = Programitem.totalcost.ToString();
                                                programdataobj.actval = Programitem.totalcost.ToString();
                                                programdataobjlist.Add(programdataobj);

                                                programdataobj = new Plandataobj();
                                                programdataobj.value = doubledesh;
                                                programdataobj.type = typero;
                                                programdataobjlist.Add(programdataobj);

                                                programdataobj = new Plandataobj();
                                                programdataobj.value = Programitem.CreatedBy.ToString();
                                                programdataobj.locked = IsEditable;
                                                programdataobj.style = cellTextColor;
                                                programdataobjlist.Add(programdataobj);

                                                programdataobj = new Plandataobj();
                                                programdataobj.value = doubledesh;
                                                programdataobjlist.Add(programdataobj);

                                                programdataobj = new Plandataobj();
                                                programdataobj.value = ConvertNumberToRoundFormate(Programitem.totalmql).ToString();
                                                programdataobj.actval = Programitem.totalmql.ToString();
                                                programdataobjlist.Add(programdataobj);

                                                programdataobj = new Plandataobj();
                                                programdataobj.value = dollarsymbol + ConvertNumberToRoundFormate(Programitem.totalrevenue).ToString();
                                                programdataobj.actval = Programitem.totalrevenue.ToString();
                                                programdataobjlist.Add(programdataobj);

                                                programrowsobj.data = programdataobjlist;


                                                if ((filterOwner.Count() == 0 && filterTacticType.Count() == 0 && filterStatus.Count() == 0 && filteredCustomFields.Count() == 0 )&& !IsFiltered )
                                                {
                                                    //Modified by Komal Rawal for #1750 - For viewing onlly those tactic where user is owner, collaborator or have edit permission.
                                                    // Modified By Nishant Sheth 
                                                    // Desc:: To resolve the #1790 observation
                                                    finalTacticfilterList = TacticfilterList.Where(tacticfilter => tacticfilter.PlanProgramId == Programitem.PlanProgramId && (tacticfilter.CreatedBy == Sessions.User.UserId)).OrderBy(t => t.Title).ToList();
                                                }
                                                else
                                                {
                                                finalTacticfilterList = TacticfilterList.Where(tacticfilter => tacticfilter.PlanProgramId == Programitem.PlanProgramId).OrderBy(t => t.Title).ToList();

                                                }
                                                if (finalTacticfilterList != null && finalTacticfilterList.Count > 0)
                                                {
                                                    Startdate = Enddate = string.Empty;
                                                    var ProgramTactic = programtactic.Where(tact => tact.PlanProgramId == Programitem.PlanProgramId).ToList();
                                                    if (ProgramTactic != null && ProgramTactic.Count > 0)
                                                    {
                                                        Startdate = ProgramTactic.Min(r => r.StartDate).ToString("MM/dd/yyyy");
                                                        Enddate = ProgramTactic.Max(r => r.EndDate).ToString("MM/dd/yyyy");

                                                    }
                                                    Planuserdatagrid programuserdata = new Planuserdatagrid();
                                                    programuserdata.tsdate = Startdate;
                                                    programuserdata.tedate = Enddate;

                                                    programrowsobj.userdata = programuserdata;

                                                    var lsttacticTaskData = finalTacticfilterList.Select((taskdata, index) => new
                                                    {
                                                        index = index,
                                                        totalcost = LineItemList.Where(l => l.PlanTacticId == taskdata.PlanTacticId).Sum(l => l.Cost),
                                                        totalmql = ListTacticMQLValue.Where(l => l.PlanTacticId == taskdata.PlanTacticId).Sum(l => l.MQL),
                                                        totalrevenue = ListTacticMQLValue.Where(l => l.PlanTacticId == taskdata.PlanTacticId).Sum(l => l.Revenue),

                                                        PlanTacticId = taskdata.PlanTacticId,
                                                        title = taskdata.Title,
                                                        startdate = taskdata.StartDate,
                                                        enddate = taskdata.EndDate,
                                                        tactictypetitle = taskdata.TacticType.Title,
                                                        CreatedBy = GetUserName(taskdata.CreatedBy),
                                                        tactictypeid = taskdata.TacticTypeId,
                                                        projectedstagevalue = taskdata.ProjectedStageValue == null ? 0 : taskdata.ProjectedStageValue,
                                                        IsPlanCreateAll = IsPlanCreateAll == false ? (taskdata.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(taskdata.CreatedBy)) ? true : false : true,
                                                        ProjectStage = taskdata.Stage.Title,
                                                        IstactEditable = (taskdata.CreatedBy.Equals(Sessions.User.UserId)) == false ? lstSubordinatesIds.Contains(taskdata.CreatedBy) == true ? lsteditableEntityIds.Contains(taskdata.PlanTacticId) ? "0" : "1" : "1" : "0",
                                                        IsRequiredfalse = false//CheckTacticRequiredfieldFinal(taskdata,DependencyListFinal, customfieldlist)//// Commented by Bhavesh:  To False required flag display in grid view Date: 05/11/2015 Ticket : #1550
                                                    });

                                                    List<PlanDHTMLXGridDataModel> tacticrowsobjlist = new List<PlanDHTMLXGridDataModel>();
                                                    PlanDHTMLXGridDataModel tacticrowsobj = new PlanDHTMLXGridDataModel();
                                                    foreach (var tactic in lsttacticTaskData)
                                                    {
                                                        cellTextColor = tactic.IstactEditable == lockedstateone ? "" : stylecolorblack;

                                                        tacticrowsobj = new PlanDHTMLXGridDataModel();
                                                        tacticrowsobj.id = "tact." + PlanCnt + "." + CampCnt + "." + ProgCnt + "." + tactic.index;
                                                        tacticrowsobj.bgColor = bgcolorTactic;
                                                        List<Plandataobj> tacticdataobjlist = new List<Plandataobj>();
                                                        Plandataobj tacticdataobj = new Plandataobj();

                                                        tacticdataobj.value = "Tactic";
                                                        tacticdataobjlist.Add(tacticdataobj);

                                                        tacticdataobj = new Plandataobj();
                                                        tacticdataobj.value = HttpUtility.HtmlEncode(tactic.title) + (tactic.IsRequiredfalse == true ? "<span id='tacticIsRequired'></span>" : "");
                                                        tacticdataobj.locked = tactic.IstactEditable;
                                                        tacticdataobj.style = cellTextColor;
                                                        tacticdataobjlist.Add(tacticdataobj);

                                                        tacticdataobj = new Plandataobj();
                                                     
                                                        

                                                        tacticdataobj.value = "<div class=grid_Search id=TP></div>" + (tactic.IsPlanCreateAll ? "<div class=grid_add id=Tactic alt=__" + Programitem.PlanProgramId + "_" + tactic.PlanTacticId + " per=" + tactic.IsPlanCreateAll.ToString().ToLower() + "></div>" : "") + " <div class=honeycombbox-icon-gantt id=TacticAdd onclick=javascript:AddRemoveEntity(this)  TaskName=" + Uri.EscapeDataString(tactic.title) + " ColorCode='" + TacticColor + "'  TacticType= '" + GettactictypeName(tactic.tactictypeid) + "' OwnerName= '" + GetOwnerName(tactic.CreatedBy) + "' altId=__" + Programitem.PlanProgramId + "_" + tactic.PlanTacticId + " per=" + tactic.IsPlanCreateAll.ToString().ToLower() + "></div>";
                                                        tacticdataobjlist.Add(tacticdataobj);

                                                        tacticdataobj = new Plandataobj();
                                                        tacticdataobj.value = tactic.PlanTacticId.ToString();
                                                        tacticdataobjlist.Add(tacticdataobj);

                                                        tacticdataobj = new Plandataobj();
                                                        tacticdataobj.value = tactic.startdate.ToString("MM/dd/yyyy");
                                                        tacticdataobj.locked = tactic.IstactEditable;
                                                        tacticdataobj.style = cellTextColor;
                                                        tacticdataobjlist.Add(tacticdataobj);

                                                        tacticdataobj = new Plandataobj();
                                                        tacticdataobj.value = tactic.enddate.ToString("MM/dd/yyyy");
                                                        tacticdataobj.locked = tactic.IstactEditable;
                                                        tacticdataobj.style = cellTextColor;
                                                        tacticdataobjlist.Add(tacticdataobj);

                                                        tacticdataobj = new Plandataobj();
                                                        tacticdataobj.value = tactic.totalcost.ToString();
                                                        tacticdataobj.actval = tactic.totalcost.ToString();
                                                        tacticdataobj.locked = tactic.IstactEditable;
                                                        tacticdataobj.type = "edn";
                                                        tacticdataobj.style = cellTextColor;
                                                        tacticdataobjlist.Add(tacticdataobj);

                                                        tacticdataobj = new Plandataobj();
                                                        tacticdataobj.value = tactic.tactictypeid.ToString();
                                                        tacticdataobj.style = cellTextColor;
                                                        tacticdataobj.locked = tactic.IstactEditable;
                                                        tacticdataobjlist.Add(tacticdataobj);

                                                        tacticdataobj = new Plandataobj();
                                                        tacticdataobj.value = tactic.CreatedBy.ToString();
                                                        tacticdataobj.locked = tactic.IstactEditable;
                                                        tacticdataobj.style = cellTextColor;
                                                        tacticdataobjlist.Add(tacticdataobj);

                                                        tacticdataobj = new Plandataobj();
                                                        tacticdataobj.value = (Math.Round(Convert.ToDouble(tactic.projectedstagevalue)) > 0 ? Math.Round(Convert.ToDouble(tactic.projectedstagevalue)).ToString("#,#") : "0") + " " + tactic.ProjectStage;
                                                        tacticdataobj.locked = tactic.IstactEditable;
                                                        tacticdataobj.style = cellTextColor;
                                                        tacticdataobj.type = "edn";
                                                        tacticdataobjlist.Add(tacticdataobj);

                                                        tacticdataobj = new Plandataobj();
                                                        tacticdataobj.value = ConvertNumberToRoundFormate(tactic.totalmql).ToString();
                                                        tacticdataobj.actval = tactic.totalmql.ToString();
                                                        tacticdataobjlist.Add(tacticdataobj);

                                                        tacticdataobj = new Plandataobj();
                                                        tacticdataobj.value = dollarsymbol + ConvertNumberToRoundFormate(tactic.totalrevenue).ToString();
                                                        tacticdataobj.actval = tactic.totalrevenue.ToString();
                                                        tacticdataobjlist.Add(tacticdataobj);

                                                        tacticrowsobj.data = tacticdataobjlist;

                                                        Planuserdatagrid tacticuserdata = new Planuserdatagrid();
                                                        tacticuserdata.stage = tactic.ProjectStage;
                                                        tacticuserdata.tactictype = tactic.tactictypeid.ToString();

                                                        tacticrowsobj.userdata = tacticuserdata;

                                                        finalLineitem = DBLineItemList.Where(lintitem => lintitem.PlanTacticId == tactic.PlanTacticId).OrderBy(l => l.Title).ToList(); // Ticket #1753 : Add default sorting for task name : Added By Bhavesh : Date - 17-Nov-2015 : Addd orderby clause for line item title
                                                        if (finalLineitem != null && finalLineitem.Count > 0)
                                                        {
                                                            var lstLineItemTaskData = finalLineitem.Select((taskdata, index) => new
                                                            {
                                                                index = index,
                                                                Cost = taskdata.Cost,
                                                                lineitemtype = taskdata.LineItemTypeId,
                                                                PlanLineItemId = taskdata.PlanLineItemId,
                                                                title = taskdata.Title,
                                                                Typeid = taskdata.LineItemTypeId,
                                                                Type = taskdata.LineItemTypeId != null ? taskdata.LineItemType.Title : "",
                                                                CreatedBy = taskdata.CreatedBy,
                                                                IsPlanCreateAll = IsPlanCreateAll == false ? (taskdata.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(taskdata.CreatedBy)) ? true : false : true,
                                                                IstactEditable = (taskdata.CreatedBy.Equals(Sessions.User.UserId)) == true ? lsteditableEntityIds.Contains(taskdata.PlanTacticId) ? "0" : "1" : "1"
                                                            });

                                                            List<PlanDHTMLXGridDataModel> lineitemrowsobjlist = new List<PlanDHTMLXGridDataModel>();
                                                            PlanDHTMLXGridDataModel lineitemrowsobj = new PlanDHTMLXGridDataModel();
                                                            foreach (var lineitem in lstLineItemTaskData)
                                                            {
                                                                cellTextColor = lineitem.IstactEditable == lockedstateone ? "" : stylecolorblack;

                                                                lineitemrowsobj = new PlanDHTMLXGridDataModel();
                                                                lineitemrowsobj.id = "line." + PlanCnt + "." + CampCnt + "." + ProgCnt + "." + tactic.index + "." + lineitem.index;
                                                                lineitemrowsobj.bgColor = bgcolorLineItem;
                                                                List<Plandataobj> lineitemdataobjlist = new List<Plandataobj>();
                                                                Plandataobj lineitemdataobj = new Plandataobj();

                                                                lineitemdataobj.value = "LineItem";
                                                                lineitemdataobjlist.Add(lineitemdataobj);

                                                                lineitemdataobj = new Plandataobj();
                                                                lineitemdataobj.value = HttpUtility.HtmlEncode(lineitem.title);
                                                                lineitemdataobj.locked = lineitem.IstactEditable;
                                                                lineitemdataobj.style = cellTextColor;
                                                                lineitemdataobjlist.Add(lineitemdataobj);

                                                                lineitemdataobj = new Plandataobj();
                                                                lineitemdataobj.value = "<div class=grid_Search id=LP></div>" + (tactic.IsPlanCreateAll ? "<div class=grid_add id=Line alt=___" + tactic.PlanTacticId + "_" + lineitem.PlanLineItemId + " lt=" + ((lineitem.lineitemtype == null) ? 0 : lineitem.lineitemtype) + " dt=" + HttpUtility.HtmlEncode(lineitem.title) + " per=" + tactic.IsPlanCreateAll.ToString().ToLower() + "></div>" : "");
                                                                lineitemdataobjlist.Add(lineitemdataobj);

                                                                lineitemdataobj = new Plandataobj();
                                                                lineitemdataobj.value = lineitem.PlanLineItemId.ToString();
                                                                lineitemdataobjlist.Add(lineitemdataobj);

                                                                lineitemdataobj = new Plandataobj();
                                                                lineitemdataobj.locked = lockedstateone;
                                                                lineitemdataobjlist.Add(lineitemdataobj);

                                                                lineitemdataobj = new Plandataobj();
                                                                lineitemdataobj.locked = lockedstateone;
                                                                lineitemdataobjlist.Add(lineitemdataobj);

                                                                lineitemdataobj = new Plandataobj();
                                                                lineitemdataobj.value = lineitem.Cost.ToString();
                                                                lineitemdataobj.locked = ((lineitem.Type == null || lineitem.Type == "") ? lockedstateone : lineitem.IstactEditable);
                                                                lineitemdataobj.type = "edn";
                                                                lineitemdataobj.style = cellTextColor;
                                                                lineitemdataobjlist.Add(lineitemdataobj);

                                                                lineitemdataobj = new Plandataobj();
                                                                lineitemdataobj.value = HttpUtility.HtmlEncode(lineitem.Type);
                                                                lineitemdataobj.style = cellTextColor;
                                                                lineitemdataobj.locked = ((lineitem.Type == null || lineitem.Type == "") ? lockedstateone : lineitem.IstactEditable);
                                                                lineitemdataobjlist.Add(lineitemdataobj);

                                                                lineitemdataobj = new Plandataobj();
                                                                lineitemdataobj.value = lstUserDetails.Where(lst => lst.UserId == lineitem.CreatedBy).Select(lst => string.Format("{0} {1}", HttpUtility.HtmlDecode(lst.FirstName), HttpUtility.HtmlDecode(lst.LastName))).FirstOrDefault();
                                                                lineitemdataobjlist.Add(lineitemdataobj);

                                                                lineitemdataobj = new Plandataobj();
                                                                lineitemdataobj.value = doubledesh;
                                                                lineitemdataobj.type = typero;
                                                                lineitemdataobjlist.Add(lineitemdataobj);

                                                                lineitemdataobj = new Plandataobj();
                                                                lineitemdataobj.value = doubledesh;
                                                                lineitemdataobj.type = typero;
                                                                lineitemdataobjlist.Add(lineitemdataobj);

                                                                lineitemdataobj = new Plandataobj();
                                                                lineitemdataobj.value = doubledesh;
                                                                lineitemdataobj.type = typero;
                                                                lineitemdataobjlist.Add(lineitemdataobj);

                                                                lineitemrowsobj.data = lineitemdataobjlist;

                                                                Planuserdatagrid lineitemuserdata = new Planuserdatagrid();
                                                                lineitemuserdata.IsOther = ((lineitem.Type == null || lineitem.Type == "") ? true : false).ToString();
                                                                lineitemrowsobj.userdata = lineitemuserdata;

                                                                lineitemrowsobjlist.Add(lineitemrowsobj);
                                                            }
                                                            tacticrowsobj.rows = lineitemrowsobjlist;
                                                        }
                                                        tacticrowsobjlist.Add(tacticrowsobj);
                                                    }
                                                    programrowsobj.rows = tacticrowsobjlist;
                                                }
                                                ProgCnt = ProgCnt + 1;
                                                programrowsobjlist.Add(programrowsobj);//Add for : To resolve filter issue for display grid
                                            }
                                            //programrowsobjlist.Add(programrowsobj); //Comment for : To resolve filter issue for display grid
                                        }
                                        campaignrowsobj.rows = programrowsobjlist;//Add for : To resolve filter issue for display grid
                                    }

                                    CampCnt = CampCnt + 1;
                                    campaignrowsobjlist.Add(campaignrowsobj);
                                }
                                //campaignrowsobjlist.Add(campaignrowsobj); //Comment for : To resolve filter issue for display grid
                            }
                            gridjsonlistplanobj.rows = campaignrowsobjlist;
                        }
                        
                        PlanCnt = PlanCnt + 1;
                        gridjsonlist.Add(gridjsonlistplanobj);
                    }
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
            }
            objPlanMainDHTMLXGrid.rows = gridjsonlist;
            objplangrid.PlanDHTMLXGrid = objPlanMainDHTMLXGrid;
            await Task.Delay(1);
            return PartialView("_HomeGrid", objplangrid);
        }

        #region method to generate grid header
        protected List<PlanHead> GenerateJsonHeader(string MQLTitle, int modelid,List<TacticTypeModel> TacticTypeList, string PlanYear)
        {
            List<PlanHead> headobjlist = new List<PlanHead>();
            PlanHead headobj = new PlanHead();
            try
            {
                List<PlanOptions> lstTacticType = TacticTypeList.Select(tacttype => new PlanOptions { id = tacttype.TacticTypeId.ToString(), value = HttpUtility.HtmlDecode(tacttype.Title) }).ToList();

                List<User> lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
                lstUsers = lstUsers.Where(i => i.IsDeleted == false).ToList();
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId, lstUsers);
                List<PlanOptions> lstOwner = new List<PlanOptions>();

                if (lstClientUsers.Count() > 0)
                {
                    string strUserList = string.Join(",", lstClientUsers);
                    lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberNameByApplicationId(strUserList, Sessions.ApplicationId);   //PL #1532 Grid View & Tactic Pop-up: Displaying Users which are not in Gameplan - Dashrath Prajapati                

                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        lstOwner = lstUserDetails.Select(user => new PlanOptions { id = user.UserId.ToString(), value = HttpUtility.HtmlEncode(string.Format("{0} {1}", user.FirstName, user.LastName)) }).ToList();
                    }
                }

                // First Column Activity Type
                headobj.type = "ro";
                headobj.align = "center";
                headobj.id = "activitytype";
                headobj.sort = "na";
                headobj.width = 0;
                headobj.value = "Activity Type";
                headobjlist.Add(headobj);

                //Second Column : Task Name
                headobj = new PlanHead();
                headobj.type = "tree";
                headobj.align = "left";
                headobj.id = "taskname";
                headobj.sort = "str";
                headobj.width = 330;
                headobj.value = "Task Name";
                headobjlist.Add(headobj);

                // Third Column : Empty
                headobj = new PlanHead();
                headobj.type = "ro";
                headobj.align = "center";
                headobj.id = "add";
                headobj.sort = "na";
                headobj.width = 85; //modified by Rahul shah on 04/12/2015-to increase width for honeycomb feature
                headobj.value = "";
                headobjlist.Add(headobj);

                // Fourth Column : Id
                headobj = new PlanHead();
                headobj.type = "ro";
                headobj.align = "center";
                headobj.id = "id";
                headobj.sort = "na";
                headobj.width = 0;
                headobj.value = "id";
                headobjlist.Add(headobj);

                // Fifth Column : Start Date
                headobj = new PlanHead();
                headobj.type = "dhxCalendar";
                headobj.align = "center";
                headobj.id = "startdate";
                headobj.sort = "date";
                headobj.width = 110;
                headobj.value = "Start Date";
                headobjlist.Add(headobj);

                // Sixth Column : End Date
                headobj = new PlanHead();
                headobj.type = "dhxCalendar";
                headobj.align = "center";
                headobj.id = "enddate";
                headobj.sort = "date";
                headobj.width = 100;
                headobj.value = "End Date";
                headobjlist.Add(headobj);

                // Seventh Column: Planned Cost
                headobj = new PlanHead();
                headobj.type = "ron";
                headobj.align = "center";
                headobj.id = "plannedcost";
                headobj.sort = "int";
                headobj.width = 160;
                headobj.value = "Planned Cost";
                headobjlist.Add(headobj);

                // Eight Column : Type
                headobj = new PlanHead();
                headobj.type = "coro";
                headobj.align = "center";
                headobj.id = "tactictype";
                headobj.sort = "sort_TacticType";
                headobj.width = 150;
                headobj.value = "Type";
                headobj.options = lstTacticType;
                headobjlist.Add(headobj);

                //Nineth Column : Owner
                headobj = new PlanHead();
                headobj.type = "coro";
                headobj.align = "center";
                headobj.id = "owner";
                headobj.sort = "sort_Owner";
                headobj.width = 115;
                headobj.value = "Owner";
                headobj.options = lstOwner;
                headobjlist.Add(headobj);

                // Tenth Column : Target Stage Goal
                headobj = new PlanHead();
                headobj.type = "ron";
                headobj.align = "center";
                headobj.id = "inq";
                headobj.sort = "int";
                headobj.width = 150;
                headobj.value = "Target Stage Goal";
                headobjlist.Add(headobj);

                // Eleventh Column: MQl
                headobj = new PlanHead();
                headobj.type = "ron";
                headobj.align = "center";
                headobj.id = "mql";
                headobj.sort = "int";
                headobj.width = 150;
                headobj.value = MQLTitle;
                headobjlist.Add(headobj);
                // Twelveth Column : Revenue
                headobj = new PlanHead();
                headobj.type = "ron";
                headobj.align = "center";
                headobj.id = "revenue";
                headobj.sort = "int";
                headobj.width = 150;
                headobj.value = "Revenue";
                headobjlist.Add(headobj);
              
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

            }
            return headobjlist;
        }
        #endregion

        #endregion
        #region Check tactic required field condition
        /// <summary>
        /// Added By Bhavesh
        /// Date: 05-Nov-2015
        /// Check all require field exists in tactic or not with attribute 
        /// Ticket #1550
        /// </summary>
        /// <param name="tacticObj">Tactic Object</param>
        /// <param name="lstCustomFieldsDependency">List of Custom Field Dependency</param>
        /// <param name="customfieldlist">Custom Field Entity List</param>
        /// <returns>True or False</returns>
        public bool CheckTacticRequiredfieldFinal(Plan_Campaign_Program_Tactic tacticObj, List<CustomFieldDependency> lstCustomFieldsDependency, List<CustomField_Entity> customfieldlist)
        {
            try
            {
                if (string.IsNullOrEmpty(tacticObj.Title))
                    return true;
                else if (tacticObj.TacticTypeId == 0)
                    return true;
                else if (tacticObj.CreatedBy == null)
                    return true;
                else
                {
                    var tacticcustomfieldlist = customfieldlist.Where(t => t.EntityId == tacticObj.PlanTacticId && lstCustomFieldsRequired.Contains(t.CustomFieldId)).ToList();
                    List<int> tacticentitycustomfieldid = tacticcustomfieldlist.Select(a => a.CustomFieldId).Distinct().ToList();
                    List<int> missedcustomfieldid = lstCustomFieldsRequired.Except(tacticentitycustomfieldid).ToList();
                    foreach (int mcfi in missedcustomfieldid)
                    {
                        if (lstCustomFieldsDependency.Where(d => d.ChildCustomFieldId == mcfi).Any())
                        {
                            List<string> dependencylistids = lstCustomFieldsDependency.Where(d => d.ChildCustomFieldId == mcfi && d.ParentCustomFieldId != null).Select(d =>
                                d.ParentOptionId.ToString()
                            ).ToList<string>();
                            var tacticfinalrequiredfield = tacticcustomfieldlist.Any(tc => dependencylistids.Contains(tc.Value));
                            if (tacticfinalrequiredfield)
                                return true;
                            else
                            {
                                if (lstCustomFieldsDependency.Any(d => d.ChildCustomFieldId == mcfi && d.ParentCustomFieldId == null && d.ParentOptionId == tacticObj.TacticTypeId))
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
                return true;
            }

            //End
        }

        public List<CustomFieldModel> GetEntityData(int id, List<CustomFieldModel> lstCustomFields, List<CustomField_Entity> customfieldlist)
        {
            List<int> customFieldIds = lstCustomFields.Select(cs => cs.customFieldId).ToList();
            var EntityValue = customfieldlist.Where(ct => ct.EntityId == id && customFieldIds.Contains(ct.CustomFieldId)).Select(ct => new { ct.Value, ct.CustomFieldId }).ToList();
            foreach (var CustomFieldId in customFieldIds)
            {
                lstCustomFields.Where(c => c.customFieldId == CustomFieldId).FirstOrDefault().value = EntityValue.Where(ev => ev.CustomFieldId == CustomFieldId).Select(ev => ev.Value).ToList();

            }

            return lstCustomFields;

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
        public ActionResult SaveGridDetail(string UpdateType, string UpdateColumn, string UpdateVal, int id = 0)
        {
            string PeriodChar = "Y";
            Guid oldOwnerId = new Guid();
            int oldProgramId = 0;
            string oldProgramTitle = "";
            int oldCampaignId = 0;
            UpdateColumn = UpdateColumn.Trim();
            try
            {
                #region update Plan Detail
                if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.plan.ToString())
                {
                    Plan plan = db.Plans.Where(_plan => _plan.PlanId == id).ToList().FirstOrDefault();
                    if (UpdateColumn == "Task Name")
                    {
                        plan.Title = UpdateVal.Trim();
                    }

                    db.Entry(plan).State = EntityState.Modified;
                    db.SaveChanges();
                    Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                }
                #endregion

                #region update tactic detail
                if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.tactic.ToString())
                {
                    Plan_Campaign_Program_Tactic pcpobj = db.Plan_Campaign_Program_Tactic.Where(pcptobjw => pcptobjw.PlanTacticId.Equals(id)).FirstOrDefault();
                    List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = new List<Plan_Campaign_Program_Tactic_LineItem>();
                    double totalLineitemCost = 0;
                    double otherLineItemCost = 0;
                    // update tactic detail
                    if (UpdateColumn == Enums.PlanGrid_Column["taskname"])
                    {
                        var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                      join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                      join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                      where pcpt.Title.Trim().ToLower().Equals(UpdateVal) && !pcpt.PlanTacticId.Equals(12345) && pcpt.IsDeleted.Equals(false)
                                      && pcp.PlanProgramId == pcpobj.Plan_Campaign_Program.PlanProgramId    //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
                                      select pcp).FirstOrDefault();
                        if (pcpvar != null)
                        {
                            string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                            return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                        }
                        else
                            pcpobj.Title = UpdateVal;

                    }
                    else if (UpdateColumn == Enums.PlanGrid_Column["startdate"])
                    {
                        //  var pstartdate = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(pcpobj.PlanProgramId)).FirstOrDefault().StartDate;
                        if (!string.IsNullOrEmpty(UpdateVal))
                        {
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
                    else if (UpdateColumn == Enums.PlanGrid_Column["enddate"])
                    {
                        //  var pstartdate = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(pcpobj.PlanProgramId)).FirstOrDefault().StartDate;
                        if (!string.IsNullOrEmpty(UpdateVal))
                        {
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
                    else if (UpdateColumn == Enums.PlanGrid_Column["tacticplancost"])
                    {
                        if ((db.Plan_Campaign_Program_Tactic_Cost.Where(_tacCost => _tacCost.PlanTacticId == id).ToList()).Count() == 0 ||
                                    pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower()
                                    || pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                        {
                            pcpobj.Cost = Convert.ToDouble(UpdateVal);
                        }



                        tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == pcpobj.PlanTacticId).ToList();
                        List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                        var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                        List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();

                        if (objtotalLineitemCost != null && objtotalLineitemCost.Count() > 0)
                            totalLineitemCost = objtotalLineitemCost.Sum(l => l.Cost);
                        if (totalLineitemCost > Convert.ToDouble(UpdateVal))
                        {
                            UpdateVal = totalLineitemCost.ToString();
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
                                objTacticCost.CreatedBy = Sessions.User.UserId;
                                objTacticCost.CreatedDate = DateTime.Now;
                                db.Entry(objTacticCost).State = EntityState.Added;
                            }

                        }
                        else if (Convert.ToDouble(UpdateVal) < pcpobj.Cost)
                        {
                            var diffcost = pcpobj.Cost - Convert.ToDouble(UpdateVal);
                            int endmonth = 12;
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
                                if (endmonth > 0)
                                {
                                    endmonth -= 1;
                                }

                            }

                        }

                        pcpobj.Cost = Convert.ToDouble(UpdateVal);

                    }
                    else if (UpdateColumn == Enums.PlanGrid_Column["tactictype"])
                    {
                        int tactictypeid = Convert.ToInt32(UpdateVal);
                        pcpobj.TacticTypeId = tactictypeid;
                        TacticType tt = db.TacticTypes.Where(tacType => tacType.TacticTypeId == tactictypeid).FirstOrDefault();
                        pcpobj.ProjectedStageValue = tt.ProjectedStageValue == null ? 0 : tt.ProjectedStageValue;

                        pcpobj.StageId = tt.StageId == null ? 0 : (int)tt.StageId;


                    }
                    else if (UpdateColumn == Enums.PlanGrid_Column["targetstagegoal"])
                    {
                        pcpobj.ProjectedStageValue = Convert.ToDouble(UpdateVal);
                    }
                    else if (UpdateColumn == Enums.PlanGrid_Column["owner"])
                    {
                        oldOwnerId = pcpobj.CreatedBy;
                        pcpobj.CreatedBy = new Guid(UpdateVal);
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

                    }
                    db.Entry(pcpobj).State = EntityState.Modified;
                    db.SaveChanges();
                    int result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);

                    if (result > 0)
                    {
                        if (UpdateColumn == Enums.PlanGrid_Column["owner"])
                            SendEmailnotification(pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId, id, oldOwnerId, new Guid(UpdateVal), pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.Title.ToString(), pcpobj.Plan_Campaign_Program.Plan_Campaign.Title.ToString(), pcpobj.Plan_Campaign_Program.Title.ToString(), pcpobj.Title.ToString(), Enums.Section.Tactic.ToString().ToLower());
                        if (UpdateColumn == "ParentID")
                        {
                            if (pcpobj.IntegrationInstanceTacticId != null && oldProgramId > 0)
                            {
                                if (pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstance.IntegrationType.Code == Enums.IntegrationInstanceType.Salesforce.ToString())
                                {
                                    ExternalIntegration externalIntegration = new ExternalIntegration(pcpobj.PlanTacticId, Sessions.ApplicationId, new Guid(), EntityType.Tactic, true);
                                    externalIntegration.Sync();

                                }
                            }
                            if (oldProgramId > 0)
                            {
                                var actionSuffix = oldProgramTitle + " to " + pcpobj.Plan_Campaign_Program.Title;
                                Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.moved, actionSuffix);
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
                        if (UpdateColumn == Enums.PlanGrid_Column["tacticplancost"])
                        {
                            Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = tblTacticLineItem.FirstOrDefault(lineItem => lineItem.LineItemTypeId == null);
                            if (objOtherLineItem == null)
                            {
                                Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                objNewLineitem.PlanTacticId = pcpobj.PlanTacticId;
                                objNewLineitem.Title = Common.LineItemTitleDefault + pcpobj.Title;
                                if (pcpobj.Cost > totalLineitemCost)
                                {
                                    objNewLineitem.Cost = pcpobj.Cost - totalLineitemCost;
                                }
                                else
                                {
                                    objNewLineitem.Cost = 0;
                                }
                                objNewLineitem.Description = string.Empty;
                                objNewLineitem.CreatedBy = Sessions.User.UserId;
                                objNewLineitem.CreatedDate = DateTime.Now;
                                db.Entry(objNewLineitem).State = EntityState.Added;
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
                                }
                                db.Entry(objOtherLineItem).State = EntityState.Modified;
                            }
                            db.SaveChanges();
                        }
                    }
                    //Added By Rahul Shah on 16/10/2015 for PL 1559
                    //Added By Komal Rawal to update owner in HoneyComb
                    //var OwnerName = "";
                    //if (UpdateColumn == Enums.PlanGrid_Column["owner"])
                    //{
                    //     OwnerName = GetOwnerName(UpdateVal);
                    //}
                    return Json(new { lineItemCost = totalLineitemCost, OtherLineItemCost = otherLineItemCost }, JsonRequestBehavior.AllowGet);
                }
                #endregion
                #region update program detail

                if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.program.ToString())
                {
                    Plan_Campaign_Program pcpobj = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(id)).FirstOrDefault();
                    if (UpdateColumn == Enums.PlanGrid_Column["taskname"])
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
                    else if (UpdateColumn == Enums.PlanGrid_Column["startdate"])
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
                    else if (UpdateColumn == Enums.PlanGrid_Column["enddate"])
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
                    else if (UpdateColumn == Enums.PlanGrid_Column["owner"])
                    {
                        oldOwnerId = pcpobj.CreatedBy;
                        pcpobj.CreatedBy = new Guid(UpdateVal);
                    }
                    db.Entry(pcpobj).State = EntityState.Modified;
                    db.SaveChanges();
                    int result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanProgramId, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);

                    if (result > 0)
                    {
                        if (UpdateColumn == Enums.PlanGrid_Column["owner"])
                            SendEmailnotification(pcpobj.Plan_Campaign.Plan.PlanId, id, oldOwnerId, new Guid(UpdateVal), pcpobj.Plan_Campaign.Plan.Title.ToString(), pcpobj.Plan_Campaign.Title.ToString(), pcpobj.Title.ToString(), pcpobj.Title.ToString(), Enums.Section.Program.ToString().ToLower());

                    }

                    //Added By Komal Rawal to update owner in HoneyComb
                    //var OwnerName = "";
                    //if (UpdateColumn == Enums.PlanGrid_Column["owner"])
                    //{
                    //    OwnerName = GetOwnerName(UpdateVal);
                    //}
                    //return Json(new { OwnerName = OwnerName }, JsonRequestBehavior.AllowGet);
                }


                #endregion
                #region update Campaign detail
                if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.campaign.ToString())
                {
                    int planId = db.Plan_Campaign.Where(_plan => _plan.PlanCampaignId.Equals(id)).FirstOrDefault().PlanId;

                    Plan_Campaign pcobj = db.Plan_Campaign.Where(pcobjw => pcobjw.PlanCampaignId.Equals(id) && pcobjw.IsDeleted.Equals(false)).FirstOrDefault();
                    if (UpdateColumn == Enums.PlanGrid_Column["taskname"])
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
                    else if (UpdateColumn == Enums.PlanGrid_Column["startdate"])
                    {
                        if (!string.IsNullOrEmpty(UpdateVal))
                            pcobj.StartDate = Convert.ToDateTime(UpdateVal);

                    }
                    else if (UpdateColumn == Enums.PlanGrid_Column["enddate"])
                    {
                        if (!string.IsNullOrEmpty(UpdateVal))
                            pcobj.EndDate = Convert.ToDateTime(UpdateVal);

                    }
                    else if (UpdateColumn == Enums.PlanGrid_Column["owner"])
                    {
                        oldOwnerId = pcobj.CreatedBy;
                        pcobj.CreatedBy = new Guid(UpdateVal);
                    }
                    db.Entry(pcobj).State = EntityState.Modified;
                    db.SaveChanges();
                    int result = Common.InsertChangeLog(Sessions.PlanId, null, pcobj.PlanCampaignId, pcobj.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);

                    if (result > 0)
                    {
                        if (UpdateColumn == Enums.PlanGrid_Column["owner"])
                            SendEmailnotification(pcobj.Plan.PlanId, id, oldOwnerId, new Guid(UpdateVal), pcobj.Plan.Title, pcobj.Title, pcobj.Title, pcobj.Title, Enums.Section.Campaign.ToString().ToLower());

                    }

                    //Added By Komal Rawal to update owner in HoneyComb
                    //var OwnerName = "";
                    //if (UpdateColumn == Enums.PlanGrid_Column["owner"])
                    //{
                    //    OwnerName = GetOwnerName(UpdateVal);
                    //}
                    //return Json(new { OwnerName = OwnerName }, JsonRequestBehavior.AllowGet);
                }

                #endregion
                #region update LineItem Detail
                if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.lineitem.ToString())
                {
                    Plan_Campaign_Program_Tactic_LineItem objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobjw => pcpobjw.PlanLineItemId.Equals(id));
                    var objTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == objLineitem.PlanTacticId);
                    //Added By Rahul Shah on 16/10/2015 for PL 1559
                    double tacticostNew = objTactic.Plan_Campaign_Program_Tactic_Cost.Select(tactic => tactic.Value).Sum();
                    if (UpdateColumn == Enums.PlanGrid_Column["taskname"])
                    {
                        //// Get duplicate record to check duplication.
                        var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                       where pcptl.Title.Trim().ToLower().Equals(UpdateVal.Trim().ToLower()) && !pcptl.PlanLineItemId.Equals(id) && pcptl.IsDeleted.Equals(false)
                                       select pcptl).FirstOrDefault();

                        if (pcptvar != null)
                        {
                            string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);
                            return Json(new { isSaved = false, errormsg = strDuplicateMessage });
                        }
                        else
                        {
                            objLineitem.Title = UpdateVal.Trim();
                        }

                    }
                    else if (UpdateColumn == Enums.PlanGrid_Column["tactictype"])
                    {
                        objLineitem.LineItemTypeId = Convert.ToInt32(UpdateVal);
                    }
                    else if (UpdateColumn == Enums.PlanGrid_Column["tacticplancost"])
                    {
                        double lCost = 0;
                        if (!string.IsNullOrEmpty(UpdateVal))
                            lCost = Convert.ToDouble(UpdateVal);
                        if (lCost > objLineitem.Cost)
                        {
                            var diffcost = lCost - objLineitem.Cost;
                            int startmonth = objTactic.StartDate.Month;

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
                                objlineitemCost.CreatedBy = Sessions.User.UserId;
                                objlineitemCost.CreatedDate = DateTime.Now;
                                db.Entry(objlineitemCost).State = EntityState.Added;
                            }

                            List<Plan_Campaign_Program_Tactic_LineItem> tblTacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == objTactic.PlanTacticId).ToList();
                            List<Plan_Campaign_Program_Tactic_LineItem> objtotalLineitemCost = tblTacticLineItem.Where(lineItem => lineItem.LineItemTypeId != null && lineItem.IsDeleted == false).ToList();
                            var lineitemidlist = objtotalLineitemCost.Select(lineitem => lineitem.PlanLineItemId).ToList();
                            List<Plan_Campaign_Program_Tactic_LineItem_Cost> lineitemcostlist = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lic => lineitemidlist.Contains(lic.PlanLineItemId)).ToList();

                            List<Plan_Campaign_Program_Tactic_Cost> tacticostslist = objTactic.Plan_Campaign_Program_Tactic_Cost.ToList();
                            double tacticost = objTactic.Plan_Campaign_Program_Tactic_Cost.Select(tactic => tactic.Value).Sum();

                            if (tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).Any())
                            {
                                var tacticmonthcost = tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value;
                                double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.PlanLineItemId != id && lineitem.Period == PeriodChar + startmonth).Sum(lineitem => lineitem.Value) + lCost;
                                if (tacticlineitemcostmonth > tacticmonthcost)
                                {
                                    tacticostslist.Where(pcptc => pcptc.Period == PeriodChar + startmonth).FirstOrDefault().Value = tacticlineitemcostmonth;
                                    objTactic.Cost = objTactic.Cost + (tacticlineitemcostmonth - tacticmonthcost);
                                }
                            }
                            else
                            {
                                double tacticlineitemcostmonth = lineitemcostlist.Where(lineitem => lineitem.PlanLineItemId != id && lineitem.Period == PeriodChar + startmonth).Sum(lineitem => lineitem.Value) + lCost;
                                Plan_Campaign_Program_Tactic_Cost objtacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                objtacticCost.PlanTacticId = objTactic.PlanTacticId;
                                objtacticCost.Period = PeriodChar + startmonth;
                                objtacticCost.Value = tacticlineitemcostmonth;
                                objtacticCost.CreatedBy = Sessions.User.UserId;
                                objtacticCost.CreatedDate = DateTime.Now;
                                db.Entry(objtacticCost).State = EntityState.Added;
                                objTactic.Cost = objTactic.Cost + tacticlineitemcostmonth;
                            }
                            db.Entry(objTactic).State = EntityState.Modified;
                        }
                        else if (lCost < objLineitem.Cost)
                        {
                            var diffcost = objLineitem.Cost - lCost;
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

                        }

                        objLineitem.Cost = lCost;
                    }
                    objLineitem.ModifiedBy = Sessions.User.UserId;
                    objLineitem.ModifiedDate = DateTime.Now;
                    db.Entry(objLineitem).State = EntityState.Modified;
                    int result = Common.InsertChangeLog(objTactic.Plan_Campaign_Program.Plan_Campaign.PlanId, null, objLineitem.PlanLineItemId, objLineitem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                    result = db.SaveChanges();

                    //// Calculate TotalLineItemCost.
                    double totalLineitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == objTactic.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);

                    //// Insert or Modified LineItem Data.
                    var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == objTactic.PlanTacticId && l.LineItemTypeId == null);

                    //// if Tactic total cost is greater than totalLineItem cost then Insert LineItem record otherwise delete objOtherLineItem data from table Plan_Campaign_Program_Tactic_LineItem. 
                    if (objOtherLineItem == null)
                    {
                        //// Insert New record to table.
                        Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                        objNewLineitem.PlanTacticId = objTactic.PlanTacticId;
                        objNewLineitem.Title = Common.LineItemTitleDefault + objTactic.Title;
                        if (objTactic.Cost > totalLineitemCost)
                        {
                            objNewLineitem.Cost = objTactic.Cost - totalLineitemCost;
                        }
                        else
                        {
                            objNewLineitem.Cost = 0;
                        }
                        objNewLineitem.Description = string.Empty;
                        objNewLineitem.CreatedBy = Sessions.User.UserId;
                        objNewLineitem.CreatedDate = DateTime.Now;
                        db.Entry(objNewLineitem).State = EntityState.Added;
                        db.SaveChanges();
                    }
                    else
                    {
                        objOtherLineItem.IsDeleted = false;
                        if (objTactic.Cost > totalLineitemCost)
                        {
                            objOtherLineItem.Cost = objTactic.Cost - totalLineitemCost;
                        }
                        else
                        {
                            objOtherLineItem.Cost = 0;
                        }
                        db.Entry(objOtherLineItem).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    //Added By Rahul Shah on 16/10/2015 for PL 1559
                    double totalLineitemCost1 = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == objTactic.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
                    var objOtherLineItemNew = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == objTactic.PlanTacticId && l.LineItemTypeId == null);
                    tacticostNew = objTactic.Cost;
                    
                    return Json(new { lineItemCost = totalLineitemCost1, tacticCost = tacticostNew, otherLineItemCost = objOtherLineItemNew.Cost }, JsonRequestBehavior.AllowGet);
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
        #endregion

        public JsonResult CalculateMQL(int tactictid, int TacticTypeId, double projectedStageValue, bool RedirectType, bool isTacticTypeChange, int? StageID)
        {
            DateTime StartDate = new DateTime();
            DateTime EndDate = new DateTime();
            string stageMQL = Enums.Stage.MQL.ToString();
            int tacticStageLevel = 0;
            try
            {
                int levelMQL = db.Stages.Single(stage => stage.ClientId.Equals(Sessions.User.ClientId) && stage.Code.Equals(stageMQL) && stage.IsDeleted == false).Level.Value;

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
        public void SendEmailnotification(int PlanID, int ChangeID, Guid oldOwnerID, Guid NewOwnerID, string PlanTitle, string CampaignTitle, string ProgramTitle, string Title, string section)
        {

            try
            {
                //Send Email Notification For Owner changed.
                if (NewOwnerID != oldOwnerID && NewOwnerID != Guid.Empty)
                {
                    if (Sessions.User != null)
                    {
                        List<string> lstRecepientEmail = new List<string>();
                        List<User> UsersDetails = new List<BDSService.User>();
                        var csv = string.Concat(NewOwnerID.ToString(), ",", oldOwnerID.ToString(), ",", Sessions.User.UserId.ToString());

                        try
                        {
                            UsersDetails = objBDSServiceClient.GetMultipleTeamMemberDetails(csv, Sessions.ApplicationId);
                        }
                        catch (Exception e)
                        {
                            ErrorSignal.FromCurrentContext().Raise(e);


                        }

                        var NewOwner = UsersDetails.Where(u => u.UserId == NewOwnerID).Select(u => u).FirstOrDefault();
                        var ModifierUser = UsersDetails.Where(u => u.UserId == Sessions.User.UserId).Select(u => u).FirstOrDefault();
                        if (NewOwner.Email != string.Empty)
                        {
                            lstRecepientEmail.Add(NewOwner.Email);
                        }
                        string NewOwnerName = NewOwner.FirstName + " " + NewOwner.LastName;
                        string ModifierName = ModifierUser.FirstName + " " + ModifierUser.LastName;

                        if (lstRecepientEmail.Count > 0)
                        {
                            string strURL = GetNotificationURLbyStatus(PlanID, ChangeID, section);
                            ////Added by Rahul Shah on 10/09/2015 fo PL Ticket #1521
                            if (Enums.Section.Program.ToString().ToLower() == section)
                            {

                                Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Program.ToString().ToLower(), strURL);
                            }
                            else if (Enums.Section.Campaign.ToString().ToLower() == section)
                            {
                                Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Campaign.ToString().ToLower(), strURL);
                            }
                            else
                            {
                                Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Tactic.ToString().ToLower(), strURL);
                            }
                            //Common.SendNotificationMailForOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, Title, ProgramTitle, CampaignTitle, PlanTitle, Enums.Section.Campaign.ToString().ToLower(), strURL); ////Added by Rahul Shah on 03/09/2015 fo PL Ticket #1521
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
                objplangrid.Revenue = Revenue;
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

        #region method to get owner full name by guid
        public string GetUserName(Guid UserGuid)
        {

            var userName = lstUserDetails.Where(user => user.UserId == UserGuid).ToList();

            if (userName.Count > 0)
            {
                return UserGuid.ToString();
            }
            return "";
        }
        #endregion
        #region method to calculate tactic cost , revenue and MQl
        protected void CalculateTacticCostRevenue(int? modelId, List<int> lsttacticId, List<Plan_Campaign_Program_Tactic> programtactic, int PlanId)
        {

            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(mdl => mdl.ClientId == Sessions.User.ClientId && mdl.IsDeleted == false);
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
                                          MQL = Math.Round(tactic.MQLValue, 0, MidpointRounding.AwayFromZero),
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
        public JsonResult CheckPermissionByOwner(string NewOwnerID, string UpdateType, int updatedid = 0)
        {
            string cellTextColor = string.Empty;
            string CellBackGroundcolor = string.Empty;
            string IsLocked = string.Empty;
            List<Plan_Campaign_Program_Tactic> TacticfilterList = new List<Plan_Campaign_Program_Tactic>();
            List<int> CustomTacticids = new List<int>();
            List<int> lsteditableEntityIds = new List<int>();
            Guid OwnerID = new Guid(NewOwnerID);
            bool IsEditable = false;
            try
            {
                if (TempData["lsteditableEntityIds"] != null)
                {
                    lsteditableEntityIds = (List<int>)TempData["lsteditableEntityIds"];
                    TempData["lsteditableEntityIds"] = lsteditableEntityIds;
                }
                List<Guid> lstSubordinatesIds = new List<Guid>();
                bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                if (IsTacticAllowForSubordinates)
                {
                    lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
                }
                if (TempData["TacticfilterList"] != null)
                {
                    TacticfilterList = (List<Plan_Campaign_Program_Tactic>)TempData["TacticfilterList"];
                    TempData["TacticfilterList"] = TacticfilterList;
                    IsEditable = OwnerID.Equals(Sessions.User.UserId) == true ? true : false;
                    if (UpdateType.ToLower() == Enums.ChangeLog_ComponentType.tactic.ToString())
                    {
                        CustomTacticids = new List<int>(updatedid);
                        CellBackGroundcolor = "background-color:#E4F1E1;";
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
                    if (IsEditable == false)
                    {
                        if (lstSubordinatesIds.Contains(OwnerID))
                        {

                            if (CustomTacticids.Count > 0 && lsteditableEntityIds.Select(x => x).Intersect(CustomTacticids).Count() != CustomTacticids.Count)
                            {
                                IsLocked = "1";
                                cellTextColor = "color:#999; " + CellBackGroundcolor + "";
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
                            cellTextColor = "color:#999; " + CellBackGroundcolor + "";

                        }
                    }
                    else
                    {
                        cellTextColor = "style='color:#000; " + CellBackGroundcolor + "'";
                        IsLocked = "0";
                    }
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
                List<Guid> lstSubordinatesIds = new List<Guid>();
                if (IsPlanEditSubordinatesAuthorized)
                    lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);

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
                               where _tac.IsDeleted.Equals(false) && _tac.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(_tac.CreatedBy)
                               select _tac).ToList().Where(_tac => lst_AllPrograms.Select(_prgram => _prgram.PlanProgramId).Contains(_tac.PlanProgramId)).ToList();

                //// Get Improvement Tactics those created by Current User.
                lst_ImprvTactics = (from _imprvCampagn in db.Plan_Improvement_Campaign
                                    where _imprvCampagn.ImprovePlanId == PlanId
                                    join _imprvPrgrm in db.Plan_Improvement_Campaign_Program on _imprvCampagn.ImprovementPlanCampaignId equals _imprvPrgrm.ImprovementPlanCampaignId
                                    join _imprvTactic in db.Plan_Improvement_Campaign_Program_Tactic on _imprvPrgrm.ImprovementPlanProgramId equals _imprvTactic.ImprovementPlanProgramId
                                    where _imprvTactic.IsDeleted.Equals(false) && _imprvTactic.CreatedBy.Equals(Sessions.User.UserId)
                                    select _imprvTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

                lst_CampaignIds = lst_AllCampaigns.Where(_campagn => _campagn.CreatedBy.Equals(Sessions.User.UserId)).Select(_campagn => _campagn.PlanCampaignId).ToList();
                lst_ProgramIds = lst_AllPrograms.Where(_prgram => _prgram.CreatedBy.Equals(Sessions.User.UserId)).Select(_prgram => _prgram.PlanProgramId).ToList();

                if (lst_Tactics.Count() > 0)
                {
                    //// Check custrom restriction permissions for Tactic.
                    List<int> lstTacticIds = lst_Tactics.Select(tactic => tactic.PlanTacticId).ToList();
                    List<int> editableTacticIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);
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
                return num.ToString("#,#");
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

        public string GetOwnerName(string UserGuid)
        {
            var OwnerName = "";
            if(UserGuid != "")
            {
               if(lstUserDetails == null ||lstUserDetails.Count == 0)
               {
                 lstUserDetails =  objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
               }
           
            var userName = lstUserDetails.Where(user => user.UserId.ToString() == UserGuid).Select(user=>new{
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

        #endregion

        #region "Feature: Copy Tactic/Program/Campaign between Plan"

        #region "Bind Planlist & Tree list"
        public ActionResult LoadCopyEntityPopup(string entityId, string section)
        {
            CopyEntiyBetweenPlanModel objModel = new CopyEntiyBetweenPlanModel();
            try
            {
            string planId = !string.IsNullOrEmpty(entityId) ? entityId.Split(new char[] { '_' })[0] : string.Empty;
            entityId = !string.IsNullOrEmpty(entityId) ? entityId.Split(new char[] { '_' })[1] : string.Empty;

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
                        }
                }
            }
            #endregion

                #region "Get Plan List"
                HomePlanModelHeader planmodel = new Models.HomePlanModelHeader();
                // var PlanId = Sessions.PlanId;
                planmodel = Common.GetPlanHeaderValue(int.Parse(planId), onlyplan: true);
                List<SelectListItem> lstPlans = planmodel.plans;
                #endregion

                // Handle user can not copy entity (Tactic/Program/Campaign) to the source Plan.
                if (lstPlans != null && lstPlans.Count > 0 && !string.IsNullOrEmpty(planId))
                {
                    var objPlan = lstPlans.Where(plan => plan.Value == planId).FirstOrDefault();
                    if (objPlan != null)
                        lstPlans.Remove(objPlan);
                }

                ViewBag.plans = lstPlans;
                #region "Get Model"
                
                int selectedPlanId = lstPlans != null ? Convert.ToInt32(lstPlans.FirstOrDefault().Value) : 0;
                //selectedPlanId = 14832;
                objModel = GetParentEntitySelectionList(selectedPlanId);
                objModel.srcSectionType = section;
                objModel.srcEntityId = entityId;
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
            objModel = GetParentEntitySelectionList(PlanId,true);
            #endregion

            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        private CopyEntiyBetweenPlanModel GetParentEntitySelectionList(int planId,bool isAjaxCall = false)
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
                objPlanMapping.Name = isAjaxCall?HttpUtility.HtmlDecode(objPlan.Title):HttpUtility.HtmlEncode(objPlan.Title);
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
        public JsonResult ClonetoOtherPlan(string CloneType, string srcEntityId, string destEntityID, string srcPlanID, string destPlanID, string sourceEntityTitle)
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
                    Plan_Campaign objCampaign=db.Plan_Campaign.Where(campagn => campagn.PlanCampaignId == sourceEntityId).FirstOrDefault();
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
                        UpdateParentEntityStartEndData(sourceEntityId, destEntityId, CloneType, srcPlanId,destPlanId);
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
                 sourceEntityHtmlDecodedTitle=HttpUtility.HtmlDecode(sourceEntityTitle);
                #endregion
            }
            catch (Exception ex)
            {
                return Json(new { msg = Common.objCached.ExceptionErrorMessage, isSuccess = false }, JsonRequestBehavior.AllowGet);
               // throw ex;
            }
            return Json(new { msg = Common.objCached.CloneEntitySuccessMessage.Replace("{0}", CloneType).Replace("{1}", sourceEntityHtmlDecodedTitle).Replace("{2}", destPlanTitle), isSuccess = true }, JsonRequestBehavior.AllowGet);
        }

        public List<PlanTactic_TacticTypeMapping> CheckTacticTypeIdToDestinationModel(string CloneType, int sourceEntityId, int destModelId, ref string invalidTacticIds)
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
                        objTacticMapping.TargetStageId =lstTacticType.FirstOrDefault().StageId; // Destination Model StageId.
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
                                invalidTacticIds += childTactic.PlanTacticId.ToString() + ",";
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
                            invalidTacticIds += childTactic.PlanTacticId.ToString() + ",";
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

        /// <summary>
        /// Created By: Viral Kadiya on 11/23/2015 for PL ticket #1748: Abiliy to move a tactic/program/campign between different plans.
        /// Update Parent Entity(i.e. Campaign,Program) Start-End Date.
        /// </summary>
        /// <param name="srcPlanEntityId">Source EntityId like PlanTacticId </param>
        /// <param name="destPlanEntityId">Destination EntityId: Under which source entity copied</param>
        /// <param name="CloneType">CloneType: Entity will be copied</param>
        /// <returns></returns>
        public void UpdateParentEntityStartEndData(int srcPlanEntityId, int destPlanEntityId, string CloneType,int srcPlanId, int destPlanId)
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
                        destProgram.StartDate = objClonehelper.GetResultDate(srcTactic.StartDate, destProgram.StartDate,true,isParentUpdate:true);

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

        #endregion
    }
}

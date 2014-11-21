using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Transactions;
using System.Data.Objects;
using System.IO;
using RevenuePlanner.BDSService;
using System.Web;
using Newtonsoft.Json;

namespace RevenuePlanner.Controllers
{
    public class PlanController : CommonController
    {
        #region Variables

        private MRPEntities db = new MRPEntities();
        private BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();

        private DateTime CalendarStartDate;
        private DateTime CalendarEndDate;

        //Added By : Kalpesh Sharma : Functional Review Points #697
        private string DefaultLineItemTitle = "Line Item";

        #endregion

        #region List

        #endregion

        #region Create
        /// <summary>
        /// Function to create Plan
        /// </summary>
        /// <returns></returns>
        /// added id parameter by kunal on 01/17/2014 for edit plan
        public ActionResult Create(int id = 0, bool isBackFromAssortment = false)
        {
            /*Added by Mitesh Vaishnav on 25/07/2014 for PL ticket 619*/
            if (isBackFromAssortment == true)
            {
                TempData["IsBackFromAssortment"] = true;
            }
            /*End :Added by Mitesh Vaishnav on 25/07/2014 for PL ticket 619*/
            // Added by dharmraj to check user activity permission
            bool IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            ViewBag.IsPlanCreateAuthorized = IsPlanCreateAuthorized;
            if (id == 0 && !IsPlanCreateAuthorized)
            {
                return AuthorizeUserAttribute.RedirectToNoAccess();
            }

            try
            {
                // Start - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                bool IsBusinessUnitEditable = Common.IsBusinessUnitEditable(db.Plans.Where(a => a.PlanId == id).Select(a => a.Model.BusinessUnitId).FirstOrDefault());
                if (id != 0 && !IsBusinessUnitEditable)
                    return AuthorizeUserAttribute.RedirectToNoAccess();
                // End - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units

                if (id > 0)
                {
                    // Added by Dharmraj Mangukiya for edit authentication of plan, PL ticket #519
                    var objplan = db.Plans.FirstOrDefault(m => m.PlanId == id && m.IsDeleted == false);
                    bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                    bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                    //Get all subordinates of current user upto n level
                    var lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
                    bool IsPlanEditable = false;
                    if (objplan.CreatedBy.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
                    {
                        IsPlanEditable = true;
                    }
                    else if (IsPlanEditAllAuthorized)
                    {
                        IsPlanEditable = true;
                    }
                    else if (IsPlanEditSubordinatesAuthorized)
                    {
                        if (lstSubOrdinates.Contains(objplan.CreatedBy))
                        {
                            IsPlanEditable = true;
                        }
                    }

                    if (!IsPlanEditable)
                    {
                        return AuthorizeUserAttribute.RedirectToNoAccess();
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
                    return RedirectToAction("Index", "Login");
                }
            }

            PlanModel objPlanModel = new PlanModel();
            try
            {
                ViewBag.ActiveMenu = Enums.ActiveMenu.Plan;
                //objPlanModel.IsDirector = Sessions.IsDirector;
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
                    var objplan = db.Plans.Where(m => m.PlanId == id && m.IsDeleted == false).FirstOrDefault();/*changed by Nirav for plan consistency on 14 apr 2014*/
                    objPlanModel.PlanId = objplan.PlanId;
                    objPlanModel.ModelId = objplan.ModelId;
                    objPlanModel.Title = objplan.Title;
                    objPlanModel.Year = objplan.Year;
                    //objPlanModel.MQls = Convert.ToString(objplan.MQLs);
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
                            Listyear.Add(new SelectListItem { Text = (yr + i).ToString(), Value = (yr + i).ToString(), Selected = false });//Modified by Mitesh Vaishnav for PL ticket #622
                        }

                        year = Listyear;
                        TempData["selectYearList"] = new SelectList(year, "Value", "Text");//Modified by Mitesh Vaishnav for PL ticket #622
                    }
                    #endregion
                    ViewBag.IsBusinessUnitEditable = Common.IsBusinessUnitEditable(db.Models.Where(b => b.ModelId == objplan.ModelId).Select(b => b.BusinessUnitId).FirstOrDefault());  // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                }
                else
                {
                    objPlanModel.Title = "Plan Title";
                    objPlanModel.GoalValue = "0";
                    objPlanModel.Budget = 0;
                    objPlanModel.Year = DateTime.Now.Year.ToString(); // Added by dharmraj to add default year in year dropdown
                    ViewBag.IsBusinessUnitEditable = true; // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                }
                //end

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
        #endregion

        //Commented Code - 14Apr2014
        ///// <summary>
        ///// POST: Save Plan
        ///// </summary>
        ///// <param name="form"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public JsonResult SavePlan(PlanModel objPlanModel)
        //{

        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            Plan plan = new Plan();

        //            if (objPlanModel.PlanId != 0)
        //            {
        //                plan = db.Plans.Where(m => m.PlanId == objPlanModel.PlanId).ToList().FirstOrDefault();
        //            }
        //            else
        //            {
        //                string planDraftStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Draft.ToString())).Value;
        //                plan.Status = planDraftStatus;
        //                plan.CreatedDate = System.DateTime.Now;
        //                plan.CreatedBy = Sessions.User.UserId;
        //                plan.IsActive = true;
        //                plan.IsDeleted = false;
        //                double version = 0;
        //                var plantable = db.Plans.Where(m => m.ModelId == objPlanModel.ModelId && m.IsActive == true && m.IsDeleted == false).FirstOrDefault();
        //                if (plantable != null)
        //                {
        //                    version = Convert.ToDouble(plantable.Version) + 0.1;
        //                }
        //                else
        //                {
        //                    version = 1;
        //                }
        //                plan.Version = version.ToString();
        //            }

        //            plan.Title = objPlanModel.Title.Trim();
        //            plan.MQLs = Convert.ToInt64(objPlanModel.MQls.Trim().Replace(",", "").Replace("$", ""));
        //            plan.Budget = Convert.ToDouble(objPlanModel.Budget.ToString().Trim().Replace(",", "").Replace("$", ""));
        //            plan.ModelId = objPlanModel.ModelId;
        //            plan.Year = objPlanModel.Year;
        //            if (objPlanModel.PlanId == 0)
        //            {
        //                db.Plans.Add(plan);
        //            }
        //            else
        //            {
        //                plan.ModifiedBy = Sessions.User.UserId;
        //                plan.ModifiedDate = System.DateTime.Now;
        //                db.Entry(plan).State = EntityState.Modified;
        //            }

        //            int result = db.SaveChanges();
        //            if (objPlanModel.PlanId == 0)
        //            {
        //                Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
        //            }
        //            else
        //            {
        //                Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
        //            }
        //            if (result > 0)
        //            {
        //                Sessions.PlanId = plan.PlanId;

        //                //Create default Plan Improvement Campaign, Program
        //                int returnValue = CreatePlanImprovementCampaignAndProgram();
        //            }

        //            return Json(new { id = Sessions.PlanId, redirect = Url.Action("Assortment", new { ismsg = "Plan Saved Successfully." }) });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ErrorSignal.FromCurrentContext().Raise(e);
        //    }
        //    return Json(new { id = 0 });
        //}

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
                    if (objPlanModel.PlanId == 0)  //Add Mode
                    {
                        string planDraftStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Draft.ToString())).Value;
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
                        plan.GoalValue = Convert.ToInt64(objPlanModel.GoalValue.Trim().Replace(",", "").Replace("$", ""));
                        plan.AllocatedBy = objPlanModel.AllocatedBy;
                        plan.Budget = Convert.ToDouble(objPlanModel.Budget.ToString().Trim().Replace(",", "").Replace("$", ""));
                        plan.ModelId = objPlanModel.ModelId;
                        plan.Year = objPlanModel.Year;
                        db.Plans.Add(plan);
                    }
                    else //Edit Mode
                    {
                        plan = db.Plans.Where(m => m.PlanId == objPlanModel.PlanId).ToList().FirstOrDefault();

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
                        plan.GoalValue = Convert.ToInt64(objPlanModel.GoalValue.Trim().Replace(",", "").Replace("$", ""));
                        plan.AllocatedBy = objPlanModel.AllocatedBy;
                        plan.Description = objPlanModel.Description;    /* Added by Sohel Pathan on 04/08/2014 for PL ticket #623 */
                        plan.Budget = Convert.ToDouble(objPlanModel.Budget.ToString().Trim().Replace(",", "").Replace("$", ""));
                        plan.ModelId = objPlanModel.ModelId;
                        plan.Year = objPlanModel.Year;
                        plan.ModifiedBy = Sessions.User.UserId;
                        plan.ModifiedDate = System.DateTime.Now;
                        db.Entry(plan).State = EntityState.Modified;

                        // Start - Added by Sohel Pathan on 01/08/2014 for PL ticket #623
                        #region Update Budget Allocation Value
                        if (BudgetInputValues != "")
                        {
                            string[] arrBudgetInputValues = BudgetInputValues.Split(',');

                            var PrevPlanBudgetAllocationList = db.Plan_Budget.Where(pb => pb.PlanId == objPlanModel.PlanId).Select(pb => pb).ToList();
                            //PrevPlanBudgetAllocationList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                            if (arrBudgetInputValues.Length == 12)
                            {
                                for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                {
                                    // Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                    bool isExists = false;
                                    if (PrevPlanBudgetAllocationList != null)
                                    {
                                        if (PrevPlanBudgetAllocationList.Count > 0)
                                        {
                                            var updatePlanBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == ("Y" + (i + 1))).FirstOrDefault();
                                            if (updatePlanBudget != null)
                                            {
                                                if (arrBudgetInputValues[i] != "")
                                                {
                                                    var newValue = Convert.ToDouble(arrBudgetInputValues[i]);
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
                                    }
                                    if (!isExists && arrBudgetInputValues[i] != "")
                                    {
                                        // End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                        Plan_Budget objPlanBudget = new Plan_Budget();
                                        objPlanBudget.PlanId = objPlanModel.PlanId;
                                        objPlanBudget.Period = "Y" + (i + 1);
                                        objPlanBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                        objPlanBudget.CreatedBy = Sessions.User.UserId;
                                        objPlanBudget.CreatedDate = DateTime.Now;
                                        db.Entry(objPlanBudget).State = EntityState.Added;
                                    }
                                }
                            }
                            else if (arrBudgetInputValues.Length == 4)
                            {
                                int QuarterCnt = 1;
                                for (int i = 0; i < 4; i++)
                                {
                                    // Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                    bool isExists = false;
                                    if (PrevPlanBudgetAllocationList != null)
                                    {
                                        if (PrevPlanBudgetAllocationList.Count > 0)
                                        {
                                            var thisQuartersMonthList = PrevPlanBudgetAllocationList.Where(pb => pb.Period == ("Y" + (QuarterCnt)) || pb.Period == ("Y" + (QuarterCnt + 1)) || pb.Period == ("Y" + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                            var thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                            if (thisQuarterFirstMonthBudget != null)
                                            {
                                                if (arrBudgetInputValues[i] != "")
                                                {
                                                    var thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                                    var thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                                    var newValue = Convert.ToDouble(arrBudgetInputValues[i]);

                                                    if (thisQuarterTotalBudget != newValue)
                                                    {
                                                        var BudgetDiff = newValue - thisQuarterTotalBudget;
                                                        if (BudgetDiff > 0)
                                                        {
                                                            thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            int j = 1;
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
                                                                    thisQuarterFirstMonthBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == ("Y" + (QuarterCnt + j))).FirstOrDefault();
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
                                    }
                                    if (!isExists && arrBudgetInputValues[i] != "")
                                    {
                                        // End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                        Plan_Budget objPlanBudget = new Plan_Budget();
                                        objPlanBudget.PlanId = objPlanModel.PlanId;
                                        objPlanBudget.Period = "Y" + QuarterCnt;
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
                        TempData["SuccessMessage"] = "Plan Saved Successfully";
                        return Json(new { id = Sessions.PlanId, redirect = Url.Action("Budgeting") });
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

        #region Calculate Plan Budget
        /// <summary>
        /// Calculate Plan budget based on goal type selected
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>15/07/2014</CreatedDate>
        /// <param name="modelId"></param>
        /// <param name="goalType"></param>
        /// <param name="goalValue"></param>
        /// <returns></returns>
        public JsonResult CalculateBudget(int modelId, string goalType, string goalValue)
        {
            string msg1 = "", msg2 = "";
            string input1 = "0", input2 = "0";
            double ADS = 0;

            try
            {
                if (modelId != 0)
                {
                    string marketing = Enums.Funnel.Marketing.ToString();
                    double ADSValue = db.Model_Funnel.Single(mf => mf.ModelId == modelId && mf.Funnel.Title == marketing).AverageDealSize;
                    ADS = ADSValue;
                }

                if (goalType.ToString() != "")
                {
                    BudgetAllocationModel objBudgetAllocationModel = new BudgetAllocationModel();
                    objBudgetAllocationModel = Common.CalculateBudgetInputs(modelId, goalType, goalValue, ADS);
                    List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();

                    if (goalType.ToString().ToLower() == Enums.PlanGoalType.INQ.ToString().ToLower())
                    {
                        msg1 = stageList.Where(a => a.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(a => a.Title.ToLower()).FirstOrDefault();
                        msg2 = " in revenue";
                        input1 = objBudgetAllocationModel.MQLValue.ToString();
                        input2 = objBudgetAllocationModel.RevenueValue.ToString();

                    }
                    else if (goalType.ToString().ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower())
                    {
                        msg1 = stageList.Where(a => a.Code.ToLower() == Enums.PlanGoalType.INQ.ToString().ToLower()).Select(a => a.Title.ToLower()).FirstOrDefault();
                        msg2 = " in revenue";
                        input1 = objBudgetAllocationModel.INQValue.ToString();
                        input2 = objBudgetAllocationModel.RevenueValue.ToString();
                    }
                    else if (goalType.ToString().ToLower() == Enums.PlanGoalType.Revenue.ToString().ToLower())
                    {
                        msg1 = stageList.Where(a => a.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(a => a.Title.ToLower()).FirstOrDefault();
                        msg2 = stageList.Where(a => a.Code.ToLower() == Enums.PlanGoalType.INQ.ToString().ToLower()).Select(a => a.Title.ToLower()).FirstOrDefault();
                        input1 = objBudgetAllocationModel.MQLValue.ToString();
                        input2 = objBudgetAllocationModel.INQValue.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { msg1 = msg1, msg2 = msg2, input1 = input1, input2 = input2, ADS = ADS }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Plan Details in Slide Panel
        /// <summary>
        /// Action to return view of Plan Details to show in Slide Panel
        /// </summary>
        /// <param name="id"></param>
        /// <param name="RedirectType"></param>
        /// <returns></returns>
        public PartialViewResult PlanDetails(int id = 0, string RedirectType = "", string CalledFromBudget = "")
        {
            ViewBag.CalledFromBudget = CalledFromBudget;
            if (id > 0)
            {
                TempData["selectList"] = GetModelName();

                var objplan = db.Plans.FirstOrDefault(m => m.PlanId == id && m.IsDeleted == false);
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                //Get all subordinates of current user upto n level
                var lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
                bool IsPlanEditable = false;
                if (objplan.CreatedBy.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditAllAuthorized)
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditSubordinatesAuthorized)
                {
                    if (lstSubOrdinates.Contains(objplan.CreatedBy))
                    {
                        IsPlanEditable = true;
                    }
                }

                if (!IsPlanEditable)
                {
                    return null;
                }
            }

            ViewBag.IsCreated = false;
            if (RedirectType == "Assortment")
            {
                ViewBag.RedirectType = false;
            }
            else
            {
                ViewBag.RedirectType = true;
            }

            var objPlanData = db.Plans.Where(m => m.PlanId == id && m.IsDeleted == false).FirstOrDefault();

            if (objPlanData == null)
            {
                return null;
            }

            PlanModel objPlanModel = new PlanModel();
            objPlanModel.PlanId = objPlanData.PlanId;
            objPlanModel.ModelId = objPlanData.ModelId;
            objPlanModel.Title = objPlanData.Title;
            objPlanModel.Year = objPlanData.Year;
            objPlanModel.GoalType = objPlanData.GoalType;
            objPlanModel.GoalValue = Convert.ToString(objPlanData.GoalValue);
            objPlanModel.AllocatedBy = objPlanData.AllocatedBy;
            objPlanModel.Budget = objPlanData.Budget;
            objPlanModel.Description = objPlanData.Description;    /* Added by Sohel Pathan on 04/08/2014 for PL ticket #623 */
            double TotalAllocatedCampaignBudget = 0;
            var PlanCampaignBudgetList = db.Plan_Campaign_Budget.Where(pcb => pcb.Plan_Campaign.PlanId == objPlanData.PlanId && pcb.Plan_Campaign.IsDeleted == false).Select(a => a.Value).ToList();
            if (PlanCampaignBudgetList.Count > 0)
            {
                TotalAllocatedCampaignBudget = PlanCampaignBudgetList.Sum();
            }
            objPlanModel.TotalAllocatedCampaignBudget = TotalAllocatedCampaignBudget;
            ViewBag.IsBusinessUnitEditable = Common.IsBusinessUnitEditable(db.Models.Where(b => b.ModelId == objPlanData.ModelId).Select(b => b.BusinessUnitId).FirstOrDefault());  // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units

            double planBudget = objPlanData.Budget;
            double planRemainingBudget = planBudget - TotalAllocatedCampaignBudget;
            ViewBag.planRemainingBudget = planRemainingBudget;

            if (Sessions.User.UserId == objPlanData.CreatedBy)
            {
                ViewBag.IsOwner = true;

                // Added by Dharmraj Mangukiya to hide/show delete program as per custom restrictions PL ticket #577
                //var AllTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.PlanCampaignId == pc.PlanCampaignId && t.IsDeleted == false).ToList();
                //bool IsCampaignDeleteble = true;
                //if (AllTactic.Count > 0)
                //{
                //    var OthersTactic = AllTactic.Where(t => t.CreatedBy != Sessions.User.UserId).ToList();
                //    if (OthersTactic.Count > 0)
                //    {
                //        IsCampaignDeleteble = false;
                //    }
                //    else
                //    {
                //        var lstUserCustomRestriction = Common.GetUserCustomRestriction();
                //        int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                //        var lstAllowedVertical = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId).ToList();
                //        var lstAllowedGeography = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId).ToList();
                //        var lstAllowedBusinessUnit = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).Select(r => r.CustomFieldId).ToList();
                //        var a = AllTactic.Where(t => t.CreatedBy == Sessions.User.UserId && (!lstAllowedGeography.Contains(t.GeographyId.ToString()) || !lstAllowedVertical.Contains(t.VerticalId.ToString()) || !lstAllowedBusinessUnit.Contains(t.BusinessUnitId.ToString()))).ToList();
                //        if (AllTactic.Where(t => t.CreatedBy == Sessions.User.UserId && (!lstAllowedGeography.Contains(t.GeographyId.ToString()) || !lstAllowedVertical.Contains(t.VerticalId.ToString()) || !lstAllowedBusinessUnit.Contains(t.BusinessUnitId.ToString()))).ToList().Count > 0)
                //        {
                //            IsCampaignDeleteble = false;
                //        }
                //    }
                //}

                //ViewBag.IsCampaignDeleteble = IsCampaignDeleteble;
            }
            else
            {
                ViewBag.IsOwner = false;
                ViewBag.IsCampaignDeleteble = false;
            }

            return PartialView("PlanAssortment", objPlanModel);
        }
        #endregion

        #endregion

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
            var objPlan_Campaign_Program_Tactic = (from pc in db.Plan_Campaign
                                                   join pcp in db.Plan_Campaign_Program on pc.PlanCampaignId equals pcp.PlanCampaignId
                                                   join pcpt in db.Plan_Campaign_Program_Tactic on pcp.PlanProgramId equals pcpt.PlanProgramId
                                                   where pc.PlanId == planId && pcpt.IsDeleted == false
                                                   select pcpt).FirstOrDefault();
            if (objPlan_Campaign_Program_Tactic != null)
            {
                List<string> lstTacticType = db.TacticTypes.Where(t => t.ModelId == modelId).Select(o => o.Title).ToList();
                return (from pc in db.Plan_Campaign
                        join pcp in db.Plan_Campaign_Program on pc.PlanCampaignId equals pcp.PlanCampaignId
                        join pcpt in db.Plan_Campaign_Program_Tactic on pcp.PlanProgramId equals pcpt.PlanProgramId
                        where pc.PlanId == planId && pcpt.IsDeleted == false && !lstTacticType.Contains(pcpt.TacticType.Title)
                        select pcpt.Title).ToList();
            }
            else
            {
                return new List<string>();
            }
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
            var objPlan_Campaign_Program_Tactic = (from pc in db.Plan_Campaign
                                                   join pcp in db.Plan_Campaign_Program on pc.PlanCampaignId equals pcp.PlanCampaignId
                                                   join pcpt in db.Plan_Campaign_Program_Tactic on pcp.PlanProgramId equals pcpt.PlanProgramId
                                                   where pc.PlanId == planId && pcpt.IsDeleted == false
                                                   select pcpt).FirstOrDefault();
            if (objPlan_Campaign_Program_Tactic != null)
            {
                Guid businessUnitId = db.Models.Where(m => m.IsDeleted == false && m.ModelId == modelId).Select(o => o.BusinessUnitId).FirstOrDefault();

                List<string> lstTacticType = db.TacticTypes.Where(t => t.ModelId == modelId).Select(o => o.Title).ToList();
                List<Plan_Campaign_Program_Tactic> lstTactic = (from pc in db.Plan_Campaign
                                                                join pcp in db.Plan_Campaign_Program on pc.PlanCampaignId equals pcp.PlanCampaignId
                                                                join pcpt in db.Plan_Campaign_Program_Tactic on pcp.PlanProgramId equals pcpt.PlanProgramId
                                                                where pc.PlanId == planId && pcpt.IsDeleted == false && lstTacticType.Contains(pcpt.TacticType.Title)
                                                                select pcpt).ToList();
                foreach (var tactic in lstTactic)
                {
                    if (tactic != null)
                    {
                        int newTacticTypeId = db.TacticTypes.Where(t => t.ModelId == modelId && t.Title == tactic.TacticType.Title).Select(i => i.TacticTypeId).FirstOrDefault();
                        if (newTacticTypeId > 0)
                        {
                            tactic.ModifiedBy = Sessions.User.UserId;
                            tactic.ModifiedDate = DateTime.Now;
                            tactic.TacticTypeId = newTacticTypeId; //Update TacticTypeId column in Plan_Campaign_Program_Tactic Table based on the new model selected
                            if (businessUnitId != null)
                            {
                                tactic.BusinessUnitId = businessUnitId; //Update BussinessUnitID column in Plan_Campaign_Program_Tactic Table based on the new model selected
                            }
                            db.Entry(tactic).State = EntityState.Modified;
                            db.SaveChanges();
                        }
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
                if (Convert.ToString(objModel.Status).ToLower() == Convert.ToString(Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value).ToLower())
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
            //List<Model> lstmodel = new List<Model>();

            List<Model> objModelList = new List<Model>();
            List<Model> lstModels = new List<Model>();
            try
            {
                Guid clientId = Sessions.User.ClientId;
                List<Guid> objBusinessUnit = new List<Guid>();

                var lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList();
                List<Guid> lstAllowedBusinessUnitIds = new List<Guid>();
                if (lstAllowedBusinessUnits.Count > 0)
                    lstAllowedBusinessUnits.ForEach(g => lstAllowedBusinessUnitIds.Add(Guid.Parse(g)));
                if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin) && lstAllowedBusinessUnitIds.Count == 0)//if (Sessions.IsSystemAdmin || Sessions.IsClientAdmin || Sessions.IsDirector)
                {
                    objBusinessUnit = db.BusinessUnits.Where(bu => bu.ClientId == clientId && bu.IsDeleted == false).Select(bu => bu.BusinessUnitId).ToList();
                }
                else
                {
                    // Start - Added by Sohel Pathan on 30/06/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                    if (lstAllowedBusinessUnitIds.Count > 0)
                    {
                        objBusinessUnit = db.BusinessUnits.Where(bu => lstAllowedBusinessUnitIds.Contains(bu.BusinessUnitId) && bu.IsDeleted == false).Select(bu => bu.BusinessUnitId).ToList();
                    }
                    else
                    {
                        objBusinessUnit = db.BusinessUnits.Where(bu => bu.BusinessUnitId == Sessions.User.BusinessUnitId && bu.IsDeleted == false).Select(bu => bu.BusinessUnitId).ToList();
                    }
                    // End - Added by Sohel Pathan on 30/06/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                }
                //fetch published models by businessunitid
                string strPublish = Convert.ToString(Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value);
                string strDraft = Convert.ToString(Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Draft.ToString())).Value);
                /*Added by Nirav shah on 20 feb 2014 for TFS Point 252 : editing a published model*/
                lstModels = (from m in db.Models
                             where m.IsDeleted == false && objBusinessUnit.Contains(m.BusinessUnitId) && (m.ParentModelId == 0 || m.ParentModelId == null)
                             select m).ToList();
                if (lstModels != null && lstModels.Count > 0)
                {
                    foreach (Model obj in lstModels)
                    {
                        objModelList.Add(GetLatestModelVersion(obj));
                    }
                }

                List<Model> objModelDraftList = objModelList.Where(m => m.Status == strDraft).ToList();
                objModelList = objModelList.Where(m => m.Status == strPublish).ToList();

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

            foreach (var v in objModelList)
            {
                if (v != null)
                {
                    PlanModel objPlanModel = new PlanModel();
                    objPlanModel.ModelId = v.ModelId;
                    objPlanModel.ModelTitle = v.Title + " " + v.Version;
                    lstPlanModel.Add(objPlanModel);
                }
            }
            return lstPlanModel.OrderBy(a=>a.ModelTitle).ToList();
        }
        #endregion

        #region PlanZero
        /// <summary>
        /// Called when no plan exist.
        /// </summary>
        /// <returns></returns>
        public ActionResult PlanZero(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Plan)
        {
            ViewBag.IsDirector = Sessions.User.IsDirector;
            ViewBag.IsPlan = Enums.ActiveMenu.Plan.Equals(activeMenu);
            ViewBag.ActiveMenu = activeMenu;
            return View();
        }
        #endregion

        #region GetPlanByPlanID
        /// <summary>
        /// Get plan by plan id
        /// </summary>
        /// <param name="planid"></param>
        public JsonResult GetPlanByPlanID(int planid)
        {
            try
            {
                return Json(new
                {
                    lstHomePlanModelHeader = Common.GetPlanHeaderValue(planid),
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
        /// <returns>returns Json object with values required to show in plan/home header</returns>
        public JsonResult GetPlanByMultiplePlanIDs(string planid, string activeMenu)
        {
            planid = System.Web.HttpUtility.UrlDecode(planid);
            List<int> planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(p => int.Parse(p)).ToList();

            try
            {
                return Json(new
                {
                    lstHomePlanModelHeader = Common.GetPlanHeaderValueForMultiplePlans(planIds, activeMenu),
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
        /// <returns>Returns view as action result.</returns>
        public ActionResult ApplyToCalendar(string ismsg = "", bool isError = false)
        {
            try
            {
                // Start - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                bool IsBusinessUnitEditable = Common.IsBusinessUnitEditable(db.Plans.Where(a => a.PlanId == Sessions.PlanId).Select(a => a.Model.BusinessUnitId).FirstOrDefault());
                if (!IsBusinessUnitEditable)
                    return AuthorizeUserAttribute.RedirectToNoAccess();
                // End - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units

                //if (Sessions.RolePermission != null)
                //{
                //    Common.Permission permission = Common.GetPermission(ActionItem.Plan);
                //    switch (permission)
                //    {
                //        case Common.Permission.FullAccess:
                //            break;
                //        case Common.Permission.NoAccess:
                //            return RedirectToAction("Homezero", "Home");
                //        case Common.Permission.NotAnEntity:
                //            break;
                //        case Common.Permission.ViewOnly:
                //            ViewBag.IsViewOnly = "true";
                //            break;
                //    }
                //}

                // To get permission status for Plan Edit , By dharmraj PL #519
                //Get all subordinates of current user upto n level
                var lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
                // Get current user permission for edit own and subordinates plans.
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                var objPlan = db.Plans.FirstOrDefault(p => p.PlanId == Sessions.PlanId);
                bool IsPlanEditable = false;
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

            // Added by dharmraj to check user activity permission
            bool IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            ViewBag.IsPlanCreateAuthorized = IsPlanCreateAuthorized;

            var plan = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId));
            Sessions.BusinessUnitId = plan.Model.BusinessUnitId;
            HomePlanModel planModel = new HomePlanModel();
            planModel.objplanhomemodelheader = Common.GetPlanHeaderValue(Sessions.PlanId);
            planModel.PlanId = plan.PlanId;
            planModel.PlanTitle = plan.Title;

            //// Modified By Maninder Singh Wadhva to Address PL#203
            //planModel.CollaboratorId = GetCollaborator(plan);
            planModel.LastUpdatedDate = GetLastUpdatedDate(plan);

            //List<SelectListItem> planList = Common.GetPlan().Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).ToList();
            //planList.Single(p => p.Value.Equals(Sessions.PlanId.ToString())).Selected = true;

            /*changed by nirav Shah on 9 Jan 2014*/

            // planModel.plans = planList;

            List<SelectListItem> UpcomingActivityList = Common.GetUpcomingActivity().Select(p => new SelectListItem() { Text = p.Text, Value = p.Value.ToString() }).ToList();
            planModel.objplanhomemodelheader.UpcomingActivity = UpcomingActivityList;

            ViewBag.SuccessMessageDuplicatePlan = TempData["SuccessMessageDuplicatePlan"];
            ViewBag.ErrorMessageDuplicatePlan = TempData["ErrorMessageDuplicatePlan"];
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
            //planModel.BusinessUnitIds = Common.GetBussinessUnitIds(Sessions.User.ClientId);
            var lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList();
            List<Guid> lstAllowedBusinessUnitIds = new List<Guid>();
            if (lstAllowedBusinessUnits.Count > 0)
                lstAllowedBusinessUnits.ForEach(g => lstAllowedBusinessUnitIds.Add(Guid.Parse(g)));
            if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin) && lstAllowedBusinessUnitIds.Count == 0)//if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
            {
                //// Getting all business unit for client of director.
                planModel.BusinessUnitIds = Common.GetBussinessUnitIds(Sessions.User.ClientId);
                //Added by Nirav for Custom Dropdown - 388
                ViewBag.BusinessUnitIds = planModel.BusinessUnitIds; // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                ViewBag.showBid = true;
            }
            else
            {
                try
                {
                    // Start - Added by Sohel Pathan on 30/06/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                    if (lstAllowedBusinessUnitIds.Count > 0)
                    {
                        var list = db.BusinessUnits.Where(s => lstAllowedBusinessUnitIds.Contains(s.BusinessUnitId) && s.IsDeleted == false).ToList().Select(u => new SelectListItem
                        {
                            Text = u.Title,
                            Value = u.BusinessUnitId.ToString()
                        });
                        List<SelectListItem> items = new List<SelectListItem>(list);
                        planModel.BusinessUnitIds = items;
                        ViewBag.BusinessUnitIds = items;
                    }
                    else
                    {
                        //Added by Nirav for Custom Dropdown - 388
                        var list = db.BusinessUnits.Where(s => s.BusinessUnitId == Sessions.User.BusinessUnitId && s.IsDeleted == false).ToList().Select(u => new SelectListItem
                        {
                            Text = u.Title,
                            Value = u.BusinessUnitId.ToString()
                        });
                        List<SelectListItem> items = new List<SelectListItem>(list);
                        planModel.BusinessUnitIds = items;
                        ViewBag.BusinessUnitIds = items;
                    }
                    // End - Added by Sohel Pathan on 30/06/2014 for PL ticket #563 to apply custom restriction logic on Business Units
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

                ViewBag.showBid = true;
            }
            ViewBag.Msg = ismsg;
            ViewBag.isError = isError;
            return View(planModel);
        }

        public ActionResult PlanList(string Bid)
        {
            HomePlan objHomePlan = new HomePlan();
            //objHomePlan.IsDirector = Sessions.IsDirector;
            List<SelectListItem> planList;
            if (Bid == "false")
            {
                planList = Common.GetPlan().Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).ToList();
                if (planList.Count > 0)
                {
                    var objexists = planList.Where(p => p.Value == Sessions.PlanId.ToString()).ToList();
                    if (objexists.Count != 0)
                    {
                        planList.Single(p => p.Value.Equals(Sessions.PlanId.ToString())).Selected = true;
                    }
                    /*changed by Nirav for plan consistency on 14 apr 2014*/
                    Sessions.BusinessUnitId = Common.GetPlan().Where(m => m.PlanId == Sessions.PlanId).Select(m => m.Model.BusinessUnitId).FirstOrDefault();
                    if (!Common.IsPlanPublished(Sessions.PlanId))
                    {
                        string planPublishedStatus = Enums.PlanStatus.Published.ToString();
                        var activeplan = db.Plans.Where(p => p.PlanId == Sessions.PlanId && p.IsDeleted == false && p.Status == planPublishedStatus).ToList();
                        if (activeplan.Count > 0)
                        {
                            Sessions.PublishedPlanId = Sessions.PlanId;
                        }
                        else
                        {
                            Sessions.PublishedPlanId = 0;
                        }
                    }
                }
            }
            else
            {
                /*changed by Nirav for plan consistency on 14 apr 2014*/
                Guid bId = new Guid(Bid);
                if (Sessions.BusinessUnitId == bId)
                {
                    bId = Common.GetPlan().Where(m => m.PlanId == Sessions.PlanId).Select(m => m.Model.BusinessUnitId).FirstOrDefault();
                }
                Sessions.BusinessUnitId = bId;
                planList = Common.GetPlan().Where(s => s.Model.BusinessUnitId == bId).Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).ToList();
                if (planList.Count > 0)
                {
                    var objexists = planList.Where(p => p.Value == Sessions.PlanId.ToString()).ToList();
                    if (objexists.Count != 0)
                    {
                        planList.Single(p => p.Value.Equals(Sessions.PlanId.ToString())).Selected = true;
                    }
                    else
                    {
                        planList.FirstOrDefault().Selected = true;
                        int planID = 0;
                        int.TryParse(planList.Select(s => s.Value).FirstOrDefault(), out planID);
                        Sessions.PlanId = planID;
                        if (!Common.IsPlanPublished(Sessions.PlanId))
                        {
                            string planPublishedStatus = Enums.PlanStatus.Published.ToString();
                            var activeplan = db.Plans.Where(p => p.PlanId == Sessions.PlanId && p.IsDeleted == false && p.Status == planPublishedStatus).ToList();
                            if (activeplan.Count > 0)
                            {
                                Sessions.PublishedPlanId = planID;
                            }
                            else
                            {
                                Sessions.PublishedPlanId = 0;
                            }
                        }
                    }
                }
            }
            objHomePlan.plans = planList;

            return PartialView("_ApplytoCalendarPlanList", objHomePlan);
        }

        ///// <summary>
        ///// Getting list of collaborator for current plan.
        ///// </summary>
        ///// <param name="plan">Plan</param>
        ///// <param name="currentPlanId">PlanId</param>
        ///// <returns>Returns list of collaborators for current plan.</returns>
        //public List<string> GetCollaborator(Plan plan)
        //{
        //    List<string> collaboratorId = new List<string>();
        //    if (plan.ModifiedBy != null)
        //    {
        //        collaboratorId.Add(plan.ModifiedBy.ToString());
        //    }

        //    if (plan.CreatedBy != null)
        //    {
        //        collaboratorId.Add(plan.CreatedBy.ToString());
        //    }

        //    var planTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(t => t);

        //    var planTacticModifiedBy = planTactic.ToList().Where(t => t.ModifiedBy != null).Select(t => t.ModifiedBy.ToString()).ToList();
        //    var planTacticCreatedBy = planTactic.ToList().Select(t => t.CreatedBy.ToString()).ToList();

        //    var planProgramModifiedBy = planTactic.ToList().Where(t => t.Plan_Campaign_Program.ModifiedBy != null).Select(t => t.Plan_Campaign_Program.ModifiedBy.ToString()).ToList();
        //    var planProgramCreatedBy = planTactic.ToList().Select(t => t.Plan_Campaign_Program.CreatedBy.ToString()).ToList();

        //    var planCampaignModifiedBy = planTactic.ToList().Where(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedBy != null).Select(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedBy.ToString()).ToList();
        //    var planCampaignCreatedBy = planTactic.ToList().Select(t => t.Plan_Campaign_Program.Plan_Campaign.CreatedBy.ToString()).ToList();

        //    var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId))
        //                                                                   .Select(pc => pc);
        //    var planTacticCommentCreatedBy = planTacticComment.ToList().Select(pc => pc.CreatedBy.ToString()).ToList();

        //    collaboratorId.AddRange(planTacticCreatedBy);
        //    collaboratorId.AddRange(planTacticModifiedBy);
        //    collaboratorId.AddRange(planProgramCreatedBy);
        //    collaboratorId.AddRange(planProgramModifiedBy);
        //    collaboratorId.AddRange(planCampaignCreatedBy);
        //    collaboratorId.AddRange(planCampaignModifiedBy);
        //    collaboratorId.AddRange(planTacticCommentCreatedBy);
        //    return collaboratorId.Distinct().ToList<string>();
        //}

        #region Get Collaborator Details

        /// <summary>
        /// Get Collaborator Details for current plan.
        /// Modified By Maninder Singh Wadhva to Address PL#203
        /// </summary>
        /// <param name="currentPlanId">PlanId</param>
        /// <returns>Json Result.</returns>
        public JsonResult GetCollaboratorDetails(int currentPlanId)
        {
            JsonResult collaboratorsImage = Common.GetCollaboratorImage(currentPlanId);
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
            if (plan.CreatedDate != null)
            {
                lastUpdatedDate.Add(plan.CreatedDate);
            }

            if (plan.ModifiedDate != null)
            {
                lastUpdatedDate.Add(plan.ModifiedDate);
            }

            var planTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId)).Select(t => t);

            if (planTactic.Count() > 0)
            {

                var planTacticModifiedDate = planTactic.ToList().Select(t => t.ModifiedDate).Max();
                lastUpdatedDate.Add(planTacticModifiedDate);

                var planTacticCreatedDate = planTactic.ToList().Select(t => t.CreatedDate).Max();
                lastUpdatedDate.Add(planTacticCreatedDate);

                var planProgramModifiedDate = planTactic.ToList().Select(t => t.Plan_Campaign_Program.ModifiedDate).Max();
                lastUpdatedDate.Add(planProgramModifiedDate);

                var planProgramCreatedDate = planTactic.ToList().Select(t => t.Plan_Campaign_Program.CreatedDate).Max();
                lastUpdatedDate.Add(planProgramCreatedDate);

                var planCampaignModifiedDate = planTactic.ToList().Select(t => t.Plan_Campaign_Program.Plan_Campaign.ModifiedDate).Max();
                lastUpdatedDate.Add(planCampaignModifiedDate);

                var planCampaignCreatedDate = planTactic.ToList().Select(t => t.Plan_Campaign_Program.Plan_Campaign.CreatedDate).Max();
                lastUpdatedDate.Add(planCampaignCreatedDate);

                var planTacticComment = db.Plan_Campaign_Program_Tactic_Comment.Where(pc => pc.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId))
                                                                               .Select(pc => pc);
                if (planTacticComment.Count() > 0)
                {
                    var planTacticCommentCreatedDate = planTacticComment.ToList().Select(pc => pc.CreatedDate).Max();
                    lastUpdatedDate.Add(planTacticCommentCreatedDate);
                }
            }

            return Convert.ToDateTime(lastUpdatedDate.Max());
        }

        /// <summary>
        /// Function to get gantt data.
        /// Added By: Maninde Singh Wadhva.
        /// Modified By Maninder Singh Wadhva PL Ticket#47
        /// Date: 12/04/2013
        /// </summary>
        /// <param name="planId">Plan id for which gantt data to be fetched.</param>
        /// <returns>Json Result.</returns>
        public JsonResult GetGanttData(int planId, string isQuater)
        {
            Sessions.PlanId = planId;
            Plan plan = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId));
            bool isPublished = plan.Status.Equals(Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString());
            List<object> ganttTaskData = GetTaskDetailTactic(plan, isQuater);

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
        /// <param name="planId">Plan Id.</param>
        /// <param name="isQuarter">Flag to indicate whether to fetch data for current Quarter.</param>
        /// <returns>Returns list of task for GANNT CHART.</returns>
        public List<object> GetTaskDetailTactic(Plan plan, string isQuater)
        {
            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;

            Common.GetPlanGanttStartEndDate(plan.Year, isQuater, ref CalendarStartDate, ref CalendarEndDate);

            var taskDataCampaign = db.Plan_Campaign.Where(c => c.PlanId.Equals(plan.PlanId) && c.IsDeleted.Equals(false))
                                                   .ToList()
                                                   .Where(c => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                            CalendarEndDate,
                                                                                                            c.StartDate,
                                                                                                            c.EndDate).Equals(false))
                                                    .Select(c => new
                                                    {
                                                        id = string.Format("C{0}", c.PlanCampaignId),
                                                        text = c.Title,
                                                        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, c.StartDate),
                                                        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                                                  CalendarEndDate,
                                                                                                  c.StartDate,
                                                                                                  c.EndDate),
                                                        progress = GetCampaignProgress(plan, c),//progress = 0,
                                                        open = true,
                                                        color = Common.COLORC6EBF3_WITH_BORDER,
                                                        PlanCampaignId = c.PlanCampaignId,
                                                        IsHideDragHandleLeft = c.StartDate < CalendarStartDate,
                                                        IsHideDragHandleRight = c.EndDate > CalendarEndDate,
                                                        Status = c.Status       //// Added by Sohel on 19/05/2014 for PL #425 to Show status of tactics on ApplyToCalender screen
                                                    }).Select(c => c).OrderBy(c => c.text);

            var NewtaskDataCampaign = taskDataCampaign.Select(t => new
            {
                id = t.id,
                text = t.text,
                start_date = t.start_date,
                duration = t.duration,
                progress = t.progress,
                open = t.open,
                color = t.color + (t.progress == 1 ? " stripe" : (t.progress > 0 ? "stripe" : "")),
                PlanCampaignId = t.PlanCampaignId,
                IsHideDragHandleLeft = t.IsHideDragHandleLeft,
                IsHideDragHandleRight = t.IsHideDragHandleRight,
                Status = t.Status       //// Added by Sohel on 19/05/2014 for PL #425 to Show status of tactics on ApplyToCalender screen
            });

            var taskDataProgram = db.Plan_Campaign_Program.Where(p => p.Plan_Campaign.PlanId.Equals(plan.PlanId) &&
                                                                      p.IsDeleted.Equals(false))
                                                          .ToList()
                                                          .Where(p => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                  CalendarEndDate,
                                                                                                                  p.StartDate,
                                                                                                                  p.EndDate).Equals(false))
                                                          .Select(p => new
                                                          {
                                                              id = string.Format("C{0}_P{1}", p.PlanCampaignId, p.PlanProgramId),
                                                              text = p.Title,
                                                              start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, p.StartDate),
                                                              duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                                                        CalendarEndDate,
                                                                                                        p.StartDate,
                                                                                                        p.EndDate),
                                                              progress = GetProgramProgress(plan, p), //progress = 0,
                                                              open = true,
                                                              parent = string.Format("C{0}", p.PlanCampaignId),
                                                              color = Common.COLOR27A4E5,
                                                              PlanProgramId = p.PlanProgramId,
                                                              IsHideDragHandleLeft = p.StartDate < CalendarStartDate,
                                                              IsHideDragHandleRight = p.EndDate > CalendarEndDate,
                                                              Status = p.Status     //// Added by Sohel on 19/05/2014 for PL #425 to Show status of tactics on ApplyToCalender screen
                                                          }).Select(p => p).Distinct().OrderBy(p => p.text).ToList();

            var NewtaskDataProgram = taskDataProgram.Select(t => new
            {
                id = t.id,
                text = t.text,
                start_date = t.start_date,
                duration = t.duration,
                progress = t.progress,
                open = t.open,
                parent = t.parent,
                color = t.color + (t.progress == 1 ? " stripe stripe-no-border" : (t.progress > 0 ? "stripe" : "")),
                PlanProgramId = t.PlanProgramId,
                IsHideDragHandleLeft = t.IsHideDragHandleLeft,
                IsHideDragHandleRight = t.IsHideDragHandleRight,
                Status = t.Status       //// Added by Sohel on 19/05/2014 for PL #425 to Show status of tactics on ApplyToCalender screen
            });

            // Added By Bhavesh : 25-June-2014 : #538 Custom Restriction
            List<UserCustomRestrictionModel> lstUserCustomRestriction = Common.GetUserCustomRestriction();

            var taskDataTactic = db.Plan_Campaign_Program_Tactic.Where(p => p.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(plan.PlanId) &&
                                                                            p.IsDeleted.Equals(false))
                                                                .ToList()
                                                                .Where(p => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                        CalendarEndDate,
                                                                                                                        p.StartDate,
                                                                                                                        p.EndDate).Equals(false) && Common.GetRightsForTacticVisibility(lstUserCustomRestriction, p.VerticalId, p.GeographyId))
                                                                .Select(t => new
                                                                {
                                                                    id = string.Format("C{0}_P{1}_T{2}", t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId),
                                                                    text = t.Title,
                                                                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
                                                                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                                                              CalendarEndDate,
                                                                                                              t.StartDate,
                                                                                                              t.EndDate),
                                                                    progress = GetTacticProgress(plan, t),//progress = 0,
                                                                    open = true,
                                                                    parent = string.Format("C{0}_P{1}", t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId),
                                                                    color = Common.COLORC6EBF3_WITH_BORDER,
                                                                    plantacticid = t.PlanTacticId,
                                                                    IsHideDragHandleLeft = t.StartDate < CalendarStartDate,
                                                                    IsHideDragHandleRight = t.EndDate > CalendarEndDate,
                                                                    Status = t.Status       //// Added by Sohel on 19/05/2014 for PL #425 to Show status of tactics on ApplyToCalender screen
                                                                }).OrderBy(t => t.text).ToList();

            var NewTaskDataTactic = taskDataTactic.Select(t => new
            {
                id = t.id,
                text = t.text,
                start_date = t.start_date,
                duration = t.duration,
                progress = t.progress,
                open = t.open,
                parent = t.parent,
                color = t.color + (t.progress == 1 ? " stripe" : ""),
                plantacticid = t.plantacticid,
                IsHideDragHandleLeft = t.IsHideDragHandleLeft,
                IsHideDragHandleRight = t.IsHideDragHandleRight,
                Status = t.Status       //// Added by Sohel on 19/05/2014 for PL #425 to Show status of tactics on ApplyToCalender screen
            });

            //return taskDataCampaign.Concat<object>(taskDataTactic).Concat<object>(taskDataProgram).ToList<object>();
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
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
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
            // List of all improvement tactic.
            List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(improveTactic => improveTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(plan.PlanId) &&
                                                                                      improveTactic.IsDeleted.Equals(false) &&
                                                                                      (improveTactic.EffectiveDate > CalendarEndDate).Equals(false))
                                                                               .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

            if (improvementTactic.Count > 0)
            {
                DateTime minDate = improvementTactic.Select(t => t.EffectiveDate).Min(); // Minimun date of improvement tactic

                // Start date of program
                DateTime programStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaignProgram.StartDate));

                // List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = planCampaignProgram.Plan_Campaign_Program_Tactic.Where(p => p.IsDeleted.Equals(false) && (p.StartDate > minDate).Equals(true))
                                                                                              .Select(t => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate)) })
                                                                                              .ToList();

                if (lstAffectedTactic.Count > 0)
                {
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(t => t.startDate).Min(); // minimum start Date of tactics
                    if (tacticMinStartDate > minDate) // If any tactic affected by at least one improvement tactic.
                    {
                        double programDuration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, planCampaignProgram.StartDate, planCampaignProgram.EndDate);

                        // difference b/w program start date and tactic minimum date
                        double daysDifference = (tacticMinStartDate - programStartDate).TotalDays;

                        if (daysDifference > 0) // If no. of days are more then zero then it will return progress
                        {
                            return (daysDifference / programDuration);
                        }
                        else
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                    return 0;
            }
            else
            {
                return 0;
            }
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
                var lstTactic = db.Plan_Campaign_Program_Tactic.Where(p => p.Plan_Campaign_Program.PlanCampaignId.Equals(planCampaign.PlanCampaignId) &&
                                                                            p.IsDeleted.Equals(false))
                                                                .Select(p => p)
                                                                .ToList()
                                                                .Where(p => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                        CalendarEndDate,
                                                                                                                        p.StartDate,
                                                                                                                        p.EndDate).Equals(false));

                // List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = lstTactic.Where(p => (p.StartDate > minDate).Equals(true))
                                                 .Select(t => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate)) })
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
                            return (daysDifference / campaignDuration);
                        }
                        else
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                    return 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Function to update status of current plan.
        /// Added By: Maninder Singh Wadhva.
        /// Date: 12/04/2013
        /// </summary>
        /// <param name="planId">Plan Id whose status is to be updated.</param>
        /// <returns>Returns ApplyToCalendar action result.</returns>
        [HttpPost]
        [ActionName("ApplyToCalendar")]
        public RedirectToRouteResult ApplyToCalendarPost()
        {
            var plan = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId));
            plan.Status = Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()];
            plan.ModifiedBy = Sessions.User.UserId;
            plan.ModifiedDate = DateTime.Now;

            //db.Entry(plan).State = EntityState.Modified;
            int returnValue = db.SaveChanges();
            Common.InsertChangeLog(Sessions.PlanId, 0, Sessions.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.published);
            ViewBag.ActiveMenu = RevenuePlanner.Helpers.Enums.ActiveMenu.Plan;
            return RedirectToAction("Index", "Home", new { activeMenu = Enums.ActiveMenu.Plan, currentPlanId = Sessions.PlanId });
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 12/04/2013
        /// Function to update start and end date for tactic.
        /// </summary>
        /// <param name="planTacticId">Plan Tactic Id to be updated.</param>
        /// <param name="startDate">Start date field.</param>
        /// <param name="duration">Duration of task.</param>
        /// <returns>Returns json result that indicate whether date was updated successfully.</returns>
        public JsonResult UpdateStartEndDate(int id, string startDate, double duration, bool isPlanCampaign, bool isPlanProgram, bool isPlanTactic)
        {
            int returnValue = 0;
            if (isPlanCampaign)
            {
                //// Getting campaign to be updated.
                var planCampaign = db.Plan_Campaign.Single(pc => pc.PlanCampaignId.Equals(id));
                bool isApproved = planCampaign.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
                //Start Manoj Limbachiya  Ticket #363 Tactic Creation - Do not automatically submit a tactic
                //planCampaign.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                //End Manoj Limbachiya  Ticket #363 Tactic Creation - Do not automatically submit a tactic
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
                {
                    Common.InsertChangeLog(Sessions.PlanId, 0, planCampaign.PlanCampaignId, planCampaign.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                }

            }
            else if (isPlanProgram)
            {
                //// Getting program to be updated.
                var planProgram = db.Plan_Campaign_Program.Single(pc => pc.PlanProgramId.Equals(id));
                bool isApproved = planProgram.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
                //Start Manoj Limbachiya  Ticket #363 Tactic Creation - Do not automatically submit a tactic
                //planProgram.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                //End Manoj Limbachiya  Ticket #363 Tactic Creation - Do not automatically submit a tactic
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
                {
                    Common.InsertChangeLog(Sessions.PlanId, 0, planProgram.PlanProgramId, planProgram.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                }
            }
            else if (isPlanTactic)
            {
                //// Getting plan tactic to be updated.
                var planTactic = db.Plan_Campaign_Program_Tactic.Single(pt => pt.PlanTacticId.Equals(id));

                bool isApproved = planTactic.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
                //// Changing status of tactic to submitted.
                //Start Manoj Limbachiya  Ticket #363 Tactic Creation - Do not automatically submit a tactic
                //planTactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                bool isDirectorLevelUser = false;
                //if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
                //{
                //    if (planTactic.CreatedBy != Sessions.User.UserId) isDirectorLevelUser = true;
                //}
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
                Common.ChangeProgramStatus(planTactic.PlanProgramId);
                var PlanCampaignId = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.PlanProgramId == planTactic.PlanProgramId).Select(a => a.PlanCampaignId).Single();
                Common.ChangeCampaignStatus(PlanCampaignId);
                //// End - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425

                if (isApproved)
                {
                    Common.InsertChangeLog(Sessions.PlanId, 0, planTactic.PlanTacticId, planTactic.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                }
            }

            //// Checking whether operation was successfully or not.
            if (returnValue > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Assortment Mix

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Assortment.
        /// </summary>
        /// <returns>Returns View Of Assortment.</returns>
        public ActionResult Assortment(int campaignId = 0, int programId = 0, int tacticId = 0, string ismsg = "", string EditObject = "", bool isError = false)
        {
            if (campaignId != 0)
            {
                if (EditObject == "ImprovementTactic")
                {
                    Sessions.PlanId = db.Plan_Improvement_Campaign.Where(c => c.ImprovementPlanCampaignId == campaignId).Select(c => c.ImprovePlanId).FirstOrDefault();
                }
                else
                {
                Sessions.PlanId = db.Plan_Campaign.Where(c => c.PlanCampaignId == campaignId).Select(c => c.PlanId).FirstOrDefault();
                }
            }
            
            // Added by dharmraj to check user activity permission
            bool IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            ViewBag.IsPlanCreateAuthorized = IsPlanCreateAuthorized;
            bool IsBusinessUnitEditable = false;
            Plan plan = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId));
            if (plan != null)
            {

            // Start - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                IsBusinessUnitEditable = Common.IsBusinessUnitEditable(plan.Model.BusinessUnitId);
            if (!IsBusinessUnitEditable)
                return AuthorizeUserAttribute.RedirectToNoAccess();
            // End - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units

                // Added by Dharmraj Mangukiya for edit authentication of plan, PL ticket #519
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                
                bool IsPlanEditable = false;
                if (plan.CreatedBy.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditAllAuthorized)
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditSubordinatesAuthorized)
                {
                    //Get all subordinates of current user upto n level
                    var lstSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
                    if (lstSubOrdinates.Contains(plan.CreatedBy))
                    {
                        IsPlanEditable = true;
                    }
                }

                if (!IsPlanEditable)
                {
                    return AuthorizeUserAttribute.RedirectToNoAccess();
                }
            }

            ViewBag.PlanId = plan.PlanId;
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

            ViewBag.ActiveMenu = Enums.ActiveMenu.Plan;
            ViewBag.CampaignID = campaignId;
            ViewBag.ProgramID = programId;
            ViewBag.TacticID = tacticId;
            //ViewBag.SuccessMessageDuplicatePlan = TempData["SuccessMessageDuplicatePlan"];
            //ViewBag.ErrorMessageDuplicatePlan = TempData["ErrorMessageDuplicatePlan"];
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
            ViewBag.TacticId = tacticId;
            ViewBag.EditOjbect = EditObject;
            ViewBag.Msg = ismsg;
            ViewBag.isError = isError;

            int improvementProgramId = db.Plan_Improvement_Campaign_Program.Where(p => p.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId).Select(p => p.ImprovementPlanProgramId).SingleOrDefault();
            if (improvementProgramId != 0)
            {
                ViewBag.ImprovementPlanProgramId = improvementProgramId;
            }
            else
            {
                CreatePlanImprovementCampaignAndProgram();
                ViewBag.ImprovementPlanProgramId = db.Plan_Improvement_Campaign_Program.Where(p => p.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId).Select(p => p.ImprovementPlanProgramId).SingleOrDefault();
            }
            string mqlStage = Enums.Stage.MQL.ToString();
            string MQLStageLabel = Common.GetLabel(Common.StageModeMQL);
            if (!string.IsNullOrEmpty(MQLStageLabel))
            {
                mqlStage = MQLStageLabel;
            }
            ViewBag.MQLLabel = mqlStage;
            ViewBag.IsBusinessUnitEditable = IsBusinessUnitEditable;  // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            return View("Assortment");
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Camapign , Program & Tactic.
        /// </summary>
        /// <returns>Returns Json Result.</returns>
        public JsonResult GetCampaign()
        {
            List<UserCustomRestrictionModel> lstUserCustomRestriction = Common.GetUserCustomRestriction();
            List<Plan_Campaign> CampaignList = db.Plan_Campaign.Where(pc => pc.PlanId.Equals(Sessions.PlanId) && pc.IsDeleted.Equals(false)).Select(pc => pc).ToList();
            List<int> CampaignIds = CampaignList.Select(pc => pc.PlanCampaignId).ToList();
            List<Plan_Campaign_Program> ProgramList = db.Plan_Campaign_Program.Where(pcp => CampaignIds.Contains(pcp.PlanCampaignId) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp).ToList();
            List<int> ProgramIds = ProgramList.Select(pcp => pcp.PlanProgramId).ToList();
            List<Plan_Campaign_Program_Tactic> TacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => ProgramIds.Contains(pcpt.PlanProgramId) && pcpt.IsDeleted.Equals(false)).Select(pcpt => pcpt).ToList();
            List<int> TacticIds = TacticList.Select(pcpt => pcpt.PlanTacticId).ToList();
            List<Plan_Campaign_Program_Tactic_LineItem> LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcptl => TacticIds.Contains(pcptl.PlanTacticId) && pcptl.IsDeleted.Equals(false)).Select(pcptl => pcptl).ToList();
            List<int> LineItemIds = LineItemList.Select(pcptl => pcptl.PlanLineItemId).ToList();
           // List<Plan_Tactic_Values> ListTacticMQLValue = Common.GetMQLValueTacticList(TacticList, true);


            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && t.IsDeleted == false).Select(t => t).ToList();
            //Added By Bhavesh For Performance Issue #955
            List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
            List<StageList> stageListType = Common.GetStageList();
            int? ModelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(m => m.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(m => m.ModelId == ModelId).Select(m => m).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(m => m.ModelId).ToList());

            var improvementTacticTypeIds = improvementActivities.Select(imptype => imptype.ImprovementTacticTypeId).ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId).Select(stage => stage).ToList();

            List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelationForSinglePlan(TacticList, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, false);

            List<Plan_Tactic_Values> ListTacticMQLValue = (from tactic in TacticDataWithoutImprovement
                                                      select new Plan_Tactic_Values
                                                      {
                                                          PlanTacticId = tactic.TacticObj.PlanTacticId,
                                                          MQL = Math.Round(tactic.MQLValue, 0, MidpointRounding.AwayFromZero),
                                                          Revenue = tactic.RevenueValue
                                                      }).ToList();

            var campaignobj = CampaignList.Select(p => new
            {
                id = p.PlanCampaignId,
                title = p.Title,
                description = p.Description,
                cost = LineItemList.Where(l => (TacticList.Where(pcpt => (ProgramList.Where(pcp => pcp.PlanCampaignId == p.PlanCampaignId).Select(pcp => pcp.PlanProgramId).ToList()).Contains(pcpt.PlanProgramId)).Select(pcpt => pcpt.PlanTacticId)).Contains(l.PlanTacticId)).Sum(l => l.Cost),
                //inqs = p.INQs.HasValue ? p.INQs : 0,
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
                    tactics = (TacticList.Where(pcpt => pcpt.PlanProgramId.Equals(pcpj.PlanProgramId) && Common.GetRightsForTacticVisibility(lstUserCustomRestriction, pcpt.VerticalId, pcpt.GeographyId)).Select(pcpt => pcpt).ToList()).Select(pcptj => new
                    {
                        id = pcptj.PlanTacticId,
                        title = pcptj.Title,
                        description = pcptj.Description,
                        cost = LineItemList.Where(l => l.PlanTacticId == pcptj.PlanTacticId).Sum(l => l.Cost),
                        mqls = ListTacticMQLValue.Where(lt => lt.PlanTacticId == pcptj.PlanTacticId).Sum(lt => lt.MQL),
                        isOwner = Sessions.User.UserId == pcptj.CreatedBy ? (Common.GetRightsForTactic(lstUserCustomRestriction, pcptj.VerticalId, pcptj.GeographyId) ? 0 : 1) : 1,
                        lineitems = (LineItemList.Where(pcptl => pcptl.PlanTacticId.Equals(pcptj.PlanTacticId))).Select(pcptlj => new
                        {
                            id = pcptlj.PlanLineItemId,
                            type = pcptlj.LineItemTypeId,
                            title = pcptlj.Title,
                            cost = pcptlj.Cost
                        }).Select(pcptlj => pcptlj).Distinct().OrderByDescending(pcptlj => pcptlj.type)
                    }).Select(pcptj => pcptj).Distinct().OrderBy(pcptj => pcptj.id)
                }).Select(pcpj => pcpj).Distinct().OrderBy(pcpj => pcpj.id)
            }).Select(p => p).Distinct().OrderBy(p => p.id);

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
                    isOwner = p.isOwner == 0 ? (p.tactics.Any(t => t.isOwner == 1) ? 1 : 0) : 1,
                    tactics = p.tactics
                })
            });

            var lstCampaign = lstCampaignTmp.Select(c => new
            {
                id = c.id,
                title = c.title,
                description = c.description,
                cost = c.cost,
                mqls = c.mqls,
                isOwner = c.isOwner == 0 ? (c.programs.Any(p => p.isOwner == 1) ? 1 : 0) : 1,
                programs = c.programs
            });

            return Json(lstCampaign, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Function for calculate MQL based on tactic stage level
        /// added by dharmraj for ticket #440
        /// </summary>
        /// <param name="objTactic"></param>
        /// <returns></returns>
        public string GetTacticMQL(Plan_Campaign_Program_Tactic objTactic)
        {
            string TitleMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            int MQLStageLevel = Convert.ToInt32(db.Stages.FirstOrDefault(s => s.ClientId == Sessions.User.ClientId && s.Code == TitleMQL).Level);
            if (objTactic.Stage.Level > MQLStageLevel)
            {
                return "N/A";
            }
            else
            {
                // Added by Bhavesh Dobariya #183
                return Convert.ToString(Common.CalculateMQLTactic(Convert.ToDouble(objTactic.ProjectedStageValue), objTactic.StartDate, objTactic.PlanTacticId, objTactic.StageId, objTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId));
            }
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Create Campaign.
        /// </summary>
        /// <returns>Returns Partial View Of Campaign.</returns>
        public PartialViewResult CreateCampaign()
        {
            // Dropdown for Verticals
            /* clientID add by Nirav shah on 15 jan 2014 for get verical and audience by client wise*/
            /* changed by Nirav on 11 APR for PL 322*/
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId);
            //ViewBag.Geography = db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId);

            // added by Dharmraj for ticket #435 MAP/CRM Integration - Tactic Creation
            var objPlan = db.Plans.SingleOrDefault(varP => varP.PlanId == Sessions.PlanId);
            ////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);
            ViewBag.IsDeployedToIntegration = false;


            ViewBag.IsCreated = true;
            ViewBag.RedirectType = false;
            ViewBag.IsOwner = true;
            // Start - Added by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
            Plan_CampaignModel pc = new Plan_CampaignModel();
            pc.StartDate = GetCurrentDateBasedOnPlan();
            pc.EndDate = GetCurrentDateBasedOnPlan(true);
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;
            pc.CampaignBudget = 0; // Added By Dharmraj #567 : Budget allocation for campaign
            pc.AllocatedBy = objPlan.AllocatedBy;

            pc.CustomFieldHtmlContent = HtmlHelpers.GenerateCustomFields(0, Enums.EntityType.Campaign.ToString());//Added by Mitesh Vaishnav for PL ticket #718

            var lstAllCampaign = db.Plan_Campaign.Where(c => c.PlanId == Sessions.PlanId && c.IsDeleted == false).ToList();
            double allCampaignBudget = lstAllCampaign.Sum(c => c.CampaignBudget);
            double planBudget = objPlan.Budget;
            double planRemainingBudget = planBudget - allCampaignBudget;
            ViewBag.planRemainingBudget = planRemainingBudget;

            // End - Added by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
            return PartialView("CampaignAssortment", pc);   // Modified by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Create Campaign.
        /// </summary>
        /// <param name="id">Campaign Id.</param>
        /// <param name="RedirectType">Redirect Type</param>
        /// <returns>Returns Partial View Of Campaign.</returns>
        public PartialViewResult EditCampaign(int id = 0, string RedirectType = "", string CalledFromBudget = "")
        {
            ViewBag.CalledFromBudget = CalledFromBudget;
            // Dropdown for Verticals
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false);

            /* clientID add by Nirav shah on 15 jan 2014 for get verical and audience by client wise*/
            /* changed by Nirav on 11 APR for PL 322*/
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId);
            //ViewBag.Geography = db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId);
            ViewBag.IsCreated = false;
            if (RedirectType == "Assortment")
            {
                ViewBag.RedirectType = false;
            }
            else
            {
                ViewBag.RedirectType = true;
            }
            Plan_Campaign pc = db.Plan_Campaign.Where(pcobj => pcobj.PlanCampaignId.Equals(id) && pcobj.IsDeleted.Equals(false)).SingleOrDefault();
            if (pc == null)
            {
                return null;
            }

            ////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(pc.Plan.Model);
            Plan_CampaignModel pcm = new Plan_CampaignModel();
            pcm.PlanCampaignId = pc.PlanCampaignId;
            pcm.Title = HttpUtility.HtmlDecode(pc.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            pcm.Description = HttpUtility.HtmlDecode(pc.Description);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            pcm.IsDeployedToIntegration = pc.IsDeployedToIntegration;
            ViewBag.IsDeployedToIntegration = pcm.IsDeployedToIntegration;
            /* changed by Nirav on 11 APR for PL 322*/
            //pcm.VerticalId = pc.VerticalId;
            //pcm.AudienceId = pc.AudienceId;
            //pcm.GeographyId = pc.GeographyId;
            pcm.StartDate = pc.StartDate;
            pcm.EndDate = pc.EndDate;
            //if (RedirectType != "Assortment") // Commented by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
            //{
            var psd = (from p in db.Plan_Campaign_Program where p.PlanCampaignId == id && p.IsDeleted.Equals(false) select p);
            if (psd.Count() > 0)
            {
                pcm.PStartDate = (from opsd in psd select opsd.StartDate).Min();
            }

            var ped = (from p in db.Plan_Campaign_Program where p.PlanCampaignId == id && p.IsDeleted.Equals(false) select p);
            if (ped.Count() > 0)
            {
                pcm.PEndDate = (from oped in ped select oped.EndDate).Max();
            }
            var tsd = (from t in db.Plan_Campaign_Program_Tactic where t.Plan_Campaign_Program.PlanCampaignId == id && t.IsDeleted.Equals(false) select t);
            if (tsd.Count() > 0)
            {
                pcm.TStartDate = (from otsd in tsd select otsd.StartDate).Min();
            }
            var ted = (from t in db.Plan_Campaign_Program_Tactic where t.Plan_Campaign_Program.PlanCampaignId == id && t.IsDeleted.Equals(false) select t);
            if (ted.Count() > 0)
            {
                pcm.TEndDate = (from oted in ted select oted.EndDate).Max();
            }
            //}
            //pcm.INQs = pc.INQs;
            List<Plan_Tactic_Values> PlanTacticValuesList = Common.GetMQLValueTacticList(db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.PlanCampaignId == pc.PlanCampaignId && t.IsDeleted == false).ToList());
            pcm.MQLs = PlanTacticValuesList.Sum(tm => tm.MQL);
            pcm.Cost = Common.CalculateCampaignCost(pc.PlanCampaignId); //pc.Cost; // Modified for PL#440 by Dharmraj
            // Start Added By Dharmraj #567 : Budget allocation for campaign
            pcm.CampaignBudget = pc.CampaignBudget;
            pcm.AllocatedBy = pc.Plan.AllocatedBy;

            var lstAllCampaign = db.Plan_Campaign.Where(c => c.PlanId == Sessions.PlanId && c.IsDeleted == false).ToList();
            double allCampaignBudget = lstAllCampaign.Sum(c => c.CampaignBudget);
            double planBudget = pc.Plan.Budget;
            double planRemainingBudget = planBudget - allCampaignBudget;
            ViewBag.planRemainingBudget = planRemainingBudget;

            pcm.Revenue = Math.Round(PlanTacticValuesList.Sum(tm => tm.Revenue)); //  Update by Bhavesh to Display Revenue
            // End Added By Dharmraj #567 : Budget allocation for campaign

            pcm.CustomFieldHtmlContent = HtmlHelpers.GenerateCustomFields(id, Enums.EntityType.Campaign.ToString());//Added by Mitesh Vaishnav for PL ticket #718
            if (Sessions.User.UserId == pc.CreatedBy)
            {
                ViewBag.IsOwner = true;

                // Added by Dharmraj Mangukiya to hide/show delete program as per custom restrictions PL ticket #577
                var AllTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.PlanCampaignId == pc.PlanCampaignId && t.IsDeleted == false).ToList();
                bool IsCampaignDeleteble = true;
                if (AllTactic.Count > 0)
                {
                    var OthersTactic = AllTactic.Where(t => t.CreatedBy != Sessions.User.UserId).ToList();
                    if (OthersTactic.Count > 0)
                    {
                        IsCampaignDeleteble = false;
                    }
                    else
                    {
                        var lstUserCustomRestriction = Common.GetUserCustomRestriction();
                        int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                        var lstAllowedVertical = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId.ToLower()).ToList();
                        var lstAllowedGeography = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId.ToLower()).ToList();
                        var lstAllowedBusinessUnit = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).Select(r => r.CustomFieldId.ToLower()).ToList();
                        if (AllTactic.Where(t => t.CreatedBy == Sessions.User.UserId && (!lstAllowedGeography.Contains(t.GeographyId.ToString().ToLower()) || !lstAllowedVertical.Contains(t.VerticalId.ToString().ToLower()) || !lstAllowedBusinessUnit.Contains(t.BusinessUnitId.ToString().ToLower()))).ToList().Count > 0)
                        {
                            IsCampaignDeleteble = false;
                        }
                    }
                }

                ViewBag.IsCampaignDeleteble = IsCampaignDeleteble;
            }
            else
            {
                ViewBag.IsOwner = false;
                ViewBag.IsCampaignDeleteble = false;
            }
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;

            //Clonehelper.PlanClone(Sessions.PlanId);

            return PartialView("CampaignAssortment", pcm);
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
                List<string> lstMonthly = new List<string>() { "Y1", "Y2", "Y3", "Y4", "Y5", "Y6", "Y7", "Y8", "Y9", "Y10", "Y11", "Y12" };
                var objPlanCampaign = db.Plan_Campaign.SingleOrDefault(c => c.PlanCampaignId == id && c.IsDeleted == false);    // Modified by Sohel Pathan on 04/09/2014 for PL ticket #758
                var objPlan = db.Plans.SingleOrDefault(p => p.PlanId == Sessions.PlanId && p.IsDeleted == false);   // Modified by Sohel Pathan on 04/09/2014 for PL ticket #758

                var lstAllCampaign = db.Plan_Campaign.Where(c => c.PlanId == Sessions.PlanId && c.IsDeleted == false).ToList();
                var planCampaignId = lstAllCampaign.Select(c => c.PlanCampaignId);
                var lstAllProgram = db.Plan_Campaign_Program.Where(p => p.PlanCampaignId == id && p.IsDeleted == false).ToList();
                var ProgramId = lstAllProgram.Select(p => p.PlanProgramId);
                var lstCampaignBudget = db.Plan_Campaign_Budget.Where(c => planCampaignId.Contains(c.PlanCampaignId)).ToList()
                                                               .Select(c => new
                                                               {
                                                                   c.PlanCampaignBudgetId,
                                                                   c.PlanCampaignId,
                                                                   c.Period,
                                                                   c.Value
                                                               }).ToList();
                var lstProgramBudget = db.Plan_Campaign_Program_Budget.Where(c => ProgramId.Contains(c.PlanProgramId)).ToList()
                                                               .Select(c => new
                                                               {
                                                                   c.PlanProgramBudgetId,
                                                                   c.PlanProgramId,
                                                                   c.Period,
                                                                   c.Value
                                                               }).ToList();

                var lstPlanBudget = db.Plan_Budget.Where(p => p.PlanId == Sessions.PlanId);

                double allCampaignBudget = lstAllCampaign.Sum(c => c.CampaignBudget);
                double planBudget = objPlan.Budget;
                double planRemainingBudget = planBudget - allCampaignBudget;

                // Start - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                if (objPlan.AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString())
                {
                    List<PlanBudgetAllocationValue> lstPlanBudgetAllocationValue = new List<PlanBudgetAllocationValue>();
                    var quarterPeriods = new string[] { "Y1", "Y2", "Y3" };
                    for (int i = 0; i < 12; i++)
                    {
                        if ((i + 1) % 3 == 0)
                        {
                            PlanBudgetAllocationValue objPlanBudgetAllocationValue = new PlanBudgetAllocationValue();
                            objPlanBudgetAllocationValue.periodTitle = "Y" + (i - 1).ToString();
                            objPlanBudgetAllocationValue.budgetValue = lstCampaignBudget.Where(a => quarterPeriods.Contains(a.Period) && a.PlanCampaignId == id).FirstOrDefault() == null ? "" : lstCampaignBudget.Where(a => quarterPeriods.Contains(a.Period) && a.PlanCampaignId == id).Select(a => a.Value).Sum().ToString();
                            objPlanBudgetAllocationValue.remainingMonthlyBudget = (lstPlanBudget.Where(a => quarterPeriods.Contains(a.Period)).FirstOrDefault() == null ? 0 : lstPlanBudget.Where(a => quarterPeriods.Contains(a.Period)).Select(a => a.Value).Sum()) - (lstCampaignBudget.Where(a => quarterPeriods.Contains(a.Period)).Sum(c => c.Value));
                            objPlanBudgetAllocationValue.programMonthlyBudget = lstProgramBudget.Where(a => quarterPeriods.Contains(a.Period)).Select(a => a.Value).Sum();

                            /// Add into return list
                            lstPlanBudgetAllocationValue.Add(objPlanBudgetAllocationValue);

                            quarterPeriods = new string[] { "Y" + (i + 2), "Y" + (i + 3), "Y" + (i + 4) };
                        }
                    }

                    var objBudgetAllocationData = new { budgetData = lstPlanBudgetAllocationValue, planRemainingBudget = planRemainingBudget };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                // End - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                else
                {
                    var budgetData = lstMonthly.Select(m => new
                    {
                        periodTitle = m,
                        budgetValue = lstCampaignBudget.SingleOrDefault(c => c.Period == m && c.PlanCampaignId == id) == null ? "" : lstCampaignBudget.SingleOrDefault(c => c.Period == m && c.PlanCampaignId == id).Value.ToString(),
                        remainingMonthlyBudget = (lstPlanBudget.SingleOrDefault(p => p.Period == m) == null ? 0 : lstPlanBudget.SingleOrDefault(p => p.Period == m).Value) - (lstCampaignBudget.Where(c => c.Period == m).Sum(c => c.Value)),
                        programMonthlyBudget = lstProgramBudget.Where(c => c.Period == m).Sum(c => c.Value)
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
        /// Added By: Bhavesh Dobariya.
        /// Action to Get TacticType.
        /// </summary>
        /// <returns>Returns JSon Result Of TacticType.</returns>
        public JsonResult GetTacticType()
        {
            var tacticType = from t in db.TacticTypes
                             join p in db.Plans on t.ModelId equals p.ModelId
                             where p.PlanId == Sessions.PlanId && (t.IsDeleted == null || t.IsDeleted == false) && t.IsDeployedToModel == true
                             orderby t.Title
                             select new { t.Title, t.TacticTypeId };
            return Json(tacticType, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Campaign.
        /// </summary>
        /// <param name="form">Form object of Plan_CampaignModel.</param>
        /// <param name="programs">Program list string array.</param>
        /// <param name="RedirectType">Redirect Type.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveCampaign(Plan_CampaignModel form, string programs, bool RedirectType, string closedTask, string BudgetInputValues,string customFieldInputs, string UserId = "", string CalledFromBudget = "")
        {
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
                //Deserialize customFieldInputs json string to  KeyValuePair List
                var customFields = JsonConvert.DeserializeObject<List<KeyValuePair<string,string>>>(customFieldInputs);
                string[] arrBudgetInputValues = BudgetInputValues.Split(',');

                if (form.PlanCampaignId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pc = db.Plan_Campaign.Where(plancampaign => (plancampaign.PlanId.Equals(Sessions.PlanId) && plancampaign.IsDeleted.Equals(false) && plancampaign.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()))).FirstOrDefault();

                            if (pc != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                Plan_Campaign pcobj = new Plan_Campaign();
                                //Change Plan Id
                                pcobj.PlanId = Sessions.PlanId;
                                pcobj.Title = form.Title;
                                pcobj.Description = form.Description;
                                pcobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                /* changed by Nirav on 11 APR for PL 322*/
                                //pcobj.VerticalId = form.VerticalId;
                                //pcobj.AudienceId = form.AudienceId;
                                //pcobj.GeographyId = form.GeographyId;
                                //pcobj.INQs = 0;
                                //pcobj.Cost = 0;
                                pcobj.StartDate = form.StartDate;   // Modified by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                pcobj.EndDate = form.EndDate;   // Modified by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                pcobj.CreatedBy = Sessions.User.UserId;
                                pcobj.CreatedDate = DateTime.Now;
                                pcobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString(); // status field in Plan_Campaign table 
                                pcobj.CampaignBudget = form.CampaignBudget;
                                db.Entry(pcobj).State = EntityState.Added;
                                int result = db.SaveChanges();
                                int campaignid = pcobj.PlanCampaignId;
                                result = Common.InsertChangeLog(Sessions.PlanId, null, campaignid, pcobj.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                if (result >= 1)
                                {

                                    if (programs != string.Empty)
                                    {
                                        string[] program = programs.Split(',');

                                        foreach (string prg in program)
                                        {
                                            Plan_Campaign_Program pcpobj = new Plan_Campaign_Program();
                                            //Change Plan Id
                                            pcpobj.PlanCampaignId = campaignid;
                                            pcpobj.Title = prg;
                                            /* changed by Nirav on 11 APR for PL 322*/
                                            //pcpobj.VerticalId = form.VerticalId;
                                            //pcpobj.AudienceId = form.AudienceId;
                                            //pcpobj.GeographyId = form.GeographyId;
                                            //pcpobj.INQs = 0;
                                            //pcpobj.Cost = 0;
                                            pcpobj.StartDate = form.StartDate;  // Modified by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                            pcpobj.EndDate = form.EndDate;  // Modified by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                            pcpobj.CreatedBy = Sessions.User.UserId;
                                            pcpobj.CreatedDate = DateTime.Now;
                                            pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString(); // status field in Plan_Campaign_Program table 
                                            pcpobj.ProgramBudget = 0;
                                            db.Entry(pcpobj).State = EntityState.Added;
                                            result = db.SaveChanges();
                                            int programId = pcpobj.PlanProgramId;
                                            result = Common.InsertChangeLog(Sessions.PlanId, null, programId, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                        }
                                    }

                                    // Start Added By Dharmraj #567 : Budget allocation for campaign
                                    var PrevAllocationList = db.Plan_Campaign_Budget.Where(c => c.PlanCampaignId == campaignid).Select(c => c).ToList();
                                    PrevAllocationList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                    if (arrBudgetInputValues.Length == 12)
                                    {
                                        for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                        {
                                            if (arrBudgetInputValues[i] != "")
                                            {
                                                Plan_Campaign_Budget objPlanCampaignBudget = new Plan_Campaign_Budget();
                                                objPlanCampaignBudget.PlanCampaignId = campaignid;
                                                objPlanCampaignBudget.Period = "Y" + (i + 1);
                                                objPlanCampaignBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                                objPlanCampaignBudget.CreatedBy = Sessions.User.UserId;
                                                objPlanCampaignBudget.CreatedDate = DateTime.Now;
                                                db.Entry(objPlanCampaignBudget).State = EntityState.Added;
                                            }
                                        }
                                    }
                                    else if (arrBudgetInputValues.Length == 4)
                                    {
                                        int QuarterCnt = 1;
                                        for (int i = 0; i < 4; i++)
                                        {
                                            if (arrBudgetInputValues[i] != "")
                                            {
                                                Plan_Campaign_Budget objPlanCampaignBudget = new Plan_Campaign_Budget();
                                                objPlanCampaignBudget.PlanCampaignId = campaignid;
                                                objPlanCampaignBudget.Period = "Y" + QuarterCnt;
                                                objPlanCampaignBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                                objPlanCampaignBudget.CreatedBy = Sessions.User.UserId;
                                                objPlanCampaignBudget.CreatedDate = DateTime.Now;
                                                db.Entry(objPlanCampaignBudget).State = EntityState.Added;
                                            }
                                            QuarterCnt = QuarterCnt + 3;
                                        }
                                    }
                                    // End Added By Dharmraj #567 : Budget allocation for campaign
                                    ////Start Added by Mitesh Vaishnav for PL ticket #718 Custom fields for Campaigns
                                    ////save custom fields value for particular campaign
                                    if (customFields.Count != 0)
                                    {
                                        foreach (var item in customFields)
                                        {
                                            CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                            objcustomFieldEntity.EntityId = campaignid;
                                            objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                            objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                            objcustomFieldEntity.CreatedDate = DateTime.Now;
                                            objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                            db.Entry(objcustomFieldEntity).State = EntityState.Added;

                                        }
                                    }
                                    ////End Added by Mitesh Vaishnav for PL ticket #718 Custom fields for Campaigns
                                    db.SaveChanges();

                                }
                                scope.Complete();
                                if (!string.IsNullOrEmpty(CalledFromBudget))
                                {
                                    return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget }) });
                                }
                                else
                                {
                                    return Json(new { redirect = Url.Action("Assortment") });
                                }
                            }
                        }
                    }
                }
                else
                {

                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pc = db.Plan_Campaign.Where(plancampaign => (plancampaign.PlanId.Equals(Sessions.PlanId) && plancampaign.IsDeleted.Equals(false) && plancampaign.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !plancampaign.PlanCampaignId.Equals(form.PlanCampaignId))).FirstOrDefault();

                            if (pc != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                Plan_Campaign pcobj = db.Plan_Campaign.Where(pcobjw => pcobjw.PlanCampaignId.Equals(form.PlanCampaignId) && pcobjw.IsDeleted.Equals(false)).SingleOrDefault();
                                //Change Plan Id
                                pcobj.Title = form.Title;
                                pcobj.Description = form.Description;
                                pcobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                /* changed by Nirav on 11 APR for PL 322*/
                                //pcobj.VerticalId = form.VerticalId;
                                //pcobj.AudienceId = form.AudienceId;
                                //pcobj.GeographyId = form.GeographyId;
                                //if (RedirectType) // Commented by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                //{
                                pcobj.StartDate = form.StartDate;
                                pcobj.EndDate = form.EndDate;
                                //}
                                //pcobj.INQs = (form.INQs == null ? 0 : form.INQs);
                                //pcobj.Cost = (form.Cost == null ? 0 : form.Cost);
                                pcobj.ModifiedBy = Sessions.User.UserId;
                                pcobj.ModifiedDate = DateTime.Now;
                                pcobj.CampaignBudget = form.CampaignBudget;
                                db.Entry(pcobj).State = EntityState.Modified;

                                Common.InsertChangeLog(Sessions.PlanId, null, pcobj.PlanCampaignId, pcobj.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);

                                if (arrBudgetInputValues.Length > 0)    // Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                                {
                                    // Start Added By Dharmraj #567 : Budget allocation for campaign
                                    var PrevAllocationList = db.Plan_Campaign_Budget.Where(c => c.PlanCampaignId == form.PlanCampaignId).Select(c => c).ToList();
                                    //PrevAllocationList.ForEach(a => db.Entry(a).State = EntityState.Deleted); // Commented by Sohel Pathan on 27/08/2014 for PL ticket #758

                                    if (arrBudgetInputValues.Length == 12)
                                    {
                                        for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                        {
                                            // Start - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                                            bool isExists = false;
                                            if (PrevAllocationList != null)
                                            {
                                                if (PrevAllocationList.Count > 0)
                                                {
                                                    var updatePlanCampaignBudget = PrevAllocationList.Where(pb => pb.Period == ("Y" + (i + 1))).FirstOrDefault();
                                                    if (updatePlanCampaignBudget != null)
                                                    {
                                                        if (arrBudgetInputValues[i] != "")
                                                        {
                                                            var newValue = Convert.ToDouble(arrBudgetInputValues[i]);
                                                            if (updatePlanCampaignBudget.Value != newValue)
                                                            {
                                                                updatePlanCampaignBudget.Value = newValue;
                                                                db.Entry(updatePlanCampaignBudget).State = EntityState.Modified;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            db.Entry(updatePlanCampaignBudget).State = EntityState.Deleted; //// Added by Sohel Pathan on 01/09/2014 for PL ticket #758
                                                        }
                                                        isExists = true;
                                                    }
                                                }
                                            }
                                            if (!isExists && arrBudgetInputValues[i] != "")
                                            {
                                                // End - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                                                Plan_Campaign_Budget objPlanCampaignBudget = new Plan_Campaign_Budget();
                                                objPlanCampaignBudget.PlanCampaignId = form.PlanCampaignId;
                                                objPlanCampaignBudget.Period = "Y" + (i + 1);
                                                objPlanCampaignBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                                objPlanCampaignBudget.CreatedBy = Sessions.User.UserId;
                                                objPlanCampaignBudget.CreatedDate = DateTime.Now;
                                                db.Entry(objPlanCampaignBudget).State = EntityState.Added;
                                            }
                                        }
                                    }
                                    else if (arrBudgetInputValues.Length == 4)
                                    {
                                        int QuarterCnt = 1;
                                        for (int i = 0; i < 4; i++)
                                        {
                                            // Start - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                                            bool isExists = false;
                                            if (PrevAllocationList != null)
                                            {
                                                if (PrevAllocationList.Count > 0)
                                                {
                                                    var thisQuartersMonthList = PrevAllocationList.Where(pb => pb.Period == ("Y" + (QuarterCnt)) || pb.Period == ("Y" + (QuarterCnt + 1)) || pb.Period == ("Y" + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                                    var thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                                    if (thisQuarterFirstMonthBudget != null)
                                                    {
                                                        if (arrBudgetInputValues[i] != "")
                                                        {
                                                            var thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                                            var thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                                            var newValue = Convert.ToDouble(arrBudgetInputValues[i]);

                                                            if (thisQuarterTotalBudget != newValue)
                                                            {
                                                                var BudgetDiff = newValue - thisQuarterTotalBudget;
                                                                if (BudgetDiff > 0)
                                                                {
                                                                    thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                                                    db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                                }
                                                                else
                                                                {
                                                                    int j = 1;
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
                                                                            thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == ("Y" + (QuarterCnt + j))).FirstOrDefault();
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
                                            }
                                            if (!isExists && arrBudgetInputValues[i] != "")
                                            {
                                                // End - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                                                Plan_Campaign_Budget objPlanCampaignBudget = new Plan_Campaign_Budget();
                                                objPlanCampaignBudget.PlanCampaignId = form.PlanCampaignId;
                                                objPlanCampaignBudget.Period = "Y" + QuarterCnt;
                                                objPlanCampaignBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                                objPlanCampaignBudget.CreatedBy = Sessions.User.UserId;
                                                objPlanCampaignBudget.CreatedDate = DateTime.Now;
                                                db.Entry(objPlanCampaignBudget).State = EntityState.Added;
                                            }
                                            QuarterCnt = QuarterCnt + 3;
                                        }
                                    }
                                }
                                // End Added By Dharmraj #567 : Budget allocation for campaign
                                ////Start Added by Mitesh Vaishnav for PL ticket #718 Custom fields for Campaigns
                                //// delete previous custom field values and save modified custom fields value for particular campaign
                                string entityTypeCampaign=Enums.EntityType.Campaign.ToString();
                                var prevCustomFieldList = db.CustomField_Entity.Where(c => c.EntityId == pcobj.PlanCampaignId && c.CustomField.EntityType == entityTypeCampaign).ToList();
                                prevCustomFieldList.ForEach(c => db.Entry(c).State = EntityState.Deleted);

                                if (customFields.Count != 0)
                                {
                                    foreach (var item in customFields)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = pcobj.PlanCampaignId;
                                        objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;

                                    }
                                }
                                ////End Added by Mitesh Vaishnav for PL ticket #718 Custom fields for Campaigns
                                db.SaveChanges();


                                scope.Complete();
                                if (!string.IsNullOrEmpty(CalledFromBudget))
                                {
                                    TempData["SuccessMessage"] = "Campaign Saved Successfully";
                                    return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget }) });
                                }
                                else
                                {
                                    if (RedirectType)
                                    {
                                        TempData["ClosedTask"] = closedTask;
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
        /// Action to Delete Campaign.
        /// </summary>
        /// <param name="id">Campaign Id.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid
            changed by : Nirav Shah on 13 feb 2014
            Changed : add new Parameter  RedirectType
         */
        public ActionResult DeleteCampaign(int id = 0, bool RedirectType = false, string closedTask = null, string UserId = "", string CalledFromBudget = "")
        {
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
                            Plan_Campaign pc = db.Plan_Campaign.Where(p => p.PlanCampaignId == id).SingleOrDefault();
                            Title = pc.Title;
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pc.PlanCampaignId, pc.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                            if (returnValue >= 1)
                            {
                                scope.Complete();
                                /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid
                                  changed by : Nirav Shah on 13 feb 2014
                                  Changed : set message and based on request redirect page.
                                */


                                //return Json(new { redirect = Url.Action("Assortment") });
                                if (!string.IsNullOrEmpty(CalledFromBudget))
                                {
                                    TempData["SuccessMessage"] = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                                    return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget }) });
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
        /// Action to Create Program.
        /// </summary>
        /// <returns>Returns Partial View Of Program.</returns>
        public PartialViewResult CreateProgram(int id = 0)
        {
            // Dropdown for Verticals
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false);
            /* clientID add by Nirav shah on 15 jan 2014 for get verical and audience by client wise*/
            /* changed by Nirav on 11 APR for PL 322*/
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId);
            //ViewBag.Geography = db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId);
            ViewBag.IsCreated = true;


            var objPlan = db.Plans.SingleOrDefault(varP => varP.PlanId == Sessions.PlanId);
            ////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);

            Plan_Campaign pcp = db.Plan_Campaign.Where(pcpobj => pcpobj.PlanCampaignId.Equals(id) && pcpobj.IsDeleted.Equals(false)).SingleOrDefault();
            if (pcp == null)
            {
                return null;
            }//uday PL Ticket #550 3-7-2014

            ViewBag.Campaign = pcp.Title;//uday PL Ticket #550 3-7-2014

            Plan_Campaign_ProgramModel pcpm = new Plan_Campaign_ProgramModel();
            pcpm.PlanCampaignId = id;
            pcpm.IsDeployedToIntegration = false;
            //Plan_Campaign pc = db.Plan_Campaign.Where(pco => pco.PlanCampaignId == id).SingleOrDefault();
            /* changed by Nirav on 11 APR for PL 322*/
            //pcpm.GeographyId = pc.GeographyId;
            //pcpm.VerticalId = pc.VerticalId;
            //pcpm.AudienceId = pc.AudienceId;
            ViewBag.IsOwner = true;      /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/
            ViewBag.RedirectType = false;
            // Start - Added by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
            pcpm.StartDate = GetCurrentDateBasedOnPlan();
            pcpm.EndDate = GetCurrentDateBasedOnPlan(true);
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;
            pcpm.CStartDate = pcp.StartDate;
            pcpm.CEndDate = pcp.EndDate;
            // End - Added by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen

            // Start - Added by :- Sohel Pathan on 18/17/2014 for PL ticket #594.
            bool canCreateTactic = false;
            var IsAudienceExists = db.Audiences.Where(a => a.ClientId == Sessions.User.ClientId && a.IsDeleted == false).Any();
            if (IsAudienceExists)
            {
                var userCustomRestrictionList = objBDSServiceClient.GetUserCustomRestrictionList(Sessions.User.UserId, Sessions.ApplicationId);
                var isGeographyAllowed = userCustomRestrictionList != null ? userCustomRestrictionList.Where(ucr => ucr.CustomField == Enums.CustomRestrictionType.Geography.ToString()
                    && (ucr.Permission == (int)Enums.CustomRestrictionPermission.ViewEdit)).Any() : false;
                if (isGeographyAllowed)
                {
                    var isVerticalAllowed = userCustomRestrictionList != null ? userCustomRestrictionList.Where(ucr => ucr.CustomField == Enums.CustomRestrictionType.Verticals.ToString()
                                            && (ucr.Permission == (int)Enums.CustomRestrictionPermission.ViewEdit)).Any() : false;

                    if (isVerticalAllowed)
                        canCreateTactic = true;
                }
            }

            if (canCreateTactic == false)
            {
                // Modified by Dharmraj,27-8-2014,#738, To set custom Audience label in message
                ViewBag.CannotCreateQuickTacticMessage = string.Format(Common.objCached.CannotCreateQuickTacticMessage, Common.CustomLabelFor(Enums.CustomLabelCode.Audience));
            }

            ViewBag.CanCreateTactic = canCreateTactic;
            // End - Added by :- Sohel Pathan on 18/17/2014 for PL ticket #594.

            pcpm.ProgramBudget = 0; // Added By Kalpesh #604 : Budget allocation for campaign
            pcpm.AllocatedBy = objPlan.AllocatedBy;

            pcpm.CustomFieldHtmlContent = HtmlHelpers.GenerateCustomFields(0, Enums.EntityType.Program.ToString());//Added by Mitesh Vaishnav for PL ticket #719

            return PartialView("ProgramAssortment", pcpm);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Edit Program.
        /// </summary>
        /// <param name="id">Program Id.</param>
        /// <param name="RedirectType">Redirect Type</param>
        /// <returns>Returns Partial View Of Program.</returns>
        public PartialViewResult EditProgram(int id = 0, string RedirectType = "", string CalledFromBudget = "")
        {
            ViewBag.CalledFromBudget = CalledFromBudget;
            // Dropdown for Verticals
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false);
            /* clientID add by Nirav shah on 15 jan 2014 for get verical and audience by client wise*/
            /* changed by Nirav on 11 APR for PL 322*/
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId);
            //ViewBag.Geography = db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId);
            ViewBag.IsCreated = false;
            if (RedirectType == "Assortment")
            {
                ViewBag.RedirectType = false;
            }
            else
            {
                ViewBag.RedirectType = true;
            }
            Plan_Campaign_Program pcp = db.Plan_Campaign_Program.Where(pcpobj => pcpobj.PlanProgramId.Equals(id) && pcpobj.IsDeleted.Equals(false)).SingleOrDefault();
            if (pcp == null)
            {
                return null;
            }

            ////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(pcp.Plan_Campaign.Plan.Model);
            Plan_Campaign_ProgramModel pcpm = new Plan_Campaign_ProgramModel();
            pcpm.PlanProgramId = pcp.PlanProgramId;
            pcpm.PlanCampaignId = pcp.PlanCampaignId;
            pcpm.Title = HttpUtility.HtmlDecode(pcp.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            pcpm.Description = HttpUtility.HtmlDecode(pcp.Description);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            /* changed by Nirav on 11 APR for PL 322*/
            //pcpm.VerticalId = pcp.VerticalId;
            //pcpm.AudienceId = pcp.AudienceId;
            //pcpm.GeographyId = pcp.GeographyId;
            pcpm.StartDate = pcp.StartDate;
            pcpm.EndDate = pcp.EndDate;
            //if (RedirectType != "Assortment") // Commented by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
            //{
            pcpm.CStartDate = pcp.Plan_Campaign.StartDate;
            pcpm.CEndDate = pcp.Plan_Campaign.EndDate;
            var tsd = (from t in db.Plan_Campaign_Program_Tactic where t.PlanProgramId == id select t);
            if (tsd.Count() > 0)
            {
                pcpm.TStartDate = (from otsd in tsd select otsd.StartDate).Min();
            }
            var ted = (from t in db.Plan_Campaign_Program_Tactic where t.PlanProgramId == id select t);
            if (ted.Count() > 0)
            {
                pcpm.TEndDate = (from oted in ted select oted.EndDate).Max();
            }
            //}
            //pcpm.INQs = pcp.INQs;
            pcpm.MQLs = Common.GetMQLValueTacticList(db.Plan_Campaign_Program_Tactic.Where(t => t.PlanProgramId == pcp.PlanProgramId && t.IsDeleted == false).ToList()).Sum(tm => tm.MQL);
            pcpm.Cost = Common.CalculateProgramCost(pcp.PlanProgramId); //pcp.Cost; modified for PL #440 by dharmraj 

            //Added By : Kalpesh Sharma : PL #605 : 07/29/2014
            List<Plan_Tactic_Values> PlanTacticValuesList = Common.GetMQLValueTacticList(db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.PlanCampaignId == pcp.PlanCampaignId &&
                t.Plan_Campaign_Program.PlanProgramId == pcp.PlanProgramId && t.IsDeleted == false).ToList());
            pcpm.Revenue = Math.Round(PlanTacticValuesList.Sum(tm => tm.Revenue));

            /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/

            //Start added by Kalpesh  #608: Budget allocation for Program
            pcpm.ProgramBudget = pcp.ProgramBudget;
            var objPlan = db.Plans.SingleOrDefault(varP => varP.PlanId == Sessions.PlanId);
            pcpm.AllocatedBy = objPlan.AllocatedBy;

            pcpm.IsDeployedToIntegration = pcp.IsDeployedToIntegration;

            pcpm.CustomFieldHtmlContent = HtmlHelpers.GenerateCustomFields(id, Enums.EntityType.Program.ToString());//Added by Mitesh Vaishnav for PL ticket #719

            if (Sessions.User.UserId == pcp.CreatedBy)
            {
                ViewBag.IsOwner = true;

                // Added by Dharmraj Mangukiya to hide/show delete program as per custom restrictions PL ticket #577
                var AllTactic = pcp.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted.Equals(false)).ToList();
                bool IsProgramDeleteble = true;
                if (AllTactic.Count > 0)
                {
                    var OthersTactic = AllTactic.Where(t => t.CreatedBy != Sessions.User.UserId).ToList();
                    if (OthersTactic.Count > 0)
                    {
                        IsProgramDeleteble = false;
                    }
                    else
                    {
                        var lstUserCustomRestriction = Common.GetUserCustomRestriction();
                        int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                        var lstAllowedVertical = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId.ToLower()).ToList();
                        var lstAllowedGeography = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId.ToLower()).ToList();
                        var lstAllowedBusinessUnit = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).Select(r => r.CustomFieldId.ToLower()).ToList();
                        if (AllTactic.Where(t => t.CreatedBy == Sessions.User.UserId && (!lstAllowedGeography.Contains(t.GeographyId.ToString().ToLower()) || !lstAllowedVertical.Contains(t.VerticalId.ToString().ToLower()) || !lstAllowedBusinessUnit.Contains(t.BusinessUnitId.ToString().ToLower()))).ToList().Count > 0)
                        {
                            IsProgramDeleteble = false;
                        }
                    }
                }

                ViewBag.IsProgramDeleteble = IsProgramDeleteble;
            }
            else
            {
                ViewBag.IsOwner = false;
                ViewBag.IsProgramDeleteble = false;
            }
            ViewBag.Campaign = HttpUtility.HtmlDecode(pcp.Plan_Campaign.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;

            var objPlanCampaign = db.Plan_Campaign.SingleOrDefault(c => c.PlanCampaignId == pcp.PlanCampaignId);
            var lstSelectedProgram = db.Plan_Campaign_Program.Where(p => p.PlanCampaignId == pcp.PlanCampaignId && p.IsDeleted == false).ToList();
            double allProgramBudget = lstSelectedProgram.Sum(c => c.ProgramBudget);
            ViewBag.planRemainingBudget = (objPlanCampaign.CampaignBudget - allProgramBudget);

            return PartialView("ProgramAssortment", pcpm);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Program.
        /// </summary>
        /// <param name="form">Form object of Plan_Campaign_ProgramModel.</param>
        /// <param name="programs">Program list string array.</param>
        /// <param name="RedirectType">Redirect Type.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveProgram(Plan_Campaign_ProgramModel form, string tactics, bool RedirectType, string closedTask, string BudgetInputValues, string customFieldInputs, string UserId = "", string CalledFromBudget = "")
        {
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
                //Deserialize customFieldInputs json string to  KeyValuePair List
                var customFields = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(customFieldInputs);
                string[] arrBudgetInputValues = BudgetInputValues.Split(',');

                if (form.PlanProgramId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pcpvar = (from pcp in db.Plan_Campaign_Program
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pc.PlanId == Sessions.PlanId && pcp.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcp.IsDeleted.Equals(false)
                                          && pc.PlanCampaignId == form.PlanCampaignId       //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
                                          select pcp).FirstOrDefault();

                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                Plan_Campaign_Program pcpobj = new Plan_Campaign_Program();
                                pcpobj.PlanCampaignId = form.PlanCampaignId;
                                pcpobj.Title = form.Title;
                                pcpobj.Description = form.Description;
                                /* changed by Nirav on 11 APR for PL 322*/
                                //pcpobj.VerticalId = form.VerticalId;
                                //pcpobj.AudienceId = form.AudienceId;
                                //pcpobj.GeographyId = form.GeographyId;
                                pcpobj.StartDate = form.StartDate;  // Modified by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                pcpobj.EndDate = form.EndDate;  // Modified by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                pcpobj.CreatedBy = Sessions.User.UserId;
                                pcpobj.CreatedDate = DateTime.Now;
                                pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString(); //status field added for Plan_Campaign_Program table
                                pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;

                                //Added By : Kalpesh Sharma : PL #604 : 07/29/2014
                                pcpobj.ProgramBudget = form.ProgramBudget;

                                db.Entry(pcpobj).State = EntityState.Added;

                                // Start - Added by Sohel Pathan on 04/09/2014 for PL ticket #758
                                var PrevAllocationList = db.Plan_Campaign_Program_Budget.Where(c => c.PlanProgramId == form.PlanProgramId).Select(c => c).ToList();
                                PrevAllocationList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                // End - Added by Sohel Pathan on 04/09/2014 for PL ticket #758

                                //Start added by Kalpesh  #608: Budget allocation for Program
                                if (arrBudgetInputValues.Length == 12)
                                {
                                    for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                    {
                                        if (arrBudgetInputValues[i] != "")
                                        {
                                            Plan_Campaign_Program_Budget objPlanCampaignProgramBudget = new Plan_Campaign_Program_Budget();
                                            objPlanCampaignProgramBudget.PlanProgramId = form.PlanProgramId;
                                            objPlanCampaignProgramBudget.Period = "Y" + (i + 1);
                                            objPlanCampaignProgramBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                            objPlanCampaignProgramBudget.CreatedBy = Sessions.User.UserId;
                                            objPlanCampaignProgramBudget.CreatedDate = DateTime.Now;
                                            db.Entry(objPlanCampaignProgramBudget).State = EntityState.Added;
                                        }
                                    }
                                }
                                else if (arrBudgetInputValues.Length == 4)
                                {
                                    int BudgetInputValuesCounter = 1;

                                    for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                    {
                                        if (arrBudgetInputValues[i] != "")
                                        {
                                            Plan_Campaign_Program_Budget objPlanCampaignProgramBudget = new Plan_Campaign_Program_Budget();
                                            objPlanCampaignProgramBudget.PlanProgramId = form.PlanProgramId;
                                            objPlanCampaignProgramBudget.Period = "Y" + BudgetInputValuesCounter;
                                            objPlanCampaignProgramBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                            objPlanCampaignProgramBudget.CreatedBy = Sessions.User.UserId;
                                            objPlanCampaignProgramBudget.CreatedDate = DateTime.Now;
                                            db.Entry(objPlanCampaignProgramBudget).State = EntityState.Added;
                                        }

                                        BudgetInputValuesCounter = BudgetInputValuesCounter + 3;
                                    }
                                }

                                int result = db.SaveChanges();
                                int programid = pcpobj.PlanProgramId;

                                ////Start Added by Mitesh Vaishnav for PL ticket #719 Custom fields for Program
                                ////save custom fields value for particular Program
                                if (customFields.Count != 0)
                                {
                                    foreach (var item in customFields)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = programid;
                                        objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;

                                    }
                                }
                                ////End Added by Mitesh Vaishnav for PL ticket #719 Custom fields for Program

                                // Start - Added by Sohel Pathan on 09/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                Plan_Campaign pcp = db.Plan_Campaign.Where(pcobj => pcobj.PlanCampaignId.Equals(pcpobj.PlanCampaignId) && pcobj.IsDeleted.Equals(false)).SingleOrDefault();
                                if (pcp != null)
                                {
                                    if (pcp.StartDate > form.StartDate)
                                    {
                                        pcp.StartDate = form.StartDate;
                                    }

                                    if (form.EndDate > pcp.EndDate)
                                    {
                                        pcp.EndDate = form.EndDate;
                                    }
                                    db.Entry(pcp).State = EntityState.Modified;
                                    result = db.SaveChanges();
                                }
                                // End - Added by Sohel Pathan on 09/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                result = Common.InsertChangeLog(Sessions.PlanId, null, programid, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                if (tactics != string.Empty)
                                {
                                    string[] tactic = tactics.Split(',');
                                    var TacticType = db.TacticTypes;
                                    foreach (string tac in tactic)
                                    {
                                        Plan_Campaign_Program_Tactic pcptobj = new Plan_Campaign_Program_Tactic();
                                        pcptobj.PlanProgramId = programid;
                                        int tacid = Convert.ToInt32(tac);
                                        pcptobj.TacticTypeId = tacid;
                                        TacticType mt = db.TacticTypes.Where(m => m.TacticTypeId == tacid).FirstOrDefault();

                                        pcptobj.IsDeployedToIntegration = mt.IsDeployedToIntegration;//mt.Model.IntegrationInstanceId == null ? false : true;
                                        pcptobj.Title = mt.Title;
                                        /* changed by Nirav on 11 APR for PL 322*/

                                        // Start - Added by :- Sohel Pathan on 18/17/2014 for PL ticket #594.
                                        var userCustomRestrictionList = objBDSServiceClient.GetUserCustomRestrictionList(Sessions.User.UserId, Sessions.ApplicationId);
                                        List<int> lstVerticalRestricted = new List<int>();
                                        List<Guid> lstGeographyRestricted = new List<Guid>();
                                        if (userCustomRestrictionList != null)
                                        {
                                            lstVerticalRestricted = userCustomRestrictionList.Where(ucr => ucr.CustomField == Enums.CustomRestrictionType.Verticals.ToString()
                                                                    && (ucr.Permission == (int)Enums.CustomRestrictionPermission.None)).Select(a => Convert.ToInt32(a.CustomFieldId)).ToList();
                                            lstGeographyRestricted = userCustomRestrictionList.Where(ucr => ucr.CustomField == Enums.CustomRestrictionType.Geography.ToString()
                                                                    && (ucr.Permission == (int)Enums.CustomRestrictionPermission.None)).Select(a => Guid.Parse(a.CustomFieldId)).ToList();
                                        }
                                        // End - Added by :- Sohel Pathan on 18/17/2014 for PL ticket #594.

                                        // Start - Added by :- Sohel Pathan on 18/17/2014 for PL ticket #594.
                                        pcptobj.VerticalId = db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId && !lstVerticalRestricted.Contains(vertical.VerticalId)).Select(s => s.VerticalId).FirstOrDefault();
                                        pcptobj.AudienceId = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId).Select(s => s.AudienceId).FirstOrDefault();
                                        pcptobj.GeographyId = db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId && !lstGeographyRestricted.Contains(geography.GeographyId)).Select(s => s.GeographyId).FirstOrDefault();
                                        // End - Added by :- Sohel Pathan on 18/17/2014 for PL ticket #594.

                                        //pcptobj.VerticalId = form.VerticalId;
                                        //pcptobj.AudienceId = form.AudienceId;
                                        //pcptobj.GeographyId = form.GeographyId;
                                        //pcptobj.INQs = mt.ProjectedInquiries == null ? 0 : Convert.ToInt32(mt.ProjectedInquiries);
                                        pcptobj.Cost = mt.ProjectedRevenue == null ? 0 : Convert.ToDouble(mt.ProjectedRevenue);
                                        pcptobj.StartDate = form.StartDate; // Modified by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                        pcptobj.EndDate = form.EndDate; // Modified by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                        pcptobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString();
                                        pcptobj.BusinessUnitId = (from m in db.Models
                                                                  join p in db.Plans on m.ModelId equals p.ModelId
                                                                  where p.PlanId == Sessions.PlanId
                                                                  select m.BusinessUnitId).FirstOrDefault();

                                        pcptobj.StageId = Convert.ToInt32(mt.StageId);
                                        pcptobj.ProjectedStageValue = mt.ProjectedStageValue;

                                        pcptobj.CreatedBy = Sessions.User.UserId;
                                        pcptobj.CreatedDate = DateTime.Now;
                                        db.Entry(pcptobj).State = EntityState.Added;
                                        result = db.SaveChanges();
                                        int tid = pcptobj.PlanTacticId;

                                        //Start by Kalpesh Sharma #605: Cost allocation for Tactic 07/30/2014
                                        if (mt.ProjectedRevenue > 0)
                                        {
                                            Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                            objNewLineitem.PlanTacticId = tid;
                                            objNewLineitem.Title = Common.DefaultLineItemTitle;
                                            objNewLineitem.Cost = Convert.ToDouble(mt.ProjectedRevenue);
                                            objNewLineitem.Description = string.Empty;
                                            objNewLineitem.CreatedBy = Sessions.User.UserId;
                                            objNewLineitem.CreatedDate = DateTime.Now;
                                            db.Entry(objNewLineitem).State = EntityState.Added;
                                            db.SaveChanges();
                                        }

                                        result = Common.InsertChangeLog(Sessions.PlanId, null, tid, pcptobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                    }
                                }
                                //result = TacticValueCalculate(pcpobj.PlanProgramId); // Modified by dharmraj for PL #440
                                Common.ChangeCampaignStatus(pcpobj.PlanCampaignId);     //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                scope.Complete();
                                return Json(new { redirect = Url.Action("Assortment", new { campaignId = form.PlanCampaignId }) });
                            }
                        }
                    }
                }
                else
                {

                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pcpvar = (from pcp in db.Plan_Campaign_Program
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pc.PlanId == Sessions.PlanId && pcp.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcp.PlanProgramId.Equals(form.PlanProgramId) && pcp.IsDeleted.Equals(false)
                                          && pc.PlanCampaignId == form.PlanCampaignId   //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
                                          select pcp).FirstOrDefault();

                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                Plan_Campaign_Program pcpobj = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(form.PlanProgramId)).SingleOrDefault();
                                //Change Plan Id
                                pcpobj.Title = form.Title;
                                pcpobj.Description = form.Description;
                                pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                /* changed by Nirav on 11 APR for PL 322*/
                                //pcpobj.VerticalId = form.VerticalId;
                                //pcpobj.AudienceId = form.AudienceId;
                                //pcpobj.GeographyId = form.GeographyId;
                                //if (RedirectType) // Commented by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                //{
                                pcpobj.StartDate = form.StartDate;
                                pcpobj.EndDate = form.EndDate;
                                if (form.CStartDate > form.StartDate)
                                {
                                    pcpobj.Plan_Campaign.StartDate = form.StartDate;
                                }

                                if (form.EndDate > form.CEndDate)
                                {
                                    pcpobj.Plan_Campaign.EndDate = form.EndDate;
                                }
                                //}
                                //pcpobj.INQs = (form.INQs == null ? 0 : form.INQs);
                                //pcpobj.MQLs = (form.MQLs == null ? 0 : form.MQLs);
                                //pcpobj.Cost = (form.Cost == null ? 0 : form.Cost);
                                pcpobj.ModifiedBy = Sessions.User.UserId;
                                pcpobj.ModifiedDate = DateTime.Now;

                                //Added By : Kalpesh Sharma : PL #604 : 07/29/2014
                                pcpobj.ProgramBudget = form.ProgramBudget;

                                //Start added by Kalpesh  #608: Budget allocation for Program
                                var PrevAllocationList = db.Plan_Campaign_Program_Budget.Where(c => c.PlanProgramId == form.PlanProgramId).Select(c => c).ToList();    // Modified by Sohel Pathan on 04/09/2014 for PL ticket #758
                                //PrevAllocationList.ForEach(a => db.Entry(a).State = EntityState.Deleted); // Commented by Sohel Pathan on 03/09/2014 for PL ticket #758

                                if (arrBudgetInputValues.Length == 12)
                                {
                                    for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                    {
                                        // Start - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
                                        bool isExists = false;
                                        if (PrevAllocationList != null)
                                        {
                                            if (PrevAllocationList.Count > 0)
                                            {
                                                var updatePlanProgramBudget = PrevAllocationList.Where(pb => pb.Period == ("Y" + (i + 1))).FirstOrDefault();
                                                if (updatePlanProgramBudget != null)
                                                {
                                                    if (arrBudgetInputValues[i] != "")
                                                    {
                                                        var newValue = Convert.ToDouble(arrBudgetInputValues[i]);
                                                        if (updatePlanProgramBudget.Value != newValue)
                                                        {
                                                            updatePlanProgramBudget.Value = newValue;
                                                            db.Entry(updatePlanProgramBudget).State = EntityState.Modified;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        db.Entry(updatePlanProgramBudget).State = EntityState.Deleted;
                                                    }
                                                    isExists = true;
                                                }
                                            }
                                        }
                                        if (!isExists && arrBudgetInputValues[i] != "")
                                        {
                                            // End - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
                                            Plan_Campaign_Program_Budget objPlanCampaignProgramBudget = new Plan_Campaign_Program_Budget();
                                            objPlanCampaignProgramBudget.PlanProgramId = form.PlanProgramId;
                                            objPlanCampaignProgramBudget.Period = "Y" + (i + 1);
                                            objPlanCampaignProgramBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                            objPlanCampaignProgramBudget.CreatedBy = Sessions.User.UserId;
                                            objPlanCampaignProgramBudget.CreatedDate = DateTime.Now;
                                            db.Entry(objPlanCampaignProgramBudget).State = EntityState.Added;
                                        }
                                    }
                                }
                                else if (arrBudgetInputValues.Length == 4)
                                {
                                    int BudgetInputValuesCounter = 1;
                                    for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                    {
                                        // Start - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
                                        bool isExists = false;
                                        if (PrevAllocationList != null)
                                        {
                                            if (PrevAllocationList.Count > 0)
                                            {
                                                var thisQuartersMonthList = PrevAllocationList.Where(pb => pb.Period == ("Y" + (BudgetInputValuesCounter)) || pb.Period == ("Y" + (BudgetInputValuesCounter + 1)) || pb.Period == ("Y" + (BudgetInputValuesCounter + 2))).ToList().OrderBy(a => a.Period).ToList();
                                                var thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                                if (thisQuarterFirstMonthBudget != null)
                                                {
                                                    if (arrBudgetInputValues[i] != "")
                                                    {
                                                        var thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                                        var thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                                        var newValue = Convert.ToDouble(arrBudgetInputValues[i]);

                                                        if (thisQuarterTotalBudget != newValue)
                                                        {
                                                            var BudgetDiff = newValue - thisQuarterTotalBudget;
                                                            if (BudgetDiff > 0)
                                                            {
                                                                thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                                                db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                            }
                                                            else
                                                            {
                                                                int j = 1;
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
                                                                    if ((BudgetInputValuesCounter + j) <= (BudgetInputValuesCounter + 2))
                                                                    {
                                                                        thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == ("Y" + (BudgetInputValuesCounter + j))).FirstOrDefault();
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
                                        }
                                        if (!isExists && arrBudgetInputValues[i] != "")
                                        {
                                            // End - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
                                            Plan_Campaign_Program_Budget objPlanCampaignProgramBudget = new Plan_Campaign_Program_Budget();
                                            objPlanCampaignProgramBudget.PlanProgramId = form.PlanProgramId;
                                            objPlanCampaignProgramBudget.Period = "Y" + BudgetInputValuesCounter;
                                            objPlanCampaignProgramBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                            objPlanCampaignProgramBudget.CreatedBy = Sessions.User.UserId;
                                            objPlanCampaignProgramBudget.CreatedDate = DateTime.Now;
                                            db.Entry(objPlanCampaignProgramBudget).State = EntityState.Added;
                                        }
                                        BudgetInputValuesCounter = BudgetInputValuesCounter + 3;
                                    }
                                }

                                db.Entry(pcpobj).State = EntityState.Modified;

                                ////Start Added by Mitesh Vaishnav for PL ticket #719 Custom fields for Program
                                //// delete previous custom field values and save modified custom fields value for particular Program
                                string entityTypeProgram = Enums.EntityType.Program.ToString();
                                var prevCustomFieldList = db.CustomField_Entity.Where(c => c.EntityId == form.PlanProgramId && c.CustomField.EntityType == entityTypeProgram).ToList();
                                prevCustomFieldList.ForEach(c => db.Entry(c).State = EntityState.Deleted);

                                if (customFields.Count != 0)
                                {
                                    foreach (var item in customFields)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = form.PlanProgramId;
                                        objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;
                                    }
                                }
                                ////End Added by Mitesh Vaishnav for PL ticket #719 Custom fields for Program

                                int result = db.SaveChanges();
                                result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanProgramId, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                if (result >= 1)
                                {
                                    Common.ChangeCampaignStatus(pcpobj.PlanCampaignId);     //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                    scope.Complete();
                                    if (!string.IsNullOrEmpty(CalledFromBudget))
                                    {
                                        TempData["SuccessMessage"] = "Program saved successfully";
                                        return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget, expand = "program" + form.PlanProgramId.ToString() }) });
                                    }
                                    else
                                    {
                                        if (RedirectType)
                                        {
                                            TempData["ClosedTask"] = closedTask;
                                            return Json(new { redirect = Url.Action("ApplyToCalendar") });
                                        }
                                        else
                                        {
                                            return Json(new { redirect = Url.Action("Assortment", new { campaignId = form.PlanCampaignId }) });
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

            return Json(new { });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Delete Program.
        /// </summary>
        /// <param name="id">Program Id.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/
        public ActionResult DeleteProgram(int id = 0, bool RedirectType = false, string closedTask = null, string UserId = "", string CalledFromBudget = "")
        {
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
                            Plan_Campaign_Program pc = db.Plan_Campaign_Program.Where(p => p.PlanProgramId == id).SingleOrDefault();
                            cid = pc.PlanCampaignId;
                            Title = pc.Title;
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pc.PlanProgramId, pc.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                            if (returnValue >= 1)
                            {
                                //TacticValueCalculate(pc.PlanCampaignId, false); modofied for PL #440 by dharmraj
                                Common.ChangeCampaignStatus(pc.PlanCampaignId);     //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                scope.Complete();
                                /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/


                                //return Json(new { redirect = Url.Action("Assortment", new { campaignId = pc.PlanCampaignId }) });
                                if (!string.IsNullOrEmpty(CalledFromBudget))
                                {
                                    TempData["SuccessMessage"] = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                                    return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget, expand = "campaign" + cid.ToString() }) });
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
        /// Added By: Bhavesh Dobariya.
        /// Action to Create Tactic.
        /// </summary>
        /// <returns>Returns Partial View Of Tactic.</returns>
        public PartialViewResult CreateTactic(int id = 0)
        {
            // Added By Bhavesh : 25-June-2014 : #538 Custom Restriction
            List<UserCustomRestrictionModel> lstUserCustomRestriction = Common.GetUserCustomRestriction();

            // Dropdown for Verticals
            //ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false);
            //ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false);
            /* clientID add by Nirav shah on 15 jan 2014 for get verical and audience by client wise*/
            // Added By Bhavesh : 25-June-2014 : #538 Custom Restriction
            ViewBag.Verticals = (from v in db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId).ToList()
                                 join lu in lstUserCustomRestriction on v.VerticalId.ToString() equals lu.CustomFieldId
                                 where lu.CustomField == Enums.CustomRestrictionType.Verticals.ToString() && lu.Permission == (int)Enums.CustomRestrictionPermission.ViewEdit
                                 select v).ToList();
            ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId);
            // Added By Bhavesh : 25-June-2014 : #538 Custom Restriction // Modified by dharmraj for Geography dropdown issue
            ViewBag.Geography = (from g in db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId).ToList()
                                 join lu in lstUserCustomRestriction on g.GeographyId.ToString().ToLower() equals lu.CustomFieldId.ToLower()
                                 where lu.CustomField == Enums.CustomRestrictionType.Geography.ToString() && lu.Permission == (int)Enums.CustomRestrictionPermission.ViewEdit
                                 select g).ToList();
            ////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            var tactics = from t in db.TacticTypes
                          join p in db.Plans on t.ModelId equals p.ModelId
                          where p.PlanId == Sessions.PlanId && (t.IsDeleted == null || t.IsDeleted == false) && t.IsDeployedToModel == true //// Modified by Sohel Pathan on 17/07/2014 for PL ticket #594
                          orderby t.Title
                          select t;
            foreach (var item in tactics)
            {
                item.Title = HttpUtility.HtmlDecode(item.Title);
            }
            ViewBag.Tactics = tactics;
            ////End :Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            ViewBag.IsCreated = true;


            var objPlan = db.Plans.SingleOrDefault(varP => varP.PlanId == Sessions.PlanId);
            ////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);
            Plan_Campaign_Program pcpt = db.Plan_Campaign_Program.Where(pcpobj => pcpobj.PlanProgramId.Equals(id)).SingleOrDefault();
            if (pcpt == null)
            {
                return null;
            }//uday PL Ticket #550 3-7-2014

            Plan_Campaign_Program_TacticModel pcptm = new Plan_Campaign_Program_TacticModel();
            pcptm.PlanProgramId = id;
            pcptm.IsDeployedToIntegration = false;
            pcptm.StageId = 0;
            pcptm.StageTitle = "Stage";
            //Plan_Campaign_Program pcp = db.Plan_Campaign_Program.Where(pcpo => pcpo.PlanProgramId == id).SingleOrDefault();
            //pcptm.GeographyId = pcp.GeographyId;
            //pcptm.VerticalId = pcp.VerticalId;
            //pcptm.AudienceId = pcp.AudienceId;
            ViewBag.IsOwner = true;/*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/

            // Added By Bhavesh : 25-June-2014 : #538 Custom Restriction
            ViewBag.IsAllowCustomRestriction = true;
            ViewBag.Program = HttpUtility.HtmlDecode(pcpt.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            ViewBag.Campaign = HttpUtility.HtmlDecode(pcpt.Plan_Campaign.Title); ////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            ViewBag.RedirectType = false;
            // Start - Added by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
            pcptm.StartDate = GetCurrentDateBasedOnPlan();
            pcptm.EndDate = GetCurrentDateBasedOnPlan(true);
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;
            // End - Added by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen

            //Start by Kalpesh Sharma #605: Cost allocation for Tactic
            pcptm.TacticCost = 0;
            pcptm.AllocatedBy = objPlan.AllocatedBy;

            pcptm.CustomFieldHtmlContent = HtmlHelpers.GenerateCustomFields(0, Enums.EntityType.Tactic.ToString());//Added by Mitesh Vaishnav for PL ticket #720

            return PartialView("TacticAssortment", pcptm);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Create Tactic.
        /// </summary>
        /// <param name="id">Tactic Id.</param>
        /// <param name="RedirectType">Redirect Type</param>
        /// <returns>Returns Partial View Of Tactic.</returns>
        public PartialViewResult EditTactic(int id = 0, string RedirectType = "", string CalledFromBudget = "")
        {
            ViewBag.CalledFromBudget = CalledFromBudget;
            var tList = from t in db.TacticTypes
                        join p in db.Plans on t.ModelId equals p.ModelId
                        where p.PlanId == Sessions.PlanId && (t.IsDeleted == null || t.IsDeleted == false) && t.IsDeployedToModel == true //// Modified by Sohel Pathan on 17/07/2014 for PL ticket #594
                        orderby t.Title
                        select t;

            ViewBag.IsCreated = false;
            if (RedirectType == "Assortment")
            {
                ViewBag.RedirectType = false;
            }
            else
            {
                ViewBag.RedirectType = true;

            }
            Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(pcptobj => pcptobj.PlanTacticId.Equals(id)).SingleOrDefault();
            if (pcpt == null)
            {
                return null;
            }

            ViewBag.IsTacticAfterApproved =Common.CheckAfterApprovedStatus(pcpt.Status);////Added by Mitesh Vaishnav for PL ticket #571 for checking tactic has after approved status
            // Start - Added by Sohel Pathan on 17/07/2014 for PL ticket #594 
            if (!tList.Any(t => t.TacticTypeId == pcpt.TacticTypeId))
            {
                var tacticTypeSpecial = from t in db.TacticTypes
                                        join p in db.Plans on t.ModelId equals p.ModelId
                                        where p.PlanId == Sessions.PlanId && t.TacticTypeId == pcpt.TacticTypeId
                                        orderby t.Title
                                        select t;
                tList = tList.Concat<TacticType>(tacticTypeSpecial);
                tList = tList.OrderBy(a => a.Title);
            }
            // End - Added by Sohel Pathan on 17/07/2014 for PL ticket #594 

            /*Added by Mitesh Vaishnav on 07/07/2014 for PL ticket #584*/
            foreach (var item in tList)
            {
                item.Title = HttpUtility.HtmlDecode(item.Title);
            }
            /*End :Added by Mitesh Vaishnav on 07/07/2014 for PL ticket #584*/

            // Added By Bhavesh : 25-June-2014 : #538 Custom Restriction
            List<UserCustomRestrictionModel> lstUserCustomRestriction = Common.GetUserCustomRestriction();
            bool isallowrestriction = Common.GetRightsForTactic(lstUserCustomRestriction, pcpt.VerticalId, pcpt.GeographyId);
            ViewBag.IsAllowCustomRestriction = isallowrestriction;
            // Dropdown for Verticals
            /* clientID add by Nirav shah on 15 jan 2014 for get verical and audience by client wise*/
            // Added By Bhavesh : 25-June-2014 : #538 Custom Restriction
            if (isallowrestriction)
            {
                ViewBag.Verticals = (from v in db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId).ToList()
                                     join lu in lstUserCustomRestriction on v.VerticalId.ToString() equals lu.CustomFieldId
                                     where lu.CustomField == Enums.CustomRestrictionType.Verticals.ToString() && lu.Permission == (int)Enums.CustomRestrictionPermission.ViewEdit
                                     select v).ToList();
            }
            else
            {
                ViewBag.Verticals = db.Verticals.Where(vertical => vertical.IsDeleted == false && vertical.ClientId == Sessions.User.ClientId);
            }
            ViewBag.Audience = db.Audiences.Where(audience => audience.IsDeleted == false && audience.ClientId == Sessions.User.ClientId);

            // Added By Bhavesh : 25-June-2014 : #538 Custom Restriction
            if (isallowrestriction)
            {
                ViewBag.Geography = (from g in db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId).ToList()
                                     join lu in lstUserCustomRestriction on g.GeographyId.ToString().ToLower() equals lu.CustomFieldId.ToLower()////Modified by Mitesh Vaishnav For functional review point 89
                                     where lu.CustomField == Enums.CustomRestrictionType.Geography.ToString() && lu.Permission == (int)Enums.CustomRestrictionPermission.ViewEdit
                                     select g).ToList();
            }
            else
            {
                ViewBag.Geography = db.Geographies.Where(geography => geography.IsDeleted == false && geography.ClientId == Sessions.User.ClientId);
            }
            ////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(pcpt.TacticType.Model);

            Plan_Campaign_Program_TacticModel pcptm = new Plan_Campaign_Program_TacticModel();
            pcptm.PlanProgramId = pcpt.PlanProgramId;
            pcptm.PlanTacticId = pcpt.PlanTacticId;
            pcptm.TacticTypeId = pcpt.TacticTypeId;
            pcptm.Title = HttpUtility.HtmlDecode(pcpt.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            pcptm.Description = HttpUtility.HtmlDecode(pcpt.Description);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            pcptm.VerticalId = pcpt.VerticalId;
            pcptm.AudienceId = pcpt.AudienceId;
            pcptm.GeographyId = pcpt.GeographyId;
            pcptm.StartDate = pcpt.StartDate;
            pcptm.EndDate = pcpt.EndDate;
            //if (RedirectType != "Assortment") // Commented by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
            //{
            pcptm.PStartDate = pcpt.Plan_Campaign_Program.StartDate;
            pcptm.PEndDate = pcpt.Plan_Campaign_Program.EndDate;
            pcptm.CStartDate = pcpt.Plan_Campaign_Program.Plan_Campaign.StartDate;
            pcptm.CEndDate = pcpt.Plan_Campaign_Program.Plan_Campaign.EndDate;
            //}

            //pcptm.INQs = pcpt.INQs;
            /// Added by Dharmraj on 4-Sep-2014
            /// #760 Advanced budgeting – show correct revenue in Tactic fly out
            List<Plan_Campaign_Program_Tactic> lstTmpTac = new List<Plan_Campaign_Program_Tactic>();
            lstTmpTac.Add(pcpt);
            List<TacticStageValue> varTacticStageValue = Common.GetTacticStageRelation(lstTmpTac, false);
            // Set MQL
            string stageMQL = Enums.Stage.MQL.ToString();
            int levelMQL = db.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageMQL)).Level.Value;
            int tacticStageLevel = Convert.ToInt32(db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == pcpt.PlanTacticId).Stage.Level);
            if (tacticStageLevel < levelMQL)
            {
                //pcptm.MQLs = Common.CalculateMQLTactic(Convert.ToDouble(pcpt.ProjectedStageValue), pcpt.StartDate, pcpt.PlanTacticId, pcpt.StageId, pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId);
                if (varTacticStageValue.Count > 0)
                {
                    pcptm.MQLs = varTacticStageValue[0].MQLValue;
                }
                else
                {
                    pcptm.MQLs = 0;
                }
            }
            else if (tacticStageLevel == levelMQL)
            {
                pcptm.MQLs = Convert.ToDouble(pcpt.ProjectedStageValue);
            }
            else if (tacticStageLevel > levelMQL)
            {
                pcptm.MQLs = 0;
                TempData["TacticMQL"] = "N/A";
            }

            // Set Revenue
            if (varTacticStageValue.Count > 0)
            {
                pcptm.Revenue = Math.Round(varTacticStageValue[0].RevenueValue, 2); // Modified by Sohel Pathan on 15/09/2014 for PL ticket #760
            }
            else
            {
                pcptm.Revenue = 0;
            }

            pcptm.Cost = pcpt.Cost;

            pcptm.IsDeployedToIntegration = pcpt.IsDeployedToIntegration;

            pcptm.StageId = Convert.ToInt32(pcpt.StageId);
            pcptm.StageTitle = db.Stages.FirstOrDefault(varS => varS.StageId == pcpt.StageId).Title;
            pcptm.ProjectedStageValue = Convert.ToDouble(pcpt.ProjectedStageValue);
            /*Added by Mitesh Vaishnav on 13/06/2014 to address changes related to #498 Customized Target Stage - Publish model*/
            var modelTacticStageType = tList.Where(tt => tt.TacticTypeId == pcpt.TacticTypeId).FirstOrDefault().StageId;
            var plantacticStageType = pcpt.StageId;
            if (modelTacticStageType == plantacticStageType)
            {
                ViewBag.IsDiffrentStageType = false;
            }
            else
            {
                ViewBag.IsDiffrentStageType = true;
            }
            /*End Added by Mitesh Vaishnav on 13/06/2014 to address changes related to #498 Customized Target Stage - Publish model */

            /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     changed by : Nirav Shah on 13 feb 2014*/
            if (Sessions.User.UserId == pcpt.CreatedBy)
            {
                ViewBag.IsOwner = true;
            }
            else
            {
                ViewBag.IsOwner = false;
            }
            List<TacticType> tnewList = tList.ToList();
            TacticType tobj = db.TacticTypes.Where(t => t.TacticTypeId == pcptm.TacticTypeId && t.IsDeleted == true).SingleOrDefault();
            if (tobj != null)
            {
                TacticType tSameExist = tnewList.Where(t => t.Title.Equals(tobj.Title)).SingleOrDefault();
                if (tSameExist != null)
                {
                    tnewList.Remove(tSameExist);
                }
                tnewList.Add(tobj);

            }
            ViewBag.Tactics = tnewList.OrderBy(t => t.Title);
            ViewBag.Program = HttpUtility.HtmlDecode(pcpt.Plan_Campaign_Program.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            ViewBag.Campaign = HttpUtility.HtmlDecode(pcpt.Plan_Campaign_Program.Plan_Campaign.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;

            //Start by Kalpesh Sharma #605: Cost allocation for Tactic
            pcptm.TacticCost = pcpt.Cost;
            var objPlan = db.Plans.SingleOrDefault(varP => varP.PlanId == Sessions.PlanId);
            pcptm.AllocatedBy = objPlan.AllocatedBy;

            pcptm.CustomFieldHtmlContent = HtmlHelpers.GenerateCustomFields(id, Enums.EntityType.Tactic.ToString());//Added by Mitesh Vaishnav for PL ticket #720

            //Added By : Kalpesh Sharma : PL #605 : 07/29/2014
            //List<Plan_Tactic_Values> PlanTacticValuesList = Common.GetMQLValueTacticList(db.Plan_Campaign_Program_Tactic.Where(t => t.PlanProgramId == pcpt.PlanProgramId &&
            //    t.PlanTacticId == pcpt.PlanTacticId && t.IsDeleted == false).ToList());
            //pcptm.Revenue = Math.Round(PlanTacticValuesList.Sum(tm => tm.Revenue));

            //Added By : Kalpesh Sharma Functioan and code review #693
            var CostTacticsBudget = db.Plan_Campaign_Program_Tactic.Where(c => c.PlanProgramId == pcpt.PlanProgramId).ToList().Sum(c => c.Cost);
            var objPlanCampaignProgram = db.Plan_Campaign_Program.SingleOrDefault(p => p.PlanProgramId == pcpt.PlanProgramId);
            ViewBag.planRemainingBudget = (objPlanCampaignProgram.ProgramBudget - (!string.IsNullOrEmpty(Convert.ToString(CostTacticsBudget)) ? CostTacticsBudget : 0));

            return PartialView("TacticAssortment", pcptm);
        }

        /// <summary>
        /// Added By : Kalpesh Sharma : Functional Review Points #697
        /// This method is responsible to add the Line items in Tactic.
        /// </summary>
        /// <param name="form">Pass the Tactic form model for fetching Start date and end date values</param>
        /// <param name="lineitems">pass the lineitems</param>
        /// <param name="tacticId">Tactic Id that used to insert into Lineitem table </param>
        public int SaveLineItems(Plan_Campaign_Program_TacticModel form, string lineitems, int tacticId)
        {
            int Result = 0;

            // Added by Dharmraj for PL #644

            //Fetch the lineitems and store into array of string
            string[] lineitem = lineitems.Split(',');

            // Modified By : Kalpesh Sharma 
            // Functional and Code Review Point #697
            // Fetch the current plan object and based on it's ModelID we have fetch the lineitems.
            Plan objPlan = db.Plans.Where(p => p.PlanId == Sessions.PlanId).SingleOrDefault();
            var lineItemType = db.LineItemTypes.Where(l => l.ModelId == objPlan.ModelId).ToList();
            // End Modified By : Kalpesh Sharma 

            //Iterating the collection of lineitems that we hasd perviously fetch.
            foreach (string li in lineitem)
            {
                //insert new lineItem into the LineItem Table. 
                Plan_Campaign_Program_Tactic_LineItem pcptlobj = new Plan_Campaign_Program_Tactic_LineItem();
                pcptlobj.PlanTacticId = tacticId;
                int lineItemTypeid = Convert.ToInt32(li);
                pcptlobj.LineItemTypeId = lineItemTypeid;

                //Fetch the LineItemType by it's ID
                LineItemType lit = lineItemType.Where(m => m.LineItemTypeId == lineItemTypeid).FirstOrDefault();

                // Added By : Kalpesh Sharma 
                // Functional and Code Review Point #697
                pcptlobj.Title = (lit.Title == Enums.LineItemTypes.None.ToString()) ? DefaultLineItemTitle : lit.Title;
                pcptlobj.Cost = 0;
                pcptlobj.StartDate = form.StartDate;
                pcptlobj.EndDate = form.EndDate;
                pcptlobj.CreatedBy = Sessions.User.UserId;
                pcptlobj.CreatedDate = DateTime.Now;
                db.Entry(pcptlobj).State = EntityState.Added;

                //Save the database changes and get new inserted PlanLineItemId   
                Result = db.SaveChanges();
                int liid = pcptlobj.PlanLineItemId;

                //insert chnage log into the database 
                Result = Common.InsertChangeLog(Sessions.PlanId, null, liid, pcptlobj.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
            }

            //retturn the reuslt status
            return Result;
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Tactic.
        /// </summary>
        /// <param name="form">Form object of Plan_Campaign_Program_TacticModel.</param>
        /// <param name="programs">Program list string array.</param>
        /// <param name="RedirectType">Redirect Type.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveTactic(Plan_Campaign_Program_TacticModel form, string lineitems, bool RedirectType, string closedTask, string BudgetInputValues, string actualInputValues, string customFieldInputs, string UserId = "", string CalledFromBudget = "")
        {
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
                int cid = db.Plan_Campaign_Program.Where(p => p.PlanProgramId == form.PlanProgramId).Select(p => p.PlanCampaignId).FirstOrDefault();
                int pid = form.PlanProgramId;

                string[] arrBudgetInputValues = BudgetInputValues.Split(',');
                string[] arrActualCostInputValues = actualInputValues.Split(',');
                //Deserialize customFieldInputs json string to  KeyValuePair List
                var customFields = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(customFieldInputs);

                if (form.PlanTacticId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                          join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pc.PlanId == Sessions.PlanId && pcpt.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcpt.IsDeleted.Equals(false)
                                          && pcp.PlanProgramId == form.PlanProgramId    //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
                                          select pcp).FirstOrDefault();

                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                Plan_Campaign_Program_Tactic pcpobj = new Plan_Campaign_Program_Tactic();
                                pcpobj.PlanProgramId = form.PlanProgramId;
                                pcpobj.Title = form.Title;
                                pcpobj.TacticTypeId = form.TacticTypeId;
                                pcpobj.Description = form.Description;
                                pcpobj.VerticalId = form.VerticalId;
                                pcpobj.AudienceId = form.AudienceId;
                                pcpobj.GeographyId = form.GeographyId;
                                //pcpobj.INQs = form.INQs;
                                //Added By Kalpesh Sharma : #752 Update line item cost with the total cost from the monthly/quarterly allocation
                                pcpobj.Cost = UpdateBugdetAllocationCost(arrBudgetInputValues, form.TacticCost);
                                pcpobj.StartDate = form.StartDate;  // Modified by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                pcpobj.EndDate = form.EndDate;  // Modified by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString();
                                pcpobj.BusinessUnitId = (from m in db.Models
                                                         join p in db.Plans on m.ModelId equals p.ModelId
                                                         where p.PlanId == Sessions.PlanId
                                                         select m.BusinessUnitId).FirstOrDefault();
                                pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                pcpobj.StageId = form.StageId;
                                pcpobj.ProjectedStageValue = form.ProjectedStageValue;
                                pcpobj.CreatedBy = Sessions.User.UserId;
                                pcpobj.CreatedDate = DateTime.Now;
                                db.Entry(pcpobj).State = EntityState.Added;
                                int result = db.SaveChanges();
                                int tacticId = pcpobj.PlanTacticId;

                                // Start Added by dharmraj for ticket #644
                                if (pcpobj.Cost > 0)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = tacticId;
                                    objNewLineitem.Title = Common.DefaultLineItemTitle;
                                    objNewLineitem.Cost = pcpobj.Cost;
                                    objNewLineitem.Description = string.Empty;
                                    objNewLineitem.CreatedBy = Sessions.User.UserId;
                                    objNewLineitem.CreatedDate = DateTime.Now;
                                    db.Entry(objNewLineitem).State = EntityState.Added;
                                    db.SaveChanges();
                                }
                                // End Added by dharmraj for ticket #644

                                ////Start - Added by : Mitesh Vaishnav on 25-06-2014    for PL ticket 554 Home & Plan Pages: Program and Campaign Blocks are not covering newly added Tactic.
                                var planCampaignProgramDetails = (from pcp in db.Plan_Campaign_Program
                                                                  join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                                                  where pcp.PlanProgramId == pcpobj.PlanProgramId
                                                                  select pcp).FirstOrDefault();
                                if (planCampaignProgramDetails.StartDate > pcpobj.StartDate)
                                {
                                    planCampaignProgramDetails.StartDate = pcpobj.StartDate;
                                }
                                if (planCampaignProgramDetails.Plan_Campaign.StartDate > pcpobj.StartDate)
                                {
                                    planCampaignProgramDetails.Plan_Campaign.StartDate = pcpobj.StartDate;
                                }
                                if (pcpobj.EndDate > planCampaignProgramDetails.EndDate)
                                {
                                    planCampaignProgramDetails.EndDate = pcpobj.EndDate;
                                }
                                if (pcpobj.EndDate > planCampaignProgramDetails.Plan_Campaign.EndDate)
                                {
                                    planCampaignProgramDetails.Plan_Campaign.EndDate = pcpobj.EndDate;
                                }
                                db.Entry(planCampaignProgramDetails).State = EntityState.Modified;

                                ////start Added by Mitesh Vaishnav for PL ticket #571
                                //Insert actual cost of tactic
                                for (int i = 0; i < arrActualCostInputValues.Length; i++)
                                {
                                    if (arrActualCostInputValues[i] != "")
                                    {
                                        Plan_Campaign_Program_Tactic_Actual obPlanCampaignProgramTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                        obPlanCampaignProgramTacticActual.PlanTacticId = tacticId;
                                        obPlanCampaignProgramTacticActual.StageTitle = Enums.InspectStage.Cost.ToString();
                                        obPlanCampaignProgramTacticActual.Period = "Y" + (i + 1);
                                        obPlanCampaignProgramTacticActual.Actualvalue = Convert.ToDouble(arrActualCostInputValues[i]);
                                        obPlanCampaignProgramTacticActual.CreatedBy = Sessions.User.UserId;
                                        obPlanCampaignProgramTacticActual.CreatedDate = DateTime.Now;
                                        db.Entry(obPlanCampaignProgramTacticActual).State = EntityState.Added;
                                    }
                                }
                                ////End Added by Mitesh Vaishnav for PL ticket #571

                                // Start - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
                                var PrevAllocationList = db.Plan_Campaign_Program_Tactic_Cost.Where(c => c.PlanTacticId == form.PlanTacticId).Select(c => c).ToList();
                                PrevAllocationList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                                // End - Added by Sohel Pathan on 04/09/2014 for PL ticket #759

                                //Start by Kalpesh Sharma #605: Cost allocation for Tactic
                                if (arrBudgetInputValues.Length == 12)
                                {
                                    for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                    {
                                        if (arrBudgetInputValues[i] != "")
                                        {
                                            Plan_Campaign_Program_Tactic_Cost obPlanCampaignProgramTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                            obPlanCampaignProgramTacticCost.PlanTacticId = tacticId;
                                            obPlanCampaignProgramTacticCost.Period = "Y" + (i + 1);
                                            obPlanCampaignProgramTacticCost.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                            obPlanCampaignProgramTacticCost.CreatedBy = Sessions.User.UserId;
                                            obPlanCampaignProgramTacticCost.CreatedDate = DateTime.Now;
                                            db.Entry(obPlanCampaignProgramTacticCost).State = EntityState.Added;
                                        }
                                    }
                                }
                                else if (arrBudgetInputValues.Length == 4)
                                {
                                    int BudgetInputValuesCounter = 1;

                                    for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                    {
                                        if (arrBudgetInputValues[i] != "")
                                        {
                                            Plan_Campaign_Program_Tactic_Cost obPlanCampaignProgramTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                            obPlanCampaignProgramTacticCost.PlanTacticId = tacticId;
                                            obPlanCampaignProgramTacticCost.Period = "Y" + BudgetInputValuesCounter;
                                            obPlanCampaignProgramTacticCost.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                            obPlanCampaignProgramTacticCost.CreatedBy = Sessions.User.UserId;
                                            obPlanCampaignProgramTacticCost.CreatedDate = DateTime.Now;
                                            db.Entry(obPlanCampaignProgramTacticCost).State = EntityState.Added;
                                        }

                                        BudgetInputValuesCounter = BudgetInputValuesCounter + 3;
                                    }
                                }

                                ////Start Added by Mitesh Vaishnav for PL ticket #720 Custom fields for Tactics
                                ////save custom fields value for particular Tactic
                                if (customFields.Count != 0)
                                {
                                    foreach (var item in customFields)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = tacticId;
                                        objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;

                                    }
                                }
                                ////End Added by Mitesh Vaishnav for PL ticket #720 Custom fields for Tactics

                                db.SaveChanges();

                                //result = TacticValueCalculate(pcpobj.PlanProgramId); // Commented by Dharmraj for PL #440
                                result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);

                                // Added By : Kalpesh Sharma : Functional Review Points #697
                                // Check whether the lineitmes is empty or not . if lineitems have any instance at that time call the SaveLineItems function and insert the data into the lineItems table. 
                                if (lineitems != string.Empty)
                                {
                                    result = SaveLineItems(form, lineitems, tacticId);
                                }

                                if (result >= 1)
                                {
                                    //// Start - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                    Common.ChangeProgramStatus(pcpobj.PlanProgramId);
                                    var PlanCampaignId = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.PlanProgramId == pcpobj.PlanProgramId).Select(a => a.PlanCampaignId).Single();
                                    Common.ChangeCampaignStatus(PlanCampaignId);
                                    //// End - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425

                                    scope.Complete();

                                    return Json(new { redirect = Url.Action("Assortment", new { campaignId = cid, programId = pid }) });
                                }
                            }
                        }
                    }
                }
                else
                {

                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                          join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pc.PlanId == Sessions.PlanId && pcpt.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(form.PlanTacticId) && pcpt.IsDeleted.Equals(false)
                                          && pcp.PlanProgramId == form.PlanProgramId    //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
                                          select pcp).FirstOrDefault();

                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                bool isReSubmission = false;
                                bool isDirectorLevelUser = false;
                                bool isOwner = false;
                                string status = string.Empty;

                                Plan_Campaign_Program_Tactic pcpobj = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(form.PlanTacticId)).SingleOrDefault();
                                if (pcpobj.CreatedBy == Sessions.User.UserId) isOwner = true;
                                //if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
                                //{
                                //    if (!isOwner) isDirectorLevelUser = true;
                                //}
                                // Added by dharmraj for Ticket #537
                                var lstUserHierarchy = objBDSServiceClient.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
                                var lstSubordinates = lstUserHierarchy.Where(u => u.ManagerId == Sessions.User.UserId).ToList().Select(u => u.UserId).ToList();
                                if (lstSubordinates.Count > 0)
                                {
                                    if (lstSubordinates.Contains(pcpobj.CreatedBy))
                                    {
                                        if (!isOwner) isDirectorLevelUser = true;
                                    }
                                }


                                pcpobj.Title = form.Title;
                                status = pcpobj.Status;
                                if (pcpobj.TacticTypeId != form.TacticTypeId)
                                {
                                    pcpobj.TacticTypeId = form.TacticTypeId;
                                    if (!isDirectorLevelUser) isReSubmission = true;
                                }
                                pcpobj.Description = form.Description;
                                if (pcpobj.VerticalId != form.VerticalId)
                                {
                                    pcpobj.VerticalId = form.VerticalId;
                                    if (!isDirectorLevelUser) isReSubmission = true;
                                }
                                if (pcpobj.AudienceId != form.AudienceId)
                                {
                                    pcpobj.AudienceId = form.AudienceId;
                                    if (!isDirectorLevelUser) isReSubmission = true;
                                }
                                if (pcpobj.GeographyId != form.GeographyId)
                                {
                                    pcpobj.GeographyId = form.GeographyId;
                                    if (!isDirectorLevelUser) isReSubmission = true;
                                }
                                //if (RedirectType) // Commented by Sohel Pathan on 08/07/2014 for PL ticket #549 to add Start and End date field in Campaign. Program and Tactic screen
                                //{

                                DateTime todaydate = DateTime.Now;

                                /// Modified by:   Dharmraj
                                /// Modified date: 2-Sep-2014
                                /// Purpose:       #625 Changing the dates on an approved tactic needs to go through the approval process
                                // To check whether status is Approved or not
                                if (Common.CheckAfterApprovedStatus(pcpobj.Status))
                                {
                                    // If any changes in start/end dates then tactic will go through the approval process
                                    if (pcpobj.EndDate != form.EndDate || pcpobj.StartDate != form.StartDate)
                                    {
                                        if (!isDirectorLevelUser) isReSubmission = true;
                                        //Comment because it already called beloe in isresubmission.PL Ticket 359.
                                        // pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                        // Common.mailSendForTactic(pcpobj.PlanTacticId, pcpobj.Status, pcpobj.Title, section: Convert.ToString(Enums.Section.Tactic).ToLower());
                                    }
                                    else
                                    {
                                        if (todaydate > form.StartDate && todaydate < form.EndDate)
                                        {
                                            pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                                        }
                                        else if (todaydate > form.EndDate)
                                        {
                                            pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                                        }
                                    }
                                }

                                pcpobj.StartDate = form.StartDate;
                                pcpobj.EndDate = form.EndDate;

                                if (form.PStartDate > form.StartDate)
                                {
                                    pcpobj.Plan_Campaign_Program.StartDate = form.StartDate;
                                }

                                if (form.EndDate > form.PEndDate)
                                {
                                    pcpobj.Plan_Campaign_Program.EndDate = form.EndDate;
                                }

                                if (form.CStartDate > form.StartDate)
                                {
                                    pcpobj.Plan_Campaign_Program.Plan_Campaign.StartDate = form.StartDate;
                                }

                                if (form.EndDate > form.CEndDate)
                                {
                                    pcpobj.Plan_Campaign_Program.Plan_Campaign.EndDate = form.EndDate;
                                }

                                //}
                                if (pcpobj.ProjectedStageValue != form.ProjectedStageValue)
                                {
                                    pcpobj.ProjectedStageValue = form.ProjectedStageValue;
                                    if (!isDirectorLevelUser) isReSubmission = true;
                                }
                                /* TFS Bug 207 : Cant override the Cost from the defaults coming out of the model
                                 * changed by Nirav shah on 10 feb 2014  
                                 */
                                //if (pcpobj.Cost != form.TacticCost)
                                //{
                                //Added By Kalpesh Sharma : #752 Update line item cost with the total cost from the monthly/quarterly allocation
                                pcpobj.Cost = UpdateBugdetAllocationCost(arrBudgetInputValues, form.TacticCost);
                                //    if (!isDirectorLevelUser) isReSubmission = true;
                                //}
                                /* TFS Bug 207 : end changes */
                                // pcpobj.Cost = form.Cost; 

                                pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                pcpobj.StageId = form.StageId;
                                pcpobj.ProjectedStageValue = form.ProjectedStageValue;

                                pcpobj.ModifiedBy = Sessions.User.UserId;
                                pcpobj.ModifiedDate = DateTime.Now;

                                ////start Added by Mitesh Vaishnav for PL ticket #571
                                ////For edit mode, remove all actual cost for tactic if tactic has no line item
                                var tacticLineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == form.PlanTacticId && l.IsDeleted == false).Select(l => l.PlanLineItemId).ToList();
                                if (tacticLineItemList.Count == 0)
                                {
                                    var PrevActualAllocationListTactics = db.Plan_Campaign_Program_Tactic_Actual.Where(c => c.PlanTacticId == form.PlanTacticId).Select(c => c).ToList();
                                    PrevActualAllocationListTactics.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                    //Insert actual cost of tactic
                                    for (int i = 0; i < arrActualCostInputValues.Length; i++)
                                    {
                                        if (arrActualCostInputValues[i] != "")
                                        {
                                            Plan_Campaign_Program_Tactic_Actual obPlanCampaignProgramTacticActual = new Plan_Campaign_Program_Tactic_Actual();
                                            obPlanCampaignProgramTacticActual.PlanTacticId = form.PlanTacticId;
                                            obPlanCampaignProgramTacticActual.StageTitle = Enums.InspectStage.Cost.ToString();
                                            obPlanCampaignProgramTacticActual.Period = "Y" + (i + 1);
                                            obPlanCampaignProgramTacticActual.Actualvalue = Convert.ToDouble(arrActualCostInputValues[i]);
                                            obPlanCampaignProgramTacticActual.CreatedBy = Sessions.User.UserId;
                                            obPlanCampaignProgramTacticActual.CreatedDate = DateTime.Now;
                                            db.Entry(obPlanCampaignProgramTacticActual).State = EntityState.Added;
                                        }
                                    }
                                }
                                ////End Added by Mitesh Vaishnav for PL ticket #571

                                //Start by Kalpesh Sharma #605: Cost allocation for Tactic
                                var PrevAllocationList = db.Plan_Campaign_Program_Tactic_Cost.Where(c => c.PlanTacticId == form.PlanTacticId).Select(c => c).ToList();  // Modified by Sohel Pathan on 04/09/2014 for PL ticket #759
                                //PrevAllocationList.ForEach(a => db.Entry(a).State = EntityState.Deleted); // Commented by Sohel Pathan on 04/09/2014 for PL ticket #759

                                if (arrBudgetInputValues.Length == 12)
                                {
                                    for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                    {
                                        // Start - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
                                        bool isExists = false;
                                        if (PrevAllocationList != null)
                                        {
                                            if (PrevAllocationList.Count > 0)
                                            {
                                                var updatePlanTacticBudget = PrevAllocationList.Where(pb => pb.Period == ("Y" + (i + 1))).FirstOrDefault();
                                                if (updatePlanTacticBudget != null)
                                                {
                                                    if (arrBudgetInputValues[i] != "")
                                                    {
                                                        var newValue = Convert.ToDouble(arrBudgetInputValues[i]);
                                                        if (updatePlanTacticBudget.Value != newValue)
                                                        {
                                                            updatePlanTacticBudget.Value = newValue;
                                                            db.Entry(updatePlanTacticBudget).State = EntityState.Modified;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        db.Entry(updatePlanTacticBudget).State = EntityState.Deleted;
                                                    }
                                                    isExists = true;
                                                }
                                            }
                                        }
                                        if (!isExists && arrBudgetInputValues[i] != "")
                                        {
                                            // End - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
                                            Plan_Campaign_Program_Tactic_Cost obPlanCampaignProgramTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                            obPlanCampaignProgramTacticCost.PlanTacticId = form.PlanTacticId;
                                            obPlanCampaignProgramTacticCost.Period = "Y" + (i + 1);
                                            obPlanCampaignProgramTacticCost.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                            obPlanCampaignProgramTacticCost.CreatedBy = Sessions.User.UserId;
                                            obPlanCampaignProgramTacticCost.CreatedDate = DateTime.Now;
                                            db.Entry(obPlanCampaignProgramTacticCost).State = EntityState.Added;
                                        }
                                    }
                                }
                                else if (arrBudgetInputValues.Length == 4)
                                {
                                    int BudgetInputValuesCounter = 1;

                                    for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                    {
                                        // Start - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
                                        bool isExists = false;
                                        if (PrevAllocationList != null)
                                        {
                                            if (PrevAllocationList.Count > 0)
                                            {
                                                //var thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == ("Y" + BudgetInputValuesCounter)).FirstOrDefault();
                                                var thisQuartersMonthList = PrevAllocationList.Where(pb => pb.Period == ("Y" + (BudgetInputValuesCounter)) || pb.Period == ("Y" + (BudgetInputValuesCounter + 1)) || pb.Period == ("Y" + (BudgetInputValuesCounter + 2))).ToList().OrderBy(a => a.Period).ToList();
                                                var thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                                if (thisQuarterFirstMonthBudget != null)
                                                {
                                                    if (arrBudgetInputValues[i] != "")
                                                    {
                                                        var thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                                        var thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                                        var newValue = Convert.ToDouble(arrBudgetInputValues[i]);

                                                        if (thisQuarterTotalBudget != newValue)
                                                        {
                                                            var BudgetDiff = newValue - thisQuarterTotalBudget;
                                                            if (BudgetDiff > 0)
                                                            {
                                                                thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                                                db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                            }
                                                            else
                                                            {
                                                                int j = 1;
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
                                                                    if ((BudgetInputValuesCounter + j) <= (BudgetInputValuesCounter + 2))
                                                                    {
                                                                        thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == ("Y" + (BudgetInputValuesCounter + j))).FirstOrDefault();
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
                                        }
                                        if (!isExists && arrBudgetInputValues[i] != "")
                                        {
                                            // End - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
                                            Plan_Campaign_Program_Tactic_Cost obPlanCampaignProgramTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                            obPlanCampaignProgramTacticCost.PlanTacticId = form.PlanTacticId;
                                            obPlanCampaignProgramTacticCost.Period = "Y" + BudgetInputValuesCounter;
                                            obPlanCampaignProgramTacticCost.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                            obPlanCampaignProgramTacticCost.CreatedBy = Sessions.User.UserId;
                                            obPlanCampaignProgramTacticCost.CreatedDate = DateTime.Now;
                                            db.Entry(obPlanCampaignProgramTacticCost).State = EntityState.Added;
                                        }
                                        BudgetInputValuesCounter = BudgetInputValuesCounter + 3;
                                    }
                                }

                                db.Entry(pcpobj).State = EntityState.Modified;
                                int result;
                                if (Common.CheckAfterApprovedStatus(pcpobj.Status))
                                {
                                    result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                }
                                if (isReSubmission && Common.CheckAfterApprovedStatus(status) && isOwner)
                                {
                                    pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                    Common.mailSendForTactic(pcpobj.PlanTacticId, pcpobj.Status, pcpobj.Title, section: Convert.ToString(Enums.Section.Tactic).ToLower());
                                }
                                result = db.SaveChanges();

                                // Start Added by dharmraj for ticket #644
                                var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == pcpobj.PlanTacticId && l.Title == Common.DefaultLineItemTitle && l.LineItemTypeId == null);
                                var objtotalLoneitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == pcpobj.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false);
                                double totalLoneitemCost = 0;
                                if (objtotalLoneitemCost != null && objtotalLoneitemCost.Count() > 0)
                                {
                                    totalLoneitemCost = objtotalLoneitemCost.Sum(l => l.Cost);
                                }

                                if (pcpobj.Cost > totalLoneitemCost)
                                {
                                    double diffCost = pcpobj.Cost - totalLoneitemCost;
                                    if (objOtherLineItem == null)
                                    {
                                        Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                        objNewLineitem.PlanTacticId = pcpobj.PlanTacticId;
                                        objNewLineitem.Title = Common.DefaultLineItemTitle;
                                        objNewLineitem.Cost = diffCost;
                                        objNewLineitem.Description = string.Empty;
                                        objNewLineitem.CreatedBy = Sessions.User.UserId;
                                        objNewLineitem.CreatedDate = DateTime.Now;
                                        db.Entry(objNewLineitem).State = EntityState.Added;
                                    }
                                    else
                                    {
                                        objOtherLineItem.IsDeleted = false;
                                        objOtherLineItem.Cost = diffCost;
                                        db.Entry(objOtherLineItem).State = EntityState.Modified;
                                    }
                                }
                                else
                                {
                                    if (objOtherLineItem != null)
                                    {
                                        objOtherLineItem.IsDeleted = true;

                                        objOtherLineItem.Cost = 0;
                                        objOtherLineItem.Description = string.Empty;
                                        db.Entry(objOtherLineItem).State = EntityState.Modified;
                                        List<Plan_Campaign_Program_Tactic_LineItem_Actual> objOtherActualCost = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                                        objOtherActualCost = objOtherLineItem.Plan_Campaign_Program_Tactic_LineItem_Actual.ToList();
                                        objOtherActualCost.ForEach(oal => db.Entry(oal).State = EntityState.Deleted);
                                    }
                                }
                                // End Added by dharmraj for ticket #644

                                ////Start Added by Mitesh Vaishnav for PL ticket #720 Custom fields for Tactics
                                //// delete previous custom field values and save modified custom fields value for particular Tactic
                                string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                                var prevCustomFieldList = db.CustomField_Entity.Where(c => c.EntityId == pcpobj.PlanTacticId && c.CustomField.EntityType == entityTypeTactic).ToList();
                                prevCustomFieldList.ForEach(c => db.Entry(c).State = EntityState.Deleted);

                                if (customFields.Count != 0)
                                {
                                    foreach (var item in customFields)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = pcpobj.PlanTacticId;
                                        objcustomFieldEntity.CustomFieldId = Convert.ToInt32(item.Key);
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;

                                    }
                                }
                                ////End Added by Mitesh Vaishnav for PL ticket #720 Custom fields for Tactics

                                db.SaveChanges();

                                //result = TacticValueCalculate(pcpobj.PlanProgramId); // Modified by Dharmraj for PL #440
                                if (result >= 1)
                                {
                                    //// Start - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                    Common.ChangeProgramStatus(pcpobj.PlanProgramId);
                                    var PlanCampaignId = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.PlanProgramId == pcpobj.PlanProgramId).Select(a => a.PlanCampaignId).Single();
                                    Common.ChangeCampaignStatus(PlanCampaignId);
                                    //// End - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425

                                    scope.Complete();
                                    if (!string.IsNullOrEmpty(CalledFromBudget))
                                    {
                                        TempData["SuccessMessage"] = "Tactic saved successfully";
                                        return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget, expand = "tactic" + form.PlanTacticId.ToString() }) });
                                    }
                                    else
                                    {
                                        if (RedirectType)
                                        {
                                            TempData["ClosedTask"] = closedTask;
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
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { });
        }

        /*Changed for TFS Bug  255:Plan Campaign screen - Add delete icon for tactic and campaign in the grid     
         * changed by : Nirav Shah on 13 feb 2014
           Add delete tactic feature
         */
        [HttpPost]
        public ActionResult DeleteTactic(int id = 0, bool RedirectType = false, string closedTask = null, string UserId = "", string CalledFromBudget = "")
        {
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
                            Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(p => p.PlanTacticId == id).SingleOrDefault();
                            cid = pcpt.Plan_Campaign_Program.PlanCampaignId;
                            pid = pcpt.PlanProgramId;
                            Title = pcpt.Title;
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcpt.PlanTacticId, pcpt.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                            if (returnValue >= 1)
                            {
                                //TacticValueCalculate(pcpt.PlanProgramId); // Modified by Dharmraj for PL #440

                                //// Start - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                Common.ChangeProgramStatus(pcpt.PlanProgramId);
                                var PlanCampaignId = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.PlanProgramId == pcpt.PlanProgramId).Select(a => a.PlanCampaignId).Single();
                                Common.ChangeCampaignStatus(PlanCampaignId);
                                //// End - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425

                                scope.Complete();


                                if (!string.IsNullOrEmpty(CalledFromBudget))
                                {
                                    TempData["SuccessMessage"] = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                                    return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget, expand = "program" + pid.ToString() }) });
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
        /// Total Stage Value Calculated & Update.
        /// Modified for PL #440 by dharmraj
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>return int.</returns>
        //public int TacticValueCalculate(int id, bool isProgram = true)
        //{
        //    try
        //    {
        //        if (isProgram)
        //        {
        //            Plan_Campaign_Program pcp = new Plan_Campaign_Program();
        //            pcp = db.Plan_Campaign_Program.Where(p => p.PlanProgramId == id).SingleOrDefault();
        //            var totalProgram = (from t in db.Plan_Campaign_Program_Tactic
        //                                where t.PlanProgramId == id && t.IsDeleted.Equals(false)
        //                                select new { t.INQs, t.Cost }).ToList();
        //            pcp.INQs = totalProgram.Sum(tp => tp.INQs);
        //            pcp.Cost = totalProgram.Sum(tp => tp.Cost);
        //            db.Entry(pcp).State = EntityState.Modified;
        //            id = pcp.PlanCampaignId;
        //        }

        //        Plan_Campaign pc = new Plan_Campaign();
        //        pc = db.Plan_Campaign.Where(p => p.PlanCampaignId == id).SingleOrDefault();
        //        var totalCampaign = (from t in db.Plan_Campaign_Program_Tactic
        //                             where t.Plan_Campaign_Program.PlanCampaignId == id && t.IsDeleted.Equals(false)
        //                             select new { t.INQs, t.Cost }).ToList();
        //        pc.INQs = totalCampaign.Sum(tp => tp.INQs);
        //        pc.Cost = totalCampaign.Sum(tp => tp.Cost);

        //        db.Entry(pc).State = EntityState.Modified;
        //        return db.SaveChanges();
        //    }
        //    catch (Exception e)
        //    {
        //        ErrorSignal.FromCurrentContext().Raise(e);
        //    }
        //    return 0;
        //}

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Tactic Type INQ,MQL,Cost,Stage Value.
        /// </summary>
        /// <param name="tacticTypeId">Tactic Type Id</param>
        /// <returns>Returns Json Result of Tactic Type. </returns>
        [HttpPost]
        public JsonResult LoadTacticTypeValue(int tacticTypeId)
        {
            TacticType tt = db.TacticTypes.Where(t => t.TacticTypeId == tacticTypeId).FirstOrDefault();
            return Json(new
            {
                revenue = tt.ProjectedRevenue == null ? 0 : tt.ProjectedRevenue,
                IsDeployedToIntegration = tt.IsDeployedToIntegration,
                stageId = tt.StageId,
                stageTitle = tt.Stage.Title,
                projectedStageValue = tt.ProjectedStageValue == null ? 0 : tt.ProjectedStageValue
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Calculate MQL Conerstion Rate based on Session Plan Id.
        /// Added by Bhavesh Dobariya.
        /// Modified By: Maninder Singh Wadhva to address TFS Bug#280 :Error Message Showing when editing a tactic - Preventing MQLs from updating
        /// Modified By: Maninder Singh Wadhva 1-March-2014 to address TFS Bug#322 : Changes made to INQ, MQL and Projected Revenue Calculation.
        /// </summary>
        /// <returns>JsonResult MQl Rate.</returns>
        public JsonResult CalculateMQL(Plan_Campaign_Program_TacticModel form, double projectedStageValue, bool RedirectType, bool isTacticTypeChange)
        {
            DateTime StartDate = new DateTime();
            string stageMQL = Enums.Stage.MQL.ToString();
            int tacticStageLevel = 0;
            int levelMQL = db.Stages.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageMQL)).Level.Value;
            if (form.PlanTacticId != 0)
            {
                if (isTacticTypeChange)
                {
                    //if (form.TacticTypeId != 0)
                    //{
                    tacticStageLevel = Convert.ToInt32(db.TacticTypes.FirstOrDefault(t => t.TacticTypeId == form.TacticTypeId).Stage.Level);
                    //}
                    //else
                    //{
                    //    if (RedirectType)
                    //    {
                    //        StartDate = form.StartDate;
                    //    }
                    //    else
                    //    {
                    //        StartDate = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanTacticId == form.PlanTacticId).Select(t => t.StartDate).SingleOrDefault();
                    //    }

                    //    int modelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
                    //    return Json(new { mql = Common.CalculateMQLTactic(projectedStageValue, StartDate, form.PlanTacticId, form.StageId, modelId) });
                    //}
                }
                else
                {
                    tacticStageLevel = Convert.ToInt32(db.Stages.FirstOrDefault(t => t.StageId == form.StageId).Level);
                }

                if (RedirectType)
                {
                    StartDate = form.StartDate;
                }
                else
                {
                    StartDate = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanTacticId == form.PlanTacticId).Select(t => t.StartDate).SingleOrDefault();
                }

                int modelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
                /// Added by Dharmraj on 4-Sep-2014
                /// #760 Advanced budgeting – show correct revenue in Tactic fly out
                List<Plan_Campaign_Program_Tactic> lstTactic = new List<Plan_Campaign_Program_Tactic>();
                Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
                objTactic.StartDate = StartDate;
                objTactic.EndDate = form.EndDate;
                objTactic.StageId = form.StageId;
                objTactic.Plan_Campaign_Program = new Plan_Campaign_Program() { Plan_Campaign = new Plan_Campaign() { PlanId = Sessions.PlanId, Plan = new Plan() { } } };
                objTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId = modelId;
                objTactic.ProjectedStageValue = projectedStageValue;
                lstTactic.Add(objTactic);
                var lstTacticStageRelation = Common.GetTacticStageRelation(lstTactic, false);
                double calculatedMQL = 0;
                double CalculatedRevenue = 0;
                if (lstTacticStageRelation.Count > 0)
                {
                    calculatedMQL = lstTacticStageRelation[0].MQLValue;
                    CalculatedRevenue = lstTacticStageRelation[0].RevenueValue;
                    CalculatedRevenue = Math.Round(CalculatedRevenue, 2); // Modified by Sohel Pathan on 16/09/2014 for PL ticket #760
                }
                else
                {
                    calculatedMQL = CalculatedRevenue = 0;
                }
                calculatedMQL = Math.Round(calculatedMQL, 0, MidpointRounding.AwayFromZero);
                if (tacticStageLevel < levelMQL)
                {
                    //if (RedirectType)
                    //{
                    //    StartDate = form.StartDate;
                    //}
                    //else
                    //{
                    //    StartDate = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanTacticId == form.PlanTacticId).Select(t => t.StartDate).SingleOrDefault();
                    //}

                    //return Json(new { mql = Common.CalculateMQLTactic(projectedStageValue, StartDate, form.PlanTacticId, form.StageId, modelId) });

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
            else
            {
                //if (form.TacticTypeId != 0)
                //{
                tacticStageLevel = Convert.ToInt32(db.TacticTypes.FirstOrDefault(t => t.TacticTypeId == form.TacticTypeId).Stage.Level);
                //}
                //else
                //{
                //    StartDate = DateTime.Now;
                //    int modelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
                //    return Json(new { mql = Common.CalculateMQLTactic(projectedStageValue, StartDate, form.PlanTacticId, form.StageId, modelId) });
                //}

                /// Added by Dharmraj on 4-Sep-2014
                /// #760 Advanced budgeting – show correct revenue in Tactic fly out
                int modelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
                List<Plan_Campaign_Program_Tactic> lstTactic = new List<Plan_Campaign_Program_Tactic>();
                Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
                if (tacticStageLevel < levelMQL)
                {
                    objTactic.StartDate = DateTime.Now;
                }
                else
                {
                    objTactic.StartDate = StartDate;
                }
                objTactic.EndDate = form.EndDate;
                objTactic.StageId = form.StageId;
                objTactic.Plan_Campaign_Program = new Plan_Campaign_Program() { Plan_Campaign = new Plan_Campaign() { PlanId = Sessions.PlanId, Plan = new Plan() { } } };
                objTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId = modelId;
                objTactic.ProjectedStageValue = projectedStageValue;
                lstTactic.Add(objTactic);
                var lstTacticStageRelation = Common.GetTacticStageRelation(lstTactic, false);
                double calculatedMQL = 0;
                double CalculatedRevenue = 0;
                if (lstTacticStageRelation.Count > 0)
                {
                    calculatedMQL = lstTacticStageRelation[0].MQLValue;
                    CalculatedRevenue = lstTacticStageRelation[0].RevenueValue;
                    CalculatedRevenue = Math.Round(CalculatedRevenue, 2); // Modified by Sohel Pathan on 16/09/2014 for PL ticket #760
                }
                else
                {
                    calculatedMQL = CalculatedRevenue = 0;
                }

                if (tacticStageLevel < levelMQL)
                {
                    //StartDate = DateTime.Now;
                    //return Json(new { mql = Common.CalculateMQLTactic(projectedStageValue, StartDate, form.PlanTacticId, form.StageId, modelId) });
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
        }

        //Added by Mitesh Vaishnav for PL ticket 619
        /// <summary>
        /// Added By Dharmraj, ticket #574
        /// Action to create line item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PartialViewResult createLine(int id = 0)
        {
            var objPlan = db.Plans.SingleOrDefault(varP => varP.PlanId == Sessions.PlanId);
            //User Custom Restriction
            List<UserCustomRestrictionModel> lstUserCustomRestriction = Common.GetUserCustomRestriction();
            // Line item Type
            var lineItemTypes = from lit in db.LineItemTypes
                                where lit.ModelId == objPlan.ModelId && lit.IsDeleted == false
                                orderby lit.Title
                                select lit;
            foreach (var item in lineItemTypes)
            {
                item.Title = HttpUtility.HtmlDecode(item.Title);
            }
            ViewBag.lineItemTypes = lineItemTypes;
            ViewBag.IsCreated = true;

            Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.FirstOrDefault(pcpobj => pcpobj.PlanTacticId.Equals(id));
            if (pcpt == null)
            {
                return null;
            }
            ViewBag.IsTacticAfterApproved = Common.CheckAfterApprovedStatus(pcpt.Status);////Added by Mitesh Vaishnav for PL ticket #571 for checking tactic has after approved status
            double totalLineItemCost = pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
            double TacticCost = pcpt.Cost;
            double diffCost = TacticCost - totalLineItemCost;
            double otherLineItemCost = diffCost < 0 ? 0 : diffCost;

            ViewBag.tacticCost = TacticCost;
            ViewBag.totalLineItemCost = totalLineItemCost;
            ViewBag.otherLineItemCost = otherLineItemCost;


            Plan_Campaign_Program_Tactic_LineItemModel pcptlm = new Plan_Campaign_Program_Tactic_LineItemModel();
            pcptlm.PlanTacticId = id;
            ViewBag.IsOwner = true;
            // Custom Restriction
            ViewBag.IsAllowCustomRestriction = true;
            ViewBag.Tactic = HttpUtility.HtmlDecode(pcpt.Title);
            ViewBag.Program = HttpUtility.HtmlDecode(pcpt.Plan_Campaign_Program.Title);
            ViewBag.Campaign = HttpUtility.HtmlDecode(pcpt.Plan_Campaign_Program.Plan_Campaign.Title);
            ViewBag.RedirectType = false;
            // To add Start and End date field in Campaign. Program and Tactic screen
            pcptlm.StartDate = GetCurrentDateBasedOnPlan();
            pcptlm.EndDate = GetCurrentDateBasedOnPlan(true);
            pcptlm.Cost = 0;
            pcptlm.AllocatedBy = objPlan.AllocatedBy;
            pcptlm.IsOtherLineItem = false;
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;

            return PartialView("LineItemAssortment", pcptlm);
        }

        /// <summary>
        /// Added By Dharmraj, ticket #574
        /// Action to edit line item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PartialViewResult EditLineItem(int id = 0, string RedirectType = "", string CalledFromBudget = "")
        {
            ViewBag.CalledFromBudget = CalledFromBudget;
            ViewBag.IsCreated = false;
            if (RedirectType == "Assortment")
            {
                ViewBag.RedirectType = false;
            }
            else
            {
                ViewBag.RedirectType = true;
            }
            var objPlan = db.Plans.SingleOrDefault(varP => varP.PlanId == Sessions.PlanId);
            //User Custom Restriction
            List<UserCustomRestrictionModel> lstUserCustomRestriction = Common.GetUserCustomRestriction();
            // Line item Type
            var lineItemTypes = from lit in db.LineItemTypes
                                where lit.ModelId == objPlan.ModelId && lit.IsDeleted == false
                                orderby lit.Title
                                select lit;
            foreach (var item in lineItemTypes)
            {
                item.Title = HttpUtility.HtmlDecode(item.Title);
            }
            ViewBag.lineItemTypes = lineItemTypes;

            Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobj => pcpobj.PlanLineItemId.Equals(id));
            if (pcptl == null)
            {
                return null;
            }

            Plan_Campaign_Program_Tactic_LineItemModel pcptlm = new Plan_Campaign_Program_Tactic_LineItemModel();
            if (Sessions.User.UserId == pcptl.CreatedBy)
            {
                ViewBag.IsOwner = true;
            }
            else
            {
                ViewBag.IsOwner = false;
            }

            //Added By : Kalpesh Sharma #697 08/26/2014
            bool isallowrestriction = Common.GetRightsForTactic(lstUserCustomRestriction, pcptl.Plan_Campaign_Program_Tactic.VerticalId, pcptl.Plan_Campaign_Program_Tactic.GeographyId);
            ViewBag.IsAllowCustomRestriction = isallowrestriction;

            if (pcptl.LineItemTypeId == null)
            {
                pcptlm.IsOtherLineItem = true;
            }
            else
            {
                pcptlm.IsOtherLineItem = false;
            }

            pcptlm.PlanTacticId = id;

            //Added By : Kalpesh Sharma #697 08/26/2014
            //ViewBag.IsOwner = true;
            // Custom Restriction
            //ViewBag.IsAllowCustomRestriction = true;
            ViewBag.IsTacticAfterApproved = Common.CheckAfterApprovedStatus(pcptl.Plan_Campaign_Program_Tactic.Status);////Added by Mitesh Vaishnav for PL ticket #571 for checking tactic has after approved status
            ViewBag.Tactic = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Title);
            ViewBag.Program = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Title);
            ViewBag.Campaign = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Title);
            ViewBag.RedirectType = false;

            double totalLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == pcptl.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
            double TacticCost = pcptl.Plan_Campaign_Program_Tactic.Cost;
            double diffCost = TacticCost - totalLineItemCost;
            double otherLineItemCost = diffCost < 0 ? 0 : diffCost;

            ViewBag.tacticCost = TacticCost;
            ViewBag.totalLineItemCost = totalLineItemCost;
            ViewBag.otherLineItemCost = otherLineItemCost;

            pcptlm.PlanLineItemId = pcptl.PlanLineItemId;
            pcptlm.PlanTacticId = pcptl.PlanTacticId;
            pcptlm.LineItemTypeId = pcptl.LineItemTypeId == null ? 0 : Convert.ToInt32(pcptl.LineItemTypeId);
            pcptlm.Title = HttpUtility.HtmlDecode(pcptl.Title);
            pcptlm.Description = HttpUtility.HtmlDecode(pcptl.Description);
            pcptlm.StartDate = Convert.ToDateTime(pcptl.StartDate);
            pcptlm.EndDate = Convert.ToDateTime(pcptl.EndDate);
            pcptlm.Cost = pcptl.Cost;
            pcptlm.AllocatedBy = objPlan.AllocatedBy;
            pcptlm.TStartDate = pcptl.Plan_Campaign_Program_Tactic.StartDate;
            pcptlm.TEndDate = pcptl.Plan_Campaign_Program_Tactic.EndDate;
            pcptlm.PStartDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.StartDate;
            pcptlm.PEndDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.EndDate;
            pcptlm.CStartDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.StartDate;
            pcptlm.CEndDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.EndDate;
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;

            return PartialView("LineItemAssortment", pcptlm);
        }

        /// <summary>
        /// Added By: Dharmraj Mangukiya.
        /// Action to Get month/Quarter wise cost Value Of line item.
        /// </summary>
        /// <param name="id">Plan line item Id.</param>
        /// <returns>Returns Json Result of line item cost allocation Value.</returns>
        public JsonResult GetCostAllocationLineItemData(int id, int tid)
        {
            try
            {
                List<string> lstMonthly = new List<string>() { "Y1", "Y2", "Y3", "Y4", "Y5", "Y6", "Y7", "Y8", "Y9", "Y10", "Y11", "Y12" };

                //Reintialize the Monthly list for Actual Allocation object 
                List<string> lstActualAllocationMonthly = lstMonthly;
                var objPlanLineItem = db.Plan_Campaign_Program_Tactic_LineItem.SingleOrDefault(c => c.PlanLineItemId == id && c.IsDeleted == false); // Modified by :- Sohel Pathan on 05/09/2014 for PL ticket #749
                var objPlan = db.Plans.SingleOrDefault(p => p.PlanId == Sessions.PlanId && p.IsDeleted == false);   // Modified by :- Sohel Pathan on 05/09/2014 for PL ticket #749
                var objPlanTactic = db.Plan_Campaign_Program_Tactic.SingleOrDefault(p => p.PlanTacticId == tid && p.IsDeleted == false);    // Modified by :- Sohel Pathan on 01/09/2014 for PL ticket #745

                var lstAllLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(c => c.PlanTacticId == tid && c.IsDeleted == false).ToList();
                var planLineItemId = lstAllLineItem.Select(c => c.PlanLineItemId);
                var lstLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(c => planLineItemId.Contains(c.PlanLineItemId)).ToList()
                                                               .Select(c => new
                                                               {
                                                                   c.PlanLineItemBudgetId,
                                                                   c.PlanLineItemId,
                                                                   c.Period,
                                                                   c.Value
                                                               }).ToList();

                var lstTacticCost = db.Plan_Campaign_Program_Tactic_Cost.Where(p => p.PlanTacticId == tid).ToList();

                double totalLoneitemCost = lstAllLineItem.Where(l => l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
                double TacticCost = objPlanTactic.Cost;
                double diffCost = TacticCost - totalLoneitemCost;
                double otherLineItemCost = diffCost < 0 ? 0 : diffCost;

                //Added By : Kalpesh Sharma #697 08/26/2014
                //Fetch the Line items actual by LineItemId and give it to the object.
                var lstActualLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(c => planLineItemId.Contains(c.PlanLineItemId)).ToList()
                                                              .Select(c => new
                                                              {
                                                                  c.PlanLineItemId,
                                                                  c.Period,
                                                                  c.Value
                                                              }).ToList();

                //Set the custom array for fecthed Line item Actual data .
                var ActualCostData = lstActualAllocationMonthly.Select(m => new
                {
                    periodTitle = m,
                    costValue = lstActualLineItemCost.SingleOrDefault(c => c.Period == m && c.PlanLineItemId == id) == null ? "" : lstActualLineItemCost.SingleOrDefault(c => c.Period == m && c.PlanLineItemId == id).Value.ToString()
                });

                // Start - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
                if (objPlan.AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString())
                {
                    List<PlanBudgetAllocationValue> lstPlanBudgetAllocationValue = new List<PlanBudgetAllocationValue>();
                    var quarterPeriods = new string[] { "Y1", "Y2", "Y3" };
                    for (int i = 0; i < 12; i++)
                    {
                        if ((i + 1) % 3 == 0)
                        {
                            PlanBudgetAllocationValue objPlanBudgetAllocationValue = new PlanBudgetAllocationValue();
                            objPlanBudgetAllocationValue.periodTitle = "Y" + (i - 1).ToString();
                            objPlanBudgetAllocationValue.costValue = lstLineItemCost.Where(a => quarterPeriods.Contains(a.Period) && a.PlanLineItemId == id).FirstOrDefault() == null ? "" : lstLineItemCost.Where(a => quarterPeriods.Contains(a.Period) && a.PlanLineItemId == id).Select(a => a.Value).Sum().ToString();
                            objPlanBudgetAllocationValue.remainingMonthlyCost = (lstTacticCost.Where(a => quarterPeriods.Contains(a.Period)).FirstOrDefault() == null ? 0 : lstTacticCost.Where(a => quarterPeriods.Contains(a.Period)).Select(a => a.Value).Sum()) - (lstLineItemCost.Where(a => quarterPeriods.Contains(a.Period)).Sum(c => c.Value));

                            /// Add into return list
                            lstPlanBudgetAllocationValue.Add(objPlanBudgetAllocationValue);

                            quarterPeriods = new string[] { "Y" + (i + 2), "Y" + (i + 3), "Y" + (i + 4) };
                        }
                    }

                    var objBudgetAllocationData = new { costData = lstPlanBudgetAllocationValue, ActualCostData = ActualCostData, otherLineItemCost = otherLineItemCost };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                // End - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
                else
                {
                    var costData = lstMonthly.Select(m => new
                    {
                        periodTitle = m,
                        costValue = lstLineItemCost.SingleOrDefault(c => c.Period == m && c.PlanLineItemId == id) == null ? "" : lstLineItemCost.SingleOrDefault(c => c.Period == m && c.PlanLineItemId == id).Value.ToString(),
                        remainingMonthlyCost = (lstTacticCost.SingleOrDefault(p => p.Period == m) == null ? 0 : lstTacticCost.SingleOrDefault(p => p.Period == m).Value) - (lstLineItemCost.Where(c => c.Period == m).Sum(c => c.Value))
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
        /// Added By: Dharmraj
        /// Action to Save Line item.
        /// </summary>
        /// <param name="form">Form object of Plan_Campaign_Program_Tactic_LineItemModel.</param>
        /// <param name="programs">Program list string array.</param>
        /// <param name="RedirectType">Redirect Type.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveLineitem(Plan_Campaign_Program_Tactic_LineItemModel form, bool RedirectType, string closedTask, string CostInputValues, string ActualInputValues, string UserId = "", string CalledFromBudget = "")
        {
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

                //Added By : Kalpesh Sharma #697 08/26/2014
                //Spilt the Actuals allocation values into the array (Comma Seprated)
                string[] arrCostInputValues = CostInputValues.Split(',');
                string[] arrActualInputValues = ActualInputValues.Split(',');

                ////Start Added by Mitesh Vaishnav for PL ticket #752 - Update line item cost with the total cost from the monthly/quarterly allocation
                //// Check when monthly planned cost of tactic is lower than total monthly planned cost of line items than update monthly planned cost of tactic
                var lineItemIds = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.PlanTacticId == form.PlanTacticId && a.IsDeleted == false && a.PlanLineItemId != form.PlanLineItemId).Select(a => a.PlanLineItemId).ToList();
                ////list of Total monthly planned cost
                var lstMonthlyLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem_Cost
                    .Where(a => lineItemIds.Contains(a.PlanLineItemId))
                    .GroupBy(a => a.Period)
                    .Select(a => new
                    {
                        Period = a.Key,
                        Cost = a.Sum(b => b.Value)
                    }).ToList();
                ////List of monthly plaaned cost of tactic
                var lstMonthlyTacticCost = db.Plan_Campaign_Program_Tactic_Cost.Where(a => a.PlanTacticId == form.PlanTacticId).ToList();
                bool isBudgetLower = true;
                double TotalNewPlannedCost = 0;
                foreach (string s in arrCostInputValues)
                {
                    if (s != null && s != "")
                    {
                        TotalNewPlannedCost += Convert.ToDouble(s);
                    }
                }
                TotalNewPlannedCost += lstMonthlyLineItemCost.Sum(a => a.Cost);
                double TotalTacticCost = lstMonthlyTacticCost.Sum(a => a.Value);
                if (TotalNewPlannedCost>=TotalTacticCost)
                {
                    isBudgetLower = false;
                }

                ////check budget allocation type e.g. month,Quarter etc
                if (arrCostInputValues.Length == 12)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (arrCostInputValues[i] != "")
                        {
                            string period = "Y" + (i + 1).ToString();
                            double monthlyTotalLineItemCost = lstMonthlyLineItemCost.Where(a => a.Period == period).FirstOrDefault() == null ? 0 : lstMonthlyLineItemCost.Where(a => a.Period == period).FirstOrDefault().Cost;
                            monthlyTotalLineItemCost = monthlyTotalLineItemCost + Convert.ToDouble(arrCostInputValues[i]);
                            double monthlyTotalTacticCost = lstMonthlyTacticCost.Where(a => a.Period == period).FirstOrDefault() == null ? 0 : lstMonthlyTacticCost.Where(a => a.Period == period).FirstOrDefault().Value;
                            if (monthlyTotalLineItemCost > monthlyTotalTacticCost || !isBudgetLower)
                            {
                                lstMonthlyTacticCost.Where(a => a.Period == period).ToList().ForEach(a => { a.Value = monthlyTotalLineItemCost; db.Entry(a).State = EntityState.Modified; });

                            }
                        }
                    }
                }
                else if (arrCostInputValues.Length == 4)
                {
                    int QuarterCnt = 1;
                    for (int i = 0; i < 4; i++)
                    {
                        if (arrCostInputValues[i] != "")
                        {
                            ////QurterList which contains list of month as per quarter. e.g. for Q1, list is Y1,Y2 and Y3
                            List<string> QuarterList = new List<string>();
                            for (int J = 0; J < 3; J++)
                            {
                                QuarterList.Add("Y" + (QuarterCnt + J).ToString());
                            }
                            //string period = "Y" + QuarterCnt.ToString();
                            double monthlyTotalLineItemCost = lstMonthlyLineItemCost.Where(a => QuarterList.Contains(a.Period)).ToList().Sum(a=>a.Cost);
                            monthlyTotalLineItemCost = monthlyTotalLineItemCost + Convert.ToDouble(arrCostInputValues[i]);
                            double monthlyTotalTacticCost = lstMonthlyTacticCost.Where(a => QuarterList.Contains(a.Period)).ToList().Sum(a => a.Value);
                            if (monthlyTotalLineItemCost > monthlyTotalTacticCost || !isBudgetLower)
                            {
                                string period = QuarterList[0].ToString();
                                double diffCost = monthlyTotalLineItemCost - monthlyTotalTacticCost;
                                if (diffCost >= 0)
                                {
                                    lstMonthlyTacticCost.Where(a => a.Period == period).ToList().ForEach(a => { a.Value = a.Value + diffCost; db.Entry(a).State = EntityState.Modified; });
                                }
                                int periodCount = 0;
                                ////If cost diffrence is lower than 0 than reduce it from quarter in series of 1st month of quarter,2nd month of quarter...
                                while (diffCost < 0)
                                {
                                    period = QuarterList[periodCount].ToString();
                                    double tacticCost = lstMonthlyTacticCost.Where(a => a.Period == period).ToList().Sum(a => a.Value);
                                    if ((diffCost + tacticCost) >= 0)
                                    {
                                        lstMonthlyTacticCost.Where(a => a.Period == period).ToList().ForEach(a => { a.Value = a.Value + diffCost; db.Entry(a).State = EntityState.Modified; });
                                    }
                                    else
                                    {
                                        lstMonthlyTacticCost.Where(a => a.Period == period).ToList().ForEach(a => { a.Value = 0; db.Entry(a).State = EntityState.Modified; });
                                    }
                                    diffCost = diffCost + tacticCost;
                                    periodCount = periodCount + 1;
                                }

                            }
                        }
                        QuarterCnt = QuarterCnt + 3;
                    }
                }


                var objTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == form.PlanTacticId);
                ////update tactic cost as per its monthly total planned cost
                objTactic.Cost = lstMonthlyTacticCost.Sum(a => a.Value);
                db.SaveChanges();

                ////End Added by Mitesh Vaishnav for PL ticket #752 - Update line item cost with the total cost from the monthly/quarterly allocation
                int cid = objTactic.Plan_Campaign_Program.PlanCampaignId;
                int pid = objTactic.PlanProgramId;
                int tid = form.PlanTacticId;

                if (form.PlanLineItemId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                           join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                                           join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                           join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                           where pc.PlanId == Sessions.PlanId && pcptl.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcptl.IsDeleted.Equals(false)
                                           && pcpt.PlanTacticId == form.PlanTacticId
                                           select pcpt).FirstOrDefault();

                            if (pcptvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                Plan_Campaign_Program_Tactic_LineItem objLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                objLineitem.PlanTacticId = form.PlanTacticId;
                                objLineitem.Title = form.Title;
                                objLineitem.LineItemTypeId = form.LineItemTypeId;
                                objLineitem.Description = form.Description;
                                //Added By Kalpesh Sharma : #752 Update line item cost with the total cost from the monthly/quarterly allocation
                                objLineitem.Cost = UpdateBugdetAllocationCost(arrCostInputValues, form.Cost);
                                //Added By :Kalpesh Sharma #890 Line Item Dates need to go away
                                objLineitem.StartDate = null;
                                objLineitem.EndDate = null;
                                objLineitem.CreatedBy = Sessions.User.UserId;
                                objLineitem.CreatedDate = DateTime.Now;
                                db.Entry(objLineitem).State = EntityState.Added;
                                int result = db.SaveChanges();
                                
                                int lineitemId = objLineitem.PlanLineItemId;

                                var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == form.PlanTacticId && l.Title == Common.DefaultLineItemTitle && l.LineItemTypeId == null);
                                double totalLoneitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == form.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
                                if (objTactic.Cost > totalLoneitemCost)
                                {
                                    double diffCost = objTactic.Cost - totalLoneitemCost;
                                    if (objOtherLineItem == null)
                                    {
                                        Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                        objNewLineitem.PlanTacticId = form.PlanTacticId;
                                        objNewLineitem.Title = Common.DefaultLineItemTitle;
                                        objNewLineitem.Cost = diffCost;
                                        objNewLineitem.Description = string.Empty;
                                        objNewLineitem.CreatedBy = Sessions.User.UserId;
                                        objNewLineitem.CreatedDate = DateTime.Now;
                                        db.Entry(objNewLineitem).State = EntityState.Added;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        objOtherLineItem.IsDeleted = false;
                                        objOtherLineItem.Cost = diffCost;
                                        db.Entry(objOtherLineItem).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }
                                else
                                {
                                    if (objOtherLineItem != null)
                                    {
                                        objOtherLineItem.IsDeleted = true;
                                        objOtherLineItem.Cost = 0;
                                        objOtherLineItem.Description = string.Empty;
                                        db.Entry(objOtherLineItem).State = EntityState.Modified;
                                        List<Plan_Campaign_Program_Tactic_LineItem_Actual> objOtherActualCost = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                                        objOtherActualCost = objOtherLineItem.Plan_Campaign_Program_Tactic_LineItem_Actual.ToList();
                                        objOtherActualCost.ForEach(oal => db.Entry(oal).State = EntityState.Deleted);
                                        db.SaveChanges();
                                    }
                                }

                                result = Common.InsertChangeLog(Sessions.PlanId, null, objLineitem.PlanLineItemId, objLineitem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                if (result >= 1)
                                {
                                    var PrevAllocationList = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(c => c.PlanLineItemId == lineitemId).Select(c => c).ToList();
                                    PrevAllocationList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                    if (arrCostInputValues.Length == 12)
                                    {
                                        for (int i = 0; i < arrCostInputValues.Length; i++)
                                        {
                                            if (arrCostInputValues[i] != "")
                                            {
                                                Plan_Campaign_Program_Tactic_LineItem_Cost objlineItemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                                objlineItemCost.PlanLineItemId = lineitemId;
                                                objlineItemCost.Period = "Y" + (i + 1);
                                                objlineItemCost.Value = Convert.ToDouble(arrCostInputValues[i]);
                                                objlineItemCost.CreatedBy = Sessions.User.UserId;
                                                objlineItemCost.CreatedDate = DateTime.Now;
                                                db.Entry(objlineItemCost).State = EntityState.Added;
                                            }
                                        }
                                    }
                                    else if (arrCostInputValues.Length == 4)
                                    {
                                        int QuarterCnt = 1;
                                        for (int i = 0; i < 4; i++)
                                        {
                                            if (arrCostInputValues[i] != "")
                                            {
                                                Plan_Campaign_Program_Tactic_LineItem_Cost objlineItemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                                objlineItemCost.PlanLineItemId = lineitemId;
                                                objlineItemCost.Period = "Y" + QuarterCnt;
                                                objlineItemCost.Value = Convert.ToDouble(arrCostInputValues[i]);
                                                objlineItemCost.CreatedBy = Sessions.User.UserId;
                                                objlineItemCost.CreatedDate = DateTime.Now;
                                                db.Entry(objlineItemCost).State = EntityState.Added;
                                            }
                                            QuarterCnt = QuarterCnt + 3;
                                        }
                                    }

                                    //Added By : Kalpesh Sharma #697 08/26/2014
                                    //Save the Actual allocation value into the Tactic_LineItem_Actual Table  
                                    for (int i = 0; i < arrActualInputValues.Length; i++)
                                    {
                                        if (arrActualInputValues[i] != "")
                                        {
                                            Plan_Campaign_Program_Tactic_LineItem_Actual objlineItemActual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                            objlineItemActual.PlanLineItemId = lineitemId;
                                            objlineItemActual.Period = "Y" + (i + 1);
                                            objlineItemActual.Value = Convert.ToDouble(arrActualInputValues[i]);
                                            objlineItemActual.CreatedBy = Sessions.User.UserId;
                                            objlineItemActual.CreatedDate = DateTime.Now;
                                            db.Entry(objlineItemActual).State = EntityState.Added;
                                        }
                                    }

                                    db.SaveChanges();

                                }
                                scope.Complete();
                                return Json(new { redirect = Url.Action("Assortment", new { campaignId = cid, programId = pid, tacticId = tid }) });
                            }
                        }
                    }
                }
                else
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                           join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                                           join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                           join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                           where pc.PlanId == Sessions.PlanId && pcptl.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcptl.PlanLineItemId.Equals(form.PlanLineItemId) && pcptl.IsDeleted.Equals(false)
                                           && pcpt.PlanTacticId == form.PlanTacticId
                                           select pcpt).FirstOrDefault();

                            if (pcptvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                Plan_Campaign_Program_Tactic_LineItem objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobjw => pcpobjw.PlanLineItemId.Equals(form.PlanLineItemId));

                                objLineitem.Description = form.Description;
                                if (!form.IsOtherLineItem)
                                {
                                    objLineitem.Title = form.Title;
                                    objLineitem.LineItemTypeId = form.LineItemTypeId;
                                    //Added By Kalpesh Sharma : #752 Update line item cost with the total cost from the monthly/quarterly allocation
                                    objLineitem.Cost = UpdateBugdetAllocationCost(arrCostInputValues, form.Cost);
                                    //objLineitem.StartDate = null;
                                    //objLineitem.EndDate = null;
                                }

                                objLineitem.ModifiedBy = Sessions.User.UserId;
                                objLineitem.ModifiedDate = DateTime.Now;
                                db.Entry(objLineitem).State = EntityState.Modified;
                                int result;

                                result = Common.InsertChangeLog(Sessions.PlanId, null, objLineitem.PlanLineItemId, objLineitem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                result = db.SaveChanges();

                                if (!form.IsOtherLineItem)
                                {
                                    var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == form.PlanTacticId && l.Title == Common.DefaultLineItemTitle && l.LineItemTypeId == null);
                                    double totalLoneitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == form.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
                                    if (objTactic.Cost > totalLoneitemCost)
                                    {
                                        double diffCost = objTactic.Cost - totalLoneitemCost;
                                        if (objOtherLineItem == null)
                                        {
                                            Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                            objNewLineitem.PlanTacticId = form.PlanTacticId;
                                            objNewLineitem.Title = Common.DefaultLineItemTitle;
                                            objNewLineitem.Cost = diffCost;
                                            objNewLineitem.Description = string.Empty;
                                            objNewLineitem.CreatedBy = Sessions.User.UserId;
                                            objNewLineitem.CreatedDate = DateTime.Now;
                                            db.Entry(objNewLineitem).State = EntityState.Added;
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            if (diffCost != objOtherLineItem.Cost)
                                            {
                                                objOtherLineItem.IsDeleted = false;
                                                objOtherLineItem.Cost = diffCost;
                                                db.Entry(objOtherLineItem).State = EntityState.Modified;
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (objOtherLineItem != null)
                                        {
                                            objOtherLineItem.IsDeleted = true;
                                            objOtherLineItem.Cost = 0;
                                            objOtherLineItem.Description = string.Empty;
                                            db.Entry(objOtherLineItem).State = EntityState.Modified;
                                            List<Plan_Campaign_Program_Tactic_LineItem_Actual> objOtherActualCost = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                                            objOtherActualCost = objOtherLineItem.Plan_Campaign_Program_Tactic_LineItem_Actual.ToList();
                                            objOtherActualCost.ForEach(oal => db.Entry(oal).State = EntityState.Deleted);
                                            db.SaveChanges();
                                        }
                                    }
                                }

                                if (result >= 1)
                                {
                                    var PrevAllocationList = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(c => c.PlanLineItemId == form.PlanLineItemId).Select(c => c).ToList();
                                    //PrevAllocationList.ForEach(a => db.Entry(a).State = EntityState.Deleted); // Commented by :- Sohel Pathan on 05/09/2014 for PL ticket #759

                                    // Added By : Kalpesh Sharma #697 08/26/2014
                                    // Fetch the Actual Allocation values by it's LineItem Id and delete all the record.
                                    var PrevActualAllocationList = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(c => c.PlanLineItemId == form.PlanLineItemId).Select(c => c).ToList();
                                    PrevActualAllocationList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                                    if (arrCostInputValues.Length == 12)
                                    {
                                        for (int i = 0; i < arrCostInputValues.Length; i++)
                                        {
                                            // Start - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
                                            bool isExists = false;
                                            if (PrevAllocationList != null)
                                            {
                                                if (PrevAllocationList.Count > 0)
                                                {
                                                    var updatePlanTacticBudget = PrevAllocationList.Where(pb => pb.Period == ("Y" + (i + 1))).FirstOrDefault();
                                                    if (updatePlanTacticBudget != null)
                                                    {
                                                        if (arrCostInputValues[i] != "")
                                                        {
                                                            var newValue = Convert.ToDouble(arrCostInputValues[i]);
                                                            if (updatePlanTacticBudget.Value != newValue)
                                                            {
                                                                updatePlanTacticBudget.Value = newValue;
                                                                db.Entry(updatePlanTacticBudget).State = EntityState.Modified;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            db.Entry(updatePlanTacticBudget).State = EntityState.Deleted;
                                                        }
                                                        isExists = true;
                                                    }
                                                }
                                            }
                                            if (!isExists && arrCostInputValues[i] != "")
                                            {
                                                // End - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
                                                Plan_Campaign_Program_Tactic_LineItem_Cost objlineItemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                                objlineItemCost.PlanLineItemId = form.PlanLineItemId;
                                                objlineItemCost.Period = "Y" + (i + 1);
                                                objlineItemCost.Value = Convert.ToDouble(arrCostInputValues[i]);
                                                objlineItemCost.CreatedBy = Sessions.User.UserId;
                                                objlineItemCost.CreatedDate = DateTime.Now;
                                                db.Entry(objlineItemCost).State = EntityState.Added;
                                            }
                                        }
                                    }
                                    else if (arrCostInputValues.Length == 4)
                                    {
                                        int QuarterCnt = 1;
                                        for (int i = 0; i < 4; i++)
                                        {
                                            // Start - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
                                            bool isExists = false;
                                            if (PrevAllocationList != null)
                                            {
                                                if (PrevAllocationList.Count > 0)
                                                {
                                                    var thisQuartersMonthList = PrevAllocationList.Where(pb => pb.Period == ("Y" + (QuarterCnt)) || pb.Period == ("Y" + (QuarterCnt + 1)) || pb.Period == ("Y" + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                                    var thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                                    if (thisQuarterFirstMonthBudget != null)
                                                    {
                                                        if (arrCostInputValues[i] != "")
                                                        {
                                                            var thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                                            var thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                                            var newValue = Convert.ToDouble(arrCostInputValues[i]);

                                                            if (thisQuarterTotalBudget != newValue)
                                                            {
                                                                var BudgetDiff = newValue - thisQuarterTotalBudget;
                                                                if (BudgetDiff > 0)
                                                                {
                                                                    thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                                                    db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                                }
                                                                else
                                                                {
                                                                    int j = 1;
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
                                                                            thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == ("Y" + (QuarterCnt + j))).FirstOrDefault();
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
                                            }
                                            if (!isExists && arrCostInputValues[i] != "")
                                            {
                                                // End - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
                                                Plan_Campaign_Program_Tactic_LineItem_Cost objlineItemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                                objlineItemCost.PlanLineItemId = form.PlanLineItemId;
                                                objlineItemCost.Period = "Y" + QuarterCnt;
                                                objlineItemCost.Value = Convert.ToDouble(arrCostInputValues[i]);
                                                objlineItemCost.CreatedBy = Sessions.User.UserId;
                                                objlineItemCost.CreatedDate = DateTime.Now;
                                                db.Entry(objlineItemCost).State = EntityState.Added;
                                            }
                                            QuarterCnt = QuarterCnt + 3;
                                        }
                                    }

                                    //Added By : Kalpesh Sharma #697 08/26/2014
                                    //Save the Actual allocation value into the Tactic_LineItem_Actual Table  
                                    for (int i = 0; i < arrActualInputValues.Length; i++)
                                    {
                                        if (arrActualInputValues[i] != "")
                                        {
                                            Plan_Campaign_Program_Tactic_LineItem_Actual objlineItemActual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                                            objlineItemActual.PlanLineItemId = form.PlanLineItemId;
                                            objlineItemActual.Period = "Y" + (i + 1);
                                            objlineItemActual.Value = Convert.ToDouble(arrActualInputValues[i]);
                                            objlineItemActual.CreatedBy = Sessions.User.UserId;
                                            objlineItemActual.CreatedDate = DateTime.Now;
                                            db.Entry(objlineItemActual).State = EntityState.Added;
                                        }
                                    }

                                    db.SaveChanges();

                                    scope.Complete();
                                    if (!string.IsNullOrEmpty(CalledFromBudget))
                                    {
                                        TempData["SuccessMessage"] = "Line item saved successfully";
                                        return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget, expand = "lineitem" + form.PlanLineItemId.ToString() }) });
                                    }
                                    else
                                    {
                                        if (RedirectType)
                                        {
                                            TempData["ClosedTask"] = closedTask;
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
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { });
        }

        /// <summary>
        /// Added By Mitesh
        /// Action to delete line item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="RedirectType"></param>
        /// <param name="closedTask"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public ActionResult DeleteLineItem(int id = 0, bool RedirectType = false, string closedTask = null, string UserId = "", string CalledFromBudget = "")
        {
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
                            Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.Where(p => p.PlanLineItemId == id).SingleOrDefault();

                            // Start added by dharmraj to handle "Other" line item
                            var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == pcptl.Plan_Campaign_Program_Tactic.PlanTacticId && l.Title == Common.DefaultLineItemTitle && l.LineItemTypeId == null);
                            double totalLoneitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == pcptl.Plan_Campaign_Program_Tactic.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
                            if (pcptl.Plan_Campaign_Program_Tactic.Cost > totalLoneitemCost)
                            {
                                double diffCost = pcptl.Plan_Campaign_Program_Tactic.Cost - totalLoneitemCost;
                                if (objOtherLineItem == null)
                                {
                                    Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                    objNewLineitem.PlanTacticId = pcptl.Plan_Campaign_Program_Tactic.PlanTacticId;
                                    objNewLineitem.Title = Common.DefaultLineItemTitle;
                                    objNewLineitem.Cost = diffCost;
                                    objNewLineitem.Description = string.Empty;
                                    objNewLineitem.CreatedBy = Sessions.User.UserId;
                                    objNewLineitem.CreatedDate = DateTime.Now;
                                    db.Entry(objNewLineitem).State = EntityState.Added;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    if (diffCost != objOtherLineItem.Cost)
                                    {
                                        objOtherLineItem.IsDeleted = false;
                                        objOtherLineItem.Cost = diffCost;
                                        db.Entry(objOtherLineItem).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                if (objOtherLineItem != null)
                                {
                                    objOtherLineItem.IsDeleted = true;
                                    objOtherLineItem.Cost = 0;
                                    objOtherLineItem.Description = string.Empty;
                                    db.Entry(objOtherLineItem).State = EntityState.Modified;
                                    List<Plan_Campaign_Program_Tactic_LineItem_Actual> objOtherActualCost = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                                    objOtherActualCost = objOtherLineItem.Plan_Campaign_Program_Tactic_LineItem_Actual.ToList();
                                    objOtherActualCost.ForEach(oal => db.Entry(oal).State = EntityState.Deleted);
                                    db.SaveChanges();
                                }
                            }
                            // End added by dharmraj to handle "Other" line item


                            cid = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId;
                            pid = pcptl.Plan_Campaign_Program_Tactic.PlanProgramId;
                            tid = pcptl.PlanTacticId;
                            Title = pcptl.Title;
                            returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcptl.PlanLineItemId, pcptl.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                            if (returnValue >= 1)
                            {
                                //TacticValueCalculate(pcpt.PlanProgramId); // Modified by Dharmraj for PL #440

                                //// Start - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                var planProgramId = pcptl.Plan_Campaign_Program_Tactic.PlanProgramId;
                                Common.ChangeProgramStatus(planProgramId);
                                var PlanCampaignId = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.PlanProgramId == pcptl.Plan_Campaign_Program_Tactic.PlanProgramId).Select(a => a.PlanCampaignId).Single();
                                Common.ChangeCampaignStatus(PlanCampaignId);
                                //// End - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425

                                scope.Complete();


                                if (!string.IsNullOrEmpty(CalledFromBudget))
                                {
                                    TempData["SuccessMessage"] = string.Format("Line Item {0} deleted successfully", Title);
                                    return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget, expand = "tactic" + tid.ToString() }) });
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

        /// <summary>
        /// Added By: Dharmraj Mangukiya.
        /// Action to Get Line Item Type.
        /// </summary>
        /// <returns>Returns JSon Result Of Line Item Type.</returns>
        public JsonResult GetLineItemType()
        {
            var objPlan = db.Plans.SingleOrDefault(varP => varP.PlanId == Sessions.PlanId);

            // Line item Type
            var lineItemTypes = from lit in db.LineItemTypes
                                where lit.ModelId == objPlan.ModelId && lit.IsDeleted == false
                                orderby lit.Title
                                select new { lit.Title, lit.LineItemTypeId };

            return Json(lineItemTypes, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region "Duplicate"

        /// <summary>
        /// Function to duplicate current plan.
        /// Added By: Maninder Singh Wadhva.
        /// Updated By : Bhavesh B Dobariya.
        /// Date: 12/19/2013
        /// </summary>
        /// <param name="planId">Plan Id to be duplicated.</param>
        /// <returns>Returns ApplyToCalendar action result.</returns>
        [HttpPost]
        public ActionResult DuplicatePlan()
        {
            try
            {
                int returnValue = DuplicateClone(Sessions.PlanId, "Plan");
                if (returnValue != 0)
                {
                    Common.InsertChangeLog(returnValue, 0, returnValue, db.Plans.Single(p => p.PlanId.Equals(returnValue)).Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.created, db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Title);
                    Sessions.PlanId = returnValue;
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { isSuccess = true, planId = Sessions.PlanId }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Function to duplicate current Campaign,Program,Tactic.
        /// Added By: Bhavesh B Dobariya.
        /// Date: 09/01/2014
        /// </summary>
        /// <param name="Id">Id to be duplicated.</param>
        /// <param name="RedirectType">Redirect type.</param>
        /// <param name="CopyClone">copy of .</param>
        /// <returns>Returns action result.</returns>
        [HttpPost]
        public ActionResult DuplicateCopyClone(int id, bool RedirectType, string CopyClone, string closedTask, string CalledFromBudget = "")
        {
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        int cid = 0;
                        int pid = 0;
                        int returnValue = DuplicateClone(id, CopyClone);
                        if (returnValue != 0)
                        {
                            if (CopyClone == "Tactic")
                            {
                                Plan_Campaign_Program_Tactic opcpt = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(returnValue)).SingleOrDefault();
                                cid = opcpt.Plan_Campaign_Program.PlanCampaignId;
                                pid = opcpt.PlanProgramId;
                                //TacticValueCalculate(opcpt.PlanProgramId); /*Added by Nirav Shah on 17 feb 2013  for Duplicate Tactic */ // Modified by Dharmraj for PL #440
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, opcpt.PlanProgramId, opcpt.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                            }
                            else if (CopyClone == "Program")
                            {
                                Plan_Campaign_Program opcp = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(returnValue)).SingleOrDefault();
                                cid = opcp.PlanCampaignId;
                                //TacticValueCalculate(opcp.PlanCampaignId, false); /*Added by Nirav Shah on 17 feb 2013  for Duplicate Program */ // Modified by Dharmraj for PL #440
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, opcp.PlanProgramId, opcp.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                            }
                            else if (CopyClone == "Campaign")
                            {
                                Plan_Campaign opc = db.Plan_Campaign.Where(pcpobjw => pcpobjw.PlanCampaignId.Equals(returnValue)).SingleOrDefault();
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, opc.PlanCampaignId, opc.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                            }

                        }
                        if (returnValue >= 1)
                        {
                            scope.Complete();
                            if (!string.IsNullOrEmpty(CalledFromBudget))
                            {
                                string expand = CopyClone.ToLower();
                                if (expand == "campaign")
                                    return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget }) });
                                else
                                    return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget, expand = expand + id.ToString() }) });
                            }
                            else
                            {
                                if (RedirectType)
                                {
                                    TempData["ClosedTask"] = closedTask;
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
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { });
        }

        /// <summary>
        /// Function to duplicate clone.
        /// Added By: Bhavesh B Dobariya.
        /// Date: 09/01/2014
        /// </summary>
        /// <param name="Id">Id to be duplicated.</param>
        /// <param name="CopyClone">copy of copyclone.</param>
        /// <returns>Returns action result.</returns>
        public int DuplicateClone(int id = 0, string CopyClone = "Plan")
        {
            int returnValue = 0;
            return returnValue;
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
            //ViewBag.IsViewOnly = "false";
            try
            {
                //if (Sessions.RolePermission != null)
                //{
                //    Common.Permission permission = Common.GetPermission(ActionItem.Pref);
                //    switch (permission)
                //    {
                //        case Common.Permission.FullAccess:
                //            break;
                //        case Common.Permission.NoAccess:
                //            return RedirectToAction("Index", "NoAccess");
                //        case Common.Permission.NotAnEntity:
                //            break;
                //        case Common.Permission.ViewOnly:
                //            ViewBag.IsViewOnly = "true";
                //            break;
                //    }
                //}

                // To get permission status for Plan create, By dharmraj PL #519
                ViewBag.IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
                // To get permission status for Plan Edit, By dharmraj PL #519
                // Modified by Dharmraj for #712 Edit Own and Subordinate Plan
                //ViewBag.IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                // To get permission status for edit own and subordinate's plans, By dharmraj PL #519
                //ViewBag.IsPlanEditOwnAndSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditOwnAndSubordinates);

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
        /// <param name="BusinessUnit"></param>
        /// <returns></returns>
        public JsonResult GetPlanSelectorData(string Year, string BusinessUnit)
        {
            //Get all subordinates of current user upto n level
            var lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
            // Get current user permission for edit own and subordinates plans.
            bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            // To get permission status for Plan Edit, By dharmraj PL #519
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);

            Guid BUId = Guid.Empty;
            if (!string.IsNullOrEmpty(BusinessUnit))
            {
                BUId = Guid.Parse(BusinessUnit);
            }
            string str_Year = Convert.ToString(!string.IsNullOrEmpty(Year) ? Convert.ToInt32(Year) : 0);
            int Int_Year = Convert.ToInt32(!string.IsNullOrEmpty(Year) ? Convert.ToInt32(Year) : 0);
            List<Plan> objPlan = new List<Plan>();
            List<Plan_Selector> lstPlanSelector = new List<Plan_Selector>();
            try
            {
                Guid clientId = Sessions.User.ClientId;

                // Get the list of plan, filtered by Business Unit and Year selected
                if (!string.IsNullOrEmpty(BusinessUnit) && Int_Year > 0)
                {
                    objPlan = (from p in db.Plans
                               join m in db.Models on p.ModelId equals m.ModelId
                               join bu in db.BusinessUnits on m.BusinessUnitId equals bu.BusinessUnitId
                               where bu.ClientId == clientId && bu.IsDeleted == false && m.IsDeleted == false &&
                               p.IsDeleted == false && p.Year == str_Year && m.BusinessUnitId.Equals(BUId)
                               select p).OrderByDescending(p => p.ModifiedDate ?? p.CreatedDate).ThenBy(p => p.Title).ToList();
                }
                List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).Select(stage => stage).ToList();
                if (objPlan != null && objPlan.Count > 0)
                {
                    foreach (var item in objPlan)
                    {
                        var LastUpdated = !string.IsNullOrEmpty(Convert.ToString(item.ModifiedDate)) ? item.ModifiedDate : item.CreatedDate;
                        Plan_Selector objPlanSelector = new Plan_Selector();
                        objPlanSelector.PlanId = item.PlanId;
                        objPlanSelector.PlanTitle = item.Title;
                        objPlanSelector.LastUpdated = LastUpdated.Value.Date.ToString("M/d/yy");
                        //objPlanSelector.MQLS = (item.MQLs).ToString("#,##0");
                        // Start - Modified by Sohel Pathan on 15/07/2014 for PL ticket #566
                        if (item.GoalType.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower())
                        {
                            objPlanSelector.MQLS = item.GoalValue.ToString("#,##0");
                        }
                        else
                        {
                            // Get ADS value
                            string marketing = Enums.Funnel.Marketing.ToString();
                            double ADSValue = db.Model_Funnel.Single(mf => mf.ModelId == item.ModelId && mf.Funnel.Title == marketing).AverageDealSize;

                            objPlanSelector.MQLS = Common.CalculateMQLOnly(item.ModelId, item.GoalType, item.GoalValue.ToString(), ADSValue, stageList).ToString("#,##0"); ;
                        }
                        // End - Modified by Sohel Pathan on 15/07/2014 for PL ticket #566
                        objPlanSelector.Budget = (item.Budget).ToString("#,##0");
                        objPlanSelector.Status = item.Status;

                        // Start - Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                        bool IsBusinessUnitEditable = Common.IsBusinessUnitEditable(BUId);
                        bool IsPlanEditable = false;

                        // Added to check edit status for current user by dharmraj for #538
                        if (IsBusinessUnitEditable)
                        {
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
                        }

                        objPlanSelector.IsPlanEditable = IsPlanEditable;

                        // End - Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units

                        lstPlanSelector.Add(objPlanSelector);
                    }
                }

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
            var objPlan = (from p in db.Plans
                           join m in db.Models on p.ModelId equals m.ModelId
                           join bu in db.BusinessUnits on m.BusinessUnitId equals bu.BusinessUnitId
                           where bu.ClientId == clientId && bu.IsDeleted == false && m.IsDeleted == false && p.IsDeleted == false
                           select p).OrderBy(q => q.Year).ToList();

            /* Modified by Sohel on 08/04/2014 for PL #424 to Show year's tab starting from left to right i.e. 2010, 2011, 2012..., Ordering has been changed.*/
            var lstYears = objPlan.OrderByDescending(p => p.Year).Select(p => p.Year).Distinct().Take(10).ToList();

            return Json(lstYears, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Method to get the list of business Unit (Role wise)
        /// </summary>
        /// <returns></returns>
        public JsonResult GetBUTab()
        {
            var lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList();
            List<Guid> lstAllowedBusinessUnitIds = new List<Guid>();
            if (lstAllowedBusinessUnits.Count > 0)
                lstAllowedBusinessUnits.ForEach(g => lstAllowedBusinessUnitIds.Add(Guid.Parse(g)));
            if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin) && lstAllowedBusinessUnitIds.Count == 0)   // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                var returnDataGuid = (db.BusinessUnits.ToList().Where(bu => bu.ClientId.Equals(Sessions.User.ClientId) && bu.IsDeleted.Equals(false)).Select(bu => bu).ToList()).Select(b => new
                {
                    id = b.BusinessUnitId,
                    title = b.Title
                }).Select(b => b).Distinct().OrderBy(b => b.title); /* Modified by Sohel on 08/04/2014 for PL #424 to Show The business unit tabs sorted in alphabetic order. */

                return Json(returnDataGuid, JsonRequestBehavior.AllowGet);
            }
            else
            // Modified by Dharmraj, For #537
            //if (!AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin))//if (Sessions.IsPlanner)
            {
                // Start - Added by Sohel Pathan on 30/06/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                if (lstAllowedBusinessUnitIds.Count > 0)
                {
                    var returnDataGuid = (db.BusinessUnits.ToList().Where(bu => lstAllowedBusinessUnitIds.Contains(bu.BusinessUnitId) && bu.IsDeleted.Equals(false)).Select(bu => bu).ToList()).Select(b => new
                {
                    id = b.BusinessUnitId,
                    title = b.Title
                }).Select(b => b).Distinct().OrderBy(b => b.title); /* Modified by Sohel on 08/04/2014 for PL #424 to Show The business unit tabs sorted in alphabetic order. */
                    return Json(returnDataGuid, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var returnDataGuid = (db.BusinessUnits.ToList().Where(bu => bu.ClientId.Equals(Sessions.User.ClientId) && bu.BusinessUnitId.Equals(Sessions.User.BusinessUnitId) && bu.IsDeleted.Equals(false)).Select(bu => bu).ToList()).Select(b => new
                    {
                        id = b.BusinessUnitId,
                        title = b.Title
                    }).Select(b => b).Distinct().OrderBy(b => b.title); /* Modified by Sohel on 08/04/2014 for PL #424 to Show The business unit tabs sorted in alphabetic order. */
                    return Json(returnDataGuid, JsonRequestBehavior.AllowGet);
                }
                // End - Added by Sohel Pathan on 30/06/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            }
        }


        /// <summary>
        /// Method to delete the plan
        /// </summary>
        /// <param name="PlanId"></param>
        /// <returns></returns>
        public JsonResult DeletePlan(int PlanId = 0)
        {
            int returnValue = 0;
            string PlanName = "";
            using (var scope = new TransactionScope())
            {
                if (PlanId > 0)
                {

                    ////Modified By Mitesh Vaishnav for PL ticket #718
                    ////modification : delete custom field values of particular plan's campaign,program and tactic
                    string entityTypeCampaign = Enums.EntityType.Campaign.ToString();
                    string entityTypeProgram = Enums.EntityType.Program.ToString();
                    string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                    List<CustomField_Entity> customFieldList = new List<CustomField_Entity>();
                    PlanName = db.Plans.Where(p => p.PlanId == PlanId && p.IsDeleted == false).FirstOrDefault().Title;
                    var tacticList = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.Plan_Campaign.PlanId == PlanId && pcpt.IsDeleted == false).ToList();
                    tacticList.ForEach(pcpt => pcpt.IsDeleted = true);
                    var tacticIds = tacticList.Select(a => a.PlanTacticId).ToList();
                    db.CustomField_Entity.Where(a => tacticIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeTactic).ToList().ForEach(a => customFieldList.Add(a));
                    customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                    customFieldList = new List<CustomField_Entity>();

                    var programList = db.Plan_Campaign_Program.Where(pcp => pcp.Plan_Campaign.PlanId == PlanId && pcp.IsDeleted == false).ToList();
                    programList.ForEach(pcp => pcp.IsDeleted = true);
                    var programIds = programList.Select(a => a.PlanProgramId).ToList();
                    db.CustomField_Entity.Where(a => programIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeProgram).ToList().ForEach(a => customFieldList.Add(a));
                    customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                    customFieldList = new List<CustomField_Entity>();

                    var campaignList= db.Plan_Campaign.Where(pc => pc.PlanId == PlanId && pc.IsDeleted == false).ToList();
                    campaignList.ForEach(pc => pc.IsDeleted = true);
                    var campaignIds = campaignList.Select(a => a.PlanCampaignId).ToList();
                    db.CustomField_Entity.Where(a => campaignIds.Contains(a.EntityId) && a.CustomField.EntityType == entityTypeCampaign).ToList().ForEach(a => customFieldList.Add(a));
                    db.Plans.Where(p => p.PlanId == PlanId && p.IsDeleted == false).ToList().ForEach(p => p.IsDeleted = true);

                    customFieldList.ForEach(a => db.Entry(a).State = EntityState.Deleted);

                    returnValue = db.SaveChanges();
                    scope.Complete();
                }
            }
            if (returnValue > 0)
                return Json(new { successmsg = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Plan.ToString()]) }, JsonRequestBehavior.AllowGet);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
            else
                return Json(new { errorMsg = string.Format(Common.objCached.PlanDeleteError, PlanName) }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Plan General Function

        /// <summary>
        /// Get Current Date Based on Plan Year.
        /// </summary>
        /// <param name="isEndDate"></param>
        /// <returns></returns>
        private DateTime GetCurrentDateBasedOnPlan(bool isEndDate = false)
        {
            string Year = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.Year).SingleOrDefault();
            DateTime CurrentDate = DateTime.Now;
            int CurrentYear = CurrentDate.Year;
            int diffYear = Convert.ToInt32(Year) - CurrentYear;
            DateTime returnDate = DateTime.Now;
            if (isEndDate)
            {
                DateTime lastEndDate = new DateTime(CurrentDate.AddYears(diffYear).Year, 12, 31);
                DateTime endDate = CurrentDate.AddYears(diffYear).AddMonths(1);
                returnDate = endDate > lastEndDate ? lastEndDate : endDate;
            }
            else
            {
                returnDate = DateTime.Now.AddYears(diffYear);
            }
            return returnDate;
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
                    var planImprovementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Single(improvementTactic => improvementTactic.ImprovementPlanTacticId.Equals(id));

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
                objPlan = db.Plan_Improvement_Campaign.Where(p => p.ImprovePlanId == Sessions.PlanId).FirstOrDefault();
                if (objPlan == null)
                {
                    // Setup default title for improvement campaign.
                    string planImprovementCampaignTitle = Common.ImprovementActivities;

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
        public JsonResult GetImprovementTactic()
        {
            var tactics = db.Plan_Improvement_Campaign_Program_Tactic.Where(pc => pc.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && pc.IsDeleted.Equals(false)).Select(pc => pc).ToList();
            var tacticobj = tactics.Select(p => new
            {
                id = p.ImprovementPlanTacticId,
                title = p.Title,
                cost = p.Cost,
                ImprovementProgramId = p.ImprovementPlanProgramId,
                isOwner = Sessions.User.UserId == p.CreatedBy ? 0 : 1,
            }).Select(p => p).Distinct().OrderBy(p => p.id);

            return Json(tacticobj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Create Improvement Tactic.
        /// </summary>
        /// <returns>Returns Partial View Of Tactic.</returns>
        public PartialViewResult CreateImprovementTactic(int id = 0)
        {
            List<int> impTacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(it => it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && it.IsDeleted == false).Select(it => it.ImprovementTacticTypeId).ToList();
            ViewBag.Tactics = from t in db.ImprovementTacticTypes
                              where t.ClientId == Sessions.User.ClientId && t.IsDeployed == true && !impTacticList.Contains(t.ImprovementTacticTypeId)
                              && t.IsDeleted == false       //// Added by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                              orderby t.Title
                              select t;
            ViewBag.IsCreated = true;


            var objPlan = db.Plans.SingleOrDefault(varP => varP.PlanId == Sessions.PlanId);
            ////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);
            PlanImprovementTactic pitm = new PlanImprovementTactic();
            pitm.ImprovementPlanProgramId = id;
            // Set today date as default for effective date.
            pitm.EffectiveDate = DateTime.Now;
            pitm.IsDeployedToIntegration = false;

            ViewBag.IsOwner = true;
            ViewBag.RedirectType = false;
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;
            return PartialView("ImprovementTactic", pitm);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Edit Improvement Tactic.
        /// </summary>
        /// <param name="id">Tactic Id.</param>
        /// <param name="RedirectType">Redirect Type</param>
        /// <returns>Returns Partial View Of Tactic.</returns>
        public PartialViewResult EditImprovementTactic(int id = 0, string RedirectType = "")
        {
            List<int> impTacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(it => it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && it.IsDeleted == false && it.ImprovementPlanTacticId != id).Select(it => it.ImprovementTacticTypeId).ToList();
            /*Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584  */
            var tactics = from t in db.ImprovementTacticTypes
                          where t.ClientId == Sessions.User.ClientId && t.IsDeployed == true && !impTacticList.Contains(t.ImprovementTacticTypeId)
                          && t.IsDeleted == false       //// Added by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                          select t;
            
            /*End: Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584  */
            ViewBag.IsCreated = false;
            if (RedirectType == "Assortment")
            {
                ViewBag.RedirectType = false;
            }
            else
            {
                ViewBag.RedirectType = true;

            }


            var objPlan = db.Plans.SingleOrDefault(varP => varP.PlanId == Sessions.PlanId);
            ////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);
            Plan_Improvement_Campaign_Program_Tactic pcpt = db.Plan_Improvement_Campaign_Program_Tactic.Where(pcptobj => pcptobj.ImprovementPlanTacticId.Equals(id)).SingleOrDefault();
            if (pcpt == null)
            {
                return null;
            }

            foreach (var item in tactics)
            {
                item.Title = HttpUtility.HtmlDecode(item.Title);
            }
            ////Added by Mitesh Vaishnav for internal review point 84
            if (!tactics.Any(a => a.ImprovementTacticTypeId == pcpt.ImprovementTacticTypeId))
            {
               var tacticTypeSpecial= from t in db.ImprovementTacticTypes
                          where t.ClientId == Sessions.User.ClientId && t.ImprovementTacticTypeId==pcpt.ImprovementTacticTypeId     
                          orderby t.Title
                          select t;
               tactics = tactics.Concat<ImprovementTacticType>(tacticTypeSpecial);
               tactics.OrderBy(a => a.Title);
                
            }
            ViewBag.Tactics = tactics;

            PlanImprovementTactic pcptm = new PlanImprovementTactic();
            pcptm.ImprovementPlanProgramId = pcpt.ImprovementPlanProgramId;
            pcptm.ImprovementPlanTacticId = pcpt.ImprovementPlanTacticId;
            pcptm.ImprovementTacticTypeId = pcpt.ImprovementTacticTypeId;
            pcptm.Title = HttpUtility.HtmlDecode(pcpt.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            pcptm.Description = HttpUtility.HtmlDecode(pcpt.Description);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            pcptm.EffectiveDate = pcpt.EffectiveDate;
            pcptm.Cost = pcpt.Cost;
            pcptm.IsDeployedToIntegration = pcpt.IsDeployedToIntegration;

            if (Sessions.User.UserId == pcpt.CreatedBy)
            {
                ViewBag.IsOwner = true;
            }
            else
            {
                ViewBag.IsOwner = false;
            }
            ViewBag.Program = HttpUtility.HtmlDecode(pcpt.Plan_Improvement_Campaign_Program.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            ViewBag.Campaign = HttpUtility.HtmlDecode(pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;
            return PartialView("ImprovementTactic", pcptm);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Improvement Tactic.
        /// </summary>
        /// <param name="form">Form object of PlanImprovementTactic.</param>
        /// <param name="RedirectType">Redirect Type.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveImprovementTactic(PlanImprovementTactic form, bool RedirectType)
        {
            try
            {
                if (form.ImprovementPlanTacticId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// Check for duplicate exits or not.
                            var pcpvar = (from pcpt in db.Plan_Improvement_Campaign_Program_Tactic
                                          where pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && pcpt.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcpt.IsDeleted.Equals(false)
                                          select pcpt).FirstOrDefault();

                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                Plan_Improvement_Campaign_Program_Tactic picpt = new Plan_Improvement_Campaign_Program_Tactic();
                                picpt.ImprovementPlanProgramId = form.ImprovementPlanProgramId;
                                picpt.Title = form.Title;
                                picpt.ImprovementTacticTypeId = form.ImprovementTacticTypeId;
                                picpt.Description = form.Description;
                                picpt.Cost = form.Cost;
                                picpt.EffectiveDate = form.EffectiveDate;
                                picpt.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString();
                                //// Get Businessunit id from model.
                                picpt.BusinessUnitId = (from m in db.Models
                                                        join p in db.Plans on m.ModelId equals p.ModelId
                                                        where p.PlanId == Sessions.PlanId
                                                        select m.BusinessUnitId).FirstOrDefault();
                                picpt.CreatedBy = Sessions.User.UserId;
                                picpt.CreatedDate = DateTime.Now;
                                picpt.IsDeployedToIntegration = form.IsDeployedToIntegration;

                                db.Entry(picpt).State = EntityState.Added;
                                int result = db.SaveChanges();

                                // Set isDeployedToIntegration in improvement program and improvement campaign
                                var objIProgram = db.Plan_Improvement_Campaign_Program.SingleOrDefault(varP => varP.ImprovementPlanProgramId == picpt.ImprovementPlanProgramId);
                                var objICampaign = db.Plan_Improvement_Campaign.SingleOrDefault(varC => varC.ImprovementPlanCampaignId == objIProgram.ImprovementPlanCampaignId);
                                if (form.IsDeployedToIntegration)
                                {
                                    objIProgram.IsDeployedToIntegration = true;
                                    db.Entry(objIProgram).State = EntityState.Modified;
                                    db.SaveChanges();

                                    objICampaign.IsDeployedToIntegration = true;
                                    db.Entry(objICampaign).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    bool flag = false;
                                    flag = objIProgram.Plan_Improvement_Campaign_Program_Tactic.Any(varT => varT.IsDeployedToIntegration == true && varT.IsDeleted == false);
                                    if (!flag)
                                    {
                                        objIProgram.IsDeployedToIntegration = false;
                                        db.Entry(objIProgram).State = EntityState.Modified;
                                        db.SaveChanges();

                                        objICampaign.IsDeployedToIntegration = false;
                                        db.Entry(objICampaign).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }

                                //// Insert change log entry.
                                result = Common.InsertChangeLog(Sessions.PlanId, null, picpt.ImprovementPlanTacticId, picpt.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                if (result >= 1)
                                {
                                    scope.Complete();
                                    return Json(new { redirect = Url.Action("Assortment") });
                                }
                            }
                        }
                    }
                }
                else
                {

                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// Check for Duplicate or not.
                            var pcpvar = (from pcpt in db.Plan_Improvement_Campaign_Program_Tactic
                                          where pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && pcpt.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcpt.ImprovementPlanTacticId.Equals(form.ImprovementPlanTacticId) && pcpt.IsDeleted.Equals(false)
                                          select pcpt).FirstOrDefault();

                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                bool isReSubmission = false;
                                //bool isDirectorLevelUser = false;
                                bool isManagerLevelUser = false;
                                string status = string.Empty;
                                //if (Sessions.IsDirector || Sessions.IsClientAdmin || Sessions.IsSystemAdmin)
                                //{
                                //    isDirectorLevelUser = true;
                                //}
                                Plan_Improvement_Campaign_Program_Tactic pcpobj = db.Plan_Improvement_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.ImprovementPlanTacticId.Equals(form.ImprovementPlanTacticId)).SingleOrDefault();

                                //If improvement tacitc modified by immediate manager then no resubmission will take place, By dharmraj, Ticket #537
                                var lstUserHierarchy = objBDSServiceClient.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
                                var lstSubordinates = lstUserHierarchy.Where(u => u.ManagerId == Sessions.User.UserId).Select(u => u.UserId).ToList();
                                if (lstSubordinates.Contains(pcpobj.CreatedBy))
                                {
                                    isManagerLevelUser = true;
                                }

                                pcpobj.Title = form.Title;
                                status = pcpobj.Status;

                                if (pcpobj.ImprovementTacticTypeId != form.ImprovementTacticTypeId)
                                {
                                    pcpobj.ImprovementTacticTypeId = form.ImprovementTacticTypeId;
                                    if (!isManagerLevelUser) isReSubmission = true;
                                }
                                pcpobj.Description = form.Description;

                                if (pcpobj.EffectiveDate != form.EffectiveDate)
                                {
                                    pcpobj.EffectiveDate = form.EffectiveDate;
                                    if (!isManagerLevelUser) isReSubmission = true;
                                }

                                if (pcpobj.Cost != form.Cost)
                                {
                                    pcpobj.Cost = form.Cost;
                                    if (!isManagerLevelUser) isReSubmission = true;
                                }

                                pcpobj.ModifiedBy = Sessions.User.UserId;
                                pcpobj.ModifiedDate = DateTime.Now;
                                pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                db.Entry(pcpobj).State = EntityState.Modified;
                                int result;
                                if (Common.CheckAfterApprovedStatus(pcpobj.Status))
                                {
                                    result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.ImprovementPlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                }
                                if (isReSubmission && Common.CheckAfterApprovedStatus(status))
                                {
                                    pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                    Common.mailSendForTactic(pcpobj.ImprovementPlanTacticId, pcpobj.Status, pcpobj.Title, section: Convert.ToString(Enums.Section.ImprovementTactic).ToLower());
                                }
                                result = db.SaveChanges();


                                // Set isDeployedToIntegration in improvement program and improvement campaign
                                var objIProgram = db.Plan_Improvement_Campaign_Program.SingleOrDefault(varP => varP.ImprovementPlanProgramId == pcpobj.ImprovementPlanProgramId);
                                var objICampaign = db.Plan_Improvement_Campaign.SingleOrDefault(varC => varC.ImprovementPlanCampaignId == objIProgram.ImprovementPlanCampaignId);
                                if (form.IsDeployedToIntegration)
                                {
                                    objIProgram.IsDeployedToIntegration = true;
                                    db.Entry(objIProgram).State = EntityState.Modified;
                                    db.SaveChanges();

                                    objICampaign.IsDeployedToIntegration = true;
                                    db.Entry(objICampaign).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    bool flag = false;
                                    flag = objIProgram.Plan_Improvement_Campaign_Program_Tactic.Any(varT => varT.IsDeployedToIntegration == true && varT.IsDeleted == false);
                                    if (!flag)
                                    {
                                        objIProgram.IsDeployedToIntegration = false;
                                        db.Entry(objIProgram).State = EntityState.Modified;
                                        db.SaveChanges();

                                        objICampaign.IsDeployedToIntegration = false;
                                        db.Entry(objICampaign).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }

                                if (result >= 1)
                                {
                                    scope.Complete();
                                    if (RedirectType)
                                    {
                                        return Json(new { redirect = Url.Action("ApplyToCalendar") });
                                    }
                                    else
                                    {
                                        return Json(new { isSaved = true, redirect = Url.Action("Assortment"), msg = "Changes saved." });
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
                        Plan_Improvement_Campaign_Program_Tactic pcpt = db.Plan_Improvement_Campaign_Program_Tactic.Where(p => p.ImprovementPlanTacticId == id).SingleOrDefault();
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

        /// <summary>
        /// Loag Improvement Stages for Tactic.
        /// Added by Bhavesh Dobariya.
        /// </summary>
        /// <returns>JsonResult.</returns>
        public JsonResult LoadImprovementStages(PlanImprovementTactic form)
        {
            int ImprovementPlanTacticId = form.ImprovementPlanTacticId;
            int ImprovementTacticTypeId = form.ImprovementTacticTypeId;
            DateTime EffectiveDate = form.EffectiveDate;

            var objImprovementTacticType = db.ImprovementTacticTypes.Where(itt => itt.ImprovementTacticTypeId == ImprovementTacticTypeId && itt.IsDeleted.Equals(false)).SingleOrDefault(); //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
            double Cost = objImprovementTacticType == null ? 0 : objImprovementTacticType.Cost;
            bool isDeployedToIntegration = objImprovementTacticType == null ? false : objImprovementTacticType.IsDeployedToIntegration;

            // Call function for calculate improvement for each Stage.
            List<ImprovementStage> ImprovementMetric = GetImprovementStages(ImprovementPlanTacticId, ImprovementTacticTypeId, EffectiveDate);
            string CR = Enums.StageType.CR.ToString();
            string SV = Enums.StageType.SV.ToString();
            string Size = Enums.StageType.Size.ToString();
            var tacticobj = ImprovementMetric.Select(p => new
            {
                MetricId = p.StageId,
                MetricCode = p.StageCode,
                MetricName = p.StageName,
                MetricType = p.StageType,
                BaseLineRate = p.BaseLineRate,
                PlanWithoutTactic = p.PlanWithoutTactic,
                PlanWithTactic = p.PlanWithTactic,
                Rank = p.StageType == CR ? 1 : (p.StageType == SV ? 2 : 3),
            }).Select(p => p).Distinct().OrderBy(p => p.Rank).ToList();

            return Json(new { data = tacticobj, cost = Cost, isDeployedToIntegration = isDeployedToIntegration }, JsonRequestBehavior.AllowGet);
        }

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
            int ModelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();

            //// Get Model id based on effective Date.
            ModelId = Common.GetModelId(EffectiveDate, ModelId);

            //// Get Funnelid for Marketing Funnel.
            string Marketing = Enums.Funnel.Marketing.ToString();
            int funnelId = db.Funnels.Where(f => f.Title == Marketing).Select(f => f.FunnelId).SingleOrDefault();

            //// Loop Execute for Each Stage/Metric.
            foreach (var im in ImprovementMetric)
            {
                //// Get Baseline value based on SatgeType.
                double modelvalue = 0;
                if (im.StageType == Enums.StageType.Size.ToString())
                {
                    modelvalue = db.Model_Funnel.Where(mf => mf.ModelId == ModelId && mf.FunnelId == funnelId).Select(mf => mf.AverageDealSize).SingleOrDefault();
                }
                else
                {
                    modelvalue = db.Model_Funnel_Stage.Where(mfs => mfs.Model_Funnel.ModelId == ModelId && mfs.Model_Funnel.FunnelId == funnelId && mfs.StageId == im.StageId && mfs.StageType == im.StageType).Select(mfs => mfs.Value).SingleOrDefault();
                    if (im.StageType == Enums.StageType.CR.ToString())
                    {
                        modelvalue = modelvalue / 100;
                    }
                }

                //// Get BestInClas value for MetricId.
                double bestInClassValue = db.BestInClasses.
                    Where(bic => bic.StageId == im.StageId && bic.StageType == im.StageType).
                    Select(bic => bic.Value).SingleOrDefault();

                //// Modified by Maninder singh wadhva for Ticket#159
                if (im.StageType == Enums.StageType.CR.ToString() && bestInClassValue != 0)
                {
                    bestInClassValue = bestInClassValue / 100;
                }

                //// Declare variable.
                int TotalCountWithTactic = 0;
                double TotalWeightWithTactic = 0;
                int TotalCountWithoutTactic = 0;
                double TotalWeightWithoutTactic = 0;

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
                    double improvementWeight = improveTacticList.Count() == 0 ? 0 : improveTacticList.Sum(itl => itl.Weight);
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
                    //// Get ImprovementTactic & its Weight based on filter criteria for MetricId & without current improvement tactic.
                    var improveTacticList = (from pit in db.Plan_Improvement_Campaign_Program_Tactic
                                             join itm in db.ImprovementTacticType_Metric on pit.ImprovementTacticTypeId equals itm.ImprovementTacticTypeId
                                             where pit.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && itm.ImprovementTacticType.IsDeployed == true
                                             && itm.StageId == im.StageId
                                             && itm.StageType == im.StageType
                                             && itm.Weight > 0 && pit.ImprovementPlanTacticId != ImprovementPlanTacticId && pit.IsDeleted == false
                                             select new { ImprovemetPlanTacticId = pit.ImprovementPlanTacticId, Weight = itm.Weight }).ToList();

                    //// Calculate Total ImprovementCount for PlanWithoutTactic
                    TotalCountWithoutTactic = improveTacticList.Count();

                    //// Calculate Total ImprovementWeight for PlanWithoutTactic
                    double improvementWeight = improveTacticList.Count() == 0 ? 0 : improveTacticList.Sum(itl => itl.Weight);
                    TotalWeightWithoutTactic = improvementWeight;

                    //// Get ImprovementTactic & its Weight based on filter criteria for MetricId with current tactic.
                    var improvementCountWithTacticList = (from pit in db.Plan_Improvement_Campaign_Program_Tactic
                                                          join itm in db.ImprovementTacticType_Metric on pit.ImprovementTacticTypeId equals itm.ImprovementTacticTypeId
                                                          where pit.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && itm.ImprovementTacticType.IsDeployed == true
                                                           && itm.StageId == im.StageId
                                                            && itm.StageType == im.StageType
                                                          && itm.Weight > 0 && pit.IsDeleted == false
                                                          select new { ImprovemetPlanTacticId = pit.ImprovementPlanTacticId, Weight = itm.Weight }).ToList();

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
        /// <returns>Returns Partial View Of Delete Improvement Tactic.</returns>
        public PartialViewResult ShowDeleteImprovementTactic(int id = 0, bool AssortmentType = false, bool RedirectType = false)
        {
            ViewBag.AssortmentType = AssortmentType;
            ViewBag.ImprovementPlanTacticId = id;
            ViewBag.RedirectType = RedirectType;
            int ImprovementTacticTypeId = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.ImprovementPlanTacticId == id).Select(t => t.ImprovementTacticTypeId).SingleOrDefault();
            DateTime EffectiveDate = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.ImprovementPlanTacticId == id).Select(t => t.EffectiveDate).SingleOrDefault();
            List<ImprovementStage> ImprovementMetric = GetImprovementStages(id, ImprovementTacticTypeId, EffectiveDate);
            string CR = Enums.StageType.CR.ToString();
            string SV = Enums.StageType.SV.ToString();
            double conversionRateHigher = ImprovementMetric.Where(im => im.StageType == CR).Select(im => im.PlanWithTactic).Sum();
            double conversionRateLower = ImprovementMetric.Where(im => im.StageType == CR).Select(im => im.PlanWithoutTactic).Sum();

            double stageVelocityHigher = ImprovementMetric.Where(im => im.StageType == SV).Select(im => im.PlanWithTactic).Sum();
            double stageVelocityLower = ImprovementMetric.Where(im => im.StageType == SV).Select(im => im.PlanWithoutTactic).Sum();

            string conversionUpDownString = string.Empty;
            string velocityUpDownString = string.Empty;
            string planNegativePositive = string.Empty;
            string Decreases = "Decreases";
            string Increases = "Increases";
            string Negative = "negatively";
            string Positive = "positively";
            double ConversionValue = conversionRateHigher - conversionRateLower;
            if (ConversionValue < 0)
            {
                conversionUpDownString = Increases;
            }
            else
            {
                conversionUpDownString = Decreases;
            }

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

            ViewBag.ConversionValue = Math.Abs(Math.Round(ConversionValue, 2));
            ViewBag.VelocityValue = Math.Abs(Math.Round(VelocityValue, 2));
            ViewBag.ConversionUpDownString = conversionUpDownString;
            ViewBag.VelocityUpDownString = velocityUpDownString;
            ViewBag.NegativePositiveString = planNegativePositive;

            int NoOfTactic = 0;
            var ListOfLessEffectiveDate = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.ImprovementPlanTacticId != id && t.EffectiveDate <= EffectiveDate && t.IsDeleted == false && t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId).ToList();
            if (ListOfLessEffectiveDate.Count() == 0)
            {
                var ListOfGreaterEffectiveDate = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.ImprovementPlanTacticId != id && t.IsDeleted == false && t.EffectiveDate >= EffectiveDate && t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId).Select(t => t).OrderBy(t => t.EffectiveDate).ToList();
                if (ListOfGreaterEffectiveDate.Count() == 0)
                {
                    NoOfTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.StartDate >= EffectiveDate && t.IsDeleted == false && t.Plan_Campaign_Program.Plan_Campaign.PlanId == Sessions.PlanId).ToList().Count();
                }
                else
                {
                    DateTime NextEffectiveDate = ListOfGreaterEffectiveDate.Min(l => l.EffectiveDate);
                    NoOfTactic = db.Plan_Campaign_Program_Tactic.Where(t => t.StartDate >= EffectiveDate && t.StartDate < NextEffectiveDate && t.IsDeleted == false && t.Plan_Campaign_Program.Plan_Campaign.PlanId == Sessions.PlanId).ToList().Count();
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
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && t.IsDeleted == false).Select(t => t).ToList();

            

            //Added By Bhavesh For Performance Issue #955
            List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
            List<StageList> stageListType = Common.GetStageList();
            int? ModelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(m => m.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(m => m.ModelId == ModelId).Select(m => m).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(m => m.ModelId).ToList());

            var improvementTacticTypeIds = improvementActivities.Select(imptype => imptype.ImprovementTacticTypeId).Distinct().ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId).Select(stage => stage).ToList();
            //End #955

            List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelationForSinglePlan(tacticList, bestInClassStageRelation, stageListType, modleStageRelationList,improvementTacticTypeMetric, improvementActivities, modelDateList,MainModelId, stageList, false);
            List<TacticStageValue> TacticDataWithImprovement = Common.GetTacticStageRelationForSinglePlan(tacticList, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, true);

            //// Calculating MQL difference.
            double? improvedMQL = TacticDataWithImprovement.Sum(t => t.MQLValue);
            double planMQL = TacticDataWithoutImprovement.Sum(t => t.MQLValue);
            double differenceMQL = Convert.ToDouble(improvedMQL) - planMQL;

            //// Calculating CW difference.
            double? improvedCW = TacticDataWithImprovement.Sum(t => t.CWValue);
            double planCW = TacticDataWithoutImprovement.Sum(t => t.CWValue);
            double differenceCW = Convert.ToDouble(improvedCW) - planCW;

            string stageTypeSV = Enums.StageType.SV.ToString();
            //double improvedSV = Common.GetCalculatedValueImproved(Sessions.PlanId, improvementActivities, stageTypeSV);
            double improvedSV = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSV);
            //double sv = Common.GetCalculatedValueImproved(Sessions.PlanId, improvementActivities, stageTypeSV, false);
            double sv = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSV, false);
            double differenceSV = Convert.ToDouble(improvedSV) - sv;

            //// Calcualting Deal size.
            string stageTypeSize = Enums.StageType.Size.ToString();
            //double improvedDealSize = Common.GetCalculatedValueImproved(Sessions.PlanId, improvementActivities, stageTypeSize);
            double improvedDealSize = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSize);
            //double averageDealSize = Common.GetCalculatedValueImproved(Sessions.PlanId, improvementActivities, stageTypeSize, false);
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
            var improvementTacticList = db.ImprovementTacticTypes.Where(itt => itt.IsDeployed == true && itt.ClientId == Sessions.User.ClientId && itt.IsDeleted.Equals(false)).ToList();       //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
            List<Plan_Campaign_Program_Tactic> marketingActivities = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(Sessions.PlanId) && t.IsDeleted == false).Select(t => t).ToList();
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && t.IsDeleted == false).Select(t => t).ToList();
            double projectedRevenueWithoutTactic = 0;
            string stageTypeSize = Enums.StageType.Size.ToString();
           // List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelation(marketingActivities, false);
           // List<TacticStageValue> TacticDataWithImprovement = Common.GetTacticStageRelation(marketingActivities, true);

            //Added By Bhavesh For Performance Issue #955
            List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
            List<StageList> stageListType = Common.GetStageList();
            int? ModelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(m => m.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(m => m.ModelId == ModelId).Select(m => m).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(m => m.ModelId).ToList());

            var improvementTacticTypeIds = improvementTacticList.Select(imptype => imptype.ImprovementTacticTypeId).ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId).Select(stage => stage).ToList();

            List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelationForSinglePlan(marketingActivities, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, false);
            List<TacticStageValue> TacticDataWithImprovement = Common.GetTacticStageRelationForSinglePlan(marketingActivities, bestInClassStageRelation, stageListType, modleStageRelationList, improvementTacticTypeMetric, improvementActivities, modelDateList, MainModelId, stageList, true);

            //// Checking whether improvement activities exist.
            if (improvementActivities.Count() > 0)
            {

                double improvedDealSize = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivities, improvementTacticTypeMetric, stageTypeSize);
                List<double> revenueList = new List<double>();
                TacticDataWithImprovement.ForEach(t => revenueList.Add(t.CWValue * improvedDealSize));
                projectedRevenueWithoutTactic = revenueList.Sum();
            }
            else
            {
                projectedRevenueWithoutTactic = TacticDataWithoutImprovement.Sum(t => t.RevenueValue);
            }

            List<SuggestedImprovementActivities> suggestedImproveentActivities = new List<SuggestedImprovementActivities>();

            foreach (var imptactic in improvementTacticList)
            {
                SuggestedImprovementActivities suggestedImprovement = new SuggestedImprovementActivities();
                List<Plan_Improvement_Campaign_Program_Tactic> improvementActivitiesWithType = new List<Plan_Improvement_Campaign_Program_Tactic>();
                improvementActivitiesWithType = (from ia in improvementActivities select ia).ToList();
                var impExits = improvementActivities.Where(ia => ia.ImprovementTacticTypeId == imptactic.ImprovementTacticTypeId).ToList();
                suggestedImprovement.isOwner = false;
                if (impExits.Count() == 0)
                {
                    Plan_Improvement_Campaign_Program_Tactic ptcpt = new Plan_Improvement_Campaign_Program_Tactic();
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
                    suggestedImprovement.ImprovementPlanTacticId = improvementActivities.Where(ia => ia.ImprovementTacticTypeId == imptactic.ImprovementTacticTypeId).Select(ia => ia.ImprovementPlanTacticId).SingleOrDefault();
                    suggestedImprovement.Cost = improvementActivities.Where(ia => ia.ImprovementTacticTypeId == imptactic.ImprovementTacticTypeId).Select(ia => ia.Cost).SingleOrDefault();
                    if (Sessions.User.UserId == improvementActivities.Where(ia => ia.ImprovementTacticTypeId == imptactic.ImprovementTacticTypeId).Select(ia => ia.CreatedBy).SingleOrDefault())
                    {
                        suggestedImprovement.isOwner = true;
                    }
                }

                List<TacticStageValue> tacticStageValueInnerList = Common.GetTacticStageValueListForSuggestedImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementTacticTypeMetric, marketingActivities, improvementActivitiesWithType, stageList);

               double improvedAverageDealSizeForProjectedRevenue = Common.GetCalculatedValueImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivitiesWithType, improvementTacticTypeMetric, stageTypeSize);
                double improvedValue = 0;

                //// Checking whether marketing and improvement activities exist.
                if (marketingActivities.Count() > 0)
                {
                    improvedValue = tacticStageValueInnerList.Sum(t => t.CWValue * improvedAverageDealSizeForProjectedRevenue);
                }
                double projectedRevenueWithoutTacticTemp = projectedRevenueWithoutTactic;
                if (suggestedImprovement.isExits)
                {
                    double tempValue = 0;
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
            //// Check for duplicate exits or not.
            var pcpvar = (from pcpt in db.Plan_Improvement_Campaign_Program_Tactic
                          where pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && pcpt.ImprovementTacticTypeId == improvementTacticTypeId && pcpt.IsDeleted.Equals(false)
                          select pcpt).FirstOrDefault();

            if (pcpvar != null)
            {
                return Json(new { errormsg = Common.objCached.SameImprovementTypeExits });
            }
            else
            {
                Plan_Improvement_Campaign_Program_Tactic picpt = new Plan_Improvement_Campaign_Program_Tactic();
                picpt.ImprovementPlanProgramId = improvementPlanProgramId;
                picpt.Title = db.ImprovementTacticTypes.Where(itactic => itactic.ImprovementTacticTypeId == improvementTacticTypeId && itactic.IsDeleted.Equals(false)).Select(itactic => itactic.Title).SingleOrDefault();     //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                picpt.ImprovementTacticTypeId = improvementTacticTypeId;
                picpt.Cost = db.ImprovementTacticTypes.Where(itactic => itactic.ImprovementTacticTypeId == improvementTacticTypeId && itactic.IsDeleted.Equals(false)).Select(itactic => itactic.Cost).SingleOrDefault();       //// Modified by :- Sohel Pathan on 20/05/2014 for PL #457 to delete a boost tactic.
                picpt.EffectiveDate = DateTime.Now.Date;
                picpt.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString();
                //// Get Businessunit id from model.
                picpt.BusinessUnitId = (from m in db.Models
                                        join p in db.Plans on m.ModelId equals p.ModelId
                                        where p.PlanId == Sessions.PlanId
                                        select m.BusinessUnitId).FirstOrDefault();
                picpt.CreatedBy = Sessions.User.UserId;
                picpt.CreatedDate = DateTime.Now;
                db.Entry(picpt).State = EntityState.Added;
                int result = db.SaveChanges();
                //// Insert change log entry.
                result = Common.InsertChangeLog(Sessions.PlanId, null, picpt.ImprovementPlanTacticId, picpt.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                if (result >= 1)
                {
                    return Json(new { redirect = Url.Action("Assortment") });
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

            List<Plan_Campaign_Program_Tactic> marketingActivities = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(Sessions.PlanId) && t.IsDeleted == false).Select(t => t).ToList();
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && t.IsDeleted == false).OrderBy(t => t.ImprovementPlanTacticId).Select(t => t).ToList();
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivitiesWithIncluded = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && t.IsDeleted == false && !plantacticids.Contains(t.ImprovementPlanTacticId)).OrderBy(t => t.ImprovementPlanTacticId).Select(t => t).ToList();

            string stageTypeSize = Enums.StageType.Size.ToString();
           // List<TacticStageValue> TacticDataWithoutImprovement = Common.GetTacticStageRelation(marketingActivities, false);
           // List<TacticStageValue> TacticDataWithImprovement = Common.GetTacticStageRelation(marketingActivities, true);

            //Added By Bhavesh For Performance Issue #955
            List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
            List<StageList> stageListType = Common.GetStageList();
            int? ModelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(m => m.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(m => m.ModelId == ModelId).Select(m => m).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(m => m.ModelId).ToList());

            var improvementTacticTypeIds = improvementActivities.Select(imptype => imptype.ImprovementTacticTypeId).Distinct().ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId).Select(stage => stage).ToList();
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
                improvedValue = TacticDataWithoutImprovement.Sum(p => p.RevenueValue);
            }

            List<SuggestedImprovementActivities> suggestedImprovementActivities = new List<SuggestedImprovementActivities>();

            foreach (var imptactic in improvementActivities)
            {
                SuggestedImprovementActivities suggestedImprovement = new SuggestedImprovementActivities();
                List<Plan_Improvement_Campaign_Program_Tactic> improvementActivitiesWithType = new List<Plan_Improvement_Campaign_Program_Tactic>();
                improvementActivitiesWithType = (from ia in improvementActivities select ia).ToList();

                bool impExits = plantacticids.Contains(imptactic.ImprovementPlanTacticId);
                if (!impExits)
                {
                    improvementActivitiesWithType = improvementActivitiesWithType.Where(sa => sa.ImprovementPlanTacticId != imptactic.ImprovementPlanTacticId && !plantacticids.Contains(sa.ImprovementPlanTacticId)).ToList();
                    suggestedImprovement.isExits = true;
                }
                else
                {
                    List<int> excludedids = new List<int>();
                    excludedids = (from p in plantacticids select p).ToList();
                    excludedids.Remove(imptactic.ImprovementPlanTacticId);
                    improvementActivitiesWithType = improvementActivitiesWithType.Where(sa => !excludedids.Contains(sa.ImprovementPlanTacticId)).ToList();
                    suggestedImprovement.isExits = false;
                }

                suggestedImprovement.ImprovementPlanTacticId = imptactic.ImprovementPlanTacticId;

                double? dealSizeInner = null;
                double improvedAverageDealSizeForProjectedRevenueInner = 0;
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

                double projectedRevenueWithoutTactic = 0;
                List<TacticStageValue> tacticStageValueInnerList = Common.GetTacticStageValueListForSuggestedImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementTacticTypeMetric, marketingActivities, improvementActivitiesWithType, stageList);

                //// Checking whether marketing and improvement activities exist.
                if (marketingActivities.Count() > 0)
                {
                    //// Getting Projected Reveneue or Closed Won improved based on marketing and improvement activities.
                    projectedRevenueWithoutTactic = tacticStageValueInnerList.Sum(t => t.CWValue * improvedAverageDealSizeForProjectedRevenueInner);
                }

                double projectedRevenueWithoutTacticTemp = improvedValue;
                double adsTempValue = adsWithTactic;
                if (!suggestedImprovement.isExits)
                {
                    double tempValue = 0;
                    tempValue = projectedRevenueWithoutTacticTemp;
                    projectedRevenueWithoutTacticTemp = projectedRevenueWithoutTactic;
                    projectedRevenueWithoutTactic = tempValue;

                    double tempADSValue = 0;
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
            List<Plan_Improvement_Campaign_Program_Tactic> mainImprovementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && t.IsDeleted == false).Select(t => t).ToList();

            //// Getting list of improvement activites.
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = mainImprovementActivities.Where(t => !plantacticids.Contains(t.ImprovementPlanTacticId)).Select(t => t).ToList();

            //Added By Bhavesh For Performance Issue #955
            List<StageRelation> bestInClassStageRelation = Common.GetBestInClassValue();
            List<StageList> stageListType = Common.GetStageList();
            int? ModelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(m => m.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(m => m.ModelId == ModelId).Select(m => m).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(m => m.ModelId).ToList());

            var improvementTacticTypeIds = mainImprovementActivities.Select(imptype => imptype.ImprovementTacticTypeId).Distinct().ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            List<Stage> stageList = db.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId).Select(stage => stage).ToList();
            //End #955

            List<TacticStageValue> tacticStageValueInnerList = Common.GetTacticStageValueListForSuggestedImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementTacticTypeMetric, marketingActivities, improvementActivities, stageList);

            double? improvedCW = null;

            //// Checking whether marketing and improvement activities exist.
            if (marketingActivities.Count() > 0)
            {
                //// Getting Projected Reveneue or Closed Won improved based on marketing and improvement activities.
                improvedCW = tacticStageValueInnerList.Sum(t => t.CWValue);
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

            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivities = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && t.IsDeleted == false).OrderBy(t => t.ImprovementPlanTacticId).Select(t => t).ToList();
            List<Plan_Improvement_Campaign_Program_Tactic> improvementActivitiesWithIncluded = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId.Equals(Sessions.PlanId) && t.IsDeleted == false && !plantacticids.Contains(t.ImprovementPlanTacticId)).OrderBy(t => t.ImprovementPlanTacticId).Select(t => t).ToList();
            string stageCR = Enums.MetricType.CR.ToString();
            string StageType = stageCR;
            if (!isConversion)
            {
                StageType = Enums.MetricType.SV.ToString();
            }

            string CW = Enums.Stage.CW.ToString();
            List<Stage> stageList = db.Stages.Where(s => s.ClientId == Sessions.User.ClientId && s.Level != null && s.Code != CW).ToList();
            //List<Metric> metricList = db.Metrics.Where(m => m.ClientId == Sessions.User.ClientId && m.MetricType == MetricType).OrderBy(m => m.Level).ToList();
            List<int> impTacticTypeIds = improvementActivities.Select(ia => ia.ImprovementTacticTypeId).Distinct().ToList();
            List<ImprovementTacticType_Metric> improvedMetrcList = db.ImprovementTacticType_Metric.Where(im => impTacticTypeIds.Contains(im.ImprovementTacticTypeId)).ToList();

            List<SuggestedImprovementActivitiesConversion> suggestedImprovementActivitiesConversion = new List<SuggestedImprovementActivitiesConversion>();
            foreach (var imptactic in improvementActivities)
            {
                SuggestedImprovementActivitiesConversion sIconversion = new SuggestedImprovementActivitiesConversion();
                sIconversion.ImprovementPlanTacticId = imptactic.ImprovementPlanTacticId;
                sIconversion.ImprovementTacticTypeId = imptactic.ImprovementTacticTypeId;
                sIconversion.ImprovementTacticTypeTitle = imptactic.ImprovementTacticType.Title;
                sIconversion.Cost = imptactic.Cost;
                List<ImprovedMetricWeight> imList = new List<ImprovedMetricWeight>();
                foreach (var m in stageList)
                {
                    ImprovedMetricWeight im = new ImprovedMetricWeight();
                    im.MetricId = m.StageId;
                    im.Level = m.Level;
                    var weightValue = improvedMetrcList.Where(iml => iml.ImprovementTacticTypeId == imptactic.ImprovementTacticTypeId
                        && iml.StageId == m.StageId
                        && iml.StageType == StageType
                        ).ToList();
                    if (weightValue.Count > 0)
                    {
                        im.Value = weightValue.Sum(imp => imp.Weight);
                    }
                    else
                    {
                        im.Value = 0;
                    }
                    imList.Add(im);
                }
                sIconversion.ImprovedMetricsWeight = imList;

                bool impExits = plantacticids.Contains(imptactic.ImprovementPlanTacticId);
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
            int? ModelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).SingleOrDefault();
            List<ModelDateList> modelDateList = new List<ModelDateList>();
            var ModelList = db.Models.Where(m => m.IsDeleted == false);
            int MainModelId = (int)ModelId;
            while (ModelId != null)
            {
                var model = ModelList.Where(m => m.ModelId == ModelId).Select(m => m).FirstOrDefault();
                modelDateList.Add(new ModelDateList { ModelId = model.ModelId, ParentModelId = model.ParentModelId, EffectiveDate = model.EffectiveDate });
                ModelId = model.ParentModelId;
            }

            List<ModelStageRelationList> modleStageRelationList = Common.GetModelStageRelation(modelDateList.Select(m => m.ModelId).ToList());

            var improvementTacticTypeIds = improvementActivities.Select(imptype => imptype.ImprovementTacticTypeId).Distinct().ToList();
            List<ImprovementTacticType_Metric> improvementTacticTypeMetric = db.ImprovementTacticType_Metric.Where(imptype => improvementTacticTypeIds.Contains(imptype.ImprovementTacticTypeId) && imptype.ImprovementTacticType.IsDeployed).Select(imptype => imptype).ToList();
            //End #955

            List<StageRelation> stageRelationList = Common.CalculateStageValueForSuggestedImprovement(bestInClassStageRelation, stageListType, modelDateList, MainModelId, modleStageRelationList, improvementActivitiesWithIncluded, improvementTacticTypeMetric, true);
            List<int> finalMetricList = stageList.Select(m => m.StageId).ToList();

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
                Level = stageList.Single(s => s.StageId == him.StageId).Level
            }).OrderBy(him => him.Level);

            return Json(new { data = datalist, datametriclist = dataMetricList, datafinalmetriclist = dataFinalMetricList }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Delete Improvement Tactic on click of Save & Continue
        /// Added by Bhavesh
        /// Pl Ticket 289,377,378
        /// </summary>
        /// <param name="SuggestionIMPTacticIdList"></param>
        /// <returns></returns>
        public JsonResult DeleteSuggestedBoxImprovementTactic(string SuggestionIMPTacticIdList, string UserId = "")
        {
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
                            Plan_Improvement_Campaign_Program_Tactic pcpt = db.Plan_Improvement_Campaign_Program_Tactic.Where(p => p.ImprovementPlanTacticId == pid).SingleOrDefault();
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
                                for (int i = 0; i < 12; i++)
                                {
                                    bool isExists = false;
                                    if (PrevPlanBudgetAllocationList != null)
                                    {
                                        if (PrevPlanBudgetAllocationList.Count > 0)
                                        {
                                            var updatePlanBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == ("Y" + (i + 1))).FirstOrDefault();
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
                                                    var newValue = Convert.ToInt64(inputValues[i]);
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
                                    }
                                    if (!isExists && inputValues[i] != "")  // Modified by Sohel Pathan on 02/09/2014 for Internal Review Point
                                    {
                                        Plan_Budget objPlan_Budget = new Plan_Budget();

                                        objPlan_Budget.PlanId = planId;
                                        objPlan_Budget.Period = "Y" + (i + 1).ToString();
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
                                int QuarterCnt = 1;
                                for (int i = 0; i < inputValues.Length; i++)
                                {
                                    bool isExists = false;
                                    if (PrevPlanBudgetAllocationList != null)
                                    {
                                        if (PrevPlanBudgetAllocationList.Count > 0)
                                        {
                                            var thisQuartersMonthList = PrevPlanBudgetAllocationList.Where(pb => pb.Period == ("Y" + (QuarterCnt)) || pb.Period == ("Y" + (QuarterCnt + 1)) || pb.Period == ("Y" + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                            var thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                            if (thisQuarterFirstMonthBudget != null)
                                            {
                                                if (inputValues[i] != "")
                                                {
                                                    var thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                                    var thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                                    var newValue = Convert.ToDouble(inputValues[i]);

                                                    if (thisQuarterTotalBudget != newValue)
                                                    {
                                                        var BudgetDiff = newValue - thisQuarterTotalBudget;
                                                        if (BudgetDiff > 0)
                                                        {
                                                            thisQuarterFirstMonthBudget.Value = thisQuarterFirstMonthBudget.Value + BudgetDiff;
                                                            db.Entry(thisQuarterFirstMonthBudget).State = EntityState.Modified;
                                                            isDBSaveChanges = true;
                                                        }
                                                        else
                                                        {
                                                            int j = 1;
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
                                                                    thisQuarterFirstMonthBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == ("Y" + (QuarterCnt + j))).FirstOrDefault();
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
                                    if (!isExists && inputValues[i] != "")  // Modified by Sohel Pathan on 04/09/2014 for PL ticket #642
                                    {
                                        Plan_Budget objPlan_Budget = new Plan_Budget();
                                        objPlan_Budget.PlanId = planId;
                                        objPlan_Budget.Period = "Y" + (QuarterCnt).ToString();
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
            return Json(new { id = retVal, redirect = Url.Action("Assortment", new { ismsg = "Plan Saved Successfully." }) }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Plan Budget Allocation
        /// <summary>
        /// Get plan bedget allocation by planId
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>22/07/2014</CreatedDate>
        /// <param name="planId"></param>
        /// <returns></returns>
        public JsonResult GetAllocatedBudgetForPlan(int planId, string allocatedBy)
        {
            try
            {
                if (planId != 0)
                {
                    var monthPeriods = new string[] { "Y1", "Y2", "Y3", "Y4", "Y5", "Y6", "Y7", "Y8", "Y9", "Y10", "Y11", "Y12" };
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
                        var quarterPeriods = new string[] { "Y1", "Y2", "Y3" };
                        for (int i = 0; i < 12; i++)
                        {
                            if ((i + 1) % 3 == 0)
                            {
                                PlanBudgetAllocationValue objPlanBudgetAllocationValue = new PlanBudgetAllocationValue();
                                objPlanBudgetAllocationValue.periodTitle = "Y" + (i - 1).ToString();
                                objPlanBudgetAllocationValue.budgetValue = planBudgetAllocationList.Where(a => quarterPeriods.Contains(a.Period)).FirstOrDefault() == null ? "" : planBudgetAllocationList.Where(a => quarterPeriods.Contains(a.Period)).Select(a => a.Value).Sum().ToString();
                                objPlanBudgetAllocationValue.campaignMonthlyBudget = planCampaignBudgetAllocationList.Where(pcb => quarterPeriods.Contains(pcb.Period)).Sum(pcb => pcb.Value);

                                /// Add into return list
                                lstPlanBudgetAllocationValue.Add(objPlanBudgetAllocationValue);

                                quarterPeriods = new string[] { "Y" + (i + 2), "Y" + (i + 3), "Y" + (i + 4) };
                            }
                        }

                        return Json(new { status = 1, planBudgetAllocationList = lstPlanBudgetAllocationValue }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var returnPlanBudgetList = monthPeriods.Select(m => new
                        {
                            periodTitle = m,
                            budgetValue = planBudgetAllocationList.Where(pb => pb.Period == m).FirstOrDefault() == null ? "" : planBudgetAllocationList.Where(pb => pb.Period == m).Select(pb => pb.Value).FirstOrDefault().ToString(),
                            campaignMonthlyBudget = planCampaignBudgetAllocationList.Where(pcb => pcb.Period == m).Sum(pcb => pcb.Value)
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
                List<string> lstMonthly = new List<string>() { "Y1", "Y2", "Y3", "Y4", "Y5", "Y6", "Y7", "Y8", "Y9", "Y10", "Y11", "Y12" };
                var objPlan = db.Plans.SingleOrDefault(p => p.PlanId == id && p.IsDeleted == false);    // Modified by Sohel Pathan on 04/09/2014 for PL ticket #642

                var lstPlanBudget = db.Plan_Budget.Where(pb => pb.PlanId == id)
                                                               .Select(pb => new
                                                               {
                                                                   pb.PlanBudgetId,
                                                                   pb.PlanId,
                                                                   pb.Period,
                                                                   pb.Value
                                                               }).ToList();
                var lstAllCampaign = db.Plan_Campaign.Where(c => c.PlanId == id && c.IsDeleted == false).ToList();
                var planCampaignId = lstAllCampaign.Select(c => c.PlanCampaignId);
                var lstCampaignBudget = db.Plan_Campaign_Budget.Where(c => planCampaignId.Contains(c.PlanCampaignId)).ToList()
                                                               .Select(c => new
                                                               {
                                                                   c.PlanCampaignBudgetId,
                                                                   c.PlanCampaignId,
                                                                   c.Period,
                                                                   c.Value
                                                               }).ToList();

                double allCampaignBudget = lstAllCampaign.Sum(c => c.CampaignBudget);
                double planBudget = objPlan.Budget;
                double planRemainingBudget = planBudget - allCampaignBudget;

                // Start - Added by Sohel Pathan on 27/08/2014 for PL ticket #642
                if (objPlan.AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString())
                {
                    List<PlanBudgetAllocationValue> lstPlanBudgetAllocationValue = new List<PlanBudgetAllocationValue>();
                    var quarterPeriods = new string[] { "Y1", "Y2", "Y3" };
                    for (int i = 0; i < 12; i++)
                    {
                        if ((i + 1) % 3 == 0)
                        {
                            PlanBudgetAllocationValue objPlanBudgetAllocationValue = new PlanBudgetAllocationValue();
                            objPlanBudgetAllocationValue.periodTitle = "Y" + (i - 1).ToString();
                            objPlanBudgetAllocationValue.budgetValue = lstPlanBudget.Where(a => quarterPeriods.Contains(a.Period) && a.PlanId == id).FirstOrDefault() == null ? "" : lstPlanBudget.Where(a => quarterPeriods.Contains(a.Period) && a.PlanId == id).Select(a => a.Value).Sum().ToString();
                            objPlanBudgetAllocationValue.campaignMonthlyBudget = lstCampaignBudget.Where(a => quarterPeriods.Contains(a.Period)).Select(a => a.Value).Sum();

                            /// Add into return list
                            lstPlanBudgetAllocationValue.Add(objPlanBudgetAllocationValue);

                            quarterPeriods = new string[] { "Y" + (i + 2), "Y" + (i + 3), "Y" + (i + 4) };
                        }
                    }

                    var objBudgetAllocationData = new { budgetData = lstPlanBudgetAllocationValue, planRemainingBudget = planRemainingBudget };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                // End - Added by Sohel Pathan on 27/08/2014 for PL ticket #642
                else
                {
                    var budgetData = lstMonthly.Select(m => new
                    {
                        periodTitle = m,
                        budgetValue = lstPlanBudget.SingleOrDefault(pb => pb.Period == m && pb.PlanId == id) == null ? "" : lstPlanBudget.SingleOrDefault(pb => pb.Period == m && pb.PlanId == id).Value.ToString(),
                        campaignMonthlyBudget = lstCampaignBudget.Where(c => c.Period == m).Sum(c => c.Value)
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
        #endregion

        #endregion

        public JsonResult GetBudgetAllocationProgrmaData(int CampaignId, int PlanProgramId)
        {
            try
            {
                List<string> lstMonthly = new List<string>() { "Y1", "Y2", "Y3", "Y4", "Y5", "Y6", "Y7", "Y8", "Y9", "Y10", "Y11", "Y12" };
                var objPlanCampaign = db.Plan_Campaign.SingleOrDefault(c => c.PlanCampaignId == CampaignId && c.IsDeleted == false);    // Modified by Sohel Pathan on 04/09/2014 for PL ticket #758

                var objPlan = db.Plans.SingleOrDefault(p => p.PlanId == Sessions.PlanId && p.IsDeleted == false);   // Modified by Sohel Pathan on 04/09/2014 for PL ticket #758

                var lstSelectedProgram = db.Plan_Campaign_Program.Where(p => p.PlanCampaignId == CampaignId && p.IsDeleted == false).ToList();

                var planProgramIds = lstSelectedProgram.Select(c => c.PlanProgramId);

                var lstCampaignBudget = db.Plan_Campaign_Budget.Where(c => c.PlanCampaignId == CampaignId).ToList()
                                                               .Select(c => new
                                                               {
                                                                   c.PlanCampaignBudgetId,
                                                                   c.PlanCampaignId,
                                                                   c.Period,
                                                                   c.Value
                                                               }).ToList();

                var lstProgramBudget = db.Plan_Campaign_Program_Budget.Where(c => planProgramIds.Contains(c.PlanProgramId)).ToList()
                                                               .Select(c => new
                                                               {
                                                                   c.PlanProgramBudgetId,
                                                                   c.PlanProgramId,
                                                                   c.Period,
                                                                   c.Value
                                                               }).ToList();

                var lstPlanProgramTactics = db.Plan_Campaign_Program_Tactic.Where(c => c.PlanProgramId == PlanProgramId && c.IsDeleted == false).Select(c => c.PlanTacticId).ToList();  // Modified by :- Sohel Pathan on 01/09/2014 for PL ticket #745

                var lstTacticsBudget = db.Plan_Campaign_Program_Tactic_Cost.Where(c => lstPlanProgramTactics.Contains(c.PlanTacticId)).ToList()
                                                               .Select(c => new
                                                               {
                                                                   c.PlanTacticBudgetId,
                                                                   c.PlanTacticId,
                                                                   c.Period,
                                                                   c.Value
                                                               }).ToList();

                double allCampaignBudget = lstCampaignBudget.Sum(c => c.Value);
                double allProgramBudget = lstSelectedProgram.Sum(c => c.ProgramBudget);
                double planRemainingBudget = (objPlanCampaign.CampaignBudget - allProgramBudget);

                if (objPlan.AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString())
                {
                    List<PlanBudgetAllocationValue> lstPlanBudgetAllocationValue = new List<PlanBudgetAllocationValue>();
                    var quarterPeriods = new string[] { "Y1", "Y2", "Y3" };
                    for (int i = 0; i < 12; i++)
                    {
                        if ((i + 1) % 3 == 0)
                        {
                            PlanBudgetAllocationValue objPlanBudgetAllocationValue = new PlanBudgetAllocationValue();
                            objPlanBudgetAllocationValue.periodTitle = "Y" + (i - 1).ToString();
                            objPlanBudgetAllocationValue.budgetValue = lstProgramBudget.Where(c => quarterPeriods.Contains(c.Period) && c.PlanProgramId == PlanProgramId).FirstOrDefault() == null ? "" : lstProgramBudget.Where(c => quarterPeriods.Contains(c.Period) && c.PlanProgramId == PlanProgramId).Select(a => a.Value).Sum().ToString();
                            objPlanBudgetAllocationValue.remainingMonthlyBudget = lstCampaignBudget.Where(p => quarterPeriods.Contains(p.Period)).FirstOrDefault() == null ? 0 : lstCampaignBudget.Where(p => quarterPeriods.Contains(p.Period)).Sum(a => a.Value) - (lstProgramBudget.Where(c => quarterPeriods.Contains(c.Period)).Sum(c => c.Value));
                            objPlanBudgetAllocationValue.programMonthlyBudget = lstTacticsBudget.Where(c => quarterPeriods.Contains(c.Period)).Sum(c => c.Value);

                            /// Add into return list
                            lstPlanBudgetAllocationValue.Add(objPlanBudgetAllocationValue);

                            quarterPeriods = new string[] { "Y" + (i + 2), "Y" + (i + 3), "Y" + (i + 4) };
                        }
                    }

                    var objBudgetAllocationData = new { budgetData = lstPlanBudgetAllocationValue, planRemainingBudget = planRemainingBudget };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var budgetData = lstMonthly.Select(m => new
                    {
                        periodTitle = m,
                        budgetValue = lstProgramBudget.SingleOrDefault(c => c.Period == m && c.PlanProgramId == PlanProgramId) == null ? "" : lstProgramBudget.SingleOrDefault(c => c.Period == m && c.PlanProgramId == PlanProgramId).Value.ToString(),
                        remainingMonthlyBudget = (lstCampaignBudget.SingleOrDefault(p => p.Period == m) == null ? 0 : lstCampaignBudget.SingleOrDefault(p => p.Period == m).Value) - (lstProgramBudget.Where(c => c.Period == m).Sum(c => c.Value)),
                        programMonthlyBudget = lstTacticsBudget.Where(c => c.Period == m).Sum(c => c.Value)
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
                List<string> lstMonthly = new List<string>() { "Y1", "Y2", "Y3", "Y4", "Y5", "Y6", "Y7", "Y8", "Y9", "Y10", "Y11", "Y12" };

                ////// start-Added by Mitesh Vaishnav for PL ticket #571
                //// Actual cost portion added exact under "lstMonthly" array because Actual cost portion is independent from the monthly/quarterly selection made by the user at the plan level.
                var tacticLineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == PlanTacticId && l.IsDeleted == false).Select(l => l.PlanLineItemId).ToList();
                bool isLineItemForTactic = false;////flag for line items count of tactic.If tactic has any line item than flag set to true else false
                List<Plan_Campaign_Program_Tactic_LineItem_Actual> actualCostAllocationData = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                if (tacticLineItemList.Count == 0)
                {
                    ////object for filling input of Actual Cost Allocation
                    string costTitle=Enums.InspectStage.Cost.ToString();
                    actualCostAllocationData = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => ta.PlanTacticId == PlanTacticId && ta.StageTitle==costTitle).ToList().Select(ta => new Plan_Campaign_Program_Tactic_LineItem_Actual
                      {
                          PlanLineItemId = 0,
                          Period = ta.Period,
                          Value = ta.Actualvalue
                      }).ToList();
                }
                else
                {
                    var actualLineItem = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(al => tacticLineItemList.Contains(al.PlanLineItemId)).ToList();
                    ////object for filling input of Actual Cost Allocation
                    actualCostAllocationData = lstMonthly.Select(m => new Plan_Campaign_Program_Tactic_LineItem_Actual
                        {
                            PlanLineItemId = 0,
                            Period = m,
                            Value = actualLineItem.Where(al => al.Period == m).Sum(al => al.Value)
                        }).ToList();
                    isLineItemForTactic = true;
                }
                //// End-Added by Mitesh Vaishnav for PL ticket #571

                var objPlan = db.Plans.SingleOrDefault(p => p.PlanId == Sessions.PlanId && p.IsDeleted == false);   // Modified by Sohel Pathan on 04/09/2014 for PL ticket #759

                var lstSelectedProgram = db.Plan_Campaign_Program.Where(p => p.PlanProgramId == PlanProgramId && p.IsDeleted == false).ToList();

                var planProgramIds = lstSelectedProgram.Select(c => c.PlanProgramId);

                var lstProgramBudget = db.Plan_Campaign_Program_Budget.Where(c => planProgramIds.Contains(c.PlanProgramId)).ToList()
                                                               .Select(c => new
                                                               {
                                                                   c.PlanProgramBudgetId,
                                                                   c.PlanProgramId,
                                                                   c.Period,
                                                                   c.Value
                                                               }).ToList();

                var lstPlanProgramTactics = db.Plan_Campaign_Program_Tactic.Where(c => c.PlanProgramId == PlanProgramId && c.IsDeleted == false).Select(c => c.PlanTacticId).ToList();  // Modified by :- Sohel Pathan on 01/09/2014 for PL ticket #745

                var CostTacticsBudget = db.Plan_Campaign_Program_Tactic.Where(c => c.PlanProgramId == PlanProgramId && c.IsDeleted == false).ToList().Sum(c => c.Cost); // Modified by :- Sohel Pathan on 01/09/2014 for PL ticket #745

                var lstTacticsBudget = db.Plan_Campaign_Program_Tactic_Cost.Where(c => lstPlanProgramTactics.Contains(c.PlanTacticId)).ToList()
                                                               .Select(c => new
                                                               {
                                                                   c.PlanTacticBudgetId,
                                                                   c.PlanTacticId,
                                                                   c.Period,
                                                                   c.Value
                                                               }).ToList();

                var lstPlanProgramTacticsLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(c => c.PlanTacticId == PlanTacticId && c.IsDeleted == false).Select(c => c.PlanLineItemId).ToList(); // Modified by :- Sohel Pathan on 01/09/2014 for PL ticket #745
                var lstTacticsLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(c => lstPlanProgramTacticsLineItem.Contains(c.PlanLineItemId)).ToList()
                                                               .Select(c => new
                                                               {
                                                                   c.PlanLineItemBudgetId,
                                                                   c.PlanLineItemId,
                                                                   c.Period,
                                                                   c.Value
                                                               }).ToList();

                var objPlanCampaignProgram = db.Plan_Campaign_Program.SingleOrDefault(p => p.PlanProgramId == PlanProgramId && p.IsDeleted == false);   // Modified by :- Sohel Pathan on 01/09/2014 for PL ticket #745
                double allProgramBudget = lstSelectedProgram.Sum(c => c.ProgramBudget);
                double planRemainingBudget = (objPlanCampaignProgram.ProgramBudget - (!string.IsNullOrEmpty(Convert.ToString(CostTacticsBudget)) ? CostTacticsBudget : 0));

                // Start - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
                if (objPlan.AllocatedBy == Enums.PlanAllocatedBy.quarters.ToString())
                {
                    List<PlanBudgetAllocationValue> lstPlanBudgetAllocationValue = new List<PlanBudgetAllocationValue>();
                    var quarterPeriods = new string[] { "Y1", "Y2", "Y3" };
                    for (int i = 0; i < 12; i++)
                    {
                        if ((i + 1) % 3 == 0)
                        {
                            PlanBudgetAllocationValue objPlanBudgetAllocationValue = new PlanBudgetAllocationValue();
                            objPlanBudgetAllocationValue.periodTitle = "Y" + (i - 1).ToString();
                            objPlanBudgetAllocationValue.budgetValue = lstTacticsBudget.Where(a => quarterPeriods.Contains(a.Period) && a.PlanTacticId == PlanTacticId).FirstOrDefault() == null ? "" : lstTacticsBudget.Where(a => quarterPeriods.Contains(a.Period) && a.PlanTacticId == PlanTacticId).Select(a => a.Value).Sum().ToString();
                            objPlanBudgetAllocationValue.remainingMonthlyBudget = (lstProgramBudget.Where(a => quarterPeriods.Contains(a.Period)).FirstOrDefault() == null ? 0 : lstProgramBudget.Where(a => quarterPeriods.Contains(a.Period)).Select(a => a.Value).Sum()) - (lstTacticsBudget.Where(a => quarterPeriods.Contains(a.Period)).Sum(c => c.Value));
                            objPlanBudgetAllocationValue.programMonthlyBudget = lstTacticsLineItemCost.Where(a => quarterPeriods.Contains(a.Period)).Select(a => a.Value).Sum();

                            /// Add into return list
                            lstPlanBudgetAllocationValue.Add(objPlanBudgetAllocationValue);

                            quarterPeriods = new string[] { "Y" + (i + 2), "Y" + (i + 3), "Y" + (i + 4) };
                        }
                    }

                    var objBudgetAllocationData = new { budgetData = lstPlanBudgetAllocationValue, planRemainingBudget = planRemainingBudget, actualCostData = actualCostAllocationData, IsLineItemForTactic = isLineItemForTactic };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // End - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
                    var budgetData = lstMonthly.Select(m => new
                    {
                        periodTitle = m,
                        budgetValue = lstTacticsBudget.SingleOrDefault(c => c.Period == m && c.PlanTacticId == PlanTacticId) == null ? "" : lstTacticsBudget.SingleOrDefault(c => c.Period == m && c.PlanTacticId == PlanTacticId).Value.ToString(),
                        remainingMonthlyBudget = (lstProgramBudget.SingleOrDefault(p => p.Period == m) == null ? 0 : lstProgramBudget.SingleOrDefault(p => p.Period == m).Value) - (lstTacticsBudget.Where(c => c.Period == m).Sum(c => c.Value)),
                        programMonthlyBudget = lstTacticsLineItemCost.Where(c => c.Period == m).Sum(c => c.Value)

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

        #region Budget Review Section

        #region Constant variable declaration those needs to move to common
        public const string Jan = "Y1";
        public const string Feb = "Y2";
        public const string Mar = "Y3";
        public const string Apr = "Y4";
        public const string May = "Y5";
        public const string Jun = "Y6";
        public const string Jul = "Y7";
        public const string Aug = "Y8";
        public const string Sep = "Y9";
        public const string Oct = "Y10";
        public const string Nov = "Y11";
        public const string Dec = "Y12";

        public enum BudgetTab
        {
            Allocated = 0,
            Planned = 1,
            Actual = 2
        }
        public enum ViewBy
        {
            Campaign = 0,
            Audiance = 1,
            Geography = 2,
            Vertical = 3
        }
        #endregion

        /// <summary>
        /// View fro the initial render page 
        /// </summary>
        /// <returns></returns>
        public ActionResult Budgeting()
        {
            ViewBag.ActiveMenu = Enums.ActiveMenu.Plan;
            try
            {
                ViewBag.IsPlanCreateAuthorized = true; // AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
                // Start - Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                bool IsBusinessUnitEditable = Common.IsBusinessUnitEditable(db.Plans.Where(a => a.PlanId == Sessions.PlanId).Select(a => a.Model.BusinessUnitId).FirstOrDefault());
                if (!IsBusinessUnitEditable)
                    return AuthorizeUserAttribute.RedirectToNoAccess();

                ViewBag.PlanId = Sessions.PlanId;

                #region Business Unit Ids
                List<Guid> businessUnitIds = new List<Guid>();
                var lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList();
                if (lstAllowedBusinessUnits.Count > 0)
                    lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));
                if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin) && lstAllowedBusinessUnits.Count == 0)
                {
                    var clientBusinessUnit = db.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId) && b.IsDeleted == false).Select(b => b.BusinessUnitId).ToList<Guid>();
                    businessUnitIds = clientBusinessUnit.ToList();
                    ViewBag.BusinessUnitIds = Common.GetBussinessUnitIds(Sessions.User.ClientId);
                    if (businessUnitIds.Count > 1)
                        ViewBag.showBid = true;
                    else
                        ViewBag.showBid = false;
                }
                else
                {
                    if (lstAllowedBusinessUnits.Count > 0)
                    {
                        lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));
                        var lstClientBusinessUnits = Common.GetBussinessUnitIds(Sessions.User.ClientId);
                        ViewBag.BusinessUnitIds = lstClientBusinessUnits.Where(a => businessUnitIds.Contains(Guid.Parse(a.Value)));
                        if (businessUnitIds.Count > 1)
                            ViewBag.showBid = true;
                        else
                            ViewBag.showBid = false;
                    }
                    else
                    {
                        businessUnitIds.Add(Sessions.User.BusinessUnitId);
                        ViewBag.BusinessUnitIds = Sessions.User.BusinessUnitId;//Added by Nirav for Custom Dropdown - 388
                        ViewBag.showBid = false;
                    }
                }
                #endregion

                string audLabel = Common.CustomLabelFor(Enums.CustomLabelCode.Audience);

                List<ViewByModel> lstViewBy = new List<ViewByModel>();
                lstViewBy.Add(new ViewByModel { Text = "Campaigns", Value = "0" });
                lstViewBy.Add(new ViewByModel { Text = audLabel, Value = "1" });
                lstViewBy.Add(new ViewByModel { Text = "Geography", Value = "2" });
                lstViewBy.Add(new ViewByModel { Text = "Vertical", Value = "3" });
                ViewBag.ViewBy = lstViewBy;
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return View();

        }

        /// <summary>
        /// model for the allocated tab
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="budgetTab"></param>
        /// <returns></returns>
        public ActionResult GetAllocatedBugetData(int PlanId)
        {
            //PlanId = 334;
            List<UserCustomRestrictionModel> lstUserCustomRestriction = Common.GetUserCustomRestriction();
            var campaign = db.Plan_Campaign.Where(pc => pc.PlanId.Equals(PlanId) && pc.IsDeleted.Equals(false)).Select(pc => pc).ToList();
            var campaignobj = campaign.Select(p => new
            {
                id = p.PlanCampaignId,
                title = p.Title,
                description = p.Description,
                isOwner = Sessions.User.UserId == p.CreatedBy ? 0 : 1,
                Budgeted = p.CampaignBudget,
                Budget = p.Plan_Campaign_Budget.Select(b1 => new BudgetedValue { Period = b1.Period, Value = b1.Value }).ToList(),
                programs = (db.Plan_Campaign_Program.Where(pcp => pcp.PlanCampaignId.Equals(p.PlanCampaignId) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp).ToList()).Select(pcpj => new
                {
                    id = pcpj.PlanProgramId,
                    title = pcpj.Title,
                    description = pcpj.Description,
                    Budgeted = pcpj.ProgramBudget,
                    Budget = pcpj.Plan_Campaign_Program_Budget.Select(b1 => new BudgetedValue { Period = b1.Period, Value = b1.Value }).ToList(),
                    isOwner = Sessions.User.UserId == pcpj.CreatedBy ? 0 : 1

                }).Select(pcpj => pcpj).Distinct().OrderBy(pcpj => pcpj.id)
            }).Select(p => p).Distinct().OrderBy(p => p.id);

            var lstCampaignTmp = campaignobj.Select(c => new
            {
                id = c.id,
                title = c.title,
                description = c.description,
                isOwner = c.isOwner,
                Budgeted = c.Budgeted,
                Budget = c.Budget,
                programs = c.programs.Select(p => new
                {
                    id = p.id,
                    title = p.title,
                    description = p.description,
                    Budgeted = p.Budgeted,
                    Budget = p.Budget,
                    isOwner = p.isOwner
                })
            });

            var lstCampaign = lstCampaignTmp.Select(c => new
            {
                id = c.id,
                title = c.title,
                description = c.description,
                Budget = c.Budget,
                Budgeted = c.Budgeted,
                isOwner = c.isOwner == 0 ? (c.programs.Any(p => p.isOwner == 1) ? 1 : 0) : 1,
                programs = c.programs
            });

            string AllocatedBy = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();
            List<BudgetModel> model = new List<BudgetModel>();
            BudgetModel obj;
            Plan objPlan = db.Plans.Single(p => p.PlanId.Equals(PlanId));
            string parentPlanId = "0", parentCampaignId = "0";
            if (objPlan != null)
            {
                AllocatedBy = objPlan.AllocatedBy;
                List<BudgetedValue> lst = (from bv in objPlan.Plan_Budget
                                           select new BudgetedValue
                                           {
                                               Period = bv.Period,
                                               Value = bv.Value
                                           }).ToList();
                obj = new BudgetModel();
                obj.ActivityId = objPlan.PlanId.ToString();
                obj.ActivityName = objPlan.Title;
                obj.ActivityType = ActivityType.ActivityPlan;
                obj.ParentActivityId = "0";
                obj.Budgeted = objPlan.Budget;
                obj.IsOwner = true;
                obj = GetMonthWiseData(obj, lst);
                model.Add(obj);
                parentPlanId = objPlan.PlanId.ToString();
                foreach (var c in lstCampaign)
                {
                    obj = new BudgetModel();
                    obj.ActivityId = c.id.ToString();
                    obj.ActivityName = c.title;
                    obj.ActivityType = ActivityType.ActivityCampaign;
                    obj.ParentActivityId = parentPlanId.ToString();
                    obj.IsOwner = Convert.ToBoolean(c.isOwner);
                    obj = GetMonthWiseData(obj, c.Budget);
                    obj.Budgeted = c.Budgeted;
                    model.Add(obj);
                    parentCampaignId = c.id.ToString();
                    foreach (var p in c.programs)
                    {
                        obj = new BudgetModel();
                        obj.ActivityId = p.id.ToString();
                        obj.ActivityName = p.title;
                        obj.ActivityType = ActivityType.ActivityProgram;
                        obj.ParentActivityId = parentCampaignId;
                        obj.IsOwner = Convert.ToBoolean(p.isOwner);
                        obj = GetMonthWiseData(obj, p.Budget);
                        obj.Budgeted = p.Budgeted;
                        model.Add(obj);
                    }
                }
            }

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
            a = model.Where(m => m.ActivityType == ActivityType.ActivityPlan).Select(m => m.Month).SingleOrDefault();
            //b = model.Where(m => m.ActivityType == ActivityPlan).Select(m => m.ParentMonth).SingleOrDefault();
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
            BudgetMonth month = new BudgetMonth();
            month.Jan = lst.Where(v => v.Period.ToUpper() == Jan).Select(v => v.Value).SingleOrDefault();
            month.Feb = lst.Where(v => v.Period.ToUpper() == Feb).Select(v => v.Value).SingleOrDefault();
            month.Mar = lst.Where(v => v.Period.ToUpper() == Mar).Select(v => v.Value).SingleOrDefault();
            month.Apr = lst.Where(v => v.Period.ToUpper() == Apr).Select(v => v.Value).SingleOrDefault();
            month.May = lst.Where(v => v.Period.ToUpper() == May).Select(v => v.Value).SingleOrDefault();
            month.Jun = lst.Where(v => v.Period.ToUpper() == Jun).Select(v => v.Value).SingleOrDefault();
            month.Jul = lst.Where(v => v.Period.ToUpper() == Jul).Select(v => v.Value).SingleOrDefault();
            month.Aug = lst.Where(v => v.Period.ToUpper() == Aug).Select(v => v.Value).SingleOrDefault();
            month.Sep = lst.Where(v => v.Period.ToUpper() == Sep).Select(v => v.Value).SingleOrDefault();
            month.Oct = lst.Where(v => v.Period.ToUpper() == Oct).Select(v => v.Value).SingleOrDefault();
            month.Nov = lst.Where(v => v.Period.ToUpper() == Nov).Select(v => v.Value).SingleOrDefault();
            month.Dec = lst.Where(v => v.Period.ToUpper() == Dec).Select(v => v.Value).SingleOrDefault();
            obj.Month = month;

            obj.Allocated = lst.Sum(v => v.Value);

            return obj;
        }

        /// <summary>
        /// set other data for model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<BudgetModel> SetupValues(List<BudgetModel> model)
        {
            BudgetModel plan = model.Where(p => p.ActivityType == ActivityType.ActivityPlan).SingleOrDefault();
            if (plan != null)
            {
                plan.ParentMonth = new BudgetMonth();
                plan.Allocated = plan.Budgeted;
                plan.SumMonth = plan.Month;
                List<BudgetModel> campaigns = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == plan.ActivityId).ToList();
                BudgetMonth SumCampaign = new BudgetMonth();
                foreach (BudgetModel c in campaigns)
                {
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
                    }
                }
                //finding remaining
                campaigns = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == plan.ActivityId).ToList();
                SumCampaign = new BudgetMonth();
                foreach (BudgetModel c in campaigns)
                {

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
        public ActionResult BudgetPlanList(string Bid)
        {
            HomePlan objHomePlan = new HomePlan();
            //objHomePlan.IsDirector = Sessions.IsDirector;
            List<SelectListItem> planList;
            if (Bid == "false")
            {
                planList = Common.GetPlan().Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).ToList();
                if (planList.Count > 0)
                {
                    var objexists = planList.Where(p => p.Value == Sessions.PlanId.ToString()).ToList();
                    if (objexists.Count != 0)
                    {
                        planList.Single(p => p.Value.Equals(Sessions.PlanId.ToString())).Selected = true;
                    }
                    /*changed by Nirav for plan consistency on 14 apr 2014*/
                    Sessions.BusinessUnitId = Common.GetPlan().Where(m => m.PlanId == Sessions.PlanId).Select(m => m.Model.BusinessUnitId).FirstOrDefault();
                    if (!Common.IsPlanPublished(Sessions.PlanId))
                    {
                        string planPublishedStatus = Enums.PlanStatus.Published.ToString();
                        var activeplan = db.Plans.Where(p => p.PlanId == Sessions.PlanId && p.IsDeleted == false && p.Status == planPublishedStatus).ToList();
                        if (activeplan.Count > 0)
                        {
                            Sessions.PublishedPlanId = Sessions.PlanId;
                        }
                        else
                        {
                            Sessions.PublishedPlanId = 0;
                        }
                    }
                }
            }
            else
            {
                /*changed by Nirav for plan consistency on 14 apr 2014*/
                Guid bId = new Guid(Bid);
                if (Sessions.BusinessUnitId == bId)
                {
                    bId = Common.GetPlan().Where(m => m.PlanId == Sessions.PlanId).Select(m => m.Model.BusinessUnitId).FirstOrDefault();
                }
                Sessions.BusinessUnitId = bId;
                planList = Common.GetPlan().Where(s => s.Model.BusinessUnitId == bId).Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).ToList();
                if (planList.Count > 0)
                {
                    var objexists = planList.Where(p => p.Value == Sessions.PlanId.ToString()).ToList();
                    if (objexists.Count != 0)
                    {
                        planList.Single(p => p.Value.Equals(Sessions.PlanId.ToString())).Selected = true;
                    }
                    else
                    {
                        planList.FirstOrDefault().Selected = true;
                        int planID = 0;
                        int.TryParse(planList.Select(s => s.Value).FirstOrDefault(), out planID);
                        Sessions.PlanId = planID;
                        if (!Common.IsPlanPublished(Sessions.PlanId))
                        {
                            string planPublishedStatus = Enums.PlanStatus.Published.ToString();
                            var activeplan = db.Plans.Where(p => p.PlanId == Sessions.PlanId && p.IsDeleted == false && p.Status == planPublishedStatus).ToList();
                            if (activeplan.Count > 0)
                            {
                                Sessions.PublishedPlanId = planID;
                            }
                            else
                            {
                                Sessions.PublishedPlanId = 0;
                            }
                        }
                    }
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
        /// <returns></returns>
        public ActionResult GetBudgetedData(int PlanId, BudgetTab budgetTab = BudgetTab.Planned, ViewBy viewBy = ViewBy.Campaign)
        {
            TempData["ViewBy"] = (int)viewBy;
            //PlanId = 443;
            List<UserCustomRestrictionModel> lstUserCustomRestriction = Common.GetUserCustomRestriction();
            var campaign = db.Plan_Campaign.Where(pc => pc.PlanId.Equals(PlanId) && pc.IsDeleted.Equals(false)).Select(pc => pc).ToList();
            var campaignobj = campaign.Select(p => new
            {
                id = "c_" + p.PlanCampaignId.ToString(),
                title = p.Title,
                description = p.Description,
                isOwner = Sessions.User.UserId == p.CreatedBy ? 0 : 1,
                Budget = p.Plan_Campaign_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(),
                programs = (db.Plan_Campaign_Program.Where(pcp => pcp.PlanCampaignId.Equals(p.PlanCampaignId) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp).ToList()).Select(pcpj => new
                {
                    id = "cp_" + pcpj.PlanProgramId.ToString(),
                    title = pcpj.Title,
                    description = pcpj.Description,
                    isOwner = Sessions.User.UserId == pcpj.CreatedBy ? 0 : 1,
                    Budget = pcpj.Plan_Campaign_Program_Budget.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(),
                    tactic = (db.Plan_Campaign_Program_Tactic.Where(pcp => pcp.PlanProgramId.Equals(pcpj.PlanProgramId) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp).ToList()).Select(pctj => new
                    {
                        id = "cpt_" + pctj.PlanTacticId.ToString(),
                        title = pctj.Title,
                        description = pctj.Description,
                        isOwner = Sessions.User.UserId == pctj.CreatedBy ? 0 : 1,
                        Cost = pctj.Cost,
                        Budget = budgetTab == BudgetTab.Planned ? pctj.Plan_Campaign_Program_Tactic_Cost.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList()
                                                                  : pctj.Plan_Campaign_Program_Tactic_Actual.Where(b => b.StageTitle == "Cost").Select(b => new BudgetedValue { Period = b.Period, Value = b.Actualvalue }).ToList(),
                        AudianceId = pctj.AudienceId,
                        GeographyId = pctj.GeographyId,
                        AudianceName = pctj.Audience.Title,
                        GeographyName = pctj.Geography.Title,
                        VerticalId = pctj.VerticalId,
                        VerticalName = pctj.Vertical.Title,
                        lineitems = (db.Plan_Campaign_Program_Tactic_LineItem.Where(pcp => pcp.PlanTacticId.Equals(pctj.PlanTacticId) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp).ToList()).Select(pclj => new
                        {
                            id = "cptl_" + pclj.PlanLineItemId.ToString(),
                            title = pclj.Title,
                            description = pclj.Description,
                            isOwner = Sessions.User.UserId == pclj.CreatedBy ? 0 : 1,
                            Cost = pclj.Cost,
                            Budget = budgetTab == BudgetTab.Planned ? pclj.Plan_Campaign_Program_Tactic_LineItem_Cost.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList()
                                                                     : pclj.Plan_Campaign_Program_Tactic_LineItem_Actual.Select(b => new BudgetedValue { Period = b.Period, Value = b.Value }).ToList(),
                        }).Select(pclj => pclj).Distinct().OrderBy(pclj => pclj.id)
                    }).Select(pctj => pctj).Distinct().OrderBy(pctj => pctj.id)
                }).Select(pcpj => pcpj).Distinct().OrderBy(pcpj => pcpj.id)
            }).Select(p => p).Distinct().OrderBy(p => p.id);

            string AllocatedBy = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();
            List<BudgetModel> model = new List<BudgetModel>();
            BudgetModel obj;
            Plan objPlan = db.Plans.Single(p => p.PlanId.Equals(PlanId));
            string parentPlanId = "0", parentCampaignId = "0", parentProgramId = "0", parentTacticId = "0", parentAudId = "0";
            if (objPlan != null)
            {
                AllocatedBy = objPlan.AllocatedBy;
                List<BudgetedValue> lst = (from bv in objPlan.Plan_Budget
                                           select new BudgetedValue
                                           {
                                               Period = bv.Period,
                                               Value = bv.Value
                                           }).ToList();
                obj = new BudgetModel();
                obj.Id = objPlan.PlanId.ToString();
                obj.ActivityId = "plan_" + objPlan.PlanId.ToString();
                obj.ActivityName = objPlan.Title;
                obj.ActivityType = ActivityType.ActivityPlan;
                obj.ParentActivityId = "0";
                obj.Budgeted = objPlan.Budget;
                obj.IsOwner = true;
                obj = GetMonthWiseData(obj, lst);
                model.Add(obj);
                parentPlanId = "plan_" + objPlan.PlanId.ToString();
                foreach (var c in campaignobj)
                {
                    obj = new BudgetModel();
                    obj.Id = c.id.Replace("c_", "");
                    obj.ActivityId = c.id;
                    obj.ActivityName = c.title;
                    obj.ActivityType = ActivityType.ActivityCampaign;
                    obj.ParentActivityId = parentPlanId;
                    obj.IsOwner = Convert.ToBoolean(c.isOwner);
                    obj = GetMonthWiseData(obj, c.Budget);
                    model.Add(obj);
                    parentCampaignId = c.id;
                    foreach (var p in c.programs)
                    {
                        obj = new BudgetModel();
                        obj.Id = p.id.Replace("cp_", "");
                        obj.ActivityId = p.id;
                        obj.ActivityName = p.title;
                        obj.ActivityType = ActivityType.ActivityProgram;
                        obj.ParentActivityId = parentCampaignId;
                        obj.IsOwner = Convert.ToBoolean(p.isOwner);
                        obj = GetMonthWiseData(obj, p.Budget);
                        model.Add(obj);
                        parentProgramId = p.id;
                        foreach (var t in p.tactic)
                        {
                            obj = new BudgetModel();
                            obj.Id = t.id.Replace("cpt_", "");
                            obj.ActivityId = t.id;
                            obj.ActivityName = t.title;
                            obj.ActivityType = ActivityType.ActivityTactic;
                            obj.ParentActivityId = parentProgramId;
                            obj.IsOwner = Convert.ToBoolean(t.isOwner);
                            obj.Budgeted = t.Cost;
                            obj = GetMonthWiseData(obj, t.Budget);
                            obj.AudienceId = t.AudianceId;
                            obj.GeographyId = t.GeographyId;
                            obj.AudienceTitle = t.AudianceName;
                            obj.GeographyTitle = t.GeographyName;
                            obj.VerticalId = t.VerticalId;
                            obj.VerticalTitle = t.VerticalName;
                            model.Add(obj);
                            parentTacticId = t.id;
                            foreach (var l in t.lineitems)
                            {
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
                                model.Add(obj);
                            }
                        }
                    }
                }
            }

            if (viewBy == ViewBy.Audiance || viewBy == ViewBy.Geography || viewBy == ViewBy.Vertical)
            {
                string prefix = "";
                List<BudgetModel> modelAud = new List<BudgetModel>();
                BudgetModel objPlanAud = model.Where(m => m.ActivityType == ActivityType.ActivityPlan).FirstOrDefault();
                modelAud.Add(objPlanAud); //Added plan to the Audience model

                List<string> CampaingIds;

                //Retrive the distinct audiances
                //List<int> audDistIds = model.Where(m => m.AudienceId != 0).Select(m => m.AudienceId).Distinct().ToList();
                List<string> DistIds = new List<string>();
                if (viewBy == ViewBy.Audiance)
                {
                    prefix = "a_";
                    DistIds = model.Where(m => m.AudienceId != 0).Select(m => m.AudienceId.ToString()).Distinct().ToList();
                }
                else if (viewBy == ViewBy.Geography)
                {
                    prefix = "g_";
                    DistIds = model.Where(m => m.GeographyId != Guid.Empty).Select(m => m.GeographyId.ToString()).Distinct().ToList();
                }
                else if (viewBy == ViewBy.Vertical)
                {
                    prefix = "v_";
                    DistIds = model.Where(m => m.VerticalId != 0).Select(m => m.VerticalId.ToString()).Distinct().ToList();
                }
                string prefixId = "";
                foreach (string audId in DistIds)
                {
                    CampaingIds = new List<string>();
                    //Add audiance to the model
                    //BudgetModel tmpTactic = model.Where(m => m.AudienceId == audId && m.ActivityType == ActivityTactic).FirstOrDefault();
                    BudgetModel tmpTactic;
                    obj = new BudgetModel();
                    if (viewBy == ViewBy.Audiance)
                    {
                        int objId = Convert.ToInt32(audId);
                        tmpTactic = model.Where(m => m.AudienceId == objId && m.ActivityType == ActivityType.ActivityTactic).FirstOrDefault();
                        obj.ActivityId = prefix + audId;
                        obj.ActivityName = tmpTactic.AudienceTitle;
                        obj.ActivityType = ActivityType.ActivityAudience;
                        obj.ParentActivityId = objPlanAud.ActivityId;
                        parentAudId = prefix + audId;
                        prefixId = "a" + audId + "_";
                    }
                    else if (viewBy == ViewBy.Geography)
                    {
                        Guid objId = new Guid(audId);
                        tmpTactic = model.Where(m => m.GeographyId == objId && m.ActivityType == ActivityType.ActivityTactic).FirstOrDefault();
                        obj.ActivityId = prefix + audId;
                        obj.ActivityName = tmpTactic.GeographyTitle;
                        obj.ActivityType = ActivityType.ActivityGeography;
                        obj.ParentActivityId = objPlanAud.ActivityId;
                        parentAudId = prefix + audId;
                        prefixId = "g" + audId + "_";
                    }
                    else if (viewBy == ViewBy.Vertical)
                    {
                        int objId = Convert.ToInt32(audId);
                        tmpTactic = model.Where(m => m.VerticalId == objId && m.ActivityType == ActivityType.ActivityTactic).FirstOrDefault();
                        obj.ActivityId = prefix + audId;
                        obj.ActivityName = tmpTactic.VerticalTitle;
                        obj.ActivityType = ActivityType.ActivityVertical;
                        obj.ParentActivityId = objPlanAud.ActivityId;
                        parentAudId = prefix + audId;
                        prefixId = "v" + audId + "_";
                    }
                    obj.Budgeted = 0;
                    obj.IsOwner = true;
                    obj.Month = new BudgetMonth();
                    modelAud.Add(obj);

                    //Add all tactics and its line items
                    //List<BudgetModel> lstTactic = model.Where(m => m.AudienceId == audId && m.ActivityType == ActivityTactic).ToList();
                    List<BudgetModel> lstTactic = new List<BudgetModel>();
                    if (viewBy == ViewBy.Audiance)
                    {
                        int objId = Convert.ToInt32(audId);
                        lstTactic = model.Where(m => m.AudienceId == objId && m.ActivityType == ActivityType.ActivityTactic).ToList();
                    }
                    else if (viewBy == ViewBy.Geography)
                    {
                        Guid objId = new Guid(audId);
                        lstTactic = model.Where(m => m.GeographyId == objId && m.ActivityType == ActivityType.ActivityTactic).ToList();
                    }
                    else if (viewBy == ViewBy.Vertical)
                    {
                        int objId = Convert.ToInt32(audId);
                        lstTactic = model.Where(m => m.VerticalId == objId && m.ActivityType == ActivityType.ActivityTactic).ToList();
                    }
                    foreach (BudgetModel objTactic in lstTactic)
                    {
                        BudgetModel objProgram = model.Where(m => m.ActivityId == objTactic.ParentActivityId && m.ActivityType == ActivityType.ActivityProgram).FirstOrDefault();
                        if (objProgram != null)
                        {
                            BudgetModel objCampaign = model.Where(m => m.ActivityId == objProgram.ParentActivityId && m.ActivityType == ActivityType.ActivityCampaign).FirstOrDefault();
                            if (objCampaign != null)
                            {
                                if (!CampaingIds.Contains(objCampaign.ActivityId))
                                {
                                    CampaingIds.Add(objCampaign.ActivityId);
                                }
                            }
                        }
                    }
                    foreach (string campaingid in CampaingIds)
                    {
                        BudgetModel objCampaign = model.Where(m => m.ActivityId == campaingid && m.ActivityType == ActivityType.ActivityCampaign).FirstOrDefault();
                        modelAud.Add(GetClone(objCampaign, prefixId + objCampaign.ActivityId, parentAudId));
                        List<BudgetModel> lstProgram = model.Where(m => m.ParentActivityId == campaingid && m.ActivityType == ActivityType.ActivityProgram).ToList();
                        foreach (BudgetModel objProgram in lstProgram)
                        {
                            //List<BudgetModel> lstTactics = model.Where(m => m.ParentActivityId == objProgram.ActivityId && m.ActivityType == ActivityTactic).ToList();
                            List<BudgetModel> lstTactics = new List<BudgetModel>();
                            if (viewBy == ViewBy.Audiance)
                            {
                                int objId = Convert.ToInt32(audId);
                                lstTactics = model.Where(m => m.ParentActivityId == objProgram.ActivityId && m.ActivityType == ActivityType.ActivityTactic && m.AudienceId == objId).ToList();
                            }
                            else if (viewBy == ViewBy.Geography)
                            {
                                Guid objId = new Guid(audId);
                                lstTactics = model.Where(m => m.ParentActivityId == objProgram.ActivityId && m.ActivityType == ActivityType.ActivityTactic && m.GeographyId == objId).ToList();
                            }
                            else if (viewBy == ViewBy.Vertical)
                            {
                                int objId = Convert.ToInt32(audId);
                                lstTactics = model.Where(m => m.ParentActivityId == objProgram.ActivityId && m.ActivityType == ActivityType.ActivityTactic && m.VerticalId == objId).ToList();
                            }
                            if (lstTactics.Count() > 0)
                            {
                                modelAud.Add(GetClone(objProgram, prefixId + objProgram.ActivityId, prefixId + objProgram.ParentActivityId));
                                foreach (BudgetModel objT in lstTactics)
                                {
                                    //if (objT.AudienceId == audId)
                                    //{
                                    List<BudgetModel> lstLines = model.Where(m => m.ParentActivityId == objT.ActivityId && m.ActivityType == ActivityType.ActivityLineItem).ToList();
                                    modelAud.Add(GetClone(objT, prefixId + objT.ActivityId, prefixId + objT.ParentActivityId));
                                    foreach (BudgetModel objL in lstLines)
                                    {
                                        modelAud.Add(GetClone(objL, prefixId + objL.ActivityId, prefixId + objL.ParentActivityId));
                                    }
                                    //}
                                }
                            }
                        }
                    }

                }
                model = modelAud;
            }
            //Threre is no need to manage lines for actuals
            if (budgetTab == BudgetTab.Planned)
            {
                model = ManageLineItems(model);
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
                }
            }

            ViewBag.AllocatedBy = AllocatedBy;
            ViewBag.ViewBy = (int)viewBy;
            ViewBag.Tab = (int)budgetTab;

            model = CalculateBottomUp(model, ActivityType.ActivityTactic, ActivityType.ActivityLineItem, budgetTab);
            model = CalculateBottomUp(model, ActivityType.ActivityProgram, ActivityType.ActivityTactic, budgetTab);
            model = CalculateBottomUp(model, ActivityType.ActivityCampaign, ActivityType.ActivityProgram, budgetTab);
            if (viewBy == ViewBy.Campaign)
            {
                model = CalculateBottomUp(model, ActivityType.ActivityPlan, ActivityType.ActivityCampaign, budgetTab);
            }
            else if (viewBy == ViewBy.Audiance)
            {
                model = CalculateBottomUp(model, ActivityType.ActivityAudience, ActivityType.ActivityCampaign, budgetTab);
                model = CalculateBottomUp(model, ActivityType.ActivityPlan, ActivityType.ActivityAudience, budgetTab);
            }
            else if (viewBy == ViewBy.Geography)
            {
                model = CalculateBottomUp(model, ActivityType.ActivityGeography, ActivityType.ActivityCampaign, budgetTab);
                model = CalculateBottomUp(model, ActivityType.ActivityPlan, ActivityType.ActivityGeography, budgetTab);
            }
            else if (viewBy == ViewBy.Vertical)
            {
                model = CalculateBottomUp(model, ActivityType.ActivityVertical, ActivityType.ActivityCampaign, budgetTab);
                model = CalculateBottomUp(model, ActivityType.ActivityPlan, ActivityType.ActivityVertical, budgetTab);
            }

            BudgetMonth a = new BudgetMonth();
            BudgetMonth child = new BudgetMonth();
            BudgetMonth PercAllocated = new BudgetMonth();
            child = model.Where(m => m.ActivityType == ActivityType.ActivityPlan).Select(m => m.Month).SingleOrDefault();
            a = model.Where(m => m.ActivityType == ActivityType.ActivityPlan).Select(m => m.ParentMonth).SingleOrDefault();


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
            Sessions.PlanId = PlanId;
            return PartialView("_Budget", model);
        }

        private BudgetModel GetClone(BudgetModel obj, string Id, string ParentId)
        {
            BudgetModel tmp = new BudgetModel();
            tmp.ActivityId = Id;
            tmp.ParentActivityId = ParentId;
            tmp.ActivityName = obj.ActivityName;
            tmp.ActivityType = obj.ActivityType;
            tmp.Allocated = obj.Allocated;
            tmp.AudienceId = obj.AudienceId;
            tmp.AudienceTitle = obj.AudienceTitle;
            tmp.Budgeted = obj.Budgeted;
            tmp.GeographyId = obj.GeographyId;
            tmp.GeographyTitle = obj.GeographyTitle;
            tmp.Id = obj.Id;
            tmp.IsOwner = obj.IsOwner;
            tmp.Month = obj.Month;
            tmp.ParentMonth = obj.ParentMonth;
            tmp.SumMonth = obj.SumMonth;
            return tmp;
        }

        /// <summary>
        /// Calculate the bottom up planeed cost
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ParentActivityType"></param>
        /// <param name="ChildActivityType"></param>
        /// <returns></returns>
        public List<BudgetModel> CalculateBottomUp(List<BudgetModel> model, string ParentActivityType, string ChildActivityType, BudgetTab budgetTab)
        {
            if (budgetTab == BudgetTab.Actual && ParentActivityType == ActivityType.ActivityTactic)
            {
                foreach (BudgetModel l in model.Where(l => l.ActivityType == ParentActivityType))
                {
                    List<BudgetModel> LineCheck = model.Where(lines => lines.ParentActivityId == l.ActivityId && lines.ActivityType == ActivityType.ActivityLineItem).ToList();
                    if (LineCheck.Count() > 0)
                    {
                        BudgetMonth parent = new BudgetMonth();
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
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonth = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().Month;
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().Month = parent;
                    }
                    else
                    {
                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonth = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().Month;
                    }
                }
            }
            else
            {
                foreach (BudgetModel l in model.Where(l => l.ActivityType == ParentActivityType))
                {
                    BudgetMonth parent = new BudgetMonth();
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
                    model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().ParentMonth = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().Month;
                    model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().Month = parent;
                    //l.ParentMonth = parent;
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
                BudgetMonth lineDiff = new BudgetMonth();
                List<BudgetModel> lines = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId).ToList();
                BudgetModel otherLine = lines.Where(ol => ol.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault();
                lines = lines.Where(ol => ol.ActivityName != Common.DefaultLineItemTitle).ToList();
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

                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().Month = lineDiff;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().ParentMonth = lineDiff;

                        double allocated = l.Allocated - lines.Sum(l1 => l1.Allocated);
                        allocated = allocated < 0 ? 0 : allocated;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().Allocated = allocated;
                    }
                    else
                    {
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().Month = l.Month;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().ParentMonth = l.Month;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.ActivityName == Common.DefaultLineItemTitle).SingleOrDefault().Allocated = l.Allocated < 0 ? 0 : l.Allocated;
                    }
                }
            }
            //foreach (BudgetModel l in model.Where(l => l.ActivityType == ActivityLineItem && l.ActivityName == "Other"))
            //{
            //    l.Allocated = l.Month.Jan + l.Month.Feb + l.Month.Mar + l.Month.Apr + l.Month.May + l.Month.Jun + l.Month.Jul + l.Month.Aug + l.Month.Sep + l.Month.Oct + l.Month.Nov + l.Month.Dec;
            //}
            return model;
        }

        #endregion

        /// <summary>
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="CloneType"></param>
        /// <param name="Id"></param>
        /// <param name="title"></param>
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
                            return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget }), Id=rtResult, msg = strMessage  });
                        else
                            return Json(new { redirect = Url.Action("Budgeting", new { type = CalledFromBudget, expand = expand + Id.ToString() }), Id = rtResult, msg = strMessage  });
                    }
                    else
                    {
                        TempData["SuccessMessageDeletedPlan"] = strMessage;
                        return Json(new { redirect = Url.Action("Assortment"), planId = Sessions.PlanId, Id = rtResult, msg = strMessage  });
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

        /// <summary>
        /// Kalpesh Sharma : #752 Update line item cost with the total cost from the monthly/quarterly allocation
        /// </summary>
        /// <param name="arrBudgetInputValues">budget allocation value</param>
        /// <param name="EnteredCost">entered budget cost</param>
        /// <returns>Return the Sum of Budget Allocation </returns>
        public double UpdateBugdetAllocationCost(string[] arrBudgetInputValues, double enteredCost)
        {
            //Check the budget allocation value is greater then 0
            if (arrBudgetInputValues.Length > 0)
            {
                List<double> BudgetValues = new List<double>();
                bool IsExplcitValue = false;
                //Iterate all the values of  budget allocation.
                foreach (string item in arrBudgetInputValues)
                {
                    //If  budget allocation value is "" then Skip those value 
                    if (item != "")
                    {
                        BudgetValues.Add(Convert.ToDouble(item));
                        IsExplcitValue = true;
                    }
                }
                //Get the sum of budget allocation value
                double BugdetAllocationSum = BudgetValues.Sum();
                if (BugdetAllocationSum == 0 && !IsExplcitValue)
                {
                    // Return the entered budget cost
                    return enteredCost;
                }
                return BugdetAllocationSum;
            }
            return enteredCost;
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

                List<string> lstActualAllocationMonthly = new List<string>() { "Y1", "Y2", "Y3", "Y4", "Y5", "Y6", "Y7", "Y8", "Y9", "Y10", "Y11", "Y12" };


                var lstActualLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(c => c.PlanLineItemId.Equals(planLineItemId)).ToList()
                                                              .Select(c => new
                                                              {
                                                                  c.PlanLineItemId,
                                                                  c.Period,
                                                                  c.Value
                                                              }).ToList();

                //Set the custom array for fecthed Line item Actual data .
                var ActualCostData = lstActualAllocationMonthly.Select(m => new
                {
                    periodTitle = m,
                    costValue = lstActualLineItemCost.SingleOrDefault(c => c.Period == m && c.PlanLineItemId == planLineItemId) == null ? "" : lstActualLineItemCost.SingleOrDefault(c => c.Period == m && c.PlanLineItemId == planLineItemId).Value.ToString()
                });
                var projectedData = db.Plan_Campaign_Program_Tactic_LineItem.SingleOrDefault(p => p.PlanLineItemId.Equals(planLineItemId));
                double projectedval = projectedData != null ? projectedData.Cost : 0;

                var returndata = new { ActualData = ActualCostData, ProjectedValue = projectedval, LastUpdatedBy = strLastUpdatedBy };

                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Viral Kadiya.
        /// Action to Get Actuals cost Value Of line item.
        /// </summary>
        /// <param name="planLineItemId">Plan line item Id.</param>
        /// <returns>Returns Json Result of line item actuals Value.</returns>
        public JsonResult SaveActualsLineitemData(string strActualsData, string strPlanItemId, string LineItemTitle = "")
        {
            string[] arrActualCostInputValues = strActualsData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int PlanLineItemId = int.Parse(strPlanItemId);
            int tid = db.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanLineItemId == PlanLineItemId).SingleOrDefault().PlanTacticId;
            var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                           join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                                           join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                           join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                           where pcptl.Title.Trim().ToLower().Equals(LineItemTitle.Trim().ToLower()) && !pcptl.PlanLineItemId.Equals(PlanLineItemId) && pcptl.IsDeleted.Equals(false)
                                           && pcpt.PlanTacticId == tid
                                           select pcpt).FirstOrDefault();

            if (pcptvar != null)
            {
                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
            }
            else
            {
            Plan_Campaign_Program_Tactic_LineItem objPCPTL = db.Plan_Campaign_Program_Tactic_LineItem.Where(c => c.PlanLineItemId == PlanLineItemId).SingleOrDefault();
            if (objPCPTL != null && !string.IsNullOrEmpty(LineItemTitle))
            {
                objPCPTL.Title = LineItemTitle;
                objPCPTL.ModifiedBy = Sessions.User.UserId;
                objPCPTL.ModifiedDate = DateTime.Now;
                db.Entry(objPCPTL).State = EntityState.Modified;
                db.SaveChanges();
            }
            var PrevActualAllocationListTactics = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(c => c.PlanLineItemId == PlanLineItemId).Select(c => c).ToList();
            if (PrevActualAllocationListTactics != null && PrevActualAllocationListTactics.Count > 0)
            {
                PrevActualAllocationListTactics.ForEach(a => db.Entry(a).State = EntityState.Deleted);
                int result = db.SaveChanges();
            }
            //Insert actual cost of tactic
            int saveresult = 0;
            for (int i = 0; i < arrActualCostInputValues.Length; i++)
            {
                if (arrActualCostInputValues[i] != "")
                {
                    Plan_Campaign_Program_Tactic_LineItem_Actual obPlanCampaignProgramTacticActual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
                    obPlanCampaignProgramTacticActual.PlanLineItemId = PlanLineItemId;
                    obPlanCampaignProgramTacticActual.Period = "Y" + (i + 1);
                    obPlanCampaignProgramTacticActual.Value = Convert.ToDouble(arrActualCostInputValues[i]);
                    obPlanCampaignProgramTacticActual.CreatedBy = Sessions.User.UserId;
                    obPlanCampaignProgramTacticActual.CreatedDate = DateTime.Now;
                    db.Entry(obPlanCampaignProgramTacticActual).State = EntityState.Added;
                }
            }
            saveresult = db.SaveChanges();
                
                int pid = db.Plan_Campaign_Program_Tactic.Where(s => s.PlanTacticId == tid).SingleOrDefault().PlanProgramId;
                int cid = db.Plan_Campaign_Program.Where(s => s.PlanProgramId == pid).SingleOrDefault().PlanCampaignId;
            if (saveresult > 0)
                {
                    string strMessage = Common.objCached.PlanEntityActualsUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                    return Json(new { id = strPlanItemId, TabValue = "Actuals", msg = strMessage, planCampaignID = cid, planProgramID = pid, planTacticID = tid });
                }
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}

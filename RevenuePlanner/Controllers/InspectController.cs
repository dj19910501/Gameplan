using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Elmah;
using Newtonsoft.Json;
using System.Transactions;
using RevenuePlanner.BDSService;
using System.Web;
using Integration;
using System.Text.RegularExpressions;

namespace RevenuePlanner.Controllers
{
    public class InspectController : CommonController
    {
        //
        // GET: /Inspect/

        #region Variables

        private MRPEntities db = new MRPEntities();
        private BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
        private const string Campaign_InspectPopup_Flag_Color = "C6EBF3";
        private const string Plan_InspectPopup_Flag_Color = "C6EBF3";       // Added by Sohel Pathan on 07/11/2014 for PL ticket #811
        private const string Program_InspectPopup_Flag_Color = "3DB9D3";
        ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
        private const string GameplanIntegrationService = "Gameplan Integration Service";
        private string DefaultLineItemTitle = "Line Item";
        private string PeriodChar = "Y";
        #endregion

        #region "Inspect Index"
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region "Plan related Functions"

        #region Load Setup tab for Plan Inspect Pop up
        /// <summary>
        /// Added By : Sohel Pathan
        /// Added Date : 07/11/2014
        /// Action to Load Setup Tab for Plan.
        /// </summary>
        /// <param name="id">Plan Id.</param>
        /// <param name="InspectPopupMode"></param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadPlanSetup(int id, string InspectPopupMode = "")
        {
            InspectModel _inspectmodel;
            //// Load Inspect Model data.
            if (TempData["PlanModel"] != null)
            {
                _inspectmodel = (InspectModel)TempData["PlanModel"];
            }
            else
            {
                _inspectmodel = GetInspectModel(id, Convert.ToString(Enums.Section.Plan).ToLower());
            }

            try
            {
                _inspectmodel.Owner = Common.GetUserName(_inspectmodel.OwnerId.ToString());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //// To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            ViewBag.PlanDetails = _inspectmodel;

            if (InspectPopupMode == Enums.InspectPopupMode.ReadOnly.ToString())
            {
                ViewBag.InspectMode = Enums.InspectPopupMode.ReadOnly.ToString();
            }
            else if (InspectPopupMode == Enums.InspectPopupMode.Edit.ToString())
            {
                ViewBag.InspectMode = Enums.InspectPopupMode.Edit.ToString();
            }
            else
            {
                ViewBag.InspectMode = "";
            }

            return PartialView("_SetupPlan", _inspectmodel);
        }
        #endregion

        #region Load Budget tab for Plan Inspect Pop up
        /// <summary>
        /// Added By : Sohel Pathan
        /// Added Date : 07/11/2014
        /// Action to Load Budget Tab for Plan.
        /// </summary>
        /// <param name="id">Plan Id.</param>
        /// <param name="InspectPopupMode"></param>
        /// <returns>Returns Partial View Of Budget Tab.</returns>
        public ActionResult LoadPlanBudget(int id, string InspectPopupMode = "")
        {
            InspectModel _inspectmodel;
            // Load Inspect Model data.
            if (TempData["PlanModel"] != null)
            {
                _inspectmodel = (InspectModel)TempData["PlanModel"];
            }
            else
            {
                _inspectmodel = GetInspectModel(id, Convert.ToString(Enums.Section.Plan).ToLower());
            }

            try
            {
                _inspectmodel.Owner = Common.GetUserName(_inspectmodel.OwnerId.ToString());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //// To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            ViewBag.PlanDetails = _inspectmodel;

            if (InspectPopupMode == Enums.InspectPopupMode.ReadOnly.ToString())
            {
                ViewBag.InspectMode = Enums.InspectPopupMode.ReadOnly.ToString();
            }
            else if (InspectPopupMode == Enums.InspectPopupMode.Edit.ToString())
            {
                ViewBag.InspectMode = Enums.InspectPopupMode.Edit.ToString();
            }
            else
            {
                ViewBag.InspectMode = "";
            }

            double TotalAllocatedCampaignBudget = 0;
            var PlanCampaignBudgetList = db.Plan_Campaign_Budget.Where(pcb => pcb.Plan_Campaign.PlanId == _inspectmodel.PlanId && pcb.Plan_Campaign.IsDeleted == false).Select(budget => budget.Value).ToList();
            if (PlanCampaignBudgetList.Count > 0)
            {
                TotalAllocatedCampaignBudget = PlanCampaignBudgetList.Sum();
            }
            ViewBag.TotalAllocatedCampaignBudget = TotalAllocatedCampaignBudget;

            return PartialView("_BudgetPlan", _inspectmodel);
        }
        #endregion

        #region Save Plan Details other than Budget Allocation
        /// <summary>
        /// Save Plan Details other than Budget Allocation
        /// </summary>
        /// <param name="objPlanModel"></param>
        /// <param name="BudgetInputValues"></param>
        /// <param name="planBudget"></param>
        /// <param name="RedirectType"></param>
        /// <param name="UserId"></param> Added by Sohel Pathan on 07/08/2014 for PL ticket #672
        /// <returns></returns>
        [HttpPost]
        public JsonResult SavePlanDetails(InspectModel objPlanModel, string BudgetInputValues = "", string planBudget = "", string RedirectType = "", string UserId = "")
        {
            //// check whether UserId is current loggined user or not.
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
                if (ModelState.IsValid)
                {
                    Plan plan = new Plan();
                    //// Get Plan Updated message.
                    string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Plan.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.

                    if (objPlanModel.PlanId > 0)
                    {
                        //// Get Plan list by PlanId.
                        plan = db.Plans.Where(_plan => _plan.PlanId == objPlanModel.PlanId).ToList().FirstOrDefault();

                        plan.Title = objPlanModel.Title.Trim();
                        plan.ModifiedBy = Sessions.User.UserId;
                        plan.ModifiedDate = System.DateTime.Now;

                        if (BudgetInputValues == "" && planBudget.ToString() == "") //// Setup Tab
                        {
                            plan.Description = objPlanModel.Description;
                        }
                        else   //// Budget Tab
                        {
                            //// Get budget updated message.
                            strMessage = Common.objCached.PlanEntityAllocationUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Plan.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                            plan.Budget = Convert.ToDouble(planBudget.ToString().Trim().Replace(",", "").Replace("$", ""));

                            #region Update Budget Allocation Value
                            if (BudgetInputValues != "")
                            {
                                string[] arrBudgetInputValues = BudgetInputValues.Split(',');

                                //// Get Previous budget allocation data by PlanId.
                                var PrevPlanBudgetAllocationList = db.Plan_Budget.Where(pb => pb.PlanId == objPlanModel.PlanId).Select(pb => pb).ToList();

                                if (arrBudgetInputValues.Length == 12)  // if current input values are monthly.
                                {
                                    for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                    {
                                        //// Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                        bool isExists = false;
                                        if (PrevPlanBudgetAllocationList != null)
                                        {
                                            if (PrevPlanBudgetAllocationList.Count > 0)
                                            {
                                                //// Get budget value periodically.
                                                var updatePlanBudget = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (i + 1))).FirstOrDefault();
                                                if (updatePlanBudget != null)
                                                {
                                                    if (arrBudgetInputValues[i] != "")
                                                    {
                                                        //// Get current inputed value.
                                                        var newValue = Convert.ToDouble(arrBudgetInputValues[i]);
                                                        if (updatePlanBudget.Value != newValue)
                                                        {
                                                            //// Update previous budget value with current value.
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
                                        //// if previous values does not exist then insert new values.
                                        if (!isExists && arrBudgetInputValues[i] != "")
                                        {
                                            //// End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                            Plan_Budget objPlanBudget = new Plan_Budget();
                                            objPlanBudget.PlanId = objPlanModel.PlanId;
                                            objPlanBudget.Period = PeriodChar + (i + 1);
                                            objPlanBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                            objPlanBudget.CreatedBy = Sessions.User.UserId;
                                            objPlanBudget.CreatedDate = DateTime.Now;
                                            db.Entry(objPlanBudget).State = EntityState.Added;
                                        }
                                    }
                                }
                                else if (arrBudgetInputValues.Length == 4) //// if current input values are Quarterly.
                                {
                                    int QuarterCnt = 1;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        //// Start - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                        bool isExists = false;
                                        if (PrevPlanBudgetAllocationList != null && PrevPlanBudgetAllocationList.Count > 0)
                                        {
                                            //// Get current Quarter months budget.
                                            var thisQuartersMonthList = PrevPlanBudgetAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                            var thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                            if (thisQuarterFirstMonthBudget != null)
                                            {
                                                if (arrBudgetInputValues[i] != "")
                                                {
                                                    var thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(a => a.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(a => a.Value);
                                                    //// Get quarter total budget. 
                                                    var thisQuarterTotalBudget = thisQuarterFirstMonthBudget.Value + thisQuarterOtherMonthBudget;
                                                    var newValue = Convert.ToDouble(arrBudgetInputValues[i]);

                                                    if (thisQuarterTotalBudget != newValue)
                                                    {
                                                        //// Get budget difference.
                                                        var BudgetDiff = newValue - thisQuarterTotalBudget;
                                                        if (BudgetDiff > 0)
                                                        {
                                                            //// Set quarter first month budget value.
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
                                        if (!isExists && arrBudgetInputValues[i] != "")
                                        {
                                            //// End - Added by Sohel Pathan on 26/08/2014 for PL ticket #642
                                            Plan_Budget objPlanBudget = new Plan_Budget();
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
                        }
                        db.Entry(plan).State = EntityState.Modified;
                        Common.InsertChangeLog(plan.PlanId, 0, plan.PlanId, plan.Title, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                    }

                    int result = db.SaveChanges();
                    if (result > 0)
                    {
                        if (RedirectType.ToLower() == "budgeting")
                        {
                            TempData["SuccessMessage"] = "Plan Saved Successfully";
                            return Json(new { id = plan.PlanId, redirect = Url.Action("Budgeting") });
                        }
                        else if (RedirectType.ToLower() == "")
                        {

                            return Json(new { id = plan.PlanId, succmsg = strMessage, redirect = "" });
                        }
                        else
                        {
                            return Json(new { id = plan.PlanId, redirect = Url.Action("Assortment", new { ismsg = "Plan Saved Successfully." }) });
                        }
                    }
                    else
                    {
                        return Json(new { id = 0, errormsg = Common.objCached.ErrorOccured.ToString() });
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { id = 0 });
        }
        #endregion
        #endregion

        #region "Campaign related Functions"

        /// <summary>
        /// Added By: Kunal.
        /// Action to Load Campaign Setup Tab.
        /// </summary>
        /// <param name="id">Plan Campaign Id.</param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadSetupCampaign(int id)
        {
            InspectModel _inspectmodel;
            // Load Inspect Model data.
            if (TempData["CampaignModel"] != null)
            {
                _inspectmodel = (InspectModel)TempData["CampaignModel"];
            }
            else
            {
                _inspectmodel = GetInspectModel(id, "campaign", false);
            }

            try
            {
                _inspectmodel.Owner = Common.GetUserName(_inspectmodel.OwnerId.ToString());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
            }
            //// Set Last Sync Date.
            if (_inspectmodel.LastSyncDate != null)
            {
                TimeZone localZone = TimeZone.CurrentTimeZone;

                ViewBag.LastSync = "Last synced with integration " + Common.GetFormatedDate(_inspectmodel.LastSyncDate) + ".";//// Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            }
            else
            {
                ViewBag.LastSync = string.Empty;
            }

            #region "Set values in ViewBag"

            List<Plan_Tactic_Values> PlanTacticValuesList = Common.GetMQLValueTacticList(db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.PlanCampaignId == id && tactic.IsDeleted == false).ToList());
            ViewBag.MQLs = PlanTacticValuesList.Sum(tactic => tactic.MQL);
            ViewBag.Cost = Common.CalculateCampaignCost(id); //// Modified for PL#440 by Dharmraj
            ViewBag.Revenue = Math.Round(PlanTacticValuesList.Sum(tactic => tactic.Revenue)); ////  Update by Bhavesh to Display Revenue

            ViewBag.CampaignDetail = _inspectmodel;
            double? objCampaign = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == id).FirstOrDefault().CampaignBudget;
            ViewBag.CampaignBudget = objCampaign != null ? objCampaign : 0;

            #endregion

            return PartialView("_SetupCampaign", _inspectmodel);
        }

        /// <summary>
        /// Added By: Kunal.
        /// Action to Load Campaign Review Tab.
        /// </summary>
        /// <param name="id">Plan Campaign Id.</param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadReviewCampaign(int id)
        {
            InspectModel _inspectmodel;
            // Load Inspect Model data.
            if (TempData["CampaignModel"] != null)
            {
                _inspectmodel = (InspectModel)TempData["CampaignModel"];
            }
            else
            {
                _inspectmodel = GetInspectModel(id, Convert.ToString(Enums.Section.Campaign).ToLower(), false);
            }

            //// Get Tactic comment by PlanCampaignId from Plan_Campaign_Program_Tactic_Comment table.
            var tacticComment = (from tc in db.Plan_Campaign_Program_Tactic_Comment
                                 where tc.PlanCampaignId == id && tc.PlanCampaignId.HasValue
                                 select tc).ToArray();

            //// Get Users list.
            List<Guid> userListId = new List<Guid>();
            userListId = (from tc in tacticComment select tc.CreatedBy).ToList<Guid>();
            userListId.Add(_inspectmodel.OwnerId);
            string userList = string.Join(",", userListId.Select(s => s.ToString()).ToArray());
            List<User> userName = new List<User>();

            try
            {
                userName = objBDSUserRepository.GetMultipleTeamMemberDetails(userList, Sessions.ApplicationId);
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            //// Set InspectReviewModel in ViewBag.
            ViewBag.ReviewModel = (from tc in tacticComment
                                   where (tc.PlanCampaignId.HasValue)
                                   select new InspectReviewModel
                                   {
                                       PlanCampaignId = Convert.ToInt32(tc.PlanCampaignId),
                                       Comment = tc.Comment,
                                       CommentDate = tc.CreatedDate,
                                       CommentedBy = userName.Where(user => user.UserId == tc.CreatedBy).Select(user => user.FirstName).FirstOrDefault() + " " + userName.Where(user => user.UserId == tc.CreatedBy).Select(user => user.LastName).FirstOrDefault(),
                                       CreatedBy = tc.CreatedBy
                                   }).ToList();
            //// Get Owner name by OwnerId from Username list.
            var ownername = (from user in userName
                             where user.UserId == _inspectmodel.OwnerId
                             select user.FirstName + " " + user.LastName).FirstOrDefault();
            if (ownername != null)
            {
                _inspectmodel.Owner = ownername.ToString();
            }
            // Added BY Bhavesh
            // Calculate MQL at runtime #376
            List<Plan_Campaign_Program_Tactic> PlanTacticIds = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.PlanCampaignId == id && tactic.IsDeleted == false).ToList();
            _inspectmodel.MQLs = Common.GetMQLValueTacticList(PlanTacticIds).Sum(t => t.MQL);
            ViewBag.CampaignDetail = _inspectmodel;

            bool isValidOwner = false;
            if (_inspectmodel.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
            }
            ViewBag.IsValidOwner = isValidOwner;

            ViewBag.IsModelDeploy = _inspectmodel.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690

            //To get permission status for Approve campaign , By dharmraj PL #538
            var lstSubOrdinatesPeers = new List<Guid>();
            try
            {
                lstSubOrdinatesPeers = Common.GetSubOrdinatesWithPeersNLevel();
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool isValidManagerUser = false;
            if (lstSubOrdinatesPeers.Contains(_inspectmodel.OwnerId) && Common.IsSectionApprovable(lstSubOrdinatesPeers, id, Enums.Section.Campaign.ToString()))////Modified by Sohel Pathan for PL ticket #688 and #689
            {
                isValidManagerUser = true;
            }
            ViewBag.IsValidManagerUser = isValidManagerUser;

            // Start - Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic
            ViewBag.IsCommentsViewEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.CommentsViewEdit);
            if ((bool)ViewBag.IsCommentsViewEditAuthorized == false)
                ViewBag.UnauthorizedCommentSection = Common.objCached.UnauthorizedCommentSection;
            // End - Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic

            // Added by Dharmraj Mangukiya for Deploy to integration button restrictions PL ticket #537
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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool IsCampaignEditable = false;
            if (_inspectmodel.OwnerId.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
            {
                IsCampaignEditable = true;
            }
            else if (IsPlanEditAllAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                IsCampaignEditable = true;
            }
            else if (IsPlanEditSubordinatesAuthorized)
            {
                if (lstSubOrdinates.Contains(_inspectmodel.OwnerId)) // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsCampaignEditable = true;
                }
            }
            ViewBag.IsCampaignEditable = IsCampaignEditable;

            return PartialView("_ReviewCampaign");
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Create Campaign.
        /// </summary>
        /// <param name="id">Plan Id</param>
        /// <returns>Returns Partial View Of Campaign.</returns>
        public PartialViewResult CreateCampaign(int id)
        {
            //// Get Plan by Id.
            int planId = id;
            var objPlan = db.Plans.FirstOrDefault(varP => varP.PlanId == planId);

            #region "Set values in ViewBag"
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);
            ViewBag.IsDeployedToIntegration = false;
            ViewBag.IsCreated = true;
            ViewBag.RedirectType = false;
            ViewBag.IsOwner = true;
            ViewBag.Year = objPlan.Year;
            ViewBag.PlanTitle = objPlan.Title;
            #endregion

            try
            {
                ViewBag.OwnerName = Common.GetUserName(Sessions.User.UserId.ToString());
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //// To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    ViewBag.IsServiceUnavailable = true;
                }
            }

            //// Set Plan_CampaignModel data to pass into partialview.
            Plan_CampaignModel pc = new Plan_CampaignModel();
            pc.PlanId = planId;
            pc.StartDate = GetCurrentDateBasedOnPlan();
            pc.EndDate = GetCurrentDateBasedOnPlan(true);
            pc.CampaignBudget = 0;
            pc.AllocatedBy = objPlan.AllocatedBy;

            #region "Calculate Plan remaining budget by plan Id"
            var lstAllCampaign = db.Plan_Campaign.Where(campaign => campaign.PlanId == planId && campaign.IsDeleted == false).ToList();
            double allCampaignBudget = lstAllCampaign.Sum(campaign => campaign.CampaignBudget);
            double planBudget = objPlan.Budget;
            double planRemainingBudget = planBudget - allCampaignBudget;
            ViewBag.planRemainingBudget = planRemainingBudget;
            #endregion

            return PartialView("_EditSetupCampaign", pc);
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Load Campaign Setup Tab in edit mode.
        /// </summary>
        /// <param name="id">Plan Campaign Id.</param>
        /// <returns>Returns Partial View Of edit Setup Tab.</returns>
        public ActionResult LoadEditSetupCampaign(int id)
        {
            ViewBag.IsCreated = false;
            //// Get Campaign list by Id.
            Plan_Campaign pc = db.Plan_Campaign.Where(pcobj => pcobj.PlanCampaignId.Equals(id) && pcobj.IsDeleted.Equals(false)).FirstOrDefault();
            if (pc == null)
            {
                return null;
            }

            try
            {
                ViewBag.OwnerName = Common.GetUserName(pc.CreatedBy.ToString());
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);

                }
            }
            #region "Set values in ViewBag"
            ViewBag.Year = pc.Plan.Year;
            ViewBag.PlanTitle = pc.Plan.Title;
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(pc.Plan.Model);
            #endregion

            //// Set Plan_CampaignModel data to pass into partialview.
            Plan_CampaignModel pcm = new Plan_CampaignModel();
            pcm.PlanCampaignId = pc.PlanCampaignId;
            pcm.Title = HttpUtility.HtmlDecode(pc.Title);
            pcm.Description = HttpUtility.HtmlDecode(pc.Description);
            pcm.IsDeployedToIntegration = pc.IsDeployedToIntegration;
            ViewBag.IsDeployedToIntegration = pcm.IsDeployedToIntegration;
            pcm.StartDate = pc.StartDate;
            pcm.EndDate = pc.EndDate;

            var programs = db.Plan_Campaign_Program.Where(program => program.PlanCampaignId == id && program.IsDeleted.Equals(false)).ToList();
            var tactic = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.Plan_Campaign_Program.PlanCampaignId == id && _tactic.IsDeleted.Equals(false)).ToList();

            //// Set Program Start date & End date.
            var _programdata = (from _program in programs select _program);
            if (_programdata.Count() > 0)
            {
                pcm.PStartDate = (from opsd in _programdata select opsd.StartDate).Min();
                pcm.PEndDate = (from opsd in _programdata select opsd.EndDate).Max();
            }

            //// Tactic Start date & End date.
            var _tacticdata = (from _tactic in tactic select _tactic);
            if (_tacticdata.Count() > 0)
            {
                pcm.TStartDate = (from _tactic in _tacticdata select _tactic.StartDate).Min();
                pcm.TEndDate = (from _tactic in _tacticdata select _tactic.EndDate).Max();
            }


            List<Plan_Tactic_Values> PlanTacticValuesList = Common.GetMQLValueTacticList(tactic);
            pcm.MQLs = PlanTacticValuesList.Sum(tm => tm.MQL);
            pcm.Cost = Common.CalculateCampaignCost(pc.PlanCampaignId); //pc.Cost; // Modified for PL#440 by Dharmraj
            // Start Added By Dharmraj #567 : Budget allocation for campaign
            pcm.CampaignBudget = pc.CampaignBudget;
            pcm.AllocatedBy = pc.Plan.AllocatedBy;

            #region "Calculate plan remaining budget"
            var lstAllCampaign = db.Plan_Campaign.Where(campaign => campaign.PlanId == pc.PlanId && campaign.IsDeleted == false).ToList();
            double allCampaignBudget = lstAllCampaign.Sum(campaign => campaign.CampaignBudget);
            double planBudget = pc.Plan.Budget;
            double planRemainingBudget = planBudget - allCampaignBudget;
            ViewBag.planRemainingBudget = planRemainingBudget;
            #endregion

            pcm.Revenue = Math.Round(PlanTacticValuesList.Sum(tm => tm.Revenue)); //  Update by Bhavesh to Display Revenue
            // End Added By Dharmraj #567 : Budget allocation for campaign

            if (Sessions.User.UserId == pc.CreatedBy)
            {
                ViewBag.IsOwner = true;
            }
            else
            {
                ViewBag.IsOwner = false;
            }
            /*Modified By : Kalpesh Sharma :: Optimize the code and performance of application*/
            ViewBag.Year = pc.Plan.Year;
            return PartialView("_EditSetupCampaign", pcm);
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Save Campaign.
        /// </summary>
        /// <param name="form">Form object of Plan_CampaignModel.</param>
        /// <param name="UserId">User Id.</param>
        /// <param name="RedirectType">Redirect Type.</param>
        /// <param name="title"></param>
        /// <param name="customFieldInputs"></param>
        /// <param name="planId"></param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveCampaign(Plan_CampaignModel form, string title, string customFieldInputs, string UserId = "", int planId = 0)
        {
            //// check whether UserId is current loggined user or not.
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
                var customFields = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(customFieldInputs);

                //// Add New Record
                if (form.PlanCampaignId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// check same record exist or not.
                            var pc = db.Plan_Campaign.Where(plancampaign => (plancampaign.PlanId.Equals(planId) && plancampaign.IsDeleted.Equals(false) && plancampaign.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !plancampaign.PlanCampaignId.Equals(form.PlanCampaignId))).FirstOrDefault();

                            //// if record exist then return with duplication message.
                            if (pc != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { isSaved = false, msg = strDuplicateMessage });
                            }
                            else
                            {
                                #region "Insert New Record to Plan_Campaign table"
                                Plan_Campaign pcobj = new Plan_Campaign();

                                pcobj.PlanId = Sessions.PlanId;
                                pcobj.Title = form.Title;
                                pcobj.Description = form.Description;
                                pcobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                pcobj.StartDate = form.StartDate;
                                pcobj.EndDate = form.EndDate;
                                pcobj.CreatedBy = Sessions.User.UserId;
                                pcobj.CreatedDate = DateTime.Now;
                                pcobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString(); // status field in Plan_Campaign table 
                                pcobj.CampaignBudget = form.CampaignBudget;
                                db.Entry(pcobj).State = EntityState.Added;
                                int result = db.SaveChanges();
                                #endregion

                                int campaignid = pcobj.PlanCampaignId;
                                result = Common.InsertChangeLog(Sessions.PlanId, null, campaignid, pcobj.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);

                                #region "Save custom field to CustomField_Entity table"
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
                                db.SaveChanges();
                                #endregion

                                scope.Complete();
                                string strMessage = Common.objCached.PlanEntityCreated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                return Json(new { isSaved = true, msg = strMessage, CampaignID = campaignid });
                            }
                        }
                    }
                }
                else  //// Update record.
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// Get PlanId by PlanCampaignId.
                            planId = db.Plan_Campaign.Where(_plan => _plan.PlanCampaignId.Equals(form.PlanCampaignId)).FirstOrDefault().PlanId;
                            //// check for duplicate record.
                            var pc = db.Plan_Campaign.Where(plancampaign => (plancampaign.PlanId.Equals(planId) && plancampaign.IsDeleted.Equals(false) && plancampaign.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !plancampaign.PlanCampaignId.Equals(form.PlanCampaignId))).FirstOrDefault();

                            //// If record exist then return duplicatino message.
                            if (pc != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { isSaved = false, msg = strDuplicateMessage });
                            }
                            else
                            {
                                #region "Update record into Plan_Campaign table"
                                Plan_Campaign pcobj = db.Plan_Campaign.Where(pcobjw => pcobjw.PlanCampaignId.Equals(form.PlanCampaignId) && pcobjw.IsDeleted.Equals(false)).FirstOrDefault();
                                pcobj.Title = title;
                                pcobj.Description = form.Description;
                                pcobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                pcobj.StartDate = form.StartDate;
                                pcobj.EndDate = form.EndDate;
                                pcobj.ModifiedBy = Sessions.User.UserId;
                                pcobj.ModifiedDate = DateTime.Now;
                                pcobj.CampaignBudget = form.CampaignBudget;
                                db.Entry(pcobj).State = EntityState.Modified;
                                #endregion

                                Common.InsertChangeLog(Sessions.PlanId, null, pcobj.PlanCampaignId, pcobj.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);

                                #region "Remove previous custom fields by PlanCampaignId"
                                string entityTypeCampaign = Enums.EntityType.Campaign.ToString();
                                var prevCustomFieldList = db.CustomField_Entity.Where(custmfield => custmfield.EntityId == pcobj.PlanCampaignId && custmfield.CustomField.EntityType == entityTypeCampaign).ToList();
                                prevCustomFieldList.ForEach(custmfield => db.Entry(custmfield).State = EntityState.Deleted);
                                #endregion

                                #region "Save Custom fields to CustomField_Entity table"
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
                                db.SaveChanges();
                                #endregion

                                scope.Complete();
                                string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                return Json(new { isSaved = true, msg = strMessage });
                            }

                        }
                    }
                }

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { isSaved = false });
        }

        #region Budget Allocation for Campaign Tab
        /// <summary>
        /// Fetch the Campaign Budget Allocation 
        /// </summary>
        /// <param name="id">Campaign Id</param>
        /// <returns></returns>
        public PartialViewResult LoadCampaignBudgetAllocation(int id = 0)
        {
            //// Get Budget tab data of Camapaign by Id
            Plan_Campaign pcp = db.Plan_Campaign.Where(pcpobj => pcpobj.PlanCampaignId.Equals(id) && pcpobj.IsDeleted.Equals(false)).FirstOrDefault();
            if (pcp == null)
            {
                return null;
            }
            //// Set Plan_CampaignModel to pass into partialview.
            Plan_CampaignModel pcpm = new Plan_CampaignModel();
            pcpm.PlanCampaignId = pcp.PlanCampaignId;
            pcpm.CampaignBudget = pcp.CampaignBudget;

            // Get Plan Allocated from Plan table by PlanId.
            var objPlan = db.Plans.FirstOrDefault(varP => varP.PlanId == pcp.PlanId);
            pcpm.AllocatedBy = objPlan.AllocatedBy;

            #region "Calculate Plan remaining budget"
            var lstAllCampaign = db.Plan_Campaign.Where(campaign => campaign.PlanId == pcp.PlanId && campaign.IsDeleted == false).ToList();
            double allCampaignBudget = lstAllCampaign.Sum(campaign => campaign.CampaignBudget);
            double planBudget = objPlan.Budget;
            double planRemainingBudget = planBudget - allCampaignBudget;
            ViewBag.planRemainingBudget = planRemainingBudget;
            #endregion

            return PartialView("_SetupCampaignBudgetAllocation", pcpm);
        }

        /// <summary>
        /// Action to Save Campaign Allocation.
        /// </summary>
        /// <param name="form">Form object of Plan_Campaign_ProgramModel.</param>
        /// <param name="BudgetInputValues">Budget Input Values.</param>
        /// <param name="UserId">User Id.</param>
        /// <param name="title"></param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveCampaignBudgetAllocation(Plan_CampaignModel form, string BudgetInputValues, string UserId = "", string title = "")
        {
            //// check whether UserId is loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                ////Compare login user with userid.
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    return Json(new { IsSaved = false, msg = Common.objCached.LoginWithSameSession, JsonRequestBehavior.AllowGet });
                }
            }
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        //// Get Campaign data to check duplication.
                        var pc = db.Plan_Campaign.Where(plancampaign => (plancampaign.PlanId.Equals(db.Plan_Campaign.Where(_campaign => _campaign.PlanCampaignId == form.PlanCampaignId).Select(_campaign => _campaign.PlanId).FirstOrDefault()) && plancampaign.IsDeleted.Equals(false) && plancampaign.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !plancampaign.PlanCampaignId.Equals(form.PlanCampaignId))).FirstOrDefault();
                        string[] arrBudgetInputValues = BudgetInputValues.Split(',');

                        //// if duplicate record exist then return duplication message.
                        if (pc != null)
                        {
                            string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                            return Json(new { IsSaved = false, msg = strDuplicateMessage, JsonRequestBehavior.AllowGet });
                        }
                        else
                        {
                            #region " Update record to Plan_Campaign table"
                            Plan_Campaign pcobj = db.Plan_Campaign.Where(pcobjw => pcobjw.PlanCampaignId.Equals(form.PlanCampaignId) && pcobjw.IsDeleted.Equals(false)).FirstOrDefault();
                            pcobj.Title = title;
                            pcobj.ModifiedBy = Sessions.User.UserId;
                            pcobj.ModifiedDate = DateTime.Now;
                            pcobj.CampaignBudget = form.CampaignBudget;
                            db.Entry(pcobj).State = EntityState.Modified;
                            Common.InsertChangeLog(Sessions.PlanId, null, pcobj.PlanCampaignId, pcobj.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                            #endregion

                            if (arrBudgetInputValues.Length > 0)    // Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                            {
                                // Start Added By Dharmraj #567 : Budget allocation for campaign
                                //// Get Previous budget allocation list.
                                var PrevAllocationList = db.Plan_Campaign_Budget.Where(campBudget => campBudget.PlanCampaignId == form.PlanCampaignId).Select(campBudget => campBudget).ToList();

                                //// Process for Monthly budget values.
                                if (arrBudgetInputValues.Length == 12)
                                {
                                    for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                    {
                                        // Start - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                                        bool isExists = false;
                                        if (PrevAllocationList != null && PrevAllocationList.Count > 0)
                                        {
                                            //// Get previous campaign budget values by Period.
                                            var updatePlanCampaignBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (i + 1))).FirstOrDefault();
                                            if (updatePlanCampaignBudget != null)
                                            {
                                                if (arrBudgetInputValues[i] != "")
                                                {
                                                    //// Update budget value with old value.
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
                                        //// if Old budget value does not exist then insert new value to table.
                                        if (!isExists && arrBudgetInputValues[i] != "")
                                        {
                                            // End - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                                            Plan_Campaign_Budget objPlanCampaignBudget = new Plan_Campaign_Budget();
                                            objPlanCampaignBudget.PlanCampaignId = form.PlanCampaignId;
                                            objPlanCampaignBudget.Period = PeriodChar + (i + 1);
                                            objPlanCampaignBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                            objPlanCampaignBudget.CreatedBy = Sessions.User.UserId;
                                            objPlanCampaignBudget.CreatedDate = DateTime.Now;
                                            db.Entry(objPlanCampaignBudget).State = EntityState.Added;
                                        }
                                    }
                                }
                                else if (arrBudgetInputValues.Length == 4)  //// Process for Quarterly budget values.
                                {
                                    int QuarterCnt = 1;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        // Start - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                                        bool isExists = false;
                                        if (PrevAllocationList != null && PrevAllocationList.Count > 0)
                                        {
                                            //// Get Quarter budget list.
                                            var thisQuartersMonthList = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();

                                            //// Get First month values from Quarterly budget list.
                                            var thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                            if (thisQuarterFirstMonthBudget != null)
                                            {
                                                if (arrBudgetInputValues[i] != "")
                                                {
                                                    var thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(budget => budget.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(budget => budget.Value);
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
                                                                    thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
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
                                        //// if Old budget value does not exist then insert new value to table.
                                        if (!isExists && arrBudgetInputValues[i] != "")
                                        {
                                            // End - Added by Sohel Pathan on 27/08/2014 for PL ticket #758
                                            Plan_Campaign_Budget objPlanCampaignBudget = new Plan_Campaign_Budget();
                                            objPlanCampaignBudget.PlanCampaignId = form.PlanCampaignId;
                                            objPlanCampaignBudget.Period = PeriodChar + QuarterCnt;
                                            objPlanCampaignBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                            objPlanCampaignBudget.CreatedBy = Sessions.User.UserId;
                                            objPlanCampaignBudget.CreatedDate = DateTime.Now;
                                            db.Entry(objPlanCampaignBudget).State = EntityState.Added;
                                        }
                                        QuarterCnt = QuarterCnt + 3;
                                    }
                                }
                            }

                            db.SaveChanges();
                            scope.Complete();
                            string strMessage = Common.objCached.PlanEntityAllocationUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                            return Json(new { IsSaved = true, msg = strMessage, planCampaignId = form.PlanCampaignId, JsonRequestBehavior.AllowGet });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { IsSaved = false, msg = Common.objCached.ErrorOccured, JsonRequestBehavior.AllowGet });
        }

        #endregion
        #endregion

        #region "Program related Functions"

        /// <summary>
        /// Added By: Kunal.
        /// Action to Load Program Setup Tab.
        /// </summary>
        /// <param name="id">Plan Program Id.</param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadSetupProgram(int id)
        {
            InspectModel _inspectmodel;
            //// Load Inspect Model data.
            if (TempData["ProgramModel"] != null)
            {
                _inspectmodel = (InspectModel)TempData["ProgramModel"];
            }
            else
            {
                _inspectmodel = GetInspectModel(id, "program", false);
            }

            try
            {
                _inspectmodel.Owner = Common.GetUserName(_inspectmodel.OwnerId.ToString());
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            ViewBag.ProgramDetail = _inspectmodel;
            ViewBag.OwnerName = _inspectmodel.Owner;
            if (_inspectmodel.LastSyncDate != null)
            {
                TimeZone localZone = TimeZone.CurrentTimeZone;

                ViewBag.LastSync = "Last synced with integration " + Common.GetFormatedDate(_inspectmodel.LastSyncDate) + ".";////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            }
            else
            {
                ViewBag.LastSync = string.Empty;
            }
            double? objPlanProgramBudget = db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == id).FirstOrDefault().ProgramBudget;

            List<Plan_Tactic_Values> lstTacticValues = Common.GetMQLValueTacticList(db.Plan_Campaign_Program_Tactic.Where(t => t.PlanProgramId == id && t.IsDeleted == false).ToList());

            ViewBag.MQLs = lstTacticValues.Sum(tm => tm.MQL);
            ViewBag.Cost = Common.CalculateProgramCost(id); //pcp.Cost; modified for PL #440 by dharmraj 

            //Added By : Kalpesh Sharma : PL #605 : 07/29/2014
            //List<Plan_Tactic_Values> PlanTacticValuesList = Common.GetMQLValueTacticList(db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.PlanProgramId == id && t.IsDeleted == false).ToList());
            ViewBag.Revenue = Math.Round(lstTacticValues.Sum(tm => tm.Revenue));


            ViewBag.ProgramBudget = objPlanProgramBudget != null ? objPlanProgramBudget : 0;

            return PartialView("_SetupProgram", _inspectmodel);
        }

        /// <summary>
        /// Action to Load Program Setup Tab in Edit Mode with data.
        /// </summary>
        /// <param name="id">Plan Program Id.</param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public PartialViewResult LoadSetupProgramEdit(int id = 0)
        {
            Plan_Campaign_Program pcp = db.Plan_Campaign_Program.Where(pcpobj => pcpobj.PlanProgramId.Equals(id) && pcpobj.IsDeleted.Equals(false)).FirstOrDefault();
            if (pcp == null)
            {
                return null;
            }
            ViewBag.IsCreated = false;
            ViewBag.RedirectType = false;
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(pcp.Plan_Campaign.Plan.Model);

            #region "Set Plan_Campaign_ProgramModel to pass into partialview"
            Plan_Campaign_ProgramModel pcpm = new Plan_Campaign_ProgramModel();
            pcpm.PlanProgramId = pcp.PlanProgramId;
            pcpm.PlanCampaignId = pcp.PlanCampaignId;
            pcpm.Title = HttpUtility.HtmlDecode(pcp.Title);
            pcpm.Description = HttpUtility.HtmlDecode(pcp.Description);
            pcpm.StartDate = pcp.StartDate;
            pcpm.EndDate = pcp.EndDate;
            pcpm.CStartDate = pcp.Plan_Campaign.StartDate;
            pcpm.CEndDate = pcp.Plan_Campaign.EndDate;
            List<Plan_Campaign_Program_Tactic> lstTactic = (from tac in db.Plan_Campaign_Program_Tactic where tac.PlanProgramId == id && tac.IsDeleted.Equals(false) select tac).ToList();
            if (lstTactic != null && lstTactic.Count() > 0)
            {
                pcpm.TStartDate = (from otsd in lstTactic select otsd.StartDate).Min();
                pcpm.TEndDate = (from otsd in lstTactic select otsd.EndDate).Max();
            }

            List<Plan_Tactic_Values> lstPlanTacticValues = Common.GetMQLValueTacticList(lstTactic);
            pcpm.MQLs = lstPlanTacticValues.Sum(tm => tm.MQL);
            pcpm.Cost = Common.CalculateProgramCost(pcp.PlanProgramId);

            ViewBag.CampaignTitle = HttpUtility.HtmlDecode(pcp.Plan_Campaign.Title);

            //List<Plan_Tactic_Values> PlanTacticValuesList = Common.GetMQLValueTacticList(db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.PlanCampaignId == pcp.PlanCampaignId &&
            //    t.Plan_Campaign_Program.PlanProgramId == pcp.PlanProgramId && t.IsDeleted == false).ToList());
            pcpm.Revenue = Math.Round(lstPlanTacticValues.Sum(tm => tm.Revenue));

            pcpm.ProgramBudget = pcp.ProgramBudget;
            var objPlan = db.Plans.FirstOrDefault(varP => varP.PlanId == pcp.Plan_Campaign.PlanId);
            pcpm.AllocatedBy = objPlan.AllocatedBy;

            pcpm.IsDeployedToIntegration = pcp.IsDeployedToIntegration;

            #endregion

            if (Sessions.User.UserId == pcp.CreatedBy)
            {
                ViewBag.IsOwner = true;
            }
            else
            {
                ViewBag.IsOwner = false;
            }
            ViewBag.Campaign = HttpUtility.HtmlDecode(pcp.Plan_Campaign.Title);////Modified by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
            ViewBag.Year = objPlan.Year;

            var objPlanCampaign = db.Plan_Campaign.FirstOrDefault(c => c.PlanCampaignId == pcp.PlanCampaignId);
            double lstSelectedProgram = db.Plan_Campaign_Program.Where(p => p.PlanCampaignId == pcp.PlanCampaignId && p.IsDeleted == false).ToList().Sum(c => c.ProgramBudget);
            ViewBag.planRemainingBudget = (objPlanCampaign.CampaignBudget - lstSelectedProgram);

            try
            {
                ViewBag.IsServiceUnavailable = false;
                ViewBag.OwnerName = Common.GetUserName(pcp.CreatedBy.ToString());
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
                    ViewBag.IsServiceUnavailable = true;
                }
            }

            return PartialView("_EditSetupProgram", pcpm);
        }

        /// <summary>
        /// Action to Save/Update Program Setup Tab Data.
        /// </summary>
        /// <param name="form">Form data.</param>
        /// <param name="customFieldInputs">CustomFields.</param>
        /// <param name="UserId">UserId.</param>
        /// <param name="title">Title of Program.</param>
        /// <returns>Returns Save/Error message.</returns>
        [HttpPost]
        public ActionResult SetupSaveProgram(Plan_Campaign_ProgramModel form, string customFieldInputs, string UserId = "", string title = "")
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    return Json(new { IsSaved = false, errormsg = Common.objCached.LoginWithSameSession }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                //Deserialize customFieldInputs json string to  KeyValuePair List
                var customFields = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(customFieldInputs);
                int campaignId = form.PlanCampaignId;

                //// if programId null then insert new record.
                if (form.PlanProgramId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// Get duplicate record.
                            var pcpvar = (from pcp in db.Plan_Campaign_Program
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pcp.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcp.IsDeleted.Equals(false)
                                          && pc.PlanCampaignId == form.PlanCampaignId       //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
                                          select pcp).FirstOrDefault();

                            //// if duplicate record exist then return with duplication message.
                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { IsSaved = false, errormsg = strDuplicateMessage });
                            }
                            else
                            {

                                #region "Insert new record to Plan_Campaign_Program table"
                                Plan_Campaign_Program pcpobj = new Plan_Campaign_Program();
                                pcpobj.PlanCampaignId = form.PlanCampaignId;
                                pcpobj.Title = form.Title;
                                pcpobj.Description = form.Description;
                                pcpobj.StartDate = form.StartDate;
                                pcpobj.EndDate = form.EndDate;
                                pcpobj.CreatedBy = Sessions.User.UserId;
                                pcpobj.CreatedDate = DateTime.Now;
                                pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString(); //status field added for Plan_Campaign_Program table
                                pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                pcpobj.ProgramBudget = form.ProgramBudget;
                                db.Entry(pcpobj).State = EntityState.Added;
                                int result = db.SaveChanges();
                                #endregion

                                #region "Insert custom field records to CustomField_Entity table"
                                int programid = pcpobj.PlanProgramId;
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
                                #endregion

                                #region " Set campaign Start and End Date"
                                Plan_Campaign pcp = db.Plan_Campaign.Where(pcobj => pcobj.PlanCampaignId.Equals(pcpobj.PlanCampaignId) && pcobj.IsDeleted.Equals(false)).FirstOrDefault();
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
                                #endregion

                                result = Common.InsertChangeLog(Sessions.PlanId, null, programid, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                Common.ChangeCampaignStatus(pcpobj.PlanCampaignId);     //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                scope.Complete();
                                string strMessage = Common.objCached.PlanEntityCreated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                return Json(new { IsSaved = true, Msg = strMessage, programID = programid, campaignID = campaignId }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                else    //// if programId not null then update record.
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// Get duplicate record.
                            var pcpvar = (from pcp in db.Plan_Campaign_Program
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pcp.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcp.PlanProgramId.Equals(form.PlanProgramId) && pcp.IsDeleted.Equals(false)
                                          && pc.PlanCampaignId == form.PlanCampaignId
                                          select pcp).FirstOrDefault();
                            //// if duplicate record exist then return with duplication message.
                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { IsSaved = false, errormsg = strDuplicateMessage }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                #region "Update record to Plan_Campaign_Program table"
                                Plan_Campaign_Program pcpobj = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(form.PlanProgramId)).FirstOrDefault();
                                pcpobj.Title = title;
                                pcpobj.Description = form.Description;
                                pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
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
                                pcpobj.ModifiedBy = Sessions.User.UserId;
                                pcpobj.ModifiedDate = DateTime.Now;
                                pcpobj.ProgramBudget = form.ProgramBudget;
                                db.Entry(pcpobj).State = EntityState.Modified;
                                #endregion

                                #region "Save Customfields to CustomField_Entity table"
                                //// Delete previous customfields values.
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
                                int result = db.SaveChanges();
                                #endregion

                                result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanProgramId, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);

                                if (result >= 1)
                                {
                                    Common.ChangeCampaignStatus(pcpobj.PlanCampaignId);
                                    scope.Complete();
                                    string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    return Json(new { IsSaved = true, Msg = strMessage, campaignID = campaignId }, JsonRequestBehavior.AllowGet);
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

            return Json(new { IsSaved = false, errormsg = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Kunal.
        /// Action to Load Program Review Tab.
        /// </summary>
        /// <param name="id">Plan Program Id.</param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadReviewProgram(int id)
        {
            InspectModel im;
            //// Load Inspect Model data.
            if (TempData["ProgramModel"] != null)
            {
                im = (InspectModel)TempData["ProgramModel"];
            }
            else
            {
                im = GetInspectModel(id, Convert.ToString(Enums.Section.Program).ToLower(), false);
            }

            //// Get Tactic comments list by PlanProgramId.
            var tacticComment = (from tc in db.Plan_Campaign_Program_Tactic_Comment
                                 where tc.PlanProgramId == id && tc.PlanProgramId.HasValue
                                 select tc).ToArray();

            //// Get Userslist using Tactic comment list.
            List<Guid> userListId = new List<Guid>();
            userListId = (from ta in tacticComment select ta.CreatedBy).ToList<Guid>();
            userListId.Add(im.OwnerId);
            string userList = string.Join(",", userListId.Select(s => s.ToString()).ToArray());
            List<User> userName = new List<User>();

            try
            {
                userName = objBDSUserRepository.GetMultipleTeamMemberDetails(userList, Sessions.ApplicationId);
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            //// Load InspectReviewModel to ViewBag.
            ViewBag.ReviewModel = (from tc in tacticComment
                                   where (tc.PlanProgramId.HasValue)
                                   select new InspectReviewModel
                                   {
                                       PlanProgramId = Convert.ToInt32(tc.PlanProgramId),
                                       Comment = tc.Comment,
                                       CommentDate = tc.CreatedDate,
                                       CommentedBy = userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.FirstName).FirstOrDefault() + " " + userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.LastName).FirstOrDefault(),
                                       CreatedBy = tc.CreatedBy
                                   }).ToList();

            //// Set Owner name to InspectModel.
            var ownername = (from u in userName
                             where u.UserId == im.OwnerId
                             select u.FirstName + " " + u.LastName).FirstOrDefault();
            if (ownername != null)
            {
                im.Owner = ownername.ToString();
            }
            // Added BY Bhavesh
            // Calculate MQL at runtime #376
            List<Plan_Campaign_Program_Tactic> PlanTacticIds = db.Plan_Campaign_Program_Tactic.Where(ppt => ppt.PlanProgramId == id && ppt.IsDeleted == false).ToList();
            im.MQLs = Common.GetMQLValueTacticList(PlanTacticIds).Sum(t => t.MQL);

            #region "Load ViewBags"
            ViewBag.ProgramDetail = im;

            bool isValidOwner = false;
            if (im.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
            }
            ViewBag.IsValidOwner = isValidOwner;

            ViewBag.IsModelDeploy = im.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690

            //To get permission status for Approve campaign , By dharmraj PL #538
            var lstSubOrdinatesPeers = new List<Guid>();
            try
            {
                lstSubOrdinatesPeers = Common.GetSubOrdinatesWithPeersNLevel();
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool isValidManagerUser = false;
            if (lstSubOrdinatesPeers.Contains(im.OwnerId) && Common.IsSectionApprovable(lstSubOrdinatesPeers, id, Enums.Section.Program.ToString()))////Modified by Sohel Pathan for PL ticket #688 and #689
            {
                isValidManagerUser = true;
            }
            ViewBag.IsValidManagerUser = isValidManagerUser;

            // Start - Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic
            ViewBag.IsCommentsViewEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.CommentsViewEdit);
            if ((bool)ViewBag.IsCommentsViewEditAuthorized == false)
                ViewBag.UnauthorizedCommentSection = Common.objCached.UnauthorizedCommentSection;
            // End - Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic

            #endregion
            // Added by Dharmraj Mangukiya for Deploy to integration button restrictions PL ticket #537


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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            bool IsProgramEditable = false;
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
            bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);

            if (im.OwnerId.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
            {
                IsProgramEditable = true;
            }
            else if (IsPlanEditAllAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                IsProgramEditable = true;
            }
            else if (IsPlanEditSubordinatesAuthorized)
            {
                if (lstSubOrdinates.Contains(im.OwnerId)) // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsProgramEditable = true;
                }
            }

            ViewBag.IsProgramEditable = IsProgramEditable;

            return PartialView("_ReviewProgram");
        }

        #region Budget Allocation in Program Tab
        /// <summary>
        /// Load the Program Budget Allocation values.
        /// </summary>
        /// <param name="id">Program Id</param>
        /// <returns></returns>
        public PartialViewResult LoadSetupProgramBudget(int id = 0)
        {
            Plan_Campaign_Program pcp = db.Plan_Campaign_Program.Where(pcpobj => pcpobj.PlanProgramId.Equals(id) && pcpobj.IsDeleted.Equals(false)).FirstOrDefault();
            if (pcp == null)
            {
                return null;
            }

            #region "Set Plan_Campaign_ProgramModel to pass into partialview"
            Plan_Campaign_ProgramModel pcpm = new Plan_Campaign_ProgramModel();
            pcpm.PlanProgramId = pcp.PlanProgramId;
            pcpm.PlanCampaignId = pcp.PlanCampaignId;

            pcpm.ProgramBudget = pcp.ProgramBudget;

            var objPlan = db.Plans.FirstOrDefault(varP => varP.PlanId == pcp.Plan_Campaign.PlanId);
            pcpm.AllocatedBy = objPlan.AllocatedBy;
            #endregion

            #region "Calculate Plan Remaining Budget"
            var objPlanCampaign = db.Plan_Campaign.FirstOrDefault(c => c.PlanCampaignId == pcp.PlanCampaignId);
            var lstSelectedProgram = db.Plan_Campaign_Program.Where(p => p.PlanCampaignId == pcp.PlanCampaignId && p.IsDeleted == false).ToList();
            double allProgramBudget = lstSelectedProgram.Sum(c => c.ProgramBudget);
            ViewBag.planRemainingBudget = (objPlanCampaign.CampaignBudget - allProgramBudget);
            #endregion

            return PartialView("_SetupProgramBudget", pcpm);
        }

        /// <summary>
        /// Action to Save Program.
        /// </summary>
        /// <param name="form">Form object of Plan_Campaign_ProgramModel.</param>
        /// <param name="BudgetInputValues">Budget allocation inputs values.</param>
        /// <param name="title"></param>
        /// <param name="UserId"></param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveProgramBudgetAllocation(Plan_Campaign_ProgramModel form, string BudgetInputValues, string UserId = "", string title = "")
        {
            //// check whether UserId is loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    return Json(new { IsSaved = false, msg = Common.objCached.LoginWithSameSession, JsonRequestBehavior.AllowGet });
                }
            }
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        //// Get duplicate record.
                        var pcpvar = (from pcp in db.Plan_Campaign_Program
                                      join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                      where pcp.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcp.PlanProgramId.Equals(form.PlanProgramId) && pcp.IsDeleted.Equals(false)
                                      && pc.PlanCampaignId == form.PlanCampaignId   //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
                                      select pcp).FirstOrDefault();
                        //// if duplicate record exist then return with duplication message.
                        if (pcpvar != null)
                        {
                            string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                            return Json(new { IsSaved = false, msg = strDuplicateMessage, JsonRequestBehavior.AllowGet });
                        }
                        else
                        {
                            #region "Update record to Plan_Campaign_Program table"
                            Plan_Campaign_Program pcpobj = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(form.PlanProgramId)).FirstOrDefault();
                            string[] arrBudgetInputValues = BudgetInputValues.Split(',');
                            pcpobj.ModifiedBy = Sessions.User.UserId;
                            pcpobj.ModifiedDate = DateTime.Now;
                            pcpobj.Title = title;
                            pcpobj.ProgramBudget = form.ProgramBudget;
                            #endregion

                            //Start added by Kalpesh  #608: Budget allocation for Program
                            //// Get Previous budget allocation list.
                            var PrevAllocationList = db.Plan_Campaign_Program_Budget.Where(c => c.PlanProgramId == form.PlanProgramId).Select(c => c).ToList();    // Modified by Sohel Pathan on 04/09/2014 for PL ticket #758

                            //// Process for Monthly budget values.
                            if (arrBudgetInputValues.Length == 12)
                            {
                                for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                {
                                    // Start - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
                                    bool isExists = false;
                                    if (PrevAllocationList != null && PrevAllocationList.Count > 0)
                                    {
                                        //// Get previous campaign budget values by Period.
                                        var updatePlanProgramBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (i + 1))).FirstOrDefault();
                                        if (updatePlanProgramBudget != null)
                                        {
                                            if (arrBudgetInputValues[i] != "")
                                            {
                                                //// Update budget value with old value.
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
                                    //// if Old budget value does not exist then insert new value to table.
                                    if (!isExists && arrBudgetInputValues[i] != "")
                                    {
                                        // End - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
                                        Plan_Campaign_Program_Budget objPlanCampaignProgramBudget = new Plan_Campaign_Program_Budget();
                                        objPlanCampaignProgramBudget.PlanProgramId = form.PlanProgramId;
                                        objPlanCampaignProgramBudget.Period = PeriodChar + (i + 1);
                                        objPlanCampaignProgramBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                        objPlanCampaignProgramBudget.CreatedBy = Sessions.User.UserId;
                                        objPlanCampaignProgramBudget.CreatedDate = DateTime.Now;
                                        db.Entry(objPlanCampaignProgramBudget).State = EntityState.Added;
                                    }
                                }
                            }
                            else if (arrBudgetInputValues.Length == 4)  //// Process for Quarterly budget values.
                            {
                                int BudgetInputValuesCounter = 1;
                                for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                {
                                    // Start - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
                                    bool isExists = false;
                                    if (PrevAllocationList != null && PrevAllocationList.Count > 0)
                                    {
                                        //// Get Quarter budget list.
                                        var thisQuartersMonthList = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (BudgetInputValuesCounter)) || pb.Period == (PeriodChar + (BudgetInputValuesCounter + 1)) || pb.Period == (PeriodChar + (BudgetInputValuesCounter + 2))).ToList().OrderBy(a => a.Period).ToList();

                                        //// Get First month values from Quarterly budget list.
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
                                                                thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (BudgetInputValuesCounter + j))).FirstOrDefault();
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
                                    //// if Old budget value does not exist then insert new value to table.
                                    if (!isExists && arrBudgetInputValues[i] != "")
                                    {
                                        // End - Added by Sohel Pathan on 03/09/2014 for PL ticket #758
                                        Plan_Campaign_Program_Budget objPlanCampaignProgramBudget = new Plan_Campaign_Program_Budget();
                                        objPlanCampaignProgramBudget.PlanProgramId = form.PlanProgramId;
                                        objPlanCampaignProgramBudget.Period = PeriodChar + BudgetInputValuesCounter;
                                        objPlanCampaignProgramBudget.Value = Convert.ToDouble(arrBudgetInputValues[i]);
                                        objPlanCampaignProgramBudget.CreatedBy = Sessions.User.UserId;
                                        objPlanCampaignProgramBudget.CreatedDate = DateTime.Now;
                                        db.Entry(objPlanCampaignProgramBudget).State = EntityState.Added;
                                    }
                                    BudgetInputValuesCounter = BudgetInputValuesCounter + 3;
                                }
                            }

                            db.Entry(pcpobj).State = EntityState.Modified;
                            int result = db.SaveChanges();
                            result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanProgramId, pcpobj.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                            scope.Complete();
                            string strMessage = Common.objCached.PlanEntityAllocationUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                            return Json(new { IsSaved = true, msg = strMessage, JsonRequestBehavior.AllowGet, PlanProgramId = form.PlanProgramId, PlanCampaignId = form.PlanCampaignId });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { IsSaved = false, msg = Common.objCached.ErrorOccured, JsonRequestBehavior.AllowGet });
        }

        #endregion

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Create Program.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns Partial View Of Program.</returns>
        public PartialViewResult CreateProgram(int id = 0)
        {
            Plan_Campaign pcp = db.Plan_Campaign.Where(pcpobj => pcpobj.PlanCampaignId.Equals(id) && pcpobj.IsDeleted.Equals(false)).FirstOrDefault();
            if (pcp == null)
            {
                return null;
            }
            //// Set External Integration Service by PlanModel
            var objPlan = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == id).FirstOrDefault().Plan;
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);

            ViewBag.IsCreated = true;
            ViewBag.CampaignTitle = pcp.Title;
            User userName = new User();
            try
            {
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                ViewBag.IsServiceUnavailable = false;
                userName = objBDSUserRepository.GetTeamMemberDetails(Sessions.User.UserId, Sessions.ApplicationId);
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
                    ViewBag.IsServiceUnavailable = true;
                }
            }
            ViewBag.OwnerName = userName.FirstName + " " + userName.LastName;

            #region "Set Plan_Campaign_ProgramModel to pass into Partialview"
            Plan_Campaign_ProgramModel pcpm = new Plan_Campaign_ProgramModel();
            pcpm.PlanCampaignId = id;
            pcpm.IsDeployedToIntegration = false;
            pcpm.StartDate = GetCurrentDateBasedOnPlan();
            pcpm.EndDate = GetCurrentDateBasedOnPlan(true);
            pcpm.CStartDate = pcp.StartDate;
            pcpm.CEndDate = pcp.EndDate;
            pcpm.ProgramBudget = 0;
            pcpm.AllocatedBy = objPlan.AllocatedBy;
            #endregion

            ViewBag.IsOwner = true;
            ViewBag.RedirectType = false;
            ViewBag.Year = db.Plans.Single(plan => plan.PlanId.Equals(Sessions.PlanId)).Year;

            #region "Calculate Plan Remaining Budget"
            var objPlanCampaign = db.Plan_Campaign.FirstOrDefault(c => c.PlanCampaignId == id);
            var lstSelectedProgram = db.Plan_Campaign_Program.Where(p => p.PlanCampaignId == id && p.IsDeleted == false).ToList();
            double allProgramBudget = lstSelectedProgram.Sum(c => c.ProgramBudget);
            ViewBag.planRemainingBudget = (objPlanCampaign.CampaignBudget - allProgramBudget);
            #endregion

            return PartialView("_EditSetupProgram", pcpm);
        }
        #endregion

        #region "Tactic related Functions"
        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Setup Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="Mode"></param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadSetup(int id, string Mode = "View")
        {
            InspectModel _inspetmodel;

            //// Load InspectModel data.
            if (TempData["TacticModel"] != null)
            {
                _inspetmodel = (InspectModel)TempData["TacticModel"];
            }
            else
            {
                _inspetmodel = GetInspectModel(id, Convert.ToString(Enums.Section.Tactic).ToLower(), false);
            }

            try
            {
                _inspetmodel.Owner = Common.GetUserName(_inspetmodel.OwnerId.ToString());
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            ViewBag.TacticDetail = _inspetmodel;
            ViewBag.IsTackticAddEdit = false;

            /* Added by Mitesh Vaishnav for PL ticket #1143
             Add number of stages for advance/Basic attributes waightage related to tacticType*/
            string entityType = Enums.Section.Tactic.ToString();
            /*Get existing value of Advance/Basic waightage of tactic's attributes*/
            string customFieldType=Enums.CustomFieldType.DropDownList.ToString();
            var customFieldEntity = (from customentity in db.CustomField_Entity
                     where customentity.EntityId == id && 
                     customentity.CustomField.EntityType == entityType &&
                     customentity.CustomField.CustomFieldType.Name == customFieldType
                     select customentity).ToList();
            var customfieldids = customFieldEntity.Select(customentity => customentity.CustomFieldId).ToList();
            var customOptionFieldList = db.CustomFieldOptions.Where(option => customfieldids.Contains(option.CustomFieldId)).ToList().Select(option => option.CustomFieldOptionId.ToString()).ToList();

            var customFeildsWeightage = customFieldEntity.Where(cfs => customOptionFieldList.Contains(cfs.Value)).Select(cfs => new
            {
                optionId = cfs.Value,
                CostWeight = cfs.CostWeightage,
                Weight = cfs.Weightage
            }).ToList();
                ViewBag.customFieldWeightage = JsonConvert.SerializeObject(customFeildsWeightage);
            /*End : Added by Mitesh Vaishnav for PL ticket #1143*/

            //// if Mode is "View" or "undefined" then load ReadOnly mode of Setup tab for Tactic Inspect
            if (Mode.ToLower() == "view" || Mode.ToLower() == "undefined")
            {
                ViewBag.IsTackticAddEdit = false;
                return PartialView("SetUp", _inspetmodel);
            }
            else
            {
                ViewBag.IsTackticAddEdit = true;
                return PartialView("SetupEditAdd", _inspetmodel);
            }
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Review Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadReview(int id)
        {
            InspectModel _inspetmodel;
            //// Load InspectModel data.
            if (TempData["TacticModel"] != null)
            {
                _inspetmodel = (InspectModel)TempData["TacticModel"];
            }
            else
            {
                _inspetmodel = GetInspectModel(id, Convert.ToString(Enums.Section.Tactic).ToLower(), false);
            }

            //// Get Tactic comment by PlanCampaignId from Plan_Campaign_Program_Tactic_Comment table.
            var tacticComment = (from tc in db.Plan_Campaign_Program_Tactic_Comment
                                 where tc.PlanTacticId == id
                                 select tc).ToArray();

            //// Get Users list.
            List<Guid> userListId = new List<Guid>();
            userListId = (from ta in tacticComment select ta.CreatedBy).ToList<Guid>();
            userListId.Add(_inspetmodel.OwnerId);
            string userList = string.Join(",", userListId.Select(userid => userid.ToString()).ToArray());
            List<User> userName = new List<User>();

            try
            {
                userName = objBDSUserRepository.GetMultipleTeamMemberDetails(userList, Sessions.ApplicationId);
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
            ViewBag.ReviewModel = (from tc in tacticComment
                                   where (tc.PlanTacticId.HasValue)
                                   select new InspectReviewModel
                                   {
                                       PlanTacticId = Convert.ToInt32(tc.PlanTacticId),
                                       Comment = tc.Comment,
                                       CommentDate = tc.CreatedDate,
                                       CommentedBy = userName.Where(u => u.UserId == tc.CreatedBy).Any() ? userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.FirstName).FirstOrDefault() + " " + userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.LastName).FirstOrDefault() : GameplanIntegrationService,
                                       CreatedBy = tc.CreatedBy
                                   }).ToList();

            //// Get Owner name by OwnerId from Username list.
            var ownername = (from user in userName
                             where user.UserId == _inspetmodel.OwnerId
                             select user.FirstName + " " + user.LastName).FirstOrDefault();
            if (ownername != null)
            {
                _inspetmodel.Owner = ownername.ToString();
            }

            //// Calculate MQL at runtime 
            string TitleMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            int MQLStageLevel = Convert.ToInt32(db.Stages.FirstOrDefault(stage => stage.ClientId == Sessions.User.ClientId && stage.Code == TitleMQL).Level);
            //Compareing MQL stage level with tactic stage level
            if (_inspetmodel.StageLevel < MQLStageLevel)
            {
                ViewBag.ShowMQL = true;
            }
            else
            {
                ViewBag.ShowMQL = false;
            }

            ViewBag.TacticDetail = _inspetmodel;
            ViewBag.IsModelDeploy = _inspetmodel.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690

            bool isValidOwner = false;
            if (_inspetmodel.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
            }
            ViewBag.IsValidOwner = isValidOwner;
            /*Added by Mitesh Vaishnav on 13/06/2014 to address changes related to #498 Customized Target Stage - Publish model*/
            var pcpt = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId.Equals(id)).FirstOrDefault();
            var tacticType = db.TacticTypes.Where(tt => tt.TacticTypeId == pcpt.TacticTypeId).FirstOrDefault();
            if (pcpt.StageId == tacticType.StageId)
            {
                ViewBag.IsDiffrentStageType = false;
            }
            else
            {
                ViewBag.IsDiffrentStageType = true;
            }
            /*End Added by Mitesh Vaishnav on 13/06/2014 to address changes related to #498 Customized Target Stage - Publish model */

            // To get permission status for Approve tactic , By dharmraj PL #538
            var lstSubOrdinatesPeers = new List<Guid>();
            try
            {
                lstSubOrdinatesPeers = Common.GetSubOrdinatesWithPeersNLevel();
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool isValidManagerUser = false;
            if (lstSubOrdinatesPeers.Contains(_inspetmodel.OwnerId))
            {
                isValidManagerUser = true;
            }
            ViewBag.IsValidManagerUser = isValidManagerUser;

            // Start - Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic
            ViewBag.IsCommentsViewEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.CommentsViewEdit);
            if ((bool)ViewBag.IsCommentsViewEditAuthorized == false)
                ViewBag.UnauthorizedCommentSection = Common.objCached.UnauthorizedCommentSection;
            // End - Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic

            // Added by Dharmraj Mangukiya for Deploy to integration button restrictions PL ticket #537
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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool IsDeployToIntegrationVisible = false;
            if (_inspetmodel.OwnerId.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
            {
                IsDeployToIntegrationVisible = true;
            }
            else if (IsPlanEditAllAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                IsDeployToIntegrationVisible = true;
            }
            else if (IsPlanEditSubordinatesAuthorized)
            {
                if (lstSubOrdinates.Contains(_inspetmodel.OwnerId))
                {
                    IsDeployToIntegrationVisible = true;
                }
            }

            ViewBag.IsDeployToIntegrationVisible = IsDeployToIntegrationVisible;
            ////Start : Added by Mitesh Vaishnav for PL ticket #690 Model Interface - Integration
            ViewBag.TacticIntegrationInstance = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId == _inspetmodel.PlanTacticId).FirstOrDefault().IntegrationInstanceTacticId;
            string pullResponses = Operation.Pull_Responses.ToString();
            string pullClosedWon = Operation.Pull_ClosedWon.ToString();
            string pullQualifiedLeads = Operation.Pull_QualifiedLeads.ToString();
            string planEntityType = Enums.Section.Tactic.ToString();
            var planEntityLogList = db.IntegrationInstancePlanEntityLogs.Where(ipt => ipt.EntityId == _inspetmodel.PlanTacticId && ipt.EntityType == planEntityType).OrderByDescending(ipt => ipt.IntegrationInstancePlanLogEntityId).ToList();
            if (planEntityLogList.Where(planLog => planLog.Operation == pullResponses).FirstOrDefault() != null)
            {
                // viewbag which display last synced datetime of tactic for pull responses
                ViewBag.INQLastSync = planEntityLogList.Where(planLog => planLog.Operation == pullResponses).FirstOrDefault().SyncTimeStamp;
            }
            if (planEntityLogList.Where(planLog => planLog.Operation == pullClosedWon).FirstOrDefault() != null)
            {
                // viewbag which display last synced datetime of tactic for pull closed won
                ViewBag.CWLastSync = planEntityLogList.Where(planLog => planLog.Operation == pullClosedWon).FirstOrDefault().SyncTimeStamp;
            }
            if (planEntityLogList.Where(planLog => planLog.Operation == pullQualifiedLeads).FirstOrDefault() != null)
            {
                // viewbag which display last synced datetime of tactic for pull qualified leads
                ViewBag.MQLLastSync = planEntityLogList.Where(planLog => planLog.Operation == pullQualifiedLeads).FirstOrDefault().SyncTimeStamp;
            }
            ////End : Added by Mitesh Vaishnav for PL ticket #690 Model Interface - Integration
            string entityType = Enums.EntityType.Tactic.ToString();
            var topThreeCustomFields = db.CustomFields.Where(cf => cf.IsDefault == true && cf.IsDeleted == false && cf.IsRequired == true && cf.ClientId == Sessions.User.ClientId && cf.EntityType == entityType).Take(3).ToList().Select((cf, Index) => new CustomFieldReviewTab()
            {
                Name = cf.Name,
                Class = "customfield-review" + (Index + 1).ToString(),
                Value = cf.CustomField_Entity.Where(ct => ct.EntityId == id).Select(ct => ct.Value).ToList().Count > 0 ? string.Join(",", cf.CustomFieldOptions.Where(a => cf.CustomField_Entity.Where(ct => ct.EntityId == id).Select(ct => ct.Value).ToList().Contains(a.CustomFieldOptionId.ToString())).Select(a => a.Value).ToList()) : "N/A"
            }).ToList();
            ViewBag.TopThreeCustomFields = topThreeCustomFields;

            return PartialView("Review");
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Actuals Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <returns>Returns Partial View Of Actuals Tab.</returns>
        public ActionResult LoadActuals(int id)
        {
            InspectModel _inspectmodel;
            //// Load InspectModel Data.
            if (TempData["TacticModel"] != null)
            {
                _inspectmodel = (InspectModel)TempData["TacticModel"];
            }
            else
            {
                _inspectmodel = GetInspectModel(id, Convert.ToString(Enums.Section.Tactic).ToLower(), false);
            }

            ViewBag.TacticStageId = _inspectmodel.StageId;
            ViewBag.TacticStageTitle = _inspectmodel.StageTitle;

            List<string> lstStageTitle = new List<string>();
            lstStageTitle = Common.GetTacticStageTitle(id);
            ViewBag.StageTitle = lstStageTitle;

            List<Plan_Campaign_Program_Tactic> tid = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId == id).ToList();
            List<ProjectedRevenueClass> tacticList = Common.ProjectedRevenueCalculateList(tid);
            _inspectmodel.Revenues = Math.Round(tacticList.Where(_tactic => _tactic.PlanTacticId == id).Select(_tactic => _tactic.ProjectedRevenue).FirstOrDefault(), 2); // Modified by Sohel Pathan on 15/09/2014 for PL ticket #760
            tacticList = Common.ProjectedRevenueCalculateList(tid, true);

            string TitleProjectedStageValue = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
            string TitleCW = Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString();
            string TitleMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
            string TitleRevenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();

            ////Modified by Mitesh Vaishnav for PL ticket #695
            var tacticActualList = db.Plan_Campaign_Program_Tactic_Actual.Where(planTacticActuals => planTacticActuals.PlanTacticId == id).ToList();
            _inspectmodel.ProjectedStageValueActual = tacticActualList.Where(planTacticActuals => planTacticActuals.StageTitle == TitleProjectedStageValue).Sum(planTacticActuals => planTacticActuals.Actualvalue);
            _inspectmodel.CWsActual = tacticActualList.Where(planTacticActuals => planTacticActuals.StageTitle == TitleCW).Sum(planTacticActuals => planTacticActuals.Actualvalue);
            _inspectmodel.RevenuesActual = tacticActualList.Where(planTacticActuals => planTacticActuals.StageTitle == TitleRevenue).Sum(planTacticActuals => planTacticActuals.Actualvalue);
            _inspectmodel.MQLsActual = tacticActualList.Where(planTacticActuals => planTacticActuals.StageTitle == TitleMQL).Sum(planTacticActuals => planTacticActuals.Actualvalue);
            _inspectmodel.CWs = Math.Round(tacticList.Where(tl => tl.PlanTacticId == id).Select(tl => tl.ProjectedRevenue).FirstOrDefault(), 1);

            string modifiedBy = string.Empty;
            try
            {
                modifiedBy = Common.TacticModificationMessage(_inspectmodel.PlanTacticId);////Modified by Mitesh Vaishnav for PL ticket #743 Actuals Inspect: User Name for Scheduler Integration
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);

                }
            }

            ViewBag.UpdatedBy = modifiedBy != string.Empty ? modifiedBy : null;////Modified by Mitesh Vaishnav for PL ticket #743 Actuals Inspect: User Name for Scheduler Integration

            // Modified by dharmraj for implement new formula to calculate ROI, #533
            if (_inspectmodel.Cost > 0)
            {
                _inspectmodel.ROI = (_inspectmodel.Revenues - _inspectmodel.Cost) / _inspectmodel.Cost;
            }
            else
                _inspectmodel.ROI = 0;
            ////Start Modified by Mitesh Vaishnav For PL ticket #695
            double tacticCostActual = 0;
            //// Checking whether line item actuals exists.
            var lineItemListActuals = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineitemActual => lineitemActual.Plan_Campaign_Program_Tactic_LineItem.PlanTacticId == id &&
                                                                                            lineitemActual.Plan_Campaign_Program_Tactic_LineItem.IsDeleted == false)
                                                                                            .ToList();
            if (lineItemListActuals.Count != 0)
            {
                tacticCostActual = lineItemListActuals.Sum(lineitemActual => lineitemActual.Value);
            }
            else
            {
                ////If line item actual is not exist for tactic than cost actual will be total of tactic cost actual
                string costStageTitle = Enums.InspectStage.Cost.ToString();
                var tacticActualCostList = tacticActualList.Where(tacticActual => tacticActual.StageTitle == costStageTitle).ToList();
                if (tacticActualCostList.Count != 0)
                {
                    tacticCostActual = tacticActualCostList.Sum(tacticActual => tacticActual.Actualvalue);
                }
            }
            if (tacticCostActual > 0)
            {
                _inspectmodel.ROIActual = (_inspectmodel.RevenuesActual - tacticCostActual) / tacticCostActual;
            }
            else
            {
                _inspectmodel.ROIActual = 0;
            }
            ////End Modified by Mitesh Vaishnav For PL ticket #695
            ViewBag.TacticDetail = _inspectmodel;
            bool isValidUser = true;
            if (_inspectmodel.OwnerId != Sessions.User.UserId) isValidUser = false;
            ViewBag.IsValidUser = isValidUser;

            ViewBag.IsModelDeploy = _inspectmodel.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690
            if (_inspectmodel.LastSyncDate != null)
            {
                ViewBag.LastSync = "Last synced with integration " + Common.GetFormatedDate(_inspectmodel.LastSyncDate) + ".";////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
            }
            else
            {
                ViewBag.LastSync = string.Empty;
            }

            ViewBag.LineItemList = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.PlanTacticId == id && lineitem.IsDeleted == false).OrderByDescending(lineitem => lineitem.LineItemTypeId).ToList();
            return PartialView("Actual");
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Actual Value Of Tactic.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <returns>Returns Json Result of tactic Actual Value.</returns>
        public JsonResult GetActualTacticData(int id)
        {
            var dtTactic = (from tacActual in db.Plan_Campaign_Program_Tactic_Actual
                            where tacActual.PlanTacticId == id
                            select new { tacActual.CreatedBy, tacActual.CreatedDate }).FirstOrDefault();
            var lineItemIds = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == id && lineItem.IsDeleted == false).Select(lineItem => lineItem.PlanLineItemId).ToList();
            var dtlineItemActuals = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lineActual => lineItemIds.Contains(lineActual.PlanLineItemId)).ToList();
            if (dtTactic != null || dtlineItemActuals != null)
            {
                var ActualData = db.Plan_Campaign_Program_Tactic_Actual.Where(pcpta => pcpta.PlanTacticId == id).Select(tacActual => new
                {
                    id = tacActual.PlanTacticId,
                    stageTitle = tacActual.StageTitle,
                    period = tacActual.Period,
                    actualValue = tacActual.Actualvalue
                });

                ////// start-Added by Mitesh Vaishnav for PL ticket #571
                //// Actual cost portion added exact under "lstMonthly" array because Actual cost portion is independent from the monthly/quarterly selection made by the user at the plan level.
                bool isLineItemForTactic = false;////flag for line items count of tactic.If tactic has any line item than flag set to true else false
                if (lineItemIds.Count == 0)
                {
                    var objBudgetAllocationData = new { actualData = ActualData, IsLineItemForTactic = isLineItemForTactic };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ////object for filling input of Actual Cost Allocation
                    var LineItemactualCost = dtlineItemActuals.Select(al => new
                    {
                        PlanLineItemId = al.PlanLineItemId,
                        Period = al.Period,
                        Value = al.Value,
                        Title = al.Plan_Campaign_Program_Tactic_LineItem.Title
                    }).ToList();
                    isLineItemForTactic = true;
                    var objBudgetAllocationData = new { actualData = ActualData, ActualCostAllocationData = LineItemactualCost, IsLineItemForTactic = isLineItemForTactic };
                    return Json(objBudgetAllocationData, JsonRequestBehavior.AllowGet);
                }
                //// End-Added by Mitesh Vaishnav for PL ticket #571

            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to UpdateResult.
        /// </summary>
        /// <param name="tacticactual">List of InspectActual.</param>
        /// <param name="lineItemActual"></param>
        /// <param name="tactictitle"></param>
        /// <param name="UserId"></param>
        /// <returns>Returns JsonResult.</returns>
        [HttpPost]
        public JsonResult UploadResult(List<InspectActual> tacticactual, List<Plan_Campaign_Program_Tactic_LineItem_Actual> lineItemActual, string UserId = "", string tactictitle = "")
        {
            bool isLineItemForTactic = false;
            //// check whether UserId is current loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnURL = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
            }
            try
            {
                if (tacticactual != null)
                {
                    var actualResult = (from tacActual in tacticactual
                                        select new { tacActual.PlanTacticId, tacActual.TotalProjectedStageValueActual, tacActual.TotalMQLActual, tacActual.TotalCWActual, tacActual.TotalRevenueActual, tacActual.TotalCostActual, tacActual.ROI, tacActual.ROIActual, tacActual.IsActual }).FirstOrDefault();
                    var objpcpt = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId == actualResult.PlanTacticId).FirstOrDefault();

                    //// Get Tactic duplicate record.
                    var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                  join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                  join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                  where pcpt.Title.Trim().ToLower().Equals(tactictitle.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(actualResult.PlanTacticId) && pcpt.IsDeleted.Equals(false)
                                  && pcp.PlanProgramId == objpcpt.PlanProgramId
                                  select pcp).FirstOrDefault();

                    //// if duplicate record exist then return duplication message.
                    if (pcpvar != null)
                    {
                        string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                        return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage });
                    }
                    else
                    {
                        #region " If Duplicate name does not exist"
                        if (lineItemActual != null && lineItemActual.Count > 0)
                        {
                            lineItemActual.ForEach(al => { al.CreatedBy = Sessions.User.UserId; al.CreatedDate = DateTime.Now; });
                            var lstLineItemIds = lineItemActual.Select(al => al.PlanLineItemId).Distinct().ToList();
                            var prevlineItemActual = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(al => lstLineItemIds.Contains(al.PlanLineItemId)).ToList();
                            prevlineItemActual.ForEach(al => db.Entry(al).State = EntityState.Deleted);
                            lineItemActual.ForEach(al => db.Entry(al).State = EntityState.Added);
                            lineItemActual.ForEach(al => db.Plan_Campaign_Program_Tactic_LineItem_Actual.Add(al));
                            isLineItemForTactic = true;

                        }
                        if (isLineItemForTactic)
                        {
                            tacticactual = tacticactual.Where(ta => ta.StageTitle != Enums.InspectStage.Cost.ToString()).ToList();
                        }
                        if (tacticactual != null)
                        {
                            bool isMQL = false; // Tactic stage is MQL or not
                            string inspectStageMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
                            int stageId = tacticactual[0].StageId;
                            var objStage = db.Stages.FirstOrDefault(s => s.StageId == stageId);
                            if (objStage.Code == inspectStageMQL)
                            {
                                isMQL = true;
                            }

                            using (MRPEntities mrp = new MRPEntities())
                            {
                                using (var scope = new TransactionScope())
                                {
                                    //modified by Mitesh vaishnav for functional review point - removing sp
                                    var tacticActualList = db.Plan_Campaign_Program_Tactic_Actual.Where(ta => ta.PlanTacticId == actualResult.PlanTacticId).ToList();
                                    tacticActualList.ForEach(ta => db.Entry(ta).State = EntityState.Deleted);

                                    //Added By : Kalpesh Sharma #735 Actual cost - Changes to add actuals screen 
                                    List<int> tacticLineItemActualList = db.Plan_Campaign_Program_Tactic_LineItem.Where(ta => ta.PlanTacticId == actualResult.PlanTacticId).ToList().Select(a => a.PlanLineItemId).ToList();
                                    var deleteMarkedLineItem = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(c => tacticLineItemActualList.Contains(c.PlanLineItemId)).ToList();
                                    deleteMarkedLineItem.ForEach(ta => db.Entry(ta).State = EntityState.Deleted);

                                    //Added By : Kalpesh Sharma #735 Actual cost - Changes to add actuals screen 
                                    Int64 projectedStageValue = 0, mql = 0, cw = 0, cost = 0;
                                    double revenue = 0;

                                    //// If Actuals value exist then save Actuals values.
                                    if (actualResult.IsActual)
                                    {
                                        if (isMQL)
                                        {
                                            foreach (var t in tacticactual)
                                            {
                                                if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString())
                                                {
                                                    Plan_Campaign_Program_Tactic_Actual objpcpta = new Plan_Campaign_Program_Tactic_Actual();
                                                    objpcpta.PlanTacticId = t.PlanTacticId;
                                                    objpcpta.StageTitle = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
                                                    objpcpta.Period = t.Period;
                                                    objpcpta.Actualvalue = t.ActualValue;
                                                    objpcpta.CreatedDate = DateTime.Now;
                                                    objpcpta.CreatedBy = Sessions.User.UserId;
                                                    db.Entry(objpcpta).State = EntityState.Added;
                                                    db.Plan_Campaign_Program_Tactic_Actual.Add(objpcpta);
                                                }
                                            }
                                        }
                                        //// Save Tactic Actuals values.
                                        foreach (var t in tacticactual)
                                        {
                                            //Added By : Kalpesh Sharma #735 Actual cost - Changes to add actuals screen 
                                            if (t.StageTitle != Enums.InspectStage.ProjectedStageValue.ToString() &&
                                                t.StageTitle != Enums.InspectStage.MQL.ToString() &&
                                                t.StageTitle != Enums.InspectStage.CW.ToString() &&
                                                t.StageTitle != Enums.InspectStage.Revenue.ToString() &&
                                                t.StageTitle != Enums.InspectStage.Cost.ToString() &&
                                                t.StageTitle != Enums.InspectStage.INQ.ToString())
                                            {
                                                //Added By : Kalpesh Sharma #735 Actual cost - Changes to add actuals screen 
                                                // If stage title is number and not matched up with the pre define stages then save data in Plan_Campaign_Program_Tactic_LineItem_Actual
                                                SaveActualLineItem(t);
                                            }
                                            else
                                            {
                                                Plan_Campaign_Program_Tactic_Actual objpcpta = new Plan_Campaign_Program_Tactic_Actual();
                                                objpcpta.PlanTacticId = t.PlanTacticId;
                                                objpcpta.StageTitle = t.StageTitle;
                                                if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString()) projectedStageValue += t.ActualValue;
                                                if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString()) mql += t.ActualValue;
                                                if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString()) cw += t.ActualValue;
                                                if (t.StageTitle == Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString()) revenue += t.ActualValue;
                                                if (t.StageTitle == Enums.InspectStage.Revenue.ToString()) cost += t.ActualValue;

                                                objpcpta.Period = t.Period;
                                                objpcpta.Actualvalue = t.ActualValue;
                                                objpcpta.CreatedDate = DateTime.Now;
                                                objpcpta.CreatedBy = Sessions.User.UserId;
                                                db.Entry(objpcpta).State = EntityState.Added;
                                                db.Plan_Campaign_Program_Tactic_Actual.Add(objpcpta);
                                            }
                                        }
                                    }
                                    db.SaveChanges();

                                    Plan_Campaign_Program_Tactic objPCPT = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId == actualResult.PlanTacticId).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(tactictitle)) // Added by Viral kadiya on 11/12/2014 to update tactic title for PL ticket #946.
                                        objPCPT.Title = tactictitle;
                                    objPCPT.ModifiedBy = Sessions.User.UserId;
                                    objPCPT.ModifiedDate = DateTime.Now;
                                    db.Entry(objPCPT).State = EntityState.Modified;
                                    int result = db.SaveChanges();
                                    result = Common.InsertChangeLog(Sessions.PlanId, null, actualResult.PlanTacticId, objPCPT.Title, Enums.ChangeLog_ComponentType.tacticresults, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                    scope.Complete();
                                    string strMessage = Common.objCached.PlanEntityActualsUpdated.Replace("{0}", Enums.PlanEntity.Tactic.ToString());    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    return Json(new { id = actualResult.PlanTacticId, TabValue = "Actuals", msg = strMessage });
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { id = 0 });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Create Tactic.
        /// </summary>
        /// <param name="id">Tactic Id.</param>
        /// <param name="RedirectType">Redirect Type</param>
        /// <param name="CalledFromBudget"></param>
        /// <returns>Returns Partial View Of Tactic.</returns>
        public ActionResult EditTactic(int id = 0, string RedirectType = "", string CalledFromBudget = "")
        {
            Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(pcptobj => pcptobj.PlanTacticId.Equals(id) && pcptobj.IsDeleted == false).FirstOrDefault();
            if (pcpt == null)
                return null;

            int planId = pcpt.Plan_Campaign_Program.Plan_Campaign.PlanId;

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

            List<TacticType> tblTacticTypes = db.TacticTypes.Where(tactype => tactype.IsDeleted == null || tactype.IsDeleted == false).ToList();
            //// Get those Tactic types whose ModelId exist in Plan table and IsDeployedToModel = true.
            var lstTactic = from tacType in tblTacticTypes
                            join _plan in db.Plans on tacType.ModelId equals _plan.ModelId
                            where _plan.PlanId == planId && tacType.IsDeployedToModel == true
                            orderby tacType.Title
                            select tacType;

            //// Check whether current TacticId related TacticType exist or not.
            if (!lstTactic.Any(tacType => tacType.TacticTypeId == pcpt.TacticTypeId))
            {
                //// Get list of Tactic Types based on PlanID.
                var tacticTypeSpecial = from _tacType in tblTacticTypes
                                        join _plan in db.Plans on _tacType.ModelId equals _plan.ModelId
                                        where (_plan.PlanId == planId || _plan.PlanId == Sessions.PlanId) && _tacType.TacticTypeId == pcpt.TacticTypeId
                                        orderby _tacType.Title
                                        select _tacType;
                lstTactic = lstTactic.Concat<TacticType>(tacticTypeSpecial);
                lstTactic = lstTactic.OrderBy(a => a.Title);
            }
            ViewBag.IsTacticAfterApproved = Common.CheckAfterApprovedStatus(pcpt.Status);


            foreach (var item in lstTactic)
                item.Title = HttpUtility.HtmlDecode(item.Title);

            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(pcpt.TacticType.Model);

            /* Added by Mitesh Vaishnav for PL ticket #1073
             Add number of stages for advance/Basic attributes waightage related to tacticType*/
            string entityType = Enums.Section.Tactic.ToString();
            /*Get existing value of Advance/Basic waightage of tactic's attributes*/
            string customFieldType = Enums.CustomFieldType.DropDownList.ToString();
            var customFeildsWeightage = db.CustomField_Entity.Where(cfs => cfs.EntityId == pcpt.PlanTacticId && cfs.CustomField.EntityType == entityType && cfs.CustomField.CustomFieldType.Name==customFieldType).Select(cfs => new
            {
                optionId = cfs.Value,
                CostWeight=cfs.CostWeightage,
                Weight = cfs.Weightage
            }).ToList();
                ViewBag.customFieldWeightage = JsonConvert.SerializeObject(customFeildsWeightage);
            
            /*End : Added by Mitesh Vaishnav for PL ticket #1073*/

            Inspect_Popup_Plan_Campaign_Program_TacticModel ippctm = new Inspect_Popup_Plan_Campaign_Program_TacticModel();
            ippctm.PlanProgramId = pcpt.PlanProgramId;
            ippctm.ProgramTitle = HttpUtility.HtmlDecode(pcpt.Plan_Campaign_Program.Title);
            ippctm.PlanCampaignId = pcpt.Plan_Campaign_Program.Plan_Campaign.PlanCampaignId;
            ippctm.CampaignTitle = HttpUtility.HtmlDecode(pcpt.Plan_Campaign_Program.Plan_Campaign.Title);
            ippctm.PlanTacticId = pcpt.PlanTacticId;
            ippctm.TacticTitle = HttpUtility.HtmlDecode(pcpt.Title);
            ippctm.TacticTypeId = pcpt.TacticTypeId;
            ippctm.Description = HttpUtility.HtmlDecode(pcpt.Description);
            ippctm.OwnerId = pcpt.CreatedBy;
            ippctm.StartDate = pcpt.StartDate;
            ippctm.EndDate = pcpt.EndDate;
            ippctm.PStartDate = pcpt.Plan_Campaign_Program.StartDate;
            ippctm.PEndDate = pcpt.Plan_Campaign_Program.EndDate;
            ippctm.CStartDate = pcpt.Plan_Campaign_Program.Plan_Campaign.StartDate;
            ippctm.CEndDate = pcpt.Plan_Campaign_Program.Plan_Campaign.EndDate;
            //User userName = new User();
            try
            {
                ippctm.Owner = Common.GetUserName(pcpt.CreatedBy.ToString());
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            //Updated By Bhavesh Dobariya Performance Issue
            TacticStageValue varTacticStageValue = Common.GetTacticStageRelationForSingleTactic(pcpt, false);
            List<Stage> tblStage = db.Stages.Where(stg => stg.IsDeleted.Equals(false)).ToList();
            //// Set MQL
            string stageMQL = Enums.Stage.MQL.ToString();
            int levelMQL = tblStage.Single(s => s.ClientId.Equals(Sessions.User.ClientId) && s.Code.Equals(stageMQL)).Level.Value;
            int tacticStageLevel = Convert.ToInt32(pcpt.Stage.Level);
            if (tacticStageLevel < levelMQL)
            {
                ippctm.MQLs = varTacticStageValue.MQLValue;
            }
            else if (tacticStageLevel == levelMQL)
            {
                ippctm.MQLs = Convert.ToDouble(pcpt.ProjectedStageValue);
            }
            else if (tacticStageLevel > levelMQL)
            {
                ippctm.MQLs = 0;
                TempData["TacticMQL"] = "N/A";
            }
            ippctm.MQLs = Math.Round((double)ippctm.MQLs, 0, MidpointRounding.AwayFromZero);
            //// Set Revenue
            ippctm.Revenue = Math.Round(varTacticStageValue.RevenueValue, 2);

            string statusAllocatedByNone = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower();
            string statusAllocatedByDefault = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();
            double budgetAllocation = db.Plan_Campaign_Program_Tactic_Cost.Where(_tacCost => _tacCost.PlanTacticId == id).ToList().Sum(_tacCost => _tacCost.Value);

            ippctm.Cost = (pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == pcpt.PlanTacticId && lineItem.IsDeleted == false)).Count() > 0
                && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy != statusAllocatedByNone && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy != statusAllocatedByDefault
                ?
                (budgetAllocation > 0 ? budgetAllocation : (pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == pcpt.PlanTacticId && s.IsDeleted == false)).Sum(a => a.Cost))
                : pcpt.Cost;

            ippctm.IsDeployedToIntegration = pcpt.IsDeployedToIntegration;
            ippctm.StageId = Convert.ToInt32(pcpt.StageId);
            ippctm.StageTitle = tblStage.FirstOrDefault(varS => varS.StageId == pcpt.StageId).Title;
            ippctm.ProjectedStageValue = Convert.ToDouble(pcpt.ProjectedStageValue);

            var modelTacticStageType = lstTactic.Where(_tacType => _tacType.TacticTypeId == pcpt.TacticTypeId).FirstOrDefault().StageId;
            var plantacticStageType = pcpt.StageId;
            if (modelTacticStageType == plantacticStageType)
            {
                ViewBag.IsDiffrentStageType = false;
            }
            else
            {
                ViewBag.IsDiffrentStageType = true;
            }

            if (Sessions.User.UserId == pcpt.CreatedBy)
            {
                ViewBag.IsOwner = true;
            }
            else
            {
                ViewBag.IsOwner = false;
            }

            List<TacticType> tnewList = lstTactic.ToList();
            //// if lstTactics contains the same Title tactic then remove from the new Tactic list.
            TacticType tobj = tblTacticTypes.Where(_tacType => _tacType.TacticTypeId == ippctm.TacticTypeId).FirstOrDefault();
            if (tobj != null)
            {
                TacticType tSameExist = tnewList.Where(_newTacType => _newTacType.Title.Equals(tobj.Title)).FirstOrDefault();
                //// if same Title exist then remove that TacticType from New Tactic list.
                if (tSameExist != null)
                    tnewList.Remove(tSameExist);
                tnewList.Add(tobj);
            }

            ViewBag.Tactics = tnewList.OrderBy(t => t.Title);
            ViewBag.Year = pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.Year;
            ippctm.TacticCost = pcpt.Cost;
            ippctm.AllocatedBy = pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy;

            #region "Calculate Plan remaining budget"
            var CostTacticsBudget = db.Plan_Campaign_Program_Tactic.Where(c => c.PlanProgramId == pcpt.PlanProgramId).ToList().Sum(c => c.Cost);
            double? objPlanCampaignProgram = db.Plan_Campaign_Program.FirstOrDefault(p => p.PlanProgramId == pcpt.PlanProgramId).ProgramBudget;
            objPlanCampaignProgram = objPlanCampaignProgram != null ? objPlanCampaignProgram : 0;
            ViewBag.planRemainingBudget = (objPlanCampaignProgram - (!string.IsNullOrEmpty(Convert.ToString(CostTacticsBudget)) ? CostTacticsBudget : 0));
            #endregion

            // Start - Added by Sohel Pathan on 14/11/2014 for PL ticket #708
            ViewBag.IsTackticAddEdit = true;
            try
            {
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId);
                if (lstClientUsers.Count() > 0)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    ViewBag.IsServiceUnavailable = false;
                    BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();

                    string strUserList = string.Join(",", lstClientUsers);
                    List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberName(strUserList);
                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        var lstPreparedOwners = lstUserDetails.Select(user => new { UserId = user.UserId, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();
                        ViewBag.OwnerList = lstPreparedOwners;
                    }
                    else
                    {
                        ViewBag.OwnerList = new List<User>();
                    }
                }
                else
                {
                    ViewBag.OwnerList = new List<User>();
                }
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
                    ViewBag.IsServiceUnavailable = true;
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            // End - Added by Sohel Pathan on 14/11/2014 for PL ticket #708

            return PartialView("SetupEditAdd", ippctm);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Tactic.
        /// </summary>
        /// <param name="form">Form object of Plan_Campaign_Program_TacticModel.</param>
        /// <param name="lineitems"></param>
        /// <param name="closedTask"></param>
        /// <param name="customFieldInputs"></param>
        /// <param name="UserId"></param>
        /// <param name="strDescription"></param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SetupSaveTactic(Inspect_Popup_Plan_Campaign_Program_TacticModel form, string lineitems, string closedTask, string customFieldInputs, string UserId = "", string strDescription = "", bool resubmission = false)
        {
            //// check whether UserId is current loggined user or not.
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
                int cid = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == form.PlanProgramId).Select(program => program.PlanCampaignId).FirstOrDefault();
                int pid = form.PlanProgramId;
                var customFields = JsonConvert.DeserializeObject<List<CustomFieldStageWeight>>(customFieldInputs);

                //// if PlanTacticId is null then Insert New record.
                if (form.PlanTacticId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// Get Duplicate record to check duplication
                            var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                          join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pcpt.Title.Trim().ToLower().Equals(form.TacticTitle.Trim().ToLower()) && pcpt.IsDeleted.Equals(false)
                                          && pcp.PlanProgramId == form.PlanProgramId
                                          select pcp).FirstOrDefault();

                            //// if duplicate record exist then return duplication message.
                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { IsDuplicate = true, errormsg = strDuplicateMessage, planCampaignId = cid, planProgramId = pid });
                            }
                            else
                            {
                                #region "Save New record to Plan_Campaign_Program_Tactic table"
                                Plan_Campaign_Program_Tactic pcpobj = new Plan_Campaign_Program_Tactic();
                                pcpobj.PlanProgramId = form.PlanProgramId;
                                pcpobj.Title = form.TacticTitle;
                                pcpobj.TacticTypeId = form.TacticTypeId;
                                pcpobj.Description = form.Description;
                                pcpobj.Cost = form.Cost;
                                pcpobj.StartDate = form.StartDate;
                                pcpobj.EndDate = form.EndDate;
                                pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString();
                                pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                pcpobj.StageId = form.StageId;
                                pcpobj.ProjectedStageValue = form.ProjectedStageValue;
                                pcpobj.CreatedBy = Sessions.User.UserId;
                                pcpobj.CreatedDate = DateTime.Now;
                                db.Entry(pcpobj).State = EntityState.Added;
                                int result = db.SaveChanges();
                                #endregion
                                int tacticId = pcpobj.PlanTacticId;

                                //// Insert LineItem for the Tactic.
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

                                #region "Update Start & End Date for planCampaignProgramDetails table"
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
                                #endregion

                                //// Save custom fields value for particular Tactic
                                if (customFields.Count != 0)
                                {

                                    foreach (var item in customFields)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();
                                        objcustomFieldEntity.EntityId = tacticId;
                                        objcustomFieldEntity.CustomFieldId = item.CustomFieldId;
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                            objcustomFieldEntity.Weightage = (byte)item.Weight;
                                        objcustomFieldEntity.CostWeightage = (byte)item.CostWeight;
                                        db.Entry(objcustomFieldEntity).State = EntityState.Added;
                                    }
                                }

                                db.SaveChanges();

                                result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);

                                // Check whether the lineitmes is empty or not . if lineitems have any instance at that time call the SaveLineItems function and insert the data into the lineItems table. 
                                if (lineitems != string.Empty)
                                {
                                    result = SaveLineItems(form, lineitems, tacticId);
                                }

                                if (result >= 1)
                                {
                                    Common.ChangeProgramStatus(pcpobj.PlanProgramId);
                                    var PlanCampaignId = db.Plan_Campaign_Program.Where(a => a.IsDeleted.Equals(false) && a.PlanProgramId == pcpobj.PlanProgramId).Select(a => a.PlanCampaignId).Single();
                                    Common.ChangeCampaignStatus(PlanCampaignId);

                                    scope.Complete();
                                    string strMessag = Common.objCached.PlanEntityCreated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);   // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    return Json(new { IsDuplicate = false, redirect = Url.Action("LoadSetup", new { id = form.PlanTacticId }), Msg = strMessag, planTacticId = pcpobj.PlanTacticId, planCampaignId = cid, planProgramId = pid });
                                }
                            }
                        }
                    }
                }
                else    //// Update record for Tactic
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// Get Duplicate record to check duplication
                            var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                          join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                          join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                          where pcpt.Title.Trim().ToLower().Equals(form.TacticTitle.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(form.PlanTacticId) && pcpt.IsDeleted.Equals(false)
                                          && pcp.PlanProgramId == form.PlanProgramId    //// Added by :- Sohel Pathan on 23/05/2014 for PL ticket #448 to be able to edit Tactic/Program Title while duplicating.
                                          select pcp).FirstOrDefault();

                            //// if duplicate record exist then return duplication message.
                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { IsDuplicate = true, redirect = Url.Action("LoadSetup", new { id = form.PlanTacticId }), errormsg = strDuplicateMessage });
                            }
                            else
                            {
                                #region "Variable Initialize"
                                bool isReSubmission = false;
                                bool isOwner = false;
                                string status = string.Empty;
                                #endregion
                                //Start - Added by Mitesh Vaishnav for PL ticket #1137
                                if (resubmission)
                                {
                                    isReSubmission = true;
                                }

                                Plan_Campaign_Program_Tactic pcpobj = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(form.PlanTacticId)).FirstOrDefault();
                                if (pcpobj.CreatedBy == Sessions.User.UserId) isOwner = true;

                                pcpobj.Title = form.TacticTitle;
                                status = pcpobj.Status;
                                pcpobj.Description = form.Description;
                                Guid oldOwnerId = pcpobj.CreatedBy;
                                //Start - Added by Mitesh Vaishnav - Remove old resubmission condition and combine it for PL ticket #1137
                                pcpobj.TacticTypeId = form.TacticTypeId;
                                pcpobj.CreatedBy = form.OwnerId;
                                pcpobj.ProjectedStageValue = form.ProjectedStageValue;
                                //End - Added by Mitesh Vaishnav - Remove old resubmission condition and combine it



                                DateTime todaydate = DateTime.Now;

                                /// Modified by:   Dharmraj
                                /// Modified date: 2-Sep-2014
                                /// Purpose:       #625 Changing the dates on an approved tactic needs to go through the approval process
                                // To check whether status is Approved or not
                                if (Common.CheckAfterApprovedStatus(pcpobj.Status))
                                {
                                    // Modified by Mitesh Vaishnav for PL ticket #1137 - Add resubmission flag in if condition
                                    // If any changes in start/end dates then tactic will go through the approval process
                                    if (!isReSubmission && pcpobj.EndDate == form.EndDate && pcpobj.StartDate == form.StartDate)
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

                                #region "Set pcobj Start & End Date."
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
                                #endregion

                                //// check that Tactic cost count greater than 0 OR Plan's AllocatedBy is None or Defaults.
                                if ((db.Plan_Campaign_Program_Tactic_Cost.Where(_tacCost => _tacCost.PlanTacticId == form.PlanTacticId).ToList()).Count() == 0 ||
                                    pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower()
                                    || pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                                {
                                    pcpobj.Cost = form.Cost;
                                }

                                pcpobj.IsDeployedToIntegration = form.IsDeployedToIntegration;
                                pcpobj.StageId = form.StageId;
                                pcpobj.ProjectedStageValue = form.ProjectedStageValue;
                                pcpobj.ModifiedBy = Sessions.User.UserId;
                                pcpobj.ModifiedDate = DateTime.Now;
                                db.Entry(pcpobj).State = EntityState.Modified;

                                //Start by Kalpesh Sharma #605: Cost allocation for Tactic
                                var PrevAllocationList = db.Plan_Campaign_Program_Tactic_Cost.Where(_tacCost => _tacCost.PlanTacticId == form.PlanTacticId).Select(_tacCost => _tacCost).ToList();  // Modified by Sohel Pathan on 04/09/2014 for PL ticket #759

                                int result;
                                if (Common.CheckAfterApprovedStatus(pcpobj.Status))
                                {
                                    result = Common.InsertChangeLog(Sessions.PlanId, null, pcpobj.PlanTacticId, pcpobj.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                }
                                if (isReSubmission && Common.CheckAfterApprovedStatus(status) && isOwner)
                                {
                                    pcpobj.Status = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                    //// Get URL for Tactic to send in Email.
                                    string strURL = GetNotificationURLbyStatus(pcpobj.Plan_Campaign_Program.Plan_Campaign.PlanId, form.PlanTacticId, Enums.Section.Tactic.ToString().ToLower());
                                    Common.mailSendForTactic(pcpobj.PlanTacticId, pcpobj.Status, pcpobj.Title, section: Convert.ToString(Enums.Section.Tactic).ToLower(), URL: strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                }
                                result = db.SaveChanges();
                                // Start - Added by Sohel Pathan on 14/11/2014 for PL ticket #708
                                if (result > 0)
                                {
                                    //Send Email Notification For Owner changed.
                                    if (form.OwnerId != oldOwnerId && form.OwnerId != Guid.Empty)
                                    {
                                        if (Sessions.User != null)
                                        {
                                            List<string> lstRecepientEmail = new List<string>();
                                            List<User> UsersDetails = new List<BDSService.User>();
                                            var csv = string.Concat(form.OwnerId.ToString(), ",", oldOwnerId.ToString(), ",", Sessions.User.UserId.ToString());

                                            try
                                            {
                                                UsersDetails = objBDSUserRepository.GetMultipleTeamMemberDetails(csv, Sessions.ApplicationId);
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
                                                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            var NewOwner = UsersDetails.Where(u => u.UserId == form.OwnerId).Select(u => u).FirstOrDefault();
                                            var ModifierUser = UsersDetails.Where(u => u.UserId == Sessions.User.UserId).Select(u => u).FirstOrDefault();
                                            if (NewOwner.Email != string.Empty)
                                            {
                                                lstRecepientEmail.Add(NewOwner.Email);
                                            }
                                            string NewOwnerName = NewOwner.FirstName + " " + NewOwner.LastName;
                                            string ModifierName = ModifierUser.FirstName + " " + ModifierUser.LastName;
                                            string PlanTitle = pcpobj.Plan_Campaign_Program.Plan_Campaign.Plan.Title.ToString();
                                            string CampaignTitle = pcpobj.Plan_Campaign_Program.Plan_Campaign.Title.ToString();
                                            string ProgramTitle = pcpobj.Plan_Campaign_Program.Title.ToString();
                                            if (lstRecepientEmail.Count > 0)
                                            {
                                                string strURL = GetNotificationURLbyStatus(pcpobj.Plan_Campaign_Program.Plan_Campaign.PlanId, form.PlanTacticId, Enums.Section.Tactic.ToString().ToLower());
                                                Common.SendNotificationMailForTacticOwnerChanged(lstRecepientEmail.ToList<string>(), NewOwnerName, ModifierName, pcpobj.Title, ProgramTitle, CampaignTitle, PlanTitle, strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                            }
                                        }
                                    }
                                }
                                // End - Added by Sohel Pathan on 14/11/2014 for PL ticket #708
                                // Start Added by dharmraj for ticket #644

                                //// Calculate TotalLineItem cost.
                                var objtotalLineitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == pcpobj.PlanTacticId && lineItem.LineItemTypeId != null && lineItem.IsDeleted == false);
                                double totalLineitemCost = 0;
                                if (objtotalLineitemCost != null && objtotalLineitemCost.Count() > 0)
                                    totalLineitemCost = objtotalLineitemCost.Sum(l => l.Cost);

                                var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(lineItem => lineItem.PlanTacticId == pcpobj.PlanTacticId && lineItem.Title == Common.DefaultLineItemTitle && lineItem.LineItemTypeId == null);
                                if (pcpobj.Cost > totalLineitemCost)
                                {
                                    double diffCost = pcpobj.Cost - totalLineitemCost;
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

                                ////Start modified by Mitesh Vaishnav for PL ticket #1073 multiselect attributes
                                //// delete previous custom field values and save modified custom fields value for particular Tactic
                                string entityTypeTactic = Enums.EntityType.Tactic.ToString();
                                var prevCustomFieldList = db.CustomField_Entity.Where(custField => custField.EntityId == pcpobj.PlanTacticId && custField.CustomField.EntityType == entityTypeTactic).ToList();
                                prevCustomFieldList.ForEach(custField => db.Entry(custField).State = EntityState.Deleted);

                                if (customFields.Count != 0)
                                {
                                    foreach (var item in customFields)
                                    {
                                        CustomField_Entity objcustomFieldEntity = new CustomField_Entity();

                                        objcustomFieldEntity.EntityId = pcpobj.PlanTacticId;
                                        objcustomFieldEntity.CustomFieldId = item.CustomFieldId;
                                        objcustomFieldEntity.Value = item.Value.Trim().ToString();
                                        objcustomFieldEntity.CreatedDate = DateTime.Now;
                                        objcustomFieldEntity.CreatedBy = Sessions.User.UserId;
                                            objcustomFieldEntity.Weightage = (byte)item.Weight;
                                        objcustomFieldEntity.CostWeightage = (byte)item.CostWeight;
                                        db.CustomField_Entity.Add(objcustomFieldEntity);
                                    }
                                }
                                ////End modified by Mitesh Vaishnav for PL ticket #1073 multiselect attributes

                                db.SaveChanges();

                                if (result >= 1)
                                {
                                    //// Start - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                    Common.ChangeProgramStatus(pcpobj.PlanProgramId);
                                    var PlanCampaignId = db.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false) && program.PlanProgramId == pcpobj.PlanProgramId).Select(program => program.PlanCampaignId).Single();
                                    Common.ChangeCampaignStatus(PlanCampaignId);
                                    //// End - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425

                                    scope.Complete();
                                    string strMessag = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);   // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    return Json(new { IsDuplicate = false, redirect = Url.Action("LoadSetup", new { id = form.PlanTacticId }), Msg = strMessag, planTacticId = pcpobj.PlanTacticId, planCampaignId = cid, planProgramId = pid });
                                }
                            }
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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { });
        }

        ///    <summary>
        /// Added By: Pratik Chauhan.
        /// Action to Create Tactic.
        /// </summary>
        /// <param name="id">Plan Program Id</param>
        /// <returns>Returns Partial View Of Tactic.</returns>
        public PartialViewResult CreateTactic(int id = 0)
        {
            //// Get those Tactic types whose ModelId exist in Plan table and IsDeployedToModel = true.
            var tactics = from _tacType in db.TacticTypes
                          join plan in db.Plans on _tacType.ModelId equals plan.ModelId
                          where plan.PlanId == Sessions.PlanId && (_tacType.IsDeleted == null || _tacType.IsDeleted == false) && _tacType.IsDeployedToModel == true //// Modified by Sohel Pathan on 17/07/2014 for PL ticket #594
                          orderby _tacType.Title , new AlphaNumericComparer()
                          select _tacType;
            foreach (var item in tactics)
            {
                item.Title = HttpUtility.HtmlDecode(item.Title);
            }

            ViewBag.Tactics = tactics;
            ViewBag.IsCreated = true;

            var objPlan = db.Plans.FirstOrDefault(varP => varP.PlanId == Sessions.PlanId);

            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);
            Plan_Campaign_Program pcpt = db.Plan_Campaign_Program.Where(pcpobj => pcpobj.PlanProgramId.Equals(id)).FirstOrDefault();
            if (pcpt == null)
            {
                return null;
            }

            //// Get PlanId by PlanCampaignId.
            int PlanId = (from pc in db.Plan_Campaign
                          where pc.PlanCampaignId ==
                              (from pcp in db.Plan_Campaign_Program where pcp.PlanProgramId == id select pcp.PlanCampaignId).FirstOrDefault()
                          select pc.PlanId).FirstOrDefault();

            #region "Set Inspect_Popup_Plan_Campaign_Program_TacticModel to pass into Partialview"
            Inspect_Popup_Plan_Campaign_Program_TacticModel pcptm = new Inspect_Popup_Plan_Campaign_Program_TacticModel();
            pcptm.PlanProgramId = id;
            pcptm.IsDeployedToIntegration = false;
            pcptm.StageId = 0;
            pcptm.StageTitle = "Stage";
            ViewBag.IsOwner = true;
            pcptm.ProgramTitle = HttpUtility.HtmlDecode(pcpt.Title);
            pcptm.CampaignTitle = HttpUtility.HtmlDecode(pcpt.Plan_Campaign.Title);
            ViewBag.RedirectType = false;
            pcptm.StartDate = GetCurrentDateBasedOnPlan();
            pcptm.EndDate = GetCurrentDateBasedOnPlan(true);
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;
            pcptm.TacticCost = 0;
            pcptm.AllocatedBy = objPlan.AllocatedBy;

            User userName = new User();
            try
            {
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                ViewBag.IsServiceUnavailable = false;

                userName = objBDSUserRepository.GetTeamMemberDetails(Sessions.User.UserId, Sessions.ApplicationId);
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
                    ViewBag.IsServiceUnavailable = true;
                }
            }

            pcptm.Owner = (userName.FirstName + " " + userName.LastName).ToString();
            #endregion

            return PartialView("SetupEditAdd", pcptm);
        }

        /// <summary>
        /// Calculate MQL Conerstion Rate based on Session Plan Id.
        /// Added by Bhavesh Dobariya.
        /// Modified By: Maninder Singh Wadhva to address TFS Bug#280 :Error Message Showing when editing a tactic - Preventing MQLs from updating
        /// Modified By: Maninder Singh Wadhva 1-March-2014 to address TFS Bug#322 : Changes made to INQ, MQL and Projected Revenue Calculation.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="projectedStageValue"></param>
        /// <param name="RedirectType"></param>
        /// <param name="isTacticTypeChange"></param>
        /// <returns>JsonResult MQl Rate.</returns>
        public JsonResult CalculateMQL(Inspect_Popup_Plan_Campaign_Program_TacticModel form, double projectedStageValue, bool RedirectType, bool isTacticTypeChange)
        {
            DateTime StartDate = new DateTime();
            string stageMQL = Enums.Stage.MQL.ToString();
            int tacticStageLevel = 0;
            int levelMQL = db.Stages.Single(stage => stage.ClientId.Equals(Sessions.User.ClientId) && stage.Code.Equals(stageMQL)).Level.Value;
            if (form.PlanTacticId != 0)
            {
                if (isTacticTypeChange)
                {
                    tacticStageLevel = Convert.ToInt32(db.TacticTypes.FirstOrDefault(t => t.TacticTypeId == form.TacticTypeId).Stage.Level);
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
                    StartDate = db.Plan_Campaign_Program_Tactic.Where(t => t.PlanTacticId == form.PlanTacticId).Select(t => t.StartDate).FirstOrDefault();
                }

                int modelId = db.Plans.Where(p => p.PlanId == Sessions.PlanId).Select(p => p.ModelId).FirstOrDefault();
                /// Added by Dharmraj on 4-Sep-2014
                /// #760 Advanced budgeting – show correct revenue in Tactic fly out
                Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
                objTactic.StartDate = StartDate;
                objTactic.EndDate = form.EndDate;
                objTactic.StageId = form.StageId;
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
            else
            {
                tacticStageLevel = Convert.ToInt32(db.TacticTypes.FirstOrDefault(_tacType => _tacType.TacticTypeId == form.TacticTypeId).Stage.Level);

                /// Added by Dharmraj on 4-Sep-2014
                /// #760 Advanced budgeting – show correct revenue in Tactic fly out
                int modelId = db.Plans.Where(plan => plan.PlanId == Sessions.PlanId).Select(plan => plan.ModelId).FirstOrDefault();

                Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();

                //// Set Tactic Start date.
                if (tacticStageLevel < levelMQL)
                    objTactic.StartDate = DateTime.Now;
                else
                    objTactic.StartDate = StartDate;

                objTactic.EndDate = form.EndDate;
                objTactic.StageId = form.StageId;
                objTactic.Plan_Campaign_Program = new Plan_Campaign_Program() { Plan_Campaign = new Plan_Campaign() { PlanId = Sessions.PlanId, Plan = new Plan() { } } };
                objTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId = modelId;
                objTactic.ProjectedStageValue = projectedStageValue;
                var lstTacticStageRelation = Common.GetTacticStageRelationForSingleTactic(objTactic, false);

                #region "Set MQL & Revenue value"

                double calculatedMQL = 0;
                double CalculatedRevenue = 0;
                calculatedMQL = lstTacticStageRelation.MQLValue;
                CalculatedRevenue = lstTacticStageRelation.RevenueValue;
                CalculatedRevenue = Math.Round(CalculatedRevenue, 2); // Modified by Sohel Pathan on 16/09/2014 for PL ticket #760

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

                #endregion
            }
        }

        #region Budget Tactic for Campaign Tab
        /// <summary>
        /// Fetch the Tactic Budget Allocation 
        /// </summary>
        /// <param name="id">Campaign Id</param>
        /// <returns></returns>
        public PartialViewResult LoadTacticBudgetAllocation(int id = 0)
        {
            Plan_Campaign_Program_Tactic pcp = db.Plan_Campaign_Program_Tactic.Where(pcpobj => pcpobj.PlanTacticId.Equals(id) && pcpobj.IsDeleted.Equals(false)).FirstOrDefault();
            if (pcp == null)
            {
                return null;
            }
            Plan_Campaign_Program_TacticModel pcpm = new Plan_Campaign_Program_TacticModel();
            pcpm.PlanProgramId = pcp.PlanProgramId;
            pcpm.PlanTacticId = pcp.PlanTacticId;

            string statusAllocatedByNone = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower();
            string statusAllocatedByDefault = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();
            double budgetAllocation = db.Plan_Campaign_Program_Tactic_Cost.Where(s => s.PlanTacticId == id).ToList().Sum(s => s.Value);

            pcpm.TacticCost = (pcp.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == pcp.PlanTacticId && s.IsDeleted == false)).Count() > 0 &&
                pcp.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy != statusAllocatedByNone && pcp.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy != statusAllocatedByDefault
                ?
                (budgetAllocation > 0 ? budgetAllocation : (pcp.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == pcp.PlanTacticId && s.IsDeleted == false)).Sum(a => a.Cost))
                : pcp.Cost;

            pcpm.AllocatedBy = pcp.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy;

            #region "Calculate Plan remaining budget"
            //Added By : Kalpesh Sharma Functioan and code review #693
            var CostTacticsBudget = db.Plan_Campaign_Program_Tactic.Where(c => c.PlanProgramId == pcpm.PlanProgramId).ToList().Sum(c => c.Cost);
            var objPlanCampaignProgram = db.Plan_Campaign_Program.FirstOrDefault(p => p.PlanProgramId == pcpm.PlanProgramId);
            ViewBag.planRemainingBudget = (objPlanCampaignProgram.ProgramBudget - (!string.IsNullOrEmpty(Convert.ToString(CostTacticsBudget)) ? CostTacticsBudget : 0));
            #endregion

            return PartialView("_SetupTacticBudgetAllocation", pcpm);
        }

        /// <summary>
        /// Action to Save Campaign Allocation.
        /// </summary>
        /// <param name="form">Form object of Plan_Campaign_ProgramModel.</param>
        /// <param name="BudgetInputValues">Budget Input Values.</param>
        /// <param name="UserId">User Id.</param>
        /// <param name="title"></param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveTacticBudgetAllocation(Plan_Campaign_Program_TacticModel form, string BudgetInputValues, string UserId = "", string title = "")
        {
            //// check whether UserId is loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    return Json(new { IsSaved = false, msg = Common.objCached.LoginWithSameSession, JsonRequestBehavior.AllowGet });
                }
            }

            try
            {
                string[] arrBudgetInputValues = BudgetInputValues.Split(',');
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        //// Get duplicate record to check duplication.
                        var pcpvar = (from pcpt in db.Plan_Campaign_Program_Tactic
                                      join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                      join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                      where pcpt.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcpt.PlanTacticId.Equals(form.PlanTacticId) && pcpt.IsDeleted.Equals(false)
                                      && pcp.PlanProgramId == form.PlanProgramId
                                      select pcp).FirstOrDefault();
                        //// if duplicate record exist then return duplication message.
                        if (pcpvar != null)
                        {
                            string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                            return Json(new { IsSaved = false, msg = strDuplicateMessage, JsonRequestBehavior.AllowGet });
                        }
                        else
                        {
                            string status = string.Empty;

                            Plan_Campaign_Program_Tactic pcpobj = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(form.PlanTacticId)).FirstOrDefault();
                            pcpobj.Title = title;
                            pcpobj.Cost = UpdateBugdetAllocationCost(arrBudgetInputValues, form.TacticCost);
                            pcpobj.ModifiedBy = Sessions.User.UserId;
                            pcpobj.ModifiedDate = DateTime.Now;

                            //Start by Kalpesh Sharma #605: Cost allocation for Tactic
                            var PrevAllocationList = db.Plan_Campaign_Program_Tactic_Cost.Where(_tacCost => _tacCost.PlanTacticId == form.PlanTacticId).Select(_tacCost => _tacCost).ToList();  // Modified by Sohel Pathan on 04/09/2014 for PL ticket #759

                            //// Process for Monthly budget values.
                            if (arrBudgetInputValues.Length == 12)
                            {
                                for (int i = 0; i < arrBudgetInputValues.Length; i++)
                                {
                                    // Start - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
                                    bool isExists = false;
                                    if (PrevAllocationList != null && PrevAllocationList.Count > 0)
                                    {
                                        //// Get previous campaign budget values by Period.
                                        var updatePlanTacticBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (i + 1))).FirstOrDefault();
                                        if (updatePlanTacticBudget != null)
                                        {
                                            if (arrBudgetInputValues[i] != "")
                                            {
                                                var newValue = Convert.ToDouble(arrBudgetInputValues[i]);
                                                if (updatePlanTacticBudget.Value != newValue)
                                                {
                                                    //// Update Tactic budget value with newValue.
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

                                    //// if Old budget value does not exist then insert new value to table.
                                    if (!isExists && arrBudgetInputValues[i] != "")
                                    {
                                        // End - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
                                        Plan_Campaign_Program_Tactic_Cost obPlanCampaignProgramTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                        obPlanCampaignProgramTacticCost.PlanTacticId = form.PlanTacticId;
                                        obPlanCampaignProgramTacticCost.Period = PeriodChar + (i + 1);
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
                                    if (PrevAllocationList != null && PrevAllocationList.Count > 0)
                                    {
                                        //// Get Quarter budget list.
                                        var thisQuartersMonthList = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (BudgetInputValuesCounter)) || pb.Period == (PeriodChar + (BudgetInputValuesCounter + 1)) || pb.Period == (PeriodChar + (BudgetInputValuesCounter + 2))).ToList().OrderBy(a => a.Period).ToList();

                                        //// Get First month values from Quarterly budget list.
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
                                                                thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (BudgetInputValuesCounter + j))).FirstOrDefault();
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

                                    //// if Old budget value does not exist then insert new value to table.
                                    if (!isExists && arrBudgetInputValues[i] != "")
                                    {
                                        // End - Added by Sohel Pathan on 04/09/2014 for PL ticket #759
                                        Plan_Campaign_Program_Tactic_Cost obPlanCampaignProgramTacticCost = new Plan_Campaign_Program_Tactic_Cost();
                                        obPlanCampaignProgramTacticCost.PlanTacticId = form.PlanTacticId;
                                        obPlanCampaignProgramTacticCost.Period = PeriodChar + BudgetInputValuesCounter;
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
                            result = db.SaveChanges();

                            // Start Added by dharmraj for ticket #644
                            //// Calculate Total LineItem Cost.
                            var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(lineItem => lineItem.PlanTacticId == pcpobj.PlanTacticId && lineItem.Title == Common.DefaultLineItemTitle && lineItem.LineItemTypeId == null);
                            var objtotalLoneitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanTacticId == pcpobj.PlanTacticId && lineItem.LineItemTypeId != null && lineItem.IsDeleted == false);
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

                            db.SaveChanges();
                            scope.Complete();
                            string strMessage = Common.objCached.PlanEntityAllocationUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                            return Json(new { IsSaved = true, msg = strMessage, planTacticId = form.PlanTacticId, JsonRequestBehavior.AllowGet });
                        }
                    }
                }

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { IsSaved = false, msg = Common.objCached.ErrorOccured, JsonRequestBehavior.AllowGet });
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

        #endregion

        #region fill Owner list
        /// <summary>
        /// fill Owner list based on custom fields with ViewEdit rights
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>18/11/2014</CreatedDate>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public JsonResult fillOwner(string UserId = "")
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
                List<Guid> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(Sessions.User.ClientId);
                if (lstClientUsers.Count() > 0)
                {
                    string strUserList = string.Join(",", lstClientUsers);
                    BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                    List<User> lstUserDetails = objBDSServiceClient.GetMultipleTeamMemberName(strUserList);
                    if (lstUserDetails.Count > 0)
                    {
                        lstUserDetails = lstUserDetails.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                        var lstPreparedOwners = lstUserDetails.Select(user => new { UserId = user.UserId, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();
                        return Json(new { isSuccess = true, lstOwner = lstPreparedOwners }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { isSuccess = true, lstOwner = new List<User>() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        /// <summary>
        /// Added By: Maninder Singh.
        /// Modified By Maninder Singh Wadhva PL Ticket#47
        /// Action to Save Comment & Update Status as of selected tactic.
        /// </summary>
        /// <param name="planTacticId">Plan Tactic Id.</param>
        /// <param name="isApprove"></param>
        /// <param name="isImprovement"></param>
        /// <returns>Returns flag to indicate whether operation was successfull or not.</returns>
        [HttpPost]
        public JsonResult ApproveDeclineTactic(int planTacticId, bool isApprove, bool isImprovement)
        {
            int result = 0;
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        if (ModelState.IsValid)
                        {
                            string status = isApprove ? Enums.TacticStatusValues.Single(t => t.Key.Equals(Enums.TacticStatus.Approved.ToString())).Value : Enums.TacticStatusValues.Single(t => t.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;

                            /// Modified By Maninder Singh Wadhva PL Ticket#47
                            /// Check whether Tactic is Improvement Tactic or not.
                            if (isImprovement)
                            {
                                Plan_Improvement_Campaign_Program_Tactic improvementPlanTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => improvementTactic.ImprovementPlanTacticId.Equals(planTacticId)).FirstOrDefault();
                                improvementPlanTactic.Status = status;
                                improvementPlanTactic.ModifiedBy = Sessions.User.UserId;
                                improvementPlanTactic.ModifiedDate = DateTime.Now;

                                #region "Insert ImprovementTactic Comment to Plan_Improvement_Campaign_Program_Tactic_Comment table"
                                Plan_Improvement_Campaign_Program_Tactic_Comment improvementPlanTacticComment = new Plan_Improvement_Campaign_Program_Tactic_Comment()
                                {
                                    ImprovementPlanTacticId = planTacticId,
                                    //changes done from displayname to FName and Lname by uday for internal issue on 27-6-2014
                                    Comment = string.Format("Improvement Tactic {0} by {1}", status, Sessions.User.FirstName + " " + Sessions.User.LastName),
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = Sessions.User.UserId
                                };

                                db.Entry(improvementPlanTacticComment).State = EntityState.Added;
                                #endregion

                                db.Entry(improvementPlanTactic).State = EntityState.Modified;
                                result = db.SaveChanges();
                                if (result == 2)
                                {
                                    if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                                    {
                                        result = Common.InsertChangeLog(improvementPlanTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, 0, planTacticId, improvementPlanTactic.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved);
                                        //added by uday for #532
                                        if (improvementPlanTactic.IsDeployedToIntegration == true)
                                        {
                                            ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, new Guid(), EntityType.ImprovementTactic);
                                            externalIntegration.Sync();
                                        }
                                        //added by uday for #532
                                    }
                                    else if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                    {
                                        result = Common.InsertChangeLog(improvementPlanTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, 0, planTacticId, improvementPlanTactic.Title, Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined);
                                    }
                                    //// Get URL for Tactic to send to Email.
                                    string strUrl = GetNotificationURLbyStatus(improvementPlanTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, planTacticId, Convert.ToString(Enums.Section.ImprovementTactic).ToLower()); // Added by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                    Common.mailSendForTactic(planTacticId, status, improvementPlanTactic.Title, section: Convert.ToString(Enums.Section.ImprovementTactic).ToLower(), URL: strUrl); // Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                    if (result >= 1)
                                    {
                                        scope.Complete();
                                        return Json(new { result = true }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            else
                            {
                                Plan_Campaign_Program_Tactic tactic = db.Plan_Campaign_Program_Tactic.Where(pt => pt.PlanTacticId == planTacticId).FirstOrDefault();
                                bool isApproved = false;
                                DateTime todaydate = DateTime.Now;
                                if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                                {
                                    isApproved = true;
                                    if (todaydate > tactic.StartDate && todaydate < tactic.EndDate)
                                    {
                                        tactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                                    }
                                    else if (todaydate > tactic.EndDate)
                                    {
                                        tactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                                    }
                                }
                                else
                                {
                                    tactic.Status = status;
                                }
                                tactic.ModifiedBy = Sessions.User.UserId;
                                tactic.ModifiedDate = DateTime.Now;

                                Plan_Campaign_Program_Tactic_Comment pcptc = new Plan_Campaign_Program_Tactic_Comment()
                                {
                                    PlanTacticId = planTacticId,
                                    //changes done from displayname to FName and Lname by uday for internal issue on 27-6-2014
                                    Comment = string.Format("Tactic {0} by {1}", status, Sessions.User.FirstName + " " + Sessions.User.LastName),
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = Sessions.User.UserId
                                };

                                db.Entry(pcptc).State = EntityState.Added;
                                db.Entry(tactic).State = EntityState.Modified;
                                result = db.SaveChanges();
                                if (result == 2)
                                {
                                    if (isApproved)
                                    {
                                        result = Common.InsertChangeLog(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, 0, planTacticId, tactic.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved);
                                        //// added by uday for #532
                                        if (tactic.IsDeployedToIntegration == true)
                                        {
                                            ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, new Guid(), EntityType.Tactic);
                                            externalIntegration.Sync();
                                        }
                                        //// End by uday for #532
                                    }
                                    else if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                    {
                                        result = Common.InsertChangeLog(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, 0, planTacticId, tactic.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined);
                                    }
                                    //// Get URL for Tactic to send to Email.
                                    string strUrl = GetNotificationURLbyStatus(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId, planTacticId, Convert.ToString(Enums.Section.Tactic).ToLower());
                                    Common.mailSendForTactic(planTacticId, status, tactic.Title, section: Convert.ToString(Enums.Section.Tactic).ToLower(), URL: strUrl); // Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.

                                    ////Start Added by Mitesh Vaishnav for PL ticket #766 Different Behavior for Approve Tactics via Request tab
                                    ////Update Program status according to the tactic status
                                    Common.ChangeProgramStatus(tactic.PlanProgramId);

                                    ////Update Campaign status according to the tactic and program status
                                    var PlanCampaignId = tactic.Plan_Campaign_Program.PlanCampaignId;
                                    Common.ChangeCampaignStatus(PlanCampaignId);
                                    ////End Added by Mitesh Vaishnav for PL ticket #766 Different Behavior for Approve Tactics via Request tab

                                    if (result >= 1)
                                    {
                                        scope.Complete();
                                        return Json(new { result = true }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                            /// Modified By Maninder Singh Wadhva PL Ticket#47
                            return Json(new { result = false }, JsonRequestBehavior.AllowGet);
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

            return null;
        }
        #endregion

        #region "Improvement Tactic related Functions"

        /// <summary>
        /// Added By: Pratik Chauhan.
        /// Action to Save Improvement Tactic.
        /// </summary>
        /// <param name="form">Form object of PlanImprovementTactic.</param>
        /// <param name="RedirectType">Redirect Type.</param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveImprovementTactic(InspectModel form, bool RedirectType)
        {
            try
            {
                //// If ImprovementPlanTacticId is null then insert new record to Table.
                if (form.ImprovementPlanTacticId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// Check for duplicate exist or not.
                            var pcpvar = (from pcpt in db.Plan_Improvement_Campaign_Program_Tactic
                                          where pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && pcpt.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcpt.IsDeleted.Equals(false)
                                          select pcpt).FirstOrDefault();

                            //// if duplicate record exist then return duplication message.
                            if (pcpvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { isSaved = false, redirect = Url.Action("Assortment"), errormsg = strDuplicateMessage });  // Modified by Viral Kadiya on 11/18/2014 to resolve Internal Review Points.
                            }
                            else
                            {
                                Plan_Improvement_Campaign_Program_Tactic picpt = new Plan_Improvement_Campaign_Program_Tactic();
                                picpt.ImprovementPlanProgramId = form.ImprovementPlanProgramId;
                                picpt.Title = form.Title;
                                picpt.ImprovementTacticTypeId = form.ImprovementTacticTypeId;
                                picpt.Description = form.Description;
                                picpt.Cost = form.Cost ?? 0;
                                picpt.EffectiveDate = form.EffectiveDate;
                                picpt.Status = Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString();
                                picpt.CreatedBy = Sessions.User.UserId;
                                picpt.CreatedDate = DateTime.Now;
                                picpt.IsDeployedToIntegration = form.IsDeployedToIntegration;

                                db.Entry(picpt).State = EntityState.Added;
                                int result = db.SaveChanges();

                                // Set isDeployedToIntegration in improvement program and improvement campaign
                                var objIProgram = db.Plan_Improvement_Campaign_Program.FirstOrDefault(_program => _program.ImprovementPlanProgramId == picpt.ImprovementPlanProgramId);
                                var objICampaign = db.Plan_Improvement_Campaign.FirstOrDefault(_campaign => _campaign.ImprovementPlanCampaignId == objIProgram.ImprovementPlanCampaignId);
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
                                    flag = objIProgram.Plan_Improvement_Campaign_Program_Tactic.Any(_tactic => _tactic.IsDeployedToIntegration == true && _tactic.IsDeleted == false);
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
                                    string strMessage = Common.objCached.PlanEntityCreated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    return Json(new { isSaved = true, redirect = Url.Action("Assortment"), msg = strMessage, id = picpt.ImprovementPlanTacticId });
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
                                bool isManagerLevelUser = false;
                                string status = string.Empty;

                                Plan_Improvement_Campaign_Program_Tactic pcpobj = db.Plan_Improvement_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.ImprovementPlanTacticId.Equals(form.ImprovementPlanTacticId)).FirstOrDefault();

                                //If improvement tacitc modified by immediate manager then no resubmission will take place, By dharmraj, Ticket #537
                                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
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
                                    pcpobj.Cost = form.Cost ?? 0;
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
                                var objIProgram = db.Plan_Improvement_Campaign_Program.FirstOrDefault(_program => _program.ImprovementPlanProgramId == pcpobj.ImprovementPlanProgramId);
                                var objICampaign = db.Plan_Improvement_Campaign.FirstOrDefault(_campaign => _campaign.ImprovementPlanCampaignId == objIProgram.ImprovementPlanCampaignId);
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
                                    flag = objIProgram.Plan_Improvement_Campaign_Program_Tactic.Any(_tactic => _tactic.IsDeployedToIntegration == true && _tactic.IsDeleted == false);
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
                                        string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                        return Json(new { isSaved = true, redirect = Url.Action("Assortment"), msg = strMessage });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);

                //To handle unavailability of BDSService
                if (ex is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Setup Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="InspectPopupMode"></param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadImprovementSetup(int id, string InspectPopupMode = "")
        {
            ViewBag.InspectMode = InspectPopupMode;

            if (InspectPopupMode == Enums.InspectPopupMode.Add.ToString())
            {
                var planId = (from pc in db.Plan_Improvement_Campaign where pc.ImprovementPlanCampaignId == ((from pcp in db.Plan_Improvement_Campaign_Program where pcp.ImprovementPlanProgramId == id select pcp.ImprovementPlanCampaignId).FirstOrDefault()) select pc.ImprovePlanId).FirstOrDefault();
                //// Get Improvement Tactic list for Current Plan.
                List<int> impTacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(_tactic => _tactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && _tactic.IsDeleted == false).Select(_tactic => _tactic.ImprovementTacticTypeId).ToList();

                //// load Improvement tactic list to ViewBag excluding current plan Improvement Tactics.
                ViewBag.Tactics = from _imprvTactic in db.ImprovementTacticTypes
                                  where _imprvTactic.ClientId == Sessions.User.ClientId && _imprvTactic.IsDeployed == true && !impTacticList.Contains(_imprvTactic.ImprovementTacticTypeId)
                                  && _imprvTactic.IsDeleted == false
                                  orderby _imprvTactic.Title,new AlphaNumericComparer()
                                  select _imprvTactic;
                ViewBag.IsCreated = true;

                //// Get Plan by PlanId.
                var objPlan = db.Plans.FirstOrDefault(varP => varP.PlanId == Sessions.PlanId);
                ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);

                InspectModel pitm = new InspectModel();
                pitm.ImprovementPlanProgramId = id;
                pitm.CampaignTitle = (from pc in db.Plan_Improvement_Campaign where pc.ImprovePlanId == planId select pc.Title).FirstOrDefault().ToString();

                // Set today date as default for effective date.
                pitm.EffectiveDate = DateTime.Now;
                pitm.IsDeployedToIntegration = false;

                ViewBag.IsOwner = true;
                ViewBag.RedirectType = false;
                ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;


                User userName = new User();
                try
                {
                    userName = objBDSUserRepository.GetTeamMemberDetails(Sessions.User.UserId, Sessions.ApplicationId);
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
                        return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                    }
                }

                pitm.Owner = userName.FirstName + " " + userName.LastName;
                ViewBag.TacticDetail = pitm;
                return PartialView("_SetupImprovementTactic", pitm);
            }
            else
            {
                InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.ImprovementTactic).ToLower());
                List<Guid> userListId = new List<Guid>();
                userListId.Add(im.OwnerId);
                User userName = new User();
                try
                {
                    userName = objBDSUserRepository.GetTeamMemberDetails(im.OwnerId, Sessions.ApplicationId);
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
                        return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                    }
                }
                im.Owner = (userName.FirstName + " " + userName.LastName).ToString();
                im.EffectiveDate = im.EffectiveDate;
                ViewBag.TacticDetail = im;

                var objPlan = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTactic => _imprvTactic.ImprovementPlanTacticId.Equals(id) && _imprvTactic.IsDeleted.Equals(false)).Select(_imprvTactic => _imprvTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan).FirstOrDefault();
                int planId = objPlan.PlanId;

                ViewBag.ApprovedStatus = true;
                ViewBag.NoOfTacticBoosts = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.IsDeleted == false && _tactic.StartDate >= im.StartDate && _tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).ToList().Count();


                ViewBag.IsModelDeploy = im.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690
                if (im.LastSyncDate != null)
                {
                    TimeZone localZone = TimeZone.CurrentTimeZone;

                    ViewBag.LastSync = "Last synced with integration " + Common.GetFormatedDate(im.LastSyncDate) + ".";////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
                }
                else
                {
                    ViewBag.LastSync = string.Empty;
                }
                if (InspectPopupMode == Enums.InspectPopupMode.Edit.ToString())
                {
                    ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);

                    List<int> impTacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(_imprvTactic => _imprvTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == planId && _imprvTactic.IsDeleted == false && _imprvTactic.ImprovementPlanTacticId != id).Select(_imprvTactic => _imprvTactic.ImprovementTacticTypeId).ToList();

                    //// Get Improvement Tactic Type list.
                    var tactics = from _imprvTacticType in db.ImprovementTacticTypes
                                  where _imprvTacticType.ClientId == Sessions.User.ClientId && _imprvTacticType.IsDeployed == true && !impTacticList.Contains(_imprvTacticType.ImprovementTacticTypeId)
                                  && _imprvTacticType.IsDeleted == false
                                  select _imprvTacticType;

                    foreach (var item in tactics)
                    {
                        item.Title = HttpUtility.HtmlDecode(item.Title);
                    }

                    //// Add other Improvement Tactic Type to tactics list.
                    if (!tactics.Any(a => a.ImprovementTacticTypeId == im.ImprovementTacticTypeId))
                    {
                        var tacticTypeSpecial = from t in db.ImprovementTacticTypes
                                                where t.ClientId == Sessions.User.ClientId && t.ImprovementTacticTypeId == im.ImprovementTacticTypeId
                                                orderby t.Title
                                                select t;

                        tactics = tactics.Concat<ImprovementTacticType>(tacticTypeSpecial);
                        tactics.OrderBy(a => a.Title);
                    }

                    ViewBag.Tactics = tactics;
                    ViewBag.InspectMode = InspectPopupMode;

                    if (Sessions.User.UserId == im.OwnerId)
                    {
                        ViewBag.IsOwner = true;
                    }
                    else
                    {
                        ViewBag.IsOwner = false;
                    }
                    ViewBag.Year = objPlan.Year;
                }

                if (ViewBag.Year == null)
                {
                    ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(planId) && p.IsDeleted == false).Year;
                }

                return PartialView("_SetupImprovementTactic", im);
            }
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Review Tab.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="InspectPopupMode"></param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadImprovementReview(int id, string InspectPopupMode = "")
        {
            InspectModel im = GetInspectModel(id, Convert.ToString(Enums.Section.ImprovementTactic).ToLower());

            //// Get Userlist using Improvement Tactic Comment list.
            var tacticComment = (from _imprvTacCmnt in db.Plan_Improvement_Campaign_Program_Tactic_Comment
                                 where _imprvTacCmnt.ImprovementPlanTacticId == id
                                 select _imprvTacCmnt).ToArray();
            List<Guid> userListId = new List<Guid>();
            userListId = (from ta in tacticComment select ta.CreatedBy).ToList<Guid>();
            userListId.Add(im.OwnerId);
            string userList = string.Join(",", userListId.Select(s => s.ToString()).ToArray());
            List<User> userName = new List<User>();

            try
            {
                userName = objBDSUserRepository.GetMultipleTeamMemberDetails(userList, Sessions.ApplicationId);
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
            ViewBag.ReviewModel = (from tc in tacticComment
                                   select new InspectReviewModel
                                   {
                                       PlanTacticId = Convert.ToInt32(tc.ImprovementPlanTacticId),
                                       Comment = tc.Comment,
                                       CommentDate = tc.CreatedDate,
                                       CommentedBy = userName.Where(u => u.UserId == tc.CreatedBy).Any() ? userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.FirstName).FirstOrDefault() + " " + userName.Where(u => u.UserId == tc.CreatedBy).Select(u => u.LastName).FirstOrDefault() : GameplanIntegrationService,
                                       CreatedBy = tc.CreatedBy
                                   }).ToList();

            var ownername = (from u in userName
                             where u.UserId == im.OwnerId
                             select u.FirstName + " " + u.LastName).FirstOrDefault();
            if (ownername != null)
            {
                im.Owner = ownername.ToString();
            }
            ViewBag.TacticDetail = im;

            ViewBag.IsModelDeploy = im.IsIntegrationInstanceExist == "N/A" ? false : true;////Modified by Mitesh vaishnav on 20/08/2014 for PL ticket #690

            bool isValidOwner = false;
            if (im.OwnerId == Sessions.User.UserId)
            {
                isValidOwner = true;
            }
            ViewBag.IsValidOwner = isValidOwner;

            var lstSubOrdinatesPeers = new List<Guid>();
            //To get permission status for Approve campaign , By dharmraj PL #538
            try
            {
                lstSubOrdinatesPeers = Common.GetSubOrdinatesWithPeersNLevel();
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool isValidManagerUser = false;
            if (lstSubOrdinatesPeers.Contains(im.OwnerId))
            {
                isValidManagerUser = true;
            }
            ViewBag.IsValidManagerUser = isValidManagerUser;

            // Start - Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic
            ViewBag.IsCommentsViewEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.CommentsViewEdit);
            if ((bool)ViewBag.IsCommentsViewEditAuthorized == false)
                ViewBag.UnauthorizedCommentSection = Common.objCached.UnauthorizedCommentSection;
            // End - Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic

            // Added by Dharmraj Mangukiya for Deploy to integration button restrictions PL ticket #537
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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            bool IsDeployToIntegrationVisible = false;
            if (im.OwnerId.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
            {
                IsDeployToIntegrationVisible = true;
            }
            else if (IsPlanEditAllAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            {
                IsDeployToIntegrationVisible = true;
            }
            else if (IsPlanEditSubordinatesAuthorized)
            {
                if (lstSubOrdinates.Contains(im.OwnerId)) // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsDeployToIntegrationVisible = true;
                }
            }
            ViewBag.IsDeployToIntegrationVisible = IsDeployToIntegrationVisible;

            return PartialView("_ReviewImprovementTactic");
        }

        /// <summary>
        /// Action to Load Improvement Impact View.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="InspectPopupMode"></param>
        /// <returns>Returns Partial View Of Review Tab.</returns>
        public ActionResult LoadImprovementImpact(int id, string InspectPopupMode = "")
        {
            return PartialView("_ImpactImprovementTactic");
        }

        /// <summary>
        /// Calculate Improvenet For Tactic Type & Date.
        /// Added by Bhavesh Dobariya.
        /// </summary>
        /// <param name="ImprovementPlanTacticId"></param>
        /// <returns>JsonResult.</returns>
        public JsonResult LoadImpactImprovementStages(int ImprovementPlanTacticId)
        {
            int ImprovementTacticTypeId = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.ImprovementPlanTacticId == ImprovementPlanTacticId).Select(t => t.ImprovementTacticTypeId).FirstOrDefault();
            DateTime EffectiveDate = db.Plan_Improvement_Campaign_Program_Tactic.Where(t => t.ImprovementPlanTacticId == ImprovementPlanTacticId).Select(t => t.EffectiveDate).FirstOrDefault();
            PlanController pc = new PlanController();
            List<ImprovementStage> ImprovementMetric = pc.GetImprovementStages(ImprovementPlanTacticId, ImprovementTacticTypeId, EffectiveDate);

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

            return Json(new { data = tacticobj }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Improvement Tactic Type related value using Improvement Tactic Type id.
        /// Added By : Sohel Pathan
        /// Added Date : 06/11/2014
        /// </summary>
        /// <param name="ImprovementTacticTypeId">ImprovementTacticTypeId</param>
        /// <returns>JsonResult.</returns>
        public JsonResult LoadImprovementTacticTypeData(int ImprovementTacticTypeId)
        {
            try
            {
                var objImprovementTacticType = db.ImprovementTacticTypes.Where(itt => itt.ImprovementTacticTypeId == ImprovementTacticTypeId && itt.IsDeleted.Equals(false)).FirstOrDefault();
                double Cost = objImprovementTacticType == null ? 0 : objImprovementTacticType.Cost;
                bool isDeployedToIntegration = objImprovementTacticType == null ? false : objImprovementTacticType.IsDeployedToIntegration;

                return Json(new { isSuccess = true, cost = Cost, isDeployedToIntegration = isDeployedToIntegration }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region "LineItem related Functions"

        /// <summary>
        /// Save Line item Actual 
        /// Added By : Kalpesh Sharma #735 Actual cost - Changes to add actuals screen 
        /// Save the data into the Plan_Campaign_Program_Tactic_LineItem_Actual
        /// </summary>
        /// <param name="objInspectActual"></param>
        public void SaveActualLineItem(InspectActual objInspectActual)
        {
            Plan_Campaign_Program_Tactic_LineItem_Actual objPlan_LineItem_Actual = new Plan_Campaign_Program_Tactic_LineItem_Actual();
            objPlan_LineItem_Actual.PlanLineItemId = Convert.ToInt32(objInspectActual.PlanLineItemId);
            objPlan_LineItem_Actual.Period = objInspectActual.Period;
            objPlan_LineItem_Actual.CreatedDate = DateTime.Now;
            objPlan_LineItem_Actual.CreatedBy = Sessions.User.UserId;
            objPlan_LineItem_Actual.Value = objInspectActual.ActualValue;
            db.Entry(objPlan_LineItem_Actual).State = EntityState.Added;
            db.Plan_Campaign_Program_Tactic_LineItem_Actual.Add(objPlan_LineItem_Actual);
        }

        /// <summary>
        /// Added By : Kalpesh Sharma : Functional Review Points #697
        /// This method is responsible to add the Line items in Tactic.
        /// </summary>
        /// <param name="form">Pass the Tactic form model for fetching Start date and end date values</param>
        /// <param name="lineitems">pass the lineitems</param>
        /// <param name="tacticId">Tactic Id that used to insert into Lineitem table </param>
        public int SaveLineItems(Inspect_Popup_Plan_Campaign_Program_TacticModel form, string lineitems, int tacticId)
        {
            int Result = 0;

            //Fetch the lineitems and store into array of string
            string[] lineitem = lineitems.Split(',');

            // Fetch the current plan object and based on it's ModelID we have fetch the lineitems.
            Plan objPlan = db.Plans.Where(p => p.PlanId == (db.Plan_Campaign.Where(pc => pc.PlanCampaignId == form.PlanCampaignId && pc.PlanCampaignId.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault())).FirstOrDefault();
            var lineItemType = db.LineItemTypes.Where(l => l.ModelId == objPlan.ModelId).ToList();

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
                Result = Common.InsertChangeLog(objPlan.PlanId, null, liid, pcptlobj.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
            }

            //retturn the reuslt status
            return Result;
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Load Lineitem Setup Tab.
        /// </summary>
        /// <param name="id">Plan Lineitem Id.</param>
        /// <returns>Returns Partial View Of Setup Tab.</returns>
        public ActionResult LoadSetupLineitem(int id)
        {

            ViewBag.IsCreated = false;
            Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobj => pcpobj.PlanLineItemId.Equals(id));
            if (pcptl == null)
            {
                return null;
            }

            var objPlan = db.Plans.FirstOrDefault(plan => plan.PlanId == pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId);

            //// Get list of LineItem Types based on ModelId for current PlanId.
            var lineItemTypes = from lit in db.LineItemTypes
                                where lit.ModelId == objPlan.ModelId && lit.IsDeleted == false
                                orderby lit.Title
                                select lit;

            string lineItemTypeName = "";

            foreach (var item in lineItemTypes)
            {
                if (pcptl.LineItemTypeId.ToString() == item.LineItemTypeId.ToString())
                {
                    lineItemTypeName = HttpUtility.HtmlDecode(item.Title);
                }
            }

            //// Set respected values to ViewBag.
            ViewBag.lineItemTypes = lineItemTypeName;
            ViewBag.TacticTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Title);
            ViewBag.ProgramTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Title);
            ViewBag.CampaignTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Title);
            ViewBag.PlanTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Title);


            //// Set Values to Plan_Campaign_Program_Tactic_LineItemModel to pass into PartialView.
            Plan_Campaign_Program_Tactic_LineItemModel pcptlm = new Plan_Campaign_Program_Tactic_LineItemModel();
            pcptlm.PlanLineItemId = pcptl.PlanLineItemId;
            pcptlm.PlanTacticId = pcptl.PlanTacticId;
            pcptlm.LineItemTypeId = pcptl.LineItemTypeId == null ? 0 : Convert.ToInt32(pcptl.LineItemTypeId);
            pcptlm.Title = HttpUtility.HtmlDecode(pcptl.Title);
            pcptlm.Description = HttpUtility.HtmlDecode(pcptl.Description);
            pcptlm.StartDate = Convert.ToDateTime(pcptl.StartDate);
            pcptlm.EndDate = Convert.ToDateTime(pcptl.EndDate);
            pcptlm.Cost = pcptl.Cost;
            pcptlm.TStartDate = pcptl.Plan_Campaign_Program_Tactic.StartDate;
            pcptlm.TEndDate = pcptl.Plan_Campaign_Program_Tactic.EndDate;
            pcptlm.PStartDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.StartDate;
            pcptlm.PEndDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.EndDate;
            pcptlm.CStartDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.StartDate;
            pcptlm.CEndDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.EndDate;
            pcptlm.IsOtherLineItem = pcptl.LineItemTypeId != null ? false : true;
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;


            User userName = new User();
            try
            {
                userName = objBDSUserRepository.GetTeamMemberDetails(pcptl.CreatedBy, Sessions.ApplicationId);
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            ViewBag.Owner = userName.FirstName + " " + userName.LastName;
            return PartialView("_SetupLineitem", pcptlm);
        }

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Load Lineitem Setup Tab in edit mode.
        /// </summary>
        /// <param name="id">Plan Lineitem Id.</param>
        /// <returns>Returns Partial View Of edit Setup Tab.</returns>
        public ActionResult LoadEditSetupLineitem(int id)
        {
            ViewBag.IsCreated = false;

            //// Get LineItem Data by ID.
            Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobj => pcpobj.PlanLineItemId.Equals(id));
            if (pcptl == null)
            {
                return null;
            }

            //// Get Plan data by PlanId.
            var objPlan = db.Plans.FirstOrDefault(plan => plan.PlanId == pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId);

            //// Get list of LineItem Types based on ModelId for current PlanId.
            var lineItemTypes = from lit in db.LineItemTypes
                                where lit.ModelId == objPlan.ModelId && lit.IsDeleted == false
                                orderby lit.Title
                                select lit;
            foreach (var item in lineItemTypes)
            {
                item.Title = HttpUtility.HtmlDecode(item.Title);
            }
            ViewBag.lineItemTypes = lineItemTypes;

            ViewBag.TacticTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Title);
            ViewBag.ProgramTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Title);
            ViewBag.CampaignTitle = HttpUtility.HtmlDecode(pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Title);
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;

            //// Set data to Plan_Campaign_Program_Tactic_LineItemModel to pass into PartialView.
            Plan_Campaign_Program_Tactic_LineItemModel pcptlm = new Plan_Campaign_Program_Tactic_LineItemModel();
            if (pcptl.LineItemTypeId == null)
            {
                pcptlm.IsOtherLineItem = true;
            }
            else
            {
                pcptlm.IsOtherLineItem = false;
            }
            pcptlm.PlanLineItemId = pcptl.PlanLineItemId;
            pcptlm.PlanTacticId = pcptl.PlanTacticId;
            pcptlm.LineItemTypeId = pcptl.LineItemTypeId == null ? 0 : Convert.ToInt32(pcptl.LineItemTypeId);
            pcptlm.Title = HttpUtility.HtmlDecode(pcptl.Title);
            pcptlm.Description = HttpUtility.HtmlDecode(pcptl.Description);
            pcptlm.StartDate = Convert.ToDateTime(pcptl.StartDate);
            pcptlm.EndDate = Convert.ToDateTime(pcptl.EndDate);
            pcptlm.Cost = pcptl.Cost;
            pcptlm.TStartDate = pcptl.Plan_Campaign_Program_Tactic.StartDate;
            pcptlm.TEndDate = pcptl.Plan_Campaign_Program_Tactic.EndDate;
            pcptlm.PStartDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.StartDate;
            pcptlm.PEndDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.EndDate;
            pcptlm.CStartDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.StartDate;
            pcptlm.CEndDate = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.EndDate;


            User userName = new User();
            try
            {
                userName = objBDSUserRepository.GetTeamMemberDetails(pcptl.CreatedBy, Sessions.ApplicationId);
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            ViewBag.Owner = userName.FirstName + " " + userName.LastName;
            return PartialView("_EditSetupLineitem", pcptlm);
        }

        /// <summary>
        /// Action to Save LineItem data.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="tacticId">Tactic Id</param>
        /// <param name="UserId"></param>
        /// <param name="title"></param>
        /// <returns>Returns Partial View Of edit Setup Tab.</returns>
        [HttpPost]
        public ActionResult SaveLineitem(Plan_Campaign_Program_Tactic_LineItemModel form, string title, string UserId = "", int tacticId = 0)
        {
            //// Check whether current user is loggined user or not.
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
                #region " Get CampaignId, ProgramID and TacticId"
                int cid = 0;
                int pid = 0;
                int tid = 0;

                var objTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == form.PlanTacticId);
                if (objTactic != null)
                {
                    cid = objTactic.Plan_Campaign_Program.PlanCampaignId;
                    pid = objTactic.PlanProgramId;
                    tid = form.PlanTacticId;
                }
                else
                {
                    objTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(t => t.PlanTacticId == tacticId);
                    tid = tacticId;
                    cid = objTactic.Plan_Campaign_Program.PlanCampaignId;
                    pid = objTactic.PlanProgramId;
                }
                #endregion

                //// if  PlanLineItemId is null then insert new record to table.
                if (form.PlanLineItemId == 0)
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        int lineItemId = 0;
                        using (var scope = new TransactionScope())
                        {
                            //// Get duplicate record to check duplication.
                            var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                           join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                                           join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                           join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                           where pcptl.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && pcptl.IsDeleted.Equals(false)
                                           && pcpt.PlanTacticId == tid
                                           select pcpt).FirstOrDefault();
                            //// if duplicate record exist then return duplication message.
                            if (pcptvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { msg = strDuplicateMessage });
                            }
                            else
                            {
                                #region " Insert LineItem record for Specific Improvement Tactic."
                                Plan_Campaign_Program_Tactic_LineItem objLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                objLineitem.PlanTacticId = tacticId;
                                objLineitem.Title = form.Title;
                                objLineitem.LineItemTypeId = form.LineItemTypeId;
                                objLineitem.Description = form.Description;
                                //Added By :Kalpesh Sharma #890 Line Item Dates need to go away
                                objLineitem.StartDate = null;
                                objLineitem.EndDate = null;
                                objLineitem.Cost = form.Cost;
                                objLineitem.CreatedBy = Sessions.User.UserId;
                                objLineitem.CreatedDate = DateTime.Now;
                                db.Entry(objLineitem).State = EntityState.Added;
                                int result = db.SaveChanges();
                                lineItemId = objLineitem.PlanLineItemId;
                                #endregion

                                //// Calculate TotalLineItemCost.
                                var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == tid && l.Title == Common.DefaultLineItemTitle && l.LineItemTypeId == null);
                                double totalLoneitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == tid && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);

                                //// Insert or Modified LineItem.
                                if (objTactic.Cost > totalLoneitemCost)
                                {
                                    double diffCost = objTactic.Cost - totalLoneitemCost;
                                    if (objOtherLineItem == null)
                                    {
                                        Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
                                        objNewLineitem.PlanTacticId = tacticId;
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

                                //// Insert Chnage Log to DB.
                                result = Common.InsertChangeLog(Sessions.PlanId, null, objLineitem.PlanLineItemId, objLineitem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.added);
                                db.SaveChanges();

                            }
                            scope.Complete();
                            string strMessage = Common.objCached.PlanEntityCreated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                            return Json(new { isSaved = true, msg = strMessage, planLineitemID = lineItemId, planCampaignID = cid, planProgramID = pid, planTacticID = tid });
                        }
                    }
                }
                else    //// Update LineItem Record.
                {
                    using (MRPEntities mrp = new MRPEntities())
                    {
                        using (var scope = new TransactionScope())
                        {
                            //// Get duplicate record to check duplication.
                            var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                           join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                                           join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                           join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                           where pcptl.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcptl.PlanLineItemId.Equals(form.PlanLineItemId) && pcptl.IsDeleted.Equals(false)
                                           && pcpt.PlanTacticId == tid
                                           select pcpt).FirstOrDefault();

                            if (pcptvar != null)
                            {
                                string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                                return Json(new { msg = strDuplicateMessage });
                            }
                            else
                            {
                                #region "Update record to Plan_Campaign_Program_Tactic_LineItem table."
                                Plan_Campaign_Program_Tactic_LineItem objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobjw => pcpobjw.PlanLineItemId.Equals(form.PlanLineItemId));
                                objLineitem.Description = form.Description;
                                if (!form.IsOtherLineItem)
                                {
                                    objLineitem.Title = form.Title;
                                    objLineitem.LineItemTypeId = form.LineItemTypeId;

                                    if ((db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(t => t.PlanLineItemId == form.PlanLineItemId).ToList()).Count() == 0 ||
                                        objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower()
                                    || objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower())
                                    {
                                        objLineitem.Cost = form.Cost;
                                    }
                                }

                                objLineitem.ModifiedBy = Sessions.User.UserId;
                                objLineitem.ModifiedDate = DateTime.Now;
                                db.Entry(objLineitem).State = EntityState.Modified;
                                #endregion

                                int result;
                                result = Common.InsertChangeLog(Sessions.PlanId, null, objLineitem.PlanLineItemId, objLineitem.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.updated);
                                result = db.SaveChanges();

                                if (!form.IsOtherLineItem)
                                {
                                    //// Calculate TotalLineItemCost.
                                    double totalLineitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == tid && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);

                                    //// Insert or Modified LineItem Data.
                                    var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(l => l.PlanTacticId == tid && l.Title == Common.DefaultLineItemTitle && l.LineItemTypeId == null);

                                    //// if Tactic total cost is greater than totalLineItem cost then Insert LineItem record otherwise delete objOtherLineItem data from table Plan_Campaign_Program_Tactic_LineItem. 
                                    if (objTactic.Cost > totalLineitemCost)
                                    {
                                        double diffCost = objTactic.Cost - totalLineitemCost;

                                        //// if Tactic does not have OtherLineItem.
                                        if (objOtherLineItem == null)
                                        {
                                            //// Insert New record to table.
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
                                            //// Update record to table.
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
                                        //// Delete OtherLineItem data from table Plan_Campaign_Program_Tactic_LineItem.
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

                                scope.Complete();
                                string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                return Json(new { isSaved = true, msg = strMessage, planLineitemID = form.PlanLineItemId, planCampaignID = cid, planProgramID = pid, planTacticID = tid });

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

        #region Budget Allocation for Line Item Tab
        /// <summary>
        /// Fetch the Line Item Budget Allocation 
        /// </summary>
        /// <param name="id">Line Item Id</param>
        /// <returns></returns>
        public PartialViewResult LoadLineItemBudgetAllocation(int id = 0)
        {
            Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobj => pcpobj.PlanLineItemId.Equals(id));
            if (pcptl == null)
            {
                return null;
            }

            if (Sessions.User.UserId == pcptl.CreatedBy)
            {
                ViewBag.IsOwner = true;
            }
            else
            {
                ViewBag.IsOwner = false;
            }

            List<int> lstEditableTactic = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, new List<int>() { pcptl.Plan_Campaign_Program_Tactic.PlanTacticId }, false);
            if (lstEditableTactic.Contains(pcptl.Plan_Campaign_Program_Tactic.PlanTacticId))
            {
                ViewBag.IsAllowCustomRestriction = true;
            }
            else
            {
                ViewBag.IsAllowCustomRestriction = false;
            }

            Plan_Campaign_Program_Tactic_LineItemModel pcptlm = new Plan_Campaign_Program_Tactic_LineItemModel();
            if (pcptl.LineItemTypeId == null)
            {
                pcptlm.IsOtherLineItem = true;
            }
            else
            {
                pcptlm.IsOtherLineItem = false;
            }

            pcptlm.PlanTacticId = pcptl.PlanTacticId;
            pcptlm.PlanLineItemId = pcptl.PlanLineItemId;

            var objPlan = db.Plans.FirstOrDefault(plan => plan.PlanId == pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId);
            pcptlm.AllocatedBy = objPlan.AllocatedBy;

            double totalLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(l => l.PlanTacticId == pcptl.PlanTacticId && l.LineItemTypeId != null && l.IsDeleted == false).ToList().Sum(l => l.Cost);
            double TacticCost = pcptl.Plan_Campaign_Program_Tactic.Cost;
            double diffCost = TacticCost - totalLineItemCost;
            double otherLineItemCost = diffCost < 0 ? 0 : diffCost;

            ViewBag.tacticCost = TacticCost;
            ViewBag.totalLineItemCost = totalLineItemCost;
            ViewBag.otherLineItemCost = otherLineItemCost;
            pcptlm.Cost = pcptl.Cost;

            return PartialView("_SetupLineitemBudgetAllocation", pcptlm);
        }

        /// <summary>
        /// Action to Save Line Item Allocation.
        /// </summary>
        /// <param name="form">Form object of Plan_Campaign_ProgramModel.</param>
        /// <param name="CostInputValues">Cost Input Values.</param>
        /// <param name="UserId">User Id</param>
        /// <param name="title"></param>
        /// <returns>Returns Action Result.</returns>
        [HttpPost]
        public ActionResult SaveLineItemBudgetAllocation(Plan_Campaign_Program_Tactic_LineItemModel form, string CostInputValues, string UserId = "", string title = "")
        {
            //// check whether current userId is loggined user or not.
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    return Json(new { IsSaved = false, msg = Common.objCached.LoginWithSameSession, JsonRequestBehavior.AllowGet });
                }
            }
            try
            {
                string[] arrCostInputValues = CostInputValues.Split(',');

                ////Start Added by Mitesh Vaishnav for PL ticket #752 - Update line item cost with the total cost from the monthly/quarterly allocation

                //// Check when monthly planned cost of tactic is lower than total monthly planned cost of line items than update monthly planned cost of tactic
                var lineItemIds = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.PlanTacticId == form.PlanTacticId && a.IsDeleted == false && a.PlanLineItemId != form.PlanLineItemId).Select(a => a.PlanLineItemId).ToList();

                ////list of Total monthly planned cost
                var lstMonthlyLineItemCost = db.Plan_Campaign_Program_Tactic_LineItem_Cost
                    .Where(lineCost => lineItemIds.Contains(lineCost.PlanLineItemId))
                    .GroupBy(lineCost => lineCost.Period)
                    .Select(lineCost => new
                    {
                        Period = lineCost.Key,
                        Cost = lineCost.Sum(b => b.Value)
                    }).ToList();

                ////List of monthly plaaned cost of tactic
                var lstMonthlyTacticCost = db.Plan_Campaign_Program_Tactic_Cost.Where(_tacCost => _tacCost.PlanTacticId == form.PlanTacticId).ToList();
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
                if (TotalNewPlannedCost >= TotalTacticCost)
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
                            string period = PeriodChar + (i + 1).ToString();
                            double monthlyTotalLineItemCost = lstMonthlyLineItemCost.Where(lineCost => lineCost.Period == period).FirstOrDefault() == null ? 0 : lstMonthlyLineItemCost.Where(lineCost => lineCost.Period == period).FirstOrDefault().Cost;
                            monthlyTotalLineItemCost = monthlyTotalLineItemCost + Convert.ToDouble(arrCostInputValues[i]);
                            double monthlyTotalTacticCost = lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).FirstOrDefault() == null ? 0 : lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).FirstOrDefault().Value;
                            if (monthlyTotalLineItemCost > monthlyTotalTacticCost || !isBudgetLower)
                            {
                                lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).ToList().ForEach(_tacCost => { _tacCost.Value = monthlyTotalLineItemCost; db.Entry(_tacCost).State = EntityState.Modified; });
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
                                QuarterList.Add(PeriodChar + (QuarterCnt + J).ToString());
                            }
                            //string period = PeriodChar + QuarterCnt.ToString();
                            double monthlyTotalLineItemCost = lstMonthlyLineItemCost.Where(lineCost => QuarterList.Contains(lineCost.Period)).ToList().Sum(lineCost => lineCost.Cost);
                            monthlyTotalLineItemCost = monthlyTotalLineItemCost + Convert.ToDouble(arrCostInputValues[i]);
                            double monthlyTotalTacticCost = lstMonthlyTacticCost.Where(_tacCost => QuarterList.Contains(_tacCost.Period)).ToList().Sum(_tacCost => _tacCost.Value);
                            if (monthlyTotalLineItemCost > monthlyTotalTacticCost || !isBudgetLower)
                            {
                                string period = QuarterList[0].ToString();
                                double diffCost = monthlyTotalLineItemCost - monthlyTotalTacticCost;
                                if (diffCost >= 0)
                                {
                                    lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).ToList().ForEach(_tacCost => { _tacCost.Value = _tacCost.Value + diffCost; db.Entry(_tacCost).State = EntityState.Modified; });
                                }
                                int periodCount = 0;
                                ////If cost diffrence is lower than 0 than reduce it from quarter in series of 1st month of quarter,2nd month of quarter...
                                while (diffCost < 0)
                                {
                                    period = QuarterList[periodCount].ToString();
                                    double tacticCost = lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).ToList().Sum(_tacCost => _tacCost.Value);
                                    if ((diffCost + tacticCost) >= 0)
                                    {
                                        lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).ToList().ForEach(_tacCost => { _tacCost.Value = _tacCost.Value + diffCost; db.Entry(_tacCost).State = EntityState.Modified; });
                                    }
                                    else
                                    {
                                        lstMonthlyTacticCost.Where(_tacCost => _tacCost.Period == period).ToList().ForEach(_tacCost => { _tacCost.Value = 0; db.Entry(_tacCost).State = EntityState.Modified; });
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

                int cid = objTactic.Plan_Campaign_Program.PlanCampaignId;
                int pid = objTactic.PlanProgramId;
                int tid = form.PlanTacticId;

                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        //// Get duplicate record to check duplication.
                        var pcptvar = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                                       join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                                       join pcp in db.Plan_Campaign_Program on pcpt.PlanProgramId equals pcp.PlanProgramId
                                       join pc in db.Plan_Campaign on pcp.PlanCampaignId equals pc.PlanCampaignId
                                       where pcptl.Title.Trim().ToLower().Equals(form.Title.Trim().ToLower()) && !pcptl.PlanLineItemId.Equals(form.PlanLineItemId) && pcptl.IsDeleted.Equals(false)
                                       && pcpt.PlanTacticId == form.PlanTacticId
                                       select pcpt).FirstOrDefault();

                        //// If duplicate record exist then return dupication message.
                        if (pcptvar != null)
                        {
                            string strDuplicateMessage = string.Format(Common.objCached.PlanEntityDuplicated, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                            return Json(new { IsSaved = false, msg = strDuplicateMessage, JsonRequestBehavior.AllowGet });
                        }
                        else
                        {
                            Plan_Campaign_Program_Tactic_LineItem objLineitem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobjw => pcpobjw.PlanLineItemId.Equals(form.PlanLineItemId));
                            if (!form.IsOtherLineItem)
                            {
                                objLineitem.Title = title;
                                objLineitem.Cost = UpdateBugdetAllocationCost(arrCostInputValues, form.Cost);
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

                                //// if Tactic total cost is greater than totalLineItem cost then Insert LineItem record otherwise delete objOtherLineItem data from table Plan_Campaign_Program_Tactic_LineItem.  
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
                                    //// if Tactic Cost less than totalLineItemCost then remove OtherLineItem data from table Plan_Campaign_Program_Tactic_LineItem.
                                    if (objOtherLineItem != null)
                                    {
                                        objOtherLineItem.IsDeleted = true;
                                        objOtherLineItem.Cost = 0;
                                        objOtherLineItem.Description = string.Empty;
                                        db.Entry(objOtherLineItem).State = EntityState.Modified;

                                        //// Delete LineItem Actuals value from table Plan_Campaign_Program_Tactic_LineItem_Actual.
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
                                                var updatePlanTacticBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (i + 1))).FirstOrDefault();
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
                                            objlineItemCost.Period = PeriodChar + (i + 1);
                                            objlineItemCost.Value = Convert.ToDouble(arrCostInputValues[i]);
                                            objlineItemCost.CreatedBy = Sessions.User.UserId;
                                            objlineItemCost.CreatedDate = DateTime.Now;
                                            db.Entry(objlineItemCost).State = EntityState.Added;
                                        }
                                    }
                                }
                                else if (arrCostInputValues.Length == 4)
                                {
                                    ////QurterList which contains list of month as per quarter. e.g. for Q1, list is Y1,Y2 and Y3
                                    int QuarterCnt = 1;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        // Start - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
                                        bool isExists = false;
                                        if (PrevAllocationList != null)
                                        {
                                            if (PrevAllocationList.Count > 0)
                                            {
                                                var thisQuartersMonthList = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt)) || pb.Period == (PeriodChar + (QuarterCnt + 1)) || pb.Period == (PeriodChar + (QuarterCnt + 2))).ToList().OrderBy(a => a.Period).ToList();
                                                var thisQuarterFirstMonthBudget = thisQuartersMonthList.FirstOrDefault();

                                                if (thisQuarterFirstMonthBudget != null)
                                                {
                                                    if (arrCostInputValues[i] != "")
                                                    {
                                                        var thisQuarterOtherMonthBudget = thisQuartersMonthList.Where(quartBudget => quartBudget.Period != thisQuarterFirstMonthBudget.Period).ToList().Sum(quartBudget => quartBudget.Value);
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
                                                                        thisQuarterFirstMonthBudget = PrevAllocationList.Where(pb => pb.Period == (PeriodChar + (QuarterCnt + j))).FirstOrDefault();
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
                                        //// If record does not exist then insert new record to table.
                                        if (!isExists && arrCostInputValues[i] != "")
                                        {
                                            // End - Added by Sohel Pathan on 05/09/2014 for PL ticket #759
                                            Plan_Campaign_Program_Tactic_LineItem_Cost objlineItemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
                                            objlineItemCost.PlanLineItemId = form.PlanLineItemId;
                                            objlineItemCost.Period = PeriodChar + QuarterCnt;
                                            objlineItemCost.Value = Convert.ToDouble(arrCostInputValues[i]);
                                            objlineItemCost.CreatedBy = Sessions.User.UserId;
                                            objlineItemCost.CreatedDate = DateTime.Now;
                                            db.Entry(objlineItemCost).State = EntityState.Added;
                                        }
                                        QuarterCnt = QuarterCnt + 3;
                                    }
                                }

                                db.SaveChanges();
                                scope.Complete();
                                string strMessage = Common.objCached.PlanEntityAllocationUpdated.Replace("{0}", Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                return Json(new
                                {
                                    IsSaved = true,
                                    CamapignId = objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId,
                                    ProgramId = objLineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanProgramId,
                                    TacticId = objLineitem.PlanTacticId,
                                    PlanLineItemId = form.PlanLineItemId,
                                    msg = strMessage,
                                    JsonRequestBehavior.AllowGet
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { IsSaved = false, msg = Common.objCached.ErrorOccured, JsonRequestBehavior.AllowGet });
        }

        #endregion

        #region "Actuals Tab for LineItem"
        /// <summary>
        /// Added By: Viral Kadiya on 11/11/2014.
        /// Action to Get Actuals cost Value Of line item.
        /// </summary>
        /// <param name="id">Plan line item Id.</param>
        /// <returns>Returns PartialView Result of line item actuals Value.</returns>
        public ActionResult LoadActualsLineItem(int id)
        {
            try
            {
                ViewBag.ParentTacticStatus = GetTacticStatusByPlanLineItemId(id);

                //// Get LineItem data by Id.
                Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(pcpobj => pcpobj.PlanLineItemId.Equals(id));
                bool isOtherLineItem = true;

                //// Set isOtherLineItem flag.
                if (pcptl != null && pcptl.LineItemTypeId != null)
                    isOtherLineItem = false;

                ViewBag.IsOtherLineItem = isOtherLineItem;
                return PartialView("_ActualLineitem");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Added By: Viral Kadiya on 11/11/2014.
        /// Action to Get Actuals cost Value Of line item.
        /// </summary>
        /// <param name="planlineitemid">Plan line item Id.</param>
        /// <returns>Returns Parent Tactic Status.</returns>
        public string GetTacticStatusByPlanLineItemId(int planlineitemid)
        {
            string strTacticStatus = string.Empty;
            try
            {
                //// Get Tactic status by PlanLineItemId.
                var lstPCPT = (from pcptl in db.Plan_Campaign_Program_Tactic_LineItem
                               join pcpt in db.Plan_Campaign_Program_Tactic on pcptl.PlanTacticId equals pcpt.PlanTacticId
                               where pcptl.PlanLineItemId == planlineitemid && pcpt.IsDeleted == false
                               select pcpt).FirstOrDefault();
                if (lstPCPT != null)
                    strTacticStatus = lstPCPT.Status;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return strTacticStatus;
        }
        #endregion

        /// <summary>
        /// Added By: Mitesh Vaishnav.
        /// Action to Create Campaign.
        /// </summary>
        /// <param name="id">Tactic Id</param>
        /// <returns>Returns Partial View Of Campaign.</returns>
        public PartialViewResult createLineitem(int id)
        {
            int tacticId = id;
            var pcpt = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId.Equals(tacticId) && _tactic.IsDeleted == false).FirstOrDefault();
            var objPlan = pcpt != null ? pcpt.Plan_Campaign_Program.Plan_Campaign.Plan : null;
            if (objPlan == null)
            {
                return null;
            }
            ViewBag.RedirectType = false;

            /// Get LineItemTypes List by ModelId for current PlanId.
            var lineItemTypes = from lit in db.LineItemTypes
                                where lit.ModelId == objPlan.ModelId && lit.IsDeleted == false
                                orderby lit.Title,new AlphaNumericComparer()
                                select lit;
            foreach (var item in lineItemTypes)
            {
                item.Title = HttpUtility.HtmlDecode(item.Title);
            }
            ViewBag.lineItemTypes = lineItemTypes;
            User userName = new User();
            try
            {
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                ViewBag.IsServiceUnavailable = false;
                userName = objBDSUserRepository.GetTeamMemberDetails(Sessions.User.UserId, Sessions.ApplicationId);
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
                    ViewBag.IsServiceUnavailable = true;
                }
            }
            ViewBag.Owner = userName.FirstName + " " + userName.LastName;

            #region "Set data to Plan_Campaign_Program_Tactic_LineItemModel to pass into PartialView"
            Plan_Campaign_Program_Tactic_LineItemModel pc = new Plan_Campaign_Program_Tactic_LineItemModel();
            pc.PlanTacticId = tacticId;
            ViewBag.TacticTitle = HttpUtility.HtmlDecode(pcpt.Title);
            ViewBag.ProgramTitle = HttpUtility.HtmlDecode(pcpt.Plan_Campaign_Program.Title);
            ViewBag.CampaignTitle = HttpUtility.HtmlDecode(pcpt.Plan_Campaign_Program.Plan_Campaign.Title);
            pc.StartDate = GetCurrentDateBasedOnPlan();
            pc.EndDate = GetCurrentDateBasedOnPlan(true);
            pc.Cost = 0;
            pc.AllocatedBy = objPlan.AllocatedBy;
            pc.IsOtherLineItem = false;
            pc.AllocatedBy = objPlan.AllocatedBy;
            #endregion

            return PartialView("_EditSetupLineitem", pc);
        }
        #endregion

        #region "Common Functions"

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Load Inspect Popup for all sections.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="section">Decide which section to open for Inspect Popup (tactic,program or campaign)</param>
        /// <param name="TabValue">Tab value of Popup.</param>
        /// <param name="InspectPopupMode"></param>
        /// <param name="parentId"></param>
        /// <param name="RequestedModule"></param>
        /// <returns>Returns Partial View Of Inspect Popup.</returns>
        public ActionResult LoadInspectPopup(int id, string section, string TabValue = "Setup", string InspectPopupMode = "", int parentId = 0, string RequestedModule = "")
        {
            try
            {
                ViewBag.InspectMode = InspectPopupMode;

                if (!string.IsNullOrEmpty(RequestedModule))
                {
                    ViewBag.RedirectType = RequestedModule;
                }

                //// If Id is null then return section respective PartialView.
                if (id == 0)
                {
                    if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        ViewBag.PlanId = parentId;
                        ViewBag.InspectPopup = TabValue;
                        ViewBag.CampaignDetail = null;
                        return PartialView("_InspectPopupCampaign", null);
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        ViewBag.CampaignId = parentId;
                        ViewBag.InspectPopup = TabValue;
                        ViewBag.ProgramDetail = null;
                        return PartialView("_InspectPopupProgram", null);
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        ViewBag.PlanProgrameId = parentId;
                        ViewBag.InspectPopup = TabValue;
                        ViewBag.TacticDetail = null;
                        ViewBag.IsTacticActualsAddEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);
                        return PartialView("InspectPopup", null);
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        ViewBag.PlanProgrameId = parentId;
                        ViewBag.InspectPopup = TabValue;
                        ViewBag.TacticDetail = null;
                        return PartialView("_InspectPopupImprovementTactic", null);
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.LineItem).ToLower())
                    {
                        ViewBag.tacticId = parentId;
                        ViewBag.InspectPopup = TabValue;
                        ViewBag.TacticDetail = null;
                        ViewBag.IsTacticActualsAddEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);
                        return PartialView("_InspectPopupLineitem", null);
                    }
                }

                // To get permission status for Add/Edit Actual, By dharmraj PL #519
                ViewBag.IsTacticActualsAddEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);

                //// Added by Pratik Chauhan for functional review points

                #region "Initialize variables"
                Plan_Campaign_Program_Tactic objPlan_Campaign_Program_Tactic = null;
                Plan_Campaign_Program objPlan_Campaign_Program = null;
                Plan_Campaign objPlan_Campaign = null;
                Plan_Improvement_Campaign_Program_Tactic objPlan_Improvement_Campaign_Program_Tactic = null;
                Plan_Campaign_Program_Tactic_LineItem objPlan_Campaign_Program_Tactic_LineItem = null;
                bool IsPlanEditable = false;
                #endregion

                //// load section wise data to ViewBag.
                if (Convert.ToString(section) != "")
                {
                    if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        objPlan_Campaign_Program_Tactic = db.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(id)).FirstOrDefault();
                        ViewBag.PlanId = objPlan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
                        //Start - Added by Mitesh Vaishnav for PL ticket 746 - Edit Own and Subordinates Tactics Doesnt work
                        //Verify that existing user has created tactic or it has subordinate permission and tactic owner is subordinate of existing user
                        bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                        List<Guid> lstSubordinatesIds = new List<Guid>();
                        if (IsTacticAllowForSubordinates)
                        {
                            lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
                        }
                        //End - Added by Mitesh Vaishnav for PL ticket 746 - Edit Own and Subordinates Tactics Doesnt work
                        //Modify by Mitesh Vaishnav for PL ticket 746
                        if ((objPlan_Campaign_Program_Tactic.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(objPlan_Campaign_Program_Tactic.CreatedBy)))
                        {
                            IsPlanEditable = true;
                        }

                        ViewBag.CampaignId = objPlan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId;
                        ViewBag.PlanProgrameId = objPlan_Campaign_Program_Tactic.PlanProgramId;
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        objPlan_Campaign_Program = db.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(id)).FirstOrDefault();
                        ViewBag.PlanId = objPlan_Campaign_Program.Plan_Campaign.PlanId;
                        if (objPlan_Campaign_Program.CreatedBy.Equals(Sessions.User.UserId))
                        {
                            IsPlanEditable = true;
                        }

                        ViewBag.CampaignId = objPlan_Campaign_Program.PlanCampaignId;
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        objPlan_Campaign = db.Plan_Campaign.Where(pcpobjw => pcpobjw.PlanCampaignId.Equals(id)).FirstOrDefault();
                        ViewBag.PlanId = objPlan_Campaign.PlanId;

                        if (objPlan_Campaign.CreatedBy.Equals(Sessions.User.UserId))
                        {
                            IsPlanEditable = true;
                        }
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        objPlan_Improvement_Campaign_Program_Tactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(picpobjw => picpobjw.ImprovementPlanTacticId.Equals(id)).FirstOrDefault();
                        ViewBag.PlanId = objPlan_Improvement_Campaign_Program_Tactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId;

                        if (objPlan_Improvement_Campaign_Program_Tactic.CreatedBy.Equals(Sessions.User.UserId))
                        {
                            IsPlanEditable = true;
                        }
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.LineItem).ToLower())
                    {
                        objPlan_Campaign_Program_Tactic_LineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(pcptl => pcptl.PlanLineItemId.Equals(id)).FirstOrDefault();
                        ViewBag.LineItemId = objPlan_Campaign_Program_Tactic_LineItem.PlanLineItemId;
                        ViewBag.LineItemTitle = objPlan_Campaign_Program_Tactic_LineItem.Title;
                        ViewBag.PlanId = objPlan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
                        ViewBag.tacticId = objPlan_Campaign_Program_Tactic_LineItem.PlanTacticId;

                        if (objPlan_Campaign_Program_Tactic_LineItem.CreatedBy.Equals(Sessions.User.UserId))
                        {
                            IsPlanEditable = true;
                        }

                        if (objPlan_Campaign_Program_Tactic_LineItem.LineItemTypeId == null)
                        {
                            ViewBag.IsOtherLineItem = true;
                        }
                        else
                        {
                            ViewBag.IsOtherLineItem = false;
                        }

                        ViewBag.CampaignId = objPlan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId;
                        ViewBag.PlanProgrameId = objPlan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.PlanProgramId;
                    }
                    // Start - Added by Sohel Pathan on 07/11/2014 for PL ticket #811
                    else if (Convert.ToString(section).Equals(Enums.Section.Plan.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        ViewBag.PlanId = id;
                    }
                    // End - Added by Sohel Pathan on 07/11/2014 for PL ticket #811
                }


                if (Convert.ToString(section) != "")
                {
                    if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Tactic).ToLower())
                    {
                        DateTime todaydate = DateTime.Now;

                        //// if Tactic status based on Function CheckAfterApprovedStatus
                        if (Common.CheckAfterApprovedStatus(objPlan_Campaign_Program_Tactic.Status))
                        {
                            if (todaydate > objPlan_Campaign_Program_Tactic.StartDate && todaydate < objPlan_Campaign_Program_Tactic.EndDate)
                            {
                                objPlan_Campaign_Program_Tactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                            }
                            else if (todaydate > objPlan_Campaign_Program_Tactic.EndDate)
                            {
                                objPlan_Campaign_Program_Tactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                            }

                            db.Entry(objPlan_Campaign_Program_Tactic).State = EntityState.Modified;
                            int result = db.SaveChanges();
                        }
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Program).ToLower())
                    {
                        DateTime todaydate = DateTime.Now;
                        if (Common.CheckAfterApprovedStatus(objPlan_Campaign_Program.Status))
                        {
                            if (todaydate > objPlan_Campaign_Program.StartDate && todaydate < objPlan_Campaign_Program.EndDate)
                            {
                                objPlan_Campaign_Program.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                            }
                            else if (todaydate > objPlan_Campaign_Program.EndDate)
                            {
                                objPlan_Campaign_Program.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                            }

                            db.Entry(objPlan_Campaign_Program).State = EntityState.Modified;
                            int result = db.SaveChanges();
                        }
                    }
                    else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Campaign).ToLower())
                    {
                        DateTime todaydate = DateTime.Now;
                        if (Common.CheckAfterApprovedStatus(objPlan_Campaign.Status))
                        {
                            if (todaydate > objPlan_Campaign.StartDate && todaydate < objPlan_Campaign.EndDate)
                            {
                                objPlan_Campaign.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                            }
                            else if (todaydate > objPlan_Campaign.EndDate)
                            {
                                objPlan_Campaign.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                            }

                            db.Entry(objPlan_Campaign).State = EntityState.Modified;
                            int result = db.SaveChanges();
                        }
                    }

                    //// Custom Restrictions
                    if (IsPlanEditable)
                    {
                        //// Start - Added by Sohel Pathan on 27/01/2015 for PL ticket #1140
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        int itemId = 0;

                        if (section.ToString().Equals(Enums.Section.Campaign.ToString(), StringComparison.OrdinalIgnoreCase) && objPlan_Campaign != null)
                        {
                            if (objPlan_Campaign.Plan_Campaign_Program != null)
                            {
                                List<int> programIds = objPlan_Campaign.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false)).Select(program => program.PlanProgramId).ToList();
                                if (programIds.Count() > 0)
                                {
                                    planTacticIds = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && programIds.Contains(tactic.Plan_Campaign_Program.PlanProgramId))
                                                                                    .Select(tactic => tactic.PlanTacticId).ToList();
                                    lstAllowedEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                                    planTacticIds.ForEach(tactic => { if (!lstAllowedEntityIds.Contains(tactic)) { IsPlanEditable = false; } });
                                }
                            }
                        }
                        else if (section.ToString().Equals(Enums.Section.Program.ToString(), StringComparison.OrdinalIgnoreCase) && objPlan_Campaign_Program != null)
                        {
                            if (objPlan_Campaign_Program.Plan_Campaign_Program_Tactic != null)
                            {
                                planTacticIds = objPlan_Campaign_Program.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false)).Select(tactic => tactic.PlanTacticId).ToList();
                                lstAllowedEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                                planTacticIds.ForEach(tactic => { if (!lstAllowedEntityIds.Contains(tactic)) { IsPlanEditable = false; } });
                            }
                        }
                        else if (section.ToString().Equals(Enums.Section.Tactic.ToString(), StringComparison.OrdinalIgnoreCase) && objPlan_Campaign_Program_Tactic != null)
                        {
                            itemId = objPlan_Campaign_Program_Tactic.PlanTacticId;
                            planTacticIds.Add(itemId);
                            lstAllowedEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                            if (lstAllowedEntityIds.Contains(itemId))
                            {
                                IsPlanEditable = true;
                            }
                            else
                            {
                                IsPlanEditable = false;
                            }
                        }
                        else if (section.ToString().Equals(Enums.Section.LineItem.ToString(), StringComparison.OrdinalIgnoreCase) && objPlan_Campaign_Program_Tactic_LineItem != null)
                        {
                            if (objPlan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic != null)
                            {
                                itemId = objPlan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.PlanTacticId;
                                planTacticIds.Add(itemId);
                                lstAllowedEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                                if (lstAllowedEntityIds.Contains(itemId))
                                {
                                    IsPlanEditable = true;
                                }
                                else
                                {
                                    IsPlanEditable = false;
                                }
                            }
                        }
                        //// End - Added by Sohel Pathan on 27/01/2015 for PL ticket #1140
                    }
                }

                ViewBag.IsPlanEditable = IsPlanEditable;
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
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);

                }
            }

            InspectModel im = GetInspectModel(id, section, false);      //// Modified by :- Sohel Pathan on 27/05/2014 for PL ticket #425
            ViewBag.TacticDetail = im;
            ViewBag.InspectPopup = TabValue;


            if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Program).ToLower())
            {
                ViewBag.ProgramDetail = im;
                return PartialView("_InspectPopupProgram", im);
            }
            else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Campaign).ToLower())
            {
                ViewBag.CampaignDetail = im;
                return PartialView("_InspectPopupCampaign", im);
            }
            else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
            {
                ViewBag.CampaignDetail = im;
                if (InspectPopupMode == Enums.InspectPopupMode.Edit.ToString())
                {
                    ViewBag.InspectMode = Enums.InspectPopupMode.Edit.ToString();
                }
                else if (InspectPopupMode == Enums.InspectPopupMode.Add.ToString())
                {
                    ViewBag.InspectMode = Enums.InspectPopupMode.Add.ToString();
                }
                else
                {
                    ViewBag.InspectMode = Enums.InspectPopupMode.ReadOnly.ToString();
                }
                return PartialView("_InspectPopupImprovementTactic", im);
            }
            else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.LineItem).ToLower())
            {
                return PartialView("_InspectPopupLineitem");
            }
            // Start - Added by Sohel Pathan on 07/11/2014 for PL ticket #811
            else if (Convert.ToString(section).Equals(Enums.Section.Plan.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                bool IsPlanEditable = false;
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
                        return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);

                    }
                }
                // Get current user permission for edit own and subordinates plans.
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                // To get permission status for Plan Edit, By dharmraj PL #519
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);

                if (im.OwnerId.Equals(Sessions.User.UserId))
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditAllAuthorized)
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditSubordinatesAuthorized)
                {
                    if (lstOwnAndSubOrdinates.Contains(im.OwnerId))
                    {
                        IsPlanEditable = true;
                    }
                }

                ViewBag.IsPlanEditable = IsPlanEditable;
                ViewBag.IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
                ViewBag.PlanDetails = im;
                if (InspectPopupMode == Enums.InspectPopupMode.ReadOnly.ToString())
                {
                    ViewBag.InspectMode = Enums.InspectPopupMode.ReadOnly.ToString();
                }
                else if (InspectPopupMode == Enums.InspectPopupMode.Edit.ToString())
                {
                    ViewBag.InspectMode = Enums.InspectPopupMode.Edit.ToString();
                }
                else
                {
                    ViewBag.InspectMode = "";
                }
                return PartialView("_InspectPopupPlan", im);
            }
            // End - Added by Sohel Pathan on 07/11/2014 for PL ticket #811
            return PartialView("InspectPopup", im);
        }

        /// <summary>
        /// Action to Save SyncToIntegration value for all Inspect Popup.
        /// </summary>
        /// <param name="id">Plan Tactic Id.</param>
        /// <param name="section">Decide which section to open for Inspect Popup (tactic,program or campaign)</param>
        /// <param name="IsDeployedToIntegration">bool value</param>
        public JsonResult SaveSyncToIntegration(int id, string section, bool IsDeployedToIntegration)
        {
            bool returnValue = false;
            string strPlanEntity = string.Empty;
            try
            {
                if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                {
                    var objTactic = db.Plan_Campaign_Program_Tactic.FirstOrDefault(_tactic => _tactic.PlanTacticId == id);
                    objTactic.IsDeployedToIntegration = IsDeployedToIntegration;
                    db.Entry(objTactic).State = EntityState.Modified;
                    db.SaveChanges();
                    returnValue = true;
                    strPlanEntity = Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]; // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                }
                else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                {
                    var objProgram = db.Plan_Campaign_Program.FirstOrDefault(_program => _program.PlanProgramId == id);
                    objProgram.IsDeployedToIntegration = IsDeployedToIntegration;
                    db.Entry(objProgram).State = EntityState.Modified;
                    db.SaveChanges();
                    returnValue = true;
                    strPlanEntity = Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()];    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                }
                else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                {
                    var objCampaign = db.Plan_Campaign.FirstOrDefault(_campaign => _campaign.PlanCampaignId == id);
                    objCampaign.IsDeployedToIntegration = IsDeployedToIntegration;
                    db.Entry(objCampaign).State = EntityState.Modified;
                    db.SaveChanges();
                    returnValue = true;
                    strPlanEntity = Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()];   // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                }
                else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                {
                    var objITactic = db.Plan_Improvement_Campaign_Program_Tactic.FirstOrDefault(_imprvTactic => _imprvTactic.ImprovementPlanTacticId == id);
                    objITactic.IsDeployedToIntegration = IsDeployedToIntegration;
                    db.Entry(objITactic).State = EntityState.Modified;
                    db.SaveChanges();
                    returnValue = true;
                    strPlanEntity = Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()];  // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            string strMessage = Common.objCached.PlanEntityUpdated.Replace("{0}", strPlanEntity);    // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
            return Json(new { result = returnValue, msg = strMessage }, JsonRequestBehavior.AllowGet); // Modified by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get InspectModel.
        /// </summary>
        /// Modifled by :- Sohel Pathan on 27/05/2014 for PL ticket #425, Default parameter added named isStatusChange
        /// <param name="id">Plan Tactic Id.</param>
        /// <param section="id">.Decide which section to open for Inspect Popup (tactic,program or campaign)</param>
        /// <param name="section"></param>
        /// <param name="isStatusChange"></param>
        /// <returns>Returns InspectModel.</returns>
        private InspectModel GetInspectModel(int id, string section, bool isStatusChange = true)
        {

            InspectModel imodel = new InspectModel();
            string statusapproved = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Approved.ToString()].ToString();
            string statusinprogress = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.InProgress.ToString()].ToString();
            string statuscomplete = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Complete.ToString()].ToString();
            string statusdecline = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Decline.ToString()].ToString();
            string statussubmit = RevenuePlanner.Helpers.Enums.TacticStatusValues[RevenuePlanner.Helpers.Enums.TacticStatus.Submitted.ToString()].ToString();
            string statusAllocatedByNone = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.none.ToString()].ToString().ToLower();
            string statusAllocatedByDefault = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();


            try
            {
                //// Get Inspect Model for Tactic InspectPopup.
                if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                {
                    double budgetAllocation = db.Plan_Campaign_Program_Tactic_Cost.Where(tacCost => tacCost.PlanTacticId == id).ToList().Sum(tacCost => tacCost.Value);
                    Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(pcptobj => pcptobj.PlanTacticId.Equals(id) && pcptobj.IsDeleted == false).FirstOrDefault();
                    imodel = new InspectModel()
                              {
                                  PlanTacticId = pcpt.PlanTacticId,
                                  TacticTitle = pcpt.Title,
                                  TacticTypeTitle = pcpt.TacticType.Title,
                                  CampaignTitle = pcpt.Plan_Campaign_Program.Plan_Campaign.Title,
                                  ProgramTitle = pcpt.Plan_Campaign_Program.Title,
                                  Status = pcpt.Status,
                                  TacticTypeId = pcpt.TacticTypeId,
                                  ColorCode = pcpt.TacticType.ColorCode,
                                  Description = pcpt.Description,
                                  PlanCampaignId = pcpt.Plan_Campaign_Program.PlanCampaignId,
                                  PlanProgramId = pcpt.PlanProgramId,
                                  OwnerId = pcpt.CreatedBy,
                                  //Modified By : Kalpesh Sharma #864 Add Actuals: Unable to update actuals % 864_Actuals.jpg %
                                  // If tactic has a line item at that time we have consider Project cost as sum of all the active line items
                                  Cost = (pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == pcpt.PlanTacticId && s.IsDeleted == false)).Count() > 0
                                   && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy != statusAllocatedByNone && pcpt.Plan_Campaign_Program.Plan_Campaign.Plan.AllocatedBy != statusAllocatedByDefault
                                   ?
                                   (budgetAllocation > 0 ? budgetAllocation : (pcpt.Plan_Campaign_Program_Tactic_LineItem.Where(s => s.PlanTacticId == pcpt.PlanTacticId && s.IsDeleted == false)).Sum(a => a.Cost))
                                    : pcpt.Cost,
                                  StartDate = pcpt.StartDate,
                                  EndDate = pcpt.EndDate,
                                  CostActual = pcpt.CostActual == null ? 0 : pcpt.CostActual,
                                  IsDeployedToIntegration = pcpt.IsDeployedToIntegration,
                                  LastSyncDate = pcpt.LastSyncDate,
                                  StageId = pcpt.StageId,
                                  StageTitle = pcpt.Stage.Title,
                                  StageLevel = pcpt.Stage.Level,
                                  ProjectedStageValue = pcpt.ProjectedStageValue,
                                  TacticCustomName = pcpt.TacticCustomName
                              };


                    TacticStageValue varTacticStageValue = Common.GetTacticStageRelationForSingleTactic(pcpt, false);
                    //// Set MQL
                    string stageMQL = Enums.Stage.MQL.ToString();
                    int levelMQL = db.Stages.Single(stage => stage.ClientId.Equals(Sessions.User.ClientId) && stage.Code.Equals(stageMQL)).Level.Value;
                    int tacticStageLevel = Convert.ToInt32(pcpt.Stage.Level);
                    if (tacticStageLevel < levelMQL)
                    {
                        imodel.MQLs = varTacticStageValue.MQLValue;
                    }
                    else if (tacticStageLevel == levelMQL)
                    {
                        imodel.MQLs = Convert.ToDouble(imodel.ProjectedStageValue);
                    }
                    else if (tacticStageLevel > levelMQL)
                    {
                        imodel.MQLs = 0;
                        TempData["TacticMQL"] = "N/A";
                    }
                    imodel.MQLs = Math.Round((double)imodel.MQLs, 0, MidpointRounding.AwayFromZero);
                    // Set Revenue
                    imodel.Revenues = Math.Round(varTacticStageValue.RevenueValue, 2);

                    imodel.IsIntegrationInstanceExist = CheckIntegrationInstanceExist(pcpt.TacticType.Model);
                }
                else if (section == Convert.ToString(Enums.Section.Program).ToLower())  //// Get Inspect Model for Program InspectPopup.
                {
                    var objPlan_Campaign_Program = db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == id && pcp.IsDeleted == false).FirstOrDefault();

                    if (isStatusChange)     //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                    {
                        var tacticobjList = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == id && pcpt.IsDeleted == false).ToList();
                        int cntSumbitTacticStatus = tacticobjList.Where(pcpt => !pcpt.Status.Equals(statussubmit)).Count();
                        int cntApproveTacticStatus = tacticobjList.Where(pcpt => (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();
                        int cntDeclineTacticStatus = tacticobjList.Where(pcpt => !pcpt.Status.Equals(statusdecline)).Count();

                        if (cntSumbitTacticStatus == 0)
                        {
                            objPlan_Campaign_Program.Status = statussubmit;
                            db.Entry(objPlan_Campaign_Program).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (cntApproveTacticStatus == 0)
                        {
                            objPlan_Campaign_Program.Status = statusapproved;
                            db.Entry(objPlan_Campaign_Program).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (cntDeclineTacticStatus == 0)
                        {
                            objPlan_Campaign_Program.Status = statusdecline;
                            db.Entry(objPlan_Campaign_Program).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    imodel.ProgramTitle = objPlan_Campaign_Program.Title;
                    imodel.CampaignTitle = objPlan_Campaign_Program.Plan_Campaign.Title;
                    imodel.Status = objPlan_Campaign_Program.Status;
                    imodel.ColorCode = Program_InspectPopup_Flag_Color;
                    imodel.Description = objPlan_Campaign_Program.Description;
                    imodel.PlanCampaignId = objPlan_Campaign_Program.PlanCampaignId;
                    imodel.PlanProgramId = objPlan_Campaign_Program.PlanProgramId;
                    imodel.OwnerId = objPlan_Campaign_Program.CreatedBy;
                    imodel.Cost = Common.CalculateProgramCost(objPlan_Campaign_Program.PlanProgramId); //objPlan_Campaign_Program.Cost; // Modified for PL#440 by Dharmraj
                    imodel.StartDate = objPlan_Campaign_Program.StartDate;
                    imodel.EndDate = objPlan_Campaign_Program.EndDate;

                    imodel.IsDeployedToIntegration = objPlan_Campaign_Program.IsDeployedToIntegration;
                    imodel.LastSyncDate = objPlan_Campaign_Program.LastSyncDate;

                    imodel.IsIntegrationInstanceExist = CheckIntegrationInstanceExist(objPlan_Campaign_Program.Plan_Campaign.Plan.Model);
                }
                else if (section == Convert.ToString(Enums.Section.Campaign).ToLower()) //// Get Inspect Model for Campaign InspectPopup.
                {

                    var objPlan_Campaign = db.Plan_Campaign.Where(pcp => pcp.PlanCampaignId == id && pcp.IsDeleted == false).FirstOrDefault();

                    if (isStatusChange)     //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                    {
                        var planCampaignProgramObj = db.Plan_Campaign_Program.Where(pcpt => pcpt.PlanCampaignId == id && pcpt.IsDeleted == false).ToList();
                        // Number of program with status is not 'Submitted' 
                        int cntSumbitProgramStatus = planCampaignProgramObj.Where(pcpt => !pcpt.Status.Equals(statussubmit)).Count();
                        // Number of tactic with status is not 'Submitted'
                        int cntSumbitTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.PlanCampaignId == id && pcpt.IsDeleted == false && !pcpt.Status.Equals(statussubmit)).Count();

                        // Number of program with status is not 'Approved', 'in-progress', 'complete'
                        int cntApproveProgramStatus = planCampaignProgramObj.Where(pcpt => (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();
                        // Number of tactic with status is not 'Approved', 'in-progress', 'complete'
                        int cntApproveTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.PlanCampaignId == id && pcpt.IsDeleted == false && (!pcpt.Status.Equals(statusapproved) && !pcpt.Status.Equals(statusinprogress) && !pcpt.Status.Equals(statuscomplete))).Count();

                        // Number of program with status is not 'Declained'
                        int cntDeclineProgramStatus = planCampaignProgramObj.Where(pcpt => !pcpt.Status.Equals(statusdecline)).Count();
                        // Number of tactic with status is not 'Declained'
                        int cntDeclineTacticStatus = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.Plan_Campaign_Program.PlanCampaignId == id && pcpt.IsDeleted == false && !pcpt.Status.Equals(statusdecline)).Count();


                        if (cntSumbitProgramStatus == 0 && cntSumbitTacticStatus == 0)
                        {
                            objPlan_Campaign.Status = statussubmit;
                            db.Entry(objPlan_Campaign).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (cntApproveProgramStatus == 0 && cntApproveTacticStatus == 0)
                        {
                            objPlan_Campaign.Status = statusapproved;
                            db.Entry(objPlan_Campaign).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (cntDeclineProgramStatus == 0 && cntDeclineTacticStatus == 0)
                        {
                            objPlan_Campaign.Status = statusdecline;
                            db.Entry(objPlan_Campaign).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    imodel.CampaignTitle = objPlan_Campaign.Title;
                    imodel.Status = objPlan_Campaign.Status;
                    imodel.ColorCode = Campaign_InspectPopup_Flag_Color;
                    imodel.Description = objPlan_Campaign.Description;
                    imodel.PlanCampaignId = objPlan_Campaign.PlanCampaignId;
                    imodel.OwnerId = objPlan_Campaign.CreatedBy;
                    imodel.Cost = Common.CalculateCampaignCost(objPlan_Campaign.PlanCampaignId); //objPlan_Campaign.Cost; // Modified for PL#440 by Dharmraj
                    imodel.StartDate = objPlan_Campaign.StartDate;
                    imodel.EndDate = objPlan_Campaign.EndDate;

                    imodel.IsDeployedToIntegration = objPlan_Campaign.IsDeployedToIntegration;
                    imodel.IsIntegrationInstanceExist = CheckIntegrationInstanceExist(objPlan_Campaign.Plan.Model);
                    imodel.LastSyncDate = objPlan_Campaign.LastSyncDate;

                }
                else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())    //// Get Inspect Model for ImprovementTactic InspectPopup.
                {
                    imodel = (from pcpt in db.Plan_Improvement_Campaign_Program_Tactic
                              where pcpt.ImprovementPlanTacticId == id && pcpt.IsDeleted == false
                              select new InspectModel
                              {
                                  PlanTacticId = pcpt.ImprovementPlanTacticId,
                                  TacticTitle = pcpt.Title,
                                  TacticTypeTitle = pcpt.ImprovementTacticType.Title,
                                  CampaignTitle = pcpt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Title,
                                  ProgramTitle = pcpt.Plan_Improvement_Campaign_Program.Title,
                                  Status = pcpt.Status,
                                  TacticTypeId = pcpt.ImprovementTacticTypeId,
                                  ColorCode = pcpt.ImprovementTacticType.ColorCode,
                                  Description = pcpt.Description,
                                  PlanCampaignId = pcpt.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
                                  PlanProgramId = pcpt.ImprovementPlanProgramId,
                                  OwnerId = pcpt.CreatedBy,
                                  Cost = pcpt.Cost,
                                  StartDate = pcpt.EffectiveDate,
                                  IsDeployedToIntegration = pcpt.IsDeployedToIntegration,
                                  LastSyncDate = pcpt.LastSyncDate,
                                  ImprovementPlanProgramId = pcpt.ImprovementPlanProgramId,
                                  ImprovementPlanTacticId = pcpt.ImprovementPlanTacticId,
                                  ImprovementTacticTypeId = pcpt.ImprovementTacticTypeId,
                                  EffectiveDate = pcpt.EffectiveDate,
                                  Title = pcpt.Title
                              }).FirstOrDefault();

                    imodel.IsIntegrationInstanceExist = CheckIntegrationInstanceExist(db.Plan_Improvement_Campaign_Program_Tactic.FirstOrDefault(varT => varT.ImprovementPlanTacticId == id).Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model);

                }
                // Start - Added by Sohel Pathan on 07/11/2014 for PL ticket #811
                else if (section.Equals(Enums.Section.Plan.ToString(), StringComparison.OrdinalIgnoreCase)) //// Get Inspect Model for Plan InspectPopup.
                {
                    var objPlan = (from p in db.Plans
                                   where p.PlanId == id && p.IsDeleted == false
                                   select p).FirstOrDefault();

                    imodel.PlanId = objPlan.PlanId;
                    imodel.ColorCode = Plan_InspectPopup_Flag_Color;
                    imodel.Description = objPlan.Description;
                    imodel.OwnerId = objPlan.CreatedBy;
                    imodel.Title = objPlan.Title;
                    imodel.ModelId = objPlan.ModelId;
                    imodel.ModelTitle = objPlan.Model.Title + " " + objPlan.Model.Version;
                    imodel.GoalType = objPlan.GoalType;
                    imodel.GoalValue = objPlan.GoalValue.ToString();
                    imodel.Budget = objPlan.Budget;
                    imodel.AllocatedBy = objPlan.AllocatedBy;
                }
                // End - Added by Sohel Pathan on 07/11/2014 for PL ticket #811

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return imodel;
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Comment in Review Tab.
        /// </summary>
        /// <param name="comment">Comment.</param>
        /// <param name="planTacticId">Plan Tactic Id.</param>
        /// <param name="section">Decide for which section (tactic,program or campaign comment will be added)</param>
        /// <returns>Returns Partial View Of Inspect Popup.</returns>
        [HttpPost]
        public JsonResult SaveComment(string comment, int planTacticId, string section)
        {
            int result = 0;

            try
            {
                ////Added by Mitesh Vaishnav on 07/07/2014 for PL ticket #569: make urls in tactic notes hyperlinks
                string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,_\[\]\(\)\!\$\*\|##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";   // Modified by Viral Kadiya on PL ticket to resolve issue #794.
                Regex r = new Regex(regex, RegexOptions.IgnoreCase);
                comment = r.Replace(comment, "<a href=\"$1\" title=\"Click to open in a new window or tab\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"http://www");



                ////End Added by Mitesh Vaishnav on 07/07/2014 for PL ticket #569: make urls in tactic notes hyperlinks
                if (ModelState.IsValid)
                {
                    if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                    {
                        //// save comment for ImprovementTactic section.
                        Plan_Improvement_Campaign_Program_Tactic_Comment pcpitc = new Plan_Improvement_Campaign_Program_Tactic_Comment();
                        pcpitc.ImprovementPlanTacticId = planTacticId;
                        pcpitc.Comment = comment;
                        DateTime currentdate = DateTime.Now;
                        pcpitc.CreatedDate = currentdate;
                        string displayDate = currentdate.ToString("MMM dd") + " at " + currentdate.ToString("hh:mmtt");
                        pcpitc.CreatedBy = Sessions.User.UserId;
                        db.Entry(pcpitc).State = EntityState.Added;
                        db.Plan_Improvement_Campaign_Program_Tactic_Comment.Add(pcpitc);
                        result = db.SaveChanges();
                    }
                    else
                    {
                        //// save comment for Tactic,Program,Campaign section.
                        Plan_Campaign_Program_Tactic_Comment pcptc = new Plan_Campaign_Program_Tactic_Comment();
                        if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                        {
                            pcptc.PlanTacticId = planTacticId;
                        }
                        else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                        {
                            pcptc.PlanProgramId = planTacticId;
                        }
                        else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                        {
                            pcptc.PlanCampaignId = planTacticId;
                        }
                        pcptc.Comment = comment;
                        DateTime currentdate = DateTime.Now;
                        pcptc.CreatedDate = currentdate;
                        string displayDate = currentdate.ToString("MMM dd") + " at " + currentdate.ToString("hh:mmtt");
                        pcptc.CreatedBy = Sessions.User.UserId;
                        db.Entry(pcptc).State = EntityState.Added;
                        db.Plan_Campaign_Program_Tactic_Comment.Add(pcptc);
                        result = db.SaveChanges();
                    }

                    if (result >= 1)
                    {
                        //// Send Comment Addedd Email to Users.
                        if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                        {
                            int PlanId = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == planTacticId).Select(_tactic => _tactic.Plan_Campaign_Program.Plan_Campaign.PlanId).FirstOrDefault();
                            Plan_Campaign_Program_Tactic pct = db.Plan_Campaign_Program_Tactic.Where(_tactic => _tactic.PlanTacticId == planTacticId).FirstOrDefault();
                            string strUrl = GetNotificationURLbyStatus(PlanId, planTacticId, section);
                            Common.mailSendForTactic(planTacticId, Enums.Custom_Notification.TacticCommentAdded.ToString(), pct.Title, true, comment, Convert.ToString(Enums.Section.Tactic).ToLower(), strUrl);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                        }
                        else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                        {
                            Plan_Campaign_Program pcp = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == planTacticId).FirstOrDefault();
                            int PlanId = db.Plan_Campaign_Program.Where(pcpt => pcpt.PlanProgramId == planTacticId).Select(program => program.Plan_Campaign.PlanId).FirstOrDefault();
                            string strUrl = GetNotificationURLbyStatus(PlanId, planTacticId, section);
                            Common.mailSendForTactic(planTacticId, Enums.Custom_Notification.ProgramCommentAdded.ToString(), pcp.Title, true, comment, Convert.ToString(Enums.Section.Program).ToLower(), strUrl);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                        }
                        else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                        {
                            Plan_Campaign pc = db.Plan_Campaign.Where(campaign => campaign.PlanCampaignId == planTacticId).FirstOrDefault();
                            string strUrl = GetNotificationURLbyStatus(pc.PlanId, planTacticId, section);
                            Common.mailSendForTactic(planTacticId, Enums.Custom_Notification.CampaignCommentAdded.ToString(), pc.Title, true, comment, Convert.ToString(Enums.Section.Campaign).ToLower(), strUrl);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                        }
                        else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                        {
                            Plan_Improvement_Campaign_Program_Tactic pc = db.Plan_Improvement_Campaign_Program_Tactic.Where(imprvTactic => imprvTactic.ImprovementPlanTacticId == planTacticId).FirstOrDefault();
                            string strUrl = GetNotificationURLbyStatus(pc.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, planTacticId, section);
                            Common.mailSendForTactic(planTacticId, Enums.Custom_Notification.ImprovementTacticCommentAdded.ToString(), pc.Title, true, comment, Convert.ToString(Enums.Section.ImprovementTactic).ToLower(), strUrl);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                        }
                    }
                    return Json(new { id = planTacticId, TabValue = "Review", msg = Common.objCached.EmptyFieldCommentAdded });      // Modified by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
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
            return Json(new { id = 0 });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Comment & Update Status as Per Selected Value.
        /// </summary>
        /// <param name="planTacticId">Plan Tactic Id.</param>
        /// <param name="status">status.</param>
        /// <param name="section">Decide for wich saction (tactic,program or campaign) status will be updated)</param>
        /// <returns>Returns Partial View Of Inspect Popup.</returns>
        [HttpPost]
        public JsonResult ApprovedTactic(int planTacticId, string status, string section)
        {
            int result = 0;
            string approvedComment = "";
            string strmessage = "";
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        if (ModelState.IsValid)
                        {
                            if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                            {
                                //// Save Comment for Improvement Tactic.
                                Plan_Improvement_Campaign_Program_Tactic_Comment pcptc = new Plan_Improvement_Campaign_Program_Tactic_Comment();
                                approvedComment = Convert.ToString(Enums.Section.ImprovementTactic) + " " + status + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                                pcptc.ImprovementPlanTacticId = planTacticId;
                                pcptc.Comment = approvedComment;
                                DateTime currentdate = DateTime.Now;
                                pcptc.CreatedDate = currentdate;
                                string displayDate = currentdate.ToString("MMM dd") + " at " + currentdate.ToString("hh:mmtt");
                                pcptc.CreatedBy = Sessions.User.UserId;
                                db.Entry(pcptc).State = EntityState.Added;
                                db.Plan_Improvement_Campaign_Program_Tactic_Comment.Add(pcptc);
                                result = db.SaveChanges();
                            }
                            else
                            {
                                //// Save Comment for Tactic,Program,Campaign.
                                Plan_Campaign_Program_Tactic_Comment pcptc = new Plan_Campaign_Program_Tactic_Comment();
                                if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                                {
                                    pcptc.PlanTacticId = planTacticId;
                                    approvedComment = Convert.ToString(Enums.Section.Tactic) + " " + status + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                                }
                                else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                                {
                                    pcptc.PlanProgramId = planTacticId;
                                    approvedComment = Convert.ToString(Enums.Section.Program) + " " + status + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                                }
                                else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                                {
                                    pcptc.PlanCampaignId = planTacticId;
                                    approvedComment = Convert.ToString(Enums.Section.Campaign) + " " + status + " by " + Sessions.User.FirstName + " " + Sessions.User.LastName;
                                }
                                pcptc.Comment = approvedComment;
                                DateTime currentdate = DateTime.Now;
                                pcptc.CreatedDate = currentdate;
                                string displayDate = currentdate.ToString("MMM dd") + " at " + currentdate.ToString("hh:mmtt");
                                pcptc.CreatedBy = Sessions.User.UserId;
                                db.Entry(pcptc).State = EntityState.Added;
                                db.Plan_Campaign_Program_Tactic_Comment.Add(pcptc);
                                result = db.SaveChanges();
                            }
                            if (result == 1)
                            {
                                //// Save Status for all section.
                                if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                                {
                                    Plan_Campaign_Program_Tactic tactic = db.Plan_Campaign_Program_Tactic.Where(pt => pt.PlanTacticId == planTacticId).FirstOrDefault();
                                    bool isApproved = false;
                                    DateTime todaydate = DateTime.Now;
                                    if (status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                                    {
                                        tactic.Status = status;
                                        isApproved = true;
                                        if (todaydate > tactic.StartDate && todaydate < tactic.EndDate)
                                        {
                                            tactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString();
                                        }
                                        else if (todaydate > tactic.EndDate)
                                        {
                                            tactic.Status = Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString();
                                        }
                                    }
                                    else
                                    {
                                        tactic.Status = status;
                                    }
                                    tactic.ModifiedBy = Sessions.User.UserId;
                                    tactic.ModifiedDate = DateTime.Now;
                                    db.Entry(tactic).State = EntityState.Modified;
                                    result = db.SaveChanges();
                                    if (result == 1)
                                    {
                                        if (isApproved)
                                        {
                                            result = Common.InsertChangeLog(Sessions.PlanId, null, planTacticId, tactic.Title.ToString(), Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved, null);
                                            //added by uday for #532 
                                            if (tactic.IsDeployedToIntegration == true)
                                            {
                                                ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, new Guid(), EntityType.Tactic);
                                                externalIntegration.Sync();
                                            }
                                            //end
                                        }
                                        else if (tactic.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                        {
                                            result = Common.InsertChangeLog(Sessions.PlanId, null, planTacticId, tactic.Title.ToString(), Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined, null);
                                        }
                                    }
                                    if (result >= 1)
                                    {
                                        //// Send Notification Url for Tactic.
                                        string strURL = GetNotificationURLbyStatus(Sessions.PlanId, planTacticId, section);//Url.Action("Index", "Home", new { currentPlanId = Sessions.PlanId, planTacticId = planTacticId, activeMenu = "Plan" }, Request.Url.Scheme);
                                        Common.mailSendForTactic(planTacticId, status, tactic.Title, false, "", Convert.ToString(Enums.Section.Tactic).ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                    }
                                    strmessage = Common.objCached.TacticStatusSuccessfully.Replace("{0}", status);

                                    // Start - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    string strStatusMessage = Common.GetStatusMessage(status);     // if status is Approved,SubmitforApproval or Rejected then derive status message by this function.
                                    if (!string.IsNullOrEmpty(strStatusMessage))
                                    {
                                        strmessage = strStatusMessage;
                                        strmessage = string.Format(strmessage, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);
                                    }
                                    // End - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.

                                    //// Start - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                    //-- Update Program status according to the tactic status
                                    Common.ChangeProgramStatus(tactic.PlanProgramId);

                                    //-- Update Campaign status according to the tactic and program status
                                    var PlanCampaignId = tactic.Plan_Campaign_Program.PlanCampaignId;
                                    Common.ChangeCampaignStatus(PlanCampaignId);
                                    //// End - Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                }
                                else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                                {
                                    Plan_Improvement_Campaign_Program_Tactic tactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(pt => pt.ImprovementPlanTacticId == planTacticId).FirstOrDefault();
                                    tactic.Status = status;
                                    tactic.ModifiedBy = Sessions.User.UserId;
                                    tactic.ModifiedDate = DateTime.Now;
                                    db.Entry(tactic).State = EntityState.Modified;
                                    result = db.SaveChanges();
                                    if (result == 1)
                                    {
                                        if (tactic.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                                        {
                                            result = Common.InsertChangeLog(Sessions.PlanId, null, planTacticId, tactic.Title.ToString(), Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved, null);
                                            //added by uday for #532
                                            if (tactic.IsDeployedToIntegration == true)
                                            {
                                                ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, new Guid(), EntityType.ImprovementTactic);
                                                externalIntegration.Sync();
                                            }
                                            //end by uday for #532
                                        }
                                        else if (tactic.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                        {
                                            result = Common.InsertChangeLog(Sessions.PlanId, null, planTacticId, tactic.Title.ToString(), Enums.ChangeLog_ComponentType.improvetactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined, null);
                                        }
                                    }
                                    if (result >= 1)
                                    {
                                        string strURL = GetNotificationURLbyStatus(Sessions.PlanId, planTacticId, section);
                                        Common.mailSendForTactic(planTacticId, status, tactic.Title, false, "", Convert.ToString(Enums.Section.ImprovementTactic).ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                    }
                                    strmessage = Common.objCached.ImprovementTacticStatusSuccessfully.Replace("{0}", status);
                                    // Start - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    string strStatusMessage = Common.GetStatusMessage(status);     // if status is Approved,SubmitforApproval or Rejected then derive status message by this function.
                                    if (!string.IsNullOrEmpty(strStatusMessage))
                                    {
                                        strmessage = strStatusMessage;
                                        strmessage = string.Format(strmessage, Enums.PlanEntityValues[Enums.PlanEntity.ImprovementTactic.ToString()]);
                                    }
                                    // End - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                }
                                else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                                {
                                    Plan_Campaign_Program program = db.Plan_Campaign_Program.Where(pt => pt.PlanProgramId == planTacticId).FirstOrDefault();
                                    program.Status = status;
                                    program.ModifiedBy = Sessions.User.UserId;
                                    program.ModifiedDate = DateTime.Now;
                                    db.Entry(program).State = EntityState.Modified;
                                    result = db.SaveChanges();
                                    if (result == 1)
                                    {
                                        if (program.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                                        {
                                            string strstatus = Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString();
                                            db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == planTacticId).ToList().ForEach(pcpt => pcpt.Status = strstatus);
                                            db.SaveChanges();
                                            result = Common.InsertChangeLog(Sessions.PlanId, null, planTacticId, program.Title.ToString(), Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved, null);
                                        }
                                        else if (program.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString()))
                                        {
                                            string strstatus = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                            db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == planTacticId).ToList().ForEach(pcpt => pcpt.Status = strstatus);
                                            db.SaveChanges();

                                        }
                                        else if (program.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                        {

                                            string strstatus = Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString();
                                            db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanProgramId == planTacticId).ToList().ForEach(pcpt => pcpt.Status = strstatus);
                                            db.SaveChanges();
                                            Common.InsertChangeLog(program.Plan_Campaign.PlanId, 0, program.PlanProgramId, program.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined);

                                        }

                                        //// Added by :- Sohel Pathan on 27/05/2014 for PL ticket #425
                                        //-- Update Campaign status according to the tactic and program status
                                        Common.ChangeCampaignStatus(program.PlanCampaignId);
                                    }
                                    if (result >= 1)
                                    {
                                        string strURL = GetNotificationURLbyStatus(Sessions.PlanId, planTacticId, section);
                                        Common.mailSendForTactic(planTacticId, status, program.Title, false, "", Convert.ToString(Enums.Section.Program).ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                    }
                                    strmessage = Common.objCached.ProgramStatusSuccessfully.Replace("{0}", status);
                                    // Start - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    string strStatusMessage = Common.GetStatusMessage(status);     // if status is Approved,SubmitforApproval or Rejected then derive status message by this function.
                                    if (!string.IsNullOrEmpty(strStatusMessage))
                                    {
                                        strmessage = strStatusMessage;
                                        strmessage = string.Format(strmessage, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);
                                    }
                                    // End - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                }
                                else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                                {
                                    Plan_Campaign campaign = db.Plan_Campaign.Where(pt => pt.PlanCampaignId == planTacticId).FirstOrDefault();
                                    campaign.Status = status;
                                    campaign.ModifiedBy = Sessions.User.UserId;
                                    campaign.ModifiedDate = DateTime.Now;
                                    db.Entry(campaign).State = EntityState.Modified;
                                    result = db.SaveChanges();
                                    if (result == 1)
                                    {
                                        if (campaign.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString()))
                                        {
                                            string strstatus = Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString();
                                            db.Plan_Campaign_Program.Where(pcp => pcp.PlanCampaignId == planTacticId).ToList().ForEach(pcp => pcp.Status = strstatus);
                                            db.Plan_Campaign_Program_Tactic.Where(pcp => pcp.Plan_Campaign_Program.PlanCampaignId == planTacticId).ToList().ForEach(pcpt => pcpt.Status = strstatus);
                                            db.SaveChanges();

                                            result = Common.InsertChangeLog(Sessions.PlanId, null, planTacticId, campaign.Title.ToString(), Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.approved, null);
                                        }
                                        else if (campaign.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString()))
                                        {
                                            string strstatus = Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString();
                                            db.Plan_Campaign_Program.Where(pcp => pcp.PlanCampaignId == planTacticId).ToList().ForEach(pcp => pcp.Status = strstatus);
                                            db.Plan_Campaign_Program_Tactic.Where(pcp => pcp.Plan_Campaign_Program.PlanCampaignId == planTacticId).ToList().ForEach(pcpt => pcpt.Status = strstatus);
                                            db.SaveChanges();

                                        }
                                        else if (campaign.Status.Equals(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString()))
                                        {
                                            string strstatus = Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString();
                                            db.Plan_Campaign_Program.Where(pcp => pcp.PlanCampaignId == planTacticId).ToList().ForEach(pcp => pcp.Status = strstatus);
                                            db.Plan_Campaign_Program_Tactic.Where(pcp => pcp.Plan_Campaign_Program.PlanCampaignId == planTacticId).ToList().ForEach(pcpt => pcpt.Status = strstatus);
                                            db.SaveChanges();
                                            Common.InsertChangeLog(campaign.PlanId, 0, campaign.PlanCampaignId, campaign.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.declined);

                                        }
                                    }
                                    if (result >= 1)
                                    {
                                        string strURL = GetNotificationURLbyStatus(Sessions.PlanId, planTacticId, section);
                                        Common.mailSendForTactic(planTacticId, status, campaign.Title, false, "", Convert.ToString(Enums.Section.Campaign).ToLower(), strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                                    }
                                    strmessage = Common.objCached.CampaignStatusSuccessfully.Replace("{0}", status);
                                    // Start - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                    string strStatusMessage = Common.GetStatusMessage(status);     // if status is Approved,SubmitforApproval or Rejected then derive status message by this function.
                                    if (!string.IsNullOrEmpty(strStatusMessage))
                                    {
                                        strmessage = strStatusMessage;
                                        strmessage = string.Format(strmessage, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);
                                    }
                                    // End - // Added by Viral Kadiya on 17/11/2014 to resolve isssue for PL ticket #947.
                                }
                            }
                            scope.Complete();
                            return Json(new { id = planTacticId, TabValue = "Review", msg = strmessage });
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

            return Json(new { id = 0 });
        }

        /// <summary>
        /// Get Current Date Based on Plan Year.
        /// </summary>
        /// <param name="isEndDate"></param>
        /// <returns></returns>
        private DateTime GetCurrentDateBasedOnPlan(bool isEndDate = false)
        {
            string Year = db.Plans.Where(plan => plan.PlanId == Sessions.PlanId).Select(plan => plan.Year).FirstOrDefault();
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

        /// <summary>
        /// Added By: Pratik Chauhan.
        /// Action to open resubmission popup.
        /// </summary>
        /// <param name="redirectionType">From where it open.</param>
        /// <param name="labelValues">Changed control label value(s).</param>
        /// <returns>Returns Partial View Of resubmission popup.</returns>
        public PartialViewResult LoadResubmission(string redirectionType, string labelValues)
        {
            var customFields = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(labelValues);

            //// Create list for changed control label(s).
            List<string> listLabelValue = new List<string>();

            if (customFields.Count != 0)
            {
                foreach (var item in customFields)
                {
                    listLabelValue.Add(item.Value.Replace("_", " "));
                }
            }

            ViewBag.RedirectionType = redirectionType;
            ViewBag.resubmissionValues = listLabelValue;

            return PartialView("_ResubmissionPopup");
        }

        #region "Share Tactic"
        /// <summary>
        /// Share Tactic,Campaign or Program based on the section passed.
        /// </summary>
        /// <param name="planTacticId"></param>
        /// <param name="toEmailIds"></param>
        /// <param name="optionalMessage"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public JsonResult ShareTactic(int planTacticId, string toEmailIds, string optionalMessage, string section)
        {
            int result = 0;
            string notificationShare = "";
            string emailBody = "";
            Notification notification = new Notification();
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        if (ModelState.IsValid)
                        {
                            //// Added by Sohel on 2nd April for PL#398 to decode the optionalMessage text
                            optionalMessage = HttpUtility.UrlDecode(optionalMessage, System.Text.Encoding.Default);

                            string strURL = string.Empty;
                            Plan plan = new Plan();
                            if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                            {
                                plan = db.Plan_Campaign_Program_Tactic.Single(_tactic => _tactic.PlanTacticId.Equals(planTacticId)).Plan_Campaign_Program.Plan_Campaign.Plan;
                                notificationShare = Enums.Custom_Notification.ShareTactic.ToString();
                                notification = (Notification)db.Notifications.Single(notifictn => notifictn.NotificationInternalUseOnly.Equals(notificationShare));
                                strURL = GetNotificationURLbyStatus(plan.PlanId, planTacticId, section);
                                emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage).Replace("[URL]", strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.

                            }
                            else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                            {
                                plan = db.Plan_Campaign_Program.Single(_program => _program.PlanProgramId.Equals(planTacticId)).Plan_Campaign.Plan;
                                notificationShare = Enums.Custom_Notification.ShareProgram.ToString();
                                notification = (Notification)db.Notifications.Single(notifictn => notifictn.NotificationInternalUseOnly.Equals(notificationShare));
                                strURL = GetNotificationURLbyStatus(plan.PlanId, planTacticId, section);
                                emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage).Replace("[URL]", strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                            }
                            else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                            {
                                plan = db.Plan_Campaign.Single(pt => pt.PlanCampaignId.Equals(planTacticId)).Plan;
                                notificationShare = Enums.Custom_Notification.ShareCampaign.ToString();
                                notification = (Notification)db.Notifications.Single(notifictn => notifictn.NotificationInternalUseOnly.Equals(notificationShare));
                                strURL = GetNotificationURLbyStatus(plan.PlanId, planTacticId, section);
                                emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage).Replace("[URL]", strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                            }
                            else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                            {
                                plan = db.Plan_Improvement_Campaign_Program_Tactic.Single(_tactic => _tactic.ImprovementPlanTacticId.Equals(planTacticId)).Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan;
                                notificationShare = Enums.Custom_Notification.ShareImprovementTactic.ToString();
                                notification = (Notification)db.Notifications.Single(notifictn => notifictn.NotificationInternalUseOnly.Equals(notificationShare));
                                strURL = GetNotificationURLbyStatus(plan.PlanId, planTacticId, section);
                                emailBody = notification.EmailContent.Replace("[AdditionalMessage]", optionalMessage).Replace("[URL]", strURL);// Modified by viral kadiya on 12/4/2014 to resolve PL ticket #978.
                            }

                            //// Send Share Notification Email to ToEmailIds list.
                            foreach (string toEmail in toEmailIds.Split(','))
                            {
                                if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                                {
                                    Plan_Improvement_Campaign_Program_Tactic_Share improvementTacticShare = new Plan_Improvement_Campaign_Program_Tactic_Share();
                                    improvementTacticShare.ImprovementPlanTacticId = planTacticId;
                                    improvementTacticShare.EmailId = toEmail;
                                    //// Modified by Sohel on 3rd April for PL#398 to encode the email body while inserting into DB.
                                    improvementTacticShare.EmailBody = HttpUtility.HtmlEncode(emailBody);
                                    ////
                                    improvementTacticShare.CreatedDate = DateTime.Now;
                                    improvementTacticShare.CreatedBy = Sessions.User.UserId;
                                    db.Entry(improvementTacticShare).State = EntityState.Added;
                                    db.Plan_Improvement_Campaign_Program_Tactic_Share.Add(improvementTacticShare);
                                    result = db.SaveChanges();
                                }
                                else
                                {
                                    Tactic_Share tacticShare = new Tactic_Share();
                                    if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
                                    {
                                        tacticShare.PlanTacticId = planTacticId;
                                    }
                                    else if (section == Convert.ToString(Enums.Section.Program).ToLower())
                                    {
                                        tacticShare.PlanProgramId = planTacticId;
                                    }
                                    else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
                                    {
                                        tacticShare.PlanCampaignId = planTacticId;
                                    }
                                    tacticShare.EmailId = toEmail;
                                    //// Modified by Sohel on 3rd April for PL#398 to encode the email body while inserting into DB.
                                    tacticShare.EmailBody = HttpUtility.HtmlEncode(emailBody);
                                    ////
                                    tacticShare.CreatedDate = DateTime.Now;
                                    tacticShare.CreatedBy = Sessions.User.UserId;
                                    db.Entry(tacticShare).State = EntityState.Added;
                                    db.Tactic_Share.Add(tacticShare);
                                    result = db.SaveChanges();
                                }

                                if (result == 1)
                                {
                                    Common.sendMail(toEmail, Common.FromMail, emailBody, notification.Subject, string.Empty);
                                }
                            }

                            scope.Complete();
                            return Json(true, JsonRequestBehavior.AllowGet);
                        }
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
        /// Show share Tactic,Program or Campaign based on the passed section
        /// </summary>
        /// <param name="planTacticId"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public ActionResult ShowShareTactic(int planTacticId, string section)
        {
            if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
            {
                Plan_Campaign_Program_Tactic planTactic = db.Plan_Campaign_Program_Tactic.Single(_tactic => _tactic.PlanTacticId.Equals(planTacticId));
                ViewBag.PlanTacticId = planTacticId;
                ViewBag.TacticTitle = planTactic.Title;

                //// Modified By Maninder Singh Wadhva PL Ticket#47
                ViewBag.IsImprovement = false;
            }

            else if (section == Convert.ToString(Enums.Section.Program).ToLower())
            {
                Plan_Campaign_Program planProgram = db.Plan_Campaign_Program.Single(_program => _program.PlanProgramId.Equals(planTacticId));
                ViewBag.PlanProgramId = planTacticId;
                ViewBag.ProgramTitle = planProgram.Title;
            }

            else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
            {
                Plan_Campaign planCampaign = db.Plan_Campaign.Single(_campaign => _campaign.PlanCampaignId.Equals(planTacticId));
                ViewBag.PlanCampaignId = planTacticId;
                ViewBag.CampaignTitle = planCampaign.Title;
            }
            else if (section == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
            {
                Plan_Improvement_Campaign_Program_Tactic improvementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Single(_tactic => _tactic.ImprovementPlanTacticId.Equals(planTacticId));
                ViewBag.PlanTacticId = planTacticId;
                ViewBag.TacticTitle = improvementTactic.Title;

                //// Modified By Maninder Singh Wadhva PL Ticket#47
                ViewBag.IsImprovement = true;
            }

            try
            {
                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                ViewBag.IsServiceUnavailable = false;

                BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();
                var individuals = bdsUserRepository.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, true);
                if (individuals.Count != 0)
                {
                    ViewBag.EmailIds = individuals.Select(member => member.Email).ToList<string>();
                }
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
                    ViewBag.IsServiceUnavailable = true;
                }
            }

            if (section == Convert.ToString(Enums.Section.Tactic).ToLower())
            {
                return PartialView("ShareTactic");
            }
            else if (section == Convert.ToString(Enums.Section.Program).ToLower())
            {
                return PartialView("_ShareProgram");
            }
            else if (section == Convert.ToString(Enums.Section.Campaign).ToLower())
            {
                return PartialView("_ShareCampaign");
            }

            return PartialView("ShareTactic");
        }
        #endregion

        /// <summary>
        /// Added : By Kalpesh Sharma Ticket #648 Cloning icon for tactics with allocation
        /// </summary>
        /// <param name="CloneType"></param>
        /// <param name="Id"></param>
        /// <param name="title"></param>
        /// <param name="CalledFromBudget"></param>
        /// <param name="RequsetedModule"></param>
        /// <param name="planid"></param>
        /// <returns></returns>
        public ActionResult Clone(string CloneType, int Id, string title, string CalledFromBudget = "", string RequsetedModule = "", int planid = 0)
        {
            int rtResult = 0;
            bool IsCampaign = (CloneType == Enums.Section.Campaign.ToString()) ? true : false;
            bool IsProgram = (CloneType == Enums.Section.Program.ToString()) ? true : false; ;
            bool IsTactic = (CloneType == Enums.Section.Tactic.ToString()) ? true : false; ;

            if (IsCampaign)
            {
                planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == Id && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
            }
            else if (IsProgram)
            {
                planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == (db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == Id && pcp.IsDeleted.Equals(false)).Select(pcp => pcp.PlanCampaignId).FirstOrDefault()) && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
            }
            else if (IsTactic)
            {
                planid = db.Plan_Campaign.Where(pc => pc.PlanCampaignId == (db.Plan_Campaign_Program.Where(pcp => pcp.PlanProgramId == (db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.PlanTacticId == Id && pcpt.IsDeleted.Equals(false)).Select(pcpt => pcpt.PlanProgramId).FirstOrDefault()) && pcp.IsDeleted.Equals(false)).Select(pcp => pcp.PlanCampaignId).FirstOrDefault()) && pc.IsDeleted.Equals(false)).Select(pc => pc.PlanId).FirstOrDefault();
            }

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

                    //// Create Clone by CloneType e.g Plan,Campaign,Program,Tactic,LineItem
                    rtResult = objClonehelper.ToClone("", CloneType, Id, planid);
                    if (CloneType == Enums.DuplicationModule.Plan.ToString())
                    {
                        Plan objPlan = db.Plans.Where(p => p.PlanId == Id).FirstOrDefault();
                        title = objPlan != null ? objPlan.Title : string.Empty;
                    }
                    if (CloneType == Enums.DuplicationModule.LineItem.ToString())
                    {
                        CloneType = "Line Item";
                    }

                }

                if (rtResult >= 1)
                {
                    title = HttpUtility.HtmlDecode(title);
                    string strMessage = string.Format(Common.objCached.CloneDuplicated, CloneType);     // Modified by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.

                    if (!string.IsNullOrEmpty(CalledFromBudget))
                    {
                        TempData["SuccessMessage"] = strMessage;
                        TempData["SuccessMessageDeletedPlan"] = "";

                        string expand = CloneType.ToLower().Replace(" ", "");
                        if (expand == "campaign")
                            return Json(new { IsSuccess = true, type = CalledFromBudget, Id = rtResult, msg = strMessage });
                        else
                            return Json(new { IsSuccess = true, type = CalledFromBudget, Id = rtResult, expand = expand + rtResult.ToString(), msg = strMessage });
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(RequsetedModule) && RequsetedModule == Enums.InspectPopupRequestedModules.Index.ToString())
                        {
                            return Json(new { IsSuccess = true, redirect = Url.Action("Index"), msg = strMessage, opt = Enums.InspectPopupRequestedModules.Index.ToString(), Id = rtResult });
                        }
                        else if (!string.IsNullOrEmpty(RequsetedModule) && RequsetedModule == Enums.InspectPopupRequestedModules.ApplyToCalendar.ToString())
                        {
                            TempData["SuccessMessageDeletedPlan"] = strMessage;
                            return Json(new { IsSuccess = true, msg = strMessage, redirect = Url.Action("ApplyToCalendar", "Plan"), Id = rtResult });
                        }
                        else
                        {
                            TempData["SuccessMessageDeletedPlan"] = strMessage;
                            return Json(new { IsSuccess = true, Id = rtResult, redirect = Url.Action("Assortment", "Plan"), planId = Sessions.PlanId, opt = Enums.InspectPopupRequestedModules.Assortment.ToString(), msg = strMessage });
                        }
                    }
                }
                else
                {
                    string strErrorMessage = string.Format("{0} not duplicated.", CloneType);    // Modified by Viral Kadiya on 11/18/2014 to resolve PL ticket #947.
                    return Json(new { IsSuccess = false, msg = strErrorMessage, opt = RequsetedModule });

                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                return Json(new { IsSuccess = false, msg = e.Message.ToString(), opt = RequsetedModule });
            }
        }

        /// <summary>
        /// Delete Plan,Tactic,Campaign,Program by Section.
        /// </summary>
        /// <param name="DeleteType"></param>
        /// <param name="id"></param>
        /// <param name="UserId"></param>
        /// <param name="closedTask"></param>
        /// <param name="CalledFromBudget"></param>
        /// <param name="IsIndex"></param>
        /// <param name="RedirectType"></param>
        /// <returns></returns>
        public ActionResult DeleteSection(int id = 0, string DeleteType = "", string UserId = "", string closedTask = null, string CalledFromBudget = "", bool IsIndex = false, bool RedirectType = false)
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
            try
            {
                using (MRPEntities mrp = new MRPEntities())
                {
                    using (var scope = new TransactionScope())
                    {
                        #region "Initialize Variables"
                        int returnValue = 0;
                        string Title = "";
                        string strMessage = "";
                        int cid = 0;
                        int pid = 0;
                        int tid = 0;
                        int tempLocalVariable = 0;
                        bool IsPlan = (DeleteType == Enums.Section.Plan.ToString()) ? true : false; // Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                        bool IsCampaign = (DeleteType == Enums.Section.Campaign.ToString()) ? true : false;
                        bool IsProgram = (DeleteType == Enums.Section.Program.ToString()) ? true : false;
                        bool IsTactic = (DeleteType == Enums.Section.Tactic.ToString()) ? true : false;
                        bool IsLineItem = (DeleteType == Enums.Section.LineItem.ToString()) ? true : false;
                        #endregion

                        // Start - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                        //// Delete sections e.g Plan,Campaign,Program,Tactic,LineItem.
                        if (IsPlan)
                        {
                            returnValue = Common.PlanTaskDelete(Enums.Section.Plan.ToString(), id);
                            if (returnValue != 0)
                            {
                                var planTitle = db.Plans.Where(p => p.PlanId == id).ToList().Select(p => p.Title).FirstOrDefault();
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, id, planTitle, Enums.ChangeLog_ComponentType.plan, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                                strMessage = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Plan.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                            }
                        }
                        // End - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                        else if (IsCampaign)
                        {
                            returnValue = Common.PlanTaskDelete(Enums.Section.Campaign.ToString(), id);
                            if (returnValue != 0)
                            {
                                Plan_Campaign pc = db.Plan_Campaign.Where(p => p.PlanCampaignId == id).FirstOrDefault();
                                Title = pc.Title;
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pc.PlanCampaignId, pc.Title, Enums.ChangeLog_ComponentType.campaign, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                                strMessage = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Campaign.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                            }
                        }
                        else if (IsProgram)
                        {
                            returnValue = Common.PlanTaskDelete(Enums.Section.Program.ToString(), id);
                            if (returnValue != 0)
                            {
                                Plan_Campaign_Program pc = db.Plan_Campaign_Program.Where(p => p.PlanProgramId == id).FirstOrDefault();
                                cid = pc.PlanCampaignId;
                                Title = pc.Title;
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pc.PlanProgramId, pc.Title, Enums.ChangeLog_ComponentType.program, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                                strMessage = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Program.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                            }
                        }
                        else if (IsTactic)
                        {
                            returnValue = Common.PlanTaskDelete(Enums.Section.Tactic.ToString(), id);

                            if (returnValue != 0)
                            {
                                Plan_Campaign_Program_Tactic pcpt = db.Plan_Campaign_Program_Tactic.Where(p => p.PlanTacticId == id).FirstOrDefault();
                                cid = pcpt.Plan_Campaign_Program.PlanCampaignId;
                                pid = pcpt.PlanProgramId;
                                Title = pcpt.Title;
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcpt.PlanTacticId, pcpt.Title, Enums.ChangeLog_ComponentType.tactic, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                                strMessage = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.Tactic.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                            }
                        }
                        else if (IsLineItem)
                        {
                            returnValue = Common.PlanTaskDelete(Enums.Section.LineItem.ToString(), id);
                            if (returnValue != 0)
                            {
                                Plan_Campaign_Program_Tactic_LineItem pcptl = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.PlanLineItemId == id).FirstOrDefault();
                                var objOtherLineItem = db.Plan_Campaign_Program_Tactic_LineItem.FirstOrDefault(lineitem => lineitem.PlanTacticId == pcptl.Plan_Campaign_Program_Tactic.PlanTacticId && lineitem.Title == Common.DefaultLineItemTitle && lineitem.LineItemTypeId == null);
                                double totalLoneitemCost = db.Plan_Campaign_Program_Tactic_LineItem.Where(lineitem => lineitem.PlanTacticId == pcptl.Plan_Campaign_Program_Tactic.PlanTacticId && lineitem.LineItemTypeId != null && lineitem.IsDeleted == false).ToList().Sum(lineitem => lineitem.Cost);
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

                                cid = pcptl.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.PlanCampaignId;
                                pid = pcptl.Plan_Campaign_Program_Tactic.PlanProgramId;
                                tid = pcptl.PlanTacticId;
                                Title = pcptl.Title;
                                returnValue = Common.InsertChangeLog(Sessions.PlanId, null, pcptl.PlanLineItemId, pcptl.Title, Enums.ChangeLog_ComponentType.lineitem, Enums.ChangeLog_TableName.Plan, Enums.ChangeLog_Actions.removed);
                                strMessage = string.Format(Common.objCached.PlanEntityDeleted, Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]);    // Modified by Viral Kadiya on 11/17/2014 to resolve issue for PL ticket #947.
                                tempLocalVariable = pcptl.Plan_Campaign_Program_Tactic.PlanProgramId;
                            }
                        }

                        if (returnValue >= 1)
                        {
                            //// Change Parent section status by ID.
                            if (IsProgram)
                            {
                                Common.ChangeCampaignStatus(cid);
                            }

                            if (IsTactic)
                            {
                                Common.ChangeProgramStatus(pid);
                                var PlanCampaignId = db.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false) && program.PlanProgramId == pid).Select(program => program.PlanCampaignId).Single();
                                Common.ChangeCampaignStatus(PlanCampaignId);
                            }

                            if (IsLineItem)
                            {
                                var planProgramId = tempLocalVariable;
                                Common.ChangeProgramStatus(planProgramId);
                                var PlanCampaignId = db.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false) && program.PlanProgramId == tempLocalVariable).Select(program => program.PlanCampaignId).Single();
                                Common.ChangeCampaignStatus(PlanCampaignId);
                            }

                            scope.Complete();

                            ViewBag.CampaignID = cid;
                            ViewBag.ProgramID = pid;
                            ViewBag.TacticID = tid;

                            if (!string.IsNullOrEmpty(CalledFromBudget))
                            {
                                TempData["SuccessMessage"] = strMessage;
                                // Start - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                                if (IsPlan)
                                {
                                    TempData["SuccessMessageDeletedPlan"] = strMessage;
                                    return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Budgeting.ToString(), redirect = Url.Action("PlanSelector", "Plan", new { type = CalledFromBudget }) });
                                }
                                // End - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
                                else if (IsCampaign)
                                {
                                    return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Budgeting.ToString(), redirect = Url.Action("Budgeting", "Plan", new { type = CalledFromBudget }) });
                                }
                                else if (IsProgram)
                                {
                                    return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Budgeting.ToString(), redirect = Url.Action("Budgeting", "Plan", new { type = CalledFromBudget, expand = "campaign" + cid.ToString() }) });
                                }
                                else if (IsTactic)
                                {
                                    return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Budgeting.ToString(), redirect = Url.Action("Budgeting", "Plan", new { type = CalledFromBudget, expand = "program" + pid.ToString() }) });
                                }
                                else if (IsLineItem)
                                {
                                    return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Budgeting.ToString(), redirect = Url.Action("Budgeting", "Plan", new { type = CalledFromBudget, expand = "tactic" + tid.ToString() }) });
                                }

                            }
                            else if (IsIndex)
                            {
                                //Modified by Mitesh Vaishnav for PL ticket 966
                                TempData["SuccessMessageDeletedPlan"] = "";
                                return Json(new { IsSuccess = true, redirect = Url.Action("Index"), msg = strMessage, opt = Enums.InspectPopupRequestedModules.Index.ToString() });
                            }
                            else
                            {
                                TempData["SuccessMessageDeletedPlan"] = strMessage;
                                if (RedirectType)
                                {
                                    if (closedTask != null)
                                    {
                                        TempData["ClosedTask"] = closedTask;
                                    }
                                    return Json(new { IsSuccess = true, msg = strMessage, redirect = Url.Action("ApplyToCalendar", "Plan"), opt = Enums.InspectPopupRequestedModules.ApplyToCalendar.ToString() });
                                }
                                else
                                {
                                    if (IsLineItem)
                                    {
                                        return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Assortment.ToString(), redirect = Url.Action("Assortment", "Plan", new { campaignId = cid, programId = pid, tacticId = tid }) });
                                    }

                                    return Json(new { IsSuccess = true, msg = strMessage, opt = Enums.InspectPopupRequestedModules.Assortment.ToString(), redirect = Url.Action("Assortment", "Plan", new { campaignId = cid, programId = pid }) });
                                }
                            }
                        }
                        return Json(new { IsSuccess = false, msg = Common.objCached.ErrorOccured });
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { IsSuccess = false, msg = Common.objCached.ErrorOccured });
        }

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

        /// <summary>
        /// Return integration instance exist for model
        /// Added by Mitesh Vaishnav on 12/08/2014 for PL ticket #690
        /// </summary>
        /// <param name="objModel"></param>
        /// <returns></returns>
        public string CheckIntegrationInstanceExist(Model objModel)
        {
            string returnValue = string.Empty;

            if (objModel.IntegrationInstanceId == null && objModel.IntegrationInstanceIdCW == null && objModel.IntegrationInstanceIdINQ == null && objModel.IntegrationInstanceIdMQL == null)////Modiefied by Mitesh Vaishnav on 12/08/2014 for PL ticket #690
                returnValue = "N/A";
            else
            {
                returnValue = "Yes";
            }

            return returnValue;
        }
        #endregion
    }
}

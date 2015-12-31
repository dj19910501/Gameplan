﻿using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Elmah;
using System.IO;
using System.Globalization;
using RevenuePlanner.BDSService;
using System.Data.Objects.SqlClient;
using System.Web;
using System.Xml;
using System.Data;
using Integration;
using System.Threading.Tasks;

/*
 *  Author: Manoj Limbachiya
 *  Created Date: 10/22/2013
 *  Screen: 002_000_home - Home
 *  Purpose: Home page  
  */
namespace RevenuePlanner.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class HomeController : CommonController
    {
        #region Variables

        private MRPEntities objDbMrpEntities = new MRPEntities();
        private BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
        private DateTime CalendarStartDate;
        private DateTime CalendarEndDate;
        List<User> lstUsers = new List<User>();
        List<TacticType> TacticTypeList = new List<TacticType>();
        private BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();

        #endregion

        #region "Index"

        /// <summary>
        /// Home index page
        /// In Plan Header, values of MQLs, Budget and number of Tactics of current plan shown from database.
        /// planCampaignId and planProgramId parameter added for plan and campaign popup
        /// Modified By Maninder Singh Wadhva PL Ticket#47
        /// </summary>
        /// <param name="activeMenu">Current active menu</param>
        /// <param name="currentPlanId">Current selected planId (default paremater)</param>
        /// <param name="planTacticId">planTacticId used for notification email shared link</param>
        /// <param name="planCampaignId">planCampaignId used for notification email shared link</param>
        /// <param name="planProgramId">planProgramId used for notification email shared link</param>
        /// <param name="isImprovement">isImprovement flag used with planTacticId for ImprovementTactic of notification email shared link</param>
        /// <returns>returns view as per menu selected</returns>
        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Home, int currentPlanId = 0, int planTacticId = 0, int planCampaignId = 0, int planProgramId = 0, bool isImprovement = false, bool isGridView = false, int planLineItemId = 0)
        {
            //// To get permission status for Plan create, By dharmraj PL #519
            ViewBag.IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            //// To get permission status for Add/Edit Actual, By dharmraj PL #519
            ViewBag.IsTacticActualsAddEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);
            // Get current user permission for edit own and subordinates plans.
            bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
            bool IsPlanEditable = false;
            bool isPublished = false;




            //// Set viewbag for notification email shared link inspect popup
            ViewBag.ActiveMenu = activeMenu;
            ViewBag.ShowInspectForPlanTacticId = planTacticId;
            ViewBag.ShowInspectForPlanCampaignId = planCampaignId;
            ViewBag.ShowInspectForPlanProgramId = planProgramId;
            ViewBag.IsImprovement = isImprovement;
            ViewBag.GridView = isGridView;
            ViewBag.ShowInspectForPlanLineItemId = planLineItemId;
            // Added by Komal Rawal  for new homepage ui publish button
            if (currentPlanId > 0)
            {
                //Get all subordinates of current user upto n level
                var lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
                //// Check whether his own & SubOrdinate Plan editable or Not.
                var objPlan = objDbMrpEntities.Plans.FirstOrDefault(_plan => _plan.PlanId == currentPlanId);

                if (objPlan.CreatedBy.Equals(Sessions.User.UserId)) // 
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

                Plan Plan = objDbMrpEntities.Plans.FirstOrDefault(_plan => _plan.PlanId.Equals(currentPlanId));
                isPublished = Plan.Status.Equals(Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString());



            }
            ViewBag.IsPlanEditable = IsPlanEditable;
            ViewBag.IsPublished = isPublished;
            //End
            //New Added 
            ViewBag.RedirectType = Enums.InspectPopupRequestedModules.Index.ToString();


            //// Start - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021
            //// Check insepct popup shared link validation to open insepct pop up as per link
            if (activeMenu.Equals(Enums.ActiveMenu.Plan) && currentPlanId > 0)
            {
                currentPlanId = InspectPopupSharedLinkValidation(currentPlanId, planCampaignId, planProgramId, planTacticId, isImprovement, planLineItemId);
            }
            else if (activeMenu.Equals(Enums.ActiveMenu.Home) && currentPlanId > 0 && (planTacticId > 0 || planCampaignId > 0 || planProgramId > 0))
            {
                //// Invalid url or manipulated url
                ViewBag.ShowInspectPopup = false;
                ViewBag.ShowInspectPopupErrorMessage = Common.objCached.InvalidURLForInspectPopup.ToString();
            }
            else if ((activeMenu.Equals(Enums.ActiveMenu.Plan) || activeMenu.Equals(Enums.ActiveMenu.Home)) && currentPlanId <= 0 && (planTacticId > 0 || planCampaignId > 0 || planProgramId > 0))
            {
                //// Invalid url or manipulated url
                ViewBag.ShowInspectPopup = false;
                ViewBag.ShowInspectPopupErrorMessage = Common.objCached.InvalidURLForInspectPopup.ToString();
            }
            else
            {
                ViewBag.ShowInspectPopup = true;
            }
            //// End - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021

            ViewBag.SuccessMessageDuplicatePlan = TempData["SuccessMessageDuplicatePlan"];
            ViewBag.ErrorMessageDuplicatePlan = TempData["ErrorMessageDuplicatePlan"];

            if (TempData["SuccessMessageDeletedPlan"] != null)
            {
                ViewBag.SuccessMessageDuplicatePlan = TempData["SuccessMessageDeletedPlan"];
                TempData["SuccessMessageDeletedPlan"] = string.Empty;
            }
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessageDuplicatePlan = TempData["ErrorMessage"];
                TempData["ErrorMessage"] = string.Empty;
            }

            HomePlanModel planmodel = new Models.HomePlanModel();

            //// Get active model list
            //List<Model> models = objDbMrpEntities.Models.Where(model => model.ClientId.Equals(Sessions.User.ClientId) && model.IsDeleted == false).ToList();

            //// Get modelIds
            List<int> modelIds = objDbMrpEntities.Models.Where(model => model.ClientId.Equals(Sessions.User.ClientId) && model.IsDeleted == false).Select(m => m.ModelId).ToList();
            //// Get list of active plan by selected modelId list
            List<Plan> activePlan = objDbMrpEntities.Plans.Where(p => modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true) && p.IsDeleted == false).ToList();

            //Added by Ashish Mistry on 23/11/2015 for PL ticket #1772
            Plan currentPlan = new Plan();
            Plan latestPlan = new Plan();
            string currentYear = DateTime.Now.Year.ToString();
            string planPublishedStatus = Enums.PlanStatusValues.FirstOrDefault(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            if (activePlan.Count() > 0)
            {
                //Added by Ashish Mistry on 23/11/2015 for PL ticket #1772
                IsPlanEditable = true;
                ViewBag.IsPlanEditable = IsPlanEditable;

                latestPlan = activePlan.OrderBy(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).Select(plan => plan).FirstOrDefault();
                List<Plan> fiterActivePlan = new List<Plan>();
                fiterActivePlan = activePlan.Where(plan => Convert.ToInt32(plan.Year) < Convert.ToInt32(currentYear)).ToList();
                if (fiterActivePlan != null && fiterActivePlan.Any())
                {
                    latestPlan = fiterActivePlan.OrderByDescending(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).FirstOrDefault();
                }
                if (currentPlanId != 0)
                {
                    currentPlan = activePlan.Where(p => p.PlanId.Equals(currentPlanId)).FirstOrDefault();
                    if (currentPlan == null)
                    {
                        //// Based on currentPlanId, if currentPlan not found then pick first plan from list of activeplans
                        currentPlan = latestPlan;
                        currentPlanId = currentPlan.PlanId;
                    }
                }
                else if (!Common.IsPlanPublished(Sessions.PlanId))
                {
                    if (Sessions.PublishedPlanId == 0)
                    {
                        fiterActivePlan = new List<Plan>();
                        fiterActivePlan = activePlan.Where(plan => plan.Year == currentYear && plan.Status.Equals(planPublishedStatus)).ToList();
                        //// Added by Bhavesh, Current year first plan select in dropdown
                        if (fiterActivePlan != null && fiterActivePlan.Any())
                        {
                            currentPlan = fiterActivePlan.OrderBy(plan => plan.Title).FirstOrDefault();
                        }
                        else
                        {
                            currentPlan = latestPlan;
                        }
                    }
                    else
                    {
                        currentPlan = activePlan.Where(plan => plan.PlanId.Equals(Sessions.PublishedPlanId)).OrderBy(plan => plan.Title).FirstOrDefault();
                        if (currentPlan == null)
                        {
                            currentPlan = latestPlan;
                        }
                    }
                }
                else
                {
                    //// added by Nirav shah for TFS Point : 218
                    if (Sessions.PlanId == 0)
                    {
                        fiterActivePlan = new List<Plan>();
                        fiterActivePlan = activePlan.Where(plan => plan.Year == currentYear && plan.Status.Equals(planPublishedStatus)).ToList();
                        //// Added by Bhavesh, Current year first plan select in dropdown
                        if (fiterActivePlan != null && fiterActivePlan.Any())
                        {
                            currentPlan = fiterActivePlan.OrderBy(plan => plan.Title).FirstOrDefault();
                        }
                        else
                        {
                            currentPlan = latestPlan;
                        }

                    }
                    else
                    {
                        //// Select plan using planId from session
                        currentPlan = activePlan.Where(plan => plan.PlanId.Equals(Sessions.PlanId)).FirstOrDefault();
                        if (currentPlan == null)
                        {
                            currentPlan = latestPlan;
                        }

                    }
                }
                isPublished = currentPlan.Status.Equals(Enums.PlanStatusValues[Enums.PlanStatus.Published.ToString()].ToString()); //Added by Dashrath Prajapati-PL #1758 Publish Plan: Unable to Publish Draft Plan 
                ViewBag.IsPublished = isPublished;
            }

            if (Enums.ActiveMenu.Home.Equals(activeMenu))
            {
                //// Get list of Active(published) plans for all above models
                //Modified for #1750 by Komal Rawal
                var LastSetOfPlanSelected = new List<string>();
                var Label = Enums.FilterLabel.Plan.ToString();
                var FilterName = Sessions.FilterPresetName;
                var SetOfPlanSelected = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.FilterName == Label && view.Userid == Sessions.User.UserId).ToList();
                var FinalSetOfPlanSelected = "";
                if (FilterName != null && FilterName != "")
                {
                    FinalSetOfPlanSelected = SetOfPlanSelected.Where(view => view.ViewName == FilterName).Select(View => View.FilterValues).FirstOrDefault();
                }
                else
                {
                    FinalSetOfPlanSelected = SetOfPlanSelected.Where(view => view.IsDefaultPreset == true).Select(View => View.FilterValues).FirstOrDefault();
                    if (FinalSetOfPlanSelected == null)
                    {
                        FinalSetOfPlanSelected = SetOfPlanSelected.Where(view => view.ViewName == null).Select(View => View.FilterValues).FirstOrDefault();
                    }

                }
                if (FinalSetOfPlanSelected != null)
                {
                    LastSetOfPlanSelected = FinalSetOfPlanSelected.Split(',').ToList();
                }

                activePlan = activePlan.Where(plan => plan.Status.Equals(planPublishedStatus) && plan.IsDeleted == false).ToList();
                planmodel.lstPlan = activePlan.Select(plan => new PlanListModel
                {
                    PlanId = plan.PlanId,
                    Title = HttpUtility.HtmlDecode(plan.Title),
                    Checked = LastSetOfPlanSelected.Count.Equals(0) ? plan.PlanId == currentPlan.PlanId ? "checked" : "" : LastSetOfPlanSelected.Contains(plan.PlanId.ToString()) ? "checked" : "",

                }).Where(plan => !string.IsNullOrEmpty(plan.Title)).OrderBy(plan => plan.Title, new AlphaNumericComparer()).ToList();
            }

            //// Added by Bhavesh, Current year first plan select in dropdown

            if (activePlan.Count() > 0)
            {
                try
                {

                    //// added by Nirav for plan consistency on 14 apr 2014
                    planmodel.PlanTitle = currentPlan.Title;
                    planmodel.PlanId = currentPlan.PlanId;
                    // planmodel.objplanhomemodelheader = new HomePlanModelHeader();
                    planmodel.objplanhomemodelheader = Common.GetPlanHeaderValue(currentPlan.PlanId, onlyplan: true);

                    Sessions.PlanId = planmodel.PlanId;
                    GetCustomAttributesIndex(ref planmodel);


                    //// Select Tactics of selected plans
                    //    var campaignList = objDbMrpEntities.Plan_Campaign.Where(campaign => campaign.IsDeleted.Equals(false) && planmodel.PlanId == campaign.PlanId).Select(campaign => campaign.PlanCampaignId).ToList();
                    //     var programList = objDbMrpEntities.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false) && campaignList.Contains(program.PlanCampaignId)).Select(program => program.PlanProgramId).ToList();
                    //     var tacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && programList.Contains(tactic.PlanProgramId)).Select(tactic => tactic).ToList();
                    //    List<int> planTacticIds = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
                    //    List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                    //    planmodel.lstOwner = GetOwnerList(PlanGanttTypes.Tactic.ToString(), activeMenu.ToString(), tacticList, lstAllowedEntityIds);

                    //    planmodel.lstTacticType = GetTacticTypeList(PlanGanttTypes.Tactic.ToString(), activeMenu.ToString(), tacticList, lstAllowedEntityIds);

                    // Start - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021
                    if (ViewBag.ShowInspectPopup != null)
                    {
                        if ((bool)ViewBag.ShowInspectPopup == true && activeMenu.Equals(Enums.ActiveMenu.Plan) && currentPlanId > 0)
                        {
                            bool isCustomRestrictionPass = InspectPopupSharedLinkValidationForCustomRestriction(planCampaignId, planProgramId, planTacticId, isImprovement, planLineItemId);
                            ViewBag.ShowInspectPopup = isCustomRestrictionPass;
                            if (isCustomRestrictionPass.Equals(false))
                            {
                                ViewBag.ShowInspectPopupErrorMessage = Common.objCached.CustomRestrictionFailedForInspectPopup.ToString();
                            }
                        }
                    }
                    //// End - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021
                }
                catch (Exception objException)
                {
                    ErrorSignal.FromCurrentContext().Raise(objException);

                    ////To handle unavailability of BDSService
                    if (objException is System.ServiceModel.EndpointNotFoundException)
                    {
                        return RedirectToAction("ServiceUnavailable", "Login");
                    }
                }

                return View("Index", planmodel);
            }
            else
            {
                if (activeMenu != Enums.ActiveMenu.Plan)
                {
                    TempData["ErrorMessage"] = Common.objCached.NoPublishPlanAvailable;
                }
                else
                {
                    TempData["ErrorMessage"] = null;
                }

                //// Start - Added by Sohel Pathan on 15/12/2014 for PL ticket #1021
                if (ViewBag.ShowInspectPopup != null)
                {
                    if ((bool)ViewBag.ShowInspectPopup == false)
                    {
                        TempData["ErrorMessage"] = ViewBag.ShowInspectPopupErrorMessage;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = Common.objCached.NoPublishPlanAvailable;
                    }
                }
                //// End - Added by Sohel Pathan on 15/12/2014 for PL ticket #1021

                return RedirectToAction("PlanSelector", "Plan");
            }
        }

        /// <summary>
        /// Action to retieve PlanDropdown partial view with proper values
        /// </summary>
        /// <param name="currentPlanId">current selected plan Id</param>
        /// <param name="activeMenu">current active menu</param>
        /// <returns>returns partial view of PlanDropdown</returns>
        public ActionResult HomePlan(int currentPlanId)
        {
            HomePlan objHomePlan = new HomePlan();

            //// Prepare ViewBy dropdown values
            List<ViewByModel> lstViewByTab = Common.GetDefaultGanttTypes(null);
            ViewBag.ViewByTab = lstViewByTab;

            //// Prepare upcoming activity dropdown values
            List<SelectListItem> lstUpComingActivity = UpComingActivity(Convert.ToString(currentPlanId));
            lstUpComingActivity = lstUpComingActivity.Where(activity => !string.IsNullOrEmpty(activity.Text)).OrderBy(activity => activity.Text, new AlphaNumericComparer()).ToList();
            ViewBag.ViewUpComingActivity = lstUpComingActivity;

            return PartialView("_PlanDropdown", objHomePlan);
        }

        #endregion

        #region "UserPhoto"
        /// <summary>
        /// Function to get User profile photo for header section.
        /// Reason: To show User profile photo at Header section.
        /// </summary>
        /// <returns>Returns User Profile Image.</returns>
        public ActionResult UserPhoto()
        {
            byte[] imageBytes = Common.ReadFile(Server.MapPath("~") + "/content/images/user_image_not_found.png");
            if (Sessions.User.ProfilePhoto != null)
            {
                imageBytes = Sessions.User.ProfilePhoto;
            }

            //// Prepare image file from bytes array
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                ms.Write(imageBytes, 0, imageBytes.Length);
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                image = Common.ImageResize(image, 36, 36, true, false);
                imageBytes = Common.ImageToByteArray(image);
                return File(imageBytes, "image/jpg");
            }
        }
        #endregion

        #region "View Control & GANTT Chart (Prepare GANTT Chart data)"

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Modified By Maninder Singh Wadhva PL Ticket#47
        /// Date: 11/15/2013
        /// Function to get tactic for view control.
        /// Modfied By: Maninder Singh Wadhva.
        /// Reason: To show task whose either start or end date or both date are outside current view.
        /// </summary>
        /// <param name="viewBy">selected viewby option from dropdown</param>
        /// <param name="planId">comma separated list of plan id</param>
        /// <param name="timeFrame">fiscal year or time frame selected option</param>
        /// <param name="customFieldIds">comma separated custom field ids from search filter</param>
        /// <param name="ownerIds">comma separated Owner ids from search filter</param>
        /// <param name="activeMenu">current/selected active menu</param>
        /// <param name="getViewByList">flag to retrieve viewby list</param>
        /// <returns>Returns json result object of tactic type and plan campaign program tactic.</returns>
        public async Task<JsonResult> GetViewControlDetail(string viewBy, string planId, string timeFrame, string customFieldIds, string ownerIds, string activeMenu, bool getViewByList, string TacticTypeid, string StatusIds, bool isupdate)
        {
            //Added By Komal Rawal to get all the user names for honeycomb feature
            TacticTypeList = objDbMrpEntities.TacticTypes.Where(tt => tt.IsDeleted == false).Select(tt => tt).ToList();
            lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
            //End
            //// Create plan list based on PlanIds of search filter

            List<int> planIds = string.IsNullOrWhiteSpace(planId) ? new List<int>() : planId.Split(',').Select(plan => int.Parse(plan)).ToList();

            var lstPlans = objDbMrpEntities.Plans.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && plan.Model.ClientId.Equals(Sessions.User.ClientId)).Select(plan => new { plan.PlanId, plan.Model.ClientId, plan.Year }).ToList();



            bool IsRequest = false;
            bool IsFiltered = false;
            string planYear = string.Empty;
            int year;
            bool isNumeric = int.TryParse(timeFrame, out year);

            if (isNumeric)
            {
                planYear = Convert.ToString(year);
            }
            else
            {
                planYear = DateTime.Now.Year.ToString();
            }

            // Add By Nishant Sheth
            // DESC:: For get default filter view after user log out #1750

            var Label = Enums.FilterLabel.Plan.ToString();
            var SetOfPlanSelected = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.FilterName != Label && view.Userid == Sessions.User.UserId && view.ViewName == null).Select(View => View.FilterValues).ToList();
            if (SetOfPlanSelected.Count > 0)
            {
                isupdate = true;
            }

            if ((customFieldIds != "" || ownerIds != "" || TacticTypeid != "" || StatusIds != "") && isupdate)
            {
                IsFiltered = true;

            }

            //// Get list of planIds from filtered plans
            var filteredPlanIds = lstPlans.Where(plan => plan.Year == planYear || planId.Split(',').Contains(plan.PlanId.ToString())).ToList().Select(plan => plan.PlanId).ToList();

            CalendarStartDate = CalendarEndDate = DateTime.Now;
            Common.GetPlanGanttStartEndDate(planYear, timeFrame, ref CalendarStartDate, ref CalendarEndDate);

            //// Start Maninder Singh Wadhva : 11/15/2013 - Getting list of tactic for view control for plan version id.
            //// Selecting campaign(s) of plan whose IsDelete=false.

            var lstCampaign = objDbMrpEntities.Plan_Campaign.Where(campaign => filteredPlanIds.Contains(campaign.PlanId) && campaign.IsDeleted.Equals(false) && (!((campaign.EndDate < CalendarStartDate) || (campaign.StartDate > CalendarEndDate))))
                                           .Select(campaign => campaign).ToList();

            //// Selecting campaignIds.
            List<int> lstCampaignId = lstCampaign.Select(campaign => campaign.PlanCampaignId).ToList();

            //// Selecting program(s) of campaignIds whose IsDelete=false.
            var lstProgram = objDbMrpEntities.Plan_Campaign_Program.Where(program => lstCampaignId.Contains(program.PlanCampaignId) && program.IsDeleted.Equals(false) && (!((program.EndDate < CalendarStartDate) || (program.StartDate > CalendarEndDate))))
                                                  .Select(program => program)
                                                  .ToList();

            //// Selecting programIds.
            List<int> lstProgramId = lstProgram.Select(program => program.PlanProgramId).ToList();

            //// Owner filter criteria.
            List<Guid> filterOwner = string.IsNullOrWhiteSpace(ownerIds) ? new List<Guid>() : ownerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();

            //Modified by komal rawal for #1283
            //TacticType filter criteria
            List<int> filterTacticType = string.IsNullOrWhiteSpace(TacticTypeid) ? new List<int>() : TacticTypeid.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();
            //TacticType filter criteria
            List<string> filterStatus = string.IsNullOrWhiteSpace(StatusIds) ? new List<string>() : StatusIds.Split(',').Select(tactictype => tactictype).ToList();
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);




            //// Applying filters to tactic (IsDelete, Individuals)
            List<Plan_Tactic> lstTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
                                                                       lstProgramId.Contains(tactic.PlanProgramId) &&
                                                                       (filterOwner.Count.Equals(0) || filterOwner.Contains(tactic.CreatedBy)) &&
                                                                        (filterTacticType.Count.Equals(0) || filterTacticType.Contains(tactic.TacticType.TacticTypeId)) &&
                                                                        (filterStatus.Count.Equals(0) || filterStatus.Contains(tactic.Status))
                                                                        && (!((tactic.EndDate < CalendarStartDate) || (tactic.StartDate > CalendarEndDate))))
                                                                       .ToList().Select(tactic => new Plan_Tactic
                                                                       {
                                                                           objPlanTactic = tactic,
                                                                           PlanCampaignId = tactic.Plan_Campaign_Program.PlanCampaignId,
                                                                           PlanId = tactic.Plan_Campaign_Program.Plan_Campaign.PlanId,
                                                                           objPlanTacticProgram = tactic.Plan_Campaign_Program,
                                                                           objPlanTacticCampaign = tactic.Plan_Campaign_Program.Plan_Campaign,
                                                                           objPlanTacticCampaignPlan = tactic.Plan_Campaign_Program.Plan_Campaign.Plan,
                                                                           TacticType = tactic.TacticType,
                                                                           CreatedBy = tactic.CreatedBy
                                                                       }).ToList();


            List<string> lstFilteredCustomFieldOptionIds = new List<string>();
            List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
            //// Apply Custom restriction for None type
            if (lstTactic.Count() > 0)
            {
                List<int> lstTacticIds = lstTactic.Select(tactic => tactic.objPlanTactic.PlanTacticId).ToList();

                //// Start - Added by Sohel Pathan on 16/01/2015 for PL ticket #1134
                //// Custom Field Filter Criteria.
                //  List<string> filteredCustomFields = string.IsNullOrWhiteSpace(customFieldIds) ? new List<string>() : customFieldIds.Split(',').Select(customFieldId => customFieldId.ToString()).ToList();
                string[] filteredCustomFields = string.IsNullOrWhiteSpace(customFieldIds) ? null : customFieldIds.Split(',');
                if (filteredCustomFields != null)
                {
                    //  filteredCustomFields.ForEach(customField =>
                    foreach (string customField in filteredCustomFields)
                    {
                        string[] splittedCustomField = customField.Split('_');
                        lstCustomFieldFilter.Add(new CustomFieldFilter { CustomFieldId = int.Parse(splittedCustomField[0]), OptionId = splittedCustomField[1] });
                        lstFilteredCustomFieldOptionIds.Add(splittedCustomField[1]);
                    };
                    lstTacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, lstTacticIds);
                    //// get Allowed Entity Ids
                    lstTactic = lstTactic.Where(tactic => lstTacticIds.Contains(tactic.objPlanTactic.PlanTacticId)).ToList();
                }
                //// End - Added by Sohel Pathan on 16/01/2015 for PL ticket #1134
                //    //Modified by Komal Rawal for #1750 - For viewing onlly those tactic where user is owner, collaborator or have edit permission.
                if (IsFiltered == false && customFieldIds == "" && ownerIds == "" && TacticTypeid == "" && StatusIds == "")
                {
                    //Modified by Komal Rawal for #1750 - For viewing onlly those tactic where user is owner, collaborator or have edit permission.
                    //List<string> collaboratorIds = Common.GetAllCollaborators(lstTacticIds).Distinct().ToList();
                    //List<Guid> collaboratorIdsList = collaboratorIds.Select(Guid.Parse).ToList();
                    //List<Guid> lstSubordinatesIds = new List<Guid>();
                    //if (IsTacticAllowForSubordinates)
                    //{
                    //    lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
                    //}
                    //List<int> lsteditableEntityIds = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);

                    //Modified By Komal Rawal for #1505
                    // Modified By Nishant Sheth 
                    // Desc:: To resolve the #1790 observation
                    lstTactic = lstTactic.Where(tactic => tactic.CreatedBy == Sessions.User.UserId).Select(tactic => tactic).ToList();
                }
                else
                {
                    List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);
                    //Modified By Komal Rawal for #1505
                    lstTactic = lstTactic.Where(tactic => lstAllowedEntityIds.Contains(tactic.objPlanTactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.UserId).Select(tactic => tactic).ToList();
                }
            }

            //// Modified By Maninder Singh Wadhva PL Ticket#47
            //// Get list of Improvement tactics based on selected plan ids
            List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic = objDbMrpEntities.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => filteredPlanIds.Contains(improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId) &&
                                                                                      improvementTactic.IsDeleted.Equals(false) &&
                                                                                      (improvementTactic.EffectiveDate > CalendarEndDate).Equals(false))
                                                                               .Select(improvementTactic => improvementTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

            var lstSubordinatesWithPeers = new List<Guid>();
            try
            {
                //// Get list of subordinates and peer users
                lstSubordinatesWithPeers = Common.GetSubOrdinatesWithPeersNLevel();
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
            }

            //// Get list of tactic and improvementTactic for which selected users are Owner
            var subordinatesTactic = lstTactic.Where(tactic => lstSubordinatesWithPeers.Contains(tactic.objPlanTactic.CreatedBy)).ToList();
            var subordinatesImprovementTactic = lstImprovementTactic.Where(improvementTactic => lstSubordinatesWithPeers.Contains(improvementTactic.CreatedBy)).ToList();

            //// Get Submitted tactic count for Request tab
            string tacticStatus = Enums.TacticStatusValues.FirstOrDefault(status => status.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            //// Modified By Maninder Singh Wadhva PL Ticket#47, Modofied by Dharmraj #538
            string requestCount = Convert.ToString(subordinatesTactic.Where(tactic => tactic.objPlanTactic.Status.Equals(tacticStatus)).Count() + subordinatesImprovementTactic.Where(improvementTactic => improvementTactic.Status.Equals(tacticStatus)).Count());

            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, activeMenu.ToLower());

            var tacticForAllTabs = lstTactic.ToList();

            //// Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
            if (viewBy.Equals(PlanGanttTypes.Request.ToString()))
            {
                lstTactic = subordinatesTactic;
                lstImprovementTactic = subordinatesImprovementTactic;
            }

            object improvementTacticForAccordion = new object();
            object improvementTacticTypeForAccordion = new object();

            //// Added by Dharmraj Ticket #364,#365,#366
            //// Filter tactic and improvementTactic based on status and selected ViewBy option(tab)
            if (objactivemenu.Equals(Enums.ActiveMenu.Plan))
            {

                if (viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    IsRequest = true;
                    List<string> status = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                    List<string> statusCD = new List<string>();
                    statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                    statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

                    lstTactic = lstTactic.Where(t => status.Contains(t.objPlanTactic.Status) || ((t.objPlanTactic.CreatedBy == Sessions.User.UserId && !viewBy.Equals(PlanGanttTypes.Request.ToString())) ? statusCD.Contains(t.objPlanTactic.Status) : false))
                                    .Select(planTactic => planTactic).ToList<Plan_Tactic>();

                    //List<string> improvementTacticStatus = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                    lstImprovementTactic = lstImprovementTactic.Where(improvementTactic => status.Contains(improvementTactic.Status) || ((improvementTactic.CreatedBy == Sessions.User.UserId && !viewBy.Equals(PlanGanttTypes.Request.ToString())) ? statusCD.Contains(improvementTactic.Status) : false))
                                                               .Select(improvementTactic => improvementTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();


                }
                else
                {

                    lstTactic = lstTactic.Where(t => t.objPlanTactic.IsDeleted == false && !viewBy.Equals(PlanGanttTypes.Request.ToString()))
                                    .Select(planTactic => planTactic).ToList<Plan_Tactic>();

                    if (IsFiltered && lstTactic.Count() == 0)
                    {
                        lstImprovementTactic = new List<Plan_Improvement_Campaign_Program_Tactic>();
                    }
                    else
                    {
                        lstImprovementTactic = lstImprovementTactic.Where(improvementTactic => improvementTactic.IsDeleted == false && !viewBy.Equals(PlanGanttTypes.Request.ToString()))
                                                                   .Select(improvementTactic => improvementTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

                    }

                }

            }
            else if (objactivemenu.Equals(Enums.ActiveMenu.Home))
            {
                if (viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    IsRequest = true;
                }
                List<string> status = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                List<string> statusCD = new List<string>();
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());
                lstTactic = lstTactic.Where(t => status.Contains(t.objPlanTactic.Status) || (!viewBy.Equals(PlanGanttTypes.Request.ToString()) ? statusCD.Contains(t.objPlanTactic.Status) : false)).Select(planTactic => planTactic).ToList<Plan_Tactic>();

                if (IsFiltered && lstTactic.Count() == 0)
                {
                    lstImprovementTactic = new List<Plan_Improvement_Campaign_Program_Tactic>();
                }
                else
                {
                    lstImprovementTactic = lstImprovementTactic.Where(improveTactic => status.Contains(improveTactic.Status) || (!viewBy.Equals(PlanGanttTypes.Request.ToString()) ? statusCD.Contains(improveTactic.Status) : false))
                                                               .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

                }
                //// Prepare objects for ImprovementTactic section of Left side accordian pane
                improvementTacticForAccordion = GetImprovementTacticForAccordion(lstImprovementTactic);
                improvementTacticTypeForAccordion = GetImprovementTacticTypeForAccordion(lstImprovementTactic);
            }

            //// Start - Added by Sohel Pathan on 28/10/2014 for PL ticket #885
            //// Prepare viewBy option list based on obtained tactic list
            List<ViewByModel> viewByListResult = prepareViewByList(getViewByList, tacticForAllTabs);
            if (viewByListResult.Count > 0)
            {
                if (!(viewByListResult.Where(v => v.Value.Equals(viewBy, StringComparison.OrdinalIgnoreCase)).Any()))
                {
                    viewBy = PlanGanttTypes.Tactic.ToString();
                }
            }
            //// End - Added by Sohel Pathan on 28/10/2014 for PL ticket #885
            await Task.Delay(1);
            try
            {
                if (viewBy.Equals(PlanGanttTypes.Tactic.ToString(), StringComparison.OrdinalIgnoreCase) || viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        viewBy = PlanGanttTypes.Tactic.ToString();
                    }
                    return PrepareTacticAndRequestTabResult(filteredPlanIds, viewBy, IsFiltered, IsRequest, objactivemenu, lstCampaign.ToList(), lstProgram.ToList(), lstTactic.ToList(), lstImprovementTactic, requestCount, planYear, improvementTacticForAccordion, improvementTacticTypeForAccordion, viewByListResult);
                }
                else
                {
                    return PrepareCustomFieldTabResult(viewBy, objactivemenu, lstCampaign.ToList(), lstProgram.ToList(), lstTactic.ToList(), lstImprovementTactic, requestCount, planYear, improvementTacticForAccordion, improvementTacticTypeForAccordion, viewByListResult, lstCustomFieldFilter);
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { isError = true }, JsonRequestBehavior.AllowGet);
        }


        #region Prepare Custom Fields Tab data (Tabs other than Tactic and Request)
        /// <summary>
        /// Created By :- Sohel Pathan
        /// Created Date :- 08/10/2014
        /// Prepare Json result for customField tab(viewBy) to be rendered in gantt chart
        /// this function is used for all tabs(ViewBy options) other than tactic and request
        /// </summary>
        /// <param name="viewBy">viewBy type</param>
        /// <param name="activemenu">current active menu</param>
        /// <param name="lstCampaign">list of campaigns</param>
        /// <param name="lstProgram">list of programs</param>
        /// <param name="lstTactic">list of tactics</param>
        /// <param name="lstImprovementTactic">list of improvement tactics</param>
        /// <param name="requestCount">No. of tactic count for Request tab</param>
        /// <param name="planYear">Plan year</param>
        /// <param name="improvementTacticForAccordion">list of improvement tactic for accrodian(left side pane)</param>
        /// <param name="improvementTacticTypeForAccordion">list of improvement tactic type for accrodian(left side pane)</param>
        /// <param name="viewByListResult">list of viewBy dropdown options</param>
        /// <returns>Json result, list of task to be rendered in Gantt chart</returns>
        private JsonResult PrepareCustomFieldTabResult(string viewBy, Enums.ActiveMenu activemenu, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion, List<ViewByModel> viewByListResult, List<CustomFieldFilter> lstCustomFieldFilter)
        {
            string sourceViewBy = viewBy;
            string doubledesh = "--";
            int CustomTypeId = 0;
            bool IsCampaign = viewBy.Contains(Common.CampaignCustomTitle) ? true : false;
            bool IsProgram = viewBy.Contains(Common.ProgramCustomTitle) ? true : false;
            bool IsTactic = viewBy.Contains(Common.TacticCustomTitle) ? true : false;
            string entityType = Enums.EntityType.Tactic.ToString();

            //Added By komal Rawal for #1282
            Dictionary<string, string> ColorCodelist = objDbMrpEntities.EntityTypeColors.ToDictionary(e => e.EntityType.ToLower(), e => e.ColorCode);
            var TacticColor = ColorCodelist[Enums.EntityType.Tactic.ToString().ToLower()];
            var ImprovementColor = ColorCodelist[Enums.EntityType.ImprovementTactic.ToString().ToLower()];
            var ProgramColor = ColorCodelist[Enums.EntityType.Program.ToString().ToLower()];
            var CampaignColor = ColorCodelist[Enums.EntityType.Campaign.ToString().ToLower()];
            var PlanColor = ColorCodelist[Enums.EntityType.Plan.ToString().ToLower()];

            //Added BY Ravindra Singh Sisodiya, Get Subordinates Ids List #1433
            List<Guid> lstSubordinatesIds = new List<Guid>();
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
            }
            var IsPlanCreateAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);

            //// Set viewBy option as per GanttType selecte and grap CustomTypeId in case of CustomField
            if (IsTactic)
            {
                CustomTypeId = Convert.ToInt32(viewBy.Replace(Common.TacticCustomTitle, string.Empty));
                viewBy = PlanGanttTypes.Custom.ToString();
            }
            else if (IsCampaign)
            {
                CustomTypeId = Convert.ToInt32(viewBy.Replace(Common.CampaignCustomTitle, string.Empty));
                entityType = Enums.EntityType.Campaign.ToString();
                viewBy = PlanGanttTypes.Custom.ToString();
            }
            else if (IsProgram)
            {
                CustomTypeId = Convert.ToInt32(viewBy.Replace(Common.ProgramCustomTitle, string.Empty));
                entityType = Enums.EntityType.Program.ToString();
                viewBy = PlanGanttTypes.Custom.ToString();
            }

            List<TacticTaskList> lstTacticTaskList = new List<TacticTaskList>();
            List<CustomFields> lstCustomFields = new List<CustomFields>();
            List<ImprovementTaskDetail> lstImprovementTaskDetails = new List<ImprovementTaskDetail>();
            List<Plan_Tactic> tacticListByViewById = new List<Plan_Tactic>();
            int tacticPlanId = 0, tacticstageId = 0, tacticPlanCampaignId = 0;
            var tacticstatus = "";
            DateTime MinStartDateForCustomField;
            DateTime MinStartDateForPlan, MaxEndDateForPlan;
            List<DateTime> EffectiveDateList;
            List<TacticStageMapping> lstTacticStageMap = new List<TacticStageMapping>();
            DateTime minEffectiveDate = new DateTime();
            if (viewBy.Equals(PlanGanttTypes.Stage.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                List<int> PlanIds = lstTactic.Select(_tac => _tac.PlanId).ToList();
                List<ProgressModel> EffectiveDateListByPlanIds = lstImprovementTactic.Where(imprvmnt => PlanIds.Contains(imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).Select(imprvmnt => new ProgressModel { PlanId = imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, EffectiveDate = imprvmnt.EffectiveDate }).ToList();

                List<int> lstStageIds = new List<int>();
                lstStageIds = lstTactic.Select(tac => tac.objPlanTactic.StageId).Distinct().ToList();
                TacticStageMapping objStageTac;
                List<Plan_Tactic> lstPlanTactic;
                List<int> lstProgramIds, lstCampaignIds;
                DateTime minTacticDate = new DateTime();
                DateTime minProgramDate = new DateTime();
                DateTime minCampaignDate = new DateTime();
                List<Plan_Campaign_Program> programList;
                lstStageIds.ForEach(stgId =>
                {
                    lstPlanTactic = new List<Plan_Tactic>();
                    programList = new List<Plan_Campaign_Program>();
                    lstPlanTactic = lstTactic.Where(tatic => tatic.objPlanTactic.StageId == stgId).ToList();
                    lstProgramIds = lstCampaignIds = new List<int>();
                    lstProgramIds = lstPlanTactic.Select(tac => tac.objPlanTactic.PlanProgramId).ToList();
                    programList = lstProgram.Where(prg => lstProgramIds.Contains(prg.PlanProgramId)).ToList();
                    lstCampaignIds = programList.Select(prg => prg.PlanCampaignId).ToList();
                    objStageTac = new TacticStageMapping();
                    objStageTac.StageId = stgId;
                    objStageTac.TacticList = lstPlanTactic;
                    minTacticDate = lstPlanTactic.Select(tac => tac.objPlanTactic.StartDate).Min();
                    minProgramDate = programList.Select(prg => prg.StartDate).Min();
                    minCampaignDate = lstCampaign.Where(campgn => lstCampaignIds.Contains(campgn.PlanCampaignId)).Select(campgn => campgn.StartDate).Min();
                    objStageTac.minDate = new[] { minTacticDate, minProgramDate, minCampaignDate }.Min();
                    lstTacticStageMap.Add(objStageTac);
                });
                foreach (var tacticItem in lstTactic)
                {
                    tacticstageId = tacticItem.objPlanTactic.StageId;
                    tacticListByViewById = lstTacticStageMap.Where(tatic => tatic.StageId == tacticstageId).Select(tac => tac.TacticList).FirstOrDefault();
                    tacticPlanId = tacticItem.PlanId;
                    MinStartDateForCustomField = lstTacticStageMap.Where(tatic => tatic.StageId == tacticstageId).Select(tac => tac.minDate).FirstOrDefault();//GetMinStartDateStageAndBusinessUnit(lstCampaign, lstProgram, tacticListByViewById);

                    MinStartDateForPlan = GetMinStartDateForPlanOfCustomField(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById);

                    EffectiveDateList = new List<DateTime>();
                    if (EffectiveDateListByPlanIds != null && EffectiveDateListByPlanIds.Count > 0)
                    {
                        EffectiveDateList = EffectiveDateListByPlanIds.Where(_date => _date.PlanId == tacticPlanId).Select(_date => _date.EffectiveDate).ToList();
                        if (EffectiveDateList != null && EffectiveDateList.Count > 0)
                            minEffectiveDate = EffectiveDateList.Min();
                    }
                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = tacticItem.objPlanTactic,
                        PlanTactic = tacticItem,
                        CreatedBy = tacticItem.CreatedBy,
                        CustomFieldId = tacticstageId.ToString(),
                        CustomFieldTitle = tacticItem.objPlanTactic.Stage.Title,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tacticstageId, tacticPlanId, tacticItem.PlanCampaignId, tacticItem.objPlanTactic.PlanProgramId, tacticItem.objPlanTactic.PlanTacticId),
                        ColorCode = TacticColor,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                        EndDate = DateTime.MaxValue,
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField,
                                                                GetMaxEndDateStageAndBusinessUnit(lstCampaign, lstProgram, tacticListByViewById)),
                        CampaignProgress = GetCampaignProgressViewBy(tacticListByViewById, tacticItem.objPlanTacticCampaign, minEffectiveDate),
                        ProgramProgress = GetProgramProgressViewBy(tacticListByViewById, tacticItem.objPlanTacticProgram, minEffectiveDate),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlan),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlan,
                                                    GetMaxEndDateForPlanOfCustomFields(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById)),
                                                    tacticListByViewById, lstImprovementTactic, tacticPlanId),

                    });
                }

                //// Prepare stage list for Marketting accrodian for Home screen only.
                if (activemenu.Equals(Enums.ActiveMenu.Home))
                {
                    lstCustomFields = (lstTactic.Select(tactic => tactic.objPlanTactic.Stage)).Select(tactic => new
                    {
                        CustomFieldId = tactic.StageId,
                        Title = tactic.Title,
                        ColorCode = TacticColor
                    }).ToList().Distinct().Select(tactic => new CustomFields()
                    {
                        cfId = tactic.CustomFieldId.ToString(),
                        Title = tactic.Title,
                        ColorCode = tactic.ColorCode
                    }).ToList().OrderBy(stage => stage.Title).ToList();
                }

                //// Prepare ImprovementTactic list that relates to selected tactic and stage
                lstImprovementTaskDetails = (from improvementTactic in lstImprovementTactic
                                             join tactic in lstTactic on improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals tactic.objPlanTacticProgram.Plan_Campaign.PlanId
                                             join stage in objDbMrpEntities.Stages on tactic.objPlanTactic.StageId equals stage.StageId
                                             where improvementTactic.IsDeleted == false && tactic.objPlanTactic.IsDeleted == false && stage.IsDeleted == false
                                             select new ImprovementTaskDetail()
                                             {
                                                 ImprovementTactic = improvementTactic,
                                                 MainParentId = stage.StageId.ToString(),
                                                 MinStartDate = lstImprovementTactic.Where(impTactic => impTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impTactic => impTactic.EffectiveDate).Min(),
                                             }).Distinct().ToList();
            }
            else if (viewBy.Equals(PlanGanttTypes.Status.ToString(), StringComparison.OrdinalIgnoreCase)) //Added by Komal RAwal for #1376
            {
                List<int> PlanIds = lstTactic.Select(_tac => _tac.PlanId).ToList();
                List<ProgressModel> EffectiveDateListByPlanIds = lstImprovementTactic.Where(imprvmnt => PlanIds.Contains(imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).Select(imprvmnt => new ProgressModel { PlanId = imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, EffectiveDate = imprvmnt.EffectiveDate }).ToList();

                #region " Creat Tactic-Status Mapping list"
                List<string> lstStatus = new List<string>();
                lstStatus = lstTactic.Select(tac => tac.objPlanTactic.Status).Distinct().ToList();
                TacticStageMapping objStageTac;
                List<Plan_Tactic> lstPlanTactic;
                List<int> lstProgramIds, lstCampaignIds;
                DateTime minTacticDate = new DateTime();
                DateTime minProgramDate = new DateTime();
                DateTime minCampaignDate = new DateTime();
                List<Plan_Campaign_Program> programList;
                lstStatus.ForEach(stats =>
                {
                    lstPlanTactic = new List<Plan_Tactic>();
                    programList = new List<Plan_Campaign_Program>();
                    lstPlanTactic = lstTactic.Where(tatic => tatic.objPlanTactic.Status == stats).ToList();
                    lstProgramIds = lstCampaignIds = new List<int>();
                    lstProgramIds = lstPlanTactic.Select(tac => tac.objPlanTactic.PlanProgramId).ToList();
                    programList = lstProgram.Where(prg => lstProgramIds.Contains(prg.PlanProgramId)).ToList();
                    lstCampaignIds = programList.Select(prg => prg.PlanCampaignId).ToList();
                    objStageTac = new TacticStageMapping();
                    objStageTac.Status = stats;
                    objStageTac.TacticList = lstPlanTactic;
                    minTacticDate = lstPlanTactic.Select(tac => tac.objPlanTactic.StartDate).Min();
                    minProgramDate = programList.Select(prg => prg.StartDate).Min();
                    minCampaignDate = lstCampaign.Where(campgn => lstCampaignIds.Contains(campgn.PlanCampaignId)).Select(campgn => campgn.StartDate).Min();
                    objStageTac.minDate = new[] { minTacticDate, minProgramDate, minCampaignDate }.Min();
                    lstTacticStageMap.Add(objStageTac);
                });
                #endregion

                foreach (var tacticItem in lstTactic)
                {
                    tacticstatus = tacticItem.objPlanTactic.Status;
                    tacticListByViewById = lstTacticStageMap.Where(tatic => tatic.Status == tacticstatus).Select(tatic => tatic.TacticList).FirstOrDefault();
                    tacticPlanId = tacticItem.PlanId;
                    MinStartDateForCustomField = lstTacticStageMap.Where(tatic => tatic.Status == tacticstatus).Select(tac => tac.minDate).FirstOrDefault();//GetMinStartDateStageAndBusinessUnit(lstCampaign, lstProgram, tacticListByViewById);
                    MinStartDateForPlan = GetMinStartDateForPlanOfCustomField(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById);

                    EffectiveDateList = new List<DateTime>();
                    if (EffectiveDateListByPlanIds != null && EffectiveDateListByPlanIds.Count > 0)
                    {
                        EffectiveDateList = EffectiveDateListByPlanIds.Where(_date => _date.PlanId == tacticPlanId).Select(_date => _date.EffectiveDate).ToList();
                        if (EffectiveDateList != null && EffectiveDateList.Count > 0)
                            minEffectiveDate = EffectiveDateList.Min();
                    }

                    lstTacticTaskList.Add(new TacticTaskList()
                    {

                        Tactic = tacticItem.objPlanTactic,
                        PlanTactic = tacticItem,
                        CreatedBy = tacticItem.CreatedBy,
                        CustomFieldId = tacticstatus,
                        CustomFieldTitle = tacticstatus,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tacticstatus, tacticPlanId, tacticItem.PlanCampaignId, tacticItem.objPlanTactic.PlanProgramId, tacticItem.objPlanTactic.PlanTacticId),
                        ColorCode = TacticColor,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                        EndDate = DateTime.MaxValue,
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField,
                                                                GetMaxEndDateStageAndBusinessUnit(lstCampaign, lstProgram, tacticListByViewById)),
                        CampaignProgress = GetCampaignProgressViewBy(tacticListByViewById, tacticItem.objPlanTacticCampaign, minEffectiveDate),
                        ProgramProgress = GetProgramProgressViewBy(tacticListByViewById, tacticItem.objPlanTacticProgram, minEffectiveDate),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlan),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlan,
                                                    GetMaxEndDateForPlanOfCustomFields(viewBy, tacticstatus, tacticstageId.ToString(), tacticPlanId, lstCampaign, lstProgram, tacticListByViewById)),
                                                    tacticListByViewById, lstImprovementTactic, tacticPlanId),
                    });
                }

                //// Prepare  list for Marketting accrodian for Home screen only.
                if (activemenu.Equals(Enums.ActiveMenu.Home))
                {
                    lstCustomFields = (lstTactic.Select(tactic => tactic.objPlanTactic.Status)).Select(tactic => new
                    {
                        CustomFieldId = tacticstatus,
                        Title = tacticstatus,
                        ColorCode = TacticColor
                    }).ToList().Distinct().Select(tactic => new CustomFields()
                    {
                        cfId = tactic.CustomFieldId.ToString(),
                        Title = tactic.Title,
                        ColorCode = tactic.ColorCode
                    }).ToList().OrderBy(stage => stage.Title).ToList();
                }

                //// Prepare ImprovementTactic list that relates to selected tactic and status
                lstImprovementTaskDetails = (from improvementTactic in lstImprovementTactic
                                             join tactic in lstTactic on improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals tactic.objPlanTacticProgram.Plan_Campaign.PlanId
                                             where improvementTactic.IsDeleted == false && tactic.objPlanTactic.IsDeleted == false && improvementTactic.Status == tactic.objPlanTactic.Status
                                             select new ImprovementTaskDetail()
                                             {
                                                 ImprovementTactic = improvementTactic,
                                                 MainParentId = tactic.objPlanTactic.Status,
                                                 MinStartDate = lstImprovementTactic.Where(impTactic => impTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impTactic => impTactic.EffectiveDate).Min(),
                                             }).Distinct().ToList();
            }

             //// Prepare task tactic list for CustomFields tab(ViewBy)
            else
            {
                //// Get list of tactic ids from tactic list
                //Commented By Bhavesh because it lake more time and give timeout expire error : Start Date: 30-10-2015 #1726
                List<int> entityids = lstTactic.Select(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId))).ToList();
                CustomFieldOption objCustomFieldOption = new CustomFieldOption();
                objCustomFieldOption.CustomFieldOptionId = 0;
                string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
                string customFieldType = objDbMrpEntities.CustomFields.Where(c => c.CustomFieldId == CustomTypeId).Select(c => c.CustomFieldType.Name).FirstOrDefault();
                var cusomfieldEntity = objDbMrpEntities.CustomField_Entity.Where(c => c.CustomFieldId == CustomTypeId && entityids.Contains(c.EntityId)).ToList();

                var customoptionlisttest = (from cfo in objDbMrpEntities.CustomFieldOptions
                                            where cfo.CustomFieldId == CustomTypeId && cfo.IsDeleted == false
                                            select new
                                            {
                                                CustomFieldId = cfo.CustomFieldId,
                                                CustomFieldOptionId = cfo.CustomFieldOptionId,
                                                Title = cfo.Value
                                            }).ToList();
                var lstCustomFieldTactic = (from customfieldentity in cusomfieldEntity
                                            //join tactic in lstTactic on customfieldentity.EntityId equals tactic.objPlanTactic.PlanTacticId   //Commenetd by Rahul Shah on 17/11/2015 for PL #1760. Bcz in this condition Campaign and Program data not displyed.
                                            join tactic in lstTactic on customfieldentity.EntityId equals (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId)) //Added by Rahul Shah on 17/11/2015 for PL #1760. It Will Check Campaign And Program Field data 
                                            select new
                                            {
                                                tactic = tactic,
                                                masterCustomFieldId = CustomTypeId,
                                                customFieldId = customFieldType == DropDownList ? (customoptionlisttest.Count() == 0 ? 0 : Convert.ToInt32(customfieldentity.Value)) : customfieldentity.CustomFieldEntityId,
                                                customFieldTitle = customFieldType == DropDownList ? customoptionlisttest.Where(c => c.CustomFieldOptionId.ToString() == customfieldentity.Value).Select(c => c.Title).FirstOrDefault() : customfieldentity.Value,
                                                customFieldTYpe = customFieldType,
                                                EntityId = customfieldentity.EntityId,
                                                CreatedBy = customfieldentity.CreatedBy
                                            }).ToList().Where(fltr => (fltr.customFieldTYpe == DropDownList ? (!(lstCustomFieldFilter.Where(custmlst => custmlst.CustomFieldId.Equals(fltr.masterCustomFieldId)).Any()) || lstCustomFieldFilter.Where(custmlst => custmlst.CustomFieldId.Equals(fltr.masterCustomFieldId)).Select(custmlst => custmlst.OptionId).Contains(fltr.customFieldId.ToString())) : true)).Distinct().ToList();
                //Commented By Bhavesh because it lake more time and give timeout expire error : End Date: 30-10-2015 #1726

                //Commented By Bhavesh because it lake more time and give timeout expire error Date: 30-10-2015 #1726
                //// Fetch list of tactic for selected CustomField whether CustomFieldType is textBox or Dropdown
                //var lstCustomFieldTactic = (from customfield in objDbMrpEntities.CustomFields
                //                            join customfieldtype in objDbMrpEntities.CustomFieldTypes on customfield.CustomFieldTypeId equals customfieldtype.CustomFieldTypeId
                //                            join customfieldentity in objDbMrpEntities.CustomField_Entity on customfield.CustomFieldId equals customfieldentity.CustomFieldId
                //                            join tactic in objDbMrpEntities.Plan_Campaign_Program_Tactic on customfieldentity.EntityId equals
                //                                (IsCampaign ? tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? tactic.PlanProgramId : tactic.PlanTacticId))
                //                            join customfieldoptionLeftJoin in objDbMrpEntities.CustomFieldOptions on new { Key1 = customfield.CustomFieldId, Key2 = customfieldentity.Value.Trim() } equals
                //                                new { Key1 = customfieldoptionLeftJoin.CustomFieldId, Key2 = SqlFunctions.StringConvert((double)customfieldoptionLeftJoin.CustomFieldOptionId).Trim() } into cAll
                //                            from cfo in cAll.DefaultIfEmpty()
                //                            where customfield.IsDeleted == false && tactic.IsDeleted == false && customfield.EntityType == entityType && customfield.CustomFieldId == CustomTypeId &&
                //                            customfield.ClientId == Sessions.User.ClientId && tacticIdList.Contains(tactic.PlanTacticId) && cfo.IsDeleted == false
                //                            select new
                //                            {
                //                                tactic = tactic,
                //                                masterCustomFieldId = customfield.CustomFieldId,
                //                                customFieldId = customfieldtype.Name == DropDownList ? (cfo.CustomFieldOptionId == null ? 0 : cfo.CustomFieldOptionId) : customfieldentity.CustomFieldEntityId,
                //                                customFieldTitle = customfieldtype.Name == DropDownList ? cfo.Value : customfieldentity.Value,
                //                                customFieldTYpe = customfieldtype.Name,
                //                                EntityId = customfieldentity.EntityId,
                //                                CreatedBy = customfieldentity.CreatedBy
                //                            }).ToList().Where(fltr => (fltr.customFieldTYpe == DropDownList ? (!(lstCustomFieldFilter.Where(custmlst => custmlst.CustomFieldId.Equals(fltr.masterCustomFieldId)).Any()) || lstCustomFieldFilter.Where(custmlst => custmlst.CustomFieldId.Equals(fltr.masterCustomFieldId)).Select(custmlst => custmlst.OptionId).Contains(fltr.customFieldId.ToString())) : true)).Distinct().ToList();

                //// Process CustomFieldTactic list retrieved from DB for further use
                var lstProcessedCustomFieldTactics = lstCustomFieldTactic.Select(customFieldTactic => new
                {
                    tactic = customFieldTactic.tactic,
                    CreatedBy = customFieldTactic.CreatedBy,
                    masterCustomFieldId = customFieldTactic.masterCustomFieldId,
                    customFieldId = lstCustomFieldTactic.Where(customTactic => customTactic.masterCustomFieldId == customFieldTactic.masterCustomFieldId && customTactic.customFieldTitle.Trim() == customFieldTactic.customFieldTitle.Trim()).Select(customTactic => customTactic.customFieldId).First(),
                    customFieldTitle = customFieldTactic.customFieldTitle,
                    customFieldTacticList = customFieldTactic.customFieldTYpe == DropDownList ? lstCustomFieldTactic.Where(customTactic => customTactic.customFieldId == customFieldTactic.customFieldId).Select(tt => tt.EntityId) : lstCustomFieldTactic.Where(customTactic => customTactic.customFieldTitle == customFieldTactic.customFieldTitle).Select(customTactic => customTactic.EntityId),
                }).ToList();

                TacticStageMapping objStageTac;
                var lstUniqueCustomField = lstProcessedCustomFieldTactics.Select(c => c.customFieldId).Distinct().ToList();
                if (IsCampaign)
                {
                    lstUniqueCustomField.ForEach(cstmId =>
                    {
                        objStageTac = new TacticStageMapping();
                        objStageTac.CustomFieldId = cstmId;
                        objStageTac.TacticList = lstTactic.Where(tactic => lstProcessedCustomFieldTactics.Where(lc => lc.customFieldId == cstmId).Select(lc => lc.customFieldTacticList).FirstOrDefault().Contains(tactic.PlanCampaignId)).Select(tactic => tactic).ToList();
                        lstTacticStageMap.Add(objStageTac);
                    });
                }
                else if (IsProgram)
                {
                    lstUniqueCustomField.ForEach(cstmId =>
                    {
                        objStageTac = new TacticStageMapping();
                        objStageTac.CustomFieldId = cstmId;
                        objStageTac.TacticList = lstTactic.Where(tactic => lstProcessedCustomFieldTactics.Where(lc => lc.customFieldId == cstmId).Select(lc => lc.customFieldTacticList).FirstOrDefault().Contains(tactic.objPlanTactic.PlanProgramId)).Select(tactic => tactic).ToList();
                        lstTacticStageMap.Add(objStageTac);
                    });
                }
                else
                {
                    lstUniqueCustomField.ForEach(cstmId =>
                    {
                        objStageTac = new TacticStageMapping();
                        objStageTac.CustomFieldId = cstmId;
                        objStageTac.TacticList = lstTactic.Where(tactic => lstProcessedCustomFieldTactics.Where(lc => lc.customFieldId == cstmId).Select(lc => lc.customFieldTacticList).FirstOrDefault().Contains(tactic.objPlanTactic.PlanTacticId)).Select(tactic => tactic).ToList();
                        lstTacticStageMap.Add(objStageTac);
                    });
                }

                DateTime MaxEndDateForCustomField;
                int _PlanTacticId = 0, _PlanProgramId = 0, _TypeId = 0;
                List<Plan_Tactic> fltrTactic, fltrplantacic;
                //// Prepare tactic task list for CustomField to be used in rendering of gantt chart. 
                List<int> PlanIds = lstProcessedCustomFieldTactics.Select(_tac => _tac.tactic.PlanId).ToList();
                List<ProgressModel> EffectiveDateListByPlanIds = lstImprovementTactic.Where(imprvmnt => PlanIds.Contains(imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).Select(imprvmnt => new ProgressModel { PlanId = imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, EffectiveDate = imprvmnt.EffectiveDate }).ToList();
                DateTime maxDateCampaign, maxDateProgram, maxDateTactic, minDateCampaign, minDateProgram, minDateTactic;
                System.Diagnostics.Trace.WriteLine("Start 1: " + DateTime.Now.Minute.ToString() + " : " + DateTime.Now.Second.ToString() + " : " + DateTime.Now.Millisecond.ToString());
                foreach (var tacticItem in lstProcessedCustomFieldTactics)
                {
                    tacticPlanId = tacticItem.tactic.PlanId;
                    tacticPlanCampaignId = tacticItem.tactic.PlanCampaignId;
                    _PlanTacticId = tacticItem.tactic.objPlanTactic.PlanTacticId;
                    _PlanProgramId = tacticItem.tactic.objPlanTactic.PlanProgramId;
                    _TypeId = IsCampaign ? tacticPlanCampaignId : (IsProgram ? _PlanProgramId : _PlanTacticId);

                    tacticListByViewById = lstTacticStageMap.Where(c => c.CustomFieldId == tacticItem.customFieldId).Select(c => c.TacticList).FirstOrDefault();//lstTactic.Where(tactic => tacticItem.customFieldTacticList.Contains(IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId))).Select(tactic => tactic).ToList();

                    #region "Get CampaignIds,ProgramIds & Tactic list for Min & Max Date"
                    fltrTactic = new List<Plan_Tactic>();
                    fltrTactic = tacticListByViewById.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId)) == _TypeId).ToList();

                    #endregion
                    #region "Get Tactic,Program & Campaign MaxDate"
                    maxDateTactic = fltrTactic.Select(tactic => tactic.objPlanTactic.EndDate).Max();
                    maxDateProgram = fltrTactic.Select(tactic => tactic.objPlanTacticProgram.EndDate).Max();
                    maxDateCampaign = fltrTactic.Select(tactic => tactic.objPlanTacticCampaign.EndDate).Max();
                    MaxEndDateForCustomField = new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
                    #endregion
                    #region "Get Tactic,Program & Campaign MinDate"
                    minDateTactic = new DateTime();
                    minDateTactic = fltrTactic.Select(tactic => tactic.objPlanTactic.StartDate).Min();
                    minDateProgram = fltrTactic.Select(tactic => tactic.objPlanTacticProgram.StartDate).Min();
                    minDateCampaign = fltrTactic.Select(tactic => tactic.objPlanTacticCampaign.StartDate).Min();
                    MinStartDateForCustomField = new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
                    #endregion

                    #region "Get Min Start Date of Plan"
                    fltrplantacic = tacticListByViewById.Where(tactic => tactic.PlanId == tacticPlanId).Select(tactic => tactic).ToList();
                    minDateTactic = minDateProgram = minDateCampaign = new DateTime();
                    minDateTactic = fltrplantacic.Select(tactic => tactic.objPlanTactic.StartDate).Min();
                    minDateProgram = fltrplantacic.Select(tactic => tactic.objPlanTacticProgram.StartDate).Min();
                    minDateCampaign = fltrplantacic.Select(tactic => tactic.objPlanTacticCampaign.StartDate).Min();
                    MinStartDateForPlan = new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
                    #endregion

                    #region "Get Max End Date of Plan"
                    maxDateTactic = minDateProgram = minDateCampaign = new DateTime();
                    maxDateTactic = fltrplantacic.Select(tactic => tactic.objPlanTactic.EndDate).Max();
                    maxDateProgram = fltrplantacic.Select(tactic => tactic.objPlanTacticProgram.EndDate).Max();
                    maxDateCampaign = fltrplantacic.Select(tactic => tactic.objPlanTacticCampaign.EndDate).Max();
                    MaxEndDateForPlan = new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
                    #endregion

                    EffectiveDateList = new List<DateTime>();
                    if (EffectiveDateListByPlanIds != null && EffectiveDateListByPlanIds.Count > 0)
                    {
                        EffectiveDateList = EffectiveDateListByPlanIds.Where(_date => _date.PlanId == tacticPlanId).Select(_date => _date.EffectiveDate).ToList();
                        if (EffectiveDateList != null && EffectiveDateList.Count > 0)
                            minEffectiveDate = EffectiveDateList.Min();
                    }
                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = tacticItem.tactic.objPlanTactic,
                        PlanTactic = tacticItem.tactic,
                        CustomFieldId = tacticItem.customFieldId.ToString(),
                        CustomFieldTitle = tacticItem.customFieldTitle,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tacticItem.customFieldId, tacticPlanId, tacticPlanCampaignId, _PlanProgramId, _PlanTacticId),
                        ColorCode = TacticColor,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                        EndDate = Common.GetEndDateAsPerCalendarInDateFormat(CalendarEndDate, MaxEndDateForCustomField),
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField, MaxEndDateForCustomField),
                        CampaignProgress = GetCampaignProgressViewBy(tacticListByViewById, tacticItem.tactic.objPlanTacticCampaign, minEffectiveDate),
                        ProgramProgress = GetProgramProgressViewBy(tacticListByViewById, tacticItem.tactic.objPlanTacticProgram, minEffectiveDate),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlan),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlan,
                                                        MaxEndDateForPlan)
                                                    , tacticListByViewById, lstImprovementTactic, tacticPlanId),
                        lstCustomEntityId = tacticItem.customFieldTacticList.ToList(),
                        CreatedBy = tacticItem.CreatedBy,
                        PlanStartDate = MinStartDateForPlan,
                        PlanEndDate = MaxEndDateForPlan
                    });
                }
                System.Diagnostics.Trace.WriteLine("End 1: " + DateTime.Now.Minute.ToString() + " : " + DateTime.Now.Second.ToString() + " : " + DateTime.Now.Millisecond.ToString());
                //// Prepare CustomFields list for Marketting accrodian for Home screen only.
                List<CustomFields> lstDistCustomFields = new List<CustomFields>();
                if (activemenu.Equals(Enums.ActiveMenu.Home))
                {
                    lstCustomFields = (lstProcessedCustomFieldTactics.Select(tactic => tactic)).Select(tactic => new
                {
                    CustomFieldId = tactic.customFieldId,
                    Title = tactic.customFieldTitle,
                    ColorCode = TacticColor
                }).ToList().Distinct().Select(tactic => new CustomFields()
                    {
                        cfId = tactic.CustomFieldId.ToString(),
                        Title = tactic.Title,
                        ColorCode = tactic.ColorCode
                    }).ToList().OrderBy(customField => customField.Title).ToList();
                }

                //// Prepare ImprovementTactic list that relates to selected tactic and customFields
                lstImprovementTaskDetails = (from improvementTactic in lstImprovementTactic
                                             join customfieldtactic in lstProcessedCustomFieldTactics on improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals customfieldtactic.tactic.PlanId
                                             where improvementTactic.IsDeleted == false
                                             select new ImprovementTaskDetail()
                                             {
                                                 ImprovementTactic = improvementTactic,
                                                 CreatedBy = improvementTactic.CreatedBy,
                                                 MainParentId = customfieldtactic.masterCustomFieldId.ToString(),
                                                 MinStartDate = lstImprovementTactic.Where(impTactic => impTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impTactic => impTactic.EffectiveDate).Min(),
                                             }).Select(improvementTactic => improvementTactic).Distinct().ToList();
            }

            //// Prepare task detail list using taskTacticList for all task to be rendered in gantt chart
            var lstTaskDetails = lstTacticTaskList.Select(tacticTask => new
            {
                MainParentId = tacticTask.CustomFieldId,
                MainParentTitle = tacticTask.CustomFieldTitle,
                StartDate = tacticTask.StartDate,
                EndDate = tacticTask.EndDate,
                Duration = tacticTask.Duration,
                Progress = 0,
                Open = false,
                Color = TacticColor,
                type = "Tactic",
                PlanId = tacticTask.PlanTactic.PlanId,
                PlanTitle = tacticTask.PlanTactic.objPlanTacticCampaignPlan.Title,
                Campaign = tacticTask.PlanTactic.objPlanTacticCampaign,
                Program = tacticTask.PlanTactic.objPlanTacticProgram,
                Tactic = tacticTask.Tactic,
                PlanTactic = tacticTask.PlanTactic,
                PlanProgress = tacticTask.PlanProgrss,
                CampaignProgress = tacticTask.CampaignProgress,
                ProgramProgress = tacticTask.ProgramProgress,
                lstCustomEntityId = tacticTask.lstCustomEntityId,
                CreatedBy = tacticTask.CreatedBy,
                PlanStartDate = tacticTask.PlanStartDate,
                PlanEndDate = tacticTask.PlanEndDate,
                Status = tacticTask.Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Status
            }).Distinct().ToList();

            #region Prepare CustomField task data
            var taskDataCustomeFields = lstTaskDetails.Select(tacticTask => new
            {
                id = string.Format("Z{0}", tacticTask.MainParentId),
                text = tacticTask.MainParentTitle,
                start_date = tacticTask.StartDate,
                end_date = tacticTask.EndDate,
                duration = tacticTask.Duration,
                progress = tacticTask.Progress,
                open = tacticTask.Open,
                color = tacticTask.Color,
                CreatedBy = tacticTask.CreatedBy

            }).Distinct().OrderBy(tacticTask => tacticTask.text);

            //// Group same task for Custom Field by CustomFieldId
            var groupedCustomField = taskDataCustomeFields.GroupBy(groupedTask => new { id = groupedTask.id }).Select(groupedTask => new
            {
                id = groupedTask.Key.id,
                text = groupedTask.Select(task => task.text).FirstOrDefault(),
                start_date = groupedTask.Select(task => task.start_date).Min(),
                end_date = groupedTask.Select(task => task.end_date).Max(),
                duration = groupedTask.Select(task => task.duration).Max(),
                progress = groupedTask.Select(task => task.progress).FirstOrDefault(),
                open = groupedTask.Select(task => task.open).FirstOrDefault(),
                colorcode = groupedTask.Select(task => task.color).FirstOrDefault(),
                color = ((groupedTask.Select(task => task.progress).FirstOrDefault() > 0) ? "stripe" : string.Empty)
            });

            //// Finalize Custom Field task data to be render in gantt chart
            var lstCustomFieldTaskData = groupedCustomField.Select(taskdata => new
            {
                id = taskdata.id,
                text = taskdata.text,
                start_date = taskdata.start_date,
                duration = taskdata.end_date == DateTime.MaxValue ? taskdata.duration : Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, Convert.ToDateTime(taskdata.start_date), taskdata.end_date),
                progress = taskdata.progress,
                open = taskdata.open,
                color = ((taskdata.progress > 0) ? "stripe" : string.Empty),
                colorcode = taskdata.colorcode
            });
            #endregion

            #region Prepare Plan task data
            var taskDataPlan = lstTaskDetails.Select(taskdata => new
            {
                id = string.Format("Z{0}_L{1}", taskdata.MainParentId, taskdata.PlanId),
                text = taskdata.PlanTitle,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, viewBy == "Status" ? GetMinStartDateForPlanOfCustomField(viewBy, taskdata.MainParentId,
                            taskdata.MainParentId, taskdata.PlanId, lstCampaign, lstProgram, lstTactic, IsCampaign, IsProgram) : taskdata.PlanStartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          viewBy == "Status" ? GetMinStartDateForPlanOfCustomField(viewBy, taskdata.MainParentId,
                                                          taskdata.MainParentId, taskdata.PlanId, lstCampaign, lstProgram,
                                                          lstTactic, IsCampaign, IsProgram) : taskdata.PlanStartDate,
                                                          viewBy == "Status" ? GetMaxEndDateForPlanOfCustomFields(viewBy, taskdata.MainParentId,
                                                          taskdata.MainParentId, taskdata.PlanId, lstCampaign, lstProgram,
                                                          lstTactic, IsCampaign, IsProgram) : taskdata.PlanEndDate),
                progress = taskdata.PlanProgress,
                open = false,
                parent = string.Format("Z{0}", taskdata.MainParentId),
                color = PlanColor,
                planid = taskdata.PlanId,
                CreatedBy = taskdata.CreatedBy,
                Status = taskdata.Status  //added by Rahul Shah on 16/12/2015 for PL #1782

            }).Select(taskdata => taskdata).Distinct().OrderBy(taskdata => taskdata.text);

            //// Finalize Plan task data to be render in gantt chart
            var lstPlanTaskdata = taskDataPlan.Select(taskdata => new
            {
                id = taskdata.id,
                text = taskdata.text,
                start_date = taskdata.start_date,
                duration = taskdata.duration,
                progress = taskdata.progress,//taskDataPlan.Where(plan => plan.id == taskdata.id).Select(plan => plan.progress).Min(),
                open = taskdata.open,
                parent = taskdata.parent,
                color = (taskdata.progress > 0 ? "stripe" : string.Empty),
                colorcode = taskdata.color,
                planid = taskdata.planid,
                type = "Plan",
                TacticType = doubledesh,
                OwnerName = GetOwnerName(taskdata.CreatedBy.ToString()),
                Permission = IsPlanCreateAllAuthorized == false ? (taskdata.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(taskdata.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized,
                Status = taskdata.Status //added by Rahul Shah on 16/12/2015 for PL #1782

            }).Distinct().ToList();
            #endregion

            #region Prepare Campaign task data
            var taskDataCampaign = lstTaskDetails.Select(taskdata => new
            {
                id = string.Format("Z{0}_L{1}_C{2}", taskdata.MainParentId, taskdata.PlanId, taskdata.Program.PlanCampaignId),
                text = taskdata.Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, taskdata.Campaign.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, taskdata.Campaign.StartDate, taskdata.Campaign.EndDate),
                progress = taskdata.CampaignProgress,
                open = false,
                parent = string.Format("Z{0}_L{1}", taskdata.MainParentId, taskdata.PlanId),
                color = CampaignColor,
                plancampaignid = taskdata.Program.PlanCampaignId,
                Status = taskdata.Campaign.Status,
                CreatedBy = taskdata.CreatedBy
            }).Select(taskdata => taskdata).Distinct().OrderBy(taskdata => taskdata.text);

            //// Finalize Campaign task data to be render in gantt chart
            var lstCampaignTaskData = taskDataCampaign.Select(taskdata => new
            {
                id = taskdata.id,
                text = taskdata.text,
                start_date = taskdata.start_date,
                duration = taskdata.duration,
                progress = taskdata.progress,
                open = taskdata.open,
                parent = taskdata.parent,
                color = taskdata.progress > 0 ? "stripe" : string.Empty,
                colorcode = taskdata.color,
                plancampaignid = taskdata.plancampaignid,
                Status = taskdata.Status,
                type = "Campaign",
                TacticType = doubledesh,
                OwnerName = GetOwnerName(taskdata.CreatedBy.ToString()),
                Permission = IsPlanCreateAllAuthorized == false ? (taskdata.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(taskdata.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
            }).Select(taskdata => taskdata).Distinct().ToList();
            #endregion

            #region Prepare Program task data
            var taskDataProgram = lstTaskDetails.Select(taskdata => new
            {
                id = string.Format("Z{0}_L{1}_C{2}_P{3}", taskdata.MainParentId, taskdata.PlanId, taskdata.Program.PlanCampaignId, taskdata.Program.PlanProgramId),
                text = taskdata.Program.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, taskdata.Program.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, taskdata.Program.StartDate, taskdata.Program.EndDate),
                progress = taskdata.ProgramProgress,
                open = false,
                parent = string.Format("Z{0}_L{1}_C{2}", taskdata.MainParentId, taskdata.PlanId, taskdata.Program.PlanCampaignId),
                color = ProgramColor,
                planprogramid = taskdata.Program.PlanProgramId,
                Status = taskdata.Program.Status,
                CreatedBy = taskdata.CreatedBy
            }).Select(taskdata => taskdata).Distinct().OrderBy(taskdata => taskdata.text);

            //// Finalize Program task data to be render in gantt chart
            var lstProgramTaskData = taskDataProgram.Select(taskdata => new
            {
                id = taskdata.id,
                text = taskdata.text,
                start_date = taskdata.start_date,
                duration = taskdata.duration,
                progress = taskdata.progress,
                open = taskdata.open,
                parent = taskdata.parent,
                color = (taskdata.progress == 1 ? " stripe stripe-no-border " : (taskdata.progress > 0 ? "partialStripe" : string.Empty)),
                colorcode = taskdata.color,
                planprogramid = taskdata.planprogramid,
                Status = taskdata.Status,
                type = "Program",
                TacticType = doubledesh,
                OwnerName = GetOwnerName(taskdata.CreatedBy.ToString()),
                Permission = IsPlanCreateAllAuthorized == false ? (taskdata.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(taskdata.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
            }).ToList().Distinct().ToList();
            #endregion

            #region Prepare Tactic task data
            List<int> _PlanIds = lstTaskDetails.Select(_task => _task.PlanId).ToList();
            List<ProgressModel> _EffectiveDateListByPlanIds = lstImprovementTactic.Where(imprvmnt => _PlanIds.Contains(imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).Select(imprvmnt => new ProgressModel { PlanId = imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, EffectiveDate = imprvmnt.EffectiveDate }).ToList();
            var taskDataTactic = lstTaskDetails.Select(taskdata => new
            {
                id = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", taskdata.MainParentId, taskdata.PlanId, taskdata.Program.PlanCampaignId, taskdata.Program.PlanProgramId, taskdata.Tactic.PlanTacticId),
                text = taskdata.Tactic.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, taskdata.Tactic.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, taskdata.Tactic.StartDate, taskdata.Tactic.EndDate),
                progress = GetTacticProgress((taskdata.Tactic.StartDate != null ? taskdata.Tactic.StartDate : new DateTime()), _EffectiveDateListByPlanIds, taskdata.PlanId),
                open = false,
                parent = string.Format("Z{0}_L{1}_C{2}_P{3}", taskdata.MainParentId, taskdata.PlanId, taskdata.Program.PlanCampaignId, taskdata.Tactic.PlanProgramId),
                color = taskdata.Color,
                plantacticid = taskdata.Tactic.PlanTacticId,
                Status = taskdata.Tactic.Status,
                TacticTypeId = taskdata.Tactic.TacticTypeId,
                CreatedBy = taskdata.CreatedBy
            }).Distinct().OrderBy(t => t.text);

            //// Finalize Tactic task data to be render in gantt chart
            var lstTacticTaskData = taskDataTactic.Select(taskdata => new
            {
                id = taskdata.id,
                text = taskdata.text,
                start_date = taskdata.start_date,
                duration = taskdata.duration,
                progress = taskdata.progress,
                open = taskdata.open,
                parent = taskdata.parent,
                color = (taskdata.progress == 1 ? " stripe" : string.Empty),
                colorcode = taskdata.color,
                plantacticid = taskdata.plantacticid,
                Status = taskdata.Status,
                type = "Tactic",
                TacticType = GettactictypeName(taskdata.TacticTypeId),
                OwnerName = GetOwnerName(taskdata.CreatedBy.ToString()),
                Permission = IsPlanCreateAllAuthorized == false ? (taskdata.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(taskdata.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
            }).Distinct().ToList();
            #endregion

            #region Prepare Improvement Activities & Tactics
            //// Prepare Improvement Activities task data to be render in gantt chart
            var lstImprovementActivityTaskData = lstImprovementTaskDetails.Select(taskdataImprovement => new
            {
                id = string.Format("Z{0}_L{1}_M{2}", taskdataImprovement.MainParentId, taskdataImprovement.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, taskdataImprovement.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId),
                text = taskdataImprovement.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, taskdataImprovement.MinStartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, taskdataImprovement.MinStartDate, CalendarEndDate) - 1,
                progress = 0,
                open = true,
                colorcode = ImprovementColor,
                type = "Imp Tactic",
                Permission = IsPlanCreateAllAuthorized == false ? (taskdataImprovement.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(taskdataImprovement.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized,
                color = string.Empty,
                ImprovementActivityId = taskdataImprovement.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
                isImprovement = true,
                IsHideDragHandleLeft = taskdataImprovement.MinStartDate < CalendarStartDate,
                IsHideDragHandleRight = true,
                parent = string.Format("Z{0}_L{1}", taskdataImprovement.MainParentId, taskdataImprovement.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
            }).Select(taskdataImprovement => taskdataImprovement).Distinct().ToList();

            string tacticStatusSubmitted = Enums.TacticStatusValues.FirstOrDefault(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            string tacticStatusDeclined = Enums.TacticStatusValues.FirstOrDefault(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;

            //// Prepare Improvent Tactics task data to be render in gantt chart
            var lstImprovementTacticTaskData = lstImprovementTaskDetails.Select(taskdataImprovement => new
            {
                id = string.Format("Z{0}_L{1}_M{2}_I{3}_Y{4}", taskdataImprovement.MainParentId, taskdataImprovement.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, taskdataImprovement.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId, taskdataImprovement.ImprovementTactic.ImprovementPlanTacticId, taskdataImprovement.ImprovementTactic.ImprovementTacticTypeId),
                text = taskdataImprovement.ImprovementTactic.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, taskdataImprovement.ImprovementTactic.EffectiveDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, taskdataImprovement.ImprovementTactic.EffectiveDate, CalendarEndDate) - 1,
                progress = 0,
                open = true,
                parent = string.Format("Z{0}_L{1}_M{2}", taskdataImprovement.MainParentId, taskdataImprovement.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, taskdataImprovement.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId),
                colorcode = ImprovementColor,
                type = "Imp Tactic",
                Permission = IsPlanCreateAllAuthorized == false ? (taskdataImprovement.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(taskdataImprovement.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized,
                color = string.Empty,
                isSubmitted = taskdataImprovement.ImprovementTactic.Status.Equals(tacticStatusSubmitted),
                isDeclined = taskdataImprovement.ImprovementTactic.Status.Equals(tacticStatusDeclined),
                inqs = 0,
                mqls = 0,
                cost = taskdataImprovement.ImprovementTactic.Cost,
                cws = 0,
                taskdataImprovement.ImprovementTactic.ImprovementPlanTacticId,
                isImprovement = true,
                IsHideDragHandleLeft = taskdataImprovement.ImprovementTactic.EffectiveDate < CalendarStartDate,
                IsHideDragHandleRight = true,
                Status = taskdataImprovement.ImprovementTactic.Status
            }).Distinct().ToList().OrderBy(t => t.text);
            #endregion

            //// Concate all the task data in form of object list
            var finalTaskData = lstCustomFieldTaskData.Concat<object>(lstPlanTaskdata).Concat<object>(lstImprovementActivityTaskData).Concat<object>(lstImprovementTacticTaskData).Concat<object>(lstCampaignTaskData).Concat<object>(lstTacticTaskData).Concat<object>(lstProgramTaskData).ToList<object>();

            //// return result data as per active menu
            if (activemenu.Equals(Enums.ActiveMenu.Home))
            {
                //// Prepare accordian data for Marketing and Improvement activities if active menu is Home
                #region Prepare tactic list for left side Accordian
                var lstCustomFieldTactics = lstTacticTaskList.Select(tactic => new
                {
                    //PlanTacticId = tactic.Tactic.PlanTacticId,
                    cfId = tactic.CustomFieldId,
                    Title = tactic.Tactic.Title,
                    TaskId = tactic.TaskId,
                }).ToList().Distinct().ToList();
                #endregion

                var jsonResult = Json(new
                {
                    customFieldTactics = lstCustomFieldTactics,
                    customFields = lstCustomFields,
                    taskData = finalTaskData,
                    requestCount = requestCount,
                    planYear = planYear,
                    improvementTacticForAccordion = improvementTacticForAccordion,
                    improvementTacticTypeForAccordion = improvementTacticTypeForAccordion,
                    ViewById = viewByListResult,
                    ViewBy = sourceViewBy
                }, JsonRequestBehavior.AllowGet);
                //Modified By Komal Rawal to solve maxlength Json problen in case of large number of tactics
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else if (activemenu.Equals(Enums.ActiveMenu.Plan))
            {
                var jsonResult = Json(new
              {
                  taskData = finalTaskData,
                  requestCount = requestCount,
                  planYear = planYear,
                  ViewById = viewByListResult,
                  ViewBy = sourceViewBy
              }, JsonRequestBehavior.AllowGet);
                //Modified By Komal Rawal to solve maxlength Json problen in case of large number of tactics
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Prepare Tactic and Request Tab data
        /// <summary>
        /// Prepare Json result for Tactic and Request tab(viewBy) to be rendered in gantt chart
        /// </summary>
        /// <param name="viewBy">viewBy type</param>
        /// <param name="activemenu">current active menu</param>
        /// <param name="lstCampaign">list of campaigns</param>
        /// <param name="lstProgram">list of programs</param>
        /// <param name="lstTactic">list of tactics</param>
        /// <param name="lstImprovementTactic">list of improvement tactics</param>
        /// <param name="requestCount">No. of tactic count for Request tab</param>
        /// <param name="planYear">Plan year</param>
        /// <param name="improvementTacticForAccordion">list of improvement tactic for accrodian(left side pane)</param>
        /// <param name="improvementTacticTypeForAccordion">list of improvement tactic type for accrodian(left side pane)</param>
        /// <param name="viewByListResult">list of viewBy dropdown options</param>
        /// <returns>Json result, list of task to be rendered in Gantt chart</returns>
        private JsonResult PrepareTacticAndRequestTabResult(List<int> filterplanId, string viewBy, bool IsFiltered, bool IsRequest, Enums.ActiveMenu activemenu, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion, List<ViewByModel> viewByListResult)
        {
            Dictionary<string, string> ColorCodelist = objDbMrpEntities.EntityTypeColors.ToDictionary(e => e.EntityType.ToLower(), e => e.ColorCode);
            List<object> tacticAndRequestTaskData = GetTaskDetailTactic(ColorCodelist, filterplanId, viewBy, IsFiltered, IsRequest, planYear, activemenu, lstCampaign.ToList(), lstProgram.ToList(), lstTactic.ToList(), lstImprovementTactic);
            //   Debug.WriteLine("Step 7.1: " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            //Added By komal Rawal for #1282

            var TacticColor = ColorCodelist[Enums.EntityType.Tactic.ToString().ToLower()];


            if (activemenu.Equals(Enums.ActiveMenu.Home))
            {
                //// Prepate tactic list for Marketting Acticities accordian
                var planCampaignProgramTactic = lstTactic.ToList().Select(tactic => new
                {
                    PlanTacticId = tactic.objPlanTactic.PlanTacticId,
                    TacticTypeId = tactic.objPlanTactic.TacticTypeId,
                    Title = tactic.objPlanTactic.Title,
                    TaskId = string.Format("L{0}_C{1}_P{2}_T{3}_Y{4}", tactic.PlanId, tactic.PlanCampaignId, tactic.objPlanTactic.PlanProgramId, tactic.objPlanTactic.PlanTacticId, tactic.objPlanTactic.TacticTypeId)
                });

                //// Prepate tactic type list for Marketting Acticities accordian
                var tacticType = (lstTactic.Select(tactic => tactic.TacticType)).Select(tactic => new
                {
                    tactic.TacticTypeId,
                    tactic.Title,
                    ColorCode = TacticColor
                }).Distinct().OrderBy(tactic => tactic.Title);

                var jsonResult = Json(new
                {
                    planCampaignProgramTactic = planCampaignProgramTactic.ToList(),
                    tacticType = tacticType.ToList(),
                    taskData = tacticAndRequestTaskData,
                    requestCount = requestCount,
                    planYear = planYear,
                    improvementTacticForAccordion = improvementTacticForAccordion,
                    improvementTacticTypeForAccordion = improvementTacticTypeForAccordion,
                    ViewById = viewByListResult,
                    ViewBy = viewBy
                }, JsonRequestBehavior.AllowGet);
                //Modified By Komal Rawal to solve maxlength Json problen in case of large number of tactics
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else if (activemenu.Equals(Enums.ActiveMenu.Plan))
            {
                var jsonResult = Json(new
               {
                   taskData = tacticAndRequestTaskData,
                   requestCount = requestCount,
                   planYear = planYear,
                   ViewById = viewByListResult,
                   ViewBy = viewBy
               }, JsonRequestBehavior.AllowGet);
                //Modified By Komal Rawal to solve maxlength Json problen in case of large number of tactics
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Prepare ViewBy List for Dropdown
        /// <summary>
        /// Added By : Sohel Pathan
        /// Added Date : 28/10/2014
        /// Description : Prepare a list for ViewBy Dropdown.
        /// </summary>
        /// <param name="getViewByList">flag that indicates whether to prepare new ViewBy list based on tactic list or not.</param>
        /// <param name="tacticForAllTabs">List of all tactic of selected plan</param>
        /// <returns>List of ViewBy options</returns>
        private List<ViewByModel> prepareViewByList(bool getViewByList, List<Plan_Tactic> tacticForAllTabs)
        {
            List<ViewByModel> lstViewById = new List<ViewByModel>();
            if (getViewByList)
            {
                lstViewById = Common.GetDefaultGanttTypes(tacticForAllTabs.ToList());
            }
            return lstViewById;
        }
        #endregion

        /// <summary>
        /// //Function to get current plan edit permission, PL #538
        /// Added by Dharmraj
        /// Modified by Dharmraj on 2-Sep-2014
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        public JsonResult GetCurrentPlanPermissionDetail(int planId)
        {
            //// To get permission status for Plan Edit , By dharmraj PL #519
            //// Get all subordinates of current user upto n level
            bool IsPlanEditable = false;
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
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
            var objPlan = objDbMrpEntities.Plans.FirstOrDefault(p => p.PlanId == planId);

            if (objPlan != null)
            {
                if (objPlan.CreatedBy.Equals(Sessions.User.UserId)) // Added by Dharmraj for #712 Edit Own and Subordinate Plan
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditAllAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditSubordinatesAuthorized)
                {
                    if (lstOwnAndSubOrdinates.Contains(objPlan.CreatedBy))    // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic
                    {
                        IsPlanEditable = true;
                    }
                }
            }

            return Json(new { IsPlanEditable = IsPlanEditable }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added By Sohel #866
        /// Modified By Dharmraj PL Ticket#364 & Ticket#365 & Ticket#366
        /// Function to get status as per tab.
        /// </summary>
        /// <param name="currentTab">Current Tab(selected viewBy option)</param>
        /// <param name="objactivemenu">current/selected active menu</param>
        /// <returns>Returns list of status as per tab</returns>
        private List<string> GetStatusAsPerSelectedType(string currentTab, Enums.ActiveMenu objactivemenu)
        {
            List<string> status = new List<string>();

            if (currentTab.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                status.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            }
            else
            {
                status = Common.GetStatusListAfterApproved();
                if (objactivemenu.Equals(Enums.ActiveMenu.Plan))
                {
                    status.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
                }
            }

            return status;
        }

        /// <summary>
        /// Function to get improvement tactic type for accordion.
        /// Added By Maninder Singh Wadhva PL Ticket#47
        /// </summary>
        /// <param name="improvementTactics">Improvement Tactic of current plan</param>
        /// <returns>Returns list of improvement tactic type for accordion</returns>
        private object GetImprovementTacticTypeForAccordion(List<Plan_Improvement_Campaign_Program_Tactic> improvementTactics)
        {
            //// Getting distinct improvement tactic type for left accordion.
            var improvementTacticType = improvementTactics.Select(improveTacticType => new
            {
                improveTacticType.ImprovementTacticTypeId,
                improveTacticType.ImprovementTacticType.Title,
                improveTacticType.ImprovementTacticType.ColorCode
            }).Distinct().OrderBy(improveTacticType => improveTacticType.Title);

            return improvementTacticType;
        }

        /// <summary>
        /// Function to get improvement tactic for accordion.
        /// </summary>
        /// <param name="improvementTactics">list of Improvement tactics of current plan</param>
        /// <returns>Return list of improvement tactics for accordion in object form</returns>
        private object GetImprovementTacticForAccordion(List<Plan_Improvement_Campaign_Program_Tactic> improvementTactics)
        {
            //// Modified By: Maninder Singh Wadhva to address Ticket 395
            int improvementPlanCampaignId = 0;
            if (improvementTactics.Any())
            {
                improvementPlanCampaignId = improvementTactics[0].Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId;
            }

            //// Getting plan improvement tactic for left accordion.
            var improvementPlanTactic = improvementTactics.Select(improvementTactic => new
            {
                improvementTactic.ImprovementPlanTacticId,
                improvementTactic.ImprovementTacticTypeId,
                improvementTactic.Title,
                TaskId = string.Format("L{0}_M{1}_I{2}_Y{3}", improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, improvementPlanCampaignId, improvementTactic.ImprovementPlanTacticId, improvementTactic.ImprovementTacticTypeId)
            });

            return improvementPlanTactic;
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/18/2013
        /// Function to get GANTT chart task detail for Tactic and Request tab(viewBy).
        /// Modfied By: Maninder Singh Wadhva.
        /// Reason: To show task whose either start or end date or both date are outside current view.
        /// </summary>
        /// <param name="viewBy">viewBy option for gantt chart</param>
        /// <param name="lstCampaign">list of campaigns</param>
        /// <param name="lstProgram">list of programs</param>
        /// <param name="lstTactic">list of tactics</param>
        /// <param name="lstImprovementTactic">list of imporvementTactics</param>
        /// <returns>Returns object list of tasks for GANNT CHART</returns>
        public List<object> GetTaskDetailTactic(Dictionary<string, string> ColorCodelist, List<int> filterplanId, string viewBy, bool IsFiltered, bool IsRequest, string planYear, Enums.ActiveMenu activemenu, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic)
        {

            DateTime StartDate;
            DateTime EndDate;
            StartDate = new DateTime(Convert.ToInt32(planYear), 1, 1);
            EndDate = new DateTime(Convert.ToInt32(planYear) + 1, 1, 1).AddTicks(-1);

            string tacticStatusSubmitted = Enums.TacticStatusValues.FirstOrDefault(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            string tacticStatusDeclined = Enums.TacticStatusValues.FirstOrDefault(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;
            //Added By komal Rawal for #1282
            // Dictionary<string, string> ColorCodelist = objDbMrpEntities.EntityTypeColors.ToDictionary(e => e.EntityType.ToLower(), e => e.ColorCode);
            var PlanColor = ColorCodelist[Enums.EntityType.Plan.ToString().ToLower()];
            var ProgramColor = ColorCodelist[Enums.EntityType.Program.ToString().ToLower()];
            var TacticColor = ColorCodelist[Enums.EntityType.Tactic.ToString().ToLower()];
            var CampaignColor = ColorCodelist[Enums.EntityType.Campaign.ToString().ToLower()];
            var ImprovementTacticColor = ColorCodelist[Enums.EntityType.ImprovementTactic.ToString().ToLower()];
            //Emd
            string doubledesh = "--";
            //Added BY Ravindra Singh Sisodiya, Get Subordinates Ids List #1433
            List<Guid> lstSubordinatesIds = new List<Guid>();
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
            }

            var IsPlanCreateAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);

            //// Added BY Bhavesh, Calculate MQL at runtime #376
            List<TacticStageValue> tacticStageRelationList = new List<TacticStageValue>();
            List<Stage> stageList = new List<Stage>();
            int inqLevel = 0;
            int mqlLevel = 0;
            string strRequestPlanGanttTypes = PlanGanttTypes.Request.ToString();
            if (viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase))
            {
                string inqstage = Enums.Stage.INQ.ToString();
                string mqlstage = Enums.Stage.MQL.ToString();
                tacticStageRelationList = Common.GetTacticStageRelation(lstTactic.Select(tactic => tactic.objPlanTactic).ToList(), false);
                stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId && stage.IsDeleted == false).ToList();
                inqLevel = Convert.ToInt32(stageList.FirstOrDefault(stage => stage.Code == inqstage).Level);
                mqlLevel = Convert.ToInt32(stageList.FirstOrDefault(stage => stage.Code == mqlstage).Level);
            }

            // Add By Nishant Sheth
            // DESC:: For get default filter view after user log out #1750
            //var Label = Enums.FilterLabel.Plan.ToString();
            //var SetOfPlanSelected = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.FilterName != Label && view.Userid == Sessions.User.UserId && view.ViewName == null).Select(View => View.FilterValues).ToList();

            if (IsRequest || IsFiltered) //When clicked on request tab data will be displayed in bottom up approach else top-down for ViewBy Tactic
            {

                #region Prepare Plan Task Data
                //// Prepare task data plan list for gantt chart

                var planall = lstTactic.Select(tactic => tactic.objPlanTacticCampaignPlan).Distinct().ToList();

                var taskDataPlan = planall.Select(objplan => new
            {
                id = string.Format("L{0}", objplan.PlanId),
                text = objplan.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Tactic, objplan.PlanId, lstCampaign, lstProgram, lstTactic)),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForPlan(GanttTabs.Tactic, objplan.PlanId, lstCampaign, lstProgram, lstTactic),
                                                          GetMaxEndDateForPlan(GanttTabs.Tactic, objplan.PlanId, lstCampaign, lstProgram, lstTactic)),
                progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Tactic, objplan.PlanId, lstCampaign, lstProgram, lstTactic)),
                                                Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForPlan(GanttTabs.Tactic, objplan.PlanId, lstCampaign, lstProgram, lstTactic),
                                                          GetMaxEndDateForPlan(GanttTabs.Tactic, objplan.PlanId, lstCampaign, lstProgram, lstTactic)),
                                               lstTactic, lstImprovementTactic, objplan.PlanId),
                open = false,
                color = PlanColor,
                planid = objplan.PlanId,
                CreatedBy = objplan.CreatedBy,
                Status = objplan.Status
            }).Select(objplan => objplan).OrderBy(objplan => objplan.text);

                //// Finalize task data plan list for gantt chart
                var newTaskDataPlan = taskDataPlan.Select(plan => new
                {
                    id = plan.id,
                    text = plan.text,
                    start_date = plan.start_date,
                    duration = plan.duration,
                    progress = plan.progress,
                    open = plan.open,
                    color = (plan.progress > 0 ? "stripe" : string.Empty),
                    colorcode = plan.color,
                    planid = plan.planid,
                    type = "Plan",
                    TacticType = doubledesh,
                    Status = plan.Status,
                    OwnerName = GetOwnerName(plan.CreatedBy.ToString()),
                    Permission = IsPlanCreateAllAuthorized == false ? (plan.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(plan.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });
                #endregion

                //// Get list of plan Ids
                var planIdList = lstTactic.Select(tactic => tactic.PlanId).ToList().Distinct();


                #region Prepare Plan data for Improvement
                //// Prepate task data improvement Tacic list for gantt chart
                var taskDataPlanForImprovement = lstImprovementTactic.Where(improvementTactic => !planIdList.Contains(improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).Select(improvementTactic => new
                {
                    id = string.Format("L{0}", improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                    text = improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.None, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic)),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                              GetMinStartDateForPlan(GanttTabs.None, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic),
                                                              GetMaxEndDateForPlan(GanttTabs.None, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic)),
                    progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.None, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic)),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                              GetMinStartDateForPlan(GanttTabs.None, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic),
                                                              GetMaxEndDateForPlan(GanttTabs.None, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic)),
                                                   lstTactic, lstImprovementTactic, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                    open = false,
                    color = PlanColor,
                    planid = improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId,
                    CreatedBy = improvementTactic.CreatedBy

                }).Select(improvementTactic => improvementTactic).Distinct().OrderBy(improvementTactic => improvementTactic.text);

                //// Finalize task data Improvement Tactic list for gantt chart
                var newTaskDataPlanForImprovement = taskDataPlanForImprovement.Select(improvementTactic => new
                {
                    id = improvementTactic.id,
                    text = improvementTactic.text,
                    start_date = improvementTactic.start_date,
                    duration = improvementTactic.duration,
                    progress = improvementTactic.progress,
                    open = improvementTactic.open,
                    color = (improvementTactic.progress > 0 ? "stripe" : string.Empty),
                    colorcode = improvementTactic.color,
                    planid = improvementTactic.planid,
                    type = "Plan",
                    Permission = IsPlanCreateAllAuthorized == false ? (improvementTactic.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(improvementTactic.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });
                #endregion

                //// Concat list of Plan task objects and Improvement Tactic task objects
                var taskDataPlanMerged = newTaskDataPlan.Concat<object>(newTaskDataPlanForImprovement).ToList().Distinct();

                #region Improvement Activities & Tactics
                //// Prepare list of Improvement Tactic and Activities list for gantt chart
                var improvemntTacticList = lstImprovementTactic.Select(improvementTactic => new
                {
                    ImprovementTactic = improvementTactic,
                    CreatedBy = improvementTactic.CreatedBy,
                    //// Get start date for improvement activity task.
                    minStartDate = lstImprovementTactic.Where(impTactic => impTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impTactic => impTactic.EffectiveDate).Min(),
                }).Select(improvementTactic => improvementTactic).ToList().Distinct().ToList();

                //// Prepare task data Improvement Activities list for gantt chart
                var taskDataImprovementActivity = improvemntTacticList.Select(improvementTactic => new
                {
                    id = string.Format("L{0}_M{1}", improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId),
                    text = improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, improvementTactic.minStartDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, improvementTactic.minStartDate, CalendarEndDate) - 1,
                    progress = 0,
                    open = true,
                    color = string.Empty,
                    colorcode = ImprovementTacticColor,
                    ImprovementActivityId = improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
                    isImprovement = true,
                    IsHideDragHandleLeft = improvementTactic.minStartDate < CalendarStartDate,
                    IsHideDragHandleRight = true,
                    parent = string.Format("L{0}", improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                    type = "Imp Tactic",
                    Permission = IsPlanCreateAllAuthorized == false ? (improvementTactic.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(improvementTactic.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized

                }).Select(improvementTactic => improvementTactic).Distinct().ToList();

                //// Finalize task data Improvement Activities list for gantt chart
                var taskDataImprovementTactic = improvemntTacticList.Select(improvementTacticActivty => new
                {
                    id = string.Format("L{0}_M{1}_I{2}_Y{3}", improvementTacticActivty.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, improvementTacticActivty.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId, improvementTacticActivty.ImprovementTactic.ImprovementPlanTacticId, improvementTacticActivty.ImprovementTactic.ImprovementTacticTypeId),
                    text = improvementTacticActivty.ImprovementTactic.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, improvementTacticActivty.ImprovementTactic.EffectiveDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, improvementTacticActivty.ImprovementTactic.EffectiveDate, CalendarEndDate) - 1,
                    progress = 0,
                    open = true,
                    parent = string.Format("L{0}_M{1}", improvementTacticActivty.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, improvementTacticActivty.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId),
                    color = string.Empty,
                    colorcode = ImprovementTacticColor,
                    isSubmitted = improvementTacticActivty.ImprovementTactic.Status.Equals(tacticStatusSubmitted),
                    isDeclined = improvementTacticActivty.ImprovementTactic.Status.Equals(tacticStatusDeclined),
                    inqs = 0,
                    mqls = 0,
                    cost = improvementTacticActivty.ImprovementTactic.Cost,
                    cws = 0,
                    improvementTacticActivty.ImprovementTactic.ImprovementPlanTacticId,
                    isImprovement = true,
                    IsHideDragHandleLeft = improvementTacticActivty.ImprovementTactic.EffectiveDate < CalendarStartDate,
                    IsHideDragHandleRight = true,
                    Status = improvementTacticActivty.ImprovementTactic.Status,
                    type = "Imp Tactic",
                    Permission = IsPlanCreateAllAuthorized == false ? (improvementTacticActivty.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(improvementTacticActivty.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized

                }).OrderBy(improvementTacticActivty => improvementTacticActivty.text);
                #endregion

                taskDataPlanMerged = taskDataPlanMerged.Concat<object>(taskDataImprovementActivity).Concat<object>(taskDataImprovementTactic);



                #region Prepare Tactic Task Data
                List<int> PlanIds = lstTactic.Select(_tac => _tac.PlanId).ToList();
                List<ProgressModel> EffectiveDateListByPlanIds = lstImprovementTactic.Where(imprvmnt => PlanIds.Contains(imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).Select(imprvmnt => new ProgressModel { PlanId = imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, EffectiveDate = imprvmnt.EffectiveDate }).ToList();
                //// Prepare task data tactic list for gantt chart
                var taskDataTactic = lstTactic.Select(tactic => new
                {
                    id = string.Format("L{0}_C{1}_P{2}_T{3}_Y{4}", tactic.PlanId, tactic.PlanCampaignId, tactic.objPlanTactic.PlanProgramId, tactic.objPlanTactic.PlanTacticId, tactic.objPlanTactic.TacticTypeId),
                    text = tactic.objPlanTactic.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.objPlanTactic.StartDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, tactic.objPlanTactic.StartDate, tactic.objPlanTactic.EndDate),
                    progress = GetTacticProgress((tactic.objPlanTactic.StartDate != null ? tactic.objPlanTactic.StartDate : new DateTime()), EffectiveDateListByPlanIds, tactic.PlanId),
                    open = false,
                    parent = string.Format("L{0}_C{1}_P{2}", tactic.PlanId, tactic.PlanCampaignId, tactic.objPlanTactic.PlanProgramId),
                    color = TacticColor,
                    isSubmitted = tactic.objPlanTactic.Status == tacticStatusSubmitted,
                    isDeclined = tactic.objPlanTactic.Status == tacticStatusDeclined,
                    projectedStageValue = viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase) ? stageList.FirstOrDefault(s => s.StageId == tactic.objPlanTactic.StageId).Level <= inqLevel ? Convert.ToString(tacticStageRelationList.FirstOrDefault(tm => tm.TacticObj.PlanTacticId == tactic.objPlanTactic.PlanTacticId).INQValue) : "N/A" : "0",
                    mqls = viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase) ? stageList.FirstOrDefault(s => s.StageId == tactic.objPlanTactic.StageId).Level <= mqlLevel ? Convert.ToString(tacticStageRelationList.FirstOrDefault(tacticStage => tacticStage.TacticObj.PlanTacticId == tactic.objPlanTactic.PlanTacticId).MQLValue) : "N/A" : "0",
                    cost = tactic.objPlanTactic.Cost,
                    cws = viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase) ? tactic.objPlanTactic.Status == tacticStatusSubmitted || tactic.objPlanTactic.Status == tacticStatusDeclined ? Math.Round(tacticStageRelationList.FirstOrDefault(tacticStage => tacticStage.TacticObj.PlanTacticId == tactic.objPlanTactic.PlanTacticId).RevenueValue, 1) : 0 : 0,
                    plantacticid = tactic.objPlanTactic.PlanTacticId,
                    Status = tactic.objPlanTactic.Status,
                    TacticTypeId = tactic.objPlanTactic.TacticTypeId,
                    CreatedBy = tactic.CreatedBy
                }).OrderBy(tactic => tactic.text);

                //// Finalize task data tactic list for gantt chart
                var NewTaskDataTactic = taskDataTactic.Select(tactic => new
                {
                    id = tactic.id,
                    text = tactic.text,
                    start_date = tactic.start_date,
                    duration = tactic.duration,
                    progress = tactic.progress,
                    open = tactic.open,
                    parent = tactic.parent,
                    color = (tactic.progress == 1 ? " stripe" : string.Empty),
                    colorcode = tactic.color,
                    isSubmitted = tactic.isSubmitted,
                    isDeclined = tactic.isDeclined,
                    projectedStageValue = tactic.projectedStageValue,
                    mqls = tactic.mqls,
                    cost = tactic.cost,
                    cws = tactic.cws,
                    plantacticid = tactic.plantacticid,
                    Status = tactic.Status,
                    type = "Tactic",
                    TacticType = GettactictypeName(tactic.TacticTypeId),
                    OwnerName = GetOwnerName(tactic.CreatedBy.ToString()),
                    Permission = IsPlanCreateAllAuthorized == false ? (tactic.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(tactic.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });
                #endregion

                #region Prepare Program Task Data
                //// Prepare task data program list for gantt chart
                var taskDataProgram = lstTactic.Select(tactic => new
                {
                    id = string.Format("L{0}_C{1}_P{2}", tactic.PlanId, tactic.PlanCampaignId, tactic.objPlanTactic.PlanProgramId),
                    text = tactic.objPlanTacticProgram.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.objPlanTacticProgram.StartDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, tactic.objPlanTacticProgram.StartDate, tactic.objPlanTacticProgram.EndDate),
                    progress = GetProgramProgress(lstTactic, tactic.objPlanTacticProgram, EffectiveDateListByPlanIds, tactic.PlanId),
                    open = false,
                    parent = string.Format("L{0}_C{1}", tactic.PlanId, tactic.PlanCampaignId),
                    color = ProgramColor,
                    planprogramid = tactic.objPlanTactic.PlanProgramId,
                    Status = tactic.objPlanTacticProgram.Status,
                    CreatedBy = tactic.CreatedBy

                }).Select(tactic => tactic).Distinct().OrderBy(tactic => tactic.text);

                //// Finalize task data program list for gantt chart
                var newTaskDataProgram = taskDataProgram.Select(program => new
                {
                    id = program.id,
                    text = program.text,
                    start_date = program.start_date,
                    duration = program.duration,
                    progress = program.progress,
                    open = program.open,
                    parent = program.parent,
                    color = (program.progress == 1 ? " stripe stripe-no-border " : (program.progress > 0 ? "partialStripe" : string.Empty)),
                    colorcode = program.color,
                    planprogramid = program.planprogramid,
                    Status = program.Status,
                    type = "Program",
                    TacticType = doubledesh,
                    OwnerName = GetOwnerName(program.CreatedBy.ToString()),
                    Permission = IsPlanCreateAllAuthorized == false ? (program.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(program.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });
                #endregion

                #region Prepare Campaign Task Data
                //// Prepare task data campaign list for gantt chart
                var taskDataCampaign = lstTactic.Select(tactic => new
                {
                    id = string.Format("L{0}_C{1}", tactic.PlanId, tactic.PlanCampaignId),
                    text = tactic.objPlanTacticCampaign.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.objPlanTacticCampaign.StartDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, tactic.objPlanTacticCampaign.StartDate, tactic.objPlanTacticCampaign.EndDate),
                    progress = GetCampaignProgress(lstTactic, tactic.objPlanTacticCampaign, EffectiveDateListByPlanIds, tactic.PlanId),
                    open = false,
                    parent = string.Format("L{0}", tactic.PlanId),
                    color = CampaignColor,
                    plancampaignid = tactic.PlanCampaignId,
                    Status = tactic.objPlanTacticCampaign.Status,
                    CreatedBy = tactic.CreatedBy
                }).Select(tactic => tactic).Distinct().OrderBy(tactic => tactic.text);

                //// Finalize task data campaign list for gantt chart
                var newTaskDataCampaign = taskDataCampaign.Select(campaign => new
                {
                    id = campaign.id,
                    text = campaign.text,
                    start_date = campaign.start_date,
                    duration = campaign.duration,
                    progress = campaign.progress,
                    open = campaign.open,
                    parent = campaign.parent,
                    color = (campaign.progress == 1 ? " stripe" : (campaign.progress > 0 ? "stripe" : string.Empty)),
                    colorcode = campaign.color,
                    plancampaignid = campaign.plancampaignid,
                    Status = campaign.Status,
                    type = "Campaign",
                    TacticType = doubledesh,
                    OwnerName = GetOwnerName(campaign.CreatedBy.ToString()),
                    Permission = IsPlanCreateAllAuthorized == false ? (campaign.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(campaign.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });
                #endregion
                taskDataPlanMerged = taskDataPlanMerged.Concat<object>(newTaskDataCampaign).Concat<object>(NewTaskDataTactic).Concat<object>(newTaskDataProgram);
                return taskDataPlanMerged.ToList<object>();

            }
            else
            {
                List<int> PlanIds = lstTactic.Select(_tac => _tac.PlanId).ToList();
                List<ProgressModel> EffectiveDateListByPlanIds = lstImprovementTactic.Where(imprvmnt => PlanIds.Contains(imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).Select(imprvmnt => new ProgressModel { PlanId = imprvmnt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, EffectiveDate = imprvmnt.EffectiveDate }).ToList();

                #region Plan
                //Modified by Komal Rawal for #1537 to get Plan according to year.
                //                List<int> campplanid = new List<int>();
                // Change by Nishant Sheth for remove double db trip.
                var planData = objDbMrpEntities.Plans.Where(plan => filterplanId.Contains(plan.PlanId) && plan.IsDeleted.Equals(false)).Select(a => a).ToList();
                var planList = planData.Where(plan => filterplanId.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && plan.Year == planYear).Select(a => a.PlanId).ToList();
                List<int> campplanid = new List<int>();
                if (planList.Count == 0)
                {
                    campplanid = objDbMrpEntities.Plan_Campaign.Where(camp => !(camp.StartDate > EndDate || camp.EndDate < StartDate) && filterplanId.Contains(camp.PlanId)).Select(a => a.PlanId).Distinct().ToList();
                }
                var taskDataPlan = planData.Where(plan => plan.IsDeleted.Equals(false)
                    && (campplanid.Count > 0 ? campplanid.Contains(plan.PlanId) : planList.Contains(plan.PlanId)))
                                                  .ToList()
                                                   .Select(plan => new
                                                   {
                                                       id = string.Format("L{0}", plan.PlanId),
                                                       text = plan.Title,
                                                       start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetStartDateForPlan(plan.PlanId, lstCampaign, plan.Year)),
                                                       duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                                                                 GetStartDateForPlan(plan.PlanId, lstCampaign, plan.Year),
                                                                                                 GetEndDateForPlan(plan.PlanId, lstCampaign, plan.Year)),
                                                       progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetStartDateForPlan(plan.PlanId, lstCampaign, plan.Year)),
                                                                                       Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                                                                GetStartDateForPlan(plan.PlanId, lstCampaign, plan.Year),
                                                                                                  GetEndDateForPlan(plan.PlanId, lstCampaign, plan.Year)),
                                                                                      lstTactic, lstImprovementTactic, plan.PlanId),
                                                       open = false,
                                                       color = PlanColor,
                                                       planid = plan.PlanId,
                                                       CreatedBy = plan.CreatedBy,
                                                       Status = plan.Status
                                                   }).Select(tactic => tactic).Distinct().OrderBy(tactic => tactic.text);

                //// Finalize task data plan list for gantt chart
                var newTaskDataPlan = taskDataPlan.Select(plan => new
                {
                    id = plan.id,
                    text = plan.text,
                    start_date = plan.start_date,
                    duration = plan.duration,
                    progress = plan.progress,
                    open = plan.open,
                    color = (plan.progress > 0 ? "stripe" : string.Empty),
                    colorcode = plan.color,
                    planid = plan.planid,
                    type = "Plan",
                    TacticType = doubledesh,
                    Status = plan.Status,
                    OwnerName = GetOwnerName(plan.CreatedBy.ToString()),
                    Permission = IsPlanCreateAllAuthorized == false ? (plan.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(plan.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });

                #endregion


                #region Improvement Activities & Tactics
                //// Prepare list of Improvement Tactic and Activities list for gantt chart
                var improvemntTacticList1 = lstImprovementTactic.Where(improvementTactic => improvementTactic.EffectiveDate > EndDate).ToList();
                var improvemntTacticList = lstImprovementTactic.Where(improvementTactic => improvementTactic.EffectiveDate <= EndDate && improvementTactic.EffectiveDate >= StartDate).Select(improvementTactic => new
                {
                    ImprovementTactic = improvementTactic,
                    CreatedBy = improvementTactic.CreatedBy,
                    //// Get start date for improvement activity task.
                    minStartDate = lstImprovementTactic.Where(impTactic => impTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impTactic => impTactic.EffectiveDate).Min(),
                }).Select(improvementTactic => improvementTactic).ToList().Distinct().ToList();

                //// Prepare task data Improvement Activities list for gantt chart
                var taskDataImprovementActivity = improvemntTacticList.Select(improvementTactic => new
                {
                    id = string.Format("L{0}_M{1}", improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId),
                    text = improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, improvementTactic.minStartDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, improvementTactic.minStartDate, CalendarEndDate) - 1,
                    progress = 0,
                    open = true,
                    color = string.Empty,
                    colorcode = ImprovementTacticColor,
                    ImprovementActivityId = improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
                    isImprovement = true,
                    IsHideDragHandleLeft = improvementTactic.minStartDate < CalendarStartDate,
                    IsHideDragHandleRight = true,
                    parent = string.Format("L{0}", improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                    type = "Imp Tactic",
                    Permission = IsPlanCreateAllAuthorized == false ? (improvementTactic.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(improvementTactic.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized

                }).Select(improvementTactic => improvementTactic).Distinct().ToList();

                //// Finalize task data Improvement Activities list for gantt chart
                var taskDataImprovementTactic = improvemntTacticList.Select(improvementTacticActivty => new
                {
                    id = string.Format("L{0}_M{1}_I{2}_Y{3}", improvementTacticActivty.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, improvementTacticActivty.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId, improvementTacticActivty.ImprovementTactic.ImprovementPlanTacticId, improvementTacticActivty.ImprovementTactic.ImprovementTacticTypeId),
                    text = improvementTacticActivty.ImprovementTactic.Title,
                    start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, improvementTacticActivty.ImprovementTactic.EffectiveDate),
                    duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, improvementTacticActivty.ImprovementTactic.EffectiveDate, CalendarEndDate) - 1,
                    progress = 0,
                    open = true,
                    parent = string.Format("L{0}_M{1}", improvementTacticActivty.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, improvementTacticActivty.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId),
                    color = string.Empty,
                    colorcode = ImprovementTacticColor,
                    isSubmitted = improvementTacticActivty.ImprovementTactic.Status.Equals(tacticStatusSubmitted),
                    isDeclined = improvementTacticActivty.ImprovementTactic.Status.Equals(tacticStatusDeclined),
                    inqs = 0,
                    mqls = 0,
                    cost = improvementTacticActivty.ImprovementTactic.Cost,
                    cws = 0,
                    improvementTacticActivty.ImprovementTactic.ImprovementPlanTacticId,
                    isImprovement = true,
                    IsHideDragHandleLeft = improvementTacticActivty.ImprovementTactic.EffectiveDate < CalendarStartDate,
                    IsHideDragHandleRight = true,
                    Status = improvementTacticActivty.ImprovementTactic.Status,
                    type = "Imp Tactic",
                    Permission = IsPlanCreateAllAuthorized == false ? (improvementTacticActivty.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(improvementTacticActivty.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized

                }).OrderBy(improvementTacticActivty => improvementTacticActivty.text);
                #endregion

                var taskDataPlanMerged = newTaskDataPlan.Concat<object>(taskDataImprovementActivity).Concat<object>(taskDataImprovementTactic);
                #region Prepare Campaign Task Data for PLan
                var taskDataCampaignforPlan = lstCampaign.Where(_campgn => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                CalendarEndDate,
                                                                                                                _campgn.StartDate,
                                                                                                                _campgn.EndDate).Equals(false))
                                                        .Select(_campgn => new
                                                        {
                                                            id = string.Format("L{0}_C{1}", _campgn.PlanId, _campgn.PlanCampaignId),
                                                            text = _campgn.Title,
                                                            start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, _campgn.StartDate),
                                                            duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                                                      CalendarEndDate,
                                                                                                      _campgn.StartDate,
                                                                                                      _campgn.EndDate),
                                                            //  progress = GetCampaignProgress( _campgn, ImprovementTacticForTaskData),//progress = 0,
                                                            progress = GetCampaignProgress(lstTactic, _campgn, EffectiveDateListByPlanIds, _campgn.PlanId),
                                                            open = false,
                                                            parent = string.Format("L{0}", _campgn.PlanId),
                                                            color = CampaignColor,
                                                            plancampaignid = _campgn.PlanCampaignId,
                                                            Status = _campgn.Status,
                                                            CreatedBy = _campgn.CreatedBy
                                                        }).Select(_campgn => _campgn).OrderBy(_campgn => _campgn.text);

                var NewtaskDataCampaignforPlan = taskDataCampaignforPlan.Select(_campgn => new
                {
                    id = _campgn.id,
                    text = _campgn.text,
                    start_date = _campgn.start_date,
                    duration = _campgn.duration,
                    progress = _campgn.progress,
                    open = _campgn.open,
                    parent = _campgn.parent,
                    color = (_campgn.progress == 1 ? " stripe" : (_campgn.progress > 0 ? "stripe" : string.Empty)),
                    colorcode = _campgn.color,
                    plancampaignid = _campgn.plancampaignid,
                    Status = _campgn.Status,
                    type = "Campaign",
                    TacticType = doubledesh,
                    OwnerName = GetOwnerName(_campgn.CreatedBy.ToString()),
                    Permission = IsPlanCreateAllAuthorized == false ? (_campgn.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(_campgn.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });

                #endregion
                #region Prepare Program Task Data for Plan
                var taskDataProgramforPlan = lstProgram.Where(prgrm => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                    CalendarEndDate,
                                                                                                                    prgrm.StartDate,
                                                                                                                    prgrm.EndDate).Equals(false))
                                                            .Select(prgrm => new
                                                            {
                                                                id = string.Format("L{0}_C{1}_P{2}", prgrm.Plan_Campaign.PlanId, prgrm.PlanCampaignId, prgrm.PlanProgramId),
                                                                text = prgrm.Title,
                                                                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, prgrm.StartDate),
                                                                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                                                          CalendarEndDate,
                                                                                                          prgrm.StartDate,
                                                                                                          prgrm.EndDate),
                                                                // progress = GetProgramProgress( prgrm, ImprovementTacticForTaskData),
                                                                progress = GetProgramProgress(lstTactic, prgrm, EffectiveDateListByPlanIds, prgrm.Plan_Campaign.PlanId),
                                                                // progress = 0,
                                                                open = false,
                                                                parent = string.Format("L{0}_C{1}", prgrm.Plan_Campaign.PlanId, prgrm.PlanCampaignId),
                                                                color = ProgramColor,
                                                                planprogramid = prgrm.PlanProgramId,
                                                                Status = prgrm.Status,
                                                                CreatedBy = prgrm.CreatedBy
                                                            }).Select(prgrm => prgrm).Distinct().OrderBy(prgrm => prgrm.text).ToList();

                var NewtaskDataProgramforPlan = taskDataProgramforPlan.Select(task => new
                {
                    id = task.id,
                    text = task.text,
                    start_date = task.start_date,
                    duration = task.duration,
                    progress = task.progress,
                    open = task.open,
                    parent = task.parent,
                    color = (task.progress == 1 ? " stripe stripe-no-border " : (task.progress > 0 ? "partialStripe" : string.Empty)),
                    colorcode = task.color,
                    planprogramid = task.planprogramid,
                    Status = task.Status,
                    type = "Program",
                    TacticType = doubledesh,
                    OwnerName = GetOwnerName(task.CreatedBy.ToString()),
                    Permission = IsPlanCreateAllAuthorized == false ? (task.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(task.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });
                #endregion

                #region Prepare Tactic data for Plan
                //// Prepare task data tactic list for gantt chart

                var taskDataTacticforPlan = lstTactic.Where(_tac => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                            CalendarEndDate,
                                                                                                                            _tac.objPlanTactic.StartDate,
                                                                                                                            _tac.objPlanTactic.EndDate).Equals(false))
                                                                    .Select(_tac => new
                                                                    {
                                                                        id = string.Format("L{0}_C{1}_P{2}_T{3}_Y{4}", _tac.PlanId, _tac.PlanCampaignId, _tac.objPlanTactic.PlanProgramId, _tac.objPlanTactic.PlanTacticId, _tac.objPlanTactic.TacticTypeId),
                                                                        text = _tac.objPlanTactic.Title,
                                                                        start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, _tac.objPlanTactic.StartDate),
                                                                        duration = Common.GetEndDateAsPerCalendar(CalendarStartDate,
                                                                                                                  CalendarEndDate,
                                                                                                                  _tac.objPlanTactic.StartDate,
                                                                                                                  _tac.objPlanTactic.EndDate),
                                                                        //   progress = GetTacticProgress( _tac, ImprovementTacticForTaskData),
                                                                        progress = GetTacticProgress((_tac.objPlanTactic.StartDate != null ? _tac.objPlanTactic.StartDate : new DateTime()), EffectiveDateListByPlanIds, _tac.PlanId),
                                                                        // progress = 0,
                                                                        open = false,
                                                                        isSubmitted = _tac.objPlanTactic.Status == tacticStatusSubmitted,
                                                                        isDeclined = _tac.objPlanTactic.Status == tacticStatusDeclined,
                                                                        projectedStageValue = viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase) ? stageList.FirstOrDefault(s => s.StageId == _tac.objPlanTactic.StageId).Level <= inqLevel ? Convert.ToString(tacticStageRelationList.FirstOrDefault(tm => tm.TacticObj.PlanTacticId == _tac.objPlanTactic.PlanTacticId).INQValue) : "N/A" : "0",
                                                                        mqls = viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase) ? stageList.FirstOrDefault(s => s.StageId == _tac.objPlanTactic.StageId).Level <= mqlLevel ? Convert.ToString(tacticStageRelationList.FirstOrDefault(tacticStage => tacticStage.TacticObj.PlanTacticId == _tac.objPlanTactic.PlanTacticId).MQLValue) : "N/A" : "0",
                                                                        cost = _tac.objPlanTactic.Cost,
                                                                        cws = viewBy.Equals(strRequestPlanGanttTypes, StringComparison.OrdinalIgnoreCase) ? _tac.objPlanTactic.Status == tacticStatusSubmitted || _tac.objPlanTactic.Status == tacticStatusDeclined ? Math.Round(tacticStageRelationList.FirstOrDefault(tacticStage => tacticStage.TacticObj.PlanTacticId == _tac.objPlanTactic.PlanTacticId).RevenueValue, 1) : 0 : 0,
                                                                        parent = string.Format("L{0}_C{1}_P{2}", _tac.PlanId, _tac.PlanCampaignId, _tac.objPlanTactic.PlanProgramId),
                                                                        color = TacticColor,
                                                                        plantacticid = _tac.objPlanTactic.PlanTacticId,
                                                                        Status = _tac.objPlanTactic.Status,
                                                                        TacticTypeId = _tac.objPlanTactic.TacticTypeId,
                                                                        CreatedBy = _tac.CreatedBy

                                                                    }).OrderBy(_tac => _tac.text).ToList();

                List<int> lstAllowedEntityIds = new List<int>();
                if (lstTactic.Count() > 0)
                {
                    lstAllowedEntityIds = lstTactic.Select(tactic => tactic.objPlanTactic.PlanTacticId).Distinct().ToList();
                }

                var NewTaskDataTacticforPlan = taskDataTacticforPlan.Where(task => !lstAllowedEntityIds.Count.Equals(0) && (lstAllowedEntityIds.Contains(task.plantacticid))).Select(task => new
                {
                    id = task.id,
                    text = task.text,
                    start_date = task.start_date,
                    duration = task.duration,
                    progress = task.progress,
                    open = task.open,
                    parent = task.parent,
                    color = (task.progress == 1 ? " stripe" : string.Empty),
                    colorcode = task.color,
                    isSubmitted = task.isSubmitted,
                    isDeclined = task.isDeclined,
                    projectedStageValue = task.projectedStageValue,
                    mqls = task.mqls,
                    cost = task.cost,
                    cws = task.cws,
                    plantacticid = task.plantacticid,
                    Status = task.Status,
                    type = "Tactic",
                    TacticType = GettactictypeName(task.TacticTypeId),
                    OwnerName = GetOwnerName(task.CreatedBy.ToString()),
                    Permission = IsPlanCreateAllAuthorized == false ? (task.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(task.CreatedBy)) ? true : false : IsPlanCreateAllAuthorized
                });
                #endregion

                taskDataPlanMerged = taskDataPlanMerged.Concat<object>(NewtaskDataCampaignforPlan).Concat<object>(NewTaskDataTacticforPlan).Concat<object>(NewtaskDataProgramforPlan);

                return taskDataPlanMerged.ToList<object>();
            }


        }


        /// <summary>
        /// Function to get tactic progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 2nd april, 2013
        /// <param name="planCampaignProgramTactic">tactic object</param>
        /// <param name="lstImprovementTactic">list of improvement tactics of selected plan</param>
        /// <param name="PlanId">planId of selected tactic</param>
        /// <returns>returns progress between 0 and 1</returns>
        public double GetTacticProgress(DateTime tacStartDate, List<ProgressModel> lstEffectiveDate, int PlanId)
        {
            double result = 0;
            List<DateTime> EffectiveDateListByPlanId = new List<DateTime>();
            if (lstEffectiveDate != null && lstEffectiveDate.Count > 0)
                EffectiveDateListByPlanId = lstEffectiveDate.Where(_date => _date.PlanId == PlanId).Select(_date => _date.EffectiveDate).ToList();

            if (EffectiveDateListByPlanId != null && EffectiveDateListByPlanId.Count > 0)
            {
                //// Minimun date of improvement tactic
                DateTime minDate = EffectiveDateListByPlanId.Min();

                //// Start Date of tactic
                DateTime tacticStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tacStartDate));

                //// If any tactic affected by at least one improvement tactic.
                if (tacticStartDate >= minDate)
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
        /// <param name="lstTactic">list of tactics of selected program</param>
        /// <param name="planCampaignProgram">program object</param>
        /// <param name="lstImprovementTactic">list of improvement tactics of selected plan</param>
        /// <param name="PlanId">planId of selected program</param>
        /// <returns>returns progress between 0 and 1</returns>
        public double GetProgramProgress(List<Plan_Tactic> lstTactic, Plan_Campaign_Program planCampaignProgram, List<ProgressModel> lstImprovementTactic, int PlanId)
        {
            double result = 0;
            List<DateTime> EffectiveDateTimeList = new List<DateTime>();
            if (lstImprovementTactic != null && lstImprovementTactic.Count > 0)
                EffectiveDateTimeList = lstImprovementTactic.Where(_date => _date.PlanId == PlanId).Select(_date => _date.EffectiveDate).ToList();

            if (EffectiveDateTimeList != null && EffectiveDateTimeList.Count > 0)
            {
                //// Minimun date of improvement tactic
                DateTime minDate = EffectiveDateTimeList.Min();

                //// Start date of program
                DateTime programStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaignProgram.StartDate));

                //// List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = lstTactic.Where(tactic => tactic.objPlanTactic.IsDeleted.Equals(false) && tactic.objPlanTactic.PlanProgramId == planCampaignProgram.PlanProgramId && (tactic.objPlanTactic.StartDate >= minDate).Equals(true)
                                                        && tactic.PlanId == PlanId)
                                                .Select(tactic => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.objPlanTactic.StartDate)) })
                                                .ToList();
                if (lstAffectedTactic.Count > 0)
                {
                    //// minimum start Date of tactics
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(tactic => tactic.startDate).Min();
                    result = GetProgressResult(tacticMinStartDate, minDate, programStartDate, planCampaignProgram.StartDate, planCampaignProgram.EndDate);
                }
            }
            return result;
        }

        /// <summary>
        /// Function to get program progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 2nd april, 2013
        /// <param name="lstTactic">list of tactics of selected program</param>
        /// <param name="planCampaignProgram">program object</param>
        /// <param name="lstImprovementTactic">list of improvement tactics of selected plan</param>
        /// <param name="PlanId">planId of selected program</param>
        /// <returns>returns progress between 0 and 1</returns>
        public double GetProgramProgressViewBy(List<Plan_Tactic> lstTactic, Plan_Campaign_Program planCampaignProgram, DateTime effectiveMinDate)
        {
            double result = 0;
            //List<DateTime> EffectiveDateTimeList = new List<DateTime>();
            //if (lstImprovementTactic != null && lstImprovementTactic.Count > 0)
            //    EffectiveDateTimeList = lstImprovementTactic.Where(_date => _date.PlanId == PlanId).Select(_date => _date.EffectiveDate).ToList();

            if (effectiveMinDate != null && effectiveMinDate != new DateTime())
            {
                //// Minimun date of improvement tactic
                DateTime minDate = effectiveMinDate;

                //// Start date of program
                DateTime programStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaignProgram.StartDate));

                //// List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = lstTactic.Where(tactic => tactic.objPlanTactic.PlanProgramId == planCampaignProgram.PlanProgramId && (tactic.objPlanTactic.StartDate >= minDate).Equals(true)
                                                        )
                                                .Select(tactic => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.objPlanTactic.StartDate)) })
                                                .ToList();
                if (lstAffectedTactic.Count > 0)
                {
                    //// minimum start Date of tactics
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(tactic => tactic.startDate).Min();
                    result = (tacticMinStartDate >= minDate) ? GetProgressResult(tacticMinStartDate, minDate, programStartDate, planCampaignProgram.StartDate, planCampaignProgram.EndDate) : 0;
                }
            }
            return result;
        }

        /// <summary>
        /// Function to get campaign progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 2nd april, 2013
        /// </summary>
        /// <param name="lstTactic">list of tactics of selected program</param>
        /// <param name="planCampaign">campaign object</param>
        /// <param name="lstImprovementTactic">list of improvement tactics of selected plan</param>
        /// <param name="PlanId">planId of selected program</param>
        /// <returns>returns progress between 0 and 1</returns>
        public double GetCampaignProgress(List<Plan_Tactic> lstTactic, Plan_Campaign planCampaign, List<ProgressModel> lstImprovementTactic, int PlanId)
        {
            double result = 0;
            List<DateTime> EffectiveDateList = new List<DateTime>();
            EffectiveDateList = lstImprovementTactic.Where(_date => _date.PlanId == PlanId).Select(_date => _date.EffectiveDate).ToList();
            if (EffectiveDateList != null && EffectiveDateList.Count > 0)
            {
                //// Minimun date of improvement tactic
                DateTime minDate = EffectiveDateList.Min();

                //// Start date of Campaign
                DateTime campaignStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaign.StartDate));

                //// List of all tactics
                var lstAllTactic = lstTactic.Where(tactic => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate, CalendarEndDate, tactic.objPlanTactic.StartDate, tactic.objPlanTactic.EndDate).Equals(false)
                                                    && tactic.PlanId == PlanId);

                //// List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = lstAllTactic.Where(tactic => (tactic.objPlanTactic.IsDeleted.Equals(false) && tactic.objPlanTactic.StartDate >= minDate).Equals(true) && tactic.objPlanTactic.Plan_Campaign_Program.PlanCampaignId == planCampaign.PlanCampaignId)
                                                     .Select(tactic => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.objPlanTactic.StartDate)) })
                                                     .ToList();

                if (lstAffectedTactic != null && lstAffectedTactic.Count > 0)
                {
                    //// minimum start Date of tactics
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(tactic => tactic.startDate).Min();
                    result = GetProgressResult(tacticMinStartDate, minDate, campaignStartDate, planCampaign.StartDate, planCampaign.EndDate);
                }
            }
            return result;
        }

        /// <summary>
        /// Function to get campaign progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 2nd april, 2013
        /// </summary>
        /// <param name="lstTactic">list of tactics of selected program</param>
        /// <param name="planCampaign">campaign object</param>
        /// <param name="lstImprovementTactic">list of improvement tactics of selected plan</param>
        /// <param name="PlanId">planId of selected program</param>
        /// <returns>returns progress between 0 and 1</returns>
        public double GetCampaignProgressViewBy(List<Plan_Tactic> lstTactic, Plan_Campaign planCampaign, DateTime effectiveMinDate)
        {
            double result = 0;
            //List<DateTime> EffectiveDateList = new List<DateTime>();
            //EffectiveDateList = lstImprovementTactic.Where(_date => _date.PlanId == PlanId).Select(_date => _date.EffectiveDate).ToList();
            if (effectiveMinDate != null && effectiveMinDate != new DateTime())
            {
                //// Minimun date of improvement tactic
                DateTime minDate = effectiveMinDate;

                //// Start date of Campaign
                DateTime campaignStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaign.StartDate));

                //// List of all tactics
                var lstAllTactic = lstTactic.Where(tactic => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate, CalendarEndDate, tactic.objPlanTactic.StartDate, tactic.objPlanTactic.EndDate).Equals(false));

                //// List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = lstAllTactic.Where(tactic => (tactic.objPlanTactic.StartDate >= minDate).Equals(true) && tactic.PlanCampaignId == planCampaign.PlanCampaignId)
                                                     .Select(tactic => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.objPlanTactic.StartDate)) })
                                                     .ToList();

                if (lstAffectedTactic != null && lstAffectedTactic.Count > 0)
                {
                    //// minimum start Date of tactics
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(tactic => tactic.startDate).Min();
                    result = (tacticMinStartDate >= minDate) ? GetProgressResult(tacticMinStartDate, minDate, campaignStartDate, planCampaign.StartDate, planCampaign.EndDate) : 0;
                }
            }
            return result;
        }


        /// <summary>
        /// Function to get progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 3rd april, 2013
        /// </summary>
        /// <param name="StartDate">start date of task</param>
        /// <param name="Duration">duration of task</param>
        /// <param name="lstTactic">list of tactics</param>
        /// <param name="lstImprovementTactic">list of improvement tactics</param>
        /// <returns>return progress between 0 and 1</returns>
        public double GetProgress(string taskStartDate, double taskDuration, List<Plan_Tactic> lstTactic, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic, int PlanId)
        {
            double result = 0;
            List<DateTime> EffectiveDateList = new List<DateTime>();
            if (lstImprovementTactic != null && lstImprovementTactic.Count > 0)
                EffectiveDateList = lstImprovementTactic.Where(improvementTactic => improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Select(improvementTactic => improvementTactic.EffectiveDate).ToList();
            if (EffectiveDateList != null && EffectiveDateList.Count > 0)
            {
                //// Minimun date of improvement tactic
                DateTime minDate = EffectiveDateList.Min();

                //// List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = lstTactic.Where(tactic => (tactic.objPlanTactic.StartDate >= minDate).Equals(true) && tactic.PlanId == PlanId)
                                                 .Select(tactic => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.objPlanTactic.StartDate)) })
                                                 .ToList();
                if (lstAffectedTactic.Count > 0)
                {
                    //// Minimum start Date of tactics
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(tactic => tactic.startDate).Min();
                    //// If any tactic affected by at least one improvement tactic
                    if (tacticMinStartDate >= minDate)
                    {
                        //// Difference between task start date and tactic minimum date
                        double daysDifference = (tacticMinStartDate - Convert.ToDateTime(taskStartDate)).TotalDays;

                        //// If no. of days are more then zero then it will return progress
                        if (daysDifference > 0)
                        {
                            result = (daysDifference / taskDuration);
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
        /// Function to get progress result.
        /// </summary>
        /// <param name="minStartDate">Minimum StartDate</param>
        /// <param name="minDate">Minimum Date</param>
        /// <param name="calendarStartDate">StartDate calculated as per Calendar</param>
        /// <param name="InspectStartDate">Campaign, Program or Tactic StartDate</param>
        /// <param name="InspectEndDate">Campaign, Program or Tactic EndDate</param>
        /// <returns>return progress between 0 and 1</returns>
        public double GetProgressResult(DateTime minStartDate, DateTime minDate, DateTime calendarStartDate, DateTime InspectStartDate, DateTime InspectEndDate)
        {
            double result = 0;
            try
            {
                //// If any tactic affected by at least one improvement tactic
                if (minStartDate >= minDate)
                {
                    double duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, InspectStartDate, InspectEndDate);

                    //// Difference between program start date and tactic minimum date
                    double daysDifference = (minStartDate - calendarStartDate).TotalDays;

                    //// If no. of days are more then zero then it will return progress
                    if (daysDifference > 0)
                    {
                        result = (daysDifference / duration);
                    }
                    else
                    {
                        result = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// Added By :- Sohel Pathan
        /// Date :- 09/10/2014
        /// Description :- Function to get minimum start date for custom field
        /// </summary>
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic and so on)</param>
        /// <param name="typeId">Selected Id as per selected GanttTab</param>
        /// <param name="lstCampaign">List of campaign</param>
        /// <param name="lstProgram">List of Program</param>
        /// <param name="lstTactic">List of Tactic</param>
        /// <param name="IsCampaign">flag to indicate campaign</param>
        /// <param name="IsProgram">flag to indicate program</param>
        /// <returns>Returns the min start date for program and Campaign</returns>
        public DateTime GetMinStartDateForCustomField(PlanGanttTypes currentGanttTab, int typeId, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic, bool IsCampaign = false, bool IsProgram = false)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime minDateTactic = DateTime.Now;

            //// Check the case with selected plan gantt type and if it's match then extract the min date from tactic list 
            switch (currentGanttTab)
            {
                //// If selected plan Gantt type is Custom at that time we will extract tactic based on Custom id
                case PlanGanttTypes.Custom:
                    queryPlanProgramId = lstTactic.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId)) == typeId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    minDateTactic = lstTactic.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId)) == typeId).Select(tactic => tactic.objPlanTactic.StartDate).Min();
                    break;
                default:
                    break;
            }

            //// Get the min start date of Program and Campaign.
            List<int> queryPlanCampaignId = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.PlanCampaignId).ToList<int>();
            DateTime minDateProgram = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.StartDate).Min();
            DateTime minDateCampaign = lstCampaign.Where(campaign => queryPlanCampaignId.Contains(campaign.PlanCampaignId)).Select(campaign => campaign.StartDate).Min();

            //// return min date among tactic program and campaign
            return new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
        }

        /// <summary>
        /// Function to get min start date using planID
        /// </summary>
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic and so on)</param>
        /// <param name="typeId">Selected Id as per selected GanttTab</param>
        /// <param name="planid">Planid of selected plan</param>
        /// <param name="lstCampaign">List of campaign </param>
        /// <param name="lstProgram">List of Program</param>
        /// <param name="lstTactic">List of Tactic</param>
        /// <returns>Return the min start date for program and Campaign</returns>
        public DateTime GetMinStartDateForPlan(GanttTabs currentGanttTab, int planId, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime minDateTactic = DateTime.Now;

            //// Check the case with selected plan gantt type and if it's match then extract the min date from tactic list 
            switch (currentGanttTab)
            {
                case GanttTabs.Tactic:
                    queryPlanProgramId = lstTactic.Where(t => t.PlanId == planId).Select(t => t.objPlanTactic.PlanProgramId).ToList<int>();
                    minDateTactic = lstTactic.Where(t => t.PlanId == planId).Select(t => t.objPlanTactic.StartDate).ToList().Min();
                    break;
                case GanttTabs.None:
                    queryPlanProgramId = lstProgram.Where(program => program.Plan_Campaign.PlanId == planId).Select(program => program.PlanProgramId).ToList<int>();
                    break;
                default:
                    break;
            }

            //// Get the min start date of Program and Campaign.
            ////Modified By : Kalpesh Sharma, #958 Change of Plan is not working (Some Cases)
            List<int> queryPlanCampaignId = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.PlanCampaignId).ToList<int>();
            DateTime minDateProgram = queryPlanProgramId.Count > 0 ? lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.StartDate).Min() : DateTime.MinValue;
            DateTime minDateCampaign = queryPlanCampaignId.Count() > 0 ? lstCampaign.Where(campaign => queryPlanCampaignId.Contains(campaign.PlanCampaignId)).Select(campaign => campaign.StartDate).Min() : DateTime.MinValue;

            //// return min date among tactic program and campaign
            return new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
        }

        /// <summary>
        /// Function to get min start date using planID for Custom Fields
        /// </summary>
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic and so on)</param>
        /// <param name="typeId">Selected Id as per selected GanttTab</param>
        /// <param name="planid">Planid of selected plan</param>
        /// <param name="lstCampaign">List of campaign </param>
        /// <param name="lstProgram">List of Program</param>
        /// <param name="lstTactic">List of Tactic</param>
        /// <param name="IsCampaign">flag to indicate campaign</param>
        /// <param name="IsProgram">flag to indicate program</param>
        /// <returns>Return the min start date for program and Campaign</returns>
        public DateTime GetMinStartDateForPlanOfCustomField(string currentGanttTab, string tacticstatus, string typeId, int planId, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic, bool IsCampaign = false, bool IsProgram = false)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime minDateTactic = DateTime.Now;
            PlanGanttTypes objPlanGanttTypes = (PlanGanttTypes)Enum.Parse(typeof(PlanGanttTypes), currentGanttTab, true);

            //// Check the case with selected plan gantt type and if it's match then extract the min date from tactic list
            switch (objPlanGanttTypes)
            {
                case PlanGanttTypes.Tactic:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    minDateTactic = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.StartDate).Min();
                    break;
                case PlanGanttTypes.Stage:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.objPlanTactic.StageId == Convert.ToInt32(typeId) && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    minDateTactic = lstTactic.Where(tactic => tactic.objPlanTactic.StageId == Convert.ToInt32(typeId) && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.StartDate).Min();
                    break;
                case PlanGanttTypes.Status:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.objPlanTactic.Status == tacticstatus && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    minDateTactic = lstTactic.Where(tactic => tactic.objPlanTactic.Status == tacticstatus && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.StartDate).Min();
                    break;
                case PlanGanttTypes.Custom:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    minDateTactic = lstTactic.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId)) == Convert.ToInt32(typeId) && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.StartDate).Min();
                    break;
                default:
                    break;
            }

            //// Get the min start date of Program and Campaign.
            List<int> queryPlanCampaignId = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.PlanCampaignId).ToList<int>();
            DateTime minDateProgram = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.StartDate).Min();
            DateTime minDateCampaign = lstCampaign.Where(campaign => queryPlanCampaignId.Contains(campaign.PlanCampaignId)).Select(campaign => campaign.StartDate).Min();

            //// return min date among tactic program and campaign
            return new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
        }

        /// <summary>
        /// Added By :- Sohel Pathan
        /// Date :- 09/10/2014
        /// Description :- Function to get maximum end date for custom field
        /// </summary>
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic and so on)</param>
        /// <param name="typeId">Selected Id as per selected GanttTab</param>
        /// <param name="planid">Planid of selected plan</param>
        /// <param name="lstCampaign">List of campaign </param>
        /// <param name="lstProgram">List of Program</param>
        /// <param name="lstTactic">List of Tactic</param>
        /// <param name="IsCampaign">flag to indicate campaign</param>
        /// <param name="IsProgram">flag to indicate program</param>
        /// <returns>Return the max end date for program and Campaign</returns>
        public DateTime GetMaxEndDateForCustomField(PlanGanttTypes currentGanttTab, int typeId, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic, bool IsCampaign = false, bool IsProgram = false)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime maxDateTactic = DateTime.Now;

            //// Check the case with selected plan gantt type and if it's match then extract the max date from tactic list
            switch (currentGanttTab)
            {
                case PlanGanttTypes.Custom:
                    queryPlanProgramId = lstTactic.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId)) == typeId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    maxDateTactic = lstTactic.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId)) == typeId).Select(tactic => tactic.objPlanTactic.EndDate).Max();
                    break;
                default:
                    break;
            }

            //// Get the max end start date of Program and Campaign.
            List<int> queryPlanCampaignId = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.PlanCampaignId).ToList<int>();
            DateTime maxDateProgram = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.EndDate).Max();
            DateTime maxDateCampaign = lstCampaign.Where(campaign => queryPlanCampaignId.Contains(campaign.PlanCampaignId)).Select(campaign => campaign.EndDate).Max();

            //// return max date among tactic program and campaign
            return new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
        }

        /// <summary>
        /// Function to get max end date using planID
        /// </summary>
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic and so on)</param>
        /// <param name="typeId">Selected Id as per selected GanttTab</param>
        /// <param name="planid">Planid of selected plan</param>
        /// <param name="lstCampaign">List of campaign </param>
        /// <param name="lstProgram">List of Program</param>
        /// <param name="lstTactic">List of Tactic</param>
        /// <returns>Return the min start date for program and Campaign</returns>
        public DateTime GetMaxEndDateForPlan(GanttTabs currentGanttTab, int planId, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime maxDateTactic = DateTime.Now;

            //// Check the case with selected plan gantt type and if it's match then extract the max date from tactic list
            switch (currentGanttTab)
            {
                case GanttTabs.Tactic:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    maxDateTactic = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.EndDate).Max();
                    break;
                case GanttTabs.None:
                    queryPlanProgramId = lstProgram.Where(program => program.Plan_Campaign.PlanId == planId).Select(program => program.PlanProgramId).ToList<int>();
                    break;
                default:
                    break;
            }

            //// Get the max end start date of Program and Campaign.
            List<int> queryPlanCampaignId = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.PlanCampaignId).ToList<int>();
            //// Modified By : Kalpesh Sharma, #958 : Change of Plan is not working (Some Cases)
            DateTime maxDateProgram = queryPlanProgramId.Count > 0 ? lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.EndDate).Max() : DateTime.MaxValue;
            DateTime maxDateCampaign = queryPlanCampaignId.Count > 0 ? lstCampaign.Where(campaign => queryPlanCampaignId.Contains(campaign.PlanCampaignId)).Select(campaign => campaign.EndDate).Max() : DateTime.MaxValue;

            //// return max date among tactic program and campaign
            return new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
        }

        /// <summary>
        /// Function to get max end date using planID for Custom Fields
        /// </summary>
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic and so on)</param>
        /// <param name="typeId">Selected Id as per selected GanttTab</param>
        /// <param name="planid">Planid of selected plan</param>
        /// <param name="lstCampaign">List of campaign </param>
        /// <param name="lstProgram">List of Program</param>
        /// <param name="lstTactic">List of Tactic</param>
        /// <param name="IsCampaign">flag to indicate campaign</param>
        /// <param name="IsProgram">flag to indicate program</param>
        /// <returns>Return the min start date for program and Campaign</returns>
        public DateTime GetMaxEndDateForPlanOfCustomFields(string currentGanttTab, string tacticstatus, string typeId, int planId, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic, bool IsCampaign = false, bool IsProgram = false)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime maxDateTactic = DateTime.Now;
            PlanGanttTypes objPlanGanttTypes = (PlanGanttTypes)Enum.Parse(typeof(PlanGanttTypes), currentGanttTab, true);

            //// Check the case with selected plan gantt type and if it's match then extract the max date from tactic list
            switch (objPlanGanttTypes)
            {
                case PlanGanttTypes.Tactic:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    maxDateTactic = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.EndDate).Max();
                    break;
                case PlanGanttTypes.Stage:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.objPlanTactic.StageId == Convert.ToInt32(typeId) && tactic.objPlanTacticProgram.Plan_Campaign.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    maxDateTactic = lstTactic.Where(tactic => tactic.objPlanTactic.StageId == Convert.ToInt32(typeId) && tactic.objPlanTacticProgram.Plan_Campaign.PlanId == planId).Select(tactic => tactic.objPlanTactic.EndDate).Max();
                    break;
                case PlanGanttTypes.Status:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.objPlanTactic.Status == tacticstatus && tactic.objPlanTacticProgram.Plan_Campaign.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    maxDateTactic = lstTactic.Where(tactic => tactic.objPlanTactic.Status == tacticstatus && tactic.objPlanTacticProgram.Plan_Campaign.PlanId == planId).Select(tactic => tactic.objPlanTactic.EndDate).Max();
                    break;
                case PlanGanttTypes.Custom:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    maxDateTactic = lstTactic.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId)) == Convert.ToInt32(typeId) && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.EndDate).Max();
                    break;
                default:
                    break;
            }

            //// Get the max end start date of Program and Campaign.s
            List<int> queryPlanCampaignId = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.PlanCampaignId).ToList<int>();
            DateTime maxDateProgram = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.EndDate).Max();
            DateTime maxDateCampaign = lstCampaign.Where(campaign => queryPlanCampaignId.Contains(campaign.PlanCampaignId)).Select(campaign => campaign.EndDate).Max();

            //// return max date among tactic program and campaign
            return new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
        }

        /// <summary>
        /// Created By : Sohel Pathan
        /// Created Date : 19/09/2014
        /// Description : Function to get upper border for plan and campaign, if plan does have improvement tactic
        /// </summary>
        /// <param name="lstImprovementTactics">list of improvement tactics of one or more plans</param>
        /// <param name="planId">plan id of selected plan</param>
        /// <returns>name of upper border class</returns>
        public string GetColorBasedOnImprovementActivity(List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactics, int planId)
        {
            if (lstImprovementTactics.Count > 0)
            {
                if (lstImprovementTactics.Select(improvementTactic => improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Contains(planId))
                {
                    return Common.COLORC6EBF3_WITH_BORDER;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/19/2013
        /// Function to get minimum date for stage and businessUnit task.
        /// </summary>
        /// <param name="lstCampaign">list od campaigns</param>
        /// <param name="lstProgram">list of programs</param>
        /// <param name="lstTactic">list of tactics</param>
        /// <returns>returns minimum date of stage or businessUnit task for GANNT CHART.</returns>
        public DateTime GetMinStartDateStageAndBusinessUnit(List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic)
        {
            //// Get list of program ids
            List<int> queryPlanProgramId = lstTactic.Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
            //// Get list of campaign ids
            List<int> queryPlanCampaignId = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.PlanCampaignId).ToList<int>();

            //// Get minimum start date from tactic list
            DateTime minDateTactic = lstTactic.Select(tactic => tactic.objPlanTactic.StartDate).Min();
            //// Get minimum start date from program list
            DateTime minDateProgram = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.StartDate).Min();
            //// Get minimum start date from campaign list
            DateTime minDateCampaign = lstCampaign.Where(campaign => queryPlanCampaignId.Contains(campaign.PlanCampaignId)).Select(campaign => campaign.StartDate).Min();

            //// return min date among tactic program and campaign
            return new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/19/2013
        /// Function to get maximum date for stage and businessUnit task.
        /// Modfied By: Maninder Singh Wadhva.
        /// Reason: To show task whose either start or end date or both date are outside current view.
        /// </summary>
        /// <param name="lstCampaign">list od campaigns</param>
        /// <param name="lstProgram">list of programs</param>
        /// <param name="lstTactic">list of tactics</param>
        /// <returns>returns maximum date for stage or businessUnit  task for GANNT CHART.</returns>
        public DateTime GetMaxEndDateStageAndBusinessUnit(List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic)
        {
            //// Get list of program ids
            List<int> queryPlanProgramId = lstTactic.Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
            //// Get list of campaign ids
            List<int> queryPlanCampaignId = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.PlanCampaignId).ToList<int>();

            //// Get maximmum end date from tactic list
            DateTime maxDateTactic = lstTactic.Select(tactic => tactic.objPlanTactic.EndDate).Max();
            //// Get maximmum end date from program list
            DateTime maxDateProgram = lstProgram.Where(program => queryPlanProgramId.Contains(program.PlanProgramId)).Select(program => program.EndDate).Max();
            //// Get maximmum end date from campaign list
            DateTime maxDateCampaign = lstCampaign.Where(campaign => queryPlanCampaignId.Contains(campaign.PlanCampaignId)).Select(campaign => campaign.EndDate).Max();

            //// return max date among tactic program and campaigns
            return new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
        }

        #endregion

        #region "Home-Zero"
        /// <summary>
        /// Action Result Method to retrieve HomeZero view
        /// </summary>
        /// <returns>returns HomeZero view</returns>
        public ActionResult Homezero()
        {
            try
            {
                string strURL = "#";
                MVCUrl defaultURL = Common.DefaultRedirectURL(Enums.ActiveMenu.Home);
                if (defaultURL != null)
                {
                    strURL = "~/" + defaultURL.controllerName + "/" + defaultURL.actionName + defaultURL.queryString;
                }
                ViewBag.defaultURL = strURL;
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

            return View();
        }
        #endregion

        #region GetNumberOfActivityPerMonthByPlanId
        /// <summary>
        /// Get Number of Activity Per Month for Activity Distribution Chart for Home/Plan header.
        /// </summary>
        /// <param name="planid">plan id of selected plan</param>
        /// <param name="strparam">Upcoming Activity dropdown selected option e.g. planyear, thisyear</param>
        /// <returns>returns Activity Chart object as jsonresult</returns>
        public JsonResult GetNumberOfActivityPerMonthByPlanId(int planid, string strparam, bool isSingle)
        {
            //// Get planyear of the selected Plan
            string planYear = objDbMrpEntities.Plans.Single(plan => plan.PlanId.Equals(planid)).Year;

            /// if strparam value null then set planYear as default value.
            if (string.IsNullOrEmpty(strparam))
                strparam = planYear;

            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;
            //// Set start and end date for calender
            Common.GetPlanGanttStartEndDate(planYear, strparam, ref CalendarStartDate, ref CalendarEndDate);

            //// Start Maninder Singh Wadhva : 11/15/2013 - Getting list of tactic for view control for plan version id.
            //// Select campaign(s) of plan whose IsDelete=false.
            var lstCampaign = objDbMrpEntities.Plan_Campaign.Where(campaign => campaign.PlanId.Equals(planid) && campaign.IsDeleted.Equals(false)).ToList()
                                                            .Where(campaign => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate, CalendarEndDate, campaign.StartDate, campaign.EndDate).Equals(false));

            //// Select campaignIds.
            List<int> lstCampaignId = lstCampaign.Select(campaign => campaign.PlanCampaignId).ToList<int>();

            //// Select program(s) of campaignIds whose IsDelete=false.
            var lstProgram = objDbMrpEntities.Plan_Campaign_Program.Where(program => lstCampaignId.Contains(program.PlanCampaignId) && program.IsDeleted.Equals(false))
                                                  .Select(program => program)
                                                  .ToList()
                                                  .Where(program => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                            CalendarEndDate,
                                                                                                            program.StartDate,
                                                                                                            program.EndDate).Equals(false));

            //// Select programIds.
            List<int> lstProgramId = lstProgram.Select(program => program.PlanProgramId).ToList<int>();

            //// Selecte tactic(s) from selected programs
            List<Plan_Campaign_Program_Tactic> objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
                                                                                                                                    lstProgramId.Contains(tactic.PlanProgramId)).ToList()
                                                                                                                                .Where(tactic =>
                                                                                                                                    //// Checking start and end date
                                                                                                                                Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                                                CalendarEndDate,
                                                                                                                                                tactic.StartDate,
                                                                                                                                                tactic.EndDate).Equals(false)).ToList();

            //// Prepare an array of month as per selected dropdown paramter
            int[] monthArray = new int[12];

            if (objPlan_Campaign_Program_Tactic != null && objPlan_Campaign_Program_Tactic.Count() > 0)
            {
                IEnumerable<string> differenceItems;
                int year = 0;
                if (strparam != null)
                {
                    int tempYear;
                    bool isNumeric = int.TryParse(strparam, out tempYear);

                    if (isNumeric)
                    {
                        year = tempYear;
                    }
                }
                DateTime startDate, endDate;
                int monthNo = 0;
                foreach (Plan_Campaign_Program_Tactic tactic in objPlan_Campaign_Program_Tactic)
                {
                    startDate = endDate = new DateTime();
                    startDate = Convert.ToDateTime(tactic.StartDate);
                    endDate = Convert.ToDateTime(tactic.EndDate);

                    if (year != 0)
                    {
                        if (startDate.Year == year)
                        {
                            if (startDate.Year == endDate.Year)
                            {
                                endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                            }
                            else
                            {
                                endDate = new DateTime(startDate.Year, 12, 31);
                            }

                            differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));

                            foreach (string objDifference in differenceItems)
                            {
                                monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                                if (monthNo == 1)
                                {
                                    monthArray[0] = monthArray[0] + 1;
                                }
                                else
                                {
                                    monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                }
                            }
                        }
                        else if (endDate.Year == year)
                        {
                            if (startDate.Year != year)
                            {
                                startDate = new DateTime(endDate.Year, 01, 01);
                                differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));
                                foreach (string objDifference in differenceItems)
                                {
                                    monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                                    if (monthNo == 1)
                                    {
                                        monthArray[0] = monthArray[0] + 1;
                                    }
                                    else
                                    {
                                        monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                    }
                                }
                            }
                        }
                    }
                    else if (strparam.Equals(Enums.UpcomingActivities.thismonth.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));

                        foreach (string objDifference in differenceItems)
                        {
                            monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                            if (monthNo == DateTime.Now.Month)
                            {
                                if (monthNo == 1)
                                {
                                    monthArray[0] = monthArray[0] + 1;
                                }
                                else
                                {
                                    monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                }
                            }
                        }
                    }
                    else if (strparam.Equals(Enums.UpcomingActivities.thisquarter.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (DateTime.Now.Month == 1 || DateTime.Now.Month == 2 || DateTime.Now.Month == 3)
                        {
                            if (startDate.Month == 1 || startDate.Month == 2 || startDate.Month == 3 || endDate.Month == 1 || endDate.Month == 2 || endDate.Month == 3)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q1), startDate, endDate, monthArray);
                            }
                        }
                        else if (DateTime.Now.Month == 4 || DateTime.Now.Month == 5 || DateTime.Now.Month == 6)
                        {
                            if (startDate.Month == 4 || startDate.Month == 5 || startDate.Month == 6 || endDate.Month == 4 || endDate.Month == 5 || endDate.Month == 6)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q2), startDate, endDate, monthArray);
                            }
                        }
                        else if (DateTime.Now.Month == 7 || DateTime.Now.Month == 8 || DateTime.Now.Month == 9)
                        {
                            if (startDate.Month == 7 || startDate.Month == 8 || startDate.Month == 9 || endDate.Month == 7 || endDate.Month == 8 || endDate.Month == 9)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q3), startDate, endDate, monthArray);
                            }
                        }
                        else if (DateTime.Now.Month == 10 || DateTime.Now.Month == 11 || DateTime.Now.Month == 12)
                        {
                            if (startDate.Month == 10 || startDate.Month == 11 || startDate.Month == 12 || endDate.Month == 10 || endDate.Month == 11 || endDate.Month == 12)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q4), startDate, endDate, monthArray);
                            }
                        }
                    }
                }
            }

            ActivityChart objActivityChart;
            string strMonthName;
            //// Prepare Activity Chart list
            List<ActivityChart> lstActivityChart = new List<ActivityChart>();
            for (int month = 0; month < monthArray.Count(); month++)
            {
                objActivityChart = new ActivityChart();
                strMonthName = string.Empty;
                strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month + 1);

                if (month == 0)
                {
                    objActivityChart.Month = strMonthName[0].ToString();
                }
                else
                {
                    if (month % 3 == 0)
                    {
                        objActivityChart.Month = strMonthName[0].ToString();
                    }
                    else
                    {
                        objActivityChart.Month = string.Empty;
                    }
                    if (month == 11)
                    {
                        objActivityChart.Month = strMonthName[0].ToString();
                    }
                }
                if (monthArray[month] == 0)
                {
                    objActivityChart.NoOfActivity = string.Empty;
                }
                else
                {
                    objActivityChart.NoOfActivity = monthArray[month].ToString();
                }

                objActivityChart.Color = Common.ActivityChartColor;
                lstActivityChart.Add(objActivityChart);
            }

            //// return Activity Chart list as Json Result object
            return Json(new { lstchart = lstActivityChart.ToList() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region GetNumberOfActivityPerMonthByMultiplePlanId
        /// <summary>
        /// Get Number of Activity Per Month for Activity Distribution Chart for MultiplePlanIds for Home/Plan header.
        /// </summary>
        /// <param name="strPlanIds">Comma separated string of Plan Ids</param>
        /// <param name="strparam">Upcoming Activity dropdown selected option e.g. planyear, thisyear</param>
        /// <returns>returns Activity Chart object as jsonresult</returns>
        public async Task<JsonResult> GetNumberOfActivityPerMonth(string planid, string strparam, bool isMultiplePlan, string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "")
        {
            List<int> filteredPlanIds = new List<int>();
            string planYear = string.Empty;
            int Planyear;
            bool isNumeric = false;
            List<int> campplanid = new List<int>();
            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;
            //// Get planyear of the selected Plan
            if (strparam.Contains("-"))
            {

                List<ActivityChart> lstActivityChartyears = new List<ActivityChart>();
                lstActivityChartyears = getmultipleyearActivityChart(strparam, planid, CustomFieldId, OwnerIds, TacticTypeids, StatusIds, isMultiplePlan);
                return Json(new { lstchart = lstActivityChartyears.ToList() }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                isNumeric = int.TryParse(strparam, out Planyear);
                if (isNumeric)
                {
                    planYear = Convert.ToString(Planyear);
                }
                else
                {
                    planYear = DateTime.Now.Year.ToString();
                }
            }
            //// Set start and end date for calender
            Common.GetPlanGanttStartEndDate(planYear, strparam, ref CalendarStartDate, ref CalendarEndDate);
            if (isMultiplePlan)
            {


                List<int> planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(plan => int.Parse(plan)).ToList();
                // Add By Nishant Sheth
                // Desc :: for multiple extend plan #1750/#1761
                var planData = objDbMrpEntities.Plans.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false)).Select(a => a).ToList();
                var planList = planData.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && plan.Year == planYear).Select(a => a.PlanId).ToList();

                if (planList.Count == 0)
                {
                    campplanid = objDbMrpEntities.Plan_Campaign.Where(camp => !(camp.StartDate > CalendarEndDate || camp.EndDate < CalendarStartDate) && planIds.Contains(camp.PlanId)).Select(a => a.PlanId).Distinct().ToList();
                }
                filteredPlanIds = planData.Where(plan => plan.IsDeleted == false &&
                    campplanid.Count > 0 ? campplanid.Contains(plan.PlanId) : planIds.Contains(plan.PlanId)).ToList().Select(plan => plan.PlanId).ToList();

            }
            else
            {
                int PlanId = !string.IsNullOrEmpty(planid) ? int.Parse(planid) : 0;
                //// Get planyear of the selected Plan
                planYear = objDbMrpEntities.Plans.FirstOrDefault(_plan => _plan.PlanId.Equals(PlanId)).Year;

                /// if strparam value null then set planYear as default value.
                if (string.IsNullOrEmpty(strparam))
                    strparam = planYear;
                isNumeric = int.TryParse(strparam, out Planyear);
                if (!string.IsNullOrEmpty(planid))
                    filteredPlanIds.Add(int.Parse(planid));
            }


            //Start Maninder Singh Wadhva : 11/15/2013 - Getting list of tactic for view control for plan version id.
            //// Select campaign(s) of plan whose IsDelete=false.
            //var lstCampaign = objDbMrpEntities.Plan_Campaign.Where(campaign => filteredPlanIds.Contains(campaign.PlanId) && campaign.IsDeleted.Equals(false)).ToList()
            //                                                .Where(campaign => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate, CalendarEndDate, campaign.StartDate, campaign.EndDate).Equals(false));

            ////// Select campaignIds.
            //List<int> lstCampaignId = lstCampaign.Select(campaign => campaign.PlanCampaignId).ToList<int>();

            //// Select program(s) of campaignIds whose IsDelete=false.
            //var lstProgram = objDbMrpEntities.Plan_Campaign_Program.Where(program => lstCampaignId.Contains(program.PlanCampaignId) && program.IsDeleted.Equals(false))
            //                                                          .Select(program => program)
            //                                                          .ToList()
            //                                                          .Where(program => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
            //                                                                                                CalendarEndDate,
            //                                                                                                program.StartDate,
            //                                                                                                program.EndDate).Equals(false));

            //// Select programIds.
            //List<int> lstProgramId = lstProgram.Select(program => program.PlanProgramId).ToList<int>();

            //// Selecte tactic(s) from selected programs
            //var objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
            //                             campplanid.Count > 0 ? campplanid.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) : filteredPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate }).ToList();

            var objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
                                                    campplanid.Count > 0 ? campplanid.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) : filteredPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate))).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate }).ToList();

            //Modified By Komal Rawal for #1447
            List<string> lstFilteredCustomFieldOptionIds = new List<string>();
            List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
            List<int> lstTacticIds = new List<int>();

            //// Owner filter criteria.
            List<Guid> filterOwner = string.IsNullOrWhiteSpace(OwnerIds) ? new List<Guid>() : OwnerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();

            //TacticType filter criteria
            List<int> filterTacticType = string.IsNullOrWhiteSpace(TacticTypeids) ? new List<int>() : TacticTypeids.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();

            //Status filter criteria
            List<string> filterStatus = string.IsNullOrWhiteSpace(StatusIds) ? new List<string>() : StatusIds.Split(',').Select(tactictype => tactictype).ToList();

            //// Custom Field Filter Criteria.
            List<string> filteredCustomFields = string.IsNullOrWhiteSpace(CustomFieldId) ? new List<string>() : CustomFieldId.Split(',').Select(customFieldId => customFieldId.ToString()).ToList();
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
                lstTacticIds = objPlan_Campaign_Program_Tactic.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(pcptobj => (filterOwner.Count.Equals(0) || filterOwner.Contains(pcptobj.CreatedBy)) &&
                                         (filterTacticType.Count.Equals(0) || filterTacticType.Contains(pcptobj.TacticTypeId)) &&
                                         (filterStatus.Count.Equals(0) || filterStatus.Contains(pcptobj.Status))).ToList();

                //// Apply Custom restriction for None type
                if (objPlan_Campaign_Program_Tactic.Count() > 0)
                {

                    if (filteredCustomFields.Count > 0)
                    {
                        lstTacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, lstTacticIds);
                        //// get Allowed Entity Ids
                        objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(tacticlist => lstTacticIds.Contains(tacticlist.PlanTacticId)).ToList();
                    }

                }
            }

            //End


            //// Prepare an array of month as per selected dropdown paramter
            int[] monthArray = new int[12];

            if (objPlan_Campaign_Program_Tactic != null && objPlan_Campaign_Program_Tactic.Count() > 0)
            {
                IEnumerable<string> differenceItems;
                int year = 0;
                if (strparam != null && isNumeric)
                {
                    year = Planyear;
                }

                int currentMonth = DateTime.Now.Month, monthNo = 0;
                DateTime startDate, endDate;
                foreach (var tactic in objPlan_Campaign_Program_Tactic)
                {
                    startDate = endDate = new DateTime();
                    startDate = Convert.ToDateTime(tactic.StartDate);
                    endDate = Convert.ToDateTime(tactic.EndDate);

                    if (year != 0)
                    {
                        if (startDate.Year == year)
                        {
                            if (startDate.Year == endDate.Year)
                            {
                                endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                            }
                            else
                            {
                                endDate = new DateTime(startDate.Year, 12, 31);
                            }

                            differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));

                            foreach (string objDifference in differenceItems)
                            {
                                monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                                if (monthNo == 1)
                                {
                                    monthArray[0] = monthArray[0] + 1;
                                }
                                else
                                {
                                    monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                }
                            }
                        }
                        else if (endDate.Year == year)
                        {
                            if (startDate.Year != year)
                            {
                                startDate = new DateTime(endDate.Year, 01, 01);
                                differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));

                                foreach (string objDifference in differenceItems)
                                {
                                    monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                                    if (monthNo == 1)
                                    {
                                        monthArray[0] = monthArray[0] + 1;
                                    }
                                    else
                                    {
                                        monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                    }
                                }
                            }
                        }
                    }
                    else if (strparam.Equals(Enums.UpcomingActivities.thismonth.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (startDate.Year == System.DateTime.Now.Year)
                        {
                        //differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));

                            differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM")); 
                            List<string> thismonthdifferenceItem = new List<string>();
                            if (differenceItems.Count() > 12)
                            {
                                thismonthdifferenceItem = differenceItems.ToList();
                                thismonthdifferenceItem.RemoveRange(12, 12);
                            }
                            else
                            {
                                thismonthdifferenceItem = differenceItems.ToList();
                            }
                           



                        foreach (string objDifference in thismonthdifferenceItem)
                            {
                                monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                                if (monthNo == DateTime.Now.Month)
                                {
                                    if (monthNo == 1)
                                    {
                                        monthArray[0] = monthArray[0] + 1;
                                    }
                                    else
                                    {
                                        monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                    }
                                }
                            }
                        }
                    }
                    else if (strparam.Equals(Enums.UpcomingActivities.thisquarter.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (startDate.Year == System.DateTime.Now.Year || endDate.Year == System.DateTime.Now.Year)
                        {
                            if (currentMonth == 1 || currentMonth == 2 || currentMonth == 3)
                            {
                                if (startDate.Month == 1 || startDate.Month == 2 || startDate.Month == 3 || endDate.Month == 1 || endDate.Month == 2 || endDate.Month == 3)
                                {
                                    monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q1), startDate, endDate, monthArray);
                                }
                            }
                            else if (currentMonth == 4 || currentMonth == 5 || currentMonth == 6)
                            {
                                if (startDate.Month == 4 || startDate.Month == 5 || startDate.Month == 6 || endDate.Month == 4 || endDate.Month == 5 || endDate.Month == 6)
                                {
                                    monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q2), startDate, endDate, monthArray);
                                }
                            }
                            else if (currentMonth == 7 || currentMonth == 8 || currentMonth == 9)
                            {
                                if (startDate.Month == 7 || startDate.Month == 8 || startDate.Month == 9 || endDate.Month == 7 || endDate.Month == 8 || endDate.Month == 9)
                                {
                                    monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q3), startDate, endDate, monthArray);
                                }
                            }
                            else if (currentMonth == 10 || currentMonth == 11 || currentMonth == 12)
                            {
                                if (startDate.Month == 10 || startDate.Month == 11 || startDate.Month == 12 || endDate.Month == 10 || endDate.Month == 11 || endDate.Month == 12)
                                {
                                    monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q4), startDate, endDate, monthArray);
                                }
                            }
                        }
                    }
                }
            }

            //// Prepare Activity Chart list
            List<ActivityChart> lstActivityChart = new List<ActivityChart>();
            for (int month = 0; month < monthArray.Count(); month++)
            {
                ActivityChart objActivityChart = new ActivityChart();
                string strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month + 1);

                if (month == 0)
                {
                    objActivityChart.Month = strMonthName[0].ToString();
                }
                else
                {
                    if (month % 3 == 0)
                    {
                        objActivityChart.Month = strMonthName[0].ToString();
                    }
                    else
                    {
                        objActivityChart.Month = string.Empty;
                    }
                    if (month == 11)
                    {
                        objActivityChart.Month = strMonthName[0].ToString();
                    }
                }
                if (monthArray[month] == 0)
                {
                    objActivityChart.NoOfActivity = string.Empty;
                }
                else
                {
                    objActivityChart.NoOfActivity = monthArray[month].ToString();
                }
                objActivityChart.Color = Common.ActivityChartColor;
                lstActivityChart.Add(objActivityChart);
            }
            await Task.Delay(1);
            //// return Activity Chart list as Json Result object
            return Json(new { lstchart = lstActivityChart.ToList() }, JsonRequestBehavior.AllowGet);
        }

        private List<ActivityChart> getmultipleyearActivityChart(string strParam, string planid, string CustomFieldId, string OwnerIds, string TacticTypeids, string StatusIds, bool isMultiplePlan)
        {
            List<int> filteredPlanIds = new List<int>();
            string planYear = string.Empty;
            int Planyear = 0;
            bool isNumeric = false;
            List<int> campplanid = new List<int>();
            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;
            List<Plan> planData = new List<Plan>();
            List<int> planIds = new List<int>();
            List<ActivityChart> lstActivityChart = new List<ActivityChart>();
            List<ActivityChart> lstActivitybothChart = new List<ActivityChart>();
            List<string> categories = new List<string>();
            if (strParam.Contains("-"))
            {
                if (isMultiplePlan)
                {
                    planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(plan => int.Parse(plan)).ToList();
                    planData = objDbMrpEntities.Plans.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false)).Select(a => a).ToList();
                }

                string[] multipleyear = strParam.Split('-');
                for (int i = 0; i < multipleyear.Count(); i++)
                {
                    isNumeric = int.TryParse(multipleyear[i], out Planyear);
                    if (isNumeric)
                    {
                        planYear = Convert.ToString(Planyear);
                    }
                    else
                    {
                        planYear = DateTime.Now.Year.ToString();
                    }
                    //// Set start and end date for calender
                    Common.GetPlanGanttStartEndDate(planYear, multipleyear[i], ref CalendarStartDate, ref CalendarEndDate);

                    if (isMultiplePlan)
                    {
                        var planList = planData.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && plan.Year == planYear).Select(a => a.PlanId).ToList();
                        if (planList.Count == 0)
                        {
                            campplanid = objDbMrpEntities.Plan_Campaign.Where(camp => !(camp.StartDate > CalendarEndDate || camp.EndDate < CalendarStartDate) && planIds.Contains(camp.PlanId)).Select(a => a.PlanId).Distinct().ToList();
                        }
                        filteredPlanIds = planData.Where(plan => plan.IsDeleted == false &&
                            campplanid.Count > 0 ? campplanid.Contains(plan.PlanId) : planIds.Contains(plan.PlanId)).ToList().Select(plan => plan.PlanId).ToList();
                    }
                    else
                    {

                        int PlanId = !string.IsNullOrEmpty(planid) ? int.Parse(planid) : 0;
                        //// Get planyear of the selected Plan
                        planYear = objDbMrpEntities.Plans.FirstOrDefault(_plan => _plan.PlanId.Equals(PlanId)).Year;

                        /// if strparam value null then set planYear as default value.
                        if (string.IsNullOrEmpty(multipleyear[i]))
                            multipleyear[i] = planYear;
                        isNumeric = int.TryParse(multipleyear[i], out Planyear);
                        if (!string.IsNullOrEmpty(planid))
                            filteredPlanIds.Add(int.Parse(planid));
                    }

                    //// Selecte tactic(s) from selected programs
                  
                       var objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
                                                     campplanid.Count > 0 ? campplanid.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) : filteredPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate))).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate }).ToList();
                   
                   

                    List<string> lstFilteredCustomFieldOptionIds = new List<string>();
                    List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                    List<int> lstTacticIds = new List<int>();

                    // Owner filter criteria.
                    List<Guid> filterOwner = string.IsNullOrWhiteSpace(OwnerIds) ? new List<Guid>() : OwnerIds.Split(',').Select(owner => Guid.Parse(owner)).ToList();

                    // TacticType filter criteria
                    List<int> filterTacticType = string.IsNullOrWhiteSpace(TacticTypeids) ? new List<int>() : TacticTypeids.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();

                    //Status filter criteria
                    List<string> filterStatus = string.IsNullOrWhiteSpace(StatusIds) ? new List<string>() : StatusIds.Split(',').Select(tactictype => tactictype).ToList();

                    // Custom Field Filter Criteria.
                    List<string> filteredCustomFields = string.IsNullOrWhiteSpace(CustomFieldId) ? new List<string>() : CustomFieldId.Split(',').Select(customFieldId => customFieldId.ToString()).ToList();
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
                        lstTacticIds = objPlan_Campaign_Program_Tactic.Select(tacticlist => tacticlist.PlanTacticId).ToList();
                        objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(pcptobj => (filterOwner.Count.Equals(0) || filterOwner.Contains(pcptobj.CreatedBy)) &&
                                                 (filterTacticType.Count.Equals(0) || filterTacticType.Contains(pcptobj.TacticTypeId)) &&
                                                 (filterStatus.Count.Equals(0) || filterStatus.Contains(pcptobj.Status))).ToList();

                        //// Apply Custom restriction for None type
                        if (objPlan_Campaign_Program_Tactic.Count() > 0)
                        {

                            if (filteredCustomFields.Count > 0)
                            {
                                lstTacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, lstTacticIds);
                                //// get Allowed Entity Ids
                                objPlan_Campaign_Program_Tactic = objPlan_Campaign_Program_Tactic.Where(tacticlist => lstTacticIds.Contains(tacticlist.PlanTacticId)).ToList();
                            }

                        }
                    }

                    //End
                    //// Prepare an array of month as per selected dropdown paramter
                    int[] monthArray = new int[12];
                    int[] monthArrayactivity = new int[12];
                    int q1 = 0;
                    int q2 = 0;
                    int q3 = 0;
                    int q4 = 0;
                    if (objPlan_Campaign_Program_Tactic != null && objPlan_Campaign_Program_Tactic.Count() > 0)
                    {
                        IEnumerable<string> differenceItems;
                        int year = 0;
                        if (multipleyear[i] != null && isNumeric)
                        {
                            year = Planyear;
                        }

                        int currentMonth = DateTime.Now.Month, monthNo = 0;
                        DateTime startDate, endDate;
                        foreach (var tactic in objPlan_Campaign_Program_Tactic)
                        {
                            startDate = endDate = new DateTime();
                            startDate = Convert.ToDateTime(tactic.StartDate);
                            endDate = Convert.ToDateTime(tactic.EndDate);

                            if (year != 0)
                            {
                                if (startDate.Year == year)
                                {
                                    if (startDate.Year == endDate.Year)
                                    {
                                        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                                    }
                                    else
                                    {
                                        endDate = new DateTime(startDate.Year, 12, 31);
                                    }

                                    differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));
                                    int q1counter = 0;
                                    int q2counter = 0;
                                    int q3counter = 0;
                                    int q4counter = 0;
                                    foreach (string objDifference in differenceItems)
                                    {
                                        monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                                        //
                                        categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
                                        string strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNo);
                                        int CurrentQuarter = (int)Math.Floor(((decimal)(monthNo) + 2) / 3);
                                        if (CurrentQuarter == 1)
                                        {
                                            if (monthNo <= 3)
                                            {
                                                if (q1counter == 0)
                                                {
                                                    monthArrayactivity[2] = q1 + 1;
                                                    q1 = monthArrayactivity[2];
                                                    q1counter++;
                                                }
                                            }
                                        }
                                        else if (CurrentQuarter == 2)
                                        {
                                            if (monthNo <= 6)
                                            {
                                                if (q2counter == 0)
                                                {
                                                    monthArrayactivity[5] = q2 + 1;
                                                    q2 = monthArrayactivity[5];
                                                    q2counter++;
                                                }
                                            }
                                        }
                                        else if (CurrentQuarter == 3)
                                        {
                                            if (monthNo <= 9)
                                            {
                                                if (q3counter == 0)
                                                {
                                                    monthArrayactivity[8] = q3 + 1;
                                                    q3 = monthArrayactivity[8];
                                                    q3counter++;
                                                }
                                            }
                                        }
                                        else if (CurrentQuarter == 4)
                                        {
                                            if (monthNo <= 12)
                                            {
                                                if (q4counter == 0)
                                                {
                                                    monthArrayactivity[11] = q4 + 1;
                                                    q4 = monthArrayactivity[11];
                                                    q4counter++;
                                                }
                                            }
                                        }
                                        //


                                        if (monthNo == 1)
                                        {
                                            monthArray[0] = monthArray[0] + 1;
                                        }
                                        else
                                        {
                                            monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                        }
                                    }
                                }
                                else if (endDate.Year == year)
                                {
                                    if (startDate.Year != year)
                                    {
                                        startDate = new DateTime(endDate.Year, 01, 01);
                                        differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));

                                        int q1counter = 0;
                                        int q2counter = 0;
                                        int q3counter = 0;
                                        int q4counter = 0;
                                        foreach (string objDifference in differenceItems)
                                        {
                                            monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                                            //
                                            categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
                                            string strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNo);
                                            int CurrentQuarter = (int)Math.Floor(((decimal)(monthNo) + 2) / 3);
                                            if (CurrentQuarter == 1)
                                            {
                                                if (monthNo <= 3)
                                                {
                                                    if (q1counter == 0)
                                                    {
                                                        monthArrayactivity[2] = q1 + 1;
                                                        q1 = monthArrayactivity[2];
                                                        q1counter++;
                                                    }
                                                }
                                            }
                                            else if (CurrentQuarter == 2)
                                            {
                                                if (monthNo <= 6)
                                                {
                                                    if (q2counter == 0)
                                                    {
                                                        monthArrayactivity[5] = q2 + 1;
                                                        q2 = monthArrayactivity[5];
                                                        q2counter++;
                                                    }
                                                }
                                            }
                                            else if (CurrentQuarter == 3)
                                            {
                                                if (monthNo <= 9)
                                                {
                                                    if (q3counter == 0)
                                                    {
                                                        monthArrayactivity[8] = q3 + 1;
                                                        q3 = monthArrayactivity[8];
                                                        q3counter++;
                                                    }
                                                }
                                            }
                                            else if (CurrentQuarter == 4)
                                            {
                                                if (monthNo <= 12)
                                                {
                                                    if (q4counter == 0)
                                                    {
                                                        monthArrayactivity[11] = q4 + 1;
                                                        q4 = monthArrayactivity[11];
                                                        q4counter++;
                                                    }
                                                }
                                            }
                                            //


                                            if (monthNo == 1)
                                            {
                                                monthArray[0] = monthArray[0] + 1;
                                            }
                                            else
                                            {
                                                monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //// Prepare Activity Chart list
                    string quater = string.Empty;
                    lstActivityChart = new List<ActivityChart>();
                    //
                    for (int month = 0; month < monthArrayactivity.Count(); month++)
                    {
                        ActivityChart objActivityChart = new ActivityChart();
                        categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
                        string strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month + 1);
                        int CurrentQuarter = (int)Math.Floor(((decimal)(month + 1) + 2) / 3);
                        if (CurrentQuarter == 1)
                        {
                            quater = categories[0];
                            if (month == 2)
                            {
                                if (monthArrayactivity[month] == 0)
                                {
                                    objActivityChart.NoOfActivity = string.Empty;
                                }
                                else
                                {
                                    objActivityChart.NoOfActivity = monthArrayactivity[month].ToString();
                                }
                            }
                            else
                            {
                                objActivityChart.NoOfActivity = string.Empty;
                            }
                        }
                        if (CurrentQuarter == 2)
                        {
                            quater = categories[1];
                            if (month == 5)
                            {
                                if (monthArrayactivity[month] == 0)
                                {
                                    objActivityChart.NoOfActivity = string.Empty;
                                }
                                else
                                {
                                    objActivityChart.NoOfActivity = monthArrayactivity[month].ToString();
                                }
                            }
                            else
                            {
                                objActivityChart.NoOfActivity = string.Empty;
                            }
                        }
                        if (CurrentQuarter == 3)
                        {
                            quater = categories[2];
                            if (month == 8)
                            {
                                if (monthArrayactivity[month] == 0)
                                {
                                    objActivityChart.NoOfActivity = string.Empty;
                                }
                                else
                                {
                                    objActivityChart.NoOfActivity = monthArrayactivity[month].ToString();
                                }
                            }
                            else
                            {
                                objActivityChart.NoOfActivity = string.Empty;
                            }
                        }
                        if (CurrentQuarter == 4)
                        {
                            quater = categories[3];
                            if (month == 11)
                            {
                                if (monthArrayactivity[month] == 0)
                                {
                                    objActivityChart.NoOfActivity = string.Empty;
                                }
                                else
                                {
                                    objActivityChart.NoOfActivity = monthArrayactivity[month].ToString();
                                }
                            }
                            else
                            {
                                objActivityChart.NoOfActivity = string.Empty;
                            }
                        }

                        if (month == 0)
                        {
                            objActivityChart.Month = quater;
                        }
                        else
                        {
                            if (month % 3 == 0)
                            {
                                objActivityChart.Month = quater;
                            }
                            else
                            {
                                objActivityChart.Month = string.Empty;
                            }
                        }
                        if (i == 1)
                        {
                            objActivityChart.Color = Common.ActivityNextYearChartColor;
                        }
                        else
                        {
                            objActivityChart.Color = Common.ActivityChartColor;
                        }

                        lstActivityChart.Add(objActivityChart);
                    }
                    lstActivitybothChart.AddRange(lstActivityChart);
                }
            }

            return lstActivitybothChart;
        }
        #endregion

        #region "Get QuarterWisegraph"
        /// <summary>
        /// Function to generate activity graph quarter wise
        /// </summary>
        /// <param name="Quarter">Quater number</param>
        /// <param name="startDateParam">start date</param>
        /// <param name="endDateParam">end date</param>
        /// <param name="monthArray">array of months</param>
        /// <returns>returns array of int for months</returns>
        public int[] GetQuarterWiseGraph(string Quarter, DateTime startDateParam, DateTime endDateParam, int[] monthArray)
        {
            IEnumerable<string> differenceItems;
            DateTime endDate = endDateParam;

            int month1 = 0;
            int month2 = 0;
            int month3 = 0;

            if (Quarter == Convert.ToString(Enums.Quarter.Q1))
            {
                month1 = 1;
                month2 = 2;
                month3 = 3;
            }
            else if (Quarter == Convert.ToString(Enums.Quarter.Q2))
            {
                month1 = 4;
                month2 = 5;
                month3 = 6;
            }
            else if (Quarter == Convert.ToString(Enums.Quarter.Q3))
            {
                month1 = 7;
                month2 = 8;
                month3 = 9;
            }
            else if (Quarter == Convert.ToString(Enums.Quarter.Q4))
            {
                month1 = 10;
                month2 = 11;
                month3 = 12;
            }

            int monthNo = 0;
            //// Prepare array of months for seelcted quarter
            if (startDateParam.Month == month1 || startDateParam.Month == month2 || startDateParam.Month == month3 || endDateParam.Month == month1 || endDateParam.Month == month2 || endDateParam.Month == month3)
            {
                differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDateParam.AddMonths(element)).TakeWhile(element => element <= endDateParam).Select(element => element.ToString("MM"));

                List<string> thismonthdifferenceItem = new List<string>();
                if (differenceItems.Count() > 12)
                {
                    thismonthdifferenceItem = differenceItems.ToList();
                    thismonthdifferenceItem.RemoveRange(12, 12);
                }
                else
                {
                    thismonthdifferenceItem = differenceItems.ToList();
                }

                foreach (string d in thismonthdifferenceItem)
                {
                    monthNo = Convert.ToInt32(d.TrimStart('0'));
                    if (monthNo == month1 || monthNo == month2 || monthNo == month3)
                    {
                        if (monthNo == 1)
                        {
                            monthArray[0] = monthArray[0] + 1;
                        }
                        else
                        {
                            monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                        }
                    }
                }

                endDateParam = endDate;
            }

            return monthArray;
        }

        #endregion

        #region "Add Actual"

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Actual Value of Tactic.
        /// </summary>
        /// <param name="status">status of tactic</param>
        /// <param name="tacticTypeId"></param>
        /// <param name="customFieldId"></param>
        /// <param name="ownerId"></param>
        /// <param name="UserId"></param>
        /// <returns>Returns Json Result.</returns>
        public JsonResult GetActualTactic(int status, string tacticTypeId, string customFieldId, string ownerId, int PlanId, string UserId = "")
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                if (!Sessions.User.UserId.Equals(Guid.Parse(UserId)))
                {
                    TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                    return Json(new { returnLoginURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            //// Start - Added by Sohel Pathan on 22/01/2015 for PL ticket #1144
            //// Get TacticTypes selected  in search filter
            List<int> filteredTacticTypeIds = string.IsNullOrWhiteSpace(tacticTypeId) ? new List<int>() : tacticTypeId.Split(',').Select(tacticType => int.Parse(tacticType)).ToList();

            //// Owner filter criteria.
            List<Guid> filteredOwner = string.IsNullOrWhiteSpace(ownerId) ? new List<Guid>() : ownerId.Split(',').Select(owner => Guid.Parse(owner)).ToList();
            //// End - Added by Sohel Pathan on 22/01/2015 for PL ticket #1144

            List<Plan_Campaign_Program_Tactic> TacticList = new List<Plan_Campaign_Program_Tactic>();
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            if (status == 0)
            {
                //// Modified By: Maninder Singh for TFS Bug#282: Extra Tactics Displaying in Add Actual Screen
                TacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(planTactic => planTactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(PlanId) &&
                                                                                tacticStatus.Contains(planTactic.Status) && planTactic.IsDeleted.Equals(false) &&
                                                                                !planTactic.Plan_Campaign_Program_Tactic_Actual.Any() &&
                                                                                (filteredTacticTypeIds.Count.Equals(0) || filteredTacticTypeIds.Contains(planTactic.TacticType.TacticTypeId)) &&
                                                                                (filteredOwner.Count.Equals(0) || filteredOwner.Contains(planTactic.CreatedBy))).ToList();
            }
            else
            {
                TacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == PlanId &&
                                                                                tacticStatus.Contains(tactic.Status) && tactic.IsDeleted == false &&
                                                                                (filteredTacticTypeIds.Count.Equals(0) || filteredTacticTypeIds.Contains(tactic.TacticType.TacticTypeId)) &&
                                                                                (filteredOwner.Count.Equals(0) || filteredOwner.Contains(tactic.CreatedBy))).ToList();
            }

            List<int> TacticIds = TacticList.Select(tactic => tactic.PlanTacticId).ToList();

            //// Apply Custom restriction for None type
            if (TacticIds.Count() > 0)
            {
                //// Custom Field Filter Criteria.
                List<string> filteredCustomFields = string.IsNullOrWhiteSpace(customFieldId) ? new List<string>() : customFieldId.Split(',').Select(customField => customField.ToString()).ToList();
                List<int> lstEntityIds = new List<int>();
                if (filteredCustomFields.Count > 0)
                {
                    List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                    filteredCustomFields.ForEach(customField =>
                    {
                        string[] splittedCustomField = customField.Split('_');
                        lstCustomFieldFilter.Add(new CustomFieldFilter { CustomFieldId = int.Parse(splittedCustomField[0]), OptionId = splittedCustomField[1] });
                    });

                    TacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, TacticIds);

                    //// get Allowed Entity Ids
                    TacticList = TacticList.Where(tactic => TacticIds.Contains(tactic.PlanTacticId)).ToList();
                }

                List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, TacticIds, false);
                TacticList = TacticList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.UserId).Select(tactic => tactic).ToList();
            }

            var lstPlanTacticActual = (from tacticActual in objDbMrpEntities.Plan_Campaign_Program_Tactic_Actual
                                       where TacticIds.Contains(tacticActual.PlanTacticId)
                                       select new { tacticActual.PlanTacticId, tacticActual.CreatedBy, tacticActual.CreatedDate }).GroupBy(tacticActual => tacticActual.PlanTacticId).Select(tacticActual => tacticActual.FirstOrDefault());

            List<Guid> userListId = new List<Guid>();
            userListId = (from tacticActual in lstPlanTacticActual select tacticActual.CreatedBy).ToList<Guid>();

            //// Added BY Bhavesh, Calculate MQL at runtime #376
            List<Plan_Tactic_Values> MQLTacticList = Common.GetMQLValueTacticList(TacticList);
            List<ProjectedRevenueClass> tacticList = Common.ProjectedRevenueCalculateList(TacticList);
            List<ProjectedRevenueClass> tacticListCW = Common.ProjectedRevenueCalculateList(TacticList, true);
            var lstModifiedTactic = TacticList.Where(tactic => tactic.ModifiedDate != null).Select(tactic => tactic).ToList();
            foreach (var tactic in lstModifiedTactic)
            {
                userListId.Add(new Guid(tactic.ModifiedBy.ToString()));
            }

            string userList = string.Join(",", userListId.Select(user => user.ToString()).ToArray());
            List<User> userName = new List<User>();

            try
            {
                objBDSUserRepository.GetMultipleTeamMemberDetails(userList, Sessions.ApplicationId);

                string TitleProjectedStageValue = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
                string TitleCW = Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString();
                string TitleMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
                string TitleRevenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
                List<Plan_Campaign_Program_Tactic_Actual> lstTacticActual = objDbMrpEntities.Plan_Campaign_Program_Tactic_Actual.Where(tacticActual => TacticIds.Contains(tacticActual.PlanTacticId)).ToList();

                //// Start - Added by Sohel Pathan on 03/02/2015 for PL ticket #1090
                bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                List<Guid> lstSubordinatesIds = new List<Guid>();
                if (IsTacticAllowForSubordinates)
                {
                    lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.UserId);
                }
                //// End - Added by Sohel Pathan on 03/02/2015 for PL ticket #1090

                List<int> lstViewEditEntities = Common.GetEditableTacticList(Sessions.User.UserId, Sessions.User.ClientId, TacticIds, true);

                // Add By Nishant Sheth 
                // Desc:: for add multiple years regarding #1765
                // To create the period of the year dynamically base on tactic period

                List<listMonthDynamic> lstMonthlyDynamic = new List<listMonthDynamic>();
                TacticList.ForEach(tactic =>
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
                    lstMonthlyDynamic.Add(new listMonthDynamic { Id = tactic.PlanTacticId, listMonthly = lstMonthlyExtended });
                });


                var tacticLineItem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => TacticIds.Contains(lineItem.PlanTacticId) && lineItem.IsDeleted == false).ToList();
                List<int> LineItemsIds = tacticLineItem.Select(lineItem => lineItem.PlanLineItemId).ToList();
                var tacticLineItemActual = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lienItemActual => LineItemsIds.Contains(lienItemActual.PlanLineItemId)).ToList();
                var tacticActuals = objDbMrpEntities.Plan_Campaign_Program_Tactic_Actual.Where(tacticActual => TacticIds.Contains(tacticActual.PlanTacticId)).ToList();

                var tacticObj = TacticList.Select(tactic => new
                {
                    id = tactic.PlanTacticId,
                    title = tactic.Title,
                    mqlProjected = MQLTacticList.Where(mqlTactic => mqlTactic.PlanTacticId == tactic.PlanTacticId).Select(mqlTactic => mqlTactic.MQL),

                    // Change by Nishant sheth
                    // Desc :: #1765 - add period condition to get value
                    mqlActual = lstTacticActual.Where(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.StageTitle == TitleMQL
                    && lstMonthlyDynamic.Where(a => a.Id == tactic.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(tacticActual.Period))
                    .Sum(tacticActual => tacticActual.Actualvalue),

                    // Change by Nishant sheth
                    // Desc :: #1765 - add period condition to get value
                    cwProjected = Math.Round(tacticListCW.Where(cwTactic => cwTactic.PlanTacticId == tactic.PlanTacticId).Select(cwTactic => cwTactic.ProjectedRevenue).FirstOrDefault(), 1),
                    cwActual = lstTacticActual.Where(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.StageTitle == TitleCW
                    && lstMonthlyDynamic.Where(a => a.Id == tactic.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(tacticActual.Period)
                    ).Sum(tacticActual => tacticActual.Actualvalue),

                    revenueProjected = Math.Round(tacticList.Where(planTactic => planTactic.PlanTacticId == tactic.PlanTacticId).Select(planTactic => planTactic.ProjectedRevenue).FirstOrDefault(), 1),

                    // Change by Nishant sheth
                    // Desc :: #1765 - add period condition to get value
                    revenueActual = lstTacticActual.Where(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.StageTitle == TitleRevenue
                    && lstMonthlyDynamic.Where(a => a.Id == tactic.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(tacticActual.Period)
                    ).Sum(tacticActual => tacticActual.Actualvalue),

                    costProjected = (tacticLineItem.Where(lineItem => lineItem.PlanTacticId == tactic.PlanTacticId)).Count() > 0 ? (tacticLineItem.Where(lineItem => lineItem.PlanTacticId == lineItem.PlanTacticId)).Sum(lineItem => lineItem.Cost) : tactic.Cost,
                    //// Get the sum of Tactic line item actuals
                    costActual = (tacticLineItem.Where(lineItem => lineItem.PlanTacticId == tactic.PlanTacticId)).Count() > 0 ?
                    (tacticLineItem.Where(lineItem => lineItem.PlanTacticId == tactic.PlanTacticId)).Select(lineItem => new
                    {
                        // Change by Nishant sheth
                        // Desc :: #1765 - add period condition to get value
                        LineItemActualCost = tacticLineItemActual.Where(lineItemActual => lineItemActual.PlanLineItemId == lineItem.PlanLineItemId
                        && lstMonthlyDynamic.Where(a => a.Id == tactic.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(lineItemActual.Period)
                        ).Sum(lineItemActual => lineItemActual.Value)
                    }).Sum(lineItemActual => lineItemActual.LineItemActualCost) : (tacticActuals.Where(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.StageTitle == Enums.InspectStageValues[Enums.InspectStage.Cost.ToString()].ToString())).Sum(tacticActual => tacticActual.Actualvalue),

                    //// First check that if tactic has a single line item at that time we need to get the cost actual data from the respective table. 

                    // Change by Nishant sheth
                    // Desc :: #1765 - add period condition to get value
                    costActualData = lstMonthlyDynamic.Where(a => a.Id == tactic.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Select(month => new
                    {
                        period = month,
                        Cost = (tacticLineItem.Where(lineItem => lineItem.PlanTacticId == tactic.PlanTacticId)).Count() > 0 ?
                        tacticLineItem.Where(lineItem => lineItem.PlanTacticId == tactic.PlanTacticId).Select(lineItem => new
                        {
                            costValue = tacticLineItemActual.Where(lineItemActual => lineItemActual.PlanLineItemId == lineItem.PlanLineItemId && lineItemActual.Period == month).Sum(lineItemActual => lineItemActual.Value)
                        }).Sum(lineItem => lineItem.costValue) :
                        tacticActuals.Where(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.Period == month && tacticActual.StageTitle == Enums.InspectStageValues[Enums.InspectStage.Cost.ToString()].ToString()).Sum(tacticActual => tacticActual.Actualvalue),
                    }),

                    roiProjected = 0,
                    roiActual = 0,
                    individualId = tactic.CreatedBy,
                    tacticTypeId = tactic.TacticTypeId,
                    ////Modified by Mitesh Vaishnav for PL ticket #743,When userId will be empty guid ,First name and last name combination will be display as Gameplan Integration Service
                    modifiedBy = Common.TacticModificationMessage(tactic.PlanTacticId, userName),

                    // Change by Nishant sheth
                    // Desc :: #1765 - add period condition to get value
                    actualData = (tacticActuals.Where(tacticActual => tacticActual.PlanTacticId.Equals(tactic.PlanTacticId)
                    && lstMonthlyDynamic.Where(a => a.Id == tactic.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(tacticActual.Period)
                    ).Select(tacticActual => tacticActual).ToList()).Select(planTactic => new
                    {
                        title = planTactic.StageTitle,
                        period = planTactic.Period,
                        actualValue = planTactic.Actualvalue,
                        IsUpdate = status
                    }).Select(planTacticActual => planTacticActual).Distinct(),
                    ////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
                    LastSync = tactic.LastSyncDate == null ? string.Empty : ("Last synced with integration " + Common.GetFormatedDate(tactic.LastSyncDate) + "."),
                    stageTitle = Common.GetTacticStageTitle(tactic.PlanTacticId),
                    tacticStageId = tactic.Stage.StageId,
                    tacticStageTitle = tactic.Stage.Title,
                    projectedStageValue = tactic.ProjectedStageValue,
                    // Change by Nishant sheth
                    // Desc :: #1765 - add period condition to get value
                    projectedStageValueActual = lstTacticActual.Where(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.StageTitle == TitleProjectedStageValue
                    && lstMonthlyDynamic.Where(a => a.Id == tactic.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(tacticActual.Period)
                    ).Sum(tacticActual => tacticActual.Actualvalue),

                    IsTacticEditable = ((tactic.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(tactic.CreatedBy)) ? (lstViewEditEntities.Contains(tactic.PlanTacticId)) : false),
                    //// Set Line Item data with it's actual values and Sum
                    LineItemsData = (tacticLineItem.Where(lineItem => lineItem.PlanTacticId.Equals(tactic.PlanTacticId)).ToList()).Select(lineItem => new
                    {
                        id = lineItem.PlanLineItemId,
                        Title = lineItem.Title,
                        LineItemCost = lineItem.Cost,
                        //// Get the sum of actual
                        // Change by Nishant sheth
                        // Desc :: #1765 - add period condition to get value
                        LineItemActualCost = (tacticLineItemActual.Where(lineItemActual => lineItemActual.PlanLineItemId == lineItem.PlanLineItemId
                        && lstMonthlyDynamic.Where(a => a.Id == tactic.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Contains(lineItemActual.Period))).ToList().Sum(lineItemActual => lineItemActual.Value),
                        LineItemActual = lstMonthlyDynamic.Where(a => a.Id == tactic.PlanTacticId).Select(a => a.listMonthly).FirstOrDefault().Select(month => new
                        {
                            period = month,
                            Cost = tacticLineItemActual.Where(lineItemActual => lineItemActual.PlanLineItemId == lineItem.PlanLineItemId && lineItemActual.Period == month).Select(lineItemActual => lineItemActual.Value)
                        }),
                    }),
                    //Add By Nishant Sheth
                    // Desc:: 1765 get year difrrence
                    YearDiffrence = Convert.ToInt32(Convert.ToInt32(tactic.EndDate.Year) - Convert.ToInt32(tactic.StartDate.Year)),
                    StartYear = Convert.ToInt32(tactic.StartDate.Year)
                });

                //// Prepare final Tactic Actual list
                var lstTactic = tacticObj.Select(tacticActual => new
                {
                    id = tacticActual.id,
                    title = tacticActual.title,
                    mqlProjected = tacticActual.mqlProjected,
                    mqlActual = tacticActual.mqlActual,
                    cwProjected = tacticActual.cwProjected,
                    cwActual = tacticActual.cwActual,
                    costActualData = tacticActual.costActualData,
                    revenueProjected = tacticActual.revenueProjected,
                    revenueActual = tacticActual.revenueActual,
                    costProjected = tacticActual.costProjected,
                    costActual = tacticActual.costActual,
                    //// Modified by dharmraj for implement new formula to calculate ROI, #533
                    roiProjected = tacticActual.costProjected == 0 ? 0 : ((tacticActual.revenueProjected - tacticActual.costProjected) / tacticActual.costProjected),
                    //// Modified by dharmraj for implement new formula to calculate ROI, #533
                    roiActual = tacticActual.costActual == 0 ? 0 : ((tacticActual.revenueActual - tacticActual.costActual) / tacticActual.costActual),
                    individualId = tacticActual.individualId,
                    tacticTypeId = tacticActual.tacticTypeId,
                    modifiedBy = tacticActual.modifiedBy,
                    actualData = tacticActual.actualData,
                    LastSync = tacticActual.LastSync,
                    stageTitle = tacticActual.stageTitle,
                    tacticStageId = tacticActual.tacticStageId,
                    tacticStageTitle = tacticActual.tacticStageTitle,
                    projectedStageValue = tacticActual.projectedStageValue,
                    projectedStageValueActual = tacticActual.projectedStageValueActual,
                    IsTacticEditable = tacticActual.IsTacticEditable,
                    LineItemsData = tacticActual.LineItemsData,
                    YearDiffrence = tacticActual.YearDiffrence,
                    StartYear = tacticActual.StartYear
                });

                var openTactics = lstTactic.Where(tactic => tactic.actualData.ToList().Count == 0).OrderBy(tactic => tactic.title);
                var allTactics = lstTactic.Where(tactic => tactic.actualData.ToList().Count != 0);

                var result = openTactics.Concat(allTactics);

                return Json(result, JsonRequestBehavior.AllowGet);
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
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ChangeLog

        /// <summary>
        /// Action method tp Load change log overview
        /// </summary>
        /// <param name="objectId">object id</param>
        /// <returns>returns partial view ChangeLog</returns>
        public ActionResult LoadChangeLog(int objectId)
        {
            List<ChangeLog_ViewModel> lstChangelog = new List<ChangeLog_ViewModel>();

            try
            {
                lstChangelog = Common.GetChangeLogs(Enums.ChangeLog_TableName.Plan.ToString(), objectId);
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

            return PartialView("_ChangeLog", lstChangelog);
        }

        #endregion

        #region Get Owners by planID Method

        public List<OwnerModel> GetOwnerList(string ViewBy, string ActiveMenu, List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstAllowedEntityIds)
        {
            var lstOwners = GetIndividualsByPlanId(ViewBy, ActiveMenu, tacticList, lstAllowedEntityIds);
            List<OwnerModel> lstAllowedOwners = lstOwners.Select(owner => new OwnerModel
            {
                OwnerId = Convert.ToString(owner.UserId),
                Title = owner.FirstName + " " + owner.LastName,
            }).Distinct().OrderBy(owner => owner.Title).ToList();



            lstAllowedOwners = lstAllowedOwners.Where(owner => !string.IsNullOrEmpty(owner.Title)).OrderBy(owner => owner.Title, new AlphaNumericComparer()).ToList();
            return lstAllowedOwners;
        }


        /// <summary>
        /// Added By :- Sohel Pathan
        /// Date :- 14/04/2014
        /// Action method to get list of users who have created tactic by planId (PL ticket # 428)
        /// </summary>
        /// <param name="PlanId">comma separated list of plan id(s)</param>
        /// <param name="ViewBy">ViewBy option selected from dropdown</param>
        /// <param name="ActiveMenu">current active menu</param>
        /// <returns>returns list of owners in json format</returns>
        public async Task<JsonResult> GetOwnerListForFilter(string PlanId, string ViewBy, string ActiveMenu)
        {
            try
            {
                List<int> PlanIds = string.IsNullOrWhiteSpace(PlanId) ? new List<int>() : PlanId.Split(',').Select(plan => int.Parse(plan)).ToList();

                //// Select Tactics of selected plans
                var campaignList = objDbMrpEntities.Plan_Campaign.Where(campaign => campaign.IsDeleted.Equals(false) && PlanIds.Contains(campaign.PlanId)).Select(campaign => campaign.PlanCampaignId).ToList();
                var programList = objDbMrpEntities.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false) && campaignList.Contains(program.PlanCampaignId)).Select(program => program.PlanProgramId).ToList();
                var tacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && programList.Contains(tactic.PlanProgramId)).Select(tactic => tactic).ToList();
                List<int> planTacticIds = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
                List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                //Added by Nishant to bring the owner name in the list even if they dont own any tactic
                var LoggedInUser = new OwnerModel
                {
                    OwnerId = Sessions.User.UserId.ToString(),
                    Title = Convert.ToString(Sessions.User.FirstName + " " + Sessions.User.LastName),
                };
                await Task.Delay(1);
                return Json(new { isSuccess = true, AllowedOwner = GetOwnerList(ViewBy, ActiveMenu, tacticList, lstAllowedEntityIds), LoggedInUser = LoggedInUser }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Function to get list of users based on selected plan id(s), tab and active menu
        /// </summary>
        /// <param name="PlanId">comma separated list of plan id(s)</param>
        /// <param name="ViewBy">/viewBy option selected from dropdown</param>
        /// <param name="ActiveMenu">current active menu</param>
        /// <param name="IsForAddActuals">flag to check call from Add Actual screen</param>
        /// <returns>returns list of users</returns>
        private List<User> GetIndividualsByPlanId(string ViewBy, string ActiveMenu, List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstAllowedEntityIds)
        {
            BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();

            //// Added by :- Sohel Pathan on 21/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
            if (ActiveMenu.Equals(Enums.ActiveMenu.Plan.ToString()))
            {
                //List<string> statusCD = new List<string>();
                //statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                //statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

                var TacticUserList = tacticList.ToList();
                //     TacticUserList = TacticUserList.Where(tactic => status.Contains(tactic.Status) || ((tactic.CreatedBy == Sessions.User.UserId && !ViewBy.Equals(GanttTabs.Request.ToString())) ? statusCD.Contains(tactic.Status) : false)).Distinct().ToList();
                TacticUserList = TacticUserList.Where(tactic => !ViewBy.Equals(GanttTabs.Request.ToString())).Distinct().ToList();

                if (TacticUserList.Count > 0)
                {
                    //// Custom Restrictions applied
                    TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.UserId).ToList();
                }

                string strContatedIndividualList = string.Join(",", TacticUserList.Select(tactic => tactic.CreatedBy.ToString()));
                //var individuals = bdsUserRepository.GetMultipleTeamMemberName(strContatedIndividualList);
                var individuals = bdsUserRepository.GetMultipleTeamMemberNameByApplicationId(strContatedIndividualList, Sessions.ApplicationId); //PL 1569 Dashrath Prajapati

                return individuals;
            }
            else
            {
                //// Modified by :- Sohel Pathan on 17/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
                //   var TacticUserList = tacticList.Where(tactic => status.Contains(tactic.Status)).Select(tactic => tactic).Distinct().ToList();
                List<string> status = Common.GetStatusListAfterApproved();
                List<string> statusCD = new List<string>();
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());
                var TacticUserList = tacticList.Distinct().ToList();
                TacticUserList = TacticUserList.Where(tactic => status.Contains(tactic.Status) || (!ViewBy.Equals(GanttTabs.Request.ToString()) ? statusCD.Contains(tactic.Status) : false)).Distinct().ToList();

                if (TacticUserList.Count > 0)
                {
                    // Custom Restrictions applied
                    TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.UserId).ToList();
                }

                string strContatedIndividualList = string.Join(",", TacticUserList.Select(tactic => tactic.CreatedBy.ToString()));
                // var individuals = bdsUserRepository.GetMultipleTeamMemberName(strContatedIndividualList);
                var individuals = bdsUserRepository.GetMultipleTeamMemberNameByApplicationId(strContatedIndividualList, Sessions.ApplicationId); //PL 1569 Dashrath Prajapati

                return individuals;
            }
        }

        #endregion

        #region Check User Id
        /// <summary>
        /// Action Method to check user id of logged in use
        /// </summary>
        /// <param name="UserId">user id</param>
        /// <returns>returns json result with redirect url.</returns>
        public ActionResult CheckUserId(string UserId)
        {
            if (Sessions.User.UserId.Equals(Guid.Parse(UserId)))
            {
                return Json(new { returnURL = "#" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                TempData["ErrorMessage"] = Common.objCached.LoginWithSameSession;
                return Json(new { returnURL = Url.Content("~/Login/Index") }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Search Filter data fill

        /// <summary>
        /// Action method to get plans
        /// </summary>
        /// <param name="activeMenu">current active menu (e.g. Home/Plan)</param>
        /// <returns>returns list of plans</returns>
        [HttpPost]
        public ActionResult GetPlans(string activeMenu)
        {
            var activePlan = (dynamic)null;

            try
            {
                if (activeMenu.Equals(Enums.ActiveMenu.Home.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    string publishedPlanStatus = Convert.ToString(Enums.PlanStatus.Published);
                    activePlan = objDbMrpEntities.Plans.Where(plan => plan.IsActive.Equals(true) && plan.IsDeleted == false && plan.Status == publishedPlanStatus && plan.Model.ClientId == Sessions.User.ClientId)
                                                            .Select(plan => new { plan.PlanId, plan.Title })
                                                            .ToList().Where(plan => !string.IsNullOrEmpty(plan.Title)).OrderBy(plan => plan.Title, new AlphaNumericComparer()).ToList();
                }
                else
                {
                    activePlan = objDbMrpEntities.Plans.Where(plan => plan.IsActive.Equals(true) && plan.IsDeleted == false && plan.Model.ClientId == Sessions.User.ClientId)
                                                            .Select(plan => new { plan.PlanId, plan.Title }).ToList()
                                                            .Where(plan => !string.IsNullOrEmpty(plan.Title)).OrderBy(plan => plan.Title, new AlphaNumericComparer()).ToList();
                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
                return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isSuccess = true, ActivePlans = activePlan }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Action method to get list of Custom fields for Search filters
        /// </summary>
        /// <returns>returns custom attributes as Json object</returns>
        [HttpPost]
        public ActionResult GetCustomAttributes()
        {
            try
            {
                List<CustomFieldsForFilter> lstCustomField = new List<CustomFieldsForFilter>();
                List<CustomFieldsForFilter> lstCustomFieldOption = new List<CustomFieldsForFilter>();

                //// Retrive Custom Fields and CustomFieldOptions list
                GetCustomFieldAndOptions(out lstCustomField, out lstCustomFieldOption);

                if (lstCustomField.Count > 0)
                {
                    if (lstCustomFieldOption.Count > 0)
                    {
                        return Json(new { isSuccess = true, lstCustomFields = lstCustomField, lstCustomFieldOptions = lstCustomFieldOption }, JsonRequestBehavior.AllowGet);
                    }
                }

                //// return all custom attribures in json object format
                return Json(new { isSuccess = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }

            return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Action method to get list of Custom fields for Search filters
        /// </summary>
        /// <returns>returns custom attributes as Json object</returns>
        public void GetCustomAttributesIndex(ref HomePlanModel planmodel)
        {
            try
            {
                List<CustomFieldsForFilter> lstCustomField = new List<CustomFieldsForFilter>();
                List<CustomFieldsForFilter> lstCustomFieldOption = new List<CustomFieldsForFilter>();

                //// Retrive Custom Fields and CustomFieldOptions list
                GetCustomFieldAndOptions(out lstCustomField, out lstCustomFieldOption);

                if (lstCustomField.Count > 0)
                {
                    if (lstCustomFieldOption.Count > 0)
                    {
                        planmodel.lstCustomFields = lstCustomField;
                        planmodel.lstCustomFieldOptions = lstCustomFieldOption;
                        // return Json(new { isSuccess = true, lstCustomFields = lstCustomField, lstCustomFieldOptions = lstCustomFieldOption }, JsonRequestBehavior.AllowGet);
                    }
                }

                //// return all custom attribures in json object format
                //return Json(new { isSuccess = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }

            //return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
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
            var lstCustomField = objDbMrpEntities.CustomFields.Where(customField => customField.ClientId == Sessions.User.ClientId && customField.IsDeleted.Equals(false) &&
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
                var lstCustomFieldOptions = objDbMrpEntities.CustomFieldOptions
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

        #region Upcoming Activity Methods

        /// <summary>
        /// Method to fetch the up comming activites value. 
        /// </summary>
        /// <param name="planids">comma sepreated plan id(s)</param>
        /// <param name="CurrentTime">Current Time</param>
        /// <returns></returns>
        public JsonResult BindUpcomingActivitesValues(string planids, string CurrentTime)
        {
            //// Fetch the list of Upcoming Activity
            List<SelectListItem> objUpcomingActivity = UpComingActivity(planids);

            bool IsItemExists = objUpcomingActivity.Where(activity => activity.Value == CurrentTime).Any();

            if (IsItemExists)
            {
                foreach (SelectListItem activity in objUpcomingActivity)
                {
                    activity.Selected = false;
                    //// Set it Selected ture if we found current time value in the list.
                    if (CurrentTime == activity.Value)
                    {
                        activity.Selected = true;
                    }
                }
            }

            objUpcomingActivity = objUpcomingActivity.Where(activity => !string.IsNullOrEmpty(activity.Text)).OrderBy(activity => activity.Text, new AlphaNumericComparer()).ToList();
            return Json(objUpcomingActivity.ToList(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Function to process and fetch the Upcoming Activity  
        /// </summary>
        /// <param name="PlanIds">comma sepreated string plan id(s)</param>
        /// <returns>List fo SelectListItem of Upcoming activity</returns>
        public List<SelectListItem> UpComingActivity(string PlanIds)
        {
            //// List of plan id(s)
            List<int> planIds = string.IsNullOrWhiteSpace(PlanIds) ? new List<int>() : PlanIds.Split(',').Select(plan => int.Parse(plan)).ToList();

            //// Fetch the active plan based of plan ids
            List<Plan> activePlan = objDbMrpEntities.Plans.Where(plan => planIds.Contains(plan.PlanId) && plan.IsActive.Equals(true) && plan.IsDeleted == false).ToList();
            List<int> lstplanid = activePlan.Select(a => a.PlanId).ToList();
            var lstCampaign = objDbMrpEntities.Plan_Campaign.Where(a => lstplanid.Contains(a.PlanId) && a.IsDeleted == false).ToList();


            //// Get the Current year and Pre define Upcoming Activites.
            string currentYear = DateTime.Now.Year.ToString();

            List<SelectListItem> UpcomingActivityList = new List<SelectListItem>();

            //// Fetch the pervious year and future year list and insert into the list object
            var yearlistPrevious = activePlan.Where(plan => plan.Year != currentYear && Convert.ToInt32(plan.Year) < DateTime.Now.Year).Select(plan => plan.Year).Distinct().OrderBy(year => year).ToList();
            yearlistPrevious.ForEach(year => UpcomingActivityList.Add(new SelectListItem { Text = year, Value = year, Selected = false }));


            string strThisQuarter = Enums.UpcomingActivities.thisquarter.ToString(), strThisMonth = Enums.UpcomingActivities.thismonth.ToString(),
                                    quartText = Enums.UpcomingActivitiesValues[strThisQuarter].ToString(), monthText = Enums.UpcomingActivitiesValues[strThisMonth].ToString();

            //// If active plan dosen't have any current plan at that time we have to remove this month and thisquater option
            if (activePlan != null && activePlan.Any() && activePlan.Where(plan => plan.Year == currentYear).Any())
            {
                //// Add current year into the list

                UpcomingActivityList.Add(new SelectListItem { Text = quartText, Value = strThisQuarter, Selected = false });
                UpcomingActivityList.Add(new SelectListItem { Text = monthText, Value = strThisMonth, Selected = false });
                foreach (var camp in lstCampaign)
                {
                    int campStYear = camp.StartDate.Year;
                    int campEdYear = camp.EndDate.Year;
                    int campYearDiffer = campEdYear - campStYear;
                    string EndYear = camp.EndDate.Year.ToString();
                    if (currentYear != EndYear)
                    {
                        var checkYear = UpcomingActivityList.Where(a => a.Text == EndYear).Select(a => a.Text).FirstOrDefault();
                        var checkFromTo = UpcomingActivityList.Where(a => a.Text == currentYear + "-" + EndYear).Select(a => a.Text).FirstOrDefault();
                        int yearDiff = Convert.ToInt32(EndYear) - Convert.ToInt32(currentYear);
                        for (int i = 1; i <= yearDiff; i++)
                        {
                            if (checkYear == null)
                            {
                                UpcomingActivityList.Add(new SelectListItem { Text = EndYear, Value = EndYear, Selected = false });
                            }
                        }
                        if (campYearDiffer > 0)
                        {
                            UpcomingActivityList.Add(new SelectListItem { Text = currentYear + "-" + EndYear, Value = currentYear + "-" + EndYear, Selected = false });
                        }
                    }
                }
                UpcomingActivityList.Add(new SelectListItem { Text = currentYear, Value = currentYear, Selected = true });
            }
            UpcomingActivityList = UpcomingActivityList.GroupBy(a => a.Text).Select(x => x.First()).ToList();

            var yearlistAfter = activePlan.Where(plan => plan.Year != currentYear && Convert.ToInt32(plan.Year) > DateTime.Now.Year).Select(plan => plan.Year).Distinct().OrderBy(year => year).ToList();
            yearlistAfter.ForEach(year => UpcomingActivityList.Add(new SelectListItem { Text = year, Value = year, Selected = false }));
            return UpcomingActivityList.Distinct().ToList();
        }

        #endregion

        #region SetSessionPlan
        /// <summary>
        /// Set the selected plans in Session planid
        /// </summary>
        /// <param name="planids">Plan id's with comma sepreated string</param>
        /// <returns>Json Result with Sucess value and Plan Id</returns>
        public JsonResult SetSessionPlan(string planids)
        {
            List<int> filterplanIds = string.IsNullOrWhiteSpace(planids) ? new List<int>() : planids.Split(',').Select(p => int.Parse(p)).ToList();
            if (filterplanIds.Count == 1)
            {
                Sessions.PlanId = filterplanIds.Select(p => p).FirstOrDefault();
            }
            return Json(new { isSuccess = true, id = Sessions.PlanId }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Validate share link of Notification email for Inspect popup

        /// <summary>
        /// Function to validate shared link for cases such as Valid Entity, is deleted entity, or cross client login
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy> 
        /// <CreatedDate>11/12/2014</CreatedDate>
        /// <param name="currentPlanId">planId from query string parameter of shared link</param>
        /// <param name="planCampaignId">planCampaignId from query string parameter of shared link</param>
        /// <param name="planProgramId">planProgramId from query string parameter of shared link</param>
        /// <param name="planTacticId">planTacticId from query string parameter of shared link</param>
        /// <param name="isImprovement">isImprovement flag from query string parameter of shared link</param>
        /// <returns>returns currentPlanId</returns>
        private int InspectPopupSharedLinkValidation(int currentPlanId, int planCampaignId, int planProgramId, int planTacticId, bool isImprovement, int planLineItemId = 0)
        {
            bool IsEntityDeleted = false;
            bool isValidLoginUser = true;
            bool isEntityExists = true;
            bool isCorrectPlanId = true;

            if (planTacticId > 0 && isImprovement.Equals(false))
            {
                isValidLoginUser = Common.ValidateNotificationShaedLink(currentPlanId, planTacticId, Enums.PlanEntity.Tactic.ToString(), out IsEntityDeleted, out isEntityExists, out isCorrectPlanId);
            }
            else if (planProgramId > 0)
            {
                isValidLoginUser = Common.ValidateNotificationShaedLink(currentPlanId, planProgramId, Enums.PlanEntity.Program.ToString(), out IsEntityDeleted, out isEntityExists, out isCorrectPlanId);
            }
            else if (planCampaignId > 0)
            {
                isValidLoginUser = Common.ValidateNotificationShaedLink(currentPlanId, planCampaignId, Enums.PlanEntity.Campaign.ToString(), out IsEntityDeleted, out isEntityExists, out isCorrectPlanId);
            }
            else if (planTacticId > 0 && isImprovement.Equals(true))
            {
                isValidLoginUser = Common.ValidateNotificationShaedLink(currentPlanId, planTacticId, Enums.PlanEntity.ImprovementTactic.ToString(), out IsEntityDeleted, out isEntityExists, out isCorrectPlanId);
            }
            else if (planLineItemId > 0)
            {
                isValidLoginUser = Common.ValidateNotificationShaedLink(currentPlanId, planLineItemId, Enums.PlanEntity.LineItem.ToString(), out IsEntityDeleted, out isEntityExists, out isCorrectPlanId);
            }
            else
            {
                //// Invalid URL
                ViewBag.ShowInspectPopup = false;
            }

            if (isEntityExists.Equals(false))
            {
                //// Entity does not exists in DB
                ViewBag.ShowInspectPopup = false;
                ViewBag.ShowInspectPopupErrorMessage = Common.objCached.FakeEntityForInspectPopup.ToString();
                if (isValidLoginUser.Equals(false))
                {
                    currentPlanId = 0;
                }
            }
            else if (isValidLoginUser.Equals(false))
            {
                //// Cross client login
                currentPlanId = 0;
                ViewBag.ShowInspectPopup = false;
                ViewBag.ShowInspectPopupErrorMessage = Common.objCached.CrossClientLoginForInspectPopup.ToString();
            }
            else if (isCorrectPlanId.Equals(false))
            {
                //// PlanId and EntityId does not match
                ViewBag.ShowInspectPopup = false;
                ViewBag.ShowInspectPopupErrorMessage = Common.objCached.InvalidURLForInspectPopup.ToString();
                if (isValidLoginUser.Equals(false))
                {
                    currentPlanId = 0;
                }
            }
            else if (IsEntityDeleted.Equals(true))
            {
                //// Entity is deleted
                ViewBag.ShowInspectPopup = false;
                ViewBag.ShowInspectPopupErrorMessage = Common.objCached.DeletedEntityForInspectPopup.ToString();
            }
            else
            {
                //// Valid notification shared link
                ViewBag.ShowInspectPopup = true;
            }

            return currentPlanId;
        }

        /// <summary>
        /// Function to validate shared link as per Custom Restrictions
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy> 
        /// <CreatedDate>11/12/2014</CreatedDate>
        /// <param name="planCampaignId">planCampaignId from query string parameter of shared link</param>
        /// <param name="planProgramId">planProgramId from query string parameter of shared link</param>
        /// <param name="planTacticId">planTacticId from query string parameter of shared link</param>
        /// <param name="isImprovement">isImprovement flag from query string parameter of shared link</param>
        /// <param name="lstUserCustomRestriction">list of custom restrictions for current logged in user</param>
        /// <param name="ViewOnlyPermission">ViewOnlyPermission flag in form of int</param>
        /// <param name="ViewEditPermission">ViewEditPermission flag in form of int</param>
        /// <returns>returns flag for custom restriction as per custom restriction</returns>
        private bool InspectPopupSharedLinkValidationForCustomRestriction(int planCampaignId, int planProgramId, int planTacticId, bool isImprovement, int planLineItemId = 0)
        {
            bool isValidEntity = false;

            if (planTacticId > 0 && isImprovement.Equals(false))
            {
                List<int> AllowedTacticIds = new List<int>();
                AllowedTacticIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, new List<int>() { planTacticId }, false);

                var objTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == planTacticId && tactic.IsDeleted == false
                                                                    && (AllowedTacticIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.UserId))
                                                                    .Select(tactic => tactic.PlanTacticId);

                if (objTactic.Count() != 0)
                {
                    isValidEntity = true;
                }
            }
            else if (planProgramId > 0)
            {
                var objProgram = objDbMrpEntities.Plan_Campaign_Program.Where(program => program.PlanProgramId == planProgramId && program.IsDeleted == false
                                                                ).Select(program => program);

                if (objProgram.Count() != 0)
                {
                    isValidEntity = true;
                }
            }
            else if (planCampaignId > 0)
            {
                var objCampaign = objDbMrpEntities.Plan_Campaign.Where(campaign => campaign.PlanCampaignId == planCampaignId && campaign.IsDeleted == false
                                                                ).Select(campaign => campaign.PlanCampaignId);

                if (objCampaign.Count() != 0)
                {
                    isValidEntity = true;
                }
            }
            else if (planTacticId > 0 && isImprovement.Equals(true))
            {
                var objImprovementTactic = objDbMrpEntities.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => improvementTactic.ImprovementPlanTacticId == planTacticId && improvementTactic.IsDeleted == false
                                                                                        ).Select(improvementTactic => improvementTactic.ImprovementPlanTacticId);

                if (objImprovementTactic.Count() != 0)
                {
                    isValidEntity = true;
                }
            }
            else if (planLineItemId > 0)
            {
                var objPlanLineItem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => lineItem.PlanLineItemId == planLineItemId && lineItem.IsDeleted == false
                                                                ).Select(lineItem => lineItem.PlanLineItemId);

                if (objPlanLineItem.Count() != 0)
                {
                    isValidEntity = true;
                }
            }

            return isValidEntity;
        }

        #endregion

        #region Tactic type list

        public List<TacticTypeModel> GetTacticTypeList(List<Plan_Campaign_Program_Tactic> tacticList, List<int> lstAllowedEntityIds)
        {


            var TacticUserList = tacticList.ToList();

            if (TacticUserList.Count > 0)
            {
                //// Custom Restrictions applied
                TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId) || tactic.CreatedBy == Sessions.User.UserId).ToList();
            }
            List<TacticTypeModel> objTacticType = TacticUserList.GroupBy(pc => new { title = pc.TacticType.Title, id = pc.TacticTypeId }).Select(pc => new TacticTypeModel
            {
                Title = pc.Key.title,
                TacticTypeId = pc.Key.id,
                Number = pc.Count()
            }).OrderBy(TacticType => TacticType.Title, new AlphaNumericComparer()).ToList();

            return objTacticType;
        }


        //Added by Komal rawal for #1283
        public async Task<JsonResult> GetTacticTypeListForFilter(string PlanId)
        {
            try
            {

                List<int> lstPlanIds = string.IsNullOrWhiteSpace(PlanId) ? new List<int>() : PlanId.Split(',').Select(plan => int.Parse(plan)).ToList();
                var tacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && lstPlanIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactic => tactic).ToList();
                List<int> planTacticIds = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
                List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);
                await Task.Delay(1);
                return Json(new { isSuccess = true, TacticTypelist = GetTacticTypeList(tacticList, lstAllowedEntityIds) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);

                //// To handle unavailability of BDSService
                if (objException is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get start and end dates for plan in top down approach
        //Added by Komal Rawal
        public DateTime GetStartDateForPlan(int planId, List<Plan_Campaign> lstCampaign, string year)
        {
            DateTime minDate = DateTime.Now;
            int Year = int.Parse(year);
            var CampaignList = lstCampaign.Where(campaign => campaign.PlanId == planId).Select(campaign => campaign).ToList();
            if (CampaignList.Count() > 0)
            {
                //List<int> queryPlanCampaignId = lstCampaign.Where(campaign => campaign.PlanId == planId).Select(campaign => campaign.PlanCampaignId).ToList<int>();
                //minDate = queryPlanCampaignId.Count() > 0 ? lstCampaign.Where(campaign => queryPlanCampaignId.Contains(campaign.PlanCampaignId)).Select(campaign => campaign.StartDate).Min() : DateTime.MinValue;

                minDate = CampaignList.Select(campaign => campaign.StartDate).Min();

            }
            else
            {
                minDate = new DateTime(Year, 1, 1);

            }

            return minDate;
        }

        public DateTime GetEndDateForPlan(int planId, List<Plan_Campaign> lstCampaign, string year)
        {
            DateTime EndDate = DateTime.Now;
            int Year = int.Parse(year);
            var CampaignList = lstCampaign.Where(campaign => campaign.PlanId == planId).Select(campaign => campaign).ToList();
            if (CampaignList.Count > 0)
            {
                //List<int> queryPlanCampaignId = lstCampaign.Where(campaign => campaign.PlanId == planId).Select(campaign => campaign.PlanCampaignId).ToList<int>();
                //EndDate = queryPlanCampaignId.Count() > 0 ? lstCampaign.Where(campaign => queryPlanCampaignId.Contains(campaign.PlanCampaignId)).Select(campaign => campaign.EndDate).Min() : DateTime.MinValue;
                EndDate = CampaignList.Select(campaign => campaign.EndDate).Max();
            }
            else
            {
                EndDate = new DateTime(Year, 12, 31);

            }

            return EndDate;
        }

        //End
        #endregion

        #region --entity Type Permission based access---

        public PermissionModel GetPermission(int id, string section, List<Guid> lstSubordinatesIds, string InspectPopupMode = "")
        {
            PermissionModel _model = new PermissionModel();
            _model.IsPlanCreateAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);

            Plan_Campaign_Program_Tactic objPlan_Campaign_Program_Tactic = null;
            Plan_Campaign_Program objPlan_Campaign_Program = null;
            Plan_Campaign objPlan_Campaign = null;
            Plan_Campaign_Program_Tactic_LineItem objPlan_Campaign_Program_Tactic_LineItem = null;

            if (Convert.ToString(section) != "")
            {
                DateTime todaydate = DateTime.Now;
                if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Tactic).ToLower())
                {
                    objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(pcpobjw => pcpobjw.PlanTacticId.Equals(id)).FirstOrDefault();
                    if (_model.IsPlanCreateAllAuthorized)
                    {
                        _model.IsPlanCreateAll = true;
                    }
                    else
                    {
                        if (objPlan_Campaign_Program_Tactic.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(objPlan_Campaign_Program_Tactic.CreatedBy))
                        {
                            _model.IsPlanCreateAll = true;
                        }
                        else
                        {
                            _model.IsPlanCreateAll = false;
                        }
                    }
                }

                else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Program).ToLower())
                {
                    objPlan_Campaign_Program = objDbMrpEntities.Plan_Campaign_Program.Where(pcpobjw => pcpobjw.PlanProgramId.Equals(id)).FirstOrDefault();
                    if (_model.IsPlanCreateAllAuthorized)
                    {
                        _model.IsPlanCreateAll = true;
                    }
                    else
                    {
                        if (objPlan_Campaign_Program.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(objPlan_Campaign_Program.CreatedBy))
                        {
                            _model.IsPlanCreateAll = true;
                        }
                        else
                        {
                            _model.IsPlanCreateAll = false;
                        }
                    }
                }

                else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.Campaign).ToLower())
                {
                    objPlan_Campaign = objDbMrpEntities.Plan_Campaign.Where(pcpobjw => pcpobjw.PlanCampaignId.Equals(id)).FirstOrDefault();
                    if (_model.IsPlanCreateAllAuthorized)
                    {
                        _model.IsPlanCreateAll = true;
                    }
                    else
                    {
                        if (objPlan_Campaign.CreatedBy.Equals(Sessions.User.UserId) || lstSubordinatesIds.Contains(objPlan_Campaign.CreatedBy))
                        {
                            _model.IsPlanCreateAll = true;
                        }
                        else
                        {
                            _model.IsPlanCreateAll = false;
                        }
                    }
                }

                else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.ImprovementTactic).ToLower())
                {
                    if (InspectPopupMode == Enums.InspectPopupMode.Edit.ToString())
                    {
                        _model.InspectMode = Enums.InspectPopupMode.Edit.ToString();
                    }
                    else if (InspectPopupMode == Enums.InspectPopupMode.Add.ToString())
                    {
                        _model.InspectMode = Enums.InspectPopupMode.Add.ToString();
                    }
                    else
                    {
                        _model.InspectMode = Enums.InspectPopupMode.ReadOnly.ToString();
                    }
                }

                else if (Convert.ToString(section).Trim().ToLower() == Convert.ToString(Enums.Section.LineItem).ToLower())
                {
                    objPlan_Campaign_Program_Tactic_LineItem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(pcptl => pcptl.PlanLineItemId.Equals(id)).FirstOrDefault();
                    if (_model.IsPlanCreateAllAuthorized)
                    {
                        _model.IsPlanCreateAll = true;
                    }
                    else
                    {
                        if (objPlan_Campaign_Program_Tactic_LineItem.CreatedBy.Equals(Sessions.User.UserId))
                        {
                            _model.IsPlanCreateAll = true;
                        }
                        else
                        {
                            _model.IsPlanCreateAll = false;
                        }
                    }
                }
            }

            if (Convert.ToString(section).Equals(Enums.Section.Plan.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                _model.IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
                _model.IsPlanCreateAll = false;
                if (id > 0)
                {
                    var objplan = objDbMrpEntities.Plans.FirstOrDefault(m => m.PlanId == id && m.IsDeleted == false);
                    if (_model.IsPlanCreateAuthorized)
                    {
                        _model.IsPlanCreateAll = true;
                    }
                    else
                    {
                        if (objplan.CreatedBy.Equals(Sessions.User.UserId))
                        {
                            _model.IsPlanCreateAll = true;
                        }
                    }
                }
                else
                {
                    _model.IsPlanCreateAll = true;
                }
            }
            return _model;
        }

        #endregion

        #region --Saving and rendering Last accessed data of Views---
        /// <summary>
        /// Added By: Komal Rawal.
        /// Action to get last accessed data
        /// </summary>
        public ActionResult LastSetOfViews(string PresetName = "", Boolean isLoadPreset = false)
        {
            var StatusLabel = Enums.FilterLabel.Status.ToString();
            var LastSetOfStatus = new List<string>();
            //Modified for #1750 by Komal Rawal
            var listofsavedviews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).Select(view => view).ToList();
            //Modified by Rahul shah on 24/12/2015 for PL#1837
            if (isLoadPreset == true)
            {

                List<Preset> newList = (from item in listofsavedviews
                                        where item.ViewName != null
                                        select new Preset
                                        {
                                            Id = Convert.ToString(item.Id),
                                            Name = item.ViewName,
                                            IsDefaultPreset = item.IsDefaultPreset
                                        }).ToList();


                newList = newList.GroupBy(g => g.Name).Select(x => x.FirstOrDefault()).ToList();

                //for searching 
                if (!string.IsNullOrEmpty(PresetName))
                {
                    List<Preset> newListSearch = (from item in listofsavedviews
                                                  where item.ViewName != null && item.ViewName.ToUpper().Contains(PresetName.ToUpper())
                                                  select new Preset
                                                  {
                                                      Id = Convert.ToString(item.Id),
                                                      Name = item.ViewName,
                                                      IsDefaultPreset = item.IsDefaultPreset
                                                  }).ToList();
                    newListSearch = newListSearch.GroupBy(g => g.Name).Select(x => x.FirstOrDefault()).ToList();
                    return PartialView("~/Views/Shared/_DefaultViewFilters.cshtml", newListSearch);
                }


                return PartialView("~/Views/Shared/_DefaultViewFilters.cshtml", newList);
            }
            else
            {
                if (!string.IsNullOrEmpty(PresetName))
                {
                    listofsavedviews = listofsavedviews.Where(name => name.ViewName == PresetName).ToList();
                }
                else
                {
                    listofsavedviews = listofsavedviews.Where(view => view.IsDefaultPreset == true).ToList();
                    if (listofsavedviews.Count == 0)
                    {
                        listofsavedviews = listofsavedviews.Where(view => view.ViewName == null).ToList();
                    }
                }

                var SetOfStatus = listofsavedviews.Where(view => view.FilterName == StatusLabel).Select(View => View.FilterValues).ToList();
                if (SetOfStatus.Count > 0)
                {
                    if (SetOfStatus.FirstOrDefault() != null)
                    {
                        LastSetOfStatus = SetOfStatus.FirstOrDefault().Split(',').ToList();
                    }
                    //else
                    //{
                    //    LastSetOfStatus = null;
                    //}
                }

                var OwnerLabel = Enums.FilterLabel.Owner.ToString();

                var LastSetOfOwners = new List<string>();


                var SetOfOwners = listofsavedviews.Where(view => view.FilterName == OwnerLabel).Select(View => View.FilterValues).FirstOrDefault();
                if (SetOfOwners != null)
                {
                    LastSetOfOwners = SetOfOwners.Split(',').ToList();
                }

                var TTLabel = Enums.FilterLabel.TacticType.ToString();
                var LastSetOfTacticType = new List<string>();

                var SetOfTacticType = listofsavedviews.Where(view => view.FilterName == TTLabel).Select(View => View.FilterValues).ToList();
                if (SetOfTacticType.Count > 0)
                {
                    if (SetOfTacticType.FirstOrDefault() != null)
                    {
                        LastSetOfTacticType = SetOfTacticType.FirstOrDefault().Split(',').ToList();
                    }
                    //else
                    //{
                    //    LastSetOfTacticType = null;
                    //}

                }

                var LastSetofCustomField = listofsavedviews.Where(view => view.FilterName.Contains("CF")).Select(view => new { ID = view.FilterName, Value = view.FilterValues }).ToList();

                Sessions.FilterPresetName = null;
                return Json(new { StatusNAmes = LastSetOfStatus, Customfields = LastSetofCustomField, OwnerNames = LastSetOfOwners, TTList = LastSetOfTacticType }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Added By: KOmal Rawal.
        /// Action to save last accessed data
        /// </summary>
        /// <param name="PlanId">Plan Id and filters</param>
        public JsonResult SaveLastSetofViews(string planId, string customFieldIds, string ownerIds, string TacticTypeid, string StatusIds, string ViewName)
        {


            List<int> planIds = string.IsNullOrWhiteSpace(planId) ? new List<int>() : planId.Split(',').Select(plan => int.Parse(plan)).ToList();
            List<Plan> ListofPlans = objDbMrpEntities.Plans.Where(p => planIds.Contains(p.PlanId)).ToList();
            string planPublishedStatus = Enums.PlanStatusValues.FirstOrDefault(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
            planIds = ListofPlans.Where(plan => plan.Status.Equals(planPublishedStatus)).Select(plan => plan.PlanId).ToList();
            // Added By Komal Rawal for #1750 to default users to their last view
            List<Plan_UserSavedViews> NewCustomFieldData = new List<Plan_UserSavedViews>();
            NewCustomFieldData = new List<Plan_UserSavedViews>();
            List<string> tempFilterValues = new List<string>();
            #region "Remove previous records by userid"
            var prevCustomFieldList = objDbMrpEntities.Plan_UserSavedViews.Where(custmfield => custmfield.Userid == Sessions.User.UserId).ToList();
            if (ViewName != null && ViewName != "")
            {
                var ViewNames = prevCustomFieldList.Where(custmfield => custmfield.ViewName != null).Select(name => name.ViewName).ToList();
                if (ViewNames.Contains(ViewName))
                {
                    return Json(new { isSuccess = false, msg = "Given Preset name already exists" }, JsonRequestBehavior.AllowGet);

                }

            }
            else
            {
                prevCustomFieldList = prevCustomFieldList.Where(custmfield => custmfield.ViewName == null).ToList();
            }

            #endregion


            #region "Save filter values to Plan_UserSavedViews"


            if (planIds.Count != 0)
            {
                Plan_UserSavedViews objFilterValues = new Plan_UserSavedViews();
                objFilterValues.ViewName = null;
                if (ViewName != null && ViewName != "")
                {
                    objFilterValues.ViewName = ViewName;
                }
                objFilterValues.FilterName = Enums.FilterLabel.Plan.ToString();
                objFilterValues.FilterValues = planId;
                objFilterValues.Userid = Sessions.User.UserId;
                objFilterValues.LastModifiedDate = DateTime.Now;
                objFilterValues.IsDefaultPreset = false;
                NewCustomFieldData.Add(objFilterValues);
                objDbMrpEntities.Entry(objFilterValues).State = EntityState.Added;

            }
            if (ownerIds != null && ownerIds != "")
            {
                Plan_UserSavedViews objFilterValues = new Plan_UserSavedViews();
                objFilterValues.ViewName = null;
                if (ViewName != null && ViewName != "")
                {
                    objFilterValues.ViewName = ViewName;
                }
                objFilterValues.FilterName = Enums.FilterLabel.Owner.ToString();
                objFilterValues.FilterValues = ownerIds;
                objFilterValues.Userid = Sessions.User.UserId;
                objFilterValues.LastModifiedDate = DateTime.Now;
                objFilterValues.IsDefaultPreset = false;
                objDbMrpEntities.Entry(objFilterValues).State = EntityState.Added;
                NewCustomFieldData.Add(objFilterValues);
            }
            if (TacticTypeid != null && TacticTypeid != "")
            {
                //if(TacticTypeid == "0")
                //{
                //    TacticTypeid = null;
                //}
                Plan_UserSavedViews objFilterValues = new Plan_UserSavedViews();
                objFilterValues.ViewName = null;
                if (ViewName != null && ViewName != "")
                {
                    objFilterValues.ViewName = ViewName;
                }
                objFilterValues.FilterName = Enums.FilterLabel.TacticType.ToString();
                objFilterValues.FilterValues = TacticTypeid;
                objFilterValues.Userid = Sessions.User.UserId;
                objFilterValues.LastModifiedDate = DateTime.Now;
                objFilterValues.IsDefaultPreset = false;
                objDbMrpEntities.Entry(objFilterValues).State = EntityState.Added;
                NewCustomFieldData.Add(objFilterValues);
            }
            if (StatusIds != "AddActual" && StatusIds != "Report")
            {
                if (StatusIds != null && StatusIds != "")
                {
                    //if (StatusIds == "0")
                    //{
                    //    StatusIds = null;
                    //}
                    Plan_UserSavedViews objFilterValues = new Plan_UserSavedViews();
                    objFilterValues.ViewName = null;
                    if (ViewName != null && ViewName != "")
                    {
                        objFilterValues.ViewName = ViewName;
                    }
                    objFilterValues.FilterName = Enums.FilterLabel.Status.ToString();
                    objFilterValues.FilterValues = StatusIds;
                    objFilterValues.Userid = Sessions.User.UserId;
                    objFilterValues.LastModifiedDate = DateTime.Now;
                    objFilterValues.IsDefaultPreset = false;
                    objDbMrpEntities.Entry(objFilterValues).State = EntityState.Added;
                    NewCustomFieldData.Add(objFilterValues);
                }
            }

            if (customFieldIds != "")
            {

                string[] filteredCustomFields = string.IsNullOrWhiteSpace(customFieldIds) ? null : customFieldIds.Split(',');
                if (filteredCustomFields != null)
                {
                    Plan_UserSavedViews objFilterValues = new Plan_UserSavedViews();
                    List<Plan_UserSavedViews> listLineitem = objDbMrpEntities.Plan_UserSavedViews.Where(a => a.Userid == Sessions.User.UserId).Select(a => a).ToList();
                    Plan_UserSavedViews objLineitem = new Plan_UserSavedViews();
                    string FilterNameTemp = "";
                    string Previousval = "";
                    foreach (string customField in filteredCustomFields)
                    {
                        var PreviousValue = "CF" + "_" + customField.Split('_')[0].ToString();
                        var ExistingFieldlist = listLineitem.Where(pcpobjw => pcpobjw.FilterName.Equals(PreviousValue)).FirstOrDefault();
                        FilterNameTemp += string.IsNullOrEmpty(FilterNameTemp) ? (ExistingFieldlist != null ? ExistingFieldlist.FilterValues : "") : "";
                        if (FilterNameTemp == PreviousValue)
                        {

                            Previousval += ',' + customField.Split('_')[1];
                            objFilterValues.FilterValues = Previousval;
                        }

                        else
                        {
                            objFilterValues = new Plan_UserSavedViews();
                            objFilterValues.ViewName = null;
                            if (ViewName != null && ViewName != "")
                            {
                                objFilterValues.ViewName = ViewName;
                            }
                            objFilterValues.FilterName = "CF" + "_" + customField.Split('_')[0];
                            objFilterValues.FilterValues = customField.Split('_')[1];
                            objFilterValues.Userid = Sessions.User.UserId;
                            objFilterValues.LastModifiedDate = DateTime.Now;
                            objFilterValues.IsDefaultPreset = false;
                            FilterNameTemp = "";
                            FilterNameTemp += "CF" + "_" + customField.Split('_')[0].ToString();
                            Previousval = "";
                            Previousval = customField.Split('_')[1];
                            objDbMrpEntities.Plan_UserSavedViews.Add(objFilterValues);
                        }

                        NewCustomFieldData.Add(objFilterValues);
                    }


                }
            }

            if (StatusIds != null && StatusIds != "")
            {
                if (StatusIds == "Report" || StatusIds == "AddActual")
                {
                    var statulist = prevCustomFieldList.Where(a => a.FilterName == Enums.FilterLabel.Status.ToString());
                    prevCustomFieldList = prevCustomFieldList.Except(statulist).ToList();
                }
            }

            if (ViewName != null)
            {


                objDbMrpEntities.SaveChanges();
            }
            else
            {


                var isCheckinPrev = prevCustomFieldList.Select(a => a.FilterValues).Except(NewCustomFieldData.Select(b => b.FilterValues)).Any();
                var isCheckinNew = NewCustomFieldData.Select(a => a.FilterValues).Except(prevCustomFieldList.Select(b => b.FilterValues)).Any();
                if (isCheckinPrev == true || isCheckinNew == true)
                {
                    prevCustomFieldList.ForEach(custmfield => objDbMrpEntities.Entry(custmfield).State = EntityState.Deleted);
                    objDbMrpEntities.SaveChanges();
                }
                else
                {
                    if (prevCustomFieldList.Count == 0)
                    {
                        objDbMrpEntities.SaveChanges();
                    }
                }
            }

            #endregion
            //End
            return Json(new { isSuccess = true, ViewName = ViewName }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Set the default filter
        /// Addded By komal Rawal for 1749
        /// </summary>
        /// <param name="PresetName"></param>
        /// <returns></returns>
        public ActionResult SaveDefaultPreset(string PresetName)
        {
            PresetName = Convert.ToString(PresetName).TrimStart();
            PresetName = Convert.ToString(PresetName).TrimEnd();
            PresetName = Convert.ToString(PresetName).Trim();
            var ListOfUserViews = objDbMrpEntities.Plan_UserSavedViews.Where(view => view.Userid == Sessions.User.UserId).ToList();
            var ResetFlagList = ListOfUserViews.Where(view => view.IsDefaultPreset == true).ToList();
            ResetFlagList.ForEach(s => s.IsDefaultPreset = false);
            if (!string.IsNullOrEmpty(PresetName))
            {
                ListOfUserViews = ListOfUserViews.Where(name => name.ViewName == PresetName).ToList();
                ListOfUserViews.ForEach(s => s.IsDefaultPreset = true);

            }
            objDbMrpEntities.SaveChanges();


            return Json(new { isSuccess = true }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Set filter name in sessions
        /// Addded By komal Rawal for 1749
        /// </summary>
        /// <param name="PresetName"></param>
        /// <returns></returns>
        public JsonResult SetFilterPresetName(string Filtername)
        {
            Sessions.FilterPresetName = Filtername;
            return Json(new { isSuccess = true }, JsonRequestBehavior.AllowGet);
        }

        #region --Delete Preset Data---
        /// <summary>
        /// Added By: Rahul Shah.
        /// Action to Delete Preset data.
        /// </summary>
        public ActionResult DeletePreset(string PresetName)
        {
            if (!string.IsNullOrEmpty(PresetName))
            {

                PresetName = Convert.ToString(PresetName).TrimStart();
                PresetName = Convert.ToString(PresetName).TrimEnd();
                PresetName = Convert.ToString(PresetName).Trim();

                var lstViewID = objDbMrpEntities.Plan_UserSavedViews.Where(x => x.Userid == Sessions.User.UserId && x.ViewName == PresetName).ToList();
                if (lstViewID != null)
                {
                    foreach (var item in lstViewID)
                    {
                        Plan_UserSavedViews planToDelete = lstViewID.Where(x => x.Id == item.Id).SingleOrDefault();
                        objDbMrpEntities.Plan_UserSavedViews.Remove(planToDelete);
                    }
                    objDbMrpEntities.SaveChanges();
                    return Json(new { isSuccess = true, msg = "Preset " + PresetName + " deleted successfuly" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #endregion

        #region --Get Header Data for HoneyComb Pdf---
        /// <summary>
        /// Added By: Rahul Shah.
        /// Action to get Header Data for HoneyComb Pdf
        /// </summary>
        public JsonResult GetHeaderDataforHoneycombPDF(string TactIds, string strParam)
        {
            //For MQL Value.
            List<int> TacticIds = string.IsNullOrWhiteSpace(TactIds) ? new List<int>() : TactIds.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();

            var objPlan_Campaign_Program_Tactic1 = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
                                      TacticIds.Contains(tactic.PlanTacticId)).ToList();
            var tactcost = objPlan_Campaign_Program_Tactic1.Sum(TactItem => TactItem.Cost);
            var tactcount = objPlan_Campaign_Program_Tactic1.Count();

            var MQLs = Common.GetTacticStageRelation(objPlan_Campaign_Program_Tactic1, false).Sum(tactic => tactic.MQLValue);
            string planYear = string.Empty;
            int Planyear;
            bool isNumeric = false;

            //For Activity Distribution
            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;
            if (strParam.Contains("-"))
            {
                List<ActivityChart> lstActivityChartyears = new List<ActivityChart>();
                lstActivityChartyears = getmultipleyearActivityChartHoneyComb(strParam, TactIds);
                return Json(new { lstchart = lstActivityChartyears.ToList() }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                isNumeric = int.TryParse(strParam, out Planyear);
                if (isNumeric)
                {
                    planYear = Convert.ToString(Planyear);
                }
                else
                {
                    planYear = DateTime.Now.Year.ToString();
                }
            }

            Common.GetPlanGanttStartEndDate(planYear, strParam, ref CalendarStartDate, ref CalendarEndDate);
            var objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
                                      TacticIds.Contains(tactic.PlanTacticId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate))).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate }).ToList();



            //// Prepare an array of month as per selected dropdown paramter
            int[] monthArray = new int[12];

            if (objPlan_Campaign_Program_Tactic != null && objPlan_Campaign_Program_Tactic.Count() > 0)
            {
                IEnumerable<string> differenceItems;
                int year = 0;
                if (strParam != null && isNumeric)
                {
                    year = Planyear;
                }

                int currentMonth = DateTime.Now.Month, monthNo = 0;
                DateTime startDate, endDate;
                foreach (var tactic in objPlan_Campaign_Program_Tactic)
                {
                    startDate = endDate = new DateTime();
                    startDate = Convert.ToDateTime(tactic.StartDate);
                    endDate = Convert.ToDateTime(tactic.EndDate);

                    if (year != 0)
                    {
                        if (startDate.Year == year)
                        {
                            if (startDate.Year == endDate.Year)
                            {
                                endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                            }
                            else
                            {
                                endDate = new DateTime(startDate.Year, 12, 31);
                            }

                            differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));

                            foreach (string objDifference in differenceItems)
                            {
                                monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                                if (monthNo == 1)
                                {
                                    monthArray[0] = monthArray[0] + 1;
                                }
                                else
                                {
                                    monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                }
                            }
                        }
                        else if (endDate.Year == year)
                        {
                            if (startDate.Year != year)
                            {
                                startDate = new DateTime(endDate.Year, 01, 01);
                                differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));

                                foreach (string objDifference in differenceItems)
                                {
                                    monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                                    if (monthNo == 1)
                                    {
                                        monthArray[0] = monthArray[0] + 1;
                                    }
                                    else
                                    {
                                        monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                    }
                                }
                            }
                        }
                    }
                    else if (strParam.Equals(Enums.UpcomingActivities.thismonth.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));
                        List<string> thismonthdifferenceItem = new List<string>();
                        if (differenceItems.Count() > 12)
                        {
                            thismonthdifferenceItem = differenceItems.ToList();
                            thismonthdifferenceItem.RemoveRange(12, 12);
                        }
                        else
                        {
                            thismonthdifferenceItem = differenceItems.ToList();
                        }
                        foreach (string objDifference in thismonthdifferenceItem)
                        {
                            monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                            if (monthNo == DateTime.Now.Month)
                            {
                                if (monthNo == 1)
                                {
                                    monthArray[0] = monthArray[0] + 1;
                                }
                                else
                                {
                                    monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                }
                            }
                        }
                    }
                    else if (strParam.Equals(Enums.UpcomingActivities.thisquarter.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentMonth == 1 || currentMonth == 2 || currentMonth == 3)
                        {
                            if (startDate.Month == 1 || startDate.Month == 2 || startDate.Month == 3 || endDate.Month == 1 || endDate.Month == 2 || endDate.Month == 3)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q1), startDate, endDate, monthArray);
                            }
                        }
                        else if (currentMonth == 4 || currentMonth == 5 || currentMonth == 6)
                        {
                            if (startDate.Month == 4 || startDate.Month == 5 || startDate.Month == 6 || endDate.Month == 4 || endDate.Month == 5 || endDate.Month == 6)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q2), startDate, endDate, monthArray);
                            }
                        }
                        else if (currentMonth == 7 || currentMonth == 8 || currentMonth == 9)
                        {
                            if (startDate.Month == 7 || startDate.Month == 8 || startDate.Month == 9 || endDate.Month == 7 || endDate.Month == 8 || endDate.Month == 9)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q3), startDate, endDate, monthArray);
                            }
                        }
                        else if (currentMonth == 10 || currentMonth == 11 || currentMonth == 12)
                        {
                            if (startDate.Month == 10 || startDate.Month == 11 || startDate.Month == 12 || endDate.Month == 10 || endDate.Month == 11 || endDate.Month == 12)
                            {
                                monthArray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q4), startDate, endDate, monthArray);
                            }
                        }
                    }
                }
            }

            //// Prepare Activity Chart list
            List<ActivityChart> lstActivityChart = new List<ActivityChart>();
            for (int month = 0; month < monthArray.Count(); month++)
            {
                ActivityChart objActivityChart = new ActivityChart();
                string strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month + 1);

                if (month == 0)
                {
                    objActivityChart.Month = strMonthName[0].ToString();
                }
                else
                {
                    if (month % 3 == 0)
                    {
                        objActivityChart.Month = strMonthName[0].ToString();
                    }
                    else
                    {
                        objActivityChart.Month = string.Empty;
                    }
                    if (month == 11)
                    {
                        objActivityChart.Month = strMonthName[0].ToString();
                    }
                }
                if (monthArray[month] == 0)
                {
                    objActivityChart.NoOfActivity = string.Empty;
                }
                else
                {
                    objActivityChart.NoOfActivity = monthArray[month].ToString();
                }
                objActivityChart.Color = Common.ActivityChartColor;
                lstActivityChart.Add(objActivityChart);
            }
            // await Task.Delay(1);


            return Json(new { lstchart = lstActivityChart.ToList(), TotalMql = MQLs, TotalCost = tactcost, TotalCount = tactcount, }, JsonRequestBehavior.AllowGet);

        }

        private List<ActivityChart> getmultipleyearActivityChartHoneyComb(string strParam, string TactIds)
        {
            List<int> TacticIds = string.IsNullOrWhiteSpace(TactIds) ? new List<int>() : TactIds.Split(',').Select(tactictype => int.Parse(tactictype)).ToList();

            string planYear = string.Empty;
            int Planyear = 0;
            bool isNumeric = false;
            List<ActivityChart> lstActivityChart = new List<ActivityChart>();
            List<ActivityChart> lstActivitybothChart = new List<ActivityChart>();
            List<string> categories = new List<string>();
            string[] multipleyear = strParam.Split('-');

            for (int i = 0; i < multipleyear.Count(); i++)
            {
                isNumeric = int.TryParse(multipleyear[i], out Planyear);
                if (isNumeric)
                {
                    planYear = Convert.ToString(Planyear);
                }
                else
                {
                    planYear = DateTime.Now.Year.ToString();
                }
                //// Set start and end date for calender
                Common.GetPlanGanttStartEndDate(planYear, multipleyear[i], ref CalendarStartDate, ref CalendarEndDate);

                //// Selecte tactic(s) from selected programs
                var objPlan_Campaign_Program_Tactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
                                    TacticIds.Contains(tactic.PlanTacticId) && ((tactic.StartDate >= CalendarStartDate && tactic.EndDate >= CalendarStartDate) || (tactic.StartDate <= CalendarStartDate && tactic.EndDate >= CalendarStartDate))).Select(tactic => new { PlanTacticId = tactic.PlanTacticId, CreatedBy = tactic.CreatedBy, TacticTypeId = tactic.TacticTypeId, Status = tactic.Status, StartDate = tactic.StartDate, EndDate = tactic.EndDate }).ToList();

                //// Prepare an array of month as per selected dropdown paramter
                int[] monthArray = new int[12];
                int[] monthArrayactivity = new int[12];
                int q1 = 0;
                int q2 = 0;
                int q3 = 0;
                int q4 = 0;

                if (objPlan_Campaign_Program_Tactic != null && objPlan_Campaign_Program_Tactic.Count() > 0)
                {
                    IEnumerable<string> differenceItems;
                    int year = 0;
                    if (multipleyear[i] != null && isNumeric)
                    {
                        year = Planyear;
                    }

                    int currentMonth = DateTime.Now.Month, monthNo = 0;
                    DateTime startDate, endDate;
                    foreach (var tactic in objPlan_Campaign_Program_Tactic)
                    {
                        startDate = endDate = new DateTime();
                        startDate = Convert.ToDateTime(tactic.StartDate);
                        endDate = Convert.ToDateTime(tactic.EndDate);

                        if (year != 0)
                        {
                            if (startDate.Year == year)
                            {
                                if (startDate.Year == endDate.Year)
                                {
                                    endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                                }
                                else
                                {
                                    endDate = new DateTime(startDate.Year, 12, 31);
                                }

                                differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));
                                int q1counter = 0;
                                int q2counter = 0;
                                int q3counter = 0;
                                int q4counter = 0;
                                foreach (string objDifference in differenceItems)
                                {
                                    monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                                    //
                                    categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
                                    string strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNo);
                                    int CurrentQuarter = (int)Math.Floor(((decimal)(monthNo) + 2) / 3);
                                    if (CurrentQuarter == 1)
                                    {
                                        if (monthNo <= 3)
                                        {
                                            if (q1counter == 0)
                                            {
                                                monthArrayactivity[2] = q1 + 1;
                                                q1 = monthArrayactivity[2];
                                                q1counter++;
                                            }
                                        }
                                    }
                                    else if (CurrentQuarter == 2)
                                    {
                                        if (monthNo <= 6)
                                        {
                                            if (q2counter == 0)
                                            {
                                                monthArrayactivity[5] = q2 + 1;
                                                q2 = monthArrayactivity[5];
                                                q2counter++;
                                            }
                                        }
                                    }
                                    else if (CurrentQuarter == 3)
                                    {
                                        if (monthNo <= 9)
                                        {
                                            if (q3counter == 0)
                                            {
                                                monthArrayactivity[8] = q3 + 1;
                                                q3 = monthArrayactivity[8];
                                                q3counter++;
                                            }
                                        }
                                    }
                                    else if (CurrentQuarter == 4)
                                    {
                                        if (monthNo <= 12)
                                        {
                                            if (q4counter == 0)
                                            {
                                                monthArrayactivity[11] = q4 + 1;
                                                q4 = monthArrayactivity[11];
                                                q4counter++;
                                            }
                                        }
                                    }
                                    //


                                    if (monthNo == 1)
                                    {
                                        monthArray[0] = monthArray[0] + 1;
                                    }
                                    else
                                    {
                                        monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                    }
                                }
                            }
                            else if (endDate.Year == year)
                            {
                                if (startDate.Year != year)
                                {
                                    startDate = new DateTime(endDate.Year, 01, 01);
                                    differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDate.AddMonths(element)).TakeWhile(element => element <= endDate).Select(element => element.ToString("MM"));

                                    int q1counter = 0;
                                    int q2counter = 0;
                                    int q3counter = 0;
                                    int q4counter = 0;
                                    foreach (string objDifference in differenceItems)
                                    {
                                        monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
                                        //
                                        categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
                                        string strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNo);
                                        int CurrentQuarter = (int)Math.Floor(((decimal)(monthNo) + 2) / 3);
                                        if (CurrentQuarter == 1)
                                        {
                                            if (monthNo <= 3)
                                            {
                                                if (q1counter == 0)
                                                {
                                                    monthArrayactivity[2] = q1 + 1;
                                                    q1 = monthArrayactivity[2];
                                                    q1counter++;
                                                }
                                            }
                                        }
                                        else if (CurrentQuarter == 2)
                                        {
                                            if (monthNo <= 6)
                                            {
                                                if (q2counter == 0)
                                                {
                                                    monthArrayactivity[5] = q2 + 1;
                                                    q2 = monthArrayactivity[5];
                                                    q2counter++;
                                                }
                                            }
                                        }
                                        else if (CurrentQuarter == 3)
                                        {
                                            if (monthNo <= 9)
                                            {
                                                if (q3counter == 0)
                                                {
                                                    monthArrayactivity[8] = q3 + 1;
                                                    q3 = monthArrayactivity[8];
                                                    q3counter++;
                                                }
                                            }
                                        }
                                        else if (CurrentQuarter == 4)
                                        {
                                            if (monthNo <= 12)
                                            {
                                                if (q4counter == 0)
                                                {
                                                    monthArrayactivity[11] = q4 + 1;
                                                    q4 = monthArrayactivity[11];
                                                    q4counter++;
                                                }
                                            }
                                        }
                                        //


                                        if (monthNo == 1)
                                        {
                                            monthArray[0] = monthArray[0] + 1;
                                        }
                                        else
                                        {
                                            monthArray[monthNo - 1] = monthArray[monthNo - 1] + 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //// Prepare Activity Chart list

                string quater = string.Empty;
                lstActivityChart = new List<ActivityChart>();
                for (int month = 0; month < monthArrayactivity.Count(); month++)
                {
                    ActivityChart objActivityChart = new ActivityChart();
                    categories = new List<string>() { "Q1", "Q2", "Q3", "Q4" };
                    string strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month + 1);
                    int CurrentQuarter = (int)Math.Floor(((decimal)(month + 1) + 2) / 3);
                    if (CurrentQuarter == 1)
                    {
                        quater = categories[0];
                        if (month == 2)
                        {
                            if (monthArrayactivity[month] == 0)
                            {
                                objActivityChart.NoOfActivity = string.Empty;
                            }
                            else
                            {
                                objActivityChart.NoOfActivity = monthArrayactivity[month].ToString();
                            }
                        }
                        else
                        {
                            objActivityChart.NoOfActivity = string.Empty;
                        }
                    }
                    if (CurrentQuarter == 2)
                    {
                        quater = categories[1];
                        if (month == 5)
                        {
                            if (monthArrayactivity[month] == 0)
                            {
                                objActivityChart.NoOfActivity = string.Empty;
                            }
                            else
                            {
                                objActivityChart.NoOfActivity = monthArrayactivity[month].ToString();
                            }
                        }
                        else
                        {
                            objActivityChart.NoOfActivity = string.Empty;
                        }
                    }
                    if (CurrentQuarter == 3)
                    {
                        quater = categories[2];
                        if (month == 8)
                        {
                            if (monthArrayactivity[month] == 0)
                            {
                                objActivityChart.NoOfActivity = string.Empty;
                            }
                            else
                            {
                                objActivityChart.NoOfActivity = monthArrayactivity[month].ToString();
                            }
                        }
                        else
                        {
                            objActivityChart.NoOfActivity = string.Empty;
                        }
                    }
                    if (CurrentQuarter == 4)
                    {
                        quater = categories[3];
                        if (month == 11)
                        {
                            if (monthArrayactivity[month] == 0)
                            {
                                objActivityChart.NoOfActivity = string.Empty;
                            }
                            else
                            {
                                objActivityChart.NoOfActivity = monthArrayactivity[month].ToString();
                            }
                        }
                        else
                        {
                            objActivityChart.NoOfActivity = string.Empty;
                        }
                    }

                    if (month == 0)
                    {
                        objActivityChart.Month = quater;
                    }
                    else
                    {
                        if (month % 3 == 0)
                        {
                            objActivityChart.Month = quater;
                        }
                        else
                        {
                            objActivityChart.Month = string.Empty;
                        }
                    }
                    if (i == 1)
                    {
                        objActivityChart.Color = Common.ActivityNextYearChartColor;
                    }
                    else
                    {
                        objActivityChart.Color = Common.ActivityChartColor;
                    }

                    lstActivityChart.Add(objActivityChart);
                }
                lstActivitybothChart.AddRange(lstActivityChart);
            }
            return lstActivitybothChart;

        }
        #endregion

        #region GetOwnerName and TacticTypeName
        /// <summary>
        /// Added By: Komal Rawal.
        /// Action to get all UserNames and TacticType
        /// </summary>
        public string GettactictypeName(int TacticTypeID)
        {
            if (TacticTypeList.Count == 0 || TacticTypeList == null)
            {
                TacticTypeList = objDbMrpEntities.TacticTypes.Where(tt => tt.TacticTypeId == TacticTypeID && tt.IsDeleted == false).Select(tt => tt).ToList();
            }
            var TacticType = TacticTypeList.Where(tt => tt.TacticTypeId == TacticTypeID).Select(tt => tt.Title).FirstOrDefault();

            return TacticType;
        }

        public string GetOwnerName(string UserGuid)
        {
            var OwnerName = "";
            if (lstUsers.Count == 0 || lstUsers == null)
            {
                lstUsers = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId);
            }
            if (UserGuid != "")
            {


                var userName = lstUsers.Where(user => user.UserId.ToString() == UserGuid).Select(user => new
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


        #endregion

    }

    public class ProgressModel
    {
        public int PlanId { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

    public class TacticStageMapping
    {
        public int StageId { get; set; }
        public int CustomFieldId { get; set; }
        public string Status { get; set; }
        public List<Plan_Tactic> TacticList { get; set; }
        public DateTime minDate { get; set; }
    }
}
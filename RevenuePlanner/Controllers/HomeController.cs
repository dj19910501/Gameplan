using RevenuePlanner.Helpers;
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

/*
 *  Author: Manoj Limbachiya
 *  Created Date: 10/22/2013
 *  Screen: 002_000_home - Home
 *  Purpose: Home page  
  */
namespace RevenuePlanner.Controllers
{
    public class HomeController : CommonController
    {
        #region Variables

        private MRPEntities objDbMrpEntities = new MRPEntities();
        private const string GANTT_BAR_CSS_CLASS_PREFIX = "color";
        private BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
        private DateTime CalendarStartDate;
        private DateTime CalendarEndDate;

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
        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Home, int currentPlanId = 0, int planTacticId = 0, int planCampaignId = 0, int planProgramId = 0, bool isImprovement = false)
        {
            //// To get permission status for Plan create, By dharmraj PL #519
            ViewBag.IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            //// To get permission status for Add/Edit Actual, By dharmraj PL #519
            ViewBag.IsTacticActualsAddEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);

            //// Set viewbag for notification email shared link inspect popup
            ViewBag.ActiveMenu = activeMenu;
            ViewBag.ShowInspectForPlanTacticId = planTacticId;
            ViewBag.ShowInspectForPlanCampaignId = planCampaignId;
            ViewBag.ShowInspectForPlanProgramId = planProgramId;
            ViewBag.IsImprovement = isImprovement;

            //// Start - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021
            //// Check insepct popup shared link validation to open insepct pop up as per link
            if (activeMenu.Equals(Enums.ActiveMenu.Plan) && currentPlanId > 0)
            {
                currentPlanId = InspectPopupSharedLinkValidation(currentPlanId, planCampaignId, planProgramId, planTacticId, isImprovement);
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
            ViewBag.BusinessUnitTitle = string.Empty;

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

            //// Get active model list of above selected business units 
            List<Model> models = objDbMrpEntities.Models.Where(model => model.ClientId.Equals(Sessions.User.ClientId) && model.IsDeleted == false).ToList();

            //// Get modelIds
            List<int> modelIds = models.Select(m => m.ModelId).ToList();
            //// Get list of active plan by selected modelId list
            List<Plan> activePlan = objDbMrpEntities.Plans.Where(p => modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true) && p.IsDeleted == false).ToList();

            if (Enums.ActiveMenu.Home.Equals(activeMenu))
            {
                //// Get list of Active(published) plans for all above models
                string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
                activePlan = activePlan.Where(plan => plan.Status.Equals(planPublishedStatus) && plan.IsDeleted == false).ToList();

                if (activePlan.Count == 0)
                {
                    //// Get active model of client id
                    models = objDbMrpEntities.Models.Where(m => m.ClientId == Sessions.User.ClientId && m.IsDeleted == false).ToList();

                    //// Get modelIds
                    modelIds = models.Select(m => m.ModelId).ToList();

                    activePlan = objDbMrpEntities.Plans.Where(plan => modelIds.Contains(plan.Model.ModelId) && plan.IsActive.Equals(true) && plan.IsDeleted.Equals(false)).ToList();
                    activePlan = activePlan.Where(plan => plan.Status.Equals(planPublishedStatus)).ToList();
                }
            }

            //// Added by Bhavesh, Current year first plan select in dropdown
            string currentYear = DateTime.Now.Year.ToString();
            if (activePlan.Count() != 0)
            {
                try
                {
                    Plan currentPlan = new Plan();
                    Plan latestPlan = new Plan();
                    latestPlan = activePlan.OrderBy(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).Select(plan => plan).FirstOrDefault();
                    if (activePlan.Where(plan => Convert.ToInt32(plan.Year) < Convert.ToInt32(currentYear)).Count() > 0)
                    {
                        latestPlan = activePlan.Where(plan => Convert.ToInt32(plan.Year) < Convert.ToInt32(currentYear)).OrderByDescending(plan => Convert.ToInt32(plan.Year)).ThenBy(plan => plan.Title).Select(plan => plan).FirstOrDefault();
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
                            //// Added by Bhavesh, Current year first plan select in dropdown
                            if (activePlan.Where(plan => plan.Year == currentYear).Count() > 0)
                            {
                                currentPlan = activePlan.Where(plan => plan.Year == currentYear).OrderBy(plan => plan.Title).FirstOrDefault();
                            }
                            else
                            {
                                currentPlan = latestPlan;
                            }
                        }
                        else
                        {
                            currentPlan = activePlan.Where(plan => plan.PlanId.Equals(Sessions.PublishedPlanId)).OrderBy(plan => plan.Title).FirstOrDefault();
                            //// when old published plan id (session) is from different business unit then it retune null
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
                            //// Added by Bhavesh, Current year first plan select in dropdown
                            if (activePlan.Where(plan => plan.Year == currentYear).Count() > 0)
                            {
                                currentPlan = activePlan.Where(plan => plan.Year == currentYear).OrderBy(plan => plan.Title).FirstOrDefault();
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

                    //// added by Nirav for plan consistency on 14 apr 2014
                    //////Sessions.BusinessUnitId = currentPlan.Model.BusinessUnitId;
                    planmodel.PlanTitle = currentPlan.Title;
                    planmodel.PlanId = currentPlan.PlanId;
                    planmodel.objplanhomemodelheader = Common.GetPlanHeaderValue(currentPlan.PlanId);

                    Sessions.PlanId = planmodel.PlanId;

                    //// Start - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021
                    if (ViewBag.ShowInspectPopup != null)
                    {
                        if ((bool)ViewBag.ShowInspectPopup == true && activeMenu.Equals(Enums.ActiveMenu.Plan) && currentPlanId > 0)
                        {
                            bool isCustomRestrictionPass = InspectPopupSharedLinkValidationForCustomRestriction(planCampaignId, planProgramId, planTacticId, isImprovement);
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
        public ActionResult HomePlan(int currentPlanId, string activeMenu)
        {
            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, activeMenu.ToLower());
            HomePlan objHomePlan = new HomePlan();
            List<SelectListItem> planList;

            //// if condition added to dispaly only published plan on home page
            if (objactivemenu.Equals(Enums.ActiveMenu.Plan))
            {
                var lstPlanAll = Common.GetPlan();
                planList = lstPlanAll.Select(plan => new SelectListItem() { Text = plan.Title, Value = plan.PlanId.ToString() }).OrderBy(plan => plan.Text).ToList();

                var objexists = planList.Where(plan => plan.Value == currentPlanId.ToString()).ToList();
                if (objexists.Count != 0)
                {
                    planList.Single(plan => plan.Value.Equals(currentPlanId.ToString())).Selected = true;
                }
                else
                {
                    planList.FirstOrDefault().Selected = true;
                }

                //// Set Plan dropdown values
                if (planList != null)
                    planList = planList.Where(plan => !string.IsNullOrEmpty(plan.Text)).OrderBy(plan => plan.Text, new AlphaNumericComparer()).ToList();
                objHomePlan.plans = planList;
            }

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
        /// <param name="businessunitIds">comma separated business unit ids from search filter</param>
        /// <param name="customFieldIds">comma separated custom field ids from search filter</param>
        /// <param name="ownerIds">comma separated Owner ids from search filter</param>
        /// <param name="activeMenu">current/selected active menu</param>
        /// <param name="getViewByList">flag to retrieve viewby list</param>
        /// <returns>Returns json result object of tactic type and plan campaign program tactic.</returns>
        public JsonResult GetViewControlDetail(string viewBy, string planId, string timeFrame, string businessunitIds, string customFieldIds, string ownerIds, string activeMenu, bool getViewByList)
        {
            ////// Get BusinessUnits selected  in search filter
            //List<Guid> filteredBusinessUnitIds = string.IsNullOrWhiteSpace(businessunitIds) ? new List<Guid>() : businessunitIds.Split(',').Select(businessUnit => Guid.Parse(businessUnit)).ToList();

            //// Create plan list based on PlanIds and Businessunit Ids of search filter
            List<int> planIds = string.IsNullOrWhiteSpace(planId) ? new List<int>() : planId.Split(',').Select(plan => int.Parse(plan)).ToList();
            var lstPlans = objDbMrpEntities.Plans.Where(plan => planIds.Contains(plan.PlanId) && plan.IsDeleted.Equals(false) && plan.Model.ClientId.Equals(Sessions.User.ClientId)).Select(plan => new { plan.PlanId, plan.Model.ClientId, plan.Year }).ToList();

            //// Custom Restriction for BusinessUnit
            //List<UserCustomRestrictionModel> lstUserCustomRestriction = new List<UserCustomRestrictionModel>();
            //List<string> lstAllowedBusinessUnits = new List<string>();
            //try
            //{
            //    lstUserCustomRestriction = Common.GetUserCustomRestriction();
            //    lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList(lstUserCustomRestriction);
            //}
            //catch (Exception objException)
            //{
            //    ErrorSignal.FromCurrentContext().Raise(objException);

            //    if (objException is System.ServiceModel.EndpointNotFoundException)
            //    {
            //        //// Flag to indicate unavailability of web service.
            //        //// Added By: Maninder Singh Wadhva on 11/24/2014.
            //        //// Ticket: 942 Exception handeling in Gameplan.
            //        return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
            //    }
            //}

            //if (lstAllowedBusinessUnits.Count > 0)
            //{
            //    //// Get list plans based on allowed business units
            //    List<Guid> businessUnitIds = new List<Guid>();
            //    lstAllowedBusinessUnits.ForEach(businessUnit => businessUnitIds.Add(Guid.Parse(businessUnit)));
            //    lstPlans = lstPlans.Where(plan => businessUnitIds.Contains(plan.BusinessUnitId)).Select(plan => plan).ToList();
            //}

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

            //// Get list of planIds from filtered plans
            var filteredPlanIds = lstPlans.Where(plan => plan.Year == planYear).ToList().Select(plan => plan.PlanId).ToList();

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

            //// Start - Added by Sohel Pathan on 16/01/2015 for PL ticket #1134
            //// Custom Field Filter Criteria.
            List<string> filteredCustomFields = string.IsNullOrWhiteSpace(customFieldIds) ? new List<string>() : customFieldIds.Split(',').Select(customFieldId => customFieldId.ToString()).ToList();
            List<int> lstEntityIds = new List<int>();
            bool recordsFound = true;
            if (filteredCustomFields.Count > 0)
            {
                //// get Allowed Entity Ids
                lstEntityIds = GetAllowedEntitiesBasedonCustomFieldFilterParameter(filteredCustomFields, out recordsFound);
            }
            //// End - Added by Sohel Pathan on 16/01/2015 for PL ticket #1134

            //// Applying filters to tactic (IsDelete, Geography, Individuals and Show My Tactic)
            List<Plan_Tactic> lstTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) &&
                                                                       lstProgramId.Contains(tactic.PlanProgramId) &&
                //// Start - Added by Sohel Pathan on 16/01/2015 for PL ticket #1134
                                                                        recordsFound == true &&
                                                                        (lstEntityIds.Count.Equals(0) || lstEntityIds.Contains(tactic.PlanTacticId)) &&
                //// End - Added by Sohel Pathan on 16/01/2015 for PL ticket #1134
                                                                       (filterOwner.Count.Equals(0) || filterOwner.Contains(tactic.CreatedBy))
                                                                        && (!((tactic.EndDate < CalendarStartDate) || (tactic.StartDate > CalendarEndDate)))
                                                                       )
                                                                       .ToList().Select(tactic => new Plan_Tactic
                                                                       {
                                                                           objPlanTactic = tactic,
                                                                           PlanCampaignId = tactic.Plan_Campaign_Program.PlanCampaignId,
                                                                           PlanId = tactic.Plan_Campaign_Program.Plan_Campaign.PlanId,
                                                                           objPlanTacticProgram = tactic.Plan_Campaign_Program,
                                                                           objPlanTacticCampaign = tactic.Plan_Campaign_Program.Plan_Campaign,
                                                                           objPlanTacticCampaignPlan = tactic.Plan_Campaign_Program.Plan_Campaign.Plan,
                                                                           TacticType = tactic.TacticType
                                                                       }).ToList();

            //// Apply Custom restriction for None type
            if (lstTactic.Count() > 0)
            {
                List<int> lstTacticIds = lstTactic.Select(tactic => tactic.objPlanTactic.PlanTacticId).ToList();
                List<int> lstAllowedEntityIds = Common.GetAllowedCustomFieldEntityList(Sessions.User.UserId, Sessions.User.ClientId, lstTacticIds, false);
                lstTactic = lstTactic.Where(tactic => lstAllowedEntityIds.Contains(tactic.objPlanTactic.PlanTacticId)).Select(tactic => tactic).ToList();
            }

            //// Modified By Maninder Singh Wadhva PL Ticket#47
            //// Get list of Improvement tactics based on selected plan ids
            List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic = objDbMrpEntities.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => filteredPlanIds.Contains(improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId) &&
                                                                                      improvementTactic.IsDeleted.Equals(false) &&
                                                                                      (improvementTactic.EffectiveDate > CalendarEndDate).Equals(false))
                                                                               .Select(improvementTactic => improvementTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

            //// Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
            //// Apply custom restriction on selected tactics list
            //int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
            //int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            //var lstAllowedVertical = lstUserCustomRestriction.Where(customRestriction => (customRestriction.Permission == ViewOnlyPermission || customRestriction.Permission == ViewEditPermission) && customRestriction.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(customRestriction => customRestriction.CustomFieldId).ToList();
            //var lstAllowedGeography = lstUserCustomRestriction.Where(customRestriction => (customRestriction.Permission == ViewOnlyPermission || customRestriction.Permission == ViewEditPermission) && customRestriction.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(customRestriction => customRestriction.CustomFieldId.ToString().ToLower()).ToList();////Modified by Mitesh Vaishnav For functional review point 89
            //lstTactic = lstTactic.Where(tactic => lstAllowedVertical.Contains(tactic.objPlanTactic.VerticalId.ToString()) && lstAllowedGeography.Contains(tactic.objPlanTactic.GeographyId.ToString().ToLower())).ToList();////Modified by Mitesh Vaishnav For functional review point 89

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
            string tacticStatus = Enums.TacticStatusValues.Single(status => status.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
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
                List<string> status = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                List<string> statusCD = new List<string>();
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

                lstTactic = lstTactic.Where(t => status.Contains(t.objPlanTactic.Status) || ((t.objPlanTactic.CreatedBy == Sessions.User.UserId && !viewBy.Equals(PlanGanttTypes.Request.ToString())) ? statusCD.Contains(t.objPlanTactic.Status) : false))
                                .Select(planTactic => planTactic).ToList<Plan_Tactic>();

                List<string> improvementTacticStatus = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                lstImprovementTactic = lstImprovementTactic.Where(improvementTactic => improvementTacticStatus.Contains(improvementTactic.Status) || ((improvementTactic.CreatedBy == Sessions.User.UserId && !viewBy.Equals(PlanGanttTypes.Request.ToString())) ? statusCD.Contains(improvementTactic.Status) : false))
                                                           .Select(improvementTactic => improvementTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();
            }
            else if (objactivemenu.Equals(Enums.ActiveMenu.Home))
            {
                List<string> status = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                lstTactic = lstTactic.Where(t => status.Contains(t.objPlanTactic.Status)).Select(planTactic => planTactic).ToList<Plan_Tactic>();

                List<string> improvementTacticStatus = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                lstImprovementTactic = lstImprovementTactic.Where(improveTactic => improvementTacticStatus.Contains(improveTactic.Status))
                                                           .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

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

            try
            {
                if (viewBy.Equals(PlanGanttTypes.Tactic.ToString(), StringComparison.OrdinalIgnoreCase) || viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
                {

                    return PrepareTacticAndRequestTabResult(viewBy, objactivemenu, lstCampaign.ToList(), lstProgram.ToList(), lstTactic.ToList(), lstImprovementTactic, requestCount, planYear, improvementTacticForAccordion, improvementTacticTypeForAccordion, viewByListResult);
                }
                else
                {
                    return PrepareCustomFieldTabResult(viewBy, objactivemenu, lstCampaign.ToList(), lstProgram.ToList(), lstTactic.ToList(), lstImprovementTactic, requestCount, planYear, improvementTacticForAccordion, improvementTacticTypeForAccordion, viewByListResult);
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

            return Json(new { }, JsonRequestBehavior.AllowGet);
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
        private JsonResult PrepareCustomFieldTabResult(string viewBy, Enums.ActiveMenu activemenu, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion, List<ViewByModel> viewByListResult)
        {
            string sourceViewBy = viewBy;
            int CustomTypeId = 0;
            bool IsCampaign = viewBy.Contains(Common.CampaignCustomTitle) ? true : false;
            bool IsProgram = viewBy.Contains(Common.ProgramCustomTitle) ? true : false;
            bool IsTactic = viewBy.Contains(Common.TacticCustomTitle) ? true : false;
            string entityType = Enums.EntityType.Tactic.ToString();

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
            int tacticPlanId = 0;
            DateTime MinStartDateForCustomField;
            DateTime MinStartDateForPlan;

            //// Prepare task tactic list for CustomFields tab(ViewBy)

            //// Get list of tactic ids from tactic list
            List<int> tacticIdList = lstTactic.Select(tactic => tactic.objPlanTactic.PlanTacticId).ToList().Distinct().ToList<int>();

            CustomFieldOption objCustomFieldOption = new CustomFieldOption();
            objCustomFieldOption.CustomFieldOptionId = 0;
            string DropDownList = Enums.CustomFieldType.DropDownList.ToString();

            //// Fetch list of tactic for selected CustomField whether CustomFieldType is textBox or Dropdown
            var lstCustomFieldTactic = (from customfield in objDbMrpEntities.CustomFields
                                        join customfieldtype in objDbMrpEntities.CustomFieldTypes on customfield.CustomFieldTypeId equals customfieldtype.CustomFieldTypeId
                                        join customfieldentity in objDbMrpEntities.CustomField_Entity on customfield.CustomFieldId equals customfieldentity.CustomFieldId
                                        join tactic in objDbMrpEntities.Plan_Campaign_Program_Tactic on customfieldentity.EntityId equals
                                            (IsCampaign ? tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? tactic.PlanProgramId : tactic.PlanTacticId))
                                        join customfieldoptionLeftJoin in objDbMrpEntities.CustomFieldOptions on new { Key1 = customfield.CustomFieldId, Key2 = customfieldentity.Value.Trim() } equals
                                            new { Key1 = customfieldoptionLeftJoin.CustomFieldId, Key2 = SqlFunctions.StringConvert((double)customfieldoptionLeftJoin.CustomFieldOptionId).Trim() } into cAll
                                        from cfo in cAll.DefaultIfEmpty()
                                        where customfield.IsDeleted == false && tactic.IsDeleted == false && customfield.EntityType == entityType && customfield.CustomFieldId == CustomTypeId &&
                                        customfield.ClientId == Sessions.User.ClientId && tacticIdList.Contains(tactic.PlanTacticId)
                                        select new
                                        {
                                            tactic = tactic,
                                            masterCustomFieldId = customfield.CustomFieldId,
                                            customFieldId = customfieldtype.Name == DropDownList ? (cfo.CustomFieldOptionId == null ? 0 : cfo.CustomFieldOptionId) : customfieldentity.CustomFieldEntityId,
                                            customFieldTitle = customfieldtype.Name == DropDownList ? cfo.Value : customfieldentity.Value,
                                            customFieldTYpe = customfieldtype.Name,
                                            EntityId = customfieldentity.EntityId,
                                        }).ToList().Distinct().ToList();

            //// Process CustomFieldTactic list retrieved from DB for further use
            var lstProcessedCustomFieldTactics = lstCustomFieldTactic.Select(customFieldTactic => new
            {
                tactic = customFieldTactic.tactic,
                masterCustomFieldId = customFieldTactic.masterCustomFieldId,
                customFieldId = lstCustomFieldTactic.Where(customTactic => customTactic.masterCustomFieldId == customFieldTactic.masterCustomFieldId && customTactic.customFieldTitle.Trim() == customFieldTactic.customFieldTitle.Trim()).Select(customTactic => customTactic.customFieldId).First(),
                customFieldTitle = customFieldTactic.customFieldTitle,
                customFieldTacticList = customFieldTactic.customFieldTYpe == DropDownList ? lstCustomFieldTactic.Where(customTactic => customTactic.customFieldId == customFieldTactic.customFieldId).Select(tt => tt.EntityId) : lstCustomFieldTactic.Where(customTactic => customTactic.customFieldTitle == customFieldTactic.customFieldTitle).Select(customTactic => customTactic.EntityId),
            }).ToList();

            DateTime MaxEndDateForCustomField;

            //// Prepare tactic task list for CustomField to be used in rendering of gantt chart. 
            foreach (var tacticItem in lstProcessedCustomFieldTactics)
            {
                tacticPlanId = tacticItem.tactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
                tacticListByViewById = lstTactic.Where(tactic => tacticItem.customFieldTacticList.Contains(IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId))).Select(tactic => tactic).ToList();
                MinStartDateForCustomField = GetMinStartDateForCustomField(PlanGanttTypes.Custom, IsCampaign ? tacticItem.tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? tacticItem.tactic.PlanProgramId : tacticItem.tactic.PlanTacticId),
                                                                            lstCampaign, lstProgram, tacticListByViewById, IsCampaign, IsProgram);
                MaxEndDateForCustomField = GetMaxEndDateForCustomField(PlanGanttTypes.Custom, IsCampaign ? tacticItem.tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? tacticItem.tactic.PlanProgramId : tacticItem.tactic.PlanTacticId),
                                                                            lstCampaign, lstProgram, tacticListByViewById, IsCampaign, IsProgram);
                MinStartDateForPlan = GetMinStartDateForPlanOfCustomField(viewBy, IsCampaign ? tacticItem.tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? tacticItem.tactic.PlanProgramId : tacticItem.tactic.PlanTacticId),
                                                                            tacticPlanId, lstCampaign, lstProgram, tacticListByViewById, IsCampaign, IsProgram);

                lstTacticTaskList.Add(new TacticTaskList()
                {
                    Tactic = tacticItem.tactic,
                    CustomFieldId = tacticItem.customFieldId.ToString(),
                    CustomFieldTitle = tacticItem.customFieldTitle,
                    TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", tacticItem.customFieldId, tacticPlanId, tacticItem.tactic.Plan_Campaign_Program.PlanCampaignId, tacticItem.tactic.PlanProgramId, tacticItem.tactic.PlanTacticId),
                    ColorCode = Common.ColorCodeForCustomField,
                    StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                    EndDate = Common.GetEndDateAsPerCalendarInDateFormat(CalendarEndDate, MaxEndDateForCustomField),
                    Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField, MaxEndDateForCustomField),
                    CampaignProgress = GetCampaignProgress(tacticListByViewById, tacticItem.tactic.Plan_Campaign_Program.Plan_Campaign, lstImprovementTactic, tacticPlanId),
                    ProgramProgress = GetProgramProgress(tacticListByViewById, tacticItem.tactic.Plan_Campaign_Program, lstImprovementTactic, tacticPlanId),
                    PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlan),
                                                Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlan,
                                                    GetMaxEndDateForPlanOfCustomFields(viewBy, IsCampaign ? tacticItem.tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? tacticItem.tactic.PlanProgramId : tacticItem.tactic.PlanTacticId),
                                                    tacticPlanId, lstCampaign, lstProgram, tacticListByViewById, IsCampaign, IsProgram))
                                                , tacticListByViewById, lstImprovementTactic, tacticPlanId),
                    lstCustomEntityId = tacticItem.customFieldTacticList.ToList(),
                });
            }

            //// Prepare CustomFields list for Marketting accrodian for Home screen only.
            if (activemenu.Equals(Enums.ActiveMenu.Home))
            {
                lstCustomFields = (lstProcessedCustomFieldTactics.Select(tactic => tactic)).Select(tactic => new
                {
                    CustomFieldId = tactic.customFieldId,
                    Title = tactic.customFieldTitle,
                    ColorCode = Common.ColorCodeForCustomField.ToString()
                }).ToList().Distinct().Select(tactic => new CustomFields()
                {
                    CustomFieldId = tactic.CustomFieldId.ToString(),
                    Title = tactic.Title,
                    ColorCode = tactic.ColorCode
                }).ToList().OrderBy(customField => customField.Title).ToList();
            }

            //// Prepare ImprovementTactic list that relates to selected tactic and customFields
            lstImprovementTaskDetails = (from improvementTactic in lstImprovementTactic
                                         join customfieldtactic in lstProcessedCustomFieldTactics on improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals customfieldtactic.tactic.Plan_Campaign_Program.Plan_Campaign.PlanId
                                         join customfield in objDbMrpEntities.CustomFields on customfieldtactic.masterCustomFieldId equals customfield.CustomFieldId
                                         where improvementTactic.IsDeleted == false && customfieldtactic.tactic.IsDeleted == false && customfield.IsDeleted == false
                                         select new ImprovementTaskDetail()
                                         {
                                             ImprovementTactic = improvementTactic,
                                             MainParentId = customfieldtactic.customFieldId.ToString(),
                                             MinStartDate = lstImprovementTactic.Where(impTactic => impTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impTactic => impTactic.EffectiveDate).Min(),
                                         }).Select(improvementTactic => improvementTactic).ToList().Distinct().ToList();



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
                Color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, tacticTask.ColorCode.ToLower()),
                PlanId = tacticTask.Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId,
                PlanTitle = tacticTask.Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Title,
                Campaign = tacticTask.Tactic.Plan_Campaign_Program.Plan_Campaign,
                Program = tacticTask.Tactic.Plan_Campaign_Program,
                Tactic = tacticTask.Tactic,
                PlanProgress = tacticTask.PlanProgrss,
                CampaignProgress = tacticTask.CampaignProgress,
                ProgramProgress = tacticTask.ProgramProgress,
                lstCustomEntityId = tacticTask.lstCustomEntityId,
            }).ToList().Distinct().ToList();

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
                color = tacticTask.Color
            }).Select(v => v).Distinct().OrderBy(tacticTask => tacticTask.text);

            //// Group same task for Custom Field by CustomFieldId
            var groupedCustomField = taskDataCustomeFields.GroupBy(groupedTask => new { id = groupedTask.id }).Select(groupedTask => new
            {
                id = groupedTask.Key.id,
                text = groupedTask.Select(task => task.text).FirstOrDefault(),
                start_date = groupedTask.Select(task => task.start_date).ToList().Min(),
                end_date = groupedTask.Select(task => task.end_date).ToList().Max(),
                duration = groupedTask.Select(task => task.duration).ToList().Max(),
                progress = groupedTask.Select(task => task.progress).FirstOrDefault(),
                open = groupedTask.Select(task => task.open).FirstOrDefault(),
                color = groupedTask.ToList().Select(task => task.color).FirstOrDefault() + ((groupedTask.ToList().Select(task => task.progress).FirstOrDefault() > 0) ? "stripe" : string.Empty)
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
                color = taskdata.color + ((taskdata.progress > 0) ? "stripe" : string.Empty)
            });
            #endregion

            #region Prepare Plan task data
            var taskDataPlan = lstTaskDetails.Select(taskdata => new
            {
                id = string.Format("Z{0}_L{1}", taskdata.MainParentId, taskdata.PlanId),
                text = taskdata.PlanTitle,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlanOfCustomField(viewBy,
                            ((viewBy.Equals(PlanGanttTypes.BusinessUnit.ToString(), StringComparison.OrdinalIgnoreCase) ||
                                viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)) ? IsCampaign ? taskdata.Tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? taskdata.Tactic.PlanProgramId : taskdata.Tactic.PlanTacticId)
                            : Convert.ToInt32(taskdata.MainParentId)), taskdata.PlanId, lstCampaign, lstProgram, (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)) ? lstTactic.Where(tactic => taskdata.lstCustomEntityId.Contains(IsCampaign ? tactic.PlanCampaignId
                                    : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId))).Select(tactic => tactic).ToList() : lstTactic, IsCampaign, IsProgram)),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForPlanOfCustomField(viewBy,
                                                          ((viewBy.Equals(PlanGanttTypes.BusinessUnit.ToString(), StringComparison.OrdinalIgnoreCase) ||
                                                          (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)))
                                                          ?
                                                          IsCampaign ? taskdata.Tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? taskdata.Tactic.PlanProgramId : taskdata.Tactic.PlanTacticId)
                                                          : Convert.ToInt32(taskdata.MainParentId)), taskdata.PlanId, lstCampaign, lstProgram,
                                                          (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)) ? lstTactic.Where(tactic => taskdata.lstCustomEntityId.Contains(IsCampaign ? tactic.PlanCampaignId
                                                        : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId))).Select(tactic => tactic).ToList() : lstTactic, IsCampaign, IsProgram),
                                                          GetMaxEndDateForPlanOfCustomFields(viewBy,
                                                          ((viewBy.Equals(PlanGanttTypes.BusinessUnit.ToString(), StringComparison.OrdinalIgnoreCase) ||
                                                          (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)))
                                                          ?
                                                          IsCampaign ? taskdata.Tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? taskdata.Tactic.PlanProgramId : taskdata.Tactic.PlanTacticId)
                                                          : Convert.ToInt32(taskdata.MainParentId)), taskdata.PlanId, lstCampaign, lstProgram,
                                                          (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)) ? lstTactic.Where(tactic => taskdata.lstCustomEntityId.Contains(IsCampaign ? tactic.PlanCampaignId
                                                        : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId))).Select(tactic => tactic).ToList() : lstTactic, IsCampaign, IsProgram)),
                progress = taskdata.PlanProgress,
                open = false,
                parent = string.Format("Z{0}", taskdata.MainParentId),
                color = GetColorBasedOnImprovementActivity(lstImprovementTactic, taskdata.PlanId),
                planid = taskdata.PlanId
            }).Select(taskdata => taskdata).Distinct().OrderBy(taskdata => taskdata.text);

            //// Finalize Plan task data to be render in gantt chart
            var lstPlanTaskdata = taskDataPlan.Select(taskdata => new
            {
                id = taskdata.id,
                text = taskdata.text,
                start_date = taskdata.start_date,
                duration = taskdata.duration,
                progress = taskDataPlan.Where(plan => plan.id == taskdata.id).Select(plan => plan.progress).Min(),
                open = taskdata.open,
                parent = taskdata.parent,
                color = taskdata.color + (taskdata.progress > 0 ? "stripe" : string.Empty),
                planid = taskdata.planid
            }).ToList().Distinct().ToList();
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
                color = GetColorBasedOnImprovementActivity(lstImprovementTactic, taskdata.PlanId),
                plancampaignid = taskdata.Program.PlanCampaignId,
                Status = taskdata.Campaign.Status
            }).Select(taskdata => taskdata).Distinct().OrderBy(taskdata => taskdata.text);

            //// Finalize Campaign task data to be render in gantt chart
            var lstCampaignTaskData = taskDataCampaign.Select(taskdata => new
            {
                id = taskdata.id,
                text = taskdata.text,
                start_date = taskdata.start_date,
                duration = taskdata.duration,
                progress = taskDataCampaign.Where(campaign => campaign.id == taskdata.id).Select(campaign => campaign.progress).Min(),
                open = taskdata.open,
                parent = taskdata.parent,
                color = taskdata.color + (taskdata.progress == 1 ? " stripe" : (taskdata.progress > 0 ? "stripe" : string.Empty)),
                plancampaignid = taskdata.plancampaignid,
                Status = taskdata.Status
            }).ToList().Distinct().ToList();
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
                color = string.Empty,
                planprogramid = taskdata.Program.PlanProgramId,
                Status = taskdata.Program.Status
            }).Select(taskdata => taskdata).Distinct().OrderBy(taskdata => taskdata.text);

            //// Finalize Program task data to be render in gantt chart
            var lstProgramTaskData = taskDataProgram.Select(taskdata => new
            {
                id = taskdata.id,
                text = taskdata.text,
                start_date = taskdata.start_date,
                duration = taskdata.duration,
                progress = taskDataProgram.Where(program => program.id == taskdata.id).Select(program => program.progress).Min(),
                open = taskdata.open,
                parent = taskdata.parent,
                color = (taskdata.progress == 1 ? " stripe stripe-no-border " : (taskdata.progress > 0 ? "partialStripe" : string.Empty)),
                planprogramid = taskdata.planprogramid,
                Status = taskdata.Status
            }).ToList().Distinct().ToList();
            #endregion

            #region Prepare Tactic task data
            var taskDataTactic = lstTaskDetails.Select(taskdata => new
            {
                id = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", taskdata.MainParentId, taskdata.PlanId, taskdata.Program.PlanCampaignId, taskdata.Program.PlanProgramId, taskdata.Tactic.PlanTacticId),
                text = taskdata.Tactic.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, taskdata.Tactic.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, taskdata.Tactic.StartDate, taskdata.Tactic.EndDate),
                progress = GetTacticProgress(taskdata.Tactic, lstImprovementTactic, taskdata.PlanId),
                open = false,
                parent = string.Format("Z{0}_L{1}_C{2}_P{3}", taskdata.MainParentId, taskdata.PlanId, taskdata.Program.PlanCampaignId, taskdata.Tactic.PlanProgramId),
                color = taskdata.Color,
                plantacticid = taskdata.Tactic.PlanTacticId,
                Status = taskdata.Tactic.Status
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
                color = taskdata.color + (taskdata.progress == 1 ? " stripe" : string.Empty),
                plantacticid = taskdata.plantacticid,
                Status = taskdata.Status
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
                color = GetColorBasedOnImprovementActivity(lstImprovementTactic, taskdataImprovement.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                ImprovementActivityId = taskdataImprovement.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
                isImprovement = true,
                IsHideDragHandleLeft = taskdataImprovement.MinStartDate < CalendarStartDate,
                IsHideDragHandleRight = true,
                parent = string.Format("Z{0}_L{1}", taskdataImprovement.MainParentId, taskdataImprovement.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
            }).Select(taskdataImprovement => taskdataImprovement).Distinct().ToList();

            string tacticStatusSubmitted = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            string tacticStatusDeclined = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;

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
                color = string.Concat(Common.GANTT_BAR_CSS_CLASS_PREFIX_IMPROVEMENT, taskdataImprovement.ImprovementTactic.ImprovementTacticType.ColorCode.ToLower()),
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
                    PlanTacticId = tactic.Tactic.PlanTacticId,
                    CustomFieldId = tactic.CustomFieldId,
                    Title = tactic.Tactic.Title,
                    TaskId = tactic.TaskId,
                }).ToList().Distinct().ToList();
                #endregion

                return Json(new
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
            }
            else if (activemenu.Equals(Enums.ActiveMenu.Plan))
            {
                return Json(new
                {
                    taskData = finalTaskData,
                    requestCount = requestCount,
                    planYear = planYear,
                    ViewById = viewByListResult,
                    ViewBy = sourceViewBy
                }, JsonRequestBehavior.AllowGet);
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
        private JsonResult PrepareTacticAndRequestTabResult(string viewBy, Enums.ActiveMenu activemenu, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion, List<ViewByModel> viewByListResult)
        {
            List<object> tacticAndRequestTaskData = GetTaskDetailTactic(viewBy, lstCampaign.ToList(), lstProgram.ToList(), lstTactic.ToList(), lstImprovementTactic);

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
                    ColorCode = tactic.ColorCode
                }).Distinct().OrderBy(tactic => tactic.Title);

                return Json(new
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
            }
            else if (activemenu.Equals(Enums.ActiveMenu.Plan))
            {
                return Json(new
                {
                    taskData = tacticAndRequestTaskData,
                    requestCount = requestCount,
                    planYear = planYear,
                    ViewById = viewByListResult,
                    ViewBy = viewBy
                }, JsonRequestBehavior.AllowGet);
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
                else if (IsPlanEditAllAuthorized)  // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                {
                    IsPlanEditable = true;
                }
                else if (IsPlanEditSubordinatesAuthorized)
                {
                    if (lstOwnAndSubOrdinates.Contains(objPlan.CreatedBy))    // Modified by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
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
            if (improvementTactics.Count() > 0)
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
        public List<object> GetTaskDetailTactic(string viewBy, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic)
        {
            string tacticStatusSubmitted = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            string tacticStatusDeclined = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;

            //// Added BY Bhavesh, Calculate MQL at runtime #376
            List<TacticStageValue> tacticStageRelationList = new List<TacticStageValue>();
            List<Stage> stageList = new List<Stage>();
            int inqLevel = 0;
            int mqlLevel = 0;

            if (viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                string inqstage = Enums.Stage.INQ.ToString();
                string mqlstage = Enums.Stage.MQL.ToString();
                tacticStageRelationList = Common.GetTacticStageRelation(lstTactic.Select(tactic => tactic.objPlanTactic).ToList(), false);
                stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == Sessions.User.ClientId).ToList();
                inqLevel = Convert.ToInt32(stageList.Single(stage => stage.Code == inqstage).Level);
                mqlLevel = Convert.ToInt32(stageList.Single(stage => stage.Code == mqlstage).Level);
            }

            #region Prepare Tactic Task Data
            //// Prepare task data tactic list for gantt chart
            var taskDataTactic = lstTactic.Select(tactic => new
            {
                id = string.Format("L{0}_C{1}_P{2}_T{3}_Y{4}", tactic.PlanId, tactic.PlanCampaignId, tactic.objPlanTactic.PlanProgramId, tactic.objPlanTactic.PlanTacticId, tactic.objPlanTactic.TacticTypeId),
                text = tactic.objPlanTactic.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.objPlanTactic.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, tactic.objPlanTactic.StartDate, tactic.objPlanTactic.EndDate),
                progress = GetTacticProgress(tactic.objPlanTactic, lstImprovementTactic, tactic.PlanId),
                open = false,
                parent = string.Format("L{0}_C{1}_P{2}", tactic.PlanId, tactic.PlanCampaignId, tactic.objPlanTactic.PlanProgramId),
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, tactic.TacticType.ColorCode.ToLower()),
                isSubmitted = tactic.objPlanTactic.Status == tacticStatusSubmitted,
                isDeclined = tactic.objPlanTactic.Status == tacticStatusDeclined,
                projectedStageValue = viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase) ? stageList.Single(s => s.StageId == tactic.objPlanTactic.StageId).Level <= inqLevel ? Convert.ToString(tacticStageRelationList.Single(tm => tm.TacticObj.PlanTacticId == tactic.objPlanTactic.PlanTacticId).INQValue) : "N/A" : "0",
                mqls = viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase) ? stageList.Single(s => s.StageId == tactic.objPlanTactic.StageId).Level <= mqlLevel ? Convert.ToString(tacticStageRelationList.Single(tacticStage => tacticStage.TacticObj.PlanTacticId == tactic.objPlanTactic.PlanTacticId).MQLValue) : "N/A" : "0",
                cost = tactic.objPlanTactic.Cost,
                cws = viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase) ? tactic.objPlanTactic.Status == tacticStatusSubmitted || tactic.objPlanTactic.Status == tacticStatusDeclined ? Math.Round(tacticStageRelationList.Single(tacticStage => tacticStage.TacticObj.PlanTacticId == tactic.objPlanTactic.PlanTacticId).RevenueValue, 1) : 0 : 0,
                plantacticid = tactic.objPlanTactic.PlanTacticId,
                Status = tactic.objPlanTactic.Status
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
                color = tactic.color + (tactic.progress == 1 ? " stripe" : string.Empty),
                isSubmitted = tactic.isSubmitted,
                isDeclined = tactic.isDeclined,
                projectedStageValue = tactic.projectedStageValue,
                mqls = tactic.mqls,
                cost = tactic.cost,
                cws = tactic.cws,
                plantacticid = tactic.plantacticid,
                Status = tactic.Status
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
                progress = GetProgramProgress(lstTactic, tactic.objPlanTacticProgram, lstImprovementTactic, tactic.PlanId),
                open = false,
                parent = string.Format("L{0}_C{1}", tactic.PlanId, tactic.PlanCampaignId),
                color = string.Empty,
                planprogramid = tactic.objPlanTactic.PlanProgramId,
                Status = tactic.objPlanTacticProgram.Status
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
                planprogramid = program.planprogramid,
                Status = program.Status
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
                progress = GetCampaignProgress(lstTactic, tactic.objPlanTacticCampaign, lstImprovementTactic, tactic.PlanId),
                open = false,
                parent = string.Format("L{0}", tactic.PlanId),
                color = GetColorBasedOnImprovementActivity(lstImprovementTactic, tactic.PlanId),
                plancampaignid = tactic.PlanCampaignId,
                Status = tactic.objPlanTacticCampaign.Status
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
                color = campaign.color + (campaign.progress == 1 ? " stripe" : (campaign.progress > 0 ? "stripe" : string.Empty)),
                plancampaignid = campaign.plancampaignid,
                Status = campaign.Status
            });
            #endregion

            #region Prepare Plan Task Data
            //// Prepare task data plan list for gantt chart
            var taskDataPlan = lstTactic.Select(tactic => new
            {
                id = string.Format("L{0}", tactic.PlanId),
                text = tactic.objPlanTacticCampaignPlan.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Tactic, tactic.objPlanTactic.PlanTacticId, tactic.PlanId, lstCampaign, lstProgram, lstTactic)),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForPlan(GanttTabs.Tactic, tactic.objPlanTactic.PlanTacticId, tactic.PlanId, lstCampaign, lstProgram, lstTactic),
                                                          GetMaxEndDateForPlan(GanttTabs.Tactic, tactic.objPlanTactic.PlanTacticId, tactic.PlanId, lstCampaign, lstProgram, lstTactic)),
                progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Tactic, tactic.objPlanTactic.PlanTacticId, tactic.PlanId, lstCampaign, lstProgram, lstTactic)),
                                                Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForPlan(GanttTabs.Tactic, tactic.objPlanTactic.PlanTacticId, tactic.PlanId, lstCampaign, lstProgram, lstTactic),
                                                          GetMaxEndDateForPlan(GanttTabs.Tactic, tactic.objPlanTactic.PlanTacticId, tactic.PlanId, lstCampaign, lstProgram, lstTactic)),
                                               lstTactic, lstImprovementTactic, tactic.PlanId),
                open = false,
                color = GetColorBasedOnImprovementActivity(lstImprovementTactic, tactic.PlanId),
                planid = tactic.PlanId
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
                color = plan.color + (plan.progress > 0 ? "stripe" : string.Empty),
                planid = plan.planid
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
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.None, improvementTactic.ImprovementPlanTacticId, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic)),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForPlan(GanttTabs.None, improvementTactic.ImprovementPlanTacticId, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic),
                                                          GetMaxEndDateForPlan(GanttTabs.None, improvementTactic.ImprovementPlanTacticId, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic)),
                progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.None, improvementTactic.ImprovementPlanTacticId, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic)),
                                                Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForPlan(GanttTabs.None, improvementTactic.ImprovementPlanTacticId, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic),
                                                          GetMaxEndDateForPlan(GanttTabs.None, improvementTactic.ImprovementPlanTacticId, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, lstCampaign, lstProgram, lstTactic)),
                                               lstTactic, lstImprovementTactic, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                open = false,
                color = GetColorBasedOnImprovementActivity(lstImprovementTactic, improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                planid = improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId
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
                color = improvementTactic.color + (improvementTactic.progress > 0 ? "stripe" : string.Empty),
                planid = improvementTactic.planid
            });
            #endregion

            //// Concat list of Plan task objects and Improvement Tactic task objects
            var taskDataPlanMerged = newTaskDataPlan.Concat<object>(newTaskDataPlanForImprovement).ToList().Distinct();

            #region Improvement Activities & Tactics
            //// Prepare list of Improvement Tactic and Activities list for gantt chart
            var improvemntTacticList = lstImprovementTactic.Select(improvementTactic => new
            {
                ImprovementTactic = improvementTactic,
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
                color = GetColorBasedOnImprovementActivity(lstImprovementTactic, improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                ImprovementActivityId = improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
                isImprovement = true,
                IsHideDragHandleLeft = improvementTactic.minStartDate < CalendarStartDate,
                IsHideDragHandleRight = true,
                parent = string.Format("L{0}", improvementTactic.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
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
                color = string.Concat(Common.GANTT_BAR_CSS_CLASS_PREFIX_IMPROVEMENT, improvementTacticActivty.ImprovementTactic.ImprovementTacticType.ColorCode.ToLower()),
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
                Status = improvementTacticActivty.ImprovementTactic.Status
            }).OrderBy(improvementTacticActivty => improvementTacticActivty.text);
            #endregion

            //// Contact all the task object and return as task object list to be rendered in gantt chart
            return taskDataPlanMerged.Concat<object>(taskDataImprovementActivity).Concat<object>(taskDataImprovementTactic).Concat<object>(newTaskDataCampaign).Concat<object>(NewTaskDataTactic).Concat<object>(newTaskDataProgram).ToList<object>();
        }

        /// <summary>
        /// Function to get tactic progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 2nd april, 2013
        /// <param name="planCampaignProgramTactic">tactic object</param>
        /// <param name="lstImprovementTactic">list of improvement tactics of selected plan</param>
        /// <param name="PlanId">planId of selected tactic</param>
        /// <returns>returns progress between 0 and 1</returns>
        public double GetTacticProgress(Plan_Campaign_Program_Tactic planCampaignProgramTactic, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic, int PlanId)
        {
            if (lstImprovementTactic.Count > 0 && lstImprovementTactic.Where(improvementTactic => improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Any())
            {
                //// Minimun date of improvement tactic
                DateTime minDate = lstImprovementTactic.Where(improvementTactic => improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Select(improvementTactic => improvementTactic.EffectiveDate).Min();

                //// Start Date of tactic
                DateTime tacticStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaignProgramTactic.StartDate));

                //// If any tactic affected by at least one improvement tactic.
                if (tacticStartDate >= minDate)
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
        /// <param name="lstTactic">list of tactics of selected program</param>
        /// <param name="planCampaignProgram">program object</param>
        /// <param name="lstImprovementTactic">list of improvement tactics of selected plan</param>
        /// <param name="PlanId">planId of selected program</param>
        /// <returns>returns progress between 0 and 1</returns>
        public double GetProgramProgress(List<Plan_Tactic> lstTactic, Plan_Campaign_Program planCampaignProgram, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic, int PlanId)
        {
            if (lstImprovementTactic.Count > 0 && lstImprovementTactic.Where(improvementTactic => improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Any())
            {
                //// Minimun date of improvement tactic
                DateTime minDate = lstImprovementTactic.Where(improvementTactic => improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Select(improvementTactic => improvementTactic.EffectiveDate).Min();

                //// Start date of program
                DateTime programStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaignProgram.StartDate));

                //// List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = lstTactic.Where(tactic => tactic.objPlanTactic.IsDeleted.Equals(false) && tactic.objPlanTactic.PlanProgramId == planCampaignProgram.PlanProgramId && (tactic.objPlanTactic.StartDate >= minDate).Equals(true)
                                                        && tactic.PlanId == PlanId)
                                                .Select(tactic => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.objPlanTactic.StartDate)) })
                                                .ToList();

                if (lstAffectedTactic.Count > 0)
                {
                    //// Minimum start Date of tactics
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(tactic => tactic.startDate).Min();

                    //// If any tactic affected by at least one improvement tactic
                    if (tacticMinStartDate >= minDate)
                    {
                        double programDuration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, planCampaignProgram.StartDate, planCampaignProgram.EndDate);

                        //// Difference between program start date and tactic minimum date
                        double daysDifference = (tacticMinStartDate - programStartDate).TotalDays;

                        //// If no. of days are more then zero then it will return progress
                        if (daysDifference > 0)
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
        /// Function to get campaign progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 2nd april, 2013
        /// </summary>
        /// <param name="lstTactic">list of tactics of selected program</param>
        /// <param name="planCampaign">campaign object</param>
        /// <param name="lstImprovementTactic">list of improvement tactics of selected plan</param>
        /// <param name="PlanId">planId of selected program</param>
        /// <returns>returns progress between 0 and 1</returns>
        public double GetCampaignProgress(List<Plan_Tactic> lstTactic, Plan_Campaign planCampaign, List<Plan_Improvement_Campaign_Program_Tactic> lstImprovementTactic, int PlanId)
        {
            if (lstImprovementTactic.Count > 0 && lstImprovementTactic.Where(improvementTactic => improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Any())
            {
                //// Minimun date of improvement tactic
                DateTime minDate = lstImprovementTactic.Where(improvementTactic => improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Select(improvementTactic => improvementTactic.EffectiveDate).Min();

                //// Start date of Campaign
                DateTime campaignStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaign.StartDate));

                //// List of all tactics
                var lstAllTactic = lstTactic.Where(tactic => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate, CalendarEndDate, tactic.objPlanTactic.StartDate, tactic.objPlanTactic.EndDate).Equals(false)
                                                    && tactic.PlanId == PlanId);

                //// List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = lstAllTactic.Where(tactic => (tactic.objPlanTactic.StartDate >= minDate).Equals(true))
                                                     .Select(tactic => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, tactic.objPlanTactic.StartDate)) })
                                                     .ToList();

                if (lstAffectedTactic.Count > 0)
                {
                    //// minimum start Date of tactics
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(tactic => tactic.startDate).Min();
                    //// If any tactic affected by at least one improvement tactic
                    if (tacticMinStartDate >= minDate)
                    {
                        double campaignDuration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, planCampaign.StartDate, planCampaign.EndDate);
                        //// Difference between campaign start date and tactic minimum date
                        double daysDifference = (tacticMinStartDate - campaignStartDate).TotalDays;

                        //// If no. of days are more then zero then it will return progress
                        if (daysDifference > 0)
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
            if (lstImprovementTactic.Count > 0 && lstImprovementTactic.Where(improvementTactic => improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Any())
            {
                //// Minimun date of improvement tactic
                DateTime minDate = lstImprovementTactic.Where(improvementTactic => improvementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Select(improvementTactic => improvementTactic.EffectiveDate).Min();

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
                            return (daysDifference / taskDuration);
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
        /// Added By :- Sohel Pathan
        /// Date :- 09/10/2014
        /// Description :- Function to get minimum start date for custom field
        /// </summary>
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic , Vertical and so on)</param>
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
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic , Vertical and so on)</param>
        /// <param name="typeId">Selected Id as per selected GanttTab</param>
        /// <param name="planid">Planid of selected plan</param>
        /// <param name="lstCampaign">List of campaign </param>
        /// <param name="lstProgram">List of Program</param>
        /// <param name="lstTactic">List of Tactic</param>
        /// <returns>Return the min start date for program and Campaign</returns>
        public DateTime GetMinStartDateForPlan(GanttTabs currentGanttTab, int typeId, int planId, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime minDateTactic = DateTime.Now;

            //// Check the case with selected plan gantt type and if it's match then extract the min date from tactic list 
            switch (currentGanttTab)
            {
                case GanttTabs.Stage:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.objPlanTactic.StageId == typeId && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    minDateTactic = lstTactic.Where(tactic => tactic.objPlanTactic.StageId == typeId && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.StartDate).Min();
                    break;
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
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic , Vertical and so on)</param>
        /// <param name="typeId">Selected Id as per selected GanttTab</param>
        /// <param name="planid">Planid of selected plan</param>
        /// <param name="lstCampaign">List of campaign </param>
        /// <param name="lstProgram">List of Program</param>
        /// <param name="lstTactic">List of Tactic</param>
        /// <param name="IsCampaign">flag to indicate campaign</param>
        /// <param name="IsProgram">flag to indicate program</param>
        /// <returns>Return the min start date for program and Campaign</returns>
        public DateTime GetMinStartDateForPlanOfCustomField(string currentGanttTab, int typeId, int planId, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic, bool IsCampaign = false, bool IsProgram = false)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime minDateTactic = DateTime.Now;
            PlanGanttTypes objPlanGanttTypes = (PlanGanttTypes)Enum.Parse(typeof(PlanGanttTypes), currentGanttTab, true);

            //// Check the case with selected plan gantt type and if it's match then extract the min date from tactic list
            switch (objPlanGanttTypes)
            {
                case PlanGanttTypes.Stage:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.objPlanTactic.StageId == typeId && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    minDateTactic = lstTactic.Where(tactic => tactic.objPlanTactic.StageId == typeId && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.StartDate).Min();
                    break;
                case PlanGanttTypes.Tactic:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    minDateTactic = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.StartDate).Min();
                    break;
                case PlanGanttTypes.Custom:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    minDateTactic = lstTactic.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId)) == typeId && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.StartDate).Min();
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
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic , Vertical and so on)</param>
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
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic , Vertical and so on)</param>
        /// <param name="typeId">Selected Id as per selected GanttTab</param>
        /// <param name="planid">Planid of selected plan</param>
        /// <param name="lstCampaign">List of campaign </param>
        /// <param name="lstProgram">List of Program</param>
        /// <param name="lstTactic">List of Tactic</param>
        /// <returns>Return the min start date for program and Campaign</returns>
        public DateTime GetMaxEndDateForPlan(GanttTabs currentGanttTab, int typeId, int planId, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime maxDateTactic = DateTime.Now;

            //// Check the case with selected plan gantt type and if it's match then extract the max date from tactic list
            switch (currentGanttTab)
            {
                case GanttTabs.Stage:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.objPlanTactic.StageId == typeId && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    maxDateTactic = lstTactic.Where(tactic => tactic.objPlanTactic.StageId == typeId && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.EndDate).Max();
                    break;
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
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic , Vertical and so on)</param>
        /// <param name="typeId">Selected Id as per selected GanttTab</param>
        /// <param name="planid">Planid of selected plan</param>
        /// <param name="lstCampaign">List of campaign </param>
        /// <param name="lstProgram">List of Program</param>
        /// <param name="lstTactic">List of Tactic</param>
        /// <param name="IsCampaign">flag to indicate campaign</param>
        /// <param name="IsProgram">flag to indicate program</param>
        /// <returns>Return the min start date for program and Campaign</returns>
        public DateTime GetMaxEndDateForPlanOfCustomFields(string currentGanttTab, int typeId, int planId, List<Plan_Campaign> lstCampaign, List<Plan_Campaign_Program> lstProgram, List<Plan_Tactic> lstTactic, bool IsCampaign = false, bool IsProgram = false)
        {
            List<int> queryPlanProgramId = new List<int>();
            DateTime maxDateTactic = DateTime.Now;
            PlanGanttTypes objPlanGanttTypes = (PlanGanttTypes)Enum.Parse(typeof(PlanGanttTypes), currentGanttTab, true);

            //// Check the case with selected plan gantt type and if it's match then extract the max date from tactic list
            switch (objPlanGanttTypes)
            {
                case PlanGanttTypes.Stage:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.objPlanTactic.StageId == typeId && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    maxDateTactic = lstTactic.Where(tactic => tactic.objPlanTactic.StageId == typeId && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.EndDate).Max();
                    break;
                case PlanGanttTypes.Tactic:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    maxDateTactic = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.EndDate).Max();
                    break;
                case PlanGanttTypes.Custom:
                    queryPlanProgramId = lstTactic.Where(tactic => tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.PlanProgramId).ToList<int>();
                    maxDateTactic = lstTactic.Where(tactic => (IsCampaign ? tactic.PlanCampaignId : (IsProgram ? tactic.objPlanTactic.PlanProgramId : tactic.objPlanTactic.PlanTacticId)) == typeId && tactic.PlanId == planId).Select(tactic => tactic.objPlanTactic.EndDate).Max();
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
        public JsonResult GetNumberOfActivityPerMonthByPlanId(int planid, string strparam)
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

                foreach (Plan_Campaign_Program_Tactic tactic in objPlan_Campaign_Program_Tactic)
                {
                    var startDate = Convert.ToDateTime(tactic.StartDate);
                    var endDate = Convert.ToDateTime(tactic.EndDate);

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
                                int monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
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
                                    int monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
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
                            int monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
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

                objActivityChart.Color = "#c633c9";
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
        public JsonResult GetNumberOfActivityPerMonthByMultiplePlanId(string planid, string strparam)
        {
            //planid = System.Web.HttpUtility.UrlDecode(planid);
            List<int> planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(plan => int.Parse(plan)).ToList();
            List<Plan> filteredPlans = objDbMrpEntities.Plans.Where(plan => plan.IsDeleted == false && planIds.Contains(plan.PlanId)).ToList().Select(plan => plan).ToList();
            List<int> filteredPlanIds = new List<int>();
            string planYear = string.Empty;
            int Planyear;

            //// Get planyear of the selected Plan
            bool isNumeric = int.TryParse(strparam, out Planyear);
            if (isNumeric)
            {
                planYear = Convert.ToString(Planyear);
            }
            else
            {
                planYear = DateTime.Now.Year.ToString();
            }

            filteredPlanIds = filteredPlans.Where(plan => plan.Year == planYear).ToList().Select(plan => plan.PlanId).ToList();

            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;
            //// Set start and end date for calender
            Common.GetPlanGanttStartEndDate(planYear, strparam, ref CalendarStartDate, ref CalendarEndDate);

            //Start Maninder Singh Wadhva : 11/15/2013 - Getting list of tactic for view control for plan version id.
            //// Select campaign(s) of plan whose IsDelete=false.
            var lstCampaign = objDbMrpEntities.Plan_Campaign.Where(campaign => filteredPlanIds.Contains(campaign.PlanId) && campaign.IsDeleted.Equals(false)).ToList()
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
                    if (isNumeric)
                    {
                        year = Planyear;
                    }
                }

                int currentMonth = DateTime.Now.Month;

                foreach (Plan_Campaign_Program_Tactic tactic in objPlan_Campaign_Program_Tactic)
                {
                    var startDate = Convert.ToDateTime(tactic.StartDate);
                    var endDate = Convert.ToDateTime(tactic.EndDate);

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
                                int monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
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
                                    int monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
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
                            int monthNo = Convert.ToInt32(objDifference.TrimStart('0'));
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
                objActivityChart.Color = "#c633c9";
                lstActivityChart.Add(objActivityChart);
            }

            //// return Activity Chart list as Json Result object
            return Json(new { lstchart = lstActivityChart.ToList() }, JsonRequestBehavior.AllowGet);
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

            //// Prepare array of months for seelcted quarter
            if (startDateParam.Month == month1 || startDateParam.Month == month2 || startDateParam.Month == month3 || endDateParam.Month == month1 || endDateParam.Month == month2 || endDateParam.Month == month3)
            {
                differenceItems = Enumerable.Range(0, Int32.MaxValue).Select(element => startDateParam.AddMonths(element)).TakeWhile(element => element <= endDateParam).Select(element => element.ToString("MM"));
                foreach (string d in differenceItems)
                {
                    int monthNo = Convert.ToInt32(d.TrimStart('0'));
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
        /// Action method to return AddActual view
        /// </summary>
        /// <returns>returns AddActual view</returns>
        public ActionResult AddActual()
        {
            HomePlanModel planmodel = new Models.HomePlanModel();

            try
            {
                List<string> tacticStatus = Common.GetStatusListAfterApproved();

                //// Tthis is inititalized as 0 bcoz to get the status for tactics.
                string planGanttType = PlanGanttTypes.Tactic.ToString();
                ViewBag.AddActualFlag = true;     // Added by Arpita Soni on 01/17/2015 for Ticket #1090 
                List<User> lstIndividuals = GetIndividualsByPlanId(Sessions.PlanId.ToString(), planGanttType, Enums.ActiveMenu.Home.ToString(), true);
                ////Start - Modified by Mitesh Vaishnav for PL ticket 972 - Add Actuals - Filter section formatting

                //// Fetch individual's records distinct
                planmodel.objIndividuals = lstIndividuals.Select(user => new
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }).ToList().Distinct().Select(user => new User()
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }).ToList().OrderBy(user => string.Format("{0} {1}", user.FirstName, user.LastName)).ToList();
                ////End - Modified by Mitesh Vaishnav for PL ticket 972 - Add Actuals - Filter section formatting

                List<TacticType> objTacticType = new List<TacticType>();

                //// Modified By: Maninder Singh for TFS Bug#282: Extra Tactics Displaying in Add Actual Screen
                objTacticType = (from tactic in objDbMrpEntities.Plan_Campaign_Program_Tactic
                                 where tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == Sessions.PlanId && tacticStatus.Contains(tactic.Status) && tactic.IsDeleted == false
                                 select tactic.TacticType).Distinct().OrderBy(tactic => tactic.Title).ToList();

                ViewBag.TacticTypeList = objTacticType;

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
                var objPlan = objDbMrpEntities.Plans.FirstOrDefault(plan => plan.PlanId == Sessions.PlanId);
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

                //// Start - Added by Sohel Pathan on 22/01/2015 for PL ticket #1144
                List<CustomFieldsForFilter> lstCustomField = new List<CustomFieldsForFilter>();
                List<CustomFieldsForFilter> lstCustomFieldOption = new List<CustomFieldsForFilter>();

                //// Retrive Custom Fields and CustomFieldOptions list
                GetCustomFieldAndOptions(out lstCustomField, out lstCustomFieldOption);

                planmodel.lstCustomFields = lstCustomField;
                planmodel.lstCustomFieldOptions = lstCustomFieldOption;
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
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Actual Value of Tactic.
        /// </summary>
        /// <param name="status">status of tactic</param>
        /// <param name="tacticTypeId"></param>
        /// <param name="customFieldId"></param>
        /// <param name="ownerId"></param>
        /// <param name="UserId"></param>
        /// <returns>Returns Json Result.</returns>
        public JsonResult GetActualTactic(int status, string tacticTypeId, string customFieldId, string ownerId, string UserId = "")
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

            //// Custom Field Filter Criteria.
            List<string> filteredCustomFields = string.IsNullOrWhiteSpace(customFieldId) ? new List<string>() : customFieldId.Split(',').Select(customField => customField.ToString()).ToList();
            List<int> lstEntityIds = new List<int>();
            bool recordsFound = true;
            if (filteredCustomFields.Count > 0)
            {
                //// get Allowed Entity Ids
                lstEntityIds = GetAllowedEntitiesBasedonCustomFieldFilterParameter(filteredCustomFields, out recordsFound);
            }

            //// Owner filter criteria.
            List<Guid> filteredOwner = string.IsNullOrWhiteSpace(ownerId) ? new List<Guid>() : ownerId.Split(',').Select(owner => Guid.Parse(owner)).ToList();
            //// End - Added by Sohel Pathan on 22/01/2015 for PL ticket #1144

            List<Plan_Campaign_Program_Tactic> TacticList = new List<Plan_Campaign_Program_Tactic>();
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            if (status == 0)
            {
                //// Modified By: Maninder Singh for TFS Bug#282: Extra Tactics Displaying in Add Actual Screen
                TacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(planTactic => planTactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(Sessions.PlanId) &&
                                                                                tacticStatus.Contains(planTactic.Status) && planTactic.IsDeleted.Equals(false) &&
                                                                                planTactic.CostActual == null && !planTactic.Plan_Campaign_Program_Tactic_Actual.Any() &&
                                                                                (filteredTacticTypeIds.Count.Equals(0) || filteredTacticTypeIds.Contains(planTactic.TacticType.TacticTypeId)) &&
                                                                                recordsFound == true &&
                                                                                (lstEntityIds.Count.Equals(0) || lstEntityIds.Contains(planTactic.PlanTacticId)) &&
                                                                                (filteredOwner.Count.Equals(0) || filteredOwner.Contains(planTactic.CreatedBy))
                                                                                ).ToList();
            }
            else
            {
                TacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.Plan_Campaign_Program.Plan_Campaign.PlanId == Sessions.PlanId &&
                                                                                tacticStatus.Contains(tactic.Status) && tactic.IsDeleted == false &&
                                                                                (filteredTacticTypeIds.Count.Equals(0) || filteredTacticTypeIds.Contains(tactic.TacticType.TacticTypeId)) &&
                                                                                recordsFound == true &&
                                                                                (lstEntityIds.Count.Equals(0) || lstEntityIds.Contains(tactic.PlanTacticId)) &&
                                                                                (filteredOwner.Count.Equals(0) || filteredOwner.Contains(tactic.CreatedBy))
                                                                                ).ToList();
            }


            List<int> TacticIds = TacticList.Select(tactic => tactic.PlanTacticId).ToList();

            //// Apply Custom restriction for None type
            if (TacticIds.Count() > 0)
            {
                List<int> lstAllowedEntityIds = Common.GetAllowedCustomFieldEntityList(Sessions.User.UserId, Sessions.User.ClientId, TacticIds, false);
                TacticList = TacticList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId)).Select(tactic => tactic).ToList();
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

                List<int> lstViewEditEntities = Common.GetViewEditEntityList(Sessions.User.UserId, TacticIds);

                List<string> lstMonthly = new List<string>() { "Y1", "Y2", "Y3", "Y4", "Y5", "Y6", "Y7", "Y8", "Y9", "Y10", "Y11", "Y12" };

                var tacticLineItem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => TacticIds.Contains(lineItem.PlanTacticId) && lineItem.IsDeleted == false).ToList();
                List<int> LineItemsIds = tacticLineItem.Select(lineItem => lineItem.PlanLineItemId).ToList();
                var tacticLineItemActual = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(lienItemActual => LineItemsIds.Contains(lienItemActual.PlanLineItemId)).ToList();
                var tacticActuals = objDbMrpEntities.Plan_Campaign_Program_Tactic_Actual.Where(tacticActual => TacticIds.Contains(tacticActual.PlanTacticId)).ToList();

                var tacticObj = TacticList.Select(tactic => new
                {
                    id = tactic.PlanTacticId,
                    title = tactic.Title,
                    mqlProjected = MQLTacticList.Where(mqlTactic => mqlTactic.PlanTacticId == tactic.PlanTacticId).Select(mqlTactic => mqlTactic.MQL),
                    mqlActual = lstTacticActual.Where(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.StageTitle == TitleMQL).Sum(tacticActual => tacticActual.Actualvalue),
                    cwProjected = Math.Round(tacticListCW.Where(cwTactic => cwTactic.PlanTacticId == tactic.PlanTacticId).Select(cwTactic => cwTactic.ProjectedRevenue).FirstOrDefault(), 1),
                    cwActual = lstTacticActual.Where(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.StageTitle == TitleCW).Sum(tacticActual => tacticActual.Actualvalue),
                    revenueProjected = Math.Round(tacticList.Where(planTactic => planTactic.PlanTacticId == tactic.PlanTacticId).Select(planTactic => planTactic.ProjectedRevenue).FirstOrDefault(), 1),
                    revenueActual = lstTacticActual.Where(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.StageTitle == TitleRevenue).Sum(tacticActual => tacticActual.Actualvalue),
                    costProjected = (tacticLineItem.Where(lineItem => lineItem.PlanTacticId == tactic.PlanTacticId)).Count() > 0 ? (tacticLineItem.Where(lineItem => lineItem.PlanTacticId == lineItem.PlanTacticId)).Sum(lineItem => lineItem.Cost) : tactic.Cost,
                    //// Get the sum of Tactic line item actuals
                    costActual = (tacticLineItem.Where(lineItem => lineItem.PlanTacticId == tactic.PlanTacticId)).Count() > 0 ?
                    (tacticLineItem.Where(lineItem => lineItem.PlanTacticId == tactic.PlanTacticId)).Select(lineItem => new
                    {
                        LineItemActualCost = tacticLineItemActual.Where(lineItemActual => lineItemActual.PlanLineItemId == lineItem.PlanLineItemId).Sum(lineItemActual => lineItemActual.Value)
                    }).Sum(lineItemActual => lineItemActual.LineItemActualCost) : (tacticActuals.Where(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.StageTitle == Enums.InspectStageValues[Enums.InspectStage.Cost.ToString()].ToString())).Sum(tacticActual => tacticActual.Actualvalue),

                    //// First check that if tactic has a single line item at that time we need to get the cost actual data from the respective table. 
                    costActualData = lstMonthly.Select(month => new
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
                    actualData = (tacticActuals.Where(tacticActual => tacticActual.PlanTacticId.Equals(tactic.PlanTacticId)).Select(tacticActual => tacticActual).ToList()).Select(planTactic => new
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
                    projectedStageValueActual = lstTacticActual.Where(tacticActual => tacticActual.PlanTacticId == tactic.PlanTacticId && tacticActual.StageTitle == TitleProjectedStageValue).Sum(tacticActual => tacticActual.Actualvalue),
                    IsTacticEditable = (lstViewEditEntities.Contains(tactic.PlanTacticId)),
                    //// Set Line Item data with it's actual values and Sum
                    LineItemsData = (tacticLineItem.Where(lineItem => lineItem.PlanTacticId.Equals(tactic.PlanTacticId)).ToList()).Select(lineItem => new
                    {
                        id = lineItem.PlanLineItemId,
                        Title = lineItem.Title,
                        LineItemCost = lineItem.Cost,
                        //// Get the sum of actual
                        LineItemActualCost = (tacticLineItemActual.Where(lineItemActual => lineItemActual.PlanLineItemId == lineItem.PlanLineItemId)).ToList().Sum(lineItemActual => lineItemActual.Value),
                        LineItemActual = lstMonthly.Select(month => new
                        {
                            period = month,
                            Cost = tacticLineItemActual.Where(lineItemActual => lineItemActual.PlanLineItemId == lineItem.PlanLineItemId && lineItemActual.Period == month).Select(lineItemActual => lineItemActual.Value)
                        }),
                    })
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
                    LineItemsData = tacticActual.LineItemsData
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

        /// <summary>
        /// Added By :- Sohel Pathan
        /// Date :- 14/04/2014
        /// Action method to get list of users who have created tactic by planId (PL ticket # 428)
        /// </summary>
        /// <param name="PlanId">comma separated list of plan id(s)</param>
        /// <param name="ViewBy">ViewBy option selected from dropdown</param>
        /// <param name="ActiveMenu">current active menu</param>
        /// <returns>returns list of owners in json format</returns>
        public JsonResult GetOwnerListForFilter(string PlanId, string ViewBy, string ActiveMenu)
        {
            try
            {
                var lstOwners = GetIndividualsByPlanId(PlanId, ViewBy, ActiveMenu);
                var lstAllowedOwners = lstOwners.Select(owner => new
                {
                    OwnerId = owner.UserId,
                    Title = owner.FirstName + " " + owner.LastName,
                }).Distinct().ToList().OrderBy(owner => owner.Title).ToList();

                lstAllowedOwners = lstAllowedOwners.Where(owner => !string.IsNullOrEmpty(owner.Title)).OrderBy(owner => owner.Title, new AlphaNumericComparer()).ToList();

                return Json(new { isSuccess = true, AllowedOwner = lstAllowedOwners }, JsonRequestBehavior.AllowGet);
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
        private List<User> GetIndividualsByPlanId(string PlanId, string ViewBy, string ActiveMenu, bool IsForAddActuals = false)
        {
            List<int> PlanIds = string.IsNullOrWhiteSpace(PlanId) ? new List<int>() : PlanId.Split(',').Select(plan => int.Parse(plan)).ToList();

            //// Added by :- Sohel Pathan on 17/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, ActiveMenu.ToLower());
            List<string> status = Common.GetStatusListAfterApproved();

            // Start - Added by Arpita Soni on 01/17/2015 for Ticket #1090 
            // To remove owner of submitted tactics from filter list
            if (IsForAddActuals)
            {
                status.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            }
            // End - Added by Arpita Soni on 01/17/2015 for Ticket #1090

            //// Select Tactics of selected plans
            var campaignList = objDbMrpEntities.Plan_Campaign.Where(campaign => campaign.IsDeleted.Equals(false) && PlanIds.Contains(campaign.PlanId)).Select(campaign => campaign.PlanCampaignId).ToList();
            var programList = objDbMrpEntities.Plan_Campaign_Program.Where(program => program.IsDeleted.Equals(false) && campaignList.Contains(program.PlanCampaignId)).Select(program => program.PlanProgramId).ToList();
            var tacticList = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted.Equals(false) && programList.Contains(tactic.PlanProgramId)).Select(tactic => tactic);

            BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();

            List<int> lstAllowedEntityIds = new List<int>();

            //// Added by :- Sohel Pathan on 21/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
            if (ActiveMenu.Equals(Enums.ActiveMenu.Plan.ToString()))
            {
                List<string> statusCD = new List<string>();
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

                var TacticUserList = tacticList.Distinct().ToList();
                TacticUserList = TacticUserList.Where(tactic => status.Contains(tactic.Status) || ((tactic.CreatedBy == Sessions.User.UserId && !ViewBy.Equals(GanttTabs.Request.ToString())) ? statusCD.Contains(tactic.Status) : false)).Distinct().ToList();

                if (TacticUserList.Count > 0)
                {
                    List<int> planTacticIds = TacticUserList.Select(tactic => tactic.PlanTacticId).ToList();
                    lstAllowedEntityIds = Common.GetAllowedCustomFieldEntityList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);

                    //// Custom Restrictions applied
                    TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId)).ToList();
                }

                string strContatedIndividualList = string.Join(",", TacticUserList.Select(tactic => tactic.CreatedBy.ToString()));
                var individuals = bdsUserRepository.GetMultipleTeamMemberName(strContatedIndividualList);

                return individuals;
            }
            else
            {
                //// Modified by :- Sohel Pathan on 17/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
                var TacticUserList = tacticList.Where(tactic => status.Contains(tactic.Status)).Select(tactic => tactic).Distinct().ToList();

                if (TacticUserList.Count > 0)
                {
                    List<int> planTacticIds = TacticUserList.Select(tactic => tactic.PlanTacticId).ToList();
                    lstAllowedEntityIds = Common.GetAllowedCustomFieldEntityList(Sessions.User.UserId, Sessions.User.ClientId, planTacticIds, false);

                    //// Custom Restrictions applied
                    TacticUserList = TacticUserList.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId)).ToList();
                }

                string strContatedIndividualList = string.Join(",", TacticUserList.Select(tactic => tactic.CreatedBy.ToString()));
                var individuals = bdsUserRepository.GetMultipleTeamMemberName(strContatedIndividualList);

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
        /// Action method to get plans by businessunit id(s)
        /// </summary>
        /// <param name="businessUnitIds">comma separated list of businessunit id(s)</param>
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
                    activePlan = objDbMrpEntities.Plans.Where(plan => plan.IsActive.Equals(true) && plan.IsDeleted == false && plan.Status == publishedPlanStatus)
                                                            .Select(plan => new { plan.PlanId, plan.Title })
                                                            .ToList().Where(plan => !string.IsNullOrEmpty(plan.Title)).OrderBy(plan => plan.Title, new AlphaNumericComparer()).ToList();
                }
                else
                {
                    activePlan = objDbMrpEntities.Plans.Where(plan => plan.IsActive.Equals(true) && plan.IsDeleted == false)
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
        /// Action method to get list of Custom Atributes like Geography, Audience, Vertical and Owner
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
        /// Function to prepare list of custom fields and customfield options
        /// </summary>
        /// <param name="customFieldListOut"></param>
        /// <param name="customFieldOptionsListOut"></param>
        public void GetCustomFieldAndOptions(out List<CustomFieldsForFilter> customFieldListOut, out List<CustomFieldsForFilter> customFieldOptionsListOut)
        {
            customFieldListOut = new List<CustomFieldsForFilter>();
            customFieldOptionsListOut = new List<CustomFieldsForFilter>();

            //// Custom Restrictions
            var userCustomRestrictionList = Common.GetUserCustomRestrictionsList(Sessions.User.UserId, true);   //// Modified by Sohel Pathan on 15/01/2015 for PL ticket #1139
            int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;

            //// Prepare list of allowed Custom Field Option Ids
            List<int> lstAllowedCustomFieldOptionIds = userCustomRestrictionList.Where(customRestriction => (customRestriction.Permission == ViewOnlyPermission || customRestriction.Permission == ViewEditPermission)).Select(customRestriction => customRestriction.CustomFieldOptionId).ToList();

            //// Get list of custom fields
            string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
            string EntityTypeTactic = Enums.EntityType.Tactic.ToString();
            var lstCustomField = objDbMrpEntities.CustomFields.Where(customField => customField.ClientId == Sessions.User.ClientId && customField.IsDeleted.Equals(false) &&
                                                                customField.EntityType.Equals(EntityTypeTactic) && customField.CustomFieldType.Name.Equals(DropDownList) &&
                                                                customField.IsDisplayForFilter.Equals(true))
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

                ////// Sort custom fields by name
                lstCustomField = lstCustomField.OrderBy(customField => customField.Name).ToList();

                //// Get list of custom field options
                var lstCustomFieldOption = objDbMrpEntities.CustomFieldOptions
                                                            .Where(customFieldOption => lstCustomFieldId.Contains(customFieldOption.CustomFieldId) && lstAllowedCustomFieldOptionIds.Contains(customFieldOption.CustomFieldOptionId))
                                                            .Select(customFieldOption => new
                                                            {
                                                                customFieldOption.CustomFieldId,
                                                                customFieldOption.CustomFieldOptionId,
                                                                customFieldOption.Value
                                                            }).ToList();

                if (lstCustomFieldOption.Count > 0)
                {
                    List<int> customFieldIdFromOptions = new List<int>();
                    customFieldIdFromOptions = lstCustomFieldOption.Select(option => option.CustomFieldId).Distinct().ToList();
                    if (customFieldIdFromOptions.Count() > 0)
                    {
                        lstCustomField = lstCustomField.Where(customField => customFieldIdFromOptions.Contains(customField.CustomFieldId)).ToList();
                    }

                    ////// Sort custom field option list by value and custom field id
                    lstCustomFieldOption = lstCustomFieldOption.OrderBy(customFieldOption => customFieldOption.CustomFieldId).ThenBy(customFieldOption => customFieldOption.Value).ToList();

                    customFieldListOut = lstCustomField.Select(customField => new CustomFieldsForFilter()
                                                            {
                                                                CustomFieldId = customField.CustomFieldId,
                                                                Title = customField.Name
                                                            }).ToList();

                    customFieldOptionsListOut = lstCustomFieldOption.Select(customFieldOption => new CustomFieldsForFilter()
                                                                            {
                                                                                CustomFieldId = customFieldOption.CustomFieldId,
                                                                                CustomFieldOptionId = customFieldOption.CustomFieldOptionId,
                                                                                Title = customFieldOption.Value
                                                                            }).ToList();

                }
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

            //// Get the Current year and Pre define Upcoming Activites.
            string currentYear = DateTime.Now.Year.ToString();
            List<SelectListItem> UpcomingActivityList = new List<SelectListItem>();

            //// Fetch the pervious year and future year list and insert into the list object
            var yearlistPrevious = activePlan.Where(plan => plan.Year != DateTime.Now.Year.ToString() && Convert.ToInt32(plan.Year) < DateTime.Now.Year).Select(plan => plan.Year).Distinct().OrderBy(year => year).ToList();
            yearlistPrevious.ForEach(year => UpcomingActivityList.Add(new SelectListItem { Text = year, Value = year, Selected = false }));

            //// If active plan dosen't have any current plan at that time we have to remove this month and thisquater option
            if (activePlan.Count > 0)
            {
                if (activePlan.Where(plan => plan.Year == currentYear).Count() != 0)
                {
                    //// Add current year into the list
                    UpcomingActivityList.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thisquarter.ToString()].ToString(), Value = Enums.UpcomingActivities.thisquarter.ToString(), Selected = false });
                    UpcomingActivityList.Add(new SelectListItem { Text = Enums.UpcomingActivitiesValues[Enums.UpcomingActivities.thismonth.ToString()].ToString(), Value = Enums.UpcomingActivities.thismonth.ToString(), Selected = false });
                    UpcomingActivityList.Add(new SelectListItem { Text = DateTime.Now.Year.ToString(), Value = DateTime.Now.Year.ToString(), Selected = true });
                }
            }

            var yearlistAfter = activePlan.Where(plan => plan.Year != DateTime.Now.Year.ToString() && Convert.ToInt32(plan.Year) > DateTime.Now.Year).Select(plan => plan.Year).Distinct().OrderBy(year => year).ToList();
            yearlistAfter.ForEach(year => UpcomingActivityList.Add(new SelectListItem { Text = year, Value = year, Selected = false }));
            return UpcomingActivityList;
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
        private int InspectPopupSharedLinkValidation(int currentPlanId, int planCampaignId, int planProgramId, int planTacticId, bool isImprovement)
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
        /// <param name="lstAllowedGeographyIds">list of GeographyIds allowed to current logged in user</param>
        /// <param name="ViewOnlyPermission">ViewOnlyPermission flag in form of int</param>
        /// <param name="ViewEditPermission">ViewEditPermission flag in form of int</param>
        /// <returns>returns flag for custom restriction as per custom restriction</returns>
        private bool InspectPopupSharedLinkValidationForCustomRestriction(int planCampaignId, int planProgramId, int planTacticId, bool isImprovement)
        {
            bool isValidEntity = false;

            if (planTacticId > 0 && isImprovement.Equals(false))
            {
                List<int> AllowedTacticIds = new List<int>();
                AllowedTacticIds = Common.GetAllowedCustomFieldEntityList(Sessions.User.UserId, Sessions.User.ClientId, new List<int>() { planTacticId }, false);

                var objTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == planTacticId && tactic.IsDeleted == false
                                                                    && AllowedTacticIds.Contains(tactic.PlanTacticId))
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

            return isValidEntity;
        }

        #endregion

        #region User defined functions

        #region Get Allowed Entity Ids based on Custom Field Filter Critria
        /// <summary>
        /// Function to retrieve list of Entity Ids based on given custom field filter paramaters
        /// </summary>
        /// <param name="filteredCustomFields"></param>
        /// <param name="recordsFound"></param>
        /// <returns></returns>
        private List<int> GetAllowedEntitiesBasedonCustomFieldFilterParameter(List<string> filteredCustomFields, out bool recordsFound)
        {
            recordsFound = true;
            List<int> lstEntityIds = new List<int>();

            if (filteredCustomFields.Count > 0)
            {
                List<int> filterdCustomFieldIds = new List<int>();
                List<int> filterdCustomFieldOptionIds = new List<int>();
                List<CustomField_Entity> lstCustomFieldEntities = new List<CustomField_Entity>();

                filteredCustomFields.ForEach(customField =>
                {

                    string[] splittedCustomField = customField.Split('_');
                    filterdCustomFieldIds.Add(int.Parse(splittedCustomField[0]));
                    filterdCustomFieldOptionIds.Add(int.Parse(splittedCustomField[1]));
                });

                filterdCustomFieldIds = filterdCustomFieldIds.Distinct().ToList();
                filterdCustomFieldOptionIds = filterdCustomFieldOptionIds.Distinct().ToList();

                lstCustomFieldEntities = objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => filterdCustomFieldIds.Contains(customFieldEntity.CustomFieldId))
                                                                            .Select(customFieldEntity => customFieldEntity).ToList();

                lstCustomFieldEntities = lstCustomFieldEntities.Where(customFieldEntity => filterdCustomFieldOptionIds.Contains(int.Parse(customFieldEntity.Value)))
                                                    .Select(customFieldEntity => customFieldEntity).Distinct().ToList();

                var CustomFieldWiseEntityList = filterdCustomFieldIds.Select(customField => new
                {
                    customField,
                    EntityList = lstCustomFieldEntities.Where(cc => cc.CustomFieldId == customField).Select(cc => cc.EntityId).ToList(),
                }).ToList();

                var AndEntityList = from list in CustomFieldWiseEntityList
                                    from option in list.EntityList
                                    where CustomFieldWiseEntityList.All(l => l.EntityList.Any(o => o == option))
                                    orderby option
                                    select option;

                if (AndEntityList.Count() > 0)
                {
                    lstEntityIds = AndEntityList.Distinct().ToList();
                }
                else
                {
                    recordsFound = false;
                }
            }

            return lstEntityIds;
        }
        #endregion

        #endregion
    }
}
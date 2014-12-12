using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Elmah;
using System.IO;
using Newtonsoft.Json;
using System.Globalization;
using System.Transactions;
using System.Data.Objects;
using RevenuePlanner.BDSService;
using System.Web.Routing;
using System.Reflection;
using System.Web;
using Integration;
using System.Text.RegularExpressions;
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

        private MRPEntities db = new MRPEntities();
        private const string GANTT_BAR_CSS_CLASS_PREFIX = "color";
        private BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
        private DateTime CalendarStartDate;
        private DateTime CalendarEndDate;
        private const string Campaign_InspectPopup_Flag_Color = "C6EBF3";
        private const string Plan_InspectPopup_Flag_Color = "C6EBF3";       // Added by Sohel Pathan on 07/11/2014 for PL ticket #811
        private const string Program_InspectPopup_Flag_Color = "3DB9D3";
        ////Modified by Maninder Singh Wadhva on 06/26/2014 #531 When a tactic is synced a comment should be created in that tactic
        private const string GameplanIntegrationService = "Gameplan Integration Service";
        private string DefaultLineItemTitle = "Line Item";
        #endregion

        #region "Index"

        /// <summary>
        /// Home index page
        /// In Plan Header, values of MQLs, Budget and number of Tactics of current plan shown from database.
        /// planCampaignId and planProgramId parameter added for plan and campaign popup
        /// Modified By Maninder Singh Wadhva PL Ticket#47
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Home, int currentPlanId = 0, int planTacticId = 0, int planCampaignId = 0, int planProgramId = 0, bool isImprovement = false)
        {
            // To get permission status for Plan create, By dharmraj PL #519
            ViewBag.IsPlanCreateAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            // To get permission status for Add/Edit Actual, By dharmraj PL #519
            ViewBag.IsTacticActualsAddEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.TacticActualsAddEdit);

            ViewBag.ActiveMenu = activeMenu;
            ViewBag.ShowInspectForPlanTacticId = planTacticId;
            ViewBag.ShowInspectForPlanCampaignId = planCampaignId;
            ViewBag.ShowInspectForPlanProgramId = planProgramId;

            //// Modified By Maninder Singh Wadhva PL Ticket#47
            ViewBag.IsImprovement = isImprovement;
            // Start - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021
            if (activeMenu.Equals(Enums.ActiveMenu.Plan) && currentPlanId > 0)
            {
                currentPlanId = InspectPopupSharedLinkValidation(currentPlanId, planCampaignId, planProgramId, planTacticId, isImprovement);
            }
            else if (activeMenu.Equals(Enums.ActiveMenu.Home) && currentPlanId > 0 && (planTacticId > 0 || planCampaignId > 0 || planProgramId > 0))
            {
                ViewBag.ShowInspectPopup = false;
                ViewBag.ShowInspectPopupErrorMessage = Common.objCached.InvalidURLForInspectPopup.ToString();
            }
            else if ((activeMenu.Equals(Enums.ActiveMenu.Plan) || activeMenu.Equals(Enums.ActiveMenu.Home)) && currentPlanId <= 0 && (planTacticId > 0 || planCampaignId > 0 || planProgramId > 0))
            {
                ViewBag.ShowInspectPopup = false;
                ViewBag.ShowInspectPopupErrorMessage = Common.objCached.InvalidURLForInspectPopup.ToString();
            }
            else
            {
                ViewBag.ShowInspectPopup = true;
            }
            // End - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021

            ViewBag.SuccessMessageDuplicatePlan = TempData["SuccessMessageDuplicatePlan"];
            ViewBag.ErrorMessageDuplicatePlan = TempData["ErrorMessageDuplicatePlan"];
            ViewBag.BusinessUnitTitle = "";

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

            List<Guid> businessUnitIds = new List<Guid>();

            var lstAllowedBusinessUnits = new List<string>();
            try
            {
                lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList();
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

            if (lstAllowedBusinessUnits.Count > 0)
                lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));

            var lstClientBusinessUnits = Common.GetBussinessUnitIds(Sessions.User.ClientId);

            if (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin) && lstAllowedBusinessUnits.Count == 0) //else if (Sessions.IsDirector || Sessions.IsClientAdmin)
            {
                //var clientBusinessUnit = db.BusinessUnits.Where(b => b.ClientId.Equals(Sessions.User.ClientId) && b.IsDeleted == false).Select(b => b.BusinessUnitId).ToList<Guid>();
                //businessUnitIds = clientBusinessUnit.ToList();
                lstClientBusinessUnits = lstClientBusinessUnits.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
                planmodel.BusinessUnitIds = lstClientBusinessUnits;
                ViewBag.BusinessUnitIds = lstClientBusinessUnits;
                businessUnitIds = lstClientBusinessUnits.ToList().Select(b => Guid.Parse(b.Text)).ToList();
                if (businessUnitIds.Count > 1)
                    ViewBag.showBid = true;
                else
                    ViewBag.showBid = false;
            }
            else
            {
                // Start - Added by Sohel Pathan on 30/06/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                if (lstAllowedBusinessUnits.Count > 0)
                {
                    //lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));
                    var lstAllowedUserBusinessUnits = lstClientBusinessUnits.Where(a => businessUnitIds.Contains(Guid.Parse(a.Value))).ToList().Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
                    planmodel.BusinessUnitIds = lstAllowedUserBusinessUnits;
                    ViewBag.BusinessUnitIds = lstAllowedUserBusinessUnits;
                    if (lstAllowedBusinessUnits.Count >= 1)
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
                // End - Added by Sohel Pathan on 30/06/2014 for PL ticket #563 to apply custom restriction logic on Business Units
            }

            //// Getting active model of above business unit. 
            //string modelPublishedStatus = Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value;
            List<Model> models = db.Models.Where(m => businessUnitIds.Contains(m.BusinessUnitId) && m.IsDeleted == false).ToList();
            if (currentPlanId == 0)
            {
                /*added by Nirav for plan consistency on 14 apr 2014*/
                if (Sessions.BusinessUnitId != Guid.Empty)
                {
                    List<Model> filteredModels = models.Where(m => m.BusinessUnitId == Sessions.BusinessUnitId).ToList();
                    if (filteredModels.Count() > 0)
                    {
                        models = filteredModels;
                    }
                    else
                    {
                        models = db.Models.Where(m => m.BusinessUnitId == Sessions.BusinessUnitId && m.IsDeleted == false).ToList();
                    }
                }
            }

            //// Getting modelIds
            var modelIds = models.Select(m => m.ModelId).ToList();

            List<Plan> activePlan = db.Plans.Where(p => modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true) && p.IsDeleted == false).ToList();

            if (Enums.ActiveMenu.Home.Equals(activeMenu))
            {
                //// Getting Active plan for all above models.
                string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
                activePlan = activePlan.Where(p => p.Status.Equals(planPublishedStatus)).ToList();

                if (activePlan.Count == 0)
                {
                    bool hasPublishedPlan = false;
                    for (int i = 0; i < businessUnitIds.Count; i++)
                    {
                        Guid BUnitId = businessUnitIds[i];
                        if (!hasPublishedPlan)
                        {
                            // Getting active model of above business unit. 
                            models = db.Models.Where(m => m.BusinessUnitId == BUnitId && m.IsDeleted == false).ToList();

                            //// Getting modelIds
                            modelIds = models.Select(m => m.ModelId).ToList();

                            activePlan = db.Plans.Where(p => modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true) && p.IsDeleted == false).ToList();
                            activePlan = activePlan.Where(p => p.Status.Equals(planPublishedStatus)).ToList();
                        }
                        if (activePlan.Count > 0)
                        {
                            hasPublishedPlan = true;
                            Sessions.BusinessUnitId = BUnitId;
                        }
                    }
                }
            }
            // Added by Bhavesh, Current year first plan select in dropdown
            string currentYear = DateTime.Now.Year.ToString();
            if (activePlan.Count() != 0)
            {
                try
                {
                    Plan currentPlan = new Plan();
                    if (currentPlanId != 0)
                    {
                        currentPlan = activePlan.Where(p => p.PlanId.Equals(currentPlanId)).FirstOrDefault();
                        if (planmodel.BusinessUnitIds.Count > 0 && currentPlan != null) // Modified by Viral Kadiya on 12/2/2014 to resolve PL ticket #978.
                        {
                            ViewBag.BusinessUnitTitle = planmodel.BusinessUnitIds.Where(b => b.Value.ToLower() == currentPlan.Model.BusinessUnitId.ToString().ToLower()).Select(b => b.Text).FirstOrDefault();
                        }
                        // Start - Added by Sohel Pathan on 12/12/2014 for PL ticket #1021
                        else
                        {
                            currentPlan = activePlan.Select(p => p).FirstOrDefault();
                            ViewBag.BusinessUnitTitle = planmodel.BusinessUnitIds.Where(b => b.Value.ToLower() == currentPlan.Model.BusinessUnitId.ToString().ToLower()).Select(b => b.Text).FirstOrDefault();
                            currentPlanId = currentPlan.PlanId;
                        }
                        // End - Added by Sohel Pathan on 12/12/2014 for PL ticket #1021
                    }
                    else if (!Common.IsPlanPublished(Sessions.PlanId))
                    {
                        /* added by Nirav shah for TFS Point : 218*/
                        if (Sessions.PublishedPlanId == 0)
                        {
                            // Added by Bhavesh, Current year first plan select in dropdown
                            if (activePlan.Where(p => p.Year == currentYear).Count() > 0)
                            {
                                currentPlan = activePlan.Where(p => p.Year == currentYear).OrderBy(p => p.Title).FirstOrDefault();
                            }
                            else
                            {
                                currentPlan = activePlan.Select(p => p).OrderBy(p => p.Title).FirstOrDefault();
                            }
                        }
                        else
                        {
                            currentPlan = activePlan.Where(p => p.PlanId.Equals(Sessions.PublishedPlanId)).OrderBy(p => p.Title).FirstOrDefault();
                            //when old published plan id (session) is from different business unit then it retunr null
                            if (currentPlan == null)
                            {
                                currentPlan = activePlan.OrderBy(p => p.Title).FirstOrDefault();
                            }
                            //when old published plan id (session) is from different business unit then it retunr null
                        }
                    }
                    else
                    {
                        /* added by Nirav shah for TFS Point : 218*/
                        if (Sessions.PlanId == 0)
                        {
                            // Added by Bhavesh, Current year first plan select in dropdown
                            if (activePlan.Where(p => p.Year == currentYear).Count() > 0)
                            {
                                currentPlan = activePlan.Where(p => p.Year == currentYear).OrderBy(p => p.Title).FirstOrDefault();
                            }
                            else
                            {
                                currentPlan = activePlan.OrderBy(p => p.Title).FirstOrDefault();
                            }
                        }
                        else
                        {
                            currentPlan = activePlan.Where(p => p.PlanId.Equals(Sessions.PlanId)).FirstOrDefault();

                            if (currentPlan == null)
                            {
                                currentPlan = activePlan.OrderBy(p => p.Title).FirstOrDefault();
                            }

                        }
                    }
                    /*added by Nirav for plan consistency on 14 apr 2014*/
                    Sessions.BusinessUnitId = currentPlan.Model.BusinessUnitId;
                    planmodel.PlanTitle = currentPlan.Title;
                    planmodel.PlanId = currentPlan.PlanId;
                    planmodel.objplanhomemodelheader = Common.GetPlanHeaderValue(currentPlan.PlanId);

                    //List<SelectListItem> UpcomingActivityList = UpComingActivity(Convert.ToString(currentPlan.PlanId));
                    //planmodel.objplanhomemodelheader.UpcomingActivity = UpcomingActivityList;

                    // Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
                    var lstUserCustomRestriction = Common.GetUserCustomRestriction();
                    int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
                    int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                    var lstAllowedGeography = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId).ToList();
                    List<Guid> lstAllowedGeographyId = new List<Guid>();
                    lstAllowedGeography.ForEach(g => lstAllowedGeographyId.Add(Guid.Parse(g)));
                    //Start Maninder Singh Wadhva : 11/25/2013 - Getting list of geographies and individuals.
                    planmodel.objGeography = db.Geographies.Where(g => g.IsDeleted.Equals(false) && g.ClientId.Equals(Sessions.User.ClientId) && lstAllowedGeographyId.Contains(g.GeographyId)).Select(g => g).OrderBy(g => g.Title).ToList();

                    Sessions.PlanId = planmodel.PlanId;

                    // Start - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021
                    if (ViewBag.ShowInspectPopup != null)
                    {
                        if ((bool)ViewBag.ShowInspectPopup == true && activeMenu.Equals(Enums.ActiveMenu.Plan) && currentPlanId > 0)
                        {
                            bool isCustomRestrictionPass = InspectPopupSharedLinkValidationForCustomRestriction(planCampaignId, planProgramId, planTacticId, isImprovement, lstUserCustomRestriction, lstAllowedGeographyId, ViewOnlyPermission, ViewEditPermission);
                            ViewBag.ShowInspectPopup = isCustomRestrictionPass;
                            if (isCustomRestrictionPass.Equals(false))
                            {
                                ViewBag.ShowInspectPopupErrorMessage = Common.objCached.CustomRestrictionFailedForInspectPopup.ToString();
                            }

                        }
                    }
                    // End - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021
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
                ViewBag.IsBusinessUnitEditable = Common.IsBusinessUnitEditable(Sessions.BusinessUnitId);    // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units
                return View(planmodel);
            }
            else
            {
                if (activeMenu != Enums.ActiveMenu.Plan)
                {
                    TempData["ErrorMessage"] = Common.objCached.NoPublishPlanAvailable;  //// Error Message modified by Sohel Pathan on 22/05/2014 to address internal review points                    
                }
                else
                {
                    TempData["ErrorMessage"] = null;
                }

                return RedirectToAction("PlanSelector", "Plan");
            }

        }

        //New parameter activeMenu added to check whether this method is called from Home or Plan
        public ActionResult HomePlan(string Bid, int currentPlanId, string activeMenu)
        {
            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, activeMenu.ToLower());
            HomePlan objHomePlan = new HomePlan();
            //objHomePlan.IsDirector = Sessions.IsDirector;
            //objHomePlan.IsClientAdmin = Sessions.IsClientAdmin;
            //objHomePlan.IsDirector = Sessions.IsDirector;
            //objHomePlan.IsClientAdmin = Sessions.IsClientAdmin;
            // Set the flag (IsManager) if current user have sub ordinates, By Dharmraj Mangukiya, #538
            //var lstUserHierarchy = objBDSUserRepository.GetUserHierarchy(Sessions.User.ClientId, Sessions.ApplicationId);
            //var lstSubordinates = lstUserHierarchy.Where(u => u.ManagerId == Sessions.User.UserId).ToList();
            var lstSubordinates = new List<Guid>();
            try
            {
                lstSubordinates = Common.GetSubOrdinatesWithPeersNLevel();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }

            if (lstSubordinates.Count > 0)
            {
                objHomePlan.IsManager = true;
            }
            else
            {
                objHomePlan.IsManager = false;
            }

            List<SelectListItem> planList;
            if (Bid == "false")
            {
                var plan = Common.GetPlan();
                //if condition added to dispaly only published plan on home page
                if (objactivemenu.Equals(Enums.ActiveMenu.Home))
                {
                    //// Getting Active plan for all above models.
                    string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
                    planList = plan.Where(p => p.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).ToList();
                }
                else
                {
                    planList = plan.Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).ToList();
                }

                var objexists = planList.Where(p => p.Value == currentPlanId.ToString()).ToList();
                if (objexists.Count != 0)
                {
                    planList.Single(p => p.Value.Equals(currentPlanId.ToString())).Selected = true;
                }
            }
            else
            {
                Guid bId = new Guid(Bid);
                /*added by Nirav for plan consistency on 14 apr 2014*/
                Sessions.BusinessUnitId = bId;
                var plan = Common.GetPlan().Where(s => s.Model.BusinessUnitId == bId);
                //if condition added to dispaly only published plan on home page
                if (objactivemenu.Equals(Enums.ActiveMenu.Home))
                {
                    //// Getting Active plan for all above models.
                    string planPublishedStatus = Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value;
                    planList = plan.Where(p => p.Status.Equals(planPublishedStatus)).Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).ToList();
                }
                else
                {
                    planList = plan.Select(p => new SelectListItem() { Text = p.Title, Value = p.PlanId.ToString() }).OrderBy(p => p.Text).ToList();
                }

                if (planList.Count > 0)
                {
                    var objexists = planList.Where(p => p.Value == currentPlanId.ToString()).ToList();
                    if (objexists.Count != 0)
                    {
                        planList.Single(p => p.Value.Equals(currentPlanId.ToString())).Selected = true;
                    }
                    else
                    {
                        planList.FirstOrDefault().Selected = true;
                    }
                }
                else
                {
                    Sessions.BusinessUnitId = Guid.Empty;
                }

            }
            if (planList != null)
                planList = planList.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            objHomePlan.plans = planList;

            List<ViewByModel> lstViewByTab = Common.GetDefaultGanttTypes(null);
            ViewBag.ViewByTab = lstViewByTab;

            List<SelectListItem> lstUpComingActivity = UpComingActivity(Convert.ToString(currentPlanId));
            lstUpComingActivity = lstUpComingActivity.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
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

        #region "View Control & GANTT Chart"
        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Modified By Maninder Singh Wadhva PL Ticket#47
        /// Date: 11/15/2013
        /// Function to get tactic for view control.
        /// Modfied By: Maninder Singh Wadhva.
        /// Reason: To show task whose either start or end date or both date are outside current view.
        /// </summary>
        /// <param name="planVersionId">Plan version Id.</param>
        /// <returns>Returns tactic type and paln campaign program tactic as Json Result.</returns>
        public JsonResult GetViewControlDetail(string viewBy, string planId, string isQuarter, string businessunitIds, string geographyIds, string verticalIds, string audienceIds, string ownerIds, string activeMenu, bool getViewByList)
        {
            #region "For all users"

            //// BusinessUnit Filter Criteria.
            List<Guid> filteredBusinessUnitIds = string.IsNullOrWhiteSpace(businessunitIds) ? new List<Guid>() : businessunitIds.Split(',').Select(bu => Guid.Parse(bu)).ToList();

            //// Create plan list based on PlanIds and Businessunit Ids filter criteria
            List<int> planIds = string.IsNullOrWhiteSpace(planId) ? new List<int>() : planId.Split(',').Select(plan => int.Parse(plan)).ToList();
            var plans = db.Plans.Where(p => planIds.Contains(p.PlanId) && p.IsDeleted.Equals(false) && filteredBusinessUnitIds.Contains(p.Model.BusinessUnitId)).Select(p => new { p.PlanId, p.Model.BusinessUnitId, p.Year }).ToList();

            //// Custom Restriction for BusinessUnit
            var lstUserCustomRestriction = new List<UserCustomRestrictionModel>();
            var lstAllowedBusinessUnits = new List<string>();
            try
            {
                lstUserCustomRestriction = Common.GetUserCustomRestriction();
                lstAllowedBusinessUnits = Common.GetViewEditBusinessUnitList(lstUserCustomRestriction);
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


            if (lstAllowedBusinessUnits.Count > 0)
            {
                List<Guid> businessUnitIds = new List<Guid>();
                lstAllowedBusinessUnits.ForEach(g => businessUnitIds.Add(Guid.Parse(g)));
                plans = plans.Where(p => businessUnitIds.Contains(p.BusinessUnitId)).Select(p => p).ToList();
            }
            string planYear = "";
            int year;
            bool isNumeric = int.TryParse(isQuarter, out year);

            if (isNumeric)
            {
                planYear = Convert.ToString(year);
            }
            else
            {
                planYear = DateTime.Now.Year.ToString();
            }

            var filteredPlanIds = plans.Where(p => p.Year == planYear).ToList().Select(p => p.PlanId).ToList();

            CalendarStartDate = CalendarEndDate = DateTime.Now;
            Common.GetPlanGanttStartEndDate(planYear, isQuarter, ref CalendarStartDate, ref CalendarEndDate);

            //Start Maninder Singh Wadhva : 11/15/2013 - Getting list of tactic for view control for plan version id.
            //// Selecting campaign(s) of plan whose IsDelete=false.
            var campaign = db.Plan_Campaign.Where(pc => filteredPlanIds.Contains(pc.PlanId) && pc.IsDeleted.Equals(false))
                                           .Select(pc => pc).ToList()
                                           .Where(pc => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate, CalendarEndDate, pc.StartDate, pc.EndDate).Equals(false));

            //// Selecting campaignIds.
            List<int> campaignId = campaign.Select(pc => pc.PlanCampaignId).ToList();

            //// Selecting program(s) of campaignIds whose IsDelete=false.
            var program = db.Plan_Campaign_Program.Where(pcp => campaignId.Contains(pcp.PlanCampaignId) && pcp.IsDeleted.Equals(false))
                                                  .Select(pcp => pcp)
                                                  .ToList()
                                                  .Where(pcp => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                            CalendarEndDate,
                                                                                                            pcp.StartDate,
                                                                                                            pcp.EndDate).Equals(false));

            //// Selecting programIds.
            List<int> programId = program.Select(pcp => pcp.PlanProgramId).ToList();

            //// Geography Filter Criteria.
            List<Guid> filteredGeography = string.IsNullOrWhiteSpace(geographyIds) ? new List<Guid>() : geographyIds.Split(',').Select(geographyId => Guid.Parse(geographyId)).ToList();

            //// Vertical filter criteria.
            List<int> filteredVertical = string.IsNullOrWhiteSpace(verticalIds) ? new List<int>() : verticalIds.Split(',').Select(v => int.Parse(v)).ToList();

            //// Audience filter criteria.
            List<int> filteredAudience = string.IsNullOrWhiteSpace(audienceIds) ? new List<int>() : audienceIds.Split(',').Select(a => int.Parse(a)).ToList();

            //// Owner filter criteria.
            List<Guid> filterOwner = string.IsNullOrWhiteSpace(ownerIds) ? new List<Guid>() : ownerIds.Split(',').Select(o => Guid.Parse(o)).ToList();

            //// Applying filters to tactic (IsDelete, Geography, Individuals and Show My Tactic)
            var tactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted.Equals(false) &&
                                                                       programId.Contains(pcpt.PlanProgramId) &&
                                                                       (filteredGeography.Count.Equals(0) || filteredGeography.Contains(pcpt.GeographyId)) &&
                                                                       (filteredVertical.Count.Equals(0) || filteredVertical.Contains(pcpt.VerticalId)) &&
                                                                       (filteredAudience.Count.Equals(0) || filteredAudience.Contains(pcpt.AudienceId)) &&
                                                                       (filterOwner.Count.Equals(0) || filterOwner.Contains(pcpt.CreatedBy)))
                                                                       .ToList().Where(pcpt =>
                                                                           //// Checking start and end date
                                                                        Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate, CalendarEndDate, pcpt.StartDate, pcpt.EndDate).Equals(false));

            //// Modified By Maninder Singh Wadhva PL Ticket#47
            List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(improveTactic => filteredPlanIds.Contains(improveTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId) &&
                                                                                      improveTactic.IsDeleted.Equals(false) &&
                                                                                      (improveTactic.EffectiveDate > CalendarEndDate).Equals(false))
                                                                               .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

            // Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
            int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            var lstAllowedVertical = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId).ToList();
            var lstAllowedGeography = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId.ToString().ToLower()).ToList();////Modified by Mitesh Vaishnav For functional review point 89
            tactic = tactic.Where(t => lstAllowedVertical.Contains(t.VerticalId.ToString()) && lstAllowedGeography.Contains(t.GeographyId.ToString().ToLower())).ToList();////Modified by Mitesh Vaishnav For functional review point 89

            var lstSubordinatesWithPeers = new List<Guid>();
            try
            {
                lstSubordinatesWithPeers = Common.GetSubOrdinatesWithPeersNLevel();
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

            var subordinatesTactic = tactic.Where(t => lstSubordinatesWithPeers.Contains(t.CreatedBy)).ToList();
            var subordinatesImprovementTactic = improvementTactic.Where(t => lstSubordinatesWithPeers.Contains(t.CreatedBy)).ToList();

            string tacticStatus = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            //// Modified By Maninder Singh Wadhva PL Ticket#47, Modofied by Dharmraj #538
            string requestCount = Convert.ToString(subordinatesTactic.Where(t => t.Status.Equals(tacticStatus)).Count() + subordinatesImprovementTactic.Where(improveTactic => improveTactic.Status.Equals(tacticStatus)).Count());

            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, activeMenu.ToLower());

            var tacticForAllTabs = tactic.ToList();

            // Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
            if (viewBy.Equals(PlanGanttTypes.Request.ToString()))
            {
                tactic = tactic.Where(t => lstSubordinatesWithPeers.Contains(t.CreatedBy)).ToList();
                improvementTactic = improvementTactic.Where(t => lstSubordinatesWithPeers.Contains(t.CreatedBy)).ToList();
            }

            object improvementTacticForAccordion = new object();
            object improvementTacticTypeForAccordion = new object();

            // Added by Dharmraj Ticket #364,#365,#366
            if (objactivemenu.Equals(Enums.ActiveMenu.Plan))
            {
                List<string> status = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                List<string> statusCD = new List<string>();
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

                tactic = tactic.Where(t => status.Contains(t.Status) || ((t.CreatedBy == Sessions.User.UserId && !viewBy.Equals(PlanGanttTypes.Request.ToString())) ? statusCD.Contains(t.Status) : false))
                                .Select(planTactic => planTactic).ToList<Plan_Campaign_Program_Tactic>();

                List<string> improvementTacticStatus = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                improvementTactic = improvementTactic.Where(improveTactic => improvementTacticStatus.Contains(improveTactic.Status) || ((improveTactic.CreatedBy == Sessions.User.UserId && !viewBy.Equals(PlanGanttTypes.Request.ToString())) ? statusCD.Contains(improveTactic.Status) : false))
                                                           .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();
            }
            else if (objactivemenu.Equals(Enums.ActiveMenu.Home))
            {
                List<string> status = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                tactic = tactic.Where(t => status.Contains(t.Status)).Select(planTactic => planTactic).ToList<Plan_Campaign_Program_Tactic>();

                List<string> improvementTacticStatus = GetStatusAsPerSelectedType(viewBy, objactivemenu);
                improvementTactic = improvementTactic.Where(improveTactic => improvementTacticStatus.Contains(improveTactic.Status))
                                                           .Select(improveTactic => improveTactic).ToList<Plan_Improvement_Campaign_Program_Tactic>();

                improvementTacticForAccordion = GetImprovementTacticForAccordion(improvementTactic);
                improvementTacticTypeForAccordion = GetImprovementTacticTypeForAccordion(improvementTactic);
            }

            // Start - Added by Sohel Pathan on 28/10/2014 for PL ticket #885
            var viewByListResult = prepareViewByList(getViewByList, tacticForAllTabs);
            if (viewByListResult.Count > 0)
            {
                if (!(viewByListResult.Where(v => v.Value.Equals(viewBy, StringComparison.OrdinalIgnoreCase)).Any()))
                {
                    viewBy = PlanGanttTypes.Tactic.ToString();
                }
            }
            // End - Added by Sohel Pathan on 28/10/2014 for PL ticket #885

            if (viewBy.Equals(PlanGanttTypes.Tactic.ToString(), StringComparison.OrdinalIgnoreCase) || viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return PrepareTacticAndRequestTabResult(viewBy, objactivemenu, campaign.ToList(), program.ToList(), tactic.ToList(), improvementTactic, requestCount, planYear, improvementTacticForAccordion, improvementTacticTypeForAccordion, planId, viewByListResult);
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
            else
            {
                return PrepareCustomFieldResult(viewBy, objactivemenu, campaign.ToList(), program.ToList(), tactic.ToList(), improvementTactic, requestCount, planYear, improvementTacticForAccordion, improvementTacticTypeForAccordion, planId, viewByListResult);
            }

            #endregion
        }

        /// <summary>
        /// Created By :- Sohel Pathan
        /// Created Date :- 08/10/2014
        /// </summary>
        /// <param name="type"></param>
        /// <param name="campaign"></param>
        /// <param name="program"></param>
        /// <param name="tactic"></param>
        /// <param name="improvementTactic"></param>
        /// <param name="requestCount"></param>
        /// <param name="planYear"></param>
        /// <param name="improvementTacticForAccordion"></param>
        /// <param name="improvementTacticTypeForAccordion"></param>
        /// <returns></returns>
        private JsonResult PrepareCustomFieldResult(string viewBy, Enums.ActiveMenu activemenu, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion, string planId, List<ViewByModel> viewByListResult)
        {
            string sourceViewBy = viewBy;
            int CustomTypeId = 0;
            bool IsCampaign = viewBy.Contains(Common.CampaignCustomTitle) ? true : false;
            bool IsProgram = viewBy.Contains(Common.ProgramCustomTitle) ? true : false;
            bool IsTactic = viewBy.Contains(Common.TacticCustomTitle) ? true : false;
            string entityType = Enums.EntityType.Tactic.ToString();
            if (IsTactic)
            {
                CustomTypeId = Convert.ToInt32(viewBy.Replace(Common.TacticCustomTitle, ""));
                viewBy = PlanGanttTypes.Custom.ToString();
            }
            else if (IsCampaign)
            {
                CustomTypeId = Convert.ToInt32(viewBy.Replace(Common.CampaignCustomTitle, ""));
                entityType = Enums.EntityType.Campaign.ToString();
                viewBy = PlanGanttTypes.Custom.ToString();
            }
            else if (IsProgram)
            {
                CustomTypeId = Convert.ToInt32(viewBy.Replace(Common.ProgramCustomTitle, ""));
                entityType = Enums.EntityType.Program.ToString();
                viewBy = PlanGanttTypes.Custom.ToString();
            }

            List<TacticTaskList> lstTacticTaskList = new List<TacticTaskList>();
            List<CustomFields> lstCustomFields = new List<CustomFields>();
            List<ImprovementTaskDetail> lstImprovementTaskDetails = new List<ImprovementTaskDetail>();

            List<Plan_Campaign_Program_Tactic> tacticListByViewById = new List<Plan_Campaign_Program_Tactic>();
            int tacticPlanId = 0;
            DateTime MinStartDateForCustomField;
            DateTime MinStartDateForPlanNew;

            if (viewBy.Equals(PlanGanttTypes.Vertical.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in tactic)
                {
                    tacticListByViewById = tactic.Where(t => t.VerticalId == item.VerticalId).Select(t => t).ToList();
                    tacticPlanId = item.Plan_Campaign_Program.Plan_Campaign.PlanId;
                    MinStartDateForCustomField = GetMinStartDateForCustomField(PlanGanttTypes.Vertical, item.VerticalId, campaign, program, tacticListByViewById);
                    MinStartDateForPlanNew = GetMinStartDateForPlanNew(viewBy, item.VerticalId, tacticPlanId, campaign, program, tacticListByViewById);

                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = item,
                        CustomFieldId = item.VerticalId.ToString(),
                        CustomFieldTitle = item.Vertical.Title,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", item.VerticalId, tacticPlanId, item.Plan_Campaign_Program.PlanCampaignId, item.PlanProgramId, item.PlanTacticId),
                        ColorCode = item.Vertical.ColorCode,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                        EndDate = DateTime.MaxValue,
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField,
                                                                    GetMaxEndDateForCustomField(PlanGanttTypes.Vertical, item.VerticalId, campaign, program, tactic)),
                        CampaignProgress = GetCampaignProgress(tacticListByViewById, item.Plan_Campaign_Program.Plan_Campaign, improvementTactic, tacticPlanId),
                        ProgramProgress = GetProgramProgress(tacticListByViewById, item.Plan_Campaign_Program, improvementTactic, tacticPlanId),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlanNew),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlanNew,
                                                    GetMaxEndDateForPlanNew(viewBy, item.VerticalId, tacticPlanId, campaign, program, tacticListByViewById)),
                                                    tacticListByViewById, improvementTactic, tacticPlanId),
                    });
                }

                if (activemenu.Equals(Enums.ActiveMenu.Home))
                {
                    lstCustomFields = (tactic.Select(t => t.Vertical)).Select(vertical => new
                    {
                        CustomFieldId = vertical.VerticalId,
                        Title = vertical.Title,
                        ColorCode = vertical.ColorCode
                    }).ToList().Distinct().Select(a => new CustomFields()
                    {
                        CustomFieldId = a.CustomFieldId.ToString(),
                        Title = a.Title,
                        ColorCode = a.ColorCode
                    }).ToList().OrderBy(vertical => vertical.Title).ToList();
                }

                lstImprovementTaskDetails = (from it in improvementTactic
                                             join t in tactic on it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals t.Plan_Campaign_Program.Plan_Campaign.PlanId
                                             join v in db.Verticals on t.VerticalId equals v.VerticalId
                                             where it.IsDeleted == false && t.IsDeleted == false && v.IsDeleted == false
                                             select new ImprovementTaskDetail()
                                             {
                                                 ImprovementTactic = it,
                                                 MainParentId = v.VerticalId.ToString(),
                                                 MinStartDate = improvementTactic.Where(impt => impt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impt => impt.EffectiveDate).Min(),
                                             }).Select(it => it).ToList().Distinct().ToList();
            }
            else if (viewBy.Equals(PlanGanttTypes.Stage.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in tactic)
                {
                    tacticListByViewById = tactic.Where(t => t.StageId == item.StageId).Select(t => t).ToList();
                    tacticPlanId = item.Plan_Campaign_Program.Plan_Campaign.PlanId;
                    MinStartDateForCustomField = GetMinStartDateStageAndBusinessUnit(campaign, program, tacticListByViewById);
                    MinStartDateForPlanNew = GetMinStartDateForPlanNew(viewBy, item.StageId, tacticPlanId, campaign, program, tacticListByViewById);

                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = item,
                        CustomFieldId = item.StageId.ToString(),
                        CustomFieldTitle = item.Stage.Title,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", item.StageId, tacticPlanId, item.Plan_Campaign_Program.PlanCampaignId, item.PlanProgramId, item.PlanTacticId),
                        ColorCode = item.Stage.ColorCode,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                        EndDate = DateTime.MaxValue,
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField,
                                                                GetMaxEndDateStageAndBusinessUnit(campaign, program, tacticListByViewById)),
                        CampaignProgress = GetCampaignProgress(tacticListByViewById, item.Plan_Campaign_Program.Plan_Campaign, improvementTactic, tacticPlanId),
                        ProgramProgress = GetProgramProgress(tacticListByViewById, item.Plan_Campaign_Program, improvementTactic, tacticPlanId),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlanNew),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlanNew,
                                                    GetMaxEndDateForPlanNew(viewBy, item.StageId, tacticPlanId, campaign, program, tacticListByViewById)),
                                                    tacticListByViewById, improvementTactic, tacticPlanId),
                    });
                }

                if (activemenu.Equals(Enums.ActiveMenu.Home))
                {
                    lstCustomFields = (tactic.Select(t => t.Stage)).Select(c => new
                    {
                        CustomFieldId = c.StageId,
                        Title = c.Title,
                        ColorCode = c.ColorCode
                    }).ToList().Distinct().Select(a => new CustomFields()
                    {
                        CustomFieldId = a.CustomFieldId.ToString(),
                        Title = a.Title,
                        ColorCode = a.ColorCode
                    }).ToList().OrderBy(t => t.Title).ToList();
                }

                lstImprovementTaskDetails = (from it in improvementTactic
                                             join t in tactic on it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals t.Plan_Campaign_Program.Plan_Campaign.PlanId
                                             join v in db.Stages on t.StageId equals v.StageId
                                             where it.IsDeleted == false && t.IsDeleted == false && v.IsDeleted == false
                                             select new ImprovementTaskDetail()
                                             {
                                                 ImprovementTactic = it,
                                                 MainParentId = v.StageId.ToString(),
                                                 MinStartDate = improvementTactic.Where(impt => impt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impt => impt.EffectiveDate).Min(),
                                             }).Select(it => it).ToList().Distinct().ToList();
            }
            else if (viewBy.Equals(PlanGanttTypes.Audience.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in tactic)
                {
                    tacticPlanId = item.Plan_Campaign_Program.Plan_Campaign.PlanId;
                    tacticListByViewById = tactic.Where(t => t.AudienceId == item.AudienceId).Select(t => t).ToList();
                    MinStartDateForCustomField = GetMinStartDateForCustomField(PlanGanttTypes.Audience, item.AudienceId, campaign, program, tacticListByViewById);
                    MinStartDateForPlanNew = GetMinStartDateForPlanNew(viewBy, item.AudienceId, tacticPlanId, campaign, program, tacticListByViewById);

                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = item,
                        CustomFieldId = item.AudienceId.ToString(),
                        CustomFieldTitle = item.Audience.Title,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", item.AudienceId, tacticPlanId, item.Plan_Campaign_Program.PlanCampaignId, item.PlanProgramId, item.PlanTacticId),
                        ColorCode = item.Audience.ColorCode,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                        EndDate = DateTime.MaxValue,
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField,
                                                                GetMaxEndDateForCustomField(PlanGanttTypes.Audience, item.AudienceId, campaign, program, tacticListByViewById)),
                        CampaignProgress = GetCampaignProgress(tacticListByViewById, item.Plan_Campaign_Program.Plan_Campaign, improvementTactic, tacticPlanId),
                        ProgramProgress = GetProgramProgress(tacticListByViewById, item.Plan_Campaign_Program, improvementTactic, tacticPlanId),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlanNew),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlanNew,
                                                    GetMaxEndDateForPlanNew(viewBy, item.AudienceId, tacticPlanId, campaign, program, tacticListByViewById)),
                                                    tacticListByViewById, improvementTactic, tacticPlanId),
                    });
                }

                if (activemenu.Equals(Enums.ActiveMenu.Home))
                {
                    lstCustomFields = (tactic.Select(t => t.Audience)).Select(t => new
                    {
                        CustomFieldId = t.AudienceId,
                        Title = t.Title,
                        ColorCode = t.ColorCode
                    }).ToList().Distinct().Select(a => new CustomFields()
                    {
                        CustomFieldId = a.CustomFieldId.ToString(),
                        Title = a.Title,
                        ColorCode = a.ColorCode
                    }).ToList().OrderBy(vertical => vertical.Title).ToList();
                }

                lstImprovementTaskDetails = (from it in improvementTactic
                                             join t in tactic on it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals t.Plan_Campaign_Program.Plan_Campaign.PlanId
                                             join a in db.Audiences on t.AudienceId equals a.AudienceId
                                             where it.IsDeleted == false && t.IsDeleted == false && a.IsDeleted == false
                                             select new ImprovementTaskDetail()
                                             {
                                                 ImprovementTactic = it,
                                                 MainParentId = a.AudienceId.ToString(),
                                                 MinStartDate = improvementTactic.Where(impt => impt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impt => impt.EffectiveDate).Min(),
                                             }).Select(it => it).ToList().Distinct().ToList();
            }
            else if (viewBy.Equals(PlanGanttTypes.BusinessUnit.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in tactic)
                {
                    tacticPlanId = item.Plan_Campaign_Program.Plan_Campaign.PlanId;
                    tacticListByViewById = tactic.Where(t => t.BusinessUnitId.Equals(item.BusinessUnitId)).Select(t => t).ToList();
                    MinStartDateForCustomField = GetMinStartDateStageAndBusinessUnit(campaign, program, tacticListByViewById);
                    MinStartDateForPlanNew = GetMinStartDateForPlanNew(viewBy, item.PlanTacticId, tacticPlanId, campaign, program, tacticListByViewById);

                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = item,
                        CustomFieldId = item.BusinessUnitId.ToString(),
                        CustomFieldTitle = item.BusinessUnit.Title,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", item.BusinessUnitId, tacticPlanId, item.Plan_Campaign_Program.PlanCampaignId, item.PlanProgramId, item.PlanTacticId),
                        ColorCode = item.BusinessUnit.ColorCode,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                        EndDate = DateTime.MaxValue,
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField,
                                                                GetMaxEndDateStageAndBusinessUnit(campaign, program, tacticListByViewById)),
                        CampaignProgress = GetCampaignProgress(tacticListByViewById, item.Plan_Campaign_Program.Plan_Campaign, improvementTactic, tacticPlanId),
                        ProgramProgress = GetProgramProgress(tacticListByViewById, item.Plan_Campaign_Program, improvementTactic, tacticPlanId),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlanNew),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlanNew,
                                                    GetMaxEndDateForPlanNew(viewBy, item.PlanTacticId, tacticPlanId, campaign, program, tacticListByViewById)),
                                                    tacticListByViewById, improvementTactic, tacticPlanId),
                    });
                }

                if (activemenu.Equals(Enums.ActiveMenu.Home))
                {
                    lstCustomFields = (tactic.Select(t => t.BusinessUnit)).Select(t => new
                    {
                        CustomFieldId = t.BusinessUnitId,
                        Title = t.Title,
                        ColorCode = t.ColorCode
                    }).ToList().Distinct().Select(a => new CustomFields()
                    {
                        CustomFieldId = a.CustomFieldId.ToString(),
                        Title = a.Title,
                        ColorCode = a.ColorCode
                    }).ToList().OrderBy(vertical => vertical.Title).ToList();
                }

                lstImprovementTaskDetails = improvementTactic.Select(it => new ImprovementTaskDetail()
                {
                    ImprovementTactic = it,
                    MainParentId = it.BusinessUnitId.ToString(),
                    MinStartDate = improvementTactic.Where(impt => impt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impt => impt.EffectiveDate).Min(),
                }).Select(it => it).ToList().Distinct().ToList();
            }
            else
            {
                var tacticIdList = tactic.Select(t => t.PlanTacticId).ToList().Distinct();

                CustomFieldOption objCustomFieldOption = new CustomFieldOption();
                objCustomFieldOption.CustomFieldOptionId = 0;
                string DropDownList = Enums.CustomFieldType.DropDownList.ToString();

                var tempTactic = (from cf in db.CustomFields
                                  join cft in db.CustomFieldTypes on cf.CustomFieldTypeId equals cft.CustomFieldTypeId
                                  join cfe in db.CustomField_Entity on cf.CustomFieldId equals cfe.CustomFieldId
                                  join t in db.Plan_Campaign_Program_Tactic on cfe.EntityId equals
                                  (IsCampaign ? t.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? t.PlanProgramId : t.PlanTacticId))
                                  join cfoLeft in db.CustomFieldOptions on new { Key1 = cf.CustomFieldId, Key2 = cfe.Value.Trim() } equals
                                     new { Key1 = cfoLeft.CustomFieldId, Key2 = SqlFunctions.StringConvert((double)cfoLeft.CustomFieldOptionId).Trim() } into cAll
                                  from cfo in cAll.DefaultIfEmpty()
                                  where cf.IsDeleted == false && t.IsDeleted == false && cf.EntityType == entityType && cf.CustomFieldId == CustomTypeId &&
                                  cf.ClientId == Sessions.User.ClientId && tacticIdList.Contains(t.PlanTacticId)
                                  select new
                                  {
                                      tactic = t,
                                      masterCustomFieldId = cf.CustomFieldId,
                                      customFieldId = cft.Name == DropDownList ? (cfo.CustomFieldOptionId == null ? 0 : cfo.CustomFieldOptionId) : cfe.CustomFieldEntityId,
                                      customFieldTitle = cft.Name == DropDownList ? cfo.Value : cfe.Value,
                                      customFieldTYpe = cft.Name,
                                      EntityId = cfe.EntityId,
                                  }).ToList().Distinct().ToList().ToList();

                var newtactic = tempTactic.Select(t => new
                {
                    tactic = t.tactic,
                    masterCustomFieldId = t.masterCustomFieldId,
                    customFieldId = tempTactic.Where(tt => tt.masterCustomFieldId == t.masterCustomFieldId && tt.customFieldTitle.Trim() == t.customFieldTitle.Trim()).Select(tt => tt.customFieldId).First(),
                    customFieldTitle = t.customFieldTitle,
                    customFieldTacticList = t.customFieldTYpe == DropDownList ? tempTactic.Where(tt => tt.customFieldId == t.customFieldId).Select(tt => tt.EntityId) : tempTactic.Where(tt => tt.customFieldTitle == t.customFieldTitle).Select(tt => tt.EntityId),
                }).ToList();

                DateTime MaxEndDateForCustomField;

                foreach (var item in newtactic)
                {
                    tacticPlanId = item.tactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
                    tacticListByViewById = tactic.Where(t => item.customFieldTacticList.Contains(IsCampaign ? t.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? t.PlanProgramId : t.PlanTacticId))).Select(t => t).ToList();
                    MinStartDateForCustomField = GetMinStartDateForCustomField(PlanGanttTypes.Custom, IsCampaign ? item.tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? item.tactic.PlanProgramId : item.tactic.PlanTacticId),
                                                                                campaign, program, tacticListByViewById, IsCampaign, IsProgram);
                    MaxEndDateForCustomField = GetMaxEndDateForCustomField(PlanGanttTypes.Custom, IsCampaign ? item.tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? item.tactic.PlanProgramId : item.tactic.PlanTacticId),
                                                                                campaign, program, tacticListByViewById, IsCampaign, IsProgram);
                    MinStartDateForPlanNew = GetMinStartDateForPlanNew(viewBy, IsCampaign ? item.tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? item.tactic.PlanProgramId : item.tactic.PlanTacticId),
                                                                                tacticPlanId, campaign, program, tacticListByViewById, IsCampaign, IsProgram);

                    lstTacticTaskList.Add(new TacticTaskList()
                    {
                        Tactic = item.tactic,
                        CustomFieldId = item.customFieldId.ToString(),
                        CustomFieldTitle = item.customFieldTitle,
                        TaskId = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", item.customFieldId, tacticPlanId, item.tactic.Plan_Campaign_Program.PlanCampaignId, item.tactic.PlanProgramId, item.tactic.PlanTacticId),
                        ColorCode = Common.ColorCodeForCustomField,
                        StartDate = Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForCustomField),
                        EndDate = Common.GetEndDateAsPerCalendarInDateFormat(CalendarEndDate, MaxEndDateForCustomField),  //item.tactic.EndDate,
                        Duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForCustomField, MaxEndDateForCustomField),
                        CampaignProgress = GetCampaignProgress(tacticListByViewById, item.tactic.Plan_Campaign_Program.Plan_Campaign, improvementTactic, tacticPlanId),
                        ProgramProgress = GetProgramProgress(tacticListByViewById, item.tactic.Plan_Campaign_Program, improvementTactic, tacticPlanId),
                        PlanProgrss = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, MinStartDateForPlanNew),
                                                    Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, MinStartDateForPlanNew,
                                                        GetMaxEndDateForPlanNew(viewBy, IsCampaign ? item.tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? item.tactic.PlanProgramId : item.tactic.PlanTacticId),
                                                        tacticPlanId, campaign, program, tacticListByViewById, IsCampaign, IsProgram))
                                                    , tacticListByViewById, improvementTactic, tacticPlanId),
                        lstCustomEntityId = item.customFieldTacticList.ToList(),
                    });
                }

                if (activemenu.Equals(Enums.ActiveMenu.Home))
                {
                    lstCustomFields = (newtactic.Select(t => t)).Select(t => new
                    {
                        CustomFieldId = t.customFieldId,
                        Title = t.customFieldTitle,
                        ColorCode = Common.ColorCodeForCustomField.ToString()
                    }).ToList().Distinct().Select(a => new CustomFields()
                    {
                        CustomFieldId = a.CustomFieldId.ToString(),
                        Title = a.Title,
                        ColorCode = a.ColorCode
                    }).ToList().OrderBy(vertical => vertical.Title).ToList();
                }

                lstImprovementTaskDetails = (from it in improvementTactic
                                             join t in newtactic on it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId equals t.tactic.Plan_Campaign_Program.Plan_Campaign.PlanId
                                             join a in db.CustomFields on t.masterCustomFieldId equals a.CustomFieldId
                                             where it.IsDeleted == false && t.tactic.IsDeleted == false && a.IsDeleted == false
                                             select new ImprovementTaskDetail()
                                             {
                                                 ImprovementTactic = it,
                                                 MainParentId = t.customFieldId.ToString(),
                                                 MinStartDate = improvementTactic.Where(impt => impt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impt => impt.EffectiveDate).Min(),
                                             }).Select(it => it).ToList().Distinct().ToList();

            }

            var lstTaskDetails = lstTacticTaskList.Select(t => new
            {
                MainParentId = t.CustomFieldId,
                MainParentTitle = t.CustomFieldTitle,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Duration = t.Duration,
                Progress = 0,
                Open = false,
                Color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.ColorCode.ToLower()),
                PlanId = t.Tactic.Plan_Campaign_Program.Plan_Campaign.PlanId,
                PlanTitle = t.Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Title,
                Campaign = t.Tactic.Plan_Campaign_Program.Plan_Campaign,
                Program = t.Tactic.Plan_Campaign_Program,
                Tactic = t.Tactic,
                PlanProgress = t.PlanProgrss,
                CampaignProgress = t.CampaignProgress,
                ProgramProgress = t.ProgramProgress,
                lstCustomEntityId = t.lstCustomEntityId,
            }).ToList().Distinct().ToList();

            #region Custom Field
            var taskDataCustomeFields = lstTaskDetails.Select(t => new
            {
                id = string.Format("Z{0}", t.MainParentId),
                text = t.MainParentTitle,
                start_date = t.StartDate,
                end_date = t.EndDate,
                duration = t.Duration,
                progress = t.Progress,
                open = t.Open,
                color = t.Color
            }).Select(v => v).Distinct().OrderBy(t => t.text);

            var groupedCustomField = taskDataCustomeFields.GroupBy(v => new { id = v.id }).Select(v => new
            {
                id = v.Key.id,
                text = v.Select(a => a.text).FirstOrDefault(),
                start_date = v.Select(a => a.start_date).ToList().Min(),
                end_date = v.Select(a => a.end_date).ToList().Max(),
                duration = v.Select(a => a.duration).ToList().Max(),
                progress = v.Select(a => a.progress).FirstOrDefault(),
                open = v.Select(a => a.open).FirstOrDefault(),
                color = v.ToList().Select(a => a.color).FirstOrDefault() + ((v.ToList().Select(a => a.progress).FirstOrDefault() > 0) ? "stripe" : "")
            });

            var newTaskDataCustomField = groupedCustomField.Select(v => new
            {
                id = v.id,
                text = v.text,
                start_date = v.start_date,
                duration = v.end_date == DateTime.MaxValue ? v.duration : Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, Convert.ToDateTime(v.start_date), v.end_date),
                progress = v.progress,
                open = v.open,
                color = v.color + ((v.progress > 0) ? "stripe" : "")
            });
            #endregion

            #region Plan
            var taskDataPlan = lstTaskDetails.Select(t => new
            {
                id = string.Format("Z{0}_L{1}", t.MainParentId, t.PlanId),
                text = t.PlanTitle,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlanNew(viewBy,
                ((viewBy.Equals(PlanGanttTypes.BusinessUnit.ToString(), StringComparison.OrdinalIgnoreCase) ||
                                viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)) ? IsCampaign ? t.Tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? t.Tactic.PlanProgramId : t.Tactic.PlanTacticId)
                : Convert.ToInt32(t.MainParentId)), t.PlanId, campaign, program, (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)) ? tactic.Where(tt => t.lstCustomEntityId.Contains(IsCampaign ? tt.Plan_Campaign_Program.PlanCampaignId
                            : (IsProgram ? tt.PlanProgramId : tt.PlanTacticId))).Select(tt => tt).ToList() : tactic, IsCampaign, IsProgram)),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForPlanNew(viewBy,
                                                          ((viewBy.Equals(PlanGanttTypes.BusinessUnit.ToString(), StringComparison.OrdinalIgnoreCase) ||
                                                          (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)))
                                                          ?
                                                          IsCampaign ? t.Tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? t.Tactic.PlanProgramId : t.Tactic.PlanTacticId)
                                                          : Convert.ToInt32(t.MainParentId)), t.PlanId, campaign, program,
                                                          (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)) ? tactic.Where(tt => t.lstCustomEntityId.Contains(IsCampaign ? tt.Plan_Campaign_Program.PlanCampaignId
                            : (IsProgram ? tt.PlanProgramId : tt.PlanTacticId))).Select(tt => tt).ToList() : tactic, IsCampaign, IsProgram),
                                                          GetMaxEndDateForPlanNew(viewBy,
                                                          ((viewBy.Equals(PlanGanttTypes.BusinessUnit.ToString(), StringComparison.OrdinalIgnoreCase) ||
                                                          (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)))
                                                          ?
                                                          IsCampaign ? t.Tactic.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? t.Tactic.PlanProgramId : t.Tactic.PlanTacticId)
                                                          : Convert.ToInt32(t.MainParentId)), t.PlanId, campaign, program,
                                                          (viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase)) ? tactic.Where(tt => t.lstCustomEntityId.Contains(IsCampaign ? tt.Plan_Campaign_Program.PlanCampaignId
                            : (IsProgram ? tt.PlanProgramId : tt.PlanTacticId))).Select(tt => tt).ToList() : tactic, IsCampaign, IsProgram)),
                progress = t.PlanProgress,
                open = false,
                parent = string.Format("Z{0}", t.MainParentId),
                color = GetColorBasedOnImprovementActivity(improvementTactic, t.PlanId),
                planid = t.PlanId
            }).Select(t => t).Distinct().OrderBy(t => t.text);

            var newTaskDataPlan = taskDataPlan.Select(c => new
            {
                id = c.id,
                text = c.text,
                start_date = c.start_date,
                duration = c.duration,
                progress = taskDataPlan.Where(p => p.id == c.id).Select(p => p.progress).Min(),
                open = c.open,
                parent = c.parent,
                color = c.color + (c.progress > 0 ? "stripe" : ""),
                planid = c.planid
            }).ToList().Distinct().ToList();
            #endregion

            #region Campaign
            var taskDataCampaign = lstTaskDetails.Select(t => new
            {
                id = string.Format("Z{0}_L{1}_C{2}", t.MainParentId, t.PlanId, t.Program.PlanCampaignId),
                text = t.Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Campaign.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, t.Campaign.StartDate, t.Campaign.EndDate),
                progress = t.CampaignProgress, //GetCampaignProgress(tactic, t.Campaign, improvementTactic, t.PlanId),
                open = false,
                parent = string.Format("Z{0}_L{1}", t.MainParentId, t.PlanId),
                color = GetColorBasedOnImprovementActivity(improvementTactic, t.PlanId),
                plancampaignid = t.Program.PlanCampaignId,
                Status = t.Campaign.Status
            }).Select(t => t).Distinct().OrderBy(t => t.text);

            var newTaskDataCampaign = taskDataCampaign.Select(c => new
            {
                id = c.id,
                text = c.text,
                start_date = c.start_date,
                duration = c.duration,
                progress = taskDataCampaign.Where(p => p.id == c.id).Select(p => p.progress).Min(),
                open = c.open,
                parent = c.parent,
                color = c.color + (c.progress == 1 ? " stripe" : (c.progress > 0 ? "stripe" : "")),
                plancampaignid = c.plancampaignid,
                Status = c.Status
            }).ToList().Distinct().ToList();
            #endregion

            #region Program
            var taskDataProgram = lstTaskDetails.Select(t => new
            {
                id = string.Format("Z{0}_L{1}_C{2}_P{3}", t.MainParentId, t.PlanId, t.Program.PlanCampaignId, t.Program.PlanProgramId),
                text = t.Program.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Program.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, t.Program.StartDate, t.Program.EndDate),
                progress = t.ProgramProgress,//GetProgramProgress(tactic, t.Program, improvementTactic, t.PlanId),
                open = false,
                parent = string.Format("Z{0}_L{1}_C{2}", t.MainParentId, t.PlanId, t.Program.PlanCampaignId),
                color = "",
                planprogramid = t.Program.PlanProgramId,
                Status = t.Program.Status
            }).Select(t => t).Distinct().OrderBy(t => t.text);

            var newTaskDataProgram = taskDataProgram.Select(p => new
            {
                id = p.id,
                text = p.text,
                start_date = p.start_date,
                duration = p.duration,
                progress = taskDataProgram.Where(c => c.id == p.id).Select(c => c.progress).Min(),
                open = p.open,
                parent = p.parent,
                color = (p.progress == 1 ? " stripe stripe-no-border " : (p.progress > 0 ? "partialStripe" : "")),
                planprogramid = p.planprogramid,
                Status = p.Status
            }).ToList().Distinct().ToList();
            #endregion

            #region Tactic
            var taskDataTactic = lstTaskDetails.Select(t => new
            {
                id = string.Format("Z{0}_L{1}_C{2}_P{3}_T{4}", t.MainParentId, t.PlanId, t.Program.PlanCampaignId, t.Program.PlanProgramId, t.Tactic.PlanTacticId),
                text = t.Tactic.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.Tactic.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, t.Tactic.StartDate, t.Tactic.EndDate),
                progress = GetTacticProgress(t.Tactic, improvementTactic, t.PlanId),
                open = false,
                parent = string.Format("Z{0}_L{1}_C{2}_P{3}", t.MainParentId, t.PlanId, t.Program.PlanCampaignId, t.Tactic.PlanProgramId),
                color = t.Color,
                plantacticid = t.Tactic.PlanTacticId,
                Status = t.Tactic.Status
            }).OrderBy(t => t.text);

            var newTaskDataTactic = taskDataTactic.Select(t => new
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
                Status = t.Status
            });
            #endregion

            #region Improvement Activities & Tactics
            //// Improvement Activities
            var taskDataImprovementActivity = lstImprovementTaskDetails.Select(ita => new
            {
                id = string.Format("Z{0}_L{1}_M{2}", ita.MainParentId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId),
                text = ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, ita.MinStartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, ita.MinStartDate, CalendarEndDate) - 1,
                progress = 0,
                open = true,
                color = GetColorBasedOnImprovementActivity(improvementTactic, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                ImprovementActivityId = ita.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
                isImprovement = true,
                IsHideDragHandleLeft = ita.MinStartDate < CalendarStartDate,
                IsHideDragHandleRight = true,
                parent = string.Format("Z{0}_L{1}", ita.MainParentId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
            }).Select(i => i).Distinct().ToList();

            //// Improvent Tactics
            string tacticStatusSubmitted = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            string tacticStatusDeclined = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;

            var taskDataImprovementTactic = lstImprovementTaskDetails.Select(it => new
            {
                id = string.Format("Z{0}_L{1}_M{2}_I{3}_Y{4}", it.MainParentId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId, it.ImprovementTactic.ImprovementPlanTacticId, it.ImprovementTactic.ImprovementTacticTypeId),
                text = it.ImprovementTactic.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, it.ImprovementTactic.EffectiveDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, it.ImprovementTactic.EffectiveDate, CalendarEndDate) - 1,
                progress = 0,
                open = true,
                parent = string.Format("Z{0}_L{1}_M{2}", it.MainParentId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId),
                color = string.Concat(Common.GANTT_BAR_CSS_CLASS_PREFIX_IMPROVEMENT, it.ImprovementTactic.ImprovementTacticType.ColorCode.ToLower()),
                isSubmitted = it.ImprovementTactic.Status.Equals(tacticStatusSubmitted),
                isDeclined = it.ImprovementTactic.Status.Equals(tacticStatusDeclined),
                inqs = 0,
                mqls = 0,
                cost = it.ImprovementTactic.Cost,
                cws = 0,
                it.ImprovementTactic.ImprovementPlanTacticId,
                isImprovement = true,
                IsHideDragHandleLeft = it.ImprovementTactic.EffectiveDate < CalendarStartDate,
                IsHideDragHandleRight = true,
                Status = it.ImprovementTactic.Status
            }).Distinct().ToList().OrderBy(t => t.text);
            #endregion

            var finalTaskData = newTaskDataCustomField.Concat<object>(newTaskDataPlan).Concat<object>(taskDataImprovementActivity).Concat<object>(taskDataImprovementTactic).Concat<object>(newTaskDataCampaign).Concat<object>(newTaskDataTactic).Concat<object>(newTaskDataProgram).ToList<object>();

            if (activemenu.Equals(Enums.ActiveMenu.Home))
            {
                #region Prepare tactic list for left side Accordian
                var lstCustomFieldTactics = lstTacticTaskList.Select(pcpt => new
                {
                    PlanTacticId = pcpt.Tactic.PlanTacticId,
                    CustomFieldId = pcpt.CustomFieldId,
                    Title = pcpt.Tactic.Title,
                    TaskId = pcpt.TaskId,
                }).ToList().Distinct().ToList();
                #endregion

                return Json(new
                {
                    customFieldTactics = lstCustomFieldTactics,
                    customFields = lstCustomFields,
                    taskData = finalTaskData,
                    requestCount = requestCount,
                    planYear = planYear,
                    improvementTacticForAccordion = improvementTacticForAccordion, //viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase) ? 0 : improvementTacticForAccordion,
                    improvementTacticTypeForAccordion = improvementTacticTypeForAccordion, //viewBy.Equals(PlanGanttTypes.Custom.ToString(), StringComparison.OrdinalIgnoreCase) ? 0 : improvementTacticTypeForAccordion,
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

        #region Prepare Tactic and Request Tab data
        /// <summary>
        /// Prepare Json result for Vertical tab to be rendered in gantt chart
        /// </summary>
        /// <param name="campaign">list of campaigns</param>
        /// <param name="program">list of programs</param>
        /// <param name="tactic">list of tactics</param>
        /// <param name="improvementTactic">list of improvement tactics</param>
        /// <param name="requestCount">No. of tactic count for Request tab</param>
        /// <param name="planYear">Plan year</param>
        /// <returns>Json result, list of task to be rendered in Gantt chart</returns>
        private JsonResult PrepareTacticAndRequestTabResult(string viewBy, Enums.ActiveMenu activemenu, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic, string requestCount, string planYear, object improvementTacticForAccordion, object improvementTacticTypeForAccordion, string planId, List<ViewByModel> viewByListResult)
        {
            List<object> tacticAndRequestTaskData = GetTaskDetailTactic(viewBy, campaign.ToList(), program.ToList(), tactic.ToList(), improvementTactic);

            if (activemenu.Equals(Enums.ActiveMenu.Home))
            {
                var planCampaignProgramTactic = tactic.ToList().Select(pcpt => new
                {
                    PlanTacticId = pcpt.PlanTacticId,
                    TacticTypeId = pcpt.TacticTypeId,
                    Title = pcpt.Title,
                    TaskId = string.Format("L{0}_C{1}_P{2}_T{3}_Y{4}", pcpt.Plan_Campaign_Program.Plan_Campaign.PlanId, pcpt.Plan_Campaign_Program.PlanCampaignId, pcpt.PlanProgramId, pcpt.PlanTacticId, pcpt.TacticTypeId)
                });

                var tacticType = (tactic.Select(pcpt => pcpt.TacticType)).Select(pcpt => new
                {
                    pcpt.TacticTypeId,
                    pcpt.Title,
                    ColorCode = pcpt.ColorCode
                }).Distinct().OrderBy(pcpt => pcpt.Title);

                //// Modified By Maninder Singh Wadhva PL Ticket#47
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
        private List<ViewByModel> prepareViewByList(bool getViewByList, List<Plan_Campaign_Program_Tactic> tacticForAllTabs)
        {
            List<ViewByModel> lstViewById = new List<ViewByModel>();
            if (getViewByList)
            {
                lstViewById = Common.GetDefaultGanttTypes(tacticForAllTabs.ToList());
                //lstViewById = lstViewById.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
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
            // To get permission status for Plan Edit , By dharmraj PL #519
            //Get all subordinates of current user upto n level
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
            var objPlan = db.Plans.FirstOrDefault(p => p.PlanId == Sessions.PlanId);
            bool IsBusinessUnitEditable = Common.IsBusinessUnitEditable(Sessions.BusinessUnitId);   // Added by Sohel Pathan on 02/07/2014 for PL ticket #563 to apply custom restriction logic on Business Units

            if (IsBusinessUnitEditable)
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
        /// <param name="currentTab">Current Tab.</param>
        /// <returns>Returns list of status as per tab.</returns>
        private List<string> GetStatusAsPerSelectedType(string currentTab, Enums.ActiveMenu objactivemenu)
        {
            List<string> status = new List<string>();

            if (currentTab.Equals(PlanGanttTypes.Request.ToString()))
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
        /// <param name="improvementTactics">Improvement Tactic of current plan.</param>
        /// <returns>Returns improvement tactic type for accordion.</returns>
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
        /// <param name="improvementTactics">Improvement tactic of current plan.</param>
        /// <returns>Return improvement tactic for accordion</returns>
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
        /// Function to get GANTT chart task detail for Tactic.
        /// Modfied By: Maninder Singh Wadhva.
        /// Reason: To show task whose either start or end date or both date are outside current view.
        /// </summary>
        /// <param name="campaign">Campaign of plan version.</param>
        /// <param name="program">Program of plan version.</param>
        /// <param name="tactic">Tactic of plan version.</param>
        /// <returns>Returns list of task for GANNT CHART.</returns>
        public List<object> GetTaskDetailTactic(string viewBy, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic)
        {
            string tacticStatusSubmitted = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Submitted.ToString())).Value;
            string tacticStatusDeclined = Enums.TacticStatusValues.Single(s => s.Key.Equals(Enums.TacticStatus.Decline.ToString())).Value;
            // Added BY Bhavesh
            // Calculate MQL at runtime #376
            List<TacticStageValue> tacticStageRelationList = new List<TacticStageValue>();
            List<Stage> stageList = new List<Stage>();
            int inqLevel = 0;
            int mqlLevel = 0;
            if (viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                string inqstage = Enums.Stage.INQ.ToString();
                string mqlstage = Enums.Stage.MQL.ToString();
                tacticStageRelationList = Common.GetTacticStageRelation(tactic, false);
                stageList = db.Stages.Where(s => s.ClientId == Sessions.User.ClientId).ToList();
                inqLevel = Convert.ToInt32(stageList.Single(s => s.Code == inqstage).Level);
                mqlLevel = Convert.ToInt32(stageList.Single(s => s.Code == mqlstage).Level);
            }

            var taskDataTactic = tactic.Select(t => new
            {
                id = string.Format("L{0}_C{1}_P{2}_T{3}_Y{4}", t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId, t.PlanTacticId, t.TacticTypeId),
                text = t.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, t.StartDate, t.EndDate),
                progress = GetTacticProgress(t, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
                open = false,
                parent = string.Format("L{0}_C{1}_P{2}", t.Plan_Campaign_Program.Plan_Campaign.PlanId, t.Plan_Campaign_Program.PlanCampaignId, t.Plan_Campaign_Program.PlanProgramId),
                color = string.Concat(GANTT_BAR_CSS_CLASS_PREFIX, t.TacticType.ColorCode.ToLower()),
                isSubmitted = t.Status.Equals(tacticStatusSubmitted),
                isDeclined = t.Status.Equals(tacticStatusDeclined),
                projectedStageValue = viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase) ? stageList.Single(s => s.StageId == t.StageId).Level <= inqLevel ? Convert.ToString(tacticStageRelationList.Single(tm => tm.TacticObj.PlanTacticId == t.PlanTacticId).INQValue) : "N/A" : "0",
                mqls = viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase) ? stageList.Single(s => s.StageId == t.StageId).Level <= mqlLevel ? Convert.ToString(tacticStageRelationList.Single(tm => tm.TacticObj.PlanTacticId == t.PlanTacticId).MQLValue) : "N/A" : "0",
                cost = t.Cost,
                cws = viewBy.Equals(PlanGanttTypes.Request.ToString(), StringComparison.OrdinalIgnoreCase) ? t.Status.Equals(tacticStatusSubmitted) || t.Status.Equals(tacticStatusDeclined) ? Math.Round(tacticStageRelationList.Single(tm => tm.TacticObj.PlanTacticId == t.PlanTacticId).RevenueValue, 1) : 0 : 0,
                plantacticid = t.PlanTacticId,
                Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            }).OrderBy(t => t.text);

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
                isSubmitted = t.isSubmitted,
                isDeclined = t.isDeclined,
                projectedStageValue = t.projectedStageValue,
                mqls = t.mqls,
                cost = t.cost,
                cws = t.cws,
                plantacticid = t.plantacticid,
                Status = t.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            });

            var taskDataProgram = tactic.Select(p => new
            {
                id = string.Format("L{0}_C{1}_P{2}", p.Plan_Campaign_Program.Plan_Campaign.PlanId, p.Plan_Campaign_Program.PlanCampaignId, p.PlanProgramId),
                text = p.Plan_Campaign_Program.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, p.Plan_Campaign_Program.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, p.Plan_Campaign_Program.StartDate, p.Plan_Campaign_Program.EndDate),
                progress = GetProgramProgress(tactic, p.Plan_Campaign_Program, improvementTactic, p.Plan_Campaign_Program.Plan_Campaign.PlanId),
                open = false,
                parent = string.Format("L{0}_C{1}", p.Plan_Campaign_Program.Plan_Campaign.PlanId, p.Plan_Campaign_Program.PlanCampaignId),
                color = "",
                planprogramid = p.PlanProgramId,
                Status = p.Plan_Campaign_Program.Status     //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            }).Select(p => p).Distinct().OrderBy(p => p.text);

            var newTaskDataProgram = taskDataProgram.Select(p => new
            {
                id = p.id,
                text = p.text,
                start_date = p.start_date,
                duration = p.duration,
                progress = p.progress,
                open = p.open,
                parent = p.parent,
                color = (p.progress == 1 ? " stripe stripe-no-border " : (p.progress > 0 ? "partialStripe" : "")),
                planprogramid = p.planprogramid,
                Status = p.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            });

            var taskDataCampaign = tactic.Select(c => new
            {
                id = string.Format("L{0}_C{1}", c.Plan_Campaign_Program.Plan_Campaign.PlanId, c.Plan_Campaign_Program.PlanCampaignId),
                text = c.Plan_Campaign_Program.Plan_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, c.Plan_Campaign_Program.Plan_Campaign.StartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, c.Plan_Campaign_Program.Plan_Campaign.StartDate, c.Plan_Campaign_Program.Plan_Campaign.EndDate),
                progress = GetCampaignProgress(tactic, c.Plan_Campaign_Program.Plan_Campaign, improvementTactic, c.Plan_Campaign_Program.Plan_Campaign.PlanId),
                open = false,
                parent = string.Format("L{0}", c.Plan_Campaign_Program.Plan_Campaign.PlanId),
                color = GetColorBasedOnImprovementActivity(improvementTactic, c.Plan_Campaign_Program.Plan_Campaign.PlanId),
                plancampaignid = c.Plan_Campaign_Program.PlanCampaignId,
                Status = c.Plan_Campaign_Program.Plan_Campaign.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            }).Select(c => c).Distinct().OrderBy(c => c.text);

            var newTaskDataCampaign = taskDataCampaign.Select(c => new
            {
                id = c.id,
                text = c.text,
                start_date = c.start_date,
                duration = c.duration,
                progress = c.progress,
                open = c.open,
                parent = c.parent,
                color = c.color + (c.progress == 1 ? " stripe" : (c.progress > 0 ? "stripe" : "")),
                plancampaignid = c.plancampaignid,
                Status = c.Status       //// Added by Sohel on 16/05/2014 for PL #425 to Show status of tactics on Home and Plan screen
            });

            var taskDataPlan = tactic.Select(t => new
            {
                id = string.Format("L{0}", t.Plan_Campaign_Program.Plan_Campaign.PlanId),
                text = t.Plan_Campaign_Program.Plan_Campaign.Plan.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Tactic, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForPlan(GanttTabs.Tactic, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic),
                                                          GetMaxEndDateForPlan(GanttTabs.Tactic, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
                progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.Tactic, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
                                                Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForPlan(GanttTabs.Tactic, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic),
                                                          GetMaxEndDateForPlan(GanttTabs.Tactic, t.PlanTacticId, t.Plan_Campaign_Program.Plan_Campaign.PlanId, campaign, program, tactic)),
                                               tactic, improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
                open = false,
                color = GetColorBasedOnImprovementActivity(improvementTactic, t.Plan_Campaign_Program.Plan_Campaign.PlanId),
                planid = t.Plan_Campaign_Program.Plan_Campaign.PlanId
            }).Select(t => t).Distinct().OrderBy(t => t.text);

            var newTaskDataPlan = taskDataPlan.Select(c => new
            {
                id = c.id,
                text = c.text,
                start_date = c.start_date,
                duration = c.duration,
                progress = c.progress,
                open = c.open,
                color = c.color + (c.progress > 0 ? "stripe" : ""),
                planid = c.planid
            });

            var planIdList = tactic.Select(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId).ToList().Distinct();

            var taskDataPlanForImprovement = improvementTactic.Where(it => !planIdList.Contains(it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId)).Select(t => new
            {
                id = string.Format("L{0}", t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                text = t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.None, t.ImprovementPlanTacticId, t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, campaign, program, tactic)),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForPlan(GanttTabs.None, t.ImprovementPlanTacticId, t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, campaign, program, tactic),
                                                          GetMaxEndDateForPlan(GanttTabs.None, t.ImprovementPlanTacticId, t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, campaign, program, tactic)),
                progress = GetProgress(Common.GetStartDateAsPerCalendar(CalendarStartDate, GetMinStartDateForPlan(GanttTabs.None, t.ImprovementPlanTacticId, t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, campaign, program, tactic)),
                                                Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate,
                                                          GetMinStartDateForPlan(GanttTabs.None, t.ImprovementPlanTacticId, t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, campaign, program, tactic),
                                                          GetMaxEndDateForPlan(GanttTabs.None, t.ImprovementPlanTacticId, t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, campaign, program, tactic)),
                                               tactic, improvementTactic, t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                open = false,
                color = GetColorBasedOnImprovementActivity(improvementTactic, t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                planid = t.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId
            }).Select(t => t).Distinct().OrderBy(t => t.text);

            var newTaskDataPlanForImprovement = taskDataPlanForImprovement.Select(c => new
            {
                id = c.id,
                text = c.text,
                start_date = c.start_date,
                duration = c.duration,
                progress = c.progress,
                open = c.open,
                color = c.color + (c.progress > 0 ? "stripe" : ""),
                planid = c.planid
            });

            var taskDataPlanMerged = newTaskDataPlan.Concat<object>(newTaskDataPlanForImprovement).ToList().Distinct();

            #region Improvement Activities & Tactics
            var improvemntTacticList = improvementTactic.Select(it => new
            {
                ImprovementTactic = it,
                //// Getting start date for improvement activity task.
                minStartDate = improvementTactic.Where(impt => impt.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Select(impt => impt.EffectiveDate).Min(),
            }).Select(a => a).ToList().Distinct().ToList();

            //// Improvement Activities
            var taskDataImprovementActivity = improvemntTacticList.Select(ita => new
            {
                id = string.Format("L{0}_M{1}", ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId),
                text = ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, ita.minStartDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, ita.minStartDate, CalendarEndDate) - 1,
                progress = 0,
                open = true,
                color = GetColorBasedOnImprovementActivity(improvementTactic, ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
                ImprovementActivityId = ita.ImprovementTactic.Plan_Improvement_Campaign_Program.ImprovementPlanCampaignId,
                isImprovement = true,
                IsHideDragHandleLeft = ita.minStartDate < CalendarStartDate,
                IsHideDragHandleRight = true,
                parent = string.Format("L{0}", ita.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId),
            }).Select(i => i).Distinct().ToList();

            var taskDataImprovementTactic = improvemntTacticList.Select(it => new
            {
                id = string.Format("L{0}_M{1}_I{2}_Y{3}", it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId, it.ImprovementTactic.ImprovementPlanTacticId, it.ImprovementTactic.ImprovementTacticTypeId),
                text = it.ImprovementTactic.Title,
                start_date = Common.GetStartDateAsPerCalendar(CalendarStartDate, it.ImprovementTactic.EffectiveDate),
                duration = Common.GetEndDateAsPerCalendar(CalendarStartDate, CalendarEndDate, it.ImprovementTactic.EffectiveDate, CalendarEndDate) - 1,
                progress = 0,
                open = true,
                parent = string.Format("L{0}_M{1}", it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId, it.ImprovementTactic.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovementPlanCampaignId),
                color = string.Concat(Common.GANTT_BAR_CSS_CLASS_PREFIX_IMPROVEMENT, it.ImprovementTactic.ImprovementTacticType.ColorCode.ToLower()),
                isSubmitted = it.ImprovementTactic.Status.Equals(tacticStatusSubmitted),
                isDeclined = it.ImprovementTactic.Status.Equals(tacticStatusDeclined),
                inqs = 0,
                mqls = 0,
                cost = it.ImprovementTactic.Cost,
                cws = 0,
                it.ImprovementTactic.ImprovementPlanTacticId,
                isImprovement = true,
                IsHideDragHandleLeft = it.ImprovementTactic.EffectiveDate < CalendarStartDate,
                IsHideDragHandleRight = true,
                Status = it.ImprovementTactic.Status
            }).OrderBy(t => t.text);
            #endregion

            return taskDataPlanMerged.Concat<object>(taskDataImprovementActivity).Concat<object>(taskDataImprovementTactic).Concat<object>(newTaskDataCampaign).Concat<object>(NewTaskDataTactic).Concat<object>(newTaskDataProgram).ToList<object>();
        }

        /// <summary>
        /// Function to get tactic progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 2nd april, 2013
        /// <param name="planCampaignProgramTactic"></param>
        /// <param name="improvementTactic"></param>
        /// <returns>return progress B/W 0 and 1</returns>
        public double GetTacticProgress(Plan_Campaign_Program_Tactic planCampaignProgramTactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic, int PlanId)
        {
            if (improvementTactic.Count > 0 && improvementTactic.Where(it => it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Any())
            {
                DateTime minDate = improvementTactic.Where(it => it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Select(t => t.EffectiveDate).Min(); // Minimun date of improvement tactic

                DateTime tacticStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaignProgramTactic.StartDate)); // start Date of tactic

                if (tacticStartDate >= minDate) // If any tactic affected by at least one improvement tactic.
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
        /// <param name="planCampaignProgram"></param>
        /// <param name="improvementTactic"></param>
        /// <returns>return progress B/W 0 and 1</returns>
        public double GetProgramProgress(List<Plan_Campaign_Program_Tactic> tactic, Plan_Campaign_Program planCampaignProgram, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic, int PlanId)
        {
            if (improvementTactic.Count > 0 && improvementTactic.Where(it => it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Any())
            {
                DateTime minDate = improvementTactic.Where(it => it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Select(t => t.EffectiveDate).Min(); // Minimun date of improvement tactic

                // Start date of program
                DateTime programStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaignProgram.StartDate));

                // List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = tactic.Where(p => p.IsDeleted.Equals(false) && p.PlanProgramId == planCampaignProgram.PlanProgramId && (p.StartDate >= minDate).Equals(true)
                                                                                                && p.Plan_Campaign_Program.Plan_Campaign.PlanId == PlanId)
                                                                                              .Select(t => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate)) })
                                                                                              .ToList();

                if (lstAffectedTactic.Count > 0)
                {
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(t => t.startDate).Min(); // minimum start Date of tactics
                    if (tacticMinStartDate >= minDate) // If any tactic affected by at least one improvement tactic
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
        /// </summary>
        /// <param name="planCampaign"></param>
        /// <param name="improvementTactic"></param>
        /// <returns>return progress B/W 0 and 1</returns>
        public double GetCampaignProgress(List<Plan_Campaign_Program_Tactic> tactic, Plan_Campaign planCampaign, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic, int PlanId)
        {
            if (improvementTactic.Count > 0 && improvementTactic.Where(it => it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Any())
            {
                DateTime minDate = improvementTactic.Where(it => it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Select(t => t.EffectiveDate).Min(); // Minimun date of improvement tactic

                // Start date of Campaign
                DateTime campaignStartDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, planCampaign.StartDate));

                // List of all tactics
                var lstTactic = tactic.Where(p => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                        CalendarEndDate,
                                                                                                                        p.StartDate,
                                                                                                                        p.EndDate).Equals(false)
                                                                                                                        && p.Plan_Campaign_Program.Plan_Campaign.PlanId == PlanId);

                // List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = lstTactic.Where(p => (p.StartDate >= minDate).Equals(true))
                                                 .Select(t => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate)) })
                                                 .ToList();

                if (lstAffectedTactic.Count > 0)
                {
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(t => t.startDate).Min(); // minimum start Date of tactics
                    if (tacticMinStartDate >= minDate) // If any tactic affected by at least one improvement tactic.
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
        /// Function to get progress. Ticket #394 Apply styling on improvement activity in calendar
        /// Added By: Dharmraj mangukiya.
        /// Date: 3rd april, 2013
        /// </summary>
        /// <param name="StartDate"></param>
        /// <param name="Duration"></param>
        /// <param name="tactic"></param>
        /// <param name="improvementTactic"></param>
        /// <returns>return progress B/W 0 and 1</returns>
        public double GetProgress(string taskStartDate, double taskDuration, List<Plan_Campaign_Program_Tactic> tactic, List<Plan_Improvement_Campaign_Program_Tactic> improvementTactic, int PlanId)
        {
            if (improvementTactic.Count > 0 && improvementTactic.Where(it => it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Any())
            {
                DateTime minDate = improvementTactic.Where(it => it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == PlanId).Select(t => t.EffectiveDate).Min(); // Minimun date of improvement tactic

                // List of all tactics that are affected by improvement tactic
                var lstAffectedTactic = tactic.Where(p => (p.StartDate >= minDate).Equals(true) && p.Plan_Campaign_Program.Plan_Campaign.PlanId == PlanId)
                                                 .Select(t => new { startDate = Convert.ToDateTime(Common.GetStartDateAsPerCalendar(CalendarStartDate, t.StartDate)) })
                                                 .ToList();

                if (lstAffectedTactic.Count > 0)
                {
                    DateTime tacticMinStartDate = lstAffectedTactic.Select(t => t.startDate).Min(); // minimum start Date of tactics
                    if (tacticMinStartDate >= minDate) // If any tactic affected by at least one improvement tactic.
                    {
                        // difference b/w task start date and tactic minimum date
                        double daysDifference = (tacticMinStartDate - Convert.ToDateTime(taskStartDate)).TotalDays;

                        if (daysDifference > 0) // If no. of days are more then zero then it will return progress
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
                    return 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Added By :- Sohel Pathan
        /// Date :- 09/10/2014
        /// Description :- To get minimum start date for custom field
        /// </summary>
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic , Vertical and so on)</param>
        /// <param name="typeId">Id</param>
        /// <param name="campaign">List of campaign </param>
        /// <param name="program">List of Program</param>
        /// <param name="tactic">List of Tactic</param>
        /// <returns>Return the min start date fo program and Campaign</returns>
        public DateTime GetMinStartDateForCustomField(PlanGanttTypes currentGanttTab, int typeId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, bool IsCampaing = false, bool IsProgram = false)
        {
            var queryPlanProgramId = new List<int>();

            DateTime minDateTactic = DateTime.Now;
            switch (currentGanttTab)
            {
                //If selected plan Gantt type is Vertical at that time we will extract tactic based on vertical id
                case PlanGanttTypes.Vertical:
                    queryPlanProgramId = tactic.Where(t => t.VerticalId == typeId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.VerticalId == typeId).Select(t => t.StartDate).Min();
                    break;
                //If selected plan Gantt type is Audience at that time we will extract tactic based on Audience id
                case PlanGanttTypes.Audience:
                    queryPlanProgramId = tactic.Where(t => t.AudienceId == typeId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.AudienceId == typeId).Select(t => t.StartDate).Min();
                    break;
                //If selected plan Gantt type is Custom at that time we will extract tactic based on Custom id
                case PlanGanttTypes.Custom:
                    queryPlanProgramId = tactic.Where(t => (IsCampaing ? t.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? t.PlanProgramId : t.PlanTacticId)) == typeId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => (IsCampaing ? t.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? t.PlanProgramId : t.PlanTacticId)) == typeId).Select(t => t.StartDate).Min();
                    break;

                default:
                    break;
            }

            //Get the min start date of Program and Campaign.
            var queryPlanCampaignId = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.PlanCampaignId).ToList<int>();
            DateTime minDateProgram = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.StartDate).Min();

            DateTime minDateCampaign = campaign.Where(c => queryPlanCampaignId.Contains(c.PlanCampaignId)).Select(c => c.StartDate).Min();

            return new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
        }

        /// <summary>
        /// Get min start date using planID
        /// </summary>
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic , Vertical and so on)</param>
        /// <param name="typeId">Selected Id</param>
        /// /// <param name="planid">Planid</param>
        /// <param name="campaign">List of campaign </param>
        /// <param name="program">List of Program</param>
        /// <param name="tactic">List of Tactic</param>
        /// <returns>Return the min start date fo program and Campaign</returns>
        public DateTime GetMinStartDateForPlan(GanttTabs currentGanttTab, int typeId, int planId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
        {
            var queryPlanProgramId = new List<int>();

            DateTime minDateTactic = DateTime.Now;
            //Check the case with selected plan gantt type and if it's match then extract the min date from tactic list 
            switch (currentGanttTab)
            {
                case GanttTabs.Vertical:
                    queryPlanProgramId = tactic.Where(t => t.VerticalId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.VerticalId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.StartDate).Min();
                    break;
                case GanttTabs.Stage:
                    queryPlanProgramId = tactic.Where(t => t.StageId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.StageId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.StartDate).Min();
                    break;
                case GanttTabs.Audience:
                    queryPlanProgramId = tactic.Where(t => t.AudienceId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.AudienceId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.StartDate).Min();
                    break;
                case GanttTabs.Tactic:
                    queryPlanProgramId = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.StartDate).ToList().Min();
                    break;
                case GanttTabs.BusinessUnit:
                    var businessUnitId = tactic.Where(t => t.PlanTacticId == typeId).Select(t => t.BusinessUnitId).FirstOrDefault();
                    queryPlanProgramId = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId && t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId == businessUnitId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId && t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId == businessUnitId).Select(t => t.StartDate).Min();
                    break;
                case GanttTabs.None:
                    queryPlanProgramId = program.Where(p => p.Plan_Campaign.PlanId == planId).Select(p => p.PlanProgramId).ToList<int>();
                    break;
                default:
                    break;
            }

            //Get the min start date of Program and Campaign.
            //Modified By : Kalpesh Sharma
            //#958 Change of Plan is not working (Some Cases)
            var queryPlanCampaignId = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.PlanCampaignId).ToList<int>();
            DateTime minDateProgram = queryPlanProgramId.Count > 0 ? program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.StartDate).Min() : DateTime.MinValue;

            DateTime minDateCampaign = queryPlanCampaignId.Count() > 0 ? campaign.Where(c => queryPlanCampaignId.Contains(c.PlanCampaignId)).Select(c => c.StartDate).Min() : DateTime.MinValue;

            return new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
        }

        /// <summary>
        /// Get min start date using planID
        /// </summary>
        /// <param name="currentGanttTab">Selected Plan Gantt Type (Tactic , Vertical and so on)</param>
        /// <param name="typeId">Selected Id</param>
        /// /// <param name="planid">Planid</param>
        /// <param name="campaign">List of campaign </param>
        /// <param name="program">List of Program</param>
        /// <param name="tactic">List of Tactic</param>
        /// <returns>Return the min start date fo program and Campaign</returns>
        public DateTime GetMinStartDateForPlanNew(string currentGanttTab, int typeId, int planId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, bool IsCamaping = false, bool IsProgram = false)
        {
            var queryPlanProgramId = new List<int>();
            DateTime minDateTactic = DateTime.Now;
            PlanGanttTypes objPlanGanttTypes = (PlanGanttTypes)Enum.Parse(typeof(PlanGanttTypes), currentGanttTab, true);
            //Check the case with selected plan gantt type and if it's match then extract the min date from tactic list
            switch (objPlanGanttTypes)
            {
                case PlanGanttTypes.Vertical:
                    queryPlanProgramId = tactic.Where(t => t.VerticalId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.VerticalId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.StartDate).Min();
                    break;
                case PlanGanttTypes.Stage:
                    queryPlanProgramId = tactic.Where(t => t.StageId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.StageId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.StartDate).Min();
                    break;
                case PlanGanttTypes.Audience:
                    queryPlanProgramId = tactic.Where(t => t.AudienceId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.AudienceId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.StartDate).Min();
                    break;
                case PlanGanttTypes.Tactic:
                    queryPlanProgramId = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.StartDate).Min();
                    break;
                case PlanGanttTypes.BusinessUnit:
                    var businessUnitId = tactic.Where(t => t.PlanTacticId == typeId).Select(t => t.BusinessUnitId).FirstOrDefault();
                    queryPlanProgramId = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId && t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId == businessUnitId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId && t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId == businessUnitId).Select(t => t.StartDate).Min();
                    break;
                case PlanGanttTypes.Custom:
                    queryPlanProgramId = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    minDateTactic = tactic.Where(t => (IsCamaping ? t.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? t.PlanProgramId : t.PlanTacticId)) == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.StartDate).Min();
                    break;
                default:
                    break;
            }

            var queryPlanCampaignId = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.PlanCampaignId).ToList<int>();
            DateTime minDateProgram = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.StartDate).Min();

            DateTime minDateCampaign = campaign.Where(c => queryPlanCampaignId.Contains(c.PlanCampaignId)).Select(c => c.StartDate).Min();

            return new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
        }

        /// <summary>
        /// Added By :- Sohel Pathan
        /// Date :- 09/10/2014
        /// Description :- To get maximum end date for custom field
        /// </summary>
        /// <param name="currentGanttTab"></param>
        /// <param name="typeId"></param>
        /// <param name="campaign"></param>
        /// <param name="program"></param>
        /// <param name="tactic"></param>
        /// <returns></returns>
        public DateTime GetMaxEndDateForCustomField(PlanGanttTypes currentGanttTab, int typeId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, bool IsCamaping = false, bool IsProgram = false)
        {
            var queryPlanProgramId = new List<int>();

            DateTime maxDateTactic = DateTime.Now;

            switch (currentGanttTab)
            {
                case PlanGanttTypes.Vertical:
                    queryPlanProgramId = tactic.Where(t => t.VerticalId == typeId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.VerticalId == typeId).Select(t => t.EndDate).Max();
                    break;
                case PlanGanttTypes.Audience:
                    queryPlanProgramId = tactic.Where(t => t.AudienceId == typeId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.AudienceId == typeId).Select(t => t.EndDate).Max();
                    break;
                case PlanGanttTypes.Custom:
                    queryPlanProgramId = tactic.Where(t => (IsCamaping ? t.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? t.PlanProgramId : t.PlanTacticId)) == typeId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => (IsCamaping ? t.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? t.PlanProgramId : t.PlanTacticId)) == typeId).Select(t => t.EndDate).Max();
                    break;
                default:
                    break;
            }

            var queryPlanCampaignId = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.PlanCampaignId).ToList<int>();
            DateTime maxDateProgram = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.EndDate).Max();

            DateTime maxDateCampaign = campaign.Where(c => queryPlanCampaignId.Contains(c.PlanCampaignId)).Select(c => c.EndDate).Max();

            return new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
        }

        /// <summary>
        /// Get max end date using planID
        /// </summary>
        /// <param name="currentGanttTab"></param>
        /// <param name="typeId"></param>
        /// <param name="planId"></param>
        /// <param name="campaign"></param>
        /// <param name="program"></param>
        /// <param name="tactic"></param>
        /// <returns></returns>
        public DateTime GetMaxEndDateForPlan(GanttTabs currentGanttTab, int typeId, int planId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
        {
            var queryPlanProgramId = new List<int>();

            DateTime maxDateTactic = DateTime.Now;

            switch (currentGanttTab)
            {
                case GanttTabs.Vertical:
                    queryPlanProgramId = tactic.Where(t => t.VerticalId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.VerticalId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.EndDate).Max();
                    break;
                case GanttTabs.Stage:
                    queryPlanProgramId = tactic.Where(t => t.StageId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.StageId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.EndDate).Max();
                    break;
                case GanttTabs.Audience:
                    queryPlanProgramId = tactic.Where(t => t.AudienceId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.AudienceId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.EndDate).Max();
                    break;
                case GanttTabs.Tactic:
                    queryPlanProgramId = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.EndDate).Max();
                    break;
                case GanttTabs.BusinessUnit:
                    var businessUnitId = tactic.Where(t => t.PlanTacticId == typeId).Select(t => t.BusinessUnitId).FirstOrDefault();
                    queryPlanProgramId = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId && t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId == businessUnitId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId && t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId == businessUnitId).Select(t => t.EndDate).Max();
                    break;
                case GanttTabs.None:
                    queryPlanProgramId = program.Where(p => p.Plan_Campaign.PlanId == planId).Select(p => p.PlanProgramId).ToList<int>();
                    break;
                default:
                    break;
            }

            var queryPlanCampaignId = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.PlanCampaignId).ToList<int>();
            // Modified By : Kalpesh Sharma 
            // #958 : Change of Plan is not working (Some Cases)

            DateTime maxDateProgram = queryPlanProgramId.Count > 0 ? program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.EndDate).Max() : DateTime.MaxValue;

            DateTime maxDateCampaign = queryPlanCampaignId.Count > 0 ? campaign.Where(c => queryPlanCampaignId.Contains(c.PlanCampaignId)).Select(c => c.EndDate).Max() : DateTime.MaxValue;

            return new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
        }

        /// <summary>
        /// Get max end date using planID
        /// </summary>
        /// <param name="currentGanttTab"></param>
        /// <param name="typeId"></param>
        /// <param name="planId"></param>
        /// <param name="campaign"></param>
        /// <param name="program"></param>
        /// <param name="tactic"></param>
        /// <returns></returns>
        public DateTime GetMaxEndDateForPlanNew(string currentGanttTab, int typeId, int planId, List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic, bool IsCamaping = false, bool IsProgram = false)
        {
            var queryPlanProgramId = new List<int>();
            DateTime maxDateTactic = DateTime.Now;
            PlanGanttTypes objPlanGanttTypes = (PlanGanttTypes)Enum.Parse(typeof(PlanGanttTypes), currentGanttTab, true);
            switch (objPlanGanttTypes)
            {
                case PlanGanttTypes.Vertical:
                    queryPlanProgramId = tactic.Where(t => t.VerticalId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.VerticalId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.EndDate).Max();
                    break;
                case PlanGanttTypes.Stage:
                    queryPlanProgramId = tactic.Where(t => t.StageId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.StageId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.EndDate).Max();
                    break;
                case PlanGanttTypes.Audience:
                    queryPlanProgramId = tactic.Where(t => t.AudienceId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.AudienceId == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.EndDate).Max();
                    break;
                case PlanGanttTypes.Tactic:
                    queryPlanProgramId = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.EndDate).Max();
                    break;
                case PlanGanttTypes.BusinessUnit:
                    var businessUnitId = tactic.Where(t => t.PlanTacticId == typeId).Select(t => t.BusinessUnitId).FirstOrDefault();
                    queryPlanProgramId = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId && t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId == businessUnitId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId && t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.BusinessUnitId == businessUnitId).Select(t => t.EndDate).Max();
                    break;
                case PlanGanttTypes.Custom:
                    queryPlanProgramId = tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.PlanProgramId).ToList<int>();
                    maxDateTactic = tactic.Where(t => (IsCamaping ? t.Plan_Campaign_Program.PlanCampaignId : (IsProgram ? t.PlanProgramId : t.PlanTacticId)) == typeId && t.Plan_Campaign_Program.Plan_Campaign.PlanId == planId).Select(t => t.EndDate).Max();
                    break;
                default:
                    break;
            }

            var queryPlanCampaignId = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.PlanCampaignId).ToList<int>();
            DateTime maxDateProgram = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.EndDate).Max();

            DateTime maxDateCampaign = campaign.Where(c => queryPlanCampaignId.Contains(c.PlanCampaignId)).Select(c => c.EndDate).Max();

            return new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/19/2013
        /// Function to get minimum date for stage task.
        /// </summary>
        /// <param name="campaign">Campaign of plan version.</param>
        /// <param name="program">Program of plan version.</param>
        /// <param name="tactic">Tactic of plan version.</param>
        /// <returns>Returns minimum date for stage task for GANNT CHART.</returns>
        public DateTime GetMinStartDateStageAndBusinessUnit(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
        {
            //// Tactic and Model Tactic
            var queryPlanProgramId = tactic.Select(tmt => tmt.PlanProgramId).ToList<int>();
            var queryPlanCampaignId = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.PlanCampaignId).ToList<int>();

            DateTime minDateTactic = tactic.Select(t => t.StartDate).Min();

            DateTime minDateProgram = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.StartDate).Min();

            DateTime minDateCampaign = campaign.Where(c => queryPlanCampaignId.Contains(c.PlanCampaignId)).Select(c => c.StartDate).Min();

            return new[] { minDateTactic, minDateProgram, minDateCampaign }.Min();
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 11/19/2013
        /// Function to get maximum date for stage task.
        /// Modfied By: Maninder Singh Wadhva.
        /// Reason: To show task whose either start or end date or both date are outside current view.
        /// </summary>
        /// <param name="campaign">Campaign of plan version.</param>
        /// <param name="program">Program of plan version.</param>
        /// <param name="tactic">Tactic of plan version.</param>
        /// <returns>Returns maximum date for stage task for GANNT CHART.</returns>
        public DateTime GetMaxEndDateStageAndBusinessUnit(List<Plan_Campaign> campaign, List<Plan_Campaign_Program> program, List<Plan_Campaign_Program_Tactic> tactic)
        {
            //// Tactic and Model Tactic
            var queryPlanProgramId = tactic.Select(tmt => tmt.PlanProgramId).ToList<int>();
            var queryPlanCampaignId = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.PlanCampaignId).ToList<int>();

            DateTime maxDateTactic = tactic.Select(t => t.EndDate).Max();

            DateTime maxDateProgram = program.Where(p => queryPlanProgramId.Contains(p.PlanProgramId)).Select(p => p.EndDate).Max();

            DateTime maxDateCampaign = campaign.Where(c => queryPlanCampaignId.Contains(c.PlanCampaignId)).Select(c => c.EndDate).Max();

            return new[] { maxDateTactic, maxDateProgram, maxDateCampaign }.Max();
        }

        /// <summary>
        /// Created By : Sohel Pathan
        /// Created Date : 19/09/2014
        /// Description : Get upper border for plan and campaign, if plan does have improvement tactic
        /// </summary>
        /// <param name="improvementTactics">list of improvement tactics of one or more plans</param>
        /// <param name="planId">plan id of selected plan</param>
        /// <returns>upper border classs</returns>
        public string GetColorBasedOnImprovementActivity(List<Plan_Improvement_Campaign_Program_Tactic> improvementTactics, int planId)
        {
            if (improvementTactics.Count > 0)
            {
                if (improvementTactics.Select(i => i.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId).Contains(planId))
                {
                    return Common.COLORC6EBF3_WITH_BORDER;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        #endregion

        #region "Inspect"

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Save Tactic Details.
        /// </summary>
        /// <param name="form">Inspect Model object</param>
        /// <returns>Returns Partial View Of Inspect Popup.</returns>
        [HttpPost]
        public JsonResult SaveTactic(InspectModel form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Plan_Campaign_Program_Tactic tactic = db.Plan_Campaign_Program_Tactic.Where(pt => pt.PlanTacticId == form.PlanTacticId).SingleOrDefault();
                    if (form.PlanProgramId != 0)
                    {
                        tactic.PlanProgramId = form.PlanProgramId;
                    }

                    if (form.TacticTypeId != 0)
                    {
                        tactic.TacticTypeId = form.TacticTypeId;
                    }

                    if (form.TacticTitle.Trim() != string.Empty)
                    {
                        tactic.Title = form.TacticTitle.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(form.Description))
                    {
                        tactic.Description = form.Description.Trim();
                    }

                    if (form.VerticalId != 0)
                    {
                        tactic.VerticalId = form.tacticVerticalId;      /* changed by Nirav on 11 APR for PL 322*/
                    }

                    if (form.AudienceId != 0)
                    {
                        tactic.AudienceId = form.tacticAudienceId;      /* changed by Nirav on 11 APR for PL 322*/
                    }

                    tactic.StartDate = Convert.ToDateTime(form.StartDate);
                    tactic.EndDate = Convert.ToDateTime(form.EndDate);
                    tactic.Status = form.Status;
                    tactic.Cost = Convert.ToDouble(form.Cost);
                    tactic.ModifiedBy = Sessions.User.UserId;
                    tactic.ModifiedDate = DateTime.Now;
                    db.Entry(tactic).State = EntityState.Modified;
                    int result = db.SaveChanges();
                    return Json(new { id = form.PlanTacticId, TabValue = "Review" });
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { id = form.PlanTacticId, TabValue = "Setup" });
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Program List.
        /// </summary>
        /// <param name="campaignId">Campaign Id</param>
        /// <returns>Returns Json Result of Campaign List.</returns>
        [HttpPost]
        public JsonResult LoadProgram(int campaignId)
        {
            var program = db.Plan_Campaign_Program.Where(pcp => pcp.PlanCampaignId == campaignId).OrderBy(pcp => pcp.Title);
            if (program == null)
                return Json(null);
            var programList = (from p in program
                               select new
                               {
                                   p.PlanProgramId,
                                   p.Title
                               }
                               ).ToList();
            return Json(programList, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Added By: Pratik Chauhan.
        /// Action to Create Improvement Tactic.
        /// </summary>
        /// <returns>Returns Partial View Of Tactic.</returns>
        public PartialViewResult CreateImprovementTactic(int id = 0)
        {
            List<int> impTacticList = db.Plan_Improvement_Campaign_Program_Tactic.Where(it => it.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.ImprovePlanId == Sessions.PlanId && it.IsDeleted == false).Select(it => it.ImprovementTacticTypeId).ToList();
            ViewBag.Tactics = from t in db.ImprovementTacticTypes
                              where t.ClientId == Sessions.User.ClientId && t.IsDeployed == true && !impTacticList.Contains(t.ImprovementTacticTypeId)
                              && t.IsDeleted == false
                              orderby t.Title
                              select t;
            ViewBag.IsCreated = true;

            var objPlan = db.Plans.SingleOrDefault(varP => varP.PlanId == Sessions.PlanId);
            ViewBag.ExtIntService = Common.CheckModelIntegrationExist(objPlan.Model);
            PlanImprovementTactic pitm = new PlanImprovementTactic();
            pitm.ImprovementPlanProgramId = id;
            // Set today date as default for effective date.
            pitm.EffectiveDate = DateTime.Now;
            pitm.IsDeployedToIntegration = false;

            ViewBag.IsOwner = true;
            ViewBag.RedirectType = false;
            ViewBag.Year = db.Plans.Single(p => p.PlanId.Equals(Sessions.PlanId)).Year;
            return PartialView("_SetupImprovementTactic", pitm);
        }

        #endregion

        #region "Home-Zero"
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
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("ServiceUnavailable", "Login");
                }

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }

            return View();
        }
        #endregion

        #region GetNumberOfActivityPerMonthByPlanId
        /// <summary>
        /// Get Number of Activity Per Month for Activity Distribution Chart.
        /// </summary>
        /// <param name="planid">Plan Id.</param>
        /// <returns>JsonResult.</returns>
        public JsonResult GetNumberOfActivityPerMonthByPlanId(int planid, string strparam)
        {
            string planYear = db.Plans.Single(p => p.PlanId.Equals(planid)).Year;
            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;
            Common.GetPlanGanttStartEndDate(planYear, strparam, ref CalendarStartDate, ref CalendarEndDate);

            //Start Maninder Singh Wadhva : 11/15/2013 - Getting list of tactic for view control for plan version id.
            //// Selecting campaign(s) of plan whose IsDelete=false.
            var campaign = db.Plan_Campaign.Where(pc => pc.PlanId.Equals(planid) && pc.IsDeleted.Equals(false)).ToList().Where(pc => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                               CalendarEndDate,
                                                                                                                               pc.StartDate, pc.EndDate).Equals(false));

            //// Selecting campaignIds.
            var campaignId = campaign.Select(pc => pc.PlanCampaignId).ToList<int>();

            //// Selecting program(s) of campaignIds whose IsDelete=false.
            var program = db.Plan_Campaign_Program.Where(pcp => campaignId.Contains(pcp.PlanCampaignId) && pcp.IsDeleted.Equals(false))
                                                  .Select(pcp => pcp)
                                                  .ToList()
                                                  .Where(pcp => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                            CalendarEndDate,
                                                                                                            pcp.StartDate,
                                                                                                            pcp.EndDate).Equals(false));

            //// Selecting programIds.
            var programId = program.Select(pcp => pcp.PlanProgramId);

            //// Applying filters to tactic (IsDelete, Geography, Individuals and Show My Tactic)
            List<Plan_Campaign_Program_Tactic> objPlan_Campaign_Program_Tactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted.Equals(false) &&
                                                                                                programId.Contains(pcpt.PlanProgramId)).ToList()
                                                                                 .Where(pcpt =>
                                                                                     //// Checking start and end date
                                                                                                    Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                                                CalendarEndDate,
                                                                                                                                                pcpt.StartDate,
                                                                                                                                                pcpt.EndDate).Equals(false)).ToList();


            Dictionary<int, string> activitymonth = new Dictionary<int, string>();

            int[] montharray = new int[12];


            if (objPlan_Campaign_Program_Tactic != null && objPlan_Campaign_Program_Tactic.Count() > 0)
            {
                IEnumerable<string> diff;
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

                foreach (Plan_Campaign_Program_Tactic a in objPlan_Campaign_Program_Tactic)
                {
                    var start = Convert.ToDateTime(a.StartDate);
                    var end = Convert.ToDateTime(a.EndDate);
                    if (year != 0)
                    {
                        if (start.Year == year)
                        {
                            if (start.Year == end.Year)
                            {
                                end = new DateTime(end.Year, end.Month, DateTime.DaysInMonth(end.Year, end.Month));
                            }
                            else
                            {
                                end = new DateTime(start.Year, 12, 31);
                            }

                            diff = Enumerable.Range(0, Int32.MaxValue)
                                                 .Select(e => start.AddMonths(e))
                                                 .TakeWhile(e => e <= end)
                                                 .Select(e => e.ToString("MM"));
                            foreach (string d in diff)
                            {
                                int monthno = Convert.ToInt32(d.TrimStart('0'));
                                if (monthno == 1)
                                {
                                    montharray[0] = montharray[0] + 1;
                                }
                                else
                                {
                                    montharray[monthno - 1] = montharray[monthno - 1] + 1;
                                }

                            }
                        }
                        else if (end.Year == year)
                        {
                            if (start.Year != year)
                            {
                                start = new DateTime(end.Year, 01, 01);
                                diff = Enumerable.Range(0, Int32.MaxValue)
                                                 .Select(e => start.AddMonths(e))
                                                 .TakeWhile(e => e <= end)
                                                 .Select(e => e.ToString("MM"));
                                foreach (string d in diff)
                                {
                                    int monthno = Convert.ToInt32(d.TrimStart('0'));
                                    if (monthno == 1)
                                    {
                                        montharray[0] = montharray[0] + 1;
                                    }
                                    else
                                    {
                                        montharray[monthno - 1] = montharray[monthno - 1] + 1;
                                    }

                                }
                            }
                        }
                    }

                    else if (strparam == Enums.UpcomingActivities.thismonth.ToString())
                    {
                        if (start.Month == DateTime.Now.Month || end.Month == DateTime.Now.Month)
                        {
                            if (start.Month == end.Month)
                            {
                                end = new DateTime(end.Year, end.Month, DateTime.DaysInMonth(end.Year, end.Month));
                            }
                            else
                            {
                                if (start.Month == 2)
                                {
                                    if (start.Year % 4 == 0)
                                    {
                                        end = new DateTime(start.Year, start.Month, 29);
                                    }
                                    else
                                    {
                                        end = new DateTime(start.Year, start.Month, 28);
                                    }
                                }
                                else if (end.Month == 2)
                                {
                                    start = new DateTime(end.Year, end.Month, 01);

                                }
                                else
                                {
                                    if (end.Month == DateTime.Now.Month && start.Month != DateTime.Now.Month)
                                    {
                                        start = new DateTime(end.Year, end.Month, 01);
                                    }
                                    else
                                    {
                                        end = new DateTime(start.Year, start.Month, new DateTime(start.Year, start.AddMonths(1).Month, 01).AddDays(-1).Day);    // Modified by Sohel Pathan on 22/09/2014 for PL ticket #782
                                    }
                                }

                            }

                            diff = Enumerable.Range(0, Int32.MaxValue)
                                                 .Select(e => start.AddMonths(e))
                                                 .TakeWhile(e => e <= end)
                                                 .Select(e => e.ToString("MM"));
                            foreach (string d in diff)
                            {
                                int monthno = Convert.ToInt32(d.TrimStart('0'));
                                if (monthno == 1)
                                {
                                    montharray[0] = montharray[0] + 1;
                                }
                                else
                                {
                                    montharray[monthno - 1] = montharray[monthno - 1] + 1;
                                }

                            }
                        }
                    }
                    else if (strparam == Enums.UpcomingActivities.thisquarter.ToString())
                    {

                        if (DateTime.Now.Month == 1 || DateTime.Now.Month == 2 || DateTime.Now.Month == 3)
                        {
                            if (start.Month == 1 || start.Month == 2 || start.Month == 3 || end.Month == 1 || end.Month == 2 || end.Month == 3)
                            {
                                montharray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q1), start, end, montharray);
                            }

                        }
                        else if (DateTime.Now.Month == 4 || DateTime.Now.Month == 5 || DateTime.Now.Month == 6)
                        {
                            if (start.Month == 4 || start.Month == 5 || start.Month == 6 || end.Month == 4 || end.Month == 5 || end.Month == 6)
                            {
                                montharray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q2), start, end, montharray);
                            }

                        }
                        else if (DateTime.Now.Month == 7 || DateTime.Now.Month == 8 || DateTime.Now.Month == 9)
                        {
                            if (start.Month == 7 || start.Month == 8 || start.Month == 9 || end.Month == 7 || end.Month == 8 || end.Month == 9)
                            {
                                montharray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q3), start, end, montharray);
                            }

                        }
                        else if (DateTime.Now.Month == 10 || DateTime.Now.Month == 11 || DateTime.Now.Month == 12)
                        {
                            if (start.Month == 10 || start.Month == 11 || start.Month == 12 || end.Month == 10 || end.Month == 11 || end.Month == 12)
                            {
                                montharray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q4), start, end, montharray);
                            }

                        }
                    }

                }


            }


            List<ActivityChart> act = new List<ActivityChart>();
            for (int i = 0; i < montharray.Count(); i++)
            {
                ActivityChart a = new ActivityChart();
                string strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i + 1);
                if (i == 0)
                {
                    a.Month = strMonthName[0].ToString();
                }
                else
                {

                    if (i % 3 == 0)
                    {
                        a.Month = strMonthName[0].ToString();
                    }
                    else
                    {
                        a.Month = "";
                    }
                    if (i == 11)
                    {
                        a.Month = strMonthName[0].ToString();
                    }
                }
                if (montharray[i] == 0)
                {
                    a.NoOfActivity = "";
                }
                else
                {
                    a.NoOfActivity = montharray[i].ToString();

                }
                a.Color = "#c633c9";
                act.Add(a);
            }

            return Json(new
            {
                lstchart = act.ToList(),


            }, JsonRequestBehavior.AllowGet);


        }
        #endregion

        #region GetNumberOfActivityPerMonthByMultiplePlanId
        /// <summary>
        /// Get Number of Activity Per Month for Activity Distribution Chart for MultiplePlanIds.
        /// </summary>
        /// <param name="strparam">Upcoming Activity dropdown selected option e.g. planyear, thisyear</param>
        /// <param name="strPlanIds">Comma separated string of Plan Ids</param>
        /// <returns>JsonResult.</returns>
        public JsonResult GetNumberOfActivityPerMonthByMultiplePlanId(string planid, string strparam)
        {
            planid = System.Web.HttpUtility.UrlDecode(planid);
            List<int> planIds = string.IsNullOrWhiteSpace(planid) ? new List<int>() : planid.Split(',').Select(p => int.Parse(p)).ToList();

            List<Plan> filteredPlans = db.Plans.Where(p => p.IsDeleted == false && planIds.Contains(p.PlanId)).ToList().Select(p => p).ToList();
            List<int> filteredPlanIds = new List<int>();

            string planYear = "";

            int Planyear;
            bool isNumeric = int.TryParse(strparam, out Planyear);

            if (isNumeric)
            {
                planYear = Convert.ToString(Planyear);
            }
            else
            {
                planYear = DateTime.Now.Year.ToString();
            }

            filteredPlanIds = filteredPlans.Where(p => p.Year == planYear).ToList().Select(p => p.PlanId).ToList();

            CalendarStartDate = DateTime.Now;
            CalendarEndDate = DateTime.Now;
            Common.GetPlanGanttStartEndDate(planYear, strparam, ref CalendarStartDate, ref CalendarEndDate);

            //Start Maninder Singh Wadhva : 11/15/2013 - Getting list of tactic for view control for plan version id.
            //// Selecting campaign(s) of plan whose IsDelete=false.
            var campaign = db.Plan_Campaign.Where(pc => filteredPlanIds.Contains(pc.PlanId) && pc.IsDeleted.Equals(false)).ToList().Where(pc => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                               CalendarEndDate,
                                                                                                                               pc.StartDate, pc.EndDate).Equals(false));

            //// Selecting campaignIds.
            var campaignId = campaign.Select(pc => pc.PlanCampaignId).ToList<int>();

            //// Selecting program(s) of campaignIds whose IsDelete=false.
            var program = db.Plan_Campaign_Program.Where(pcp => campaignId.Contains(pcp.PlanCampaignId) && pcp.IsDeleted.Equals(false))
                                                  .Select(pcp => pcp)
                                                  .ToList()
                                                  .Where(pcp => Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                            CalendarEndDate,
                                                                                                            pcp.StartDate,
                                                                                                            pcp.EndDate).Equals(false));

            //// Selecting programIds.
            var programId = program.Select(pcp => pcp.PlanProgramId);

            //// Applying filters to tactic (IsDelete, Geography, Individuals and Show My Tactic)
            List<Plan_Campaign_Program_Tactic> objPlan_Campaign_Program_Tactic = db.Plan_Campaign_Program_Tactic.Where(pcpt => pcpt.IsDeleted.Equals(false) &&
                                                                                                programId.Contains(pcpt.PlanProgramId)).ToList()
                                                                                 .Where(pcpt =>
                                                                                     //// Checking start and end date
                                                                                                    Common.CheckBothStartEndDateOutSideCalendar(CalendarStartDate,
                                                                                                                                                CalendarEndDate,
                                                                                                                                                pcpt.StartDate,
                                                                                                                                                pcpt.EndDate).Equals(false)).ToList();


            Dictionary<int, string> activitymonth = new Dictionary<int, string>();

            int[] montharray = new int[12];


            if (objPlan_Campaign_Program_Tactic != null && objPlan_Campaign_Program_Tactic.Count() > 0)
            {
                IEnumerable<string> diff;
                int year = 0;
                if (strparam != null)
                {
                    if (isNumeric)
                    {
                        year = Planyear;
                    }
                }
                int currentMonth = DateTime.Now.Month;
                foreach (Plan_Campaign_Program_Tactic a in objPlan_Campaign_Program_Tactic)
                {
                    var start = Convert.ToDateTime(a.StartDate);
                    var end = Convert.ToDateTime(a.EndDate);
                    if (year != 0)
                    {
                        if (start.Year == year)
                        {
                            if (start.Year == end.Year)
                            {
                                end = new DateTime(end.Year, end.Month, DateTime.DaysInMonth(end.Year, end.Month));
                            }
                            else
                            {
                                end = new DateTime(start.Year, 12, 31);
                            }

                            diff = Enumerable.Range(0, Int32.MaxValue)
                                                 .Select(e => start.AddMonths(e))
                                                 .TakeWhile(e => e <= end)
                                                 .Select(e => e.ToString("MM"));
                            foreach (string d in diff)
                            {
                                int monthno = Convert.ToInt32(d.TrimStart('0'));
                                if (monthno == 1)
                                {
                                    montharray[0] = montharray[0] + 1;
                                }
                                else
                                {
                                    montharray[monthno - 1] = montharray[monthno - 1] + 1;
                                }

                            }
                        }
                        else if (end.Year == year)
                        {
                            if (start.Year != year)
                            {
                                start = new DateTime(end.Year, 01, 01);
                                diff = Enumerable.Range(0, Int32.MaxValue)
                                                 .Select(e => start.AddMonths(e))
                                                 .TakeWhile(e => e <= end)
                                                 .Select(e => e.ToString("MM"));
                                foreach (string d in diff)
                                {
                                    int monthno = Convert.ToInt32(d.TrimStart('0'));
                                    if (monthno == 1)
                                    {
                                        montharray[0] = montharray[0] + 1;
                                    }
                                    else
                                    {
                                        montharray[monthno - 1] = montharray[monthno - 1] + 1;
                                    }

                                }
                            }
                        }
                    }

                    else if (strparam == Enums.UpcomingActivities.thismonth.ToString())
                    {
                        if (start.Month == DateTime.Now.Month || end.Month == DateTime.Now.Month)
                        {
                            if (start.Month == end.Month)
                            {
                                end = new DateTime(end.Year, end.Month, DateTime.DaysInMonth(end.Year, end.Month));
                            }
                            else
                            {
                                if (start.Month == 2)
                                {
                                    if (start.Year % 4 == 0)
                                    {
                                        end = new DateTime(start.Year, start.Month, 29);
                                    }
                                    else
                                    {
                                        end = new DateTime(start.Year, start.Month, 28);
                                    }
                                }
                                else if (end.Month == 2)
                                {
                                    start = new DateTime(end.Year, end.Month, 01);

                                }
                                else
                                {
                                    if (end.Month == DateTime.Now.Month && start.Month != DateTime.Now.Month)
                                    {
                                        start = new DateTime(end.Year, end.Month, 01);
                                    }
                                    else
                                    {
                                        end = new DateTime(start.Year, start.Month, new DateTime(start.Year, start.AddMonths(1).Month, 01).AddDays(-1).Day);
                                    }
                                }

                            }

                            diff = Enumerable.Range(0, Int32.MaxValue)
                                                 .Select(e => start.AddMonths(e))
                                                 .TakeWhile(e => e <= end)
                                                 .Select(e => e.ToString("MM"));
                            foreach (string d in diff)
                            {
                                int monthno = Convert.ToInt32(d.TrimStart('0'));
                                if (monthno == 1)
                                {
                                    montharray[0] = montharray[0] + 1;
                                }
                                else
                                {
                                    montharray[monthno - 1] = montharray[monthno - 1] + 1;
                                }

                            }
                        }
                    }
                    else if (strparam == Enums.UpcomingActivities.thisquarter.ToString())
                    {

                        if (currentMonth == 1 || currentMonth == 2 || currentMonth == 3)
                        {
                            if (start.Month == 1 || start.Month == 2 || start.Month == 3 || end.Month == 1 || end.Month == 2 || end.Month == 3)
                            {
                                montharray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q1), start, end, montharray);
                            }

                        }
                        else if (currentMonth == 4 || currentMonth == 5 || currentMonth == 6)
                        {
                            if (start.Month == 4 || start.Month == 5 || start.Month == 6 || end.Month == 4 || end.Month == 5 || end.Month == 6)
                            {
                                montharray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q2), start, end, montharray);
                            }

                        }
                        else if (currentMonth == 7 || currentMonth == 8 || currentMonth == 9)
                        {
                            if (start.Month == 7 || start.Month == 8 || start.Month == 9 || end.Month == 7 || end.Month == 8 || end.Month == 9)
                            {
                                montharray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q3), start, end, montharray);
                            }

                        }
                        else if (currentMonth == 10 || currentMonth == 11 || currentMonth == 12)
                        {
                            if (start.Month == 10 || start.Month == 11 || start.Month == 12 || end.Month == 10 || end.Month == 11 || end.Month == 12)
                            {
                                montharray = GetQuarterWiseGraph(Convert.ToString(Enums.Quarter.Q4), start, end, montharray);
                            }

                        }
                    }

                }


            }


            List<ActivityChart> act = new List<ActivityChart>();
            for (int i = 0; i < montharray.Count(); i++)
            {
                ActivityChart a = new ActivityChart();
                string strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i + 1);
                if (i == 0)
                {
                    a.Month = strMonthName[0].ToString();
                }
                else
                {

                    if (i % 3 == 0)
                    {
                        a.Month = strMonthName[0].ToString();
                    }
                    else
                    {
                        a.Month = "";
                    }
                    if (i == 11)
                    {
                        a.Month = strMonthName[0].ToString();
                    }
                }
                if (montharray[i] == 0)
                {
                    a.NoOfActivity = "";
                }
                else
                {
                    a.NoOfActivity = montharray[i].ToString();

                }
                a.Color = "#c633c9";
                act.Add(a);
            }

            return Json(new
            {
                lstchart = act.ToList(),


            }, JsonRequestBehavior.AllowGet);


        }
        #endregion

        #region "Get QuarterWisegraph"

        /// <summary>
        /// Function to generate activity graph quarter wise
        /// </summary>
        /// <param name="Quarter"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="montharray"></param>
        /// <returns>int[] array</returns>
        public int[] GetQuarterWiseGraph(string Quarter, DateTime start, DateTime end, int[] montharray)
        {
            IEnumerable<string> diff;
            DateTime enddate = end;

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
            if (start.Month == month1 || start.Month == month2 || start.Month == month3)
            {
                if (start.Month == end.Month)
                {
                    end = new DateTime(end.Year, end.Month, DateTime.DaysInMonth(end.Year, end.Month));
                }
                else if (start.Month == 2)
                {
                    if (start.Year % 4 == 0)
                    {
                        end = new DateTime(start.Year, start.Month, 29);
                    }
                    else
                    {
                        end = new DateTime(start.Year, start.Month, 28);
                    }
                }
                else
                {
                    end = new DateTime(start.Year, start.Month, new DateTime(start.Year, start.AddMonths(1).Month, 01).AddDays(-1).Day);    // Modified by Sohel Pathan on 22/09/2014 for PL ticket #782
                }

                diff = Enumerable.Range(0, Int32.MaxValue)
                                     .Select(e => start.AddMonths(e))
                                     .TakeWhile(e => e <= end)
                                     .Select(e => e.ToString("MM"));
                foreach (string d in diff)
                {
                    int monthno = Convert.ToInt32(d.TrimStart('0'));
                    if (monthno == 1)
                    {
                        montharray[0] = montharray[0] + 1;
                    }
                    else
                    {
                        montharray[monthno - 1] = montharray[monthno - 1] + 1;
                    }

                }

                end = enddate;
            }
            if (end.Month == month1 || end.Month == month2 || end.Month == month3)
            {
                if (start.Month != end.Month)
                {
                    start = new DateTime(end.Year, end.Month, 01);

                    diff = Enumerable.Range(0, Int32.MaxValue)
                                         .Select(e => start.AddMonths(e))
                                         .TakeWhile(e => e <= end)
                                         .Select(e => e.ToString("MM"));
                    foreach (string d in diff)
                    {
                        int monthno = Convert.ToInt32(d.TrimStart('0'));
                        if (monthno == 1)
                        {
                            montharray[0] = montharray[0] + 1;
                        }
                        else
                        {
                            montharray[monthno - 1] = montharray[monthno - 1] + 1;
                        }

                    }
                }

            }

            return montharray;
        }

        #endregion

        #region "Add Actual"

        public ActionResult AddActual()
        {
            HomePlanModel planmodel = new Models.HomePlanModel();

            try
            {
                // Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
                var lstUserCustomRestriction = Common.GetUserCustomRestriction();
                int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
                int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
                var lstAllowedGeography = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId).ToList();
                List<Guid> lstAllowedGeographyId = new List<Guid>();
                lstAllowedGeography.ForEach(g => lstAllowedGeographyId.Add(Guid.Parse(g)));
                planmodel.objGeography = db.Geographies.Where(g => g.IsDeleted.Equals(false) && g.ClientId == Sessions.User.ClientId && lstAllowedGeographyId.Contains(g.GeographyId)).Select(g => g).OrderBy(g => g.Title).ToList();
                List<string> tacticStatus = Common.GetStatusListAfterApproved();

                //added by uday for internal point on 15-7-2014
                string type = PlanGanttTypes.Tactic.ToString(); // this is inititalized as 0 bcoz to get the status for tactics.
                var individuals = GetIndividualsByPlanId(Sessions.PlanId.ToString(), type, Enums.ActiveMenu.Home.ToString());
                ////Start - Modified by Mitesh Vaishnav for PL ticket 972 - Add Actuals - Filter section formatting
                ////Fetch individual's records distinct
                planmodel.objIndividuals = individuals.Select(a => new
                {
                    UserId = a.UserId,
                    FirstName = a.FirstName,
                    LastName = a.LastName
                }).ToList().Distinct().Select(a => new User()
                {
                    UserId = a.UserId,
                    FirstName = a.FirstName,
                    LastName = a.LastName
                }).ToList().OrderBy(i => string.Format("{0} {1}", i.FirstName, i.LastName)).ToList();
                ////End - Modified by Mitesh Vaishnav for PL ticket 972 - Add Actuals - Filter section formatting

                //end by uday

                List<TacticType> objTacticType = new List<TacticType>();

                //// Modified By: Maninder Singh for TFS Bug#282: Extra Tactics Displaying in Add Actual Screen
                objTacticType = (from t in db.Plan_Campaign_Program_Tactic
                                 where t.Plan_Campaign_Program.Plan_Campaign.PlanId == Sessions.PlanId
                                 && tacticStatus.Contains(t.Status) && t.IsDeleted == false
                                 select t.TacticType).Distinct().OrderBy(t => t.Title).ToList();

                ViewBag.TacticTypeList = objTacticType;

                // Added by Dharmraj Mangukiya to implement custom restrictions PL ticket #537
                // Get current user permission for edit own and subordinates plans.
                var lstOwnAndSubOrdinates = new List<Guid>();
                try
                {
                    lstOwnAndSubOrdinates = Common.GetAllSubordinates(Sessions.User.UserId);
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
                        return RedirectToAction("ServiceUnavailable", "Login");
                    }
                }

                bool IsPlanEditable = false;
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                var objPlan = db.Plans.FirstOrDefault(p => p.PlanId == Sessions.PlanId);
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
                ViewBag.IsNewPlanEnable = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
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
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
            }

            return View(planmodel);
        }

        /// <summary>
        /// Added By: Bhavesh Dobariya.
        /// Action to Get Actual Value of Tactic.
        /// </summary>
        /// <returns>Returns Json Result.</returns>
        public JsonResult GetActualTactic(int status)
        {
            List<Plan_Campaign_Program_Tactic> TacticList = new List<Plan_Campaign_Program_Tactic>();
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            if (status == 0)
            {
                //// Modified By: Maninder Singh for TFS Bug#282: Extra Tactics Displaying in Add Actual Screen
                TacticList = db.Plan_Campaign_Program_Tactic.Where(planTactic => planTactic.Plan_Campaign_Program.Plan_Campaign.PlanId.Equals(Sessions.PlanId) &&
                                                                       tacticStatus.Contains(planTactic.Status) &&
                                                                       planTactic.IsDeleted.Equals(false) && planTactic.CostActual == null &&
                                                                       !planTactic.Plan_Campaign_Program_Tactic_Actual.Any())
                                                            .ToList();
            }
            else
            {
                TacticList = db.Plan_Campaign_Program_Tactic.Where(t => t.Plan_Campaign_Program.Plan_Campaign.PlanId == Sessions.PlanId && tacticStatus.Contains(t.Status) && t.IsDeleted == false).ToList();

            }

            // Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
            var lstUserCustomRestriction = new List<UserCustomRestrictionModel>();
            try
            {
                lstUserCustomRestriction = Common.GetUserCustomRestriction();
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

            int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            var lstAllowedVertical = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId).ToList();
            var lstAllowedGeography = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId.ToString().ToLower()).ToList();////Modified by Mitesh Vaishnav For functional review point 89
            var lstAllowedBusinessUnit = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).Select(r => r.CustomFieldId).ToList();

            TacticList = TacticList.Where(t => lstAllowedVertical.Contains(t.VerticalId.ToString()) && lstAllowedGeography.Contains(t.GeographyId.ToString().ToLower())).ToList();////Modified by Mitesh Vaishnav For functional review point 89

            List<int> TacticIds = TacticList.Select(t => t.PlanTacticId).ToList();
            var dtTactic = (from pt in db.Plan_Campaign_Program_Tactic_Actual
                            where TacticIds.Contains(pt.PlanTacticId)
                            select new { pt.PlanTacticId, pt.CreatedBy, pt.CreatedDate }).GroupBy(pt => pt.PlanTacticId).Select(pt => pt.FirstOrDefault());
            List<Guid> userListId = new List<Guid>();
            userListId = (from ta in dtTactic select ta.CreatedBy).ToList<Guid>();

            // Added BY Bhavesh
            // Calculate MQL at runtime #376
            List<Plan_Tactic_Values> MQLTacticList = Common.GetMQLValueTacticList(TacticList);
            List<ProjectedRevenueClass> tacticList = Common.ProjectedRevenueCalculateList(TacticList);
            List<ProjectedRevenueClass> tacticListCW = Common.ProjectedRevenueCalculateList(TacticList, true);
            var listModified = TacticList.Where(t => t.ModifiedDate != null).Select(t => t).ToList();
            foreach (var t in listModified)
            {
                userListId.Add(new Guid(t.ModifiedBy.ToString()));
            }
            string userList = string.Join(",", userListId.Select(s => s.ToString()).ToArray());

            List<User> userName = new List<User>();

            try
            {
                objBDSUserRepository.GetMultipleTeamMemberDetails(userList, Sessions.ApplicationId);


                string TitleProjectedStageValue = Enums.InspectStageValues[Enums.InspectStage.ProjectedStageValue.ToString()].ToString();
                string TitleCW = Enums.InspectStageValues[Enums.InspectStage.CW.ToString()].ToString();
                string TitleMQL = Enums.InspectStageValues[Enums.InspectStage.MQL.ToString()].ToString();
                string TitleRevenue = Enums.InspectStageValues[Enums.InspectStage.Revenue.ToString()].ToString();
                List<Plan_Campaign_Program_Tactic_Actual> lstTacticActual = db.Plan_Campaign_Program_Tactic_Actual.Where(a => TacticIds.Contains(a.PlanTacticId)).ToList();
                // Added by Dharmraj Mangukiya for filtering tactic as per custom restrictions PL ticket #538
                var lstEditableVertical = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId).ToList();
                var lstEditableGeography = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId.ToString().ToLower()).ToList();////Modified by Mitesh Vaishnav For functional review point 89
                var lstEditableBusinessUnit = lstUserCustomRestriction.Where(r => r.Permission == ViewEditPermission && r.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).Select(r => r.CustomFieldId.ToString()).ToList();

                List<string> lstMonthly = new List<string>() { "Y1", "Y2", "Y3", "Y4", "Y5", "Y6", "Y7", "Y8", "Y9", "Y10", "Y11", "Y12" };

                var tacticLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => TacticIds.Contains(a.PlanTacticId) && a.IsDeleted == false).ToList();
                List<int> LineItemsIds = tacticLineItem.Select(t => t.PlanLineItemId).ToList();
                var tacticLineItemActual = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(s => LineItemsIds.Contains(s.PlanLineItemId)).ToList();
                var tacticActuals = db.Plan_Campaign_Program_Tactic_Actual.Where(a => TacticIds.Contains(a.PlanTacticId)).ToList();

                var tacticObj = TacticList.Select(t => new
                {
                    id = t.PlanTacticId,
                    title = t.Title,
                    //inqProjected = t.INQs,
                    //inqActual = t.INQsActual == null ? 0 : t.INQsActual,
                    mqlProjected = MQLTacticList.Where(tm => tm.PlanTacticId == t.PlanTacticId).Select(tm => tm.MQL),
                    mqlActual = lstTacticActual.Where(a => a.PlanTacticId == t.PlanTacticId && a.StageTitle == TitleMQL).Sum(a => a.Actualvalue),//t.MQLsActual == null ? 0 : t.MQLsActual,
                    cwProjected = Math.Round(tacticListCW.Where(tl => tl.PlanTacticId == t.PlanTacticId).Select(tl => tl.ProjectedRevenue).SingleOrDefault(), 1),
                    cwActual = lstTacticActual.Where(a => a.PlanTacticId == t.PlanTacticId && a.StageTitle == TitleCW).Sum(a => a.Actualvalue),//t.CWsActual == null ? 0 : t.CWsActual,
                    revenueProjected = Math.Round(tacticList.Where(tl => tl.PlanTacticId == t.PlanTacticId).Select(tl => tl.ProjectedRevenue).SingleOrDefault(), 1),
                    revenueActual = lstTacticActual.Where(a => a.PlanTacticId == t.PlanTacticId && a.StageTitle == TitleRevenue).Sum(a => a.Actualvalue),//t.RevenuesActual == null ? 0 : t.RevenuesActual,
                    //costProjected = t.Cost,
                    costProjected = (tacticLineItem.Where(s => s.PlanTacticId == t.PlanTacticId)).Count() > 0 ?
                    (tacticLineItem.Where(s => s.PlanTacticId == t.PlanTacticId)).Sum(a => a.Cost) : t.Cost,
                    //costActual = t.CostActual == null ? 0 : t.CostActual,

                    //Get the sum of Tactic line item actuals
                    costActual = (tacticLineItem.Where(s => s.PlanTacticId == t.PlanTacticId)).Count() > 0 ?
                    (tacticLineItem.Where(s => s.PlanTacticId == t.PlanTacticId)).Select(pp => new
                    {
                        LineItemActualCost = tacticLineItemActual.Where(ww => ww.PlanLineItemId == pp.PlanLineItemId).Sum(q => q.Value)
                    }).Sum(a => a.LineItemActualCost) : (tacticActuals.Where(s => s.PlanTacticId == t.PlanTacticId && s.StageTitle == Enums.InspectStageValues[Enums.InspectStage.Cost.ToString()].ToString())).Sum(a => a.Actualvalue),

                    //First check that if tactic has a single line item at that time we need to get the cost actual data from the respective table. 
                    costActualData = lstMonthly.Select(m => new
                    {
                        period = m,
                        Cost = (tacticLineItem.Where(s => s.PlanTacticId == t.PlanTacticId)).Count() > 0 ?
                        tacticLineItem.Where(s => s.PlanTacticId == t.PlanTacticId).Select(ss => new
                        {
                            costValue = tacticLineItemActual.Where(y => y.PlanLineItemId == ss.PlanLineItemId && y.Period == m).Sum(s => s.Value)
                        }).Sum(s => s.costValue) :
                        tacticActuals.Where(s => s.PlanTacticId == t.PlanTacticId && s.Period == m && s.StageTitle == Enums.InspectStageValues[Enums.InspectStage.Cost.ToString()].ToString()).Sum(s => s.Actualvalue),
                    }),

                    roiProjected = 0,//t.ROI == null ? 0 : t.ROI,
                    roiActual = 0,//t.ROIActual == null ? 0 : t.ROIActual,
                    geographyId = t.GeographyId,
                    individualId = t.CreatedBy,
                    tacticTypeId = t.TacticTypeId,
                    modifiedBy = Common.TacticModificationMessage(t.PlanTacticId, userName),////Modified by Mitesh Vaishnav for PL ticket #743,When userId will be empty guid ,First name and last name combination will be display as Gameplan Integration Service
                    actualData = (tacticActuals.Where(pct => pct.PlanTacticId.Equals(t.PlanTacticId)).Select(pcp => pcp).ToList()).Select(pcpt => new
                    {
                        title = pcpt.StageTitle,
                        period = pcpt.Period,
                        actualValue = pcpt.Actualvalue,
                        IsUpdate = status
                    }).Select(pcp => pcp).Distinct(),
                    LastSync = t.LastSyncDate == null ? string.Empty : ("Last synced with integration " + Common.GetFormatedDate(t.LastSyncDate) + "."),////Modified by Mitesh vaishnav on 12/08/2014 for PL ticket #690
                    stageTitle = Common.GetTacticStageTitle(t.PlanTacticId),
                    tacticStageId = t.Stage.StageId,
                    tacticStageTitle = t.Stage.Title,
                    projectedStageValue = t.ProjectedStageValue,
                    projectedStageValueActual = lstTacticActual.Where(a => a.PlanTacticId == t.PlanTacticId && a.StageTitle == TitleProjectedStageValue).Sum(a => a.Actualvalue),
                    IsTacticEditable = (lstEditableGeography.Contains(t.GeographyId.ToString().ToLower()) && lstEditableVertical.Contains(t.VerticalId.ToString()) && lstEditableBusinessUnit.Contains(t.BusinessUnitId.ToString())),////Modified by Mitesh Vaishnav For functional review point 89
                    //Set Line Item data with it's actual values and Sum
                    LineItemsData = (tacticLineItem.Where(pctq => pctq.PlanTacticId.Equals(t.PlanTacticId)).ToList()).Select(pcpt => new
                    {
                        id = pcpt.PlanLineItemId,
                        Title = pcpt.Title,
                        LineItemCost = pcpt.Cost,
                        //Get the sum of actual
                        LineItemActualCost = (tacticLineItemActual.Where(lta => lta.PlanLineItemId == pcpt.PlanLineItemId)).ToList().Sum(q => q.Value),
                        LineItemActual = lstMonthly.Select(m => new
                        {
                            period = m,
                            Cost = tacticLineItemActual.Where(lta => lta.PlanLineItemId == pcpt.PlanLineItemId && lta.Period == m).Select(ltai => ltai.Value)
                        }),
                    })
                });

                var lstTactic = tacticObj.Select(t => new
                {
                    id = t.id,
                    title = t.title,
                    mqlProjected = t.mqlProjected,
                    mqlActual = t.mqlActual,
                    cwProjected = t.cwProjected,
                    cwActual = t.cwActual,
                    costActualData = t.costActualData,
                    revenueProjected = t.revenueProjected,
                    revenueActual = t.revenueActual,
                    costProjected = t.costProjected,
                    costActual = t.costActual,
                    roiProjected = t.costProjected == 0 ? 0 : ((t.revenueProjected - t.costProjected) / t.costProjected), // Modified by dharmraj for implement new formula to calculate ROI, #533
                    roiActual = t.costActual == 0 ? 0 : ((t.revenueActual - t.costActual) / t.costActual), // Modified by dharmraj for implement new formula to calculate ROI, #533
                    geographyId = t.geographyId,
                    individualId = t.individualId,
                    tacticTypeId = t.tacticTypeId,
                    modifiedBy = t.modifiedBy,
                    actualData = t.actualData,
                    LastSync = t.LastSync,
                    stageTitle = t.stageTitle,
                    tacticStageId = t.tacticStageId,
                    tacticStageTitle = t.tacticStageTitle,
                    projectedStageValue = t.projectedStageValue,
                    projectedStageValueActual = t.projectedStageValueActual,
                    IsTacticEditable = t.IsTacticEditable,
                    LineItemsData = t.LineItemsData
                });

                var opens = lstTactic.Where(x => x.actualData.ToList().Count == 0).OrderBy(t => t.title);
                var all = lstTactic.Where(x => x.actualData.ToList().Count != 0);

                var result = opens.Concat(all);

                return Json(result, JsonRequestBehavior.AllowGet);
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

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ChangeLog

        /// <summary>
        /// Load overview
        /// </summary>
        /// <param name="title"></param>
        /// <param name="BusinessUnitId"></param>
        /// <returns></returns>
        public ActionResult LoadChangeLog(int objectId)
        {
            List<ChangeLog_ViewModel> lst_changelog = new List<ChangeLog_ViewModel>();
            try
            {
                lst_changelog = Common.GetChangeLogs(Enums.ChangeLog_TableName.Plan.ToString(), objectId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                }
            }
            return PartialView("_ChangeLog", lst_changelog);
        }

        #endregion

        #region Get Owners by planID Method
        /// <summary>
        /// Added By :- Sohel Pathan
        /// Date :- 14/04/2014
        /// Reason :- To get list of users who have created tactic by planId (PL ticket # 428)
        /// </summary>
        /// <param name="PlanId"></param>
        /// <returns></returns>
        public JsonResult GetOwnerListForFilter(string PlanId, string ViewBy, string ActiveMenu)
        {
            try
            {
                var owners = GetIndividualsByPlanId(PlanId, ViewBy, ActiveMenu);
                var allowedOwner = owners.Select(a => new
                {
                    OwnerId = a.UserId,
                    Title = a.FirstName + " " + a.LastName,
                }).Distinct().ToList().OrderBy(i => i.Title).ToList();
                allowedOwner = allowedOwner.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();

                return Json(new { isSuccess = true, AllowedOwner = allowedOwner }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { returnURL = '#' }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
        }

        private List<User> GetIndividualsByPlanId(string PlanId, string ViewBy, string ActiveMenu)
        {
            List<int> PlanIds = string.IsNullOrWhiteSpace(PlanId) ? new List<int>() : PlanId.Split(',').Select(plan => int.Parse(plan)).ToList();

            BDSService.BDSServiceClient bdsUserRepository = new BDSService.BDSServiceClient();
            //// Added by :- Sohel Pathan on 17/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
            Enums.ActiveMenu objactivemenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, ActiveMenu.ToLower());
            List<string> status = Common.GetStatusListAfterApproved(); //GetStatusAsPerSelectedType(ViewBy, objactivemenu);
            status.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            ////

            //// Custom Restrictions
            var lstUserCustomRestriction = Common.GetUserCustomRestriction();
            int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
            int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;
            var lstAllowedVertical = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId).ToList();
            var lstAllowedGeography = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId.ToString().ToLower()).ToList();////Modified by Mitesh Vaishnav For functional review point 89

            var campaignList = db.Plan_Campaign.Where(c => c.IsDeleted.Equals(false) && PlanIds.Contains(c.PlanId)).Select(c => c.PlanCampaignId).ToList();
            var programList = db.Plan_Campaign_Program.Where(p => p.IsDeleted.Equals(false) && campaignList.Contains(p.PlanCampaignId)).Select(p => p.PlanProgramId).ToList();
            var tacticList = db.Plan_Campaign_Program_Tactic.Where(t => t.IsDeleted.Equals(false) && programList.Contains(t.PlanProgramId)).Select(t => t);

            //// Added by :- Sohel Pathan on 21/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
            if (ActiveMenu.Equals(Enums.ActiveMenu.Plan.ToString()))
            {
                List<string> statusCD = new List<string>();
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                statusCD.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

                var TacticUserList = tacticList.Distinct().ToList();
                TacticUserList = TacticUserList.Where(t => status.Contains(t.Status) || ((t.CreatedBy == Sessions.User.UserId && !ViewBy.Equals(GanttTabs.Request.ToString())) ? statusCD.Contains(t.Status) : false)).Distinct().ToList();

                //// Custom Restrictions applied
                TacticUserList = TacticUserList.Where(t => lstAllowedVertical.Contains(t.VerticalId.ToString()) && lstAllowedGeography.Contains(t.GeographyId.ToString().ToLower())).ToList();

                string strContatedIndividualList = string.Join(",", TacticUserList.Select(tactic => tactic.CreatedBy.ToString()));
                var individuals = bdsUserRepository.GetMultipleTeamMemberName(strContatedIndividualList);

                return individuals;
            }
            else
            {
                //// Modified by :- Sohel Pathan on 17/04/2014 for PL ticket #428 to disply users in individual filter according to selected plan and status of tactis 
                var TacticUserList = tacticList.Where(pcpt => status.Contains(pcpt.Status)).Select(a => a).Distinct().ToList();
                ////

                //// Custom Restrictions applied
                TacticUserList = TacticUserList.Where(t => lstAllowedVertical.Contains(t.VerticalId.ToString()) && lstAllowedGeography.Contains(t.GeographyId.ToString().ToLower())).ToList();

                string strContatedIndividualList = string.Join(",", TacticUserList.Select(tactic => tactic.CreatedBy.ToString()));
                var individuals = bdsUserRepository.GetMultipleTeamMemberName(strContatedIndividualList);

                return individuals;
            }
        }

        #endregion

        //[HttpPost]
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

        [HttpPost]
        public ActionResult GetPlansByBusinessId(string businessIds, string requstedModule)
        {
            var activePlan = (dynamic)null;

            try
            {
                if (!string.IsNullOrEmpty(businessIds))
                {

                    List<string> selectedBusinessId = businessIds.Split(',').ToList();
                    List<Guid> BusinessIds = new List<Guid>();

                    foreach (var item in selectedBusinessId)
                    {
                        BusinessIds.Add(Guid.Parse(item));
                    }

                    List<Model> models = db.Models.Where(m => BusinessIds.Contains(m.BusinessUnitId) && m.IsDeleted == false).ToList();

                    var modelIds = models.Select(m => m.ModelId).ToList();

                    if (requstedModule == Enums.ActiveMenu.Home.ToString().ToLower())
                    {
                        string Status = Convert.ToString(Enums.PlanStatus.Published);
                        activePlan = db.Plans.Where(p => modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true) && p.IsDeleted == false && p.Status == Status).Select(t =>
                            new
                            {
                                t.PlanId,
                                t.Title
                            }).ToList().Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
                    }
                    else
                    {
                        activePlan = db.Plans.Where(p => modelIds.Contains(p.Model.ModelId) && p.IsActive.Equals(true) && p.IsDeleted == false).Select(t =>
                        new
                        {
                            t.PlanId,
                            t.Title
                        }).ToList().Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();
                    }
                }
                else
                {
                    return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Json(new { isSuccess = true, ActivePlans = activePlan }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCustomAttributes()
        {
            try
            {
                var lstUserCustomRestriction = Common.GetUserCustomRestriction();
                int ViewOnlyPermission = (int)Enums.CustomRestrictionPermission.ViewOnly;
                int ViewEditPermission = (int)Enums.CustomRestrictionPermission.ViewEdit;

                var lstAllowedGeography = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Geography.ToString()).Select(r => r.CustomFieldId).ToList();
                List<Guid> lstAllowedGeographyId = new List<Guid>();
                lstAllowedGeography.ForEach(g => lstAllowedGeographyId.Add(Guid.Parse(g)));
                var allowedGeography = db.Geographies.Where(g => g.IsDeleted.Equals(false) && lstAllowedGeographyId.Contains(g.GeographyId)).Distinct().Select(t => new
                {
                    t.GeographyId,
                    t.Title
                }).OrderBy(t => t.Title).ToList();
                allowedGeography = allowedGeography.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();

                var lstAllowedVerticals = lstUserCustomRestriction.Where(r => (r.Permission == ViewOnlyPermission || r.Permission == ViewEditPermission) && r.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(r => r.CustomFieldId).ToList();
                List<int> lstAllowedVerticalsId = new List<int>();
                lstAllowedVerticals.ForEach(g => lstAllowedVerticalsId.Add(int.Parse(g)));
                var allowedVerticals = db.Verticals.Where(g => g.IsDeleted.Equals(false) && lstAllowedVerticalsId.Contains(g.VerticalId)).Distinct().Select(t => new
                {
                    t.VerticalId,
                    t.Title
                }).OrderBy(t => t.Title).ToList();
                allowedVerticals = allowedVerticals.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();

                var allowedAudience = db.Audiences.Where(g => g.IsDeleted.Equals(false) && g.ClientId == Sessions.User.ClientId).Distinct().Select(t => new
                {
                    t.AudienceId,
                    t.Title
                }).OrderBy(t => t.Title).ToList();
                allowedAudience = allowedAudience.Where(s => !string.IsNullOrEmpty(s.Title)).OrderBy(s => s.Title, new AlphaNumericComparer()).ToList();


                return Json(new { isSuccess = true, AllowedGeography = allowedGeography, AllowedVerticals = allowedVerticals, AllowedAudience = allowedAudience }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
        }

        //New parameter activeMenu added to check whether this method is called from Home or Plan
        /// <summary>
        /// Fetch the up comming activites value and select it again true. 
        /// </summary>
        /// <param name="planids">Plan id's with comma sepreated</param>
        /// <param name="CurrentTime">Current Time</param>
        /// <returns></returns>
        public JsonResult BindUpcomingActivitesValues(string planids, string CurrentTime)
        {
            //Fetch the list of Upcoming Activity
            List<SelectListItem> objUpcomingActivity = UpComingActivity(planids);

            bool IsItemExists = objUpcomingActivity.Where(s => s.Value == CurrentTime).Any();

            if (IsItemExists)
            {
                foreach (SelectListItem item in objUpcomingActivity)
                {
                    item.Selected = false;
                    //Set it Selected ture if we found current time value in the list.
                    if (CurrentTime == item.Value)
                    {
                        item.Selected = true;
                    }
                }
            }
            objUpcomingActivity = objUpcomingActivity.Where(s => !string.IsNullOrEmpty(s.Text)).OrderBy(s => s.Text, new AlphaNumericComparer()).ToList();
            return Json(objUpcomingActivity.ToList(), JsonRequestBehavior.AllowGet);
        }

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

        /// <summary>
        /// Process and fetch the Upcoming Activity  
        /// </summary>
        /// <param name="PlanIds">Plan id's with comma sepreated string</param>
        /// <returns>List fo SelectListItem of Upcoming activity</returns>
        public List<SelectListItem> UpComingActivity(string PlanIds)
        {
            List<int> planIds = string.IsNullOrWhiteSpace(PlanIds) ? new List<int>() : PlanIds.Split(',').Select(p => int.Parse(p)).ToList();

            //Fetch the active plan based of plan ids
            List<Plan> activePlan = db.Plans.Where(p => planIds.Contains(p.PlanId) && p.IsActive.Equals(true) && p.IsDeleted == false).ToList();

            //Get the Current year and Pre define Upcoming Activites.
            string currentYear = DateTime.Now.Year.ToString();
            List<SelectListItem> UpcomingActivityList = Common.GetUpcomingActivity().Select(p => new SelectListItem() { Text = p.Text, Value = p.Value.ToString(), Selected = p.Selected }).ToList();
            UpcomingActivityList.RemoveAll(s => s.Value == Enums.UpcomingActivities.thisyear.ToString() || s.Value == Enums.UpcomingActivities.planYear.ToString() ||
                s.Value == Enums.UpcomingActivities.nextyear.ToString());

            //If active plan dosen't have any current plan at that time we have to remove this month and thisquater option
            if (activePlan.Count > 0)
            {
                if (activePlan.Where(s => s.Year == currentYear).Count() == 0)
                {
                    UpcomingActivityList.RemoveAll(s => s.Value == Enums.UpcomingActivities.thismonth.ToString() || s.Value == Enums.UpcomingActivities.thisquarter.ToString());
                }
                else
                {
                    //Add current year into the list
                    UpcomingActivityList.Add(new SelectListItem { Text = DateTime.Now.Year.ToString(), Value = DateTime.Now.Year.ToString(), Selected = true });
                }
            }

            //Fetch the pervious year and future year list and insert into the list object
            var yearlistPrevious = activePlan.Where(p => p.Year != DateTime.Now.Year.ToString() && p.Year != DateTime.Now.AddYears(-1).Year.ToString() && Convert.ToInt32(p.Year) < DateTime.Now.AddYears(-1).Year).Select(p => p.Year).Distinct().OrderBy(p => p).ToList();
            yearlistPrevious.ForEach(p => UpcomingActivityList.Add(new SelectListItem { Text = p, Value = p, Selected = false }));
            var yearlistAfter = activePlan.Where(p => p.Year != DateTime.Now.Year.ToString() && p.Year != DateTime.Now.AddYears(-1).Year.ToString() && Convert.ToInt32(p.Year) > DateTime.Now.Year).Select(p => p.Year).Distinct().OrderBy(p => p).ToList();
            yearlistAfter.ForEach(p => UpcomingActivityList.Add(new SelectListItem { Text = p, Value = p, Selected = false }));
            return UpcomingActivityList;
        }

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
        private bool InspectPopupSharedLinkValidationForCustomRestriction(int planCampaignId, int planProgramId, int planTacticId, bool isImprovement, List<UserCustomRestrictionModel> lstUserCustomRestriction, List<Guid> lstAllowedGeographyIds, int ViewOnlyPermission, int ViewEditPermission)
        {
            bool isValidEntity = false;

            var lstAllowedBusinessUnits = lstUserCustomRestriction.Where(customRestrictions => (customRestrictions.Permission == ViewOnlyPermission || customRestrictions.Permission == ViewEditPermission) && customRestrictions.CustomField == Enums.CustomRestrictionType.BusinessUnit.ToString()).Select(customRestrictions => customRestrictions.CustomFieldId).ToList();
            List<Guid> lstAllowedBusinessUnitIds = new List<Guid>();
            lstAllowedBusinessUnits.ForEach(businessUnit => lstAllowedBusinessUnitIds.Add(Guid.Parse(businessUnit)));

            if (planTacticId > 0 && isImprovement.Equals(false))
            {
                var lstAllowedVerticals = lstUserCustomRestriction.Where(customRestrictions => (customRestrictions.Permission == ViewOnlyPermission || customRestrictions.Permission == ViewEditPermission) && customRestrictions.CustomField == Enums.CustomRestrictionType.Verticals.ToString()).Select(customRestrictions => customRestrictions.CustomFieldId).ToList();
                List<int> lstAllowedVerticalIds = new List<int>();
                lstAllowedVerticals.ForEach(vertical => lstAllowedVerticalIds.Add(int.Parse(vertical)));

                var objTactic = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.PlanTacticId == planTacticId && tactic.IsDeleted == false
                                                                    && lstAllowedBusinessUnitIds.Contains(tactic.BusinessUnitId)
                                                                    && lstAllowedGeographyIds.Contains(tactic.GeographyId)
                                                                    && lstAllowedVerticalIds.Contains(tactic.VerticalId)).Select(tactic => tactic.PlanTacticId);

                if (objTactic.Count() != 0)
                {
                    isValidEntity = true;
                }
            }
            else if (planProgramId > 0)
            {
                var objProgram = db.Plan_Campaign_Program.Where(program => program.PlanProgramId == planProgramId && program.IsDeleted == false
                                                                && lstAllowedBusinessUnitIds.Contains(program.Plan_Campaign.Plan.Model.BusinessUnitId)
                                                                ).Select(program => program);

                if (objProgram.Count() != 0)
                {
                    isValidEntity = true;
                }
            }
            else if (planCampaignId > 0)
            {
                var objCampaign = db.Plan_Campaign.Where(campaign => campaign.PlanCampaignId == planCampaignId && campaign.IsDeleted == false
                                                                && lstAllowedBusinessUnitIds.Contains(campaign.Plan.Model.BusinessUnitId)
                                                                ).Select(campaign => campaign.PlanCampaignId);

                if (objCampaign.Count() != 0)
                {
                    isValidEntity = true;
                }
            }
            else if (planTacticId > 0 && isImprovement.Equals(true))
            {
                var objImprovementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(improvementTactic => improvementTactic.ImprovementPlanTacticId == planTacticId && improvementTactic.IsDeleted == false
                                                                                        && lstAllowedBusinessUnitIds.Contains(improvementTactic.BusinessUnitId))
                                                                                        .Select(improvementTactic => improvementTactic.ImprovementPlanTacticId);

                if (objImprovementTactic.Count() != 0)
                {
                    isValidEntity = true;
                }
            }

            return isValidEntity;
        }

        #endregion
    }
}